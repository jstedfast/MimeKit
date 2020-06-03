//
// OpenPgpContext.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 Xamarin Inc. (www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto.Parameters;

using MimeKit.IO;
using MimeKit.Utils;

namespace MimeKit.Cryptography
{
	// NOTE: This class should really be called "GnuPGContext", since it's based upon the GnuPG way to handle keys.
	// However, renaming it now is impossible, since that would break every single class currently inheriting form it :/
	/// <summary>
	/// An abstract OpenPGP cryptography context which can be used for PGP/MIME that is based upon GnuPG
	/// files to store PGP keys.
	/// </summary>
	/// <remarks>
	/// Generally speaking, applications should not use a <see cref="OpenPgpContext"/>
	/// directly, but rather via higher level APIs such as <see cref="MultipartSigned"/>
	/// and <see cref="MultipartEncrypted"/>.
	/// </remarks>
	public abstract class OpenPgpContext : PgpContext
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="OpenPgpContext"/> class.
		/// </summary>
		/// <remarks>
		/// Subclasses choosing to use this constructor MUST set the <see cref="PublicKeyRingPath"/>,
		/// <see cref="SecretKeyRingPath"/>, <see cref="PublicKeyRingBundle"/>, and the
		/// <see cref="SecretKeyRingBundle"/> properties themselves.
		/// </remarks>
		protected OpenPgpContext () : base() // Base constructor sets all defaults.
		{ }

		/// <summary>
		/// Initialize a new instance of the <see cref="OpenPgpContext"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="OpenPgpContext"/> using the specified public and private keyring paths.
		/// </remarks>
		/// <param name="pubring">The public keyring file path.</param>
		/// <param name="secring">The secret keyring file path.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="pubring"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="secring"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An error occurred while reading one of the keyring files.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Bcpg.OpenPgp.PgpException">
		/// An error occurred while parsing one of the keyring files.
		/// </exception>
		protected OpenPgpContext (string pubring, string secring) : this ()
		{
			if (pubring == null)
				throw new ArgumentNullException (nameof (pubring));

			if (secring == null)
				throw new ArgumentNullException (nameof (secring));

			PublicKeyRingPath = pubring;
			SecretKeyRingPath = secring;

			if (File.Exists (pubring)) {
				using (var file = File.OpenRead (pubring)) {
					PublicKeyRingBundle = new PgpPublicKeyRingBundle (file);
				}
			} else {
				PublicKeyRingBundle = new PgpPublicKeyRingBundle (new byte[0]);
			}

			if (File.Exists (secring)) {
				using (var file = File.OpenRead (secring)) {
					SecretKeyRingBundle = new PgpSecretKeyRingBundle (file);
				}
			} else {
				SecretKeyRingBundle = new PgpSecretKeyRingBundle (new byte[0]);
			}
		}

		/// <summary>
		/// Get the public keyring path.
		/// </summary>
		/// <remarks>
		/// Gets the public keyring path.
		/// </remarks>
		/// <value>The public key ring path.</value>
		protected string PublicKeyRingPath {
			get; set;
		}

		/// <summary>
		/// Get the secret keyring path.
		/// </summary>
		/// <remarks>
		/// Gets the secret keyring path.
		/// </remarks>
		/// <value>The secret key ring path.</value>
		protected string SecretKeyRingPath {
			get; set;
		}

		/// <summary>
		/// Get the public keyring bundle.
		/// </summary>
		/// <remarks>
		/// Gets the public keyring bundle.
		/// </remarks>
		/// <value>The public keyring bundle.</value>
		public PgpPublicKeyRingBundle PublicKeyRingBundle {
			get; protected set;
		}

		/// <summary>
		/// Get the secret keyring bundle.
		/// </summary>
		/// <remarks>
		/// Gets the secret keyring bundle.
		/// </remarks>
		/// <value>The secret keyring bundle.</value>
		public PgpSecretKeyRingBundle SecretKeyRingBundle {
			get; protected set;
		}

		/// <summary>
		/// Helper method to retrieve a public key, and its keyring, given a key's ID
		/// </summary>
		/// <param name="keyId"></param>
		/// <param name="doAsync"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public override async Task<KeyRetrievalResults> GetPublicKeyRingAsync (long keyId, bool doAsync, CancellationToken cancellationToken)
		{
			foreach (PgpPublicKeyRing ring in PublicKeyRingBundle.GetKeyRings ()) {
				foreach (PgpPublicKey key in ring.GetPublicKeys ()) {
					if (key.KeyId == keyId)
						return new KeyRetrievalResults (ring, key);
				}
			}

			if (AutoKeyRetrieve && IsValidKeyServer) {
				var keyring = await RetrievePublicKeyRingAsync (keyId, doAsync, cancellationToken).ConfigureAwait (false);
				return new KeyRetrievalResults (keyring, keyring.GetPublicKey (keyId));
			}

			return new KeyRetrievalResults (null, null);
		}

		/// <summary>
		/// Enumerate all public keyrings.
		/// </summary>
		/// <remarks>
		/// Enumerates all public keyrings.
		/// </remarks>
		/// <returns>The list of available public keyrings.</returns>
		public virtual IEnumerable<PgpPublicKeyRing> EnumeratePublicKeyRings ()
		{
			foreach (PgpPublicKeyRing keyring in PublicKeyRingBundle.GetKeyRings ())
				yield return keyring;

			yield break;
		}

		/// <summary>
		/// Enumerate all public keys.
		/// </summary>
		/// <remarks>
		/// Enumerates all public keys.
		/// </remarks>
		/// <returns>The list of available public keys.</returns>
		public virtual IEnumerable<PgpPublicKey> EnumeratePublicKeys ()
		{
			foreach (var keyring in EnumeratePublicKeyRings ()) {
				foreach (PgpPublicKey key in keyring.GetPublicKeys ())
					yield return key;
			}

			yield break;
		}

		/// <summary>
		/// Enumerate the public keyrings for a particular mailbox.
		/// </summary>
		/// <remarks>
		/// Enumerates all public keyrings for the specified mailbox.
		/// </remarks>
		/// <returns>The public keys.</returns>
		/// <param name="mailbox">Mailbox.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailbox"/> is <c>null</c>.
		/// </exception>
		public virtual IEnumerable<PgpPublicKeyRing> EnumeratePublicKeyRings (MailboxAddress mailbox)
		{
			if (mailbox == null)
				throw new ArgumentNullException (nameof (mailbox));

			foreach (var keyring in EnumeratePublicKeyRings ()) {
				if (PgpPublicKeyMatches (keyring.GetPublicKey (), mailbox))
					yield return keyring;
			}

			yield break;
		}

		/// <summary>
		/// Enumerate the public keys for a particular mailbox.
		/// </summary>
		/// <remarks>
		/// Enumerates all public keys for the specified mailbox.
		/// </remarks>
		/// <returns>The public keys.</returns>
		/// <param name="mailbox">The mailbox address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailbox"/> is <c>null</c>.
		/// </exception>
		public virtual IEnumerable<PgpPublicKey> EnumeratePublicKeys (MailboxAddress mailbox)
		{
			foreach (var keyring in EnumeratePublicKeyRings (mailbox)) {
				foreach (PgpPublicKey key in keyring.GetPublicKeys ())
					yield return key;
			}

			yield break;
		}

		/// <summary>
		/// Enumerate all secret keyrings.
		/// </summary>
		/// <remarks>
		/// Enumerates all secret keyrings.
		/// </remarks>
		/// <returns>The list of available secret keyrings.</returns>
		public virtual IEnumerable<PgpSecretKeyRing> EnumerateSecretKeyRings ()
		{
			foreach (PgpSecretKeyRing keyring in SecretKeyRingBundle.GetKeyRings ())
				yield return keyring;

			yield break;
		}

		/// <summary>
		/// Enumerate all secret keys.
		/// </summary>
		/// <remarks>
		/// Enumerates all secret keys.
		/// </remarks>
		/// <returns>The list of available secret keys.</returns>
		public virtual IEnumerable<PgpSecretKey> EnumerateSecretKeys ()
		{
			foreach (var keyring in EnumerateSecretKeyRings ()) {
				foreach (PgpSecretKey key in keyring.GetSecretKeys ())
					yield return key;
			}

			yield break;
		}

		/// <summary>
		/// Enumerate the secret keyrings for a particular mailbox.
		/// </summary>
		/// <remarks>
		/// Enumerates all secret keyrings for the specified mailbox.
		/// </remarks>
		/// <returns>The secret keys.</returns>
		/// <param name="mailbox">The mailbox address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailbox"/> is <c>null</c>.
		/// </exception>
		public virtual IEnumerable<PgpSecretKeyRing> EnumerateSecretKeyRings (MailboxAddress mailbox)
		{
			if (mailbox == null)
				throw new ArgumentNullException (nameof (mailbox));

			foreach (var keyring in EnumerateSecretKeyRings ()) {
				if (PgpSecretKeyMatches (keyring.GetSecretKey (), mailbox))
					yield return keyring;
			}

			yield break;
		}

		/// <summary>
		/// Enumerate the secret keys for a particular mailbox.
		/// </summary>
		/// <remarks>
		/// Enumerates all secret keys for the specified mailbox.
		/// </remarks>
		/// <returns>The public keys.</returns>
		/// <param name="mailbox">The mailbox address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailbox"/> is <c>null</c>.
		/// </exception>
		public virtual IEnumerable<PgpSecretKey> EnumerateSecretKeys (MailboxAddress mailbox)
		{
			foreach (var keyring in EnumerateSecretKeyRings (mailbox)) {
				foreach (PgpSecretKey key in keyring.GetSecretKeys ())
					yield return key;
			}

			yield break;
		}

		/// <summary>
		/// Get the public key associated with the mailbox address.
		/// </summary>
		/// <remarks>
		/// Gets a valid public key associated with the mailbox address that can be used for encryption.
		/// </remarks>
		/// <returns>The public encryption key.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailbox"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// The public key for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		protected virtual PgpPublicKey GetPublicKey (MailboxAddress mailbox)
		{
			foreach (var key in EnumeratePublicKeys (mailbox)) {
				if (!key.IsEncryptionKey || key.IsRevoked () || IsExpired (key))
					continue;

				return key;
			}

			throw new PublicKeyNotFoundException (mailbox, "The public key could not be found.");
		}

		/// <summary>
		/// Get the public keys for the specified mailbox addresses.
		/// </summary>
		/// <remarks>
		/// Gets a list of valid public keys for the specified mailbox addresses that can be used for encryption.
		/// </remarks>
		/// <returns>The encryption keys.</returns>
		/// <param name="mailboxes">The mailboxes.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailboxes"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// A public key for one or more of the <paramref name="mailboxes"/> could not be found.
		/// </exception>
		public override IList<PgpPublicKey> GetPublicKeys (IEnumerable<MailboxAddress> mailboxes)
		{
			if (mailboxes == null)
				throw new ArgumentNullException (nameof (mailboxes));

			var recipients = new List<PgpPublicKey> ();

			foreach (var mailbox in mailboxes)
				recipients.Add (GetPublicKey (mailbox));

			return recipients;
		}

		static bool PgpSecretKeyMatches (PgpSecretKey key, MailboxAddress mailbox)
		{
			if (mailbox is SecureMailboxAddress secure && !string.IsNullOrEmpty (secure.Fingerprint)) {
				if (secure.Fingerprint.Length > 16) {
					var fingerprint = HexEncode (key.PublicKey.GetFingerprint ());

					return secure.Fingerprint.Equals (fingerprint, StringComparison.OrdinalIgnoreCase);
				}

				var id = ((int) key.KeyId).ToString ("X2");

				return secure.Fingerprint.EndsWith (id, StringComparison.OrdinalIgnoreCase);
			}

			foreach (string userId in key.UserIds) {
				if (!MailboxAddress.TryParse (userId, out var email))
					continue;

				if (mailbox.Address.Equals (email.Address, StringComparison.OrdinalIgnoreCase))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Get the secret key for a specified key identifier.
		/// </summary>
		/// <remarks>
		/// Gets the secret key for a specified key identifier.
		/// </remarks>
		/// <param name="keyId">The key identifier for the desired secret key.</param>
		/// <returns>The secret key.</returns>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The secret key specified by the <paramref name="keyId"/> could not be found.
		/// </exception>
		public override PgpSecretKey GetSecretKey (long keyId)
		{
			foreach (var key in EnumerateSecretKeys ()) {
				if (key.KeyId == keyId)
					return key;
			}

			throw new PrivateKeyNotFoundException (keyId, "The secret key could not be found.");
		}

		/// <summary>
		/// Get the signing key associated with the mailbox address.
		/// </summary>
		/// <remarks>
		/// Gets the signing key associated with the mailbox address.
		/// </remarks>
		/// <returns>The signing key.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailbox"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// A private key for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		public override PgpSecretKey GetSigningKey (MailboxAddress mailbox)
		{
			if (mailbox == null)
				throw new ArgumentNullException (nameof (mailbox));

			foreach (var keyring in EnumerateSecretKeyRings (mailbox)) {
				foreach (PgpSecretKey key in keyring.GetSecretKeys ()) {
					if (!key.IsSigningKey)
						continue;

					var pubkey = key.PublicKey;
					if (pubkey.IsRevoked () || IsExpired (pubkey))
						continue;

					return key;
				}
			}

			throw new PrivateKeyNotFoundException (mailbox, "The private key could not be found.");
		}

		/// <summary>
		/// Check whether or not a particular mailbox address can be used for signing.
		/// </summary>
		/// <remarks>
		/// Checks whether or not as particular mailbocx address can be used for signing.
		/// </remarks>
		/// <returns><c>true</c> if the mailbox address can be used for signing; otherwise, <c>false</c>.</returns>
		/// <param name="signer">The signer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="signer"/> is <c>null</c>.
		/// </exception>
		public override bool CanSign (MailboxAddress signer)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			foreach (var key in EnumerateSecretKeys (signer)) {
				if (!key.IsSigningKey)
					continue;

				var pubkey = key.PublicKey;
				if (pubkey.IsRevoked () || IsExpired (pubkey))
					continue;

				return true;
			}

			return false;
		}

		/// <summary>
		/// Check whether or not the cryptography context can encrypt to a particular recipient.
		/// </summary>
		/// <remarks>
		/// Checks whether or not the cryptography context can be used to encrypt to a particular recipient.
		/// </remarks>
		/// <returns><c>true</c> if the cryptography context can be used to encrypt to the designated recipient; otherwise, <c>false</c>.</returns>
		/// <param name="mailbox">The recipient's mailbox address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailbox"/> is <c>null</c>.
		/// </exception>
		public override bool CanEncrypt (MailboxAddress mailbox)
		{
			if (mailbox == null)
				throw new ArgumentNullException (nameof (mailbox));

			foreach (var key in EnumeratePublicKeys (mailbox)) {
				if (!key.IsEncryptionKey || key.IsRevoked () || IsExpired (key))
					continue;

				return true;
			}

			return false;
		}

#if false
		/// <summary>
		/// Gets the private key.
		/// </summary>
		/// <remarks>
		/// Gets the private key.
		/// </remarks>
		/// <returns>The private key.</returns>
		/// <param name="keyId">The key identifier.</param>
		/// <exception cref="CertificateNotFoundException">
		/// The specified secret key could not be found.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The user chose to cancel the password prompt.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		protected PgpPrivateKey GetPrivateKey (long keyId)
		{
			var secret = GetSecretKey (keyId);

			return GetPrivateKey (secret);
		}

		PublicKeyAlgorithmTag GetPublicKeyAlgorithmTag (PublicKeyAlgorithm algorithm)
		{
			switch (algorithm) {
			case PublicKeyAlgorithm.DiffieHellman: return PublicKeyAlgorithmTag.DiffieHellman;
			case PublicKeyAlgorithm.Dsa: return PublicKeyAlgorithmTag.Dsa;
			case PublicKeyAlgorithm.EdwardsCurveDsa: throw new NotSupportedException ("EDDSA is not currently supported.");
			case PublicKeyAlgorithm.ElGamalEncrypt: return PublicKeyAlgorithmTag.ElGamalEncrypt;
			case PublicKeyAlgorithm.ElGamalGeneral: return PublicKeyAlgorithmTag.ElGamalGeneral;
			case PublicKeyAlgorithm.EllipticCurve: return PublicKeyAlgorithmTag.ECDH;
			case PublicKeyAlgorithm.EllipticCurveDsa: return PublicKeyAlgorithmTag.ECDsa;
			case PublicKeyAlgorithm.RsaEncrypt: return PublicKeyAlgorithmTag.RsaEncrypt;
			case PublicKeyAlgorithm.RsaGeneral: return PublicKeyAlgorithmTag.RsaGeneral;
			case PublicKeyAlgorithm.RsaSign: return PublicKeyAlgorithmTag.RsaSign;
			default: throw new ArgumentOutOfRangeException (nameof (algorithm));
			}
		}
#endif

		void AddEncryptionKeyPair (PgpKeyRingGenerator keyRingGenerator, KeyGenerationParameters parameters, PublicKeyAlgorithmTag algorithm, DateTime now, long expirationTime, int[] encryptionAlgorithms, int[] digestAlgorithms)
		{
			var keyPairGenerator = GeneratorUtilities.GetKeyPairGenerator ("RSA");

			keyPairGenerator.Init (parameters);

			var keyPair = new PgpKeyPair (algorithm, keyPairGenerator.GenerateKeyPair (), now);
			var subpacketGenerator = new PgpSignatureSubpacketGenerator ();

			subpacketGenerator.SetKeyFlags (false, PgpKeyFlags.CanEncryptCommunications | PgpKeyFlags.CanEncryptStorage);
			subpacketGenerator.SetPreferredSymmetricAlgorithms (false, encryptionAlgorithms);
			subpacketGenerator.SetPreferredHashAlgorithms (false, digestAlgorithms);

			if (expirationTime > 0) {
				subpacketGenerator.SetKeyExpirationTime (false, expirationTime);
				subpacketGenerator.SetSignatureExpirationTime (false, expirationTime);
			}

			keyRingGenerator.AddSubKey (keyPair, subpacketGenerator.Generate (), null);
		}

		PgpKeyRingGenerator CreateKeyRingGenerator (MailboxAddress mailbox, EncryptionAlgorithm algorithm, long expirationTime, string password, DateTime now, SecureRandom random)
		{
			var enabledEncryptionAlgorithms = EnabledEncryptionAlgorithms;
			var enabledDigestAlgorithms = EnabledDigestAlgorithms;
			var encryptionAlgorithms = new int[enabledEncryptionAlgorithms.Length];
			var digestAlgorithms = new int[enabledDigestAlgorithms.Length];

			for (int i = 0; i < enabledEncryptionAlgorithms.Length; i++)
				encryptionAlgorithms[i] = (int) enabledEncryptionAlgorithms[i];
			for (int i = 0; i < enabledDigestAlgorithms.Length; i++)
				digestAlgorithms[i] = (int) enabledDigestAlgorithms[i];

			var parameters = new RsaKeyGenerationParameters (BigInteger.ValueOf (0x10001), random, 2048, 12);
			var signingAlgorithm = PublicKeyAlgorithmTag.RsaSign;

			var keyPairGenerator = GeneratorUtilities.GetKeyPairGenerator ("RSA");

			keyPairGenerator.Init (parameters);

			var signingKeyPair = new PgpKeyPair (signingAlgorithm, keyPairGenerator.GenerateKeyPair (), now);

			var subpacketGenerator = new PgpSignatureSubpacketGenerator ();
			subpacketGenerator.SetKeyFlags (false, PgpKeyFlags.CanSign | PgpKeyFlags.CanCertify);
			subpacketGenerator.SetPreferredSymmetricAlgorithms (false, encryptionAlgorithms);
			subpacketGenerator.SetPreferredHashAlgorithms (false, digestAlgorithms);

			if (expirationTime > 0) {
				subpacketGenerator.SetKeyExpirationTime (false, expirationTime);
				subpacketGenerator.SetSignatureExpirationTime (false, expirationTime);
			}

			subpacketGenerator.SetFeature (false, Org.BouncyCastle.Bcpg.Sig.Features.FEATURE_MODIFICATION_DETECTION);

			var keyRingGenerator = new PgpKeyRingGenerator (
				PgpSignature.PositiveCertification,
				signingKeyPair,
				mailbox.ToString (false),
				GetSymmetricKeyAlgorithm (algorithm),
				CharsetUtils.UTF8.GetBytes (password),
				true,
				subpacketGenerator.Generate (),
				null,
				random);

			// Add the (optional) encryption subkey.
			AddEncryptionKeyPair (keyRingGenerator, parameters, PublicKeyAlgorithmTag.RsaGeneral, now, expirationTime, encryptionAlgorithms, digestAlgorithms);

			return keyRingGenerator;
		}

		/// <summary>
		/// Generate a new key pair.
		/// </summary>
		/// <remarks>
		/// Generates a new RSA key pair.
		/// </remarks>
		/// <param name="mailbox">The mailbox to generate the key pair for.</param>
		/// <param name="password">The password to be set on the secret key.</param>
		/// <param name="expirationDate">The expiration date for the generated key pair.</param>
		/// <param name="algorithm">The symmetric key algorithm to use.</param>
		/// <param name="random">The source of randomness to use when generating the key pair.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="mailbox"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="expirationDate"/> is not a date in the future.
		/// </exception>
		public void GenerateKeyPair (MailboxAddress mailbox, string password, DateTime? expirationDate = null, EncryptionAlgorithm algorithm = EncryptionAlgorithm.Aes256, SecureRandom random = null)
		{
			var now = DateTime.UtcNow;
			long expirationTime = 0;

			if (mailbox == null)
				throw new ArgumentNullException (nameof (mailbox));

			if (password == null)
				throw new ArgumentNullException (nameof (password));

			if (expirationDate.HasValue) {
				var utc = expirationDate.Value.ToUniversalTime ();

				if (utc <= now)
					throw new ArgumentException ("expirationDate needs to be greater than DateTime.Now", nameof (expirationDate));

				if ((expirationTime = Convert.ToInt64 (utc.Subtract (now).TotalSeconds)) <= 0)
					throw new ArgumentException ("expirationDate needs to be greater than DateTime.Now", nameof (expirationDate));
			}

			if (random == null) {
#if !NETSTANDARD1_3 && !NETSTANDARD1_6
				random = new SecureRandom (new CryptoApiRandomGenerator ());
#else
				random = new SecureRandom ();
#endif
			}

			var generator = CreateKeyRingGenerator (mailbox, algorithm, expirationTime, password, now, random);

			Import (generator.GenerateSecretKeyRing ());
			Import (generator.GeneratePublicKeyRing ());
		}

		/// <summary>
		/// Sign a public key.
		/// </summary>
		/// <remarks>
		/// <para>Signs a public key using the specified secret key.</para>
		/// <para>Most OpenPGP implementations use <see cref="OpenPgpKeyCertification.GenericCertification"/>
		/// to make their "key signatures". Some implementations are known to use the other
		/// certification types, but few differentiate between them.</para>
		/// </remarks>
		/// <param name="secretKey">The secret key to use for signing.</param>
		/// <param name="publicKey">The public key to sign.</param>
		/// <param name="digestAlgo">The digest algorithm.</param>
		/// <param name="certification">The certification to give the signed key.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="secretKey"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="publicKey"/> is <c>null</c>.</para>
		/// </exception>
		public void SignKey (PgpSecretKey secretKey, PgpPublicKey publicKey, DigestAlgorithm digestAlgo = DigestAlgorithm.Sha1, OpenPgpKeyCertification certification = OpenPgpKeyCertification.GenericCertification)
		{
			if (secretKey == null)
				throw new ArgumentNullException (nameof (secretKey));

			if (publicKey == null)
				throw new ArgumentNullException (nameof (publicKey));

			var privateKey = GetPrivateKey (secretKey);
			var signatureGenerator = new PgpSignatureGenerator (secretKey.PublicKey.Algorithm, GetHashAlgorithm (digestAlgo));

			signatureGenerator.InitSign ((int) certification, privateKey);
			signatureGenerator.GenerateOnePassVersion (false);

			var subpacketGenerator = new PgpSignatureSubpacketGenerator ();
			var subpacketVector = subpacketGenerator.Generate ();

			signatureGenerator.SetHashedSubpackets (subpacketVector);

			var signedKey = PgpPublicKey.AddCertification (publicKey, signatureGenerator.Generate ());
			PgpPublicKeyRing keyring = null;

			foreach (var ring in EnumeratePublicKeyRings ()) {
				foreach (PgpPublicKey key in ring.GetPublicKeys ()) {
					if (key.KeyId == publicKey.KeyId) {
						PublicKeyRingBundle = PgpPublicKeyRingBundle.RemovePublicKeyRing (PublicKeyRingBundle, ring);
						keyring = PgpPublicKeyRing.InsertPublicKey (ring, signedKey);
						break;
					}
				}
			}

			if (keyring == null)
				keyring = new PgpPublicKeyRing (signedKey.GetEncoded ());

			Import (keyring);
		}

		/// <summary>
		/// Saves the public key-ring bundle.
		/// </summary>
		/// <remarks>
		/// <para>Atomically saves the public key-ring bundle to the path specified by <see cref="PublicKeyRingPath"/>.</para>
		/// <para>Called by <see cref="Import(Stream)"/> if any public keys were successfully imported.</para>
		/// </remarks>
		/// <exception cref="System.IO.IOException">
		/// An error occured while saving the public key-ring bundle.
		/// </exception>
		protected void SavePublicKeyRingBundle ()
		{
			var filename = Path.GetFileName (PublicKeyRingPath) + "~";
			var dirname = Path.GetDirectoryName (PublicKeyRingPath);
			var tmp = Path.Combine (dirname, "." + filename);
			var bak = Path.Combine (dirname, filename);

			Directory.CreateDirectory (dirname);

			using (var file = File.Open (tmp, FileMode.Create, FileAccess.Write)) {
				PublicKeyRingBundle.Encode (file);
				file.Flush ();
			}

			if (File.Exists (PublicKeyRingPath)) {
#if !NETSTANDARD1_3 && !NETSTANDARD1_6
				File.Replace (tmp, PublicKeyRingPath, bak);
#else
				if (File.Exists (bak))
					File.Delete (bak);
				File.Move (PublicKeyRingPath, bak);
				File.Move (tmp, PublicKeyRingPath);
#endif
			} else {
				File.Move (tmp, PublicKeyRingPath);
			}
		}

		/// <summary>
		/// Saves the secret key-ring bundle.
		/// </summary>
		/// <remarks>
		/// <para>Atomically saves the secret key-ring bundle to the path specified by <see cref="SecretKeyRingPath"/>.</para>
		/// <para>Called by <see cref="Import(PgpSecretKeyRing)"/> if any secret keys were successfully imported.</para>
		/// </remarks>
		/// <exception cref="System.IO.IOException">
		/// An error occured while saving the secret key-ring bundle.
		/// </exception>
		protected void SaveSecretKeyRingBundle ()
		{
			var filename = Path.GetFileName (SecretKeyRingPath) + "~";
			var dirname = Path.GetDirectoryName (SecretKeyRingPath);
			var tmp = Path.Combine (dirname, "." + filename);
			var bak = Path.Combine (dirname, filename);

			Directory.CreateDirectory (dirname);

			using (var file = File.Open (tmp, FileMode.Create, FileAccess.Write)) {
				SecretKeyRingBundle.Encode (file);
				file.Flush ();
			}

			if (File.Exists (SecretKeyRingPath)) {
#if !NETSTANDARD1_3 && !NETSTANDARD1_6
				File.Replace (tmp, SecretKeyRingPath, bak);
#else
				if (File.Exists (bak))
					File.Delete (bak);
				File.Move (SecretKeyRingPath, bak);
				File.Move (tmp, SecretKeyRingPath);
#endif
			} else {
				File.Move (tmp, SecretKeyRingPath);
			}
		}

		/// <summary>
		/// Imports a public pgp keyring.
		/// </summary>
		/// <remarks>
		/// Imports a public pgp keyring.
		/// </remarks>
		/// <param name="keyring">The pgp keyring.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="keyring"/> is <c>null</c>.
		/// </exception>
		public virtual void Import (PgpPublicKeyRing keyring)
		{
			if (keyring == null)
				throw new ArgumentNullException (nameof (keyring));

			PublicKeyRingBundle = PgpPublicKeyRingBundle.AddPublicKeyRing (PublicKeyRingBundle, keyring);
			SavePublicKeyRingBundle ();
		}

		/// <summary>
		/// Imports a public pgp keyring bundle.
		/// </summary>
		/// <remarks>
		/// Imports a public pgp keyring bundle.
		/// </remarks>
		/// <param name="bundle">The pgp keyring bundle.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="bundle"/> is <c>null</c>.
		/// </exception>
		public override void Import (PgpPublicKeyRingBundle bundle)
		{
			if (bundle == null)
				throw new ArgumentNullException (nameof (bundle));

			int publicKeysAdded = 0;

			foreach (PgpPublicKeyRing pubring in bundle.GetKeyRings ()) {
				PublicKeyRingBundle = PgpPublicKeyRingBundle.AddPublicKeyRing (PublicKeyRingBundle, pubring);
				publicKeysAdded++;
			}

			if (publicKeysAdded > 0)
				SavePublicKeyRingBundle ();
		}

		/// <summary>
		/// Imports public pgp keys from the specified stream.
		/// </summary>
		/// <remarks>
		/// Imports public pgp keys from the specified stream.
		/// </remarks>
		/// <param name="stream">The raw key data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// <para>An error occurred while parsing the raw key-ring data</para>
		/// <para>-or-</para>
		/// <para>An error occured while saving the public key-ring bundle.</para>
		/// </exception>
		public override void Import (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			using (var armored = new ArmoredInputStream (stream))
				Import (new PgpPublicKeyRingBundle (armored));
		}

		/// <summary>
		/// Imports a secret pgp keyring.
		/// </summary>
		/// <remarks>
		/// Imports a secret pgp keyring.
		/// </remarks>
		/// <param name="keyring">The pgp keyring.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="keyring"/> is <c>null</c>.
		/// </exception>
		public virtual void Import (PgpSecretKeyRing keyring)
		{
			if (keyring == null)
				throw new ArgumentNullException (nameof (keyring));

			SecretKeyRingBundle = PgpSecretKeyRingBundle.AddSecretKeyRing (SecretKeyRingBundle, keyring);
			SaveSecretKeyRingBundle ();
		}

		/// <summary>
		/// Imports a secret pgp keyring bundle.
		/// </summary>
		/// <remarks>
		/// Imports a secret pgp keyring bundle.
		/// </remarks>
		/// <param name="bundle">The pgp keyring bundle.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="bundle"/> is <c>null</c>.
		/// </exception>
		public virtual void Import (PgpSecretKeyRingBundle bundle)
		{
			if (bundle == null)
				throw new ArgumentNullException (nameof (bundle));

			int secretKeysAdded = 0;

			foreach (PgpSecretKeyRing secring in bundle.GetKeyRings ()) {
				SecretKeyRingBundle = PgpSecretKeyRingBundle.AddSecretKeyRing (SecretKeyRingBundle, secring);
				secretKeysAdded++;
			}

			if (secretKeysAdded > 0)
				SaveSecretKeyRingBundle ();
		}

		/// <summary>
		/// Exports the public keys for the specified mailboxes.
		/// </summary>
		/// <remarks>
		/// Exports the public keys for the specified mailboxes.
		/// </remarks>
		/// <returns>A new <see cref="MimePart"/> instance containing the exported public keys.</returns>
		/// <param name="mailboxes">The mailboxes associated with the public keys to export.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailboxes"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="mailboxes"/> was empty.
		/// </exception>
		public override MimePart Export (IEnumerable<MailboxAddress> mailboxes)
		{
			if (mailboxes == null)
				throw new ArgumentNullException (nameof (mailboxes));

			var keyrings = new List<PgpPublicKeyRing> ();
			foreach (var mailbox in mailboxes)
				keyrings.AddRange (EnumeratePublicKeyRings (mailbox));

			var bundle = new PgpPublicKeyRingBundle (keyrings);

			return Export (bundle);
		}

		/// <summary>
		/// Exports the specified public keys.
		/// </summary>
		/// <remarks>
		/// Exports the specified public keys.
		/// </remarks>
		/// <returns>A new <see cref="MimePart"/> instance containing the exported public keys.</returns>
		/// <param name="keys">The public keys to export.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="keys"/> is <c>null</c>.
		/// </exception>
		public MimePart Export (IEnumerable<PgpPublicKey> keys)
		{
			if (keys == null)
				throw new ArgumentNullException (nameof (keys));

			var keyrings = keys.Select (key => new PgpPublicKeyRing (key.GetEncoded ()));
			var bundle = new PgpPublicKeyRingBundle (keyrings);

			return Export (bundle);
		}

		/// <summary>
		/// Export the specified public keys.
		/// </summary>
		/// <remarks>
		/// Exports the specified public keys.
		/// </remarks>
		/// <returns>A new <see cref="MimePart"/> instance containing the exported public keys.</returns>
		/// <param name="keys">The public keys to export.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="keys"/> is <c>null</c>.
		/// </exception>
		public MimePart Export (PgpPublicKeyRingBundle keys)
		{
			if (keys == null)
				throw new ArgumentNullException (nameof (keys));

			var content = new MemoryBlockStream ();

			Export (keys, content, true);

			content.Position = 0;

			return new MimePart ("application", "pgp-keys") {
				ContentDisposition = new ContentDisposition (ContentDisposition.Attachment),
				Content = new MimeContent (content)
			};
		}

		/// <summary>
		/// Export the public keyrings for the specified mailboxes.
		/// </summary>
		/// <remarks>
		/// Exports the public keyrings for the specified mailboxes.
		/// </remarks>
		/// <param name="mailboxes">The mailboxes.</param>
		/// <param name="stream">The output stream.</param>
		/// <param name="armor"><c>true</c> if the output should be armored; otherwise, <c>false</c>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="mailboxes"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public void Export (IEnumerable<MailboxAddress> mailboxes, Stream stream, bool armor)
		{
			if (mailboxes == null)
				throw new ArgumentNullException (nameof (mailboxes));

			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			var keyrings = new List<PgpPublicKeyRing> ();
			foreach (var mailbox in mailboxes)
				keyrings.AddRange (EnumeratePublicKeyRings (mailbox));

			var bundle = new PgpPublicKeyRingBundle (keyrings);

			Export (bundle, stream, armor);
		}

		/// <summary>
		/// Export the specified public keys.
		/// </summary>
		/// <remarks>
		/// Exports the specified public keys.
		/// </remarks>
		/// <returns>A new <see cref="MimePart"/> instance containing the exported public keys.</returns>
		/// <param name="keys">The public keys to export.</param>
		/// <param name="stream">The output stream.</param>
		/// <param name="armor"><c>true</c> if the output should be armored; otherwise, <c>false</c>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="keys"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public void Export (IEnumerable<PgpPublicKey> keys, Stream stream, bool armor)
		{
			if (keys == null)
				throw new ArgumentNullException (nameof (keys));

			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			var keyrings = keys.Select (key => new PgpPublicKeyRing (key.GetEncoded ()));
			var bundle = new PgpPublicKeyRingBundle (keyrings);

			Export (bundle, stream, armor);
		}

		/// <summary>
		/// Export the public keyring bundle.
		/// </summary>
		/// <remarks>
		/// Exports the public keyring bundle.
		/// </remarks>
		/// <param name="keys">The public keyring bundle to export.</param>
		/// <param name="stream">The output stream.</param>
		/// <param name="armor"><c>true</c> if the output should be armored; otherwise, <c>false</c>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="keys"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public void Export (PgpPublicKeyRingBundle keys, Stream stream, bool armor)
		{
			if (keys == null)
				throw new ArgumentNullException (nameof (keys));

			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			if (armor) {
				using (var armored = new ArmoredOutputStream (stream)) {
					armored.SetHeader ("Version", null);

					keys.Encode (armored);
					armored.Flush ();
				}
			} else {
				keys.Encode (stream);
			}
		}

		/// <summary>
		/// Delete a public pgp keyring.
		/// </summary>
		/// <remarks>
		/// Deletes a public pgp keyring.
		/// </remarks>
		/// <param name="keyring">The pgp keyring.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="keyring"/> is <c>null</c>.
		/// </exception>
		public virtual void Delete (PgpPublicKeyRing keyring)
		{
			if (keyring == null)
				throw new ArgumentNullException (nameof (keyring));

			PublicKeyRingBundle = PgpPublicKeyRingBundle.RemovePublicKeyRing (PublicKeyRingBundle, keyring);
			SavePublicKeyRingBundle ();
		}

		/// <summary>
		/// Delete a secret pgp keyring.
		/// </summary>
		/// <remarks>
		/// Deletes a secret pgp keyring.
		/// </remarks>
		/// <param name="keyring">The pgp keyring.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="keyring"/> is <c>null</c>.
		/// </exception>
		public virtual void Delete (PgpSecretKeyRing keyring)
		{
			if (keyring == null)
				throw new ArgumentNullException (nameof (keyring));

			SecretKeyRingBundle = PgpSecretKeyRingBundle.RemoveSecretKeyRing (SecretKeyRingBundle, keyring);
			SaveSecretKeyRingBundle ();
		}
	}
}

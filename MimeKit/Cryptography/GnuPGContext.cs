//
// GnuPGContext.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2024 .NET Foundation and Contributors
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
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Org.BouncyCastle.Math;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto.Parameters;

using MimeKit.Utils;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An <see cref="OpenPgpContext"/> that uses the GnuPG keyrings.
	/// </summary>
	/// <remarks>
	/// An <see cref="OpenPgpContext"/> that uses the GnuPG keyrings.
	/// </remarks>
	public abstract class GnuPGContext : OpenPgpContext
	{
		static readonly Dictionary<string, EncryptionAlgorithm> EncryptionAlgorithms;
		//static readonly Dictionary<string, PublicKeyAlgorithm> PublicKeyAlgorithms;
		static readonly Dictionary<string, DigestAlgorithm> DigestAlgorithms;
		static readonly byte[] EmptyKeyRingBundle = Array.Empty<byte> ();
		static readonly char[] Whitespace = { ' ', '\t' };
		static readonly string GnuPGHomeDir;

		static GnuPGContext ()
		{
			var gnupg = Environment.GetEnvironmentVariable ("GNUPGHOME");

			if (gnupg == null) {
				if (Path.DirectorySeparatorChar == '\\') {
					var appData = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
					gnupg = Path.Combine (appData, "gnupg");
				} else {
					var home = Environment.GetFolderPath (Environment.SpecialFolder.UserProfile);
					gnupg = Path.Combine (home, ".gnupg");
				}
			}

			GnuPGHomeDir = gnupg;

			EncryptionAlgorithms = new Dictionary<string, EncryptionAlgorithm> (StringComparer.Ordinal) {
				{ "AES", EncryptionAlgorithm.Aes128 },
				{ "AES128", EncryptionAlgorithm.Aes128 },
				{ "AES192", EncryptionAlgorithm.Aes192 },
				{ "AES256", EncryptionAlgorithm.Aes256 },
				{ "BLOWFISH", EncryptionAlgorithm.Blowfish },
				{ "CAMELLIA128", EncryptionAlgorithm.Camellia128 },
				{ "CAMELLIA192", EncryptionAlgorithm.Camellia192 },
				{ "CAMELLIA256", EncryptionAlgorithm.Camellia256 },
				{ "CAST5", EncryptionAlgorithm.Cast5 },
				{ "IDEA", EncryptionAlgorithm.Idea },
				{ "3DES", EncryptionAlgorithm.TripleDes },
				{ "TWOFISH", EncryptionAlgorithm.Twofish }
			};

			//PublicKeyAlgorithms = new Dictionary<string, PublicKeyAlgorithm> {
			//	{ "DSA", PublicKeyAlgorithm.Dsa },
			//	{ "ECDH", PublicKeyAlgorithm.EllipticCurve },
			//	{ "ECDSA", PublicKeyAlgorithm.EllipticCurveDsa },
			//	{ "EDDSA", PublicKeyAlgorithm.EdwardsCurveDsa },
			//	{ "ELG", PublicKeyAlgorithm.ElGamalGeneral },
			//	{ "RSA", PublicKeyAlgorithm.RsaGeneral }
			//};

			DigestAlgorithms = new Dictionary<string, DigestAlgorithm> (StringComparer.Ordinal) {
				{ "RIPEMD160", DigestAlgorithm.RipeMD160 },
				{ "SHA1", DigestAlgorithm.Sha1 },
				{ "SHA224", DigestAlgorithm.Sha224 },
				{ "SHA256", DigestAlgorithm.Sha256 },
				{ "SHA384", DigestAlgorithm.Sha384 },
				{ "SHA512", DigestAlgorithm.Sha512 }
			};
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="GnuPGContext"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="GnuPGContext"/> using the default path for the GnuPG home directory.
		/// </remarks>
		protected GnuPGContext () : this (GnuPGHomeDir)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="GnuPGContext"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="GnuPGContext"/> using the specified path for the GnuPG home directory.
		/// </remarks>
		/// <param name="gnupgDir">The path to the GnuPG home directory.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="gnupgDir"/> is <c>null</c>.
		/// </exception>
		protected GnuPGContext (string gnupgDir) : base ()
		{
			if (gnupgDir == null)
				throw new ArgumentNullException (nameof (gnupgDir));

			var pubring = Path.Combine (gnupgDir, "pubring.gpg");
			var secring = Path.Combine (gnupgDir, "secring.gpg");

			PublicKeyRingPath = pubring;
			SecretKeyRingPath = secring;

			if (File.Exists (pubring)) {
				using (var file = File.OpenRead (pubring)) {
					PublicKeyRingBundle = new PgpPublicKeyRingBundle (file);
				}
			} else {
				PublicKeyRingBundle = new PgpPublicKeyRingBundle (EmptyKeyRingBundle);
			}

			if (File.Exists (secring)) {
				using (var file = File.OpenRead (secring)) {
					SecretKeyRingBundle = new PgpSecretKeyRingBundle (file);
				}
			} else {
				SecretKeyRingBundle = new PgpSecretKeyRingBundle (EmptyKeyRingBundle);
			}

			var configFile = Path.Combine (gnupgDir, "gpg.conf");

			LoadConfiguration (configFile);

			foreach (var algorithm in EncryptionAlgorithmRank)
				Enable (algorithm);

			foreach (var algorithm in DigestAlgorithmRank)
				Enable (algorithm);
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

		bool TryGetPublicKeyRing (long keyId, out PgpPublicKeyRing keyring)
		{
			foreach (PgpPublicKeyRing ring in PublicKeyRingBundle.GetKeyRings ()) {
				foreach (PgpPublicKey key in ring.GetPublicKeys ()) {
					if (key.KeyId == keyId) {
						keyring = ring;
						return true;
					}
				}
			}

			keyring = null;

			return false;
		}

		/// <summary>
		/// Get the public keyring that contains the specified key.
		/// </summary>
		/// <remarks>
		/// <para>Gets the public keyring that contains the specified key.</para>
		/// <note type="note">Implementations should first try to obtain the keyring stored (or cached) locally.
		/// Failing that, if <see cref="OpenPgpContext.AutoKeyRetrieve"/> is enabled, they should use
		/// <see cref="OpenPgpContext.RetrievePublicKeyRing(long, CancellationToken)"/> to attempt to
		/// retrieve the keyring from the configured <see cref="OpenPgpContext.KeyServer"/>.</note>
		/// </remarks>
		/// <param name="keyId">The public key identifier.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The public keyring that contains the specified key or <c>null</c> if the keyring could not be found.</returns>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled.
		/// </exception>
		protected override PgpPublicKeyRing GetPublicKeyRing (long keyId, CancellationToken cancellationToken)
		{
			if (TryGetPublicKeyRing (keyId, out var keyring))
				return keyring;

			if (AutoKeyRetrieve)
				return RetrievePublicKeyRing (keyId, cancellationToken);

			return null;
		}

		/// <summary>
		/// Asynchronously get the public keyring that contains the specified key.
		/// </summary>
		/// <remarks>
		/// <para>Gets the public keyring that contains the specified key.</para>
		/// <note type="note">Implementations should first try to obtain the keyring stored (or cached) locally.
		/// Failing that, if <see cref="OpenPgpContext.AutoKeyRetrieve"/> is enabled, they should use
		/// <see cref="OpenPgpContext.RetrievePublicKeyRingAsync(long, CancellationToken)"/> to attempt to
		/// retrieve the keyring from the configured <see cref="OpenPgpContext.KeyServer"/>.</note>
		/// </remarks>
		/// <param name="keyId">The public key identifier.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The public keyring that contains the specified key or <c>null</c> if the keyring could not be found.</returns>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled.
		/// </exception>
		protected override async Task<PgpPublicKeyRing> GetPublicKeyRingAsync (long keyId, CancellationToken cancellationToken)
		{
			if (TryGetPublicKeyRing (keyId, out var keyring))
				return keyring;

			if (AutoKeyRetrieve)
				return await RetrievePublicKeyRingAsync (keyId, cancellationToken).ConfigureAwait (false);

			return null;
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
				if (IsMatch (keyring.GetPublicKey (), mailbox))
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
				if (IsMatch (keyring.GetSecretKey (), mailbox))
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
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailboxes"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// A public key for one or more of the <paramref name="mailboxes"/> could not be found.
		/// </exception>
		public override IList<PgpPublicKey> GetPublicKeys (IEnumerable<MailboxAddress> mailboxes, CancellationToken cancellationToken = default)
		{
			if (mailboxes == null)
				throw new ArgumentNullException (nameof (mailboxes));

			var keys = new List<PgpPublicKey> ();

			foreach (var mailbox in mailboxes)
				keys.Add (GetPublicKey (mailbox));

			return keys;
		}

		/// <summary>
		/// Get the secret key for a specified key identifier.
		/// </summary>
		/// <remarks>
		/// Gets the secret key for a specified key identifier.
		/// </remarks>
		/// <returns>The secret key.</returns>
		/// <param name="keyId">The key identifier for the desired secret key.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The secret key specified by the <paramref name="keyId"/> could not be found.
		/// </exception>
		protected override PgpSecretKey GetSecretKey (long keyId, CancellationToken cancellationToken)
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
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailbox"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// A secret key for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		public override PgpSecretKey GetSigningKey (MailboxAddress mailbox, CancellationToken cancellationToken = default)
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
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="signer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public override bool CanSign (MailboxAddress signer, CancellationToken cancellationToken = default)
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
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailbox"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public override bool CanEncrypt (MailboxAddress mailbox, CancellationToken cancellationToken = default)
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

		static void AddEncryptionKeyPair (PgpKeyRingGenerator keyRingGenerator, KeyGenerationParameters parameters, PublicKeyAlgorithmTag algorithm, DateTime now, long expirationTime, int[] encryptionAlgorithms, int[] digestAlgorithms)
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

			random ??= new SecureRandom (new CryptoApiRandomGenerator ());

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

			keyring ??= new PgpPublicKeyRing (signedKey.GetEncoded ());

			Import (keyring);
		}

		/// <summary>
		/// Saves the public key-ring bundle.
		/// </summary>
		/// <remarks>
		/// <para>Atomically saves the public key-ring bundle to the path specified by <see cref="PublicKeyRingPath"/>.</para>
		/// <para>Called by <see cref="Import(PgpPublicKeyRingBundle,CancellationToken)"/> if any public keys were successfully imported.</para>
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
				File.Replace (tmp, PublicKeyRingPath, bak);
			} else {
				File.Move (tmp, PublicKeyRingPath);
			}
		}

		/// <summary>
		/// Saves the secret key-ring bundle.
		/// </summary>
		/// <remarks>
		/// <para>Atomically saves the secret key-ring bundle to the path specified by <see cref="SecretKeyRingPath"/>.</para>
		/// <para>Called by <see cref="Import(PgpSecretKeyRingBundle,CancellationToken)"/> if any secret keys were successfully imported.</para>
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
				File.Replace (tmp, SecretKeyRingPath, bak);
			} else {
				File.Move (tmp, SecretKeyRingPath);
			}
		}

		void UpdateKeyServer (string value)
		{
			if (string.IsNullOrEmpty (value)) {
				KeyServer = null;
				return;
			}

			if (!Uri.IsWellFormedUriString (value, UriKind.Absolute))
				return;

			KeyServer = new Uri (value, UriKind.Absolute);
		}

		void UpdateKeyServerOptions (string value)
		{
			if (string.IsNullOrEmpty (value))
				return;

			var options = value.Split (Whitespace, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < options.Length; i++) {
				switch (options[i]) {
				case "auto-key-retrieve":
					AutoKeyRetrieve = true;
					break;
				}
			}
		}

		static EncryptionAlgorithm[] ParseEncryptionAlgorithms (string value)
		{
			var names = value != null ? value.Split (Whitespace, StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string> ();
			var algorithms = new List<EncryptionAlgorithm> ();
			var seen = new HashSet<EncryptionAlgorithm> ();

			for (int i = 0; i < names.Length; i++) {
				var name = names[i].ToUpperInvariant ();

				if (EncryptionAlgorithms.TryGetValue (name, out var algorithm) && seen.Add (algorithm))
					algorithms.Add (algorithm);
			}

			if (!seen.Contains (EncryptionAlgorithm.TripleDes))
				algorithms.Add (EncryptionAlgorithm.TripleDes);

			return algorithms.ToArray ();
		}

		//static PublicKeyAlgorithm[] ParsePublicKeyAlgorithms (string value)
		//{
		//	var names = value.Split (new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
		//	var algorithms = new List<PublicKeyAlgorithm> ();
		//	var seen = new HashSet<PublicKeyAlgorithm> ();

		//	for (int i = 0; i < names.Length; i++) {
		//		var name = names[i].ToUpperInvariant ();
		//		PublicKeyAlgorithm algorithm;

		//		if (PublicKeyAlgorithms.TryGetValue (name, out algorithm) && seen.Add (algorithm))
		//			algorithms.Add (algorithm);
		//	}

		//	if (!seen.Contains (PublicKeyAlgorithm.Dsa))
		//		seen.Add (PublicKeyAlgorithm.Dsa);

		//	return algorithms.ToArray ();
		//}

		static DigestAlgorithm[] ParseDigestAlgorithms (string value)
		{
			var names = value != null ? value.Split (Whitespace, StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string> ();
			var algorithms = new List<DigestAlgorithm> ();
			var seen = new HashSet<DigestAlgorithm> ();

			for (int i = 0; i < names.Length; i++) {
				var name = names[i].ToUpperInvariant ();

				if (DigestAlgorithms.TryGetValue (name, out var algorithm) && seen.Add (algorithm))
					algorithms.Add (algorithm);
			}

			if (!seen.Contains (DigestAlgorithm.Sha1))
				algorithms.Add (DigestAlgorithm.Sha1);

			return algorithms.ToArray ();
		}

		void UpdatePersonalCipherPreferences (string value)
		{
			EncryptionAlgorithmRank = ParseEncryptionAlgorithms (value);
		}

		void UpdatePersonalDigestPreferences (string value)
		{
			DigestAlgorithmRank = ParseDigestAlgorithms (value);
		}

		void LoadConfiguration (string configFile)
		{
			if (!File.Exists (configFile))
				return;

			using (var reader = File.OpenText (configFile)) {
				string line;

				while ((line = reader.ReadLine ()) != null) {
					int startIndex = 0;

					while (startIndex < line.Length && char.IsWhiteSpace (line[startIndex]))
						startIndex++;

					if (startIndex == line.Length || line[startIndex] == '#')
						continue;

					int endIndex = startIndex;
					while (endIndex < line.Length && !char.IsWhiteSpace (line[endIndex]))
						endIndex++;

					var option = line.Substring (startIndex, endIndex - startIndex);
					string value;

					if (endIndex < line.Length)
						value = line.AsSpan (endIndex + 1).Trim ().ToString ();
					else
						value = null;

					switch (option) {
					case "keyserver":
						UpdateKeyServer (value);
						break;
					case "keyserver-options":
						UpdateKeyServerOptions (value);
						break;
					case "personal-cipher-preferences":
						UpdatePersonalCipherPreferences (value);
						break;
					case "personal-digest-preferences":
						UpdatePersonalDigestPreferences (value);
						break;
					//case "personal-compress-preferences":
					//	break;
					}
				}
			}
		}

		/// <summary>
		/// Import a public pgp keyring.
		/// </summary>
		/// <remarks>
		/// Imports a public pgp keyring.
		/// </remarks>
		/// <param name="keyring">The public key-ring to import.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="keyring"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An error occured while saving the public key-ring.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public override void Import (PgpPublicKeyRing keyring, CancellationToken cancellationToken = default)
		{
			if (keyring == null)
				throw new ArgumentNullException (nameof (keyring));

			PublicKeyRingBundle = PgpPublicKeyRingBundle.AddPublicKeyRing (PublicKeyRingBundle, keyring);
			SavePublicKeyRingBundle ();
		}

		/// <summary>
		/// Import the specified public keyring bundle.
		/// </summary>
		/// <remarks>
		/// Imports the specified public keyring bundle.
		/// </remarks>
		/// <param name="bundle">The bundle of public keyrings to import.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="bundle"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An error occured while saving the public key-ring.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public override void Import (PgpPublicKeyRingBundle bundle, CancellationToken cancellationToken = default)
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
		/// Import a secret pgp keyring.
		/// </summary>
		/// <remarks>
		/// Imports a secret pgp keyring.
		/// </remarks>
		/// <param name="keyring">The secret key-ring to import.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="keyring"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An error occured while saving the secret key-ring.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public override void Import (PgpSecretKeyRing keyring, CancellationToken cancellationToken = default)
		{
			if (keyring == null)
				throw new ArgumentNullException (nameof (keyring));

			SecretKeyRingBundle = PgpSecretKeyRingBundle.AddSecretKeyRing (SecretKeyRingBundle, keyring);
			SaveSecretKeyRingBundle ();
		}

		/// <summary>
		/// Import a secret pgp keyring bundle.
		/// </summary>
		/// <remarks>
		/// Imports a secret pgp keyring bundle.
		/// </remarks>
		/// <param name="bundle">The bundle of secret keyrings to import.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="bundle"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An error occured while saving the secret key-ring bundle.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public override void Import (PgpSecretKeyRingBundle bundle, CancellationToken cancellationToken = default)
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
		/// Export the public keys for the specified mailboxes.
		/// </summary>
		/// <remarks>
		/// Exports the public keys for the specified mailboxes.
		/// </remarks>
		/// <returns>A new <see cref="MimePart"/> instance containing the exported public keys.</returns>
		/// <param name="mailboxes">The mailboxes associated with the public keys to export.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailboxes"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="mailboxes"/> was empty.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public override MimePart Export (IEnumerable<MailboxAddress> mailboxes, CancellationToken cancellationToken = default)
		{
			if (mailboxes == null)
				throw new ArgumentNullException (nameof (mailboxes));

			var keyrings = new List<PgpPublicKeyRing> ();
			foreach (var mailbox in mailboxes)
				keyrings.AddRange (EnumeratePublicKeyRings (mailbox));

			var bundle = new PgpPublicKeyRingBundle (keyrings);

			return Export (bundle, cancellationToken);
		}

		/// <summary>
		/// Asynchronously export the public keys for the specified mailboxes.
		/// </summary>
		/// <remarks>
		/// Asynchronously exports the public keys for the specified mailboxes.
		/// </remarks>
		/// <returns>A new <see cref="MimePart"/> instance containing the exported public keys.</returns>
		/// <param name="mailboxes">The mailboxes associated with the public keys to export.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailboxes"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="mailboxes"/> was empty.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public override Task<MimePart> ExportAsync (IEnumerable<MailboxAddress> mailboxes, CancellationToken cancellationToken = default)
		{
			if (mailboxes == null)
				throw new ArgumentNullException (nameof (mailboxes));

			var keyrings = new List<PgpPublicKeyRing> ();
			foreach (var mailbox in mailboxes)
				keyrings.AddRange (EnumeratePublicKeyRings (mailbox));

			var bundle = new PgpPublicKeyRingBundle (keyrings);

			return ExportAsync (bundle, cancellationToken);
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
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="mailboxes"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="mailboxes"/> was empty.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// Exporting keys is not supported by this cryptography context.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public override void Export (IEnumerable<MailboxAddress> mailboxes, Stream stream, bool armor, CancellationToken cancellationToken = default)
		{
			if (mailboxes == null)
				throw new ArgumentNullException (nameof (mailboxes));

			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			var keyrings = new List<PgpPublicKeyRing> ();
			foreach (var mailbox in mailboxes)
				keyrings.AddRange (EnumeratePublicKeyRings (mailbox));

			var bundle = new PgpPublicKeyRingBundle (keyrings);

			Export (bundle, stream, armor, cancellationToken);
		}

		/// <summary>
		/// Asynchronously export the public keyrings for the specified mailboxes.
		/// </summary>
		/// <remarks>
		/// Asynchronously exports the public keyrings for the specified mailboxes.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="mailboxes">The mailboxes.</param>
		/// <param name="stream">The output stream.</param>
		/// <param name="armor"><c>true</c> if the output should be armored; otherwise, <c>false</c>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="mailboxes"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="mailboxes"/> was empty.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// Exporting keys is not supported by this cryptography context.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public override Task ExportAsync (IEnumerable<MailboxAddress> mailboxes, Stream stream, bool armor, CancellationToken cancellationToken = default)
		{
			if (mailboxes == null)
				throw new ArgumentNullException (nameof (mailboxes));

			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			var keyrings = new List<PgpPublicKeyRing> ();
			foreach (var mailbox in mailboxes)
				keyrings.AddRange (EnumeratePublicKeyRings (mailbox));

			var bundle = new PgpPublicKeyRingBundle (keyrings);

			return ExportAsync (bundle, stream, armor, cancellationToken);
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

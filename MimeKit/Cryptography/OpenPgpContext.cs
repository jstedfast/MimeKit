//
// OpenPgpContext.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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
using System.Diagnostics;
using System.Collections.Generic;

using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;

using MimeKit.IO;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An abstract OpenPGP cryptography context which can be used for PGP/MIME.
	/// </summary>
	/// <remarks>
	/// Generally speaking, applications should not use a <see cref="OpenPgpContext"/>
	/// directly, but rather via higher level APIs such as <see cref="MultipartSigned"/>
	/// and <see cref="MultipartEncrypted"/>.
	/// </remarks>
	public abstract class OpenPgpContext : CryptographyContext
	{
		EncryptionAlgorithm defaultAlgorithm = EncryptionAlgorithm.Cast5;

		/// <summary>
		/// Gets or sets the default encryption algorithm.
		/// </summary>
		/// <remarks>
		/// Gets or sets the default encryption algorithm.
		/// </remarks>
		/// <value>The encryption algorithm.</value>
		/// <exception cref="System.NotSupportedException">
		/// The specified encryption algorithm is not supported.
		/// </exception>
		public EncryptionAlgorithm DefaultEncryptionAlgorithm {
			get { return defaultAlgorithm; }
			set {
				GetSymmetricKeyAlgorithm (value);
				defaultAlgorithm = value;
			}
		}

		/// <summary>
		/// Gets the public keyring path.
		/// </summary>
		/// <remarks>
		/// Gets the public keyring path.
		/// </remarks>
		/// <value>The public key ring path.</value>
		protected string PublicKeyRingPath {
			get; set;
		}

		/// <summary>
		/// Gets the secret keyring path.
		/// </summary>
		/// <remarks>
		/// Gets the secret keyring path.
		/// </remarks>
		/// <value>The secret key ring path.</value>
		protected string SecretKeyRingPath {
			get; set;
		}

		/// <summary>
		/// Gets the public keyring bundle.
		/// </summary>
		/// <remarks>
		/// Gets the public keyring bundle.
		/// </remarks>
		/// <value>The public keyring bundle.</value>
		public PgpPublicKeyRingBundle PublicKeyRingBundle {
			get; protected set;
		}

		/// <summary>
		/// Gets the secret keyring bundle.
		/// </summary>
		/// <remarks>
		/// Gets the secret keyring bundle.
		/// </remarks>
		/// <value>The secret keyring bundle.</value>
		public PgpSecretKeyRingBundle SecretKeyRingBundle {
			get; protected set;
		}

		/// <summary>
		/// Gets the signature protocol.
		/// </summary>
		/// <remarks>
		/// <para>The signature protocol is used by <see cref="MultipartSigned"/>
		/// in order to determine what the protocol parameter of the Content-Type
		/// header should be.</para>
		/// </remarks>
		/// <value>The signature protocol.</value>
		public override string SignatureProtocol {
			get { return "application/pgp-signature"; }
		}

		/// <summary>
		/// Gets the encryption protocol.
		/// </summary>
		/// <remarks>
		/// <para>The encryption protocol is used by <see cref="MultipartEncrypted"/>
		/// in order to determine what the protocol parameter of the Content-Type
		/// header should be.</para>
		/// </remarks>
		/// <value>The encryption protocol.</value>
		public override string EncryptionProtocol {
			get { return "application/pgp-encrypted"; }
		}

		/// <summary>
		/// Gets the key exchange protocol.
		/// </summary>
		/// <remarks>
		/// Gets the key exchange protocol.
		/// </remarks>
		/// <value>The key exchange protocol.</value>
		public override string KeyExchangeProtocol {
			get { return "application/pgp-keys"; }
		}

		/// <summary>
		/// Checks whether or not the specified protocol is supported.
		/// </summary>
		/// <remarks>
		/// Used in order to make sure that the protocol parameter value specified in either a multipart/signed
		/// or multipart/encrypted part is supported by the supplied cryptography context.
		/// </remarks>
		/// <returns><c>true</c> if the protocol is supported; otherwise <c>false</c></returns>
		/// <param name="protocol">The protocol.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="protocol"/> is <c>null</c>.
		/// </exception>
		public override bool Supports (string protocol)
		{
			if (protocol == null)
				throw new ArgumentNullException ("protocol");

			var type = protocol.ToLowerInvariant ().Split ('/');
			if (type.Length != 2 || type[0] != "application")
				return false;

			if (type[1].StartsWith ("x-", StringComparison.Ordinal))
				type[1] = type[1].Substring (2);

			return type[1] == "pgp-signature" || type[1] == "pgp-encrypted" || type[1] == "pgp-keys";
		}

		/// <summary>
		/// Gets the string name of the digest algorithm for use with the micalg parameter of a multipart/signed part.
		/// </summary>
		/// <remarks>
		/// <para>Maps the <see cref="DigestAlgorithm"/> to the appropriate string identifier
		/// as used by the micalg parameter value of a multipart/signed Content-Type
		/// header. For example:</para>
		/// <list type="table">
		/// <listheader><term>Algorithm</term><description>Name</description></listheader>
		/// <item><term><see cref="DigestAlgorithm.MD5"/></term><description>pgp-md5</description></item>
		/// <item><term><see cref="DigestAlgorithm.Sha1"/></term><description>pgp-sha1</description></item>
		/// <item><term><see cref="DigestAlgorithm.RipeMD160"/></term><description>pgp-ripemd160</description></item>
		/// <item><term><see cref="DigestAlgorithm.MD2"/></term><description>pgp-md2</description></item>
		/// <item><term><see cref="DigestAlgorithm.Tiger192"/></term><description>pgp-tiger192</description></item>
		/// <item><term><see cref="DigestAlgorithm.Haval5160"/></term><description>pgp-haval-5-160</description></item>
		/// <item><term><see cref="DigestAlgorithm.Sha256"/></term><description>pgp-sha256</description></item>
		/// <item><term><see cref="DigestAlgorithm.Sha384"/></term><description>pgp-sha384</description></item>
		/// <item><term><see cref="DigestAlgorithm.Sha512"/></term><description>pgp-sha512</description></item>
		/// <item><term><see cref="DigestAlgorithm.Sha224"/></term><description>pgp-sha224</description></item>
		/// </list>
		/// </remarks>
		/// <returns>The micalg value.</returns>
		/// <param name="micalg">The digest algorithm.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="micalg"/> is out of range.
		/// </exception>
		public override string GetDigestAlgorithmName (DigestAlgorithm micalg)
		{
			switch (micalg) {
			case DigestAlgorithm.MD5:        return "pgp-md5";
			case DigestAlgorithm.Sha1:       return "pgp-sha1";
			case DigestAlgorithm.RipeMD160:  return "pgp-ripemd160";
			case DigestAlgorithm.MD2:        return "pgp-md2";
			case DigestAlgorithm.Tiger192:   return "pgp-tiger192";
			case DigestAlgorithm.Haval5160:  return "pgp-haval-5-160";
			case DigestAlgorithm.Sha256:     return "pgp-sha256";
			case DigestAlgorithm.Sha384:     return "pgp-sha384";
			case DigestAlgorithm.Sha512:     return "pgp-sha512";
			case DigestAlgorithm.Sha224:     return "pgp-sha224";
			case DigestAlgorithm.MD4:        return "pgp-md4";
			default: throw new ArgumentOutOfRangeException ("micalg");
			}
		}

		/// <summary>
		/// Gets the digest algorithm from the micalg parameter value in a multipart/signed part.
		/// </summary>
		/// <remarks>
		/// Maps the micalg parameter value string back to the appropriate <see cref="DigestAlgorithm"/>.
		/// </remarks>
		/// <returns>The digest algorithm.</returns>
		/// <param name="micalg">The micalg parameter value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="micalg"/> is <c>null</c>.
		/// </exception>
		public override DigestAlgorithm GetDigestAlgorithm (string micalg)
		{
			if (micalg == null)
				throw new ArgumentNullException ("micalg");

			switch (micalg.ToLowerInvariant ()) {
			case "pgp-md5":         return DigestAlgorithm.MD5;
			case "pgp-sha1":        return DigestAlgorithm.Sha1;
			case "pgp-ripemd160":   return DigestAlgorithm.RipeMD160;
			case "pgp-md2":         return DigestAlgorithm.MD2;
			case "pgp-tiger192":    return DigestAlgorithm.Tiger192;
			case "pgp-haval-5-160": return DigestAlgorithm.Haval5160;
			case "pgp-sha256":      return DigestAlgorithm.Sha256;
			case "pgp-sha384":      return DigestAlgorithm.Sha384;
			case "pgp-sha512":      return DigestAlgorithm.Sha512;
			case "pgp-sha224":      return DigestAlgorithm.Sha224;
			case "pgp-md4":         return DigestAlgorithm.MD4;
			default:                return DigestAlgorithm.None;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.OpenPgpContext"/> class.
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
		protected OpenPgpContext (string pubring, string secring)
		{
			if (pubring == null)
				throw new ArgumentNullException ("pubring");

			if (secring == null)
				throw new ArgumentNullException ("secring");

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
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.OpenPgpContext"/> class.
		/// </summary>
		/// <remarks>
		/// Subclasses choosing to use this constructor MUST set the <see cref="PublicKeyRingPath"/>,
		/// <see cref="SecretKeyRingPath"/>, <see cref="PublicKeyRingBundle"/>, and the
		/// <see cref="SecretKeyRingBundle"/> properties themselves.
		/// </remarks>
		protected OpenPgpContext ()
		{
		}

		static string GetFingerprint (long keyId, int length)
		{
			if (length < 16)
				return ((int) keyId).ToString ("X2");

			return keyId.ToString ("X2");
		}

		static bool PgpPublicKeyMatches (PgpPublicKey key, MailboxAddress mailbox)
		{
			var secure = mailbox as SecureMailboxAddress;

			if (secure != null) {
				var fingerprint = GetFingerprint (key.KeyId, secure.Fingerprint.Length);

				return secure.Fingerprint.EndsWith (fingerprint, StringComparison.OrdinalIgnoreCase);
			}

			foreach (string userId in key.GetUserIds ()) {
				MailboxAddress email;

				if (!MailboxAddress.TryParse (userId, out email))
					continue;

				if (string.Compare (mailbox.Address, email.Address, StringComparison.OrdinalIgnoreCase) == 0)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Gets the public key associated with the mailbox address.
		/// </summary>
		/// <remarks>
		/// Gets the public key associated with the mailbox address.
		/// </remarks>
		/// <returns>The encryption key.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailbox"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// The public key for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		protected virtual PgpPublicKey GetPublicKey (MailboxAddress mailbox)
		{
			if (mailbox == null)
				throw new ArgumentNullException ("mailbox");

			foreach (PgpPublicKeyRing keyring in PublicKeyRingBundle.GetKeyRings ()) {
				foreach (PgpPublicKey key in keyring.GetPublicKeys ()) {
					if (!PgpPublicKeyMatches (keyring.GetPublicKey (), mailbox))
						continue;

					if (!key.IsEncryptionKey || key.IsRevoked ())
						continue;

					long seconds = key.GetValidSeconds ();
					if (seconds != 0) {
						var expires = key.CreationTime.AddSeconds ((double) seconds);
						if (expires >= DateTime.Now)
							continue;
					}

					return key;
				}
			}

			throw new PublicKeyNotFoundException (mailbox, "The public key could not be found.");
		}

		/// <summary>
		/// Gets the public keys for the specified mailbox addresses.
		/// </summary>
		/// <remarks>
		/// Gets the public keys for the specified mailbox addresses.
		/// </remarks>
		/// <returns>The encryption keys.</returns>
		/// <param name="mailboxes">The mailboxes.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailboxes"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// A public key for one or more of the <paramref name="mailboxes"/> could not be found.
		/// </exception>
		protected virtual IList<PgpPublicKey> GetPublicKeys (IEnumerable<MailboxAddress> mailboxes)
		{
			if (mailboxes == null)
				throw new ArgumentNullException ("mailboxes");

			var recipients = new List<PgpPublicKey> ();

			foreach (var mailbox in mailboxes)
				recipients.Add (GetPublicKey (mailbox));

			return recipients;
		}

		static bool PgpSecretKeyMatches (PgpSecretKey key, MailboxAddress mailbox)
		{
			var secure = mailbox as SecureMailboxAddress;

			if (secure != null) {
				var fingerprint = GetFingerprint (key.KeyId, secure.Fingerprint.Length);

				return secure.Fingerprint.EndsWith (fingerprint, StringComparison.OrdinalIgnoreCase);
			}

			foreach (string userId in key.UserIds) {
				MailboxAddress email;

				if (!MailboxAddress.TryParse (userId, out email))
					continue;

				if (string.Compare (mailbox.Address, email.Address, StringComparison.OrdinalIgnoreCase) == 0)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Gets the signing key associated with the mailbox address.
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
		protected virtual PgpSecretKey GetSigningKey (MailboxAddress mailbox)
		{
			if (mailbox == null)
				throw new ArgumentNullException ("mailbox");

			foreach (PgpSecretKeyRing keyring in SecretKeyRingBundle.GetKeyRings ()) {
				foreach (PgpSecretKey key in keyring.GetSecretKeys ()) {
					if (!PgpSecretKeyMatches (keyring.GetSecretKey (), mailbox))
						continue;

					if (!key.IsSigningKey)
						continue;

					var pubkey = key.PublicKey;
					if (pubkey.IsRevoked ())
						continue;

					long seconds = pubkey.GetValidSeconds ();
					if (seconds != 0) {
						var expires = pubkey.CreationTime.AddSeconds ((double) seconds);
						if (DateTime.Now >= expires)
							continue;
					}

					return key;
				}
			}

			throw new PrivateKeyNotFoundException (mailbox, "The private key could not be found.");
		}

		/// <summary>
		/// Gets the password for key.
		/// </summary>
		/// <remarks>
		/// Gets the password for key.
		/// </remarks>
		/// <returns>The password for key.</returns>
		/// <param name="key">The key.</param>
		/// <exception cref="System.OperationCanceledException">
		/// The user chose to cancel the password request.
		/// </exception>
		protected abstract string GetPasswordForKey (PgpSecretKey key);

		/// <summary>
		/// Gets the private key from the specified secret key.
		/// </summary>
		/// <remarks>
		/// Gets the private key from the specified secret key.
		/// </remarks>
		/// <returns>The private key.</returns>
		/// <param name="key">The secret key.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The user chose to cancel the password prompt.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		protected PgpPrivateKey GetPrivateKey (PgpSecretKey key)
		{
			int attempts = 0;
			string password;

			if (key == null)
				throw new ArgumentNullException ("key");

			do {
				if ((password = GetPasswordForKey (key)) == null)
					throw new OperationCanceledException ();

				try {
					var privateKey = key.ExtractPrivateKey (password.ToCharArray ());

					// Note: the private key will be null if the private key is empty.
					if (privateKey == null)
						break;

					return privateKey;
				} catch (Exception ex) {
#if DEBUG
					Debug.WriteLine (string.Format ("Failed to extract secret key: {0}", ex));
#endif
				}

				attempts++;
			} while (attempts < 3);

			throw new UnauthorizedAccessException ();
		}

		PgpSecretKey GetSecretKey (long keyId)
		{
			foreach (PgpSecretKeyRing keyring in SecretKeyRingBundle.GetKeyRings ()) {
				foreach (PgpSecretKey key in keyring.GetSecretKeys ()) {
					if (key.KeyId == keyId)
						return key;
				}
			}

			throw new PrivateKeyNotFoundException (keyId, "The private key could not be found.");
		}

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

		/// <summary>
		/// Gets the equivalent <see cref="Org.BouncyCastle.Bcpg.HashAlgorithmTag"/> for the 
		/// specified <see cref="DigestAlgorithm"/>. 
		/// </summary>
		/// <remarks>
		/// Maps a <see cref="DigestAlgorithm"/> to the equivalent <see cref="Org.BouncyCastle.Bcpg.HashAlgorithmTag"/>.
		/// </remarks>
		/// <returns>The hash algorithm.</returns>
		/// <param name="digestAlgo">The digest algorithm.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="digestAlgo"/> is out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <paramref name="digestAlgo"/> does not have an equivalent
		/// <see cref="Org.BouncyCastle.Bcpg.HashAlgorithmTag"/> value.
		/// </exception>
		public static HashAlgorithmTag GetHashAlgorithm (DigestAlgorithm digestAlgo)
		{
			switch (digestAlgo) {
			case DigestAlgorithm.MD5:       return HashAlgorithmTag.MD5;
			case DigestAlgorithm.Sha1:      return HashAlgorithmTag.Sha1;
			case DigestAlgorithm.RipeMD160: return HashAlgorithmTag.RipeMD160;
			case DigestAlgorithm.DoubleSha: return HashAlgorithmTag.DoubleSha;
			case DigestAlgorithm.MD2:       return HashAlgorithmTag.MD2;
			case DigestAlgorithm.Tiger192:  return HashAlgorithmTag.Tiger192;
			case DigestAlgorithm.Haval5160: return HashAlgorithmTag.Haval5pass160;
			case DigestAlgorithm.Sha256:    return HashAlgorithmTag.Sha256;
			case DigestAlgorithm.Sha384:    return HashAlgorithmTag.Sha384;
			case DigestAlgorithm.Sha512:    return HashAlgorithmTag.Sha512;
			case DigestAlgorithm.Sha224:    return HashAlgorithmTag.Sha224;
			case DigestAlgorithm.MD4: throw new NotSupportedException ("The MD4 digest algorithm is not supported.");
			default: throw new ArgumentOutOfRangeException ("digestAlgo");
			}
		}

		/// <summary>
		/// Cryptographically signs the content.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs the content using the specified signer and digest algorithm.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.MimePart"/> instance
		/// containing the detached signature data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="content">The content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="digestAlgo"/> is out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The specified <see cref="DigestAlgorithm"/> is not supported by this context.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// A signing key could not be found for <paramref name="signer"/>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The user chose to cancel the password prompt.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public override MimePart Sign (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content)
		{
			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (content == null)
				throw new ArgumentNullException ("content");

			var key = GetSigningKey (signer);

			return Sign (key, digestAlgo, content);
		}

		/// <summary>
		/// Cryptographically signs the content.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs the content using the specified signer and digest algorithm.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.MimePart"/> instance
		/// containing the detached signature data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="content">The content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="signer"/> cannot be used for signing.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The <paramref name="digestAlgo"/> is not supported.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The user chose to cancel the password prompt.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public ApplicationPgpSignature Sign (PgpSecretKey signer, DigestAlgorithm digestAlgo, Stream content)
		{
			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (!signer.IsSigningKey)
				throw new ArgumentException ("The specified secret key cannot be used for signing.", "signer");

			if (content == null)
				throw new ArgumentNullException ("content");

			var hashAlgorithm = GetHashAlgorithm (digestAlgo);
			var memory = new MemoryBlockStream ();

			using (var armored = new ArmoredOutputStream (memory)) {
				var compresser = new PgpCompressedDataGenerator (CompressionAlgorithmTag.ZLib);
				using (var compressed = compresser.Open (armored)) {
					var signatureGenerator = new PgpSignatureGenerator (signer.PublicKey.Algorithm, hashAlgorithm);
					var buf = new byte[4096];
					int nread;

					signatureGenerator.InitSign (PgpSignature.CanonicalTextDocument, GetPrivateKey (signer));

					while ((nread = content.Read (buf, 0, buf.Length)) > 0)
						signatureGenerator.Update (buf, 0, nread);

					var signature = signatureGenerator.Generate ();

					signature.Encode (compressed);
					compressed.Flush ();
				}

				armored.Flush ();
			}

			memory.Position = 0;

			return new ApplicationPgpSignature (memory);
		}

		/// <summary>
		/// Gets the equivalent <see cref="DigestAlgorithm"/> for the specified
		/// <see cref="Org.BouncyCastle.Bcpg.HashAlgorithmTag"/>.
		/// </summary>
		/// <remarks>
		/// Gets the equivalent <see cref="DigestAlgorithm"/> for the specified
		/// <see cref="Org.BouncyCastle.Bcpg.HashAlgorithmTag"/>.
		/// </remarks>
		/// <returns>The digest algorithm.</returns>
		/// <param name="hashAlgorithm">The hash algorithm.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="hashAlgorithm"/> is out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <paramref name="hashAlgorithm"/> does not have an equivalent <see cref="DigestAlgorithm"/> value.
		/// </exception>
		public static DigestAlgorithm GetDigestAlgorithm (HashAlgorithmTag hashAlgorithm)
		{
			switch (hashAlgorithm) {
			case HashAlgorithmTag.MD5:           return DigestAlgorithm.MD5;
			case HashAlgorithmTag.Sha1:          return DigestAlgorithm.Sha1;
			case HashAlgorithmTag.RipeMD160:     return DigestAlgorithm.RipeMD160;
			case HashAlgorithmTag.DoubleSha:     return DigestAlgorithm.DoubleSha;
			case HashAlgorithmTag.MD2:           return DigestAlgorithm.MD2;
			case HashAlgorithmTag.Tiger192:      return DigestAlgorithm.Tiger192;
			case HashAlgorithmTag.Haval5pass160: return DigestAlgorithm.Haval5160;
			case HashAlgorithmTag.Sha256:        return DigestAlgorithm.Sha256;
			case HashAlgorithmTag.Sha384:        return DigestAlgorithm.Sha384;
			case HashAlgorithmTag.Sha512:        return DigestAlgorithm.Sha512;
			case HashAlgorithmTag.Sha224:        return DigestAlgorithm.Sha224;
			default: throw new ArgumentOutOfRangeException ("hashAlgorithm");
			}
		}

		/// <summary>
		/// Gets the equivalent <see cref="PublicKeyAlgorithm"/> for the specified
		/// <see cref="Org.BouncyCastle.Bcpg.PublicKeyAlgorithmTag"/>.
		/// </summary>
		/// <remarks>
		/// Gets the equivalent <see cref="PublicKeyAlgorithm"/> for the specified
		/// <see cref="Org.BouncyCastle.Bcpg.PublicKeyAlgorithmTag"/>.
		/// </remarks>
		/// <returns>The public-key algorithm.</returns>
		/// <param name="algorithm">The public-key algorithm.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="algorithm"/> is out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <paramref name="algorithm"/> does not have an equivalent <see cref="PublicKeyAlgorithm"/> value.
		/// </exception>
		public static PublicKeyAlgorithm GetPublicKeyAlgorithm (PublicKeyAlgorithmTag algorithm)
		{
			switch (algorithm) {
			case PublicKeyAlgorithmTag.RsaGeneral:     return PublicKeyAlgorithm.RsaGeneral;
			case PublicKeyAlgorithmTag.RsaEncrypt:     return PublicKeyAlgorithm.RsaEncrypt;
			case PublicKeyAlgorithmTag.RsaSign:        return PublicKeyAlgorithm.RsaSign;
			case PublicKeyAlgorithmTag.ElGamalEncrypt: return PublicKeyAlgorithm.ElGamalEncrypt;
			case PublicKeyAlgorithmTag.Dsa:            return PublicKeyAlgorithm.Dsa;
			case PublicKeyAlgorithmTag.EC:             return PublicKeyAlgorithm.EllipticCurve;
			case PublicKeyAlgorithmTag.ECDsa:          return PublicKeyAlgorithm.EllipticCurveDsa;
			case PublicKeyAlgorithmTag.ElGamalGeneral: return PublicKeyAlgorithm.ElGamalGeneral;
			case PublicKeyAlgorithmTag.DiffieHellman:  return PublicKeyAlgorithm.DiffieHellman;
			default: throw new ArgumentOutOfRangeException ("algorithm");
			}
		}

		DigitalSignatureCollection GetDigitalSignatures (PgpSignatureList signatureList, Stream content)
		{
			var signatures = new List<IDigitalSignature> ();
			var buf = new byte[4096];
			int nread;

			for (int i = 0; i < signatureList.Count; i++) {
				var pubkey = PublicKeyRingBundle.GetPublicKey (signatureList[i].KeyId);
				var signature = new OpenPgpDigitalSignature (pubkey, signatureList[i]) {
					PublicKeyAlgorithm = GetPublicKeyAlgorithm (signatureList[i].KeyAlgorithm),
					DigestAlgorithm = GetDigestAlgorithm (signatureList[i].HashAlgorithm),
					CreationDate = signatureList[i].CreationTime,
				};

				if (pubkey != null)
					signatureList[i].InitVerify (pubkey);

				signatures.Add (signature);
			}

			var memory = content as MemoryStream;
			if (memory != null) {
				// We can optimize things a bit if we've got a memory stream...
				var buffer = memory.GetBuffer ();

				for (int index = (int) memory.Position; index < (int) memory.Length; index++) {
					byte c = buffer[index];

					for (int i = 0; i < signatures.Count; i++) {
						if (signatures[i].SignerCertificate != null) {
							var pgp = (OpenPgpDigitalSignature) signatures[i];
							pgp.Signature.Update (c);
						}
					}
				}
			} else {
				while ((nread = content.Read (buf, 0, buf.Length)) > 0) {
					for (int i = 0; i < signatures.Count; i++) {
						if (signatures[i].SignerCertificate != null) {
							var pgp = (OpenPgpDigitalSignature) signatures[i];
							pgp.Signature.Update (buf, 0, nread);
						}
					}
				}
			}

			return new DigitalSignatureCollection (signatures);
		}

		/// <summary>
		/// Verifies the specified content using the detached signatureData.
		/// </summary>
		/// <remarks>
		/// Verifies the specified content using the detached signatureData.
		/// </remarks>
		/// <returns>A list of digital signatures.</returns>
		/// <param name="content">The content.</param>
		/// <param name="signatureData">The signature data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signatureData"/> is <c>null</c>.</para>
		/// </exception>
		public override DigitalSignatureCollection Verify (Stream content, Stream signatureData)
		{
			if (content == null)
				throw new ArgumentNullException ("content");

			if (signatureData == null)
				throw new ArgumentNullException ("signatureData");

			using (var armored = new ArmoredInputStream (signatureData)) {
				var factory = new PgpObjectFactory (armored);
				var data = factory.NextPgpObject ();
				PgpSignatureList signatureList;

				var compressed = data as PgpCompressedData;
				if (compressed != null) {
					factory = new PgpObjectFactory (compressed.GetDataStream ());
					data = factory.NextPgpObject ();
				}

				signatureList = (PgpSignatureList) data;

				return GetDigitalSignatures (signatureList, content);
			}
		}

		/// <summary>
		/// Encrypts the specified content for the specified recipients.
		/// </summary>
		/// <remarks>
		/// Encrypts the specified content for the specified recipients.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.MimePart"/> instance
		/// containing the encrypted data.</returns>
		/// <param name="recipients">The recipients.</param>
		/// <param name="content">The content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para>One or more of the recipient keys cannot be used for encrypting.</para>
		/// <para>-or-</para>
		/// <para>No recipients were specified.</para>
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// A public key could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		public override MimePart Encrypt (IEnumerable<MailboxAddress> recipients, Stream content)
		{
			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (content == null)
				throw new ArgumentNullException ("content");

			// TODO: document the exceptions that can be thrown by BouncyCastle
			return Encrypt (GetPublicKeys (recipients), content);
		}

		static Stream Compress (Stream content)
		{
			var compresser = new PgpCompressedDataGenerator (CompressionAlgorithmTag.ZLib);
			var memory = new MemoryBlockStream ();

			using (var compressed = compresser.Open (memory)) {
				var literalGenerator = new PgpLiteralDataGenerator ();

				using (var literal = literalGenerator.Open (compressed, 't', "mime.txt", content.Length, DateTime.Now)) {
					content.CopyTo (literal, 4096);
					literal.Flush ();
				}

				compressed.Flush ();
			}

			memory.Position = 0;

			return memory;
		}

		static Stream Encrypt (PgpEncryptedDataGenerator encrypter, Stream content)
		{
			var memory = new MemoryBlockStream ();

			using (var armored = new ArmoredOutputStream (memory)) {
				using (var compressed = Compress (content)) {
					using (var encrypted = encrypter.Open (armored, compressed.Length)) {
						compressed.CopyTo (encrypted, 4096);
						encrypted.Flush ();
					}
				}

				armored.Flush ();
			}

			memory.Position = 0;

			return memory;
		}

		static SymmetricKeyAlgorithmTag GetSymmetricKeyAlgorithm (EncryptionAlgorithm algorithm)
		{
			switch (algorithm) {
			case EncryptionAlgorithm.Aes128:      return SymmetricKeyAlgorithmTag.Aes128;
			case EncryptionAlgorithm.Aes192:      return SymmetricKeyAlgorithmTag.Aes192;
			case EncryptionAlgorithm.Aes256:      return SymmetricKeyAlgorithmTag.Aes256;
			case EncryptionAlgorithm.Camellia128: return SymmetricKeyAlgorithmTag.Camellia128;
			case EncryptionAlgorithm.Camellia192: return SymmetricKeyAlgorithmTag.Camellia192;
			case EncryptionAlgorithm.Camellia256: return SymmetricKeyAlgorithmTag.Camellia256;
			case EncryptionAlgorithm.Cast5:       return SymmetricKeyAlgorithmTag.Cast5;
			case EncryptionAlgorithm.Des:         return SymmetricKeyAlgorithmTag.Des;
			case EncryptionAlgorithm.TripleDes:   return SymmetricKeyAlgorithmTag.TripleDes;
			case EncryptionAlgorithm.Idea:        return SymmetricKeyAlgorithmTag.Idea;
			case EncryptionAlgorithm.Blowfish:    return SymmetricKeyAlgorithmTag.Blowfish;
			case EncryptionAlgorithm.Twofish:     return SymmetricKeyAlgorithmTag.Twofish;
			default: throw new NotSupportedException ();
			}
		}

		/// <summary>
		/// Encrypts the specified content for the specified recipients.
		/// </summary>
		/// <remarks>
		/// Encrypts the specified content for the specified recipients.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.MimePart"/> instance
		/// containing the encrypted data.</returns>
		/// <param name="algorithm">The encryption algorithm.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="content">The content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para>One or more of the recipient keys cannot be used for encrypting.</para>
		/// <para>-or-</para>
		/// <para>No recipients were specified.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The specified encryption algorithm is not supported.
		/// </exception>
		public MimePart Encrypt (EncryptionAlgorithm algorithm, IEnumerable<PgpPublicKey> recipients, Stream content)
		{
			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (content == null)
				throw new ArgumentNullException ("content");

			var encrypter = new PgpEncryptedDataGenerator (GetSymmetricKeyAlgorithm (algorithm), true);
			int count = 0;

			foreach (var recipient in recipients) {
				if (!recipient.IsEncryptionKey)
					throw new ArgumentException ("One or more of the recipient keys cannot be used for encrypting.", "recipients");

				encrypter.AddMethod (recipient);
				count++;
			}

			if (count == 0)
				throw new ArgumentException ("No recipients specified.", "recipients");

			var encrypted = Encrypt (encrypter, content);

			return new MimePart ("application", "octet-stream") {
				ContentDisposition = new ContentDisposition ("attachment"),
				ContentObject = new ContentObject (encrypted),
			};
		}

		/// <summary>
		/// Encrypts the specified content for the specified recipients.
		/// </summary>
		/// <remarks>
		/// Encrypts the specified content for the specified recipients.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.MimePart"/> instance
		/// containing the encrypted data.</returns>
		/// <param name="recipients">The recipients.</param>
		/// <param name="content">The content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para>One or more of the recipient keys cannot be used for encrypting.</para>
		/// <para>-or-</para>
		/// <para>No recipients were specified.</para>
		/// </exception>
		public MimePart Encrypt (IEnumerable<PgpPublicKey> recipients, Stream content)
		{
			return Encrypt (defaultAlgorithm, recipients, content);
		}

		/// <summary>
		/// Cryptographically signs and encrypts the specified content for the specified recipients.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs and encrypts the specified content for the specified recipients.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.MimePart"/> instance
		/// containing the encrypted data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="content">The content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="digestAlgo"/> is out of range.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para>One or more of the recipient keys cannot be used for encrypting.</para>
		/// <para>-or-</para>
		/// <para>No recipients were specified.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The specified <see cref="DigestAlgorithm"/> is not supported by this context.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found for <paramref name="signer"/>.
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// A public key could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The user chose to cancel the password prompt.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public MimePart SignAndEncrypt (MailboxAddress signer, DigestAlgorithm digestAlgo, IEnumerable<MailboxAddress> recipients, Stream content)
		{
			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (content == null)
				throw new ArgumentNullException ("content");

			var key = GetSigningKey (signer);

			return SignAndEncrypt (key, digestAlgo, GetPublicKeys (recipients), content);
		}

		/// <summary>
		/// Cryptographically signs and encrypts the specified content for the specified recipients.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs and encrypts the specified content for the specified recipients.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.MimePart"/> instance
		/// containing the encrypted data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="cipherAlgo">The encryption algorithm.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="content">The content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="signer"/> cannot be used for signing.</para>
		/// <para>-or-</para>
		/// <para>One or more of the recipient keys cannot be used for encrypting.</para>
		/// <para>-or-</para>
		/// <para>No recipients were specified.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The specified encryption algorithm is not supported.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The user chose to cancel the password prompt.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public MimePart SignAndEncrypt (PgpSecretKey signer, DigestAlgorithm digestAlgo, EncryptionAlgorithm cipherAlgo, IEnumerable<PgpPublicKey> recipients, Stream content)
		{
			// TODO: document the exceptions that can be thrown by BouncyCastle

			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (!signer.IsSigningKey)
				throw new ArgumentException ("The specified secret key cannot be used for signing.", "signer");

			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (content == null)
				throw new ArgumentNullException ("content");

			var encrypter = new PgpEncryptedDataGenerator (GetSymmetricKeyAlgorithm (cipherAlgo), true);
			var hashAlgorithm = GetHashAlgorithm (digestAlgo);
			int count = 0;

			foreach (var recipient in recipients) {
				if (!recipient.IsEncryptionKey)
					throw new ArgumentException ("One or more of the recipient keys cannot be used for encrypting.", "recipients");

				encrypter.AddMethod (recipient);
				count++;
			}

			if (count == 0)
				throw new ArgumentException ("No recipients specified.", "recipients");

			var compresser = new PgpCompressedDataGenerator (CompressionAlgorithmTag.ZLib);

			using (var compressed = new MemoryBlockStream ()) {
				using (var signed = compresser.Open (compressed)) {
					var signatureGenerator = new PgpSignatureGenerator (signer.PublicKey.Algorithm, hashAlgorithm);
					signatureGenerator.InitSign (PgpSignature.CanonicalTextDocument, GetPrivateKey (signer));
					var subpacket = new PgpSignatureSubpacketGenerator ();

					foreach (string userId in signer.PublicKey.GetUserIds ()) {
						subpacket.SetSignerUserId (false, userId);
						break;
					}

					signatureGenerator.SetHashedSubpackets (subpacket.Generate ());

					var onepass = signatureGenerator.GenerateOnePassVersion (false);
					onepass.Encode (signed);

					var literalGenerator = new PgpLiteralDataGenerator ();
					using (var literal = literalGenerator.Open (signed, 't', "mime.txt", content.Length, DateTime.Now)) {
						var buf = new byte[4096];
						int nread;

						while ((nread = content.Read (buf, 0, buf.Length)) > 0) {
							signatureGenerator.Update (buf, 0, nread);
							literal.Write (buf, 0, nread);
						}

						literal.Flush ();
					}

					var signature = signatureGenerator.Generate ();
					signature.Encode (signed);

					signed.Flush ();
				}

				compressed.Position = 0;

				var memory = new MemoryBlockStream ();
				using (var armored = new ArmoredOutputStream (memory)) {
					using (var encrypted = encrypter.Open (armored, compressed.Length)) {
						compressed.CopyTo (encrypted, 4096);
						encrypted.Flush ();
					}

					armored.Flush ();
				}

				memory.Position = 0;

				return new MimePart ("application", "octet-stream") {
					ContentDisposition = new ContentDisposition ("attachment"),
					ContentObject = new ContentObject (memory)
				};
			}
		}

		/// <summary>
		/// Cryptographically signs and encrypts the specified content for the specified recipients.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs and encrypts the specified content for the specified recipients.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.MimePart"/> instance
		/// containing the encrypted data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="content">The content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="signer"/> cannot be used for signing.</para>
		/// <para>-or-</para>
		/// <para>One or more of the recipient keys cannot be used for encrypting.</para>
		/// <para>-or-</para>
		/// <para>No recipients were specified.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The user chose to cancel the password prompt.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public MimePart SignAndEncrypt (PgpSecretKey signer, DigestAlgorithm digestAlgo, IEnumerable<PgpPublicKey> recipients, Stream content)
		{
			return SignAndEncrypt (signer, digestAlgo, defaultAlgorithm, recipients, content);
		}

		/// <summary>
		/// Decrypts the specified encryptedData and extracts the digital signers if the content was also signed.
		/// </summary>
		/// <remarks>
		/// Decrypts the specified encryptedData and extracts the digital signers if the content was also signed.
		/// </remarks>
		/// <returns>The decrypted stream.</returns>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <param name="signatures">A list of digital signatures if the data was both signed and encrypted.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="encryptedData"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found to decrypt the stream.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The user chose to cancel the password prompt.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Bcpg.OpenPgp.PgpException">
		/// An OpenPGP error occurred.
		/// </exception>
		public Stream GetDecryptedStream (Stream encryptedData, out DigitalSignatureCollection signatures)
		{
			if (encryptedData == null)
				throw new ArgumentNullException ("encryptedData");

			using (var armored = new ArmoredInputStream (encryptedData)) {
				var factory = new PgpObjectFactory (armored);
				var obj = factory.NextPgpObject ();
				var list = obj as PgpEncryptedDataList;

				if (list == null) {
					// probably a PgpMarker...
					obj = factory.NextPgpObject ();

					list = obj as PgpEncryptedDataList;

					if (list == null)
						throw new PgpException ("Unexpected OpenPGP packet.");
				}

				PgpPublicKeyEncryptedData encrypted = null;
				PrivateKeyNotFoundException pkex = null;
				bool hasEncryptedPackets = false;
				PgpSecretKey secret = null;

				foreach (PgpEncryptedData data in list.GetEncryptedDataObjects ()) {
					if ((encrypted = data as PgpPublicKeyEncryptedData) == null)
						continue;

					hasEncryptedPackets = true;

					try {
						secret = GetSecretKey (encrypted.KeyId);
						break;
					} catch (PrivateKeyNotFoundException ex) {
						pkex = ex;
					}
				}

				if (!hasEncryptedPackets)
					throw new PgpException ("No encrypted packets found.");

				if (secret == null)
					throw pkex;

				factory = new PgpObjectFactory (encrypted.GetDataStream (GetPrivateKey (secret)));
				List<IDigitalSignature> onepassList = null;
				PgpSignatureList signatureList = null;
				PgpCompressedData compressed = null;
				var memory = new MemoryBlockStream ();

				obj = factory.NextPgpObject ();
				while (obj != null) {
					if (obj is PgpCompressedData) {
						if (compressed != null)
							throw new PgpException ("Recursive compression packets are not supported.");

						compressed = (PgpCompressedData) obj;
						factory = new PgpObjectFactory (compressed.GetDataStream ());
					} else if (obj is PgpOnePassSignatureList) {
						if (memory.Length == 0) {
							var onepasses = (PgpOnePassSignatureList) obj;

							onepassList = new List<IDigitalSignature> ();

							for (int i = 0; i < onepasses.Count; i++) {
								var onepass = onepasses[i];
								var pubkey = PublicKeyRingBundle.GetPublicKey (onepass.KeyId);

								if (pubkey == null) {
									// too messy, pretend we never found a one-pass signature list
									onepassList = null;
									break;
								}

								onepass.InitVerify (pubkey);

								var signature = new OpenPgpDigitalSignature (pubkey, onepass) {
									PublicKeyAlgorithm = GetPublicKeyAlgorithm (onepass.KeyAlgorithm),
									DigestAlgorithm = GetDigestAlgorithm (onepass.HashAlgorithm),
								};

								onepassList.Add (signature);
							}
						}
					} else if (obj is PgpSignatureList) {
						signatureList = (PgpSignatureList) obj;
					} else if (obj is PgpLiteralData) {
						var literal = (PgpLiteralData) obj;

						using (var stream = literal.GetDataStream ()) {
							var buffer = new byte[4096];
							int nread;

							while ((nread = stream.Read (buffer, 0, buffer.Length)) > 0) {
								if (onepassList != null) {
									// update our one-pass signatures...
									for (int index = 0; index < nread; index++) {
										byte c = buffer[index];

										for (int i = 0; i < onepassList.Count; i++) {
											var pgp = (OpenPgpDigitalSignature) onepassList[i];
											pgp.OnePassSignature.Update (c);
										}
									}
								}

								memory.Write (buffer, 0, nread);
							}
						}
					}

					obj = factory.NextPgpObject ();
				}

				memory.Position = 0;

				if (signatureList != null) {
					if (onepassList != null && signatureList.Count == onepassList.Count) {
						for (int i = 0; i < onepassList.Count; i++) {
							var pgp = (OpenPgpDigitalSignature) onepassList[i];
							pgp.CreationDate = signatureList[i].CreationTime;
							pgp.Signature = signatureList[i];
						}

						signatures = new DigitalSignatureCollection (onepassList);
					} else {
						signatures = GetDigitalSignatures (signatureList, memory);
						memory.Position = 0;
					}
				} else {
					signatures = null;
				}

				return memory;
			}
		}

		/// <summary>
		/// Decrypts the specified encryptedData.
		/// </summary>
		/// <remarks>
		/// Decrypts the specified encryptedData.
		/// </remarks>
		/// <returns>The decrypted stream.</returns>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="encryptedData"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found to decrypt the stream.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The user chose to cancel the password prompt.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Bcpg.OpenPgp.PgpException">
		/// An OpenPGP error occurred.
		/// </exception>
		public Stream GetDecryptedStream (Stream encryptedData)
		{
			DigitalSignatureCollection signatures;

			if (encryptedData == null)
				throw new ArgumentNullException ("encryptedData");

			return GetDecryptedStream (encryptedData, out signatures);
		}

		/// <summary>
		/// Decrypts the specified encryptedData and extracts the digital signers if the content was also signed.
		/// </summary>
		/// <remarks>
		/// Decrypts the specified encryptedData and extracts the digital signers if the content was also signed.
		/// </remarks>
		/// <returns>The decrypted <see cref="MimeKit.MimeEntity"/>.</returns>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <param name="signatures">A list of digital signatures if the data was both signed and encrypted.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="encryptedData"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found to decrypt the stream.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The user chose to cancel the password prompt.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Bcpg.OpenPgp.PgpException">
		/// An OpenPGP error occurred.
		/// </exception>
		public MimeEntity Decrypt (Stream encryptedData, out DigitalSignatureCollection signatures)
		{
			if (encryptedData == null)
				throw new ArgumentNullException ("encryptedData");

			var decrypted = GetDecryptedStream (encryptedData, out signatures);

			return MimeEntity.Load (decrypted, true);
		}

		/// <summary>
		/// Decrypts the specified encryptedData.
		/// </summary>
		/// <remarks>
		/// Decrypts the specified encryptedData.
		/// </remarks>
		/// <returns>The decrypted <see cref="MimeKit.MimeEntity"/>.</returns>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="encryptedData"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found to decrypt the stream.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The user chose to cancel the password prompt.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Bcpg.OpenPgp.PgpException">
		/// An OpenPGP error occurred.
		/// </exception>
		public override MimeEntity Decrypt (Stream encryptedData)
		{
			DigitalSignatureCollection signatures;

			if (encryptedData == null)
				throw new ArgumentNullException ("encryptedData");

			return Decrypt (encryptedData, out signatures);
		}

		/// <summary>
		/// Saves the public key-ring bundle.
		/// </summary>
		/// <remarks>
		/// <para>Atomically saves the public key-ring bundle to the path specified by <see cref="PublicKeyRingPath"/>.</para>
		/// <para>Called by <see cref="Import"/> if any public keys were successfully imported.</para>
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

			if (!Directory.Exists (dirname))
				Directory.CreateDirectory (dirname);

			using (var file = File.OpenWrite (tmp)) {
				PublicKeyRingBundle.Encode (file);
				file.Flush ();
			}

			if (File.Exists (PublicKeyRingPath))
				File.Replace (tmp, PublicKeyRingPath, bak);
			else
				File.Move (tmp, PublicKeyRingPath);
		}

		/// <summary>
		/// Saves the secret key-ring bundle.
		/// </summary>
		/// <remarks>
		/// <para>Atomically saves the secret key-ring bundle to the path specified by <see cref="SecretKeyRingPath"/>.</para>
		/// <para>Called by <see cref="ImportSecretKeys"/> if any secret keys were successfully imported.</para>
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

			if (!Directory.Exists (dirname))
				Directory.CreateDirectory (dirname);

			using (var file = File.OpenWrite (tmp)) {
				SecretKeyRingBundle.Encode (file);
				file.Flush ();
			}

			if (File.Exists (SecretKeyRingPath))
				File.Replace (tmp, SecretKeyRingPath, bak);
			else
				File.Move (tmp, SecretKeyRingPath);
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
				throw new ArgumentNullException ("stream");

			using (var armored = new ArmoredInputStream (stream)) {
				var imported = new PgpPublicKeyRingBundle (armored);
				if (imported.Count == 0)
					return;

				int publicKeysAdded = 0;

				foreach (PgpPublicKeyRing pubring in imported.GetKeyRings ()) {
					if (!PublicKeyRingBundle.Contains (pubring.GetPublicKey ().KeyId)) {
						PublicKeyRingBundle = PgpPublicKeyRingBundle.AddPublicKeyRing (PublicKeyRingBundle, pubring);
						publicKeysAdded++;
					}
				}

				if (publicKeysAdded > 0)
					SavePublicKeyRingBundle ();
			}
		}

		/// <summary>
		/// Imports secret pgp keys from the specified stream.
		/// </summary>
		/// <remarks>
		/// Imports secret pgp keys from the specified stream.
		/// </remarks>
		/// <param name="rawData">The raw key data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="rawData"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// <para>An error occurred while parsing the raw key-ring data</para>
		/// <para>-or-</para>
		/// <para>An error occured while saving the public key-ring bundle.</para>
		/// </exception>
		public virtual void ImportSecretKeys (Stream rawData)
		{
			if (rawData == null)
				throw new ArgumentNullException ("rawData");

			using (var armored = new ArmoredInputStream (rawData)) {
				var imported = new PgpSecretKeyRingBundle (armored);
				if (imported.Count == 0)
					return;

				int secretKeysAdded = 0;

				foreach (PgpSecretKeyRing secring in imported.GetKeyRings ()) {
					if (!SecretKeyRingBundle.Contains (secring.GetSecretKey ().KeyId)) {
						SecretKeyRingBundle = PgpSecretKeyRingBundle.AddSecretKeyRing (SecretKeyRingBundle, secring);
						secretKeysAdded++;
					}
				}

				if (secretKeysAdded > 0)
					SaveSecretKeyRingBundle ();
			}
		}

		/// <summary>
		/// Exports the public keys for the specified mailboxes.
		/// </summary>
		/// <remarks>
		/// Exports the public keys for the specified mailboxes.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.MimePart"/> instance containing the exported public keys.</returns>
		/// <param name="mailboxes">The mailboxes.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailboxes"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="mailboxes"/> was empty.
		/// </exception>
		public override MimePart Export (IEnumerable<MailboxAddress> mailboxes)
		{
			if (mailboxes == null)
				throw new ArgumentNullException ("mailboxes");

			return Export (GetPublicKeys (mailboxes));
		}

		/// <summary>
		/// Exports the specified public keys.
		/// </summary>
		/// <remarks>
		/// Exports the specified public keys.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.MimePart"/> instance containing the exported public keys.</returns>
		/// <param name="keys">The keys.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="keys"/> is <c>null</c>.
		/// </exception>
		public MimePart Export (IEnumerable<PgpPublicKey> keys)
		{
			if (keys == null)
				throw new ArgumentNullException ("keys");

			var keyrings = keys.Select (key => new PgpPublicKeyRing (key.GetEncoded ()));
			var bundle = new PgpPublicKeyRingBundle (keyrings);

			return Export (bundle);
		}

		/// <summary>
		/// Exports the specified public keys.
		/// </summary>
		/// <remarks>
		/// Exports the specified public keys.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.MimePart"/> instance containing the exported public keys.</returns>
		/// <param name="keys">The keys.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="keys"/> is <c>null</c>.
		/// </exception>
		public MimePart Export (PgpPublicKeyRingBundle keys)
		{
			if (keys == null)
				throw new ArgumentNullException ("keys");

			var content = new MemoryBlockStream ();

			using (var armored = new ArmoredOutputStream (content)) {
				keys.Encode (armored);
				armored.Flush ();
			}

			content.Position = 0;

			return new MimePart ("application", "pgp-keys") {
				ContentDisposition = new ContentDisposition ("attachment"),
				ContentObject = new ContentObject (content)
			};
		}
	}
}

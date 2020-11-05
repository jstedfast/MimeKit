//
// OpenPgpContext.cs
//
// Authors: Jeffrey Stedfast <jestedfa@microsoft.com>
//          Thomas Hansen <thomas@servergardens.com>
//
// Copyright (c) 2013-2020 .NET Foundation and Contributors
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
using System.Net;
using System.Text;
using System.Buffers;
using System.Net.Http;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;

using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;

using MimeKit.IO;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An abstract OpenPGP cryptography context which can be used for PGP/MIME.
	/// </summary>
	/// <remarks>
	/// Generally speaking, applications should not use a <see cref="OpenPgpContextBase"/>
	/// directly, but rather via higher level APIs such as <see cref="MultipartSigned"/>
	/// and <see cref="MultipartEncrypted"/>.
	/// </remarks>
	public abstract class OpenPgpContextBase : CryptographyContext
	{
		static readonly string[] ProtocolSubtypes = { "pgp-signature", "pgp-encrypted", "pgp-keys", "x-pgp-signature", "x-pgp-encrypted", "x-pgp-keys" };
		const string BeginPublicKeyBlock = "-----BEGIN PGP PUBLIC KEY BLOCK-----";
		const string EndPublicKeyBlock = "-----END PGP PUBLIC KEY BLOCK-----";
		const int BufferLength = 4096;

		static readonly EncryptionAlgorithm[] DefaultEncryptionAlgorithmRank = {
			EncryptionAlgorithm.Idea,
			EncryptionAlgorithm.TripleDes,
			EncryptionAlgorithm.Cast5,
			EncryptionAlgorithm.Blowfish,
			EncryptionAlgorithm.Aes128,
			EncryptionAlgorithm.Aes192,
			EncryptionAlgorithm.Aes256,
			EncryptionAlgorithm.Twofish,
			EncryptionAlgorithm.Camellia128,
			EncryptionAlgorithm.Camellia192,
			EncryptionAlgorithm.Camellia256
		};

		static readonly DigestAlgorithm[] DefaultDigestAlgorithmRank = {
			DigestAlgorithm.Sha1,
			DigestAlgorithm.RipeMD160,
			DigestAlgorithm.Sha256,
			DigestAlgorithm.Sha384,
			DigestAlgorithm.Sha512,
			DigestAlgorithm.Sha224
		};

		EncryptionAlgorithm defaultAlgorithm;
		HttpClient client;
		Uri keyServer;

		/// <summary>
		/// Initialize a new instance of the <see cref="OpenPgpContextBase"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="OpenPgpContextBase"/>.
		/// </remarks>
		protected OpenPgpContextBase ()
		{
			EncryptionAlgorithmRank = DefaultEncryptionAlgorithmRank;
			DigestAlgorithmRank = DefaultDigestAlgorithmRank;

			foreach (var algorithm in EncryptionAlgorithmRank)
				Enable (algorithm);

			foreach (var algorithm in DigestAlgorithmRank)
				Enable (algorithm);

			defaultAlgorithm = EncryptionAlgorithm.Cast5;

			client = new HttpClient ();
		}

		/// <summary>
		/// Get the password for a secret key.
		/// </summary>
		/// <remarks>
		/// Gets the password for a secret key.
		/// </remarks>
		/// <returns>The password for the secret key.</returns>
		/// <param name="key">The secret key.</param>
		/// <exception cref="System.OperationCanceledException">
		/// The user chose to cancel the password request.
		/// </exception>
		protected abstract string GetPasswordForKey (PgpSecretKey key); // FIXME: rename this to GetPassword() in the future

		/// <summary>
		/// Get the public keyring that contains the specified key.
		/// </summary>
		/// <remarks>
		/// <para>Gets the public keyring that contains the specified key.</para>
		/// <note type="note">Implementations should first try to obtain the keyring stored (or cached) locally.
		/// Failing that, if <see cref="AutoKeyRetrieve"/> is enabled, they should use
		/// <see cref="RetrievePublicKeyRing(long, CancellationToken)"/> to attempt to
		/// retrieve the keyring from the configured <see cref="KeyServer"/>.</note>
		/// </remarks>
		/// <param name="keyId">The public key identifier.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The public keyring that contains the specified key or <c>null</c> if the keyring could not be found.</returns>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled.
		/// </exception>
		protected abstract PgpPublicKeyRing GetPublicKeyRing (long keyId, CancellationToken cancellationToken);

		/// <summary>
		/// Get the public keyring that contains the specified key asynchronously.
		/// </summary>
		/// <remarks>
		/// <para>Gets the public keyring that contains the specified key.</para>
		/// <note type="note">Implementations should first try to obtain the keyring stored (or cached) locally.
		/// Failing that, if <see cref="AutoKeyRetrieve"/> is enabled, they should use
		/// <see cref="RetrievePublicKeyRingAsync(long, CancellationToken)"/> to attempt to
		/// retrieve the keyring from the configured <see cref="KeyServer"/>.</note>
		/// </remarks>
		/// <param name="keyId">The public key identifier.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The public keyring that contains the specified key or <c>null</c> if the keyring could not be found.</returns>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled.
		/// </exception>
		protected abstract Task<PgpPublicKeyRing> GetPublicKeyRingAsync (long keyId, CancellationToken cancellationToken);

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
		protected abstract PgpSecretKey GetSecretKey (long keyId);

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
		public abstract IList<PgpPublicKey> GetPublicKeys (IEnumerable<MailboxAddress> mailboxes);

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
		/// A secret key for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		public abstract PgpSecretKey GetSigningKey (MailboxAddress mailbox);

		/// <summary>
		/// Get or set the default encryption algorithm.
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

		bool IsValidKeyServer {
			get {
				if (keyServer == null)
					return false;

				switch (keyServer.Scheme.ToLowerInvariant ()) {
				case "https": case "http": case "hkp": return true;
				default: return false;
				}
			}
		}

		/// <summary>
		/// Get or set the key server to use when automatically retrieving keys.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the key server to use when verifying keys that are
		/// not already in the public keychain.</para>
		/// <note type="note">Only HTTP and HKP protocols are supported.</note>
		/// </remarks>
		/// <value>The key server.</value>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="value"/> is not an absolute URI.
		/// </exception>
		public Uri KeyServer {
			get { return keyServer; }
			set {
				if (value != null && !value.IsAbsoluteUri)
					throw new ArgumentException ("The key server URI must be absolute.", nameof (value));

				keyServer = value;
			}
		}

		/// <summary>
		/// Get or set whether unknown PGP keys should automtically be retrieved.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets whether or not the <see cref="OpenPgpContext"/> should automatically
		/// fetch keys as needed from the keyserver when verifying signatures.</para>
		/// <note type="note">Requires a valid <see cref="KeyServer"/> to be set.</note>
		/// </remarks>
		/// <value><c>true</c> if unknown PGP keys should automatically be retrieved; otherwise, <c>false</c>.</value>
		public bool AutoKeyRetrieve {
			get; set;
		}

		/// <summary>
		/// Get the signature protocol.
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
		/// Get the encryption protocol.
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
		/// Get the key exchange protocol.
		/// </summary>
		/// <remarks>
		/// Gets the key exchange protocol.
		/// </remarks>
		/// <value>The key exchange protocol.</value>
		public override string KeyExchangeProtocol {
			get { return "application/pgp-keys"; }
		}

		/// <summary>
		/// Check whether or not the specified protocol is supported.
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
				throw new ArgumentNullException (nameof (protocol));

			if (!protocol.StartsWith ("application/", StringComparison.OrdinalIgnoreCase))
				return false;

			int startIndex = "application/".Length;
			int subtypeLength = protocol.Length - startIndex;

			for (int i = 0; i < ProtocolSubtypes.Length; i++) {
				if (subtypeLength != ProtocolSubtypes[i].Length)
					continue;

				if (string.Compare (protocol, startIndex, ProtocolSubtypes[i], 0, subtypeLength, StringComparison.OrdinalIgnoreCase) == 0)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Get the string name of the digest algorithm for use with the micalg parameter of a multipart/signed part.
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
			default: throw new ArgumentOutOfRangeException (nameof (micalg));
			}
		}

		/// <summary>
		/// Get the digest algorithm from the micalg parameter value in a multipart/signed part.
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
				throw new ArgumentNullException (nameof (micalg));

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
		/// Hex encode an array of bytes.
		/// </summary>
		/// <remarks>
		/// This method is used to hex-encode the PGP key fingerprints.
		/// </remarks>
		/// <param name="data">The data to encode.</param>
		/// <returns>A string representing the hex-encoded data.</returns>
		static string HexEncode (byte[] data)
		{
			var fingerprint = new StringBuilder ();

			for (int i = 0; i < data.Length; i++)
				fingerprint.Append (data[i].ToString ("x2"));

			return fingerprint.ToString ();
		}

		/// <summary>
		/// Check that a public key is a match for the specified mailbox.
		/// </summary>
		/// <remarks>
		/// <para>Checks that the public key is a match for the specified mailbox.</para>
		/// <para>If the <paramref name="mailbox"/> is a <see cref="SecureMailboxAddress"/> with a non-empty
		/// <see cref="SecureMailboxAddress.Fingerprint"/>, then the fingerprint is used to match the key's
		/// fingerprint. Otherwise, the email address(es) contained within the key's user identifier strings
		/// are compared to the mailbox address.</para>
		/// </remarks>
		/// <param name="key">The public key.</param>
		/// <param name="mailbox">The mailbox address.</param>
		/// <returns><c>true</c> if the key is a match for the specified mailbox; otherwise, <c>false</c>.</returns>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="key"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="mailbox"/> is <c>null</c>.</para>
		/// </exception>
		protected static bool IsMatch (PgpPublicKey key, MailboxAddress mailbox)
		{
			if (key == null)
				throw new ArgumentNullException (nameof (key));

			if (mailbox == null)
				throw new ArgumentNullException (nameof (mailbox));

			if (mailbox is SecureMailboxAddress secure && !string.IsNullOrEmpty (secure.Fingerprint)) {
				if (secure.Fingerprint.Length > 16) {
					var fingerprint = HexEncode (key.GetFingerprint ());

					return secure.Fingerprint.Equals (fingerprint, StringComparison.OrdinalIgnoreCase);
				}

				var id = ((int) key.KeyId).ToString ("X2");

				return secure.Fingerprint.EndsWith (id, StringComparison.OrdinalIgnoreCase);
			}

			foreach (string userId in key.GetUserIds ()) {
				if (!MailboxAddress.TryParse (userId, out var email))
					continue;

				if (mailbox.Address.Equals (email.Address, StringComparison.OrdinalIgnoreCase))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Check that a secret key is a match for the specified mailbox.
		/// </summary>
		/// <remarks>
		/// <para>Checks that the secret key is a match for the specified mailbox.</para>
		/// <para>If the <paramref name="mailbox"/> is a <see cref="SecureMailboxAddress"/> with a non-empty
		/// <see cref="SecureMailboxAddress.Fingerprint"/>, then the fingerprint is used to match the key's
		/// fingerprint. Otherwise, the email address(es) contained within the key's user identifier strings
		/// are compared to the mailbox address.</para>
		/// </remarks>
		/// <param name="key">The secret key.</param>
		/// <param name="mailbox">The mailbox address.</param>
		/// <returns><c>true</c> if the key is a match for the specified mailbox; otherwise, <c>false</c>.</returns>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="key"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="mailbox"/> is <c>null</c>.</para>
		/// </exception>
		protected static bool IsMatch (PgpSecretKey key, MailboxAddress mailbox)
		{
			if (key == null)
				throw new ArgumentNullException (nameof (key));

			if (mailbox == null)
				throw new ArgumentNullException (nameof (mailbox));

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
		/// Check if a public key is expired.
		/// </summary>
		/// <remarks>
		/// Checks if a public key is expired.
		/// </remarks>
		/// <param name="key">The public key.</param>
		/// <returns><c>true</c> if the public key is expired; otherwise, <c>false</c>.</returns>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		protected static bool IsExpired (PgpPublicKey key)
		{
			if (key == null)
				throw new ArgumentNullException (nameof (key));

			long seconds = key.GetValidSeconds ();

			if (seconds != 0) {
				var expires = key.CreationTime.AddSeconds ((double) seconds);
				if (expires <= DateTime.Now)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Retrieves the public keyring, using the preferred key server, automatically importing it afterwards.
		/// </summary>
		/// <param name="keyId">The identifier of the key to be retrieved.</param>
		/// <param name="doAsync"><c>true</c> if this operation should be done asynchronously; otherweise, <c>false</c>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The public key ring.</returns>
		async Task<PgpPublicKeyRing> RetrievePublicKeyRingAsync (long keyId, bool doAsync, CancellationToken cancellationToken)
		{
			if (!IsValidKeyServer)
				return null;

			var scheme = keyServer.Scheme.ToLowerInvariant ();
			var uri = new UriBuilder ();

			uri.Scheme = scheme == "hkp" ? "http" : scheme;
			uri.Host = keyServer.Host;

			if (keyServer.IsDefaultPort) {
				if (scheme == "hkp")
					uri.Port = 11371;
			} else {
				uri.Port = keyServer.Port;
			}

			uri.Path = "/pks/lookup";
			uri.Query = string.Format (CultureInfo.InvariantCulture, "op=get&search=0x{0:X}", keyId);

			using (var stream = new MemoryBlockStream ()) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (new OpenPgpBlockFilter (BeginPublicKeyBlock, EndPublicKeyBlock));

					if (doAsync) {
						using (var response = await client.GetAsync (uri.ToString (), cancellationToken).ConfigureAwait (false))
							await response.Content.CopyToAsync (filtered).ConfigureAwait (false);
					} else {
#if !NETSTANDARD1_3 && !NETSTANDARD1_6
						var request = (HttpWebRequest) WebRequest.Create (uri.ToString ());
						using (var response = request.GetResponse ()) {
							var content = response.GetResponseStream ();
							content.CopyTo (filtered, 4096);
						}
#else
						using (var response = client.GetAsync (uri.ToString (), cancellationToken).GetAwaiter ().GetResult ())
							response.Content.CopyToAsync (filtered).GetAwaiter ().GetResult ();
#endif
					}

					filtered.Flush ();
				}

				stream.Position = 0;

				using (var armored = new ArmoredInputStream (stream, true)) {
					var bundle = new PgpPublicKeyRingBundle (armored);

					Import (bundle);

					return bundle.GetPublicKeyRing (keyId);
				}
			}
		}

		/// <summary>
		/// Retrieve the public keyring using the configured key server.
		/// </summary>
		/// <remarks>
		/// <para>Retrieves the public keyring specified by the <paramref name="keyId"/> from the key server
		/// set on the <see cref="KeyServer"/> property. If the keyring is successfully retrieved, it will
		/// be imported via <see cref="Import(PgpPublicKeyRingBundle)"/>.</para>
		/// <para>This method should be called by <see cref="GetPublicKeyRing(long, CancellationToken)"/>
		/// when the keyring is not available locally.</para>
		/// </remarks>
		/// <param name="keyId">The identifier of the public key to be retrieved.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The public key ring.</returns>
		protected PgpPublicKeyRing RetrievePublicKeyRing (long keyId, CancellationToken cancellationToken)
		{
			return RetrievePublicKeyRingAsync (keyId, false, cancellationToken).GetAwaiter ().GetResult ();
		}

		/// <summary>
		/// Asynchronously retrieve the public keyring using the configured key server.
		/// </summary>
		/// <remarks>
		/// <para>Retrieves the public keyring specified by the <paramref name="keyId"/> from the key server
		/// set on the <see cref="KeyServer"/> property. If the keyring is successfully retrieved, it will
		/// be imported via <see cref="Import(PgpPublicKeyRingBundle)"/>.</para>
		/// <para>This method should be called by <see cref="GetPublicKeyRingAsync(long, CancellationToken)"/>
		/// when the keyring is not available locally.</para>
		/// </remarks>
		/// <param name="keyId">The identifier of the public key to be retrieved.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The public key ring.</returns>
		protected Task<PgpPublicKeyRing> RetrievePublicKeyRingAsync (long keyId, CancellationToken cancellationToken)
		{
			return RetrievePublicKeyRingAsync (keyId, true, cancellationToken); 
		}

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
				throw new ArgumentNullException (nameof (key));

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
		/// <paramref name="digestAlgo"/> is not a supported digest algorithm.
		/// </exception>
		public static HashAlgorithmTag GetHashAlgorithm (DigestAlgorithm digestAlgo)
		{
			switch (digestAlgo) {
			case DigestAlgorithm.MD5:       return HashAlgorithmTag.MD5;
			case DigestAlgorithm.Sha1:      return HashAlgorithmTag.Sha1;
			case DigestAlgorithm.RipeMD160: return HashAlgorithmTag.RipeMD160;
			case DigestAlgorithm.DoubleSha: throw new NotSupportedException ("The Double SHA digest algorithm is not supported.");
			case DigestAlgorithm.MD2:       return HashAlgorithmTag.MD2;
			case DigestAlgorithm.Tiger192:  throw new NotSupportedException ("The Tiger-192 digest algorithm is not supported.");
			case DigestAlgorithm.Haval5160: throw new NotSupportedException ("The HAVAL 5 160 digest algorithm is not supported.");
			case DigestAlgorithm.Sha256:    return HashAlgorithmTag.Sha256;
			case DigestAlgorithm.Sha384:    return HashAlgorithmTag.Sha384;
			case DigestAlgorithm.Sha512:    return HashAlgorithmTag.Sha512;
			case DigestAlgorithm.Sha224:    return HashAlgorithmTag.Sha224;
			case DigestAlgorithm.MD4:       throw new NotSupportedException ("The MD4 digest algorithm is not supported.");
			default: throw new ArgumentOutOfRangeException (nameof (digestAlgo));
			}
		}

		/// <summary>
		/// Cryptographically signs the content.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs the content using the specified signer and digest algorithm.
		/// </remarks>
		/// <returns>A new <see cref="MimePart"/> instance
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
				throw new ArgumentNullException (nameof (signer));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var key = GetSigningKey (signer);

			return Sign (key, digestAlgo, content);
		}

		/// <summary>
		/// Cryptographically signs the content.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs the content using the specified signer and digest algorithm.
		/// </remarks>
		/// <returns>A new <see cref="MimePart"/> instance
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
				throw new ArgumentNullException (nameof (signer));

			if (!signer.IsSigningKey)
				throw new ArgumentException ("The specified secret key cannot be used for signing.", nameof (signer));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var hashAlgorithm = GetHashAlgorithm (digestAlgo);
			var memory = new MemoryBlockStream ();

			using (var armored = new ArmoredOutputStream (memory)) {
				armored.SetHeader ("Version", null);

				var compresser = new PgpCompressedDataGenerator (CompressionAlgorithmTag.ZLib);
				using (var compressed = compresser.Open (armored)) {
					var signatureGenerator = new PgpSignatureGenerator (signer.PublicKey.Algorithm, hashAlgorithm);
					var buf = ArrayPool<byte>.Shared.Rent (BufferLength);
					int nread;

					try {
						signatureGenerator.InitSign (PgpSignature.CanonicalTextDocument, GetPrivateKey (signer));

						while ((nread = content.Read (buf, 0, BufferLength)) > 0)
							signatureGenerator.Update (buf, 0, nread);
					} finally {
						ArrayPool<byte>.Shared.Return (buf);
					}

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
			default: throw new ArgumentOutOfRangeException (nameof (hashAlgorithm));
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
			case PublicKeyAlgorithmTag.ElGamalGeneral: return PublicKeyAlgorithm.ElGamalGeneral;
			case PublicKeyAlgorithmTag.ElGamalEncrypt: return PublicKeyAlgorithm.ElGamalEncrypt;
			case PublicKeyAlgorithmTag.Dsa:            return PublicKeyAlgorithm.Dsa;
			case PublicKeyAlgorithmTag.ECDH:           return PublicKeyAlgorithm.EllipticCurve;
			case PublicKeyAlgorithmTag.ECDsa:          return PublicKeyAlgorithm.EllipticCurveDsa;
			case PublicKeyAlgorithmTag.DiffieHellman:  return PublicKeyAlgorithm.DiffieHellman;
			default: throw new ArgumentOutOfRangeException (nameof (algorithm));
			}
		}

		bool TryGetPublicKey (PgpPublicKeyRing keyring, long keyId, out PgpPublicKey pubkey)
		{
			if (keyring != null) {
				foreach (PgpPublicKey key in keyring.GetPublicKeys ()) {
					if (key.KeyId == keyId) {
						pubkey = key;
						return true;
					}
				}
			}

			pubkey = null;

			return false;
		}

		async Task<DigitalSignatureCollection> GetDigitalSignaturesAsync (PgpSignatureList signatureList, Stream content, bool doAsync, CancellationToken cancellationToken)
		{
			var signatures = new List<IDigitalSignature> ();

			for (int i = 0; i < signatureList.Count; i++) {
				long keyId = signatureList[i].KeyId;
				PgpPublicKeyRing keyring;

				if (doAsync)
					keyring = await GetPublicKeyRingAsync (keyId, cancellationToken).ConfigureAwait (false);
				else
					keyring = GetPublicKeyRing (keyId, cancellationToken);

				TryGetPublicKey (keyring, keyId, out var key);

				var signature = new OpenPgpDigitalSignature (keyring, key, signatureList[i]) {
					PublicKeyAlgorithm = GetPublicKeyAlgorithm (signatureList[i].KeyAlgorithm),
					DigestAlgorithm = GetDigestAlgorithm (signatureList[i].HashAlgorithm),
					CreationDate = signatureList[i].CreationTime,
				};

				if (key != null)
					signatureList[i].InitVerify (key);

				signatures.Add (signature);
			}

			var buf = ArrayPool<byte>.Shared.Rent (BufferLength);
			int nread;

			try {
				while ((nread = content.Read (buf, 0, BufferLength)) > 0) {
					for (int i = 0; i < signatures.Count; i++) {
						if (signatures[i].SignerCertificate != null) {
							var pgp = (OpenPgpDigitalSignature) signatures[i];
							pgp.Signature.Update (buf, 0, nread);
						}
					}
				}
			} finally {
				ArrayPool<byte>.Shared.Return (buf);
			}

			return new DigitalSignatureCollection (signatures);
		}

		Task<DigitalSignatureCollection> VerifyAsync (Stream content, Stream signatureData, bool doAsync, CancellationToken cancellationToken)
		{
			if (content == null)
				throw new ArgumentNullException (nameof (content));

			if (signatureData == null)
				throw new ArgumentNullException (nameof (signatureData));

			using (var armored = new ArmoredInputStream (signatureData)) {
				var factory = new PgpObjectFactory (armored);
				var data = factory.NextPgpObject ();
				PgpSignatureList signatureList;

				var compressed = data as PgpCompressedData;
				if (compressed != null) {
					factory = new PgpObjectFactory (compressed.GetDataStream ());
					data = factory.NextPgpObject ();
				}

				if (data == null)
					throw new FormatException ("Invalid PGP format.");

				signatureList = (PgpSignatureList) data;

				return GetDigitalSignaturesAsync (signatureList, content, doAsync, cancellationToken);
			}
		}

		/// <summary>
		/// Verify the specified content using the detached signatureData.
		/// </summary>
		/// <remarks>
		/// <para>Verifies the specified content using the detached signatureData.</para>
		/// <para>If any of the signatures were made with an unrecognized key and <see cref="AutoKeyRetrieve"/> is enabled,
		/// an attempt will be made to retrieve said key(s). The <paramref name="cancellationToken"/> can be used to cancel
		/// key retrieval.</para>
		/// </remarks>
		/// <returns>A list of digital signatures.</returns>
		/// <param name="content">The content.</param>
		/// <param name="signatureData">The signature data.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signatureData"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.FormatException">
		/// <paramref name="signatureData"/> does not contain valid PGP signature data.
		/// </exception>
		public override DigitalSignatureCollection Verify (Stream content, Stream signatureData, CancellationToken cancellationToken = default (CancellationToken))
		{
			return VerifyAsync (content, signatureData, false, cancellationToken).GetAwaiter ().GetResult ();
		}

		/// <summary>
		/// Asynchronously verify the specified content using the detached signatureData.
		/// </summary>
		/// <remarks>
		/// <para>Verifies the specified content using the detached signatureData.</para>
		/// <para>If any of the signatures were made with an unrecognized key and <see cref="AutoKeyRetrieve"/> is enabled,
		/// an attempt will be made to retrieve said key(s). The <paramref name="cancellationToken"/> can be used to cancel
		/// key retrieval.</para>
		/// </remarks>
		/// <returns>A list of digital signatures.</returns>
		/// <param name="content">The content.</param>
		/// <param name="signatureData">The signature data.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signatureData"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.FormatException">
		/// <paramref name="signatureData"/> does not contain valid PGP signature data.
		/// </exception>
		public override Task<DigitalSignatureCollection> VerifyAsync (Stream content, Stream signatureData, CancellationToken cancellationToken = default (CancellationToken))
		{
			return VerifyAsync (content, signatureData, true, cancellationToken);
		}

		static Stream Compress (Stream content, byte[] buf, int bufferLength)
		{
			var compresser = new PgpCompressedDataGenerator (CompressionAlgorithmTag.ZLib);
			var memory = new MemoryBlockStream ();

			using (var compressed = compresser.Open (memory)) {
				var literalGenerator = new PgpLiteralDataGenerator ();

				using (var literal = literalGenerator.Open (compressed, 't', "mime.txt", content.Length, DateTime.Now)) {
					int nread;

					while ((nread = content.Read (buf, 0, bufferLength)) > 0)
						literal.Write (buf, 0, nread);

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
				var buf = ArrayPool<byte>.Shared.Rent (BufferLength);

				try {
					armored.SetHeader ("Version", null);

					using (var compressed = Compress (content, buf, BufferLength)) {
						using (var encrypted = encrypter.Open (armored, compressed.Length)) {
							int nread;

							try {
								while ((nread = compressed.Read (buf, 0, BufferLength)) > 0)
									encrypted.Write (buf, 0, nread);
							} finally {
								ArrayPool<byte>.Shared.Return (buf);
							}

							encrypted.Flush ();
						}
					}

					armored.Flush ();
				} finally {
					ArrayPool<byte>.Shared.Return (buf);
				}
			}

			memory.Position = 0;

			return memory;
		}

		internal static SymmetricKeyAlgorithmTag GetSymmetricKeyAlgorithm (EncryptionAlgorithm algorithm)
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
			default: throw new NotSupportedException (string.Format ("{0} is not supported.", algorithm));
			}
		}

		/// <summary>
		/// Encrypt the specified content for the specified recipients.
		/// </summary>
		/// <remarks>
		/// Encrypts the specified content for the specified recipients.
		/// </remarks>
		/// <returns>A new <see cref="MimePart"/> instance
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
				throw new ArgumentNullException (nameof (recipients));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			// TODO: document the exceptions that can be thrown by BouncyCastle
			return Encrypt (GetPublicKeys (recipients), content);
		}

		/// <summary>
		/// Encrypt the specified content for the specified recipients.
		/// </summary>
		/// <remarks>
		/// Encrypts the specified content for the specified recipients.
		/// </remarks>
		/// <returns>A new <see cref="MimePart"/> instance
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
		/// <exception cref="PublicKeyNotFoundException">
		/// A public key could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The specified encryption algorithm is not supported.
		/// </exception>
		public MimePart Encrypt (EncryptionAlgorithm algorithm, IEnumerable<MailboxAddress> recipients, Stream content)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			// TODO: document the exceptions that can be thrown by BouncyCastle
			return Encrypt (algorithm, GetPublicKeys (recipients), content);
		}

		/// <summary>
		/// Encrypt the specified content for the specified recipients.
		/// </summary>
		/// <remarks>
		/// Encrypts the specified content for the specified recipients.
		/// </remarks>
		/// <returns>A new <see cref="MimePart"/> instance
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
				throw new ArgumentNullException (nameof (recipients));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var encrypter = new PgpEncryptedDataGenerator (GetSymmetricKeyAlgorithm (algorithm), true);
			var unique = new HashSet<long> ();
			int count = 0;

			foreach (var recipient in recipients) {
				if (!recipient.IsEncryptionKey)
					throw new ArgumentException ("One or more of the recipient keys cannot be used for encrypting.", nameof (recipients));

				if (unique.Add (recipient.KeyId)) {
					encrypter.AddMethod (recipient);
					count++;
				}
			}

			if (count == 0)
				throw new ArgumentException ("No recipients specified.", nameof (recipients));

			var encrypted = Encrypt (encrypter, content);

			return new MimePart ("application", "octet-stream") {
				ContentDisposition = new ContentDisposition (ContentDisposition.Attachment),
				Content = new MimeContent (encrypted),
			};
		}

		/// <summary>
		/// Encrypt the specified content for the specified recipients.
		/// </summary>
		/// <remarks>
		/// Encrypts the specified content for the specified recipients.
		/// </remarks>
		/// <returns>A new <see cref="MimePart"/> instance
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
		/// Cryptographically sign and encrypt the specified content for the specified recipients.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs and encrypts the specified content for the specified recipients.
		/// </remarks>
		/// <returns>A new <see cref="MimePart"/> instance
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
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var key = GetSigningKey (signer);

			return SignAndEncrypt (key, digestAlgo, GetPublicKeys (recipients), content);
		}

		/// <summary>
		/// Cryptographically sign and encrypt the specified content for the specified recipients.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs and encrypts the specified content for the specified recipients.
		/// </remarks>
		/// <returns>A new <see cref="MimePart"/> instance
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
		public MimePart SignAndEncrypt (MailboxAddress signer, DigestAlgorithm digestAlgo, EncryptionAlgorithm cipherAlgo, IEnumerable<MailboxAddress> recipients, Stream content)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var key = GetSigningKey (signer);

			return SignAndEncrypt (key, digestAlgo, cipherAlgo, GetPublicKeys (recipients), content);
		}

		/// <summary>
		/// Cryptographically sign and encrypt the specified content for the specified recipients.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs and encrypts the specified content for the specified recipients.
		/// </remarks>
		/// <returns>A new <see cref="MimePart"/> instance
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
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (!signer.IsSigningKey)
				throw new ArgumentException ("The specified secret key cannot be used for signing.", nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (content == null)
				throw new ArgumentNullException (nameof (content));

			var encrypter = new PgpEncryptedDataGenerator (GetSymmetricKeyAlgorithm (cipherAlgo), true);
			var hashAlgorithm = GetHashAlgorithm (digestAlgo);
			var unique = new HashSet<long> ();
			int count = 0;

			foreach (var recipient in recipients) {
				if (!recipient.IsEncryptionKey)
					throw new ArgumentException ("One or more of the recipient keys cannot be used for encrypting.", nameof (recipients));

				if (unique.Add (recipient.KeyId)) {
					encrypter.AddMethod (recipient);
					count++;
				}
			}

			if (count == 0)
				throw new ArgumentException ("No recipients specified.", nameof (recipients));

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
						var buf = ArrayPool<byte>.Shared.Rent (BufferLength);
						int nread;

						try {
							while ((nread = content.Read (buf, 0, BufferLength)) > 0) {
								signatureGenerator.Update (buf, 0, nread);
								literal.Write (buf, 0, nread);
							}
						} finally {
							ArrayPool<byte>.Shared.Return (buf);
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
					armored.SetHeader ("Version", null);

					using (var encrypted = encrypter.Open (armored, compressed.Length)) {
						var buf = ArrayPool<byte>.Shared.Rent (BufferLength);
						int nread;

						try {
							while ((nread = compressed.Read (buf, 0, BufferLength)) > 0)
								encrypted.Write (buf, 0, nread);
						} finally {
							ArrayPool<byte>.Shared.Return (buf);
						}

						encrypted.Flush ();
					}

					armored.Flush ();
				}

				memory.Position = 0;

				return new MimePart ("application", "octet-stream") {
					ContentDisposition = new ContentDisposition (ContentDisposition.Attachment),
					Content = new MimeContent (memory)
				};
			}
		}

		/// <summary>
		/// Cryptographically sign and encrypt the specified content for the specified recipients.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs and encrypts the specified content for the specified recipients.
		/// </remarks>
		/// <returns>A new <see cref="MimePart"/> instance
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

		async Task<DigitalSignatureCollection> DecryptToAsync (Stream encryptedData, Stream decryptedData, bool doAsync, CancellationToken cancellationToken)
		{
			if (encryptedData == null)
				throw new ArgumentNullException (nameof (encryptedData));

			if (decryptedData == null)
				throw new ArgumentNullException (nameof (decryptedData));

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
				DigitalSignatureCollection signatures;
				PgpSignatureList signatureList = null;
				PgpCompressedData compressed = null;
				var position = decryptedData.Position;
				long nwritten = 0;

				obj = factory.NextPgpObject ();
				while (obj != null) {
					if (obj is PgpCompressedData) {
						if (compressed != null)
							throw new PgpException ("Recursive compression packets are not supported.");

						compressed = (PgpCompressedData) obj;
						factory = new PgpObjectFactory (compressed.GetDataStream ());
					} else if (obj is PgpOnePassSignatureList) {
						if (nwritten == 0) {
							var onepasses = (PgpOnePassSignatureList) obj;

							onepassList = new List<IDigitalSignature> ();

							for (int i = 0; i < onepasses.Count; i++) {
								var onepass = onepasses[i];
								PgpPublicKeyRing keyring;

								if (doAsync)
									keyring = await GetPublicKeyRingAsync (onepass.KeyId, cancellationToken).ConfigureAwait (false);
								else
									keyring = GetPublicKeyRing (onepass.KeyId, cancellationToken);

								if (!TryGetPublicKey (keyring, onepass.KeyId, out var key)) {
									// too messy, pretend we never found a one-pass signature list
									onepassList = null;
									break;
								}

								onepass.InitVerify (key);

								var signature = new OpenPgpDigitalSignature (keyring, key, onepass) {
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
							var buf = ArrayPool<byte>.Shared.Rent (BufferLength);
							int nread;

							try {
								while ((nread = stream.Read (buf, 0, BufferLength)) > 0) {
									if (onepassList != null) {
										// update our one-pass signatures...
										for (int index = 0; index < nread; index++) {
											byte c = buf[index];

											for (int i = 0; i < onepassList.Count; i++) {
												var pgp = (OpenPgpDigitalSignature) onepassList[i];
												pgp.OnePassSignature.Update (c);
											}
										}
									}

									if (doAsync)
										await decryptedData.WriteAsync (buf, 0, nread, cancellationToken).ConfigureAwait (false);
									else
										decryptedData.Write (buf, 0, nread);

									nwritten += nread;
								}
							} finally {
								ArrayPool<byte>.Shared.Return (buf);
							}
						}
					}

					obj = factory.NextPgpObject ();
				}

				if (signatureList != null) {
					if (onepassList != null && signatureList.Count == onepassList.Count) {
						for (int i = 0; i < onepassList.Count; i++) {
							var pgp = (OpenPgpDigitalSignature) onepassList[i];
							pgp.CreationDate = signatureList[i].CreationTime;
							pgp.Signature = signatureList[i];
						}

						signatures = new DigitalSignatureCollection (onepassList);
					} else {
						decryptedData.Position = position;
						signatures = await GetDigitalSignaturesAsync (signatureList, decryptedData, doAsync, cancellationToken).ConfigureAwait (false);
						decryptedData.Position = decryptedData.Length;
					}
				} else {
					signatures = null;
				}

				return signatures;
			}
		}

		/// <summary>
		/// Decrypt an encrypted stream and extract the digital signers if the content was also signed.
		/// </summary>
		/// <remarks>
		/// <para>Decrypts an encrypted stream and extracts the digital signers if the content was also signed.</para>
		/// <para>If any of the signatures were made with an unrecognized key and <see cref="AutoKeyRetrieve"/> is enabled,
		/// an attempt will be made to retrieve said key(s). The <paramref name="cancellationToken"/> can be used to cancel
		/// key retrieval.</para>
		/// </remarks>
		/// <returns>The list of digital signatures if the data was both signed and encrypted; otherwise, <c>null</c>.</returns>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <param name="decryptedData">The stream to write the decrypted data to.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="encryptedData"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="decryptedData"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found to decrypt the stream.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// <para>The user chose to cancel the password prompt.</para>
		/// <para>-or-</para>
		/// <para>The operation was cancelled via the cancellation token.</para>
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Bcpg.OpenPgp.PgpException">
		/// An OpenPGP error occurred.
		/// </exception>
		public DigitalSignatureCollection DecryptTo (Stream encryptedData, Stream decryptedData, CancellationToken cancellationToken = default (CancellationToken))
		{
			return DecryptToAsync (encryptedData, decryptedData, false, cancellationToken).GetAwaiter ().GetResult ();
		}

		/// <summary>
		/// Asynchronously decrypt an encrypted stream and extract the digital signers if the content was also signed.
		/// </summary>
		/// <remarks>
		/// <para>Decrypts an encrypted stream and extracts the digital signers if the content was also signed.</para>
		/// <para>If any of the signatures were made with an unrecognized key and <see cref="AutoKeyRetrieve"/> is enabled,
		/// an attempt will be made to retrieve said key(s). The <paramref name="cancellationToken"/> can be used to cancel
		/// key retrieval.</para>
		/// </remarks>
		/// <returns>The list of digital signatures if the data was both signed and encrypted; otherwise, <c>null</c>.</returns>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <param name="decryptedData">The stream to write the decrypted data to.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="encryptedData"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="decryptedData"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found to decrypt the stream.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// <para>The user chose to cancel the password prompt.</para>
		/// <para>-or-</para>
		/// <para>The operation was cancelled via the cancellation token.</para>
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Bcpg.OpenPgp.PgpException">
		/// An OpenPGP error occurred.
		/// </exception>
		public Task<DigitalSignatureCollection> DecryptToAsync (Stream encryptedData, Stream decryptedData, CancellationToken cancellationToken = default (CancellationToken))
		{
			return DecryptToAsync (encryptedData, decryptedData, true, cancellationToken);
		}

		/// <summary>
		/// Decrypts the specified encryptedData and extracts the digital signers if the content was also signed.
		/// </summary>
		/// <remarks>
		/// Decrypts the specified encryptedData and extracts the digital signers if the content was also signed.
		/// </remarks>
		/// <returns>The decrypted <see cref="MimeEntity"/>.</returns>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <param name="signatures">A list of digital signatures if the data was both signed and encrypted.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="encryptedData"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found to decrypt the stream.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// <para>The user chose to cancel the password prompt.</para>
		/// <para>-or-</para>
		/// <para>The operation was cancelled via the cancellation token.</para>
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Bcpg.OpenPgp.PgpException">
		/// An OpenPGP error occurred.
		/// </exception>
		public MimeEntity Decrypt (Stream encryptedData, out DigitalSignatureCollection signatures, CancellationToken cancellationToken = default (CancellationToken))
		{
			using (var decryptedData = new MemoryBlockStream ()) {
				signatures = DecryptTo (encryptedData, decryptedData, cancellationToken);
				decryptedData.Position = 0;

				return MimeEntity.Load (decryptedData, cancellationToken);
			}
		}

		/// <summary>
		/// Decrypts the specified encryptedData.
		/// </summary>
		/// <remarks>
		/// Decrypts the specified encryptedData.
		/// </remarks>
		/// <returns>The decrypted <see cref="MimeEntity"/>.</returns>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="encryptedData"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found to decrypt the stream.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// <para>The user chose to cancel the password prompt.</para>
		/// <para>-or-</para>
		/// <para>The operation was cancelled via the cancellation token.</para>
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Bcpg.OpenPgp.PgpException">
		/// An OpenPGP error occurred.
		/// </exception>
		public override MimeEntity Decrypt (Stream encryptedData, CancellationToken cancellationToken = default (CancellationToken))
		{
			using (var decryptedData = new MemoryBlockStream ()) {
				DecryptTo (encryptedData, decryptedData, cancellationToken);
				decryptedData.Position = 0;

				return MimeEntity.Load (decryptedData, cancellationToken);
			}
		}

		/// <summary>
		/// Import the specified public keyring bundle.
		/// </summary>
		/// <remarks>
		/// Imports the specified public keyring bundle.
		/// </remarks>
		/// <param name="bundle">THe bundle of public keyrings to import.</param>
		public abstract void Import (PgpPublicKeyRingBundle bundle);

		/// <summary>
		/// Releases all resources used by the <see cref="OpenPgpContext"/> object.
		/// </summary>
		/// <remarks>Call <see cref="CryptographyContext.Dispose()"/> when you are finished using the <see cref="OpenPgpContext"/>. The
		/// <see cref="CryptographyContext.Dispose()"/> method leaves the <see cref="OpenPgpContext"/> in an unusable state. After
		/// calling <see cref="CryptographyContext.Dispose()"/>, you must release all references to the <see cref="OpenPgpContext"/> so
		/// the garbage collector can reclaim the memory that the <see cref="OpenPgpContext"/> was occupying.</remarks>
		protected override void Dispose (bool disposing)
		{
			if (disposing && client != null) {
				client.Dispose ();
				client = null;
			}

			base.Dispose (disposing);
		}
	}
}

//
// OpenPgpContext.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2018 Xamarin Inc. (www.xamarin.com)
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
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto.Parameters;

using MimeKit.IO;
using MimeKit.Utils;

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
		const string BeginPublicKeyBlock = "-----BEGIN PGP PUBLIC KEY BLOCK-----";
		const string EndPublicKeyBlock = "-----END PGP PUBLIC KEY BLOCK-----";

		internal static readonly EncryptionAlgorithm[] DefaultEncryptionAlgorithmRank = {
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

		internal static readonly DigestAlgorithm[] DefaultDigestAlgorithmRank = {
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
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.OpenPgpContext"/> class.
		/// </summary>
		/// <remarks>
		/// Subclasses choosing to use this constructor MUST set the <see cref="PublicKeyRingPath"/>,
		/// <see cref="SecretKeyRingPath"/>, <see cref="PublicKeyRingBundle"/>, and the
		/// <see cref="SecretKeyRingBundle"/> properties themselves.
		/// </remarks>
		protected OpenPgpContext ()
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

#if PORTABLE
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.OpenPgpContext"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="OpenPgpContext"/> using the specified public and private keyrings.
		/// </remarks>
		/// <param name="pubring">The public keyring.</param>
		/// <param name="secring">The secret keyring.</param>
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
		protected OpenPgpContext (Stream pubring, Stream secring) : this ()
		{
			if (pubring == null)
				throw new ArgumentNullException ("pubring");

			if (secring == null)
				throw new ArgumentNullException ("secring");

			PublicKeyRing = pubring;
			SecretKeyRing = secring;

			PublicKeyRingBundle = new PgpPublicKeyRingBundle (pubring);
			SecretKeyRingBundle = new PgpSecretKeyRingBundle (secring);

			if (pubring.CanSeek)
				pubring.Seek (0, SeekOrigin.Begin);

			if (secring.CanSeek)
				secring.Seek (0, SeekOrigin.Begin);
		}
#else
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
		protected OpenPgpContext (string pubring, string secring) : this ()
		{
			if (pubring == null)
				throw new ArgumentNullException (nameof (pubring));

			if (secring == null)
				throw new ArgumentNullException (nameof (secring));

			PublicKeyRingPath = pubring;
			SecretKeyRingPath = secring;

			if (File.Exists (pubring)) {
				using (var file = File.Open (pubring, FileMode.Open, FileAccess.Read)) {
					PublicKeyRingBundle = new PgpPublicKeyRingBundle (file);
				}
			} else {
				PublicKeyRingBundle = new PgpPublicKeyRingBundle (new byte[0]);
			}

			if (File.Exists (secring)) {
				using (var file = File.Open (secring, FileMode.Open, FileAccess.Read)) {
					SecretKeyRingBundle = new PgpSecretKeyRingBundle (file);
				}
			} else {
				SecretKeyRingBundle = new PgpSecretKeyRingBundle (new byte[0]);
			}
		}
#endif

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

#if PORTABLE
		/// <summary>
		/// Get the public keyring.
		/// </summary>
		/// <remarks>
		/// Gets the public keyring.
		/// </remarks>
		/// <value>The public key ring.</value>
		protected Stream PublicKeyRing {
			get; set;
		}

		/// <summary>
		/// Get the secret keyring.
		/// </summary>
		/// <remarks>
		/// Gets the secret keyring.
		/// </remarks>
		/// <value>The secret key ring.</value>
		protected Stream SecretKeyRing {
			get; set;
		}
#else
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
#endif

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

			var type = protocol.ToLowerInvariant ().Split ('/');
			if (type.Length != 2 || type[0] != "application")
				return false;

			if (type[1].StartsWith ("x-", StringComparison.Ordinal))
				type[1] = type[1].Substring (2);

			return type[1] == "pgp-signature" || type[1] == "pgp-encrypted" || type[1] == "pgp-keys";
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

		static string HexEncode (byte[] data)
		{
			var fingerprint = new StringBuilder ();

			for (int i = 0; i < data.Length; i++)
				fingerprint.Append (data[i].ToString ("x2"));

			return fingerprint.ToString ();
		}

		static bool PgpPublicKeyMatches (PgpPublicKey key, MailboxAddress mailbox)
		{
			var secure = mailbox as SecureMailboxAddress;

			if (secure != null && !string.IsNullOrEmpty (secure.Fingerprint)) {
				if (secure.Fingerprint.Length > 16) {
					var fingerprint = HexEncode (key.GetFingerprint ());

					return secure.Fingerprint.Equals (fingerprint, StringComparison.OrdinalIgnoreCase);
				}

				var id = ((int) key.KeyId).ToString ("X2");

				return secure.Fingerprint.EndsWith (id, StringComparison.OrdinalIgnoreCase);
			}

			foreach (string userId in key.GetUserIds ()) {
				MailboxAddress email;

				if (!MailboxAddress.TryParse (userId, out email))
					continue;

				if (mailbox.Address.Equals (email.Address, StringComparison.OrdinalIgnoreCase))
					return true;
			}

			return false;
		}

		async Task<PgpPublicKeyRing> RetrievePublicKeyRingAsync (long keyId, bool doAsync, CancellationToken cancellationToken)
		{
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
			uri.Query = string.Format ("op=get&search=0x{0:X}", keyId);

			using (var stream = new MemoryBlockStream ()) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (new OpenPgpBlockFilter (BeginPublicKeyBlock, EndPublicKeyBlock));

					if (doAsync) {
						using (var response = await client.GetAsync (uri.ToString (), cancellationToken))
							await response.Content.CopyToAsync (filtered);
					} else {
#if !NETSTANDARD && !PORTABLE
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

		class KeyRetrievalResults
		{
			public readonly PgpPublicKeyRing KeyRing;
			public readonly PgpPublicKey Key;

			public KeyRetrievalResults (PgpPublicKeyRing keyring, PgpPublicKey pubkey)
			{
				KeyRing = keyring;
				Key = pubkey;
			}
		}

		async Task<KeyRetrievalResults> GetPublicKeyRingAsync (long keyId, bool doAsync, CancellationToken cancellationToken)
		{
			foreach (PgpPublicKeyRing ring in PublicKeyRingBundle.GetKeyRings ()) {
				foreach (PgpPublicKey key in ring.GetPublicKeys ()) {
					if (key.KeyId == keyId)
						return new KeyRetrievalResults (ring, key);
				}
			}

			if (AutoKeyRetrieve && IsValidKeyServer) {
				try {
					var keyring = await RetrievePublicKeyRingAsync (keyId, doAsync, cancellationToken).ConfigureAwait (false);

					return new KeyRetrievalResults (keyring, keyring.GetPublicKey (keyId));
				} catch (OperationCanceledException) {
					throw;
				} catch {
				}
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
		public IEnumerable<PgpPublicKeyRing> EnumeratePublicKeyRings ()
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
		public IEnumerable<PgpPublicKey> EnumeratePublicKeys ()
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
		public IEnumerable<PgpPublicKeyRing> EnumeratePublicKeyRings (MailboxAddress mailbox)
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
		public IEnumerable<PgpPublicKey> EnumeratePublicKeys (MailboxAddress mailbox)
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
		public IEnumerable<PgpSecretKeyRing> EnumerateSecretKeyRings ()
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
		public IEnumerable<PgpSecretKey> EnumerateSecretKeys ()
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
		public IEnumerable<PgpSecretKeyRing> EnumerateSecretKeyRings (MailboxAddress mailbox)
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
		public IEnumerable<PgpSecretKey> EnumerateSecretKeys (MailboxAddress mailbox)
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
				if (!key.IsEncryptionKey || key.IsRevoked ())
					continue;

				long seconds = key.GetValidSeconds ();
				if (seconds != 0) {
					var expires = key.CreationTime.AddSeconds ((double) seconds);
					if (expires <= DateTime.Now)
						continue;
				}

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
		internal protected virtual IList<PgpPublicKey> GetPublicKeys (IEnumerable<MailboxAddress> mailboxes)
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
			var secure = mailbox as SecureMailboxAddress;

			if (secure != null && !string.IsNullOrEmpty (secure.Fingerprint)) {
				if (secure.Fingerprint.Length > 16) {
					var fingerprint = HexEncode (key.PublicKey.GetFingerprint ());

					return secure.Fingerprint.Equals (fingerprint, StringComparison.OrdinalIgnoreCase);
				}

				var id = ((int) key.KeyId).ToString ("X2");

				return secure.Fingerprint.EndsWith (id, StringComparison.OrdinalIgnoreCase);
			}

			foreach (string userId in key.UserIds) {
				MailboxAddress email;

				if (!MailboxAddress.TryParse (userId, out email))
					continue;

				if (mailbox.Address.Equals (email.Address, StringComparison.OrdinalIgnoreCase))
					return true;
			}

			return false;
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
		internal protected virtual PgpSecretKey GetSigningKey (MailboxAddress mailbox)
		{
			if (mailbox == null)
				throw new ArgumentNullException (nameof (mailbox));

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
						if (expires <= DateTime.Now)
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

		PgpKeyRingGenerator CreateKeyRingGenerator (MailboxAddress mailbox, EncryptionAlgorithm algorithm, long expirationTime, string password, DateTime now)
		{
			var enabledEncryptionAlgorithms = EnabledEncryptionAlgorithms;
			var enabledDigestAlgorithms = EnabledDigestAlgorithms;
			var encryptionAlgorithms = new int[enabledEncryptionAlgorithms.Length];
			var digestAlgorithms = new int[enabledDigestAlgorithms.Length];

			for (int i = 0; i < enabledEncryptionAlgorithms.Length; i++)
				encryptionAlgorithms[i] = (int) enabledEncryptionAlgorithms[i];
			for (int i = 0; i < enabledDigestAlgorithms.Length; i++)
				digestAlgorithms[i] = (int) enabledDigestAlgorithms[i];

			var parameters = new RsaKeyGenerationParameters (BigInteger.ValueOf (0x10001), new SecureRandom (), 2048, 12);
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
				new SecureRandom ());

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
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="mailbox"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="expirationDate"/> is not a date in the future.
		/// </exception>
		public void GenerateKeyPair (MailboxAddress mailbox, string password, DateTime? expirationDate = null, EncryptionAlgorithm algorithm = EncryptionAlgorithm.Aes256)
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
					throw new ArgumentException ("expirationDate needs to be greater than DateTime.Now");

				if ((expirationTime = Convert.ToInt64 (utc.Subtract (now).TotalSeconds)) <= 0)
					throw new ArgumentException ("expirationDate needs to be greater than DateTime.Now");
			}

			var generator = CreateKeyRingGenerator (mailbox, algorithm, expirationTime, password, now);

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

			PublicKeyRingBundle = PgpPublicKeyRingBundle.AddPublicKeyRing (PublicKeyRingBundle, keyring);
			SavePublicKeyRingBundle ();
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

			foreach (PgpSecretKeyRing keyring in SecretKeyRingBundle.GetKeyRings ()) {
				foreach (PgpSecretKey key in keyring.GetSecretKeys ()) {
					if (!PgpSecretKeyMatches (keyring.GetSecretKey (), signer))
						continue;

					if (!key.IsSigningKey)
						continue;

					var pubkey = key.PublicKey;
					if (pubkey.IsRevoked ())
						continue;

					long seconds = pubkey.GetValidSeconds ();
					if (seconds != 0) {
						var expires = pubkey.CreationTime.AddSeconds ((double) seconds);
						if (expires <= DateTime.Now)
							continue;
					}

					return true;
				}
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
				if (!key.IsEncryptionKey || key.IsRevoked ())
					continue;

				long seconds = key.GetValidSeconds ();
				if (seconds != 0) {
					var expires = key.CreationTime.AddSeconds ((double) seconds);
					if (expires <= DateTime.Now)
						continue;
				}

				return true;
			}

			return false;
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
			case PublicKeyAlgorithmTag.ElGamalEncrypt: return PublicKeyAlgorithm.ElGamalEncrypt;
			case PublicKeyAlgorithmTag.Dsa:            return PublicKeyAlgorithm.Dsa;
			case PublicKeyAlgorithmTag.ECDH:           return PublicKeyAlgorithm.EllipticCurve;
			case PublicKeyAlgorithmTag.ECDsa:          return PublicKeyAlgorithm.EllipticCurveDsa;
			case PublicKeyAlgorithmTag.ElGamalGeneral: return PublicKeyAlgorithm.ElGamalGeneral;
			case PublicKeyAlgorithmTag.DiffieHellman:  return PublicKeyAlgorithm.DiffieHellman;
			default: throw new ArgumentOutOfRangeException (nameof (algorithm));
			}
		}

		async Task<DigitalSignatureCollection> GetDigitalSignaturesAsync (PgpSignatureList signatureList, Stream content, bool doAsync, CancellationToken cancellationToken)
		{
			var signatures = new List<IDigitalSignature> ();
			var buf = new byte[4096];
			int nread;

			for (int i = 0; i < signatureList.Count; i++) {
				long keyId = signatureList[i].KeyId;
				KeyRetrievalResults results;

				if (doAsync)
					results = await GetPublicKeyRingAsync (keyId, doAsync, cancellationToken).ConfigureAwait (false);
				else
					results = GetPublicKeyRingAsync (keyId, doAsync, cancellationToken).GetAwaiter ().GetResult (); 

				var signature = new OpenPgpDigitalSignature (results.KeyRing, results.Key, signatureList[i]) {
					PublicKeyAlgorithm = GetPublicKeyAlgorithm (signatureList[i].KeyAlgorithm),
					DigestAlgorithm = GetDigestAlgorithm (signatureList[i].HashAlgorithm),
					CreationDate = signatureList[i].CreationTime,
				};

				if (results.Key != null)
					signatureList[i].InitVerify (results.Key);

				signatures.Add (signature);
			}

			while ((nread = content.Read (buf, 0, buf.Length)) > 0) {
				for (int i = 0; i < signatures.Count; i++) {
					if (signatures[i].SignerCertificate != null) {
						var pgp = (OpenPgpDigitalSignature) signatures[i];
						pgp.Signature.Update (buf, 0, nread);
					}
				}
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

		static Stream Compress (Stream content, byte[] buf)
		{
			var compresser = new PgpCompressedDataGenerator (CompressionAlgorithmTag.ZLib);
			var memory = new MemoryBlockStream ();

			using (var compressed = compresser.Open (memory)) {
				var literalGenerator = new PgpLiteralDataGenerator ();

				using (var literal = literalGenerator.Open (compressed, 't', "mime.txt", content.Length, DateTime.Now)) {
					int nread;

					while ((nread = content.Read (buf, 0, buf.Length)) > 0)
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
				var buf = new byte[4096];

				armored.SetHeader ("Version", null);

				using (var compressed = Compress (content, buf)) {
					using (var encrypted = encrypter.Open (armored, compressed.Length)) {
						int nread;

						while ((nread = compressed.Read (buf, 0, buf.Length)) > 0)
							encrypted.Write (buf, 0, nread);

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
			default: throw new NotSupportedException (string.Format ("{0} is not supported.", algorithm));
			}
		}

		/// <summary>
		/// Encrypt the specified content for the specified recipients.
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
		/// Cryptographically sign and encrypt the specified content for the specified recipients.
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
			var buf = new byte[4096];
			int nread, count = 0;

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
					armored.SetHeader ("Version", null);

					using (var encrypted = encrypter.Open (armored, compressed.Length)) {
						while ((nread = compressed.Read (buf, 0, buf.Length)) > 0)
							encrypted.Write (buf, 0, nread);

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

								var results = await GetPublicKeyRingAsync (onepass.KeyId, doAsync, cancellationToken).ConfigureAwait (false);

								if (results.KeyRing == null) {
									// too messy, pretend we never found a one-pass signature list
									onepassList = null;
									break;
								}

								onepass.InitVerify (results.Key);

								var signature = new OpenPgpDigitalSignature (results.KeyRing, results.Key, onepass) {
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

								if (doAsync)
									await decryptedData.WriteAsync (buffer, 0, nread, cancellationToken).ConfigureAwait (false);
								else
									decryptedData.Write (buffer, 0, nread);

								nwritten += nread;
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
		/// <returns>The decrypted <see cref="MimeKit.MimeEntity"/>.</returns>
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
			var decryptedData = new MemoryBlockStream ();

			signatures = DecryptTo (encryptedData, decryptedData, cancellationToken);
			decryptedData.Position = 0;

			return MimeEntity.Load (decryptedData, true, cancellationToken);
		}

		/// <summary>
		/// Decrypts the specified encryptedData.
		/// </summary>
		/// <remarks>
		/// Decrypts the specified encryptedData.
		/// </remarks>
		/// <returns>The decrypted <see cref="MimeKit.MimeEntity"/>.</returns>
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
			var decryptedData = new MemoryBlockStream ();

			DecryptTo (encryptedData, decryptedData, cancellationToken);
			decryptedData.Position = 0;

			return MimeEntity.Load (decryptedData, true, cancellationToken);
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
#if !PORTABLE
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
#if !NETSTANDARD
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
#else
			PublicKeyRingBundle.Encode (PublicKeyRing);
			PublicKeyRing.Seek (0, SeekOrigin.Begin);
#endif
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
#if !PORTABLE
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
#if !NETSTANDARD
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
#else
			SecretKeyRingBundle.Encode (SecretKeyRing);
			SecretKeyRing.Seek (0, SeekOrigin.Begin);
#endif
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
		public void Import (PgpPublicKeyRing keyring)
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
		public void Import (PgpPublicKeyRingBundle bundle)
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
		public void Import (PgpSecretKeyRing keyring)
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
		public void Import (PgpSecretKeyRingBundle bundle)
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
		/// <returns>A new <see cref="MimeKit.MimePart"/> instance containing the exported public keys.</returns>
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
		/// <returns>A new <see cref="MimeKit.MimePart"/> instance containing the exported public keys.</returns>
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
		/// <returns>A new <see cref="MimeKit.MimePart"/> instance containing the exported public keys.</returns>
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
		/// <returns>A new <see cref="MimeKit.MimePart"/> instance containing the exported public keys.</returns>
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
		public void Delete (PgpPublicKeyRing keyring)
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
		public void Delete (PgpSecretKeyRing keyring)
		{
			if (keyring == null)
				throw new ArgumentNullException (nameof (keyring));

			SecretKeyRingBundle = PgpSecretKeyRingBundle.RemoveSecretKeyRing (SecretKeyRingBundle, keyring);
			SaveSecretKeyRingBundle ();
		}

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

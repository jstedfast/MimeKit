//
// SecureMimeContext.cs
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
using System.Security.Cryptography.X509Certificates;

using MimeKit.IO;

using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Asn1.Ntt;
using Org.BouncyCastle.Asn1.Kisa;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Smime;

using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A Secure MIME (S/MIME) cryptography context.
	/// </summary>
	/// <remarks>
	/// Generally speaking, applications should not use a <see cref="SecureMimeContext"/>
	/// directly, but rather via higher level APIs such as <see cref="MultipartSigned"/>
	/// and <see cref="ApplicationPkcs7Mime"/>.
	/// </remarks>
	public abstract class SecureMimeContext : CryptographyContext, ISecureMimeContext
	{
		static readonly string[] ProtocolSubtypes = { "pkcs7-signature", "pkcs7-mime", "pkcs7-keys", "x-pkcs7-signature", "x-pkcs7-mime", "x-pkcs7-keys" };
		internal const X509KeyUsageFlags DigitalSignatureKeyUsageFlags = X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.NonRepudiation;
		internal static readonly int EncryptionAlgorithmCount = Enum.GetValues (typeof (EncryptionAlgorithm)).Length;
		internal static readonly DerObjectIdentifier Blowfish = new DerObjectIdentifier ("1.3.6.1.4.1.3029.1.2");
		internal static readonly DerObjectIdentifier Twofish = new DerObjectIdentifier ("1.3.6.1.4.1.25258.3.3");

		/// <summary>
		/// Initialize a new instance of the <see cref="SecureMimeContext"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Enables the following encryption algorithms by default:</para>
		/// <list type="bullet">
		/// <item><see cref="EncryptionAlgorithm.Aes256"/></item>
		/// <item><see cref="EncryptionAlgorithm.Aes192"/></item>
		/// <item><see cref="EncryptionAlgorithm.Aes128"/></item>
		/// <item><see cref="EncryptionAlgorithm.Camellia256"/></item>
		/// <item><see cref="EncryptionAlgorithm.Camellia192"/></item>
		/// <item><see cref="EncryptionAlgorithm.Camellia128"/></item>
		/// <item><see cref="EncryptionAlgorithm.Cast5"/></item>
		/// <item><see cref="EncryptionAlgorithm.TripleDes"/></item>
		/// <item><see cref="EncryptionAlgorithm.Seed"/></item>
		/// </list>
		/// </remarks>
		protected SecureMimeContext ()
		{
			EncryptionAlgorithmRank = new[] {
				EncryptionAlgorithm.Aes256,
				EncryptionAlgorithm.Aes192,
				EncryptionAlgorithm.Aes128,
				//EncryptionAlgorithm.Twofish,
				EncryptionAlgorithm.Seed,
				EncryptionAlgorithm.Camellia256,
				EncryptionAlgorithm.Camellia192,
				EncryptionAlgorithm.Camellia128,
				EncryptionAlgorithm.Cast5,
				EncryptionAlgorithm.Blowfish,
				EncryptionAlgorithm.TripleDes,
				EncryptionAlgorithm.Idea,
				EncryptionAlgorithm.RC2128,
				EncryptionAlgorithm.RC264,
				EncryptionAlgorithm.Des,
				EncryptionAlgorithm.RC240
			};

			foreach (var algorithm in EncryptionAlgorithmRank) {
				Enable (algorithm);

				// Don't enable anything weaker than Triple-DES by default
				if (algorithm == EncryptionAlgorithm.TripleDes)
					break;
			}

			// Disable Blowfish and Twofish by default for now
			Disable (EncryptionAlgorithm.Blowfish);
			Disable (EncryptionAlgorithm.Twofish);

			// TODO: Set a preferred digest algorithm rank and enable them.
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
			get { return "application/pkcs7-signature"; }
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
			get { return "application/pkcs7-mime"; }
		}

		/// <summary>
		/// Get the key exchange protocol.
		/// </summary>
		/// <remarks>
		/// Gets the key exchange protocol.
		/// </remarks>
		/// <value>The key exchange protocol.</value>
		public override string KeyExchangeProtocol {
			get { return "application/pkcs7-mime"; }
		}

		/// <summary>
		/// Check whether or not the specified protocol is supported by the <see cref="CryptographyContext"/>.
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
		/// <item><term><see cref="DigestAlgorithm.MD2"/></term><description>md2</description></item>
		/// <item><term><see cref="DigestAlgorithm.MD4"/></term><description>md4</description></item>
		/// <item><term><see cref="DigestAlgorithm.MD5"/></term><description>md5</description></item>
		/// <item><term><see cref="DigestAlgorithm.Sha1"/></term><description>sha-1</description></item>
		/// <item><term><see cref="DigestAlgorithm.Sha224"/></term><description>sha-224</description></item>
		/// <item><term><see cref="DigestAlgorithm.Sha256"/></term><description>sha-256</description></item>
		/// <item><term><see cref="DigestAlgorithm.Sha384"/></term><description>sha-384</description></item>
		/// <item><term><see cref="DigestAlgorithm.Sha512"/></term><description>sha-512</description></item>
		/// <item><term><see cref="DigestAlgorithm.Tiger192"/></term><description>tiger-192</description></item>
		/// <item><term><see cref="DigestAlgorithm.RipeMD160"/></term><description>ripemd160</description></item>
		/// <item><term><see cref="DigestAlgorithm.Haval5160"/></term><description>haval-5-160</description></item>
		/// </list>
		/// </remarks>
		/// <returns>The micalg value.</returns>
		/// <param name="micalg">The digest algorithm.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="micalg"/> is out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The specified <see cref="DigestAlgorithm"/> is not supported by this context.
		/// </exception>
		public override string GetDigestAlgorithmName (DigestAlgorithm micalg)
		{
			switch (micalg) {
			case DigestAlgorithm.MD5:        return "md5";
			case DigestAlgorithm.Sha1:       return "sha-1";
			case DigestAlgorithm.RipeMD160:  return "ripemd160";
			case DigestAlgorithm.MD2:        return "md2";
			case DigestAlgorithm.Tiger192:   return "tiger192";
			case DigestAlgorithm.Haval5160:  return "haval-5-160";
			case DigestAlgorithm.Sha256:     return "sha-256";
			case DigestAlgorithm.Sha384:     return "sha-384";
			case DigestAlgorithm.Sha512:     return "sha-512";
			case DigestAlgorithm.Sha224:     return "sha-224";
			case DigestAlgorithm.MD4:        return "md4";
			case DigestAlgorithm.DoubleSha:
				throw new NotSupportedException (string.Format ("{0} is not supported.", micalg));
			default:
				throw new ArgumentOutOfRangeException (nameof (micalg), micalg, string.Format ("Unknown DigestAlgorithm: {0}", micalg));
			}
		}

		/// <summary>
		/// Get the digest algorithm from the micalg parameter value in a multipart/signed part.
		/// </summary>
		/// <remarks>
		/// Maps the micalg parameter value string back to the appropriate <see cref="DigestAlgorithm"/>.
		/// <para>Maps the micalg parameter value string back to the appropriate <see cref="DigestAlgorithm"/></para>
		/// <list type="table">
		/// <listheader><term>Algorithm</term><description>Name</description></listheader>
		/// <item><term><see cref="DigestAlgorithm.MD2"/></term><description>md2</description></item>
		/// <item><term><see cref="DigestAlgorithm.MD4"/></term><description>md4</description></item>
		/// <item><term><see cref="DigestAlgorithm.MD5"/></term><description>md5</description></item>
		/// <item><term><see cref="DigestAlgorithm.Sha1"/></term><description>sha-1</description></item>
		/// <item><term><see cref="DigestAlgorithm.Sha224"/></term><description>sha-224</description></item>
		/// <item><term><see cref="DigestAlgorithm.Sha256"/></term><description>sha-256</description></item>
		/// <item><term><see cref="DigestAlgorithm.Sha384"/></term><description>sha-384</description></item>
		/// <item><term><see cref="DigestAlgorithm.Sha512"/></term><description>sha-512</description></item>
		/// <item><term><see cref="DigestAlgorithm.Tiger192"/></term><description>tiger-192</description></item>
		/// <item><term><see cref="DigestAlgorithm.RipeMD160"/></term><description>ripemd160</description></item>
		/// <item><term><see cref="DigestAlgorithm.Haval5160"/></term><description>haval-5-160</description></item>
		/// </list>
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
			case "md5":         return DigestAlgorithm.MD5;
			case "sha1":
			case "sha-1":       return DigestAlgorithm.Sha1;
			case "ripemd160":   return DigestAlgorithm.RipeMD160;
			case "md2":         return DigestAlgorithm.MD2;
			case "tiger192":    return DigestAlgorithm.Tiger192;
			case "haval-5-160": return DigestAlgorithm.Haval5160;
			case "sha256":
			case "sha-256":     return DigestAlgorithm.Sha256;
			case "sha384":
			case "sha-384":     return DigestAlgorithm.Sha384;
			case "sha512":
			case "sha-512":     return DigestAlgorithm.Sha512;
			case "sha224":
			case "sha-224":     return DigestAlgorithm.Sha224;
			case "md4":         return DigestAlgorithm.MD4;
			default:            return DigestAlgorithm.None;
			}
		}

		/// <summary>
		/// Get the OID for the digest algorithm.
		/// </summary>
		/// <remarks>
		/// Gets the OID for the digest algorithm.
		/// </remarks>
		/// <returns>The digest oid.</returns>
		/// <param name="digestAlgo">The digest algorithm.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="digestAlgo"/> is out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The specified <see cref="DigestAlgorithm"/> is not supported by this context.
		/// </exception>
		internal protected static string GetDigestOid (DigestAlgorithm digestAlgo)
		{
			switch (digestAlgo) {
			case DigestAlgorithm.MD5:        return CmsSignedGenerator.DigestMD5;
			case DigestAlgorithm.Sha1:       return CmsSignedGenerator.DigestSha1;
			case DigestAlgorithm.MD2:        return PkcsObjectIdentifiers.MD2.Id;
			case DigestAlgorithm.Sha256:     return CmsSignedGenerator.DigestSha256;
			case DigestAlgorithm.Sha384:     return CmsSignedGenerator.DigestSha384;
			case DigestAlgorithm.Sha512:     return CmsSignedGenerator.DigestSha512;
			case DigestAlgorithm.Sha224:     return CmsSignedGenerator.DigestSha224;
			case DigestAlgorithm.MD4:        return PkcsObjectIdentifiers.MD4.Id;
			case DigestAlgorithm.RipeMD160:  return CmsSignedGenerator.DigestRipeMD160;
			case DigestAlgorithm.DoubleSha:
			case DigestAlgorithm.Tiger192:
			case DigestAlgorithm.Haval5160:
				throw new NotSupportedException (string.Format ("{0} is not supported.", digestAlgo));
			default:
				throw new ArgumentOutOfRangeException (nameof (digestAlgo), digestAlgo, string.Format ("Unknown DigestAlgorithm: {0}", digestAlgo));
			}
		}

		internal static bool TryGetDigestAlgorithm (string id, out DigestAlgorithm algorithm)
		{
			if (id == CmsSignedGenerator.DigestSha1) {
				algorithm = DigestAlgorithm.Sha1;
				return true;
			}

			if (id == CmsSignedGenerator.DigestSha224) {
				algorithm = DigestAlgorithm.Sha224;
				return true;
			}

			if (id == CmsSignedGenerator.DigestSha256) {
				algorithm = DigestAlgorithm.Sha256;
				return true;
			}

			if (id == CmsSignedGenerator.DigestSha384) {
				algorithm = DigestAlgorithm.Sha384;
				return true;
			}

			if (id == CmsSignedGenerator.DigestSha512) {
				algorithm = DigestAlgorithm.Sha512;
				return true;
			}

			if (id == CmsSignedGenerator.DigestRipeMD160) {
				algorithm = DigestAlgorithm.RipeMD160;
				return true;
			}

			if (id == CmsSignedGenerator.DigestMD5) {
				algorithm = DigestAlgorithm.MD5;
				return true;
			}

			if (id == PkcsObjectIdentifiers.MD4.Id) {
				algorithm = DigestAlgorithm.MD4;
				return true;
			}

			if (id == PkcsObjectIdentifiers.MD2.Id) {
				algorithm = DigestAlgorithm.MD2;
				return true;
			}

			algorithm = DigestAlgorithm.None;

			return false;
		}

		//class VoteComparer : IComparer<int>
		//{
		//	public int Compare (int x, int y)
		//	{
		//		return y - x;
		//	}
		//}

		/// <summary>
		/// Get the preferred encryption algorithm to use for encrypting to the specified recipients.
		/// </summary>
		/// <remarks>
		/// <para>Gets the preferred encryption algorithm to use for encrypting to the specified recipients
		/// based on the encryption algorithms supported by each of the recipients, the
		/// <see cref="CryptographyContext.EnabledEncryptionAlgorithms"/>, and the
		/// <see cref="CryptographyContext.EncryptionAlgorithmRank"/>.</para>
		/// <para>If the supported encryption algorithms are unknown for any recipient, it is assumed that
		/// the recipient supports at least the Triple-DES encryption algorithm.</para>
		/// </remarks>
		/// <returns>The preferred encryption algorithm.</returns>
		/// <param name="recipients">The recipients.</param>
		protected virtual EncryptionAlgorithm GetPreferredEncryptionAlgorithm (CmsRecipientCollection recipients)
		{
			var votes = new int[EncryptionAlgorithmCount];
			int need = recipients.Count;

			foreach (var recipient in recipients) {
				int cast = EncryptionAlgorithmCount;

				foreach (var algorithm in recipient.EncryptionAlgorithms)
					votes[(int) algorithm]++;
			}

			// Starting with S/MIME v3 (published in 1999), Triple-DES is a REQUIRED algorithm.
			// S/MIME v2.x and older only required RC2/40, but SUGGESTED Triple-DES.
			// Considering the fact that Bruce Schneier was able to write a
			// screensaver that could crack RC2/40 back in the late 90's, let's
			// not default to anything weaker than Triple-DES...
			EncryptionAlgorithm chosen = EncryptionAlgorithm.TripleDes;
			int nvotes = 0;

			votes[(int) EncryptionAlgorithm.TripleDes] = need;

			// iterate through the algorithms, from strongest to weakest, keeping track
			// of the algorithm with the most amount of votes (between algorithms with
			// the same number of votes, choose the strongest of the 2 - i.e. the one
			// that we arrive at first).
			var algorithms = EncryptionAlgorithmRank;
			for (int i = 0; i < algorithms.Length; i++) {
				var algorithm = algorithms[i];

				if (!IsEnabled (algorithm))
					continue;

				if (votes[(int) algorithm] > nvotes) {
					nvotes = votes[(int) algorithm];
					chosen = algorithm;
				}
			}

			return chosen;
		}

		/// <summary>
		/// Compress the specified stream.
		/// </summary>
		/// <remarks>
		/// Compresses the specified stream.
		/// </remarks>
		/// <returns>A new <see cref="ApplicationPkcs7Mime"/> instance
		/// containing the compressed content.</returns>
		/// <param name="stream">The stream to compress.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public virtual ApplicationPkcs7Mime Compress (Stream stream, CancellationToken cancellationToken = default)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			cancellationToken.ThrowIfCancellationRequested ();

			var compresser = new CmsCompressedDataGenerator ();
			var processable = new CmsProcessableInputStream (stream);
			var compressed = compresser.Generate (processable, CmsCompressedDataGenerator.ZLib);

			var content = new MemoryBlockStream ();
			compressed.ContentInfo.EncodeTo (content);
			content.Position = 0;

			return new ApplicationPkcs7Mime (SecureMimeType.CompressedData, content);
		}

		/// <summary>
		/// Asynchronously compress the specified stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously compresses the specified stream.
		/// </remarks>
		/// <returns>A new <see cref="ApplicationPkcs7Mime"/> instance
		/// containing the compressed content.</returns>
		/// <param name="stream">The stream to compress.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public virtual Task<ApplicationPkcs7Mime> CompressAsync (Stream stream, CancellationToken cancellationToken = default)
		{
			// TODO: find a way to compress asynchronously
			return Task.FromResult (Compress (stream, cancellationToken));
		}

		/// <summary>
		/// Decompress the specified stream.
		/// </summary>
		/// <remarks>
		/// Decompresses the specified stream.
		/// </remarks>
		/// <returns>The decompressed mime part.</returns>
		/// <param name="stream">The stream to decompress.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public virtual MimeEntity Decompress (Stream stream, CancellationToken cancellationToken = default)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			cancellationToken.ThrowIfCancellationRequested ();

			using (var parser = new CmsCompressedDataParser (stream)) {
				var content = parser.GetContent ();

				try {
					return MimeEntity.Load (content.ContentStream, cancellationToken);
				} finally {
					content.ContentStream.Dispose ();
				}
			}
		}

		/// <summary>
		/// Asynchronously decompress the specified stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously decompresses the specified stream.
		/// </remarks>
		/// <returns>The decompressed mime part.</returns>
		/// <param name="stream">The stream to decompress.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public virtual async Task<MimeEntity> DecompressAsync (Stream stream, CancellationToken cancellationToken = default)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			cancellationToken.ThrowIfCancellationRequested ();

			using (var parser = new CmsCompressedDataParser (stream)) {
				var content = parser.GetContent ();

				try {
					return await MimeEntity.LoadAsync (content.ContentStream, cancellationToken).ConfigureAwait (false);
				} finally {
					content.ContentStream.Dispose ();
				}
			}
		}

		/// <summary>
		/// Decompress the specified stream to an output stream.
		/// </summary>
		/// <remarks>
		/// Decompresses the specified stream to an output stream.
		/// </remarks>
		/// <param name="stream">The stream to decompress.</param>
		/// <param name="output">The output stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="output"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public virtual void DecompressTo (Stream stream, Stream output, CancellationToken cancellationToken = default)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			if (output == null)
				throw new ArgumentNullException (nameof (output));

			cancellationToken.ThrowIfCancellationRequested ();

			using (var parser = new CmsCompressedDataParser (stream)) {
				var content = parser.GetContent ();

				try {
					cancellationToken.ThrowIfCancellationRequested ();
					content.ContentStream.CopyTo (output, 4096);
				} finally {
					content.ContentStream.Dispose ();
				}
			}
		}

		/// <summary>
		/// Asynchronously decompress the specified stream to an output stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously decompresses the specified stream to an output stream.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="stream">The stream to decompress.</param>
		/// <param name="output">The output stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="output"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public virtual async Task DecompressToAsync (Stream stream, Stream output, CancellationToken cancellationToken = default)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			if (output == null)
				throw new ArgumentNullException (nameof (output));

			cancellationToken.ThrowIfCancellationRequested ();

			using (var parser = new CmsCompressedDataParser (stream)) {
				var content = parser.GetContent ();

				try {
					cancellationToken.ThrowIfCancellationRequested ();
					await content.ContentStream.CopyToAsync (output, 4096, cancellationToken).ConfigureAwait (false);
				} finally {
					content.ContentStream.Dispose ();
				}
			}
		}

		internal SmimeCapabilitiesAttribute GetSecureMimeCapabilitiesAttribute (bool includeRsaesOaep)
		{
			var capabilities = new SmimeCapabilityVector ();

			foreach (var algorithm in EncryptionAlgorithmRank) {
				if (!IsEnabled (algorithm))
					continue;

				switch (algorithm) {
				case EncryptionAlgorithm.Aes128:
					capabilities.AddCapability (SmimeCapabilities.Aes128Cbc);
					break;
				case EncryptionAlgorithm.Aes192:
					capabilities.AddCapability (SmimeCapabilities.Aes192Cbc);
					break;
				case EncryptionAlgorithm.Aes256:
					capabilities.AddCapability (SmimeCapabilities.Aes256Cbc);
					break;
				case EncryptionAlgorithm.Blowfish:
					capabilities.AddCapability (Blowfish);
					break;
				case EncryptionAlgorithm.Camellia128:
					capabilities.AddCapability (NttObjectIdentifiers.IdCamellia128Cbc);
					break;
				case EncryptionAlgorithm.Camellia192:
					capabilities.AddCapability (NttObjectIdentifiers.IdCamellia192Cbc);
					break;
				case EncryptionAlgorithm.Camellia256:
					capabilities.AddCapability (NttObjectIdentifiers.IdCamellia256Cbc);
					break;
				case EncryptionAlgorithm.Cast5:
					capabilities.AddCapability (SmimeCapabilities.Cast5Cbc);
					break;
				case EncryptionAlgorithm.Des:
					capabilities.AddCapability (SmimeCapabilities.DesCbc);
					break;
				case EncryptionAlgorithm.Idea:
					capabilities.AddCapability (SmimeCapabilities.IdeaCbc);
					break;
				case EncryptionAlgorithm.RC240:
					capabilities.AddCapability (SmimeCapabilities.RC2Cbc, 40);
					break;
				case EncryptionAlgorithm.RC264:
					capabilities.AddCapability (SmimeCapabilities.RC2Cbc, 64);
					break;
				case EncryptionAlgorithm.RC2128:
					capabilities.AddCapability (SmimeCapabilities.RC2Cbc, 128);
					break;
				case EncryptionAlgorithm.Seed:
					capabilities.AddCapability (KisaObjectIdentifiers.IdSeedCbc);
					break;
				case EncryptionAlgorithm.TripleDes:
					capabilities.AddCapability (SmimeCapabilities.DesEde3Cbc);
					break;
				//case EncryptionAlgorithm.Twofish:
				//	capabilities.AddCapability (Twofish);
				//	break;
				}
			}

			if (includeRsaesOaep) {
				capabilities.AddCapability (PkcsObjectIdentifiers.IdRsaesOaep, RsaEncryptionPadding.OaepSha1.GetRsaesOaepParameters ());
				capabilities.AddCapability (PkcsObjectIdentifiers.IdRsaesOaep, RsaEncryptionPadding.OaepSha256.GetRsaesOaepParameters ());
				capabilities.AddCapability (PkcsObjectIdentifiers.IdRsaesOaep, RsaEncryptionPadding.OaepSha384.GetRsaesOaepParameters ());
				capabilities.AddCapability (PkcsObjectIdentifiers.IdRsaesOaep, RsaEncryptionPadding.OaepSha512.GetRsaesOaepParameters ());
			}

			return new SmimeCapabilitiesAttribute (capabilities);
		}

		/// <summary>
		/// Sign and encapsulate the content using the specified signer.
		/// </summary>
		/// <remarks>
		/// Signs and encapsulates the content using the specified signer.
		/// </remarks>
		/// <returns>A new <see cref="ApplicationPkcs7Mime"/> instance
		/// containing the detached signature data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="content">The content.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public abstract ApplicationPkcs7Mime EncapsulatedSign (CmsSigner signer, Stream content, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously sign and encapsulate the content using the specified signer.
		/// </summary>
		/// <remarks>
		/// Asynchronously signs and encapsulates the content using the specified signer.
		/// </remarks>
		/// <returns>A new <see cref="ApplicationPkcs7Mime"/> instance
		/// containing the detached signature data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="content">The content.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public abstract Task<ApplicationPkcs7Mime> EncapsulatedSignAsync (CmsSigner signer, Stream content, CancellationToken cancellationToken = default);

		/// <summary>
		/// Sign and encapsulate the content using the specified signer and digest algorithm.
		/// </summary>
		/// <remarks>
		/// Signs and encapsulates the content using the specified signer and digest algorithm.
		/// </remarks>
		/// <returns>A new <see cref="ApplicationPkcs7Mime"/> instance
		/// containing the detached signature data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="content">The content.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
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
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for <paramref name="signer"/>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public abstract ApplicationPkcs7Mime EncapsulatedSign (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously sign and encapsulate the content using the specified signer and digest algorithm.
		/// </summary>
		/// <remarks>
		/// Asynchronously signs and encapsulates the content using the specified signer and digest algorithm.
		/// </remarks>
		/// <returns>A new <see cref="ApplicationPkcs7Mime"/> instance
		/// containing the detached signature data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="content">The content.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
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
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for <paramref name="signer"/>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public abstract Task<ApplicationPkcs7Mime> EncapsulatedSignAsync (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content, CancellationToken cancellationToken = default);

		/// <summary>
		/// Sign the content using the specified signer.
		/// </summary>
		/// <remarks>
		/// Signs the content using the specified signer.
		/// </remarks>
		/// <returns>A new <see cref="ApplicationPkcs7Signature"/> instance
		/// containing the detached signature data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="content">The content.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public abstract ApplicationPkcs7Signature Sign (CmsSigner signer, Stream content, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously sign the content using the specified signer.
		/// </summary>
		/// <remarks>
		/// Asynchronously signs the content using the specified signer.
		/// </remarks>
		/// <returns>A new <see cref="ApplicationPkcs7Signature"/> instance
		/// containing the detached signature data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="content">The content.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public abstract Task<ApplicationPkcs7Signature> SignAsync (CmsSigner signer, Stream content, CancellationToken cancellationToken = default);

		/// <summary>
		/// Verify the digital signatures of the specified signed data and extract the original content.
		/// </summary>
		/// <remarks>
		/// Verifies the digital signatures of the specified signed data and extracts the original content.
		/// </remarks>
		/// <returns>The list of digital signatures.</returns>
		/// <param name="signedData">The signed data.</param>
		/// <param name="entity">The extracted MIME entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="signedData"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The extracted content could not be parsed as a MIME entity.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		public abstract DigitalSignatureCollection Verify (Stream signedData, out MimeEntity entity, CancellationToken cancellationToken = default);

		/// <summary>
		/// Verify the digital signatures of the specified signed data and extract the original content.
		/// </summary>
		/// <remarks>
		/// Verifies the digital signatures of the specified signed data and extracts the original content.
		/// </remarks>
		/// <returns>The extracted content stream.</returns>
		/// <param name="signedData">The signed data.</param>
		/// <param name="signatures">The digital signatures.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="signedData"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		public abstract Stream Verify (Stream signedData, out DigitalSignatureCollection signatures, CancellationToken cancellationToken = default);

		/// <summary>
		/// Encrypts the specified content for the specified recipients.
		/// </summary>
		/// <remarks>
		/// Encrypts the specified content for the specified recipients.
		/// </remarks>
		/// <returns>A new <see cref="ApplicationPkcs7Mime"/> instance
		/// containing the encrypted content.</returns>
		/// <param name="recipients">The recipients.</param>
		/// <param name="content">The content.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		public abstract ApplicationPkcs7Mime Encrypt (CmsRecipientCollection recipients, Stream content, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously encrypts the specified content for the specified recipients.
		/// </summary>
		/// <remarks>
		/// Asynchronously encrypts the specified content for the specified recipients.
		/// </remarks>
		/// <returns>A new <see cref="ApplicationPkcs7Mime"/> instance
		/// containing the encrypted content.</returns>
		/// <param name="recipients">The recipients.</param>
		/// <param name="content">The content.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		public abstract Task<ApplicationPkcs7Mime> EncryptAsync (CmsRecipientCollection recipients, Stream content, CancellationToken cancellationToken = default);

		/// <summary>
		/// Decrypts the specified encryptedData to an output stream.
		/// </summary>
		/// <remarks>
		/// Decrypts the specified encryptedData to an output stream.
		/// </remarks>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <param name="decryptedData">The stream to write the decrypted data to.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="encryptedData"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="decryptedData"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		public abstract void DecryptTo (Stream encryptedData, Stream decryptedData, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously decrypts the specified encryptedData to an output stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously decrypts the specified encryptedData to an output stream.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <param name="decryptedData">The stream to write the decrypted data to.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="encryptedData"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="decryptedData"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		public abstract Task DecryptToAsync (Stream encryptedData, Stream decryptedData, CancellationToken cancellationToken = default);

		/// <summary>
		/// Imports certificates and keys from a pkcs12-encoded stream.
		/// </summary>
		/// <remarks>
		/// Imports certificates and keys from a pkcs12-encoded stream.
		/// </remarks>
		/// <param name="stream">The raw certificate and key data.</param>
		/// <param name="password">The password to unlock the stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// Importing keys is not supported by this cryptography context.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		public abstract void Import (Stream stream, string password, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously imports certificates and keys from a pkcs12-encoded stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously imports certificates and keys from a pkcs12-encoded stream.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="stream">The raw certificate and key data.</param>
		/// <param name="password">The password to unlock the stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// Importing keys is not supported by this cryptography context.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		public abstract Task ImportAsync (Stream stream, string password, CancellationToken cancellationToken = default);

		/// <summary>
		/// Imports certificates and keys from a pkcs12 file.
		/// </summary>
		/// <remarks>
		/// Imports certificates and keys from a pkcs12 file.
		/// </remarks>
		/// <param name="fileName">The raw certificate and key data in pkcs12 format.</param>
		/// <param name="password">The password to unlock the stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.</para>
		/// <para>-or-</para>
		/// <para><paramref name="fileName"/> does not contain a private key.</para>
		/// <para>-or-</para>
		/// <para><paramref name="fileName"/> does not contain a certificate that could be used for signing.</para>
		/// </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// <paramref name="fileName"/> is an invalid file path.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file path could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// Importing keys is not supported by this cryptography context.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		public virtual void Import (string fileName, string password, CancellationToken cancellationToken = default)
		{
			if (fileName == null)
				throw new ArgumentNullException (nameof (fileName));

			if (password == null)
				throw new ArgumentNullException (nameof (password));

			cancellationToken.ThrowIfCancellationRequested ();

			using (var stream = File.OpenRead (fileName))
				Import (stream, password, cancellationToken);
		}

		/// <summary>
		/// Asynchronously imports certificates and keys from a pkcs12 file.
		/// </summary>
		/// <remarks>
		/// Asynchronously imports certificates and keys from a pkcs12 file.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="fileName">The raw certificate and key data in pkcs12 format.</param>
		/// <param name="password">The password to unlock the stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.</para>
		/// <para>-or-</para>
		/// <para><paramref name="fileName"/> does not contain a private key.</para>
		/// <para>-or-</para>
		/// <para><paramref name="fileName"/> does not contain a certificate that could be used for signing.</para>
		/// </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// <paramref name="fileName"/> is an invalid file path.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file path could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// Importing keys is not supported by this cryptography context.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		public virtual async Task ImportAsync (string fileName, string password, CancellationToken cancellationToken = default)
		{
			if (fileName == null)
				throw new ArgumentNullException (nameof (fileName));

			if (password == null)
				throw new ArgumentNullException (nameof (password));

			cancellationToken.ThrowIfCancellationRequested ();

			using (var stream = File.OpenRead (fileName))
				await ImportAsync (stream, password, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Import a certificate.
		/// </summary>
		/// <remarks>
		/// Imports a certificate.
		/// </remarks>
		/// <param name="certificate">The certificate.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		public abstract void Import (X509Certificate certificate, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously import a certificate.
		/// </summary>
		/// <remarks>
		/// Asynchronously imports a certificate.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="certificate">The certificate.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		public virtual Task ImportAsync (X509Certificate certificate, CancellationToken cancellationToken = default)
		{
			Import (certificate, cancellationToken);
			return Task.FromResult (true);
		}

		/// <summary>
		/// Import a certificate.
		/// </summary>
		/// <remarks>
		/// Imports a certificate.
		/// </remarks>
		/// <param name="certificate">The certificate.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		public virtual void Import (X509Certificate2 certificate, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Asynchronously import a certificate.
		/// </summary>
		/// <remarks>
		/// Asynchronously imports a certificate.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="certificate">The certificate.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		public virtual Task ImportAsync (X509Certificate2 certificate, CancellationToken cancellationToken = default)
		{
			Import (certificate, cancellationToken);
			return Task.FromResult (true);
		}

		/// <summary>
		/// Import a certificate revocation list.
		/// </summary>
		/// <remarks>
		/// Imports a certificate revocation list.
		/// </remarks>
		/// <param name="crl">The certificate revocation list.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="crl"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		public abstract void Import (X509Crl crl, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously import a certificate revocation list.
		/// </summary>
		/// <remarks>
		/// Asynchronously imports a certificate revocation list.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="crl">The certificate revocation list.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="crl"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		public virtual Task ImportAsync (X509Crl crl, CancellationToken cancellationToken = default)
		{
			Import (crl, cancellationToken);
			return Task.FromResult (true);
		}

		async Task ImportAsync (Stream stream, bool doAsync, CancellationToken cancellationToken)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			cancellationToken.ThrowIfCancellationRequested ();

			using (var parser = new CmsSignedDataParser (stream)) {
				var certificates = parser.GetCertificates ();

				foreach (X509Certificate certificate in certificates.EnumerateMatches (null)) {
					if (doAsync)
						await ImportAsync (certificate, cancellationToken).ConfigureAwait (false);
					else
						Import (certificate, cancellationToken);
				}

				var crls = parser.GetCrls ();

				foreach (X509Crl crl in crls.EnumerateMatches (null)) {
					if (doAsync)
						await ImportAsync (crl, cancellationToken).ConfigureAwait (false);
					else
						Import (crl, cancellationToken);
				}
			}
		}

		/// <summary>
		/// Import certificates (as from a certs-only application/pkcs-mime part)
		/// from the specified stream.
		/// </summary>
		/// <remarks>
		/// Imports certificates (as from a certs-only application/pkcs-mime part)
		/// from the specified stream.
		/// </remarks>
		/// <param name="stream">The raw key data.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override void Import (Stream stream, CancellationToken cancellationToken = default)
		{
			ImportAsync (stream, false, cancellationToken).GetAwaiter ().GetResult ();
		}

		/// <summary>
		/// Asynchronously import certificates (as from a certs-only application/pkcs-mime part)
		/// from the specified stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously imports certificates (as from a certs-only application/pkcs-mime part)
		/// from the specified stream.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="stream">The raw key data.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override Task ImportAsync (Stream stream, CancellationToken cancellationToken = default)
		{
			return ImportAsync (stream, true, cancellationToken);
		}
	}
}

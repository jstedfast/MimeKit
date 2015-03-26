//
// SecureMimeContext.cs
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
using System.Collections;
using System.Collections.Generic;

using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.X509.Store;
using Org.BouncyCastle.Utilities.Date;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.Asn1.Smime;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.Ntt;
using Org.BouncyCastle.Asn1.Nist;

using MimeKit.IO;
using MimeKit.Utils;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A Secure MIME (S/MIME) cryptography context.
	/// </summary>
	/// <remarks>
	/// Generally speaking, applications should not use a <see cref="SecureMimeContext"/>
	/// directly, but rather via higher level APIs such as <see cref="MultipartSigned"/>
	/// and <see cref="ApplicationPkcs7Mime"/>.
	/// </remarks>
	public abstract class SecureMimeContext : CryptographyContext
	{
		internal const X509KeyUsageFlags DigitalSignatureKeyUsageFlags = X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.NonRepudiation;
		static readonly int EncryptionAlgorithmCount = Enum.GetValues (typeof (EncryptionAlgorithm)).Length;
		static readonly EncryptionAlgorithm[] DefaultEncryptionAlgorithmRank;
		int enabled;

		static SecureMimeContext ()
		{
			DefaultEncryptionAlgorithmRank = new [] {
				EncryptionAlgorithm.Camellia256,
				EncryptionAlgorithm.Aes256,
				EncryptionAlgorithm.Camellia192,
				EncryptionAlgorithm.Aes192,
				EncryptionAlgorithm.Camellia128,
				EncryptionAlgorithm.Aes128,
				EncryptionAlgorithm.Idea,
				EncryptionAlgorithm.Cast5,
				EncryptionAlgorithm.TripleDes,
				EncryptionAlgorithm.RC2128,
				EncryptionAlgorithm.RC264,
				EncryptionAlgorithm.Des,
				EncryptionAlgorithm.RC240
			};
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.SecureMimeContext"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Enables the following encryption algorithms by default:</para>
		/// <list type="bullet">
		/// <item><term><see cref="EncryptionAlgorithm.Camellia256"/></term></item>
		/// <item><term><see cref="EncryptionAlgorithm.Camellia192"/></term></item>
		/// <item><term><see cref="EncryptionAlgorithm.Camellia128"/></term></item>
		/// <item><term><see cref="EncryptionAlgorithm.Aes256"/></term></item>
		/// <item><term><see cref="EncryptionAlgorithm.Aes192"/></term></item>
		/// <item><term><see cref="EncryptionAlgorithm.Aes128"/></term></item>
		/// <item><term><see cref="EncryptionAlgorithm.Cast5"/></term></item>
		/// <item><term><see cref="EncryptionAlgorithm.Idea"/></term></item>
		/// <item><term><see cref="EncryptionAlgorithm.TripleDes"/></term></item>
		/// </list>
		/// </remarks>
		protected SecureMimeContext ()
		{
			foreach (var algorithm in DefaultEncryptionAlgorithmRank) {
				Enable (algorithm);

				// Don't enable anything weaker than Triple-DES by default
				if (algorithm == EncryptionAlgorithm.TripleDes)
					break;
			}
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
			get { return "application/pkcs7-signature"; }
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
			get { return "application/pkcs7-mime"; }
		}

		/// <summary>
		/// Gets the key exchange protocol.
		/// </summary>
		/// <remarks>
		/// Gets the key exchange protocol.
		/// </remarks>
		/// <value>The key exchange protocol.</value>
		public override string KeyExchangeProtocol {
			get { return "application/pkcs7-mime"; }
		}

		/// <summary>
		/// Checks whether or not the specified protocol is supported by the <see cref="CryptographyContext"/>.
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

			var type = protocol.ToLowerInvariant ().Split (new [] { '/' });
			if (type.Length != 2 || type[0] != "application")
				return false;

			if (type[1].StartsWith ("x-", StringComparison.Ordinal))
				type[1] = type[1].Substring (2);

			return type[1] == "pkcs7-signature" || type[1] == "pkcs7-mime" || type[1] == "pkcs7-keys";
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
		/// <item><term><see cref="DigestAlgorithm.MD5"/></term><description>md5</description></item>
		/// <item><term><see cref="DigestAlgorithm.Sha1"/></term><description>sha-1</description></item>
		/// <item><term><see cref="DigestAlgorithm.Sha224"/></term><description>sha-224</description></item>
		/// <item><term><see cref="DigestAlgorithm.Sha256"/></term><description>sha-256</description></item>
		/// <item><term><see cref="DigestAlgorithm.Sha384"/></term><description>sha-384</description></item>
		/// <item><term><see cref="DigestAlgorithm.Sha512"/></term><description>sha-512</description></item>
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
			case "md5":         return DigestAlgorithm.MD5;
			case "sha-1":       return DigestAlgorithm.Sha1;
			case "ripemd160":   return DigestAlgorithm.RipeMD160;
			case "md2":         return DigestAlgorithm.MD2;
			case "tiger192":    return DigestAlgorithm.Tiger192;
			case "haval-5-160": return DigestAlgorithm.Haval5160;
			case "sha-256":     return DigestAlgorithm.Sha256;
			case "sha-384":     return DigestAlgorithm.Sha384;
			case "sha-512":     return DigestAlgorithm.Sha512;
			case "sha-224":     return DigestAlgorithm.Sha224;
			case "md4":         return DigestAlgorithm.MD4;
			default:            return DigestAlgorithm.None;
			}
		}

		/// <summary>
		/// Gets the preferred rank order for the encryption algorithms; from the strongest to the weakest.
		/// </summary>
		/// <remarks>
		/// Gets the preferred rank order for the encryption algorithms; from the strongest to the weakest.
		/// </remarks>
		/// <value>The preferred encryption algorithm ranking.</value>
		protected virtual EncryptionAlgorithm[] EncryptionAlgorithmRank {
			get { return DefaultEncryptionAlgorithmRank; }
		}

		/// <summary>
		/// Gets the enabled encryption algorithms in ranked order.
		/// </summary>
		/// <remarks>
		/// Gets the enabled encryption algorithms in ranked order.
		/// </remarks>
		/// <value>The enabled encryption algorithms.</value>
		protected EncryptionAlgorithm[] EnabledEncryptionAlgorithms {
			get {
				var algorithms = new List<EncryptionAlgorithm> ();

				foreach (var algorithm in EncryptionAlgorithmRank) {
					if (IsEnabled (algorithm))
						algorithms.Add (algorithm);
				}

				return algorithms.ToArray ();
			}
		}

		/// <summary>
		/// Enables the encryption algorithm.
		/// </summary>
		/// <remarks>
		/// Enables the encryption algorithm.
		/// </remarks>
		/// <param name="algorithm">The encryption algorithm.</param>
		public void Enable (EncryptionAlgorithm algorithm)
		{
			enabled |= 1 << (int) algorithm;
		}

		/// <summary>
		/// Disables the encryption algorithm.
		/// </summary>
		/// <remarks>
		/// Disables the encryption algorithm.
		/// </remarks>
		/// <param name="algorithm">The encryption algorithm.</param>
		public void Disable (EncryptionAlgorithm algorithm)
		{
			enabled &= ~(1 << (int) algorithm);
		}

		/// <summary>
		/// Checks whether the specified encryption algorithm is enabled.
		/// </summary>
		/// <remarks>
		/// Determines whether the specified encryption algorithm is enabled.
		/// </remarks>
		/// <returns><c>true</c> if the specified encryption algorithm is enabled; otherwise, <c>false</c>.</returns>
		/// <param name="algorithm">Algorithm.</param>
		public bool IsEnabled (EncryptionAlgorithm algorithm)
		{
			return (enabled & (1 << (int) algorithm)) != 0;
		}

		/// <summary>
		/// Gets the X.509 certificate matching the specified selector.
		/// </summary>
		/// <remarks>
		/// Gets the first certificate that matches the specified selector.
		/// </remarks>
		/// <returns>The certificate on success; otherwise <c>null</c>.</returns>
		/// <param name="selector">The search criteria for the certificate.</param>
		protected abstract X509Certificate GetCertificate (IX509Selector selector);

		/// <summary>
		/// Gets the private key for the certificate matching the specified selector.
		/// </summary>
		/// <remarks>
		/// Gets the private key for the first certificate that matches the specified selector.
		/// </remarks>
		/// <returns>The private key on success; otherwise <c>null</c>.</returns>
		/// <param name="selector">The search criteria for the private key.</param>
		protected abstract AsymmetricKeyParameter GetPrivateKey (IX509Selector selector);

		/// <summary>
		/// Gets the trusted anchors.
		/// </summary>
		/// <remarks>
		/// A trusted anchor is a trusted root-level X.509 certificate,
		/// generally issued by a certificate authority (CA).
		/// </remarks>
		/// <returns>The trusted anchors.</returns>
		protected abstract HashSet GetTrustedAnchors ();

		/// <summary>
		/// Gets the intermediate certificates.
		/// </summary>
		/// <remarks>
		/// An intermediate certificate is any certificate that exists between the root
		/// certificate issued by a Certificate Authority (CA) and the certificate at
		/// the end of the chain.
		/// </remarks>
		/// <returns>The intermediate certificates.</returns>
		protected abstract IX509Store GetIntermediateCertificates ();

		/// <summary>
		/// Gets the certificate revocation lists.
		/// </summary>
		/// <remarks>
		/// A Certificate Revocation List (CRL) is a list of certificate serial numbers issued
		/// by a particular Certificate Authority (CA) that have been revoked, either by the CA
		/// itself or by the owner of the revoked certificate.
		/// </remarks>
		/// <returns>The certificate revocation lists.</returns>
		protected abstract IX509Store GetCertificateRevocationLists ();

		/// <summary>
		/// Gets the <see cref="CmsRecipient"/> for the specified mailbox.
		/// </summary>
		/// <remarks>
		/// <para>Constructs a <see cref="CmsRecipient"/> with the appropriate certificate and
		/// <see cref="CmsRecipient.EncryptionAlgorithms"/> for the specified mailbox.</para>
		/// <para>If the mailbox is a <see cref="SecureMailboxAddress"/>, the
		/// <see cref="SecureMailboxAddress.Fingerprint"/> property will be used instead of
		/// the mailbox address.</para>
		/// </remarks>
		/// <returns>A <see cref="CmsRecipient"/>.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		protected abstract CmsRecipient GetCmsRecipient (MailboxAddress mailbox);

		/// <summary>
		/// Gets a collection of CmsRecipients for the specified mailboxes.
		/// </summary>
		/// <remarks>
		/// Gets a collection of CmsRecipients for the specified mailboxes.
		/// </remarks>
		/// <returns>A <see cref="CmsRecipientCollection"/>.</returns>
		/// <param name="mailboxes">The mailboxes.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailboxes"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for one or more of the specified <paramref name="mailboxes"/> could not be found.
		/// </exception>
		protected CmsRecipientCollection GetCmsRecipients (IEnumerable<MailboxAddress> mailboxes)
		{
			if (mailboxes == null)
				throw new ArgumentNullException ("mailboxes");

			var recipients = new CmsRecipientCollection ();

			foreach (var mailbox in mailboxes)
				recipients.Add (GetCmsRecipient (mailbox));

			return recipients;
		}

		/// <summary>
		/// Gets the <see cref="CmsSigner"/> for the specified mailbox.
		/// </summary>
		/// <remarks>
		/// <para>Constructs a <see cref="CmsSigner"/> with the appropriate signing certificate
		/// for the specified mailbox.</para>
		/// <para>If the mailbox is a <see cref="SecureMailboxAddress"/>, the
		/// <see cref="SecureMailboxAddress.Fingerprint"/> property will be used instead of
		/// the mailbox address for database lookups.</para>
		/// </remarks>
		/// <returns>A <see cref="CmsSigner"/>.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <param name="digestAlgo">The preferred digest algorithm.</param>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		protected abstract CmsSigner GetCmsSigner (MailboxAddress mailbox, DigestAlgorithm digestAlgo);

		/// <summary>
		/// Updates the known S/MIME capabilities of the client used by the recipient that owns the specified certificate.
		/// </summary>
		/// <remarks>
		/// Updates the known S/MIME capabilities of the client used by the recipient that owns the specified certificate.
		/// </remarks>
		/// <param name="certificate">The certificate.</param>
		/// <param name="algorithms">The encryption algorithm capabilities of the client (in preferred order).</param>
		/// <param name="timestamp">The timestamp.</param>
		protected abstract void UpdateSecureMimeCapabilities (X509Certificate certificate, EncryptionAlgorithm[] algorithms, DateTime timestamp);

		/// <summary>
		/// Gets the OID for the digest algorithm.
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
		protected static string GetDigestOid (DigestAlgorithm digestAlgo)
		{
			switch (digestAlgo) {
			case DigestAlgorithm.MD5:        return PkcsObjectIdentifiers.MD5.Id;
			case DigestAlgorithm.Sha1:       return X509ObjectIdentifiers.IdSha1.Id;
			case DigestAlgorithm.MD2:        return PkcsObjectIdentifiers.MD2.Id;
			case DigestAlgorithm.Sha256:     return NistObjectIdentifiers.IdSha256.Id;
			case DigestAlgorithm.Sha384:     return NistObjectIdentifiers.IdSha384.Id;
			case DigestAlgorithm.Sha512:     return NistObjectIdentifiers.IdSha512.Id;
			case DigestAlgorithm.Sha224:     return NistObjectIdentifiers.IdSha224.Id;
			case DigestAlgorithm.MD4:        return PkcsObjectIdentifiers.MD4.Id;
			case DigestAlgorithm.RipeMD160:
			case DigestAlgorithm.DoubleSha:
			case DigestAlgorithm.Tiger192:
			case DigestAlgorithm.Haval5160:
				throw new NotSupportedException ();
			default:
				throw new ArgumentOutOfRangeException ();
			}
		}

		/// <summary>
		/// Compresses the specified stream.
		/// </summary>
		/// <remarks>
		/// Compresses the specified stream.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.Cryptography.ApplicationPkcs7Mime"/> instance
		/// containing the compressed content.</returns>
		/// <param name="stream">The stream to compress.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public ApplicationPkcs7Mime Compress (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			var compresser = new CmsCompressedDataGenerator ();
			var processable = new CmsProcessableInputStream (stream);
			var compressed = compresser.Generate (processable, CmsCompressedDataGenerator.ZLib);
			var encoded = compressed.GetEncoded ();

			return new ApplicationPkcs7Mime (SecureMimeType.CompressedData, new MemoryStream (encoded, false));
		}

		/// <summary>
		/// Decompress the specified stream.
		/// </summary>
		/// <remarks>
		/// Decompress the specified stream.
		/// </remarks>
		/// <returns>The decompressed mime part.</returns>
		/// <param name="stream">The stream to decompress.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public MimeEntity Decompress (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			var parser = new CmsCompressedDataParser (stream);
			var content = parser.GetContent ();

			return MimeEntity.Load (content.ContentStream);
		}

		Org.BouncyCastle.Asn1.Cms.AttributeTable AddSecureMimeCapabilities (Org.BouncyCastle.Asn1.Cms.AttributeTable signedAttributes)
		{
			var capabilities = new SmimeCapabilityVector ();

			foreach (var algorithm in EncryptionAlgorithmRank) {
				if (!IsEnabled (algorithm))
					continue;

				switch (algorithm) {
				case EncryptionAlgorithm.Camellia256:
					capabilities.AddCapability (NttObjectIdentifiers.IdCamellia256Cbc);
					break;
				case EncryptionAlgorithm.Camellia192:
					capabilities.AddCapability (NttObjectIdentifiers.IdCamellia192Cbc);
					break;
				case EncryptionAlgorithm.Camellia128:
					capabilities.AddCapability (NttObjectIdentifiers.IdCamellia128Cbc);
					break;
				case EncryptionAlgorithm.Aes256:
					capabilities.AddCapability (SmimeCapabilities.Aes256Cbc);
					break;
				case EncryptionAlgorithm.Aes192:
					capabilities.AddCapability (SmimeCapabilities.Aes192Cbc);
					break;
				case EncryptionAlgorithm.Aes128:
					capabilities.AddCapability (SmimeCapabilities.Aes128Cbc);
					break;
				case EncryptionAlgorithm.Idea:
					capabilities.AddCapability (SmimeCapabilities.IdeaCbc);
					break;
				case EncryptionAlgorithm.Cast5:
					capabilities.AddCapability (SmimeCapabilities.Cast5Cbc);
					break;
				case EncryptionAlgorithm.TripleDes:
					capabilities.AddCapability (SmimeCapabilities.DesEde3Cbc);
					break;
				case EncryptionAlgorithm.RC2128:
					capabilities.AddCapability (SmimeCapabilities.RC2Cbc, 128);
					break;
				case EncryptionAlgorithm.RC264:
					capabilities.AddCapability (SmimeCapabilities.RC2Cbc, 64);
					break;
				case EncryptionAlgorithm.RC240:
					capabilities.AddCapability (SmimeCapabilities.RC2Cbc, 40);
					break;
				case EncryptionAlgorithm.Des:
					capabilities.AddCapability (SmimeCapabilities.DesCbc);
					break;
				}
			}

			var attr = new SmimeCapabilitiesAttribute (capabilities);

			// populate our signed attributes with some S/MIME capabilities
			return signedAttributes.Add (attr.AttrType, attr.AttrValues[0]);
		}

		Stream Sign (CmsSigner signer, Stream content, bool encapsulate)
		{
			var signedData = new CmsSignedDataStreamGenerator ();

			signedData.AddSigner (signer.PrivateKey, signer.Certificate, GetDigestOid (signer.DigestAlgorithm),
				AddSecureMimeCapabilities (signer.SignedAttributes), signer.UnsignedAttributes);

			signedData.AddCertificates (signer.CertificateChain);

			var memory = new MemoryBlockStream ();

			using (var stream = signedData.Open (memory, encapsulate))
				content.CopyTo (stream, 4096);

			memory.Position = 0;

			return memory;
		}

		/// <summary>
		/// Cryptographically signs and encapsulates the content using the specified signer.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs and encapsulates the content using the specified signer.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.Cryptography.ApplicationPkcs7Mime"/> instance
		/// containing the detached signature data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="content">The content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public ApplicationPkcs7Mime EncapsulatedSign (CmsSigner signer, Stream content)
		{
			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (signer.Certificate == null)
				throw new ArgumentException ("No signer certificate specified.", "signer");

			if (signer.PrivateKey == null)
				throw new ArgumentException ("No private key specified.", "signer");

			if (content == null)
				throw new ArgumentNullException ("content");

			return new ApplicationPkcs7Mime (SecureMimeType.SignedData, Sign (signer, content, true));
		}

		/// <summary>
		/// Cryptographically signs and encapsulates the content using the specified signer and digest algorithm.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs and encapsulates the content using the specified signer and digest algorithm.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.Cryptography.ApplicationPkcs7Mime"/> instance
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
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for <paramref name="signer"/>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public virtual ApplicationPkcs7Mime EncapsulatedSign (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content)
		{
			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (content == null)
				throw new ArgumentNullException ("content");

			var cmsSigner = GetCmsSigner (signer, digestAlgo);

			return EncapsulatedSign (cmsSigner, content);
		}

		/// <summary>
		/// Cryptographically signs the content using the specified signer.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs the content using the specified signer.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.Cryptography.ApplicationPkcs7Signature"/> instance
		/// containing the detached signature data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="content">The content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public ApplicationPkcs7Signature Sign (CmsSigner signer, Stream content)
		{
			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (signer.Certificate == null)
				throw new ArgumentException ("No signer certificate specified.", "signer");

			if (signer.PrivateKey == null)
				throw new ArgumentException ("No private key specified.", "signer");

			if (content == null)
				throw new ArgumentNullException ("content");

			return new ApplicationPkcs7Signature (Sign (signer, content, false));
		}

		/// <summary>
		/// Cryptographically signs the content using the specified signer and digest algorithm.
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
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for <paramref name="signer"/>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override MimePart Sign (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content)
		{
			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (content == null)
				throw new ArgumentNullException ("content");

			var cmsSigner = GetCmsSigner (signer, digestAlgo);

			return Sign (cmsSigner, content);
		}

		X509Certificate GetCertificate (IX509Store store, SignerID signer)
		{
			var matches = store.GetMatches (signer);

			foreach (X509Certificate certificate in matches) {
				return certificate;
			}

			return GetCertificate (signer);
		}

		PkixCertPath BuildCertPath (HashSet anchors, IX509Store certificates, IX509Store crls, X509Certificate certificate, DateTime? signingTime)
		{
			var intermediate = new X509CertificateStore ();
			foreach (X509Certificate cert in certificates.GetMatches (null))
				intermediate.Add (cert);

			var selector = new X509CertStoreSelector ();
			selector.Certificate = certificate;

			var parameters = new PkixBuilderParameters (anchors, selector);
			parameters.AddStore (GetIntermediateCertificates ());
			parameters.AddStore (intermediate);

			var localCrls = GetCertificateRevocationLists ();
			parameters.AddStore (localCrls);
			parameters.AddStore (crls);

			parameters.ValidityModel = PkixParameters.ChainValidityModel;
			parameters.IsRevocationEnabled = true;

			if (signingTime.HasValue)
				parameters.Date = new DateTimeObject (signingTime.Value);

			var result = new PkixCertPathBuilder ().Build (parameters);

			return result.CertPath;
		}

		/// <summary>
		/// Attempts to map a <see cref="Org.BouncyCastle.Asn1.X509.AlgorithmIdentifier"/>
		/// to a <see cref="EncryptionAlgorithm"/>.
		/// </summary>
		/// <remarks>
		/// Attempts to map a <see cref="Org.BouncyCastle.Asn1.X509.AlgorithmIdentifier"/>
		/// to a <see cref="EncryptionAlgorithm"/>.
		/// </remarks>
		/// <returns><c>true</c> if the algorithm identifier was successfully mapped; <c>false</c> otherwise.</returns>
		/// <param name="identifier">The algorithm identifier.</param>
		/// <param name="algorithm">The encryption algorithm.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="identifier"/> is <c>null</c>.
		/// </exception>
		protected static bool TryGetEncryptionAlgorithm (AlgorithmIdentifier identifier, out EncryptionAlgorithm algorithm)
		{
			if (identifier == null)
				throw new ArgumentNullException ("identifier");

			if (identifier.ObjectID.Id == CmsEnvelopedGenerator.Aes256Cbc) {
				algorithm = EncryptionAlgorithm.Aes256;
				return true;
			}

			if (identifier.ObjectID.Id == CmsEnvelopedGenerator.Aes192Cbc) {
				algorithm = EncryptionAlgorithm.Aes192;
				return true;
			}

			if (identifier.ObjectID.Id == CmsEnvelopedGenerator.Aes128Cbc) {
				algorithm = EncryptionAlgorithm.Aes128;
				return true;
			}

			if (identifier.ObjectID.Id == CmsEnvelopedGenerator.Camellia256Cbc) {
				algorithm = EncryptionAlgorithm.Camellia256;
				return true;
			}

			if (identifier.ObjectID.Id == CmsEnvelopedGenerator.Camellia192Cbc) {
				algorithm = EncryptionAlgorithm.Camellia192;
				return true;
			}

			if (identifier.ObjectID.Id == CmsEnvelopedGenerator.Camellia128Cbc) {
				algorithm = EncryptionAlgorithm.Camellia128;
				return true;
			}

			if (identifier.ObjectID.Id == CmsEnvelopedGenerator.Cast5Cbc) {
				algorithm = EncryptionAlgorithm.Cast5;
				return true;
			}

			if (identifier.ObjectID.Id == CmsEnvelopedGenerator.DesEde3Cbc) {
				algorithm = EncryptionAlgorithm.TripleDes;
				return true;
			}

			if (identifier.ObjectID.Id == SmimeCapability.DesCbc.Id) {
				algorithm = EncryptionAlgorithm.Des;
				return true;
			}

			if (identifier.ObjectID.Id == CmsEnvelopedGenerator.IdeaCbc) {
				algorithm = EncryptionAlgorithm.Idea;
				return true;
			}

			if (identifier.ObjectID.Id == CmsEnvelopedGenerator.RC2Cbc) {
				var param = (DerInteger) identifier.Parameters;
				int bits = param.Value.IntValue;

				switch (bits) {
				case 128: algorithm = EncryptionAlgorithm.RC2128; return true;
				case 64: algorithm = EncryptionAlgorithm.RC264; return true;
				case 40: algorithm = EncryptionAlgorithm.RC240; return true;
				}
			}

			algorithm = EncryptionAlgorithm.RC240;

			return false;
		}

		static DateTime ToAdjustedDateTime (DerUtcTime time)
		{
			try {
				return time.ToAdjustedDateTime ();
			} catch {
				return DateUtils.Parse (time.AdjustedTimeString, "yyyyMMddHHmmsszzz");
			}
		}

		DigitalSignatureCollection GetDigitalSignatures (CmsSignedDataParser parser)
		{
			var certificates = parser.GetCertificates ("Collection");
			var signatures = new List<IDigitalSignature> ();
			var crls = parser.GetCrls ("Collection");
			var store = parser.GetSignerInfos ();

			// FIXME: we might not want to import these...
			foreach (X509Crl crl in crls.GetMatches (null))
				Import (crl);

			foreach (SignerInformation signerInfo in store.GetSigners ()) {
				var certificate = GetCertificate (certificates, signerInfo.SignerID);
				var signature = new SecureMimeDigitalSignature (signerInfo);
				var algorithms = new List<EncryptionAlgorithm> ();
				DateTime? signedDate = null;

				if (signerInfo.SignedAttributes != null) {
					Asn1EncodableVector vector = signerInfo.SignedAttributes.GetAll (CmsAttributes.SigningTime);
					foreach (Org.BouncyCastle.Asn1.Cms.Attribute attr in vector) {
						var signingTime = (DerUtcTime) ((DerSet) attr.AttrValues)[0];
						signature.CreationDate = ToAdjustedDateTime (signingTime);
						signedDate = signature.CreationDate;
						break;
					}

					vector = signerInfo.SignedAttributes.GetAll (SmimeAttributes.SmimeCapabilities);
					foreach (Org.BouncyCastle.Asn1.Cms.Attribute attr in vector) {
						foreach (Asn1Sequence sequence in attr.AttrValues) {
							for (int i = 0; i < sequence.Count; i++) {
								var identifier = AlgorithmIdentifier.GetInstance (sequence[i]);
								EncryptionAlgorithm algorithm;

								if (TryGetEncryptionAlgorithm (identifier, out algorithm))
									algorithms.Add (algorithm);
							}
						}
					}

					signature.EncryptionAlgorithms = algorithms.ToArray ();
				}

				if (certificate != null) {
					signature.SignerCertificate = new SecureMimeDigitalCertificate (certificate);
					if (algorithms.Count > 0 && signedDate != null)
						UpdateSecureMimeCapabilities (certificate, signature.EncryptionAlgorithms, signedDate.Value);
					else
						Import (certificate);
				}

				var anchors = GetTrustedAnchors ();

				try {
					signature.Chain = BuildCertPath (anchors, certificates, crls, certificate, signedDate);
				} catch (Exception ex) {
					signature.ChainException = ex;
				}

				signatures.Add (signature);
			}

			return new DigitalSignatureCollection (signatures);
		}

		/// <summary>
		/// Verifies the specified content using the detached signatureData.
		/// </summary>
		/// <remarks>
		/// Verifies the specified content using the detached signatureData.
		/// </remarks>
		/// <returns>A list of the digital signatures.</returns>
		/// <param name="content">The content.</param>
		/// <param name="signatureData">The detached signature data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signatureData"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override DigitalSignatureCollection Verify (Stream content, Stream signatureData)
		{
			if (content == null)
				throw new ArgumentNullException ("content");

			if (signatureData == null)
				throw new ArgumentNullException ("signatureData");

			var parser = new CmsSignedDataParser (new CmsTypedStream (content), signatureData);

			parser.GetSignedContent ().Drain ();

			return GetDigitalSignatures (parser);
		}

		/// <summary>
		/// Verifies the digital signatures of the specified signedData and extract the original content.
		/// </summary>
		/// <remarks>
		/// Verifies the digital signatures of the specified signedData and extract the original content.
		/// </remarks>
		/// <returns>The list of digital signatures.</returns>
		/// <param name="signedData">The signed data.</param>
		/// <param name="entity">The unencapsulated entity.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="signedData"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public DigitalSignatureCollection Verify (Stream signedData, out MimeEntity entity)
		{
			if (signedData == null)
				throw new ArgumentNullException ("signedData");

			var parser = new CmsSignedDataParser (signedData);
			var signed = parser.GetSignedContent ();

			entity = MimeEntity.Load (signed.ContentStream);

			return GetDigitalSignatures (parser);
		}

		class VoteComparer : IComparer<int>
		{
			#region IComparer implementation
			public int Compare (int x, int y)
			{
				return y - x;
			}
			#endregion
		}

		/// <summary>
		/// Gets the preferred encryption algorithm to use for encrypting to the specified recipients.
		/// </summary>
		/// <remarks>
		/// <para>Gets the preferred encryption algorithm to use for encrypting to the specified recipients
		/// based on the encryption algorithms supported by each of the recipients, the
		/// <see cref="EnabledEncryptionAlgorithms"/>, and the <see cref="EncryptionAlgorithmRank"/>.</para>
		/// <para>If the supported encryption algorithms are unknown for any recipient, it is assumed that
		/// the recipient supports at least the Triple-DES encryption algorithm.</para>
		/// </remarks>
		/// <returns>The preferred encryption algorithm.</returns>
		/// <param name="recipients">The recipients.</param>
		protected virtual EncryptionAlgorithm GetPreferredEncryptionAlgorithm (CmsRecipientCollection recipients)
		{
			var votes = new int[EncryptionAlgorithmCount];

			foreach (var recipient in recipients) {
				int cast = EncryptionAlgorithmCount;

				foreach (var algorithm in recipient.EncryptionAlgorithms) {
					votes[(int) algorithm] += cast;
					cast--;
				}
			}

			// Starting with S/MIME v3 (published in 1999), Triple-DES is a REQUIRED algorithm.
			// S/MIME v2.x and older only required RC2/40, but SUGGESTED Triple-DES.
			// Considering the fact that Bruce Schneier was able to write a
			// screensaver that could crack RC2/40 back in the late 90's, let's
			// not default to anything weaker than Triple-DES...
			EncryptionAlgorithm chosen = EncryptionAlgorithm.TripleDes;
			int nvotes = 0;

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

		Stream Envelope (CmsRecipientCollection recipients, Stream content)
		{
			var cms = new CmsEnvelopedDataGenerator ();
			int count = 0;

			foreach (var recipient in recipients) {
				cms.AddKeyTransRecipient (recipient.Certificate);
				count++;
			}

			if (count == 0)
				throw new ArgumentException ("No recipients specified.", "recipients");

			var input = new CmsProcessableInputStream (content);
			CmsEnvelopedData envelopedData;

			switch (GetPreferredEncryptionAlgorithm (recipients)) {
			case EncryptionAlgorithm.Camellia256:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.Camellia256Cbc);
				break;
			case EncryptionAlgorithm.Camellia192:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.Camellia192Cbc);
				break;
			case EncryptionAlgorithm.Camellia128:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.Camellia128Cbc);
				break;
			case EncryptionAlgorithm.Aes256:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.Aes256Cbc);
				break;
			case EncryptionAlgorithm.Aes192:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.Aes192Cbc);
				break;
			case EncryptionAlgorithm.Aes128:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.Aes128Cbc);
				break;
			case EncryptionAlgorithm.Cast5:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.Cast5Cbc);
				break;
			case EncryptionAlgorithm.Idea:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.IdeaCbc);
				break;
			case EncryptionAlgorithm.RC2128:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.RC2Cbc, 128);
				break;
			case EncryptionAlgorithm.RC264:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.RC2Cbc, 64);
				break;
			case EncryptionAlgorithm.RC240:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.RC2Cbc, 40);
				break;
			default:
				envelopedData = cms.Generate (input, CmsEnvelopedGenerator.DesEde3Cbc);
				break;
			}

			return new MemoryStream (envelopedData.GetEncoded (), false);
		}

		/// <summary>
		/// Encrypts the specified content for the specified recipients.
		/// </summary>
		/// <remarks>
		/// Encrypts the specified content for the specified recipients.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.Cryptography.ApplicationPkcs7Mime"/> instance
		/// containing the encrypted content.</returns>
		/// <param name="recipients">The recipients.</param>
		/// <param name="content">The content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public ApplicationPkcs7Mime Encrypt (CmsRecipientCollection recipients, Stream content)
		{
			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (content == null)
				throw new ArgumentNullException ("content");

			return new ApplicationPkcs7Mime (SecureMimeType.EnvelopedData, Envelope (recipients, content));
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
		/// A certificate for one or more of the <paramref name="recipients"/> could not be found.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override MimePart Encrypt (IEnumerable<MailboxAddress> recipients, Stream content)
		{
			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (content == null)
				throw new ArgumentNullException ("content");

			return Encrypt (GetCmsRecipients (recipients), content);
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
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override MimeEntity Decrypt (Stream encryptedData)
		{
			if (encryptedData == null)
				throw new ArgumentNullException ("encryptedData");

			var parser = new CmsEnvelopedDataParser (encryptedData);
			var recipients = parser.GetRecipientInfos ();
			var algorithm = parser.EncryptionAlgorithmID;
			AsymmetricKeyParameter key;

			foreach (RecipientInformation recipient in recipients.GetRecipients ()) {
				if ((key = GetPrivateKey (recipient.RecipientID)) == null)
					continue;

				var content = recipient.GetContent (key);
				var memory = new MemoryStream (content, false);

				return MimeEntity.Load (memory, true);
			}

			throw new CmsException ("A suitable private key could not be found for decrypting.");
		}

		/// <summary>
		/// Imports certificates and keys from a pkcs12-encoded stream.
		/// </summary>
		/// <remarks>
		/// Imports certificates and keys from a pkcs12-encoded stream.
		/// </remarks>
		/// <param name="stream">The raw certificate and key data.</param>
		/// <param name="password">The password to unlock the stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// Importing keys is not supported by this cryptography context.
		/// </exception>
		public abstract void Import (Stream stream, string password);

		/// <summary>
		/// Exports the certificates for the specified mailboxes.
		/// </summary>
		/// <remarks>
		/// Exports the certificates for the specified mailboxes.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.Cryptography.ApplicationPkcs7Mime"/> instance containing
		/// the exported keys.</returns>
		/// <param name="mailboxes">The mailboxes.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailboxes"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// No mailboxes were specified.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for one or more of the <paramref name="mailboxes"/> could not be found.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override MimePart Export (IEnumerable<MailboxAddress> mailboxes)
		{
			if (mailboxes == null)
				throw new ArgumentNullException ("mailboxes");

			var certificates = new X509CertificateStore ();
			int count = 0;

			foreach (var mailbox in mailboxes) {
				var recipient = GetCmsRecipient (mailbox);
				certificates.Add (recipient.Certificate);
				count++;
			}

			if (count == 0)
				throw new ArgumentException ("No mailboxes specified.", "mailboxes");

			var cms = new CmsSignedDataStreamGenerator ();
			var memory = new MemoryBlockStream ();

			cms.AddCertificates (certificates);
			cms.Open (memory).Close ();
			memory.Position = 0;

			return new ApplicationPkcs7Mime (SecureMimeType.CertsOnly, memory);
		}

		/// <summary>
		/// Imports the specified certificate.
		/// </summary>
		/// <remarks>
		/// Imports the specified certificate.
		/// </remarks>
		/// <param name="certificate">The certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public abstract void Import (X509Certificate certificate);

		/// <summary>
		/// Imports the specified certificate revocation list.
		/// </summary>
		/// <remarks>
		/// Imports the specified certificate revocation list.
		/// </remarks>
		/// <param name="crl">The certificate revocation list.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="crl"/> is <c>null</c>.
		/// </exception>
		public abstract void Import (X509Crl crl);

		/// <summary>
		/// Imports certificates (as from a certs-only application/pkcs-mime part)
		/// from the specified stream.
		/// </summary>
		/// <remarks>
		/// Imports certificates (as from a certs-only application/pkcs-mime part)
		/// from the specified stream.
		/// </remarks>
		/// <param name="stream">The raw key data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override void Import (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			var parser = new CmsSignedDataParser (stream);
			var certificates = parser.GetCertificates ("Collection");

			foreach (X509Certificate certificate in certificates.GetMatches (null))
				Import (certificate);

			var crls = parser.GetCrls ("Collection");

			foreach (X509Crl crl in crls.GetMatches (null))
				Import (crl);
		}
	}
}

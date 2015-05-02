//
// CmsSigner.cs
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
using System.Collections.Generic;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Security;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An S/MIME signer.
	/// </summary>
	/// <remarks>
	/// If the X.509 certificate is known for the signer, you may wish to use a
	/// <see cref="CmsSigner"/> as opposed to having the <see cref="CryptographyContext"/>
	/// do its own certificate lookup for the signer's <see cref="MailboxAddress"/>.
	/// </remarks>
	public class CmsSigner
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.CmsSigner"/> class.
		/// </summary>
		/// <remarks>
		/// <para>The initial value of the <see cref="DigestAlgorithm"/> will be set to
		/// <see cref="MimeKit.Cryptography.DigestAlgorithm.Sha1"/> and both the
		/// <see cref="SignedAttributes"/> and <see cref="UnsignedAttributes"/> properties
		/// will be initialized to empty tables.</para>
		/// </remarks>
		CmsSigner ()
		{
			UnsignedAttributes = new AttributeTable (new Dictionary<DerObjectIdentifier, Asn1Encodable> ());
			SignedAttributes = new AttributeTable (new Dictionary<DerObjectIdentifier, Asn1Encodable> ());
			DigestAlgorithm = DigestAlgorithm.Sha1;
		}

		static void CheckCertificateCanBeUsedForSigning (X509Certificate certificate)
		{
			var flags = certificate.GetKeyUsageFlags ();

			if (flags != X509KeyUsageFlags.None && (flags & SecureMimeContext.DigitalSignatureKeyUsageFlags) == 0)
				throw new ArgumentException ("The certificate cannot be used for signing.");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.CmsSigner"/> class.
		/// </summary>
		/// <remarks>
		/// <para>The initial value of the <see cref="DigestAlgorithm"/> will be set to
		/// <see cref="MimeKit.Cryptography.DigestAlgorithm.Sha1"/> and both the
		/// <see cref="SignedAttributes"/> and <see cref="UnsignedAttributes"/> properties
		/// will be initialized to empty tables.</para>
		/// </remarks>
		/// <param name="chain">The chain of certificates starting with the signer's certificate back to the root.</param>
		/// <param name="key">The signer's private key.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="chain"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="key"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="chain"/> did not contain any certificates.</para>
		/// <para>-or-</para>
		/// <para>The certificate cannot be used for signing.</para>
		/// <para>-or-</para>
		/// <para><paramref name="key"/> is not a private key.</para>
		/// </exception>
		public CmsSigner (IEnumerable<X509CertificateEntry> chain, AsymmetricKeyParameter key) : this ()
		{
			if (chain == null)
				throw new ArgumentNullException ("chain");

			if (key == null)
				throw new ArgumentNullException ("key");

			CertificateChain = new X509CertificateChain ();
			foreach (var entry in chain) {
				CertificateChain.Add (entry.Certificate);
				if (Certificate == null)
					Certificate = entry.Certificate;
			}

			if (CertificateChain.Count == 0)
				throw new ArgumentException ("The certificate chain was empty.", "chain");

			CheckCertificateCanBeUsedForSigning (Certificate);

			if (!key.IsPrivate)
				throw new ArgumentException ("The key must be a private key.", "key");

			PrivateKey = key;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.CmsSigner"/> class.
		/// </summary>
		/// <remarks>
		/// <para>The initial value of the <see cref="MimeKit.Cryptography.DigestAlgorithm"/> will
		/// be set to <see cref="MimeKit.Cryptography.DigestAlgorithm.Sha1"/> and both the
		/// <see cref="SignedAttributes"/> and <see cref="UnsignedAttributes"/> properties will be
		/// initialized to empty tables.</para>
		/// </remarks>
		/// <param name="certificate">The signer's certificate.</param>
		/// <param name="key">The signer's private key.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="certificate"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="key"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="certificate"/> cannot be used for signing.</para>
		/// <para>-or-</para>
		/// <para><paramref name="key"/> is not a private key.</para>
		/// </exception>
		public CmsSigner (X509Certificate certificate, AsymmetricKeyParameter key) : this ()
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			CheckCertificateCanBeUsedForSigning (certificate);

			if (key == null)
				throw new ArgumentNullException ("key");

			if (!key.IsPrivate)
				throw new ArgumentException ("The key must be a private key.", "key");

			CertificateChain = new X509CertificateChain ();
			CertificateChain.Add (certificate);
			Certificate = certificate;
			PrivateKey = key;
		}

#if !PORTABLE
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.CmsSigner"/> class.
		/// </summary>
		/// <remarks>
		/// <para>The initial value of the <see cref="MimeKit.Cryptography.DigestAlgorithm"/> will
		/// be set to <see cref="MimeKit.Cryptography.DigestAlgorithm.Sha1"/> and both the
		/// <see cref="SignedAttributes"/> and <see cref="UnsignedAttributes"/> properties will be
		/// initialized to empty tables.</para>
		/// </remarks>
		/// <param name="certificate">The signer's certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="certificate"/> cannot be used for signing.
		/// </exception>
		public CmsSigner (System.Security.Cryptography.X509Certificates.X509Certificate2 certificate) : this ()
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			if (!certificate.HasPrivateKey)
				throw new ArgumentException ("The certificate does not contain a private key.", "certificate");

			var cert = DotNetUtilities.FromX509Certificate (certificate);
			var key = DotNetUtilities.GetKeyPair (certificate.PrivateKey);

			CheckCertificateCanBeUsedForSigning (cert);

			CertificateChain = new X509CertificateChain ();
			CertificateChain.Add (cert);
			Certificate = cert;
			PrivateKey = key.Private;
		}
#endif

		/// <summary>
		/// Gets the signer's certificate.
		/// </summary>
		/// <remarks>
		/// The signer's certificate that contains a public key that can be used for
		/// verifying the digital signature.
		/// </remarks>
		/// <value>The signer's certificate.</value>
		public X509Certificate Certificate {
			get; private set;
		}

		/// <summary>
		/// Gets the certificate chain.
		/// </summary>
		/// <remarks>
		/// Gets the certificate chain.
		/// </remarks>
		/// <value>The certificate chain.</value>
		public X509CertificateChain CertificateChain {
			get; private set;
		}

		/// <summary>
		/// Gets or sets the digest algorithm.
		/// </summary>
		/// <remarks>
		/// Specifies which digest algorithm to use to generate the
		/// cryptographic hash of the content being signed.
		/// </remarks>
		/// <value>The digest algorithm.</value>
		public DigestAlgorithm DigestAlgorithm {
			get; set;
		}

		/// <summary>
		/// Gets the private key.
		/// </summary>
		/// <remarks>
		/// The private key used for signing.
		/// </remarks>
		/// <value>The private key.</value>
		public AsymmetricKeyParameter PrivateKey {
			get; private set;
		}

		/// <summary>
		/// Gets or sets the signed attributes.
		/// </summary>
		/// <remarks>
		/// A table of attributes that should be included in the signature.
		/// </remarks>
		/// <value>The signed attributes.</value>
		public AttributeTable SignedAttributes {
			get; set;
		}

		/// <summary>
		/// Gets or sets the unsigned attributes.
		/// </summary>
		/// <remarks>
		/// A table of attributes that should not be signed in the signature,
		/// but still included in transport.
		/// </remarks>
		/// <value>The unsigned attributes.</value>
		public AttributeTable UnsignedAttributes {
			get; set;
		}
	}
}

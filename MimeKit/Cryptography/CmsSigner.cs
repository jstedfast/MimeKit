//
// CmsSigner.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Jeffrey Stedfast
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

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Asn1.Ntt;
using Org.BouncyCastle.Asn1.Smime;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An S/MIME signer.
	/// </summary>
	public sealed class CmsSigner
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.CmsSigner"/> class.
		/// </summary>
		public CmsSigner ()
		{
			UnsignedAttributes = new AttributeTable (new Dictionary<DerObjectIdentifier, Asn1Encodable> ());
			SignedAttributes = new AttributeTable (new Dictionary<DerObjectIdentifier, Asn1Encodable> ());
			DigestAlgorithm = DigestAlgorithm.Sha1;

			var capabilities = new SmimeCapabilityVector ();
			capabilities.AddCapability (NttObjectIdentifiers.IdCamellia256Cbc);
			capabilities.AddCapability (NttObjectIdentifiers.IdCamellia192Cbc);
			capabilities.AddCapability (NttObjectIdentifiers.IdCamellia128Cbc);
			capabilities.AddCapability (SmimeCapabilities.Aes256Cbc);
			capabilities.AddCapability (SmimeCapabilities.Aes192Cbc);
			capabilities.AddCapability (SmimeCapabilities.Aes128Cbc);
			capabilities.AddCapability (SmimeCapabilities.DesEde3Cbc);
			capabilities.AddCapability (SmimeCapabilities.IdeaCbc);
			capabilities.AddCapability (SmimeCapabilities.Cast5Cbc);
			capabilities.AddCapability (SmimeCapabilities.RC2Cbc, 128);

			// For compatibility with older S/MIME implementations
			capabilities.AddCapability (SmimeCapabilities.DesCbc);
			capabilities.AddCapability (SmimeCapabilities.RC2Cbc, 64);
			capabilities.AddCapability (SmimeCapabilities.RC2Cbc, 40);

			var attr = new SmimeCapabilitiesAttribute (capabilities);

			// populate our signed attributes with some S/MIME capabilities
			SignedAttributes = SignedAttributes.Add (attr.AttrType, attr.AttrValues[0]);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.CmsSigner"/> class.
		/// </summary>
		/// <param name="chain">The chain of certificates starting with the signer's certificate back to the root.</param>
		/// <param name="key">The signer's private key.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="chain"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="key"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="chain"/> did not contain any certificates.
		/// </exception>
		public CmsSigner (IEnumerable<X509CertificateEntry> chain, AsymmetricKeyEntry key) : this ()
		{
			if (chain == null)
				throw new ArgumentNullException ("chain");

			if (key == null)
				throw new ArgumentNullException ("key");

			CertificateChain = new List<X509Certificate> ();
			foreach (var entry in chain) {
				CertificateChain.Add (entry.Certificate);
				if (Certificate == null)
					Certificate = entry.Certificate;
			}

			if (CertificateChain.Count == 0)
				throw new ArgumentException ("The certificate chain was empty.", "chain");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.CmsSigner"/> class.
		/// </summary>
		/// <param name="certificate">The signer's certificate.</param>
		/// <param name="key">The signer's private key.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="certificate"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="key"/> is <c>null</c>.</para>
		/// </exception>
		public CmsSigner (X509Certificate certificate, AsymmetricKeyParameter key) : this ()
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			if (key == null)
				throw new ArgumentNullException ("key");

			CertificateChain = new List<X509Certificate> ();
			CertificateChain.Add (certificate);
			Certificate = certificate;
			PrivateKey = key;
		}

		/// <summary>
		/// Gets or sets the signer's certificate.
		/// </summary>
		/// <value>The signer's certificate.</value>
		public X509Certificate Certificate {
			get; set;
		}

		/// <summary>
		/// Gets or sets the certificate chain.
		/// </summary>
		/// <value>The certificate chain.</value>
		public IList<X509Certificate> CertificateChain {
			get; set;
		}

		/// <summary>
		/// Gets or sets the digest algorithm.
		/// </summary>
		/// <value>The digest algorithm.</value>
		public DigestAlgorithm DigestAlgorithm {
			get; set;
		}

		/// <summary>
		/// Gets or sets the private key.
		/// </summary>
		/// <value>The private key.</value>
		public AsymmetricKeyParameter PrivateKey {
			get; set;
		}

		/// <summary>
		/// Gets or sets the signed attributes.
		/// </summary>
		/// <value>The signed attributes.</value>
		public AttributeTable SignedAttributes {
			get; set;
		}

		/// <summary>
		/// Gets or sets the unsigned attributes.
		/// </summary>
		/// <value>The unsigned attributes.</value>
		public AttributeTable UnsignedAttributes {
			get; set;
		}
	}
}

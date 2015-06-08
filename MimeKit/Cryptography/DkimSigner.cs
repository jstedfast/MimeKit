//
// DkimSigner.cs
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

#if !PORTABLE
using X509Certificate2 = System.Security.Cryptography.X509Certificates.X509Certificate2;
#endif

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Security;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A DKIM signer.
	/// </summary>
	/// <remarks>
	/// A DKIM signer.
	/// </remarks>
	public class DkimSigner
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.DkimSigner"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="DkimSigner"/>.
		/// </remarks>
		/// <param name="certificate">The signer's certificate.</param>
		/// <param name="key">The signer's private key.</param>
		/// <param name="domain">The domain that the signer represents.</param>
		/// <param name="selector">The selector subdividing the domain.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="certificate"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="key"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="domain"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="selector"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="certificate"/> cannot be used for signing.</para>
		/// <para>-or-</para>
		/// <para><paramref name="key"/> is not a private key.</para>
		/// </exception>
		public DkimSigner (X509Certificate certificate, AsymmetricKeyParameter key, string domain, string selector)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			if (key == null)
				throw new ArgumentNullException ("key");

			if (domain == null)
				throw new ArgumentNullException ("domain");

			if (selector == null)
				throw new ArgumentNullException ("selector");

			CheckCertificateCanBeUsedForSigning (certificate);

			if (!key.IsPrivate)
				throw new ArgumentException ("The key must be a private key.", "key");

			SignatureAlgorithm = DkimSignatureAlgorithm.RsaSha256;
			Certificate = certificate;
			Selector = selector;
			PrivateKey = key;
			Domain = domain;
		}

#if !PORTABLE
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.DkimSigner"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="DkimSigner"/>.
		/// </remarks>
		/// <param name="certificate">The signer's certificate.</param>
		/// <param name="domain">The domain that the signer represents.</param>
		/// <param name="selector">The selector subdividing the domain.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="certificate"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="domain"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="selector"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="certificate"/> cannot be used for signing.
		/// </exception>
		public DkimSigner (X509Certificate2 certificate, string domain, string selector)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			if (domain == null)
				throw new ArgumentNullException ("domain");

			if (!certificate.HasPrivateKey)
				throw new ArgumentException ("The certificate does not contain a private key.", "certificate");

			var cert = DotNetUtilities.FromX509Certificate (certificate);
			var key = DotNetUtilities.GetKeyPair (certificate.PrivateKey);

			CheckCertificateCanBeUsedForSigning (cert);

			SignatureAlgorithm = DkimSignatureAlgorithm.RsaSha256;
			PrivateKey = key.Private;
			Selector = selector;
			Certificate = cert;
			Domain = domain;
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
		/// Get the domain that the signer represents.
		/// </summary>
		/// <remarks>
		/// Gets the domain that the signer represents.
		/// </remarks>
		/// <value>The domain.</value>
		public string Domain {
			get; private set;
		}

		/// <summary>
		/// Get the selector subdividing the domain.
		/// </summary>
		/// <remarks>
		/// Gets the selector subdividing the domain.
		/// </remarks>
		/// <value>The selector.</value>
		public string Selector {
			get; private set;
		}

		/// <summary>
		/// Get or set the algorithm to use for signing.
		/// </summary>
		/// <remarks>
		/// Gets or sets the algorithm to use for signing.
		/// </remarks>
		/// <value>The signature algorithm.</value>
		public DkimSignatureAlgorithm SignatureAlgorithm {
			get; set;
		}

		/// <summary>
		/// Get or set the public key query method.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the public key query method.</para>
		/// <para>The value should be a colon-separated list of query methods used to
		/// retrieve the public key (plain-text; OPTIONAL, default is "dns/txt"). Each
		/// query method is of the form "type[/options]", where the syntax and
		/// semantics of the options depend on the type and specified options.</para>
		/// </remarks>
		/// <value>The public key query method.</value>
		public string QueryMethod {
			get; set;
		}

		static void CheckCertificateCanBeUsedForSigning (X509Certificate certificate)
		{
			var flags = certificate.GetKeyUsageFlags ();

			if (flags != X509KeyUsageFlags.None && (flags & SecureMimeContext.DigitalSignatureKeyUsageFlags) == 0)
				throw new ArgumentException ("The certificate cannot be used for signing.");
		}
	}
}

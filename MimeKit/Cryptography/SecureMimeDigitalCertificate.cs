//
// SecureMimeDigitalCertificate.cs
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
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An S/MIME digital certificate.
	/// </summary>
	public class SecureMimeDigitalCertificate : IDigitalCertificate
	{
		internal SecureMimeDigitalCertificate (SignerInfo signerInfo)
		{
			Certificate = signerInfo.Certificate;
		}

		SecureMimeDigitalCertificate ()
		{
		}

		/// <summary>
		/// Gets the <see cref="System.Security.Cryptography.X509Certificates.X509Certificate2" />.
		/// </summary>
		/// <value>The certificate.</value>
		public X509Certificate2 Certificate {
			get; private set;
		}

		/// <summary>
		/// Gets the chain status.
		/// </summary>
		/// <value>The chain status.</value>
		public X509ChainStatusFlags ChainStatus {
			get; internal set;
		}

		#region IDigitalCertificate implementation

		/// <summary>
		/// Gets the public key algorithm supported by the certificate.
		/// </summary>
		/// <value>The public key algorithm.</value>
		public PublicKeyAlgorithm PublicKeyAlgorithm {
			get; private set;
		}

		/// <summary>
		/// Gets the date that the certificate was created.
		/// </summary>
		/// <value>The creation date.</value>
		public DateTime CreationDate {
			get { return Certificate.NotBefore; }
		}

		/// <summary>
		/// Gets the expiration date of the certificate.
		/// </summary>
		/// <value>The expiration date.</value>
		public DateTime ExpirationDate {
			get { return Certificate.NotAfter; }
		}

		/// <summary>
		/// Gets the trust level for the certificate.
		/// </summary>
		/// <value>The trust level.</value>
		public TrustLevel TrustLevel {
			get; private set;
		}

		/// <summary>
		/// Gets the fingerprint of the certificate.
		/// </summary>
		/// <value>The fingerprint.</value>
		public string Fingerprint {
			get { return Certificate.Thumbprint; }
		}

		/// <summary>
		/// Gets the email address of the owner of the certificate.
		/// </summary>
		/// <value>The email address.</value>
		public string Email {
			get { return Certificate.GetNameInfo (X509NameType.EmailName, false); }
		}

		/// <summary>
		/// Gets the name of the owner of the certificate.
		/// </summary>
		/// <value>The name of the owner.</value>
		public string Name {
			get { return Certificate.GetNameInfo (X509NameType.SimpleName, false); }
		}

		#endregion
	}
}

//
// SecureMimeDigitalCertificate.cs
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

using Org.BouncyCastle.X509;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An S/MIME digital certificate.
	/// </summary>
	/// <remarks>
	/// An S/MIME digital certificate.
	/// </remarks>
	public class SecureMimeDigitalCertificate : IDigitalCertificate
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="SecureMimeDigitalCertificate"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="SecureMimeDigitalCertificate"/>.
		/// </remarks>
		/// <param name="certificate">An X.509 certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public SecureMimeDigitalCertificate (X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			Certificate = certificate;
			Fingerprint = certificate.GetFingerprint ();
			PublicKeyAlgorithm = certificate.GetPublicKeyAlgorithm ();
		}

		/// <summary>
		/// Get the X.509 certificate.
		/// </summary>
		/// <remarks>
		/// Gets the X.509 certificate.
		/// </remarks>
		/// <value>The certificate.</value>
		public X509Certificate Certificate {
			get; private set;
		}

//		/// <summary>
//		/// Gets the chain status.
//		/// </summary>
//		/// <value>The chain status.</value>
//		public X509ChainStatusFlags ChainStatus {
//			get; internal set;
//		}

		#region IDigitalCertificate implementation

		/// <summary>
		/// Gets the public key algorithm supported by the certificate.
		/// </summary>
		/// <remarks>
		/// Gets the public key algorithm supported by the certificate.
		/// </remarks>
		/// <value>The public key algorithm.</value>
		public PublicKeyAlgorithm PublicKeyAlgorithm {
			get; private set;
		}

		/// <summary>
		/// Gets the date that the certificate was created.
		/// </summary>
		/// <remarks>
		/// Gets the date that the certificate was created.
		/// </remarks>
		/// <value>The creation date.</value>
		public DateTime CreationDate {
			get { return Certificate.NotBefore.ToUniversalTime (); }
		}

		/// <summary>
		/// Gets the expiration date of the certificate.
		/// </summary>
		/// <remarks>
		/// Gets the expiration date of the certificate.
		/// </remarks>
		/// <value>The expiration date.</value>
		public DateTime ExpirationDate {
			get { return Certificate.NotAfter.ToUniversalTime (); }
		}

		/// <summary>
		/// Gets the fingerprint of the certificate.
		/// </summary>
		/// <remarks>
		/// Gets the fingerprint of the certificate.
		/// </remarks>
		/// <value>The fingerprint.</value>
		public string Fingerprint {
			get; private set;
		}

		/// <summary>
		/// Gets the email address of the owner of the certificate.
		/// </summary>
		/// <remarks>
		/// Gets the email address of the owner of the certificate.
		/// </remarks>
		/// <value>The email address.</value>
		public string Email {
			get { return Certificate.GetSubjectEmailAddress (); }
		}

		/// <summary>
		/// Gets the name of the owner of the certificate.
		/// </summary>
		/// <remarks>
		/// Gets the name of the owner of the certificate.
		/// </remarks>
		/// <value>The name of the owner.</value>
		public string Name {
			get { return Certificate.GetCommonName (); }
		}

		#endregion
	}
}

//
// SecureMimeDigitalCertificate.cs
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

using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An S/MIME digital certificate.
	/// </summary>
	/// <remarks>
	/// An S/MIME digital certificate.
	/// </remarks>
	public class SecureMimeDigitalCertificate : IDigitalCertificate
	{
		internal SecureMimeDigitalCertificate (X509Certificate certificate)
		{
			Certificate = certificate;

			var pubkey = certificate.GetPublicKey ();
			if (pubkey is DsaKeyParameters)
				PublicKeyAlgorithm = PublicKeyAlgorithm.Dsa;
			else if (pubkey is RsaKeyParameters)
				PublicKeyAlgorithm = PublicKeyAlgorithm.RsaGeneral;
			else if (pubkey is ElGamalKeyParameters)
				PublicKeyAlgorithm = PublicKeyAlgorithm.ElGamalGeneral;
			else if (pubkey is ECKeyParameters)
				PublicKeyAlgorithm = PublicKeyAlgorithm.EllipticCurve;
			else if (pubkey is DHKeyParameters)
				PublicKeyAlgorithm = PublicKeyAlgorithm.DiffieHellman;

			Fingerprint = certificate.GetFingerprint ();
		}

		SecureMimeDigitalCertificate ()
		{
		}

		/// <summary>
		/// Gets the <see cref="Org.BouncyCastle.X509.X509Certificate" />.
		/// </summary>
		/// <remarks>
		/// Gets the <see cref="Org.BouncyCastle.X509.X509Certificate" />.
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
			get { return Certificate.NotBefore; }
		}

		/// <summary>
		/// Gets the expiration date of the certificate.
		/// </summary>
		/// <remarks>
		/// Gets the expiration date of the certificate.
		/// </remarks>
		/// <value>The expiration date.</value>
		public DateTime ExpirationDate {
			get { return Certificate.NotAfter; }
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
			get { return Certificate.GetSubjectName (); }
		}

		#endregion
	}
}

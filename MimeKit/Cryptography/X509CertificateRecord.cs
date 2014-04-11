//
// X509CertificateRecord.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2014 Xamarin Inc. (www.xamarin.com)
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

namespace MimeKit.Cryptography {
	[Flags]
	public enum X509CertificateRecordFields {
		Id                = 1 << 0,
		Trusted           = 1 << 1,
		Algorithms        = 1 << 3,
		AlgorithmsUpdated = 1 << 4,
		Certificate       = 1 << 5,
		PrivateKey        = 1 << 6,

		// helpers
		CmsRecipient      = Algorithms | Certificate,
		CmsSigner         = Certificate | PrivateKey,
		PrivateKeyLookup  = Certificate | PrivateKey,
		UpdateAlgorithms  = Id | Algorithms | AlgorithmsUpdated,
		ImportPkcs12      = UpdateAlgorithms | Trusted | PrivateKey,
		All               = 0xff
	}

	/// <summary>
	/// An X.509 Certificate record.
	/// </summary>
	public class X509CertificateRecord
	{
		internal static readonly string[] ColumnNames = {
			"ID",
			"BASICCONSTRAINTS",
			"TRUSTED",
			"KEYUSAGE",
			"NOTBEFORE",
			"NOTAFTER",
			"ISSUERNAME",
			"SERIALNUMBER",
			"SUBJECTEMAIL",
			"FINGERPRINT",
			"ALGORITHMS",
			"ALGORITHMSUPDATED",
			"CERTIFICATE",
			"PRIVATEKEY"
		};

		/// <summary>
		/// Gets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		public int Id { get; internal set; }

		/// <summary>
		/// Gets the basic constraints of the certificate.
		/// </summary>
		/// <value>The basic constraints of the certificate.</value>
		public int BasicConstraints { get { return Certificate.GetBasicConstraints (); } }

		/// <summary>
		/// Gets or sets a value indicating whether the certificate is trusted.
		/// </summary>
		/// <value><c>true</c> if the certificate is trusted; otherwise, <c>false</c>.</value>
		public bool IsTrusted { get; set; }

		/// <summary>
		/// Gets or sets the X.509 key usage.
		/// </summary>
		/// <value>The X.509 key usage.</value>
		public X509KeyUsageFlags KeyUsage { get { return Certificate.GetKeyUsageFlags (); } }

		/// <summary>
		/// Gets the starting date and time where the certificate is valid.
		/// </summary>
		/// <value>The date and time.</value>
		public DateTime NotBefore { get { return Certificate.NotBefore; } }

		/// <summary>
		/// Gets the end date and time where the certificate is valid.
		/// </summary>
		/// <value>The date and time.</value>
		public DateTime NotAfter { get { return Certificate.NotAfter; } }

		/// <summary>
		/// Gets the certificate issuer's name.
		/// </summary>
		/// <value>The issuer's name.</value>
		public string IssuerName { get { return Certificate.IssuerDN.ToString (); } }

		/// <summary>
		/// Gets the serial number of the certificate.
		/// </summary>
		/// <value>The serial number.</value>
		public string SerialNumber { get { return Certificate.SerialNumber.ToString (); } }

		/// <summary>
		/// Gets the subject email address.
		/// </summary>
		/// <value>The subject email address.</value>
		public string SubjectEmail { get { return Certificate.GetSubjectEmailAddress (); } }

		/// <summary>
		/// Gets the fingerprint of the certificate.
		/// </summary>
		/// <value>The fingerprint.</value>
		public string Fingerprint { get { return Certificate.GetFingerprint (); } }

		/// <summary>
		/// Gets or sets the encryption algorithm capabilities.
		/// </summary>
		/// <value>The encryption algorithms.</value>
		public EncryptionAlgorithm[] Algorithms { get; set; }

		/// <summary>
		/// Gets or sets the date when the algorithms were last updated.
		/// </summary>
		/// <value>The date the algorithms were updated.</value>
		public DateTime AlgorithmsUpdated { get; set; }

		/// <summary>
		/// Gets the certificate.
		/// </summary>
		/// <value>The certificate.</value>
		public X509Certificate Certificate { get; internal set; }

		/// <summary>
		/// Gets the private key.
		/// </summary>
		/// <value>The private key.</value>
		public AsymmetricKeyParameter PrivateKey { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.X509CertificateRecord"/> class.
		/// </summary>
		/// <param name="certificate">The certificate.</param>
		/// <param name="key">The private key.</param>
		public X509CertificateRecord (X509Certificate certificate, AsymmetricKeyParameter key)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			if (key == null)
				throw new ArgumentNullException ("key");

			if (!key.IsPrivate)
				throw new ArgumentException ("The key must be private.", "key");

			AlgorithmsUpdated = DateTime.MinValue;
			Certificate = certificate;
			PrivateKey = key;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.X509CertificateRecord"/> class.
		/// </summary>
		/// <param name="certificate">The certificate.</param>
		public X509CertificateRecord (X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			AlgorithmsUpdated = DateTime.MinValue;
			Certificate = certificate;
		}

		internal X509CertificateRecord ()
		{
		}
	}
}

//
// X509CertificateRecord.cs
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// X.509 certificate record fields.
	/// </summary>
	/// <remarks>
	/// The record fields are used when querying the <see cref="IX509CertificateDatabase"/>
	/// for certificates.
	/// </remarks>
	[Flags]
	public enum X509CertificateRecordFields {
		/// <summary>
		/// The "id" field is typically just the ROWID in the database.
		/// </summary>
		Id                = 1 << 0,

		/// <summary>
		/// The "trusted" field is a boolean value indicating whether the certificate
		/// is trusted.
		/// </summary>
		Trusted           = 1 << 1,

		/// <summary>
		/// The "algorithms" field is used for storing the last known list of
		/// <see cref="EncryptionAlgorithm"/> values that are supported by the
		/// client associated with the certificate.
		/// </summary>
		Algorithms        = 1 << 3,

		/// <summary>
		/// The "algorithms updated" field is used to store the timestamp of the
		/// most recent update to the Algorithms field.
		/// </summary>
		AlgorithmsUpdated = 1 << 4,

		/// <summary>
		/// The "certificate" field is sued for storing the binary data of the actual
		/// certificate.
		/// </summary>
		Certificate       = 1 << 5,

		/// <summary>
		/// The "private key" field is used to store the encrypted binary data of the
		/// private key associated with the certificate, if available.
		/// </summary>
		PrivateKey        = 1 << 6,
	}

	/// <summary>
	/// An X.509 certificate record.
	/// </summary>
	/// <remarks>
	/// Represents an X.509 certificate record loaded from a database.
	/// </remarks>
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
		/// <remarks>
		/// The id is typically the ROWID of the certificate in the database and is not
		/// generally useful outside of the internals of the database implementation.
		/// </remarks>
		/// <value>The identifier.</value>
		public int Id { get; internal set; }

		/// <summary>
		/// Gets the basic constraints of the certificate.
		/// </summary>
		/// <remarks>
		/// Gets the basic constraints of the certificate.
		/// </remarks>
		/// <value>The basic constraints of the certificate.</value>
		public int BasicConstraints { get { return Certificate.GetBasicConstraints (); } }

		/// <summary>
		/// Gets or sets a value indicating whether the certificate is trusted.
		/// </summary>
		/// <remarks>
		/// Indiciates whether or not the certificate is trusted.
		/// </remarks>
		/// <value><c>true</c> if the certificate is trusted; otherwise, <c>false</c>.</value>
		public bool IsTrusted { get; set; }

		/// <summary>
		/// Gets the key usage flags for the certificate.
		/// </summary>
		/// <remarks>
		/// Gets the key usage flags for the certificate.
		/// </remarks>
		/// <value>The X.509 key usage.</value>
		public X509KeyUsageFlags KeyUsage { get { return Certificate.GetKeyUsageFlags (); } }

		/// <summary>
		/// Gets the starting date and time where the certificate is valid.
		/// </summary>
		/// <remarks>
		/// Gets the starting date and time where the certificate is valid.
		/// </remarks>
		/// <value>The date and time.</value>
		public DateTime NotBefore { get { return Certificate.NotBefore; } }

		/// <summary>
		/// Gets the end date and time where the certificate is valid.
		/// </summary>
		/// <remarks>
		/// Gets the end date and time where the certificate is valid.
		/// </remarks>
		/// <value>The date and time.</value>
		public DateTime NotAfter { get { return Certificate.NotAfter; } }

		/// <summary>
		/// Gets the certificate issuer's name.
		/// </summary>
		/// <remarks>
		/// Gets the certificate issuer's name.
		/// </remarks>
		/// <value>The issuer's name.</value>
		public string IssuerName { get { return Certificate.IssuerDN.ToString (); } }

		/// <summary>
		/// Gets the serial number of the certificate.
		/// </summary>
		/// <remarks>
		/// Gets the serial number of the certificate.
		/// </remarks>
		/// <value>The serial number.</value>
		public string SerialNumber { get { return Certificate.SerialNumber.ToString (); } }

		/// <summary>
		/// Gets the subject email address.
		/// </summary>
		/// <remarks>
		/// Gets the subject email address.
		/// </remarks>
		/// <value>The subject email address.</value>
		public string SubjectEmail { get { return Certificate.GetSubjectEmailAddress (); } }

		/// <summary>
		/// Gets the fingerprint of the certificate.
		/// </summary>
		/// <remarks>
		/// Gets the fingerprint of the certificate.
		/// </remarks>
		/// <value>The fingerprint.</value>
		public string Fingerprint { get { return Certificate.GetFingerprint (); } }

		/// <summary>
		/// Gets or sets the encryption algorithm capabilities.
		/// </summary>
		/// <remarks>
		/// Gets or sets the encryption algorithm capabilities.
		/// </remarks>
		/// <value>The encryption algorithms.</value>
		public EncryptionAlgorithm[] Algorithms { get; set; }

		/// <summary>
		/// Gets or sets the date when the algorithms were last updated.
		/// </summary>
		/// <remarks>
		/// Gets or sets the date when the algorithms were last updated.
		/// </remarks>
		/// <value>The date the algorithms were updated.</value>
		public DateTime AlgorithmsUpdated { get; set; }

		/// <summary>
		/// Gets the certificate.
		/// </summary>
		/// <remarks>
		/// Gets the certificate.
		/// </remarks>
		/// <value>The certificate.</value>
		public X509Certificate Certificate { get; internal set; }

		/// <summary>
		/// Gets the private key.
		/// </summary>
		/// <remarks>
		/// Gets the private key.
		/// </remarks>
		/// <value>The private key.</value>
		public AsymmetricKeyParameter PrivateKey { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.X509CertificateRecord"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new certificate record with a private key for storing in a
		/// <see cref="IX509CertificateDatabase"/>.
		/// </remarks>
		/// <param name="certificate">The certificate.</param>
		/// <param name="key">The private key.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="certificate"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="key"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="key"/> is not a private key.
		/// </exception>
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
		/// <remarks>
		/// Creates a new certificate record for storing in a <see cref="IX509CertificateDatabase"/>.
		/// </remarks>
		/// <param name="certificate">The certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public X509CertificateRecord (X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			AlgorithmsUpdated = DateTime.MinValue;
			Certificate = certificate;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.X509CertificateRecord"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is only meant to be used by implementors of <see cref="IX509CertificateDatabase"/>
		/// when loading records from the database.
		/// </remarks>
		public X509CertificateRecord ()
		{
		}
	}
}

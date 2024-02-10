//
// CmsRecipient.cs
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

using Org.BouncyCastle.X509;

using X509Certificate2 = System.Security.Cryptography.X509Certificates.X509Certificate2;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An S/MIME recipient.
	/// </summary>
	/// <remarks>
	/// If the X.509 certificates are known for each of the recipients, you
	/// may wish to use a <see cref="CmsRecipient"/> as opposed to having
	/// the <see cref="CryptographyContext"/> do its own certificate
	/// lookups for each <see cref="MailboxAddress"/>.
	/// </remarks>
	public class CmsRecipient
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="CmsRecipient"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="CmsRecipient"/> based on the provided certificate.</para>
		/// <para>If the X.509 certificate contains an S/MIME capability extension, the initial value of the
		/// <see cref="EncryptionAlgorithms"/> property will be set to whatever encryption algorithms are
		/// defined by the S/MIME capability extension, otherwise int will be initialized to a list
		/// containing only the Triple-Des encryption algorithm which should be safe to assume for all
		/// modern S/MIME v3.x client implementations.</para>
		/// </remarks>
		/// <param name="certificate">The recipient's certificate.</param>
		/// <param name="recipientIdentifierType">The recipient identifier type.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public CmsRecipient (X509Certificate certificate, SubjectIdentifierType recipientIdentifierType = SubjectIdentifierType.IssuerAndSerialNumber)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			if (recipientIdentifierType != SubjectIdentifierType.SubjectKeyIdentifier)
				RecipientIdentifierType = SubjectIdentifierType.IssuerAndSerialNumber;
			else
				RecipientIdentifierType = SubjectIdentifierType.SubjectKeyIdentifier;

			EncryptionAlgorithms = certificate.GetEncryptionAlgorithms ();
			Certificate = certificate;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="CmsRecipient"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="CmsRecipient"/>, loading the certificate from the specified stream.</para>
		/// <para>If the X.509 certificate contains an S/MIME capability extension, the initial value of the
		/// <see cref="EncryptionAlgorithms"/> property will be set to whatever encryption algorithms are
		/// defined by the S/MIME capability extension, otherwise int will be initialized to a list
		/// containing only the Triple-Des encryption algorithm which should be safe to assume for all
		/// modern S/MIME v3.x client implementations.</para>
		/// </remarks>
		/// <param name="stream">The stream containing the recipient's certificate.</param>
		/// <param name="recipientIdentifierType">The recipient identifier type.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The specified file does not contain a certificate.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public CmsRecipient (Stream stream, SubjectIdentifierType recipientIdentifierType = SubjectIdentifierType.IssuerAndSerialNumber)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			if (recipientIdentifierType != SubjectIdentifierType.SubjectKeyIdentifier)
				RecipientIdentifierType = SubjectIdentifierType.IssuerAndSerialNumber;
			else
				RecipientIdentifierType = SubjectIdentifierType.SubjectKeyIdentifier;

			var parser = new X509CertificateParser ();

			Certificate = parser.ReadCertificate (stream);

			if (Certificate == null)
				throw new FormatException ();

			EncryptionAlgorithms = Certificate.GetEncryptionAlgorithms ();
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="CmsRecipient"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="CmsRecipient"/>, loading the certificate from the specified file.</para>
		/// <para>If the X.509 certificate contains an S/MIME capability extension, the initial value of the
		/// <see cref="EncryptionAlgorithms"/> property will be set to whatever encryption algorithms are
		/// defined by the S/MIME capability extension, otherwise int will be initialized to a list
		/// containing only the Triple-Des encryption algorithm which should be safe to assume for all
		/// modern S/MIME v3.x client implementations.</para>
		/// </remarks>
		/// <param name="fileName">The file containing the recipient's certificate.</param>
		/// <param name="recipientIdentifierType">The recipient identifier type.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.
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
		/// <exception cref="System.FormatException">
		/// The specified file does not contain a certificate.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public CmsRecipient (string fileName, SubjectIdentifierType recipientIdentifierType = SubjectIdentifierType.IssuerAndSerialNumber)
		{
			if (fileName == null)
				throw new ArgumentNullException (nameof (fileName));

			if (recipientIdentifierType != SubjectIdentifierType.SubjectKeyIdentifier)
				RecipientIdentifierType = SubjectIdentifierType.IssuerAndSerialNumber;
			else
				RecipientIdentifierType = SubjectIdentifierType.SubjectKeyIdentifier;

			using (var stream = File.OpenRead (fileName)) {
				var parser = new X509CertificateParser ();

				Certificate = parser.ReadCertificate (stream);
			}

			if (Certificate == null)
				throw new FormatException ();

			EncryptionAlgorithms = Certificate.GetEncryptionAlgorithms ();
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="CmsRecipient"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="CmsRecipient"/> based on the provided certificate.</para>
		/// <para>If the X.509 certificate contains an S/MIME capability extension, the initial value of the
		/// <see cref="EncryptionAlgorithms"/> property will be set to whatever encryption algorithms are
		/// defined by the S/MIME capability extension, otherwise int will be initialized to a list
		/// containing only the Triple-Des encryption algorithm which should be safe to assume for all
		/// modern S/MIME v3.x client implementations.</para>
		/// </remarks>
		/// <param name="certificate">The recipient's certificate.</param>
		/// <param name="recipientIdentifierType">The recipient identifier type.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public CmsRecipient (X509Certificate2 certificate, SubjectIdentifierType recipientIdentifierType = SubjectIdentifierType.IssuerAndSerialNumber)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			if (recipientIdentifierType != SubjectIdentifierType.SubjectKeyIdentifier)
				RecipientIdentifierType = SubjectIdentifierType.IssuerAndSerialNumber;
			else
				RecipientIdentifierType = SubjectIdentifierType.SubjectKeyIdentifier;

			EncryptionAlgorithms = certificate.GetEncryptionAlgorithms ();
			Certificate = certificate.AsBouncyCastleCertificate ();
		}

		/// <summary>
		/// Gets the recipient's certificate.
		/// </summary>
		/// <remarks>
		/// The certificate is used for the purpose of encrypting data.
		/// </remarks>
		/// <value>The certificate.</value>
		public X509Certificate Certificate {
			get; private set;
		}

		/// <summary>
		/// Gets the recipient identifier type.
		/// </summary>
		/// <remarks>
		/// Specifies how the certificate should be looked up on the recipient's end.
		/// </remarks>
		/// <value>The recipient identifier type.</value>
		public SubjectIdentifierType RecipientIdentifierType {
			get; private set;
		}

		/// <summary>
		/// Gets or sets the known S/MIME encryption capabilities of the
		/// recipient's mail client, in their preferred order.
		/// </summary>
		/// <remarks>
		/// Provides the <see cref="SecureMimeContext"/> with an array of
		/// encryption algorithms that are known to be supported by the
		/// recpipient's client software and should be in the recipient's
		/// order of preference.
		/// </remarks>
		/// <value>The encryption algorithms.</value>
		public EncryptionAlgorithm[] EncryptionAlgorithms {
			get; set;
		}

		/// <summary>
		/// Get or set the RSA key encryption padding.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the padding to use for key encryption when
		/// the <see cref="Certificate"/>'s public key is an RSA key.</para>
		/// </remarks>
		/// <value>The encryption padding scheme.</value>
		public RsaEncryptionPadding RsaEncryptionPadding {
			get; set;
		}
	}
}

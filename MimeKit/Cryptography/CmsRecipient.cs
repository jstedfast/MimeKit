//
// CmsRecipient.cs
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

using Org.BouncyCastle.X509;

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
	public sealed class CmsRecipient
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.CmsRecipient"/> class.
		/// </summary>
		/// <param name="certificate">The recipient's certificate.</param>
		/// <param name="recipientIdentifierType">The recipient identifier type.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public CmsRecipient (X509Certificate certificate, SubjectIdentifierType recipientIdentifierType)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			if (recipientIdentifierType == SubjectIdentifierType.Unknown)
				RecipientIdentifierType = SubjectIdentifierType.IssuerAndSerialNumber;
			else
				RecipientIdentifierType = recipientIdentifierType;

			EncryptionAlgorithms = new EncryptionAlgorithm[] { EncryptionAlgorithm.TripleDes };
			Certificate = certificate;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.CmsRecipient"/> class.
		/// </summary>
		/// <param name="certificate">The recipient's certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public CmsRecipient (X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			EncryptionAlgorithms = new EncryptionAlgorithm[] { EncryptionAlgorithm.TripleDes };
			RecipientIdentifierType = SubjectIdentifierType.IssuerAndSerialNumber;
			Certificate = certificate;
		}

		/// <summary>
		/// Gets the recipient's certificate.
		/// </summary>
		/// <value>The certificate.</value>
		public X509Certificate Certificate {
			get; private set;
		}

		/// <summary>
		/// Gets the recipient identifier type.
		/// </summary>
		/// <value>The recipient identifier type.</value>
		public SubjectIdentifierType RecipientIdentifierType {
			get; private set;
		}

		/// <summary>
		/// Gets or sets the known S/MIME encryption capabilities of the
		/// recipient's mail client, in their preferred order.
		/// </summary>
		/// <value>The encryption algorithms.</value>
		public EncryptionAlgorithm[] EncryptionAlgorithms {
			get; set;
		}
	}
}

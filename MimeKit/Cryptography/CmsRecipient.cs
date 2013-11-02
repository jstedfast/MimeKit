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
	public class CmsRecipient
	{
		public CmsRecipient (X509Certificate certificate, SubjectIdentifierType recipientIdentifierType)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			if (recipientIdentifierType == SubjectIdentifierType.Unknown)
				RecipientIdentifierType = SubjectIdentifierType.IssuerAndSerialNumber;
			else
				RecipientIdentifierType = recipientIdentifierType;

			Certificate = certificate;
		}

		public CmsRecipient (X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			RecipientIdentifierType = SubjectIdentifierType.IssuerAndSerialNumber;
			Certificate = certificate;
		}

		public X509Certificate Certificate {
			get; private set;
		}

		public SubjectIdentifierType RecipientIdentifierType {
			get; private set;
		}
	}
}

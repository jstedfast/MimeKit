//
// X509CertificateExtensions.cs
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
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;

namespace MimeKit.Cryptography {
	static class X509CertificateExtensions
	{
		public static string GetIssuerNameInfo (this X509Certificate cert, DerObjectIdentifier identifier)
		{
			// FIXME: this should be fixed to return IList<string>
			var list = cert.IssuerDN.GetValueList (identifier);
			if (list.Count == 0)
				return null;

			return (string) list[0];
		}

		public static string GetSubjectNameInfo (this X509Certificate cert, DerObjectIdentifier identifier)
		{
			// FIXME: this should be fixed to return IList<string>
			var list = cert.SubjectDN.GetValueList (identifier);
			if (list.Count == 0)
				return null;

			return (string) list[0];
		}

		public static string GetCommonName (this X509Certificate cert)
		{
			return cert.GetSubjectNameInfo (X509Name.CN);
		}

		public static string GetSubjectName (this X509Certificate cert)
		{
			return cert.GetSubjectNameInfo (X509Name.Name);
		}

		public static string GetSubjectEmail (this X509Certificate cert)
		{
			return cert.GetSubjectNameInfo (X509Name.EmailAddress);
		}
	}
}

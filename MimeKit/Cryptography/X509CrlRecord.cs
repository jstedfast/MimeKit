//
// X509CrlRecord.cs
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
	/// An X.509 Certificate Revocation List (CRL) record.
	/// </summary>
	class X509CrlRecord
	{
		internal static readonly string[] ColumnNames = {
			"ID",
			"ISSUERNAME",
			"THISUPDATE",
			"NEXTUPDATE",
			"CRL"
		};

		/// <summary>
		/// Gets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		public int Id { get; internal set; }

		/// <summary>
		/// Gets the certificate issuer's name.
		/// </summary>
		/// <value>The issuer's name.</value>
		public string IssuerName { get { return Crl.IssuerDN.ToString (); } }

		/// <summary>
		/// Gets the date and time of the most recent update.
		/// </summary>
		/// <value>The date and time.</value>
		public DateTime ThisUpdate { get { return Crl.ThisUpdate; } }

		/// <summary>
		/// Gets the end date and time where the certificate is valid.
		/// </summary>
		/// <value>The date and time.</value>
		public DateTime NextUpdate {
			get {
				if (Crl.NextUpdate == null)
					return DateTime.MinValue;

				return Crl.NextUpdate.Value;
			}
		}

		/// <summary>
		/// Gets the certificate revocation list.
		/// </summary>
		/// <value>The certificate revocation list.</value>
		public X509Crl Crl { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.X509CrlRecord"/> class.
		/// </summary>
		/// <param name="crl">Crl.</param>
		public X509CrlRecord (X509Crl crl)
		{
			if (crl == null)
				throw new ArgumentNullException ("crl");

			Crl = crl;
		}

		internal X509CrlRecord ()
		{
		}
	}
}

//
// X509CrlRecord.cs
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

namespace MimeKit.Cryptography {
	[Flags]
	public enum X509CrlRecordFields {
		Id                = 1 << 0,
		IsDelta           = 1 << 1,
		IssuerName        = 1 << 2,
		ThisUpdate        = 1 << 3,
		NextUpdate        = 1 << 4,
		Crl               = 1 << 5,

		// helpers
		AllExeptCrl       = All & ~Crl,
		All               = 0xff
	}

	/// <summary>
	/// An X.509 Certificate Revocation List (CRL) record.
	/// </summary>
	public class X509CrlRecord
	{
		internal static readonly string[] ColumnNames = {
			"ID",
			"DELTA",
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
		/// Gets whether or not this certificate revocation list is a delta.
		/// </summary>
		/// <value><c>true</c> if th crl is delta; otherwise, <c>false</c>.</value>
		public bool IsDelta { get; internal set; }

		/// <summary>
		/// Gets the certificate issuer's name.
		/// </summary>
		/// <value>The issuer's name.</value>
		public string IssuerName { get; internal set; }

		/// <summary>
		/// Gets the date and time of the most recent update.
		/// </summary>
		/// <value>The date and time.</value>
		public DateTime ThisUpdate { get; internal set; }

		/// <summary>
		/// Gets the end date and time where the certificate is valid.
		/// </summary>
		/// <value>The date and time.</value>
		public DateTime NextUpdate { get; internal set; }

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

			if (crl.NextUpdate != null)
				NextUpdate = crl.NextUpdate.Value;

			IssuerName = crl.IssuerDN.ToString ();
			ThisUpdate = crl.ThisUpdate;
			IsDelta = crl.IsDelta ();
			Crl = crl;
		}

		internal X509CrlRecord ()
		{
		}
	}
}

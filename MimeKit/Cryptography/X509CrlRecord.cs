//
// X509CrlRecord.cs
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// X.509 certificate revocation list record fields.
	/// </summary>
	/// <remarks>
	/// The record fields are used when querying the <see cref="IX509CertificateDatabase"/>
	/// for certificate revocation lists.
	/// </remarks>
	[Flags]
	public enum X509CrlRecordFields {
		/// <summary>
		/// The "id" field is typically just the ROWID in the database.
		/// </summary>
		Id                = 1 << 0,

		/// <summary>
		/// The "delta" field is a boolean value indicating whether the certificate
		/// revocation list is a delta.
		/// </summary>
		IsDelta           = 1 << 1,

		/// <summary>
		/// The "issuer name" field stores the issuer name of the certificate revocation list.
		/// </summary>
		IssuerName        = 1 << 2,

		/// <summary>
		/// The "this update" field stores the date and time of the most recent update.
		/// </summary>
		ThisUpdate        = 1 << 3,

		/// <summary>
		/// The "next update" field stores the date and time of the next scheduled update.
		/// </summary>
		NextUpdate        = 1 << 4,

		/// <summary>
		/// The "crl" field stores the raw binary data of the certificate revocation list.
		/// </summary>
		Crl               = 1 << 5,
	}

	/// <summary>
	/// An X.509 certificate revocation list (CRL) record.
	/// </summary>
	/// <remarks>
	/// Represents an X.509 certificate revocation list record loaded from a database.
	/// </remarks>
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
		/// <remarks>
		/// The id is typically the ROWID of the certificate revocation list in the
		/// database and is not generally useful outside of the internals of the
		/// database implementation.
		/// </remarks>
		/// <value>The identifier.</value>
		public int Id { get; internal set; }

		/// <summary>
		/// Gets whether or not this certificate revocation list is a delta.
		/// </summary>
		/// <remarks>
		/// Indicates whether or not this certificate revocation list is a delta.
		/// </remarks>
		/// <value><c>true</c> if th crl is delta; otherwise, <c>false</c>.</value>
		public bool IsDelta { get; internal set; }

		/// <summary>
		/// Gets the issuer name of the certificate revocation list.
		/// </summary>
		/// <remarks>
		/// Gets the issuer name of the certificate revocation list.
		/// </remarks>
		/// <value>The issuer's name.</value>
		public string IssuerName { get; internal set; }

		/// <summary>
		/// Gets the date and time of the most recent update.
		/// </summary>
		/// <remarks>
		/// Gets the date and time of the most recent update.
		/// </remarks>
		/// <value>The date and time.</value>
		public DateTime ThisUpdate { get; internal set; }

		/// <summary>
		/// Gets the end date and time where the certificate is valid.
		/// </summary>
		/// <remarks>
		/// Gets the end date and time where the certificate is valid.
		/// </remarks>
		/// <value>The date and time.</value>
		public DateTime NextUpdate { get; internal set; }

		/// <summary>
		/// Gets the certificate revocation list.
		/// </summary>
		/// <remarks>
		/// Gets the certificate revocation list.
		/// </remarks>
		/// <value>The certificate revocation list.</value>
		public X509Crl Crl { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.X509CrlRecord"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new CRL record for storing in a <see cref="IX509CertificateDatabase"/>.
		/// </remarks>
		/// <param name="crl">The certificate revocation list.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="crl"/> is <c>null</c>.
		/// </exception>
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

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.X509CrlRecord"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is only meant to be used by implementors of <see cref="IX509CertificateDatabase"/>
		/// when loading records from the database.
		/// </remarks>
		public X509CrlRecord ()
		{
		}
	}
}

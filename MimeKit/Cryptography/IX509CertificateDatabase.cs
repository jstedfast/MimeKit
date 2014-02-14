//
// IX509CertificateDatabase.cs
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
using System.Collections.Generic;

using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509.Store;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An interface for an X509 Certificate database.
	/// </summary>
	interface IX509CertificateDatabase : IX509Store, IDisposable
	{
		/// <summary>
		/// Find the specified certificate.
		/// </summary>
		/// <param name="certificate">The certificate.</param>
		/// <param name="fields">The desired fields.</param>
		X509CertificateRecord Find (X509Certificate certificate, X509CertificateRecordFields fields);

		/// <summary>
		/// Finds the certificates matching the specified selector.
		/// </summary>
		/// <returns>The matching certificates.</returns>
		/// <param name="selector">The match criteria.</param>
		IEnumerable<X509Certificate> FindCertificates (IX509Selector selector);

		/// <summary>
		/// Finds the private keys matching the specified selector.
		/// </summary>
		/// <param name="selector">The selector.</param>
		IEnumerable<AsymmetricKeyParameter> FindPrivateKeys (IX509Selector selector);

		/// <summary>
		/// Finds the certificate records for the specified mailbox.
		/// </summary>
		/// <param name="mailbox">The mailbox.</param>
		/// <param name="now">The date and time.</param>
		/// <param name="requirePrivateKey"><c>true</c> if a private key is required.</param>
		/// <param name="fields">The desired fields.</param>
		IEnumerable<X509CertificateRecord> Find (MailboxAddress mailbox, DateTime now, bool requirePrivateKey, X509CertificateRecordFields fields);

		/// <summary>
		/// Finds the certificate records matching the specified selector.
		/// </summary>
		/// <param name="selector">The selector.</param>
		/// <param name="trustedOnly"><c>true</c> if only trusted certificates should be returned.</param>
		/// <param name="fields">The desired fields.</param>
		IEnumerable<X509CertificateRecord> Find (IX509Selector selector, bool trustedOnly, X509CertificateRecordFields fields);

		/// <summary>
		/// Add the specified certificate record.
		/// </summary>
		/// <param name="record">The certificate record.</param>
		void Add (X509CertificateRecord record);

		/// <summary>
		/// Remove the specified certificate record.
		/// </summary>
		/// <param name="record">The certificate record.</param>
		void Remove (X509CertificateRecord record);

		/// <summary>
		/// Update the specified certificate record.
		/// </summary>
		/// <param name="record">The certificate record.</param>
		/// <param name="fields">The fields to update.</param>
		void Update (X509CertificateRecord record, X509CertificateRecordFields fields);

		/// <summary>
		/// Finds the CRL records for the specified issuer.
		/// </summary>
		/// <param name="issuer">The issuer.</param>
		/// <param name="fields">The desired fields.</param>
		IEnumerable<X509CrlRecord> Find (X509Name issuer, X509CrlRecordFields fields);

		/// <summary>
		/// Finds the specified certificate revocation list.
		/// </summary>
		/// <param name="crl">The certificate revocation list.</param>
		/// <param name="fields">The desired fields.</param>
		X509CrlRecord Find (X509Crl crl, X509CrlRecordFields fields);

		/// <summary>
		/// Add the specified CRL record.
		/// </summary>
		/// <param name="record">The CRL record.</param>
		void Add (X509CrlRecord record);

		/// <summary>
		/// Remove the specified CRL record.
		/// </summary>
		/// <param name="record">The CRL record.</param>
		void Remove (X509CrlRecord record);

		/// <summary>
		/// Update the specified CRL record.
		/// </summary>
		/// <param name="record">The CRL record.</param>
		void Update (X509CrlRecord record);

		/// <summary>
		/// Gets a certificate revocation list store.
		/// </summary>
		/// <returns>A certificate recovation list store.</returns>
		IX509Store GetCrlStore ();
	}
}

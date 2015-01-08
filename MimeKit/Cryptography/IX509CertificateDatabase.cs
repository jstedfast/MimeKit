//
// IX509CertificateDatabase.cs
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
using System.Collections.Generic;

using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509.Store;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An interface for an X.509 Certificate database.
	/// </summary>
	/// <remarks>
	/// An X.509 certificate database is used for storing certificates, metdata related to the certificates
	/// (such as encryption algorithms supported by the associated client), certificate revocation lists (CRLs),
	/// and private keys.
	/// </remarks>
	public interface IX509CertificateDatabase : IX509Store, IDisposable
	{
		/// <summary>
		/// Find the specified certificate.
		/// </summary>
		/// <remarks>
		/// Searches the database for the specified certificate, returning the matching
		/// record with the desired fields populated.
		/// </remarks>
		/// <returns>The matching record if found; otherwise <c>null</c>.</returns>
		/// <param name="certificate">The certificate.</param>
		/// <param name="fields">The desired fields.</param>
		X509CertificateRecord Find (X509Certificate certificate, X509CertificateRecordFields fields);

		/// <summary>
		/// Finds the certificates matching the specified selector.
		/// </summary>
		/// <remarks>
		/// Searches the database for certificates matching the selector, returning all
		/// matching certificates.
		/// </remarks>
		/// <returns>The matching certificates.</returns>
		/// <param name="selector">The match selector or <c>null</c> to return all certificates.</param>
		IEnumerable<X509Certificate> FindCertificates (IX509Selector selector);

		/// <summary>
		/// Finds the private keys matching the specified selector.
		/// </summary>
		/// <remarks>
		/// Searches the database for certificate records matching the selector, returning the
		/// private keys for each matching record.
		/// </remarks>
		/// <returns>The matching certificates.</returns>
		/// <param name="selector">The match selector or <c>null</c> to return all private keys.</param>
		IEnumerable<AsymmetricKeyParameter> FindPrivateKeys (IX509Selector selector);

		/// <summary>
		/// Finds the certificate records for the specified mailbox.
		/// </summary>
		/// <remarks>
		/// Searches the database for certificates matching the specified mailbox that are valid
		/// for the date and time specified, returning all matching records populated with the
		/// desired fields.
		/// </remarks>
		/// <returns>The matching certificate records populated with the desired fields.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <param name="now">The date and time.</param>
		/// <param name="requirePrivateKey"><c>true</c> if a private key is required.</param>
		/// <param name="fields">The desired fields.</param>
		IEnumerable<X509CertificateRecord> Find (MailboxAddress mailbox, DateTime now, bool requirePrivateKey, X509CertificateRecordFields fields);

		/// <summary>
		/// Finds the certificate records matching the specified selector.
		/// </summary>
		/// <remarks>
		/// Searches the database for certificate records matching the selector, returning all
		/// of the matching records populated with the desired fields.
		/// </remarks>
		/// <returns>The matching certificate records populated with the desired fields.</returns>
		/// <param name="selector">The match selector or <c>null</c> to match all certificates.</param>
		/// <param name="trustedOnly"><c>true</c> if only trusted certificates should be returned.</param>
		/// <param name="fields">The desired fields.</param>
		IEnumerable<X509CertificateRecord> Find (IX509Selector selector, bool trustedOnly, X509CertificateRecordFields fields);

		/// <summary>
		/// Add the specified certificate record.
		/// </summary>
		/// <remarks>
		/// Adds the specified certificate record to the database.
		/// </remarks>
		/// <param name="record">The certificate record.</param>
		void Add (X509CertificateRecord record);

		/// <summary>
		/// Remove the specified certificate record.
		/// </summary>
		/// <remarks>
		/// Removes the specified certificate record from the database.
		/// </remarks>
		/// <param name="record">The certificate record.</param>
		void Remove (X509CertificateRecord record);

		/// <summary>
		/// Update the specified certificate record.
		/// </summary>
		/// <remarks>
		/// Updates the specified fields of the record in the database.
		/// </remarks>
		/// <param name="record">The certificate record.</param>
		/// <param name="fields">The fields to update.</param>
		void Update (X509CertificateRecord record, X509CertificateRecordFields fields);

		/// <summary>
		/// Finds the CRL records for the specified issuer.
		/// </summary>
		/// <remarks>
		/// Searches the database for CRL records matching the specified issuer, returning
		/// all matching records populated with the desired fields.
		/// </remarks>
		/// <returns>The matching CRL records populated with the desired fields.</returns>
		/// <param name="issuer">The issuer.</param>
		/// <param name="fields">The desired fields.</param>
		IEnumerable<X509CrlRecord> Find (X509Name issuer, X509CrlRecordFields fields);

		/// <summary>
		/// Finds the specified certificate revocation list.
		/// </summary>
		/// <remarks>
		/// Searches the database for the specified CRL, returning the matching record with
		/// the desired fields populated.
		/// </remarks>
		/// <returns>The matching record if found; otherwise <c>null</c>.</returns>
		/// <param name="crl">The certificate revocation list.</param>
		/// <param name="fields">The desired fields.</param>
		X509CrlRecord Find (X509Crl crl, X509CrlRecordFields fields);

		/// <summary>
		/// Add the specified CRL record.
		/// </summary>
		/// <remarks>
		/// Adds the specified CRL record to the database.
		/// </remarks>
		/// <param name="record">The CRL record.</param>
		void Add (X509CrlRecord record);

		/// <summary>
		/// Remove the specified CRL record.
		/// </summary>
		/// <remarks>
		/// Removes the specified CRL record from the database.
		/// </remarks>
		/// <param name="record">The CRL record.</param>
		void Remove (X509CrlRecord record);

		/// <summary>
		/// Update the specified CRL record.
		/// </summary>
		/// <remarks>
		/// Updates the specified fields of the record in the database.
		/// </remarks>
		/// <param name="record">The CRL record.</param>
		void Update (X509CrlRecord record);

		/// <summary>
		/// Gets a certificate revocation list store.
		/// </summary>
		/// <remarks>
		/// Gets a certificate revocation list store.
		/// </remarks>
		/// <returns>A certificate recovation list store.</returns>
		IX509Store GetCrlStore ();
	}
}

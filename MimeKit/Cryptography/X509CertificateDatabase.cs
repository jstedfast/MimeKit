//
// X509CertificateDatabase.cs
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
using System.IO;
using System.Collections;
using System.Collections.Generic;

using Mono.Data.Sqlite;

using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An X.509 certificate database.
	/// </summary>
	class X509CertificateDatabase : IDisposable, IX509Store
	{
		SqliteConnection sqlite;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.X509CertificateDatabase"/> class.
		/// </summary>
		/// <param name="fileName">The file name.</param>
		/// <param name="password">The password.</param>
		public X509CertificateDatabase (string fileName, string password)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			var builder = new SqliteConnectionStringBuilder ();
			builder.DateTimeFormat = SQLiteDateFormats.UnixEpoch;
			builder.DataSource = fileName;
			builder.Password = password;

			if (!File.Exists (fileName)) {
				SqliteConnection.CreateFile (fileName);
				sqlite = new SqliteConnection (builder.ConnectionString);
				using (var command = X509CertificateRecord.GetCreateTableCommand (sqlite)) {
					command.ExecuteNonQuery ();
				}
			} else {
				sqlite = new SqliteConnection (builder.ConnectionString);
			}
		}

		/// <summary>
		/// Find the specified certificate.
		/// </summary>
		/// <param name="certificate">The certificate.</param>
		public X509CertificateRecord Find (X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			using (var command = X509CertificateRecord.GetFindCertificateCommand (sqlite, certificate)) {
				var reader = command.ExecuteReader ();

				try {
					if (reader.Read ()) {
						var parser = new X509CertificateParser ();
						var buffer = new byte[4096];

						return X509CertificateRecord.Load (reader, parser, ref buffer);
					}
				} finally {
					reader.Close ();
				}
			}

			return null;
		}

		/// <summary>
		/// Find the records for the specified mailboxthat is valid for the given date and time.
		/// </summary>
		/// <param name="mailbox">The mailbox.</param>
		/// <param name="now">The date and time.</param>
		public IEnumerable<X509CertificateRecord> Find (MailboxAddress mailbox, DateTime now)
		{
			if (mailbox == null)
				throw new ArgumentNullException ("mailbox");

			using (var command = X509CertificateRecord.GetFindCertificatesCommand (sqlite, mailbox, now)) {
				var reader = command.ExecuteReader ();

				try {
					var parser = new X509CertificateParser ();
					var buffer = new byte[4096];

					while (reader.Read ())
						yield return X509CertificateRecord.Load (reader, parser, ref buffer);
				} finally {
					reader.Close ();
				}
			}

			yield break;
		}

		/// <summary>
		/// Add the specified record.
		/// </summary>
		/// <param name="record">Record.</param>
		public void Add (X509CertificateRecord record)
		{
			if (record == null)
				throw new ArgumentNullException ("record");

			using (var command = record.GetInsertCommand (sqlite)) {
				command.ExecuteNonQuery ();
			}
		}

		/// <summary>
		/// Remove the specified record.
		/// </summary>
		/// <param name="record">Record.</param>
		public void Remove (X509CertificateRecord record)
		{
			if (record == null)
				throw new ArgumentNullException ("record");

			using (var command = record.GetDeleteCommand (sqlite)) {
				command.ExecuteNonQuery ();
			}
		}

		/// <summary>
		/// Update the specified record.
		/// </summary>
		/// <param name="record">Record.</param>
		public void Update (X509CertificateRecord record)
		{
			if (record == null)
				throw new ArgumentNullException ("record");

			using (var command = record.GetUpdateCommand (sqlite)) {
				command.ExecuteNonQuery ();
			}
		}

		#region IX509Store implementation

		/// <summary>
		/// Gets an enumerator of matching <see cref="Org.BouncyCastle.X509.X509Certificate"/>s
		/// based on the specified selector.
		/// </summary>
		/// <returns>The matching certificates.</returns>
		/// <param name="selector">The match criteria.</param>
		public IEnumerable<X509Certificate> GetMatches (IX509Selector selector)
		{
			using (var command = X509CertificateRecord.GetFindCertificatesCommand (sqlite)) {
				var reader = command.ExecuteReader ();

				try {
					var parser = new X509CertificateParser ();
					var buffer = new byte[4096];

					while (reader.Read ()) {
						var record = X509CertificateRecord.Load (reader, parser, ref buffer);
						if (selector == null || selector.Match (record.Certificate))
							yield return record.Certificate;
					}
				} finally {
					reader.Close ();
				}
			}

			yield break;
		}

		/// <summary>
		/// Gets a collection of matching <see cref="Org.BouncyCastle.X509.X509Certificate"/>s
		/// based on the specified selector.
		/// </summary>
		/// <returns>The matching certificates.</returns>
		/// <param name="selector">The match criteria.</param>
		ICollection IX509Store.GetMatches (IX509Selector selector)
		{
			return new List<X509Certificate> (GetMatches (selector));
		}

		#endregion

		#region IDisposable implementation

		public void Dispose ()
		{
			if (sqlite != null) {
				sqlite.Dispose ();
				sqlite = null;
			}
		}

		#endregion
	}
}

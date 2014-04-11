//
// SqliteCertificateDatabase.cs
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
using System.IO;
using System.Data;
using System.Text;

#if __MOBILE__
using Mono.Data.Sqlite;
#else
using System.Reflection;
#endif

using Org.BouncyCastle.X509;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509.Store;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An X.509 certificate database built on SQLite.
	/// </summary>
	class SqliteCertificateDatabase : X509CertificateDatabase
	{
#if !__MOBILE__
		static readonly Type sqliteConnectionStringBuilderClass;
		static readonly Type sqliteConnectionClass;
		static readonly Assembly sqliteAssembly;
#endif

		readonly IDbConnection sqlite;
		bool disposed;

		// At class initialization we try to use reflection to load the
		// Mono.Data.Sqlite assembly: this allows to use Sqlite as the 
		// default certificate store without depending explicitly on the
		// DLL.

		static SqliteCertificateDatabase ()
		{
#if !__MOBILE__
			sqliteAssembly = Assembly.Load ("Mono.Data.Sqlite");
			if (sqliteAssembly != null) {
				sqliteConnectionClass = sqliteAssembly.GetType ("Mono.Data.Sqlite.SqliteConnection");
				sqliteConnectionStringBuilderClass = sqliteAssembly.GetType ("Mono.Data.Sqlite.SqliteConnectionStringBuilder");
				IsAvailable = true;
			}
#else
			IsAvailable = true;
#endif
		}

		internal static bool IsAvailable {
			get; private set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.SqliteCertificateDatabase"/> class.
		/// </summary>
		/// <param name="fileName">The file name.</param>
		/// <param name="password">The password used for encrypting and decrypting the private keys.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The specified file path is empty.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An error occurred reading the file.
		/// </exception>
		public SqliteCertificateDatabase (string fileName, string password) : base (password)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

#if !__MOBILE__
			var builder = Activator.CreateInstance (sqliteConnectionStringBuilderClass);
			sqliteConnectionStringBuilderClass.GetProperty ("DateTimeFormat").SetValue (builder, 0, null);
			sqliteConnectionStringBuilderClass.GetProperty ("DataSource").SetValue (builder, fileName, null);

			if (!File.Exists (fileName))
				sqliteConnectionClass.GetMethod ("CreateFile").Invoke (null, new [] {fileName});

			var connectionString = (string) sqliteConnectionStringBuilderClass.GetProperty ("ConnectionString").GetValue (builder, null);
			sqlite = (IDbConnection) Activator.CreateInstance (sqliteConnectionClass, new [] { connectionString });
			sqlite.Open ();
#else
			var builder = new SqliteConnectionStringBuilder ();
			builder.DateTimeFormat = SQLiteDateFormats.Ticks;
			builder.DataSource = fileName;

			if (!File.Exists (fileName))
				SqliteConnection.CreateFile (fileName);

			sqlite = new SqliteConnection (builder.ConnectionString);
			sqlite.Open ();
#endif

			CreateCertificatesTable ();
			CreateCrlsTable ();
		}

		static IDbCommand GetCreateCertificatesTableCommand (IDbConnection sqlite)
		{
			var statement = new StringBuilder ("CREATE TABLE IF NOT EXISTS CERTIFICATES(");
			var columns = X509CertificateRecord.ColumnNames;

			for (int i = 0; i < columns.Length; i++) {
				if (i > 0)
					statement.Append (", ");

				statement.Append (columns[i] + " ");
				switch (columns[i]) {
				case "ID": statement.Append ("INTEGER PRIMARY KEY AUTOINCREMENT"); break;
				case "BASICCONSTRAINTS": statement.Append ("INTEGER NOT NULL"); break;
				case "TRUSTED":  statement.Append ("INTEGER NOT NULL"); break;
				case "KEYUSAGE": statement.Append ("INTEGER NOT NULL"); break;
				case "NOTBEFORE": statement.Append ("INTEGER NOT NULL"); break;
				case "NOTAFTER": statement.Append ("INTEGER NOT NULL"); break;
				case "ISSUERNAME": statement.Append ("TEXT NOT NULL"); break;
				case "SERIALNUMBER": statement.Append ("TEXT NOT NULL"); break;
				case "SUBJECTEMAIL": statement.Append ("TEXT COLLATE NOCASE"); break;
				case "FINGERPRINT": statement.Append ("TEXT COLLATE NOCASE NOT NULL"); break;
				case "ALGORITHMS": statement.Append ("TEXT"); break;
				case "ALGORITHMSUPDATED": statement.Append ("INTEGER NOT NULL"); break;
				case "CERTIFICATE": statement.Append ("BLOB UNIQUE NOT NULL"); break;
				case "PRIVATEKEY": statement.Append ("BLOB"); break;
				}
			}

			statement.Append (')');

			var command = sqlite.CreateCommand ();

			command.CommandText = statement.ToString ();
			command.CommandType = CommandType.Text;

			return command;
		}

		void CreateCertificatesTable ()
		{
			using (var command = GetCreateCertificatesTableCommand (sqlite)) {
				command.ExecuteNonQuery ();
			}

			// FIXME: create some indexes as well?
		}

		static IDbCommand GetCreateCrlsTableCommand (IDbConnection sqlite)
		{
			var statement = new StringBuilder ("CREATE TABLE IF NOT EXISTS CRLS(");
			var columns = X509CrlRecord.ColumnNames;

			for (int i = 0; i < columns.Length; i++) {
				if (i > 0)
					statement.Append (", ");

				statement.Append (columns[i] + " ");
				switch (columns[i]) {
				case "ID": statement.Append ("INTEGER PRIMARY KEY AUTOINCREMENT"); break;
				case "DELTA" : statement.Append ("INTEGER NOT NULL"); break;
				case "ISSUERNAME": statement.Append ("TEXT NOT NULL"); break;
				case "THISUPDATE": statement.Append ("INTEGER NOT NULL"); break;
				case "NEXTUPDATE": statement.Append ("INTEGER NOT NULL"); break;
				case "CRL": statement.Append ("BLOB NOT NULL"); break;
				}
			}

			statement.Append (')');

			var command = sqlite.CreateCommand ();

			command.CommandText = statement.ToString ();
			command.CommandType = CommandType.Text;

			return command;
		}

		void CreateCrlsTable ()
		{
			using (var command = GetCreateCrlsTableCommand (sqlite)) {
				command.ExecuteNonQuery ();
			}

			// FIXME: create some indexes as well?
		}

		/// <summary>
		/// Gets the database command to select the record matching the specified certificate.
		/// </summary>
		/// <returns>The database command.</returns>
		/// <param name="certificate">The certificate.</param>
		/// <param name="fields">The fields to return.</param>
		protected override IDbCommand GetSelectCommand (X509Certificate certificate, X509CertificateRecordFields fields)
		{
			var query = "SELECT " + string.Join (", ", GetColumnNames (fields)) + " FROM CERTIFICATES ";
			var issuerName = certificate.IssuerDN.ToString ();
			var serialNumber = certificate.SerialNumber.ToString ();
			var fingerprint = certificate.GetFingerprint ();
			var command = sqlite.CreateCommand ();

			command.CommandText = query + "WHERE ISSUERNAME = @ISSUERNAME AND SERIALNUMBER = @SERIALNUMBER AND FINGERPRINT = @FINGERPRINT LIMIT 1";
			command.AddParameterWithValue ("@ISSUERNAME", issuerName);
			command.AddParameterWithValue ("@SERIALNUMBER", serialNumber);
			command.AddParameterWithValue ("@FINGERPRINT", fingerprint);
			command.CommandType = CommandType.Text;

			return command;
		}

		/// <summary>
		/// Gets the database command to select the certificate records for the specified mailbox.
		/// </summary>
		/// <returns>The database command.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <param name="now">The date and time for which the certificate should be valid.</param>
		/// <param name="requirePrivateKey">true</param>
		/// <param name="fields">The fields to return.</param>
		protected override IDbCommand GetSelectCommand (MailboxAddress mailbox, DateTime now, bool requirePrivateKey, X509CertificateRecordFields fields)
		{
			var query = "SELECT " + string.Join (", ", GetColumnNames (fields)) + " FROM CERTIFICATES";
			var secure = mailbox as SecureMailboxAddress;
			var command = sqlite.CreateCommand ();
			var constraints = " WHERE ";

			command.AddParameterWithValue ("@BASICCONSTRAINTS", -1);
			constraints += "BASICCONSTRAINTS = @BASICCONSTRAINTS ";

			constraints += "AND NOTBEFORE < @NOW AND NOTAFTER > @NOW ";
			command.AddParameterWithValue ("@NOW", now);

			if (requirePrivateKey)
				constraints += "AND PRIVATEKEY NOT NULL ";

			if (secure != null && !string.IsNullOrEmpty (secure.Fingerprint)) {
				if (secure.Fingerprint.Length < 40) {
					constraints += "AND FINGERPRINT LIKE @FINGERPRINT";
					command.AddParameterWithValue ("@FINGERPRINT", secure.Fingerprint + "%");
				} else {
					constraints += "AND FINGERPRINT = @FINGERPRINT";
					command.AddParameterWithValue ("@FINGERPRINT", secure.Fingerprint);
				}
			} else {
				constraints += "AND SUBJECTEMAIL = @SUBJECTEMAIL";
				command.AddParameterWithValue ("@SUBJECTEMAIL", mailbox.Address);
			}

			command.CommandText = query + constraints;
			command.CommandType = CommandType.Text;

			return command;
		}

		/// <summary>
		/// Gets the database command to select the certificate records for the specified mailbox.
		/// </summary>
		/// <returns>The database command.</returns>
		/// <param name="selector">Selector.</param>
		/// <param name="trustedOnly">If set to <c>true</c> trusted only.</param>
		/// <param name="requirePrivateKey">true</param>
		/// <param name="fields">The fields to return.</param>
		protected override IDbCommand GetSelectCommand (IX509Selector selector, bool trustedOnly, bool requirePrivateKey, X509CertificateRecordFields fields)
		{
			var query = "SELECT " + string.Join (", ", GetColumnNames (fields)) + " FROM CERTIFICATES";
			var match = selector as X509CertStoreSelector;
			var command = sqlite.CreateCommand ();
			var constraints = " WHERE ";

			if (trustedOnly) {
				command.AddParameterWithValue ("@TRUSTED", true);
				constraints += "TRUSTED = @TRUSTED";
			}

			if (match != null) {
				if (match.BasicConstraints != -1) {
					if (command.Parameters.Count > 0)
						constraints += " AND ";

					command.AddParameterWithValue ("@BASICCONSTRAINTS", match.BasicConstraints);
					constraints += "BASICCONSTRAINTS = @BASICCONSTRAINTS";
				}

				if (match.KeyUsage != null) {
					var flags = X509CertificateExtensions.GetKeyUsageFlags (match.KeyUsage);

					if (flags != X509KeyUsageFlags.None) {
						if (command.Parameters.Count > 0)
							constraints += " AND ";

						command.AddParameterWithValue ("@FLAGS", (int) flags);
						constraints += "KEYUSAGE & @FLAGS";
					}
				}

				if (match.Issuer != null) {
					if (command.Parameters.Count > 0)
						constraints += " AND ";

					command.AddParameterWithValue ("@ISSUERNAME", match.Issuer.ToString ());
					constraints += "ISSUERNAME = @ISSUERNAME";
				}

				if (match.SerialNumber != null) {
					if (command.Parameters.Count > 0)
						constraints += " AND ";

					command.AddParameterWithValue ("@SERIALNUMBER", match.SerialNumber.ToString ());
					constraints += "SERIALNUMBER = @SERIALNUMBER";
				}
			}

			if (requirePrivateKey) {
				if (command.Parameters.Count > 0)
					constraints += " AND ";

				constraints += "PRIVATEKEY NOT NULL";
			} else if (command.Parameters.Count == 0) {
				constraints = string.Empty;
			}

			command.CommandText = query + constraints;
			command.CommandType = CommandType.Text;

			return command;
		}

		/// <summary>
		/// Gets the database command to select the CRL records matching the specified issuer.
		/// </summary>
		/// <returns>The database command.</returns>
		/// <param name="issuer">The issuer.</param>
		/// <param name="fields">The fields to return.</param>
		protected override IDbCommand GetSelectCommand (X509Name issuer, X509CrlRecordFields fields)
		{
			var query = "SELECT " + string.Join (", ", GetColumnNames (fields)) + " FROM CRLS ";
			var command = sqlite.CreateCommand ();

			command.CommandText = query + "WHERE ISSUERNAME = @ISSUERNAME";
			command.AddParameterWithValue ("@ISSUERNAME", issuer.ToString ());
			command.CommandType = CommandType.Text;

			return command;
		}

		/// <summary>
		/// Gets the database command to select the record for the specified CRL.
		/// </summary>
		/// <returns>The database command.</returns>
		/// <param name="crl">The X.509 CRL.</param>
		/// <param name="fields">The fields to return.</param>
		protected override IDbCommand GetSelectCommand (X509Crl crl, X509CrlRecordFields fields)
		{
			var query = "SELECT " + string.Join (", ", GetColumnNames (fields)) + " FROM CRLS ";
			var issuerName = crl.IssuerDN.ToString ();
			var command = sqlite.CreateCommand ();

			command.CommandText = query + "WHERE DELTA = @DELTA AND ISSUERNAME = @ISSUERNAME AND THISUPDATE = @THISUPDATE LIMIT 1";
			command.AddParameterWithValue ("@DELTA", crl.IsDelta ());
			command.AddParameterWithValue ("@ISSUERNAME", issuerName);
			command.AddParameterWithValue ("@THISUPDATE", crl.ThisUpdate);
			command.CommandType = CommandType.Text;

			return command;
		}

		/// <summary>
		/// Gets the database command to select all CRLs in the table.
		/// </summary>
		/// <returns>The database command.</returns>
		protected override IDbCommand GetSelectAllCrlsCommand ()
		{
			var command = sqlite.CreateCommand ();

			command.CommandText = "SELECT ID, CRL FROM CRLS";
			command.CommandType = CommandType.Text;

			return command;
		}

		/// <summary>
		/// Gets the database command to delete the specified certificate record.
		/// </summary>
		/// <returns>The database command.</returns>
		/// <param name="record">The certificate record.</param>
		protected override IDbCommand GetDeleteCommand (X509CertificateRecord record)
		{
			var command = sqlite.CreateCommand ();

			command.CommandText = "DELETE FROM CERTIFICATES WHERE ID = @ID";
			command.AddParameterWithValue ("@ID", record.Id);
			command.CommandType = CommandType.Text;

			return command;
		}

		/// <summary>
		/// Gets the database command to delete the specified CRL record.
		/// </summary>
		/// <returns>The database command.</returns>
		/// <param name="record">The record.</param>
		protected override IDbCommand GetDeleteCommand (X509CrlRecord record)
		{
			var command = sqlite.CreateCommand ();

			command.CommandText = "DELETE FROM CRLS WHERE ID = @ID";
			command.AddParameterWithValue ("@ID", record.Id);
			command.CommandType = CommandType.Text;

			return command;
		}

		/// <summary>
		/// Gets the database command to insert the specified certificate record.
		/// </summary>
		/// <returns>The database command.</returns>
		/// <param name="record">The certificate record.</param>
		protected override IDbCommand GetInsertCommand (X509CertificateRecord record)
		{
			var statement = new StringBuilder ("INSERT INTO CERTIFICATES(");
			var columns = X509CertificateRecord.ColumnNames;
			var variables = new StringBuilder ("VALUES(");
			var command = sqlite.CreateCommand ();

			for (int i = 1; i < columns.Length; i++) {
				if (i > 1) {
					statement.Append (", ");
					variables.Append (", ");
				}

				var value = GetValue (record, columns[i]);
				var variable = "@" + columns[i];

				command.AddParameterWithValue (variable, value);
				statement.Append (columns[i]);
				variables.Append (variable);
			}

			statement.Append (')');
			variables.Append (')');

			command.CommandText = statement + " " + variables;
			command.CommandType = CommandType.Text;

			return command;
		}

		/// <summary>
		/// Gets the database command to insert the specified CRL record.
		/// </summary>
		/// <returns>The database command.</returns>
		/// <param name="record">The CRL record.</param>
		protected override IDbCommand GetInsertCommand (X509CrlRecord record)
		{
			var statement = new StringBuilder ("INSERT INTO CRLS(");
			var variables = new StringBuilder ("VALUES(");
			var columns = X509CrlRecord.ColumnNames;
			var command = sqlite.CreateCommand ();

			for (int i = 1; i < columns.Length; i++) {
				if (i > 1) {
					statement.Append (", ");
					variables.Append (", ");
				}

				var value = GetValue (record, columns[i]);
				var variable = "@" + columns[i];

				command.AddParameterWithValue (variable, value);
				statement.Append (columns[i]);
				variables.Append (variable);
			}

			statement.Append (')');
			variables.Append (')');

			command.CommandText = statement + " " + variables;
			command.CommandType = CommandType.Text;

			return command;
		}

		/// <summary>
		/// Gets the database command to update the specified record.
		/// </summary>
		/// <returns>The database command.</returns>
		/// <param name="record">The certificate record.</param>
		/// <param name="fields">The fields to update.</param>
		protected override IDbCommand GetUpdateCommand (X509CertificateRecord record, X509CertificateRecordFields fields)
		{
			var statement = new StringBuilder ("UPDATE CERTIFICATES SET ");
			var columns = GetColumnNames (fields & ~X509CertificateRecordFields.Id);
			var command = sqlite.CreateCommand ();

			for (int i = 0; i < columns.Length; i++) {
				var value = GetValue (record, columns[i]);
				var variable = "@" + columns[i];

				if (i > 0)
					statement.Append (", ");

				statement.Append (columns[i]);
				statement.Append (" = ");
				statement.Append (variable);

				command.AddParameterWithValue (variable, value);
			}

			statement.Append (" WHERE ID = @ID");
			command.AddParameterWithValue ("@ID", record.Id);

			command.CommandText = statement.ToString ();
			command.CommandType = CommandType.Text;

			return command;
		}

		/// <summary>
		/// Gets the database command to update the specified CRL record.
		/// </summary>
		/// <returns>The database command.</returns>
		/// <param name="record">The CRL record.</param>
		protected override IDbCommand GetUpdateCommand (X509CrlRecord record)
		{
			var statement = new StringBuilder ("UPDATE CRLS SET ");
			var columns = X509CrlRecord.ColumnNames;
			var command = sqlite.CreateCommand ();

			for (int i = 1; i < columns.Length; i++) {
				var value = GetValue (record, columns[i]);
				var variable = "@" + columns[i];

				if (i > 1)
					statement.Append (", ");

				statement.Append (columns[i]);
				statement.Append (" = ");
				statement.Append (variable);

				command.AddParameterWithValue (variable, value);
			}

			statement.Append (" WHERE ID = @ID");
			command.AddParameterWithValue ("@ID", record.Id);

			command.CommandText = statement.ToString ();
			command.CommandType = CommandType.Text;

			return command;
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="SqliteCertificateDatabase"/> and
		/// optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
		/// <c>false</c> to release only the unmanaged resources.</param>
		protected override void Dispose (bool disposing)
		{
			if (disposing && !disposed)
				sqlite.Dispose ();

			disposed = true;
		}
	}
}

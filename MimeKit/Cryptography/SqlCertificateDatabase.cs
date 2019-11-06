//
// SqlCertificateDatabase.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2019 Xamarin Inc. (www.xamarin.com)
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
using System.Data;
using System.Text;
using System.Data.Common;

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
	/// An abstract X.509 certificate database built on generic SQL storage.
	/// </summary>
	/// <remarks>
	/// <para>An X.509 certificate database is used for storing certificates, metdata related to the certificates
	/// (such as encryption algorithms supported by the associated client), certificate revocation lists (CRLs),
	/// and private keys.</para>
	/// <para>This particular database uses SQLite to store the data.</para>
	/// </remarks>
	public abstract class SqlCertificateDatabase : X509CertificateDatabase
	{
		readonly DbConnection connection;
		bool disposed;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.SqlCertificateDatabase"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="SqlCertificateDatabase"/> using the provided database connection.
		/// </remarks>
		/// <param name="connection">The database <see cref="System.Data.IDbConnection"/>.</param>
		/// <param name="password">The password used for encrypting and decrypting the private keys.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="connection"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		protected SqlCertificateDatabase (DbConnection connection, string password) : base (password)
		{
			if (connection == null)
				throw new ArgumentNullException (nameof (connection));

			this.connection = connection;

			if (connection.State != ConnectionState.Open)
				connection.Open ();

			CreateCertificatesTable ();
			CreateCrlsTable ();
		}

		/// <summary>
		/// Gets the command to create the certificates table.
		/// </summary>
		/// <remarks>
		/// Constructs the command to create a certificates table suitable for storing
		/// <see cref="X509CertificateRecord"/> objects.
		/// </remarks>
		/// <returns>The <see cref="System.Data.Common.DbCommand"/>.</returns>
		/// <param name="connection">The <see cref="System.Data.Common.DbConnection"/>.</param>
		protected abstract DbCommand GetCreateCertificatesTableCommand (DbConnection connection);

		/// <summary>
		/// Gets the command to create the CRLs table.
		/// </summary>
		/// <remarks>
		/// Constructs the command to create a CRLs table suitable for storing
		/// <see cref="X509CertificateRecord"/> objects.
		/// </remarks>
		/// <returns>The <see cref="System.Data.Common.DbCommand"/>.</returns>
		/// <param name="connection">The <see cref="System.Data.Common.DbConnection"/>.</param>
		protected abstract DbCommand GetCreateCrlsTableCommand (DbConnection connection);

		static void CreateIndex (DbConnection connection, string tableName, string[] columnNames)
		{
			var indexName = string.Format ("{0}_{1}_INDEX", tableName, string.Join ("_", columnNames));
			var query = string.Format ("CREATE INDEX IF NOT EXISTS {0} ON {1}({2})", indexName, tableName, string.Join (", ", columnNames));

			using (var command = connection.CreateCommand ()) {
				command.CommandText = query;
				command.ExecuteNonQuery ();
			}
		}

		void CreateCertificatesTable ()
		{
			using (var command = GetCreateCertificatesTableCommand (connection))
				command.ExecuteNonQuery ();

			CreateIndex (connection, "CERTIFICATES", new [] { "ISSUERNAME", "SERIALNUMBER", "FINGERPRINT" });
			CreateIndex (connection, "CERTIFICATES", new [] { "BASICCONSTRAINTS", "FINGERPRINT" });
			CreateIndex (connection, "CERTIFICATES", new [] { "BASICCONSTRAINTS", "SUBJECTEMAIL" });
			CreateIndex (connection, "CERTIFICATES", new [] { "TRUSTED" });
			CreateIndex (connection, "CERTIFICATES", new [] { "TRUSTED", "BASICCONSTRAINTS", "ISSUERNAME", "SERIALNUMBER" });
			CreateIndex (connection, "CERTIFICATES", new [] { "BASICCONSTRAINTS", "ISSUERNAME", "SERIALNUMBER" });
		}

		void CreateCrlsTable ()
		{
			using (var command = GetCreateCrlsTableCommand (connection))
				command.ExecuteNonQuery ();

			CreateIndex (connection, "CRLS", new [] { "ISSUERNAME" });
			CreateIndex (connection, "CRLS", new [] { "DELTA", "ISSUERNAME", "THISUPDATE" });
		}

		static StringBuilder CreateSelectQuery (X509CertificateRecordFields fields)
		{
			var query = new StringBuilder ("SELECT ");
			var columns = GetColumnNames (fields);

			for (int i = 0; i < columns.Length; i++) {
				if (i > 0)
					query = query.Append (", ");

				query = query.Append (columns[i]);
			}

			return query.Append (" FROM CERTIFICATES");
		}

		/// <summary>
		/// Gets the database command to select the record matching the specified certificate.
		/// </summary>
		/// <remarks>
		/// Gets the database command to select the record matching the specified certificate.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="certificate">The certificate.</param>
		/// <param name="fields">The fields to return.</param>
		protected override DbCommand GetSelectCommand (X509Certificate certificate, X509CertificateRecordFields fields)
		{
			var fingerprint = certificate.GetFingerprint ().ToLowerInvariant ();
			var serialNumber = certificate.SerialNumber.ToString ();
			var issuerName = certificate.IssuerDN.ToString ();
			var command = connection.CreateCommand ();
			var query = CreateSelectQuery (fields);

			// FIXME: Is this really the best way to query for an exact match of a certificate?
			query = query.Append (" WHERE ISSUERNAME = @ISSUERNAME AND SERIALNUMBER = @SERIALNUMBER AND FINGERPRINT = @FINGERPRINT LIMIT 1");
			command.AddParameterWithValue ("@ISSUERNAME", issuerName);
			command.AddParameterWithValue ("@SERIALNUMBER", serialNumber);
			command.AddParameterWithValue ("@FINGERPRINT", fingerprint);

			command.CommandText = query.ToString ();
			command.CommandType = CommandType.Text;

			return command;
		}

		/// <summary>
		/// Get the database command to select the certificate records for the specified mailbox.
		/// </summary>
		/// <remarks>
		/// Gets the database command to select the certificate records for the specified mailbox.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <param name="now">The date and time for which the certificate should be valid.</param>
		/// <param name="requirePrivateKey">true</param>
		/// <param name="fields">The fields to return.</param>
		protected override DbCommand GetSelectCommand (MailboxAddress mailbox, DateTime now, bool requirePrivateKey, X509CertificateRecordFields fields)
		{
			var secure = mailbox as SecureMailboxAddress;
			var command = connection.CreateCommand ();
			var query = CreateSelectQuery (fields);

			query = query.Append (" WHERE BASICCONSTRAINTS = @BASICCONSTRAINTS ");
			command.AddParameterWithValue ("@BASICCONSTRAINTS", -1);

			if (secure != null && !string.IsNullOrEmpty (secure.Fingerprint)) {
				if (secure.Fingerprint.Length < 40) {
					command.AddParameterWithValue ("@FINGERPRINT", secure.Fingerprint.ToLowerInvariant () + "%");
					query = query.Append ("AND FINGERPRINT LIKE @FINGERPRINT ");
				} else {
					command.AddParameterWithValue ("@FINGERPRINT", secure.Fingerprint.ToLowerInvariant ());
					query = query.Append ("AND FINGERPRINT = @FINGERPRINT ");
				}
			} else {
				command.AddParameterWithValue ("@SUBJECTEMAIL", mailbox.Address.ToLowerInvariant ());
				query = query.Append ("AND SUBJECTEMAIL = @SUBJECTEMAIL ");
			}

			query = query.Append ("AND NOTBEFORE < @NOW AND NOTAFTER > @NOW");
			command.AddParameterWithValue ("@NOW", now.ToUniversalTime ());

			if (requirePrivateKey)
				query = query.Append (" AND PRIVATEKEY IS NOT NULL");

			command.CommandText = query.ToString ();
			command.CommandType = CommandType.Text;

			return command;
		}

		/// <summary>
		/// Get the database command to select the requested certificate records.
		/// </summary>
		/// <remarks>
		/// Gets the database command to select the requested certificate records.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="selector">The certificate selector.</param>
		/// <param name="trustedOnly"><c>true</c> if only trusted certificates should be matched; otherwise, <c>false</c>.</param>
		/// <param name="requirePrivateKey"><c>true</c> if the certificate must have a private key; otherwise, <c>false</c>.</param>
		/// <param name="fields">The fields to return.</param>
		protected override DbCommand GetSelectCommand (IX509Selector selector, bool trustedOnly, bool requirePrivateKey, X509CertificateRecordFields fields)
		{
			var match = selector as X509CertStoreSelector;
			var command = connection.CreateCommand ();
			var query = CreateSelectQuery (fields);
			int baseQueryLength = query.Length;

			query = query.Append (" WHERE ");

			// FIXME: We could create an X509CertificateDatabaseSelector subclass of X509CertStoreSelector that
			// adds properties like bool Trusted, bool Anchor, and bool HasPrivateKey ? Then we could drop the
			// bool method arguments...
			if (trustedOnly) {
				command.AddParameterWithValue ("@TRUSTED", true);
				query = query.Append ("TRUSTED = @TRUSTED");
			}

			// FIXME: This query is used to get the TrustedAnchors in DefaultSecureMimeContext. If the database
			// had an ANCHOR (or ROOT?) column, that would likely improve performance a bit because we would
			// protentially reduce the number of certificates we load.

			if (match != null) {
				if (match.BasicConstraints >= 0 || match.BasicConstraints == -2) {
					if (command.Parameters.Count > 0)
						query = query.Append (" AND ");

					if (match.BasicConstraints == -2) {
						command.AddParameterWithValue ("@BASICCONSTRAINTS", -1);
						query = query.Append ("BASICCONSTRAINTS = @BASICCONSTRAINTS");
					} else {
						command.AddParameterWithValue ("@BASICCONSTRAINTS", match.BasicConstraints);
						query = query.Append ("BASICCONSTRAINTS >= @BASICCONSTRAINTS");
					}
				}

				if (match.CertificateValid != null) {
					if (command.Parameters.Count > 0)
						query = query.Append (" AND ");

					command.AddParameterWithValue ("@DATETIME", match.CertificateValid.Value.ToUniversalTime ());
					query = query.Append ("NOTBEFORE < @DATETIME AND NOTAFTER > @DATETIME");
				}

				if (match.Issuer != null || match.Certificate != null) {
					// Note: GetSelectCommand (X509Certificate certificate, X509CertificateRecordFields fields)
					// queries for ISSUERNAME, SERIALNUMBER, and FINGERPRINT so we'll do the same.
					var issuer = match.Issuer ?? match.Certificate.IssuerDN;

					if (command.Parameters.Count > 0)
						query = query.Append (" AND ");

					command.AddParameterWithValue ("@ISSUERNAME", issuer.ToString ());
					query = query.Append ("ISSUERNAME = @ISSUERNAME");
				}

				if (match.SerialNumber != null || match.Certificate != null) {
					// Note: GetSelectCommand (X509Certificate certificate, X509CertificateRecordFields fields)
					// queries for ISSUERNAME, SERIALNUMBER, and FINGERPRINT so we'll do the same.
					var serialNumber = match.SerialNumber ?? match.Certificate.SerialNumber;

					if (command.Parameters.Count > 0)
						query = query.Append (" AND ");

					command.AddParameterWithValue ("@SERIALNUMBER", serialNumber.ToString ());
					query = query.Append ("SERIALNUMBER = @SERIALNUMBER");
				}

				if (match.Certificate != null) {
					// Note: GetSelectCommand (X509Certificate certificate, X509CertificateRecordFields fields)
					// queries for ISSUERNAME, SERIALNUMBER, and FINGERPRINT so we'll do the same.
					if (command.Parameters.Count > 0)
						query = query.Append (" AND ");

					command.AddParameterWithValue ("@FINGERPRINT", match.Certificate.GetFingerprint ());
					query = query.Append ("FINGERPRINT = @FINGERPRINT");
				}

				// FIXME: maybe the database should have a SUBJECTNAME column as well? Then we could match against
				// selector.SubjectDN. Plus it might be nice to have when querying the database manually to see
				// what's there.

				if (match.KeyUsage != null) {
					var flags = BouncyCastleCertificateExtensions.GetKeyUsageFlags (match.KeyUsage);

					if (flags != X509KeyUsageFlags.None) {
						if (command.Parameters.Count > 0)
							query = query.Append (" AND ");

						command.AddParameterWithValue ("@FLAGS", (int) flags);
						query = query.Append ("(KEYUSAGE = 0 OR (KEYUSAGE & @FLAGS) = @FLAGS)");
					}
				}
			}

			if (requirePrivateKey) {
				if (command.Parameters.Count > 0)
					query = query.Append (" AND ");

				query = query.Append ("PRIVATEKEY IS NOT NULL");
			} else if (command.Parameters.Count == 0) {
				query.Length = baseQueryLength;
			}

			command.CommandText = query.ToString ();
			command.CommandType = CommandType.Text;

			return command;
		}

		/// <summary>
		/// Gets the database command to select the CRL records matching the specified issuer.
		/// </summary>
		/// <remarks>
		/// Gets the database command to select the CRL records matching the specified issuer.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="issuer">The issuer.</param>
		/// <param name="fields">The fields to return.</param>
		protected override DbCommand GetSelectCommand (X509Name issuer, X509CrlRecordFields fields)
		{
			var query = "SELECT " + string.Join (", ", GetColumnNames (fields)) + " FROM CRLS ";
			var command = connection.CreateCommand ();

			command.CommandText = query + "WHERE ISSUERNAME = @ISSUERNAME";
			command.AddParameterWithValue ("@ISSUERNAME", issuer.ToString ());
			command.CommandType = CommandType.Text;

			return command;
		}

		/// <summary>
		/// Gets the database command to select the record for the specified CRL.
		/// </summary>
		/// <remarks>
		/// Gets the database command to select the record for the specified CRL.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="crl">The X.509 CRL.</param>
		/// <param name="fields">The fields to return.</param>
		protected override DbCommand GetSelectCommand (X509Crl crl, X509CrlRecordFields fields)
		{
			var query = "SELECT " + string.Join (", ", GetColumnNames (fields)) + " FROM CRLS ";
			var issuerName = crl.IssuerDN.ToString ();
			var command = connection.CreateCommand ();

			command.CommandText = query + "WHERE DELTA = @DELTA AND ISSUERNAME = @ISSUERNAME AND THISUPDATE = @THISUPDATE LIMIT 1";
			command.AddParameterWithValue ("@DELTA", crl.IsDelta ());
			command.AddParameterWithValue ("@ISSUERNAME", issuerName);
			command.AddParameterWithValue ("@THISUPDATE", crl.ThisUpdate.ToUniversalTime ());
			command.CommandType = CommandType.Text;

			return command;
		}

		/// <summary>
		/// Gets the database command to select all CRLs in the table.
		/// </summary>
		/// <remarks>
		/// Gets the database command to select all CRLs in the table.
		/// </remarks>
		/// <returns>The database command.</returns>
		protected override DbCommand GetSelectAllCrlsCommand ()
		{
			var command = connection.CreateCommand ();

			command.CommandText = "SELECT ID, CRL FROM CRLS";
			command.CommandType = CommandType.Text;

			return command;
		}

		/// <summary>
		/// Gets the database command to delete the specified certificate record.
		/// </summary>
		/// <remarks>
		/// Gets the database command to delete the specified certificate record.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="record">The certificate record.</param>
		protected override DbCommand GetDeleteCommand (X509CertificateRecord record)
		{
			var command = connection.CreateCommand ();

			command.CommandText = "DELETE FROM CERTIFICATES WHERE ID = @ID";
			command.AddParameterWithValue ("@ID", record.Id);
			command.CommandType = CommandType.Text;

			return command;
		}

		/// <summary>
		/// Gets the database command to delete the specified CRL record.
		/// </summary>
		/// <remarks>
		/// Gets the database command to delete the specified CRL record.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="record">The record.</param>
		protected override DbCommand GetDeleteCommand (X509CrlRecord record)
		{
			var command = connection.CreateCommand ();

			command.CommandText = "DELETE FROM CRLS WHERE ID = @ID";
			command.AddParameterWithValue ("@ID", record.Id);
			command.CommandType = CommandType.Text;

			return command;
		}

		/// <summary>
		/// Gets the database command to insert the specified certificate record.
		/// </summary>
		/// <remarks>
		/// Gets the database command to insert the specified certificate record.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="record">The certificate record.</param>
		protected override DbCommand GetInsertCommand (X509CertificateRecord record)
		{
			var statement = new StringBuilder ("INSERT INTO CERTIFICATES(");
			var columns = X509CertificateRecord.ColumnNames;
			var variables = new StringBuilder ("VALUES(");
			var command = connection.CreateCommand ();

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
		/// <remarks>
		/// Gets the database command to insert the specified CRL record.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="record">The CRL record.</param>
		protected override DbCommand GetInsertCommand (X509CrlRecord record)
		{
			var statement = new StringBuilder ("INSERT INTO CRLS(");
			var variables = new StringBuilder ("VALUES(");
			var columns = X509CrlRecord.ColumnNames;
			var command = connection.CreateCommand ();

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
		/// <remarks>
		/// Gets the database command to update the specified record.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="record">The certificate record.</param>
		/// <param name="fields">The fields to update.</param>
		protected override DbCommand GetUpdateCommand (X509CertificateRecord record, X509CertificateRecordFields fields)
		{
			var statement = new StringBuilder ("UPDATE CERTIFICATES SET ");
			var columns = GetColumnNames (fields & ~X509CertificateRecordFields.Id);
			var command = connection.CreateCommand ();

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
		/// <remarks>
		/// Gets the database command to update the specified CRL record.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="record">The CRL record.</param>
		protected override DbCommand GetUpdateCommand (X509CrlRecord record)
		{
			var statement = new StringBuilder ("UPDATE CRLS SET ");
			var columns = X509CrlRecord.ColumnNames;
			var command = connection.CreateCommand ();

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
		/// <remarks>
		/// Releases the unmanaged resources used by the <see cref="SqliteCertificateDatabase"/> and
		/// optionally releases the managed resources.
		/// </remarks>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
		/// <c>false</c> to release only the unmanaged resources.</param>
		protected override void Dispose (bool disposing)
		{
			if (disposing && !disposed) {
				if (connection != null)
					connection.Dispose ();
				disposed = true;
			}

			base.Dispose (disposing);
		}
	}
}

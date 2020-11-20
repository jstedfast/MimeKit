//
// SqlCertificateDatabase.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 .NET Foundation and Contributors
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
using System.Collections.Generic;

#if __MOBILE__
using Mono.Data.Sqlite;
#else
using System.Reflection;
#endif

using Org.BouncyCastle.Asn1;
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
		bool disposed;

		/// <summary>
		/// Initialize a new instance of the <see cref="SqlCertificateDatabase"/> class.
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

			Connection = connection;

			if (connection.State != ConnectionState.Open)
				connection.Open ();

			CertificatesTable = CreateCertificatesDataTable ("CERTIFICATES");
			CrlsTable = CreateCrlsDataTable ("CRLS");

			CreateCertificatesTable (CertificatesTable);
			CreateCrlsTable (CrlsTable);
		}

		/// <summary>
		/// Gets the database connection.
		/// </summary>
		/// <remarks>
		/// Gets the database connection.
		/// </remarks>
		/// <value>The database connection.</value>
		protected DbConnection Connection {
			get; private set; // TODO: push this down into X509CertificateDatabase and fix GetSelectCommand()/GetInsertCommand()/etc to take a DbConnection arg, then this could be private.
		}

		/// <summary>
		/// Gets the X.509 certificate table definition.
		/// </summary>
		/// <remarks>
		/// Gets the X.509 certificate table definition.
		/// </remarks>
		/// <value>The X.509 certificates table definition.</value>
		protected DataTable CertificatesTable {
			get; private set;
		}

		/// <summary>
		/// Gets the X.509 certificate revocation lists (CRLs) table definition.
		/// </summary>
		/// <remarks>
		/// Gets the X.509 certificate revocation lists (CRLs) table definition.
		/// </remarks>
		/// <value>The X.509 certificate revocation lists table definition.</value>
		protected DataTable CrlsTable {
			get; private set;
		}

#if NETSTANDARD1_3 || NETSTANDARD1_6
#pragma warning disable 1591
		protected class DataColumn
		{
			public DataColumn (string columnName, Type dataType)
			{
				ColumnName = columnName;
				DataType = dataType;
			}

			public DataColumn ()
			{
			}

			public bool AllowDBNull {
				get; set;
			}

			public bool AutoIncrement {
				get; set;
			}

			public string ColumnName {
				get; set;
			}

			public Type DataType {
				get; set;
			}

			public bool Unique {
				get; set;
			}
		}

		protected class DataColumnCollection : List<DataColumn>
		{
			public int IndexOf (string columnName)
			{
				for (int i = 0; i < Count; i++) {
					if (this[i].ColumnName.Equals (columnName, StringComparison.Ordinal))
						return i;
				}

				return -1;
			}
		}

		protected class DataTable
		{
			public DataTable (string tableName)
			{
				Columns = new DataColumnCollection ();
				TableName = tableName;
			}

			public string TableName {
				get; set;
			}

			public DataColumnCollection Columns {
				get; private set;
			}

			public DataColumn[] PrimaryKey {
				get; set;
			}
		}
#pragma warning restore 1591
#endif

		static DataTable CreateCertificatesDataTable (string tableName)
		{
			var table = new DataTable (tableName);
			table.Columns.Add (new DataColumn ("ID", typeof (int)) { AutoIncrement = true });
			table.Columns.Add (new DataColumn ("TRUSTED", typeof (bool)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn ("ANCHOR", typeof (bool)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn ("BASICCONSTRAINTS", typeof (int)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn ("KEYUSAGE", typeof (int)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn ("NOTBEFORE", typeof (long)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn ("NOTAFTER", typeof (long)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn ("ISSUERNAME", typeof (string)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn ("SERIALNUMBER", typeof (string)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn ("SUBJECTNAME", typeof (string)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn ("SUBJECTKEYIDENTIFIER", typeof (string)) { AllowDBNull = true });
			table.Columns.Add (new DataColumn ("SUBJECTEMAIL", typeof (string)) { AllowDBNull = true });
			table.Columns.Add (new DataColumn ("FINGERPRINT", typeof (string)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn ("ALGORITHMS", typeof (string)) { AllowDBNull = true });
			table.Columns.Add (new DataColumn ("ALGORITHMSUPDATED", typeof (long)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn ("CERTIFICATE", typeof (byte[])) { AllowDBNull = false, Unique = true });
			table.Columns.Add (new DataColumn ("PRIVATEKEY", typeof (byte[])) { AllowDBNull = true });
			table.PrimaryKey = new DataColumn[] { table.Columns[0] };

			return table;
		}

		static DataTable CreateCrlsDataTable (string tableName)
		{
			var table = new DataTable (tableName);
			table.Columns.Add (new DataColumn ("ID", typeof (int)) { AutoIncrement = true });
			table.Columns.Add (new DataColumn ("DELTA", typeof (bool)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn ("ISSUERNAME", typeof (string)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn ("THISUPDATE", typeof (long)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn ("NEXTUPDATE", typeof (long)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn ("CRL", typeof (byte[])) { AllowDBNull = false });
			table.PrimaryKey = new DataColumn[] { table.Columns[0] };

			return table;
		}

		/// <summary>
		/// Gets the columns for the specified table.
		/// </summary>
		/// <remarks>
		/// Gets the list of columns for the specified table.
		/// </remarks>
		/// <param name="connection">The <see cref="System.Data.Common.DbConnection"/>.</param>
		/// <param name="tableName">The name of the table.</param>
		/// <returns>The list of columns.</returns>
		protected abstract IList<DataColumn> GetTableColumns (DbConnection connection, string tableName);

		/// <summary>
		/// Gets the command to create a table.
		/// </summary>
		/// <remarks>
		/// Constructs the command to create a table.
		/// </remarks>
		/// <param name="connection">The <see cref="System.Data.Common.DbConnection"/>.</param>
		/// <param name="table">The table.</param>
		protected abstract void CreateTable (DbConnection connection, DataTable table);

		/// <summary>
		/// Adds a column to a table.
		/// </summary>
		/// <remarks>
		/// Adds a column to a table.
		/// </remarks>
		/// <param name="connection">The <see cref="System.Data.Common.DbConnection"/>.</param>
		/// <param name="table">The table.</param>
		/// <param name="column">The column to add.</param>
		protected abstract void AddTableColumn (DbConnection connection, DataTable table, DataColumn column);

		/// <summary>
		/// Gets the name of an index based on the table and columns that it is built against.
		/// </summary>
		/// <remarks>
		/// Gets the name of an index based on the table and columns that it is built against.
		/// </remarks>
		/// <param name="tableName">The name of the table.</param>
		/// <param name="columnNames">The names of the columns that are indexed.</param>
		/// <returns></returns>
		protected string GetIndexName (string tableName, string[] columnNames)
		{
			return string.Format ("{0}_{1}_INDEX", tableName, string.Join ("_", columnNames));
		}

		/// <summary>
		/// Creates an index for faster table lookups.
		/// </summary>
		/// <remarks>
		/// Creates an index for faster table lookups.
		/// </remarks>
		/// <param name="connection">The <see cref="System.Data.Common.DbConnection"/>.</param>
		/// <param name="tableName">The name of the table.</param>
		/// <param name="columnNames">The names of the columns to index.</param>
		protected virtual void CreateIndex (DbConnection connection, string tableName, string[] columnNames)
		{
			var indexName = GetIndexName (tableName, columnNames);
			var query = string.Format ("CREATE INDEX IF NOT EXISTS {0} ON {1}({2})", indexName, tableName, string.Join (", ", columnNames));

			using (var command = connection.CreateCommand ()) {
				command.CommandText = query;
				command.ExecuteNonQuery ();
			}
		}

		/// <summary>
		/// Removes an index that is no longer needed.
		/// </summary>
		/// <remarks>
		/// Removes an index that is no longer needed.
		/// </remarks>
		/// <param name="connection">The <see cref="System.Data.Common.DbConnection"/>.</param>
		/// <param name="tableName">The name of the table.</param>
		/// <param name="columnNames">The names of the columns that were indexed.</param>
		protected virtual void RemoveIndex (DbConnection connection, string tableName, string[] columnNames)
		{
			var indexName = GetIndexName (tableName, columnNames);
			var query = string.Format ("DROP INDEX IF EXISTS {0}", indexName);

			using (var command = connection.CreateCommand ()) {
				command.CommandText = query;
				command.ExecuteNonQuery ();
			}
		}

		void CreateCertificatesTable (DataTable table)
		{
			CreateTable (Connection, table);

			var currentColumns = GetTableColumns (Connection, table.TableName);
			bool hasAnchorColumn = false;

			for (int i = 0; i < currentColumns.Count; i++) {
				if (currentColumns[i].ColumnName.Equals ("ANCHOR", StringComparison.Ordinal)) {
					hasAnchorColumn = true;
					break;
				}
			}

			// Note: The ANCHOR, SUBJECTNAME and SUBJECTKEYIDENTIFIER columns were all added in the same version,
			// so if the ANCHOR column is missing, they all are.
			if (!hasAnchorColumn) {
				using (var transaction = Connection.BeginTransaction ()) {
					try {
						var column = table.Columns[table.Columns.IndexOf ("ANCHOR")];
						AddTableColumn (Connection, table, column);

						column = table.Columns[table.Columns.IndexOf ("SUBJECTNAME")];
						AddTableColumn (Connection, table, column);

						column = table.Columns[table.Columns.IndexOf ("SUBJECTKEYIDENTIFIER")];
						AddTableColumn (Connection, table, column);

						foreach (var record in Find (null, false, X509CertificateRecordFields.Id | X509CertificateRecordFields.Certificate)) {
							var statement = "UPDATE CERTIFICATES SET ANCHOR = @ANCHOR, SUBJECTNAME = @SUBJECTNAME, SUBJECTKEYIDENTIFIER = @SUBJECTKEYIDENTIFIER WHERE ID = @ID";

							using (var command = Connection.CreateCommand ()) {
								command.AddParameterWithValue ("@ID", record.Id);
								command.AddParameterWithValue ("@ANCHOR", record.IsAnchor);
								command.AddParameterWithValue ("@SUBJECTNAME", record.SubjectName);
								command.AddParameterWithValue ("@SUBJECTKEYIDENTIFIER", record.SubjectKeyIdentifier?.AsHex ());
								command.CommandType = CommandType.Text;
								command.CommandText = statement;

								command.ExecuteNonQuery ();
							}
						}

						transaction.Commit ();
					} catch {
						transaction.Rollback ();
						throw;
					}
				}

				// Remove some old indexes
				RemoveIndex (Connection, table.TableName, new[] { "TRUSTED" });
				RemoveIndex (Connection, table.TableName, new[] { "TRUSTED", "BASICCONSTRAINTS", "ISSUERNAME", "SERIALNUMBER" });
				RemoveIndex (Connection, table.TableName, new[] { "BASICCONSTRAINTS", "ISSUERNAME", "SERIALNUMBER" });
				RemoveIndex (Connection, table.TableName, new[] { "BASICCONSTRAINTS", "FINGERPRINT" });
				RemoveIndex (Connection, table.TableName, new[] { "BASICCONSTRAINTS", "SUBJECTEMAIL" });
			}

			// Note: Use "EXPLAIN QUERY PLAN SELECT ... FROM CERTIFICATES WHERE ..." to verify that any indexes we create get used as expected.

			// Index for matching against a specific certificate
			CreateIndex (Connection, table.TableName, new [] { "ISSUERNAME", "SERIALNUMBER", "FINGERPRINT" });

			// Index for searching for a certificate based on a SecureMailboxAddress
			CreateIndex (Connection, table.TableName, new [] { "BASICCONSTRAINTS", "FINGERPRINT", "NOTBEFORE", "NOTAFTER" });

			// Index for searching for a certificate based on a MailboxAddress
			CreateIndex (Connection, table.TableName, new [] { "BASICCONSTRAINTS", "SUBJECTEMAIL", "NOTBEFORE", "NOTAFTER" });

			// Index for gathering a list of Trusted Anchors
			CreateIndex (Connection, table.TableName, new [] { "TRUSTED", "ANCHOR", "KEYUSAGE" });
		}

		void CreateCrlsTable (DataTable table)
		{
			CreateTable (Connection, table);

			CreateIndex (Connection, table.TableName, new [] { "ISSUERNAME" });
			CreateIndex (Connection, table.TableName, new [] { "DELTA", "ISSUERNAME", "THISUPDATE" });
		}

		/// <summary>
		/// Creates a SELECT query string builder for the specified fields of an X.509 certificate record.
		/// </summary>
		/// <remarks>
		/// Creates a SELECT query string builder for the specified fields of an X.509 certificate record.
		/// </remarks>
		/// <param name="fields">THe X.509 certificate fields.</param>
		/// <returns>A <see cref="StringBuilder"/> containing a basic SELECT query string.</returns>
		protected StringBuilder CreateSelectQuery (X509CertificateRecordFields fields)
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
			var command = Connection.CreateCommand ();
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
			var command = Connection.CreateCommand ();
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
		/// <param name="trustedAnchorsOnly"><c>true</c> if only trusted anchor certificates should be matched; otherwise, <c>false</c>.</param>
		/// <param name="requirePrivateKey"><c>true</c> if the certificate must have a private key; otherwise, <c>false</c>.</param>
		/// <param name="fields">The fields to return.</param>
		protected override DbCommand GetSelectCommand (IX509Selector selector, bool trustedAnchorsOnly, bool requirePrivateKey, X509CertificateRecordFields fields)
		{
			var match = selector as X509CertStoreSelector;
			var command = Connection.CreateCommand ();
			var query = CreateSelectQuery (fields);
			int baseQueryLength = query.Length;

			query = query.Append (" WHERE ");

			// FIXME: We could create an X509CertificateDatabaseSelector subclass of X509CertStoreSelector that
			// adds properties like bool Trusted, bool Anchor, and bool HasPrivateKey ? Then we could drop the
			// bool method arguments...
			if (trustedAnchorsOnly) {
				query = query.Append ("TRUSTED = @TRUSTED AND ANCHOR = @ANCHOR");
				command.AddParameterWithValue ("@TRUSTED", true);
				command.AddParameterWithValue ("@ANCHOR", true);
			}

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

				if (match.Subject != null) {
					if (command.Parameters.Count > 0)
						query = query.Append (" AND ");

					command.AddParameterWithValue ("@SUBJECTNAME", match.Subject.ToString ());
					query = query.Append ("SUBJECTNAME = @SUBJECTNAME");
				}

				if (match.SubjectKeyIdentifier != null) {
					if (command.Parameters.Count > 0)
						query = query.Append (" AND ");

					var id = (Asn1OctetString) Asn1Object.FromByteArray (match.SubjectKeyIdentifier);
					var subjectKeyIdentifier = id.GetOctets ().AsHex ();

					command.AddParameterWithValue ("@SUBJECTKEYIDENTIFIER", subjectKeyIdentifier);
					query = query.Append ("SUBJECTKEYIDENTIFIER = @SUBJECTKEYIDENTIFIER");
				}

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
			var command = Connection.CreateCommand ();

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
			var command = Connection.CreateCommand ();

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
			var command = Connection.CreateCommand ();

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
			var command = Connection.CreateCommand ();

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
			var command = Connection.CreateCommand ();

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
			var variables = new StringBuilder ("VALUES(");
			var command = Connection.CreateCommand ();
			var columns = CertificatesTable.Columns;

			for (int i = 1; i < columns.Count; i++) {
				if (i > 1) {
					statement.Append (", ");
					variables.Append (", ");
				}

				var value = GetValue (record, columns[i].ColumnName);
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
			var command = Connection.CreateCommand ();
			var columns = CrlsTable.Columns;

			for (int i = 1; i < columns.Count; i++) {
				if (i > 1) {
					statement.Append (", ");
					variables.Append (", ");
				}

				var value = GetValue (record, columns[i].ColumnName);
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
			var command = Connection.CreateCommand ();

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
			var command = Connection.CreateCommand ();
			var columns = CrlsTable.Columns;

			for (int i = 1; i < columns.Count; i++) {
				var value = GetValue (record, columns[i].ColumnName);
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
				if (Connection != null)
					Connection.Dispose ();
				disposed = true;
			}

			base.Dispose (disposing);
		}
	}
}

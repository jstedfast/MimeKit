//
// SqlCertificateDatabase.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
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
using System.Linq;
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
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Collections;

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
		readonly Dictionary<string, string> paramNameCache = new Dictionary<string, string> (StringComparer.Ordinal);
		bool disposed;

		/// <summary>
		/// Initialize a new instance of the <see cref="SqlCertificateDatabase"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="SqlCertificateDatabase"/> using the provided database connection.
		/// </remarks>
		/// <param name="connection">The database connection.</param>
		/// <param name="password">The password used for encrypting and decrypting the private keys.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="connection"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <see langword="null"/>.</para>
		/// </exception>
		protected SqlCertificateDatabase (DbConnection connection, string password) : this (connection, password, new SecureRandom ())
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="SqlCertificateDatabase"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="SqlCertificateDatabase"/> using the provided database connection.
		/// </remarks>
		/// <param name="connection">The database connection.</param>
		/// <param name="password">The password used for encrypting and decrypting the private keys.</param>
		/// <param name="random">The secure pseuido-random number generator.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="connection"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="random"/> is <see langword="null"/>.</para>
		/// </exception>
		protected SqlCertificateDatabase (DbConnection connection, string password, SecureRandom random) : base (connection, password, random)
		{
			if (connection.State != ConnectionState.Open)
				connection.Open ();

			CertificatesTable = CreateCertificatesDataTable (CertificatesTableName);
			CrlsTable = CreateCrlsDataTable (CrlsTableName);

			CreateCertificatesTable (connection, CertificatesTable);
			CreateCrlsTable (connection, CrlsTable);
		}

		/// <summary>
		/// Get the X.509 certificate table definition.
		/// </summary>
		/// <remarks>
		/// Gets the X.509 certificate table definition.
		/// </remarks>
		/// <value>The X.509 certificates table definition.</value>
		protected DataTable CertificatesTable {
			get; private set;
		}

		/// <summary>
		/// Get the X.509 certificate revocation lists (CRLs) table definition.
		/// </summary>
		/// <remarks>
		/// Gets the X.509 certificate revocation lists (CRLs) table definition.
		/// </remarks>
		/// <value>The X.509 certificate revocation lists table definition.</value>
		protected DataTable CrlsTable {
			get; private set;
		}

		static DataTable CreateCertificatesDataTable (string tableName)
		{
			var table = new DataTable (tableName);
			table.Columns.Add (new DataColumn (CertificateColumnNames.Id, typeof (int)) { AutoIncrement = true });
			table.Columns.Add (new DataColumn (CertificateColumnNames.Trusted, typeof (bool)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn (CertificateColumnNames.Anchor, typeof (bool)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn (CertificateColumnNames.BasicConstraints, typeof (int)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn (CertificateColumnNames.KeyUsage, typeof (int)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn (CertificateColumnNames.NotBefore, typeof (long)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn (CertificateColumnNames.NotAfter, typeof (long)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn (CertificateColumnNames.IssuerName, typeof (string)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn (CertificateColumnNames.SerialNumber, typeof (string)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn (CertificateColumnNames.SubjectName, typeof (string)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn (CertificateColumnNames.SubjectKeyIdentifier, typeof (string)) { AllowDBNull = true });
			table.Columns.Add (new DataColumn (CertificateColumnNames.SubjectEmail, typeof (string)) { AllowDBNull = true });
			table.Columns.Add (new DataColumn (CertificateColumnNames.SubjectDnsNames, typeof (string)) { AllowDBNull = true });
			table.Columns.Add (new DataColumn (CertificateColumnNames.Fingerprint, typeof (string)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn (CertificateColumnNames.Algorithms, typeof (string)) { AllowDBNull = true });
			table.Columns.Add (new DataColumn (CertificateColumnNames.AlgorithmsUpdated, typeof (long)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn (CertificateColumnNames.Certificate, typeof (byte[])) { AllowDBNull = false, Unique = true });
			table.Columns.Add (new DataColumn (CertificateColumnNames.PrivateKey, typeof (byte[])) { AllowDBNull = true });
			table.PrimaryKey = new DataColumn[] { table.Columns[0] };

			return table;
		}

		static DataTable CreateCrlsDataTable (string tableName)
		{
			var table = new DataTable (tableName);
			table.Columns.Add (new DataColumn (CrlColumnNames.Id, typeof (int)) { AutoIncrement = true });
			table.Columns.Add (new DataColumn (CrlColumnNames.Delta, typeof (bool)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn (CrlColumnNames.IssuerName, typeof (string)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn (CrlColumnNames.ThisUpdate, typeof (long)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn (CrlColumnNames.NextUpdate, typeof (long)) { AllowDBNull = false });
			table.Columns.Add (new DataColumn (CrlColumnNames.Crl, typeof (byte[])) { AllowDBNull = false });
			table.PrimaryKey = new DataColumn[] { table.Columns[0] };

			return table;
		}

		/// <summary>
		/// Gets the columns for the specified table.
		/// </summary>
		/// <remarks>
		/// Gets the list of columns for the specified table.
		/// </remarks>
		/// <param name="connection">The database connection.</param>
		/// <param name="tableName">The name of the table.</param>
		/// <returns>The list of columns.</returns>
		protected abstract IList<DataColumn> GetTableColumns (DbConnection connection, string tableName);

		/// <summary>
		/// Gets the command to create a table.
		/// </summary>
		/// <remarks>
		/// Constructs the command to create a table.
		/// </remarks>
		/// <param name="connection">The database connection.</param>
		/// <param name="table">The table.</param>
		protected abstract void CreateTable (DbConnection connection, DataTable table);

		/// <summary>
		/// Adds a column to a table.
		/// </summary>
		/// <remarks>
		/// Adds a column to a table.
		/// </remarks>
		/// <param name="connection">The database connection.</param>
		/// <param name="table">The table.</param>
		/// <param name="column">The column to add.</param>
		protected abstract void AddTableColumn (DbConnection connection, DataTable table, DataColumn column);

		/// <summary>
		/// Creates a parameter name for the underlying database by prefixing '@' to the specified name.
		/// </summary>
		/// <remarks>
		/// Usually either @name or :name depending on the database.
		/// </remarks>
		/// <param name="name">Original name of the parameter</param>
		/// <returns>The parameter name to be used in queries and <see cref="DbParameter.ParameterName"/>.</returns>
		protected virtual string CreateParameterName (string name)
		{
			return "@" + name;
		}

		string GetParameterName (string columnName)
		{
			if (paramNameCache.TryGetValue (columnName, out var parameterName))
				return parameterName;

			parameterName = CreateParameterName (columnName);
			paramNameCache[columnName] = parameterName;

			return parameterName;
		}

		/// <summary>
		/// Gets the name of an index based on the table and columns that it is built against.
		/// </summary>
		/// <remarks>
		/// Gets the name of an index based on the table and columns that it is built against.
		/// </remarks>
		/// <param name="tableName">The name of the table.</param>
		/// <param name="columnNames">The names of the columns that are indexed.</param>
		/// <returns>The name of the index for the specified table and columns.</returns>
		protected static string GetIndexName (string tableName, string[] columnNames)
		{
			return string.Format ("{0}_{1}_INDEX", tableName, string.Join ("_", columnNames));
		}

		/// <summary>
		/// Creates an index for faster table lookups.
		/// </summary>
		/// <remarks>
		/// Creates an index for faster table lookups.
		/// </remarks>
		/// <param name="connection">The database connection.</param>
		/// <param name="tableName">The name of the table.</param>
		/// <param name="columnNames">The names of the columns to index.</param>
		protected virtual void CreateIndex (DbConnection connection, string tableName, params string[] columnNames)
		{
			var indexName = GetIndexName (tableName, columnNames);
			var query = string.Format ("CREATE INDEX IF NOT EXISTS {0} ON {1}({2})", indexName, tableName, string.Join (", ", columnNames));

			using (var command = CreateCommand ()) {
				command.CommandText = query;
				ExecuteNonQuery (command);
			}
		}

		/// <summary>
		/// Removes an index that is no longer needed.
		/// </summary>
		/// <remarks>
		/// Removes an index that is no longer needed.
		/// </remarks>
		/// <param name="connection">The database connection.</param>
		/// <param name="tableName">The name of the table.</param>
		/// <param name="columnNames">The names of the columns that were indexed.</param>
		protected virtual void RemoveIndex (DbConnection connection, string tableName, params string[] columnNames)
		{
			var indexName = GetIndexName (tableName, columnNames);
			var query = string.Format ("DROP INDEX IF EXISTS {0}", indexName);

			using (var command = CreateCommand ()) {
				command.CommandText = query;
				ExecuteNonQuery (command);
			}
		}

		void CreateCertificatesTable (DbConnection connection, DataTable table)
		{
			CreateTable (connection, table);

			var currentColumns = GetTableColumns (connection, table.TableName);
			bool hasSubjectDnsNamesColumn = false;
			bool hasAnchorColumn = false;

			// Figure out which columns are missing...
			for (int i = 0; i < currentColumns.Count; i++) {
				if (currentColumns[i].ColumnName.Equals (CertificateColumnNames.SubjectDnsNames, StringComparison.Ordinal))
					hasSubjectDnsNamesColumn = true;
				else if (currentColumns[i].ColumnName.Equals (CertificateColumnNames.Anchor, StringComparison.Ordinal))
					hasAnchorColumn = true;
			}

			// Certificates Table Version History:
			//
			// * Version 0: Initial version.
			// * Version 1: v2.5.0 added the ANCHOR, SUBJECTNAME, and SUBJECTKEYIDENTIFIER columns.
			// * Version 2: v4.9.0 added the SUBJECTDNSNAMES column and started canonicalizing the SUBJECTEMAIL and SUBJECTDNSNAMES columns with the IDN-encoded values.

			if (!hasAnchorColumn) {
				// Upgrade from Version 1.
				ExecuteWithinTransaction (() => {
					var idParameter = GetParameterName (CertificateColumnNames.Id);
					var anchorParameter = GetParameterName (CertificateColumnNames.Anchor);
					var subjectNameParameter = GetParameterName (CertificateColumnNames.SubjectName);
					var subjectKeyIdentifierParameter = GetParameterName (CertificateColumnNames.SubjectKeyIdentifier);
					var subjectEmailParameter = GetParameterName (CertificateColumnNames.SubjectEmail);
					var subjectDnsNamesParameter = GetParameterName (CertificateColumnNames.SubjectDnsNames);

					var column = table.Columns[table.Columns.IndexOf (CertificateColumnNames.Anchor)];
					AddTableColumn (connection, table, column);

					column = table.Columns[table.Columns.IndexOf (CertificateColumnNames.SubjectName)];
					AddTableColumn (connection, table, column);

					column = table.Columns[table.Columns.IndexOf (CertificateColumnNames.SubjectKeyIdentifier)];
					AddTableColumn (connection, table, column);

					// Note: The SubjectEmail column exists, but the SubjectDnsNames column was added later, so make sure to add that.
					column = table.Columns[table.Columns.IndexOf (CertificateColumnNames.SubjectDnsNames)];
					AddTableColumn (connection, table, column);

					// Note: We need to call ToArray() or ToList() here because the Find() method will still have a DataReader open which will block calling new DbCommands until the SELECT command completes.
					foreach (var record in Find (null, false, X509CertificateRecordFields.Id | X509CertificateRecordFields.Certificate).ToArray ()) {
						var statement = $"UPDATE {CertificatesTableName} SET {CertificateColumnNames.Anchor} = {anchorParameter}, {CertificateColumnNames.SubjectName} = {subjectNameParameter}, {CertificateColumnNames.SubjectKeyIdentifier} = {subjectKeyIdentifierParameter}, {CertificateColumnNames.SubjectEmail} = {subjectEmailParameter}, {CertificateColumnNames.SubjectDnsNames} = {subjectDnsNamesParameter} WHERE {CertificateColumnNames.Id} = {idParameter}";

						using (var command = CreateCommand ()) {
							command.AddParameterWithValue (idParameter, record.Id);
							command.AddParameterWithValue (anchorParameter, record.IsAnchor);
							command.AddParameterWithValue (subjectNameParameter, record.SubjectName);
							command.AddParameterWithValue (subjectKeyIdentifierParameter, record.SubjectKeyIdentifier?.AsHex ());
							command.AddParameterWithValue (subjectEmailParameter, record.SubjectEmail);
							command.AddParameterWithValue (subjectDnsNamesParameter, EncodeDnsNames (record.SubjectDnsNames));
							command.CommandType = CommandType.Text;
							command.CommandText = statement;

							ExecuteNonQuery (command);
						}
					}
				});

				// Remove some old indexes
				RemoveIndex (connection, table.TableName, CertificateColumnNames.Trusted);
				RemoveIndex (connection, table.TableName, CertificateColumnNames.Trusted, CertificateColumnNames.BasicConstraints, CertificateColumnNames.IssuerName, CertificateColumnNames.SerialNumber);
				RemoveIndex (connection, table.TableName, CertificateColumnNames.BasicConstraints, CertificateColumnNames.IssuerName, CertificateColumnNames.SerialNumber);
				RemoveIndex (connection, table.TableName, CertificateColumnNames.BasicConstraints, CertificateColumnNames.Fingerprint);
				RemoveIndex (connection, table.TableName, CertificateColumnNames.BasicConstraints, CertificateColumnNames.SubjectEmail);
			} else if (!hasSubjectDnsNamesColumn) {
				// Upgrade from Version 2.
				ExecuteWithinTransaction (() => {
					var idParameter = GetParameterName (CertificateColumnNames.Id);
					var subjectEmailParameter = GetParameterName (CertificateColumnNames.SubjectEmail);
					var subjectDnsNamesParameter = GetParameterName (CertificateColumnNames.SubjectDnsNames);

					var column = table.Columns[table.Columns.IndexOf (CertificateColumnNames.SubjectDnsNames)];
					AddTableColumn (connection, table, column);

					// Note: We need to call ToArray() or ToList() here because the Find() method will still have a DataReader open which will block calling new DbCommands until the SELECT command completes.
					foreach (var record in Find (null, false, X509CertificateRecordFields.Id | X509CertificateRecordFields.Certificate).ToArray ()) {
						var statement = $"UPDATE {CertificatesTableName} SET {CertificateColumnNames.SubjectEmail} = {subjectEmailParameter}, {CertificateColumnNames.SubjectDnsNames} = {subjectDnsNamesParameter} WHERE {CertificateColumnNames.Id} = {idParameter}";

						using (var command = CreateCommand ()) {
							command.AddParameterWithValue (idParameter, record.Id);
							command.AddParameterWithValue (subjectEmailParameter, record.SubjectEmail);
							command.AddParameterWithValue (subjectDnsNamesParameter, EncodeDnsNames (record.SubjectDnsNames));
							command.CommandType = CommandType.Text;
							command.CommandText = statement;

							ExecuteNonQuery (command);
						}
					}
				});

				// Remove some old indexes
				RemoveIndex (connection, table.TableName, CertificateColumnNames.BasicConstraints, CertificateColumnNames.SubjectEmail, CertificateColumnNames.NotBefore, CertificateColumnNames.NotAfter);
			}

			// Note: Use "EXPLAIN QUERY PLAN SELECT ... FROM CERTIFICATES WHERE ..." to verify that any indexes we create get used as expected.

			// Index for matching against a specific certificate
			CreateIndex (connection, table.TableName, CertificateColumnNames.IssuerName, CertificateColumnNames.SerialNumber, CertificateColumnNames.Fingerprint);

			// Index for searching for a certificate based on a SecureMailboxAddress
			CreateIndex (connection, table.TableName, CertificateColumnNames.BasicConstraints, CertificateColumnNames.Fingerprint, CertificateColumnNames.NotBefore, CertificateColumnNames.NotAfter);

			// Index for searching for a certificate based on a MailboxAddress
			CreateIndex (connection, table.TableName, CertificateColumnNames.BasicConstraints, CertificateColumnNames.SubjectEmail, CertificateColumnNames.SubjectDnsNames, CertificateColumnNames.NotBefore, CertificateColumnNames.NotAfter);

			// Index for gathering a list of Trusted Anchors
			CreateIndex (connection, table.TableName, CertificateColumnNames.Trusted, CertificateColumnNames.Anchor, CertificateColumnNames.KeyUsage);
		}

		void CreateCrlsTable (DbConnection connection, DataTable table)
		{
			CreateTable (connection, table);

			CreateIndex (connection, table.TableName, CrlColumnNames.IssuerName);
			CreateIndex (connection, table.TableName, CrlColumnNames.Delta, CrlColumnNames.IssuerName, CrlColumnNames.ThisUpdate);
		}

		/// <summary>
		/// Creates a SELECT query string builder for the specified fields of an X.509 certificate record.
		/// </summary>
		/// <remarks>
		/// Creates a SELECT query string builder for the specified fields of an X.509 certificate record.
		/// </remarks>
		/// <param name="fields">The X.509 certificate fields.</param>
		/// <returns>A <see cref="StringBuilder"/> containing a basic SELECT query string.</returns>
		protected static StringBuilder CreateSelectQuery (X509CertificateRecordFields fields)
		{
			var query = new StringBuilder ("SELECT ");
			var columns = GetColumnNames (fields);

			for (int i = 0; i < columns.Length; i++) {
				if (i > 0)
					query = query.Append (", ");

				query = query.Append (columns[i]);
			}

			return query.Append (" FROM ").Append (CertificatesTableName);
		}

		/// <summary>
		/// Creates a SELECT query string builder for the specified fields of an X.509 CRL record.
		/// </summary>
		/// <remarks>
		/// Creates a SELECT query string builder for the specified fields of an X.509 CRL record.
		/// </remarks>
		/// <param name="fields">The X.509 CRL fields.</param>
		/// <returns>A <see cref="StringBuilder"/> containing a basic SELECT query string.</returns>
		protected static StringBuilder CreateSelectQuery (X509CrlRecordFields fields)
		{
			var query = new StringBuilder ("SELECT ");
			var columns = GetColumnNames (fields);

			for (int i = 0; i < columns.Length; i++) {
				if (i > 0)
					query = query.Append (", ");

				query = query.Append (columns[i]);
			}

			return query.Append (" FROM ").Append (CrlsTableName);
		}

		/// <summary>
		/// Gets the database command to select the record matching the specified certificate.
		/// </summary>
		/// <remarks>
		/// Gets the database command to select the record matching the specified certificate.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="connection">The database connection.</param>
		/// <param name="certificate">The certificate.</param>
		/// <param name="fields">The fields to return.</param>
		protected override DbCommand GetSelectCommand (DbConnection connection, X509Certificate certificate, X509CertificateRecordFields fields)
		{
			var fingerprint = certificate.GetFingerprint ().ToLowerInvariant ();
			var serialNumber = certificate.SerialNumber.ToString ();
			var issuerName = certificate.IssuerDN.ToString ();
			var command = CreateCommand ();
			var query = CreateSelectQuery (fields);

			var issuerNameParameter = GetParameterName (CertificateColumnNames.IssuerName);
			var serialNumberParameter = GetParameterName (CertificateColumnNames.SerialNumber);
			var fingerprintParameter = GetParameterName (CertificateColumnNames.Fingerprint);

			// FIXME: Is this really the best way to query for an exact match of a certificate?
			query = query.Append (" WHERE ")
				.Append (CertificateColumnNames.IssuerName).Append (" = ").Append (issuerNameParameter).Append (" AND ")
				.Append (CertificateColumnNames.SerialNumber).Append (" = ").Append (serialNumberParameter).Append (" AND ")
				.Append (CertificateColumnNames.Fingerprint).Append (" = ").Append (fingerprintParameter).Append (" LIMIT 1");
			command.AddParameterWithValue (issuerNameParameter, issuerName);
			command.AddParameterWithValue (serialNumberParameter, serialNumber);
			command.AddParameterWithValue (fingerprintParameter, fingerprint);

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
		/// <param name="connection">The database connection.</param>
		/// <param name="mailbox">The mailbox.</param>
		/// <param name="now">The date and time for which the certificate should be valid.</param>
		/// <param name="requirePrivateKey">true</param>
		/// <param name="fields">The fields to return.</param>
		protected override DbCommand GetSelectCommand (DbConnection connection, MailboxAddress mailbox, DateTime now, bool requirePrivateKey, X509CertificateRecordFields fields)
		{
			var command = CreateCommand ();
			var query = CreateSelectQuery (fields);

			var basicConstraintsParameter = GetParameterName (CertificateColumnNames.BasicConstraints);

			command.AddParameterWithValue (basicConstraintsParameter, -1);
			query = query.Append (" WHERE ").Append (CertificateColumnNames.BasicConstraints).Append (" = ").Append (basicConstraintsParameter).Append (" ");

			if (mailbox is SecureMailboxAddress secure && !string.IsNullOrEmpty (secure.Fingerprint)) {
				var fingerprintParameter = GetParameterName (CertificateColumnNames.Fingerprint);

				if (secure.Fingerprint.Length < 40) {
					command.AddParameterWithValue (fingerprintParameter, secure.Fingerprint.ToLowerInvariant () + "%");
					query = query.Append ("AND ").Append (CertificateColumnNames.Fingerprint).Append (" LIKE ").Append (fingerprintParameter).Append (" ");
				} else {
					command.AddParameterWithValue (fingerprintParameter, secure.Fingerprint.ToLowerInvariant ());
					query = query.Append ("AND ").Append (CertificateColumnNames.Fingerprint).Append (" = ").Append (fingerprintParameter) .Append (" ");
				}
			} else {
				var domain = MailboxAddress.IdnMapping.Encode (mailbox.Domain);
				var address = mailbox.GetAddress (true);

				var subjectEmailParameter = GetParameterName (CertificateColumnNames.SubjectEmail);
				var subjectDnsNameParameter = GetParameterName (CertificateColumnNames.SubjectDnsNames);

				command.AddParameterWithValue (subjectEmailParameter, address.ToLowerInvariant ());
				command.AddParameterWithValue (subjectDnsNameParameter, $"%|{domain.ToLowerInvariant ()}|%");

				query = query.Append ("AND (")
					.Append (CertificateColumnNames.SubjectEmail).Append (" = ").Append (subjectEmailParameter).Append (" OR ")
					.Append (CertificateColumnNames.SubjectDnsNames).Append (" LIKE ").Append (subjectDnsNameParameter).Append (") ");
			}

			var nowParameter = GetParameterName ("NOW");

			query = query.Append ("AND ")
				.Append (CertificateColumnNames.NotBefore).Append (" < ").Append (nowParameter).Append (" AND ")
				.Append (CertificateColumnNames.NotAfter).Append (" > ").Append (nowParameter);
			command.AddParameterWithValue (nowParameter, now.ToUniversalTime ());

			if (requirePrivateKey)
				query = query.Append (" AND ").Append (CertificateColumnNames.PrivateKey).Append (" IS NOT NULL");

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
		/// <param name="connection">The database connection.</param>
		/// <param name="selector">The certificate selector.</param>
		/// <param name="trustedAnchorsOnly"><see langword="true" /> if only trusted anchor certificates should be matched; otherwise, <see langword="false" />.</param>
		/// <param name="requirePrivateKey"><see langword="true" /> if the certificate must have a private key; otherwise, <see langword="false" />.</param>
		/// <param name="fields">The fields to return.</param>
		protected override DbCommand GetSelectCommand (DbConnection connection, ISelector<X509Certificate>? selector, bool trustedAnchorsOnly, bool requirePrivateKey, X509CertificateRecordFields fields)
		{
			var command = CreateCommand ();
			var query = CreateSelectQuery (fields);
			int baseQueryLength = query.Length;

			query = query.Append (" WHERE ");

			// FIXME: We could create an X509CertificateDatabaseSelector subclass of X509CertStoreSelector that
			// adds properties like bool Trusted, bool Anchor, and bool HasPrivateKey ? Then we could drop the
			// bool method arguments...
			if (trustedAnchorsOnly) {
				var trustedParameter = GetParameterName (CertificateColumnNames.Trusted);
				var anchorParameter = GetParameterName (CertificateColumnNames.Anchor);

				query = query.Append (CertificateColumnNames.Trusted).Append (" = ").Append (trustedParameter).Append (" AND ")
					.Append (CertificateColumnNames.Anchor).Append (" = ").Append (anchorParameter);
				command.AddParameterWithValue (trustedParameter, true);
				command.AddParameterWithValue (anchorParameter, true);
			}

			if (selector is X509CertStoreSelector match) {
				if (match.BasicConstraints >= 0 || match.BasicConstraints == -2) {
					var basicConstraintsParameter = GetParameterName (CertificateColumnNames.BasicConstraints);
					if (command.Parameters.Count > 0)
						query = query.Append (" AND ");

					if (match.BasicConstraints == -2) {
						command.AddParameterWithValue (basicConstraintsParameter, -1);
						query = query.Append (CertificateColumnNames.BasicConstraints).Append (" = ").Append (basicConstraintsParameter);
					} else {
						command.AddParameterWithValue (basicConstraintsParameter, match.BasicConstraints);
						query = query.Append (CertificateColumnNames.BasicConstraints).Append (" >= ").Append (basicConstraintsParameter);
					}
				}

				if (match.CertificateValid != null) {
					if (command.Parameters.Count > 0)
						query = query.Append (" AND ");

					var dateTimeParameter = GetParameterName ("DATETIME");
					command.AddParameterWithValue (dateTimeParameter, match.CertificateValid.Value.ToUniversalTime ());
					query = query.Append (CertificateColumnNames.NotBefore).Append (" < ").Append (dateTimeParameter).Append (" AND ")
						.Append (CertificateColumnNames.NotAfter).Append (" > ").Append (dateTimeParameter);
				}

				if (match.Issuer != null || match.Certificate != null) {
					// Note: GetSelectCommand (X509Certificate certificate, X509CertificateRecordFields fields)
					// queries for ISSUERNAME, SERIALNUMBER, and FINGERPRINT so we'll do the same.
					var issuer = match.Issuer ?? match.Certificate.IssuerDN;

					if (command.Parameters.Count > 0)
						query = query.Append (" AND ");

					var issuerNameParameter = GetParameterName (CertificateColumnNames.IssuerName);
					command.AddParameterWithValue (issuerNameParameter, issuer.ToString ());
					query = query.Append (CertificateColumnNames.IssuerName).Append (" = ").Append (issuerNameParameter);
				}

				var serialNumber = match.SerialNumber ?? match.Certificate?.SerialNumber;

				if (serialNumber != null) {
					// Note: GetSelectCommand (X509Certificate certificate, X509CertificateRecordFields fields)
					// queries for ISSUERNAME, SERIALNUMBER, and FINGERPRINT so we'll do the same.

					if (command.Parameters.Count > 0)
						query = query.Append (" AND ");

					var serialNumberParameter = GetParameterName (CertificateColumnNames.SerialNumber);
					command.AddParameterWithValue (serialNumberParameter, serialNumber.ToString ());
					query = query.Append (CertificateColumnNames.SerialNumber).Append (" = ").Append (serialNumberParameter);
				}

				if (match.Certificate != null) {
					// Note: GetSelectCommand (X509Certificate certificate, X509CertificateRecordFields fields)
					// queries for ISSUERNAME, SERIALNUMBER, and FINGERPRINT so we'll do the same.
					if (command.Parameters.Count > 0)
						query = query.Append (" AND ");

					var fingerprintParameter = GetParameterName (CertificateColumnNames.Fingerprint);
					command.AddParameterWithValue (fingerprintParameter, match.Certificate.GetFingerprint ());
					query = query.Append (CertificateColumnNames.Fingerprint).Append (" = ").Append (fingerprintParameter);
				}

				if (match.Subject != null) {
					if (command.Parameters.Count > 0)
						query = query.Append (" AND ");

					var subjectNameParameter = GetParameterName (CertificateColumnNames.SubjectName);
					command.AddParameterWithValue (subjectNameParameter, match.Subject.ToString ());
					query = query.Append (CertificateColumnNames.SubjectName).Append (" = ").Append (subjectNameParameter);
				}

				if (match.SubjectKeyIdentifier != null) {
					if (command.Parameters.Count > 0)
						query = query.Append (" AND ");

					var id = (Asn1OctetString) Asn1Object.FromByteArray (match.SubjectKeyIdentifier);
					var subjectKeyIdentifier = id.GetOctets ().AsHex ();

					var subjectKeyIdentifierParameter = GetParameterName (CertificateColumnNames.SubjectKeyIdentifier);
					command.AddParameterWithValue (subjectKeyIdentifierParameter, subjectKeyIdentifier);
					query = query.Append (CertificateColumnNames.SubjectKeyIdentifier).Append (" = ").Append (subjectKeyIdentifierParameter);
				}

				if (match.KeyUsage != null) {
					var flags = BouncyCastleCertificateExtensions.GetKeyUsageFlags (match.KeyUsage);

					if (flags != X509KeyUsageFlags.None) {
						if (command.Parameters.Count > 0)
							query = query.Append (" AND ");

						var flagsParameter = GetParameterName ("FLAGS");
						command.AddParameterWithValue (flagsParameter, (int) flags);
						query = query.Append ('(').Append (CertificateColumnNames.KeyUsage).Append (" = 0 OR (")
							.Append (CertificateColumnNames.KeyUsage).Append (" & ").Append (flagsParameter).Append (") = ").Append (flagsParameter).Append (")");
					}
				}
			}

			if (requirePrivateKey) {
				if (command.Parameters.Count > 0)
					query = query.Append (" AND ");

				query = query.Append (CertificateColumnNames.PrivateKey).Append (" IS NOT NULL");
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
		/// <param name="connection">The database connection.</param>
		/// <param name="issuer">The issuer.</param>
		/// <param name="fields">The fields to return.</param>
		protected override DbCommand GetSelectCommand (DbConnection connection, X509Name issuer, X509CrlRecordFields fields)
		{
			var issuerNameParameter = GetParameterName (CrlColumnNames.IssuerName);
			var query = CreateSelectQuery (fields).Append (" WHERE ").Append (CrlColumnNames.IssuerName).Append (" = ").Append (issuerNameParameter);
			var command = CreateCommand ();

			command.CommandText = query.ToString ();
			command.AddParameterWithValue (issuerNameParameter, issuer.ToString ());
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
		/// <param name="connection">The database connection.</param>
		/// <param name="crl">The X.509 CRL.</param>
		/// <param name="fields">The fields to return.</param>
		protected override DbCommand GetSelectCommand (DbConnection connection, X509Crl crl, X509CrlRecordFields fields)
		{
			var deltaParameter = GetParameterName (CrlColumnNames.Delta);
			var issuerNameParameter = GetParameterName (CrlColumnNames.IssuerName);
			var thisUpdateParameter = GetParameterName (CrlColumnNames.ThisUpdate);

			var query = CreateSelectQuery (fields).Append (" WHERE ")
				.Append (CrlColumnNames.Delta).Append (" = ").Append (deltaParameter).Append (" AND ")
				.Append (CrlColumnNames.IssuerName).Append ("= ").Append (issuerNameParameter).Append (" AND ")
				.Append (CrlColumnNames.ThisUpdate).Append (" = ").Append (thisUpdateParameter).Append (" LIMIT 1");
			var issuerName = crl.IssuerDN.ToString ();
			var command = CreateCommand ();

			command.CommandText = query.ToString ();
			command.AddParameterWithValue (deltaParameter, crl.IsDelta ());
			command.AddParameterWithValue (issuerNameParameter, issuerName);
			command.AddParameterWithValue (thisUpdateParameter, crl.ThisUpdate.ToUniversalTime ());
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
		/// <param name="connection">The database connection.</param>
		protected override DbCommand GetSelectAllCrlsCommand (DbConnection connection)
		{
			var command = CreateCommand ();

			command.CommandText = $"SELECT {CrlColumnNames.Id}, {CrlColumnNames.Crl} FROM {CrlsTableName}";
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
		/// <param name="connection">The database connection.</param>
		/// <param name="record">The certificate record.</param>
		protected override DbCommand GetDeleteCommand (DbConnection connection, X509CertificateRecord record)
		{
			var command = CreateCommand ();

			var idParameter = GetParameterName (CertificateColumnNames.Id);
			command.CommandText = $"DELETE FROM {CertificatesTableName} WHERE {CertificateColumnNames.Id} = {idParameter}";
			command.AddParameterWithValue (idParameter, record.Id);
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
		/// <param name="connection">The database connection.</param>
		/// <param name="record">The record.</param>
		protected override DbCommand GetDeleteCommand (DbConnection connection, X509CrlRecord record)
		{
			var command = CreateCommand ();

			var idParameter = GetParameterName (CrlColumnNames.Id);
			command.CommandText = $"DELETE FROM {CrlsTableName} WHERE {CrlColumnNames.Id} = {idParameter}";
			command.AddParameterWithValue (idParameter, record.Id);
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
		/// <param name="connection">The database connection.</param>
		/// <param name="record">The certificate record.</param>
		protected override DbCommand GetInsertCommand (DbConnection connection, X509CertificateRecord record)
		{
			var statement = new StringBuilder ("INSERT INTO ").Append (CertificatesTableName).Append ('(');
			var variables = new StringBuilder ("VALUES(");
			var command = CreateCommand ();
			var columns = CertificatesTable.Columns;

			for (int i = 1; i < columns.Count; i++) {
				if (i > 1) {
					statement.Append (", ");
					variables.Append (", ");
				}

				var value = GetValue (record, columns[i].ColumnName);
				var variable = GetParameterName (columns[i].ColumnName);

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
		/// <param name="connection">The database connection.</param>
		/// <param name="record">The CRL record.</param>
		protected override DbCommand GetInsertCommand (DbConnection connection, X509CrlRecord record)
		{
			var statement = new StringBuilder ("INSERT INTO ").Append (CrlsTableName).Append ('(');
			var variables = new StringBuilder ("VALUES(");
			var command = CreateCommand ();
			var columns = CrlsTable.Columns;

			for (int i = 1; i < columns.Count; i++) {
				if (i > 1) {
					statement.Append (", ");
					variables.Append (", ");
				}

				var value = GetValue (record, columns[i].ColumnName);
				var variable = GetParameterName (columns[i].ColumnName);

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
		/// <param name="connection">The database connection.</param>
		/// <param name="record">The certificate record.</param>
		/// <param name="fields">The fields to update.</param>
		protected override DbCommand GetUpdateCommand (DbConnection connection, X509CertificateRecord record, X509CertificateRecordFields fields)
		{
			var statement = new StringBuilder ("UPDATE ").Append (CertificatesTableName).Append (" SET ");
			var columns = GetColumnNames (fields & ~X509CertificateRecordFields.Id);
			var command = CreateCommand ();

			for (int i = 0; i < columns.Length; i++) {
				var value = GetValue (record, columns[i]);
				var variable = GetParameterName (columns[i]);

				if (i > 0)
					statement.Append (", ");

				statement.Append (columns[i]);
				statement.Append (" = ");
				statement.Append (variable);

				command.AddParameterWithValue (variable, value);
			}

			var idParameter = GetParameterName (CertificateColumnNames.Id);
			statement.Append (" WHERE ").Append (CertificateColumnNames.Id).Append (" = ").Append (idParameter);
			command.AddParameterWithValue (idParameter, record.Id);

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
		/// <param name="connection">The database connection.</param>
		/// <param name="record">The CRL record.</param>
		[Obsolete ("This method is not used and will be removed in a future release.")]
		protected override DbCommand GetUpdateCommand (DbConnection connection, X509CrlRecord record)
		{
			var statement = new StringBuilder ("UPDATE ").Append (CrlsTableName).Append (" SET ");
			var command = CreateCommand ();
			var columns = CrlsTable.Columns;

			for (int i = 1; i < columns.Count; i++) {
				var value = GetValue (record, columns[i].ColumnName);
				var variable = GetParameterName (columns[i].ColumnName);

				if (i > 1)
					statement.Append (", ");

				statement.Append (columns[i]);
				statement.Append (" = ");
				statement.Append (variable);

				command.AddParameterWithValue (variable, value);
			}

			var idParameter = GetParameterName (CrlColumnNames.Id);
			statement.Append (" WHERE ").Append (CrlColumnNames.Id).Append (" = ").Append (idParameter);
			command.AddParameterWithValue (idParameter, record.Id);

			command.CommandText = statement.ToString ();
			command.CommandType = CommandType.Text;

			return command;
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="SqlCertificateDatabase"/> and
		/// optionally releases the managed resources.
		/// </summary>
		/// <remarks>
		/// Releases the unmanaged resources used by the <see cref="SqlCertificateDatabase"/> and
		/// optionally releases the managed resources.
		/// </remarks>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources;
		/// <see langword="false" /> to release only the unmanaged resources.</param>
		protected override void Dispose (bool disposing)
		{
			if (disposing && !disposed) {
				CertificatesTable.Dispose ();
				CrlsTable.Dispose ();
				disposed = true;
			}

			base.Dispose (disposing);
		}
	}
}

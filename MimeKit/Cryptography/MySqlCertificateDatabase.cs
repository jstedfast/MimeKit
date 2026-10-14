//
// MySqlCertificateDatabase.cs
//
// Author: Rob Blackin <rob@5-9z.com>
//
// Copyright (c) 2013-2024 .NET Foundation and Contributors
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
using System.Text;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;

using Org.BouncyCastle.X509;
using Org.BouncyCastle.Security;

using MimeKit.Utils;
using System.Runtime.CompilerServices;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An X.509 certificate database built on SQL Server.
	/// </summary>
	/// <remarks>
	/// <para>An X.509 certificate database is used for storing certificates, metdata related to the certificates
	/// (such as encryption algorithms supported by the associated client), certificate revocation lists (CRLs),
	/// and private keys.</para>
	/// <para>This particular database uses MySql to store the data.</para>
	/// </remarks>
	public class MySqlCertificateDatabase : SqlCertificateDatabase
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="MySqlCertificateDatabase"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MySqlCertificateDatabase"/> using the provided SQL Server database connection.
		/// </remarks>
		/// <param name="connection">The SQL Server connection.</param>
		/// <param name="password">The password used for encrypting and decrypting the private keys.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="connection"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		public MySqlCertificateDatabase (DbConnection connection, string password) : base (connection, password)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MySqlCertificateDatabase"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MySqlCertificateDatabase"/> using the provided SQL Server database connection.
		/// </remarks>
		/// <param name="connection">The SQL Server connection.</param>
		/// <param name="password">The password used for encrypting and decrypting the private keys.</param>
		/// <param name="random">The secure pseudo-random number generator.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="connection"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="random"/> is <c>null</c>.</para>
		/// </exception>
		public MySqlCertificateDatabase (DbConnection connection, string password, SecureRandom random) : base (connection, password, random)
		{
		}

		/// <summary>
		/// Adds a column to a table.
		/// </summary>
		/// <remarks>
		/// Adds a column to a table.
		/// </remarks>
		/// <param name="connection">The database connection.</param>
		/// <param name="table">The table.</param>
		/// <param name="column">The column to add.</param>
		protected override void AddTableColumn (DbConnection connection, DataTable table, DataColumn column)
		{
			var statement = new StringBuilder ("ALTER TABLE ");
			int primaryKeys = table.PrimaryKey?.Length ?? 0;

			statement.Append (table.TableName);
			statement.Append (" ADD COLUMN ");
			Build (statement, table, column, ref primaryKeys);

			using (var command = connection.CreateCommand ()) {
				command.CommandText = statement.ToString ();
				command.CommandType = CommandType.Text;
				command.ExecuteNonQuery ();
			}
		}

		/// <summary>
		/// Create a table.
		/// </summary>
		/// <remarks>
		/// Creates the specified table.
		/// </remarks>
		/// <param name="connection">The database connection.</param>
		/// <param name="table">The table.</param>
		protected override void CreateTable (DbConnection connection, DataTable table)
		{
			int primaryKeys = 0;

			var statement = new StringBuilder ($"Create table IF NOT EXISTS {table.TableName} (");

			foreach (DataColumn column in table.Columns) {
				Build (statement, table, column, ref primaryKeys);
				statement.Append (", ");
			}

			if (table.Columns.Count > 0)
				statement.Length -= 2;

			statement.Append (')');

			using (var command = connection.CreateCommand ()) {
				command.CommandText = statement.ToString ();
				command.CommandType = CommandType.Text;
				command.ExecuteNonQuery ();
			}
		}

		static void Build (StringBuilder statement, DataTable table, DataColumn column, ref int primaryKeys)
		{
			statement.Append (column.ColumnName);
			statement.Append (' ');

			if (column.DataType == typeof (long) || column.DataType == typeof (int)) {
				if (column.AutoIncrement)
					statement.Append ("int NOT NULL AUTO_INCREMENT");
				else if (column.DataType == typeof (long))
					statement.Append ("DateTime(3)");
				else
					statement.Append ("int");
			} else if (column.DataType == typeof (bool)) {
				statement.Append ("tinyint UNSIGNED");
			} else if (column.DataType == typeof (byte[])) {
				statement.Append ($"varbinary(4096)");
			} else if (column.DataType == typeof (string)) {
				statement.Append ("varchar(256)");
			} else {
				throw new NotImplementedException ();
			}

			if (table != null && table.PrimaryKey != null && primaryKeys < table.PrimaryKey.Length) {
				for (int i = 0; i < table.PrimaryKey.Length; i++) {
					if (column == table.PrimaryKey[i]) {
						statement.Append (" PRIMARY KEY ");
						primaryKeys++;
						break;
					}
				}
			}

			if (!column.AllowDBNull)
				statement.Append (" NOT NULL");
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
		protected override IList<DataColumn> GetTableColumns (DbConnection connection, string tableName)
		{
			using (var command = connection.CreateCommand ()) {
				command.CommandText = $"select * from {tableName} LIMIT 1";
				using (var reader = command.ExecuteReader ()) {
					var columns = new List<DataColumn> ();
					var table = reader.GetSchemaTable ();

					foreach (DataRow row in table.Rows)
						columns.Add (new DataColumn { ColumnName = row.Field<string> ("ColumnName") });

					return columns;
				}
			}
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
		protected override void CreateIndex (DbConnection connection, string tableName, string[] columnNames)
		{
			var indexName = GenerateIndexName (tableName, columnNames);
			var query = string.Format ("SELECT COUNT(distinct index_name) indexExists FROM INFORMATION_SCHEMA.STATISTICS WHERE table_schema=DATABASE() AND table_name='{0}' AND index_name='{1}';", tableName, indexName);
			var indexExists = 0;
			using (var command = connection.CreateCommand ()) {
				command.CommandText = query;
				using (var reader = command.ExecuteReader ()) {
					reader.Read();
					indexExists=reader.GetInt32 (0);
				}
			}

			if (indexExists == 0) {
				using (var command = connection.CreateCommand ()) {
					command.CommandText = string.Format("CREATE INDEX {0} ON {1}({2})", indexName, tableName, string.Join (", ", columnNames));
					command.ExecuteNonQuery ();
				}
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
		protected override void RemoveIndex (DbConnection connection, string tableName, string[] columnNames)
		{
			var indexName = GenerateIndexName (tableName, columnNames);
			var query = string.Format ("SELECT COUNT(distinct index_name) indexExists FROM INFORMATION_SCHEMA.STATISTICS WHERE table_schema=DATABASE() AND table_name='{0}' AND index_name='{1}';", tableName, indexName);
			var indexExists = 0;
			using (var command = connection.CreateCommand ()) {
				command.CommandText = query;
				using (var reader = command.ExecuteReader ()) {
					reader.Read ();
					indexExists = reader.GetInt32 (0);
				}
			}

			if (indexExists == 1) {
				using (var command = connection.CreateCommand ()) {
					command.CommandText = string.Format ("DROP INDEX {0} ON {1}", indexName, tableName);
					command.ExecuteNonQuery ();
				}
			}
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
			var command = connection.CreateCommand ();
			var query = CreateSelectQuery (fields);

			// FIXME: Is this really the best way to query for an exact match of a certificate?
			query = query.Append (" WHERE ISSUERNAME = @ISSUERNAME AND SERIALNUMBER = @SERIALNUMBER AND FINGERPRINT = @FINGERPRINT LIMIT 1;");
			command.AddParameterWithValue ("@ISSUERNAME", issuerName);
			command.AddParameterWithValue ("@SERIALNUMBER", serialNumber);
			command.AddParameterWithValue ("@FINGERPRINT", fingerprint);

			command.CommandText = query.ToString ();
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
			var statement = new StringBuilder ("INSERT INTO CERTIFICATES(");
			var variables = new StringBuilder ("VALUES(");
			var command = connection.CreateCommand ();
			var columns = CertificatesTable.Columns;

			for (int i = 1; i < columns.Count; i++) {
				if (i > 1) {
					statement.Append (", ");
					variables.Append (", ");
				}

				var value = GetValue (record, columns[i].ColumnName);
				if (value is DateTime dateTime && dateTime < DateUtils.UnixEpoch)
					value = DateUtils.UnixEpoch;

				if (columns[i].ColumnName == "PRIVATEKEY" && value is DBNull)
					value = Array.Empty<byte> ();

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

		private static string GenerateIndexName(string tableName, string[] columnNames)
		{
			var name = string.Format ("{0}_{1}_INDEX", tableName, string.Join ("_", columnNames));
			return name.Length <= 64 ? name : name.Substring (0, 56) + name.GetHashCode().ToString("X8");
		}
	}
}

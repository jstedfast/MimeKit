//
// NpgsqlCertificateDatabase.cs
//
// Author: Federico Di Gregorio <fog@initd.org>
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
using System.IO;
using System.Data;
using System.Text;
using System.Reflection;
using System.Data.Common;
using System.Collections.Generic;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An X.509 certificate database built on PostgreSQL.
	/// </summary>
	/// <remarks>
	/// <para>An X.509 certificate database is used for storing certificates, metdata related to the certificates
	/// (such as encryption algorithms supported by the associated client), certificate revocation lists (CRLs),
	/// and private keys.</para>
	/// <para>This particular database uses PostgreSQL to store the data.</para>
	/// </remarks>
	public class NpgsqlCertificateDatabase : SqlCertificateDatabase
	{
		static readonly Type npgsqlConnectionClass;
		static readonly Assembly npgsqlAssembly;

		static NpgsqlCertificateDatabase ()
		{
			try {
				npgsqlAssembly = Assembly.Load ("Npgsql");
				if (npgsqlAssembly != null) {
					npgsqlConnectionClass = npgsqlAssembly.GetType ("Npgsql.NpgsqlConnection");

					IsAvailable = true;
				}
			} catch (FileNotFoundException) {
			} catch (FileLoadException) {
			} catch (BadImageFormatException) {
			}
		}

		internal static bool IsAvailable {
			get; private set;
		}

		static DbConnection CreateConnection (string connectionString)
		{
			if (connectionString == null)
				throw new ArgumentNullException (nameof (connectionString));

			if (connectionString.Length == 0)
				throw new ArgumentException ("The connection string cannot be empty.", nameof (connectionString));

			return (DbConnection) Activator.CreateInstance (npgsqlConnectionClass, new [] { connectionString });
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.NpgsqlCertificateDatabase"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="NpgsqlCertificateDatabase"/> and opens a connection to the
		/// PostgreSQL database using the specified connection string.</para>
		/// </remarks>
		/// <param name="connectionString">The connection string.</param>
		/// <param name="password">The password used for encrypting and decrypting the private keys.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="connectionString"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="connectionString"/> is empty.
		/// </exception>
		public NpgsqlCertificateDatabase (string connectionString, string password) : base (CreateConnection (connectionString), password)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.NpgsqlCertificateDatabase"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="NpgsqlCertificateDatabase"/> using the provided Npgsql database connection.
		/// </remarks>
		/// <param name="connection">The Npgsql connection.</param>
		/// <param name="password">The password used for encrypting and decrypting the private keys.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="connection"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		public NpgsqlCertificateDatabase (DbConnection connection, string password) : base (connection, password)
		{
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
		protected override IList<DataColumn> GetTableColumns (DbConnection connection, string tableName)
		{
			using (var command = connection.CreateCommand ()) {
				command.CommandText = $"PRAGMA table_info({tableName})";
				using (var reader = command.ExecuteReader ()) {
					var columns = new List<DataColumn> ();

					while (reader.Read ()) {
						var column = new DataColumn ();

						for (int i = 0; i < reader.FieldCount; i++) {
							var field = reader.GetName (i).ToUpperInvariant ();

							switch (field) {
							case "NAME":
								column.ColumnName = reader.GetString (i);
								break;
							case "TYPE":
								var type = reader.GetString (i);
								switch (type) {
								case "boolean": column.DataType = typeof (bool); break;
								case "integer": column.DataType = typeof (long); break;
								case "bytea": column.DataType = typeof (byte[]); break;
								case "text": column.DataType = typeof (string); break;
								}
								break;
							case "NOTNULL":
								column.AllowDBNull = !reader.GetBoolean (i);
								break;
							}
						}

						columns.Add (column);
					}

					return columns;
				}
			}
		}

		static void Build (StringBuilder statement, DataTable table, DataColumn column, ref int primaryKeys)
		{
			statement.Append (column.ColumnName);
			statement.Append (' ');

			if (column.DataType == typeof (long) || column.DataType == typeof (int)) {
				if (column.AutoIncrement)
					statement.Append ("serial");
				else
					statement.Append ("integer");
			} else if (column.DataType == typeof (bool)) {
				statement.Append ("boolean");
			} else if (column.DataType == typeof (byte[])) {
				statement.Append ("bytea");
			} else if (column.DataType == typeof (string)) {
				statement.Append ("text");
			} else {
				throw new NotImplementedException ();
			}

			bool isPrimaryKey = false;
			if (table != null && table.PrimaryKey != null && primaryKeys < table.PrimaryKey.Length) {
				for (int i = 0; i < table.PrimaryKey.Length; i++) {
					if (column == table.PrimaryKey[i]) {
						statement.Append (" PRIMARY KEY");
						isPrimaryKey = true;
						primaryKeys++;
						break;
					}
				}
			}

			if (column.Unique && !isPrimaryKey)
				statement.Append (" UNIQUE");

			if (!column.AllowDBNull)
				statement.Append (" NOT NULL");
		}

		/// <summary>
		/// Create a table.
		/// </summary>
		/// <remarks>
		/// Creates the specified table.
		/// </remarks>
		/// <param name="connection">The <see cref="System.Data.Common.DbConnection"/>.</param>
		/// <param name="table">The table.</param>
		protected override void CreateTable (DbConnection connection, DataTable table)
		{
			var statement = new StringBuilder ("CREATE TABLE IF NOT EXISTS ");
			int primaryKeys = 0;

			statement.Append (table.TableName);
			statement.Append ('(');

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

		/// <summary>
		/// Adds a column to a table.
		/// </summary>
		/// <remarks>
		/// Adds a column to a table.
		/// </remarks>
		/// <param name="connection">The <see cref="System.Data.Common.DbConnection"/>.</param>
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
	}
}

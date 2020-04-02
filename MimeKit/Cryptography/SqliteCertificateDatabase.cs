﻿//
// SqliteCertificateDatabase.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 Xamarin Inc. (www.xamarin.com)
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
using System.Data.Common;
using System.Collections.Generic;

#if __MOBILE__
using Mono.Data.Sqlite;
#else
using System.Reflection;
#endif

namespace MimeKit.Cryptography {
	/// <summary>
	/// An X.509 certificate database built on SQLite.
	/// </summary>
	/// <remarks>
	/// <para>An X.509 certificate database is used for storing certificates, metdata related to the certificates
	/// (such as encryption algorithms supported by the associated client), certificate revocation lists (CRLs),
	/// and private keys.</para>
	/// <para>This particular database uses SQLite to store the data.</para>
	/// </remarks>
	public class SqliteCertificateDatabase : SqlCertificateDatabase
	{
#if !__MOBILE__
		static readonly Type sqliteConnectionStringBuilderClass;
		static readonly Type sqliteConnectionClass;
		static readonly Assembly sqliteAssembly;
#endif

		// At class initialization we try to use reflection to load the
		// Mono.Data.Sqlite assembly: this allows us to use Sqlite as the 
		// default certificate store without explicitly depending on the
		// assembly.
		static SqliteCertificateDatabase ()
		{
#if __MOBILE__
			IsAvailable = true;
#endif

#if NETFRAMEWORK || NETSTANDARD2_0 || NETCOREAPP3_0
			var platform = Environment.OSVersion.Platform;
#endif

#if NETSTANDARD1_3 || NETSTANDARD1_6 || NETSTANDARD2_0 || NETCOREAPP3_0
			try {
				if ((sqliteAssembly = Assembly.Load (new AssemblyName ("Microsoft.Data.Sqlite"))) != null) {
					sqliteConnectionClass = sqliteAssembly.GetType ("Microsoft.Data.Sqlite.SqliteConnection");
					sqliteConnectionStringBuilderClass = sqliteAssembly.GetType ("Microsoft.Data.Sqlite.SqliteConnectionStringBuilder");

					// Make sure that the runtime can load the native sqlite library
					var builder = Activator.CreateInstance (sqliteConnectionStringBuilderClass);

					IsAvailable = true;
					return;
				}
			} catch (FileNotFoundException) {
			} catch (FileLoadException) {
			} catch (BadImageFormatException) {
			}
#endif

#if NETFRAMEWORK || NETCOREAPP3_0
			try {
				// Mono.Data.Sqlite will only work on Unix-based platforms and 32-bit Windows platforms.
				if (platform == PlatformID.Unix || platform == PlatformID.MacOSX || IntPtr.Size == 4) {
					if ((sqliteAssembly = Assembly.Load ("Mono.Data.Sqlite")) != null) {
						sqliteConnectionClass = sqliteAssembly.GetType ("Mono.Data.Sqlite.SqliteConnection");
						sqliteConnectionStringBuilderClass = sqliteAssembly.GetType ("Mono.Data.Sqlite.SqliteConnectionStringBuilder");

						// Make sure that the runtime can load the native sqlite3 library
						var builder = Activator.CreateInstance (sqliteConnectionStringBuilderClass);
						sqliteConnectionStringBuilderClass.GetProperty ("DateTimeFormat").SetValue (builder, 0, null);

						IsAvailable = true;
						return;
					}
				}
			} catch (FileNotFoundException) {
			} catch (FileLoadException) {
			} catch (BadImageFormatException) {
			}
#endif

#if NETFRAMEWORK || NETSTANDARD2_0 || NETCOREAPP3_0
			try {
				if ((sqliteAssembly = Assembly.Load ("System.Data.SQLite")) != null) {
					sqliteConnectionClass = sqliteAssembly.GetType ("System.Data.SQLite.SQLiteConnection");
					sqliteConnectionStringBuilderClass = sqliteAssembly.GetType ("System.Data.SQLite.SQLiteConnectionStringBuilder");

					// Make sure that the runtime can load the native sqlite3 library
					var builder = Activator.CreateInstance (sqliteConnectionStringBuilderClass);
					sqliteConnectionStringBuilderClass.GetProperty ("DateTimeFormat").SetValue (builder, 0, null);

					IsAvailable = true;
					return;
				}
			} catch (FileNotFoundException) {
			} catch (FileLoadException) {
			} catch (BadImageFormatException) {
			}
#endif
		}

		internal static bool IsAvailable {
			get; private set;
		}

		static DbConnection CreateConnection (string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException (nameof (fileName));

			if (fileName.Length == 0)
				throw new ArgumentException ("The file name cannot be empty.", nameof (fileName));

			if (!File.Exists (fileName)) {
				var dir = Path.GetDirectoryName (fileName);

				if (!string.IsNullOrEmpty (dir) && !Directory.Exists (dir))
					Directory.CreateDirectory (dir);

#if __MOBILE__
				SqliteConnection.CreateFile (fileName);
#else
				File.Create (fileName).Dispose ();
#endif
			}

#if !__MOBILE__
			var dateTimeFormat = sqliteConnectionStringBuilderClass.GetProperty ("DateTimeFormat");
			var builder = Activator.CreateInstance (sqliteConnectionStringBuilderClass);

			sqliteConnectionStringBuilderClass.GetProperty ("DataSource").SetValue (builder, fileName, null);

			if (dateTimeFormat != null)
				dateTimeFormat.SetValue (builder, 0, null);

			var connectionString = (string) sqliteConnectionStringBuilderClass.GetProperty ("ConnectionString").GetValue (builder, null);

			return (DbConnection) Activator.CreateInstance (sqliteConnectionClass, new [] { connectionString });
#else
			var builder = new SqliteConnectionStringBuilder ();
			builder.DateTimeFormat = SQLiteDateFormats.Ticks;
			builder.DataSource = fileName;

			return new SqliteConnection (builder.ConnectionString);
#endif
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.SqliteCertificateDatabase"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="SqliteCertificateDatabase"/> and opens a connection to the
		/// SQLite database at the specified path using the Mono.Data.Sqlite binding to the native
		/// SQLite library.</para>
		/// <para>If Mono.Data.Sqlite is not available or if an alternative binding to the native
		/// SQLite library is preferred, then consider using
		/// <see cref="SqlCertificateDatabase(System.Data.Common.DbConnection,string)"/> instead.</para>
		/// </remarks>
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
		public SqliteCertificateDatabase (string fileName, string password) : this (CreateConnection (fileName), password)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.SqliteCertificateDatabase"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="SqliteCertificateDatabase"/> using the provided SQLite database connection.
		/// </remarks>
		/// <param name="connection">The SQLite connection.</param>
		/// <param name="password">The password used for encrypting and decrypting the private keys.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="connection"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		public SqliteCertificateDatabase (DbConnection connection, string password) : base (connection, password)
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
								case "INTEGER": column.DataType = typeof (long); break;
								case "BLOB": column.DataType = typeof (byte[]); break;
								case "TEXT": column.DataType = typeof (string); break;
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

		static void Build (StringBuilder statement, DataTable table, DataColumn column, ref int primaryKeys, bool create)
		{
			statement.Append (column.ColumnName);
			statement.Append (' ');

			if (column.DataType == typeof (long) || column.DataType == typeof (int) || column.DataType == typeof (bool)) {
				statement.Append ("INTEGER");
			} else if (column.DataType == typeof (byte[])) {
				statement.Append ("BLOB");
			} else if (column.DataType == typeof (string)) {
				statement.Append ("TEXT");
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

			if (column.AutoIncrement)
				statement.Append (" AUTOINCREMENT");

			if (column.Unique && !isPrimaryKey)
				statement.Append (" UNIQUE");

			// Note: Normally we'd want to include NOT NULL, but we can't *add* new columns with the NOT NULL restriction
			if (create && !column.AllowDBNull)
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
				Build (statement, table, column, ref primaryKeys, true);
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
			Build (statement, table, column, ref primaryKeys, false);

			using (var command = connection.CreateCommand ()) {
				command.CommandText = statement.ToString ();
				command.CommandType = CommandType.Text;
				command.ExecuteNonQuery ();
			}
		}
	}
}

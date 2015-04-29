//
// SqliteCertificateDatabase.cs
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
using System.IO;
using System.Data;
using System.Text;

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
#if !__MOBILE__
			var platform = Environment.OSVersion.Platform;

			try {
				// Mono.Data.Sqlite will only work on Unix-based platforms and 32-bit Windows platforms.
				if (platform == PlatformID.Unix || platform == PlatformID.MacOSX || IntPtr.Size == 4) {
					sqliteAssembly = Assembly.Load ("Mono.Data.Sqlite");
					if (sqliteAssembly != null) {
						sqliteConnectionClass = sqliteAssembly.GetType ("Mono.Data.Sqlite.SqliteConnection");
						sqliteConnectionStringBuilderClass = sqliteAssembly.GetType ("Mono.Data.Sqlite.SqliteConnectionStringBuilder");

						// Make sure that the runtime can load the native sqlite3 library
						var builder = Activator.CreateInstance (sqliteConnectionStringBuilderClass);
						sqliteConnectionStringBuilderClass.GetProperty ("DateTimeFormat").SetValue (builder, 0, null);

						IsAvailable = true;
					}
				}

				// System.Data.Sqlite is only available for Windows-based platforms.
				if (!IsAvailable && platform != PlatformID.Unix && platform != PlatformID.MacOSX) {
					sqliteAssembly = Assembly.Load ("System.Data.SQLite");
					if (sqliteAssembly != null) {
						sqliteConnectionClass = sqliteAssembly.GetType ("System.Data.SQLite.SQLiteConnection");
						sqliteConnectionStringBuilderClass = sqliteAssembly.GetType ("System.Data.SQLite.SQLiteConnectionStringBuilder");

						// Make sure that the runtime can load the native sqlite3 library
						var builder = Activator.CreateInstance (sqliteConnectionStringBuilderClass);
						sqliteConnectionStringBuilderClass.GetProperty ("DateTimeFormat").SetValue (builder, 0, null);

						IsAvailable = true;
					}
				}
			} catch (FileNotFoundException) {
			} catch (FileLoadException) {
			} catch (BadImageFormatException) {
			}
#else
			IsAvailable = true;
#endif
		}

		internal static bool IsAvailable {
			get; private set;
		}

		static IDbConnection CreateConnection (string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			if (fileName.Length == 0)
				throw new ArgumentException ("The file name cannot be empty.", "fileName");

#if !__MOBILE__
			var builder = Activator.CreateInstance (sqliteConnectionStringBuilderClass);
			sqliteConnectionStringBuilderClass.GetProperty ("DateTimeFormat").SetValue (builder, 0, null);
			sqliteConnectionStringBuilderClass.GetProperty ("DataSource").SetValue (builder, fileName, null);

			if (!File.Exists (fileName))
				sqliteConnectionClass.GetMethod ("CreateFile").Invoke (null, new [] {fileName});

			var connectionString = (string) sqliteConnectionStringBuilderClass.GetProperty ("ConnectionString").GetValue (builder, null);

			return (IDbConnection) Activator.CreateInstance (sqliteConnectionClass, new [] { connectionString });
#else
			var builder = new SqliteConnectionStringBuilder ();
			builder.DateTimeFormat = SQLiteDateFormats.Ticks;
			builder.DataSource = fileName;

			if (!File.Exists (fileName))
				SqliteConnection.CreateFile (fileName);

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
		/// <see cref="SqliteCertificateDatabase(System.Data.IDbConnection,string)"/> instead.</para>
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
		public SqliteCertificateDatabase (string fileName, string password) : base (CreateConnection (fileName), password)
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
		public SqliteCertificateDatabase (IDbConnection connection, string password) : base (connection, password)
		{
		}

		/// <summary>
		/// Gets the command to create the certificates table.
		/// </summary>
		/// <remarks>
		/// Constructs the command to create a certificates table suitable for storing
		/// <see cref="X509CertificateRecord"/> objects.
		/// </remarks>
		/// <returns>The <see cref="System.Data.IDbCommand"/>.</returns>
		/// <param name="connection">The <see cref="System.Data.IDbConnection"/>.</param>
		protected override IDbCommand GetCreateCertificatesTableCommand (IDbConnection connection)
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
				case "SUBJECTEMAIL": statement.Append ("TEXT"); break;
				case "FINGERPRINT": statement.Append ("TEXT NOT NULL"); break;
				case "ALGORITHMS": statement.Append ("TEXT"); break;
				case "ALGORITHMSUPDATED": statement.Append ("INTEGER NOT NULL"); break;
				case "CERTIFICATE": statement.Append ("BLOB UNIQUE NOT NULL"); break;
				case "PRIVATEKEY": statement.Append ("BLOB"); break;
				}
			}

			statement.Append (')');

			var command = connection.CreateCommand ();

			command.CommandText = statement.ToString ();
			command.CommandType = CommandType.Text;

			return command;
		}

		/// <summary>
		/// Gets the command to create the CRLs table.
		/// </summary>
		/// <remarks>
		/// Constructs the command to create a CRLs table suitable for storing
		/// <see cref="X509CertificateRecord"/> objects.
		/// </remarks>
		/// <returns>The <see cref="System.Data.IDbCommand"/>.</returns>
		/// <param name="connection">The <see cref="System.Data.IDbConnection"/>.</param>
		protected override IDbCommand GetCreateCrlsTableCommand (IDbConnection connection)
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

			var command = connection.CreateCommand ();

			command.CommandText = statement.ToString ();
			command.CommandType = CommandType.Text;

			return command;
		}
	}
}

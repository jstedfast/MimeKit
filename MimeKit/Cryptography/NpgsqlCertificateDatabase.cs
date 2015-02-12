//
// NpgsqlCertificateDatabase.cs
//
// Author: Federico Di Gregorio <fog@initd.org>
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
using System.Reflection;

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

		static IDbConnection CreateConnection (string connectionString)
		{
			if (connectionString == null)
				throw new ArgumentNullException ("connectionString");

			if (connectionString.Length == 0)
				throw new ArgumentException ("The connection string cannot be empty.", "connectionString");

			return (IDbConnection) Activator.CreateInstance (npgsqlConnectionClass, new [] { connectionString });
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
		public NpgsqlCertificateDatabase (IDbConnection connection, string password) : base (connection, password)
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

				statement.Append (columns[i]).Append (' ');
				switch (columns[i]) {
				case "ID": statement.Append ("serial PRIMARY KEY"); break;
				case "BASICCONSTRAINTS": statement.Append ("integer NOT NULL"); break;
				case "TRUSTED":  statement.Append ("boolean NOT NULL"); break;
				case "KEYUSAGE": statement.Append ("integer NOT NULL"); break;
				case "NOTBEFORE": statement.Append ("timestamp NOT NULL"); break;
				case "NOTAFTER": statement.Append ("timestamp NOT NULL"); break;
				case "ISSUERNAME": statement.Append ("text NOT NULL"); break;
				case "SERIALNUMBER": statement.Append ("text NOT NULL"); break;
				case "SUBJECTEMAIL": statement.Append ("text "); break;
				case "FINGERPRINT": statement.Append ("text NOT NULL"); break;
				case "ALGORITHMS": statement.Append ("text"); break;
				case "ALGORITHMSUPDATED": statement.Append ("timestamp NOT NULL"); break;
				case "CERTIFICATE": statement.Append ("bytea UNIQUE NOT NULL"); break;
				case "PRIVATEKEY": statement.Append ("bytea"); break;
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

				statement.Append (columns[i]).Append (' ');
				switch (columns[i]) {
				case "ID": statement.Append ("serial PRIMARY KEY"); break;
				case "DELTA" : statement.Append ("integer NOT NULL"); break;
				case "ISSUERNAME": statement.Append ("text NOT NULL"); break;
				case "THISUPDATE": statement.Append ("integer NOT NULL"); break;
				case "NEXTUPDATE": statement.Append ("integer NOT NULL"); break;
				case "CRL": statement.Append ("bytea NOT NULL"); break;
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

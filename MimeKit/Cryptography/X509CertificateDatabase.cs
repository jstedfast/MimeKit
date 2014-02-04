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
using System.Data;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using Mono.Data.Sqlite;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509.Store;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An X.509 certificate database.
	/// </summary>
	class X509CertificateDatabase : IDisposable, IX509Store
	{
		static readonly DerObjectIdentifier KeyAlgorithm = PkcsObjectIdentifiers.PbeWithShaAnd3KeyTripleDesCbc;
		const int MinIterations = 1024;
		const int SaltSize = 20;

		SqliteConnection sqlite;
		readonly char[] passwd;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.X509CertificateDatabase"/> class.
		/// </summary>
		/// <param name="fileName">The file name.</param>
		/// <param name="password">The password.</param>
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
		public X509CertificateDatabase (string fileName, string password)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			var builder = new SqliteConnectionStringBuilder ();
			builder.DateTimeFormat = SQLiteDateFormats.Ticks;
			builder.DataSource = fileName;
			//builder.Password = password;

			if (!File.Exists (fileName))
				SqliteConnection.CreateFile (fileName);

			sqlite = new SqliteConnection (builder.ConnectionString);
			sqlite.Open ();

			passwd = password.ToCharArray ();
			CreateCertificatesTable ();
			CreateCrlsTable ();
		}

		static int ReadBinaryBlob (SqliteDataReader reader, int column, ref byte[] buffer)
		{
			long nread;

			// first, get the length of the buffer needed
			if ((nread = reader.GetBytes (column, 0, null, 0, buffer.Length)) > buffer.Length)
				Array.Resize (ref buffer, (int) nread);

			// read the certificate data
			return (int) reader.GetBytes (column, 0, buffer, 0, (int) nread);
		}

		static X509Certificate DecodeCertificate (SqliteDataReader reader, X509CertificateParser parser, int column, ref byte[] buffer)
		{
			int nread = ReadBinaryBlob (reader, column, ref buffer);

			using (var memory = new MemoryStream (buffer, 0, nread, false)) {
				return parser.ReadCertificate (memory);
			}
		}

		static X509Crl DecodeX509Crl (SqliteDataReader reader, X509CrlParser parser, int column, ref byte[] buffer)
		{
			int nread = ReadBinaryBlob (reader, column, ref buffer);

			using (var memory = new MemoryStream (buffer, 0, nread, false)) {
				return parser.ReadCrl (memory);
			}
		}

		byte[] EncryptAsymmetricKeyParameter (AsymmetricKeyParameter key)
		{
			var cipher = PbeUtilities.CreateEngine (KeyAlgorithm.Id) as IBufferedCipher;
			var keyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo (key);
			var random = new SecureRandom ();
			var salt = new byte[SaltSize];

			if (cipher == null)
				throw new Exception ("Unknown encryption algorithm: " + KeyAlgorithm.Id);

			random.NextBytes (salt);

			var pbeParameters = PbeUtilities.GenerateAlgorithmParameters (KeyAlgorithm.Id, salt, MinIterations);
			var algorithm = new AlgorithmIdentifier (KeyAlgorithm, pbeParameters);
			var cipherParameters = PbeUtilities.GenerateCipherParameters (algorithm, passwd);

			cipher.Init (true, cipherParameters);

			var encoded = cipher.DoFinal (keyInfo.GetEncoded ());

			var encrypted = new EncryptedPrivateKeyInfo (algorithm, encoded);

			return encrypted.GetEncoded ();
		}

		AsymmetricKeyParameter DecryptAsymmetricKeyParameter (byte[] buffer, int length)
		{
			using (var memory = new MemoryStream (buffer, 0, length, false)) {
				using (var asn1 = new Asn1InputStream (memory)) {
					var sequence = asn1.ReadObject () as Asn1Sequence;
					if (sequence == null)
						return null;

					var encrypted = EncryptedPrivateKeyInfo.GetInstance (sequence);
					var algorithm = encrypted.EncryptionAlgorithm;
					var encoded = encrypted.GetEncryptedData ();

					var cipher = PbeUtilities.CreateEngine (algorithm) as IBufferedCipher;
					if (cipher == null)
						return null;

					var cipherParameters = PbeUtilities.GenerateCipherParameters (algorithm, passwd);

					cipher.Init (false, cipherParameters);

					var decrypted = cipher.DoFinal (encoded);
					var keyInfo = PrivateKeyInfo.GetInstance (decrypted);

					return PrivateKeyFactory.CreateKey (keyInfo);
				}
			}
		}

		AsymmetricKeyParameter DecodePrivateKey (SqliteDataReader reader, int column, ref byte[] buffer)
		{
			if (reader.IsDBNull (column))
				return null;

			int nread = ReadBinaryBlob (reader, column, ref buffer);

			return DecryptAsymmetricKeyParameter (buffer, nread);
		}

		byte[] EncodePrivateKey (AsymmetricKeyParameter key)
		{
			if (key == null)
				return null;

			return EncryptAsymmetricKeyParameter (key);
		}

		static EncryptionAlgorithm[] DecodeEncryptionAlgorithms (SqliteDataReader reader, int column)
		{
			if (reader.IsDBNull (column))
				return null;

			var algorithms = new List<EncryptionAlgorithm> ();
			var values = reader.GetString (column);

			foreach (var token in values.Split (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) {
				EncryptionAlgorithm algorithm;

				if (Enum.TryParse (token.Trim (), out algorithm))
					algorithms.Add (algorithm);
			}

			return algorithms.ToArray ();
		}

		static string EncodeEncryptionAlgorithms (EncryptionAlgorithm[] algorithms)
		{
			if (algorithms == null)
				return null;

			var tokens = new string[algorithms.Length];
			for (int i = 0; i < algorithms.Length; i++)
				tokens[i] = algorithms[i].ToString ();

			return string.Join (",", tokens);
		}

		X509CertificateRecord LoadCertificateRecord (SqliteDataReader reader, X509CertificateParser parser, ref byte[] buffer)
		{
			var record = new X509CertificateRecord ();

			for (int i = 0; i < reader.FieldCount; i++) {
				switch (reader.GetName (i)) {
				case "CERTIFICATE":
					record.Certificate = DecodeCertificate (reader, parser, i, ref buffer);
					break;
				case "PRIVATEKEY":
					record.PrivateKey = DecodePrivateKey (reader, i, ref buffer);
					break;
				case "ALGORITHMS":
					record.Algorithms = DecodeEncryptionAlgorithms (reader, i);
					break;
				case "ALGORITHMSUPDATED":
					record.AlgorithmsUpdated = reader.GetDateTime (i);
					break;
				case "TRUSTED":
					record.IsTrusted = reader.GetBoolean (i);
					break;
				case "ID":
					record.Id = reader.GetInt32 (i);
					break;
				}
			}

			return record;
		}

		X509CrlRecord LoadCrlRecord (SqliteDataReader reader, X509CrlParser parser, ref byte[] buffer)
		{
			var record = new X509CrlRecord ();

			for (int i = 0; i < reader.FieldCount; i++) {
				switch (reader.GetName (i)) {
				case "CRL":
					record.Crl = DecodeX509Crl (reader, parser, i, ref buffer);
					break;
				case "THISUPDATE":
					record.ThisUpdate = reader.GetDateTime (i);
					break;
				case "NEXTUPDATE":
					record.NextUpdate = reader.GetDateTime (i);
					break;
				case "DELTA":
					record.IsDelta = reader.GetBoolean (i);
					break;
				case "ID":
					record.Id = reader.GetInt32 (i);
					break;
				}
			}

			return record;
		}

		static SqliteCommand GetCreateCertificatesTableCommand (SqliteConnection sqlite)
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

		static SqliteCommand GetCreateCrlsTableCommand (SqliteConnection sqlite)
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

		static string[] GetColumnNames (X509CertificateRecordFields fields)
		{
			var columns = new List<string> ();

			if ((fields & X509CertificateRecordFields.Id) != 0)
				columns.Add ("ID");
			if ((fields & X509CertificateRecordFields.Trusted) != 0)
				columns.Add ("TRUSTED");
			if ((fields & X509CertificateRecordFields.Algorithms) != 0)
				columns.Add ("ALGORITHMS");
			if ((fields & X509CertificateRecordFields.AlgorithmsUpdated) != 0)
				columns.Add ("ALGORITHMSUPDATED");
			if ((fields & X509CertificateRecordFields.Certificate) != 0)
				columns.Add ("CERTIFICATE");
			if ((fields & X509CertificateRecordFields.PrivateKey) != 0)
				columns.Add ("PRIVATEKEY");

			return columns.ToArray ();
		}

		SqliteCommand GetSelectCommand (X509Certificate certificate, X509CertificateRecordFields fields)
		{
			var query = "SELECT " + string.Join (", ", GetColumnNames (fields)) + " FROM CERTIFICATES ";
			var issuerName = certificate.IssuerDN.ToString ();
			var serialNumber = certificate.SerialNumber.ToString ();
			var fingerprint = certificate.GetFingerprint ();
			var command = sqlite.CreateCommand ();

			command.CommandText = query + "WHERE ISSUERNAME = @ISSUERNAME AND SERIALNUMBER = @SERIALNUMBER AND FINGERPRINT = @FINGERPRINT LIMIT 1";
			command.Parameters.AddWithValue ("@ISSUERNAME", issuerName);
			command.Parameters.AddWithValue ("@SERIALNUMBER", serialNumber);
			command.Parameters.AddWithValue ("@FINGERPRINT", fingerprint);
			command.CommandType = CommandType.Text;

			return command;
		}

		SqliteCommand GetSelectCommand (MailboxAddress mailbox, DateTime now, bool requirePrivateKey, X509CertificateRecordFields fields)
		{
			var query = "SELECT " + string.Join (", ", GetColumnNames (fields)) + " FROM CERTIFICATES";
			var secure = mailbox as SecureMailboxAddress;
			var command = sqlite.CreateCommand ();
			var constraints = " WHERE ";

			command.Parameters.AddWithValue ("@BASICCONSTRAINTS", -1);
			constraints += "BASICCONSTRAINTS = @BASICCONSTRAINTS ";

			constraints += "AND NOTBEFORE < @NOW AND NOTAFTER > @NOW ";
			command.Parameters.AddWithValue ("@NOW", now);

			if (requirePrivateKey)
				constraints += "AND PRIVATEKEY NOT NULL ";

			if (secure != null && !string.IsNullOrEmpty (secure.Fingerprint)) {
				if (secure.Fingerprint.Length < 40) {
					constraints += "AND FINGERPRINT LIKE @FINGERPRINT";
					command.Parameters.AddWithValue ("@FINGERPRINT", secure.Fingerprint + "%");
				} else {
					constraints += "AND FINGERPRINT = @FINGERPRINT";
					command.Parameters.AddWithValue ("@FINGERPRINT", secure.Fingerprint);
				}
			} else {
				constraints += "AND SUBJECTEMAIL = @SUBJECTEMAIL";
				command.Parameters.AddWithValue ("@SUBJECTEMAIL", mailbox.Address);
			}

			command.CommandText = query + constraints;
			command.CommandType = CommandType.Text;

			return command;
		}

		SqliteCommand GetSelectCommand (IX509Selector selector, bool trustedOnly, bool requirePrivateKey, X509CertificateRecordFields fields)
		{
			var query = "SELECT " + string.Join (", ", GetColumnNames (fields)) + " FROM CERTIFICATES";
			var match = selector as X509CertStoreSelector;
			var command = sqlite.CreateCommand ();
			var constraints = " WHERE ";

			if (trustedOnly) {
				command.Parameters.AddWithValue ("@TRUSTED", true);
				constraints += "TRUSTED = @TRUSTED";
			}

			if (match != null) {
				if (match.BasicConstraints != -1) {
					if (command.Parameters.Count > 0)
						constraints += " AND ";

					command.Parameters.AddWithValue ("@BASICCONSTRAINTS", match.BasicConstraints);
					constraints += "BASICCONSTRAINTS = @BASICCONSTRAINTS";
				}

				if (match.KeyUsage != null) {
					var flags = X509CertificateExtensions.GetKeyUsageFlags (match.KeyUsage);

					if (flags != X509KeyUsageFlags.None) {
						if (command.Parameters.Count > 0)
							constraints += " AND ";

						command.Parameters.AddWithValue ("@FLAGS", (int) flags);
						constraints += "KEYUSAGE & @FLAGS";
					}
				}

				if (match.Issuer != null) {
					if (command.Parameters.Count > 0)
						constraints += " AND ";

					command.Parameters.AddWithValue ("@ISSUERNAME", match.Issuer.ToString ());
					constraints += "ISSUERNAME = @ISSUERNAME";
				}

				if (match.SerialNumber != null) {
					if (command.Parameters.Count > 0)
						constraints += " AND ";

					command.Parameters.AddWithValue ("@SERIALNUMBER", match.SerialNumber.ToString ());
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

		static string[] GetColumnNames (X509CrlRecordFields fields)
		{
			if (fields == X509CrlRecordFields.All)
				return new string[] { "*" };

			var columns = new List<string> ();

			if ((fields & X509CrlRecordFields.Id) != 0)
				columns.Add ("ID");
			if ((fields & X509CrlRecordFields.IsDelta) != 0)
				columns.Add ("DELTA");
			if ((fields & X509CrlRecordFields.IssuerName) != 0)
				columns.Add ("ISSUERNAME");
			if ((fields & X509CrlRecordFields.ThisUpdate) != 0)
				columns.Add ("THISUPDATE");
			if ((fields & X509CrlRecordFields.NextUpdate) != 0)
				columns.Add ("NEXTUPDATE");
			if ((fields & X509CrlRecordFields.Crl) != 0)
				columns.Add ("CRL");

			return columns.ToArray ();
		}

		SqliteCommand GetSelectCommand (X509Name issuer, X509CrlRecordFields fields)
		{
			var query = "SELECT " + string.Join (", ", GetColumnNames (fields)) + " FROM CRLS ";
			var command = sqlite.CreateCommand ();

			command.CommandText = query + "WHERE ISSUERNAME = @ISSUERNAME";
			command.Parameters.AddWithValue ("@ISSUERNAME", issuer.ToString ());
			command.CommandType = CommandType.Text;

			return command;
		}

		SqliteCommand GetSelectCommand (X509Crl crl, X509CrlRecordFields fields)
		{
			var query = "SELECT " + string.Join (", ", GetColumnNames (fields)) + " FROM CRLS ";
			var issuerName = crl.IssuerDN.ToString ();
			var command = sqlite.CreateCommand ();

			command.CommandText = query + "WHERE DELTA = @DELTA AND ISSUERNAME = @ISSUERNAME AND THISUPDATE = @THISUPDATE LIMIT 1";
			command.Parameters.AddWithValue ("@DELTA", crl.IsDelta ());
			command.Parameters.AddWithValue ("@ISSUERNAME", issuerName);
			command.Parameters.AddWithValue ("@THISUPDATE", crl.ThisUpdate);
			command.CommandType = CommandType.Text;

			return command;
		}

		SqliteCommand GetSelectAllCrlsCommand ()
		{
			var command = sqlite.CreateCommand ();

			command.CommandText = "SELECT ID, CRL FROM CRLS";
			command.CommandType = CommandType.Text;

			return command;
		}

		SqliteCommand GetDeleteCommand (X509CertificateRecord record)
		{
			var command = sqlite.CreateCommand ();

			command.CommandText = "DELETE FROM CERTIFICATES WHERE ID = @ID";
			command.Parameters.AddWithValue ("@ID", record.Id);
			command.CommandType = CommandType.Text;

			return command;
		}

		SqliteCommand GetDeleteCommand (X509CrlRecord record)
		{
			var command = sqlite.CreateCommand ();

			command.CommandText = "DELETE FROM CRLS WHERE ID = @ID";
			command.Parameters.AddWithValue ("@ID", record.Id);
			command.CommandType = CommandType.Text;

			return command;
		}

		object GetValue (X509CertificateRecord record, string columnName)
		{
			switch (columnName) {
			case "ID": return record.Id;
			case "BASICCONSTRAINTS": return record.BasicConstraints;
			case "TRUSTED": return record.IsTrusted;
			case "KEYUSAGE": return record.KeyUsage;
			case "NOTBEFORE": return record.NotBefore;
			case "NOTAFTER": return record.NotAfter;
			case "ISSUERNAME": return record.IssuerName;
			case "SERIALNUMBER": return record.SerialNumber;
			case "SUBJECTEMAIL": return record.SubjectEmail;
			case "FINGERPRINT": return record.Fingerprint;
			case "ALGORITHMS": return EncodeEncryptionAlgorithms (record.Algorithms);
			case "ALGORITHMSUPDATED": return record.AlgorithmsUpdated;
			case "CERTIFICATE": return record.Certificate.GetEncoded ();
			case "PRIVATEKEY": return EncodePrivateKey (record.PrivateKey);
			default: throw new ArgumentOutOfRangeException ("columnName");
			}
		}

		static object GetValue (X509CrlRecord record, string columnName)
		{
			switch (columnName) {
			case "ID": return record.Id;
			case "DELTA": return record.IsDelta;
			case "ISSUERNAME": return record.IssuerName;
			case "THISUPDATE": return record.ThisUpdate;
			case "NEXTUPDATE": return record.NextUpdate;
			case "CRL": return record.Crl.GetEncoded ();
			default: throw new ArgumentOutOfRangeException ("columnName");
			}
		}

		SqliteCommand GetInsertCommand (X509CertificateRecord record)
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

				command.Parameters.AddWithValue (variable, value);
				statement.Append (columns[i]);
				variables.Append (variable);
			}

			statement.Append (')');
			variables.Append (')');

			command.CommandText = statement + " " + variables;
			command.CommandType = CommandType.Text;

			return command;
		}

		SqliteCommand GetInsertCommand (X509CrlRecord record)
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

				command.Parameters.AddWithValue (variable, value);
				statement.Append (columns[i]);
				variables.Append (variable);
			}

			statement.Append (')');
			variables.Append (')');

			command.CommandText = statement + " " + variables;
			command.CommandType = CommandType.Text;

			return command;
		}

		SqliteCommand GetUpdateCommand (X509CertificateRecord record, X509CertificateRecordFields fields)
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

				command.Parameters.AddWithValue (variable, value);
			}

			statement.Append (" WHERE ID = @ID");
			command.Parameters.AddWithValue ("@ID", record.Id);

			command.CommandText = statement.ToString ();
			command.CommandType = CommandType.Text;

			return command;
		}

		SqliteCommand GetUpdateCommand (X509CrlRecord record)
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

				command.Parameters.AddWithValue (variable, value);
			}

			statement.Append (" WHERE ID = @ID");
			command.Parameters.AddWithValue ("@ID", record.Id);

			command.CommandText = statement.ToString ();
			command.CommandType = CommandType.Text;

			return command;
		}

		/// <summary>
		/// Find the specified certificate.
		/// </summary>
		/// <param name="certificate">The certificate.</param>
		/// <param name="fields">The desired fields.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public X509CertificateRecord Find (X509Certificate certificate, X509CertificateRecordFields fields)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			using (var command = GetSelectCommand (certificate, fields)) {
				var reader = command.ExecuteReader ();

				try {
					if (reader.Read ()) {
						var parser = new X509CertificateParser ();
						var buffer = new byte[4096];

						return LoadCertificateRecord (reader, parser, ref buffer);
					}
				} finally {
					reader.Close ();
				}
			}

			return null;
		}

		/// <summary>
		/// Finds the certificates matching the specified selector.
		/// </summary>
		/// <returns>The matching certificates.</returns>
		/// <param name="selector">The match criteria.</param>
		public IEnumerable<X509Certificate> FindCertificates (IX509Selector selector)
		{
			using (var command = GetSelectCommand (selector, false, false, X509CertificateRecordFields.Certificate)) {
				var reader = command.ExecuteReader ();

				try {
					var parser = new X509CertificateParser ();
					var buffer = new byte[4096];

					while (reader.Read ()) {
						var record = LoadCertificateRecord (reader, parser, ref buffer);
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
		/// Finds the private keys matching the specified selector.
		/// </summary>
		/// <param name="selector">The selector.</param>
		public IEnumerable<AsymmetricKeyParameter> FindPrivateKeys (IX509Selector selector)
		{
			using (var command = GetSelectCommand (selector, false, true, X509CertificateRecordFields.PrivateKeyLookup)) {
				var reader = command.ExecuteReader ();

				try {
					var parser = new X509CertificateParser ();
					var buffer = new byte[4096];

					while (reader.Read ()) {
						var record = LoadCertificateRecord (reader, parser, ref buffer);

						if (selector == null || selector.Match (record.Certificate))
							yield return record.PrivateKey;
					}
				} finally {
					reader.Close ();
				}
			}

			yield break;
		}

		/// <summary>
		/// Finds the certificate records for the specified mailbox.
		/// </summary>
		/// <param name="mailbox">The mailbox.</param>
		/// <param name="now">The date and time.</param>
		/// <param name="requirePrivateKey"><c>true</c> if a private key is required.</param>
		/// <param name="fields">The desired fields.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailbox"/> is <c>null</c>.
		/// </exception>
		public IEnumerable<X509CertificateRecord> Find (MailboxAddress mailbox, DateTime now, bool requirePrivateKey, X509CertificateRecordFields fields)
		{
			if (mailbox == null)
				throw new ArgumentNullException ("mailbox");

			using (var command = GetSelectCommand (mailbox, now, requirePrivateKey, fields)) {
				var reader = command.ExecuteReader ();

				try {
					var parser = new X509CertificateParser ();
					var buffer = new byte[4096];

					while (reader.Read ()) {
						yield return LoadCertificateRecord (reader, parser, ref buffer);
					}
				} finally {
					reader.Close ();
				}
			}

			yield break;
		}

		/// <summary>
		/// Finds the certificate records matching the specified selector.
		/// </summary>
		/// <param name="selector">The selector.</param>
		/// <param name="trustedOnly"><c>true</c> if only trusted certificates should be returned.</param>
		/// <param name="fields">The desired fields.</param>
		public IEnumerable<X509CertificateRecord> Find (IX509Selector selector, bool trustedOnly, X509CertificateRecordFields fields)
		{
			using (var command = GetSelectCommand (selector, trustedOnly, false, fields | X509CertificateRecordFields.Certificate)) {
				var reader = command.ExecuteReader ();

				try {
					var parser = new X509CertificateParser ();
					var buffer = new byte[4096];

					while (reader.Read ()) {
						var record = LoadCertificateRecord (reader, parser, ref buffer);

						if (selector == null || selector.Match (record.Certificate))
							yield return record;
					}
				} finally {
					reader.Close ();
				}
			}

			yield break;
		}

		/// <summary>
		/// Add the specified certificate record.
		/// </summary>
		/// <param name="record">The record.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="record"/> is <c>null</c>.
		/// </exception>
		public void Add (X509CertificateRecord record)
		{
			if (record == null)
				throw new ArgumentNullException ("record");

			using (var command = GetInsertCommand (record)) {
				command.ExecuteNonQuery ();
			}
		}

		/// <summary>
		/// Remove the specified certificate record.
		/// </summary>
		/// <param name="record">The record.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="record"/> is <c>null</c>.
		/// </exception>
		public void Remove (X509CertificateRecord record)
		{
			if (record == null)
				throw new ArgumentNullException ("record");

			using (var command = GetDeleteCommand (record)) {
				command.ExecuteNonQuery ();
			}
		}

		/// <summary>
		/// Update the specified certificate record.
		/// </summary>
		/// <param name="record">The record.</param>
		/// <param name="fields">The fields to update.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="record"/> is <c>null</c>.
		/// </exception>
		public void Update (X509CertificateRecord record, X509CertificateRecordFields fields)
		{
			if (record == null)
				throw new ArgumentNullException ("record");

			using (var command = GetUpdateCommand (record, fields)) {
				command.ExecuteNonQuery ();
			}
		}

		/// <summary>
		/// Finds the CRL records for the specified issuer.
		/// </summary>
		/// <param name="issuer">The issuer.</param>
		/// <param name="fields">The desired fields.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="issuer"/> is <c>null</c>.
		/// </exception>
		public IEnumerable<X509CrlRecord> Find (X509Name issuer, X509CrlRecordFields fields)
		{
			if (issuer == null)
				throw new ArgumentNullException ("issuer");

			using (var command = GetSelectCommand (issuer, fields)) {
				var reader = command.ExecuteReader ();

				try {
					var parser = new X509CrlParser ();
					var buffer = new byte[4096];

					while (reader.Read ()) {
						yield return LoadCrlRecord (reader, parser, ref buffer);
					}
				} finally {
					reader.Close ();
				}
			}

			yield break;
		}

		/// <summary>
		/// Finds the specified certificate revocation list.
		/// </summary>
		/// <param name="crl">The certificate revocation list.</param>
		/// <param name="fields">The desired fields.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="crl"/> is <c>null</c>.
		/// </exception>
		public X509CrlRecord Find (X509Crl crl, X509CrlRecordFields fields)
		{
			if (crl == null)
				throw new ArgumentNullException ("crl");

			using (var command = GetSelectCommand (crl, fields)) {
				var reader = command.ExecuteReader ();

				try {
					if (reader.Read ()) {
						var parser = new X509CrlParser ();
						var buffer = new byte[4096];

						return LoadCrlRecord (reader, parser, ref buffer);
					}
				} finally {
					reader.Close ();
				}
			}

			return null;
		}

		/// <summary>
		/// Add the specified CRL record.
		/// </summary>
		/// <param name="record">The record.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="record"/> is <c>null</c>.
		/// </exception>
		public void Add (X509CrlRecord record)
		{
			if (record == null)
				throw new ArgumentNullException ("record");

			using (var command = GetInsertCommand (record)) {
				command.ExecuteNonQuery ();
			}
		}

		/// <summary>
		/// Remove the specified CRL record.
		/// </summary>
		/// <param name="record">The record.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="record"/> is <c>null</c>.
		/// </exception>
		public void Remove (X509CrlRecord record)
		{
			if (record == null)
				throw new ArgumentNullException ("record");

			using (var command = GetDeleteCommand (record)) {
				command.ExecuteNonQuery ();
			}
		}

		/// <summary>
		/// Update the specified CRL record.
		/// </summary>
		/// <param name="record">The record.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="record"/> is <c>null</c>.
		/// </exception>
		public void Update (X509CrlRecord record)
		{
			if (record == null)
				throw new ArgumentNullException ("record");

			using (var command = GetUpdateCommand (record)) {
				command.ExecuteNonQuery ();
			}
		}

		/// <summary>
		/// Gets a certificate revocation list store.
		/// </summary>
		/// <returns>A certificate recovation list store.</returns>
		public IX509Store GetCrlStore ()
		{
			var crls = new List<X509Crl> ();

			// TODO: we could cache this...
			using (var command = GetSelectAllCrlsCommand ()) {
				var reader = command.ExecuteReader ();

				try {
					var parser = new X509CrlParser ();
					var buffer = new byte[4096];

					while (reader.Read ()) {
						var record = LoadCrlRecord (reader, parser, ref buffer);
						crls.Add (record.Crl);
					}
				} finally {
					reader.Close ();
				}
			}

			return X509StoreFactory.Create ("Crl/Collection", new X509CollectionStoreParameters (crls));
		}

		#region IX509Store implementation

		/// <summary>
		/// Gets a collection of matching <see cref="Org.BouncyCastle.X509.X509Certificate"/>s
		/// based on the specified selector.
		/// </summary>
		/// <returns>The matching certificates.</returns>
		/// <param name="selector">The match criteria.</param>
		ICollection IX509Store.GetMatches (IX509Selector selector)
		{
			return new List<X509Certificate> (FindCertificates (selector));
		}

		#endregion

		#region IDisposable implementation

		/// <summary>
		/// Releases all resource used by the <see cref="MimeKit.Cryptography.X509CertificateDatabase"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the
		/// <see cref="MimeKit.Cryptography.X509CertificateDatabase"/>. The <see cref="Dispose"/> method leaves the
		/// <see cref="MimeKit.Cryptography.X509CertificateDatabase"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the
		/// <see cref="MimeKit.Cryptography.X509CertificateDatabase"/> so the garbage collector can reclaim the memory that
		/// the <see cref="MimeKit.Cryptography.X509CertificateDatabase"/> was occupying.</remarks>
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

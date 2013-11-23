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

using Org.BouncyCastle.Math;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509.Store;
using Org.BouncyCastle.Crypto.Parameters;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An X.509 certificate database.
	/// </summary>
	class X509CertificateDatabase : IDisposable, IX509Store
	{
		SqliteConnection sqlite;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.X509CertificateDatabase"/> class.
		/// </summary>
		/// <param name="fileName">The file name.</param>
		/// <param name="password">The password.</param>
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

		static X509Certificate DecodeX509Certificate (SqliteDataReader reader, X509CertificateParser parser, int column, ref byte[] buffer)
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

		static BigInteger DecodeBigInteger (byte[] buffer, ref int index)
		{
			int length = DecodeInt16 (buffer, ref index);
			int offset = index;

			index += length;

			return new BigInteger (buffer, offset, length);
		}

		static void EncodeBigInteger (BigInteger value, Stream stream)
		{
			var bytes = value.ToByteArray ();
			EncodeInt16 (bytes.Length, stream);
			stream.Write (bytes, 0, bytes.Length);
		}

		static int DecodeInt16 (byte[] buffer, ref int index)
		{
			return ((int) buffer[index++] << 8) | (int) buffer[index++];
		}

		static void EncodeInt16 (int value, Stream stream)
		{
			stream.WriteByte ((byte) ((value >> 8) & 0xff));
			stream.WriteByte ((byte) (value & 0xff));
		}

		static int DecodeInt32 (byte[] buffer, ref int index)
		{
			int value = ((int) buffer[index++]) << 24;
			value |= ((int) buffer[index++]) << 16;
			value |= ((int) buffer[index++]) << 8;
			value |= (int) buffer[index++];
			return value;
		}

		static void EncodeInt32 (int value, Stream stream)
		{
			stream.WriteByte ((byte) ((value >> 24) & 0xff));
			stream.WriteByte ((byte) ((value >> 16) & 0xff));
			stream.WriteByte ((byte) ((value >> 8) & 0xff));
			stream.WriteByte ((byte) (value & 0xff));
		}

		enum PrivateKeyType : byte {
			DiffieHellman,
			Dsa,
			Rsa
		}

		static DHPrivateKeyParameters DecodePrivateDiffieHellmanKey (byte[] buffer, int length)
		{
			int index = 1;

			var x = DecodeBigInteger (buffer, ref index);
			var p = DecodeBigInteger (buffer, ref index);
			var g = DecodeBigInteger (buffer, ref index);
			var q = DecodeBigInteger (buffer, ref index);
			var m = DecodeInt32 (buffer, ref index);
			var l = DecodeInt32 (buffer, ref index);
			var j = DecodeBigInteger (buffer, ref index);
			var c = DecodeInt32 (buffer, ref index);
			var s = new byte[length - index];

			for (int i = 0; i < s.Length; i++)
				s[i] = buffer[index++];

			var validation = new DHValidationParameters (s, c);
			var param = new DHParameters (p, g, q, m, l, j, validation);

			return new DHPrivateKeyParameters (x, param);
		}

		static void EncodePrivateDiffieHellmanKey (DHPrivateKeyParameters key, Stream stream)
		{
			stream.WriteByte ((byte) PrivateKeyType.DiffieHellman);
			EncodeBigInteger (key.X, stream);
			EncodeBigInteger (key.Parameters.P, stream);
			EncodeBigInteger (key.Parameters.G, stream);
			EncodeBigInteger (key.Parameters.Q, stream);
			EncodeInt32 (key.Parameters.M, stream);
			EncodeInt32 (key.Parameters.L, stream);
			EncodeBigInteger (key.Parameters.J, stream);
			EncodeInt32 (key.Parameters.ValidationParameters.Counter, stream);
			var seed = key.Parameters.ValidationParameters.GetSeed ();
			stream.Write (seed, 0, seed.Length);
		}

		static DsaPrivateKeyParameters DecodePrivateDsaKey (byte[] buffer, int length)
		{
			int index = 1;

			var x = DecodeBigInteger (buffer, ref index);
			var p = DecodeBigInteger (buffer, ref index);
			var q = DecodeBigInteger (buffer, ref index);
			var g = DecodeBigInteger (buffer, ref index);

			var param = new DsaParameters (p, q, g);

			return new DsaPrivateKeyParameters (x, param);
		}

		static void EncodePrivateDsaKey (DsaPrivateKeyParameters key, Stream stream)
		{
			stream.WriteByte ((byte) PrivateKeyType.Dsa);
			EncodeBigInteger (key.X, stream);
			EncodeBigInteger (key.Parameters.P, stream);
			EncodeBigInteger (key.Parameters.Q, stream);
			EncodeBigInteger (key.Parameters.G, stream);
		}

		static RsaKeyParameters DecodePrivateRsaKey (byte[] buffer, int length)
		{
			int index = 1;

			var m = DecodeBigInteger (buffer, ref index);
			var e = DecodeBigInteger (buffer, ref index);

			return new RsaKeyParameters (true, m, e);
		}

		static void EncodePrivateRsaKey (RsaKeyParameters key, Stream stream)
		{
			stream.WriteByte ((byte) PrivateKeyType.Rsa);
			EncodeBigInteger (key.Modulus, stream);
			EncodeBigInteger (key.Exponent, stream);
		}

		static byte[] EncodePrivateKey (AsymmetricKeyParameter key)
		{
			if (key == null)
				return null;

			using (var memory = new MemoryStream ()) {
				if (key is DHPrivateKeyParameters)
					EncodePrivateDiffieHellmanKey ((DHPrivateKeyParameters) key, memory);
				else if (key is DsaPrivateKeyParameters)
					EncodePrivateDsaKey ((DsaPrivateKeyParameters) key, memory);
				else if (key is RsaKeyParameters)
					EncodePrivateRsaKey ((RsaKeyParameters) key, memory);

				return memory.ToArray ();
			}
		}

		static AsymmetricKeyParameter DecodeAsymmetricKeyParameter (SqliteDataReader reader, int column, ref byte[] buffer)
		{
			if (reader.IsDBNull (column))
				return null;

			int nread = ReadBinaryBlob (reader, column, ref buffer);

			switch ((PrivateKeyType) buffer[0]) {
			case PrivateKeyType.DiffieHellman:
				return DecodePrivateDiffieHellmanKey (buffer, nread);
			case PrivateKeyType.Dsa:
				return DecodePrivateDsaKey (buffer, nread);
			case PrivateKeyType.Rsa:
				return DecodePrivateRsaKey (buffer, nread);
			default:
				throw new ArgumentOutOfRangeException ();
			}
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

		static X509CertificateRecord LoadCertificateRecord (SqliteDataReader reader, X509CertificateParser parser, ref byte[] buffer)
		{
			var record = new X509CertificateRecord ();

			for (int i = 0; i < reader.FieldCount; i++) {
				switch (reader.GetName (i)) {
				case "CERTIFICATE":
					record.Certificate = DecodeX509Certificate (reader, parser, i, ref buffer);
					break;
				case "PRIVATEKEY":
					record.PrivateKey = DecodeAsymmetricKeyParameter (reader, i, ref buffer);
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

		static X509CrlRecord LoadCrlRecord (SqliteDataReader reader, X509CrlParser parser, ref byte[] buffer)
		{
			var record = new X509CrlRecord ();

			for (int i = 0; i < reader.FieldCount; i++) {
				switch (reader.GetName (i)) {
				case "CRL":
					record.Crl = DecodeX509Crl (reader, parser, i, ref buffer);
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
				case "SUBJECTEMAIL": statement.Append ("TEXT"); break;
				case "FINGERPRINT": statement.Append ("TEXT NOT NULL"); break;
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

		SqliteCommand GetSelectCommand (MailboxAddress mailbox, DateTime now, X509CertificateRecordFields fields)
		{
			var query = "SELECT " + string.Join (", ", GetColumnNames (fields)) + " FROM CERTIFICATES ";
			var command = sqlite.CreateCommand ();

			command.CommandText = query + "WHERE BASICCONSTRAINTS = @BASICCONSTRAINTS AND SUBJECTEMAIL = @SUBJECTEMAIL AND NOTBEFORE < @NOW AND NOTAFTER > @NOW";
			command.Parameters.AddWithValue ("@BASICCONSTRAINTS", -1);
			command.Parameters.AddWithValue ("@SUBJECTEMAIL", mailbox.Address);
			command.Parameters.AddWithValue ("@NOW", now);
			command.CommandType = CommandType.Text;

			return command;
		}

		SqliteCommand GetSelectCommand (IX509Selector selector, X509CertificateRecordFields fields)
		{
			var query = "SELECT " + string.Join (", ", GetColumnNames (fields)) + " FROM CERTIFICATES";
			var match = selector as X509CertStoreSelector;
			var command = sqlite.CreateCommand ();
			var constraints = string.Empty;

			if (match != null) {
				constraints = " WHERE ";

				if (match.BasicConstraints != -1) {
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

				if (command.Parameters.Count == 0)
					constraints = string.Empty;
			}

			command.CommandText = query + constraints;
			command.CommandType = CommandType.Text;

			return command;
		}

		SqliteCommand GetSelectCommand (X509Crl crl)
		{
			var query = "SELECT ID, CRL FROM CRLS ";
			var issuerName = crl.IssuerDN.ToString ();
			var command = sqlite.CreateCommand ();

			// FIXME: can we depend on the issuer name as being unique?
			command.CommandText = query + "WHERE ISSUERNAME = @ISSUERNAME LIMIT 1";
			command.Parameters.AddWithValue ("@ISSUERNAME", issuerName);
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

		static object GetValue (X509CertificateRecord record, string columnName)
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
			default: throw new ArgumentOutOfRangeException ();
			}
		}

		static object GetValue (X509CrlRecord record, string columnName)
		{
			switch (columnName) {
			case "ID": return record.Id;
			case "ISSUERNAME": return record.IssuerName;
			case "THISUPDATE": return record.ThisUpdate;
			case "NEXTUPDATE": return record.NextUpdate;
			case "CRL": return record.Crl.GetEncoded ();
			default: throw new ArgumentOutOfRangeException ();
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
		/// Find the records for the specified mailbox that is valid for the given date and time.
		/// </summary>
		/// <param name="mailbox">The mailbox.</param>
		/// <param name="now">The date and time.</param>
		/// <param name="fields">The desired fields.</param>
		public IEnumerable<X509CertificateRecord> Find (MailboxAddress mailbox, DateTime now, X509CertificateRecordFields fields)
		{
			if (mailbox == null)
				throw new ArgumentNullException ("mailbox");

			using (var command = GetSelectCommand (mailbox, now, fields)) {
				var reader = command.ExecuteReader ();

				try {
					var parser = new X509CertificateParser ();
					var buffer = new byte[4096];

					while (reader.Read ())
						yield return LoadCertificateRecord (reader, parser, ref buffer);
				} finally {
					reader.Close ();
				}
			}

			yield break;
		}

		/// <summary>
		/// Find the records match the specified selector.
		/// </summary>
		/// <param name="selector">The selector.</param>
		/// <param name="fields">The desired fields.</param>
		public IEnumerable<X509CertificateRecord> Find (IX509Selector selector, X509CertificateRecordFields fields)
		{
			using (var command = GetSelectCommand (selector, fields | X509CertificateRecordFields.Certificate)) {
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
		/// Add the specified record.
		/// </summary>
		/// <param name="record">The record.</param>
		public void Add (X509CertificateRecord record)
		{
			if (record == null)
				throw new ArgumentNullException ("record");

			using (var command = GetInsertCommand (record)) {
				command.ExecuteNonQuery ();
			}
		}

		/// <summary>
		/// Remove the specified record.
		/// </summary>
		/// <param name="record">The record.</param>
		public void Remove (X509CertificateRecord record)
		{
			if (record == null)
				throw new ArgumentNullException ("record");

			using (var command = GetDeleteCommand (record)) {
				command.ExecuteNonQuery ();
			}
		}

		/// <summary>
		/// Update the specified record.
		/// </summary>
		/// <param name="record">The record.</param>
		/// <param name="fields">The fields to update.</param>
		public void Update (X509CertificateRecord record, X509CertificateRecordFields fields)
		{
			if (record == null)
				throw new ArgumentNullException ("record");

			using (var command = GetUpdateCommand (record, fields)) {
				command.ExecuteNonQuery ();
			}
		}

		/// <summary>
		/// Find the specified certificate revocation list.
		/// </summary>
		/// <param name="crl">The certificate revocation list.</param>
		public X509CrlRecord Find (X509Crl crl)
		{
			if (crl == null)
				throw new ArgumentNullException ("crl");

			using (var command = GetSelectCommand (crl)) {
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
		/// Add the specified record.
		/// </summary>
		/// <param name="record">The record.</param>
		public void Add (X509CrlRecord record)
		{
			if (record == null)
				throw new ArgumentNullException ("record");

			using (var command = GetInsertCommand (record)) {
				command.ExecuteNonQuery ();
			}
		}

		/// <summary>
		/// Remove the specified record.
		/// </summary>
		/// <param name="record">The record.</param>
		public void Remove (X509CrlRecord record)
		{
			if (record == null)
				throw new ArgumentNullException ("record");

			using (var command = GetDeleteCommand (record)) {
				command.ExecuteNonQuery ();
			}
		}

		/// <summary>
		/// Update the specified record.
		/// </summary>
		/// <param name="record">The record.</param>
		public void Update (X509CrlRecord record)
		{
			if (record == null)
				throw new ArgumentNullException ("record");

			using (var command = GetUpdateCommand (record)) {
				command.ExecuteNonQuery ();
			}
		}

		/// <summary>
		/// Gets an enumerator of matching <see cref="Org.BouncyCastle.X509.X509Certificate"/>s
		/// based on the specified selector.
		/// </summary>
		/// <returns>The matching certificates.</returns>
		/// <param name="selector">The match criteria.</param>
		public IEnumerable<X509Certificate> GetCertificates (IX509Selector selector)
		{
			using (var command = GetSelectCommand (selector, X509CertificateRecordFields.Certificate)) {
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
		/// Gets a certificate revocation list store.
		/// </summary>
		/// <returns>The certificate recovation list store.</returns>
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
			return new List<X509Certificate> (GetCertificates (selector));
		}

		#endregion

		#region IDisposable implementation

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

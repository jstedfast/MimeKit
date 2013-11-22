//
// X509CertificateRecord.cs
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
using System.Collections.Generic;

using Mono.Data.Sqlite;

using Org.BouncyCastle.Math;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Parameters;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An X.509 Certificate record.
	/// </summary>
	class X509CertificateRecord
	{
		static readonly string[] ColumnNames = {
			"ID",
			"TRUSTED",
			"KEYUSAGE",
			"NOTBEFORE",
			"NOTAFTER",
			"ISSUERNAME",
			"ISSUERUID",
			"SERIALNUMBER",
			"SUBJECTEMAIL",
			"FINGERPRINT",
			"ALGORITHMS",
			"ALGORITHMSUPDATED",
			"CERTIFICATE",
			"PRIVATEKEY"
		};

		/// <summary>
		/// Gets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		public int Id { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether the certificate is trusted.
		/// </summary>
		/// <value><c>true</c> if the certificate is trusted; otherwise, <c>false</c>.</value>
		public bool IsTrusted { get; set; }

		/// <summary>
		/// Gets or sets the X.509 key usage.
		/// </summary>
		/// <value>The X.509 key usage.</value>
		public int KeyUsage { get; set; }

		/// <summary>
		/// Gets the starting date and time where the certificate is valid.
		/// </summary>
		/// <value>The date and time.</value>
		public DateTime NotBefore { get { return Certificate.NotBefore; } }

		/// <summary>
		/// Gets the end date and time where the certificate is valid.
		/// </summary>
		/// <value>The date and time.</value>
		public DateTime NotAfter { get { return Certificate.NotAfter; } }

		/// <summary>
		/// Gets the certificate issuer's name.
		/// </summary>
		/// <value>The issuer's name.</value>
		public string IssuerName { get { return Certificate.GetIssuerNameInfo (X509Name.CN); } }

		/// <summary>
		/// Gets the certificate issuer's unique identifier.
		/// </summary>
		/// <value>The issuer's unique identifier.</value>
		public string IssuerUid { get { return Certificate.IssuerUniqueID.GetString (); } }

		/// <summary>
		/// Gets the serial number of the certificate.
		/// </summary>
		/// <value>The serial number.</value>
		public string SerialNumber { get { return Certificate.SerialNumber.ToString (); } }

		/// <summary>
		/// Gets the subject email address.
		/// </summary>
		/// <value>The subject email address.</value>
		public string SubjectEmail { get { return Certificate.GetSubjectEmailAddress (); } }

		/// <summary>
		/// Gets the fingerprint of the certificate.
		/// </summary>
		/// <value>The fingerprint.</value>
		public string Fingerprint { get { return Certificate.GetFingerprint (); } }

		/// <summary>
		/// Gets or sets the encryption algorithm capabilities.
		/// </summary>
		/// <value>The encryption algorithms.</value>
		public EncryptionAlgorithm[] Algorithms { get; set; }

		/// <summary>
		/// Gets or sets the date when the algorithms were last updated.
		/// </summary>
		/// <value>The date the algorithms were updated.</value>
		public DateTime AlgorithmsUpdated { get; set; }

		/// <summary>
		/// Gets the certificate.
		/// </summary>
		/// <value>The certificate.</value>
		public X509Certificate Certificate { get; private set; }

		/// <summary>
		/// Gets the private key.
		/// </summary>
		/// <value>The private key.</value>
		public AsymmetricKeyParameter PrivateKey { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.X509CertificateRecord"/> class.
		/// </summary>
		/// <param name="certificate">The certificate.</param>
		/// <param name="key">The private key.</param>
		public X509CertificateRecord (X509Certificate certificate, AsymmetricKeyParameter key)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			if (key == null)
				throw new ArgumentNullException ("key");

			if (!key.IsPrivate)
				throw new ArgumentException ("The key must be private.", "key");

			KeyUsage = certificate.GetKeyUsageFlags ();
			AlgorithmsUpdated = DateTime.MinValue;
			Certificate = certificate;
			PrivateKey = key;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.X509CertificateRecord"/> class.
		/// </summary>
		/// <param name="certificate">The certificate.</param>
		public X509CertificateRecord (X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			KeyUsage = certificate.GetKeyUsageFlags ();
			AlgorithmsUpdated = DateTime.MinValue;
			Certificate = certificate;
		}

		private X509CertificateRecord ()
		{
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

		static BigInteger DecodeBigInteger (byte[] buffer, ref int index)
		{
			int length = buffer[index++];
			int offset = index;

			index += length;

			return new BigInteger (buffer, offset, length);
		}

		static void EncodeBigInteger (BigInteger value, Stream stream)
		{
			var bytes = value.ToByteArray ();
			stream.WriteByte ((byte) bytes.Length);
			stream.Write (bytes, 0, bytes.Length);
		}

		static int DecodeInt32 (byte[] buffer, ref int index)
		{
			int value = 0;

			for (int i = 0; i < 4; i++)
				value <<= buffer[index++];

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

		internal static X509CertificateRecord Load (SqliteDataReader reader, X509CertificateParser parser, ref byte[] buffer)
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
				case "KEYUSAGE":
					record.KeyUsage = reader.GetInt32 (i);
					break;
				case "ID":
					record.Id = reader.GetInt32 (i);
					break;
				}
			}

			return record;
		}

		internal static SqliteCommand GetCreateTableCommand (SqliteConnection sqlite)
		{
			var statement = new StringBuilder ("CREATE TABLE CERTIFICATES(");
			var command = sqlite.CreateCommand ();

			for (int i = 0; i < ColumnNames.Length; i++) {
				if (i > 0)
					statement.Append (", ");

				statement.Append (ColumnNames[i] + " ");
				switch (ColumnNames[i]) {
				case "ID": statement.Append ("INTEGER PRIMARY KEY AUTOINCREMENT"); break;
				case "TRUSTED":  statement.Append ("INTEGER NOT NULL"); break;
				case "KEYUSAGE": statement.Append ("INTEGER NOT NULL"); break;
				case "NOTBEFORE": statement.Append ("INTEGER NOT NULL"); break;
				case "NOTAFTER": statement.Append ("INTEGER NOT NULL"); break;
				case "ISSUERNAME": statement.Append ("TEXT NOT NULL"); break;
				case "ISSUERUID": statement.Append ("TEXT NOT NULL"); break;
				case "SERIALNUMBER": statement.Append ("TEXT NOT NULL"); break;
				case "SUBJECTEMAIL": statement.Append ("TEXT NOT NULL"); break;
				case "FINGERPRINT": statement.Append ("TEXT NOT NULL"); break;
				case "ALGORITHMS": statement.Append ("TEXT"); break;
				case "ALGORITHMSUPDATED": statement.Append ("INTEGER NOT NULL"); break;
				case "CERTIFICATE": statement.Append ("BLOB UNIQUE"); break;
				case "PRIVATEKEY": statement.Append ("BLOB"); break;
				}
			}

			return command;
		}

		internal static SqliteCommand GetFindCertificateCommand (SqliteConnection sqlite, X509Certificate certificate)
		{
			var issuerName = certificate.GetIssuerNameInfo (X509Name.CN);
			var serialNumber = certificate.SerialNumber.ToString ();
			var fingerprint = certificate.GetFingerprint ();
			var command = sqlite.CreateCommand ();

			command.CommandText = "SELECT * FROM CERTIFICATES WHERE ISSUERNAME = @ISSUERNAME AND SERIALNUMBER = @SERIALNUMBER AND FINGERPRINT = @FINGERPRINT LIMIT 1";
			command.Parameters.AddWithValue ("@ISSUERNAME", issuerName);
			command.Parameters.AddWithValue ("@SERIALNUMBER", serialNumber);
			command.Parameters.AddWithValue ("@FINGERPRINT", fingerprint);
			command.CommandType = CommandType.Text;

			return command;
		}

		internal static SqliteCommand GetFindCertificatesCommand (SqliteConnection sqlite, MailboxAddress mailbox, DateTime now)
		{
			var command = sqlite.CreateCommand ();

			command.CommandText = "SELECT * FROM CERTIFICATES WHERE SUBJECTEMAIL = @SUBJECTEMAIL AND NOTBEFORE < @NOW AND NOTAFTER > @NOW";
			command.Parameters.AddWithValue ("@SUBJECTEMAIL", mailbox.Address);
			command.Parameters.AddWithValue ("@NOW", now);
			command.CommandType = CommandType.Text;

			return command;
		}

		internal static SqliteCommand GetFindCertificatesCommand (SqliteConnection sqlite)
		{
			var command = sqlite.CreateCommand ();

			command.CommandText = "SELECT * FROM CERTIFICATES";
			command.CommandType = CommandType.Text;

			return command;
		}

		internal SqliteCommand GetDeleteCommand (SqliteConnection sqlite)
		{
			var command = sqlite.CreateCommand ();

			command.CommandText = "DELETE FROM CERTIFICATES WHERE ID = @ID";
			command.Parameters.AddWithValue ("@ID", Id);
			command.CommandType = CommandType.Text;

			return command;
		}

		internal SqliteCommand GetInsertCommand (SqliteConnection sqlite)
		{
			var statement = new StringBuilder ("INSERT INTO CERTIFICATES(");
			var variables = new StringBuilder ("VALUES(");
			var command = sqlite.CreateCommand ();

			for (int i = 1; i < ColumnNames.Length; i++) {
				if (i > 1) {
					statement.Append (", ");
					variables.Append (", ");
				}

				var variable = "@" + ColumnNames[i];
				object value = null;

				switch (ColumnNames[i]) {
				case "TRUSTED":  value = IsTrusted; break;
				case "KEYUSAGE": value = KeyUsage; break;
				case "NOTBEFORE": value = NotBefore; break;
				case "NOTAFTER": value = NotAfter; break;
				case "ISSUERNAME": value = IssuerName; break;
				case "ISSUERUID": value = IssuerUid; break;
				case "SERIALNUMBER": value = SerialNumber; break;
				case "SUBJECTEMAIL": value = SubjectEmail; break;
				case "FINGERPRINT": value = Fingerprint; break;
				case "ALGORITHMS": value = EncodeEncryptionAlgorithms (Algorithms); break;
				case "ALGORITHMSUPDATED": value = AlgorithmsUpdated; break;
				case "CERTIFICATE": value = Certificate.GetEncoded (); break;
				case "PRIVATEKEY": value = EncodePrivateKey (PrivateKey); break;
				}

				command.Parameters.AddWithValue (variable, value);
				statement.Append (ColumnNames[i]);
				variables.Append (variable);
			}

			statement.Append (')');
			variables.Append (')');

			command.CommandText = statement + " " + variables;
			command.CommandType = CommandType.Text;

			return command;
		}

		internal SqliteCommand GetUpdateCommand (SqliteConnection sqlite)
		{
			var statement = new StringBuilder ("REPLACE INTO CERTIFICATES(");
			var variables = new StringBuilder ("VALUES(");
			var command = sqlite.CreateCommand ();

			for (int i = 0; i < ColumnNames.Length; i++) {
				var variable = "@" + ColumnNames[i];
				object value = null;

				if (i > 0) {
					statement.Append (", ");
					variables.Append (", ");
				}

				switch (ColumnNames[i]) {
				case "ID": value = Id; break;
				case "TRUSTED":  value = IsTrusted; break;
				case "KEYUSAGE": value = KeyUsage; break;
				case "NOTBEFORE": value = NotBefore; break;
				case "NOTAFTER": value = NotAfter; break;
				case "ISSUERNAME": value = IssuerName; break;
				case "ISSUERUID": value = IssuerUid; break;
				case "SERIALNUMBER": value = SerialNumber; break;
				case "SUBJECTEMAIL": value = SubjectEmail; break;
				case "FINGERPRINT": value = Fingerprint; break;
				case "ALGORITHMS": value = EncodeEncryptionAlgorithms (Algorithms); break;
				case "ALGORITHMSUPDATED": value = AlgorithmsUpdated; break;
				case "CERTIFICATE": value = Certificate.GetEncoded (); break;
				case "PRIVATEKEY": value = EncodePrivateKey (PrivateKey); break;
				}

				command.Parameters.AddWithValue (variable, value);
				statement.Append (ColumnNames[i]);
				variables.Append (variable);
			}

			statement.Append (')');
			variables.Append (')');

			command.CommandText = statement + " " + variables;
			command.CommandType = CommandType.Text;

			return command;
		}
	}
}

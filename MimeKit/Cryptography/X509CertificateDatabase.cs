//
// X509CertificateDatabase.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
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
using System.IO;
using System.Data.Common;
using System.Collections.Generic;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.BC;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;

using MimeKit.Utils;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An X.509 certificate database.
	/// </summary>
	/// <remarks>
	/// An X.509 certificate database is used for storing certificates, metadata related to the certificates
	/// (such as encryption algorithms supported by the associated client), certificate revocation lists (CRLs),
	/// and private keys.
	/// </remarks>
	public abstract class X509CertificateDatabase : IX509CertificateDatabase
	{
		const X509CertificateRecordFields PrivateKeyFields = X509CertificateRecordFields.Certificate | X509CertificateRecordFields.PrivateKey;
		static readonly DerObjectIdentifier DefaultEncryptionAlgorithm = BCObjectIdentifiers.bc_pbe_sha256_pkcs12_aes256_cbc;
		const int DefaultMinIterations = 1024;
		const int DefaultSaltSize = 20;

		DbConnection connection;
		char[] password;

		/// <summary>
		/// The name of the database table containing the certificates.
		/// </summary>
		/// <remarks>
		/// The name of the database table containing the certificates.
		/// </remarks>
		protected const string CertificatesTableName = "CERTIFICATES";

		/// <summary>
		/// The name of the database table containing the CRLs.
		/// </summary>
		/// <remarks>
		/// The name of the database table containing the CRLs.
		/// </remarks>
		protected const string CrlsTableName = "CRLS";

		/// <summary>
		/// The column names for the certificates table.
		/// </summary>
		/// <remarks>
		/// The column names for the certificates table.
		/// </remarks>
		protected class CertificateColumnNames
		{
			/// <summary>
			/// The auto-increment primary key identifier.
			/// </summary>
			/// <remarks>
			/// The auto-increment primary key identifier.
			/// </remarks>
			public const string Id = "ID";

			/// <summary>
			/// A column specifying whether the certificate is trusted or not.
			/// </summary>
			/// <remarks>
			/// <para>A column specifying whether the certificate is trusted or not.</para>
			/// <para>This data-type for this column should be <see langword="bool"/>.</para>
			/// </remarks>
			public const string Trusted = "TRUSTED";

			/// <summary>
			/// A column specifying whether the certificate is an anchor.
			/// </summary>
			/// <remarks>
			/// <para>A column specifying whether the certificate is an anchor.</para>
			/// <para>This data-type for this column should be <see langword="bool"/>.</para>
			/// </remarks>
			public const string Anchor = "ANCHOR";

			/// <summary>
			/// A column specifying the basic constraints of the certificate.
			/// </summary>
			/// <remarks>
			/// <para>A column specifying the basic constraints of the certificate.</para>
			/// <para>This data-type for this column should be <see langword="int"/>.</para>
			/// </remarks>
			public const string BasicConstraints = "BASICCONSTRAINTS";

			/// <summary>
			/// A column specifying the key usage of the certificate.
			/// </summary>
			/// <remarks>
			/// <para>A column specifying the key usage of the certificate.</para>
			/// <para>This data-type for this column should be <see langword="int"/>.</para>
			/// </remarks>
			public const string KeyUsage = "KEYUSAGE";

			/// <summary>
			/// A column specifying the date and time when the certificate first becomes valid.
			/// </summary>
			/// <remarks>
			/// <para>A column specifying the date and time when the certificate first becomes valid.</para>
			/// <para>This data-type for this column should be <see langword="long"/>.</para>
			/// </remarks>
			public const string NotBefore = "NOTBEFORE";

			/// <summary>
			/// A column specifying the date and time after which the certificate becomes invalid.
			/// </summary>
			/// <remarks>
			/// <para>A column specifying the date and time after which the certificate becomes invalid.</para>
			/// <para>This data-type for this column should be <see langword="long"/>.</para>
			/// </remarks>
			public const string NotAfter = "NOTAFTER";

			/// <summary>
			/// A column specifying the issuer name of the certificate.
			/// </summary>
			/// <remarks>
			/// <para>A column specifying the issuer name of the certificate.</para>
			/// <para>This data-type for this column should be <see langword="string"/>.</para>
			/// </remarks>
			public const string IssuerName = "ISSUERNAME";

			/// <summary>
			/// A column specifying the serial number of the certificate.
			/// </summary>
			/// <remarks>
			/// <para>A column specifying the serial number of the certificate.</para>
			/// <para>This data-type for this column should be <see langword="string"/>.</para>
			/// </remarks>
			public const string SerialNumber = "SERIALNUMBER";

			/// <summary>
			/// A column specifying the subject name of the certificate.
			/// </summary>
			/// <remarks>
			/// <para>A column specifying the subject name of the certificate.</para>
			/// <para>This data-type for this column should be <see langword="string"/>.</para>
			/// </remarks>
			public const string SubjectName = "SUBJECTNAME";

			/// <summary>
			/// A column specifying the subject key identifier of the certificate.
			/// </summary>
			/// <remarks>
			/// <para>A column specifying the subject key identifier of the certificate.</para>
			/// <para>This data-type for this column should be <see langword="string"/>.</para>
			/// </remarks>
			public const string SubjectKeyIdentifier = "SUBJECTKEYIDENTIFIER";

			/// <summary>
			/// A column specifying the subject email address of the certificate.
			/// </summary>
			/// <remarks>
			/// <para>A column specifying the subject email address of the certificate.</para>
			/// <para>This data-type for this column should be <see langword="string"/>.</para>
			/// </remarks>
			public const string SubjectEmail = "SUBJECTEMAIL";

			/// <summary>
			/// A column specifying the subject DNS names of the certificate.
			/// </summary>
			/// <remarks>
			/// <para>A column specifying the subject DNS names of the certificate.</para>
			/// <para>This data-type for this column should be <see langword="string"/>.</para>
			/// </remarks>
			public const string SubjectDnsNames = "SUBJECTDNSNAMES";

			/// <summary>
			/// A column specifying the fingerprint of the certificate.
			/// </summary>
			/// <remarks>
			/// <para>A column specifying the fingerprint of the certificate.</para>
			/// <para>This data-type for this column should be <see langword="string"/>.</para>
			/// </remarks>
			public const string Fingerprint = "FINGERPRINT";

			/// <summary>
			/// A column specifying the encryption algorithms supported by the certificate.
			/// </summary>
			/// <remarks>
			/// <para>A column specifying the encryption algorithms supported by the certificate.</para>
			/// <para>This data-type for this column should be <see langword="string"/>.</para>
			/// </remarks>
			public const string Algorithms = "ALGORITHMS";

			/// <summary>
			/// A column specifying the date and time of the last update to the <see cref="Algorithms"/> column.
			/// </summary>
			/// <remarks>
			/// <para>A column specifying the date and time of the last update to the <see cref="Algorithms"/> column.</para>
			/// <para>This data-type for this column should be <see langword="long"/>.</para>
			/// </remarks>
			public const string AlgorithmsUpdated = "ALGORITHMSUPDATED";

			/// <summary>
			/// A column containing the raw certificate data.
			/// </summary>
			/// <remarks>
			/// <para>A column containing the raw certificate data.</para>
			/// <para>This data-type for this column should be <see langword="byte[]"/>.</para>
			/// </remarks>
			public const string Certificate = "CERTIFICATE";

			/// <summary>
			/// A column containing the raw private key data.
			/// </summary>
			/// <remarks>
			/// <para>A column containing the raw private key data.</para>
			/// <para>This data-type for this column should be <see langword="byte[]"/>.</para>
			/// </remarks>
			public const string PrivateKey = "PRIVATEKEY";
		}

		/// <summary>
		/// The column names for the CRLs table.
		/// </summary>
		/// <remarks>
		/// The column names for the CRLs table.
		/// </remarks>
		protected class CrlColumnNames
		{
			/// <summary>
			/// The auto-increment primary key identifier.
			/// </summary>
			/// <remarks>
			/// The auto-increment primary key identifier.
			/// </remarks>
			public const string Id = "ID";

			/// <summary>
			/// A column specifying whether the CRL data is a delta update.
			/// </summary>
			/// <remarks>
			/// <para>A column specifying whether the CRL data is a delta update.</para>
			/// <para>This data-type for this column should be <see langword="bool"/>.</para>
			/// </remarks>
			public const string Delta = "DELTA";

			/// <summary>
			/// A column specifying the issuer name of the certificate.
			/// </summary>
			/// <remarks>
			/// <para>A column specifying the issuer name of the certificate.</para>
			/// <para>This data-type for this column should be <see langword="string"/>.</para>
			/// </remarks>
			public const string IssuerName = "ISSUERNAME";

			/// <summary>
			/// A column specifying the date and time of the last update.
			/// </summary>
			/// <remarks>
			/// <para>A column specifying the date and time of the last update.</para>
			/// <para>This data-type for this column should be <see langword="long"/>.</para>
			/// </remarks>
			public const string ThisUpdate = "THISUPDATE";

			/// <summary>
			/// A column specifying the date and time of the next update.
			/// </summary>
			/// <remarks>
			/// <para>A column specifying the date and time of the next update.</para>
			/// <para>This data-type for this column should be <see langword="long"/>.</para>
			/// </remarks>
			public const string NextUpdate = "NEXTUPDATE";

			/// <summary>
			/// A column containing the raw CRL data.
			/// </summary>
			/// <remarks>
			/// <para>A column containing the raw CRL data.</para>
			/// <para>This data-type for this column should be <see langword="byte[]"/>.</para>
			/// </remarks>
			public const string Crl = "CRL";
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="X509CertificateDatabase"/> class.
		/// </summary>
		/// <remarks>
		/// The password is used to encrypt and decrypt private keys in the database and cannot be null.
		/// </remarks>
		/// <param name="connection">The database connection.</param>
		/// <param name="password">The password used for encrypting and decrypting the private keys.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="connection"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <see langword="null"/>.</para>
		/// </exception>
		protected X509CertificateDatabase (DbConnection connection, string password) : this (connection, password, new SecureRandom ())
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="X509CertificateDatabase"/> class.
		/// </summary>
		/// <remarks>
		/// The password is used to encrypt and decrypt private keys in the database and cannot be null.
		/// </remarks>
		/// <param name="connection">The database connection.</param>
		/// <param name="password">The password used for encrypting and decrypting the private keys.</param>
		/// <param name="random">The secure pseudo-random number generator.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="connection"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="random"/> is <see langword="null"/>.</para>
		/// </exception>
		protected X509CertificateDatabase (DbConnection connection, string password, SecureRandom random)
		{
			if (connection == null)
				throw new ArgumentNullException (nameof (connection));

			if (password == null)
				throw new ArgumentNullException (nameof (password));

			if (random == null)
				throw new ArgumentNullException (nameof (random));

			EncryptionAlgorithm = DefaultEncryptionAlgorithm;
			MinIterations = DefaultMinIterations;
			RandomNumberGenerator = random;
			SaltSize = DefaultSaltSize;

			this.password = password.ToCharArray ();
			this.connection = connection;
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="X509CertificateDatabase"/> is reclaimed by garbage collection.
		/// </summary>
		/// <remarks>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="X509CertificateDatabase"/> is reclaimed by garbage collection.
		/// </remarks>
		~X509CertificateDatabase ()
		{
			Dispose (false);
		}

		/// <summary>
		/// Gets or sets the algorithm used for encrypting the private keys.
		/// </summary>
		/// <remarks>
		/// <para>The encryption algorithm should be one of the PBE (password-based encryption) algorithms
		/// supported by Bouncy Castle.</para>
		/// <para>The default algorithm is SHA-256 + AES256.</para>
		/// </remarks>
		/// <value>The encryption algorithm.</value>
		protected DerObjectIdentifier EncryptionAlgorithm {
			get; set;
		}

		/// <summary>
		/// Gets or sets the minimum iterations.
		/// </summary>
		/// <remarks>
		/// The default minimum number of iterations is <c>1024</c>.
		/// </remarks>
		/// <value>The minimum iterations.</value>
		protected int MinIterations {
			get; set;
		}

		/// <summary>
		/// Get the secure pseudo-random number generator used when encrypting private keys.
		/// </summary>
		/// <remarks>
		/// Gets the secure pseudo-random number generator used when encrypting private keys.
		/// </remarks>
		/// <value>The secure pseudo-random number generator.</value>
		protected SecureRandom RandomNumberGenerator {
			get; private set;
		}

		/// <summary>
		/// Gets or sets the size of the salt.
		/// </summary>
		/// <remarks>
		/// The default salt size is <c>20</c>.
		/// </remarks>
		/// <value>The size of the salt.</value>
		protected int SaltSize {
			get; set;
		}

		internal static string EncodeDnsNames (string[] dnsNames)
		{
			if (dnsNames.Length == 0)
				return string.Empty;

			int size = 1;

			for (int i = 0; i < dnsNames.Length; i++)
				size += dnsNames[i].Length + 1;

			var encoded = new ValueStringBuilder (size);
			encoded.Append ('|');
			for (int i = 0; i < dnsNames.Length; i++) {
				encoded.Append (dnsNames[i]);
				encoded.Append ('|');
			}

			return encoded.ToString ();
		}

		static int ReadBinaryBlob (DbDataReader reader, int column, ref byte[] buffer)
		{
			long nread;

			// first, get the length of the buffer needed
			if ((nread = reader.GetBytes (column, 0, null, 0, buffer.Length)) > buffer.Length)
				Array.Resize (ref buffer, (int) nread);

			// read the certificate data
			return (int) reader.GetBytes (column, 0, buffer, 0, (int) nread);
		}

		static X509Certificate DecodeCertificate (DbDataReader reader, X509CertificateParser parser, int column, ref byte[] buffer)
		{
			int nread = ReadBinaryBlob (reader, column, ref buffer);

			using (var memory = new MemoryStream (buffer, 0, nread, false)) {
				return parser.ReadCertificate (memory);
			}
		}

		static X509Crl DecodeX509Crl (DbDataReader reader, X509CrlParser parser, int column, ref byte[] buffer)
		{
			int nread = ReadBinaryBlob (reader, column, ref buffer);

			using (var memory = new MemoryStream (buffer, 0, nread, false)) {
				return parser.ReadCrl (memory);
			}
		}

		byte[] EncryptAsymmetricKeyParameter (AsymmetricKeyParameter key)
		{
			if (PbeUtilities.CreateEngine (EncryptionAlgorithm.Id) is not IBufferedCipher cipher)
				throw new Exception ("Unknown encryption algorithm: " + EncryptionAlgorithm.Id);

			var keyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo (key);
			var salt = new byte[SaltSize];

			RandomNumberGenerator.NextBytes (salt);

			var pbeParameters = PbeUtilities.GenerateAlgorithmParameters (EncryptionAlgorithm.Id, salt, MinIterations);
			var algorithm = new AlgorithmIdentifier (EncryptionAlgorithm, pbeParameters);
			var cipherParameters = PbeUtilities.GenerateCipherParameters (algorithm, password);

			if (cipherParameters == null)
				throw new Exception ("BouncyCastle bug detected: Failed to generate cipher parameters.");

			cipher.Init (true, cipherParameters);

			var encoded = cipher.DoFinal (keyInfo.GetEncoded ());

			var encrypted = new EncryptedPrivateKeyInfo (algorithm, encoded);

			return encrypted.GetEncoded ();
		}

		AsymmetricKeyParameter DecryptAsymmetricKeyParameter (byte[] buffer, int length)
		{
			using (var memory = new MemoryStream (buffer, 0, length, false)) {
				using (var asn1 = new Asn1InputStream (memory)) {
					if (asn1.ReadObject () is not Asn1Sequence sequence)
						return null;

					var encrypted = EncryptedPrivateKeyInfo.GetInstance (sequence);
					var algorithm = encrypted.EncryptionAlgorithm;

					if (PbeUtilities.CreateEngine (algorithm) is not IBufferedCipher cipher)
						return null;

					var cipherParameters = PbeUtilities.GenerateCipherParameters (algorithm, password);

					if (cipherParameters == null)
						throw new Exception ("BouncyCastle bug detected: Failed to generate cipher parameters.");

					cipher.Init (false, cipherParameters);

					var encoded = encrypted.GetEncryptedData ();
					var decrypted = cipher.DoFinal (encoded);
					var keyInfo = PrivateKeyInfo.GetInstance (decrypted);

					return PrivateKeyFactory.CreateKey (keyInfo);
				}
			}
		}

		AsymmetricKeyParameter DecodePrivateKey (DbDataReader reader, int column, ref byte[] buffer)
		{
			if (reader.IsDBNull (column))
				return null;

			int nread = ReadBinaryBlob (reader, column, ref buffer);

			return DecryptAsymmetricKeyParameter (buffer, nread);
		}

		object EncodePrivateKey (AsymmetricKeyParameter key)
		{
			return key != null ? (object) EncryptAsymmetricKeyParameter (key) : DBNull.Value;
		}

		static EncryptionAlgorithm[] DecodeEncryptionAlgorithms (DbDataReader reader, int column)
		{
			if (reader.IsDBNull (column))
				return null;

			var algorithms = new List<EncryptionAlgorithm> ();
			var values = reader.GetString (column);

			foreach (var token in values.Tokenize (',')) {
				var value = token.Trim ();

				if (value.IsEmpty)
					continue;

				if (Enum.TryParse (value.ToString (), true, out EncryptionAlgorithm algorithm))
					algorithms.Add (algorithm);
			}

			return algorithms.ToArray ();
		}

		static object EncodeEncryptionAlgorithms (EncryptionAlgorithm[] algorithms)
		{
			if (algorithms == null || algorithms.Length == 0)
				return DBNull.Value;

			var tokens = new string[algorithms.Length];
			for (int i = 0; i < algorithms.Length; i++)
				tokens[i] = algorithms[i].ToString ();

			return string.Join (",", tokens);
		}

		X509CertificateRecord LoadCertificateRecord (DbDataReader reader, X509CertificateParser parser, ref byte[] buffer)
		{
			var record = new X509CertificateRecord ();

			for (int i = 0; i < reader.FieldCount; i++) {
				switch (reader.GetName (i).ToUpperInvariant ()) {
				case CertificateColumnNames.Certificate:
					record.Certificate = DecodeCertificate (reader, parser, i, ref buffer);
					break;
				case CertificateColumnNames.PrivateKey:
					record.PrivateKey = DecodePrivateKey (reader, i, ref buffer);
					break;
				case CertificateColumnNames.Algorithms:
					record.Algorithms = DecodeEncryptionAlgorithms (reader, i);
					break;
				case CertificateColumnNames.AlgorithmsUpdated:
					record.AlgorithmsUpdated = DateTime.SpecifyKind (reader.GetDateTime (i), DateTimeKind.Utc);
					break;
				case CertificateColumnNames.Trusted:
					record.IsTrusted = reader.GetBoolean (i);
					break;
				case CertificateColumnNames.Id:
					record.Id = reader.GetInt32 (i);
					break;
				}
			}

			return record;
		}

		static X509CrlRecord LoadCrlRecord (DbDataReader reader, X509CrlParser parser, ref byte[] buffer)
		{
			var record = new X509CrlRecord ();

			for (int i = 0; i < reader.FieldCount; i++) {
				switch (reader.GetName (i).ToUpperInvariant ()) {
				case CrlColumnNames.Crl:
					record.Crl = DecodeX509Crl (reader, parser, i, ref buffer);
					break;
				case CrlColumnNames.ThisUpdate:
					record.ThisUpdate = DateTime.SpecifyKind (reader.GetDateTime (i), DateTimeKind.Utc);
					break;
				case CrlColumnNames.NextUpdate:
					record.NextUpdate = DateTime.SpecifyKind (reader.GetDateTime (i), DateTimeKind.Utc);
					break;
				case CrlColumnNames.Delta:
					record.IsDelta = reader.GetBoolean (i);
					break;
				case CrlColumnNames.Id:
					record.Id = reader.GetInt32 (i);
					break;
				}
			}

			return record;
		}

		/// <summary>
		/// Gets the column names for the specified fields.
		/// </summary>
		/// <remarks>
		/// Gets the column names for the specified fields.
		/// </remarks>
		/// <returns>The column names.</returns>
		/// <param name="fields">The fields.</param>
		protected static string[] GetColumnNames (X509CertificateRecordFields fields)
		{
			var columns = new List<string> ();

			if ((fields & X509CertificateRecordFields.Id) != 0)
				columns.Add (CertificateColumnNames.Id);
			if ((fields & X509CertificateRecordFields.Trusted) != 0)
				columns.Add (CertificateColumnNames.Trusted);
			if ((fields & X509CertificateRecordFields.Algorithms) != 0)
				columns.Add (CertificateColumnNames.Algorithms);
			if ((fields & X509CertificateRecordFields.AlgorithmsUpdated) != 0)
				columns.Add (CertificateColumnNames.AlgorithmsUpdated);
			if ((fields & X509CertificateRecordFields.Certificate) != 0)
				columns.Add (CertificateColumnNames.Certificate);
			if ((fields & X509CertificateRecordFields.PrivateKey) != 0)
				columns.Add (CertificateColumnNames.PrivateKey);

			return columns.ToArray ();
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
		protected abstract DbCommand GetSelectCommand (DbConnection connection, X509Certificate certificate, X509CertificateRecordFields fields);

		/// <summary>
		/// Gets the database command to select the certificate records for the specified mailbox.
		/// </summary>
		/// <remarks>
		/// Gets the database command to select the certificate records for the specified mailbox.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="connection">The database connection.</param>
		/// <param name="mailbox">The mailbox.</param>
		/// <param name="now">The date and time for which the certificate should be valid.</param>
		/// <param name="requirePrivateKey"><c>true</c> if the certificate must have a private key; otherwise, <c>false</c>.</param>
		/// <param name="fields">The fields to return.</param>
		protected abstract DbCommand GetSelectCommand (DbConnection connection, MailboxAddress mailbox, DateTime now, bool requirePrivateKey, X509CertificateRecordFields fields);

		/// <summary>
		/// Gets the database command to select certificate records matching the specified selector.
		/// </summary>
		/// <remarks>
		/// Gets the database command to select certificate records matching the specified selector.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="connection">The database connection.</param>
		/// <param name="selector">The certificate selector.</param>
		/// <param name="trustedAnchorsOnly"><c>true</c> if only trusted anchor certificates should be matched; otherwise, <c>false</c>.</param>
		/// <param name="requirePrivateKey"><c>true</c> if the certificate must have a private key; otherwise, <c>false</c>.</param>
		/// <param name="fields">The fields to return.</param>
		protected abstract DbCommand GetSelectCommand (DbConnection connection, ISelector<X509Certificate> selector, bool trustedAnchorsOnly, bool requirePrivateKey, X509CertificateRecordFields fields);

		/// <summary>
		/// Gets the column names for the specified fields.
		/// </summary>
		/// <remarks>
		/// Gets the column names for the specified fields.
		/// </remarks>
		/// <returns>The column names.</returns>
		/// <param name="fields">The fields.</param>
		protected static string[] GetColumnNames (X509CrlRecordFields fields)
		{
			var columns = new List<string> ();

			if ((fields & X509CrlRecordFields.Id) != 0)
				columns.Add (CrlColumnNames.Id);
			if ((fields & X509CrlRecordFields.IsDelta) != 0)
				columns.Add (CrlColumnNames.Delta);
			if ((fields & X509CrlRecordFields.IssuerName) != 0)
				columns.Add (CrlColumnNames.IssuerName);
			if ((fields & X509CrlRecordFields.ThisUpdate) != 0)
				columns.Add (CrlColumnNames.ThisUpdate);
			if ((fields & X509CrlRecordFields.NextUpdate) != 0)
				columns.Add (CrlColumnNames.NextUpdate);
			if ((fields & X509CrlRecordFields.Crl) != 0)
				columns.Add (CrlColumnNames.Crl);

			return columns.ToArray ();
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
		protected abstract DbCommand GetSelectCommand (DbConnection connection, X509Name issuer, X509CrlRecordFields fields);

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
		protected abstract DbCommand GetSelectCommand (DbConnection connection, X509Crl crl, X509CrlRecordFields fields);

		/// <summary>
		/// Gets the database command to select all CRLs in the table.
		/// </summary>
		/// <remarks>
		/// Gets the database command to select all CRLs in the table.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="connection">The database connection.</param>
		protected abstract DbCommand GetSelectAllCrlsCommand (DbConnection connection);

		/// <summary>
		/// Gets the database command to delete the specified certificate record.
		/// </summary>
		/// <remarks>
		/// Gets the database command to delete the specified certificate record.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="connection">The database connection.</param>
		/// <param name="record">The certificate record.</param>
		protected abstract DbCommand GetDeleteCommand (DbConnection connection, X509CertificateRecord record);

		/// <summary>
		/// Gets the database command to delete the specified CRL record.
		/// </summary>
		/// <remarks>
		/// Gets the database command to delete the specified CRL record.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="connection">The database connection.</param>
		/// <param name="record">The record.</param>
		protected abstract DbCommand GetDeleteCommand (DbConnection connection, X509CrlRecord record);

		/// <summary>
		/// Gets the value for the specified column.
		/// </summary>
		/// <remarks>
		/// Gets the value for the specified column.
		/// </remarks>
		/// <returns>The value.</returns>
		/// <param name="record">The certificate record.</param>
		/// <param name="columnName">The column name.</param>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="columnName"/> is not a known column name.
		/// </exception>
		protected object GetValue (X509CertificateRecord record, string columnName)
		{
			switch (columnName) {
			//case CertificateColumnNames.Id: return record.Id;
			case CertificateColumnNames.BasicConstraints: return record.BasicConstraints;
			case CertificateColumnNames.Trusted: return record.IsTrusted;
			case CertificateColumnNames.Anchor: return record.IsAnchor;
			case CertificateColumnNames.KeyUsage: return (int) record.KeyUsage;
			case CertificateColumnNames.NotBefore: return record.NotBefore.ToUniversalTime ();
			case CertificateColumnNames.NotAfter: return record.NotAfter.ToUniversalTime ();
			case CertificateColumnNames.IssuerName: return record.IssuerName;
			case CertificateColumnNames.SerialNumber: return record.SerialNumber;
			case CertificateColumnNames.SubjectName: return record.SubjectName;
			case CertificateColumnNames.SubjectKeyIdentifier: return record.SubjectKeyIdentifier?.AsHex ();
			case CertificateColumnNames.SubjectEmail: return record.SubjectEmail;
			case CertificateColumnNames.SubjectDnsNames: return EncodeDnsNames (record.SubjectDnsNames);
			case CertificateColumnNames.Fingerprint: return record.Fingerprint.ToLowerInvariant ();
			case CertificateColumnNames.Algorithms: return EncodeEncryptionAlgorithms (record.Algorithms);
			case CertificateColumnNames.AlgorithmsUpdated: return record.AlgorithmsUpdated;
			case CertificateColumnNames.Certificate: return record.Certificate.GetEncoded ();
			case CertificateColumnNames.PrivateKey: return EncodePrivateKey (record.PrivateKey);
			default: throw new ArgumentException (string.Format ("Unknown column name: {0}", columnName), nameof (columnName));
			}
		}

		/// <summary>
		/// Gets the value for the specified column.
		/// </summary>
		/// <remarks>
		/// Gets the value for the specified column.
		/// </remarks>
		/// <returns>The value.</returns>
		/// <param name="record">The CRL record.</param>
		/// <param name="columnName">The column name.</param>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="columnName"/> is not a known column name.
		/// </exception>
		protected static object GetValue (X509CrlRecord record, string columnName)
		{
			switch (columnName) {
			//case CrlColumnNames.Id: return record.Id;
			case CrlColumnNames.Delta: return record.IsDelta;
			case CrlColumnNames.IssuerName: return record.IssuerName;
			case CrlColumnNames.ThisUpdate: return record.ThisUpdate;
			case CrlColumnNames.NextUpdate: return record.NextUpdate;
			case CrlColumnNames.Crl: return record.Crl.GetEncoded ();
			default: throw new ArgumentException (string.Format ("Unknown column name: {0}", columnName), nameof (columnName));
			}
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
		protected abstract DbCommand GetInsertCommand (DbConnection connection, X509CertificateRecord record);

		/// <summary>
		/// Gets the database command to insert the specified CRL record.
		/// </summary>
		/// <remarks>
		/// Gets the database command to insert the specified CRL record.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="connection">The database connection.</param>
		/// <param name="record">The CRL record.</param>
		protected abstract DbCommand GetInsertCommand (DbConnection connection, X509CrlRecord record);

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
		protected abstract DbCommand GetUpdateCommand (DbConnection connection, X509CertificateRecord record, X509CertificateRecordFields fields);

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
		protected abstract DbCommand GetUpdateCommand (DbConnection connection, X509CrlRecord record);

		/// <summary>
		/// Find the specified certificate.
		/// </summary>
		/// <remarks>
		/// Searches the database for the specified certificate, returning the matching
		/// record with the desired fields populated.
		/// </remarks>
		/// <returns>The matching record if found; otherwise <see langword="null"/>.</returns>
		/// <param name="certificate">The certificate.</param>
		/// <param name="fields">The desired fields.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <see langword="null"/>.
		/// </exception>
		public X509CertificateRecord Find (X509Certificate certificate, X509CertificateRecordFields fields)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			using (var command = GetSelectCommand (connection, certificate, fields)) {
				using (var reader = command.ExecuteReader ()) {
					if (reader.Read ()) {
						var parser = new X509CertificateParser ();
						var buffer = new byte[4096];

						return LoadCertificateRecord (reader, parser, ref buffer);
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Finds the certificates matching the specified selector.
		/// </summary>
		/// <remarks>
		/// Searches the database for certificates matching the selector, returning all
		/// matching certificates.
		/// </remarks>
		/// <returns>The matching certificates.</returns>
		/// <param name="selector">The match selector or <see langword="null"/> to return all certificates.</param>
		public IEnumerable<X509Certificate> FindCertificates (ISelector<X509Certificate> selector)
		{
			using (var command = GetSelectCommand (connection, selector, false, false, X509CertificateRecordFields.Certificate)) {
				using (var reader = command.ExecuteReader ()) {
					var parser = new X509CertificateParser ();
					var buffer = new byte[4096];

					while (reader.Read ()) {
						var record = LoadCertificateRecord (reader, parser, ref buffer);
						if (selector == null || selector.Match (record.Certificate))
							yield return record.Certificate;
					}
				}
			}

			yield break;
		}

		/// <summary>
		/// Finds the private keys matching the specified selector.
		/// </summary>
		/// <remarks>
		/// Searches the database for certificate records matching the selector, returning the
		/// private keys for each matching record.
		/// </remarks>
		/// <returns>The matching certificates.</returns>
		/// <param name="selector">The match selector or <see langword="null"/> to return all private keys.</param>
		public IEnumerable<AsymmetricKeyParameter> FindPrivateKeys (ISelector<X509Certificate> selector)
		{
			using (var command = GetSelectCommand (connection, selector, false, true, PrivateKeyFields)) {
				using (var reader = command.ExecuteReader ()) {
					var parser = new X509CertificateParser ();
					var buffer = new byte[4096];

					while (reader.Read ()) {
						var record = LoadCertificateRecord (reader, parser, ref buffer);

						if (selector == null || selector.Match (record.Certificate))
							yield return record.PrivateKey;
					}
				}
			}

			yield break;
		}

		/// <summary>
		/// Finds the certificate records for the specified mailbox.
		/// </summary>
		/// <remarks>
		/// Searches the database for certificates matching the specified mailbox that are valid
		/// for the date and time specified, returning all matching records populated with the
		/// desired fields.
		/// </remarks>
		/// <returns>The matching certificate records populated with the desired fields.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <param name="now">The date and time.</param>
		/// <param name="requirePrivateKey"><c>true</c> if a private key is required.</param>
		/// <param name="fields">The desired fields.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailbox"/> is <see langword="null"/>.
		/// </exception>
		public IEnumerable<X509CertificateRecord> Find (MailboxAddress mailbox, DateTime now, bool requirePrivateKey, X509CertificateRecordFields fields)
		{
			if (mailbox == null)
				throw new ArgumentNullException (nameof (mailbox));

			using (var command = GetSelectCommand (connection, mailbox, now, requirePrivateKey, fields)) {
				using (var reader = command.ExecuteReader ()) {
					var parser = new X509CertificateParser ();
					var buffer = new byte[4096];

					while (reader.Read ()) {
						yield return LoadCertificateRecord (reader, parser, ref buffer);
					}
				}
			}

			yield break;
		}

		/// <summary>
		/// Finds the certificate records matching the specified selector.
		/// </summary>
		/// <remarks>
		/// Searches the database for certificate records matching the selector, returning all
		/// the matching records populated with the desired fields.
		/// </remarks>
		/// <returns>The matching certificate records populated with the desired fields.</returns>
		/// <param name="selector">The match selector or <see langword="null"/> to match all certificates.</param>
		/// <param name="trustedAnchorsOnly"><c>true</c> if only trusted anchor certificates should be returned.</param>
		/// <param name="fields">The desired fields.</param>
		public IEnumerable<X509CertificateRecord> Find (ISelector<X509Certificate> selector, bool trustedAnchorsOnly, X509CertificateRecordFields fields)
		{
			using (var command = GetSelectCommand (connection, selector, trustedAnchorsOnly, false, fields | X509CertificateRecordFields.Certificate)) {
				using (var reader = command.ExecuteReader ()) {
					var parser = new X509CertificateParser ();
					var buffer = new byte[4096];

					while (reader.Read ()) {
						var record = LoadCertificateRecord (reader, parser, ref buffer);

						if (selector == null || selector.Match (record.Certificate))
							yield return record;
					}
				}
			}

			yield break;
		}

		/// <summary>
		/// Add the specified certificate record.
		/// </summary>
		/// <remarks>
		/// Adds the specified certificate record to the database.
		/// </remarks>
		/// <param name="record">The certificate record.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="record"/> is <see langword="null"/>.
		/// </exception>
		public void Add (X509CertificateRecord record)
		{
			if (record == null)
				throw new ArgumentNullException (nameof (record));

			using (var command = GetInsertCommand (connection, record))
				command.ExecuteNonQuery ();
		}

		/// <summary>
		/// Remove the specified certificate record.
		/// </summary>
		/// <remarks>
		/// Removes the specified certificate record from the database.
		/// </remarks>
		/// <param name="record">The certificate record.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="record"/> is <see langword="null"/>.
		/// </exception>
		public void Remove (X509CertificateRecord record)
		{
			if (record == null)
				throw new ArgumentNullException (nameof (record));

			using (var command = GetDeleteCommand (connection, record))
				command.ExecuteNonQuery ();
		}

		/// <summary>
		/// Update the specified certificate record.
		/// </summary>
		/// <remarks>
		/// Updates the specified fields of the record in the database.
		/// </remarks>
		/// <param name="record">The certificate record.</param>
		/// <param name="fields">The fields to update.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="record"/> is <see langword="null"/>.
		/// </exception>
		public void Update (X509CertificateRecord record, X509CertificateRecordFields fields)
		{
			if (record == null)
				throw new ArgumentNullException (nameof (record));

			using (var command = GetUpdateCommand (connection, record, fields))
				command.ExecuteNonQuery ();
		}

		/// <summary>
		/// Finds the CRL records for the specified issuer.
		/// </summary>
		/// <remarks>
		/// Searches the database for CRL records matching the specified issuer, returning
		/// all matching records populated with the desired fields.
		/// </remarks>
		/// <returns>The matching CRL records populated with the desired fields.</returns>
		/// <param name="issuer">The issuer.</param>
		/// <param name="fields">The desired fields.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="issuer"/> is <see langword="null"/>.
		/// </exception>
		public IEnumerable<X509CrlRecord> Find (X509Name issuer, X509CrlRecordFields fields)
		{
			if (issuer == null)
				throw new ArgumentNullException (nameof (issuer));

			using (var command = GetSelectCommand (connection, issuer, fields)) {
				using (var reader = command.ExecuteReader ()) {
					var parser = new X509CrlParser ();
					var buffer = new byte[4096];

					while (reader.Read ()) {
						yield return LoadCrlRecord (reader, parser, ref buffer);
					}
				}
			}

			yield break;
		}

		/// <summary>
		/// Finds the specified certificate revocation list.
		/// </summary>
		/// <remarks>
		/// Searches the database for the specified CRL, returning the matching record with
		/// the desired fields populated.
		/// </remarks>
		/// <returns>The matching record if found; otherwise <see langword="null"/>.</returns>
		/// <param name="crl">The certificate revocation list.</param>
		/// <param name="fields">The desired fields.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="crl"/> is <see langword="null"/>.
		/// </exception>
		public X509CrlRecord Find (X509Crl crl, X509CrlRecordFields fields)
		{
			if (crl == null)
				throw new ArgumentNullException (nameof (crl));

			using (var command = GetSelectCommand (connection, crl, fields)) {
				using (var reader = command.ExecuteReader ()) {
					if (reader.Read ()) {
						var parser = new X509CrlParser ();
						var buffer = new byte[4096];

						return LoadCrlRecord (reader, parser, ref buffer);
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Add the specified CRL record.
		/// </summary>
		/// <remarks>
		/// Adds the specified CRL record to the database.
		/// </remarks>
		/// <param name="record">The CRL record.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="record"/> is <see langword="null"/>.
		/// </exception>
		public void Add (X509CrlRecord record)
		{
			if (record == null)
				throw new ArgumentNullException (nameof (record));

			using (var command = GetInsertCommand (connection, record))
				command.ExecuteNonQuery ();
		}

		/// <summary>
		/// Remove the specified CRL record.
		/// </summary>
		/// <remarks>
		/// Removes the specified CRL record from the database.
		/// </remarks>
		/// <param name="record">The CRL record.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="record"/> is <see langword="null"/>.
		/// </exception>
		public void Remove (X509CrlRecord record)
		{
			if (record == null)
				throw new ArgumentNullException (nameof (record));

			using (var command = GetDeleteCommand (connection, record))
				command.ExecuteNonQuery ();
		}

		/// <summary>
		/// Update the specified CRL record.
		/// </summary>
		/// <remarks>
		/// Updates the specified fields of the record in the database.
		/// </remarks>
		/// <param name="record">The CRL record.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="record"/> is <see langword="null"/>.
		/// </exception>
		[Obsolete ("This method is not used and will be removed in a future release.")]
		public void Update (X509CrlRecord record)
		{
			if (record == null)
				throw new ArgumentNullException (nameof (record));

			using (var command = GetUpdateCommand (connection, record))
				command.ExecuteNonQuery ();
		}

		/// <summary>
		/// Gets a certificate revocation list store.
		/// </summary>
		/// <remarks>
		/// Gets a certificate revocation list store.
		/// </remarks>
		/// <returns>A certificate revocation list store.</returns>
		public IStore<X509Crl> GetCrlStore ()
		{
			var crls = new List<X509Crl> ();

			using (var command = GetSelectAllCrlsCommand (connection)) {
				using (var reader = command.ExecuteReader ()) {
					var parser = new X509CrlParser ();
					var buffer = new byte[4096];

					while (reader.Read ()) {
						var record = LoadCrlRecord (reader, parser, ref buffer);
						crls.Add (record.Crl);
					}
				}
			}

			return CollectionUtilities.CreateStore (crls);
		}

#region IX509Store implementation

		/// <summary>
		/// Gets a collection of matching certificates matching the specified selector.
		/// </summary>
		/// <remarks>
		/// Gets a collection of matching certificates matching the specified selector.
		/// </remarks>
		/// <returns>The matching certificates.</returns>
		/// <param name="selector">The match criteria.</param>
		IEnumerable<X509Certificate> IStore<X509Certificate>.EnumerateMatches (ISelector<X509Certificate> selector)
		{
			return new List<X509Certificate> (FindCertificates (selector));
		}

#endregion

#region IDisposable implementation

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="X509CertificateDatabase"/> and
		/// optionally releases the managed resources.
		/// </summary>
		/// <remarks>
		/// Releases the unmanaged resources used by the <see cref="X509CertificateDatabase"/> and
		/// optionally releases the managed resources.
		/// </remarks>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
		/// <c>false</c> to release only the unmanaged resources.</param>
		protected virtual void Dispose (bool disposing)
		{
			if (!disposing)
				return;

			if (password != null) {
				for (int i = 0; i < password.Length; i++)
					password[i] = '\0';
				password = null;
			}

			if (connection != null) {
				connection.Dispose ();
				connection = null;
			}
		}

		/// <summary>
		/// Releases all resource used by the <see cref="X509CertificateDatabase"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose()"/> when you are finished using the
		/// <see cref="X509CertificateDatabase"/>. The <see cref="Dispose()"/> method leaves the
		/// <see cref="X509CertificateDatabase"/> in an unusable state. After calling
		/// <see cref="Dispose()"/>, you must release all references to the
		/// <see cref="X509CertificateDatabase"/> so the garbage collector can reclaim the memory that
		/// the <see cref="X509CertificateDatabase"/> was occupying.</remarks>
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

#endregion
	}
}

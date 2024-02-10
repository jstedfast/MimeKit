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
		/// Initialize a new instance of the <see cref="X509CertificateDatabase"/> class.
		/// </summary>
		/// <remarks>
		/// The password is used to encrypt and decrypt private keys in the database and cannot be null.
		/// </remarks>
		/// <param name="connection">The database connection.</param>
		/// <param name="password">The password used for encrypting and decrypting the private keys.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="connection"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
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
		/// <para><paramref name="connection"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="random"/> is <c>null</c>.</para>
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
					record.AlgorithmsUpdated = DateTime.SpecifyKind (reader.GetDateTime (i), DateTimeKind.Utc);
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

		static X509CrlRecord LoadCrlRecord (DbDataReader reader, X509CrlParser parser, ref byte[] buffer)
		{
			var record = new X509CrlRecord ();

			for (int i = 0; i < reader.FieldCount; i++) {
				switch (reader.GetName (i).ToUpperInvariant ()) {
				case "CRL":
					record.Crl = DecodeX509Crl (reader, parser, i, ref buffer);
					break;
				case "THISUPDATE":
					record.ThisUpdate = DateTime.SpecifyKind (reader.GetDateTime (i), DateTimeKind.Utc);
					break;
				case "NEXTUPDATE":
					record.NextUpdate = DateTime.SpecifyKind (reader.GetDateTime (i), DateTimeKind.Utc);
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
			//case "ID": return record.Id;
			case "BASICCONSTRAINTS": return record.BasicConstraints;
			case "TRUSTED": return record.IsTrusted;
			case "ANCHOR": return record.IsAnchor;
			case "KEYUSAGE": return (int) record.KeyUsage;
			case "NOTBEFORE": return record.NotBefore.ToUniversalTime ();
			case "NOTAFTER": return record.NotAfter.ToUniversalTime ();
			case "ISSUERNAME": return record.IssuerName;
			case "SERIALNUMBER": return record.SerialNumber;
			case "SUBJECTNAME": return record.SubjectName;
			case "SUBJECTKEYIDENTIFIER": return record.SubjectKeyIdentifier?.AsHex ();
			case "SUBJECTEMAIL": return record.SubjectEmail != null ? record.SubjectEmail.ToLowerInvariant () : string.Empty;
			case "FINGERPRINT": return record.Fingerprint.ToLowerInvariant ();
			case "ALGORITHMS": return EncodeEncryptionAlgorithms (record.Algorithms);
			case "ALGORITHMSUPDATED": return record.AlgorithmsUpdated;
			case "CERTIFICATE": return record.Certificate.GetEncoded ();
			case "PRIVATEKEY": return EncodePrivateKey (record.PrivateKey);
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
			//case "ID": return record.Id;
			case "DELTA": return record.IsDelta;
			case "ISSUERNAME": return record.IssuerName;
			case "THISUPDATE": return record.ThisUpdate;
			case "NEXTUPDATE": return record.NextUpdate;
			case "CRL": return record.Crl.GetEncoded ();
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
		protected abstract DbCommand GetUpdateCommand (DbConnection connection, X509CrlRecord record);

		/// <summary>
		/// Find the specified certificate.
		/// </summary>
		/// <remarks>
		/// Searches the database for the specified certificate, returning the matching
		/// record with the desired fields populated.
		/// </remarks>
		/// <returns>The matching record if found; otherwise <c>null</c>.</returns>
		/// <param name="certificate">The certificate.</param>
		/// <param name="fields">The desired fields.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
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
		/// <param name="selector">The match selector or <c>null</c> to return all certificates.</param>
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
		/// <param name="selector">The match selector or <c>null</c> to return all private keys.</param>
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
		/// <paramref name="mailbox"/> is <c>null</c>.
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
		/// of the matching records populated with the desired fields.
		/// </remarks>
		/// <returns>The matching certificate records populated with the desired fields.</returns>
		/// <param name="selector">The match selector or <c>null</c> to match all certificates.</param>
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
		/// <paramref name="record"/> is <c>null</c>.
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
		/// <paramref name="record"/> is <c>null</c>.
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
		/// <paramref name="record"/> is <c>null</c>.
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
		/// <paramref name="issuer"/> is <c>null</c>.
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
		/// <returns>The matching record if found; otherwise <c>null</c>.</returns>
		/// <param name="crl">The certificate revocation list.</param>
		/// <param name="fields">The desired fields.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="crl"/> is <c>null</c>.
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
		/// <paramref name="record"/> is <c>null</c>.
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
		/// <paramref name="record"/> is <c>null</c>.
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
		/// <paramref name="record"/> is <c>null</c>.
		/// </exception>
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

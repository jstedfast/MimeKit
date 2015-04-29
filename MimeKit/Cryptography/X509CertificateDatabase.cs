//
// X509CertificateDatabase.cs
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
using System.Collections;
using System.Collections.Generic;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Asn1.BC;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509.Store;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An X.509 certificate database.
	/// </summary>
	/// <remarks>
	/// An X.509 certificate database is used for storing certificates, metdata related to the certificates
	/// (such as encryption algorithms supported by the associated client), certificate revocation lists (CRLs),
	/// and private keys.
	/// </remarks>
	public abstract class X509CertificateDatabase : IX509CertificateDatabase
	{
		const X509CertificateRecordFields PrivateKeyFields = X509CertificateRecordFields.Certificate | X509CertificateRecordFields.PrivateKey;
		static readonly DerObjectIdentifier DefaultEncryptionAlgorithm = BCObjectIdentifiers.bc_pbe_sha256_pkcs12_aes256_cbc;
		const int DefaultMinIterations = 1024;
		const int DefaultSaltSize = 20;

		readonly char[] passwd;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.X509CertificateDatabase"/> class.
		/// </summary>
		/// <remarks>
		/// The password is used to encrypt and decrypt private keys in the database and cannot be null.
		/// </remarks>
		/// <param name="password">The password used for encrypting and decrypting the private keys.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="password"/> is <c>null</c>.
		/// </exception>
		protected X509CertificateDatabase (string password)
		{
			if (password == null)
				throw new ArgumentNullException ("password");

			EncryptionAlgorithm = DefaultEncryptionAlgorithm;
			MinIterations = DefaultMinIterations;
			SaltSize = DefaultSaltSize;

			passwd = password.ToCharArray ();
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="MimeKit.Cryptography.X509CertificateDatabase"/> is reclaimed by garbage collection.
		/// </summary>
		/// <remarks>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="MimeKit.Cryptography.X509CertificateDatabase"/> is reclaimed by garbage collection.
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
		/// Gets or sets the size of the salt.
		/// </summary>
		/// <remarks>
		/// The default salt size is <c>20</c>.
		/// </remarks>
		/// <value>The size of the salt.</value>
		protected int SaltSize {
			get; set;
		}

		static int ReadBinaryBlob (IDataRecord reader, int column, ref byte[] buffer)
		{
			long nread;

			// first, get the length of the buffer needed
			if ((nread = reader.GetBytes (column, 0, null, 0, buffer.Length)) > buffer.Length)
				Array.Resize (ref buffer, (int) nread);

			// read the certificate data
			return (int) reader.GetBytes (column, 0, buffer, 0, (int) nread);
		}

		static X509Certificate DecodeCertificate (IDataRecord reader, X509CertificateParser parser, int column, ref byte[] buffer)
		{
			int nread = ReadBinaryBlob (reader, column, ref buffer);

			using (var memory = new MemoryStream (buffer, 0, nread, false)) {
				return parser.ReadCertificate (memory);
			}
		}

		static X509Crl DecodeX509Crl (IDataRecord reader, X509CrlParser parser, int column, ref byte[] buffer)
		{
			int nread = ReadBinaryBlob (reader, column, ref buffer);

			using (var memory = new MemoryStream (buffer, 0, nread, false)) {
				return parser.ReadCrl (memory);
			}
		}

		byte[] EncryptAsymmetricKeyParameter (AsymmetricKeyParameter key)
		{
			var cipher = PbeUtilities.CreateEngine (EncryptionAlgorithm.Id) as IBufferedCipher;
			var keyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo (key);
			var random = new SecureRandom ();
			var salt = new byte[SaltSize];

			if (cipher == null)
				throw new Exception ("Unknown encryption algorithm: " + EncryptionAlgorithm.Id);

			random.NextBytes (salt);

			var pbeParameters = PbeUtilities.GenerateAlgorithmParameters (EncryptionAlgorithm.Id, salt, MinIterations);
			var algorithm = new AlgorithmIdentifier (EncryptionAlgorithm, pbeParameters);
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

		AsymmetricKeyParameter DecodePrivateKey (IDataRecord reader, int column, ref byte[] buffer)
		{
			if (reader.IsDBNull (column))
				return null;

			int nread = ReadBinaryBlob (reader, column, ref buffer);

			return DecryptAsymmetricKeyParameter (buffer, nread);
		}

		byte[] EncodePrivateKey (AsymmetricKeyParameter key)
		{
			return key != null ? EncryptAsymmetricKeyParameter (key) : null;
		}

		static EncryptionAlgorithm[] DecodeEncryptionAlgorithms (IDataRecord reader, int column)
		{
			if (reader.IsDBNull (column))
				return null;

			var algorithms = new List<EncryptionAlgorithm> ();
			var values = reader.GetString (column);

			foreach (var token in values.Split (new [] { ',' }, StringSplitOptions.RemoveEmptyEntries)) {
				EncryptionAlgorithm algorithm;

#if NET_3_5
				try {
					algorithm = (EncryptionAlgorithm) Enum.Parse (typeof (EncryptionAlgorithm), token.Trim (), true);
					algorithms.Add (algorithm);
				} catch (ArgumentException) {
				} catch (OverflowException) {
				}
#else
				if (Enum.TryParse (token.Trim (), true, out algorithm))
					algorithms.Add (algorithm);
#endif
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

		X509CertificateRecord LoadCertificateRecord (IDataRecord reader, X509CertificateParser parser, ref byte[] buffer)
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

		X509CrlRecord LoadCrlRecord (IDataRecord reader, X509CrlParser parser, ref byte[] buffer)
		{
			var record = new X509CrlRecord ();

			for (int i = 0; i < reader.FieldCount; i++) {
				switch (reader.GetName (i).ToUpperInvariant ()) {
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
		/// <param name="certificate">The certificate.</param>
		/// <param name="fields">The fields to return.</param>
		protected abstract IDbCommand GetSelectCommand (X509Certificate certificate, X509CertificateRecordFields fields);

		/// <summary>
		/// Gets the database command to select the certificate records for the specified mailbox.
		/// </summary>
		/// <remarks>
		/// Gets the database command to select the certificate records for the specified mailbox.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <param name="now">The date and time for which the certificate should be valid.</param>
		/// <param name="requirePrivateKey"><c>true</c> if the certificate must have a private key.</param>
		/// <param name="fields">The fields to return.</param>
		protected abstract IDbCommand GetSelectCommand (MailboxAddress mailbox, DateTime now, bool requirePrivateKey, X509CertificateRecordFields fields);

		/// <summary>
		/// Gets the database command to select certificate records matching the specified selector.
		/// </summary>
		/// <remarks>
		/// Gets the database command to select certificate records matching the specified selector.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="selector">Selector.</param>
		/// <param name="trustedOnly"><c>true</c> if only trusted certificates should be matched.</param>
		/// <param name="requirePrivateKey"><c>true</c> if the certificate must have a private key.</param>
		/// <param name="fields">The fields to return.</param>
		protected abstract IDbCommand GetSelectCommand (IX509Selector selector, bool trustedOnly, bool requirePrivateKey, X509CertificateRecordFields fields);

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
			const X509CrlRecordFields all = X509CrlRecordFields.Id | X509CrlRecordFields.IsDelta |
				X509CrlRecordFields.IssuerName | X509CrlRecordFields.ThisUpdate |
				X509CrlRecordFields.NextUpdate | X509CrlRecordFields.Crl;

			if (fields == all)
				return new [] { "*" };

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
		/// <param name="issuer">The issuer.</param>
		/// <param name="fields">The fields to return.</param>
		protected abstract IDbCommand GetSelectCommand (X509Name issuer, X509CrlRecordFields fields);

		/// <summary>
		/// Gets the database command to select the record for the specified CRL.
		/// </summary>
		/// <remarks>
		/// Gets the database command to select the record for the specified CRL.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="crl">The X.509 CRL.</param>
		/// <param name="fields">The fields to return.</param>
		protected abstract IDbCommand GetSelectCommand (X509Crl crl, X509CrlRecordFields fields);

		/// <summary>
		/// Gets the database command to select all CRLs in the table.
		/// </summary>
		/// <remarks>
		/// Gets the database command to select all CRLs in the table.
		/// </remarks>
		/// <returns>The database command.</returns>
		protected abstract IDbCommand GetSelectAllCrlsCommand ();

		/// <summary>
		/// Gets the database command to delete the specified certificate record.
		/// </summary>
		/// <remarks>
		/// Gets the database command to delete the specified certificate record.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="record">The certificate record.</param>
		protected abstract IDbCommand GetDeleteCommand (X509CertificateRecord record);

		/// <summary>
		/// Gets the database command to delete the specified CRL record.
		/// </summary>
		/// <remarks>
		/// Gets the database command to delete the specified CRL record.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="record">The record.</param>
		protected abstract IDbCommand GetDeleteCommand (X509CrlRecord record);

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
			case "ID": return record.Id;
			case "BASICCONSTRAINTS": return record.BasicConstraints;
			case "TRUSTED": return record.IsTrusted;
			case "KEYUSAGE": return (int) record.KeyUsage;
			case "NOTBEFORE": return record.NotBefore;
			case "NOTAFTER": return record.NotAfter;
			case "ISSUERNAME": return record.IssuerName;
			case "SERIALNUMBER": return record.SerialNumber;
			case "SUBJECTEMAIL": return record.SubjectEmail != null ? record.SubjectEmail.ToLowerInvariant () : string.Empty;
			case "FINGERPRINT": return record.Fingerprint.ToLowerInvariant ();
			case "ALGORITHMS": return EncodeEncryptionAlgorithms (record.Algorithms);
			case "ALGORITHMSUPDATED": return record.AlgorithmsUpdated;
			case "CERTIFICATE": return record.Certificate.GetEncoded ();
			case "PRIVATEKEY": return EncodePrivateKey (record.PrivateKey);
			default: throw new ArgumentException (string.Format ("Unknown column name: {0}", columnName), "columnName");
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
			case "ID": return record.Id;
			case "DELTA": return record.IsDelta;
			case "ISSUERNAME": return record.IssuerName;
			case "THISUPDATE": return record.ThisUpdate;
			case "NEXTUPDATE": return record.NextUpdate;
			case "CRL": return record.Crl.GetEncoded ();
			default: throw new ArgumentException (string.Format ("Unknown column name: {0}", columnName), "columnName");
			}
		}

		/// <summary>
		/// Gets the database command to insert the specified certificate record.
		/// </summary>
		/// <remarks>
		/// Gets the database command to insert the specified certificate record.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="record">The certificate record.</param>
		protected abstract IDbCommand GetInsertCommand (X509CertificateRecord record);

		/// <summary>
		/// Gets the database command to insert the specified CRL record.
		/// </summary>
		/// <remarks>
		/// Gets the database command to insert the specified CRL record.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="record">The CRL record.</param>
		protected abstract IDbCommand GetInsertCommand (X509CrlRecord record);

		/// <summary>
		/// Gets the database command to update the specified record.
		/// </summary>
		/// <remarks>
		/// Gets the database command to update the specified record.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="record">The certificate record.</param>
		/// <param name="fields">The fields to update.</param>
		protected abstract IDbCommand GetUpdateCommand (X509CertificateRecord record, X509CertificateRecordFields fields);

		/// <summary>
		/// Gets the database command to update the specified CRL record.
		/// </summary>
		/// <remarks>
		/// Gets the database command to update the specified CRL record.
		/// </remarks>
		/// <returns>The database command.</returns>
		/// <param name="record">The CRL record.</param>
		protected abstract IDbCommand GetUpdateCommand (X509CrlRecord record);

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
		/// <remarks>
		/// Searches the database for certificates matching the selector, returning all
		/// matching certificates.
		/// </remarks>
		/// <returns>The matching certificates.</returns>
		/// <param name="selector">The match selector or <c>null</c> to return all certificates.</param>
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
		/// <remarks>
		/// Searches the database for certificate records matching the selector, returning the
		/// private keys for each matching record.
		/// </remarks>
		/// <returns>The matching certificates.</returns>
		/// <param name="selector">The match selector or <c>null</c> to return all private keys.</param>
		public IEnumerable<AsymmetricKeyParameter> FindPrivateKeys (IX509Selector selector)
		{
			using (var command = GetSelectCommand (selector, false, true, PrivateKeyFields)) {
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
		/// <remarks>
		/// Searches the database for certificate records matching the selector, returning all
		/// of the matching records populated with the desired fields.
		/// </remarks>
		/// <returns>The matching certificate records populated with the desired fields.</returns>
		/// <param name="selector">The match selector or <c>null</c> to match all certificates.</param>
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
				throw new ArgumentNullException ("record");

			using (var command = GetInsertCommand (record)) {
				command.ExecuteNonQuery ();
			}
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
				throw new ArgumentNullException ("record");

			using (var command = GetDeleteCommand (record)) {
				command.ExecuteNonQuery ();
			}
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
				throw new ArgumentNullException ("record");

			using (var command = GetUpdateCommand (record, fields)) {
				command.ExecuteNonQuery ();
			}
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
				throw new ArgumentNullException ("record");

			using (var command = GetInsertCommand (record)) {
				command.ExecuteNonQuery ();
			}
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
				throw new ArgumentNullException ("record");

			using (var command = GetDeleteCommand (record)) {
				command.ExecuteNonQuery ();
			}
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
				throw new ArgumentNullException ("record");

			using (var command = GetUpdateCommand (record)) {
				command.ExecuteNonQuery ();
			}
		}

		/// <summary>
		/// Gets a certificate revocation list store.
		/// </summary>
		/// <remarks>
		/// Gets a certificate revocation list store.
		/// </remarks>
		/// <returns>A certificate recovation list store.</returns>
		public IX509Store GetCrlStore ()
		{
			var crls = new List<X509Crl> ();

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
		/// Gets a collection of matching certificates matching the specified selector.
		/// </summary>
		/// <remarks>
		/// Gets a collection of matching certificates matching the specified selector.
		/// </remarks>
		/// <returns>The matching certificates.</returns>
		/// <param name="selector">The match criteria.</param>
		ICollection IX509Store.GetMatches (IX509Selector selector)
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
			for (int i = 0; i < passwd.Length; i++)
				passwd[i] = '\0';
		}

		/// <summary>
		/// Releases all resource used by the <see cref="MimeKit.Cryptography.X509CertificateDatabase"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose()"/> when you are finished using the
		/// <see cref="MimeKit.Cryptography.X509CertificateDatabase"/>. The <see cref="Dispose()"/> method leaves the
		/// <see cref="MimeKit.Cryptography.X509CertificateDatabase"/> in an unusable state. After calling
		/// <see cref="Dispose()"/>, you must release all references to the
		/// <see cref="MimeKit.Cryptography.X509CertificateDatabase"/> so the garbage collector can reclaim the memory that
		/// the <see cref="MimeKit.Cryptography.X509CertificateDatabase"/> was occupying.</remarks>
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		#endregion
	}
}

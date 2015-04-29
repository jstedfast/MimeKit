//
// DefaultSecureMimeContext.cs
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
using System.Linq;
using System.Collections.Generic;

using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.Smime;
using Org.BouncyCastle.X509.Store;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A default <see cref="SecureMimeContext"/> implementation that uses
	/// an SQLite database as a certificate and private key store.
	/// </summary>
	/// <remarks>
	/// The default S/MIME context is designed to be usable on any platform
	/// where there exists a .NET runtime by storing certificates, CRLs, and
	/// (encrypted) private keys in a SQLite database.
	/// </remarks>
	public class DefaultSecureMimeContext : SecureMimeContext
	{
		const X509CertificateRecordFields CmsRecipientFields = X509CertificateRecordFields.Algorithms | X509CertificateRecordFields.Certificate;
		const X509CertificateRecordFields CmsSignerFields = X509CertificateRecordFields.Certificate | X509CertificateRecordFields.PrivateKey;
		const X509CertificateRecordFields AlgorithmFields = X509CertificateRecordFields.Id | X509CertificateRecordFields.Algorithms | X509CertificateRecordFields.AlgorithmsUpdated;
		const X509CertificateRecordFields ImportPkcs12Fields = AlgorithmFields | X509CertificateRecordFields.Trusted | X509CertificateRecordFields.PrivateKey;

		/// <summary>
		/// The default database path for certificates, private keys and CRLs.
		/// </summary>
		/// <remarks>
		/// <para>On Microsoft Windows-based systems, this path will be something like <c>C:\Users\UserName\AppData\Roaming\mimekit\smime.db</c>.</para>
		/// <para>On Unix systems such as Linux and Mac OS X, this path will be <c>~/.mimekit/smime.db</c>.</para>
		/// </remarks>
		public static readonly string DefaultDatabasePath;

		readonly IX509CertificateDatabase dbase;

		static DefaultSecureMimeContext ()
		{
			string path;

			if (Path.DirectorySeparatorChar == '\\') {
				var appData = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
				path = Path.Combine (appData, "Roaming\\mimekit");
			} else {
				var home = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
				path = Path.Combine (home, ".mimekit");
			}

			if (!Directory.Exists (path))
				Directory.CreateDirectory (path);

			DefaultDatabasePath = Path.Combine (path, "smime.db");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.DefaultSecureMimeContext"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Allows the program to specify its own location for the SQLite database. If the file does not exist,
		/// it will be created and the necessary tables and indexes will be constructed.</para>
		/// <para>Requires linking with Mono.Data.Sqlite.</para>
		/// </remarks>
		/// <param name="fileName">The path to the SQLite database.</param>
		/// <param name="password">The password used for encrypting and decrypting the private keys.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The specified file path is empty.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// Mono.Data.Sqlite is not available.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An error occurred reading the file.
		/// </exception>
		public DefaultSecureMimeContext (string fileName, string password)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			if (password == null)
				throw new ArgumentNullException ("password");

			var dir = Path.GetDirectoryName (fileName);
			var exists = File.Exists (fileName);

			if (!string.IsNullOrEmpty (dir) && !Directory.Exists (dir))
				Directory.CreateDirectory (dir);

			if (SqliteCertificateDatabase.IsAvailable)
				dbase = new SqliteCertificateDatabase (fileName, password);
			else
				throw new NotSupportedException ("Mono.Data.Sqlite is not available.");

			if (!exists) {
				// TODO: initialize our dbase with some root CA certificates.
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.DefaultSecureMimeContext"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Allows the program to specify its own password for the default database.</para>
		/// <para>Requires linking with Mono.Data.Sqlite.</para>
		/// </remarks>
		/// <param name="password">The password used for encrypting and decrypting the private keys.</param>
		/// <exception cref="System.NotImplementedException">
		/// Mono.Data.Sqlite is not available.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the database at the default location.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An error occurred reading the database at the default location.
		/// </exception>
		public DefaultSecureMimeContext (string password) : this (DefaultDatabasePath, password)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.DefaultSecureMimeContext"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Not recommended for production use as the password to unlock the private keys is hard-coded.</para>
		/// <para>Requires linking with Mono.Data.Sqlite.</para>
		/// </remarks>
		/// <exception cref="System.NotImplementedException">
		/// Mono.Data.Sqlite is not available.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the database at the default location.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An error occurred reading the database at the default location.
		/// </exception>
		public DefaultSecureMimeContext () : this (DefaultDatabasePath, "no.secret")
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.DefaultSecureMimeContext"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is useful for supplying a custom <see cref="IX509CertificateDatabase"/>.
		/// </remarks>
		/// <param name="database">The certificate database.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="database"/> is <c>null</c>.
		/// </exception>
		public DefaultSecureMimeContext (IX509CertificateDatabase database)
		{
			if (database == null)
				throw new ArgumentNullException ("database");

			dbase = database;
		}

		#region implemented abstract members of SecureMimeContext

		/// <summary>
		/// Gets the X.509 certificate matching the specified selector.
		/// </summary>
		/// <remarks>
		/// Gets the first certificate that matches the specified selector.
		/// </remarks>
		/// <returns>The certificate on success; otherwise <c>null</c>.</returns>
		/// <param name="selector">The search criteria for the certificate.</param>
		protected override X509Certificate GetCertificate (IX509Selector selector)
		{
			return dbase.FindCertificates (selector).FirstOrDefault ();
		}

		/// <summary>
		/// Gets the private key for the certificate matching the specified selector.
		/// </summary>
		/// <remarks>
		/// Gets the private key for the first certificate that matches the specified selector.
		/// </remarks>
		/// <returns>The private key on success; otherwise <c>null</c>.</returns>
		/// <param name="selector">The search criteria for the private key.</param>
		protected override AsymmetricKeyParameter GetPrivateKey (IX509Selector selector)
		{
			return dbase.FindPrivateKeys (selector).FirstOrDefault ();
		}

		/// <summary>
		/// Gets the trusted anchors.
		/// </summary>
		/// <remarks>
		/// A trusted anchor is a trusted root-level X.509 certificate,
		/// generally issued by a Certificate Authority (CA).
		/// </remarks>
		/// <returns>The trusted anchors.</returns>
		protected override Org.BouncyCastle.Utilities.Collections.HashSet GetTrustedAnchors ()
		{
			var anchors = new Org.BouncyCastle.Utilities.Collections.HashSet ();
			var selector = new X509CertStoreSelector ();
			var keyUsage = new bool[9];

			keyUsage[(int) X509KeyUsageBits.KeyCertSign] = true;
			selector.KeyUsage = keyUsage;

			foreach (var record in dbase.Find (selector, true, X509CertificateRecordFields.Certificate)) {
				anchors.Add (new TrustAnchor (record.Certificate, null));
			}

			return anchors;
		}

		/// <summary>
		/// Gets the intermediate certificates.
		/// </summary>
		/// <remarks>
		/// An intermediate certificate is any certificate that exists between the root
		/// certificate issued by a Certificate Authority (CA) and the certificate at
		/// the end of the chain.
		/// </remarks>
		/// <returns>The intermediate certificates.</returns>
		protected override IX509Store GetIntermediateCertificates ()
		{
			return dbase;
		}

		/// <summary>
		/// Gets the certificate revocation lists.
		/// </summary>
		/// <remarks>
		/// A Certificate Revocation List (CRL) is a list of certificate serial numbers issued
		/// by a particular Certificate Authority (CA) that have been revoked, either by the CA
		/// itself or by the owner of the revoked certificate.
		/// </remarks>
		/// <returns>The certificate revocation lists.</returns>
		protected override IX509Store GetCertificateRevocationLists ()
		{
			return dbase.GetCrlStore ();
		}

		static EncryptionAlgorithm[] DecodeEncryptionAlgorithms (byte[] rawData)
		{
			using (var memory = new MemoryStream (rawData, false)) {
				using (var asn1 = new Asn1InputStream (memory)) {
					var algorithms = new List<EncryptionAlgorithm> ();
					var sequence = asn1.ReadObject () as Asn1Sequence;

					if (sequence == null)
						return null;

					for (int i = 0; i < sequence.Count; i++) {
						var identifier = AlgorithmIdentifier.GetInstance (sequence[i]);
						EncryptionAlgorithm algorithm;

						if (TryGetEncryptionAlgorithm (identifier, out algorithm))
							algorithms.Add (algorithm);
					}

					return algorithms.ToArray ();
				}
			}
		}

		/// <summary>
		/// Gets the <see cref="CmsRecipient"/> for the specified mailbox.
		/// </summary>
		/// <remarks>
		/// <para>Constructs a <see cref="CmsRecipient"/> with the appropriate certificate and
		/// <see cref="CmsRecipient.EncryptionAlgorithms"/> for the specified mailbox.</para>
		/// <para>If the mailbox is a <see cref="SecureMailboxAddress"/>, the
		/// <see cref="SecureMailboxAddress.Fingerprint"/> property will be used instead of
		/// the mailbox address for database lookups.</para>
		/// </remarks>
		/// <returns>A <see cref="CmsRecipient"/>.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		protected override CmsRecipient GetCmsRecipient (MailboxAddress mailbox)
		{
			foreach (var record in dbase.Find (mailbox, DateTime.Now, false, CmsRecipientFields)) {
				if (record.KeyUsage != 0 && (record.KeyUsage & X509KeyUsageFlags.KeyEncipherment) == 0)
					continue;

				var recipient = new CmsRecipient (record.Certificate);
				if (record.Algorithms == null) {
					var capabilities = record.Certificate.GetExtensionValue (SmimeAttributes.SmimeCapabilities);
					if (capabilities != null) {
						var algorithms = DecodeEncryptionAlgorithms (capabilities.GetOctets ());

						if (algorithms != null)
							recipient.EncryptionAlgorithms = algorithms;
					}
				} else {
					recipient.EncryptionAlgorithms = record.Algorithms;
				}

				return recipient;
			}

			throw new CertificateNotFoundException (mailbox, "A valid certificate could not be found.");
		}

		/// <summary>
		/// Gets the <see cref="CmsSigner"/> for the specified mailbox.
		/// </summary>
		/// <remarks>
		/// <para>Constructs a <see cref="CmsSigner"/> with the appropriate signing certificate
		/// for the specified mailbox.</para>
		/// <para>If the mailbox is a <see cref="SecureMailboxAddress"/>, the
		/// <see cref="SecureMailboxAddress.Fingerprint"/> property will be used instead of
		/// the mailbox address for database lookups.</para>
		/// </remarks>
		/// <returns>A <see cref="CmsSigner"/>.</returns>
		/// <param name="mailbox">The mailbox.</param>
		/// <param name="digestAlgo">The preferred digest algorithm.</param>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		protected override CmsSigner GetCmsSigner (MailboxAddress mailbox, DigestAlgorithm digestAlgo)
		{
			foreach (var record in dbase.Find (mailbox, DateTime.Now, true, CmsSignerFields)) {
				if (record.KeyUsage != X509KeyUsageFlags.None && (record.KeyUsage & SecureMimeContext.DigitalSignatureKeyUsageFlags) == 0)
					continue;

				var signer = new CmsSigner (record.Certificate, record.PrivateKey);
				signer.DigestAlgorithm = digestAlgo;

				return signer;
			}

			throw new CertificateNotFoundException (mailbox, "A valid signing certificate could not be found.");
		}

		/// <summary>
		/// Updates the known S/MIME capabilities of the client used by the recipient that owns the specified certificate.
		/// </summary>
		/// <remarks>
		/// Updates the known S/MIME capabilities of the client used by the recipient that owns the specified certificate.
		/// </remarks>
		/// <param name="certificate">The certificate.</param>
		/// <param name="algorithms">The encryption algorithm capabilities of the client (in preferred order).</param>
		/// <param name="timestamp">The timestamp.</param>
		protected override void UpdateSecureMimeCapabilities (X509Certificate certificate, EncryptionAlgorithm[] algorithms, DateTime timestamp)
		{
			X509CertificateRecord record;

			if ((record = dbase.Find (certificate, AlgorithmFields)) == null) {
				record = new X509CertificateRecord (certificate);
				record.AlgorithmsUpdated = timestamp;
				record.Algorithms = algorithms;

				dbase.Add (record);
			} else if (timestamp > record.AlgorithmsUpdated) {
				record.AlgorithmsUpdated = timestamp;
				record.Algorithms = algorithms;

				dbase.Update (record, AlgorithmFields);
			}
		}

		/// <summary>
		/// Imports a certificate.
		/// </summary>
		/// <remarks>
		/// Imports the specified certificate into the database.
		/// </remarks>
		/// <param name="certificate">The certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public override void Import (X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			if (dbase.Find (certificate, X509CertificateRecordFields.Id) == null)
				dbase.Add (new X509CertificateRecord (certificate));
		}

		/// <summary>
		/// Imports a certificate revocation list.
		/// </summary>
		/// <remarks>
		/// Imports the specified certificate revocation list.
		/// </remarks>
		/// <param name="crl">The certificate revocation list.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="crl"/> is <c>null</c>.
		/// </exception>
		public override void Import (X509Crl crl)
		{
			if (crl == null)
				throw new ArgumentNullException ("crl");

			// check for an exact match...
			if (dbase.Find (crl, X509CrlRecordFields.Id) != null)
				return;

			const X509CrlRecordFields fields = ~X509CrlRecordFields.Crl;
			var obsolete = new List<X509CrlRecord> ();
			var delta = crl.IsDelta ();

			// scan over our list of CRLs by the same issuer to check if this CRL obsoletes any
			// older CRLs or if there are any newer CRLs that obsolete that obsolete this one.
			foreach (var record in dbase.Find (crl.IssuerDN, fields)) {
				if (!record.IsDelta && record.ThisUpdate >= crl.ThisUpdate) {
					// we have a complete CRL that obsoletes this CRL
					return;
				}

				if (!delta)
					obsolete.Add (record);
			}

			// remove any obsoleted CRLs
			foreach (var record in obsolete)
				dbase.Remove (record);

			dbase.Add (new X509CrlRecord (crl));
		}

		/// <summary>
		/// Imports certificates and keys from a pkcs12-encoded stream.
		/// </summary>
		/// <remarks>
		/// Imports all of the certificates and keys from the pkcs12-encoded stream.
		/// </remarks>
		/// <param name="stream">The raw certificate and key data.</param>
		/// <param name="password">The password to unlock the data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override void Import (Stream stream, string password)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			if (password == null)
				throw new ArgumentNullException ("password");

			var pkcs12 = new Pkcs12Store (stream, password.ToCharArray ());
			var enabledAlgorithms = EnabledEncryptionAlgorithms;
			X509CertificateRecord record;

			foreach (string alias in pkcs12.Aliases) {
				if (pkcs12.IsKeyEntry (alias)) {
					var chain = pkcs12.GetCertificateChain (alias);
					var entry = pkcs12.GetKey (alias);
					int startIndex = 0;

					if (entry.Key.IsPrivate) {
						if ((record = dbase.Find (chain[0].Certificate, ImportPkcs12Fields)) == null) {
							record = new X509CertificateRecord (chain[0].Certificate, entry.Key);
							record.AlgorithmsUpdated = DateTime.Now;
							record.Algorithms = enabledAlgorithms;
							record.IsTrusted = true;
							dbase.Add (record);
						} else {
							record.AlgorithmsUpdated = DateTime.Now;
							record.Algorithms = enabledAlgorithms;
							if (record.PrivateKey == null)
								record.PrivateKey = entry.Key;
							record.IsTrusted = true;
							dbase.Update (record, ImportPkcs12Fields);
						}

						startIndex = 1;
					}

					for (int i = startIndex; i < chain.Length; i++) {
						if ((record = dbase.Find (chain[i].Certificate, X509CertificateRecordFields.Id)) == null)
							dbase.Add (new X509CertificateRecord (chain[i].Certificate));
					}
				} else if (pkcs12.IsCertificateEntry (alias)) {
					var entry = pkcs12.GetCertificate (alias);

					if ((record = dbase.Find (entry.Certificate, X509CertificateRecordFields.Id)) == null)
						dbase.Add (new X509CertificateRecord (entry.Certificate));
				}
			}
		}

		#endregion

		/// <summary>
		/// Imports a DER-encoded certificate stream.
		/// </summary>
		/// <remarks>
		/// Imports all of the certificates in the DER-encoded stream.
		/// </remarks>
		/// <param name="stream">The raw certificate(s).</param>
		/// <param name="trusted"><c>true</c> if the certificates are trusted.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		public void Import (Stream stream, bool trusted)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			var parser = new X509CertificateParser ();

			foreach (X509Certificate certificate in parser.ReadCertificates (stream)) {
				if (dbase.Find (certificate, X509CertificateRecordFields.Id) != null)
					continue;

				var record = new X509CertificateRecord (certificate);
				record.IsTrusted = trusted;
				dbase.Add (record);
			}
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="DefaultSecureMimeContext"/> and
		/// optionally releases the managed resources.
		/// </summary>
		/// <remarks>
		/// Releases the unmanaged resources used by the <see cref="DefaultSecureMimeContext"/> and
		/// optionally releases the managed resources.
		/// </remarks>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
		/// <c>false</c> to release only the unmanaged resources.</param>
		protected override void Dispose (bool disposing)
		{
			dbase.Dispose ();

			base.Dispose (disposing);
		}
	}
}

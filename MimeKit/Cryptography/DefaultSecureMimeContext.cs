//
// DefaultSecureMimeContext.cs
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509.Store;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Collections;

using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A default <see cref="SecureMimeContext"/> implementation that uses
	/// an SQLite database as a certificate and private key store.
	/// </summary>
	/// <remarks>
	/// The default S/MIME context is designed to be usable on any platform
	/// where there exists a .NET runtime by storing certificates, CRLs, and
	/// (encrypted) private keys in a SQL database.
	/// </remarks>
	public class DefaultSecureMimeContext : BouncyCastleSecureMimeContext
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
				var compatPath = Path.Combine (appData, "Roaming\\mimekit");
				path = Path.Combine (appData, "mimekit");

				if (!Directory.Exists (path) && Directory.Exists (compatPath)) {
					try {
						Directory.Move (compatPath, path);
					} catch {
						path = compatPath;
					}
				}
			} else {
				var home = Environment.GetFolderPath (Environment.SpecialFolder.UserProfile);
				path = Path.Combine (home, ".mimekit");
			}

			DefaultDatabasePath = Path.Combine (path, "smime.db");
		}

		static void CheckIsAvailable ()
		{
			if (!SqliteCertificateDatabase.IsAvailable)
				throw new NotSupportedException ("SQLite is not available. Install the System.Data.SQLite nuget package.");
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="DefaultSecureMimeContext"/> class.
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
		public DefaultSecureMimeContext (string fileName, string password) : this (fileName, password, new SecureRandom ())
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="DefaultSecureMimeContext"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Allows the program to specify its own location for the SQLite database. If the file does not exist,
		/// it will be created and the necessary tables and indexes will be constructed.</para>
		/// <para>Requires linking with Mono.Data.Sqlite.</para>
		/// </remarks>
		/// <param name="fileName">The path to the SQLite database.</param>
		/// <param name="password">The password used for encrypting and decrypting the private keys.</param>
		/// <param name="random">A secure pseudo-random number generator.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="random"/> is <c>null</c>.</para>
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
		public DefaultSecureMimeContext (string fileName, string password, SecureRandom random) : base (random)
		{
			if (fileName == null)
				throw new ArgumentNullException (nameof (fileName));

			if (password == null)
				throw new ArgumentNullException (nameof (password));

			CheckIsAvailable ();

			var dir = Path.GetDirectoryName (fileName);
			var exists = File.Exists (fileName);

			if (!string.IsNullOrEmpty (dir) && !Directory.Exists (dir))
				Directory.CreateDirectory (dir);

			dbase = new SqliteCertificateDatabase (fileName, password, random);

			if (!exists) {
				// TODO: initialize our dbase with some root CA certificates.
			}
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="DefaultSecureMimeContext"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Allows the program to specify its own password for the default database.</para>
		/// <para>Requires linking with Mono.Data.Sqlite.</para>
		/// </remarks>
		/// <param name="password">The password used for encrypting and decrypting the private keys.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="password"/> is <c>null</c>.
		/// </exception>
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
		/// Initialize a new instance of the <see cref="DefaultSecureMimeContext"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Allows the program to specify its own password for the default database.</para>
		/// <para>Requires linking with Mono.Data.Sqlite.</para>
		/// </remarks>
		/// <param name="password">The password used for encrypting and decrypting the private keys.</param>
		/// <param name="random">A secure pseudo-random number generator.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="random"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.NotImplementedException">
		/// Mono.Data.Sqlite is not available.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the database at the default location.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An error occurred reading the database at the default location.
		/// </exception>
		public DefaultSecureMimeContext (string password, SecureRandom random) : this (DefaultDatabasePath, password, random)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="DefaultSecureMimeContext"/> class.
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
		/// Initialize a new instance of the <see cref="DefaultSecureMimeContext"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is useful for supplying a custom <see cref="IX509CertificateDatabase"/>.
		/// </remarks>
		/// <param name="database">The certificate database.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="database"/> is <c>null</c>.
		/// </exception>
		public DefaultSecureMimeContext (IX509CertificateDatabase database) : this (database, new SecureRandom ())
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="DefaultSecureMimeContext"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is useful for supplying a custom <see cref="IX509CertificateDatabase"/>.
		/// </remarks>
		/// <param name="database">The certificate database.</param>
		/// <param name="random">A secure pseudo-random number generator.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="database"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="random"/> is <c>null</c>.</para>
		/// </exception>
		public DefaultSecureMimeContext (IX509CertificateDatabase database, SecureRandom random) : base (random)
		{
			if (database == null)
				throw new ArgumentNullException (nameof (database));

			dbase = database;
		}

		/// <summary>
		/// Check whether or not a particular mailbox address can be used for signing.
		/// </summary>
		/// <remarks>
		/// Checks whether or not as particular mailbocx address can be used for signing.
		/// </remarks>
		/// <returns><c>true</c> if the mailbox address can be used for signing; otherwise, <c>false</c>.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="signer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public override bool CanSign (MailboxAddress signer, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			foreach (var record in dbase.Find (signer, DateTime.UtcNow, true, CmsSignerFields)) {
				if (record.KeyUsage != X509KeyUsageFlags.None && (record.KeyUsage & SecureMimeContext.DigitalSignatureKeyUsageFlags) == 0)
					continue;

				return true;
			}

			return false;
		}

		/// <summary>
		/// Check whether or not the cryptography context can encrypt to a particular recipient.
		/// </summary>
		/// <remarks>
		/// Checks whether or not the cryptography context can be used to encrypt to a particular recipient.
		/// </remarks>
		/// <returns><c>true</c> if the cryptography context can be used to encrypt to the designated recipient; otherwise, <c>false</c>.</returns>
		/// <param name="mailbox">The recipient's mailbox address.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailbox"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public override bool CanEncrypt (MailboxAddress mailbox, CancellationToken cancellationToken = default)
		{
			if (mailbox == null)
				throw new ArgumentNullException (nameof (mailbox));

			foreach (var record in dbase.Find (mailbox, DateTime.UtcNow, false, CmsRecipientFields)) {
				if (record.KeyUsage != 0 && (record.KeyUsage & X509KeyUsageFlags.KeyEncipherment) == 0)
					continue;

				return true;
			}

			return false;
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
		protected override X509Certificate GetCertificate (ISelector<X509Certificate> selector)
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
		protected override AsymmetricKeyParameter GetPrivateKey (ISelector<X509Certificate> selector)
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
		protected override ISet<TrustAnchor> GetTrustedAnchors ()
		{
			var anchors = new HashSet<TrustAnchor> ();
			var selector = new X509CertStoreSelector ();
			var keyUsage = new bool[9];

			keyUsage[(int) X509KeyUsageBits.KeyCertSign] = true;
			selector.KeyUsage = keyUsage;

			foreach (var record in dbase.Find (selector, true, X509CertificateRecordFields.Certificate))
				anchors.Add (new TrustAnchor (record.Certificate, null));

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
		protected override IStore<X509Certificate> GetIntermediateCertificates ()
		{
			//var intermediates = new X509CertificateStore ();
			//var selector = new X509CertStoreSelector ();
			//var keyUsage = new bool[9];

			//keyUsage[(int) X509KeyUsageBits.KeyCertSign] = true;
			//selector.KeyUsage = keyUsage;

			//foreach (var record in dbase.Find (selector, false, X509CertificateRecordFields.Certificate)) {
			//	if (!record.Certificate.IsSelfSigned ())
			//		intermediates.Add (record.Certificate);
			//}

			//return intermediates;
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
		protected override IStore<X509Crl> GetCertificateRevocationLists ()
		{
			return dbase.GetCrlStore ();
		}

		/// <summary>
		/// Get the date &amp; time for the next scheduled certificate revocation list update for the specified issuer.
		/// </summary>
		/// <remarks>
		/// Gets the date &amp; time for the next scheduled certificate revocation list update for the specified issuer.
		/// </remarks>
		/// <returns>The date &amp; time for the next update (in UTC).</returns>
		/// <param name="issuer">The issuer.</param>
		protected override DateTime GetNextCertificateRevocationListUpdate (X509Name issuer)
		{
			var nextUpdate = DateTime.MinValue.ToUniversalTime ();

			foreach (var record in dbase.Find (issuer, X509CrlRecordFields.NextUpdate))
				nextUpdate = record.NextUpdate > nextUpdate ? record.NextUpdate : nextUpdate;

			return nextUpdate;
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
		/// <param name="mailbox">The recipient's mailbox address.</param>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		protected override CmsRecipient GetCmsRecipient (MailboxAddress mailbox)
		{
			foreach (var record in dbase.Find (mailbox, DateTime.UtcNow, false, CmsRecipientFields)) {
				if (record.KeyUsage != 0 && (record.KeyUsage & X509KeyUsageFlags.KeyEncipherment) == 0)
					continue;

				var recipient = new CmsRecipient (record.Certificate);

				if (record.Algorithms != null)
					recipient.EncryptionAlgorithms = record.Algorithms;

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
		/// <param name="mailbox">The signer's mailbox address.</param>
		/// <param name="digestAlgo">The preferred digest algorithm.</param>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate for the specified <paramref name="mailbox"/> could not be found.
		/// </exception>
		protected override CmsSigner GetCmsSigner (MailboxAddress mailbox, DigestAlgorithm digestAlgo)
		{
			AsymmetricKeyParameter privateKey = null;
			X509Certificate certificate = null;

			foreach (var record in dbase.Find (mailbox, DateTime.UtcNow, true, CmsSignerFields)) {
				if (record.KeyUsage != X509KeyUsageFlags.None && (record.KeyUsage & DigitalSignatureKeyUsageFlags) == 0)
					continue;

				certificate = record.Certificate;
				privateKey = record.PrivateKey;
				break;
			}

			if (certificate != null && privateKey != null) {
				var signer = new CmsSigner (BuildCertificateChain (certificate), privateKey) {
					DigestAlgorithm = digestAlgo
				};

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
		/// <param name="timestamp">The timestamp in coordinated universal time (UTC).</param>
		protected override void UpdateSecureMimeCapabilities (X509Certificate certificate, EncryptionAlgorithm[] algorithms, DateTime timestamp)
		{
			X509CertificateRecord record;

			if ((record = dbase.Find (certificate, AlgorithmFields)) == null) {
				record = new X509CertificateRecord (certificate) {
					AlgorithmsUpdated = timestamp,
					Algorithms = algorithms
				};

				dbase.Add (record);
			} else if (timestamp > record.AlgorithmsUpdated) {
				record.AlgorithmsUpdated = timestamp;
				record.Algorithms = algorithms;

				dbase.Update (record, AlgorithmFields);
			}
		}

		/// <summary>
		/// Import a certificate.
		/// </summary>
		/// <remarks>
		/// Imports the specified certificate into the database.
		/// </remarks>
		/// <param name="certificate">The certificate.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public override void Import (X509Certificate certificate, CancellationToken cancellationToken = default)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			cancellationToken.ThrowIfCancellationRequested ();

			if (dbase.Find (certificate, X509CertificateRecordFields.Id) == null)
				dbase.Add (new X509CertificateRecord (certificate));
		}

		/// <summary>
		/// Import a certificate.
		/// </summary>
		/// <remarks>
		/// Imports a certificate.
		/// </remarks>
		/// <param name="certificate">The certificate.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		public override void Import (X509Certificate2 certificate, CancellationToken cancellationToken = default)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			cancellationToken.ThrowIfCancellationRequested ();

			var cert = certificate.AsBouncyCastleCertificate ();

			if (certificate.HasPrivateKey) {
				var privateKey = certificate.GetPrivateKeyAsAsymmetricKeyParameter ();
				X509CertificateRecord record;

				if ((record = dbase.Find (cert, ImportPkcs12Fields)) == null) {
					if (privateKey != null)
						record = new X509CertificateRecord (cert, privateKey);
					else
						record = new X509CertificateRecord (cert);

					record.Algorithms = EnabledEncryptionAlgorithms;
					record.AlgorithmsUpdated = DateTime.UtcNow;
					record.IsTrusted = privateKey != null;
					dbase.Add (record);
				} else {
					record.AlgorithmsUpdated = DateTime.UtcNow;
					record.Algorithms = EnabledEncryptionAlgorithms;
					record.PrivateKey ??= privateKey;
					record.IsTrusted = record.IsTrusted || privateKey != null;
					dbase.Update (record, ImportPkcs12Fields);
				}
			} else {
				Import (cert, true, cancellationToken);
			}
		}

		/// <summary>
		/// Imports a certificate revocation list.
		/// </summary>
		/// <remarks>
		/// Imports the specified certificate revocation list.
		/// </remarks>
		/// <param name="crl">The certificate revocation list.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="crl"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public override void Import (X509Crl crl, CancellationToken cancellationToken = default)
		{
			if (crl == null)
				throw new ArgumentNullException (nameof (crl));

			cancellationToken.ThrowIfCancellationRequested ();

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
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override void Import (Stream stream, string password, CancellationToken cancellationToken = default)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			if (password == null)
				throw new ArgumentNullException (nameof (password));

			cancellationToken.ThrowIfCancellationRequested ();

			var pkcs12 = new Pkcs12StoreBuilder ().Build ();
			pkcs12.Load (stream, password.ToCharArray ());
			var enabledAlgorithms = EnabledEncryptionAlgorithms;
			X509CertificateRecord record;

			foreach (string alias in pkcs12.Aliases) {
				if (pkcs12.IsKeyEntry (alias)) {
					var chain = pkcs12.GetCertificateChain (alias);
					var entry = pkcs12.GetKey (alias);
					int startIndex = 0;

					if (entry.Key.IsPrivate) {
						if ((record = dbase.Find (chain[0].Certificate, ImportPkcs12Fields)) == null) {
							record = new X509CertificateRecord (chain[0].Certificate, entry.Key) {
								AlgorithmsUpdated = DateTime.UtcNow,
								Algorithms = enabledAlgorithms,
								IsTrusted = true
							};
							dbase.Add (record);
						} else {
							record.AlgorithmsUpdated = DateTime.UtcNow;
							record.Algorithms = enabledAlgorithms;
							record.PrivateKey ??= entry.Key;
							record.IsTrusted = true;
							dbase.Update (record, ImportPkcs12Fields);
						}

						startIndex = 1;
					}

					for (int i = startIndex; i < chain.Length; i++)
						Import (chain[i].Certificate, true, cancellationToken);
				} else if (pkcs12.IsCertificateEntry (alias)) {
					var entry = pkcs12.GetCertificate (alias);

					Import (entry.Certificate, true, cancellationToken);
				}
			}
		}

		/// <summary>
		/// Asynchronously imports certificates and keys from a pkcs12-encoded stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously imports all of the certificates and keys from the pkcs12-encoded stream.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="stream">The raw certificate and key data.</param>
		/// <param name="password">The password to unlock the data.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public override Task ImportAsync (Stream stream, string password, CancellationToken cancellationToken = default)
		{
			Import (stream, password, cancellationToken);
			return Task.FromResult (true);
		}

		#endregion

		/// <summary>
		/// Import a certificate.
		/// </summary>
		/// <remarks>
		/// <para>Imports the certificate.</para>
		/// <para>If the certificate already exists in the database and <paramref name="trusted"/> is <c>true</c>,
		/// then the IsTrusted state is updated otherwise the certificate is added to the database with the
		/// specified trust.</para>
		/// </remarks>
		/// <param name="certificate">The certificate.</param>
		/// <param name="trusted"><c>true</c> if the certificate is trusted; otherwise, <c>false</c>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public void Import (X509Certificate certificate, bool trusted, CancellationToken cancellationToken = default)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			X509CertificateRecord record;

			if ((record = dbase.Find (certificate, X509CertificateRecordFields.Id | X509CertificateRecordFields.Trusted)) != null) {
				if (trusted && !record.IsTrusted) {
					record.IsTrusted = trusted;
					dbase.Update (record, X509CertificateRecordFields.Trusted);
				}

				return;
			}

			record = new X509CertificateRecord (certificate) {
				IsTrusted = trusted
			};
			dbase.Add (record);
		}

		/// <summary>
		/// Asynchronously import a certificate.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously imports the certificate.</para>
		/// <para>If the certificate already exists in the database and <paramref name="trusted"/> is <c>true</c>,
		/// then the IsTrusted state is updated otherwise the certificate is added to the database with the
		/// specified trust.</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="certificate">The certificate.</param>
		/// <param name="trusted"><c>true</c> if the certificate is trusted; otherwise, <c>false</c>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public Task ImportAsync (X509Certificate certificate, bool trusted, CancellationToken cancellationToken = default)
		{
			// TODO: Add Async APIs to IX509CertificateDatabase
			Import (certificate, trusted, cancellationToken);
			return Task.FromResult (true);
		}

		/// <summary>
		/// Import a DER-encoded certificate stream.
		/// </summary>
		/// <remarks>
		/// Imports the certificate(s).
		/// </remarks>
		/// <param name="stream">The raw certificate(s).</param>
		/// <param name="trusted"><c>true</c> if the certificates are trusted; othewrwise, <c>false</c>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public void Import (Stream stream, bool trusted, CancellationToken cancellationToken = default)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			cancellationToken.ThrowIfCancellationRequested ();

			var parser = new X509CertificateParser ();

			foreach (X509Certificate certificate in parser.ReadCertificates (stream))
				Import (certificate, trusted, cancellationToken);
		}

		/// <summary>
		/// Asynchronously import a DER-encoded certificate stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously imports the certificate(s).
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="stream">The raw certificate(s).</param>
		/// <param name="trusted"><c>true</c> if the certificates are trusted; othewrwise, <c>false</c>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public Task ImportAsync (Stream stream, bool trusted, CancellationToken cancellationToken = default)
		{
			// TODO: Add Async APIs to IX509CertificateDatabase
			Import (stream, trusted, cancellationToken);
			return Task.FromResult (true);
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

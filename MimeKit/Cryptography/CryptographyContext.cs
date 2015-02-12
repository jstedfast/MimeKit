//
// CryptographyContext.cs
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
using System.Reflection;
using System.Collections.Generic;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An abstract cryptography context.
	/// </summary>
	/// <remarks>
	/// Generally speaking, applications should not use a <see cref="CryptographyContext"/>
	/// directly, but rather via higher level APIs such as <see cref="MultipartSigned"/>,
	/// <see cref="MultipartEncrypted"/> and <see cref="ApplicationPkcs7Mime"/>.
	/// </remarks>
	public abstract class CryptographyContext : IDisposable
	{
		static ConstructorInfo SecureMimeContextConstructor;
		static ConstructorInfo OpenPgpContextConstructor;
		static readonly object mutex = new object ();

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.CryptographyContext"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="CryptographyContext"/>.
		/// </remarks>
		protected CryptographyContext ()
		{
		}

		/// <summary>
		/// Gets the signature protocol.
		/// </summary>
		/// <remarks>
		/// <para>The signature protocol is used by <see cref="MultipartSigned"/>
		/// in order to determine what the protocol parameter of the Content-Type
		/// header should be.</para>
		/// </remarks>
		/// <value>The signature protocol.</value>
		public abstract string SignatureProtocol { get; }

		/// <summary>
		/// Gets the encryption protocol.
		/// </summary>
		/// <remarks>
		/// <para>The encryption protocol is used by <see cref="MultipartEncrypted"/>
		/// in order to determine what the protocol parameter of the Content-Type
		/// header should be.</para>
		/// </remarks>
		/// <value>The encryption protocol.</value>
		public abstract string EncryptionProtocol { get; }

		/// <summary>
		/// Gets the key exchange protocol.
		/// </summary>
		/// <remarks>
		/// <para>The key exchange protocol is really only used for PGP.</para>
		/// </remarks>
		/// <value>The key exchange protocol.</value>
		public abstract string KeyExchangeProtocol { get; }

		#if NOT_YET
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="MimeKit.Cryptography.CryptographyContext"/> allows online
		/// certificate retrieval.
		/// </summary>
		/// <value><c>true</c> if online certificate retrieval should be allowed; otherwise, <c>false</c>.</value>
		public bool AllowOnlineCertificateRetrieval { get; set; }

		/// <summary>
		/// Gets or sets the online certificate retrieval timeout.
		/// </summary>
		/// <value>The online certificate retrieval timeout.</value>
		public TimeSpan OnlineCertificateRetrievalTimeout { get; set; }
		#endif

		/// <summary>
		/// Checks whether or not the specified protocol is supported by the <see cref="CryptographyContext"/>.
		/// </summary>
		/// <remarks>
		/// Used in order to make sure that the protocol parameter value specified in either a multipart/signed
		/// or multipart/encrypted part is supported by the supplied cryptography context.
		/// </remarks>
		/// <returns><c>true</c> if the protocol is supported; otherwise <c>false</c></returns>
		/// <param name="protocol">The protocol.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="protocol"/> is <c>null</c>.
		/// </exception>
		public abstract bool Supports (string protocol);

		/// <summary>
		/// Gets the string name of the digest algorithm for use with the micalg parameter of a multipart/signed part.
		/// </summary>
		/// <remarks>
		/// Maps the <see cref="DigestAlgorithm"/> to the appropriate string identifier
		/// as used by the micalg parameter value of a multipart/signed Content-Type
		/// header.
		/// </remarks>
		/// <returns>The micalg value.</returns>
		/// <param name="micalg">The digest algorithm.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="micalg"/> is out of range.
		/// </exception>
		public abstract string GetDigestAlgorithmName (DigestAlgorithm micalg);

		/// <summary>
		/// Gets the digest algorithm from the micalg parameter value in a multipart/signed part.
		/// </summary>
		/// <remarks>
		/// Maps the micalg parameter value string back to the appropriate <see cref="DigestAlgorithm"/>.
		/// </remarks>
		/// <returns>The digest algorithm.</returns>
		/// <param name="micalg">The micalg parameter value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="micalg"/> is <c>null</c>.
		/// </exception>
		public abstract DigestAlgorithm GetDigestAlgorithm (string micalg);

		/// <summary>
		/// Cryptographically signs the content.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs the content using the specified signer and digest algorithm.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.MimePart"/> instance
		/// containing the detached signature data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="content">The content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="digestAlgo"/> is out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The specified <see cref="DigestAlgorithm"/> is not supported by this context.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for <paramref name="signer"/>.
		/// </exception>
		public abstract MimePart Sign (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content);

		/// <summary>
		/// Verifies the specified content using the detached signatureData.
		/// </summary>
		/// <remarks>
		/// Verifies the specified content using the detached signatureData.
		/// </remarks>
		/// <returns>A list of digital signatures.</returns>
		/// <param name="content">The content.</param>
		/// <param name="signatureData">The signature data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signatureData"/> is <c>null</c>.</para>
		/// </exception>
		public abstract DigitalSignatureCollection Verify (Stream content, Stream signatureData);

		/// <summary>
		/// Encrypts the specified content for the specified recipients.
		/// </summary>
		/// <remarks>
		/// Encrypts the specified content for the specified recipients.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.MimePart"/> instance
		/// containing the encrypted data.</returns>
		/// <param name="recipients">The recipients.</param>
		/// <param name="content">The content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		public abstract MimePart Encrypt (IEnumerable<MailboxAddress> recipients, Stream content);

		/// <summary>
		/// Decrypts the specified encryptedData.
		/// </summary>
		/// <remarks>
		/// Decrypts the specified encryptedData.
		/// </remarks>
		/// <returns>The decrypted <see cref="MimeKit.MimeEntity"/>.</returns>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="encryptedData"/> is <c>null</c>.
		/// </exception>
		public abstract MimeEntity Decrypt (Stream encryptedData);

		/// <summary>
		/// Imports the public certificates or keys from the specified stream.
		/// </summary>
		/// <remarks>
		/// Imports the public certificates or keys from the specified stream.
		/// </remarks>
		/// <param name="stream">The raw certificate or key data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// Importing keys is not supported by this cryptography context.
		/// </exception>
		public abstract void Import (Stream stream);

		/// <summary>
		/// Exports the keys for the specified mailboxes.
		/// </summary>
		/// <remarks>
		/// Exports the keys for the specified mailboxes.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.MimePart"/> instance containing the exported keys.</returns>
		/// <param name="mailboxes">The mailboxes.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailboxes"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="mailboxes"/> was empty.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// Exporting keys is not supported by this cryptography context.
		/// </exception>
		public abstract MimePart Export (IEnumerable<MailboxAddress> mailboxes);

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="CryptographyContext"/> and
		/// optionally releases the managed resources.
		/// </summary>
		/// <remarks>
		/// Releases the unmanaged resources used by the <see cref="CryptographyContext"/> and
		/// optionally releases the managed resources.
		/// </remarks>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
		/// <c>false</c> to release only the unmanaged resources.</param>
		protected virtual void Dispose (bool disposing)
		{
		}

		/// <summary>
		/// Releases all resources used by the <see cref="MimeKit.Cryptography.CryptographyContext"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose()"/> when you are finished using the <see cref="MimeKit.Cryptography.CryptographyContext"/>. The
		/// <see cref="Dispose()"/> method leaves the <see cref="MimeKit.Cryptography.CryptographyContext"/> in an unusable state. After
		/// calling <see cref="Dispose()"/>, you must release all references to the <see cref="MimeKit.Cryptography.CryptographyContext"/> so
		/// the garbage collector can reclaim the memory that the <see cref="MimeKit.Cryptography.CryptographyContext"/> was occupying.</remarks>
		public void Dispose ()
		{
			Dispose (true);
		}

		/// <summary>
		/// Creates a new <see cref="CryptographyContext"/> for the specified protocol.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="CryptographyContext"/> for the specified protocol.</para>
		/// <para>The default <see cref="CryptographyContext"/> types can over overridden by calling
		/// the <see cref="Register"/> method with the preferred type.</para>
		/// </remarks>
		/// <returns>The <see cref="CryptographyContext"/> for the protocol.</returns>
		/// <param name="protocol">The protocol.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="protocol"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// There are no supported <see cref="CryptographyContext"/>s that support 
		/// the specified <paramref name="protocol"/>.
		/// </exception>
		public static CryptographyContext Create (string protocol)
		{
			if (protocol == null)
				throw new ArgumentNullException ("protocol");

			protocol = protocol.ToLowerInvariant ();

			lock (mutex) {
				switch (protocol) {
				case "application/x-pkcs7-signature":
				case "application/pkcs7-signature":
				case "application/x-pkcs7-mime":
				case "application/pkcs7-mime":
				case "application/x-pkcs7-keys":
				case "application/pkcs7-keys":
					if (SecureMimeContextConstructor != null)
						return (CryptographyContext) SecureMimeContextConstructor.Invoke (new object[0]);

					if (!SqliteCertificateDatabase.IsAvailable)
						throw new NotSupportedException ("You need to subclass MimeKit.Cryptography.SecureMimeContext and then register it with MimeKit.Cryptography.CryptographyContext.Register().");

					return new DefaultSecureMimeContext ();
				case "application/x-pgp-signature":
				case "application/pgp-signature":
				case "application/x-pgp-encrypted":
				case "application/pgp-encrypted":
				case "application/x-pgp-keys":
				case "application/pgp-keys":
					if (OpenPgpContextConstructor != null)
						return (CryptographyContext) OpenPgpContextConstructor.Invoke (new object[0]);

					throw new NotSupportedException ("You need to subclass MimeKit.Cryptography.GnuPGContext and then register it with MimeKit.Cryptography.CryptographyContext.Register().");
				default:
					throw new NotSupportedException ();
				}
			}
		}

		/// <summary>
		/// Registers a default <see cref="SecureMimeContext"/> or <see cref="OpenPgpContext"/>.
		/// </summary>
		/// <remarks>
		/// Registers the specified type as the default <see cref="SecureMimeContext"/> or
		/// <see cref="OpenPgpContext"/>.
		/// </remarks>
		/// <param name="type">A custom subclass of <see cref="SecureMimeContext"/> or
		/// <see cref="OpenPgpContext"/>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="type"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="type"/> is not a subclass of
		/// <see cref="SecureMimeContext"/> or <see cref="OpenPgpContext"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="type"/> does not have a parameterless constructor.</para>
		/// </exception>
		public static void Register (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			var ctor = type.GetConstructor (new Type[0]);
			if (ctor == null)
				throw new ArgumentException ("The specified type must have a parameterless constructor.", "type");

			if (type.IsSubclassOf (typeof (SecureMimeContext))) {
				lock (mutex) {
					SecureMimeContextConstructor = ctor;
				}
			} else if (type.IsSubclassOf (typeof (OpenPgpContext))) {
				lock (mutex) {
					OpenPgpContextConstructor = ctor;
				}
			} else {
				throw new ArgumentException ("The specified type must be a subclass of SecureMimeContext or OpenPgpContext.", "type");
			}
		}
	}
}

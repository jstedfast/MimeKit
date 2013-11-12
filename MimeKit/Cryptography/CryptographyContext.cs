//
// CryptographyContext.cs
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
		/// Gets the signature protocol.
		/// </summary>
		/// <value>The signature protocol.</value>
		public abstract string SignatureProtocol { get; }

		/// <summary>
		/// Gets the encryption protocol.
		/// </summary>
		/// <value>The encryption protocol.</value>
		public abstract string EncryptionProtocol { get; }

		/// <summary>
		/// Gets the key exchange protocol.
		/// </summary>
		/// <value>The key exchange protocol.</value>
		public abstract string KeyExchangeProtocol { get; }

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

		/// <summary>
		/// Checks whether or not the specified protocol is supported by the <see cref="CryptographyContext"/>.
		/// </summary>
		/// <returns><c>true</c> if the protocol is supported; otherwise <c>false</c></returns>
		/// <param name="protocol">The protocol.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="protocol"/> is <c>null</c>.
		/// </exception>
		public abstract bool Supports (string protocol);

		/// <summary>
		/// Gets the string name of the digest algorithm for use with the micalg parameter of a multipart/signed part.
		/// </summary>
		/// <returns>The micalg value.</returns>
		/// <param name="micalg">The digest algorithm.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="micalg"/> is out of range.
		/// </exception>
		public abstract string GetMicAlgorithmName (DigestAlgorithm micalg);

		/// <summary>
		/// Gets the digest algorithm from the micalg parameter value in a multipart/signed part.
		/// </summary>
		/// <returns>The digest algorithm.</returns>
		/// <param name="micalg">The micalg parameter value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="micalg"/> is <c>null</c>.
		/// </exception>
		public abstract DigestAlgorithm GetDigestAlgorithm (string micalg);

		/// <summary>
		/// Sign the content using the specified signer.
		/// </summary>
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
		/// Verify the specified content and signatureData.
		/// </summary>
		/// <returns>A list of digital signatures.</returns>
		/// <param name="content">The content.</param>
		/// <param name="signatureData">The signature data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signatureData"/> is <c>null</c>.</para>
		/// </exception>
		public abstract IList<IDigitalSignature> Verify (Stream content, Stream signatureData);

		/// <summary>
		/// Encrypts the specified content for the specified recipients.
		/// </summary>
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
		/// Decrypt the specified encryptedData.
		/// </summary>
		/// <returns>The decrypted <see cref="MimeKit.MimeEntity"/>.</returns>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="encryptedData"/> is <c>null</c>.
		/// </exception>
		public abstract MimeEntity Decrypt (Stream encryptedData);

		/// <summary>
		/// Imports the public certificates or keys from the specified stream.
		/// </summary>
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
		/// Releases all resources used by the <see cref="MimeKit.Cryptography.CryptographyContext"/> object.
		/// </summary>
		/// <param name="disposing">If <c>true</c>, this method is being called by
		/// <see cref="Dispose()"/>; otherwise it is being called by the finalizer.</param>
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
		/// Creates a new <see cref="MimeKit.Cryptography.CryptographyContext"/> for the specified protocol.
		/// </summary>
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

					return new DefaultSecureMimeContext ();
				case "application/x-pgp-signature":
				case "application/pgp-signature":
				case "application/x-pgp-encrypted":
				case "application/pgp-encrypted":
				case "application/x-pgp-keys":
				case "application/pgp-keys":
					if (OpenPgpContextConstructor != null)
						return (CryptographyContext) OpenPgpContextConstructor.Invoke (new object[0]);

					throw new NotSupportedException ("You need to subclass MimeKit.Cryptography.GnuPGContext and then registering it with MimeKit.Cryptography.CryptographyContext.Register().");
				default:
					throw new NotSupportedException ();
				}
			}
		}

		/// <summary>
		/// Registers a default <see cref="SecureMimeContext"/> or <see cref="OpenPgpContext"/>.
		/// </summary>
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

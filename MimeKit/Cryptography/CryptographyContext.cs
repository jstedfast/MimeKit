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
using System.Collections.Generic;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An abstract cryptography context.
	/// </summary>
	public abstract class CryptographyContext : IDisposable
	{
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
		/// Sign the content using the specified signer.
		/// </summary>
		/// <returns>A new <see cref="MimeKit.MimePart"/> instance
		/// containing the detached signature data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="content">The content.</param>
		/// <param name="digestAlgo">The digest algorithm used.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		public abstract MimePart Sign (MailboxAddress signer, byte[] content, out string digestAlgo);

		// FIXME: come up with a generic Verify() API that will work for PGP/MIME as well as S/MIME

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
		public abstract MimePart Encrypt (IEnumerable<MailboxAddress> recipients, byte[] content);

		/// <summary>
		/// Signs and encrypts the specified content for the specified recipients.
		/// </summary>
		/// <returns>A new <see cref="MimeKit.MimePart"/> instance
		/// containing the encrypted data.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="content">The content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		public abstract MimePart SignAndEncrypt (MailboxAddress signer, IEnumerable<MailboxAddress> recipients, byte[] content);

		// FIXME: come up with a generic Decrypt() API that will work for PGP/MIME as well as S/MIME

		/// <summary>
		/// Imports the keys.
		/// </summary>
		/// <param name="keyData">The key data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="keyData"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// Importing keys is not supported by this cryptography context.
		/// </exception>
		public abstract void ImportKeys (byte[] keyData);

		/// <summary>
		/// Exports the keys for the specified mailboxes.
		/// </summary>
		/// <returns>The mailboxes.</returns>
		/// <param name="mailboxes">The mailboxes.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailboxes"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// Exporting keys is not supported by this cryptography context.
		/// </exception>
		public abstract MimePart ExportKeys (IEnumerable<MailboxAddress> mailboxes);

		/// <summary>
		/// Dispose the specified disposing.
		/// </summary>
		/// <param name="disposing">If set to <c>true</c> disposing.</param>
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

			switch (protocol.ToLowerInvariant ()) {
			case "application/x-pkcs7-signature":
			case "application/pkcs7-signature":
			case "application/x-pkcs7-mime":
			case "application/pkcs7-mime":
			case "application/x-pkcs7-keys":
			case "application/pkcs7-keys":
				return new SecureMimeContext ();
			default:
				throw new NotSupportedException ();
			}
		}
	}
}

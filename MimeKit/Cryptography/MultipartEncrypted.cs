//
// MultipartEncrypted.cs
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
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Org.BouncyCastle.Bcpg.OpenPgp;

using MimeKit.IO;
using MimeKit.IO.Filters;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A multipart MIME part with a ContentType of multipart/encrypted containing an encrypted MIME part.
	/// </summary>
	/// <remarks>
	/// This mime-type is common when dealing with PGP/MIME but is not used for S/MIME.
	/// </remarks>
	public class MultipartEncrypted : Multipart, IMultipartEncrypted
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="MultipartEncrypted"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is used by <see cref="MimeParser"/>.
		/// </remarks>
		/// <param name="args">Information used by the constructor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		public MultipartEncrypted (MimeEntityConstructorArgs args) : base (args)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MultipartEncrypted"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MultipartEncrypted"/>.
		/// </remarks>
		public MultipartEncrypted () : base ("encrypted")
		{
		}

		void CheckDisposed ()
		{
			CheckDisposed (nameof (MultipartEncrypted));
		}

		/// <summary>
		/// Dispatches to the specific visit method for this MIME entity.
		/// </summary>
		/// <remarks>
		/// This default implementation for <see cref="MultipartEncrypted"/> nodes
		/// calls <see cref="MimeVisitor.VisitMultipartEncrypted"/>. Override this
		/// method to call into a more specific method on a derived visitor class
		/// of the <see cref="MimeVisitor"/> class. However, it should still
		/// support unknown visitors by calling
		/// <see cref="MimeVisitor.VisitMultipartEncrypted"/>.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="MultipartEncrypted"/> has been disposed.
		/// </exception>
		public override void Accept (MimeVisitor visitor)
		{
			if (visitor == null)
				throw new ArgumentNullException (nameof (visitor));

			CheckDisposed ();

			visitor.VisitMultipartEncrypted (this);
		}

		static MultipartEncrypted CreateMultipartEncrypted (OpenPgpContext ctx, MimeEntity part)
		{
			var encrypted = new MultipartEncrypted ();

			encrypted.ContentType.Parameters["protocol"] = ctx.EncryptionProtocol;

			// add the protocol version part
			encrypted.Add (new ApplicationPgpEncrypted ());

			// add the encrypted entity as the second part
			encrypted.Add (part);

			return encrypted;
		}

		static async Task<MultipartEncrypted> SignAndEncryptAsync (OpenPgpContext ctx, MailboxAddress signer, DigestAlgorithm digestAlgo, EncryptionAlgorithm cipherAlgo, IEnumerable<MailboxAddress> recipients, MimeEntity entity, bool doAsync, CancellationToken cancellationToken)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				if (doAsync)
					await entity.WriteToAsync (options, memory, cancellationToken).ConfigureAwait (false);
				else
					entity.WriteTo (options, memory, cancellationToken);
				memory.Position = 0;

				MimePart part;

				if (doAsync)
					part = await ctx.SignAndEncryptAsync (signer, digestAlgo, cipherAlgo, recipients, memory, cancellationToken).ConfigureAwait (false);
				else
					part = ctx.SignAndEncrypt (signer, digestAlgo, cipherAlgo, recipients, memory, cancellationToken);

				return CreateMultipartEncrypted (ctx, part);
			}
		}

		/// <summary>
		/// Create a multipart/encrypted MIME part by signing and encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Signs the entity using the supplied signer and digest algorithm and then encrypts to
		/// the specified recipients, encapsulating the result in a new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the signed and encrypted version of the specified entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for singing and encrypting.</param>
		/// <param name="signer">The signer to use to sign the entity.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="cipherAlgo">The encryption algorithm.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="signer"/> cannot be used for signing.</para>
		/// <para>-or-</para>
		/// <para>One or more of the recipient keys cannot be used for encrypting.</para>
		/// <para>-or-</para>
		/// <para>No recipients were specified.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <para>The <paramref name="digestAlgo"/> is not supported.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="cipherAlgo"/> is not supported.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// <para>The user chose to cancel the password prompt.</para>
		/// <para>-or-</para>
		/// <para>The operation was canceled via the cancellation token.</para>
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public static MultipartEncrypted SignAndEncrypt (OpenPgpContext ctx, MailboxAddress signer, DigestAlgorithm digestAlgo, EncryptionAlgorithm cipherAlgo, IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			return SignAndEncryptAsync (ctx, signer, digestAlgo, cipherAlgo, recipients, entity, false, cancellationToken).GetAwaiter ().GetResult ();
		}

		/// <summary>
		/// Asynchronously create a multipart/encrypted MIME part by signing and encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Signs the entity using the supplied signer and digest algorithm and then encrypts to
		/// the specified recipients, encapsulating the result in a new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the signed and encrypted version of the specified entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for singing and encrypting.</param>
		/// <param name="signer">The signer to use to sign the entity.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="cipherAlgo">The encryption algorithm.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="signer"/> cannot be used for signing.</para>
		/// <para>-or-</para>
		/// <para>One or more of the recipient keys cannot be used for encrypting.</para>
		/// <para>-or-</para>
		/// <para>No recipients were specified.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <para>The <paramref name="digestAlgo"/> is not supported.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="cipherAlgo"/> is not supported.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// <para>The user chose to cancel the password prompt.</para>
		/// <para>-or-</para>
		/// <para>The operation was canceled via the cancellation token.</para>
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public static Task<MultipartEncrypted> SignAndEncryptAsync (OpenPgpContext ctx, MailboxAddress signer, DigestAlgorithm digestAlgo, EncryptionAlgorithm cipherAlgo, IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			return SignAndEncryptAsync (ctx, signer, digestAlgo, cipherAlgo, recipients, entity, true, cancellationToken);
		}

		static async Task<MultipartEncrypted> SignAndEncryptAsync (OpenPgpContext ctx, MailboxAddress signer, DigestAlgorithm digestAlgo, IEnumerable<MailboxAddress> recipients, MimeEntity entity, bool doAsync, CancellationToken cancellationToken)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				if (doAsync)
					await entity.WriteToAsync (options, memory, cancellationToken).ConfigureAwait (false);
				else
					entity.WriteTo (options, memory, cancellationToken);
				memory.Position = 0;

				MimePart part;

				if (doAsync)
					part = await ctx.SignAndEncryptAsync (signer, digestAlgo, recipients, memory, cancellationToken).ConfigureAwait (false);
				else
					part = ctx.SignAndEncrypt (signer, digestAlgo, recipients, memory, cancellationToken);

				return CreateMultipartEncrypted (ctx, part);
			}
		}

		/// <summary>
		/// Create a multipart/encrypted MIME part by signing and encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Signs the entity using the supplied signer and digest algorithm and then encrypts to
		/// the specified recipients, encapsulating the result in a new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the signed and encrypted version of the specified entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for signing and encrypting.</param>
		/// <param name="signer">The signer to use to sign the entity.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key for <paramref name="signer"/> could not be found.
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// A public key for one or more of the <paramref name="recipients"/> could not be found.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// <para>The user chose to cancel the password prompt.</para>
		/// <para>-or-</para>
		/// <para>The operation was canceled via the cancellation token.</para>
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public static MultipartEncrypted SignAndEncrypt (OpenPgpContext ctx, MailboxAddress signer, DigestAlgorithm digestAlgo, IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			return SignAndEncryptAsync (ctx, signer, digestAlgo, recipients, entity, false, cancellationToken).GetAwaiter ().GetResult ();
		}

		/// <summary>
		/// Asynchronously create a multipart/encrypted MIME part by signing and encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Signs the entity using the supplied signer and digest algorithm and then encrypts to
		/// the specified recipients, encapsulating the result in a new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the signed and encrypted version of the specified entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for signing and encrypting.</param>
		/// <param name="signer">The signer to use to sign the entity.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key for <paramref name="signer"/> could not be found.
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// A public key for one or more of the <paramref name="recipients"/> could not be found.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// <para>The user chose to cancel the password prompt.</para>
		/// <para>-or-</para>
		/// <para>The operation was canceled via the cancellation token.</para>
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public static Task<MultipartEncrypted> SignAndEncryptAsync (OpenPgpContext ctx, MailboxAddress signer, DigestAlgorithm digestAlgo, IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			return SignAndEncryptAsync (ctx, signer, digestAlgo, recipients, entity, true, cancellationToken);
		}

		/// <summary>
		/// Create a multipart/encrypted MIME part by signing and encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Signs the entity using the supplied signer and digest algorithm and then encrypts to
		/// the specified recipients, encapsulating the result in a new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the signed and encrypted version of the specified entity.</returns>
		/// <param name="signer">The signer to use to sign the entity.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="cipherAlgo">The encryption algorithm.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="signer"/> cannot be used for signing.</para>
		/// <para>-or-</para>
		/// <para>One or more of the recipient keys cannot be used for encrypting.</para>
		/// <para>-or-</para>
		/// <para>No recipients were specified.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <para>A default <see cref="OpenPgpContext"/> has not been registered.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="digestAlgo"/> is not supported.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="cipherAlgo"/> is not supported.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// <para>The user chose to cancel the password prompt.</para>
		/// <para>-or-</para>
		/// <para>The operation was canceled via the cancellation token.</para>
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public static MultipartEncrypted SignAndEncrypt (MailboxAddress signer, DigestAlgorithm digestAlgo, EncryptionAlgorithm cipherAlgo, IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (OpenPgpContext) CryptographyContext.Create ("application/pgp-encrypted"))
				return SignAndEncrypt (ctx, signer, digestAlgo, cipherAlgo, recipients, entity, cancellationToken);
		}

		/// <summary>
		/// Asynchronously create a multipart/encrypted MIME part by signing and encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Signs the entity using the supplied signer and digest algorithm and then encrypts to
		/// the specified recipients, encapsulating the result in a new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the signed and encrypted version of the specified entity.</returns>
		/// <param name="signer">The signer to use to sign the entity.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="cipherAlgo">The encryption algorithm.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="signer"/> cannot be used for signing.</para>
		/// <para>-or-</para>
		/// <para>One or more of the recipient keys cannot be used for encrypting.</para>
		/// <para>-or-</para>
		/// <para>No recipients were specified.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <para>A default <see cref="OpenPgpContext"/> has not been registered.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="digestAlgo"/> is not supported.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="cipherAlgo"/> is not supported.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// <para>The user chose to cancel the password prompt.</para>
		/// <para>-or-</para>
		/// <para>The operation was canceled via the cancellation token.</para>
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public static async Task<MultipartEncrypted> SignAndEncryptAsync (MailboxAddress signer, DigestAlgorithm digestAlgo, EncryptionAlgorithm cipherAlgo, IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (OpenPgpContext) CryptographyContext.Create ("application/pgp-encrypted"))
				return await SignAndEncryptAsync (ctx, signer, digestAlgo, cipherAlgo, recipients, entity, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Create a multipart/encrypted MIME part by signing and encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Signs the entity using the supplied signer and digest algorithm and then encrypts to
		/// the specified recipients, encapsulating the result in a new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the signed and encrypted version of the specified entity.</returns>
		/// <param name="signer">The signer to use to sign the entity.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// A default <see cref="OpenPgpContext"/> has not been registered.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key for <paramref name="signer"/> could not be found.
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// A public key for one or more of the <paramref name="recipients"/> could not be found.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The user chose to cancel the password prompt.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public static MultipartEncrypted SignAndEncrypt (MailboxAddress signer, DigestAlgorithm digestAlgo, IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (OpenPgpContext) CryptographyContext.Create ("application/pgp-encrypted"))
				return SignAndEncrypt (ctx, signer, digestAlgo, recipients, entity, cancellationToken);
		}

		/// <summary>
		/// Asynchronously create a multipart/encrypted MIME part by signing and encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Signs the entity using the supplied signer and digest algorithm and then encrypts to
		/// the specified recipients, encapsulating the result in a new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the signed and encrypted version of the specified entity.</returns>
		/// <param name="signer">The signer to use to sign the entity.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// A default <see cref="OpenPgpContext"/> has not been registered.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key for <paramref name="signer"/> could not be found.
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// A public key for one or more of the <paramref name="recipients"/> could not be found.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The user chose to cancel the password prompt.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public static async Task<MultipartEncrypted> SignAndEncryptAsync (MailboxAddress signer, DigestAlgorithm digestAlgo, IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (OpenPgpContext) CryptographyContext.Create ("application/pgp-encrypted"))
				return await SignAndEncryptAsync (ctx, signer, digestAlgo, recipients, entity, cancellationToken).ConfigureAwait (false);
		}

		static async Task<MultipartEncrypted> SignAndEncryptAsync (OpenPgpContext ctx, PgpSecretKey signer, DigestAlgorithm digestAlgo, EncryptionAlgorithm cipherAlgo, IEnumerable<PgpPublicKey> recipients, MimeEntity entity, bool doAsync, CancellationToken cancellationToken)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				if (doAsync)
					await entity.WriteToAsync (options, memory, cancellationToken).ConfigureAwait (false);
				else
					entity.WriteTo (options, memory, cancellationToken);
				memory.Position = 0;

				MimePart part;

				if (doAsync)
					part = await ctx.SignAndEncryptAsync (signer, digestAlgo, cipherAlgo, recipients, memory, cancellationToken).ConfigureAwait (false);
				else
					part = ctx.SignAndEncrypt (signer, digestAlgo, cipherAlgo, recipients, memory, cancellationToken);

				return CreateMultipartEncrypted (ctx, part);
			}
		}

		/// <summary>
		/// Create a multipart/encrypted MIME part by signing and encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Signs the entity using the supplied signer and digest algorithm and then encrypts to
		/// the specified recipients, encapsulating the result in a new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the signed and encrypted version of the specified entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for singing and encrypting.</param>
		/// <param name="signer">The signer to use to sign the entity.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="cipherAlgo">The encryption algorithm.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="signer"/> cannot be used for signing.</para>
		/// <para>-or-</para>
		/// <para>One or more of the recipient keys cannot be used for encrypting.</para>
		/// <para>-or-</para>
		/// <para>No recipients were specified.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <para>The <paramref name="digestAlgo"/> is not supported.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="cipherAlgo"/> is not supported.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// <para>The user chose to cancel the password prompt.</para>
		/// <para>-or-</para>
		/// <para>The operation was canceled via the cancellation token.</para>
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public static MultipartEncrypted SignAndEncrypt (OpenPgpContext ctx, PgpSecretKey signer, DigestAlgorithm digestAlgo, EncryptionAlgorithm cipherAlgo, IEnumerable<PgpPublicKey> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			return SignAndEncryptAsync (ctx, signer, digestAlgo, cipherAlgo, recipients, entity, false, cancellationToken).GetAwaiter ().GetResult ();
		}

		/// <summary>
		/// Asynchronously create a multipart/encrypted MIME part by signing and encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Signs the entity using the supplied signer and digest algorithm and then encrypts to
		/// the specified recipients, encapsulating the result in a new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the signed and encrypted version of the specified entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for singing and encrypting.</param>
		/// <param name="signer">The signer to use to sign the entity.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="cipherAlgo">The encryption algorithm.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="signer"/> cannot be used for signing.</para>
		/// <para>-or-</para>
		/// <para>One or more of the recipient keys cannot be used for encrypting.</para>
		/// <para>-or-</para>
		/// <para>No recipients were specified.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <para>The <paramref name="digestAlgo"/> is not supported.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="cipherAlgo"/> is not supported.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// <para>The user chose to cancel the password prompt.</para>
		/// <para>-or-</para>
		/// <para>The operation was canceled via the cancellation token.</para>
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public static Task<MultipartEncrypted> SignAndEncryptAsync (OpenPgpContext ctx, PgpSecretKey signer, DigestAlgorithm digestAlgo, EncryptionAlgorithm cipherAlgo, IEnumerable<PgpPublicKey> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			return SignAndEncryptAsync (ctx, signer, digestAlgo, cipherAlgo, recipients, entity, true, cancellationToken);
		}

		static async Task<MultipartEncrypted> SignAndEncryptAsync (OpenPgpContext ctx, PgpSecretKey signer, DigestAlgorithm digestAlgo, IEnumerable<PgpPublicKey> recipients, MimeEntity entity, bool doAsync, CancellationToken cancellationToken)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				if (doAsync)
					await entity.WriteToAsync (options, memory, cancellationToken).ConfigureAwait (false);
				else
					entity.WriteTo (options, memory, cancellationToken);
				memory.Position = 0;

				MimePart part;

				if (doAsync)
					part = await ctx.SignAndEncryptAsync (signer, digestAlgo, recipients, memory, cancellationToken).ConfigureAwait (false);
				else
					part = ctx.SignAndEncrypt (signer, digestAlgo, recipients, memory, cancellationToken);

				return CreateMultipartEncrypted (ctx, part);
			}
		}

		/// <summary>
		/// Create a multipart/encrypted MIME part by signing and encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Signs the entity using the supplied signer and digest algorithm and then encrypts to
		/// the specified recipients, encapsulating the result in a new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the signed and encrypted version of the specified entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for singing and encrypting.</param>
		/// <param name="signer">The signer to use to sign the entity.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="signer"/> cannot be used for signing.</para>
		/// <para>-or-</para>
		/// <para>One or more of the recipient keys cannot be used for encrypting.</para>
		/// <para>-or-</para>
		/// <para>No recipients were specified.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The <paramref name="digestAlgo"/> is not supported.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// <para>The user chose to cancel the password prompt.</para>
		/// <para>-or-</para>
		/// <para>The operation was canceled via the cancellation token.</para>
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public static MultipartEncrypted SignAndEncrypt (OpenPgpContext ctx, PgpSecretKey signer, DigestAlgorithm digestAlgo, IEnumerable<PgpPublicKey> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			return SignAndEncryptAsync (ctx, signer, digestAlgo, recipients, entity, false, cancellationToken).GetAwaiter ().GetResult ();
		}

		/// <summary>
		/// Asynchronously create a multipart/encrypted MIME part by signing and encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Signs the entity using the supplied signer and digest algorithm and then encrypts to
		/// the specified recipients, encapsulating the result in a new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the signed and encrypted version of the specified entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for singing and encrypting.</param>
		/// <param name="signer">The signer to use to sign the entity.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="signer"/> cannot be used for signing.</para>
		/// <para>-or-</para>
		/// <para>One or more of the recipient keys cannot be used for encrypting.</para>
		/// <para>-or-</para>
		/// <para>No recipients were specified.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The <paramref name="digestAlgo"/> is not supported.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// <para>The user chose to cancel the password prompt.</para>
		/// <para>-or-</para>
		/// <para>The operation was canceled via the cancellation token.</para>
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public static Task<MultipartEncrypted> SignAndEncryptAsync (OpenPgpContext ctx, PgpSecretKey signer, DigestAlgorithm digestAlgo, IEnumerable<PgpPublicKey> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			return SignAndEncryptAsync (ctx, signer, digestAlgo, recipients, entity, true, cancellationToken);
		}

		/// <summary>
		/// Create a multipart/encrypted MIME part by signing and encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Signs the entity using the supplied signer and digest algorithm and then encrypts to
		/// the specified recipients, encapsulating the result in a new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the signed and encrypted version of the specified entity.</returns>
		/// <param name="signer">The signer to use to sign the entity.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="cipherAlgo">The encryption algorithm.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="signer"/> cannot be used for signing.</para>
		/// <para>-or-</para>
		/// <para>One or more of the recipient keys cannot be used for encrypting.</para>
		/// <para>-or-</para>
		/// <para>No recipients were specified.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <para>A default <see cref="OpenPgpContext"/> has not been registered.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="digestAlgo"/> is not supported.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="cipherAlgo"/> is not supported.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// <para>The user chose to cancel the password prompt.</para>
		/// <para>-or-</para>
		/// <para>The operation was canceled via the cancellation token.</para>
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public static MultipartEncrypted SignAndEncrypt (PgpSecretKey signer, DigestAlgorithm digestAlgo, EncryptionAlgorithm cipherAlgo, IEnumerable<PgpPublicKey> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (OpenPgpContext) CryptographyContext.Create ("application/pgp-encrypted"))
				return SignAndEncrypt (ctx, signer, digestAlgo, cipherAlgo, recipients, entity, cancellationToken);
		}

		/// <summary>
		/// Asynchronously create a multipart/encrypted MIME part by signing and encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Signs the entity using the supplied signer and digest algorithm and then encrypts to
		/// the specified recipients, encapsulating the result in a new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the signed and encrypted version of the specified entity.</returns>
		/// <param name="signer">The signer to use to sign the entity.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="cipherAlgo">The encryption algorithm.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="signer"/> cannot be used for signing.</para>
		/// <para>-or-</para>
		/// <para>One or more of the recipient keys cannot be used for encrypting.</para>
		/// <para>-or-</para>
		/// <para>No recipients were specified.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <para>A default <see cref="OpenPgpContext"/> has not been registered.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="digestAlgo"/> is not supported.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="cipherAlgo"/> is not supported.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// <para>The user chose to cancel the password prompt.</para>
		/// <para>-or-</para>
		/// <para>The operation was canceled via the cancellation token.</para>
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public static async Task<MultipartEncrypted> SignAndEncryptAsync (PgpSecretKey signer, DigestAlgorithm digestAlgo, EncryptionAlgorithm cipherAlgo, IEnumerable<PgpPublicKey> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (OpenPgpContext) CryptographyContext.Create ("application/pgp-encrypted"))
				return await SignAndEncryptAsync (ctx, signer, digestAlgo, cipherAlgo, recipients, entity, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Create a multipart/encrypted MIME part by signing and encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Signs the entity using the supplied signer and digest algorithm and then encrypts to
		/// the specified recipients, encapsulating the result in a new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the signed and encrypted version of the specified entity.</returns>
		/// <param name="signer">The signer to use to sign the entity.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="signer"/> cannot be used for signing.</para>
		/// <para>-or-</para>
		/// <para>One or more of the recipient keys cannot be used for encrypting.</para>
		/// <para>-or-</para>
		/// <para>No recipients were specified.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <para>A default <see cref="OpenPgpContext"/> has not been registered.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="digestAlgo"/> is not supported.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// <para>The user chose to cancel the password prompt.</para>
		/// <para>-or-</para>
		/// <para>The operation was canceled via the cancellation token.</para>
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public static MultipartEncrypted SignAndEncrypt (PgpSecretKey signer, DigestAlgorithm digestAlgo, IEnumerable<PgpPublicKey> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (OpenPgpContext) CryptographyContext.Create ("application/pgp-encrypted"))
				return SignAndEncrypt (ctx, signer, digestAlgo, recipients, entity, cancellationToken);
		}

		/// <summary>
		/// Asynchronously create a multipart/encrypted MIME part by signing and encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Signs the entity using the supplied signer and digest algorithm and then encrypts to
		/// the specified recipients, encapsulating the result in a new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the signed and encrypted version of the specified entity.</returns>
		/// <param name="signer">The signer to use to sign the entity.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="signer"/> cannot be used for signing.</para>
		/// <para>-or-</para>
		/// <para>One or more of the recipient keys cannot be used for encrypting.</para>
		/// <para>-or-</para>
		/// <para>No recipients were specified.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <para>A default <see cref="OpenPgpContext"/> has not been registered.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="digestAlgo"/> is not supported.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// <para>The user chose to cancel the password prompt.</para>
		/// <para>-or-</para>
		/// <para>The operation was canceled via the cancellation token.</para>
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public static async Task<MultipartEncrypted> SignAndEncryptAsync (PgpSecretKey signer, DigestAlgorithm digestAlgo, IEnumerable<PgpPublicKey> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (OpenPgpContext) CryptographyContext.Create ("application/pgp-encrypted"))
				return await SignAndEncryptAsync (ctx, signer, digestAlgo, recipients, entity, cancellationToken).ConfigureAwait (false);
		}

		static async Task<MultipartEncrypted> EncryptAsync (OpenPgpContext ctx, EncryptionAlgorithm algorithm, IEnumerable<MailboxAddress> recipients, MimeEntity entity, bool doAsync, CancellationToken cancellationToken)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				using (var filtered = new FilteredStream (memory)) {
					filtered.Add (new Unix2DosFilter ());

					if (doAsync) {
						await entity.WriteToAsync (filtered, cancellationToken).ConfigureAwait (false);
						await filtered.FlushAsync (cancellationToken).ConfigureAwait (false);
					} else {
						entity.WriteTo (filtered, cancellationToken);
						filtered.Flush (cancellationToken);
					}
				}

				memory.Position = 0;

				MimePart part;

				if (doAsync)
					part = await ctx.EncryptAsync (algorithm, recipients, memory, cancellationToken).ConfigureAwait (false);
				else
					part = ctx.Encrypt (algorithm, recipients, memory, cancellationToken);

				return CreateMultipartEncrypted (ctx, part);
			}
		}

		/// <summary>
		/// Create a multipart/encrypted MIME part by encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients, encapsulating the result in a
		/// new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the encrypted version of the specified entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for encrypting.</param>
		/// <param name="algorithm">The encryption algorithm.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// One or more of the recipient keys cannot be used for encrypting.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The specified encryption algorithm is not supported.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public static MultipartEncrypted Encrypt (OpenPgpContext ctx, EncryptionAlgorithm algorithm, IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			return EncryptAsync (ctx, algorithm, recipients, entity, false, cancellationToken).GetAwaiter ().GetResult ();
		}

		/// <summary>
		/// Asynchronously create a multipart/encrypted MIME part by encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients, encapsulating the result in a
		/// new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the encrypted version of the specified entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for encrypting.</param>
		/// <param name="algorithm">The encryption algorithm.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// One or more of the recipient keys cannot be used for encrypting.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The specified encryption algorithm is not supported.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public static Task<MultipartEncrypted> EncryptAsync (OpenPgpContext ctx, EncryptionAlgorithm algorithm, IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			return EncryptAsync (ctx, algorithm, recipients, entity, true, cancellationToken);
		}

		static async Task<MultipartEncrypted> EncryptAsync (OpenPgpContext ctx, IEnumerable<MailboxAddress> recipients, MimeEntity entity, bool doAsync, CancellationToken cancellationToken)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				using (var filtered = new FilteredStream (memory)) {
					filtered.Add (new Unix2DosFilter ());

					if (doAsync) {
						await entity.WriteToAsync (filtered, cancellationToken).ConfigureAwait (false);
						await filtered.FlushAsync (cancellationToken).ConfigureAwait (false);
					} else {
						entity.WriteTo (filtered, cancellationToken);
						filtered.Flush (cancellationToken);
					}
				}

				memory.Position = 0;

				MimePart part;

				if (doAsync)
					part = await ctx.EncryptAsync (recipients, memory, cancellationToken).ConfigureAwait (false);
				else
					part = ctx.Encrypt (recipients, memory, cancellationToken);

				return CreateMultipartEncrypted (ctx, part);
			}
		}

		/// <summary>
		/// Create a multipart/encrypted MIME part by encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients, encapsulating the result in a
		/// new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the encrypted version of the specified entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for encrypting.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// A public key for one or more of the <paramref name="recipients"/> could not be found.
		/// </exception>
		public static MultipartEncrypted Encrypt (OpenPgpContext ctx, IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			return EncryptAsync (ctx, recipients, entity, false, cancellationToken).GetAwaiter ().GetResult ();
		}

		/// <summary>
		/// Asynchronously create a multipart/encrypted MIME part by encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients, encapsulating the result in a
		/// new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the encrypted version of the specified entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for encrypting.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// A public key for one or more of the <paramref name="recipients"/> could not be found.
		/// </exception>
		public static Task<MultipartEncrypted> EncryptAsync (OpenPgpContext ctx, IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			return EncryptAsync (ctx, recipients, entity, true, cancellationToken);
		}

		/// <summary>
		/// Create a multipart/encrypted MIME part by encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients, encapsulating the result in a
		/// new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the encrypted version of the specified entity.</returns>
		/// <param name="algorithm">The encryption algorithm.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// One or more of the recipient keys cannot be used for encrypting.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <para>A default <see cref="OpenPgpContext"/> has not been registered.</para>
		/// <para>-or-</para>
		/// <para>The specified encryption algorithm is not supported.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public static MultipartEncrypted Encrypt (EncryptionAlgorithm algorithm, IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (OpenPgpContext) CryptographyContext.Create ("application/pgp-encrypted"))
				return Encrypt (ctx, algorithm, recipients, entity, cancellationToken);
		}

		/// <summary>
		/// Asynchronously create a multipart/encrypted MIME part by encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients, encapsulating the result in a
		/// new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the encrypted version of the specified entity.</returns>
		/// <param name="algorithm">The encryption algorithm.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// One or more of the recipient keys cannot be used for encrypting.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <para>A default <see cref="OpenPgpContext"/> has not been registered.</para>
		/// <para>-or-</para>
		/// <para>The specified encryption algorithm is not supported.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public static async Task<MultipartEncrypted> EncryptAsync (EncryptionAlgorithm algorithm, IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (OpenPgpContext) CryptographyContext.Create ("application/pgp-encrypted"))
				return await EncryptAsync (ctx, algorithm, recipients, entity, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Create a multipart/encrypted MIME part by encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients, encapsulating the result in a
		/// new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the encrypted version of the specified entity.</returns>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// A default <see cref="OpenPgpContext"/> has not been registered.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// A public key for one or more of the <paramref name="recipients"/> could not be found.
		/// </exception>
		public static MultipartEncrypted Encrypt (IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (OpenPgpContext) CryptographyContext.Create ("application/pgp-encrypted"))
				return Encrypt (ctx, recipients, entity, cancellationToken);
		}

		/// <summary>
		/// Asynchronously create a multipart/encrypted MIME part by encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients, encapsulating the result in a
		/// new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the encrypted version of the specified entity.</returns>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// A default <see cref="OpenPgpContext"/> has not been registered.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// A public key for one or more of the <paramref name="recipients"/> could not be found.
		/// </exception>
		public static async Task<MultipartEncrypted> EncryptAsync (IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (OpenPgpContext) CryptographyContext.Create ("application/pgp-encrypted"))
				return await EncryptAsync (ctx, recipients, entity, cancellationToken).ConfigureAwait (false);
		}

		static async Task<MultipartEncrypted> EncryptAsync (OpenPgpContext ctx, EncryptionAlgorithm algorithm, IEnumerable<PgpPublicKey> recipients, MimeEntity entity, bool doAsync, CancellationToken cancellationToken)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				using (var filtered = new FilteredStream (memory)) {
					filtered.Add (new Unix2DosFilter ());

					if (doAsync) {
						await entity.WriteToAsync (filtered, cancellationToken).ConfigureAwait (false);
						await filtered.FlushAsync (cancellationToken).ConfigureAwait (false);
					} else {
						entity.WriteTo (filtered, cancellationToken);
						filtered.Flush (cancellationToken);
					}
				}

				memory.Position = 0;

				MimePart part;

				if (doAsync)
					part = await ctx.EncryptAsync (algorithm, recipients, memory, cancellationToken).ConfigureAwait (false);
				else
					part = ctx.Encrypt (algorithm, recipients, memory, cancellationToken);

				return CreateMultipartEncrypted (ctx, part);
			}
		}

		/// <summary>
		/// Create a multipart/encrypted MIME part by encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients, encapsulating the result in a
		/// new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the encrypted version of the specified entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for encrypting.</param>
		/// <param name="algorithm">The encryption algorithm.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// One or more of the recipient keys cannot be used for encrypting.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The specified encryption algorithm is not supported.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public static MultipartEncrypted Encrypt (OpenPgpContext ctx, EncryptionAlgorithm algorithm, IEnumerable<PgpPublicKey> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			return EncryptAsync (ctx, algorithm, recipients, entity, false, cancellationToken).GetAwaiter ().GetResult ();
		}

		/// <summary>
		/// Asynchronously create a multipart/encrypted MIME part by encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients, encapsulating the result in a
		/// new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the encrypted version of the specified entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for encrypting.</param>
		/// <param name="algorithm">The encryption algorithm.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// One or more of the recipient keys cannot be used for encrypting.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The specified encryption algorithm is not supported.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public static Task<MultipartEncrypted> EncryptAsync (OpenPgpContext ctx, EncryptionAlgorithm algorithm, IEnumerable<PgpPublicKey> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			return EncryptAsync (ctx, algorithm, recipients, entity, true, cancellationToken);
		}

		static async Task<MultipartEncrypted> EncryptAsync (OpenPgpContext ctx, IEnumerable<PgpPublicKey> recipients, MimeEntity entity, bool doAsync, CancellationToken cancellationToken)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				using (var filtered = new FilteredStream (memory)) {
					filtered.Add (new Unix2DosFilter ());

					if (doAsync) {
						await entity.WriteToAsync (filtered, cancellationToken).ConfigureAwait (false);
						await filtered.FlushAsync (cancellationToken).ConfigureAwait (false);
					} else {
						entity.WriteTo (filtered, cancellationToken);
						filtered.Flush (cancellationToken);
					}
				}

				memory.Position = 0;

				MimePart part;

				if (doAsync)
					part = await ctx.EncryptAsync (recipients, memory, cancellationToken).ConfigureAwait (false);
				else
					part = ctx.Encrypt (recipients, memory, cancellationToken);

				return CreateMultipartEncrypted (ctx, part);
			}
		}

		/// <summary>
		/// Create a multipart/encrypted MIME part by encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients, encapsulating the result in a
		/// new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the encrypted version of the specified entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for encrypting.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// One or more of the recipient keys cannot be used for encrypting.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public static MultipartEncrypted Encrypt (OpenPgpContext ctx, IEnumerable<PgpPublicKey> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			return EncryptAsync (ctx, recipients, entity, false, cancellationToken).GetAwaiter ().GetResult ();
		}

		/// <summary>
		/// Asynchronously create a multipart/encrypted MIME part by encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients, encapsulating the result in a
		/// new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the encrypted version of the specified entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for encrypting.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// One or more of the recipient keys cannot be used for encrypting.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public static Task<MultipartEncrypted> EncryptAsync (OpenPgpContext ctx, IEnumerable<PgpPublicKey> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			return EncryptAsync (ctx, recipients, entity, true, cancellationToken);
		}

		/// <summary>
		/// Create a multipart/encrypted MIME part by encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients, encapsulating the result in a
		/// new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the encrypted version of the specified entity.</returns>
		/// <param name="algorithm">The encryption algorithm.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// One or more of the recipient keys cannot be used for encrypting.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <para>A default <see cref="OpenPgpContext"/> has not been registered.</para>
		/// <para>-or-</para>
		/// <para>The specified encryption algorithm is not supported.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public static MultipartEncrypted Encrypt (EncryptionAlgorithm algorithm, IEnumerable<PgpPublicKey> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (OpenPgpContext) CryptographyContext.Create ("application/pgp-encrypted"))
				return Encrypt (ctx, algorithm, recipients, entity, cancellationToken);
		}

		/// <summary>
		/// Asynchronously create a multipart/encrypted MIME part by encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients, encapsulating the result in a
		/// new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the encrypted version of the specified entity.</returns>
		/// <param name="algorithm">The encryption algorithm.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// One or more of the recipient keys cannot be used for encrypting.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <para>A default <see cref="OpenPgpContext"/> has not been registered.</para>
		/// <para>-or-</para>
		/// <para>The specified encryption algorithm is not supported.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public static async Task<MultipartEncrypted> EncryptAsync (EncryptionAlgorithm algorithm, IEnumerable<PgpPublicKey> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (OpenPgpContext) CryptographyContext.Create ("application/pgp-encrypted"))
				return await EncryptAsync (ctx, algorithm, recipients, entity, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Create a multipart/encrypted MIME part by encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients, encapsulating the result in a
		/// new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the encrypted version of the specified entity.</returns>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// One or more of the recipient keys cannot be used for encrypting.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// A default <see cref="OpenPgpContext"/> has not been registered.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public static MultipartEncrypted Encrypt (IEnumerable<PgpPublicKey> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (OpenPgpContext) CryptographyContext.Create ("application/pgp-encrypted"))
				return Encrypt (ctx, recipients, entity, cancellationToken);
		}

		/// <summary>
		/// Asynchronously create a multipart/encrypted MIME part by encrypting the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients, encapsulating the result in a
		/// new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the encrypted version of the specified entity.</returns>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// One or more of the recipient keys cannot be used for encrypting.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// A default <see cref="OpenPgpContext"/> has not been registered.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public static async Task<MultipartEncrypted> EncryptAsync (IEnumerable<PgpPublicKey> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (OpenPgpContext) CryptographyContext.Create ("application/pgp-encrypted"))
				return await EncryptAsync (ctx, recipients, entity, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Decrypts the <see cref="MultipartEncrypted"/> part.
		/// </summary>
		/// <remarks>
		/// Decrypts the <see cref="MultipartEncrypted"/> and extracts any digital signatures in cases
		/// where the content was also signed.
		/// </remarks>
		/// <returns>The decrypted entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for decrypting.</param>
		/// <param name="signatures">A list of digital signatures if the data was both signed and encrypted.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// <para>The <c>protocol</c> parameter was not specified.</para>
		/// <para>-or-</para>
		/// <para>The multipart is malformed in some way.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The provided <see cref="OpenPgpContext"/> does not support the protocol parameter.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found to decrypt the encrypted data.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="MultipartEncrypted"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// <para>The user chose to cancel the password prompt.</para>
		/// <para>-or-</para>
		/// <para>The operation was cancelled via the cancellation token.</para>
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public MimeEntity Decrypt (OpenPgpContext ctx, out DigitalSignatureCollection signatures, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			CheckDisposed ();

			var protocol = ContentType.Parameters["protocol"]?.Trim ();
			if (string.IsNullOrEmpty (protocol))
				throw new FormatException ();

			if (!ctx.Supports (protocol))
				throw new NotSupportedException ();

			if (Count < 2)
				throw new FormatException ();

			if (this[0] is not MimePart version)
				throw new FormatException ();

			var ctype = version.ContentType;
			var value = string.Format ("{0}/{1}", ctype.MediaType, ctype.MediaSubtype);
			if (!value.Equals (protocol, StringComparison.OrdinalIgnoreCase))
				throw new FormatException ();

			if (this[1] is not MimePart encrypted || encrypted.Content == null)
				throw new FormatException ();

			if (!encrypted.ContentType.IsMimeType ("application", "octet-stream"))
				throw new FormatException ();

			using (var memory = new MemoryBlockStream ()) {
				encrypted.Content.DecodeTo (memory, cancellationToken);
				memory.Position = 0;

				return ctx.Decrypt (memory, out signatures, cancellationToken);
			}
		}

		/// <summary>
		/// Decrypts the <see cref="MultipartEncrypted"/> part.
		/// </summary>
		/// <remarks>
		/// Decrypts the <see cref="MultipartEncrypted"/> part.
		/// </remarks>
		/// <returns>The decrypted entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for decrypting.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// <para>The <c>protocol</c> parameter was not specified.</para>
		/// <para>-or-</para>
		/// <para>The multipart is malformed in some way.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The provided <see cref="OpenPgpContext"/> does not support the protocol parameter.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found to decrypt the encrypted data.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="MultipartEncrypted"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// <para>The user chose to cancel the password prompt.</para>
		/// <para>-or-</para>
		/// <para>The operation was cancelled via the cancellation token.</para>
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public MimeEntity Decrypt (OpenPgpContext ctx, CancellationToken cancellationToken = default)
		{
			return Decrypt (ctx, out _, cancellationToken);
		}

		/// <summary>
		/// Decrypts the <see cref="MultipartEncrypted"/> part.
		/// </summary>
		/// <remarks>
		/// Decrypts the <see cref="MultipartEncrypted"/> and extracts any digital signatures in cases
		/// where the content was also signed.
		/// </remarks>
		/// <returns>The decrypted entity.</returns>
		/// <param name="signatures">A list of digital signatures if the data was both signed and encrypted.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.FormatException">
		/// <para>The <c>protocol</c> parameter was not specified.</para>
		/// <para>-or-</para>
		/// <para>The multipart is malformed in some way.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// A suitable <see cref="CryptographyContext"/> for
		/// decrypting could not be found.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found to decrypt the encrypted data.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="MultipartEncrypted"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// <para>The user chose to cancel the password prompt.</para>
		/// <para>-or-</para>
		/// <para>The operation was cancelled via the cancellation token.</para>
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public MimeEntity Decrypt (out DigitalSignatureCollection signatures, CancellationToken cancellationToken = default)
		{
			CheckDisposed ();

			var protocol = ContentType.Parameters["protocol"]?.Trim ();
			if (string.IsNullOrEmpty (protocol))
				throw new FormatException ();

			if (Count < 2)
				throw new FormatException ();

			if (this[0] is not MimePart version)
				throw new FormatException ();

			var ctype = version.ContentType;
			var value = string.Format ("{0}/{1}", ctype.MediaType, ctype.MediaSubtype);
			if (!value.Equals (protocol, StringComparison.OrdinalIgnoreCase))
				throw new FormatException ();

			if (this[1] is not MimePart encrypted || encrypted.Content == null)
				throw new FormatException ();

			if (!encrypted.ContentType.IsMimeType ("application", "octet-stream"))
				throw new FormatException ();

			using (var ctx = CryptographyContext.Create (protocol)) {
				using (var memory = new MemoryBlockStream ()) {
					encrypted.Content.DecodeTo (memory, cancellationToken);
					memory.Position = 0;

					if (ctx is OpenPgpContext pgp)
						return pgp.Decrypt (memory, out signatures, cancellationToken);

					signatures = null;

					return ctx.Decrypt (memory, cancellationToken);
				}
			}
		}

		/// <summary>
		/// Decrypts the <see cref="MultipartEncrypted"/> part.
		/// </summary>
		/// <remarks>
		/// Decrypts the <see cref="MultipartEncrypted"/> part.
		/// </remarks>
		/// <returns>The decrypted entity.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.FormatException">
		/// <para>The <c>protocol</c> parameter was not specified.</para>
		/// <para>-or-</para>
		/// <para>The multipart is malformed in some way.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// A suitable <see cref="CryptographyContext"/> for
		/// decrypting could not be found.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found to decrypt the encrypted data.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="MultipartEncrypted"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// <para>The user chose to cancel the password prompt.</para>
		/// <para>-or-</para>
		/// <para>The operation was cancelled via the cancellation token.</para>
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public MimeEntity Decrypt (CancellationToken cancellationToken = default)
		{
			return Decrypt (out _, cancellationToken);
		}
	}
}

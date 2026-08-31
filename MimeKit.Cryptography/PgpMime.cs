//
// PgpMime.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
//

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using MimeKit;
using MimeKit.IO;
using MimeKit.IO.Filters;

using Org.BouncyCastle.Bcpg.OpenPgp;

namespace MimeKit.Cryptography {
	/// <summary>
	/// Cryptography helper methods.
	/// </summary>
	public static class PgpMime
	{
		static async Task<MultipartSigned> SignAsync (OpenPgpContext ctx, PgpSecretKey signer, DigestAlgorithm digestAlgo, MimeEntity entity, bool doAsync, CancellationToken cancellationToken)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				MimeEntity prepared;

				if (doAsync)
					prepared = await MultipartSigned.PrepareAsync (ctx, entity, memory, cancellationToken).ConfigureAwait (false);
				else
					prepared = MultipartSigned.Prepare (ctx, entity, memory, cancellationToken);

				memory.Position = 0;

				// sign the cleartext content
				MimePart signature;

				if (doAsync)
					signature = await ctx.SignAsync (signer, digestAlgo, memory, cancellationToken).ConfigureAwait (false);
				else
					signature = ctx.Sign (signer, digestAlgo, memory, cancellationToken);

				return MultipartSigned.Create (ctx, digestAlgo, prepared, signature);
			}
		}

		/// <summary>
		/// Create a new <see cref="MultipartSigned"/>.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs the entity using the supplied signer and digest algorithm in
		/// order to generate a detached signature and then adds the entity along with the
		/// detached signature data to a new multipart/signed part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartSigned"/> instance.</returns>
		/// <param name="ctx">The OpenPGP context to use for signing.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="entity">The entity to sign.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="signer"/> cannot be used for signing.
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
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Bcpg.OpenPgp.PgpException">
		/// An error occurred in the OpenPGP subsystem.
		/// </exception>
		public static MultipartSigned Sign (OpenPgpContext ctx, PgpSecretKey signer, DigestAlgorithm digestAlgo, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			return SignAsync (ctx, signer, digestAlgo, entity, false, cancellationToken).GetAwaiter ().GetResult ();
		}

		/// <summary>
		/// Asynchronously create a new <see cref="MultipartSigned"/>.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs the entity using the supplied signer and digest algorithm in
		/// order to generate a detached signature and then adds the entity along with the
		/// detached signature data to a new multipart/signed part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartSigned"/> instance.</returns>
		/// <param name="ctx">The OpenPGP context to use for signing.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="entity">The entity to sign.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="signer"/> cannot be used for signing.
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
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Bcpg.OpenPgp.PgpException">
		/// An error occurred in the OpenPGP subsystem.
		/// </exception>
		public static Task<MultipartSigned> SignAsync (OpenPgpContext ctx, PgpSecretKey signer, DigestAlgorithm digestAlgo, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			return SignAsync (ctx, signer, digestAlgo, entity, true, cancellationToken);
		}

		/// <summary>
		/// Create a new <see cref="MultipartSigned"/>.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs the entity using the supplied signer and digest algorithm in
		/// order to generate a detached signature and then adds the entity along with the
		/// detached signature data to a new multipart/signed part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartSigned"/> instance.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="entity">The entity to sign.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="signer"/> cannot be used for signing.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <para>A cryptography context suitable for signing could not be found.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="digestAlgo"/> is not supported.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Bcpg.OpenPgp.PgpException">
		/// An error occurred in the OpenPGP subsystem.
		/// </exception>
		public static MultipartSigned Sign (PgpSecretKey signer, DigestAlgorithm digestAlgo, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			using (var ctx = (OpenPgpContext) CryptographyContext.Create ("application/pgp-signature"))
				return Sign (ctx, signer, digestAlgo, entity, cancellationToken);
		}

		/// <summary>
		/// Asynchronously create a new <see cref="MultipartSigned"/>.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs the entity using the supplied signer and digest algorithm in
		/// order to generate a detached signature and then adds the entity along with the
		/// detached signature data to a new multipart/signed part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartSigned"/> instance.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="entity">The entity to sign.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="signer"/> cannot be used for signing.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <para>A cryptography context suitable for signing could not be found.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="digestAlgo"/> is not supported.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Bcpg.OpenPgp.PgpException">
		/// An error occurred in the OpenPGP subsystem.
		/// </exception>
		public static async Task<MultipartSigned> SignAsync (PgpSecretKey signer, DigestAlgorithm digestAlgo, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			using (var ctx = (OpenPgpContext) CryptographyContext.Create ("application/pgp-signature"))
				return await SignAsync (ctx, signer, digestAlgo, entity, cancellationToken).ConfigureAwait (false);
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
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
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
		/// <paramref name="ctx"/> is <see langword="null"/>.
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
	}
}

//
// SecureMime.cs
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

using Org.BouncyCastle.Bcpg.OpenPgp;

namespace MimeKit.Cryptography {
	/// <summary>
	/// Cryptography helper methods.
	/// </summary>
	public static class SecureMime
	{
		static async Task<MultipartSigned> SignAsync (SecureMimeContext ctx, CmsSigner signer, MimeEntity entity, bool doAsync, CancellationToken cancellationToken)
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
					signature = await ctx.SignAsync (signer, memory, cancellationToken).ConfigureAwait (false);
				else
					signature = ctx.Sign (signer, memory, cancellationToken);

				return MultipartSigned.Create (ctx, signer.DigestAlgorithm, prepared, signature);
			}
		}

		/// <summary>
		/// Create a new <see cref="MultipartSigned"/>.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs the entity using the supplied signer in order
		/// to generate a detached signature and then adds the entity along with
		/// the detached signature data to a new multipart/signed part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartSigned"/> instance.</returns>
		/// <param name="ctx">The S/MIME context to use for signing.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="entity">The entity to sign.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static MultipartSigned Sign (SecureMimeContext ctx, CmsSigner signer, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			return SignAsync (ctx, signer, entity, false, cancellationToken).GetAwaiter ().GetResult ();
		}

		/// <summary>
		/// Asynchronously create a new <see cref="MultipartSigned"/>.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs the entity using the supplied signer in order
		/// to generate a detached signature and then adds the entity along with
		/// the detached signature data to a new multipart/signed part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartSigned"/> instance.</returns>
		/// <param name="ctx">The S/MIME context to use for signing.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="entity">The entity to sign.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static Task<MultipartSigned> SignAsync (SecureMimeContext ctx, CmsSigner signer, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			return SignAsync (ctx, signer, entity, true, cancellationToken);
		}

		/// <summary>
		/// Create a new <see cref="MultipartSigned"/>.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs the entity using the supplied signer in order
		/// to generate a detached signature and then adds the entity along with
		/// the detached signature data to a new multipart/signed part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartSigned"/> instance.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="entity">The entity to sign.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// A cryptography context suitable for signing could not be found.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static MultipartSigned Sign (CmsSigner signer, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-signature"))
				return Sign (ctx, signer, entity, cancellationToken);
		}

		/// <summary>
		/// Asynchronously create a new <see cref="MultipartSigned"/>.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs the entity using the supplied signer in order
		/// to generate a detached signature and then adds the entity along with
		/// the detached signature data to a new multipart/signed part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartSigned"/> instance.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="entity">The entity to sign.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// A cryptography context suitable for signing could not be found.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<MultipartSigned> SignAsync (CmsSigner signer, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-signature"))
				return await SignAsync (ctx, signer, entity, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Prepare the MIME entity for transport using the specified encoding constraints.
		/// </summary>
		/// <remarks>
		/// Prepares the MIME entity for transport using the specified encoding constraints.
		/// </remarks>
		/// <param name="constraint">The encoding constraint.</param>
		/// <param name="maxLineLength">The maximum number of octets allowed per line (not counting the CRLF). Must be between <c>60</c> and <c>998</c> (inclusive).</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="maxLineLength"/> is not between <c>60</c> and <c>998</c> (inclusive).</para>
		/// <para>-or-</para>
		/// <para><paramref name="constraint"/> is not a valid value.</para>
		/// </exception>

		static async Task<ApplicationPkcs7Mime> CompressAsync (SecureMimeContext ctx, MimeEntity entity, bool doAsync, CancellationToken cancellationToken)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

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

				if (doAsync)
					return await ctx.CompressAsync (memory, cancellationToken).ConfigureAwait (false);

				return ctx.Compress (memory, cancellationToken);
			}
		}

		/// <summary>
		/// Compress the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Compresses the specified entity using the specified <see cref="SecureMimeContext"/>.</para>
		/// <note type="warning">Most mail clients, even among those that support S/MIME, do not support compression.</note>
		/// </remarks>
		/// <returns>The compressed entity.</returns>
		/// <param name="ctx">The S/MIME context to use for compressing.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime Compress (SecureMimeContext ctx, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			return CompressAsync (ctx, entity, false, cancellationToken).GetAwaiter ().GetResult ();
		}

		/// <summary>
		/// Asynchronously compress the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously compresses the specified entity using the specified <see cref="SecureMimeContext"/>.</para>
		/// <note type="warning">Most mail clients, even among those that support S/MIME, do not support compression.</note>
		/// </remarks>
		/// <returns>The compressed entity.</returns>
		/// <param name="ctx">The S/MIME context to use for compressing.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static Task<ApplicationPkcs7Mime> CompressAsync (SecureMimeContext ctx, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			return CompressAsync (ctx, entity, true, cancellationToken);
		}

		/// <summary>
		/// Compress the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Compresses the specified entity using the default <see cref="SecureMimeContext"/>.</para>
		/// <note type="warning">Most mail clients, even among those that support S/MIME, do not support compression.</note>
		/// </remarks>
		/// <returns>The compressed entity.</returns>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="entity"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime Compress (MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return Compress (ctx, entity, cancellationToken);
		}

		/// <summary>
		/// Asynchronously compress the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously compresses the specified entity using the default <see cref="SecureMimeContext"/>.</para>
		/// <note type="warning">Most mail clients, even among those that support S/MIME, do not support compression.</note>
		/// </remarks>
		/// <returns>The compressed entity.</returns>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="entity"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<ApplicationPkcs7Mime> CompressAsync (MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return await CompressAsync (ctx, entity, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients using the supplied <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The encrypted entity.</returns>
		/// <param name="ctx">The S/MIME context to use for encrypting.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
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
		/// <exception cref="CmsEnvelopeException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime Encrypt (SecureMimeContext ctx, CmsRecipientCollection recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				entity.WriteTo (options, memory, cancellationToken);
				memory.Position = 0;

				return ctx.Encrypt (recipients, memory, cancellationToken);
			}
		}

		/// <summary>
		/// Asynchronously encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Asynchronously encrypts the entity to the specified recipients using the supplied <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The encrypted entity.</returns>
		/// <param name="ctx">The S/MIME context to use for encrypting.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
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
		/// <exception cref="CmsEnvelopeException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<ApplicationPkcs7Mime> EncryptAsync (SecureMimeContext ctx, CmsRecipientCollection recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				await entity.WriteToAsync (options, memory, cancellationToken).ConfigureAwait (false);
				memory.Position = 0;

				return await ctx.EncryptAsync (recipients, memory, cancellationToken).ConfigureAwait (false);
			}
		}

		/// <summary>
		/// Encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients using the default <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The encrypted entity.</returns>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
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
		/// <exception cref="CmsEnvelopeException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime Encrypt (CmsRecipientCollection recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return Encrypt (ctx, recipients, entity, cancellationToken);
		}

		/// <summary>
		/// Asynchronously encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Asynchronously encrypts the entity to the specified recipients using the default <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The encrypted entity.</returns>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
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
		/// <exception cref="CmsEnvelopeException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<ApplicationPkcs7Mime> EncryptAsync (CmsRecipientCollection recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return await EncryptAsync (ctx, recipients, entity, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients using the supplied <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The encrypted entity.</returns>
		/// <param name="ctx">The S/MIME context to use for encrypting.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// Valid certificates could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		/// <exception cref="CmsEnvelopeException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime Encrypt (SecureMimeContext ctx, IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				entity.WriteTo (options, memory, cancellationToken);
				memory.Position = 0;

				return (ApplicationPkcs7Mime) ctx.Encrypt (recipients, memory, cancellationToken);
			}
		}

		/// <summary>
		/// Asynchronously encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Asynchronously encrypts the entity to the specified recipients using the supplied <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The encrypted entity.</returns>
		/// <param name="ctx">The S/MIME context to use for encrypting.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// Valid certificates could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		/// <exception cref="CmsEnvelopeException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<ApplicationPkcs7Mime> EncryptAsync (SecureMimeContext ctx, IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				await entity.WriteToAsync (options, memory, cancellationToken).ConfigureAwait (false);
				memory.Position = 0;

				return (ApplicationPkcs7Mime) await ctx.EncryptAsync (recipients, memory, cancellationToken).ConfigureAwait (false);
			}
		}

		/// <summary>
		/// Encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients using the default <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The encrypted entity.</returns>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// Valid certificates could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		/// <exception cref="CmsEnvelopeException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime Encrypt (IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return Encrypt (ctx, recipients, entity, cancellationToken);
		}

		/// <summary>
		/// Asynchronously encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Asynchronously encrypts the entity to the specified recipients using the default <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The encrypted entity.</returns>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// Valid certificates could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		/// <exception cref="CmsEnvelopeException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<ApplicationPkcs7Mime> EncryptAsync (IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return await EncryptAsync (ctx, recipients, entity, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Sign the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Signs the entity using the supplied signer and <see cref="SecureMimeContext"/>.</para>
		/// <para>For better interoperability with other mail clients, you should use
		/// <see cref="MultipartSigned.Create(SecureMimeContext, CmsSigner, MimeEntity,CancellationToken)"/>
		/// instead as the multipart/signed format is supported among a much larger
		/// subset of mail client software.</para>
		/// </remarks>
		/// <returns>The signed entity.</returns>
		/// <param name="ctx">The S/MIME context to use for signing.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime EncapsulatedSign (SecureMimeContext ctx, CmsSigner signer, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				entity.WriteTo (options, memory, cancellationToken);
				memory.Position = 0;

				return ctx.EncapsulatedSign (signer, memory, cancellationToken);
			}
		}

		/// <summary>
		/// Asynchronously sign the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously signs the entity using the supplied signer and <see cref="SecureMimeContext"/>.</para>
		/// <para>For better interoperability with other mail clients, you should use
		/// <see cref="MultipartSigned.CreateAsync(SecureMimeContext, CmsSigner, MimeEntity,CancellationToken)"/>
		/// instead as the multipart/signed format is supported among a much larger
		/// subset of mail client software.</para>
		/// </remarks>
		/// <returns>The signed entity.</returns>
		/// <param name="ctx">The S/MIME context to use for signing.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<ApplicationPkcs7Mime> EncapsulatedSignAsync (SecureMimeContext ctx, CmsSigner signer, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				await entity.WriteToAsync (options, memory, cancellationToken).ConfigureAwait (false);
				memory.Position = 0;

				return await ctx.EncapsulatedSignAsync (signer, memory, cancellationToken).ConfigureAwait (false);
			}
		}

		/// <summary>
		/// Sign the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Signs the entity using the supplied signer and the default <see cref="SecureMimeContext"/>.</para>
		/// <para>For better interoperability with other mail clients, you should use
		/// <see cref="MultipartSigned.Create(SecureMimeContext, CmsSigner, MimeEntity,CancellationToken)"/>
		/// instead as the multipart/signed format is supported among a much larger
		/// subset of mail client software.</para>
		/// </remarks>
		/// <returns>The signed entity.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime EncapsulatedSign (CmsSigner signer, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return EncapsulatedSign (ctx, signer, entity, cancellationToken);
		}

		/// <summary>
		/// Asynchronously sign the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously signs the entity using the supplied signer and the default <see cref="SecureMimeContext"/>.</para>
		/// <para>For better interoperability with other mail clients, you should use
		/// <see cref="MultipartSigned.CreateAsync(SecureMimeContext, CmsSigner, MimeEntity,CancellationToken)"/>
		/// instead as the multipart/signed format is supported among a much larger
		/// subset of mail client software.</para>
		/// </remarks>
		/// <returns>The signed entity.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<ApplicationPkcs7Mime> EncapsulatedSignAsync (CmsSigner signer, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return await EncapsulatedSignAsync (ctx, signer, entity, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Sign the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Signs the entity using the supplied signer, digest algorithm and <see cref="SecureMimeContext"/>.</para>
		/// <para>For better interoperability with other mail clients, you should use
		/// <see cref="MultipartSigned.Create(SecureMimeContext, CmsSigner, MimeEntity,CancellationToken)"/>
		/// instead as the multipart/signed format is supported among a much larger
		/// subset of mail client software.</para>
		/// </remarks>
		/// <returns>The signed entity.</returns>
		/// <param name="ctx">The S/MIME context to use for signing.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for <paramref name="signer"/>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime EncapsulatedSign (SecureMimeContext ctx, MailboxAddress signer, DigestAlgorithm digestAlgo, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				entity.WriteTo (options, memory, cancellationToken);
				memory.Position = 0;

				return ctx.EncapsulatedSign (signer, digestAlgo, memory, cancellationToken);
			}
		}

		/// <summary>
		/// Asynchronously sign the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously signs the entity using the supplied signer, digest algorithm and <see cref="SecureMimeContext"/>.</para>
		/// <para>For better interoperability with other mail clients, you should use
		/// <see cref="MultipartSigned.Create(SecureMimeContext, CmsSigner, MimeEntity,CancellationToken)"/>
		/// instead as the multipart/signed format is supported among a much larger
		/// subset of mail client software.</para>
		/// </remarks>
		/// <returns>The signed entity.</returns>
		/// <param name="ctx">The S/MIME context to use for signing.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for <paramref name="signer"/>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<ApplicationPkcs7Mime> EncapsulatedSignAsync (SecureMimeContext ctx, MailboxAddress signer, DigestAlgorithm digestAlgo, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				await entity.WriteToAsync (options, memory, cancellationToken).ConfigureAwait (false);
				memory.Position = 0;

				return await ctx.EncapsulatedSignAsync (signer, digestAlgo, memory, cancellationToken).ConfigureAwait (false);
			}
		}

		/// <summary>
		/// Sign the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Signs the entity using the supplied signer, digest algorithm and the default
		/// <see cref="SecureMimeContext"/>.</para>
		/// <para>For better interoperability with other mail clients, you should use
		/// <see cref="MultipartSigned.Create(SecureMimeContext, CmsSigner, MimeEntity,CancellationToken)"/>
		/// instead as the multipart/signed format is supported among a much larger
		/// subset of mail client software.</para>
		/// </remarks>
		/// <returns>The signed entity.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for <paramref name="signer"/>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime EncapsulatedSign (MailboxAddress signer, DigestAlgorithm digestAlgo, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return EncapsulatedSign (ctx, signer, digestAlgo, entity, cancellationToken);
		}

		/// <summary>
		/// Asynchronously sign the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously signs the entity using the supplied signer, digest algorithm and the default
		/// <see cref="SecureMimeContext"/>.</para>
		/// <para>For better interoperability with other mail clients, you should use
		/// <see cref="MultipartSigned.CreateAsync(SecureMimeContext, CmsSigner, MimeEntity,CancellationToken)"/>
		/// instead as the multipart/signed format is supported among a much larger
		/// subset of mail client software.</para>
		/// </remarks>
		/// <returns>The signed entity.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for <paramref name="signer"/>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<ApplicationPkcs7Mime> EncapsulatedSignAsync (MailboxAddress signer, DigestAlgorithm digestAlgo, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return await EncapsulatedSignAsync (ctx, signer, digestAlgo, entity, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Sign and encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Signs entity using the supplied signer and then encrypts the result to the specified recipients.
		/// </remarks>
		/// <returns>The signed and encrypted entity.</returns>
		/// <param name="ctx">The S/MIME context to use for signing and encrypting.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
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
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime SignAndEncrypt (SecureMimeContext ctx, CmsSigner signer, CmsRecipientCollection recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			var signed = SecureMime.Sign (ctx, signer, entity, cancellationToken);

			return Encrypt (ctx, recipients, signed, cancellationToken);
		}

		/// <summary>
		/// Asynchronously sign and encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Asynchronously signs entity using the supplied signer and then encrypts the result to the specified recipients.
		/// </remarks>
		/// <returns>The signed and encrypted entity.</returns>
		/// <param name="ctx">The S/MIME context to use for signing and encrypting.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
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
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<ApplicationPkcs7Mime> SignAndEncryptAsync (SecureMimeContext ctx, CmsSigner signer, CmsRecipientCollection recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			var signed = await SecureMime.SignAsync (ctx, signer, entity, cancellationToken).ConfigureAwait (false);

			return await EncryptAsync (ctx, recipients, signed, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Sign and encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Signs entity using the supplied signer and the default <see cref="SecureMimeContext"/>
		/// and then encrypts the result to the specified recipients.
		/// </remarks>
		/// <returns>The signed and encrypted entity.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
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
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime SignAndEncrypt (CmsSigner signer, CmsRecipientCollection recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return SignAndEncrypt (ctx, signer, recipients, entity, cancellationToken);
		}

		/// <summary>
		/// Asynchroinously sign and encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Asynchronously signs entity using the supplied signer and the default <see cref="SecureMimeContext"/>
		/// and then encrypts the result to the specified recipients.
		/// </remarks>
		/// <returns>The signed and encrypted entity.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
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
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<ApplicationPkcs7Mime> SignAndEncryptAsync (CmsSigner signer, CmsRecipientCollection recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return await SignAndEncryptAsync (ctx, signer, recipients, entity, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Sign and encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Signs entity using the supplied signer and then encrypts the result to the specified recipients.
		/// </remarks>
		/// <returns>The signed and encrypted entity.</returns>
		/// <param name="ctx">The S/MIME context to use for signing and encrypting.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
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
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// <para>A signing certificate could not be found for <paramref name="signer"/>.</para>
		/// <para>-or-</para>
		/// <para>A certificate could not be found for one or more of the <paramref name="recipients"/>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime SignAndEncrypt (SecureMimeContext ctx, MailboxAddress signer, DigestAlgorithm digestAlgo, IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			var signed = MultipartSigned.Create (ctx, signer, digestAlgo, entity, cancellationToken);

			return Encrypt (ctx, recipients, signed, cancellationToken);
		}

		/// <summary>
		/// Asynchronously sign and encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Asynchronously signs entity using the supplied signer and then encrypts the result to the specified recipients.
		/// </remarks>
		/// <returns>The signed and encrypted entity.</returns>
		/// <param name="ctx">The S/MIME context to use for signing and encrypting.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
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
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// <para>A signing certificate could not be found for <paramref name="signer"/>.</para>
		/// <para>-or-</para>
		/// <para>A certificate could not be found for one or more of the <paramref name="recipients"/>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<ApplicationPkcs7Mime> SignAndEncryptAsync (SecureMimeContext ctx, MailboxAddress signer, DigestAlgorithm digestAlgo, IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			var signed = await MultipartSigned.CreateAsync (ctx, signer, digestAlgo, entity, cancellationToken).ConfigureAwait (false);

			return await EncryptAsync (ctx, recipients, signed, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Sign and encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Signs entity using the supplied signer and the default <see cref="SecureMimeContext"/>
		/// and then encrypts the result to the specified recipients.
		/// </remarks>
		/// <returns>The signed and encrypted entity.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
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
		/// <exception cref="CertificateNotFoundException">
		/// <para>A signing certificate could not be found for <paramref name="signer"/>.</para>
		/// <para>-or-</para>
		/// <para>A certificate could not be found for one or more of the <paramref name="recipients"/>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime SignAndEncrypt (MailboxAddress signer, DigestAlgorithm digestAlgo, IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return SignAndEncrypt (ctx, signer, digestAlgo, recipients, entity, cancellationToken);
		}

		/// <summary>
		/// Asynchronously sign and encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Asynchronously signs entity using the supplied signer and the default <see cref="SecureMimeContext"/>
		/// and then encrypts the result to the specified recipients.
		/// </remarks>
		/// <returns>The signed and encrypted entity.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <see langword="null"/>.</para>
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
		/// <exception cref="CertificateNotFoundException">
		/// <para>A signing certificate could not be found for <paramref name="signer"/>.</para>
		/// <para>-or-</para>
		/// <para>A certificate could not be found for one or more of the <paramref name="recipients"/>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<ApplicationPkcs7Mime> SignAndEncryptAsync (MailboxAddress signer, DigestAlgorithm digestAlgo, IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return await SignAndEncryptAsync (ctx, signer, digestAlgo, recipients, entity, cancellationToken).ConfigureAwait (false);
		}
	}
}

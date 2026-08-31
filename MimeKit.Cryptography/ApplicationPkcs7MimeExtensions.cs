//
// ApplicationPkcs7MimeExtensions.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;

using MimeKit;
using MimeKit.IO;

namespace MimeKit.Cryptography {
	/// <summary>
	/// Cryptography helper methods.
	/// </summary>
	public static class ApplicationPkcs7MimeExtensions
	{
		/// <summary>
		/// Decompress the compressed-data.
		/// </summary>
		/// <remarks>
		/// Decompresses the compressed-data using the specified <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The decompressed <see cref="MimeEntity"/>.</returns>
		/// <param name="ctx">The S/MIME context to use for decompressing.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the application.Content-Type header is not "compressed-data".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static MimeEntity Decompress (this ApplicationPkcs7Mime application, SecureMimeContext ctx, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			application.CheckDisposed (nameof (ApplicationPkcs7Mime));

			if (application.SecureMimeType != SecureMimeType.CompressedData && application.SecureMimeType != SecureMimeType.Unknown)
				throw new InvalidOperationException ();

			using (var memory = new MemoryBlockStream ()) {
				if (application.Content != null) {
					application.Content.DecodeTo (memory, cancellationToken);
					memory.Position = 0;
				}

				return ctx.Decompress (memory, cancellationToken);
			}
		}

		/// <summary>
		/// Asynchronously decompress the compressed-data.
		/// </summary>
		/// <remarks>
		/// Asynchronously decompresses the compressed-data using the specified <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The decompressed <see cref="MimeEntity"/>.</returns>
		/// <param name="ctx">The S/MIME context to use for decompressing.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the application.Content-Type header is not "compressed-data".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<MimeEntity> DecompressAsync (this ApplicationPkcs7Mime application, SecureMimeContext ctx, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			application.CheckDisposed (nameof (ApplicationPkcs7Mime));

			if (application.SecureMimeType != SecureMimeType.CompressedData && application.SecureMimeType != SecureMimeType.Unknown)
				throw new InvalidOperationException ();

			using (var memory = new MemoryBlockStream ()) {
				if (application.Content != null) {
					await application.Content.DecodeToAsync (memory, cancellationToken).ConfigureAwait (false);
					memory.Position = 0;
				}

				return await ctx.DecompressAsync (memory, cancellationToken).ConfigureAwait (false);
			}
		}

		/// <summary>
		/// Decompress the compressed-data.
		/// </summary>
		/// <remarks>
		/// Decompresses the compressed-data using the default <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The decompressed <see cref="MimeEntity"/>.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the application.Content-Type header is not "compressed-data".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static MimeEntity Decompress (this ApplicationPkcs7Mime application, CancellationToken cancellationToken = default)
		{
			application.CheckDisposed (nameof (ApplicationPkcs7Mime));

			if (application.SecureMimeType != SecureMimeType.CompressedData && application.SecureMimeType != SecureMimeType.Unknown)
				throw new InvalidOperationException ();

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return application.Decompress (ctx, cancellationToken);
		}

		/// <summary>
		/// Asynchronously decompress the compressed-data.
		/// </summary>
		/// <remarks>
		/// Asynchronously decompresses the compressed-data using the default <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The decompressed <see cref="MimeEntity"/>.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the application.Content-Type header is not "compressed-data".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<MimeEntity> DecompressAsync (this ApplicationPkcs7Mime application, CancellationToken cancellationToken = default)
		{
			application.CheckDisposed (nameof (ApplicationPkcs7Mime));

			if (application.SecureMimeType != SecureMimeType.CompressedData && application.SecureMimeType != SecureMimeType.Unknown)
				throw new InvalidOperationException ();

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return await application.DecompressAsync (ctx, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Decrypt the enveloped-data.
		/// </summary>
		/// <remarks>
		/// Decrypts the enveloped-data using the specified <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The decrypted <see cref="MimeEntity"/>.</returns>
		/// <param name="ctx">The S/MIME context to use for decrypting.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the application.Content-Type header is not "enveloped-data".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static MimeEntity Decrypt (this ApplicationPkcs7Mime application, SecureMimeContext ctx, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			application.CheckDisposed (nameof (ApplicationPkcs7Mime));

			if (application.SecureMimeType != SecureMimeType.EnvelopedData && application.SecureMimeType != SecureMimeType.Unknown)
				throw new InvalidOperationException ();

			using (var memory = new MemoryBlockStream ()) {
				if (application.Content != null) {
					application.Content.DecodeTo (memory, cancellationToken);
					memory.Position = 0;
				}

				return ctx.Decrypt (memory, cancellationToken);
			}
		}

		/// <summary>
		/// Asynchronously decrypt the enveloped-data.
		/// </summary>
		/// <remarks>
		/// Asynchronously decrypts the enveloped-data using the specified <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The decrypted <see cref="MimeEntity"/>.</returns>
		/// <param name="ctx">The S/MIME context to use for decrypting.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the application.Content-Type header is not "enveloped-data".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<MimeEntity> DecryptAsync (this ApplicationPkcs7Mime application, SecureMimeContext ctx, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			application.CheckDisposed (nameof (ApplicationPkcs7Mime));

			if (application.SecureMimeType != SecureMimeType.EnvelopedData && application.SecureMimeType != SecureMimeType.Unknown)
				throw new InvalidOperationException ();

			using (var memory = new MemoryBlockStream ()) {
				if (application.Content != null) {
					await application.Content.DecodeToAsync (memory, cancellationToken).ConfigureAwait (false);
					memory.Position = 0;
				}

				return await ctx.DecryptAsync (memory, cancellationToken).ConfigureAwait (false);
			}
		}

		/// <summary>
		/// Decrypt the enveloped-data.
		/// </summary>
		/// <remarks>
		/// Decrypts the enveloped-data using the default <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The decrypted <see cref="MimeEntity"/>.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the application.Content-Type header is not "certs-only".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static MimeEntity Decrypt (this ApplicationPkcs7Mime application, CancellationToken cancellationToken = default)
		{
			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return application.Decrypt (ctx, cancellationToken);
		}

		/// <summary>
		/// Asynchronously decrypt the enveloped-data.
		/// </summary>
		/// <remarks>
		/// Asynchronously decrypts the enveloped-data using the default <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The decrypted <see cref="MimeEntity"/>.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the application.Content-Type header is not "certs-only".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<MimeEntity> DecryptAsync (this ApplicationPkcs7Mime application, CancellationToken cancellationToken = default)
		{
			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return await application.DecryptAsync (ctx, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Import the certificates contained in the application/pkcs7-mime content.
		/// </summary>
		/// <remarks>
		/// Imports the certificates contained in the application/pkcs7-mime content.
		/// </remarks>
		/// <param name="ctx">The S/MIME context to import certificates into.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the application.Content-Type header is not "certs-only".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static void Import (this ApplicationPkcs7Mime application, SecureMimeContext ctx, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			application.CheckDisposed (nameof (ApplicationPkcs7Mime));

			if (application.SecureMimeType != SecureMimeType.CertsOnly && application.SecureMimeType != SecureMimeType.Unknown)
				throw new InvalidOperationException ();

			if (application.Content is null)
				return;

			using (var memory = new MemoryBlockStream ()) {
				application.Content.DecodeTo (memory, cancellationToken);
				memory.Position = 0;

				ctx.Import (memory, cancellationToken);
			}
		}

		/// <summary>
		/// Asynchronously import the certificates contained in the application/pkcs7-mime content.
		/// </summary>
		/// <remarks>
		/// Asynchronously imports the certificates contained in the application/pkcs7-mime content.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="ctx">The S/MIME context to import certificates into.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the application.Content-Type header is not "certs-only".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task ImportAsync (this ApplicationPkcs7Mime application, SecureMimeContext ctx, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			application.CheckDisposed (nameof (ApplicationPkcs7Mime));

			if (application.SecureMimeType != SecureMimeType.CertsOnly && application.SecureMimeType != SecureMimeType.Unknown)
				throw new InvalidOperationException ();

			if (application.Content is null)
				return;

			using (var memory = new MemoryBlockStream ()) {
				await application.Content.DecodeToAsync (memory, cancellationToken).ConfigureAwait (false);
				memory.Position = 0;

				await ctx.ImportAsync (memory, cancellationToken).ConfigureAwait (false);
			}
		}

		/// <summary>
		/// Verify the signed-data and return the unencapsulated <see cref="MimeEntity"/>.
		/// </summary>
		/// <remarks>
		/// Verifies the signed-data and returns the unencapsulated <see cref="MimeEntity"/>.
		/// </remarks>
		/// <returns>The list of digital signatures.</returns>
		/// <param name="ctx">The S/MIME context to use for verifying the signature.</param>
		/// <param name="entity">The unencapsulated entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the application.Content-Type header is not "signed-data".
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The extracted content could not be parsed as a MIME entity.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static DigitalSignatureCollection Verify (this ApplicationPkcs7Mime application, SecureMimeContext ctx, out MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			application.CheckDisposed (nameof (ApplicationPkcs7Mime));

			if (application.SecureMimeType != SecureMimeType.SignedData && application.SecureMimeType != SecureMimeType.Unknown)
				throw new InvalidOperationException ();

			using (var memory = new MemoryBlockStream ()) {
				if (application.Content != null) {
					application.Content.DecodeTo (memory, cancellationToken);
					memory.Position = 0;
				}

				return ctx.Verify (memory, out entity, cancellationToken);
			}
		}

		/// <summary>
		/// Verifies the signed-data and returns the unencapsulated <see cref="MimeEntity"/>.
		/// </summary>
		/// <remarks>
		/// Verifies the signed-data using the default <see cref="SecureMimeContext"/> and returns the
		/// unencapsulated <see cref="MimeEntity"/>.
		/// </remarks>
		/// <returns>The list of digital signatures.</returns>
		/// <param name="entity">The unencapsulated entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the application.Content-Type header is not "signed-data".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static DigitalSignatureCollection Verify (this ApplicationPkcs7Mime application, out MimeEntity entity, CancellationToken cancellationToken = default)
		{
			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return application.Verify (ctx, out entity, cancellationToken);
		}
	}
}

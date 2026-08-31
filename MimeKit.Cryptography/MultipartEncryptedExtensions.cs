//
// MultipartEncryptedExtensions.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
//

using System;
using System.Threading;
using System.Collections.Generic;

using MimeKit;
using MimeKit.IO;

using Org.BouncyCastle.Bcpg.OpenPgp;

namespace MimeKit.Cryptography {
	/// <summary>
	/// Cryptography helper methods.
	/// </summary>
	public static class MultipartEncryptedExtensions
	{
		public static MimeEntity Decrypt (this MultipartEncrypted multipart, OpenPgpContext ctx, out DigitalSignatureCollection? signatures, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			multipart.CheckDisposed (nameof (MultipartEncrypted));

			var protocol = multipart.ContentType.Parameters["protocol"]?.Trim ();
			if (string.IsNullOrEmpty (protocol))
				throw new FormatException ();

			if (!ctx.Supports (protocol))
				throw new NotSupportedException ();

			if (multipart.Count < 2)
				throw new FormatException ();

			if (multipart[0] is not MimePart version)
				throw new FormatException ();

			var ctype = version.ContentType;
			var value = string.Format ("{0}/{1}", ctype.MediaType, ctype.MediaSubtype);
			if (!value.Equals (protocol, StringComparison.OrdinalIgnoreCase))
				throw new FormatException ();

			if (multipart[1] is not MimePart encrypted || encrypted.Content == null)
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
		public static MimeEntity Decrypt (this MultipartEncrypted multipart, OpenPgpContext ctx, CancellationToken cancellationToken = default)
		{
			return multipart.Decrypt (ctx, out _, cancellationToken);
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
		public static MimeEntity Decrypt (this MultipartEncrypted multipart, out DigitalSignatureCollection? signatures, CancellationToken cancellationToken = default)
		{
			multipart.CheckDisposed (nameof (MultipartEncrypted));

			var protocol = multipart.ContentType.Parameters["protocol"]?.Trim ();
			if (string.IsNullOrEmpty (protocol))
				throw new FormatException ();

			if (multipart.Count < 2)
				throw new FormatException ();

			if (multipart[0] is not MimePart version)
				throw new FormatException ();

			var ctype = version.ContentType;
			var value = string.Format ("{0}/{1}", ctype.MediaType, ctype.MediaSubtype);
			if (!value.Equals (protocol, StringComparison.OrdinalIgnoreCase))
				throw new FormatException ();

			if (multipart[1] is not MimePart encrypted || encrypted.Content == null)
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
		public static MimeEntity Decrypt (this MultipartEncrypted multipart, CancellationToken cancellationToken = default)
		{
			return multipart.Decrypt (out _, cancellationToken);
		}
	}
}

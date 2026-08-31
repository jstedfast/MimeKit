//
// MimeMessageCryptographyExtensions.cs
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
using MimeKit.IO.Filters;

namespace MimeKit.Cryptography {
	/// <summary>
	/// Cryptography helper methods.
	/// </summary>
	public static class MimeMessageCryptographyExtensions
	{
		internal static byte[] HashBody (this MimeMessage message, FormatOptions options, DkimSignatureAlgorithm signatureAlgorithm, DkimCanonicalizationAlgorithm bodyCanonicalizationAlgorithm, int maxLength)
		{
			using (var stream = new DkimHashStream (signatureAlgorithm, maxLength)) {
				using (var filtered = new FilteredStream (stream)) {
					DkimBodyFilter dkim;

					if (bodyCanonicalizationAlgorithm == DkimCanonicalizationAlgorithm.Relaxed)
						dkim = new DkimRelaxedBodyFilter ();
					else
						dkim = new DkimSimpleBodyFilter ();

					filtered.Add (options.CreateNewLineFilter ());
					filtered.Add (dkim);

					if (message.Body != null) {
						try {
							message.Body.EnsureNewLine = message.compliance == RfcComplianceMode.Strict || options.EnsureNewLine;
							message.Body.WriteTo (options, filtered, true, CancellationToken.None);
						} finally {
							message.Body.EnsureNewLine = false;
						}
					}

					filtered.Flush ();

					if (!dkim.LastWasNewLine)
						stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
				}

				return stream.GenerateHash ();
			}
		}

		/// <summary>
		/// Sign the message using the specified cryptography context and digest algorithm.
		/// </summary>
		/// <remarks>
		/// If either of the Resent-Sender or Resent-From headers are set, then the message
		/// will be signed using the Resent-Sender (or first mailbox in the Resent-From)
		/// address as the signer address, otherwise the Sender or From address will be
		/// used instead.
		/// </remarks>
		/// <param name="ctx">The cryptography context.</param>
		/// <param name="digestAlgo">The digest algorithm.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="message.Body"/> has not been set.</para>
		/// <para>-or-</para>
		/// <para>A sender has not been specified.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The <paramref name="digestAlgo"/> is not supported.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for the sender.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found for the sender.
		/// </exception>
		public static void Sign (this MimeMessage message, CryptographyContext ctx, DigestAlgorithm digestAlgo, CancellationToken cancellationToken = default)
		{
			if (ctx is null)
				throw new ArgumentNullException (nameof (ctx));

			if (message.Body is null)
				throw new InvalidOperationException ("No message body has been set.");

			var signer = message.GetMessageSigner () ?? throw new InvalidOperationException ("The sender has not been set.");
			message.Body = MultipartSigned.Create (ctx, signer, digestAlgo, message.Body, cancellationToken);
		}

		/// <summary>
		/// Asynchronously sign the message using the specified cryptography context and digest algorithm.
		/// </summary>
		/// <remarks>
		/// If either of the Resent-Sender or Resent-From headers are set, then the message
		/// will be signed using the Resent-Sender (or first mailbox in the Resent-From)
		/// address as the signer address, otherwise the Sender or From address will be
		/// used instead.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="ctx">The cryptography context.</param>
		/// <param name="digestAlgo">The digest algorithm.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="message.Body"/> has not been set.</para>
		/// <para>-or-</para>
		/// <para>A sender has not been specified.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The <paramref name="digestAlgo"/> is not supported.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for the sender.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found for the sender.
		/// </exception>
		public static async Task SignAsync (this MimeMessage message, CryptographyContext ctx, DigestAlgorithm digestAlgo, CancellationToken cancellationToken = default)
		{
			if (ctx is null)
				throw new ArgumentNullException (nameof (ctx));

			if (message.Body is null)
				throw new InvalidOperationException ("No message body has been set.");

			var signer = message.GetMessageSigner () ?? throw new InvalidOperationException ("The sender has not been set.");
			message.Body = await MultipartSigned.CreateAsync (ctx, signer, digestAlgo, message.Body, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Sign the message using the specified cryptography context and the SHA-1 digest algorithm.
		/// </summary>
		/// <remarks>
		/// If either of the Resent-Sender or Resent-From headers are set, then the message
		/// will be signed using the Resent-Sender (or first mailbox in the Resent-From)
		/// address as the signer address, otherwise the Sender or From address will be
		/// used instead.
		/// </remarks>
		/// <param name="ctx">The cryptography context.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="message.Body"/> has not been set.</para>
		/// <para>-or-</para>
		/// <para>A sender has not been specified.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for the sender.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found for the sender.
		/// </exception>
		public static void Sign (this MimeMessage message, CryptographyContext ctx, CancellationToken cancellationToken = default)
		{
			message.Sign (ctx, DigestAlgorithm.Sha1, cancellationToken);
		}

		/// <summary>
		/// Asynchronously sign the message using the specified cryptography context and the SHA-1 digest algorithm.
		/// </summary>
		/// <remarks>
		/// If either of the Resent-Sender or Resent-From headers are set, then the message
		/// will be signed using the Resent-Sender (or first mailbox in the Resent-From)
		/// address as the signer address, otherwise the Sender or From address will be
		/// used instead.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="ctx">The cryptography context.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="message.Body"/> has not been set.</para>
		/// <para>-or-</para>
		/// <para>A sender has not been specified.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for the sender.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found for the sender.
		/// </exception>
		public static Task SignAsync (this MimeMessage message, CryptographyContext ctx, CancellationToken cancellationToken = default)
		{
			return message.SignAsync (ctx, DigestAlgorithm.Sha1, cancellationToken);
		}

		/// <summary>
		/// Encrypt the message to the sender and all the recipients
		/// using the specified cryptography context.
		/// </summary>
		/// <remarks>
		/// If either of the Resent-Sender or Resent-From headers are set, then the message
		/// will be encrypted to all the addresses specified in the Resent headers
		/// (Resent-Sender, Resent-From, Resent-To, Resent-Cc, and Resent-Bcc),
		/// otherwise the message will be encrypted to all the addresses specified in
		/// the standard address headers (Sender, From, To, Cc, and Bcc).
		/// </remarks>
		/// <param name="ctx">The cryptography context.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// An unknown type of cryptography context was used.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="message.Body"/> has not been set.</para>
		/// <para>-or-</para>
		/// <para>No recipients have been specified.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for one or more of the recipients.
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// The public key could not be found for one or more of the recipients.
		/// </exception>
		public static void Encrypt (this MimeMessage message, CryptographyContext ctx, CancellationToken cancellationToken = default)
		{
			if (ctx is null)
				throw new ArgumentNullException (nameof (ctx));

			if (message.Body is null)
				throw new InvalidOperationException ("No message body has been set.");

			var recipients = message.GetEncryptionRecipients ();
			if (recipients.Count == 0)
				throw new InvalidOperationException ("No recipients have been set.");

			if (ctx is SecureMimeContext smime) {
				message.Body = SecureMime.Encrypt (smime, recipients, message.Body, cancellationToken);
			} else if (ctx is OpenPgpContext pgp) {
				message.Body = PgpMime.Encrypt (pgp, recipients, message.Body, cancellationToken);
			} else {
				throw new ArgumentException ("Unknown type of cryptography context.", nameof (ctx));
			}
		}

		/// <summary>
		/// Asynchronously encrypt the message to the sender and all the recipients
		/// using the specified cryptography context.
		/// </summary>
		/// <remarks>
		/// If either of the Resent-Sender or Resent-From headers are set, then the message
		/// will be encrypted to all the addresses specified in the Resent headers
		/// (Resent-Sender, Resent-From, Resent-To, Resent-Cc, and Resent-Bcc),
		/// otherwise the message will be encrypted to all the addresses specified in
		/// the standard address headers (Sender, From, To, Cc, and Bcc).
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="ctx">The cryptography context.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// An unknown type of cryptography context was used.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="message.Body"/> has not been set.</para>
		/// <para>-or-</para>
		/// <para>No recipients have been specified.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for one or more of the recipients.
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// The public key could not be found for one or more of the recipients.
		/// </exception>
		public static async Task EncryptAsync (this MimeMessage message, CryptographyContext ctx, CancellationToken cancellationToken = default)
		{
			if (ctx is null)
				throw new ArgumentNullException (nameof (ctx));

			if (message.Body is null)
				throw new InvalidOperationException ("No message body has been set.");

			var recipients = message.GetEncryptionRecipients ();
			if (recipients.Count == 0)
				throw new InvalidOperationException ("No recipients have been set.");

			if (ctx is SecureMimeContext smime) {
				message.Body = await SecureMime.EncryptAsync (smime, recipients, message.Body, cancellationToken).ConfigureAwait (false);
			} else if (ctx is OpenPgpContext pgp) {
				message.Body = await PgpMime.EncryptAsync (pgp, recipients, message.Body, cancellationToken).ConfigureAwait (false);
			} else {
				throw new ArgumentException ("Unknown type of cryptography context.", nameof (ctx));
			}
		}

		/// <summary>
		/// Sign and encrypt the message to the sender and all the recipients using
		/// the specified cryptography context and the specified digest algorithm.
		/// </summary>
		/// <remarks>
		/// <para>If either of the Resent-Sender or Resent-From headers are set, then the message
		/// will be signed using the Resent-Sender (or first mailbox in the Resent-From)
		/// address as the signer address, otherwise the Sender or From address will be
		/// used instead.</para>
		/// <para>Likewise, if either of the Resent-Sender or Resent-From headers are set, then the
		/// message will be encrypted to all the addresses specified in the Resent headers
		/// (Resent-Sender, Resent-From, Resent-To, Resent-Cc, and Resent-Bcc),
		/// otherwise the message will be encrypted to all the addresses specified in
		/// the standard address headers (Sender, From, To, Cc, and Bcc).</para>
		/// </remarks>
		/// <param name="ctx">The cryptography context.</param>
		/// <param name="digestAlgo">The digest algorithm.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// An unknown type of cryptography context was used.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="message.Body"/> has not been set.</para>
		/// <para>-or-</para>
		/// <para>No sender has been specified.</para>
		/// <para>-or-</para>
		/// <para>No recipients have been specified.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The <paramref name="digestAlgo"/> is not supported.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for the signer or one or more of the recipients.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found for the sender.
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// The public key could not be found for one or more of the recipients.
		/// </exception>
		public static void SignAndEncrypt (this MimeMessage message, CryptographyContext ctx, DigestAlgorithm digestAlgo, CancellationToken cancellationToken = default)
		{
			if (ctx is null)
				throw new ArgumentNullException (nameof (ctx));

			if (message.Body is null)
				throw new InvalidOperationException ("No message body has been set.");

			var signer = message.GetMessageSigner () ?? throw new InvalidOperationException ("The sender has not been set.");
			var recipients = message.GetEncryptionRecipients ();

			if (ctx is SecureMimeContext smime) {
				message.Body = SecureMime.SignAndEncrypt (smime, signer, digestAlgo, recipients, message.Body, cancellationToken);
			} else if (ctx is OpenPgpContext pgp) {
				message.Body = PgpMime.SignAndEncrypt (pgp, signer, digestAlgo, recipients, message.Body, cancellationToken);
			} else {
				throw new ArgumentException ("Unknown type of cryptography context.", nameof (ctx));
			}
		}

		/// <summary>
		/// Asynchronously sign and encrypt the message to the sender and all the recipients using
		/// the specified cryptography context and the specified digest algorithm.
		/// </summary>
		/// <remarks>
		/// <para>If either of the Resent-Sender or Resent-From headers are set, then the message
		/// will be signed using the Resent-Sender (or first mailbox in the Resent-From)
		/// address as the signer address, otherwise the Sender or From address will be
		/// used instead.</para>
		/// <para>Likewise, if either of the Resent-Sender or Resent-From headers are set, then the
		/// message will be encrypted to all the addresses specified in the Resent headers
		/// (Resent-Sender, Resent-From, Resent-To, Resent-Cc, and Resent-Bcc),
		/// otherwise the message will be encrypted to all the addresses specified in
		/// the standard address headers (Sender, From, To, Cc, and Bcc).</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="ctx">The cryptography context.</param>
		/// <param name="digestAlgo">The digest algorithm.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// An unknown type of cryptography context was used.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="message.Body"/> has not been set.</para>
		/// <para>-or-</para>
		/// <para>No sender has been specified.</para>
		/// <para>-or-</para>
		/// <para>No recipients have been specified.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The <paramref name="digestAlgo"/> is not supported.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for the signer or one or more of the recipients.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found for the sender.
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// The public key could not be found for one or more of the recipients.
		/// </exception>
		public static async Task SignAndEncryptAsync (this MimeMessage message, CryptographyContext ctx, DigestAlgorithm digestAlgo, CancellationToken cancellationToken = default)
		{
			if (ctx is null)
				throw new ArgumentNullException (nameof (ctx));

			if (message.Body is null)
				throw new InvalidOperationException ("No message body has been set.");

			var signer = message.GetMessageSigner () ?? throw new InvalidOperationException ("The sender has not been set.");
			var recipients = message.GetEncryptionRecipients ();

			if (ctx is SecureMimeContext smime) {
				message.Body = await SecureMime.SignAndEncryptAsync (smime, signer, digestAlgo, recipients, message.Body, cancellationToken).ConfigureAwait (false);
			} else if (ctx is OpenPgpContext pgp) {
				message.Body = await PgpMime.SignAndEncryptAsync (pgp, signer, digestAlgo, recipients, message.Body, cancellationToken).ConfigureAwait (false);
			} else {
				throw new ArgumentException ("Unknown type of cryptography context.", nameof (ctx));
			}
		}

		/// <summary>
		/// Sign and encrypt the message to the sender and all the recipients using
		/// the specified cryptography context and the SHA-1 digest algorithm.
		/// </summary>
		/// <remarks>
		/// <para>If either of the Resent-Sender or Resent-From headers are set, then the message
		/// will be signed using the Resent-Sender (or first mailbox in the Resent-From)
		/// address as the signer address, otherwise the Sender or From address will be
		/// used instead.</para>
		/// <para>Likewise, if either of the Resent-Sender or Resent-From headers are set, then the
		/// message will be encrypted to all the addresses specified in the Resent headers
		/// (Resent-Sender, Resent-From, Resent-To, Resent-Cc, and Resent-Bcc),
		/// otherwise the message will be encrypted to all the addresses specified in
		/// the standard address headers (Sender, From, To, Cc, and Bcc).</para>
		/// </remarks>
		/// <param name="ctx">The cryptography context.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// An unknown type of cryptography context was used.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="message.Body"/> has not been set.</para>
		/// <para>-or-</para>
		/// <para>No sender has been specified.</para>
		/// <para>-or-</para>
		/// <para>No recipients have been specified.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for the signer or one or more of the recipients.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found for the sender.
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// The public key could not be found for one or more of the recipients.
		/// </exception>
		public static void SignAndEncrypt (this MimeMessage message, CryptographyContext ctx, CancellationToken cancellationToken = default)
		{
			message.SignAndEncrypt (ctx, DigestAlgorithm.Sha1, cancellationToken);
		}

		/// <summary>
		/// Asynchronously sign and encrypt the message to the sender and all the recipients using
		/// the specified cryptography context and the SHA-1 digest algorithm.
		/// </summary>
		/// <remarks>
		/// <para>If either of the Resent-Sender or Resent-From headers are set, then the message
		/// will be signed using the Resent-Sender (or first mailbox in the Resent-From)
		/// address as the signer address, otherwise the Sender or From address will be
		/// used instead.</para>
		/// <para>Likewise, if either of the Resent-Sender or Resent-From headers are set, then the
		/// message will be encrypted to all the addresses specified in the Resent headers
		/// (Resent-Sender, Resent-From, Resent-To, Resent-Cc, and Resent-Bcc),
		/// otherwise the message will be encrypted to all the addresses specified in
		/// the standard address headers (Sender, From, To, Cc, and Bcc).</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="ctx">The cryptography context.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// An unknown type of cryptography context was used.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="message.Body"/> has not been set.</para>
		/// <para>-or-</para>
		/// <para>No sender has been specified.</para>
		/// <para>-or-</para>
		/// <para>No recipients have been specified.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for the signer or one or more of the recipients.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found for the sender.
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// The public key could not be found for one or more of the recipients.
		/// </exception>
		public static Task SignAndEncryptAsync (this MimeMessage message, CryptographyContext ctx, CancellationToken cancellationToken = default)
		{
			return message.SignAndEncryptAsync (ctx, DigestAlgorithm.Sha1, cancellationToken);
		}
	}
}

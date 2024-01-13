//
// MimeMessageCryptographyExtensions.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2023 .NET Foundation and Contributors
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using MimeKit.IO;

namespace MimeKit.Cryptography {
	/// <summary>
	/// Extension methods for <see cref="MimeMessage"/> that add convenient S/MIME and DKIM support.
	/// </summary>
	/// <remarks>
	/// Extension methods for <see cref="MimeMessage"/> that add convenient S/MIME and DKIM support.
	/// </remarks>
	public static class MimeMessageCryptographyExtensions
	{
		static MailboxAddress GetMessageSigner (MimeMessage message)
		{
			if (message.ResentSender != null)
				return message.ResentSender;

			if (message.ResentFrom.Count > 0)
				return message.ResentFrom.Mailboxes.FirstOrDefault ();

			if (message.Sender != null)
				return message.Sender;

			return message.From.Mailboxes.FirstOrDefault ();
		}

		static IList<MailboxAddress> GetEncryptionRecipients (MimeMessage message)
		{
			return message.GetMailboxes (true, true);
		}

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
							message.Body.EnsureNewLine = message.Compliance == RfcComplianceMode.Strict || options.EnsureNewLine;
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
		/// <param name="message">The message to sign.</param>
		/// <param name="ctx">The cryptography context.</param>
		/// <param name="digestAlgo">The digest algorithm.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The message <see cref="MimeMessage.Body"/> has not been set.</para>
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

			var signer = GetMessageSigner (message) ?? throw new InvalidOperationException ("The sender has not been set.");
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
		/// <param name="message">The message to sign.</param>
		/// <param name="ctx">The cryptography context.</param>
		/// <param name="digestAlgo">The digest algorithm.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The message <see cref="MimeMessage.Body"/> has not been set.</para>
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

			var signer = GetMessageSigner (message) ?? throw new InvalidOperationException ("The sender has not been set.");
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
		/// <param name="message">The message to sign.</param>
		/// <param name="ctx">The cryptography context.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The message <see cref="MimeMessage.Body"/> has not been set.</para>
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
			Sign (message, ctx, DigestAlgorithm.Sha1, cancellationToken);
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
		/// <param name="message">The message to sign.</param>
		/// <param name="ctx">The cryptography context.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The message <see cref="MimeMessage.Body"/> has not been set.</para>
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
			return SignAsync (message, ctx, DigestAlgorithm.Sha1, cancellationToken);
		}

		/// <summary>
		/// Encrypt the message to the sender and all of the recipients
		/// using the specified cryptography context.
		/// </summary>
		/// <remarks>
		/// If either of the Resent-Sender or Resent-From headers are set, then the message
		/// will be encrypted to all of the addresses specified in the Resent headers
		/// (Resent-Sender, Resent-From, Resent-To, Resent-Cc, and Resent-Bcc),
		/// otherwise the message will be encrypted to all of the addresses specified in
		/// the standard address headers (Sender, From, To, Cc, and Bcc).
		/// </remarks>
		/// <param name="message">The message to encrypt.</param>
		/// <param name="ctx">The cryptography context.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// An unknown type of cryptography context was used.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The message <see cref="MimeMessage.Body"/> has not been set.</para>
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

			var recipients = GetEncryptionRecipients (message);
			if (recipients.Count == 0)
				throw new InvalidOperationException ("No recipients have been set.");

			if (ctx is SecureMimeContext smime) {
				message.Body = ApplicationPkcs7Mime.Encrypt (smime, recipients, message.Body, cancellationToken);
			} else if (ctx is OpenPgpContext pgp) {
				message.Body = MultipartEncrypted.Encrypt (pgp, recipients, message.Body, cancellationToken);
			} else {
				throw new ArgumentException ("Unknown type of cryptography context.", nameof (ctx));
			}
		}

		/// <summary>
		/// Asynchronously encrypt the message to the sender and all of the recipients
		/// using the specified cryptography context.
		/// </summary>
		/// <remarks>
		/// If either of the Resent-Sender or Resent-From headers are set, then the message
		/// will be encrypted to all of the addresses specified in the Resent headers
		/// (Resent-Sender, Resent-From, Resent-To, Resent-Cc, and Resent-Bcc),
		/// otherwise the message will be encrypted to all of the addresses specified in
		/// the standard address headers (Sender, From, To, Cc, and Bcc).
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="message">The message to encrypt.</param>
		/// <param name="ctx">The cryptography context.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// An unknown type of cryptography context was used.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The message <see cref="MimeMessage.Body"/> has not been set.</para>
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

			var recipients = GetEncryptionRecipients (message);
			if (recipients.Count == 0)
				throw new InvalidOperationException ("No recipients have been set.");

			if (ctx is SecureMimeContext smime) {
				message.Body = await ApplicationPkcs7Mime.EncryptAsync (smime, recipients, message.Body, cancellationToken).ConfigureAwait (false);
			} else if (ctx is OpenPgpContext pgp) {
				message.Body = await MultipartEncrypted.EncryptAsync (pgp, recipients, message.Body, cancellationToken).ConfigureAwait (false);
			} else {
				throw new ArgumentException ("Unknown type of cryptography context.", nameof (ctx));
			}
		}

		/// <summary>
		/// Sign and encrypt the message to the sender and all of the recipients using
		/// the specified cryptography context and the specified digest algorithm.
		/// </summary>
		/// <remarks>
		/// <para>If either of the Resent-Sender or Resent-From headers are set, then the message
		/// will be signed using the Resent-Sender (or first mailbox in the Resent-From)
		/// address as the signer address, otherwise the Sender or From address will be
		/// used instead.</para>
		/// <para>Likewise, if either of the Resent-Sender or Resent-From headers are set, then the
		/// message will be encrypted to all of the addresses specified in the Resent headers
		/// (Resent-Sender, Resent-From, Resent-To, Resent-Cc, and Resent-Bcc),
		/// otherwise the message will be encrypted to all of the addresses specified in
		/// the standard address headers (Sender, From, To, Cc, and Bcc).</para>
		/// </remarks>
		/// <param name="message">The message to sign and encrypt.</param>
		/// <param name="ctx">The cryptography context.</param>
		/// <param name="digestAlgo">The digest algorithm.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// An unknown type of cryptography context was used.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The message <see cref="MimeMessage.Body"/> has not been set.</para>
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

			var signer = GetMessageSigner (message) ?? throw new InvalidOperationException ("The sender has not been set.");
			var recipients = GetEncryptionRecipients (message);

			if (ctx is SecureMimeContext smime) {
				message.Body = ApplicationPkcs7Mime.SignAndEncrypt (smime, signer, digestAlgo, recipients, message.Body, cancellationToken);
			} else if (ctx is OpenPgpContext pgp) {
				message.Body = MultipartEncrypted.SignAndEncrypt (pgp, signer, digestAlgo, recipients, message.Body, cancellationToken);
			} else {
				throw new ArgumentException ("Unknown type of cryptography context.", nameof (ctx));
			}
		}

		/// <summary>
		/// Asynchronously sign and encrypt the message to the sender and all of the recipients using
		/// the specified cryptography context and the specified digest algorithm.
		/// </summary>
		/// <remarks>
		/// <para>If either of the Resent-Sender or Resent-From headers are set, then the message
		/// will be signed using the Resent-Sender (or first mailbox in the Resent-From)
		/// address as the signer address, otherwise the Sender or From address will be
		/// used instead.</para>
		/// <para>Likewise, if either of the Resent-Sender or Resent-From headers are set, then the
		/// message will be encrypted to all of the addresses specified in the Resent headers
		/// (Resent-Sender, Resent-From, Resent-To, Resent-Cc, and Resent-Bcc),
		/// otherwise the message will be encrypted to all of the addresses specified in
		/// the standard address headers (Sender, From, To, Cc, and Bcc).</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="message">The message to sign and encrypt.</param>
		/// <param name="ctx">The cryptography context.</param>
		/// <param name="digestAlgo">The digest algorithm.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// An unknown type of cryptography context was used.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The message <see cref="MimeMessage.Body"/> has not been set.</para>
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

			var signer = GetMessageSigner (message) ?? throw new InvalidOperationException ("The sender has not been set.");
			var recipients = GetEncryptionRecipients (message);

			if (ctx is SecureMimeContext smime) {
				message.Body = await ApplicationPkcs7Mime.SignAndEncryptAsync (smime, signer, digestAlgo, recipients, message.Body, cancellationToken).ConfigureAwait (false);
			} else if (ctx is OpenPgpContext pgp) {
				message.Body = await MultipartEncrypted.SignAndEncryptAsync (pgp, signer, digestAlgo, recipients, message.Body, cancellationToken).ConfigureAwait (false);
			} else {
				throw new ArgumentException ("Unknown type of cryptography context.", nameof (ctx));
			}
		}

		/// <summary>
		/// Sign and encrypt the message to the sender and all of the recipients using
		/// the specified cryptography context and the SHA-1 digest algorithm.
		/// </summary>
		/// <remarks>
		/// <para>If either of the Resent-Sender or Resent-From headers are set, then the message
		/// will be signed using the Resent-Sender (or first mailbox in the Resent-From)
		/// address as the signer address, otherwise the Sender or From address will be
		/// used instead.</para>
		/// <para>Likewise, if either of the Resent-Sender or Resent-From headers are set, then the
		/// message will be encrypted to all of the addresses specified in the Resent headers
		/// (Resent-Sender, Resent-From, Resent-To, Resent-Cc, and Resent-Bcc),
		/// otherwise the message will be encrypted to all of the addresses specified in
		/// the standard address headers (Sender, From, To, Cc, and Bcc).</para>
		/// </remarks>
		/// <param name="message">The message to sign and encrypt.</param>
		/// <param name="ctx">The cryptography context.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// An unknown type of cryptography context was used.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The message <see cref="MimeMessage.Body"/> has not been set.</para>
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
			SignAndEncrypt (message, ctx, DigestAlgorithm.Sha1, cancellationToken);
		}

		/// <summary>
		/// Asynchronously sign and encrypt the message to the sender and all of the recipients using
		/// the specified cryptography context and the SHA-1 digest algorithm.
		/// </summary>
		/// <remarks>
		/// <para>If either of the Resent-Sender or Resent-From headers are set, then the message
		/// will be signed using the Resent-Sender (or first mailbox in the Resent-From)
		/// address as the signer address, otherwise the Sender or From address will be
		/// used instead.</para>
		/// <para>Likewise, if either of the Resent-Sender or Resent-From headers are set, then the
		/// message will be encrypted to all of the addresses specified in the Resent headers
		/// (Resent-Sender, Resent-From, Resent-To, Resent-Cc, and Resent-Bcc),
		/// otherwise the message will be encrypted to all of the addresses specified in
		/// the standard address headers (Sender, From, To, Cc, and Bcc).</para>
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="message">The message to sign and encrypt.</param>
		/// <param name="ctx">The cryptography context.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// An unknown type of cryptography context was used.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The message <see cref="MimeMessage.Body"/> has not been set.</para>
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
			return SignAndEncryptAsync (message, ctx, DigestAlgorithm.Sha1, cancellationToken);
		}
	}
}

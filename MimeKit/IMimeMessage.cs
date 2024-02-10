//
// IMimeMessage.cs
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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using MimeKit.Text;

namespace MimeKit {
	/// <summary>
	/// An interface for a MIME message.
	/// </summary>
	/// <remarks>
	/// <para>A message consists of header fields and, optionally, a body.</para>
	/// <para>The body of the message can either be plain text or it can be a
	/// tree of MIME entities such as a text/plain MIME part and a collection
	/// of file attachments.</para>
	/// </remarks>
	public interface IMimeMessage : IDisposable
	{
		/// <summary>
		/// Get the list of headers.
		/// </summary>
		/// <remarks>
		/// <para>Represents the list of headers for a message. Typically, the headers of
		/// a message will contain transmission headers such as From and To along
		/// with metadata headers such as Subject and Date, but may include just
		/// about anything.</para>
		/// <note type="tip">To access any MIME headers such as <see cref="HeaderId.ContentType"/>,
		/// <see cref="HeaderId.ContentDisposition"/>, <see cref="HeaderId.ContentTransferEncoding"/>
		/// or any other <c>Content-*</c> header, you will need to access the
		/// <see cref="MimeEntity.Headers"/> property of the <see cref="Body"/>.
		/// </note>
		/// </remarks>
		/// <value>The list of headers.</value>
		HeaderList Headers {
			get;
		}

		/// <summary>
		/// Get or set the value of the Importance header.
		/// </summary>
		/// <remarks>
		/// Gets or sets the value of the Importance header.
		/// </remarks>
		/// <value>The importance.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is not a valid <see cref="MessageImportance"/>.
		/// </exception>
		MessageImportance Importance {
			get; set;
		}

		/// <summary>
		/// Get or set the value of the Priority header.
		/// </summary>
		/// <remarks>
		/// Gets or sets the value of the Priority header.
		/// </remarks>
		/// <value>The priority.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is not a valid <see cref="MessagePriority"/>.
		/// </exception>
		MessagePriority Priority {
			get; set;
		}

		/// <summary>
		/// Get or set the value of the X-Priority header.
		/// </summary>
		/// <remarks>
		/// Gets or sets the value of the X-Priority header.
		/// </remarks>
		/// <value>The priority.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is not a valid <see cref="MessagePriority"/>.
		/// </exception>
		XMessagePriority XPriority {
			get; set;
		}

		/// <summary>
		/// Get or set the address in the Sender header.
		/// </summary>
		/// <remarks>
		/// The sender may differ from the addresses in <see cref="From"/> if
		/// the message was sent by someone on behalf of someone else.
		/// </remarks>
		/// <value>The address in the Sender header.</value>
		MailboxAddress Sender {
			get; set;
		}

		/// <summary>
		/// Get or set the address in the Resent-Sender header.
		/// </summary>
		/// <remarks>
		/// The resent sender may differ from the addresses in <see cref="ResentFrom"/> if
		/// the message was sent by someone on behalf of someone else.
		/// </remarks>
		/// <value>The address in the Resent-Sender header.</value>
		MailboxAddress ResentSender {
			get; set;
		}

		/// <summary>
		/// Get the list of addresses in the From header.
		/// </summary>
		/// <remarks>
		/// <para>The "From" header specifies the author(s) of the message.</para>
		/// <para>If more than one <see cref="MailboxAddress"/> is added to the
		/// list of "From" addresses, the <see cref="Sender"/> should be set to the
		/// single <see cref="MailboxAddress"/> of the personal actually sending
		/// the message.</para>
		/// </remarks>
		/// <value>The list of addresses in the From header.</value>
		InternetAddressList From {
			get;
		}

		/// <summary>
		/// Get the list of addresses in the Resent-From header.
		/// </summary>
		/// <remarks>
		/// <para>The "Resent-From" header specifies the author(s) of the messagebeing
		/// resent.</para>
		/// <para>If more than one <see cref="MailboxAddress"/> is added to the
		/// list of "Resent-From" addresses, the <see cref="ResentSender"/> should
		/// be set to the single <see cref="MailboxAddress"/> of the personal actually
		/// sending the message.</para>
		/// </remarks>
		/// <value>The list of addresses in the Resent-From header.</value>
		InternetAddressList ResentFrom {
			get;
		}

		/// <summary>
		/// Get the list of addresses in the Reply-To header.
		/// </summary>
		/// <remarks>
		/// <para>When the list of addresses in the Reply-To header is not empty,
		/// it contains the address(es) where the author(s) of the message prefer
		/// that replies be sent.</para>
		/// <para>When the list of addresses in the Reply-To header is empty,
		/// replies should be sent to the mailbox(es) specified in the From
		/// header.</para>
		/// </remarks>
		/// <value>The list of addresses in the Reply-To header.</value>
		InternetAddressList ReplyTo {
			get;
		}

		/// <summary>
		/// Get the list of addresses in the Resent-Reply-To header.
		/// </summary>
		/// <remarks>
		/// <para>When the list of addresses in the Resent-Reply-To header is not empty,
		/// it contains the address(es) where the author(s) of the resent message prefer
		/// that replies be sent.</para>
		/// <para>When the list of addresses in the Resent-Reply-To header is empty,
		/// replies should be sent to the mailbox(es) specified in the Resent-From
		/// header.</para>
		/// </remarks>
		/// <value>The list of addresses in the Resent-Reply-To header.</value>
		InternetAddressList ResentReplyTo {
			get;
		}

		/// <summary>
		/// Get the list of addresses in the To header.
		/// </summary>
		/// <remarks>
		/// The addresses in the To header are the primary recipients of
		/// the message.
		/// </remarks>
		/// <value>The list of addresses in the To header.</value>
		InternetAddressList To {
			get;
		}

		/// <summary>
		/// Get the list of addresses in the Resent-To header.
		/// </summary>
		/// <remarks>
		/// The addresses in the Resent-To header are the primary recipients of
		/// the message.
		/// </remarks>
		/// <value>The list of addresses in the Resent-To header.</value>
		InternetAddressList ResentTo {
			get;
		}

		/// <summary>
		/// Get the list of addresses in the Cc header.
		/// </summary>
		/// <remarks>
		/// The addresses in the Cc header are secondary recipients of the message
		/// and are usually not the individuals being directly addressed in the
		/// content of the message.
		/// </remarks>
		/// <value>The list of addresses in the Cc header.</value>
		InternetAddressList Cc {
			get;
		}

		/// <summary>
		/// Get the list of addresses in the Resent-Cc header.
		/// </summary>
		/// <remarks>
		/// The addresses in the Resent-Cc header are secondary recipients of the message
		/// and are usually not the individuals being directly addressed in the
		/// content of the message.
		/// </remarks>
		/// <value>The list of addresses in the Resent-Cc header.</value>
		InternetAddressList ResentCc {
			get;
		}

		/// <summary>
		/// Get the list of addresses in the Bcc header.
		/// </summary>
		/// <remarks>
		/// Recipients in the Blind-Carbon-Copy list will not be visible to
		/// the other recipients of the message.
		/// </remarks>
		/// <value>The list of addresses in the Bcc header.</value>
		InternetAddressList Bcc {
			get;
		}

		/// <summary>
		/// Get the list of addresses in the Resent-Bcc header.
		/// </summary>
		/// <remarks>
		/// Recipients in the Resent-Bcc list will not be visible to
		/// the other recipients of the message.
		/// </remarks>
		/// <value>The list of addresses in the Resent-Bcc header.</value>
		InternetAddressList ResentBcc {
			get;
		}

		/// <summary>
		/// Get or set the subject of the message.
		/// </summary>
		/// <remarks>
		/// <para>The Subject is typically a short string denoting the topic of the message.</para>
		/// <para>Replies will often use <c>"Re: "</c> followed by the Subject of the original message.</para>
		/// </remarks>
		/// <value>The subject of the message.</value>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		string Subject {
			get; set;
		}

		/// <summary>
		/// Get or set the date of the message.
		/// </summary>
		/// <remarks>
		/// If the date is not explicitly set before the message is written to a stream,
		/// the date will default to the exact moment when it is written to said stream.
		/// </remarks>
		/// <value>The date of the message.</value>
		DateTimeOffset Date {
			get; set;
		}

		/// <summary>
		/// Get or set the Resent-Date of the message.
		/// </summary>
		/// <remarks>
		/// Gets or sets the Resent-Date of the message.
		/// </remarks>
		/// <value>The Resent-Date of the message.</value>
		DateTimeOffset ResentDate {
			get; set;
		}

		/// <summary>
		/// Get the list of references to other messages.
		/// </summary>
		/// <remarks>
		/// The References header contains a chain of Message-Ids back to the
		/// original message that started the thread.
		/// </remarks>
		/// <value>The references.</value>
		MessageIdList References {
			get;
		}

		/// <summary>
		/// Get or set the Message-Id that this message is replying to.
		/// </summary>
		/// <remarks>
		/// If the message is a reply to another message, it will typically
		/// use the In-Reply-To header to specify the Message-Id of the
		/// original message being replied to.
		/// </remarks>
		/// <value>The message id that this message is in reply to.</value>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="value"/> is improperly formatted.
		/// </exception>
		string InReplyTo {
			get; set;
		}

		/// <summary>
		/// Get or set the message identifier.
		/// </summary>
		/// <remarks>
		/// <para>The Message-Id is meant to be a globally unique identifier for
		/// a message.</para>
		/// <para><see cref="Utils.MimeUtils.GenerateMessageId()"/> can be used
		/// to generate this value.</para>
		/// </remarks>
		/// <value>The message identifier.</value>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="value"/> is improperly formatted.
		/// </exception>
		string MessageId {
			get; set;
		}

		/// <summary>
		/// Get or set the Resent-Message-Id header.
		/// </summary>
		/// <remarks>
		/// <para>The Resent-Message-Id is meant to be a globally unique identifier for
		/// a message.</para>
		/// <para><see cref="Utils.MimeUtils.GenerateMessageId()"/> can be used
		/// to generate this value.</para>
		/// </remarks>
		/// <value>The Resent-Message-Id.</value>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="value"/> is improperly formatted.
		/// </exception>
		string ResentMessageId {
			get; set;
		}

		/// <summary>
		/// Get or set the MIME-Version.
		/// </summary>
		/// <remarks>
		/// The MIME-Version header specifies the version of the MIME specification
		/// that the message was created for.
		/// </remarks>
		/// <value>The MIME version.</value>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		Version MimeVersion {
			get; set;
		}

		/// <summary>
		/// Get or set the body of the message.
		/// </summary>
		/// <remarks>
		/// <para>The body of the message can either be plain text or it can be a
		/// tree of MIME entities such as a text/plain MIME part and a collection
		/// of file attachments.</para>
		/// <para>For a convenient way of constructing message bodies, see the
		/// <see cref="BodyBuilder"/> class.</para>
		/// </remarks>
		/// <value>The body of the message.</value>
		MimeEntity Body {
			get; set;
		}

		/// <summary>
		/// Get the text body of the message if it exists.
		/// </summary>
		/// <remarks>
		/// <para>Gets the text content of the first text/plain body part that is found (in depth-first
		/// search order) which is not an attachment.</para>
		/// </remarks>
		/// <value>The text body if it exists; otherwise, <c>null</c>.</value>
		string TextBody {
			get;
		}

		/// <summary>
		/// Get the html body of the message if it exists.
		/// </summary>
		/// <remarks>
		/// <para>Gets the HTML-formatted body of the message if it exists.</para>
		/// </remarks>
		/// <value>The html body if it exists; otherwise, <c>null</c>.</value>
		string HtmlBody {
			get;
		}

		/// <summary>
		/// Get the text body in the specified format.
		/// </summary>
		/// <remarks>
		/// Gets the text body in the specified format, if it exists.
		/// </remarks>
		/// <returns>The text body in the desired format if it exists; otherwise, <c>null</c>.</returns>
		/// <param name="format">The desired text format.</param>
		string GetTextBody (TextFormat format);

		/// <summary>
		/// Get the body parts of the message.
		/// </summary>
		/// <remarks>
		/// Traverses over the MIME tree, enumerating all of the <see cref="MimeEntity"/> objects,
		/// but does not traverse into the bodies of attached messages.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\AttachmentExamples.cs" region="SaveBodyParts" />
		/// </example>
		/// <value>The body parts.</value>
		IEnumerable<MimeEntity> BodyParts {
			get;
		}

		/// <summary>
		/// Get the attachments.
		/// </summary>
		/// <remarks>
		/// Traverses over the MIME tree, enumerating all of the <see cref="MimeEntity"/> objects that
		/// have a Content-Disposition header set to <c>"attachment"</c>.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\AttachmentExamples.cs" region="SaveAttachments" />
		/// </example>
		/// <value>The attachments.</value>
		IEnumerable<MimeEntity> Attachments {
			get;
		}

		/// <summary>
		/// Dispatches to the specific visit method for this MIME message.
		/// </summary>
		/// <remarks>
		/// Dispatches to the specific visit method for this MIME message.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <c>null</c>.
		/// </exception>
		void Accept (MimeVisitor visitor);

		/// <summary>
		/// Prepare the message for transport using the specified encoding constraints.
		/// </summary>
		/// <remarks>
		/// Prepares the message for transport using the specified encoding constraints.
		/// </remarks>
		/// <param name="constraint">The encoding constraint.</param>
		/// <param name="maxLineLength">The maximum allowable length for a line (not counting the CRLF). Must be between <c>60</c> and <c>998</c> (inclusive).</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="maxLineLength"/> is not between <c>60</c> and <c>998</c> (inclusive).</para>
		/// <para>-or-</para>
		/// <para><paramref name="constraint"/> is not a valid value.</para>
		/// </exception>
		void Prepare (EncodingConstraint constraint, int maxLineLength = 78);

		/// <summary>
		/// Write the message to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Writes the message to the output stream using the provided formatting options.
		/// </remarks>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The output stream.</param>
		/// <param name="headersOnly"><c>true</c> if only the headers should be written; otherwise, <c>false</c>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		void WriteTo (FormatOptions options, Stream stream, bool headersOnly, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously write the message to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously writes the message to the output stream using the provided formatting options.
		/// </remarks>
		/// <returns>An awaitable task.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The output stream.</param>
		/// <param name="headersOnly"><c>true</c> if only the headers should be written; otherwise, <c>false</c>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		Task WriteToAsync (FormatOptions options, Stream stream, bool headersOnly, CancellationToken cancellationToken = default);

		/// <summary>
		/// Write the message to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Writes the message to the output stream using the provided formatting options.
		/// </remarks>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The output stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		void WriteTo (FormatOptions options, Stream stream, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously write the message to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously writes the message to the output stream using the provided formatting options.
		/// </remarks>
		/// <returns>An awaitable task.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The output stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		Task WriteToAsync (FormatOptions options, Stream stream, CancellationToken cancellationToken = default);

		/// <summary>
		/// Write the message to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Writes the message to the output stream using the default formatting options.
		/// </remarks>
		/// <param name="stream">The output stream.</param>
		/// <param name="headersOnly"><c>true</c> if only the headers should be written; otherwise, <c>false</c>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		void WriteTo (Stream stream, bool headersOnly, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously write the message to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously writes the message to the output stream using the default formatting options.
		/// </remarks>
		/// <returns>An awaitable task.</returns>
		/// <param name="stream">The output stream.</param>
		/// <param name="headersOnly"><c>true</c> if only the headers should be written; otherwise, <c>false</c>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		Task WriteToAsync (Stream stream, bool headersOnly, CancellationToken cancellationToken = default);

		/// <summary>
		/// Write the message to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Writes the message to the output stream using the default formatting options.
		/// </remarks>
		/// <param name="stream">The output stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		void WriteTo (Stream stream, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously write the message to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously writes the message to the output stream using the default formatting options.
		/// </remarks>
		/// <returns>An awaitable task.</returns>
		/// <param name="stream">The output stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		Task WriteToAsync (Stream stream, CancellationToken cancellationToken = default);

		/// <summary>
		/// Write the message to the specified file.
		/// </summary>
		/// <remarks>
		/// Writes the message to the specified file using the provided formatting options.
		/// </remarks>
		/// <param name="options">The formatting options.</param>
		/// <param name="fileName">The file.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// <paramref name="fileName"/> is an invalid file path.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file path could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to write to the specified file.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		void WriteTo (FormatOptions options, string fileName, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously write the message to the specified file.
		/// </summary>
		/// <remarks>
		/// Asynchronously writes the message to the specified file using the provided formatting options.
		/// </remarks>
		/// <returns>An awaitable task.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="fileName">The file.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// <paramref name="fileName"/> is an invalid file path.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file path could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to write to the specified file.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		Task WriteToAsync (FormatOptions options, string fileName, CancellationToken cancellationToken = default);

		/// <summary>
		/// Write the message to the specified file.
		/// </summary>
		/// <remarks>
		/// Writes the message to the specified file using the default formatting options.
		/// </remarks>
		/// <param name="fileName">The file.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// <paramref name="fileName"/> is an invalid file path.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file path could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to write to the specified file.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		void WriteTo (string fileName, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously write the message to the specified file.
		/// </summary>
		/// <remarks>
		/// Asynchronously writes the message to the specified file using the default formatting options.
		/// </remarks>
		/// <returns>An awaitable task.</returns>
		/// <param name="fileName">The file.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// <paramref name="fileName"/> is an invalid file path.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file path could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to write to the specified file.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		Task WriteToAsync (string fileName, CancellationToken cancellationToken = default);
	}
}

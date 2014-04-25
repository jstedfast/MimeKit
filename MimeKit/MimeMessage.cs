//
// MimeMessage.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2014 Xamarin Inc. (www.xamarin.com)
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
using System.Text;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

#if PORTABLE
using Encoding = Portable.Text.Encoding;
#endif

#if ENABLE_SNM
using System.Net.Mail;
#endif

#if ENABLE_CRYPTO
using MimeKit.Cryptography;
#endif

using MimeKit.Utils;
using MimeKit.IO;

namespace MimeKit {
	/// <summary>
	/// A MIME message.
	/// </summary>
	/// <remarks>
	/// <para>A message consists of header fields and, optionally, a body.</para>
	/// <para>The body of the message can either be plain text or it can be a
	/// tree of MIME entities such as a text/plain MIME part and a collection
	/// of file attachments.</para>
	/// </remarks>
	public class MimeMessage
	{
		static readonly StringComparer icase = StringComparer.OrdinalIgnoreCase;
		static readonly string[] StandardAddressHeaders = {
			"Resent-From", "Resent-Reply-To", "Resent-To", "Resent-Cc", "Resent-Bcc",
			"From", "Reply-To", "To", "Cc", "Bcc"
		};

		readonly Dictionary<string, InternetAddressList> addresses;
		readonly MessageIdList references;
		MailboxAddress resentSender;
		DateTimeOffset resentDate;
		string resentMessageId;
		MailboxAddress sender;
		DateTimeOffset date;
		string messageId;
		string inreplyto;
		Version version;

		internal MimeMessage (ParserOptions options, IEnumerable<Header> headers)
		{
			addresses = new Dictionary<string, InternetAddressList> (icase);
			Headers = new HeaderList (options);

			// initialize our address lists
			foreach (var name in StandardAddressHeaders) {
				var list = new InternetAddressList ();
				list.Changed += InternetAddressListChanged;
				addresses.Add (name, list);
			}

			references = new MessageIdList ();
			references.Changed += ReferencesChanged;
			inreplyto = null;

			Headers.Changed += HeadersChanged;

			// add all of our message headers...
			foreach (var header in headers) {
				if (header.Field.StartsWith ("Content-", StringComparison.OrdinalIgnoreCase))
					continue;

				Headers.Add (header);
			}
		}

		internal MimeMessage (ParserOptions options)
		{
			addresses = new Dictionary<string, InternetAddressList> (icase);
			Headers = new HeaderList (options);

			// initialize our address lists
			foreach (var name in StandardAddressHeaders) {
				var list = new InternetAddressList ();
				list.Changed += InternetAddressListChanged;
				addresses.Add (name, list);
			}

			references = new MessageIdList ();
			references.Changed += ReferencesChanged;
			inreplyto = null;

			Headers.Changed += HeadersChanged;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MimeMessage"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimeMessage"/>.
		/// </remarks>
		/// <param name="args">An array of initialization parameters: headers and message parts.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="args"/> contains more than one <see cref="MimeKit.MimeEntity"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="args"/> contains one or more arguments of an unknown type.</para>
		/// </exception>
		public MimeMessage (params object[] args) : this (ParserOptions.Default.Clone ())
		{
			if (args == null)
				throw new ArgumentNullException ("args");

			MimeEntity body = null;

			foreach (object obj in args) {
				if (obj == null)
					continue;

				// Just add the headers and let the events (already setup) keep the
				// addresses in sync.

				var header = obj as Header;
				if (header != null) {
					if (!header.Field.StartsWith ("Content-", StringComparison.OrdinalIgnoreCase))
						Headers.Add (header);

					continue;
				}

				var headers = obj as IEnumerable<Header>;
				if (headers != null) {
					foreach (var h in headers) {
						if (!h.Field.StartsWith ("Content-", StringComparison.OrdinalIgnoreCase))
							Headers.Add (h);
					}

					continue;						
				}

				var entity = obj as MimeEntity;
				if (entity != null) {
					if (body != null)
						throw new ArgumentException ("Message body should not be specified more than once.");

					body = entity;
					continue;
				}

				throw new ArgumentException ("Unknown initialization parameter: " + obj.GetType ());
			}

			if (body != null)
				Body = body;

			// Do exactly as in the parameterless constructor but avoid setting a default
			// value if an header already provided one.

			if (!Headers.Contains (HeaderId.From))
				Headers[HeaderId.From] = string.Empty;
			if (!Headers.Contains (HeaderId.To))
				Headers[HeaderId.To] = string.Empty;
			if (date == default (DateTimeOffset))
				Date = DateTimeOffset.Now;
			if (!Headers.Contains (HeaderId.Subject))
				Subject = string.Empty;
			if (messageId == null)
				MessageId = MimeUtils.GenerateMessageId ();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MimeMessage"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new MIME message.
		/// </remarks>
		public MimeMessage () : this (ParserOptions.Default.Clone ())
		{
			Headers[HeaderId.From] = string.Empty;
			Headers[HeaderId.To] = string.Empty;
			Date = DateTimeOffset.Now;
			Subject = string.Empty;
			MessageId = MimeUtils.GenerateMessageId ();
		}

		/// <summary>
		/// Gets the list of headers.
		/// </summary>
		/// <remarks>
		/// Represents the list of headers for a message. Typically, the headers of
		/// a message will contain transmission headers such as From and To along
		/// with metadata headers such as Subject and Date, but may include just
		/// about anything.
		/// </remarks>
		/// <value>The list of headers.</value>
		public HeaderList Headers {
			get; private set;
		}

		/// <summary>
		/// Gets or sets the address in the Sender header.
		/// </summary>
		/// <remarks>
		/// The sender may differ from the addresses in <see cref="From"/> if
		/// the message was sent by someone on behalf of someone else.
		/// </remarks>
		/// <value>The address in the Sender header.</value>
		public MailboxAddress Sender {
			get { return sender; }
			set {
				if (value == sender)
					return;

				if (value == null) {
					Headers.Changed -= HeadersChanged;
					Headers.RemoveAll (HeaderId.Sender);
					Headers.Changed += HeadersChanged;
					sender = null;
					return;
				}

				var builder = new StringBuilder ();
				int len = "Sender: ".Length;

				value.Encode (FormatOptions.Default, builder, ref len);
				builder.Append (FormatOptions.Default.NewLine);

				var raw = Encoding.ASCII.GetBytes (builder.ToString ());

				Headers.Changed -= HeadersChanged;
				Headers.Replace (new Header (Headers.Options, "Sender", raw));
				Headers.Changed += HeadersChanged;

				sender = value;
			}
		}

		/// <summary>
		/// Gets or sets the address in the Resent-Sender header.
		/// </summary>
		/// <remarks>
		/// The resent sender may differ from the addresses in <see cref="ResentFrom"/> if
		/// the message was sent by someone on behalf of someone else.
		/// </remarks>
		/// <value>The address in the Resent-Sender header.</value>
		public MailboxAddress ResentSender {
			get { return resentSender; }
			set {
				if (value == resentSender)
					return;

				if (value == null) {
					Headers.Changed -= HeadersChanged;
					Headers.RemoveAll (HeaderId.ResentSender);
					Headers.Changed += HeadersChanged;
					resentSender = null;
					return;
				}

				var builder = new StringBuilder ();
				int len = "Resent-Sender: ".Length;

				value.Encode (FormatOptions.Default, builder, ref len);
				builder.Append (FormatOptions.Default.NewLine);

				var raw = Encoding.ASCII.GetBytes (builder.ToString ());

				Headers.Changed -= HeadersChanged;
				Headers.Replace (new Header (Headers.Options, "Resent-Sender", raw));
				Headers.Changed += HeadersChanged;

				resentSender = value;
			}
		}

		/// <summary>
		/// Gets the list of addresses in the From header.
		/// </summary>
		/// <remarks>
		/// <para>The "From" header specifies the author(s) of the message.</para>
		/// <para>If more than one <see cref="MailboxAddress"/> is added to the
		/// list of "From" addresses, the <see cref="Sender"/> should be set to the
		/// single <see cref="MailboxAddress"/> of the personal actually sending
		/// the message.</para>
		/// </remarks>
		/// <value>The list of addresses in the From header.</value>
		public InternetAddressList From {
			get { return addresses["From"]; }
		}

		/// <summary>
		/// Gets the list of addresses in the Resent-From header.
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
		public InternetAddressList ResentFrom {
			get { return addresses["Resent-From"]; }
		}

		/// <summary>
		/// Gets the list of addresses in the Reply-To header.
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
		public InternetAddressList ReplyTo {
			get { return addresses["Reply-To"]; }
		}

		/// <summary>
		/// Gets the list of addresses in the Resent-Reply-To header.
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
		public InternetAddressList ResentReplyTo {
			get { return addresses["Resent-Reply-To"]; }
		}

		/// <summary>
		/// Gets the list of addresses in the To header.
		/// </summary>
		/// <remarks>
		/// The addresses in the To header are the primary recipients of
		/// the message.
		/// </remarks>
		/// <value>The list of addresses in the To header.</value>
		public InternetAddressList To {
			get { return addresses["To"]; }
		}

		/// <summary>
		/// Gets the list of addresses in the Resent-To header.
		/// </summary>
		/// <remarks>
		/// The addresses in the Resent-To header are the primary recipients of
		/// the message.
		/// </remarks>
		/// <value>The list of addresses in the Resent-To header.</value>
		public InternetAddressList ResentTo {
			get { return addresses["Resent-To"]; }
		}

		/// <summary>
		/// Gets the list of addresses in the Cc header.
		/// </summary>
		/// <remarks>
		/// The addresses in the Cc header are secondary recipients of the message
		/// and are usually not the individuals being directly addressed in the
		/// content of the message.
		/// </remarks>
		/// <value>The list of addresses in the Cc header.</value>
		public InternetAddressList Cc {
			get { return addresses["Cc"]; }
		}

		/// <summary>
		/// Gets the list of addresses in the Resent-Cc header.
		/// </summary>
		/// <remarks>
		/// The addresses in the Resent-Cc header are secondary recipients of the message
		/// and are usually not the individuals being directly addressed in the
		/// content of the message.
		/// </remarks>
		/// <value>The list of addresses in the Resent-Cc header.</value>
		public InternetAddressList ResentCc {
			get { return addresses["Resent-Cc"]; }
		}

		/// <summary>
		/// Gets the list of addresses in the Bcc header.
		/// </summary>
		/// <remarks>
		/// Recipients in the Blind-Carpbon-Copy list will not be visible to
		/// the other recipients of the message.
		/// </remarks>
		/// <value>The list of addresses in the Bcc header.</value>
		public InternetAddressList Bcc {
			get { return addresses["Bcc"]; }
		}

		/// <summary>
		/// Gets the list of addresses in the Resent-Bcc header.
		/// </summary>
		/// <remarks>
		/// Recipients in the Resent-Bcc list will not be visible to
		/// the other recipients of the message.
		/// </remarks>
		/// <value>The list of addresses in the Resent-Bcc header.</value>
		public InternetAddressList ResentBcc {
			get { return addresses["Resent-Bcc"]; }
		}

		/// <summary>
		/// Gets or sets the subject of the message.
		/// </summary>
		/// <remarks>
		/// <para>The Subject is typically a short string denoting the topic of the message.</para>
		/// <para>Replies will often use <c>"Re: "</c> followed by the Subject of the original message.</para>
		/// </remarks>
		/// <value>The subject of the message.</value>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		public string Subject {
			get { return Headers["Subject"]; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				Headers.Changed -= HeadersChanged;
				Headers["Subject"] = value;
				Headers.Changed += HeadersChanged;
			}
		}

		/// <summary>
		/// Gets or sets the date of the message.
		/// </summary>
		/// <remarks>
		/// If the date is not explicitly set before the message is written to a stream,
		/// the date will default to the exact moment when it is written to said stream.
		/// </remarks>
		/// <value>The date of the message.</value>
		public DateTimeOffset Date {
			get { return date; }
			set {
				if (date == value)
					return;

				Headers.Changed -= HeadersChanged;
				Headers["Date"] = DateUtils.FormatDate (value);
				Headers.Changed += HeadersChanged;

				date = value;
			}
		}

		/// <summary>
		/// Gets or sets the Resent-Date of the message.
		/// </summary>
		/// <remarks>
		/// Gets or sets the Resent-Date of the message.
		/// </remarks>
		/// <value>The Resent-Date of the message.</value>
		public DateTimeOffset ResentDate {
			get { return resentDate; }
			set {
				if (resentDate == value)
					return;

				Headers.Changed -= HeadersChanged;
				Headers["Resent-Date"] = DateUtils.FormatDate (value);
				Headers.Changed += HeadersChanged;

				resentDate = value;
			}
		}

		/// <summary>
		/// Gets or sets the list of references to other messages.
		/// </summary>
		/// <remarks>
		/// The References header contains a chain of Message-Ids back to the
		/// original message that started the thread.
		/// </remarks>
		/// <value>The references.</value>
		public MessageIdList References {
			get { return references; }
		}

		/// <summary>
		/// Gets or sets the Message-Id that this message is in reply to.
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
		public string InReplyTo {
			get { return inreplyto; }
			set {
				if (inreplyto == value)
					return;

				if (value == null) {
					Headers.Changed -= HeadersChanged;
					Headers.RemoveAll (HeaderId.InReplyTo);
					Headers.Changed += HeadersChanged;
					inreplyto = null;
					return;
				}

				var buffer = Encoding.ASCII.GetBytes (value);
				InternetAddress addr;
				int index = 0;

				if (!InternetAddress.TryParse (Headers.Options, buffer, ref index, buffer.Length, false, out addr) || !(addr is MailboxAddress))
					throw new ArgumentException ("Invalid Message-Id format.", "value");

				inreplyto = ((MailboxAddress) addr).Address;

				Headers.Changed -= HeadersChanged;
				Headers["In-Reply-To"] = "<" + inreplyto + ">";
				Headers.Changed += HeadersChanged;
			}
		}

		/// <summary>
		/// Gets or sets the message identifier.
		/// </summary>
		/// <remarks>
		/// <para>The Message-Id is meant to be a globally unique identifier for
		/// a message.</para>
		/// <para><see cref="MimeKit.Utils.MimeUtils.GenerateMessageId()"/> can be used
		/// to generate this value.</para>
		/// </remarks>
		/// <value>The message identifier.</value>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="value"/> is improperly formatted.
		/// </exception>
		public string MessageId {
			get { return messageId; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (messageId == value)
					return;

				var buffer = Encoding.ASCII.GetBytes (value);
				InternetAddress addr;
				int index = 0;

				if (!InternetAddress.TryParse (Headers.Options, buffer, ref index, buffer.Length, false, out addr) || !(addr is MailboxAddress))
					throw new ArgumentException ("Invalid Message-Id format.", "value");

				messageId = ((MailboxAddress) addr).Address;

				Headers.Changed -= HeadersChanged;
				Headers["Message-Id"] = "<" + messageId + ">";
				Headers.Changed += HeadersChanged;
			}
		}

		/// <summary>
		/// Gets or sets the Resent-Message-Id header.
		/// </summary>
		/// <remarks>
		/// <para>The Resent-Message-Id is meant to be a globally unique identifier for
		/// a message.</para>
		/// <para><see cref="MimeKit.Utils.MimeUtils.GenerateMessageId()"/> can be used
		/// to generate this value.</para>
		/// </remarks>
		/// <value>The Resent-Message-Id.</value>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="value"/> is improperly formatted.
		/// </exception>
		public string ResentMessageId {
			get { return resentMessageId; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (resentMessageId == value)
					return;

				var buffer = Encoding.ASCII.GetBytes (value);
				InternetAddress addr;
				int index = 0;

				if (!InternetAddress.TryParse (Headers.Options, buffer, ref index, buffer.Length, false, out addr) || !(addr is MailboxAddress))
					throw new ArgumentException ("Invalid Resent-Message-Id format.", "value");

				resentMessageId = ((MailboxAddress) addr).Address;

				Headers.Changed -= HeadersChanged;
				Headers["Resent-Message-Id"] = "<" + resentMessageId + ">";
				Headers.Changed += HeadersChanged;
			}
		}

		/// <summary>
		/// Gets or sets the MIME-Version.
		/// </summary>
		/// <remarks>
		/// The MIME-Version header specifies the version of the MIME specification
		/// that the message was created for.
		/// </remarks>
		/// <value>The MIME version.</value>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		public Version MimeVersion {
			get { return version; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (version != null && version.CompareTo (value) == 0)
					return;

				version = value;

				Headers.Changed -= HeadersChanged;
				Headers["MIME-Version"] = version.ToString ();
				Headers.Changed += HeadersChanged;
			}
		}

		/// <summary>
		/// Gets or sets the body of the message.
		/// </summary>
		/// <remarks>
		/// <para>The body of the message can either be plain text or it can be a
		/// tree of MIME entities such as a text/plain MIME part and a collection
		/// of file attachments.</para>
		/// <para>For a convenient way of constructing message bodies, see the
		/// <see cref="BodyBuilder"/> class.</para>
		/// </remarks>
		/// <value>The body of the message.</value>
		public MimeEntity Body {
			get; set;
		}

		static IEnumerable<MimePart> EnumerateMimeParts (MimeEntity entity)
		{
			if (entity == null)
				yield break;

			var multipart = entity as Multipart;

			if (multipart != null) {
				foreach (var subpart in multipart) {
					foreach (var part in EnumerateMimeParts (subpart))
						yield return part;
				}

				yield break;
			}

			var msgpart = entity as MessagePart;

			if (msgpart != null) {
				var message = msgpart.Message;

				foreach (var part in EnumerateMimeParts (message.Body))
					yield return part;

				yield break;
			}

			yield return (MimePart) entity;
		}

		/// <summary>
		/// Gets the body parts of the message.
		/// </summary>
		/// <remarks>
		/// Traverses over the MIME tree, enumerating all  of the <see cref="MimePart"/> objects.
		/// </remarks>
		/// <value>The body parts.</value>
		public IEnumerable<MimePart> BodyParts {
			get { return EnumerateMimeParts (Body); }
		}

		/// <summary>
		/// Gets the attachments.
		/// </summary>
		/// <remarks>
		/// Traverses over the MIME tree, enumerating all of the <see cref="MimePart"/> objects that
		/// have a Content-Disposition header set to <c>"attachment"</c>.
		/// </remarks>
		/// <value>The attachments.</value>
		public IEnumerable<MimePart> Attachments {
			get { return EnumerateMimeParts (Body).Where (part => part.IsAttachment); }
		}

		/// <summary>
		/// Writes the message to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Writes the message to the output stream using the provided formatting options.
		/// </remarks>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The output stream.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
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
		public void WriteTo (FormatOptions options, Stream stream, CancellationToken cancellationToken)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (stream == null)
				throw new ArgumentNullException ("stream");

			if (version == null && Body != null && Body.Headers.Count > 0)
				MimeVersion = new Version (1, 0);

			if (Body == null) {
				Headers.WriteTo (options, stream, cancellationToken);

				cancellationToken.ThrowIfCancellationRequested ();
				stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
			} else {
				cancellationToken.ThrowIfCancellationRequested ();

				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (options.CreateNewLineFilter ());

					foreach (var header in MergeHeaders ()) {
						if (options.HiddenHeaders.Contains (header.Id))
							continue;

						cancellationToken.ThrowIfCancellationRequested ();

						var name = Encoding.ASCII.GetBytes (header.Field);

						filtered.Write (name, 0, name.Length);
						filtered.WriteByte ((byte) ':');
						filtered.Write (header.RawValue, 0, header.RawValue.Length);
					}

					filtered.Flush ();
				}

				options.WriteHeaders = false;
				Body.WriteTo (options, stream, cancellationToken);
			}
		}

		/// <summary>
		/// Writes the message to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Writes the message to the output stream using the provided formatting options.
		/// </remarks>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The output stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public void WriteTo (FormatOptions options, Stream stream)
		{
			WriteTo (options, stream, CancellationToken.None);
		}

		/// <summary>
		/// Writes the message to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Writes the message to the output stream using the default formatting options.
		/// </remarks>
		/// <param name="stream">The output stream.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public void WriteTo (Stream stream, CancellationToken cancellationToken)
		{
			WriteTo (FormatOptions.Default, stream, cancellationToken);
		}

		/// <summary>
		/// Writes the message to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Writes the message to the output stream using the default formatting options.
		/// </remarks>
		/// <param name="stream">The output stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public void WriteTo (Stream stream)
		{
			WriteTo (FormatOptions.Default, stream);
		}

		MailboxAddress GetMessageSigner ()
		{
			if (ResentSender != null)
				return ResentSender;

			if (ResentFrom.Count > 0)
				return ResentFrom.Mailboxes.FirstOrDefault ();

			if (Sender != null)
				return Sender;

			if (From.Count > 0)
				return From.Mailboxes.FirstOrDefault ();

			return null;
		}

		IList<MailboxAddress> GetMessageRecipients (bool includeSenders)
		{
			var recipients = new List<MailboxAddress> ();

			if (ResentSender != null || ResentFrom.Count > 0) {
				if (includeSenders) {
					if (ResentSender != null)
						recipients.Add (ResentSender);

					if (ResentFrom.Count > 0)
						recipients.AddRange (ResentFrom.Mailboxes);
				}

				recipients.AddRange (ResentTo.Mailboxes);
				recipients.AddRange (ResentCc.Mailboxes);
				recipients.AddRange (ResentBcc.Mailboxes);
			} else {
				if (includeSenders) {
					if (Sender != null)
						recipients.Add (Sender);

					if (From.Count > 0)
						recipients.AddRange (From.Mailboxes);
				}

				recipients.AddRange (To.Mailboxes);
				recipients.AddRange (Cc.Mailboxes);
				recipients.AddRange (Bcc.Mailboxes);
			}

			return recipients;
		}

#if ENABLE_CRYPTO
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
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="Body"/> has not been set.</para>
		/// <para>-or-</para>
		/// <para>A sender has not been specified.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The <paramref name="digestAlgo"/> is not supported.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for the sender.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found for the sender.
		/// </exception>
		public void Sign (CryptographyContext ctx, DigestAlgorithm digestAlgo)
		{
			if (ctx == null)
				throw new ArgumentNullException ("ctx");

			if (Body == null)
				throw new InvalidOperationException ("No message body has been set.");

			var signer = GetMessageSigner ();
			if (signer == null)
				throw new InvalidOperationException ("The sender has not been set.");

			Body = MultipartSigned.Create (ctx, signer, digestAlgo, Body);
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
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="Body"/> has not been set.</para>
		/// <para>-or-</para>
		/// <para>A sender has not been specified.</para>
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for the sender.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found for the sender.
		/// </exception>
		public void Sign (CryptographyContext ctx)
		{
			Sign (ctx, DigestAlgorithm.Sha1);
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
		/// <param name="ctx">The cryptography context.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// An unknown type of cryptography context was used.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="Body"/> has not been set.</para>
		/// <para>-or-</para>
		/// <para>No recipients have been specified.</para>
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for one or more of the recipients.
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// The public key could not be found for one or more of the recipients.
		/// </exception>
		public void Encrypt (CryptographyContext ctx)
		{
			if (ctx == null)
				throw new ArgumentNullException ("ctx");

			if (Body == null)
				throw new InvalidOperationException ("No message body has been set.");

			var recipients = GetMessageRecipients (true);
			if (recipients.Count == 0)
				throw new InvalidOperationException ("No recipients have been set.");

			if (ctx is SecureMimeContext) {
				Body = ApplicationPkcs7Mime.Encrypt ((SecureMimeContext) ctx, recipients, Body);
			} else if (ctx is OpenPgpContext) {
				Body = MultipartEncrypted.Create ((OpenPgpContext) ctx, recipients, Body);
			} else {
				throw new ArgumentException ("Unknown type of cryptography context.", "ctx");
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
		/// <param name="ctx">The cryptography context.</param>
		/// <param name="digestAlgo">The digest algorithm.</param>
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
		/// <para>The <see cref="Body"/> has not been set.</para>
		/// <para>-or-</para>
		/// <para>The sender has been specified.</para>
		/// <para>-or-</para>
		/// <para>No recipients have been specified.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The <paramref name="digestAlgo"/> is not supported.
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
		public void SignAndEncrypt (CryptographyContext ctx, DigestAlgorithm digestAlgo)
		{
			if (ctx == null)
				throw new ArgumentNullException ("ctx");

			if (Body == null)
				throw new InvalidOperationException ("No message body has been set.");

			var signer = GetMessageSigner ();
			if (signer == null)
				throw new InvalidOperationException ("The sender has not been set.");

			var recipients = GetMessageRecipients (true);
			if (recipients.Count == 0)
				throw new InvalidOperationException ("No recipients have been set.");

			if (ctx is SecureMimeContext) {
				Body = ApplicationPkcs7Mime.SignAndEncrypt ((SecureMimeContext) ctx, signer, digestAlgo, recipients, Body);
			} else if (ctx is OpenPgpContext) {
				Body = MultipartEncrypted.Create ((OpenPgpContext) ctx, signer, digestAlgo, recipients, Body);
			} else {
				throw new ArgumentException ("Unknown type of cryptography context.", "ctx");
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
		/// <param name="ctx">The cryptography context.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// An unknown type of cryptography context was used.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="Body"/> has not been set.</para>
		/// <para>-or-</para>
		/// <para>The sender has been specified.</para>
		/// <para>-or-</para>
		/// <para>No recipients have been specified.</para>
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
		public void SignAndEncrypt (CryptographyContext ctx)
		{
			SignAndEncrypt (ctx, DigestAlgorithm.Sha1);
		}
#endif // ENABLE_CRYPTO

		IEnumerable<Header> MergeHeaders ()
		{
			int mesgIndex = 0, bodyIndex = 0;

			while (mesgIndex < Headers.Count && bodyIndex < Body.Headers.Count) {
				var bodyHeader = Body.Headers[bodyIndex];
				if (!bodyHeader.Offset.HasValue)
					break;

				var mesgHeader = Headers[mesgIndex];

				if (mesgHeader.Offset.HasValue && mesgHeader.Offset < bodyHeader.Offset) {
					yield return mesgHeader;

					mesgIndex++;
				} else {
					yield return bodyHeader;

					bodyIndex++;
				}
			}

			while (mesgIndex < Headers.Count)
				yield return Headers[mesgIndex++];

			while (bodyIndex < Body.Headers.Count)
				yield return Body.Headers[bodyIndex++];
		}

		void SerializeAddressList (string field, InternetAddressList list)
		{
			var builder = new StringBuilder (" ");
			int lineLength = field.Length + 2;

			list.Encode (FormatOptions.Default, builder, ref lineLength);
			builder.Append (FormatOptions.Default.NewLine);

			var raw = Encoding.ASCII.GetBytes (builder.ToString ());

			Headers.Changed -= HeadersChanged;
			Headers.Replace (new Header (Headers.Options, field, raw));
			Headers.Changed += HeadersChanged;
		}

		void InternetAddressListChanged (object addrlist, EventArgs e)
		{
			var list = (InternetAddressList) addrlist;

			foreach (var name in StandardAddressHeaders) {
				if (addresses[name] == list) {
					SerializeAddressList (name, list);
					break;
				}
			}
		}

		void ReferencesChanged (object o, EventArgs e)
		{
			if (references.Count > 0) {
				int lineLength = "References".Length + 1;
				var options = FormatOptions.Default;
				var builder = new StringBuilder ();

				for (int i = 0; i < references.Count; i++) {
					if (i > 0 && lineLength + references[i].Length + 2 >= options.MaxLineLength) {
						builder.Append (options.NewLine);
						builder.Append ('\t');
						lineLength = 1;
					} else {
						builder.Append (' ');
						lineLength++;
					}

					lineLength += references[i].Length;
					builder.Append ("<" + references[i] + ">");
				}

				builder.Append (options.NewLine);

				var raw = Encoding.UTF8.GetBytes (builder.ToString ());

				Headers.Changed -= HeadersChanged;
				Headers.Replace (new Header (Headers.Options, "References", raw));
				Headers.Changed += HeadersChanged;
			} else {
				Headers.Changed -= HeadersChanged;
				Headers.RemoveAll (HeaderId.References);
				Headers.Changed += HeadersChanged;
			}
		}

		void AddAddresses (Header header, InternetAddressList list)
		{
			int length = header.RawValue.Length;
			List<InternetAddress> parsed;
			int index = 0;

			// parse the addresses in the new header and add them to our address list
			if (!InternetAddressList.TryParse (Headers.Options, header.RawValue, ref index, length, false, false, out parsed))
				return;

			list.Changed -= InternetAddressListChanged;
			list.AddRange (parsed);
			list.Changed += InternetAddressListChanged;
		}

		void ReloadAddressList (HeaderId id, InternetAddressList list)
		{
			// clear the address list and reload
			list.Changed -= InternetAddressListChanged;
			list.Clear ();

			foreach (var header in Headers) {
				if (header.Id != id)
					continue;

				int length = header.RawValue.Length;
				List<InternetAddress> parsed;
				int index = 0;

				if (!InternetAddressList.TryParse (Headers.Options, header.RawValue, ref index, length, false, false, out parsed))
					continue;

				list.AddRange (parsed);
			}

			list.Changed += InternetAddressListChanged;
		}

		void ReloadHeader (HeaderId id)
		{
			if (id == HeaderId.Unknown)
				return;

			switch (id) {
			case HeaderId.ResentMessageId:
				resentMessageId = null;
				break;
			case HeaderId.ResentSender:
				resentSender = null;
				break;
			case HeaderId.ResentDate:
				resentDate = DateTimeOffset.MinValue;
				break;
			case HeaderId.References:
				references.Changed -= ReferencesChanged;
				references.Clear ();
				references.Changed += ReferencesChanged;
				break;
			case HeaderId.InReplyTo:
				inreplyto = null;
				break;
			case HeaderId.MessageId:
				messageId = null;
				break;
			case HeaderId.Sender:
				sender = null;
				break;
			case HeaderId.Date:
				date = DateTimeOffset.MinValue;
				break;
			}

			foreach (var header in Headers) {
				if (header.Id != id)
					continue;

				var rawValue = header.RawValue;
				InternetAddress address;
				int index = 0;

				switch (id) {
				case HeaderId.MimeVersion:
					if (MimeUtils.TryParseVersion (rawValue, 0, rawValue.Length, out version))
						return;
					break;
				case HeaderId.References:
					references.Changed -= ReferencesChanged;
					foreach (var msgid in MimeUtils.EnumerateReferences (rawValue, 0, rawValue.Length))
						references.Add (msgid);
					references.Changed += ReferencesChanged;
					break;
				case HeaderId.InReplyTo:
					inreplyto = MimeUtils.EnumerateReferences (rawValue, 0, rawValue.Length).FirstOrDefault ();
					break;
				case HeaderId.ResentMessageId:
					resentMessageId = MimeUtils.EnumerateReferences (rawValue, 0, rawValue.Length).FirstOrDefault ();
					if (resentMessageId != null)
						return;
					break;
				case HeaderId.MessageId:
					messageId = MimeUtils.EnumerateReferences (rawValue, 0, rawValue.Length).FirstOrDefault ();
					if (messageId != null)
						return;
					break;
				case HeaderId.ResentSender:
					if (InternetAddress.TryParse (Headers.Options, rawValue, ref index, rawValue.Length, false, out address))
						resentSender = address as MailboxAddress;
					if (resentSender != null)
						return;
					break;
				case HeaderId.Sender:
					if (InternetAddress.TryParse (Headers.Options, rawValue, ref index, rawValue.Length, false, out address))
						sender = address as MailboxAddress;
					if (sender != null)
						return;
					break;
				case HeaderId.ResentDate:
					if (DateUtils.TryParseDateTime (rawValue, 0, rawValue.Length, out resentDate))
						return;
					break;
				case HeaderId.Date:
					if (DateUtils.TryParseDateTime (rawValue, 0, rawValue.Length, out date))
						return;
					break;
				}
			}
		}

		void HeadersChanged (object o, HeaderListChangedEventArgs e)
		{
			InternetAddressList list;
			InternetAddress address;
			byte[] rawValue;
			int index = 0;

			switch (e.Action) {
			case HeaderListChangedAction.Added:
				if (addresses.TryGetValue (e.Header.Field, out list)) {
					AddAddresses (e.Header, list);
					break;
				}

				rawValue = e.Header.RawValue;

				switch (e.Header.Id) {
				case HeaderId.MimeVersion:
					MimeUtils.TryParseVersion (rawValue, 0, rawValue.Length, out version);
					break;
				case HeaderId.References:
					references.Changed -= ReferencesChanged;
					foreach (var msgid in MimeUtils.EnumerateReferences (rawValue, 0, rawValue.Length))
						references.Add (msgid);
					references.Changed += ReferencesChanged;
					break;
				case HeaderId.InReplyTo:
					inreplyto = MimeUtils.EnumerateReferences (rawValue, 0, rawValue.Length).FirstOrDefault ();
					break;
				case HeaderId.ResentMessageId:
					resentMessageId = MimeUtils.EnumerateReferences (rawValue, 0, rawValue.Length).FirstOrDefault ();
					break;
				case HeaderId.MessageId:
					messageId = MimeUtils.EnumerateReferences (rawValue, 0, rawValue.Length).FirstOrDefault ();
					break;
				case HeaderId.ResentSender:
					if (InternetAddress.TryParse (Headers.Options, rawValue, ref index, rawValue.Length, false, out address))
						resentSender = address as MailboxAddress;
					break;
				case HeaderId.Sender:
					if (InternetAddress.TryParse (Headers.Options, rawValue, ref index, rawValue.Length, false, out address))
						sender = address as MailboxAddress;
					break;
				case HeaderId.ResentDate:
					DateUtils.TryParseDateTime (rawValue, 0, rawValue.Length, out resentDate);
					break;
				case HeaderId.Date:
					DateUtils.TryParseDateTime (rawValue, 0, rawValue.Length, out date);
					break;
				}
				break;
			case HeaderListChangedAction.Changed:
			case HeaderListChangedAction.Removed:
				if (addresses.TryGetValue (e.Header.Field, out list)) {
					ReloadAddressList (e.Header.Id, list);
					break;
				}

				ReloadHeader (e.Header.Id);
				break;
			case HeaderListChangedAction.Cleared:
				foreach (var kvp in addresses) {
					kvp.Value.Changed -= InternetAddressListChanged;
					kvp.Value.Clear ();
					kvp.Value.Changed += InternetAddressListChanged;
				}

				references.Changed -= ReferencesChanged;
				references.Clear ();
				references.Changed += ReferencesChanged;

				resentMessageId = null;
				resentSender = null;
				inreplyto = null;
				messageId = null;
				version = null;
				sender = null;
				break;
			default:
				throw new ArgumentOutOfRangeException ();
			}
		}

		/// <summary>
		/// Load a <see cref="MimeMessage"/> from the specified stream.
		/// </summary>
		/// <remarks>
		/// Loads a <see cref="MimeMessage"/> from the given stream, using the
		/// specified <see cref="ParserOptions"/>.
		/// </remarks>
		/// <returns>The parsed message.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="stream">The stream.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the entity.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeMessage Load (ParserOptions options, Stream stream, CancellationToken cancellationToken)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (stream == null)
				throw new ArgumentNullException ("stream");

			var parser = new MimeParser (options, stream, MimeFormat.Entity);

			return parser.ParseMessage (cancellationToken);
		}

		/// <summary>
		/// Load a <see cref="MimeMessage"/> from the specified stream.
		/// </summary>
		/// <remarks>
		/// Loads a <see cref="MimeMessage"/> from the given stream, using the
		/// default <see cref="ParserOptions"/>.
		/// </remarks>
		/// <returns>The parsed message.</returns>
		/// <param name="stream">The stream.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the entity.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeMessage Load (Stream stream, CancellationToken cancellationToken)
		{
			return Load (ParserOptions.Default, stream, cancellationToken);
		}

		/// <summary>
		/// Load a <see cref="MimeMessage"/> from the specified stream.
		/// </summary>
		/// <remarks>
		/// Loads a <see cref="MimeMessage"/> from the given stream, using the
		/// specified <see cref="ParserOptions"/>.
		/// </remarks>
		/// <returns>The parsed message.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="stream">The stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the entity.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeMessage Load (ParserOptions options, Stream stream)
		{
			return Load (options, stream, CancellationToken.None);
		}

		/// <summary>
		/// Load a <see cref="MimeMessage"/> from the specified stream.
		/// </summary>
		/// <remarks>
		/// Loads a <see cref="MimeMessage"/> from the given stream, using the
		/// default <see cref="ParserOptions"/>.
		/// </remarks>
		/// <returns>The parsed message.</returns>
		/// <param name="stream">The stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the entity.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeMessage Load (Stream stream)
		{
			return Load (ParserOptions.Default, stream, CancellationToken.None);
		}

#if !PORTABLE
		/// <summary>
		/// Load a <see cref="MimeMessage"/> from the specified file.
		/// </summary>
		/// <remarks>
		/// Loads a <see cref="MimeMessage"/> from the file at the given path, using the
		/// specified <see cref="ParserOptions"/>.
		/// </remarks>
		/// <returns>The parsed message.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="fileName">The name of the file to load.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The specified file path is empty.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the entity.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeMessage Load (ParserOptions options, string fileName, CancellationToken cancellationToken)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			using (var stream = File.OpenRead (fileName)) {
				return Load (options, stream, cancellationToken);
			}
		}

		/// <summary>
		/// Load a <see cref="MimeMessage"/> from the specified file.
		/// </summary>
		/// <remarks>
		/// Loads a <see cref="MimeMessage"/> from the file at the given path, using the
		/// default <see cref="ParserOptions"/>.
		/// </remarks>
		/// <returns>The parsed message.</returns>
		/// <param name="fileName">The name of the file to load.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The specified file path is empty.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the entity.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeMessage Load (string fileName, CancellationToken cancellationToken)
		{
			return Load (ParserOptions.Default, fileName, cancellationToken);
		}

		/// <summary>
		/// Load a <see cref="MimeMessage"/> from the specified file.
		/// </summary>
		/// <remarks>
		/// Loads a <see cref="MimeMessage"/> from the file at the given path, using the
		/// specified <see cref="ParserOptions"/>.
		/// </remarks>
		/// <returns>The parsed message.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="fileName">The name of the file to load.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The specified file path is empty.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the entity.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeMessage Load (ParserOptions options, string fileName)
		{
			return Load (options, fileName, CancellationToken.None);
		}

		/// <summary>
		/// Load a <see cref="MimeMessage"/> from the specified file.
		/// </summary>
		/// <remarks>
		/// Loads a <see cref="MimeMessage"/> from the file at the given path, using the
		/// default <see cref="ParserOptions"/>.
		/// </remarks>
		/// <returns>The parsed message.</returns>
		/// <param name="fileName">The name of the file to load.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The specified file path is empty.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the entity.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeMessage Load (string fileName)
		{
			return Load (ParserOptions.Default, fileName, CancellationToken.None);
		}
#endif // !PORTABLE

#if ENABLE_SNM
		#region System.Net.Mail support

		static MimePart GetMimePart (AttachmentBase item)
		{
			var mimeType = item.ContentType.ToString ();
			var part = new MimePart (ContentType.Parse (mimeType));
			var attachment = item as Attachment;

			if (attachment != null) {
				var disposition = attachment.ContentDisposition.ToString ();
				part.ContentDisposition = ContentDisposition.Parse (disposition);
			}

			switch (item.TransferEncoding) {
			case System.Net.Mime.TransferEncoding.QuotedPrintable:
				part.ContentTransferEncoding = ContentEncoding.QuotedPrintable;
				break;
			case System.Net.Mime.TransferEncoding.Base64:
				part.ContentTransferEncoding = ContentEncoding.Base64;
				break;
			case System.Net.Mime.TransferEncoding.SevenBit:
				part.ContentTransferEncoding = ContentEncoding.SevenBit;
				break;
			}

			if (item.ContentId != null)
				part.ContentId = item.ContentId;

			var stream = new MemoryStream ();
			item.ContentStream.CopyTo (stream);
			stream.Position = 0;

			part.ContentObject = new ContentObject (stream, ContentEncoding.Default);

			return part;
		}

		/// <summary>
		/// Creates a new <see cref="MimeMessage"/> from a <see cref="System.Net.Mail.MailMessage"/>.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimeMessage"/> from a <see cref="System.Net.Mail.MailMessage"/>.
		/// </remarks>
		/// <returns>The equivalent <see cref="MimeMessage"/>.</returns>
		/// <param name="message">The message.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="message"/> is <c>null</c>.
		/// </exception>
		public static MimeMessage CreateFromMailMessage (MailMessage message)
		{
			if (message == null)
				throw new ArgumentNullException ("message");

			var headers = new List<Header> ();
			foreach (var field in message.Headers.AllKeys) {
				foreach (var value in message.Headers.GetValues (field))
					headers.Add (new Header (field, value));
			}

			var msg = new MimeMessage (ParserOptions.Default, headers);
			MimeEntity body = null;

			if (message.Sender != null)
				msg.Sender = (MailboxAddress) message.Sender;
			if (message.From != null)
				msg.From.Add ((MailboxAddress) message.From);
			msg.ReplyTo.AddRange ((InternetAddressList) message.ReplyToList);
			msg.To.AddRange ((InternetAddressList) message.To);
			msg.Cc.AddRange ((InternetAddressList) message.CC);
			msg.Bcc.AddRange ((InternetAddressList) message.Bcc);
			msg.Subject = message.Subject ?? string.Empty;

			switch (message.Priority) {
			case MailPriority.High:
				msg.Headers.Add (HeaderId.Priority, "urgent");
				msg.Headers.Add (HeaderId.Importance, "high");
				msg.Headers.Add ("X-Priority", "1");
				break;
			case MailPriority.Low:
				msg.Headers.Add (HeaderId.Priority, "non-urgent");
				msg.Headers.Add (HeaderId.Importance, "low");
				msg.Headers.Add ("X-Priority", "5");
				break;
			}

			if (message.Body != null) {
				var text = new TextPart (message.IsBodyHtml ? "html" : "plain");
				text.SetText (message.BodyEncoding ?? Encoding.UTF8, message.Body);
				body = text;
			}

			if (message.AlternateViews.Count > 0) {
				var alternative = new Multipart ("alternative");

				if (body != null)
					alternative.Add (body);

				foreach (var view in message.AlternateViews) {
					var part = GetMimePart (view);

					if (view.BaseUri != null)
						part.ContentLocation = view.BaseUri;

					if (view.LinkedResources.Count > 0) {
						var type = part.ContentType.MediaType + "/" + part.ContentType.MediaSubtype;
						var related = new Multipart ("related");

						related.ContentType.Parameters.Add ("type", type);

						if (view.BaseUri != null)
							related.ContentLocation = view.BaseUri;

						related.Add (part);

						foreach (var resource in view.LinkedResources) {
							part = GetMimePart (resource);

							if (resource.ContentLink != null)
								part.ContentLocation = resource.ContentLink;

							related.Add (part);
						}

						alternative.Add (related);
					} else {
						alternative.Add (part);
					}
				}

				body = alternative;
			}

			if (message.Attachments.Count > 0) {
				var mixed = new Multipart ("mixed");

				if (body != null)
					mixed.Add (body);

				foreach (var attachment in message.Attachments)
					mixed.Add (GetMimePart (attachment));

				body = mixed;
			}

			msg.Body = body;

			return msg;
		}

		/// <summary>
		/// Explicit cast to convert a <see cref="System.Net.Mail.MailMessage"/> to a
		/// <see cref="MimeMessage"/>.
		/// </summary>
		/// <remarks>
		/// Allows creation of messages using Microsoft's System.Net.Mail APIs.
		/// </remarks>
		/// <returns>The equivalent <see cref="MimeMessage"/>.</returns>
		/// <param name="message">The message.</param>
		public static explicit operator MimeMessage (MailMessage message)
		{
			return message != null ? CreateFromMailMessage (message) : null;
		}

		#endregion
#endif
	}
}

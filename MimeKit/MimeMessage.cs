//
// MimeMessage.cs
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
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

#if ENABLE_SNM
using System.Net.Mail;
#endif

#if ENABLE_CRYPTO
using MimeKit.Cryptography;
#endif

using MimeKit.IO;
using MimeKit.Text;
using MimeKit.Utils;

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
	public class MimeMessage : IMimeMessage
	{
		static readonly HeaderId[] StandardAddressHeaders = {
			HeaderId.ResentFrom, HeaderId.ResentReplyTo, HeaderId.ResentTo, HeaderId.ResentCc, HeaderId.ResentBcc,
			HeaderId.From, HeaderId.ReplyTo, HeaderId.To, HeaderId.Cc, HeaderId.Bcc
		};

		enum LazyLoadedFields {
			None            = 0,
			ResentSender    = 1 << 0,
			ResentFrom      = 1 << 1,
			ResentReplyTo   = 1 << 2,
			ResentTo        = 1 << 3,
			ResentCc        = 1 << 4,
			ResentBcc       = 1 << 5,
			ResentDate      = 1 << 6,
			ResentMessageId = 1 << 7,
			Sender          = 1 << 8,
			From            = 1 << 9,
			ReplyTo         = 1 << 10,
			To              = 1 << 11,
			Cc              = 1 << 12,
			Bcc             = 1 << 13,
			Date            = 1 << 14,
			MessageId       = 1 << 15,
			InReplyTo       = 1 << 16,
			References      = 1 << 17,
			MimeVersion     = 1 << 18,
			Importance      = 1 << 29,
			Priority        = 1 << 20,
			XPriority       = 1 << 21
		}

		readonly Dictionary<HeaderId, InternetAddressList> addresses;
		MessageImportance importance = MessageImportance.Normal;
		XMessagePriority xpriority = XMessagePriority.Normal;
		MessagePriority priority = MessagePriority.Normal;
		readonly RfcComplianceMode compliance;
		readonly MessageIdList references;
		LazyLoadedFields lazyLoaded;
		MailboxAddress resentSender;
		DateTimeOffset resentDate;
		string resentMessageId;
		MailboxAddress sender;
		DateTimeOffset date;
		string messageId;
		string inreplyto;
		Version version;

		// Note: this .ctor is used only by the MimeParser and MimeMessage.CreateFromMailMessage()
		internal MimeMessage (ParserOptions options, IEnumerable<Header> headers, RfcComplianceMode mode)
		{
			addresses = new Dictionary<HeaderId, InternetAddressList> ();
			Headers = new HeaderList (options);

			compliance = mode;

			// initialize our address lists
			foreach (var id in StandardAddressHeaders) {
				var list = new InternetAddressList ();
				list.Changed += InternetAddressListChanged;
				addresses.Add (id, list);
			}

			references = new MessageIdList ();
			references.Changed += ReferencesChanged;

			// add all of our message headers...
			foreach (var header in headers) {
				if (header.Field.StartsWith ("Content-", StringComparison.OrdinalIgnoreCase))
					continue;

				Headers.Add (header);
			}

			Headers.Changed += HeadersChanged;
		}

		internal MimeMessage (ParserOptions options)
		{
			addresses = new Dictionary<HeaderId, InternetAddressList> ();
			Headers = new HeaderList (options);

			compliance = RfcComplianceMode.Strict;

			// initialize our address lists
			foreach (var id in StandardAddressHeaders) {
				var list = new InternetAddressList ();
				list.Changed += InternetAddressListChanged;
				addresses.Add (id, list);
			}

			references = new MessageIdList ();
			references.Changed += ReferencesChanged;

			Headers.Changed += HeadersChanged;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MimeMessage"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimeMessage"/>.
		/// </remarks>
		/// <param name="args">An array of initialization parameters: headers and message parts.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="args"/> contains more than one <see cref="MimeEntity"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="args"/> contains one or more arguments of an unknown type.</para>
		/// </exception>
		public MimeMessage (params object[] args) : this (ParserOptions.Default.Clone ())
		{
			if (args is null)
				throw new ArgumentNullException (nameof (args));

			MimeEntity body = null;

			foreach (var obj in args) {
				if (obj is null)
					continue;

				// Just add the headers and let the events (already setup) keep the
				// addresses in sync.
				if (obj is Header header) {
					if (!header.Field.StartsWith ("Content-", StringComparison.OrdinalIgnoreCase))
						Headers.Add (header);

					continue;
				}

				if (obj is IEnumerable<Header> headers) {
					foreach (var h in headers) {
						if (!h.Field.StartsWith ("Content-", StringComparison.OrdinalIgnoreCase))
							Headers.Add (h);
					}

					continue;
				}

				if (obj is MimeEntity entity) {
					if (body != null)
						throw new ArgumentException ("Message body should not be specified more than once.");

					body = entity;
					continue;
				}

				throw new ArgumentException ("Unknown initialization parameter: " + obj.GetType ());
			}

			if (body != null)
				Body = body;

			// Only set the default headers if they have not already been provided.
			if (!Headers.Contains (HeaderId.From))
				Headers[HeaderId.From] = string.Empty;
			if (!Headers.Contains (HeaderId.Date))
				Date = DateTimeOffset.Now;
			if (!Headers.Contains (HeaderId.Subject))
				Subject = string.Empty;
			if (!Headers.Contains (HeaderId.MessageId))
				MessageId = MimeUtils.GenerateMessageId ();
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MimeMessage"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new MIME message, specifying details at creation time.
		/// </remarks>
		/// <param name="from">The list of addresses in the From header.</param>
		/// <param name="to">The list of addresses in the To header.</param>
		/// <param name="subject">The subject of the message.</param>
		/// <param name="body">The body of the message.</param>
		public MimeMessage (IEnumerable<InternetAddress> from, IEnumerable<InternetAddress> to, string subject, MimeEntity body) : this ()
		{
			From.AddRange (from);
			To.AddRange (to);
			Subject = subject;
			Body = body;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MimeMessage"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new MIME message.
		/// </remarks>
		public MimeMessage () : this (ParserOptions.Default.Clone ())
		{
			Headers[HeaderId.From] = string.Empty;
			Date = DateTimeOffset.Now;
			Subject = string.Empty;
			MessageId = MimeUtils.GenerateMessageId ();
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="MimeMessage"/> is reclaimed by garbage collection.
		/// </summary>
		/// <remarks>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="MimeMessage"/> is reclaimed by garbage collection.
		/// </remarks>
		~MimeMessage ()
		{
			Dispose (false);
		}

		/// <summary>
		/// Get or set the mbox marker.
		/// </summary>
		/// <remarks>
		/// Set by the <see cref="MimeParser"/> when parsing attached message/rfc822 parts
		/// so that the message/rfc822 part can be reserialized back to its original form.
		/// </remarks>
		/// <value>The mbox marker.</value>
		internal byte[] MboxMarker {
			get; set;
		}

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
		public HeaderList Headers {
			get; private set;
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
		public MessageImportance Importance {
			get {
				if ((lazyLoaded & LazyLoadedFields.Importance) == 0) {
					if (Headers.TryGetHeader (HeaderId.Importance, out var header)) {
						switch (header.Value.ToLowerInvariant ().Trim ()) {
						case "high": importance = MessageImportance.High; break;
						case "low": importance = MessageImportance.Low; break;
						default: importance = MessageImportance.Normal; break;
						}
					}

					lazyLoaded |= LazyLoadedFields.Importance;
				}

				return importance;
			}
			set {
				if (value == importance)
					return;

				switch (value) {
				case MessageImportance.Normal:
				case MessageImportance.High:
				case MessageImportance.Low:
					SetHeader ("Importance", value.ToString ().ToLowerInvariant ());
					lazyLoaded |= LazyLoadedFields.Importance;
					importance = value;
					break;
				default:
					throw new ArgumentOutOfRangeException (nameof (value));
				}
			}
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
		public MessagePriority Priority {
			get {
				if ((lazyLoaded & LazyLoadedFields.Priority) == 0) {
					if (Headers.TryGetHeader (HeaderId.Priority, out var header)) {
						switch (header.Value.ToLowerInvariant ().Trim ()) {
						case "non-urgent": priority = MessagePriority.NonUrgent; break;
						case "urgent": priority = MessagePriority.Urgent; break;
						default: priority = MessagePriority.Normal; break;
						}
					}

					lazyLoaded |= LazyLoadedFields.Priority;
				}

				return priority;
			}
			set {
				if (value == priority)
					return;

				string rawValue;

				switch (value) {
				case MessagePriority.NonUrgent:
					rawValue = "non-urgent";
					break;
				case MessagePriority.Normal:
					rawValue = "normal";
					break;
				case MessagePriority.Urgent:
					rawValue = "urgent";
					break;
				default:
					throw new ArgumentOutOfRangeException (nameof (value));
				}

				SetHeader ("Priority", rawValue);

				lazyLoaded |= LazyLoadedFields.Priority;
				priority = value;
			}
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
		public XMessagePriority XPriority {
			get {
				if ((lazyLoaded & LazyLoadedFields.XPriority) == 0) {
					if (Headers.TryGetHeader (HeaderId.XPriority, out var header)) {
						int index = 0;

						ParseUtils.SkipWhiteSpace (header.RawValue, ref index, header.RawValue.Length);

						if (ParseUtils.TryParseInt32 (header.RawValue, ref index, header.RawValue.Length, out var number)) {
							xpriority = (XMessagePriority) Math.Min (Math.Max (number, 1), 5);
						} else {
							xpriority = XMessagePriority.Normal;
						}
					}

					lazyLoaded |= LazyLoadedFields.XPriority;
				}

				return xpriority;
			}
			set {
				if (value == xpriority)
					return;

				string rawValue;

				switch (value) {
				case XMessagePriority.Highest:
					rawValue = "1 (Highest)";
					break;
				case XMessagePriority.High:
					rawValue = "2 (High)";
					break;
				case XMessagePriority.Normal:
					rawValue = "3 (Normal)";
					break;
				case XMessagePriority.Low:
					rawValue = "4 (Low)";
					break;
				case XMessagePriority.Lowest:
					rawValue = "5 (Lowest)";
					break;
				default:
					throw new ArgumentOutOfRangeException (nameof (value));
				}

				SetHeader ("X-Priority", rawValue);

				lazyLoaded |= LazyLoadedFields.XPriority;
				xpriority = value;
			}
		}

		/// <summary>
		/// Get or set the address in the Sender header.
		/// </summary>
		/// <remarks>
		/// The sender may differ from the addresses in <see cref="From"/> if
		/// the message was sent by someone on behalf of someone else.
		/// </remarks>
		/// <value>The address in the Sender header.</value>
		public MailboxAddress Sender {
			get {
				if ((lazyLoaded & LazyLoadedFields.Sender) == 0) {
					if (Headers.TryGetHeader (HeaderId.Sender, out var header)) {
						var rawValue = header.RawValue;
						int index = 0;

						MailboxAddress.TryParse (Headers.Options, rawValue, ref index, rawValue.Length, false, out sender);
					}

					lazyLoaded |= LazyLoadedFields.Sender;
				}

				return sender;
			}
			set {
				if ((lazyLoaded & LazyLoadedFields.Sender) != 0 && value == sender)
					return;

				if (value is null) {
					RemoveHeader (HeaderId.Sender);
					lazyLoaded |= LazyLoadedFields.Sender;
					sender = null;
					return;
				}

				var options = FormatOptions.Default;
				var builder = new StringBuilder (" ");
				int len = "Sender: ".Length;

				value.Encode (options, builder, true, ref len);
				builder.Append (options.NewLine);

				var raw = Encoding.UTF8.GetBytes (builder.ToString ());

				ReplaceHeader (HeaderId.Sender, "Sender", raw);
				lazyLoaded |= LazyLoadedFields.Sender;
				sender = value;
			}
		}

		/// <summary>
		/// Get or set the address in the Resent-Sender header.
		/// </summary>
		/// <remarks>
		/// The resent sender may differ from the addresses in <see cref="ResentFrom"/> if
		/// the message was sent by someone on behalf of someone else.
		/// </remarks>
		/// <value>The address in the Resent-Sender header.</value>
		public MailboxAddress ResentSender {
			get {
				if ((lazyLoaded & LazyLoadedFields.ResentSender) == 0) {
					if (Headers.TryGetHeader (HeaderId.ResentSender, out var header)) {
						var rawValue = header.RawValue;
						int index = 0;

						MailboxAddress.TryParse (Headers.Options, rawValue, ref index, rawValue.Length, false, out resentSender);
					}

					lazyLoaded |= LazyLoadedFields.ResentSender;
				}

				return resentSender;
			}
			set {
				if ((lazyLoaded & LazyLoadedFields.ResentSender) != 0 && value == resentSender)
					return;

				if (value is null) {
					RemoveHeader (HeaderId.ResentSender);
					lazyLoaded |= LazyLoadedFields.ResentSender;
					resentSender = null;
					return;
				}

				var options = FormatOptions.Default;
				var builder = new StringBuilder (" ");
				int len = "Resent-Sender: ".Length;

				value.Encode (options, builder, true, ref len);
				builder.Append (options.NewLine);

				var raw = Encoding.UTF8.GetBytes (builder.ToString ());

				ReplaceHeader (HeaderId.ResentSender, "Resent-Sender", raw);
				lazyLoaded |= LazyLoadedFields.ResentSender;
				resentSender = value;
			}
		}

		InternetAddressList GetLazyLoadedAddresses (HeaderId id, LazyLoadedFields bit)
		{
			var list = addresses[id];

			if ((lazyLoaded & bit) == 0) {
				for (int i = 0; i < Headers.Count; i++) {
					if (Headers[i].Id != id)
						continue;

					AddAddresses (Headers[i], list);
				}

				lazyLoaded |= bit;
			}

			return list;
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
		public InternetAddressList From {
			get { return GetLazyLoadedAddresses (HeaderId.From, LazyLoadedFields.From); }
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
		public InternetAddressList ResentFrom {
			get { return GetLazyLoadedAddresses (HeaderId.ResentFrom, LazyLoadedFields.ResentFrom); }
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
		public InternetAddressList ReplyTo {
			get { return GetLazyLoadedAddresses (HeaderId.ReplyTo, LazyLoadedFields.ReplyTo); }
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
		public InternetAddressList ResentReplyTo {
			get { return GetLazyLoadedAddresses (HeaderId.ResentReplyTo, LazyLoadedFields.ResentReplyTo); }
		}

		/// <summary>
		/// Get the list of addresses in the To header.
		/// </summary>
		/// <remarks>
		/// The addresses in the To header are the primary recipients of
		/// the message.
		/// </remarks>
		/// <value>The list of addresses in the To header.</value>
		public InternetAddressList To {
			get { return GetLazyLoadedAddresses (HeaderId.To, LazyLoadedFields.To); }
		}

		/// <summary>
		/// Get the list of addresses in the Resent-To header.
		/// </summary>
		/// <remarks>
		/// The addresses in the Resent-To header are the primary recipients of
		/// the message.
		/// </remarks>
		/// <value>The list of addresses in the Resent-To header.</value>
		public InternetAddressList ResentTo {
			get { return GetLazyLoadedAddresses (HeaderId.ResentTo, LazyLoadedFields.ResentTo); }
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
		public InternetAddressList Cc {
			get { return GetLazyLoadedAddresses (HeaderId.Cc, LazyLoadedFields.Cc); }
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
		public InternetAddressList ResentCc {
			get { return GetLazyLoadedAddresses (HeaderId.ResentCc, LazyLoadedFields.ResentCc); }
		}

		/// <summary>
		/// Get the list of addresses in the Bcc header.
		/// </summary>
		/// <remarks>
		/// Recipients in the Blind-Carbon-Copy list will not be visible to
		/// the other recipients of the message.
		/// </remarks>
		/// <value>The list of addresses in the Bcc header.</value>
		public InternetAddressList Bcc {
			get { return GetLazyLoadedAddresses (HeaderId.Bcc, LazyLoadedFields.Bcc); }
		}

		/// <summary>
		/// Get the list of addresses in the Resent-Bcc header.
		/// </summary>
		/// <remarks>
		/// Recipients in the Resent-Bcc list will not be visible to
		/// the other recipients of the message.
		/// </remarks>
		/// <value>The list of addresses in the Resent-Bcc header.</value>
		public InternetAddressList ResentBcc {
			get { return GetLazyLoadedAddresses (HeaderId.ResentBcc, LazyLoadedFields.ResentBcc); }
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
		public string Subject {
			get { return Headers["Subject"]; }
			set {
				if (value is null)
					throw new ArgumentNullException (nameof (value));

				SetHeader ("Subject", value);
			}
		}

		/// <summary>
		/// Get or set the date of the message.
		/// </summary>
		/// <remarks>
		/// If the date is not explicitly set before the message is written to a stream,
		/// the date will default to the exact moment when it is written to said stream.
		/// </remarks>
		/// <value>The date of the message.</value>
		public DateTimeOffset Date {
			get {
				if ((lazyLoaded & LazyLoadedFields.Date) == 0) {
					if (Headers.TryGetHeader (HeaderId.Date, out var header)) {
						var rawValue = header.RawValue;

						DateUtils.TryParse (rawValue, 0, rawValue.Length, out date);
					}

					lazyLoaded |= LazyLoadedFields.Date;
				}

				return date;
			}
			set {
				if ((lazyLoaded & LazyLoadedFields.Date) != 0 && date == value)
					return;

				SetHeader ("Date", DateUtils.FormatDate (value));
				lazyLoaded |= LazyLoadedFields.Date;
				date = value;
			}
		}

		/// <summary>
		/// Get or set the Resent-Date of the message.
		/// </summary>
		/// <remarks>
		/// Gets or sets the Resent-Date of the message.
		/// </remarks>
		/// <value>The Resent-Date of the message.</value>
		public DateTimeOffset ResentDate {
			get {
				if ((lazyLoaded & LazyLoadedFields.ResentDate) == 0) {
					if (Headers.TryGetHeader (HeaderId.ResentDate, out var header)) {
						var rawValue = header.RawValue;

						DateUtils.TryParse (rawValue, 0, rawValue.Length, out resentDate);
					}

					lazyLoaded |= LazyLoadedFields.ResentDate;
				}

				return resentDate;
			}
			set {
				if (resentDate == value)
					return;

				SetHeader ("Resent-Date", DateUtils.FormatDate (value));
				lazyLoaded |= LazyLoadedFields.ResentDate;
				resentDate = value;
			}
		}

		/// <summary>
		/// Get the list of references to other messages.
		/// </summary>
		/// <remarks>
		/// The References header contains a chain of Message-Ids back to the
		/// original message that started the thread.
		/// </remarks>
		/// <value>The references.</value>
		public MessageIdList References {
			get {
				if ((lazyLoaded & LazyLoadedFields.References) == 0) {
					if (Headers.TryGetHeader (HeaderId.References, out var header)) {
						var rawValue = header.RawValue;

						references.Changed -= ReferencesChanged;
						foreach (var msgid in MimeUtils.EnumerateReferences (rawValue, 0, rawValue.Length))
							references.Add (msgid);
						references.Changed += ReferencesChanged;
					}

					lazyLoaded |= LazyLoadedFields.References;
				}

				return references;
			}
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
		public string InReplyTo {
			get {
				if ((lazyLoaded & LazyLoadedFields.InReplyTo) == 0) {
					if (Headers.TryGetHeader (HeaderId.InReplyTo, out var header)) {
						var rawValue = header.RawValue;

						inreplyto = MimeUtils.EnumerateReferences (rawValue, 0, rawValue.Length).FirstOrDefault ();
					}

					lazyLoaded |= LazyLoadedFields.InReplyTo;
				}

				return inreplyto;
			}
			set {
				if ((lazyLoaded & LazyLoadedFields.InReplyTo) != 0 && inreplyto == value)
					return;

				if (value is null) {
					RemoveHeader (HeaderId.InReplyTo);
					lazyLoaded |= LazyLoadedFields.InReplyTo;
					inreplyto = null;
					return;
				}

				var buffer = Encoding.UTF8.GetBytes (value);
				int index = 0;

				if (!ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, false, out string msgid))
					throw new ArgumentException ("Invalid Message-Id format.", nameof (value));

				lazyLoaded |= LazyLoadedFields.InReplyTo;
				inreplyto = msgid;

				SetHeader ("In-Reply-To", "<" + inreplyto + ">");
			}
		}

		/// <summary>
		/// Get or set the message identifier.
		/// </summary>
		/// <remarks>
		/// <para>The Message-Id is meant to be a globally unique identifier for
		/// a message.</para>
		/// <para><see cref="MimeUtils.GenerateMessageId()"/> can be used
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
			get {
				if ((lazyLoaded & LazyLoadedFields.MessageId) == 0) {
					if (Headers.TryGetHeader (HeaderId.MessageId, out var header)) {
						var rawValue = header.RawValue;

						messageId = MimeUtils.ParseMessageId (rawValue, 0, rawValue.Length);
					}

					lazyLoaded |= LazyLoadedFields.MessageId;
				}

				return messageId;
			}
			set {
				if (value is null)
					throw new ArgumentNullException (nameof (value));

				if ((lazyLoaded & LazyLoadedFields.MessageId) != 0 && messageId == value)
					return;

				var buffer = Encoding.UTF8.GetBytes (value);
				int index = 0;

				if (!ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, false, out string msgid))
					throw new ArgumentException ("Invalid Message-Id format.", nameof (value));

				lazyLoaded |= LazyLoadedFields.MessageId;
				messageId = msgid;

				SetHeader ("Message-Id", "<" + messageId + ">");
			}
		}

		/// <summary>
		/// Get or set the Resent-Message-Id header.
		/// </summary>
		/// <remarks>
		/// <para>The Resent-Message-Id is meant to be a globally unique identifier for
		/// a message.</para>
		/// <para><see cref="MimeUtils.GenerateMessageId()"/> can be used
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
			get {
				if ((lazyLoaded & LazyLoadedFields.ResentMessageId) == 0) {
					if (Headers.TryGetHeader (HeaderId.ResentMessageId, out var header)) {
						var rawValue = header.RawValue;

						resentMessageId = MimeUtils.ParseMessageId (rawValue, 0, rawValue.Length);
					}

					lazyLoaded |= LazyLoadedFields.ResentMessageId;
				}

				return resentMessageId;
			}
			set {
				if (value is null)
					throw new ArgumentNullException (nameof (value));

				if ((lazyLoaded & LazyLoadedFields.ResentMessageId) != 0 && resentMessageId == value)
					return;

				var buffer = Encoding.UTF8.GetBytes (value);
				int index = 0;

				if (!ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, false, out string msgid))
					throw new ArgumentException ("Invalid Resent-Message-Id format.", nameof (value));

				lazyLoaded |= LazyLoadedFields.ResentMessageId;
				resentMessageId = msgid;

				SetHeader ("Resent-Message-Id", "<" + resentMessageId + ">");
			}
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
		public Version MimeVersion {
			get {
				if ((lazyLoaded & LazyLoadedFields.MimeVersion) == 0) {
					if (Headers.TryGetHeader (HeaderId.MimeVersion, out var header)) {
						var rawValue = header.RawValue;

						MimeUtils.TryParse (rawValue, 0, rawValue.Length, out version);
					}

					lazyLoaded |= LazyLoadedFields.MimeVersion;
				}

				return version;
			}
			set {
				if (value is null)
					throw new ArgumentNullException (nameof (value));

				if (version != null && version.CompareTo (value) == 0)
					return;

				SetHeader ("MIME-Version", value.ToString ());
				lazyLoaded |= LazyLoadedFields.MimeVersion;
				version = value;
			}
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
		public MimeEntity Body {
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
		public string TextBody {
			get { return GetTextBody (TextFormat.Plain); }
		}

		/// <summary>
		/// Get the html body of the message if it exists.
		/// </summary>
		/// <remarks>
		/// <para>Gets the HTML-formatted body of the message if it exists.</para>
		/// </remarks>
		/// <value>The html body if it exists; otherwise, <c>null</c>.</value>
		public string HtmlBody {
			get { return GetTextBody (TextFormat.Html); }
		}

		/// <summary>
		/// Get the text body in the specified format.
		/// </summary>
		/// <remarks>
		/// Gets the text body in the specified format, if it exists.
		/// </remarks>
		/// <returns>The text body in the desired format if it exists; otherwise, <c>null</c>.</returns>
		/// <param name="format">The desired text format.</param>
		public string GetTextBody (TextFormat format)
		{
			if (Body is Multipart multipart) {
				if (multipart.TryGetValue (format, out var body))
					return MultipartAlternative.GetText (body);
			} else if (Body is TextPart text && text.IsFormat (format) && !text.IsAttachment) {
				return MultipartAlternative.GetText (text);
			}

			return null;
		}

		static IEnumerable<MimeEntity> EnumerateMimeParts (MimeEntity entity)
		{
			if (entity is null)
				yield break;

			if (entity is Multipart multipart) {
				foreach (var subpart in multipart) {
					foreach (var part in EnumerateMimeParts (subpart))
						yield return part;
				}

				yield break;
			}

			yield return entity;
		}

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
		public IEnumerable<MimeEntity> BodyParts {
			get { return EnumerateMimeParts (Body); }
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
		public IEnumerable<MimeEntity> Attachments {
			get { return EnumerateMimeParts (Body).Where (x => x.IsAttachment); }
		}

		static void AddMailboxes (List<MailboxAddress> recipients, HashSet<string> unique, IEnumerable<MailboxAddress> mailboxes)
		{
			foreach (var mailbox in mailboxes) {
				if (unique is null || unique.Add (mailbox.Address))
					recipients.Add (mailbox);
			}
		}

		IList<MailboxAddress> GetMailboxes (bool includeSenders, bool onlyUnique)
		{
			HashSet<string> unique = onlyUnique ? new HashSet<string> (MimeUtils.OrdinalIgnoreCase) : null;
			var recipients = new List<MailboxAddress> ();

			if (ResentSender != null || ResentFrom.Count > 0) {
				if (includeSenders) {
					if (ResentSender != null) {
						if (unique is null || unique.Add (ResentSender.Address))
							recipients.Add (ResentSender);
					}

					AddMailboxes (recipients, unique, ResentFrom.Mailboxes);
				}

				AddMailboxes (recipients, unique, ResentTo.Mailboxes);
				AddMailboxes (recipients, unique, ResentCc.Mailboxes);
				AddMailboxes (recipients, unique, ResentBcc.Mailboxes);
			} else {
				if (includeSenders) {
					if (Sender != null) {
						if (unique is null || unique.Add (Sender.Address))
							recipients.Add (Sender);
					}

					AddMailboxes (recipients, unique, From.Mailboxes);
				}

				AddMailboxes (recipients, unique, To.Mailboxes);
				AddMailboxes (recipients, unique, Cc.Mailboxes);
				AddMailboxes (recipients, unique, Bcc.Mailboxes);
			}

			return recipients;
		}

		/// <summary>
		/// Get the concatenated list of recipients.
		/// </summary>
		/// <remarks>
		/// <para>Gets the concatenated list of recipients.</para>
		/// <para>If the <c>Resent-Sender</c> or <c>Resent-From</c> headers exist, then the recipients defined by the <c>Resent-To</c>,
		/// <c>Resent-Cc</c> and <c>Resent-Bcc</c> headers will be used. Otherwise, the recipients defined by the <c>To</c>, <c>Cc</c>
		/// and <c>Bcc</c> headers will be used.</para>
		/// </remarks>
		/// <param name="onlyUnique">If <c>true</c>, only mailboxes with a unique address will be included.</param>
		/// <returns>The concatenated list of recipients.</returns>
		public IList<MailboxAddress> GetRecipients (bool onlyUnique = false)
		{
			return GetMailboxes (false, onlyUnique);
		}

		/// <summary>
		/// Returns a <see cref="String"/> that represents the <see cref="MimeMessage"/> for debugging purposes.
		/// </summary>
		/// <remarks>
		/// <para>Returns a <see cref="String"/> that represents the <see cref="MimeMessage"/> for debugging purposes.</para>
		/// <note type="warning"><para>In general, the string returned from this method SHOULD NOT be used for serializing
		/// the message to disk. It is recommended that you use <see cref="WriteTo(Stream,CancellationToken)"/> instead.</para>
		/// <para>If this method is used for serializing the message to disk, the iso-8859-1 text encoding should be used for
		/// conversion.</para></note>
		/// </remarks>
		/// <returns>A <see cref="String"/> that represents the <see cref="MimeMessage"/> for debugging purposes.</returns>
		public override string ToString ()
		{
			using (var memory = new MemoryStream ()) {
				WriteTo (FormatOptions.Default, memory);

				var buffer = memory.GetBuffer ();
				int count = (int) memory.Length;

				return CharsetUtils.Latin1.GetString (buffer, 0, count);
			}
		}

		/// <summary>
		/// Dispatches to the specific visit method for this MIME message.
		/// </summary>
		/// <remarks>
		/// This default implementation for <see cref="MimeMessage"/> nodes
		/// calls <see cref="MimeVisitor.VisitMimeMessage"/>. Override this
		/// method to call into a more specific method on a derived visitor class
		/// of the <see cref="MimeVisitor"/> class. However, it should still
		/// support unknown visitors by calling
		/// <see cref="MimeVisitor.VisitMimeMessage"/>.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <c>null</c>.
		/// </exception>
		public virtual void Accept (MimeVisitor visitor)
		{
			if (visitor is null)
				throw new ArgumentNullException (nameof (visitor));

			visitor.VisitMimeMessage (this);
		}

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
		public virtual void Prepare (EncodingConstraint constraint, int maxLineLength = 78)
		{
			if (maxLineLength < FormatOptions.MinimumLineLength || maxLineLength > FormatOptions.MaximumLineLength)
				throw new ArgumentOutOfRangeException (nameof (maxLineLength));

			if (Body != null) {
				if (MimeVersion is null && Body.Headers.Count > 0)
					MimeVersion = new Version (1, 0);

				Body.Prepare (constraint, maxLineLength);
			}
		}

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
		public void WriteTo (FormatOptions options, Stream stream, bool headersOnly, CancellationToken cancellationToken = default)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (stream is null)
				throw new ArgumentNullException (nameof (stream));

			if (compliance == RfcComplianceMode.Strict && Body != null && Body.Headers.Count > 0 && !Headers.Contains (HeaderId.MimeVersion))
				MimeVersion = new Version (1, 0);

			if (Body != null) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (options.CreateNewLineFilter ());

					foreach (var header in MergeHeaders ()) {
						if (options.HiddenHeaders.Contains (header.Id))
							continue;

						filtered.Write (header.RawField, 0, header.RawField.Length, cancellationToken);

						if (!header.IsInvalid) {
							var rawValue = header.GetRawValue (options);

							filtered.Write (Header.Colon, 0, Header.Colon.Length, cancellationToken);
							filtered.Write (rawValue, 0, rawValue.Length, cancellationToken);
						}
					}

					filtered.Flush (cancellationToken);
				}

				if (stream is ICancellableStream cancellable) {
					cancellable.Write (options.NewLineBytes, 0, options.NewLineBytes.Length, cancellationToken);
				} else {
					cancellationToken.ThrowIfCancellationRequested ();
					stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
				}

				if (!headersOnly) {
					try {
						Body.EnsureNewLine = compliance == RfcComplianceMode.Strict || options.EnsureNewLine;
						Body.WriteTo (options, stream, true, cancellationToken);
					} finally {
						Body.EnsureNewLine = false;
					}
				}
			} else {
				Headers.WriteTo (options, stream, cancellationToken);
			}
		}

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
		public async Task WriteToAsync (FormatOptions options, Stream stream, bool headersOnly, CancellationToken cancellationToken = default)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (stream is null)
				throw new ArgumentNullException (nameof (stream));

			if (compliance == RfcComplianceMode.Strict && Body != null && Body.Headers.Count > 0 && !Headers.Contains (HeaderId.MimeVersion))
				MimeVersion = new Version (1, 0);

			if (Body != null) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (options.CreateNewLineFilter ());

					foreach (var header in MergeHeaders ()) {
						if (options.HiddenHeaders.Contains (header.Id))
							continue;

						await filtered.WriteAsync (header.RawField, 0, header.RawField.Length, cancellationToken).ConfigureAwait (false);

						if (!header.IsInvalid) {
							var rawValue = header.GetRawValue (options);

							await filtered.WriteAsync (Header.Colon, 0, Header.Colon.Length, cancellationToken).ConfigureAwait (false);
							await filtered.WriteAsync (rawValue, 0, rawValue.Length, cancellationToken).ConfigureAwait (false);
						}
					}

					await filtered.FlushAsync (cancellationToken).ConfigureAwait (false);
				}

				await stream.WriteAsync (options.NewLineBytes, 0, options.NewLineBytes.Length, cancellationToken).ConfigureAwait (false);

				if (!headersOnly) {
					try {
						Body.EnsureNewLine = compliance == RfcComplianceMode.Strict || options.EnsureNewLine;
						await Body.WriteToAsync (options, stream, true, cancellationToken).ConfigureAwait (false);
					} finally {
						Body.EnsureNewLine = false;
					}
				}
			} else {
				await Headers.WriteToAsync (options, stream, cancellationToken).ConfigureAwait (false);
			}
		}

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
		public void WriteTo (FormatOptions options, Stream stream, CancellationToken cancellationToken = default)
		{
			WriteTo (options, stream, false, cancellationToken);
		}

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
		public Task WriteToAsync (FormatOptions options, Stream stream, CancellationToken cancellationToken = default)
		{
			return WriteToAsync (options, stream, false, cancellationToken);
		}

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
		public void WriteTo (Stream stream, bool headersOnly, CancellationToken cancellationToken = default)
		{
			WriteTo (FormatOptions.Default, stream, headersOnly, cancellationToken);
		}

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
		public Task WriteToAsync (Stream stream, bool headersOnly, CancellationToken cancellationToken = default)
		{
			return WriteToAsync (FormatOptions.Default, stream, headersOnly, cancellationToken);
		}

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
		public void WriteTo (Stream stream, CancellationToken cancellationToken = default)
		{
			WriteTo (FormatOptions.Default, stream, false, cancellationToken);
		}

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
		public Task WriteToAsync (Stream stream, CancellationToken cancellationToken = default)
		{
			return WriteToAsync (FormatOptions.Default, stream, false, cancellationToken);
		}

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
		public void WriteTo (FormatOptions options, string fileName, CancellationToken cancellationToken = default)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (fileName is null)
				throw new ArgumentNullException (nameof (fileName));

			using (var stream = File.Open (fileName, FileMode.Create, FileAccess.Write)) {
				WriteTo (options, stream, cancellationToken);
				stream.Flush ();
			}
		}

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
		public async Task WriteToAsync (FormatOptions options, string fileName, CancellationToken cancellationToken = default)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (fileName is null)
				throw new ArgumentNullException (nameof (fileName));

			using (var stream = File.Open (fileName, FileMode.Create, FileAccess.Write)) {
				await WriteToAsync (options, stream, cancellationToken).ConfigureAwait (false);
				await stream.FlushAsync (cancellationToken).ConfigureAwait (false);
			}
		}

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
		public void WriteTo (string fileName, CancellationToken cancellationToken = default)
		{
			WriteTo (FormatOptions.Default, fileName, cancellationToken);
		}

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
		public Task WriteToAsync (string fileName, CancellationToken cancellationToken = default)
		{
			return WriteToAsync (FormatOptions.Default, fileName, cancellationToken);
		}

		MailboxAddress GetMessageSigner ()
		{
			if (ResentSender != null)
				return ResentSender;

			if (ResentFrom.Count > 0)
				return ResentFrom.Mailboxes.FirstOrDefault ();

			if (Sender != null)
				return Sender;

			return From.Mailboxes.FirstOrDefault ();
		}

		IList<MailboxAddress> GetEncryptionRecipients ()
		{
			return GetMailboxes (true, true);
		}

#if ENABLE_CRYPTO
		internal byte[] HashBody (FormatOptions options, DkimSignatureAlgorithm signatureAlgorithm, DkimCanonicalizationAlgorithm bodyCanonicalizationAlgorithm, int maxLength)
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

					if (Body != null) {
						try {
							Body.EnsureNewLine = compliance == RfcComplianceMode.Strict || options.EnsureNewLine;
							Body.WriteTo (options, filtered, true, CancellationToken.None);
						} finally {
							Body.EnsureNewLine = false;
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
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for the sender.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found for the sender.
		/// </exception>
		public void Sign (CryptographyContext ctx, DigestAlgorithm digestAlgo, CancellationToken cancellationToken = default)
		{
			if (ctx is null)
				throw new ArgumentNullException (nameof (ctx));

			if (Body is null)
				throw new InvalidOperationException ("No message body has been set.");

			var signer = GetMessageSigner () ?? throw new InvalidOperationException ("The sender has not been set.");
			Body = MultipartSigned.Create (ctx, signer, digestAlgo, Body, cancellationToken);
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
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for the sender.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found for the sender.
		/// </exception>
		public async Task SignAsync (CryptographyContext ctx, DigestAlgorithm digestAlgo, CancellationToken cancellationToken = default)
		{
			if (ctx is null)
				throw new ArgumentNullException (nameof (ctx));

			if (Body is null)
				throw new InvalidOperationException ("No message body has been set.");

			var signer = GetMessageSigner () ?? throw new InvalidOperationException ("The sender has not been set.");
			Body = await MultipartSigned.CreateAsync (ctx, signer, digestAlgo, Body, cancellationToken).ConfigureAwait (false);
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
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="Body"/> has not been set.</para>
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
		public void Sign (CryptographyContext ctx, CancellationToken cancellationToken = default)
		{
			Sign (ctx, DigestAlgorithm.Sha1, cancellationToken);
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
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="Body"/> has not been set.</para>
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
		public Task SignAsync (CryptographyContext ctx, CancellationToken cancellationToken = default)
		{
			return SignAsync (ctx, DigestAlgorithm.Sha1, cancellationToken);
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
		/// <param name="cancellationToken">The cancellation token.</param>
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
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for one or more of the recipients.
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// The public key could not be found for one or more of the recipients.
		/// </exception>
		public void Encrypt (CryptographyContext ctx, CancellationToken cancellationToken = default)
		{
			if (ctx is null)
				throw new ArgumentNullException (nameof (ctx));

			if (Body is null)
				throw new InvalidOperationException ("No message body has been set.");

			var recipients = GetEncryptionRecipients ();
			if (recipients.Count == 0)
				throw new InvalidOperationException ("No recipients have been set.");

			if (ctx is SecureMimeContext smime) {
				Body = ApplicationPkcs7Mime.Encrypt (smime, recipients, Body, cancellationToken);
			} else if (ctx is OpenPgpContext pgp) {
				Body = MultipartEncrypted.Encrypt (pgp, recipients, Body, cancellationToken);
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
		/// <param name="ctx">The cryptography context.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
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
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for one or more of the recipients.
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// The public key could not be found for one or more of the recipients.
		/// </exception>
		public async Task EncryptAsync (CryptographyContext ctx, CancellationToken cancellationToken = default)
		{
			if (ctx is null)
				throw new ArgumentNullException (nameof (ctx));

			if (Body is null)
				throw new InvalidOperationException ("No message body has been set.");

			var recipients = GetEncryptionRecipients ();
			if (recipients.Count == 0)
				throw new InvalidOperationException ("No recipients have been set.");

			if (ctx is SecureMimeContext smime) {
				Body = await ApplicationPkcs7Mime.EncryptAsync (smime, recipients, Body, cancellationToken).ConfigureAwait (false);
			} else if (ctx is OpenPgpContext pgp) {
				Body = await MultipartEncrypted.EncryptAsync (pgp, recipients, Body, cancellationToken).ConfigureAwait (false);
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
		/// <para>The <see cref="Body"/> has not been set.</para>
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
		public void SignAndEncrypt (CryptographyContext ctx, DigestAlgorithm digestAlgo, CancellationToken cancellationToken = default)
		{
			if (ctx is null)
				throw new ArgumentNullException (nameof (ctx));

			if (Body is null)
				throw new InvalidOperationException ("No message body has been set.");

			var signer = GetMessageSigner () ?? throw new InvalidOperationException ("The sender has not been set.");
			var recipients = GetEncryptionRecipients ();

			if (ctx is SecureMimeContext smime) {
				Body = ApplicationPkcs7Mime.SignAndEncrypt (smime, signer, digestAlgo, recipients, Body, cancellationToken);
			} else if (ctx is OpenPgpContext pgp) {
				Body = MultipartEncrypted.SignAndEncrypt (pgp, signer, digestAlgo, recipients, Body, cancellationToken);
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
		/// <para>The <see cref="Body"/> has not been set.</para>
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
		public async Task SignAndEncryptAsync (CryptographyContext ctx, DigestAlgorithm digestAlgo, CancellationToken cancellationToken = default)
		{
			if (ctx is null)
				throw new ArgumentNullException (nameof (ctx));

			if (Body is null)
				throw new InvalidOperationException ("No message body has been set.");

			var signer = GetMessageSigner () ?? throw new InvalidOperationException ("The sender has not been set.");
			var recipients = GetEncryptionRecipients ();

			if (ctx is SecureMimeContext smime) {
				Body = await ApplicationPkcs7Mime.SignAndEncryptAsync (smime, signer, digestAlgo, recipients, Body, cancellationToken).ConfigureAwait (false);
			} else if (ctx is OpenPgpContext pgp) {
				Body = await MultipartEncrypted.SignAndEncryptAsync (pgp, signer, digestAlgo, recipients, Body, cancellationToken).ConfigureAwait (false);
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
		/// <param name="ctx">The cryptography context.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// An unknown type of cryptography context was used.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="Body"/> has not been set.</para>
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
		public void SignAndEncrypt (CryptographyContext ctx, CancellationToken cancellationToken = default)
		{
			SignAndEncrypt (ctx, DigestAlgorithm.Sha1, cancellationToken);
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
		/// <param name="ctx">The cryptography context.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// An unknown type of cryptography context was used.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="Body"/> has not been set.</para>
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
		public Task SignAndEncryptAsync (CryptographyContext ctx, CancellationToken cancellationToken = default)
		{
			return SignAndEncryptAsync (ctx, DigestAlgorithm.Sha1, cancellationToken);
		}
#endif // ENABLE_CRYPTO

		IEnumerable<Header> MergeHeaders ()
		{
			int mesgIndex = 0, bodyIndex = 0;

			// write all of the prepended message headers first
			while (mesgIndex < Headers.Count) {
				var mesgHeader = Headers[mesgIndex];
				if (mesgHeader.Offset.HasValue)
					break;

				yield return mesgHeader;
				mesgIndex++;
			}

			// now merge the message and body headers as they appeared in the raw message
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

		void RemoveHeader (HeaderId id)
		{
			Headers.Changed -= HeadersChanged;

			try {
				Headers.RemoveAll (id);
			} finally {
				Headers.Changed += HeadersChanged;
			}
		}

		void ReplaceHeader (HeaderId id, string name, byte[] raw)
		{
			Headers.Changed -= HeadersChanged;

			try {
				Headers.Replace (new Header (Headers.Options, id, name, raw));
			} finally {
				Headers.Changed += HeadersChanged;
			}
		}

		void SetHeader (string name, string value)
		{
			Headers.Changed -= HeadersChanged;

			try {
				Headers[name] = value;
			} finally {
				Headers.Changed += HeadersChanged;
			}
		}

		void SerializeAddressList (HeaderId id, InternetAddressList list)
		{
			if (list.Count == 0) {
				RemoveHeader (id);
				return;
			}

			var builder = new StringBuilder (" ");
			var options = FormatOptions.Default;
			var field = id.ToHeaderName ();
			int lineLength = field.Length + 2;

			list.Encode (options, builder, true, ref lineLength);
			builder.Append (options.NewLine);

			var raw = Encoding.UTF8.GetBytes (builder.ToString ());

			ReplaceHeader (id, field, raw);
		}

		void InternetAddressListChanged (object addrlist, EventArgs e)
		{
			var list = (InternetAddressList) addrlist;

			foreach (var id in StandardAddressHeaders) {
				if (addresses[id] == list) {
					SerializeAddressList (id, list);
					break;
				}
			}
		}

		void ReferencesChanged (object o, EventArgs e)
		{
			if (references.Count > 0) {
				var builder = new ValueStringBuilder (128);
				int lineLength = "References".Length + 1;
				var options = FormatOptions.Default;

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
					builder.Append ('<');
					builder.Append (references[i]);
					builder.Append ('>');
				}

				builder.Append (options.NewLine);

				var raw = Encoding.UTF8.GetBytes (builder.ToString ());

				ReplaceHeader (HeaderId.References, "References", raw);
			} else {
				RemoveHeader (HeaderId.References);
			}
		}

		void AddAddresses (Header header, InternetAddressList list)
		{
			int length = header.RawValue.Length;
			int index = 0;

			// parse the addresses in the new header and add them to our address list
			if (!InternetAddressList.TryParse (AddressParserFlags.InternalTryParse, Headers.Options, header.RawValue, ref index, length, false, 0, out var parsed))
				return;

			list.Changed -= InternetAddressListChanged;
			list.AddRange (parsed);
			list.Changed += InternetAddressListChanged;
		}

		static LazyLoadedFields GetAddressListLazyLoadField (HeaderId id)
		{
			switch (id) {
			case HeaderId.From: return LazyLoadedFields.From;
			case HeaderId.ReplyTo: return LazyLoadedFields.ReplyTo;
			case HeaderId.To: return LazyLoadedFields.To;
			case HeaderId.Cc: return LazyLoadedFields.Cc;
			case HeaderId.Bcc: return LazyLoadedFields.Bcc;
			case HeaderId.ResentFrom: return LazyLoadedFields.ResentFrom;
			case HeaderId.ResentReplyTo: return LazyLoadedFields.ResentReplyTo;
			case HeaderId.ResentTo: return LazyLoadedFields.ResentTo;
			case HeaderId.ResentCc: return LazyLoadedFields.ResentCc;
			case HeaderId.ResentBcc: return LazyLoadedFields.ResentBcc;
			default: return LazyLoadedFields.None;
			}
		}

		void HeadersChanged (object o, HeaderListChangedEventArgs e)
		{
			if (e.Action != HeaderListChangedAction.Cleared && addresses.TryGetValue (e.Header.Id, out var list)) {
				var bit = GetAddressListLazyLoadField (e.Header.Id);

				if ((lazyLoaded & bit) != 0) {
					switch (e.Action) {
					case HeaderListChangedAction.Added:
						// Note: Only append new addresses of this type if the address list is already lazy-loaded.
						AddAddresses (e.Header, list);
						break;
					case HeaderListChangedAction.Changed:
					case HeaderListChangedAction.Removed:
						// Unload the address list if it has already been loaded
						list.Changed -= InternetAddressListChanged;
						list.Clear ();
						list.Changed += InternetAddressListChanged;
						lazyLoaded &= ~bit;
						break;
					}
				}

				return;
			}

			switch (e.Action) {
			case HeaderListChangedAction.Added:
			case HeaderListChangedAction.Changed:
			case HeaderListChangedAction.Removed:
				switch (e.Header.Id) {
				case HeaderId.ResentSender:
					lazyLoaded &= ~LazyLoadedFields.ResentSender;
					resentSender = null;
					break;
				case HeaderId.Sender:
					lazyLoaded &= ~LazyLoadedFields.Sender;
					sender = null;
					break;
				case HeaderId.ResentDate:
					lazyLoaded &= ~LazyLoadedFields.ResentDate;
					resentDate = DateTimeOffset.MinValue;
					break;
				case HeaderId.Date:
					lazyLoaded &= ~LazyLoadedFields.Date;
					date = DateTimeOffset.MinValue;
					break;
				case HeaderId.ResentMessageId:
					lazyLoaded &= ~LazyLoadedFields.ResentMessageId;
					resentMessageId = null;
					break;
				case HeaderId.MessageId:
					lazyLoaded &= ~LazyLoadedFields.MessageId;
					messageId = null;
					break;
				case HeaderId.References:
					lazyLoaded &= ~LazyLoadedFields.References;
					references.Changed -= ReferencesChanged;
					references.Clear ();
					references.Changed += ReferencesChanged;
					break;
				case HeaderId.InReplyTo:
					lazyLoaded &= ~LazyLoadedFields.InReplyTo;
					inreplyto = null;
					break;
				case HeaderId.MimeVersion:
					lazyLoaded &= ~LazyLoadedFields.MimeVersion;
					version = null;
					break;
				case HeaderId.Importance:
					lazyLoaded &= ~LazyLoadedFields.Importance;
					importance = MessageImportance.Normal;
					break;
				case HeaderId.Priority:
					lazyLoaded &= ~LazyLoadedFields.Priority;
					priority = MessagePriority.Normal;
					break;
				case HeaderId.XPriority:
					lazyLoaded &= ~LazyLoadedFields.XPriority;
					xpriority = XMessagePriority.Normal;
					break;
				}
				break;
			case HeaderListChangedAction.Cleared:
				lazyLoaded = LazyLoadedFields.None;

				foreach (var kvp in addresses) {
					kvp.Value.Changed -= InternetAddressListChanged;
					kvp.Value.Clear ();
					kvp.Value.Changed += InternetAddressListChanged;
				}

				references.Changed -= ReferencesChanged;
				references.Clear ();
				references.Changed += ReferencesChanged;

				resentDate = date = DateTimeOffset.MinValue;
				importance = MessageImportance.Normal;
				xpriority = XMessagePriority.Normal;
				priority = MessagePriority.Normal;
				resentMessageId = null;
				resentSender = null;
				inreplyto = null;
				messageId = null;
				version = null;
				sender = null;
				break;
			}
		}

		#region IDisposable implementation

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="MimeMessage"/> and
		/// optionally releases the managed resources.
		/// </summary>
		/// <remarks>
		/// Releases the unmanaged resources used by the <see cref="MimeMessage"/> and
		/// optionally releases the managed resources.
		/// </remarks>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
		/// <c>false</c> to release only the unmanaged resources.</param>
		protected virtual void Dispose (bool disposing)
		{
			if (disposing && Body != null)
				Body.Dispose ();
		}

		/// <summary>
		/// Releases all resources used by the <see cref="MimeMessage"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose()"/> when you are finished using the <see cref="MimeMessage"/>. The
		/// <see cref="Dispose()"/> method leaves the <see cref="MimeMessage"/> in an unusable state. After
		/// calling <see cref="Dispose()"/>, you must release all references to the <see cref="MimeMessage"/> so
		/// the garbage collector can reclaim the memory that the <see cref="MimeMessage"/> was occupying.</remarks>
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		#endregion

		/// <summary>
		/// Load a <see cref="MimeMessage"/> from the specified stream.
		/// </summary>
		/// <remarks>
		/// <para>Loads a <see cref="MimeMessage"/> from the given stream, using the
		/// specified <see cref="ParserOptions"/>.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="MimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save memory usage, but also improve <see cref="MimeParser"/>
		/// performance.</para>
		/// </remarks>
		/// <returns>The parsed message.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="stream">The stream.</param>
		/// <param name="persistent"><c>true</c> if the stream is persistent; otherwise <c>false</c>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
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
		public static MimeMessage Load (ParserOptions options, Stream stream, bool persistent, CancellationToken cancellationToken = default)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (stream is null)
				throw new ArgumentNullException (nameof (stream));

			var parser = new MimeParser (options, stream, MimeFormat.Entity, persistent);

			return parser.ParseMessage (cancellationToken);
		}

		/// <summary>
		/// Asynchronously load a <see cref="MimeMessage"/> from the specified stream.
		/// </summary>
		/// <remarks>
		/// <para>Loads a <see cref="MimeMessage"/> from the given stream, using the
		/// specified <see cref="ParserOptions"/>.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="MimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save memory usage, but also improve <see cref="MimeParser"/>
		/// performance.</para>
		/// </remarks>
		/// <returns>The parsed message.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="stream">The stream.</param>
		/// <param name="persistent"><c>true</c> if the stream is persistent; otherwise <c>false</c>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
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
		public static Task<MimeMessage> LoadAsync (ParserOptions options, Stream stream, bool persistent, CancellationToken cancellationToken = default)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (stream is null)
				throw new ArgumentNullException (nameof (stream));

			var parser = new MimeParser (options, stream, MimeFormat.Entity, persistent);

			return parser.ParseMessageAsync (cancellationToken);
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
		/// <param name="cancellationToken">The cancellation token.</param>
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
		public static MimeMessage Load (ParserOptions options, Stream stream, CancellationToken cancellationToken = default)
		{
			return Load (options, stream, false, cancellationToken);
		}

		/// <summary>
		/// Asynchronously load a <see cref="MimeMessage"/> from the specified stream.
		/// </summary>
		/// <remarks>
		/// Loads a <see cref="MimeMessage"/> from the given stream, using the
		/// specified <see cref="ParserOptions"/>.
		/// </remarks>
		/// <returns>The parsed message.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="stream">The stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
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
		public static Task<MimeMessage> LoadAsync (ParserOptions options, Stream stream, CancellationToken cancellationToken = default)
		{
			return LoadAsync (options, stream, false, cancellationToken);
		}

		/// <summary>
		/// Load a <see cref="MimeMessage"/> from the specified stream.
		/// </summary>
		/// <remarks>
		/// <para>Loads a <see cref="MimeMessage"/> from the given stream, using the
		/// default <see cref="ParserOptions"/>.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="MimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save memory usage, but also improve <see cref="MimeParser"/>
		/// performance.</para>
		/// </remarks>
		/// <returns>The parsed message.</returns>
		/// <param name="stream">The stream.</param>
		/// <param name="persistent"><c>true</c> if the stream is persistent; otherwise <c>false</c>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
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
		public static MimeMessage Load (Stream stream, bool persistent, CancellationToken cancellationToken = default)
		{
			return Load (ParserOptions.Default, stream, persistent, cancellationToken);
		}

		/// <summary>
		/// Asynchronously load a <see cref="MimeMessage"/> from the specified stream.
		/// </summary>
		/// <remarks>
		/// <para>Loads a <see cref="MimeMessage"/> from the given stream, using the
		/// default <see cref="ParserOptions"/>.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="MimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save memory usage, but also improve <see cref="MimeParser"/>
		/// performance.</para>
		/// </remarks>
		/// <returns>The parsed message.</returns>
		/// <param name="stream">The stream.</param>
		/// <param name="persistent"><c>true</c> if the stream is persistent; otherwise <c>false</c>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
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
		public static Task<MimeMessage> LoadAsync (Stream stream, bool persistent, CancellationToken cancellationToken = default)
		{
			return LoadAsync (ParserOptions.Default, stream, persistent, cancellationToken);
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
		/// <param name="cancellationToken">The cancellation token.</param>
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
		public static MimeMessage Load (Stream stream, CancellationToken cancellationToken = default)
		{
			return Load (ParserOptions.Default, stream, false, cancellationToken);
		}

		/// <summary>
		/// Asynchronously load a <see cref="MimeMessage"/> from the specified stream.
		/// </summary>
		/// <remarks>
		/// Loads a <see cref="MimeMessage"/> from the given stream, using the
		/// default <see cref="ParserOptions"/>.
		/// </remarks>
		/// <returns>The parsed message.</returns>
		/// <param name="stream">The stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
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
		public static Task<MimeMessage> LoadAsync (Stream stream, CancellationToken cancellationToken = default)
		{
			return LoadAsync (ParserOptions.Default, stream, false, cancellationToken);
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
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// <paramref name="fileName"/> is an invalid file path.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file path could not be found.
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
		public static MimeMessage Load (ParserOptions options, string fileName, CancellationToken cancellationToken = default)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (fileName is null)
				throw new ArgumentNullException (nameof (fileName));

			using (var stream = File.OpenRead (fileName))
				return Load (options, stream, cancellationToken);
		}

		/// <summary>
		/// Asynchronously load a <see cref="MimeMessage"/> from the specified file.
		/// </summary>
		/// <remarks>
		/// Loads a <see cref="MimeMessage"/> from the file at the given path, using the
		/// specified <see cref="ParserOptions"/>.
		/// </remarks>
		/// <returns>The parsed message.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="fileName">The name of the file to load.</param>
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
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// <paramref name="fileName"/> is an invalid file path.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file path could not be found.
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
		public static async Task<MimeMessage> LoadAsync (ParserOptions options, string fileName, CancellationToken cancellationToken = default)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (fileName is null)
				throw new ArgumentNullException (nameof (fileName));

			using (var stream = File.OpenRead (fileName))
				return await LoadAsync (options, stream, cancellationToken).ConfigureAwait (false);
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
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.
		/// </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// <paramref name="fileName"/> is an invalid file path.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file path could not be found.
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
		public static MimeMessage Load (string fileName, CancellationToken cancellationToken = default)
		{
			return Load (ParserOptions.Default, fileName, cancellationToken);
		}

		/// <summary>
		/// Asynchronously load a <see cref="MimeMessage"/> from the specified file.
		/// </summary>
		/// <remarks>
		/// Loads a <see cref="MimeMessage"/> from the file at the given path, using the
		/// default <see cref="ParserOptions"/>.
		/// </remarks>
		/// <returns>The parsed message.</returns>
		/// <param name="fileName">The name of the file to load.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.
		/// </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// <paramref name="fileName"/> is an invalid file path.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file path could not be found.
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
		public static Task<MimeMessage> LoadAsync (string fileName, CancellationToken cancellationToken = default)
		{
			return LoadAsync (ParserOptions.Default, fileName, cancellationToken);
		}

#if ENABLE_SNM
		static MimePart GetMimePart (AttachmentBase item)
		{
			if (item.ContentStream.CanSeek)
				item.ContentStream.Position = 0;

			var stream = new MemoryBlockStream ();

			try {
				item.ContentStream.CopyTo (stream);
				stream.Position = 0;
			} catch {
				stream.Dispose ();
				throw;
			}

			try {
				item.ContentStream.Position = 0;
			} catch { }

			var mimeType = item.ContentType.ToString ();
			if (!ContentType.TryParse (mimeType, out var contentType))
				contentType = new ContentType ("application", "octet-stream");

			MimePart part;

			if (contentType.MediaType.Equals ("text", StringComparison.OrdinalIgnoreCase))
				part = new TextPart (contentType);
			else
				part = new MimePart (contentType);

			if (item is Attachment attachment) {
				var value = attachment.ContentDisposition.ToString ();
				if (ContentDisposition.TryParse (value, out var disposition))
					part.ContentDisposition = disposition;
			}

			if (item.ContentId != null)
				part.ContentId = item.ContentId;

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
			case System.Net.Mime.TransferEncoding.EightBit:
				part.ContentTransferEncoding = ContentEncoding.EightBit;
				break;
			}

			part.Content = new MimeContent (stream);

			return part;
		}

		static void AddLinkedResources (MultipartAlternative alternative, MimePart root, AlternateView view)
		{
			var related = new MultipartRelated ();

			related.ContentType.Parameters.Add ("type", root.ContentType.MimeType);
			related.ContentBase = view.BaseUri;

			related.Add (root);

			foreach (var resource in view.LinkedResources) {
				var part = GetMimePart (resource);

				if (resource.ContentLink != null)
					part.ContentLocation = resource.ContentLink;

				related.Add (part);
			}

			alternative.Add (related);
		}

		static MimeEntity AddAlternateViews (MimeEntity body, AlternateViewCollection alternateViews)
		{
			var alternative = new MultipartAlternative ();

			if (body != null)
				alternative.Add (body);

			foreach (var view in alternateViews) {
				var part = GetMimePart (view);

				if (view.LinkedResources.Count > 0) {
					AddLinkedResources (alternative, part, view);
				} else {
					part.ContentBase = view.BaseUri;
					alternative.Add (part);
				}
			}

			return alternative;
		}

		/// <summary>
		/// Create a new <see cref="MimeMessage"/> from a <see cref="System.Net.Mail.MailMessage"/>.
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
			if (message is null)
				throw new ArgumentNullException (nameof (message));

			var headerEncoding = message.HeadersEncoding ?? Encoding.UTF8;

			var headers = new List<Header> ();
			foreach (var field in message.Headers.AllKeys) {
				foreach (var value in message.Headers.GetValues (field))
					headers.Add (new Header (headerEncoding, field, value));
			}

			var msg = new MimeMessage (ParserOptions.Default, headers, RfcComplianceMode.Strict);
			MimeEntity body = null;

			// Note: If the user has already sent their MailMessage via System.Net.Mail.SmtpClient,
			// then the following MailMessage properties will have been merged into the Headers, so
			// check to make sure our MimeMessage properties are empty before adding them.
			if (message.Sender != null)
				msg.Sender = (MailboxAddress) message.Sender;

			if (message.From != null) {
				msg.Headers.Replace (HeaderId.From, string.Empty);
				msg.From.Add ((MailboxAddress) message.From);
			}

			if (message.ReplyToList.Count > 0) {
				msg.Headers.Replace (HeaderId.ReplyTo, string.Empty);
				msg.ReplyTo.AddRange ((InternetAddressList) message.ReplyToList);
			}

			if (message.To.Count > 0) {
				msg.Headers.Replace (HeaderId.To, string.Empty);
				msg.To.AddRange ((InternetAddressList) message.To);
			}

			if (message.CC.Count > 0) {
				msg.Headers.Replace (HeaderId.Cc, string.Empty);
				msg.Cc.AddRange ((InternetAddressList) message.CC);
			}

			if (message.Bcc.Count > 0) {
				msg.Headers.Replace (HeaderId.Bcc, string.Empty);
				msg.Bcc.AddRange ((InternetAddressList) message.Bcc);
			}

			if (message.SubjectEncoding != null)
				msg.Headers.Replace (HeaderId.Subject, message.SubjectEncoding, message.Subject ?? string.Empty);
			else
				msg.Subject = message.Subject ?? string.Empty;

			if (!msg.Headers.Contains (HeaderId.Date))
				msg.Date = DateTimeOffset.Now;

			switch (message.Priority) {
			case MailPriority.Normal:
				msg.Headers.RemoveAll (HeaderId.XMSMailPriority);
				msg.Headers.RemoveAll (HeaderId.Importance);
				msg.Headers.RemoveAll (HeaderId.XPriority);
				msg.Headers.RemoveAll (HeaderId.Priority);
				break;
			case MailPriority.High:
				msg.Headers.Replace (HeaderId.Priority, "urgent");
				msg.Headers.Replace (HeaderId.Importance, "high");
				msg.Headers.Replace (HeaderId.XPriority, "2 (High)");
				break;
			case MailPriority.Low:
				msg.Headers.Replace (HeaderId.Priority, "non-urgent");
				msg.Headers.Replace (HeaderId.Importance, "low");
				msg.Headers.Replace (HeaderId.XPriority, "4 (Low)");
				break;
			}

			if (!string.IsNullOrEmpty (message.Body)) {
				var text = new TextPart (message.IsBodyHtml ? "html" : "plain");
				text.SetText (message.BodyEncoding ?? Encoding.UTF8, message.Body);
				body = text;
			}

			if (message.AlternateViews.Count > 0)
				body = AddAlternateViews (body, message.AlternateViews);

			body ??= new TextPart (message.IsBodyHtml ? "html" : "plain");

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
#endif
	}
}

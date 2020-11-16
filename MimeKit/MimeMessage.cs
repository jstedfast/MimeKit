//
// MimeMessage.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 .NET Foundation and Contributors
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
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;

#if ENABLE_SNM
using System.Net.Mail;
#endif

#if ENABLE_CRYPTO
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;

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
	public class MimeMessage
	{
		static readonly HeaderId[] StandardAddressHeaders = {
			HeaderId.ResentFrom, HeaderId.ResentReplyTo, HeaderId.ResentTo, HeaderId.ResentCc, HeaderId.ResentBcc,
			HeaderId.From, HeaderId.ReplyTo, HeaderId.To, HeaderId.Cc, HeaderId.Bcc
		};

		readonly Dictionary<HeaderId, InternetAddressList> addresses;
		MessageImportance importance = MessageImportance.Normal;
		XMessagePriority xpriority = XMessagePriority.Normal;
		MessagePriority priority = MessagePriority.Normal;
		readonly RfcComplianceMode compliance;
		readonly MessageIdList references;
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
			inreplyto = null;

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
			if (args == null)
				throw new ArgumentNullException (nameof (args));

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
			if (date == default (DateTimeOffset))
				Date = DateTimeOffset.Now;
			if (!Headers.Contains (HeaderId.Subject))
				Subject = string.Empty;
			if (messageId == null)
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
		/// <note type="tip">To access any MIME headers other than
		/// <see cref="HeaderId.MimeVersion"/>, you will need to access the
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
			get { return importance; }
			set {
				if (value == importance)
					return;

				switch (value) {
				case MessageImportance.Normal:
				case MessageImportance.High:
				case MessageImportance.Low:
					SetHeader ("Importance", value.ToString ().ToLowerInvariant ());
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
			get { return priority; }
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
			get { return xpriority; }
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
			get { return sender; }
			set {
				if (value == sender)
					return;

				if (value == null) {
					RemoveHeader (HeaderId.Sender);
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
			get { return resentSender; }
			set {
				if (value == resentSender)
					return;

				if (value == null) {
					RemoveHeader (HeaderId.ResentSender);
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

				resentSender = value;
			}
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
			get { return addresses[HeaderId.From]; }
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
			get { return addresses[HeaderId.ResentFrom]; }
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
			get { return addresses[HeaderId.ReplyTo]; }
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
			get { return addresses[HeaderId.ResentReplyTo]; }
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
			get { return addresses[HeaderId.To]; }
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
			get { return addresses[HeaderId.ResentTo]; }
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
			get { return addresses[HeaderId.Cc]; }
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
			get { return addresses[HeaderId.ResentCc]; }
		}

		/// <summary>
		/// Get the list of addresses in the Bcc header.
		/// </summary>
		/// <remarks>
		/// Recipients in the Blind-Carpbon-Copy list will not be visible to
		/// the other recipients of the message.
		/// </remarks>
		/// <value>The list of addresses in the Bcc header.</value>
		public InternetAddressList Bcc {
			get { return addresses[HeaderId.Bcc]; }
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
			get { return addresses[HeaderId.ResentBcc]; }
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
				if (value == null)
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
			get { return date; }
			set {
				if (date == value)
					return;

				SetHeader ("Date", DateUtils.FormatDate (value));
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
			get { return resentDate; }
			set {
				if (resentDate == value)
					return;

				SetHeader ("Resent-Date", DateUtils.FormatDate (value));
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
			get { return references; }
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
			get { return inreplyto; }
			set {
				if (inreplyto == value)
					return;

				if (value == null) {
					RemoveHeader (HeaderId.InReplyTo);
					inreplyto = null;
					return;
				}

				var buffer = Encoding.UTF8.GetBytes (value);
				MailboxAddress mailbox;
				int index = 0;

				if (!MailboxAddress.TryParse (Headers.Options, buffer, ref index, buffer.Length, false, out mailbox))
					throw new ArgumentException ("Invalid Message-Id format.", nameof (value));

				inreplyto = mailbox.Address;

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
			get { return messageId; }
			set {
				if (value == null)
					throw new ArgumentNullException (nameof (value));

				if (messageId == value)
					return;

				var buffer = Encoding.UTF8.GetBytes (value);
				MailboxAddress mailbox;
				int index = 0;

				if (!MailboxAddress.TryParse (Headers.Options, buffer, ref index, buffer.Length, false, out mailbox))
					throw new ArgumentException ("Invalid Message-Id format.", nameof (value));

				messageId = mailbox.Address;

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
			get { return resentMessageId; }
			set {
				if (value == null)
					throw new ArgumentNullException (nameof (value));

				if (resentMessageId == value)
					return;

				var buffer = Encoding.UTF8.GetBytes (value);
				MailboxAddress mailbox;
				int index = 0;

				if (!MailboxAddress.TryParse (Headers.Options, buffer, ref index, buffer.Length, false, out mailbox))
					throw new ArgumentException ("Invalid Resent-Message-Id format.", nameof (value));

				resentMessageId = mailbox.Address;

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
			get { return version; }
			set {
				if (value == null)
					throw new ArgumentNullException (nameof (value));

				if (version != null && version.CompareTo (value) == 0)
					return;

				SetHeader ("MIME-Version", value.ToString ());
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

		static bool TryGetMultipartBody (Multipart multipart, TextFormat format, out string body)
		{
			var alternative = multipart as MultipartAlternative;

			if (alternative != null) {
				body = alternative.GetTextBody (format);
				return body != null;
			}

			var related = multipart as MultipartRelated;
			Multipart multi;
			TextPart text;

			if (related == null) {
				// Note: This is probably a multipart/mixed... and if not, we can still treat it like it is.
				for (int i = 0; i < multipart.Count; i++) {
					multi = multipart[i] as Multipart;

					// descend into nested multiparts, if there are any...
					if (multi != null) {
						if (TryGetMultipartBody (multi, format, out body))
							return true;

						// The text body should never come after a multipart.
						break;
					}

					text = multipart[i] as TextPart;

					// Look for the first non-attachment text part (realistically, the body text will
					// preceed any attachments, but I'm not sure we can rely on that assumption).
					if (text != null && !text.IsAttachment) {
						if (text.IsFormat (format)) {
							body = MultipartAlternative.GetText (text);
							return true;
						}

						// Note: the first text/* part in a multipart/mixed is the text body.
						// If it's not in the format we're looking for, then it doesn't exist.
						break;
					}
				}
			} else {
				// Note: If the multipart/related root document is HTML, then this is the droid we are looking for.
				var root = related.Root;

				text = root as TextPart;

				if (text != null) {
					body = text.IsFormat (format) ? text.Text : null;
					return body != null;
				}

				// maybe the root is another multipart (like multipart/alternative)?
				multi = root as Multipart;

				if (multi != null)
					return TryGetMultipartBody (multi, format, out body);
			}

			body = null;

			return false;
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
			var multipart = Body as Multipart;

			if (multipart != null) {
				string text;

				if (TryGetMultipartBody (multipart, format, out text))
					return text;
			} else {
				var body = Body as TextPart;

				if (body != null && body.IsFormat (format) && !body.IsAttachment)
					return body.Text;
			}

			return null;
		}

		static IEnumerable<MimeEntity> EnumerateMimeParts (MimeEntity entity)
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

		static readonly byte[] ToStringWarning = Encoding.UTF8.GetBytes ("X-MimeKit-Warning: Do NOT use ToString() to serialize messages! Use one of the WriteTo() methods instead!");

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
				memory.Write (ToStringWarning, 0, ToStringWarning.Length);
				memory.Write (FormatOptions.Default.NewLineBytes, 0, FormatOptions.Default.NewLineBytes.Length);

				WriteTo (FormatOptions.Default, memory);

#if !NETSTANDARD1_3 && !NETSTANDARD1_6
				var buffer = memory.GetBuffer ();
#else
				var buffer = memory.ToArray ();
#endif
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
			if (visitor == null)
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
				if (MimeVersion == null && Body.Headers.Count > 0)
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
		public void WriteTo (FormatOptions options, Stream stream, bool headersOnly, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			if (stream == null)
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

				var cancellable = stream as ICancellableStream;

				if (cancellable != null) {
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
		public async Task WriteToAsync (FormatOptions options, Stream stream, bool headersOnly, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			if (stream == null)
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
		public void WriteTo (FormatOptions options, Stream stream, CancellationToken cancellationToken = default (CancellationToken))
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
		public Task WriteToAsync (FormatOptions options, Stream stream, CancellationToken cancellationToken = default (CancellationToken))
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
		public void WriteTo (Stream stream, bool headersOnly, CancellationToken cancellationToken = default (CancellationToken))
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
		public Task WriteToAsync (Stream stream, bool headersOnly, CancellationToken cancellationToken = default (CancellationToken))
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
		public void WriteTo (Stream stream, CancellationToken cancellationToken = default (CancellationToken))
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
		public Task WriteToAsync (Stream stream, CancellationToken cancellationToken = default (CancellationToken))
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
		public void WriteTo (FormatOptions options, string fileName, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			if (fileName == null)
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
		public async Task WriteToAsync (FormatOptions options, string fileName, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			if (fileName == null)
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
		public void WriteTo (string fileName, CancellationToken cancellationToken = default (CancellationToken))
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
		public Task WriteToAsync (string fileName, CancellationToken cancellationToken = default (CancellationToken))
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

			if (From.Count > 0)
				return From.Mailboxes.FirstOrDefault ();

			return null;
		}

		IList<MailboxAddress> GetMessageRecipients (bool includeSenders)
		{
			var recipients = new HashSet<MailboxAddress> ();

			if (ResentSender != null || ResentFrom.Count > 0) {
				if (includeSenders) {
					if (ResentSender != null)
						recipients.Add (ResentSender);

					if (ResentFrom.Count > 0) {
						foreach (var mailbox in ResentFrom.Mailboxes)
							recipients.Add (mailbox);
					}
				}

				foreach (var mailbox in ResentTo.Mailboxes)
					recipients.Add (mailbox);
				foreach (var mailbox in ResentCc.Mailboxes)
					recipients.Add (mailbox);
				foreach (var mailbox in ResentBcc.Mailboxes)
					recipients.Add (mailbox);
			} else {
				if (includeSenders) {
					if (Sender != null)
						recipients.Add (Sender);

					if (From.Count > 0) {
						foreach (var mailbox in From.Mailboxes)
							recipients.Add (mailbox);
					}
				}

				foreach (var mailbox in To.Mailboxes)
					recipients.Add (mailbox);
				foreach (var mailbox in Cc.Mailboxes)
					recipients.Add (mailbox);
				foreach (var mailbox in Bcc.Mailboxes)
					recipients.Add (mailbox);
			}

			return recipients.ToList ();
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
		/// Digitally sign the message using a DomainKeys Identified Mail (DKIM) signature.
		/// </summary>
		/// <remarks>
		/// Digitally signs the message using a DomainKeys Identified Mail (DKIM) signature.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimExamples.cs" region="DkimSign" />
		/// </example>
		/// <param name="options">The formatting options.</param>
		/// <param name="signer">The DKIM signer.</param>
		/// <param name="headers">The list of header fields to sign.</param>
		/// <param name="headerCanonicalizationAlgorithm">The header canonicalization algorithm.</param>
		/// <param name="bodyCanonicalizationAlgorithm">The body canonicalization algorithm.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="headers"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="headers"/> does not contain the 'From' header.</para>
		/// <para>-or-</para>
		/// <para><paramref name="headers"/> contains one or more of the following headers: Return-Path,
		/// Received, Comments, Keywords, Bcc, Resent-Bcc, or DKIM-Signature.</para>
		/// </exception>
		[Obsolete ("Use DkimSigner.Sign() instead.")]
		public void Sign (FormatOptions options, DkimSigner signer, IList<string> headers, DkimCanonicalizationAlgorithm headerCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Simple, DkimCanonicalizationAlgorithm bodyCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Simple)
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			signer.HeaderCanonicalizationAlgorithm = headerCanonicalizationAlgorithm;
			signer.BodyCanonicalizationAlgorithm = bodyCanonicalizationAlgorithm;

			signer.Sign (options, this, headers);
		}

		/// <summary>
		/// Digitally sign the message using a DomainKeys Identified Mail (DKIM) signature.
		/// </summary>
		/// <remarks>
		/// Digitally signs the message using a DomainKeys Identified Mail (DKIM) signature.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimExamples.cs" region="DkimSign" />
		/// </example>
		/// <param name="signer">The DKIM signer.</param>
		/// <param name="headers">The headers to sign.</param>
		/// <param name="headerCanonicalizationAlgorithm">The header canonicalization algorithm.</param>
		/// <param name="bodyCanonicalizationAlgorithm">The body canonicalization algorithm.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="headers"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="headers"/> does not contain the 'From' header.</para>
		/// <para>-or-</para>
		/// <para><paramref name="headers"/> contains one or more of the following headers: Return-Path,
		/// Received, Comments, Keywords, Bcc, Resent-Bcc, or DKIM-Signature.</para>
		/// </exception>
		[Obsolete ("Use DkimSigner.Sign() instead.")]
		public void Sign (DkimSigner signer, IList<string> headers, DkimCanonicalizationAlgorithm headerCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Simple, DkimCanonicalizationAlgorithm bodyCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Simple)
		{
			Sign (FormatOptions.Default, signer, headers, headerCanonicalizationAlgorithm, bodyCanonicalizationAlgorithm);
		}

		/// <summary>
		/// Digitally sign the message using a DomainKeys Identified Mail (DKIM) signature.
		/// </summary>
		/// <remarks>
		/// Digitally signs the message using a DomainKeys Identified Mail (DKIM) signature.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimExamples.cs" region="DkimSign" />
		/// </example>
		/// <param name="options">The formatting options.</param>
		/// <param name="signer">The DKIM signer.</param>
		/// <param name="headers">The list of header fields to sign.</param>
		/// <param name="headerCanonicalizationAlgorithm">The header canonicalization algorithm.</param>
		/// <param name="bodyCanonicalizationAlgorithm">The body canonicalization algorithm.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="headers"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="headers"/> does not contain the 'From' header.</para>
		/// <para>-or-</para>
		/// <para><paramref name="headers"/> contains one or more of the following headers: Return-Path,
		/// Received, Comments, Keywords, Bcc, Resent-Bcc, or DKIM-Signature.</para>
		/// </exception>
		[Obsolete ("Use DkimSigner.Sign() instead.")]
		public void Sign (FormatOptions options, DkimSigner signer, IList<HeaderId> headers, DkimCanonicalizationAlgorithm headerCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Simple, DkimCanonicalizationAlgorithm bodyCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Simple)
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			signer.HeaderCanonicalizationAlgorithm = headerCanonicalizationAlgorithm;
			signer.BodyCanonicalizationAlgorithm = bodyCanonicalizationAlgorithm;

			signer.Sign (options, this, headers);
		}

		/// <summary>
		/// Digitally sign the message using a DomainKeys Identified Mail (DKIM) signature.
		/// </summary>
		/// <remarks>
		/// Digitally signs the message using a DomainKeys Identified Mail (DKIM) signature.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimExamples.cs" region="DkimSign" />
		/// </example>
		/// <param name="signer">The DKIM signer.</param>
		/// <param name="headers">The headers to sign.</param>
		/// <param name="headerCanonicalizationAlgorithm">The header canonicalization algorithm.</param>
		/// <param name="bodyCanonicalizationAlgorithm">The body canonicalization algorithm.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="headers"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="headers"/> does not contain the 'From' header.</para>
		/// <para>-or-</para>
		/// <para><paramref name="headers"/> contains one or more of the following headers: Return-Path,
		/// Received, Comments, Keywords, Bcc, Resent-Bcc, or DKIM-Signature.</para>
		/// </exception>
		[Obsolete ("Use DkimSigner.Sign() instead.")]
		public void Sign (DkimSigner signer, IList<HeaderId> headers, DkimCanonicalizationAlgorithm headerCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Simple, DkimCanonicalizationAlgorithm bodyCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Simple)
		{
			Sign (FormatOptions.Default, signer, headers, headerCanonicalizationAlgorithm, bodyCanonicalizationAlgorithm);
		}

		Task<bool> DkimVerifyAsync (FormatOptions options, Header dkimSignature, IDkimPublicKeyLocator publicKeyLocator, bool doAsync, CancellationToken cancellationToken)
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			if (dkimSignature == null)
				throw new ArgumentNullException (nameof (dkimSignature));

			if (dkimSignature.Id != HeaderId.DkimSignature)
				throw new ArgumentException ("The signature parameter MUST be a DKIM-Signature header.", nameof (dkimSignature));

			var verifier = new DkimVerifier (publicKeyLocator);

			if (doAsync)
				return verifier.VerifyAsync (options, this, dkimSignature, cancellationToken);

			return Task.FromResult (verifier.Verify (options, this, dkimSignature, cancellationToken));
		}

		/// <summary>
		/// Verify the specified DKIM-Signature header.
		/// </summary>
		/// <remarks>
		/// Verifies the specified DKIM-Signature header.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
		/// </example>
		/// <returns><c>true</c> if the DKIM-Signature is valid; otherwise, <c>false</c>.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="dkimSignature">The DKIM-Signature header.</param>
		/// <param name="publicKeyLocator">The public key locator service.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="dkimSignature"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="publicKeyLocator"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="dkimSignature"/> is not a DKIM-Signature header.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The DKIM-Signature header value is malformed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		[Obsolete ("Use the DkimVerifier class instead.")]
		public bool Verify (FormatOptions options, Header dkimSignature, IDkimPublicKeyLocator publicKeyLocator, CancellationToken cancellationToken = default (CancellationToken))
		{
			return DkimVerifyAsync (options, dkimSignature, publicKeyLocator, false, cancellationToken).GetAwaiter ().GetResult ();
		}

		/// <summary>
		/// Asynchronously verify the specified DKIM-Signature header.
		/// </summary>
		/// <remarks>
		/// Verifies the specified DKIM-Signature header.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
		/// </example>
		/// <returns><c>true</c> if the DKIM-Signature is valid; otherwise, <c>false</c>.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="dkimSignature">The DKIM-Signature header.</param>
		/// <param name="publicKeyLocator">The public key locator service.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="dkimSignature"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="publicKeyLocator"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="dkimSignature"/> is not a DKIM-Signature header.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The DKIM-Signature header value is malformed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		[Obsolete ("Use the DkimVerifier class instead.")]
		public Task<bool> VerifyAsync (FormatOptions options, Header dkimSignature, IDkimPublicKeyLocator publicKeyLocator, CancellationToken cancellationToken = default (CancellationToken))
		{
			return DkimVerifyAsync (options, dkimSignature, publicKeyLocator, true, cancellationToken);
		}

		/// <summary>
		/// Verify the specified DKIM-Signature header.
		/// </summary>
		/// <remarks>
		/// Verifies the specified DKIM-Signature header.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
		/// </example>
		/// <returns><c>true</c> if the DKIM-Signature is valid; otherwise, <c>false</c>.</returns>
		/// <param name="dkimSignature">The DKIM-Signature header.</param>
		/// <param name="publicKeyLocator">The public key locator service.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="dkimSignature"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="publicKeyLocator"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="dkimSignature"/> is not a DKIM-Signature header.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The DKIM-Signature header value is malformed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		[Obsolete ("Use the DkimVerifier class instead.")]
		public bool Verify (Header dkimSignature, IDkimPublicKeyLocator publicKeyLocator, CancellationToken cancellationToken = default (CancellationToken))
		{
			return Verify (FormatOptions.Default, dkimSignature, publicKeyLocator, cancellationToken);
		}

		/// <summary>
		/// Asynchronously verify the specified DKIM-Signature header.
		/// </summary>
		/// <remarks>
		/// Verifies the specified DKIM-Signature header.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
		/// </example>
		/// <returns><c>true</c> if the DKIM-Signature is valid; otherwise, <c>false</c>.</returns>
		/// <param name="dkimSignature">The DKIM-Signature header.</param>
		/// <param name="publicKeyLocator">The public key locator service.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="dkimSignature"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="publicKeyLocator"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="dkimSignature"/> is not a DKIM-Signature header.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The DKIM-Signature header value is malformed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		[Obsolete ("Use the DkimVerifier class instead.")]
		public Task<bool> VerifyAsync (Header dkimSignature, IDkimPublicKeyLocator publicKeyLocator, CancellationToken cancellationToken = default (CancellationToken))
		{
			return VerifyAsync (FormatOptions.Default, dkimSignature, publicKeyLocator, cancellationToken);
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
				throw new ArgumentNullException (nameof (ctx));

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
				throw new ArgumentNullException (nameof (ctx));

			if (Body == null)
				throw new InvalidOperationException ("No message body has been set.");

			var recipients = GetMessageRecipients (true);
			if (recipients.Count == 0)
				throw new InvalidOperationException ("No recipients have been set.");

			if (ctx is SecureMimeContext) {
				Body = ApplicationPkcs7Mime.Encrypt ((SecureMimeContext) ctx, recipients, Body);
			} else if (ctx is OpenPgpContext) {
				Body = MultipartEncrypted.Encrypt ((OpenPgpContext) ctx, recipients, Body);
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
				throw new ArgumentNullException (nameof (ctx));

			if (Body == null)
				throw new InvalidOperationException ("No message body has been set.");

			var signer = GetMessageSigner ();
			if (signer == null)
				throw new InvalidOperationException ("The sender has not been set.");

			var recipients = GetMessageRecipients (true);

			if (ctx is SecureMimeContext) {
				Body = ApplicationPkcs7Mime.SignAndEncrypt ((SecureMimeContext) ctx, signer, digestAlgo, recipients, Body);
			} else if (ctx is OpenPgpContext) {
				Body = MultipartEncrypted.SignAndEncrypt ((OpenPgpContext) ctx, signer, digestAlgo, recipients, Body);
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

				ReplaceHeader (HeaderId.References, "References", raw);
			} else {
				RemoveHeader (HeaderId.References);
			}
		}

		void AddAddresses (Header header, InternetAddressList list)
		{
			int length = header.RawValue.Length;
			List<InternetAddress> parsed;
			int index = 0;

			// parse the addresses in the new header and add them to our address list
			if (!InternetAddressList.TryParse (Headers.Options, header.RawValue, ref index, length, false, 0, false, out parsed))
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

				if (!InternetAddressList.TryParse (Headers.Options, header.RawValue, ref index, length, false, 0, false, out parsed))
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
			case HeaderId.Importance:
				importance = MessageImportance.Normal;
				break;
			case HeaderId.XPriority:
				xpriority = XMessagePriority.Normal;
				break;
			case HeaderId.Priority:
				priority = MessagePriority.Normal;
				break;
			case HeaderId.Date:
				date = DateTimeOffset.MinValue;
				break;
			}

			foreach (var header in Headers) {
				if (header.Id != id)
					continue;

				var rawValue = header.RawValue;
				int number, index = 0;

				switch (id) {
				case HeaderId.MimeVersion:
					MimeUtils.TryParse (rawValue, 0, rawValue.Length, out version);
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
					resentMessageId = MimeUtils.ParseMessageId (rawValue, 0, rawValue.Length);
					break;
				case HeaderId.MessageId:
					messageId = MimeUtils.ParseMessageId (rawValue, 0, rawValue.Length);
					break;
				case HeaderId.ResentSender:
					MailboxAddress.TryParse (Headers.Options, rawValue, ref index, rawValue.Length, false, out resentSender);
					break;
				case HeaderId.Sender:
					MailboxAddress.TryParse (Headers.Options, rawValue, ref index, rawValue.Length, false, out sender);
					break;
				case HeaderId.ResentDate:
					DateUtils.TryParse (rawValue, 0, rawValue.Length, out resentDate);
					break;
				case HeaderId.Importance:
					switch (header.Value.ToLowerInvariant ().Trim ()) {
					case "high": importance = MessageImportance.High; break;
					case "low": importance = MessageImportance.Low; break;
					default: importance = MessageImportance.Normal; break;
					}
					break;
				case HeaderId.Priority:
					switch (header.Value.ToLowerInvariant ().Trim ()) {
					case "non-urgent": priority = MessagePriority.NonUrgent; break;
					case "urgent": priority = MessagePriority.Urgent; break;
					default: priority = MessagePriority.Normal; break;
					}
					break;
				case HeaderId.XPriority:
					ParseUtils.SkipWhiteSpace (rawValue, ref index, rawValue.Length);

					if (ParseUtils.TryParseInt32 (rawValue, ref index, rawValue.Length, out number)) {
						xpriority = (XMessagePriority) Math.Min (Math.Max (number, 1), 5);
					} else {
						xpriority = XMessagePriority.Normal;
					}
					break;
				case HeaderId.Date:
					DateUtils.TryParse (rawValue, 0, rawValue.Length, out date);
					break;
				}
			}
		}

		void HeadersChanged (object o, HeaderListChangedEventArgs e)
		{
			InternetAddressList list;
			byte[] rawValue;
			int index = 0;
			int number;

			switch (e.Action) {
			case HeaderListChangedAction.Added:
				if (addresses.TryGetValue (e.Header.Id, out list)) {
					AddAddresses (e.Header, list);
					break;
				}

				rawValue = e.Header.RawValue;

				switch (e.Header.Id) {
				case HeaderId.MimeVersion:
					MimeUtils.TryParse (rawValue, 0, rawValue.Length, out version);
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
					resentMessageId = MimeUtils.ParseMessageId (rawValue, 0, rawValue.Length);
					break;
				case HeaderId.MessageId:
					messageId = MimeUtils.ParseMessageId (rawValue, 0, rawValue.Length);
					break;
				case HeaderId.ResentSender:
					MailboxAddress.TryParse (Headers.Options, rawValue, ref index, rawValue.Length, false, out resentSender);
					break;
				case HeaderId.Sender:
					MailboxAddress.TryParse (Headers.Options, rawValue, ref index, rawValue.Length, false, out sender);
					break;
				case HeaderId.ResentDate:
					DateUtils.TryParse (rawValue, 0, rawValue.Length, out resentDate);
					break;
				case HeaderId.Importance:
					switch (e.Header.Value.ToLowerInvariant ().Trim ()) {
					case "high": importance = MessageImportance.High; break;
					case "low": importance = MessageImportance.Low; break;
					default: importance = MessageImportance.Normal; break;
					}
					break;
				case HeaderId.Priority:
					switch (e.Header.Value.ToLowerInvariant ().Trim ()) {
					case "non-urgent": priority = MessagePriority.NonUrgent; break;
					case "urgent": priority = MessagePriority.Urgent; break;
					default: priority = MessagePriority.Normal; break;
					}
					break;
				case HeaderId.XPriority:
					ParseUtils.SkipWhiteSpace (rawValue, ref index, rawValue.Length);

					if (ParseUtils.TryParseInt32 (rawValue, ref index, rawValue.Length, out number)) {
						xpriority = (XMessagePriority) Math.Min (Math.Max (number, 1), 5);
					} else {
						xpriority = XMessagePriority.Normal;
					}
					break;
				case HeaderId.Date:
					DateUtils.TryParse (rawValue, 0, rawValue.Length, out date);
					break;
				}
				break;
			case HeaderListChangedAction.Changed:
			case HeaderListChangedAction.Removed:
				if (addresses.TryGetValue (e.Header.Id, out list)) {
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
			default:
				throw new ArgumentOutOfRangeException ();
			}
		}

		/// <summary>
		/// Load a <see cref="MimeMessage"/> from the specified stream.
		/// </summary>
		/// <remarks>
		/// <para>Loads a <see cref="MimeMessage"/> from the given stream, using the
		/// specified <see cref="ParserOptions"/>.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="MimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save mmeory usage, but also improve <see cref="MimeParser"/>
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
		public static MimeMessage Load (ParserOptions options, Stream stream, bool persistent, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			if (stream == null)
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
		/// This has the potential to not only save mmeory usage, but also improve <see cref="MimeParser"/>
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
		public static Task<MimeMessage> LoadAsync (ParserOptions options, Stream stream, bool persistent, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			if (stream == null)
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
		public static MimeMessage Load (ParserOptions options, Stream stream, CancellationToken cancellationToken = default (CancellationToken))
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
		public static Task<MimeMessage> LoadAsync (ParserOptions options, Stream stream, CancellationToken cancellationToken = default (CancellationToken))
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
		/// This has the potential to not only save mmeory usage, but also improve <see cref="MimeParser"/>
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
		public static MimeMessage Load (Stream stream, bool persistent, CancellationToken cancellationToken = default (CancellationToken))
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
		/// This has the potential to not only save mmeory usage, but also improve <see cref="MimeParser"/>
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
		public static Task<MimeMessage> LoadAsync (Stream stream, bool persistent, CancellationToken cancellationToken = default (CancellationToken))
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
		public static MimeMessage Load (Stream stream, CancellationToken cancellationToken = default (CancellationToken))
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
		public static Task<MimeMessage> LoadAsync (Stream stream, CancellationToken cancellationToken = default (CancellationToken))
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
		public static MimeMessage Load (ParserOptions options, string fileName, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			if (fileName == null)
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
		public static async Task<MimeMessage> LoadAsync (ParserOptions options, string fileName, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			if (fileName == null)
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
		public static MimeMessage Load (string fileName, CancellationToken cancellationToken = default (CancellationToken))
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
		public static Task<MimeMessage> LoadAsync (string fileName, CancellationToken cancellationToken = default (CancellationToken))
		{
			return LoadAsync (ParserOptions.Default, fileName, cancellationToken);
		}

#if ENABLE_SNM
		static MimePart GetMimePart (AttachmentBase item)
		{
			var mimeType = item.ContentType.ToString ();
			var contentType = ContentType.Parse (mimeType);
			var attachment = item as Attachment;
			MimePart part;

			if (contentType.MediaType.Equals ("text", StringComparison.OrdinalIgnoreCase))
				part = new TextPart (contentType);
			else
				part = new MimePart (contentType);

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
			//case System.Net.Mime.TransferEncoding.EightBit:
			//	part.ContentTransferEncoding = ContentEncoding.EightBit;
			//	break;
			}

			if (item.ContentId != null)
				part.ContentId = item.ContentId;

			var stream = new MemoryBlockStream ();
			if (item.ContentStream.CanSeek)
				item.ContentStream.Position = 0;
			item.ContentStream.CopyTo (stream);
			stream.Position = 0;

			part.Content = new MimeContent (stream);

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
				throw new ArgumentNullException (nameof (message));

			var headers = new List<Header> ();
			foreach (var field in message.Headers.AllKeys) {
				foreach (var value in message.Headers.GetValues (field))
					headers.Add (new Header (field, value));
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

			if (message.AlternateViews.Count > 0) {
				var alternative = new MultipartAlternative ();

				if (body != null)
					alternative.Add (body);

				foreach (var view in message.AlternateViews) {
					var part = GetMimePart (view);

					if (view.LinkedResources.Count > 0) {
						var type = part.ContentType.MediaType + "/" + part.ContentType.MediaSubtype;
						var related = new MultipartRelated ();

						related.ContentType.Parameters.Add ("type", type);
						related.ContentBase = view.BaseUri;

						related.Add (part);

						foreach (var resource in view.LinkedResources) {
							part = GetMimePart (resource);

							if (resource.ContentLink != null)
								part.ContentLocation = resource.ContentLink;

							related.Add (part);
						}

						alternative.Add (related);
					} else {
						part.ContentBase = view.BaseUri;
						alternative.Add (part);
					}
				}

				body = alternative;
			}

			if (body == null)
				body = new TextPart (message.IsBodyHtml ? "html" : "plain");

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

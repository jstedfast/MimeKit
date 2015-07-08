//
// MimeMessage.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

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
		static readonly string[] StandardAddressHeaders = {
			"Resent-From", "Resent-Reply-To", "Resent-To", "Resent-Cc", "Resent-Bcc",
			"From", "Reply-To", "To", "Cc", "Bcc"
		};

		readonly Dictionary<string, InternetAddressList> addresses;
		MessageImportance importance = MessageImportance.Normal;
		MessagePriority priority = MessagePriority.Normal;
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
			addresses = new Dictionary<string, InternetAddressList> (StringComparer.OrdinalIgnoreCase);
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
			addresses = new Dictionary<string, InternetAddressList> (StringComparer.OrdinalIgnoreCase);
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
					throw new ArgumentOutOfRangeException ("value");
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
					throw new ArgumentOutOfRangeException ("value");
				}

				SetHeader ("Priority", rawValue);

				priority = value;
			}
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
					RemoveHeader (HeaderId.Sender);
					sender = null;
					return;
				}

				var options = FormatOptions.Default;
				var builder = new StringBuilder ();
				int len = "Sender: ".Length;

				value.Encode (options, builder, ref len);
				builder.Append (options.NewLine);

				var raw = Encoding.UTF8.GetBytes (builder.ToString ());

				ReplaceHeader (HeaderId.Sender, "Sender", raw);

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
					RemoveHeader (HeaderId.ResentSender);
					resentSender = null;
					return;
				}

				var options = FormatOptions.Default;
				var builder = new StringBuilder ();
				int len = "Resent-Sender: ".Length;

				value.Encode (options, builder, ref len);
				builder.Append (options.NewLine);

				var raw = Encoding.UTF8.GetBytes (builder.ToString ());

				ReplaceHeader (HeaderId.ResentSender, "Resent-Sender", raw);

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

				SetHeader ("Subject", value);
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

				SetHeader ("Date", DateUtils.FormatDate (value));
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

				SetHeader ("Resent-Date", DateUtils.FormatDate (value));
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
					RemoveHeader (HeaderId.InReplyTo);
					inreplyto = null;
					return;
				}

				var buffer = Encoding.UTF8.GetBytes (value);
				InternetAddress addr;
				int index = 0;

				if (!InternetAddress.TryParse (Headers.Options, buffer, ref index, buffer.Length, false, out addr) || !(addr is MailboxAddress))
					throw new ArgumentException ("Invalid Message-Id format.", "value");

				inreplyto = ((MailboxAddress) addr).Address;

				SetHeader ("In-Reply-To", "<" + inreplyto + ">");
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

				var buffer = Encoding.UTF8.GetBytes (value);
				InternetAddress addr;
				int index = 0;

				if (!InternetAddress.TryParse (Headers.Options, buffer, ref index, buffer.Length, false, out addr) || !(addr is MailboxAddress))
					throw new ArgumentException ("Invalid Message-Id format.", "value");

				messageId = ((MailboxAddress) addr).Address;

				SetHeader ("Message-Id", "<" + messageId + ">");
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

				var buffer = Encoding.UTF8.GetBytes (value);
				InternetAddress addr;
				int index = 0;

				if (!InternetAddress.TryParse (Headers.Options, buffer, ref index, buffer.Length, false, out addr) || !(addr is MailboxAddress))
					throw new ArgumentException ("Invalid Resent-Message-Id format.", "value");

				resentMessageId = ((MailboxAddress) addr).Address;

				SetHeader ("Resent-Message-Id", "<" + resentMessageId + ">");
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

				SetHeader ("MIME-Version", value.ToString ());
				version = value;
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
		/// Gets the text body of the message if it exists.
		/// </summary>
		/// <remarks>
		/// <para>Gets the text content of the first text/plain body part that is found (in depth-first
		/// search order) which is not an attachment.</para>
		/// </remarks>
		/// <value>The text body if it exists; otherwise, <c>null</c>.</value>
		public string TextBody {
			get { return GetTextBody (TextFormat.Text); }
		}

		/// <summary>
		/// Gets the html body of the message if it exists.
		/// </summary>
		/// <remarks>
		/// <para>Gets the HTML-formatted body of the message if it exists.</para>
		/// </remarks>
		/// <value>The html body if it exists; otherwise, <c>null</c>.</value>
		public string HtmlBody {
			get { return GetTextBody (TextFormat.Html); }
		}

		/// <summary>
		/// Gets the text body in the specified format.
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

				if (body != null && body.IsFormat (format))
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
		/// Gets the body parts of the message.
		/// </summary>
		/// <remarks>
		/// Traverses over the MIME tree, enumerating all of the <see cref="MimeEntity"/> objects,
		/// but does not traverse into the bodies of attached messages.
		/// </remarks>
		/// <value>The body parts.</value>
		public IEnumerable<MimeEntity> BodyParts {
			get { return EnumerateMimeParts (Body); }
		}

		/// <summary>
		/// Gets the attachments.
		/// </summary>
		/// <remarks>
		/// Traverses over the MIME tree, enumerating all of the <see cref="MimeEntity"/> objects that
		/// have a Content-Disposition header set to <c>"attachment"</c>.
		/// </remarks>
		/// <value>The attachments.</value>
		public IEnumerable<MimeEntity> Attachments {
			get { return EnumerateMimeParts (Body).Where (x => x.IsAttachment); }
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="MimeKit.MimeMessage"/>.
		/// </summary>
		/// <remarks>
		/// <para>Returns a <see cref="System.String"/> that represents the current <see cref="MimeKit.MimeMessage"/>.</para>
		/// <para>Note: In general, the string returned from this method SHOULD NOT be used for serializing the message
		/// to disk. It is recommended that you use <see cref="WriteTo(Stream,CancellationToken)"/> instead.</para>
		/// </remarks>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="MimeKit.MimeMessage"/>.</returns>
		public override string ToString ()
		{
			using (var memory = new MemoryStream ()) {
				WriteTo (FormatOptions.Default, memory);

#if !PORTABLE
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
		/// This default implementation for <see cref="MimeKit.MimeMessage"/> nodes
		/// calls <see cref="MimeKit.MimeVisitor.VisitMimeMessage"/>. Override this
		/// method to call into a more specific method on a derived visitor class
		/// of the <see cref="MimeKit.MimeVisitor"/> class. However, it should still
		/// support unknown visitors by calling
		/// <see cref="MimeKit.MimeVisitor.VisitMimeMessage"/>.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <c>null</c>.
		/// </exception>
		public virtual void Accept (MimeVisitor visitor)
		{
			if (visitor == null)
				throw new ArgumentNullException ("visitor");

			visitor.VisitMimeMessage (this);
		}

		/// <summary>
		/// Prepare the message for transport using the specified encoding constraints.
		/// </summary>
		/// <remarks>
		/// Prepares the message for transport using the specified encoding constraints.
		/// </remarks>
		/// <param name="constraint">The encoding constraint.</param>
		/// <param name="maxLineLength">The maximum allowable length for a line (not counting the CRLF). Must be between <c>72</c> and <c>998</c> (inclusive).</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="maxLineLength"/> is not between <c>72</c> and <c>998</c> (inclusive).</para>
		/// <para>-or-</para>
		/// <para><paramref name="constraint"/> is not a valid value.</para>
		/// </exception>
		public virtual void Prepare (EncodingConstraint constraint, int maxLineLength = 78)
		{
			if (maxLineLength < 72 || maxLineLength > 998)
				throw new ArgumentOutOfRangeException ("maxLineLength");

			if (Body != null)
				Body.Prepare (constraint, maxLineLength);
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
		public void WriteTo (FormatOptions options, Stream stream, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (stream == null)
				throw new ArgumentNullException ("stream");

			if (version == null && Body != null && Body.Headers.Count > 0)
				MimeVersion = new Version (1, 0);

			if (Body != null) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (options.CreateNewLineFilter ());

					foreach (var header in MergeHeaders ()) {
						if (options.HiddenHeaders.Contains (header.Id))
							continue;

						filtered.Write (header.RawField, 0, header.RawField.Length, cancellationToken);
						filtered.Write (new [] { (byte) ':' }, 0, 1, cancellationToken);
						filtered.Write (header.RawValue, 0, header.RawValue.Length, cancellationToken);
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

				try {
					Body.Headers.Suppress = true;
					Body.WriteTo (options, stream, cancellationToken);
				} finally {
					Body.Headers.Suppress = false;
				}
			} else {
				Headers.WriteTo (options, stream, cancellationToken);
			}
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
		public void WriteTo (Stream stream, CancellationToken cancellationToken = default (CancellationToken))
		{
			WriteTo (FormatOptions.Default, stream, cancellationToken);
		}

#if !PORTABLE
		/// <summary>
		/// Writes the message to the specified file.
		/// </summary>
		/// <remarks>
		/// Writes the message to the specified file using the provided formatting options.
		/// </remarks>
		/// <param name="options">The formatting options.</param>
		/// <param name="fileName">The file.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters as defined by
		/// <see cref="System.IO.Path.InvalidPathChars"/>.
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
				throw new ArgumentNullException ("options");

			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			using (var stream = File.OpenWrite (fileName))
				WriteTo (options, stream, cancellationToken);
		}

		/// <summary>
		/// Writes the message to the specified file.
		/// </summary>
		/// <remarks>
		/// Writes the message to the specified file using the default formatting options.
		/// </remarks>
		/// <param name="fileName">The file.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters as defined by
		/// <see cref="System.IO.Path.InvalidPathChars"/>.
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
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			using (var stream = File.OpenWrite (fileName))
				WriteTo (FormatOptions.Default, stream, cancellationToken);
		}
#endif

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
		static void DkimWriteHeaderRelaxed (FormatOptions options, Stream stream, Header header)
		{
			var name = Encoding.ASCII.GetBytes (header.Field.ToLowerInvariant ());
			var rawValue = header.GetRawValue (options);
			int index = 0;

			stream.Write (name, 0, name.Length);
			stream.WriteByte ((byte) ':');

			// look for the first non-whitespace character
			while (index < rawValue.Length && rawValue[index].IsBlank ())
				index++;

			while (index < rawValue.Length) {
				int startIndex = index;
				int endIndex, nextLine;

				// look for the first non-whitespace character
				while (index < rawValue.Length && rawValue[index].IsBlank ())
					index++;

				// look for the end of the line
				endIndex = index;
				while (endIndex < rawValue.Length && rawValue[endIndex] != (byte) '\n')
					endIndex++;

				nextLine = endIndex + 1;

				if (endIndex > index && rawValue[endIndex - 1] == (byte) '\r')
					endIndex--;

				if (index > startIndex)
					stream.WriteByte ((byte) ' ');

				while (index < endIndex) {
					startIndex = index;

					while (index < endIndex && !rawValue[index].IsBlank ())
						index++;

					stream.Write (rawValue, startIndex, index - startIndex);

					startIndex = index;

					while (index < endIndex && rawValue[index].IsBlank ())
						index++;

					if (index > startIndex)
						stream.WriteByte ((byte) ' ');
				}

				index = nextLine;
			}

			stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
		}

		static void DkimWriteHeaderSimple (FormatOptions options, Stream stream, Header header)
		{
			var rawValue = header.GetRawValue (options);

			stream.Write (header.RawField, 0, header.RawField.Length);
			stream.Write (new [] { (byte) ':' }, 0, 1);
			stream.Write (rawValue, 0, rawValue.Length);
		}

		static ISigner DkimGetDigestSigner (DkimSignatureAlgorithm algorithm, AsymmetricKeyParameter key)
		{
			DerObjectIdentifier id;

			if (algorithm == DkimSignatureAlgorithm.RsaSha256)
				id = PkcsObjectIdentifiers.Sha256WithRsaEncryption;
			else
				id = PkcsObjectIdentifiers.Sha1WithRsaEncryption;

			var signer = SignerUtilities.GetSigner (id);

			signer.Init (key.IsPrivate, key);

			return signer;
		}

		byte[] DkimHashBody (FormatOptions options, DkimSignatureAlgorithm signatureAlgorithm, DkimCanonicalizationAlgorithm bodyCanonicalizationAlgorithm, int maxLength)
		{
			using (var stream = new DkimHashStream (signatureAlgorithm, maxLength)) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (options.CreateNewLineFilter ());

					if (bodyCanonicalizationAlgorithm == DkimCanonicalizationAlgorithm.Relaxed)
						filtered.Add (new DkimRelaxedBodyFilter ());
					else
						filtered.Add (new DkimSimpleBodyFilter ());

					if (Body != null) {
						try {
							Body.Headers.Suppress = true;
							Body.WriteTo (options, stream, CancellationToken.None);
						} finally {
							Body.Headers.Suppress = false;
						}
					}

					filtered.Flush ();
				}

				return stream.GenerateHash ();
			}
		}

		void DkimWriteHeaders (FormatOptions options, IList<string> fields, DkimCanonicalizationAlgorithm headerCanonicalizationAlgorithm, Stream stream)
		{
			var counts = new Dictionary<string, int> ();
			Header header;

			for (int i = 0; i < fields.Count; i++) {
				var name = fields[i].ToLowerInvariant ();
				int index, count, n = 0;

				if (!counts.TryGetValue (name, out count))
					count = 0;

				// Note: signers choosing to sign an existing header field that occurs more
				// than once in the message (such as Received) MUST sign the physically last
				// instance of that header field in the header block. Signers wishing to sign
				// multiple instances of such a header field MUST include the header field
				// name multiple times in the list of header fields and MUST sign such header
				// fields in order from the bottom of the header field block to the top.
				index = Headers.LastIndexOf (name);

				// find the n'th header with this name
				while (n < count && --index >= 0) {
					if (Headers[index].Field.Equals (name, StringComparison.OrdinalIgnoreCase))
						n++;
				}

				if (index < 0)
					continue;

				header = Headers[index];

				switch (headerCanonicalizationAlgorithm) {
				case DkimCanonicalizationAlgorithm.Relaxed:
					DkimWriteHeaderRelaxed (options, stream, header);
					break;
				default:
					DkimWriteHeaderSimple (options, stream, header);
					break;
				}

				counts[name] = ++count;
			}
		}

		static readonly HeaderId[] DkimShouldNotInclude = { HeaderId.ReturnPath, HeaderId.Received, HeaderId.Comments, HeaderId.Keywords, HeaderId.Bcc, HeaderId.ResentBcc, HeaderId.DkimSignature, HeaderId.Unknown };

		/// <summary>
		/// Digitally sign the message using a DomainKeys Identified Mail (DKIM) signature.
		/// </summary>
		/// <remarks>
		/// Digitally signs the message using a DomainKeys Identified Mail (DKIM) signature.
		/// </remarks>
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
		void Sign (FormatOptions options, DkimSigner signer, IList<HeaderId> headers, DkimCanonicalizationAlgorithm headerCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Simple, DkimCanonicalizationAlgorithm bodyCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Simple)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (headers == null)
				throw new ArgumentNullException ("headers");

			if (!headers.Contains (HeaderId.From))
				throw new ArgumentException ("The list of headers to sign MUST include the 'From' header.");

			var fields = new string[headers.Count];
			for (int i = 0; i < headers.Count; i++) {
				if (DkimShouldNotInclude.Contains (headers[i]))
					throw new ArgumentException (string.Format ("The list of headers to sign SHOULD NOT include the '{0}' header.", headers[i].ToHeaderName ()));

				fields[i] = headers[i].ToHeaderName ().ToLowerInvariant ();
			}

			if (version == null && Body != null && Body.Headers.Count > 0)
				MimeVersion = new Version (1, 0);

			Prepare (EncodingConstraint.SevenBit, 78);

			var t = DateTime.Now - DateUtils.UnixEpoch;
			var value = new StringBuilder ("v=1");
			byte[] signature, hash;
			Header dkim;

			options = options.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;

			switch (signer.SignatureAlgorithm) {
			case DkimSignatureAlgorithm.RsaSha256:
				value.Append ("; a=rsa-sha256");
				break;
			default:
				value.Append ("; a=rsa-sha1");
				break;
			}

			value.AppendFormat ("; d={0}; s={1}", signer.Domain, signer.Selector);
			value.AppendFormat ("; c={0}/{1}",
				headerCanonicalizationAlgorithm.ToString ().ToLowerInvariant (),
				bodyCanonicalizationAlgorithm.ToString ().ToLowerInvariant ());
			if (!string.IsNullOrEmpty (signer.QueryMethod))
				value.AppendFormat ("; q={0}", signer.QueryMethod);
			if (!string.IsNullOrEmpty (signer.AgentOrUserIdentifier))
				value.AppendFormat ("; i={0}", signer.AgentOrUserIdentifier);
			value.AppendFormat ("; t={0}", (long) t.TotalSeconds);

			using (var stream = new DkimSignatureStream (DkimGetDigestSigner (signer.SignatureAlgorithm, signer.PrivateKey))) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (options.CreateNewLineFilter ());

					// write the specified message headers
					DkimWriteHeaders (options, fields, headerCanonicalizationAlgorithm, filtered);

					value.AppendFormat ("; h={0}", string.Join (":", fields.ToArray ()));

					hash = DkimHashBody (options, signer.SignatureAlgorithm, bodyCanonicalizationAlgorithm, -1);
					value.AppendFormat ("; bh={0}", Convert.ToBase64String (hash));
					value.Append ("; b=");

					dkim = new Header (HeaderId.DkimSignature, value.ToString ());
					Headers.Insert (0, dkim);

					switch (headerCanonicalizationAlgorithm) {
					case DkimCanonicalizationAlgorithm.Relaxed:
						DkimWriteHeaderRelaxed (options, filtered, dkim);
						break;
					default:
						DkimWriteHeaderSimple (options, filtered, dkim);
						break;
					}

					filtered.Flush ();
				}

				signature = stream.GenerateSignature ();

				dkim.Value += Convert.ToBase64String (signature);
			}
		}

		/// <summary>
		/// Digitally sign the message using a DomainKeys Identified Mail (DKIM) signature.
		/// </summary>
		/// <remarks>
		/// Digitally signs the message using a DomainKeys Identified Mail (DKIM) signature.
		/// </remarks>
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
		public void Sign (DkimSigner signer, IList<HeaderId> headers, DkimCanonicalizationAlgorithm headerCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Simple, DkimCanonicalizationAlgorithm bodyCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Simple)
		{
			Sign (FormatOptions.Default, signer, headers, headerCanonicalizationAlgorithm, bodyCanonicalizationAlgorithm);
		}

		static bool IsWhiteSpace (char c)
		{
			return c == ' ' || c == '\t';
		}

		static IDictionary<string, string> ParseDkimSignature (string signature)
		{
			var parameters = new Dictionary<string, string> ();

			foreach (var token in signature.Split (';')) {
				var value = new StringBuilder ();
				int startIndex, index = 0;
				string name;

				while (index < token.Length && IsWhiteSpace (token[index]))
					index++;

				startIndex = index;

				while (index < token.Length && token[index] != '=')
					index++;

				if (index + 1 >= token.Length)
					throw new FormatException ("Malformed DKIM-Signature value.");

				name = token.Substring (startIndex, index - startIndex).Trim ();
				index++;

				while (index < token.Length) {
					if (!IsWhiteSpace (token[index]))
						value.Append (token[index]);
					index++;
				}

				if (parameters.ContainsKey (name))
					throw new FormatException (string.Format ("Malformed DKIM-Signature value: duplicate parameter '{0}'.", name));

				parameters.Add (name, value.ToString ());
			}

			return parameters;
		}

		static void ValidateDkimSignatureParameters (IDictionary<string, string> parameters, out DkimSignatureAlgorithm algorithm, out DkimCanonicalizationAlgorithm headerAlgorithm,
			out DkimCanonicalizationAlgorithm bodyAlgorithm, out string d, out string s, out string q, out string h, out string bh, out string b, out int maxLength)
		{
			string v, a, c, l;

			if (!parameters.TryGetValue ("v", out v))
				throw new FormatException ("Malformed DKIM-Signature header: no version parameter detected.");

			if (v != "1")
				throw new FormatException (string.Format ("Unrecognized DKIM-Signature version: v={0}", v));

			if (!parameters.TryGetValue ("a", out a))
				throw new FormatException ("Malformed DKIM-Signature header: no signature algorithm parameter detected.");

			switch (a.ToLowerInvariant ()) {
			case "rsa-sha256": algorithm = DkimSignatureAlgorithm.RsaSha256; break;
			case "rsa-sha1": algorithm = DkimSignatureAlgorithm.RsaSha1; break;
			default: throw new FormatException (string.Format ("Unrecognized DKIM-Signature algorithm parameter: a={0}", a));
			}

			if (!parameters.TryGetValue ("d", out d))
				throw new FormatException ("Malformed DKIM-Signature header: no domain parameter detected.");

			if (!parameters.TryGetValue ("s", out s))
				throw new FormatException ("Malformed DKIM-Signature header: no selector parameter detected.");

			if (!parameters.TryGetValue ("q", out q))
				q = "dns/txt";

			if (parameters.TryGetValue ("l", out l)) {
				if (!int.TryParse (l, out maxLength))
					throw new FormatException (string.Format ("Malformed DKIM-Signature header: invalid length parameter: l={0}", l));
			} else {
				maxLength = -1;
			}

			if (parameters.TryGetValue ("c", out c)) {
				var tokens = c.ToLowerInvariant ().Split ('/');

				if (tokens.Length == 0 || tokens.Length > 2)
					throw new FormatException (string.Format ("Malformed DKIM-Signature header: invalid canonicalization parameter: c={0}", c));

				switch (tokens[0]) {
				case "relaxed": headerAlgorithm = DkimCanonicalizationAlgorithm.Relaxed; break;
				case "simple": headerAlgorithm = DkimCanonicalizationAlgorithm.Simple; break;
				default: throw new FormatException (string.Format ("Malformed DKIM-Signature header: invalid canonicalization parameter: c={0}", c));
				}

				if (tokens.Length == 2) {
					switch (tokens[1]) {
					case "relaxed": bodyAlgorithm = DkimCanonicalizationAlgorithm.Relaxed; break;
					case "simple": bodyAlgorithm = DkimCanonicalizationAlgorithm.Simple; break;
					default: throw new FormatException (string.Format ("Malformed DKIM-Signature header: invalid canonicalization parameter: c={0}", c));
					}
				} else {
					bodyAlgorithm = DkimCanonicalizationAlgorithm.Simple;
				}
			} else {
				headerAlgorithm = DkimCanonicalizationAlgorithm.Simple;
				bodyAlgorithm = DkimCanonicalizationAlgorithm.Simple;
			}

			if (!parameters.TryGetValue ("h", out h))
				throw new FormatException ("Malformed DKIM-Signature header: no signed header parameter detected.");

			if (!parameters.TryGetValue ("bh", out bh))
				throw new FormatException ("Malformed DKIM-Signature header: no body hash parameter detected.");

			if (!parameters.TryGetValue ("b", out b))
				throw new FormatException ("Malformed DKIM-Signature header: no signature parameter detected.");
		}

		static Header GetSignedDkimSignatureHeader (Header dkimSignature)
		{
			// modify the raw DKIM-Signature header value by chopping off the signature after the "b=" at the end
			var rawValue = (byte[]) dkimSignature.RawValue.Clone ();
			int length = rawValue.Length;

			// find the last ';' before the b=...
			while (length > 0 && rawValue[length - 1] != (byte) ';')
				length--;

			// skip over any whitespace
			while (length < rawValue.Length && rawValue[length].IsWhitespace ())
				length++;

			if (length + 1 >= rawValue.Length || (rawValue[length] != (byte) 'b' && rawValue[length + 1] != (byte) '='))
				throw new FormatException ("Malformed DKIM-Signature header: signature parameter is not at the end.");

			// skip over "b="
			length += 2;

			if (length + 2 < rawValue.Length) {
				rawValue[length++] = (byte) '\r';
				rawValue[length++] = (byte) '\n';
			}

			Array.Resize (ref rawValue, length);

			return new Header (dkimSignature.Options, dkimSignature.RawField, rawValue);
		}

		/// <summary>
		/// Verify the specified DKIM-Signature header.
		/// </summary>
		/// <remarks>
		/// Verifies the specified DKIM-Signature header.
		/// </remarks>
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
		bool Verify (FormatOptions options, Header dkimSignature, IDkimPublicKeyLocator publicKeyLocator, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (dkimSignature == null)
				throw new ArgumentNullException ("dkimSignature");

			if (dkimSignature.Id != HeaderId.DkimSignature)
				throw new ArgumentException ("The dkimSignature parameter MUST be a DKIM-Signature header.", "dkimSignature");

			if (publicKeyLocator == null)
				throw new ArgumentNullException ("publicKeyLocator");

			var parameters = ParseDkimSignature (dkimSignature.Value);
			DkimCanonicalizationAlgorithm headerAlgorithm, bodyAlgorithm;
			DkimSignatureAlgorithm signatureAlgorithm;
			AsymmetricKeyParameter key;
			string d, s, q, h, bh, b;
			int maxLength;

			ValidateDkimSignatureParameters (parameters, out signatureAlgorithm, out headerAlgorithm, out bodyAlgorithm,
				out d, out s, out q, out h, out bh, out b, out maxLength);

			key = publicKeyLocator.LocatePublicKey (q, d, s, cancellationToken);

			options = options.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;

			// first check the body hash (if that's invalid, then the entire signature is invalid)
			var hash = Convert.ToBase64String (DkimHashBody (options, signatureAlgorithm, bodyAlgorithm, maxLength));

			if (hash != bh)
				return false;

			using (var stream = new DkimSignatureStream (DkimGetDigestSigner (signatureAlgorithm, key))) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (options.CreateNewLineFilter ());

					DkimWriteHeaders (options, h.Split (':'), headerAlgorithm, filtered);

					// now include the DKIM-Signature header that we are verifying,
					// but only after removing the "b=" signature value.
					var header = GetSignedDkimSignatureHeader (dkimSignature);

					switch (headerAlgorithm) {
					case DkimCanonicalizationAlgorithm.Relaxed:
						DkimWriteHeaderRelaxed (options, filtered, header);
						break;
					default:
						DkimWriteHeaderSimple (options, filtered, header);
						break;
					}

					filtered.Flush ();
				}

				return stream.VerifySignature (b);
			}
		}

		/// <summary>
		/// Verify the specified DKIM-Signature header.
		/// </summary>
		/// <remarks>
		/// Verifies the specified DKIM-Signature header.
		/// </remarks>
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
		public bool Verify (Header dkimSignature, IDkimPublicKeyLocator publicKeyLocator, CancellationToken cancellationToken = default (CancellationToken))
		{
			return Verify (FormatOptions.Default, dkimSignature, publicKeyLocator, cancellationToken);
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
				Body = MultipartEncrypted.Encrypt ((OpenPgpContext) ctx, recipients, Body);
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
				Body = MultipartEncrypted.SignAndEncrypt ((OpenPgpContext) ctx, signer, digestAlgo, recipients, Body);
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

		void SerializeAddressList (string field, InternetAddressList list)
		{
			var builder = new StringBuilder (" ");
			var options = FormatOptions.Default;
			int lineLength = field.Length + 2;

			list.Encode (options, builder, ref lineLength);
			builder.Append (options.NewLine);

			var raw = Encoding.UTF8.GetBytes (builder.ToString ());

			ReplaceHeader (field.ToHeaderId (), field, raw);
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
			case HeaderId.Importance:
				importance = MessageImportance.Normal;
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
				InternetAddress address;
				int index = 0;

				switch (id) {
				case HeaderId.MimeVersion:
					if (MimeUtils.TryParse (rawValue, 0, rawValue.Length, out version))
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
					if (DateUtils.TryParse (rawValue, 0, rawValue.Length, out resentDate))
						return;
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
				case HeaderId.Date:
					if (DateUtils.TryParse (rawValue, 0, rawValue.Length, out date))
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
				case HeaderId.Date:
					DateUtils.TryParse (rawValue, 0, rawValue.Length, out date);
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

				importance = MessageImportance.Normal;
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
		/// it will use a <see cref="MimeKit.IO.BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save mmeory usage, but also improve <see cref="MimeParser"/>
		/// performance.</para>
		/// </remarks>
		/// <returns>The parsed message.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="stream">The stream.</param>
		/// <param name="persistent"><c>true</c> if the stream is persistent; otherwise <c>false</c>.</param>
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
		public static MimeMessage Load (ParserOptions options, Stream stream, bool persistent, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (stream == null)
				throw new ArgumentNullException ("stream");

			var parser = new MimeParser (options, stream, MimeFormat.Entity, persistent);

			return parser.ParseMessage (cancellationToken);
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
		public static MimeMessage Load (ParserOptions options, Stream stream, CancellationToken cancellationToken = default (CancellationToken))
		{
			return Load (options, stream, false, cancellationToken);
		}

		/// <summary>
		/// Load a <see cref="MimeMessage"/> from the specified stream.
		/// </summary>
		/// <remarks>
		/// <para>Loads a <see cref="MimeMessage"/> from the given stream, using the
		/// default <see cref="ParserOptions"/>.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="MimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="MimeKit.IO.BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save mmeory usage, but also improve <see cref="MimeParser"/>
		/// performance.</para>
		/// </remarks>
		/// <returns>The parsed message.</returns>
		/// <param name="stream">The stream.</param>
		/// <param name="persistent"><c>true</c> if the stream is persistent; otherwise <c>false</c>.</param>
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
		public static MimeMessage Load (Stream stream, bool persistent, CancellationToken cancellationToken = default (CancellationToken))
		{
			return Load (ParserOptions.Default, stream, persistent, cancellationToken);
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
		public static MimeMessage Load (Stream stream, CancellationToken cancellationToken = default (CancellationToken))
		{
			return Load (ParserOptions.Default, stream, false, cancellationToken);
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
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters as defined by
		/// <see cref="System.IO.Path.InvalidPathChars"/>.
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
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters as defined by
		/// <see cref="System.IO.Path.InvalidPathChars"/>.
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
#endif // !PORTABLE

#if ENABLE_SNM
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

			var stream = new MemoryBlockStream ();
			item.ContentStream.CopyTo (stream);
			stream.Position = 0;

			part.ContentObject = new ContentObject (stream);

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

			// Note: If the user has already sent their MailMessage via System.Net.Mail.SmtpClient,
			// then the following MailMessage properties will have been merged into the Headers, so
			// check to make sure our MimeMessage properties are empty before adding them.
			if (msg.Sender == null && message.Sender != null)
				msg.Sender = (MailboxAddress) message.Sender;
			if (msg.From.Count == 0 && message.From != null)
				msg.From.Add ((MailboxAddress) message.From);
#if NET_3_5
			if (msg.ReplyTo.Count == 0 && message.ReplyTo != null)
				msg.ReplyTo.Add ((MailboxAddress) message.ReplyTo);
#else
			if (msg.ReplyTo.Count == 0 && message.ReplyToList.Count > 0)
				msg.ReplyTo.AddRange ((InternetAddressList) message.ReplyToList);
#endif
			if (msg.To.Count == 0 && message.To.Count > 0)
				msg.To.AddRange ((InternetAddressList) message.To);
			if (msg.Cc.Count == 0 && message.CC.Count > 0)
				msg.Cc.AddRange ((InternetAddressList) message.CC);
			if (msg.Bcc.Count == 0 && message.Bcc.Count > 0)
				msg.Bcc.AddRange ((InternetAddressList) message.Bcc);
			if (string.IsNullOrEmpty (msg.Subject))
				msg.Subject = message.Subject ?? string.Empty;

			switch (message.Priority) {
			case MailPriority.High:
				msg.Headers.Replace (HeaderId.Priority, "urgent");
				msg.Headers.Replace (HeaderId.Importance, "high");
				msg.Headers.Replace ("X-Priority", "1");
				break;
			case MailPriority.Low:
				msg.Headers.Replace (HeaderId.Priority, "non-urgent");
				msg.Headers.Replace (HeaderId.Importance, "low");
				msg.Headers.Replace ("X-Priority", "5");
				break;
			}

			if (!string.IsNullOrEmpty (message.Body)) {
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

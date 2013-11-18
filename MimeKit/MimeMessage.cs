//
// MimeMessage.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Jeffrey Stedfast
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
using System.Collections.Generic;
using System.Net.Mail;

using MimeKit.Utils;
using MimeKit.IO;

namespace MimeKit {
	/// <summary>
	/// A MIME message.
	/// </summary>
	public sealed class MimeMessage
	{
		static readonly StringComparer icase = StringComparer.OrdinalIgnoreCase;
		static readonly string[] StandardAddressHeaders = new string[] {
			"Sender", "From", "Reply-To", "To", "Cc", "Bcc"
		};

		readonly Dictionary<string, InternetAddressList> addresses;
		readonly MessageIdList references;
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
		/// <param name="args">An array of initialization parameters: headers and message parts.</param>
		/// </summary>
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

			if (!Headers.Contains ("From"))
				Headers["From"] = string.Empty;
			if (!Headers.Contains ("To"))
				Headers["To"] = string.Empty;
			if (date == default (DateTimeOffset))
				Date = DateTimeOffset.Now;
			if (!Headers.Contains ("Subject"))
				Subject = string.Empty;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MimeMessage"/> class.
		/// </summary>
		public MimeMessage () : this (ParserOptions.Default.Clone ())
		{
			Headers["From"] = string.Empty;
			Headers["To"] = string.Empty;
			Date = DateTimeOffset.Now;
			Subject = string.Empty;
		}

		/// <summary>
		/// Gets the list of headers.
		/// </summary>
		/// <value>The list of headers.</value>
		public HeaderList Headers {
			get; private set;
		}

		/// <summary>
		/// Gets the list of addresses in the Sender header.
		/// </summary>
		/// <value>The list of addresses in the Sender header.</value>
		public InternetAddressList Sender {
			get { return addresses["Sender"]; }
		}

		/// <summary>
		/// Gets the list of addresses in the From header.
		/// </summary>
		/// <value>The list of addresses in the From header.</value>
		public InternetAddressList From {
			get { return addresses["From"]; }
		}

		/// <summary>
		/// Gets the list of addresses in the Reply-To header.
		/// </summary>
		/// <value>The list of addresses in the Reply-To header.</value>
		public InternetAddressList ReplyTo {
			get { return addresses["Reply-To"]; }
		}

		/// <summary>
		/// Gets the list of addresses in the To header.
		/// </summary>
		/// <value>The list of addresses in the To header.</value>
		public InternetAddressList To {
			get { return addresses["To"]; }
		}

		/// <summary>
		/// Gets the list of addresses in the Cc header.
		/// </summary>
		/// <value>The list of addresses in the Cc header.</value>
		public InternetAddressList Cc {
			get { return addresses["Cc"]; }
		}

		/// <summary>
		/// Gets the list of addresses in the Bcc header.
		/// </summary>
		/// <value>The list of addresses in the Bcc header.</value>
		public InternetAddressList Bcc {
			get { return addresses["Bcc"]; }
		}

		/// <summary>
		/// Gets or sets the subject of the message.
		/// </summary>
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
		/// <value>The date of the message.</value>
		public DateTimeOffset Date {
			get { return date; }
			set {
				if (date == value)
					return;

				date = value;

				Headers.Changed -= HeadersChanged;
				Headers["Date"] = DateUtils.FormatDate (date);
				Headers.Changed += HeadersChanged;
			}
		}

		/// <summary>
		/// Gets or sets the list of references to other messages.
		/// </summary>
		/// <value>The references.</value>
		public MessageIdList References {
			get { return references; }
		}

		/// <summary>
		/// Gets or sets the message-id that this message is in reply to.
		/// </summary>
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
					Headers.RemoveAll ("In-Reply-To");
					Headers.Changed += HeadersChanged;
					return;
				}

				var buffer = Encoding.ASCII.GetBytes (value);
				InternetAddress addr;
				int index = 0;

				if (!InternetAddress.TryParse (Headers.Options, buffer, ref index, buffer.Length, false, out addr) || !(addr is MailboxAddress))
					throw new ArgumentException ("Invalid Message-Id format.", "value");

				inreplyto = "<" + ((MailboxAddress) addr).Address + ">";

				Headers.Changed -= HeadersChanged;
				Headers["In-Reply-To"] = inreplyto;
				Headers.Changed += HeadersChanged;
			}
		}

		/// <summary>
		/// Gets or sets the message identifier.
		/// </summary>
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

				messageId = "<" + ((MailboxAddress) addr).Address + ">";

				Headers.Changed -= HeadersChanged;
				Headers["Message-Id"] = messageId;
				Headers.Changed += HeadersChanged;
			}
		}

		/// <summary>
		/// Gets or sets the MIME-Version.
		/// </summary>
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
		/// <value>The body of the message.</value>
		public MimeEntity Body {
			get; set;
		}

		/// <summary>
		/// Writes the message to the specified stream.
		/// </summary>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		public void WriteTo (FormatOptions options, Stream stream)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (stream == null)
				throw new ArgumentNullException ("stream");

			if (!Headers.Contains ("Date"))
				Date = DateTimeOffset.Now;

			if (messageId == null)
				MessageId = MimeUtils.GenerateMessageId ();

			if (version == null && Body != null && Body.Headers.Count > 0)
				MimeVersion = new Version (1, 0);

			if (Body == null) {
				Headers.WriteTo (stream);

				stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
			} else {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (options.CreateNewLineFilter ());

					foreach (var header in MergeHeaders ()) {
						var name = Encoding.ASCII.GetBytes (header.Field);

						filtered.Write (name, 0, name.Length);
						filtered.WriteByte ((byte) ':');
						filtered.Write (header.RawValue, 0, header.RawValue.Length);
					}

					filtered.Flush ();
				}

				options.WriteHeaders = false;
				Body.WriteTo (options, stream);
			}
		}

		/// <summary>
		/// Writes the message to the specified stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		public void WriteTo (Stream stream)
		{
			WriteTo (FormatOptions.Default, stream);
		}

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

		void InternetAddressListChanged (object sender, EventArgs e)
		{
			var list = (InternetAddressList) sender;

			foreach (var name in StandardAddressHeaders) {
				if (addresses[name] == list) {
					SerializeAddressList (name, list);
					break;
				}
			}
		}

		void ReferencesChanged (object sender, EventArgs e)
		{
			if (references.Count > 0) {
				int lineLength = "References".Length + 1;
				var options = FormatOptions.Default;
				var builder = new StringBuilder ();

				for (int i = 0; i < references.Count; i++) {
					if (lineLength + references[i].Length >= options.MaxLineLength) {
						builder.Append (options.NewLine);
						builder.Append ('\t');
						lineLength = 1;
					} else {
						builder.Append (' ');
						lineLength++;
					}

					lineLength += references[i].Length;
					builder.Append (references[i]);
				}

				builder.Append (options.NewLine);

				var raw = Encoding.UTF8.GetBytes (builder.ToString ());

				Headers.Changed -= HeadersChanged;
				Headers.Replace (new Header (Headers.Options, "References", raw));
				Headers.Changed += HeadersChanged;
			} else {
				Headers.Changed -= HeadersChanged;
				Headers.RemoveAll ("References");
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

		void ReloadAddressList (string field, InternetAddressList list)
		{
			// clear the address list and reload
			list.Changed -= InternetAddressListChanged;
			list.Clear ();

			foreach (var header in Headers) {
				if (icase.Compare (header.Field, field) != 0)
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

		void ReloadHeader (HeaderId id, string field)
		{
			if (id == HeaderId.Unknown)
				return;

			if (id == HeaderId.References) {
				references.Changed -= ReferencesChanged;
				references.Clear ();
				references.Changed += ReferencesChanged;
			} else if (id == HeaderId.InReplyTo) {
				inreplyto = null;
			}

			foreach (var header in Headers) {
				if (header.Id != id)
					continue;

				switch (id) {
				case HeaderId.MimeVersion:
					if (MimeUtils.TryParseVersion (header.RawValue, 0, header.RawValue.Length, out version))
						return;
					break;
				case HeaderId.References:
					references.Changed -= ReferencesChanged;
					foreach (var msgid in MimeUtils.EnumerateReferences (header.RawValue, 0, header.RawValue.Length))
						references.Add (msgid);
					references.Changed += ReferencesChanged;
					break;
				case HeaderId.InReplyTo:
					inreplyto = MimeUtils.EnumerateReferences (header.RawValue, 0, header.RawValue.Length).FirstOrDefault ();
					break;
				case HeaderId.MessageId:
					messageId = MimeUtils.EnumerateReferences (header.RawValue, 0, header.RawValue.Length).FirstOrDefault ();
					if (messageId != null)
						return;
					break;
				case HeaderId.Date:
					if (DateUtils.TryParseDateTime (header.RawValue, 0, header.RawValue.Length, out date))
						return;
					break;
				}
			}
		}

		void HeadersChanged (object sender, HeaderListChangedEventArgs e)
		{
			InternetAddressList list;

			switch (e.Action) {
			case HeaderListChangedAction.Added:
				if (addresses.TryGetValue (e.Header.Field, out list)) {
					AddAddresses (e.Header, list);
					break;
				}

				switch (e.Header.Id) {
				case HeaderId.MimeVersion:
					MimeUtils.TryParseVersion (e.Header.RawValue, 0, e.Header.RawValue.Length, out version);
					break;
				case HeaderId.References:
					references.Changed -= ReferencesChanged;
					foreach (var msgid in MimeUtils.EnumerateReferences (e.Header.RawValue, 0, e.Header.RawValue.Length))
						references.Add (msgid);
					references.Changed += ReferencesChanged;
					break;
				case HeaderId.InReplyTo:
					inreplyto = MimeUtils.EnumerateReferences (e.Header.RawValue, 0, e.Header.RawValue.Length).FirstOrDefault ();
					break;
				case HeaderId.MessageId:
					messageId = MimeUtils.EnumerateReferences (e.Header.RawValue, 0, e.Header.RawValue.Length).FirstOrDefault ();
					break;
				case HeaderId.Date:
					DateUtils.TryParseDateTime (e.Header.RawValue, 0, e.Header.RawValue.Length, out date);
					break;
				}
				break;
			case HeaderListChangedAction.Changed:
			case HeaderListChangedAction.Removed:
				if (addresses.TryGetValue (e.Header.Field, out list)) {
					ReloadAddressList (e.Header.Field, list);
					break;
				}

				ReloadHeader (e.Header.Id, e.Header.Field);
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

				inreplyto = null;
				messageId = null;
				version = null;
				break;
			default:
				throw new ArgumentOutOfRangeException ();
			}
		}

		/// <summary>
		/// Load a <see cref="MimeMessage"/> from the specified stream.
		/// </summary>
		/// <returns>The parsed message.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="stream">The stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		public static MimeMessage Load (ParserOptions options, Stream stream)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (stream == null)
				throw new ArgumentNullException ("stream");

			var parser = new MimeParser (options, stream, MimeFormat.Entity);

			return parser.ParseMessage ();
		}

		/// <summary>
		/// Load a <see cref="MimeMessage"/> from the specified stream.
		/// </summary>
		/// <returns>The parsed message.</returns>
		/// <param name="stream">The stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		public static MimeMessage Load (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			var parser = new MimeParser (stream, MimeFormat.Entity);

			return parser.ParseMessage ();
		}

		/// <summary>
		/// Load a <see cref="MimeMessage"/> from the specified file.
		/// </summary>
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
		/// <exception cref="System.IO.IOException">
		/// An error occurred reading the file.
		/// </exception>
		public static MimeMessage Load (ParserOptions options, string fileName)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			using (var stream = File.OpenRead (fileName)) {
				return Load (options, stream);
			}
		}

		/// <summary>
		/// Load a <see cref="MimeMessage"/> from the specified file.
		/// </summary>
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
		/// <exception cref="System.IO.IOException">
		/// An error occurred reading the file.
		/// </exception>
		public static MimeMessage Load (string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			using (var stream = File.OpenRead (fileName)) {
				return Load (stream);
			}
		}

		#region System.Net.Mail support

		static System.Net.Mime.ContentType GetContentType (ContentType contentType)
		{
			var ctype = new System.Net.Mime.ContentType ();
			ctype.MediaType = string.Format ("{0}/{1}", contentType.MediaType, contentType.MediaSubtype);

			foreach (var param in contentType.Parameters)
				ctype.Parameters.Add (param.Name, param.Value);

			return ctype;
		}

		static System.Net.Mime.TransferEncoding GetTransferEncoding (ContentEncoding encoding)
		{
			switch (encoding) {
			case ContentEncoding.QuotedPrintable:
			case ContentEncoding.EightBit:
				return System.Net.Mime.TransferEncoding.QuotedPrintable;
			case ContentEncoding.SevenBit:
				return System.Net.Mime.TransferEncoding.SevenBit;
			default:
				return System.Net.Mime.TransferEncoding.Base64;
			}
		}

		static void AddBodyPart (MailMessage message, MimeEntity entity)
		{
			if (entity is MessagePart) {
				// FIXME: how should this be converted into a MailMessage?
			} else if (entity is Multipart) {
				var multipart = (Multipart) entity;

				if (multipart.ContentType.Matches ("multipart", "alternative")) {
					foreach (var part in multipart.OfType<MimePart> ()) {
						// clone the content
						var content = new MemoryStream ();
						part.ContentObject.DecodeTo (content);
						content.Position = 0;

						var view = new AlternateView (content, GetContentType (part.ContentType));
						view.TransferEncoding = GetTransferEncoding (part.ContentTransferEncoding);
						if (!string.IsNullOrEmpty (part.ContentId))
							view.ContentId = part.ContentId;

						message.AlternateViews.Add (view);
					}
				} else {
					foreach (var part in multipart)
						AddBodyPart (message, part);
				}
			} else {
				var part = (MimePart) entity;

				if (part.IsAttachment || !string.IsNullOrEmpty (message.Body) || !(part is TextPart)) {
					// clone the content
					var content = new MemoryStream ();
					part.ContentObject.DecodeTo (content);
					content.Position = 0;

					var attachment = new Attachment (content, GetContentType (part.ContentType));

					if (part.ContentDisposition != null) {
						attachment.ContentDisposition.DispositionType = part.ContentDisposition.Disposition;
						foreach (var param in part.ContentDisposition.Parameters)
							attachment.ContentDisposition.Parameters.Add (param.Name, param.Value);
					}

					attachment.TransferEncoding = GetTransferEncoding (part.ContentTransferEncoding);

					if (!string.IsNullOrEmpty (part.ContentId))
						attachment.ContentId = part.ContentId;

					message.Attachments.Add (attachment);
				} else {
					message.IsBodyHtml = part.ContentType.Matches ("text", "html");
					message.Body = ((TextPart) part).Text;
				}
			}
		}

		/// <summary>
		/// Explicit cast to convert a <see cref="MimeMessage"/> to a
		/// <see cref="System.Net.Mail.MailMessage"/>.
		/// </summary>
		/// <remarks>
		/// Casting a <see cref="MimeMessage"/> to a <see cref="System.Net.Mail.MailMessage"/>
		/// makes it possible to use MimeKit with <see cref="System.Net.Mail.SmtpClient"/>.
		/// </remarks>
		/// <param name="message">The message.</param>
		/// <exception cref="System.ArgumentNullException">
		/// The <paramref name="message"/> is <c>null</c>.
		/// </exception>
		public static explicit operator MailMessage (MimeMessage message)
		{
			if (message == null)
				throw new ArgumentNullException ("message");

			var sender = message.Sender.Mailboxes.FirstOrDefault ();
			var from = message.From.Mailboxes.FirstOrDefault ();
			var msg = new MailMessage ();

			foreach (var header in message.Headers)
				msg.Headers.Add (header.Field, header.Value);

			if (sender != null)
				msg.Sender = (MailAddress) sender;

			if (from != null)
				msg.From = (MailAddress) from;

			foreach (var mailbox in message.ReplyTo.Mailboxes)
				msg.ReplyToList.Add ((MailAddress) mailbox);

			foreach (var mailbox in message.To.Mailboxes)
				msg.To.Add ((MailAddress) mailbox);

			foreach (var mailbox in message.Cc.Mailboxes)
				msg.CC.Add ((MailAddress) mailbox);

			foreach (var mailbox in message.Bcc.Mailboxes)
				msg.Bcc.Add ((MailAddress) mailbox);

			msg.Subject = message.Subject;

			if (message.Body != null)
				AddBodyPart (msg, message.Body);

			return msg;
		}

		#endregion
	}
}

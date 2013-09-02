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

namespace MimeKit {
	public sealed class MimeMessage
	{
		static readonly StringComparer icase = StringComparer.OrdinalIgnoreCase;
		static readonly string[] StandardAddressHeaders = new string[] {
			"Sender", "From", "Reply-To", "To", "Cc", "Bcc"
		};
		const string DateTimeFormat = "ddd, dd MMM yyyy HH:mm:ss K";

		Dictionary<string, InternetAddressList> addresses;
		IList<string> inreplyto, references;
		string messageId;
		Version version;
		DateTime date;

		internal MimeMessage (HeaderList headers)
		{
			addresses = new Dictionary<string, InternetAddressList> (icase);
			headers.Changed += HeadersChanged;
			Headers = headers;

			// initialize our address lists
			foreach (var name in StandardAddressHeaders) {
				var list = new InternetAddressList ();
				addresses.Add (name, list);
			}

			references = new List<string> ();
			inreplyto = new List<string> ();

			// parse all of our address headers
			foreach (var header in headers) {
				int length = header.RawValue.Length;
				List<InternetAddress> parsed;
				InternetAddressList list;
				int index = 0;

				if (addresses.TryGetValue (header.Field, out list)) {
					if (!InternetAddressList.TryParse (header.RawValue, ref index, length, false, false, out parsed))
						continue;

					list.AddRange (parsed);
				} else if (icase.Compare (header.Field, "References") == 0) {
					// while these aren't addresses per se, they are still a list of addr-spec tokens...
					if (!InternetAddressList.TryParse (header.RawValue, ref index, length, false, false, out parsed))
						continue;

					foreach (var addr in parsed.OfType<MailboxAddress> ())
						references.Add (addr.Address);
				} else if (icase.Compare (header.Field, "In-Reply-To") == 0) {
					// while these aren't addresses per se, they are still a list of addr-spec tokens...
					if (!InternetAddressList.TryParse (header.RawValue, ref index, length, false, false, out parsed))
						continue;

					foreach (var addr in parsed.OfType<MailboxAddress> ())
						inreplyto.Add (addr.Address);
				} else if (icase.Compare (header.Field, "Message-Id") == 0) {
					// while this isn't an addresses per se, it is still an addr-spec tokens...
					if (!InternetAddressList.TryParse (header.RawValue, ref index, length, false, false, out parsed))
						continue;

					messageId = parsed.OfType<MailboxAddress> ().Select (x => x.Address).FirstOrDefault ();
				} else if (icase.Compare (header.Field, "MIME-Version") == 0) {
					// FIXME: implement a parser for this...
				} else if (icase.Compare (header.Field, "Date") == 0) {
					// FIXME: implement a parser for this...
				}
			}

			// listen for changes to our address lists
			foreach (var name in StandardAddressHeaders)
				addresses[name].Changed += InternetAddressListChanged;
		}

		public MimeMessage () : this (new HeaderList ())
		{
		}

		public HeaderList Headers {
			get; private set;
		}

		public InternetAddressList Sender {
			get { return addresses["Sender"]; }
		}

		public InternetAddressList From {
			get { return addresses["From"]; }
		}

		public InternetAddressList ReplyTo {
			get { return addresses["Reply-To"]; }
		}

		public InternetAddressList To {
			get { return addresses["To"]; }
		}

		public InternetAddressList Cc {
			get { return addresses["Cc"]; }
		}

		public InternetAddressList Bcc {
			get { return addresses["Bcc"]; }
		}

		public string Subject {
			get { return Headers["Subject"]; }
			set {
				if (value == null)
					Headers.RemoveAll ("Subject");
				else
					Headers["Subject"] = value;
			}
		}

		public DateTime Date {
			get { return date; }
			set {
				if (date == value)
					return;

				date = value;

				Headers.Changed -= HeadersChanged;
				Headers["Date"] = date.ToString (DateTimeFormat);
				Headers.Changed += HeadersChanged;
			}
		}

		public string[] References {
			get { return references.ToArray (); }
			// FIXME: implement a setter
		}

		public string[] InReplyTo {
			get { return inreplyto.ToArray (); }
			// FIXME: implement a setter
		}

		public string MessageId {
			get { return messageId; }
			set {
				if (messageId == value)
					return;

				Headers.Changed -= HeadersChanged;

				messageId = value.Trim ();

				if (value != null)
					Headers["Message-Id"] = "<" + messageId + ">";
				else
					Headers.RemoveAll ("Message-Id");

				Headers.Changed += HeadersChanged;
			}
		}

		public Version MimeVersion {
			get { return version; }
			set {
				if (version.CompareTo (value) == 0)
					return;

				Headers.Changed -= HeadersChanged;
				Headers["MIME-Version"] = value.ToString ();
				Headers.Changed += HeadersChanged;

				version = value;
			}
		}

		public MimeEntity Body {
			get; set;
		}

		static string GenerateMessageId ()
		{
			// FIXME: implement me
			return null;
		}

		public void WriteTo (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			if (!Headers.Contains ("Date"))
				Date = DateTime.Now;

			if (messageId == null)
				MessageId = GenerateMessageId ();

			if (version == null && Body != null && Body.Headers.Count > 0)
				MimeVersion = new Version (1, 0);

			Headers.WriteTo (stream);

			if (Body == null) {
				stream.Write (MimeEntity.NewLine, 0, MimeEntity.NewLine.Length);
			} else {
				Body.WriteTo (stream);
			}
		}

		void SerializeAddressList (string field, InternetAddressList list)
		{
			var builder = new StringBuilder (" ");
			int lineLength = field.Length + 2;

			list.Encode (builder, ref lineLength, Encoding.UTF8);

			var raw = Encoding.ASCII.GetBytes (builder.ToString ());

			Headers.Changed -= HeadersChanged;
			Headers.Replace (new Header (field, raw));
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

		void HeadersChanged (object sender, HeaderListChangedEventArgs e)
		{
			List<InternetAddress> parsed;
			InternetAddressList list;
			int index, length;

			switch (e.Action) {
			case HeaderListChangedAction.Added:
				if (addresses.TryGetValue (e.Header.Field, out list)) {
					// parse the addresses in the new header and add them to our address list
					length = e.Header.RawValue.Length;
					index = 0;

					if (InternetAddressList.TryParse (e.Header.RawValue, ref index, length, false, false, out parsed)) {
						list.Changed -= InternetAddressListChanged;
						list.AddRange (parsed);
						list.Changed += InternetAddressListChanged;
					}
				}
				break;
			case HeaderListChangedAction.Changed:
			case HeaderListChangedAction.Removed:
				if (addresses.TryGetValue (e.Header.Field, out list)) {
					// clear the address list and reload
					list.Changed -= InternetAddressListChanged;
					list.Clear ();

					foreach (var header in Headers) {
						if (icase.Compare (e.Header.Field, header.Field) != 0)
							continue;

						length = e.Header.RawValue.Length;
						index = 0;

						if (!InternetAddressList.TryParse (e.Header.RawValue, ref index, length, false, false, out parsed))
							continue;

						list.AddRange (parsed);
					}

					list.Changed += InternetAddressListChanged;
				}
				break;
			case HeaderListChangedAction.Cleared:
				foreach (var kvp in addresses) {
					kvp.Value.Changed -= InternetAddressListChanged;
					kvp.Value.Clear ();
					kvp.Value.Changed += InternetAddressListChanged;
				}

				references.Clear ();
				inreplyto.Clear ();
				messageId = null;
				version = null;
				break;
			default:
				throw new ArgumentOutOfRangeException ();
			}
		}
	}
}

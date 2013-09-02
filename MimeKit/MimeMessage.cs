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
using System.Collections.Generic;

namespace MimeKit {
	public sealed class MimeMessage
	{
		static readonly StringComparer icase = StringComparer.OrdinalIgnoreCase;
		static readonly string[] StandardAddressHeaders = new string[] {
			"Sender", "From", "Reply-To", "To", "Cc", "Bcc"
		};

		Dictionary<string, InternetAddressList> addresses;

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

			// parse all of our address headers
			foreach (var header in headers) {
				InternetAddressList parsed, list;

				if (!addresses.TryGetValue (header.Field, out list))
					continue;

				if (!InternetAddressList.TryParse (header.RawValue, out parsed))
					continue;

				list.AddRange (parsed);
				parsed.Clear ();
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

		InternetAddressList GetAddressList (string field)
		{
			InternetAddressList parsed, all;

			all = new InternetAddressList ();

			foreach (var header in Headers) {
				if (icase.Compare (header.Field, field) != 0)
					continue;

				if (InternetAddressList.TryParse (header.RawValue, out parsed))
					all.AddRange (parsed);
			}

			return all;
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
					Headers.Remove ("Subject");
				else
					Headers["Subject"] = value;
			}
		}

		public DateTime Date {
			get; set;
		}

		public string References {
			get; set;
		}

		public string InReplyTo {
			get; set;
		}

		public string MessageId {
			get; set;
		}

		public Version MimeVersion {
			get; set;
		}

		public MimeEntity Body {
			get; set;
		}

		public void WriteTo (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

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
			InternetAddressList parsed, list;

			switch (e.Action) {
			case HeaderListChangedAction.Added:
				if (addresses.TryGetValue (e.Header.Field, out list)) {
					// parse the addresses in the new header and add them to our address list
					if (InternetAddressList.TryParse (e.Header.RawValue, out parsed)) {
						list.Changed -= InternetAddressListChanged;
						list.AddRange (parsed);
						parsed.Clear ();
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

						if (!InternetAddressList.TryParse (header.RawValue, out parsed))
							continue;

						list.AddRange (parsed);
						parsed.Clear ();
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
				break;
			default:
				throw new ArgumentOutOfRangeException ();
			}
		}
	}
}

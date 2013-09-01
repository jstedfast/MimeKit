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

namespace MimeKit {
	public sealed class MimeMessage
	{
		static readonly StringComparer icase = StringComparer.OrdinalIgnoreCase;
		InternetAddressList from, replyto, to, cc, bcc;

		internal MimeMessage (HeaderList headers)
		{
			Headers = headers;
		}

		public MimeMessage ()
		{
			Headers = new HeaderList ();
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

		public InternetAddressList From {
			get; set;
		}

		public InternetAddressList ReplyTo {
			get; set;
		}

		public InternetAddressList To {
			get; set;
		}

		public InternetAddressList Cc {
			get; set;
		}

		public InternetAddressList Bcc {
			get; set;
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

		public MimeEntity Body {
			get; set;
		}

		public void WriteTo (Stream stream)
		{
			// FIXME: implement me
		}
	}
}

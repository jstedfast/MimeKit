//
// MessagePartial.cs
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
using System.Collections.Generic;

namespace MimeKit {
	public class MessagePartial : MimePart
	{
		internal MessagePartial (ParserOptions options, ContentType type, IEnumerable<Header> headers, bool toplevel) : base (options, type, headers, toplevel)
		{
		}

		public MessagePartial (string id, int number, int total) : base ("message", "partial")
		{
			if (id == null)
				throw new ArgumentNullException ("id");

			if (number < 0)
				throw new ArgumentException ("number");

			if (total < number)
				throw new ArgumentException ("total");

			ContentType.Parameters.Add (new Parameter ("id", id));
			ContentType.Parameters.Add (new Parameter ("number", number.ToString ()));
			ContentType.Parameters.Add (new Parameter ("total", total.ToString ()));
		}

		public string Id {
			get { return ContentType.Parameters["id"]; }
		}

		public int? Number {
			get {
				var text = ContentType.Parameters["number"];
				int number;

				if (text == null || !int.TryParse (text, out number))
					return null;

				return number;
			}
		}

		public int? Total {
			get {
				var text = ContentType.Parameters["total"];
				int total;

				if (text == null || !int.TryParse (text, out total))
					return null;

				return total;
			}
		}
	}
}

//
// Group.cs
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
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace MimeKit {
	public class Group : InternetAddress
	{
		InternetAddressList members;

		public Group (string name, IEnumerable<InternetAddress> addresses) : base (name)
		{
			members = new InternetAddressList (addresses);
		}

		public Group (string name) : base (name)
		{
			members = new InternetAddressList ();
		}

		public InternetAddressList Members {
			get { return members; }
		}

		public override void Encode (StringBuilder sb, ref int lineLength, Encoding charset)
		{
			if (sb == null)
				throw new ArgumentNullException ("sb");

			if (lineLength < 0)
				throw new ArgumentOutOfRangeException ("lineLength");

			if (charset == null)
				throw new ArgumentNullException ("charset");

			if (!string.IsNullOrEmpty (Name)) {
				var encoded = Rfc2047.EncodePhrase (charset, Name);
				var str = Encoding.ASCII.GetString (encoded);

				if (lineLength + str.Length > Rfc2047.MaxLineLength) {
					if (str.Length > Rfc2047.MaxLineLength) {
						// we need to break up the name...
						sb.AppendFolded (str, ref lineLength);
					} else {
						// the name itself is short enough to fit on a single line,
						// but only if we write it on a line by itself
						if (lineLength > 1) {
							sb.LineWrap ();
							lineLength = 1;
						}

						lineLength += str.Length;
						sb.Append (str);
					}
				} else {
					// we can safely fit the name on this line...
					lineLength += str.Length;
					sb.Append (str);
				}
			}

			sb.Append (": ");
			lineLength += 2;

			foreach (var member in members)
				member.Encode (sb, ref lineLength, charset);
		}

		public override string ToString (Encoding charset, bool encode)
		{
			var sb = new StringBuilder ();

			if (charset == null)
				throw new ArgumentNullException ("charset");

			if (encode) {
				int lineLength = 0;

				Encode (sb, ref lineLength, charset);
			} else {
				sb.Append (Name);
				sb.Append (':');
				sb.Append (' ');

				for (int i = 0; i < members.Count; i++) {
					if (i > 0)
						sb.Append (", ");

					sb.Append (members[i]);
				}

				sb.Append (';');
			}

			return sb.ToString ();
		}

		public override string ToString ()
		{
			return ToString (Encoding.UTF8, false);
		}
	}
}

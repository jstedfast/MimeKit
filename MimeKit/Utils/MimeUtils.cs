//
// MimeUtils.cs
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
	public static class MimeUtils
	{
		public static IEnumerable<string> TryEnumerateReferences (byte[] text, int startIndex, int length)
		{
			int endIndex = startIndex + length;
			int index = startIndex;
			InternetAddress addr;

			if (text == null)
				throw new ArgumentNullException ("text");

			if (startIndex < 0 || startIndex > text.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (length < 0 || startIndex + length > text.Length)
				throw new ArgumentOutOfRangeException ("length");

			do {
				if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, false))
					break;

				if (index >= endIndex)
					break;

				if (!InternetAddress.TryParseMailbox (text, startIndex, ref index, startIndex + length, "", false, out addr))
					break;

				yield return "<" + ((MailboxAddress) addr).Address + ">";
			} while (index < endIndex);

			yield break;
		}

		public static bool TryParseVersion (byte[] text, int startIndex, int length, out Version version)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			if (startIndex < 0 || startIndex > text.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (length < 0 || startIndex + length > text.Length)
				throw new ArgumentOutOfRangeException ("length");

			List<int> values = new List<int> ();
			int endIndex = startIndex + length;
			int index = startIndex;
			int value;

			version = null;

			do {
				if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, false) || index >= endIndex)
					return false;

				if (!ParseUtils.TryParseInt32 (text, ref index, endIndex, out value))
					return false;

				values.Add (value);

				if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, false))
					return false;

				if (index >= endIndex)
					break;

				if (text[index++] != (byte) '.')
					return false;
			} while (index < endIndex);

			switch (values.Count) {
			case 4: version = new Version (values[0], values[1], values[2], values[3]); break;
			case 3: version = new Version (values[0], values[1], values[2]); break;
			case 2: version = new Version (values[0], values[1]); break;
			default: return false;
			}

			return true;
		}
	}
}

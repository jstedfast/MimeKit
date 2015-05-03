//
// HtmlUtils.cs
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
using System.Globalization;

namespace MimeKit.Text {
	static class HtmlUtils
	{
		static bool IsValidStartCharacter (char c)
		{
			return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_';
		}

		static bool IsValidNameCharacter (char c)
		{
			return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-' || c == '_';
		}

		public static bool IsValidAttributeName (string name)
		{
			if (string.IsNullOrEmpty (name))
				return false;

			if (!IsValidStartCharacter (name[0]))
				return false;

			for (int i = 0; i < name.Length; i++) {
				if (!IsValidNameCharacter (name[i]))
					return false;
			}

			return true;
		}

		public static bool IsValidTagName (string name)
		{
			if (string.IsNullOrEmpty (name))
				return false;

			if (!IsValidStartCharacter (name[0]))
				return false;

			for (int i = 0; i < name.Length; i++) {
				if (!IsValidNameCharacter (name[i]))
					return false;
			}

			return true;
		}

		static int IndexOfHtmlEncodeAttributeChar (char[] value, int startIndex, int endIndex)
		{
			for (int i = startIndex; i < endIndex; i++) {
				switch (value[i]) {
				case '\'': case '"': case '&': case '<': case '>':
					return i;
				}
			}

			return endIndex;
		}

		public static void HtmlEncodeAttribute (TextWriter writer, char[] value, int startIndex, int count)
		{
			int endIndex = startIndex + count;
			int index;

			index = IndexOfHtmlEncodeAttributeChar (value, startIndex, endIndex);

			if (index > startIndex)
				writer.Write (value, startIndex, index - startIndex);

			while (index < endIndex) {
				char c = value[index++];
				int nextIndex;

				switch (c) {
				case '\'': writer.Write ("&#39;"); break;
				case '"': writer.Write ("&quot;"); break;
				case '&': writer.Write ("&amp;"); break;
				case '<': writer.Write ("&lt;"); break;
				case '>': writer.Write ("&gt;"); break;
				default:
					writer.Write (c);
					break;
				}

				if (index >= endIndex)
					break;

				if ((nextIndex = IndexOfHtmlEncodeAttributeChar (value, index, endIndex)) > index) {
					writer.Write (value, index, nextIndex - index);
					index = nextIndex;
				}
			}
		}

		static int IndexOfHtmlEncodeChar (char[] value, int startIndex, int endIndex)
		{
			for (int i = startIndex; i < endIndex; i++) {
				switch (value[i]) {
				case '\'': case '"': case '&': case '<': case '>':
					return i;
				default:
					if (value[i] >= 160 && value[i] < 256)
						return i;

					if (char.IsSurrogate (value[i]))
						return i;

					break;
				}
			}

			return endIndex;
		}

		public static void HtmlEncode (TextWriter writer, char[] value, int startIndex, int count)
		{
			int endIndex = startIndex + count;
			int index;

			index = IndexOfHtmlEncodeChar (value, startIndex, endIndex);

			if (index > startIndex)
				writer.Write (value, startIndex, index - startIndex);

			while (index < endIndex) {
				char c = value[index++];
				int unichar, nextIndex;

				switch (c) {
				case '\'': writer.Write ("&#39;"); break;
				case '"': writer.Write ("&quot;"); break;
				case '&': writer.Write ("&amp;"); break;
				case '<': writer.Write ("&lt;"); break;
				case '>': writer.Write ("&gt;"); break;
				default:
					if (c >= 160 && c < 256) {
						unichar = c;
					} else if (char.IsSurrogate (c)) {
						if (index + 1 < value.Length && char.IsSurrogatePair (c, value[index])) {
							unichar = char.ConvertToUtf32 (c, value[index]);
							index++;
						} else {
							unichar = c;
						}
					} else {
						writer.Write (c);
						break;
					}

					writer.Write (string.Format (CultureInfo.InvariantCulture, "&#{0};", unichar));
					break;
				}

				if (index >= endIndex)
					break;

				if ((nextIndex = IndexOfHtmlEncodeChar (value, index, endIndex)) > index) {
					writer.Write (value, index, nextIndex - index);
					index = nextIndex;
				}
			}
		}
	}
}

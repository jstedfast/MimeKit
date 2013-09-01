//
// StringBuilderExtensions.cs
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

namespace MimeKit {
	static class StringBuilderExtensions
	{
		public static StringBuilder LineWrap (this StringBuilder text)
		{
			if (text.Length == 0)
				return text;

			if (text[text.Length - 1] == ' ') {
				text[text.Length - 1] = '\n';
				text.Append ('\t');
			} else {
				text.Append ("\n\t");
			}

			return text;
		}

		public static StringBuilder AppendFolded (this StringBuilder text, string value, ref int lineLength)
		{
			int wordIndex = 0;
			int lwspIndex;

			while (wordIndex < value.Length) {
				lwspIndex = wordIndex;

				if (value[wordIndex] == '"') {
					// quoted string; don't break these up...
					lwspIndex++;

					while (lwspIndex < value.Length && value[lwspIndex] != '"') {
						if (value[lwspIndex] == '\\') {
							lwspIndex++;

							if (lwspIndex < value.Length)
								lwspIndex++;
						} else {
							lwspIndex++;
						}
					}

				} else {
					// normal word
					while (lwspIndex < value.Length && !char.IsWhiteSpace (value[lwspIndex]))
						lwspIndex++;
				}

				int length = lwspIndex - wordIndex;
				if (lineLength > 1 && (lineLength + length) > Rfc2047.MaxLineLength) {
					text.LineWrap ();
					lineLength = 1;
				}

				text.Append (value, wordIndex, length);
				lineLength += length;

				wordIndex = lwspIndex;
				while (wordIndex < value.Length && char.IsWhiteSpace (value[wordIndex]))
					wordIndex++;

				if (wordIndex < value.Length && wordIndex > lwspIndex) {
					text.Append (' ');
					lineLength++;
				}
			}

			return text;
		}
	}
}


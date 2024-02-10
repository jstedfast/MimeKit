//
// StringBuilderExtensions.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2024 .NET Foundation and Contributors
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
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MimeKit.Utils {
	static class StringBuilderExtensions
	{
		public static StringBuilder LineWrap (this StringBuilder text, FormatOptions options)
		{
			if (text.Length == 0)
				return text;

			if (char.IsWhiteSpace (text[text.Length - 1])) {
				text.Insert (text.Length - 1, options.NewLine);
			} else {
				text.Append (options.NewLine);
				text.Append ('\t');
			}

			return text;
		}

		public static void AppendTokens (this StringBuilder text, FormatOptions options, ref int lineLength, List<string> tokens)
		{
			var spaces = string.Empty;

			foreach (var token in tokens) {
				if (string.IsNullOrWhiteSpace (token)) {
					spaces = token;
					continue;
				}

				if (lineLength + spaces.Length + token.Length > options.MaxLineLength) {
					text.Append (options.NewLine);
					spaces = string.Empty;
					text.Append ('\t');
					lineLength = 1;
				} else {
					lineLength += spaces.Length;
					text.Append (spaces);
					spaces = string.Empty;
				}

				lineLength += token.Length;
				text.Append (token);
			}
		}

		public static StringBuilder AppendFolded (this StringBuilder text, FormatOptions options, bool firstToken, string value, ref int lineLength)
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

					// consume the end-quote
					if (lwspIndex < value.Length)
						lwspIndex++;
				} else {
					// normal word
					while (lwspIndex < value.Length && !char.IsWhiteSpace (value[lwspIndex]))
						lwspIndex++;
				}

				int length = lwspIndex - wordIndex;
				if (!firstToken && lineLength > 1 && (lineLength + length) > options.MaxLineLength) {
					text.LineWrap (options);
					lineLength = 1;
				}

				text.Append (value, wordIndex, length);
				lineLength += length;
				firstToken = false;

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

#if NETSTANDARD2_0 || NETFRAMEWORK
		public static void Append (this StringBuilder text, ReadOnlySpan<char> value)
		{
			char[] buffer = System.Buffers.ArrayPool<char>.Shared.Rent (value.Length);

			try {
				value.CopyTo (new Span<char> (buffer));
				text.Append (buffer, 0, value.Length);
			} finally {
				System.Buffers.ArrayPool<char>.Shared.Return (buffer);
			}
		}

		public static void Append (this StringBuilder sb, StringBuilder value)
		{
			sb.Append (value.ToString ());
		}
#endif

#if DEBUG
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static void AppendCStringByte (this StringBuilder text, byte c)
		{
			switch (c) {
			case 0x00: text.Append ("\\0"); break;
			case 0x07: text.Append ("\\a"); break;
			case 0x08: text.Append ("\\b"); break;
			case 0x09: text.Append ("\\t"); break;
			case 0x0A: text.Append ("\\n"); break;
			case 0x0B: text.Append ("\\v"); break;
			case 0x0D: text.Append ("\\r"); break;
			default:
				if (c < 0x20 || c > 0x7e) {
					text.AppendFormat ("\\x{0:x2}", c);
				} else {
					text.Append ((char) c);
				}
				break;
			}
		}

		public static void AppendCString (this StringBuilder text, byte[] cstr, int startIndex, int length)
		{
			for (int i = startIndex; i < startIndex + length; i++)
				text.AppendCStringByte (cstr[i]);
		}
#endif
	}
}


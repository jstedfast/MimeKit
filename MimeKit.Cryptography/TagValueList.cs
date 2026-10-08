//
// TagValueList.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A parser for the tag=value lists used by DKIM key records (RFC 6376, Section 3.2) and DMARC policy
	/// records (RFC 9989).
	/// </summary>
	static class TagValueList
	{
		// Note: RFC 6376 limits a single TXT record to far less than this, but a resolver may concatenate
		// multiple character-strings. Cap the number of tags so a hostile record cannot make us allocate
		// an unbounded list.
		internal const int MaxTags = 64;

		static bool IsWhiteSpace (char c)
		{
			return c == ' ' || c == '\t' || c == '\r' || c == '\n';
		}

		static bool IsAlpha (char c)
		{
			return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
		}

		static bool IsTagNameChar (char c)
		{
			return IsAlpha (c) || (c >= '0' && c <= '9') || c == '_';
		}

		/// <summary>
		/// Try to parse a tag=value list.
		/// </summary>
		/// <remarks>
		/// <para>Parses a tag-list as defined by RFC 6376, Section 3.2. Whitespace surrounding tag names and
		/// values is removed. Empty tag-specs (e.g. <c>";;"</c>) are tolerated.</para>
		/// <para>Duplicate tags are not rejected here; it is up to the caller to decide how to handle them.</para>
		/// </remarks>
		/// <returns><see langword="true" /> if the tag-list was successfully parsed; otherwise, <see langword="false" />.</returns>
		/// <param name="text">The text to parse.</param>
		/// <param name="tags">The tags, in the order in which they appeared.</param>
		public static bool TryParse (string text, [NotNullWhen (true)] out List<KeyValuePair<string, string>>? tags)
		{
			var list = new List<KeyValuePair<string, string>> ();
			int index = 0;

			tags = null;

			while (index < text.Length) {
				while (index < text.Length && IsWhiteSpace (text[index]))
					index++;

				if (index == text.Length)
					break;

				if (text[index] == ';') {
					index++;
					continue;
				}

				if (!IsAlpha (text[index]))
					return false;

				int nameIndex = index++;

				while (index < text.Length && IsTagNameChar (text[index]))
					index++;

				var name = text.Substring (nameIndex, index - nameIndex);

				while (index < text.Length && IsWhiteSpace (text[index]))
					index++;

				if (index == text.Length || text[index] != '=')
					return false;

				// skip over the '='
				index++;

				while (index < text.Length && IsWhiteSpace (text[index]))
					index++;

				int valueIndex = index;

				while (index < text.Length && text[index] != ';')
					index++;

				int valueEnd = index;

				while (valueEnd > valueIndex && IsWhiteSpace (text[valueEnd - 1]))
					valueEnd--;

				if (list.Count == MaxTags)
					return false;

				list.Add (new KeyValuePair<string, string> (name, text.Substring (valueIndex, valueEnd - valueIndex)));

				// skip over the ';'
				if (index < text.Length)
					index++;
			}

			tags = list;

			return true;
		}

		/// <summary>
		/// Remove all whitespace from a tag value.
		/// </summary>
		/// <remarks>
		/// Removes all whitespace from a tag value (e.g. a base64-encoded value that has been folded).
		/// </remarks>
		/// <returns>The value with all whitespace removed.</returns>
		/// <param name="value">The tag value.</param>
		public static string RemoveWhiteSpace (string value)
		{
			int i;

			for (i = 0; i < value.Length; i++) {
				if (IsWhiteSpace (value[i]))
					break;
			}

			if (i == value.Length)
				return value;

			var buffer = new char[value.Length];
			int length = 0;

			for (i = 0; i < value.Length; i++) {
				if (!IsWhiteSpace (value[i]))
					buffer[length++] = value[i];
			}

			return new string (buffer, 0, length);
		}

		/// <summary>
		/// Split a colon-separated tag value into its trimmed, non-empty components.
		/// </summary>
		/// <remarks>
		/// Splits a colon-separated tag value (e.g. <c>"sha1 : sha256"</c>) into its trimmed, non-empty components.
		/// </remarks>
		/// <returns>The components.</returns>
		/// <param name="value">The tag value.</param>
		public static List<string> SplitColonList (string value)
		{
			var items = new List<string> ();
			int index = 0;

			while (index < value.Length) {
				while (index < value.Length && IsWhiteSpace (value[index]))
					index++;

				int startIndex = index;

				while (index < value.Length && value[index] != ':')
					index++;

				int endIndex = index;

				while (endIndex > startIndex && IsWhiteSpace (value[endIndex - 1]))
					endIndex--;

				if (endIndex > startIndex)
					items.Add (value.Substring (startIndex, endIndex - startIndex));

				// skip over the ':'
				index++;
			}

			return items;
		}
	}
}

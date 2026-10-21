//
// AddressParserInput.cs
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

using System;
using System.Buffers;
using System.Text;

using MimeKit.Utils;

namespace MimeKit {
	// Indices use native input units; diagnostics retain the public UTF-8 byte offsets.
	internal interface IAddressParserInput
	{
		char this[int index] { get; }
		int Length { get; }
		bool IsAtom (int index);
		bool IsPhraseAtom (int index);
		bool IsBlank (int index);
		bool IsWhitespace (int index);
		bool IsDomain (int index);
		int GetByteOffset (int index);
		char GetErrorCharacter (int index);
		string GetUtf8String (int startIndex, int length);
		string GetLatin1String (int startIndex, int length);
		string DecodePhrase (ParserOptions options, int startIndex, int length, out int codepage);
		string DecodeComment (ParserOptions options, int startIndex, int length);
	}

	internal readonly struct ByteArrayAddressInput : IAddressParserInput
	{
		readonly byte[] text;

		public ByteArrayAddressInput (byte[] text)
		{
			this.text = text;
		}

		public char this[int index] => (char) text[index];
		public int Length => text.Length;
		public bool IsAtom (int index) => text[index].IsAtom ();
		public bool IsPhraseAtom (int index) => text[index].IsPhraseAtom ();
		public bool IsBlank (int index) => text[index].IsBlank ();
		public bool IsWhitespace (int index) => text[index].IsWhitespace ();
		public bool IsDomain (int index) => text[index].IsDomain ();
		public int GetByteOffset (int index) => index;
		public char GetErrorCharacter (int index) => (char) text[index];
		public string GetUtf8String (int startIndex, int length) => CharsetUtils.UTF8.GetString (text, startIndex, length);
		public string GetLatin1String (int startIndex, int length) => CharsetUtils.Latin1.GetString (text, startIndex, length);

		public string DecodePhrase (ParserOptions options, int startIndex, int length, out int codepage)
		{
			var unquoted = MimeUtils.Unquote (text, startIndex, length, true);

			return Rfc2047.DecodePhrase (options, unquoted, 0, unquoted.Length, out codepage);
		}

		public string DecodeComment (ParserOptions options, int startIndex, int length)
		{
			if (text.AsSpan (startIndex, length).IndexOf ((byte) '\t') == -1)
				return Rfc2047.DecodePhrase (options, text, startIndex, length).Trim ();

			var buffer = new byte[length];

			for (int i = 0; i < length; i++) {
				byte c = text[startIndex + i];

				buffer[i] = c == (byte) '\t' ? (byte) ' ' : c;
			}

			return Rfc2047.DecodePhrase (options, buffer, 0, length).Trim ();
		}
	}

	internal readonly struct StringAddressInput : IAddressParserInput
	{
		readonly string text;

		public StringAddressInput (string text)
		{
			this.text = text;
		}

		public char this[int index] => text[index];
		public int Length => text.Length;
		public bool IsAtom (int index) => text[index] >= 128 || ((byte) text[index]).IsAtom ();
		public bool IsPhraseAtom (int index) => text[index] >= 128 || ((byte) text[index]).IsPhraseAtom ();
		public bool IsBlank (int index) => text[index] == ' ' || text[index] == '\t';
		public bool IsWhitespace (int index) => text[index] == ' ' || text[index] == '\t' || text[index] == '\r' || text[index] == '\n';
		public bool IsDomain (int index) => text[index] < 128 && ((byte) text[index]).IsDomain ();

		public unsafe int GetByteOffset (int index)
		{
			fixed (char* ptr = text)
				return Encoding.UTF8.GetByteCount (ptr, index);
		}

		public char GetErrorCharacter (int index)
		{
			char c = text[index];

			if (c < 128)
				return c;
			if (c < 2048)
				return (char) (0xc0 | (c >> 6));
			if (char.IsHighSurrogate (c) && index + 1 < text.Length && char.IsLowSurrogate (text[index + 1]))
				return (char) (0xf0 | (char.ConvertToUtf32 (c, text[index + 1]) >> 18));
			if (char.IsSurrogate (c))
				return (char) 0xef;

			return (char) (0xe0 | (c >> 12));
		}

		public string GetUtf8String (int startIndex, int length)
		{
			int endIndex = startIndex + length;

			for (int i = startIndex; i < endIndex; i++) {
				if (!char.IsSurrogate (text[i]))
					continue;

				if (char.IsHighSurrogate (text[i]) && i + 1 < endIndex && char.IsLowSurrogate (text[i + 1])) {
					i++;
					continue;
				}

				// Match the replacement performed by the former whole-input UTF-8 conversion.
				var bytes = GetBytes (startIndex, length);
				return Encoding.UTF8.GetString (bytes, 0, bytes.Length);
			}

			return text.Substring (startIndex, length);
		}

		public string GetLatin1String (int startIndex, int length) => GetUtf8String (startIndex, length);

		unsafe byte[] GetBytes (int startIndex, int length)
		{
			fixed (char* ptr = text) {
				var bytes = new byte[Encoding.UTF8.GetByteCount (ptr + startIndex, length)];

				if (bytes.Length > 0) {
					fixed (byte* output = bytes)
						Encoding.UTF8.GetBytes (ptr + startIndex, length, output, bytes.Length);
				}

				return bytes;
			}
		}

		bool IsPlainAscii (int startIndex, int length)
		{
			int endIndex = startIndex + length;

			for (int i = startIndex; i < endIndex; i++) {
				if (text[i] >= 128 || text[i] == '=')
					return false;
			}

			return true;
		}

		unsafe string DecodeName (ParserOptions options, int startIndex, int length, bool unquote, out int codepage)
		{
			fixed (char* ptr = text) {
				int count = Encoding.UTF8.GetByteCount (ptr + startIndex, length);
				var bytes = ArrayPool<byte>.Shared.Rent (count);

				try {
					fixed (byte* output = bytes)
						Encoding.UTF8.GetBytes (ptr + startIndex, length, output, bytes.Length);

					var input = new ByteArrayAddressInput (bytes);

					if (unquote)
						return input.DecodePhrase (options, 0, count, out codepage);

					codepage = 65001;
					return input.DecodeComment (options, 0, count);
				} finally {
					ArrayPool<byte>.Shared.Return (bytes);
				}
			}
		}

		public string DecodePhrase (ParserOptions options, int startIndex, int length, out int codepage)
		{
			if (IsPlainAscii (startIndex, length)) {
				codepage = 65001;

				var phrase = text.AsSpan (startIndex, length);
				bool unquote = false;

				for (int i = 0; i < phrase.Length; i++) {
					if (phrase[i] == '"' || phrase[i] == '\\' || phrase[i] == '\r' || phrase[i] == '\n' || phrase[i] == '\t') {
						unquote = true;
						break;
					}
				}

				if (!unquote)
					return text.Substring (startIndex, length);

				var builder = new ValueStringBuilder (length);
				MimeUtils.UnquoteTo (phrase, true, ref builder);
				return builder.ToString ();
			}

			// Keep charset guessing, encoded-word tokenization and surrogate replacement in
			// the existing decoder. Only the name token needs transcoding, not the input.
			return DecodeName (options, startIndex, length, true, out codepage);
		}

		public string DecodeComment (ParserOptions options, int startIndex, int length)
		{
			if (IsPlainAscii (startIndex, length))
				return text.Substring (startIndex, length).Replace ('\t', ' ').Trim ();

			return DecodeName (options, startIndex, length, false, out _);
		}
	}

	internal static class InternetAddressParser
	{
		internal static InternetAddressParser<ByteArrayAddressInput> Create (ParserOptions options, byte[] text, int startIndex, int endIndex)
		{
			return new InternetAddressParser<ByteArrayAddressInput> (options, new ByteArrayAddressInput (text), startIndex, endIndex);
		}

		internal static InternetAddressParser<StringAddressInput> Create (ParserOptions options, string text)
		{
			return new InternetAddressParser<StringAddressInput> (options, new StringAddressInput (text), 0, text.Length);
		}
	}
}

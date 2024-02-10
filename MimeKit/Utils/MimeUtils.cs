//
// MimeUtils.cs
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
using System.Net.NetworkInformation;
using System.Security.Cryptography;

namespace MimeKit.Utils {
	/// <summary>
	/// MIME utility methods.
	/// </summary>
	/// <remarks>
	/// Various utility methods that don't belong anywhere else.
	/// </remarks>
	public static class MimeUtils
	{
		const string base36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		static string DefaultHostName = null;
		static readonly char[] UnquoteChars = new[] { '\r', '\n', '\t', '\\', '"' };

		/// <summary>
		/// A string comparer that performs a case-insensitive ordinal string comparison.
		/// </summary>
		/// <remarks>
		/// A string comparer that performs a case-insensitive ordinal string comparison.
		/// </remarks>
		public static readonly IEqualityComparer<string> OrdinalIgnoreCase;

		static MimeUtils ()
		{
#if NETFRAMEWORK || NETSTANDARD2_0
			OrdinalIgnoreCase = new OptimizedOrdinalIgnoreCaseComparer ();
#else
			OrdinalIgnoreCase = StringComparer.OrdinalIgnoreCase;
#endif
		}

#if !NET6_0_OR_GREATER
		internal static void GetRandomBytes (byte[] buffer)
		{
#if NETSTANDARD2_1 || NET5_0_OR_GREATER
			RandomNumberGenerator.Fill (buffer);
#else
			using (var random = RandomNumberGenerator.Create ())
				random.GetBytes (buffer);
#endif
		}
#endif

		/// <summary>
		/// Generate a Message-Id or Content-Id.
		/// </summary>
		/// <remarks>
		/// Generates a new Message-Id (or Content-Id) using the supplied domain.
		/// </remarks>
		/// <returns>The message identifier.</returns>
		/// <param name="domain">A domain to use.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="domain"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="domain"/> is invalid.
		/// </exception>
		public static string GenerateMessageId (string domain)
		{
			if (domain is null)
				throw new ArgumentNullException (nameof (domain));

			if (domain.Length == 0)
				throw new ArgumentException ("The domain is invalid.", nameof (domain));

			ulong value = (ulong) DateTime.UtcNow.Ticks;
			var id = new ValueStringBuilder (64);

#if NET6_0_OR_GREATER
			Span<byte> block = stackalloc byte[8];

			RandomNumberGenerator.Fill (block);
#else
			var block = new byte[8];

			GetRandomBytes (block);
#endif

			do {
				id.Append (base36[(int) (value % 36)]);
				value /= 36;
			} while (value != 0);

			id.Append ('.');

			value = 0;
			for (int i = 0; i < 8; i++)
				value = (value << 8) | (ulong) block[i];

			do {
				id.Append (base36[(int) (value % 36)]);
				value /= 36;
			} while (value != 0);

			id.Append ('@');
			id.Append (MailboxAddress.IdnMapping.Encode (domain));

			return id.ToString ();
		}

		/// <summary>
		/// Generate a Message-Id or Content-Id.
		/// </summary>
		/// <remarks>
		/// Generates a new Message-Id (or Content-Id) using the local machine's domain.
		/// </remarks>
		/// <returns>The message identifier.</returns>
		public static string GenerateMessageId ()
		{
			if (DefaultHostName is null) {
				try {
					var properties = IPGlobalProperties.GetIPGlobalProperties ();

					DefaultHostName = properties.HostName.ToLowerInvariant ();
				} catch {
					DefaultHostName = "localhost.localdomain";
				}
			}

			return GenerateMessageId (DefaultHostName);
		}

		/// <summary>
		/// Enumerate the Message-Id references such as those that can be found in
		/// the In-Reply-To or References headers.
		/// </summary>
		/// <remarks>
		/// Incrementally parses Message-Ids (such as those from a References header
		/// in a MIME message) from the supplied buffer starting at the given index
		/// and spanning across the specified number of bytes.
		/// </remarks>
		/// <returns>The references.</returns>
		/// <param name="buffer">The raw byte buffer to parse.</param>
		/// <param name="startIndex">The index into the buffer to start parsing.</param>
		/// <param name="length">The length of the buffer to parse.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static IEnumerable<string> EnumerateReferences (byte[] buffer, int startIndex, int length)
		{
			ParseUtils.ValidateArguments (buffer, startIndex, length);

			int endIndex = startIndex + length;
			int index = startIndex;

			do {
				if (!ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, false))
					break;

				if (index >= endIndex)
					break;

				if (buffer[index] == '<') {
					if (ParseUtils.TryParseMsgId (buffer, ref index, endIndex, true, false, out string msgid))
						yield return msgid;
				} else if (!ParseUtils.SkipWord (buffer, ref index, endIndex, false)) {
					index++;
				}
			} while (index < endIndex);

			yield break;
		}

		/// <summary>
		/// Enumerate the Message-Id references such as those that can be found in
		/// the In-Reply-To or References headers.
		/// </summary>
		/// <remarks>
		/// Incrementally parses Message-Ids (such as those from a References header
		/// in a MIME message) from the specified text.
		/// </remarks>
		/// <returns>The references.</returns>
		/// <param name="text">The text to parse.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public static IEnumerable<string> EnumerateReferences (string text)
		{
			if (text is null)
				throw new ArgumentNullException (nameof (text));

			var buffer = Encoding.UTF8.GetBytes (text);

			return EnumerateReferences (buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Parse a Message-Id or Content-Id header value.
		/// </summary>
		/// <remarks>
		/// Parses the Message-Id (or Content-Id) value, returning the addr-spec portion of the msg-id token.
		/// </remarks>
		/// <returns>The addr-spec portion of the msg-id token.</returns>
		/// <param name="buffer">The raw byte buffer to parse.</param>
		/// <param name="startIndex">The index into the buffer to start parsing.</param>
		/// <param name="length">The length of the buffer to parse.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static string ParseMessageId (byte[] buffer, int startIndex, int length)
		{
			ParseUtils.ValidateArguments (buffer, startIndex, length);

			int endIndex = startIndex + length;
			int index = startIndex;

			ParseUtils.TryParseMsgId (buffer, ref index, endIndex, false, false, out string msgid);

			return msgid;
		}

		/// <summary>
		/// Parse a Message-Id or Content-Id header value.
		/// </summary>
		/// <remarks>
		/// Parses the Message-Id (or Content-Id) value, returning the addr-spec portion of the msg-id token.
		/// </remarks>
		/// <returns>The addr-spec portion of the msg-id token.</returns>
		/// <param name="text">The text to parse.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public static string ParseMessageId (string text)
		{
			if (text is null)
				throw new ArgumentNullException (nameof (text));

			var buffer = Encoding.UTF8.GetBytes (text);

			return ParseMessageId (buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Try to parse a version from a header such as Mime-Version.
		/// </summary>
		/// <remarks>
		/// Parses a MIME version string from the supplied buffer starting at the given index
		/// and spanning across the specified number of bytes.
		/// </remarks>
		/// <returns><c>true</c>, if the version was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The raw byte buffer to parse.</param>
		/// <param name="startIndex">The index into the buffer to start parsing.</param>
		/// <param name="length">The length of the buffer to parse.</param>
		/// <param name="version">The parsed version.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static bool TryParse (byte[] buffer, int startIndex, int length, out Version version)
		{
			ParseUtils.ValidateArguments (buffer, startIndex, length);

			var values = new List<int> ();
			int endIndex = startIndex + length;
			int index = startIndex;

			version = null;

			do {
				if (!ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, false) || index >= endIndex)
					return false;

				if (!ParseUtils.TryParseInt32 (buffer, ref index, endIndex, out int value))
					return false;

				values.Add (value);

				if (!ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, false))
					return false;

				if (index >= endIndex)
					break;

				if (buffer[index++] != (byte) '.')
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

		/// <summary>
		/// Try to parse a version from a header such as Mime-Version.
		/// </summary>
		/// <remarks>
		/// Parses a MIME version string from the specified text.
		/// </remarks>
		/// <returns><c>true</c>, if the version was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="text">The text to parse.</param>
		/// <param name="version">The parsed version.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public static bool TryParse (string text, out Version version)
		{
			if (text is null)
				throw new ArgumentNullException (nameof (text));

			var buffer = Encoding.UTF8.GetBytes (text);

			return TryParse (buffer, 0, buffer.Length, out version);
		}

		static bool IsEncoding (string value, string text, int startIndex, int length)
		{
			return length == value.Length && string.Compare (value, 0, text, startIndex, length, StringComparison.OrdinalIgnoreCase) == 0;
		}

		/// <summary>
		/// Try to parse the value of a Content-Transfer-Encoding header.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Transfer-Encoding header value.
		/// </remarks>
		/// <returns><c>true</c>, if the encoding was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="text">The text to parse.</param>
		/// <param name="encoding">The parsed encoding.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public static bool TryParse (string text, out ContentEncoding encoding)
		{
			if (text is null)
				throw new ArgumentNullException (nameof (text));

			int i = 0;

			// trim leading whitespace
			while (i < text.Length && char.IsWhiteSpace (text[i]))
				i++;

			int startIndex = i, n = 0;

			// Note: Google Docs tacks a ';' on the end... *sigh*
			// See https://github.com/jstedfast/MimeKit/issues/106 for an example.
			while (i < text.Length && text[i] != ';' && !char.IsWhiteSpace (text[i])) {
				i++;
				n++;
			}

			if (IsEncoding ("7bit", text, startIndex, n))
				encoding = ContentEncoding.SevenBit;
			else if (IsEncoding ("8bit", text, startIndex, n))
				encoding = ContentEncoding.EightBit;
			else if (IsEncoding ("binary", text, startIndex, n))
				encoding = ContentEncoding.Binary;
			else if (IsEncoding ("base64", text, startIndex, n))
				encoding = ContentEncoding.Base64;
			else if (IsEncoding ("quoted-printable", text, startIndex, n))
				encoding = ContentEncoding.QuotedPrintable;
			else if (IsEncoding ("x-uuencode", text, startIndex, n))
				encoding = ContentEncoding.UUEncode;
			else if (IsEncoding ("uuencode", text, startIndex, n))
				encoding = ContentEncoding.UUEncode;
			else if (IsEncoding ("x-uue", text, startIndex, n))
				encoding = ContentEncoding.UUEncode;
			else
				encoding = ContentEncoding.Default;

			return encoding != ContentEncoding.Default;
		}

		/// <summary>
		/// Quote the specified text and append it into the string builder.
		/// </summary>
		/// <remarks>
		/// Quotes the specified text, enclosing it in double-quotes and escaping
		/// any backslashes and double-quotes within.
		/// </remarks>
		/// <returns>The string builder.</returns>
		/// <param name="builder">The string builder.</param>
		/// <param name="text">The text to quote.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="builder"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public static StringBuilder AppendQuoted (StringBuilder builder, string text)
		{
			if (builder is null)
				throw new ArgumentNullException (nameof (builder));

			if (text is null)
				throw new ArgumentNullException (nameof (text));

			builder.Append ('"');
			for (int i = 0; i < text.Length; i++) {
				if (text[i] == '\\' || text[i] == '"')
					builder.Append ('\\');
				builder.Append (text[i]);
			}
			builder.Append ('"');

			return builder;
		}

		/// <summary>
		/// Quote the specified text and append it into the value string builder.
		/// </summary>
		/// <remarks>
		/// Quotes the specified text, enclosing it in double-quotes and escaping
		/// any backslashes and double-quotes within.
		/// </remarks>
		/// <param name="builder">The value string builder.</param>
		/// <param name="text">The text to quote.</param>
		internal static void AppendQuoted (ref ValueStringBuilder builder, ReadOnlySpan<char> text)
		{
			builder.Append ('"');
			foreach (char c in text) {
				if (c == '\\' || c == '"')
					builder.Append ('\\');
				builder.Append (c);
			}
			builder.Append ('"');
		}

		/// <summary>
		/// Quote the specified text and append it into the value string builder.
		/// </summary>
		/// <remarks>
		/// Quotes the specified text, enclosing it in double-quotes and escaping
		/// any backslashes and double-quotes within.
		/// </remarks>
		/// <param name="builder">The value string builder.</param>
		/// <param name="text">The text to quote.</param>
		internal static void AppendQuoted (this ref ValueStringBuilder builder, string text)
		{
			AppendQuoted (ref builder, text.AsSpan ());
		}

		/// <summary>
		/// Quote the specified text.
		/// </summary>
		/// <remarks>
		/// Quotes the specified text, enclosing it in double-quotes and escaping
		/// any backslashes and double-quotes within.
		/// </remarks>
		/// <returns>The quoted text.</returns>
		/// <param name="text">The text to quote.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public static string Quote (ReadOnlySpan<char> text)
		{
			var quoted = new ValueStringBuilder ((text.Length * 2) + 2);

			AppendQuoted (ref quoted, text);

			return quoted.ToString ();
		}

		/// <summary>
		/// Quote the specified text.
		/// </summary>
		/// <remarks>
		/// Quotes the specified text, enclosing it in double-quotes and escaping
		/// any backslashes and double-quotes within.
		/// </remarks>
		/// <returns>The quoted text.</returns>
		/// <param name="text">The text to quote.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public static string Quote (string text)
		{
			if (text is null)
				throw new ArgumentNullException (nameof (text));

			return Quote (text.AsSpan ());
		}

		/// <summary>
		/// Unquote the specified text.
		/// </summary>
		/// <remarks>
		/// Unquotes the specified text, removing any escaped backslashes within.
		/// </remarks>
		/// <returns>The unquoted text.</returns>
		/// <param name="text">The text to unquote.</param>
		/// <param name="convertTabsToSpaces"><c>true</c> if tab characters should be converted to a space; otherwise, <c>false</c>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public static string Unquote (string text, bool convertTabsToSpaces = false)
		{
			if (text is null)
				throw new ArgumentNullException (nameof (text));

			int index = text.IndexOfAny (UnquoteChars);

			if (index == -1)
				return text;

			var builder = new ValueStringBuilder (text.Length);
			bool escaped = false;
			bool quoted = false;

			for (int i = 0; i < text.Length; i++) {
				switch (text[i]) {
				case '\r':
				case '\n':
					escaped = false;
					break;
				case '\t':
					builder.Append (convertTabsToSpaces ? ' ' : '\t');
					escaped = false;
					break;
				case '\\':
					if (escaped)
						builder.Append ('\\');
					escaped = !escaped;
					break;
				case '"':
					if (escaped) {
						builder.Append ('"');
						escaped = false;
					} else {
						quoted = !quoted;
					}
					break;
				default:
					builder.Append (text[i]);
					escaped = false;
					break;
				}
			}

			return builder.ToString ();
		}

		internal static byte[] Unquote (byte[] text, int startIndex, int length, bool convertTabsToSpaces = false)
		{
			var builder = new ByteArrayBuilder (length);
			bool escaped = false;
			bool quoted = false;

			for (int i = startIndex; i < startIndex + length; i++) {
				switch ((char) text[i]) {
				case '\r':
				case '\n':
					escaped = false;
					break;
				case '\t':
					builder.Append ((byte) (convertTabsToSpaces ? ' ' : '\t'));
					escaped = false;
					break;
				case '\\':
					if (escaped)
						builder.Append ((byte) '\\');
					escaped = !escaped;
					break;
				case '"':
					if (escaped) {
						builder.Append ((byte) '"');
						escaped = false;
					} else {
						quoted = !quoted;
					}
					break;
				default:
					builder.Append (text[i]);
					escaped = false;
					break;
				}
			}

			return builder.ToArray ();
		}
	}
}

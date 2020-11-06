//
// MimeUtils.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 .NET Foundation and Contributors
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
#if !NETSTANDARD1_3 && !NETSTANDARD1_6
		static string DefaultHostName = null;
#endif

		/// <summary>
		/// A string comparer that performs a case-insensitive ordinal string comparison.
		/// </summary>
		/// <remarks>
		/// A string comparer that performs a case-insensitive ordinal string comparison.
		/// </remarks>
		public static readonly IEqualityComparer<string> OrdinalIgnoreCase = new OptimizedOrdinalIgnoreCaseComparer ();

		internal static void GetRandomBytes (byte[] buffer)
		{
			using (var random = RandomNumberGenerator.Create ())
				random.GetBytes (buffer);
		}

		/// <summary>
		/// Generates a Message-Id.
		/// </summary>
		/// <remarks>
		/// Generates a new Message-Id using the supplied domain.
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
			if (domain == null)
				throw new ArgumentNullException (nameof (domain));

			if (domain.Length == 0)
				throw new ArgumentException ("The domain is invalid.", nameof (domain));

			ulong value = (ulong) DateTime.Now.Ticks;
			var id = new StringBuilder ();
			var block = new byte[8];

			GetRandomBytes (block);

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

			id.Append ('@').Append (ParseUtils.IdnEncode (domain));

			return id.ToString ();
		}

		/// <summary>
		/// Generates a Message-Id.
		/// </summary>
		/// <remarks>
		/// Generates a new Message-Id using the local machine's domain.
		/// </remarks>
		/// <returns>The message identifier.</returns>
		public static string GenerateMessageId ()
		{
#if NETSTANDARD1_3 || NETSTANDARD1_6
			return GenerateMessageId ("localhost.localdomain");
#else
			if (DefaultHostName == null) {
				var properties = IPGlobalProperties.GetIPGlobalProperties ();

				DefaultHostName = properties.HostName;
			}

			return GenerateMessageId (DefaultHostName);
#endif
		}

		/// <summary>
		/// Enumerates the message-id references such as those that can be found in
		/// the In-Reply-To or References header.
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
		/// Enumerates the message-id references such as those that can be found in
		/// the In-Reply-To or References header.
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
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var buffer = Encoding.UTF8.GetBytes (text);

			return EnumerateReferences (buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Parses a Message-Id header value.
		/// </summary>
		/// <remarks>
		/// Parses the Message-Id value, returning the addr-spec portion of the msg-id token.
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
			string msgid;

			ParseUtils.TryParseMsgId (buffer, ref index, endIndex, false, false, out msgid);

			return msgid;
		}

		/// <summary>
		/// Parses a Message-Id header value.
		/// </summary>
		/// <remarks>
		/// Parses the Message-Id value, returning the addr-spec portion of the msg-id token.
		/// </remarks>
		/// <returns>The addr-spec portion of the msg-id token.</returns>
		/// <param name="text">The text to parse.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public static string ParseMessageId (string text)
		{
			if (text == null)
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
			int value;

			version = null;

			do {
				if (!ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, false) || index >= endIndex)
					return false;

				if (!ParseUtils.TryParseInt32 (buffer, ref index, endIndex, out value))
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
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var buffer = Encoding.UTF8.GetBytes (text);

			return TryParse (buffer, 0, buffer.Length, out version);
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
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var value = new char[text.Length];
			int i = 0, n = 0;
			string name;

			// trim leading whitespace
			while (i < text.Length && char.IsWhiteSpace (text[i]))
				i++;

			// copy the encoding name
			// Note: Google Docs tacks a ';' on the end... *sigh*
			// See https://github.com/jstedfast/MimeKit/issues/106 for an example.
			while (i < text.Length && text[i] != ';' && !char.IsWhiteSpace (text[i]))
				value[n++] = char.ToLowerInvariant (text[i++]);

			name = new string (value, 0, n);

			switch (name) {
			case "7bit":             encoding = ContentEncoding.SevenBit; break;
			case "8bit":             encoding = ContentEncoding.EightBit; break;
			case "binary":           encoding = ContentEncoding.Binary; break;
			case "base64":           encoding = ContentEncoding.Base64; break;
			case "quoted-printable": encoding = ContentEncoding.QuotedPrintable; break;
			case "x-uuencode":       encoding = ContentEncoding.UUEncode; break;
			case "uuencode":         encoding = ContentEncoding.UUEncode; break;
			case "x-uue":            encoding = ContentEncoding.UUEncode; break;
			default:                 encoding = ContentEncoding.Default; break;
			}

			return encoding != ContentEncoding.Default;
		}

		/// <summary>
		/// Quotes the specified text.
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
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var quoted = new StringBuilder (text.Length + 2, (text.Length * 2) + 2);

			quoted.Append ("\"");
			for (int i = 0; i < text.Length; i++) {
				if (text[i] == '\\' || text[i] == '"')
					quoted.Append ('\\');
				quoted.Append (text[i]);
			}
			quoted.Append ("\"");

			return quoted.ToString ();
		}

		/// <summary>
		/// Unquotes the specified text.
		/// </summary>
		/// <remarks>
		/// Unquotes the specified text, removing any escaped backslashes within.
		/// </remarks>
		/// <returns>The unquoted text.</returns>
		/// <param name="text">The text to unquote.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public static string Unquote (string text)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			int index = text.IndexOfAny (new [] { '\r', '\n', '\t', '\\', '"' });

			if (index == -1)
				return text;

			var builder = new StringBuilder (text.Length);
			bool escaped = false;
			bool quoted = false;

			for (int i = 0; i < text.Length; i++) {
				switch (text[i]) {
				case '\r':
				case '\n':
					escaped = false;
					break;
				case '\t':
					builder.Append (' ');
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
	}
}

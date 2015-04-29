//
// MimeUtils.cs
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
using System.Net;
using System.Text;
using System.Collections.Generic;

#if !PORTABLE
using System.Security.Cryptography;
#endif

namespace MimeKit.Utils {
	/// <summary>
	/// MIME utility methods.
	/// </summary>
	/// <remarks>
	/// Various utility methods that don't belong anywhere else.
	/// </remarks>
	public static class MimeUtils
	{
#if PORTABLE
		static readonly Random random = new Random ((int) DateTime.Now.Ticks);
#endif
		const string base36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		internal static void GetRandomBytes (byte[] buffer)
		{
#if NET_3_5
			var random = new RNGCryptoServiceProvider ();
			random.GetBytes (buffer);
#elif !PORTABLE
			using (var random = new RNGCryptoServiceProvider ())
				random.GetBytes (buffer);
#else
			lock (random) {
				random.NextBytes (buffer);
			}
#endif
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
		public static string GenerateMessageId (string domain)
		{
			if (domain == null)
				throw new ArgumentNullException ("domain");

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

			id.Append ('@').Append (domain);

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
#if PORTABLE
			return GenerateMessageId ("localhost.localdomain");
#else
			return GenerateMessageId (Dns.GetHostName ());
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
			byte[] sentinels = { (byte) '>' };
			int endIndex = startIndex + length;
			int index = startIndex;
			string msgid;

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (length < 0 || length > (buffer.Length - startIndex))
				throw new ArgumentOutOfRangeException ("length");

			do {
				if (!ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, false))
					break;

				if (index >= endIndex)
					break;

				if (buffer[index] == '<') {
					// skip over the '<'
					index++;

					if (index >= endIndex)
						break;

					string localpart;
					if (!InternetAddress.TryParseLocalPart (buffer, ref index, endIndex, false, out localpart))
						continue;

					if (index >= endIndex)
						break;

					if (buffer[index] == (byte) '>') {
						// The msgid token did not contain an @domain. Technically this is illegal, but for the
						// sake of maximum compatibility, I guess we have no choice but to accept it...
						index++;

						yield return localpart;
						continue;
					}

					if (buffer[index] != (byte) '@') {
						// who the hell knows what we have here... ignore it and continue on?
						continue;
					}

					// skip over the '@'
					index++;

					if (!ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, false))
						break;

					if (index >= endIndex)
						break;

					if (buffer[index] == (byte) '>') {
						// The msgid token was in the form "<local-part@>". Technically this is illegal, but for
						// the sake of maximum compatibility, I guess we have no choice but to accept it...
						// https://github.com/jstedfast/MimeKit/issues/102
						index++;

						yield return localpart + "@";
						continue;
					}

					string domain;
					if (!ParseUtils.TryParseDomain (buffer, ref index, endIndex, sentinels, false, out domain))
						continue;

					msgid = localpart + "@" + domain;

					// Note: some Message-Id's are broken and in the form "<local-part@domain@domain>"
					// https://github.com/jstedfast/MailKit/issues/138
					while (index < endIndex && buffer[index] == (byte) '@') {
						int saved = index;

						index++;

						if (!ParseUtils.TryParseDomain (buffer, ref index, endIndex, sentinels, false, out domain)) {
							index = saved;
							break;
						}

						msgid += "@" + domain;
					}

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
				throw new ArgumentNullException ("text");

			var buffer = Encoding.UTF8.GetBytes (text);

			return EnumerateReferences (buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Tries to parse a version from a header such as Mime-Version.
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
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (length < 0 || length > (buffer.Length - startIndex))
				throw new ArgumentOutOfRangeException ("length");

			List<int> values = new List<int> ();
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
		/// Tries to parse a version from a header such as Mime-Version.
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
				throw new ArgumentNullException ("text");

			var buffer = Encoding.UTF8.GetBytes (text);

			return TryParse (buffer, 0, buffer.Length, out version);
		}

		/// <summary>
		/// Tries to parse a version from a header such as Mime-Version.
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
		[Obsolete ("Use TryParse (byte[] buffer, int startIndex, int length, out Version version) instead.")]
		public static bool TryParseVersion (byte[] buffer, int startIndex, int length, out Version version)
		{
			return TryParse (buffer, startIndex, length, out version);
		}

		/// <summary>
		/// Tries to parse a version from a header such as Mime-Version.
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
		[Obsolete ("Use TryParse (string text, out Version version) instead.")]
		public static bool TryParseVersion (string text, out Version version)
		{
			return TryParse (text, out version);
		}

		/// <summary>
		/// Tries to parse the value of a Content-Transfer-Encoding header.
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
				throw new ArgumentNullException ("text");

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
				throw new ArgumentNullException ("text");

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
				throw new ArgumentNullException ("text");

			int index = text.IndexOfAny (new [] { '\r', '\n', '\t', '\\', '"' });

			if (index == -1)
				return text;

			var builder = new StringBuilder ();
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

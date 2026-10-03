//
// CalendarWriter.cs
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
using System.IO;
using System.Text;
using System.Globalization;

namespace MimeKit.Tnef {
	/// <summary>
	/// Writes iCalendar content lines as specified by RFC 5545.
	/// </summary>
	/// <remarks>
	/// <para>Each content line is built up with <see cref="BeginProperty"/>, any number of calls to
	/// <see cref="WriteParameter"/> and finally <see cref="WriteValue"/> or <see cref="WriteTextValue"/>, which
	/// terminates the line.</para>
	/// <para>Lines are written in UTF-8, terminated by CRLF and folded so that no physical line is longer than 75
	/// octets (RFC 5545 3.1). A line is never folded in the middle of a UTF-8 sequence.</para>
	/// </remarks>
	sealed class CalendarWriter
	{
		const int MaxLineLength = 75;

		static readonly byte[] FoldSequence = { (byte) '\r', (byte) '\n', (byte) ' ' };
		static readonly byte[] NewLine = { (byte) '\r', (byte) '\n' };

		readonly StringBuilder line = new StringBuilder ();
		readonly Stream stream;
		byte[] buffer = new byte[256];
		bool inProperty;

		public CalendarWriter (Stream stream)
		{
			this.stream = stream;
		}

		public void BeginComponent (string name)
		{
			WriteProperty ("BEGIN", name);
		}

		public void EndComponent (string name)
		{
			WriteProperty ("END", name);
		}

		public void BeginProperty (string name)
		{
			if (inProperty)
				throw new InvalidOperationException ("The previous property has not been terminated.");

			line.Clear ();
			line.Append (name);
			inProperty = true;
		}

		public void WriteParameter (string name, string value)
		{
			if (!inProperty)
				throw new InvalidOperationException ("No property has been started.");

			line.Append (';').Append (name).Append ('=');
			AppendParameterValue (line, value);
		}

		/// <summary>
		/// Terminate the current content line with a value that is written as-is.
		/// </summary>
		/// <remarks>
		/// Used for values that are already in their iCalendar form, such as DATE-TIME, INTEGER, RECUR and URI values.
		/// </remarks>
		public void WriteValue (string value)
		{
			if (!inProperty)
				throw new InvalidOperationException ("No property has been started.");

			line.Append (':');

			foreach (var c in value) {
				if (!IsControl (c))
					line.Append (c);
			}

			Flush ();
		}

		/// <summary>
		/// Terminate the current content line with a TEXT value (RFC 5545 3.3.11), escaping it as needed.
		/// </summary>
		public void WriteTextValue (string text)
		{
			if (!inProperty)
				throw new InvalidOperationException ("No property has been started.");

			line.Append (':');
			AppendText (line, text);
			Flush ();
		}

		public void WriteProperty (string name, string value)
		{
			BeginProperty (name);
			WriteValue (value);
		}

		public void WriteTextProperty (string name, string text)
		{
			BeginProperty (name);
			WriteTextValue (text);
		}

		static bool IsControl (char c)
		{
			// RFC 5545 3.1: CONTROL is %x00-08 / %x0A-1F / %x7F, which is not allowed in any value.
			return (c < 0x20 && c != '\t') || c == 0x7F;
		}

		// RFC 5545 3.3.11: backslash, semicolon, comma and line breaks are escaped. Other control characters cannot be
		// represented and are dropped.
		internal static void AppendText (StringBuilder builder, string text)
		{
			for (int i = 0; i < text.Length; i++) {
				char c = text[i];

				switch (c) {
				case '\\': builder.Append ("\\\\"); break;
				case ';': builder.Append ("\\;"); break;
				case ',': builder.Append ("\\,"); break;
				case '\r':
					if (i + 1 < text.Length && text[i + 1] == '\n')
						i++;
					builder.Append ("\\n");
					break;
				case '\n': builder.Append ("\\n"); break;
				default:
					if (!IsControl (c))
						builder.Append (c);
					break;
				}
			}
		}

		internal static string EscapeText (string text)
		{
			var builder = new StringBuilder (text.Length);

			AppendText (builder, text);

			return builder.ToString ();
		}

		// RFC 5545 3.2: a param-value is either paramtext (no DQUOTE, ';', ':', ',' or CONTROL) or a quoted-string
		// (no DQUOTE or CONTROL). A double quote cannot be represented at all without the RFC 6868 extension, which
		// many readers do not implement, so it is replaced by a single quote.
		static void AppendParameterValue (StringBuilder builder, string value)
		{
			bool quote = false;

			foreach (var c in value) {
				if (c == ';' || c == ':' || c == ',') {
					quote = true;
					break;
				}
			}

			if (quote)
				builder.Append ('"');

			foreach (var c in value) {
				if (c == '"')
					builder.Append ('\'');
				else if (!IsControl (c))
					builder.Append (c);
			}

			if (quote)
				builder.Append ('"');
		}

		void Flush ()
		{
			var text = line.ToString ();
			int count = Encoding.UTF8.GetMaxByteCount (text.Length);

			if (buffer.Length < count)
				buffer = new byte[Math.Max (count, buffer.Length * 2)];

			int length = Encoding.UTF8.GetBytes (text, 0, text.Length, buffer, 0);
			int limit = MaxLineLength;
			int index = 0;

			// RFC 5545 3.1: lines longer than 75 octets are folded by inserting CRLF followed by a single space. The
			// space counts towards the length of the continuation line.
			while (length - index > limit) {
				int end = index + limit;

				// Never split a UTF-8 sequence: back up to the start of the sequence that straddles the limit.
				while (end > index + 1 && (buffer[end] & 0xC0) == 0x80)
					end--;

				stream.Write (buffer, index, end - index);
				stream.Write (FoldSequence, 0, FoldSequence.Length);
				limit = MaxLineLength - 1;
				index = end;
			}

			stream.Write (buffer, index, length - index);
			stream.Write (NewLine, 0, NewLine.Length);
			inProperty = false;
			line.Clear ();
		}

		/// <summary>
		/// Format a DATE-TIME value (RFC 5545 3.3.5).
		/// </summary>
		/// <remarks>
		/// UTC values get the "Z" suffix. Other values are written as local time, which needs a TZID parameter.
		/// </remarks>
		public static string FormatDateTime (DateTime value, bool utc)
		{
			return value.ToString (utc ? "yyyyMMdd'T'HHmmss'Z'" : "yyyyMMdd'T'HHmmss", CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Format a DATE value (RFC 5545 3.3.4).
		/// </summary>
		public static string FormatDate (DateTime value)
		{
			return value.ToString ("yyyyMMdd", CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Format a UTC-OFFSET value (RFC 5545 3.3.14) from an offset in minutes.
		/// </summary>
		public static string FormatUtcOffset (int minutes)
		{
			char sign = minutes < 0 ? '-' : '+';

			minutes = Math.Abs (minutes);

			return string.Format (CultureInfo.InvariantCulture, "{0}{1:D2}{2:D2}", sign, minutes / 60, minutes % 60);
		}
	}
}

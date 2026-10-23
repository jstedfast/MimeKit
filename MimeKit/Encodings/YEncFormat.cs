//
// YEncFormat.cs
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
using System.Globalization;
using System.Collections.Generic;
using System.Threading;

using MimeKit.IO;

namespace MimeKit.Encodings {
	static class YEncFormat
	{
		internal static bool StartsWith (byte[] line, int length, ReadOnlySpan<byte> prefix)
		{
			if (length < prefix.Length)
				return false;

			for (int i = 0; i < prefix.Length; i++) {
				if (line[i] != prefix[i])
					return false;
			}

			return true;
		}

		internal static Dictionary<string, string> ParseFields (string text, int start, bool isHeader)
		{
			var fields = new Dictionary<string, string> (StringComparer.Ordinal);

			while (start < text.Length) {
				if (text[start] == ' ') {
					start++;
					continue;
				}

				int end = text.IndexOf (' ', start);
				if (end == -1)
					end = text.Length;

				int equals = text.IndexOf ('=', start, end - start);

				if (equals <= start)
					throw new FormatException ("Invalid yEnc control field.");

				var key = text.Substring (start, equals - start);
				bool fileName = isHeader && key == "name";
				var value = fileName ? text.Substring (equals + 1).Trim (' ') : text.Substring (equals + 1, end - equals - 1);

				if (value.Length == 0 || fields.ContainsKey (key))
					throw new FormatException ("Empty or duplicate yEnc control field.");

				fields.Add (key, value);

				if (fileName)
					break;

				start = end;
			}

			return fields;
		}

		internal static long Number (Dictionary<string, string> fields, string name, bool required = true)
		{
			if (!fields.TryGetValue (name, out var value)) {
				if (required)
					throw new FormatException ($"Missing yEnc {name} field.");

				return 0;
			}

			if (!long.TryParse (value, NumberStyles.None, CultureInfo.InvariantCulture, out long number))
				throw new FormatException ($"Invalid yEnc {name} field.");

			return number;
		}

		internal static int Integer (Dictionary<string, string> fields, string name, bool required = true)
		{
			long number = Number (fields, name, required);

			if (number > int.MaxValue)
				throw new FormatException ($"Invalid yEnc {name} field.");

			return (int) number;
		}

		internal static uint? Checksum (Dictionary<string, string> fields, string name)
		{
			if (!fields.TryGetValue (name, out var value))
				return null;

			if (value.Length != 8 || !uint.TryParse (value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out uint crc))
				throw new FormatException ($"Invalid yEnc {name} field.");

			return crc;
		}

		internal static void ValidateStream (Stream stream, bool writable, string name)
		{
			if (stream is null)
				throw new ArgumentNullException (name);

			if (writable ? !stream.CanWrite : !stream.CanRead)
				throw new ArgumentException (writable ? "The stream must be writable." : "The stream must be readable.", name);
		}

		internal static int Read (Stream stream, byte[] buffer, int count, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested ();

			if (stream is ICancellableStream cancellable)
				return cancellable.Read (buffer, 0, count, cancellationToken);

			return stream.Read (buffer, 0, count);
		}

		internal static void Write (Stream stream, byte[] buffer, int count, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested ();

			if (stream is ICancellableStream cancellable)
				cancellable.Write (buffer, 0, count, cancellationToken);
			else
				stream.Write (buffer, 0, count);
		}
	}
}

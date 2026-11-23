//
// AutocryptValue.cs
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
using System.Text;
using System.Diagnostics.CodeAnalysis;

using MimeKit.Utils;

namespace MimeKit.Cryptography {
	sealed class AutocryptValue
	{
		const int MaxSize = 10 * 1024;

		internal readonly AutocryptPreference Preference;
		internal readonly string Address;
		internal readonly byte[] KeyData;

		AutocryptValue (string address, byte[] keyData, AutocryptPreference preference)
		{
			Address = address;
			KeyData = keyData;
			Preference = preference;
		}

		internal static bool TryCanonicalize (string address, [NotNullWhen (true)] out string? canonical)
		{
			canonical = null;

			if (address.Length == 0 || address.Length > MaxSize)
				return false;

			foreach (char c in address) {
				if (char.IsControl (c))
					return false;
			}

			try {
				var bytes = CharsetUtils.UTF8.GetBytes (address);
				int index = 0;

				if (!InternetAddress.TryParseAddrspec (bytes, ref index, bytes.Length, default, RfcComplianceMode.Strict, false, out var parsed, out int at) ||
				    index != bytes.Length || at <= 0 || parsed != address)
					return false;

				// Autocrypt compares local parts case-insensitively and uses ASCII labels for internationalized domains.
				canonical = MailboxAddress.EncodeAddrspec (parsed).ToLowerInvariant ();

				return true;
			} catch (ArgumentException) {
				// Invalid UTF-16 or internationalized domain name.
				return false;
			}
		}

		internal static AutocryptValue Create (string address, byte[] keyData, AutocryptPreference preference)
		{
			if (address is null)
				throw new ArgumentNullException (nameof (address));

			if (!TryCanonicalize (address, out var canonical))
				throw new ArgumentException ("Expected a single addr-spec.", nameof (address));

			if (keyData is null)
				throw new ArgumentNullException (nameof (keyData));

			// Three key bytes need four Base64 characters. ToHeader checks the tighter limit once folding is known.
			if (keyData.Length == 0 || keyData.Length > MaxSize * 3 / 4)
				throw new ArgumentOutOfRangeException (nameof (keyData));

			if (preference != AutocryptPreference.NoPreference && preference != AutocryptPreference.Mutual)
				throw new ArgumentOutOfRangeException (nameof (preference));

			return new AutocryptValue (canonical, (byte[]) keyData.Clone (), preference);
		}

		static bool IsValidFolding (byte[] bytes, int start, int end)
		{
			for (int i = start; i < end; i++) {
				byte b = bytes[i];

				if (b == '\r') {
					if (++i == end || bytes[i] != '\n')
						return false;
				} else if (b != '\n') {
					if (b < 32 && !b.IsBlank () || b == 127)
						return false;

					continue;
				}

				// Header.RawValue may include a final newline. Any earlier newline must introduce a continuation line.
				if (i + 1 < end && !bytes[i + 1].IsBlank ())
					return false;
			}

			return true;
		}

		internal static bool TryParse (HeaderId id, string? text, [NotNullWhen (true)] out AutocryptValue? value)
		{
			value = null;

			if (text is null || text.Length > MaxSize)
				return false;

			try {
				var bytes = CharsetUtils.UTF8.GetBytes (text);

				return TryParse (id, bytes, 0, bytes.Length, out value);
			} catch (EncoderFallbackException) {
				return false;
			}
		}

		// Read one name=value pair at a time, e.g. addr="a;b"@example.org; prefer-encrypt=mutual; keydata=AQID
		// The value excludes surrounding whitespace but retains quotes, escapes and internal folding.
		static bool TryReadAttribute (byte[] bytes, ref int index, int end, out ReadOnlySpan<byte> name, out int valueStart, out int valueLength)
		{
			valueStart = valueLength = 0;

			// skip to the beginning of the attribute name
			while (index < end && bytes[index].IsWhitespace ())
				index++;

			int start = index;

			// scan over the name
			while (index < end && bytes[index].IsToken () && bytes[index] != '=')
				index++;

			name = bytes.AsSpan (start, index - start);

			if (name.IsEmpty)
				return false;

			// skip over any whitespace after the name
			while (index < end && bytes[index].IsWhitespace ())
				index++;

			if (index == end || bytes[index++] != '=')
				return false;

			// skip whitespace after the '='
			while (index < end && bytes[index].IsWhitespace ())
				index++;

			valueStart = index;
			bool quoted = false;

			// A semicolon inside a quoted local part belongs to the address, not the attribute list.
			while (index < end && (quoted || bytes[index] != ';')) {
				byte b = bytes[index++];

				if (quoted && b == '\\' && index < end)
					index++;
				else if (b == '"')
					quoted = !quoted;
			}

			if (quoted)
				return false;

			// trim trailing whitespace from the value
			int last = index;
			while (last > valueStart && bytes[last - 1].IsWhitespace ())
				last--;

			valueLength = last - valueStart;

			// Consume the separator for the next call; a trailing semicolon would introduce an empty attribute.
			if (index < end && ++index == end)
				return false;

			return true;
		}

		internal static bool TryParse (HeaderId id, byte[]? bytes, int start, int length, [NotNullWhen (true)] out AutocryptValue? value)
		{
			value = null;

			// The 10 KiB limit includes the field name and colon, which are not part of Header.RawValue.
			if (!ArgumentValidator.TryValidate (bytes, start, length) || length > MaxSize - id.ToHeaderName ().Length - 1)
				return false;

			int index = start, end = start + length;

			if (!IsValidFolding (bytes, start, end))
				return false;

			AutocryptPreference? preference = null;
			string? address = null;
			byte[]? key = null;

			while (index < end) {
				if (!TryReadAttribute (bytes, ref index, end, out var attr, out int valueStart, out int valueLength))
					return false;

				if (attr.Equals ("addr"u8, StringComparison.OrdinalIgnoreCase)) {
					if (address != null)
						return false;

					try {
						if (!TryCanonicalize (CharsetUtils.UTF8.GetString (bytes, valueStart, valueLength), out address))
							return false;
					} catch (DecoderFallbackException) {
						return false;
					}
				} else if (attr.Equals ("keydata"u8, StringComparison.OrdinalIgnoreCase)) {
					if (index != end || valueLength == 0)
						return false;

					try {
						// FromBase64String ignores folding whitespace, even in the middle of a Base64 quartet.
						key = Convert.FromBase64String (Encoding.ASCII.GetString (bytes, valueStart, valueLength));
					} catch (FormatException) {
						return false;
					}
				} else if (attr.Equals ("prefer-encrypt"u8, StringComparison.OrdinalIgnoreCase)) {
					if (preference.HasValue)
						return false;

					// An unrecognized preference still counts as present: a second prefer-encrypt is a duplicate.
					ReadOnlySpan<byte> token = bytes.AsSpan (valueStart, valueLength);
					preference = token.Equals ("mutual"u8, StringComparison.OrdinalIgnoreCase) ? AutocryptPreference.Mutual : AutocryptPreference.NoPreference;
				} else if (attr[0] != '_') {
					// Only underscore-prefixed extensions are noncritical; other unknown attributes invalidate the field.
					return false;
				}
			}

			if (address is null || key is null)
				return false;

			value = new AutocryptValue (address, key, preference ?? AutocryptPreference.NoPreference);

			return true;
		}

		static void AppendToken (FormatOptions options, StringBuilder raw, ref int column, string text)
		{
			// Fold between attributes, not inside an addr-spec. A UTF-8 local part can occupy several bytes per character.
			int size = CharsetUtils.UTF8.GetByteCount (text);

			if (column + size + 1 > options.MaxLineLength) {
				raw.Append (options.NewLine).Append ('\t');
				column = 1;
			} else {
				raw.Append (' ');
				column++;
			}

			raw.Append (text);
			column += size;
		}

		internal Header ToHeader (HeaderId id, FormatOptions options)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			var field = id.ToHeaderName ();
			var raw = new StringBuilder ();
			int column = field.Length + 1;

			AppendToken (options, raw, ref column, "addr=" + Address + ";");

			if (id == HeaderId.Autocrypt && Preference == AutocryptPreference.Mutual)
				AppendToken (options, raw, ref column, "prefer-encrypt=mutual;");

			AppendToken (options, raw, ref column, "keydata=");

			var encoded = Convert.ToBase64String (KeyData);

			// Unlike the address, keydata can be split at any column because its decoder ignores folding whitespace.
			for (int offset = 0; offset < encoded.Length;) {
				if (column >= options.MaxLineLength) {
					raw.Append (options.NewLine).Append ('\t');
					column = 1;
				}

				int count = Math.Min (encoded.Length - offset, options.MaxLineLength - column);

				raw.Append (encoded, offset, count);
				column += count;
				offset += count;
			}

			raw.Append (options.NewLine);

			var bytes = CharsetUtils.UTF8.GetBytes (raw.ToString ());
			int wireLength = bytes.Length;

			// LF output can later be written with CRLF, including the final newline.
			if (options.NewLineFormat == NewLineFormat.Unix) {
				int index = 0, next;

				while ((next = bytes.AsSpan (index).IndexOf ((byte) '\n')) != -1) {
					wireLength++;
					index += next + 1;
				}
			}

			if (wireLength > MaxSize - field.Length - 1)
				throw new FormatException ("Autocrypt field exceeds 10 KiB.");

			var header = new Header (ParserOptions.Default, id, field, bytes);
			header.SetRawValue (bytes);

			return header;
		}
	}
}

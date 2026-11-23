//
// AutocryptGossip.cs
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
using System.Diagnostics.CodeAnalysis;

using MimeKit.Utils;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An Autocrypt-Gossip key advertisement for a recipient.
	/// </summary>
	/// <remarks>
	/// Gossip belongs inside an encrypted payload. Parsing alone does not validate or authenticate a key.
	/// </remarks>
	public sealed class AutocryptGossip
	{
		readonly AutocryptValue data;

		AutocryptGossip (AutocryptValue data)
		{
			this.data = data;
		}

		/// <summary>
		/// Create an advertisement for an addr-spec, copying the supplied binary key data.
		/// </summary>
		/// <remarks>
		/// Supply the binary key data rather than its Base64 representation. The constructor copies the bytes
		/// but does not validate the certificate.
		/// </remarks>
		/// <param name="address">The recipient's mailbox address without a display name.</param>
		/// <param name="keyData">The binary OpenPGP public key to advertise.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="address"/> or <paramref name="keyData"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="address"/> is not a single addr-spec with a domain.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="keyData"/> is empty or exceeds 7680 bytes.
		/// </exception>
		public AutocryptGossip (string address, byte[] keyData)
			: this (AutocryptValue.Create (address, keyData, AutocryptPreference.NoPreference))
		{
		}

		/// <summary>
		/// The canonical Autocrypt address.
		/// </summary>
		/// <remarks>
		/// Use the canonical form for Autocrypt address comparisons, not as evidence of identity.
		/// </remarks>
		/// <value>The lower-case addr-spec with internationalized domain names encoded as ASCII.</value>
		public string Address {
			get { return data.Address; }
		}

		/// <summary>
		/// The advertised binary key data.
		/// </summary>
		/// <remarks>
		/// The key bytes have not been checked for OpenPGP validity or ownership.
		/// </remarks>
		/// <value>The binary key data, without Base64 encoding.</value>
		public ReadOnlyMemory<byte> KeyData {
			get { return data.KeyData; }
		}

		/// <summary>
		/// Try to parse a raw field value, excluding the field name and colon.
		/// </summary>
		/// <remarks>
		/// The slice must contain only the field value. Folding whitespace is accepted and counts toward the 10
		/// KiB field limit.
		/// </remarks>
		/// <param name="buffer">The buffer containing the raw field value.</param>
		/// <param name="startIndex">The zero-based byte offset of the field value.</param>
		/// <param name="length">The number of bytes to parse.</param>
		/// <param name="value">The parsed gossip advertisement, or <see langword="null"/> on failure.</param>
		/// <returns><see langword="true"/> if the slice contains a well-formed gossip advertisement; otherwise, <see langword="false"/>.</returns>
		public static bool TryParse (byte[]? buffer, int startIndex, int length, [NotNullWhen (true)] out AutocryptGossip? value)
		{
			if (AutocryptValue.TryParse (HeaderId.AutocryptGossip, buffer, startIndex, length, out var data)) {
				value = new AutocryptGossip (data);
				return true;
			}

			value = null;

			return false;
		}

		/// <summary>
		/// Try to parse a <see cref="Header.RawValue"/>.
		/// </summary>
		/// <remarks>
		/// Pass <see cref="Header.RawValue"/> directly, including any folding whitespace but not the field name
		/// or colon.
		/// </remarks>
		/// <param name="buffer">The raw field value.</param>
		/// <param name="value">The parsed gossip advertisement, or <see langword="null"/> on failure.</param>
		/// <returns><see langword="true"/> if the field is well formed; otherwise, <see langword="false"/>.</returns>
		public static bool TryParse (byte[]? buffer, [NotNullWhen (true)] out AutocryptGossip? value)
		{
			return TryParse (buffer, 0, buffer?.Length ?? 0, out value);
		}

		/// <summary>
		/// Try to parse a field value without RFC 2047 decoding.
		/// </summary>
		/// <remarks>
		/// Supply only the field value, not the field name or colon. Use the byte overload when the original
		/// header bytes are available.
		/// </remarks>
		/// <param name="text">The field value to parse.</param>
		/// <param name="value">The parsed gossip advertisement, or <see langword="null"/> on failure.</param>
		/// <returns><see langword="true"/> if the field is well formed; otherwise, <see langword="false"/>.</returns>
		public static bool TryParse (string? text, [NotNullWhen (true)] out AutocryptGossip? value)
		{
			if (AutocryptValue.TryParse (HeaderId.AutocryptGossip, text, out var data)) {
				value = new AutocryptGossip (data);
				return true;
			}

			value = null;

			return false;
		}

		/// <summary>
		/// Parse a raw field value, excluding the field name and colon.
		/// </summary>
		/// <remarks>
		/// The slice must contain only the field value. Folding whitespace is accepted and counts toward the 10
		/// KiB field limit.
		/// </remarks>
		/// <param name="buffer">The buffer containing the raw field value.</param>
		/// <param name="startIndex">The zero-based byte offset of the field value.</param>
		/// <param name="length">The number of bytes to parse.</param>
		/// <returns>The parsed gossip advertisement.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="buffer"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The specified range is outside the buffer.
		/// </exception>
		/// <exception cref="ParseException">
		/// The slice does not contain a valid Autocrypt-Gossip field value.
		/// </exception>
		public static AutocryptGossip Parse (byte[] buffer, int startIndex, int length)
		{
			ArgumentValidator.Validate (buffer, startIndex, length);

			if (TryParse (buffer, startIndex, length, out var value))
				return value;

			throw new ParseException ("Invalid Autocrypt-Gossip field value.", startIndex, startIndex);
		}

		/// <summary>
		/// Parse a <see cref="Header.RawValue"/>.
		/// </summary>
		/// <remarks>
		/// Pass <see cref="Header.RawValue"/> directly, including any folding whitespace but not the field name
		/// or colon.
		/// </remarks>
		/// <param name="buffer">The raw field value.</param>
		/// <returns>The parsed gossip advertisement.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="buffer"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ParseException">
		/// The buffer does not contain a valid Autocrypt-Gossip field value.
		/// </exception>
		public static AutocryptGossip Parse (byte[] buffer)
		{
			return Parse (buffer, 0, buffer?.Length ?? 0);
		}

		/// <summary>
		/// Parse a field value without RFC 2047 decoding.
		/// </summary>
		/// <remarks>
		/// Supply only the field value, not the field name or colon. Use the byte overload when the original
		/// header bytes are available.
		/// </remarks>
		/// <param name="text">The field value to parse.</param>
		/// <returns>The parsed gossip advertisement.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="text"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ParseException">
		/// The text is not a valid Autocrypt-Gossip field value.
		/// </exception>
		public static AutocryptGossip Parse (string text)
		{
			if (text is null)
				throw new ArgumentNullException (nameof (text));

			if (TryParse (text, out var value))
				return value;

			throw new ParseException ("Invalid Autocrypt-Gossip field value.", 0, 0);
		}

		/// <summary>
		/// Format an Autocrypt-Gossip header with default options.
		/// </summary>
		/// <remarks>
		/// Uses <see cref="FormatOptions.Default"/> and emits keydata as the last attribute.
		/// </remarks>
		/// <returns>A new Autocrypt-Gossip header for the root of the encrypted payload.</returns>
		/// <exception cref="FormatException">
		/// The formatted field exceeds the protocol size limit.
		/// </exception>
		public Header ToHeader ()
		{
			return ToHeader (FormatOptions.Default);
		}

		/// <summary>
		/// Format an Autocrypt-Gossip header, folding the Base64 key data.
		/// </summary>
		/// <remarks>
		/// The 10 KiB field limit includes the field name and the whitespace used for folding.
		/// </remarks>
		/// <param name="options">The formatting options to use for line endings and folding.</param>
		/// <returns>A new Autocrypt-Gossip header for the root of the encrypted payload.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="options"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="FormatException">
		/// The formatted field exceeds the protocol size limit.
		/// </exception>
		public Header ToHeader (FormatOptions options)
		{
			return data.ToHeader (HeaderId.AutocryptGossip, options);
		}
	}
}

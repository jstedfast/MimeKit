//
// Header.cs
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

using MimeKit.Utils;
using MimeKit.Cryptography;

namespace MimeKit {
	/// <summary>
	/// A class representing a Message or MIME header.
	/// </summary>
	/// <remarks>
	/// Represents a single header field and value pair.
	/// </remarks>
	public class Header
	{
		internal static readonly byte[] Colon = { (byte) ':' };
		static readonly char[] WhiteSpace = { ' ', '\t', '\r', '\n' };
		internal readonly ParserOptions Options;

		// cached FormatOptions that change the way the header is formatted
		//bool allowMixedHeaderCharsets = FormatOptions.Default.AllowMixedHeaderCharsets;
		//NewLineFormat newLineFormat = FormatOptions.Default.NewLineFormat;
		//bool international = FormatOptions.Default.International;
		//Encoding charset = CharsetUtils.UTF8;

		readonly byte[] rawField;
		bool explicitRawValue;
		string textValue;
		byte[] rawValue;

		/// <summary>
		/// Initialize a new instance of the <see cref="Header"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new message or entity header for the specified field and
		/// value pair. The encoding is used to determine which charset to use
		/// when encoding the value according to the rules of rfc2047.
		/// </remarks>
		/// <param name="encoding">The character encoding that should be used to
		/// encode the header value.</param>
		/// <param name="id">The header identifier.</param>
		/// <param name="value">The value of the header.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="encoding"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="id"/> is not a valid <see cref="HeaderId"/>.
		/// </exception>
		public Header (Encoding encoding, HeaderId id, string value)
		{
			if (encoding is null)
				throw new ArgumentNullException (nameof (encoding));

			if (id == HeaderId.Unknown)
				throw new ArgumentOutOfRangeException (nameof (id));

			if (value is null)
				throw new ArgumentNullException (nameof (value));

			Options = ParserOptions.Default.Clone ();
			Field = id.ToHeaderName ();
			Id = id;

			rawField = Encoding.ASCII.GetBytes (Field);
			SetValue (encoding, value);
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="Header"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new message or entity header for the specified field and
		/// value pair. The encoding is used to determine which charset to use
		/// when encoding the value according to the rules of rfc2047.
		/// </remarks>
		/// <param name="charset">The charset that should be used to encode the
		/// header value.</param>
		/// <param name="id">The header identifier.</param>
		/// <param name="value">The value of the header.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="charset"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="id"/> is not a valid <see cref="HeaderId"/>.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <paramref name="charset"/> is not supported.
		/// </exception>
		public Header (string charset, HeaderId id, string value)
		{
			if (charset is null)
				throw new ArgumentNullException (nameof (charset));

			if (id == HeaderId.Unknown)
				throw new ArgumentOutOfRangeException (nameof (id));

			if (value is null)
				throw new ArgumentNullException (nameof (value));

			var encoding = CharsetUtils.GetEncoding (charset);
			Options = ParserOptions.Default.Clone ();
			Field = id.ToHeaderName ();
			Id = id;

			rawField = Encoding.ASCII.GetBytes (Field);
			SetValue (encoding, value);
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="Header"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new message or entity header for the specified field and
		/// value pair with the UTF-8 encoding.
		/// </remarks>
		/// <param name="id">The header identifier.</param>
		/// <param name="value">The value of the header.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="id"/> is not a valid <see cref="HeaderId"/>.
		/// </exception>
		public Header (HeaderId id, string value) : this (Encoding.UTF8, id, value)
		{
		}

		static void ValidateFieldName (string field)
		{
			if (field is null)
				throw new ArgumentNullException (nameof (field));

			if (field.Length == 0)
				throw new ArgumentException ("Header field names are not allowed to be empty.", nameof (field));

			for (int i = 0; i < field.Length; i++) {
				if (field[i] >= 127 || !IsFieldText ((byte) field[i]))
					throw new ArgumentException ("Illegal characters in header field name.", nameof (field));
			}
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="Header"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new message or entity header for the specified field and
		/// value pair. The encoding is used to determine which charset to use
		/// when encoding the value according to the rules of rfc2047.
		/// </remarks>
		/// <param name="encoding">The character encoding that should be used
		/// to encode the header value.</param>
		/// <param name="field">The name of the header field.</param>
		/// <param name="value">The value of the header.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="encoding"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="field"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The <paramref name="field"/> contains illegal characters.
		/// </exception>
		public Header (Encoding encoding, string field, string value)
		{
			if (encoding is null)
				throw new ArgumentNullException (nameof (encoding));

			ValidateFieldName (field);

			if (value is null)
				throw new ArgumentNullException (nameof (value));

			Options = ParserOptions.Default.Clone ();
			Id = field.ToHeaderId ();
			Field = field;

			rawField = Encoding.ASCII.GetBytes (field);
			SetValue (encoding, value);
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="Header"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new message or entity header for the specified field and
		/// value pair. The encoding is used to determine which charset to use
		/// when encoding the value according to the rules of rfc2047.
		/// </remarks>
		/// <param name="charset">The charset that should be used to encode the
		/// header value.</param>
		/// <param name="field">The name of the header field.</param>
		/// <param name="value">The value of the header.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="charset"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="field"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The <paramref name="field"/> contains illegal characters.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <paramref name="charset"/> is not supported.
		/// </exception>
		public Header (string charset, string field, string value)
		{
			if (charset is null)
				throw new ArgumentNullException (nameof (charset));

			ValidateFieldName (field);

			if (value is null)
				throw new ArgumentNullException (nameof (value));

			var encoding = CharsetUtils.GetEncoding (charset);
			Options = ParserOptions.Default.Clone ();
			Id = field.ToHeaderId ();
			Field = field;

			rawField = Encoding.ASCII.GetBytes (field);
			SetValue (encoding, value);
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="Header"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new message or entity header for the specified field and
		/// value pair with the UTF-8 encoding.
		/// </remarks>
		/// <param name="field">The name of the header field.</param>
		/// <param name="value">The value of the header.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="field"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The <paramref name="field"/> contains illegal characters.
		/// </exception>
		public Header (string field, string value) : this (Encoding.UTF8, field, value)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="Header"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new message or entity header with the specified values.</para>
		/// <para>This constructor is used by <see cref="Clone"/>.</para>
		/// </remarks>
		/// <param name="options">The parser options used.</param>
		/// <param name="id">The id of the header.</param>
		/// <param name="name">The name of the header field.</param>
		/// <param name="field">The raw header field.</param>
		/// <param name="value">The raw value of the header.</param>
		protected Header (ParserOptions options, HeaderId id, string name, byte[] field, byte[] value)
		{
			Options = options;
			rawField = field;
			rawValue = value;
			Field = name;
			Id = id;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="Header"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new message or entity header with the specified raw values.</para>
		/// <para>This constructor is used by the
		/// <a href="Overload_MimeKit_Header_TryParse.htm">TryParse</a> methods.</para>
		/// </remarks>
		/// <param name="options">The parser options used.</param>
		/// <param name="field">The raw header field.</param>
		/// <param name="fieldNameLength">The length of the field name (not including trailing whitespace).</param>
		/// <param name="value">The raw value of the header.</param>
		/// <param name="invalid"><c>true</c> if the header field is invalid; otherwise, <c>false</c>.</param>
#if NET5_0_OR_GREATER
		[System.Runtime.CompilerServices.SkipLocalsInit]
#endif
		internal protected Header (ParserOptions options, byte[] field, int fieldNameLength, byte[] value, bool invalid)
		{
			Span<char> chars = fieldNameLength <= 32
				? stackalloc char[32]
				: new char[fieldNameLength];

			for (int i = 0; i < fieldNameLength; i++)
				chars[i] = (char) field[i];

			Options = options;
			rawField = field;
			rawValue = value;

			Field = chars.Slice (0, fieldNameLength).ToString ();
			Id = Field.ToHeaderId ();
			IsInvalid = invalid;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="Header"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new message or entity header with the specified raw values.</para>
		/// <para>This constructor is used by the
		/// <a href="Overload_MimeKit_Header_TryParse.htm">TryParse</a> methods.</para>
		/// </remarks>
		/// <param name="options">The parser options used.</param>
		/// <param name="field">The raw header field.</param>
		/// <param name="value">The raw value of the header.</param>
		/// <param name="invalid"><c>true</c> if the header field is invalid; otherwise, <c>false</c>.</param>
#if NET5_0_OR_GREATER
		[System.Runtime.CompilerServices.SkipLocalsInit]
#endif
		internal protected Header (ParserOptions options, byte[] field, byte[] value, bool invalid)
		{
			Span<char> chars = field.Length <= 32
				? stackalloc char[32]
				: new char[field.Length];

			int count = 0;

			while (count < field.Length && (invalid || !field[count].IsBlank ())) {
				chars[count] = (char) field[count];
				count++;
			}

			Options = options;
			rawField = field;
			rawValue = value;

			Field = chars.Slice (0, count).ToString ();
			Id = Field.ToHeaderId ();
			IsInvalid = invalid;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="Header"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new message or entity header with the specified raw values.</para>
		/// <para>This constructor is used by <see cref="MimeMessage"/> and <see cref="MimeEntity"/>
		/// when serializing new values for headers.</para>
		/// </remarks>
		/// <param name="options">The parser options used.</param>
		/// <param name="id">The id of the header.</param>
		/// <param name="field">The raw header field.</param>
		/// <param name="value">The raw value of the header.</param>
		internal protected Header (ParserOptions options, HeaderId id, string field, byte[] value)
		{
			Options = options;
			rawField = Encoding.ASCII.GetBytes (field);
			rawValue = value;
			Field = field;
			Id = id;
		}

		/// <summary>
		/// Clone the header.
		/// </summary>
		/// <remarks>
		/// Clones the header, copying the current RawValue.
		/// </remarks>
		/// <returns>A copy of the header with its current state.</returns>
		public Header Clone ()
		{
			return new Header (Options, Id, Field, rawField, rawValue) {
				explicitRawValue = explicitRawValue,
				IsInvalid = IsInvalid,

				// if the textValue has already been calculated, set it on the cloned header as well.
				textValue = textValue
			};
		}

		/// <summary>
		/// Get the stream offset of the beginning of the header.
		/// </summary>
		/// <remarks>
		/// If the offset is set, it refers to the byte offset where it
		/// was found in the stream it was parsed from.
		/// </remarks>
		/// <value>The stream offset.</value>
		public long? Offset {
			get; internal set;
		}

		/// <summary>
		/// Get the name of the header field.
		/// </summary>
		/// <remarks>
		/// Represents the field name of the header.
		/// </remarks>
		/// <value>The name of the header field.</value>
		public string Field {
			get; private set;
		}

		/// <summary>
		/// Get the header identifier.
		/// </summary>
		/// <remarks>
		/// This property is mainly used for switch-statements for performance reasons.
		/// </remarks>
		/// <value>The header identifier.</value>
		public HeaderId Id {
			get; private set;
		}

		internal bool IsInvalid {
			get; private set;
		}

		/// <summary>
		/// Get the raw field name of the header.
		/// </summary>
		/// <remarks>
		/// Contains the raw field name of the header.
		/// </remarks>
		/// <value>The raw field name of the header.</value>
		public byte[] RawField {
			get { return rawField; }
		}

		/// <summary>
		/// Get the raw value of the header.
		/// </summary>
		/// <remarks>
		/// Contains the raw value of the header, before any decoding or charset conversion.
		/// </remarks>
		/// <value>The raw value of the header.</value>
		public byte[] RawValue {
			get { return rawValue; }
		}

		/// <summary>
		/// Get or sets the header value.
		/// </summary>
		/// <remarks>
		/// Represents the decoded header value and is suitable for displaying to the user.
		/// </remarks>
		/// <value>The header value.</value>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		public string Value {
			get {
				textValue ??= Unfold (Rfc2047.DecodeText (Options, rawValue));

				return textValue;
			}
			set {
				SetValue (FormatOptions.Default, Encoding.UTF8, value);
			}
		}

		/// <summary>
		/// Get the header value using the specified character encoding.
		/// </summary>
		/// <remarks>
		/// <para>If the raw header value does not properly encode non-ASCII text, the decoder
		/// will fall back to a default charset encoding. Sometimes, however, this
		/// default charset fallback is wrong and the mail client may wish to override
		/// that default charset on a per-header basis.</para>
		/// <para>By using this method, the client is able to override the fallback charset
		/// on a per-header basis.</para>
		/// </remarks>
		/// <returns>The value.</returns>
		/// <param name="encoding">The character encoding to use as a fallback.</param>
		public string GetValue (Encoding encoding)
		{
			if (encoding is null)
				throw new ArgumentNullException (nameof (encoding));

			var options = Options.Clone ();
			options.CharsetEncoding = encoding;

			return Unfold (Rfc2047.DecodeText (options, rawValue));
		}

		/// <summary>
		/// Get the header value using the specified charset.
		/// </summary>
		/// <remarks>
		/// <para>If the raw header value does not properly encode non-ASCII text, the decoder
		/// will fall back to a default charset encoding. Sometimes, however, this
		/// default charset fallback is wrong and the mail client may wish to override
		/// that default charset on a per-header basis.</para>
		/// <para>By using this method, the client is able to override the fallback charset
		/// on a per-header basis.</para>
		/// </remarks>
		/// <returns>The value.</returns>
		/// <param name="charset">The charset to use as a fallback.</param>
		public string GetValue (string charset)
		{
			if (charset is null)
				throw new ArgumentNullException (nameof (charset));

			var encoding = CharsetUtils.GetEncoding (charset);

			return GetValue (encoding);
		}

		static byte[] ReformatAddressHeader (ParserOptions options, FormatOptions format, Encoding encoding, string field, byte[] rawValue)
		{
			if (!InternetAddressList.TryParse (options, rawValue, 0, rawValue.Length, out var list))
				return rawValue;

			var encoded = new StringBuilder (" ");
			int lineLength = field.Length + 2;

			list.Encode (format, encoded, true, ref lineLength);
			encoded.Append (format.NewLine);

			if (format.International)
				return Encoding.UTF8.GetBytes (encoded.ToString ());

			return Encoding.ASCII.GetBytes (encoded.ToString ());
		}

		static byte[] EncodeAddressHeader (ParserOptions options, FormatOptions format, Encoding encoding, string field, string value)
		{
			if (!InternetAddressList.TryParse (options, value, out var list))
				return EncodeUnstructuredHeader (options, format, encoding, field, value);

			var encoded = new StringBuilder (" ");
			int lineLength = field.Length + 2;

			list.Encode (format, encoded, true, ref lineLength);
			encoded.Append (format.NewLine);

			if (format.International)
				return Encoding.UTF8.GetBytes (encoded.ToString ());

			return Encoding.ASCII.GetBytes (encoded.ToString ());
		}

		static byte[] EncodeMessageIdHeader (ParserOptions options, FormatOptions format, Encoding encoding, string field, string value)
		{
			return encoding.GetBytes (" " + value + format.NewLine);
		}

		delegate void ReceivedTokenSkipValueFunc (byte[] text, ref int index);

		static void ReceivedTokenSkipAtom (byte[] text, ref int index)
		{
			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, text.Length, false) || index >= text.Length)
				return;

			ParseUtils.SkipAtom (text, ref index, text.Length);
		}

		static void ReceivedTokenSkipDomain (byte[] text, ref int index)
		{
			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, text.Length, false))
				return;

			if (text[index] == (byte) '[') {
				while (index < text.Length && text[index] != (byte) ']')
					index++;

				if (index < text.Length)
					index++;

				return;
			}

			while (ParseUtils.SkipAtom (text, ref index, text.Length) && index < text.Length && text[index] == (byte) '.')
				index++;
		}

		static ReadOnlySpan<byte> ReceivedAddrSpecSentinels => new[] { (byte) '>', (byte) ';' };
		static ReadOnlySpan<byte> ReceivedMessageIdSentinels => new[] { (byte) '>' };

		static void ReceivedTokenSkipAddress (byte[] text, ref int index)
		{
			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, text.Length, false) || index >= text.Length)
				return;

			if (text[index] == (byte) '<')
				index++;

			InternetAddress.TryParseAddrspec (text, ref index, text.Length, ReceivedAddrSpecSentinels, RfcComplianceMode.Strict, false, out _, out _);

			if (index < text.Length && text[index] == (byte) '>')
				index++;
		}

		static void ReceivedTokenSkipMessageId (byte[] text, ref int index)
		{
			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, text.Length, false) || index >= text.Length)
				return;

			if (text[index] == (byte) '<') {
				index++;

				InternetAddress.TryParseAddrspec (text, ref index, text.Length, ReceivedMessageIdSentinels, RfcComplianceMode.Strict, false, out _, out _);

				if (index < text.Length && text[index] == (byte) '>')
					index++;
			} else {
				ParseUtils.SkipAtom (text, ref index, text.Length);
			}
		}

		readonly struct ReceivedToken {
			public readonly ReceivedTokenSkipValueFunc Skip;
			public readonly string Atom;

			public ReceivedToken (string atom, ReceivedTokenSkipValueFunc skip)
			{
				Atom = atom;
				Skip = skip;
			}
		}

		static readonly ReceivedToken[] ReceivedTokens = {
			new ReceivedToken ("from", ReceivedTokenSkipDomain),
			new ReceivedToken ("by", ReceivedTokenSkipDomain),
			new ReceivedToken ("via", ReceivedTokenSkipDomain),
			new ReceivedToken ("with", ReceivedTokenSkipAtom),
			new ReceivedToken ("id", ReceivedTokenSkipMessageId),
			new ReceivedToken ("for", ReceivedTokenSkipAddress),
		};

		class ReceivedTokenValue
		{
			public readonly int StartIndex;
			public readonly int Length;

			public ReceivedTokenValue (int startIndex, int length)
			{
				StartIndex = startIndex;
				Length = length;
			}
		}

		static byte[] EncodeReceivedHeader (ParserOptions options, FormatOptions format, Encoding encoding, string field, string value)
		{
			var tokens = new List<ReceivedTokenValue> ();
			var rawValue = encoding.GetBytes (value);
			var encoded = new ValueStringBuilder (rawValue.Length);
			int lineLength = field.Length + 1;
			bool date = false;
			int index = 0;
			int count = 0;

			while (index < rawValue.Length) {
				ReceivedTokenValue token = null;
				int startIndex = index;

				if (!ParseUtils.SkipCommentsAndWhiteSpace (rawValue, ref index, rawValue.Length, false) || index >= rawValue.Length) {
					tokens.Add (new ReceivedTokenValue (startIndex, index - startIndex));
					break;
				}

				while (index < rawValue.Length && !rawValue[index].IsWhitespace ())
					index++;

				var atom = encoding.GetString (rawValue, startIndex, index - startIndex);

				for (int i = 0; i < ReceivedTokens.Length; i++) {
					if (atom == ReceivedTokens[i].Atom) {
						ReceivedTokens[i].Skip (rawValue, ref index);

						if (ParseUtils.SkipCommentsAndWhiteSpace (rawValue, ref index, rawValue.Length, false)) {
							if (index < rawValue.Length && rawValue[index] == (byte) ';') {
								date = true;
								index++;
							}
						}

						token = new ReceivedTokenValue (startIndex, index - startIndex);
						break;
					}
				}

				if (token is null) {
					if (ParseUtils.SkipCommentsAndWhiteSpace (rawValue, ref index, rawValue.Length, false)) {
						while (index < rawValue.Length && !rawValue[index].IsWhitespace ())
							index++;
					}

					token = new ReceivedTokenValue (startIndex, index - startIndex);
				}

				tokens.Add (token);

				ParseUtils.SkipWhiteSpace (rawValue, ref index, rawValue.Length);

				if (date && index < rawValue.Length) {
					// slurp up the date (the final token)
					tokens.Add (new ReceivedTokenValue (index, rawValue.Length - index));
					break;
				}
			}

			foreach (var token in tokens) {
				var text = encoding.GetString (rawValue, token.StartIndex, token.Length).TrimEnd ();

				if (count > 0 && lineLength + text.Length + 1 > format.MaxLineLength) {
					encoded.Append (format.NewLine);
					encoded.Append ('\t');
					lineLength = 1;
					count = 0;
				} else {
					encoded.Append (' ');
					lineLength++;
				}

				lineLength += text.Length;
				encoded.Append (text);
				count++;
			}

			encoded.Append (format.NewLine);

			return encoding.GetBytes (encoded.ToString ());
		}

		static byte[] EncodeAuthenticationResultsHeader (ParserOptions options, FormatOptions format, Encoding encoding, string field, string value)
		{
			var buffer = Encoding.UTF8.GetBytes (value);

			if (!AuthenticationResults.TryParse (buffer, out AuthenticationResults authres))
				return EncodeUnstructuredHeader (options, format, encoding, field, value);

			var encoded = new StringBuilder ();
			int lineLength = field.Length + 1;

			authres.Encode (format, encoded, lineLength);

			return encoding.GetBytes (encoded.ToString ());
		}

		static void EncodeDkimLongValue (FormatOptions format, ref ValueStringBuilder encoded, ref int lineLength, ReadOnlySpan<char> value)
		{
			int startIndex = 0;

			do {
				int lineLeft = format.MaxLineLength - lineLength;
				int index = Math.Min (startIndex + lineLeft, value.Length);

				encoded.Append (value.Slice (startIndex, index - startIndex));
				lineLength += (index - startIndex);

				if (index == value.Length)
					break;

				encoded.Append (format.NewLine);
				encoded.Append ('\t');
				lineLength = 1;

				startIndex = index;
			} while (true);
		}

		static void EncodeDkimHeaderList (FormatOptions format, ref ValueStringBuilder encoded, ref int lineLength, ReadOnlySpan<char> value, char delim)
		{
			int i = 0;

			foreach (var token in value.Tokenize (delim)) {
				if (i > 0) {
					encoded.Append (delim);
					lineLength++;
				}

				if (lineLength + token.Length + 1 > format.MaxLineLength) {
					encoded.Append (format.NewLine);
					encoded.Append ('\t');
					lineLength = 1;

					if (token.Length + 1 > format.MaxLineLength) {
						EncodeDkimLongValue (format, ref encoded, ref lineLength, token);
					} else {
						lineLength += token.Length;
						encoded.Append (token);
					}
				} else {
					lineLength += token.Length;
					encoded.Append (token);
				}

				i++;
			}
		}

		static byte[] EncodeDkimOrArcSignatureHeader (ParserOptions options, FormatOptions format, Encoding encoding, string field, string value)
		{
			var encoded = new ValueStringBuilder (value.Length);
			int lineLength = field.Length + 1;
			int index = 0;

			while (index < value.Length) {
				using var token = new ValueStringBuilder (128);

				while (index < value.Length && IsWhiteSpace (value[index]))
					index++;

				int startIndex = index;

				while (index < value.Length && value[index] != '=') {
					if (!IsWhiteSpace (value[index]))
						token.Append (value[index]);
					index++;
				}

				var name = value.AsSpan (startIndex, index - startIndex);

				while (index < value.Length && value[index] != ';') {
					if (!IsWhiteSpace (value[index]))
						token.Append (value[index]);
					index++;
				}

				if (index < value.Length && value[index] == ';') {
					token.Append (';');
					index++;
				}

				if (lineLength + token.Length + 1 > format.MaxLineLength || name.SequenceEqual ("bh".AsSpan ()) || name.SequenceEqual ("b".AsSpan ())) {
					encoded.Append (format.NewLine);
					encoded.Append ('\t');
					lineLength = 1;
				} else {
					encoded.Append (' ');
					lineLength++;
				}

				if (token.Length > format.MaxLineLength) {
					if (name.SequenceEqual ("z".AsSpan ())) {
						EncodeDkimHeaderList (format, ref encoded, ref lineLength, token.AsSpan (), '|');
					} else if (name.SequenceEqual ("h".AsSpan ())) {
						EncodeDkimHeaderList (format, ref encoded, ref lineLength, token.AsSpan (), ':');
					} else {
						EncodeDkimLongValue (format, ref encoded, ref lineLength, token.AsSpan ());
					}
				} else {
					encoded.Append (token.AsSpan ());
					lineLength += token.Length;
				}
			}

			encoded.Append (format.NewLine);

			return encoding.GetBytes (encoded.ToString ());
		}

		static byte[] EncodeDispositionNotificationOptions (ParserOptions options, FormatOptions format, Encoding encoding, string field, string value)
		{
			var encoded = new ValueStringBuilder (value.Length);
			int lineLength = field.Length + 1;
			int index = 0;

			while (index < value.Length) {
				using var parameter = new ValueStringBuilder (128);

				while (index < value.Length && IsWhiteSpace (value[index]))
					index++;

				int startIndex = index;

				while (index < value.Length && value[index] != '=') {
					if (!IsWhiteSpace (value[index]))
						parameter.Append (value[index]);
					index++;
				}

				while (index < value.Length && value[index] != ';') {
					if (!IsWhiteSpace (value[index]))
						parameter.Append (value[index]);
					index++;
				}

				if (index < value.Length && value[index] == ';') {
					parameter.Append (';');
					index++;
				}

				if (lineLength + parameter.Length + 1 > format.MaxLineLength && encoded.Length > 0) {
					encoded.Append (format.NewLine);
					encoded.Append ('\t');
					lineLength = 1;
				} else {
					encoded.Append (' ');
					lineLength++;
				}

				encoded.Append (parameter.AsSpan ());
				lineLength += parameter.Length;
			}

			encoded.Append (format.NewLine);

			return encoding.GetBytes (encoded.ToString ());
		}

		static byte[] EncodeReferencesHeader (ParserOptions options, FormatOptions format, Encoding encoding, string field, string value)
		{
			var encoded = new ValueStringBuilder (value.Length);
			int lineLength = field.Length + 1;
			int count = 0;

			foreach (var reference in MimeUtils.EnumerateReferences (value)) {
				if (count > 0 && lineLength + reference.Length + 3 > format.MaxLineLength) {
					encoded.Append (format.NewLine);
					encoded.Append ('\t');
					lineLength = 1;
					count = 0;
				} else {
					encoded.Append (' ');
					lineLength++;
				}

				encoded.Append ('<');
				encoded.Append (reference);
				encoded.Append ('>');
				lineLength += reference.Length + 2;
				count++;
			}

			encoded.Append (format.NewLine);

			return encoding.GetBytes (encoded.ToString ());
		}

		static bool IsWhiteSpace (char c)
		{
			return c is ' ' or '\t' or '\r' or '\n';
		}

		readonly struct Word
		{
			readonly string text;
			readonly int startIndex;
			readonly int length;

			public Word (string text, int startIndex, int length)
			{
				this.startIndex = startIndex;
				this.length = length;
				this.text = text;
			}

			public int Length => length;

			public char this[int index] {
				get {
					return text[startIndex + index];
				}
			}

			public ReadOnlySpan<char> AsSpan ()
			{
				return text.AsSpan (startIndex, length);
			}

			public IEnumerable<Word> Break (FormatOptions format, int lineLength)
			{
				int endIndex = startIndex + length;
				int index = startIndex;

				lineLength = Math.Max (lineLength, 1);

				while (index < endIndex) {
					int length = Math.Min (format.MaxLineLength - lineLength, endIndex - index);

					if (char.IsSurrogatePair (text, index + length - 1))
						length--;

					yield return new Word (text, index, length);

					index += length;
					lineLength = 1;
				}

				yield break;
			}
		}

		static IEnumerable<Word> TokenizeText (string text)
		{
			int index = 0;

			while (index < text.Length) {
				int startIndex = index;

				while (index < text.Length && !IsWhiteSpace (text[index]))
					index++;

				if (index > startIndex)
					yield return new Word (text, startIndex, index - startIndex);

				if (index == text.Length)
					break;

				startIndex = index;

				while (index < text.Length && IsWhiteSpace (text[index]))
					index++;

				yield return new Word (text, startIndex, index - startIndex);
			}

			yield break;
		}

		internal static string Fold (FormatOptions format, string field, string value)
		{
			var folded = new ValueStringBuilder (value.Length);
			int lineLength = field.Length + 2;
			int lastLwsp = -1;

			folded.Append (' ');

			var words = TokenizeText (value);

			foreach (var word in words) {
				if (IsWhiteSpace (word[0])) {
					if (lineLength + word.Length > format.MaxLineLength) {
						for (int i = 0; i < word.Length; i++) {
							if (lineLength > format.MaxLineLength) {
								folded.Append (format.NewLine);
								lineLength = 0;
							}

							folded.Append (word[i]);
							lineLength++;
						}
					} else {
						lineLength += word.Length;
						folded.Append (word.AsSpan ());
					}

					lastLwsp = folded.Length - 1;
					continue;
				}

				if (lastLwsp != -1 && lineLength + word.Length > format.MaxLineLength) {
					folded.Insert (lastLwsp, format.NewLine);
					lineLength = 1;
					lastLwsp = -1;
				}

				if (word.Length > format.MaxLineLength) {
					foreach (var broken in word.Break (format, lineLength)) {
						if (lineLength + broken.Length > format.MaxLineLength) {
							folded.Append (format.NewLine);
							folded.Append (' ');
							lineLength = 1;
						}

						folded.Append (broken.AsSpan ());
						lineLength += broken.Length;
					}
				} else {
					folded.Append (word.AsSpan ());
					lineLength += word.Length;
				}
			}

			folded.Append (format.NewLine);

			return folded.ToString ();
		}

		static byte[] ReformatContentDisposition (ParserOptions options, FormatOptions format, Encoding encoding, string field, byte[] rawValue)
		{
			if (!ContentDisposition.TryParse (options, rawValue, out var disposition))
				return rawValue;

			var encoded = disposition.Encode (format, encoding);

			return Encoding.UTF8.GetBytes (encoded);
		}

		static byte[] EncodeContentDisposition (ParserOptions options, FormatOptions format, Encoding encoding, string field, string value)
		{
			var disposition = ContentDisposition.Parse (options, value);
			var encoded = disposition.Encode (format, encoding);

			return Encoding.UTF8.GetBytes (encoded);
		}

		static byte[] ReformatContentType (ParserOptions options, FormatOptions format, Encoding encoding, string field, byte[] rawValue)
		{
			if (!ContentType.TryParse (options, rawValue, out var contentType))
				return rawValue;

			var encoded = contentType.Encode (format, encoding);

			return Encoding.UTF8.GetBytes (encoded);
		}

		static byte[] EncodeContentType (ParserOptions options, FormatOptions format, Encoding encoding, string field, string value)
		{
			var contentType = ContentType.Parse (options, value);
			var encoded = contentType.Encode (format, encoding);

			return Encoding.UTF8.GetBytes (encoded);
		}

		static void AppendWord (FormatOptions format, ref ValueStringBuilder builder, ref int lineLength, ReadOnlySpan<char> word)
		{
			if (lineLength + word.Length + 1 <= format.MaxLineLength) {
				builder.Append (' ');
				lineLength++;

				builder.Append (word);
				lineLength += word.Length;
			} else if (word.Length + 1 <= format.MaxLineLength) {
				builder.Append (format.NewLine);
				builder.Append (' ');
				builder.Append (word);
				lineLength = word.Length + 1;
			} else {
				int remaining = word.Length;
				int index = 0;

				do {
					int wordLength = Math.Min (remaining, format.MaxLineLength - (lineLength + 1));

					builder.Append (' ');
					lineLength++;

					builder.Append (word.Slice (index, wordLength));
					lineLength += wordLength;
					remaining -= wordLength;
					index += wordLength;

					if (remaining == 0)
						break;

					builder.Append (format.NewLine);
					lineLength = 0;
				} while (true);
			}
		}

		static void AppendComment (FormatOptions format, Encoding encoding, ref ValueStringBuilder builder, ref int lineLength, string value, int startIndex, int length)
		{
			ReadOnlySpan<char> comment;

			if (!format.International) {
				comment = Rfc2047.EncodeComment (format, encoding, value, startIndex + 1, length - 2).AsSpan ();
			} else {
				comment = value.AsSpan (startIndex, length);
			}

			// Try to fit the entire comment on a single line.
			if (lineLength + comment.Length + 1 <= format.MaxLineLength) {
				builder.Append (' ');
				lineLength++;

				builder.Append (comment);
				lineLength += comment.Length;
			} else if (comment.Length + 1 <= format.MaxLineLength) {
				builder.Append (format.NewLine);
				builder.Append (' ');
				builder.Append (comment);
				lineLength = comment.Length + 1;
			} else {
				// We'll need to split the comment over multiple lines.
				int index = 0;

				do {
					// Try to split on words within the comment.
					int wspIndex = comment.Slice (index).IndexOfAny (WhiteSpace.AsSpan ());
					wspIndex = wspIndex != -1 ? index + wspIndex : comment.Length;

					int wordLength = wspIndex - index;

					AppendWord (format, ref builder, ref lineLength, comment.Slice (index, wordLength));
					index = wspIndex;

					if (index < comment.Length) {
						// Skip over any whitespace (which will effectively compact it).
						// Note: Since we know this is a comment, the very last char will be a ')', so it can never end with whitespace.
						while (IsWhiteSpace (comment[index]))
							index++;
					}
				} while (index < comment.Length);
			}
		}

		static bool IsMailingListCommandSpecial (char c)
		{
			return c is '<' or '(' or ',';
		}

		static byte[] EncodeMailingListCommandHeader (ParserOptions options, FormatOptions format, Encoding encoding, string field, string value)
		{
			var encoded = new ValueStringBuilder (value.Length);
			int lineLength = field.Length + 1;
			int index = 0;

			while (index < value.Length) {
				while (index < value.Length && IsWhiteSpace (value[index]))
					index++;

				if (index >= value.Length)
					break;

				int startIndex = index;

				if (value[index] == '<') {
					// url
					while (index < value.Length && value[index] != '>')
						index++;

					if (index < value.Length) {
						index++;

						int urlLength = index - startIndex;

						if (lineLength + urlLength + 1 < format.MaxLineLength) {
							encoded.Append (' ');
							lineLength++;

							encoded.Append (value.AsSpan (startIndex, urlLength));
							lineLength += urlLength;
						} else {
							// Do not break apart a URL that is not already broken.
							encoded.Append (format.NewLine);
							encoded.Append (' ');
							lineLength = 1;

							encoded.Append (value.AsSpan (startIndex, urlLength));
							lineLength += urlLength;
						}

						continue;
					}

					// Fall through to handling this token as a normal word token.
				} else if (value[index] == '(') {
					// comment
					if (ParseUtils.SkipComment (value, ref index, value.Length)) {
						AppendComment (format, encoding, ref encoded, ref lineLength, value, startIndex, index - startIndex);
						continue;
					}

					// Fall through to handling this token as a normal word token.
				} else if (value[index] == ',') {
					// Collapse multiple commas into a single comma.
					while (index < value.Length && (value[index] == ',' || IsWhiteSpace (value[index])))
						index++;

					encoded.Append (',');
					lineLength++;
					continue;
				}

				// word
				while (index < value.Length && !IsWhiteSpace (value[index]) && !IsMailingListCommandSpecial (value[index]))
					index++;

				AppendWord (format, ref encoded, ref lineLength, value.AsSpan (startIndex, index - startIndex));
			}

			encoded.Append (format.NewLine);

			return encoding.GetBytes (encoded.ToString ());
		}

		static byte[] EncodeUnstructuredHeader (ParserOptions options, FormatOptions format, Encoding encoding, string field, string value)
		{
			if (format.International) {
				var folded = Fold (format, field, value);

				return Encoding.UTF8.GetBytes (folded);
			}

			var encoded = Rfc2047.EncodeText (format, encoding, value);

			return Rfc2047.FoldUnstructuredHeader (format, field, encoded);
		}

		/// <summary>
		/// Format the raw value of the header to conform with the specified formatting options.
		/// </summary>
		/// <remarks>
		/// <para>This method is called by the <a href="Overload_MimeKit_Header_SetValue.htm">SetValue</a>
		/// methods.</para>
		/// <para>This method should encode unicode characters according to the rules of rfc2047 (when
		/// <see cref="FormatOptions.International"/> is <c>false</c>) as well as properly folding the
		/// value to conform with rfc5322.</para>
		/// </remarks>
		/// <param name="format">The formatting options.</param>
		/// <param name="encoding">The character encoding to be used.</param>
		/// <param name="value">The decoded (and unfolded) header value.</param>
		/// <returns>A byte array containing the raw header value that should be written.</returns>
		protected virtual byte[] FormatRawValue (FormatOptions format, Encoding encoding, string value)
		{
			switch (Id) {
			case HeaderId.DispositionNotificationTo:
			case HeaderId.ResentReplyTo:
			case HeaderId.ResentSender:
			case HeaderId.ResentFrom:
			case HeaderId.ResentBcc:
			case HeaderId.ResentCc:
			case HeaderId.ResentTo:
			case HeaderId.ReplyTo:
			case HeaderId.Sender:
			case HeaderId.From:
			case HeaderId.Bcc:
			case HeaderId.Cc:
			case HeaderId.To:
				return EncodeAddressHeader (Options, format, encoding, Field, value);
			case HeaderId.Received:
				return EncodeReceivedHeader (Options, format, encoding, Field, value);
			case HeaderId.ResentMessageId:
			case HeaderId.InReplyTo:
			case HeaderId.MessageId:
			case HeaderId.ContentId:
				return EncodeMessageIdHeader (Options, format, encoding, Field, value);
			case HeaderId.References:
				return EncodeReferencesHeader (Options, format, encoding, Field, value);
			case HeaderId.ContentDisposition:
				return EncodeContentDisposition (Options, format, encoding, Field, value);
			case HeaderId.ContentType:
				return EncodeContentType (Options, format, encoding, Field, value);
			case HeaderId.DispositionNotificationOptions:
				return EncodeDispositionNotificationOptions (Options, format, encoding, Field, value);
			case HeaderId.ArcAuthenticationResults:
			case HeaderId.AuthenticationResults:
				return EncodeAuthenticationResultsHeader (Options, format, encoding, Field, value);
			case HeaderId.ArcMessageSignature:
			case HeaderId.ArcSeal:
			case HeaderId.DkimSignature:
				return EncodeDkimOrArcSignatureHeader (Options, format, encoding, Field, value);
			case HeaderId.ListArchive:
			case HeaderId.ListHelp:
			case HeaderId.ListOwner:
			case HeaderId.ListPost:
			case HeaderId.ListSubscribe:
			case HeaderId.ListUnsubscribe:
				return EncodeMailingListCommandHeader (Options, format, encoding, Field, value);
			default:
				return EncodeUnstructuredHeader (Options, format, encoding, Field, value);
			}
		}

		internal byte[] GetRawValue (FormatOptions format)
		{
			if (format.International && !explicitRawValue) {
				switch (Id) {
				case HeaderId.DispositionNotificationTo:
				case HeaderId.ResentReplyTo:
				case HeaderId.ResentSender:
				case HeaderId.ResentFrom:
				case HeaderId.ResentBcc:
				case HeaderId.ResentCc:
				case HeaderId.ResentTo:
				case HeaderId.ReplyTo:
				case HeaderId.Sender:
				case HeaderId.From:
				case HeaderId.Bcc:
				case HeaderId.Cc:
				case HeaderId.To:
					return ReformatAddressHeader (Options, format, CharsetUtils.UTF8, Field, rawValue);
				case HeaderId.Received:
					// Note: Received headers should never be reformatted.
					return rawValue;
				case HeaderId.ResentMessageId:
				case HeaderId.InReplyTo:
				case HeaderId.MessageId:
				case HeaderId.ContentId:
					// Note: No text that can be internationalized.
					return rawValue;
				case HeaderId.References:
					// Note: No text that can be internationalized.
					return rawValue;
				case HeaderId.ContentDisposition:
					return ReformatContentDisposition (Options, format, CharsetUtils.UTF8, Field, rawValue);
				case HeaderId.ContentType:
					return ReformatContentType (Options, format, CharsetUtils.UTF8, Field, rawValue);
				case HeaderId.DispositionNotificationOptions:
					return rawValue;
				case HeaderId.ArcAuthenticationResults:
				case HeaderId.AuthenticationResults:
					// Note: No text that can be internationalized.
					return rawValue;
				case HeaderId.ArcMessageSignature:
				case HeaderId.ArcSeal:
				case HeaderId.DkimSignature:
					// TODO: Is there any value in reformatting this for internationalized text?
					return rawValue;
				case HeaderId.ListArchive:
				case HeaderId.ListHelp:
				case HeaderId.ListOwner:
				case HeaderId.ListPost:
				case HeaderId.ListSubscribe:
				case HeaderId.ListUnsubscribe:
					return EncodeMailingListCommandHeader (Options, format, CharsetUtils.UTF8, Field, Value);
				default:
					return EncodeUnstructuredHeader (Options, format, CharsetUtils.UTF8, Field, Value);
				}
			}

			return rawValue;
		}

		/// <summary>
		/// Ses the header value using the specified formatting options and character encoding.
		/// </summary>
		/// <remarks>
		/// When a particular charset is desired for encoding the header value
		/// according to the rules of rfc2047, this method should be used
		/// instead of the <see cref="Value"/> setter.
		/// </remarks>
		/// <param name="format">The formatting options.</param>
		/// <param name="encoding">A character encoding.</param>
		/// <param name="value">The header value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="format"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="encoding"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		public void SetValue (FormatOptions format, Encoding encoding, string value)
		{
			if (format is null)
				throw new ArgumentNullException (nameof (format));

			if (encoding is null)
				throw new ArgumentNullException (nameof (encoding));

			if (value is null)
				throw new ArgumentNullException (nameof (value));

			textValue = Unfold (value.Trim ());

			rawValue = FormatRawValue (format, encoding, textValue);
			explicitRawValue = false;

			// cache the formatting options that change the way the header is formatted
			//allowMixedHeaderCharsets = format.AllowMixedHeaderCharsets;
			//newLineFormat = format.NewLineFormat;
			//international = format.International;
			//charset = encoding;

			OnChanged ();
		}

		/// <summary>
		/// Set the header value using the specified character encoding.
		/// </summary>
		/// <remarks>
		/// When a particular charset is desired for encoding the header value
		/// according to the rules of rfc2047, this method should be used
		/// instead of the <see cref="Value"/> setter.
		/// </remarks>
		/// <param name="encoding">A character encoding.</param>
		/// <param name="value">The header value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="encoding"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		public void SetValue (Encoding encoding, string value)
		{
			SetValue (FormatOptions.Default, encoding, value);
		}

		/// <summary>
		/// Set the header value using the specified formatting options and charset.
		/// </summary>
		/// <remarks>
		/// When a particular charset is desired for encoding the header value
		/// according to the rules of rfc2047, this method should be used
		/// instead of the <see cref="Value"/> setter.
		/// </remarks>
		/// <param name="format">The formatting options.</param>
		/// <param name="charset">A charset encoding.</param>
		/// <param name="value">The header value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="format"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="charset"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <paramref name="charset"/> is not supported.
		/// </exception>
		public void SetValue (FormatOptions format, string charset, string value)
		{
			if (format is null)
				throw new ArgumentNullException (nameof (format));

			if (charset is null)
				throw new ArgumentNullException (nameof (charset));

			var encoding = CharsetUtils.GetEncoding (charset);

			SetValue (format, encoding, value);
		}

		/// <summary>
		/// Set the header value using the specified charset.
		/// </summary>
		/// <remarks>
		/// When a particular charset is desired for encoding the header value
		/// according to the rules of rfc2047, this method should be used
		/// instead of the <see cref="Value"/> setter.
		/// </remarks>
		/// <param name="charset">A charset encoding.</param>
		/// <param name="value">The header value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="charset"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <paramref name="charset"/> is not supported.
		/// </exception>
		public void SetValue (string charset, string value)
		{
			if (charset is null)
				throw new ArgumentNullException (nameof (charset));

			var encoding = CharsetUtils.GetEncoding (charset);

			SetValue (FormatOptions.Default, encoding, value);
		}

		/// <summary>
		/// Set the raw header value.
		/// </summary>
		/// <remarks>
		/// <para>Sets the raw header value.</para>
		/// <para>This method can be used to override default encoding and folding behavior
		/// for a particular header.</para>
		/// </remarks>
		/// <param name="value">The raw header value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="value"/> does not end with a new-line character.
		/// </exception>
		public void SetRawValue (byte[] value)
		{
			if (value is null)
				throw new ArgumentNullException (nameof (value));

			if (value.Length == 0 || value[value.Length - 1] != (byte) '\n')
				throw new ArgumentException ("The raw value MUST end with a new-line character.", nameof (value));

			explicitRawValue = true;
			rawValue = value;
			textValue = null;

			OnChanged ();
		}

		internal event EventHandler Changed;

		void OnChanged ()
		{
			Changed?.Invoke (this, EventArgs.Empty);
		}

		/// <summary>
		/// Return a string representation of the header.
		/// </summary>
		/// <remarks>
		/// Formats the header field and value in a way that is suitable for display.
		/// </remarks>
		/// <returns>A string representing the <see cref="Header"/>.</returns>
		public override string ToString ()
		{
			return IsInvalid ? Field : Field + ": " + Value;
		}

		/// <summary>
		/// Unfold the specified header value.
		/// </summary>
		/// <remarks>
		/// Unfolds the header value so that it becomes suitable for display.
		/// Since <see cref="Value"/> is already unfolded, this method is really
		/// only needed when working with raw header strings.
		/// </remarks>
		/// <returns>The unfolded header value.</returns>
		/// <param name="text">The header text.</param>
#if NET5_0_OR_GREATER
		[System.Runtime.CompilerServices.SkipLocalsInit]
#endif
		public static string Unfold (string text)
		{
			int startIndex;
			int endIndex;
			int i = 0;

			if (text is null)
				return string.Empty;

			while (i < text.Length && char.IsWhiteSpace (text[i]))
				i++;

			if (i == text.Length)
				return string.Empty;

			startIndex = i;
			endIndex = i;

			while (i < text.Length) {
				if (!char.IsWhiteSpace (text[i++]))
					endIndex = i;
			}

			int count = endIndex - startIndex;
			Span<char> chars = count <= 32
				? stackalloc char[32]
				: new char[count];

			for (i = startIndex, count = 0; i < endIndex; i++) {
				if (text[i] != '\r' && text[i] != '\n')
					chars[count++] = text[i];
			}

			return chars.Slice (0, count).ToString ();
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static bool IsFieldText (byte c)
		{
			return c.IsFieldText ();
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static bool IsBlank (byte c)
		{
			return c.IsBlank ();
		}

		internal static unsafe bool TryParse (ParserOptions options, byte* input, int length, bool strict, out Header header)
		{
			byte* inend = input + length;
			byte* start = input;
			byte* inptr = input;
			var invalid = false;

			// find the end of the field name
			while (inptr < inend && IsFieldText (*inptr))
				inptr++;

			while (inptr < inend && IsBlank (*inptr))
				inptr++;

			if (inptr == inend || *inptr != ':') {
				if (strict) {
					header = null;
					return false;
				}

				// mark the header as invalid and consume the entire input as the 'field'
				invalid = true;
				inptr = inend;
			}

			var field = new byte[(int) (inptr - start)];
			fixed (byte* outbuf = field) {
				byte* outptr = outbuf;

				while (start < inptr)
					*outptr++ = *start++;
			}

			byte[] value;

			if (inptr < inend) {
				// skip over the ':'
				inptr++;

				int count = (int) (inend - inptr);

				// When in strict mode (aka when called from any of the public Parse/TryParse APIs), force the value to be canonicalized by ending with a new-line sequence.
				if (strict && inend[-1] != (byte) '\n')
					count += FormatOptions.Default.NewLine.Length;

				value = new byte[count];

				fixed (byte* outbuf = value) {
					byte* outptr = outbuf;

					while (inptr < inend)
						*outptr++ = *inptr++;

					// When in strict mode (aka when called from any of the public Parse/TryParse APIs), force the value to be canonicalized by ending with a new-line sequence.
					// See https://github.com/jstedfast/MimeKit/issues/695 for more information.
					if (strict && inend[-1] != (byte) '\n') {
						var newLine = FormatOptions.Default.NewLineBytes;

						for (int i = 0; i < newLine.Length; i++)
							*outptr++ = newLine[i];
					}
				}
			} else {
				// Note: The only way to get here is if we have an invalid header, in which case the entire 'header' is stored as the 'field'.
				value = Array.Empty<byte> ();
			}

			header = new Header (options, field, value, invalid);

			return true;
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="Header"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a header from the supplied buffer starting at the given index
		/// and spanning across the specified number of bytes.
		/// </remarks>
		/// <returns><c>true</c>, if the header was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="header">The parsed header.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static bool TryParse (ParserOptions options, byte[] buffer, int startIndex, int length, out Header header)
		{
			ParseUtils.ValidateArguments (options, buffer, startIndex, length);

			unsafe {
				fixed (byte* inptr = buffer) {
					return TryParse (options.Clone (), inptr + startIndex, length, true, out header);
				}
			}
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="Header"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a header from the supplied buffer starting at the given index
		/// and spanning across the specified number of bytes.
		/// </remarks>
		/// <returns><c>true</c>, if the header was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="header">The parsed header.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static bool TryParse (byte[] buffer, int startIndex, int length, out Header header)
		{
			return TryParse (ParserOptions.Default, buffer, startIndex, length, out header);
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="Header"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a header from the supplied buffer starting at the specified index.
		/// </remarks>
		/// <returns><c>true</c>, if the header was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="header">The parsed header.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> is out of range.
		/// </exception>
		public static bool TryParse (ParserOptions options, byte[] buffer, int startIndex, out Header header)
		{
			ParseUtils.ValidateArguments (options, buffer, startIndex);

			int length = buffer.Length - startIndex;

			unsafe {
				fixed (byte* inptr = buffer) {
					return TryParse (options.Clone (), inptr + startIndex, length, true, out header);
				}
			}
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="Header"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a header from the supplied buffer starting at the specified index.
		/// </remarks>
		/// <returns><c>true</c>, if the header was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="header">The parsed header.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> is out of range.
		/// </exception>
		public static bool TryParse (byte[] buffer, int startIndex, out Header header)
		{
			return TryParse (ParserOptions.Default, buffer, startIndex, out header);
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="Header"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a header from the specified buffer.
		/// </remarks>
		/// <returns><c>true</c>, if the header was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="header">The parsed header.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		public static bool TryParse (ParserOptions options, byte[] buffer, out Header header)
		{
			return TryParse (options, buffer, 0, out header);
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="Header"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a header from the specified buffer.
		/// </remarks>
		/// <returns><c>true</c>, if the header was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="header">The parsed header.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		public static bool TryParse (byte[] buffer, out Header header)
		{
			return TryParse (ParserOptions.Default, buffer, out header);
		}

		/// <summary>
		/// Try to parse the given text into a new <see cref="Header"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a header from the specified text.
		/// </remarks>
		/// <returns><c>true</c>, if the header was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="text">The text to parse.</param>
		/// <param name="header">The parsed header.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="text"/> is <c>null</c>.</para>
		/// </exception>
		public static bool TryParse (ParserOptions options, string text, out Header header)
		{
			ParseUtils.ValidateArguments (options, text);

			var buffer = Encoding.UTF8.GetBytes (text);

			unsafe {
				fixed (byte *inptr = buffer) {
					return TryParse (options.Clone (), inptr, buffer.Length, true, out header);
				}
			}
		}

		/// <summary>
		/// Try to parse the given text into a new <see cref="Header"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a header from the specified text.
		/// </remarks>
		/// <returns><c>true</c>, if the header was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="text">The text to parse.</param>
		/// <param name="header">The parsed header.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public static bool TryParse (string text, out Header header)
		{
			return TryParse (ParserOptions.Default, text, out header);
		}
	}
}

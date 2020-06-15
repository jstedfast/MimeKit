//
// Header.cs
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
			if (encoding == null)
				throw new ArgumentNullException (nameof (encoding));

			if (id == HeaderId.Unknown)
				throw new ArgumentOutOfRangeException (nameof (id));

			if (value == null)
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
			if (charset == null)
				throw new ArgumentNullException (nameof (charset));

			if (id == HeaderId.Unknown)
				throw new ArgumentOutOfRangeException (nameof (id));

			if (value == null)
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
			if (encoding == null)
				throw new ArgumentNullException (nameof (encoding));

			if (field == null)
				throw new ArgumentNullException (nameof (field));

			if (field.Length == 0)
				throw new ArgumentException ("Header field names are not allowed to be empty.", nameof (field));

			for (int i = 0; i < field.Length; i++) {
				if (field[i] >= 127 || !IsAsciiAtom ((byte) field[i]))
					throw new ArgumentException ("Illegal characters in header field name.", nameof (field));
			}

			if (value == null)
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
			if (charset == null)
				throw new ArgumentNullException (nameof (charset));

			if (field == null)
				throw new ArgumentNullException (nameof (field));

			if (field.Length == 0)
				throw new ArgumentException ("Header field names are not allowed to be empty.", nameof (field));

			for (int i = 0; i < field.Length; i++) {
				if (field[i] >= 127 || !IsAsciiAtom ((byte) field[i]))
					throw new ArgumentException ("Illegal characters in header field name.", nameof (field));
			}

			if (value == null)
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
		/// <param name="value">The raw value of the header.</param>
		/// <param name="invalid"><c>true</c> if the header field is invalid; othereise, <c>false</c>.</param>
		internal protected Header (ParserOptions options, byte[] field, byte[] value, bool invalid)
		{
			var chars = new char[field.Length];
			int count = 0;

			while (count < field.Length && (invalid || !field[count].IsBlank ())) {
				chars[count] = (char) field[count];
				count++;
			}

			Options = options;
			rawField = field;
			rawValue = value;

			Field = new string (chars, 0, count);
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
			var header = new Header (Options, Id, Field, rawField, rawValue) {
				explicitRawValue = explicitRawValue,
				IsInvalid = IsInvalid
			};

			// if the textValue has already been calculated, set it on the cloned header as well.
			header.textValue = textValue;

			return header;
		}

		/// <summary>
		/// Gets the stream offset of the beginning of the header.
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
		/// Gets the name of the header field.
		/// </summary>
		/// <remarks>
		/// Represents the field name of the header.
		/// </remarks>
		/// <value>The name of the header field.</value>
		public string Field {
			get; private set;
		}

		/// <summary>
		/// Gets the header identifier.
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
		/// Gets the raw field name of the header.
		/// </summary>
		/// <remarks>
		/// Contains the raw field name of the header.
		/// </remarks>
		/// <value>The raw field name of the header.</value>
		public byte[] RawField {
			get { return rawField; }
		}

		/// <summary>
		/// Gets the raw value of the header.
		/// </summary>
		/// <remarks>
		/// Contains the raw value of the header, before any decoding or charset conversion.
		/// </remarks>
		/// <value>The raw value of the header.</value>
		public byte[] RawValue {
			get { return rawValue; }
		}

		/// <summary>
		/// Gets or sets the header value.
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
				if (textValue == null)
					textValue = Unfold (Rfc2047.DecodeText (Options, rawValue));

				return textValue;
			}
			set {
				SetValue (FormatOptions.Default, Encoding.UTF8, value);
			}
		}

		/// <summary>
		/// Gets the header value using the specified character encoding.
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
			if (encoding == null)
				throw new ArgumentNullException (nameof (encoding));

			var options = Options.Clone ();
			options.CharsetEncoding = encoding;

			return Unfold (Rfc2047.DecodeText (options, rawValue));
		}

		/// <summary>
		/// Gets the header value using the specified charset.
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
			if (charset == null)
				throw new ArgumentNullException (nameof (charset));

			var encoding = CharsetUtils.GetEncoding (charset);

			return GetValue (encoding);
		}

		static byte[] EncodeAddressHeader (ParserOptions options, FormatOptions format, Encoding encoding, string field, string value)
		{
			var encoded = new StringBuilder (" ");
			int lineLength = field.Length + 2;
			InternetAddressList list;

			if (!InternetAddressList.TryParse (options, value, out list))
				return (byte[]) format.NewLineBytes.Clone ();

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

		static readonly byte[] ReceivedAddrSpecSentinels = { (byte) '>', (byte) ';' };
		static readonly byte[] ReceivedMessageIdSentinels = { (byte) '>' };

		static void ReceivedTokenSkipAddress (byte[] text, ref int index)
		{
			string addrspec;
			int at;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, text.Length, false) || index >= text.Length)
				return;

			if (text[index] == (byte) '<')
				index++;

			InternetAddress.TryParseAddrspec (text, ref index, text.Length, ReceivedAddrSpecSentinels, false, out addrspec, out at);

			if (index < text.Length && text[index] == (byte) '>')
				index++;
		}

		static void ReceivedTokenSkipMessageId (byte[] text, ref int index)
		{
			string addrspec;
			int at;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, text.Length, false) || index >= text.Length)
				return;

			if (text[index] == (byte) '<') {
				index++;

				InternetAddress.TryParseAddrspec (text, ref index, text.Length, ReceivedMessageIdSentinels, false, out addrspec, out at);

				if (index < text.Length && text[index] == (byte) '>')
					index++;
			} else {
				ParseUtils.SkipAtom (text, ref index, text.Length);
			}
		}

		struct ReceivedToken {
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
			var encoded = new StringBuilder ();
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

				if (token == null) {
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

		static void EncodeDkimLongValue (FormatOptions format, StringBuilder encoded, ref int lineLength, string value)
		{
			int startIndex = 0;

			do {
				int lineLeft = format.MaxLineLength - lineLength;
				int index = Math.Min (startIndex + lineLeft, value.Length);

				encoded.Append (value.Substring (startIndex, index - startIndex));
				lineLength += (index - startIndex);

				if (index == value.Length)
					break;

				encoded.Append (format.NewLine);
				encoded.Append ('\t');
				lineLength = 1;

				startIndex = index;
			} while (true);
		}

		static void EncodeDkimHeaderList (FormatOptions format, StringBuilder encoded, ref int lineLength, string value, char delim)
		{
			var tokens = value.Split (delim);

			for (int i = 0; i < tokens.Length; i++) {
				if (i > 0) {
					encoded.Append (delim);
					lineLength++;
				}

				if (lineLength + tokens[i].Length + 1 > format.MaxLineLength) {
					encoded.Append (format.NewLine);
					encoded.Append ('\t');
					lineLength = 1;

					if (tokens[i].Length + 1 > format.MaxLineLength) {
						EncodeDkimLongValue (format, encoded, ref lineLength, tokens[i]);
					} else {
						lineLength += tokens[i].Length;
						encoded.Append (tokens[i]);
					}
				} else {
					lineLength += tokens[i].Length;
					encoded.Append (tokens[i]);
				}
			}
		}

		static byte[] EncodeDkimOrArcSignatureHeader (ParserOptions options, FormatOptions format, Encoding encoding, string field, string value)
		{
			var encoded = new StringBuilder ();
			int lineLength = field.Length + 1;
			var token = new StringBuilder ();
			int index = 0;

			while (index < value.Length) {
				while (index < value.Length && IsWhiteSpace (value[index]))
					index++;

				int startIndex = index;
				string name;

				while (index < value.Length && value[index] != '=') {
					if (!IsWhiteSpace (value[index]))
						token.Append (value[index]);
					index++;
				}

				name = value.Substring (startIndex, index - startIndex);

				while (index < value.Length && value[index] != ';') {
					if (!IsWhiteSpace (value[index]))
						token.Append (value[index]);
					index++;
				}

				if (index < value.Length && value[index] == ';') {
					token.Append (';');
					index++;
				}

				if (lineLength + token.Length + 1 > format.MaxLineLength || name == "bh" || name == "b") {
					encoded.Append (format.NewLine);
					encoded.Append ('\t');
					lineLength = 1;
				} else {
					encoded.Append (' ');
					lineLength++;
				}

				if (token.Length > format.MaxLineLength) {
					switch (name) {
					case "z":
						EncodeDkimHeaderList (format, encoded, ref lineLength, token.ToString (), '|');
						break;
					case "h":
						EncodeDkimHeaderList (format, encoded, ref lineLength, token.ToString (), ':');
						break;
					default:
						EncodeDkimLongValue (format, encoded, ref lineLength, token.ToString ());
						break;
					}
				} else {
					encoded.Append (token.ToString ());
					lineLength += token.Length;
				}

				token.Length = 0;
			}

			encoded.Append (format.NewLine);

			return encoding.GetBytes (encoded.ToString ());
		}

		static byte[] EncodeReferencesHeader (ParserOptions options, FormatOptions format, Encoding encoding, string field, string value)
		{
			var encoded = new StringBuilder ();
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

				encoded.Append ('<').Append (reference).Append ('>');
				lineLength += reference.Length + 2;
				count++;
			}

			encoded.Append (format.NewLine);

			return encoding.GetBytes (encoded.ToString ());
		}

		static bool IsWhiteSpace (char c)
		{
			return c == ' ' || c == '\t' || c == '\r' || c == '\n';
		}

		static IEnumerable<string> TokenizeText (string text)
		{
			int index = 0;

			while (index < text.Length) {
				int startIndex = index;

				while (index < text.Length && !IsWhiteSpace (text[index]))
					index++;

				yield return text.Substring (startIndex, index - startIndex);

				if (index == text.Length)
					break;

				startIndex = index;

				while (index < text.Length && IsWhiteSpace (text[index]))
					index++;

				yield return text.Substring (startIndex, index - startIndex);
			}

			yield break;
		}

		class BrokenWord
		{
			public readonly char[] Text;
			public readonly int StartIndex;
			public readonly int Length;

			public BrokenWord (char[] text, int startIndex, int length)
			{
				StartIndex = startIndex;
				Length = length;
				Text = text;
			}
		}

		static IEnumerable<BrokenWord> WordBreak (FormatOptions format, string word, int lineLength)
		{
			var chars = word.ToCharArray ();
			int startIndex = 0;

			lineLength = Math.Max (lineLength, 1);

			while (startIndex < word.Length) {
				int length = Math.Min (format.MaxLineLength - lineLength, word.Length - startIndex);

				if (char.IsSurrogatePair (word, startIndex + length - 1))
					length--;

				yield return new BrokenWord (chars, startIndex, length);

				startIndex += length;
				lineLength = 1;
			}

			yield break;
		}

		internal static string Fold (FormatOptions format, string field, string value)
		{
			var folded = new StringBuilder (value.Length);
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
						folded.Append (word);
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
					foreach (var broken in WordBreak (format, word, lineLength)) {
						if (lineLength + broken.Length > format.MaxLineLength) {
							folded.Append (format.NewLine);
							folded.Append (' ');
							lineLength = 1;
						}

						folded.Append (broken.Text, broken.StartIndex, broken.Length);
						lineLength += broken.Length;
					}
				} else {
					lineLength += word.Length;
					folded.Append (word);
				}
			}

			folded.Append (format.NewLine);

			return folded.ToString ();
		}

		static byte[] EncodeContentDisposition (ParserOptions options, FormatOptions format, Encoding encoding, string field, string value)
		{
			var disposition = ContentDisposition.Parse (options, value);
			var encoded = disposition.Encode (format, encoding);

			return Encoding.UTF8.GetBytes (encoded);
		}

		static byte[] EncodeContentType (ParserOptions options, FormatOptions format, Encoding encoding, string field, string value)
		{
			var contentType = ContentType.Parse (options, value);
			var encoded = contentType.Encode (format, encoding);

			return Encoding.UTF8.GetBytes (encoded);
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
		/// This method will called by the <a href="Overload_MimeKit_Header_SetValue.htm">SetValue</a>
		/// methods and may also be conditionally called when the header is being written to a
		/// <see cref="System.IO.Stream"/>.
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
			case HeaderId.ArcAuthenticationResults:
			case HeaderId.AuthenticationResults:
				return EncodeAuthenticationResultsHeader (Options, format, encoding, Field, value);
			case HeaderId.ArcMessageSignature:
			case HeaderId.ArcSeal:
			case HeaderId.DkimSignature:
				return EncodeDkimOrArcSignatureHeader (Options, format, encoding, Field, value);
			default:
				return EncodeUnstructuredHeader (Options, format, encoding, Field, value);
			}
		}

		internal byte[] GetRawValue (FormatOptions format)
		{
			if (format.International && !explicitRawValue) {
				if (textValue == null)
					textValue = Unfold (Rfc2047.DecodeText (Options, rawValue));

				// Note: if we're reformatting to be International, then charset doesn't matter.
				return FormatRawValue (format, CharsetUtils.UTF8, textValue);
			}

			return rawValue;
		}

		/// <summary>
		/// Sets the header value using the specified formatting options and character encoding.
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
			if (format == null)
				throw new ArgumentNullException (nameof (format));

			if (encoding == null)
				throw new ArgumentNullException (nameof (encoding));

			if (value == null)
				throw new ArgumentNullException (nameof (value));

			textValue = Unfold (value.Trim ());

			rawValue = FormatRawValue (format, encoding, textValue);

			// cache the formatting options that change the way the header is formatted
			//allowMixedHeaderCharsets = format.AllowMixedHeaderCharsets;
			//newLineFormat = format.NewLineFormat;
			//international = format.International;
			//charset = encoding;

			OnChanged ();
		}

		/// <summary>
		/// Sets the header value using the specified character encoding.
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
		/// Sets the header value using the specified formatting options and charset.
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
			if (format == null)
				throw new ArgumentNullException (nameof (format));

			if (charset == null)
				throw new ArgumentNullException (nameof (charset));

			var encoding = CharsetUtils.GetEncoding (charset);

			SetValue (format, encoding, value);
		}

		/// <summary>
		/// Sets the header value using the specified charset.
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
			if (charset == null)
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
			if (value == null)
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
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}

		/// <summary>
		/// Returns a string representation of the header.
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
		public static unsafe string Unfold (string text)
		{
			int startIndex;
			int endIndex;
			int i = 0;

			if (text == null)
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
			char[] chars = new char[count];

			fixed (char* outbuf = chars) {
				char* outptr = outbuf;

				for (i = startIndex; i < endIndex; i++) {
					if (text[i] != '\r' && text[i] != '\n')
						*outptr++ = text[i];
				}

				count = (int) (outptr - outbuf);
			}

			return new string (chars, 0, count);
		}

		static bool IsAsciiAtom (byte c)
		{
			return c.IsAsciiAtom ();
		}

		static bool IsControl (byte c)
		{
			return c.IsCtrl ();
		}

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
			if (strict) {
				while (inptr < inend && IsAsciiAtom (*inptr))
					inptr++;
			} else {
				while (inptr < inend && *inptr != (byte) ':' && !IsControl (*inptr))
					inptr++;
			}

			while (inptr < inend && IsBlank (*inptr))
				inptr++;

			if (inptr == inend || *inptr != ':') {
				if (strict) {
					header = null;
					return false;
				}

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
				inptr++;

				int count = (int) (inend - inptr);
				value = new byte[count];

				fixed (byte* outbuf = value) {
					byte* outptr = outbuf;

					while (inptr < inend)
						*outptr++ = *inptr++;
				}
			} else {
				value = new byte[0];
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

//
// Header.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2014 Xamarin Inc.
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

#if PORTABLE
using Encoding = Portable.Text.Encoding;
#endif

using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// A class representing a Message or MIME header.
	/// </summary>
	/// <remarks>
	/// Represents a single header field and value pair.
	/// </remarks>
	public class Header
	{
		internal readonly ParserOptions Options;
		string textValue;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Header"/> class.
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
		public Header (Encoding charset, HeaderId id, string value)
		{
			if (charset == null)
				throw new ArgumentNullException ("charset");

			if (id == HeaderId.Unknown)
				throw new ArgumentOutOfRangeException ("id");

			if (value == null)
				throw new ArgumentNullException ("value");

			Options = ParserOptions.Default.Clone ();
			Field = id.ToHeaderName ();
			Id = id;

			SetValue (charset, value);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Header"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new message or entity header for the specified field and
		/// value pair.
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
		/// Initializes a new instance of the <see cref="MimeKit.Header"/> class.
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
		public Header (Encoding charset, string field, string value)
		{
			if (charset == null)
				throw new ArgumentNullException ("charset");

			if (field == null)
				throw new ArgumentNullException ("field");

			if (field.Length == 0)
				throw new ArgumentException ("Header field names are not allowed to be empty.", "field");

			for (int i = 0; i < field.Length; i++) {
				if (field[i] > 127 || !IsAtom ((byte) field[i]))
					throw new ArgumentException ("Illegal characters in header field name.", "field");
			}

			if (value == null)
				throw new ArgumentNullException ("value");

			Options = ParserOptions.Default.Clone ();
			Id = field.ToHeaderId ();
			Field = field;

			SetValue (charset, value);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Header"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new message or entity header for the specified field and
		/// value pair.
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

		// Note: this ctor is only used by the parser
		internal Header (ParserOptions options, string field, byte[] value)
		{
			Id = field.ToHeaderId ();
			Options = options;
			RawValue = value;
			Field = field;
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

		/// <summary>
		/// Gets the raw value of the header.
		/// </summary>
		/// <remarks>
		/// Contains the raw value of the header, before any decoding or charset conversion.
		/// </remarks>
		/// <value>The raw value of the header.</value>
		public byte[] RawValue {
			get; internal set;
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
					textValue = Unfold (Rfc2047.DecodeText (Options, RawValue));

				return textValue;
			}
			set {
				SetValue (Encoding.UTF8, value);
			}
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
		/// <param name="charset">Charset.</param>
		public string GetValue (Encoding charset)
		{
			if (charset == null)
				throw new ArgumentNullException ("charset");

			var options = Options.Clone ();
			options.CharsetEncoding = charset;

			return Unfold (Rfc2047.DecodeText (options, RawValue));
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
		public void SetValue (Encoding charset, string value)
		{
			if (charset == null)
				throw new ArgumentNullException ("charset");

			if (value == null)
				throw new ArgumentNullException ("value");

			textValue = Unfold (value.Trim ());

			if (Id != HeaderId.ContentId && Id != HeaderId.MessageId && Id != HeaderId.ResentMessageId) {
				var encoded = Rfc2047.EncodeText (FormatOptions.Default, charset, textValue);

				RawValue = Rfc2047.FoldUnstructuredHeader (FormatOptions.Default, Field, encoded);
			} else {
				RawValue = charset.GetBytes (" " + textValue);
			}

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
			return Field + ": " + Value;
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

		static bool IsAtom (byte c)
		{
			return c.IsAtom ();
		}

		static bool IsBlankOrControl (byte c)
		{
			return c.IsType (CharType.IsBlank | CharType.IsControl);
		}

		internal static unsafe bool TryParse (ParserOptions options, byte* input, int length, bool strict, out Header header)
		{
			byte* inend = input + length;
			byte* start = input;
			byte* inptr = input;

			// find the end of the field name
			if (strict) {
				while (inptr < inend && IsAtom (*inptr))
					inptr++;
			} else {
				while (inptr < inend && *inptr != (byte) ':' && !IsBlankOrControl (*inptr))
					inptr++;
			}

			if (inptr == inend || *inptr != ':') {
				header = null;
				return false;
			}

			var chars = new char[(int) (inptr - start)];
			fixed (char* outbuf = chars) {
				char* outptr = outbuf;

				while (start < inptr)
					*outptr++ = (char) *start++;
			}

			var field = new string (chars);

			inptr++;

			int count = (int) (inend - inptr);
			var value = new byte[count];

			fixed (byte *outbuf = value) {
				byte* outptr = outbuf;

				while (inptr < inend)
					*outptr++ = *inptr++;
			}

			header = new Header (options, field, value);

			return true;
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.Header"/> instance.
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
			if (options == null)
				throw new ArgumentNullException ("options");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (length < 0 || length > (buffer.Length - startIndex))
				throw new ArgumentOutOfRangeException ("length");

			unsafe {
				fixed (byte* inptr = buffer) {
					return TryParse (options.Clone (), inptr + startIndex, length, true, out header);
				}
			}
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.Header"/> instance.
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
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.Header"/> instance.
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
			int length = buffer.Length - startIndex;

			return TryParse (options, buffer, startIndex, length, out header);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.Header"/> instance.
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
			int length = buffer.Length - startIndex;

			return TryParse (ParserOptions.Default, buffer, startIndex, length, out header);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.Header"/> instance.
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
			return TryParse (options, buffer, 0, buffer.Length, out header);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.Header"/> instance.
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
			return TryParse (ParserOptions.Default, buffer, 0, buffer.Length, out header);
		}

		/// <summary>
		/// Tries to parse the given text into a new <see cref="MimeKit.Header"/> instance.
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
			if (options == null)
				throw new ArgumentNullException ("options");

			if (text == null)
				throw new ArgumentNullException ("text");

			var buffer = Encoding.UTF8.GetBytes (text);

			unsafe {
				fixed (byte *inptr = buffer) {
					return TryParse (options.Clone (), inptr, buffer.Length, true, out header);
				}
			}
		}

		/// <summary>
		/// Tries to parse the given text into a new <see cref="MimeKit.Header"/> instance.
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

﻿//
// ContentType.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 Xamarin Inc. (www.xamarin.com)
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

using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// A class representing a Content-Type header value.
	/// </summary>
	/// <remarks>
	/// The Content-Type header is a way for the originating client to
	/// suggest to the receiving client the mime-type of the content and,
	/// depending on that mime-type, presentation options such as charset.
	/// </remarks>
	public class ContentType
	{
		ParameterList parameters;
		string type, subtype;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.ContentType"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="ContentType"/> based on the media type and subtype provided.
		/// </remarks>
		/// <param name="mediaType">Media type.</param>
		/// <param name="mediaSubtype">Media subtype.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="mediaType"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="mediaSubtype"/> is <c>null</c>.</para>
		/// </exception>
		public ContentType (string mediaType, string mediaSubtype)
		{
			if (mediaType == null)
				throw new ArgumentNullException (nameof (mediaType));

			if (mediaSubtype == null)
				throw new ArgumentNullException (nameof (mediaSubtype));

			Parameters = new ParameterList ();
			subtype = mediaSubtype;
			type = mediaType;
		}

		/// <summary>
		/// Get or set the type of the media.
		/// </summary>
		/// <remarks>
		/// Represents the media type of the <see cref="MimeEntity"/>. Examples include
		/// <c>"text"</c>, <c>"image"</c>, and <c>"application"</c>. This string should
		/// always be treated as case-insensitive.
		/// </remarks>
		/// <value>The type of the media.</value>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		public string MediaType {
			get { return type; }
			set {
				if (value == null)
					throw new ArgumentNullException (nameof (value));

				if (type == value)
					return;

				type = value;

				OnChanged ();
			}
		}

		/// <summary>
		/// Get or set the media subtype.
		/// </summary>
		/// <remarks>
		/// Represents the media subtype of the <see cref="MimeEntity"/>. Examples include
		/// <c>"html"</c>, <c>"jpeg"</c>, and <c>"octet-stream"</c>. This string should
		/// always be treated as case-insensitive.
		/// </remarks>
		/// <value>The media subtype.</value>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		public string MediaSubtype {
			get { return subtype; }
			set {
				if (value == null)
					throw new ArgumentNullException (nameof (value));

				if (subtype == value)
					return;

				subtype = value;

				OnChanged ();
			}
		}

		/// <summary>
		/// Get the parameters.
		/// </summary>
		/// <remarks>
		/// In addition to the media type and subtype, the Content-Type header may also
		/// contain parameters to provide further hints to the receiving client as to
		/// how to process or display the content.
		/// </remarks>
		/// <value>The parameters.</value>
		public ParameterList Parameters {
			get { return parameters; }
			internal set {
				if (parameters != null)
					parameters.Changed -= OnParametersChanged;

				value.Changed += OnParametersChanged;
				parameters = value;
			}
		}

		/// <summary>
		/// Get or set the boundary parameter.
		/// </summary>
		/// <remarks>
		/// This is a special parameter on <see cref="Multipart"/> entities, designating to the
		/// parser a unique string that should be considered the boundary marker for each sub-part.
		/// </remarks>
		/// <value>The boundary.</value>
		public string Boundary {
			get { return Parameters["boundary"]; }
			set {
				if (value != null)
					Parameters["boundary"] = value;
				else
					Parameters.Remove ("boundary");
			}
		}

		/// <summary>
		/// Get or set the charset parameter.
		/// </summary>
		/// <remarks>
		/// Text-based <see cref="MimePart"/> entities will often include a charset parameter
		/// so that the receiving client can properly render the text.
		/// </remarks>
		/// <value>The charset.</value>
		public string Charset {
			get { return Parameters["charset"]; }
			set {
				if (value != null)
					Parameters["charset"] = value;
				else
					Parameters.Remove ("charset");
			}
		}

		/// <summary>
		/// Get or set the charset parameter as an <see cref="Encoding"/>.
		/// </summary>
		/// <remarks>
		/// Text-based <see cref="MimePart"/> entities will often include a charset parameter
		/// so that the receiving client can properly render the text.
		/// </remarks>
		/// <value>The charset encoding.</value>
		public Encoding CharsetEncoding {
			get {
				var charset = Charset;

				if (charset == null)
					return null;

				try {
					return CharsetUtils.GetEncoding (charset);
				} catch {
					return null;
				}
			}
			set {
				Charset = value != null ? CharsetUtils.GetMimeCharset (value) : null;
			}
		}

		/// <summary>
		/// Get or set the format parameter.
		/// </summary>
		/// <remarks>
		/// The format parameter is typically use with text/plain <see cref="MimePart"/>
		/// entities and will either have a value of <c>"fixed"</c> or <c>"flowed"</c>.
		/// </remarks>
		/// <value>The charset.</value>
		public string Format {
			get { return Parameters["format"]; }
			set {
				if (value != null)
					Parameters["format"] = value;
				else
					Parameters.Remove ("format");
			}
		}

		/// <summary>
		/// Gets the simple mime-type.
		/// </summary>
		/// <remarks>
		/// Gets the simple mime-type.
		/// </remarks>
		/// <value>The mime-type.</value>
		public string MimeType {
			get { return string.Format ("{0}/{1}", MediaType, MediaSubtype); }
		}

		/// <summary>
		/// Get or set the name parameter.
		/// </summary>
		/// <remarks>
		/// The name parameter is a way for the originiating client to suggest
		/// to the receiving client a display-name for the content, which may
		/// be used by the receiving client if it cannot display the actual
		/// content to the user.
		/// </remarks>
		/// <value>The name.</value>
		public string Name {
			get { return Parameters["name"]; }
			set {
				if (value != null)
					Parameters["name"] = value;
				else
					Parameters.Remove ("name");
			}
		}

		/// <summary>
		/// Checks if the this instance of <see cref="MimeKit.ContentType"/> matches
		/// the specified MIME media type and subtype.
		/// </summary>
		/// <remarks>
		/// If the specified <paramref name="mediaType"/> or <paramref name="mediaSubtype"/>
		/// are <c>"*"</c>, they match anything.
		/// </remarks>
		/// <returns><c>true</c> if the <see cref="ContentType"/> matches the
		/// provided media type and subtype.</returns>
		/// <param name="mediaType">The media type.</param>
		/// <param name="mediaSubtype">The media subtype.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="mediaType"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="mediaSubtype"/> is <c>null</c>.</para>
		/// </exception>
		public bool IsMimeType (string mediaType, string mediaSubtype)
		{
			if (mediaType == null)
				throw new ArgumentNullException (nameof (mediaType));

			if (mediaSubtype == null)
				throw new ArgumentNullException (nameof (mediaSubtype));

			if (mediaType == "*" || mediaType.Equals (type, StringComparison.OrdinalIgnoreCase))
				return mediaSubtype == "*" || mediaSubtype.Equals (subtype, StringComparison.OrdinalIgnoreCase);

			return false;
		}

		internal string Encode (FormatOptions options, Encoding charset)
		{
			int lineLength = "Content-Type:".Length;
			var value = new StringBuilder (" ");

			value.Append (MediaType);
			value.Append ('/');
			value.Append (MediaSubtype);

			lineLength += value.Length;

			Parameters.Encode (options, value, ref lineLength, charset);
			value.Append (options.NewLine);

			return value.ToString ();
		}

		/// <summary>
		/// Serializes the <see cref="ContentType"/> to a string,
		/// optionally encoding the parameters.
		/// </summary>
		/// <remarks>
		/// Creates a string-representation of the <see cref="ContentType"/>, optionally encoding
		/// the parameters as they would be encoded for transport.
		/// </remarks>
		/// <returns>The serialized string.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="charset">The charset to be used when encoding the parameter values.</param>
		/// <param name="encode">If set to <c>true</c>, the parameter values will be encoded.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="charset"/> is <c>null</c>.</para>
		/// </exception>
		public string ToString (FormatOptions options, Encoding charset, bool encode)
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			if (charset == null)
				throw new ArgumentNullException (nameof (charset));

			var value = new StringBuilder ("Content-Type: ");
			value.Append (MediaType);
			value.Append ('/');
			value.Append (MediaSubtype);

			if (encode) {
				int lineLength = value.Length;

				Parameters.Encode (options, value, ref lineLength, charset);
			} else {
				value.Append (Parameters.ToString ());
			}

			return value.ToString ();
		}

		/// <summary>
		/// Serializes the <see cref="ContentType"/> to a string,
		/// optionally encoding the parameters.
		/// </summary>
		/// <remarks>
		/// Creates a string-representation of the <see cref="ContentType"/>, optionally encoding
		/// the parameters as they would be encoded for transport.
		/// </remarks>
		/// <returns>The serialized string.</returns>
		/// <param name="charset">The charset to be used when encoding the parameter values.</param>
		/// <param name="encode">If set to <c>true</c>, the parameter values will be encoded.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="charset"/> is <c>null</c>.
		/// </exception>
		public string ToString (Encoding charset, bool encode)
		{
			return ToString (FormatOptions.Default, charset, encode);
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current
		/// <see cref="MimeKit.ContentType"/>.
		/// </summary>
		/// <remarks>
		/// Creates a string-representation of the <see cref="ContentType"/>.
		/// </remarks>
		/// <returns>A <see cref="System.String"/> that represents the current
		/// <see cref="MimeKit.ContentType"/>.</returns>
		public override string ToString ()
		{
			return ToString (FormatOptions.Default, Encoding.UTF8, false);
		}

		internal event EventHandler Changed;

		void OnParametersChanged (object sender, EventArgs e)
		{
			OnChanged ();
		}

		void OnChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}

		static bool SkipType (byte[] text, ref int index, int endIndex)
		{
			int startIndex = index;

			while (index < endIndex && text[index].IsAsciiAtom () && text[index] != (byte) '/')
				index++;

			return index > startIndex;
		}

		internal static bool TryParse (ParserOptions options, byte[] text, ref int index, int endIndex, bool throwOnError, out ContentType contentType)
		{
			string type, subtype;
			int start;

			contentType = null;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			start = index;
			if (!SkipType (text, ref index, endIndex)) {
				if (throwOnError)
					throw new ParseException (string.Format ("Invalid type token at position {0}", start), start, index);

				return false;
			}

			type = Encoding.ASCII.GetString (text, start, index - start);

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			if (index >= endIndex || text[index] != (byte) '/') {
				if (throwOnError)
					throw new ParseException (string.Format ("Expected '/' at position {0}", index), index, index);

				return false;
			}

			// skip over the '/'
			index++;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			start = index;
			if (!ParseUtils.SkipToken (text, ref index, endIndex)) {
				if (throwOnError)
					throw new ParseException (string.Format ("Invalid atom token at position {0}", start), start, index);

				return false;
			}

			subtype = Encoding.ASCII.GetString (text, start, index - start);

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			contentType = new ContentType (type, subtype);

			if (index >= endIndex)
				return true;

			if (text[index] != (byte) ';') {
				if (throwOnError)
					throw new ParseException (string.Format ("Expected ';' at position {0}", index), index, index);

				return false;
			}

			index++;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			if (index >= endIndex)
				return true;

			ParameterList parameters;
			if (!ParameterList.TryParse (options, text, ref index, endIndex, throwOnError, out parameters))
				return false;

			contentType.Parameters = parameters;

			return true;
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.ContentType"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Type value from the supplied buffer starting at the given index
		/// and spanning across the specified number of bytes.
		/// </remarks>
		/// <returns><c>true</c> if the content type was successfully parsed; otherwise, <c>false</c>.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="type">The parsed content type.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static bool TryParse (ParserOptions options, byte[] buffer, int startIndex, int length, out ContentType type)
		{
			ParseUtils.ValidateArguments (options, buffer, startIndex, length);

			int index = startIndex;

			return TryParse (options, buffer, ref index, startIndex + length, false, out type);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.ContentType"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Type value from the supplied buffer starting at the given index
		/// and spanning across the specified number of bytes.
		/// </remarks>
		/// <returns><c>true</c> if the content type was successfully parsed; otherwise, <c>false</c>.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="type">The parsed content type.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static bool TryParse (byte[] buffer, int startIndex, int length, out ContentType type)
		{
			return TryParse (ParserOptions.Default, buffer, startIndex, length, out type);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.ContentType"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Type value from the supplied buffer starting at the specified index.
		/// </remarks>
		/// <returns><c>true</c> if the content type was successfully parsed; otherwise, <c>false</c>.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="type">The parsed content type.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> is out of range.
		/// </exception>
		public static bool TryParse (ParserOptions options, byte[] buffer, int startIndex, out ContentType type)
		{
			ParseUtils.ValidateArguments (options, buffer, startIndex);

			int index = startIndex;

			return TryParse (options, buffer, ref index, buffer.Length, false, out type);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.ContentType"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Type value from the supplied buffer starting at the specified index.
		/// </remarks>
		/// <returns><c>true</c> if the content type was successfully parsed; otherwise, <c>false</c>.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="type">The parsed content type.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> is out of range.
		/// </exception>
		public static bool TryParse (byte[] buffer, int startIndex, out ContentType type)
		{
			return TryParse (ParserOptions.Default, buffer, startIndex, out type);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.ContentType"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Type value from the specified buffer.
		/// </remarks>
		/// <returns><c>true</c> if the content type was successfully parsed; otherwise, <c>false</c>.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="type">The parsed content type.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		public static bool TryParse (ParserOptions options, byte[] buffer, out ContentType type)
		{
			ParseUtils.ValidateArguments (options, buffer);

			int index = 0;

			return TryParse (options, buffer, ref index, buffer.Length, false, out type);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.ContentType"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Type value from the specified buffer.
		/// </remarks>
		/// <returns><c>true</c> if the content type was successfully parsed; otherwise, <c>false</c>.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="type">The parsed content type.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		public static bool TryParse (byte[] buffer, out ContentType type)
		{
			return TryParse (ParserOptions.Default, buffer, out type);
		}

		/// <summary>
		/// Tries to parse the given text into a new <see cref="MimeKit.ContentType"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Type value from the specified text.
		/// </remarks>
		/// <returns><c>true</c> if the content type was successfully parsed; otherwise, <c>false</c>.</returns>
		/// <param name="options">THe parser options.</param>
		/// <param name="text">The text to parse.</param>
		/// <param name="type">The parsed content type.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="text"/> is <c>null</c>.</para>
		/// </exception>
		public static bool TryParse (ParserOptions options, string text, out ContentType type)
		{
			ParseUtils.ValidateArguments (options, text);

			var buffer = Encoding.UTF8.GetBytes (text);
			int index = 0;

			return TryParse (options, buffer, ref index, buffer.Length, false, out type);
		}

		/// <summary>
		/// Tries to parse the given text into a new <see cref="MimeKit.ContentType"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Type value from the specified text.
		/// </remarks>
		/// <returns><c>true</c> if the content type was successfully parsed; otherwise, <c>false</c>.</returns>
		/// <param name="text">The text to parse.</param>
		/// <param name="type">The parsed content type.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public static bool TryParse (string text, out ContentType type)
		{
			return TryParse (ParserOptions.Default, text, out type);
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="MimeKit.ContentType"/> class.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Type value from the supplied buffer starting at the given index
		/// and spanning across the specified number of bytes.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.ContentType"/>.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The start index of the buffer.</param>
		/// <param name="length">The length of the buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// The <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static ContentType Parse (ParserOptions options, byte[] buffer, int startIndex, int length)
		{
			ParseUtils.ValidateArguments (options, buffer, startIndex, length);

			int index = startIndex;
			ContentType type;

			TryParse (options, buffer, ref index, startIndex + length, true, out type);

			return type;
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="MimeKit.ContentType"/> class.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Type value from the supplied buffer starting at the given index
		/// and spanning across the specified number of bytes.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.ContentType"/>.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The start index of the buffer.</param>
		/// <param name="length">The length of the buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// The <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static ContentType Parse (byte[] buffer, int startIndex, int length)
		{
			return Parse (ParserOptions.Default, buffer, startIndex, length);
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="MimeKit.ContentType"/> class.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Type value from the supplied buffer starting at the specified index.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.ContentType"/>.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The start index of the buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> is out of range.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// The <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static ContentType Parse (ParserOptions options, byte[] buffer, int startIndex)
		{
			ParseUtils.ValidateArguments (options, buffer, startIndex);

			int index = startIndex;
			ContentType type;

			TryParse (options, buffer, ref index, buffer.Length, true, out type);

			return type;
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="MimeKit.ContentType"/> class.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Type value from the supplied buffer starting at the specified index.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.ContentType"/>.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The start index of the buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> is out of range.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// The <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static ContentType Parse (byte[] buffer, int startIndex)
		{
			return Parse (ParserOptions.Default, buffer, startIndex);
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="MimeKit.ContentType"/> class.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Type value from the specified buffer.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.ContentType"/>.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// The <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static ContentType Parse (ParserOptions options, byte[] buffer)
		{
			ParseUtils.ValidateArguments (options, buffer);

			ContentType type;
			int index = 0;

			TryParse (options, buffer, ref index, buffer.Length, true, out type);

			return type;
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="MimeKit.ContentType"/> class.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Type value from the specified buffer.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.ContentType"/>.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// The <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static ContentType Parse (byte[] buffer)
		{
			return Parse (ParserOptions.Default, buffer);
		}

		/// <summary>
		/// Parse the specified text into a new instance of the <see cref="MimeKit.ContentType"/> class.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Type value from the specified text.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.ContentType"/>.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="text">The text.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="text"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// The <paramref name="text"/> could not be parsed.
		/// </exception>
		public static ContentType Parse (ParserOptions options, string text)
		{
			ParseUtils.ValidateArguments (options, text);

			var buffer = Encoding.UTF8.GetBytes (text);
			ContentType type;
			int index = 0;

			TryParse (options, buffer, ref index, buffer.Length, true, out type);

			return type;
		}

		/// <summary>
		/// Parse the specified text into a new instance of the <see cref="MimeKit.ContentType"/> class.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Type value from the specified text.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.ContentType"/>.</returns>
		/// <param name="text">The text.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// The <paramref name="text"/> could not be parsed.
		/// </exception>
		public static ContentType Parse (string text)
		{
			return Parse (ParserOptions.Default, text);
		}
	}
}

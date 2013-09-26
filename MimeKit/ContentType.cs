//
// ContentType.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2012 Jeffrey Stedfast
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
	public sealed class ContentType
	{
		ParameterList parameters;
		string type, subtype;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.ContentType"/> class.
		/// </summary>
		/// <param name="mediaType">Media type.</param>
		/// <param name="mediaSubtype">Media subtype.</param>
		public ContentType (string mediaType, string mediaSubtype)
		{
			if (mediaType == null)
				throw new ArgumentNullException ("mediaType");

			if (mediaType.Length == 0)
				throw new ArgumentException ("The type is not allowed to be empty.", "mediaType");

			for (int i = 0; i < mediaType.Length; i++) {
				if (mediaType[i] >= 127 || !IsAtom ((byte) mediaType[i]))
					throw new ArgumentException ("Illegal characters in type.", "mediaType");
			}

			if (mediaSubtype == null)
				throw new ArgumentNullException ("mediaSubtype");

			if (mediaSubtype.Length == 0)
				throw new ArgumentException ("The subtype is not allowed to be empty.", "mediaSubtype");

			for (int i = 0; i < mediaSubtype.Length; i++) {
				if (mediaSubtype[i] >= 127 || !IsAtom ((byte) mediaSubtype[i]))
					throw new ArgumentException ("Illegal characters in subtype.", "mediaSubtype");
			}

			Parameters = new ParameterList ();
			subtype = mediaSubtype;
			type = mediaType;
		}

		static bool IsAtom (byte c)
		{
			return c.IsAtom ();
		}

		/// <summary>
		/// Gets or sets the type of the media.
		/// </summary>
		/// <value>The type of the media.</value>
		public string MediaType {
			get { return type; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (value.Length == 0)
					throw new ArgumentException ("MediaType is not allowed to be empty.", "value");

				for (int i = 0; i < value.Length; i++) {
					if (value[i] > 127 || !IsAtom ((byte) value[i]))
						throw new ArgumentException ("Illegal characters in media type.", "value");
				}

				if (type == value)
					return;

				type = value;

				OnChanged ();
			}
		}

		/// <summary>
		/// Gets or sets the media subtype.
		/// </summary>
		/// <value>The media subtype.</value>
		public string MediaSubtype {
			get { return subtype; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (value.Length == 0)
					throw new ArgumentException ("MediaSubtype is not allowed to be empty.", "value");

				for (int i = 0; i < value.Length; i++) {
					if (value[i] > 127 || !IsAtom ((byte) value[i]))
						throw new ArgumentException ("Illegal characters in media subtype.", "value");
				}

				if (subtype == value)
					return;

				subtype = value;

				OnChanged ();
			}
		}

		/// <summary>
		/// Gets the parameters.
		/// </summary>
		/// <value>The parameters.</value>
		public ParameterList Parameters {
			get { return parameters; }
			private set {
				if (parameters != null)
					parameters.Changed -= OnParametersChanged;

				value.Changed += OnParametersChanged;
				parameters = value;
			}
		}

		/// <summary>
		/// Gets or sets the boundary parameter.
		/// </summary>
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
		/// Gets or sets the charset parameter.
		/// </summary>
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
		/// Gets or sets the name parameter.
		/// </summary>
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
		/// the specified mediaType and mediaSubtype.
		/// 
		/// Note: If the specified mediaType or mediaSubtype are "*", they match anything.
		/// </summary>
		/// <param name="mediaType">The media type.</param>
		/// <param name="mediaSubtype">The media subtype.</param>
		public bool Matches (string mediaType, string mediaSubtype)
		{
			if (mediaType == null)
				throw new ArgumentNullException ("mediaType");

			if (mediaSubtype == null)
				throw new ArgumentNullException ("mediaSubtype");

			var stricase = StringComparer.OrdinalIgnoreCase;
			if (mediaType == "*" || stricase.Compare (mediaType, type) == 0)
				return mediaSubtype == "*" || stricase.Compare (mediaSubtype, subtype) == 0;

			return false;
		}

		internal string Encode (FormatOptions options, Encoding charset)
		{
			int lineLength = "Content-Type: ".Length;
			var value = new StringBuilder (" ");

			value.Append (MediaType);
			value.Append ('/');
			value.Append (MediaSubtype);

			Parameters.Encode (options, value, ref lineLength, charset);
			value.Append ('\n');

			return value.ToString ();
		}

		/// <summary>
		/// Serializes the <see cref="MimeKit.ContentType"/> to a string,
		/// optionally encoding the parameters.
		/// </summary>
		/// <returns>The serialized string.</returns>
		/// <param name="charset">The charset to be used when encoding the parameter values.</param>
		/// <param name="encode">If set to <c>true</c>, the parameter values will be encoded.</param>
		public string ToString (Encoding charset, bool encode)
		{
			if (charset == null)
				throw new ArgumentNullException ("charset");

			var value = new StringBuilder ("Content-Type: ");
			value.Append (MediaType);
			value.Append ('/');
			value.Append (MediaSubtype);

			if (encode) {
				int lineLength = value.Length;

				Parameters.Encode (FormatOptions.Default, value, ref lineLength, charset);
			} else {
				value.Append (Parameters.ToString ());
			}

			return value.ToString ();
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current
		/// <see cref="MimeKit.ContentType"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current
		/// <see cref="MimeKit.ContentType"/>.</returns>
		public override string ToString ()
		{
			return ToString (Encoding.UTF8, false);
		}

		public event EventHandler Changed;

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

			while (index < endIndex && text[index].IsAtom () && text[index] != (byte) '/')
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
			if (!ParseUtils.SkipAtom (text, ref index, endIndex)) {
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
		/// <returns><c>true</c>, if the content type was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="type">The parsed content type.</param>
		public static bool TryParse (ParserOptions options, byte[] buffer, int startIndex, int length, out ContentType type)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (startIndex < 0 || startIndex >= buffer.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (length < 0 || startIndex + length >= buffer.Length)
				throw new ArgumentOutOfRangeException ("length");

			int index = startIndex;

			return TryParse (options, buffer, ref index, startIndex + length, false, out type);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.ContentType"/> instance.
		/// </summary>
		/// <returns><c>true</c>, if the content type was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="type">The parsed content type.</param>
		public static bool TryParse (byte[] buffer, int startIndex, int length, out ContentType type)
		{
			return TryParse (ParserOptions.Default, buffer, startIndex, length, out type);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.ContentType"/> instance.
		/// </summary>
		/// <returns><c>true</c>, if the content type was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="type">The parsed content type.</param>
		public static bool TryParse (ParserOptions options, byte[] buffer, int startIndex, out ContentType type)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (startIndex < 0 || startIndex >= buffer.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			int index = startIndex;

			return TryParse (options, buffer, ref index, buffer.Length, false, out type);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.ContentType"/> instance.
		/// </summary>
		/// <returns><c>true</c>, if the content type was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="type">The parsed content type.</param>
		public static bool TryParse (byte[] buffer, int startIndex, out ContentType type)
		{
			return TryParse (ParserOptions.Default, buffer, startIndex, out type);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.ContentType"/> instance.
		/// </summary>
		/// <returns><c>true</c>, if the content type was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="type">The parsed content type.</param>
		public static bool TryParse (ParserOptions options, byte[] buffer, out ContentType type)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			int index = 0;

			return TryParse (options, buffer, ref index, buffer.Length, false, out type);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.ContentType"/> instance.
		/// </summary>
		/// <returns><c>true</c>, if the content type was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="type">The parsed content type.</param>
		public static bool TryParse (byte[] buffer, out ContentType type)
		{
			return TryParse (ParserOptions.Default, buffer, out type);
		}

		/// <summary>
		/// Tries to parse the given text into a new <see cref="MimeKit.ContentType"/> instance.
		/// </summary>
		/// <returns><c>true</c>, if the content type was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="text">The text to parse.</param>
		/// <param name="type">The parsed content type.</param>
		public static bool TryParse (string text, out ContentType type)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			var buffer = Encoding.UTF8.GetBytes (text);
			int index = 0;

			return TryParse (ParserOptions.Default, buffer, ref index, buffer.Length, false, out type);
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="MimeKit.ContentType"/> class.
		/// </summary>
		/// <returns>The parsed <see cref="MimeKit.ContentType"/>.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The start index of the buffer.</param>
		/// <param name="length">The length of the buffer.</param>
		public static ContentType Parse (ParserOptions options, byte[] buffer, int startIndex, int length)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (length < 0 || startIndex + length > buffer.Length)
				throw new ArgumentOutOfRangeException ("length");

			int index = startIndex;
			ContentType type;

			TryParse (options, buffer, ref index, startIndex + length, true, out type);

			return type;
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="MimeKit.ContentType"/> class.
		/// </summary>
		/// <returns>The parsed <see cref="MimeKit.ContentType"/>.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The start index of the buffer.</param>
		/// <param name="length">The length of the buffer.</param>
		public static ContentType Parse (byte[] buffer, int startIndex, int length)
		{
			return Parse (ParserOptions.Default, buffer, startIndex, length);
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="MimeKit.ContentType"/> class.
		/// </summary>
		/// <returns>The parsed <see cref="MimeKit.ContentType"/>.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The start index of the buffer.</param>
		public static ContentType Parse (ParserOptions options, byte[] buffer, int startIndex)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			int index = startIndex;
			ContentType type;

			TryParse (options, buffer, ref index, buffer.Length, true, out type);

			return type;
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="MimeKit.ContentType"/> class.
		/// </summary>
		/// <returns>The parsed <see cref="MimeKit.ContentType"/>.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The start index of the buffer.</param>
		public static ContentType Parse (byte[] buffer, int startIndex)
		{
			return Parse (ParserOptions.Default, buffer, startIndex);
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="MimeKit.ContentType"/> class.
		/// </summary>
		/// <returns>The parsed <see cref="MimeKit.ContentType"/>.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="buffer">The input buffer.</param>
		public static ContentType Parse (ParserOptions options, byte[] buffer)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			ContentType type;
			int index = 0;

			TryParse (options, buffer, ref index, buffer.Length, true, out type);

			return type;
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="MimeKit.ContentType"/> class.
		/// </summary>
		/// <returns>The parsed <see cref="MimeKit.ContentType"/>.</returns>
		/// <param name="buffer">The input buffer.</param>
		public static ContentType Parse (byte[] buffer)
		{
			return Parse (ParserOptions.Default, buffer);
		}

		/// <summary>
		/// Parse the specified text into a new instance of the <see cref="MimeKit.ContentType"/> class.
		/// </summary>
		/// <returns>The parsed <see cref="MimeKit.ContentType"/>.</returns>
		/// <param name="text">The text.</param>
		public static ContentType Parse (string text)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			var buffer = Encoding.UTF8.GetBytes (text);
			ContentType type;
			int index = 0;

			TryParse (ParserOptions.Default, buffer, ref index, buffer.Length, true, out type);

			return type;
		}
	}
}

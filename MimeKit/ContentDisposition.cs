//
// ContentDisposition.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc.
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
	/// A class representing a Content-Disposition header value.
	/// </summary>
	/// <remarks>
	/// The Content-Disposition header is a way for the originating client to
	/// suggest to the receiving client whether to present the part to the user
	/// as an attachment or as part of the content (inline).
	/// </remarks>
	public class ContentDisposition
	{
		/// <summary>
		/// The attachment disposition.
		/// </summary>
		/// <remarks>
		/// Indicates that the <see cref="MimePart"/> should be treated as an attachment.
		/// </remarks>
		public const string Attachment = "attachment";

		/// <summary>
		/// The form-data disposition.
		/// </summary>
		/// <remarks>
		/// Indicates that the <see cref="MimePart"/> should be treated as form data.
		/// </remarks>
		public const string FormData = "form-data";

		/// <summary>
		/// The inline disposition.
		/// </summary>
		/// <remarks>
		/// Indicates that the <see cref="MimePart"/> should be rendered inline.
		/// </remarks>
		public const string Inline = "inline";

		static readonly StringComparer icase = StringComparer.OrdinalIgnoreCase;
		ParameterList parameters;
		string disposition;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.ContentDisposition"/> class.
		/// </summary>
		/// <remarks>
		/// The disposition should either be <see cref="ContentDisposition.Attachment"/>
		/// or <see cref="ContentDisposition.Inline"/>.
		/// </remarks>
		/// <param name="disposition">The disposition.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="disposition"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="disposition"/> is not <c>"attachment"</c> or <c>"inline"</c>.
		/// </exception>
		public ContentDisposition (string disposition)
		{
			Parameters = new ParameterList ();
			Disposition = disposition;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.ContentDisposition"/> class.
		/// </summary>
		/// <remarks>
		/// This is identical to <see cref="ContentDisposition(string)"/> with a disposition
		/// value of <see cref="ContentDisposition.Attachment"/>.
		/// </remarks>
		public ContentDisposition () : this (Attachment)
		{
		}

		static bool IsAsciiAtom (byte c)
		{
			return c.IsAsciiAtom ();
		}

		/// <summary>
		/// Gets or sets the disposition.
		/// </summary>
		/// <remarks>
		/// The disposition is typically either <c>"attachment"</c> or <c>"inline"</c>.
		/// </remarks>
		/// <value>The disposition.</value>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="value"/> is an invalid disposition value.
		/// </exception>
		public string Disposition {
			get { return disposition; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (value.Length == 0)
					throw new ArgumentException ("The disposition is not allowed to be empty.", "value");

				for (int i = 0; i < value.Length; i++) {
					if (value[i] >= 127 || !IsAsciiAtom ((byte) value[i]))
						throw new ArgumentException ("Illegal characters in disposition value.", "value");
				}

				if (disposition == value)
					return;

				disposition = value;
				OnChanged ();
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the <see cref="MimePart"/> is an attachment.
		/// </summary>
		/// <remarks>
		/// A convenience property to determine if the entity should be considered an attachment or not.
		/// </remarks>
		/// <value><c>true</c> if the <see cref="MimePart"/> is an attachment; otherwise, <c>false</c>.</value>
		public bool IsAttachment {
			get { return icase.Compare (disposition, Attachment) == 0; }
			set { disposition = value ? Attachment : Inline; }
		}

		/// <summary>
		/// Gets the parameters.
		/// </summary>
		/// <remarks>
		/// In addition to specifying whether the entity should be treated as an
		/// attachment vs displayed inline, the Content-Disposition header may also
		/// contain parameters to provide further information to the receiving client
		/// such as the file attributes.
		/// </remarks>
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
		/// Gets or sets the name of the file.
		/// </summary>
		/// <remarks>
		/// When set, this can provide a useful hint for a default file name for the
		/// content when the user decides to save it to disk.
		/// </remarks>
		/// <value>The name of the file.</value>
		public string FileName {
			get { return Parameters["filename"]; }
			set {
				if (value != null)
					Parameters["filename"] = value;
				else
					Parameters.Remove ("filename");
			}
		}

		static bool IsNullOrWhiteSpace (string value)
		{
			if (string.IsNullOrEmpty (value))
				return true;

			for (int i = 0; i < value.Length; i++) {
				if (!char.IsWhiteSpace (value[i]))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Gets or sets the creation-date parameter.
		/// </summary>
		/// <remarks>
		/// Refers to the date and time that the content file was created on the
		/// originating system. This parameter serves little purpose and is
		/// typically not used by mail clients.
		/// </remarks>
		/// <value>The creation date.</value>
		public DateTimeOffset? CreationDate {
			get {
				var value = Parameters["creation-date"];
				if (IsNullOrWhiteSpace (value))
					return null;

				var buffer = Encoding.UTF8.GetBytes (value);
				DateTimeOffset ctime;

				if (!DateUtils.TryParse (buffer, 0, buffer.Length, out ctime))
					return null;

				return ctime;
			}
			set {
				if (value.HasValue)
					Parameters["creation-date"] = DateUtils.FormatDate (value.Value);
				else
					Parameters.Remove ("creation-date");
			}
		}

		/// <summary>
		/// Gets or sets the modification-date parameter.
		/// </summary>
		/// <remarks>
		/// Refers to the date and time that the content file was last modified on
		/// the originating system. This parameter serves little purpose and is
		/// typically not used by mail clients.
		/// </remarks>
		/// <value>The modification date.</value>
		public DateTimeOffset? ModificationDate {
			get {
				var value = Parameters["modification-date"];
				if (IsNullOrWhiteSpace (value))
					return null;

				var buffer = Encoding.UTF8.GetBytes (value);
				DateTimeOffset mtime;

				if (!DateUtils.TryParse (buffer, 0, buffer.Length, out mtime))
					return null;

				return mtime;
			}
			set {
				if (value.HasValue)
					Parameters["modification-date"] = DateUtils.FormatDate (value.Value);
				else
					Parameters.Remove ("modification-date");
			}
		}

		/// <summary>
		/// Gets or sets the read-date parameter.
		/// </summary>
		/// <remarks>
		/// Refers to the date and time that the content file was last read on the
		/// originating system. This parameter serves little purpose and is typically
		/// not used by mail clients.
		/// </remarks>
		/// <value>The read date.</value>
		public DateTimeOffset? ReadDate {
			get {
				var value = Parameters["read-date"];
				if (IsNullOrWhiteSpace (value))
					return null;

				var buffer = Encoding.UTF8.GetBytes (value);
				DateTimeOffset atime;

				if (!DateUtils.TryParse (buffer, 0, buffer.Length, out atime))
					return null;

				return atime;
			}
			set {
				if (value.HasValue)
					Parameters["read-date"] = DateUtils.FormatDate (value.Value);
				else
					Parameters.Remove ("read-date");
			}
		}

		/// <summary>
		/// Gets or sets the size parameter.
		/// </summary>
		/// <remarks>
		/// When set, the size parameter typically refers to the original size of the
		/// content on disk. This parameter is rarely used by mail clients as it serves
		/// little purpose.
		/// </remarks>
		/// <value>The size.</value>
		public long? Size {
			get {
				var value = Parameters["size"];
				if (IsNullOrWhiteSpace (value))
					return null;

				long size;
				if (!long.TryParse (value, out size))
					return null;

				return size;
			}
			set {
				if (value.HasValue)
					Parameters["size"] = value.Value.ToString ();
				else
					Parameters.Remove ("size");
			}
		}

		internal string Encode (FormatOptions options, Encoding charset)
		{
			int lineLength = "Content-Disposition: ".Length;
			var value = new StringBuilder (" ");

			value.Append (disposition);
			Parameters.Encode (options, value, ref lineLength, charset);
			value.Append (options.NewLine);

			return value.ToString ();
		}

		/// <summary>
		/// Serializes the <see cref="MimeKit.ContentDisposition"/> to a string,
		/// optionally encoding the parameters.
		/// </summary>
		/// <remarks>
		/// Creates a string-representation of the <see cref="ContentDisposition"/>,
		/// optionally encoding the parameters as they would be encoded for trabsport.
		/// </remarks>
		/// <returns>The serialized string.</returns>
		/// <param name="charset">The charset to be used when encoding the parameter values.</param>
		/// <param name="encode">If set to <c>true</c>, the parameter values will be encoded.</param>
		public string ToString (Encoding charset, bool encode)
		{
			if (charset == null)
				throw new ArgumentNullException ("charset");

			var value = new StringBuilder ("Content-Disposition: ");
			value.Append (disposition);

			if (encode) {
				var options = FormatOptions.GetDefault ();
				int lineLength = value.Length;

				Parameters.Encode (options, value, ref lineLength, charset);
			} else {
				value.Append (Parameters.ToString ());
			}

			return value.ToString ();
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current
		/// <see cref="MimeKit.ContentDisposition"/>.
		/// </summary>
		/// <remarks>
		/// Creates a string-representation of the <see cref="ContentDisposition"/>.
		/// </remarks>
		/// <returns>A <see cref="System.String"/> that represents the current
		/// <see cref="MimeKit.ContentDisposition"/>.</returns>
		public override string ToString ()
		{
			return ToString (Encoding.UTF8, false);
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

		internal static bool TryParse (ParserOptions options, byte[] text, ref int index, int endIndex, bool throwOnError, out ContentDisposition disposition)
		{
			string type;
			int atom;

			disposition = null;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			atom = index;
			if (!ParseUtils.SkipAtom (text, ref index, endIndex)) {
				if (throwOnError)
					throw new ParseException (string.Format ("Invalid atom token at position {0}", atom), atom, index);

				return false;
			}

			type = Encoding.ASCII.GetString (text, atom, index - atom);

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			disposition = new ContentDisposition ();
			disposition.disposition = type;

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

			disposition.Parameters = parameters;

			return true;
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.ContentDisposition"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the supplied buffer starting at the given index
		/// and spanning across the specified number of bytes.
		/// </remarks>
		/// <returns><c>true</c>, if the disposition was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="disposition">The parsed disposition.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static bool TryParse (ParserOptions options, byte[] buffer, int startIndex, int length, out ContentDisposition disposition)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (length < 0 || length > (buffer.Length - startIndex))
				throw new ArgumentOutOfRangeException ("length");

			int index = startIndex;

			return TryParse (options, buffer, ref index, startIndex + length, false, out disposition);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.ContentDisposition"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the supplied buffer starting at the given index
		/// and spanning across the specified number of bytes.
		/// </remarks>
		/// <returns><c>true</c>, if the disposition was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="disposition">The parsed disposition.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static bool TryParse (byte[] buffer, int startIndex, int length, out ContentDisposition disposition)
		{
			return TryParse (ParserOptions.Default, buffer, startIndex, length, out disposition);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.ContentDisposition"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the supplied buffer starting at the specified index.
		/// </remarks>
		/// <returns><c>true</c>, if the disposition was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="disposition">The parsed disposition.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> is out of range.
		/// </exception>
		public static bool TryParse (ParserOptions options, byte[] buffer, int startIndex, out ContentDisposition disposition)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (startIndex < 0 || startIndex >= buffer.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			int index = startIndex;

			return TryParse (options, buffer, ref index, buffer.Length, false, out disposition);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.ContentDisposition"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the supplied buffer starting at the specified index.
		/// </remarks>
		/// <returns><c>true</c>, if the disposition was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="disposition">The parsed disposition.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> is out of range.
		/// </exception>
		public static bool TryParse (byte[] buffer, int startIndex, out ContentDisposition disposition)
		{
			return TryParse (ParserOptions.Default, buffer, startIndex, out disposition);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.ContentDisposition"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the specified buffer.
		/// </remarks>
		/// <returns><c>true</c>, if the disposition was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="disposition">The parsed disposition.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		public static bool TryParse (ParserOptions options, byte[] buffer, out ContentDisposition disposition)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			int index = 0;

			return TryParse (options, buffer, ref index, buffer.Length, false, out disposition);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.ContentDisposition"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the specified buffer.
		/// </remarks>
		/// <returns><c>true</c>, if the disposition was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="disposition">The parsed disposition.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		public static bool TryParse (byte[] buffer, out ContentDisposition disposition)
		{
			return TryParse (ParserOptions.Default, buffer, out disposition);
		}

		/// <summary>
		/// Tries to parse the given text into a new <see cref="MimeKit.ContentDisposition"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the supplied text.
		/// </remarks>
		/// <returns><c>true</c>, if the disposition was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="text">The text to parse.</param>
		/// <param name="disposition">The parsed disposition.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public static bool TryParse (string text, out ContentDisposition disposition)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			var buffer = Encoding.UTF8.GetBytes (text);
			int index = 0;

			return TryParse (ParserOptions.Default, buffer, ref index, buffer.Length, false, out disposition);
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="MimeKit.ContentDisposition"/> class.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the supplied buffer starting at the given index
		/// and spanning across the specified number of bytes.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.ContentDisposition"/>.</returns>
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
		public static ContentDisposition Parse (ParserOptions options, byte[] buffer, int startIndex, int length)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (length < 0 || length > (buffer.Length - startIndex))
				throw new ArgumentOutOfRangeException ("length");

			ContentDisposition disposition;
			int index = startIndex;

			TryParse (options, buffer, ref index, startIndex + length, true, out disposition);

			return disposition;
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="MimeKit.ContentDisposition"/> class.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the supplied buffer starting at the given index
		/// and spanning across the specified number of bytes.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.ContentDisposition"/>.</returns>
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
		public static ContentDisposition Parse (byte[] buffer, int startIndex, int length)
		{
			return Parse (ParserOptions.Default, buffer, startIndex, length);
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="MimeKit.ContentDisposition"/> class.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the supplied buffer starting at the specified index.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.ContentDisposition"/>.</returns>
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
		public static ContentDisposition Parse (ParserOptions options, byte[] buffer, int startIndex)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			ContentDisposition disposition;
			int index = startIndex;

			TryParse (options, buffer, ref index, buffer.Length, true, out disposition);

			return disposition;
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="MimeKit.ContentDisposition"/> class.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the supplied buffer starting at the specified index.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.ContentDisposition"/>.</returns>
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
		public static ContentDisposition Parse (byte[] buffer, int startIndex)
		{
			return Parse (ParserOptions.Default, buffer, startIndex);
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="MimeKit.ContentDisposition"/> class.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the supplied buffer.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.ContentDisposition"/>.</returns>
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
		public static ContentDisposition Parse (ParserOptions options, byte[] buffer)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			ContentDisposition disposition;
			int index = 0;

			TryParse (options, buffer, ref index, buffer.Length, true, out disposition);

			return disposition;
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="MimeKit.ContentDisposition"/> class.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the supplied buffer.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.ContentDisposition"/>.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// The <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static ContentDisposition Parse (byte[] buffer)
		{
			return Parse (ParserOptions.Default, buffer);
		}

		/// <summary>
		/// Parse the specified text into a new instance of the <see cref="MimeKit.ContentDisposition"/> class.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the specified text.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.ContentDisposition"/>.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="text">The input text.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="text"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// The <paramref name="text"/> could not be parsed.
		/// </exception>
		public static ContentDisposition Parse (ParserOptions options, string text)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (text == null)
				throw new ArgumentNullException ("text");

			var buffer = Encoding.UTF8.GetBytes (text);
			ContentDisposition disposition;
			int index = 0;

			TryParse (options, buffer, ref index, buffer.Length, true, out disposition);

			return disposition;
		}

		/// <summary>
		/// Parse the specified text into a new instance of the <see cref="MimeKit.ContentDisposition"/> class.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the specified text.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.ContentDisposition"/>.</returns>
		/// <param name="text">The input text.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// The <paramref name="text"/> could not be parsed.
		/// </exception>
		public static ContentDisposition Parse (string text)
		{
			return Parse (ParserOptions.Default, text);
		}
	}
}

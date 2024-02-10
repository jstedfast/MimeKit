//
// ContentDisposition.cs
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
using System.Globalization;

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

		ParameterList parameters;
		string disposition;

		/// <summary>
		/// Initialize a new instance of the <see cref="ContentDisposition"/> class.
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
		/// Initialize a new instance of the <see cref="ContentDisposition"/> class.
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
		/// Get or set the disposition.
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
				if (value is null)
					throw new ArgumentNullException (nameof (value));

				if (value.Length == 0)
					throw new ArgumentException ("The disposition is not allowed to be empty.", nameof (value));

				for (int i = 0; i < value.Length; i++) {
					if (value[i] >= 127 || !IsAsciiAtom ((byte) value[i]))
						throw new ArgumentException ("Illegal characters in disposition value.", nameof (value));
				}

				if (disposition == value)
					return;

				disposition = value;
				OnChanged ();
			}
		}

		/// <summary>
		/// Get or set a value indicating whether the <see cref="MimePart"/> is an attachment.
		/// </summary>
		/// <remarks>
		/// A convenience property to determine if the entity should be considered an attachment or not.
		/// </remarks>
		/// <value><c>true</c> if the <see cref="MimePart"/> is an attachment; otherwise, <c>false</c>.</value>
		public bool IsAttachment {
			get { return disposition.Equals (Attachment, StringComparison.OrdinalIgnoreCase); }
			set { disposition = value ? Attachment : Inline; }
		}

		/// <summary>
		/// Get the list of parameters on the <see cref="ContentDisposition"/>.
		/// </summary>
		/// <remarks>
		/// In addition to specifying whether the entity should be treated as an
		/// attachment vs displayed inline, the Content-Disposition header may also
		/// contain parameters to provide further information to the receiving client
		/// such as the file attributes.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\ParameterExamples.cs" region="OverrideFileNameParameterEncoding"/>
		/// </example>
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
		/// Get or set the name of the file.
		/// </summary>
		/// <remarks>
		/// When set, this can provide a useful hint for a default file name for the
		/// content when the user decides to save it to disk.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\AttachmentExamples.cs" region="SaveAttachments" />
		/// </example>
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

		/// <summary>
		/// Get or set the creation-date parameter.
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
				if (string.IsNullOrWhiteSpace (value))
					return null;

				var buffer = Encoding.UTF8.GetBytes (value);

				if (!DateUtils.TryParse (buffer, 0, buffer.Length, out var ctime))
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
		/// Get or set the modification-date parameter.
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
				if (string.IsNullOrWhiteSpace (value))
					return null;

				var buffer = Encoding.UTF8.GetBytes (value);

				if (!DateUtils.TryParse (buffer, 0, buffer.Length, out var mtime))
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
		/// Get or set the read-date parameter.
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
				if (string.IsNullOrWhiteSpace (value))
					return null;

				var buffer = Encoding.UTF8.GetBytes (value);

				if (!DateUtils.TryParse (buffer, 0, buffer.Length, out var atime))
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
		/// Get or set the size parameter.
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
				if (string.IsNullOrWhiteSpace (value))
					return null;

				if (!long.TryParse (value, out var size))
					return null;

				return size;
			}
			set {
				if (value.HasValue)
					Parameters["size"] = value.Value.ToString (CultureInfo.InvariantCulture);
				else
					Parameters.Remove ("size");
			}
		}

		/// <summary>
		/// Clone the content disposition.
		/// </summary>
		/// <remarks>
		/// Clones the content disposition.
		/// </remarks>
		/// <returns>The cloned content disposition.s</returns>
		public ContentDisposition Clone ()
		{
			var contentDisposition = new ContentDisposition (disposition);

			foreach (var parameter in parameters)
				contentDisposition.Parameters.Add (parameter.Clone ());

			return contentDisposition;
		}

		internal string Encode (FormatOptions options, Encoding charset)
		{
			int lineLength = "Content-Disposition:".Length;
			var builder = new ValueStringBuilder (128);

			builder.Append (' ');
			builder.Append (disposition);
			lineLength += builder.Length;

			Parameters.Encode (options, ref builder, ref lineLength, charset);
			builder.Append (options.NewLine);

			return builder.ToString ();
		}

		/// <summary>
		/// Serialize the <see cref="ContentDisposition"/> to a string, optionally encoding the parameters.
		/// </summary>
		/// <remarks>
		/// Creates a string-representation of the <see cref="ContentDisposition"/>,
		/// optionally encoding the parameters as they would be encoded for transport.
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
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (charset is null)
				throw new ArgumentNullException (nameof (charset));

			var builder = new ValueStringBuilder (256);
			builder.Append ("Content-Disposition: ");
			builder.Append (disposition);

			if (encode) {
				int lineLength = builder.Length;

				Parameters.Encode (options, ref builder, ref lineLength, charset);
			} else {
				Parameters.WriteTo (ref builder);
			}

			return builder.ToString ();
		}

		/// <summary>
		/// Serialize the <see cref="ContentDisposition"/> to a string, optionally encoding the parameters.
		/// </summary>
		/// <remarks>
		/// Creates a string-representation of the <see cref="ContentDisposition"/>,
		/// optionally encoding the parameters as they would be encoded for transport.
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
		/// Serialize the <see cref="ContentDisposition"/> to a string, optionally encoding the parameters.
		/// </summary>
		/// <remarks>
		/// Creates a string-representation of the <see cref="ContentDisposition"/>,
		/// optionally encoding the parameters as they would be encoded for transport.
		/// </remarks>
		/// <returns>The serialized string.</returns>
		/// <param name="encode">If set to <c>true</c>, the parameter values will be encoded.</param>
		public string ToString (bool encode)
		{
			return ToString (FormatOptions.Default, Encoding.UTF8, encode);
		}

		/// <summary>
		/// Serialize the <see cref="ContentDisposition"/> to a string.
		/// </summary>
		/// <remarks>
		/// Creates a string-representation of the <see cref="ContentDisposition"/>.
		/// </remarks>
		/// <returns>A <see cref="System.String"/> that represents the current
		/// <see cref="ContentDisposition"/>.</returns>
		public override string ToString ()
		{
			return ToString (false);
		}

		internal event EventHandler Changed;

		void OnParametersChanged (object sender, EventArgs e)
		{
			OnChanged ();
		}

		void OnChanged ()
		{
			Changed?.Invoke (this, EventArgs.Empty);
		}

		internal static bool TryParse (ParserOptions options, byte[] text, ref int index, int endIndex, bool throwOnError, out ContentDisposition disposition)
		{
			string type;
			int atom;

			disposition = null;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			if (index >= endIndex) {
				if (throwOnError)
					throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Expected atom token at position {0}", index), index, index);

				return false;
			}

			atom = index;
			if (text[index] == '"') {
				if (throwOnError)
					throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Unexpected qstring token at position {0}", atom), atom, index);

				// Note: This is a work-around for broken mailers that quote the disposition value...
				//
				// See https://github.com/jstedfast/MailKit/issues/486 for details.
				if (!ParseUtils.SkipQuoted (text, ref index, endIndex, throwOnError))
					return false;

				type = CharsetUtils.ConvertToUnicode (options, text, atom, index - atom);
				type = MimeUtils.Unquote (type);

				if (string.IsNullOrEmpty (type))
					type = Attachment;
			} else {
				if (!ParseUtils.SkipAtom (text, ref index, endIndex)) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid atom token at position {0}", atom), atom, index);

					// Note: this is a work-around for broken mailers that do not specify a disposition value...
					//
					// See https://github.com/jstedfast/MailKit/issues/486 for details.
					if (index > atom || text[index] != (byte) ';')
						return false;

					type = Attachment;
				} else {
					type = Encoding.ASCII.GetString (text, atom, index - atom);
				}
			}

			disposition = new ContentDisposition () {
				disposition = type
			};

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			if (index >= endIndex)
				return true;

			if (text[index] != (byte) ';') {
				if (throwOnError)
					throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Expected ';' at position {0}", index), index, index);

				return false;
			}

			index++;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			if (index >= endIndex)
				return true;

			if (!ParameterList.TryParse (options, text, ref index, endIndex, throwOnError, out var parameters))
				return false;

			disposition.Parameters = parameters;

			return true;
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="ContentDisposition"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the supplied buffer starting at the given index
		/// and spanning across the specified number of bytes.
		/// </remarks>
		/// <returns><c>true</c> if the disposition was successfully parsed; otherwise, <c>false</c>.</returns>
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
			ParseUtils.ValidateArguments (options, buffer, startIndex, length);

			int index = startIndex;

			return TryParse (options, buffer, ref index, startIndex + length, false, out disposition);
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="ContentDisposition"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the supplied buffer starting at the given index
		/// and spanning across the specified number of bytes.
		/// </remarks>
		/// <returns><c>true</c> if the disposition was successfully parsed; otherwise, <c>false</c>.</returns>
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
		/// Try to parse the given input buffer into a new <see cref="ContentDisposition"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the supplied buffer starting at the specified index.
		/// </remarks>
		/// <returns><c>true</c> if the disposition was successfully parsed; otherwise, <c>false</c>.</returns>
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
			ParseUtils.ValidateArguments (options, buffer, startIndex);

			int index = startIndex;

			return TryParse (options, buffer, ref index, buffer.Length, false, out disposition);
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="ContentDisposition"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the supplied buffer starting at the specified index.
		/// </remarks>
		/// <returns><c>true</c> if the disposition was successfully parsed; otherwise, <c>false</c>.</returns>
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
		/// Try to parse the given input buffer into a new <see cref="ContentDisposition"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the specified buffer.
		/// </remarks>
		/// <returns><c>true</c> if the disposition was successfully parsed; otherwise, <c>false</c>.</returns>
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
			ParseUtils.ValidateArguments (options, buffer);

			int index = 0;

			return TryParse (options, buffer, ref index, buffer.Length, false, out disposition);
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="ContentDisposition"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the specified buffer.
		/// </remarks>
		/// <returns><c>true</c> if the disposition was successfully parsed; otherwise, <c>false</c>.</returns>
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
		/// Try to parse the given text into a new <see cref="ContentDisposition"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the supplied text.
		/// </remarks>
		/// <returns><c>true</c> if the disposition was successfully parsed; otherwise, <c>false</c>.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="text">The text to parse.</param>
		/// <param name="disposition">The parsed disposition.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="text"/> is <c>null</c>.</para>
		/// </exception>
		public static bool TryParse (ParserOptions options, string text, out ContentDisposition disposition)
		{
			ParseUtils.ValidateArguments (options, text);

			var buffer = Encoding.UTF8.GetBytes (text);
			int index = 0;

			return TryParse (ParserOptions.Default, buffer, ref index, buffer.Length, false, out disposition);
		}

		/// <summary>
		/// Try to parse the given text into a new <see cref="ContentDisposition"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the supplied text.
		/// </remarks>
		/// <returns><c>true</c> if the disposition was successfully parsed; otherwise, <c>false</c>.</returns>
		/// <param name="text">The text to parse.</param>
		/// <param name="disposition">The parsed disposition.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public static bool TryParse (string text, out ContentDisposition disposition)
		{
			return TryParse (ParserOptions.Default, text, out disposition);
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="ContentDisposition"/> class.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the supplied buffer starting at the given index
		/// and spanning across the specified number of bytes.
		/// </remarks>
		/// <returns>The parsed <see cref="ContentDisposition"/>.</returns>
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
			ParseUtils.ValidateArguments (options, buffer, startIndex, length);

			int index = startIndex;

			TryParse (options, buffer, ref index, startIndex + length, true, out var disposition);

			return disposition;
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="ContentDisposition"/> class.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the supplied buffer starting at the given index
		/// and spanning across the specified number of bytes.
		/// </remarks>
		/// <returns>The parsed <see cref="ContentDisposition"/>.</returns>
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
		/// Parse the specified input buffer into a new instance of the <see cref="ContentDisposition"/> class.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the supplied buffer starting at the specified index.
		/// </remarks>
		/// <returns>The parsed <see cref="ContentDisposition"/>.</returns>
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
			ParseUtils.ValidateArguments (options, buffer, startIndex);

			int index = startIndex;

			TryParse (options, buffer, ref index, buffer.Length, true, out var disposition);

			return disposition;
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="ContentDisposition"/> class.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the supplied buffer starting at the specified index.
		/// </remarks>
		/// <returns>The parsed <see cref="ContentDisposition"/>.</returns>
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
		/// Parse the specified input buffer into a new instance of the <see cref="ContentDisposition"/> class.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the supplied buffer.
		/// </remarks>
		/// <returns>The parsed <see cref="ContentDisposition"/>.</returns>
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
			ParseUtils.ValidateArguments (options, buffer);

			int index = 0;

			TryParse (options, buffer, ref index, buffer.Length, true, out var disposition);

			return disposition;
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="ContentDisposition"/> class.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the supplied buffer.
		/// </remarks>
		/// <returns>The parsed <see cref="ContentDisposition"/>.</returns>
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
		/// Parse the specified text into a new instance of the <see cref="ContentDisposition"/> class.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the specified text.
		/// </remarks>
		/// <returns>The parsed <see cref="ContentDisposition"/>.</returns>
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
			ParseUtils.ValidateArguments (options, text);

			var buffer = Encoding.UTF8.GetBytes (text);
			int index = 0;

			TryParse (options, buffer, ref index, buffer.Length, true, out var disposition);

			return disposition;
		}

		/// <summary>
		/// Parse the specified text into a new instance of the <see cref="ContentDisposition"/> class.
		/// </summary>
		/// <remarks>
		/// Parses a Content-Disposition value from the specified text.
		/// </remarks>
		/// <returns>The parsed <see cref="ContentDisposition"/>.</returns>
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

//
// ContentDisposition.cs
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
	public sealed class ContentDisposition
	{
		static readonly StringComparer icase = StringComparer.OrdinalIgnoreCase;
		ParameterList parameters;
		string disposition;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.ContentDisposition"/> class.
		/// </summary>
		/// <param name="disposition">The disposition.</param>
		public ContentDisposition (string disposition)
		{
			Parameters = new ParameterList ();
			Disposition = disposition;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.ContentDisposition"/> class.
		/// </summary>
		public ContentDisposition () : this ("attachment")
		{
		}

		/// <summary>
		/// Gets or sets the disposition.
		/// </summary>
		/// <value>The disposition.</value>
		public string Disposition {
			get { return disposition; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (value.Length == 0)
					throw new ArgumentException ("The disposition is not allowed to be empty.", "value");

				if (icase.Compare ("attachment", value) != 0 && icase.Compare ("inline", value) != 0)
					throw new ArgumentException ("The disposition is only allowed to be either 'attachment' or 'inline'.", "value");

				if (disposition == value)
					return;

				disposition = value;
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
		/// Gets or sets the name of the file.
		/// </summary>
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
		/// Gets or sets the creation-date parameter.
		/// </summary>
		/// <value>The creation date.</value>
		public DateTimeOffset? CreationDate {
			get {
				var value = Parameters["creation-date"];
				if (string.IsNullOrWhiteSpace (value))
					return null;

				var buffer = Encoding.UTF8.GetBytes (value);
				DateTimeOffset ctime;

				if (!DateUtils.TryParseDateTime (buffer, 0, buffer.Length, out ctime))
					return null;

				return ctime;
			}
			set {
				if (value.HasValue)
					Parameters["creation-date"] = DateUtils.ToString (value.Value);
				else
					Parameters.Remove ("creation-date");
			}
		}

		/// <summary>
		/// Gets or sets the modification-date parameter.
		/// </summary>
		/// <value>The modification date.</value>
		public DateTimeOffset? ModificationDate {
			get {
				var value = Parameters["modification-date"];
				if (string.IsNullOrWhiteSpace (value))
					return null;

				var buffer = Encoding.UTF8.GetBytes (value);
				DateTimeOffset mtime;

				if (!DateUtils.TryParseDateTime (buffer, 0, buffer.Length, out mtime))
					return null;

				return mtime;
			}
			set {
				if (value.HasValue)
					Parameters["modification-date"] = DateUtils.ToString (value.Value);
				else
					Parameters.Remove ("modification-date");
			}
		}

		/// <summary>
		/// Gets or sets the read-date parameter.
		/// </summary>
		/// <value>The read date.</value>
		public DateTimeOffset? ReadDate {
			get {
				var value = Parameters["read-date"];
				if (string.IsNullOrWhiteSpace (value))
					return null;

				var buffer = Encoding.UTF8.GetBytes (value);
				DateTimeOffset atime;

				if (!DateUtils.TryParseDateTime (buffer, 0, buffer.Length, out atime))
					return null;

				return atime;
			}
			set {
				if (value.HasValue)
					Parameters["read-date"] = DateUtils.ToString (value.Value);
				else
					Parameters.Remove ("read-date");
			}
		}

		/// <summary>
		/// Gets or sets the size parameter.
		/// </summary>
		/// <value>The size.</value>
		public long? Size {
			get {
				var value = Parameters["size"];
				if (string.IsNullOrWhiteSpace (value))
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
			value.Append ('\n');

			return value.ToString ();
		}

		/// <summary>
		/// Serializes the <see cref="MimeKit.ContentDisposition"/> to a string,
		/// optionally encoding the parameters.
		/// </summary>
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
				int lineLength = value.Length;

				Parameters.Encode (FormatOptions.Default, value, ref lineLength, charset);
			} else {
				value.Append (Parameters.ToString ());
			}

			return value.ToString ();
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current
		/// <see cref="MimeKit.ContentDisposition"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current
		/// <see cref="MimeKit.ContentDisposition"/>.</returns>
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
		/// <returns><c>true</c>, if the disposition was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="disposition">The parsed disposition.</param>
		public static bool TryParse (ParserOptions options, byte[] buffer, int startIndex, int length, out ContentDisposition disposition)
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

			return TryParse (options, buffer, ref index, startIndex + length, false, out disposition);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.ContentDisposition"/> instance.
		/// </summary>
		/// <returns><c>true</c>, if the disposition was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="disposition">The parsed disposition.</param>
		public static bool TryParse (byte[] buffer, int startIndex, int length, out ContentDisposition disposition)
		{
			return TryParse (ParserOptions.Default, buffer, startIndex, length, out disposition);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.ContentDisposition"/> instance.
		/// </summary>
		/// <returns><c>true</c>, if the disposition was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="disposition">The parsed disposition.</param>
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
		/// <returns><c>true</c>, if the disposition was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="disposition">The parsed disposition.</param>
		public static bool TryParse (byte[] buffer, int startIndex, out ContentDisposition disposition)
		{
			return TryParse (ParserOptions.Default, buffer, startIndex, out disposition);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.ContentDisposition"/> instance.
		/// </summary>
		/// <returns><c>true</c>, if the disposition was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="disposition">The parsed disposition.</param>
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
		/// <returns><c>true</c>, if the disposition was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="disposition">The parsed disposition.</param>
		public static bool TryParse (byte[] buffer, out ContentDisposition disposition)
		{
			return TryParse (ParserOptions.Default, buffer, out disposition);
		}

		/// <summary>
		/// Tries to parse the given text into a new <see cref="MimeKit.ContentDisposition"/> instance.
		/// </summary>
		/// <returns><c>true</c>, if the disposition was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="text">The text to parse.</param>
		/// <param name="disposition">The parsed disposition.</param>
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
		/// <returns>The parsed <see cref="MimeKit.ContentDisposition"/>.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The start index of the buffer.</param>
		/// <param name="length">The length of the buffer.</param>
		public static ContentDisposition Parse (ParserOptions options, byte[] buffer, int startIndex, int length)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (length < 0 || startIndex + length > buffer.Length)
				throw new ArgumentOutOfRangeException ("length");

			ContentDisposition disposition;
			int index = startIndex;

			TryParse (options, buffer, ref index, startIndex + length, true, out disposition);

			return disposition;
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="MimeKit.ContentDisposition"/> class.
		/// </summary>
		/// <returns>The parsed <see cref="MimeKit.ContentDisposition"/>.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The start index of the buffer.</param>
		/// <param name="length">The length of the buffer.</param>
		public static ContentDisposition Parse (byte[] buffer, int startIndex, int length)
		{
			return Parse (ParserOptions.Default, buffer, startIndex, length);
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="MimeKit.ContentDisposition"/> class.
		/// </summary>
		/// <returns>The parsed <see cref="MimeKit.ContentDisposition"/>.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The start index of the buffer.</param>
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
		/// <returns>The parsed <see cref="MimeKit.ContentDisposition"/>.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The start index of the buffer.</param>
		public static ContentDisposition Parse (byte[] buffer, int startIndex)
		{
			return Parse (ParserOptions.Default, buffer, startIndex);
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="MimeKit.ContentDisposition"/> class.
		/// </summary>
		/// <returns>The parsed <see cref="MimeKit.ContentDisposition"/>.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="buffer">The input buffer.</param>
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
		/// <returns>The parsed <see cref="MimeKit.ContentDisposition"/>.</returns>
		/// <param name="buffer">The input buffer.</param>
		public static ContentDisposition Parse (byte[] buffer)
		{
			return Parse (ParserOptions.Default, buffer);
		}

		/// <summary>
		/// Parse the specified text into a new instance of the <see cref="MimeKit.ContentDisposition"/> class.
		/// </summary>
		/// <returns>The parsed <see cref="MimeKit.ContentDisposition"/>.</returns>
		/// <param name="text">The input text.</param>
		public static ContentDisposition Parse (string text)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			var buffer = Encoding.UTF8.GetBytes (text);
			ContentDisposition disposition;
			int index = 0;

			TryParse (ParserOptions.Default, buffer, ref index, buffer.Length, true, out disposition);

			return disposition;
		}
	}
}

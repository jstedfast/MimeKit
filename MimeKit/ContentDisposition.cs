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

namespace MimeKit {
	public sealed class ContentDisposition
	{
		static readonly StringComparer icase = StringComparer.InvariantCultureIgnoreCase;
		ParameterList parameters;
		string disposition;

		public ContentDisposition (string disposition)
		{
			Parameters = new ParameterList ();
			Disposition = disposition;
		}

		public ContentDisposition () : this ("attachment")
		{
		}

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

		public ParameterList Parameters {
			get { return parameters; }
			private set {
				if (parameters != null)
					parameters.Changed -= OnParametersChanged;

				value.Changed += OnParametersChanged;
				parameters = value;
			}
		}

		public string ToString (Encoding charset, bool encode)
		{
			if (charset == null)
				throw new ArgumentNullException ("charset");

			var sb = new StringBuilder ("Content-Disposition: ");
			sb.Append (Disposition);

			if (encode) {
				int lineLength = sb.Length;

				Parameters.Encode (sb, ref lineLength, charset);
			} else {
				sb.Append (Parameters.ToString ());
			}

			return sb.ToString ();
		}

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

		internal static bool TryParse (byte[] text, ref int index, int endIndex, bool throwOnError, out ContentDisposition disposition)
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

			disposition = new ContentDisposition (type);

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
			if (!ParameterList.TryParse (text, ref index, endIndex, throwOnError, out parameters))
				return false;

			disposition.Parameters = parameters;

			return true;
		}

		public static bool TryParse (byte[] text, int startIndex, int count, out ContentDisposition disposition)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			if (startIndex < 0 || startIndex >= text.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (count < 0 || startIndex + count >= text.Length)
				throw new ArgumentOutOfRangeException ("count");

			int index = startIndex;

			return TryParse (text, ref index, startIndex + count, false, out disposition);
		}

		public static bool TryParse (byte[] text, int startIndex, out ContentDisposition disposition)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			if (startIndex < 0 || startIndex >= text.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			int index = startIndex;

			return TryParse (text, ref index, text.Length, false, out disposition);
		}

		public static bool TryParse (byte[] text, out ContentDisposition disposition)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			int index = 0;

			return TryParse (text, ref index, text.Length, false, out disposition);
		}

		public static bool TryParse (string text, out ContentDisposition disposition)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			var buffer = Encoding.UTF8.GetBytes (text);
			int index = 0;

			return TryParse (buffer, ref index, buffer.Length, false, out disposition);
		}

		public static ContentDisposition Parse (byte[] text, int startIndex, int count)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			if (startIndex < 0 || startIndex > text.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (count < 0 || startIndex + count > text.Length)
				throw new ArgumentOutOfRangeException ("count");

			ContentDisposition disposition;
			int index = startIndex;

			TryParse (text, ref index, startIndex + count, true, out disposition);

			return disposition;
		}

		public static ContentDisposition Parse (byte[] text, int startIndex)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			if (startIndex < 0 || startIndex > text.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			ContentDisposition disposition;
			int index = startIndex;

			TryParse (text, ref index, text.Length, true, out disposition);

			return disposition;
		}

		public static ContentDisposition Parse (byte[] text)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			ContentDisposition disposition;
			int index = 0;

			TryParse (text, ref index, text.Length, true, out disposition);

			return disposition;
		}

		public static ContentDisposition Parse (string text)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			var buffer = Encoding.UTF8.GetBytes (text);
			ContentDisposition disposition;
			int index = 0;

			TryParse (buffer, ref index, buffer.Length, true, out disposition);

			return disposition;
		}
	}
}

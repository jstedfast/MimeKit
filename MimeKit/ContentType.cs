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

namespace MimeKit {
	public sealed class ContentType
	{
		ParameterList parameters;
		string type, subtype;

		public ContentType (string type, string subtype)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			if (type.Length == 0)
				throw new ArgumentException ("The type is not allowed to be empty.", "type");

			for (int i = 0; i < type.Length; i++) {
				if (type[i] > 127 || !IsAtom ((byte) type[i]))
					throw new ArgumentException ("Illegal characters in type.", "type");
			}

			if (subtype == null)
				throw new ArgumentNullException ("subtype");

			if (subtype.Length == 0)
				throw new ArgumentException ("The subtype is not allowed to be empty.", "subtype");

			for (int i = 0; i < subtype.Length; i++) {
				if (subtype[i] > 127 || !IsAtom ((byte) subtype[i]))
					throw new ArgumentException ("Illegal characters in subtype.", "subtype");
			}

			parameters = new ParameterList ();
			parameters.Changed += OnParametersChanged;
			this.subtype = subtype;
			this.type = type;
		}

		static bool IsAtom (byte c)
		{
			return c.IsAtom ();
		}

		public string Type {
			get { return type; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (value.Length == 0)
					throw new ArgumentException ("Type is not allowed to be empty.", "value");

				for (int i = 0; i < value.Length; i++) {
					if (value[i] > 127 || !IsAtom ((byte) value[i]))
						throw new ArgumentException ("Illegal characters in type.", "value");
				}

				if (type == value)
					return;

				type = value;

				OnChanged ();
			}
		}

		public string Subtype {
			get { return subtype; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (value.Length == 0)
					throw new ArgumentException ("Subtype is not allowed to be empty.", "value");

				for (int i = 0; i < value.Length; i++) {
					if (value[i] > 127 || !IsAtom ((byte) value[i]))
						throw new ArgumentException ("Illegal characters in subtype.", "value");
				}

				if (subtype == value)
					return;

				subtype = value;

				OnChanged ();
			}
		}

		public ParameterList Parameters {
			get { return parameters; }
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

		internal static bool TryParse (byte[] text, ref int index, int endIndex, bool throwOnError, out ContentType contentType)
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

			return ParameterList.TryParse (text, ref index, endIndex, throwOnError, out contentType.parameters);
		}

		public static bool TryParse (byte[] text, int startIndex, int count, out ContentType contentType)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			if (startIndex < 0 || startIndex >= text.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (count < 0 || startIndex + count >= text.Length)
				throw new ArgumentOutOfRangeException ("count");

			int index = startIndex;

			return TryParse (text, ref index, startIndex + count, false, out contentType);
		}

		public static bool TryParse (byte[] text, int startIndex, out ContentType contentType)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			if (startIndex < 0 || startIndex >= text.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			int index = startIndex;

			return TryParse (text, ref index, text.Length, false, out contentType);
		}

		public static bool TryParse (byte[] text, out ContentType contentType)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			int index = 0;

			return TryParse (text, ref index, text.Length, false, out contentType);
		}

		public static bool TryParse (string text, out ContentType contentType)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			var buffer = Encoding.UTF8.GetBytes (text);
			int index = 0;

			return TryParse (buffer, ref index, buffer.Length, false, out contentType);
		}

		public static ContentType Parse (byte[] text, int startIndex, int count)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			if (startIndex < 0 || startIndex > text.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (count < 0 || startIndex + count > text.Length)
				throw new ArgumentOutOfRangeException ("count");

			ContentType contentType;
			int index = startIndex;

			TryParse (text, ref index, startIndex + count, true, out contentType);

			return contentType;
		}

		public static ContentType Parse (byte[] text, int startIndex)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			if (startIndex < 0 || startIndex > text.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			ContentType contentType;
			int index = startIndex;

			TryParse (text, ref index, text.Length, true, out contentType);

			return contentType;
		}

		public static ContentType Parse (byte[] text)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			ContentType contentType;
			int index = 0;

			TryParse (text, ref index, text.Length, true, out contentType);

			return contentType;
		}

		public static ContentType Parse (string text)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			var buffer = Encoding.UTF8.GetBytes (text);
			ContentType contentType;
			int index = 0;

			TryParse (buffer, ref index, buffer.Length, true, out contentType);

			return contentType;
		}
	}
}

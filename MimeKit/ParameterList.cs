//
// ParameterList.cs
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
using System.Collections;
using System.Collections.Generic;

namespace MimeKit {
	public sealed class ParameterList : IList<Parameter>
	{
		static readonly StringComparer icase = StringComparer.InvariantCultureIgnoreCase;

		Dictionary<string, Parameter> table;
		List<Parameter> parameters;

		public ParameterList ()
		{
			table = new Dictionary<string, Parameter> (icase);
			parameters = new List<Parameter> ();
		}

		public void Add (string name, string value)
		{
			Add (new Parameter (name, value));
		}

		public bool Contains (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			return table.ContainsKey (name);
		}

		public int IndexOf (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			for (int i = 0; i < parameters.Count; i++) {
				if (parameters[i].Name == name)
					return i;
			}

			return -1;
		}

		public void Insert (int index, string name, string value)
		{
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException ("index");

			Insert (index, new Parameter (name, value));
		}

		public bool Remove (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			Parameter param;
			if (!table.TryGetValue (name, out param))
				return false;

			return Remove (param);
		}

		public string this [string name] {
			get {
				if (name == null)
					throw new ArgumentNullException ("name");

				Parameter param;
				if (table.TryGetValue (name, out param))
					return param.Value;

				return null;
			}
			set {
				if (name == null)
					throw new ArgumentNullException ("name");

				if (value == null)
					throw new ArgumentNullException ("value");

				Parameter param;
				if (table.TryGetValue (name, out param)) {
					param.Value = value;
				} else {
					Add (name, value);
				}
			}
		}

		#region ICollection implementation

		public int Count {
			get { return parameters.Count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public void Add (Parameter param)
		{
			if (param == null)
				throw new ArgumentNullException ("param");

			if (table.ContainsKey (param.Name))
				throw new ArgumentException ("A parameter of that name already exists.");

			param.Changed += OnParamChanged;
			table.Add (param.Name, param);
			parameters.Add (param);

			OnChanged ();
		}

		public void Clear ()
		{
			foreach (var param in parameters)
				param.Changed -= OnParamChanged;

			parameters.Clear ();
			table.Clear ();

			OnChanged ();
		}

		public bool Contains (Parameter param)
		{
			return parameters.Contains (param);
		}

		public void CopyTo (Parameter[] array, int arrayIndex)
		{
			parameters.CopyTo (array, arrayIndex);
		}

		public bool Remove (Parameter param)
		{
			if (param == null)
				throw new ArgumentNullException ("param");

			if (!parameters.Remove (param))
				return false;

			param.Changed -= OnParamChanged;
			table.Remove (param.Name);

			OnChanged ();

			return true;
		}

		#endregion

		#region IList implementation

		public int IndexOf (Parameter param)
		{
			return parameters.IndexOf (param);
		}

		public void Insert (int index, Parameter param)
		{
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException ("index");

			if (param == null)
				throw new ArgumentNullException ("param");

			if (table.ContainsKey (param.Name))
				throw new ArgumentException ("A parameter of that name already exists.");

			parameters.Insert (index, param);
			table.Add (param.Name, param);
			param.Changed += OnParamChanged;

			OnChanged ();
		}

		public void RemoveAt (int index)
		{
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException ("index");

			var param = parameters[index];

			param.Changed -= OnParamChanged;
			parameters.RemoveAt (index);
			table.Remove (param.Name);

			OnChanged ();
		}

		public Parameter this [int index] {
			get {
				return parameters[index];
			}
			set {
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ("index");

				var param = parameters[index];

				if (param == value)
					return;

				if (icase.Compare (param.Name, value.Name) == 0) {
					// replace the old param with the new one
					if (table[param.Name] == param)
						table[param.Name] = value;
				} else if (table.ContainsKey (value.Name)) {
					throw new ArgumentException ("A parameter of that name already exists.");
				} else {
					table.Add (value.Name, value);
					table.Remove (param.Name);
				}

				param.Changed -= OnParamChanged;
				value.Changed += OnParamChanged;
				parameters[index] = value;

				OnChanged ();
			}
		}

		#endregion

		#region IEnumerable implementation

		public IEnumerator<Parameter> GetEnumerator ()
		{
			return parameters.GetEnumerator ();
		}

		#endregion

		#region IEnumerable implementation

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return parameters.GetEnumerator ();
		}

		#endregion

		public event EventHandler Changed;

		void OnParamChanged (object sender, EventArgs args)
		{
			OnChanged ();
		}

		void OnChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}

		static bool TryParseInt32 (byte[] text, ref int index, int endIndex, out int value)
		{
			int startIndex = index;

			value = 0;

			while (index < endIndex && text[index] >= (byte) '0' && text[index] <= (byte) '9')
				value = (value * 10) + (text[index++] - (byte) '0');

			return index > startIndex;
		}

		static bool SkipParamName (byte[] text, ref int index, int endIndex)
		{
			int startIndex = index;

			while (index < endIndex && text[index].IsTToken () && text[index] != (byte) '*')
				index++;

			return index > startIndex;
		}

		class NameValuePair : IComparable<NameValuePair> {
			public int ValueLength;
			public int ValueStart;
			public bool Encoded;
			public string Name;
			public int? Id;

			#region IComparable implementation
			public int CompareTo (NameValuePair other)
			{
				if (!Id.HasValue) {
					if (other.Id.HasValue)
						return -1;

					return 0;
				}

				if (!other.Id.HasValue)
					return 1;

				return Id.Value - other.Id.Value;
			}
			#endregion
		}

		static bool TryParseNameValuePair (byte[] text, ref int index, int endIndex, bool throwOnError, out NameValuePair pair)
		{
			int valueIndex, startIndex;
			bool encoded = false;
			int? id = null;
			string name;

			pair = null;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			startIndex = index;
			if (!SkipParamName (text, ref index, endIndex)) {
				if (throwOnError)
					throw new ParseException (string.Format ("Invalid parameter name token at offset {0}", startIndex), startIndex, index);

				return false;
			}

			name = Encoding.ASCII.GetString (text, startIndex, index - startIndex);

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			if (index >= endIndex) {
				if (throwOnError)
					throw new ParseException (string.Format ("Incomplete parameter at offset {0}", startIndex), startIndex, index);

				return false;
			}

			if (text[index] == (byte) '*') {
				// the parameter is either encoded or it has a part id
				index++;

				if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;

				if (index >= endIndex) {
					if (throwOnError)
						throw new ParseException (string.Format ("Incomplete parameter at offset {0}", startIndex), startIndex, index);

					return false;
				}

				int value;
				if (TryParseInt32 (text, ref index, endIndex, out value)) {
					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;

					if (index >= endIndex) {
						if (throwOnError)
							throw new ParseException (string.Format ("Incomplete parameter at offset {0}", startIndex), startIndex, index);

						return false;
					}

					if (text[index] == (byte) '*') {
						encoded = true;
						index++;

						if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
							return false;

						if (index >= endIndex) {
							if (throwOnError)
								throw new ParseException (string.Format ("Incomplete parameter at offset {0}", startIndex), startIndex, index);

							return false;
						}
					}

					id = value;
				} else {
					encoded = true;
				}
			}

			if (text[index] != (byte) '=') {
				if (index >= endIndex) {
					if (throwOnError)
						throw new ParseException (string.Format ("Incomplete parameter at offset {0}", startIndex), startIndex, index);

					return false;
				}
			}

			index++;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			if (index >= endIndex) {
				if (index >= endIndex) {
					if (throwOnError)
						throw new ParseException (string.Format ("Incomplete parameter at offset {0}", startIndex), startIndex, index);

					return false;
				}
			}

			valueIndex = index;

			if (text[index] == (byte) '"')
				ParseUtils.SkipQuoted (text, ref index, endIndex, throwOnError);
			else
				ParseUtils.SkipTToken (text, ref index, endIndex);

			pair = new NameValuePair () {
				ValueLength = index - valueIndex,
				ValueStart = valueIndex,
				Encoded = encoded,
				Name = name,
				Id = id
			};

			return true;
		}

		static bool TryParseCharset (byte[] text, ref int index, int endIndex, out string charset)
		{
			int startIndex = index;
			int charsetEnd;
			int i;

			charset = null;

			for (i = index; i < endIndex; i++) {
				if (text[i] == (byte) '\'')
					break;
			}

			if (i == startIndex || i == endIndex)
				return false;

			charsetEnd = i;

			for (i++; i < endIndex; i++) {
				if (text[i] == (byte) '\'')
					break;
			}

			if (i == endIndex)
				return false;

			charset = Encoding.ASCII.GetString (text, startIndex, charsetEnd - startIndex);
			index = i + 1;

			return true;
		}

		static string DecodeRfc2184 (byte[] text, int startIndex, int count)
		{
			int endIndex = startIndex + count;
			Encoding encoding = null;
			int index = startIndex;
			string charset;

			if (TryParseCharset (text, ref index, endIndex, out charset))
				encoding = CharsetUtils.GetEncoding (charset);

			int length = endIndex - index;
			var decoded = new byte[length];

			// hex decode...
			length = new HexDecoder ().Decode (text, index, length, decoded);

			if (encoding == null)
				return CharsetUtils.ConvertToUnicode (decoded, 0, length);

			return CharsetUtils.ConvertToUnicode (encoding, decoded, 0, length);
		}

		internal static bool TryParse (byte[] text, ref int index, int endIndex, bool throwOnError, out ParameterList paramList)
		{
			var rfc2184 = new Dictionary<string, List<NameValuePair>> ();
			var @params = new List<NameValuePair> ();
			List<NameValuePair> list;

			paramList = null;

			do {
				if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;

				NameValuePair pair;
				if (!TryParseNameValuePair (text, ref index, endIndex, throwOnError, out pair))
					return false;

				if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;

				if (pair.Id.HasValue) {
					if (rfc2184.TryGetValue (pair.Name, out list)) {
						list.Add (pair);
					} else {
						list = new List<NameValuePair> ();
						rfc2184.Add (pair.Name, list);
						@params.Add (pair);
						list.Add (pair);
					}
				} else {
					@params.Add (pair);
				}

				if (index >= endIndex)
					break;

				if (text[index] != (byte) ';') {
					if (throwOnError)
						throw new ParseException (string.Format ("Invalid parameter list token at offset {0}", index), index, index);

					return false;
				}
			} while (true);

			paramList = new ParameterList ();

			foreach (var param in @params) {
				string value;

				if (param.Id.HasValue) {
					list = rfc2184[param.Name];
					list.Sort ();

					value = string.Empty;
					foreach (var part in list) {
						if (part.Encoded) {
							value += DecodeRfc2184 (text, part.ValueStart, part.ValueLength);
						} else if (part.ValueLength > 2 && text[part.ValueStart] == (byte) '"') {
							value += CharsetUtils.ConvertToUnicode (text, param.ValueStart + 1, param.ValueLength - 2);
						} else if (part.ValueLength > 0) {
							value += CharsetUtils.ConvertToUnicode (text, param.ValueStart, param.ValueLength);
						}
					}
				} else if (param.Encoded) {
					value = DecodeRfc2184 (text, param.ValueStart, param.ValueLength);
				} else if (param.ValueLength > 2 && text[param.ValueStart] == (byte) '"') {
					value = Rfc2047.DecodeText (text, param.ValueStart + 1, param.ValueLength - 2);
				} else if (param.ValueLength > 0) {
					value = Rfc2047.DecodeText (text, param.ValueStart, param.ValueLength);
				} else {
					value = string.Empty;
				}

				paramList.Add (new Parameter (param.Name, value));
			}

			return true;
		}
	}
}

//
// ParameterList.cs
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
using System.Collections;
using System.Collections.Generic;

#if PORTABLE
using Encoding = Portable.Text.Encoding;
using Encoder = Portable.Text.Encoder;
using Decoder = Portable.Text.Decoder;
#endif

using MimeKit.Encodings;
using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// A list of parameters, as found in the Content-Type and Content-Disposition headers.
	/// </summary>
	/// <remarks>
	/// Parameters are used by both <see cref="ContentType"/> and <see cref="ContentDisposition"/>.
	/// </remarks>
	public class ParameterList : IList<Parameter>
	{
		static readonly StringComparer icase = StringComparer.OrdinalIgnoreCase;

		readonly Dictionary<string, Parameter> table;
		readonly List<Parameter> parameters;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.ParameterList"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new parameter list.
		/// </remarks>
		public ParameterList ()
		{
			table = new Dictionary<string, Parameter> (icase);
			parameters = new List<Parameter> ();
		}

		/// <summary>
		/// Adds a parameter with the specified name and value.
		/// </summary>
		/// <remarks>
		/// Adds a new parameter to the list with the specified name and value.
		/// </remarks>
		/// <param name="name">The parameter name.</param>
		/// <param name="value">The parameter value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="name"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The <paramref name="name"/> contains illegal characters.
		/// </exception>
		public void Add (string name, string value)
		{
			Add (new Parameter (name, value));
		}

		/// <summary>
		/// Checks if the <see cref="MimeKit.ParameterList"/> contains a parameter with the specified name.
		/// </summary>
		/// <remarks>
		/// Determines whether or not the parameter list contains a parameter with the specified name.
		/// </remarks>
		/// <returns><value>true</value> if the requested parameter exists;
		/// otherwise <value>false</value>.</returns>
		/// <param name="name">The parameter name.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="name"/> is <c>null</c>.
		/// </exception>
		public bool Contains (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			return table.ContainsKey (name);
		}

		/// <summary>
		/// Gets the index of the requested parameter, if it exists.
		/// </summary>
		/// <remarks>
		/// Finds the index of the parameter with the specified name, if it exists.
		/// </remarks>
		/// <returns>The index of the requested parameter; otherwise <value>-1</value>.</returns>
		/// <param name="name">The parameter name.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="name"/> is <c>null</c>.
		/// </exception>
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

		/// <summary>
		/// Inserts a parameter with the specified name and value at the given index.
		/// </summary>
		/// <remarks>
		/// Inserts a new parameter with the given name and value at the specified index.
		/// </remarks>
		/// <param name="index">The index to insert the parameter.</param>
		/// <param name="name">The parameter name.</param>
		/// <param name="value">The parameter value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="name"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The <paramref name="name"/> contains illegal characters.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public void Insert (int index, string name, string value)
		{
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException ("index");

			Insert (index, new Parameter (name, value));
		}

		/// <summary>
		/// Removes the specified parameter.
		/// </summary>
		/// <remarks>
		/// Removes the parameter with the specified name from the list, if it exists.
		/// </remarks>
		/// <returns><value>true</value> if the specified parameter was removed;
		/// otherwise <value>false</value>.</returns>
		/// <param name="name">The parameter name.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="name"/> is <c>null</c>.
		/// </exception>
		public bool Remove (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			Parameter param;
			if (!table.TryGetValue (name, out param))
				return false;

			return Remove (param);
		}

		/// <summary>
		/// Gets or sets the value of a parameter with the specified name.
		/// </summary>
		/// <remarks>
		/// Gets or sets the value of a parameter with the specified name.
		/// </remarks>
		/// <value>The value of the specified parameter if it exists; otherwise <c>null</c>.</value>
		/// <param name="name">The parameter name.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="name"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The <paramref name="name"/> contains illegal characters.
		/// </exception>
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

		/// <summary>
		/// Gets the number of parameters in the <see cref="MimeKit.ParameterList"/>.
		/// </summary>
		/// <remarks>
		/// Indicates the number of parameters in the list.
		/// </remarks>
		/// <value>The number of parameters.</value>
		public int Count {
			get { return parameters.Count; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <remarks>
		/// A <see cref="ParameterList"/> is never read-only.
		/// </remarks>
		/// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
		public bool IsReadOnly {
			get { return false; }
		}

		/// <summary>
		/// Adds the specified parameter.
		/// </summary>
		/// <remarks>
		/// Adds the specified parameter to the end of the list.
		/// </remarks>
		/// <param name="param">The parameter to add.</param>
		/// <exception cref="System.ArgumentNullException">
		/// The <paramref name="param"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// A parameter with the same name as <paramref name="param"/>
		/// already exists.
		/// </exception>
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

		/// <summary>
		/// Clears the parameter list.
		/// </summary>
		/// <remarks>
		/// Removes all of the parameters from the list.
		/// </remarks>
		public void Clear ()
		{
			foreach (var param in parameters)
				param.Changed -= OnParamChanged;

			parameters.Clear ();
			table.Clear ();

			OnChanged ();
		}

		/// <summary>
		/// Checks if the <see cref="MimeKit.ParameterList"/> contains the specified parameter.
		/// </summary>
		/// <remarks>
		/// Determines whether or not the parameter list contains the specified parameter.
		/// </remarks>
		/// <returns><value>true</value> if the specified parameter is contained;
		/// otherwise <value>false</value>.</returns>
		/// <param name="param">The parameter.</param>
		/// <exception cref="System.ArgumentNullException">
		/// The <paramref name="param"/> is <c>null</c>.
		/// </exception>
		public bool Contains (Parameter param)
		{
			if (param == null)
				throw new ArgumentNullException ("param");

			return parameters.Contains (param);
		}

		/// <summary>
		/// Copies all of the contained parameters to the specified array.
		/// </summary>
		/// <remarks>
		/// Copies all of the parameters within the <see cref="ParameterList"/> into the array,
		/// starting at the specified array index.
		/// </remarks>
		/// <param name="array">The array to copy the parameters to.</param>
		/// <param name="arrayIndex">The index into the array.</param>
		public void CopyTo (Parameter[] array, int arrayIndex)
		{
			parameters.CopyTo (array, arrayIndex);
		}

		/// <summary>
		/// Removes the specified parameter.
		/// </summary>
		/// <remarks>
		/// Removes the specified parameter from the list.
		/// </remarks>
		/// <returns><value>true</value> if the specified parameter was removed;
		/// otherwise <value>false</value>.</returns>
		/// <param name="param">The parameter.</param>
		/// <exception cref="System.ArgumentNullException">
		/// The <paramref name="param"/> is <c>null</c>.
		/// </exception>
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

		/// <summary>
		/// Gets the index of the requested parameter, if it exists.
		/// </summary>
		/// <remarks>
		/// Finds the index of the specified parameter, if it exists.
		/// </remarks>
		/// <returns>The index of the requested parameter; otherwise <value>-1</value>.</returns>
		/// <param name="param">The parameter.</param>
		/// <exception cref="System.ArgumentNullException">
		/// The <paramref name="param"/> is <c>null</c>.
		/// </exception>
		public int IndexOf (Parameter param)
		{
			if (param == null)
				throw new ArgumentNullException ("param");

			return parameters.IndexOf (param);
		}

		/// <summary>
		/// Inserts the specified parameter at the given index.
		/// </summary>
		/// <remarks>
		/// Inserts the parameter at the specified index in the list.
		/// </remarks>
		/// <param name="index">The index to insert the parameter.</param>
		/// <param name="param">The parameter.</param>
		/// <exception cref="System.ArgumentNullException">
		/// The <paramref name="param"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="index"/> is out of range.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// A parameter with the same name as <paramref name="param"/>
		/// already exists.
		/// </exception>
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

		/// <summary>
		/// Removes the parameter at the specified index.
		/// </summary>
		/// <remarks>
		/// Removes the parameter at the specified index.
		/// </remarks>
		/// <param name="index">The index.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="index"/> is out of range.
		/// </exception>
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

		/// <summary>
		/// Gets or sets the <see cref="MimeKit.Parameter"/> at the specified index.
		/// </summary>
		/// <remarks>
		/// Gets or sets the <see cref="MimeKit.Parameter"/> at the specified index.
		/// </remarks>
		/// <value>The parameter at the specified index.</value>
		/// <param name="index">The index.</param>
		/// <exception cref="System.ArgumentNullException">
		/// The <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="index"/> is out of range.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// A parameter with the same name as <paramref name="value"/>
		/// already exists.
		/// </exception>
		public Parameter this [int index] {
			get {
				return parameters[index];
			}
			set {
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ("index");

				if (value == null)
					throw new ArgumentNullException ("value");

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

		/// <summary>
		/// Gets an enumerator for the list of parameters.
		/// </summary>
		/// <remarks>
		/// Gets an enumerator for the list of parameters.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		public IEnumerator<Parameter> GetEnumerator ()
		{
			return parameters.GetEnumerator ();
		}

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Gets an enumerator for the list of parameters.
		/// </summary>
		/// <remarks>
		/// Gets an enumerator for the list of parameters.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return parameters.GetEnumerator ();
		}

		#endregion

		internal void Encode (FormatOptions options, StringBuilder builder, ref int lineLength, Encoding charset)
		{
			foreach (var param in parameters)
				param.Encode (options, builder, ref lineLength, charset);
		}

		/// <summary>
		/// Returns a string representation of the parameters in the <see cref="ParameterList"/>.
		/// </summary>
		/// <remarks>
		/// If there are multiple parameters in the list, they will be separated by a semicolon.
		/// </remarks>
		/// <returns>A string representing the <see cref="ParameterList"/>.</returns>
		public override string ToString ()
		{
			var values = new StringBuilder ();

			foreach (var param in parameters) {
				values.Append ("; ");
				values.Append (param.ToString ());
			}

			return values.ToString ();
		}

		internal event EventHandler Changed;

		void OnParamChanged (object sender, EventArgs args)
		{
			OnChanged ();
		}

		void OnChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}

		static bool SkipParamName (byte[] text, ref int index, int endIndex)
		{
			int startIndex = index;

			while (index < endIndex && text[index].IsAttr ())
				index++;

			return index > startIndex;
		}

		class NameValuePair : IComparable<NameValuePair>
		{
			public int ValueLength;
			public int ValueStart;
			public bool Encoded;
			public string Name;
			public int? Id;

			#region IComparable implementation
			public int CompareTo (NameValuePair other)
			{
				if (!Id.HasValue)
					return other.Id.HasValue ? -1 : 0;

				if (!other.Id.HasValue)
					return 1;

				return Id.Value - other.Id.Value;
			}
			#endregion
		}

		static bool TryParseNameValuePair (ParserOptions options, byte[] text, ref int index, int endIndex, bool throwOnError, out NameValuePair pair)
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
				if (ParseUtils.TryParseInt32 (text, ref index, endIndex, out value)) {
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

			if (text[index] == (byte) '"') {
				ParseUtils.SkipQuoted (text, ref index, endIndex, throwOnError);
			} else if (options.ParameterComplianceMode == RfcComplianceMode.Strict) {
				ParseUtils.SkipToken (text, ref index, endIndex);
			} else {
				// Note: Google Docs, for example, does not always quote name/filename parameters
				// with spaces in the name. See https://github.com/jstedfast/MimeKit/issues/106
				// for details.
				while (index < endIndex && text[index] != (byte) ';' && text[index] != (byte) '\r' && text[index] != (byte) '\n')
					index++;
			}

			pair = new NameValuePair {
				ValueLength = index - valueIndex,
				ValueStart = valueIndex,
				Encoded = encoded,
				Name = name,
				Id = id
			};

			return true;
		}

		static bool TryGetCharset (byte[] text, ref int index, int endIndex, out string charset)
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

		static string DecodeRfc2184 (ref Decoder decoder, HexDecoder hex, byte[] text, int startIndex, int count, bool flush)
		{
			int endIndex = startIndex + count;
			int index = startIndex;
			string charset;

			// Note: decoder is only null if this is the first segment
			if (decoder == null) {
				if (TryGetCharset (text, ref index, endIndex, out charset)) {
					try {
						var encoding = CharsetUtils.GetEncoding (charset, "?");
						decoder = (Decoder) encoding.GetDecoder ();
					} catch (NotSupportedException) {
						var encoding = Encoding.GetEncoding (28591); // iso-8859-1
						decoder = (Decoder) encoding.GetDecoder ();
					}
				} else {
					// When no charset is specified, it should be safe to assume US-ASCII...
					// but we all know what assume means, right??
					var encoding = Encoding.GetEncoding (28591); // iso-8859-1
					decoder = (Decoder) encoding.GetDecoder ();
				}
			}

			int length = endIndex - index;
			var decoded = new byte[hex.EstimateOutputLength (length)];

			// hex decode...
			length = hex.Decode (text, index, length, decoded);

			int outLength = decoder.GetCharCount (decoded, 0, length, flush);
			var output = new char[outLength];

			outLength = decoder.GetChars (decoded, 0, length, output, 0, flush);

			return new string (output, 0, outLength);
		}

		internal static bool TryParse (ParserOptions options, byte[] text, ref int index, int endIndex, bool throwOnError, out ParameterList paramList)
		{
			var rfc2184 = new Dictionary<string, List<NameValuePair>> (icase);
			var @params = new List<NameValuePair> ();
			List<NameValuePair> parts;

			paramList = null;

			do {
				if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;

				if (index >= endIndex)
					break;

				// handle empty parameter name/value pairs
				if (text[index] == (byte) ';') {
					index++;
					continue;
				}

				NameValuePair pair;
				if (!TryParseNameValuePair (options, text, ref index, endIndex, throwOnError, out pair))
					return false;

				if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;

				if (pair.Id.HasValue) {
					if (rfc2184.TryGetValue (pair.Name, out parts)) {
						parts.Add (pair);
					} else {
						parts = new List<NameValuePair> ();
						rfc2184[pair.Name] = parts;
						@params.Add (pair);
						parts.Add (pair);
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

				index++;
			} while (true);

			paramList = new ParameterList ();
			var hex = new HexDecoder ();

			foreach (var param in @params) {
				int startIndex = param.ValueStart;
				int length = param.ValueLength;
				Decoder decoder = null;
				string value;

				if (param.Id.HasValue) {
					parts = rfc2184[param.Name];
					parts.Sort ();

					value = string.Empty;
					for (int i = 0; i < parts.Count; i++) {
						startIndex = parts[i].ValueStart;
						length = parts[i].ValueLength;

						if (parts[i].Encoded) {
							bool flush = i + 1 >= parts.Count || !parts[i+1].Encoded;

							// Note: Some mail clients mistakenly quote encoded parameter values when they shouldn't
							if (length >= 2 && text[startIndex] == (byte) '"' && text[startIndex + length - 1] == (byte) '"') {
								startIndex++;
								length -= 2;
							}

							value += DecodeRfc2184 (ref decoder, hex, text, startIndex, length, flush);
						} else if (length >= 2 && text[startIndex] == (byte) '"') {
							var quoted = CharsetUtils.ConvertToUnicode (options, text, startIndex, length);
							value += MimeUtils.Unquote (quoted);
							hex.Reset ();
						} else if (length > 0) {
							value += CharsetUtils.ConvertToUnicode (options, text, startIndex, length);
							hex.Reset ();
						}
					}
					hex.Reset ();
				} else if (param.Encoded) {
					value = DecodeRfc2184 (ref decoder, hex, text, startIndex, length, true);
					hex.Reset ();
				} else if (!paramList.Contains (param.Name)) {
					// Note: If we've got an rfc2184-encoded version of the same parameter, then
					// we'll want to choose that one as opposed to the ASCII variant (i.e. this one).
					//
					// While most mail clients that I know of do not send multiple parameters of the
					// same name, rfc6266 suggests that HTTP servers are using this approach to work
					// around HTTP clients that do not (yet) implement support for the rfc2184/2231
					// encoding of parameter values. Since none of the MIME specifications provide
					// any suggestions for dealing with this, following rfc6266 seems to make the
					// most sense, even though it is meant for HTTP clients and servers.
					if (length >= 2 && text[startIndex] == (byte) '"') {
						var quoted = Rfc2047.DecodeText (options, text, startIndex, length);
						value = MimeUtils.Unquote (quoted);
					} else if (length > 0) {
						value = Rfc2047.DecodeText (options, text, startIndex, length);
					} else {
						value = string.Empty;
					}
				} else {
					continue;
				}

				paramList[param.Name] = value;
			}

			return true;
		}
	}
}

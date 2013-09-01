//
// HeaderList.cs
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
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace MimeKit {
	public sealed class HeaderList : IList<Header>
	{
		static readonly StringComparer icase = StringComparer.OrdinalIgnoreCase;

		// this table references the first header of each field
		Dictionary<string, Header> table;
		List<Header> headers;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.HeaderList"/> class.
		/// </summary>
		public HeaderList ()
		{
			table = new Dictionary<string, Header> (icase);
			headers = new List<Header> ();
		}

		/// <summary>
		/// Adds a header with the specified field and value.
		/// </summary>
		/// <param name="field">The name of the header field.</param>
		/// <param name="value">The header value.</param>
		public void Add (string field, string value)
		{
			Add (new Header (field, value));
		}

		/// <summary>
		/// Checks if the <see cref="MimeKit.HeaderList"/> contains a header with the specified field name.
		/// </summary>
		/// <returns><value>true</value> if the requested header exists;
		/// otherwise <value>false</value>.</returns>
		/// <param name="field">The name of the header field.</param>
		public bool Contains (string field)
		{
			if (field == null)
				throw new ArgumentNullException ("field");

			return table.ContainsKey (field);
		}

		/// <summary>
		/// Gets the index of the requested header, if it exists.
		/// </summary>
		/// <returns>The index of the requested header; otherwise <value>-1</value>.</returns>
		/// <param name="field">The name of the header field.</param>
		public int IndexOf (string field)
		{
			if (field == null)
				throw new ArgumentNullException ("field");

			for (int i = 0; i < headers.Count; i++) {
				if (headers[i].Field == field)
					return i;
			}

			return -1;
		}

		/// <summary>
		/// Inserts a header with the specified field and value at the given index.
		/// </summary>
		/// <param name="index">The index to insert the header.</param>
		/// <param name="field">The name of the header field.</param>
		/// <param name="value">The header value.</param>
		public void Insert (int index, string field, string value)
		{
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException ("index");

			Insert (index, new Header (field, value));
		}

		/// <summary>
		/// Removes the first occurance of the specified header field.
		/// </summary>
		/// <returns><value>true</value> if the frst occurance of the specified
		/// header was removed; otherwise <value>false</value>.</returns>
		/// <param name="field">The name of the header field.</param>
		public bool Remove (string field)
		{
			if (field == null)
				throw new ArgumentNullException ("field");

			Header header;
			if (!table.TryGetValue (field, out header))
				return false;

			return Remove (header);
		}

		/// <summary>
		/// Gets or sets the value of the first occurance of a header
		/// with the specified field name.
		/// </summary>
		/// <param name="field">The name of the header field.</param>
		public string this [string field] {
			get {
				if (field == null)
					throw new ArgumentNullException ("field");

				Header header;
				if (table.TryGetValue (field, out header))
					return header.Value;

				return null;
			}
			set {
				if (field == null)
					throw new ArgumentNullException ("field");

				if (value == null)
					throw new ArgumentNullException ("value");

				Header header;
				if (table.TryGetValue (field, out header)) {
					header.Value = value;
				} else {
					Add (field, value);
				}
			}
		}

		/// <summary>
		/// Writes the <see cref="MimeKit.HeaderList"/> to a stream.
		/// </summary>
		/// <param name="stream">The output stream.</param>
		public void WriteTo (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			foreach (var header in headers) {
				var name = Encoding.ASCII.GetBytes (header.Field);

				stream.Write (name, 0, name.Length);
				stream.WriteByte ((byte) ':');
				stream.Write (header.RawValue, 0, header.RawValue.Length);
			}
		}

		#region ICollection implementation

		/// <summary>
		/// Gets the number of headers in the <see cref="MimeKit.HeaderList"/>.
		/// </summary>
		/// <value>The number of headers.</value>
		public int Count {
			get { return headers.Count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		/// <summary>
		/// Adds the specified header.
		/// </summary>
		/// <param name="header">The header to add.</param>
		public void Add (Header header)
		{
			if (header == null)
				throw new ArgumentNullException ("header");

			if (!table.ContainsKey (header.Field))
				table.Add (header.Field, header);

			header.Changed += OnHeaderChanged;
			headers.Add (header);

			OnChanged (header, HeaderListChangedAction.Added);
		}

		/// <summary>
		/// Removes all headers from the <see cref="MimeKit.HeaderList"/>.
		/// </summary>
		public void Clear ()
		{
			foreach (var header in headers)
				header.Changed -= OnHeaderChanged;

			headers.Clear ();
			table.Clear ();

			OnChanged (null, HeaderListChangedAction.Cleared);
		}

		/// <summary>
		/// Checks if the <see cref="HeaderList"/> contains the specified header.
		/// </summary>
		/// <returns><value>true</value> if the specified header is contained;
		/// otherwise <value>false</value>.</returns>
		/// <param name="header">The header.</param>
		public bool Contains (Header header)
		{
			return headers.Contains (header);
		}

		/// <summary>
		/// Copies all of the contained headers to the specified array.
		/// </summary>
		/// <param name="array">The array to copy the headers to.</param>
		/// <param name="arrayIndex">The index into the array.</param>
		public void CopyTo (Header[] array, int arrayIndex)
		{
			headers.CopyTo (array, arrayIndex);
		}

		/// <summary>
		/// Removes the specified header.
		/// </summary>
		/// <returns><value>true</value> if the specified header was removed;
		/// otherwise <value>false</value>.</returns>
		/// <param name="header">The header.</param>
		public bool Remove (Header header)
		{
			int index = headers.IndexOf (header);

			if (index == -1)
				return false;

			header.Changed -= OnHeaderChanged;

			if (table[header.Field] == header) {
				table.Remove (header.Field);

				// find the next matching header and add it to the lookup table
				for (int i = index + 1; i < headers.Count; i++) {
					if (icase.Compare (headers[i].Field, header.Field) == 0) {
						table.Add (headers[i].Field, headers[i]);
						break;
					}
				}
			}

			headers.RemoveAt (index);

			OnChanged (header, HeaderListChangedAction.Removed);

			return true;
		}

		#endregion

		#region IList implementation

		/// <summary>
		/// Gets the index of the requested header, if it exists.
		/// </summary>
		/// <returns>The index of the requested header; otherwise <value>-1</value>.</returns>
		/// <param name="header">The header.</param>
		public int IndexOf (Header header)
		{
			return headers.IndexOf (header);
		}

		/// <summary>
		/// Inserts the specified header at the given index.
		/// </summary>
		/// <param name="index">The index to insert the header.</param>
		/// <param name="header">The header.</param>
		public void Insert (int index, Header header)
		{
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException ("index");

			if (header == null)
				throw new ArgumentNullException ("header");

			// update the lookup table
			Header hdr;
			if (table.TryGetValue (header.Field, out hdr)) {
				int idx = headers.IndexOf (hdr);

				if (idx >= index)
					table[header.Field] = header;
			} else {
				table.Add (header.Field, header);
			}

			headers.Insert (index, header);
			header.Changed += OnHeaderChanged;

			OnChanged (header, HeaderListChangedAction.Added);
		}

		/// <summary>
		/// Removes the header at the specified index.
		/// </summary>
		/// <param name="index">The index.</param>
		public void RemoveAt (int index)
		{
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException ("index");

			var header = headers[index];

			header.Changed -= OnHeaderChanged;

			if (table[header.Field] == header) {
				table.Remove (header.Field);

				// find the next matching header and add it to the lookup table
				for (int i = index + 1; i < headers.Count; i++) {
					if (icase.Compare (headers[i].Field, header.Field) == 0) {
						table.Add (headers[i].Field, headers[i]);
						break;
					}
				}
			}

			headers.RemoveAt (index);

			OnChanged (header, HeaderListChangedAction.Removed);
		}

		/// <summary>
		/// Gets or sets the <see cref="MimeKit.Header"/> at the specified index.
		/// </summary>
		/// <param name="index">The index.</param>
		public Header this [int index] {
			get {
				return headers[index];
			}
			set {
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ("index");

				var header = headers[index];

				if (header == value)
					return;

				header.Changed -= OnHeaderChanged;
				value.Changed += OnHeaderChanged;

				if (icase.Compare (header.Field, value.Field) == 0) {
					// replace the old header with the new one
					if (table[header.Field] == header)
						table[header.Field] = value;
				} else {
					// update the table for the header field being replaced
					if (table[header.Field] == header) {
						table.Remove (header.Field);

						// find the next matching header and add it to the lookup table
						for (int i = index + 1; i < headers.Count; i++) {
							if (icase.Compare (headers[i].Field, header.Field) == 0) {
								table.Add (headers[i].Field, headers[i]);
								break;
							}
						}
					}

					// update the table for the header being set
					if (table.TryGetValue (value.Field, out header)) {
						int idx = headers.IndexOf (header);

						if (idx > index)
							table[header.Field] = value;
					} else {
						table.Add (value.Field, value);
					}
				}

				headers[index] = value;

				OnChanged (header, HeaderListChangedAction.Removed);
				OnChanged (value, HeaderListChangedAction.Added);
			}
		}

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Gets an enumerator for the list of headers.
		/// </summary>
		/// <returns>The enumerator.</returns>
		public IEnumerator<Header> GetEnumerator ()
		{
			return headers.GetEnumerator ();
		}

		#endregion

		#region IEnumerable implementation

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return headers.GetEnumerator ();
		}

		#endregion

		public event EventHandler<HeaderListChangedEventArgs> Changed;

		void OnHeaderChanged (object sender, EventArgs args)
		{
			OnChanged ((Header) sender, HeaderListChangedAction.Changed);
		}

		void OnChanged (Header header, HeaderListChangedAction action)
		{
			if (Changed != null)
				Changed (this, new HeaderListChangedEventArgs (header, action));
		}

		internal bool TryGetHeader (string field, out Header header)
		{
			if (field == null)
				throw new ArgumentNullException ("field");

			return table.TryGetValue (field, out header);
		}
	}
}

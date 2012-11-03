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
using System.Collections;
using System.Collections.Generic;

namespace MimeKit {
	public sealed class HeaderList : ICollection<Header>, IList<Header>
	{
		static readonly StringComparer icase = StringComparer.InvariantCultureIgnoreCase;

		// this table references the first header of each field
		Dictionary<string, Header> table;
		List<Header> headers;

		public HeaderList ()
		{
			table = new Dictionary<string, Header> (icase);
			headers = new List<Header> ();
		}

		public void Add (string field, string value)
		{
			Add (new Header (field, value));
		}

		public void Insert (int index, string field, string value)
		{
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException ("index");

			Insert (index, new Header (field, value));
		}

		public bool Remove (string field)
		{
			if (field == null)
				throw new ArgumentNullException ("field");

			Header header;
			if (!table.TryGetValue (field, out header))
				return false;

			return Remove (header);
		}

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

		#region ICollection implementation

		public int Count {
			get { return headers.Count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

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

		public void Clear ()
		{
			foreach (var header in headers)
				header.Changed -= OnHeaderChanged;

			headers.Clear ();
			table.Clear ();

			OnChanged (null, HeaderListChangedAction.Cleared);
		}

		public bool Contains (Header header)
		{
			return headers.Contains (header);
		}

		public void CopyTo (Header[] array, int arrayIndex)
		{
			headers.CopyTo (array, arrayIndex);
		}

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

		public int IndexOf (Header header)
		{
			return headers.IndexOf (header);
		}

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
	}
}

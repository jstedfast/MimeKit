//
// DomainList.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Jeffrey Stedfast
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
	public sealed class DomainList : IList<string>
	{
		List<string> list;

		public DomainList (IEnumerable<string> domains)
		{
			list = new List<string> (domains);
		}

		public DomainList ()
		{
			list = new List<string> ();
		}

		#region IList implementation

		public int IndexOf (string domain)
		{
			return list.IndexOf (domain);
		}

		public void Insert (int index, string domain)
		{
			if (domain == null)
				throw new ArgumentNullException ("domain");

			list.Insert (index, domain);
			OnChanged ();
		}

		public void RemoveAt (int index)
		{
			list.RemoveAt (index);
			OnChanged ();
		}

		public string this [int index] {
			get { return list[index]; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (list[index] == value)
					return;

				list[index] = value;
				OnChanged ();
			}
		}

		#endregion

		#region ICollection implementation

		public void Add (string domain)
		{
			if (domain == null)
				throw new ArgumentNullException ("domain");

			list.Add (domain);
			OnChanged ();
		}

		public void Clear ()
		{
			list.Clear ();
			OnChanged ();
		}

		public bool Contains (string domain)
		{
			return list.Contains (domain);
		}

		public void CopyTo (string[] array, int arrayIndex)
		{
			list.CopyTo (array, arrayIndex);
		}

		public bool Remove (string domain)
		{
			if (list.Remove (domain)) {
				OnChanged ();
				return true;
			}

			return false;
		}

		public int Count {
			get { return list.Count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		#endregion

		#region IEnumerable implementation

		public IEnumerator<string> GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		#endregion

		#region IEnumerable implementation

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		#endregion

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			for (int i = 0; i < list.Count; i++) {
				if (string.IsNullOrEmpty (list[i]) && sb.Length == 0)
					continue;

				if (sb.Length > 0)
					sb.Append (',');

				if (!string.IsNullOrEmpty (list[i]))
					sb.Append ('@');

				sb.Append (list[i]);
			}

			return sb.ToString ();
		}

		public event EventHandler Changed;

		void OnChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}
	}
}

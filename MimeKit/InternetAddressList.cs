//
// InternetAddressList.cs
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
using System.Collections;
using System.Collections.Generic;

namespace MimeKit {
	public class InternetAddressList : IList<InternetAddress>
	{
		List<InternetAddress> list = new List<InternetAddress> ();

		public InternetAddressList (IEnumerable<InternetAddress> addresses)
		{
			foreach (var address in addresses) {
				address.Changed += AddressChanged;
				list.Add (address);
			}
		}

		public InternetAddressList ()
		{
		}

		#region IList implementation

		public int IndexOf (InternetAddress address)
		{
			return list.IndexOf (address);
		}

		public void Insert (int index, InternetAddress address)
		{
			if (address == null)
				throw new ArgumentNullException ("address");

			address.Changed += AddressChanged;
			list.Insert (index, address);
		}

		public void RemoveAt (int index)
		{
			if (index < 0 || index >= list.Count)
				throw new ArgumentOutOfRangeException ("index");

			list[index].Changed -= AddressChanged;
			list.RemoveAt (index);
		}

		public InternetAddress this [int index] {
			get { return list[index]; }
			set { list[index] = value; }
		}

		#endregion

		#region ICollection implementation

		public void Add (InternetAddress address)
		{
			if (address == null)
				throw new ArgumentNullException ("address");

			address.Changed += AddressChanged;
			list.Add (address);
		}

		public void Clear ()
		{
			foreach (var address in list)
				address.Changed -= AddressChanged;

			list.Clear ();
		}

		public bool Contains (InternetAddress address)
		{
			return list.Contains (address);
		}

		public void CopyTo (InternetAddress[] array, int arrayIndex)
		{
			list.CopyTo (array, arrayIndex);
		}

		public bool Remove (InternetAddress address)
		{
			if (address == null)
				throw new ArgumentNullException ("address");

			if (list.Remove (address)) {
				address.Changed -= AddressChanged;
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

		public IEnumerator<InternetAddress> GetEnumerator ()
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

		public event EventHandler Changed;

		protected virtual void OnChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}

		void AddressChanged (object sender, EventArgs e)
		{
			OnChanged ();
		}
	}
}


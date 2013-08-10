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
using System.Text;
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
			OnChanged ();
		}

		public void RemoveAt (int index)
		{
			if (index < 0 || index >= list.Count)
				throw new ArgumentOutOfRangeException ("index");

			list[index].Changed -= AddressChanged;
			list.RemoveAt (index);
			OnChanged ();
		}

		public InternetAddress this [int index] {
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

		public void Add (InternetAddress address)
		{
			if (address == null)
				throw new ArgumentNullException ("address");

			address.Changed += AddressChanged;
			list.Add (address);
			OnChanged ();
		}

		public void Clear ()
		{
			foreach (var address in list)
				address.Changed -= AddressChanged;

			list.Clear ();
			OnChanged ();
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

		internal static bool TryParse (byte[] text, ref int index, int endIndex, bool isGroup, bool throwOnError, out InternetAddressList addresses)
		{
			List<InternetAddress> list = new List<InternetAddress> ();
			InternetAddress address;

			addresses = null;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			while (index < endIndex) {
				if (isGroup && text[index] == (byte) ';')
					break;

				if (!InternetAddress.TryParse (text, ref index, endIndex, throwOnError, out address)) {
					// skip this address...
					while (index < endIndex && text[index] != (byte) ',' && (!isGroup || text[index] != (byte) ';'))
						index++;
				} else {
					list.Add (address);
				}

				// Note: we loop here in case there are any null addresses between commas
				do {
					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;

					if (index >= endIndex || text[index] != (byte) ',')
						break;

					index++;
				} while (true);
			}

			addresses = new InternetAddressList (list);

			return true;
		}

		public static bool TryParse (byte[] text, int startIndex, int count, out InternetAddressList addresses)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			if (startIndex < 0 || startIndex >= text.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (count < 0 || startIndex + count >= text.Length)
				throw new ArgumentOutOfRangeException ("count");

			int index = startIndex;

			return TryParse (text, ref index, startIndex + count, false, false, out addresses);
		}

		public static bool TryParse (byte[] text, int startIndex, out InternetAddressList addresses)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			if (startIndex < 0 || startIndex >= text.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			int index = startIndex;

			return TryParse (text, ref index, text.Length, false, false, out addresses);
		}

		public static bool TryParse (byte[] text, out InternetAddressList addresses)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			int index = 0;

			return TryParse (text, ref index, text.Length, false, false, out addresses);
		}

		public static bool TryParse (string text, out InternetAddressList addresses)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			var buffer = Encoding.UTF8.GetBytes (text);
			int index = 0;

			return TryParse (buffer, ref index, buffer.Length, false, false, out addresses);
		}

		public static InternetAddressList Parse (byte[] text, int startIndex, int count)
		{
			InternetAddressList addresses;
			int index = startIndex;

			if (startIndex < 0 || startIndex > text.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (count < 0 || startIndex + count > text.Length)
				throw new ArgumentOutOfRangeException ("count");

			TryParse (text, ref index, startIndex + count, false, true, out addresses);

			return addresses;
		}

		public static InternetAddressList Parse (byte[] text, int startIndex)
		{
			return Parse (text, startIndex, text.Length - startIndex);
		}

		public static InternetAddressList Parse (byte[] text)
		{
			return Parse (text, 0, text.Length);
		}

		public static InternetAddressList Parse (string text)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			var buffer = Encoding.UTF8.GetBytes (text);
			InternetAddressList addresses;
			int index;

			TryParse (buffer, ref index, buffer.Length, false, true, out addresses);

			return addresses;
		}
	}
}

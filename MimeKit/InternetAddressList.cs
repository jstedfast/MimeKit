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

using MimeKit.Utils;

namespace MimeKit {
	public class InternetAddressList : IList<InternetAddress>, IEquatable<InternetAddressList>
	{
		readonly List<InternetAddress> list = new List<InternetAddress> ();

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.InternetAddressList"/> class.
		/// </summary>
		/// <param name="addresses">An initial list of addresses.</param>
		public InternetAddressList (IEnumerable<InternetAddress> addresses)
		{
			foreach (var address in addresses) {
				address.Changed += AddressChanged;
				list.Add (address);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.InternetAddressList"/> class.
		/// </summary>
		public InternetAddressList ()
		{
		}

		#region IList implementation

		/// <summary>
		/// Gets the index of the specified address.
		/// </summary>
		/// <returns>The index of the specified address if found; otherwise <c>-1</c>.</returns>
		/// <param name="address">The address to get the index of.</param>
		public int IndexOf (InternetAddress address)
		{
			return list.IndexOf (address);
		}

		/// <summary>
		/// Inserts the address at the specified index.
		/// </summary>
		/// <param name="index">The index to insert the address.</param>
		/// <param name="address">The address.</param>
		public void Insert (int index, InternetAddress address)
		{
			if (address == null)
				throw new ArgumentNullException ("address");

			address.Changed += AddressChanged;
			list.Insert (index, address);
			OnChanged ();
		}

		/// <summary>
		/// Removes the address at the specified index.
		/// </summary>
		/// <param name="index">The index of the address to remove.</param>
		public void RemoveAt (int index)
		{
			if (index < 0 || index >= list.Count)
				throw new ArgumentOutOfRangeException ("index");

			list[index].Changed -= AddressChanged;
			list.RemoveAt (index);
			OnChanged ();
		}

		/// <summary>
		/// Gets or sets the <see cref="MimeKit.InternetAddressList"/> at the specified index.
		/// </summary>
		/// <param name="index">The idnex of the addres to get or set.</param>
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

		/// <summary>
		/// Adds the specified address.
		/// </summary>
		/// <param name="address">The address.</param>
		public void Add (InternetAddress address)
		{
			if (address == null)
				throw new ArgumentNullException ("address");

			address.Changed += AddressChanged;
			list.Add (address);
			OnChanged ();
		}

		/// <summary>
		/// Adds a collection of addresses.
		/// </summary>
		/// <param name="addresses">A colelction of addresses.</param>
		public void AddRange (IEnumerable<InternetAddress> addresses)
		{
			if (addresses == null)
				throw new ArgumentNullException ("addresses");

			foreach (var address in addresses) {
				address.Changed += AddressChanged;
				list.Add (address);
			}

			OnChanged ();
		}

		/// <summary>
		/// Clears the <see cref="MimeKit.InternetAddressList"/>.
		/// </summary>
		public void Clear ()
		{
			foreach (var address in list)
				address.Changed -= AddressChanged;

			list.Clear ();
			OnChanged ();
		}

		/// <summary>
		/// Checks if the <see cref="MimeKit.InternetAddressList"/> contains the specified address.
		/// </summary>
		/// <returns><value>true</value> if the requested address exists;
		/// otherwise <value>false</value>.</returns>
		/// <param name="address">The address.</param>
		public bool Contains (InternetAddress address)
		{
			return list.Contains (address);
		}

		/// <summary>
		/// Copies all of the addresses in the <see cref="MimeKit.InternetAddressList"/> to the specified array.
		/// </summary>
		/// <param name="array">The array to copy the addresses to.</param>
		/// <param name="arrayIndex">The index into the array.</param>
		public void CopyTo (InternetAddress[] array, int arrayIndex)
		{
			list.CopyTo (array, arrayIndex);
		}

		/// <summary>
		/// Removes the specified address.
		/// </summary>
		/// <param name="address">The address.</param>
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

		/// <summary>
		/// Gets the number of addresses in the <see cref="MimeKit.InternetAddressList"/>.
		/// </summary>
		/// <value>The number of addresses.</value>
		public int Count {
			get { return list.Count; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
		public bool IsReadOnly {
			get { return false; }
		}

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Gets an enumerator for the list of addresses.
		/// </summary>
		/// <returns>The enumerator.</returns>
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

		#region IEquatable implementation

		/// <summary>
		/// Determines whether the specified <see cref="MimeKit.InternetAddressList"/> is equal to the current <see cref="MimeKit.InternetAddressList"/>.
		/// </summary>
		/// <param name="other">The <see cref="MimeKit.InternetAddressList"/> to compare with the current <see cref="MimeKit.InternetAddressList"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="MimeKit.InternetAddressList"/> is equal to the current
		/// <see cref="MimeKit.InternetAddressList"/>; otherwise, <c>false</c>.</returns>
		public bool Equals (InternetAddressList other)
		{
			if (other == null)
				return false;

			if (other.Count != Count)
				return false;

			for (int i = 0; i < Count; i++) {
				if (!list[i].Equals (other[i]))
					return false;
			}

			return true;
		}

		#endregion

		internal void Encode (FormatOptions options, StringBuilder builder, ref int lineLength)
		{
			foreach (var addr in list)
				addr.Encode (options, builder, ref lineLength);
		}

		/// <summary>
		/// Serializes the <see cref="MimeKit.InternetAddressList"/> to a string, optionally encoding it for transport.
		/// </summary>
		/// <returns>A string representing the <see cref="MimeKit.InternetAddressList"/>.</returns>
		/// <param name="encode">If set to <c>true</c>, the <see cref="MimeKit.InternetAddressList"/> will be encoded.</param>
		public string ToString (bool encode)
		{
			var builder = new StringBuilder ();

			if (encode) {
				int lineLength = 0;

				Encode (FormatOptions.Default, builder, ref lineLength);

				return builder.ToString ();
			}

			for (int i = 0; i < list.Count; i++) {
				if (i > 0)
					builder.Append (", ");

				builder.Append (list[i].ToString ());
			}

			return builder.ToString ();
		}

		/// <summary>
		/// Serializes the <see cref="MimeKit.InternetAddressList"/> to a string suitable for display.
		/// </summary>
		/// <returns>A string representing the <see cref="MimeKit.InternetAddressList"/>.</returns>
		public override string ToString ()
		{
			return ToString (false);
		}

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

		internal static bool TryParse (ParserOptions options, byte[] text, ref int index, int endIndex, bool isGroup, bool throwOnError, out List<InternetAddress> addresses)
		{
			List<InternetAddress> list = new List<InternetAddress> ();
			InternetAddress address;

			addresses = null;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			while (index < endIndex) {
				if (isGroup && text[index] == (byte) ';')
					break;

				if (!InternetAddress.TryParse (options, text, ref index, endIndex, throwOnError, out address)) {
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

			addresses = list;

			return true;
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.InternetAddressList"/> instance.
		/// </summary>
		/// <returns><c>true</c>, if the address list was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="addresses">The parsed addresses.</param>
		public static bool TryParse (ParserOptions options, byte[] buffer, int startIndex, int length, out InternetAddressList addresses)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (startIndex < 0 || startIndex >= buffer.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (length < 0 || startIndex + length >= buffer.Length)
				throw new ArgumentOutOfRangeException ("length");

			List<InternetAddress> addrlist;
			int index = startIndex;

			if (!TryParse (options, buffer, ref index, startIndex + length, false, false, out addrlist)) {
				addresses = null;
				return false;
			}

			addresses = new InternetAddressList (addrlist);

			return true;
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.InternetAddressList"/> instance.
		/// </summary>
		/// <returns><c>true</c>, if the address list was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="addresses">The parsed addresses.</param>
		public static bool TryParse (byte[] buffer, int startIndex, int length, out InternetAddressList addresses)
		{
			return TryParse (ParserOptions.Default, buffer, startIndex, length, out addresses);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.InternetAddressList"/> instance.
		/// </summary>
		/// <returns><c>true</c>, if the address list was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="addresses">The parsed addresses.</param>
		public static bool TryParse (ParserOptions options, byte[] buffer, int startIndex, out InternetAddressList addresses)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (startIndex < 0 || startIndex >= buffer.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			List<InternetAddress> addrlist;
			int index = startIndex;

			if (!TryParse (options, buffer, ref index, buffer.Length, false, false, out addrlist)) {
				addresses = null;
				return false;
			}

			addresses = new InternetAddressList (addrlist);

			return true;
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.InternetAddressList"/> instance.
		/// </summary>
		/// <returns><c>true</c>, if the address list was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="addresses">The parsed addresses.</param>
		public static bool TryParse (byte[] buffer, int startIndex, out InternetAddressList addresses)
		{
			return TryParse (ParserOptions.Default, buffer, startIndex, out addresses);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.InternetAddressList"/> instance.
		/// </summary>
		/// <returns><c>true</c>, if the address list was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="addresses">The parsed addresses.</param>
		public static bool TryParse (ParserOptions options, byte[] buffer, out InternetAddressList addresses)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			List<InternetAddress> addrlist;
			int index = 0;

			if (!TryParse (options, buffer, ref index, buffer.Length, false, false, out addrlist)) {
				addresses = null;
				return false;
			}

			addresses = new InternetAddressList (addrlist);

			return true;
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.InternetAddressList"/> instance.
		/// </summary>
		/// <returns><c>true</c>, if the address list was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="addresses">The parsed addresses.</param>
		public static bool TryParse (byte[] buffer, out InternetAddressList addresses)
		{
			return TryParse (ParserOptions.Default, buffer, out addresses);
		}

		/// <summary>
		/// Tries to parse the given text into a new <see cref="MimeKit.InternetAddressList"/> instance.
		/// </summary>
		/// <returns><c>true</c>, if the address list was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="text">The text.</param>
		/// <param name="addresses">The parsed addresses.</param>
		public static bool TryParse (ParserOptions options, string text, out InternetAddressList addresses)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (text == null)
				throw new ArgumentNullException ("text");

			var buffer = Encoding.UTF8.GetBytes (text);
			List<InternetAddress> addrlist;
			int index = 0;

			if (!TryParse (options, buffer, ref index, buffer.Length, false, false, out addrlist)) {
				addresses = null;
				return false;
			}

			addresses = new InternetAddressList (addrlist);

			return true;
		}

		/// <summary>
		/// Tries to parse the given text into a new <see cref="MimeKit.InternetAddressList"/> instance.
		/// </summary>
		/// <returns><c>true</c>, if the address list was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="text">The text.</param>
		/// <param name="addresses">The parsed addresses.</param>
		public static bool TryParse (string text, out InternetAddressList addresses)
		{
			return TryParse (ParserOptions.Default, text, out addresses);
		}

		/// <summary>
		/// Parses the given input buffer into a new <see cref="MimeKit.InternetAddressList"/> instance.
		/// </summary>
		/// <returns>The parsed <see cref="MimeKit.InternetAddressList"/>.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		public static InternetAddressList Parse (ParserOptions options, byte[] buffer, int startIndex, int length)
		{
			List<InternetAddress> addrlist;
			int index = startIndex;

			if (options == null)
				throw new ArgumentNullException ("options");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (length < 0 || startIndex + length > buffer.Length)
				throw new ArgumentOutOfRangeException ("length");

			TryParse (options, buffer, ref index, startIndex + length, false, true, out addrlist);

			return new InternetAddressList (addrlist);
		}

		/// <summary>
		/// Parses the given input buffer into a new <see cref="MimeKit.InternetAddressList"/> instance.
		/// </summary>
		/// <returns>The parsed <see cref="MimeKit.InternetAddressList"/>.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		public static InternetAddressList Parse (byte[] buffer, int startIndex, int length)
		{
			return Parse (ParserOptions.Default, buffer, startIndex, length);
		}

		/// <summary>
		/// Parses the given input buffer into a new <see cref="MimeKit.InternetAddressList"/> instance.
		/// </summary>
		/// <returns>The parsed <see cref="MimeKit.InternetAddressList"/>.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		public static InternetAddressList Parse (ParserOptions options, byte[] buffer, int startIndex)
		{
			List<InternetAddress> addrlist;
			int index = startIndex;

			if (options == null)
				throw new ArgumentNullException ("options");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			TryParse (options, buffer, ref index, buffer.Length, false, true, out addrlist);

			return new InternetAddressList (addrlist);
		}

		/// <summary>
		/// Parses the given input buffer into a new <see cref="MimeKit.InternetAddressList"/> instance.
		/// </summary>
		/// <returns>The parsed <see cref="MimeKit.InternetAddressList"/>.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		public static InternetAddressList Parse (byte[] buffer, int startIndex)
		{
			return Parse (ParserOptions.Default, buffer, startIndex);
		}

		/// <summary>
		/// Parses the given input buffer into a new <see cref="MimeKit.InternetAddressList"/> instance.
		/// </summary>
		/// <returns>The parsed <see cref="MimeKit.InternetAddressList"/>.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		public static InternetAddressList Parse (ParserOptions options, byte[] buffer)
		{
			List<InternetAddress> addrlist;
			int index = 0;

			if (options == null)
				throw new ArgumentNullException ("options");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			TryParse (options, buffer, ref index, buffer.Length, false, true, out addrlist);

			return new InternetAddressList (addrlist);
		}

		/// <summary>
		/// Parses the given input buffer into a new <see cref="MimeKit.InternetAddressList"/> instance.
		/// </summary>
		/// <returns>The parsed <see cref="MimeKit.InternetAddressList"/>.</returns>
		/// <param name="buffer">The input buffer.</param>
		public static InternetAddressList Parse (byte[] buffer)
		{
			return Parse (ParserOptions.Default, buffer);
		}

		/// <summary>
		/// Parses the given text into a new <see cref="MimeKit.InternetAddressList"/> instance.
		/// </summary>
		/// <returns>The parsed <see cref="MimeKit.InternetAddressList"/>.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="text">The text.</param>
		public static InternetAddressList Parse (ParserOptions options, string text)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (text == null)
				throw new ArgumentNullException ("text");

			var buffer = Encoding.UTF8.GetBytes (text);
			List<InternetAddress> addrlist;
			int index = 0;

			TryParse (options, buffer, ref index, buffer.Length, false, true, out addrlist);

			return new InternetAddressList (addrlist);
		}

		/// <summary>
		/// Parses the given text into a new <see cref="MimeKit.InternetAddressList"/> instance.
		/// </summary>
		/// <returns>The parsed <see cref="MimeKit.InternetAddressList"/>.</returns>
		/// <param name="text">The text.</param>
		public static InternetAddressList Parse (string text)
		{
			return Parse (ParserOptions.Default, text);
		}
	}
}

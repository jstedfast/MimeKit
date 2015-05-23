//
// InternetAddressList.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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

#if ENABLE_SNM
using System.Net.Mail;
#endif

using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// A list of email addresses. 
	/// </summary>
	/// <remarks>
	/// <para>An <see cref="InternetAddressList"/> may contain any number of addresses of any type
	/// defined by the original Internet Message specification.</para>
	/// <para>There are effectively two (2) types of addresses: mailboxes and groups.</para>
	/// <para>Mailbox addresses are what are most commonly known as email addresses and are
	/// represented by the <see cref="MailboxAddress"/> class.</para>
	/// <para>Group addresses are themselves lists of addresses and are represented by the
	/// <see cref="GroupAddress"/> class. While rare, it is still important to handle these
	/// types of addresses. They typically only contain mailbox addresses, but may also
	/// contain other group addresses.</para>
	/// </remarks>
	public class InternetAddressList : IList<InternetAddress>, IEquatable<InternetAddressList>
	{
		readonly List<InternetAddress> list = new List<InternetAddress> ();

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.InternetAddressList"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="InternetAddressList"/> containing the supplied addresses.
		/// </remarks>
		/// <param name="addresses">An initial list of addresses.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="addresses"/> is <c>null</c>.
		/// </exception>
		public InternetAddressList (IEnumerable<InternetAddress> addresses)
		{
			if (addresses == null)
				throw new ArgumentNullException ("addresses");

			foreach (var address in addresses) {
				address.Changed += AddressChanged;
				list.Add (address);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.InternetAddressList"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new, empty, <see cref="InternetAddressList"/>.
		/// </remarks>
		public InternetAddressList ()
		{
		}

		/// <summary>
		/// Recursively gets all of the mailboxes contained within the <see cref="MimeKit.InternetAddressList"/>.
		/// </summary>
		/// <remarks>
		/// This API is useful for collecting a flattened list of <see cref="MimeKit.MailboxAddress"/>
		/// recipients for use with sending via SMTP or for encrypting via S/MIME or PGP/MIME.
		/// </remarks>
		/// <value>The mailboxes.</value>
		public IEnumerable<MailboxAddress> Mailboxes {
			get {
				foreach (var address in list) {
					var group = address as GroupAddress;

					if (group != null) {
						foreach (var mailbox in group.Members.Mailboxes)
							yield return mailbox;
					} else {
						yield return (MailboxAddress) address;
					}
				}

				yield break;
			}
		}

		#region IList implementation

		/// <summary>
		/// Gets the index of the specified address.
		/// </summary>
		/// <remarks>
		/// Finds the index of the specified address, if it exists.
		/// </remarks>
		/// <returns>The index of the specified address if found; otherwise <c>-1</c>.</returns>
		/// <param name="address">The address to get the index of.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="address"/> is <c>null</c>.
		/// </exception>
		public int IndexOf (InternetAddress address)
		{
			if (address == null)
				throw new ArgumentNullException ("address");

			return list.IndexOf (address);
		}

		/// <summary>
		/// Inserts the address at the specified index.
		/// </summary>
		/// <remarks>
		/// Inserts the address at the specified index in the list.
		/// </remarks>
		/// <param name="index">The index to insert the address.</param>
		/// <param name="address">The address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="address"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
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
		/// <remarks>
		/// Removes the address at the specified index.
		/// </remarks>
		/// <param name="index">The index of the address to remove.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public void RemoveAt (int index)
		{
			if (index < 0 || index >= list.Count)
				throw new ArgumentOutOfRangeException ("index");

			list[index].Changed -= AddressChanged;
			list.RemoveAt (index);
			OnChanged ();
		}

		/// <summary>
		/// Gets or sets the <see cref="MimeKit.InternetAddress"/> at the specified index.
		/// </summary>
		/// <remarks>
		/// Gets or sets the <see cref="MimeKit.InternetAddress"/> at the specified index.
		/// </remarks>
		/// <value>The internet address at the specified index.</value>
		/// <param name="index">The index of the address to get or set.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
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
		/// Gets the number of addresses in the <see cref="MimeKit.InternetAddressList"/>.
		/// </summary>
		/// <remarks>
		/// Indicates the number of addresses in the list.
		/// </remarks>
		/// <value>The number of addresses.</value>
		public int Count {
			get { return list.Count; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <remarks>
		/// A <see cref="InternetAddressList"/> is never read-only.
		/// </remarks>
		/// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
		public bool IsReadOnly {
			get { return false; }
		}

		/// <summary>
		/// Adds the specified address.
		/// </summary>
		/// <remarks>
		/// Adds the specified address to the end of the address list.
		/// </remarks>
		/// <param name="address">The address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="address"/> is <c>null</c>.
		/// </exception>
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
		/// <remarks>
		/// Adds a range of addresses to the end of the address list.
		/// </remarks>
		/// <param name="addresses">A colelction of addresses.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="addresses"/> is <c>null</c>.
		/// </exception>
		public void AddRange (IEnumerable<InternetAddress> addresses)
		{
			if (addresses == null)
				throw new ArgumentNullException ("addresses");

			bool changed = false;

			foreach (var address in addresses) {
				address.Changed += AddressChanged;
				list.Add (address);
				changed = true;
			}

			if (changed)
				OnChanged ();
		}

		/// <summary>
		/// Clears the address list.
		/// </summary>
		/// <remarks>
		/// Removes all of the addresses from the list.
		/// </remarks>
		public void Clear ()
		{
			if (list.Count == 0)
				return;

			for (int i = 0; i < list.Count; i++)
				list[i].Changed -= AddressChanged;

			list.Clear ();
			OnChanged ();
		}

		/// <summary>
		/// Checks if the <see cref="MimeKit.InternetAddressList"/> contains the specified address.
		/// </summary>
		/// <remarks>
		/// Determines whether or not the address list contains the specified address.
		/// </remarks>
		/// <returns><value>true</value> if the specified address exists;
		/// otherwise <value>false</value>.</returns>
		/// <param name="address">The address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="address"/> is <c>null</c>.
		/// </exception>
		public bool Contains (InternetAddress address)
		{
			if (address == null)
				throw new ArgumentNullException ("address");

			return list.Contains (address);
		}

		/// <summary>
		/// Copies all of the addresses in the <see cref="MimeKit.InternetAddressList"/> to the specified array.
		/// </summary>
		/// <remarks>
		/// Copies all of the addresses within the <see cref="InternetAddressList"/> into the array,
		/// starting at the specified array index.
		/// </remarks>
		/// <param name="array">The array to copy the addresses to.</param>
		/// <param name="arrayIndex">The index into the array.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="array"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex"/> is out of range.
		/// </exception>
		public void CopyTo (InternetAddress[] array, int arrayIndex)
		{
			list.CopyTo (array, arrayIndex);
		}

		/// <summary>
		/// Removes the specified address.
		/// </summary>
		/// <remarks>
		/// Removes the specified address.
		/// </remarks>
		/// <returns><value>true</value> if the address was removed; otherwise <value>false</value>.</returns>
		/// <param name="address">The address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="address"/> is <c>null</c>.
		/// </exception>
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

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Gets an enumerator for the list of addresses.
		/// </summary>
		/// <remarks>
		/// Gets an enumerator for the list of addresses.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		public IEnumerator<InternetAddress> GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Gets an enumerator for the list of addresses.
		/// </summary>
		/// <remarks>
		/// Gets an enumerator for the list of addresses.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		#endregion

		#region IEquatable implementation

		/// <summary>
		/// Determines whether the specified <see cref="MimeKit.InternetAddressList"/> is equal to the current <see cref="MimeKit.InternetAddressList"/>.
		/// </summary>
		/// <remarks>
		/// Determines whether the specified <see cref="MimeKit.InternetAddressList"/> is equal to the current <see cref="MimeKit.InternetAddressList"/>.
		/// </remarks>
		/// <param name="other">The <see cref="MimeKit.InternetAddressList"/> to compare with the current <see cref="MimeKit.InternetAddressList"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="MimeKit.InternetAddressList"/> is equal to the current
		/// <see cref="MimeKit.InternetAddressList"/>; otherwise, <c>false</c>.</returns>
		public bool Equals (InternetAddressList other)
		{
			if (other == null)
				return false;

			if (other.Count != Count)
				return false;

			var otherSorted = new List<InternetAddress> (other);
			otherSorted.Sort ();

			var sorted = new List<InternetAddress> (this);
			sorted.Sort ();

			for (int i = 0; i < sorted.Count; i++) {
				if (!sorted[i].Equals (otherSorted[i]))
					return false;
			}

			return true;
		}

		#endregion

		internal void Encode (FormatOptions options, StringBuilder builder, ref int lineLength)
		{
			for (int i = 0; i < list.Count; i++) {
				if (i > 0)
					builder.Append (", ");

				list[i].Encode (options, builder, ref lineLength);
			}
		}

		/// <summary>
		/// Returns a string representation of the email addresses in the <see cref="InternetAddressList"/>,
		/// optionally encoding them for transport.
		/// </summary>
		/// <remarks>
		/// <para>If <paramref name="encode"/> is <c>true</c>, each address in the list will be encoded
		/// according to the rules defined in rfc2047.</para>
		/// <para>If there are multiple addresses in the list, they will be separated by a comma.</para>
		/// </remarks>
		/// <returns>A string representing the <see cref="InternetAddressList"/>.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="encode">If set to <c>true</c>, each <see cref="InternetAddress"/> in the list will be encoded.</param>
		public string ToString (FormatOptions options, bool encode)
		{
			var builder = new StringBuilder ();

			if (encode) {
				int lineLength = 0;

				Encode (options, builder, ref lineLength);

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
		/// Returns a string representation of the email addresses in the <see cref="InternetAddressList"/>,
		/// optionally encoding them for transport.
		/// </summary>
		/// <remarks>
		/// <para>If <paramref name="encode"/> is <c>true</c>, each address in the list will be encoded
		/// according to the rules defined in rfc2047.</para>
		/// <para>If there are multiple addresses in the list, they will be separated by a comma.</para>
		/// </remarks>
		/// <returns>A string representing the <see cref="InternetAddressList"/>.</returns>
		/// <param name="encode">If set to <c>true</c>, each <see cref="InternetAddress"/> in the list will be encoded.</param>
		public string ToString (bool encode)
		{
			return ToString (FormatOptions.GetDefault (), encode);
		}

		/// <summary>
		/// Returns a string representation of the email addresses in the <see cref="InternetAddressList"/>.
		/// </summary>
		/// <remarks>
		/// If there are multiple addresses in the list, they will be separated by a comma.
		/// </remarks>
		/// <returns>A string representing the <see cref="InternetAddressList"/>.</returns>
		public override string ToString ()
		{
			return ToString (FormatOptions.GetDefault (), false);
		}

		internal event EventHandler Changed;

		void OnChanged ()
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

			if (index == endIndex) {
				if (throwOnError)
					throw new ParseException ("No addresses found.", index, index);

				return false;
			}

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
		/// <remarks>
		/// Parses a list of addresses from the supplied buffer starting at the given index
		/// and spanning across the specified number of bytes.
		/// </remarks>
		/// <returns><c>true</c>, if the address list was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="addresses">The parsed addresses.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static bool TryParse (ParserOptions options, byte[] buffer, int startIndex, int length, out InternetAddressList addresses)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (length < 0 || length > (buffer.Length - startIndex))
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
		/// <remarks>
		/// Parses a list of addresses from the supplied buffer starting at the given index
		/// and spanning across the specified number of bytes.
		/// </remarks>
		/// <returns><c>true</c>, if the address list was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="addresses">The parsed addresses.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static bool TryParse (byte[] buffer, int startIndex, int length, out InternetAddressList addresses)
		{
			return TryParse (ParserOptions.Default, buffer, startIndex, length, out addresses);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.InternetAddressList"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a list of addresses from the supplied buffer starting at the specified index.
		/// </remarks>
		/// <returns><c>true</c>, if the address list was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="addresses">The parsed addresses.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> is out of range.
		/// </exception>
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
		/// <remarks>
		/// Parses a list of addresses from the supplied buffer starting at the specified index.
		/// </remarks>
		/// <returns><c>true</c>, if the address list was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="addresses">The parsed addresses.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> is out of range.
		/// </exception>
		public static bool TryParse (byte[] buffer, int startIndex, out InternetAddressList addresses)
		{
			return TryParse (ParserOptions.Default, buffer, startIndex, out addresses);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.InternetAddressList"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a list of addresses from the specified buffer.
		/// </remarks>
		/// <returns><c>true</c>, if the address list was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="addresses">The parsed addresses.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
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
		/// <remarks>
		/// Parses a list of addresses from the specified buffer.
		/// </remarks>
		/// <returns><c>true</c>, if the address list was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="addresses">The parsed addresses.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		public static bool TryParse (byte[] buffer, out InternetAddressList addresses)
		{
			return TryParse (ParserOptions.Default, buffer, out addresses);
		}

		/// <summary>
		/// Tries to parse the given text into a new <see cref="MimeKit.InternetAddressList"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a list of addresses from the specified text.
		/// </remarks>
		/// <returns><c>true</c>, if the address list was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="text">The text.</param>
		/// <param name="addresses">The parsed addresses.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="text"/> is <c>null</c>.</para>
		/// </exception>
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
		/// <remarks>
		/// Parses a list of addresses from the specified text.
		/// </remarks>
		/// <returns><c>true</c>, if the address list was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="text">The text.</param>
		/// <param name="addresses">The parsed addresses.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public static bool TryParse (string text, out InternetAddressList addresses)
		{
			return TryParse (ParserOptions.Default, text, out addresses);
		}

		/// <summary>
		/// Parses the given input buffer into a new <see cref="MimeKit.InternetAddressList"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a list of addresses from the supplied buffer starting at the given index
		/// and spanning across the specified number of bytes.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.InternetAddressList"/>.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// <paramref name="buffer"/> could not be parsed.
		/// </exception>
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

			if (length < 0 || length > (buffer.Length - startIndex))
				throw new ArgumentOutOfRangeException ("length");

			TryParse (options, buffer, ref index, startIndex + length, false, true, out addrlist);

			return new InternetAddressList (addrlist);
		}

		/// <summary>
		/// Parses the given input buffer into a new <see cref="MimeKit.InternetAddressList"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a list of addresses from the supplied buffer starting at the given index
		/// and spanning across the specified number of bytes.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.InternetAddressList"/>.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static InternetAddressList Parse (byte[] buffer, int startIndex, int length)
		{
			return Parse (ParserOptions.Default, buffer, startIndex, length);
		}

		/// <summary>
		/// Parses the given input buffer into a new <see cref="MimeKit.InternetAddressList"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a list of addresses from the supplied buffer starting at the specified index.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.InternetAddressList"/>.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/>is out of range.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// <paramref name="buffer"/> could not be parsed.
		/// </exception>
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
		/// <remarks>
		/// Parses a list of addresses from the supplied buffer starting at the specified index.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.InternetAddressList"/>.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> is out of range.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static InternetAddressList Parse (byte[] buffer, int startIndex)
		{
			return Parse (ParserOptions.Default, buffer, startIndex);
		}

		/// <summary>
		/// Parses the given input buffer into a new <see cref="MimeKit.InternetAddressList"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a list of addresses from the specified buffer.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.InternetAddressList"/>.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// <paramref name="buffer"/> could not be parsed.
		/// </exception>
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
		/// <remarks>
		/// Parses a list of addresses from the specified buffer.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.InternetAddressList"/>.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static InternetAddressList Parse (byte[] buffer)
		{
			return Parse (ParserOptions.Default, buffer);
		}

		/// <summary>
		/// Parses the given text into a new <see cref="MimeKit.InternetAddressList"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a list of addresses from the specified text.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.InternetAddressList"/>.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="text">The text.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="text"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// <paramref name="text"/> could not be parsed.
		/// </exception>
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
		/// <remarks>
		/// Parses a list of addresses from the specified text.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.InternetAddressList"/>.</returns>
		/// <param name="text">The text.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// <paramref name="text"/> could not be parsed.
		/// </exception>
		public static InternetAddressList Parse (string text)
		{
			return Parse (ParserOptions.Default, text);
		}

#if ENABLE_SNM
		/// <summary>
		/// Explicit cast to convert a <see cref="InternetAddressList"/> to a
		/// <see cref="System.Net.Mail.MailAddressCollection"/>.
		/// </summary>
		/// <remarks>
		/// Casts a <see cref="InternetAddressList"/> to a <see cref="System.Net.Mail.MailAddressCollection"/>
		/// in cases where you might want to make use of the System.Net.Mail APIs.
		/// </remarks>
		/// <returns>The equivalent <see cref="System.Net.Mail.MailAddressCollection"/>.</returns>
		/// <param name="addresses">The addresses.</param>
		/// <exception cref="System.InvalidCastException">
		/// <paramref name="addresses"/> contains one or more group addresses and cannot be converted.
		/// </exception>
		public static explicit operator MailAddressCollection (InternetAddressList addresses)
		{
			if (addresses == null)
				return null;

			var collection = new MailAddressCollection ();
			for (int i = 0; i < addresses.Count; i++) {
				if (addresses[i] is GroupAddress)
					throw new InvalidCastException ("Cannot cast a MailKit.GroupAddress to a System.Net.Mail.MailAddress.");

				var mailbox = (MailboxAddress) addresses[i];

				collection.Add ((MailAddress) mailbox);
			}

			return collection;
		}

		/// <summary>
		/// Explicit cast to convert a <see cref="System.Net.Mail.MailAddressCollection"/>
		/// to a <see cref="InternetAddressList"/>.
		/// </summary>
		/// <remarks>
		/// Casts a <see cref="System.Net.Mail.MailAddressCollection"/> to a <see cref="InternetAddressList"/>
		/// in cases where you might want to make use of the the superior MimeKit APIs.
		/// </remarks>
		/// <returns>The equivalent <see cref="InternetAddressList"/>.</returns>
		/// <param name="addresses">The mail address.</param>
		public static explicit operator InternetAddressList (MailAddressCollection addresses)
		{
			if (addresses == null)
				return null;

			var list = new InternetAddressList ();
			foreach (var address in addresses)
				list.Add ((MailboxAddress) address);

			return list;
		}
#endif
	}
}

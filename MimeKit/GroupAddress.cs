﻿//
// GroupAddress.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 Xamarin Inc. (www.xamarin.com)
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
using System.Linq;
using System.Text;
using System.Collections.Generic;

using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// An address group, as specified by rfc0822.
	/// </summary>
	/// <remarks>
	/// Group addresses are rarely used anymore. Typically, if you see a group address,
	/// it will be of the form: <c>"undisclosed-recipients: ;"</c>.
	/// </remarks>
	public class GroupAddress : InternetAddress
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.GroupAddress"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="GroupAddress"/> with the specified name and list of addresses. The
		/// specified text encoding is used when encoding the name according to the rules of rfc2047.
		/// </remarks>
		/// <param name="encoding">The character encoding to be used for encoding the name.</param>
		/// <param name="name">The name of the group.</param>
		/// <param name="addresses">A list of addresses.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="encoding"/> is <c>null</c>.
		/// </exception>
		public GroupAddress (Encoding encoding, string name, IEnumerable<InternetAddress> addresses) : base (encoding, name)
		{
			Members = new InternetAddressList (addresses);
			Members.Changed += MembersChanged;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.GroupAddress"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="GroupAddress"/> with the specified name and list of addresses.
		/// </remarks>
		/// <param name="name">The name of the group.</param>
		/// <param name="addresses">A list of addresses.</param>
		public GroupAddress (string name, IEnumerable<InternetAddress> addresses) : this (Encoding.UTF8, name, addresses)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.GroupAddress"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="GroupAddress"/> with the specified name. The specified
		/// text encoding is used when encoding the name according to the rules of rfc2047.
		/// </remarks>
		/// <param name="encoding">The character encoding to be used for encoding the name.</param>
		/// <param name="name">The name of the group.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="encoding"/> is <c>null</c>.
		/// </exception>
		public GroupAddress (Encoding encoding, string name) : base (encoding, name)
		{
			Members = new InternetAddressList ();
			Members.Changed += MembersChanged;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.GroupAddress"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="GroupAddress"/> with the specified name.
		/// </remarks>
		/// <param name="name">The name of the group.</param>
		public GroupAddress (string name) : this (Encoding.UTF8, name)
		{
		}

		/// <summary>
		/// Clone the group address.
		/// </summary>
		/// <remarks>
		/// Clones the group address.
		/// </remarks>
		/// <returns>The cloned group address.</returns>
		public override InternetAddress Clone ()
		{
			return new GroupAddress (Encoding, Name, Members.Select (x => x.Clone ()));
		}

		/// <summary>
		/// Gets the members of the group.
		/// </summary>
		/// <remarks>
		/// Represents the member addresses of the group. Typically the member addresses
		/// will be of the <see cref="MailboxAddress"/> variety, but it is possible
		/// for groups to contain other groups.
		/// </remarks>
		/// <value>The list of members.</value>
		public InternetAddressList Members {
			get; private set;
		}

		internal override void Encode (FormatOptions options, StringBuilder builder, bool firstToken, ref int lineLength)
		{
			if (!string.IsNullOrEmpty (Name)) {
				string name;

				if (!options.International) {
					var encoded = Rfc2047.EncodePhrase (options, Encoding, Name);
					name = Encoding.ASCII.GetString (encoded, 0, encoded.Length);
				} else {
					name = EncodeInternationalizedPhrase (Name);
				}

				if (lineLength + name.Length > options.MaxLineLength) {
					if (name.Length > options.MaxLineLength) {
						// we need to break up the name...
						builder.AppendFolded (options, firstToken, name, ref lineLength);
					} else {
						// the name itself is short enough to fit on a single line,
						// but only if we write it on a line by itself
						if (!firstToken && lineLength > 1) {
							builder.LineWrap (options);
							lineLength = 1;
						}

						lineLength += name.Length;
						builder.Append (name);
					}
				} else {
					// we can safely fit the name on this line...
					lineLength += name.Length;
					builder.Append (name);
				}
			}

			builder.Append (": ");
			lineLength += 2;

			Members.Encode (options, builder, false, ref lineLength);

			builder.Append (';');
			lineLength++;
		}

		/// <summary>
		/// Returns a string representation of the <see cref="GroupAddress"/>,
		/// optionally encoding it for transport.
		/// </summary>
		/// <remarks>
		/// Returns a string containing the formatted group of addresses. If the <paramref name="encode"/>
		/// parameter is <c>true</c>, then the name of the group and all member addresses will be encoded
		/// according to the rules defined in rfc2047, otherwise the names will not be encoded at all and
		/// will therefor only be suitable for display purposes.
		/// </remarks>
		/// <returns>A string representing the <see cref="GroupAddress"/>.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="encode">If set to <c>true</c>, the <see cref="GroupAddress"/> will be encoded.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="options"/> is <c>null</c>.
		/// </exception>
		public override string ToString (FormatOptions options, bool encode)
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			var builder = new StringBuilder ();

			if (encode) {
				int lineLength = 0;

				Encode (options, builder, true, ref lineLength);
			} else {
				builder.Append (Name);
				builder.Append (':');
				builder.Append (' ');

				for (int i = 0; i < Members.Count; i++) {
					if (i > 0)
						builder.Append (", ");

					builder.Append (Members[i]);
				}

				builder.Append (';');
			}

			return builder.ToString ();
		}

		#region IEquatable implementation

		/// <summary>
		/// Determines whether the specified <see cref="MimeKit.GroupAddress"/> is equal to the current <see cref="MimeKit.GroupAddress"/>.
		/// </summary>
		/// <remarks>
		/// Compares two group addresses to determine if they are identical or not.
		/// </remarks>
		/// <param name="other">The <see cref="MimeKit.GroupAddress"/> to compare with the current <see cref="MimeKit.GroupAddress"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="MimeKit.GroupAddress"/> is equal to the current
		/// <see cref="MimeKit.GroupAddress"/>; otherwise, <c>false</c>.</returns>
		public override bool Equals (InternetAddress other)
		{
			var group = other as GroupAddress;

			if (group == null)
				return false;

			return Name == group.Name && Members.Equals (group.Members);
		}

		#endregion

		void MembersChanged (object sender, EventArgs e)
		{
			OnChanged ();
		}

		static bool TryParse (ParserOptions options, byte[] text, ref int index, int endIndex, bool throwOnError, out GroupAddress group)
		{
			var flags = AddressParserFlags.AllowGroupAddress;
			InternetAddress address;

			if (throwOnError)
				flags |= AddressParserFlags.ThrowOnError;

			if (!InternetAddress.TryParse (options, text, ref index, endIndex, 0, flags, out address)) {
				group = null;
				return false;
			}

			group = (GroupAddress) address;

			return true;
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.GroupAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="GroupAddress"/>. If the address is not a group address or
		/// there is more than a single group address, then parsing will fail.
		/// </remarks>
		/// <returns><c>true</c>, if the address was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="group">The parsed group address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static bool TryParse (ParserOptions options, byte[] buffer, int startIndex, int length, out GroupAddress group)
		{
			ParseUtils.ValidateArguments (options, buffer, startIndex, length);

			int endIndex = startIndex + length;
			int index = startIndex;

			if (!TryParse (options, buffer, ref index, endIndex, false, out group))
				return false;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, false) || index != endIndex) {
				group = null;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.GroupAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="GroupAddress"/>. If the address is not a group address or
		/// there is more than a single group address, then parsing will fail.
		/// </remarks>
		/// <returns><c>true</c>, if the address was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="group">The parsed group address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static bool TryParse (byte[] buffer, int startIndex, int length, out GroupAddress group)
		{
			return TryParse (ParserOptions.Default, buffer, startIndex, length, out group);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.GroupAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="GroupAddress"/>. If the address is not a group address or
		/// there is more than a single group address, then parsing will fail.
		/// </remarks>
		/// <returns><c>true</c>, if the address was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="group">The parsed group address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> is out of range.
		/// </exception>
		public static bool TryParse (ParserOptions options, byte[] buffer, int startIndex, out GroupAddress group)
		{
			ParseUtils.ValidateArguments (options, buffer, startIndex);

			int endIndex = buffer.Length;
			int index = startIndex;

			if (!TryParse (options, buffer, ref index, endIndex, false, out group))
				return false;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, false) || index != endIndex) {
				group = null;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.GroupAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="GroupAddress"/>. If the address is not a group address or
		/// there is more than a single group address, then parsing will fail.
		/// </remarks>
		/// <returns><c>true</c>, if the address was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="group">The parsed group address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> is out of range.
		/// </exception>
		public static bool TryParse (byte[] buffer, int startIndex, out GroupAddress group)
		{
			return TryParse (ParserOptions.Default, buffer, startIndex, out group);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.GroupAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="GroupAddress"/>. If the address is not a group address or
		/// there is more than a single group address, then parsing will fail.
		/// </remarks>
		/// <returns><c>true</c>, if the address was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="group">The parsed group address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		public static bool TryParse (ParserOptions options, byte[] buffer, out GroupAddress group)
		{
			ParseUtils.ValidateArguments (options, buffer);

			int endIndex = buffer.Length;
			int index = 0;

			if (!TryParse (options, buffer, ref index, endIndex, false, out group))
				return false;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, false) || index != endIndex) {
				group = null;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.GroupAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="GroupAddress"/>. If the address is not a group address or
		/// there is more than a single group address, then parsing will fail.
		/// </remarks>
		/// <returns><c>true</c>, if the address was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="group">The parsed group address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		public static bool TryParse (byte[] buffer, out GroupAddress group)
		{
			return TryParse (ParserOptions.Default, buffer, out group);
		}

		/// <summary>
		/// Tries to parse the given text into a new <see cref="MimeKit.GroupAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="GroupAddress"/>. If the address is not a group address or
		/// there is more than a single group address, then parsing will fail.
		/// </remarks>
		/// <returns><c>true</c>, if the address was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="text">The text.</param>
		/// <param name="group">The parsed group address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public static bool TryParse (ParserOptions options, string text, out GroupAddress group)
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var buffer = Encoding.UTF8.GetBytes (text);
			int endIndex = buffer.Length;
			int index = 0;

			if (!TryParse (options, buffer, ref index, endIndex, false, out group))
				return false;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, false) || index != endIndex) {
				group = null;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Tries to parse the given text into a new <see cref="MimeKit.GroupAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="GroupAddress"/>. If the address is not a group address or
		/// there is more than a single group address, then parsing will fail.
		/// </remarks>
		/// <returns><c>true</c>, if the address was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="text">The text.</param>
		/// <param name="group">The parsed group address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public static bool TryParse (string text, out GroupAddress group)
		{
			return TryParse (ParserOptions.Default, text, out group);
		}

		/// <summary>
		/// Parses the given input buffer into a new <see cref="MimeKit.GroupAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="GroupAddress"/>. If the address is not a group address or
		/// there is more than a single group address, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.GroupAddress"/>.</returns>
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
		public static new GroupAddress Parse (ParserOptions options, byte[] buffer, int startIndex, int length)
		{
			ParseUtils.ValidateArguments (options, buffer, startIndex, length);

			int endIndex = startIndex + length;
			int index = startIndex;
			GroupAddress group;

			if (!TryParse (options, buffer, ref index, endIndex, true, out group))
				throw new ParseException ("No group address found.", startIndex, startIndex);

			ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, true);

			if (index != endIndex)
				throw new ParseException (string.Format ("Unexpected token at offset {0}", index), index, index);

			return group;
		}

		/// <summary>
		/// Parses the given input buffer into a new <see cref="MimeKit.GroupAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="GroupAddress"/>. If the address is not a group address or
		/// there is more than a single group address, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.GroupAddress"/>.</returns>
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
		public static new GroupAddress Parse (byte[] buffer, int startIndex, int length)
		{
			return Parse (ParserOptions.Default, buffer, startIndex, length);
		}

		/// <summary>
		/// Parses the given input buffer into a new <see cref="MimeKit.GroupAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="GroupAddress"/>. If the address is not a group address or
		/// there is more than a single group address, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.GroupAddress"/>.</returns>
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
		public static new GroupAddress Parse (ParserOptions options, byte[] buffer, int startIndex)
		{
			ParseUtils.ValidateArguments (options, buffer, startIndex);

			int endIndex = buffer.Length;
			int index = startIndex;
			GroupAddress group;

			if (!TryParse (options, buffer, ref index, endIndex, true, out group))
				throw new ParseException ("No group address found.", startIndex, startIndex);

			ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, true);

			if (index != endIndex)
				throw new ParseException (string.Format ("Unexpected token at offset {0}", index), index, index);

			return group;
		}

		/// <summary>
		/// Parses the given input buffer into a new <see cref="MimeKit.GroupAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="GroupAddress"/>. If the address is not a group address or
		/// there is more than a single group address, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.GroupAddress"/>.</returns>
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
		public static new GroupAddress Parse (byte[] buffer, int startIndex)
		{
			return Parse (ParserOptions.Default, buffer, startIndex);
		}

		/// <summary>
		/// Parses the given input buffer into a new <see cref="MimeKit.GroupAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="GroupAddress"/>. If the address is not a group address or
		/// there is more than a single group address, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.GroupAddress"/>.</returns>
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
		public static new GroupAddress Parse (ParserOptions options, byte[] buffer)
		{
			ParseUtils.ValidateArguments (options, buffer);

			int endIndex = buffer.Length;
			GroupAddress group;
			int index = 0;

			if (!TryParse (options, buffer, ref index, endIndex, true, out group))
				throw new ParseException ("No group address found.", 0, 0);

			ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, true);

			if (index != endIndex)
				throw new ParseException (string.Format ("Unexpected token at offset {0}", index), index, index);

			return group;
		}

		/// <summary>
		/// Parses the given input buffer into a new <see cref="MimeKit.GroupAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="GroupAddress"/>. If the address is not a group address or
		/// there is more than a single group address, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.GroupAddress"/>.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static new GroupAddress Parse (byte[] buffer)
		{
			return Parse (ParserOptions.Default, buffer);
		}

		/// <summary>
		/// Parses the given text into a new <see cref="MimeKit.GroupAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="GroupAddress"/>. If the address is not a group address or
		/// there is more than a single group address, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.GroupAddress"/>.</returns>
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
		public static new GroupAddress Parse (ParserOptions options, string text)
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var buffer = Encoding.UTF8.GetBytes (text);
			int endIndex = buffer.Length;
			GroupAddress group;
			int index = 0;

			if (!TryParse (options, buffer, ref index, endIndex, true, out group))
				throw new ParseException ("No group address found.", 0, 0);

			ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, true);

			if (index != endIndex)
				throw new ParseException (string.Format ("Unexpected token at offset {0}", index), index, index);

			return group;
		}

		/// <summary>
		/// Parses the given text into a new <see cref="MimeKit.GroupAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="GroupAddress"/>. If the address is not a group address or
		/// there is more than a single group address, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.GroupAddress"/>.</returns>
		/// <param name="text">The text.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// <paramref name="text"/> could not be parsed.
		/// </exception>
		public static new GroupAddress Parse (string text)
		{
			return Parse (ParserOptions.Default, text);
		}
	}
}

//
// InternetAddress.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
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
using System.Globalization;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// An abstract internet address, as specified by rfc0822.
	/// </summary>
	/// <remarks>
	/// <para>An <see cref="InternetAddress"/> can be any type of address defined by the
	/// original Internet Message specification.</para>
	/// <para>There are effectively two (2) types of addresses: mailboxes and groups.</para>
	/// <para>Mailbox addresses are what are most commonly known as email addresses and are
	/// represented by the <see cref="MailboxAddress"/> class.</para>
	/// <para>Group addresses are themselves lists of addresses and are represented by the
	/// <see cref="GroupAddress"/> class. While rare, it is still important to handle these
	/// types of addresses. They typically only contain mailbox addresses, but may also
	/// contain other group addresses.</para>
	/// </remarks>
	[TypeConverter (typeof (InternetAddressConverter))]
	public abstract class InternetAddress : IComparable<InternetAddress>, IEquatable<InternetAddress>
	{
		const string AtomSpecials = "()<>@,;:\\\".[]";
		Encoding encoding;
		string? name;

		/// <summary>
		/// Initialize a new instance of the <see cref="InternetAddress"/> class.
		/// </summary>
		/// <remarks>
		/// Initializes the <see cref="Encoding"/> and <see cref="Name"/> properties of the internet address.
		/// </remarks>
		/// <param name="encoding">The character encoding to be used for encoding the name.</param>
		/// <param name="name">The name of the mailbox or group.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="encoding"/> is <see langword="null"/>.
		/// </exception>
		protected InternetAddress (Encoding encoding, string? name)
		{
			if (encoding is null)
				throw new ArgumentNullException (nameof (encoding));

			this.encoding = encoding;
			this.name = name?.Trim ();
		}

		/// <summary>
		/// Get or set the character encoding to use when encoding the name of the address.
		/// </summary>
		/// <remarks>
		/// The character encoding is used to convert the <see cref="Name"/> property, if it is set,
		/// to a stream of bytes when encoding the internet address for transport.
		/// </remarks>
		/// <value>The character encoding.</value>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <see langword="null"/>.
		/// </exception>
		public Encoding Encoding {
			get { return encoding; }
			set {
				if (value is null)
					throw new ArgumentNullException (nameof (value));

				if (value == encoding)
					return;

				encoding = value;
				OnChanged ();
			}
		}

		/// <summary>
		/// Get or set the display name of the address.
		/// </summary>
		/// <remarks>
		/// <para>A name is optional and is typically set to the name of the person
		/// or group that own the internet address.</para>
		/// <para>For example, the <see cref="Name"/> property of the following <see cref="MailboxAddress"/> would be <c>"John Smith"</c>.</para>
		/// <para><c>John Smith &lt;j.smith@example.com&gt;</c></para>
		/// <para>Likewise, the <see cref="Name"/> property of the following <see cref="GroupAddress"/> would be <c>"undisclosed-recipients"</c>.</para>
		/// <para><c>undisclosed-recipients: Alice &lt;alice@wonderland.com&gt;, Bob &lt;bob@the-builder.com&gt;;</c></para>
		/// <para>Leading and trailing whitespace is removed from the name when it is set.</para>
		/// </remarks>
		/// <value>The name of the address.</value>
		public string? Name {
			get { return name; }
			set {
				// Note: Leading and trailing whitespace cannot be reliably preserved when the address is encoded (it gets
				// folded or dropped next to rfc2047 encoded-words), so trim it to keep names consistent across round trips.
				value = value?.Trim ();

				if (value == name)
					return;

				name = value;
				OnChanged ();
			}
		}

		/// <summary>
		/// Clone the address.
		/// </summary>
		/// <remarks>
		/// Clones the address.
		/// </remarks>
		/// <returns>The cloned address.</returns>
		public abstract InternetAddress Clone ();

		#region IComparable implementation

		/// <summary>
		/// Compares two internet addresses.
		/// </summary>
		/// <remarks>
		/// Compares two internet addresses for the purpose of sorting.
		/// </remarks>
		/// <returns>The sort order of the current internet address compared to the other internet address.</returns>
		/// <param name="other">The internet address to compare to.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="other"/> is <see langword="null"/>.
		/// </exception>
		public int CompareTo (InternetAddress? other)
		{
			int rv;

			if (other is null)
				throw new ArgumentNullException (nameof (other));

			if ((rv = string.Compare (Name, other.Name, StringComparison.OrdinalIgnoreCase)) != 0)
				return rv;

			var otherMailbox = other as MailboxAddress;
			var mailbox = this as MailboxAddress;

			if (mailbox != null && otherMailbox != null) {
				string otherAddress = otherMailbox.Address;
				int otherAt = otherAddress.IndexOf ('@');
				string address = mailbox.Address;
				int at = address.IndexOf ('@');

				if (at != -1 && otherAt != -1) {
					int length = Math.Min (address.Length - (at + 1), otherAddress.Length - (otherAt + 1));

					rv = string.Compare (address, at + 1, otherAddress, otherAt + 1, length, StringComparison.OrdinalIgnoreCase);
				}

				if (rv == 0) {
					int otherLength = otherAt == -1 ? otherAddress.Length : otherAt;
					int length = at == -1 ? address.Length : at;
					int n = Math.Min (length, otherLength);

					if ((rv = string.Compare (address, 0, otherAddress, 0, n, StringComparison.OrdinalIgnoreCase)) == 0) {
						// The local-part's of the addresses are identical for the first `n` characters. The address
						// with the longer local-part should sort as > the address with the shorter local-part.
						rv = length - otherLength;
					}
				}

				return rv;
			}

			// sort mailbox addresses before group addresses
			if (mailbox != null && otherMailbox is null)
				return -1;

			if (mailbox is null && otherMailbox != null)
				return 1;

			return 0;
		}

		#endregion

		#region IEquatable implementation

		/// <summary>
		/// Determine whether the specified <see cref="InternetAddress"/> is equal to the current <see cref="InternetAddress"/>.
		/// </summary>
		/// <remarks>
		/// Compares two internet addresses to determine if they are identical or not.
		/// </remarks>
		/// <param name="other">The <see cref="InternetAddress"/> to compare with the current <see cref="InternetAddress"/>.</param>
		/// <returns><see langword="true" /> if the specified <see cref="InternetAddress"/> is equal to the current
		/// <see cref="InternetAddress"/>; otherwise, <see langword="false" />.</returns>
		public abstract bool Equals (InternetAddress? other);

		#endregion

		/// <summary>
		/// Determine whether the specified object is equal to the current object.
		/// </summary>
		/// <remarks>
		/// The type of comparison between the current instance and the <paramref name="obj"/> parameter depends on whether
		/// the current instance is a reference type or a value type.
		/// </remarks>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
		public override bool Equals (object? obj)
		{
			return Equals (obj as InternetAddress);
		}

		/// <summary>
		/// Return the hash code for this instance.
		/// </summary>
		/// <remarks>
		/// Returns the hash code for this instance.
		/// </remarks>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode ()
		{
			return ToString ().GetHashCode ();
		}

		internal static string EncodeInternationalizedPhrase (string phrase)
		{
			for (int i = 0; i < phrase.Length; i++) {
				if (AtomSpecials.IndexOf (phrase[i]) != -1)
					return MimeUtils.Quote (phrase);
			}

			return phrase;
		}

		internal abstract void Encode (FormatOptions options, StringBuilder builder, bool firstToken, ref int lineLength);

		/// <summary>
		/// Serialize an <see cref="InternetAddress"/> to a string, optionally encoding it for transport.
		/// </summary>
		/// <remarks>
		/// <para>If the <paramref name="encode"/> parameter is <see langword="true" />, then this method will return
		/// an encoded version of the internet address according to the rules described in rfc2047.</para>
		/// <para>However, if the <paramref name="encode"/> parameter is <see langword="false" />, then this method will
		/// return a string suitable only for display purposes.</para>
		/// </remarks>
		/// <returns>A string representing the <see cref="InternetAddress"/>.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="encode">If set to <see langword="true" />, the <see cref="InternetAddress"/> will be encoded.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="options"/> is <see langword="null"/>.
		/// </exception>
		public abstract string ToString (FormatOptions options, bool encode);

		/// <summary>
		/// Serialize an <see cref="InternetAddress"/> to a string, optionally encoding it for transport.
		/// </summary>
		/// <remarks>
		/// <para>If the <paramref name="encode"/> parameter is <see langword="true" />, then this method will return
		/// an encoded version of the internet address according to the rules described in rfc2047.</para>
		/// <para>However, if the <paramref name="encode"/> parameter is <see langword="false" />, then this method will
		/// return a string suitable only for display purposes.</para>
		/// </remarks>
		/// <returns>A string representing the <see cref="InternetAddress"/>.</returns>
		/// <param name="encode">If set to <see langword="true" />, the <see cref="InternetAddress"/> will be encoded.</param>
		public string ToString (bool encode)
		{
			return ToString (FormatOptions.Default, encode);
		}

		/// <summary>
		/// Serialize an <see cref="InternetAddress"/> to a string suitable for display.
		/// </summary>
		/// <remarks>
		/// The string returned by this method is suitable only for display purposes.
		/// </remarks>
		/// <returns>A string representing the <see cref="InternetAddress"/>.</returns>
		public override string ToString ()
		{
			return ToString (FormatOptions.Default, false);
		}

		internal event EventHandler? Changed;

		/// <summary>
		/// Raise the internal changed event used by <see cref="MimeMessage"/> to keep headers in sync.
		/// </summary>
		/// <remarks>
		/// This method is called whenever a property of the internet address is changed.
		/// </remarks>
		protected virtual void OnChanged ()
		{
			Changed?.Invoke (this, EventArgs.Empty);
		}

		internal static bool TryParseAddrspec (byte[] text, ref int index, int endIndex, ReadOnlySpan<byte> sentinels, RfcComplianceMode compliance, bool throwOnError, [NotNullWhen (true)] out string? addrspec, out int at)
		{
			var parser = InternetAddressParser.Create (ParserOptions.Default, text, index, endIndex);

			try {
				return parser.TryParseAddrspec (sentinels, compliance, throwOnError, out addrspec, out at);
			} finally {
				index = parser.Position;
			}
		}

		internal static bool TryParse (AddressParserFlags flags, ParserOptions options, byte[] text, ref int index, int endIndex, int groupDepth, [NotNullWhen (true)] out InternetAddress? address)
		{
			var parser = InternetAddressParser.Create (options, text, index, endIndex);

			try {
				return parser.TryParseAddress (flags, groupDepth, out address);
			} finally {
				index = parser.Position;
			}
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the buffer contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns><see langword="true" /> if the address was successfully parsed; otherwise, <see langword="false" />.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="address">The parsed address.</param>
		public static bool TryParse (ParserOptions? options, byte[]? buffer, int startIndex, int length, [NotNullWhen (true)] out InternetAddress? address)
		{
			if (!ArgumentValidator.TryValidate (options, buffer, startIndex, length)) {
				address = null;
				return false;
			}

			int endIndex = startIndex + length;
			int index = startIndex;

			if (!TryParse (AddressParserFlags.TryParse, options, buffer, ref index, endIndex, 0, out address))
				return false;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, false)) {
				address = null;
				return false;
			}

			if (index != endIndex) {
				address = null;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the buffer contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns><see langword="true" /> if the address was successfully parsed; otherwise, <see langword="false" />.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="address">The parsed address.</param>
		public static bool TryParse (byte[]? buffer, int startIndex, int length, [NotNullWhen (true)] out InternetAddress? address)
		{
			return TryParse (ParserOptions.Default, buffer, startIndex, length, out address);
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the buffer contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns><see langword="true" /> if the address was successfully parsed; otherwise, <see langword="false" />.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="address">The parsed address.</param>
		public static bool TryParse (ParserOptions? options, byte[]? buffer, int startIndex, [NotNullWhen (true)] out InternetAddress? address)
		{
			if (!ArgumentValidator.TryValidate (options, buffer, startIndex)) {
				address = null;
				return false;
			}

			int endIndex = buffer.Length;
			int index = startIndex;

			if (!TryParse (AddressParserFlags.TryParse, options, buffer, ref index, endIndex, 0, out address))
				return false;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, false) || index != endIndex) {
				address = null;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the buffer contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns><see langword="true" /> if the address was successfully parsed; otherwise, <see langword="false" />.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="address">The parsed address.</param>
		public static bool TryParse (byte[]? buffer, int startIndex, [NotNullWhen (true)] out InternetAddress? address)
		{
			return TryParse (ParserOptions.Default, buffer, startIndex, out address);
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the buffer contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns><see langword="true" /> if the address was successfully parsed; otherwise, <see langword="false" />.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="address">The parsed address.</param>
		public static bool TryParse (ParserOptions? options, byte[]? buffer, [NotNullWhen (true)] out InternetAddress? address)
		{
			if (!ArgumentValidator.TryValidate (options, buffer)) {
				address = null;
				return false;
			}

			int endIndex = buffer.Length;
			int index = 0;

			if (!TryParse (AddressParserFlags.TryParse, options, buffer, ref index, endIndex, 0, out address))
				return false;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, false) || index != endIndex) {
				address = null;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the buffer contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns><see langword="true" /> if the address was successfully parsed; otherwise, <see langword="false" />.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="address">The parsed address.</param>
		public static bool TryParse (byte[]? buffer, [NotNullWhen (true)] out InternetAddress? address)
		{
			return TryParse (ParserOptions.Default, buffer, out address);
		}

		/// <summary>
		/// Try to parse the given text into a new <see cref="InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the text contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns><see langword="true" /> if the address was successfully parsed; otherwise, <see langword="false" />.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="text">The text.</param>
		/// <param name="address">The parsed address.</param>
		public static bool TryParse (ParserOptions? options, string? text, [NotNullWhen (true)] out InternetAddress? address)
		{
			if (!ArgumentValidator.TryValidate (options, text)) {
				address = null;
				return false;
			}

			var parser = InternetAddressParser.Create (options, text);

			return parser.TryParseCompleteAddress (AddressParserFlags.TryParse, out address);
		}

		/// <summary>
		/// Try to parse the given text into a new <see cref="InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the text contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns><see langword="true" /> if the address was successfully parsed; otherwise, <see langword="false" />.</returns>
		/// <param name="text">The text.</param>
		/// <param name="address">The parsed address.</param>
		public static bool TryParse (string? text, [NotNullWhen (true)] out InternetAddress? address)
		{
			return TryParse (ParserOptions.Default, text, out address);
		}

		/// <summary>
		/// Parse the given input buffer into a new <see cref="InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the buffer contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="InternetAddress"/>.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static InternetAddress Parse (ParserOptions options, byte[] buffer, int startIndex, int length)
		{
			ArgumentValidator.Validate (options, buffer, startIndex, length);

			int endIndex = startIndex + length;
			int index = startIndex;

			TryParse (AddressParserFlags.Parse, options, buffer, ref index, endIndex, 0, out var address);

			ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, true);

			if (index != endIndex)
				throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Unexpected token at offset {0}", index), index, index);

			return address!;
		}

		/// <summary>
		/// Parse the given input buffer into a new <see cref="InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the buffer contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="InternetAddress"/>.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static InternetAddress Parse (byte[] buffer, int startIndex, int length)
		{
			return Parse (ParserOptions.Default, buffer, startIndex, length);
		}

		/// <summary>
		/// Parse the given input buffer into a new <see cref="InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the buffer contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="InternetAddress"/>.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/>is out of range.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static InternetAddress Parse (ParserOptions options, byte[] buffer, int startIndex)
		{
			ArgumentValidator.Validate (options, buffer, startIndex);

			int endIndex = buffer.Length;
			int index = startIndex;

			TryParse (AddressParserFlags.Parse, options, buffer, ref index, endIndex, 0, out var address);

			ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, true);

			if (index != endIndex)
				throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Unexpected '{0}' token at offset {1}", (char) buffer[index], index), index, index);

			return address!;
		}

		/// <summary>
		/// Parse the given input buffer into a new <see cref="InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the buffer contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="InternetAddress"/>.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> is out of range.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static InternetAddress Parse (byte[] buffer, int startIndex)
		{
			return Parse (ParserOptions.Default, buffer, startIndex);
		}

		/// <summary>
		/// Parse the given input buffer into a new <see cref="InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the buffer contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="InternetAddress"/>.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static InternetAddress Parse (ParserOptions options, byte[] buffer)
		{
			ArgumentValidator.Validate (options, buffer);

			int endIndex = buffer.Length;
			int index = 0;

			TryParse (AddressParserFlags.Parse, options, buffer, ref index, endIndex, 0, out var address);

			ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, true);

			if (index != endIndex)
				throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Unexpected '{0}' token at offset {1}", (char) buffer[index], index), index, index);

			return address!;
		}

		/// <summary>
		/// Parse the given input buffer into a new <see cref="InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the buffer contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="InternetAddress"/>.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static InternetAddress Parse (byte[] buffer)
		{
			return Parse (ParserOptions.Default, buffer);
		}

		/// <summary>
		/// Parse the given text into a new <see cref="InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the text contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="InternetAddress"/>.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="text">The text.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="text"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// <paramref name="text"/> could not be parsed.
		/// </exception>
		public static InternetAddress Parse (ParserOptions options, string text)
		{
			ArgumentValidator.Validate (options, text);

			var parser = InternetAddressParser.Create (options, text);
			parser.TryParseCompleteAddress (AddressParserFlags.Parse, out var address);

			return address!;
		}

		/// <summary>
		/// Parse the given text into a new <see cref="InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the text contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="InternetAddress"/>.</returns>
		/// <param name="text">The text.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// <paramref name="text"/> could not be parsed.
		/// </exception>
		public static InternetAddress Parse (string text)
		{
			return Parse (ParserOptions.Default, text);
		}
	}
}

//
// InternetAddress.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2018 Xamarin Inc. (www.xamarin.com)
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
using System.Collections.Generic;

#if PORTABLE
using EncoderReplacementFallback = Portable.Text.EncoderReplacementFallback;
using DecoderReplacementFallback = Portable.Text.DecoderReplacementFallback;
using EncoderExceptionFallback = Portable.Text.EncoderExceptionFallback;
using DecoderExceptionFallback = Portable.Text.DecoderExceptionFallback;
using EncoderFallbackException = Portable.Text.EncoderFallbackException;
using DecoderFallbackException = Portable.Text.DecoderFallbackException;
using DecoderFallbackBuffer = Portable.Text.DecoderFallbackBuffer;
using DecoderFallback = Portable.Text.DecoderFallback;
using Encoding = Portable.Text.Encoding;
using Encoder = Portable.Text.Encoder;
using Decoder = Portable.Text.Decoder;
#endif

using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// An internet address, as specified by rfc0822.
	/// </summary>
	/// <remarks>
	/// <para>A <see cref="InternetAddress"/> can be any type of address defined by the
	/// original Internet Message specification.</para>
	/// <para>There are effectively two (2) types of addresses: mailboxes and groups.</para>
	/// <para>Mailbox addresses are what are most commonly known as email addresses and are
	/// represented by the <see cref="MailboxAddress"/> class.</para>
	/// <para>Group addresses are themselves lists of addresses and are represented by the
	/// <see cref="GroupAddress"/> class. While rare, it is still important to handle these
	/// types of addresses. They typically only contain mailbox addresses, but may also
	/// contain other group addresses.</para>
	/// </remarks>
	public abstract class InternetAddress : IComparable<InternetAddress>, IEquatable<InternetAddress>
	{
		const string AtomSpecials = "()<>@,;:\\\".[]";
		Encoding encoding;
		string name;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.InternetAddress"/> class.
		/// </summary>
		/// <remarks>
		/// Initializes the <see cref="Encoding"/> and <see cref="Name"/> properties of the internet address.
		/// </remarks>
		/// <param name="encoding">The character encoding to be used for encoding the name.</param>
		/// <param name="name">The name of the mailbox or group.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="encoding"/> is <c>null</c>.
		/// </exception>
		protected InternetAddress (Encoding encoding, string name)
		{
			if (encoding == null)
				throw new ArgumentNullException (nameof (encoding));

			Encoding = encoding;
			Name = name;
		}

		/// <summary>
		/// Gets or sets the character encoding to use when encoding the name of the address.
		/// </summary>
		/// <remarks>
		/// The character encoding is used to convert the <see cref="Name"/> property, if it is set,
		/// to a stream of bytes when encoding the internet address for transport.
		/// </remarks>
		/// <value>The character encoding.</value>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		public Encoding Encoding {
			get { return encoding; }
			set {
				if (value == null)
					throw new ArgumentNullException (nameof (value));

				if (value == encoding)
					return;

				encoding = value;
				OnChanged ();
			}
		}

		/// <summary>
		/// Gets or sets the display name of the address.
		/// </summary>
		/// <remarks>
		/// A name is optional and is typically set to the name of the person
		/// or group that own the internet address.
		/// </remarks>
		/// <value>The name of the address.</value>
		public string Name {
			get { return name; }
			set {
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
		/// <paramref name="other"/> is <c>null</c>.
		/// </exception>
		public int CompareTo (InternetAddress other)
		{
			int rv;

			if (other == null)
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
					string otherUser = otherAt != -1 ? otherAddress.Substring (0, otherAt) : otherAddress;
					string user = at != -1 ? address.Substring (0, at) : address;

					rv = string.Compare (user, otherUser, StringComparison.OrdinalIgnoreCase);
				}

				return rv;
			}

			// sort mailbox addresses before group addresses
			if (mailbox != null && otherMailbox == null)
				return -1;

			if (mailbox == null && otherMailbox != null)
				return 1;

			return 0;
		}

		#endregion

		#region IEquatable implementation

		/// <summary>
		/// Determines whether the specified <see cref="MimeKit.InternetAddress"/> is equal to the current <see cref="MimeKit.InternetAddress"/>.
		/// </summary>
		/// <remarks>
		/// Compares two internet addresses to determine if they are identical or not.
		/// </remarks>
		/// <param name="other">The <see cref="MimeKit.InternetAddress"/> to compare with the current <see cref="MimeKit.InternetAddress"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="MimeKit.InternetAddress"/> is equal to the current
		/// <see cref="MimeKit.InternetAddress"/>; otherwise, <c>false</c>.</returns>
		public abstract bool Equals (InternetAddress other);

		#endregion

		internal static string EncodeInternationalizedPhrase (string phrase)
		{
			for (int i = 0; i < phrase.Length; i++) {
				if (AtomSpecials.IndexOf (phrase[i]) != -1)
					return MimeUtils.Quote (phrase);
			}

			return phrase;
		}

		internal abstract void Encode (FormatOptions options, StringBuilder builder, ref int lineLength);

		/// <summary>
		/// Returns a string representation of the <see cref="InternetAddress"/>,
		/// optionally encoding it for transport.
		/// </summary>
		/// <remarks>
		/// <para>If the <paramref name="encode"/> parameter is <c>true</c>, then this method will return
		/// an encoded version of the internet address according to the rules described in rfc2047.</para>
		/// <para>However, if the <paramref name="encode"/> parameter is <c>false</c>, then this method will
		/// return a string suitable only for display purposes.</para>
		/// </remarks>
		/// <returns>A string representing the <see cref="InternetAddress"/>.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="encode">If set to <c>true</c>, the <see cref="InternetAddress"/> will be encoded.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="options"/> is <c>null</c>.
		/// </exception>
		public abstract string ToString (FormatOptions options, bool encode);

		/// <summary>
		/// Returns a string representation of the <see cref="InternetAddress"/>,
		/// optionally encoding it for transport.
		/// </summary>
		/// <remarks>
		/// <para>If the <paramref name="encode"/> parameter is <c>true</c>, then this method will return
		/// an encoded version of the internet address according to the rules described in rfc2047.</para>
		/// <para>However, if the <paramref name="encode"/> parameter is <c>false</c>, then this method will
		/// return a string suitable only for display purposes.</para>
		/// </remarks>
		/// <returns>A string representing the <see cref="InternetAddress"/>.</returns>
		/// <param name="encode">If set to <c>true</c>, the <see cref="InternetAddress"/> will be encoded.</param>
		public string ToString (bool encode)
		{
			return ToString (FormatOptions.Default, encode);
		}

		/// <summary>
		/// Returns a string representation of a <see cref="InternetAddress"/> suitable for display.
		/// </summary>
		/// <remarks>
		/// The string returned by this method is suitable only for display purposes.
		/// </remarks>
		/// <returns>A string representing the <see cref="InternetAddress"/>.</returns>
		public override string ToString ()
		{
			return ToString (FormatOptions.Default, false);
		}

		internal event EventHandler Changed;

		/// <summary>
		/// Raises the internal changed event used by <see cref="MimeKit.MimeMessage"/> to keep headers in sync.
		/// </summary>
		/// <remarks>
		/// This method is called whenever a property of the internet address is changed.
		/// </remarks>
		protected virtual void OnChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}

		internal static bool TryParseLocalPart (byte[] text, ref int index, int endIndex, bool throwOnError, out string localpart)
		{
			var token = new StringBuilder ();
			int startIndex = index;

			localpart = null;

			do {
				if (!text[index].IsAtom () && text[index] != '"') {
					if (throwOnError)
						throw new ParseException (string.Format ("Invalid local-part at offset {0}", startIndex), startIndex, index);

					return false;
				}

				int start = index;
				if (!ParseUtils.SkipWord (text, ref index, endIndex, throwOnError))
					return false;

				try {
					token.Append (CharsetUtils.UTF8.GetString (text, start, index - start));
				} catch (DecoderFallbackException ex) {
					if (throwOnError)
						throw new ParseException ("Internationalized local-part tokens may only contain UTF-8 characters.", start, start, ex);

					return false;
				}

				if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;

				if (index >= endIndex || text[index] != (byte) '.')
					break;

				token.Append ('.');
				index++;

				if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;

				if (index >= endIndex) {
					if (throwOnError)
						throw new ParseException (string.Format ("Incomplete local-part at offset {0}", startIndex), startIndex, index);

					return false;
				}
			} while (true);

			localpart = token.ToString ();

			return true;
		}

		static readonly byte[] CommaGreaterThanOrSemiColon = { (byte) ',', (byte) '>', (byte) ';' };

		internal static bool TryParseAddrspec (byte[] text, ref int index, int endIndex, byte[] sentinels, bool throwOnError, out string addrspec, out int at)
		{
			int startIndex = index;

			addrspec = null;
			at = -1;

			string localpart;
			if (!TryParseLocalPart (text, ref index, endIndex, throwOnError, out localpart))
				return false;

			if (index >= endIndex || ParseUtils.IsSentinel (text[index], sentinels)) {
				addrspec = localpart;
				return true;
			}

			if (text[index] != (byte) '@') {
				if (throwOnError)
					throw new ParseException (string.Format ("Invalid addr-spec token at offset {0}", startIndex), startIndex, index);

				return false;
			}

			index++;
			if (index >= endIndex) {
				if (throwOnError)
					throw new ParseException (string.Format ("Incomplete addr-spec token at offset {0}", startIndex), startIndex, index);

				return false;
			}

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			if (index >= endIndex) {
				if (throwOnError)
					throw new ParseException (string.Format ("Incomplete addr-spec token at offset {0}", startIndex), startIndex, index);

				return false;
			}

			string domain;
			if (!ParseUtils.TryParseDomain (text, ref index, endIndex, sentinels, throwOnError, out domain))
				return false;

			if (ParseUtils.IsIdnEncoded (domain))
				domain = ParseUtils.IdnDecode (domain);

			addrspec = localpart + "@" + domain;
			at = localpart.Length;

			return true;
		}

		internal static bool TryParseMailbox (ParserOptions options, byte[] text, int startIndex, ref int index, int endIndex, string name, int codepage, bool throwOnError, out InternetAddress address)
		{
			DomainList route = null;
			Encoding encoding;

			try {
				encoding = Encoding.GetEncoding (codepage);
			} catch {
				encoding = Encoding.UTF8;
			}

			address = null;

			// skip over the '<'
			index++;

			// Note: check for excessive angle brackets like the example described in section 7.1.2 of rfc7103...
			if (index < endIndex && text[index] == (byte) '<') {
				if (options.AddressParserComplianceMode == RfcComplianceMode.Strict) {
					if (throwOnError)
						throw new ParseException (string.Format ("Excessive angle brackets at offset {0}", index), startIndex, index);

					return false;
				}

				do {
					index++;
				} while (index < endIndex && text[index] == '<');
			}

			if (index >= endIndex) {
				if (throwOnError)
					throw new ParseException (string.Format ("Incomplete mailbox at offset {0}", startIndex), startIndex, index);

				return false;
			}

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			if (text[index] == (byte) '@') {
				// Note: we always pass 'false' as the throwOnError argument here so that we can throw a more informative exception on error
				if (!DomainList.TryParse (text, ref index, endIndex, false, out route)) {
					if (throwOnError)
						throw new ParseException (string.Format ("Invalid route in mailbox at offset {0}", startIndex), startIndex, index);

					return false;
				}

				if (index + 1 >= endIndex || text[index] != (byte) ':') {
					if (throwOnError)
						throw new ParseException (string.Format ("Incomplete route in mailbox at offset {0}", startIndex), startIndex, index);

					return false;
				}

				index++;

				if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;
			}

			// Note: The only syntactically correct sentinel token here is the '>', but alas... to deal with the first example
			// in section 7.1.5 of rfc7103, we need to at least handle ',' as a sentinel and might as well handle ';' as well
			// in case the mailbox is within a group address.
			//
			// Example: <third@example.net, fourth@example.net>
			string addrspec;
			int at;

			if (!TryParseAddrspec (text, ref index, endIndex, CommaGreaterThanOrSemiColon, throwOnError, out addrspec, out at))
				return false;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			if (index >= endIndex || text[index] != (byte) '>') {
				if (options.AddressParserComplianceMode == RfcComplianceMode.Strict) {
					if (throwOnError)
						throw new ParseException (string.Format ("Unexpected end of mailbox at offset {0}", startIndex), startIndex, index);

					return false;
				}
			} else {
				// skip over the '>'
				index++;

				// Note: check for excessive angle brackets like the example described in section 7.1.2 of rfc7103...
				if (index < endIndex && text[index] == (byte) '>') {
					if (options.AddressParserComplianceMode == RfcComplianceMode.Strict) {
						if (throwOnError)
							throw new ParseException (string.Format ("Excessive angle brackets at offset {0}", index), startIndex, index);

						return false;
					}

					do {
						index++;
					} while (index < endIndex && text[index] == '>');
				}
			}

			if (route != null)
				address = new MailboxAddress (encoding, name, route, addrspec, at);
			else
				address = new MailboxAddress (encoding, name, addrspec, at);

			return true;
		}

		static bool TryParseGroup (ParserOptions options, byte[] text, int startIndex, ref int index, int endIndex, int groupDepth, string name, int codepage, bool throwOnError, out InternetAddress address)
		{
			List<InternetAddress> members;
			Encoding encoding;

			try {
				encoding = Encoding.GetEncoding (codepage);
			} catch {
				encoding = Encoding.UTF8;
			}

			address = null;

			// skip over the ':'
			index++;

			while (index < endIndex && (text[index] == ':' || text[index].IsBlank ()))
				index++;

			if (InternetAddressList.TryParse (options, text, ref index, endIndex, true, groupDepth, throwOnError, out members))
				address = new GroupAddress (encoding, name, members);
			else
				address = new GroupAddress (encoding, name);

			if (index >= endIndex || text[index] != (byte) ';') {
				if (throwOnError && options.AddressParserComplianceMode == RfcComplianceMode.Strict)
					throw new ParseException (string.Format ("Expected to find ';' at offset {0}", index), startIndex, index);

				while (index < endIndex && text[index] != (byte) ';')
					index++;
			} else {
				index++;
			}

			return true;
		}

		[Flags]
		internal enum AddressParserFlags {
			AllowMailboxAddress = 1 << 0,
			AllowGroupAddress   = 1 << 1,
			ThrowOnError        = 1 << 2,

			TryParse            = AllowMailboxAddress | AllowGroupAddress,
			Parse               = TryParse | ThrowOnError
		}

		internal static bool TryParse (ParserOptions options, byte[] text, ref int index, int endIndex, int groupDepth, AddressParserFlags flags, out InternetAddress address)
		{
			bool strict = options.AddressParserComplianceMode == RfcComplianceMode.Strict;
			bool throwOnError = (flags & AddressParserFlags.ThrowOnError) != 0;
			int minWordCount = options.AllowAddressesWithoutDomain ? 1 : 0;

			address = null;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			if (index == endIndex) {
				if (throwOnError)
					throw new ParseException ("No address found.", index, index);

				return false;
			}

			// keep track of the start & length of the phrase
			bool trimLeadingQuote = false;
			int startIndex = index;
			int length = 0;
			int words = 0;

			while (index < endIndex) {
				if (strict) {
					if (!ParseUtils.SkipWord (text, ref index, endIndex, throwOnError))
						break;
				} else if (text[index] == (byte) '"') {
					int qstringIndex = index;

					if (!ParseUtils.SkipQuoted (text, ref index, endIndex, false)) {
						index = qstringIndex + 1;

						ParseUtils.SkipWhiteSpace (text, ref index, endIndex);

						if (!ParseUtils.SkipAtom (text, ref index, endIndex)) {
							if (throwOnError)
								throw new ParseException (string.Format ("Incomplete quoted-string token at offset {0}", qstringIndex), qstringIndex, endIndex);

							break;
						}

						if (startIndex == qstringIndex)
							trimLeadingQuote = true;
					}
				} else {
					if (!ParseUtils.SkipAtom (text, ref index, endIndex))
						break;
				}

				length = index - startIndex;

				do {
					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;

					// Note: some clients don't quote dots in the name
					if (index >= endIndex || text[index] != (byte) '.')
						break;

					index++;

					length = index - startIndex;
				} while (true);

				words++;

				// Note: some clients don't quote commas in the name
				if (index < endIndex && text[index] == ',' && words > minWordCount) {
					index++;

					length = index - startIndex;

					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;
				}
			}

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			// specials    =  "(" / ")" / "<" / ">" / "@"  ; Must be in quoted-
			//             /  "," / ";" / ":" / "\" / <">  ;  string, to use
			//             /  "." / "[" / "]"              ;  within a word.

			if (index >= endIndex || text[index] == (byte) ',' || text[index] == (byte) '>' || text[index] == ';') {
				// we've completely gobbled up an addr-spec w/o a domain
				byte sentinel = index < endIndex ? text[index] : (byte) ',';
				var sentinels = new byte [] { sentinel };
				string name, addrspec;
				int at;

				if ((flags & AddressParserFlags.AllowMailboxAddress) == 0) {
					if (throwOnError)
						throw new ParseException (string.Format ("Addr-spec token at offset {0}", startIndex), startIndex, index);

					return false;
				}

				// rewind back to the beginning of the local-part
				index = startIndex;

				if (!TryParseAddrspec (text, ref index, endIndex, sentinels, throwOnError, out addrspec, out at))
					return false;

				ParseUtils.SkipWhiteSpace (text, ref index, endIndex);

				if (index < endIndex && text[index] == '(') {
					int comment = index;

					if (!ParseUtils.SkipComment (text, ref index, endIndex)) {
						if (throwOnError)
							throw new ParseException (string.Format ("Incomplete comment token at offset {0}", comment), comment, index);

						return false;
					}

					comment++;

					name = Rfc2047.DecodePhrase (options, text, comment, (index - 1) - comment).Trim ();
				} else {
					name = string.Empty;
				}

				if (index < endIndex && text[index] == (byte) '>') {
					if (strict) {
						if (throwOnError)
							throw new ParseException (string.Format ("Unexpected '>' token at offset {0}", index), startIndex, index);

						return false;
					}

					index++;
				}

				address = new MailboxAddress (Encoding.UTF8, name, addrspec, at);

				return true;
			}

			if (text[index] == (byte) ':') {
				// rfc2822 group address
				int nameIndex = startIndex;
				int codepage = -1;
				string name;

				if ((flags & AddressParserFlags.AllowGroupAddress) == 0) {
					if (throwOnError)
						throw new ParseException (string.Format ("Group address token at offset {0}", startIndex), startIndex, index);

					return false;
				}

				if (groupDepth >= options.MaxAddressGroupDepth) {
					if (throwOnError)
						throw new ParseException (string.Format ("Exceeded maximum rfc822 group depth at offset {0}", startIndex), startIndex, index);

					return false;
				}

				if (trimLeadingQuote) {
					nameIndex++;
					length--;
				}

				if (length > 0) {
					name = Rfc2047.DecodePhrase (options, text, nameIndex, length, out codepage);
				} else {
					name = string.Empty;
				}

				if (codepage == -1)
					codepage = 65001;

				return TryParseGroup (options, text, startIndex, ref index, endIndex, groupDepth + 1, MimeUtils.Unquote (name), codepage, throwOnError, out address);
			}

			if ((flags & AddressParserFlags.AllowMailboxAddress) == 0) {
				if (throwOnError)
					throw new ParseException (string.Format ("Mailbox address token at offset {0}", startIndex), startIndex, index);

				return false;
			}

			if (text[index] == (byte) '@') {
				// we're either in the middle of an addr-spec token or we completely gobbled up an addr-spec w/o a domain
				string name, addrspec;
				int at;

				// rewind back to the beginning of the local-part
				index = startIndex;

				if (!TryParseAddrspec (text, ref index, endIndex, CommaGreaterThanOrSemiColon, throwOnError, out addrspec, out at))
					return false;

				ParseUtils.SkipWhiteSpace (text, ref index, endIndex);

				if (index < endIndex && text[index] == '(') {
					int comment = index;

					if (!ParseUtils.SkipComment (text, ref index, endIndex)) {
						if (throwOnError)
							throw new ParseException (string.Format ("Incomplete comment token at offset {0}", comment), comment, index);

						return false;
					}

					comment++;

					name = Rfc2047.DecodePhrase (options, text, comment, (index - 1) - comment).Trim ();
				} else {
					name = string.Empty;
				}

				if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;

				if (index >= endIndex) {
					address = new MailboxAddress (Encoding.UTF8, name, addrspec, at);
					return true;
				}

				if (text[index] == (byte) '<') {
					// We have an address like "user@example.com <user@example.com>"; i.e. the name is an unquoted string with an '@'.
					if (strict) {
						if (throwOnError)
							throw new ParseException (string.Format ("Unexpected '<' token at offset {0}", index), startIndex, index);

						return false;
					}

					int nameEndIndex = index;
					while (nameEndIndex > startIndex && text[nameEndIndex - 1].IsWhitespace ())
						nameEndIndex--;

					length = nameEndIndex - startIndex;

					// fall through to the rfc822 angle-addr token case...
				} else {
					// Note: since there was no '<', there should not be a '>'... but we handle it anyway in order to
					// deal with the second Unbalanced Angle Brackets example in section 7.1.3: second@example.org>
					if (text[index] == (byte) '>') {
						if (strict) {
							if (throwOnError)
								throw new ParseException (string.Format ("Unexpected '>' token at offset {0}", index), startIndex, index);

							return false;
						}

						index++;
					}

					address = new MailboxAddress (Encoding.UTF8, name, addrspec, at);

					return true;
				}
			}

			if (text[index] == (byte) '<') {
				// rfc2822 angle-addr token
				int nameIndex = startIndex;
				int codepage = -1;
				string name;

				if (trimLeadingQuote) {
					nameIndex++;
					length--;
				}

				if (length > 0) {
					name = Rfc2047.DecodePhrase (options, text, nameIndex, length, out codepage);
				} else {
					name = string.Empty;
				}

				if (codepage == -1)
					codepage = 65001;

				return TryParseMailbox (options, text, startIndex, ref index, endIndex, MimeUtils.Unquote (name), codepage, throwOnError, out address);
			}

			if (throwOnError)
				throw new ParseException (string.Format ("Invalid address token at offset {0}", startIndex), startIndex, index);

			return false;
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the buffer contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns><c>true</c>, if the address was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="address">The parsed address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static bool TryParse (ParserOptions options, byte[] buffer, int startIndex, int length, out InternetAddress address)
		{
			ParseUtils.ValidateArguments (options, buffer, startIndex, length);

			int endIndex = startIndex + length;
			int index = startIndex;

			if (!TryParse (options, buffer, ref index, endIndex, 0, AddressParserFlags.TryParse, out address))
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
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the buffer contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns><c>true</c>, if the address was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="address">The parsed address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static bool TryParse (byte[] buffer, int startIndex, int length, out InternetAddress address)
		{
			return TryParse (ParserOptions.Default, buffer, startIndex, length, out address);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the buffer contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns><c>true</c>, if the address was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="address">The parsed address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> is out of range.
		/// </exception>
		public static bool TryParse (ParserOptions options, byte[] buffer, int startIndex, out InternetAddress address)
		{
			ParseUtils.ValidateArguments (options, buffer, startIndex);

			int endIndex = buffer.Length;
			int index = startIndex;

			if (!TryParse (options, buffer, ref index, endIndex, 0, AddressParserFlags.TryParse, out address))
				return false;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, false) || index != endIndex) {
				address = null;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the buffer contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns><c>true</c>, if the address was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="address">The parsed address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> is out of range.
		/// </exception>
		public static bool TryParse (byte[] buffer, int startIndex, out InternetAddress address)
		{
			return TryParse (ParserOptions.Default, buffer, startIndex, out address);
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the buffer contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns><c>true</c>, if the address was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="address">The parsed address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		public static bool TryParse (ParserOptions options, byte[] buffer, out InternetAddress address)
		{
			ParseUtils.ValidateArguments (options, buffer);

			int endIndex = buffer.Length;
			int index = 0;

			if (!TryParse (options, buffer, ref index, endIndex, 0, AddressParserFlags.TryParse, out address))
				return false;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, false) || index != endIndex) {
				address = null;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the buffer contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns><c>true</c>, if the address was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="address">The parsed address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		public static bool TryParse (byte[] buffer, out InternetAddress address)
		{
			return TryParse (ParserOptions.Default, buffer, out address);
		}

		/// <summary>
		/// Tries to parse the given text into a new <see cref="MimeKit.InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the text contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns><c>true</c>, if the address was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="text">The text.</param>
		/// <param name="address">The parsed address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public static bool TryParse (ParserOptions options, string text, out InternetAddress address)
		{
			ParseUtils.ValidateArguments (options, text);

			var buffer = Encoding.UTF8.GetBytes (text);
			int endIndex = buffer.Length;
			int index = 0;

			if (!TryParse (options, buffer, ref index, endIndex, 0, AddressParserFlags.TryParse, out address))
				return false;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, false) || index != endIndex) {
				address = null;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Tries to parse the given text into a new <see cref="MimeKit.InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the text contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns><c>true</c>, if the address was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="text">The text.</param>
		/// <param name="address">The parsed address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public static bool TryParse (string text, out InternetAddress address)
		{
			return TryParse (ParserOptions.Default, text, out address);
		}

		/// <summary>
		/// Parses the given input buffer into a new <see cref="MimeKit.InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the buffer contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.InternetAddress"/>.</returns>
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
		public static InternetAddress Parse (ParserOptions options, byte[] buffer, int startIndex, int length)
		{
			ParseUtils.ValidateArguments (options, buffer, startIndex, length);

			int endIndex = startIndex + length;
			InternetAddress address;
			int index = startIndex;

			if (!TryParse (options, buffer, ref index, endIndex, 0, AddressParserFlags.Parse, out address))
				throw new ParseException ("No address found.", startIndex, startIndex);

			ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, true);

			if (index != endIndex)
				throw new ParseException (string.Format ("Unexpected token at offset {0}", index), index, index);

			return address;
		}

		/// <summary>
		/// Parses the given input buffer into a new <see cref="MimeKit.InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the buffer contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.InternetAddress"/>.</returns>
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
		public static InternetAddress Parse (byte[] buffer, int startIndex, int length)
		{
			return Parse (ParserOptions.Default, buffer, startIndex, length);
		}

		/// <summary>
		/// Parses the given input buffer into a new <see cref="MimeKit.InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the buffer contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.InternetAddress"/>.</returns>
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
		public static InternetAddress Parse (ParserOptions options, byte[] buffer, int startIndex)
		{
			ParseUtils.ValidateArguments (options, buffer, startIndex);

			int endIndex = buffer.Length;
			InternetAddress address;
			int index = startIndex;

			if (!TryParse (options, buffer, ref index, endIndex, 0, AddressParserFlags.Parse, out address))
				throw new ParseException ("No address found.", startIndex, startIndex);

			ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, true);

			if (index != endIndex)
				throw new ParseException (string.Format ("Unexpected token at offset {0}", index), index, index);

			return address;
		}

		/// <summary>
		/// Parses the given input buffer into a new <see cref="MimeKit.InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the buffer contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.InternetAddress"/>.</returns>
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
		public static InternetAddress Parse (byte[] buffer, int startIndex)
		{
			return Parse (ParserOptions.Default, buffer, startIndex);
		}

		/// <summary>
		/// Parses the given input buffer into a new <see cref="MimeKit.InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the buffer contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.InternetAddress"/>.</returns>
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
		public static InternetAddress Parse (ParserOptions options, byte[] buffer)
		{
			ParseUtils.ValidateArguments (options, buffer);

			int endIndex = buffer.Length;
			InternetAddress address;
			int index = 0;

			if (!TryParse (options, buffer, ref index, endIndex, 0, AddressParserFlags.Parse, out address))
				throw new ParseException ("No address found.", 0, 0);

			ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, true);

			if (index != endIndex)
				throw new ParseException (string.Format ("Unexpected token at offset {0}", index), index, index);

			return address;
		}

		/// <summary>
		/// Parses the given input buffer into a new <see cref="MimeKit.InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the buffer contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.InternetAddress"/>.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static InternetAddress Parse (byte[] buffer)
		{
			return Parse (ParserOptions.Default, buffer);
		}

		/// <summary>
		/// Parses the given text into a new <see cref="MimeKit.InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the text contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.InternetAddress"/>.</returns>
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
		public static InternetAddress Parse (ParserOptions options, string text)
		{
			ParseUtils.ValidateArguments (options, text);

			var buffer = Encoding.UTF8.GetBytes (text);
			int endIndex = buffer.Length;
			InternetAddress address;
			int index = 0;

			if (!TryParse (options, buffer, ref index, endIndex, 0, AddressParserFlags.Parse, out address))
				throw new ParseException ("No address found.", 0, 0);

			ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, true);

			if (index != endIndex)
				throw new ParseException (string.Format ("Unexpected token at offset {0}", index), index, index);

			return address;
		}

		/// <summary>
		/// Parses the given text into a new <see cref="MimeKit.InternetAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/> or <see cref="GroupAddress"/>. If the text contains
		/// more data, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="MimeKit.InternetAddress"/>.</returns>
		/// <param name="text">The text.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
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

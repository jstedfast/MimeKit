//
// InternetAddress.cs
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
using System.Collections.Generic;

namespace MimeKit {
	public abstract class InternetAddress
	{
		Encoding encoding;
		string name;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.InternetAddress"/> class.
		/// </summary>
		/// <param name="encoding">The character encoding to be used for encoding the name.</param>
		/// <param name="name">The name of the mailbox or group.</param>
		protected InternetAddress (Encoding encoding, string name)
		{
			if (encoding == null)
				throw new ArgumentNullException ("encoding");

			Encoding = encoding;
			Name = name;
		}

		/// <summary>
		/// Gets or sets the character encoding to use when encoding the name of the address.
		/// </summary>
		/// <value>The character encoding.</value>
		public Encoding Encoding {
			get { return encoding; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (value == encoding)
					return;

				encoding = value;
				OnChanged ();
			}
		}

		/// <summary>
		/// Gets or sets the name of the address.
		/// </summary>
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

		internal abstract void Encode (StringBuilder builder, ref int lineLength);

		/// <summary>
		/// Serializes the <see cref="MimeKit.InternetAddress"/> to a string, optionally encoding it for transport.
		/// </summary>
		/// <returns>A string representing the <see cref="MimeKit.InternetAddress"/>.</returns>
		/// <param name="encode">If set to <c>true</c>, the <see cref="MimeKit.InternetAddress"/> will be encoded.</param>
		public abstract string ToString (bool encode);

		/// <summary>
		/// Serializes the <see cref="MimeKit.InternetAddress"/> to a string suitable for display.
		/// </summary>
		/// <returns>A string representing the <see cref="MimeKit.InternetAddress"/>.</returns>
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

		static bool TryParseLocalPart (byte[] text, ref int index, int endIndex, bool throwOnError, out string localpart)
		{
			StringBuilder token = new StringBuilder ();
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

				token.Append (Encoding.ASCII.GetString (text, start, index - start));

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

		static bool TryParseAddrspec (byte[] text, ref int index, int endIndex, byte sentinel, bool throwOnError, out string addrspec)
		{
			int startIndex = index;

			addrspec = null;

			string localpart;
			if (!TryParseLocalPart (text, ref index, endIndex, throwOnError, out localpart))
				return false;

			if (index >= endIndex || text[index] == sentinel) {
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
			if (!ParseUtils.TryParseDomain (text, ref index, endIndex, throwOnError, out domain))
				return false;

			addrspec = localpart + "@" + domain;

			return true;
		}

		internal static bool TryParseMailbox (byte[] text, int startIndex, ref int index, int endIndex, string name, int codepage, bool throwOnError, out InternetAddress address)
		{
			Encoding encoding = Encoding.GetEncoding (codepage);
			DomainList route = null;

			address = null;

			// skip over the '<'
			index++;
			if (index >= endIndex) {
				if (throwOnError)
					throw new ParseException (string.Format ("Incomplete mailbox at offset {0}", startIndex), startIndex, index);

				return false;
			}

			if (text[index] == (byte) '@') {
				if (!DomainList.TryParse (text, ref index, endIndex, throwOnError, out route)) {
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
			}

			string addrspec;
			if (!TryParseAddrspec (text, ref index, endIndex, (byte) '>', throwOnError, out addrspec))
				return false;

			if (index >= endIndex || text[index] != (byte) '>') {
				if (throwOnError)
					throw new ParseException (string.Format ("Unexpected end of mailbox at offset {0}", startIndex), startIndex, index);

				return false;
			}

			if (route != null)
				address = new MailboxAddress (encoding, name, route, addrspec);
			else
				address = new MailboxAddress (encoding, name, addrspec);

			index++;

			return true;
		}

		static bool TryParseGroup (ParserOptions options, byte[] text, int startIndex, ref int index, int endIndex, string name, int codepage, bool throwOnError, out InternetAddress address)
		{
			Encoding encoding = Encoding.GetEncoding (codepage);
			List<InternetAddress> members;

			address = null;

			// skip over the ':'
			index++;
			if (index >= endIndex) {
				if (throwOnError)
					throw new ParseException (string.Format ("Incomplete address group at offset {0}", startIndex), startIndex, index);

				return false;
			}

			if (InternetAddressList.TryParse (options, text, ref index, endIndex, true, throwOnError, out members))
				address = new GroupAddress (encoding, name, members);
			else
				address = new GroupAddress (encoding, name);

			if (index >= endIndex || text[index] != (byte) ';') {
				if (throwOnError)
					throw new ParseException (string.Format ("Expected to find ';' at offset {0}", index), startIndex, index);

				while (index < endIndex && text[index] != (byte) ';')
					index++;
			} else {
				index++;
			}

			return true;
		}

		internal static bool TryParse (ParserOptions options, byte[] text, ref int index, int endIndex, bool throwOnError, out InternetAddress address)
		{
			address = null;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			// keep track of the start & length of the phrase
			int startIndex = index;
			int length = 0;

			while (index < endIndex && ParseUtils.SkipWord (text, ref index, endIndex, throwOnError)) {
				length = index - startIndex;

				if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;
			}

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			// specials    =  "(" / ")" / "<" / ">" / "@"  ; Must be in quoted-
			//             /  "," / ";" / ":" / "\" / <">  ;  string, to use
			//             /  "." / "[" / "]"              ;  within a word.

			if (index >= endIndex || text[index] == (byte) ',' || text[index] == ';') {
				// we've completely gobbled up an addr-spec w/o a domain
				byte sentinel = index < endIndex ? text[index] : (byte) ',';
				string addrspec;

				// rewind back to the beginning of the local-part
				index = startIndex;

				if (!TryParseAddrspec (text, ref index, endIndex, sentinel, throwOnError, out addrspec))
					return false;

				address = new MailboxAddress (string.Empty, addrspec);

				return true;
			}

			if (text[index] == (byte) ':') {
				// rfc2822 group address
				int codepage;
				string name = length > 0 ? Rfc2047.DecodePhrase (options, text, startIndex, length, out codepage) : string.Empty;

				return TryParseGroup (options, text, startIndex, ref index, endIndex, Rfc2047.Unquote (name), codepage, throwOnError, out address);
			}

			if (text[index] == (byte) '<') {
				// rfc2822 angle-addr token
				int codepage;
				string name = length > 0 ? Rfc2047.DecodePhrase (options, text, startIndex, length, out codepage) : string.Empty;

				return TryParseMailbox (text, startIndex, ref index, endIndex, Rfc2047.Unquote (name), codepage, throwOnError, out address);
			}

			if (text[index] == '.' || text[index] == (byte) '@') {
				// we're either in the middle of an addr-spec token or we completely gobbled up an addr-spec w/o a domain
				string addrspec;

				// rewind back to the beginning of the local-part
				index = startIndex;

				if (!TryParseAddrspec (text, ref index, endIndex, (byte) ',', throwOnError, out addrspec))
					return false;

				address = new MailboxAddress (string.Empty, addrspec);

				return true;
			}

			if (throwOnError)
				throw new ParseException (string.Format ("Invalid address token at offset {0}", startIndex), startIndex, index);

			return false;
		}
	}
}

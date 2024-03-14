//
// MailboxAddress.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2024 .NET Foundation and Contributors
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
using System.Collections.Generic;

#if ENABLE_SNM
using System.Net.Mail;
#endif

using MimeKit.Utils;
using MimeKit.Encodings;

namespace MimeKit {
	/// <summary>
	/// A mailbox address, as specified by rfc822.
	/// </summary>
	/// <remarks>
	/// Represents a mailbox address (commonly referred to as an email address)
	/// for a single recipient.
	/// </remarks>
	public class MailboxAddress : InternetAddress
	{
		static readonly byte[] EmptySentinels = Array.Empty<byte> ();

		/// <summary>
		/// Get or set the punycode implementation that should be used for encoding and decoding mailbox addresses.
		/// </summary>
		/// <remarks>
		/// Gets or sets the punycode implementation that should be used for encoding and decoding mailbox addresses.
		/// </remarks>
		/// <value>The punycode implementation.</value>
		public static IPunycode IdnMapping { get; set; }

		string address;
		int at;

		static MailboxAddress ()
		{
			IdnMapping = new Punycode ();
		}

		internal MailboxAddress (Encoding encoding, string name, IEnumerable<string> route, string address, int at) : base (encoding, name)
		{
			Route = new DomainList (route);
			Route.Changed += RouteChanged;

			this.address = address;
			this.at = at;
		}

		internal MailboxAddress (Encoding encoding, string name, string address, int at) : base (encoding, name)
		{
			Route = new DomainList ();
			Route.Changed += RouteChanged;

			this.address = address;
			this.at = at;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MailboxAddress"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MailboxAddress"/> with the specified name, address and route. The
		/// specified text encoding is used when encoding the name according to the rules of rfc2047.
		/// </remarks>
		/// <param name="encoding">The character encoding to be used for encoding the name.</param>
		/// <param name="name">The name of the mailbox.</param>
		/// <param name="route">The route of the mailbox.</param>
		/// <param name="address">The address of the mailbox.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="encoding"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="route"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="address"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="ParseException">
		/// <paramref name="address"/> is malformed.
		/// </exception>
		public MailboxAddress (Encoding encoding, string name, IEnumerable<string> route, string address) : base (encoding, name)
		{
			if (address is null)
				throw new ArgumentNullException (nameof (address));

			Route = new DomainList (route);
			Route.Changed += RouteChanged;
			Address = address;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MailboxAddress"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MailboxAddress"/> with the specified name, address and route.
		/// </remarks>
		/// <param name="name">The name of the mailbox.</param>
		/// <param name="route">The route of the mailbox.</param>
		/// <param name="address">The address of the mailbox.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="route"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="address"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="ParseException">
		/// <paramref name="address"/> is malformed.
		/// </exception>
		public MailboxAddress (string name, IEnumerable<string> route, string address) : this (Encoding.UTF8, name, route, address)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MailboxAddress"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MailboxAddress"/> with the specified name and address. The
		/// specified text encoding is used when encoding the name according to the rules of rfc2047.
		/// </remarks>
		/// <param name="encoding">The character encoding to be used for encoding the name.</param>
		/// <param name="name">The name of the mailbox.</param>
		/// <param name="address">The address of the mailbox.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="encoding"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="address"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="ParseException">
		/// <paramref name="address"/> is malformed.
		/// </exception>
		public MailboxAddress (Encoding encoding, string name, string address) : base (encoding, name)
		{
			if (address is null)
				throw new ArgumentNullException (nameof (address));

			Route = new DomainList ();
			Route.Changed += RouteChanged;
			Address = address;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MailboxAddress"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MailboxAddress"/> with the specified name and address.
		/// </remarks>
		/// <param name="name">The name of the mailbox.</param>
		/// <param name="address">The address of the mailbox.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="address"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ParseException">
		/// <paramref name="address"/> is malformed.
		/// </exception>
		public MailboxAddress (string name, string address) : this (Encoding.UTF8, name, address)
		{
		}

		/// <summary>
		/// Clone the mailbox address.
		/// </summary>
		/// <remarks>
		/// Clones the mailbox address.
		/// </remarks>
		/// <returns>The cloned mailbox address.</returns>
		public override InternetAddress Clone ()
		{
			return new MailboxAddress (Encoding, Name, Route, Address);
		}

		/// <summary>
		/// Get the mailbox route.
		/// </summary>
		/// <remarks>
		/// A route is convention that is rarely seen in modern email systems, but is supported
		/// for compatibility with email archives.
		/// </remarks>
		/// <value>The mailbox route.</value>
		public DomainList Route {
			get; private set;
		}

		/// <summary>
		/// Get or set the mailbox address.
		/// </summary>
		/// <remarks>
		/// Represents the actual email address and is in the form of <c>user@domain.com</c>.
		/// </remarks>
		/// <value>The mailbox address.</value>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ParseException">
		/// <paramref name="value"/> is malformed.
		/// </exception>
		public string Address {
			get { return address; }
			set {
				if (value is null)
					throw new ArgumentNullException (nameof (value));

				if (value == address)
					return;

				if (value.Length > 0) {
					var compliance = ParserOptions.Default.AddressParserComplianceMode;
					var buffer = CharsetUtils.UTF8.GetBytes (value);
					int index = 0;

					TryParseAddrspec (buffer, ref index, buffer.Length, EmptySentinels, compliance, true, out string addrspec, out int atIndex);

					ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, buffer.Length, false);

					if (index != buffer.Length)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Unexpected '{0}' token at offset {1}", (char) buffer[index], index), index, index);

					address = addrspec;
					at = atIndex;
				} else {
					address = string.Empty;
					at = -1;
				}

				OnChanged ();
			}
		}

		/// <summary>
		/// Get the local-part of the email address.
		/// </summary>
		/// <remarks>
		/// Gets the local-part of the email address, sometimes referred to as the "user" portion of an email address.
		/// </remarks>
		/// <value>The local-part portion of the mailbox address.</value>
		public string LocalPart {
			get {
				return at != -1 ? address.Substring (0, at) : address;
			}
		}

		/// <summary>
		/// Get the domain of the email address.
		/// </summary>
		/// <remarks>
		/// Gets the domain of the email address.
		/// </remarks>
		/// <value>The domain portion of the mailbox address.</value>
		public string Domain {
			get {
				return at != -1 ? address.Substring (at + 1) : string.Empty;
			}
		}

		/// <summary>
		/// Get whether or not the address is an international address.
		/// </summary>
		/// <remarks>
		/// <para>International addresses are addresses that contain international
		/// characters in either their local-parts or their domains.</para>
		/// <para>For more information, see section 3.2 of
		/// <a href="https://tools.ietf.org/html/rfc6532#section-3.2">rfc6532</a>.</para>
		/// </remarks>
		/// <value><c>true</c> if the address is an international address; otherwise, <c>false</c>.</value>
		public bool IsInternational {
			get {
				if (string.IsNullOrEmpty (address))
					return false;

				if (ParseUtils.IsInternational (address))
					return true;

				foreach (var domain in Route) {
					if (ParseUtils.IsInternational (domain))
						return true;
				}

				return false;
			}
		}

		static string EncodeAddrspec (string addrspec, int at)
		{
			if (at != -1) {
				var domainEncoded = ParseUtils.IsInternational (addrspec, at + 1);

				if (!domainEncoded)
					return addrspec;

				var local = addrspec.Substring (0, at);
				string domain;

				if (domainEncoded)
					domain = IdnMapping.Encode (addrspec, at + 1);
				else
					domain = addrspec.Substring (at + 1);

				return local + "@" + domain;
			}

			return addrspec;
		}

		/// <summary>
		/// Encode an addrspec token according to IDN encoding rules.
		/// </summary>
		/// <remarks>
		/// Encodes an addrspec token according to IDN encoding rules.
		/// </remarks>
		/// <returns>The encoded addrspec token.</returns>
		/// <param name="addrspec">The addrspec token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="addrspec"/> is <c>null</c>.
		/// </exception>
		public static string EncodeAddrspec (string addrspec)
		{
			if (addrspec is null)
				throw new ArgumentNullException (nameof (addrspec));

			if (addrspec.Length == 0)
				return addrspec;

			var buffer = CharsetUtils.UTF8.GetBytes (addrspec);
			int index = 0;

			if (!TryParseAddrspec (buffer, ref index, buffer.Length, EmptySentinels, RfcComplianceMode.Looser, false, out string address, out int at))
				return addrspec;

			return EncodeAddrspec (address, at);
		}

		/// <summary>
		/// Decode an addrspec token according to IDN decoding rules.
		/// </summary>
		/// <remarks>
		/// Decodes an addrspec token according to IDN decoding rules.
		/// </remarks>
		/// <returns>The decoded addrspec token.</returns>
		/// <param name="addrspec">The addrspec token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="addrspec"/> is <c>null</c>.
		/// </exception>
		public static string DecodeAddrspec (string addrspec)
		{
			if (addrspec is null)
				throw new ArgumentNullException (nameof (addrspec));

			if (addrspec.Length == 0)
				return addrspec;

			var buffer = CharsetUtils.UTF8.GetBytes (addrspec);
			int index = 0;

			// Note: The parsed address will be IDN-decoded.
			if (!TryParseAddrspec (buffer, ref index, buffer.Length, EmptySentinels, RfcComplianceMode.Looser, false, out string address, out _))
				return addrspec;

			return address;
		}

		/// <summary>
		/// Get the mailbox address, optionally encoded according to IDN encoding rules.
		/// </summary>
		/// <remarks>
		/// If <paramref name="idnEncode"/> is <c>true</c>, then the returned mailbox address will be encoded according to the IDN encoding rules.
		/// </remarks>
		/// <param name="idnEncode"><c>true</c> if the address should be encoded according to IDN encoding rules; otherwise, <c>false</c>.</param>
		/// <returns>The mailbox address.</returns>
		public string GetAddress (bool idnEncode)
		{
			if (idnEncode)
				return EncodeAddrspec (address, at);

			// Note: The address is *always* in its decoded form.
			return address;
		}

		internal override void Encode (FormatOptions options, StringBuilder builder, bool firstToken, ref int lineLength)
		{
			var route = Route.Encode (options);
			if (!string.IsNullOrEmpty (route))
				route += ":";

			var addrspec = GetAddress (!options.International);

			if (!string.IsNullOrEmpty (Name)) {
				string name;

				if (!options.International) {
					name = Rfc2047.EncodePhraseAsString (options, Encoding, Name);
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

				if ((lineLength + route.Length + addrspec.Length + 3) > options.MaxLineLength) {
					builder.Append (options.NewLine);
					builder.Append ("\t<");
					lineLength = 2;
				} else {
					builder.Append (" <");
					lineLength += 2;
				}

				lineLength += route.Length;
				builder.Append (route);

				lineLength += addrspec.Length + 1;
				builder.Append (addrspec);
				builder.Append ('>');
			} else if (!string.IsNullOrEmpty (route)) {
				if (!firstToken && (lineLength + route.Length + addrspec.Length + 2) > options.MaxLineLength) {
					builder.Append (options.NewLine);
					builder.Append ("\t<");
					lineLength = 2;
				} else {
					builder.Append ('<');
					lineLength++;
				}

				lineLength += route.Length;
				builder.Append (route);

				lineLength += addrspec.Length + 1;
				builder.Append (addrspec);
				builder.Append ('>');
			} else {
				if (!firstToken && (lineLength + addrspec.Length) > options.MaxLineLength) {
					builder.LineWrap (options);
					lineLength = 1;
				}

				lineLength += addrspec.Length;
				builder.Append (addrspec);
			}
		}

		/// <summary>
		/// Return a string representation of the <see cref="MailboxAddress"/>,
		/// optionally encoding it for transport.
		/// </summary>
		/// <remarks>
		/// Returns a string containing the formatted mailbox address. If the <paramref name="encode"/>
		/// parameter is <c>true</c>, then the mailbox name will be encoded according to the rules defined
		/// in rfc2047, otherwise the name will not be encoded at all and will therefor only be suitable
		/// for display purposes.
		/// </remarks>
		/// <returns>A string representing the <see cref="MailboxAddress"/>.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="encode">If set to <c>true</c>, the <see cref="MailboxAddress"/> will be encoded.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="options"/> is <c>null</c>.
		/// </exception>
		public override string ToString (FormatOptions options, bool encode)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (encode) {
				var builder = new StringBuilder ();
				int lineLength = 0;

				Encode (options, builder, true, ref lineLength);

				return builder.ToString ();
			}

			string route = Route.ToString ();
			if (!string.IsNullOrEmpty (route))
				route += ":";

			if (!string.IsNullOrEmpty (Name))
				return MimeUtils.Quote (Name) + " <" + route + Address + ">";

			if (!string.IsNullOrEmpty (route))
				return "<" + route + Address + ">";

			return Address;
		}

		#region IEquatable implementation

		/// <summary>
		/// Determine whether the specified <see cref="MailboxAddress"/> is equal to the current <see cref="MailboxAddress"/>.
		/// </summary>
		/// <remarks>
		/// Compares two mailbox addresses to determine if they are identical or not.
		/// </remarks>
		/// <param name="other">The <see cref="MailboxAddress"/> to compare with the current <see cref="MailboxAddress"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="MailboxAddress"/> is equal to the current
		/// <see cref="MailboxAddress"/>; otherwise, <c>false</c>.</returns>
		public override bool Equals (InternetAddress other)
		{
			return other is MailboxAddress mailbox
				&& Name == mailbox.Name
				&& Address == mailbox.Address;
		}

		#endregion

		void RouteChanged (object sender, EventArgs e)
		{
			OnChanged ();
		}

		internal static bool TryParse (ParserOptions options, byte[] text, ref int index, int endIndex, bool throwOnError, out MailboxAddress mailbox)
		{
			var flags = AddressParserFlags.AllowMailboxAddress;

			if (throwOnError)
				flags |= AddressParserFlags.ThrowOnError;

			if (!InternetAddress.TryParse (flags, options, text, ref index, endIndex, 0, out var address)) {
				mailbox = null;
				return false;
			}

			mailbox = (MailboxAddress) address;

			return true;
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="MailboxAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/>. If the address is not a mailbox address or
		/// there is more than a single mailbox address, then parsing will fail.
		/// </remarks>
		/// <returns><c>true</c>, if the address was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="mailbox">The parsed mailbox address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static bool TryParse (ParserOptions options, byte[] buffer, int startIndex, int length, out MailboxAddress mailbox)
		{
			ParseUtils.ValidateArguments (options, buffer, startIndex, length);

			int endIndex = startIndex + length;
			int index = startIndex;

			if (!TryParse (options, buffer, ref index, endIndex, false, out mailbox))
				return false;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, false) || index != endIndex) {
				mailbox = null;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="MailboxAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/>. If the address is not a mailbox address or
		/// there is more than a single mailbox address, then parsing will fail.
		/// </remarks>
		/// <returns><c>true</c>, if the address was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="mailbox">The parsed mailbox address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static bool TryParse (byte[] buffer, int startIndex, int length, out MailboxAddress mailbox)
		{
			return TryParse (ParserOptions.Default, buffer, startIndex, length, out mailbox);
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="MailboxAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/>. If the address is not a mailbox address or
		/// there is more than a single mailbox address, then parsing will fail.
		/// </remarks>
		/// <returns><c>true</c>, if the address was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="mailbox">The parsed mailbox address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> is out of range.
		/// </exception>
		public static bool TryParse (ParserOptions options, byte[] buffer, int startIndex, out MailboxAddress mailbox)
		{
			ParseUtils.ValidateArguments (options, buffer, startIndex);

			int endIndex = buffer.Length;
			int index = startIndex;

			if (!TryParse (options, buffer, ref index, endIndex, false, out mailbox))
				return false;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, false) || index != endIndex) {
				mailbox = null;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="MailboxAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/>. If the address is not a mailbox address or
		/// there is more than a single mailbox address, then parsing will fail.
		/// </remarks>
		/// <returns><c>true</c>, if the address was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="mailbox">The parsed mailbox address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> is out of range.
		/// </exception>
		public static bool TryParse (byte[] buffer, int startIndex, out MailboxAddress mailbox)
		{
			return TryParse (ParserOptions.Default, buffer, startIndex, out mailbox);
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="MailboxAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/>. If the address is not a mailbox address or
		/// there is more than a single mailbox address, then parsing will fail.
		/// </remarks>
		/// <returns><c>true</c>, if the address was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="mailbox">The parsed mailbox address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		public static bool TryParse (ParserOptions options, byte[] buffer, out MailboxAddress mailbox)
		{
			ParseUtils.ValidateArguments (options, buffer);

			int endIndex = buffer.Length;
			int index = 0;

			if (!TryParse (options, buffer, ref index, endIndex, false, out mailbox))
				return false;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, false) || index != endIndex) {
				mailbox = null;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="MailboxAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/>. If the address is not a mailbox address or
		/// there is more than a single mailbox address, then parsing will fail.
		/// </remarks>
		/// <returns><c>true</c>, if the address was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="mailbox">The parsed mailbox address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		public static bool TryParse (byte[] buffer, out MailboxAddress mailbox)
		{
			return TryParse (ParserOptions.Default, buffer, out mailbox);
		}

		/// <summary>
		/// Try to parse the given text into a new <see cref="MailboxAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/>. If the address is not a mailbox address or
		/// there is more than a single mailbox address, then parsing will fail.
		/// </remarks>
		/// <returns><c>true</c>, if the address was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="text">The text.</param>
		/// <param name="mailbox">The parsed mailbox address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public static bool TryParse (ParserOptions options, string text, out MailboxAddress mailbox)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (text is null)
				throw new ArgumentNullException (nameof (text));

			var buffer = Encoding.UTF8.GetBytes (text);
			int endIndex = buffer.Length;
			int index = 0;

			if (!TryParse (options, buffer, ref index, endIndex, false, out mailbox))
				return false;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, false) || index != endIndex) {
				mailbox = null;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Try to parse the given text into a new <see cref="MailboxAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/>. If the address is not a mailbox address or
		/// there is more than a single mailbox address, then parsing will fail.
		/// </remarks>
		/// <returns><c>true</c>, if the address was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="text">The text.</param>
		/// <param name="mailbox">The parsed mailbox address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public static bool TryParse (string text, out MailboxAddress mailbox)
		{
			return TryParse (ParserOptions.Default, text, out mailbox);
		}

		/// <summary>
		/// Parse the given input buffer into a new <see cref="MailboxAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/>. If the address is not a mailbox address or
		/// there is more than a single mailbox address, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="MailboxAddress"/>.</returns>
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
		public static new MailboxAddress Parse (ParserOptions options, byte[] buffer, int startIndex, int length)
		{
			ParseUtils.ValidateArguments (options, buffer, startIndex, length);

			int endIndex = startIndex + length;
			int index = startIndex;

			TryParse (options, buffer, ref index, endIndex, true, out var mailbox);

			ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, true);

			if (index != endIndex)
				throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Unexpected token at offset {0}", index), index, index);

			return mailbox;
		}

		/// <summary>
		/// Parse the given input buffer into a new <see cref="MailboxAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/>. If the address is not a mailbox address or
		/// there is more than a single mailbox address, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="MailboxAddress"/>.</returns>
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
		/// <exception cref="ParseException">
		/// <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static new MailboxAddress Parse (byte[] buffer, int startIndex, int length)
		{
			return Parse (ParserOptions.Default, buffer, startIndex, length);
		}

		/// <summary>
		/// Parse the given input buffer into a new <see cref="MailboxAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/>. If the address is not a mailbox address or
		/// there is more than a single mailbox address, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="MailboxAddress"/>.</returns>
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
		/// <exception cref="ParseException">
		/// <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static new MailboxAddress Parse (ParserOptions options, byte[] buffer, int startIndex)
		{
			ParseUtils.ValidateArguments (options, buffer, startIndex);

			int endIndex = buffer.Length;
			int index = startIndex;

			TryParse (options, buffer, ref index, endIndex, true, out var mailbox);

			ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, true);

			if (index != endIndex)
				throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Unexpected token at offset {0}", index), index, index);

			return mailbox;
		}

		/// <summary>
		/// Parse the given input buffer into a new <see cref="MailboxAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/>. If the address is not a mailbox address or
		/// there is more than a single mailbox address, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="MailboxAddress"/>.</returns>
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
		public static new MailboxAddress Parse (byte[] buffer, int startIndex)
		{
			return Parse (ParserOptions.Default, buffer, startIndex);
		}

		/// <summary>
		/// Parse the given input buffer into a new <see cref="MailboxAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/>. If the address is not a mailbox address or
		/// there is more than a single mailbox address, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="MailboxAddress"/>.</returns>
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
		public static new MailboxAddress Parse (ParserOptions options, byte[] buffer)
		{
			ParseUtils.ValidateArguments (options, buffer);

			int endIndex = buffer.Length;
			int index = 0;

			TryParse (options, buffer, ref index, endIndex, true, out var mailbox);

			ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, true);

			if (index != endIndex)
				throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Unexpected token at offset {0}", index), index, index);

			return mailbox;
		}

		/// <summary>
		/// Parse the given input buffer into a new <see cref="MailboxAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/>. If the address is not a mailbox address or
		/// there is more than a single mailbox address, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="MailboxAddress"/>.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static new MailboxAddress Parse (byte[] buffer)
		{
			return Parse (ParserOptions.Default, buffer);
		}

		/// <summary>
		/// Parse the given text into a new <see cref="MailboxAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/>. If the address is not a mailbox address or
		/// there is more than a single mailbox address, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="MailboxAddress"/>.</returns>
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
		public static new MailboxAddress Parse (ParserOptions options, string text)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (text is null)
				throw new ArgumentNullException (nameof (text));

			var buffer = Encoding.UTF8.GetBytes (text);
			int endIndex = buffer.Length;
			int index = 0;

			TryParse (options, buffer, ref index, endIndex, true, out var mailbox);

			ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, true);

			if (index != endIndex)
				throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Unexpected token at offset {0}", index), index, index);

			return mailbox;
		}

		/// <summary>
		/// Parse the given text into a new <see cref="MailboxAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/>. If the address is not a mailbox address or
		/// there is more than a single mailbox address, then parsing will fail.
		/// </remarks>
		/// <returns>The parsed <see cref="MailboxAddress"/>.</returns>
		/// <param name="text">The text.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// <paramref name="text"/> could not be parsed.
		/// </exception>
		public static new MailboxAddress Parse (string text)
		{
			return Parse (ParserOptions.Default, text);
		}

#if ENABLE_SNM
		/// <summary>
		/// Explicit cast to convert a <see cref="MailboxAddress"/> to a
		/// <see cref="System.Net.Mail.MailAddress"/>.
		/// </summary>
		/// <remarks>
		/// Casts a <see cref="MailboxAddress"/> to a <see cref="System.Net.Mail.MailAddress"/>
		/// in cases where you might want to make use of the System.Net.Mail APIs.
		/// </remarks>
		/// <returns>The equivalent <see cref="System.Net.Mail.MailAddress"/>.</returns>
		/// <param name="mailbox">The mailbox.</param>
		public static explicit operator MailAddress (MailboxAddress mailbox)
		{
			return mailbox != null ? new MailAddress (mailbox.Address, mailbox.Name, mailbox.Encoding) : null;
		}

		/// <summary>
		/// Explicit cast to convert a <see cref="System.Net.Mail.MailAddress"/>
		/// to a <see cref="MailboxAddress"/>.
		/// </summary>
		/// <remarks>
		/// Casts a <see cref="System.Net.Mail.MailAddress"/> to a <see cref="MailboxAddress"/>
		/// in cases where you might want to make use of the the superior MimeKit APIs.
		/// </remarks>
		/// <returns>The equivalent <see cref="MailboxAddress"/>.</returns>
		/// <param name="address">The mail address.</param>
		public static explicit operator MailboxAddress (MailAddress address)
		{
			return address != null ? new MailboxAddress (address.DisplayName, address.Address) : null;
		}
#endif
	}
}

//
// MailboxAddress.cs
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
using System.Collections.Generic;

#if PORTABLE
using Encoding = Portable.Text.Encoding;
#endif

#if ENABLE_SNM
using System.Net.Mail;
#endif

using MimeKit.Utils;

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
		string address;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MailboxAddress"/> class.
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
		public MailboxAddress (Encoding encoding, string name, IEnumerable<string> route, string address) : base (encoding, name)
		{
			if (address == null)
				throw new ArgumentNullException ("address");

			Route = new DomainList (route);
			Route.Changed += RouteChanged;
			Address = address;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MailboxAddress"/> class.
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
		public MailboxAddress (string name, IEnumerable<string> route, string address) : this (Encoding.UTF8, name, route, address)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MailboxAddress"/> class.
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
		public MailboxAddress (Encoding encoding, string name, string address) : base (encoding, name)
		{
			Route = new DomainList ();
			Route.Changed += RouteChanged;

			this.address = address;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MailboxAddress"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MailboxAddress"/> with the specified name and address.
		/// </remarks>
		/// <param name="name">The name of the mailbox.</param>
		/// <param name="address">The address of the mailbox.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="address"/> is <c>null</c>.
		/// </exception>
		public MailboxAddress (string name, string address) : this (Encoding.UTF8, name, address)
		{
		}

		/// <summary>
		/// Gets the mailbox route.
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
		/// Gets or sets the mailbox address.
		/// </summary>
		/// <remarks>
		/// Represents the actual email address and is in the form of <c>"name@example.com"</c>.
		/// </remarks>
		/// <value>The mailbox address.</value>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		public string Address {
			get { return address; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (value == address)
					return;

				address = value;
				OnChanged ();
			}
		}

		/// <summary>
		/// Gets whether or not the address is an international address.
		/// </summary>
		/// <remarks>
		/// <para>International addresses are addresses that contain international
		/// characters in either their local-parts or their domains.</para>
		/// <para>For more information, see http://tools.ietf.org/html/rfc6532#section-3.2</para>
		/// </remarks>
		/// <value><c>true</c> if th address is an international address; otherwise, <c>false</c>.</value>
		public bool IsInternational {
			get {
				if (address == null)
					return false;

				for (int i = 0; i < address.Length; i++) {
					if (address[i] > 127)
						return true;
				}

				foreach (var domain in Route) {
					for (int i = 0; i < domain.Length; i++) {
						if (domain[i] > 127)
							return true;
					}
				}

				return false;
			}
		}

		internal override void Encode (FormatOptions options, StringBuilder builder, ref int lineLength)
		{
			if (builder == null)
				throw new ArgumentNullException ("builder");

			if (lineLength < 0)
				throw new ArgumentOutOfRangeException ("lineLength");

			string route = Route.ToString ();
			if (!string.IsNullOrEmpty (route))
				route += ":";

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
						builder.AppendFolded (options, name, ref lineLength);
					} else {
						// the name itself is short enough to fit on a single line,
						// but only if we write it on a line by itself
						if (lineLength > 1) {
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

				if ((lineLength + route.Length + Address.Length + 3) > options.MaxLineLength) {
					builder.Append (options.NewLine);
					builder.Append ("\t<");
					lineLength = 2;
				} else {
					builder.Append (" <");
					lineLength += 2;
				}

				lineLength += route.Length;
				builder.Append (route);

				lineLength += Address.Length + 1;
				builder.Append (Address);
				builder.Append ('>');
			} else if (!string.IsNullOrEmpty (route)) {
				if ((lineLength + route.Length + Address.Length + 2) > options.MaxLineLength) {
					builder.Append (options.NewLine);
					builder.Append ("\t<");
					lineLength = 2;
				} else {
					builder.Append ('<');
					lineLength++;
				}

				lineLength += route.Length;
				builder.Append (route);

				lineLength += Address.Length + 1;
				builder.Append (Address);
				builder.Append ('>');
			} else {
				if ((lineLength + Address.Length) > options.MaxLineLength) {
					builder.LineWrap (options);
					lineLength = 1;
				}

				lineLength += Address.Length;
				builder.Append (Address);
			}
		}

		/// <summary>
		/// Returns a string representation of the <see cref="MailboxAddress"/>,
		/// optionally encoding it for transport.
		/// </summary>
		/// <remarks>
		/// Returns a string containing the formatted mailbox address. If the <paramref name="encode"/>
		/// parameter is <c>true</c>, then the mailbox name will be encoded according to the rules defined
		/// in rfc2047, otherwise the name will not be encoded at all and will therefor only be suitable
		/// for display purposes.
		/// </remarks>
		/// <returns>A string representing the <see cref="MailboxAddress"/>.</returns>
		/// <param name="encode">If set to <c>true</c>, the <see cref="MailboxAddress"/> will be encoded.</param>
		public override string ToString (bool encode)
		{
			if (encode) {
				var builder = new StringBuilder ();
				int lineLength = 0;

				Encode (FormatOptions.GetDefault (), builder, ref lineLength);

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
		/// Determines whether the specified <see cref="MimeKit.MailboxAddress"/> is equal to the current <see cref="MimeKit.MailboxAddress"/>.
		/// </summary>
		/// <remarks>
		/// Compares two mailbox addresses to determine if they are identical or not.
		/// </remarks>
		/// <param name="other">The <see cref="MimeKit.MailboxAddress"/> to compare with the current <see cref="MimeKit.MailboxAddress"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="MimeKit.MailboxAddress"/> is equal to the current
		/// <see cref="MimeKit.MailboxAddress"/>; otherwise, <c>false</c>.</returns>
		public override bool Equals (InternetAddress other)
		{
			var mailbox = other as MailboxAddress;

			if (mailbox == null)
				return false;

			return Name == mailbox.Name && Address == mailbox.Address;
		}

		#endregion

		void RouteChanged (object sender, EventArgs e)
		{
			OnChanged ();
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.MailboxAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/>. If the the address is not a mailbox address or
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
			InternetAddress address;

			if (!InternetAddress.TryParse (options, buffer, startIndex, length, out address)) {
				mailbox = null;
				return false;
			}

			mailbox = address as MailboxAddress;

			return mailbox != null;
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.MailboxAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/>. If the the address is not a mailbox address or
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
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.MailboxAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/>. If the the address is not a mailbox address or
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
			InternetAddress address;

			if (!InternetAddress.TryParse (options, buffer, startIndex, out address)) {
				mailbox = null;
				return false;
			}

			mailbox = address as MailboxAddress;

			return mailbox != null;
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.MailboxAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/>. If the the address is not a mailbox address or
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
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.MailboxAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/>. If the the address is not a mailbox address or
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
			InternetAddress address;

			if (!InternetAddress.TryParse (options, buffer, out address)) {
				mailbox = null;
				return false;
			}

			mailbox = address as MailboxAddress;

			return mailbox != null;
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="MimeKit.MailboxAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/>. If the the address is not a mailbox address or
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
		/// Tries to parse the given text into a new <see cref="MimeKit.MailboxAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/>. If the the address is not a mailbox address or
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
			InternetAddress address;

			if (!InternetAddress.TryParse (options, text, out address)) {
				mailbox = null;
				return false;
			}

			mailbox = address as MailboxAddress;

			return mailbox != null;
		}

		/// <summary>
		/// Tries to parse the given text into a new <see cref="MimeKit.MailboxAddress"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a single <see cref="MailboxAddress"/>. If the the address is not a mailbox address or
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

//
// MailboxAddress.cs
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

using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// A mailbox address, as specified by rfc0822.
	/// </summary>
	public sealed class MailboxAddress : InternetAddress, IEquatable<MailboxAddress>
	{
		string address;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MailboxAddress"/> class.
		/// </summary>
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
		/// <value>The mailbox route.</value>
		public DomainList Route {
			get; private set;
		}

		/// <summary>
		/// Gets or sets the mailbox address.
		/// </summary>
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
				var encoded = Rfc2047.EncodePhrase (options, Encoding, Name);
				var str = Encoding.ASCII.GetString (encoded);

				if (lineLength + str.Length > options.MaxLineLength) {
					if (str.Length > options.MaxLineLength) {
						// we need to break up the name...
						builder.AppendFolded (options, str, ref lineLength);
					} else {
						// the name itself is short enough to fit on a single line,
						// but only if we write it on a line by itself
						if (lineLength > 1) {
							builder.LineWrap (options);
							lineLength = 1;
						}

						lineLength += str.Length;
						builder.Append (str);
					}
				} else {
					// we can safely fit the name on this line...
					lineLength += str.Length;
					builder.Append (str);
				}

				if ((lineLength + route.Length + Address.Length + 3) > options.MaxLineLength) {
					builder.Append ("\n\t<");
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
					builder.Append ("\n\t<");
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
		/// Serializes the <see cref="MimeKit.MailboxAddress"/> to a string, optionally encoding it for transport.
		/// </summary>
		/// <returns>A string representing the <see cref="MimeKit.MailboxAddress"/>.</returns>
		/// <param name="encode">If set to <c>true</c>, the <see cref="MimeKit.MailboxAddress"/> will be encoded.</param>
		public override string ToString (bool encode)
		{
			if (encode) {
				var builder = new StringBuilder ();
				int lineLength = 0;

				Encode (FormatOptions.Default, builder, ref lineLength);

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
		/// <param name="other">The <see cref="MimeKit.MailboxAddress"/> to compare with the current <see cref="MimeKit.MailboxAddress"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="MimeKit.MailboxAddress"/> is equal to the current
		/// <see cref="MimeKit.MailboxAddress"/>; otherwise, <c>false</c>.</returns>
		public bool Equals (MailboxAddress other)
		{
			if (other == null)
				return false;

			return Name == other.Name && Address == other.Address;
		}

		#endregion

		void RouteChanged (object sender, EventArgs e)
		{
			OnChanged ();
		}
	}
}

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

namespace MimeKit {
	public sealed class MailboxAddress : InternetAddress, IEquatable<MailboxAddress>
	{
		string address;

		public MailboxAddress (string name, IEnumerable<string> route, string address) : base (name)
		{
			Route = new DomainList (route);
			Route.Changed += RouteChanged;

			this.address = address;
		}

		public MailboxAddress (string name, string address) : base (name)
		{
			Route = new DomainList ();
			Route.Changed += RouteChanged;

			this.address = address;
		}

		public DomainList Route {
			get; private set;
		}

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

		internal override void Encode (StringBuilder sb, ref int lineLength, Encoding charset)
		{
			if (sb == null)
				throw new ArgumentNullException ("sb");

			if (lineLength < 0)
				throw new ArgumentOutOfRangeException ("lineLength");

			if (charset == null)
				throw new ArgumentNullException ("charset");

			string route = Route.ToString ();
			if (!string.IsNullOrEmpty (route))
				route += ":";

			if (!string.IsNullOrEmpty (Name)) {
				var encoded = Rfc2047.EncodePhrase (charset, Name);
				var str = Encoding.ASCII.GetString (encoded);

				if (lineLength + str.Length > Rfc2047.MaxLineLength) {
					if (str.Length > Rfc2047.MaxLineLength) {
						// we need to break up the name...
						sb.AppendFolded (str, ref lineLength);
					} else {
						// the name itself is short enough to fit on a single line,
						// but only if we write it on a line by itself
						if (lineLength > 1) {
							sb.LineWrap ();
							lineLength = 1;
						}

						lineLength += str.Length;
						sb.Append (str);
					}
				} else {
					// we can safely fit the name on this line...
					lineLength += str.Length;
					sb.Append (str);
				}

				if ((lineLength + route.Length + Address.Length + 3) > Rfc2047.MaxLineLength) {
					sb.Append (Environment.NewLine);
					sb.Append ("\t<");
					lineLength = 2;
				} else {
					sb.Append (" <");
					lineLength += 2;
				}

				lineLength += route.Length;
				sb.Append (route);

				lineLength += Address.Length + 1;
				sb.Append (Address);
				sb.Append ('>');
			} else if (!string.IsNullOrEmpty (route)) {
				if ((lineLength + route.Length + Address.Length + 2) > Rfc2047.MaxLineLength) {
					sb.Append (Environment.NewLine);
					sb.Append ("\t<");
					lineLength = 2;
				} else {
					sb.Append ('<');
					lineLength++;
				}

				lineLength += route.Length;
				sb.Append (route);

				lineLength += Address.Length + 1;
				sb.Append (Address);
				sb.Append ('>');
			} else {
				if ((lineLength + Address.Length) > Rfc2047.MaxLineLength) {
					sb.LineWrap ();
					lineLength = 1;
				}

				lineLength += Address.Length;
				sb.Append (Address);
			}
		}

		public override string ToString (Encoding charset, bool encode)
		{
			if (charset == null)
				throw new ArgumentNullException ("charset");

			if (encode) {
				var sb = new StringBuilder ();
				int lineLength = 0;

				Encode (sb, ref lineLength, charset);

				return sb.ToString ();
			}

			string route = Route.ToString ();
			if (!string.IsNullOrEmpty (route))
				route += ":";

			if (!string.IsNullOrEmpty (Name))
				return Rfc2047.Quote (Name) + " <" + route + Address + ">";

			if (!string.IsNullOrEmpty (route))
				return "<" + route + Address + ">";

			return Address;
		}

		#region IEquatable implementation
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

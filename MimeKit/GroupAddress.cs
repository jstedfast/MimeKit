//
// GroupAddress.cs
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
	public sealed class GroupAddress : InternetAddress, IEquatable<GroupAddress>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.GroupAddress"/> class.
		/// </summary>
		/// <param name="encoding">The character encoding to be used for encoding the name.</param>
		/// <param name="name">The name of the group.</param>
		/// <param name="addresses">A list of addresses.</param>
		public GroupAddress (Encoding encoding, string name, IEnumerable<InternetAddress> addresses) : base (encoding, name)
		{
			Members = new InternetAddressList (addresses);
			Members.Changed += MembersChanged;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.GroupAddress"/> class.
		/// </summary>
		/// <param name="name">The name of the group.</param>
		/// <param name="addresses">A list of addresses.</param>
		public GroupAddress (string name, IEnumerable<InternetAddress> addresses) : this (Encoding.UTF8, name, addresses)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.GroupAddress"/> class.
		/// </summary>
		/// <param name="encoding">The character encoding to be used for encoding the name.</param>
		/// <param name="name">The name of the group.</param>
		public GroupAddress (Encoding encoding, string name) : base (encoding, name)
		{
			Members = new InternetAddressList ();
			Members.Changed += MembersChanged;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.GroupAddress"/> class.
		/// </summary>
		/// <param name="name">The name of the group.</param>
		public GroupAddress (string name) : this (Encoding.UTF8, name)
		{
		}

		/// <summary>
		/// Gets the members of the group.
		/// </summary>
		/// <value>The list of members.</value>
		public InternetAddressList Members {
			get; private set;
		}

		internal override void Encode (StringBuilder builder, ref int lineLength)
		{
			if (builder == null)
				throw new ArgumentNullException ("builder");

			if (lineLength < 0)
				throw new ArgumentOutOfRangeException ("lineLength");

			if (!string.IsNullOrEmpty (Name)) {
				var encoded = Rfc2047.EncodePhrase (Encoding, Name);
				var str = Encoding.ASCII.GetString (encoded);

				if (lineLength + str.Length > Rfc2047.MaxLineLength) {
					if (str.Length > Rfc2047.MaxLineLength) {
						// we need to break up the name...
						builder.AppendFolded (str, ref lineLength);
					} else {
						// the name itself is short enough to fit on a single line,
						// but only if we write it on a line by itself
						if (lineLength > 1) {
							builder.LineWrap ();
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
			}

			builder.Append (": ");
			lineLength += 2;

			foreach (var member in Members)
				member.Encode (builder, ref lineLength);
		}

		/// <summary>
		/// Serializes the <see cref="MimeKit.GroupAddress"/> to a string, optionally encoding it for transport.
		/// </summary>
		/// <returns>A string representing the <see cref="MimeKit.GroupAddress"/>.</returns>
		/// <param name="encode">If set to <c>true</c>, the <see cref="MimeKit.GroupAddress"/> will be encoded.</param>
		public override string ToString (bool encode)
		{
			var builder = new StringBuilder ();

			if (encode) {
				int lineLength = 0;

				Encode (builder, ref lineLength);
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
		/// <param name="other">The <see cref="MimeKit.GroupAddress"/> to compare with the current <see cref="MimeKit.GroupAddress"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="MimeKit.GroupAddress"/> is equal to the current
		/// <see cref="MimeKit.GroupAddress"/>; otherwise, <c>false</c>.</returns>
		public bool Equals (GroupAddress other)
		{
			if (other == null)
				return false;

			return Name == other.Name && Members == other.Members;
		}

		#endregion

		void MembersChanged (object sender, EventArgs e)
		{
			OnChanged ();
		}
	}
}

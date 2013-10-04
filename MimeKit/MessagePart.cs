//
// MessagePart.cs
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
using System.IO;
using System.Collections.Generic;

namespace MimeKit {
	/// <summary>
	/// A MIME part containing a <see cref="MimeKit.MimeMessage"/> as its content.
	/// </summary>
	/// <remarks>
	/// Represents MIME parts such as those with a Content-Type of message/rfc822 or message/news.
	/// </remarks>
	public class MessagePart : MimeEntity
	{
		internal MessagePart (ParserOptions options, ContentType type, IEnumerable<Header> headers, bool toplevel) : base (options, type, headers, toplevel)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MessagePart"/> class.
		/// </summary>
		/// <param name="subtype">The message subtype.</param>
		/// <param name="args">An array of initialization parameters: headers and message parts.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="subtype"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="args"/> is <c>null</c>.</para>
		/// </exception>
		public MessagePart (string subtype, params object[] args) : this (subtype)
		{
			if (args == null)
				throw new ArgumentNullException ("args");

			MimeMessage message = null;

			foreach (object obj in args) {
				if (obj == null || base.TryInit (obj))
					continue;

				MimeMessage m = obj as MimeMessage;
				if (m != null) {
					if (message != null)
						throw new ArgumentException ("MimeMessage should not be specified more than once.");
					message = m;
					continue;
				}

				throw new ArgumentException ("Unknown initialization parameter: " + obj.GetType ());
			}

			if (message != null)
				Message = message;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MessagePart"/> class.
		/// </summary>
		/// <param name="subtype">The message subtype.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="subtype"/> is <c>null</c>.
		/// </exception>
		public MessagePart (string subtype) : base ("message", subtype)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MessagePart"/> class.
		/// </summary>
		public MessagePart () : base ("message", "rfc822")
		{
		}

		/// <summary>
		/// Gets or sets the message content.
		/// </summary>
		/// <value>The message content.</value>
		public MimeMessage Message {
			get; set;
		}

		/// <summary>
		/// Writes the <see cref="MimeKit.MessagePart"/> to the stream.
		/// </summary>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The output stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		public override void WriteTo (FormatOptions options, Stream stream)
		{
			base.WriteTo (options, stream);

			if (Message != null)
				Message.WriteTo (options, stream);
		}
	}
}

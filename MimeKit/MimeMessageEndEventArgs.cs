//
// MimeMessageEndEventArgs.cs
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

namespace MimeKit {
	/// <summary>
	/// Event args emitted by the <see cref="MimeParser"/> when a <see cref="MimeMessage"/> is parsed.
	/// </summary>
	/// <remarks>
	/// Event args emitted by the <see cref="MimeParser"/> when a <see cref="MimeMessage"/> is parsed.
	/// </remarks>
	public class MimeMessageEndEventArgs : MimeMessageBeginEventArgs
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="MimeMessageEndEventArgs"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimeMessageEndEventArgs"/>.
		/// </remarks>
		/// <param name="message">The message that was parsed.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="message"/> is <c>null</c>.
		/// </exception>
		public MimeMessageEndEventArgs (MimeMessage message) : base (message)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MimeMessageEndEventArgs"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimeMessageEndEventArgs"/>.
		/// </remarks>
		/// <param name="message">The message that was parsed.</param>
		/// <param name="parent">The parent message part.</param>
		/// <exception cref="ArgumentNullException">
		/// <para><paramref name="message"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="parent"/> is <c>null</c>.</para>
		/// </exception>
		public MimeMessageEndEventArgs (MimeMessage message, MessagePart parent) : base (message, parent)
		{
		}

		/// <summary>
		/// Get or set the stream offset that marks the end of the message headers.
		/// </summary>
		/// <remarks>
		/// Gets or sets the stream offset that marks the end of the message headers.
		/// </remarks>
		/// <value>The stream offset.</value>
		public long HeadersEndOffset { get; set; }

		/// <summary>
		/// Get or set the stream offset that marks the end of the message.
		/// </summary>
		/// <remarks>
		/// Gets or sets the stream offset that marks the end of the message.
		/// </remarks>
		/// <value>The stream offset.</value>
		public long EndOffset { get; set; }
	}
}

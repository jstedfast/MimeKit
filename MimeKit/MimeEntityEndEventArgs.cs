//
// MimeEntityEndEventArgs.cs
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
	/// Event args emitted by the <see cref="MimeParser"/> when a <see cref="MimeEntity"/> is parsed.
	/// </summary>
	/// <remarks>
	/// Event args emitted by the <see cref="MimeParser"/> when a <see cref="MimeEntity"/> is parsed.
	/// </remarks>
	public class MimeEntityEndEventArgs : MimeEntityBeginEventArgs
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="MimeEntityEndEventArgs"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimeEntityEndEventArgs"/>.
		/// </remarks>
		/// <param name="entity">The entity that was parsed.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="entity"/> is <c>null</c>.
		/// </exception>
		public MimeEntityEndEventArgs (MimeEntity entity) : base (entity)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MimeEntityEndEventArgs"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimeEntityEndEventArgs"/>.
		/// </remarks>
		/// <param name="entity">The entity that was parsed.</param>
		/// <param name="parent">The parent multipart.</param>
		/// <exception cref="ArgumentNullException">
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="parent"/> is <c>null</c>.</para>
		/// </exception>
		public MimeEntityEndEventArgs (MimeEntity entity, Multipart parent) : base (entity, parent)
		{
		}

		/// <summary>
		/// Get or set the stream offset that marks the end of the entity's headers.
		/// </summary>
		/// <remarks>
		/// Gets or sets the stream offset that marks the end of the entity's headers.
		/// </remarks>
		/// <value>The stream offset.</value>
		public long HeadersEndOffset { get; set; }

		/// <summary>
		/// Get or set the stream offset that marks the end of the entity.
		/// </summary>
		/// <remarks>
		/// Gets or sets the stream offset that marks the end of the entity.
		/// </remarks>
		/// <value>The stream offset.</value>
		public long EndOffset { get; set; }

		/// <summary>
		/// Get or set the content length of the entity as measured in lines.
		/// </summary>
		/// <remarks>
		/// <para>Get or set the content length of the entity as measured in lines.</para>
		/// <note type="note">The line count reported by this property is the number of lines in its
		/// content transfer encoding and not the resulting line count after any decoding.</note>
		/// </remarks>
		/// <value>The length of the content in lines.</value>
		public int Lines { get; set; }
	}
}

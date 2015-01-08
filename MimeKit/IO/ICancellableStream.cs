//
// ICancellableStream.cs
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

using System.Threading;

namespace MimeKit.IO {
	/// <summary>
	/// An interface allowing for a cancellable stream reading operation.
	/// </summary>
	/// <remarks>
	/// <para>This interface is meant to extend the functionality of a <see cref="System.IO.Stream"/>,
	/// allowing the <see cref="MimeKit.MimeParser"/> to have much finer-grained canellability.</para>
	/// <para>When a custom stream implementation also implements this interface,
	/// the <see cref="MimeKit.MimeParser"/> will opt to use this interface
	/// instead of the normal <see cref="System.IO.Stream.Read(byte[],int,int)"/>
	/// API to read data from the stream.</para>
	/// <para>This is really useful when parsing a message or other MIME entity
	/// directly from a network-based stream.</para>
	/// </remarks>
	public interface ICancellableStream
	{
		/// <summary>
		/// Reads a sequence of bytes from the stream and advances the position
		/// within the stream by the number of bytes read.
		/// </summary>
		/// <remarks>
		/// <para>When a custom stream implementation also implements this interface,
		/// the <see cref="MimeKit.MimeParser"/> will opt to use this interface
		/// instead of the normal <see cref="System.IO.Stream.Read(byte[],int,int)"/>
		/// API to read data from the stream.</para>
		/// <para>This is really useful when parsing a message or other MIME entity
		/// directly from a network-based stream.</para>
		/// </remarks>
		/// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many
		/// bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
		/// <param name="buffer">The buffer to read data into.</param>
		/// <param name="offset">The offset into the buffer to start reading data.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		int Read (byte[] buffer, int offset, int count, CancellationToken cancellationToken);

		/// <summary>
		/// Writes a sequence of bytes to the stream and advances the current
		/// position within this stream by the number of bytes written.
		/// </summary>
		/// <remarks>
		/// <para>When a custom stream implementation also implements this interface,
		/// writing a <see cref="MimeKit.MimeMessage"/> or <see cref="MimeKit.MimeEntity"/>
		/// to the custom stream will opt to use this interface
		/// instead of the normal <see cref="System.IO.Stream.Write(byte[],int,int)"/>
		/// API to write data to the stream.</para>
		/// <para>This is really useful when writing a message or other MIME entity
		/// directly to a network-based stream.</para>
		/// </remarks>
		/// <param name="buffer">The buffer to write.</param>
		/// <param name="offset">The offset of the first byte to write.</param>
		/// <param name="count">The number of bytes to write.</param>
		/// <param name="cancellationToken">The cancellation token</param>
		void Write (byte[] buffer, int offset, int count, CancellationToken cancellationToken);

		/// <summary>
		/// Clears all buffers for this stream and causes any buffered data to be written
		/// to the underlying device.
		/// </summary>
		/// <remarks>
		/// Clears all buffers for this stream and causes any buffered data to be written
		/// to the underlying device.
		/// </remarks>
		/// <param name="cancellationToken">The cancellation token.</param>
		void Flush (CancellationToken cancellationToken);
	}
}

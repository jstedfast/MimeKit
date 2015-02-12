//
// BoundStream.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc.
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

namespace MimeKit.IO {
	/// <summary>
	/// A bounded stream, confined to reading and writing data to a limited subset of the overall source stream.
	/// </summary>
	/// <remarks>
	/// <para>Wraps an arbitrary stream, limiting I/O operations to a subset of the source stream.
	/// If the <see cref="EndBoundary"/> is <c>-1</c>, then the end of the stream is unbound.</para>
	/// <para>When a <see cref="MimeParser"/> is set to parse a persistent stream, it will construct
	/// <see cref="ContentObject"/>s using bounded streams instead of loading the content into memory.</para>
	/// </remarks>
	public class BoundStream : Stream
	{
		long position;
		bool disposed;
		bool eos;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.IO.BoundStream"/> class.
		/// </summary>
		/// <remarks>
		/// If the <paramref name="endBoundary"/> is less than <c>0</c>, then the end of the stream
		/// is unbounded.
		/// </remarks>
		/// <param name="baseStream">The underlying stream.</param>
		/// <param name="startBoundary">The offset in the base stream that will mark the start of this substream.</param>
		/// <param name="endBoundary">The offset in the base stream that will mark the end of this substream.</param>
		/// <param name="leaveOpen"><c>true</c> to leave the baseStream open after the
		/// <see cref="BoundStream"/> is disposed; otherwise, <c>false</c>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="baseStream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="startBoundary"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="endBoundary"/> is greater than or equal to zero
		/// -and- is less than <paramref name="startBoundary"/>.</para>
		/// </exception>
		public BoundStream (Stream baseStream, long startBoundary, long endBoundary, bool leaveOpen)
		{
			if (baseStream == null)
				throw new ArgumentNullException ("baseStream");

			if (startBoundary < 0)
				throw new ArgumentOutOfRangeException ("startBoundary");

			if (endBoundary >= 0 && endBoundary < startBoundary)
				throw new ArgumentOutOfRangeException ("endBoundary");

			EndBoundary = endBoundary < 0 ? -1 : endBoundary;
			StartBoundary = startBoundary;
			BaseStream = baseStream;
			LeaveOpen = leaveOpen;
			position = 0;
			eos = false;
		}

		/// <summary>
		/// Gets the underlying stream.
		/// </summary>
		/// <remarks>
		/// All I/O is performed on the base stream.
		/// </remarks>
		/// <value>The underlying stream.</value>
		public Stream BaseStream {
			get; private set;
		}

		/// <summary>
		/// Gets the start boundary offset of the underlying stream.
		/// </summary>
		/// <remarks>
		/// The start boundary is the byte offset into the <see cref="BaseStream"/>
		/// that marks the beginning of the substream.
		/// </remarks>
		/// <value>The start boundary offset of the underlying stream.</value>
		public long StartBoundary {
			get; private set;
		}

		/// <summary>
		/// Gets the end boundary offset of the underlying stream.
		/// </summary>
		/// <remarks>
		/// The end boundary is the byte offset into the <see cref="BaseStream"/>
		/// that marks the end of the substream. If the value is less than 0,
		/// then the end of the stream is treated as unbound.
		/// </remarks>
		/// <value>The end boundary offset of the underlying stream.</value>
		public long EndBoundary {
			get; private set;
		}

		/// <summary>
		/// Checks whether or not the underlying stream will remain open after
		/// the <see cref="MimeKit.IO.BoundStream"/> is disposed.
		/// </summary>
		/// <value><c>true</c> if the underlying stream should remain open after the
		/// <see cref="BoundStream"/> is disposed; otherwise, <c>false</c>.</value>
		bool LeaveOpen {
			get; set;
		}

		void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException ("Substream");
		}

		void CheckCanSeek ()
		{
			if (!BaseStream.CanSeek)
				throw new NotSupportedException ("The stream does not support seeking");
		}

		void CheckCanRead ()
		{
			if (!BaseStream.CanRead)
				throw new NotSupportedException ("The stream does not support reading");
		}

		void CheckCanWrite ()
		{
			if (!BaseStream.CanWrite)
				throw new NotSupportedException ("The stream does not support writing");
		}

		#region Stream implementation

		/// <summary>
		/// Checks whether or not the stream supports reading.
		/// </summary>
		/// <remarks>
		/// The <see cref="BoundStream"/> will only support reading if the
		/// <see cref="BaseStream"/> supports it.
		/// </remarks>
		/// <value><c>true</c> if the stream supports reading; otherwise, <c>false</c>.</value>
		public override bool CanRead {
			get { return BaseStream.CanRead; }
		}

		/// <summary>
		/// Checks whether or not the stream supports writing.
		/// </summary>
		/// <remarks>
		/// The <see cref="BoundStream"/> will only support writing if the
		/// <see cref="BaseStream"/> supports it.
		/// </remarks>
		/// <value><c>true</c> if the stream supports writing; otherwise, <c>false</c>.</value>
		public override bool CanWrite {
			get { return BaseStream.CanWrite; }
		}

		/// <summary>
		/// Checks whether or not the stream supports seeking.
		/// </summary>
		/// <remarks>
		/// The <see cref="BoundStream"/> will only support seeking if the
		/// <see cref="BaseStream"/> supports it.
		/// </remarks>
		/// <value><c>true</c> if the stream supports seeking; otherwise, <c>false</c>.</value>
		public override bool CanSeek {
			get { return BaseStream.CanSeek; }
		}

		/// <summary>
		/// Checks whether or not I/O operations can timeout.
		/// </summary>
		/// <remarks>
		/// The <see cref="BoundStream"/> will only support timing out if the
		/// <see cref="BaseStream"/> supports it.
		/// </remarks>
		/// <value><c>true</c> if I/O operations can timeout; otherwise, <c>false</c>.</value>
		public override bool CanTimeout {
			get { return BaseStream.CanTimeout; }
		}

		/// <summary>
		/// Gets the length of the stream, in bytes.
		/// </summary>
		/// <remarks>
		/// If the <see cref="EndBoundary"/> property is greater than or equal to <c>0</c>,
		/// then the length will be calculated by subtracting the <see cref="StartBoundary"/>
		/// from the <see cref="EndBoundary"/>. If the end of the stream is unbound, then the
		/// <see cref="StartBoundary"/> will be subtracted from the length of the
		/// <see cref="BaseStream"/>.
		/// </remarks>
		/// <value>The length of the stream in bytes.</value>
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support seeking.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The stream has been disposed.
		/// </exception>
		public override long Length {
			get {
				CheckDisposed ();

				if (EndBoundary != -1)
					return EndBoundary - StartBoundary;

				if (eos)
					return position;

				return BaseStream.Length - StartBoundary;
			}
		}

		/// <summary>
		/// Gets or sets the current position within the stream.
		/// </summary>
		/// <remarks>
		/// The <see cref="Position"/> is relative to the <see cref="StartBoundary"/>.
		/// </remarks>
		/// <value>The position of the stream.</value>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support seeking.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The stream has been disposed.
		/// </exception>
		public override long Position {
			get { return position; }
			set { Seek (value, SeekOrigin.Begin); }
		}

		/// <summary>
		/// Gets or sets a value, in miliseconds, that determines how long the stream will attempt to read before timing out.
		/// </summary>
		/// <remarks>
		/// Gets or sets the <see cref="BaseStream"/>'s read timeout.
		/// </remarks>
		/// <value>A value, in miliseconds, that determines how long the stream will attempt to read before timing out.</value>
		public override int ReadTimeout {
			get { return BaseStream.ReadTimeout; }
			set { BaseStream.ReadTimeout = value; }
		}

		/// <summary>
		/// Gets or sets a value, in miliseconds, that determines how long the stream will attempt to write before timing out.
		/// </summary>
		/// <remarks>
		/// Gets or sets the <see cref="BaseStream"/>'s write timeout.
		/// </remarks>
		/// <value>A value, in miliseconds, that determines how long the stream will attempt to write before timing out.</value>
		public override int WriteTimeout {
			get { return BaseStream.WriteTimeout; }
			set { BaseStream.WriteTimeout = value; }
		}

		static void ValidateArguments (byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (offset < 0 || offset > buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");

			if (count < 0 || offset + count > buffer.Length)
				throw new ArgumentOutOfRangeException ("count");
		}

		/// <summary>
		/// Reads a sequence of bytes from the stream and advances the position
		/// within the stream by the number of bytes read.
		/// </summary>
		/// <remarks>
		/// Reads data from the <see cref="BaseStream"/>, not allowing it to
		/// read beyond the <see cref="EndBoundary"/>.
		/// </remarks>
		/// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many
		/// bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
		/// <param name="buffer">The buffer to read data into.</param>
		/// <param name="offset">The offset into the buffer to start reading data.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="offset"/> is less than zero or greater than the length of <paramref name="buffer"/>.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="buffer"/> is not large enough to contain <paramref name="count"/> bytes starting
		/// at the specified <paramref name="offset"/>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The stream has been disposed.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support reading.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public override int Read (byte[] buffer, int offset, int count)
		{
			CheckDisposed ();
			CheckCanRead ();

			ValidateArguments (buffer, offset, count);

			// if we are at the end of the stream, we cannot read anymore data
			if (EndBoundary != -1 && StartBoundary + position >= EndBoundary) {
				eos = true;
				return 0;
			}

			// make sure that the source stream is in the expected position
			if (BaseStream.Position != StartBoundary + position)
				BaseStream.Seek (StartBoundary + position, SeekOrigin.Begin);

			int n = EndBoundary != -1 ? (int) Math.Min (EndBoundary - (StartBoundary + position), count) : count;
			int nread = BaseStream.Read (buffer, offset, n);

			if (nread > 0)
				position += nread;
			else if (nread == 0)
				eos = true;

			return nread;
		}

		/// <summary>
		/// Writes a sequence of bytes to the stream and advances the current
		/// position within this stream by the number of bytes written.
		/// </summary>
		/// <remarks>
		/// Writes data to the <see cref="BaseStream"/>, not allowing it to
		/// write beyond the <see cref="EndBoundary"/>.
		/// </remarks>
		/// <param name="buffer">The buffer to write.</param>
		/// <param name="offset">The offset of the first byte to write.</param>
		/// <param name="count">The number of bytes to write.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="offset"/> is less than zero or greater than the length of <paramref name="buffer"/>.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="buffer"/> is not large enough to contain <paramref name="count"/> bytes starting
		/// at the specified <paramref name="offset"/>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The stream has been disposed.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support writing.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public override void Write (byte[] buffer, int offset, int count)
		{
			CheckDisposed ();
			CheckCanWrite ();

			ValidateArguments (buffer, offset, count);

			// if we are at the end of the stream, we cannot write anymore data
			if (EndBoundary != -1 && StartBoundary + position + count > EndBoundary) {
				eos = StartBoundary + position >= EndBoundary;
				throw new IOException ();
			}

			// make sure that the source stream is in the expected position
			if (BaseStream.Position != StartBoundary + position)
				BaseStream.Seek (StartBoundary + position, SeekOrigin.Begin);

			BaseStream.Write (buffer, offset, count);
			position += count;

			if (EndBoundary != -1 && StartBoundary + position >= EndBoundary)
				eos = true;
		}

		/// <summary>
		/// Sets the position within the current stream.
		/// </summary>
		/// <remarks>
		/// Seeks within the confines of the <see cref="StartBoundary"/> and the <see cref="EndBoundary"/>.
		/// </remarks>
		/// <returns>The new position within the stream.</returns>
		/// <param name="offset">The offset into the stream relative to the <paramref name="origin"/>.</param>
		/// <param name="origin">The origin to seek from.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="origin"/> is not a valid <see cref="System.IO.SeekOrigin"/>. 
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The stream has been disposed.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support seeking.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public override long Seek (long offset, SeekOrigin origin)
		{
			CheckDisposed ();
			CheckCanSeek ();

			long real;

			switch (origin) {
			case SeekOrigin.Begin:
				real = StartBoundary + offset;
				break;
			case SeekOrigin.Current:
				real = position + offset;
				break;
			case SeekOrigin.End:
				if (offset >= 0 || (EndBoundary == -1 && !eos)) {
					// We don't know if the underlying stream can seek past the end or not...
					if ((real = BaseStream.Seek (offset, origin)) == -1)
						return -1;
				} else if (EndBoundary == -1) {
					// seeking backwards from eos (which happens to be our current position)
					real = position + offset;
				} else {
					// seeking backwards from a known position
					real = EndBoundary + offset;
				}
				
				break;
			default:
				throw new ArgumentOutOfRangeException ("origin", "Invalid SeekOrigin specified");
			}
			
			// sanity check the resultant offset
			if (real < StartBoundary)
				throw new IOException ("Cannot seek to a position before the beginning of the stream");
			
			// short-cut if we are seeking to our current position
			if (real == StartBoundary + position)
				return position;
			
			if (EndBoundary != -1 && real > EndBoundary)
				throw new IOException ("Cannot seek beyond the end of the stream");
			
			if ((real = BaseStream.Seek (real, SeekOrigin.Begin)) == -1)
				return -1;
			
			// reset eos if appropriate
			if ((EndBoundary != -1 && real < EndBoundary) || (eos && real < StartBoundary + position))
				eos = false;
			
			position = real - StartBoundary;

			return position;
		}

		/// <summary>
		/// Clears all buffers for this stream and causes any buffered data to be written
		/// to the underlying device.
		/// </summary>
		/// <remarks>
		/// Flushes the <see cref="BaseStream"/>.
		/// </remarks>
		/// <exception cref="System.ObjectDisposedException">
		/// The stream has been disposed.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support writing.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public override void Flush ()
		{
			CheckDisposed ();
			CheckCanWrite ();

			BaseStream.Flush ();
		}

		/// <summary>
		/// Sets the length of the stream.
		/// </summary>
		/// <remarks>
		/// Updates the <see cref="EndBoundary"/> to be <see cref="StartBoundary"/> plus
		/// the specified new length. If the <see cref="BaseStream"/> needs to be grown
		/// to allow this, then the length of the <see cref="BaseStream"/> will also be
		/// updated.
		/// </remarks>
		/// <param name="value">The desired length of the stream in bytes.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The stream has been disposed.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support setting the length.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public override void SetLength (long value)
		{
			CheckDisposed ();

			if (EndBoundary == -1 || StartBoundary + value > EndBoundary) {
				long end = BaseStream.Length;

				if (StartBoundary + value > end)
					BaseStream.SetLength (StartBoundary + value);
				
				EndBoundary = StartBoundary + value;
			} else {
				EndBoundary = StartBoundary + value;
			}
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="BoundStream"/> and
		/// optionally releases the managed resources.
		/// </summary>
		/// <remarks>
		/// If the <see cref="LeaveOpen"/> property is <c>false</c>, then
		/// the <see cref="BaseStream"/> is also disposed.
		/// </remarks>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
		/// <c>false</c> to release only the unmanaged resources.</param>
		protected override void Dispose (bool disposing)
		{
			if (disposing && !LeaveOpen)
				BaseStream.Dispose ();

			base.Dispose (disposing);
			disposed = true;
		}
		
		#endregion
	}
}

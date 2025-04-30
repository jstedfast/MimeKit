﻿//
// FilteredStream.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2025 .NET Foundation and Contributors
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
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using MimeKit.IO.Filters;

namespace MimeKit.IO {
	/// <summary>
	/// A stream which filters data as it is read or written.
	/// </summary>
	/// <remarks>
	/// Passes data through each <see cref="IMimeFilter"/> as the data is read or written.
	/// </remarks>
	public class FilteredStream : Stream, ICancellableStream
	{
		const int ReadBufferSize = 4096;

		enum IOOperation : byte {
			Read,
			Write
		}

		List<IMimeFilter> filters = new List<IMimeFilter> ();
		IOOperation lastOp = IOOperation.Write;
		int filteredLength;
		int filteredIndex;
		byte[] filtered;
		byte[] readbuf;
		bool disposed;
		bool flushed;

		/// <summary>
		/// Initialize a new instance of the <see cref="FilteredStream"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a filtered stream using the specified source stream.
		/// The source stream will be left open when this <see cref="FilteredStream"/> is disposed.
		/// </remarks>
		/// <param name="source">The underlying stream to filter.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="source"/> is <see langword="null"/>.
		/// </exception>
		public FilteredStream (Stream source)
		{
			if (source is null)
				throw new ArgumentNullException (nameof (source));

			Source = source;
		}

		/// <summary>
		/// Get the underlying source stream.
		/// </summary>
		/// <remarks>
		/// In general, it is not a good idea to manipulate the underlying
		/// source stream because most <see cref="IMimeFilter"/>s store
		/// important state about previous bytes read from or written to
		/// the source stream.
		/// </remarks>
		/// <value>The underlying source stream.</value>
		public Stream Source {
			get; private set;
		}

		/// <summary>
		/// Add a filter.
		/// </summary>
		/// <remarks>
		/// Adds the <paramref name="filter"/> to the end of the list of filters
		/// that data will pass through as data is read from or written to the
		/// underlying source stream.
		/// </remarks>
		/// <param name="filter">The filter.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="filter"/> is <see langword="null"/>.
		/// </exception>
		public void Add (IMimeFilter filter)
		{
			CheckDisposed ();

			if (filter is null)
				throw new ArgumentNullException (nameof (filter));

			filters.Add (filter);
		}

		/// <summary>
		/// Check if the filtered stream contains the specified filter.
		/// </summary>
		/// <remarks>
		/// Determines whether the filtered stream contains the specified filter.
		/// </remarks>
		/// <returns><see langword="true" /> if the specified filter exists;
		/// otherwise, <see langword="false" />.</returns>
		/// <param name="filter">The filter.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="filter"/> is <see langword="null"/>.
		/// </exception>
		public bool Contains (IMimeFilter filter)
		{
			CheckDisposed ();

			if (filter is null)
				throw new ArgumentNullException (nameof (filter));

			return filters.Contains (filter);
		}

		/// <summary>
		/// Remove a filter.
		/// </summary>
		/// <remarks>
		/// Removes the specified filter from the list if it exists.
		/// </remarks>
		/// <returns><see langword="true" /> if the filter was removed; otherwise, <see langword="false" />.</returns>
		/// <param name="filter">The filter.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="filter"/> is <see langword="null"/>.
		/// </exception>
		public bool Remove (IMimeFilter filter)
		{
			CheckDisposed ();

			if (filter is null)
				throw new ArgumentNullException (nameof (filter));

			return filters.Remove (filter);
		}

		void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (nameof (FilteredStream));
		}

		void CheckCanRead ()
		{
			if (!Source.CanRead)
				throw new NotSupportedException ("The stream does not support reading");
		}

		void CheckCanWrite ()
		{
			if (!Source.CanWrite)
				throw new NotSupportedException ("The stream does not support writing");
		}

		#region Stream implementation

		/// <summary>
		/// Check whether the stream supports reading.
		/// </summary>
		/// <remarks>
		/// The <see cref="FilteredStream"/> will only support reading if the
		/// <see cref="Source"/> supports it.
		/// </remarks>
		/// <value><see langword="true" /> if the stream supports reading; otherwise, <see langword="false" />.</value>
		public override bool CanRead {
			get { return Source.CanRead; }
		}

		/// <summary>
		/// Check whether the stream supports writing.
		/// </summary>
		/// <remarks>
		/// The <see cref="FilteredStream"/> will only support writing if the
		/// <see cref="Source"/> supports it.
		/// </remarks>
		/// <value><see langword="true" /> if the stream supports writing; otherwise, <see langword="false" />.</value>
		public override bool CanWrite {
			get { return Source.CanWrite; }
		}

		/// <summary>
		/// Check whether the stream supports seeking.
		/// </summary>
		/// <remarks>
		/// Seeking is not supported by the <see cref="FilteredStream"/>.
		/// </remarks>
		/// <value><see langword="true" /> if the stream supports seeking; otherwise, <see langword="false" />.</value>
		public override bool CanSeek {
			get { return false; }
		}

		/// <summary>
		/// Check whether I/O operations can time out.
		/// </summary>
		/// <remarks>
		/// The <see cref="FilteredStream"/> will only support timing out if the
		/// <see cref="Source"/> supports it.
		/// </remarks>
		/// <value><see langword="true" /> if I/O operations can time out; otherwise, <see langword="false" />.</value>
		public override bool CanTimeout {
			get { return Source.CanTimeout; }
		}

		/// <summary>
		/// Get the length of the stream, in bytes.
		/// </summary>
		/// <remarks>
		/// Getting the length of a <see cref="FilteredStream"/> is not supported.
		/// </remarks>
		/// <value>The length of the stream in bytes.</value>
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support seeking.
		/// </exception>
		public override long Length {
			get { throw new NotSupportedException ("Cannot get the length of the stream"); }
		}

		/// <summary>
		/// Get or set the current position within the stream.
		/// </summary>
		/// <remarks>
		/// Getting and setting the position of a <see cref="FilteredStream"/> is not supported.
		/// </remarks>
		/// <value>The position of the stream.</value>
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support seeking.
		/// </exception>
		public override long Position {
			get { throw new NotSupportedException ("The stream does not support seeking"); }
			set { throw new NotSupportedException ("The stream does not support seeking"); }
		}

		/// <summary>
		/// Get or set a value, in milliseconds, that determines how long the stream will attempt to read before timing out.
		/// </summary>
		/// <remarks>
		/// Gets or sets the read timeout on the <see cref="Source"/> stream.
		/// </remarks>
		/// <value>A value, in milliseconds, that determines how long the stream will attempt to read before timing out.</value>
		public override int ReadTimeout
		{
			get { return Source.ReadTimeout; }
			set { Source.ReadTimeout = value; }
		}

		/// <summary>
		/// Get or set a value, in milliseconds, that determines how long the stream will attempt to write before timing out.
		/// </summary>
		/// <remarks>
		/// Gets or sets the write timeout on the <see cref="Source"/> stream.
		/// </remarks>
		/// <value>A value, in milliseconds, that determines how long the stream will attempt to write before timing out.</value>
		public override int WriteTimeout
		{
			get { return Source.WriteTimeout; }
			set { Source.WriteTimeout = value; }
		}

		static void ValidateArguments (byte[] buffer, int offset, int count)
		{
			if (buffer is null)
				throw new ArgumentNullException (nameof (buffer));

			if (offset < 0 || offset > buffer.Length)
				throw new ArgumentOutOfRangeException (nameof (offset));

			if (count < 0 || count > (buffer.Length - offset))
				throw new ArgumentOutOfRangeException (nameof (count));
		}

		/// <summary>
		/// Read a sequence of bytes from the stream and advances the position
		/// within the stream by the number of bytes read.
		/// </summary>
		/// <remarks>
		/// Reads up to the requested number of bytes, passing the data read from the <see cref="Source"/> stream
		/// through each of the filters before finally copying the result into the provided buffer.
		/// </remarks>
		/// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if
		/// that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
		/// <param name="buffer">The buffer to read data into.</param>
		/// <param name="offset">The offset into the buffer to start reading data.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <see langword="null"/>.
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
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public int Read (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			CheckDisposed ();
			CheckCanRead ();

			ValidateArguments (buffer, offset, count);

			lastOp = IOOperation.Read;
			readbuf ??= new byte[ReadBufferSize];

			int nread;

			if (filteredLength == 0) {
				if (Source is ICancellableStream cancellable) {
					if ((nread = cancellable.Read (readbuf, 0, ReadBufferSize, cancellationToken)) <= 0)
						return nread;
				} else {
					cancellationToken.ThrowIfCancellationRequested ();
					if ((nread = Source.Read (readbuf, 0, ReadBufferSize)) <= 0)
						return nread;
				}

				// filter the data we've just read...
				filteredLength = nread;
				filteredIndex = 0;
				filtered = readbuf;

				foreach (var filter in filters)
					filtered = filter.Filter (filtered, filteredIndex, filteredLength, out filteredIndex, out filteredLength);
			}

			// copy our filtered data into our caller's buffer
			nread = Math.Min (filteredLength, count);

			if (nread > 0) {
				Buffer.BlockCopy (filtered, filteredIndex, buffer, offset, nread);
				filteredLength -= nread;
				filteredIndex += nread;
			}

			return nread;
		}

		/// <summary>
		/// Read a sequence of bytes from the stream and advances the position
		/// within the stream by the number of bytes read.
		/// </summary>
		/// <remarks>
		/// Reads up to the requested number of bytes, passing the data read from the <see cref="Source"/> stream
		/// through each of the filters before finally copying the result into the provided buffer.
		/// </remarks>
		/// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if
		/// that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
		/// <param name="buffer">The buffer to read data into.</param>
		/// <param name="offset">The offset into the buffer to start reading data.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <see langword="null"/>.
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
			return Read (buffer, offset, count, CancellationToken.None);
		}

		/// <summary>
		/// Asynchronously read a sequence of bytes from the stream and advances the position
		/// within the stream by the number of bytes read.
		/// </summary>
		/// <remarks>
		/// Reads up to the requested number of bytes, passing the data read from the <see cref="Source"/> stream
		/// through each of the filters before finally copying the result into the provided buffer.
		/// </remarks>
		/// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if
		/// that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
		/// <param name="buffer">The buffer to read data into.</param>
		/// <param name="offset">The offset into the buffer to start reading data.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <see langword="null"/>.
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
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public override async Task<int> ReadAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			CheckDisposed ();
			CheckCanRead ();

			ValidateArguments (buffer, offset, count);

			lastOp = IOOperation.Read;
			readbuf ??= new byte[ReadBufferSize];

			int nread;

			if (filteredLength == 0) {
				if ((nread = await Source.ReadAsync (readbuf, 0, ReadBufferSize, cancellationToken).ConfigureAwait (false)) <= 0)
					return nread;

				// filter the data we've just read...
				filteredLength = nread;
				filteredIndex = 0;
				filtered = readbuf;

				foreach (var filter in filters)
					filtered = filter.Filter (filtered, filteredIndex, filteredLength, out filteredIndex, out filteredLength);
			}

			// copy our filtered data into our caller's buffer
			nread = Math.Min (filteredLength, count);

			if (nread > 0) {
				Buffer.BlockCopy (filtered, filteredIndex, buffer, offset, nread);
				filteredLength -= nread;
				filteredIndex += nread;
			}

			return nread;
		}

		/// <summary>
		/// Write a sequence of bytes to the stream and advances the current
		/// position within this stream by the number of bytes written.
		/// </summary>
		/// <remarks>
		/// Filters the provided buffer through each of the filters before finally writing
		/// the result to the underlying <see cref="Source"/> stream.
		/// </remarks>
		/// <param name="buffer">The buffer to write.</param>
		/// <param name="offset">The offset of the first byte to write.</param>
		/// <param name="count">The number of bytes to write.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <see langword="null"/>.
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
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public void Write (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			CheckDisposed ();
			CheckCanWrite ();

			ValidateArguments (buffer, offset, count);

			lastOp = IOOperation.Write;
			flushed = false;

			filteredIndex = offset;
			filteredLength = count;
			filtered = buffer;

			foreach (var filter in filters)
				filtered = filter.Filter (filtered, filteredIndex, filteredLength, out filteredIndex, out filteredLength);

			if (filteredLength == 0)
				return;

			if (Source is ICancellableStream cancellable) {
				cancellable.Write (filtered, filteredIndex, filteredLength, cancellationToken);
			} else {
				cancellationToken.ThrowIfCancellationRequested ();
				Source.Write (filtered, filteredIndex, filteredLength);
			}
		}

		/// <summary>
		/// Write a sequence of bytes to the stream and advances the current
		/// position within this stream by the number of bytes written.
		/// </summary>
		/// <remarks>
		/// Filters the provided buffer through each of the filters before finally writing
		/// the result to the underlying <see cref="Source"/> stream.
		/// </remarks>
		/// <param name="buffer">The buffer to write.</param>
		/// <param name="offset">The offset of the first byte to write.</param>
		/// <param name="count">The number of bytes to write.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <see langword="null"/>.
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
			Write (buffer, offset, count, CancellationToken.None);
		}

		/// <summary>
		/// Asynchronously write a sequence of bytes to the stream and advances the current
		/// position within this stream by the number of bytes written.
		/// </summary>
		/// <remarks>
		/// Filters the provided buffer through each of the filters before finally writing
		/// the result to the underlying <see cref="Source"/> stream.
		/// </remarks>
		/// <returns>A task that represents the asynchronous write operation.</returns>
		/// <param name="buffer">The buffer to write.</param>
		/// <param name="offset">The offset of the first byte to write.</param>
		/// <param name="count">The number of bytes to write.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <see langword="null"/>.
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
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public override Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			CheckDisposed ();
			CheckCanWrite ();

			ValidateArguments (buffer, offset, count);

			lastOp = IOOperation.Write;
			flushed = false;

			filteredIndex = offset;
			filteredLength = count;
			filtered = buffer;

			foreach (var filter in filters)
				filtered = filter.Filter (filtered, filteredIndex, filteredLength, out filteredIndex, out filteredLength);

			return Source.WriteAsync (filtered, filteredIndex, filteredLength, cancellationToken);
		}

		/// <summary>
		/// Set the position within the current stream.
		/// </summary>
		/// <remarks>
		/// Seeking is not supported by the <see cref="FilteredStream"/>.
		/// </remarks>
		/// <returns>The new position within the stream.</returns>
		/// <param name="offset">The offset into the stream relative to the <paramref name="origin"/>.</param>
		/// <param name="origin">The origin to seek from.</param>
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support seeking.
		/// </exception>
		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotSupportedException ("The stream does not support seeking");
		}

		/// <summary>
		/// Clear all buffers for this stream and causes any buffered data to be written
		/// to the underlying device.
		/// </summary>
		/// <remarks>
		/// Flushes the state of all filters, writing any output to the underlying <see cref="Source"/>
		/// stream and then calling <see cref="System.IO.Stream.Flush"/> on the <see cref="Source"/>.
		/// </remarks>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The stream has been disposed.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support writing.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public void Flush (CancellationToken cancellationToken)
		{
			CheckDisposed ();
			CheckCanWrite ();

			if (lastOp == IOOperation.Read)
				return;

			if (!flushed) {
				filtered = Array.Empty<byte> ();
				filteredIndex = 0;
				filteredLength = 0;

				foreach (var filter in filters)
					filtered = filter.Flush (filtered, filteredIndex, filteredLength, out filteredIndex, out filteredLength);

				flushed = true;
			}

			if (filteredLength > 0) {
				if (Source is ICancellableStream cancellable) {
					cancellable.Write (filtered, filteredIndex, filteredLength, cancellationToken);
				} else {
					cancellationToken.ThrowIfCancellationRequested ();
					Source.Write (filtered, filteredIndex, filteredLength);
				}

				filteredIndex = 0;
				filteredLength = 0;
			}
		}

		/// <summary>
		/// Clear all buffers for this stream and causes any buffered data to be written
		/// to the underlying device.
		/// </summary>
		/// <remarks>
		/// Flushes the state of all filters, writing any output to the underlying <see cref="Source"/>
		/// stream and then calling <see cref="System.IO.Stream.Flush"/> on the <see cref="Source"/>.
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
			Flush (CancellationToken.None);
		}

		/// <summary>
		/// Asynchronously clear all buffers for this stream and causes any buffered data to be written
		/// to the underlying device.
		/// </summary>
		/// <remarks>
		/// Flushes the state of all filters, writing any output to the underlying <see cref="Source"/>
		/// stream and then calling <see cref="System.IO.Stream.Flush"/> on the <see cref="Source"/>.
		/// </remarks>
		/// <returns>A task that represents the asynchronous flush operation.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The stream has been disposed.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support writing.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public override async Task FlushAsync (CancellationToken cancellationToken)
		{
			CheckDisposed ();
			CheckCanWrite ();

			if (lastOp == IOOperation.Read)
				return;

			if (!flushed) {
				filtered = Array.Empty<byte> ();
				filteredIndex = 0;
				filteredLength = 0;

				foreach (var filter in filters)
					filtered = filter.Flush (filtered, filteredIndex, filteredLength, out filteredIndex, out filteredLength);

				flushed = true;
			}

			if (filteredLength > 0) {
				await Source.WriteAsync (filtered, filteredIndex, filteredLength, cancellationToken).ConfigureAwait (false);

				filteredIndex = 0;
				filteredLength = 0;
			}
		}

		/// <summary>
		/// Set the length of the stream.
		/// </summary>
		/// <remarks>
		/// Setting the length of a <see cref="FilteredStream"/> is not supported.
		/// </remarks>
		/// <param name="value">The desired length of the stream in bytes.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The stream has been disposed.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support setting the length.
		/// </exception>
		public override void SetLength (long value)
		{
			CheckDisposed ();

			throw new NotSupportedException ("Cannot set a length on the stream");
		}

		/// <summary>
		/// Release the unmanaged resources used by the <see cref="FilteredStream"/> and
		/// optionally releases the managed resources.
		/// </summary>
		/// <remarks>
		/// Releases the unmanaged resources used by the <see cref="FilteredStream"/> and
		/// optionally releases the managed resources.
		/// </remarks>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources;
		/// <see langword="false" /> to release only the unmanaged resources.</param>
		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (filters != null) {
					filters.Clear ();
					filters = null;
				}

				readbuf = null;
			}

			base.Dispose (disposing);
			disposed = true;
		}

		#endregion
	}
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;

namespace MimeKit.IO {
	/// <summary>
	/// An interface allowing for a cancellable stream reading operation.
	/// </summary>
	/// <remarks>
	/// <para>This interface is meant to extend the functionality of a <see cref="System.IO.Stream"/>,
	/// allowing the <see cref="MimeParser"/> to have much finer-grained canellability.</para>
	/// <para>When a custom stream implementation also implements this interface,
	/// the <see cref="MimeParser"/> will opt to use this interface
	/// instead of the normal <see cref="System.IO.Stream.Read(byte[],int,int)"/>
	/// API to read data from the stream.</para>
	/// <para>This is really useful when parsing a message or other MIME entity
	/// directly from a network-based stream.</para>
	/// </remarks>
	public interface ICancellableStream
	{
		/// <summary>
		/// Read a sequence of bytes from the stream and advances the position
		/// within the stream by the number of bytes read.
		/// </summary>
		/// <remarks>
		/// <para>When a custom stream implementation also implements this interface,
		/// the <see cref="MimeParser"/> will opt to use this interface
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
		/// Write a sequence of bytes to the stream and advances the current
		/// position within this stream by the number of bytes written.
		/// </summary>
		/// <remarks>
		/// <para>When a custom stream implementation also implements this interface,
		/// writing a <see cref="MimeMessage"/> or <see cref="MimeEntity"/>
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
		/// Clear all buffers for this stream and causes any buffered data to be written
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

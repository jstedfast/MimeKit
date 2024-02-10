//
// DkimSignatureStream.cs
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
using System.IO;

using Org.BouncyCastle.Crypto;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A DKIM signature stream.
	/// </summary>
	/// <remarks>
	/// A DKIM signature stream.
	/// </remarks>
	class DkimSignatureStream : Stream
	{
		bool disposed;
		long length;

		/// <summary>
		/// Initialize a new instance of the <see cref="DkimSignatureStream"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="DkimSignatureStream"/>.
		/// </remarks>
		/// <param name="signer">The digest signer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="signer"/>is <c>null</c>.
		/// </exception>
		public DkimSignatureStream (ISigner signer)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			Signer = signer;
		}

		/// <summary>
		/// Get the digest signer.
		/// </summary>
		/// <remarks>
		/// Gets the digest signer.
		/// </remarks>
		/// <value>The signer.</value>
		public ISigner Signer {
			get; private set;
		}

		/// <summary>
		/// Generate the signature.
		/// </summary>
		/// <remarks>
		/// Generates the signature.
		/// </remarks>
		/// <returns>The signature.</returns>
		public byte[] GenerateSignature ()
		{
			return Signer.GenerateSignature ();
		}

		/// <summary>
		/// Verify the DKIM signature.
		/// </summary>
		/// <remarks>
		/// Verifies the DKIM signature.
		/// </remarks>
		/// <returns><c>true</c> if signature is valid; otherwise, <c>false</c>.</returns>
		/// <param name="signature">The base64 encoded DKIM signature from the <c>b=</c> parameter.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="signature"/> is <c>null</c>.
		/// </exception>
		public bool VerifySignature (string signature)
		{
			if (signature == null)
				throw new ArgumentNullException (nameof (signature));

			var rawSignature = Convert.FromBase64String (signature);

			return Signer.VerifySignature (rawSignature);
		}

		void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (nameof (DkimSignatureStream));
		}

		/// <summary>
		/// Checks whether or not the stream supports reading.
		/// </summary>
		/// <remarks>
		/// A <see cref="DkimSignatureStream"/> is not readable.
		/// </remarks>
		/// <value><c>true</c> if the stream supports reading; otherwise, <c>false</c>.</value>
		public override bool CanRead {
			get { return false; }
		}

		/// <summary>
		/// Checks whether or not the stream supports writing.
		/// </summary>
		/// <remarks>
		/// A <see cref="DkimSignatureStream"/> is always writable.
		/// </remarks>
		/// <value><c>true</c> if the stream supports writing; otherwise, <c>false</c>.</value>
		public override bool CanWrite {
			get { return true; }
		}

		/// <summary>
		/// Checks whether or not the stream supports seeking.
		/// </summary>
		/// <remarks>
		/// A <see cref="DkimSignatureStream"/> is not seekable.
		/// </remarks>
		/// <value><c>true</c> if the stream supports seeking; otherwise, <c>false</c>.</value>
		public override bool CanSeek {
			get { return false; }
		}

		/// <summary>
		/// Checks whether or not reading and writing to the stream can timeout.
		/// </summary>
		/// <remarks>
		/// Writing to a <see cref="DkimSignatureStream"/> cannot timeout.
		/// </remarks>
		/// <value><c>true</c> if reading and writing to the stream can timeout; otherwise, <c>false</c>.</value>
		public override bool CanTimeout {
			get { return false; }
		}

		/// <summary>
		/// Gets the length of the stream, in bytes.
		/// </summary>
		/// <remarks>
		/// The length of a <see cref="DkimSignatureStream"/> indicates the
		/// number of bytes that have been written to it.
		/// </remarks>
		/// <value>The length of the stream in bytes.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The stream has been disposed.
		/// </exception>
		public override long Length {
			get {
				CheckDisposed ();

				return length;
			}
		}

		/// <summary>
		/// Gets or sets the current position within the stream.
		/// </summary>
		/// <remarks>
		/// Since it is possible to seek within a <see cref="DkimSignatureStream"/>,
		/// it is possible that the position will not always be identical to the
		/// length of the stream, but typically it will be.
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
			get { return length; }
			set { Seek (value, SeekOrigin.Begin); }
		}

		static void ValidateArguments (byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException (nameof (buffer));

			if (offset < 0 || offset > buffer.Length)
				throw new ArgumentOutOfRangeException (nameof (offset));

			if (count < 0 || count > (buffer.Length - offset))
				throw new ArgumentOutOfRangeException (nameof (count));
		}

		/// <summary>
		/// Reads a sequence of bytes from the stream and advances the position
		/// within the stream by the number of bytes read.
		/// </summary>
		/// <remarks>
		/// Reading from a <see cref="DkimSignatureStream"/> is not supported.
		/// </remarks>
		/// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many
		/// bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
		/// <param name="buffer">The buffer to read data into.</param>
		/// <param name="offset">The offset into the buffer to start reading data.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The stream has been disposed.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The stream does not support reading.
		/// </exception>
		public override int Read (byte[] buffer, int offset, int count)
		{
			CheckDisposed ();

			throw new NotSupportedException ("The stream does not support reading");
		}

		/// <summary>
		/// Writes a sequence of bytes to the stream and advances the current
		/// position within this stream by the number of bytes written.
		/// </summary>
		/// <remarks>
		/// Increments the <see cref="Position"/> property by the number of bytes written.
		/// If the updated position is greater than the current length of the stream, then
		/// the <see cref="Length"/> property will be updated to be identical to the
		/// position.
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

			ValidateArguments (buffer, offset, count);

			Signer.BlockUpdate (buffer, offset, count);

			length += count;
		}

		/// <summary>
		/// Sets the position within the current stream.
		/// </summary>
		/// <remarks>
		/// Updates the <see cref="Position"/> within the stream.
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
		public override long Seek (long offset, SeekOrigin origin)
		{
			CheckDisposed ();

			throw new NotSupportedException ("The stream does not support seeking.");
		}

		/// <summary>
		/// Clears all buffers for this stream and causes any buffered data to be written
		/// to the underlying device.
		/// </summary>
		/// <remarks>
		/// Since a <see cref="DkimSignatureStream"/> does not actually do anything other than
		/// count bytes, this method is a no-op.
		/// </remarks>
		/// <exception cref="System.ObjectDisposedException">
		/// The stream has been disposed.
		/// </exception>
		public override void Flush ()
		{
			CheckDisposed ();

			// nothing to do...
		}

		/// <summary>
		/// Sets the length of the stream.
		/// </summary>
		/// <remarks>
		/// Sets the <see cref="Length"/> to the specified value and updates
		/// <see cref="Position"/> to the specified value if (and only if)
		/// the current position is greater than the new length value.
		/// </remarks>
		/// <param name="value">The desired length of the stream in bytes.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is out of range.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The stream has been disposed.
		/// </exception>
		public override void SetLength (long value)
		{
			CheckDisposed ();

			throw new NotSupportedException ("The stream does not support setting the length.");
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="DkimSignatureStream"/> and
		/// optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
		/// <c>false</c> to release only the unmanaged resources.</param>
		protected override void Dispose (bool disposing)
		{
			if (disposing && !disposed) {
#if ENABLE_NATIVE_DKIM
				var sss = Signer as SystemSecuritySigner;

				if (sss != null)
					sss.Dispose ();
#endif

				disposed = true;
			}

			base.Dispose (disposing);
		}
	}
}

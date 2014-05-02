//
// System.Security.Cryptography.MD5CryptoServiceProvider.cs
//
// Authors:
//	Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Copyright 2001 by Matthew S. Ford.
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;

namespace MimeKit.Cryptography {
	/// <summary>
	/// The MD5 hash algorithm.
	/// </summary>
	/// <remarks>
	/// This class is only here for for portability reasons and should
	/// not really be considered part of the MimeKit API.
	/// </remarks>
	public sealed class MD5 : IDisposable
	{
		const int BLOCK_SIZE_BYTES = 64;

		static readonly uint[] K = {
			0xd76aa478, 0xe8c7b756, 0x242070db, 0xc1bdceee,
			0xf57c0faf, 0x4787c62a, 0xa8304613, 0xfd469501,
			0x698098d8, 0x8b44f7af, 0xffff5bb1, 0x895cd7be,
			0x6b901122, 0xfd987193, 0xa679438e, 0x49b40821,
			0xf61e2562, 0xc040b340, 0x265e5a51, 0xe9b6c7aa,
			0xd62f105d, 0x02441453, 0xd8a1e681, 0xe7d3fbc8,
			0x21e1cde6, 0xc33707d6, 0xf4d50d87, 0x455a14ed,
			0xa9e3e905, 0xfcefa3f8, 0x676f02d9, 0x8d2a4c8a,
			0xfffa3942, 0x8771f681, 0x6d9d6122, 0xfde5380c,
			0xa4beea44, 0x4bdecfa9, 0xf6bb4b60, 0xbebfbc70,
			0x289b7ec6, 0xeaa127fa, 0xd4ef3085, 0x04881d05,
			0xd9d4d039, 0xe6db99e5, 0x1fa27cf8, 0xc4ac5665,
			0xf4292244, 0x432aff97, 0xab9423a7, 0xfc93a039,
			0x655b59c3, 0x8f0ccc92, 0xffeff47d, 0x85845dd1,
			0x6fa87e4f, 0xfe2ce6e0, 0xa3014314, 0x4e0811a1,
			0xf7537e82, 0xbd3af235, 0x2ad7d2bb, 0xeb86d391
		};

		byte[] hashValue;
		byte[] queuedData;   // Used to store data when passed less than a block worth.
		int queuedCount; // Counts how much data we have stored that still needs processed.
		uint[] _H, buff;
		bool disposed;
		ulong count;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.MD5"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new instance of an MD5 hash algorithm context.
		/// </remarks>
		public MD5 ()
		{
			queuedData = new byte [BLOCK_SIZE_BYTES];
			buff = new uint[16];
			_H = new uint[4];

			Initialize ();
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="MimeKit.Cryptography.MD5"/> is reclaimed by garbage collection.
		/// </summary>
		/// <remarks>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="MimeKit.Cryptography.MD5"/> is reclaimed by garbage collection.
		/// </remarks>
		~MD5 ()
		{
			Dispose (false);
		}

		/// <summary>
		/// Gets the value of the computed hash code.
		/// </summary>
		/// <remarks>
		/// Gets the value of the computed hash code.
		/// </remarks>
		/// <value>The computed hash code.</value>
		/// <exception cref="System.InvalidOperationException">
		/// No hash value has been computed.
		/// </exception>
		public byte[] Hash {
			get {
				if (hashValue == null)
					throw new InvalidOperationException ("No hash value computed.");

				return hashValue;
			}
		}

		void HashCore (byte[] block, int offset, int size)
		{
			int i;

			if (queuedCount != 0) {
				if (size < (BLOCK_SIZE_BYTES - queuedCount)) {
					Buffer.BlockCopy (block, offset, queuedData, queuedCount, size);
					queuedCount += size;
					return;
				}

				i = (BLOCK_SIZE_BYTES - queuedCount);
				Buffer.BlockCopy (block, offset, queuedData, queuedCount, i);
				ProcessBlock (queuedData, 0);
				queuedCount = 0;
				offset += i;
				size -= i;
			}

			for (i = 0; i < size - size % BLOCK_SIZE_BYTES; i += BLOCK_SIZE_BYTES)
				ProcessBlock (block, offset + i);

			if (size % BLOCK_SIZE_BYTES != 0) {
				Buffer.BlockCopy (block, size - size % BLOCK_SIZE_BYTES + offset, queuedData, 0, size % BLOCK_SIZE_BYTES);
				queuedCount = size % BLOCK_SIZE_BYTES;
			}
		}

		byte[] HashFinal ()
		{
			byte[] hash = new byte[16];
			int i, j;

			ProcessFinalBlock (queuedData, 0, queuedCount);

			for (i = 0; i < 4; i++) {
				for (j = 0; j < 4; j++) {
					hash[i * 4 + j] = (byte)(_H[i] >> j * 8);
				}
			}

			return hash;
		}

		/// <summary>
		/// Initializes (or re-initializes) the MD5 hash algorithm context.
		/// </summary>
		/// <remarks>
		/// Initializes (or re-initializes) the MD5 hash algorithm context.
		/// </remarks>
		public void Initialize ()
		{
			queuedCount = 0;
			count = 0;

			_H[0] = 0x67452301;
			_H[1] = 0xefcdab89;
			_H[2] = 0x98badcfe;
			_H[3] = 0x10325476;
		}

		void ProcessBlock (byte[] block, int offset)
		{
			uint a, b, c, d;
			int i;

			count += BLOCK_SIZE_BYTES;

			for (i = 0; i < 16; i++) {
				buff[i] = (uint)(block[offset + 4 * i])
					| (((uint)(block[offset + 4 * i + 1])) <<  8)
					| (((uint)(block[offset + 4 * i + 2])) << 16)
					| (((uint)(block[offset + 4 * i + 3])) << 24);
			}

			a = _H[0];
			b = _H[1];
			c = _H[2];
			d = _H[3];

			// This function was unrolled because it seems to be doubling our performance with current compiler/VM.
			// Possibly roll up if this changes.

			// ---- Round 1 --------

			a += (((c ^ d) & b) ^ d) + (uint) K[0] + buff[0];
			a = (a << 7) | (a >> 25);
			a += b;

			d += (((b ^ c) & a) ^ c) + (uint) K[1] + buff[1];
			d = (d << 12) | (d >> 20);
			d += a;

			c += (((a ^ b) & d) ^ b) + (uint) K[2] + buff[2];
			c = (c << 17) | (c >> 15);
			c += d;

			b += (((d ^ a) & c) ^ a) + (uint) K[3] + buff[3];
			b = (b << 22) | (b >> 10);
			b += c;

			a += (((c ^ d) & b) ^ d) + (uint) K[4] + buff[4];
			a = (a << 7) | (a >> 25);
			a += b;

			d += (((b ^ c) & a) ^ c) + (uint) K[5] + buff[5];
			d = (d << 12) | (d >> 20);
			d += a;

			c += (((a ^ b) & d) ^ b) + (uint) K[6] + buff[6];
			c = (c << 17) | (c >> 15);
			c += d;

			b += (((d ^ a) & c) ^ a) + (uint) K[7] + buff[7];
			b = (b << 22) | (b >> 10);
			b += c;

			a += (((c ^ d) & b) ^ d) + (uint) K[8] + buff[8];
			a = (a << 7) | (a >> 25);
			a += b;

			d += (((b ^ c) & a) ^ c) + (uint) K[9] + buff[9];
			d = (d << 12) | (d >> 20);
			d += a;

			c += (((a ^ b) & d) ^ b) + (uint) K[10] + buff[10];
			c = (c << 17) | (c >> 15);
			c += d;

			b += (((d ^ a) & c) ^ a) + (uint) K[11] + buff[11];
			b = (b << 22) | (b >> 10);
			b += c;

			a += (((c ^ d) & b) ^ d) + (uint) K[12] + buff[12];
			a = (a << 7) | (a >> 25);
			a += b;

			d += (((b ^ c) & a) ^ c) + (uint) K[13] + buff[13];
			d = (d << 12) | (d >> 20);
			d += a;

			c += (((a ^ b) & d) ^ b) + (uint) K[14] + buff[14];
			c = (c << 17) | (c >> 15);
			c += d;

			b += (((d ^ a) & c) ^ a) + (uint) K[15] + buff[15];
			b = (b << 22) | (b >> 10);
			b += c;


			// ---- Round 2 --------

			a += (((b ^ c) & d) ^ c) + (uint) K[16] + buff[1];
			a = (a << 5) | (a >> 27);
			a += b;

			d += (((a ^ b) & c) ^ b) + (uint) K[17] + buff[6];
			d = (d << 9) | (d >> 23);
			d += a;

			c += (((d ^ a) & b) ^ a) + (uint) K[18] + buff[11];
			c = (c << 14) | (c >> 18);
			c += d;

			b += (((c ^ d) & a) ^ d) + (uint) K[19] + buff[0];
			b = (b << 20) | (b >> 12);
			b += c;

			a += (((b ^ c) & d) ^ c) + (uint) K[20] + buff[5];
			a = (a << 5) | (a >> 27);
			a += b;

			d += (((a ^ b) & c) ^ b) + (uint) K[21] + buff[10];
			d = (d << 9) | (d >> 23);
			d += a;

			c += (((d ^ a) & b) ^ a) + (uint) K[22] + buff[15];
			c = (c << 14) | (c >> 18);
			c += d;

			b += (((c ^ d) & a) ^ d) + (uint) K[23] + buff[4];
			b = (b << 20) | (b >> 12);
			b += c;

			a += (((b ^ c) & d) ^ c) + (uint) K[24] + buff[9];
			a = (a << 5) | (a >> 27);
			a += b;

			d += (((a ^ b) & c) ^ b) + (uint) K[25] + buff[14];
			d = (d << 9) | (d >> 23);
			d += a;

			c += (((d ^ a) & b) ^ a) + (uint) K[26] + buff[3];
			c = (c << 14) | (c >> 18);
			c += d;

			b += (((c ^ d) & a) ^ d) + (uint) K[27] + buff[8];
			b = (b << 20) | (b >> 12);
			b += c;

			a += (((b ^ c) & d) ^ c) + (uint) K[28] + buff[13];
			a = (a << 5) | (a >> 27);
			a += b;

			d += (((a ^ b) & c) ^ b) + (uint) K[29] + buff[2];
			d = (d << 9) | (d >> 23);
			d += a;

			c += (((d ^ a) & b) ^ a) + (uint) K[30] + buff[7];
			c = (c << 14) | (c >> 18);
			c += d;

			b += (((c ^ d) & a) ^ d) + (uint) K[31] + buff[12];
			b = (b << 20) | (b >> 12);
			b += c;


			// ---- Round 3 --------

			a += (b ^ c ^ d) + (uint) K[32] + buff[5];
			a = (a << 4) | (a >> 28);
			a += b;

			d += (a ^ b ^ c) + (uint) K[33] + buff[8];
			d = (d << 11) | (d >> 21);
			d += a;

			c += (d ^ a ^ b) + (uint) K[34] + buff[11];
			c = (c << 16) | (c >> 16);
			c += d;

			b += (c ^ d ^ a) + (uint) K[35] + buff[14];
			b = (b << 23) | (b >> 9);
			b += c;

			a += (b ^ c ^ d) + (uint) K[36] + buff[1];
			a = (a << 4) | (a >> 28);
			a += b;

			d += (a ^ b ^ c) + (uint) K[37] + buff[4];
			d = (d << 11) | (d >> 21);
			d += a;

			c += (d ^ a ^ b) + (uint) K[38] + buff[7];
			c = (c << 16) | (c >> 16);
			c += d;

			b += (c ^ d ^ a) + (uint) K[39] + buff[10];
			b = (b << 23) | (b >> 9);
			b += c;

			a += (b ^ c ^ d) + (uint) K[40] + buff[13];
			a = (a << 4) | (a >> 28);
			a += b;

			d += (a ^ b ^ c) + (uint) K[41] + buff[0];
			d = (d << 11) | (d >> 21);
			d += a;

			c += (d ^ a ^ b) + (uint) K[42] + buff[3];
			c = (c << 16) | (c >> 16);
			c += d;

			b += (c ^ d ^ a) + (uint) K[43] + buff[6];
			b = (b << 23) | (b >> 9);
			b += c;

			a += (b ^ c ^ d) + (uint) K[44] + buff[9];
			a = (a << 4) | (a >> 28);
			a += b;

			d += (a ^ b ^ c) + (uint) K[45] + buff[12];
			d = (d << 11) | (d >> 21);
			d += a;

			c += (d ^ a ^ b) + (uint) K[46] + buff[15];
			c = (c << 16) | (c >> 16);
			c += d;

			b += (c ^ d ^ a) + (uint) K[47] + buff[2];
			b = (b << 23) | (b >> 9);
			b += c;


			// ---- Round 4 --------

			a += (((~d) | b) ^ c) + (uint) K[48] + buff[0];
			a = (a << 6) | (a >> 26);
			a += b;

			d += (((~c) | a) ^ b) + (uint) K[49] + buff[7];
			d = (d << 10) | (d >> 22);
			d += a;

			c += (((~b) | d) ^ a) + (uint) K[50] + buff[14];
			c = (c << 15) | (c >> 17);
			c += d;

			b += (((~a) | c) ^ d) + (uint) K[51] + buff[5];
			b = (b << 21) | (b >> 11);
			b += c;

			a += (((~d) | b) ^ c) + (uint) K[52] + buff[12];
			a = (a << 6) | (a >> 26);
			a += b;

			d += (((~c) | a) ^ b) + (uint) K[53] + buff[3];
			d = (d << 10) | (d >> 22);
			d += a;

			c += (((~b) | d) ^ a) + (uint) K[54] + buff[10];
			c = (c << 15) | (c >> 17);
			c += d;

			b += (((~a) | c) ^ d) + (uint) K[55] + buff[1];
			b = (b << 21) | (b >> 11);
			b += c;

			a += (((~d) | b) ^ c) + (uint) K[56] + buff[8];
			a = (a << 6) | (a >> 26);
			a += b;

			d += (((~c) | a) ^ b) + (uint) K[57] + buff[15];
			d = (d << 10) | (d >> 22);
			d += a;

			c += (((~b) | d) ^ a) + (uint) K[58] + buff[6];
			c = (c << 15) | (c >> 17);
			c += d;

			b += (((~a) | c) ^ d) + (uint) K[59] + buff[13];
			b = (b << 21) | (b >> 11);
			b += c;

			a += (((~d) | b) ^ c) + (uint) K[60] + buff[4];
			a = (a << 6) | (a >> 26);
			a += b;

			d += (((~c) | a) ^ b) + (uint) K[61] + buff[11];
			d = (d << 10) | (d >> 22);
			d += a;

			c += (((~b) | d) ^ a) + (uint) K[62] + buff[2];
			c = (c << 15) | (c >> 17);
			c += d;

			b += (((~a) | c) ^ d) + (uint) K[63] + buff[9];
			b = (b << 21) | (b >> 11);
			b += c;

			_H[0] += a;
			_H[1] += b;
			_H[2] += c;
			_H[3] += d;
		}

		void ProcessFinalBlock (byte[] inbuf, int startIndex, int length)
		{
			ulong total = count + (ulong) length;
			int padding = (int)(56 - total % BLOCK_SIZE_BYTES);

			if (padding < 1)
				padding += BLOCK_SIZE_BYTES;

			var block = new byte [length + padding + 8];

			for (int i = 0; i < length; i++)
				block[i] = inbuf[startIndex + i];

			block[length] = 0x80;
			for (int i = length + 1; i < length + padding; i++)
				block[i] = 0x00;

			// I deal in bytes. The algorithm deals in bits.
			ulong size = total << 3;
			AddLength (size, block, length+padding);
			ProcessBlock (block, 0);

			if (length + padding + 8 == 128)
				ProcessBlock (block, 64);
		}

		void AddLength (ulong length, byte[] buffer, int index)
		{
			buffer[index++] = (byte) length;
			buffer[index++] = (byte) (length >>  8);
			buffer[index++] = (byte) (length >> 16);
			buffer[index++] = (byte) (length >> 24);
			buffer[index++] = (byte) (length >> 32);
			buffer[index++] = (byte) (length >> 40);
			buffer[index++] = (byte) (length >> 48);
			buffer[index]   = (byte) (length >> 56);
		}

		/// <summary>
		/// Computes the MD5 hash code for the specified subrange of the buffer.
		/// </summary>
		/// <remarks>
		/// Computes the MD5 hash code for the specified subrange of the buffer.
		/// </remarks>
		/// <returns>The computed hash code.</returns>
		/// <param name="buffer">The buffer.</param>
		/// <param name="offset">The starting offset.</param>
		/// <param name="count">The number of bytes to hash.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="offset"/> and <paramref name="count"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The MD5 context has been disposed.
		/// </exception>
		public byte[] ComputeHash (byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (offset < 0 || offset > buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");

			if (count < 0 || offset > buffer.Length - count)
				throw new ArgumentOutOfRangeException ("count");

			if (disposed)
				throw new ObjectDisposedException ("HashAlgorithm");

			HashCore (buffer, offset, count);
			hashValue = HashFinal ();
			Initialize ();

			return hashValue;
		}

		/// <summary>
		/// Computes the MD5 hash code for the buffer.
		/// </summary>
		/// <remarks>
		/// Computes the MD5 hash code for the buffer.
		/// </remarks>
		/// <returns>The computed hash code.</returns>
		/// <param name="buffer">The buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The MD5 context has been disposed.
		/// </exception>
		public byte[] ComputeHash (byte[] buffer)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			return ComputeHash (buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Computes the MD5 hash code for the stream.
		/// </summary>
		/// <remarks>
		/// Computes the MD5 hash code for the stream.
		/// </remarks>
		/// <returns>The computed hash code.</returns>
		/// <param name="inputStream">The input stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="inputStream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The MD5 context has been disposed.
		/// </exception>
		public byte[] ComputeHash (Stream inputStream)
		{
			if (inputStream == null)
				throw new ArgumentNullException ("inputStream");

			// don't read stream unless object is ready to use
			if (disposed)
				throw new ObjectDisposedException ("HashAlgorithm");

			var buffer = new byte [4096];
			int nread;

			do {
				if ((nread = inputStream.Read (buffer, 0, buffer.Length)) > 0)
					HashCore (buffer, 0, nread);
			} while (nread > 0);

			hashValue = HashFinal ();
			Initialize ();

			return hashValue;
		}

		/// <summary>
		/// Computes a partial MD5 hash value for the specified region of the
		/// input buffer and copies the input into the output buffer.
		/// </summary>
		/// <remarks>
		/// <para>Computes a partial MD5 hash value for the specified region of the
		/// input buffer and copies the input into the output buffer.</para>
		/// <para>Use <see cref="TransformFinalBlock"/> to complete the computation
		/// of the MD5 hash code.</para>
		/// </remarks>
		/// <returns>The number of bytes copied into the output buffer.</returns>
		/// <param name="inputBuffer">The input buffer.</param>
		/// <param name="inputOffset">The input buffer offset.</param>
		/// <param name="inputCount">The input count.</param>
		/// <param name="outputBuffer">The output buffer.</param>
		/// <param name="outputOffset">The output buffer offset.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="inputBuffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="inputOffset"/> and <paramref name="inputCount"/> do not specify
		/// a valid range in the <paramref name="inputBuffer"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="outputOffset"/> is outside the bounds of the
		/// <paramref name="outputBuffer"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="outputBuffer"/> is not large enough to hold the range of input
		/// starting at <paramref name="outputOffset"/>.</para>
		/// </exception>
		public int TransformBlock (byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
		{
			if (inputBuffer == null)
				throw new ArgumentNullException ("inputBuffer");

			if (inputOffset < 0 || inputOffset > inputBuffer.Length)
				throw new ArgumentOutOfRangeException ("inputOffset");

			if (inputCount < 0 || inputOffset > inputBuffer.Length - inputCount)
				throw new ArgumentOutOfRangeException ("inputCount");

			if (outputBuffer != null) {
				if (outputOffset < 0 || outputOffset > outputBuffer.Length - inputCount)
					throw new ArgumentOutOfRangeException ("outputOffset");
			}

			HashCore (inputBuffer, inputOffset, inputCount);

			if (outputBuffer != null)
				Buffer.BlockCopy (inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);

			return inputCount;
		}

		/// <summary>
		/// Completes the MD5 hash compuation given the final block of input.
		/// </summary>
		/// <remarks>
		/// Completes the MD5 hash compuation given the final block of input.
		/// </remarks>
		/// <returns>A new buffer containing the specified range of input.</returns>
		/// <param name="inputBuffer">The input buffer.</param>
		/// <param name="inputOffset">The input buffer offset.</param>
		/// <param name="inputCount">The input count.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="inputBuffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="inputOffset"/> and <paramref name="inputCount"/> do not specify
		/// a valid range in the <paramref name="inputBuffer"/>.
		/// </exception>
		public byte[] TransformFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount)
		{
			if (inputBuffer == null)
				throw new ArgumentNullException ("inputBuffer");

			if (inputOffset < 0 || inputOffset > inputBuffer.Length)
				throw new ArgumentOutOfRangeException ("inputOffset");

			if (inputCount < 0 || inputOffset > inputBuffer.Length - inputCount)
				throw new ArgumentOutOfRangeException ("inputCount");

			var outputBuffer = new byte [inputCount];

			// note: other exceptions are handled by Buffer.BlockCopy
			Buffer.BlockCopy (inputBuffer, inputOffset, outputBuffer, 0, inputCount);

			HashCore (inputBuffer, inputOffset, inputCount);
			hashValue = HashFinal ();
			Initialize ();

			return outputBuffer;
		}

		void Dispose (bool disposing)
		{
			if (queuedData != null) {
				Array.Clear (queuedData, 0, queuedData.Length);
				queuedData = null;
			}

			if (_H != null) {
				Array.Clear (_H, 0, _H.Length);
				_H = null;
			}

			if (buff != null) {
				Array.Clear (buff, 0, buff.Length);
				buff = null;
			}
		}

		/// <summary>
		/// Releases all resource used by the <see cref="MimeKit.Cryptography.MD5"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose()"/> when you are finished using the <see cref="MimeKit.Cryptography.MD5"/>. The
		/// <see cref="Dispose()"/> method leaves the <see cref="MimeKit.Cryptography.MD5"/> in an unusable state. After calling
		/// <see cref="Dispose()"/>, you must release all references to the <see cref="MimeKit.Cryptography.MD5"/> so the
		/// garbage collector can reclaim the memory that the <see cref="MimeKit.Cryptography.MD5"/> was occupying.</remarks>
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
			disposed = true;
		}
	}
}

//
// TnefChecksum.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
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

using System.Numerics;

namespace MimeKit.Tnef {
	static class TnefChecksum
	{
		/// <summary>
		/// Add the specified bytes to a running TNEF attribute checksum.
		/// </summary>
		/// <remarks>
		/// The checksum is the sum of the attribute value bytes modulo 65536 ([MS-OXTNEF] 2.1.3.3).
		/// </remarks>
		/// <returns>The updated checksum.</returns>
		/// <param name="checksum">The current checksum.</param>
		/// <param name="buffer">The buffer.</param>
		/// <param name="offset">The offset into the buffer.</param>
		/// <param name="count">The number of bytes to add.</param>
		public static ushort Update (ushort checksum, byte[] buffer, int offset, int count)
		{
			// Note: Addition modulo 65536 is associative, so the sum can be accumulated at full
			// width and truncated once at the end instead of after every byte.
			int end = offset + count;
			long sum = checksum;
			int i = offset;

			if (Vector.IsHardwareAccelerated && count >= Vector<byte>.Count) {
				// Note: each 32-bit lane accumulates at most 4 * 255 per iteration, so the lanes are
				// flushed into the 64-bit sum every BlockIterations iterations to rule out overflow no
				// matter how large the buffer is.
				const int BlockIterations = 1 << 20;
				int limit = end - Vector<byte>.Count;

				while (i <= limit) {
					var vsum = Vector<uint>.Zero;
					int n = 0;

					while (i <= limit && n < BlockIterations) {
						Vector.Widen (new Vector<byte> (buffer, i), out Vector<ushort> low, out Vector<ushort> high);
						Vector.Widen (low, out Vector<uint> a, out Vector<uint> b);
						Vector.Widen (high, out Vector<uint> c, out Vector<uint> d);

						vsum += a + b + c + d;
						i += Vector<byte>.Count;
						n++;
					}

					for (int lane = 0; lane < Vector<uint>.Count; lane++)
						sum += vsum[lane];
				}
			}

			while (i < end)
				sum += buffer[i++];

			return (ushort) sum;
		}
	}
}

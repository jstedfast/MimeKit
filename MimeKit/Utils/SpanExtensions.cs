//
// SpanExtensions.cs
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MimeKit.Utils {
	static class SpanExtensions
	{
#if NET8_0_OR_GREATER
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static int EndOfLine (this Span<byte> span)
		{
			// Note: Span<byte>.IndexOf(byte) is insanely fast in .NET >= 8.0, so use it.
			return span.IndexOf ((byte) '\n');
		}
#else
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public unsafe static int EndOfLine (this Span<byte> span)
		{
			fixed (byte* inbuf = &MemoryMarshal.GetReference (span)) {
				byte* inptr = inbuf;

				// scan for a linefeed character until we are 4-byte aligned.
				switch (((long) inptr) & 0x03) {
				case 1:
					if (*inptr == (byte) '\n')
						return (int) (inptr - inbuf);
					inptr++;
					goto case 2;
				case 2:
					if (*inptr == (byte) '\n')
						return (int) (inptr - inbuf);
					inptr++;
					goto case 3;
				case 3:
					if (*inptr == (byte) '\n')
						return (int) (inptr - inbuf);
					inptr++;
					break;
				}

				// -funroll-loops, yippee ki-yay.
				do {
					uint mask = *((uint*) inptr) ^ 0x0A0A0A0A;
					mask = ((mask - 0x01010101) & (~mask & 0x80808080));

					if (mask != 0)
						break;

					inptr += 4;
				} while (true);

				while (*inptr != (byte) '\n')
					inptr++;

				return (int) (inptr - inbuf);
			}
		}
#endif
	}
}

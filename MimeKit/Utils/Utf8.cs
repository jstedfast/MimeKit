//
// Utf8.cs
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

#if NETFRAMEWORK || NETSTANDARD || !NET8_0_OR_GREATER

using System;
using System.Runtime.CompilerServices;

using Microsoft.Win32;

namespace System.Text.Unicode
{
	static class Utf8
	{
		/// <summary>
		/// Validate that the value is well-formed UTF-8.
		/// </summary>
		/// <remarks>
		/// Validates that the value is well-formed UTF-8.
		/// </remarks>
		/// <param name="value">The byte string.</param>
		/// <returns><see langword="true"/> if the value is well-formed UTF-8; otherwise, <see langword="false"/>.</returns>
		public static bool IsValid (ReadOnlySpan<byte> value)
		{
			int index = 0;

			if (value.IsEmpty)
				return true;

			while (index < value.Length) {
				uint u = ReadUnichar (value, ref index);

				if (u == 0xfffe)
					return false;
			}

			return true;
		}

		static uint ReadUnichar (ReadOnlySpan<byte> value, ref int index)
		{
			uint u = 0;
			byte c, r;

			if (index >= value.Length)
				return 0;

			r = value[index++];

			if (r < 0x80) {
				// simple ascii character
				u = r;
			} else if (r < 0xfe) {
				// mask for utf-8 length bits
				uint mask = 0x7f80;

				u = r;

				do {
					if (index >= value.Length)
						return 0xfffe;

					c = value[index++];
					if ((c & 0xc0) != 0x80) {
						// invalid utf-8 sequence
						return 0xfffe;
					}
					
					u = (u << 6) | ((uint) c & 0x3f);
					r <<= 1;
					mask <<= 5;
				} while ((r & 0x40) != 0);

				u &= ~mask;
			} else {
				// invalid utf-8 start character
				return 0xfffe;
			}

			return u;
		}
	}
}

#endif

﻿//
// Dos2UnixFilter.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 Xamarin Inc. (www.xamarin.com)
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

namespace MimeKit.IO.Filters {
	/// <summary>
	/// A filter that will convert from Windows/DOS line endings to Unix line endings.
	/// </summary>
	/// <remarks>
	/// Converts from Windows/DOS line endings to Unix line endings.
	/// </remarks>
	public class Dos2UnixFilter : MimeFilterBase
	{
		readonly bool ensureNewLine;
		byte pc;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.IO.Filters.Dos2UnixFilter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="Dos2UnixFilter"/>.
		/// </remarks>
		/// <param name="ensureNewLine">Ensure that the stream ends with a new line.</param>
		public Dos2UnixFilter (bool ensureNewLine = false)
		{
			this.ensureNewLine = ensureNewLine;
		}

		unsafe int Filter (byte* inbuf, int length, byte* outbuf, bool flush)
		{
			byte* inend = inbuf + length;
			byte* outptr = outbuf;
			byte* inptr = inbuf;

			while (inptr < inend) {
				if (*inptr == (byte) '\n') {
					*outptr++ = *inptr;
				} else {
					if (pc == (byte) '\r')
						*outptr++ = pc;

					if (*inptr != (byte) '\r')
						*outptr++ = *inptr;
				}

				pc = *inptr++;
			}

			if (flush && ensureNewLine && pc != (byte) '\n')
				*outptr++ = (byte) '\n';

			return (int) (outptr - outbuf);
		}

		/// <summary>
		/// Filter the specified input.
		/// </summary>
		/// <remarks>
		/// Filters the specified input buffer starting at the given index,
		/// spanning across the specified number of bytes.
		/// </remarks>
		/// <returns>The filtered output.</returns>
		/// <param name="input">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The length of the input buffer, starting at <paramref name="startIndex"/>.</param>
		/// <param name="outputIndex">The output index.</param>
		/// <param name="outputLength">The output length.</param>
		/// <param name="flush">If set to <c>true</c>, all internally buffered data should be flushed to the output buffer.</param>
		protected override byte[] Filter (byte[] input, int startIndex, int length, out int outputIndex, out int outputLength, bool flush)
		{
			if (pc == (byte) '\r')
				EnsureOutputSize (length + (flush && ensureNewLine ? 2 : 1), false);
			else
				EnsureOutputSize (length + (flush && ensureNewLine ? 1 : 0), false);

			outputIndex = 0;

			unsafe {
				fixed (byte* inptr = input, outptr = OutputBuffer) {
					outputLength = Filter (inptr + startIndex, length, outptr, flush);
				}
			}

			return OutputBuffer;
		}

		/// <summary>
		/// Resets the filter.
		/// </summary>
		/// <remarks>
		/// Resets the filter.
		/// </remarks>
		public override void Reset ()
		{
			pc = 0;
			base.Reset ();
		}
	}
}

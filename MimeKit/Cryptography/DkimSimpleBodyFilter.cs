﻿//
// DkimSimpleBodyFilter.cs
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// A filter for the DKIM simple body canonicalization.
	/// </summary>
	/// <remarks>
	/// A filter for the DKIM simple body canonicalization.
	/// </remarks>
	class DkimSimpleBodyFilter : DkimBodyFilter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.DkimSimpleBodyFilter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="DkimSimpleBodyFilter"/>.
		/// </remarks>
		public DkimSimpleBodyFilter ()
		{
			LastWasNewLine = false;
			IsEmptyLine = true;
			EmptyLines = 0;
		}

		unsafe int Filter (byte* inbuf, int length, byte* outbuf)
		{
			byte* inend = inbuf + length;
			byte* outptr = outbuf;
			byte* inptr = inbuf;
			int count = 0;

			while (inptr < inend) {
				if (*inptr == (byte) '\r') {
					if (!IsEmptyLine) {
						*outptr++ = *inptr;
						count++;
					}
				} else if (*inptr == (byte) '\n') {
					if (!IsEmptyLine) {
						*outptr++ = *inptr;
						LastWasNewLine = true;
						IsEmptyLine = true;
						EmptyLines = 0;
						count++;
					} else {
						EmptyLines++;
					}
				} else {
					if (EmptyLines > 0) {
						// unwind our collection of empty lines
						while (EmptyLines > 0) {
							*outptr++ = (byte) '\r';
							*outptr++ = (byte) '\n';
							EmptyLines--;
							count += 2;
						}
					}

					LastWasNewLine = false;
					IsEmptyLine = false;

					*outptr++ = *inptr;
					count++;
				}

				inptr++;
			}

			return count;
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
			EnsureOutputSize (length + EmptyLines * 2 + 1, false);

			unsafe {
				fixed (byte* inptr = input, outptr = OutputBuffer) {
					outputLength = Filter (inptr + startIndex, length, outptr);
				}
			}

			outputIndex = 0;

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
			LastWasNewLine = false;
			IsEmptyLine = true;
			EmptyLines = 0;

			base.Reset ();
		}
	}
}

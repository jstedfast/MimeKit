﻿//
// DkimRelaxedBodyFilter.cs
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

using MimeKit.Utils;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A filter for the DKIM relaxed body canonicalization.
	/// </summary>
	/// <remarks>
	/// A filter for the DKIM relaxed body canonicalization.
	/// </remarks>
	class DkimRelaxedBodyFilter : DkimBodyFilter
	{
		bool lwsp, cr;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.DkimRelaxedBodyFilter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="DkimRelaxedBodyFilter"/>.
		/// </remarks>
		public DkimRelaxedBodyFilter ()
		{
			LastWasNewLine = true;
			IsEmptyLine = true;
		}

		unsafe int Filter (byte* inbuf, int length, byte* outbuf)
		{
			byte* inend = inbuf + length;
			byte* outptr = outbuf;
			byte* inptr = inbuf;
			int count = 0;

			while (inptr < inend) {
				if (*inptr == (byte) '\n') {
					if (IsEmptyLine) {
						EmptyLines++;
					} else {
						if (cr) {
							*outptr++ = (byte) '\r';
							count++;
						}

						*outptr++ = (byte) '\n';
						LastWasNewLine = true;
						IsEmptyLine = true;
						count++;
					}

					lwsp = false;
					cr = false;
				} else {
					if (cr) {
						*outptr++ = (byte) '\r';
						cr = false;
						count++;
					}

					if (*inptr == (byte) '\r') {
						lwsp = false;
						cr = true;
					} else if ((*inptr).IsBlank ()) {
						lwsp = true;
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

						if (lwsp) {
							// collapse lwsp to a single space
							*outptr++ = (byte) ' ';
							lwsp = false;
							count++;
						}

						LastWasNewLine = false;
						IsEmptyLine = false;

						*outptr++ = *inptr;
						count++;
					}
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
			EnsureOutputSize (length + (lwsp ? 1 : 0) + (EmptyLines * 2) + (cr ? 1 : 0) + 1, false);

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
			LastWasNewLine = true;
			IsEmptyLine = true;
			EmptyLines = 0;
			lwsp = false;
			cr = false;

			base.Reset ();
		}
	}
}

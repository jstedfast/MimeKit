//
// DkimSimpleBodyFilter.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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

using MimeKit.IO.Filters;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A filter for the DKIM simple body canonicalization.
	/// </summary>
	/// <remarks>
	/// A filter for the DKIM simple body canonicalization.
	/// </remarks>
	class DkimSimpleBodyFilter : MimeFilterBase
	{
		bool lastWasNewLine, isEmptyLine;
		int emptyLines;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.DkimSimpleBodyFilter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="DkimSimpleBodyFilter"/>.
		/// </remarks>
		public DkimSimpleBodyFilter ()
		{
			lastWasNewLine = false;
			isEmptyLine = true;
			emptyLines = 0;
		}

		unsafe int Filter (byte* inbuf, int length, byte* outbuf, bool flush)
		{
			byte* inend = inbuf + length;
			byte* outptr = outbuf;
			byte* inptr = inbuf;
			int count = 0;

			while (inptr < inend) {
				if (*inptr == (byte) '\r') {
					if (!isEmptyLine) {
						*outptr++ = *inptr;
						count++;
					}
				} else if (*inptr == (byte) '\n') {
					if (!isEmptyLine) {
						*outptr++ = *inptr;
						lastWasNewLine = true;
						isEmptyLine = true;
						emptyLines = 0;
						count++;
					} else {
						emptyLines++;
					}
				} else {
					if (emptyLines > 0) {
						// unwind our collection of empty lines
						while (emptyLines > 0) {
							*outptr++ = (byte) '\r';
							*outptr++ = (byte) '\n';
							emptyLines--;
							count += 2;
						}
					}

					lastWasNewLine = false;
					isEmptyLine = false;

					*outptr++ = *inptr;
					count++;
				}

				inptr++;
			}

			if (flush && !lastWasNewLine) {
				*outptr++ = (byte) '\r';
				*outptr++ = (byte) '\n';
				lastWasNewLine = true;
				isEmptyLine = true;
				emptyLines = 0;
				count += 2;
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
			EnsureOutputSize (length + emptyLines * 2 + (flush ? 2 : 0), false);

			unsafe {
				fixed (byte* inptr = input, outptr = OutputBuffer) {
					outputLength = Filter (inptr + startIndex, length, outptr, flush);
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
			lastWasNewLine = false;
			isEmptyLine = true;
			emptyLines = 0;

			base.Reset ();
		}
	}
}

//
// TrailingWhitespaceFilter.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc.
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

namespace MimeKit.IO.Filters {
	/// <summary>
	/// A filter for stripping trailing whitespace from lines in a textual stream.
	/// </summary>
	/// <remarks>
	/// Strips trailing whitespace from lines in a textual stream.
	/// </remarks>
	public class TrailingWhitespaceFilter : MimeFilterBase
	{
		readonly PackedByteArray lwsp = new PackedByteArray ();

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.IO.Filters.TrailingWhitespaceFilter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="TrailingWhitespaceFilter"/>.
		/// </remarks>
		public TrailingWhitespaceFilter ()
		{
		}

		unsafe int Filter (byte* inbuf, int length, byte* outbuf)
		{
			byte* inend = inbuf + length;
			byte* outptr = outbuf;
			byte* inptr = inbuf;
			int count = 0;

			while (inptr < inend) {
				if ((*inptr).IsBlank ()) {
					lwsp.Add (*inptr);
				} else if (*inptr == (byte) '\r') {
					*outptr++ = *inptr;
					lwsp.Clear ();
					count++;
				} else if (*inptr == (byte) '\n') {
					*outptr++ = *inptr;
					lwsp.Clear ();
					count++;
				} else {
					if (lwsp.Count > 0) {
						lwsp.CopyTo (OutputBuffer, count);
						outptr += lwsp.Count;
						count += lwsp.Count;
						lwsp.Clear ();
					}

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
			if (length == 0) {
				if (flush)
					lwsp.Clear ();

				outputIndex = startIndex;
				outputLength = length;

				return input;
			}

			EnsureOutputSize (length + lwsp.Count, false);

			unsafe {
				fixed (byte* inptr = input, outptr = OutputBuffer) {
					outputLength = Filter (inptr + startIndex, length, outptr);
				}
			}

			if (flush)
				lwsp.Clear ();

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
			lwsp.Clear ();
			base.Reset ();
		}
	}
}

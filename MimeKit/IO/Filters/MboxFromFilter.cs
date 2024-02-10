//
// MboxFromFilter.cs
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
using System.Collections.Generic;

namespace MimeKit.IO.Filters {
	/// <summary>
	/// A filter that munges lines beginning with "From " by stuffing a '&gt;' into the beginning of the line.
	/// </summary>
	/// <remarks>
	/// <para>Munging Mbox-style "From "-lines is a workaround to prevent Mbox parsers from misinterpreting a
	/// line beginning with "From " as an mbox marker delineating messages. This munging is non-reversable but
	/// is necessary to properly format a message for saving to an Mbox file.</para>
	/// </remarks>
	public class MboxFromFilter : MimeFilterBase
	{
		static ReadOnlySpan<byte> From => "From "u8;
		bool midline;

		/// <summary>
		/// Initialize a new instance of the <see cref="MboxFromFilter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MboxFromFilter"/>.
		/// </remarks>
		public MboxFromFilter ()
		{
		}

		static bool StartsWithFrom (byte[] input, int startIndex, int endIndex)
		{
			for (int i = 0, index = startIndex; i < From.Length && index < endIndex; i++, index++) {
				if (input[index] != From[i])
					return false;
			}

			return true;
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
			var fromOffsets = new List<int> ();
			int endIndex = startIndex + length;
			int index = startIndex;
			int left;

			while (index < endIndex) {
				byte c = 0;

				if (midline) {
					while (index < endIndex) {
						c = input[index++];
						if (c == (byte) '\n')
							break;
					}
				}

				if (c == (byte) '\n' || !midline) {
					if ((left = endIndex - index) > 0) {
						midline = true;

						if (left < 5) {
							if (StartsWithFrom (input, index, endIndex)) {
								SaveRemainingInput (input, index, left);
								endIndex = index;
								midline = false;
								break;
							}
						} else {
							if (StartsWithFrom (input, index, endIndex)) {
								fromOffsets.Add (index);
								index += 5;
							}
						}
					} else {
						midline = false;
					}
				}
			}

			if (fromOffsets.Count > 0) {
				int need = (endIndex - startIndex) + fromOffsets.Count;

				EnsureOutputSize (need, false);
				outputLength = 0;
				outputIndex = 0;

				index = startIndex;
				foreach (var offset in fromOffsets) {
					if (index < offset) {
						Buffer.BlockCopy (input, index, OutputBuffer, outputLength, offset - index);
						outputLength += offset - index;
						index = offset;
					}

					// munge the beginning of the "From "-line.
					OutputBuffer[outputLength++] = (byte) '>';
				}

				Buffer.BlockCopy (input, index, OutputBuffer, outputLength, endIndex - index);
				outputLength += endIndex - index;

				return OutputBuffer;
			}

			outputLength = endIndex - startIndex;
			outputIndex = 0;
			return input;
		}

		/// <summary>
		/// Reset the filter.
		/// </summary>
		/// <remarks>
		/// Resets the filter.
		/// </remarks>
		public override void Reset ()
		{
			midline = false;
			base.Reset ();
		}
	}
}

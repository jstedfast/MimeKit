//
// OpenPgpArmoredFilter.cs
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

using MimeKit.Utils;
using MimeKit.IO.Filters;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A filter to strip off data before and after an armored OpenPGP block.
	/// </summary>
	/// <remarks>
	/// Filters out data before and after armored OpenPGP blocks.
	/// </remarks>
	class OpenPgpBlockFilter : MimeFilterBase
	{
		readonly byte[] beginMarker;
		readonly byte[] endMarker;
		bool seenBeginMarker;
		bool seenEndMarker;
		bool midline;

		/// <summary>
		/// Initialize a new instance of the <see cref="OpenPgpBlockFilter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="OpenPgpBlockFilter"/>.
		/// </remarks>
		/// <param name="beginMarker">An OpenPGP begin marker.</param>
		/// <param name="endMarker">An OpenPGP end marker.</param>
		public OpenPgpBlockFilter (string beginMarker, string endMarker)
		{
			this.beginMarker = CharsetUtils.UTF8.GetBytes (beginMarker);
			this.endMarker = CharsetUtils.UTF8.GetBytes (endMarker);
		}

		static bool IsMarker (byte[] input, int startIndex, byte[] marker)
		{
			int i = startIndex;

			for (int j = 0; j < marker.Length; i++, j++) {
				if (input[i] != marker[j])
					return false;
			}

			if (input[i] == (byte) '\r')
				i++;

			return input[i] == (byte) '\n';
		}

		static bool IsPartialMatch (byte[] input, int startIndex, int endIndex, byte[] marker)
		{
			int i = startIndex;

			for (int j = 0; j < marker.Length && i < endIndex; i++, j++) {
				if (input[i] != marker[j])
					return false;
			}

			if (i < endIndex && input[i] == (byte) '\r')
				i++;

			return i == endIndex;
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
			int endIndex = startIndex + length;
			int index = startIndex;

			outputIndex = startIndex;
			outputLength = 0;

			if (seenEndMarker || length == 0)
				return input;

			if (midline) {
				while (index < endIndex && input[index] != (byte) '\n')
					index++;

				if (index == endIndex) {
					if (seenBeginMarker)
						outputLength = index - startIndex;

					return input;
				}

				midline = false;
			}

			if (!seenBeginMarker) {
				do {
					int lineIndex = index;

					while (index < endIndex && input[index] != (byte) '\n')
						index++;

					if (index == endIndex) {
						if (IsPartialMatch (input, lineIndex, index, beginMarker))
							SaveRemainingInput (input, lineIndex, index - lineIndex);
						else
							midline = true;
						return input;
					}

					index++;

					if (IsMarker (input, lineIndex, beginMarker)) {
						outputLength = index - lineIndex;
						outputIndex = lineIndex;
						seenBeginMarker = true;
						break;
					}
				} while (index < endIndex);

				if (index == endIndex)
					return input;
			}

			do {
				int lineIndex = index;

				while (index < endIndex && input[index] != (byte) '\n')
					index++;

				if (index == endIndex) {
					if (!flush) {
						if (IsPartialMatch (input, lineIndex, index, endMarker)) {
							SaveRemainingInput (input, lineIndex, index - lineIndex);
							outputLength = lineIndex - outputIndex;
						} else {
							outputLength = index - outputIndex;
							midline = true;
						}

						return input;
					}

					outputLength = index - outputIndex;
					return input;
				}

				index++;

				if (IsMarker (input, lineIndex, endMarker)) {
					seenEndMarker = true;
					break;
				}
			} while (index < endIndex);

			outputLength = index - outputIndex;

			return input;
		}

		/// <summary>
		/// Resets the filter.
		/// </summary>
		/// <remarks>
		/// Resets the filter.
		/// </remarks>
		public override void Reset ()
		{
			seenBeginMarker = false;
			seenEndMarker = false;
			midline = false;

			base.Reset ();
		}
	}
}

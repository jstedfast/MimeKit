//
// TrailingWhitespaceFilter.cs
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
		/// Initialize a new instance of the <see cref="TrailingWhitespaceFilter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="TrailingWhitespaceFilter"/>.
		/// </remarks>
		public TrailingWhitespaceFilter ()
		{
		}

		int Filter (ReadOnlySpan<byte> input, byte[] output)
		{
			int outputIndex = 0;
			int count = 0;

			foreach (var c in input) {
				if (c.IsBlank ()) {
					lwsp.Add (c);
				} else if (c == (byte) '\r') {
					output[outputIndex++] = c;
					lwsp.Clear ();
					count++;
				} else if (c == (byte) '\n') {
					output[outputIndex++] = c;
					lwsp.Clear ();
					count++;
				} else {
					if (lwsp.Count > 0) {
						lwsp.CopyTo (OutputBuffer, count);
						outputIndex += lwsp.Count;
						count += lwsp.Count;
						lwsp.Clear ();
					}

					output[outputIndex++] = c;
					count++;
				}
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
			outputLength = Filter (input.AsSpan (startIndex, length), OutputBuffer);
			outputIndex = 0;

			if (flush)
				lwsp.Clear ();

			return OutputBuffer;
		}

		/// <summary>
		/// Reset the filter.
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

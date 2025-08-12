﻿//
// Unix2DosFilter.cs
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

namespace MimeKit.IO.Filters {
	/// <summary>
	/// A filter that will convert from Unix line endings to Windows/DOS line endings.
	/// </summary>
	/// <remarks>
	/// Converts from Unix line endings to Windows/DOS line endings.
	/// </remarks>
	public class Unix2DosFilter : MimeFilterBase
	{
		readonly bool ensureNewLine;
		byte pc;

		/// <summary>
		/// Initialize a new instance of the <see cref="Unix2DosFilter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="Unix2DosFilter"/>.
		/// </remarks>
		/// <param name="ensureNewLine">Ensure that the stream ends with a new line.</param>
		public Unix2DosFilter (bool ensureNewLine = false)
		{
			this.ensureNewLine = ensureNewLine;
		}

		int Filter (ReadOnlySpan<byte> input, byte[] output, bool flush)
		{
			int outputIndex = 0;

			for (int i = 0; i < input.Length; i++) {
				byte c = input[i];

				if (c == (byte) '\n') {
					if (pc != (byte) '\r')
						output[outputIndex++] = (byte) '\r';
					output[outputIndex++] = c;
				} else {
					output[outputIndex++] = c;
				}

				pc = c;
			}

			if (flush && ensureNewLine && pc != (byte) '\n') {
				if (pc != (byte) '\r')
					output[outputIndex++] = (byte) '\r';
				output[outputIndex++] = (byte) '\n';
				pc = (byte) '\n';
			}

			return outputIndex;
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
		/// <param name="flush">If set to <see langword="true" />, all internally buffered data should be flushed to the output buffer.</param>
		protected override byte[] Filter (byte[] input, int startIndex, int length, out int outputIndex, out int outputLength, bool flush)
		{
			EnsureOutputSize (length * 2 + (flush && ensureNewLine ? 2 : 0), false);

			outputLength = Filter (input.AsSpan (startIndex, length), OutputBuffer, flush);
			outputIndex = 0;

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
			pc = 0;
			base.Reset ();
		}
	}
}

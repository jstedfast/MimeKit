//
// DkimSimpleBodyFilter.cs
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

using System;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A filter for the DKIM simple body canonicalization.
	/// </summary>
	/// <remarks>
	/// A filter for the DKIM simple body canonicalization.
	/// </remarks>
	class DkimSimpleBodyFilter : DkimBodyFilter
	{
		bool cr;

		/// <summary>
		/// Initialize a new instance of the <see cref="DkimSimpleBodyFilter"/> class.
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

		int WriteContent (byte c, Span<byte> output, int outputIndex)
		{
			// unwind our collection of empty lines
			while (EmptyLines > 0) {
				output[outputIndex++] = (byte) '\r';
				output[outputIndex++] = (byte) '\n';
				EmptyLines--;
			}

			LastWasNewLine = false;
			IsEmptyLine = false;

			output[outputIndex++] = c;

			return outputIndex;
		}

		int Filter (ReadOnlySpan<byte> input, Span<byte> output, bool flush)
		{
			int outputIndex = 0;

			foreach (var c in input) {
				if (c == (byte) '\r') {
					if (IsEmptyLine && !cr) {
						// defer the CR until we know whether or not it is part of an empty line
						cr = true;
					} else {
						if (cr) {
							outputIndex = WriteContent ((byte) '\r', output, outputIndex);
							cr = false;
						}

						outputIndex = WriteContent (c, output, outputIndex);
					}
				} else if (c == (byte) '\n') {
					if (IsEmptyLine) {
						EmptyLines++;
					} else {
						output[outputIndex++] = c;
						LastWasNewLine = true;
						IsEmptyLine = true;
					}

					cr = false;
				} else {
					// Note: rfc6376 only recognizes CRLF as a line terminator, so a carriage return that is not
					// followed by a line feed is part of the line's content.
					if (cr) {
						outputIndex = WriteContent ((byte) '\r', output, outputIndex);
						cr = false;
					}

					outputIndex = WriteContent (c, output, outputIndex);
				}
			}

			if (flush && cr) {
				outputIndex = WriteContent ((byte) '\r', output, outputIndex);
				cr = false;
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
			// Note: each '\n' in the input may get deferred as an empty line and later get output as "\r\n".
			EnsureOutputSize ((length * 2) + (EmptyLines * 2) + (cr ? 1 : 0) + 1, false);

			outputLength = Filter (input.AsSpan (startIndex, length), OutputBuffer.AsSpan (), flush);

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
			cr = false;

			base.Reset ();
		}
	}
}

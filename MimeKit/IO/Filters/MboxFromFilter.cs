//
// MboxFromFilter.cs
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
		static ReadOnlySpan<byte> MboxFromMarker => "From "u8;
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
			var span = new ReadOnlySpan<byte> (input, startIndex, length);
			var fromOffsets = new List<int> ();
			int endIndex = length;
			int index = 0;

			if (midline) {
				// in our last pass, we ended midline...
				int next = span.IndexOf ((byte) '\n');

				if (next < 0) {
					// there was no '\n' in the slice, so we're done scanning for mbox From markers
					index = length;
				} else {
					// we are no longer in the middle of a line...
					index = next + 1;
					midline = false;
				}
			}

			while (index < length) {
				// Note: if we are inside this loop, then midline is false.
				var slice = span.Slice (index);
				int next = slice.IndexOf ((byte) '\n');

				if (next >= 0) {
					// we've got a complete line of input...
					if (next >= MboxFromMarker.Length) {
						if (slice.StartsWith (MboxFromMarker)) {
							// record our match...
							fromOffsets.Add (index);
						}
					}
				} else {
					// we've got an incomplete line of input...
					if (slice.Length >= MboxFromMarker.Length) {
						if (slice.StartsWith (MboxFromMarker)) {
							// record our match...
							fromOffsets.Add (index);
						}
					} else {
						if (!flush && slice.SequenceEqual (MboxFromMarker.Slice (0, slice.Length))) {
							// this line *might* start with "From ", but we ran out of data...
							SaveRemainingInput (input, startIndex + index, slice.Length);

							// mark the end of our "consumed" input at 'index' so that we don't
							// copy this remaining data into the output buffer
							endIndex = index;
							break;
						}
					}

					midline = true;
					break;
				}

				// advance to the beginning of the next line...
				index += next + 1;
			}

			if (fromOffsets.Count > 0) {
				int need = endIndex + fromOffsets.Count;

				EnsureOutputSize (need, false);
				outputLength = 0;
				outputIndex = 0;
				index = 0;

				var output = OutputBuffer.AsSpan (0, need);
				ReadOnlySpan<byte> src;
				Span<byte> dest;

				foreach (var offset in fromOffsets) {
					if (index < offset) {
						src = span.Slice (index, offset - index);
						dest = output.Slice (outputLength, src.Length);

						src.CopyTo (dest);

						outputLength += src.Length;
						index = offset;
					}

					// munge the beginning of the "From "-line.
					output[outputLength++] = (byte) '>';
				}

				// copy the remaining chunk of input (minus any bytes we may have saved for the next pass)
				src = span.Slice (index, endIndex - index);
				dest = output.Slice (outputLength, src.Length);

				src.CopyTo (dest);

				outputLength += src.Length;

				return OutputBuffer;
			}

			outputIndex = startIndex;
			outputLength = endIndex;

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

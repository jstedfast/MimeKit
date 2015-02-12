//
// ArmoredFromFilter.cs
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

using System;
using System.Collections.Generic;

namespace MimeKit.IO.Filters {
	/// <summary>
	/// A filter that armors lines beginning with "From " by encoding the 'F' with the
	/// Quoted-Printable encoding.
	/// </summary>
	/// <remarks>
	/// <para>From-armoring is a workaround to prevent receiving clients (or servers)
	/// that uses the mbox file format for local storage from munging the line
	/// by prepending a ">", as is typical with the mbox format.</para>
	/// <para>This armoring technique ensures that the receiving client will still
	/// be able to verify S/MIME signatures.</para>
	/// </remarks>
	public class ArmoredFromFilter : MimeFilterBase
	{
		const string From = "From ";
		bool midline;

		/// <summary>
		/// Initializes a new instance of the <see cref="ArmoredFromFilter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="ArmoredFromFilter"/>.
		/// </remarks>
		public ArmoredFromFilter ()
		{
		}

		static bool StartsWithFrom (byte[] input, int startIndex, int endIndex)
		{
			for (int i = 0, index = startIndex; i < From.Length && index < endIndex; i++, index++) {
				if (input[index] != (byte) From[i])
					return false;
			}

			return true;
		}

		static void ValidateArguments (byte[] input, int startIndex, int length)
		{
			if (input == null)
				throw new ArgumentNullException ("input");

			if (startIndex < 0 || startIndex > input.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (length < 0 || length > (input.Length - startIndex))
				throw new ArgumentOutOfRangeException ("length");
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
			ValidateArguments (input, startIndex, length);

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
				int need = (endIndex - startIndex) + fromOffsets.Count * 2;

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

					// encode the F using quoted-printable
					OutputBuffer[outputLength++] = (byte) '=';
					OutputBuffer[outputLength++] = (byte) '4';
					OutputBuffer[outputLength++] = (byte) '6';
					index++;
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
		/// Resets the filter.
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

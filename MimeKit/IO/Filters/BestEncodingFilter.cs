//
// BestEncodingFilter.cs
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

namespace MimeKit.IO.Filters {
	/// <summary>
	/// A filter that can be used to determine the most efficient Content-Transfer-Encoding.
	/// </summary>
	/// <remarks>
	/// Keeps track of the content that gets passed through the filter in order to
	/// determine the most efficient <see cref="ContentEncoding"/> to use.
	/// </remarks>
	public class BestEncodingFilter : IMimeFilter
	{
		readonly byte[] marker = new byte[6];
		bool midline, hasMarker;
		int maxline, linelen;
		int count0, count8;
		int markerLength;
		int total;

		/// <summary>
		/// Initializes a new instance of the <see cref="BestEncodingFilter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="BestEncodingFilter"/>.
		/// </remarks>
		public BestEncodingFilter ()
		{
		}

		/// <summary>
		/// Gets the best encoding given the specified constraints.
		/// </summary>
		/// <remarks>
		/// Gets the best encoding given the specified constraints.
		/// </remarks>
		/// <returns>The best encoding.</returns>
		/// <param name="constraint">The encoding constraint.</param>
		/// <param name="maxLineLength">The maximum allowable line length (not counting the CRLF). Must be between <c>72</c> and <c>998</c> (inclusive).</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="maxLineLength"/> is not between <c>72</c> and <c>998</c> (inclusive).</para>
		/// <para>-or-</para>
		/// <para><paramref name="constraint"/> is not a valid value.</para>
		/// </exception>
		public ContentEncoding GetBestEncoding (EncodingConstraint constraint, int maxLineLength = 78)
		{
			if (maxLineLength < 72 || maxLineLength > 998)
				throw new ArgumentOutOfRangeException ("maxLineLength");

			switch (constraint) {
			case EncodingConstraint.SevenBit:
				if (count0 > 0)
					return ContentEncoding.Base64;

				if (count8 > 0) {
					if (count8 >= (int) (total * (17.0 / 100.0)))
						return ContentEncoding.Base64;

					return ContentEncoding.QuotedPrintable;
				}

				if (maxline > maxLineLength)
					return ContentEncoding.QuotedPrintable;

				break;
			case EncodingConstraint.EightBit:
				if (count0 > 0)
					return ContentEncoding.Base64;

				if (maxline > maxLineLength)
					return ContentEncoding.QuotedPrintable;

				if (count8 > 0)
					return ContentEncoding.EightBit;

				break;
			case EncodingConstraint.None:
				if (count0 > 0)
					return ContentEncoding.Binary;

				if (maxline > maxLineLength)
					return ContentEncoding.QuotedPrintable;

				if (count8 > 0)
					return ContentEncoding.EightBit;

				break;
			default:
				throw new ArgumentOutOfRangeException ("constraint");
			}

			if (hasMarker)
				return ContentEncoding.QuotedPrintable;

			return ContentEncoding.SevenBit;
		}

		#region IMimeFilter implementation

		static unsafe bool IsMboxMarker (byte[] marker)
		{
			const uint FromMask = 0xFFFFFFFF;
			const uint From     = 0x6D6F7246;

			fixed (byte* buf = marker) {
				uint* word = (uint*) buf;

				if ((*word & FromMask) != From)
					return false;

				return *(buf + 4) == (byte) ' ';
			}
		}

		unsafe void Scan (byte* inptr, byte* inend)
		{
			while (inptr < inend) {
				if (midline) {
					byte c = 0;

					while (inptr < inend && (c = *inptr++) != (byte) '\n') {
						if (c == 0)
							count0++;
						else if ((c & 0x80) != 0)
							count8++;

						if (!hasMarker && markerLength > 0 && markerLength < 5)
							marker[markerLength++] = c;

						linelen++;
					}

					if (c == (byte) '\n') {
						maxline = Math.Max (maxline, linelen);
						midline = false;
						linelen = 0;
					}
				}

				// check our from-save buffer for "From "
				if (!hasMarker && markerLength == 5 && IsMboxMarker (marker))
					hasMarker = true;

				markerLength = 0;
				midline = true;
			}
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
		/// Filters the specified input.
		/// </summary>
		/// <remarks>
		/// Filters the specified input buffer starting at the given index,
		/// spanning across the specified number of bytes.
		/// </remarks>
		/// <returns>The filtered output.</returns>
		/// <param name="input">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes of the input to filter.</param>
		/// <param name="outputIndex">The starting index of the output in the returned buffer.</param>
		/// <param name="outputLength">The length of the output buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="input"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the <paramref name="input"/> byte array.
		/// </exception>
		public byte[] Filter (byte[] input, int startIndex, int length, out int outputIndex, out int outputLength)
		{
			ValidateArguments (input, startIndex, length);

			unsafe {
				fixed (byte* inptr = input) {
					Scan (inptr + startIndex, inptr + startIndex + length);
				}
			}

			maxline = Math.Max (maxline, linelen);
			total += length;

			outputIndex = startIndex;
			outputLength = length;

			return input;
		}

		/// <summary>
		/// Filters the specified input, flushing all internally buffered data to the output.
		/// </summary>
		/// <remarks>
		/// Filters the specified input buffer starting at the given index,
		/// spanning across the specified number of bytes.
		/// </remarks>
		/// <returns>The filtered output.</returns>
		/// <param name="input">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes of the input to filter.</param>
		/// <param name="outputIndex">The starting index of the output in the returned buffer.</param>
		/// <param name="outputLength">The length of the output buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="input"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the <paramref name="input"/> byte array.
		/// </exception>
		public byte[] Flush (byte[] input, int startIndex, int length, out int outputIndex, out int outputLength)
		{
			return Filter (input, startIndex, length, out outputIndex, out outputLength);
		}

		/// <summary>
		/// Resets the filter.
		/// </summary>
		/// <remarks>
		/// Resets the filter.
		/// </remarks>
		public void Reset ()
		{
			hasMarker = false;
			markerLength = 0;
			midline = false;
			linelen = 0;
			maxline = 0;
			count0 = 0;
			count8 = 0;
			total = 0;
		}

		#endregion
	}
}


//
// BestEncodingFilter.cs
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
using System.Buffers.Binary;

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
		int maxline, linelen;
		int count0, count8;
		int markerLength;
		bool hasMarker;
		int total;
		byte pc;

		/// <summary>
		/// Initialize a new instance of the <see cref="BestEncodingFilter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="BestEncodingFilter"/>.
		/// </remarks>
		public BestEncodingFilter ()
		{
		}

		/// <summary>
		/// Get the best encoding given the specified constraints.
		/// </summary>
		/// <remarks>
		/// Gets the best encoding given the specified constraints.
		/// </remarks>
		/// <returns>The best encoding.</returns>
		/// <param name="constraint">The encoding constraint.</param>
		/// <param name="maxLineLength">The maximum allowable line length (not counting the CRLF). Must be between <c>60</c> and <c>998</c> (inclusive).</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="maxLineLength"/> is not between <c>60</c> and <c>998</c> (inclusive).</para>
		/// <para>-or-</para>
		/// <para><paramref name="constraint"/> is not a valid value.</para>
		/// </exception>
		public ContentEncoding GetBestEncoding (EncodingConstraint constraint, int maxLineLength = 78)
		{
			if (maxLineLength < FormatOptions.MinimumLineLength || maxLineLength > FormatOptions.MaximumLineLength)
				throw new ArgumentOutOfRangeException (nameof (maxLineLength));

			switch (constraint) {
			case EncodingConstraint.SevenBit:
				if (count0 > 0)
					return ContentEncoding.Base64;

				if (count8 > 0) {
					if (count8 >= (int) (total * (17.0 / 100.0)))
						return ContentEncoding.Base64;

					return ContentEncoding.QuotedPrintable;
				}

				if (hasMarker || maxline > maxLineLength)
					return ContentEncoding.QuotedPrintable;

				break;
			case EncodingConstraint.EightBit:
				if (count0 > 0)
					return ContentEncoding.Base64;

				if (hasMarker || maxline > maxLineLength)
					return ContentEncoding.QuotedPrintable;

				if (count8 > 0)
					return ContentEncoding.EightBit;

				break;
			case EncodingConstraint.None:
				if (hasMarker || maxline > maxLineLength) {
					if (count0 > 0 || count8 > (int) (total * (17.0 / 100.0)))
						return ContentEncoding.Base64;

					return ContentEncoding.QuotedPrintable;
				}

				if (count0 > 0)
					return ContentEncoding.Binary;

				if (count8 > 0)
					return ContentEncoding.EightBit;

				break;
			default:
				throw new ArgumentOutOfRangeException (nameof (constraint));
			}

			return ContentEncoding.SevenBit;
		}

		#region IMimeFilter implementation

		static bool IsMboxMarker (byte[] marker)
		{
			const uint From = 0x6D6F7246;

			uint word = BinaryPrimitives.ReadUInt32LittleEndian (marker.AsSpan ());

			if (word != From)
				return false;

			return marker[4] == (byte) ' ';
		}

		unsafe void Scan (byte* inptr, byte* inend)
		{
			while (inptr < inend) {
				byte c = 0;

				while (inptr < inend && (c = *inptr++) != (byte) '\n') {
					if (c == 0)
						count0++;
					else if ((c & 0x80) != 0)
						count8++;

					if (!hasMarker && markerLength < 5)
						marker[markerLength++] = c;

					linelen++;
					pc = c;
				}

				if (c == (byte) '\n') {
					if (pc == (byte) '\r')
						linelen--;

					maxline = Math.Max (maxline, linelen);
					linelen = 0;

					// check our from-save buffer for "From "
					if (!hasMarker && markerLength == 5 && IsMboxMarker (marker))
						hasMarker = true;

					markerLength = 0;
				}
			}
		}

		static void ValidateArguments (byte[] input, int startIndex, int length)
		{
			if (input is null)
				throw new ArgumentNullException (nameof (input));

			if (startIndex < 0 || startIndex > input.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (length < 0 || length > (input.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (length));
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
		/// Filter the specified input, flushing all internally buffered data to the output.
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
		/// Reset the filter.
		/// </summary>
		/// <remarks>
		/// Resets the filter.
		/// </remarks>
		public void Reset ()
		{
			hasMarker = false;
			markerLength = 0;
			linelen = 0;
			maxline = 0;
			count0 = 0;
			count8 = 0;
			total = 0;
			pc = 0;
		}

		#endregion
	}
}


//
// MimeFilterBase.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2014 Xamarin Inc.
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
	/// A base implementation for MIME filters.
	/// </summary>
    public abstract class MimeFilterBase : IMimeFilter
    {
		protected byte[] output = null;
		byte[] preload = null;
		byte[] inbuf = null;
		int preloadLength;

		/// <summary>
		/// Filter the specified input.
		/// </summary>
		/// <returns>The filtered output.</returns>
		/// <param name="input">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The length of the input buffer, starting at <paramref name="startIndex"/>.</param>
		/// <param name="outputIndex">The output index.</param>
		/// <param name="outputLength">The output length.</param>
		/// <param name="flush">If set to <c>true</c>, all internally buffered data should be flushed to the output buffer.</param>
		protected abstract byte[] Filter (byte[] input, int startIndex, int length, out int outputIndex, out int outputLength, bool flush);

		static int GetIdealBufferSize (int need)
		{
			return (need + 63) & ~63;
		}

		byte[] PreFilter (byte[] input, ref int startIndex, ref int length)
		{
			if (preloadLength == 0)
				return input;
			
			// We need to preload any data from a previous filter iteration into 
			// the input buffer, so make sure that we have room...
			int totalLength = length + preloadLength;
			
			if (inbuf == null || inbuf.Length < totalLength) {
				// NOTE: Array.Resize() copies data, we don't need that (slower)
				inbuf = new byte[GetIdealBufferSize (totalLength)];
			}
			
			// Copy our preload data into our internal input buffer
			Array.Copy (preload, 0, inbuf, 0, preloadLength);
			
			// Copy our input to the end of our internal input buffer
			Array.Copy (input, startIndex, inbuf, preloadLength, length);
			
			startIndex = preloadLength;
			length = totalLength;
			preloadLength = 0;
			
			return inbuf;
		}

		static void ValidateArguments (byte[] input, int startIndex, int length)
		{
			if (input == null)
				throw new ArgumentNullException ("input");

			if (startIndex < 0 || startIndex > input.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (length < 0 || startIndex + length > input.Length)
				throw new ArgumentOutOfRangeException ("length");
		}
		
		/// <summary>
		/// Filters the specified input.
		/// </summary>
		/// <param name='input'>The input buffer.</param>
		/// <param name='startIndex'>The starting index of the input buffer.</param>
		/// <param name='length'>The number of bytes of the input to filter.</param>
		/// <param name='outputIndex'>The starting index of the output in the returned buffer.</param>
		/// <param name='outputLength'>The length of the output buffer.</param>
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

			input = PreFilter (input, ref startIndex, ref length);
			
			return Filter (input, startIndex, length, out outputIndex, out outputLength, false);
		}
		
		/// <summary>
		/// Filters the specified input, flushing all internally buffered data to the output.
		/// </summary>
		/// <param name='input'>The input buffer.</param>
		/// <param name='startIndex'>The starting index of the input buffer.</param>
		/// <param name='length'>The number of bytes of the input to filter.</param>
		/// <param name='outputIndex'>The starting index of the output in the returned buffer.</param>
		/// <param name='outputLength'>The length of the output buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="input"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the <paramref name="input"/> byte array.
		/// </exception>
		public byte[] Flush (byte[] input, int startIndex, int length, out int outputIndex, out int outputLength)
		{
			ValidateArguments (input, startIndex, length);

			input = PreFilter (input, ref startIndex, ref length);

			return Filter (input, startIndex, length, out outputIndex, out outputLength, true);
		}

		/// <summary>
		/// Resets the filter.
		/// </summary>
		public virtual void Reset ()
		{
			preloadLength = 0;
		}

		/// <summary>
		/// Saves the remaining input for the next round of processing.
		/// </summary>
		/// <param name="input">The input buffer.</param>
		/// <param name="startIndex">The starting index of the buffer to save.</param>
		/// <param name="length">The length of the buffer to save, starting at <paramref name="startIndex"/>.</param>
		protected void SaveRemainingInput (byte[] input, int startIndex, int length)
		{
			if (length == 0)
				return;

			if (preload == null || preload.Length < length)
				preload = new byte[GetIdealBufferSize (length)];

			Array.Copy (input, startIndex, preload, 0, length);
			preloadLength = length;
		}

		/// <summary>
		/// Ensures that the output buffer is greater than or equal to the specified size.
		/// </summary>
		/// <param name="size">The minimum size needed.</param>
		/// <param name="keep">If set to <c>true</c>, the current output should be preserved.</param>
		protected void EnsureOutputSize (int size, bool keep)
		{
			if (size == 0)
				return;

			int outputSize = output != null ? output.Length : 0;

			if (outputSize >= size)
				return;

			if (keep)
				Array.Resize<byte> (ref output, GetIdealBufferSize (size));
			else
				output = new byte[GetIdealBufferSize (size)];
		}
	}
}

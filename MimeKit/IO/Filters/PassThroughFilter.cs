//
// PassThroughFilter.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc.
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

namespace MimeKit.IO.Filters {
	/// <summary>
	/// A filter that simply passes data through without any processing.
	/// </summary>
	/// <remarks>
	/// Passes data through without any processing.
	/// </remarks>
	public class PassThroughFilter : IMimeFilter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.IO.Filters.PassThroughFilter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="PassThroughFilter"/>.
		/// </remarks>
		public PassThroughFilter ()
		{
		}

		#region IMimeFilter implementation

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
		public byte[] Filter (byte[] input, int startIndex, int length, out int outputIndex, out int outputLength)
		{
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
		public byte[] Flush (byte[] input, int startIndex, int length, out int outputIndex, out int outputLength)
		{
			outputIndex = startIndex;
			outputLength = length;
			return input;
		}

		/// <summary>
		/// Resets the filter.
		/// </summary>
		/// <remarks>
		/// Resets the filter.
		/// </remarks>
		public void Reset ()
		{
		}

		#endregion
	}
}

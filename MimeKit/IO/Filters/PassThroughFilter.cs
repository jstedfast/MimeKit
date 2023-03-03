// License// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
		/// Initialize a new instance of the <see cref="PassThroughFilter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="PassThroughFilter"/>.
		/// </remarks>
		public PassThroughFilter ()
		{
		}

		#region IMimeFilter implementation

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
		public byte[] Filter (byte[] input, int startIndex, int length, out int outputIndex, out int outputLength)
		{
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
		public byte[] Flush (byte[] input, int startIndex, int length, out int outputIndex, out int outputLength)
		{
			outputIndex = startIndex;
			outputLength = length;
			return input;
		}

		/// <summary>
		/// Reset the filter.
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

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace MimeKit.IO.Filters {
	/// <summary>
	/// A filter that will convert from Windows/DOS line endings to Unix line endings.
	/// </summary>
	/// <remarks>
	/// Converts from Windows/DOS line endings to Unix line endings.
	/// </remarks>
	public class Dos2UnixFilter : MimeFilterBase
	{
		readonly bool ensureNewLine;
		byte pc;

		/// <summary>
		/// Initialize a new instance of the <see cref="Dos2UnixFilter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="Dos2UnixFilter"/>.
		/// </remarks>
		/// <param name="ensureNewLine">Ensure that the stream ends with a new line.</param>
		public Dos2UnixFilter (bool ensureNewLine = false)
		{
			this.ensureNewLine = ensureNewLine;
		}

		int Filter (ReadOnlySpan<byte> input, byte[] output, bool flush)
		{
			int outputIndex = 0;

			foreach (var c in input) {
				if (c == (byte) '\n') {
					output[outputIndex++] = c;
				} else {
					if (pc == (byte) '\r')
						output[outputIndex++] = pc;

					if (c != (byte) '\r')
						output[outputIndex++] = c;
				}

				pc = c;
			}

			if (flush && ensureNewLine && pc != (byte) '\n')
				output[outputIndex++] = (byte) '\n';

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
		/// <param name="flush">If set to <c>true</c>, all internally buffered data should be flushed to the output buffer.</param>
		protected override byte[] Filter (byte[] input, int startIndex, int length, out int outputIndex, out int outputLength, bool flush)
		{
			if (pc == (byte) '\r')
				EnsureOutputSize (length + (flush && ensureNewLine ? 2 : 1), false);
			else
				EnsureOutputSize (length + (flush && ensureNewLine ? 1 : 0), false);

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

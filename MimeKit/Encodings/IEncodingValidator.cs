//
// IEncodingValidator.cs
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

namespace MimeKit.Encodings {
	/// <summary>
	/// Incrementally validates content encoded with a particular encoding.
	/// </summary>
	/// <remarks>
	/// MIME uses various content encodings to transform 8bit and binary content such
	/// as images and other types of multimedia to ensure that the data remains
	/// intact when sent via 7bit transports such as SMTP.
	/// </remarks>
	interface IEncodingValidator
	{
		/// <summary>
		/// Get the encoding.
		/// </summary>
		/// <remarks>
		/// Gets the encoding that the validator supports.
		/// </remarks>
		/// <value>The encoding.</value>
		ContentEncoding Encoding { get; }

#if NET8_0_OR_GREATER
		/// <summary>
		/// Get whether the current state permits consuming prevalidated content.
		/// </summary>
		bool CanConsumeCleanContent { get; }

		/// <summary>
		/// Get the number of encoded payload bytes remaining in the current UU line.
		/// </summary>
		/// <remarks>
		/// For UU encoding, -1 indicates a new length byte is expected, while 0
		/// indicates only the line ending remains. Other encodings return 0,
		/// and the reader does not use this property for those encodings.
		/// </remarks>
		int CleanContentRemaining { get; }

		/// <summary>
		/// Advance the validator state over a prevalidated content prefix.
		/// </summary>
		/// <remarks>
		/// The caller must validate the content for this encoding and end the range
		/// immediately after an LF. This method does not report diagnostics.
		/// </remarks>
		/// <param name="input">The prevalidated content.</param>
		/// <param name="length">The number of bytes to consume.</param>
		/// <param name="alphabetCount">The number of Base64 alphabet bytes, ignored by other encodings.</param>
		/// <param name="lines">The number of line feeds in the content.</param>
		unsafe void ConsumeCleanContent (byte* input, int length, uint alphabetCount, int lines);
#endif

		/// <summary>
		/// Write a sequence of bytes to the validator.
		/// </summary>
		/// <remarks>
		/// Writes a sequence of bytes to the validator.
		/// </remarks>
		/// <param name="buffer">The buffer.</param>
		/// <param name="startIndex">The starting index of the buffer.</param>
		/// <param name="length">The length of the buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the <paramref name="buffer"/> byte array.
		/// </exception>
		void Write (byte[] buffer, int startIndex, int length);

		/// <summary>
		/// Flush the validator state.
		/// </summary>
		/// <remarks>
		/// Flushes the validator state.
		/// </remarks>
		void Flush ();
	}
}

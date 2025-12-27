//
// IEncodingValidator.cs
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

		/// <summary>
		/// Validate that a buffer contains only valid encoded content.
		/// </summary>
		/// <remarks>
		/// Validates that the input buffer contains only valid encoded content.
		/// </remarks>
		/// <param name="input">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The length of the input buffer.</param>
		/// <returns><see langword="true"/> if the content is valid; otherwise, <see langword="false"/>.</returns>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="input"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the <paramref name="input"/> byte array.
		/// </exception>
		bool Validate (byte[] input, int startIndex, int length);

		/// <summary>
		/// Complete the validation process.
		/// </summary>
		/// <remarks>
		/// Completes the validation process.
		/// </remarks>
		/// <returns><see langword="true"/> if the content was valid; otherwise, <see langword="false"/>.</returns>
		bool Complete ();
	}
}

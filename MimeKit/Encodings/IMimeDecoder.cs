//
// IMimeDecoder.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2012 Jeffrey Stedfast
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

namespace MimeKit.Encodings {
	/// <summary>
	/// An interface for incrementally decoding content.
	/// </summary>
	public interface IMimeDecoder : ICloneable
	{
		/// <summary>
		/// Gets the encoding.
		/// </summary>
		/// <value>
		/// The encoding.
		/// </value>
		ContentEncoding Encoding { get; }

		/// <summary>
		/// Estimates the length of the output.
		/// </summary>
		/// <returns>
		/// The estimated output length.
		/// </returns>
		/// <param name='inputLength'>
		/// The input length.
		/// </param>
		int EstimateOutputLength (int inputLength);

		/// <summary>
		/// Decodes the specified input into the output buffer.
		/// </summary>
		/// <returns>
		/// The number of bytes written to the output buffer.
		/// </returns>
		/// <param name='input'>
		/// A pointer to the beginning of the input buffer.
		/// </param>
		/// <param name='length'>
		/// The length of the input buffer.
		/// </param>
		/// <param name='output'>
		/// A pointer to the beginning of the output buffer.
		/// </param>
		unsafe int Decode (byte* input, int length, byte* output);

		/// <summary>
		/// Decodes the specified input into the output buffer.
		/// </summary>
		/// <returns>
		/// The number of bytes written to the output buffer.
		/// </returns>
		/// <param name='input'>
		/// The input buffer.
		/// </param>
		/// <param name='startIndex'>
		/// The starting index of the input buffer.
		/// </param>
		/// <param name='length'>
		/// The length of the input buffer.
		/// </param>
		/// <param name='output'>
		/// The output buffer.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="input"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="output"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the <paramref name="input"/> byte array.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="output"/> is not large enough to contain the decoded content.
		/// Use the <see cref="EstimateOutputLength"/> method to properly determine the 
		/// necessary length of the <paramref name="output"/> byte array.
		/// </exception>
		int Decode (byte[] input, int startIndex, int length, byte[] output);

		/// <summary>
		/// Resets the decoder.
		/// </summary>
		void Reset ();
	}
}

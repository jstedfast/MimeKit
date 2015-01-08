//
// CharsetFilter.cs
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

using System;
using System.Text;

#if PORTABLE
using Encoding = Portable.Text.Encoding;
using Encoder = Portable.Text.Encoder;
using Decoder = Portable.Text.Decoder;
#endif

namespace MimeKit.IO.Filters {
	/// <summary>
	/// A charset filter for incrementally converting text streams from
	/// one charset encoding to another.
	/// </summary>
	/// <remarks>
	/// Incrementally converts text from one charset encoding to another.
	/// </remarks>
	public class CharsetFilter : MimeFilterBase
	{
		readonly char[] chars = new char[1024];
		readonly Decoder decoder;
		readonly Encoder encoder;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.IO.Filters.CharsetFilter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="CharsetFilter"/> to convert text from the specified
		/// source encoding into the target charset encoding.
		/// </remarks>
		/// <param name="sourceEncodingName">Source encoding name.</param>
		/// <param name="targetEncodingName">Target encoding name.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="sourceEncodingName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="targetEncodingName"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <para>The <paramref name="sourceEncodingName"/> is not supported by the system.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="targetEncodingName"/> is not supported by the system.</para>
		/// </exception>
		public CharsetFilter (string sourceEncodingName, string targetEncodingName)
			: this (Encoding.GetEncoding (sourceEncodingName), Encoding.GetEncoding (targetEncodingName))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.IO.Filters.CharsetFilter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="CharsetFilter"/> to convert text from the specified
		/// source encoding into the target charset encoding.
		/// </remarks>
		/// <param name="sourceCodePage">Source code page.</param>
		/// <param name="targetCodePage">Target code page.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="sourceCodePage"/> is less than zero or greater than 65535.</para>
		/// <para>-or-</para>
		/// <para><paramref name="targetCodePage"/> is less than zero or greater than 65535.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <para>The <paramref name="sourceCodePage"/> is not supported by the system.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="targetCodePage"/> is not supported by the system.</para>
		/// </exception>
		public CharsetFilter (int sourceCodePage, int targetCodePage)
			: this (Encoding.GetEncoding (sourceCodePage), Encoding.GetEncoding (targetCodePage))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.IO.Filters.CharsetFilter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="CharsetFilter"/> to convert text from the specified
		/// source encoding into the target charset encoding.
		/// </remarks>
		/// <param name="sourceEncoding">Source encoding.</param>
		/// <param name="targetEncoding">Target encoding.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="sourceEncoding"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="targetEncoding"/> is <c>null</c>.</para>
		/// </exception>
		public CharsetFilter (Encoding sourceEncoding, Encoding targetEncoding)
		{
			if (sourceEncoding == null)
				throw new ArgumentNullException ("sourceEncoding");

			if (targetEncoding == null)
				throw new ArgumentNullException ("targetEncoding");

			SourceEncoding = sourceEncoding;
			TargetEncoding = targetEncoding;

			decoder = (Decoder) SourceEncoding.GetDecoder ();
			encoder = (Encoder) TargetEncoding.GetEncoder ();
		}

		/// <summary>
		/// Gets the source encoding.
		/// </summary>
		/// <remarks>
		/// Gets the source encoding.
		/// </remarks>
		/// <value>The source encoding.</value>
		public Encoding SourceEncoding {
			get; private set;
		}

		/// <summary>
		/// Gets the target encoding.
		/// </summary>
		/// <remarks>
		/// Gets the target encoding.
		/// </remarks>
		/// <value>The target encoding.</value>
		public Encoding TargetEncoding {
			get; private set;
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
			int inputIndex = startIndex;
			int inputLeft = length;
			bool decoded = false;
			bool encoded = false;
			int nwritten, nread;
			int outputOffset = 0;
			int outputLeft;
			int charIndex;
			int charsLeft;
			
			do {
				charsLeft = chars.Length;
				charIndex = 0;

				if (!decoded) {
					decoder.Convert (input, inputIndex, inputLeft, chars, charIndex, charsLeft, flush, out nread, out nwritten, out decoded);
					if (nwritten > 0)
						encoded = false;
					charIndex += nwritten;
					inputIndex += nread;
					inputLeft -= nread;
				}
				
				charsLeft = charIndex;
				charIndex = 0;
				
				// encode *all* input chars into the output buffer
				while (!encoded) {
					EnsureOutputSize (outputOffset + TargetEncoding.GetMaxByteCount (charsLeft) + 4, true);
					outputLeft = OutputBuffer.Length - outputOffset;
					
					encoder.Convert (chars, charIndex, charsLeft, OutputBuffer, outputOffset, outputLeft, flush, out nread, out nwritten, out encoded);
					outputOffset += nwritten;
					charIndex += nread;
					charsLeft -= nread;
				}
			} while (!decoded);

			outputLength = outputOffset;
			outputIndex = 0;
				
			return OutputBuffer;
		}

		/// <summary>
		/// Resets the filter.
		/// </summary>
		/// <remarks>
		/// Resets the filter.
		/// </remarks>
		public override void Reset ()
		{
			decoder.Reset ();
			encoder.Reset ();
			base.Reset ();
		}
	}
}

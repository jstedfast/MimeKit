//
// CharsetFilter.cs
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
using System.Text;

namespace MimeKit.IO.Filters {
	public class CharsetFilter : MimeFilterBase
	{
		readonly char[] chars = new char[1024];
		readonly Decoder decoder;
		readonly Encoder encoder;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.IO.Filters.CharsetFilter"/> class.
		/// </summary>
		/// <param name='sourceEncodingName'>
		/// Source encoding name.
		/// </param>
		/// <param name='targetEncodingName'>
		/// Target encoding name.
		/// </param>
		public CharsetFilter (string sourceEncodingName, string targetEncodingName)
			: this (Encoding.GetEncoding (sourceEncodingName), Encoding.GetEncoding (targetEncodingName))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.IO.Filters.CharsetFilter"/> class.
		/// </summary>
		/// <param name='sourceCodePage'>
		/// Source code page.
		/// </param>
		/// <param name='targetCodePage'>
		/// Target code page.
		/// </param>
		public CharsetFilter (int sourceCodePage, int targetCodePage)
			: this (Encoding.GetEncoding (sourceCodePage), Encoding.GetEncoding (targetCodePage))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.IO.Filters.CharsetFilter"/> class.
		/// </summary>
		/// <param name='sourceEncoding'>
		/// Source encoding.
		/// </param>
		/// <param name='targetEncoding'>
		/// Target encoding.
		/// </param>
		public CharsetFilter (Encoding sourceEncoding, Encoding targetEncoding)
		{
			if (sourceEncoding == null)
				throw new ArgumentNullException ("sourceEncoding");

			if (targetEncoding == null)
				throw new ArgumentNullException ("targetEncoding");

			SourceEncoding = sourceEncoding;
			TargetEncoding = targetEncoding;

			decoder = SourceEncoding.GetDecoder ();
			encoder = TargetEncoding.GetEncoder ();
		}

		/// <summary>
		/// Gets the source encoding.
		/// </summary>
		/// <value>
		/// The source encoding.
		/// </value>
		public Encoding SourceEncoding {
			get; private set;
		}

		/// <summary>
		/// Gets the target encoding.
		/// </summary>
		/// <value>
		/// The target encoding.
		/// </value>
		public Encoding TargetEncoding {
			get; private set;
		}

		protected override byte[] Filter (byte[] input, int startIndex, int length, out int outputIndex, out int outputLength, bool flush)
		{
			bool decodeCompleted = false;
			bool encodeCompleted = false;
			int inputIndex = startIndex;
			int inputLeft = length;
			int nwritten, nread;
			int outputOffset = 0;
			int outputLeft;
			int charIndex;
			int charsLeft;
			
			do {
				charsLeft = chars.Length;
				charIndex = 0;

				if (!decodeCompleted) {
					decoder.Convert (input, inputIndex, inputLeft, chars, charIndex, charsLeft, flush, out nread, out nwritten, out decodeCompleted);
					if (nwritten > 0)
						encodeCompleted = false;
					charIndex += nwritten;
					inputIndex += nread;
					inputLeft -= nread;
				}
				
				charsLeft = charIndex;
				charIndex = 0;
				
				// encode *all* input chars into the output buffer
				while (!encodeCompleted) {
					EnsureOutputSize (outputOffset + TargetEncoding.GetMaxByteCount (charsLeft) + 4, true);
					outputLeft = output.Length - outputOffset;
					
					encoder.Convert (chars, charIndex, charsLeft, output, outputOffset, outputLeft, flush, out nread, out nwritten, out encodeCompleted);
					outputOffset += nwritten;
					charIndex += nread;
					charsLeft -= nread;
				}
			} while (!decodeCompleted);

			outputLength = outputOffset;
			outputIndex = 0;
				
			return output;
		}

		/// <summary>
		/// Reset this instance.
		/// </summary>
		public override void Reset ()
		{
			decoder.Reset ();
			encoder.Reset ();
			base.Reset ();
		}
	}
}

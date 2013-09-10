//
// Dos2UnixFilter.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Jeffrey Stedfast
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

namespace MimeKit {
	public class Dos2UnixFilter : MimeFilterBase
	{
		byte pc;

		public Dos2UnixFilter ()
		{
		}

		unsafe int Filter (byte* inbuf, int length, byte* outbuf)
		{
			byte* inend = inbuf + length;
			byte* outptr = outbuf;
			byte* inptr = inbuf;

			while (inptr < inend) {
				if (*inptr == (byte) '\n') {
					*outptr++ = *inptr;
				} else {
					if (pc == (byte) '\r')
						*outptr++ = pc;

					if (*inptr != (byte) '\r')
						*outptr++ = *inptr;
				}

				pc = *inptr++;
			}

			return (int) (outptr - outbuf);
		}

		protected override byte[] Filter (byte[] input, int startIndex, int length, out int outputIndex, out int outputLength, bool flush)
		{
			if (pc == (byte) '\r')
				EnsureOutputSize (length + 1, false);
			else
				EnsureOutputSize (length, false);

			outputIndex = 0;

			unsafe {
				fixed (byte* inptr = input, outptr = output) {
					outputLength = Filter (inptr + startIndex, length, outptr);
				}
			}

			return output;
		}

		public override void Reset ()
		{
			pc = 0;
			base.Reset ();
		}
	}
}

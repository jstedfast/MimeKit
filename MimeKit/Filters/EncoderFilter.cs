//
// EncoderFilter.cs
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

namespace MimeKit {
	public class EncoderFilter : MimeFilterBase
	{
		/// <summary>
		/// Gets the encoder used by this filter.
		/// </summary>
		/// <value>
		/// The encoder.
		/// </value>
		public IMimeEncoder Encoder {
			get; private set;
		}

		/// <summary>
		/// Gets the encoding.
		/// </summary>
		/// <value>
		/// The encoding.
		/// </value>
		public ContentEncoding Encoding {
			get { return Encoder.Encoding; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.EncoderFilter"/> class.
		/// </summary>
		/// <param name='encoder'>
		/// A specific encoder for the filter to use.
		/// </param>
		public EncoderFilter (IMimeEncoder encoder)
		{
			Encoder = encoder;
		}

		/// <summary>
		/// Create a filter that will encode using specified encoding.
		/// </summary>
		/// <param name='encoding'>
		/// The encoding to create a filter for.
		/// </param>
		public static IMimeFilter Create (ContentEncoding encoding)
		{
			switch (encoding) {
			case ContentEncoding.Base64: return new EncoderFilter (new Base64Encoder ());
			case ContentEncoding.QuotedPrintable: return new EncoderFilter (new QuotedPrintableEncoder ());
			case ContentEncoding.UuEncode: return new EncoderFilter (new UuEncoder ());
			default: return new PassThroughFilter ();
			}
		}

		protected override byte[] Filter (byte[] input, int startIndex, int length, out int outputIndex, out int outputLength, bool flush)
		{
			EnsureOutputSize (Encoder.EstimateOutputLength (length), false);

			if (flush)
				outputLength = Encoder.Flush (input, startIndex, length, output);
			else
				outputLength = Encoder.Encode (input, startIndex, length, output);

			outputIndex = 0;

			return output;
		}

		/// <summary>
		/// Resets the filter.
		/// </summary>
		public override void Reset ()
		{
			Encoder.Reset ();
			base.Reset ();
		}
	}
}

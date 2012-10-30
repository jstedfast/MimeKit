//
// DecoderFilter.cs
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
	public class DecoderFilter : MimeFilterBase
	{
		/// <summary>
		/// Gets the decoder used by this filter.
		/// </summary>
		/// <value>
		/// The decoder.
		/// </value>
		public IMimeDecoder Decoder {
			get; private set;
		}

		/// <summary>
		/// Gets the encoding.
		/// </summary>
		/// <value>
		/// The encoding.
		/// </value>
		public ContentEncoding Encoding {
			get { return Decoder.Encoding; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.DecoderFilter"/> class.
		/// </summary>
		/// <param name='decoder'>
		/// A specific decoder for the filter to use.
		/// </param>
		public DecoderFilter (IMimeDecoder decoder)
		{
			Decoder = decoder;
		}

		/// <summary>
		/// Create a filter that will decode the specified encoding.
		/// </summary>
		/// <param name='encoding'>
		/// The encoding to create a filter for.
		/// </param>
		public static IMimeFilter Create (ContentEncoding encoding)
		{
			switch (encoding) {
			case ContentEncoding.Base64: return new DecoderFilter (new Base64Decoder ());
			case ContentEncoding.QuotedPrintable: return new DecoderFilter (new QuotedPrintableDecoder ());
			case ContentEncoding.UUEncode: return new DecoderFilter (new UUDecoder ());
			default: return new PassThroughFilter ();
			}
		}

		protected override byte[] Filter (byte[] input, int startIndex, int length, out int outputIndex, out int outputLength, bool flush)
		{
			EnsureOutputSize (Decoder.EstimateOutputLength (length), false);

			outputLength = Decoder.Decode (input, startIndex, length, output);
			outputIndex = 0;

			return output;
		}

		/// <summary>
		/// Resets the filter.
		/// </summary>
		public override void Reset ()
		{
			Decoder.Reset ();
			base.Reset ();
		}
	}
}

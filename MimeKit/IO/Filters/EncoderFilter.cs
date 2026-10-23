//
// EncoderFilter.cs
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

using System;

using MimeKit.Utils;
using MimeKit.Encodings;

namespace MimeKit.IO.Filters {
	/// <summary>
	/// A filter for encoding MIME content.
	/// </summary>
	/// <remarks>
	/// Uses a <see cref="IMimeEncoder"/> to incrementally encode data.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\FilterExamples.cs" region="EncodeFile"/>
	/// </example>
	public class EncoderFilter : MimeFilterBase
	{
		internal bool HasOutput { get; private set; }

		/// <summary>
		/// Get the encoder used by this filter.
		/// </summary>
		/// <remarks>
		/// Gets the encoder used by this filter.
		/// </remarks>
		/// <value>The encoder.</value>
		public IMimeEncoder Encoder {
			get; private set;
		}

		/// <summary>
		/// Get the encoding.
		/// </summary>
		/// <remarks>
		/// Gets the encoding that the encoder supports.
		/// </remarks>
		/// <value>The encoding.</value>
		public ContentEncoding Encoding {
			get { return Encoder.Encoding; }
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="EncoderFilter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="IMimeFilter"/> using the specified encoder.
		/// </remarks>
		/// <param name="encoder">A specific encoder for the filter to use.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="encoder"/> is <see langword="null"/>.
		/// </exception>
		public EncoderFilter (IMimeEncoder encoder)
		{
			if (encoder is null)
				throw new ArgumentNullException (nameof (encoder));

			Encoder = encoder;
		}

		/// <summary>
		/// Create a filter that will encode using specified encoding.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="IMimeFilter"/> for the specified encoding using CRLF line endings.
		/// </remarks>
		/// <returns>A new encoder filter.</returns>
		/// <param name="encoding">The encoding to create a filter for.</param>
		/// <param name="maxLineLength">The maximum number of octets allowed per line (not counting the CRLF). Must be between <c>60</c> and <c>998</c> (inclusive).</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="maxLineLength"/> is not between <c>60</c> and <c>998</c> (inclusive).
		/// </exception>
		public static IMimeFilter Create (ContentEncoding encoding, int maxLineLength = 78)
		{
			return Create (encoding, NewLineFormat.Dos, maxLineLength);
		}

		/// <summary>
		/// Create a filter that will encode using the specified encoding and line endings.
		/// </summary>
		/// <remarks>
		/// Creates an encoder filter for base64, quoted-printable or uuencode. Other encodings pass through unchanged.
		/// </remarks>
		/// <returns>A new encoder filter.</returns>
		/// <param name="encoding">The encoding to create a filter for.</param>
		/// <param name="newLineFormat">The line ending format.</param>
		/// <param name="maxLineLength">The maximum line length, excluding line endings, between 60 and 998 (inclusive).</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="newLineFormat"/> is not <see cref="NewLineFormat.Dos"/> or <see cref="NewLineFormat.Unix"/>,
		/// or <paramref name="maxLineLength"/> is out of range for the selected encoder.
		/// </exception>
		public static IMimeFilter Create (ContentEncoding encoding, NewLineFormat newLineFormat, int maxLineLength = 78)
		{
			if (newLineFormat != NewLineFormat.Dos && newLineFormat != NewLineFormat.Unix)
				throw new ArgumentOutOfRangeException (nameof (newLineFormat));

			switch (encoding) {
			case ContentEncoding.Base64: return new EncoderFilter (new Base64Encoder (newLineFormat, maxLineLength));
			case ContentEncoding.QuotedPrintable: return new EncoderFilter (new QuotedPrintableEncoder (newLineFormat, maxLineLength));
			case ContentEncoding.UUEncode: return new EncoderFilter (new UUEncoder (newLineFormat));
			default: return new PassThroughFilter ();
			}
		}

		/// <summary>
		/// Create a filter that will encode using specified encoding.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="EncoderFilter"/> for the specified encoding using CRLF line endings.
		/// </remarks>
		/// <returns>A new encoder filter.</returns>
		/// <param name="name">The name of the encoding to create a filter for.</param>
		/// <param name="maxLineLength">The maximum number of octets allowed per line (not counting the CRLF). Must be between <c>60</c> and <c>998</c> (inclusive).</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="maxLineLength"/> is not between <c>60</c> and <c>998</c> (inclusive).
		/// </exception>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="name"/> is <see langword="null"/>.
		/// </exception>
		public static IMimeFilter Create (string name, int maxLineLength = 78)
		{
			return Create (name, NewLineFormat.Dos, maxLineLength);
		}

		/// <summary>
		/// Create a filter that will encode using the specified encoding and line endings.
		/// </summary>
		/// <remarks>
		/// Creates an encoder filter for base64, quoted-printable or uuencode. Other encodings pass through unchanged.
		/// </remarks>
		/// <returns>A new encoder filter.</returns>
		/// <param name="name">The name of the encoding to create a filter for.</param>
		/// <param name="newLineFormat">The line ending format.</param>
		/// <param name="maxLineLength">The maximum line length, excluding line endings, between 60 and 998 (inclusive).</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="name"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="newLineFormat"/> is not <see cref="NewLineFormat.Dos"/> or <see cref="NewLineFormat.Unix"/>,
		/// or <paramref name="maxLineLength"/> is out of range for the selected encoder.
		/// </exception>
		public static IMimeFilter Create (string name, NewLineFormat newLineFormat, int maxLineLength = 78)
		{
			if (name is null)
				throw new ArgumentNullException (nameof (name));

			if (!MimeUtils.TryParse (name, out ContentEncoding encoding))
				encoding = ContentEncoding.Unspecified;

			return Create (encoding, newLineFormat, maxLineLength);
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
		/// <param name="flush">If set to <see langword="true" />, all internally buffered data should be flushed to the output buffer.</param>
		protected override byte[] Filter (byte[] input, int startIndex, int length, out int outputIndex, out int outputLength, bool flush)
		{
			EnsureOutputSize (Encoder.EstimateOutputLength (length), false);

			if (flush)
				outputLength = Encoder.Flush (input, startIndex, length, OutputBuffer);
			else
				outputLength = Encoder.Encode (input, startIndex, length, OutputBuffer);

			outputIndex = 0;
			HasOutput |= outputLength > 0;

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
			HasOutput = false;
			Encoder.Reset ();
			base.Reset ();
		}
	}
}

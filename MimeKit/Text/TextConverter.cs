//
// TextConverter.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (www.xamarin.com)
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
using System.IO;

#if PORTABLE
using Encoding = Portable.Text.Encoding;
using Encoder = Portable.Text.Encoder;
using Decoder = Portable.Text.Decoder;
#else
using Encoding = System.Text.Encoding;
using Encoder = System.Text.Encoder;
using Decoder = System.Text.Decoder;
#endif

using MimeKit.IO;
using MimeKit.IO.Filters;

namespace MimeKit.Text {
	/// <summary>
	/// An abstract class for converting text from one format to another.
	/// </summary>
	/// <remarks>
	/// An abstract class for converting text from one format to another.
	/// </remarks>
	public abstract class TextConverter : IMimeFilter
	{
		readonly Encoder encoder;
		readonly Decoder decoder;
		byte[] output;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Text.TextConverter"/> class.
		/// </summary>
		/// <remarks>
		/// Initializes a new instance of the <see cref="MimeKit.Text.TextConverter"/> class.
		/// </remarks>
		protected TextConverter ()
		{
			encoder = Encoding.UTF8.GetEncoder ();
			decoder = Encoding.UTF8.GetDecoder ();
		}

		/// <summary>
		/// Gets the input format.
		/// </summary>
		/// <remarks>
		/// Gets the input format.
		/// </remarks>
		/// <value>The input format.</value>
		public abstract TextFormat InputFormat {
			get;
		}

		/// <summary>
		/// Gets the output format.
		/// </summary>
		/// <remarks>
		/// Gets the output format.
		/// </remarks>
		/// <value>The output format.</value>
		public abstract TextFormat OutputFormat {
			get;
		}

		/// <summary>
		/// Converts the contents of <paramref name="source"/> from the <see cref="InputFormat"/> to the
		/// <see cref="OutputFormat"/> and writes the resulting text to <paramref name="destination"/>.
		/// </summary>
		/// <remarks>
		/// Converts the contents of <paramref name="source"/> from the <see cref="InputFormat"/> to the
		/// <see cref="OutputFormat"/> and writes the resulting text to <paramref name="destination"/>.
		/// </remarks>
		/// <param name="source">The source stream.</param>
		/// <param name="destination">The destination stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="source"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="destination"/> is <c>null</c>.</para>
		/// </exception>
		public void Convert (Stream source, Stream destination)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			if (destination == null)
				throw new ArgumentNullException ("destination");

			using (var filtered = new FilteredStream (destination)) {
				filtered.Add (this);
				source.CopyTo (filtered, 4096);
				filtered.Flush ();
			}
		}

		/// <summary>
		/// Converts the contents of <paramref name="source"/> from the <see cref="InputFormat"/> to the
		/// <see cref="OutputFormat"/> and uses the <paramref name="writer"/> to write the resulting text.
		/// </summary>
		/// <remarks>
		/// Converts the contents of <paramref name="source"/> from the <see cref="InputFormat"/> to the
		/// <see cref="OutputFormat"/> and uses the <paramref name="writer"/> to write the resulting text.
		/// </remarks>
		/// <param name="source">The source stream.</param>
		/// <param name="writer">The text writer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="source"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="writer"/> is <c>null</c>.</para>
		/// </exception>
		public void Convert (Stream source, TextWriter writer)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			if (writer == null)
				throw new ArgumentNullException ("writer");

			var buffer = new byte[4096];
			var text = new char[2048];
			int nread, count, n;
			char[] converted;

			while ((nread = source.Read (buffer, 0, buffer.Length)) > 0) {
				int left = nread;
				int index = 0;
				bool decoded;

				do {
					decoder.Convert (buffer, index, left, text, 0, text.Length, false, out n, out count, out decoded);
					index += n;
					left -= n;

					converted = Convert (text, 0, count, false, out n, out count);
					writer.Write (converted, n, count);
				} while (!decoded);
			}

			converted = Convert (text, 0, 0, true, out n, out count);

			if (count > 0)
				writer.Write (converted, n, count);

			writer.Flush ();
		}

		/// <summary>
		/// Converts the contents of <paramref name="reader"/> from the <see cref="InputFormat"/> to the
		/// <see cref="OutputFormat"/> and writes the resulting text to <paramref name="destination"/>.
		/// </summary>
		/// <remarks>
		/// Converts the contents of <paramref name="reader"/> from the <see cref="InputFormat"/> to the
		/// <see cref="OutputFormat"/> and writes the resulting text to <paramref name="destination"/>.
		/// </remarks>
		/// <param name="reader">The text reader.</param>
		/// <param name="destination">The destination stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="reader"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="destination"/> is <c>null</c>.</para>
		/// </exception>
		public void Convert (TextReader reader, Stream destination)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			if (destination == null)
				throw new ArgumentNullException ("destination");

			var text = new char[2048];
			int count = 0;
			int nread;

			while ((nread = reader.Read (text, 0, text.Length)) > 0) {
				WriteToOutputBuffer (text, 0, nread, false, ref count);
				destination.Write (output, 0, count);
				count = 0;
			}

			WriteToOutputBuffer (text, 0, 0, true, ref count);

			if (count > 0)
				destination.Write (output, 0, count);

			destination.Flush ();
		}

		/// <summary>
		/// Converts the contents of <paramref name="reader"/> from the <see cref="InputFormat"/> to the
		/// <see cref="OutputFormat"/> and uses the <paramref name="writer"/> to write the resulting text.
		/// </summary>
		/// <remarks>
		/// Converts the contents of <paramref name="reader"/> from the <see cref="InputFormat"/> to the
		/// <see cref="OutputFormat"/> and uses the <paramref name="writer"/> to write the resulting text.
		/// </remarks>
		/// <param name="reader">The text reader.</param>
		/// <param name="writer">The text writer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="reader"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="writer"/> is <c>null</c>.</para>
		/// </exception>
		public void Convert (TextReader reader, TextWriter writer)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			if (writer == null)
				throw new ArgumentNullException ("writer");

			var text = new char[2048];
			char[] converted;
			int index, count;
			int nread;

			while ((nread = reader.Read (text, 0, text.Length)) > 0) {
				converted = Convert (text, 0, nread, false, out index, out count);
				writer.Write (converted, index, count);
			}

			converted = Convert (text, 0, 0, true, out index, out count);

			if (count > 0)
				writer.Write (converted, index, count);

			writer.Flush ();
		}

		protected abstract char[] Convert (char[] text, int startIndex, int length, bool flush, out int outputIndex, out int outputLength);

		static int GetIdealBufferSize (int need)
		{
			return (need + 63) & ~63;
		}

		void EnsureOutputSize (int size, bool keep)
		{
			if (size == 0)
				return;

			int outputSize = output != null ? output.Length : 0;

			if (outputSize >= size)
				return;

			if (keep)
				Array.Resize<byte> (ref output, GetIdealBufferSize (size));
			else
				output = new byte[GetIdealBufferSize (size)];
		}

		void WriteToOutputBuffer (char[] text, int startIndex, int length, bool flush, ref int outputIndex)
		{
			char[] converted;
			int index, left;
			bool encoded;
			int nwritten;
			int nread;

			converted = Convert (text, startIndex, length, flush, out index, out left);

			// encode *all* converted characters into the output buffer
			do {
				EnsureOutputSize (outputIndex + Encoding.UTF8.GetMaxByteCount (left) + 4, outputIndex > 0);
				int outputLeft = output.Length - outputIndex;

				encoder.Convert (converted, index, left, output, outputIndex, outputLeft, flush, out nread, out nwritten, out encoded);
				outputIndex += nwritten;
				index += nread;
				left -= nread;
			} while (!encoded);
		}

		byte[] Filter (byte[] input, int startIndex, int length, bool flush, out int outputIndex, out int outputLength)
		{
			var text = new char[2048]; // FIXME: store a text buffer on the class
			int inputIndex = startIndex;
			int inputLeft = length;
			int charCount, nread;
			int offset = 0;
			bool decoded;

			do {
				decoder.Convert (input, inputIndex, inputLeft, text, 0, text.Length, flush, out nread, out charCount, out decoded);
				inputIndex += nread;
				inputLeft -= nread;

				WriteToOutputBuffer (text, 0, charCount, flush && decoded, ref offset);
			} while (!decoded);

			outputLength = offset;
			outputIndex = 0;

			return output;
		}

		/// <summary>
		/// Resets the text converter.
		/// </summary>
		/// <remarks>
		/// Resets the text converter.
		/// </remarks>
		/// <remarks>Resets the text converter.</remarks>
		public virtual void Reset ()
		{
			encoder.Reset ();
			decoder.Reset ();
		}

		#region IMimeFilter implementation

		static void ValidateArguments (byte[] input, int startIndex, int length)
		{
			if (input == null)
				throw new ArgumentNullException ("input");

			if (startIndex < 0 || startIndex > input.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (length < 0 || length > (input.Length - startIndex))
				throw new ArgumentOutOfRangeException ("length");
		}

		byte[] IMimeFilter.Filter (byte[] input, int startIndex, int length, out int outputIndex, out int outputLength)
		{
			ValidateArguments (input, startIndex, length);

			return Filter (input, startIndex, length, false, out outputIndex, out outputLength);
		}

		byte[] IMimeFilter.Flush (byte[] input, int startIndex, int length, out int outputIndex, out int outputLength)
		{
			ValidateArguments (input, startIndex, length);

			return Filter (input, startIndex, length, true, out outputIndex, out outputLength);
		}

		void IMimeFilter.Reset ()
		{
			Reset ();
		}

		#endregion
	}
}

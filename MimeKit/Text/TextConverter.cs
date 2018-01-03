//
// TextConverter.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2018 Xamarin Inc. (www.xamarin.com)
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
using System.Text;
using System.Collections.Generic;

#if PORTABLE
using Encoding = Portable.Text.Encoding;
using Encoder = Portable.Text.Encoder;
using Decoder = Portable.Text.Decoder;
#else
using Encoding = System.Text.Encoding;
using Encoder = System.Text.Encoder;
using Decoder = System.Text.Decoder;
#endif

namespace MimeKit.Text {
	/// <summary>
	/// An abstract class for converting text from one format to another.
	/// </summary>
	/// <remarks>
	/// An abstract class for converting text from one format to another.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
	/// </example>
	public abstract class TextConverter
	{
		internal readonly static List<UrlPattern> UrlPatterns;
		Encoding outputEncoding = Encoding.UTF8;
		Encoding inputEncoding = Encoding.UTF8;
		int outputStreamBufferSize = 4096;
		int inputStreamBufferSize = 4096;

		static TextConverter ()
		{
			UrlPatterns = new List<UrlPattern> (new [] {
				new UrlPattern (UrlPatternType.Addrspec, "@",         "mailto:"),
				new UrlPattern (UrlPatternType.MailTo,   "mailto:",   ""),
				new UrlPattern (UrlPatternType.Web,      "www.",      "http://"),
				new UrlPattern (UrlPatternType.Web,      "ftp.",      "ftp://"),
				new UrlPattern (UrlPatternType.File,     "file://",   ""),
				new UrlPattern (UrlPatternType.Web,      "ftp://",    ""),
				new UrlPattern (UrlPatternType.Web,      "sftp://",   ""),
				new UrlPattern (UrlPatternType.Web,      "http://",   ""),
				new UrlPattern (UrlPatternType.Web,      "https://",  ""),
				new UrlPattern (UrlPatternType.Web,      "news://",   ""),
				new UrlPattern (UrlPatternType.Web,      "nntp://",   ""),
				new UrlPattern (UrlPatternType.Web,      "telnet://", ""),
				new UrlPattern (UrlPatternType.Web,      "webcal://", ""),
				new UrlPattern (UrlPatternType.Web,      "callto:",   ""),
				new UrlPattern (UrlPatternType.Web,      "h323:",     ""),
				new UrlPattern (UrlPatternType.Web,      "sip:",      "")
			});
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Text.TextConverter"/> class.
		/// </summary>
		/// <remarks>
		/// Initializes a new instance of the <see cref="MimeKit.Text.TextConverter"/> class.
		/// </remarks>
		protected TextConverter ()
		{
		}

		/// <summary>
		/// Get or set whether the encoding of the input is detected from the byte order mark or
		/// determined by the <see cref="InputEncoding"/> property.
		/// </summary>
		/// <remarks>
		/// Gets or sets whether the encoding of the input is detected from the byte order mark or
		/// determined by the <see cref="InputEncoding"/> property.
		/// </remarks>
		/// <value><c>true</c> if detect encoding from byte order mark; otherwise, <c>false</c>.</value>
		public bool DetectEncodingFromByteOrderMark {
			get; set;
		}

		/// <summary>
		/// Get or set the input encoding.
		/// </summary>
		/// <remarks>
		/// Gets or sets the input encoding.
		/// </remarks>
		/// <value>The input encoding.</value>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		public Encoding InputEncoding {
			get { return inputEncoding; }
			set {
				if (value == null)
					throw new ArgumentNullException (nameof (value));

				inputEncoding = value;
			}
		}

		/// <summary>
		/// Get the input format.
		/// </summary>
		/// <remarks>
		/// Gets the input format.
		/// </remarks>
		/// <value>The input format.</value>
		public abstract TextFormat InputFormat {
			get;
		}

		/// <summary>
		/// Get or set the output encoding.
		/// </summary>
		/// <remarks>
		/// Gets or sets the output encoding.
		/// </remarks>
		/// <value>The output encoding.</value>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		public Encoding OutputEncoding {
			get { return outputEncoding; }
			set {
				if (value == null)
					throw new ArgumentNullException (nameof (value));

				outputEncoding = value;
			}
		}

		/// <summary>
		/// Get the output format.
		/// </summary>
		/// <remarks>
		/// Gets the output format.
		/// </remarks>
		/// <value>The output format.</value>
		public abstract TextFormat OutputFormat {
			get;
		}

		/// <summary>
		/// Get or set the size of the input stream buffer.
		/// </summary>
		/// <remarks>
		/// Gets or sets the size of the input stream buffer.
		/// </remarks>
		/// <value>The size of the input stream buffer.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is less than or equal to <c>0</c>.
		/// </exception>
		public int InputStreamBufferSize {
			get { return inputStreamBufferSize; }
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException (nameof (value));

				inputStreamBufferSize = value;
			}
		}

		/// <summary>
		/// Get or set the size of the output stream buffer.
		/// </summary>
		/// <remarks>
		/// Gets or sets the size of the output stream buffer.
		/// </remarks>
		/// <value>The size of the output stream buffer.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is less than or equal to <c>0</c>.
		/// </exception>
		public int OutputStreamBufferSize {
			get { return outputStreamBufferSize; }
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException (nameof (value));

				outputStreamBufferSize = value;
			}
		}

		TextReader CreateReader (Stream stream)
		{
			return new StreamReader (stream, InputEncoding, DetectEncodingFromByteOrderMark, InputStreamBufferSize);
		}

		TextWriter CreateWriter (Stream stream)
		{
			return new StreamWriter (stream, OutputEncoding, OutputStreamBufferSize);
		}

		/// <summary>
		/// Convert the contents of <paramref name="source"/> from the <see cref="InputFormat"/> to the
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
		public virtual void Convert (Stream source, Stream destination)
		{
			if (source == null)
				throw new ArgumentNullException (nameof (source));

			if (destination == null)
				throw new ArgumentNullException (nameof (destination));

			Convert (CreateReader (source), CreateWriter (destination));
		}

		/// <summary>
		/// Convert the contents of <paramref name="source"/> from the <see cref="InputFormat"/> to the
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
		public virtual void Convert (Stream source, TextWriter writer)
		{
			if (source == null)
				throw new ArgumentNullException (nameof (source));

			if (writer == null)
				throw new ArgumentNullException (nameof (writer));

			Convert (CreateReader (source), writer);
		}

		/// <summary>
		/// Convert the contents of <paramref name="reader"/> from the <see cref="InputFormat"/> to the
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
		public virtual void Convert (TextReader reader, Stream destination)
		{
			if (reader == null)
				throw new ArgumentNullException (nameof (reader));

			if (destination == null)
				throw new ArgumentNullException (nameof (destination));

			Convert (reader, CreateWriter (destination));
		}

		/// <summary>
		/// Convert the contents of <paramref name="reader"/> from the <see cref="InputFormat"/> to the
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
		public abstract void Convert (TextReader reader, TextWriter writer);

		/// <summary>
		/// Convert text from the <see cref="InputFormat"/> to the <see cref="OutputFormat"/>.
		/// </summary>
		/// <remarks>
		/// Converts text from the <see cref="InputFormat"/> to the <see cref="OutputFormat"/>.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <returns>The converted text.</returns>
		/// <param name="text">The text to convert.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public virtual string Convert (string text)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			using (var reader = new StringReader (text)) {
				var output = new StringBuilder ();

				using (var writer = new StringWriter (output))
					Convert (reader, writer);

				return output.ToString ();
			}
		}
	}
}

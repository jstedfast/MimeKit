//
// TnefMessageBody.cs
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
using System.IO;
using System.Text;

using MimeKit.IO;

namespace MimeKit.Tnef {
	/// <summary>
	/// A body of a TNEF message.
	/// </summary>
	/// <remarks>
	/// <para>A <see cref="TnefMessageBody"/> holds the content of one of the bodies of a <see cref="TnefMessage"/>.</para>
	/// <para>The content is buffered in memory when the message is loaded, so it may be read any number of times.</para>
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\TnefExamples.cs" region="ReadBodies"/>
	/// </example>
	public sealed class TnefMessageBody : IDisposable
	{
		readonly MemoryBlockStream content;
		Encoding fallbackEncoding;
		bool disposed;

		internal TnefMessageBody (TnefMessageBodyFormat format, TnefPropertyTag tag, MemoryBlockStream content, Encoding? encoding, Encoding fallbackEncoding)
		{
			this.fallbackEncoding = fallbackEncoding;
			this.content = content;
			Encoding = encoding;
			Format = format;
			Tag = tag;
		}

		/// <summary>
		/// Get the format of the body.
		/// </summary>
		/// <remarks>
		/// Gets the format of the body.
		/// </remarks>
		/// <value>The format.</value>
		public TnefMessageBodyFormat Format {
			get;
		}

		/// <summary>
		/// Get the tag of the property that the body was read from.
		/// </summary>
		/// <remarks>
		/// <para>Gets the tag of the property that the body was read from, such as <see cref="TnefPropertyTag.BodyW"/>,
		/// <see cref="TnefPropertyTag.BodyHtmlB"/> or <see cref="TnefPropertyTag.RtfCompressed"/>.</para>
		/// <para>A body that was read from the legacy <see cref="TnefAttributeTag.Body"/> attribute has a tag of
		/// <see cref="TnefPropertyTag.BodyA"/>.</para>
		/// </remarks>
		/// <value>The property tag.</value>
		public TnefPropertyTag Tag {
			get;
		}

		/// <summary>
		/// Get the text encoding of the content.
		/// </summary>
		/// <remarks>
		/// <para>Gets the text encoding of the raw content returned by <see cref="OpenRead"/>.</para>
		/// <para>The encoding of a <see cref="TnefPropertyType.Unicode"/> body is UTF-16 (little endian), and the encoding
		/// of a <see cref="TnefPropertyType.String8"/> body is the encoding of the message's codepage.</para>
		/// <para>The encoding of a <see cref="TnefPropertyType.Binary"/> HTML body is the encoding of the message's
		/// <see cref="TnefPropertyId.InternetCodepage"/>, if it has one; otherwise, it is <see langword="null"/> and the
		/// encoding may only be determined from the HTML content itself. The encoding of a compressed RTF body is always
		/// <see langword="null"/>.</para>
		/// </remarks>
		/// <value>The text encoding, or <see langword="null"/> if it is not known.</value>
		public Encoding? Encoding {
			get; internal set;
		}

		// Updates the encodings of the body once the message's codepage is known.
		internal void SetMessageEncoding (Encoding encoding)
		{
			if (Tag.ValueTnefType == TnefPropertyType.String8)
				Encoding = encoding;

			fallbackEncoding = encoding;
		}

		/// <summary>
		/// Get the length of the content.
		/// </summary>
		/// <remarks>
		/// <para>Gets the length of the raw content, in bytes.</para>
		/// <para>Trailing null characters are removed from text bodies when the message is loaded.</para>
		/// </remarks>
		/// <value>The length of the content.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The body has been disposed.
		/// </exception>
		public long Length {
			get {
				CheckDisposed ();
				return content.Length;
			}
		}

		void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (nameof (TnefMessageBody));
		}

		/// <summary>
		/// Open a stream for reading the raw content.
		/// </summary>
		/// <remarks>
		/// <para>Opens a stream for reading the raw content of the body, exactly as it was stored in the TNEF stream.
		/// The content of a <see cref="TnefMessageBodyFormat.CompressedRtf"/> body is compressed; use
		/// <see cref="OpenDecodedRead"/> to read the decompressed RTF.</para>
		/// <para>Each call returns a new, independent stream. The stream must be disposed by the caller, and must not be
		/// used once the body has been disposed.</para>
		/// </remarks>
		/// <returns>The content stream.</returns>
		/// <exception cref="System.ObjectDisposedException">
		/// The body has been disposed.
		/// </exception>
		public Stream OpenRead ()
		{
			CheckDisposed ();

			return new BoundStream (content, 0, content.Length, true);
		}

		/// <summary>
		/// Open a stream for reading the decoded content.
		/// </summary>
		/// <remarks>
		/// <para>Opens a stream for reading the decoded content of the body. For a
		/// <see cref="TnefMessageBodyFormat.CompressedRtf"/> body, this is the decompressed RTF. For any other body, it
		/// is the same as the raw content returned by <see cref="OpenRead"/>.</para>
		/// <para>A <see cref="TnefMessageBodyFormat.CompressedRtf"/> body whose compression type is neither
		/// <see cref="RtfCompressionMode.Compressed"/> nor <see cref="RtfCompressionMode.Uncompressed"/> cannot be
		/// decoded, so the stream is empty (see <see cref="RtfCompressedToRtf.CompressionMode"/>).</para>
		/// <para>Each call returns a new, independent stream. The stream must be disposed by the caller, and must not be
		/// used once the body has been disposed.</para>
		/// </remarks>
		/// <returns>The decoded content stream.</returns>
		/// <exception cref="System.ObjectDisposedException">
		/// The body has been disposed.
		/// </exception>
		public Stream OpenDecodedRead ()
		{
			var stream = OpenRead ();

			if (Format != TnefMessageBodyFormat.CompressedRtf)
				return stream;

			var filtered = new FilteredStream (stream);
			filtered.Add (new RtfCompressedToRtf ());

			return filtered;
		}

		/// <summary>
		/// Get the content as text.
		/// </summary>
		/// <remarks>
		/// <para>Gets the decoded content of the body as text, using <see cref="Encoding"/>.</para>
		/// <para>If <see cref="Encoding"/> is <see langword="null"/>, the encoding of the message's codepage is used
		/// instead.</para>
		/// </remarks>
		/// <returns>The text.</returns>
		/// <exception cref="System.ObjectDisposedException">
		/// The body has been disposed.
		/// </exception>
		public string GetText ()
		{
			using (var stream = OpenDecodedRead ())
			using (var reader = new StreamReader (stream, Encoding ?? fallbackEncoding, false))
				return reader.ReadToEnd ();
		}

		/// <summary>
		/// Release all resources used by the <see cref="TnefMessageBody"/> object.
		/// </summary>
		/// <remarks>
		/// Releases the buffered content of the body.
		/// </remarks>
		public void Dispose ()
		{
			if (disposed)
				return;

			disposed = true;
			content.Dispose ();
		}
	}
}

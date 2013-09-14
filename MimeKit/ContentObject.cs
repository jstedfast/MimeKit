//
// ContentObject.cs
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
using System.IO;

namespace MimeKit {
	public class ContentObject : IContentObject
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.ContentObject"/> class.
		/// </summary>
		/// <param name="stream">The content stream.</param>
		/// <param name="encoding">The stream encoding.</param>
		public ContentObject (Stream stream, ContentEncoding encoding)
		{
			Encoding = encoding;
			Stream = stream;
		}

		#region IContentObject implementation

		/// <summary>
		/// Gets or sets the content encoding.
		/// </summary>
		/// <value>The content encoding.</value>
		public ContentEncoding Encoding {
			get; set;
		}

		/// <summary>
		/// Gets or sets the content stream.
		/// </summary>
		/// <value>The content stream.</value>
		public Stream Stream {
			get; set;
		}

		/// <summary>
		/// Writes the raw content stream to to another stream.
		/// </summary>
		/// <param name="stream">The output stream.</param>
		public void WriteTo (Stream stream)
		{
			byte[] buf = new byte[4096];
			int nread;

			Stream.Seek (0, SeekOrigin.Begin);

			do {
				if ((nread = Stream.Read (buf, 0, buf.Length)) <= 0)
					break;

				stream.Write (buf, 0, nread);
			} while (true);

			Stream.Seek (0, SeekOrigin.Begin);
		}

		/// <summary>
		/// Decodes the content stream into another stream.
		/// </summary>
		/// <param name="stream">The output stream.</param>
		public void DecodeTo (Stream stream)
		{
			using (var filtered = new FilteredStream (stream)) {
				filtered.Add (DecoderFilter.Create (Encoding));
				WriteTo (filtered);
				filtered.Flush ();
			}
		}

		#endregion
	}
}

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
		public ContentObject (Stream content, ContentEncoding encoding)
		{
			ContentEncoding = encoding;
			Content = content;
		}

		#region IContentObject implementation

		public ContentEncoding ContentEncoding {
			get; set;
		}

		public Stream Content {
			get; set;
		}

		public void WriteTo (Stream stream)
		{
			byte[] buf = new byte[4096];
			int nread;

			Content.Seek (0, SeekOrigin.Begin);

			do {
				if ((nread = Content.Read (buf, 0, buf.Length)) <= 0)
					break;

				stream.Write (buf, 0, nread);
			} while (true);

			Content.Seek (0, SeekOrigin.Begin);
		}

		public void DecodeTo (Stream stream)
		{
			using (var filtered = new FilteredStream (stream)) {
				filtered.Add (DecoderFilter.Create (ContentEncoding));
				WriteTo (filtered);
				filtered.Flush ();
			}
		}

		#endregion
	}
}

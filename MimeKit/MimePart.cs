//
// MimePart.cs
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
using System.Text;
using System.Collections.Generic;

namespace MimeKit {
	public class MimePart : MimeEntity
	{
		static readonly string[] ContentTransferEncodings = new string[] {
			null, "7bit", "8bit", "binary", "base64", "quoted-printable", "x-uuencode"
		};
		ContentEncoding encoding;

		internal MimePart (ParserOptions options, ContentType type, IEnumerable<Header> headers, bool toplevel) : base (options, type, headers, toplevel)
		{
		}

		public MimePart (string mediaType, string mediaSubtype) : base (mediaType, mediaSubtype)
		{
		}

		public MimePart () : base ("application", "octet-stream")
		{
		}

		public ContentEncoding ContentTransferEncoding {
			get { return encoding; }
			set {
				if (encoding == value)
					return;

				var text = ContentTransferEncodings[(int) value];

				encoding = value;

				if (IsInitializing)
					return;

				Headers.Changed -= HeadersChanged;
				if (text == null)
					Headers.RemoveAll ("Content-Transfer-Encoding");
				else
					Headers["Content-Transfer-Encoding"] = text;
				Headers.Changed += HeadersChanged;
			}
		}

		public string FileName {
			get {
				string filename = null;

				if (ContentDisposition != null)
					filename = ContentDisposition.Parameters["filename"];

				if (string.IsNullOrWhiteSpace (filename))
					filename = ContentType.Parameters["name"];

				if (string.IsNullOrWhiteSpace (filename))
					return null;

				return filename.Trim ();
			}
		}

		public ContentObject ContentObject {
			get; set;
		}

		public override void WriteTo (Stream stream)
		{
			base.WriteTo (stream);

			if (ContentObject == null)
				return;

			if (ContentObject.Encoding != encoding) {
				if (encoding == ContentEncoding.UUEncode) {
					var begin = string.Format ("begin 0644 {0}\n", FileName ?? "unknown");
					var buffer = Encoding.UTF8.GetBytes (begin);
					stream.Write (buffer, 0, buffer.Length);
				}

				// transcode the content into the desired Content-Transfer-Encoding
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (EncoderFilter.Create (encoding));
					ContentObject.DecodeTo (filtered);
					filtered.Flush ();
				}

				if (encoding == ContentEncoding.UUEncode) {
					var buffer = Encoding.ASCII.GetBytes ("end\n");
					stream.Write (buffer, 0, buffer.Length);
				}
			} else {
				ContentObject.WriteTo (stream);
			}
		}

		protected override void OnHeadersChanged (HeaderListChangedAction action, ContentHeader type, Header header)
		{
			string text;

			base.OnHeadersChanged (action, type, header);

			switch (action) {
			case HeaderListChangedAction.Added:
			case HeaderListChangedAction.Changed:
				switch (type) {
				case ContentHeader.ContentTransferEncoding:
					text = header.Value.Trim ().ToLowerInvariant ();
					encoding = ContentEncoding.Default;
					for (int i = 0; i < ContentTransferEncodings.Length; i++) {
						if (ContentTransferEncodings[i] == text) {
							encoding = (ContentEncoding) i;
							break;
						}
					}
					break;
				}
				break;
			case HeaderListChangedAction.Removed:
				switch (type) {
				case ContentHeader.ContentTransferEncoding:
					encoding = ContentEncoding.Default;
					break;
				}
				break;
			case HeaderListChangedAction.Cleared:
				encoding = ContentEncoding.Default;
				break;
			default:
				throw new ArgumentOutOfRangeException ();
			}
		}
	}
}

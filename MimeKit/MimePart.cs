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
using System.Security.Cryptography;

namespace MimeKit {
	public class MimePart : MimeEntity
	{
		static readonly string[] ContentTransferEncodings = new string[] {
			null, "7bit", "8bit", "binary", "base64", "quoted-printable", "x-uuencode"
		};
		ContentEncoding encoding;
		string content_md5;

		internal MimePart (ParserOptions options, ContentType type, IEnumerable<Header> headers, bool toplevel) : base (options, type, headers, toplevel)
		{
		}

		public MimePart (string mediaType, string mediaSubtype) : base (mediaType, mediaSubtype)
		{
		}

		public MimePart () : base ("application", "octet-stream")
		{
		}

		public string ContentMd5 {
			get { return content_md5; }
			set {
				if (content_md5 == value)
					return;

				if (value != null)
					content_md5 = value.Trim ();
				else
					content_md5 = null;

				if (value != null)
					SetHeader ("Content-Md5", content_md5);
				else
					RemoveHeader ("Content-Md5");
			}
		}

		public ContentEncoding ContentTransferEncoding {
			get { return encoding; }
			set {
				if (encoding == value)
					return;

				var text = ContentTransferEncodings[(int) value];

				encoding = value;

				if (text != null)
					SetHeader ("Content-Transfer-Encoding", text);
				else
					RemoveHeader ("Content-Transfer-Encoding");
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

		public string ComputeContentMd5 ()
		{
			if (ContentObject == null)
				throw new InvalidOperationException ("Cannot compute Md5 checksum without a ContentObject.");

			var stream = ContentObject.Stream;
			stream.Seek (0, SeekOrigin.Begin);

			byte[] checksum;

			using (var filtered = new FilteredStream (stream)) {
				if (ContentObject.Encoding > ContentEncoding.Binary)
					filtered.Add (DecoderFilter.Create (ContentObject.Encoding));

				if (ContentType.Matches ("text", "*"))
					filtered.Add (new Unix2DosFilter ());

				using (var md5 = MD5.Create ())
					checksum = md5.ComputeHash (filtered);
			}

			var base64 = new Base64Encoder (true);
			var digest = new byte[base64.EstimateOutputLength (checksum.Length)];
			int n = base64.Flush (checksum, 0, checksum.Length, digest);

			return Encoding.ASCII.GetString (digest, 0, n);
		}

		public bool VerifyContentMd5 ()
		{
			if (string.IsNullOrWhiteSpace (content_md5) || ContentObject == null)
				return false;

			return content_md5 == ComputeContentMd5 ();
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

		protected override void OnHeadersChanged (HeaderListChangedAction action, HeaderId id, Header header)
		{
			string text;

			base.OnHeadersChanged (action, id, header);

			switch (action) {
			case HeaderListChangedAction.Added:
			case HeaderListChangedAction.Changed:
				switch (id) {
				case HeaderId.ContentTransferEncoding:
					text = header.Value.Trim ().ToLowerInvariant ();
					encoding = ContentEncoding.Default;
					for (int i = 0; i < ContentTransferEncodings.Length; i++) {
						if (ContentTransferEncodings[i] == text) {
							encoding = (ContentEncoding) i;
							break;
						}
					}
					break;
				case HeaderId.ContentMd5:
					content_md5 = header.Value.Trim ();
					break;
				}
				break;
			case HeaderListChangedAction.Removed:
				switch (id) {
				case HeaderId.ContentTransferEncoding:
					encoding = ContentEncoding.Default;
					break;
				case HeaderId.ContentMd5:
					content_md5 = null;
					break;
				}
				break;
			case HeaderListChangedAction.Cleared:
				encoding = ContentEncoding.Default;
				content_md5 = null;
				break;
			default:
				throw new ArgumentOutOfRangeException ();
			}
		}
	}
}

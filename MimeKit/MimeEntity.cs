//
// MimeEntity.cs
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
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace MimeKit {
	public abstract class MimeEntity
	{
		internal static readonly byte[] NewLine = Encoding.ASCII.GetBytes (Environment.NewLine);
		static readonly StringComparer icase = StringComparer.OrdinalIgnoreCase;
		ContentDisposition disposition;
		string contentId;

		// Note: this ctor is only used by the parser...
		internal protected MimeEntity (ContentType type, IEnumerable<Header> headers, bool toplevel)
		{
			Headers = new HeaderList ();
			ContentType = type;

			ContentType.Changed += ContentTypeChanged;
			Headers.Changed += OnHeadersChanged;

			foreach (var header in headers) {
				if (toplevel && !header.Field.StartsWith ("Content-", StringComparison.OrdinalIgnoreCase))
					continue;

				Headers.Add (header);
			}
		}

		protected MimeEntity (string mediaType, string mediaSubtype)
		{
			ContentType = new ContentType (mediaType, mediaSubtype);
			Headers = new HeaderList ();

			ContentType.Changed += ContentTypeChanged;
			Headers.Changed += OnHeadersChanged;

			SerializeContentType ();
		}

		public HeaderList Headers {
			get; private set;
		}

		public ContentDisposition ContentDisposition {
			get { return disposition; }
			set {
				if (disposition == value)
					return;

				Headers.Changed -= OnHeadersChanged;

				if (disposition != null) {
					disposition.Changed -= ContentDispositionChanged;
					if (value == null)
						Headers.Remove ("Content-Disposition");
				}

				disposition = value;
				if (disposition != null) {
					disposition.Changed += ContentDispositionChanged;
					SerializeContentDisposition ();
				}

				Headers.Changed += OnHeadersChanged;
				OnChanged ();
			}
		}

		public ContentType ContentType {
			get; private set;
		}

		public string ContentId {
			get { return contentId; }
			set {
				if (contentId == value)
					return;

				Headers.Changed -= OnHeadersChanged;

				if (value != null) {
					value = value.Trim ();

					if (value != string.Empty)
						Headers["Content-Id"] = "<" + value + ">";
					else
						Headers["Content-Id"] = string.Empty;
				} else {
					Headers.Remove ("Content-Id");
				}

				Headers.Changed += OnHeadersChanged;
				contentId = value;
				OnChanged ();
			}
		}

		public virtual void WriteTo (Stream stream)
		{
			Headers.WriteTo (stream);

			stream.Write (NewLine, 0, NewLine.Length);
		}

		void SerializeContentDisposition ()
		{
			var text = disposition.Encode (Encoding.UTF8);
			var raw = Encoding.ASCII.GetBytes (text);

			Header header;
			if (!Headers.TryGetHeader ("Content-Disposition", out header))
				Headers.Add (new Header ("Content-Disposition", raw));
			else
				header.RawValue = raw;
		}

		void SerializeContentType ()
		{
			var text = ContentType.Encode (Encoding.UTF8);
			var raw = Encoding.ASCII.GetBytes (text);

			Header header;
			if (!Headers.TryGetHeader ("Content-Type", out header))
				Headers.Add (new Header ("Content-Type", raw));
			else
				header.RawValue = raw;
		}

		void ContentDispositionChanged (object sender, EventArgs e)
		{
			Headers.Changed -= OnHeadersChanged;
			SerializeContentDisposition ();
			Headers.Changed += OnHeadersChanged;

			OnChanged ();
		}

		void ContentTypeChanged (object sender, EventArgs e)
		{
			Headers.Changed -= OnHeadersChanged;
			SerializeContentType ();
			Headers.Changed += OnHeadersChanged;

			OnChanged ();
		}

		void UpdatePropertyValue (Header header, bool initializing)
		{
			if (icase.Compare (header.Field, "Content-Disposition") == 0) {
				ContentDisposition = ContentDisposition.Parse (header.RawValue);
			} else if (icase.Compare (header.Field, "Content-Type") == 0) {
				if (initializing)
					return;

				ContentType = ContentType.Parse (header.RawValue);
			} else if (icase.Compare (header.Field, "Content-Id") == 0) {
				// FIXME: extract only the value between <>'s
				contentId = header.Value;
			}
		}

		protected virtual void OnHeadersChanged (object sender, HeaderListChangedEventArgs e)
		{
			switch (e.Action) {
			case HeaderListChangedAction.Added:
			case HeaderListChangedAction.Changed:
				UpdatePropertyValue (e.Header, false);
				break;
			case HeaderListChangedAction.Removed:
				if (icase.Compare (e.Header.Field, "Content-Disposition") == 0) {
					ContentDisposition = null;
				} else if (icase.Compare (e.Header.Field, "Content-Id") == 0) {
					contentId = null;
				}
				break;
			case HeaderListChangedAction.Cleared:
				ContentDisposition = null;
				contentId = null;
				break;
			default:
				throw new ArgumentOutOfRangeException ();
			}

			OnChanged ();
		}

		public event EventHandler Changed;

		protected void OnChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}

		internal static MimeEntity Create (ContentType type, IEnumerable<Header> headers, bool toplevel)
		{
			if (headers == null)
				throw new ArgumentNullException ("headers");

			if (type == null)
				throw new ArgumentNullException ("type");

			if (icase.Compare (type.MediaType, "message") == 0) {
				if (icase.Compare (type.MediaSubtype, "partial") == 0)
					return new MessagePartial (type, headers, toplevel);

				return new MessagePart (type, headers, toplevel);
			}

			if (icase.Compare (type.MediaType, "multipart") == 0) {
				//if (icase.Compare (type.Subtype, "encrypted") == 0)
				//	return new MultipartEncrypted (headers, type);

				//if (icase.Compare (type.Subtype, "signed") == 0)
				//	return new MultipartSigned (headers, type);

				return new Multipart (type, headers, toplevel);
			}

			//if (type.Matches ("text", "*"))
			//	return new TextPart (headers, type);

			return new MimePart (type, headers, toplevel);
		}
	}
}

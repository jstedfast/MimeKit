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

namespace MimeKit {
	public abstract class MimeEntity
	{
		static readonly StringComparer icase = StringComparer.OrdinalIgnoreCase;
		ContentDisposition disposition;
		string contentId;

		protected MimeEntity (string mediaType, string mediaSubtype)
		{
			Initialize (new HeaderList (), new ContentType (mediaType, mediaSubtype));
		}

		protected MimeEntity (HeaderList headers, ContentType type)
		{
			if (headers == null)
				throw new ArgumentNullException ("headers");

			if (type == null)
				throw new ArgumentNullException ("type");

			Initialize (headers, type);
		}

		void Initialize (HeaderList headers, ContentType type)
		{
			type.Changed += OnContentTypeChanged;
			headers.Changed += OnHeadersChanged;

			foreach (var header in headers)
				UpdatePropertyValue (header, true);

			ContentType = type;
			Headers = headers;
		}

		public ContentDisposition ContentDisposition {
			get { return disposition; }
			set {
				if (disposition == value)
					return;

				Headers.Changed -= OnHeadersChanged;

				if (disposition != null) {
					disposition.Changed -= OnContentDispositionChanged;
					if (value == null)
						Headers.Remove ("Content-Disposition");
				}

				disposition = value;
				if (disposition != null) {
					disposition.Changed += OnContentDispositionChanged;
					Headers["Content-Disposition"] = disposition.ToString ();
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

		public HeaderList Headers {
			get; private set;
		}

		public virtual void WriteTo (Stream stream)
		{
			Headers.WriteTo (stream);

			stream.WriteByte ((byte) '\n');
		}

		protected virtual void OnContentDispositionChanged (object sender, EventArgs e)
		{
			OnChanged ();
		}

		protected virtual void OnContentTypeChanged (object sender, EventArgs e)
		{
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
	}
}

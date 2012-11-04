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
		static readonly StringComparer icase = StringComparer.InvariantCultureIgnoreCase;

		string content_id;

		protected MimeEntity (string type, string subtype)
		{
			ContentType = new ContentType (type, subtype);
			ContentType.Changed += OnContentTypeChanged;

			Headers = new HeaderList ();
			Headers.Changed += OnHeadersChanged;
		}

		public ContentDisposition ContentDisposition {
			get; private set;
		}

		public ContentType ContentType {
			get; private set;
		}

		public string ContentId {
			get { return content_id; }
			set {
				if (content_id == value)
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
				content_id = value;
				OnChanged ();
			}
		}

		public HeaderList Headers {
			get; private set;
		}

		public abstract void CopyTo (Stream stream);

		protected virtual void OnContentDispositionChanged (object sender, EventArgs e)
		{
			OnChanged ();
		}

		protected virtual void OnContentTypeChanged (object sender, EventArgs e)
		{
			OnChanged ();
		}

		protected virtual void OnHeadersChanged (object sender, HeaderListChangedEventArgs e)
		{
			switch (e.Action) {
			case HeaderListChangedAction.Added:
			case HeaderListChangedAction.Changed:
				if (icase.Compare (e.Header.Field, "Content-Disposition") == 0) {
				} else if (icase.Compare (e.Header.Field, "Content-Type") == 0) {
				} else if (icase.Compare (e.Header.Field, "Content-Id") == 0) {
					// FIXME: extract only the value between <>'s
					content_id = e.Header.Value;
				}
				break;
			case HeaderListChangedAction.Removed:
				if (icase.Compare (e.Header.Field, "Content-Disposition") == 0) {
				} else if (icase.Compare (e.Header.Field, "Content-Type") == 0) {
				} else if (icase.Compare (e.Header.Field, "Content-Id") == 0) {
					content_id = null;
				}
				break;
			case HeaderListChangedAction.Cleared:
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

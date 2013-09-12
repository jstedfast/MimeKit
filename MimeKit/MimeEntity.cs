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
using System.Linq;
using System.Collections.Generic;

namespace MimeKit {
	public abstract class MimeEntity
	{
		static readonly StringComparer icase = StringComparer.OrdinalIgnoreCase;
		protected bool IsInitializing { get; private set; }
		ContentDisposition disposition;
		string contentId;

		protected enum ContentHeader
		{
			ContentTransferEncoding,
			ContentDescription,
			ContentDisposition,
			ContentLocation,
			ContentType,
			ContentMd5,
			ContentId,
			Unknown
		}

		// Note: this ctor is only used by the parser...
		internal protected MimeEntity (ParserOptions options, ContentType ctype, IEnumerable<Header> headers, bool toplevel)
		{
			Headers = new HeaderList (options);
			ContentType = ctype;

			ContentType.Changed += ContentTypeChanged;
			Headers.Changed += HeadersChanged;

			IsInitializing = true;
			foreach (var header in headers) {
				if (toplevel && !header.Field.StartsWith ("Content-", StringComparison.OrdinalIgnoreCase))
					continue;

				Headers.Add (header);
			}
			IsInitializing = false;
		}

		protected MimeEntity (string mediaType, string mediaSubtype)
		{
			ContentType = new ContentType (mediaType, mediaSubtype);
			Headers = new HeaderList ();

			ContentType.Changed += ContentTypeChanged;
			Headers.Changed += HeadersChanged;

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

				if (IsInitializing) {
					disposition = value;
					return;
				}

				Headers.Changed -= HeadersChanged;

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

				Headers.Changed += HeadersChanged;
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

				if (value == null) {
					Headers.Changed -= HeadersChanged;
					Headers.RemoveAll ("Content-Id");
					Headers.Changed += HeadersChanged;
					contentId = null;
					return;
				}

				var buffer = Encoding.ASCII.GetBytes (value);
				InternetAddress addr;
				int index = 0;

				if (!InternetAddress.TryParse (Headers.Options, buffer, ref index, buffer.Length, false, out addr) || !(addr is MailboxAddress))
					throw new ArgumentException ("Invalid Content-Id format.");

				contentId = "<" + ((MailboxAddress) addr).Address + ">";

				if (IsInitializing)
					return;

				Headers.Changed -= HeadersChanged;
				Headers["Content-Id"] = contentId;
				Headers.Changed += HeadersChanged;
			}
		}

		public virtual void WriteTo (Stream stream)
		{
			Headers.WriteTo (stream);

			stream.WriteByte ((byte) '\n');
		}

		void SerializeContentDisposition ()
		{
			var text = disposition.Encode (Encoding.UTF8);
			var raw = Encoding.ASCII.GetBytes (text);

			Header header;
			if (!Headers.TryGetHeader ("Content-Disposition", out header))
				Headers.Add (new Header (Headers.Options, "Content-Disposition", raw));
			else
				header.RawValue = raw;
		}

		void SerializeContentType ()
		{
			var text = ContentType.Encode (Encoding.UTF8);
			var raw = Encoding.ASCII.GetBytes (text);

			Header header;
			if (!Headers.TryGetHeader ("Content-Type", out header))
				Headers.Add (new Header (Headers.Options, "Content-Type", raw));
			else
				header.RawValue = raw;
		}

		void ContentDispositionChanged (object sender, EventArgs e)
		{
			Headers.Changed -= HeadersChanged;
			SerializeContentDisposition ();
			Headers.Changed += HeadersChanged;

			OnChanged ();
		}

		void ContentTypeChanged (object sender, EventArgs e)
		{
			Headers.Changed -= HeadersChanged;
			SerializeContentType ();
			Headers.Changed += HeadersChanged;

			OnChanged ();
		}

		static ContentHeader GetContentHeader (string field)
		{
			switch (field.ToUpperInvariant ()) {
			case "CONTENT-TRANSFER-ENCODING": return ContentHeader.ContentTransferEncoding;
			case "CONTENT-DESCRIPTION": return ContentHeader.ContentDescription;
			case "CONTENT-DISPOSITION": return ContentHeader.ContentDisposition;
			case "CONTENT-LOCATION": return ContentHeader.ContentLocation;
			case "CONTENT-TYPE": return ContentHeader.ContentType;
			case "CONTENT-MD5": return ContentHeader.ContentMd5;
			case "CONTENT-ID": return ContentHeader.ContentId;
			default: return ContentHeader.Unknown;
			}
		}

		protected virtual void OnHeadersChanged (HeaderListChangedAction action, ContentHeader type, Header header)
		{
			switch (action) {
			case HeaderListChangedAction.Added:
			case HeaderListChangedAction.Changed:
				switch (type) {
				case ContentHeader.ContentDisposition:
					ContentDisposition = ContentDisposition.Parse (header.RawValue);
					break;
				case ContentHeader.ContentId:
					contentId = MimeUtils.TryEnumerateReferences (header.RawValue, 0, header.RawValue.Length).FirstOrDefault ();
					break;
				}
				break;
			case HeaderListChangedAction.Removed:
				switch (type) {
				case ContentHeader.ContentDisposition:
					if (ContentDisposition != null)
						ContentDisposition.Changed -= ContentDispositionChanged;
					ContentDisposition = null;
					break;
				case ContentHeader.ContentId:
					contentId = null;
					break;
				}
				break;
			case HeaderListChangedAction.Cleared:
				if (ContentDisposition != null)
					ContentDisposition.Changed -= ContentDispositionChanged;

				ContentDisposition = null;
				contentId = null;
				break;
			default:
				throw new ArgumentOutOfRangeException ();
			}
		}

		protected void HeadersChanged (object sender, HeaderListChangedEventArgs e)
		{
			ContentHeader type = ContentHeader.Unknown;

			if (e.Action != HeaderListChangedAction.Cleared)
				type = GetContentHeader (e.Header.Field);

			OnHeadersChanged (e.Action, type, e.Header);
			OnChanged ();
		}

		public event EventHandler Changed;

		protected void OnChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}

		internal static MimeEntity Create (ParserOptions options, ContentType type, IEnumerable<Header> headers, bool toplevel)
		{
			if (icase.Compare (type.MediaType, "message") == 0) {
				if (icase.Compare (type.MediaSubtype, "partial") == 0)
					return new MessagePartial (options, type, headers, toplevel);

				return new MessagePart (options, type, headers, toplevel);
			}

			if (icase.Compare (type.MediaType, "multipart") == 0) {
				//if (icase.Compare (type.Subtype, "encrypted") == 0)
				//	return new MultipartEncrypted (headers, type);

				//if (icase.Compare (type.Subtype, "signed") == 0)
				//	return new MultipartSigned (headers, type);

				return new Multipart (options, type, headers, toplevel);
			}

			if (type.Matches ("text", "*"))
				return new TextPart (options, type, headers, toplevel);

			return new MimePart (options, type, headers, toplevel);
		}
	}
}

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

#if !__MOBILE__
using MimeKit.Cryptography;
#endif

using MimeKit.Utils;

namespace MimeKit {
	public abstract class MimeEntity
	{
		protected bool IsInitializing { get; private set; }
		ContentDisposition disposition;
		string contentId;

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

		/// <summary>
		/// Gets the list of headers.
		/// </summary>
		/// <value>The list of headers.</value>
		public HeaderList Headers {
			get; private set;
		}

		/// <summary>
		/// Gets or sets the content disposition.
		/// </summary>
		/// <value>The content disposition.</value>
		public ContentDisposition ContentDisposition {
			get { return disposition; }
			set {
				if (disposition == value)
					return;

				if (IsInitializing) {
					disposition = value;
					return;
				}

				if (disposition != null) {
					disposition.Changed -= ContentDispositionChanged;
					RemoveHeader ("Content-Disposition");
				}

				disposition = value;
				if (disposition != null) {
					disposition.Changed += ContentDispositionChanged;
					SerializeContentDisposition ();
				}

				OnChanged ();
			}
		}

		/// <summary>
		/// Gets the type of the content.
		/// </summary>
		/// <value>The type of the content.</value>
		public ContentType ContentType {
			get; private set;
		}

		/// <summary>
		/// Gets or sets the content identifier.
		/// </summary>
		/// <value>The content identifier.</value>
		public string ContentId {
			get { return contentId; }
			set {
				if (contentId == value)
					return;

				if (value == null) {
					RemoveHeader ("Content-Id");
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

				SetHeader ("Content-Id", contentId);
			}
		}

		/// <summary>
		/// Writes the entity instance to the specified stream.
		/// </summary>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The stream.</param>
		public virtual void WriteTo (FormatOptions options, Stream stream)
		{
			Headers.WriteTo (options, stream);

			stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
		}

		/// <summary>
		/// Writes the entity instance to the specified stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		public void WriteTo (Stream stream)
		{
			WriteTo (FormatOptions.Default, stream);
		}

		/// <summary>
		/// Removes the header.
		/// </summary>
		/// <param name="name">The name of the header.</param>
		protected void RemoveHeader (string name)
		{
			Headers.Changed -= HeadersChanged;
			Headers.RemoveAll (name);
			Headers.Changed += HeadersChanged;
		}

		/// <summary>
		/// Sets the header.
		/// </summary>
		/// <param name="name">The name of the header.</param>
		/// <param name="value">The value of the header.</param>
		protected void SetHeader (string name, string value)
		{
			Headers.Changed -= HeadersChanged;
			Headers[name] = value;
			Headers.Changed += HeadersChanged;
		}

		/// <summary>
		/// Sets the header using the raw value.
		/// </summary>
		/// <param name="name">The name of the header.</param>
		/// <param name="rawValue">The raw value of the header.</param>
		protected void SetHeader (string name, byte[] rawValue)
		{
			Header header = new Header (Headers.Options, name, rawValue);

			Headers.Changed -= HeadersChanged;
			Headers.Replace (header);
			Headers.Changed += HeadersChanged;
		}

		void SerializeContentDisposition ()
		{
			var text = disposition.Encode (FormatOptions.Default, Encoding.UTF8);
			var raw = Encoding.ASCII.GetBytes (text);

			SetHeader ("Content-Disposition", raw);
		}

		void SerializeContentType ()
		{
			var text = ContentType.Encode (FormatOptions.Default, Encoding.UTF8);
			var raw = Encoding.ASCII.GetBytes (text);

			SetHeader ("Content-Type", raw);
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

		/// <summary>
		/// Called when the headers change in some way.
		/// </summary>
		/// <param name="action">The type of change.</param>
		/// <param name="id">The <see cref="MimeKit.HeaderId"/> for the header, if known.</param>
		/// <param name="header">The header being added, changed or removed.</param>
		protected virtual void OnHeadersChanged (HeaderListChangedAction action, HeaderId id, Header header)
		{
			switch (action) {
			case HeaderListChangedAction.Added:
			case HeaderListChangedAction.Changed:
				switch (id) {
				case HeaderId.ContentDisposition:
					ContentDisposition.TryParse (Headers.Options, header.RawValue, out disposition);
					break;
				case HeaderId.ContentId:
					contentId = MimeUtils.EnumerateReferences (header.RawValue, 0, header.RawValue.Length).FirstOrDefault ();
					break;
				}
				break;
			case HeaderListChangedAction.Removed:
				switch (id) {
				case HeaderId.ContentDisposition:
					if (disposition != null)
						disposition.Changed -= ContentDispositionChanged;

					disposition = null;
					break;
				case HeaderId.ContentId:
					contentId = null;
					break;
				}
				break;
			case HeaderListChangedAction.Cleared:
				if (disposition != null)
					disposition.Changed -= ContentDispositionChanged;

				disposition = null;
				contentId = null;
				break;
			default:
				throw new ArgumentOutOfRangeException ();
			}
		}

		void HeadersChanged (object sender, HeaderListChangedEventArgs e)
		{
			HeaderId id = HeaderId.Unknown;

			if (e.Action != HeaderListChangedAction.Cleared)
				id = e.Header.Field.ToHeaderId ();

			OnHeadersChanged (e.Action, id, e.Header);
			OnChanged ();
		}

		internal event EventHandler Changed;

		protected void OnChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}

		internal static MimeEntity Create (ParserOptions options, ContentType ctype, IEnumerable<Header> headers, bool toplevel)
		{
			var subtype = ctype.MediaSubtype.ToLowerInvariant ();
			var type = ctype.MediaType.ToLowerInvariant ();

			if (type == "message") {
				if (subtype == "partial")
					return new MessagePartial (options, ctype, headers, toplevel);

				return new MessagePart (options, ctype, headers, toplevel);
			}

			if (type == "multipart") {
				#if !__MOBILE__
				if (subtype == "encrypted")
					return new MultipartEncrypted (options, ctype, headers, toplevel);

				if (subtype == "signed")
					return new MultipartSigned (options, ctype, headers, toplevel);
				#endif

				return new Multipart (options, ctype, headers, toplevel);
			}

			#if !__MOBILE__
			if (type == "application") {
				switch (subtype) {
				case "x-pkcs7-signature":
				case "pkcs7-signature":
					return new ApplicationPkcs7Signature (options, ctype, headers, toplevel);
				case "x-pgp-encrypted":
				case "pgp-encrypted":
					return new ApplicationPgpEncrypted (options, ctype, headers, toplevel);
				case "x-pgp-signature":
				case "pgp-signature":
					return new ApplicationPgpSignature (options, ctype, headers, toplevel);
				case "x-pkcs7-mime":
				case "pkcs7-mime":
					return new ApplicationPkcs7Mime (options, ctype, headers, toplevel);
				}
			}
			#endif

			if (type == "text")
				return new TextPart (options, ctype, headers, toplevel);

			return new MimePart (options, ctype, headers, toplevel);
		}
	}
}

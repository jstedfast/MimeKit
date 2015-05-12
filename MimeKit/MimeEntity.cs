//
// MimeEntity.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc.
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
using System.Threading;
using System.Collections.Generic;

#if PORTABLE
using Encoding = Portable.Text.Encoding;
#endif

using MimeKit.Utils;
using MimeKit.IO;

namespace MimeKit {
	/// <summary>
	/// An abstract MIME entity.
	/// </summary>
	/// <remarks>
	/// <para>A MIME entity is really just a node in a tree structure of MIME parts in a MIME message.</para>
	/// <para>There are 3 basic types of entities: <see cref="MimePart"/>, <see cref="Multipart"/>,
	/// and <see cref="MessagePart"/> (which is actually just a special variation of
	/// <see cref="MimePart"/> who's content is another MIME message/document). All other types are
	/// derivatives of one of those.</para>
	/// </remarks>
	public abstract class MimeEntity
	{
		ContentDisposition disposition;
		string contentId;
		Uri location;
		Uri baseUri;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MimeEntity"/> class
		/// based on the <see cref="MimeEntityConstructorInfo"/>.
		/// </summary>
		/// <remarks>
		/// Custom <see cref="MimeEntity"/> subclasses MUST implement this constructor
		/// in order to register it using <see cref="ParserOptions.RegisterMimeType"/>.
		/// </remarks>
		/// <param name="entity">Information used by the constructor.</param>
		protected MimeEntity (MimeEntityConstructorInfo entity)
		{
			if (entity == null)
				throw new ArgumentNullException ("entity");

			Headers = new HeaderList (entity.ParserOptions);
			ContentType = entity.ContentType;

			ContentType.Changed += ContentTypeChanged;
			Headers.Changed += HeadersChanged;

			foreach (var header in entity.Headers) {
				if (entity.IsTopLevel && !header.Field.StartsWith ("Content-", StringComparison.OrdinalIgnoreCase))
					continue;

				Headers.Add (header);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MimeEntity"/> class.
		/// </summary>
		/// <remarks>
		/// Initializes the <see cref="ContentType"/> based on the provided media type and subtype.
		/// </remarks>
		/// <param name="mediaType">The media type.</param>
		/// <param name="mediaSubtype">The media subtype.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="mediaType"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="mediaSubtype"/> is <c>null</c>.</para>
		/// </exception>
		protected MimeEntity (string mediaType, string mediaSubtype) : this (new ContentType (mediaType, mediaSubtype))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MimeEntity"/> class.
		/// </summary>
		/// <remarks>
		/// Initializes the <see cref="ContentType"/> to the one provided.
		/// </remarks>
		/// <param name="contentType">The content type.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="contentType"/> is <c>null</c>.
		/// </exception>
		protected MimeEntity (ContentType contentType)
		{
			if (contentType == null)
				throw new ArgumentNullException ("contentType");

			Headers = new HeaderList ();
			ContentType = contentType;

			ContentType.Changed += ContentTypeChanged;
			Headers.Changed += HeadersChanged;

			SerializeContentType ();
		}

		/// <summary>
		/// Tries to use the given object to initialize the appropriate property.
		/// </summary>
		/// <remarks>
		/// Initializes the appropriate property based on the type of the object.
		/// </remarks>
		/// <param name="obj">The object.</param>
		/// <returns><c>true</c> if the object was recognized and used; <c>false</c> otherwise.</returns>
		protected bool TryInit (object obj)
		{
			// The base MimeEntity class only knows about Headers.
			var header = obj as Header;
			if (header != null) {
				Headers.Add (header);
				return true;
			}

			var headers = obj as IEnumerable<Header>;
			if (headers != null) {
				foreach (Header h in headers)
					Headers.Add (h);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Gets the list of headers.
		/// </summary>
		/// <remarks>
		/// Represents the list of headers for a MIME part. Typically, the headers of
		/// a MIME part will be various Content-* headers such as Content-Type or
		/// Content-Disposition, but may include just about anything.
		/// </remarks>
		/// <value>The list of headers.</value>
		public HeaderList Headers {
			get; private set;
		}

		/// <summary>
		/// Gets or sets the content disposition.
		/// </summary>
		/// <remarks>
		/// Represents the pre-parsed Content-Disposition header value, if present.
		/// If the Content-Disposition header is not set, then this property will
		/// be <c>null</c>.
		/// </remarks>
		/// <value>The content disposition.</value>
		public ContentDisposition ContentDisposition {
			get { return disposition; }
			set {
				if (disposition == value)
					return;

				if (disposition != null) {
					disposition.Changed -= ContentDispositionChanged;
					RemoveHeader ("Content-Disposition");
				}

				disposition = value;
				if (disposition != null) {
					disposition.Changed += ContentDispositionChanged;
					SerializeContentDisposition ();
				}
			}
		}

		/// <summary>
		/// Gets the type of the content.
		/// </summary>
		/// <remarks>
		/// <para>The Content-Type header specifies information about the type of content contained
		/// within the MIME entity.</para>
		/// </remarks>
		/// <value>The type of the content.</value>
		public ContentType ContentType {
			get; private set;
		}

		/// <summary>
		/// Gets or sets the base content URI.
		/// </summary>
		/// <remarks>
		/// <para>The Content-Base header specifies the base URI for the <see cref="MimeEntity"/>
		/// in cases where the <see cref="ContentLocation"/> is a relative URI.</para>
		/// <para>The Content-Base URI must be an absolute URI.</para>
		/// <para>For more information, see http://www.ietf.org/rfc/rfc2110.txt</para>
		/// </remarks>
		/// <value>The base content URI or <c>null</c>.</value>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="value"/> is not an absolute URI.
		/// </exception>
		public Uri ContentBase {
			get { return baseUri; }
			set {
				if (baseUri == value)
					return;

				if (value != null && !value.IsAbsoluteUri)
					throw new ArgumentException ("The Content-Base URI may only be set to an absolute URI.", "value");

				baseUri = value;

				if (value != null)
					SetHeader ("Content-Base", value.ToString ());
				else
					RemoveHeader ("Content-Base");
			}
		}

		/// <summary>
		/// Gets or sets the content location.
		/// </summary>
		/// <remarks>
		/// <para>The Content-Location header specifies the URI for a MIME entity and can be
		/// either absolute or relative.</para>
		/// <para>Setting a Content-Location URI allows other <see cref="MimePart"/> objects
		/// within the same multipart/related container to reference this part by URI. This
		/// can be useful, for example, when constructing an HTML message body that needs to
		/// reference image attachments.</para>
		/// <para>For more information, see http://www.ietf.org/rfc/rfc2110.txt</para>
		/// </remarks>
		/// <value>The content location or <c>null</c>.</value>
		public Uri ContentLocation {
			get { return location; }
			set {
				if (location == value)
					return;

				location = value;

				if (value != null)
					SetHeader ("Content-Location", value.ToString ());
				else
					RemoveHeader ("Content-Location");
			}
		}

		/// <summary>
		/// Gets or sets the content identifier.
		/// </summary>
		/// <remarks>
		/// <para>The Content-Id header is used for uniquely identifying a particular entity and
		/// uses the same syntax as the Message-Id header on MIME messages.</para>
		/// <para>Setting a Content-Id allows other <see cref="MimePart"/> objects within the same
		/// multipart/related container to reference this part by its unique identifier, typically
		/// by using a "cid:" URI in an HTML-formatted message body. This can be useful, for example,
		/// when the HTML-formatted message body needs to reference image attachments.</para>
		/// </remarks>
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

				var buffer = Encoding.UTF8.GetBytes (value);
				InternetAddress addr;
				int index = 0;

				if (!InternetAddress.TryParse (Headers.Options, buffer, ref index, buffer.Length, false, out addr) || !(addr is MailboxAddress))
					throw new ArgumentException ("Invalid Content-Id format.");

				contentId = ((MailboxAddress) addr).Address;

				SetHeader ("Content-Id", "<" + contentId + ">");
			}
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="MimeKit.MimeEntity"/>.
		/// </summary>
		/// <remarks>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="MimeKit.MimeEntity"/>.
		/// </remarks>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="MimeKit.MimeEntity"/>.</returns>
		public override string ToString ()
		{
			using (var memory = new MemoryStream ()) {
				WriteTo (memory);

				#if !PORTABLE
				var buffer = memory.GetBuffer ();
				#else
				var buffer = memory.ToArray ();
				#endif
				int count = (int) memory.Length;

				return CharsetUtils.Latin1.GetString (buffer, 0, count);
			}
		}

		/// <summary>
		/// Writes the <see cref="MimeKit.MimeEntity"/> to the specified output stream.
		/// </summary>
		/// <remarks>
		/// <para>Writes the headers to the output stream, followed by a blank line.</para>
		/// <para>Subclasses should override this method to write the content of the entity.</para>
		/// </remarks>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The output stream.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public virtual void WriteTo (FormatOptions options, Stream stream, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (stream == null)
				throw new ArgumentNullException ("stream");

			if (options.WriteHeaders)
				Headers.WriteTo (options, stream, cancellationToken);
			else
				options.WriteHeaders = true;

			var cancellable = stream as ICancellableStream;

			if (cancellable != null) {
				cancellable.Write (options.NewLineBytes, 0, options.NewLineBytes.Length, cancellationToken);
			} else {
				cancellationToken.ThrowIfCancellationRequested ();
				stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
			}
		}

		/// <summary>
		/// Writes the <see cref="MimeKit.MimeEntity"/> to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Writes the entity to the output stream.
		/// </remarks>
		/// <param name="stream">The output stream.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public void WriteTo (Stream stream, CancellationToken cancellationToken = default (CancellationToken))
		{
			WriteTo (FormatOptions.GetDefault (), stream, cancellationToken);
		}

		#if !PORTABLE
		/// <summary>
		/// Writes the <see cref="MimeKit.MimeEntity"/> to the specified file.
		/// </summary>
		/// <remarks>
		/// Writes the <see cref="MimeKit.MimeEntity"/> to the specified file using the provided formatting options.
		/// </remarks>
		/// <param name="options">The formatting options.</param>
		/// <param name="fileName">The file.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters as defined by
		/// <see cref="System.IO.Path.InvalidPathChars"/>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// <paramref name="fileName"/> is an invalid file path.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file path could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to write to the specified file.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public void WriteTo (FormatOptions options, string fileName, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			using (var stream = File.OpenWrite (fileName))
				WriteTo (options, stream, cancellationToken);
		}

		/// <summary>
		/// Writes the <see cref="MimeKit.MimeEntity"/> to the specified file.
		/// </summary>
		/// <remarks>
		/// Writes the <see cref="MimeKit.MimeEntity"/> to the specified file using the default formatting options.
		/// </remarks>
		/// <param name="fileName">The file.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters as defined by
		/// <see cref="System.IO.Path.InvalidPathChars"/>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// <paramref name="fileName"/> is an invalid file path.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file path could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to write to the specified file.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public void WriteTo (string fileName, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			using (var stream = File.OpenWrite (fileName))
				WriteTo (FormatOptions.GetDefault (), stream, cancellationToken);
		}
		#endif

		/// <summary>
		/// Removes the header.
		/// </summary>
		/// <remarks>
		/// Removes all headers matching the specified name without
		/// calling <see cref="OnHeadersChanged"/>.
		/// </remarks>
		/// <param name="name">The name of the header.</param>
		protected void RemoveHeader (string name)
		{
			Headers.Changed -= HeadersChanged;

			try {
				Headers.RemoveAll (name);
			} finally {
				Headers.Changed += HeadersChanged;
			}
		}

		/// <summary>
		/// Sets the header.
		/// </summary>
		/// <remarks>
		/// Sets the header to the specified value without
		/// calling <see cref="OnHeadersChanged"/>.
		/// </remarks>
		/// <param name="name">The name of the header.</param>
		/// <param name="value">The value of the header.</param>
		protected void SetHeader (string name, string value)
		{
			Headers.Changed -= HeadersChanged;

			try {
				Headers[name] = value;
			} finally {
				Headers.Changed += HeadersChanged;
			}
		}

		/// <summary>
		/// Sets the header using the raw value.
		/// </summary>
		/// <remarks>
		/// Sets the header to the specified value without
		/// calling <see cref="OnHeadersChanged"/>.
		/// </remarks>
		/// <param name="name">The name of the header.</param>
		/// <param name="rawValue">The raw value of the header.</param>
		protected void SetHeader (string name, byte[] rawValue)
		{
			var header = new Header (Headers.Options, name.ToHeaderId (), name, rawValue);

			Headers.Changed -= HeadersChanged;

			try {
				Headers.Replace (header);
			} finally {
				Headers.Changed += HeadersChanged;
			}
		}

		void SerializeContentDisposition ()
		{
			var text = disposition.Encode (FormatOptions.GetDefault (), Encoding.UTF8);
			var raw = Encoding.UTF8.GetBytes (text);

			SetHeader ("Content-Disposition", raw);
		}

		void SerializeContentType ()
		{
			var text = ContentType.Encode (FormatOptions.GetDefault (), Encoding.UTF8);
			var raw = Encoding.UTF8.GetBytes (text);

			SetHeader ("Content-Type", raw);
		}

		void ContentDispositionChanged (object sender, EventArgs e)
		{
			SerializeContentDisposition ();
		}

		void ContentTypeChanged (object sender, EventArgs e)
		{
			SerializeContentType ();
		}

		/// <summary>
		/// Called when the headers change in some way.
		/// </summary>
		/// <remarks>
		/// <para>Whenever a header is added, changed, or removed, this method will
		/// be called in order to allow custom <see cref="MimeEntity"/> subclasses
		/// to update their state.</para>
		/// <para>Overrides of this method should call the base method so that their
		/// superclass may also update its own state.</para>
		/// </remarks>
		/// <param name="action">The type of change.</param>
		/// <param name="header">The header being added, changed or removed.</param>
		protected virtual void OnHeadersChanged (HeaderListChangedAction action, Header header)
		{
			string text;

			switch (action) {
			case HeaderListChangedAction.Added:
			case HeaderListChangedAction.Changed:
				switch (header.Id) {
				case HeaderId.ContentDisposition:
					if (disposition != null)
						disposition.Changed -= ContentDispositionChanged;

					if (ContentDisposition.TryParse (Headers.Options, header.RawValue, out disposition))
						disposition.Changed += ContentDispositionChanged;
					break;
				case HeaderId.ContentLocation:
					text = header.Value.Trim ();

					if (Uri.IsWellFormedUriString (text, UriKind.Absolute))
						location = new Uri (text, UriKind.Absolute);
					else if (Uri.IsWellFormedUriString (text, UriKind.Relative))
						location = new Uri (text, UriKind.Relative);
					else
						location = null;
					break;
				case HeaderId.ContentBase:
					text = header.Value.Trim ();

					if (Uri.IsWellFormedUriString (text, UriKind.Absolute))
						baseUri = new Uri (text, UriKind.Absolute);
					else
						baseUri = null;
					break;
				case HeaderId.ContentId:
					contentId = MimeUtils.EnumerateReferences (header.RawValue, 0, header.RawValue.Length).FirstOrDefault ();
					break;
				}
				break;
			case HeaderListChangedAction.Removed:
				switch (header.Id) {
				case HeaderId.ContentDisposition:
					if (disposition != null)
						disposition.Changed -= ContentDispositionChanged;

					disposition = null;
					break;
				case HeaderId.ContentLocation:
					location = null;
					break;
				case HeaderId.ContentBase:
					baseUri = null;
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
				location = null;
				baseUri = null;
				break;
			default:
				throw new ArgumentOutOfRangeException ("action");
			}
		}

		void HeadersChanged (object sender, HeaderListChangedEventArgs e)
		{
			OnHeadersChanged (e.Action, e.Header);
		}

		/// <summary>
		/// Load a <see cref="MimeEntity"/> from the specified stream.
		/// </summary>
		/// <remarks>
		/// <para>Loads a <see cref="MimeEntity"/> from the given stream, using the
		/// specified <see cref="ParserOptions"/>.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="MimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="MimeKit.IO.BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save mmeory usage, but also improve <see cref="MimeParser"/>
		/// performance.</para>
		/// </remarks>
		/// <returns>The parsed MIME entity.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="stream">The stream.</param>
		/// <param name="persistent"><c>true</c> if the stream is persistent; otherwise <c>false</c>.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the entity.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeEntity Load (ParserOptions options, Stream stream, bool persistent, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (stream == null)
				throw new ArgumentNullException ("stream");

			var parser = new MimeParser (options, stream, MimeFormat.Entity, persistent);

			return parser.ParseEntity (cancellationToken);
		}

		/// <summary>
		/// Load a <see cref="MimeEntity"/> from the specified stream.
		/// </summary>
		/// <remarks>
		/// Loads a <see cref="MimeEntity"/> from the given stream, using the
		/// specified <see cref="ParserOptions"/>.
		/// </remarks>
		/// <returns>The parsed MIME entity.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="stream">The stream.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the entity.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeEntity Load (ParserOptions options, Stream stream, CancellationToken cancellationToken = default (CancellationToken))
		{
			return Load (options, stream, false, cancellationToken);
		}

		/// <summary>
		/// Load a <see cref="MimeEntity"/> from the specified stream.
		/// </summary>
		/// <remarks>
		/// <para>Loads a <see cref="MimeEntity"/> from the given stream, using the
		/// default <see cref="ParserOptions"/>.</para>
		/// <para>If <paramref name="persistent"/> is <c>true</c> and <paramref name="stream"/> is seekable, then
		/// the <see cref="MimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="MimeKit.IO.BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save mmeory usage, but also improve <see cref="MimeParser"/>
		/// performance.</para>
		/// </remarks>
		/// <returns>The parsed MIME entity.</returns>
		/// <param name="stream">The stream.</param>
		/// <param name="persistent"><c>true</c> if the stream is persistent; otherwise <c>false</c>.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the entity.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeEntity Load (Stream stream, bool persistent, CancellationToken cancellationToken = default (CancellationToken))
		{
			return Load (ParserOptions.Default, stream, persistent, cancellationToken);
		}

		/// <summary>
		/// Load a <see cref="MimeEntity"/> from the specified stream.
		/// </summary>
		/// <remarks>
		/// Loads a <see cref="MimeEntity"/> from the given stream, using the
		/// default <see cref="ParserOptions"/>.
		/// </remarks>
		/// <returns>The parsed MIME entity.</returns>
		/// <param name="stream">The stream.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the entity.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeEntity Load (Stream stream, CancellationToken cancellationToken = default (CancellationToken))
		{
			return Load (ParserOptions.Default, stream, false, cancellationToken);
		}

#if !PORTABLE
		/// <summary>
		/// Load a <see cref="MimeEntity"/> from the specified file.
		/// </summary>
		/// <remarks>
		/// Loads a <see cref="MimeEntity"/> from the file at the give file path,
		/// using the specified <see cref="ParserOptions"/>.
		/// </remarks>
		/// <returns>The parsed entity.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="fileName">The name of the file to load.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters as defined by
		/// <see cref="System.IO.Path.InvalidPathChars"/>.
		/// </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// <paramref name="fileName"/> is an invalid file path.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file path could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the entity.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeEntity Load (ParserOptions options, string fileName, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			using (var stream = File.OpenRead (fileName)) {
				return Load (options, stream, cancellationToken);
			}
		}

		/// <summary>
		/// Load a <see cref="MimeEntity"/> from the specified file.
		/// </summary>
		/// <remarks>
		/// Loads a <see cref="MimeEntity"/> from the file at the give file path,
		/// using the default <see cref="ParserOptions"/>.
		/// </remarks>
		/// <returns>The parsed entity.</returns>
		/// <param name="fileName">The name of the file to load.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters as defined by
		/// <see cref="System.IO.Path.InvalidPathChars"/>.
		/// </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// <paramref name="fileName"/> is an invalid file path.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file path could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the entity.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeEntity Load (string fileName, CancellationToken cancellationToken = default (CancellationToken))
		{
			return Load (ParserOptions.Default, fileName, cancellationToken);
		}
#endif // !PORTABLE

		/// <summary>
		/// Load a <see cref="MimeEntity"/> from the specified content stream.
		/// </summary>
		/// <remarks>
		/// This method is mostly meant for use with APIs such as <see cref="System.Net.HttpWebResponse"/>
		/// where the headers are parsed separately from the content.
		/// </remarks>
		/// <returns>The parsed MIME entity.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="contentType">The Content-Type of the stream.</param>
		/// <param name="content">The content stream.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="contentType"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the entity.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeEntity Load (ParserOptions options, ContentType contentType, Stream content, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (contentType == null)
				throw new ArgumentNullException ("contentType");

			if (content == null)
				throw new ArgumentNullException ("content");

			var format = FormatOptions.GetDefault ();
			format.NewLineFormat = NewLineFormat.Dos;

			var encoded = contentType.Encode (format, Encoding.UTF8);
			var header = string.Format ("Content-Type:{0}\r\n", encoded);
			var chained = new ChainedStream ();

			chained.Add (new MemoryStream (Encoding.UTF8.GetBytes (header), false));
			chained.Add (content);

			return Load (options, chained, cancellationToken);
		}

		/// <summary>
		/// Load a <see cref="MimeEntity"/> from the specified content stream.
		/// </summary>
		/// <remarks>
		/// This method is mostly meant for use with APIs such as <see cref="System.Net.HttpWebResponse"/>
		/// where the headers are parsed separately from the content.
		/// </remarks>
		/// <returns>The parsed MIME entity.</returns>
		/// <param name="contentType">The Content-Type of the stream.</param>
		/// <param name="content">The content stream.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="contentType"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="content"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the entity.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeEntity Load (ContentType contentType, Stream content, CancellationToken cancellationToken = default (CancellationToken))
		{
			return Load (ParserOptions.Default, contentType, content, cancellationToken);
		}
	}
}

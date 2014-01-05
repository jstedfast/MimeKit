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
using System.Threading;
using System.Collections.Generic;

using MimeKit.Utils;
using MimeKit.IO;

namespace MimeKit {
	/// <summary>
	/// An abstract MIME entity.
	/// </summary>
	public abstract class MimeEntity
	{
		ContentDisposition disposition;
		string contentId;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MimeEntity"/> class
		/// based on the <see cref="MimeEntityConstructorInfo"/>.
		/// </summary>
		/// <param name="entity">Information used by the constructor.</param>
		/// <remarks>
		/// Custom <see cref="MimeEntity"/> subclasses MUST implement this constructor
		/// in order to register it using <see cref="ParserOptions.RegisterMimeType"/>.
		/// </remarks>
		protected MimeEntity (MimeEntityConstructorInfo entity)
		{
			if (entity == null)
				throw new ArgumentNullException ("entity");

			Headers = new HeaderList (entity.ParserOptions);
			ContentType = entity.ContentType;

			ContentType.Changed += ContentTypeChanged;
			Headers.Changed += HeadersChanged;

			IsInitializing = true;
			foreach (var header in entity.Headers) {
				if (entity.IsTopLevel && !header.Field.StartsWith ("Content-", StringComparison.OrdinalIgnoreCase))
					continue;

				Headers.Add (header);
			}
			IsInitializing = false;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MimeEntity"/> class.
		/// </summary>
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
		/// Try to use given object to initialize itself.
		/// </summary>
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
		/// Gets a value indicating whether this instance is initializing.
		/// </summary>
		/// <value><c>true</c> if this instance is initializing; otherwise, <c>false</c>.</value>
		protected bool IsInitializing {
			get; private set;
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

				SetHeader ("Content-Id", contentId);
			}
		}

		/// <summary>
		/// Writes the <see cref="MimeKit.MimeEntity"/> to the specified output stream.
		/// </summary>
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
		public virtual void WriteTo (FormatOptions options, Stream stream, CancellationToken cancellationToken)
		{
			if (options.WriteHeaders)
				Headers.WriteTo (options, stream, cancellationToken);
			else
				options.WriteHeaders = true;

			stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
		}

		/// <summary>
		/// Writes the <see cref="MimeKit.MimeEntity"/> to the specified output stream.
		/// </summary>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The output stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public void WriteTo (FormatOptions options, Stream stream)
		{
			WriteTo (options, stream, CancellationToken.None);
		}

		/// <summary>
		/// Writes the <see cref="MimeKit.MimeEntity"/> to the specified output stream.
		/// </summary>
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
		public void WriteTo (Stream stream, CancellationToken cancellationToken)
		{
			WriteTo (FormatOptions.Default, stream, cancellationToken);
		}

		/// <summary>
		/// Writes the <see cref="MimeKit.MimeEntity"/> to the specified output stream.
		/// </summary>
		/// <param name="stream">The output stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
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
			var header = new Header (Headers.Options, name, rawValue);

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
		/// <param name="header">The header being added, changed or removed.</param>
		protected virtual void OnHeadersChanged (HeaderListChangedAction action, Header header)
		{
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
			OnHeadersChanged (e.Action, e.Header);
			OnChanged ();
		}

		internal event EventHandler Changed;

		/// <summary>
		/// Raises the changed event.
		/// </summary>
		protected void OnChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}

		/// <summary>
		/// Load a <see cref="MimeEntity"/> from the specified stream.
		/// </summary>
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
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeEntity Load (ParserOptions options, Stream stream, CancellationToken cancellationToken)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (stream == null)
				throw new ArgumentNullException ("stream");

			var parser = new MimeParser (options, stream, MimeFormat.Entity);

			return parser.ParseEntity (cancellationToken);
		}

		/// <summary>
		/// Load a <see cref="MimeEntity"/> from the specified stream.
		/// </summary>
		/// <returns>The parsed MIME entity.</returns>
		/// <param name="stream">The stream.</param>
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
		public static MimeEntity Load (Stream stream, CancellationToken cancellationToken)
		{
			return Load (ParserOptions.Default, stream, cancellationToken);
		}

		/// <summary>
		/// Load a <see cref="MimeEntity"/> from the specified stream.
		/// </summary>
		/// <returns>The parsed MIME entity.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="stream">The stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeEntity Load (ParserOptions options, Stream stream)
		{
			return Load (options, stream, CancellationToken.None);
		}

		/// <summary>
		/// Load a <see cref="MimeEntity"/> from the specified stream.
		/// </summary>
		/// <returns>The parsed MIME entity.</returns>
		/// <param name="stream">The stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeEntity Load (Stream stream)
		{
			return Load (ParserOptions.Default, stream, CancellationToken.None);
		}

		/// <summary>
		/// Load a <see cref="MimeEntity"/> from the specified file.
		/// </summary>
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
		/// The specified file path is empty.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeEntity Load (ParserOptions options, string fileName, CancellationToken cancellationToken)
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
		/// <returns>The parsed entity.</returns>
		/// <param name="fileName">The name of the file to load.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The specified file path is empty.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeEntity Load (string fileName, CancellationToken cancellationToken)
		{
			return Load (ParserOptions.Default, fileName, cancellationToken);
		}

		/// <summary>
		/// Load a <see cref="MimeEntity"/> from the specified file.
		/// </summary>
		/// <returns>The parsed entity.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="fileName">The name of the file to load.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The specified file path is empty.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeEntity Load (ParserOptions options, string fileName)
		{
			return Load (options, fileName, CancellationToken.None);
		}

		/// <summary>
		/// Load a <see cref="MimeEntity"/> from the specified file.
		/// </summary>
		/// <returns>The parsed entity.</returns>
		/// <param name="fileName">The name of the file to load.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The specified file path is empty.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeEntity Load (string fileName)
		{
			return Load (ParserOptions.Default, fileName, CancellationToken.None);
		}

		/// <summary>
		/// Load a <see cref="MimeEntity"/> from the specified web response.
		/// </summary>
		/// <returns>The parsed MIME entity.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="response">The web response.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="response"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeEntity Load (ParserOptions options, System.Net.WebResponse response, CancellationToken cancellationToken)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (response == null)
				throw new ArgumentNullException ("response");

			var contentType = string.Format ("Content-Type: {0}\r\n\r\n", response.ContentType);
			var chained = new ChainedStream ();

			chained.Add (new MemoryStream (Encoding.UTF8.GetBytes (contentType), false));
			chained.Add (response.GetResponseStream ());

			return Load (options, chained, cancellationToken);
		}

		/// <summary>
		/// Load a <see cref="MimeEntity"/> from the specified web response.
		/// </summary>
		/// <returns>The parsed MIME entity.</returns>
		/// <param name="response">The web response.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="response"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeEntity Load (System.Net.WebResponse response, CancellationToken cancellationToken)
		{
			return Load (ParserOptions.Default, response, cancellationToken);
		}

		/// <summary>
		/// Load a <see cref="MimeEntity"/> from the specified web response.
		/// </summary>
		/// <returns>The parsed MIME entity.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="response">The web response.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="response"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeEntity Load (ParserOptions options, System.Net.WebResponse response)
		{
			return Load (options, response, CancellationToken.None);
		}

		/// <summary>
		/// Load a <see cref="MimeEntity"/> from the specified web response.
		/// </summary>
		/// <returns>The parsed MIME entity.</returns>
		/// <param name="response">The web response.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="response"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static MimeEntity Load (System.Net.WebResponse response)
		{
			return Load (ParserOptions.Default, response, CancellationToken.None);
		}
	}
}

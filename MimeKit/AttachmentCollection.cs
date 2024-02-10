//
// AttachmentCollection.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2024 .NET Foundation and Contributors
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
using System.Buffers;
using System.Threading;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

using MimeKit.IO;
using MimeKit.IO.Filters;

namespace MimeKit {
	/// <summary>
	/// A collection of attachments.
	/// </summary>
	/// <remarks>
	/// The <see cref="AttachmentCollection"/> is only used when building a message body with a <see cref="BodyBuilder"/>.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\BodyBuilder.cs" region="Complex" />
	/// </example>
	public class AttachmentCollection : IList<MimeEntity>
	{
		const int BufferLength = 4096;

		readonly List<MimeEntity> attachments;
		readonly bool linked;

		/// <summary>
		/// Initialize a new instance of the <see cref="AttachmentCollection"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="AttachmentCollection"/>.</para>
		/// <para>If <paramref name="linkedResources"/> is <c>true</c>, then the attachments
		/// are treated as if they are linked to another <see cref="MimePart"/>.</para>
		/// </remarks>
		/// <param name="linkedResources">If set to <c>true</c>; the attachments are treated as linked resources.</param>
		public AttachmentCollection (bool linkedResources)
		{
			attachments = new List<MimeEntity> ();
			linked = linkedResources;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="AttachmentCollection"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="AttachmentCollection"/>.
		/// </remarks>
		public AttachmentCollection () : this (false)
		{
		}

		#region IList implementation

		/// <summary>
		/// Get the number of attachments currently in the collection.
		/// </summary>
		/// <remarks>
		/// Indicates the number of attachments in the collection.
		/// </remarks>
		/// <value>The number of attachments.</value>
		public int Count {
			get { return attachments.Count; }
		}

		/// <summary>
		/// Get whther or not the collection is read-only.
		/// </summary>
		/// <remarks>
		/// A <see cref="AttachmentCollection"/> is never read-only.
		/// </remarks>
		/// <value><c>true</c> if the collection is read only; otherwise, <c>false</c>.</value>
		public bool IsReadOnly {
			get { return false; }
		}

		/// <summary>
		/// Get or set the <see cref="MimeEntity"/> at the specified index.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the <see cref="MimeEntity"/> at the specified index.</para>
		/// <note type="note">It is the responsibility of the caller to dispose the original entity at the specified <paramref name="index"/>.</note>
		/// </remarks>
		/// <value>The attachment at the specified index.</value>
		/// <param name="index">The index.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public MimeEntity this [int index] {
			get {
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException (nameof (index));

				return attachments[index];
			}
			set {
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException (nameof (index));

				if (value is null)
					throw new ArgumentNullException (nameof (value));

				attachments[index] = value;
			}
		}

		static void LoadContent (MimePart attachment, Stream stream, bool copyStream, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested ();

			Stream content = copyStream ? new MemoryBlockStream () : null;

			try {
				if (attachment.ContentType.IsMimeType ("text", "*")) {
					var buf = ArrayPool<byte>.Shared.Rent (BufferLength);
					var filter = new BestEncodingFilter ();
					int index, length;
					int nread;

					try {
						while ((nread = stream.Read (buf, 0, BufferLength)) > 0) {
							cancellationToken.ThrowIfCancellationRequested ();
							filter.Filter (buf, 0, nread, out index, out length);
							content?.Write (buf, 0, nread);
						}

						filter.Flush (buf, 0, 0, out index, out length);
					} finally {
						ArrayPool<byte>.Shared.Return (buf);
					}

					attachment.ContentTransferEncoding = filter.GetBestEncoding (EncodingConstraint.SevenBit);
				} else {
					attachment.ContentTransferEncoding = ContentEncoding.Base64;

					if (copyStream)
						stream.CopyTo (content, 4096);
				}

				if (copyStream)
					content.Position = 0;
				else
					stream.Position = 0;

				attachment.Content = new MimeContent (copyStream ? content : stream);
			} catch {
				content?.Dispose ();
				throw;
			}
		}

		static async Task LoadContentAsync (MimePart attachment, Stream stream, bool copyStream, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested ();

			Stream content = copyStream ? new MemoryBlockStream () : null;

			try {
				if (attachment.ContentType.IsMimeType ("text", "*")) {
					var buf = ArrayPool<byte>.Shared.Rent (BufferLength);
					var filter = new BestEncodingFilter ();
					int index, length;
					int nread;

					try {
						while ((nread = await stream.ReadAsync (buf, 0, BufferLength, cancellationToken).ConfigureAwait (false)) > 0) {
							cancellationToken.ThrowIfCancellationRequested ();
							filter.Filter (buf, 0, nread, out index, out length);
							content?.Write (buf, 0, nread);
						}

						filter.Flush (buf, 0, 0, out index, out length);
					} finally {
						ArrayPool<byte>.Shared.Return (buf);
					}

					attachment.ContentTransferEncoding = filter.GetBestEncoding (EncodingConstraint.SevenBit);
				} else {
					attachment.ContentTransferEncoding = ContentEncoding.Base64;

					if (copyStream)
						await stream.CopyToAsync (content, 4096, cancellationToken).ConfigureAwait (false);
				}

				if (copyStream)
					content.Position = 0;
				else
					stream.Position = 0;

				attachment.Content = new MimeContent (copyStream ? content : stream);
			} catch {
				content?.Dispose ();
				throw;
			}
		}

		static ContentType GetMimeType (string fileName)
		{
			var mimeType = MimeTypes.GetMimeType (fileName);

			return ContentType.Parse (mimeType);
		}

		static string GetFileName (string path)
		{
			int index = path.LastIndexOf (Path.DirectorySeparatorChar);

			return index > 0 ? path.Substring (index + 1) : path;
		}

		MimeEntity CreateAttachment (ContentType contentType, bool autoDetected, string path, Stream stream, bool copyStream, CancellationToken cancellationToken)
		{
			var fileName = GetFileName (path);
			MimeEntity attachment = null;

			if (contentType.IsMimeType ("message", "rfc822")) {
				long position = stream.CanSeek ? stream.Position : 0;

				try {
					var message = MimeMessage.Load (stream, cancellationToken);

					if (!copyStream)
						stream.Dispose ();

					attachment = new MessagePart { Message = message };
				} catch (FormatException) {
					if (autoDetected && stream.CanSeek) {
						// If the contentType was auto-detected and the stream is seekable, fall back to attaching this content as a generic stream
						contentType = new ContentType ("application", "octet-stream");
						stream.Position = position;
					} else {
						throw;
					}
				}
			}

			if (attachment is null) {
				MimePart part;

				if (contentType.IsMimeType ("text", "*")) {
					// TODO: should we try to auto-detect charsets if no charset parameter is specified?
					part = new TextPart (contentType);
				} else {
					part = new MimePart (contentType);
				}

				LoadContent (part, stream, copyStream, cancellationToken);
				attachment = part;
			}

			attachment.ContentDisposition = new ContentDisposition (linked ? ContentDisposition.Inline : ContentDisposition.Attachment) {
				FileName = fileName
			};
			attachment.ContentType.Name = fileName;

			if (linked)
				attachment.ContentLocation = new Uri (fileName, UriKind.Relative);

			return attachment;
		}

		async Task<MimeEntity> CreateAttachmentAsync (ContentType contentType, bool autoDetected, string path, Stream stream, bool copyStream, CancellationToken cancellationToken)
		{
			var fileName = GetFileName (path);
			MimeEntity attachment = null;

			if (contentType.IsMimeType ("message", "rfc822")) {
				long position = stream.CanSeek ? stream.Position : 0;

				try {
					var message = await MimeMessage.LoadAsync (stream, cancellationToken).ConfigureAwait (false);

					if (!copyStream)
						stream.Dispose ();

					attachment = new MessagePart { Message = message };
				} catch (FormatException) {
					if (autoDetected && stream.CanSeek) {
						// If the contentType was auto-detected and the stream is seekable, fall back to attaching this content as a generic stream
						contentType = new ContentType ("application", "octet-stream");
						stream.Position = position;
					} else {
						throw;
					}
				}
			}

			if (attachment is null) {
				MimePart part;

				if (contentType.IsMimeType ("text", "*")) {
					// TODO: should we try to auto-detect charsets if no charset parameter is specified?
					part = new TextPart (contentType);
				} else {
					part = new MimePart (contentType);
				}

				await LoadContentAsync (part, stream, copyStream, cancellationToken).ConfigureAwait (false);
				attachment = part;
			}

			attachment.ContentDisposition = new ContentDisposition (linked ? ContentDisposition.Inline : ContentDisposition.Attachment) {
				FileName = fileName
			};
			attachment.ContentType.Name = fileName;

			if (linked)
				attachment.ContentLocation = new Uri (fileName, UriKind.Relative);

			return attachment;
		}

		/// <summary>
		/// Add an attachment.
		/// </summary>
		/// <remarks>
		/// <para>Adds the specified data as an attachment using the supplied Content-Type.</para>
		/// <para>The file name parameter is used to set the Content-Location.</para>
		/// <para>For a list of known mime-types and their associated file extensions, see
		/// http://svn.apache.org/repos/asf/httpd/httpd/trunk/docs/conf/mime.types</para>
		/// </remarks>
		/// <returns>The newly added attachment <see cref="MimeEntity"/>.</returns>
		/// <param name="fileName">The name of the file.</param>
		/// <param name="data">The file data.</param>
		/// <param name="contentType">The mime-type of the file.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="data"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="contentType"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The specified file path is empty.
		/// </exception>
		public MimeEntity Add (string fileName, byte[] data, ContentType contentType)
		{
			if (fileName is null)
				throw new ArgumentNullException (nameof (fileName));

			if (fileName.Length == 0)
				throw new ArgumentException ("The specified file path is empty.", nameof (fileName));

			if (data is null)
				throw new ArgumentNullException (nameof (data));

			if (contentType is null)
				throw new ArgumentNullException (nameof (contentType));

			var stream = new MemoryStream (data, false);
			var attachment = CreateAttachment (contentType, false, fileName, stream, false, CancellationToken.None);

			attachments.Add (attachment);

			return attachment;
		}

		/// <summary>
		/// Add an attachment.
		/// </summary>
		/// <remarks>
		/// <para>Adds the specified data as an attachment using the supplied Content-Type.</para>
		/// <para>The file name parameter is used to set the Content-Location.</para>
		/// <para>For a list of known mime-types and their associated file extensions, see
		/// http://svn.apache.org/repos/asf/httpd/httpd/trunk/docs/conf/mime.types</para>
		/// </remarks>
		/// <returns>The newly added attachment <see cref="MimeEntity"/>.</returns>
		/// <param name="fileName">The name of the file.</param>
		/// <param name="stream">The content stream.</param>
		/// <param name="contentType">The mime-type of the file.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="contentType"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The specified file path is empty.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		/// <exception cref="OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public MimeEntity Add (string fileName, Stream stream, ContentType contentType, CancellationToken cancellationToken = default)
		{
			if (fileName is null)
				throw new ArgumentNullException (nameof (fileName));

			if (fileName.Length == 0)
				throw new ArgumentException ("The specified file path is empty.", nameof (fileName));

			if (stream is null)
				throw new ArgumentNullException (nameof (stream));

			if (contentType is null)
				throw new ArgumentNullException (nameof (contentType));

			var attachment = CreateAttachment (contentType, false, fileName, stream, true, cancellationToken);

			attachments.Add (attachment);

			return attachment;
		}

		/// <summary>
		/// Asynchronously add an attachment.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously adds the specified data as an attachment using the supplied Content-Type.</para>
		/// <para>The file name parameter is used to set the Content-Location.</para>
		/// <para>For a list of known mime-types and their associated file extensions, see
		/// http://svn.apache.org/repos/asf/httpd/httpd/trunk/docs/conf/mime.types</para>
		/// </remarks>
		/// <returns>The newly added attachment <see cref="MimeEntity"/>.</returns>
		/// <param name="fileName">The name of the file.</param>
		/// <param name="stream">The content stream.</param>
		/// <param name="contentType">The mime-type of the file.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="contentType"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The specified file path is empty.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		/// <exception cref="OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public async Task<MimeEntity> AddAsync (string fileName, Stream stream, ContentType contentType, CancellationToken cancellationToken = default)
		{
			if (fileName is null)
				throw new ArgumentNullException (nameof (fileName));

			if (fileName.Length == 0)
				throw new ArgumentException ("The specified file path is empty.", nameof (fileName));

			if (stream is null)
				throw new ArgumentNullException (nameof (stream));

			if (contentType is null)
				throw new ArgumentNullException (nameof (contentType));

			var attachment = await CreateAttachmentAsync (contentType, false, fileName, stream, true, cancellationToken).ConfigureAwait (false);

			attachments.Add (attachment);

			return attachment;
		}

		/// <summary>
		/// Add an attachment.
		/// </summary>
		/// <remarks>
		/// <para>Adds the data as an attachment, using the specified file name for deducing
		/// the mime-type by extension and for setting the Content-Location.</para>
		/// </remarks>
		/// <returns>The newly added attachment <see cref="MimeEntity"/>.</returns>
		/// <param name="fileName">The name of the file.</param>
		/// <param name="data">The file data to attach.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="data"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The specified file path is empty.
		/// </exception>
		public MimeEntity Add (string fileName, byte[] data)
		{
			if (fileName is null)
				throw new ArgumentNullException (nameof (fileName));

			if (fileName.Length == 0)
				throw new ArgumentException ("The specified file path is empty.", nameof (fileName));

			if (data is null)
				throw new ArgumentNullException (nameof (data));

			var stream = new MemoryStream (data, false);
			var attachment = CreateAttachment (GetMimeType (fileName), true, fileName, stream, false, CancellationToken.None);

			attachments.Add (attachment);

			return attachment;
		}

		/// <summary>
		/// Add an attachment.
		/// </summary>
		/// <remarks>
		/// <para>Adds the stream as an attachment, using the specified file name for deducing
		/// the mime-type by extension and for setting the Content-Location.</para>
		/// </remarks>
		/// <returns>The newly added attachment <see cref="MimeEntity"/>.</returns>
		/// <param name="fileName">The name of the file.</param>
		/// <param name="stream">The content stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The specified file path is empty.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		/// <exception cref="OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public MimeEntity Add (string fileName, Stream stream, CancellationToken cancellationToken = default)
		{
			if (fileName is null)
				throw new ArgumentNullException (nameof (fileName));

			if (fileName.Length == 0)
				throw new ArgumentException ("The specified file path is empty.", nameof (fileName));

			if (stream is null)
				throw new ArgumentNullException (nameof (stream));

			var attachment = CreateAttachment (GetMimeType (fileName), true, fileName, stream, true, cancellationToken);

			attachments.Add (attachment);

			return attachment;
		}

		/// <summary>
		/// Asynchronously add an attachment.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously adds the stream as an attachment, using the specified file name for deducing
		/// the mime-type by extension and for setting the Content-Location.</para>
		/// </remarks>
		/// <returns>The newly added attachment <see cref="MimeEntity"/>.</returns>
		/// <param name="fileName">The name of the file.</param>
		/// <param name="stream">The content stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The specified file path is empty.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		/// <exception cref="OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public async Task<MimeEntity> AddAsync (string fileName, Stream stream, CancellationToken cancellationToken = default)
		{
			if (fileName is null)
				throw new ArgumentNullException (nameof (fileName));

			if (fileName.Length == 0)
				throw new ArgumentException ("The specified file path is empty.", nameof (fileName));

			if (stream is null)
				throw new ArgumentNullException (nameof (stream));

			var attachment = await CreateAttachmentAsync (GetMimeType (fileName), true, fileName, stream, true, cancellationToken).ConfigureAwait (false);

			attachments.Add (attachment);

			return attachment;
		}

		/// <summary>
		/// Add an attachment.
		/// </summary>
		/// <remarks>
		/// <para>Adds the specified file as an attachment using the supplied Content-Type.</para>
		/// <para>For a list of known mime-types and their associated file extensions, see
		/// http://svn.apache.org/repos/asf/httpd/httpd/trunk/docs/conf/mime.types</para>
		/// </remarks>
		/// <returns>The newly added attachment <see cref="MimeEntity"/>.</returns>
		/// <param name="fileName">The name of the file.</param>
		/// <param name="contentType">The mime-type of the file.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="contentType"/> is <c>null</c>.</para>
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
		/// <exception cref="OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public MimeEntity Add (string fileName, ContentType contentType, CancellationToken cancellationToken = default)
		{
			if (fileName is null)
				throw new ArgumentNullException (nameof (fileName));

			if (fileName.Length == 0)
				throw new ArgumentException ("The specified file path is empty.", nameof (fileName));

			if (contentType is null)
				throw new ArgumentNullException (nameof (contentType));

			using (var stream = File.OpenRead (fileName)) {
				var attachment = CreateAttachment (contentType, false, fileName, stream, true, cancellationToken);

				attachments.Add (attachment);

				return attachment;
			}
		}

		/// <summary>
		/// Asynchronously add an attachment.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously adds the specified file as an attachment using the supplied Content-Type.</para>
		/// <para>For a list of known mime-types and their associated file extensions, see
		/// http://svn.apache.org/repos/asf/httpd/httpd/trunk/docs/conf/mime.types</para>
		/// </remarks>
		/// <returns>The newly added attachment <see cref="MimeEntity"/>.</returns>
		/// <param name="fileName">The name of the file.</param>
		/// <param name="contentType">The mime-type of the file.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="contentType"/> is <c>null</c>.</para>
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
		/// <exception cref="OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public async Task<MimeEntity> AddAsync (string fileName, ContentType contentType, CancellationToken cancellationToken = default)
		{
			if (fileName is null)
				throw new ArgumentNullException (nameof (fileName));

			if (fileName.Length == 0)
				throw new ArgumentException ("The specified file path is empty.", nameof (fileName));

			if (contentType is null)
				throw new ArgumentNullException (nameof (contentType));

			using (var stream = File.OpenRead (fileName)) {
				var attachment = await CreateAttachmentAsync (contentType, false, fileName, stream, true, cancellationToken).ConfigureAwait (false);

				attachments.Add (attachment);

				return attachment;
			}
		}

		/// <summary>
		/// Add an attachment.
		/// </summary>
		/// <remarks>
		/// <para>Adds the specified file as an attachment.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\BodyBuilder.cs" region="Complex" />
		/// </example>
		/// <returns>The newly added attachment <see cref="MimeEntity"/>.</returns>
		/// <param name="fileName">The name of the file.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.
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
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		/// <exception cref="OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public MimeEntity Add (string fileName, CancellationToken cancellationToken = default)
		{
			if (fileName is null)
				throw new ArgumentNullException (nameof (fileName));

			if (fileName.Length == 0)
				throw new ArgumentException ("The specified file path is empty.", nameof (fileName));

			using (var stream = File.OpenRead (fileName)) {
				var attachment = CreateAttachment (GetMimeType (fileName), true, fileName, stream, true, cancellationToken);

				attachments.Add (attachment);

				return attachment;
			}
		}

		/// <summary>
		/// Asynchronously add an attachment.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously adds the specified file as an attachment.</para>
		/// </remarks>
		/// <returns>The newly added attachment <see cref="MimeEntity"/>.</returns>
		/// <param name="fileName">The name of the file.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.
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
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		/// <exception cref="OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public async Task<MimeEntity> AddAsync (string fileName, CancellationToken cancellationToken = default)
		{
			if (fileName is null)
				throw new ArgumentNullException (nameof (fileName));

			if (fileName.Length == 0)
				throw new ArgumentException ("The specified file path is empty.", nameof (fileName));

			using (var stream = File.OpenRead (fileName)) {
				var attachment = await CreateAttachmentAsync (GetMimeType (fileName), true, fileName, stream, true, cancellationToken).ConfigureAwait (false);

				attachments.Add (attachment);

				return attachment;
			}
		}

		/// <summary>
		/// Add an attachment.
		/// </summary>
		/// <remarks>
		/// Adds the specified <see cref="MimePart"/> as an attachment.
		/// </remarks>
		/// <param name="attachment">The attachment.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="attachment"/> is <c>null</c>.
		/// </exception>
		public void Add (MimeEntity attachment)
		{
			if (attachment is null)
				throw new ArgumentNullException (nameof (attachment));

			attachments.Add (attachment);
		}

		/// <summary>
		/// Clear the attachment collection.
		/// </summary>
		/// <remarks>
		/// Removes all attachments from the collection.
		/// </remarks>
		public void Clear ()
		{
			Clear (false);
		}

		/// <summary>
		/// Clear the attachment collection.
		/// </summary>
		/// <remarks>
		/// Removes all attachments from the collection, optionally disposing them in the process.
		/// </remarks>
		/// <param name="dispose"><c>true</c> if all of the attachments should be disposed; otherwise, <c>false</c>.</param>
		public void Clear (bool dispose)
		{
			if (dispose) {
				for (int i = 0; i < attachments.Count; i++)
					attachments[i].Dispose ();
			}

			attachments.Clear ();
		}

		/// <summary>
		/// Check if the collection contains the specified attachment.
		/// </summary>
		/// <remarks>
		/// Determines whether or not the collection contains the specified attachment.
		/// </remarks>
		/// <returns><value>true</value> if the specified attachment exists;
		/// otherwise <value>false</value>.</returns>
		/// <param name="attachment">The attachment.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="attachment"/> is <c>null</c>.
		/// </exception>
		public bool Contains (MimeEntity attachment)
		{
			if (attachment is null)
				throw new ArgumentNullException (nameof (attachment));

			return attachments.Contains (attachment);
		}

		/// <summary>
		/// Copy all of the attachments in the collection to an array.
		/// </summary>
		/// <remarks>
		/// Copies all of the attachments within the <see cref="AttachmentCollection"/> into the array,
		/// starting at the specified array index.
		/// </remarks>
		/// <param name="array">The array to copy the attachments to.</param>
		/// <param name="arrayIndex">The index into the array.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="array"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex"/> is out of range.
		/// </exception>
		public void CopyTo (MimeEntity[] array, int arrayIndex)
		{
			if (array is null)
				throw new ArgumentNullException (nameof (array));

			if (arrayIndex < 0 || arrayIndex >= array.Length)
				throw new ArgumentOutOfRangeException (nameof (arrayIndex));

			attachments.CopyTo (array, arrayIndex);
		}

		/// <summary>
		/// Get the index of the requested attachment, if it exists.
		/// </summary>
		/// <remarks>
		/// Finds the index of the specified attachment, if it exists.
		/// </remarks>
		/// <returns>The index of the requested attachment; otherwise <value>-1</value>.</returns>
		/// <param name="attachment">The attachment.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="attachment"/> is <c>null</c>.
		/// </exception>
		public int IndexOf (MimeEntity attachment)
		{
			if (attachment is null)
				throw new ArgumentNullException (nameof (attachment));

			return attachments.IndexOf (attachment);
		}

		/// <summary>
		/// Insert an attachment at the given index.
		/// </summary>
		/// <remarks>
		/// Inserts the attachment at the specified index.
		/// </remarks>
		/// <param name="index">The index to insert the attachment.</param>
		/// <param name="attachment">The attachment.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="attachment"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public void Insert (int index, MimeEntity attachment)
		{
			if (index < 0 || index >= Count)
				throw new ArgumentOutOfRangeException (nameof (index));

			if (attachment is null)
				throw new ArgumentNullException (nameof (attachment));

			attachments.Insert (index, attachment);
		}

		/// <summary>
		/// Remove an attachment.
		/// </summary>
		/// <remarks>
		/// Removes the specified attachment.
		/// </remarks>
		/// <returns><value>true</value> if the attachment was removed; otherwise <value>false</value>.</returns>
		/// <param name="attachment">The attachment.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="attachment"/> is <c>null</c>.
		/// </exception>
		public bool Remove (MimeEntity attachment)
		{
			if (attachment is null)
				throw new ArgumentNullException (nameof (attachment));

			return attachments.Remove (attachment);
		}

		/// <summary>
		/// Remove the attachment at the specified index.
		/// </summary>
		/// <remarks>
		/// <para>Removes the attachment at the specified index.</para>
		/// <note type="note">It is the responsibility of the caller to dispose the entity at the specified <paramref name="index"/>.</note>
		/// </remarks>
		/// <param name="index">The index.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public void RemoveAt (int index)
		{
			if (index < 0 || index >= Count)
				throw new ArgumentOutOfRangeException (nameof (index));

			attachments.RemoveAt (index);
		}

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Get an enumerator for the list of attachments.
		/// </summary>
		/// <remarks>
		/// Gets an enumerator for the list of attachments.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		public IEnumerator<MimeEntity> GetEnumerator ()
		{
			return attachments.GetEnumerator ();
		}

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Get an enumerator for the list of attachments.
		/// </summary>
		/// <remarks>
		/// Gets an enumerator for the list of attachments.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		#endregion
	}
}

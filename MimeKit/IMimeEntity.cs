//
// IMimeEntity.cs
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
using System.Threading;
using System.Threading.Tasks;

namespace MimeKit {
	/// <summary>
	/// An interface for a MIME entity.
	/// </summary>
	/// <remarks>
	/// <para>A MIME entity is really just a node in a tree structure of MIME parts in a MIME message.</para>
	/// <para>There are 3 basic types of entities: <see cref="MimePart"/>, <see cref="Multipart"/>,
	/// and <see cref="MessagePart"/> (which is actually just a special variation of
	/// <see cref="MimePart"/> who's content is another MIME message/document). All other types are
	/// derivatives of one of those.</para>
	/// </remarks>
	public interface IMimeEntity : IDisposable
	{
		/// <summary>
		/// Get the list of headers.
		/// </summary>
		/// <remarks>
		/// Represents the list of headers for a MIME part. Typically, the headers of
		/// a MIME part will be various Content-* headers such as Content-Type or
		/// Content-Disposition, but may include just about anything.
		/// </remarks>
		/// <value>The list of headers.</value>
		HeaderList Headers {
			get;
		}

		/// <summary>
		/// Get or set the content disposition.
		/// </summary>
		/// <remarks>
		/// Represents the pre-parsed Content-Disposition header value, if present.
		/// If the Content-Disposition header is not set, then this property will
		/// be <c>null</c>.
		/// </remarks>
		/// <value>The content disposition.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimeEntity"/> has been disposed.
		/// </exception>
		ContentDisposition ContentDisposition {
			get; set;
		}

		/// <summary>
		/// Get the type of the content.
		/// </summary>
		/// <remarks>
		/// <para>The Content-Type header specifies information about the type of content contained
		/// within the MIME entity.</para>
		/// </remarks>
		/// <value>The type of the content.</value>
		ContentType ContentType {
			get;
		}

		/// <summary>
		/// Get or set the base content URI.
		/// </summary>
		/// <remarks>
		/// <para>The Content-Base header specifies the base URI for the <see cref="MimeEntity"/>
		/// in cases where the <see cref="ContentLocation"/> is a relative URI.</para>
		/// <para>The Content-Base URI must be an absolute URI.</para>
		/// <para>For more information, see <a href="https://tools.ietf.org/html/rfc2110">rfc2110</a>.</para>
		/// </remarks>
		/// <value>The base content URI or <c>null</c>.</value>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="value"/> is not an absolute URI.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimeEntity"/> has been disposed.
		/// </exception>
		Uri ContentBase {
			get; set;
		}

		/// <summary>
		/// Get or set the content location.
		/// </summary>
		/// <remarks>
		/// <para>The Content-Location header specifies the URI for a MIME entity and can be
		/// either absolute or relative.</para>
		/// <para>Setting a Content-Location URI allows other <see cref="MimePart"/> objects
		/// within the same multipart/related container to reference this part by URI. This
		/// can be useful, for example, when constructing an HTML message body that needs to
		/// reference image attachments.</para>
		/// <para>For more information, see <a href="https://tools.ietf.org/html/rfc2110">rfc2110</a>.</para>
		/// </remarks>
		/// <value>The content location or <c>null</c>.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimeEntity"/> has been disposed.
		/// </exception>
		public Uri ContentLocation {
			get; set;
		}

		/// <summary>
		/// Get or set the Content-Id.
		/// </summary>
		/// <remarks>
		/// <para>The <c>Content-Id</c> header is used for uniquely identifying a particular entity and
		/// uses the same syntax as the <c>Message-Id</c> header on MIME messages.</para>
		/// <para>Setting a <c>Content-Id</c> allows other <see cref="MimePart"/> objects within the same
		/// multipart/related container to reference this part by its unique identifier, typically
		/// by using a "cid:" URI in an HTML-formatted message body. This can be useful, for example,
		/// when the HTML-formatted message body needs to reference image attachments.</para>
		/// <note type="note">It is recommended that <see cref="Utils.MimeUtils.GenerateMessageId()"/> or
		/// <see cref="Utils.MimeUtils.GenerateMessageId(string)"/> be used to generate a valid
		/// <c>Content-Id</c> value.</note>
		/// </remarks>
		/// <value>The content identifier.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimeEntity"/> has been disposed.
		/// </exception>
		string ContentId {
			get; set;
		}

		/// <summary>
		/// Get a value indicating whether this entity is an attachment.
		/// </summary>
		/// <remarks>
		/// If the Content-Disposition header is set and has a value of <c>"attachment"</c>,
		/// then this property returns <c>true</c>. Otherwise it is assumed that the
		/// entity is not meant to be treated as an attachment.
		/// </remarks>
		/// <value><c>true</c> if this entity is an attachment; otherwise, <c>false</c>.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimeEntity"/> has been disposed.
		/// </exception>
		bool IsAttachment {
			get; set;
		}

		/// <summary>
		/// Dispatches to the specific visit method for this MIME entity.
		/// </summary>
		/// <remarks>
		/// Dispatches to the specific visit method for this MIME entity.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimeEntity"/> has been disposed.
		/// </exception>
		void Accept (MimeVisitor visitor);

		/// <summary>
		/// Prepare the MIME entity for transport using the specified encoding constraints.
		/// </summary>
		/// <remarks>
		/// Prepares the MIME entity for transport using the specified encoding constraints.
		/// </remarks>
		/// <param name="constraint">The encoding constraint.</param>
		/// <param name="maxLineLength">The maximum allowable length for a line (not counting the CRLF). Must be between <c>72</c> and <c>998</c> (inclusive).</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="maxLineLength"/> is not between <c>72</c> and <c>998</c> (inclusive).</para>
		/// <para>-or-</para>
		/// <para><paramref name="constraint"/> is not a valid value.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimeEntity"/> has been disposed.
		/// </exception>
		void Prepare (EncodingConstraint constraint, int maxLineLength = 78);

		/// <summary>
		/// Write the <see cref="IMimeEntity"/> to the specified output stream.
		/// </summary>
		/// <remarks>
		/// <para>Writes the headers to the output stream, followed by a blank line.</para>
		/// <para>Subclasses should override this method to write the content of the entity.</para>
		/// </remarks>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The output stream.</param>
		/// <param name="contentOnly"><c>true</c> if only the content should be written; otherwise, <c>false</c>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimeEntity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		void WriteTo (FormatOptions options, Stream stream, bool contentOnly, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously write the <see cref="IMimeEntity"/> to the specified output stream.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously writes the headers to the output stream, followed by a blank line.</para>
		/// <para>Subclasses should override this method to write the content of the entity.</para>
		/// </remarks>
		/// <returns>An awaitable task.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The output stream.</param>
		/// <param name="contentOnly"><c>true</c> if only the content should be written; otherwise, <c>false</c>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimeEntity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		Task WriteToAsync (FormatOptions options, Stream stream, bool contentOnly, CancellationToken cancellationToken = default);

		/// <summary>
		/// Write the <see cref="IMimeEntity"/> to the specified output stream.
		/// </summary>
		/// <remarks>
		/// <para>Writes the headers to the output stream, followed by a blank line.</para>
		/// <para>Subclasses should override this method to write the content of the entity.</para>
		/// </remarks>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The output stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimeEntity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		void WriteTo (FormatOptions options, Stream stream, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously write the <see cref="IMimeEntity"/> to the specified output stream.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously writes the headers to the output stream, followed by a blank line.</para>
		/// <para>Subclasses should override this method to write the content of the entity.</para>
		/// </remarks>
		/// <returns>An awaitable task.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The output stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimeEntity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		Task WriteToAsync (FormatOptions options, Stream stream, CancellationToken cancellationToken = default);

		/// <summary>
		/// Write the <see cref="IMimeEntity"/> to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Writes the entity to the output stream.
		/// </remarks>
		/// <param name="stream">The output stream.</param>
		/// <param name="contentOnly"><c>true</c> if only the content should be written; otherwise, <c>false</c>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimeEntity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		void WriteTo (Stream stream, bool contentOnly, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously write the <see cref="IMimeEntity"/> to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously writes the entity to the output stream.
		/// </remarks>
		/// <returns>An awaitable task.</returns>
		/// <param name="stream">The output stream.</param>
		/// <param name="contentOnly"><c>true</c> if only the content should be written; otherwise, <c>false</c>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimeEntity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		Task WriteToAsync (Stream stream, bool contentOnly, CancellationToken cancellationToken = default);

		/// <summary>
		/// Write the <see cref="IMimeEntity"/> to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Writes the entity to the output stream.
		/// </remarks>
		/// <param name="stream">The output stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimeEntity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		void WriteTo (Stream stream, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously write the <see cref="IMimeEntity"/> to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously writes the entity to the output stream.
		/// </remarks>
		/// <returns>An awaitable task.</returns>
		/// <param name="stream">The output stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimeEntity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		Task WriteToAsync (Stream stream, CancellationToken cancellationToken = default);

		/// <summary>
		/// Write the <see cref="IMimeEntity"/> to the specified file.
		/// </summary>
		/// <remarks>
		/// Writes the entity to the specified file using the provided formatting options.
		/// </remarks>
		/// <param name="options">The formatting options.</param>
		/// <param name="fileName">The file.</param>
		/// <param name="contentOnly"><c>true</c> if only the content should be written; otherwise, <c>false</c>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimeEntity"/> has been disposed.
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
		void WriteTo (FormatOptions options, string fileName, bool contentOnly, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously write the <see cref="IMimeEntity"/> to the specified file.
		/// </summary>
		/// <remarks>
		/// Asynchronously writes the entity to the specified file using the provided formatting options.
		/// </remarks>
		/// <returns>An awaitable task.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="fileName">The file.</param>
		/// <param name="contentOnly"><c>true</c> if only the content should be written; otherwise, <c>false</c>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimeEntity"/> has been disposed.
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
		Task WriteToAsync (FormatOptions options, string fileName, bool contentOnly, CancellationToken cancellationToken = default);

		/// <summary>
		/// Write the <see cref="IMimeEntity"/> to the specified file.
		/// </summary>
		/// <remarks>
		/// Writes the entity to the specified file using the provided formatting options.
		/// </remarks>
		/// <param name="options">The formatting options.</param>
		/// <param name="fileName">The file.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimeEntity"/> has been disposed.
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
		void WriteTo (FormatOptions options, string fileName, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously write the <see cref="IMimeEntity"/> to the specified file.
		/// </summary>
		/// <remarks>
		/// Asynchronously writes the entity to the specified file using the provided formatting options.
		/// </remarks>
		/// <returns>An awaitable task.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="fileName">The file.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimeEntity"/> has been disposed.
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
		Task WriteToAsync (FormatOptions options, string fileName, CancellationToken cancellationToken = default);

		/// <summary>
		/// Write the <see cref="IMimeEntity"/> to the specified file.
		/// </summary>
		/// <remarks>
		/// Writes the entity to the specified file using the default formatting options.
		/// </remarks>
		/// <param name="fileName">The file.</param>
		/// <param name="contentOnly"><c>true</c> if only the content should be written; otherwise, <c>false</c>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimeEntity"/> has been disposed.
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
		void WriteTo (string fileName, bool contentOnly, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously write the <see cref="IMimeEntity"/> to the specified file.
		/// </summary>
		/// <remarks>
		/// Asynchronously writes the entity to the specified file using the default formatting options.
		/// </remarks>
		/// <returns>An awaitable task.</returns>
		/// <param name="fileName">The file.</param>
		/// <param name="contentOnly"><c>true</c> if only the content should be written; otherwise, <c>false</c>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimeEntity"/> has been disposed.
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
		Task WriteToAsync (string fileName, bool contentOnly, CancellationToken cancellationToken = default);

		/// <summary>
		/// Write the <see cref="IMimeEntity"/> to the specified file.
		/// </summary>
		/// <remarks>
		/// Writes the entity to the specified file using the default formatting options.
		/// </remarks>
		/// <param name="fileName">The file.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimeEntity"/> has been disposed.
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
		void WriteTo (string fileName, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously write the <see cref="IMimeEntity"/> to the specified file.
		/// </summary>
		/// <remarks>
		/// Asynchronously writes the entity to the specified file using the default formatting options.
		/// </remarks>
		/// <returns>An awaitable task.</returns>
		/// <param name="fileName">The file.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimeEntity"/> has been disposed.
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
		Task WriteToAsync (string fileName, CancellationToken cancellationToken = default);
	}
}

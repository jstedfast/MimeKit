//
// IMimePart.cs
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

using System.Threading;

namespace MimeKit {
	/// <summary>
	/// An interface for a leaf-node MIME part that contains content such as the message body text or an attachment.
	/// </summary>
	/// <remarks>
	/// A leaf-node MIME part that contains content such as the message body text or an attachment.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\AttachmentExamples.cs" region="SaveAttachments" />
	/// </example>
	public interface IMimePart : IMimeEntity
	{
		/// <summary>
		/// Get or set the description of the content if available.
		/// </summary>
		/// <remarks>
		/// <para>The Content-Description header can be used to set a description of the content.</para>
		/// </remarks>
		/// <value>The description of the content.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is negative.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimePart"/> has been disposed.
		/// </exception>
		string ContentDescription {
			get; set;
		}

		/// <summary>
		/// Get or set the duration of the content if available.
		/// </summary>
		/// <remarks>
		/// <para>The Content-Duration header specifies duration of timed media,
		/// such as audio or video, in seconds.</para>
		/// </remarks>
		/// <value>The duration of the content.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is negative.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimePart"/> has been disposed.
		/// </exception>
		int? ContentDuration {
			get; set;
		}

		/// <summary>
		/// Get or set the md5sum of the content.
		/// </summary>
		/// <remarks>
		/// <para>The Content-MD5 header specifies the base64-encoded MD5 checksum of the content
		/// in its canonical format.</para>
		/// <para>For more information, see <a href="https://tools.ietf.org/html/rfc1864">rfc1864</a>.</para>
		/// </remarks>
		/// <value>The md5sum of the content.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimePart"/> has been disposed.
		/// </exception>
		string ContentMd5 {
			get; set;
		}

		/// <summary>
		/// Get or set the content transfer encoding.
		/// </summary>
		/// <remarks>
		/// The Content-Transfer-Encoding header specifies an auxiliary encoding
		/// that was applied to the content in order to allow it to pass through
		/// mail transport mechanisms (such as SMTP) which may have limitations
		/// in the byte ranges that it accepts. For example, many SMTP servers
		/// do not accept data outside of the 7-bit ASCII range and so sending
		/// binary attachments or even non-English text is not possible without
		/// applying an encoding such as base64 or quoted-printable.
		/// </remarks>
		/// <value>The content transfer encoding.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is not a valid content encoding.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimePart"/> has been disposed.
		/// </exception>
		ContentEncoding ContentTransferEncoding {
			get; set;
		}

		/// <summary>
		/// Get or set the name of the file.
		/// </summary>
		/// <remarks>
		/// <para>First checks for the "filename" parameter on the Content-Disposition header. If
		/// that does not exist, then the "name" parameter on the Content-Type header is used.</para>
		/// <para>When setting the filename, both the "filename" parameter on the Content-Disposition
		/// header and the "name" parameter on the Content-Type header are set.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\AttachmentExamples.cs" region="SaveAttachments" />
		/// </example>
		/// <value>The name of the file.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimePart"/> has been disposed.
		/// </exception>
		string FileName {
			get; set;
		}

		/// <summary>
		/// Get or set the MIME content.
		/// </summary>
		/// <remarks>
		/// Gets or sets the MIME content.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\AttachmentExamples.cs" region="SaveAttachments" />
		/// </example>
		/// <value>The MIME content.</value>
		IMimeContent Content {
			get; set;
		}

		/// <summary>
		/// Calculate the most efficient content encoding given the specified constraint.
		/// </summary>
		/// <remarks>
		/// If no <see cref="Content"/> is set, <see cref="ContentEncoding.SevenBit"/> will be returned.
		/// </remarks>
		/// <returns>The most efficient content encoding.</returns>
		/// <param name="constraint">The encoding constraint.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="constraint"/> is not a valid value.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimePart"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		ContentEncoding GetBestEncoding (EncodingConstraint constraint, CancellationToken cancellationToken = default);

		/// <summary>
		/// Calculate the most efficient content encoding given the specified constraint.
		/// </summary>
		/// <remarks>
		/// If no <see cref="Content"/> is set, <see cref="ContentEncoding.SevenBit"/> will be returned.
		/// </remarks>
		/// <returns>The most efficient content encoding.</returns>
		/// <param name="constraint">The encoding constraint.</param>
		/// <param name="maxLineLength">The maximum allowable length for a line (not counting the CRLF). Must be between <c>72</c> and <c>998</c> (inclusive).</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="maxLineLength"/> is not between <c>72</c> and <c>998</c> (inclusive).</para>
		/// <para>-or-</para>
		/// <para><paramref name="constraint"/> is not a valid value.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimePart"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		ContentEncoding GetBestEncoding (EncodingConstraint constraint, int maxLineLength, CancellationToken cancellationToken = default);

		/// <summary>
		/// Compute the MD5 checksum of the content.
		/// </summary>
		/// <remarks>
		/// Computes the MD5 checksum of the MIME content in its canonical
		/// format and then base64-encodes the result.
		/// </remarks>
		/// <returns>The md5sum of the content.</returns>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimePart"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="Content"/> is <c>null</c>.
		/// </exception>
		string ComputeContentMd5 ();

		/// <summary>
		/// Verify the Content-Md5 value against an independently computed md5sum.
		/// </summary>
		/// <remarks>
		/// Computes the MD5 checksum of the MIME content and compares it with the
		/// value in the Content-MD5 header, returning <c>true</c> if and only if
		/// the values match.
		/// </remarks>
		/// <returns><c>true</c>, if content MD5 checksum was verified, <c>false</c> otherwise.</returns>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMimePart"/> has been disposed.
		/// </exception>
		bool VerifyContentMd5 ();
	}
}

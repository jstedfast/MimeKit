//
// IMimeContent.cs
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
	/// An interface for content stream encapsulation as used by <see cref="MimePart"/>.
	/// </summary>
    /// <remarks>
    /// Implemented by <see cref="MimeContent"/>.
    /// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\AttachmentExamples.cs" region="SaveAttachments" />
	/// </example>
	public interface IMimeContent : IDisposable
	{
		/// <summary>
		/// Get the content encoding.
		/// </summary>
        /// <remarks>
        /// If the <see cref="Stream"/> is not encoded, this value will be
        /// <see cref="ContentEncoding.Default"/>. Otherwise, it will be
        /// set to the raw content encoding of the stream.
        /// </remarks>
		/// <value>The encoding.</value>
		ContentEncoding Encoding { get; }

		/// <summary>
		/// Get the new-line format, if known.
		/// </summary>
		/// <remarks>
		/// <para>This property is typically only set by the <see cref="MimeParser"/> as it parses
		/// the content of a <see cref="MimePart"/> and is only used as a hint when verifying
		/// digital signatures.</para>
		/// </remarks>
		/// <value>The new-line format, if known.</value>
		NewLineFormat? NewLineFormat { get; }

		/// <summary>
		/// Get the content stream.
		/// </summary>
		/// <remarks>
		/// Gets the content stream.
		/// </remarks>
		/// <value>The stream.</value>
		Stream Stream { get; }

		/// <summary>
		/// Open the decoded content stream.
		/// </summary>
		/// <remarks>
		/// Provides a means of reading the decoded content without having to first write it to another
		/// stream using <see cref="DecodeTo(System.IO.Stream,System.Threading.CancellationToken)"/>.
		/// </remarks>
		/// <returns>The decoded content stream.</returns>
		Stream Open ();

		/// <summary>
		/// Decode the content stream into another stream.
		/// </summary>
		/// <remarks>
		/// If the content stream is encoded, this method will decode it into the output stream
		/// using a suitable decoder based on the <see cref="Encoding"/> property, otherwise the
		/// stream will be copied into the output stream as-is.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\AttachmentExamples.cs" region="SaveAttachments" />
		/// </example>
		/// <param name="stream">The output stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		void DecodeTo (Stream stream, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously decode the content stream into another stream.
		/// </summary>
		/// <remarks>
		/// If the content stream is encoded, this method will decode it into the output stream
		/// using a suitable decoder based on the <see cref="Encoding"/> property, otherwise the
		/// stream will be copied into the output stream as-is.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\AttachmentExamples.cs" region="SaveAttachments" />
		/// </example>
		/// <returns>An awaitable task.</returns>
		/// <param name="stream">The output stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		Task DecodeToAsync (Stream stream, CancellationToken cancellationToken = default);

		/// <summary>
		/// Copy the content stream to the specified output stream.
		/// </summary>
		/// <remarks>
		/// <para>This is equivalent to simply using <see cref="System.IO.Stream.CopyTo(System.IO.Stream)"/>
		/// to copy the content stream to the output stream except that this method is cancellable.</para>
		/// <note type="note">If you want the decoded content, use
		/// <see cref="DecodeTo(System.IO.Stream,System.Threading.CancellationToken)"/> instead.</note>
		/// </remarks>
		/// <param name="stream">The output stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		void WriteTo (Stream stream, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously copy the content stream to the specified output stream.
		/// </summary>
		/// <remarks>
		/// <para>This is equivalent to simply using <see cref="System.IO.Stream.CopyTo(System.IO.Stream)"/>
		/// to copy the content stream to the output stream except that this method is cancellable.</para>
		/// <note type="note">If you want the decoded content, use
		/// <see cref="DecodeTo(System.IO.Stream,System.Threading.CancellationToken)"/> instead.</note>
		/// </remarks>
		/// <returns>An awaitable task.</returns>
		/// <param name="stream">The output stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		Task WriteToAsync (Stream stream, CancellationToken cancellationToken = default);
	}
}

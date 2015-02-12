//
// IContentObject.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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

using System.IO;
using System.Threading;

namespace MimeKit {
	/// <summary>
	/// An interface for content stream encapsulation as used by <see cref="MimeKit.MimePart"/>.
	/// </summary>
    /// <remarks>
    /// Implemented by <see cref="ContentObject"/>.
    /// </remarks>
	public interface IContentObject
	{
		/// <summary>
		/// Gets the content encoding.
		/// </summary>
        /// <remarks>
        /// If the <see cref="Stream"/> is not encoded, this value will be
        /// <see cref="ContentEncoding.Default"/>. Otherwise, it will be
        /// set to the raw content encoding of the stream.
        /// </remarks>
		/// <value>The encoding.</value>
		ContentEncoding Encoding { get; }

		/// <summary>
		/// Opens the decoded content stream.
		/// </summary>
		/// <remarks>
		/// Provides a means of reading the decoded content without having to first write it to another
		/// stream using <see cref="DecodeTo(System.IO.Stream,System.Threading.CancellationToken)"/>.
		/// </remarks>
		/// <returns>The decoded content stream.</returns>
		Stream Open ();

		/// <summary>
		/// Decodes the content stream into another stream.
		/// </summary>
        /// <remarks>
        /// If the content stream is encoded, this method will decode it into the
        /// output stream using a suitable decoder.
        /// </remarks>
		/// <param name="stream">The output stream.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		void DecodeTo (Stream stream, CancellationToken cancellationToken = default (CancellationToken));

		/// <summary>
		/// Copies the content stream to the specified output stream.
		/// </summary>
        /// <remarks>
        /// Copies the data from <see cref="Stream"/> into <paramref name="stream"/>.
        /// </remarks>
		/// <param name="stream">The output stream.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		void WriteTo (Stream stream, CancellationToken cancellationToken = default (CancellationToken));
	}
}

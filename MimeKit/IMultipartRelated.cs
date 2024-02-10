//
// IMultipartRelated.cs
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

namespace MimeKit {
	/// <summary>
	/// An interface for a multipart/related MIME entity.
	/// </summary>
	/// <remarks>
	/// A multipart/related MIME entity contains, as one might expect, inter-related MIME parts which
	/// typically reference each other via URIs based on the Content-Id and/or Content-Location headers.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
	/// </example>
	public interface IMultipartRelated : IMultipart
	{
		/// <summary>
		/// Get or set the root document of the multipart/related part and the appropriate Content-Type parameters.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the root document that references the other MIME parts within the multipart/related.</para>
		/// <para>When getting the root document, the <c>"start"</c> parameter of the Content-Type header is used to
		/// determine which of the parts is the root. If the <c>"start"</c> parameter does not exist or does not reference
		/// any of the child parts, then the first child is assumed to be the root.</para>
		/// <para>When setting the root document MIME part, the Content-Type header of the multipart/related part is also
		/// updated with a appropriate <c>"start"</c> and <c>"type"</c> parameters.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <value>The root MIME part.</value>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMultipartRelated"/> has been disposed.
		/// </exception>
		MimeEntity Root {
			get; set;
		}

		/// <summary>
		/// Check if the <see cref="IMultipartRelated"/> contains a part matching the specified URI.
		/// </summary>
		/// <remarks>
		/// Determines whether or not the multipart/related entity contains a part matching the specified URI.
		/// </remarks>
		/// <returns><value>true</value> if the specified part exists; otherwise <value>false</value>.</returns>
		/// <param name="uri">The URI of the MIME part.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="uri"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMultipartRelated"/> has been disposed.
		/// </exception>
		bool Contains (Uri uri);

		/// <summary>
		/// Get the index of the part matching the specified URI.
		/// </summary>
		/// <remarks>
		/// <para>Finds the index of the part matching the specified URI, if it exists.</para>
		/// <para>If the URI scheme is <c>"cid"</c>, then matching is performed based on the Content-Id header
		/// values, otherwise the Content-Location headers are used. If the provided URI is absolute and a child
		/// part's Content-Location is relative, then then the child part's Content-Location URI will be combined
		/// with the value of its Content-Base header, if available, otherwise it will be combined with the
		/// multipart/related part's Content-Base header in order to produce an absolute URI that can be
		/// compared with the provided absolute URI.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <returns>The index of the part matching the specified URI if found; otherwise <c>-1</c>.</returns>
		/// <param name="uri">The URI of the MIME part.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="uri"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMultipartRelated"/> has been disposed.
		/// </exception>
		int IndexOf (Uri uri);

		/// <summary>
		/// Open a stream for reading the decoded content of the MIME part specified by the provided URI.
		/// </summary>
		/// <remarks>
		/// Opens a stream for reading the decoded content of the MIME part specified by the provided URI.
		/// </remarks>
		/// <returns>A stream for reading the decoded content of the MIME part specified by the provided URI.</returns>
		/// <param name="uri">The URI.</param>
		/// <param name="mimeType">The mime-type of the content.</param>
		/// <param name="charset">The charset of the content (if the content is text-based)</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="uri"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The MIME part for the specified URI could not be found.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMultipartRelated"/> has been disposed.
		/// </exception>
		Stream Open (Uri uri, out string mimeType, out string charset);

		/// <summary>
		/// Open a stream for reading the decoded content of the MIME part specified by the provided URI.
		/// </summary>
		/// <remarks>
		/// Opens a stream for reading the decoded content of the MIME part specified by the provided URI.
		/// </remarks>
		/// <returns>A stream for reading the decoded content of the MIME part specified by the provided URI.</returns>
		/// <param name="uri">The URI.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="uri"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The MIME part for the specified URI could not be found.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMultipartRelated"/> has been disposed.
		/// </exception>
		Stream Open (Uri uri);
	}
}

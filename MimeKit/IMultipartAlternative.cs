//
// IMultipartAlternative.cs
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

using MimeKit.Text;

namespace MimeKit {
	/// <summary>
	/// An interface for a multipart/alternative MIME entity.
	/// </summary>
	/// <remarks>
	/// A multipart/alternative MIME entity contains, as one might expect, is used to offer a list of
	/// alternative formats for the main body of the message (usually they will be "text/plain" and
	/// "text/html"). These alternatives are in order of increasing faithfulness to the original document
	/// (in other words, the last entity will be in a format that, when rendered, will most closely match
	/// what the sending client's WYSISYG editor produced).
	/// </remarks>
	public interface IMultipartAlternative : IMultipart
	{
		/// <summary>
		/// Get the text of the text/plain alternative.
		/// </summary>
		/// <remarks>
		/// Gets the text of the text/plain alternative, if it exists.
		/// </remarks>
		/// <value>The text if a text/plain alternative exists; otherwise, <c>null</c>.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMultipartAlternative"/> has been disposed.
		/// </exception>
		string TextBody {
			get;
		}

		/// <summary>
		/// Get the HTML-formatted text of the text/html alternative.
		/// </summary>
		/// <remarks>
		/// Gets the HTML-formatted text of the text/html alternative, if it exists.
		/// </remarks>
		/// <value>The HTML if a text/html alternative exists; otherwise, <c>null</c>.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMultipartAlternative"/> has been disposed.
		/// </exception>
		string HtmlBody {
			get;
		}

		/// <summary>
		/// Get the text body in the specified format.
		/// </summary>
		/// <remarks>
		/// Gets the text body in the specified format, if it exists.
		/// </remarks>
		/// <returns>The text body in the desired format if it exists; otherwise, <c>null</c>.</returns>
		/// <param name="format">The desired text format.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMultipartAlternative"/> has been disposed.
		/// </exception>
		string GetTextBody (TextFormat format);
	}
}

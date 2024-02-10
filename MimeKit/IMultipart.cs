//
// IMultipart.cs
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

using System.Collections.Generic;

namespace MimeKit {
	/// <summary>
	/// An interface for a multipart MIME entity which may contain a collection of MIME entities.
	/// </summary>
	/// <remarks>
	/// <para>All multipart MIME entities will have a Content-Type with a media type of <c>"multipart"</c>.
	/// The most common multipart MIME entity used in email is the <c>"multipart/mixed"</c> entity.</para>
	/// <para>Four (4) initial subtypes were defined in the original MIME specifications: mixed, alternative,
	/// digest, and parallel.</para>
	/// <para>The "multipart/mixed" type is a sort of general-purpose container. When used in email, the
	/// first entity is typically the "body" of the message while additional entities are most often
	/// file attachments.</para>
	/// <para>Speaking of message "bodies", the "multipart/alternative" type is used to offer a list of
	/// alternative formats for the main body of the message (usually they will be "text/plain" and
	/// "text/html"). These alternatives are in order of increasing faithfulness to the original document
	/// (in other words, the last entity will be in a format that, when rendered, will most closely match
	/// what the sending client's WYSISYG editor produced).</para>
	/// <para>The "multipart/digest" type will typically contain a digest of MIME messages and is most
	/// commonly used by mailing-list software.</para>
	/// <para>The "multipart/parallel" type contains entities that are all meant to be shown (or heard)
	/// in parallel.</para>
	/// <para>Another commonly used type is the "multipart/related" type which contains, as one might expect,
	/// inter-related MIME parts which typically reference each other via URIs based on the Content-Id and/or
	/// Content-Location headers.</para>
	/// </remarks>
	public interface IMultipart : IMimeEntity, ICollection<MimeEntity>, IList<MimeEntity>
	{
		/// <summary>
		/// Get or set the boundary.
		/// </summary>
		/// <remarks>
		/// Gets or sets the boundary parameter on the Content-Type header.
		/// </remarks>
		/// <value>The boundary.</value>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		string Boundary {
			get; set;
		}

		/// <summary>
		/// Get or set the preamble.
		/// </summary>
		/// <remarks>
		/// A multipart preamble appears before the first child entity of the
		/// multipart and is typically used only in the top-level multipart
		/// of the message to specify that the message is in MIME format and
		/// therefore requires a MIME compliant email application to render
		/// it correctly.
		/// </remarks>
		/// <value>The preamble.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMultipart"/> has been disposed.
		/// </exception>
		string Preamble {
			get; set;
		}

		/// <summary>
		/// Get or set the epilogue.
		/// </summary>
		/// <remarks>
		/// A multipart epiloque is the text that appears after the closing boundary
		/// of the multipart and is typically either empty or a single new line
		/// character sequence.
		/// </remarks>
		/// <value>The epilogue.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMultipart"/> has been disposed.
		/// </exception>
		string Epilogue {
			get; set;
		}
	}
}

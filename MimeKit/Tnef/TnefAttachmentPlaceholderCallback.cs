//
// TnefAttachmentPlaceholderCallback.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
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

namespace MimeKit.Tnef {
	/// <summary>
	/// A TNEF attachment placeholder callback delegate.
	/// </summary>
	/// <remarks>
	/// <para>The <see cref="TnefAttachmentPlaceholderCallback"/> delegate is used by the TNEF to MIME conversion (see
	/// <see cref="TnefConversionOptions.AttachmentPlaceholderCallback"/>) to generate the text that is written in place
	/// of an attachment when the text/plain and text/html message bodies are generated from an RTF message body.
	/// Microsoft Outlook and Exchange mark the position at which each attachment is displayed in an RTF message body
	/// with a placeholder, as described by [MS-OXRTFEX] section 2.2.3.4.</para>
	/// <para>A typical implementation returns the attachment's file name or display name, for example
	/// <c>"&lt;&lt;" + (attachment.FileName ?? "attachment") + "&gt;&gt;"</c>.</para>
	/// </remarks>
	/// <param name="attachment">The TNEF attachment.</param>
	/// <param name="entity">The MIME entity that the attachment was converted to, or <see langword="null"/> if the
	/// attachment was not converted to a MIME entity of its own.</param>
	/// <returns>The text to write in place of the attachment, or <see langword="null"/> to write nothing.</returns>
	public delegate string? TnefAttachmentPlaceholderCallback (TnefAttachment attachment, MimeEntity? entity);
}
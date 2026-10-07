//
// RtfHtmlObjectPlaceholderCallback.cs
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

namespace MimeKit.Text {
	/// <summary>
	/// An RTF to HTML object placeholder callback delegate.
	/// </summary>
	/// <remarks>
	/// <para>The <see cref="RtfHtmlObjectPlaceholderCallback"/> delegate is called by <see cref="RtfToHtml"/> for each
	/// <c>\objattph</c> control word that it renders. Microsoft Outlook and Exchange use <c>\objattph</c> to mark the
	/// location in an RTF message body at which each of the message's attachments is rendered, as described by
	/// [MS-OXRTFEX] section 2.2.3.4.</para>
	/// <para>The placeholders are numbered from <c>0</c> in document order. [MS-OXRTFEX] matches them, in order, with
	/// the message's attachments that are not hidden and whose rendering position is not <c>0xFFFFFFFF</c>, sorted
	/// by rendering position. If the number of placeholders does not match the number of those attachments, the
	/// placeholders SHOULD be ignored and the attachments appended to the end of the body instead.</para>
	/// <para>The callback may write markup (for example, an <c>&lt;img&gt;</c> element that refers to the attachment
	/// using a <c>cid:</c> URL) to the <paramref name="htmlWriter"/>, or nothing at all.</para>
	/// </remarks>
	/// <param name="index">The zero-based index of the placeholder.</param>
	/// <param name="htmlWriter">The HTML writer.</param>
	public delegate void RtfHtmlObjectPlaceholderCallback (int index, HtmlWriter htmlWriter);
}
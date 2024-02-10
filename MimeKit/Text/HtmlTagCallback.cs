//
// HtmlTagCallback.cs
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

namespace MimeKit.Text {
	/// <summary>
	/// An HTML tag callback delegate.
	/// </summary>
	/// <remarks>
	/// The <see cref="HtmlTagCallback"/> delegate is called when a converter
	/// is ready to write a new HTML tag, allowing developers to customize
	/// whether the tag gets written at all, which attributes get written, etc.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
	/// </example>
	/// <param name="tagContext">The HTML tag context.</param>
	/// <param name="htmlWriter">The HTML writer.</param>
	public delegate void HtmlTagCallback (HtmlTagContext tagContext, HtmlWriter htmlWriter);
}

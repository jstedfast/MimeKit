//
// HtmlNoScriptHandling.cs
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
	/// An enumeration of the ways that <see cref="HtmlToHtml"/> can handle <c>&lt;noscript&gt;</c> elements.
	/// </summary>
	/// <remarks>
	/// <para>An enumeration of the ways that <see cref="HtmlToHtml"/> can handle <c>&lt;noscript&gt;</c> elements.</para>
	/// <para>The HTML5 specification tokenizes the content of a <c>&lt;noscript&gt;</c> element differently depending
	/// on whether scripting is enabled in the application that renders the HTML. If the <see cref="HtmlToHtml.HtmlTagCallback"/>
	/// is being used to filter the HTML, then the content must be tokenized the same way that the renderer will interpret it,
	/// or else markup could bypass the callback. See <see cref="HtmlToHtml.NoScriptHandling"/> for details.</para>
	/// </remarks>
	public enum HtmlNoScriptHandling {
		/// <summary>
		/// The content of <c>&lt;noscript&gt;</c> elements is tokenized as normal markup and passed to the
		/// <see cref="HtmlToHtml.HtmlTagCallback"/>, but the <c>&lt;noscript&gt;</c> start and end tags themselves are
		/// removed from the output. The resulting HTML is interpreted the same way regardless of whether scripting is
		/// enabled in the renderer. This is the default and the recommended value when the renderer is unknown.
		/// </summary>
		Unwrap,

		/// <summary>
		/// Tokenize the HTML as if scripting is enabled: the content of <c>&lt;noscript&gt;</c> elements is treated as
		/// raw text and written to the output verbatim <em>without</em> being passed to the
		/// <see cref="HtmlToHtml.HtmlTagCallback"/>. Only use this value if the output will always be rendered with
		/// scripting enabled.
		/// </summary>
		ScriptingEnabled,

		/// <summary>
		/// Tokenize the HTML as if scripting is disabled: the content of <c>&lt;noscript&gt;</c> elements is tokenized
		/// as normal markup and passed to the <see cref="HtmlToHtml.HtmlTagCallback"/>, and the <c>&lt;noscript&gt;</c>
		/// tags are also passed to the callback. Only use this value if the output will always be rendered with
		/// scripting disabled.
		/// </summary>
		ScriptingDisabled
	}
}

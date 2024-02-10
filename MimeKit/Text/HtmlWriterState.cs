//
// HtmlWriterState.cs
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
	/// An enumeration of possible states of a <see cref="HtmlWriter"/>.
	/// </summary>
	/// <remarks>
	/// An enumeration of possible states of a <see cref="HtmlWriter"/>.
	/// </remarks>
	public enum HtmlWriterState {
		/// <summary>
		/// The <see cref="HtmlWriter"/> is not within a tag. In this state, the <see cref="HtmlWriter"/>
		/// can only write a tag or text.
		/// </summary>
		Default,

		/// <summary>
		/// The <see cref="HtmlWriter"/> is inside a tag but has not started to write an attribute. In this
		/// state, the <see cref="HtmlWriter"/> can write an attribute, another tag, or text.
		/// </summary>
		Tag,

		/// <summary>
		/// The <see cref="HtmlWriter"/> is inside an attribute. In this state, the <see cref="HtmlWriter"/>
		/// can append a value to the current attribute, start the next attribute, or write another tag or text.
		/// </summary>
		Attribute
	}
}

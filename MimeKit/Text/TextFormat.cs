﻿//
// TextFormat.cs
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

namespace MimeKit.Text {
	/// <summary>
	/// An enumeration of text formats.
	/// </summary>
	/// <remarks>
	/// An enumeration of text formats.
	/// </remarks>
	public enum TextFormat {
		/// <summary>
		/// The plain text format.
		/// </summary>
		Text,

		/// <summary>
		/// The flowed text format (as described in rfc3676).
		/// </summary>
		Flowed,

		/// <summary>
		/// The HTML text format.
		/// </summary>
		Html,

		/// <summary>
		/// The enriched text format.
		/// </summary>
		Enriched,

		/// <summary>
		/// The compressed rich text format.
		/// </summary>
		CompressedRichText,

		/// <summary>
		/// The rich text format.
		/// </summary>
		RichText,
	}
}

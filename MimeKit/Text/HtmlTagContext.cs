//
// HtmlTagContext.cs
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

namespace MimeKit.Text {
	/// <summary>
	/// An HTML tag context.
	/// </summary>
	/// <remarks>
	/// An HTML tag context used with the <see cref="HtmlTagCallback"/> delegate.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
	/// </example>
	public abstract class HtmlTagContext
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="HtmlTagContext"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="HtmlTagContext"/>.
		/// </remarks>
		/// <param name="tagId">The HTML tag identifier.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="tagId"/> is invalid.
		/// </exception>
		protected HtmlTagContext (HtmlTagId tagId)
		{
			TagId = tagId;
		}

		/// <summary>
		/// Get the HTML tag attributes.
		/// </summary>
		/// <remarks>
		/// Gets the HTML tag attributes.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <value>The attributes.</value>
		public abstract HtmlAttributeCollection Attributes {
			get;
		}

		/// <summary>
		/// Get or set whether or not the end tag should be deleted.
		/// </summary>
		/// <remarks>
		/// Gets or sets whether or not the end tag should be deleted.
		/// </remarks>
		/// <value><c>true</c> if the end tag should be deleted; otherwise, <c>false</c>.</value>
		public bool DeleteEndTag {
			get; set;
		}

		/// <summary>
		/// Get or set whether or not the tag should be deleted.
		/// </summary>
		/// <remarks>
		/// Gets or sets whether or not the tag should be deleted.
		/// </remarks>
		/// <value><c>true</c> if the tag should be deleted; otherwise, <c>false</c>.</value>
		public bool DeleteTag {
			get; set;
		}

		/// <summary>
		/// Get or set whether or not the <see cref="HtmlTagCallback"/> should be invoked for the end tag.
		/// </summary>
		/// <remarks>
		/// Gets or sets whether or not the <see cref="HtmlTagCallback"/> should be invoked for the end tag.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <value><c>true</c> if the callback should be invoked for end tag; otherwise, <c>false</c>.</value>
		public bool InvokeCallbackForEndTag {
			get; set;
		}

		/// <summary>
		/// Get whether or not the tag is an empty element.
		/// </summary>
		/// <remarks>
		/// Gets whether or not the tag is an empty element.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <value><c>true</c> if the tag is an empty element; otherwise, <c>false</c>.</value>
		public abstract bool IsEmptyElementTag {
			get;
		}

		/// <summary>
		/// Get whether or not the tag is an end tag.
		/// </summary>
		/// <remarks>
		/// Gets whether or not the tag is an end tag.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <value><c>true</c> if the tag is an end tag; otherwise, <c>false</c>.</value>
		public abstract bool IsEndTag {
			get;
		}

		/// <summary>
		/// Get or set whether or not the inner content of the tag should be suppressed.
		/// </summary>
		/// <remarks>
		/// Gets or sets whether or not the inner content of the tag should be suppressed.
		/// </remarks>
		/// <value><c>true</c> if the inner content should be suppressed; otherwise, <c>false</c>.</value>
		public bool SuppressInnerContent {
			get; set;
		}

		/// <summary>
		/// Get the HTML tag identifier.
		/// </summary>
		/// <remarks>
		/// Gets the HTML tag identifier.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <value>The HTML tag identifier.</value>
		public HtmlTagId TagId {
			get; private set;
		}

		/// <summary>
		/// Get the HTML tag name.
		/// </summary>
		/// <remarks>
		/// Gets the HTML tag name.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <value>The HTML tag name.</value>
		public abstract string TagName {
			get;
		}

		/// <summary>
		/// Write the HTML tag.
		/// </summary>
		/// <remarks>
		/// Writes the HTML tag to the given <see cref="HtmlWriter"/>.
		/// </remarks>
		/// <param name="htmlWriter">The HTML writer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="htmlWriter"/> is <c>null</c>.
		/// </exception>
		public void WriteTag (HtmlWriter htmlWriter)
		{
			WriteTag (htmlWriter, false);
		}

		/// <summary>
		/// Write the HTML tag.
		/// </summary>
		/// <remarks>
		/// Writes the HTML tag to the given <see cref="HtmlWriter"/>.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <param name="htmlWriter">The HTML writer.</param>
		/// <param name="writeAttributes"><c>true</c> if the <see cref="Attributes"/> should also be written; otherwise, <c>false</c>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="htmlWriter"/> is <c>null</c>.
		/// </exception>
		public void WriteTag (HtmlWriter htmlWriter, bool writeAttributes)
		{
			if (htmlWriter is null)
				throw new ArgumentNullException (nameof (htmlWriter));

			if (IsEndTag) {
				htmlWriter.WriteEndTag (TagName);
				return;
			}

			if (IsEmptyElementTag)
				htmlWriter.WriteEmptyElementTag (TagName);
			else
				htmlWriter.WriteStartTag (TagName);

			if (writeAttributes) {
				for (int i = 0; i < Attributes.Count; i++)
					htmlWriter.WriteAttribute (Attributes[i]);
			}
		}
	}
}

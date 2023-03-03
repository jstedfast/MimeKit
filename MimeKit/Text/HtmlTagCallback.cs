// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

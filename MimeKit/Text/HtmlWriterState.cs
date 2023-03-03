// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

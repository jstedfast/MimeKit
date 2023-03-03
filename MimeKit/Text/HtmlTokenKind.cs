// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MimeKit.Text {
	/// <summary>
	/// The kinds of tokens that the <see cref="HtmlTokenizer"/> can emit.
	/// </summary>
	/// <remarks>
	/// The kinds of tokens that the <see cref="HtmlTokenizer"/> can emit.
	/// </remarks>
	public enum HtmlTokenKind {
		/// <summary>
		/// A token consisting of <c>[CDATA[</c>.
		/// </summary>
		CData,

		/// <summary>
		/// An HTML comment token.
		/// </summary>
		Comment,

		/// <summary>
		/// A token consisting of character data.
		/// </summary>
		Data,

		/// <summary>
		/// An HTML DOCTYPE token.
		/// </summary>
		DocType,

		/// <summary>
		/// A token consisting of script data.
		/// </summary>
		ScriptData,

		/// <summary>
		/// An HTML tag token.
		/// </summary>
		Tag,
	}
}

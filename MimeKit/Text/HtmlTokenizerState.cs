//
// HtmlTokenizerState.cs
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
	/// The HTML tokenizer state.
	/// </summary>
	/// <remarks>
	/// The HTML tokenizer state.
	/// </remarks>
	public enum HtmlTokenizerState {
		/// <summary>
		/// The data state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#data-state">
		/// http://www.w3.org/TR/html5/syntax.html#data-state</a>.
		/// </summary>
		Data,

		/// <summary>
		/// The character reference in data state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#character-reference-in-data-state">
		/// http://www.w3.org/TR/html5/syntax.html#character-reference-in-data-state</a>.
		/// </summary>
		CharacterReferenceInData,

		/// <summary>
		/// The RCDATA state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#rcdata-state">
		/// http://www.w3.org/TR/html5/syntax.html#rcdata-state</a>.
		/// </summary>
		RcData,

		/// <summary>
		/// The character reference in RCDATA state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#character-reference-in-rcdata-state">
		/// http://www.w3.org/TR/html5/syntax.html#character-reference-in-rcdata-state</a>.
		/// </summary>
		CharacterReferenceInRcData,

		/// <summary>
		/// The RAWTEXT state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#rawtext-state">
		/// http://www.w3.org/TR/html5/syntax.html#rawtext-state</a>.
		/// </summary>
		RawText,

		/// <summary>
		/// The script data state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#script-data-state">
		/// http://www.w3.org/TR/html5/syntax.html#script-data-state</a>.
		/// </summary>
		ScriptData,

		/// <summary>
		/// The PLAINTEXT state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#plaintext-state">
		/// http://www.w3.org/TR/html5/syntax.html#plaintext-state</a>.
		/// </summary>
		PlainText,

		/// <summary>
		/// The tag open state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#tag-open-state">
		/// http://www.w3.org/TR/html5/syntax.html#tag-open-state</a>.
		/// </summary>
		TagOpen,

		/// <summary>
		/// The end tag open state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#end-tag-open-state">
		/// http://www.w3.org/TR/html5/syntax.html#end-tag-open-state</a>.
		/// </summary>
		EndTagOpen,

		/// <summary>
		/// The tag name state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#tag-name-state">
		/// http://www.w3.org/TR/html5/syntax.html#tag-name-state</a>.
		/// </summary>
		TagName,

		/// <summary>
		/// The RCDATA less-than state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#rcdata-less-than-sign-state">
		/// http://www.w3.org/TR/html5/syntax.html#rcdata-less-than-sign-state</a>.
		/// </summary>
		RcDataLessThan,

		/// <summary>
		/// The RCDATA end tag open state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#rcdata-end-tag-open-state">
		/// http://www.w3.org/TR/html5/syntax.html#rcdata-end-tag-open-state</a>.
		/// </summary>
		RcDataEndTagOpen,

		/// <summary>
		/// The RCDATA end tag name state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#rcdata-end-tag-name-state">
		/// http://www.w3.org/TR/html5/syntax.html#rcdata-end-tag-name-state</a>.
		/// </summary>
		RcDataEndTagName,

		/// <summary>
		/// The RAWTEXT less-than state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#rawtext-less-than-sign-state">
		/// http://www.w3.org/TR/html5/syntax.html#rawtext-less-than-sign-state</a>.
		/// </summary>
		RawTextLessThan,

		/// <summary>
		/// The RAWTEXT end tag open state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#rawtext-end-tag-open-state">
		/// http://www.w3.org/TR/html5/syntax.html#rawtext-end-tag-open-state</a>.
		/// </summary>
		RawTextEndTagOpen,

		/// <summary>
		/// The RAWTEXT end tag name state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#rawtext-end-tag-name-state">
		/// http://www.w3.org/TR/html5/syntax.html#rawtext-end-tag-name-state</a>.
		/// </summary>
		RawTextEndTagName,

		/// <summary>
		/// The script data less-than state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#script-data-less-than-sign-state">
		/// http://www.w3.org/TR/html5/syntax.html#script-data-less-than-sign-state</a>.
		/// </summary>
		ScriptDataLessThan,

		/// <summary>
		/// The script data end tag open state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#script-data-end-tag-open-state">
		/// http://www.w3.org/TR/html5/syntax.html#script-data-end-tag-open-state</a>.
		/// </summary>
		ScriptDataEndTagOpen,

		/// <summary>
		/// The script data end tag name state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#script-data-end-tag-name-state">
		/// http://www.w3.org/TR/html5/syntax.html#script-data-end-tag-name-state</a>.
		/// </summary>
		ScriptDataEndTagName,

		/// <summary>
		/// The script data escape start state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#script-data-escape-start-state">
		/// http://www.w3.org/TR/html5/syntax.html#script-data-escape-start-state</a>.
		/// </summary>
		ScriptDataEscapeStart,

		/// <summary>
		/// The script data escape start state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#script-data-escape-start-dash-state">
		/// http://www.w3.org/TR/html5/syntax.html#script-data-escape-start-dash-state</a>.
		/// </summary>
		ScriptDataEscapeStartDash,

		/// <summary>
		/// The script data escaped state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#script-data-escaped-state">
		/// http://www.w3.org/TR/html5/syntax.html#script-data-escaped-state</a>.
		/// </summary>
		ScriptDataEscaped,

		/// <summary>
		/// The script data escaped dash state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#script-data-escaped-dash-state">
		/// http://www.w3.org/TR/html5/syntax.html#script-data-escaped-dash-state</a>.
		/// </summary>
		ScriptDataEscapedDash,

		/// <summary>
		/// The script data escaped dash dash state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#script-data-escaped-dash-dash-state">
		/// http://www.w3.org/TR/html5/syntax.html#script-data-escaped-dash-dash-state</a>.
		/// </summary>
		ScriptDataEscapedDashDash,

		/// <summary>
		/// The script data escaped less-than state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#script-data-escaped-less-than-sign-state">
		/// http://www.w3.org/TR/html5/syntax.html#script-data-escaped-less-than-sign-state</a>.
		/// </summary>
		ScriptDataEscapedLessThan,

		/// <summary>
		/// The script data escaped end tag open state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#script-data-escaped-end-tag-open-state">
		/// http://www.w3.org/TR/html5/syntax.html#script-data-escaped-end-tag-open-state</a>.
		/// </summary>
		ScriptDataEscapedEndTagOpen,

		/// <summary>
		/// The script data escaped end tag name state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#script-data-escaped-end-tag-name-state">
		/// http://www.w3.org/TR/html5/syntax.html#script-data-escaped-end-tag-name-state</a>.
		/// </summary>
		ScriptDataEscapedEndTagName,

		/// <summary>
		/// The script data double escape start state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#script-data-double-escape-start-state">
		/// http://www.w3.org/TR/html5/syntax.html#script-data-double-escape-start-state</a>.
		/// </summary>
		ScriptDataDoubleEscapeStart,

		/// <summary>
		/// The script data double escaped state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#script-data-double-escaped-state">
		/// http://www.w3.org/TR/html5/syntax.html#script-data-double-escaped-state</a>.
		/// </summary>
		ScriptDataDoubleEscaped,

		/// <summary>
		/// The script data double escaped dash state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#script-data-double-escaped-dash-state">
		/// http://www.w3.org/TR/html5/syntax.html#script-data-double-escaped-dash-state</a>.
		/// </summary>
		ScriptDataDoubleEscapedDash,

		/// <summary>
		/// The script data double escaped dash dash state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#script-data-double-escaped-dash-dash-state">
		/// http://www.w3.org/TR/html5/syntax.html#script-data-double-escaped-dash-dash-state</a>.
		/// </summary>
		ScriptDataDoubleEscapedDashDash,

		/// <summary>
		/// The script data double escaped less-than state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#script-data-double-escaped-less-than-sign-state">
		/// http://www.w3.org/TR/html5/syntax.html#script-data-double-escaped-less-than-sign-state</a>.
		/// </summary>
		ScriptDataDoubleEscapedLessThan,

		/// <summary>
		/// The script data double escape end state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#script-data-double-escape-end-state">
		/// http://www.w3.org/TR/html5/syntax.html#script-data-double-escape-end-state</a>.
		/// </summary>
		ScriptDataDoubleEscapeEnd,

		/// <summary>
		/// The before attribute name state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#before-attribute-name-state">
		/// http://www.w3.org/TR/html5/syntax.html#before-attribute-name-state</a>.
		/// </summary>
		BeforeAttributeName,

		/// <summary>
		/// The attribute name state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#attribute-name-state">
		/// http://www.w3.org/TR/html5/syntax.html#attribute-name-state</a>.
		/// </summary>
		AttributeName,

		/// <summary>
		/// The after attribute name state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#after-attribute-name-state">
		/// http://www.w3.org/TR/html5/syntax.html#after-attribute-name-state</a>.
		/// </summary>
		AfterAttributeName,

		/// <summary>
		/// The beforw attribute value state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#before-attribute-value-state">
		/// http://www.w3.org/TR/html5/syntax.html#before-attribute-value-state</a>.
		/// </summary>
		BeforeAttributeValue,

		/// <summary>
		/// The attribute value quoted state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#attribute-value-(double-quoted)-state">
		/// http://www.w3.org/TR/html5/syntax.html#attribute-value-(double-quoted)-state</a>.
		/// </summary>
		AttributeValueQuoted,

		/// <summary>
		/// The attribute value unquoted state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#attribute-value-(unquoted)-state">
		/// http://www.w3.org/TR/html5/syntax.html#attribute-value-(unquoted)-state</a>.
		/// </summary>
		AttributeValueUnquoted,

		/// <summary>
		/// The character reference in attribute value state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#character-reference-in-attribute-value-state">
		/// http://www.w3.org/TR/html5/syntax.html#character-reference-in-attribute-value-state</a>.
		/// </summary>
		CharacterReferenceInAttributeValue,

		/// <summary>
		/// The after attribute value quoted state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#after-attribute-value-(quoted)-state">
		/// http://www.w3.org/TR/html5/syntax.html#after-attribute-value-(quoted)-state</a>.
		/// </summary>
		AfterAttributeValueQuoted,

		/// <summary>
		/// The self-closing start tag state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#self-closing-start-tag-state">
		/// http://www.w3.org/TR/html5/syntax.html#self-closing-start-tag-state</a>.
		/// </summary>
		SelfClosingStartTag,

		/// <summary>
		/// The bogus comment state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#bogus-comment-state">
		/// http://www.w3.org/TR/html5/syntax.html#bogus-comment-state</a>.
		/// </summary>
		BogusComment,

		/// <summary>
		/// The markup declaration open state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#markup-declaration-open-state">
		/// http://www.w3.org/TR/html5/syntax.html#markup-declaration-open-state</a>.
		/// </summary>
		MarkupDeclarationOpen,

		/// <summary>
		/// The comment start state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#comment-start-state">
		/// http://www.w3.org/TR/html5/syntax.html#comment-start-state</a>.
		/// </summary>
		CommentStart,

		/// <summary>
		/// The comment start dash state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#comment-start-dash-state">
		/// http://www.w3.org/TR/html5/syntax.html#comment-start-dash-state</a>.
		/// </summary>
		CommentStartDash,

		/// <summary>
		/// The comment state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#comment-state">
		/// http://www.w3.org/TR/html5/syntax.html#comment-state</a>.
		/// </summary>
		Comment,

		/// <summary>
		/// The comment end dash state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#comment-end-dash-state">
		/// http://www.w3.org/TR/html5/syntax.html#comment-end-dash-state</a>.
		/// </summary>
		CommentEndDash,

		/// <summary>
		/// The comment end state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#comment-end-state">
		/// http://www.w3.org/TR/html5/syntax.html#comment-end-state</a>.
		/// </summary>
		CommentEnd,

		/// <summary>
		/// The comment end bang state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#comment-end-bang-state">
		/// http://www.w3.org/TR/html5/syntax.html#comment-end-bang-state</a>.
		/// </summary>
		CommentEndBang,

		/// <summary>
		/// The DOCTYPE state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#doctype-state">
		/// http://www.w3.org/TR/html5/syntax.html#doctype-state</a>.
		/// </summary>
		DocType,

		/// <summary>
		/// The before DOCTYPE name state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#before-doctype-name-state">
		/// http://www.w3.org/TR/html5/syntax.html#before-doctype-name-state</a>.
		/// </summary>
		BeforeDocTypeName,

		/// <summary>
		/// The DOCTYPE name state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#doctype-name-state">
		/// http://www.w3.org/TR/html5/syntax.html#doctype-name-state</a>.
		/// </summary>
		DocTypeName,

		/// <summary>
		/// The after DOCTYPE name state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#after-doctype-name-state">
		/// http://www.w3.org/TR/html5/syntax.html#after-doctype-name-state</a>.
		/// </summary>
		AfterDocTypeName,

		/// <summary>
		/// The after DOCTYPE public keyword state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#after-doctype-public-keyword-state">
		/// http://www.w3.org/TR/html5/syntax.html#after-doctype-public-keyword-state</a>.
		/// </summary>
		AfterDocTypePublicKeyword,

		/// <summary>
		/// The before DOCTYPE public identifier state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#before-doctype-public-identifier-state">
		/// http://www.w3.org/TR/html5/syntax.html#before-doctype-public-identifier-state</a>.
		/// </summary>
		BeforeDocTypePublicIdentifier,

		/// <summary>
		/// The DOCTYPE public identifier quoted state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#doctype-public-identifier-(double-quoted)-state">
		/// http://www.w3.org/TR/html5/syntax.html#doctype-public-identifier-(double-quoted)-state</a>.
		/// </summary>
		DocTypePublicIdentifierQuoted,

		/// <summary>
		/// The after DOCTYPE public identifier state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#after-doctype-public-identifier-state">
		/// http://www.w3.org/TR/html5/syntax.html#after-doctype-public-identifier-state</a>.
		/// </summary>
		AfterDocTypePublicIdentifier,

		/// <summary>
		/// The between DOCTYPE public and system identifiers state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#between-doctype-public-and-system-identifiers-state">
		/// http://www.w3.org/TR/html5/syntax.html#between-doctype-public-and-system-identifiers-state</a>.
		/// </summary>
		BetweenDocTypePublicAndSystemIdentifiers,

		/// <summary>
		/// The after DOCTYPE system keyword state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#after-doctype-system-keyword-state">
		/// http://www.w3.org/TR/html5/syntax.html#after-doctype-system-keyword-state</a>.
		/// </summary>
		AfterDocTypeSystemKeyword,

		/// <summary>
		/// The before DOCTYPE system identifier state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#before-doctype-system-identifier-state">
		/// http://www.w3.org/TR/html5/syntax.html#before-doctype-system-identifier-state</a>.
		/// </summary>
		BeforeDocTypeSystemIdentifier,

		/// <summary>
		/// The DOCTYPE system identifier quoted state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#doctype-system-identifier-(double-quoted)-state">
		/// http://www.w3.org/TR/html5/syntax.html#doctype-system-identifier-(double-quoted)-state</a>.
		/// </summary>
		DocTypeSystemIdentifierQuoted,

		/// <summary>
		/// The after DOCTYPE system identifier state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#after-doctype-system-identifier-state">
		/// http://www.w3.org/TR/html5/syntax.html#after-doctype-system-identifier-state</a>.
		/// </summary>
		AfterDocTypeSystemIdentifier,

		/// <summary>
		/// The bogus DOCTYPE state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#bogus-doctype-state">
		/// http://www.w3.org/TR/html5/syntax.html#bogus-doctype-state</a>.
		/// </summary>
		BogusDocType,

		/// <summary>
		/// The CDATA section state as described at
		/// <a href="http://www.w3.org/TR/html5/syntax.html#cdata-section-state">
		/// http://www.w3.org/TR/html5/syntax.html#cdata-section-state</a>.
		/// </summary>
		CDataSection,

		/// <summary>
		/// The end of file state.
		/// </summary>
		EndOfFile
	}
}

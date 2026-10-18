//
// HtmlComplianceViolation.cs
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
	/// An enumeration of potential HTML compliance violations.
	/// </summary>
	/// <remarks>
	/// <para>An enumeration of the parse errors defined by the tokenization section of the HTML Living
	/// Standard, as reported to an <see cref="IHtmlComplianceLogger"/>.</para>
	/// <para>Parse errors that describe the input stream itself (such as control characters,
	/// noncharacters and surrogates that appear literally in the text) are not reported.</para>
	/// <para>A handful of members do not describe a defect in the document at all, but rather a
	/// limit that the document exceeded. They are reported the same way so that a consumer can
	/// tell that some of the content was not processed.</para>
	/// </remarks>
	public enum HtmlComplianceViolation
	{
		// Note: The members below are grouped by subject matter so that the enumeration reads well,
		// but the numeric values are append-only. C# inlines enum constants into consuming
		// assemblies at compile time, so renumbering an existing member silently changes the meaning
		// of code that was compiled against an earlier version of MimeKit, as well as any value that
		// has been persisted or logged. A new violation therefore belongs in whichever #region
		// covers its subject matter, but it must take the next unused value rather than the value
		// that its position would suggest.

		/// <summary>
		/// No violation.
		/// </summary>
		/// <remarks>
		/// This value is never reported. It exists so that the default value of an
		/// <see cref="HtmlComplianceIssue"/> is distinguishable from a genuine violation.
		/// </remarks>
		None                                                    = 0,

		#region Tags

		/// <summary>
		/// The input ended immediately after a &lt; or &lt;/.
		/// </summary>
		/// <remarks>
		/// A &lt; or &lt;/ at the very end of the input cannot start a tag, so it is treated as text.
		/// </remarks>
		EofBeforeTagName                                        = 1,

		/// <summary>
		/// A &lt; or &lt;/ was followed by a character that cannot start a tag name.
		/// </summary>
		/// <remarks>
		/// A tag name must begin with an ASCII letter. After &lt;, the &lt; is treated as text; after &lt;/, the
		/// content up to the next &gt; is treated as a bogus comment, which browsers do not render.
		/// </remarks>
		InvalidFirstCharacterOfTagName                          = 2,

		/// <summary>
		/// A &lt; was followed by a question mark.
		/// </summary>
		/// <remarks>
		/// Markup such as an XML processing instruction (&lt;?xml ...?&gt;) is not valid in HTML. The content up to
		/// the next &gt; is treated as a bogus comment, which browsers do not render.
		/// </remarks>
		UnexpectedQuestionMarkInsteadOfTagName                  = 3,

		/// <summary>
		/// An end tag had no name (&lt;/&gt;).
		/// </summary>
		/// <remarks>
		/// The &lt;/&gt; is ignored entirely; it neither closes an element nor is rendered as text.
		/// </remarks>
		MissingEndTagName                                       = 4,

		/// <summary>
		/// The input ended inside of a tag.
		/// </summary>
		/// <remarks>
		/// The input ended before the tag was closed by a &gt;. The incomplete tag is dropped by browsers, so
		/// whatever followed the point of truncation is lost. Depending on the value of <see
		/// cref="HtmlTokenizer.IgnoreTruncatedTags"/>, the tokenizer either drops it as well or returns it as text.
		/// </remarks>
		EofInTag                                                = 5,

		/// <summary>
		/// A / inside of a tag was not immediately followed by &gt;.
		/// </summary>
		/// <remarks>
		/// A / within a tag is only meaningful immediately before the closing &gt;. Elsewhere it is ignored and
		/// treated as whitespace between attributes, which makes it a common way to separate attributes from a tag
		/// name (e.g. &lt;img/src=...&gt;) without using whitespace.
		/// </remarks>
		UnexpectedSolidusInTag                                  = 6,

		/// <summary>
		/// An end tag had attributes.
		/// </summary>
		/// <remarks>
		/// End tags cannot have attributes, so browsers ignore them.
		/// </remarks>
		EndTagWithAttributes                                    = 7,

		/// <summary>
		/// An end tag had a trailing /.
		/// </summary>
		/// <remarks>
		/// End tags cannot be self-closing, so browsers ignore the /.
		/// </remarks>
		EndTagWithTrailingSolidus                               = 8,

		#endregion

		#region Attributes

		/// <summary>
		/// A tag had more than one attribute with the same name.
		/// </summary>
		/// <remarks>
		/// Browsers keep the first occurrence of an attribute and ignore the rest, but other HTML processors may
		/// keep the last one instead. A filter or scanner that examines a different occurrence than the browser
		/// renders can be bypassed.
		/// </remarks>
		DuplicateAttribute                                      = 9,

		/// <summary>
		/// An attribute name and = were not followed by a value.
		/// </summary>
		/// <remarks>
		/// An attribute such as &lt;a href=&gt; is treated as having an empty value.
		/// </remarks>
		MissingAttributeValue                                   = 10,

		/// <summary>
		/// A quoted attribute value was not followed by whitespace.
		/// </summary>
		/// <remarks>
		/// Attributes must be separated by whitespace. Browsers treat the next character as the start of a new
		/// attribute regardless, but other HTML processors may not, so the attributes they see may differ from the
		/// attributes that the browser renders.
		/// </remarks>
		MissingWhitespaceBetweenAttributes                      = 11,

		/// <summary>
		/// An attribute name contained a ", ', or &lt; character.
		/// </summary>
		/// <remarks>
		/// These characters are not valid within an attribute name, but browsers treat them as part of the name.
		/// They usually indicate broken quoting, and other HTML processors may split the tag into attributes
		/// differently than the browser does.
		/// </remarks>
		UnexpectedCharacterInAttributeName                      = 12,

		/// <summary>
		/// An unquoted attribute value contained a ", ', &lt;, =, or ` character.
		/// </summary>
		/// <remarks>
		/// These characters are not valid within an unquoted attribute value, but browsers treat them as part of
		/// the value. They usually indicate broken quoting, and some older browsers treat ` as a quote character,
		/// so different HTML processors may disagree about where the value ends.
		/// </remarks>
		UnexpectedCharacterInUnquotedAttributeValue             = 13,

		/// <summary>
		/// An attribute name began with =.
		/// </summary>
		/// <remarks>
		/// Browsers treat the = as the first character of the attribute name, but other HTML processors may treat
		/// it as the separator between a name and a value.
		/// </remarks>
		UnexpectedEqualsSignBeforeAttributeName                 = 14,

		#endregion

		#region Comments

		/// <summary>
		/// A comment was closed by &lt;!--&gt; or &lt;!---&gt;.
		/// </summary>
		/// <remarks>
		/// Browsers treat &lt;!--&gt; and &lt;!---&gt; as complete, empty comments, but other HTML processors may
		/// treat them as the start of a comment that continues up to the next --&gt;, hiding the content in between
		/// from them but not from the browser.
		/// </remarks>
		AbruptClosingOfEmptyComment                             = 15,

		/// <summary>
		/// The input ended inside of a comment.
		/// </summary>
		/// <remarks>
		/// Browsers treat everything from the start of an unterminated comment to the end of the input as part of
		/// the comment, so none of it is rendered. Other HTML processors may render it instead.
		/// </remarks>
		EofInComment                                            = 16,

		/// <summary>
		/// A comment was closed by --!&gt; instead of --&gt;.
		/// </summary>
		/// <remarks>
		/// Browsers accept --!&gt; as the end of a comment, but other HTML processors may not, and continue the
		/// comment up to the next --&gt;, hiding the content in between from them but not from the browser.
		/// </remarks>
		IncorrectlyClosedComment                                = 17,

		/// <summary>
		/// A &lt;! was not followed by --, DOCTYPE or [CDATA[.
		/// </summary>
		/// <remarks>
		/// Browsers treat the content up to the next &gt; as a bogus comment, which they do not render. Other HTML
		/// processors may handle it differently. This is often the result of a conditional comment such as &lt;![if
		/// !mso]&gt;.
		/// </remarks>
		IncorrectlyOpenedComment                                = 18,

		/// <summary>
		/// A comment contained &lt;!--.
		/// </summary>
		/// <remarks>
		/// Comments do not nest; the first --&gt; closes the comment regardless of how many &lt;!-- precede it. A
		/// processor that believes comments nest will consider content to be commented out that a browser renders.
		/// </remarks>
		NestedComment                                           = 19,

		#endregion

		#region Document Type

		/// <summary>
		/// A DOCTYPE public identifier was terminated by &gt; before its closing quote.
		/// </summary>
		/// <remarks>
		/// The DOCTYPE ends at the &gt;, the public identifier is truncated, and the document is rendered in quirks
		/// mode.
		/// </remarks>
		AbruptDocTypePublicIdentifier                           = 20,

		/// <summary>
		/// A DOCTYPE system identifier was terminated by &gt; before its closing quote.
		/// </summary>
		/// <remarks>
		/// The DOCTYPE ends at the &gt;, the system identifier is truncated, and the document is rendered in quirks
		/// mode.
		/// </remarks>
		AbruptDocTypeSystemIdentifier                           = 21,

		/// <summary>
		/// The input ended inside of a DOCTYPE.
		/// </summary>
		/// <remarks>
		/// The DOCTYPE is closed at the end of the input and the document is rendered in quirks mode.
		/// </remarks>
		EofInDocType                                            = 22,

		/// <summary>
		/// A DOCTYPE name was followed by something other than PUBLIC, SYSTEM or &gt;.
		/// </summary>
		/// <remarks>
		/// The remainder of the DOCTYPE is ignored and the document is rendered in quirks mode.
		/// </remarks>
		InvalidCharacterSequenceAfterDocTypeName                = 23,

		/// <summary>
		/// A DOCTYPE had no name.
		/// </summary>
		/// <remarks>
		/// A DOCTYPE without a name causes the document to be rendered in quirks mode.
		/// </remarks>
		MissingDocTypeName                                      = 24,

		/// <summary>
		/// The PUBLIC keyword of a DOCTYPE was not followed by a public identifier.
		/// </summary>
		/// <remarks>
		/// The DOCTYPE is closed and the document is rendered in quirks mode.
		/// </remarks>
		MissingDocTypePublicIdentifier                          = 25,

		/// <summary>
		/// The SYSTEM keyword of a DOCTYPE was not followed by a system identifier.
		/// </summary>
		/// <remarks>
		/// The DOCTYPE is closed and the document is rendered in quirks mode.
		/// </remarks>
		MissingDocTypeSystemIdentifier                          = 26,

		/// <summary>
		/// A DOCTYPE public identifier was not quoted.
		/// </summary>
		/// <remarks>
		/// The remainder of the DOCTYPE is ignored and the document is rendered in quirks mode.
		/// </remarks>
		MissingQuoteBeforeDocTypePublicIdentifier               = 27,

		/// <summary>
		/// A DOCTYPE system identifier was not quoted.
		/// </summary>
		/// <remarks>
		/// The remainder of the DOCTYPE is ignored and the document is rendered in quirks mode.
		/// </remarks>
		MissingQuoteBeforeDocTypeSystemIdentifier               = 28,

		/// <summary>
		/// The PUBLIC keyword of a DOCTYPE was not followed by whitespace.
		/// </summary>
		/// <remarks>
		/// Browsers parse the public identifier regardless.
		/// </remarks>
		MissingWhitespaceAfterDocTypePublicKeyword              = 29,

		/// <summary>
		/// The SYSTEM keyword of a DOCTYPE was not followed by whitespace.
		/// </summary>
		/// <remarks>
		/// Browsers parse the system identifier regardless.
		/// </remarks>
		MissingWhitespaceAfterDocTypeSystemKeyword              = 30,

		/// <summary>
		/// The DOCTYPE keyword was not followed by whitespace.
		/// </summary>
		/// <remarks>
		/// Browsers parse the DOCTYPE name regardless.
		/// </remarks>
		MissingWhitespaceBeforeDocTypeName                      = 31,

		/// <summary>
		/// The public and system identifiers of a DOCTYPE were not separated by whitespace.
		/// </summary>
		/// <remarks>
		/// Browsers parse the system identifier regardless.
		/// </remarks>
		MissingWhitespaceBetweenDocTypePublicAndSystemIdentifiers = 32,

		/// <summary>
		/// A DOCTYPE system identifier was followed by something other than whitespace or &gt;.
		/// </summary>
		/// <remarks>
		/// The remainder of the DOCTYPE is ignored. Unlike most other DOCTYPE errors, this does not cause the
		/// document to be rendered in quirks mode.
		/// </remarks>
		UnexpectedCharacterAfterDocTypeSystemIdentifier         = 33,

		#endregion

		#region Character References

		/// <summary>
		/// A numeric character reference (&amp;# or &amp;#x) had no digits.
		/// </summary>
		/// <remarks>
		/// The reference is not decoded and is rendered as text.
		/// </remarks>
		AbsenceOfDigitsInNumericCharacterReference              = 34,

		/// <summary>
		/// A numeric character reference referred to a code point beyond U+10FFFF.
		/// </summary>
		/// <remarks>
		/// There is no such character, so the reference is decoded as U+FFFD REPLACEMENT CHARACTER.
		/// </remarks>
		CharacterReferenceOutsideUnicodeRange                   = 35,

		/// <summary>
		/// A numeric character reference referred to a control character.
		/// </summary>
		/// <remarks>
		/// Control characters other than ASCII whitespace are not allowed in HTML. For historical reasons,
		/// references to the C1 controls (U+0080 through U+009F) are decoded as the windows-1252 characters with
		/// the same values, and other control characters are decoded as-is.
		/// </remarks>
		ControlCharacterReference                               = 36,

		/// <summary>
		/// A character reference was not terminated by a semicolon.
		/// </summary>
		/// <remarks>
		/// Browsers decode numeric references and a legacy set of named references (such as &amp;amp and &amp;copy)
		/// regardless, except within attribute values when a named reference is followed by = or an ASCII letter or
		/// digit (e.g. &amp;copy=1 in a URL). Other HTML processors may decode them differently.
		/// </remarks>
		MissingSemicolonAfterCharacterReference                 = 37,

		/// <summary>
		/// A numeric character reference referred to a Unicode noncharacter.
		/// </summary>
		/// <remarks>
		/// Noncharacters are permanently reserved for internal use and are not allowed in HTML. Browsers decode the
		/// reference as-is regardless.
		/// </remarks>
		NoncharacterCharacterReference                          = 38,

		/// <summary>
		/// A numeric character reference referred to U+0000 NULL.
		/// </summary>
		/// <remarks>
		/// NULL characters are not allowed in HTML, so the reference is decoded as U+FFFD REPLACEMENT CHARACTER.
		/// </remarks>
		NullCharacterReference                                  = 39,

		/// <summary>
		/// A numeric character reference referred to a UTF-16 surrogate code point.
		/// </summary>
		/// <remarks>
		/// Surrogate code points are not characters, so the reference is decoded as U+FFFD REPLACEMENT CHARACTER.
		/// </remarks>
		SurrogateCharacterReference                             = 40,

		/// <summary>
		/// A named character reference terminated by a semicolon did not match any known name.
		/// </summary>
		/// <remarks>
		/// The reference is not decoded and is rendered as text.
		/// </remarks>
		UnknownNamedCharacterReference                          = 41,

		#endregion

		#region Content

		/// <summary>
		/// A CDATA section appeared outside of SVG or MathML content.
		/// </summary>
		/// <remarks>
		/// CDATA sections are only allowed within SVG and MathML content. Elsewhere, browsers treat &lt;![CDATA[ as
		/// the start of a bogus comment that ends at the next &gt; rather than at ]]&gt;, so other HTML processors
		/// may disagree about where it ends and what content it hides.
		/// </remarks>
		CDataInHtmlContent                                      = 42,

		/// <summary>
		/// The input ended inside of a CDATA section.
		/// </summary>
		/// <remarks>
		/// The CDATA section is closed at the end of the input. The input was most likely truncated, and whatever
		/// followed the point of truncation is lost.
		/// </remarks>
		EofInCData                                              = 43,

		/// <summary>
		/// The input ended inside of an HTML comment-like construct within a script element.
		/// </summary>
		/// <remarks>
		/// Within a &lt;script&gt; element, text such as &lt;!-- &lt;script&gt; changes how the end of the script
		/// is detected, so the script may continue to the end of the input, consuming content that a different
		/// processor would render as markup.
		/// </remarks>
		EofInScriptHtmlCommentLikeText                          = 44,

		/// <summary>
		/// The input contained a U+0000 NULL character.
		/// </summary>
		/// <remarks>
		/// NULL characters are not allowed in HTML. Depending on where they appear, browsers either ignore them or
		/// replace them with U+FFFD REPLACEMENT CHARACTER, while other HTML processors may truncate the content at
		/// the NULL.
		/// </remarks>
		UnexpectedNullCharacter                                 = 45,

		#endregion

		#region Tree Construction

		/// <summary>
		/// A start tag was ignored.
		/// </summary>
		/// <remarks>
		/// <para>Browsers do not insert an element for some start tags that appear where they are not allowed, e.g. a second
		/// &lt;head&gt;, a &lt;frameset&gt; after the body content has started, a nested &lt;form&gt;, a table structure
		/// start tag such as &lt;tr&gt; outside of a table, or any start tag other than &lt;html&gt;, &lt;frameset&gt;,
		/// &lt;frame&gt; or &lt;noframes&gt; in a frameset document.</para>
		/// <para>Since the ignored start tag does not change how the content that follows it gets tokenized, other HTML
		/// processors that do honor it may disagree about the structure of the document. The issue is reported at the
		/// &gt; that ends the start tag.</para>
		/// </remarks>
		IgnoredStartTag                                         = 47,

		/// <summary>
		/// A tag closed SVG or MathML elements by breaking out of foreign content.
		/// </summary>
		/// <remarks>
		/// <para>Within SVG or MathML content (other than at an integration point), start tags such as &lt;p&gt;,
		/// &lt;b&gt;, &lt;img&gt; or &lt;font color&gt;, as well as the &lt;/p&gt; and &lt;/br&gt; end tags, implicitly close
		/// all of the open SVG and MathML elements and get processed as HTML.</para>
		/// <para>Since this changes whether the content that follows gets tokenized as HTML or as foreign content (e.g. how
		/// &lt;style&gt; or &lt;![CDATA[ are handled), it is a common technique for smuggling markup past HTML sanitizers.
		/// The issue is reported at the &gt; that ends the tag.</para>
		/// </remarks>
		ForeignContentBreakout                                  = 48,

		/// <summary>
		/// The attributes of a start tag were added to an existing element.
		/// </summary>
		/// <remarks>
		/// <para>An &lt;html&gt; or &lt;body&gt; start tag that appears after the element has already been inserted (explicitly
		/// or implicitly) does not insert another element. Instead, browsers add any of its attributes that the existing
		/// element does not already have to that element.</para>
		/// <para>This allows attributes such as event handlers to be attached to the document's &lt;body&gt; element from
		/// anywhere in the document, which HTML processors that discard the duplicate start tag will not expect. The issue
		/// is reported at the &gt; that ends the start tag.</para>
		/// </remarks>
		AttributesMergedIntoExistingElement                     = 49,

		#endregion

		#region Limits

		/// <summary>
		/// Elements were nested more deeply than the configured limit.
		/// </summary>
		/// <remarks>
		/// This is not a defect in the document, but the remainder of the input was treated as text rather than
		/// markup.
		/// </remarks>
		ElementDepthLimitExceeded                               = 46,

		#endregion

		/// <summary>
		/// Too many compliance issues were detected.
		/// </summary>
		/// <remarks>
		/// <para>This is not a defect in the document. It is reported once, in place of the first issue
		/// that was suppressed because its violation had already been reported the maximum number of
		/// times allowed by <see cref="HtmlComplianceOptions.MaxIssuesPerViolation"/>.</para>
		/// <para>Its value lies outside of the range of the other members so that it is never mistaken
		/// for a violation that a later version of MimeKit adds.</para>
		/// </remarks>
		TooManyComplianceIssues                                 = int.MaxValue
	}
}

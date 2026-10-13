//
// RtfComplianceViolation.cs
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
	/// An enumeration of potential RTF compliance violations.
	/// </summary>
	/// <remarks>
	/// <para>An enumeration of the ways in which a Rich Text Format (RTF) document may deviate from the
	/// Microsoft Rich Text Format (RTF) Specification, Version 1.9.1, as reported to an
	/// <see cref="IRtfComplianceLogger"/>.</para>
	/// <para>A handful of members do not describe a defect in the document at all, but rather a
	/// limit that the document exceeded. They are reported the same way so that a consumer can
	/// tell that some of the content was not processed.</para>
	/// </remarks>
	public enum RtfComplianceViolation
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
		/// <see cref="RtfComplianceIssue"/> is distinguishable from a genuine violation.
		/// </remarks>
		None                                        = 0,

		#region Document Structure

		/// <summary>
		/// The document did not begin with the <c>{\rtf</c> header.
		/// </summary>
		/// <remarks>
		/// RTF 1.9.1 requires that a document begin with an opening brace followed by the <c>\rtfN</c>
		/// control word. The content is interpreted as RTF regardless, but other readers may reject the
		/// document or treat it as plain text.
		/// </remarks>
		MissingRtfHeader                            = 1,

		/// <summary>
		/// A group end (<c>}</c>) did not match any group start.
		/// </summary>
		/// <remarks>
		/// Every <c>}</c> must close a group that was opened by a matching <c>{</c>. The extra
		/// <c>}</c> is ignored.
		/// </remarks>
		UnbalancedGroupEnd                          = 2,

		/// <summary>
		/// Content followed the end of the outermost group.
		/// </summary>
		/// <remarks>
		/// <para>RTF 1.9.1 defines a document as a single group. Content that follows the <c>}</c> that
		/// closes that group is rendered, but readers disagree about whether to render it, so it may be
		/// shown to some recipients and not to others.</para>
		/// <para>Trailing whitespace and NUL characters are not reported.</para>
		/// </remarks>
		ContentAfterDocument                        = 3,

		/// <summary>
		/// The document ended before all of its groups were closed.
		/// </summary>
		/// <remarks>
		/// The document ended within a group, or within the binary data of a <c>\binN</c> control word.
		/// It was most likely truncated in transit or in storage, and whatever followed the point of
		/// truncation is lost.
		/// </remarks>
		TruncatedStream                             = 4,

		#endregion

		#region Lexical

		/// <summary>
		/// A control word was longer than 32 letters.
		/// </summary>
		/// <remarks>
		/// RTF 1.9.1 limits a control word to 32 letters. A longer control word cannot be valid, so it
		/// is treated as an unknown control word and ignored.
		/// </remarks>
		KeywordTooLong                              = 5,

		/// <summary>
		/// A control word had a malformed or out-of-range numeric parameter.
		/// </summary>
		/// <remarks>
		/// RTF 1.9.1 defines a control word parameter as an optional hyphen followed by digits, forming a
		/// signed 16-bit or 32-bit integer. A hyphen that is not followed by any digits is ignored, and a
		/// value that is out of range is clamped or replaced, which may not be what the writer intended.
		/// </remarks>
		InvalidParameter                            = 6,

		/// <summary>
		/// A <c>\'</c> escape was not followed by two hexadecimal digits.
		/// </summary>
		/// <remarks>
		/// RTF 1.9.1 requires that <c>\'</c> be followed by exactly two hexadecimal digits. An escape
		/// without any hexadecimal digits is dropped, and one with a single hexadecimal digit is decoded
		/// using that digit alone, so the character is either lost or decoded incorrectly.
		/// </remarks>
		InvalidHexEscape                            = 7,

		/// <summary>
		/// The document contained a non-ASCII character that was not escaped.
		/// </summary>
		/// <remarks>
		/// RTF is a 7-bit format, so characters outside of the ASCII range are supposed to be written
		/// using <c>\'hh</c> or <c>\uN</c> escapes. Unescaped 8-bit characters are common in practice
		/// and are decoded using the code page of the current font, exactly like <c>\'hh</c>, but they
		/// do not survive transports that are not 8-bit clean.
		/// </remarks>
		UnescapedEightBitCharacter                  = 8,

		/// <summary>
		/// A <c>\uN</c> control word specified one half of a UTF-16 surrogate pair without the other.
		/// </summary>
		/// <remarks>
		/// RTF 1.9.1 writes characters outside of the Basic Multilingual Plane as a pair of <c>\uN</c>
		/// control words holding the high and low surrogates. A surrogate without its other half does
		/// not represent a character, so it is replaced with U+FFFD REPLACEMENT CHARACTER.
		/// </remarks>
		UnpairedSurrogate                           = 9,

		#endregion

		#region Character Formatting

		/// <summary>
		/// A code page specified by the document could not be resolved.
		/// </summary>
		/// <remarks>
		/// <para>Text is decoded using the code page specified by <c>\ansicpgN</c>, <c>\fcharsetN</c> or
		/// <c>\cpgN</c>. When that code page is not available, the text is decoded using a fallback
		/// encoding instead, which may decode it incorrectly.</para>
		/// <para>On .NET Core and .NET 5 or later, most Windows code pages are only available after the
		/// application has registered <c>System.Text.CodePagesEncodingProvider</c>, so this may reflect
		/// the environment rather than the document.</para>
		/// </remarks>
		UnsupportedCodePage                         = 10,

		/// <summary>
		/// A <c>\fN</c> control word referred to a font that was not defined in the font table.
		/// </summary>
		/// <remarks>
		/// The font determines the code page that text is decoded with. Text formatted with an undefined
		/// font is decoded using the document's default code page, which may not be what the writer
		/// intended.
		/// </remarks>
		UndefinedFont                               = 11,

		/// <summary>
		/// A <c>\cfN</c> or <c>\cbN</c> control word referred to a color that was not defined in the color table.
		/// </summary>
		/// <remarks>
		/// Text formatted with an undefined color is rendered using the automatic (default) color.
		/// </remarks>
		UndefinedColor                              = 12,

		/// <summary>
		/// A <c>\fsN</c> control word specified a font size that was not positive or was too large.
		/// </summary>
		/// <remarks>
		/// RTF 1.9.1 measures font sizes in half-points and Microsoft Word limits them to 1638 points. A
		/// font size of zero is a well-known way to hide text from the reader of a message while leaving
		/// it visible to a content scanner, and readers disagree about how to render such text.
		/// </remarks>
		InvalidFontSize                             = 13,

		/// <summary>
		/// A field instruction was longer than the maximum length that is examined.
		/// </summary>
		/// <remarks>
		/// Field instructions are only examined for <c>HYPERLINK</c> targets, so only the first 2048
		/// characters are kept. A longer instruction is truncated, so a hyperlink may be lost or its
		/// target may be truncated.
		/// </remarks>
		FieldInstructionTooLong                     = 14,

		#endregion

		#region Limits

		/// <summary>
		/// The font table contained more entries than the configured limit.
		/// </summary>
		/// <remarks>
		/// This is not a defect in the document, but the font table entries beyond the limit were
		/// ignored, so text formatted with those fonts may have been decoded incorrectly.
		/// </remarks>
		FontTableLimitExceeded                      = 15,

		/// <summary>
		/// The color table contained more entries than the configured limit.
		/// </summary>
		/// <remarks>
		/// This is not a defect in the document, but the color table entries beyond the limit were
		/// ignored, so text formatted with those colors was rendered using the automatic color.
		/// </remarks>
		ColorTableLimitExceeded                     = 16,

		/// <summary>
		/// Groups that change the formatting state were nested more deeply than the configured limit.
		/// </summary>
		/// <remarks>
		/// This is not a defect in the document, but the content of the groups beyond the limit was
		/// not processed.
		/// </remarks>
		GroupDepthLimitExceeded                     = 17,

		#endregion

		#region Reporting

		/// <summary>
		/// Too many compliance issues were detected and the remainder were suppressed.
		/// </summary>
		/// <remarks>
		/// <para>This is reported once, at the point where some violation first exceeds the configured
		/// per-violation limit, so that a report that has been truncated is never mistaken for a
		/// complete one.</para>
		/// <para>It is not a defect and does not belong in the catalogue of them, so it is deliberately
		/// parked at the end of the range, leaving the defect values contiguous and free to grow.</para>
		/// </remarks>
		TooManyComplianceIssues                     = int.MaxValue,

		#endregion
	}
}

//
// RtfComplianceIssue.cs
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

using System;
using System.Text;
using System.Globalization;

namespace MimeKit.Text {
	/// <summary>
	/// An RTF compliance issue detected while converting.
	/// </summary>
	/// <remarks>
	/// <para>Describes a single deviation from the Rich Text Format (RTF) Specification along with the
	/// location within the document where it was detected.</para>
	/// <para>New properties may be added to this structure in future versions of MimeKit in order to
	/// provide richer context about a violation. For that reason, always construct instances using one
	/// of the available constructors rather than relying on the default value.</para>
	/// <para>The default value of this structure has a <see cref="Violation"/> of
	/// <see cref="RtfComplianceViolation.None"/>, which is never reported and cannot be constructed,
	/// so it may be used to detect an uninitialized instance.</para>
	/// </remarks>
	public readonly struct RtfComplianceIssue : IEquatable<RtfComplianceIssue>
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="RtfComplianceIssue"/> struct.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="RtfComplianceIssue"/>.
		/// </remarks>
		/// <param name="violation">The specific RTF compliance violation that occurred.</param>
		/// <param name="offset">The character offset within the document where the violation was found.</param>
		/// <param name="lineNumber">The line number where the violation was found.</param>
		/// <param name="linePosition">The position within the line where the violation was found.</param>
		/// <param name="depth">The group nesting depth where the violation was found.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="violation"/> is not a valid <see cref="RtfComplianceViolation"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="offset"/> is negative.</para>
		/// <para>-or-</para>
		/// <para><paramref name="lineNumber"/> is less than <c>1</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="linePosition"/> is less than <c>1</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="depth"/> is negative.</para>
		/// </exception>
		public RtfComplianceIssue (RtfComplianceViolation violation, long offset, int lineNumber, int linePosition, int depth)
		{
			// Note: TooManyComplianceIssues is not part of the contiguous range of defects, so it is
			// checked separately.
			if ((violation <= RtfComplianceViolation.None || violation > RtfComplianceViolation.GroupDepthLimitExceeded) && violation != RtfComplianceViolation.TooManyComplianceIssues)
				throw new ArgumentOutOfRangeException (nameof (violation));

			if (offset < 0)
				throw new ArgumentOutOfRangeException (nameof (offset));

			if (lineNumber < 1)
				throw new ArgumentOutOfRangeException (nameof (lineNumber));

			if (linePosition < 1)
				throw new ArgumentOutOfRangeException (nameof (linePosition));

			if (depth < 0)
				throw new ArgumentOutOfRangeException (nameof (depth));

			Violation = violation;
			Offset = offset;
			LineNumber = lineNumber;
			LinePosition = linePosition;
			Depth = depth;
		}

		/// <summary>
		/// Get the specific RTF compliance violation that occurred.
		/// </summary>
		/// <remarks>
		/// Gets the specific RTF compliance violation that occurred.
		/// </remarks>
		/// <value>The RTF compliance violation.</value>
		public RtfComplianceViolation Violation {
			get;
		}

		/// <summary>
		/// Get the character offset within the document where the violation was found.
		/// </summary>
		/// <remarks>
		/// <para>Gets the zero-based offset, in characters, of the token where the violation was found,
		/// relative to the start of the document.</para>
		/// <para>The RTF converters read their input as Latin-1 by default, in which case this is also
		/// the byte offset.</para>
		/// </remarks>
		/// <value>The character offset.</value>
		public long Offset {
			get;
		}

		/// <summary>
		/// Get the line number where the violation was found.
		/// </summary>
		/// <remarks>
		/// Gets the one-based line number of the token where the violation was found. Lines are
		/// delimited by line feed characters, including any within <c>\binN</c> data.
		/// </remarks>
		/// <value>The line number.</value>
		public int LineNumber {
			get;
		}

		/// <summary>
		/// Get the position within the line where the violation was found.
		/// </summary>
		/// <remarks>
		/// Gets the one-based character position, within the line, of the token where the violation
		/// was found.
		/// </remarks>
		/// <value>The line position.</value>
		public int LinePosition {
			get;
		}

		/// <summary>
		/// Get the group nesting depth where the violation was found.
		/// </summary>
		/// <remarks>
		/// <para>Gets the number of groups that enclose the token where the violation was found. The
		/// content of a well-formed document is at a depth of at least <c>1</c>, because the document
		/// itself is a group.</para>
		/// <para>A group start or group end is at the depth of the group that encloses it.</para>
		/// </remarks>
		/// <value>The group nesting depth.</value>
		public int Depth {
			get;
		}

		/// <summary>
		/// Get the severity of the RTF compliance violation.
		/// </summary>
		/// <remarks>
		/// Gets how much practical harm the violation is likely to cause.
		/// </remarks>
		/// <value>The severity.</value>
		public MimeComplianceSeverity Severity {
			get { return GetSeverity (Violation); }
		}

		/// <summary>
		/// Get the categories of harm that the RTF compliance violation may cause.
		/// </summary>
		/// <remarks>
		/// <para>Gets what kind of harm the violation may cause, as opposed to <see cref="Severity"/>,
		/// which rates how much.</para>
		/// <para>A violation may fall into more than one category, so this is a bit field.</para>
		/// </remarks>
		/// <value>The categories.</value>
		public MimeComplianceCategories Categories {
			get { return GetCategories (Violation); }
		}

		/// <summary>
		/// Get a brief description of the RTF compliance violation.
		/// </summary>
		/// <remarks>
		/// Gets a brief, single-sentence description of what went wrong. Use <see cref="Remarks"/>
		/// for a longer explanation of why it matters.
		/// </remarks>
		/// <value>The description.</value>
		public string Description {
			get { return GetDescription (Violation); }
		}

		/// <summary>
		/// Get a detailed explanation of the RTF compliance violation.
		/// </summary>
		/// <remarks>
		/// Gets a detailed explanation of what the RTF specification requires and the problems that
		/// the violation is likely to cause.
		/// </remarks>
		/// <value>The remarks.</value>
		public string Remarks {
			get { return GetRemarks (Violation); }
		}

		/// <summary>
		/// Get a string representation of the RTF compliance issue.
		/// </summary>
		/// <remarks>
		/// Gets a string representation of the RTF compliance issue.
		/// </remarks>
		/// <returns>A string representation of the RTF compliance issue.</returns>
		public override string ToString ()
		{
			var builder = new StringBuilder ();

			builder.Append (Violation);
			builder.AppendFormat (CultureInfo.InvariantCulture, " at line {0}, position {1} (offset {2})", LineNumber, LinePosition, Offset);

			if (Depth > 0)
				builder.AppendFormat (CultureInfo.InvariantCulture, " (depth {0})", Depth);

			return builder.ToString ();
		}

		/// <summary>
		/// Determine whether the specified <see cref="RtfComplianceIssue"/> is equal to the current <see cref="RtfComplianceIssue"/>.
		/// </summary>
		/// <remarks>
		/// Determines whether the specified <see cref="RtfComplianceIssue"/> is equal to the current <see cref="RtfComplianceIssue"/>.
		/// </remarks>
		/// <param name="other">The <see cref="RtfComplianceIssue"/> to compare with the current <see cref="RtfComplianceIssue"/>.</param>
		/// <returns><see langword="true" /> if the specified <see cref="RtfComplianceIssue"/> is equal to the current
		/// <see cref="RtfComplianceIssue"/>; otherwise, <see langword="false" />.</returns>
		public bool Equals (RtfComplianceIssue other)
		{
			return other.Violation == Violation && other.Offset == Offset && other.LineNumber == LineNumber &&
				other.LinePosition == LinePosition && other.Depth == Depth;
		}

		/// <summary>
		/// Determine whether the specified <see cref="System.Object"/> is equal to the current <see cref="RtfComplianceIssue"/>.
		/// </summary>
		/// <remarks>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="RtfComplianceIssue"/>.
		/// </remarks>
		/// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="RtfComplianceIssue"/>.</param>
		/// <returns><see langword="true" /> if the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="RtfComplianceIssue"/>; otherwise, <see langword="false" />.</returns>
		public override bool Equals (object? obj)
		{
			return obj is RtfComplianceIssue other && Equals (other);
		}

		/// <summary>
		/// Serve as a hash function for a <see cref="RtfComplianceIssue"/> object.
		/// </summary>
		/// <remarks>
		/// Serves as a hash function for a <see cref="RtfComplianceIssue"/> object.
		/// </remarks>
		/// <returns>A hash code for this instance that is suitable for use in hashing algorithms
		/// and data structures such as a hash table.</returns>
		public override int GetHashCode ()
		{
			return Violation.GetHashCode () ^ Offset.GetHashCode () ^ (LineNumber << 16) ^ LinePosition ^ (Depth << 8);
		}

		/// <summary>
		/// Compare two <see cref="RtfComplianceIssue"/> objects for equality.
		/// </summary>
		/// <remarks>
		/// Compares two <see cref="RtfComplianceIssue"/> objects for equality.
		/// </remarks>
		/// <param name="left">The first issue to compare.</param>
		/// <param name="right">The second issue to compare.</param>
		/// <returns><see langword="true" /> if the two issues are equal; otherwise, <see langword="false" />.</returns>
		public static bool operator == (RtfComplianceIssue left, RtfComplianceIssue right)
		{
			return left.Equals (right);
		}

		/// <summary>
		/// Compare two <see cref="RtfComplianceIssue"/> objects for inequality.
		/// </summary>
		/// <remarks>
		/// Compares two <see cref="RtfComplianceIssue"/> objects for inequality.
		/// </remarks>
		/// <param name="left">The first issue to compare.</param>
		/// <param name="right">The second issue to compare.</param>
		/// <returns><see langword="true" /> if the two issues are not equal; otherwise, <see langword="false" />.</returns>
		public static bool operator != (RtfComplianceIssue left, RtfComplianceIssue right)
		{
			return !left.Equals (right);
		}

		/// <summary>
		/// Get the severity of an RTF compliance violation.
		/// </summary>
		/// <remarks>
		/// Gets how much practical harm the violation is likely to cause.
		/// </remarks>
		/// <returns>The severity.</returns>
		/// <param name="violation">The RTF compliance violation.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="violation"/> is not a valid <see cref="RtfComplianceViolation"/>.
		/// </exception>
		public static MimeComplianceSeverity GetSeverity (RtfComplianceViolation violation)
		{
			switch (violation) {
			// Note: Lexical sloppiness that RTF readers routinely tolerate in the same way, and that
			// loses at most a single character or control word.
			case RtfComplianceViolation.MissingRtfHeader:
			case RtfComplianceViolation.UnbalancedGroupEnd:
			case RtfComplianceViolation.KeywordTooLong:
			case RtfComplianceViolation.InvalidParameter:
			case RtfComplianceViolation.InvalidHexEscape:
			case RtfComplianceViolation.UnescapedEightBitCharacter:
			case RtfComplianceViolation.UnpairedSurrogate:
			// Note: These fall back to the defaults, which changes how text looks but not what it says.
			case RtfComplianceViolation.UndefinedFont:
			case RtfComplianceViolation.UndefinedColor:
				return MimeComplianceSeverity.Minor;

			// Note: Content that some readers render and others do not, or that is not shown to the
			// reader at all, means that what a content scanner examined may not be what the recipient
			// sees.
			case RtfComplianceViolation.ContentAfterDocument:
			case RtfComplianceViolation.InvalidFontSize:
				return MimeComplianceSeverity.Major;

			// Note: Content that was lost or may have been decoded incorrectly.
			case RtfComplianceViolation.TruncatedStream:
			case RtfComplianceViolation.UnsupportedCodePage:
			case RtfComplianceViolation.FieldInstructionTooLong:
				return MimeComplianceSeverity.Major;

			// Note: These are not defects in the document, but some of its content was not processed,
			// and a consumer that only looks at Major and above needs to know that.
			case RtfComplianceViolation.FontTableLimitExceeded:
			case RtfComplianceViolation.ColorTableLimitExceeded:
			case RtfComplianceViolation.GroupDepthLimitExceeded:
			case RtfComplianceViolation.TooManyComplianceIssues:
				return MimeComplianceSeverity.Major;

			default:
				throw new ArgumentOutOfRangeException (nameof (violation));
			}
		}

		/// <summary>
		/// Get the categories of harm that an RTF compliance violation may cause.
		/// </summary>
		/// <remarks>
		/// <para>Gets what kind of harm the violation may cause, as opposed to
		/// <see cref="GetSeverity(RtfComplianceViolation)"/>, which rates how much.</para>
		/// <para>A violation may fall into more than one category, so this is a bit field.</para>
		/// </remarks>
		/// <returns>The categories.</returns>
		/// <param name="violation">The RTF compliance violation.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="violation"/> is not a valid <see cref="RtfComplianceViolation"/>.
		/// </exception>
		public static MimeComplianceCategories GetCategories (RtfComplianceViolation violation)
		{
			const MimeComplianceCategories Interop = MimeComplianceCategories.Interoperability;
			const MimeComplianceCategories DataLoss = MimeComplianceCategories.DataLoss;
			const MimeComplianceCategories Security = MimeComplianceCategories.Security;

			switch (violation) {
			case RtfComplianceViolation.MissingRtfHeader:
			case RtfComplianceViolation.UnbalancedGroupEnd:
			case RtfComplianceViolation.KeywordTooLong:
			case RtfComplianceViolation.InvalidParameter:
			case RtfComplianceViolation.UnescapedEightBitCharacter:
			case RtfComplianceViolation.UndefinedFont:
			case RtfComplianceViolation.UndefinedColor:
				return Interop;

			// Note: A character is lost or decoded incorrectly.
			case RtfComplianceViolation.InvalidHexEscape:
			case RtfComplianceViolation.UnpairedSurrogate:
			case RtfComplianceViolation.UnsupportedCodePage:
				return Interop | DataLoss;

			// Note: Different readers may render the content differently, or not at all, so what a
			// scanner examines may not be what the recipient is shown.
			case RtfComplianceViolation.ContentAfterDocument:
			case RtfComplianceViolation.InvalidFontSize:
				return Interop | Security;

			// Note: Neither of these is likely to be interpreted differently by different readers,
			// but content has been lost.
			case RtfComplianceViolation.TruncatedStream:
			case RtfComplianceViolation.FieldInstructionTooLong:
				return DataLoss;

			// Note: The document did not cause these, so they do not harm interoperability, but content
			// that went unprocessed is content that went unscanned.
			case RtfComplianceViolation.FontTableLimitExceeded:
			case RtfComplianceViolation.ColorTableLimitExceeded:
			case RtfComplianceViolation.GroupDepthLimitExceeded:
				return DataLoss | Security;

			// Note: A suppressed report is a blind spot, and hiding a violation from whatever is
			// inspecting the document is exactly what an attacker would want out of a flood of cheap ones.
			case RtfComplianceViolation.TooManyComplianceIssues:
				return Security;

			default:
				throw new ArgumentOutOfRangeException (nameof (violation));
			}
		}

		/// <summary>
		/// Get a brief description of an RTF compliance violation.
		/// </summary>
		/// <remarks>
		/// Gets a brief, single-sentence description of what went wrong. Use
		/// <see cref="GetRemarks(RtfComplianceViolation)"/> for a longer explanation of why it matters.
		/// </remarks>
		/// <returns>The description.</returns>
		/// <param name="violation">The RTF compliance violation.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="violation"/> is not a valid <see cref="RtfComplianceViolation"/>.
		/// </exception>
		public static string GetDescription (RtfComplianceViolation violation)
		{
			switch (violation) {
			case RtfComplianceViolation.MissingRtfHeader:
				return "The document did not begin with the {\\rtf header.";
			case RtfComplianceViolation.UnbalancedGroupEnd:
				return "A group end did not match any group start.";
			case RtfComplianceViolation.ContentAfterDocument:
				return "Content followed the end of the outermost group.";
			case RtfComplianceViolation.TruncatedStream:
				return "The document ended before all of its groups were closed.";
			case RtfComplianceViolation.KeywordTooLong:
				return "A control word was longer than 32 letters.";
			case RtfComplianceViolation.InvalidParameter:
				return "A control word had a malformed or out-of-range numeric parameter.";
			case RtfComplianceViolation.InvalidHexEscape:
				return "A \\' escape was not followed by two hexadecimal digits.";
			case RtfComplianceViolation.UnescapedEightBitCharacter:
				return "The document contained a non-ASCII character that was not escaped.";
			case RtfComplianceViolation.UnpairedSurrogate:
				return "A \\uN control word specified one half of a UTF-16 surrogate pair without the other.";
			case RtfComplianceViolation.UnsupportedCodePage:
				return "A code page specified by the document could not be resolved.";
			case RtfComplianceViolation.UndefinedFont:
				return "A \\fN control word referred to a font that was not defined in the font table.";
			case RtfComplianceViolation.UndefinedColor:
				return "A \\cfN or \\cbN control word referred to a color that was not defined in the color table.";
			case RtfComplianceViolation.InvalidFontSize:
				return "A \\fsN control word specified a font size that was not positive or was too large.";
			case RtfComplianceViolation.FieldInstructionTooLong:
				return "A field instruction was longer than the maximum length that is examined.";
			case RtfComplianceViolation.FontTableLimitExceeded:
				return "The font table contained more entries than the configured limit.";
			case RtfComplianceViolation.ColorTableLimitExceeded:
				return "The color table contained more entries than the configured limit.";
			case RtfComplianceViolation.GroupDepthLimitExceeded:
				return "Groups that change the formatting state were nested more deeply than the configured limit.";
			case RtfComplianceViolation.TooManyComplianceIssues:
				return "Too many compliance issues were detected and the remainder were suppressed.";
			default:
				throw new ArgumentOutOfRangeException (nameof (violation));
			}
		}

		/// <summary>
		/// Get a detailed explanation of an RTF compliance violation.
		/// </summary>
		/// <remarks>
		/// Gets a detailed explanation of what the RTF specification requires and the problems that
		/// the violation is likely to cause.
		/// </remarks>
		/// <returns>The remarks.</returns>
		/// <param name="violation">The RTF compliance violation.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="violation"/> is not a valid <see cref="RtfComplianceViolation"/>.
		/// </exception>
		public static string GetRemarks (RtfComplianceViolation violation)
		{
			switch (violation) {
			case RtfComplianceViolation.MissingRtfHeader:
				return "The RTF specification requires that a document begin with an opening brace followed by the \\rtfN control word. The content is interpreted as RTF regardless, but other readers may reject the document or treat it as plain text.";
			case RtfComplianceViolation.UnbalancedGroupEnd:
				return "Every closing brace must close a group that was opened by a matching opening brace. The extra closing brace is ignored.";
			case RtfComplianceViolation.ContentAfterDocument:
				return "The RTF specification defines a document as a single group. Content that follows the closing brace of that group is rendered, but readers disagree about whether to render it, so it may be shown to some recipients and not to others.";
			case RtfComplianceViolation.TruncatedStream:
				return "The document ended within a group, or within the binary data of a \\binN control word. It was most likely truncated in transit or in storage, and whatever followed the point of truncation is lost.";
			case RtfComplianceViolation.KeywordTooLong:
				return "The RTF specification limits a control word to 32 letters. A longer control word cannot be valid, so it is treated as an unknown control word and ignored.";
			case RtfComplianceViolation.InvalidParameter:
				return "The RTF specification defines a control word parameter as an optional hyphen followed by digits, forming a signed 16-bit or 32-bit integer. A hyphen that is not followed by any digits is ignored, and a value that is out of range is clamped or replaced, which may not be what the writer intended.";
			case RtfComplianceViolation.InvalidHexEscape:
				return "The RTF specification requires that \\' be followed by exactly two hexadecimal digits. An escape without any hexadecimal digits is dropped, and one with a single hexadecimal digit is decoded using that digit alone, so the character is either lost or decoded incorrectly.";
			case RtfComplianceViolation.UnescapedEightBitCharacter:
				return "RTF is a 7-bit format, so characters outside of the ASCII range are supposed to be written using \\'hh or \\uN escapes. Unescaped 8-bit characters are common in practice and are decoded using the code page of the current font, but they do not survive transports that are not 8-bit clean.";
			case RtfComplianceViolation.UnpairedSurrogate:
				return "The RTF specification writes characters outside of the Basic Multilingual Plane as a pair of \\uN control words holding the high and low surrogates. A surrogate without its other half does not represent a character, so it is replaced with U+FFFD REPLACEMENT CHARACTER.";
			case RtfComplianceViolation.UnsupportedCodePage:
				return "Text is decoded using the code page specified by \\ansicpgN, \\fcharsetN or \\cpgN. When that code page is not available, the text is decoded using a fallback encoding instead, which may decode it incorrectly. On .NET Core and .NET 5 or later, most Windows code pages are only available after the application has registered System.Text.CodePagesEncodingProvider.";
			case RtfComplianceViolation.UndefinedFont:
				return "The font determines the code page that text is decoded with. Text formatted with an undefined font is decoded using the document's default code page, which may not be what the writer intended.";
			case RtfComplianceViolation.UndefinedColor:
				return "Text formatted with an undefined color is rendered using the automatic (default) color.";
			case RtfComplianceViolation.InvalidFontSize:
				return "The RTF specification measures font sizes in half-points, and Microsoft Word limits them to 1638 points. A font size of zero is a well-known way to hide text from the reader of a message while leaving it visible to a content scanner, and readers disagree about how to render such text.";
			case RtfComplianceViolation.FieldInstructionTooLong:
				return "Field instructions are only examined for HYPERLINK targets, so only the first 2048 characters are kept. A longer instruction is truncated, so a hyperlink may be lost or its target may be truncated.";
			case RtfComplianceViolation.FontTableLimitExceeded:
				return "This is not a defect in the document, but the font table entries beyond the limit were ignored, so text formatted with those fonts may have been decoded incorrectly.";
			case RtfComplianceViolation.ColorTableLimitExceeded:
				return "This is not a defect in the document, but the color table entries beyond the limit were ignored, so text formatted with those colors was rendered using the automatic color.";
			case RtfComplianceViolation.GroupDepthLimitExceeded:
				return "This is not a defect in the document, but the content of the groups beyond the limit was not processed.";
			case RtfComplianceViolation.TooManyComplianceIssues:
				return "Some violation was detected more often than the configured per-violation limit, so the remainder of that violation, and of any other violation that exceeds the limit, were not reported.";
			default:
				throw new ArgumentOutOfRangeException (nameof (violation));
			}
		}
	}
}

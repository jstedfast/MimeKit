//
// HtmlComplianceIssue.cs
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
	/// An HTML compliance issue detected while tokenizing.
	/// </summary>
	/// <remarks>
	/// <para>Describes a single parse error, as defined by the tokenization section of the HTML Living
	/// Standard, along with the location within the document where it was detected.</para>
	/// <para>New properties may be added to this structure in future versions of MimeKit in order to
	/// provide richer context about a violation. For that reason, always construct instances using one
	/// of the available constructors rather than relying on the default value.</para>
	/// <para>The default value of this structure has a <see cref="Violation"/> of
	/// <see cref="HtmlComplianceViolation.None"/>, which is never reported and cannot be constructed,
	/// so it may be used to detect an uninitialized instance.</para>
	/// </remarks>
	public readonly struct HtmlComplianceIssue : IEquatable<HtmlComplianceIssue>
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="HtmlComplianceIssue"/> struct.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="HtmlComplianceIssue"/>.
		/// </remarks>
		/// <param name="violation">The specific HTML compliance violation that occurred.</param>
		/// <param name="offset">The character offset within the document where the violation was found.</param>
		/// <param name="lineNumber">The line number where the violation was found.</param>
		/// <param name="linePosition">The position within the line where the violation was found.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="violation"/> is not a valid <see cref="HtmlComplianceViolation"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="offset"/> is negative.</para>
		/// <para>-or-</para>
		/// <para><paramref name="lineNumber"/> is less than <c>1</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="linePosition"/> is less than <c>1</c>.</para>
		/// </exception>
		public HtmlComplianceIssue (HtmlComplianceViolation violation, long offset, int lineNumber, int linePosition)
		{
			// Note: TooManyComplianceIssues is not part of the contiguous range of defects, so it is
			// checked separately.
			if ((violation <= HtmlComplianceViolation.None || violation > HtmlComplianceViolation.ElementDepthLimitExceeded) && violation != HtmlComplianceViolation.TooManyComplianceIssues)
				throw new ArgumentOutOfRangeException (nameof (violation));

			if (offset < 0)
				throw new ArgumentOutOfRangeException (nameof (offset));

			if (lineNumber < 1)
				throw new ArgumentOutOfRangeException (nameof (lineNumber));

			if (linePosition < 1)
				throw new ArgumentOutOfRangeException (nameof (linePosition));

			Violation = violation;
			Offset = offset;
			LineNumber = lineNumber;
			LinePosition = linePosition;
		}

		/// <summary>
		/// Get the specific HTML compliance violation that occurred.
		/// </summary>
		/// <remarks>
		/// Gets the specific HTML compliance violation that occurred.
		/// </remarks>
		/// <value>The HTML compliance violation.</value>
		public HtmlComplianceViolation Violation {
			get;
		}

		/// <summary>
		/// Get the character offset within the document where the violation was found.
		/// </summary>
		/// <remarks>
		/// <para>Gets the zero-based offset, in UTF-16 code units, of the character where the violation
		/// was found, relative to the start of the decoded text. A byte-order mark is not counted.</para>
		/// <para>Violations that describe a character reference are reported at the <c>&amp;</c> that
		/// begins it, and violations caused by the end of the input are reported at the end of the
		/// input.</para>
		/// </remarks>
		/// <value>The character offset.</value>
		public long Offset {
			get;
		}

		/// <summary>
		/// Get the line number where the violation was found.
		/// </summary>
		/// <remarks>
		/// Gets the one-based line number of the character where the violation was found. Lines are
		/// delimited by line feed characters.
		/// </remarks>
		/// <value>The line number.</value>
		public int LineNumber {
			get;
		}

		/// <summary>
		/// Get the position within the line where the violation was found.
		/// </summary>
		/// <remarks>
		/// Gets the one-based position, in UTF-16 code units, of the character where the violation was
		/// found within the line.
		/// </remarks>
		/// <value>The line position.</value>
		public int LinePosition {
			get;
		}

		/// <summary>
		/// Get the severity of the HTML compliance violation.
		/// </summary>
		/// <remarks>
		/// Gets how much practical harm the violation is likely to cause.
		/// </remarks>
		/// <value>The severity.</value>
		public MimeComplianceSeverity Severity {
			get { return GetSeverity (Violation); }
		}

		/// <summary>
		/// Get the categories of harm that the HTML compliance violation may cause.
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
		/// Get a brief description of the HTML compliance violation.
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
		/// Get a detailed explanation of the HTML compliance violation.
		/// </summary>
		/// <remarks>
		/// Gets a detailed explanation of how browsers handle the violation and the problems that it
		/// is likely to cause.
		/// </remarks>
		/// <value>The remarks.</value>
		public string Remarks {
			get { return GetRemarks (Violation); }
		}

		/// <summary>
		/// Get a string representation of the HTML compliance issue.
		/// </summary>
		/// <remarks>
		/// Gets a string representation of the HTML compliance issue.
		/// </remarks>
		/// <returns>A string representation of the HTML compliance issue.</returns>
		public override string ToString ()
		{
			var builder = new StringBuilder ();

			builder.Append (Violation);
			builder.AppendFormat (CultureInfo.InvariantCulture, " at line {0}, position {1} (offset {2})", LineNumber, LinePosition, Offset);

			return builder.ToString ();
		}

		/// <summary>
		/// Determine whether the specified <see cref="HtmlComplianceIssue"/> is equal to the current <see cref="HtmlComplianceIssue"/>.
		/// </summary>
		/// <remarks>
		/// Determines whether the specified <see cref="HtmlComplianceIssue"/> is equal to the current <see cref="HtmlComplianceIssue"/>.
		/// </remarks>
		/// <param name="other">The <see cref="HtmlComplianceIssue"/> to compare with the current <see cref="HtmlComplianceIssue"/>.</param>
		/// <returns><see langword="true" /> if the specified <see cref="HtmlComplianceIssue"/> is equal to the current
		/// <see cref="HtmlComplianceIssue"/>; otherwise, <see langword="false" />.</returns>
		public bool Equals (HtmlComplianceIssue other)
		{
			return other.Violation == Violation && other.Offset == Offset && other.LineNumber == LineNumber &&
				other.LinePosition == LinePosition;
		}

		/// <summary>
		/// Determine whether the specified <see cref="System.Object"/> is equal to the current <see cref="HtmlComplianceIssue"/>.
		/// </summary>
		/// <remarks>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="HtmlComplianceIssue"/>.
		/// </remarks>
		/// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="HtmlComplianceIssue"/>.</param>
		/// <returns><see langword="true" /> if the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="HtmlComplianceIssue"/>; otherwise, <see langword="false" />.</returns>
		public override bool Equals (object? obj)
		{
			return obj is HtmlComplianceIssue other && Equals (other);
		}

		/// <summary>
		/// Serve as a hash function for a <see cref="HtmlComplianceIssue"/> object.
		/// </summary>
		/// <remarks>
		/// Serves as a hash function for a <see cref="HtmlComplianceIssue"/> object.
		/// </remarks>
		/// <returns>A hash code for this instance that is suitable for use in hashing algorithms
		/// and data structures such as a hash table.</returns>
		public override int GetHashCode ()
		{
			return Violation.GetHashCode () ^ Offset.GetHashCode () ^ (LineNumber << 16) ^ LinePosition;
		}

		/// <summary>
		/// Compare two <see cref="HtmlComplianceIssue"/> objects for equality.
		/// </summary>
		/// <remarks>
		/// Compares two <see cref="HtmlComplianceIssue"/> objects for equality.
		/// </remarks>
		/// <param name="left">The first issue to compare.</param>
		/// <param name="right">The second issue to compare.</param>
		/// <returns><see langword="true" /> if the two issues are equal; otherwise, <see langword="false" />.</returns>
		public static bool operator == (HtmlComplianceIssue left, HtmlComplianceIssue right)
		{
			return left.Equals (right);
		}

		/// <summary>
		/// Compare two <see cref="HtmlComplianceIssue"/> objects for inequality.
		/// </summary>
		/// <remarks>
		/// Compares two <see cref="HtmlComplianceIssue"/> objects for inequality.
		/// </remarks>
		/// <param name="left">The first issue to compare.</param>
		/// <param name="right">The second issue to compare.</param>
		/// <returns><see langword="true" /> if the two issues are not equal; otherwise, <see langword="false" />.</returns>
		public static bool operator != (HtmlComplianceIssue left, HtmlComplianceIssue right)
		{
			return !left.Equals (right);
		}

		/// <summary>
		/// Get the severity of an HTML compliance violation.
		/// </summary>
		/// <remarks>
		/// Gets how much practical harm the violation is likely to cause.
		/// </remarks>
		/// <returns>The severity.</returns>
		/// <param name="violation">The HTML compliance violation.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="violation"/> is not a valid <see cref="HtmlComplianceViolation"/>.
		/// </exception>
		public static MimeComplianceSeverity GetSeverity (HtmlComplianceViolation violation)
		{
			switch (violation) {
			// Note: Deviations that browsers recover from in a well-defined way and that other HTML
			// processors are unlikely to interpret differently in a way that matters.
			case HtmlComplianceViolation.EofBeforeTagName:
			case HtmlComplianceViolation.InvalidFirstCharacterOfTagName:
			case HtmlComplianceViolation.UnexpectedQuestionMarkInsteadOfTagName:
			case HtmlComplianceViolation.MissingEndTagName:
			case HtmlComplianceViolation.EndTagWithAttributes:
			case HtmlComplianceViolation.EndTagWithTrailingSolidus:
			case HtmlComplianceViolation.MissingAttributeValue:
			case HtmlComplianceViolation.AbruptDocTypePublicIdentifier:
			case HtmlComplianceViolation.AbruptDocTypeSystemIdentifier:
			case HtmlComplianceViolation.EofInDocType:
			case HtmlComplianceViolation.InvalidCharacterSequenceAfterDocTypeName:
			case HtmlComplianceViolation.MissingDocTypeName:
			case HtmlComplianceViolation.MissingDocTypePublicIdentifier:
			case HtmlComplianceViolation.MissingDocTypeSystemIdentifier:
			case HtmlComplianceViolation.MissingQuoteBeforeDocTypePublicIdentifier:
			case HtmlComplianceViolation.MissingQuoteBeforeDocTypeSystemIdentifier:
			case HtmlComplianceViolation.MissingWhitespaceAfterDocTypePublicKeyword:
			case HtmlComplianceViolation.MissingWhitespaceAfterDocTypeSystemKeyword:
			case HtmlComplianceViolation.MissingWhitespaceBeforeDocTypeName:
			case HtmlComplianceViolation.MissingWhitespaceBetweenDocTypePublicAndSystemIdentifiers:
			case HtmlComplianceViolation.UnexpectedCharacterAfterDocTypeSystemIdentifier:
			case HtmlComplianceViolation.AbsenceOfDigitsInNumericCharacterReference:
			case HtmlComplianceViolation.ControlCharacterReference:
			case HtmlComplianceViolation.MissingSemicolonAfterCharacterReference:
			case HtmlComplianceViolation.NoncharacterCharacterReference:
			case HtmlComplianceViolation.UnknownNamedCharacterReference:
			case HtmlComplianceViolation.CharacterReferenceOutsideUnicodeRange:
			case HtmlComplianceViolation.NullCharacterReference:
			case HtmlComplianceViolation.SurrogateCharacterReference:
				return MimeComplianceSeverity.Minor;

			// Note: Constructs that different HTML processors are known to interpret differently, so that
			// what a filter or scanner examined may not be what the recipient sees, or content that was lost.
			case HtmlComplianceViolation.EofInTag:
			case HtmlComplianceViolation.ElementDepthLimitExceeded:
			case HtmlComplianceViolation.UnexpectedSolidusInTag:
			case HtmlComplianceViolation.DuplicateAttribute:
			case HtmlComplianceViolation.MissingWhitespaceBetweenAttributes:
			case HtmlComplianceViolation.UnexpectedCharacterInAttributeName:
			case HtmlComplianceViolation.UnexpectedCharacterInUnquotedAttributeValue:
			case HtmlComplianceViolation.UnexpectedEqualsSignBeforeAttributeName:
			case HtmlComplianceViolation.AbruptClosingOfEmptyComment:
			case HtmlComplianceViolation.EofInComment:
			case HtmlComplianceViolation.IncorrectlyClosedComment:
			case HtmlComplianceViolation.IncorrectlyOpenedComment:
			case HtmlComplianceViolation.NestedComment:
			case HtmlComplianceViolation.CDataInHtmlContent:
			case HtmlComplianceViolation.EofInScriptHtmlCommentLikeText:
			case HtmlComplianceViolation.UnexpectedNullCharacter:
			case HtmlComplianceViolation.EofInCData:
				return MimeComplianceSeverity.Major;

			// Note: Not a defect in the document, but a consumer that only looks at Major and above needs
			// to know that the report is incomplete.
			case HtmlComplianceViolation.TooManyComplianceIssues:
				return MimeComplianceSeverity.Major;

			default:
				throw new ArgumentOutOfRangeException (nameof (violation));
			}
		}

		/// <summary>
		/// Get the categories of harm that an HTML compliance violation may cause.
		/// </summary>
		/// <remarks>
		/// <para>Gets what kind of harm the violation may cause, as opposed to
		/// <see cref="GetSeverity(HtmlComplianceViolation)"/>, which rates how much.</para>
		/// <para>A violation may fall into more than one category, so this is a bit field.</para>
		/// </remarks>
		/// <returns>The categories.</returns>
		/// <param name="violation">The HTML compliance violation.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="violation"/> is not a valid <see cref="HtmlComplianceViolation"/>.
		/// </exception>
		public static MimeComplianceCategories GetCategories (HtmlComplianceViolation violation)
		{
			const MimeComplianceCategories Interop = MimeComplianceCategories.Interoperability;
			const MimeComplianceCategories DataLoss = MimeComplianceCategories.DataLoss;
			const MimeComplianceCategories Security = MimeComplianceCategories.Security;

			switch (violation) {
			case HtmlComplianceViolation.EofBeforeTagName:
			case HtmlComplianceViolation.InvalidFirstCharacterOfTagName:
			case HtmlComplianceViolation.UnexpectedQuestionMarkInsteadOfTagName:
			case HtmlComplianceViolation.MissingEndTagName:
			case HtmlComplianceViolation.EndTagWithAttributes:
			case HtmlComplianceViolation.EndTagWithTrailingSolidus:
			case HtmlComplianceViolation.MissingAttributeValue:
			case HtmlComplianceViolation.AbruptDocTypePublicIdentifier:
			case HtmlComplianceViolation.AbruptDocTypeSystemIdentifier:
			case HtmlComplianceViolation.EofInDocType:
			case HtmlComplianceViolation.InvalidCharacterSequenceAfterDocTypeName:
			case HtmlComplianceViolation.MissingDocTypeName:
			case HtmlComplianceViolation.MissingDocTypePublicIdentifier:
			case HtmlComplianceViolation.MissingDocTypeSystemIdentifier:
			case HtmlComplianceViolation.MissingQuoteBeforeDocTypePublicIdentifier:
			case HtmlComplianceViolation.MissingQuoteBeforeDocTypeSystemIdentifier:
			case HtmlComplianceViolation.MissingWhitespaceAfterDocTypePublicKeyword:
			case HtmlComplianceViolation.MissingWhitespaceAfterDocTypeSystemKeyword:
			case HtmlComplianceViolation.MissingWhitespaceBeforeDocTypeName:
			case HtmlComplianceViolation.MissingWhitespaceBetweenDocTypePublicAndSystemIdentifiers:
			case HtmlComplianceViolation.UnexpectedCharacterAfterDocTypeSystemIdentifier:
			case HtmlComplianceViolation.AbsenceOfDigitsInNumericCharacterReference:
			case HtmlComplianceViolation.ControlCharacterReference:
			case HtmlComplianceViolation.MissingSemicolonAfterCharacterReference:
			case HtmlComplianceViolation.NoncharacterCharacterReference:
			case HtmlComplianceViolation.UnknownNamedCharacterReference:
				return Interop;

			// Note: Content that was lost or went unprocessed is content that went unscanned.
			case HtmlComplianceViolation.EofInTag:
			case HtmlComplianceViolation.ElementDepthLimitExceeded:
				return DataLoss | Security;

			// Note: Different HTML processors may disagree about the structure of the document, so what a
			// filter or scanner examines may not be what the recipient is shown.
			case HtmlComplianceViolation.UnexpectedSolidusInTag:
			case HtmlComplianceViolation.DuplicateAttribute:
			case HtmlComplianceViolation.MissingWhitespaceBetweenAttributes:
			case HtmlComplianceViolation.UnexpectedCharacterInAttributeName:
			case HtmlComplianceViolation.UnexpectedCharacterInUnquotedAttributeValue:
			case HtmlComplianceViolation.UnexpectedEqualsSignBeforeAttributeName:
			case HtmlComplianceViolation.AbruptClosingOfEmptyComment:
			case HtmlComplianceViolation.EofInComment:
			case HtmlComplianceViolation.IncorrectlyClosedComment:
			case HtmlComplianceViolation.IncorrectlyOpenedComment:
			case HtmlComplianceViolation.NestedComment:
			case HtmlComplianceViolation.CDataInHtmlContent:
			case HtmlComplianceViolation.EofInScriptHtmlCommentLikeText:
			case HtmlComplianceViolation.UnexpectedNullCharacter:
				return Interop | Security;

			// Note: A character is decoded as U+FFFD REPLACEMENT CHARACTER.
			case HtmlComplianceViolation.CharacterReferenceOutsideUnicodeRange:
			case HtmlComplianceViolation.NullCharacterReference:
			case HtmlComplianceViolation.SurrogateCharacterReference:
				return Interop | DataLoss;

			// Note: Not likely to be interpreted differently, but content has been lost.
			case HtmlComplianceViolation.EofInCData:
				return DataLoss;

			// Note: A suppressed report is a blind spot, and hiding a violation from whatever is
			// inspecting the document is exactly what an attacker would want out of a flood of cheap ones.
			case HtmlComplianceViolation.TooManyComplianceIssues:
				return Security;

			default:
				throw new ArgumentOutOfRangeException (nameof (violation));
			}
		}

		/// <summary>
		/// Get a brief description of an HTML compliance violation.
		/// </summary>
		/// <remarks>
		/// Gets a brief, single-sentence description of what went wrong. Use
		/// <see cref="GetRemarks(HtmlComplianceViolation)"/> for a longer explanation of why it matters.
		/// </remarks>
		/// <returns>The description.</returns>
		/// <param name="violation">The HTML compliance violation.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="violation"/> is not a valid <see cref="HtmlComplianceViolation"/>.
		/// </exception>
		public static string GetDescription (HtmlComplianceViolation violation)
		{
			switch (violation) {
			case HtmlComplianceViolation.EofBeforeTagName:
				return "The input ended immediately after a < or </.";
			case HtmlComplianceViolation.InvalidFirstCharacterOfTagName:
				return "A < or </ was followed by a character that cannot start a tag name.";
			case HtmlComplianceViolation.UnexpectedQuestionMarkInsteadOfTagName:
				return "A < was followed by a question mark.";
			case HtmlComplianceViolation.MissingEndTagName:
				return "An end tag had no name (</>).";
			case HtmlComplianceViolation.EofInTag:
				return "The input ended inside of a tag.";
			case HtmlComplianceViolation.UnexpectedSolidusInTag:
				return "A / inside of a tag was not immediately followed by >.";
			case HtmlComplianceViolation.EndTagWithAttributes:
				return "An end tag had attributes.";
			case HtmlComplianceViolation.EndTagWithTrailingSolidus:
				return "An end tag had a trailing /.";
			case HtmlComplianceViolation.DuplicateAttribute:
				return "A tag had more than one attribute with the same name.";
			case HtmlComplianceViolation.MissingAttributeValue:
				return "An attribute name and = were not followed by a value.";
			case HtmlComplianceViolation.MissingWhitespaceBetweenAttributes:
				return "A quoted attribute value was not followed by whitespace.";
			case HtmlComplianceViolation.UnexpectedCharacterInAttributeName:
				return "An attribute name contained a \", ', or < character.";
			case HtmlComplianceViolation.UnexpectedCharacterInUnquotedAttributeValue:
				return "An unquoted attribute value contained a \", ', <, =, or ` character.";
			case HtmlComplianceViolation.UnexpectedEqualsSignBeforeAttributeName:
				return "An attribute name began with =.";
			case HtmlComplianceViolation.AbruptClosingOfEmptyComment:
				return "A comment was closed by <!--> or <!--->.";
			case HtmlComplianceViolation.EofInComment:
				return "The input ended inside of a comment.";
			case HtmlComplianceViolation.IncorrectlyClosedComment:
				return "A comment was closed by --!> instead of -->.";
			case HtmlComplianceViolation.IncorrectlyOpenedComment:
				return "A <! was not followed by --, DOCTYPE or [CDATA[.";
			case HtmlComplianceViolation.NestedComment:
				return "A comment contained <!--.";
			case HtmlComplianceViolation.AbruptDocTypePublicIdentifier:
				return "A DOCTYPE public identifier was terminated by > before its closing quote.";
			case HtmlComplianceViolation.AbruptDocTypeSystemIdentifier:
				return "A DOCTYPE system identifier was terminated by > before its closing quote.";
			case HtmlComplianceViolation.EofInDocType:
				return "The input ended inside of a DOCTYPE.";
			case HtmlComplianceViolation.InvalidCharacterSequenceAfterDocTypeName:
				return "A DOCTYPE name was followed by something other than PUBLIC, SYSTEM or >.";
			case HtmlComplianceViolation.MissingDocTypeName:
				return "A DOCTYPE had no name.";
			case HtmlComplianceViolation.MissingDocTypePublicIdentifier:
				return "The PUBLIC keyword of a DOCTYPE was not followed by a public identifier.";
			case HtmlComplianceViolation.MissingDocTypeSystemIdentifier:
				return "The SYSTEM keyword of a DOCTYPE was not followed by a system identifier.";
			case HtmlComplianceViolation.MissingQuoteBeforeDocTypePublicIdentifier:
				return "A DOCTYPE public identifier was not quoted.";
			case HtmlComplianceViolation.MissingQuoteBeforeDocTypeSystemIdentifier:
				return "A DOCTYPE system identifier was not quoted.";
			case HtmlComplianceViolation.MissingWhitespaceAfterDocTypePublicKeyword:
				return "The PUBLIC keyword of a DOCTYPE was not followed by whitespace.";
			case HtmlComplianceViolation.MissingWhitespaceAfterDocTypeSystemKeyword:
				return "The SYSTEM keyword of a DOCTYPE was not followed by whitespace.";
			case HtmlComplianceViolation.MissingWhitespaceBeforeDocTypeName:
				return "The DOCTYPE keyword was not followed by whitespace.";
			case HtmlComplianceViolation.MissingWhitespaceBetweenDocTypePublicAndSystemIdentifiers:
				return "The public and system identifiers of a DOCTYPE were not separated by whitespace.";
			case HtmlComplianceViolation.UnexpectedCharacterAfterDocTypeSystemIdentifier:
				return "A DOCTYPE system identifier was followed by something other than whitespace or >.";
			case HtmlComplianceViolation.AbsenceOfDigitsInNumericCharacterReference:
				return "A numeric character reference (&# or &#x) had no digits.";
			case HtmlComplianceViolation.CharacterReferenceOutsideUnicodeRange:
				return "A numeric character reference referred to a code point beyond U+10FFFF.";
			case HtmlComplianceViolation.ControlCharacterReference:
				return "A numeric character reference referred to a control character.";
			case HtmlComplianceViolation.MissingSemicolonAfterCharacterReference:
				return "A character reference was not terminated by a semicolon.";
			case HtmlComplianceViolation.NoncharacterCharacterReference:
				return "A numeric character reference referred to a Unicode noncharacter.";
			case HtmlComplianceViolation.NullCharacterReference:
				return "A numeric character reference referred to U+0000 NULL.";
			case HtmlComplianceViolation.SurrogateCharacterReference:
				return "A numeric character reference referred to a UTF-16 surrogate code point.";
			case HtmlComplianceViolation.UnknownNamedCharacterReference:
				return "A named character reference terminated by a semicolon did not match any known name.";
			case HtmlComplianceViolation.CDataInHtmlContent:
				return "A CDATA section appeared outside of SVG or MathML content.";
			case HtmlComplianceViolation.EofInCData:
				return "The input ended inside of a CDATA section.";
			case HtmlComplianceViolation.EofInScriptHtmlCommentLikeText:
				return "The input ended inside of an HTML comment-like construct within a script element.";
			case HtmlComplianceViolation.UnexpectedNullCharacter:
				return "The input contained a U+0000 NULL character.";
			case HtmlComplianceViolation.ElementDepthLimitExceeded:
				return "Elements were nested more deeply than the configured limit.";
			case HtmlComplianceViolation.TooManyComplianceIssues:
				return "Too many compliance issues were detected and the remainder were suppressed.";
			default:
				throw new ArgumentOutOfRangeException (nameof (violation));
			}
		}

		/// <summary>
		/// Get a detailed explanation of an HTML compliance violation.
		/// </summary>
		/// <remarks>
		/// Gets a detailed explanation of how browsers handle the violation and the problems that it
		/// is likely to cause.
		/// </remarks>
		/// <returns>The remarks.</returns>
		/// <param name="violation">The HTML compliance violation.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="violation"/> is not a valid <see cref="HtmlComplianceViolation"/>.
		/// </exception>
		public static string GetRemarks (HtmlComplianceViolation violation)
		{
			switch (violation) {
			case HtmlComplianceViolation.EofBeforeTagName:
				return "A < or </ at the very end of the input cannot start a tag, so it is treated as text.";
			case HtmlComplianceViolation.InvalidFirstCharacterOfTagName:
				return "A tag name must begin with an ASCII letter. After <, the < is treated as text; after </, the content up to the next > is treated as a bogus comment, which browsers do not render.";
			case HtmlComplianceViolation.UnexpectedQuestionMarkInsteadOfTagName:
				return "Markup such as an XML processing instruction (<?xml ...?>) is not valid in HTML. The content up to the next > is treated as a bogus comment, which browsers do not render.";
			case HtmlComplianceViolation.MissingEndTagName:
				return "The </> is ignored entirely; it neither closes an element nor is rendered as text.";
			case HtmlComplianceViolation.EofInTag:
				return "The input ended before the tag was closed by a >. The incomplete tag is dropped by browsers, so whatever followed the point of truncation is lost. Depending on the value of HtmlTokenizer.IgnoreTruncatedTags, the tokenizer either drops it as well or returns it as text.";
			case HtmlComplianceViolation.UnexpectedSolidusInTag:
				return "A / within a tag is only meaningful immediately before the closing >. Elsewhere it is ignored and treated as whitespace between attributes, which makes it a common way to separate attributes from a tag name (e.g. <img/src=...>) without using whitespace.";
			case HtmlComplianceViolation.EndTagWithAttributes:
				return "End tags cannot have attributes, so browsers ignore them.";
			case HtmlComplianceViolation.EndTagWithTrailingSolidus:
				return "End tags cannot be self-closing, so browsers ignore the /.";
			case HtmlComplianceViolation.DuplicateAttribute:
				return "Browsers keep the first occurrence of an attribute and ignore the rest, but other HTML processors may keep the last one instead. A filter or scanner that examines a different occurrence than the browser renders can be bypassed.";
			case HtmlComplianceViolation.MissingAttributeValue:
				return "An attribute such as <a href=> is treated as having an empty value.";
			case HtmlComplianceViolation.MissingWhitespaceBetweenAttributes:
				return "Attributes must be separated by whitespace. Browsers treat the next character as the start of a new attribute regardless, but other HTML processors may not, so the attributes they see may differ from the attributes that the browser renders.";
			case HtmlComplianceViolation.UnexpectedCharacterInAttributeName:
				return "These characters are not valid within an attribute name, but browsers treat them as part of the name. They usually indicate broken quoting, and other HTML processors may split the tag into attributes differently than the browser does.";
			case HtmlComplianceViolation.UnexpectedCharacterInUnquotedAttributeValue:
				return "These characters are not valid within an unquoted attribute value, but browsers treat them as part of the value. They usually indicate broken quoting, and some older browsers treat ` as a quote character, so different HTML processors may disagree about where the value ends.";
			case HtmlComplianceViolation.UnexpectedEqualsSignBeforeAttributeName:
				return "Browsers treat the = as the first character of the attribute name, but other HTML processors may treat it as the separator between a name and a value.";
			case HtmlComplianceViolation.AbruptClosingOfEmptyComment:
				return "Browsers treat <!--> and <!---> as complete, empty comments, but other HTML processors may treat them as the start of a comment that continues up to the next -->, hiding the content in between from them but not from the browser.";
			case HtmlComplianceViolation.EofInComment:
				return "Browsers treat everything from the start of an unterminated comment to the end of the input as part of the comment, so none of it is rendered. Other HTML processors may render it instead.";
			case HtmlComplianceViolation.IncorrectlyClosedComment:
				return "Browsers accept --!> as the end of a comment, but other HTML processors may not, and continue the comment up to the next -->, hiding the content in between from them but not from the browser.";
			case HtmlComplianceViolation.IncorrectlyOpenedComment:
				return "Browsers treat the content up to the next > as a bogus comment, which they do not render. Other HTML processors may handle it differently. This is often the result of a conditional comment such as <![if !mso]>.";
			case HtmlComplianceViolation.NestedComment:
				return "Comments do not nest; the first --> closes the comment regardless of how many <!-- precede it. A processor that believes comments nest will consider content to be commented out that a browser renders.";
			case HtmlComplianceViolation.AbruptDocTypePublicIdentifier:
				return "The DOCTYPE ends at the >, the public identifier is truncated, and the document is rendered in quirks mode.";
			case HtmlComplianceViolation.AbruptDocTypeSystemIdentifier:
				return "The DOCTYPE ends at the >, the system identifier is truncated, and the document is rendered in quirks mode.";
			case HtmlComplianceViolation.EofInDocType:
				return "The DOCTYPE is closed at the end of the input and the document is rendered in quirks mode.";
			case HtmlComplianceViolation.InvalidCharacterSequenceAfterDocTypeName:
				return "The remainder of the DOCTYPE is ignored and the document is rendered in quirks mode.";
			case HtmlComplianceViolation.MissingDocTypeName:
				return "A DOCTYPE without a name causes the document to be rendered in quirks mode.";
			case HtmlComplianceViolation.MissingDocTypePublicIdentifier:
				return "The DOCTYPE is closed and the document is rendered in quirks mode.";
			case HtmlComplianceViolation.MissingDocTypeSystemIdentifier:
				return "The DOCTYPE is closed and the document is rendered in quirks mode.";
			case HtmlComplianceViolation.MissingQuoteBeforeDocTypePublicIdentifier:
				return "The remainder of the DOCTYPE is ignored and the document is rendered in quirks mode.";
			case HtmlComplianceViolation.MissingQuoteBeforeDocTypeSystemIdentifier:
				return "The remainder of the DOCTYPE is ignored and the document is rendered in quirks mode.";
			case HtmlComplianceViolation.MissingWhitespaceAfterDocTypePublicKeyword:
				return "Browsers parse the public identifier regardless.";
			case HtmlComplianceViolation.MissingWhitespaceAfterDocTypeSystemKeyword:
				return "Browsers parse the system identifier regardless.";
			case HtmlComplianceViolation.MissingWhitespaceBeforeDocTypeName:
				return "Browsers parse the DOCTYPE name regardless.";
			case HtmlComplianceViolation.MissingWhitespaceBetweenDocTypePublicAndSystemIdentifiers:
				return "Browsers parse the system identifier regardless.";
			case HtmlComplianceViolation.UnexpectedCharacterAfterDocTypeSystemIdentifier:
				return "The remainder of the DOCTYPE is ignored. Unlike most other DOCTYPE errors, this does not cause the document to be rendered in quirks mode.";
			case HtmlComplianceViolation.AbsenceOfDigitsInNumericCharacterReference:
				return "The reference is not decoded and is rendered as text.";
			case HtmlComplianceViolation.CharacterReferenceOutsideUnicodeRange:
				return "There is no such character, so the reference is decoded as U+FFFD REPLACEMENT CHARACTER.";
			case HtmlComplianceViolation.ControlCharacterReference:
				return "Control characters other than ASCII whitespace are not allowed in HTML. For historical reasons, references to the C1 controls (U+0080 through U+009F) are decoded as the windows-1252 characters with the same values, and other control characters are decoded as-is.";
			case HtmlComplianceViolation.MissingSemicolonAfterCharacterReference:
				return "Browsers decode numeric references and a legacy set of named references (such as &amp and &copy) regardless, except within attribute values when a named reference is followed by = or an ASCII letter or digit (e.g. &copy=1 in a URL). Other HTML processors may decode them differently.";
			case HtmlComplianceViolation.NoncharacterCharacterReference:
				return "Noncharacters are permanently reserved for internal use and are not allowed in HTML. Browsers decode the reference as-is regardless.";
			case HtmlComplianceViolation.NullCharacterReference:
				return "NULL characters are not allowed in HTML, so the reference is decoded as U+FFFD REPLACEMENT CHARACTER.";
			case HtmlComplianceViolation.SurrogateCharacterReference:
				return "Surrogate code points are not characters, so the reference is decoded as U+FFFD REPLACEMENT CHARACTER.";
			case HtmlComplianceViolation.UnknownNamedCharacterReference:
				return "The reference is not decoded and is rendered as text.";
			case HtmlComplianceViolation.CDataInHtmlContent:
				return "CDATA sections are only allowed within SVG and MathML content. Elsewhere, browsers treat <![CDATA[ as the start of a bogus comment that ends at the next > rather than at ]]>, so other HTML processors may disagree about where it ends and what content it hides.";
			case HtmlComplianceViolation.EofInCData:
				return "The CDATA section is closed at the end of the input. The input was most likely truncated, and whatever followed the point of truncation is lost.";
			case HtmlComplianceViolation.EofInScriptHtmlCommentLikeText:
				return "Within a <script> element, text such as <!-- <script> changes how the end of the script is detected, so the script may continue to the end of the input, consuming content that a different processor would render as markup.";
			case HtmlComplianceViolation.UnexpectedNullCharacter:
				return "NULL characters are not allowed in HTML. Depending on where they appear, browsers either ignore them or replace them with U+FFFD REPLACEMENT CHARACTER, while other HTML processors may truncate the content at the NULL.";
			case HtmlComplianceViolation.ElementDepthLimitExceeded:
				return "This is not a defect in the document, but the remainder of the input was treated as text rather than markup.";
			case HtmlComplianceViolation.TooManyComplianceIssues:
				return "Some violation was detected more often than the configured per-violation limit, so the remainder of that violation, and of any other violation that exceeds the limit, were not reported.";
			default:
				throw new ArgumentOutOfRangeException (nameof (violation));
			}
		}
	}
}

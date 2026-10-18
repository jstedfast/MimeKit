//
// HtmlComplianceViolationTests.cs
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

using MimeKit.Text;

namespace UnitTests.Text {
	[TestFixture]
	public class HtmlComplianceViolationTests
	{
		// Note: This table pins the numeric value of every HtmlComplianceViolation. The values are public
		// contract (see TnefComplianceViolationTests for the full rationale). A new violation must take the
		// next unused value; an unreleased one may be inserted into its group as long as this table is updated
		// in the same commit.
		static readonly Dictionary<HtmlComplianceViolation, int> ExpectedValues = new () {
			{ HtmlComplianceViolation.None, 0 },
			{ HtmlComplianceViolation.EofBeforeTagName, 1 },
			{ HtmlComplianceViolation.InvalidFirstCharacterOfTagName, 2 },
			{ HtmlComplianceViolation.UnexpectedQuestionMarkInsteadOfTagName, 3 },
			{ HtmlComplianceViolation.MissingEndTagName, 4 },
			{ HtmlComplianceViolation.EofInTag, 5 },
			{ HtmlComplianceViolation.UnexpectedSolidusInTag, 6 },
			{ HtmlComplianceViolation.EndTagWithAttributes, 7 },
			{ HtmlComplianceViolation.EndTagWithTrailingSolidus, 8 },
			{ HtmlComplianceViolation.DuplicateAttribute, 9 },
			{ HtmlComplianceViolation.MissingAttributeValue, 10 },
			{ HtmlComplianceViolation.MissingWhitespaceBetweenAttributes, 11 },
			{ HtmlComplianceViolation.UnexpectedCharacterInAttributeName, 12 },
			{ HtmlComplianceViolation.UnexpectedCharacterInUnquotedAttributeValue, 13 },
			{ HtmlComplianceViolation.UnexpectedEqualsSignBeforeAttributeName, 14 },
			{ HtmlComplianceViolation.AbruptClosingOfEmptyComment, 15 },
			{ HtmlComplianceViolation.EofInComment, 16 },
			{ HtmlComplianceViolation.IncorrectlyClosedComment, 17 },
			{ HtmlComplianceViolation.IncorrectlyOpenedComment, 18 },
			{ HtmlComplianceViolation.NestedComment, 19 },
			{ HtmlComplianceViolation.AbruptDocTypePublicIdentifier, 20 },
			{ HtmlComplianceViolation.AbruptDocTypeSystemIdentifier, 21 },
			{ HtmlComplianceViolation.EofInDocType, 22 },
			{ HtmlComplianceViolation.InvalidCharacterSequenceAfterDocTypeName, 23 },
			{ HtmlComplianceViolation.MissingDocTypeName, 24 },
			{ HtmlComplianceViolation.MissingDocTypePublicIdentifier, 25 },
			{ HtmlComplianceViolation.MissingDocTypeSystemIdentifier, 26 },
			{ HtmlComplianceViolation.MissingQuoteBeforeDocTypePublicIdentifier, 27 },
			{ HtmlComplianceViolation.MissingQuoteBeforeDocTypeSystemIdentifier, 28 },
			{ HtmlComplianceViolation.MissingWhitespaceAfterDocTypePublicKeyword, 29 },
			{ HtmlComplianceViolation.MissingWhitespaceAfterDocTypeSystemKeyword, 30 },
			{ HtmlComplianceViolation.MissingWhitespaceBeforeDocTypeName, 31 },
			{ HtmlComplianceViolation.MissingWhitespaceBetweenDocTypePublicAndSystemIdentifiers, 32 },
			{ HtmlComplianceViolation.UnexpectedCharacterAfterDocTypeSystemIdentifier, 33 },
			{ HtmlComplianceViolation.AbsenceOfDigitsInNumericCharacterReference, 34 },
			{ HtmlComplianceViolation.CharacterReferenceOutsideUnicodeRange, 35 },
			{ HtmlComplianceViolation.ControlCharacterReference, 36 },
			{ HtmlComplianceViolation.MissingSemicolonAfterCharacterReference, 37 },
			{ HtmlComplianceViolation.NoncharacterCharacterReference, 38 },
			{ HtmlComplianceViolation.NullCharacterReference, 39 },
			{ HtmlComplianceViolation.SurrogateCharacterReference, 40 },
			{ HtmlComplianceViolation.UnknownNamedCharacterReference, 41 },
			{ HtmlComplianceViolation.CDataInHtmlContent, 42 },
			{ HtmlComplianceViolation.EofInCData, 43 },
			{ HtmlComplianceViolation.EofInScriptHtmlCommentLikeText, 44 },
			{ HtmlComplianceViolation.UnexpectedNullCharacter, 45 },
			{ HtmlComplianceViolation.ElementDepthLimitExceeded, 46 },
			{ HtmlComplianceViolation.IgnoredStartTag, 47 },
			{ HtmlComplianceViolation.ForeignContentBreakout, 48 },
			{ HtmlComplianceViolation.AttributesMergedIntoExistingElement, 49 },
			{ HtmlComplianceViolation.TooManyComplianceIssues, int.MaxValue },
		};

		[Test]
		public void TestNumericValuesAreStable ()
		{
			foreach (var violation in Enum.GetValues<HtmlComplianceViolation> ()) {
				Assert.That (ExpectedValues.ContainsKey (violation), Is.True,
					$"{violation} is missing from the expected value table. If it is a new violation, make sure it took the next unused value and then add it here as {{ HtmlComplianceViolation.{violation}, {(int) violation} }}.");

				Assert.That ((int) violation, Is.EqualTo (ExpectedValues[violation]),
					$"The numeric value of {violation} changed from {ExpectedValues[violation]} to {(int) violation}. This is a breaking change for callers compiled against an earlier version of MimeKit and for any persisted value.");
			}
		}

		[Test]
		public void TestValuesAreUnique ()
		{
			var seen = new Dictionary<int, HtmlComplianceViolation> ();

			foreach (var violation in Enum.GetValues<HtmlComplianceViolation> ()) {
				var value = (int) violation;

				Assert.That (seen.ContainsKey (value), Is.False, $"{violation} and {(seen.TryGetValue (value, out var other) ? other.ToString () : null)} share the value {value}.");
				seen.Add (value, violation);
			}
		}

		[Test]
		public void TestNewViolationsTakeTheNextUnusedValue ()
		{
			var values = Enum.GetValues<HtmlComplianceViolation> ().Where (v => v != HtmlComplianceViolation.TooManyComplianceIssues).Select (v => (int) v).ToList ();

			Assert.That (values.Min (), Is.EqualTo (0), "The first violation should have the value 0.");
			Assert.That (values.Max (), Is.EqualTo (values.Count - 1), "The values should be contiguous, so a new violation must take the next unused value rather than reusing or displacing an existing one.");
		}
	}
}
//
// MimeComplianceAddressTests.cs
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

using System.Text;

using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class MimeComplianceAddressTests
	{
		// Note: The address violations are the contiguous block appended when address validation
		// was added. Listing them explicitly (rather than testing a numeric range) keeps this from
		// silently absorbing violations added later for unrelated reasons.
		static readonly HashSet<MimeComplianceViolation> AddressViolations = new HashSet<MimeComplianceViolation> {
			MimeComplianceViolation.ExcessiveAngleBracketsInAddress,
			MimeComplianceViolation.UnbalancedAngleBracketsInAddress,
			MimeComplianceViolation.UnbalancedQuotesInAddress,
			MimeComplianceViolation.UnbalancedParenthesesInAddress,
			MimeComplianceViolation.UnquotedDisplayName,
			MimeComplianceViolation.AddressInDisplayName,
			MimeComplianceViolation.AddressInGroupDisplayName,
			MimeComplianceViolation.InvalidLocalPart,
			MimeComplianceViolation.MissingAddressSeparator,
			MimeComplianceViolation.AmbiguousMailboxBoundary,
			MimeComplianceViolation.ExtraneousCommaInAddressList,
			MimeComplianceViolation.ObsoleteRouteAddress,
			MimeComplianceViolation.AddressWithoutDomain,
			MimeComplianceViolation.ObsoleteDomainSyntax,
			MimeComplianceViolation.TrailingDotInDomain,
			MimeComplianceViolation.WhitespaceInDomainLiteral,
			MimeComplianceViolation.Invalid8BitAddress,
			MimeComplianceViolation.MissingGroupTerminator,
			MimeComplianceViolation.NonConformantAddress,
			MimeComplianceViolation.NullByteInAddress,
			MimeComplianceViolation.LineBreakInAddress,
			MimeComplianceViolation.ControlCharacterInAddress,
			MimeComplianceViolation.Iso2022SequenceInLocalPart,
			MimeComplianceViolation.EmptyGroupName
		};

		// Note: Latin1 round-trips every byte value, which lets a test case embed raw 8-bit bytes
		// that are not valid UTF-8.
		static List<MimeComplianceIssue> Validate (string field, string value)
		{
			var text = $"{field}: {value}\r\nSubject: test\r\n\r\nbody\r\n";
			var logger = new TestMimeComplianceLogger ();

			using (var stream = new MemoryStream (Encoding.Latin1.GetBytes (text), false)) {
				var reader = new MimeReader (stream) { ComplianceLogger = logger };

				reader.ReadMessage ();
			}

			return logger.Issues.Where (issue => AddressViolations.Contains (issue.Violation)).ToList ();
		}

		static void AssertViolation (string value, MimeComplianceViolation expected)
		{
			var issues = Validate ("To", value);

			Assert.That (issues.Select (issue => issue.Violation), Has.Some.EqualTo (expected),
				$"Expected {expected} for \"{value}\" but got: {string.Join (", ", issues.Select (i => i.Violation))}");
		}

		[TestCase ("user@example.com")]
		[TestCase ("User <user@example.com>")]
		[TestCase ("\"Doe, John\" <john@example.com>")]
		[TestCase ("\"user name\"@example.com")]
		[TestCase ("user@[192.168.0.1]")]
		[TestCase ("user@[IPv6:2001:db8::1]")]
		[TestCase ("=?utf-8?q?Jos=C3=A9?= <jose@example.com>")]
		[TestCase ("a@example.com, b@example.com, c@example.com")]
		[TestCase ("(a comment) user@example.com (another)")]
		[TestCase ("Friends: a@example.com, b@example.com;")]
		[TestCase ("Empty:;")]
		[TestCase ("first.last@sub.domain.example.com")]
		[TestCase ("\"quoted\\\"escape\"@example.com")]
		public void TestConformantAddressesAreSilent (string value)
		{
			var issues = Validate ("To", value);

			Assert.That (issues, Is.Empty,
				$"\"{value}\" should be silent but reported: {string.Join (", ", issues.Select (i => i.Violation))}");
		}

		[TestCase ("<<user@example.com>>", MimeComplianceViolation.ExcessiveAngleBracketsInAddress)]
		[TestCase ("User <user@example.com", MimeComplianceViolation.UnbalancedAngleBracketsInAddress)]
		[TestCase ("User user@example.com>", MimeComplianceViolation.UnbalancedAngleBracketsInAddress)]
		[TestCase ("\"unterminated <user@example.com>", MimeComplianceViolation.UnbalancedQuotesInAddress)]
		[TestCase ("(unterminated <user@example.com>", MimeComplianceViolation.UnbalancedParenthesesInAddress)]
		[TestCase ("user@ex ample <user@example.com>", MimeComplianceViolation.UnquotedDisplayName)]
		[TestCase ("us..er@example.com", MimeComplianceViolation.InvalidLocalPart)]
		[TestCase ("user.@example.com", MimeComplianceViolation.InvalidLocalPart)]
		[TestCase ("a@example.com b@example.com", MimeComplianceViolation.MissingAddressSeparator)]
		[TestCase ("a@example.com,, b@example.com", MimeComplianceViolation.ExtraneousCommaInAddressList)]
		[TestCase ("a@example.com,", MimeComplianceViolation.ExtraneousCommaInAddressList)]
		[TestCase ("<@hop.example.com:user@example.com>", MimeComplianceViolation.ObsoleteRouteAddress)]
		[TestCase ("localuser", MimeComplianceViolation.AddressWithoutDomain)]
		[TestCase ("user@example.com.", MimeComplianceViolation.TrailingDotInDomain)]
		[TestCase ("user@[192.168 .0.1]", MimeComplianceViolation.WhitespaceInDomainLiteral)]
		[TestCase ("Friends: a@example.com, b@example.com", MimeComplianceViolation.MissingGroupTerminator)]
		public void TestNonConformantAddressesAreReported (string value, MimeComplianceViolation expected)
		{
			AssertViolation (value, expected);
		}

		[Test]
		public void TestNestedGroupIsReported ()
		{
			// Note: rfc5322 defines group-list as "mailbox-list / CFWS / obs-group-list", so a group
			// may only contain mailboxes. A nested group is not valid syntax at any conformance level.
			AssertViolation ("Outer: Inner: a@example.com;;", MimeComplianceViolation.NonConformantAddress);
		}

		// Note: A null byte is never legal in a header, but it is reported separately when it occurs
		// in an address because it truncates the address for anything that treats it as a C string.
		[TestCase ("\"Jo\0hn\" <j@example.com>")]
		[TestCase ("Jo\0hn <j@example.com>")]
		[TestCase ("us\0er@example.com")]
		[TestCase ("user@exa\0mple.com")]
		[TestCase ("<us\0er@example.com>")]
		[TestCase ("(co\0mment) user@example.com")]
		public void TestNullByteInAddress (string value)
		{
			var issues = Validate ("To", value);

			Assert.That (issues.Select (i => i.Violation), Has.Some.EqualTo (MimeComplianceViolation.NullByteInAddress));

			// The null byte is reported once, and does not cascade into structural violations that
			// describe nothing the sender actually did.
			Assert.That (issues, Has.Count.EqualTo (1),
				$"Expected exactly one violation but got: {string.Join (", ", issues.Select (i => i.Violation))}");
		}

		[Test]
		public void TestInjectionPrimitivesAreCritical ()
		{
			// Note: Unlike BareLinefeedInHeader, these are defects of the message rather than
			// requirements of the channel, so they are rated the same in both contexts.
			var violations = new [] {
				MimeComplianceViolation.NullByteInAddress,
				MimeComplianceViolation.LineBreakInAddress
			};

			foreach (var violation in violations) {
				Assert.That (MimeComplianceIssue.GetSeverity (violation, MimeComplianceContext.Transport),
					Is.EqualTo (MimeComplianceSeverity.Critical), $"{violation} (Transport)");
				Assert.That (MimeComplianceIssue.GetSeverity (violation, MimeComplianceContext.Storage),
					Is.EqualTo (MimeComplianceSeverity.Critical), $"{violation} (Storage)");
			}
		}

		[Test]
		public void TestNullByteSurvivesIntoTheParsedDisplayName ()
		{
			// Note: This is why the violation is worth reporting separately - MimeKit itself accepts
			// the address and hands the caller a name with an embedded null in it.
			var text = "To: \"Jo\0hn\" <j@example.com>\r\n\r\nbody\r\n";

			using var stream = new MemoryStream (Encoding.Latin1.GetBytes (text), false);
			var message = MimeMessage.Load (stream);

			Assert.That (message.To.Mailboxes.Single ().Name, Does.Contain ("\0"));
		}

		// Note: rfc5322 allows folding around the tokens of an addr-spec but not inside a
		// dot-atom-text, so these cannot be produced by a conforming mailer and are the shape that
		// header injection takes.
		[TestCase ("us\r\n er@example.com")]
		[TestCase ("us\n er@example.com")]
		[TestCase ("user@exa\r\n mple.com")]
		[TestCase ("<us\r\n er@example.com>")]
		public void TestLineBreakInAddress (string value)
		{
			var issues = Validate ("To", value);

			Assert.That (issues.Select (i => i.Violation), Has.Some.EqualTo (MimeComplianceViolation.LineBreakInAddress));
			Assert.That (issues, Has.Count.EqualTo (1),
				$"Expected exactly one violation but got: {string.Join (", ", issues.Select (i => i.Violation))}");
		}

		// Note: Section 3.2.4 of rfc5322 permits folding whitespace inside a quoted-string, so a
		// quoted local-part that is folded is legal. It is reported anyway: a quoted local-part is
		// rare enough that implementations mishandle it, and both MimeKit and Exchange have done so,
		// which makes the divergence this violation describes real regardless of the grammar.
		[TestCase ("\"us\r\n er\"@example.com")]
		[TestCase ("<\"us\r\n er\"@example.com>")]
		[TestCase ("John Doe <\"us\r\n er\"@example.com>")]
		public void TestLineBreakInQuotedLocalPartIsReported (string value)
		{
			var issues = Validate ("To", value);

			Assert.That (issues.Select (i => i.Violation), Has.Some.EqualTo (MimeComplianceViolation.LineBreakInAddress));
			Assert.That (issues, Has.Count.EqualTo (1),
				$"Expected exactly one violation but got: {string.Join (", ", issues.Select (i => i.Violation))}");
		}

		// Note: This is the false-positive risk for LineBreakInAddress. Folding is legal between
		// addresses, around the tokens of an addr-spec, between the words of a phrase, and inside
		// comments and quoted display-names, so none of these may be reported. A folded quoted
		// local-part is the one legal construct that is deliberately reported anyway; see
		// TestLineBreakInQuotedLocalPartIsReported.
		[TestCase ("a@example.com,\r\n b@example.com")]
		[TestCase ("user\r\n @example.com")]
		[TestCase ("user@\r\n example.com")]
		[TestCase ("John\r\n Doe <j@example.com>")]
		[TestCase ("\"John\r\n Doe\" <j@example.com>")]
		[TestCase ("(a\r\n comment) user@example.com")]
		[TestCase ("John Doe\r\n <j@example.com>")]
		[TestCase ("\"John\r\n Doe\" <\"user\"@example.com>")]
		public void TestLegalFoldingIsNotReported (string value)
		{
			var issues = Validate ("To", value);

			Assert.That (issues, Is.Empty,
				$"\"{value.Replace ("\r", "\\r").Replace ("\n", "\\n")}\" should be silent but reported: {string.Join (", ", issues.Select (i => i.Violation))}");
		}

		[TestCase ("us\u0001er@example.com")]
		[TestCase ("us\u001ber@example.com")]
		[TestCase ("us\u007fer@example.com")]
		[TestCase ("Jo\u001bhn <j@example.com>")]
		[TestCase ("\"Jo\u0007hn\" <j@example.com>")]
		public void TestControlCharacterInAddress (string value)
		{
			var issues = Validate ("To", value);

			Assert.That (issues.Select (i => i.Violation), Has.Some.EqualTo (MimeComplianceViolation.ControlCharacterInAddress));
			Assert.That (issues, Has.Count.EqualTo (1),
				$"Expected exactly one violation but got: {string.Join (", ", issues.Select (i => i.Violation))}");
		}

		// Note: ISO-2022-JP and its relatives switch character sets with escape sequences such as
		// "ESC $ B" and, in the Korean and Chinese variants, with shift-out and shift-in. Japanese
		// mailers have historically used these inside a local-part to carry Japanese text in a mailbox
		// name, long before rfc6532 gave addresses a defined way to be non-ASCII. That is a legacy
		// convention rather than damage or an injection attempt, so it is reported on its own.
		[TestCase ("\"\u001b$BF|K\u001b(B\"@example.com", TestName = "TestIso2022InLocalPart_QuotedString")]
		[TestCase ("Nihongo <\"\u001b$BF|K\u001b(B\"@example.com>", TestName = "TestIso2022InLocalPart_AngleAddr")]
		// Note: The second byte of a JIS X 0208 pair may be a backslash, which then has to be written
		// as a quoted-pair in order to survive the quoted-string grammar. This is the shape that
		// Exchange sees in the wild.
		[TestCase ("\"test\u001b$BF|K\\\\8l%a!<%k%F%9%H\u001b(B123\"@iso2022.jp", TestName = "TestIso2022InLocalPart_EscapedJisByte")]
		[TestCase ("\u000eF|K\u000f@example.com", TestName = "TestIso2022InLocalPart_ShiftOutShiftIn")]
		public void TestIso2022SequenceInLocalPart (string value)
		{
			var issues = Validate ("To", value);

			Assert.That (issues.Select (i => i.Violation), Has.Some.EqualTo (MimeComplianceViolation.Iso2022SequenceInLocalPart));

			// Note: The same bytes must not also be described as an arbitrary control character.
			Assert.That (issues.Select (i => i.Violation), Has.None.EqualTo (MimeComplianceViolation.ControlCharacterInAddress));
			Assert.That (issues, Has.Count.EqualTo (1),
				$"Expected exactly one violation but got: {string.Join (", ", issues.Select (i => i.Violation))}");
		}

		// Note: Outside of a local-part there is no legacy convention to defer to, so a well-formed
		// ISO-2022 sequence is just a control character in an address.
		//
		// Note: The display-name is quoted, and the other two cases use shift-out and shift-in rather
		// than an escape sequence, because the parser has no knowledge of ISO-2022 and the '(' of an
		// unquoted "ESC ( B" would open a comment. That is a real consequence of the construct, but it
		// is not what these cases are here to measure.
		[TestCase ("\"\u001b$BF|K\u001b(B\" <user@example.com>", TestName = "TestIso2022OutsideLocalPart_DisplayName")]
		[TestCase ("user@exa\u000eF|K\u000fmple.com", TestName = "TestIso2022OutsideLocalPart_Domain")]
		[TestCase ("(\u000eF|K\u000f) user@example.com", TestName = "TestIso2022OutsideLocalPart_Comment")]
		public void TestIso2022SequenceOutsideLocalPart (string value)
		{
			var issues = Validate ("To", value);

			Assert.That (issues.Select (i => i.Violation), Has.Some.EqualTo (MimeComplianceViolation.ControlCharacterInAddress));
			Assert.That (issues.Select (i => i.Violation), Has.None.EqualTo (MimeComplianceViolation.Iso2022SequenceInLocalPart));
			Assert.That (issues, Has.Count.EqualTo (1),
				$"Expected exactly one violation but got: {string.Join (", ", issues.Select (i => i.Violation))}");
		}

		// Note: An escape that merely happens to precede a letter is not a character set designation.
		// Every ISO-2022 designation sequence has at least one intermediate byte in the 0x20..0x2f
		// range between the escape and its final byte, and without one this is an ordinary control
		// character.
		[TestCase ("us\u001ber@example.com", TestName = "TestNotIso2022_NoIntermediate")]
		[TestCase ("us\u001b\u001ber@example.com", TestName = "TestNotIso2022_EscapeFollowedByEscape")]
		public void TestIncompleteEscapeSequenceIsAControlCharacter (string value)
		{
			var issues = Validate ("To", value);

			Assert.That (issues.Select (i => i.Violation), Has.Some.EqualTo (MimeComplianceViolation.ControlCharacterInAddress));
			Assert.That (issues.Select (i => i.Violation), Has.None.EqualTo (MimeComplianceViolation.Iso2022SequenceInLocalPart));
		}

		// Note: The parser has no knowledge of ISO-2022, so the '(' of an unquoted "ESC ( B" opens a
		// comment exactly as it would anywhere else in an address, and the comment then runs to the end
		// of the value. That is the intended outcome rather than an oversight: the alternative would be
		// to treat '(' as ordinary text whenever an escape happens to precede it, which would let a
		// genuinely unterminated comment go unreported. Mailers that use this convention quote the
		// local-part, which is what makes the second byte of a JIS pair survive in the first place.
		[Test]
		public void TestUnquotedIso2022LocalPartOpensAComment ()
		{
			var issues = Validate ("To", "\u001b$BF|K\u001b(B@example.com");

			Assert.That (issues.Select (i => i.Violation), Has.Some.EqualTo (MimeComplianceViolation.UnbalancedParenthesesInAddress));

			// Note: The escapes are not attributed to a local-part, because the parse never produced
			// one, so they fall through to being reported as the control characters that they are.
			Assert.That (issues.Select (i => i.Violation), Has.Some.EqualTo (MimeComplianceViolation.ControlCharacterInAddress));
			Assert.That (issues.Select (i => i.Violation), Has.None.EqualTo (MimeComplianceViolation.Iso2022SequenceInLocalPart));
			Assert.That (issues, Has.Count.EqualTo (2),
				$"Expected exactly two violations but got: {string.Join (", ", issues.Select (i => i.Violation))}");
		}

		[Test]
		public void TestTabIsNotAControlCharacter ()		{
			// Note: Tab is whitespace, not a control character, so it must not be reported as one
			// even where it appears somewhere the grammar does not allow whitespace.
			var issues = Validate ("To", "us\ter@example.com");

			Assert.That (issues.Select (i => i.Violation), Has.None.EqualTo (MimeComplianceViolation.ControlCharacterInAddress));
		}

		// Note: A carriage return may only ever appear as the first half of the CRLF of a fold. Both
		// FWS and obs-FWS require a linefeed and at least one whitespace character after it, and
		// obs-NO-WS-CTL excludes CR and LF, so a lone carriage return is illegal under every tier of
		// the grammar. That includes inside a quoted-string or a comment, where folding is otherwise
		// legal and where nothing was reported at all before.
		//
		// Note: It is described as a line break rather than as a generic control character because
		// what makes it dangerous is that it is half of one: a transport that normalizes it to CRLF
		// turns it into a header split. Reporting it as a control character ranked it below the
		// folded form in CheckLineBreak, which is the more benign of the two.
		[TestCase ("us\rer@example.com", TestName = "TestLoneCarriageReturn_DotAtom")]
		[TestCase ("us\r er@example.com", TestName = "TestLoneCarriageReturn_BeforeSpace")]
		[TestCase ("user@exa\rmple.com", TestName = "TestLoneCarriageReturn_Domain")]
		[TestCase ("\"us\rer\"@example.com", TestName = "TestLoneCarriageReturn_QuotedLocalPart")]
		[TestCase ("\"Jo\rhn\" <j@example.com>", TestName = "TestLoneCarriageReturn_QuotedDisplayName")]
		[TestCase ("(a\rb) user@example.com", TestName = "TestLoneCarriageReturn_Comment")]
		[TestCase ("user@[192.168\r.0.1]", TestName = "TestLoneCarriageReturn_DomainLiteral")]
		public void TestLoneCarriageReturnIsALineBreak (string value)
		{
			var issues = Validate ("To", value);

			Assert.That (issues.Select (i => i.Violation), Has.Some.EqualTo (MimeComplianceViolation.LineBreakInAddress));

			// Note: It is a line break rather than a control character, and it must not be described
			// as both.
			Assert.That (issues.Select (i => i.Violation), Has.None.EqualTo (MimeComplianceViolation.ControlCharacterInAddress));
			Assert.That (issues, Has.Count.EqualTo (1),
				$"Expected exactly one violation but got: {string.Join (", ", issues.Select (i => i.Violation))}");
		}

		// Note: A bare linefeed never reaches the address validator: MimeReader treats it as a line
		// terminator first and reports BareLinefeedInHeader, which already carries the Security
		// category for the SMTP smuggling it enables. This pins that division of labour so the
		// carriage-return handling above is not later extended to cover it as well.
		[Test]
		public void TestBareLinefeedIsReportedByTheReaderRatherThanTheAddressValidator ()
		{
			var text = "To: \"a\nb\"@example.com\r\nSubject: test\r\n\r\nbody\r\n";
			var logger = new TestMimeComplianceLogger ();

			using (var stream = new MemoryStream (Encoding.Latin1.GetBytes (text), false)) {
				var reader = new MimeReader (stream) { ComplianceLogger = logger };

				reader.ReadMessage ();
			}

			Assert.That (logger.Issues.Select (i => i.Violation), Has.Some.EqualTo (MimeComplianceViolation.BareLinefeedInHeader));
			Assert.That (logger.Issues.Any (i => (i.Categories & MimeComplianceCategories.Security) != 0), Is.True,
				"A bare linefeed should be reported as a security issue.");
		}

		[Test]
		public void TestLoneCarriageReturnIsSilentlyDroppedFromTheParsedDisplayName ()
		{
			// Note: This is why a lone carriage return has to be reported. MimeKit accepts the
			// address and removes the carriage return, so a filter reading the raw header and an
			// auditor reading the parsed name see two different display names.
			var text = "To: \"Jo\rhn\" <j@example.com>\r\n\r\nbody\r\n";

			using var stream = new MemoryStream (Encoding.Latin1.GetBytes (text), false);
			var message = MimeMessage.Load (stream);

			Assert.That (message.To.Mailboxes.Single ().Name, Is.EqualTo ("John"));
		}

		[TestCase (":;", TestName = "TestEmptyGroupName_Bare")]
		[TestCase (": a@example.com;", TestName = "TestEmptyGroupName_WithMembers")]
		[TestCase ("  : a@example.com;", TestName = "TestEmptyGroupName_LeadingWhitespace")]
		[TestCase ("(a comment): a@example.com;", TestName = "TestEmptyGroupName_CommentOnly")]
		public void TestEmptyGroupName (string value)
		{
			// Note: display-name is a phrase, which requires at least one word. Neither whitespace
			// nor a comment is a word.
			AssertViolation (value, MimeComplianceViolation.EmptyGroupName);
		}

		[TestCase ("Friends: a@example.com;")]
		[TestCase ("\"Undisclosed recipients\":;")]
		[TestCase ("Undisclosed recipients:;")]
		public void TestNonEmptyGroupNameIsSilent (string value)
		{
			var issues = Validate ("To", value);

			Assert.That (issues.Select (i => i.Violation), Has.None.EqualTo (MimeComplianceViolation.EmptyGroupName));
		}

		[Test]
		public void TestUnquotedCommaInDisplayName ()
		{
			// Note: Section 7.1.5 of rfc7103. Strictly, the comma splits this into a naked local-part
			// "Doe" followed by "John <j@example.com>", but the intent is an unquoted display-name,
			// so a narrow lookahead reports it as such.
			AssertViolation ("Doe, John <j@example.com>", MimeComplianceViolation.UnquotedDisplayName);

			var issues = Validate ("To", "Doe, John <j@example.com>");

			Assert.That (issues.Select (i => i.Violation), Has.None.EqualTo (MimeComplianceViolation.AddressWithoutDomain),
				"An unquoted comma in a display-name should not also be reported as a domainless address.");
			Assert.That (issues, Has.Count.EqualTo (1), "Expected exactly one violation.");
		}

		[Test]
		public void TestUnquotedCommaInDisplayNameInsideGroup ()
		{
			AssertViolation ("Friends: Doe, John <j@example.com>;", MimeComplianceViolation.UnquotedDisplayName);
		}

		[TestCase ("localuser, other@example.com")]
		[TestCase ("alice, bob, carol")]
		public void TestDomainlessListIsNotMistakenForADisplayName (string value)
		{
			// Note: The lookahead only fires when a name-addr follows, so a genuine list of
			// domainless addresses is still reported as such.
			var issues = Validate ("To", value);

			Assert.That (issues.Select (i => i.Violation), Has.Some.EqualTo (MimeComplianceViolation.AddressWithoutDomain));
			Assert.That (issues.Select (i => i.Violation), Has.None.EqualTo (MimeComplianceViolation.UnquotedDisplayName));
		}

		[Test]
		public void TestQualifiedListIsNotMistakenForADisplayName ()
		{
			// Note: The leading element has a domain, so the lookahead must not fire even though a
			// name-addr follows the comma.
			Assert.That (Validate ("To", "a@example.com, John <j@example.com>"), Is.Empty);
		}

		[Test]
		public void TestObsoleteDomainSyntax ()
		{
			// Note: rfc5322 obs-domain permits CFWS around the dots of a dot-atom domain.
			AssertViolation ("user@example . com", MimeComplianceViolation.ObsoleteDomainSyntax);
		}

		[Test]
		public void TestInvalid8BitAddress ()
		{
			// Note: 0xC0 0x20 is not a valid UTF-8 sequence, so rfc6532 gives it no interpretation.
			AssertViolation ("us\u00c0 er@example.com", MimeComplianceViolation.Invalid8BitAddress);
		}

		[Test]
		public void TestValidUtf8AddressIsSilent ()
		{
			// Note: rfc6532 permits UTF-8 in addresses, so this must not be reported as 8-bit.
			var text = "To: Jos\u00e9 <jos\u00e9@example.com>\r\n\r\nbody\r\n";
			var logger = new TestMimeComplianceLogger ();

			using (var stream = new MemoryStream (Encoding.UTF8.GetBytes (text), false)) {
				var reader = new MimeReader (stream) { ComplianceLogger = logger };

				reader.ReadMessage ();
			}

			Assert.That (logger.Issues.Select (i => i.Violation), Has.None.EqualTo (MimeComplianceViolation.Invalid8BitAddress));
		}

		[Test]
		public void TestNoLoggerMeansNoValidation ()
		{
			var text = "To: <<user@example.com>>\r\n\r\nbody\r\n";

			using var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false);

			Assert.DoesNotThrow (() => new MimeReader (stream).ReadMessage ());
		}

		[TestCase ("From")]
		[TestCase ("To")]
		[TestCase ("Cc")]
		[TestCase ("Bcc")]
		[TestCase ("Sender")]
		[TestCase ("Reply-To")]
		[TestCase ("Resent-From")]
		[TestCase ("Resent-To")]
		[TestCase ("Resent-Cc")]
		[TestCase ("Resent-Bcc")]
		[TestCase ("Resent-Sender")]
		[TestCase ("Disposition-Notification-To")]
		public void TestAllAddressHeadersAreValidated (string field)
		{
			var issues = Validate (field, "<<user@example.com>>");

			Assert.That (issues.Select (i => i.Violation), Has.Some.EqualTo (MimeComplianceViolation.ExcessiveAngleBracketsInAddress),
				$"{field} was not validated as an address header.");
		}

		[Test]
		public void TestReturnPathIsNotValidated ()
		{
			// Note: rfc5321 permits the empty path "<>" on bounce messages, which is not a valid
			// rfc5322 address, so Return-Path is deliberately excluded from address validation.
			var issues = Validate ("Return-Path", "<>");

			Assert.That (issues, Is.Empty);
		}

		[Test]
		public void TestNonAddressHeaderIsNotValidated ()
		{
			var issues = Validate ("Subject", "<<not an address>>");

			Assert.That (issues, Is.Empty);
		}

		[Test]
		public void TestViolationLocationInFoldedHeader ()
		{
			// Note: The defect is on the second physical line of the header, and the validator
			// counts line breaks on demand, so this guards the folded-header case.
			var text = "To: a@example.com,\r\n\tus..er@example.com\r\n\r\nbody\r\n";
			var logger = new TestMimeComplianceLogger ();

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var reader = new MimeReader (stream) { ComplianceLogger = logger };

				reader.ReadMessage ();
			}

			var issue = logger.Issues.Single (i => i.Violation == MimeComplianceViolation.InvalidLocalPart);

			Assert.That (issue.LineNumber, Is.EqualTo (2), "LineNumber");

			// Note: The offset points at the empty atom between the two dots, not at the start of
			// the address, so that it names the offending character.
			Assert.That (text.Substring ((int) issue.StreamOffset), Does.StartWith (".er@example.com"), "StreamOffset");

			// Note: "\tus.." - the tab is column 1, so the second dot is column 5.
			Assert.That (issue.ColumnNumber, Is.EqualTo (5), "ColumnNumber");
		}

		[TestCase ("To", "us..er@example.com", MimeComplianceViolation.InvalidLocalPart, 8)]
		[TestCase ("To", "user@example.com.", MimeComplianceViolation.TrailingDotInDomain, 22)]
		[TestCase ("Cc", "a@example.com b@example.com", MimeComplianceViolation.MissingAddressSeparator, 19)]
		[TestCase ("Bcc", "\"Jo\rhn\" <j@example.com>", MimeComplianceViolation.LineBreakInAddress, 9)]
		public void TestViolationColumnNumber (string field, string value, MimeComplianceViolation violation, int column)
		{
			// Note: The column is one-based and is relative to the start of the physical line, which
			// includes the header field name, so the first character of the value on the first line of
			// a header is at column field.Length + 3 (for the colon and the space).
			var text = $"{field}: {value}\r\n\r\nbody\r\n";
			var logger = new TestMimeComplianceLogger ();

			using (var stream = new MemoryStream (Encoding.Latin1.GetBytes (text), false)) {
				var reader = new MimeReader (stream) { ComplianceLogger = logger };

				reader.ReadMessage ();
			}

			var issue = logger.Issues.First (i => i.Violation == violation);

			Assert.That (issue.LineNumber, Is.EqualTo (1), "LineNumber");
			Assert.That (issue.ColumnNumber, Is.EqualTo (column), "ColumnNumber");

			// Note: The column and the stream offset must name the same byte.
			Assert.That (issue.StreamOffset, Is.EqualTo (issue.ColumnNumber - 1), "StreamOffset");
		}

		[Test]
		public void TestViolationLocationInSecondHeader ()
		{
			var text = "Subject: test\r\nTo: us..er@example.com\r\n\r\nbody\r\n";
			var logger = new TestMimeComplianceLogger ();

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var reader = new MimeReader (stream) { ComplianceLogger = logger };

				reader.ReadMessage ();
			}

			var issue = logger.Issues.Single (i => i.Violation == MimeComplianceViolation.InvalidLocalPart);

			Assert.That (issue.LineNumber, Is.EqualTo (2), "LineNumber");
			Assert.That (text.Substring ((int) issue.StreamOffset), Does.StartWith (".er@example.com"), "StreamOffset");
		}

		// Note: These all previously caused the validator to spin, logging the same violation at the
		// same offset forever. Since address headers are untrusted input parsed before any policy is
		// applied, failing to make forward progress is a denial-of-service vector rather than merely
		// a correctness bug.
		[TestCase ("user@[IPv6:2001:db8::1]")]
		[TestCase ("user@[IPv6:x]")]
		[TestCase ("Group: user@[IPv6:::]")]
		[TestCase ("a: b: c: d@[x:y]")]
		[TestCase ("<<<<<<<<")]
		[TestCase (">>>>>>>>")]
		[TestCase (",,,,,,,,")]
		[TestCase (";;;;;;;;")]
		[TestCase ("::::::::")]
		[TestCase ("@@@@@@@@")]
		[TestCase ("[[[[[[[[")]
		[TestCase ("\"\"\"\"\"\"\"\"")]
		[TestCase ("((((((((")]
		[TestCase ("........")]
		[TestCase ("g: a@b.com")]
		[TestCase ("g: ]")]
		[TestCase ("g: , ] ,")]
		public void TestValidatorAlwaysTerminates (string value)
		{
			var issues = Validate ("To", value);

			// A spin logs without bound, so any finite ceiling distinguishes it from a scan that
			// advances. The bound is deliberately generous because a run of garbage can legitimately
			// trip several distinct violations at the same offset.
			Assert.That (issues.Count, Is.LessThanOrEqualTo (10 * value.Length + 16),
				$"\"{value}\" produced {issues.Count} issues, which means the scan did not advance.");
		}

		[Test]
		public void TestMissingAddressSeparatorInsideGroup ()
		{
			AssertViolation ("Friends: a@example.com b@example.com;", MimeComplianceViolation.MissingAddressSeparator);
		}

		[Test]
		public void TestMultipleViolationsAreAllReported ()
		{
			var issues = Validate ("To", "us..er@example.com, <@hop:other@example.com>");
			var violations = issues.Select (i => i.Violation).ToList ();

			Assert.That (violations, Has.Some.EqualTo (MimeComplianceViolation.InvalidLocalPart));
			Assert.That (violations, Has.Some.EqualTo (MimeComplianceViolation.ObsoleteRouteAddress));
		}

		[Test]
		public void TestEmptyAddressHeaderIsSilent ()
		{
			Assert.That (Validate ("To", ""), Is.Empty);
			Assert.That (Validate ("To", "   "), Is.Empty);
		}

		// Note: A byte that no address production can consume - '[' outside a domain-literal, for
		// example - used to be re-parsed rather than stepped over. The address-list loop reported a
		// missing separator and re-entered the parser at the same byte, which re-walked the token and
		// described it a second time before the no-progress guard broke the spin. Each byte must be
		// described once.
		[TestCase ("a[b@example.com", TestName = "TestDescribedOnce_OpenBracket")]
		[TestCase ("a]b@example.com", TestName = "TestDescribedOnce_CloseBracket")]
		[TestCase ("a@example.com ] b@example.com", TestName = "TestDescribedOnce_BetweenAddresses")]
		[TestCase ("a@example.com ]] b@example.com", TestName = "TestDescribedOnce_RunBetweenAddresses")]
		[TestCase ("Group: a[b@example.com;", TestName = "TestDescribedOnce_InGroup")]
		public void TestEachByteIsDescribedOnlyOnce (string value)
		{
			var issues = Validate ("To", value);
			var duplicates = issues
				.GroupBy (i => (i.Violation, i.StreamOffset))
				.Where (g => g.Count () > 1)
				.Select (g => $"{g.Key.Violation} x{g.Count ()} @ {g.Key.StreamOffset}")
				.ToList ();

			Assert.That (duplicates, Is.Empty,
				$"Duplicate issues reported: {string.Join (", ", duplicates)}");
		}

		// Note: Stepping over the offending bytes must not swallow them, must not swallow the address
		// that follows, and must stop at a list separator rather than consuming it.
		[Test]
		public void TestByteThatCannotBeginAnAddressIsStillReported ()
		{
			var issues = Validate ("To", "a@example.com ] b@example.com");

			Assert.That (issues.Select (i => i.Violation), Has.Some.EqualTo (MimeComplianceViolation.MissingAddressSeparator));
		}

		[Test]
		public void TestSkippingUnparsableBytesStopsAtTheSeparator ()
		{
			// Note: A trailing comma is an empty final element, so it is reported. That report is
			// only reachable if the skip stopped at the comma instead of consuming it, which makes
			// it the observable proof that a separator survives the skip.
			var issues = Validate ("To", "a@example.com ],");

			Assert.That (issues.Select (i => i.Violation), Has.Some.EqualTo (MimeComplianceViolation.ExtraneousCommaInAddressList),
				$"The comma should still be seen as a separator, but got: {string.Join (", ", issues.Select (i => i.Violation))}");
		}

		// Note: An angle-addr on either side of a missing separator makes the recovery genuinely
		// ambiguous, because the grammar allows everything in front of an angle-addr to be read as
		// its display-name. A parser that recovers that way sees one mailbox where a parser that
		// splits sees two, and the two disagree about which mailbox the address names.
		[TestCase ("<spoofer@example.com> <impersonated@example.com>")]
		[TestCase ("<spoofer@example.com> impersonated@example.com")]
		[TestCase ("<spoofer@example.com> \"Real User\" <impersonated@example.com>")]
		[TestCase ("<spoofer@example.com> Real User <impersonated@example.com>")]
		[TestCase ("<spoofer@example.com> Friends: impersonated@example.com;")]
		[TestCase ("John <a@example.com> Jane <b@example.com>")]
		[TestCase ("Friends: a@example.com; <impersonated@example.com>")]
		public void TestMissingSeparatorBesideAnAngleAddrIsAmbiguous (string value)
		{
			AssertViolation (value, MimeComplianceViolation.MissingAddressSeparator);
			AssertViolation (value, MimeComplianceViolation.AmbiguousMailboxBoundary);
		}

		// Note: Two bare addr-specs have no grammatical relationship to each other, so folding them
		// into a single mailbox requires a production rfc5322 does not offer. Parsers may still
		// differ over how many mailboxes to recover, but that is a disagreement about count rather
		// than identity. Reporting the ambiguity here would attach a security signal to the ordinary
		// missing comma, which is exactly what keeping the two violations separate is meant to avoid.
		[TestCase ("a@example.com b@example.com")]
		[TestCase ("a@example.com b@example.com c@example.com")]
		public void TestMissingSeparatorBetweenBareAddressesIsNotAmbiguous (string value)
		{
			AssertViolation (value, MimeComplianceViolation.MissingAddressSeparator);

			var issues = Validate ("To", value);

			Assert.That (issues.Select (i => i.Violation), Has.None.EqualTo (MimeComplianceViolation.AmbiguousMailboxBoundary),
				$"\"{value}\" has no conformant single-mailbox reading, so it should not be reported as ambiguous.");
		}

		// Note: A display-name that is itself an address is legal rfc5322, so nothing here is
		// malformed. The report exists because software that shows the display-name in place of the
		// address shows a mailbox that will not receive the reply.
		[TestCase ("\"admin@example.com\" <attacker@example.org>")]
		[TestCase ("admin@example.com <attacker@example.org>")]
		[TestCase ("\"<admin@example.com>\" <attacker@example.org>")]
		[TestCase ("\"admin@example.com (Administrator)\" <attacker@example.org>")]
		[TestCase ("\"(admin@example.com)\" <attacker@example.org>")]
		public void TestAddressShapedDisplayNameIsReported (string value)
		{
			AssertViolation (value, MimeComplianceViolation.AddressInDisplayName);
		}

		[TestCase ("\"admin@example.com\": attacker@example.org;")]
		[TestCase ("admin@example.com: attacker@example.org;")]
		public void TestAddressShapedGroupDisplayNameIsReported (string value)
		{
			AssertViolation (value, MimeComplianceViolation.AddressInGroupDisplayName);
		}

		// Note: A display-name is free-form text, so an '@' in it is only a signal when it sits
		// between atom text and a dotted domain the way an addr-spec does.
		[TestCase ("\"Real User\" <user@example.com>")]
		[TestCase ("\"admin at example.com\" <user@example.com>")]
		[TestCase ("\"Bob @ Work\" <user@example.com>")]
		[TestCase ("\"@channel\" <user@example.com>")]
		[TestCase ("\"admin@localhost\" <user@example.com>")]
		[TestCase ("Friends: a@example.com;")]
		public void TestOrdinaryDisplayNameIsNotReportedAsAnAddress (string value)
		{
			var issues = Validate ("To", value);

			Assert.That (issues.Select (i => i.Violation), Has.None.EqualTo (MimeComplianceViolation.AddressInDisplayName),
				$"\"{value}\" is not shaped like an address.");
			Assert.That (issues.Select (i => i.Violation), Has.None.EqualTo (MimeComplianceViolation.AddressInGroupDisplayName),
				$"\"{value}\" is not shaped like an address.");
		}

		// Note: These report different things about the same header, so an input that is both
		// malformed and misleading must produce both. UnquotedDisplayName describes the syntax;
		// AddressInDisplayName describes what the value will be mistaken for.
		[Test]
		public void TestUnquotedAddressShapedDisplayNameIsReportedTwice ()
		{
			var issues = Validate ("To", "admin@example.com <attacker@example.org>");
			var violations = issues.Select (i => i.Violation).ToList ();

			Assert.That (violations, Has.Some.EqualTo (MimeComplianceViolation.UnquotedDisplayName));
			Assert.That (violations, Has.Some.EqualTo (MimeComplianceViolation.AddressInDisplayName));
		}

		// Note: The quoted form breaks no rule, so it is rated as a hint. The category is what a
		// transport filters on, not the severity.
		[Test]
		public void TestAddressShapedDisplayNameIsAMinorSecurityIssue ()
		{
			var issues = Validate ("To", "\"admin@example.com\" <attacker@example.org>");
			var issue = issues.Single (i => i.Violation == MimeComplianceViolation.AddressInDisplayName);

			Assert.That (issue.Severity, Is.EqualTo (MimeComplianceSeverity.Minor));
			Assert.That (issue.Categories.HasFlag (MimeComplianceCategories.Security), Is.True);
		}

		[Test]
		public void TestAmbiguousMailboxBoundaryIsASecurityIssue ()
		{
			var issues = Validate ("To", "<spoofer@example.com> <impersonated@example.com>");
			var issue = issues.Single (i => i.Violation == MimeComplianceViolation.AmbiguousMailboxBoundary);

			Assert.That (issue.Categories.HasFlag (MimeComplianceCategories.Security), Is.True);
		}

		// Note: A group name labels a list, a display-name labels a mailbox, and software that
		// mistakes either for the address it is shown beside is making a different mistake with a
		// different remedy. A consumer must be able to tell them apart without re-parsing.
		[Test]
		public void TestAddressShapedGroupNameAndDisplayNameAreDistinguishable ()
		{
			var mailbox = Validate ("To", "admin@example.com <attacker@example.org>");
			var group = Validate ("To", "admin@example.com: attacker@example.org;");

			Assert.That (mailbox.Select (i => i.Violation), Is.Not.EquivalentTo (group.Select (i => i.Violation)),
				"A group name and a display-name that are both shaped like an address report identically.");
		}
	}
}

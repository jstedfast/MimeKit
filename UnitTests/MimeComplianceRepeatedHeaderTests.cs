//
// MimeComplianceRepeatedHeaderTests.cs
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
	public class MimeComplianceRepeatedHeaderTests
	{
		// Note: RFC 5322, Section 3.6 limits each of these header fields to a single occurrence.
		// Listing them explicitly keeps this fixture from silently absorbing violations added later
		// for unrelated reasons.
		static readonly HashSet<MimeComplianceViolation> RepeatedHeaderViolations = new HashSet<MimeComplianceViolation> {
			MimeComplianceViolation.RepeatedDate,
			MimeComplianceViolation.RepeatedFrom,
			MimeComplianceViolation.RepeatedSender,
			MimeComplianceViolation.RepeatedReplyTo,
			MimeComplianceViolation.RepeatedTo,
			MimeComplianceViolation.RepeatedCc,
			MimeComplianceViolation.RepeatedBcc,
			MimeComplianceViolation.RepeatedMessageId,
			MimeComplianceViolation.RepeatedInReplyTo,
			MimeComplianceViolation.RepeatedReferences,
			MimeComplianceViolation.RepeatedSubject,
			MimeComplianceViolation.RepeatedReturnPath,
			MimeComplianceViolation.RepeatedResentDate,
			MimeComplianceViolation.RepeatedResentFrom,
			MimeComplianceViolation.RepeatedResentSender,
			MimeComplianceViolation.RepeatedResentTo,
			MimeComplianceViolation.RepeatedResentCc,
			MimeComplianceViolation.RepeatedResentBcc,
			MimeComplianceViolation.RepeatedResentMessageId
		};

		static List<MimeComplianceIssue> Read (string text)
		{
			var logger = new TestMimeComplianceLogger ();

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var reader = new MimeReader (stream) { ComplianceLogger = logger };

				reader.ReadMessage ();
			}

			return logger.Issues.Where (issue => RepeatedHeaderViolations.Contains (issue.Violation)).ToList ();
		}

		static async Task<List<MimeComplianceIssue>> ReadAsync (string text)
		{
			var logger = new TestMimeComplianceLogger ();

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var reader = new MimeReader (stream) { ComplianceLogger = logger };

				await reader.ReadMessageAsync ();
			}

			return logger.Issues.Where (issue => RepeatedHeaderViolations.Contains (issue.Violation)).ToList ();
		}

		static void AssertViolations (string text, params MimeComplianceViolation[] expected)
		{
			var issues = Read (text);

			Assert.That (issues.Select (issue => issue.Violation), Is.EqualTo (expected), "sync");

			var async = ReadAsync (text).GetAwaiter ().GetResult ();

			Assert.That (async.Select (issue => issue.Violation), Is.EqualTo (expected), "async");
		}

		[Test]
		public void TestRepeatedFrom ()
		{
			var text = "From: one@example.com\r\nTo: two@example.com\r\nFrom: three@example.com\r\n\r\nbody\r\n";
			var issues = Read (text);

			Assert.That (issues, Has.Count.EqualTo (1));
			Assert.That (issues[0].Violation, Is.EqualTo (MimeComplianceViolation.RepeatedFrom));

			// Note: The issue is located at the start of the repeated header field, not the original.
			Assert.That (issues[0].StreamOffset, Is.EqualTo (text.IndexOf ("From: three")));
			Assert.That (issues[0].LineNumber, Is.EqualTo (3));
			Assert.That (issues[0].ColumnNumber, Is.EqualTo (1));
		}

		[Test]
		public void TestEachRepetitionIsReported ()
		{
			AssertViolations (
				"From: one@example.com\r\nFrom: two@example.com\r\nFrom: three@example.com\r\n\r\nbody\r\n",
				MimeComplianceViolation.RepeatedFrom,
				MimeComplianceViolation.RepeatedFrom);
		}

		[TestCase ("Date", MimeComplianceViolation.RepeatedDate)]
		[TestCase ("Sender", MimeComplianceViolation.RepeatedSender)]
		[TestCase ("Reply-To", MimeComplianceViolation.RepeatedReplyTo)]
		[TestCase ("To", MimeComplianceViolation.RepeatedTo)]
		[TestCase ("Cc", MimeComplianceViolation.RepeatedCc)]
		[TestCase ("Bcc", MimeComplianceViolation.RepeatedBcc)]
		[TestCase ("Message-Id", MimeComplianceViolation.RepeatedMessageId)]
		[TestCase ("In-Reply-To", MimeComplianceViolation.RepeatedInReplyTo)]
		[TestCase ("References", MimeComplianceViolation.RepeatedReferences)]
		[TestCase ("Subject", MimeComplianceViolation.RepeatedSubject)]
		[TestCase ("Return-Path", MimeComplianceViolation.RepeatedReturnPath)]
		public void TestRepeatedField (string field, MimeComplianceViolation expected)
		{
			AssertViolations ($"From: one@example.com\r\n{field}: x\r\n{field}: y\r\n\r\nbody\r\n", expected);
		}

		[Test]
		public void TestUnrepeatableFieldsAreNotReported ()
		{
			// Note: RFC 5322 places no limit on these, and Received in particular is expected to
			// appear once per hop.
			AssertViolations ("Received: by a\r\nReceived: by b\r\nComments: x\r\nComments: y\r\nKeywords: a\r\nKeywords: b\r\nFrom: one@example.com\r\n\r\nbody\r\n");
		}

		[Test]
		public void TestResentFieldsAreLimitedPerBlock ()
		{
			AssertViolations (
				"From: one@example.com\r\nResent-From: a@example.com\r\nResent-Date: Mon, 1 Jan 2024 00:00:00 +0000\r\nResent-From: b@example.com\r\n\r\nbody\r\n",
				MimeComplianceViolation.RepeatedResentFrom);
		}

		[Test]
		public void TestSeparatedResentBlocksAreNotReported ()
		{
			// Note: A non-resent header field ends the current block of resent fields, so the same
			// field may appear again in the block that follows.
			AssertViolations ("Resent-From: b@example.com\r\nResent-To: y@example.com\r\nReceived: by relay\r\nResent-From: a@example.com\r\nResent-To: x@example.com\r\nFrom: one@example.com\r\n\r\nbody\r\n");
		}

		[Test]
		public void TestAdjacentResentBlocksCannotBeDistinguished ()
		{
			// Note: RFC 5322 provides no way to delimit adjacent blocks of resent fields, so two
			// blocks that are not separated by any other header field are indistinguishable from a
			// single block containing a repeated field. This documents that known limitation.
			AssertViolations (
				"Resent-From: b@example.com\r\nResent-From: a@example.com\r\nFrom: one@example.com\r\n\r\nbody\r\n",
				MimeComplianceViolation.RepeatedResentFrom);
		}

		[Test]
		public void TestNonResentFieldEndsTheResentBlockButNotTheMessage ()
		{
			// Note: The per-message counts are not reset by the end of a block of resent fields.
			AssertViolations (
				"From: one@example.com\r\nResent-From: a@example.com\r\nFrom: two@example.com\r\n\r\nbody\r\n",
				MimeComplianceViolation.RepeatedFrom);
		}

		[Test]
		public void TestRepeatedHeaderIsReportedInATruncatedMessage ()
		{
			// Note: When a message ends without a body separator, StepHeaderValue sets the parser
			// state to Content before the final header is created. The check therefore cannot be
			// written in terms of the current parser state; MimeReader captures whether the block
			// belonged to a message when it begins reading it.
			AssertViolations ("From: a@example.com\r\nFrom: b@example.com", MimeComplianceViolation.RepeatedFrom);
			AssertViolations ("From: a@example.com\r\nFrom: b@example.com\r\n", MimeComplianceViolation.RepeatedFrom);
		}

		[Test]
		public void TestEntityHeadersAreNotChecked ()
		{
			// Note: The header field counts in RFC 5322 constrain a message, not a MIME entity.
			AssertViolations ("From: one@example.com\r\nContent-Type: multipart/mixed; boundary=\"b\"\r\n\r\n--b\r\nTo: x@example.com\r\nTo: y@example.com\r\nSubject: a\r\nSubject: b\r\n\r\ncontent\r\n--b--\r\n");
		}

		[Test]
		public void TestEmbeddedMessageHeadersAreChecked ()
		{
			AssertViolations (
				"From: one@example.com\r\nContent-Type: message/rfc822\r\n\r\nFrom: a@example.com\r\nFrom: b@example.com\r\n\r\nbody\r\n",
				MimeComplianceViolation.RepeatedFrom);
		}

		[Test]
		public void TestEmbeddedMessageDoesNotInheritTheOuterHeaders ()
		{
			// Note: The counts are per header block, so a field that appears once in the outer
			// message and once in an embedded message is not a repeat.
			AssertViolations ("From: one@example.com\r\nContent-Type: message/rfc822\r\n\r\nFrom: a@example.com\r\nSubject: inner\r\n\r\nbody\r\n");
		}
	}
}

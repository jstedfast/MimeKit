//
// InternetAddressTests.cs
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

using System.Text;

using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class InternetAddressTests
	{
		static void AssertParseFailure (string text, bool result, int tokenIndex, int errorIndex)
		{
			var buffer = text.Length > 0 ? Encoding.UTF8.GetBytes (text) : new byte[1];
			InternetAddress address;

			Assert.That (InternetAddress.TryParse (text, out address), Is.EqualTo (result), "InternetAddress.TryParse(string)");
			Assert.That (InternetAddress.TryParse (buffer, out address), Is.EqualTo (result), "InternetAddress.TryParse(byte[])");
			Assert.That (InternetAddress.TryParse (buffer, 0, out address), Is.EqualTo (result), "InternetAddress.TryParse(byte[], int)");
			Assert.That (InternetAddress.TryParse (buffer, 0, buffer.Length, out address), Is.EqualTo (result), "InternetAddress.TryParse(byte[], int, int)");

			try {
				InternetAddress.Parse (text);
				Assert.Fail ("InternetAddress.Parse(string) should fail.");
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "ParseException did not have the correct token index.");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "ParseException did not have the correct error index.");
			} catch {
				Assert.Fail ("InternetAddress.Parse(string) should throw ParseException.");
			}

			try {
				InternetAddress.Parse (buffer);
				Assert.Fail ("InternetAddress.Parse(byte[]) should fail.");
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "ParseException did not have the correct token index.");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "ParseException did not have the correct error index.");
			} catch {
				Assert.Fail ("InternetAddress.Parse(new byte[]) should throw ParseException.");
			}

			try {
				InternetAddress.Parse (buffer, 0);
				Assert.Fail ("InternetAddress.Parse(byte[], int) should fail.");
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "ParseException did not have the correct token index.");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "ParseException did not have the correct error index.");
			} catch {
				Assert.Fail ("InternetAddress.Parse(new byte[], int) should throw ParseException.");
			}

			try {
				InternetAddress.Parse (buffer, 0, buffer.Length);
				Assert.Fail ("InternetAddress.Parse(byte[], int, int) should fail.");
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "ParseException did not have the correct token index.");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "ParseException did not have the correct error index.");
			} catch {
				Assert.Fail ("InternetAddress.Parse(new byte[], int, int) should throw ParseException.");
			}
		}

		static void AssertParse (string text)
		{
			var buffer = Encoding.UTF8.GetBytes (text);
			InternetAddress address;

			try {
				Assert.That (InternetAddress.TryParse (text, out address), Is.True, "InternetAddress.TryParse(string) should succeed.");
			} catch (Exception ex) {
				Assert.Fail ($"InternetAddress.TryParse(string) should not throw an exception: {ex}");
			}

			try {
				Assert.That (InternetAddress.TryParse (buffer, out address), Is.True, "InternetAddress.TryParse(byte[]) should succeed.");
			} catch (Exception ex) {
				Assert.Fail ($"InternetAddress.TryParse(byte[]) should not throw an exception: {ex}");
			}

			try {
				Assert.That (InternetAddress.TryParse (buffer, 0, out address), Is.True, "InternetAddress.TryParse(byte[], int) should succeed.");
			} catch (Exception ex) {
				Assert.Fail ($"InternetAddress.TryParse(byte[], int) should not throw an exception: {ex}");
			}

			try {
				Assert.That (InternetAddress.TryParse (buffer, 0, buffer.Length, out address), Is.True, "InternetAddress.TryParse(byte[], int, int) should succeed.");
			} catch (Exception ex) {
				Assert.Fail ($"InternetAddress.TryParse(byte[], int, int) should not throw an exception: {ex}");
			}

			try {
				address = InternetAddress.Parse (text);
			} catch (Exception ex) {
				Assert.Fail ($"InternetAddress.Parse(string) should not throw an exception: {ex}");
			}

			try {
				address = InternetAddress.Parse (buffer);
			} catch (Exception ex) {
				Assert.Fail ($"InternetAddress.Parse(string) should not throw an exception: {ex}");
			}

			try {
				address = InternetAddress.Parse (buffer, 0);
			} catch (Exception ex) {
				Assert.Fail ($"InternetAddress.Parse(string) should not throw an exception: {ex}");
			}

			try {
				address = InternetAddress.Parse (buffer, 0, buffer.Length);
			} catch (Exception ex) {
				Assert.Fail ($"InternetAddress.Parse(string) should not throw an exception: {ex}");
			}
		}

		[Test]
		public void TestParseEmpty ()
		{
			AssertParseFailure (string.Empty, false, 0, 0);
		}

		[Test]
		public void TestParseWhiteSpace ()
		{
			const string text = " \t\r\n";
			int tokenIndex = text.Length;
			int errorIndex = text.Length;

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseNameLessThan ()
		{
			const string text = "Name <";
			const int tokenIndex = 0;
			int errorIndex = text.Length;

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseMailboxWithEmptyDomain ()
		{
			const string text = "jeff@";
			const int tokenIndex = 0;
			int errorIndex = text.Length;

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseMailboxWithIncompleteLocalPart ()
		{
			const string text = "jeff.";
			const int tokenIndex = 0;
			int errorIndex = text.Length;

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseIncompleteQuotedString ()
		{
			const string text = "\"This quoted string never ends... oh no!";
			const int tokenIndex = 0;
			int errorIndex = text.Length;

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseMailboxWithIncompleteCommentAfterName ()
		{
			const string text = "Name (incomplete comment";
			int tokenIndex = text.IndexOf ('(');
			int errorIndex = text.Length;

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseMailboxWithIncompleteCommentAfterAddrspec ()
		{
			const string text = "jeff@xamarin.com (incomplete comment";
			int tokenIndex = text.IndexOf ('(');
			int errorIndex = text.Length;

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseMailboxWithIncompleteCommentAfterDomainLiteralAddrspec ()
		{
			const string text = "jeff@[127.0.0.1] (incomplete comment";
			int tokenIndex = text.IndexOf ('(');
			int errorIndex = text.Length;

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseMailboxWithIncompleteCommentAfterAddress ()
		{
			const string text = "<jeff@xamarin.com> (incomplete comment";
			int tokenIndex = text.IndexOf ('(');
			int errorIndex = text.Length;

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseIncompleteAddrspec ()
		{
			const string text = "jeff@ (comment)";
			const int tokenIndex = 0;
			int errorIndex = text.Length;

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseIncompleteRoutedMailboxAt ()
		{
			const string text = "Name <@";
			const int tokenIndex = 0;
			int errorIndex = text.Length;

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseIncompleteRoutedMailbox ()
		{
			const string text = "Name <@route:";
			const int tokenIndex = 0;
			int errorIndex = text.Length;

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseIncompleteRoutedMailboxSpace ()
		{
			const string text = "Name <@route: ";
			const int tokenIndex = 0;
			int errorIndex = text.Length;

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseIncompleteCommentInRoute ()
		{
			const string text = "Name <@route,(comment";
			const int tokenIndex = 0;
			int errorIndex = text.Length;

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseInvalidRouteInMailbox ()
		{
			const string text = "Name <@route,invalid:user@example.com>";
			const int tokenIndex = 0;
			int errorIndex = text.IndexOf (',') + 1;

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseMailboxWithInternationalRoute ()
		{
			const string text = "User Name <@route,@伊昭傑@郵件.商務:user@domain.com>";

			AssertParse (text);
		}

		[Test]
		public void TestParseAddrspecNoAtDomain ()
		{
			const string text = "jeff";

			AssertParse (text);
		}

		[Test]
		public void TestParseAddrspecNoAtDomainGreaterThan ()
		{
			const string text = "jeff>";

			AssertParse (text);
		}

		[Test]
		public void TestParseAddrspecNoAtDomainWithIncompleteComment ()
		{
			const string text = "jeff (Jeffrey Stedfast";
			int tokenIndex = 5;
			int errorIndex = text.Length;

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseAddrspecNoAtDomainWithComment ()
		{
			const string text = "jeff (Jeffrey Stedfast)";

			AssertParse (text);

			var mailbox = MailboxAddress.Parse (text);

			Assert.That (mailbox.Name, Is.EqualTo ("Jeffrey Stedfast"));
			Assert.That (mailbox.Address, Is.EqualTo ("jeff"));
		}

		[Test]
		public void TestParseAddrspec ()
		{
			const string text = "jeff@xamarin.com";

			AssertParse (text);
		}

		[Test]
		public void TestParseMailbox ()
		{
			const string text = "Jeffrey Stedfast <jestedfa@microsoft.com>";

			AssertParse (text);
		}

		[Test]
		public void TestParseMailboxWithUnquotedCommaAndDotInName ()
		{
			const string text = "Warren Worthington, Jr. <warren@worthington.com>";

			AssertParse (text);
		}

		[Test]
		public void TestParseMailboxWithUnquotedCommaInName ()
		{
			const string text = "Worthington, Warren <warren@worthington.com>";
			InternetAddress addr;

			AssertParse (text);

			// default options should parse this as a single mailbox address
			addr = InternetAddress.Parse (text);
			Assert.That (addr.Name, Is.EqualTo ("Worthington, Warren"));

			// this should fail when we allow mailbox addresses w/o a domain
			var options = ParserOptions.Default.Clone ();
			options.AllowUnquotedCommasInAddresses = false;
			options.AllowAddressesWithoutDomain = false;

			try {
				addr = InternetAddress.Parse (options, text);
				Assert.Fail ($"Should not have parsed \"{text}\" with AllowUnquotedCommasInAddresses = false");
			} catch (ParseException pex) {
				Assert.That (pex.TokenIndex, Is.EqualTo (0), "TokenIndex");
				Assert.That (pex.ErrorIndex, Is.EqualTo (text.IndexOf (',')), "ErrorIndex");
			} catch (Exception ex) {
				Assert.Fail ($"Should not have thrown {ex.GetType ().Name}");
			}
		}

		[Test]
		public void TestParseMailboxWithOpenAngleSpace ()
		{
			const string text = "Jeffrey Stedfast < jeff@xamarin.com>";

			AssertParse (text);
		}

		[Test]
		public void TestParseMailboxWithCloseAngleSpace ()
		{
			const string text = "Jeffrey Stedfast <jeff@xamarin.com >";

			AssertParse (text);
		}

		[Test]
		public void TestParseMailboxWithIncompleteRoute ()
		{
			const string text = "Skye <@";
			const int tokenIndex = 0;
			int errorIndex = text.Length;

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseMailboxWithoutColonAfterRoute ()
		{
			const string text = "Skye <@hackers.com,@shield.gov";
			const int tokenIndex = 0;
			int errorIndex = text.Length;

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseMultipleMailboxes ()
		{
			const string text = "Skye <skye@shield.gov>, Leo Fitz <fitz@shield.gov>, Melinda May <may@shield.gov>";
			int tokenIndex = text.IndexOf (',');
			int errorIndex = tokenIndex;

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseGroup ()
		{
			const string text = "Agents of Shield: Skye <skye@shield.gov>, Leo Fitz <fitz@shield.gov>, Melinda May <may@shield.gov>;";

			AssertParse (text);
		}

		[Test]
		public void TestParseIncompleteGroup ()
		{
			const string text = "Agents of Shield: Skye <skye@shield.gov>, Leo Fitz <fitz@shield.gov>, May <may@shield.gov>";

			AssertParse (text);
		}

		[Test]
		public void TestParseGroupNameColon ()
		{
			const string text = "Agents of Shield:";
			int tokenIndex = text.Length;
			int errorIndex = text.Length;

			// Note: the TryParse() methods are a little more forgiving than Parse().
			AssertParseFailure (text, true, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseGroupAndMailbox ()
		{
			const string text = "Agents of Shield: Skye <skye@shield.gov>, Leo Fitz <fitz@shield.gov>, May <may@shield.gov>;, Fury <fury@shield.gov>";
			int tokenIndex = text.IndexOf (';') + 1;
			int errorIndex = tokenIndex;

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		#region Rfc7103

		// TODO: test both Strict and Loose RfcCompliance modes

		[Test]
		public void TestParseMailboxWithExcessiveAngleBrackets ()
		{
			const string text = "<<<user2@example.org>>>";

			AssertParse (text);
		}

		[Test]
		public void TestParseMailboxWithMissingGreaterThan ()
		{
			const string text = "<another@example.net";

			AssertParse (text);
		}

		[Test]
		public void TestParseMailboxWithMissingLessThan ()
		{
			const string text = "second@example.org>";

			AssertParse (text);
		}

		[Test]
		public void TestParseMailboxWithUnbalancedQuotes ()
		{
			const string text = "\"Joe <joe@example.com>";

			AssertParse (text);
		}

		[Test]
		public void TestParseMailboxWithAddrspecAsUnquotedName ()
		{
			const string text = "user@example.com <user@example.com>";

			AssertParse (text);
		}

		#endregion

		[Test]
		public void TestParseMailboxWithSquareBracketsInDisplayName ()
		{
			const string text = "[Invalid Sender] <sender@tk2-201-10422.vs.sakura.ne.jp>";

			AssertParse (text);
		}
	}
}

//
// GroupAddressTests.cs
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
	public class GroupAddressTests
	{
		static void AssertParseFailure (string text, bool result, int tokenIndex, int errorIndex)
		{
			var buffer = text.Length > 0 ? Encoding.UTF8.GetBytes (text) : new byte[1];

			Assert.That (GroupAddress.TryParse (text, out _), Is.EqualTo (result), "GroupAddress.TryParse(string)");
			Assert.That (GroupAddress.TryParse (buffer, out _), Is.EqualTo (result), "GroupAddress.TryParse(byte[])");
			Assert.That (GroupAddress.TryParse (buffer, 0, out _), Is.EqualTo (result), "GroupAddress.TryParse(byte[], int)");
			Assert.That (GroupAddress.TryParse (buffer, 0, buffer.Length, out _), Is.EqualTo (result), "GroupAddress.TryParse(byte[], int, int)");

			try {
				GroupAddress.Parse (text);
				Assert.Fail ("GroupAddress.Parse(string) should fail.");
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "ParseException did not have the correct token index.");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "ParseException did not have the correct error index.");
			} catch {
				Assert.Fail ("GroupAddress.Parse(string) should throw ParseException.");
			}

			try {
				GroupAddress.Parse (buffer);
				Assert.Fail ("GroupAddress.Parse(byte[]) should fail.");
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "ParseException did not have the correct token index.");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "ParseException did not have the correct error index.");
			} catch {
				Assert.Fail ("GroupAddress.Parse(new byte[]) should throw ParseException.");
			}

			try {
				GroupAddress.Parse (buffer, 0);
				Assert.Fail ("GroupAddress.Parse(byte[], int) should fail.");
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "ParseException did not have the correct token index.");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "ParseException did not have the correct error index.");
			} catch {
				Assert.Fail ("GroupAddress.Parse(new byte[], int) should throw ParseException.");
			}

			try {
				GroupAddress.Parse (buffer, 0, buffer.Length);
				Assert.Fail ("GroupAddress.Parse(byte[], int, int) should fail.");
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "ParseException did not have the correct token index.");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "ParseException did not have the correct error index.");
			} catch {
				Assert.Fail ("GroupAddress.Parse(new byte[], int, int) should throw ParseException.");
			}
		}

		static void AssertParse (string text)
		{
			var buffer = Encoding.UTF8.GetBytes (text);
			GroupAddress group;

			try {
				Assert.That (GroupAddress.TryParse (text, out group), Is.True, "GroupAddress.TryParse(string) should succeed.");
			} catch (Exception ex) {
				Assert.Fail ($"GroupAddress.TryParse(string) should not throw an exception: {ex}");
			}

			try {
				Assert.That (GroupAddress.TryParse (buffer, out group), Is.True, "GroupAddress.TryParse(byte[]) should succeed.");
			} catch (Exception ex) {
				Assert.Fail ($"GroupAddress.TryParse(byte[]) should not throw an exception: {ex}");
			}

			try {
				Assert.That (GroupAddress.TryParse (buffer, 0, out group), Is.True, "GroupAddress.TryParse(byte[], int) should succeed.");
			} catch (Exception ex) {
				Assert.Fail ($"GroupAddress.TryParse(byte[], int) should not throw an exception: {ex}");
			}

			try {
				Assert.That (GroupAddress.TryParse (buffer, 0, buffer.Length, out group), Is.True, "GroupAddress.TryParse(byte[], int, int) should succeed.");
			} catch (Exception ex) {
				Assert.Fail ($"GroupAddress.TryParse(byte[], int, int) should not throw an exception: {ex}");
			}

			try {
				group = GroupAddress.Parse (text);
			} catch (Exception ex) {
				Assert.Fail ($"GroupAddress.Parse(string) should not throw an exception: {ex}");
			}

			try {
				group = GroupAddress.Parse (buffer);
			} catch (Exception ex) {
				Assert.Fail ($"GroupAddress.Parse(string) should not throw an exception: {ex}");
			}

			try {
				group = GroupAddress.Parse (buffer, 0);
			} catch (Exception ex) {
				Assert.Fail ($"GroupAddress.Parse(string) should not throw an exception: {ex}");
			}

			try {
				group = GroupAddress.Parse (buffer, 0, buffer.Length);
			} catch (Exception ex) {
				Assert.Fail ($"GroupAddress.Parse(string) should not throw an exception: {ex}");
			}
		}

		[Test]
		public void TestClone ()
		{
			const string encoded = "Group Name: First Name <first@address.com>, Second Name <second@address.com>,\n Inner Group Name: First Inner Name <first-inner@address.com>,\n Second Inner Name <second-inner@address.com>;, Third Name <third@address.com>;";
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;
			options.International = true;

			var inner = new GroupAddress ("Inner Group Name");
			inner.Members.Add (new MailboxAddress ("First Inner Name", "first-inner@address.com"));
			inner.Members.Add (new MailboxAddress ("Second Inner Name", "second-inner@address.com"));

			var group = new GroupAddress ("Group Name");
			group.Members.Add (new MailboxAddress ("First Name", "first@address.com"));
			group.Members.Add (new MailboxAddress ("Second Name", "second@address.com"));
			group.Members.Add (inner);
			group.Members.Add (new MailboxAddress ("Third Name", "third@address.com"));

			var clone = group.Clone ();

			Assert.That (group.CompareTo (clone), Is.EqualTo (0), "CompareTo");

			var actual = clone.ToString (options, true);

			Assert.That (actual, Is.EqualTo (encoded), "Encode");
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
			int errorIndex = text.IndexOf ('<');

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseMailboxWithEmptyDomain ()
		{
			const string text = "jeff@";
			const int tokenIndex = 0;
			int errorIndex = text.IndexOf ('@');

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
			const int tokenIndex = 0;
			int errorIndex = text.IndexOf ('@');

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseMailboxWithIncompleteCommentAfterAddress ()
		{
			const string text = "<jeff@xamarin.com> (incomplete comment";
			const int tokenIndex = 0;
			const int errorIndex = 0;

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseIncompleteAddrspec ()
		{
			const string text = "jeff@ (comment)";
			const int tokenIndex = 0;
			int errorIndex = text.IndexOf ('@');

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseAddrspecNoAtDomain ()
		{
			const string text = "jeff";
			const int tokenIndex = 0;
			int errorIndex = text.Length;

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseAddrspec ()
		{
			const string text = "jeff@xamarin.com";
			const int tokenIndex = 0;
			int errorIndex = text.IndexOf ('@');

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseMailbox ()
		{
			const string text = "Jeffrey Stedfast <jestedfa@microsoft.com>";
			const int tokenIndex = 0;
			int errorIndex = text.IndexOf ('<');

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseMailboxWithUnquotedCommaAndDotInName ()
		{
			const string text = "Warren Worthington, Jr. <warren@worthington.com>";
			const int tokenIndex = 0;
			int errorIndex = text.IndexOf ('<');

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseMailboxWithOpenAngleSpace ()
		{
			const string text = "Jeffrey Stedfast < jeff@xamarin.com>";
			const int tokenIndex = 0;
			int errorIndex = text.IndexOf ('<');

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseMailboxWithCloseAngleSpace ()
		{
			const string text = "Jeffrey Stedfast <jeff@xamarin.com >";
			const int tokenIndex = 0;
			int errorIndex = text.IndexOf ('<');

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseMailboxWithIncompleteRoute ()
		{
			const string text = "Skye <@";
			const int tokenIndex = 0;
			int errorIndex = text.IndexOf ('<');

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseMailboxWithoutColonAfterRoute ()
		{
			const string text = "Skye <@hackers.com,@shield.gov";
			const int tokenIndex = 0;
			int errorIndex = text.IndexOf ('<');

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
			const string text = "Agents of Shield: Skye <skye@shield.gov>, Leo Fitz <fitz@shield.gov>, Melinda May <may@shield.gov>";

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

		[Test]
		public void TestDefaultMaxGroupDepthOverflow ()
		{
			const string overflow = "group0: group1: group2: group3: milbox@host.com;;;;";
			const string safe = "group0: group1: group2: milbox@host.com;;;";

			AssertParse (safe);

			AssertParseFailure (overflow, false, 24, 30);
		}
	}
}

//
// InternetAddressParserTests.cs
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
	public class InternetAddressParserTests
	{
		[Test]
		public void TestPrefixVersusCompleteInput ()
		{
			const string text = "a@example.com, b@example.com";
			var buffer = Encoding.UTF8.GetBytes ("###" + text + "###");
			int endIndex = buffer.Length - 3;
			int index = 3;

			Assert.That (InternetAddress.TryParse (AddressParserFlags.TryParse, ParserOptions.Default, buffer, ref index, endIndex, 0, out var address), Is.True);
			Assert.That (((MailboxAddress) address).Address, Is.EqualTo ("a@example.com"));
			Assert.That (index, Is.EqualTo (3 + text.IndexOf (',')));
			Assert.That (InternetAddress.TryParse (buffer, 3, text.Length, out _), Is.False);
			Assert.That (MailboxAddress.TryParse (buffer, 3, text.Length, out _), Is.False);
			Assert.That (InternetAddressList.Parse (buffer, 3, text.Length), Has.Count.EqualTo (2));
		}

		[Test]
		public void TestStrictUnterminatedGroupTryParseVersusParse ()
		{
			const string text = "Friends: a@example.com";
			var options = new ParserOptions { AddressParserComplianceMode = RfcComplianceMode.Strict };
			var buffer = Encoding.UTF8.GetBytes ("###" + text + "###");

			Assert.That (InternetAddressList.TryParse (options, buffer, 3, text.Length, out var list), Is.True);
			Assert.That (((GroupAddress) list[0]).Members, Has.Count.EqualTo (1));
			Assert.That (GroupAddress.TryParse (options, buffer, 3, text.Length, out var group), Is.True);
			Assert.That (group.Members, Has.Count.EqualTo (1));

			var ex = Assert.Throws<ParseException> (() => InternetAddressList.Parse (options, buffer, 3, text.Length));
			Assert.That (ex.TokenIndex, Is.EqualTo (3));
			Assert.That (ex.ErrorIndex, Is.EqualTo (3 + text.Length));
			ex = Assert.Throws<ParseException> (() => GroupAddress.Parse (options, buffer, 3, text.Length));
			Assert.That (ex.TokenIndex, Is.EqualTo (3));
			Assert.That (ex.ErrorIndex, Is.EqualTo (3 + text.Length));
		}

		[Test]
		public void TestInternalRecoveryVersusPublicFailure ()
		{
			const string text = "first@example.com, \"Last, First\" (no address), last@example.com";
			var options = new ParserOptions {
				AddressParserComplianceMode = RfcComplianceMode.Strict,
				AllowAddressesWithoutDomain = false
			};
			var buffer = Encoding.UTF8.GetBytes ("###" + text + "###");
			int index = 3;

			Assert.That (InternetAddressList.TryParse (options, buffer, 3, text.Length, out var list), Is.False);
			Assert.That (list, Is.Null);
			Assert.That (InternetAddressList.TryParse (AddressParserFlags.InternalTryParse, options, buffer, ref index, 3 + text.Length, false, 0, out var addresses), Is.True);
			Assert.That (index, Is.EqualTo (3 + text.Length));
			Assert.That (addresses, Has.Count.EqualTo (2));
			Assert.That (((MailboxAddress) addresses[0]).Address, Is.EqualTo ("first@example.com"));
			Assert.That (((MailboxAddress) addresses[1]).Address, Is.EqualTo ("last@example.com"));
		}

		[Test]
		public void TestGroupListLeavesTerminator ()
		{
			const string text = "a@example.com;ignored@example.com";
			var buffer = Encoding.UTF8.GetBytes (text);
			int index = 0;

			Assert.That (InternetAddressList.TryParse (AddressParserFlags.TryParse, ParserOptions.Default, buffer, ref index, buffer.Length, true, 1, out var addresses), Is.True);
			Assert.That (addresses, Has.Count.EqualTo (1));
			Assert.That (index, Is.EqualTo (text.IndexOf (';')));
		}

		[Test]
		public void TestNestedGroupsAndEncodedNames ()
		{
			const string text = "Outer: =?iso-8859-1?q?Andr=E9?=: =?utf-8?b?IlxOYW1lIg==?= <a@xn--bcher-kva.example>;;";
			var options = new ParserOptions { MaxAddressGroupDepth = 2 };
			var list = InternetAddressList.Parse (options, text);
			var outer = (GroupAddress) list[0];
			var inner = (GroupAddress) outer.Members[0];
			var mailbox = (MailboxAddress) inner.Members[0];

			Assert.That (outer.Name, Is.EqualTo ("Outer"));
			Assert.That (inner.Name, Is.EqualTo ("Andr\u00e9"));
			Assert.That (inner.Encoding.CodePage, Is.EqualTo (28591));
			Assert.That (mailbox.Name, Is.EqualTo ("\"\\Name\""));
			Assert.That (mailbox.Address, Is.EqualTo ("a@xn--bcher-kva.example"));
		}

		[TestCase (RfcComplianceMode.Strict, false)]
		[TestCase (RfcComplianceMode.Loose, true)]
		[TestCase (RfcComplianceMode.Looser, true)]
		public void TestNonUtf8LocalPart (RfcComplianceMode compliance, bool expected)
		{
			var buffer = new byte[] { 0xe9, (byte) '@', (byte) 'a' };
			var options = new ParserOptions { AddressParserComplianceMode = compliance };

			Assert.That (MailboxAddress.TryParse (options, buffer, out var mailbox), Is.EqualTo (expected));
			if (expected)
				Assert.That (mailbox.Address, Is.EqualTo ("\u00e9@a"));
			else
				Assert.That (mailbox, Is.Null);
		}

		[Test]
		public void TestStringErrorOffsetsAreUtf8ByteOffsets ()
		{
			const string text = "\"\u00e9\" <a@";
			var options = new ParserOptions { AddressParserComplianceMode = RfcComplianceMode.Strict };
			var buffer = Encoding.UTF8.GetBytes (text);
			var fromString = Assert.Throws<ParseException> (() => InternetAddress.Parse (options, text));
			var fromBytes = Assert.Throws<ParseException> (() => InternetAddress.Parse (options, buffer));

			Assert.That (fromString.TokenIndex, Is.EqualTo (6));
			Assert.That (fromString.ErrorIndex, Is.EqualTo (buffer.Length));
			Assert.That (fromString.TokenIndex, Is.EqualTo (fromBytes.TokenIndex));
			Assert.That (fromString.ErrorIndex, Is.EqualTo (fromBytes.ErrorIndex));
		}

		[TestCase (false)]
		[TestCase (true)]
		public void TestFailurePreservesCursor (bool throwOnError)
		{
			const string text = "Name <a@";
			var buffer = Encoding.UTF8.GetBytes ("###" + text + "###");
			var options = new ParserOptions { AddressParserComplianceMode = RfcComplianceMode.Strict };
			int index = 3;

			if (throwOnError) {
				var ex = Assert.Throws<ParseException> (() => InternetAddress.TryParse (AddressParserFlags.Parse, options, buffer, ref index, 3 + text.Length, 0, out _));
				Assert.That (ex.ErrorIndex, Is.EqualTo (3 + text.Length));
			} else {
				Assert.That (InternetAddress.TryParse (AddressParserFlags.TryParse, options, buffer, ref index, 3 + text.Length, 0, out var address), Is.False);
				Assert.That (address, Is.Null);
			}

			Assert.That (index, Is.EqualTo (3 + text.Length));
		}
	}
}

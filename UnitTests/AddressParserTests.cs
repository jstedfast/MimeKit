//
// AddressParserTests.cs
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
	public class AddressParserTests
	{
		static string Describe (InternetAddress address)
		{
			var description = address.GetType ().Name + "|" + address.Name + "|" + address.Encoding.CodePage;

			if (address is MailboxAddress mailbox)
				return description + "|" + mailbox.Address + "|" + string.Join (",", mailbox.Route);

			foreach (var member in ((GroupAddress) address).Members)
				description += "[" + Describe (member) + "]";

			return description;
		}

		static string Describe (InternetAddressList addresses)
		{
			return string.Join (";", addresses.Select (Describe));
		}

		static string ParseOutcome (Func<string> parse)
		{
			try {
				return parse ();
			} catch (ParseException ex) {
				return ex.Message + "|" + ex.TokenIndex + "|" + ex.ErrorIndex + "|" + ex.InnerException?.GetType ().Name;
			}
		}

		[Test]
		public void TestStringAndByteInputEquivalence (
			[Values (RfcComplianceMode.Strict, RfcComplianceMode.Loose, RfcComplianceMode.Looser)] RfcComplianceMode compliance,
			[Values (RfcComplianceMode.Strict, RfcComplianceMode.Loose)] RfcComplianceMode encodedWords,
			[Values (false, true)] bool unquotedCommas)
		{
			var inputs = new List<string> {
				"", " \t\r\n", "(ignored) a@example.com", "a@example.com (A\tB)",
				"\"First\\ Last\" <a@example.com>", "\"A\r\n B\" <a@example.com>",
				"Andr\u00e9 <\u00e9@b\u00fccher.example>", "\U0001f600 <a@example.com>",
				"\ud800 <a@example.com>", "\udc00 <a@example.com>", "\ud800\\\udc00 <a@example.com>",
				"\"\ud800\\\udc00\" <a@example.com>", "\ud800@example.com", "\udc00@example.com",
				"a@\ud800.example", "a@example.com (\ud800\\\udc00)",
				"a@example.com (\u00e9)", "=?iso-8859-1?q?Andr=E9?= <a@example.com>",
				"=?utf-8?q?First?= \t =?utf-8?q?_Last?= <a@example.com>",
				"\"=?utf-8?b?IlxOYW1lIg==?=\" <a@example.com>", "a@example.com (=?utf-8?q?A_B?=)",
				"Name <@\u00e9.example,@b.example:\u00e9@example.com>",
				"\u00e9: \U0001f600: \u00e9@example.com;;", "\u00e9: a@example.com",
				"\"\u00e9\" <a@", "\u00e9 <a@[\u00e9]>", "\u00e9 <a@example.com\u00a0>",
				"\u00e9 <a@example.com> \U0001f600", "\u00e9 <a@example.com> (unterminated",
				"\"bad\0name\" <a@example.com>", "\u00e9: \"bad\\\nname\" <a@example.com>;",
				"a\\@b@example.com", "a..b@example.com", "\"Unclosed <a@example.com>",
				"first@example.com, \"Last, First\" (no address), last@example.com"
			};

			var random = new Random (941);
			string[] fragments = { "\"", "\\", "(", ")", ":", ";", ",", "<", ">", "@", ".", " ", "\t", "\r\n", "=?", "?=", "\ud800", "\udc00", "\U0001f600", "\u00e9", "\0" };
			int seedCount = inputs.Count;

			for (int i = 0; i < 512; i++) {
				string input = inputs[random.Next (seedCount)];
				int position = random.Next (input.Length + 1);
				string fragment = i < 128 ? ((char) i).ToString () : fragments[random.Next (fragments.Length)];

				inputs.Add (input.Insert (position, fragment));
				inputs.Add ("\"" + (char) random.Next (0x10000) + "\" <a@example.com>");
			}

			foreach (bool allowLocal in new[] { false, true }) {
				foreach (var charset in new[] { Encoding.UTF8, Encoding.GetEncoding (1252) }) {
					var options = new ParserOptions {
						AddressParserComplianceMode = compliance,
						Rfc2047ComplianceMode = encodedWords,
						AllowUnquotedCommasInAddresses = unquotedCommas,
						AllowAddressesWithoutDomain = allowLocal,
						CharsetEncoding = charset
					};

					foreach (var input in inputs) {
						var bytes = Encoding.UTF8.GetBytes (input);
						string context = $"{compliance}, {allowLocal}, {charset.CodePage}, {input}";

						Assert.That (InternetAddress.TryParse (options, input, out var address), Is.EqualTo (InternetAddress.TryParse (options, bytes, out var byteAddress)), context);
						Assert.That (address == null ? null : Describe (address), Is.EqualTo (byteAddress == null ? null : Describe (byteAddress)), context);
						Assert.That (MailboxAddress.TryParse (options, input, out var mailbox), Is.EqualTo (MailboxAddress.TryParse (options, bytes, out var byteMailbox)), context);
						Assert.That (mailbox == null ? null : Describe (mailbox), Is.EqualTo (byteMailbox == null ? null : Describe (byteMailbox)), context);
						Assert.That (GroupAddress.TryParse (options, input, out var group), Is.EqualTo (GroupAddress.TryParse (options, bytes, out var byteGroup)), context);
						Assert.That (group == null ? null : Describe (group), Is.EqualTo (byteGroup == null ? null : Describe (byteGroup)), context);
						Assert.That (InternetAddressList.TryParse (options, input, out var list), Is.EqualTo (InternetAddressList.TryParse (options, bytes, out var byteList)), context);
						Assert.That (list == null ? null : Describe (list), Is.EqualTo (byteList == null ? null : Describe (byteList)), context);

						Assert.That (ParseOutcome (() => Describe (InternetAddress.Parse (options, input))), Is.EqualTo (ParseOutcome (() => Describe (InternetAddress.Parse (options, bytes)))), context);
						Assert.That (ParseOutcome (() => Describe (MailboxAddress.Parse (options, input))), Is.EqualTo (ParseOutcome (() => Describe (MailboxAddress.Parse (options, bytes)))), context);
						Assert.That (ParseOutcome (() => Describe (GroupAddress.Parse (options, input))), Is.EqualTo (ParseOutcome (() => Describe (GroupAddress.Parse (options, bytes)))), context);
						Assert.That (ParseOutcome (() => Describe (InternetAddressList.Parse (options, input))), Is.EqualTo (ParseOutcome (() => Describe (InternetAddressList.Parse (options, bytes)))), context);
					}
				}
			}
		}

		static long MeasureParsingAllocation (string input)
		{
			long before = GC.GetAllocatedBytesForCurrentThread ();

			for (int i = 0; i < 10; i++)
				InternetAddressList.Parse (input);

			return GC.GetAllocatedBytesForCurrentThread () - before;
		}

		static long MeasureMailboxAllocation (Func<MailboxAddress> parse)
		{
			long before = GC.GetAllocatedBytesForCurrentThread ();

			for (int i = 0; i < 10; i++)
				parse ();

			return GC.GetAllocatedBytesForCurrentThread () - before;
		}

		[Test]
		public void TestContiguousAddrSpecAvoidsIntermediateStrings ([Values (false, true)] bool byteInput)
		{
			const string contiguous = "first.last@example.com";
			const string commented = "first.last@(ignored)example.com";
			var contiguousBytes = Encoding.UTF8.GetBytes (contiguous);
			var commentedBytes = Encoding.UTF8.GetBytes (commented);
			Func<MailboxAddress> parseContiguous = byteInput ? () => MailboxAddress.Parse (contiguousBytes) : () => MailboxAddress.Parse (contiguous);
			Func<MailboxAddress> parseCommented = byteInput ? () => MailboxAddress.Parse (commentedBytes) : () => MailboxAddress.Parse (commented);

			Assert.That (Describe (parseContiguous ()), Is.EqualTo (Describe (parseCommented ())));
			MeasureMailboxAllocation (parseContiguous);
			MeasureMailboxAllocation (parseCommented);

			Assert.That (MeasureMailboxAllocation (parseContiguous), Is.LessThanOrEqualTo (MeasureMailboxAllocation (parseCommented) - 1280));
		}

		[TestCase ("first.last@example.com", "first.last@example.com")]
		[TestCase ("first (comment) . last @ example . com", "first.last@example.com")]
		[TestCase ("\"quoted local\"@example.com", "\"quoted local\"@example.com")]
		[TestCase ("local@example.com.", "local@example.com")]
		[TestCase ("local@example.com (name)", "local@example.com")]
		[TestCase ("local@example. com", "local@example.com")]
		[TestCase ("local@exa (comment) . mple", "local@exa.mple")]
		[TestCase ("local@[127.0.0.1]", "local@[127.0.0.1]")]
		[TestCase ("\u00e9@example.com", "\u00e9@example.com")]
		[TestCase ("local@b\u00fccher.example", "local@b\u00fccher.example")]
		public void TestContiguousAddrSpecAndFallback (string text, string expected)
		{
			var bytes = Encoding.UTF8.GetBytes ("###" + text + "###");

			foreach (var compliance in new[] { RfcComplianceMode.Strict, RfcComplianceMode.Loose, RfcComplianceMode.Looser }) {
				var options = new ParserOptions { AddressParserComplianceMode = compliance };
				var fromString = MailboxAddress.Parse (options, text);
				var fromBytes = MailboxAddress.Parse (options, bytes, 3, bytes.Length - 6);

				Assert.That (fromString.Address, Is.EqualTo (expected));
				Assert.That (Describe (fromString), Is.EqualTo (Describe (fromBytes)));
			}
		}

		[TestCase ("local@example.com", "@", "local", 5, -1)]
		[TestCase ("local@example.com", "c", "local@example", 14, 5)]
		[TestCase ("local@example..com", ".", "local@example", 14, 5)]
		[TestCase ("local@example.com,next", ",", "local@example.com", 17, 5)]
		public void TestContiguousAddrSpecSentinels (string text, string terminators, string expected, int position, int at)
		{
			var sentinels = Encoding.ASCII.GetBytes (terminators);
			var bytes = Encoding.ASCII.GetBytes (text);
			var stringParser = AddressParser.Create (ParserOptions.Default, text);
			var byteParser = AddressParser.Create (ParserOptions.Default, bytes, 0, bytes.Length);

			Assert.That (stringParser.TryParseAddrspec (sentinels, RfcComplianceMode.Strict, true, out var fromString, out int stringAt), Is.True);
			Assert.That (byteParser.TryParseAddrspec (sentinels, RfcComplianceMode.Strict, true, out var fromBytes, out int byteAt), Is.True);
			Assert.That (fromString, Is.EqualTo (expected));
			Assert.That (fromBytes, Is.EqualTo (expected));
			Assert.That (stringParser.Position, Is.EqualTo (position));
			Assert.That (byteParser.Position, Is.EqualTo (position));
			Assert.That (stringAt, Is.EqualTo (at));
			Assert.That (byteAt, Is.EqualTo (at));
		}

		[Test]
		public void TestStringParsingDoesNotTranscodeIgnoredInput ()
		{
			const string shortInput = "(ignored) a@example.com";
			string longInput = "(" + new string ('x', 32768) + ") a@example.com";

			Assert.That (Describe (InternetAddressList.Parse (longInput)), Is.EqualTo (Describe (InternetAddressList.Parse (shortInput))));
			MeasureParsingAllocation (shortInput);
			MeasureParsingAllocation (longInput);

			Assert.That (MeasureParsingAllocation (longInput), Is.LessThanOrEqualTo (MeasureParsingAllocation (shortInput) + 1024));
		}

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

		[TestCase ("", "", 65001, false)]
		[TestCase ("", "", 65001, true)]
		[TestCase ("\"Last, First\"", "Last, First", 65001, false)]
		[TestCase ("\"Last, First\"", "Last, First", 65001, true)]
		[TestCase ("=?iso-8859-1?q?Andr=E9?=", "Andr\u00e9", 28591, false)]
		[TestCase ("=?iso-8859-1?q?Andr=E9?=", "Andr\u00e9", 28591, true)]
		[TestCase ("\"=?utf-8?b?IlxOYW1lIg==?=\"", "\"\\Name\"", 65001, false)]
		[TestCase ("\"=?utf-8?b?IlxOYW1lIg==?=\"", "\"\\Name\"", 65001, true)]
		[TestCase ("=?utf-8?q?First?= \t =?utf-8?q?_Last?=", "First Last", 65001, false)]
		[TestCase ("=?utf-8?q?First?= \t =?utf-8?q?_Last?=", "First Last", 65001, true)]
		[TestCase ("\"Unclosed", "Unclosed", 65001, false)]
		[TestCase ("\"Unclosed", "Unclosed", 65001, true)]
		public void TestPhraseDecodingForMailboxAndGroup (string phrase, string name, int codepage, bool group)
		{
			string text = group ? phrase + ": a@example.com;" : phrase + " <a@example.com>";
			var address = InternetAddress.Parse (text);

			Assert.That (address.Name, Is.EqualTo (name));
			Assert.That (address.Encoding.CodePage, Is.EqualTo (codepage));
			Assert.That (address is GroupAddress, Is.EqualTo (group));
			if (group)
				Assert.That (((MailboxAddress) ((GroupAddress) address).Members[0]).Address, Is.EqualTo ("a@example.com"));
			else
				Assert.That (((MailboxAddress) address).Address, Is.EqualTo ("a@example.com"));
		}

		[TestCase (RfcComplianceMode.Strict)]
		[TestCase (RfcComplianceMode.Loose)]
		[TestCase (RfcComplianceMode.Looser)]
		public void TestAddrSpecReclassifiedAsDisplayName (RfcComplianceMode compliance)
		{
			const string text = "name@example.com (Comment) <actual@example.com>";
			var options = new ParserOptions { AddressParserComplianceMode = compliance };

			if (compliance == RfcComplianceMode.Strict) {
				Assert.That (InternetAddress.TryParse (options, text, out _), Is.False);
				var ex = Assert.Throws<ParseException> (() => InternetAddress.Parse (options, text));
				Assert.That (ex.TokenIndex, Is.EqualTo (0));
				Assert.That (ex.ErrorIndex, Is.EqualTo (text.IndexOf ('<')));
			} else {
				var address = (MailboxAddress) InternetAddress.Parse (options, text);
				Assert.That (address.Name, Is.EqualTo ("name@example.com (Comment)"));
				Assert.That (address.Address, Is.EqualTo ("actual@example.com"));
			}
		}

		[Test]
		public void TestInternalGroupRecoveryLeavesGroupTerminator ()
		{
			const string text = "\"bad, name\" (no address); next@example.com";
			var buffer = Encoding.UTF8.GetBytes (text);
			var options = new ParserOptions {
				AddressParserComplianceMode = RfcComplianceMode.Strict,
				AllowAddressesWithoutDomain = false
			};
			int index = 0;

			Assert.That (InternetAddressList.TryParse (AddressParserFlags.InternalTryParse, options, buffer, ref index, buffer.Length, true, 1, out var members), Is.True);
			Assert.That (members, Is.Empty);
			Assert.That (index, Is.EqualTo (text.IndexOf (';')));
		}
	}
}

//
// MailboxAddressTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2021 .NET Foundation and Contributors
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
using System.Collections.Generic;

using NUnit.Framework;

using MimeKit;
using MimeKit.Cryptography;
using MimeKit.Utils;

namespace UnitTests {
	[TestFixture]
	public class MailboxAddressTests
	{
		[Test]
		public void ArgumentExceptionTests ()
		{
			var mailbox = new MailboxAddress ("Johnny Appleseed", "johnny@example.com");
			var route = new [] { "route.com" };

			Assert.Throws<ArgumentNullException> (() => new MailboxAddress (null, "name", route, "johnny@example.com"));
			Assert.Throws<ArgumentNullException> (() => new MailboxAddress (Encoding.UTF8, "name", null, "johnny@example.com"));
			Assert.Throws<ArgumentNullException> (() => new MailboxAddress (Encoding.UTF8, "name", route, null));

			Assert.Throws<ArgumentNullException> (() => new MailboxAddress ("name", null, "johnny@example.com"));
			Assert.Throws<ArgumentNullException> (() => new MailboxAddress ("name", route, null));

			Assert.Throws<ArgumentNullException> (() => new MailboxAddress (null, "name", "johnny@example.com"));
			Assert.Throws<ArgumentNullException> (() => new MailboxAddress (Encoding.UTF8, "name", null));

			Assert.Throws<ArgumentNullException> (() => new MailboxAddress ("name", null));

			Assert.Throws<ArgumentNullException> (() => mailbox.Address = null);

			Assert.Throws<ArgumentNullException> (() => mailbox.Encoding = null);

			Assert.Throws<ArgumentNullException> (() => mailbox.CompareTo (null));

			Assert.Throws<ArgumentNullException> (() => mailbox.ToString (null, true));

			Assert.Throws<ArgumentNullException> (() => MailboxAddress.EncodeAddrspec (null));
			Assert.Throws<ArgumentNullException> (() => MailboxAddress.DecodeAddrspec (null));

			// SecureMailboxAddress
			Assert.Throws<ArgumentNullException> (() => new SecureMailboxAddress (null, "name", route, "johnny@example.com", "ffff"));
			Assert.Throws<ArgumentNullException> (() => new SecureMailboxAddress (Encoding.UTF8, "name", null, "johnny@example.com", "ffff"));
			Assert.Throws<ArgumentNullException> (() => new SecureMailboxAddress (Encoding.UTF8, "name", route, null, "ffff"));
			Assert.Throws<ArgumentNullException> (() => new SecureMailboxAddress (Encoding.UTF8, "name", route, "johnny@example.com", null));
			Assert.Throws<ArgumentException> (() => new SecureMailboxAddress (Encoding.UTF8, "name", route, "johnny@example.com", "not hex encoded"));

			Assert.Throws<ArgumentNullException> (() => new SecureMailboxAddress ("name", null, "johhny@example.com", "ffff"));
			Assert.Throws<ArgumentNullException> (() => new SecureMailboxAddress ("name", route, null, "ffff"));
			Assert.Throws<ArgumentNullException> (() => new SecureMailboxAddress ("name", route, "johnny@example.com", null));
			Assert.Throws<ArgumentException> (() => new SecureMailboxAddress ("name", route, "johnny@example.com", "not hex encoded"));

			Assert.Throws<ArgumentNullException> (() => new SecureMailboxAddress (null, "name", "johnny@example.com", "ffff"));
			Assert.Throws<ArgumentNullException> (() => new SecureMailboxAddress (Encoding.UTF8, "name", null, "ffff"));
			Assert.Throws<ArgumentNullException> (() => new SecureMailboxAddress (Encoding.UTF8, "name", "johnny@example.com", null));
			Assert.Throws<ArgumentException> (() => new SecureMailboxAddress (Encoding.UTF8, "name", "johnny@example.com", "not hex encoded"));

			Assert.Throws<ArgumentNullException> (() => new SecureMailboxAddress ("name", null, "ffff"));
			Assert.Throws<ArgumentNullException> (() => new SecureMailboxAddress ("name", "johnny@example.com", null));
			Assert.Throws<ArgumentException> (() => new SecureMailboxAddress ("name", "johnny@example.com", "not hex encoded"));

			Assert.DoesNotThrow (() => new SecureMailboxAddress ("Mailbox Address", "user@domain.com", "ffff"));
			Assert.DoesNotThrow (() => new SecureMailboxAddress (Encoding.UTF8, "Mailbox Address", "user@domain.com", "ffff"));
			Assert.DoesNotThrow (() => new SecureMailboxAddress ("Routed Address", new[] { "route1", "route2", "route3" }, "user@domain.com", "ffff"));
			Assert.DoesNotThrow (() => new SecureMailboxAddress (Encoding.UTF8, "Routed Address", new[] { "route1", "route2", "route3" }, "user@domain.com", "ffff"));
		}

		[Test]
		public void TestSetEmptyAddress ()
		{
			Assert.DoesNotThrow(() => new MailboxAddress ("Postmaster", string.Empty));

			var mailbox = new MailboxAddress ("Postmaster", string.Empty);

			Assert.IsFalse (mailbox.IsInternational, "IsInternational");
		}

		[Test]
		public void TestGarbageAfterAddress ()
		{
			try {
				new MailboxAddress ("Name", "fejj@helixcode.com garbage");
				Assert.Fail ("Expected a ParseException");
			} catch (ParseException ex) {
				Assert.AreEqual (19, ex.TokenIndex, "TokenIndex");
				Assert.AreEqual (19, ex.ErrorIndex, "ErrorIndex");
			} catch (Exception ex) {
				Assert.Fail ($"Unexpected exception: {ex}");
			}
		}

		[Test]
		public void TestCastToMailAddress ()
		{
			var mailbox = new MailboxAddress (CharsetUtils.Latin1, "æøå", "user@example.com");
			var address = (System.Net.Mail.MailAddress) mailbox;

			Assert.AreEqual (mailbox.Address, address.Address, "Address");
			Assert.AreEqual (mailbox.Name, address.DisplayName, "DisplayName");
		}

		static void AssertParseFailure (string text, bool result, int tokenIndex, int errorIndex, RfcComplianceMode mode = RfcComplianceMode.Loose)
		{
			var buffer = text.Length > 0 ? Encoding.UTF8.GetBytes (text) : new byte[1];
			var options = ParserOptions.Default.Clone ();
			MailboxAddress mailbox;

			options.AddressParserComplianceMode = mode;

			Assert.AreEqual (result, MailboxAddress.TryParse (options, text, out mailbox), "MailboxAddress.TryParse(string)");
			Assert.AreEqual (result, MailboxAddress.TryParse (options, buffer, out mailbox), "MailboxAddress.TryParse(byte[])");
			Assert.AreEqual (result, MailboxAddress.TryParse (options, buffer, 0, out mailbox), "MailboxAddress.TryParse(byte[], int)");
			Assert.AreEqual (result, MailboxAddress.TryParse (options, buffer, 0, buffer.Length, out mailbox), "MailboxAddress.TryParse(byte[], int, int)");

			try {
				MailboxAddress.Parse (options, text);
				Assert.Fail ("MailboxAddress.Parse(string) should fail.");
			} catch (ParseException ex) {
				Assert.AreEqual (tokenIndex, ex.TokenIndex, "ParseException did not have the correct token index.");
				Assert.AreEqual (errorIndex, ex.ErrorIndex, "ParseException did not have the correct error index.");
			} catch {
				Assert.Fail ("MailboxAddress.Parse(string) should throw ParseException.");
			}

			try {
				MailboxAddress.Parse (options, buffer);
				Assert.Fail ("MailboxAddress.Parse(byte[]) should fail.");
			} catch (ParseException ex) {
				Assert.AreEqual (tokenIndex, ex.TokenIndex, "ParseException did not have the correct token index.");
				Assert.AreEqual (errorIndex, ex.ErrorIndex, "ParseException did not have the correct error index.");
			} catch {
				Assert.Fail ("MailboxAddress.Parse(new byte[]) should throw ParseException.");
			}

			try {
				MailboxAddress.Parse (options, buffer, 0);
				Assert.Fail ("MailboxAddress.Parse(byte[], int) should fail.");
			} catch (ParseException ex) {
				Assert.AreEqual (tokenIndex, ex.TokenIndex, "ParseException did not have the correct token index.");
				Assert.AreEqual (errorIndex, ex.ErrorIndex, "ParseException did not have the correct error index.");
			} catch {
				Assert.Fail ("MailboxAddress.Parse(new byte[], int) should throw ParseException.");
			}

			try {
				MailboxAddress.Parse (options, buffer, 0, buffer.Length);
				Assert.Fail ("MailboxAddress.Parse(byte[], int, int) should fail.");
			} catch (ParseException ex) {
				Assert.AreEqual (tokenIndex, ex.TokenIndex, "ParseException did not have the correct token index.");
				Assert.AreEqual (errorIndex, ex.ErrorIndex, "ParseException did not have the correct error index.");
			} catch {
				Assert.Fail ("MailboxAddress.Parse(new byte[], int, int) should throw ParseException.");
			}
		}

		static void AssertParse (string text)
		{
			var buffer = Encoding.UTF8.GetBytes (text);
			MailboxAddress mailbox;

			try {
				Assert.IsTrue (MailboxAddress.TryParse (text, out mailbox), "MailboxAddress.TryParse(string) should succeed.");
			} catch (Exception ex) {
				Assert.Fail ("MailboxAddress.TryParse(string) should not throw an exception: {0}", ex);
			}

			try {
				Assert.IsTrue (MailboxAddress.TryParse (buffer, out mailbox), "MailboxAddress.TryParse(byte[]) should succeed.");
			} catch (Exception ex) {
				Assert.Fail ("MailboxAddress.TryParse(byte[]) should not throw an exception: {0}", ex);
			}

			try {
				Assert.IsTrue (MailboxAddress.TryParse (buffer, 0, out mailbox), "MailboxAddress.TryParse(byte[], int) should succeed.");
			} catch (Exception ex) {
				Assert.Fail ("MailboxAddress.TryParse(byte[], int) should not throw an exception: {0}", ex);
			}

			try {
				Assert.IsTrue (MailboxAddress.TryParse (buffer, 0, buffer.Length, out mailbox), "MailboxAddress.TryParse(byte[], int, int) should succeed.");
			} catch (Exception ex) {
				Assert.Fail ("MailboxAddress.TryParse(byte[], int, int) should not throw an exception: {0}", ex);
			}

			try {
				mailbox = MailboxAddress.Parse (text);
			} catch (Exception ex) {
				Assert.Fail ("MailboxAddress.Parse(string) should not throw an exception: {0}", ex);
			}

			try {
				mailbox = MailboxAddress.Parse (buffer);
			} catch (Exception ex) {
				Assert.Fail ("MailboxAddress.Parse(string) should not throw an exception: {0}", ex);
			}

			try {
				mailbox = MailboxAddress.Parse (buffer, 0);
			} catch (Exception ex) {
				Assert.Fail ("MailboxAddress.Parse(string) should not throw an exception: {0}", ex);
			}

			try {
				mailbox = MailboxAddress.Parse (buffer, 0, buffer.Length);
			} catch (Exception ex) {
				Assert.Fail ("MailboxAddress.Parse(string) should not throw an exception: {0}", ex);
			}
		}

		static void AssertParse (string text, RfcComplianceMode mode)
		{
			var buffer = Encoding.UTF8.GetBytes (text);
			var options = ParserOptions.Default.Clone ();
			MailboxAddress mailbox;

			options.AddressParserComplianceMode = mode;

			try {
				Assert.IsTrue (MailboxAddress.TryParse (options, text, out mailbox), "MailboxAddress.TryParse(ParserOptions, string) should succeed.");
			} catch (Exception ex) {
				Assert.Fail ("MailboxAddress.TryParse(ParserOptions, string) should not throw an exception: {0}", ex);
			}

			try {
				Assert.IsTrue (MailboxAddress.TryParse (options, buffer, out mailbox), "MailboxAddress.TryParse(ParserOptions, byte[]) should succeed.");
			} catch (Exception ex) {
				Assert.Fail ("MailboxAddress.TryParse(ParserOptions, byte[]) should not throw an exception: {0}", ex);
			}

			try {
				Assert.IsTrue (MailboxAddress.TryParse (options, buffer, 0, out mailbox), "MailboxAddress.TryParse(ParserOptions, byte[], int) should succeed.");
			} catch (Exception ex) {
				Assert.Fail ("MailboxAddress.TryParse(ParserOptions, byte[], int) should not throw an exception: {0}", ex);
			}

			try {
				Assert.IsTrue (MailboxAddress.TryParse (options, buffer, 0, buffer.Length, out mailbox), "MailboxAddress.TryParse(ParserOptions, byte[], int, int) should succeed.");
			} catch (Exception ex) {
				Assert.Fail ("MailboxAddress.TryParse(ParserOptions, byte[], int, int) should not throw an exception: {0}", ex);
			}

			try {
				mailbox = MailboxAddress.Parse (options, text);
			} catch (Exception ex) {
				Assert.Fail ("MailboxAddress.Parse(ParserOptions, string) should not throw an exception: {0}", ex);
			}

			try {
				mailbox = MailboxAddress.Parse (options, buffer);
			} catch (Exception ex) {
				Assert.Fail ("MailboxAddress.Parse(ParserOptions, string) should not throw an exception: {0}", ex);
			}

			try {
				mailbox = MailboxAddress.Parse (options, buffer, 0);
			} catch (Exception ex) {
				Assert.Fail ("MailboxAddress.Parse(ParserOptions, string) should not throw an exception: {0}", ex);
			}

			try {
				mailbox = MailboxAddress.Parse (options, buffer, 0, buffer.Length);
			} catch (Exception ex) {
				Assert.Fail ("MailboxAddress.Parse(ParserOptions, string) should not throw an exception: {0}", ex);
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
		public void TestParseIdnAddress ()
		{
			const string encoded = "user@xn--v8jxj3d1dzdz08w.com";
			const string expected = "user@名がドメイン.com";
			MailboxAddress mailbox;

			Assert.IsTrue (MailboxAddress.TryParse (encoded, out mailbox));
			Assert.AreEqual (expected, mailbox.Address);
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
			int tokenIndex = 0;
			int errorIndex = text.Length - 1;

			AssertParseFailure (text, false, tokenIndex, errorIndex, RfcComplianceMode.Strict);
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

			Assert.AreEqual ("Jeffrey Stedfast", mailbox.Name);
			Assert.AreEqual ("jeff", mailbox.Address);
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
			MailboxAddress mailbox;

			AssertParse (text);

			// default options should parse this as a single mailbox address
			mailbox = MailboxAddress.Parse (text);
			Assert.AreEqual ("Worthington, Warren", mailbox.Name);

			// this should fail when we allow mailbox addresses w/o a domain
			var options = ParserOptions.Default.Clone ();
			options.AllowUnquotedCommasInAddresses = false;
			options.AllowAddressesWithoutDomain = false;

			try {
				mailbox = MailboxAddress.Parse (options, text);
				Assert.Fail ("Should not have parsed \"{0}\" with AllowUnquotedCommasInAddresses = false", text);
			} catch (ParseException pex) {
				Assert.AreEqual (0, pex.TokenIndex, "TokenIndex");
				Assert.AreEqual (text.IndexOf (','), pex.ErrorIndex, "ErrorIndex");
			} catch (Exception ex) {
				Assert.Fail ("Should not have thrown {0}", ex.GetType ().Name);
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
			const int tokenIndex = 0;
			int errorIndex = text.IndexOf (':');

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseIncompleteGroup ()
		{
			const string text = "Agents of Shield: Skye <skye@shield.gov>, Leo Fitz <fitz@shield.gov>, Melinda May <may@shield.gov>";
			const int tokenIndex = 0;
			int errorIndex = text.IndexOf (':');

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseGroupNameColon ()
		{
			const string text = "Agents of Shield:";
			const int tokenIndex = 0;
			int errorIndex = text.IndexOf (':');

			AssertParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestGetAddress ()
		{
			var idn = new IdnMapping ();
			MailboxAddress mailbox;

			mailbox = new MailboxAddress ("Unit Test", "點看@domain.com");
			Assert.AreEqual ("點看@domain.com", mailbox.GetAddress (false), "IDN-decode #1");
			Assert.AreEqual (idn.GetAscii ("點看") + "@domain.com", mailbox.GetAddress (true), "IDN-encode #1");

			mailbox = new MailboxAddress ("Unit Test", idn.GetAscii ("點看") + "@domain.com");
			Assert.AreEqual ("點看@domain.com", mailbox.GetAddress (false), "IDN-decode #2");
			Assert.AreEqual (idn.GetAscii ("點看") + "@domain.com", mailbox.GetAddress (true), "IDN-encode #2");

			mailbox = new MailboxAddress ("Unit Test", "user@名がドメイン.com");
			Assert.AreEqual ("user@名がドメイン.com", mailbox.GetAddress (false), "IDN-decode #3");
			Assert.AreEqual ("user@" + idn.GetAscii ("名がドメイン.com"), mailbox.GetAddress (true), "IDN-encode #3");

			mailbox = new MailboxAddress ("Unit Test", "user@" + idn.GetAscii ("名がドメイン.com"));
			Assert.AreEqual ("user@名がドメイン.com", mailbox.GetAddress (false), "IDN-decode #4");
			Assert.AreEqual ("user@" + idn.GetAscii ("名がドメイン.com"), mailbox.GetAddress (true), "IDN-encode #4");

			mailbox = new MailboxAddress ("Unit Test", "點看@名がドメイン.com");
			Assert.AreEqual ("點看@名がドメイン.com", mailbox.GetAddress (false), "IDN-decode #5");
			Assert.AreEqual (idn.GetAscii ("點看") + "@" + idn.GetAscii ("名がドメイン.com"), mailbox.GetAddress (true), "IDN-encode #5");

			mailbox = new MailboxAddress ("Unit Test", idn.GetAscii ("點看") + "@" + idn.GetAscii ("名がドメイン.com"));
			Assert.AreEqual ("點看@名がドメイン.com", mailbox.GetAddress (false), "IDN-decode #6");
			Assert.AreEqual (idn.GetAscii ("點看") + "@" + idn.GetAscii ("名がドメイン.com"), mailbox.GetAddress (true), "IDN-encode #6");
		}

		[Test]
		public void TestIsInternational ()
		{
			var options = FormatOptions.Default.Clone ();
			options.International = true;
			var idn = new IdnMapping ();
			MailboxAddress mailbox;
			string encoded;

			// Test IsInternational local-parts
			mailbox = new MailboxAddress ("Unit Test", "點看@domain.com");
			Assert.IsTrue (mailbox.IsInternational, "IsInternational local-part");
			encoded = mailbox.ToString (options, true);
			Assert.AreEqual ("Unit Test <點看@domain.com>", encoded, "ToString local-part");

			// Test IsInternational IDN-encoded local-parts
			mailbox = new MailboxAddress ("Unit Test", idn.GetAscii ("點看") + "@domain.com");
			Assert.IsTrue (mailbox.IsInternational, "IsInternational IDN-encoded local-part");
			encoded = mailbox.ToString (options, true);
			Assert.AreEqual ("Unit Test <點看@domain.com>", encoded, "ToString IDN-encoded local-part");

			// Test IsInternational domain
			mailbox = new MailboxAddress ("Unit Test", "user@名がドメイン.com");
			Assert.IsTrue (mailbox.IsInternational, "IsInternational domain");
			encoded = mailbox.ToString (options, true);
			Assert.AreEqual ("Unit Test <user@名がドメイン.com>", encoded, "ToString domain");

			// Test IsInternational IDN-encoded domain
			mailbox = new MailboxAddress ("Unit Test", "user@" + idn.GetAscii ("名がドメイン.com"));
			Assert.IsTrue (mailbox.IsInternational, "IsInternational IDN-encoded domain");
			encoded = mailbox.ToString (options, true);
			Assert.AreEqual ("Unit Test <user@名がドメイン.com>", encoded, "ToString IDN-encoded domain");

			// Test IsInternational routes
			mailbox = new MailboxAddress ("Unit Test", "user@domain.com");
			Assert.IsFalse (mailbox.IsInternational, "IsInternational");
			mailbox.Route.Add ("route1");          // non-international route
			mailbox.Route.Add ("名がドメイン.com"); // international route
			Assert.IsTrue (mailbox.IsInternational, "IsInternational route");
			encoded = mailbox.ToString (options, true);
			Assert.AreEqual ("Unit Test <@route1,@名がドメイン.com:user@domain.com>", encoded, "ToString route");
		}

		[Test]
		public void TestIdnEncoding ()
		{
			const string userAscii = "xn--c1yn36f@domain.com";
			const string userUnicode = "點看@domain.com";
			const string domainAscii = "user@xn--v8jxj3d1dzdz08w.com";
			const string domainUnicode = "user@名がドメイン.com";
			MailboxAddress mailbox;
			string encoded;

			encoded = MailboxAddress.EncodeAddrspec (string.Empty);
			Assert.AreEqual (string.Empty, encoded, "Empty (Encode)");

			encoded = MailboxAddress.DecodeAddrspec (string.Empty);
			Assert.AreEqual (string.Empty, encoded, "Empty (Decode)");

			encoded = MailboxAddress.EncodeAddrspec (domainUnicode);
			Assert.AreEqual (domainAscii, encoded, "Domain (Encode)");

			encoded = MailboxAddress.DecodeAddrspec (domainAscii);
			Assert.AreEqual (domainUnicode, encoded, "Domain (Decode)");

			encoded = MailboxAddress.EncodeAddrspec (userUnicode);
			Assert.AreEqual (userAscii, encoded, "Local-part (Encode)");

			encoded = MailboxAddress.DecodeAddrspec (userAscii);
			Assert.AreEqual (userUnicode, encoded, "Local-part (Decode)");

			mailbox = new MailboxAddress (string.Empty, userAscii);
			Assert.AreEqual (userAscii, mailbox.GetAddress (true), "Ascii Local-part GetAddress(true)");
			Assert.AreEqual (userUnicode, mailbox.GetAddress (false), "Ascii Local-part GetAddress(false)");

			mailbox = new MailboxAddress (string.Empty, userUnicode);
			Assert.AreEqual (userAscii, mailbox.GetAddress (true), "Unicode Local-part GetAddress(true)");
			Assert.AreEqual (userUnicode, mailbox.GetAddress (false), "Unicode Local-part GetAddress(false)");

			mailbox = new MailboxAddress (string.Empty, domainAscii);
			Assert.AreEqual (domainAscii, mailbox.GetAddress (true), "Ascii Domain GetAddress(true)");
			Assert.AreEqual (domainUnicode, mailbox.GetAddress (false), "Ascii Domain GetAddress(false)");

			mailbox = new MailboxAddress (string.Empty, domainUnicode);
			Assert.AreEqual (domainAscii, mailbox.GetAddress (true), "Unicode Domain GetAddress(true)");
			Assert.AreEqual (domainUnicode, mailbox.GetAddress (false), "Unicode Domain GetAddress(false)");
		}

		[Test]
		public void TestRoutedMailbox ()
		{
			const string expected = "Rusty McRouterson\n\t<@comcast.net,@forward.com,@geek.net:rusty@final-destination.com>";
			const string expectedNoName = "<@comcast.net,@forward.com,@geek.net:rusty@final-destination.com>";
			var mailbox = new MailboxAddress ("Rusty McRouterson", "rusty@final-destination.com");

			mailbox.Route.Add ("comcast.net");
			mailbox.Route.Add ("forward.com");
			mailbox.Route.Add ("geek.net");

			Assert.AreEqual (expected, mailbox.ToString (true).Replace ("\r\n", "\n"), "Encoded mailbox does not match.");

			AssertParse (expected);

			mailbox.Name = null;

			var encoded = mailbox.ToString (true);

			Assert.AreEqual (expectedNoName, encoded, "Encoded mailbox does not match after setting Name to null.");

			encoded = mailbox.ToString (false);

			Assert.AreEqual (expectedNoName, encoded, "ToString mailbox does not match after setting Name to null.");
		}

		[Test]
		public void TestInternationalRoutedMailbox ()
		{
			const string expectedIdn = "User Name <@route,@xn--@-216a8b89fj88ctw7c.xn--lhr59c:user@domain.com>";
			const string expected = "User Name <@route,@伊昭傑@郵件.商務:user@domain.com>";
			var route = new[] { "route", "伊昭傑@郵件.商務" };
			var mailbox = new MailboxAddress ("User Name", route, "user@domain.com");
			var options = FormatOptions.Default.Clone ();

			Assert.AreEqual (expectedIdn, mailbox.ToString (options, true));

			options.International = true;

			Assert.AreEqual (expected, mailbox.ToString (options, true));
		}

		#region Rfc7103

		[Test]
		public void TestParseMailboxWithExcessiveAngleBrackets ()
		{
			const string text = "<<<user2@example.org>>>";
			var example1 = "User 2 <<<user2@example.org>";
			var example2 = "User 2 <user2@example.org>>>";

			AssertParse (text);

			AssertParseFailure (example1, false, 0, example1.IndexOf ('<') + 1, RfcComplianceMode.Strict);
			AssertParseFailure (example2, false, 0, example2.IndexOf ('>') + 1, RfcComplianceMode.Strict);
		}

		[Test]
		public void TestParseMailboxWithMissingGreaterThan ()
		{
			const string text = "<another@example.net";

			AssertParse (text);

			AssertParseFailure (text, false, 0, text.Length, RfcComplianceMode.Strict);
		}

		[Test]
		public void TestParseMailboxWithMissingLessThan ()
		{
			const string text = "second@example.org>";

			AssertParse (text);

			AssertParseFailure (text, false, 0, text.Length - 1, RfcComplianceMode.Strict);
		}

		[Test]
		public void TestParseMailboxWithUnbalancedQuotes ()
		{
			const string text = "\"Joe <joe@example.com>";

			AssertParse (text);

			AssertParseFailure (text, false, 0, text.Length, RfcComplianceMode.Strict);

			// for coverage
			AssertParseFailure (" \"", false, 1, 2, RfcComplianceMode.Loose);
		}

		[Test]
		public void TestParseMailboxWithAddrspecAsUnquotedName ()
		{
			const string text = "user@example.com <user@example.com>";
			int errorIndex = text.IndexOf ('<');

			AssertParse (text);

			AssertParseFailure (text, false, 0, errorIndex, RfcComplianceMode.Strict);
		}

		[Test]
		public void TestParseMailboxWithLatin1EncodedAddrspecLoose ()
		{
			const string text = "Name <æøå@example.com>";
			var buffer = CharsetUtils.Latin1.GetBytes (text);
			MailboxAddress mailbox;

			try {
				Assert.IsTrue (MailboxAddress.TryParse (buffer, out mailbox), "MailboxAddress.TryParse(byte[]) should succeed.");
			} catch (Exception ex) {
				Assert.Fail ("MailboxAddress.TryParse(byte[]) should not throw an exception: {0}", ex);
			}

			try {
				Assert.IsTrue (MailboxAddress.TryParse (buffer, 0, out mailbox), "MailboxAddress.TryParse(byte[], int) should succeed.");
			} catch (Exception ex) {
				Assert.Fail ("MailboxAddress.TryParse(byte[], int) should not throw an exception: {0}", ex);
			}

			try {
				Assert.IsTrue (MailboxAddress.TryParse (buffer, 0, buffer.Length, out mailbox), "MailboxAddress.TryParse(byte[], int, int) should succeed.");
			} catch (Exception ex) {
				Assert.Fail ("MailboxAddress.TryParse(byte[], int, int) should not throw an exception: {0}", ex);
			}

			try {
				mailbox = MailboxAddress.Parse (buffer);
			} catch (Exception ex) {
				Assert.Fail ("MailboxAddress.Parse(byte[]) should not throw an exception: {0}", ex);
			}

			try {
				mailbox = MailboxAddress.Parse (buffer, 0);
			} catch (Exception ex) {
				Assert.Fail ("MailboxAddress.Parse(byte[], int) should not throw an exception: {0}", ex);
			}

			try {
				mailbox = MailboxAddress.Parse (buffer, 0, buffer.Length);
			} catch (Exception ex) {
				Assert.Fail ("MailboxAddress.Parse(byte[], int, int) should not throw an exception: {0}", ex);
			}
		}

		[Test]
		public void TestParseMailboxWithLatin1EncodedAddrspecStrict ()
		{
			const string text = "Name <æøå@example.com>";
			var buffer = CharsetUtils.Latin1.GetBytes (text);
			var options = ParserOptions.Default.Clone ();
			const int tokenIndex = 6;
			const int errorIndex = 6;
			MailboxAddress mailbox;

			options.AddressParserComplianceMode = RfcComplianceMode.Strict;

			try {
				Assert.IsFalse (MailboxAddress.TryParse (options, buffer, out mailbox), "MailboxAddress.TryParse(ParserOptions, byte[]) should fail.");
			} catch (Exception ex) {
				Assert.Fail ("MailboxAddress.TryParse(ParserOptions, byte[]) should not throw an exception: {0}", ex);
			}

			try {
				Assert.IsFalse (MailboxAddress.TryParse (options, buffer, 0, out mailbox), "MailboxAddress.TryParse(byte[], int) should fail.");
			} catch (Exception ex) {
				Assert.Fail ("MailboxAddress.TryParse(ParserOptions, byte[], int) should not throw an exception: {0}", ex);
			}

			try {
				Assert.IsFalse (MailboxAddress.TryParse (options, buffer, 0, buffer.Length, out mailbox), "MailboxAddress.TryParse(ParserOptions, byte[], int, int) should fail.");
			} catch (Exception ex) {
				Assert.Fail ("MailboxAddress.TryParse(ParserOptions, byte[], int, int) should not throw an exception: {0}", ex);
			}

			try {
				mailbox = MailboxAddress.Parse (options, buffer);
				Assert.Fail ("MailboxAddress.Parse(ParserOptions, byte[]) should fail.");
			} catch (ParseException ex) {
				Assert.AreEqual (tokenIndex, ex.TokenIndex, "ParseException did not have the correct token index.");
				Assert.AreEqual (errorIndex, ex.ErrorIndex, "ParseException did not have the correct error index.");
			} catch {
				Assert.Fail ("MailboxAddress.Parse(ParserOptions, byte[]) should throw ParseException.");
			}

			try {
				mailbox = MailboxAddress.Parse (options, buffer, 0);
				Assert.Fail ("MailboxAddress.Parse(ParserOptions, byte[], int) should fail.");
			} catch (ParseException ex) {
				Assert.AreEqual (tokenIndex, ex.TokenIndex, "ParseException did not have the correct token index.");
				Assert.AreEqual (errorIndex, ex.ErrorIndex, "ParseException did not have the correct error index.");
			} catch {
				Assert.Fail ("MailboxAddress.Parse(ParserOptions, byte[], int) should throw ParseException.");
			}

			try {
				mailbox = MailboxAddress.Parse (options, buffer, 0, buffer.Length);
				Assert.Fail ("MailboxAddress.Parse(ParserOptions, byte[], int, int) should fail.");
			} catch (ParseException ex) {
				Assert.AreEqual (tokenIndex, ex.TokenIndex, "ParseException did not have the correct token index.");
				Assert.AreEqual (errorIndex, ex.ErrorIndex, "ParseException did not have the correct error index.");
			} catch {
				Assert.Fail ("MailboxAddress.Parse(ParserOptions, byte[], int, int) should throw ParseException.");
			}
		}

		#endregion

		[Test]
		public void TestParseMailboxWithSquareBracketsInDisplayName ()
		{
			const string text = "[Invalid Sender] <sender@tk2-201-10422.vs.sakura.ne.jp>";

			AssertParse (text);

			AssertParseFailure (text, false, 0, 0, RfcComplianceMode.Strict);
		}

		[Test]
		public void TestParseMailboxWithSquareBracketsAnd8BitTextInDisplayName ()
		{
			const string text = "Tom Doe [Cörp Öne] <tom.doe@corpone.com>";

			AssertParse (text);

			AssertParseFailure (text, false, 0, 8, RfcComplianceMode.Strict);
		}

		[Test]
		public void TestParseAddrspecWithUnicodeLocalPart ()
		{
			const string text = "test.täst@test.net";

			AssertParse (text);
		}

		[Test]
		public void TestParseAddrspecWithZeroWidthSpace ()
		{
			const string text = "\u200Btest@test.co.uk";

			AssertParse (text);
		}

		[Test]
		public void TestParseAddrspecEndingWithDot ()
		{
			const string text = "test.@gmail.com";

			AssertParse (text, RfcComplianceMode.Looser);

			AssertParseFailure (text, false, 0, 5, RfcComplianceMode.Loose);
			AssertParseFailure (text, false, 0, 5, RfcComplianceMode.Strict);
		}

		[Test]
		public void TestParseAddrspecEndingWithDotDot ()
		{
			const string text = "test..@gmail.com";

			AssertParse (text, RfcComplianceMode.Looser);

			AssertParseFailure (text, false, 0, 5, RfcComplianceMode.Loose);
			AssertParseFailure (text, false, 0, 5, RfcComplianceMode.Strict);
		}

		[Test]
		public void TestParseAddrspecWithDotDot ()
		{
			const string text = "test..test@gmail.com";

			AssertParse (text, RfcComplianceMode.Looser);

			AssertParseFailure (text, false, 0, 5, RfcComplianceMode.Loose);
			AssertParseFailure (text, false, 0, 5, RfcComplianceMode.Strict);
		}
	}
}

//
// AddressParserTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2014 Xamarin Inc. (www.xamarin.com)
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

using NUnit.Framework;

using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class InternetAddressListTests
	{
		static FormatOptions UnixFormatOptions;

		static void AssertInternetAddressListsEqual (string text, InternetAddressList expected, InternetAddressList result)
		{
			Assert.AreEqual (expected.Count, result.Count, "Unexpected number of addresses: {0}", text);

			for (int i = 0; i < expected.Count; i++) {
				Assert.AreEqual (expected.GetType (), result.GetType (),  "Address #{0} differs in type: {1}", i, text);

				Assert.AreEqual (expected[i].ToString (), result[i].ToString (), "Display strings differ for {0}", text);
			}

			var encoded = result.ToString (true).Replace ("\r\n", "\n");

			Assert.AreEqual (text, encoded, "Encoded strings differ for {0}", text);
		}

		[SetUp]
		public void Setup ()
		{
			UnixFormatOptions = FormatOptions.Default.Clone ();
			UnixFormatOptions.NewLineFormat = NewLineFormat.Unix;
		}

		[Test]
		public void TestTryParseWhiteSpace ()
		{
			try {
				InternetAddress address;

				Assert.IsFalse (InternetAddress.TryParse ("   ", out address), "InternetAddress.TryParse() should fail to parse whitespace.");
			} catch (Exception ex) {
				Assert.Fail ("InternetAddress.TryParse() should not throw an exception: {0}", ex);
			}

			try {
				InternetAddressList list;

				Assert.IsFalse (InternetAddressList.TryParse ("   ", out list), "InternetAddressList.TryParse() should fail to parse whitespace.");
			} catch (Exception ex) {
				Assert.Fail ("InternetAddressList.TryParse() should not throw an exception: {0}", ex);
			}
		}

		[Test]
		public void TestSimpleAddrSpec ()
		{
			var expected = new InternetAddressList ();
			var mailbox = new MailboxAddress ("", "");
			InternetAddressList result;
			string text;

			expected.Add (mailbox);

			text = "fejj@helixcode.com";
			mailbox.Address = "fejj@helixcode.com";
			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (text, expected, result);

			result = InternetAddressList.Parse (text);
			AssertInternetAddressListsEqual (text, expected, result);

			text = "fejj";
			mailbox.Address = "fejj";
			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (text, expected, result);

			result = InternetAddressList.Parse (text);
			AssertInternetAddressListsEqual (text, expected, result);
		}

		[Test]
		public void TestSimpleAddrSpecWithTrailingDot ()
		{
			const string encoded = "fejj@helixcode.com";
			const string text = "fejj@helixcode.com.";
			var expected = new InternetAddressList ();
			var mailbox = new MailboxAddress ("", "");
			InternetAddressList result;

			expected.Add (mailbox);

			mailbox.Address = "fejj@helixcode.com";
			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (encoded, expected, result);

			result = InternetAddressList.Parse (text);
			AssertInternetAddressListsEqual (encoded, expected, result);
		}

		[Test]
		public void TestExampleAddrSpecWithQuotedLocalPartAndCommentsFromRfc822 ()
		{
			const string text = "\":sysmail\"@  Some-Group. Some-Org,\n Muhammed.(I am  the greatest) Ali @(the)Vegas.WBA";
			const string encoded = "\":sysmail\"@Some-Group.Some-Org, Muhammed.Ali@Vegas.WBA";
			var expected = new InternetAddressList ();
			InternetAddressList result;

			expected.Add (new MailboxAddress ("", "\":sysmail\"@Some-Group.Some-Org"));
			expected.Add (new MailboxAddress ("", "Muhammed.Ali@Vegas.WBA"));

			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (encoded, expected, result);

			result = InternetAddressList.Parse (text);
			AssertInternetAddressListsEqual (encoded, expected, result);
		}

		[Test]
		public void TestExampleMailboxWithCommentsFromRfc5322 ()
		{
			const string text = "Pete(A nice \\) chap) <pete(his account)@silly.test(his host)>";
			const string encoded = "Pete <pete@silly.test>";
			var expected = new InternetAddressList ();
			InternetAddressList result;

			expected.Add (new MailboxAddress ("Pete", "pete@silly.test"));

			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (encoded, expected, result);

			result = InternetAddressList.Parse (text);
			AssertInternetAddressListsEqual (encoded, expected, result);
		}

		[Test]
		public void TestSimpleMailboxes ()
		{
			var expected = new InternetAddressList ();
			var mailbox = new MailboxAddress ("", "");
			InternetAddressList result;
			string text;

			expected.Add (mailbox);

			mailbox.Name = "Jeffrey Stedfast";
			mailbox.Address = "fejj@helixcode.com";
			text = "Jeffrey Stedfast <fejj@helixcode.com>";
			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (text, expected, result);

			result = InternetAddressList.Parse (text);
			AssertInternetAddressListsEqual (text, expected, result);

			mailbox.Name = "this is a folded name";
			mailbox.Address = "folded@name.com";
			text = "this is\n\ta folded name <folded@name.com>";
			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual ("this is a folded name <folded@name.com>", expected, result);

			result = InternetAddressList.Parse (text);
			AssertInternetAddressListsEqual ("this is a folded name <folded@name.com>", expected, result);

			mailbox.Name = "Jeffrey fejj Stedfast";
			mailbox.Address = "fejj@helixcode.com";
			text = "Jeffrey \"fejj\" Stedfast <fejj@helixcode.com>";
			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual ("Jeffrey fejj Stedfast <fejj@helixcode.com>", expected, result);

			result = InternetAddressList.Parse (text);
			AssertInternetAddressListsEqual ("Jeffrey fejj Stedfast <fejj@helixcode.com>", expected, result);

			mailbox.Name = "Jeffrey \"fejj\" Stedfast";
			mailbox.Address = "fejj@helixcode.com";
			text = "\"Jeffrey \\\"fejj\\\" Stedfast\" <fejj@helixcode.com>";
			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (text, expected, result);

			result = InternetAddressList.Parse (text);
			AssertInternetAddressListsEqual (text, expected, result);

			mailbox.Name = "Stedfast, Jeffrey";
			mailbox.Address = "fejj@helixcode.com";
			text = "\"Stedfast, Jeffrey\" <fejj@helixcode.com>";
			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (text, expected, result);

			result = InternetAddressList.Parse (text);
			AssertInternetAddressListsEqual (text, expected, result);

			mailbox.Name = "Jeffrey Stedfast";
			mailbox.Address = "fejj@helixcode.com";
			text = "fejj@helixcode.com (Jeffrey Stedfast)";
			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual ("Jeffrey Stedfast <fejj@helixcode.com>", expected, result);

			result = InternetAddressList.Parse (text);
			AssertInternetAddressListsEqual ("Jeffrey Stedfast <fejj@helixcode.com>", expected, result);

			mailbox.Name = "Jeffrey Stedfast";
			mailbox.Address = "fejj@helixcode.com";
			text = "Jeffrey Stedfast <fejj(recursive (comment) block)@helixcode.(and a comment here)com>";
			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual ("Jeffrey Stedfast <fejj@helixcode.com>", expected, result);

			result = InternetAddressList.Parse (text);
			AssertInternetAddressListsEqual ("Jeffrey Stedfast <fejj@helixcode.com>", expected, result);

			mailbox.Name = "Jeffrey Stedfast";
			mailbox.Address = "fejj@helixcode.com";
			text = "Jeffrey Stedfast <fejj@helixcode.com.>";
			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual ("Jeffrey Stedfast <fejj@helixcode.com>", expected, result);

			result = InternetAddressList.Parse (text);
			AssertInternetAddressListsEqual ("Jeffrey Stedfast <fejj@helixcode.com>", expected, result);
		}

		[Test]
		public void TestMailboxesWithRfc2047EncodedNames ()
		{
			var expected = new InternetAddressList ();
			var mailbox = new MailboxAddress ("", "");
			InternetAddressList result;
			string text;

			expected.Add (mailbox);

			mailbox.Name = "Kristoffer Brånemyr";
			mailbox.Address = "ztion@swipenet.se";
			text = "=?iso-8859-1?q?Kristoffer_Br=E5nemyr?= <ztion@swipenet.se>";
			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual ("Kristoffer =?iso-8859-1?q?Br=E5nemyr?= <ztion@swipenet.se>", expected, result);

			result = InternetAddressList.Parse (text);
			AssertInternetAddressListsEqual ("Kristoffer =?iso-8859-1?q?Br=E5nemyr?= <ztion@swipenet.se>", expected, result);

			mailbox.Name = "François Pons";
			mailbox.Address = "fpons@mandrakesoft.com";
			text = "=?iso-8859-1?q?Fran=E7ois?= Pons <fpons@mandrakesoft.com>";
			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (text, expected, result);

			result = InternetAddressList.Parse (text);
			AssertInternetAddressListsEqual (text, expected, result);
		}

		[Test]
		public void TestListWithGroupAndAddrspec ()
		{
			const string text = "GNOME Hackers: Miguel de Icaza <miguel@gnome.org>, Havoc Pennington <hp@redhat.com>;, fejj@helixcode.com";
			string encoded = "GNOME Hackers: Miguel de Icaza <miguel@gnome.org>, Havoc Pennington\n\t<hp@redhat.com>;, fejj@helixcode.com";
			var expected = new InternetAddressList ();
			InternetAddressList result;

			expected.Add (new GroupAddress ("GNOME Hackers", new InternetAddress[] {
				new MailboxAddress ("Miguel de Icaza", "miguel@gnome.org"),
				new MailboxAddress ("Havoc Pennington", "hp@redhat.com")
			}));
			expected.Add (new MailboxAddress ("", "fejj@helixcode.com"));

			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (encoded, expected, result);

			result = InternetAddressList.Parse (text);
			AssertInternetAddressListsEqual (encoded, expected, result);
		}

		[Test]
		public void TestLocalGroupWithoutSemicolon ()
		{
			const string text = "Local recipients: phil, joe, alex, bob";
			const string encoded = "Local recipients: phil, joe, alex, bob;";
			var expected = new InternetAddressList ();
			InternetAddressList result;

			expected.Add (new GroupAddress ("Local recipients", new InternetAddress[] {
				new MailboxAddress ("", "phil"),
				new MailboxAddress ("", "joe"),
				new MailboxAddress ("", "alex"),
				new MailboxAddress ("", "bob"),
			}));

			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (encoded, expected, result);

			//Assert.Throws<ParseException> (() => InternetAddressList.Parse (text), "Parsing should have failed.");
		}

		[Test]
		public void TestExampleGroupWithCommentsFromRfc5322 ()
		{
			const string text = "A Group(Some people):Chris Jones <c@(Chris's host.)public.example>, joe@example.org, John <jdoe@one.test> (my dear friend); (the end of the group)";
			const string encoded = "A Group: Chris Jones <c@public.example>, joe@example.org, John <jdoe@one.test>;";
			var expected = new InternetAddressList ();
			InternetAddressList result;

			expected.Add (new GroupAddress ("A Group", new InternetAddress[] {
				new MailboxAddress ("Chris Jones", "c@public.example"),
				new MailboxAddress ("", "joe@example.org"),
				new MailboxAddress ("John", "jdoe@one.test")
			}));

			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (encoded, expected, result);

			result = InternetAddressList.Parse (text);
			AssertInternetAddressListsEqual (encoded, expected, result);
		}

		[Test]
		public void TestMailboxWithDotsInTheName ()
		{
			const string encoded = "\"Nathaniel S. Borenstein\" <nsb@thumper.bellcore.com>";
			const string text = "Nathaniel S. Borenstein <nsb@thumper.bellcore.com>";
			var expected = new InternetAddressList ();
			InternetAddressList result;

			expected.Add (new MailboxAddress ("Nathaniel S. Borenstein", "nsb@thumper.bellcore.com"));

			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (encoded, expected, result);

			result = InternetAddressList.Parse (text);
			AssertInternetAddressListsEqual (encoded, expected, result);
		}

		[Test]
		public void TestMailboxWith8bitName ()
		{
			const string encoded = "Patrik =?iso-8859-1?b?RqVkbHRzdHKldm0=?= <paf@nada.kth.se>";
			const string text = "Patrik F¥dltstr¥vm <paf@nada.kth.se>";
			var expected = new InternetAddressList ();
			InternetAddressList result;

			expected.Add (new MailboxAddress ("Patrik F¥dltstr¥vm", "paf@nada.kth.se"));

			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (encoded, expected, result);

			result = InternetAddressList.Parse (text);
			AssertInternetAddressListsEqual (encoded, expected, result);
		}

		[Test]
		public void TestObsoleteMailboxRoutingSyntax ()
		{
			var expected = new InternetAddressList ();
			InternetAddressList result;
			string text;

			text = "Routed Address <@route:user@domain.com>";

			expected.Add (new MailboxAddress ("Routed Address", new [] { "route" }, "user@domain.com"));

			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (text, expected, result);

			result = InternetAddressList.Parse (text);
			AssertInternetAddressListsEqual (text, expected, result);
		}

		[Test]
		public void TestObsoleteMailboxRoutingSyntaxWithEmptyDomains ()
		{
			const string text = "Routed Address <@route1,,@route2,,,@route3:user@domain.com>";
			const string encoded = "Routed Address <@route1,@route2,@route3:user@domain.com>";
			var expected = new InternetAddressList ();
			InternetAddressList result;

			expected.Add (new MailboxAddress ("Routed Address", new [] { "route1", "route2", "route3" }, "user@domain.com"));

			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (encoded, expected, result);

			result = InternetAddressList.Parse (text);
			AssertInternetAddressListsEqual (encoded, expected, result);
		}

		[Test]
		public void TestEncodingSimpleMailboxWithQuotedName ()
		{
			var mailbox = new MailboxAddress ("Stedfast, Jeffrey", "fejj@gnome.org");
			var list = new InternetAddressList ();
			list.Add (mailbox);

			var expected = "\"Stedfast, Jeffrey\" <fejj@gnome.org>";
			var actual = list.ToString (UnixFormatOptions, true);

			Assert.AreEqual (expected, actual, "Encoding quoted mailbox did not match expected result: {0}", expected);
		}

		[Test]
		public void TestEncodingSimpleMailboxWithLatin1Name ()
		{
			var latin1 = Encoding.GetEncoding ("iso-8859-1");
			var mailbox = new MailboxAddress (latin1, "Kristoffer Brånemyr", "ztion@swipenet.se");
			var list = new InternetAddressList ();
			list.Add (mailbox);

			var expected = "Kristoffer =?iso-8859-1?q?Br=E5nemyr?= <ztion@swipenet.se>";
			var actual = list.ToString (UnixFormatOptions, true);

			Assert.AreEqual (expected, actual, "Encoding latin1 mailbox did not match expected result: {0}", expected);

			mailbox = new MailboxAddress (latin1, "Tõivo Leedjärv", "leedjarv@interest.ee");
			list = new InternetAddressList ();
			list.Add (mailbox);

			expected = "=?iso-8859-1?b?VIH1aXZvIExlZWRqgeRydg==?= <leedjarv@interest.ee>";
			actual = list.ToString (UnixFormatOptions, true);

			Assert.AreEqual (expected, actual, "Encoding latin1 mailbox did not match expected result: {0}", expected);
		}

		[Test]
		public void TestEncodingMailboxWithReallyLongWord ()
		{
			var name = "reeeeeeeeeeeeeeaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaallllllllllllllllllllllllllllllllllllllllllllllllllllllly long word";
			var mailbox = new MailboxAddress (name, "really.long.word@example.com");
			var list = new InternetAddressList ();
			list.Add (mailbox);

			var expected = "=?us-ascii?q?reeeeeeeeeeeeeeaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaallllllllllll?=\n =?us-ascii?q?llllllllllllllllllllllllllllllllllllllllllly?= long word\n\t<really.long.word@example.com>";
			var actual = list.ToString (UnixFormatOptions, true);

			Assert.AreEqual (expected, actual, "Encoding really long mailbox did not match expected result: {0}", expected);
			Assert.IsTrue (InternetAddressList.TryParse (actual, out list), "Failed to parse really long mailbox");
			Assert.AreEqual (mailbox.Name, list[0].Name);
		}

		[Test]
		public void TestEncodingMailboxWithArabicName ()
		{
			var mailbox = new MailboxAddress ("هل تتكلم اللغة الإنجليزية /العربية؟", "do.you.speak@arabic.com");
			var list = new InternetAddressList ();
			list.Add (mailbox);

			var expected = "=?utf-8?b?2YfZhCDYqtiq2YPZhNmFINin2YTZhNi62Kkg2KfZhNil2YbYrNmE2YrYstmK2Kk=?=\n =?utf-8?b?IC/Yp9mE2LnYsdio2YrYqdif?= <do.you.speak@arabic.com>";
			var actual = list.ToString (UnixFormatOptions, true);

			Assert.AreEqual (expected, actual, "Encoding arabic mailbox did not match expected result: {0}", expected);
			Assert.IsTrue (InternetAddressList.TryParse (actual, out list), "Failed to parse arabic mailbox");
			Assert.AreEqual (mailbox.Name, list[0].Name);
		}

		[Test]
		public void TestEncodingMailboxWithJapaneseName ()
		{
			var mailbox = new MailboxAddress ("狂ったこの世で狂うなら気は確かだ。", "famous@quotes.ja");
			var list = new InternetAddressList ();
			list.Add (mailbox);

			var expected = "=?utf-8?b?54uC44Gj44Gf44GT44Gu5LiW44Gn54uC44GG44Gq44KJ5rCX44Gv56K644GL44Gg?=\n =?utf-8?b?44CC?= <famous@quotes.ja>";
			var actual = list.ToString (UnixFormatOptions, true);

			Assert.AreEqual (expected, actual, "Encoding japanese mailbox did not match expected result: {0}", expected);
			Assert.IsTrue (InternetAddressList.TryParse (actual, out list), "Failed to parse japanese mailbox");
			Assert.AreEqual (mailbox.Name, list[0].Name);
		}

		[Test]
		public void TestEncodingSimpleAddressList ()
		{
			const string expectedEncoded = "Kristoffer =?iso-8859-1?q?Br=E5nemyr?= <ztion@swipenet.se>, Jeffrey Stedfast\n\t<fejj@gnome.org>";
			const string expectedDisplay = "\"Kristoffer Brånemyr\" <ztion@swipenet.se>, \"Jeffrey Stedfast\" <fejj@gnome.org>";
			var latin1 = Encoding.GetEncoding ("iso-8859-1");
			var options = FormatOptions.Default.Clone ();
			var list = new InternetAddressList ();

			list.Add (new MailboxAddress (latin1, "Kristoffer Brånemyr", "ztion@swipenet.se"));
			list.Add (new MailboxAddress ("Jeffrey Stedfast", "fejj@gnome.org"));

			options.NewLineFormat = NewLineFormat.Unix;

			var display = list.ToString (options, false);
			Assert.AreEqual (expectedDisplay, display, "Display value does not match the expected result: {0}", display);

			var encoded = list.ToString (options, true);
			Assert.AreEqual (expectedEncoded, encoded, "Encoded value does not match the expected result: {0}", display);
		}

		[Test]
		public void TestDecodedMailboxHasCorrectCharsetEncoding ()
		{
			var latin1 = Encoding.GetEncoding ("iso-8859-1");
			var mailbox = new MailboxAddress (latin1, "Kristoffer Brånemyr", "ztion@swipenet.se");
			var list = new InternetAddressList ();
			list.Add (mailbox);

			var encoded = list.ToString (UnixFormatOptions, true);

			InternetAddressList parsed;
			Assert.IsTrue (InternetAddressList.TryParse (encoded, out parsed), "Failed to parse address");
			Assert.AreEqual (latin1.HeaderName, parsed[0].Encoding.HeaderName, "Parsed charset does not match");
		}

		[Test]
		public void TestUnsupportedCharsetExceptionNotThrown ()
		{
			var mailbox = new MailboxAddress (Encoding.UTF8, "狂ったこの世で狂うなら気は確かだ。", "famous@quotes.ja");
			var list = new InternetAddressList ();
			list.Add (mailbox);

			var encoded = list.ToString (true);

			encoded = encoded.Replace ("utf-8", "x-unknown");

			InternetAddressList parsed;

			try {
				Assert.IsTrue (InternetAddressList.TryParse (encoded, out parsed), "Failed to parse address");
			} catch (Exception ex) {
				Assert.Fail ("Exception thrown parsing address with unsupported charset: {0}", ex);
			}
		}

		[Test]
		public void TestInternationalEmailAddresses ()
		{
			const string text = "伊昭傑@郵件.商務, राम@मोहन.ईन्फो, юзер@екзампл.ком, θσερ@εχαμπλε.ψομ";
			InternetAddressList list;

			Assert.IsTrue (InternetAddressList.TryParse (text, out list), "Failed to parse international email addresses.");
			Assert.AreEqual (4, list.Count, "Unexpected number of international email addresses.");

			var addresses = text.Split (',');
			for (int i = 0; i < addresses.Length; i++) {
				var mailbox = (MailboxAddress) list[i];

				addresses[i] = addresses[i].Trim ();

				Assert.AreEqual (addresses[i], mailbox.Address, "International address #{0} did not match.", i);
			}
		}

		[Test]
		public void TestEquality ()
		{
			var list1 = new InternetAddressList ();

			list1.Add (new GroupAddress ("Local recipients", new InternetAddress[] {
				new MailboxAddress ("", "phil"),
				new MailboxAddress ("", "joe"),
				new MailboxAddress ("", "alex"),
				new MailboxAddress ("", "bob"),
			}));
			list1.Add (new MailboxAddress ("Joey", "joey@friends.com"));
			list1.Add (new MailboxAddress ("Chandler", "chandler@friends.com"));

			var list2 = new InternetAddressList ();

			list2.Add (new MailboxAddress ("Chandler", "chandler@friends.com"));
			list2.Add (new GroupAddress ("Local recipients", new InternetAddress[] {
				new MailboxAddress ("", "phil"),
				new MailboxAddress ("", "joe"),
				new MailboxAddress ("", "alex"),
				new MailboxAddress ("", "bob"),
			}));
			list2.Add (new MailboxAddress ("Joey", "joey@friends.com"));

			Assert.IsTrue (list1.Equals (list2), "The 2 lists should be equal.");
		}
	}
}

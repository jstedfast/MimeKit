//
// InternetAddressListTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2016 Xamarin Inc. (www.xamarin.com)
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

		public InternetAddressListTests ()
		{
			UnixFormatOptions = FormatOptions.Default.Clone ();
			UnixFormatOptions.NewLineFormat = NewLineFormat.Unix;
		}

		[Test]
		public void TestArgumentExceptions ()
		{
			var mailbox = new MailboxAddress ("MimeKit Unit Tests", "mimekit@example.com");
			var list = new InternetAddressList ();

			list.Add (new MailboxAddress ("Example User", "user@example.com"));

			Assert.Throws<ArgumentNullException> (() => new InternetAddressList (null));
			Assert.Throws<ArgumentNullException> (() => list.Add (null));
			Assert.Throws<ArgumentNullException> (() => list.AddRange (null));
			Assert.Throws<ArgumentNullException> (() => list.CompareTo (null));
			Assert.Throws<ArgumentNullException> (() => list.Contains (null));
			Assert.Throws<ArgumentNullException> (() => list.CopyTo (null, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => list.CopyTo (new InternetAddress[0], -1));
			Assert.Throws<ArgumentNullException> (() => list.IndexOf (null));
			Assert.Throws<ArgumentOutOfRangeException> (() => list.Insert (-1, mailbox));
			Assert.Throws<ArgumentNullException> (() => list.Insert (0, null));
			Assert.Throws<ArgumentNullException> (() => list.Remove (null));
			Assert.Throws<ArgumentOutOfRangeException> (() => list.RemoveAt (-1));
			Assert.Throws<ArgumentOutOfRangeException> (() => list[-1] = mailbox);
			Assert.Throws<ArgumentNullException> (() => list[0] = null);
		}

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

		static void AssertTryParse (string text, string encoded, InternetAddressList expected)
		{
			var buffer = Encoding.UTF8.GetBytes (text);
			InternetAddressList result;

			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "TryParse(string): {0}", text);
			AssertInternetAddressListsEqual (encoded, expected, result);

			Assert.IsTrue (InternetAddressList.TryParse (buffer, out result), "TryParse(byte[]): {0}", text);
			AssertInternetAddressListsEqual (encoded, expected, result);

			Assert.IsTrue (InternetAddressList.TryParse (buffer, 0, out result), "TryParse(byte[], int): {0}", text);
			AssertInternetAddressListsEqual (encoded, expected, result);

			Assert.IsTrue (InternetAddressList.TryParse (buffer, 0, buffer.Length, out result), "TryParse(byte[] int, int): {0}", text);
			AssertInternetAddressListsEqual (encoded, expected, result);
		}

		static void AssertParse (string text, string encoded, InternetAddressList expected)
		{
			var buffer = Encoding.UTF8.GetBytes (text);
			InternetAddressList result = null;

			try {
				result = InternetAddressList.Parse (text);
			} catch {
				Assert.Fail ("Parse(string): {0}", text);
			}
			AssertInternetAddressListsEqual (encoded, expected, result);

			try {
				result = InternetAddressList.Parse (buffer);
			} catch {
				Assert.Fail ("Parse(byte[]): {0}", text);
			}
			AssertInternetAddressListsEqual (encoded, expected, result);

			try {
				result = InternetAddressList.Parse (buffer, 0);
			} catch {
				Assert.Fail ("Parse(byte[], int): {0}", text);
			}
			AssertInternetAddressListsEqual (encoded, expected, result);

			try {
				result = InternetAddressList.Parse (buffer, 0, buffer.Length);
			} catch {
				Assert.Fail ("Parse(byte[], int, int): {0}", text);
			}
			AssertInternetAddressListsEqual (encoded, expected, result);
		}

		static void AssertParseAndTryParse (string text, string encoded, InternetAddressList expected)
		{
			AssertTryParse (text, encoded, expected);
			AssertParse (text, encoded, expected);
		}

		static void AssertTryParseFails (string text)
		{
			var buffer = Encoding.UTF8.GetBytes (text);
			InternetAddressList result;

			try {
				Assert.IsFalse (InternetAddressList.TryParse (text, out result), "InternetAddressList.TryParse() should fail to parse \"{0}\".", text);
			} catch (Exception ex) {
				Assert.Fail ("InternetAddressList.TryParse() should not throw an exception: {0}", ex);
			}

			try {
				Assert.IsFalse (InternetAddressList.TryParse (buffer, out result), "InternetAddressList.TryParse() should fail to parse \"{0}\".", text);
			} catch (Exception ex) {
				Assert.Fail ("InternetAddressList.TryParse() should not throw an exception: {0}", ex);
			}

			try {
				Assert.IsFalse (InternetAddressList.TryParse (buffer, 0, out result), "InternetAddressList.TryParse() should fail to parse \"{0}\".", text);
			} catch (Exception ex) {
				Assert.Fail ("InternetAddressList.TryParse() should not throw an exception: {0}", ex);
			}

			try {
				Assert.IsFalse (InternetAddressList.TryParse (buffer, 0, buffer.Length, out result), "InternetAddressList.TryParse() should fail to parse \"{0}\".", text);
			} catch (Exception ex) {
				Assert.Fail ("InternetAddressList.TryParse() should not throw an exception: {0}", ex);
			}
		}

		static void AssertParseFails (string text)
		{
			var buffer = Encoding.UTF8.GetBytes (text);

			try {
				InternetAddressList.Parse (text);
				Assert.Fail ("InternetAddressList.Parse() should fail to parse \"{0}\".", text);
			} catch (ParseException) {
				// success
			} catch {
				Assert.Fail ("InternetAddressList.Parse() should throw ParseException.");
			}

			try {
				InternetAddressList.Parse (buffer);
				Assert.Fail ("InternetAddressList.Parse() should fail to parse \"{0}\".", text);
			} catch (ParseException) {
				// success
			} catch {
				Assert.Fail ("InternetAddressList.Parse() should throw ParseException.");
			}

			try {
				InternetAddressList.Parse (buffer, 0);
				Assert.Fail ("InternetAddressList.Parse() should fail to parse \"{0}\".", text);
			} catch (ParseException) {
				// success
			} catch {
				Assert.Fail ("InternetAddressList.Parse() should throw ParseException.");
			}

			try {
				InternetAddressList.Parse (buffer, 0, buffer.Length);
				Assert.Fail ("InternetAddressList.Parse() should fail to parse \"{0}\".", text);
			} catch (ParseException) {
				// success
			} catch {
				Assert.Fail ("InternetAddressList.Parse() should throw ParseException.");
			}
		}

		static void AssertParseAndTryParseFail (string text)
		{
			AssertTryParseFails (text);
			AssertParseFails (text);
		}

		[Test]
		public void TestParseWhiteSpace ()
		{
			AssertParseAndTryParseFail ("   ");
		}

		[Test]
		public void TestSimpleAddrSpec ()
		{
			var expected = new InternetAddressList ();
			var mailbox = new MailboxAddress ("", "");
			string text;

			expected.Add (mailbox);

			text = "fejj@helixcode.com";
			mailbox.Address = "fejj@helixcode.com";
			AssertParseAndTryParse (text, text, expected);

			text = "fejj";
			mailbox.Address = "fejj";
			AssertParseAndTryParse (text, text, expected);
		}

		[Test]
		public void TestSimpleAddrSpecWithTrailingDot ()
		{
			const string encoded = "fejj@helixcode.com";
			const string text = "fejj@helixcode.com.";
			var expected = new InternetAddressList ();
			var mailbox = new MailboxAddress ("", "");

			expected.Add (mailbox);

			mailbox.Address = "fejj@helixcode.com";
			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestExampleAddrSpecWithQuotedLocalPartAndCommentsFromRfc822 ()
		{
			const string text = "\":sysmail\"@  Some-Group. Some-Org,\n Muhammed.(I am  the greatest) Ali @(the)Vegas.WBA";
			const string encoded = "\":sysmail\"@Some-Group.Some-Org, Muhammed.Ali@Vegas.WBA";
			var expected = new InternetAddressList ();

			expected.Add (new MailboxAddress ("", "\":sysmail\"@Some-Group.Some-Org"));
			expected.Add (new MailboxAddress ("", "Muhammed.Ali@Vegas.WBA"));

			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestExampleMailboxWithCommentsFromRfc5322 ()
		{
			const string text = "Pete(A nice \\) chap) <pete(his account)@silly.test(his host)>";
			const string encoded = "Pete <pete@silly.test>";
			var expected = new InternetAddressList ();

			expected.Add (new MailboxAddress ("Pete", "pete@silly.test"));

			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestSimpleMailboxes ()
		{
			var expected = new InternetAddressList ();
			var mailbox = new MailboxAddress ("", "");
			string text;

			expected.Add (mailbox);

			mailbox.Name = "Jeffrey Stedfast";
			mailbox.Address = "fejj@helixcode.com";
			text = "Jeffrey Stedfast <fejj@helixcode.com>";
			AssertParseAndTryParse (text, text, expected);

			mailbox.Name = "this is a folded name";
			mailbox.Address = "folded@name.com";
			text = "this is\n\ta folded name <folded@name.com>";
			AssertParseAndTryParse (text, "this is a folded name <folded@name.com>", expected);

			mailbox.Name = "Jeffrey fejj Stedfast";
			mailbox.Address = "fejj@helixcode.com";
			text = "Jeffrey \"fejj\" Stedfast <fejj@helixcode.com>";
			AssertParseAndTryParse (text, "Jeffrey fejj Stedfast <fejj@helixcode.com>", expected);

			mailbox.Name = "Jeffrey \"fejj\" Stedfast";
			mailbox.Address = "fejj@helixcode.com";
			text = "\"Jeffrey \\\"fejj\\\" Stedfast\" <fejj@helixcode.com>";
			AssertParseAndTryParse (text, text, expected);

			mailbox.Name = "Stedfast, Jeffrey";
			mailbox.Address = "fejj@helixcode.com";
			text = "\"Stedfast, Jeffrey\" <fejj@helixcode.com>";
			AssertParseAndTryParse (text, text, expected);

			mailbox.Name = "Jeffrey Stedfast";
			mailbox.Address = "fejj@helixcode.com";
			text = "fejj@helixcode.com (Jeffrey Stedfast)";
			AssertParseAndTryParse (text, "Jeffrey Stedfast <fejj@helixcode.com>", expected);

			mailbox.Name = "Jeffrey Stedfast";
			mailbox.Address = "fejj@helixcode.com";
			text = "Jeffrey Stedfast <fejj(recursive (comment) block)@helixcode.(and a comment here)com>";
			AssertParseAndTryParse (text, "Jeffrey Stedfast <fejj@helixcode.com>", expected);

			mailbox.Name = "Jeffrey Stedfast";
			mailbox.Address = "fejj@helixcode.com";
			text = "Jeffrey Stedfast <fejj@helixcode.com.>";
			AssertParseAndTryParse (text, "Jeffrey Stedfast <fejj@helixcode.com>", expected);
		}

		[Test]
		public void TestMailboxesWithRfc2047EncodedNames ()
		{
			var expected = new InternetAddressList ();
			var mailbox = new MailboxAddress ("", "");
			string text;

			expected.Add (mailbox);

			mailbox.Name = "Kristoffer Brånemyr";
			mailbox.Address = "ztion@swipenet.se";
			text = "=?iso-8859-1?q?Kristoffer_Br=E5nemyr?= <ztion@swipenet.se>";
			AssertParseAndTryParse (text, "Kristoffer =?iso-8859-1?q?Br=E5nemyr?= <ztion@swipenet.se>", expected);

			mailbox.Name = "François Pons";
			mailbox.Address = "fpons@mandrakesoft.com";
			text = "=?iso-8859-1?q?Fran=E7ois?= Pons <fpons@mandrakesoft.com>";
			AssertParseAndTryParse (text, text, expected);
		}

		[Test]
		public void TestListWithGroupAndAddrspec ()
		{
			const string text = "GNOME Hackers: Miguel de Icaza <miguel@gnome.org>, Havoc Pennington <hp@redhat.com>;, fejj@helixcode.com";
			const string encoded = "GNOME Hackers: Miguel de Icaza <miguel@gnome.org>, Havoc Pennington\n\t<hp@redhat.com>;, fejj@helixcode.com";
			var expected = new InternetAddressList ();

			expected.Add (new GroupAddress ("GNOME Hackers", new InternetAddress[] {
				new MailboxAddress ("Miguel de Icaza", "miguel@gnome.org"),
				new MailboxAddress ("Havoc Pennington", "hp@redhat.com")
			}));
			expected.Add (new MailboxAddress ("", "fejj@helixcode.com"));

			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestLocalGroupWithoutSemicolon ()
		{
			const string text = "Local recipients: phil, joe, alex, bob";
			const string encoded = "Local recipients: phil, joe, alex, bob;";
			var expected = new InternetAddressList ();

			expected.Add (new GroupAddress ("Local recipients", new InternetAddress[] {
				new MailboxAddress ("", "phil"),
				new MailboxAddress ("", "joe"),
				new MailboxAddress ("", "alex"),
				new MailboxAddress ("", "bob"),
			}));

			AssertTryParse (text, encoded, expected);

			//Assert.Throws<ParseException> (() => InternetAddressList.Parse (text), "Parsing should have failed.");
		}

		[Test]
		public void TestExampleGroupWithCommentsFromRfc5322 ()
		{
			const string text = "A Group(Some people):Chris Jones <c@(Chris's host.)public.example>, joe@example.org, John <jdoe@one.test> (my dear friend); (the end of the group)";
			const string encoded = "A Group: Chris Jones <c@public.example>, joe@example.org, John <jdoe@one.test>;";
			var expected = new InternetAddressList ();

			expected.Add (new GroupAddress ("A Group", new InternetAddress[] {
				new MailboxAddress ("Chris Jones", "c@public.example"),
				new MailboxAddress ("", "joe@example.org"),
				new MailboxAddress ("John", "jdoe@one.test")
			}));

			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestMailboxWithDotsInTheName ()
		{
			const string encoded = "\"Nathaniel S. Borenstein\" <nsb@thumper.bellcore.com>";
			const string text = "Nathaniel S. Borenstein <nsb@thumper.bellcore.com>";
			var expected = new InternetAddressList ();

			expected.Add (new MailboxAddress ("Nathaniel S. Borenstein", "nsb@thumper.bellcore.com"));

			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestMailboxWith8bitName ()
		{
			const string encoded = "Patrik =?iso-8859-1?b?RqVkbHRzdHKldm0=?= <paf@nada.kth.se>";
			const string text = "Patrik F¥dltstr¥vm <paf@nada.kth.se>";
			var expected = new InternetAddressList ();

			expected.Add (new MailboxAddress ("Patrik F¥dltstr¥vm", "paf@nada.kth.se"));

			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestObsoleteMailboxRoutingSyntax ()
		{
			const string text = "Routed Address <@route:user@domain.com>";
			var expected = new InternetAddressList ();

			expected.Add (new MailboxAddress ("Routed Address", new [] { "route" }, "user@domain.com"));

			AssertParseAndTryParse (text, text, expected);
		}

		[Test]
		public void TestObsoleteMailboxRoutingSyntaxWithEmptyDomains ()
		{
			const string text = "Routed Address <@route1,,@route2,,,@route3:user@domain.com>";
			const string encoded = "Routed Address <@route1,@route2,@route3:user@domain.com>";
			var expected = new InternetAddressList ();

			expected.Add (new MailboxAddress ("Routed Address", new [] { "route1", "route2", "route3" }, "user@domain.com"));

			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestEncodingSimpleMailboxWithQuotedName ()
		{
			const string expected = "\"Stedfast, Jeffrey\" <fejj@gnome.org>";
			var mailbox = new MailboxAddress ("Stedfast, Jeffrey", "fejj@gnome.org");
			var list = new InternetAddressList ();
			list.Add (mailbox);

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
			const string expected = "=?us-ascii?q?reeeeeeeeeeeeeeaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaallllllllllll?=\n =?us-ascii?q?llllllllllllllllllllllllllllllllllllllllllly?= long word\n\t<really.long.word@example.com>";
			const string name = "reeeeeeeeeeeeeeaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaallllllllllllllllllllllllllllllllllllllllllllllllllllllly long word";
			var mailbox = new MailboxAddress (name, "really.long.word@example.com");
			var list = new InternetAddressList ();
			list.Add (mailbox);

			var actual = list.ToString (UnixFormatOptions, true);

			Assert.AreEqual (expected, actual, "Encoding really long mailbox did not match expected result: {0}", expected);
			Assert.IsTrue (InternetAddressList.TryParse (actual, out list), "Failed to parse really long mailbox");
			Assert.AreEqual (mailbox.Name, list[0].Name);
		}

		[Test]
		public void TestEncodingMailboxWithArabicName ()
		{
			const string expected = "=?utf-8?b?2YfZhCDYqtiq2YPZhNmFINin2YTZhNi62Kkg2KfZhNil2YbYrNmE2YrYstmK2Kk=?=\n =?utf-8?b?IC/Yp9mE2LnYsdio2YrYqdif?= <do.you.speak@arabic.com>";
			var mailbox = new MailboxAddress ("هل تتكلم اللغة الإنجليزية /العربية؟", "do.you.speak@arabic.com");
			var list = new InternetAddressList ();
			list.Add (mailbox);

			var actual = list.ToString (UnixFormatOptions, true);

			Assert.AreEqual (expected, actual, "Encoding arabic mailbox did not match expected result: {0}", expected);
			Assert.IsTrue (InternetAddressList.TryParse (actual, out list), "Failed to parse arabic mailbox");
			Assert.AreEqual (mailbox.Name, list[0].Name);
		}

		[Test]
		public void TestEncodingMailboxWithJapaneseName ()
		{
			const string expected = "=?utf-8?b?54uC44Gj44Gf44GT44Gu5LiW44Gn54uC44GG44Gq44KJ5rCX44Gv56K644GL44Gg?=\n =?utf-8?b?44CC?= <famous@quotes.ja>";
			var mailbox = new MailboxAddress ("狂ったこの世で狂うなら気は確かだ。", "famous@quotes.ja");
			var list = new InternetAddressList ();
			list.Add (mailbox);

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

		[Test]
		public void TestCompareTo ()
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

			Assert.IsTrue (list1.CompareTo (list2) > 0, "CompareTo() should return < 0.");
			Assert.IsTrue (list2.CompareTo (list1) < 0, "CompareTo() should return > 0.");
		}

		#region Rfc7103

		// TODO: test both Strict and Loose RfcCompliance modes

		[Test]
		public void TestParseMailboxWithExcessiveAngleBrackets ()
		{
			const string text = "<<<user2@example.org>>>";
			const string encoded = "user2@example.org";
			var expected = new InternetAddressList ();

			expected.Add (new MailboxAddress ("", encoded));

			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestParseMailboxWithMissingGreaterThan ()
		{
			const string text = "<another@example.net";
			const string encoded = "another@example.net";
			var expected = new InternetAddressList ();

			expected.Add (new MailboxAddress ("", encoded));

			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestParseMailboxWithMissingLessThan ()
		{
			const string text = "second@example.org>";
			const string encoded = "second@example.org";
			var expected = new InternetAddressList ();

			expected.Add (new MailboxAddress ("", encoded));

			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestParseErrantComma ()
		{
			const string text = "<third@example.net, fourth@example.net>";
			const string encoded = "third@example.net, fourth@example.net";
			var expected = new InternetAddressList ();

			expected.Add (new MailboxAddress ("", "third@example.net"));
			expected.Add (new MailboxAddress ("", "fourth@example.net"));

			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestParseMailboxWithUnbalancedQuotes ()
		{
			const string text = "\"Joe <joe@example.com>";
			const string encoded = "Joe <joe@example.com>";
			var expected = new InternetAddressList ();

			expected.Add (new MailboxAddress ("Joe", "joe@example.com"));

			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestParseMailboxWithUnbalancedQuotes2 ()
		{
			const string text = "\"Joe <joe@example.com>, Bob <bob@example.com>";
			const string encoded = "Joe <joe@example.com>, Bob <bob@example.com>";
			var expected = new InternetAddressList ();

			expected.Add (new MailboxAddress ("Joe", "joe@example.com"));
			expected.Add (new MailboxAddress ("Bob", "bob@example.com"));

			AssertParseAndTryParse (text, encoded, expected);
		}

		#endregion
	}
}

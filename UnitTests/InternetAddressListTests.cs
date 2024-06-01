//
// InternetAddressListTests.cs
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
using MimeKit.Utils;

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
			var list = new InternetAddressList {
				new MailboxAddress ("Example User", "user@example.com")
			};

			Assert.Throws<ArgumentNullException> (() => new InternetAddressList (null));
			Assert.Throws<ArgumentNullException> (() => list.Add (null));
			Assert.Throws<ArgumentNullException> (() => list.AddRange (null));
			Assert.Throws<ArgumentNullException> (() => list.CompareTo (null));
			Assert.Throws<ArgumentNullException> (() => list.Contains (null));
			Assert.Throws<ArgumentNullException> (() => list.CopyTo (null, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => list.CopyTo (Array.Empty<InternetAddress> (), -1));
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
			Assert.That (result.Count, Is.EqualTo (expected.Count), $"Unexpected number of addresses: {text}");

			for (int i = 0; i < expected.Count; i++) {
				Assert.That (result.GetType (), Is.EqualTo (expected.GetType ()), $"Address #{i} differs in type: {text}");

				Assert.That (result[i].ToString (), Is.EqualTo (expected[i].ToString ()), $"Display strings differ for {text}");
			}

			var encoded = result.ToString (true).Replace ("\r\n", "\n");

			Assert.That (encoded, Is.EqualTo (text), $"Encoded strings differ for {text}");
		}

		static void AssertTryParse (string text, string encoded, InternetAddressList expected)
		{
			var buffer = Encoding.UTF8.GetBytes (text);
			InternetAddressList result;

			Assert.That (InternetAddressList.TryParse (text, out result), Is.True, $"TryParse(string): {text}");
			AssertInternetAddressListsEqual (encoded, expected, result);

			Assert.That (InternetAddressList.TryParse (buffer, out result), Is.True, $"TryParse(byte[]): {text}");
			AssertInternetAddressListsEqual (encoded, expected, result);

			Assert.That (InternetAddressList.TryParse (buffer, 0, out result), Is.True, $"TryParse(byte[], int): {text}");
			AssertInternetAddressListsEqual (encoded, expected, result);

			Assert.That (InternetAddressList.TryParse (buffer, 0, buffer.Length, out result), Is.True, $"TryParse(byte[] int, int): {text}");
			AssertInternetAddressListsEqual (encoded, expected, result);
		}

		static void AssertParse (string text, string encoded, InternetAddressList expected)
		{
			var buffer = Encoding.UTF8.GetBytes (text);
			InternetAddressList result = null;

			try {
				result = InternetAddressList.Parse (text);
			} catch {
				Assert.Fail ($"Parse(string): {text}");
			}
			AssertInternetAddressListsEqual (encoded, expected, result);

			try {
				result = InternetAddressList.Parse (buffer);
			} catch {
				Assert.Fail ($"Parse(byte[]): {text}");
			}
			AssertInternetAddressListsEqual (encoded, expected, result);

			try {
				result = InternetAddressList.Parse (buffer, 0);
			} catch {
				Assert.Fail ($"Parse(byte[], int): {text}");
			}
			AssertInternetAddressListsEqual (encoded, expected, result);

			try {
				result = InternetAddressList.Parse (buffer, 0, buffer.Length);
			} catch {
				Assert.Fail ($"Parse(byte[], int, int): {text}");
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
			bool success;

			try {
				success = InternetAddressList.TryParse (text, out result);
				Assert.That (success, Is.False, $"InternetAddressList.TryParse() should fail to parse \"{text}\".");
			} catch (Exception ex) {
				Assert.Fail ($"InternetAddressList.TryParse() should not throw an exception: {ex}");
			}

			try {
				success = InternetAddressList.TryParse (buffer, out result);
				Assert.That (success, Is.False, $"InternetAddressList.TryParse() should fail to parse \"{text}\".");
			} catch (Exception ex) {
				Assert.Fail ($"InternetAddressList.TryParse() should not throw an exception: {ex}");
			}

			try {
				success = InternetAddressList.TryParse (buffer, 0, out result);
				Assert.That (success, Is.False, $"InternetAddressList.TryParse() should fail to parse \"{text}\".");
			} catch (Exception ex) {
				Assert.Fail ($"InternetAddressList.TryParse() should not throw an exception: {ex}");
			}

			try {
				success = InternetAddressList.TryParse (buffer, 0, buffer.Length, out result);
				Assert.That (success, Is.False, $"InternetAddressList.TryParse() should fail to parse \"{text}\".");
			} catch (Exception ex) {
				Assert.Fail ($"InternetAddressList.TryParse() should not throw an exception: {ex}");
			}
		}

		static void AssertParseFails (string text)
		{
			var buffer = Encoding.UTF8.GetBytes (text);

			try {
				InternetAddressList.Parse (text);
				Assert.Fail ($"InternetAddressList.Parse() should fail to parse \"{text}\".");
			} catch (ParseException) {
				// success
			} catch {
				Assert.Fail ("InternetAddressList.Parse() should throw ParseException.");
			}

			try {
				InternetAddressList.Parse (buffer);
				Assert.Fail ($"InternetAddressList.Parse() should fail to parse \"{text}\".");
			} catch (ParseException) {
				// success
			} catch {
				Assert.Fail ("InternetAddressList.Parse() should throw ParseException.");
			}

			try {
				InternetAddressList.Parse (buffer, 0);
				Assert.Fail ($"InternetAddressList.Parse() should fail to parse \"{text}\".");
			} catch (ParseException) {
				// success
			} catch {
				Assert.Fail ("InternetAddressList.Parse() should throw ParseException.");
			}

			try {
				InternetAddressList.Parse (buffer, 0, buffer.Length);
				Assert.Fail ($"InternetAddressList.Parse() should fail to parse \"{text}\".");
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
		public void TestParseNameLessThan ()
		{
			AssertTryParseFails ("\"Name\" <");
			AssertParseFails ("\"Name\" <");
		}

		[Test]
		public void TestSimpleAddrSpec ()
		{
			var expected = new InternetAddressList ();
			var mailbox = new MailboxAddress ("", "example@mimekit.net");
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
			var mailbox = new MailboxAddress ("", "example@mimekit.net");

			expected.Add (mailbox);

			mailbox.Address = "fejj@helixcode.com";
			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestExampleAddrSpecWithQuotedLocalPartAndCommentsFromRfc822 ()
		{
			const string text = "\":sysmail\"@  Some-Group. Some-Org,\n Muhammed.(I am  the greatest) Ali @(the)Vegas.WBA";
			const string encoded = "\":sysmail\"@Some-Group.Some-Org, Muhammed.Ali@Vegas.WBA";
			var expected = new InternetAddressList {
				new MailboxAddress ("", "\":sysmail\"@Some-Group.Some-Org"),
				new MailboxAddress ("", "Muhammed.Ali@Vegas.WBA")
			};

			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestExampleMailboxWithCommentsFromRfc5322 ()
		{
			const string text = "Pete(A nice \\) chap) <pete(his account)@silly.test(his host)>";
			const string encoded = "Pete <pete@silly.test>";
			var expected = new InternetAddressList {
				new MailboxAddress ("Pete", "pete@silly.test")
			};

			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestSimpleMailboxes ()
		{
			var expected = new InternetAddressList ();
			var mailbox = new MailboxAddress ("", "example@mimekit.net");
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
			var mailbox = new MailboxAddress ("", "example@mimekit.net");
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
			var expected = new InternetAddressList {
				new GroupAddress ("GNOME Hackers", new InternetAddress[] {
					new MailboxAddress ("Miguel de Icaza", "miguel@gnome.org"),
					new MailboxAddress ("Havoc Pennington", "hp@redhat.com")
				}),
				new MailboxAddress ("", "fejj@helixcode.com")
			};

			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestLocalGroupWithoutSemicolon ()
		{
			const string text = "Local recipients: phil, joe, alex, bob";
			const string encoded = "Local recipients: phil, joe, alex, bob;";
			var expected = new InternetAddressList {
				new GroupAddress ("Local recipients", new InternetAddress[] {
					new MailboxAddress ("", "phil"),
					new MailboxAddress ("", "joe"),
					new MailboxAddress ("", "alex"),
					new MailboxAddress ("", "bob"),
				})
			};

			AssertTryParse (text, encoded, expected);

			//Assert.Throws<ParseException> (() => InternetAddressList.Parse (text), "Parsing should have failed.");
		}

		[Test]
		public void TestExampleGroupWithCommentsFromRfc5322 ()
		{
			const string text = "A Group(Some people):Chris Jones <c@(Chris's host.)public.example>, joe@example.org, John <jdoe@one.test> (my dear friend); (the end of the group)";
			const string encoded = "A Group: Chris Jones <c@public.example>, joe@example.org, John <jdoe@one.test>;";
			var expected = new InternetAddressList {
				new GroupAddress ("A Group", new InternetAddress[] {
					new MailboxAddress ("Chris Jones", "c@public.example"),
					new MailboxAddress ("", "joe@example.org"),
					new MailboxAddress ("John", "jdoe@one.test")
				})
			};

			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestMailboxWithDotsInTheName ()
		{
			const string encoded = "\"Nathaniel S. Borenstein\" <nsb@thumper.bellcore.com>";
			const string text = "Nathaniel S. Borenstein <nsb@thumper.bellcore.com>";
			var expected = new InternetAddressList {
				new MailboxAddress ("Nathaniel S. Borenstein", "nsb@thumper.bellcore.com")
			};

			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestMailboxWith8bitName ()
		{
			//const string encoded = "Patrik =?iso-8859-1?b?RqVkbHRzdHKldm0=?= <paf@nada.kth.se>";
			const string encoded = "Patrik =?utf-8?b?RsKlZGx0c3RywqV2bQ==?= <paf@nada.kth.se>";
			const string text = "Patrik F¥dltstr¥vm <paf@nada.kth.se>";
			var expected = new InternetAddressList {
				new MailboxAddress ("Patrik F¥dltstr¥vm", "paf@nada.kth.se")
			};

			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestObsoleteMailboxRoutingSyntax ()
		{
			const string text = "Routed Address <@route:user@domain.com>";
			var expected = new InternetAddressList {
				new MailboxAddress ("Routed Address", new[] { "route" }, "user@domain.com")
			};

			AssertParseAndTryParse (text, text, expected);
		}

		[Test]
		public void TestObsoleteMailboxRoutingSyntaxWithEmptyDomains ()
		{
			const string text = "Routed Address <@route1,,@route2,,,@route3:user@domain.com>";
			const string encoded = "Routed Address <@route1,@route2,@route3:user@domain.com>";
			var expected = new InternetAddressList {
				new MailboxAddress ("Routed Address", new[] { "route1", "route2", "route3" }, "user@domain.com")
			};

			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestEncodingSimpleMailboxWithQuotedName ()
		{
			const string expected = "\"Stedfast, Jeffrey\" <fejj@gnome.org>";
			var mailbox = new MailboxAddress ("Stedfast, Jeffrey", "fejj@gnome.org");
			var list = new InternetAddressList {
				mailbox
			};

			var actual = list.ToString (UnixFormatOptions, true);

			Assert.That (actual, Is.EqualTo (expected), $"Encoding quoted mailbox did not match expected result: {expected}");
		}

		[Test]
		public void TestEncodingSimpleMailboxWithLatin1Name ()
		{
			var latin1 = Encoding.GetEncoding ("iso-8859-1");
			var mailbox = new MailboxAddress (latin1, "Kristoffer Brånemyr", "ztion@swipenet.se");
			var list = new InternetAddressList {
				mailbox
			};

			var expected = "Kristoffer =?iso-8859-1?q?Br=E5nemyr?= <ztion@swipenet.se>";
			var actual = list.ToString (UnixFormatOptions, true);

			Assert.That (actual, Is.EqualTo (expected), $"Encoding latin1 mailbox did not match expected result: {expected}");

			mailbox = new MailboxAddress (latin1, "Tõivo Leedjärv", "leedjarv@interest.ee");
			list = new InternetAddressList {
				mailbox
			};

			expected = "=?iso-8859-1?b?VIH1aXZvIExlZWRqgeRydg==?= <leedjarv@interest.ee>";
			actual = list.ToString (UnixFormatOptions, true);

			Assert.That (actual, Is.EqualTo (expected), $"Encoding latin1 mailbox did not match expected result: {expected}");
		}

		[Test]
		public void TestEncodingMailboxWithReallyLongWord ()
		{
			const string expected = "=?us-ascii?q?reeeeeeeeeeeeeeaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaallllllllllll?=\n =?us-ascii?q?llllllllllllllllllllllllllllllllllllllllllly?= long word\n\t<really.long.word@example.com>";
			const string name = "reeeeeeeeeeeeeeaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaallllllllllllllllllllllllllllllllllllllllllllllllllllllly long word";
			var mailbox = new MailboxAddress (name, "really.long.word@example.com");
			var options = FormatOptions.Default.Clone ();
			var list = new InternetAddressList {
				mailbox
			};

			options.NewLineFormat = NewLineFormat.Unix;
			options.AllowMixedHeaderCharsets = true;

			var actual = list.ToString (options, true);

			Assert.That (actual, Is.EqualTo (expected), $"Encoding really long mailbox did not match expected result: {expected}");
			Assert.That (InternetAddressList.TryParse (actual, out list), Is.True, "Failed to parse really long mailbox");
			Assert.That (list[0].Name, Is.EqualTo (mailbox.Name));
		}

		[Test]
		public void TestEncodingMailboxWithArabicName ()
		{
			const string expected = "=?utf-8?b?2YfZhCDYqtiq2YPZhNmFINin2YTZhNi62Kkg2KfZhNil2YbYrNmE2YrYstmK2Kk=?=\n =?utf-8?b?IC/Yp9mE2LnYsdio2YrYqdif?= <do.you.speak@arabic.com>";
			var mailbox = new MailboxAddress ("هل تتكلم اللغة الإنجليزية /العربية؟", "do.you.speak@arabic.com");
			var list = new InternetAddressList {
				mailbox
			};

			var actual = list.ToString (UnixFormatOptions, true);

			Assert.That (actual, Is.EqualTo (expected), $"Encoding arabic mailbox did not match expected result: {expected}");
			Assert.That (InternetAddressList.TryParse (actual, out list), Is.True, "Failed to parse arabic mailbox");
			Assert.That (list[0].Name, Is.EqualTo (mailbox.Name));
		}

		[Test]
		public void TestEncodingMailboxWithJapaneseName ()
		{
			const string expected = "=?utf-8?b?54uC44Gj44Gf44GT44Gu5LiW44Gn54uC44GG44Gq44KJ5rCX44Gv56K644GL44Gg?=\n =?utf-8?b?44CC?= <famous@quotes.ja>";
			var mailbox = new MailboxAddress ("狂ったこの世で狂うなら気は確かだ。", "famous@quotes.ja");
			var list = new InternetAddressList {
				mailbox
			};

			var actual = list.ToString (UnixFormatOptions, true);

			Assert.That (actual, Is.EqualTo (expected), $"Encoding japanese mailbox did not match expected result: {expected}");
			Assert.That (InternetAddressList.TryParse (actual, out list), Is.True, "Failed to parse japanese mailbox");
			Assert.That (list[0].Name, Is.EqualTo (mailbox.Name));
		}

		[Test]
		public void TestEncodingSimpleAddressList ()
		{
			const string expectedEncoded = "Kristoffer =?iso-8859-1?q?Br=E5nemyr?= <ztion@swipenet.se>, Jeffrey Stedfast\n\t<fejj@gnome.org>";
			const string expectedDisplay = "\"Kristoffer Brånemyr\" <ztion@swipenet.se>, \"Jeffrey Stedfast\" <fejj@gnome.org>";
			var latin1 = Encoding.GetEncoding ("iso-8859-1");
			var options = FormatOptions.Default.Clone ();
			var list = new InternetAddressList {
				new MailboxAddress (latin1, "Kristoffer Brånemyr", "ztion@swipenet.se"),
				new MailboxAddress ("Jeffrey Stedfast", "fejj@gnome.org")
			};

			options.NewLineFormat = NewLineFormat.Unix;

			var display = list.ToString (options, false);
			Assert.That (display, Is.EqualTo (expectedDisplay), $"Display value does not match the expected result: {display}");

			var encoded = list.ToString (options, true);
			Assert.That (encoded, Is.EqualTo (expectedEncoded), $"Encoded value does not match the expected result: {display}");
		}

		[Test]
		public void TestEncodingLongNameMixedQuotingAndEncoding ()
		{
			const string name = "Dr. xxxxxxxxxx xxxxx | xxxxxx.xxxxxxx für xxxxxxxxxxxxx xxxx";
			const string encodedNameLatin1 = "\"Dr. xxxxxxxxxx xxxxx | xxxxxx.xxxxxxx\" =?iso-8859-1?b?Zvxy?= xxxxxxxxxxxxx xxxx";
			const string encodedNameUnicode = "\"Dr. xxxxxxxxxx xxxxx | xxxxxx.xxxxxxx\" =?utf-8?b?ZsO8cg==?= xxxxxxxxxxxxx xxxx";
			const string encodedMailbox = "\"Dr. xxxxxxxxxx xxxxx | xxxxxx.xxxxxxx\" =?iso-8859-1?b?Zvxy?= xxxxxxxxxxxxx\n xxxx <x.xxxxx@xxxxxxx-xxxxxx.xx>";
			const string address = "x.xxxxx@xxxxxxx-xxxxxx.xx";
			var options = FormatOptions.Default.Clone ();

			options.NewLineFormat = NewLineFormat.Unix;
			options.AllowMixedHeaderCharsets = true;

			var buffer = Rfc2047.EncodePhrase (options, Encoding.UTF8, name);
			var result = Encoding.UTF8.GetString (buffer);

			Assert.That (result, Is.EqualTo (encodedNameLatin1));

			var mailbox = new MailboxAddress (name, address);
			var list = new InternetAddressList {
				mailbox
			};

			result = list.ToString (options, true);

			Assert.That (result, Is.EqualTo (encodedMailbox));

			// Now disable smart encoding

			options.AllowMixedHeaderCharsets = false;

			buffer = Rfc2047.EncodePhrase (options, Encoding.UTF8, name);
			result = Encoding.UTF8.GetString (buffer);

			Assert.That (result, Is.EqualTo (encodedNameUnicode));
		}

		[Test]
		public void TestDecodedMailboxHasCorrectCharsetEncoding ()
		{
			var latin1 = Encoding.GetEncoding ("iso-8859-1");
			var mailbox = new MailboxAddress (latin1, "Kristoffer Brånemyr", "ztion@swipenet.se");
			var list = new InternetAddressList {
				mailbox
			};

			var encoded = list.ToString (UnixFormatOptions, true);

			InternetAddressList parsed;
			Assert.That (InternetAddressList.TryParse (encoded, out parsed), Is.True, "Failed to parse address");
			Assert.That (parsed[0].Encoding.HeaderName, Is.EqualTo (latin1.HeaderName), "Parsed charset does not match");
		}

		[Test]
		public void TestUnsupportedCharsetExceptionNotThrown ()
		{
			var mailbox = new MailboxAddress (Encoding.UTF8, "狂ったこの世で狂うなら気は確かだ。", "famous@quotes.ja");
			var list = new InternetAddressList {
				mailbox
			};

			var encoded = list.ToString (true);

			encoded = encoded.Replace ("utf-8", "x-unknown");

			InternetAddressList parsed;

			try {
				Assert.That (InternetAddressList.TryParse (encoded, out parsed), Is.True, "Failed to parse address");
			} catch (Exception ex) {
				Assert.Fail ($"Exception thrown parsing address with unsupported charset: {ex}");
			}
		}

		[Test]
		public void TestInternationalEmailAddresses ()
		{
			const string text = "伊昭傑@郵件.商務, राम@मोहन.ईन्फो, юзер@екзампл.ком, θσερ@εχαμπλε.ψομ";
			InternetAddressList list;

			Assert.That (InternetAddressList.TryParse (text, out list), Is.True, "Failed to parse international email addresses.");
			Assert.That (list.Count, Is.EqualTo (4), "Unexpected number of international email addresses.");

			var addresses = text.Split (',');
			for (int i = 0; i < addresses.Length; i++) {
				var mailbox = (MailboxAddress) list[i];

				addresses[i] = addresses[i].Trim ();

				Assert.That (mailbox.Address, Is.EqualTo (addresses[i]), $"International address #{i} did not match.");
			}
		}

		[Test]
		public void TestBasicFunctionality ()
		{
			var user0 = new MailboxAddress ("Name Zero", "user0@address.com");
			var user1 = new MailboxAddress ("Name One", "user1@address.com");
			var user2 = new MailboxAddress ("Name Two", "user2@address.com");
			var list = new InternetAddressList ();

			Assert.That (list.IsReadOnly, Is.False, "IsReadOnly");

			list.Add (user1);
			list.Add (user2);

			Assert.That (list.Count, Is.EqualTo (2), "Count");
			Assert.That (list.Contains (user1), Is.True, "Contains");
			Assert.That (list.Contains (user2), Is.True, "Contains");
			Assert.That (list.Contains (new MailboxAddress ("Unknown", "unknown@address.com")), Is.False, "Contains");
			Assert.That (list.IndexOf (user1), Is.EqualTo (0), "IndexOf");
			Assert.That (list.IndexOf (user2), Is.EqualTo (1), "IndexOf");

			list.Insert (0, user0);
			Assert.That (list.Count, Is.EqualTo (3), "Count");
			Assert.That (list.Contains (user0), Is.True, "Contains");
			Assert.That (list.IndexOf (user0), Is.EqualTo (0), "IndexOf");
			Assert.That (list[0].Name, Is.EqualTo (user0.Name), "Name");

			list.RemoveAt (0);
			Assert.That (list.Count, Is.EqualTo (2), "Count");
			Assert.That (list.Contains (user0), Is.False, "Contains");
			Assert.That (list.IndexOf (user0), Is.EqualTo (-1), "IndexOf");

			Assert.That (list.Remove (user0), Is.False, "Remove");

			Assert.That (list.Remove (user2), Is.True, "Remove");
			Assert.That (list.Count, Is.EqualTo (1), "Count");
			Assert.That (list.Contains (user2), Is.False, "Contains");
			Assert.That (list.IndexOf (user0), Is.EqualTo (-1), "IndexOf");

			list[0] = user0;
			Assert.That (list.Count, Is.EqualTo (1), "Count");
			Assert.That (list.Contains (user0), Is.True, "Contains");
			Assert.That (list.Contains (user1), Is.False, "Contains");
			Assert.That (list.IndexOf (user0), Is.EqualTo (0), "IndexOf");
			Assert.That (list.IndexOf (user1), Is.EqualTo (-1), "IndexOf");
		}

		[Test]
		public void TestEnumeratingMailboxes ()
		{
			var innerGroup = new GroupAddress ("Inner");
			innerGroup.Members.Add (new MailboxAddress ("Inner1", "inner1@address.com"));
			innerGroup.Members.Add (new MailboxAddress ("Inner2", "inner2@address.com"));

			var outerGroup = new GroupAddress ("Outer");
			outerGroup.Members.Add (new MailboxAddress ("Outer1", "outer1@address.com"));
			outerGroup.Members.Add (innerGroup);
			outerGroup.Members.Add (new MailboxAddress ("Outer2", "outer2@address.com"));

			var list = new InternetAddressList {
				new MailboxAddress ("Before", "before@address.com"),
				outerGroup,
				new MailboxAddress ("After", "after@address.com")
			};

			var expected = new List<InternetAddress> {
				list[0],
				outerGroup.Members[0],
				innerGroup.Members[0],
				innerGroup.Members[1],
				outerGroup.Members[2],
				list[2]
			};
			int i = 0;

			foreach (var mailbox in list.Mailboxes) {
				Assert.That (mailbox, Is.EqualTo (expected[i]), $"Mailbox #{i}");
				i++;
			}
		}

		[Test]
		public void TestEquality ()
		{
			var list1 = new InternetAddressList {
				new GroupAddress ("Local recipients", new InternetAddress[] {
					new MailboxAddress ("", "phil"),
					new MailboxAddress ("", "joe"),
					new MailboxAddress ("", "alex"),
					new MailboxAddress ("", "bob"),
				}),
				new MailboxAddress ("Joey", "joey@friends.com"),
				new MailboxAddress ("Chandler", "chandler@friends.com")
			};

			var list2 = new InternetAddressList {
				new GroupAddress ("Local recipients", new InternetAddress[] {
					new MailboxAddress ("", "phil"),
					new MailboxAddress ("", "joe"),
					new MailboxAddress ("", "alex"),
					new MailboxAddress ("", "bob"),
				}),
				new MailboxAddress ("Joey", "joey@friends.com"),
				new MailboxAddress ("Chandler", "chandler@friends.com")
			};

			Assert.That (list1.Equals (null), Is.False, "Equals null");
			Assert.That (list1.Equals (new InternetAddressList ()), Is.False, "Equals empty list");
			Assert.That (list1.Equals (list2), Is.True, "The 2 lists should be equal.");

			Assert.That (((object) list1).Equals ((object) list2), Is.True, "Equals(object)");
			Assert.That (list2.GetHashCode (), Is.EqualTo (list1.GetHashCode ()), "GetHashCode()");
		}

		[Test]
		public void TestCompareTo ()
		{
			var list1 = new InternetAddressList {
				new GroupAddress ("Local recipients", new InternetAddress[] {
					new MailboxAddress ("", "phil"),
					new MailboxAddress ("", "joe"),
					new MailboxAddress ("", "alex"),
					new MailboxAddress ("", "bob"),
				}),
				new MailboxAddress ("Joey", "joey@friends.com"),
				new MailboxAddress ("Chandler", "chandler@friends.com")
			};

			var list2 = new InternetAddressList {
				new MailboxAddress ("Chandler", "chandler@friends.com"),
				new GroupAddress ("Local recipients", new InternetAddress[] {
					new MailboxAddress ("", "phil"),
					new MailboxAddress ("", "joe"),
					new MailboxAddress ("", "alex"),
					new MailboxAddress ("", "bob"),
				}),
				new MailboxAddress ("Joey", "joey@friends.com")
			};

			Assert.That (list1.CompareTo (list2) > 0, Is.True, "CompareTo() should return < 0.");
			Assert.That (list2.CompareTo (list1) < 0, Is.True, "CompareTo() should return > 0.");

			var mailbox = new MailboxAddress ("Joe", "joe@inter.net");
			var group = new GroupAddress ("Joe", new InternetAddress[] {
				new MailboxAddress ("Joe", "joe@inter.net")
			});

			// MailboxAddresses with the same name should always sort first
			Assert.That (mailbox.CompareTo (group) < 0, Is.True, "CompareTo() should return < 0.");
			Assert.That (group.CompareTo (mailbox) > 0, Is.True, "CompareTo() should return > 0.");

			Assert.That (mailbox.CompareTo (group.Members[0]), Is.EqualTo (0), "CompareTo() should return 0.");

			var alice = new MailboxAddress (string.Empty, "alice@example.com");
			var bob = new MailboxAddress (string.Empty, "bob@example.com");

			Assert.That (alice.CompareTo (bob) < 0, Is.True, "alice.CompareTo(bob) should return < 0.");
			Assert.That (bob.CompareTo (alice) > 0, Is.True, "bob.CompareTo(alice) should return > 0.");

			var alexa = new MailboxAddress (string.Empty, "alexa@example.com");
			var alex = new MailboxAddress (string.Empty, "alex@example.com");

			Assert.That (alex.CompareTo (alexa) < 0, Is.True, "alex.CompareTo(alexa) should return < 0.");
			Assert.That (alexa.CompareTo (alex) > 0, Is.True, "alexa.CompareTo(alex) should return > 0.");
		}

		// issue #1043
		[TestCase (RfcComplianceMode.Strict, false)]
		[TestCase (RfcComplianceMode.Loose, false)]
		[TestCase (RfcComplianceMode.Looser, true)]
		public void TestParseMailboxWithEscapedAtSymbol (RfcComplianceMode compliance, bool espected)
		{
			const string text = "First Last <webmaster\\@custom-domain.com@mail-host.com>";
			var options = new ParserOptions { AddressParserComplianceMode = compliance };

			if (espected) {
				Assert.That (InternetAddressList.TryParse (options, text, out var list), Is.True);
				Assert.That (list.Count, Is.EqualTo (1));
				Assert.That (list[0], Is.InstanceOf<MailboxAddress> ());
				var mailbox = (MailboxAddress) list[0];
				Assert.That (mailbox.Address, Is.EqualTo ("webmaster%40custom-domain.com@mail-host.com"));
				Assert.That (mailbox.LocalPart, Is.EqualTo ("webmaster%40custom-domain.com"));
				Assert.That (mailbox.Domain, Is.EqualTo ("mail-host.com"));
			} else {
				Assert.That (InternetAddressList.TryParse (options, text, out _), Is.False);
			}
		}

		#region Rfc7103

		// TODO: test both Strict and Loose RfcCompliance modes

		[Test]
		public void TestParseMailboxWithExcessiveAngleBrackets ()
		{
			const string text = "<<<user2@example.org>>>";
			const string encoded = "user2@example.org";
			var expected = new InternetAddressList {
				new MailboxAddress ("", encoded)
			};

			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestParseMailboxWithMissingGreaterThan ()
		{
			const string text = "<another@example.net";
			const string encoded = "another@example.net";
			var expected = new InternetAddressList {
				new MailboxAddress ("", encoded)
			};

			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestParseMailboxWithMissingLessThan ()
		{
			const string text = "second@example.org>";
			const string encoded = "second@example.org";
			var expected = new InternetAddressList {
				new MailboxAddress ("", encoded)
			};

			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestParseErrantComma ()
		{
			const string text = "<third@example.net, fourth@example.net>";
			const string encoded = "third@example.net, fourth@example.net";
			var expected = new InternetAddressList {
				new MailboxAddress ("", "third@example.net"),
				new MailboxAddress ("", "fourth@example.net")
			};

			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestParseMailboxWithUnbalancedQuotes ()
		{
			const string text = "\"Joe <joe@example.com>";
			const string encoded = "Joe <joe@example.com>";
			var expected = new InternetAddressList {
				new MailboxAddress ("Joe", "joe@example.com")
			};

			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestParseMailboxWithUnbalancedQuotes2 ()
		{
			const string text = "\"Joe <joe@example.com>, Bob <bob@example.com>";
			const string encoded = "Joe <joe@example.com>, Bob <bob@example.com>";
			var expected = new InternetAddressList {
				new MailboxAddress ("Joe", "joe@example.com"),
				new MailboxAddress ("Bob", "bob@example.com")
			};

			AssertParseAndTryParse (text, encoded, expected);
		}

		[Test]
		public void TestParseMailboxWithAddrspecAsUnquotedName ()
		{
			const string encoded = "\"user@example.com\" <user@example.com>";
			const string text = "user@example.com <user@example.com>";
			var expected = new InternetAddressList {
				new MailboxAddress ("user@example.com", "user@example.com")
			};

			AssertParseAndTryParse (text, encoded, expected);
		}

		#endregion

		[Test]
		public void TestTryParseFailsWithInvalidAddrSpec ()
		{
			const string text = "name.@abc.com";

			Assert.That (InternetAddressList.TryParse (text, out _), Is.False);
		}

		[Test]
		public void TestCastToMailAddressCollection ()
		{
			var list = new InternetAddressList {
				new MailboxAddress ("Name Zero", "user0@address.com"),
				new MailboxAddress ("Name One", "user1@address.com"),
				new MailboxAddress ("Name Two", "user2@address.com")
			};

			var collection = (System.Net.Mail.MailAddressCollection) list;

			Assert.That (collection.Count, Is.EqualTo (list.Count), "Count");
			for (int i = 0; i < list.Count; i++) {
				var mailbox = (MailboxAddress) list[i];

				Assert.That (collection[i].DisplayName, Is.EqualTo (mailbox.Name), $"DisplayName[{i}]");
				Assert.That (collection[i].Address, Is.EqualTo (mailbox.Address), $"Address[{i}]");
			}

			list = new InternetAddressList {
				new GroupAddress ("Local recipients", new InternetAddress[] {
					new MailboxAddress ("", "phil"),
					new MailboxAddress ("", "joe"),
					new MailboxAddress ("", "alex"),
					new MailboxAddress ("", "bob"),
				}),
				new MailboxAddress ("Joey", "joey@friends.com"),
				new MailboxAddress ("Chandler", "chandler@friends.com")
			};

			Assert.Throws<InvalidCastException> (() => collection = (System.Net.Mail.MailAddressCollection) list);

			list = null;
			collection = (System.Net.Mail.MailAddressCollection) list;
			Assert.That (collection, Is.Null);
		}

		[Test]
		public void TestCaseFromMailAddressCollection ()
		{
			var collection = new System.Net.Mail.MailAddressCollection {
				new System.Net.Mail.MailAddress ("user0@address.com", "Name Zero"),
				new System.Net.Mail.MailAddress ("user1@address.com", "Name One"),
				new System.Net.Mail.MailAddress ("user2@address.com", "Name Two")
			};

			var list = (InternetAddressList) collection;

			Assert.That (list.Count, Is.EqualTo (collection.Count), "Count");
			for (int i = 0; i < list.Count; i++) {
				var mailbox = (MailboxAddress) list[i];

				Assert.That (mailbox.Name, Is.EqualTo (collection[i].DisplayName), $"Name[{i}]");
				Assert.That (mailbox.Address, Is.EqualTo (collection[i].Address), $"Address[{i}]");
			}

			collection = null;
			list = (InternetAddressList) collection;
			Assert.That (list, Is.Null);
		}
	}
}

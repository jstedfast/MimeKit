//
// AddressParserTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Jeffrey Stedfast
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
	public class AddressParserTests
	{
		static void AssertInternetAddressListsEqual (string text, InternetAddressList expected, InternetAddressList result)
		{
			Assert.AreEqual (expected.Count, result.Count, "Unexpected number of addresses: {0}", text);

			for (int i = 0; i < expected.Count; i++) {
				Assert.AreEqual (expected.GetType (), result.GetType (),
				                 "Address #{0} differs in type: {1}", i, text);

				Assert.AreEqual (expected[i].ToString (), result[i].ToString ());
			}
		}

		[Test]
		public void TestSimpleAddrSpec ()
		{
			InternetAddressList expected = new InternetAddressList ();
			Mailbox mailbox = new Mailbox ("", "");
			InternetAddressList result;
			string text;

			expected.Add (mailbox);

			text = "fejj@helixcode.com";
			mailbox.Address = "fejj@helixcode.com";
			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (text, expected, result);

			text = "fejj";
			mailbox.Address = "fejj";
			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (text, expected, result);
		}

		[Test]
		public void TestExampleAddrSpecWithQuotedLocalPartAndCommentsFromRfc822 ()
		{
			InternetAddressList expected = new InternetAddressList ();
			InternetAddressList result;
			string text;

			text = "\":sysmail\"@  Some-Group. Some-Org,\n Muhammed.(I am  the greatest) Ali @(the)Vegas.WBA";

			expected.Add (new Mailbox ("", "\":sysmail\"@Some-Group.Some-Org"));
			expected.Add (new Mailbox ("", "Muhammed.Ali@Vegas.WBA"));

			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (text, expected, result);
		}

		[Test]
		public void TestExampleMailboxWithCommentsFromRfc5322 ()
		{
			InternetAddressList expected = new InternetAddressList ();
			InternetAddressList result;
			string text;

			text = "Pete(A nice \\) chap) <pete(his account)@silly.test(his host)>";
			expected.Add (new Mailbox ("Pete", "pete@silly.test"));

			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (text, expected, result);
		}

		[Test]
		public void TestSimpleMailboxes ()
		{
			InternetAddressList expected = new InternetAddressList ();
			Mailbox mailbox = new Mailbox ("", "");
			InternetAddressList result;
			string text;

			expected.Add (mailbox);

			mailbox.Name = "Jeffrey Stedfast";
			mailbox.Address = "fejj@helixcode.com";
			text = "Jeffrey Stedfast <fejj@helixcode.com>";
			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (text, expected, result);

			mailbox.Name = "this is a folded name";
			mailbox.Address = "folded@name.com";
			text = "this is\n\ta folded name <folded@name.com>";
			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (text, expected, result);

			mailbox.Name = "Jeffrey fejj Stedfast";
			mailbox.Address = "fejj@helixcode.com";
			text = "Jeffrey \"fejj\" Stedfast <fejj@helixcode.com>";
			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (text, expected, result);

			mailbox.Name = "Jeffrey \"fejj\" Stedfast";
			mailbox.Address = "fejj@helixcode.com";
			text = "\"Jeffrey \\\"fejj\\\" Stedfast\" <fejj@helixcode.com>";
			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (text, expected, result);

			mailbox.Name = "Stedfast, Jeffrey";
			mailbox.Address = "fejj@helixcode.com";
			text = "\"Stedfast, Jeffrey\" <fejj@helixcode.com>";
			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (text, expected, result);

//			mailbox.Name = "Jeffrey Stedfast";
//			mailbox.Address = "fejj@helixcode.com";
//			text = "fejj@helixcode.com (Jeffrey Stedfast)";
//			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
//			AssertInternetAddressListsEqual (text, expected, result);

			mailbox.Name = "Jeffrey Stedfast";
			mailbox.Address = "fejj@helixcode.com";
			text = "Jeffrey Stedfast <fejj(recursive (comment) block)@helixcode.(and a comment here)com>";
			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (text, expected, result);
		}

		[Test]
		public void TestMailboxesWithRfc2047EncodedNames ()
		{
			InternetAddressList expected = new InternetAddressList ();
			Mailbox mailbox = new Mailbox ("", "");
			InternetAddressList result;
			string text;

			expected.Add (mailbox);

			mailbox.Name = "Kristoffer Brånemyr";
			mailbox.Address = "ztion@swipenet.se";
			text = "=?iso-8859-1?q?Kristoffer_Br=E5nemyr?= <ztion@swipenet.se>";
			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (text, expected, result);

			mailbox.Name = "François Pons";
			mailbox.Address = "fpons@mandrakesoft.com";
			text = "=?iso-8859-1?q?Fran=E7ois?= Pons <fpons@mandrakesoft.com>";
			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (text, expected, result);
		}

		[Test]
		public void TestListWithGroupAndAddrspec ()
		{
			InternetAddressList expected = new InternetAddressList ();
			InternetAddressList result;
			string text;

			text = "GNOME Hackers: Miguel de Icaza <miguel@gnome.org>, Havoc Pennington <hp@redhat.com>;, fejj@helixcode.com";

			expected.Add (new Group ("GNOME Hackers", new InternetAddress[] {
				new Mailbox ("Miguel de Icaza", "miguel@gnome.org"),
				new Mailbox ("Havoc Pennington", "hp@redhat.com")
			}));
			expected.Add (new Mailbox ("", "fejj@helixcode.com"));

			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (text, expected, result);
		}

		[Test]
		public void TestLocalGroupWithoutSemicolon ()
		{
			InternetAddressList expected = new InternetAddressList ();
			InternetAddressList result;
			string text;

			text = "Local recipients: phil, joe, alex, bob";

			expected.Add (new Group ("Local recipients", new InternetAddress[] {
				new Mailbox ("", "phil"),
				new Mailbox ("", "joe"),
				new Mailbox ("", "alex"),
				new Mailbox ("", "bob"),
			}));

			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (text, expected, result);
		}

		[Test]
		public void TestExampleGroupWithCommentsFromRfc5322 ()
		{
			InternetAddressList expected = new InternetAddressList ();
			InternetAddressList result;
			string text;

			text = "A Group(Some people):Chris Jones <c@(Chris's host.)public.example>, joe@example.org, John <jdoe@one.test> (my dear friend); (the end of the group)";

			expected.Add (new Group ("A Group", new InternetAddress[] {
				new Mailbox ("Chris Jones", "c@public.example"),
				new Mailbox ("", "joe@example.org"),
				new Mailbox ("John", "jdoe@one.test")
			}));

			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (text, expected, result);
		}

		[Test]
		public void TestObsoleteMailboxRoutingSyntax ()
		{
			InternetAddressList expected = new InternetAddressList ();
			InternetAddressList result;
			string text;

			text = "Routed Address <@route:user@domain.com>";

			expected.Add (new Mailbox ("Routed Address", new string[] { "route" }, "user@domain.com"));

			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (text, expected, result);
		}

		[Test]
		public void TestObsoleteMailboxRoutingSyntaxWithEmptyDomains ()
		{
			InternetAddressList expected = new InternetAddressList ();
			InternetAddressList result;
			string text;

			text = "Routed Address <@route1,,@route2,,,@route3:user@domain.com>";

			expected.Add (new Mailbox ("Routed Address", new string[] { "route1", "route2", "route3" }, "user@domain.com"));

			Assert.IsTrue (InternetAddressList.TryParse (text, out result), "Failed to parse: {0}", text);
			AssertInternetAddressListsEqual (text, expected, result);
		}
	}
}

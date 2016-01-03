//
// GroupAddressTests.cs
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
	public class GroupAddressTests
	{
		static void TestParseFailure (string text, bool result, int tokenIndex, int errorIndex)
		{
			var buffer = text.Length > 0 ? Encoding.ASCII.GetBytes (text) : new byte[1];
			GroupAddress group;

			Assert.AreEqual (result, GroupAddress.TryParse (text, out group), "GroupAddress.TryParse(string)");
			Assert.AreEqual (result, GroupAddress.TryParse (buffer, out group), "GroupAddress.TryParse(byte[])");
			Assert.AreEqual (result, GroupAddress.TryParse (buffer, 0, out group), "GroupAddress.TryParse(byte[], int)");
			Assert.AreEqual (result, GroupAddress.TryParse (buffer, 0, buffer.Length, out group), "GroupAddress.TryParse(byte[], int, int)");

			try {
				GroupAddress.Parse (text);
				Assert.Fail ("GroupAddress.Parse(string) should fail.");
			} catch (ParseException ex) {
				Assert.AreEqual (tokenIndex, ex.TokenIndex, "ParseException did not have the correct token index.");
				Assert.AreEqual (errorIndex, ex.ErrorIndex, "ParseException did not have the error index.");
			} catch {
				Assert.Fail ("GroupAddress.Parse(string) should throw ParseException.");
			}

			try {
				GroupAddress.Parse (buffer);
				Assert.Fail ("GroupAddress.Parse(byte[]) should fail.");
			} catch (ParseException ex) {
				Assert.AreEqual (tokenIndex, ex.TokenIndex, "ParseException did not have the correct token index.");
				Assert.AreEqual (errorIndex, ex.ErrorIndex, "ParseException did not have the error index.");
			} catch {
				Assert.Fail ("GroupAddress.Parse(new byte[]) should throw ParseException.");
			}

			try {
				GroupAddress.Parse (buffer, 0);
				Assert.Fail ("GroupAddress.Parse(byte[], int) should fail.");
			} catch (ParseException ex) {
				Assert.AreEqual (tokenIndex, ex.TokenIndex, "ParseException did not have the correct token index.");
				Assert.AreEqual (errorIndex, ex.ErrorIndex, "ParseException did not have the error index.");
			} catch {
				Assert.Fail ("GroupAddress.Parse(new byte[], int) should throw ParseException.");
			}

			try {
				GroupAddress.Parse (buffer, 0, buffer.Length);
				Assert.Fail ("GroupAddress.Parse(byte[], int, int) should fail.");
			} catch (ParseException ex) {
				Assert.AreEqual (tokenIndex, ex.TokenIndex, "ParseException did not have the correct token index.");
				Assert.AreEqual (errorIndex, ex.ErrorIndex, "ParseException did not have the error index.");
			} catch {
				Assert.Fail ("GroupAddress.Parse(new byte[], int, int) should throw ParseException.");
			}
		}

		static void TestParse (string text)
		{
			var buffer = Encoding.ASCII.GetBytes (text);
			GroupAddress group;

			try {
				Assert.IsTrue (GroupAddress.TryParse (text, out group), "GroupAddress.TryParse(string) should succeed.");
			} catch (Exception ex) {
				Assert.Fail ("GroupAddress.TryParse(string) should not throw an exception: {0}", ex);
			}

			try {
				Assert.IsTrue (GroupAddress.TryParse (buffer, out group), "GroupAddress.TryParse(byte[]) should succeed.");
			} catch (Exception ex) {
				Assert.Fail ("GroupAddress.TryParse(byte[]) should not throw an exception: {0}", ex);
			}

			try {
				Assert.IsTrue (GroupAddress.TryParse (buffer, 0, out group), "GroupAddress.TryParse(byte[], int) should succeed.");
			} catch (Exception ex) {
				Assert.Fail ("GroupAddress.TryParse(byte[], int) should not throw an exception: {0}", ex);
			}

			try {
				Assert.IsTrue (GroupAddress.TryParse (buffer, 0, buffer.Length, out group), "GroupAddress.TryParse(byte[], int, int) should succeed.");
			} catch (Exception ex) {
				Assert.Fail ("GroupAddress.TryParse(byte[], int, int) should not throw an exception: {0}", ex);
			}

			try {
				group = GroupAddress.Parse (text);
			} catch (Exception ex) {
				Assert.Fail ("GroupAddress.Parse(string) should not throw an exception: {0}", ex);
			}

			try {
				group = GroupAddress.Parse (buffer);
			} catch (Exception ex) {
				Assert.Fail ("GroupAddress.Parse(string) should not throw an exception: {0}", ex);
			}

			try {
				group = GroupAddress.Parse (buffer, 0);
			} catch (Exception ex) {
				Assert.Fail ("GroupAddress.Parse(string) should not throw an exception: {0}", ex);
			}

			try {
				group = GroupAddress.Parse (buffer, 0, buffer.Length);
			} catch (Exception ex) {
				Assert.Fail ("GroupAddress.Parse(string) should not throw an exception: {0}", ex);
			}
		}

		[Test]
		public void TestParseEmpty ()
		{
			TestParseFailure (string.Empty, false, 0, 0);
		}

		[Test]
		public void TestParseWhiteSpace ()
		{
			const string text = " \t\r\n";
			int tokenIndex = text.Length;
			int errorIndex = text.Length;

			TestParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseNameLessThan ()
		{
			const string text = "Name <";
			int tokenIndex = 0;
			int errorIndex = text.IndexOf ('<');

			TestParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseIncompleteMailbox ()
		{
			const string text = "jeff@";
			int tokenIndex = 0;
			int errorIndex = text.IndexOf ('@');

			TestParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseMailboxWithIncompleteComment ()
		{
			const string text = "jeff@xamarin.com (incomplete comment";
			int tokenIndex = 0;
			int errorIndex = text.IndexOf ('@');

			TestParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseAddrspec ()
		{
			const string text = "jeff@xamarin.com";
			int tokenIndex = 0;
			int errorIndex = text.IndexOf ('@');

			TestParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseMailbox ()
		{
			const string text = "Jeffrey Stedfast <jeff@xamarin.com>";
			int tokenIndex = 0;
			int errorIndex = text.IndexOf ('<');

			TestParseFailure (text, false, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseGroup ()
		{
			const string text = "Agents of Shield: Skye <skye@shield.gov>, Leo Fitz <fitz@shield.gov>, Melinda May <may@shield.gov>;";

			TestParse (text);
		}

		[Test]
		public void TestParseIncompleteGroup ()
		{
			const string text = "Agents of Shield: Skye <skye@shield.gov>, Leo Fitz <fitz@shield.gov>, Melinda May <may@shield.gov>";
			int tokenIndex = 0;
			int errorIndex = text.Length;

			// Note: the TryParse() methods are a little more forgiving than Parse().
			TestParseFailure (text, true, tokenIndex, errorIndex);
		}

		[Test]
		public void TestParseGroupAndMailbox ()
		{
			const string text = "Agents of Shield: Skye <skye@shield.gov>, Leo Fitz <fitz@shield.gov>, May <may@shield.gov>;, Fury <fury@shield.gov>";
			int tokenIndex = text.IndexOf (';') + 1;
			int errorIndex = tokenIndex;

			TestParseFailure (text, false, tokenIndex, errorIndex);
		}
	}
}

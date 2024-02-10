//
// ParseUtilsTests.cs
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

namespace UnitTests.Utils {
	[TestFixture]
	public class ParseUtilsTests
	{
		[Test]
		public void TestTryParseInt32 ()
		{
			int index, value;
			byte[] buffer;

			// make sure that we can parse MaxValue
			buffer = Encoding.ASCII.GetBytes (int.MaxValue.ToString ());
			index = 0;

			Assert.That (ParseUtils.TryParseInt32 (buffer, ref index, buffer.Length, out value), Is.True, "Failed to parse MaxValue.");
			Assert.That (value, Is.EqualTo (int.MaxValue));

			// make sure that MaxValue+1 fails
			buffer = Encoding.ASCII.GetBytes ((((long) int.MaxValue) + 1).ToString ());
			index = 0;

			Assert.That (ParseUtils.TryParseInt32 (buffer, ref index, buffer.Length, out _), Is.False, "Parsing MaxValue+1 should result in overflow.");

			// make sure that MaxValue*10 fails
			buffer = Encoding.ASCII.GetBytes ((((long) int.MaxValue) * 10).ToString ());
			index = 0;

			Assert.That (ParseUtils.TryParseInt32 (buffer, ref index, buffer.Length, out _), Is.False, "Parsing MaxValue*10 should result in overflow.");
		}

		[Test]
		public void TestSkipBadlyQuoted ()
		{
			var buffer = Encoding.ASCII.GetBytes ("\"This is missing the end quote.");
			int index = 0;

			Assert.That (ParseUtils.SkipQuoted (buffer, ref index, buffer.Length, false), Is.False, "Skipping an unterminated qstring should have failed.");
			Assert.That (index, Is.EqualTo (buffer.Length), "The index should be at the end of the buffer.");

			index = 0;

			var ex = Assert.Throws<ParseException> (() => ParseUtils.SkipQuoted (buffer, ref index, buffer.Length, true), "An exception should have been thrown.");
			Assert.That (ex.TokenIndex, Is.EqualTo (0), "The token index should be 0.");
			Assert.That (ex.ErrorIndex, Is.EqualTo (buffer.Length), "The error index should be at the end of the buffer.");
		}

		static readonly string[] GoodDomains = {
			"[127.0.0.1]",                                "[127.0.0.1]",
			"amazon (comment) . (comment) com (comment)", "amazon.com",
			"测试文本.cn",                                 "测试文本.cn",
		};

		[Test]
		public void TestTryParseGoodDomains ()
		{
			var sentinels = new [] { (byte) ',' };

			for (int i = 0; i < GoodDomains.Length; i += 2) {
				var buffer = Encoding.UTF8.GetBytes (GoodDomains[i]);
				string domain;
				int index = 0;

				Assert.That (ParseUtils.TryParseDomain (buffer, ref index, buffer.Length, sentinels, false, out domain), Is.True, $"Should have parsed '{GoodDomains[i]}'.");
				Assert.That (domain, Is.EqualTo (GoodDomains[i + 1]), "Parsed domains did not match.");
			}
		}

		static readonly string[] BadDomains = {
			"[127.0.0.1",                      // missing ']'
			"[127\\.0.0.1]",                   // backslash is illegal
			"amazon (comment) . (comment com", // missing ')'
			"测试文本.cn",                      // illegal unless in UTF-8
		};
		static readonly int[] BadDomainTokenIndexes = {
			0, 0, 19, 0
		};
		static readonly int[] BadDomainErrorIndexes = {
			10, 4, 31, 0
		};

		[Test]
		public void TestTryParseBadDomains ()
		{
			var sentinels = new [] { (byte) ',' };

			for (int i = 0; i < BadDomains.Length; i++) {
				var buffer = Encoding.UTF8.GetBytes (BadDomains[i]);
				string domain;
				int index = 0;

				if (BadDomains[i][0] > 127)
					buffer = Encoding.Convert (Encoding.UTF8, Encoding.GetEncoding ("GB18030"), buffer);

				Assert.That (ParseUtils.TryParseDomain (buffer, ref index, buffer.Length, sentinels, false, out domain), Is.False, $"Should have failed to parse '{BadDomains[i]}'.");

				index = 0;

				var ex = Assert.Throws<ParseException> (() => ParseUtils.TryParseDomain (buffer, ref index, buffer.Length, sentinels, true, out domain), "Parsing '{0}' should have thrown.", BadDomains[i]);
				Assert.That (ex.TokenIndex, Is.EqualTo (BadDomainTokenIndexes[i]), "Unexpected token index.");
				Assert.That (ex.ErrorIndex, Is.EqualTo (BadDomainErrorIndexes[i]), "Unexpected error index.");
			}
		}

		static readonly string[] MsgIdInputs = {
			" <Messe_Bauma_rz(1)_ae284449-6bdc-488f-8ec3-5be5e5b09efb.jpg>",
			" Messe_Bauma_rz(1)_ae284449-6bdc-488f-8ec3-5be5e5b09efb.jpg",
			" <15627601.388658.1676916781911.JavaMail.\"xxxxxx@united.com\"@xxxxxxx.ual.com>"
		};
		static readonly string[] MsgIdOutputs = {
			"Messe_Bauma_rz(1)_ae284449-6bdc-488f-8ec3-5be5e5b09efb.jpg",
			"Messe_Bauma_rz(1)_ae284449-6bdc-488f-8ec3-5be5e5b09efb.jpg",
			"15627601.388658.1676916781911.JavaMail.\"xxxxxx@united.com\"@xxxxxxx.ual.com"
		};

		[Test]
		public void TestTryParseMsgIdTokens ()
		{
			for (int i = 0; i < MsgIdInputs.Length; i++) {
				var buffer = Encoding.ASCII.GetBytes (MsgIdInputs[i]);
				int endIndex = buffer.Length;
				int index = 0;
				string msgid;

				Assert.That (ParseUtils.TryParseMsgId (buffer, ref index, endIndex, false, false, out msgid), Is.True, "TryParseMsgId");
				Assert.That (msgid, Is.EqualTo (MsgIdOutputs[i]), $"MsgIdOutputs[{i}]");
			}
		}

		[Test]
		public void TestTryParseMsgIdEmptyString ()
		{
			var buffer = Encoding.ASCII.GetBytes (" ");
			int index = 0;
			string msgid;

			Assert.That (ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, false, out msgid), Is.False, "TryParseMsgId");

			try {
				index = 0;
				ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, true, out msgid);
				Assert.Fail ("throwOnError");
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (1), "TokenIndex");
				Assert.That (ex.ErrorIndex, Is.EqualTo (1), "ErrorIndex");
			}
		}

		[Test]
		public void TestTryParseMsgIdLessThan ()
		{
			var buffer = Encoding.ASCII.GetBytes (" <");
			int index = 0;
			string msgid;

			Assert.That (ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, false, out msgid), Is.False, "TryParseMsgId");

			try {
				index = 0;
				ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, true, out msgid);
				Assert.Fail ("throwOnError");
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (1), "TokenIndex");
				Assert.That (ex.ErrorIndex, Is.EqualTo (2), "ErrorIndex");
			}
		}

		[Test]
		public void TestTryParseMsgIdLessThanLocalPart ()
		{
			var buffer = Encoding.ASCII.GetBytes (" <local-part");
			int index = 0;
			string msgid;

			Assert.That (ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, false, out msgid), Is.False, "TryParseMsgId");

			try {
				index = 0;
				ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, true, out msgid);
				Assert.Fail ("throwOnError");
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (1), "TokenIndex");
				Assert.That (ex.ErrorIndex, Is.EqualTo (12), "ErrorIndex");
			}
		}

		//[Test]
		//public void TestTryParseMsgIdLessThanLocalPartCtrl ()
		//{
		//	var buffer = Encoding.ASCII.GetBytes (" <local-part\b");
		//	int index = 0;
		//	string msgid;

		//	Assert.That (ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, false, out msgid), Is.False, "TryParseMsgId");

		//	try {
		//		index = 0;
		//		ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, true, out msgid);
		//		Assert.Fail ("throwOnError");
		//	} catch (ParseException ex) {
		//		Assert.That (ex.TokenIndex, Is.EqualTo (1), "TokenIndex");
		//		Assert.That (ex.ErrorIndex, Is.EqualTo (12), "ErrorIndex");
		//	}
		//}

		[Test]
		public void TestTryParseMsgIdLessThanLocalPartDot ()
		{
			var buffer = Encoding.ASCII.GetBytes (" <local-part.");
			int index = 0;
			string msgid;

			Assert.That (ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, false, out msgid), Is.False, "TryParseMsgId");

			try {
				index = 0;
				ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, true, out msgid);
				Assert.Fail ("throwOnError");
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (1), "TokenIndex");
				Assert.That (ex.ErrorIndex, Is.EqualTo (13), "ErrorIndex");
			}
		}

		[Test]
		public void TestTryParseMsgIdLessThanLocalPartAt ()
		{
			var buffer = Encoding.ASCII.GetBytes (" <local-part@");
			int index = 0;
			string msgid;

			Assert.That (ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, false, out msgid), Is.False, "TryParseMsgId");

			try {
				index = 0;
				ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, true, out msgid);
				Assert.Fail ("throwOnError");
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (1), "TokenIndex");
				Assert.That (ex.ErrorIndex, Is.EqualTo (13), "ErrorIndex");
			}
		}

		[Test]
		public void TestTryParseMsgIdLessThanLocalPartAtGreaterThan ()
		{
			var buffer = Encoding.ASCII.GetBytes (" <local-part@>");
			int index = 0;
			string msgid;

			Assert.That (ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, false, out msgid), Is.True, "TryParseMsgId");
			Assert.That (msgid, Is.EqualTo ("local-part@"));
		}

		[Test]
		public void TestTryParseMsgIdLessThanLocalPartAtDomainMissingGreaterThan ()
		{
			var buffer = Encoding.ASCII.GetBytes (" <local-part@domain");
			int index = 0;
			string msgid;

			Assert.That (ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, false, out msgid), Is.False, "TryParseMsgId");

			try {
				index = 0;
				ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, true, out msgid);
				Assert.Fail ("throwOnError");
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (1), "TokenIndex");
				Assert.That (ex.ErrorIndex, Is.EqualTo (19), "ErrorIndex");
			}
		}

		[Test]
		public void TestTryParseMsgIdInvalidQuotedLocalPart ()
		{
			var buffer = Encoding.ASCII.GetBytes (" <\"quoted-string@domain>");
			int index = 0;
			string msgid;

			Assert.That (ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, false, out msgid), Is.False, "TryParseMsgId");

			try {
				index = 0;
				ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, true, out msgid);
				Assert.Fail ("throwOnError");
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (2), "TokenIndex");
				Assert.That (ex.ErrorIndex, Is.EqualTo (24), "ErrorIndex");
			}
		}

		[Test]
		public void TestTryParseMsgIdInvalidInternationalLocalPart ()
		{
			var buffer = Encoding.GetEncoding ("iso-8859-1").GetBytes (" <æøå@domain>");
			int index = 0;
			string msgid;

			Assert.That (ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, false, out msgid), Is.False, "TryParseMsgId");

			try {
				index = 0;
				ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, true, out msgid);
				Assert.Fail ("throwOnError");
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (2), "TokenIndex");
				Assert.That (ex.ErrorIndex, Is.EqualTo (2), "ErrorIndex");
			}
		}

		[Test]
		public void TestTryParseMsgIdWithIdnDomain ()
		{
			var buffer = Encoding.ASCII.GetBytes (" <id@xn--v8jxj3d1dzdz08w.com>");
			const string expected = "id@名がドメイン.com";
			int index = 0;
			string msgid;

			Assert.That (ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, false, out msgid), Is.True, "TryParseMsgId");
			Assert.That (msgid, Is.EqualTo (expected), "msgid");

			index = 0;
			Assert.That (ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, true, out msgid), Is.True, "TryParseMsgId+thowOnError");
			Assert.That (msgid, Is.EqualTo (expected), "msgid");
		}
	}
}

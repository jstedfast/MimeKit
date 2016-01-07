//
// DomainListTests.cs
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

using NUnit.Framework;

using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class DomainListTests
	{
		static void AssertParseFailure (string text)
		{
			DomainList route;

			Assert.IsFalse (DomainList.TryParse (text, out route), "DomainList.TryParse(string)");
		}

		static void AssertParse (string text, DomainList expected)
		{
			DomainList route;
			int index = 0;

			Assert.IsTrue (DomainList.TryParse (text, out route), "DomainList.TryParse(string)");
			Assert.IsFalse (route.IsReadOnly, "IsReadOnly");
			Assert.AreEqual (expected.Count, route.Count, "Count");

			foreach (var domain in expected) {
				Assert.AreEqual (index, route.IndexOf (domain), "IndexOf");
				Assert.IsTrue (route.Contains (domain), "Contains");
				Assert.AreEqual (expected[index], route[index]);
				index++;
			}
		}

		[Test]
		public void TestParseEmpty ()
		{
			AssertParseFailure (string.Empty);
		}

		[Test]
		public void TestParseWhiteSpace ()
		{
			AssertParseFailure (" \t\r\n");
		}

		[Test]
		public void TestParseUnterminatedComment ()
		{
			AssertParseFailure (" (this is an unterminated comment...");
		}

		[Test]
		public void TestParseAt ()
		{
			AssertParseFailure ("@");
		}

		[Test]
		public void TestParseAtUnterminatedComment ()
		{
			AssertParseFailure ("@ (this is an unterminated comment...");
		}

		[Test]
		public void TestParseInvalidDomain ()
		{
			AssertParseFailure ("@[invalid.domain");
		}

		[Test]
		public void TestParseEmptyDomains ()
		{
			var expected = new DomainList (new [] { "domain1", "domain2" });

			AssertParse ("@domain1,,@domain2", expected);
		}
	}
}

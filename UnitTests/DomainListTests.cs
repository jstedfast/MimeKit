//
// DomainListTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 .NET Foundation and Contributors
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
		public void TestArgumentExceptions ()
		{
			var list = new DomainList ();

			Assert.Throws<ArgumentNullException> (() => new DomainList (null));
			Assert.Throws<ArgumentNullException> (() => list.Add (null));
			Assert.Throws<ArgumentNullException> (() => list.Contains (null));
			Assert.Throws<ArgumentNullException> (() => list.CopyTo (null, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => list.CopyTo (new string[0], -1));
			Assert.Throws<ArgumentNullException> (() => list.IndexOf (null));
			Assert.Throws<ArgumentOutOfRangeException> (() => list.Insert (-1, "item"));
			Assert.Throws<ArgumentNullException> (() => list.Insert (0, null));
			Assert.Throws<ArgumentNullException> (() => list[0] = null);
			Assert.Throws<ArgumentNullException> (() => list.Remove (null));
			Assert.Throws<ArgumentOutOfRangeException> (() => list.RemoveAt (-1));
		}

		[Test]
		public void TestBasicListFunctionality ()
		{
			var list = new DomainList ();

			Assert.IsFalse (list.IsReadOnly);
			Assert.AreEqual (0, list.Count, "Initial count");

			list.Add ("domain2");

			Assert.AreEqual (1, list.Count);
			Assert.AreEqual ("domain2", list[0]);

			list.Insert (0, "domain0");
			list.Insert (1, "domain1");

			Assert.AreEqual (3, list.Count);
			Assert.AreEqual ("domain0", list[0]);
			Assert.AreEqual ("domain1", list[1]);
			Assert.AreEqual ("domain2", list[2]);

			Assert.IsTrue (list.Contains ("domain1"), "Contains");
			Assert.AreEqual (1, list.IndexOf ("domain1"), "IndexOf");

			var array = new string[list.Count];
			list.CopyTo (array, 0);
			list.Clear ();

			Assert.AreEqual (0, list.Count);

			foreach (var domain in array)
				list.Add (domain);

			Assert.AreEqual (array.Length, list.Count);

			Assert.IsFalse (list.Remove ("not-in-the-list"));
			Assert.IsTrue (list.Remove ("domain2"));
			Assert.AreEqual (2, list.Count);
			Assert.AreEqual ("domain0", list[0]);
			Assert.AreEqual ("domain1", list[1]);

			list.RemoveAt (0);

			Assert.AreEqual (1, list.Count);
			Assert.AreEqual ("domain1", list[0]);

			list[0] = "domain";

			Assert.AreEqual (1, list.Count);
			Assert.AreEqual ("domain", list[0]);
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

		[Test]
		public void TestToString ()
		{
			var route = new DomainList ();
			route.Add ("route1");
			route.Add ("  \t\t ");
			route.Add ("route2");

			Assert.AreEqual ("@route1,@route2", route.ToString ());
		}
	}
}

//
// DomainListTests.cs
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

using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class DomainListTests
	{
		static void AssertParseFailure (string text)
		{
			Assert.That (DomainList.TryParse (text, out _), Is.False, "DomainList.TryParse(string)");
		}

		static void AssertParse (string text, DomainList expected)
		{
			DomainList route;
			int index = 0;

			Assert.That (DomainList.TryParse (text, out route), Is.True, "DomainList.TryParse(string)");
			Assert.That (route.IsReadOnly, Is.False, "IsReadOnly");
			Assert.That (route.Count, Is.EqualTo (expected.Count), "Count");

			foreach (var domain in expected) {
				Assert.That (route.IndexOf (domain), Is.EqualTo (index), "IndexOf");
				Assert.That (route.Contains (domain), Is.True, "Contains");
				Assert.That (route[index], Is.EqualTo (expected[index]));
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
			Assert.Throws<ArgumentOutOfRangeException> (() => list.CopyTo (Array.Empty<string> (), -1));
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

			Assert.That (list.IsReadOnly, Is.False);
			Assert.That (list.Count, Is.EqualTo (0), "Initial count");

			list.Add ("domain2");

			Assert.That (list.Count, Is.EqualTo (1));
			Assert.That (list[0], Is.EqualTo ("domain2"));

			list.Insert (0, "domain0");
			list.Insert (1, "domain1");

			Assert.That (list.Count, Is.EqualTo (3));
			Assert.That (list[0], Is.EqualTo ("domain0"));
			Assert.That (list[1], Is.EqualTo ("domain1"));
			Assert.That (list[2], Is.EqualTo ("domain2"));

			Assert.That (list.Contains ("domain1"), Is.True, "Contains");
			Assert.That (list.IndexOf ("domain1"), Is.EqualTo (1), "IndexOf");

			var array = new string[list.Count];
			list.CopyTo (array, 0);
			list.Clear ();

			Assert.That (list.Count, Is.EqualTo (0));

			foreach (var domain in array)
				list.Add (domain);

			Assert.That (list.Count, Is.EqualTo (array.Length));

			Assert.That (list.Remove ("not-in-the-list"), Is.False);
			Assert.That (list.Remove ("domain2"), Is.True);
			Assert.That (list.Count, Is.EqualTo (2));
			Assert.That (list[0], Is.EqualTo ("domain0"));
			Assert.That (list[1], Is.EqualTo ("domain1"));

			list.RemoveAt (0);

			Assert.That (list.Count, Is.EqualTo (1));
			Assert.That (list[0], Is.EqualTo ("domain1"));

			list[0] = "domain";

			Assert.That (list.Count, Is.EqualTo (1));
			Assert.That (list[0], Is.EqualTo ("domain"));
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
			var route = new DomainList {
				"route1",
				"  \t\t ",
				"route2"
			};

			Assert.That (route.ToString (), Is.EqualTo ("@route1,@route2"));
		}
	}
}

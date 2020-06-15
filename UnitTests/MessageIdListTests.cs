//
// MessageIdListTests.cs
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
	public class MessageIdListTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var list = new MessageIdList ();

			Assert.Throws<ArgumentNullException> (() => list.Add (null));
			Assert.Throws<ArgumentNullException> (() => list.AddRange (null));
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
			var list = new MessageIdList ();

			Assert.IsFalse (list.IsReadOnly);
			Assert.AreEqual (0, list.Count, "Initial count");

			list.Add ("id2@localhost");

			Assert.AreEqual (1, list.Count);
			Assert.AreEqual ("id2@localhost", list[0]);

			list.Insert (0, "id0@localhost");
			list.Insert (1, "id1@localhost");

			Assert.AreEqual (3, list.Count);
			Assert.AreEqual ("id0@localhost", list[0]);
			Assert.AreEqual ("id1@localhost", list[1]);
			Assert.AreEqual ("id2@localhost", list[2]);

			var clone = list.Clone ();

			Assert.AreEqual (3, clone.Count);
			Assert.AreEqual ("id0@localhost", clone[0]);
			Assert.AreEqual ("id1@localhost", clone[1]);
			Assert.AreEqual ("id2@localhost", clone[2]);

			Assert.IsTrue (list.Contains ("id1@localhost"), "Contains");
			Assert.AreEqual (1, list.IndexOf ("id1@localhost"), "IndexOf");

			var array = new string[list.Count];
			list.CopyTo (array, 0);
			list.Clear ();

			Assert.AreEqual (0, list.Count);

			list.AddRange (array);

			Assert.AreEqual (array.Length, list.Count);

			Assert.IsTrue (list.Remove ("id2@localhost"));
			Assert.AreEqual (2, list.Count);
			Assert.AreEqual ("id0@localhost", list[0]);
			Assert.AreEqual ("id1@localhost", list[1]);

			list.RemoveAt (0);

			Assert.AreEqual (1, list.Count);
			Assert.AreEqual ("id1@localhost", list[0]);

			list[0] = "id@localhost";

			Assert.AreEqual (1, list.Count);
			Assert.AreEqual ("id@localhost", list[0]);
		}
	}
}

//
// MessageIdListTests.cs
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

using System.Collections;

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
			var list = new MessageIdList ();

			Assert.That (list.IsReadOnly, Is.False);
			Assert.That (list.Count, Is.EqualTo (0), "Initial count");

			list.Add ("id2@localhost");

			Assert.That (list.Count, Is.EqualTo (1));
			Assert.That (list[0], Is.EqualTo ("id2@localhost"));

			list.Insert (0, "id0@localhost");
			list.Insert (1, "id1@localhost");

			Assert.That (list.Count, Is.EqualTo (3));
			Assert.That (list[0], Is.EqualTo ("id0@localhost"));
			Assert.That (list[1], Is.EqualTo ("id1@localhost"));
			Assert.That (list[2], Is.EqualTo ("id2@localhost"));

			var clone = list.Clone ();

			Assert.That (clone.Count, Is.EqualTo (3));
			Assert.That (clone[0], Is.EqualTo ("id0@localhost"));
			Assert.That (clone[1], Is.EqualTo ("id1@localhost"));
			Assert.That (clone[2], Is.EqualTo ("id2@localhost"));

			Assert.That (list.Contains ("id1@localhost"), Is.True, "Contains");
			Assert.That (list.IndexOf ("id1@localhost"), Is.EqualTo (1), "IndexOf");

			var array = new string[list.Count];
			list.CopyTo (array, 0);
			list.Clear ();

			Assert.That (list.Count, Is.EqualTo (0));

			list.AddRange (array);

			Assert.That (list.Count, Is.EqualTo (array.Length));

			Assert.That (list.Remove ("id2@localhost"), Is.True);
			Assert.That (list.Count, Is.EqualTo (2));
			Assert.That (list[0], Is.EqualTo ("id0@localhost"));
			Assert.That (list[1], Is.EqualTo ("id1@localhost"));

			list.RemoveAt (0);

			Assert.That (list.Count, Is.EqualTo (1));
			Assert.That (list[0], Is.EqualTo ("id1@localhost"));

			list[0] = "id@localhost";

			Assert.That (list.Count, Is.EqualTo (1));
			Assert.That (list[0], Is.EqualTo ("id@localhost"));
		}

		[Test]
		public void TestGetEnumerator ()
		{
			var list = new MessageIdList ();

			for (int i = 0; i < 5; i++)
				list.Add ($"{i}@example.com");

			int index = 0;
			foreach (string msgid in list)
				Assert.That (msgid, Is.EqualTo ($"{index++}@example.com"));

			index = 0;
			foreach (string msgid in (IEnumerable) list)
				Assert.That (msgid, Is.EqualTo ($"{index++}@example.com"));
		}
	}
}

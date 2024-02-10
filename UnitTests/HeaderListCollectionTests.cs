//
// HeaderListCollectionTests.cs
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
	public class HeaderListCollectionTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var collection = new HeaderListCollection ();
			var array = new HeaderList[10];

			Assert.Throws<ArgumentNullException> (() => collection.Add (null));
			Assert.Throws<ArgumentNullException> (() => collection.Contains (null));
			Assert.Throws<ArgumentNullException> (() => collection.CopyTo (null, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => collection.CopyTo (array, -1));
			Assert.Throws<ArgumentNullException> (() => collection.Remove (null));
			Assert.Throws<ArgumentOutOfRangeException> (() => { var x = collection[0]; });
			Assert.Throws<ArgumentOutOfRangeException> (() => collection[0] = new HeaderList ());

			collection.Add (new HeaderList ());
			Assert.Throws<ArgumentNullException> (() => collection[0] = null);
			Assert.DoesNotThrow (() => collection[0] = new HeaderList ());
		}

		[Test]
		public void TestCopyTo ()
		{
			var collection = new HeaderListCollection ();
			var array = new HeaderList[1];

			collection.Add (new HeaderList ());

			collection.CopyTo (array, 0);

			Assert.That (array[0], Is.EqualTo (collection[0]), "CopyTo results");
		}

		[Test]
		public void TestRemove ()
		{
			var collection = new HeaderListCollection {
				new HeaderList ()
			};
			collection[0] = collection[0];

			Assert.That (collection.Remove (new HeaderList ()), Is.False, "Remove should fail");
			Assert.That (collection.Remove (collection[0]), Is.True, "Remove should work");
		}

		[Test]
		public void TestEnumerator ()
		{
			var collection = new HeaderListCollection {
				new HeaderList (),
				new HeaderList (),
				new HeaderList ()
			};

			collection[0].Add (HeaderId.Subject, "This is HeaderList #0");
			collection[1].Add (HeaderId.Subject, "This is HeaderList #1");
			collection[2].Add (HeaderId.Subject, "This is HeaderList #2");

			int index = 0;
			foreach (var list in collection) {
				Assert.That (list[HeaderId.Subject], Is.EqualTo ($"This is HeaderList #{index}"));
				index++;
			}

			index = 0;
			foreach (HeaderList list in ((IEnumerable) collection)) {
				Assert.That (list[HeaderId.Subject], Is.EqualTo ($"This is HeaderList #{index}"));
				index++;
			}
		}
	}
}

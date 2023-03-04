//
// HtmlAttributeCollectionTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2023 .NET Foundation and Contributors
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

using MimeKit.Text;

namespace UnitTests {
	[TestFixture]
	public class HtmlAttributeCollectionTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var items = new HtmlAttribute[] { new HtmlAttribute (HtmlAttributeId.Alt, "This is some alt text."), new HtmlAttribute (HtmlAttributeId.Text, "And this is the text.") };
			var collection = new HtmlAttributeCollection (items);
			HtmlAttribute attr;

			Assert.Throws<ArgumentNullException> (() => new HtmlAttributeCollection (null));
			Assert.Throws<ArgumentNullException> (() => collection.Add (null));
			Assert.Throws<ArgumentNullException> (() => collection.Contains (null));
			Assert.Throws<ArgumentNullException> (() => collection.TryGetValue (null, out attr));
		}

		[Test]
		public void TestEmpty ()
		{
			Assert.AreEqual (0, HtmlAttributeCollection.Empty.Count);
		}

		[Test]
		public void TestContains ()
		{
			var items = new HtmlAttribute[] { new HtmlAttribute (HtmlAttributeId.Alt, "This is some alt text."), new HtmlAttribute (HtmlAttributeId.Text, "And this is the text.") };
			var collection = new HtmlAttributeCollection (items);

			Assert.IsTrue (collection.Contains (HtmlAttributeId.Alt), "HtmlAttributeId.Alt");
			Assert.IsTrue (collection.Contains (HtmlAttributeId.Text), "HtmlAttributeId.Text");
			Assert.IsFalse (collection.Contains (HtmlAttributeId.Background), "HtmlAttributeId.Background");

			Assert.IsTrue (collection.Contains ("alt"), "alt");
			Assert.IsTrue (collection.Contains ("text"), "text");
			Assert.IsFalse (collection.Contains ("background"), "background");
		}

		[Test]
		public void TestIndexOf ()
		{
			var items = new HtmlAttribute[] { new HtmlAttribute (HtmlAttributeId.Alt, "This is some alt text."), new HtmlAttribute (HtmlAttributeId.Text, "And this is the text.") };
			var collection = new HtmlAttributeCollection (items);

			Assert.AreEqual (0, collection.IndexOf (HtmlAttributeId.Alt), "HtmlAttributeId.Alt");
			Assert.AreEqual (1, collection.IndexOf (HtmlAttributeId.Text), "HtmlAttributeId.Text");
			Assert.AreEqual (-1, collection.IndexOf (HtmlAttributeId.Background), "HtmlAttributeId.Background");

			Assert.AreEqual (0, collection.IndexOf ("alt"), "alt");
			Assert.AreEqual (1, collection.IndexOf ("text"), "text");
			Assert.AreEqual (-1, collection.IndexOf ("background"), "background");
		}

		[Test]
		public void TestTryGetValue ()
		{
			var items = new HtmlAttribute[] { new HtmlAttribute (HtmlAttributeId.Alt, "This is some alt text."), new HtmlAttribute (HtmlAttributeId.Text, "And this is the text.") };
			var collection = new HtmlAttributeCollection (items);
			HtmlAttribute attr;

			Assert.IsTrue (collection.TryGetValue (HtmlAttributeId.Alt, out attr), "HtmlAttributeId.Alt");
			Assert.AreEqual ("This is some alt text.", attr.Value, "HtmlAttributeId.Alt Value");
			Assert.IsTrue (collection.TryGetValue (HtmlAttributeId.Text, out attr), "HtmlAttributeId.Text");
			Assert.AreEqual ("And this is the text.", attr.Value, "HtmlAttributeId.Text Value");
			Assert.IsFalse (collection.TryGetValue (HtmlAttributeId.Background, out attr), "HtmlAttributeId.Background");
			Assert.IsNull (attr, "HtmlAttributeId.Background is not null");

			Assert.IsTrue (collection.TryGetValue ("alt", out attr), "alt");
			Assert.AreEqual ("This is some alt text.", attr.Value, "alt Value");
			Assert.IsTrue (collection.TryGetValue ("text", out attr), "text");
			Assert.AreEqual ("And this is the text.", attr.Value, "text Value");
			Assert.IsFalse (collection.TryGetValue ("background", out attr), "background");
			Assert.IsNull (attr, "background is not null");
		}

		[Test]
		public void TestEnumerator ()
		{
			var items = new HtmlAttribute[] { new HtmlAttribute (HtmlAttributeId.Alt, "This is some alt text."), new HtmlAttribute (HtmlAttributeId.Text, "And this is the text.") };
			var collection = new HtmlAttributeCollection (items);
			int index = 0;

			foreach (var attr in collection) {
				Assert.AreEqual (items[index], attr, "GetEnumerator<HtmlAttribute>() index = {0}", index);
				index++;
			}

			index = 0;
			foreach (HtmlAttribute item in ((IEnumerable) collection)) {
				Assert.AreEqual (items[index], item, "GetEnumerator<HtmlAttribute>() index = {0}", index);
				index++;
			}
		}
	}
}

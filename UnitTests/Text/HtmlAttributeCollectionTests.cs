//
// HtmlAttributeCollectionTests.cs
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

using MimeKit.Text;

namespace UnitTests.Text {
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
			Assert.That (HtmlAttributeCollection.Empty.Count, Is.EqualTo (0));
		}

		[Test]
		public void TestContains ()
		{
			var items = new HtmlAttribute[] { new HtmlAttribute (HtmlAttributeId.Alt, "This is some alt text."), new HtmlAttribute (HtmlAttributeId.Text, "And this is the text.") };
			var collection = new HtmlAttributeCollection (items);

			Assert.That (collection.Contains (HtmlAttributeId.Alt), Is.True, "HtmlAttributeId.Alt");
			Assert.That (collection.Contains (HtmlAttributeId.Text), Is.True, "HtmlAttributeId.Text");
			Assert.That (collection.Contains (HtmlAttributeId.Background), Is.False, "HtmlAttributeId.Background");

			Assert.That (collection.Contains ("alt"), Is.True, "alt");
			Assert.That (collection.Contains ("text"), Is.True, "text");
			Assert.That (collection.Contains ("background"), Is.False, "background");
		}

		[Test]
		public void TestIndexOf ()
		{
			var items = new HtmlAttribute[] { new HtmlAttribute (HtmlAttributeId.Alt, "This is some alt text."), new HtmlAttribute (HtmlAttributeId.Text, "And this is the text.") };
			var collection = new HtmlAttributeCollection (items);

			Assert.That (collection.IndexOf (HtmlAttributeId.Alt), Is.EqualTo (0), "HtmlAttributeId.Alt");
			Assert.That (collection.IndexOf (HtmlAttributeId.Text), Is.EqualTo (1), "HtmlAttributeId.Text");
			Assert.That (collection.IndexOf (HtmlAttributeId.Background), Is.EqualTo (-1), "HtmlAttributeId.Background");

			Assert.That (collection.IndexOf ("alt"), Is.EqualTo (0), "alt");
			Assert.That (collection.IndexOf ("text"), Is.EqualTo (1), "text");
			Assert.That (collection.IndexOf ("background"), Is.EqualTo (-1), "background");
		}

		[Test]
		public void TestTryGetValue ()
		{
			var items = new HtmlAttribute[] { new HtmlAttribute (HtmlAttributeId.Alt, "This is some alt text."), new HtmlAttribute (HtmlAttributeId.Text, "And this is the text.") };
			var collection = new HtmlAttributeCollection (items);
			HtmlAttribute attr;

			Assert.That (collection.TryGetValue (HtmlAttributeId.Alt, out attr), Is.True, "HtmlAttributeId.Alt");
			Assert.That (attr.Value, Is.EqualTo ("This is some alt text."), "HtmlAttributeId.Alt Value");
			Assert.That (collection.TryGetValue (HtmlAttributeId.Text, out attr), Is.True, "HtmlAttributeId.Text");
			Assert.That (attr.Value, Is.EqualTo ("And this is the text."), "HtmlAttributeId.Text Value");
			Assert.That (collection.TryGetValue (HtmlAttributeId.Background, out attr), Is.False, "HtmlAttributeId.Background");
			Assert.That (attr, Is.Null, "HtmlAttributeId.Background is not null");

			Assert.That (collection.TryGetValue ("alt", out attr), Is.True, "alt");
			Assert.That (attr.Value, Is.EqualTo ("This is some alt text."), "alt Value");
			Assert.That (collection.TryGetValue ("text", out attr), Is.True, "text");
			Assert.That (attr.Value, Is.EqualTo ("And this is the text."), "text Value");
			Assert.That (collection.TryGetValue ("background", out attr), Is.False, "background");
			Assert.That (attr, Is.Null, "background is not null");
		}

		[Test]
		public void TestEnumerator ()
		{
			var items = new HtmlAttribute[] { new HtmlAttribute (HtmlAttributeId.Alt, "This is some alt text."), new HtmlAttribute (HtmlAttributeId.Text, "And this is the text.") };
			var collection = new HtmlAttributeCollection (items);
			int index = 0;

			foreach (var attr in collection) {
				Assert.That (attr, Is.EqualTo (items[index]), $"GetEnumerator<HtmlAttribute>() index = {index}");
				index++;
			}

			index = 0;
			foreach (HtmlAttribute item in ((IEnumerable) collection)) {
				Assert.That (item, Is.EqualTo (items[index]), $"GetEnumerator<HtmlAttribute>() index = {index}");
				index++;
			}
		}
	}
}

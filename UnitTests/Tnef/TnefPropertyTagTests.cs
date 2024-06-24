//
// TnefPropertyTagTests.cs
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

using System.Reflection;

using MimeKit.Tnef;

namespace UnitTests.Tnef {
	[TestFixture]
	public class TnefPropertyTagTests
	{
		[Test]
		public void TestBuiltIns ()
		{
			foreach (var field in typeof (TnefPropertyTag).GetFields (BindingFlags.Public | BindingFlags.Static)) {
				var propertyTag = (TnefPropertyTag) field.GetValue (null);
				int tagId = (int) propertyTag;

				Assert.That (propertyTag.IsTnefTypeValid, Is.True, $"{field.Name}.IsTnefTypeValid");

				var tag = new TnefPropertyTag (propertyTag.Id, propertyTag.TnefType);

				Assert.That (tag.Id, Is.EqualTo (propertyTag.Id), $"{field.Name}.Id #1");
				Assert.That (tag.TnefType, Is.EqualTo (propertyTag.TnefType), $"{field.Name}.TnefType #1");
				Assert.That (tag.IsNamed, Is.EqualTo (propertyTag.IsNamed), $"{field.Name}.IsNamed #1");
				Assert.That (tag.IsMultiValued, Is.EqualTo (propertyTag.IsMultiValued), $"{field.Name}.IsMultiValued #1");
				Assert.That (tag.ValueTnefType, Is.EqualTo (propertyTag.ValueTnefType), $"{field.Name}.ValueTnefType #1");

				Assert.That (tag.GetHashCode (), Is.EqualTo (propertyTag.GetHashCode ()), $"{field.Name}.GetHashCode #1");
				Assert.That (tag, Is.EqualTo (propertyTag), $"{field.Name}.Equals #1");
				Assert.That ((object) tag, Is.EqualTo ((object) propertyTag), $"{field.Name}.Equals(object) #1");
				Assert.That (tag == propertyTag, Is.True, $"{field.Name} == #1");
				Assert.That (tag != propertyTag, Is.False, $"{field.Name} != #1");

				tag = new TnefPropertyTag (tagId);

				Assert.That (tag.Id, Is.EqualTo (propertyTag.Id), $"{field.Name}.Id #2");
				Assert.That (tag.TnefType, Is.EqualTo (propertyTag.TnefType), $"{field.Name}.TnefType #2");
				Assert.That (tag.IsNamed, Is.EqualTo (propertyTag.IsNamed), $"{field.Name}.IsNamed #2");
				Assert.That (tag.IsMultiValued, Is.EqualTo (propertyTag.IsMultiValued), $"{field.Name}.IsMultiValued #2");
				Assert.That (tag.ValueTnefType, Is.EqualTo (propertyTag.ValueTnefType), $"{field.Name}.ValueTnefType #2");

				Assert.That (tag.GetHashCode (), Is.EqualTo (propertyTag.GetHashCode ()), $"{field.Name}.GetHashCode #2");
				Assert.That (tag, Is.EqualTo (propertyTag), $"{field.Name}.Equals #2");
				Assert.That ((object) tag, Is.EqualTo ((object) propertyTag), $"{field.Name}.Equals(object) #2");
				Assert.That (tag == propertyTag, Is.True, $"{field.Name} == #2");
				Assert.That (tag != propertyTag, Is.False, $"{field.Name} != #2");

				tag = (TnefPropertyTag) tagId;

				Assert.That (tag.Id, Is.EqualTo (propertyTag.Id), $"{field.Name}.Id #3");
				Assert.That (tag.TnefType, Is.EqualTo (propertyTag.TnefType), $"{field.Name}.TnefType #3");
				Assert.That (tag.IsNamed, Is.EqualTo (propertyTag.IsNamed), $"{field.Name}.IsNamed #3");
				Assert.That (tag.IsMultiValued, Is.EqualTo (propertyTag.IsMultiValued), $"{field.Name}.IsMultiValued #3");
				Assert.That (tag.ValueTnefType, Is.EqualTo (propertyTag.ValueTnefType), $"{field.Name}.ValueTnefType #3");

				Assert.That (tag.GetHashCode (), Is.EqualTo (propertyTag.GetHashCode ()), $"{field.Name}.GetHashCode #3");
				Assert.That (tag, Is.EqualTo (propertyTag), $"{field.Name}.Equals #3");
				Assert.That ((object) tag, Is.EqualTo ((object) propertyTag), $"{field.Name}.Equals(object) #3");
				Assert.That (tag == propertyTag, Is.True, $"{field.Name} == #3");
				Assert.That (tag != propertyTag, Is.False, $"{field.Name} != #3");
			}
		}

		[Test]
		public void TestToString ()
		{
			var value = TnefPropertyTag.NicknameA.ToString ();

			Assert.That (value, Is.EqualTo ("Nickname (String8)"));

			value = TnefPropertyTag.NicknameW.ToString ();

			Assert.That (value, Is.EqualTo ("Nickname (Unicode)"));
		}

		[Test]
		public void TestToUnicode ()
		{
			var unicode = TnefPropertyTag.NicknameA.ToUnicode ();

			Assert.That (unicode, Is.EqualTo (TnefPropertyTag.NicknameW));
		}
	}
}

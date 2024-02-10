//
// MimeIteratorTests.cs
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
	public class MimeIteratorTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var iter = new MimeIterator (new MimeMessage { Body = new TextPart ("plain") });

			Assert.Throws<ArgumentNullException> (() => new MimeIterator (null));
			Assert.Throws<InvalidOperationException> (() => { var x = iter.Depth; });
			Assert.Throws<InvalidOperationException> (() => { var x = iter.Current; });
			Assert.Throws<InvalidOperationException> (() => { var x = iter.Parent; });
			Assert.Throws<InvalidOperationException> (() => { var x = iter.PathSpecifier; });
			Assert.Throws<ArgumentNullException> (() => iter.MoveTo (null));
			Assert.Throws<ArgumentException> (() => iter.MoveTo (string.Empty));
			Assert.Throws<FormatException> (() => iter.MoveTo ("xyz"));
		}

		static MessagePart CreateImapExampleMessageRfc822 (List<MimeEntity> parents)
		{
			var message = new MimeMessage ();
			var mixed = new Multipart ("mixed");
			var rfc822 = new MessagePart { Message = message };

			parents.Add (rfc822);
			message.Body = mixed;

			parents.Add (mixed);
			mixed.Add (new TextPart ("plain"));

			parents.Add (mixed);
			mixed.Add (new MimePart ());

			return rfc822;
		}

		static MessagePart CreateImapExampleInnerMessageRfc822 (List<MimeEntity> parents)
		{
			var message = new MimeMessage ();
			var mixed = new Multipart ("mixed");
			var alternative = new MultipartAlternative ();
			var rfc822 = new MessagePart { Message = message };

			parents.Add (rfc822);
			message.Body = mixed;

			parents.Add (mixed);
			mixed.Add (new TextPart ("plain"));

			parents.Add (mixed);
			mixed.Add (alternative);

			parents.Add (alternative);
			alternative.Add (new TextPart ("plain"));

			parents.Add (alternative);
			alternative.Add (new TextPart ("richtext"));

			return rfc822;
		}

		static Multipart CreateImapExampleInnerMultipart (List<MimeEntity> parents)
		{
			var mixed = new Multipart ("mixed");

			parents.Add (mixed);
			mixed.Add (new MimePart ("image", "gif"));

			parents.Add (mixed);
			mixed.Add (CreateImapExampleInnerMessageRfc822 (parents));

			return mixed;
		}

		static MimeMessage CreateImapExampleMessage (List<MimeEntity> parents)
		{
			var message = new MimeMessage ();
			var mixed = new Multipart ("mixed");

			message.Body = mixed;

			parents.Add (mixed);
			mixed.Add (new TextPart ("plain"));

			parents.Add (mixed);
			mixed.Add (new MimePart ());

			parents.Add (mixed);
			mixed.Add (CreateImapExampleMessageRfc822 (parents));

			parents.Add (mixed);
			mixed.Add (CreateImapExampleInnerMultipart (parents));

			return message;
		}

		[Test]
		public void TestPathSpecifiers ()
		{
			var expectedTypes = new Type[] { typeof (Multipart), typeof (TextPart), typeof (MimePart), typeof (MessagePart), typeof (Multipart), typeof (TextPart), typeof (MimePart), typeof (Multipart), typeof (MimePart), typeof (MessagePart), typeof (Multipart), typeof (TextPart), typeof (MultipartAlternative), typeof (TextPart), typeof (TextPart) };
			var expectedPathSpecifiers = new string[] { "0", "1", "2", "3", "3.0", "3.1", "3.2", "4", "4.1", "4.2", "4.2.0", "4.2.1", "4.2.2", "4.2.2.1", "4.2.2.2" };
			var expectedDepths = new int[] { 0, 1, 1, 1, 2, 3, 3, 1, 2, 2, 3, 4, 4, 5, 5 };
			var expectedParents = new List<MimeEntity> { null };
			var message = CreateImapExampleMessage (expectedParents);
			var iter = new MimeIterator (message);
			int i = 0;

			Assert.That (iter.MoveNext (), Is.True, "Initialize");
			do {
				var current = iter.Current;
				var parent = iter.Parent;

				Assert.That (iter.Depth, Is.EqualTo (expectedDepths[i]), $"Depth #{i}");
				Assert.That (parent, Is.EqualTo (expectedParents[i]), $"Parent #{i}");
				Assert.That (current, Is.InstanceOf (expectedTypes[i]), $"Type #{i}");
				Assert.That (iter.PathSpecifier, Is.EqualTo (expectedPathSpecifiers[i]), $"PathSpecifier #{i}");
				i++;
			} while (iter.MoveNext ());

			Assert.That (i, Is.EqualTo (expectedTypes.Length));

			iter.Reset ();
			i = 0;

			Assert.That (iter.MoveNext (), Is.True, "Reset");
			do {
				var current = iter.Current;
				var parent = iter.Parent;

				Assert.That (iter.Depth, Is.EqualTo (expectedDepths[i]), $"Reset Depth #{i}");
				Assert.That (parent, Is.EqualTo (expectedParents[i]), $"Reset Parent #{i}");
				Assert.That (current, Is.InstanceOf (expectedTypes[i]), $"Reset Type #{i}");
				Assert.That (iter.PathSpecifier, Is.EqualTo (expectedPathSpecifiers[i]), $"Reset PathSpecifier #{i}");
				i++;
			} while (iter.MoveNext ());
		}

		[Test]
		public void TestMoveTo ()
		{
			var expectedTypes = new Type[] { typeof (Multipart), typeof (TextPart), typeof (MimePart), typeof (MessagePart), typeof (Multipart), typeof (TextPart), typeof (MimePart), typeof (Multipart), typeof (MimePart), typeof (MessagePart), typeof (Multipart), typeof (TextPart), typeof (MultipartAlternative), typeof (TextPart), typeof (TextPart) };
			var expectedPathSpecifiers = new List<string> { "0", "1", "2", "3", "3.0", "3.1", "3.2", "4", "4.1", "4.2", "4.2.0", "4.2.1", "4.2.2", "4.2.2.1", "4.2.2.2" };
			var paths = new string[] { "3.1", "3.2", "4", "4.2.1", "4.2.2.2", "4.2", "3.2" };
			var expectedDepths = new int[] { 0, 1, 1, 1, 2, 3, 3, 1, 2, 2, 3, 4, 4, 5, 5 };
			var expectedParents = new List<MimeEntity> { null };
			var message = CreateImapExampleMessage (expectedParents);
			var iter = new MimeIterator (message);

			foreach (var path in paths) {
				int i = expectedPathSpecifiers.IndexOf (path);

				Assert.That (iter.MoveTo (expectedPathSpecifiers[i]), Is.True, $"MoveTo {expectedPathSpecifiers[i]}");
				Assert.That (iter.PathSpecifier, Is.EqualTo (expectedPathSpecifiers[i]), $"PathSpecifier {expectedPathSpecifiers[i]}");
				Assert.That (iter.Parent, Is.EqualTo (expectedParents[i]), $"Parent {expectedPathSpecifiers[i]}");
				Assert.That (iter.Current, Is.InstanceOf (expectedTypes[i]), $"Type {expectedPathSpecifiers[i]}");
				Assert.That (iter.Depth, Is.EqualTo (expectedDepths[i]), $"Depth {expectedPathSpecifiers[i]}");
			}
		}
	}
}

//
// MimeIteratorTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
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
			var enumerator = (System.Collections.IEnumerator) iter;

			Assert.Throws<ArgumentNullException> (() => new MimeIterator (null));
			Assert.Throws<InvalidOperationException> (() => _ = iter.Depth);
			Assert.Throws<InvalidOperationException> (() => _ = iter.Current);
			Assert.Throws<InvalidOperationException> (() => _ = enumerator.Current);
			Assert.Throws<InvalidOperationException> (() => _ = iter.Parent);
			Assert.Throws<InvalidOperationException> (() => _ = iter.PathSpecifier);
			Assert.Throws<InvalidOperationException> (() => iter.SkipChildren ());
			Assert.Throws<InvalidOperationException> (() => _ = iter.Index);
			Assert.Throws<InvalidOperationException> (() => _ = iter.CurrentMessage);
			Assert.Throws<InvalidOperationException> (() => iter.GetAncestor (1));
			Assert.Throws<ArgumentNullException> (() => iter.MoveTo (null));
			Assert.Throws<ArgumentException> (() => iter.MoveTo (string.Empty));
			Assert.Throws<FormatException> (() => iter.MoveTo ("xyz"));
			Assert.Throws<FormatException> (() => iter.MoveTo ("-1"));
			Assert.Throws<FormatException> (() => iter.MoveTo ("1..2"));
			Assert.Throws<FormatException> (() => iter.MoveTo ("1."));

			Assert.That (iter.MoveNext (), Is.True);
			Assert.Throws<ArgumentOutOfRangeException> (() => iter.GetAncestor (0));
			Assert.Throws<ArgumentOutOfRangeException> (() => iter.GetAncestor (1));

			Assert.Throws<ArgumentNullException> (() => iter.Message.TryGetEntity (null, out _));
		}

		[Test]
		public void TestForEach ()
		{
			var message = CreateImapExampleMessage (new List<MimeEntity> ());
			var expected = new List<string> { "0", "1", "2", "3", "4", "4.1", "4.2" };
			var paths = new List<string> ();
			var iter = new MimeIterator (message);

			foreach (var entity in iter) {
				Assert.That (entity, Is.SameAs (iter.Current));
				paths.Add (iter.PathSpecifier);

				if (entity is MessagePart)
					iter.SkipChildren ();
			}

			Assert.That (paths, Is.EqualTo (expected));

			// foreach continues from the current position
			paths.Clear ();
			Assert.That (iter.MoveTo ("4.2.1"), Is.True);
			foreach (var entity in iter)
				paths.Add (iter.PathSpecifier);

			Assert.That (paths, Is.EqualTo (new[] { "4.2.2", "4.2.2.1", "4.2.2.2" }));
		}

		[Test]
		public void TestIndex ()
		{
			var message = CreateImapExampleMessage (new List<MimeEntity> ());
			var iter = new MimeIterator (message);
			var expected = new Dictionary<string, int> {
				{ "0", -1 }, { "1", 0 }, { "2", 1 }, { "3", 2 }, { "3.0", -1 }, { "3.1", 0 }, { "3.2", 1 },
				{ "4", 3 }, { "4.1", 0 }, { "4.2", 1 }, { "4.2.0", -1 }, { "4.2.1", 0 }, { "4.2.2", 1 },
				{ "4.2.2.1", 0 }, { "4.2.2.2", 1 }
			};
			int count = 0;

			while (iter.MoveNext ()) {
				Assert.That (iter.Index, Is.EqualTo (expected[iter.PathSpecifier]), iter.PathSpecifier);

				if (iter.Index != -1)
					Assert.That (((Multipart) iter.Parent)[iter.Index], Is.SameAs (iter.Current), iter.PathSpecifier);

				count++;
			}

			Assert.That (count, Is.EqualTo (expected.Count));

			// a single-part body of a message has no multipart parent
			iter = new MimeIterator (new MimeMessage { Body = new MessagePart { Message = new MimeMessage { Body = new TextPart ("plain") } } });
			Assert.That (iter.MoveTo ("1"), Is.True);
			Assert.That (iter.Index, Is.EqualTo (-1));
			Assert.That (iter.MoveTo ("1.1"), Is.True);
			Assert.That (iter.Index, Is.EqualTo (-1));
		}

		[Test]
		public void TestCurrentMessage ()
		{
			var message = CreateImapExampleMessage (new List<MimeEntity> ());
			var iter = new MimeIterator (message);
			var body = (Multipart) message.Body;
			var message3 = ((MessagePart) body[2]).Message;
			var message42 = ((MessagePart) ((Multipart) body[3])[1]).Message;

			var expected = new Dictionary<string, MimeMessage> {
				{ "0", message }, { "1", message }, { "2", message }, { "3", message },
				{ "3.0", message3 }, { "3.1", message3 }, { "3.2", message3 },
				{ "4", message }, { "4.1", message }, { "4.2", message },
				{ "4.2.0", message42 }, { "4.2.1", message42 }, { "4.2.2", message42 },
				{ "4.2.2.1", message42 }, { "4.2.2.2", message42 }
			};

			while (iter.MoveNext ())
				Assert.That (iter.CurrentMessage, Is.SameAs (expected[iter.PathSpecifier]), iter.PathSpecifier);
		}

		[Test]
		public void TestGetAncestor ()
		{
			var message = CreateImapExampleMessage (new List<MimeEntity> ());
			var iter = new MimeIterator (message);
			var body = (Multipart) message.Body;
			var part4 = (Multipart) body[3];
			var part42 = (MessagePart) part4[1];
			var part420 = (Multipart) part42.Message.Body;
			var part422 = (MultipartAlternative) part420[1];

			Assert.That (iter.MoveTo ("4.2.2.1"), Is.True);
			Assert.That (iter.Depth, Is.EqualTo (5));
			Assert.That (iter.GetAncestor (1), Is.SameAs (part422));
			Assert.That (iter.GetAncestor (1), Is.SameAs (iter.Parent));
			Assert.That (iter.GetAncestor (2), Is.SameAs (part420));
			Assert.That (iter.GetAncestor (3), Is.SameAs (part42));
			Assert.That (iter.GetAncestor (4), Is.SameAs (part4));
			Assert.That (iter.GetAncestor (5), Is.SameAs (body));
			Assert.Throws<ArgumentOutOfRangeException> (() => iter.GetAncestor (6));
		}

		[Test]
		public void TestTryGetEntity ()
		{
			var message = CreateImapExampleMessage (new List<MimeEntity> ());
			var iter = new MimeIterator (message);
			int count = 0;

			while (iter.MoveNext ()) {
				Assert.That (message.TryGetEntity (iter.PathSpecifier, out var entity), Is.True, iter.PathSpecifier);
				Assert.That (entity, Is.SameAs (iter.Current), iter.PathSpecifier);
				count++;
			}

			Assert.That (count, Is.EqualTo (15));

			foreach (var path in new[] { "", "xyz", "-1", "1..2", "1.", "5", "0.1", "4.0", "3.3", "1.1" }) {
				Assert.That (message.TryGetEntity (path, out var entity), Is.False, path);
				Assert.That (entity, Is.Null, path);
			}

			Assert.That (new MimeMessage ().TryGetEntity ("1", out var none), Is.False);
			Assert.That (none, Is.Null);
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

		[Test]
		public void TestMoveNextSkipsMessagePartWithoutBody ()
		{
			var message = new MimeMessage {
				Body = new Multipart ("mixed") {
					new MessagePart { Message = new MimeMessage () },
					new TextPart ("plain") { Text = "plain" }
				}
			};
			var iter = new MimeIterator (message);

			Assert.That (iter.MoveNext (), Is.True);
			Assert.That (iter.PathSpecifier, Is.EqualTo ("0"));
			Assert.That (iter.Current, Is.InstanceOf<Multipart> ());
			Assert.That (iter.MoveNext (), Is.True);
			Assert.That (iter.PathSpecifier, Is.EqualTo ("1"));
			Assert.That (iter.Current, Is.InstanceOf<MessagePart> ());
			Assert.That (iter.MoveNext (), Is.True);
			Assert.That (iter.PathSpecifier, Is.EqualTo ("2"));
			Assert.That (iter.Current, Is.InstanceOf<TextPart> ());
			Assert.That (iter.MoveNext (), Is.False);
		}

		[Test]
		public void TestMoveToEmptyMessageReturnsFalse ()
		{
			var iter = new MimeIterator (new MimeMessage ());

			Assert.That (iter.MoveTo ("1"), Is.False);
		}

		[Test]
		public void TestMoveToMissingPathReturnsFalse ()
		{
			var message = new MimeMessage {
				Body = new Multipart ("mixed") {
					new TextPart ("plain") { Text = "plain" }
				}
			};
			var iter = new MimeIterator (message);

			Assert.That (iter.MoveTo ("2"), Is.False);
			Assert.Throws<InvalidOperationException> (() => _ = iter.Current);
			Assert.That (iter.MoveNext (), Is.False);
		}

		[Test]
		public void TestMoveToInvalidPaths ()
		{
			var message = CreateImapExampleMessage (new List<MimeEntity> ());
			var iter = new MimeIterator (message);

			// "0" may only refer to a multipart that is a message body
			Assert.That (iter.MoveTo ("0.1"), Is.False, "0.1");
			Assert.That (iter.MoveTo ("4.0"), Is.False, "4.0");
			Assert.That (iter.MoveTo ("3.0.1"), Is.False, "3.0.1");

			// out of range
			Assert.That (iter.MoveTo ("5"), Is.False, "5");
			Assert.That (iter.MoveTo ("3.3"), Is.False, "3.3");
			Assert.That (iter.MoveTo ("4.2.2.3"), Is.False, "4.2.2.3");

			// leaves have no children
			Assert.That (iter.MoveTo ("1.1"), Is.False, "1.1");
			Assert.That (iter.MoveTo ("4.2.2.1.1"), Is.False, "4.2.2.1.1");
		}

		[Test]
		public void TestMoveToThenMoveNext ()
		{
			var message = CreateImapExampleMessage (new List<MimeEntity> ());
			var iter = new MimeIterator (message);

			Assert.That (iter.MoveTo ("4.2.2.2"), Is.True);
			Assert.That (iter.MoveTo ("3.1"), Is.True, "Move backwards");
			Assert.That (iter.PathSpecifier, Is.EqualTo ("3.1"));

			Assert.That (iter.MoveNext (), Is.True);
			Assert.That (iter.PathSpecifier, Is.EqualTo ("3.2"));
			Assert.That (iter.Depth, Is.EqualTo (3));

			Assert.That (iter.MoveNext (), Is.True);
			Assert.That (iter.PathSpecifier, Is.EqualTo ("4"));
			Assert.That (iter.Depth, Is.EqualTo (1));
			Assert.That (iter.Parent, Is.SameAs (message.Body));
		}

		[Test]
		public void TestSinglePartPathSpecifiers ()
		{
			var message = new MimeMessage { Body = new TextPart ("plain") { Text = "plain" } };
			var iter = new MimeIterator (message);

			Assert.That (iter.MoveNext (), Is.True);
			Assert.That (iter.PathSpecifier, Is.EqualTo ("1"));
			Assert.That (iter.Depth, Is.EqualTo (0));
			Assert.That (iter.Parent, Is.Null);
			Assert.That (iter.MoveNext (), Is.False);

			Assert.That (iter.MoveTo ("1"), Is.True);
			Assert.That (iter.Current, Is.SameAs (message.Body));
			Assert.That (iter.MoveTo ("0"), Is.False);
			Assert.That (iter.MoveTo ("2"), Is.False);
		}

		[Test]
		public void TestMessagePartPathSpecifiers ()
		{
			var inner = new TextPart ("plain") { Text = "inner" };
			var rfc822 = new MessagePart { Message = new MimeMessage { Body = inner } };
			var message = new MimeMessage { Body = rfc822 };
			var iter = new MimeIterator (message);

			// a top-level message/rfc822 body is part 1 and its single-part body is part 1.1
			Assert.That (iter.MoveNext (), Is.True);
			Assert.That (iter.PathSpecifier, Is.EqualTo ("1"));
			Assert.That (iter.Current, Is.SameAs (rfc822));
			Assert.That (iter.MoveNext (), Is.True);
			Assert.That (iter.PathSpecifier, Is.EqualTo ("1.1"));
			Assert.That (iter.Current, Is.SameAs (inner));
			Assert.That (iter.Parent, Is.SameAs (rfc822));
			Assert.That (iter.Depth, Is.EqualTo (1));
			Assert.That (iter.MoveNext (), Is.False);

			Assert.That (iter.MoveTo ("1.1"), Is.True);
			Assert.That (iter.Current, Is.SameAs (inner));
			Assert.That (iter.MoveTo ("1"), Is.True);
			Assert.That (iter.Current, Is.SameAs (rfc822));
			Assert.That (iter.MoveTo ("1.0"), Is.False);
			Assert.That (iter.MoveTo ("1.2"), Is.False);
		}

		[Test]
		public void TestSkipChildren ()
		{
			var message = CreateImapExampleMessage (new List<MimeEntity> ());
			var iter = new MimeIterator (message);

			// skip the attached message at part 3
			Assert.That (iter.MoveTo ("3"), Is.True);
			iter.SkipChildren ();
			Assert.That (iter.MoveNext (), Is.True);
			Assert.That (iter.PathSpecifier, Is.EqualTo ("4"));

			// skipping a leaf has no effect
			Assert.That (iter.MoveTo ("1"), Is.True);
			iter.SkipChildren ();
			Assert.That (iter.MoveNext (), Is.True);
			Assert.That (iter.PathSpecifier, Is.EqualTo ("2"));

			// skipping the multipart at part 4 (the last part) ends the iteration
			Assert.That (iter.MoveTo ("4"), Is.True);
			iter.SkipChildren ();
			Assert.That (iter.MoveNext (), Is.False);

			// skipping the last child of a nested multipart ends the iteration
			Assert.That (iter.MoveTo ("4.2.2"), Is.True);
			iter.SkipChildren ();
			Assert.That (iter.MoveNext (), Is.False);

			// skipping a multipart message body continues with the next sibling of its message/rfc822 part
			Assert.That (iter.MoveTo ("3.0"), Is.True);
			iter.SkipChildren ();
			Assert.That (iter.MoveNext (), Is.True);
			Assert.That (iter.PathSpecifier, Is.EqualTo ("4"));

			// skipping the top-level multipart ends the iteration
			iter.Reset ();
			Assert.That (iter.MoveNext (), Is.True);
			iter.SkipChildren ();
			Assert.That (iter.MoveNext (), Is.False);

			// Reset and MoveTo cancel a pending skip
			iter.Reset ();
			Assert.That (iter.MoveNext (), Is.True);
			iter.SkipChildren ();
			iter.Reset ();
			Assert.That (iter.MoveNext (), Is.True);
			Assert.That (iter.MoveNext (), Is.True);
			Assert.That (iter.PathSpecifier, Is.EqualTo ("1"));

			Assert.That (iter.MoveTo ("4"), Is.True);
			iter.SkipChildren ();
			Assert.That (iter.MoveTo ("4"), Is.True);
			Assert.That (iter.MoveNext (), Is.True);
			Assert.That (iter.PathSpecifier, Is.EqualTo ("4.1"));
		}

		[Test]
		public void TestDeeplyNestedMultiparts ()
		{
			const int depth = 10000;
			var leaf = new TextPart ("plain") { Text = "leaf" };
			var root = new Multipart ("mixed");
			var multipart = root;

			for (int i = 1; i < depth; i++) {
				var nested = new Multipart ("mixed");
				multipart.Add (nested);
				multipart = nested;
			}

			multipart.Add (leaf);

			var message = new MimeMessage { Body = root };
			var bodyParts = message.BodyParts.ToList ();

			Assert.That (bodyParts, Has.Count.EqualTo (1));
			Assert.That (bodyParts[0], Is.SameAs (leaf));

			var iter = new MimeIterator (message);
			var path = "0." + string.Join (".", Enumerable.Repeat ("1", depth));
			int count = 0;

			while (iter.MoveNext ())
				count++;

			Assert.That (count, Is.EqualTo (depth + 1));

			Assert.That (iter.MoveTo (path), Is.False, "0 must be the last component");
			Assert.That (iter.MoveTo (path.Substring (2)), Is.True);
			Assert.That (iter.Current, Is.SameAs (leaf));
			Assert.That (iter.Depth, Is.EqualTo (depth));
		}

		[Test]
		public void TestBodyPartsOrder ()
		{
			var message = CreateImapExampleMessage (new List<MimeEntity> ());
			var iter = new MimeIterator (message);
			var expected = new List<MimeEntity> ();

			// BodyParts yields the non-multipart entities of the top-level message without descending into message/rfc822 parts
			foreach (var path in new[] { "1", "2", "3", "4.1", "4.2" }) {
				Assert.That (iter.MoveTo (path), Is.True, path);
				expected.Add (iter.Current);
			}

			Assert.That (message.BodyParts.ToList (), Is.EqualTo (expected));
			Assert.That (message.Attachments.ToList (), Is.EqualTo (expected.Where (x => x.IsAttachment).ToList ()));
			Assert.That (new MimeMessage ().BodyParts, Is.Empty);
		}
	}
}

//
// MimeEntityTests.cs
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

using System.Text;

using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class MimeEntityTests
	{
		class TestMimeEntity : MimeEntity
		{
			public TestMimeEntity () : base ("application", "x-test")
			{
			}

			public void RaiseHeadersChanged (HeaderListChangedAction action, Header header)
			{
				OnHeadersChanged (action, header);
			}

			public override void Prepare (EncodingConstraint constraint, int maxLineLength = 78)
			{
			}
		}

		class MimeEntityVisitor : MimeVisitor
		{
			public int Visits;

			protected internal override void VisitMimeEntity (MimeEntity entity)
			{
				Visits++;
				base.VisitMimeEntity (entity);
			}
		}

		[Test]
		public void TestSettingSameLazyPropertiesDoesNotRewriteHeaders ()
		{
			using var part = new MimePart ("text", "plain");
			var disposition = new ContentDisposition (ContentDisposition.Attachment);
			var contentBase = new Uri ("http://example.com/base/");
			var contentLocation = new Uri ("relative.txt", UriKind.Relative);

			part.ContentDisposition = disposition;
			var contentDispositionHeader = part.Headers[HeaderId.ContentDisposition];
			part.ContentDisposition = disposition;
			Assert.That (part.Headers[HeaderId.ContentDisposition], Is.EqualTo (contentDispositionHeader), "ContentDisposition");

			part.ContentBase = contentBase;
			var contentBaseHeader = part.Headers[HeaderId.ContentBase];
			part.ContentBase = contentBase;
			Assert.That (part.Headers[HeaderId.ContentBase], Is.EqualTo (contentBaseHeader), "ContentBase");

			part.ContentLocation = contentLocation;
			var contentLocationHeader = part.Headers[HeaderId.ContentLocation];
			part.ContentLocation = contentLocation;
			Assert.That (part.Headers[HeaderId.ContentLocation], Is.EqualTo (contentLocationHeader), "ContentLocation");

			part.ContentId = "content-id@example.com";
			var contentIdHeader = part.Headers[HeaderId.ContentId];
			part.ContentId = "content-id@example.com";
			Assert.That (part.Headers[HeaderId.ContentId], Is.EqualTo (contentIdHeader), "ContentId");
		}

		[Test]
		public void TestBaseAccept ()
		{
			using var entity = new TestMimeEntity ();
			var visitor = new MimeEntityVisitor ();

			entity.Accept (visitor);

			Assert.That (visitor.Visits, Is.EqualTo (1));
		}

		[Test]
		public void TestOnHeadersChangedRequiresHeaderExceptWhenCleared ()
		{
			using var entity = new TestMimeEntity ();

			Assert.Throws<ArgumentNullException> (() => entity.RaiseHeadersChanged (HeaderListChangedAction.Added, null));
			Assert.DoesNotThrow (() => entity.RaiseHeadersChanged (HeaderListChangedAction.Cleared, null));
		}

		[Test]
		public async Task TestFileOverloads ()
		{
			var path = Path.Combine (TestContext.CurrentContext.WorkDirectory, "mime-entity-file-overload.txt");
			var contentOnlyPath = Path.Combine (TestContext.CurrentContext.WorkDirectory, "mime-entity-content-only-file-overload.txt");
			var newLine = FormatOptions.Default.NewLineFormat == NewLineFormat.Dos ? "\r\n" : "\n";
			var expected = $"Content-Type: text/plain; charset=utf-8{newLine}{newLine}This is the body.";

			try {
				using (var part = new TextPart ("plain") { Text = "This is the body." }) {
					using (var memory = new MemoryStream ()) {
						part.WriteTo (memory, true);
						Assert.That (Encoding.UTF8.GetString (memory.ToArray ()), Is.EqualTo ("This is the body."), "WriteTo stream contentOnly");
					}

					part.WriteTo (path);
					part.WriteTo (contentOnlyPath, true);
				}

				var actual = File.ReadAllText (path, Encoding.UTF8);
				Assert.That (actual, Is.EqualTo (expected), "WriteTo");
				Assert.That (File.ReadAllText (contentOnlyPath, Encoding.UTF8), Is.EqualTo ("This is the body."), "WriteTo contentOnly");

				using (var entity = await MimeEntity.LoadAsync (ParserOptions.Default, path)) {
					Assert.That (entity, Is.InstanceOf<TextPart> (), "Loaded entity");
					Assert.That (((TextPart) entity).Text, Is.EqualTo ("This is the body."), "Loaded text");
				}
			} finally {
				if (File.Exists (path))
					File.Delete (path);

				if (File.Exists (contentOnlyPath))
					File.Delete (contentOnlyPath);
			}
		}
	}
}

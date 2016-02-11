//
// MultipartTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2016 Xamarin Inc. (www.xamarin.com)
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
using System.IO;

using NUnit.Framework;

using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class MultipartTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var multipart = new Multipart ();

			Assert.Throws<ArgumentNullException> (() => new Multipart ((string) null));
			Assert.Throws<ArgumentNullException> (() => new Multipart ("mixed", null));
			Assert.Throws<ArgumentException> (() => new Multipart ("mixed", 5));

			Assert.Throws<ArgumentNullException> (() => multipart.Boundary = null);

			Assert.Throws<ArgumentNullException> (() => multipart.Add (null));
			Assert.Throws<ArgumentOutOfRangeException> (() => multipart.Insert (-1, new TextPart ("plain")));
			Assert.Throws<ArgumentNullException> (() => multipart.Insert (0, null));
			Assert.Throws<ArgumentNullException> (() => multipart.Remove (null));
			Assert.Throws<ArgumentOutOfRangeException> (() => multipart.RemoveAt (-1));

			Assert.Throws<ArgumentNullException> (() => multipart.Contains (null));
			Assert.Throws<ArgumentNullException> (() => multipart.IndexOf (null));

			Assert.Throws<ArgumentOutOfRangeException> (() => multipart[0] = new TextPart ("plain"));
			Assert.Throws<ArgumentNullException> (() => multipart[0] = null);

			Assert.Throws<ArgumentNullException> (() => multipart.Accept (null));

			Assert.Throws<ArgumentOutOfRangeException> (() => multipart.CopyTo (new MimeEntity[0], -1));
			Assert.Throws<ArgumentNullException> (() => multipart.CopyTo (null, 0));
		}

		[Test]
		public void TestBasicFunctionality ()
		{
			var multipart = new Multipart ();

			Assert.IsNotNullOrEmpty (multipart.Boundary, "Boundary");
			Assert.IsFalse (multipart.IsReadOnly, "IsReadOnly");

			multipart.Boundary = "__Next_Part_123";

			Assert.AreEqual ("__Next_Part_123", multipart.Boundary);

			var generic = new MimePart ("application", "octet-stream") { ContentObject = new ContentObject (new MemoryStream ()), IsAttachment = true };
			var plain = new TextPart ("plain") { Text = "This is some plain text." };

			multipart.Add (generic);
			multipart.Insert (0, plain);

			Assert.AreEqual (2, multipart.Count, "Count");

			Assert.IsTrue (multipart.Contains (generic), "Contains");
			Assert.AreEqual (0, multipart.IndexOf (plain), "IndexOf");
			Assert.IsTrue (multipart.Remove (generic), "Remove");
			Assert.IsFalse (multipart.Remove (generic), "Remove 2nd time");

			multipart.RemoveAt (0);

			Assert.AreEqual (0, multipart.Count, "Count");

			multipart.Add (generic);
			multipart.Add (plain);

			Assert.AreEqual (generic, multipart[0]);
			Assert.AreEqual (plain, multipart[1]);

			multipart[0] = plain;
			multipart[1] = generic;

			Assert.AreEqual (plain, multipart[0]);
			Assert.AreEqual (generic, multipart[1]);

			multipart.Clear ();

			Assert.AreEqual (0, multipart.Count, "Count");
		}
	}
}

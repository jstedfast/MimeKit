//
// ParserOptionsTests.cs
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
using System.IO;
using System.Threading.Tasks;

using NUnit.Framework;

using MimeKit;

namespace UnitTests
{
	class BrokenTextHtmlPart : TextPart
	{
		public BrokenTextHtmlPart () : base ("html")
		{
		}
	}

	class CustomTextHtmlPart : TextPart
	{
		public CustomTextHtmlPart (MimeEntityConstructorArgs args) : base (args)
		{
		}

		public CustomTextHtmlPart () : base ("html")
		{
		}
	}

	[TestFixture]
	public class ParserOptionsTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var options = ParserOptions.Default.Clone ();

			Assert.Throws<ArgumentNullException> (() => options.RegisterMimeType (null, typeof (CustomTextHtmlPart)));
			Assert.Throws<ArgumentNullException> (() => options.RegisterMimeType ("text/html", null));
			Assert.Throws<ArgumentException> (() => options.RegisterMimeType ("text/html", typeof (string)));
			Assert.Throws<ArgumentException> (() => options.RegisterMimeType ("text/html", typeof (BrokenTextHtmlPart)));
		}

		[Test]
		public void TestParsingOfCustomType ()
		{
			var options = ParserOptions.Default.Clone ();

			options.RegisterMimeType ("text/html", typeof (CustomTextHtmlPart));

			using (var stream = new MemoryStream ()) {
				var text = new TextPart ("html") { Text = "<html>this is some html and stuff</html>" };

				text.WriteTo (stream);
				stream.Position = 0;

				var html = MimeEntity.Load (options, stream);

				Assert.IsInstanceOf<CustomTextHtmlPart> (html, "Expected the text/html part to use our custom type.");
			}
		}

		[Test]
		public async Task TestParsingOfCustomTypeAsync ()
		{
			var options = ParserOptions.Default.Clone ();

			options.RegisterMimeType ("text/html", typeof (CustomTextHtmlPart));

			using (var stream = new MemoryStream ()) {
				var text = new TextPart ("html") { Text = "<html>this is some html and stuff</html>" };

				text.WriteTo (stream);
				stream.Position = 0;

				var html = await MimeEntity.LoadAsync (options, stream);

				Assert.IsInstanceOf<CustomTextHtmlPart> (html, "Expected the text/html part to use our custom type.");
			}
		}
	}
}

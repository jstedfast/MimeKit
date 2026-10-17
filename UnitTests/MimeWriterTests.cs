//
// MimeWriterTests.cs
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
	public class MimeWriterTests
	{
		const string InnermostContent = "This is the innermost part.";

		public enum Nesting
		{
			Multipart,
			MessagePart,
			Mixed
		}

		static bool IsMultipart (Nesting nesting, int level)
		{
			return nesting == Nesting.Multipart || (nesting == Nesting.Mixed && (level % 2) == 0);
		}

		internal static byte[] GenerateExtremelyNestedMessage (Nesting nesting, int depth)
		{
			var builder = new StringBuilder ();

			builder.Append ("From: mimekit@example.org\r\n");

			for (int i = 0; i < depth; i++) {
				if (IsMultipart (nesting, i))
					builder.Append ($"Content-Type: multipart/mixed; boundary=\"{i}\"\r\n\r\n--{i}\r\n");
				else
					builder.Append ("Content-Type: message/rfc822\r\n\r\n");
			}

			builder.Append ("Content-Type: text/plain\r\n\r\n");
			builder.Append (InnermostContent);

			for (int i = depth - 1; i >= 0; i--) {
				if (IsMultipart (nesting, i))
					builder.Append ($"\r\n--{i}--");
			}

			builder.Append ("\r\n");

			return Encoding.ASCII.GetBytes (builder.ToString ());
		}

		internal static MimeMessage ParseExtremelyNestedMessage (byte[] source)
		{
			var options = new ParserOptions { MaxMimeDepth = int.MaxValue };

			using (var stream = new MemoryStream (source, false)) {
				var parser = new MimeParser (options, stream, MimeFormat.Entity);

				return parser.ParseMessage ();
			}
		}

		static FormatOptions CreateFormatOptions ()
		{
			return new FormatOptions { NewLineFormat = NewLineFormat.Dos };
		}

		// Note: MimeMessage.WriteTo() and MimeEntity.WriteTo() must not use recursion to write nested entities, or else
		// an extremely deep nesting of multipart or message/rfc822 parts could overflow the stack (which cannot be caught
		// and terminates the process).
		[TestCase (Nesting.Multipart, 10000)]
		[TestCase (Nesting.MessagePart, 100000)]
		[TestCase (Nesting.Mixed, 20000)]
		public void TestWriteExtremelyNestedMessage (Nesting nesting, int depth)
		{
			var source = GenerateExtremelyNestedMessage (nesting, depth);
			using var message = ParseExtremelyNestedMessage (source);
			var options = CreateFormatOptions ();

			using (var stream = new MemoryStream ()) {
				message.WriteTo (options, stream);

				Assert.That (Encoding.ASCII.GetString (stream.ToArray ()), Is.EqualTo (Encoding.ASCII.GetString (source)));
			}
		}

		[TestCase (Nesting.Multipart, 10000)]
		[TestCase (Nesting.MessagePart, 100000)]
		[TestCase (Nesting.Mixed, 20000)]
		public async Task TestWriteExtremelyNestedMessageAsync (Nesting nesting, int depth)
		{
			var source = GenerateExtremelyNestedMessage (nesting, depth);
			using var message = ParseExtremelyNestedMessage (source);
			var options = CreateFormatOptions ();

			using (var stream = new MemoryStream ()) {
				await message.WriteToAsync (options, stream);

				Assert.That (Encoding.ASCII.GetString (stream.ToArray ()), Is.EqualTo (Encoding.ASCII.GetString (source)));
			}
		}

		[TestCase (Nesting.Multipart, 10000)]
		[TestCase (Nesting.MessagePart, 100000)]
		[TestCase (Nesting.Mixed, 20000)]
		public void TestWriteExtremelyNestedEntity (Nesting nesting, int depth)
		{
			var source = GenerateExtremelyNestedMessage (nesting, depth);
			using var message = ParseExtremelyNestedMessage (source);
			var options = CreateFormatOptions ();

			// Note: Skip the "From:" header which belongs to the top-level message rather than to its body.
			var expected = Encoding.ASCII.GetString (source).Substring ("From: mimekit@example.org\r\n".Length);

			using (var stream = new MemoryStream ()) {
				message.Body.WriteTo (options, stream);

				Assert.That (Encoding.ASCII.GetString (stream.ToArray ()), Is.EqualTo (expected));
			}
		}

		[TestCase (Nesting.Multipart, 10000)]
		[TestCase (Nesting.MessagePart, 100000)]
		[TestCase (Nesting.Mixed, 20000)]
		public async Task TestWriteExtremelyNestedEntityAsync (Nesting nesting, int depth)
		{
			var source = GenerateExtremelyNestedMessage (nesting, depth);
			using var message = ParseExtremelyNestedMessage (source);
			var options = CreateFormatOptions ();

			// Note: Skip the "From:" header which belongs to the top-level message rather than to its body.
			var expected = Encoding.ASCII.GetString (source).Substring ("From: mimekit@example.org\r\n".Length);

			using (var stream = new MemoryStream ()) {
				await message.Body.WriteToAsync (options, stream);

				Assert.That (Encoding.ASCII.GetString (stream.ToArray ()), Is.EqualTo (expected));
			}
		}
	}
}
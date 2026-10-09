//
// MultipartTests.cs
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
using MimeKit.Text;

using System.Collections;
using System.Text;

namespace UnitTests {
	[TestFixture]
	public class MultipartTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var multipart = new Multipart ();

			Assert.Throws<ArgumentNullException> (() => new Multipart ((MimeEntityConstructorArgs) null));
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

			Assert.Throws<ArgumentOutOfRangeException> (() => multipart.CopyTo (Array.Empty<MimeEntity> (), -1));
			Assert.Throws<ArgumentNullException> (() => multipart.CopyTo (null, 0));

			Assert.Throws<ArgumentOutOfRangeException> (() => multipart.Prepare (EncodingConstraint.SevenBit, 1));

			Assert.Throws<ArgumentNullException> (() => multipart.WriteTo ((string) null));
			Assert.Throws<ArgumentNullException> (() => multipart.WriteTo ((Stream) null));
			Assert.Throws<ArgumentNullException> (() => multipart.WriteTo ((string) null, false));
			Assert.Throws<ArgumentNullException> (() => multipart.WriteTo ((Stream) null, false));
			Assert.Throws<ArgumentNullException> (() => multipart.WriteTo (null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => multipart.WriteTo (FormatOptions.Default, (Stream) null));
			Assert.Throws<ArgumentNullException> (() => multipart.WriteTo (null, "fileName"));
			Assert.Throws<ArgumentNullException> (() => multipart.WriteTo (FormatOptions.Default, (string) null));
			Assert.Throws<ArgumentNullException> (() => multipart.WriteTo (null, Stream.Null, false));
			Assert.Throws<ArgumentNullException> (() => multipart.WriteTo (FormatOptions.Default, (Stream) null, false));
			Assert.Throws<ArgumentNullException> (() => multipart.WriteTo (null, "fileName", false));
			Assert.Throws<ArgumentNullException> (() => multipart.WriteTo (FormatOptions.Default, (string) null, false));

			Assert.ThrowsAsync<ArgumentNullException> (async () => await multipart.WriteToAsync ((string) null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await multipart.WriteToAsync ((Stream) null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await multipart.WriteToAsync ((string) null, false));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await multipart.WriteToAsync ((Stream) null, false));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await multipart.WriteToAsync (null, Stream.Null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await multipart.WriteToAsync (FormatOptions.Default, (Stream) null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await multipart.WriteToAsync (null, "fileName"));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await multipart.WriteToAsync (FormatOptions.Default, (string) null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await multipart.WriteToAsync (null, Stream.Null, false));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await multipart.WriteToAsync (FormatOptions.Default, (Stream) null, false));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await multipart.WriteToAsync (null, "fileName", false));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await multipart.WriteToAsync (FormatOptions.Default, (string) null, false));
		}

		[Test]
		public void TestBasicFunctionality ()
		{
			var multipart = new Multipart ();

			Assert.That (multipart.Boundary, Is.Not.Null, "Boundary != null");
			Assert.That (multipart.Boundary, Is.Not.Empty, "Boundary");
			Assert.That (multipart.IsReadOnly, Is.False, "IsReadOnly");

			multipart.Boundary = "__Next_Part_123";

			Assert.That (multipart.Boundary, Is.EqualTo ("__Next_Part_123"));

			var generic = new MimePart ("application", "octet-stream") { Content = new MimeContent (new MemoryStream ()), IsAttachment = true };
			var plain = new TextPart ("plain") { Text = "This is some plain text." };

			multipart.Add (generic);
			multipart.Insert (0, plain);

			Assert.That (multipart.Count, Is.EqualTo (2), "Count");

			Assert.That (multipart.Contains (generic), Is.True, "Contains");
			Assert.That (multipart.IndexOf (plain), Is.EqualTo (0), "IndexOf");

			var copied = new MimeEntity[2];
			multipart.CopyTo (copied, 0);
			Assert.That (copied.Contains (generic), Is.True, "CopyTo Contains");
			Assert.That (copied[0], Is.EqualTo (plain), "CopyTo [0]");
			Assert.That (copied[1], Is.EqualTo (generic), "CopyTo [1]");

			Assert.That (multipart.Remove (generic), Is.True, "Remove");
			Assert.That (multipart.Remove (generic), Is.False, "Remove 2nd time");

			multipart.RemoveAt (0);

			Assert.That (multipart.Count, Is.EqualTo (0), "Count");

			multipart.Add (generic);
			multipart.Add (plain);

			Assert.That (multipart[0], Is.EqualTo (generic));
			Assert.That (multipart[1], Is.EqualTo (plain));

			multipart[0] = plain;
			multipart[1] = generic;

			Assert.That (multipart[0], Is.EqualTo (plain));
			Assert.That (multipart[1], Is.EqualTo (generic));

			multipart.Clear ();

			Assert.That (multipart.Count, Is.EqualTo (0), "Count");

			multipart.Add (plain);
			multipart.Add (generic);

			// Clear & dispose the MimeParts
			multipart.Clear (true);

			Assert.That (plain.IsDisposed, Is.True, "Expected plain part to be disposed after Clear(true)");
			Assert.That (generic.IsDisposed, Is.True, "Expected generic part to be disposed after Clear(true)");
		}

		[Test]
		public void TestDispose ()
		{
			var multipart = new Multipart {
				Boundary = "__Next_Part_123"
			};

			var generic = new MimePart ("application", "octet-stream") { Content = new MimeContent (new MemoryStream ()), IsAttachment = true };
			var rfc822 = new MessagePart ("rfc822") {
				Message = new MimeMessage () {
					Body = new TextPart ("plain") {
						Text = "This is the inner message body."
					}
				}
			};
			var plain = new TextPart ("plain") { Text = "This is some plain text." };

			multipart.Add (plain);
			multipart.Add (generic);
			multipart.Add (rfc822);

			multipart.Dispose ();

			Assert.That (multipart.IsDisposed, Is.True, "Expected multipart to be disposed after Dispose()");
			Assert.That (plain.IsDisposed, Is.True, "Expected plain part to be disposed after Dispose()");
			Assert.That (generic.IsDisposed, Is.True, "Expected generic part to be disposed after Dispose()");
			Assert.That (rfc822.IsDisposed, Is.True, "Expected rfc822 part to be disposed after Dispose()");
			Assert.That (rfc822.Message.Body.IsDisposed, Is.True, "Expected rfc822 message body to be disposed after Dispose()");
		}

		[Test]
		public void TestMultiLinePreamble ()
		{
			var multipart = new Multipart ("alternative");
			const string multiline = "This is a part in a (multipart) message generated with the MimeKit library.\n\n" + 
				"All of the parts of this message are identical, however they've been encoded " +
				"for transport using different methods.\n";
			var expected = "This is a part in a (multipart) message generated with the MimeKit library.\n\n" +
				"All of the parts of this message are identical, however they've been encoded\n" +
				"for transport using different methods.\n";

			if (FormatOptions.Default.NewLineFormat != NewLineFormat.Unix)
				expected = expected.Replace ("\n", "\r\n");

			multipart.Preamble = multiline;

			Assert.That (multipart.Preamble, Is.EqualTo (expected));

			multipart.Preamble = null;

			Assert.That (multipart.Preamble, Is.Null);
		}

		[Test]
		public void TestLongPreamble ()
		{
			var multipart = new Multipart ("alternative");
			const string multiline = "This is a part in a (multipart) message generated with the MimeKit library. " + 
				"All of the parts of this message are identical, however they've been encoded " +
				"for transport using different methods.";
			var expected = "This is a part in a (multipart) message generated with the MimeKit library.\n" +
				"All of the parts of this message are identical, however they've been encoded\n" +
				"for transport using different methods.\n";

			if (FormatOptions.Default.NewLineFormat != NewLineFormat.Unix)
				expected = expected.Replace ("\n", "\r\n");

			multipart.Preamble = multiline;

			Assert.That (multipart.Preamble, Is.EqualTo (expected));

			multipart.Preamble = null;

			Assert.That (multipart.Preamble, Is.Null);
		}

		[Test]
		public void TestMultiLineEpilogue ()
		{
			var multipart = new Multipart ("alternative");
			const string multiline = "This is a part in a (multipart) message generated with the MimeKit library.\n\n" + 
				"All of the parts of this message are identical, however they've been encoded " +
				"for transport using different methods.\n";
			var expected = "This is a part in a (multipart) message generated with the MimeKit library.\n\n" +
				"All of the parts of this message are identical, however they've been encoded\n" +
				"for transport using different methods.\n";

			if (FormatOptions.Default.NewLineFormat != NewLineFormat.Unix)
				expected = expected.Replace ("\n", "\r\n");

			multipart.Epilogue = multiline;

			Assert.That (multipart.Epilogue, Is.EqualTo (expected));

			multipart.Epilogue = null;

			Assert.That (multipart.Epilogue, Is.Null);
		}

		[Test]
		public void TestLongEpilogue ()
		{
			var multipart = new Multipart ("alternative");
			const string multiline = "This is a part in a (multipart) message generated with the MimeKit library. " + 
				"All of the parts of this message are identical, however they've been encoded " +
				"for transport using different methods.";
			var expected = "This is a part in a (multipart) message generated with the MimeKit library.\n" +
				"All of the parts of this message are identical, however they've been encoded\n" +
				"for transport using different methods.\n";

			if (FormatOptions.Default.NewLineFormat != NewLineFormat.Unix)
				expected = expected.Replace ("\n", "\r\n");

			multipart.Epilogue = multiline;

			Assert.That (multipart.Epilogue, Is.EqualTo (expected));

			multipart.Epilogue = null;

			Assert.That (multipart.Epilogue, Is.Null);
		}

		[Test]
		public void TestPreambleFolding ()
		{
			const string text = "This is a multipart MIME message. If you are reading this text, then it means that your mail client does not support MIME.\n";
			const string expected = "This is a multipart MIME message. If you are reading this text, then it means\nthat your mail client does not support MIME.\n";
			var options = FormatOptions.Default.Clone ();

			options.NewLineFormat = NewLineFormat.Unix;

			var actual = Multipart.FoldPreambleOrEpilogue (options, text, false);

			Assert.That (actual, Is.EqualTo (expected), "Folded multipart preamble does not match.");
		}

		[Test]
		public void TestEpilogueFolding ()
		{
			const string text = "This is a multipart epilogue.";
			const string expected = "\nThis is a multipart epilogue.\n";
			var options = FormatOptions.Default.Clone ();

			options.NewLineFormat = NewLineFormat.Unix;

			var actual = Multipart.FoldPreambleOrEpilogue (options, text, true);

			Assert.That (actual, Is.EqualTo (expected), "Folded multipart preamble does not match.");
		}

		[Test]
		public void TestSettingPreambleHasExpectedSideEffects ()
		{
			const string preamble = "This is the preamble";
			string expected = $"{preamble}{FormatOptions.Default.NewLine}";
			var multipart = new Multipart ("mixed");

			Assert.That (multipart.Preamble, Is.Null, "Preamble should be null by default");
			Assert.That (multipart.WriteEndBoundary, Is.True, "WriteEndBoundary should be true by default");

			multipart.Preamble = preamble;
			Assert.That (multipart.Preamble, Is.EqualTo (expected), $"Preamble should now be set to '{preamble}' + newline");

			multipart.Preamble = expected;
			Assert.That (multipart.Preamble, Is.EqualTo (expected), $"Preamble should not have changed");
		}

		[Test]
		public void TestSettingEpilogueHasExpectedSideEffects ()
		{
			const string epilogue = "This is the epilogue";
			string expected = $"{epilogue}{FormatOptions.Default.NewLine}";
			var multipart = new Multipart ("mixed");

			Assert.That (multipart.Epilogue, Is.Null, "Epilogue should be null by default");
			Assert.That (multipart.WriteEndBoundary, Is.True, "WriteEndBoundary should be true by default");

			multipart.Epilogue = epilogue;
			Assert.That (multipart.Epilogue, Is.EqualTo (expected), $"Epilogue should now be set to '{epilogue}' + newline");
			Assert.That (multipart.WriteEndBoundary, Is.True, "WriteEndBoundary should now be true after setting the Epilogue");

			multipart.Epilogue = expected;
			Assert.That (multipart.Epilogue, Is.EqualTo (expected), $"Epilogue should not have changed");
			Assert.That (multipart.WriteEndBoundary, Is.True, "WriteEndBoundary should not have changed");

			// Now test to see what we'd get if the Multipart was parsed by the parser and did not include an end boundary
			multipart = new Multipart ("mixed") {
				RawEndBoundary = Array.Empty<byte> ()
			};

			Assert.That (multipart.Epilogue, Is.Null, "Epilogue should be null by default");
			Assert.That (multipart.WriteEndBoundary, Is.False, "WriteEndBoundary should be false when RawEndBoundary is empty");

			multipart.Epilogue = epilogue;
			Assert.That (multipart.Epilogue, Is.EqualTo (expected), $"Epilogue should now be set to '{epilogue}' + newline");
			Assert.That (multipart.WriteEndBoundary, Is.True, "WriteEndBoundary should now be true after setting the Epilogue");

			multipart.Epilogue = expected;
			Assert.That (multipart.Epilogue, Is.EqualTo (expected), $"Epilogue should not have changed");
			Assert.That (multipart.WriteEndBoundary, Is.True, "WriteEndBoundary should not have changed");
		}

		[Test]
		public void TestSettingSameBoundaryIsNoOp ()
		{
			var multipart = new Multipart ("mixed") {
				Boundary = "boundary"
			};

			multipart.Boundary = "boundary";

			Assert.That (multipart.Boundary, Is.EqualTo ("boundary"));
		}

		[Test]
		public void TestParsedEpilogueStartingWithUnixNewLine ()
		{
			const string message = "Content-Type: multipart/mixed; boundary=\"boundary\"\n\n--boundary\nContent-Type: text/plain\n\nbody\n--boundary--\nepilogue";
			using var stream = new MemoryStream (Encoding.ASCII.GetBytes (message), false);
			var multipart = (Multipart) MimeEntity.Load (stream);

			Assert.That (multipart.Epilogue, Is.EqualTo ("epilogue"));
		}

		[TestCase ("\n", TestName = "TestParsedEpilogueOnlyNewLine_Unix")]
		[TestCase ("\r\n", TestName = "TestParsedEpilogueOnlyNewLine_Dos")]
		public void TestParsedEpilogueOnlyNewLine (string newLine)
		{
			var message = "Content-Type: multipart/mixed; boundary=\"boundary\"" + newLine + newLine + "--boundary" + newLine + "Content-Type: text/plain" + newLine + newLine + "body" + newLine + "--boundary--" + newLine;
			using var stream = new MemoryStream (Encoding.ASCII.GetBytes (message), false);
			var multipart = (Multipart) MimeEntity.Load (stream);

			Assert.That (multipart.Epilogue, Is.EqualTo (string.Empty));
		}

		[Test]
		public void TestTryGetValueStopsAfterNestedMultipart ()
		{
			var alternative = new Multipart ("alternative") {
				new TextPart ("plain") { Text = "plain" }
			};
			var multipart = new Multipart ("mixed") {
				alternative,
				new TextPart ("html") { Text = "<p>html</p>" }
			};

			Assert.That (multipart.TryGetValue (TextFormat.Plain, out var plain), Is.True);
			Assert.That (plain.Text, Is.EqualTo ("plain"));
			Assert.That (multipart.TryGetValue (TextFormat.Html, out var html), Is.False);
			Assert.That (html, Is.Null);
		}

		[Test]
		public void TestNonGenericEnumerator ()
		{
			var plain = new TextPart ("plain") { Text = "plain" };
			IEnumerable multipart = new Multipart ("mixed") {
				plain
			};
			var enumerator = multipart.GetEnumerator ();

			Assert.That (enumerator.MoveNext (), Is.True);
			Assert.That (enumerator.Current, Is.SameAs (plain));
			Assert.That (enumerator.MoveNext (), Is.False);
		}

		[Test]
		public void TestBoundaryChangeRegeneratesParsedBoundaries ()
		{
			const string message = "Content-Type: multipart/mixed; boundary=\"old\"\n\n--old\nContent-Type: text/plain\n\nbody\n--old--\n";
			const string expected = "Content-Type: multipart/mixed; boundary=new\n\n--new\nContent-Type: text/plain\n\nbody\n--new--\n";
			using var stream = new MemoryStream (Encoding.ASCII.GetBytes (message), false);
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;
			var multipart = (Multipart) MimeEntity.Load (stream);

			multipart.Boundary = "new";

			using var output = new MemoryStream ();
			multipart.WriteTo (options, output);
			var actual = Encoding.ASCII.GetString (output.GetBuffer (), 0, (int) output.Length);

			Assert.That (actual, Is.EqualTo (expected));
		}

		[Test]
		public void TestMultipartSignedIgnoresHiddenHeadersForChildren ()
		{
			const string expected = "Content-Type: multipart/signed; boundary=boundary\n\n--boundary\nContent-Type: text/plain; charset=utf-8\nContent-Description: signed\n\nbody\n--boundary\nContent-Type: application/pgp-signature\n\nsig\n--boundary--\n";
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;
			options.HiddenHeaders.Add (HeaderId.ContentDescription);
			options.International = true;
			var multipart = new Multipart ("signed") {
				new TextPart ("plain") { Text = "body", ContentDescription = "signed" },
				new MimePart ("application", "pgp-signature") {
					Content = new MimeContent (new MemoryStream (Encoding.ASCII.GetBytes ("sig"), false))
				}
			};

			multipart.Boundary = "boundary";

			using var output = new MemoryStream ();
			multipart.WriteTo (options, output);
			var actual = Encoding.ASCII.GetString (output.GetBuffer (), 0, (int) output.Length);

			Assert.That (actual, Is.EqualTo (expected));
		}
	}
}

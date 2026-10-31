//
// MimeParserTests.cs
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

using Newtonsoft.Json;

using MimeKit;
using MimeKit.IO;
using MimeKit.Utils;
using MimeKit.IO.Filters;

using UnitTests.IO;

namespace UnitTests {
	[TestFixture]
	public class MimeParserTests
	{
		static readonly string MessagesDataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "messages");
		static readonly string MboxDataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "mbox");
		static readonly FormatOptions UnixFormatOptions;

		static MimeParserTests ()
		{
			UnixFormatOptions = FormatOptions.Default.Clone ();
			UnixFormatOptions.NewLineFormat = NewLineFormat.Unix;
		}

		static void AssertSerialization (MimeMessage message, NewLineFormat format, string expected, bool ensureNewLine = false)
		{
			if (expected.StartsWith ("From -", StringComparison.Ordinal)) {
				int eoln = expected.IndexOf ('\n');
				expected = expected.Substring (eoln + 1);
			}

			using (var memory = new MemoryStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.EnsureNewLine = ensureNewLine;
				options.NewLineFormat = format;

				message.WriteTo (options, memory);

				var actual = Encoding.ASCII.GetString (memory.GetBuffer (), 0, (int) memory.Length);
				Assert.That (actual, Is.EqualTo (expected));
			}
		}

		static async Task AssertSerializationAsync (MimeMessage message, NewLineFormat format, string expected, bool ensureNewLine = false)
		{
			if (expected.StartsWith ("From -", StringComparison.Ordinal)) {
				int eoln = expected.IndexOf ('\n');
				expected = expected.Substring (eoln + 1);
			}

			using (var memory = new MemoryStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.EnsureNewLine = ensureNewLine;
				options.NewLineFormat = format;

				await message.WriteToAsync (options, memory);

				var actual = Encoding.ASCII.GetString (memory.GetBuffer (), 0, (int) memory.Length);
				Assert.That (actual, Is.EqualTo (expected));
			}
		}

		static void AssertSerialization (MimeEntity entity, NewLineFormat format, string expected)
		{
			using (var memory = new MemoryStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = format;

				entity.WriteTo (options, memory);

				var actual = Encoding.ASCII.GetString (memory.GetBuffer (), 0, (int) memory.Length);
				Assert.That (actual, Is.EqualTo (expected));
			}
		}

		static async Task AssertSerializationAsync (MimeEntity entity, NewLineFormat format, string expected)
		{
			using (var memory = new MemoryStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = format;

				await entity.WriteToAsync (options, memory);

				var actual = Encoding.ASCII.GetString (memory.GetBuffer (), 0, (int) memory.Length);
				Assert.That (actual, Is.EqualTo (expected));
			}
		}

		[Test]
		public void TestArgumentExceptions ()
		{
			using (var stream = new MemoryStream ()) {
				var parser = new MimeParser (stream);

				Assert.That (parser.Position, Is.EqualTo (0), "Position");

				Assert.Throws<ArgumentNullException> (() => new MimeParser (null));
				Assert.Throws<ArgumentNullException> (() => new MimeParser (null, stream));
				Assert.Throws<ArgumentNullException> (() => new MimeParser (null, MimeFormat.Default));
				Assert.Throws<ArgumentNullException> (() => new MimeParser (ParserOptions.Default, null));
				Assert.Throws<ArgumentNullException> (() => new MimeParser (null, stream, MimeFormat.Default));
				Assert.Throws<ArgumentNullException> (() => new MimeParser (ParserOptions.Default, null, MimeFormat.Default));

				Assert.Throws<ArgumentNullException> (() => parser.SetStream (null, MimeContentStorage.Memory));
				Assert.Throws<ArgumentNullException> (() => parser.SetStream (null, MimeFormat.Default));
				Assert.Throws<ArgumentNullException> (() => parser.SetStream (null, MimeFormat.Default, MimeContentStorage.Memory));

				Assert.Throws<ArgumentNullException> (() => parser.Options = null);
			}
		}

		static string GetText (MimeMessage message)
		{
			return ((TextPart) message.Body).Text;
		}

		static async Task AssertRejectedSetStream (bool legacy, MimeContentStorage storage, bool doAsync)
		{
			const string text = "From sender@example.com Sat Jan  1 00:00:00 2000\nSubject: first\n\nfirst body\n" +
				"From sender@example.com Sat Jan  1 00:00:00 2000\nSubject: second\n\nsecond body\n";
			using var source = new MemoryStream (Encoding.ASCII.GetBytes (text));
			using var replacement = new MemoryStream (Encoding.ASCII.GetBytes ("Subject: replacement\n\nreplacement"));
			using var nonseekable = new NonSeekableStream (replacement);
			IMimeParser parser = legacy ? new LegacyMimeParser (source, MimeFormat.Mbox, storage == MimeContentStorage.Source) : new MimeParser (source, MimeFormat.Mbox, storage);
			using var first = doAsync ? await parser.ParseMessageAsync () : parser.ParseMessage ();
			var position = parser.Position;
			var markerOffset = parser.MboxMarkerOffset;
			var marker = parser.MboxMarker;
			var options = parser.Options;

			void AssertUnchanged ()
			{
				Assert.That (parser.Position, Is.EqualTo (position));
				Assert.That (parser.MboxMarkerOffset, Is.EqualTo (markerOffset));
				Assert.That (parser.MboxMarker, Is.EqualTo (marker));
				Assert.That (parser.Options, Is.SameAs (options));
				Assert.That (parser.IsEndOfStream, Is.False);
			}

			Assert.Throws<ArgumentException> (() => parser.SetStream (nonseekable, MimeContentStorage.Source));
			AssertUnchanged ();
			Assert.Throws<ArgumentException> (() => parser.SetStream (nonseekable, MimeFormat.Entity, MimeContentStorage.Source));
			AssertUnchanged ();
			Assert.Throws<ArgumentOutOfRangeException> (() => parser.SetStream (replacement, (MimeContentStorage) 2));
			AssertUnchanged ();
			Assert.Throws<ArgumentOutOfRangeException> (() => parser.SetStream (replacement, MimeFormat.Entity, (MimeContentStorage) (-1)));
			AssertUnchanged ();
			Assert.Throws<ArgumentOutOfRangeException> (() => parser.SetStream (replacement, (MimeFormat) 255, storage));
			AssertUnchanged ();
			Assert.Throws<ArgumentNullException> (() => parser.SetStream (null, MimeFormat.Entity, storage));
			AssertUnchanged ();
			Assert.That (GetText (first), Does.StartWith ("first body"));
			using var second = doAsync ? await parser.ParseMessageAsync () : parser.ParseMessage ();
			Assert.That (second.Subject, Is.EqualTo ("second"));
			Assert.That (GetText (second), Does.StartWith ("second body"));
			Assert.That (((MimePart) second.Body).Content.Stream is BoundedStream, Is.EqualTo (storage == MimeContentStorage.Source));
			Assert.That (parser.IsEndOfStream, Is.True);
		}

		[Test]
		public void TestRejectedSetStream ([Values (false, true)] bool legacy, [Values] MimeContentStorage storage)
		{
			AssertRejectedSetStream (legacy, storage, false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestRejectedSetStreamAsync ([Values (false, true)] bool legacy, [Values] MimeContentStorage storage)
		{
			await AssertRejectedSetStream (legacy, storage, true);
		}

		static async Task AssertContentStorageLifetime (int api, MimeContentStorage storage, bool useDefault, bool doAsync)
		{
			var data = Encoding.ASCII.GetBytes ("Content-Type: text/plain\r\n\r\noriginal body");
			using var source = new MemoryStream (data);
			MimeMessage message = null;
			TextPart part;

			try {
				if (api < 2) {
					IMimeParser parser;
					if (api == 0)
						parser = useDefault ? new MimeParser (source) : new MimeParser (source, storage);
					else
						parser = useDefault ? new LegacyMimeParser (source) : new LegacyMimeParser (source, storage == MimeContentStorage.Source);
					message = doAsync ? await parser.ParseMessageAsync () : parser.ParseMessage ();
					parser.SetStream (Stream.Null);
					part = (TextPart) message.Body;
				} else if (api == 2) {
					if (doAsync)
						message = useDefault ? await MimeMessage.LoadAsync (source) : await MimeMessage.LoadAsync (source, storage);
					else
						message = useDefault ? MimeMessage.Load (source) : MimeMessage.Load (source, storage);
					part = (TextPart) message.Body;
				} else {
					if (doAsync)
						part = (TextPart) (useDefault ? await MimeEntity.LoadAsync (source) : await MimeEntity.LoadAsync (source, storage));
					else
						part = (TextPart) (useDefault ? MimeEntity.Load (source) : MimeEntity.Load (source, storage));
				}

				using (part) {
					bool referencesSource = !useDefault && storage == MimeContentStorage.Source;
					Assert.That (part.Content.Stream is BoundedStream, Is.EqualTo (referencesSource));
					Assert.That (part.Text, Is.EqualTo ("original body"));
					if (referencesSource) {
						if (message != null)
							message.Dispose ();
						else
							part.Dispose ();
						Assert.That (source.CanRead, Is.True, "Disposing content must not close the caller's stream");

						source.Position = 0;
						using var another = doAsync ? await MimeMessage.LoadAsync (source, storage) : MimeMessage.Load (source, storage);
						source.Dispose ();
						Assert.Throws<NotSupportedException> (() => { _ = ((TextPart) another.Body).Text; });
					} else {
						source.Position = source.Length - 13;
						source.Write (Encoding.ASCII.GetBytes ("modified body"), 0, 13);
						source.Dispose ();
						Assert.That (part.Text, Is.EqualTo ("original body"), "Memory content is independent of the input");
					}
				}
			} finally {
				message?.Dispose ();
			}
		}

		[Test]
		public void TestContentStorageLifetime ([Values (0, 1, 2, 3)] int api, [Values] MimeContentStorage storage, [Values (false, true)] bool useDefault)
		{
			AssertContentStorageLifetime (api, storage, useDefault, false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestContentStorageLifetimeAsync ([Values (0, 1, 2, 3)] int api, [Values] MimeContentStorage storage, [Values (false, true)] bool useDefault)
		{
			await AssertContentStorageLifetime (api, storage, useDefault, true);
		}

		static async Task AssertSetStreamContentStorage (bool legacy, MimeContentStorage storage, bool shortOverload, bool doAsync)
		{
			using var source = new MemoryStream (Encoding.ASCII.GetBytes ("Content-Type: text/plain\r\n\r\nbody"));
			IMimeParser parser = legacy ? new LegacyMimeParser (Stream.Null) : new MimeParser (Stream.Null);
			if (shortOverload)
				parser.SetStream (source, storage);
			else
				parser.SetStream (source, MimeFormat.Entity, storage);

			using var message = doAsync ? await parser.ParseMessageAsync () : parser.ParseMessage ();
			Assert.That (((MimePart) message.Body).Content.Stream is BoundedStream, Is.EqualTo (storage == MimeContentStorage.Source));
			Assert.That (GetText (message), Is.EqualTo ("body"));
			source.Position = 0;
			parser.SetStream (source);
			using var copied = doAsync ? await parser.ParseMessageAsync () : parser.ParseMessage ();
			Assert.That (((MimePart) copied.Body).Content.Stream, Is.TypeOf<MemoryBlockStream> ());
			message.Dispose ();
			Assert.That (source.CanRead, Is.True);
		}

		[Test]
		public void TestSetStreamContentStorage ([Values (false, true)] bool legacy, [Values] MimeContentStorage storage, [Values (false, true)] bool shortOverload)
		{
			AssertSetStreamContentStorage (legacy, storage, shortOverload, false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestSetStreamContentStorageAsync ([Values (false, true)] bool legacy, [Values] MimeContentStorage storage, [Values (false, true)] bool shortOverload)
		{
			await AssertSetStreamContentStorage (legacy, storage, shortOverload, true);
		}

		[Test]
		public void TestLegacyPersistentNonseekableCompatibility ()
		{
			using var memory = new MemoryStream (Encoding.ASCII.GetBytes ("\r\nbody"));
			using var source = new NonSeekableStream (memory);
			var parser = new LegacyMimeParser (source, true);
			parser.SetStream (source, MimeFormat.Entity, true);
			using var message = parser.ParseMessage ();
			Assert.That (GetText (message), Is.EqualTo ("body"));
			Assert.That (((MimePart) message.Body).Content.Stream, Is.TypeOf<MemoryBlockStream> ());
		}

		[Test]
		public async Task TestLegacyPersistentNonseekableCompatibilityAsync ()
		{
			using var memory = new MemoryStream (Encoding.ASCII.GetBytes ("\r\nbody"));
			using var source = new NonSeekableStream (memory);
			var parser = new LegacyMimeParser (source, true);
			parser.SetStream (source, true);
			using var message = await parser.ParseMessageAsync ();
			Assert.That (GetText (message), Is.EqualTo ("body"));
			Assert.That (((MimePart) message.Body).Content.Stream, Is.TypeOf<MemoryBlockStream> ());
		}

		[Test]
		public void TestSetStreamPersistentOverloads ()
		{
			var first = Encoding.ASCII.GetBytes ("Subject: first\r\n\r\nfirst body");
			var second = Encoding.ASCII.GetBytes ("Subject: second\r\n\r\nsecond body");

			var parser = new MimeParser (Stream.Null);

			using (var stream = new MemoryStream (first, false)) {
				parser.SetStream (stream, MimeFormat.Entity, MimeContentStorage.Source);

				using var message = parser.ParseMessage ();

				Assert.That (message.Subject, Is.EqualTo ("first"));
				Assert.That (GetText (message), Is.EqualTo ("first body"));
				Assert.That (parser.Position, Is.EqualTo (first.Length), "First position");
			}

			using (var stream = new MemoryStream (second, false)) {
				parser.SetStream (stream, MimeContentStorage.Source);

				using var message = parser.ParseMessage ();

				Assert.That (message.Subject, Is.EqualTo ("second"));
				Assert.That (GetText (message), Is.EqualTo ("second body"));
				Assert.That (parser.Position, Is.EqualTo (second.Length), "Second position");
			}
		}

		[Test]
		public async Task TestSetStreamPersistentOverloadsAsync ()
		{
			var first = Encoding.ASCII.GetBytes ("Subject: first\r\n\r\nfirst body");
			var second = Encoding.ASCII.GetBytes ("Subject: second\r\n\r\nsecond body");

			var parser = new MimeParser (Stream.Null);

			using (var stream = new MemoryStream (first, false)) {
				parser.SetStream (stream, MimeFormat.Entity, MimeContentStorage.Source);

				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Subject, Is.EqualTo ("first"));
				Assert.That (GetText (message), Is.EqualTo ("first body"));
				Assert.That (parser.Position, Is.EqualTo (first.Length), "First position");
			}

			using (var stream = new MemoryStream (second, false)) {
				parser.SetStream (stream, MimeContentStorage.Source);

				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Subject, Is.EqualTo ("second"));
				Assert.That (GetText (message), Is.EqualTo ("second body"));
				Assert.That (parser.Position, Is.EqualTo (second.Length), "Second position");
			}
		}

		static void AssertPreHeaderResizeMessage (MimeMessage message)
		{
			const string expected = ">From first preheader\r\n>From second preheader line is longer\r\n";

			Assert.That (Encoding.ASCII.GetString (message.MboxMarker), Is.EqualTo (expected), "MboxMarker");
			Assert.That (message.Headers.Count, Is.EqualTo (2), "Header count");
			Assert.That (message.From.ToString (), Is.EqualTo ("sender@example.org"));
			Assert.That (message.Subject, Is.EqualTo ("preheader resize"));
			Assert.That (GetText (message), Is.EqualTo ("body" + Environment.NewLine));
		}

		[Test]
		public void TestPreHeaderBufferResizesForMultipleInvalidLeadingHeaders ()
		{
			const string text = "Content-Type: message/rfc822\r\n\r\n>From first preheader\r\n>From second preheader line is longer\r\nFrom: sender@example.org\r\nSubject: preheader resize\r\n\r\nbody\r\n";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();
				var rfc822 = (MessagePart) message.Body;

				AssertPreHeaderResizeMessage (rfc822.Message);
			}
		}

		[Test]
		public async Task TestPreHeaderBufferResizesForMultipleInvalidLeadingHeadersAsync ()
		{
			const string text = "Content-Type: message/rfc822\r\n\r\n>From first preheader\r\n>From second preheader line is longer\r\nFrom: sender@example.org\r\nSubject: preheader resize\r\n\r\nbody\r\n";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();
				var rfc822 = (MessagePart) message.Body;

				AssertPreHeaderResizeMessage (rfc822.Message);
			}
		}

		static byte[] CreateMessageThatFailsDuringBodyRead (out int throwOffset)
		{
			var headers = Encoding.ASCII.GetBytes ("Subject: broken\r\n\r\n");
			var body = Encoding.ASCII.GetBytes (new string ('x', 4096));
			var data = new byte[headers.Length + body.Length];

			Buffer.BlockCopy (headers, 0, data, 0, headers.Length);
			Buffer.BlockCopy (body, 0, data, headers.Length, body.Length);

			throwOffset = headers.Length + 64;

			return data;
		}

		static void AssertSuccessfulParseAfterFailure (MimeMessage message)
		{
			Assert.That (message.Subject, Is.EqualTo ("recovered"));
			Assert.That (GetText (message), Is.EqualTo ("ok"));
		}

		[Test]
		public void TestParseMessageRecoversAfterReadFailure ()
		{
			var failing = CreateMessageThatFailsDuringBodyRead (out var throwOffset);
			var recovered = Encoding.ASCII.GetBytes ("Subject: recovered\r\n\r\nok");

			var parser = new MimeParser (new ThrowingReadStream (failing, throwOffset, 7), MimeFormat.Entity);
			var ex = Assert.Throws<IOException> (() => parser.ParseMessage ());

			Assert.That (ex.Message, Is.EqualTo ("Read failed."));

			using (var stream = new MemoryStream (recovered, false)) {
				parser.SetStream (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				AssertSuccessfulParseAfterFailure (message);
			}
		}

		[Test]
		public async Task TestParseMessageRecoversAfterReadFailureAsync ()
		{
			var failing = CreateMessageThatFailsDuringBodyRead (out var throwOffset);
			var recovered = Encoding.ASCII.GetBytes ("Subject: recovered\r\n\r\nok");

			var parser = new MimeParser (new ThrowingReadStream (failing, throwOffset, 7), MimeFormat.Entity);
			var ex = Assert.ThrowsAsync<IOException> (async () => await parser.ParseMessageAsync ());

			Assert.That (ex.Message, Is.EqualTo ("Read failed."));

			using (var stream = new MemoryStream (recovered, false)) {
				parser.SetStream (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				AssertSuccessfulParseAfterFailure (message);
			}
		}

		static void AssertDoubleBoundaryBeforePart (MimeMessage message, string text)
		{
			Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Body type");

			var multipart = (Multipart) message.Body;

			Assert.That (multipart.Count, Is.EqualTo (2), "Child count");
			Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "First child type");
			Assert.That (((TextPart) multipart[0]).Text, Is.EqualTo (string.Empty), "First child text");
			Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Second child type");
			Assert.That (((TextPart) multipart[1]).Text, Is.EqualTo ("body"), "Second child text");

			AssertSerialization (message, NewLineFormat.Dos, text);
		}

		[Test]
		public void TestDoubleBoundaryBeforePartPreservesRawBoundary ()
		{
			const string text = "Content-Type: multipart/mixed; boundary=\"b\"\r\n\r\n--b\r\n--b\r\nContent-Type: text/plain\r\n\r\nbody\r\n--b--\r\n";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				AssertDoubleBoundaryBeforePart (message, text);
			}
		}

		[Test]
		public async Task TestDoubleBoundaryBeforePartPreservesRawBoundaryAsync ()
		{
			const string text = "Content-Type: multipart/mixed; boundary=\"b\"\r\n\r\n--b\r\n--b\r\nContent-Type: text/plain\r\n\r\nbody\r\n--b--\r\n";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				AssertDoubleBoundaryBeforePart (message, text);
			}
		}

		[Test]
		public void TestHeaderParser ()
		{
			var bytes = Encoding.ASCII.GetBytes ("Header-1: value 1\r\nHeader-2: value 2\r\nHeader-3: value 3\r\n\r\n");

			using (var memory = new MemoryStream (bytes, false)) {
				var parser = new MimeParser (memory, MimeFormat.Default);

				try {
					var headers = parser.ParseHeaders ();
					string value;

					Assert.That (headers.Count, Is.EqualTo (3), "Unexpected header count.");

					value = headers["Header-1"];

					Assert.That (value, Is.EqualTo ("value 1"), "Unexpected header value.");

					value = headers["Header-2"];

					Assert.That (value, Is.EqualTo ("value 2"), "Unexpected header value.");

					value = headers["Header-3"];

					Assert.That (value, Is.EqualTo ("value 3"), "Unexpected header value.");
				} catch (Exception ex) {
					Assert.Fail ($"Failed to parse headers: {ex}");
				}
			}
		}

		[Test]
		public async Task TestHeaderParserAsync ()
		{
			var bytes = Encoding.ASCII.GetBytes ("Header-1: value 1\r\nHeader-2: value 2\r\nHeader-3: value 3\r\n\r\n");

			using (var memory = new MemoryStream (bytes, false)) {
				var parser = new MimeParser (memory, MimeFormat.Default);

				try {
					var headers = await parser.ParseHeadersAsync ();
					string value;

					Assert.That (headers.Count, Is.EqualTo (3), "Unexpected header count.");

					value = headers["Header-1"];

					Assert.That (value, Is.EqualTo ("value 1"), "Unexpected header value.");

					value = headers["Header-2"];

					Assert.That (value, Is.EqualTo ("value 2"), "Unexpected header value.");

					value = headers["Header-3"];

					Assert.That (value, Is.EqualTo ("value 3"), "Unexpected header value.");
				} catch (Exception ex) {
					Assert.Fail ($"Failed to parse headers: {ex}");
				}
			}
		}

		static List<HeaderList> ParseStatusGroups (string text, bool readOneByteAtATime = false)
		{
			var bytes = Encoding.ASCII.GetBytes (text);
			var groups = new List<HeaderList> ();

			using (var memory = new MemoryStream (bytes, false)) {
				Stream stream = readOneByteAtATime ? new ReadOneByteStream (memory) : memory;

				try {
					var parser = new MimeParser (stream, MimeFormat.Entity);

					while (!parser.IsEndOfStream) {
						var group = parser.ParseStatusGroup ();

						if (group is null)
							break;

						groups.Add (group);
					}
				} finally {
					if (readOneByteAtATime)
						stream.Dispose ();
				}
			}

			return groups;
		}

		[Test]
		public void TestStatusGroupParser ()
		{
			var groups = ParseStatusGroups ("Reporting-MTA: dns; mm1\r\n\r\nFinal-Recipient: rfc822; user@example.com\r\n");

			Assert.That (groups.Count, Is.EqualTo (2), "Unexpected number of status groups.");
			Assert.That (groups[0]["Reporting-MTA"], Is.EqualTo ("dns; mm1"));
			Assert.That (groups[1]["Final-Recipient"], Is.EqualTo ("rfc822; user@example.com"));
		}

		[Test]
		public void TestStatusGroupParserReturnsNullAtEndOfStream ()
		{
			var bytes = Encoding.ASCII.GetBytes ("Reporting-MTA: dns; mm1\r\n\r\n");

			using var memory = new MemoryStream (bytes, false);
			var parser = new MimeParser (memory, MimeFormat.Entity);

			var group = parser.ParseStatusGroup ();
			Assert.That (group, Is.Not.Null, "Expected the first status group to be parsed.");
			Assert.That (group.Count, Is.EqualTo (1), "Unexpected number of headers in the first status group.");

			Assert.That (parser.ParseStatusGroup (), Is.Null, "Expected null once the end of the stream is reached.");
			Assert.That (parser.IsEndOfStream, Is.True, "Expected IsEndOfStream to be true.");

			// Calling it again should continue to return null rather than throw.
			Assert.That (parser.ParseStatusGroup (), Is.Null, "Expected null on subsequent calls.");
		}

		[TestCase ("", TestName = "TestStatusGroupParserNoContent")]
		[TestCase ("\r\n", TestName = "TestStatusGroupParserSingleBlankLine")]
		[TestCase ("\r\n\r\n\r\n", TestName = "TestStatusGroupParserOnlyBlankLines")]
		[TestCase ("\n\n\n", TestName = "TestStatusGroupParserOnlyBlankLinesLF")]
		[TestCase ("\r", TestName = "TestStatusGroupParserOnlyLoneCarriageReturn")]
		public void TestStatusGroupParserNoStatusGroups (string text)
		{
			var groups = ParseStatusGroups (text);

			Assert.That (groups.Count, Is.EqualTo (0), "Did not expect any status groups.");
		}

		[Test]
		public void TestStatusGroupParserTrailingLoneCarriageReturn ()
		{
			// Note: The trailing lone CR is not a status group, so it should not result in an empty status group.
			var groups = ParseStatusGroups ("Reporting-MTA: dns; mm1\r\n\r\nFinal-Recipient: rfc822; user@example.com\r\n\r\n\r");

			Assert.That (groups.Count, Is.EqualTo (2), "Unexpected number of status groups.");
			Assert.That (groups[0]["Reporting-MTA"], Is.EqualTo ("dns; mm1"));
			Assert.That (groups[1]["Final-Recipient"], Is.EqualTo ("rfc822; user@example.com"));
		}

		[Test]
		public void TestStatusGroupParserTrailingBlankLines ()
		{
			// Note: The trailing blank lines should not produce an empty status group.
			var groups = ParseStatusGroups ("Reporting-MTA: dns; mm1\r\n\r\nFinal-Recipient: rfc822; user@example.com\r\n\r\n\r\n\r\n");

			Assert.That (groups.Count, Is.EqualTo (2), "Unexpected number of status groups.");
			Assert.That (groups[0]["Reporting-MTA"], Is.EqualTo ("dns; mm1"));
			Assert.That (groups[1]["Final-Recipient"], Is.EqualTo ("rfc822; user@example.com"));
		}

		[Test]
		public void TestStatusGroupParserLoneCarriageReturnSeparator ()
		{
			var bytes = Encoding.ASCII.GetBytes ("Reporting-MTA: dns; mm1\r\n\r\n\rFinal-Recipient: rfc822; user@example.com\r\n");

			using var memory = new MemoryStream (bytes, false);
			var parser = new MimeParser (memory, MimeFormat.Entity);

			var group = parser.ParseStatusGroup ();
			Assert.That (group, Is.Not.Null, "Expected the first status group to be parsed.");
			Assert.That (group["Reporting-MTA"], Is.EqualTo ("dns; mm1"));

			// Note: A lone CR is not a blank line, so SkipBlankLines() stops there and the remainder
			// of the stream fails to parse as a block of headers.
			Assert.Throws<FormatException> (() => parser.ParseStatusGroup ());
		}

		[Test]
		public void TestStatusGroupParserLeadingBlankLines ()
		{
			var groups = ParseStatusGroups ("\r\n\r\n\r\nReporting-MTA: dns; mm1\r\n\r\n\r\n\r\nFinal-Recipient: rfc822; user@example.com\r\n");

			Assert.That (groups.Count, Is.EqualTo (2), "Unexpected number of status groups.");
			Assert.That (groups[0]["Reporting-MTA"], Is.EqualTo ("dns; mm1"));
			Assert.That (groups[1]["Final-Recipient"], Is.EqualTo ("rfc822; user@example.com"));
		}

		[Test]
		public void TestStatusGroupParserMixedNewLineFormats ()
		{
			var groups = ParseStatusGroups ("Reporting-MTA: dns; mm1\r\n\n\r\n\nFinal-Recipient: rfc822; user@example.com\n");

			Assert.That (groups.Count, Is.EqualTo (2), "Unexpected number of status groups.");
			Assert.That (groups[0]["Reporting-MTA"], Is.EqualTo ("dns; mm1"));
			Assert.That (groups[1]["Final-Recipient"], Is.EqualTo ("rfc822; user@example.com"));
		}

		[Test]
		public void TestStatusGroupParserReadOneByteAtATime ()
		{
			// Note: Reading a single byte at a time forces SkipBlankLines() to deal with
			// CRLF sequences that span multiple reads.
			var groups = ParseStatusGroups ("\r\n\r\nReporting-MTA: dns; mm1\r\n\r\n\r\nFinal-Recipient: rfc822; user@example.com\r\n\r\n\r\n", true);

			Assert.That (groups.Count, Is.EqualTo (2), "Unexpected number of status groups.");
			Assert.That (groups[0]["Reporting-MTA"], Is.EqualTo ("dns; mm1"));
			Assert.That (groups[1]["Final-Recipient"], Is.EqualTo ("rfc822; user@example.com"));
		}

		[Test]
		public void TestTruncatedHeaderName ()
		{
			var bytes = Encoding.ASCII.GetBytes ("Header-1");

			using (var memory = new MemoryStream (bytes, false)) {
				var parser = new MimeParser (memory, MimeFormat.Default);

				try {
					var headers = parser.ParseHeaders ();
					Assert.Fail ("Parsing headers should fail.");
				} catch (FormatException) {
				} catch (Exception ex) {
					Assert.Fail ($"Failed to parse headers: {ex}");
				}
			}
		}

		[Test]
		public async Task TestTruncatedHeaderNameAsync ()
		{
			var bytes = Encoding.ASCII.GetBytes ("Header-1");

			using (var memory = new MemoryStream (bytes, false)) {
				var parser = new MimeParser (memory, MimeFormat.Default);

				try {
					var headers = await parser.ParseHeadersAsync ();
					Assert.Fail ("Parsing headers should fail.");
				} catch (FormatException) {
				} catch (Exception ex) {
					Assert.Fail ($"Failed to parse headers: {ex}");
				}
			}
		}

		[TestCase ("Header-1: value 1\r\nJunk")]
		[TestCase ("Header-1: value 1\r\nJunk\r")]
		[TestCase ("Header-1: value 1\r\nJunk\r\n")]
		public void TestTruncatedInvalidHeaderAfterValidHeader (string text)
		{
			var bytes = Encoding.ASCII.GetBytes (text);

			using (var memory = new MemoryStream (bytes, false)) {
				var parser = new MimeParser (memory, MimeFormat.Default);
				var headers = parser.ParseHeaders ();

				Assert.That (headers.Count, Is.EqualTo (2), "Unexpected header count.");
				Assert.That (headers["Header-1"], Is.EqualTo ("value 1"), "Unexpected header value.");
				Assert.That (headers[1].IsInvalid, Is.True, "IsInvalid");
				Assert.That (headers[1].Field.TrimEnd (), Is.EqualTo ("Junk"), "Field");
			}
		}

		[TestCase ("Header-1: value 1\r\nJunk")]
		[TestCase ("Header-1: value 1\r\nJunk\r")]
		[TestCase ("Header-1: value 1\r\nJunk\r\n")]
		public async Task TestTruncatedInvalidHeaderAfterValidHeaderAsync (string text)
		{
			var bytes = Encoding.ASCII.GetBytes (text);

			using (var memory = new MemoryStream (bytes, false)) {
				var parser = new MimeParser (memory, MimeFormat.Default);
				var headers = await parser.ParseHeadersAsync ();

				Assert.That (headers.Count, Is.EqualTo (2), "Unexpected header count.");
				Assert.That (headers["Header-1"], Is.EqualTo ("value 1"), "Unexpected header value.");
				Assert.That (headers[1].IsInvalid, Is.True, "IsInvalid");
				Assert.That (headers[1].Field.TrimEnd (), Is.EqualTo ("Junk"), "Field");
			}
		}

		[TestCase ("From - \r\nThis is not a header\r\nSubject: test\r\n\r\nbody\r\n")]
		[TestCase ("From - \r\n>From munged\r\nThis is not a header\r\n")]
		public void TestInvalidHeaderAfterMboxMarker (string text)
		{
			var bytes = Encoding.ASCII.GetBytes (text);

			using (var memory = new MemoryStream (bytes, false)) {
				var parser = new MimeParser (memory, MimeFormat.Entity);

				Assert.Throws<FormatException> (() => parser.ParseMessage ());
			}
		}

		[TestCase ("From - \r\nThis is not a header\r\nSubject: test\r\n\r\nbody\r\n")]
		[TestCase ("From - \r\n>From munged\r\nThis is not a header\r\n")]
		public void TestInvalidHeaderAfterMboxMarkerAsync (string text)
		{
			var bytes = Encoding.ASCII.GetBytes (text);

			using (var memory = new MemoryStream (bytes, false)) {
				var parser = new MimeParser (memory, MimeFormat.Entity);

				Assert.ThrowsAsync<FormatException> (() => parser.ParseMessageAsync ());
			}
		}

		[Test]
		public void TestMungedMboxMarkersBeforeHeaders ()
		{
			var bytes = Encoding.ASCII.GetBytes ("From - \r\n>From munged\r\nSubject: test\r\n\r\nbody\r\n");

			using (var memory = new MemoryStream (bytes, false)) {
				var parser = new MimeParser (memory, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Headers.Count, Is.EqualTo (1), "Header count");
				Assert.That (message.Subject, Is.EqualTo ("test"));
				Assert.That (Encoding.ASCII.GetString (message.MboxMarker), Is.EqualTo ("From - \r\n>From munged\r\n"), "MboxMarker");
			}
		}

		[Test]
		public async Task TestMungedMboxMarkersBeforeHeadersAsync ()
		{
			var bytes = Encoding.ASCII.GetBytes ("From - \r\n>From munged\r\nSubject: test\r\n\r\nbody\r\n");

			using (var memory = new MemoryStream (bytes, false)) {
				var parser = new MimeParser (memory, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Headers.Count, Is.EqualTo (1), "Header count");
				Assert.That (message.Subject, Is.EqualTo ("test"));
				Assert.That (Encoding.ASCII.GetString (message.MboxMarker), Is.EqualTo ("From - \r\n>From munged\r\n"), "MboxMarker");
			}
		}

		[Test]
		public void TestTruncatedHeader ()
		{
			var bytes = Encoding.ASCII.GetBytes ("Header-1: value 1");

			using (var memory = new MemoryStream (bytes, false)) {
				var parser = new MimeParser (memory, MimeFormat.Default);

				try {
					var headers = parser.ParseHeaders ();

					Assert.That (headers.Count, Is.EqualTo (1), "Unexpected header count.");

					var value = headers["Header-1"];

					Assert.That (value, Is.EqualTo ("value 1"), "Unexpected header value.");
				} catch (Exception ex) {
					Assert.Fail ($"Failed to parse headers: {ex}");
				}
			}
		}

		[Test]
		public async Task TestTruncatedHeaderAsync ()
		{
			var bytes = Encoding.ASCII.GetBytes ("Header-1: value 1");

			using (var memory = new MemoryStream (bytes, false)) {
				var parser = new MimeParser (memory, MimeFormat.Default);

				try {
					var headers = await parser.ParseHeadersAsync ();

					Assert.That (headers.Count, Is.EqualTo (1), "Unexpected header count.");

					var value = headers["Header-1"];

					Assert.That (value, Is.EqualTo ("value 1"), "Unexpected header value.");
				} catch (Exception ex) {
					Assert.Fail ($"Failed to parse headers: {ex}");
				}
			}
		}

		[Test]
		public void TestSingleHeaderNoTerminator ()
		{
			var bytes = Encoding.ASCII.GetBytes ("Header-1: value 1\r\n");

			using (var memory = new MemoryStream (bytes, false)) {
				var parser = new MimeParser (memory, MimeFormat.Default);

				try {
					var headers = parser.ParseHeaders ();

					Assert.That (headers.Count, Is.EqualTo (1), "Unexpected header count.");

					var value = headers["Header-1"];

					Assert.That (value, Is.EqualTo ("value 1"), "Unexpected header value.");
				} catch (Exception ex) {
					Assert.Fail ($"Failed to parse headers: {ex}");
				}
			}
		}

		[Test]
		public async Task TestSingleHeaderNoTerminatorAsync ()
		{
			var bytes = Encoding.ASCII.GetBytes ("Header-1: value 1\r\n");

			using (var memory = new MemoryStream (bytes, false)) {
				var parser = new MimeParser (memory, MimeFormat.Default);

				try {
					var headers = await parser.ParseHeadersAsync ();

					Assert.That (headers.Count, Is.EqualTo (1), "Unexpected header count.");

					var value = headers["Header-1"];

					Assert.That (value, Is.EqualTo ("value 1"), "Unexpected header value.");
				} catch (Exception ex) {
					Assert.Fail ($"Failed to parse headers: {ex}");
				}
			}
		}

		[Test]
		public void TestEmptyHeaders ()
		{
			var bytes = Encoding.ASCII.GetBytes ("\r\n");

			using (var memory = new MemoryStream (bytes, false)) {
				var parser = new MimeParser (memory, MimeFormat.Default);

				try {
					var headers = parser.ParseHeaders ();

					Assert.That (headers.Count, Is.EqualTo (0), "Unexpected header count.");
				} catch (Exception ex) {
					Assert.Fail ($"Failed to parse headers: {ex}");
				}
			}
		}

		[Test]
		public async Task TestEmptyHeadersAsync ()
		{
			var bytes = Encoding.ASCII.GetBytes ("\r\n");

			using (var memory = new MemoryStream (bytes, false)) {
				var parser = new MimeParser (memory, MimeFormat.Default);

				try {
					var headers = await parser.ParseHeadersAsync ();

					Assert.That (headers.Count, Is.EqualTo (0), "Unexpected header count.");
				} catch (Exception ex) {
					Assert.Fail ($"Failed to parse headers: {ex}");
				}
			}
		}

		[Test]
		public void TestHeadersEndWithBareCarriageReturn ()
		{
			var bytes = Encoding.ASCII.GetBytes ("From: <mimekit@example.com>\r\nTo: <mimekit@example.com>\r\nSubject: Test of headers ending with bare carriage-return\r\n\r");

			using (var memory = new MemoryStream (bytes, false)) {
				var parser = new MimeParser (memory, MimeFormat.Entity);
				var headers = parser.ParseHeaders ();

				Assert.That (headers.Count, Is.EqualTo (3), "Unexpected header count.");
				Assert.That (headers[0].Id, Is.EqualTo (HeaderId.From));
				Assert.That (headers[0].Value, Is.EqualTo ("<mimekit@example.com>"));
				Assert.That (headers[1].Id, Is.EqualTo (HeaderId.To));
				Assert.That (headers[1].Value, Is.EqualTo ("<mimekit@example.com>"));
				Assert.That (headers[2].Id, Is.EqualTo (HeaderId.Subject));
				Assert.That (headers[2].Value, Is.EqualTo ("Test of headers ending with bare carriage-return"));
			}
		}

		[Test]
		public async Task TestHeadersEndWithBareCarriageReturnAsync ()
		{
			var bytes = Encoding.ASCII.GetBytes ("From: <mimekit@example.com>\r\nTo: <mimekit@example.com>\r\nSubject: Test of headers ending with bare carriage-return\r\n\r");

			using (var memory = new MemoryStream (bytes, false)) {
				var parser = new MimeParser (memory, MimeFormat.Entity);
				var headers = await parser.ParseHeadersAsync ();

				Assert.That (headers.Count, Is.EqualTo (3), "Unexpected header count.");
				Assert.That (headers[0].Id, Is.EqualTo (HeaderId.From));
				Assert.That (headers[0].Value, Is.EqualTo ("<mimekit@example.com>"));
				Assert.That (headers[1].Id, Is.EqualTo (HeaderId.To));
				Assert.That (headers[1].Value, Is.EqualTo ("<mimekit@example.com>"));
				Assert.That (headers[2].Id, Is.EqualTo (HeaderId.Subject));
				Assert.That (headers[2].Value, Is.EqualTo ("Test of headers ending with bare carriage-return"));
			}
		}

		[Test]
		public void TestHeadersWithBareCarriageReturn ()
		{
			var bytes = Encoding.ASCII.GetBytes ("From: <mimekit@example.com>\r\nTo: <mimekit@example.com>\r\nSubject: Test of headers ending with bare carriage-return\r\n\rYou might expect this to be a body, but it's really an invalid header.\r\n");

			using (var memory = new MemoryStream (bytes, false)) {
				var parser = new MimeParser (memory, MimeFormat.Entity);
				var headers = parser.ParseHeaders ();

				Assert.That (headers.Count, Is.EqualTo (4), "Unexpected header count.");
				Assert.That (headers[0].Id, Is.EqualTo (HeaderId.From));
				Assert.That (headers[0].Value, Is.EqualTo ("<mimekit@example.com>"));
				Assert.That (headers[1].Id, Is.EqualTo (HeaderId.To));
				Assert.That (headers[1].Value, Is.EqualTo ("<mimekit@example.com>"));
				Assert.That (headers[2].Id, Is.EqualTo (HeaderId.Subject));
				Assert.That (headers[2].Value, Is.EqualTo ("Test of headers ending with bare carriage-return"));
				Assert.That (headers[3].IsInvalid, Is.True);
				Assert.That (headers[3].Field, Is.EqualTo ("\rYou might expect this to be a body, but it's really an invalid header.\r\n"));
			}
		}

		[Test]
		public async Task TestHeadersWithBareCarriageReturnAsync ()
		{
			var bytes = Encoding.ASCII.GetBytes ("From: <mimekit@example.com>\r\nTo: <mimekit@example.com>\r\nSubject: Test of headers ending with bare carriage-return\r\n\rYou might expect this to be a body, but it's really an invalid header.\r\n");

			using (var memory = new MemoryStream (bytes, false)) {
				var parser = new MimeParser (memory, MimeFormat.Entity);
				var headers = await parser.ParseHeadersAsync ();

				Assert.That (headers.Count, Is.EqualTo (4), "Unexpected header count.");
				Assert.That (headers[0].Id, Is.EqualTo (HeaderId.From));
				Assert.That (headers[0].Value, Is.EqualTo ("<mimekit@example.com>"));
				Assert.That (headers[1].Id, Is.EqualTo (HeaderId.To));
				Assert.That (headers[1].Value, Is.EqualTo ("<mimekit@example.com>"));
				Assert.That (headers[2].Id, Is.EqualTo (HeaderId.Subject));
				Assert.That (headers[2].Value, Is.EqualTo ("Test of headers ending with bare carriage-return"));
				Assert.That (headers[3].IsInvalid, Is.True);
				Assert.That (headers[3].Field, Is.EqualTo ("\rYou might expect this to be a body, but it's really an invalid header.\r\n"));
			}
		}

		[Test]
		public void TestPartialByteOrderMarkEOF ()
		{
			var bom = new byte[] { 0xEF, 0xBB/*, 0xBF */ };

			using (var stream = new MemoryStream (bom, false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);

				Assert.Throws<FormatException> (() => parser.ParseMessage (), "ParseMessage");

				stream.Position = 0;

				parser.SetStream (stream, MimeFormat.Entity);

				Assert.ThrowsAsync<FormatException> (async () => await parser.ParseMessageAsync (), "ParseMessageAsync");
			}
		}

		[Test]
		public void TestPartialByteOrderMark ()
		{
			var bom = new byte[] { 0xEF, 0xBB/*, 0xBF */ };

			using (var stream = new MemoryStream ()) {
				stream.Write (bom, 0, bom.Length);

				using (var file = File.OpenRead (Path.Combine (MboxDataDir, "simple.mbox.txt")))
					file.CopyTo (stream, 4096);

				stream.Position = 0;

				var parser = new MimeParser (stream, MimeFormat.Entity);

				Assert.Throws<FormatException> (() => parser.ParseMessage (), "ParseMessage");

				stream.Position = 0;

				parser.SetStream (stream, MimeFormat.Entity);

				Assert.ThrowsAsync<FormatException> (async () => await parser.ParseMessageAsync (), "ParseMessageAsync");
			}
		}

		[Test]
		public void TestByteOrderMarkEOF ()
		{
			var bom = new byte[] { 0xEF, 0xBB, 0xBF };

			using (var stream = new MemoryStream (bom, false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);

				try {
					using var message = parser.ParseMessage ();
					Assert.Fail ("ParseMessage: Parsing an empty stream should fail.");
				} catch (FormatException ex) {
					Assert.That (ex.Message, Is.EqualTo ("End of stream."), "ParseMessage");
				} catch (Exception ex) {
					Assert.Fail ($"ParseMessage: Unexpected exception: {ex}");
				}
			}
		}

		[Test]
		public async Task TestByteOrderMarkEOFAsync ()
		{
			var bom = new byte[] { 0xEF, 0xBB, 0xBF };

			using (var stream = new MemoryStream (bom, false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);

				try {
					using var message = await parser.ParseMessageAsync ();
					Assert.Fail ("ParseMessageAsync: Parsing an empty stream should fail.");
				} catch (FormatException ex) {
					Assert.That (ex.Message, Is.EqualTo ("End of stream."), "ParseMessageAsync");
				} catch (Exception ex) {
					Assert.Fail ($"ParseMessageAsync: Unexpected exception: {ex}");
				}
			}
		}

		[Test]
		public void TestParsingGarbageMbox ()
		{
			using (var stream = new MemoryStream ()) {
				var line = Encoding.ASCII.GetBytes ("This is just a standard test file... nothing to see here. No MIME anywhere to be found\r\n");

				for (int i = 0; i < 200; i++)
					stream.Write (line, 0, line.Length);

				stream.Position = 0;

				var parser = new MimeParser (stream, MimeFormat.Mbox);
				try {
					using var message = parser.ParseMessage ();
				} catch (FormatException ex) {
					Assert.That (ex.Message, Is.EqualTo ("Failed to find mbox From marker."));
				} catch (Exception ex) {
					Assert.Fail ($"Unexpected exception: {ex}");
				}
			}
		}

		[Test]
		public async Task TestParsingGarbageMboxAsync ()
		{
			using (var stream = new MemoryStream ()) {
				var line = Encoding.ASCII.GetBytes ("This is just a standard test file... nothing to see here. No MIME anywhere to be found\r\n");

				for (int i = 0; i < 200; i++)
					stream.Write (line, 0, line.Length);

				stream.Position = 0;

				var parser = new MimeParser (stream, MimeFormat.Mbox);
				try {
					using var message = await parser.ParseMessageAsync ();
				} catch (FormatException ex) {
					Assert.That (ex.Message, Is.EqualTo ("Failed to find mbox From marker."));
				} catch (Exception ex) {
					Assert.Fail ($"Unexpected exception: {ex}");
				}
			}
		}

		[Test]
		public void TestParsingGarbageEntity ()
		{
			using (var stream = new MemoryStream ()) {
				var line = Encoding.ASCII.GetBytes ("This is just a standard test file... nothing to see here. No MIME anywhere to be found\r\n");

				for (int i = 0; i < 200; i++)
					stream.Write (line, 0, line.Length);

				stream.Position = 0;

				var parser = new MimeParser (stream, MimeFormat.Entity);
				try {
					using var entity = parser.ParseEntity ();
				} catch (FormatException ex) {
					Assert.That (ex.Message, Is.EqualTo ("Failed to parse entity headers."));
				} catch (Exception ex) {
					Assert.Fail ($"Unexpected exception: {ex}");
				}
			}
		}

		[Test]
		public async Task TestParsingGarbageEntityAsync ()
		{
			using (var stream = new MemoryStream ()) {
				var line = Encoding.ASCII.GetBytes ("This is just a standard test file... nothing to see here. No MIME anywhere to be found\r\n");

				for (int i = 0; i < 200; i++)
					stream.Write (line, 0, line.Length);

				stream.Position = 0;

				var parser = new MimeParser (stream, MimeFormat.Entity);
				try {
					using var message = await parser.ParseEntityAsync ();
				} catch (FormatException ex) {
					Assert.That (ex.Message, Is.EqualTo ("Failed to parse entity headers."));
				} catch (Exception ex) {
					Assert.Fail ($"Unexpected exception: {ex}");
				}
			}
		}

		[Test]
		public void TestParsingGarbageMessage ()
		{
			using (var stream = new MemoryStream ()) {
				var line = Encoding.ASCII.GetBytes ("This is just a standard test file... nothing to see here. No MIME anywhere to be found\r\n");

				for (int i = 0; i < 200; i++)
					stream.Write (line, 0, line.Length);

				stream.Position = 0;

				var parser = new MimeParser (stream, MimeFormat.Entity);
				try {
					using var message = parser.ParseMessage ();
				} catch (FormatException ex) {
					Assert.That (ex.Message, Is.EqualTo ("Failed to parse message headers."));
				} catch (Exception ex) {
					Assert.Fail ($"Unexpected exception: {ex}");
				}
			}
		}

		[Test]
		public async Task TestParsingGarbageMessageAsync ()
		{
			using (var stream = new MemoryStream ()) {
				var line = Encoding.ASCII.GetBytes ("This is just a standard test file... nothing to see here. No MIME anywhere to be found\r\n");

				for (int i = 0; i < 200; i++)
					stream.Write (line, 0, line.Length);

				stream.Position = 0;

				var parser = new MimeParser (stream, MimeFormat.Entity);
				try {
					using var message = await parser.ParseMessageAsync ();
				} catch (FormatException ex) {
					Assert.That (ex.Message, Is.EqualTo ("Failed to parse message headers."));
				} catch (Exception ex) {
					Assert.Fail ($"Unexpected exception: {ex}");
				}
			}
		}

		[Test]
		public void TestDoubleMboxMarker ()
		{
			var content = Encoding.ASCII.GetBytes ("From - \r\nFrom -\r\nFrom: sender@example.com\r\nTo: recipient@example.com\r\nSubject: test message\r\n\r\nBody text\r\n");

			using (var stream = new MemoryStream (content, false)) {
				var parser = new MimeParser (stream, MimeFormat.Mbox);

				using (var message = parser.ParseMessage ())
					Assert.That (message.Headers.Count, Is.EqualTo (0));

				using (var message = parser.ParseMessage ())
					Assert.That (message.Headers.Count, Is.EqualTo (3));
			}
		}

		[Test]
		public async Task TestDoubleMboxMarkerAsync ()
		{
			var content = Encoding.ASCII.GetBytes ("From - \r\nFrom -\r\nFrom: sender@example.com\r\nTo: recipient@example.com\r\nSubject: test message\r\n\r\nBody text\r\n");

			using (var stream = new MemoryStream (content, false)) {
				var parser = new MimeParser (stream, MimeFormat.Mbox);

				using (var message = await parser.ParseMessageAsync ())
					Assert.That (message.Headers.Count, Is.EqualTo (0));

				using (var message = await parser.ParseMessageAsync ())
					Assert.That (message.Headers.Count, Is.EqualTo (3));
			}
		}

		[Test]
		public void TestReallyLongMboxMarker ()
		{
			var content = Encoding.ASCII.GetBytes ("\r\nFrom: sender@example.com\r\nTo: recipient@example.com\r\nSubject: test message\r\n\r\nBody text\r\n");
			var marker = "From " + new string ('X', 4092);

			using (var stream = new MemoryStream ()) {
				var bytes = Encoding.ASCII.GetBytes (marker);
				stream.Write (bytes, 0, bytes.Length);
				stream.Write (content, 0, content.Length);
				stream.Position = 0;

				var parser = new MimeParser (stream, MimeFormat.Mbox);
				using (var message = parser.ParseMessage ()) {
					Assert.That (message.Headers.Count, Is.EqualTo (3));
					Assert.That (parser.MboxMarker, Is.EqualTo (marker));
				}
			}
		}

		[Test]
		public async Task TestReallyLongMboxMarkerAsync ()
		{
			var content = Encoding.ASCII.GetBytes ("\r\nFrom: sender@example.com\r\nTo: recipient@example.com\r\nSubject: test message\r\n\r\nBody text\r\n");
			var marker = "From " + new string ('X', 4092);

			using (var stream = new MemoryStream ()) {
				var bytes = Encoding.ASCII.GetBytes (marker);
				stream.Write (bytes, 0, bytes.Length);
				stream.Write (content, 0, content.Length);
				stream.Position = 0;

				var parser = new MimeParser (stream, MimeFormat.Mbox);
				using (var message = await parser.ParseMessageAsync ()) {
					Assert.That (message.Headers.Count, Is.EqualTo (3));
					Assert.That (parser.MboxMarker, Is.EqualTo (marker));
				}
			}
		}

		[Test]
		public void TestTruncatedMboxMarker ()
		{
			var bytes = Encoding.ASCII.GetBytes ("From <incomplete mbox marker>");

			using (var memory = new MemoryStream (bytes, false)) {
				var parser = new MimeParser (memory, MimeFormat.Mbox);

				Assert.Throws<FormatException> (() => parser.ParseMessage ());
			}
		}

		[Test]
		public void TestTruncatedMboxMarkerAsync ()
		{
			var bytes = Encoding.ASCII.GetBytes ("From <incomplete mbox marker>");

			using (var memory = new MemoryStream (bytes, false)) {
				var parser = new MimeParser (memory, MimeFormat.Mbox);

				Assert.ThrowsAsync<FormatException> (async () => await parser.ParseMessageAsync ());
			}
		}

		[TestCase ("From \r")]
		[TestCase ("From <incomplete mbox marker>\r")]
		[TestCase ("From \0: \x01\0\x10\0\r\xff\r\r")]
		public void TestTruncatedMboxMarkerEndingWithCarriageReturn (string text)
		{
			var bytes = Encoding.Latin1.GetBytes (text);

			// Note: This used to loop forever because the trailing '\r' was never consumed.
			var task = Task.Run (() => {
				using (var memory = new MemoryStream (bytes, false)) {
					var parser = new MimeParser (memory, MimeFormat.Mbox);

					Assert.Throws<FormatException> (() => parser.ParseMessage ());
				}
			});

			Assert.That (task.Wait (TimeSpan.FromSeconds (30)), Is.True, "Parsing the truncated mbox marker did not finish.");
		}

		[TestCase ("From \r")]
		[TestCase ("From <incomplete mbox marker>\r")]
		[TestCase ("From \0: \x01\0\x10\0\r\xff\r\r")]
		public async Task TestTruncatedMboxMarkerEndingWithCarriageReturnAsync (string text)
		{
			var bytes = Encoding.Latin1.GetBytes (text);

			// Note: This used to loop forever because the trailing '\r' was never consumed.
			var task = Task.Run (() => {
				using (var memory = new MemoryStream (bytes, false)) {
					var parser = new MimeParser (memory, MimeFormat.Mbox);

					Assert.ThrowsAsync<FormatException> (async () => await parser.ParseMessageAsync ());
				}
			});

			var completed = await Task.WhenAny (task, Task.Delay (TimeSpan.FromSeconds (30)));

			Assert.That (completed, Is.SameAs (task), "Parsing the truncated mbox marker did not finish.");
			await task;
		}

		[Test]
		public void TestEmptyMboxStream ()
		{
			using (var memory = new MemoryStream (Array.Empty<byte> (), false)) {
				try {
					var parser = new MimeParser (memory, MimeFormat.Mbox);
					using var message = parser.ParseMessage ();

					Assert.Fail ("Parsing an empty stream should fail.");
				} catch (FormatException ex) {
					Assert.That (ex.Message, Is.EqualTo ("End of stream."));
				} catch (Exception ex) {
					Assert.Fail ($"Unexpected exception: {ex}");
				}
			}
		}

		[Test]
		public async Task TestEmptyMboxStreamAsync ()
		{
			using (var memory = new MemoryStream (Array.Empty<byte> (), false)) {
				try {
					var parser = new MimeParser (memory, MimeFormat.Mbox);
					using var message = await parser.ParseMessageAsync ();

					Assert.Fail ("Parsing an empty stream should fail.");
				} catch (FormatException ex) {
					Assert.That (ex.Message, Is.EqualTo ("End of stream."));
				} catch (Exception ex) {
					Assert.Fail ($"Unexpected exception: {ex}");
				}
			}
		}

		[Test]
		public void TestEmptyEntityStream ()
		{
			using (var memory = new MemoryStream (Array.Empty<byte> (), false)) {
				try {
					var parser = new MimeParser (memory, MimeFormat.Entity);
					using var entity = parser.ParseEntity ();

					Assert.Fail ("Parsing an empty stream should fail.");
				} catch (FormatException ex) {
					Assert.That (ex.Message, Is.EqualTo ("End of stream."));
				} catch (Exception ex) {
					Assert.Fail ($"Unexpected exception: {ex}");
				}
			}
		}

		[Test]
		public async Task TestEmptyEntityStreamAsync ()
		{
			using (var memory = new MemoryStream (Array.Empty<byte> (), false)) {
				try {
					var parser = new MimeParser (memory, MimeFormat.Entity);
					using var entity = await parser.ParseEntityAsync ();

					Assert.Fail ("Parsing an empty stream should fail.");
				} catch (FormatException ex) {
					Assert.That (ex.Message, Is.EqualTo ("End of stream."));
				} catch (Exception ex) {
					Assert.Fail ($"Unexpected exception: {ex}");
				}
			}
		}

		[Test]
		public void TestEmptyMessageStream ()
		{
			using (var memory = new MemoryStream (Array.Empty<byte> (), false)) {
				try {
					var parser = new MimeParser (memory, MimeFormat.Entity);
					using var message = parser.ParseMessage ();

					Assert.Fail ("Parsing an empty stream should fail.");
				} catch (FormatException ex) {
					Assert.That (ex.Message, Is.EqualTo ("End of stream."));
				} catch (Exception ex) {
					Assert.Fail ($"Unexpected exception: {ex}");
				}
			}
		}

		[Test]
		public async Task TestEmptyMessageStreamAsync ()
		{
			using (var memory = new MemoryStream (Array.Empty<byte> (), false)) {
				try {
					var parser = new MimeParser (memory, MimeFormat.Entity);
					using var message = await parser.ParseMessageAsync ();

					Assert.Fail ("Parsing an empty stream should fail.");
				} catch (FormatException ex) {
					Assert.That (ex.Message, Is.EqualTo ("End of stream."));
				} catch (Exception ex) {
					Assert.Fail ($"Unexpected exception: {ex}");
				}
			}
		}

		[TestCase ("From sender@example.com Mon Jan  1 00:00:00 2024", MimeFormat.Entity)]
		[TestCase ("From sender@example.com Mon Jan  1 00:00:00 2024\r\n", MimeFormat.Entity)]
		[TestCase ("From sender@example.com Mon Jan  1 00:00:00 2024\r\n>From sender@example.com Mon Jan  1 00:00:00 2024\r\n", MimeFormat.Entity)]
		[TestCase ("From sender@example.com Mon Jan  1 00:00:00 2024\r\n>From sender@example.com Mon Jan  1 00:00:00 2024", MimeFormat.Mbox)]
		public void TestMessageStreamWithOnlyMboxMarkers (string text, MimeFormat format)
		{
			using (var memory = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (memory, format);

				var ex = Assert.Throws<FormatException> (() => parser.ParseMessage ());
				Assert.That (ex.Message, Is.EqualTo ("End of stream."));
			}
		}

		[TestCase ("From sender@example.com Mon Jan  1 00:00:00 2024", MimeFormat.Entity)]
		[TestCase ("From sender@example.com Mon Jan  1 00:00:00 2024\r\n", MimeFormat.Entity)]
		[TestCase ("From sender@example.com Mon Jan  1 00:00:00 2024\r\n>From sender@example.com Mon Jan  1 00:00:00 2024\r\n", MimeFormat.Entity)]
		[TestCase ("From sender@example.com Mon Jan  1 00:00:00 2024\r\n>From sender@example.com Mon Jan  1 00:00:00 2024", MimeFormat.Mbox)]
		public void TestMessageStreamWithOnlyMboxMarkersAsync (string text, MimeFormat format)
		{
			using (var memory = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (memory, format);

				var ex = Assert.ThrowsAsync<FormatException> (() => parser.ParseMessageAsync ());
				Assert.That (ex.Message, Is.EqualTo ("End of stream."));
			}
		}

		[TestCase ("From sender@example.com Mon Jan  1 00:00:00 2024\r\nSubject: test", "Subject: test")]
		[TestCase ("From sender@example.com Mon Jan  1 00:00:00 2024\r\n\r\n", "\r\n")]
		public async Task TestMessageStreamWithMboxMarkerFollowedByMessage (string text, string expected)
		{
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;

			foreach (var async in new[] { false, true }) {
				using (var memory = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
					var parser = new MimeParser (memory, MimeFormat.Entity);
					using var message = async ? await parser.ParseMessageAsync () : parser.ParseMessage ();
					using var output = new MemoryStream ();

					message.WriteTo (options, output);

					Assert.That (Encoding.ASCII.GetString (output.ToArray ()), Is.EqualTo (expected), async ? "ParseMessageAsync" : "ParseMessage");
				}
			}
		}

		[Test]
		public void TestEmptyMessage ()
		{
			var bytes = Encoding.ASCII.GetBytes ("\r\n");

			using (var memory = new MemoryStream (bytes, false)) {
				try {
					var parser = new MimeParser (memory, MimeFormat.Entity);
					using var message = parser.ParseMessage ();

					Assert.That (message.Headers.Count, Is.EqualTo (0), "Unexpected header count.");
				} catch (Exception ex) {
					Assert.Fail ($"Failed to parse message: {ex}");
				}
			}
		}

		[Test]
		public async Task TestEmptyMessageAsync ()
		{
			var bytes = Encoding.ASCII.GetBytes ("\r\n");

			using (var memory = new MemoryStream (bytes, false)) {
				try {
					var parser = new MimeParser (memory, MimeFormat.Entity);
					using var message = await parser.ParseMessageAsync ();

					Assert.That (message.Headers.Count, Is.EqualTo (0), "Unexpected header count.");
				} catch (Exception ex) {
					Assert.Fail ($"Failed to parse message: {ex}");
				}
			}
		}

		[TestCase ("garbage")]
		[TestCase ("!%^#&^!\\t  ")]
		public void TestInvalidContentType (string mimeType)
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of recovery from invalid media-type in Content-Type header
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: ${mimeType}; charset=utf-8

This is the message body.
".Replace ("${mimeType}", mimeType).Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<MimePart> (), "Expected top-level to be a MimePart");
				var part = (MimePart) message.Body;
				Assert.That (part.ContentType.MimeType, Is.EqualTo ("application/octet-stream"), "Expected application/octet-stream");
				Assert.That (part.ContentType.Charset, Is.EqualTo ("utf-8"), "Expected to keep Content-Type parameters");

				var body = new TextPart ("plain") {
					Content = part.Content
				};

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				AssertSerialization (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<MimePart> (), "Expected top-level to be a MimePart");
				var part = (MimePart) message.Body;
				Assert.That (part.ContentType.MimeType, Is.EqualTo ("application/octet-stream"), "Expected application/octet-stream");
				Assert.That (part.ContentType.Charset, Is.EqualTo ("utf-8"), "Expected to keep Content-Type parameters");

				var body = new TextPart ("plain") {
					Content = part.Content
				};

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				AssertSerialization (message, NewLineFormat.Dos, text);
			}
		}

		[TestCase ("garbage")]
		[TestCase ("!%^#&^!\\t  ")]
		public async Task TestInvalidContentTypeAsync (string mimeType)
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of recovery from invalid media-type in Content-Type header
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: ${mimeType}; charset=utf-8

This is the message body.
".Replace ("${mimeType}", mimeType).Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<MimePart> (), "Expected top-level to be a MimePart");
				var part = (MimePart) message.Body;
				Assert.That (part.ContentType.MimeType, Is.EqualTo ("application/octet-stream"), "Expected application/octet-stream");
				Assert.That (part.ContentType.Charset, Is.EqualTo ("utf-8"), "Expected to keep Content-Type parameters");

				var body = new TextPart ("plain") {
					Content = part.Content
				};

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				await AssertSerializationAsync (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<MimePart> (), "Expected top-level to be a MimePart");
				var part = (MimePart) message.Body;
				Assert.That (part.ContentType.MimeType, Is.EqualTo ("application/octet-stream"), "Expected application/octet-stream");
				Assert.That (part.ContentType.Charset, Is.EqualTo ("utf-8"), "Expected to keep Content-Type parameters");

				var body = new TextPart ("plain") {
					Content = part.Content
				};

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				await AssertSerializationAsync (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestHeaderFieldNameBeginsWithColon ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of a header line starting with ':'
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: text/plain; charset=utf-8
: What header is this?

This is the message body.
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<TextPart> (), "Expected top-level to be a TextPart");
				var header = message.Headers[message.Headers.Count - 1];

				// FIXME: Should this really be "valid"?
				Assert.That (header.IsInvalid, Is.False, "IsInvalid is expected to be false");
				Assert.That (header.Field, Is.EqualTo (string.Empty), "Field is expected to be empty");
				Assert.That (header.Value, Is.EqualTo ("What header is this?"));

				var body = (TextPart) message.Body;
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected text/plain");
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"), "Expected to keep Content-Type parameters");
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				AssertSerialization (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<TextPart> (), "Expected top-level to be a TextPart");
				var header = message.Headers[message.Headers.Count - 1];

				// FIXME: Should this really be "valid"?
				Assert.That (header.IsInvalid, Is.False, "IsInvalid is expected to be false");
				Assert.That (header.Field, Is.EqualTo (string.Empty), "Field is expected to be empty");
				Assert.That (header.Value, Is.EqualTo ("What header is this?"));

				var body = (TextPart) message.Body;
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected text/plain");
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"), "Expected to keep Content-Type parameters");
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				AssertSerialization (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestHeaderFieldNameBeginsWithColonAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of a header line starting with ':'
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: text/plain; charset=utf-8
: What header is this?

This is the message body.
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<TextPart> (), "Expected top-level to be a TextPart");
				var header = message.Headers[message.Headers.Count - 1];

				// FIXME: Should this really be "valid"?
				Assert.That (header.IsInvalid, Is.False, "IsInvalid is expected to be false");
				Assert.That (header.Field, Is.EqualTo (string.Empty), "Field is expected to be empty");
				Assert.That (header.Value, Is.EqualTo ("What header is this?"));

				var body = (TextPart) message.Body;
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected text/plain");
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"), "Expected to keep Content-Type parameters");
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				await AssertSerializationAsync (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<TextPart> (), "Expected top-level to be a TextPart");
				var header = message.Headers[message.Headers.Count - 1];

				// FIXME: Should this really be "valid"?
				Assert.That (header.IsInvalid, Is.False, "IsInvalid is expected to be false");
				Assert.That (header.Field, Is.EqualTo (string.Empty), "Field is expected to be empty");
				Assert.That (header.Value, Is.EqualTo ("What header is this?"));

				var body = (TextPart) message.Body;
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected text/plain");
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"), "Expected to keep Content-Type parameters");
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				await AssertSerializationAsync (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestHeaderFieldNameColonColon ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of a Content-Transfer-Encoding header with double ':'s
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: text/plain; charset=utf-8
Content-Transfer-Encoding:: base64
Content-Disposition: inline; name=body.txt

This is the message body.
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<TextPart> (), "Expected top-level to be a TextPart");
				var body = (TextPart) message.Body;

				var header = body.Headers[body.Headers.Count - 2];

				Assert.That (header.Id, Is.EqualTo (HeaderId.ContentTransferEncoding), "Expected Content-Transfer-Encoding header");
				Assert.That (header.IsInvalid, Is.False, "IsInvalid is expected to be false");
				Assert.That (header.Value, Is.EqualTo (": base64"));
				Assert.That (body.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Unspecified), "Expected Content-Transfer-Encoding to be Unspecified");

				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected text/plain");
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"), "Expected to keep Content-Type parameters");

				Assert.That (body.ContentDisposition, Is.Not.Null, "Expected Content-Disposition to not be null");
				Assert.That (body.ContentDisposition.Disposition, Is.EqualTo ("inline"), "Expected Content-Disposition to be inline");
				Assert.That (body.ContentDisposition.Parameters["name"], Is.EqualTo ("body.txt"), "Expected Content-Disposition name parameter to be body.txt");

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				AssertSerialization (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<TextPart> (), "Expected top-level to be a TextPart");
				var body = (TextPart) message.Body;

				var header = body.Headers[body.Headers.Count - 2];

				Assert.That (header.Id, Is.EqualTo (HeaderId.ContentTransferEncoding), "Expected Content-Transfer-Encoding header");
				Assert.That (header.IsInvalid, Is.False, "IsInvalid is expected to be false");
				Assert.That (header.Value, Is.EqualTo (": base64"));
				Assert.That (body.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Unspecified), "Expected Content-Transfer-Encoding to be Unspecified");

				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected text/plain");
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"), "Expected to keep Content-Type parameters");

				Assert.That (body.ContentDisposition, Is.Not.Null, "Expected Content-Disposition to not be null");
				Assert.That (body.ContentDisposition.Disposition, Is.EqualTo ("inline"), "Expected Content-Disposition to be inline");
				Assert.That (body.ContentDisposition.Parameters["name"], Is.EqualTo ("body.txt"), "Expected Content-Disposition name parameter to be body.txt");

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				AssertSerialization (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestHeaderFieldNameColonColonAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of a Content-Transfer-Encoding header with double ':'s
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: text/plain; charset=utf-8
Content-Transfer-Encoding:: base64
Content-Disposition: inline; name=body.txt

This is the message body.
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<TextPart> (), "Expected top-level to be a TextPart");
				var body = (TextPart) message.Body;

				var header = body.Headers[body.Headers.Count - 2];

				Assert.That (header.Id, Is.EqualTo (HeaderId.ContentTransferEncoding), "Expected Content-Transfer-Encoding header");
				Assert.That (header.IsInvalid, Is.False, "IsInvalid is expected to be false");
				Assert.That (header.Value, Is.EqualTo (": base64"));
				Assert.That (body.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Unspecified), "Expected Content-Transfer-Encoding to be Unspecified");

				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected text/plain");
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"), "Expected to keep Content-Type parameters");

				Assert.That (body.ContentDisposition, Is.Not.Null, "Expected Content-Disposition to not be null");
				Assert.That (body.ContentDisposition.Disposition, Is.EqualTo ("inline"), "Expected Content-Disposition to be inline");
				Assert.That (body.ContentDisposition.Parameters["name"], Is.EqualTo ("body.txt"), "Expected Content-Disposition name parameter to be body.txt");

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				await AssertSerializationAsync (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<TextPart> (), "Expected top-level to be a TextPart");
				var body = (TextPart) message.Body;

				var header = body.Headers[body.Headers.Count - 2];

				Assert.That (header.Id, Is.EqualTo (HeaderId.ContentTransferEncoding), "Expected Content-Transfer-Encoding header");
				Assert.That (header.IsInvalid, Is.False, "IsInvalid is expected to be false");
				Assert.That (header.Value, Is.EqualTo (": base64"));
				Assert.That (body.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Unspecified), "Expected Content-Transfer-Encoding to be Unspecified");

				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected text/plain");
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"), "Expected to keep Content-Type parameters");

				Assert.That (body.ContentDisposition, Is.Not.Null, "Expected Content-Disposition to not be null");
				Assert.That (body.ContentDisposition.Disposition, Is.EqualTo ("inline"), "Expected Content-Disposition to be inline");
				Assert.That (body.ContentDisposition.Parameters["name"], Is.EqualTo ("body.txt"), "Expected Content-Disposition name parameter to be body.txt");

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				await AssertSerializationAsync (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestMultipartTruncatedAtEndOfFirstBoundary ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of multipart truncated at the end of the first boundary
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (0));

				AssertSerialization (message, NewLineFormat.Unix, text);
				AssertSerialization (message, NewLineFormat.Unix, text + "\n", true);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (0));

				AssertSerialization (message, NewLineFormat.Dos, text);
				AssertSerialization (message, NewLineFormat.Dos, text + "\r\n", true);
			}
		}

		[Test]
		public async Task TestMultipartTruncatedAtEndOfFirstBoundaryAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of multipart truncated at the end of the first boundary
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (0));

				await AssertSerializationAsync (message, NewLineFormat.Unix, text);
				await AssertSerializationAsync (message, NewLineFormat.Unix, text + "\n", true);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (0));

				await AssertSerializationAsync (message, NewLineFormat.Dos, text);
				await AssertSerializationAsync (message, NewLineFormat.Dos, text + "\r\n", true);
			}
		}

		[Test]
		public void TestMultipartTruncatedAtEndOfSecondBoundary ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of a multipart truncated at the end of the second boundary
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the message body.

------=_NextPart_000_003F_01CE98CE.6E826F90".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1));
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				AssertSerialization (message, NewLineFormat.Unix, text);
				AssertSerialization (message, NewLineFormat.Unix, text + "\n", true);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1));
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				AssertSerialization (message, NewLineFormat.Dos, text);
				AssertSerialization (message, NewLineFormat.Dos, text + "\r\n", true);
			}
		}

		[Test]
		public async Task TestMultipartTruncatedAtEndOfSecondBoundaryAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of a multipart truncated at the end of the second boundary
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the message body.

------=_NextPart_000_003F_01CE98CE.6E826F90".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1));
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				await AssertSerializationAsync (message, NewLineFormat.Unix, text);
				await AssertSerializationAsync (message, NewLineFormat.Unix, text + "\n", true);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1));
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				await AssertSerializationAsync (message, NewLineFormat.Dos, text);
				await AssertSerializationAsync (message, NewLineFormat.Dos, text + "\r\n", true);
			}
		}

		[Test]
		public void TestMultipartTruncatedImmediatelyAfterFirstBoundary ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of multipart truncated immedately after first boundary
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1));
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Text, Is.EqualTo (string.Empty));

				AssertSerialization (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1));
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Text, Is.EqualTo (string.Empty));

				AssertSerialization (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestMultipartTruncatedImmediatelyAfterFirstBoundaryAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of multipart truncated immedately after first boundary
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1));
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Text, Is.EqualTo (string.Empty));

				await AssertSerializationAsync (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1));
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Text, Is.EqualTo (string.Empty));

				await AssertSerializationAsync (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestMultipartTruncatedImmediatelyAfterSecondBoundary ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of a multipart truncated immediately after the second boundary
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the message body.

------=_NextPart_000_003F_01CE98CE.6E826F90
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Text, Is.EqualTo (string.Empty));

				AssertSerialization (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Text, Is.EqualTo (string.Empty));

				AssertSerialization (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestMultipartTruncatedImmediatelyAfterSecondBoundaryAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of a multipart truncated immediately after the second boundary
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the message body.

------=_NextPart_000_003F_01CE98CE.6E826F90
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Text, Is.EqualTo (string.Empty));

				await AssertSerializationAsync (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Text, Is.EqualTo (string.Empty));

				await AssertSerializationAsync (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestMultipartBoundaryWithoutTrailingNewline ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of multipart boundary w/o trailing newline
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the message body.

------=_NextPart_000_003F_01CE98CE.6E826F90".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1));
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				AssertSerialization (message, NewLineFormat.Unix, text);
				AssertSerialization (message, NewLineFormat.Unix, text + "\n", true);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1));
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				AssertSerialization (message, NewLineFormat.Dos, text);
				AssertSerialization (message, NewLineFormat.Dos, text + "\r\n", true);
			}
		}

		[Test]
		public async Task TestMultipartBoundaryWithoutTrailingNewlineAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of multipart boundary w/o trailing newline
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the message body.

------=_NextPart_000_003F_01CE98CE.6E826F90".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				await AssertSerializationAsync (message, NewLineFormat.Unix, text);
				await AssertSerializationAsync (message, NewLineFormat.Unix, text + "\n", true);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				await AssertSerializationAsync (message, NewLineFormat.Dos, text);
				await AssertSerializationAsync (message, NewLineFormat.Dos, text + "\r\n", true);
			}
		}

		[Test]
		public void TestTruncatedMultipartSubpartHeaders ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of truncated multipart subpart headers
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				AssertSerialization (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				AssertSerialization (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestTruncatedMultipartSubpartHeadersAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of truncated multipart subpart headers
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				await AssertSerializationAsync (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				await AssertSerializationAsync (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestTruncatedMultipartSubpartHeaderFieldName ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of truncated multipart subpart header field name
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8
Content-Dis".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers.Count, Is.EqualTo (2), "Expected 2 headers");
				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Headers[1].IsInvalid, Is.True);
				Assert.That (body.Headers[1].Field, Is.EqualTo ("Content-Dis"));
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				AssertSerialization (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers.Count, Is.EqualTo (2), "Expected 2 headers");
				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Headers[1].IsInvalid, Is.True);
				Assert.That (body.Headers[1].Field, Is.EqualTo ("Content-Dis"));
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				AssertSerialization (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestTruncatedMultipartSubpartHeaderFieldNameAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of truncated multipart subpart header field name
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8
Content-Dis".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers.Count, Is.EqualTo (2), "Expected 2 headers");
				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Headers[1].IsInvalid, Is.True);
				Assert.That (body.Headers[1].Field, Is.EqualTo ("Content-Dis"));
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				await AssertSerializationAsync (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers.Count, Is.EqualTo (2), "Expected 2 headers");
				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Headers[1].IsInvalid, Is.True);
				Assert.That (body.Headers[1].Field, Is.EqualTo ("Content-Dis"));
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				await AssertSerializationAsync (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestMultipartSubpartHeadersEndWithBoundary ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of multipart subpart headers ending with a boundary
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8
------=_NextPart_000_003F_01CE98CE.6E826F90--
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				AssertSerialization (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				AssertSerialization (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestMultipartSubpartHeadersEndWithBoundaryAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of multipart subpart headers ending with a boundary
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8
------=_NextPart_000_003F_01CE98CE.6E826F90--
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				await AssertSerializationAsync (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				await AssertSerializationAsync (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestMultipartSubpartHeadersLineStartsWithDashDash ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of multipart subpart headers line starting with --
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8
--not-the-boundary-muhahaha

This is the message body.

------=_NextPart_000_003F_01CE98CE.6E826F90--
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Headers.Count, Is.EqualTo (2));
				Assert.That (body.Headers[1].IsInvalid, Is.True, "IsInvalid");
				Assert.That (body.Headers[1].Field, Is.EqualTo ("--not-the-boundary-muhahaha\n"));

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				AssertSerialization (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Headers.Count, Is.EqualTo (2));
				Assert.That (body.Headers[1].IsInvalid, Is.True, "IsInvalid");
				Assert.That (body.Headers[1].Field, Is.EqualTo ("--not-the-boundary-muhahaha\r\n"));

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				AssertSerialization (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestMultipartSubpartHeadersLineStartsWithDashDashAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of multipart subpart headers line starting with --
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8
--not-the-boundary-muhahaha

This is the message body.

------=_NextPart_000_003F_01CE98CE.6E826F90--
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Headers.Count, Is.EqualTo (2));
				Assert.That (body.Headers[1].IsInvalid, Is.True, "IsInvalid");
				Assert.That (body.Headers[1].Field, Is.EqualTo ("--not-the-boundary-muhahaha\n"));

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				await AssertSerializationAsync (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Headers.Count, Is.EqualTo (2));
				Assert.That (body.Headers[1].IsInvalid, Is.True, "IsInvalid");
				Assert.That (body.Headers[1].Field, Is.EqualTo ("--not-the-boundary-muhahaha\r\n"));

				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				await AssertSerializationAsync (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestMultipartSubpartHeadersLineStartsWithDashDashEOF ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of multipart subpart headers line starting with -- <EOF>
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8
--not-the-boundary-muhahaha".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Headers.Count, Is.EqualTo (2));
				Assert.That (body.Headers[1].IsInvalid, Is.True, "IsInvalid");
				Assert.That (body.Headers[1].Field, Is.EqualTo ("--not-the-boundary-muhahaha"));

				Assert.That (body.Text, Is.EqualTo (string.Empty));

				AssertSerialization (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Headers.Count, Is.EqualTo (2));
				Assert.That (body.Headers[1].IsInvalid, Is.True, "IsInvalid");
				Assert.That (body.Headers[1].Field, Is.EqualTo ("--not-the-boundary-muhahaha"));

				Assert.That (body.Text, Is.EqualTo (string.Empty));

				AssertSerialization (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestMultipartSubpartHeadersLineStartsWithDashDashEOFAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of multipart subpart headers line starting with -- <EOF>
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8
--not-the-boundary-muhahaha".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Headers.Count, Is.EqualTo (2));
				Assert.That (body.Headers[1].IsInvalid, Is.True, "IsInvalid");
				Assert.That (body.Headers[1].Field, Is.EqualTo ("--not-the-boundary-muhahaha"));

				Assert.That (body.Text, Is.EqualTo (string.Empty));

				await AssertSerializationAsync (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1), "Expected 1 child");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Headers.Count, Is.EqualTo (2));
				Assert.That (body.Headers[1].IsInvalid, Is.True, "IsInvalid");
				Assert.That (body.Headers[1].Field, Is.EqualTo ("--not-the-boundary-muhahaha"));

				Assert.That (body.Text, Is.EqualTo (string.Empty));

				await AssertSerializationAsync (message, NewLineFormat.Dos, text);
			}
		}

		const string DelimiterNewLineHeaders = "From: mimekit@example.com\nTo: mimekit@example.com\nSubject: delimiter new-line test\nMIME-Version: 1.0\n";

		static readonly string[] DelimiterNewLineMessages = {
			// a nested multipart that is truncated by the outer boundary
			"Content-Type: multipart/mixed; boundary=\"outer\"\n\n--outer\nContent-Type: multipart/alternative; boundary=\"inner\"\n\n--inner\nContent-Type: text/plain\n\ninner\n--outer\nContent-Type: text/plain\n\nlast\n--outer--\n",
			// a nested multipart that is truncated by the outer boundary and whose last child has no body
			"Content-Type: multipart/mixed; boundary=\"outer\"\n\n--outer\nContent-Type: multipart/alternative; boundary=\"inner\"\n\n--inner\nContent-Type: text/plain\n--outer\nContent-Type: text/plain\n\nlast\n--outer--\n",
			// a nested multipart that is truncated by the outer boundary right after a boundary marker
			"Content-Type: multipart/mixed; boundary=\"outer\"\n\n--outer\nContent-Type: multipart/alternative; boundary=\"inner\"\n\n--inner\nContent-Type: text/plain\n\ninner\n--inner\n--outer\nContent-Type: text/plain\n\nlast\n--outer--\n",
			// a nested multipart with only a preamble that is truncated by the outer boundary
			"Content-Type: multipart/mixed; boundary=\"outer\"\n\n--outer\nContent-Type: multipart/alternative; boundary=\"inner\"\n\npreamble\n--outer\nContent-Type: text/plain\n\nlast\n--outer--\n",
			// a nested multipart with neither a preamble nor any children that is truncated by the outer boundary
			"Content-Type: multipart/mixed; boundary=\"outer\"\n\n--outer\nContent-Type: multipart/alternative; boundary=\"inner\"\n--outer\nContent-Type: text/plain\n\nlast\n--outer--\n",
			// a nested message/rfc822 whose multipart body is truncated by the outer boundary
			"Content-Type: multipart/mixed; boundary=\"outer\"\n\n--outer\nContent-Type: message/rfc822\n\nSubject: embedded\nContent-Type: multipart/alternative; boundary=\"inner\"\n\n--inner\nContent-Type: text/plain\n\ninner\n--outer--\n",
			// children without a body (with and without a blank line after the headers)
			"Content-Type: multipart/mixed; boundary=\"b\"\n\n--b\nContent-Type: text/plain\n--b\nContent-Type: text/plain\n\n--b\nContent-Type: message/rfc822\n\n--b--\n",
			// a double boundary
			"Content-Type: multipart/mixed; boundary=\"b\"\n\n--b\n--b\nContent-Type: text/plain\n\nbody\n--b--\n",
			// a multipart truncated by the end of the stream
			"Content-Type: multipart/mixed; boundary=\"b\"\n\n--b\nContent-Type: text/plain\n\nbody\n",
			"Content-Type: multipart/mixed; boundary=\"b\"\n\n--b\nContent-Type: text/plain\n\nbody\n\n",
			"Content-Type: multipart/mixed; boundary=\"b\"\n\n--b\nContent-Type: text/plain\n\nbody",
			// a multipart truncated by the end of the stream right after a boundary marker
			"Content-Type: multipart/mixed; boundary=\"b\"\n\n--b\nContent-Type: text/plain\n\nbody\n--b\n",
			"Content-Type: multipart/mixed; boundary=\"b\"\n\n--b\nContent-Type: text/plain\n\nbody\n--b",
			// a multipart truncated by the end of the stream whose last child has no body
			"Content-Type: multipart/mixed; boundary=\"b\"\n\n--b\nContent-Type: text/plain\n\nbody\n--b\nContent-Type: text/plain\n",
			"Content-Type: multipart/mixed; boundary=\"b\"\n\n--b\nContent-Type: text/plain\n\nbody\n--b\nContent-Type: text/plain\n\n",
			// a multipart truncated by the end of the stream with only a preamble
			"Content-Type: multipart/mixed; boundary=\"b\"\n\npreamble\n",
			"Content-Type: multipart/mixed; boundary=\"b\"\n\npreamble",
			// a close delimiter at the end of the stream (with and without an epilogue or a trailing new-line)
			"Content-Type: multipart/mixed; boundary=\"b\"\n\n--b\nContent-Type: text/plain\n\nbody\n--b--",
			"Content-Type: multipart/mixed; boundary=\"b\"\n\n--b\nContent-Type: text/plain\n\nbody\n--b--\n\n",
			"Content-Type: multipart/mixed; boundary=\"b\"\n\n--b\nContent-Type: text/plain\n\nbody\n--b--\nepilogue",
			"Content-Type: multipart/mixed; boundary=\"outer\"\n\n--outer\nContent-Type: multipart/alternative; boundary=\"inner\"\n\n--inner\nContent-Type: text/plain\n\ninner\n--inner--\n--outer--",
		};

		static IEnumerable<TestCaseData> DelimiterNewLineTestCases ()
		{
			for (int i = 0; i < DelimiterNewLineMessages.Length; i++) {
				yield return new TestCaseData (i, NewLineFormat.Unix);
				yield return new TestCaseData (i, NewLineFormat.Dos);
			}
		}

		static string GetDelimiterNewLineMessage (int index, NewLineFormat format, out string ensured)
		{
			var newLine = format == NewLineFormat.Dos ? "\r\n" : "\n";
			var text = (DelimiterNewLineHeaders + DelimiterNewLineMessages[index]).Replace ("\n", newLine);

			ensured = text.EndsWith ("\n", StringComparison.Ordinal) ? text : text + newLine;

			return text;
		}

		[TestCaseSource (nameof (DelimiterNewLineTestCases))]
		public void TestDelimiterNewLineRoundTrip (int index, NewLineFormat format)
		{
			var text = GetDelimiterNewLineMessage (index, format, out var ensured);

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				AssertSerialization (message, format, text);
				AssertSerialization (message, format, ensured, true);

				// make sure that EnsureNewLine did not leave any state behind
				AssertSerialization (message, format, text);
			}
		}

		[TestCaseSource (nameof (DelimiterNewLineTestCases))]
		public async Task TestDelimiterNewLineRoundTripAsync (int index, NewLineFormat format)
		{
			var text = GetDelimiterNewLineMessage (index, format, out var ensured);

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				await AssertSerializationAsync (message, format, text);
				await AssertSerializationAsync (message, format, ensured, true);

				// make sure that EnsureNewLine did not leave any state behind
				await AssertSerializationAsync (message, format, text);
			}
		}

		static readonly string MultipartWithoutEndBoundaryOrTrailingNewLine = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of a truncated multipart w/o a trailing newline
MIME-Version: 1.0
Content-Type: multipart/mixed; boundary=""outer""

--outer
Content-Type: text/plain

This is the first part.

--outer
Content-Type: message/rfc822

From: mimekit@example.com
Subject: embedded
Content-Type: multipart/alternative; boundary=""inner""

--inner
Content-Type: text/plain

This is the last part.".Replace ("\r\n", "\n");

		[TestCase (NewLineFormat.Unix)]
		[TestCase (NewLineFormat.Dos)]
		public void TestMultipartWithoutEndBoundaryOrTrailingNewLine (NewLineFormat format)
		{
			var newLine = format == NewLineFormat.Dos ? "\r\n" : "\n";
			var text = MultipartWithoutEndBoundaryOrTrailingNewLine.Replace ("\n", newLine);

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2));

				var rfc822 = (MessagePart) multipart[1];
				var alternative = (Multipart) rfc822.Message.Body;
				Assert.That (alternative.Count, Is.EqualTo (1));
				Assert.That (((TextPart) alternative[0]).Text, Is.EqualTo ("This is the last part."));

				// Note: the output should match the input exactly unless FormatOptions.EnsureNewLine is set.
				AssertSerialization (message, format, text);
				AssertSerialization (message, format, text + newLine, true);
			}
		}

		[TestCase (NewLineFormat.Unix)]
		[TestCase (NewLineFormat.Dos)]
		public async Task TestMultipartWithoutEndBoundaryOrTrailingNewLineAsync (NewLineFormat format)
		{
			var newLine = format == NewLineFormat.Dos ? "\r\n" : "\n";
			var text = MultipartWithoutEndBoundaryOrTrailingNewLine.Replace ("\n", newLine);

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2));

				var rfc822 = (MessagePart) multipart[1];
				var alternative = (Multipart) rfc822.Message.Body;
				Assert.That (alternative.Count, Is.EqualTo (1));
				Assert.That (((TextPart) alternative[0]).Text, Is.EqualTo ("This is the last part."));

				// Note: the output should match the input exactly unless FormatOptions.EnsureNewLine is set.
				await AssertSerializationAsync (message, format, text);
				await AssertSerializationAsync (message, format, text + newLine, true);
			}
		}

		[Test]
		public void TestMultipartWithoutEndBoundary ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of a multipart without an end boundary
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the first part.

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the second part.
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));

				AssertSerialization (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));

				AssertSerialization (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestMultipartWithoutEndBoundaryAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of a multipart without an end boundary
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the first part.

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the second part.
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));

				await AssertSerializationAsync (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));

				await AssertSerializationAsync (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestMultipartBoundaryLineWithTrailingSpaces ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of a multipart boundary followed by trailing whitespace
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90   
Content-Type: text/plain; charset=utf-8

This is the first part.

------=_NextPart_000_003F_01CE98CE.6E826F90       
Content-Type: text/plain; charset=utf-8

This is the second part.

------=_NextPart_000_003F_01CE98CE.6E826F90--  
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));

				AssertSerialization (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));

				AssertSerialization (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestMultipartBoundaryLineWithTrailingSpacesAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of a multipart boundary followed by trailing whitespace
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90   
Content-Type: text/plain; charset=utf-8

This is the first part.

------=_NextPart_000_003F_01CE98CE.6E826F90       
Content-Type: text/plain; charset=utf-8

This is the second part.

------=_NextPart_000_003F_01CE98CE.6E826F90--  
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));

				await AssertSerializationAsync (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));

				await AssertSerializationAsync (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestMultipartBoundaryLineWithTrailingSpacesAndThenMoreCharacters ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of a multipart boundary followed by trailing whitespace and then more characters
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the first part.

------=_NextPart_000_003F_01CE98CE.6E826F90       oops, not it.
------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the second part.

------=_NextPart_000_003F_01CE98CE.6E826F90--
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine + Environment.NewLine + "------=_NextPart_000_003F_01CE98CE.6E826F90       oops, not it."));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));

				AssertSerialization (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine + Environment.NewLine + "------=_NextPart_000_003F_01CE98CE.6E826F90       oops, not it."));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));

				AssertSerialization (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestMultipartBoundaryLineWithTrailingSpacesAndThenMoreCharactersAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of a multipart boundary followed by trailing whitespace and then more characters
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the first part.

------=_NextPart_000_003F_01CE98CE.6E826F90       oops, not it.
------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the second part.

------=_NextPart_000_003F_01CE98CE.6E826F90--
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine + Environment.NewLine + "------=_NextPart_000_003F_01CE98CE.6E826F90       oops, not it."));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));

				await AssertSerializationAsync (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine + Environment.NewLine + "------=_NextPart_000_003F_01CE98CE.6E826F90       oops, not it."));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));

				await AssertSerializationAsync (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestMultipartDoubleBoundary ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of double multipart boundaries
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the first part.

------=_NextPart_000_003F_01CE98CE.6E826F90
------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is technically the third part.

------=_NextPart_000_003F_01CE98CE.6E826F90--
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (3), "Expected 3 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in second child of multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected second child of multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				Assert.That (multipart[2], Is.InstanceOf<TextPart> (), "Expected third child of the multipart to be text/plain");
				body = (TextPart) multipart[2];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is technically the third part." + Environment.NewLine));

				AssertSerialization (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (3), "Expected 3 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in second child of multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected second child of multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				Assert.That (multipart[2], Is.InstanceOf<TextPart> (), "Expected third child of the multipart to be text/plain");
				body = (TextPart) multipart[2];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is technically the third part." + Environment.NewLine));

				AssertSerialization (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestMultipartDoubleBoundaryAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of double multipart boundaries
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the first part.

------=_NextPart_000_003F_01CE98CE.6E826F90
------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is technically the third part.

------=_NextPart_000_003F_01CE98CE.6E826F90--
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (3), "Expected 3 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in second child of multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected second child of multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				Assert.That (multipart[2], Is.InstanceOf<TextPart> (), "Expected third child of the multipart to be text/plain");
				body = (TextPart) multipart[2];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is technically the third part." + Environment.NewLine));

				await AssertSerializationAsync (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (3), "Expected 3 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in second child of multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected second child of multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				Assert.That (multipart[2], Is.InstanceOf<TextPart> (), "Expected third child of the multipart to be text/plain");
				body = (TextPart) multipart[2];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is technically the third part." + Environment.NewLine));

				await AssertSerializationAsync (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestMultipartDoubleBoundaryEndBoundary ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of double multipart boundaries and then an end boundary
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the first part.

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the second part.

------=_NextPart_000_003F_01CE98CE.6E826F90
------=_NextPart_000_003F_01CE98CE.6E826F90
------=_NextPart_000_003F_01CE98CE.6E826F90--
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (4), "Expected 4 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));

				Assert.That (multipart[2], Is.InstanceOf<TextPart> (), "Expected third child of the multipart to be text/plain");
				body = (TextPart) multipart[2];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in third child of multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected third child of multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				Assert.That (multipart[3], Is.InstanceOf<TextPart> (), "Expected fourth child of the multipart to be text/plain");
				body = (TextPart) multipart[3];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in fourth child of multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected fourth child of multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				AssertSerialization (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (4), "Expected 4 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));

				Assert.That (multipart[2], Is.InstanceOf<TextPart> (), "Expected third child of the multipart to be text/plain");
				body = (TextPart) multipart[2];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in third child of multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected third child of multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				Assert.That (multipart[3], Is.InstanceOf<TextPart> (), "Expected fourth child of the multipart to be text/plain");
				body = (TextPart) multipart[3];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in fourth child of multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected fourth child of multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				AssertSerialization (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestMultipartDoubleBoundaryEndBoundaryAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of double multipart boundaries and then an end boundary
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the first part.

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the second part.

------=_NextPart_000_003F_01CE98CE.6E826F90
------=_NextPart_000_003F_01CE98CE.6E826F90
------=_NextPart_000_003F_01CE98CE.6E826F90--
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (4), "Expected 4 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));

				Assert.That (multipart[2], Is.InstanceOf<TextPart> (), "Expected third child of the multipart to be text/plain");
				body = (TextPart) multipart[2];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in third child of multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected third child of multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				Assert.That (multipart[3], Is.InstanceOf<TextPart> (), "Expected fourth child of the multipart to be text/plain");
				body = (TextPart) multipart[3];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in fourth child of multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected fourth child of multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				await AssertSerializationAsync (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (4), "Expected 4 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));

				Assert.That (multipart[2], Is.InstanceOf<TextPart> (), "Expected third child of the multipart to be text/plain");
				body = (TextPart) multipart[2];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in third child of multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected third child of multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				Assert.That (multipart[3], Is.InstanceOf<TextPart> (), "Expected fourth child of the multipart to be text/plain");
				body = (TextPart) multipart[3];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in fourth child of multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected fourth child of multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				await AssertSerializationAsync (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestMultipartDoubleBoundaryEof ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of multipart ending with a double boundary
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the first part.

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the second part.

------=_NextPart_000_003F_01CE98CE.6E826F90
------=_NextPart_000_003F_01CE98CE.6E826F90
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (4), "Expected 4 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));

				Assert.That (multipart[2], Is.InstanceOf<TextPart> (), "Expected third child of the multipart to be text/plain");
				body = (TextPart) multipart[2];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in third child of multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected third child of multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				Assert.That (multipart[3], Is.InstanceOf<TextPart> (), "Expected fourth child of the multipart to be text/plain");
				body = (TextPart) multipart[3];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in fourth child of multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected fourth child of multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				AssertSerialization (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (4), "Expected 4 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));

				Assert.That (multipart[2], Is.InstanceOf<TextPart> (), "Expected third child of the multipart to be text/plain");
				body = (TextPart) multipart[2];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in third child of multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected third child of multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				Assert.That (multipart[3], Is.InstanceOf<TextPart> (), "Expected fourth child of the multipart to be text/plain");
				body = (TextPart) multipart[3];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in fourth child of multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected fourth child of multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				AssertSerialization (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestMultipartDoubleBoundaryEofAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of multipart ending with a double boundary
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the first part.

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the second part.

------=_NextPart_000_003F_01CE98CE.6E826F90
------=_NextPart_000_003F_01CE98CE.6E826F90
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (4), "Expected 4 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));

				Assert.That (multipart[2], Is.InstanceOf<TextPart> (), "Expected third child of the multipart to be text/plain");
				body = (TextPart) multipart[2];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in third child of multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected third child of multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				Assert.That (multipart[3], Is.InstanceOf<TextPart> (), "Expected fourth child of the multipart to be text/plain");
				body = (TextPart) multipart[3];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in fourth child of multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected fourth child of multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				await AssertSerializationAsync (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (4), "Expected 4 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));

				Assert.That (multipart[2], Is.InstanceOf<TextPart> (), "Expected third child of the multipart to be text/plain");
				body = (TextPart) multipart[2];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in third child of multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected third child of multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				Assert.That (multipart[3], Is.InstanceOf<TextPart> (), "Expected fourth child of the multipart to be text/plain");
				body = (TextPart) multipart[3];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in fourth child of multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected fourth child of multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				await AssertSerializationAsync (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestMultipartDoubleBoundaryParentBoundary ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of multipart with double boundaries and then a parent boundary
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
Content-Type: multipart/mixed;
	boundary=""outer-boundary""

--outer-boundary
Content-Type: multipart/mixed;
	boundary=""inner-boundary""

--inner-boundary
Content-Type: text/plain; charset=utf-8

This is the first part.

--inner-boundary
Content-Type: text/plain; charset=utf-8

This is the second part.

--inner-boundary
--inner-boundary
--outer-boundary
Content-Type: image/jpeg

<base64 data>
--outer-boundary--
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var outer = (Multipart) message.Body;
				Assert.That (outer.Count, Is.EqualTo (2), "Expected 2 outer children");
				Assert.That (outer[0], Is.InstanceOf<Multipart> (), "Expected first child of the outer multipart to be multipart/mixed");
				Assert.That (outer[1], Is.InstanceOf<MimePart> (), "Expected second child of the outer multipart to be image/jpeg");

				var multipart = (Multipart) outer[0];
				Assert.That (multipart.Count, Is.EqualTo (4), "Expected 4 inner children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the inner multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the inner multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));

				Assert.That (multipart[2], Is.InstanceOf<TextPart> (), "Expected third child of the inner multipart to be text/plain");
				body = (TextPart) multipart[2];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in third child of inner multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected third child of inner multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				Assert.That (multipart[3], Is.InstanceOf<TextPart> (), "Expected fourth child of the inner multipart to be text/plain");
				body = (TextPart) multipart[3];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in fourth child of inner multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected fourth child of inner multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				AssertSerialization (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var outer = (Multipart) message.Body;
				Assert.That (outer.Count, Is.EqualTo (2), "Expected 2 outer children");
				Assert.That (outer[0], Is.InstanceOf<Multipart> (), "Expected first child of the outer multipart to be multipart/mixed");
				Assert.That (outer[1], Is.InstanceOf<MimePart> (), "Expected second child of the outer multipart to be image/jpeg");

				var multipart = (Multipart) outer[0];
				Assert.That (multipart.Count, Is.EqualTo (4), "Expected 4 inner children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the inner multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the inner multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));

				Assert.That (multipart[2], Is.InstanceOf<TextPart> (), "Expected third child of the inner multipart to be text/plain");
				body = (TextPart) multipart[2];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in third child of inner multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected third child of inner multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				Assert.That (multipart[3], Is.InstanceOf<TextPart> (), "Expected fourth child of the inner multipart to be text/plain");
				body = (TextPart) multipart[3];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in fourth child of inner multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected fourth child of inner multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				AssertSerialization (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestMultipartDoubleBoundaryParentBoundaryAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of multipart with double boundaries and then a parent boundary
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
Content-Type: multipart/mixed;
	boundary=""outer-boundary""

--outer-boundary
Content-Type: multipart/mixed;
	boundary=""inner-boundary""

--inner-boundary
Content-Type: text/plain; charset=utf-8

This is the first part.

--inner-boundary
Content-Type: text/plain; charset=utf-8

This is the second part.

--inner-boundary
--inner-boundary
--outer-boundary
Content-Type: image/jpeg

<base64 data>
--outer-boundary--
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var outer = (Multipart) message.Body;
				Assert.That (outer.Count, Is.EqualTo (2), "Expected 2 outer children");
				Assert.That (outer[0], Is.InstanceOf<Multipart> (), "Expected first child of the outer multipart to be multipart/mixed");
				Assert.That (outer[1], Is.InstanceOf<MimePart> (), "Expected second child of the outer multipart to be image/jpeg");

				var multipart = (Multipart) outer[0];
				Assert.That (multipart.Count, Is.EqualTo (4), "Expected 4 inner children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the inner multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the inner multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));

				Assert.That (multipart[2], Is.InstanceOf<TextPart> (), "Expected third child of the inner multipart to be text/plain");
				body = (TextPart) multipart[2];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in third child of inner multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected third child of inner multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				Assert.That (multipart[3], Is.InstanceOf<TextPart> (), "Expected fourth child of the inner multipart to be text/plain");
				body = (TextPart) multipart[3];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in fourth child of inner multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected fourth child of inner multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				await AssertSerializationAsync (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var outer = (Multipart) message.Body;
				Assert.That (outer.Count, Is.EqualTo (2), "Expected 2 outer children");
				Assert.That (outer[0], Is.InstanceOf<Multipart> (), "Expected first child of the outer multipart to be multipart/mixed");
				Assert.That (outer[1], Is.InstanceOf<MimePart> (), "Expected second child of the outer multipart to be image/jpeg");

				var multipart = (Multipart) outer[0];
				Assert.That (multipart.Count, Is.EqualTo (4), "Expected 4 inner children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the inner multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the first part." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<TextPart> (), "Expected second child of the inner multipart to be text/plain");
				body = (TextPart) multipart[1];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the second part." + Environment.NewLine));

				Assert.That (multipart[2], Is.InstanceOf<TextPart> (), "Expected third child of the inner multipart to be text/plain");
				body = (TextPart) multipart[2];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in third child of inner multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected third child of inner multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				Assert.That (multipart[3], Is.InstanceOf<TextPart> (), "Expected fourth child of the inner multipart to be text/plain");
				body = (TextPart) multipart[3];

				Assert.That (body.Headers.Count, Is.EqualTo (0), "Expected 0 headers in fourth child of inner multipart");
				Assert.That (body.ContentType.MimeType, Is.EqualTo ("text/plain"), "Expected fourth child of inner multipart to be treated as text/plain");
				Assert.That (body.Text, Is.EqualTo (string.Empty));

				await AssertSerializationAsync (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestMultipartWithoutBoundaryParameter ()
		{
			string text = @"Content-Type: multipart/mixed

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the first part.

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the second part.

------=_NextPart_000_003F_01CE98CE.6E826F90--
".Replace ("\r\n", "\n");
			int dashes = text.IndexOf ("--");
			var preamble = text.Substring (dashes);

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var multipart = (Multipart) parser.ParseEntity ();

				Assert.That (multipart.Boundary, Is.Null, "Boundary");
				Assert.That (multipart.Count, Is.EqualTo (0), "Expected 0 children");
				Assert.That (multipart.Preamble, Is.EqualTo (preamble), "Preamble");

				AssertSerialization (multipart, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var multipart = (Multipart) parser.ParseEntity ();

				Assert.That (multipart.Boundary, Is.Null, "Boundary");
				Assert.That (multipart.Count, Is.EqualTo (0), "Expected 0 children");
				Assert.That (multipart.Preamble, Is.EqualTo (preamble.Replace ("\n", "\r\n")), "Preamble");

				AssertSerialization (multipart, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestMultipartWithoutBoundaryParameterAsync ()
		{
			string text = @"Content-Type: multipart/mixed

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the first part.

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the second part.

------=_NextPart_000_003F_01CE98CE.6E826F90--
".Replace ("\r\n", "\n");
			int dashes = text.IndexOf ("--");
			var preamble = text.Substring (dashes);

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var multipart = (Multipart) await parser.ParseEntityAsync ();

				Assert.That (multipart.Boundary, Is.Null, "Boundary");
				Assert.That (multipart.Count, Is.EqualTo (0), "Expected 0 children");
				Assert.That (multipart.Preamble, Is.EqualTo (preamble), "Preamble");

				await AssertSerializationAsync (multipart, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var multipart = (Multipart) await parser.ParseEntityAsync ();

				Assert.That (multipart.Boundary, Is.Null, "Boundary");
				Assert.That (multipart.Count, Is.EqualTo (0), "Expected 0 children");
				Assert.That (multipart.Preamble, Is.EqualTo (preamble.Replace ("\n", "\r\n")), "Preamble");

				await AssertSerializationAsync (multipart, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestTruncatedImmediatelyAfterMessageRfc822Headers ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of message/rfc822 part truncated immediately after the MIME headers
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the message body.

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: message/rfc822

".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<MessagePart> (), "Expected second child of the multipart to be message/rfc822");
				var rfc822 = (MessagePart) multipart[1];
				Assert.That (rfc822.Message, Is.Null, "Message");

				AssertSerialization (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<MessagePart> (), "Expected second child of the multipart to be message/rfc822");
				var rfc822 = (MessagePart) multipart[1];
				Assert.That (rfc822.Message, Is.Null, "Message");

				AssertSerialization (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestTruncatedImmediatelyAfterMessageRfc822HeadersAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of message/rfc822 part truncated immediately after the MIME headers
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the message body.

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: message/rfc822

".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<MessagePart> (), "Expected second child of the multipart to be message/rfc822");
				var rfc822 = (MessagePart) multipart[1];
				Assert.That (rfc822.Message, Is.Null, "Message");

				await AssertSerializationAsync (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<MessagePart> (), "Expected second child of the multipart to be message/rfc822");
				var rfc822 = (MessagePart) multipart[1];
				Assert.That (rfc822.Message, Is.Null, "Message");

				await AssertSerializationAsync (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestMessageRfc822WithoutMessage ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of message/rfc822 part without a message
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""
Content-Length: 420


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the message body.

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: message/rfc822

------=_NextPart_000_003F_01CE98CE.6E826F90--
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<MessagePart> (), "Expected second child of the multipart to be message/rfc822");
				var rfc822 = (MessagePart) multipart[1];
				Assert.That (rfc822.Message, Is.Null, "Message");

				AssertSerialization (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<MessagePart> (), "Expected second child of the multipart to be message/rfc822");
				var rfc822 = (MessagePart) multipart[1];
				Assert.That (rfc822.Message, Is.Null, "Message");

				AssertSerialization (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestMessageRfc822WithoutMessageAsync ()
		{
			string text = @"From: mimekit@example.com
To: mimekit@example.com
Subject: test of message/rfc822 part without a message
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""
Content-Length: 420


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the message body.

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: message/rfc822

------=_NextPart_000_003F_01CE98CE.6E826F90--
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<MessagePart> (), "Expected second child of the multipart to be message/rfc822");
				var rfc822 = (MessagePart) multipart[1];
				Assert.That (rfc822.Message, Is.Null, "Message");

				await AssertSerializationAsync (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<MessagePart> (), "Expected second child of the multipart to be message/rfc822");
				var rfc822 = (MessagePart) multipart[1];
				Assert.That (rfc822.Message, Is.Null, "Message");

				await AssertSerializationAsync (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestMessageRfc822WithFromMarkerBeforeMessage ()
		{
			string text = @"From -
From: mimekit@example.com
To: mimekit@example.com
Subject: test of message/rfc822 part with a From-marker before the message
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""
Content-Length: 420


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the message body.

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: message/rfc822

From -
From: mimekit@example.com
To: mimekit@example.com
Subject: embedded message
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Content-Type: text/plain; charset=utf-8

This is the embedded message body.
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var options = ParserOptions.Default.Clone ();
				options.RespectContentLength = true;

				var parser = new MimeParser (options, stream, MimeFormat.Mbox);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<MessagePart> (), "Expected second child of the multipart to be message/rfc822");
				var rfc822 = (MessagePart) multipart[1];

				Assert.That (rfc822.Message.Body, Is.InstanceOf<TextPart> (), "Expected child of the embedded message to be text/plain");
				body = (TextPart) rfc822.Message.Body;

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the embedded message body." + Environment.NewLine));

				AssertSerialization (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var options = ParserOptions.Default.Clone ();
				options.RespectContentLength = true;

				var parser = new MimeParser (options, stream, MimeFormat.Mbox);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<MessagePart> (), "Expected second child of the multipart to be message/rfc822");
				var rfc822 = (MessagePart) multipart[1];

				Assert.That (rfc822.Message.Body, Is.InstanceOf<TextPart> (), "Expected child of the embedded message to be text/plain");
				body = (TextPart) rfc822.Message.Body;

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the embedded message body." + Environment.NewLine));

				AssertSerialization (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestMessageRfc822WithFromMarkerBeforeMessageAsync ()
		{
			string text = @"From -
From: mimekit@example.com
To: mimekit@example.com
Subject: test of message/rfc822 part with a From-marker before the message
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Message-ID: <54AD68C9E3B0184CAC6041320424FD1B5B81E74D@localhost.localdomain>
X-Mailer: Microsoft Office Outlook 12.0
Content-Type: multipart/mixed;
	boundary=""----=_NextPart_000_003F_01CE98CE.6E826F90""
Content-Length: 420


------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: text/plain; charset=utf-8

This is the message body.

------=_NextPart_000_003F_01CE98CE.6E826F90
Content-Type: message/rfc822

From -
From: mimekit@example.com
To: mimekit@example.com
Subject: embedded message
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Content-Type: text/plain; charset=utf-8

This is the embedded message body.
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var options = ParserOptions.Default.Clone ();
				options.RespectContentLength = true;

				var parser = new MimeParser (options, stream, MimeFormat.Mbox);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<MessagePart> (), "Expected second child of the multipart to be message/rfc822");
				var rfc822 = (MessagePart) multipart[1];

				Assert.That (rfc822.Message.Body, Is.InstanceOf<TextPart> (), "Expected child of the embedded message to be text/plain");
				body = (TextPart) rfc822.Message.Body;

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the embedded message body." + Environment.NewLine));

				await AssertSerializationAsync (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var options = ParserOptions.Default.Clone ();
				options.RespectContentLength = true;

				var parser = new MimeParser (options, stream, MimeFormat.Mbox);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<Multipart> (), "Expected top-level to be a multipart");
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (2), "Expected 2 children");
				Assert.That (multipart[0], Is.InstanceOf<TextPart> (), "Expected first child of the multipart to be text/plain");
				var body = (TextPart) multipart[0];

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the message body." + Environment.NewLine));

				Assert.That (multipart[1], Is.InstanceOf<MessagePart> (), "Expected second child of the multipart to be message/rfc822");
				var rfc822 = (MessagePart) multipart[1];

				Assert.That (rfc822.Message.Body, Is.InstanceOf<TextPart> (), "Expected child of the embedded message to be text/plain");
				body = (TextPart) rfc822.Message.Body;

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the embedded message body." + Environment.NewLine));

				await AssertSerializationAsync (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestMessageRfc822WithMungedFromMarkerBeforeMessage ()
		{
			string text = @"From - Fri Mar  7 02:51:22 1997
Return-Path: <jwz@netscape.com>
Received: from gruntle ([205.217.227.10]) by dredd.mcom.com
          (Netscape Mail Server v2.02) with SMTP id AAA4040
          for <jwz@netscape.com>; Fri, 7 Mar 1997 02:50:37 -0800
Sender: jwz@netscape.com (Jamie Zawinski)
Message-ID: <331FF2FF.FF6@netscape.com>
Date: Fri, 07 Mar 1997 02:50:39 -0800
From: Jamie Zawinski <jwz@netscape.com>
Organization: Netscape Communications Corporation, Mozilla Division
X-Mailer: Mozilla 3.01 (X11; U; IRIX 6.2 IP22)
MIME-Version: 1.0
To: Jamie Zawinski <jwz@netscape.com>
Subject: forwarded encrypted message
Content-Type: message/rfc822; name=""smime18-encrypted.msg""
Content-Transfer-Encoding: 7bit
Content-Disposition: inline; filename=""smime18-encrypted.msg""
X-Mozilla-Status: 0001
Content-Length: 2812

>From - Fri Dec 13 15:01:21 1996
Return-Path: <blaker@craswell.com>
Received: from maleman.mcom.com ([198.93.92.3]) by dredd.mcom.com
          (Netscape Mail Server v2.02) with SMTP id AAA19742
          for <jwz@dredd.mcom.com>; Fri, 13 Dec 1996 14:59:31 -0800
Received: from xwing.netscape.com (xwing.mcom.com [205.218.156.54]) by maleman.mcom.com (8.6.9/8.6.9) with ESMTP id OAA23726 for <jwz@netscape.com>; Fri, 13 Dec 1996 14:58:13 -0800
Received: from peapod.deming.com (host20.deming.com [206.63.131.20]) by xwing.netscape.com (8.7.6/8.7.3) with SMTP id OAA00270 for <jwz@netscape.com>; Fri, 13 Dec 1996 14:59:27 -0800 (PST)
Received: by peapod.deming.com from localhost
    (router,SLmail V2.0); Fri, 13 Dec 1996 15:01:48 Pacific Standard Time
Received: by peapod.deming.com from seth
    (206.63.131.30::mail daemon; unverified,SLmail V2.0); Fri, 13 Dec 1996 15:01:02 Pacific Standard Time
Message-Id: <3.0.32.19961213150855.009172e0@mail.craswell.com>
X-Sender: blaker@mail.craswell.com
X-Mailer: Windows Eudora Pro Version 3.0 (32)
Date: Fri, 13 Dec 1996 15:09:42 -0800
To: Jamie Zawinski <jwz@netscape.com>
From: ""Blake Ramsdell"" <blaker@craswell.com>
Subject: Re: can you send me an encrypted message?
MIME-Version: 1.0
Content-Type: application/x-pkcs7-mime; name=""smime.p7m""
Content-Transfer-Encoding: base64
Content-Disposition: attachment; filename=""smime.p7m""

MIAGCSqGSIb3DQEHA6CAMIACAQAxgDCBzAIBADB2MGIxETAPBgNVBAcTCEludGVybmV0MRcw
FQYDVQQKEw5WZXJpU2lnbiwgSW5jLjE0MDIGA1UECxMrVmVyaVNpZ24gQ2xhc3MgMSBDQSAt
IEluZGl2aWR1YWwgU3Vic2NyaWJlcgIQKQ/GF/RumodE+WtXiPJmhDANBgkqhkiG9w0BAQEF
AARAb0tthyav05ce7KBWdlfN1M0R6wLQ2FWPVQynuWo/yHUoo3hiII7j15FXNgnxF7QkY5/p
mZXg0P2eJ1iYQy1vZDCBzAIBADB2MGIxETAPBgNVBAcTCEludGVybmV0MRcwFQYDVQQKEw5W
ZXJpU2lnbiwgSW5jLjE0MDIGA1UECxMrVmVyaVNpZ24gQ2xhc3MgMSBDQSAtIEluZGl2aWR1
YWwgU3Vic2NyaWJlcgIQDOtpec1+JM3EpqAMVqgtjzANBgkqhkiG9w0BAQEFAARAuqnsnz1O
qEdx7NEMJDEdjccjdEuCM8x2euTYlU/GWNY+s2iKVahbT3/R8E8hp3YfrHd2sjvgy6teTOPO
ZI2SxwAAMIAGCSqGSIb3DQEHATAUBggqhkiG9w0DBwQIlhWqtbsElaWggASCAjBooYYTWSBz
7A4l0Aho7mK85zpMyAR0xTKqHXT0zL9XpHbKPAcETaBTh1n7e8aJeQ93ONGAs6tVVlA6bpUN
F3Q5O+ZuNXOMT83HIKRYEO1l8a+CH7XtUiQWtu/aBt12GQDX475WhPULKEJs7kLS2DwToRX/
ctwEPNwc6zfsOZoVTQ5HOwisvDZ2QGwa08Psj38SaQ0Y+ryk5FeiAtKQUZ0uuJWI/rRu64yj
KmVs1DDId18coftA2rv/u2/zABEX8u5ckEkwS7fO7UHv6XMCQ3kqgqIZZE1zIGohfUdtOYYo
M4eki3QDyovHPxEjBbnmpUw2xDN7/DdxYEZ4CteWurQ+VoP0PUM2qwi6EgM6MpVKg8KzOWdb
aV51a1oQKtpJJFZqZtFf9SQ4OW6NKXHsJ2AF8W4OQ+ySWQN43wMk8dGJYlPrREqn5RufPg3k
QM+s4VwTrS2TrU+ELZCYnJFfH+N7tE8ILrFMAteVxtqjat7OJRyDxy0cnBP+oG81Sr0zvbdC
jUPUDFlrPgFjDrswX1UpkEE2OgKWmfc134AbysJFOuCIze2XqKB96rJvxS76ygzVvrU/4sI1
6VDlZUEuUPaBUOimFxRk/rqPJDI1M8rNKykw9qsoWQMRnvrODfzo7iVWQ0TQHiwfoBhs6Dvm
UgrMwopFnzRdSHvT1acSqVfMYWm5nXImvtCuFAavkjDutE9+Y/LLFLBUpAVeu3rwW3wV0Tcv
9I6Afej0ntfbH9vlRwQIl7MeXMqoBV0AAAAAAAAAAAAA
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (ParserOptions.Default, stream, MimeFormat.Mbox);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<MessagePart> (), "Expected top-level to be a MessagePart");
				var rfc822 = (MessagePart) message.Body;

				Assert.That (rfc822.ContentType.Name, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentType.Name");
				Assert.That (rfc822.ContentDisposition.Disposition, Is.EqualTo ("inline"), "MessagePart.ContentDisposition.Disposition");
				Assert.That (rfc822.ContentDisposition.FileName, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentDisposition.FileName");
				//Assert.That (rfc822.ContentTransferEncoding, Is.EqualTo (ContentEncoding.SevenBit), "MessagePart.ContentTransferEncoding");

				Assert.That (rfc822.Message, Is.Not.Null, "MessagePart.Message");
				Assert.That (rfc822.Message.MboxMarker, Is.Not.Null, "MessagePart.Message.MboxMarker");
				Assert.That (Encoding.ASCII.GetString (rfc822.Message.MboxMarker), Is.EqualTo (">From - Fri Dec 13 15:01:21 1996\n"));
				Assert.That (rfc822.Message.Headers.Count, Is.EqualTo (14), "MessagePart.Message.Headers.Count");
				Assert.That (rfc822.Message.Body.Headers.Count, Is.EqualTo (3), "MessagePart.Message.Body.Headers.Count");

				AssertSerialization (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (ParserOptions.Default, stream, MimeFormat.Mbox);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<MessagePart> (), "Expected top-level to be a MessagePart");
				var rfc822 = (MessagePart) message.Body;

				Assert.That (rfc822.ContentType.Name, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentType.Name");
				Assert.That (rfc822.ContentDisposition.Disposition, Is.EqualTo ("inline"), "MessagePart.ContentDisposition.Disposition");
				Assert.That (rfc822.ContentDisposition.FileName, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentDisposition.FileName");
				//Assert.That (rfc822.ContentTransferEncoding, Is.EqualTo (ContentEncoding.SevenBit), "MessagePart.ContentTransferEncoding");

				Assert.That (rfc822.Message, Is.Not.Null, "MessagePart.Message");
				Assert.That (rfc822.Message.MboxMarker, Is.Not.Null, "MessagePart.Message.MboxMarker");
				Assert.That (Encoding.ASCII.GetString (rfc822.Message.MboxMarker), Is.EqualTo (">From - Fri Dec 13 15:01:21 1996\r\n"));
				Assert.That (rfc822.Message.Headers.Count, Is.EqualTo (14), "MessagePart.Message.Headers.Count");
				Assert.That (rfc822.Message.Body.Headers.Count, Is.EqualTo (3), "MessagePart.Message.Body.Headers.Count");

				AssertSerialization (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestMessageRfc822WithMungedFromMarkerBeforeMessageAsync ()
		{
			string text = @"From - Fri Mar  7 02:51:22 1997
Return-Path: <jwz@netscape.com>
Received: from gruntle ([205.217.227.10]) by dredd.mcom.com
          (Netscape Mail Server v2.02) with SMTP id AAA4040
          for <jwz@netscape.com>; Fri, 7 Mar 1997 02:50:37 -0800
Sender: jwz@netscape.com (Jamie Zawinski)
Message-ID: <331FF2FF.FF6@netscape.com>
Date: Fri, 07 Mar 1997 02:50:39 -0800
From: Jamie Zawinski <jwz@netscape.com>
Organization: Netscape Communications Corporation, Mozilla Division
X-Mailer: Mozilla 3.01 (X11; U; IRIX 6.2 IP22)
MIME-Version: 1.0
To: Jamie Zawinski <jwz@netscape.com>
Subject: forwarded encrypted message
Content-Type: message/rfc822; name=""smime18-encrypted.msg""
Content-Transfer-Encoding: 7bit
Content-Disposition: inline; filename=""smime18-encrypted.msg""
X-Mozilla-Status: 0001
Content-Length: 2812

>From - Fri Dec 13 15:01:21 1996
Return-Path: <blaker@craswell.com>
Received: from maleman.mcom.com ([198.93.92.3]) by dredd.mcom.com
          (Netscape Mail Server v2.02) with SMTP id AAA19742
          for <jwz@dredd.mcom.com>; Fri, 13 Dec 1996 14:59:31 -0800
Received: from xwing.netscape.com (xwing.mcom.com [205.218.156.54]) by maleman.mcom.com (8.6.9/8.6.9) with ESMTP id OAA23726 for <jwz@netscape.com>; Fri, 13 Dec 1996 14:58:13 -0800
Received: from peapod.deming.com (host20.deming.com [206.63.131.20]) by xwing.netscape.com (8.7.6/8.7.3) with SMTP id OAA00270 for <jwz@netscape.com>; Fri, 13 Dec 1996 14:59:27 -0800 (PST)
Received: by peapod.deming.com from localhost
    (router,SLmail V2.0); Fri, 13 Dec 1996 15:01:48 Pacific Standard Time
Received: by peapod.deming.com from seth
    (206.63.131.30::mail daemon; unverified,SLmail V2.0); Fri, 13 Dec 1996 15:01:02 Pacific Standard Time
Message-Id: <3.0.32.19961213150855.009172e0@mail.craswell.com>
X-Sender: blaker@mail.craswell.com
X-Mailer: Windows Eudora Pro Version 3.0 (32)
Date: Fri, 13 Dec 1996 15:09:42 -0800
To: Jamie Zawinski <jwz@netscape.com>
From: ""Blake Ramsdell"" <blaker@craswell.com>
Subject: Re: can you send me an encrypted message?
MIME-Version: 1.0
Content-Type: application/x-pkcs7-mime; name=""smime.p7m""
Content-Transfer-Encoding: base64
Content-Disposition: attachment; filename=""smime.p7m""

MIAGCSqGSIb3DQEHA6CAMIACAQAxgDCBzAIBADB2MGIxETAPBgNVBAcTCEludGVybmV0MRcw
FQYDVQQKEw5WZXJpU2lnbiwgSW5jLjE0MDIGA1UECxMrVmVyaVNpZ24gQ2xhc3MgMSBDQSAt
IEluZGl2aWR1YWwgU3Vic2NyaWJlcgIQKQ/GF/RumodE+WtXiPJmhDANBgkqhkiG9w0BAQEF
AARAb0tthyav05ce7KBWdlfN1M0R6wLQ2FWPVQynuWo/yHUoo3hiII7j15FXNgnxF7QkY5/p
mZXg0P2eJ1iYQy1vZDCBzAIBADB2MGIxETAPBgNVBAcTCEludGVybmV0MRcwFQYDVQQKEw5W
ZXJpU2lnbiwgSW5jLjE0MDIGA1UECxMrVmVyaVNpZ24gQ2xhc3MgMSBDQSAtIEluZGl2aWR1
YWwgU3Vic2NyaWJlcgIQDOtpec1+JM3EpqAMVqgtjzANBgkqhkiG9w0BAQEFAARAuqnsnz1O
qEdx7NEMJDEdjccjdEuCM8x2euTYlU/GWNY+s2iKVahbT3/R8E8hp3YfrHd2sjvgy6teTOPO
ZI2SxwAAMIAGCSqGSIb3DQEHATAUBggqhkiG9w0DBwQIlhWqtbsElaWggASCAjBooYYTWSBz
7A4l0Aho7mK85zpMyAR0xTKqHXT0zL9XpHbKPAcETaBTh1n7e8aJeQ93ONGAs6tVVlA6bpUN
F3Q5O+ZuNXOMT83HIKRYEO1l8a+CH7XtUiQWtu/aBt12GQDX475WhPULKEJs7kLS2DwToRX/
ctwEPNwc6zfsOZoVTQ5HOwisvDZ2QGwa08Psj38SaQ0Y+ryk5FeiAtKQUZ0uuJWI/rRu64yj
KmVs1DDId18coftA2rv/u2/zABEX8u5ckEkwS7fO7UHv6XMCQ3kqgqIZZE1zIGohfUdtOYYo
M4eki3QDyovHPxEjBbnmpUw2xDN7/DdxYEZ4CteWurQ+VoP0PUM2qwi6EgM6MpVKg8KzOWdb
aV51a1oQKtpJJFZqZtFf9SQ4OW6NKXHsJ2AF8W4OQ+ySWQN43wMk8dGJYlPrREqn5RufPg3k
QM+s4VwTrS2TrU+ELZCYnJFfH+N7tE8ILrFMAteVxtqjat7OJRyDxy0cnBP+oG81Sr0zvbdC
jUPUDFlrPgFjDrswX1UpkEE2OgKWmfc134AbysJFOuCIze2XqKB96rJvxS76ygzVvrU/4sI1
6VDlZUEuUPaBUOimFxRk/rqPJDI1M8rNKykw9qsoWQMRnvrODfzo7iVWQ0TQHiwfoBhs6Dvm
UgrMwopFnzRdSHvT1acSqVfMYWm5nXImvtCuFAavkjDutE9+Y/LLFLBUpAVeu3rwW3wV0Tcv
9I6Afej0ntfbH9vlRwQIl7MeXMqoBV0AAAAAAAAAAAAA
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (ParserOptions.Default, stream, MimeFormat.Mbox);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<MessagePart> (), "Expected top-level to be a MessagePart");
				var rfc822 = (MessagePart) message.Body;

				Assert.That (rfc822.ContentType.Name, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentType.Name");
				Assert.That (rfc822.ContentDisposition.Disposition, Is.EqualTo ("inline"), "MessagePart.ContentDisposition.Disposition");
				Assert.That (rfc822.ContentDisposition.FileName, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentDisposition.FileName");
				//Assert.That (rfc822.ContentTransferEncoding, Is.EqualTo (ContentEncoding.SevenBit), "MessagePart.ContentTransferEncoding");

				Assert.That (rfc822.Message, Is.Not.Null, "MessagePart.Message");
				Assert.That (rfc822.Message.MboxMarker, Is.Not.Null, "MessagePart.Message.MboxMarker");
				Assert.That (Encoding.ASCII.GetString (rfc822.Message.MboxMarker), Is.EqualTo (">From - Fri Dec 13 15:01:21 1996\n"));
				Assert.That (rfc822.Message.Headers.Count, Is.EqualTo (14), "MessagePart.Message.Headers.Count");
				Assert.That (rfc822.Message.Body.Headers.Count, Is.EqualTo (3), "MessagePart.Message.Body.Headers.Count");

				await AssertSerializationAsync (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (ParserOptions.Default, stream, MimeFormat.Mbox);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<MessagePart> (), "Expected top-level to be a MessagePart");
				var rfc822 = (MessagePart) message.Body;

				Assert.That (rfc822.ContentType.Name, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentType.Name");
				Assert.That (rfc822.ContentDisposition.Disposition, Is.EqualTo ("inline"), "MessagePart.ContentDisposition.Disposition");
				Assert.That (rfc822.ContentDisposition.FileName, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentDisposition.FileName");
				//Assert.That (rfc822.ContentTransferEncoding, Is.EqualTo (ContentEncoding.SevenBit), "MessagePart.ContentTransferEncoding");

				Assert.That (rfc822.Message, Is.Not.Null, "MessagePart.Message");
				Assert.That (rfc822.Message.MboxMarker, Is.Not.Null, "MessagePart.Message.MboxMarker");
				Assert.That (Encoding.ASCII.GetString (rfc822.Message.MboxMarker), Is.EqualTo (">From - Fri Dec 13 15:01:21 1996\r\n"));
				Assert.That (rfc822.Message.Headers.Count, Is.EqualTo (14), "MessagePart.Message.Headers.Count");
				Assert.That (rfc822.Message.Body.Headers.Count, Is.EqualTo (3), "MessagePart.Message.Body.Headers.Count");

				await AssertSerializationAsync (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestMessageRfc822WithMungedFromMarkerEOF ()
		{
			string text = @"From - Fri Mar  7 02:51:22 1997
Return-Path: <jwz@netscape.com>
Received: from gruntle ([205.217.227.10]) by dredd.mcom.com
          (Netscape Mail Server v2.02) with SMTP id AAA4040
          for <jwz@netscape.com>; Fri, 7 Mar 1997 02:50:37 -0800
Sender: jwz@netscape.com (Jamie Zawinski)
Message-ID: <331FF2FF.FF6@netscape.com>
Date: Fri, 07 Mar 1997 02:50:39 -0800
From: Jamie Zawinski <jwz@netscape.com>
Organization: Netscape Communications Corporation, Mozilla Division
X-Mailer: Mozilla 3.01 (X11; U; IRIX 6.2 IP22)
MIME-Version: 1.0
To: Jamie Zawinski <jwz@netscape.com>
Subject: forwarded encrypted message
Content-Type: message/rfc822; name=""smime18-encrypted.msg""
Content-Transfer-Encoding: 7bit
Content-Disposition: inline; filename=""smime18-encrypted.msg""
X-Mozilla-Status: 0001
Content-Length: 2812

>From - Fri Dec 13 15:01:21 1996".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (ParserOptions.Default, stream, MimeFormat.Mbox);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<MessagePart> (), "Expected top-level to be a MessagePart");
				var rfc822 = (MessagePart) message.Body;

				Assert.That (rfc822.ContentType.Name, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentType.Name");
				Assert.That (rfc822.ContentDisposition.Disposition, Is.EqualTo ("inline"), "MessagePart.ContentDisposition.Disposition");
				Assert.That (rfc822.ContentDisposition.FileName, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentDisposition.FileName");
				//Assert.That (rfc822.ContentTransferEncoding, Is.EqualTo (ContentEncoding.SevenBit), "MessagePart.ContentTransferEncoding");

				Assert.That (rfc822.Message, Is.Not.Null, "MessagePart.Message");
				Assert.That (rfc822.Message.MboxMarker, Is.Not.Null, "MessagePart.Message.MboxMarker");
				Assert.That (Encoding.ASCII.GetString (rfc822.Message.MboxMarker), Is.EqualTo (">From - Fri Dec 13 15:01:21 1996"));
				Assert.That (rfc822.Message.Headers.Count, Is.EqualTo (0), "MessagePart.Message.Headers.Count");
				Assert.That (rfc822.Message.Body.Headers.Count, Is.EqualTo (0), "MessagePart.Message.Body.Headers.Count");

				AssertSerialization (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (ParserOptions.Default, stream, MimeFormat.Mbox);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<MessagePart> (), "Expected top-level to be a MessagePart");
				var rfc822 = (MessagePart) message.Body;

				Assert.That (rfc822.ContentType.Name, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentType.Name");
				Assert.That (rfc822.ContentDisposition.Disposition, Is.EqualTo ("inline"), "MessagePart.ContentDisposition.Disposition");
				Assert.That (rfc822.ContentDisposition.FileName, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentDisposition.FileName");
				//Assert.That (rfc822.ContentTransferEncoding, Is.EqualTo (ContentEncoding.SevenBit), "MessagePart.ContentTransferEncoding");

				Assert.That (rfc822.Message, Is.Not.Null, "MessagePart.Message");
				Assert.That (rfc822.Message.MboxMarker, Is.Not.Null, "MessagePart.Message.MboxMarker");
				Assert.That (Encoding.ASCII.GetString (rfc822.Message.MboxMarker), Is.EqualTo (">From - Fri Dec 13 15:01:21 1996"));
				Assert.That (rfc822.Message.Headers.Count, Is.EqualTo (0), "MessagePart.Message.Headers.Count");
				Assert.That (rfc822.Message.Body.Headers.Count, Is.EqualTo (0), "MessagePart.Message.Body.Headers.Count");

				AssertSerialization (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestMessageRfc822WithMungedFromMarkerEOFAsync ()
		{
			string text = @"From - Fri Mar  7 02:51:22 1997
Return-Path: <jwz@netscape.com>
Received: from gruntle ([205.217.227.10]) by dredd.mcom.com
          (Netscape Mail Server v2.02) with SMTP id AAA4040
          for <jwz@netscape.com>; Fri, 7 Mar 1997 02:50:37 -0800
Sender: jwz@netscape.com (Jamie Zawinski)
Message-ID: <331FF2FF.FF6@netscape.com>
Date: Fri, 07 Mar 1997 02:50:39 -0800
From: Jamie Zawinski <jwz@netscape.com>
Organization: Netscape Communications Corporation, Mozilla Division
X-Mailer: Mozilla 3.01 (X11; U; IRIX 6.2 IP22)
MIME-Version: 1.0
To: Jamie Zawinski <jwz@netscape.com>
Subject: forwarded encrypted message
Content-Type: message/rfc822; name=""smime18-encrypted.msg""
Content-Transfer-Encoding: 7bit
Content-Disposition: inline; filename=""smime18-encrypted.msg""
X-Mozilla-Status: 0001
Content-Length: 2812

>From - Fri Dec 13 15:01:21 1996".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (ParserOptions.Default, stream, MimeFormat.Mbox);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<MessagePart> (), "Expected top-level to be a MessagePart");
				var rfc822 = (MessagePart) message.Body;

				Assert.That (rfc822.ContentType.Name, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentType.Name");
				Assert.That (rfc822.ContentDisposition.Disposition, Is.EqualTo ("inline"), "MessagePart.ContentDisposition.Disposition");
				Assert.That (rfc822.ContentDisposition.FileName, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentDisposition.FileName");
				//Assert.That (rfc822.ContentTransferEncoding, Is.EqualTo (ContentEncoding.SevenBit), "MessagePart.ContentTransferEncoding");

				Assert.That (rfc822.Message, Is.Not.Null, "MessagePart.Message");
				Assert.That (rfc822.Message.MboxMarker, Is.Not.Null, "MessagePart.Message.MboxMarker");
				Assert.That (Encoding.ASCII.GetString (rfc822.Message.MboxMarker), Is.EqualTo (">From - Fri Dec 13 15:01:21 1996"));
				Assert.That (rfc822.Message.Headers.Count, Is.EqualTo (0), "MessagePart.Message.Headers.Count");
				Assert.That (rfc822.Message.Body.Headers.Count, Is.EqualTo (0), "MessagePart.Message.Body.Headers.Count");

				await AssertSerializationAsync (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (ParserOptions.Default, stream, MimeFormat.Mbox);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<MessagePart> (), "Expected top-level to be a MessagePart");
				var rfc822 = (MessagePart) message.Body;

				Assert.That (rfc822.ContentType.Name, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentType.Name");
				Assert.That (rfc822.ContentDisposition.Disposition, Is.EqualTo ("inline"), "MessagePart.ContentDisposition.Disposition");
				Assert.That (rfc822.ContentDisposition.FileName, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentDisposition.FileName");
				//Assert.That (rfc822.ContentTransferEncoding, Is.EqualTo (ContentEncoding.SevenBit), "MessagePart.ContentTransferEncoding");

				Assert.That (rfc822.Message, Is.Not.Null, "MessagePart.Message");
				Assert.That (rfc822.Message.MboxMarker, Is.Not.Null, "MessagePart.Message.MboxMarker");
				Assert.That (Encoding.ASCII.GetString (rfc822.Message.MboxMarker), Is.EqualTo (">From - Fri Dec 13 15:01:21 1996"));
				Assert.That (rfc822.Message.Headers.Count, Is.EqualTo (0), "MessagePart.Message.Headers.Count");
				Assert.That (rfc822.Message.Body.Headers.Count, Is.EqualTo (0), "MessagePart.Message.Body.Headers.Count");

				await AssertSerializationAsync (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestMessageRfc822WithTruncatedMungedFromMarker ()
		{
			string text = @"From - Fri Mar  7 02:51:22 1997
Return-Path: <jwz@netscape.com>
Received: from gruntle ([205.217.227.10]) by dredd.mcom.com
          (Netscape Mail Server v2.02) with SMTP id AAA4040
          for <jwz@netscape.com>; Fri, 7 Mar 1997 02:50:37 -0800
Sender: jwz@netscape.com (Jamie Zawinski)
Message-ID: <331FF2FF.FF6@netscape.com>
Date: Fri, 07 Mar 1997 02:50:39 -0800
From: Jamie Zawinski <jwz@netscape.com>
Organization: Netscape Communications Corporation, Mozilla Division
X-Mailer: Mozilla 3.01 (X11; U; IRIX 6.2 IP22)
MIME-Version: 1.0
To: Jamie Zawinski <jwz@netscape.com>
Subject: forwarded encrypted message
Content-Type: message/rfc822; name=""smime18-encrypted.msg""
Content-Transfer-Encoding: 7bit
Content-Disposition: inline; filename=""smime18-encrypted.msg""
X-Mozilla-Status: 0001
Content-Length: 2812

>From".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (ParserOptions.Default, stream, MimeFormat.Mbox);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<MessagePart> (), "Expected top-level to be a MessagePart");
				var rfc822 = (MessagePart) message.Body;

				Assert.That (rfc822.ContentType.Name, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentType.Name");
				Assert.That (rfc822.ContentDisposition.Disposition, Is.EqualTo ("inline"), "MessagePart.ContentDisposition.Disposition");
				Assert.That (rfc822.ContentDisposition.FileName, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentDisposition.FileName");
				//Assert.That (rfc822.ContentTransferEncoding, Is.EqualTo (ContentEncoding.SevenBit), "MessagePart.ContentTransferEncoding");

				Assert.That (rfc822.Message, Is.Not.Null, "MessagePart.Message");
				Assert.That (rfc822.Message.MboxMarker, Is.Not.Null, "MessagePart.Message.MboxMarker");
				Assert.That (Encoding.ASCII.GetString (rfc822.Message.MboxMarker), Is.EqualTo (">From"));
				Assert.That (rfc822.Message.Headers.Count, Is.EqualTo (0), "MessagePart.Message.Headers.Count");
				Assert.That (rfc822.Message.Body.Headers.Count, Is.EqualTo (0), "MessagePart.Message.Body.Headers.Count");

				AssertSerialization (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (ParserOptions.Default, stream, MimeFormat.Mbox);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<MessagePart> (), "Expected top-level to be a MessagePart");
				var rfc822 = (MessagePart) message.Body;

				Assert.That (rfc822.ContentType.Name, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentType.Name");
				Assert.That (rfc822.ContentDisposition.Disposition, Is.EqualTo ("inline"), "MessagePart.ContentDisposition.Disposition");
				Assert.That (rfc822.ContentDisposition.FileName, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentDisposition.FileName");
				//Assert.That (rfc822.ContentTransferEncoding, Is.EqualTo (ContentEncoding.SevenBit), "MessagePart.ContentTransferEncoding");

				Assert.That (rfc822.Message, Is.Not.Null, "MessagePart.Message");
				Assert.That (rfc822.Message.MboxMarker, Is.Not.Null, "MessagePart.Message.MboxMarker");
				Assert.That (Encoding.ASCII.GetString (rfc822.Message.MboxMarker), Is.EqualTo (">From"));
				Assert.That (rfc822.Message.Headers.Count, Is.EqualTo (0), "MessagePart.Message.Headers.Count");
				Assert.That (rfc822.Message.Body.Headers.Count, Is.EqualTo (0), "MessagePart.Message.Body.Headers.Count");

				AssertSerialization (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestMessageRfc822WithTruncatedMungedFromMarkerAsync ()
		{
			string text = @"From - Fri Mar  7 02:51:22 1997
Return-Path: <jwz@netscape.com>
Received: from gruntle ([205.217.227.10]) by dredd.mcom.com
          (Netscape Mail Server v2.02) with SMTP id AAA4040
          for <jwz@netscape.com>; Fri, 7 Mar 1997 02:50:37 -0800
Sender: jwz@netscape.com (Jamie Zawinski)
Message-ID: <331FF2FF.FF6@netscape.com>
Date: Fri, 07 Mar 1997 02:50:39 -0800
From: Jamie Zawinski <jwz@netscape.com>
Organization: Netscape Communications Corporation, Mozilla Division
X-Mailer: Mozilla 3.01 (X11; U; IRIX 6.2 IP22)
MIME-Version: 1.0
To: Jamie Zawinski <jwz@netscape.com>
Subject: forwarded encrypted message
Content-Type: message/rfc822; name=""smime18-encrypted.msg""
Content-Transfer-Encoding: 7bit
Content-Disposition: inline; filename=""smime18-encrypted.msg""
X-Mozilla-Status: 0001
Content-Length: 2812

>From".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (ParserOptions.Default, stream, MimeFormat.Mbox);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<MessagePart> (), "Expected top-level to be a MessagePart");
				var rfc822 = (MessagePart) message.Body;

				Assert.That (rfc822.ContentType.Name, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentType.Name");
				Assert.That (rfc822.ContentDisposition.Disposition, Is.EqualTo ("inline"), "MessagePart.ContentDisposition.Disposition");
				Assert.That (rfc822.ContentDisposition.FileName, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentDisposition.FileName");
				//Assert.That (rfc822.ContentTransferEncoding, Is.EqualTo (ContentEncoding.SevenBit), "MessagePart.ContentTransferEncoding");

				Assert.That (rfc822.Message, Is.Not.Null, "MessagePart.Message");
				Assert.That (rfc822.Message.MboxMarker, Is.Not.Null, "MessagePart.Message.MboxMarker");
				Assert.That (Encoding.ASCII.GetString (rfc822.Message.MboxMarker), Is.EqualTo (">From"));
				Assert.That (rfc822.Message.Headers.Count, Is.EqualTo (0), "MessagePart.Message.Headers.Count");
				Assert.That (rfc822.Message.Body.Headers.Count, Is.EqualTo (0), "MessagePart.Message.Body.Headers.Count");

				await AssertSerializationAsync (message, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (ParserOptions.Default, stream, MimeFormat.Mbox);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<MessagePart> (), "Expected top-level to be a MessagePart");
				var rfc822 = (MessagePart) message.Body;

				Assert.That (rfc822.ContentType.Name, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentType.Name");
				Assert.That (rfc822.ContentDisposition.Disposition, Is.EqualTo ("inline"), "MessagePart.ContentDisposition.Disposition");
				Assert.That (rfc822.ContentDisposition.FileName, Is.EqualTo ("smime18-encrypted.msg"), "MessagePart.ContentDisposition.FileName");
				//Assert.That (rfc822.ContentTransferEncoding, Is.EqualTo (ContentEncoding.SevenBit), "MessagePart.ContentTransferEncoding");

				Assert.That (rfc822.Message, Is.Not.Null, "MessagePart.Message");
				Assert.That (rfc822.Message.MboxMarker, Is.Not.Null, "MessagePart.Message.MboxMarker");
				Assert.That (Encoding.ASCII.GetString (rfc822.Message.MboxMarker), Is.EqualTo (">From"));
				Assert.That (rfc822.Message.Headers.Count, Is.EqualTo (0), "MessagePart.Message.Headers.Count");
				Assert.That (rfc822.Message.Body.Headers.Count, Is.EqualTo (0), "MessagePart.Message.Body.Headers.Count");

				await AssertSerializationAsync (message, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestMessageRfc822 ()
		{
			string text = @"Content-Type: message/rfc822

From: mimekit@example.com
To: mimekit@example.com
Subject: embedded message
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Content-Type: text/plain; charset=utf-8

This is the rfc822 message body.
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var entity = parser.ParseEntity ();

				Assert.That (entity, Is.InstanceOf<MessagePart> (), "Expected message/rfc822");
				var rfc822 = (MessagePart) entity;

				Assert.That (rfc822.Message.Body, Is.InstanceOf<TextPart> (), "Expected child of the message/rfc822 to be text/plain");
				var body = (TextPart) rfc822.Message.Body;

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the rfc822 message body." + Environment.NewLine));

				AssertSerialization (entity, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var entity = parser.ParseEntity ();

				Assert.That (entity, Is.InstanceOf<MessagePart> (), "Expected message/rfc822");
				var rfc822 = (MessagePart) entity;

				Assert.That (rfc822.Message.Body, Is.InstanceOf<TextPart> (), "Expected child of the message/rfc822 to be text/plain");
				var body = (TextPart) rfc822.Message.Body;

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the rfc822 message body." + Environment.NewLine));

				AssertSerialization (entity, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestMessageRfc822Async ()
		{
			string text = @"Content-Type: message/rfc822

From: mimekit@example.com
To: mimekit@example.com
Subject: embedded message
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Content-Type: text/plain; charset=utf-8

This is the rfc822 message body.
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var entity = await parser.ParseEntityAsync ();

				Assert.That (entity, Is.InstanceOf<MessagePart> (), "Expected message/rfc822");
				var rfc822 = (MessagePart) entity;

				Assert.That (rfc822.Message.Body, Is.InstanceOf<TextPart> (), "Expected child of the message/rfc822 to be text/plain");
				var body = (TextPart) rfc822.Message.Body;

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the rfc822 message body." + Environment.NewLine));

				await AssertSerializationAsync (entity, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var entity = await parser.ParseEntityAsync ();

				Assert.That (entity, Is.InstanceOf<MessagePart> (), "Expected message/rfc822");
				var rfc822 = (MessagePart) entity;

				Assert.That (rfc822.Message.Body, Is.InstanceOf<TextPart> (), "Expected child of the message/rfc822 to be text/plain");
				var body = (TextPart) rfc822.Message.Body;

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the rfc822 message body." + Environment.NewLine));

				await AssertSerializationAsync (entity, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestMessageRfc822WithContentTransferEncoding ()
		{
			string text = @"Content-Type: message/rfc822
Content-Transfer-Encoding: 

From: mimekit@example.com
To: mimekit@example.com
Subject: embedded message
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Content-Type: text/plain; charset=utf-8

This is the rfc822 message body.
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var entity = parser.ParseEntity ();

				Assert.That (entity, Is.InstanceOf<MessagePart> (), "Expected message/rfc822");
				var rfc822 = (MessagePart) entity;

				Assert.That (rfc822.Message.Body, Is.InstanceOf<TextPart> (), "Expected child of the message/rfc822 to be text/plain");
				var body = (TextPart) rfc822.Message.Body;

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the rfc822 message body." + Environment.NewLine));

				AssertSerialization (entity, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var entity = parser.ParseEntity ();

				Assert.That (entity, Is.InstanceOf<MessagePart> (), "Expected message/rfc822");
				var rfc822 = (MessagePart) entity;

				Assert.That (rfc822.Message.Body, Is.InstanceOf<TextPart> (), "Expected child of the message/rfc822 to be text/plain");
				var body = (TextPart) rfc822.Message.Body;

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the rfc822 message body." + Environment.NewLine));

				AssertSerialization (entity, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestMessageRfc822WithContentTransferEncodingAsync ()
		{
			string text = @"Content-Type: message/rfc822
Content-Transfer-Encoding: 

From: mimekit@example.com
To: mimekit@example.com
Subject: embedded message
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Content-Type: text/plain; charset=utf-8

This is the rfc822 message body.
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var entity = await parser.ParseEntityAsync ();

				Assert.That (entity, Is.InstanceOf<MessagePart> (), "Expected message/rfc822");
				var rfc822 = (MessagePart) entity;

				Assert.That (rfc822.Message.Body, Is.InstanceOf<TextPart> (), "Expected child of the message/rfc822 to be text/plain");
				var body = (TextPart) rfc822.Message.Body;

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the rfc822 message body." + Environment.NewLine));

				await AssertSerializationAsync (entity, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var entity = await parser.ParseEntityAsync ();

				Assert.That (entity, Is.InstanceOf<MessagePart> (), "Expected message/rfc822");
				var rfc822 = (MessagePart) entity;

				Assert.That (rfc822.Message.Body, Is.InstanceOf<TextPart> (), "Expected child of the message/rfc822 to be text/plain");
				var body = (TextPart) rfc822.Message.Body;

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is the rfc822 message body." + Environment.NewLine));

				await AssertSerializationAsync (entity, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestMessageRfc822WithContentTransferEncodingBase64 ()
		{
			string text = @"Content-Type: message/rfc822
Content-Transfer-Encoding: base64

RnJvbTogbWltZWtpdEBleGFtcGxlLmNvbQpUbzogbWltZWtpdEBleGFtcGxlLmNvbQpTdWJqZWN0
OiBlbWJlZGRlZCBtZXNzYWdlCkRhdGU6IFR1ZSwgMTIgTm92IDIwMTMgMDk6MTI6NDIgLTA1MDAK
TUlNRS1WZXJzaW9uOiAxLjAKQ29udGVudC1UeXBlOiB0ZXh0L3BsYWluOyBjaGFyc2V0PXV0Zi04
CgpUaGlzIGlzIHRoZSByZmM4MjIgbWVzc2FnZSBib2R5Lgo=
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var entity = parser.ParseEntity ();

				Assert.That (entity, Is.InstanceOf<MimePart> (), "Expected message/rfc822 as a MimePart");
				var part = (MimePart) entity;

				using (var content = part.Content.Open ()) {
					parser = new MimeParser (content, MimeFormat.Entity);
					using var message = parser.ParseMessage ();

					Assert.That (message.Body, Is.InstanceOf<TextPart> (), "Expected child of the message/rfc822 to be text/plain");
					var body = (TextPart) message.Body;

					Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
					Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
					Assert.That (body.Text, Is.EqualTo ("This is the rfc822 message body." + Environment.NewLine));
				}

				AssertSerialization (entity, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var entity = parser.ParseEntity ();

				Assert.That (entity, Is.InstanceOf<MimePart> (), "Expected message/rfc822 as a MimePart");
				var part = (MimePart) entity;

				using (var content = part.Content.Open ()) {
					parser = new MimeParser (content, MimeFormat.Entity);
					using var message = parser.ParseMessage ();

					Assert.That (message.Body, Is.InstanceOf<TextPart> (), "Expected child of the message/rfc822 to be text/plain");
					var body = (TextPart) message.Body;

					Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
					Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
					Assert.That (body.Text, Is.EqualTo ("This is the rfc822 message body." + Environment.NewLine));
				}

				AssertSerialization (entity, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestMessageRfc822WithContentTransferEncodingBase64Async ()
		{
			string text = @"Content-Type: message/rfc822
Content-Transfer-Encoding: base64

RnJvbTogbWltZWtpdEBleGFtcGxlLmNvbQpUbzogbWltZWtpdEBleGFtcGxlLmNvbQpTdWJqZWN0
OiBlbWJlZGRlZCBtZXNzYWdlCkRhdGU6IFR1ZSwgMTIgTm92IDIwMTMgMDk6MTI6NDIgLTA1MDAK
TUlNRS1WZXJzaW9uOiAxLjAKQ29udGVudC1UeXBlOiB0ZXh0L3BsYWluOyBjaGFyc2V0PXV0Zi04
CgpUaGlzIGlzIHRoZSByZmM4MjIgbWVzc2FnZSBib2R5Lgo=
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var entity = await parser.ParseEntityAsync ();

				Assert.That (entity, Is.InstanceOf<MimePart> (), "Expected message/rfc822 as a MimePart");
				var part = (MimePart) entity;

				using (var content = part.Content.Open ()) {
					parser = new MimeParser (content, MimeFormat.Entity);
					using var message = await parser.ParseMessageAsync ();

					Assert.That (message.Body, Is.InstanceOf<TextPart> (), "Expected child of the message/rfc822 to be text/plain");
					var body = (TextPart) message.Body;

					Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
					Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
					Assert.That (body.Text, Is.EqualTo ("This is the rfc822 message body." + Environment.NewLine));
				}

				await AssertSerializationAsync (entity, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var entity = await parser.ParseEntityAsync ();

				Assert.That (entity, Is.InstanceOf<MimePart> (), "Expected message/rfc822 as a MimePart");
				var part = (MimePart) entity;

				using (var content = part.Content.Open ()) {
					parser = new MimeParser (content, MimeFormat.Entity);
					using var message = await parser.ParseMessageAsync ();

					Assert.That (message.Body, Is.InstanceOf<TextPart> (), "Expected child of the message/rfc822 to be text/plain");
					var body = (TextPart) message.Body;

					Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
					Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
					Assert.That (body.Text, Is.EqualTo ("This is the rfc822 message body." + Environment.NewLine));
				}

				await AssertSerializationAsync (entity, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestMimePartBasic ()
		{
			string text = @"Content-Type: application/octet-stream; name=rawData.dat
Content-Disposition: inline; filename=rawData.dat
Content-Transfer-Encoding: quoted-printable

This is some raw data.
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var entity = parser.ParseEntity ();

				Assert.That (entity, Is.InstanceOf<MimePart> (), "Expected MimePart");
				Assert.That (entity.ContentType.MimeType, Is.EqualTo ("application/octet-stream"), "MimeType");
				Assert.That (entity.ContentType.Name, Is.EqualTo ("rawData.dat"), "Name");
				var part = (MimePart) entity;

				var plain = new TextPart ("plain") {
					Content = part.Content
				};

				Assert.That (plain.Text, Is.EqualTo ("This is some raw data." + Environment.NewLine));

				AssertSerialization (entity, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var entity = parser.ParseEntity ();

				Assert.That (entity, Is.InstanceOf<MimePart> (), "Expected MimePart");
				Assert.That (entity.ContentType.MimeType, Is.EqualTo ("application/octet-stream"), "MimeType");
				Assert.That (entity.ContentType.Name, Is.EqualTo ("rawData.dat"), "Name");
				var part = (MimePart) entity;

				var plain = new TextPart ("plain") {
					Content = part.Content
				};

				Assert.That (plain.Text, Is.EqualTo ("This is some raw data." + Environment.NewLine));

				AssertSerialization (entity, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public async Task TestMimePartBasicAsync ()
		{
			string text = @"Content-Type: application/octet-stream; name=rawData.dat
Content-Disposition: inline; filename=rawData.dat
Content-Transfer-Encoding: quoted-printable

This is some raw data.
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var entity = await parser.ParseEntityAsync ();

				Assert.That (entity, Is.InstanceOf<MimePart> (), "Expected MimePart");
				Assert.That (entity.ContentType.MimeType, Is.EqualTo ("application/octet-stream"), "MimeType");
				Assert.That (entity.ContentType.Name, Is.EqualTo ("rawData.dat"), "Name");
				var part = (MimePart) entity;

				var plain = new TextPart ("plain") {
					Content = part.Content
				};

				Assert.That (plain.Text, Is.EqualTo ("This is some raw data." + Environment.NewLine));

				await AssertSerializationAsync (entity, NewLineFormat.Unix, text);
			}

			text = text.Replace ("\n", "\r\n");
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var entity = await parser.ParseEntityAsync ();

				Assert.That (entity, Is.InstanceOf<MimePart> (), "Expected MimePart");
				Assert.That (entity.ContentType.MimeType, Is.EqualTo ("application/octet-stream"), "MimeType");
				Assert.That (entity.ContentType.Name, Is.EqualTo ("rawData.dat"), "Name");
				var part = (MimePart) entity;

				var plain = new TextPart ("plain") {
					Content = part.Content
				};

				Assert.That (plain.Text, Is.EqualTo ("This is some raw data." + Environment.NewLine));

				await AssertSerializationAsync (entity, NewLineFormat.Dos, text);
			}
		}

		[Test]
		public void TestMimePartContentWithMixedLineEndings ()
		{
			string text = "From: mimekit@example.com\r\nTo: mimekit@example.com\r\nSubject: content with mixed line endings\r\nDate: Sat, Dec 21 2024 09:12:42 -0500\r\nMIME-Version: 1.0\r\nContent-Type: text/plain; charset=utf-8\r\n\r\nThis is a normal line of text.\r\nThis line ends with a bare LF.\nAnd this line ends with CRLF.\r\n";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Body, Is.InstanceOf<TextPart> (), "Expected body of the message to be text/plain");
				var body = (TextPart) message.Body;

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is a normal line of text.\r\nThis line ends with a bare LF.\r\nAnd this line ends with CRLF.\r\n".ReplaceLineEndings ()));
				Assert.That (body.Content.NewLineFormat, Is.EqualTo (NewLineFormat.Mixed));
			}
		}

		[Test]
		public async Task TestMimePartContentWithMixedLineEndingsAsync ()
		{
			string text = "From: mimekit@example.com\r\nTo: mimekit@example.com\r\nSubject: content with mixed line endings\r\nDate: Sat, Dec 21 2024 09:12:42 -0500\r\nMIME-Version: 1.0\r\nContent-Type: text/plain; charset=utf-8\r\n\r\nThis is a normal line of text.\r\nThis line ends with a bare LF.\nAnd this line ends with CRLF.\r\n";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Body, Is.InstanceOf<TextPart> (), "Expected body of the message to be text/plain");
				var body = (TextPart) message.Body;

				Assert.That (body.Headers[HeaderId.ContentType], Is.EqualTo ("text/plain; charset=utf-8"));
				Assert.That (body.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (body.Text, Is.EqualTo ("This is a normal line of text.\r\nThis line ends with a bare LF.\r\nAnd this line ends with CRLF.\r\n".ReplaceLineEndings ()));
				Assert.That (body.Content.NewLineFormat, Is.EqualTo (NewLineFormat.Mixed));
			}
		}

		[Test]
		public void TestGarbageBeforeMessageHeaders ()
		{
			string text = @">F!&^#%&^
From: mimekit@example.com
To: mimekit@example.com
Subject: This message has garbage before the headers
Date: Tue, 12 Nov 2013 09:12:42 -0500
MIME-Version: 1.0
Content-Type: text/plain; charset=utf-8

This is the message body.
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);

				Assert.Throws<FormatException> (() => parser.ParseMessage ());
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);

				Assert.Throws<FormatException> (() => parser.ParseMessage ());
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);

				Assert.ThrowsAsync<FormatException> (async () => await parser.ParseMessageAsync ());
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);

				Assert.ThrowsAsync<FormatException> (async () => await parser.ParseMessageAsync ());
			}
		}

		[Test]
		public void TestGarbageBeforeEntityHeaders ()
		{
			string text = @">F!&^#%&^
MIME-Version: 1.0
Content-Type: text/plain; charset=utf-8

This is the message body.
".Replace ("\r\n", "\n");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);

				Assert.Throws<FormatException> (() => parser.ParseEntity ());
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);

				Assert.Throws<FormatException> (() => parser.ParseEntity ());
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);

				Assert.ThrowsAsync<FormatException> (async () => await parser.ParseEntityAsync ());
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text.Replace ("\n", "\r\n")), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);

				Assert.ThrowsAsync<FormatException> (async () => await parser.ParseEntityAsync ());
			}
		}

		static void AssertSimpleMbox (Stream stream)
		{
			var parser = new MimeParser (stream, MimeFormat.Mbox);

			while (!parser.IsEndOfStream) {
				using var message = parser.ParseMessage ();
				Multipart multipart;
				MimeEntity entity;

				Assert.That (message.Body, Is.InstanceOf<Multipart> ());
				multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1));
				entity = multipart[0];

				Assert.That (entity, Is.InstanceOf<Multipart> ());
				multipart = (Multipart) entity;
				Assert.That (multipart.Count, Is.EqualTo (1));
				entity = multipart[0];

				Assert.That (entity, Is.InstanceOf<Multipart> ());
				multipart = (Multipart) entity;
				Assert.That (multipart.Count, Is.EqualTo (1));
				entity = multipart[0];

				Assert.That (entity, Is.InstanceOf<TextPart> ());

				using (var memory = new MemoryStream ()) {
					entity.WriteTo (UnixFormatOptions, memory);

					var text = Encoding.ASCII.GetString (memory.ToArray ());
					Assert.That (text.StartsWith ("Content-Type: text/plain\n\n", StringComparison.Ordinal), Is.True, "Headers are not properly terminated.");
				}
			}
		}

		[Test]
		public void TestSimpleMbox ()
		{
			using (var stream = File.OpenRead (Path.Combine (MboxDataDir, "simple.mbox.txt")))
				AssertSimpleMbox (stream);
		}

		static async Task AssertSimpleMboxAsync (Stream stream)
		{
			var parser = new MimeParser (stream, MimeFormat.Mbox);

			while (!parser.IsEndOfStream) {
				using var message = await parser.ParseMessageAsync ();
				Multipart multipart;
				MimeEntity entity;

				Assert.That (message.Body, Is.InstanceOf<Multipart> ());
				multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (1));
				entity = multipart[0];

				Assert.That (entity, Is.InstanceOf<Multipart> ());
				multipart = (Multipart) entity;
				Assert.That (multipart.Count, Is.EqualTo (1));
				entity = multipart[0];

				Assert.That (entity, Is.InstanceOf<Multipart> ());
				multipart = (Multipart) entity;
				Assert.That (multipart.Count, Is.EqualTo (1));
				entity = multipart[0];

				Assert.That (entity, Is.InstanceOf<TextPart> ());

				using (var memory = new MemoryStream ()) {
					await entity.WriteToAsync (UnixFormatOptions, memory);

					var text = Encoding.ASCII.GetString (memory.ToArray ());
					Assert.That (text.StartsWith ("Content-Type: text/plain\n\n", StringComparison.Ordinal), Is.True, "Headers are not properly terminated.");
				}
			}
		}

		[Test]
		public async Task TestSimpleMboxAsync ()
		{
			using (var stream = File.OpenRead (Path.Combine (MboxDataDir, "simple.mbox.txt")))
				await AssertSimpleMboxAsync (stream);
		}

		[Test]
		public void TestSimpleMboxWithByteOrderMark ()
		{
			using (var stream = new MemoryStream ()) {
				var bom = new byte[] { 0xEF, 0xBB, 0xBF };

				stream.Write (bom, 0, bom.Length);

				using (var file = File.OpenRead (Path.Combine (MboxDataDir, "simple.mbox.txt")))
					file.CopyTo (stream, 4096);

				stream.Position = 0;

				AssertSimpleMbox (stream);
			}
		}

		[Test]
		public async Task TestSimpleMboxWithByteOrderMarkAsync ()
		{
			using (var stream = new MemoryStream ()) {
				var bom = new byte[] { 0xEF, 0xBB, 0xBF };

				stream.Write (bom, 0, bom.Length);

				using (var file = File.OpenRead (Path.Combine (MboxDataDir, "simple.mbox.txt")))
					file.CopyTo (stream, 4096);

				stream.Position = 0;

				await AssertSimpleMboxAsync (stream);
			}
		}

		[TestCase (80)]
		[TestCase (4094)] // tests the not-enough-data code-path in MimeReader.StepMboxMarker()
		[TestCase (4096)]
		[TestCase (4097)] // tests midline code-paths in MimeReader.StepMboxMarkerStart()
		[TestCase (8193)] // tests the not-enough-data midline code-path in MimeReader.StepMboxMarkerStart()
		public void TestSimpleMboxWithGarbageBeforeMboxMarker (int garbageLength)
		{
			var garbage = new byte[garbageLength];

			for (int i = 0; i < garbageLength - 2; i++)
				garbage[i] = (byte) 'X';
			garbage[garbage.Length - 2] = (byte) '\r';
			garbage[garbage.Length - 1] = (byte) '\n';

			using (var stream = new MemoryStream ()) {
				stream.Write (garbage, 0, garbage.Length);

				using (var file = File.OpenRead (Path.Combine (MboxDataDir, "simple.mbox.txt")))
					file.CopyTo (stream, 4096);

				stream.Position = 0;

				AssertSimpleMbox (stream);
			}
		}

		[TestCase (80)]
		[TestCase (4094)] // tests the not-enough-data code-path in MimeReader.StepMboxMarker()
		[TestCase (4096)]
		[TestCase (4097)] // tests midline code-paths in MimeReader.StepMboxMarkerStart()
		[TestCase (8193)] // tests the not-enough-data midline code-path in MimeReader.StepMboxMarkerStart()
		public async Task TestSimpleMboxWithGarbageBeforeMboxMarkerAsync (int garbageLength)
		{
			var garbage = new byte[1024];

			for (int i = 0; i < garbage.Length - 2; i++)
				garbage[i] = (byte) 'X';
			garbage[garbage.Length - 2] = (byte) '\r';
			garbage[garbage.Length - 1] = (byte) '\n';

			using (var stream = new MemoryStream ()) {
				stream.Write (garbage, 0, garbage.Length);

				using (var file = File.OpenRead (Path.Combine (MboxDataDir, "simple.mbox.txt")))
					file.CopyTo (stream, 4096);

				stream.Position = 0;

				await AssertSimpleMboxAsync (stream);
			}
		}

		static void DumpMimeTree (StringBuilder builder, MimeEntity entity, int depth)
		{
			if (depth > 0)
				builder.Append (new string (' ', depth * 3));

			builder.AppendFormat ("Content-Type: {0}/{1}", entity.ContentType.MediaType, entity.ContentType.MediaSubtype).Append ('\n');

			if (entity is Multipart multipart) {
				foreach (var part in multipart)
					DumpMimeTree (builder, part, depth + 1);
			} else if (entity is MessagePart rfc822) {
				DumpMimeTree (builder, rfc822.Message.Body, depth + 1);
			}
		}

		static void DumpMimeTree (StringBuilder builder, MimeMessage message)
		{
			using (var iter = new MimeIterator (message)) {
				while (iter.MoveNext ()) {
					var ctype = iter.Current.ContentType;

					if (iter.Depth > 0)
						builder.Append (new string (' ', iter.Depth * 3));

					builder.AppendFormat ("Content-Type: {0}/{1}", ctype.MediaType, ctype.MediaSubtype).Append ('\n');
				}
			}
		}

		[Test]
		public void TestEmptyMultipartAlternative ()
		{
			string expected = @"Content-Type: multipart/mixed
   Content-Type: multipart/alternative
   Content-Type: text/plain
".Replace ("\r\n", "\n");

			using (var stream = File.OpenRead (Path.Combine (MessagesDataDir, "empty-multipart.txt"))) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();
				var builder = new StringBuilder ();

				DumpMimeTree (builder, message);

				Assert.That (builder.ToString (), Is.EqualTo (expected), "Unexpected MIME tree structure.");
			}
		}

		static NewLineFormat DetectNewLineFormat (string fileName)
		{
			using (var stream = File.OpenRead (fileName)) {
				var buffer = new byte[1024];

				var nread = stream.Read (buffer, 0, buffer.Length);

				for (int i = 0; i < nread; i++) {
					if (buffer[i] == (byte) '\n') {
						if (i > 0 && buffer[i - 1] == (byte) '\r')
							return NewLineFormat.Dos;

						return NewLineFormat.Unix;
					}
				}
			}

			return NewLineFormat.Dos;
		}

		class MimeOffsets
		{
			[JsonProperty ("mimeType", NullValueHandling = NullValueHandling.Ignore)]
			public string MimeType { get; set; }

			[JsonProperty ("mboxMarkerOffset", NullValueHandling = NullValueHandling.Ignore)]
			public long? MboxMarkerOffset { get; set; }

			[JsonProperty ("lineNumber")]
			public int LineNumber { get; set; }

			[JsonProperty ("beginOffset")]
			public long BeginOffset { get; set; }

			[JsonProperty ("headersEndOffset")]
			public long HeadersEndOffset { get; set; }

			[JsonProperty ("endOffset")]
			public long EndOffset { get; set; }

			[JsonProperty ("message", NullValueHandling = NullValueHandling.Ignore)]
			public MimeOffsets Message { get; set; }

			[JsonProperty ("body", NullValueHandling = NullValueHandling.Ignore)]
			public MimeOffsets Body { get; set; }

			[JsonProperty ("children", NullValueHandling = NullValueHandling.Ignore)]
			public List<MimeOffsets> Children { get; set; }

			[JsonProperty ("octets")]
			public long Octets { get; set; }

			[JsonProperty ("lines", NullValueHandling = NullValueHandling.Ignore)]
			public int? Lines { get; set; }
		}

		static void AssertMimeOffsets (MimeOffsets expected, MimeOffsets actual, int message, string partSpecifier)
		{
			Assert.That (actual.MimeType, Is.EqualTo (expected.MimeType), $"mime-type differs for message #{message}{partSpecifier}");
			Assert.That (actual.MboxMarkerOffset, Is.EqualTo (expected.MboxMarkerOffset), $"mbox marker begin offset differs for message #{message}{partSpecifier}");
			Assert.That (actual.BeginOffset, Is.EqualTo (expected.BeginOffset), $"begin offset differs for message #{message}{partSpecifier}");
			Assert.That (actual.LineNumber, Is.EqualTo (expected.LineNumber), $"begin line differs for message #{message}{partSpecifier}");
			Assert.That (actual.HeadersEndOffset, Is.EqualTo (expected.HeadersEndOffset), $"headers end offset differs for message #{message}{partSpecifier}");
			Assert.That (actual.EndOffset, Is.EqualTo (expected.EndOffset), $"end offset differs for message #{message}{partSpecifier}");
			Assert.That (actual.Octets, Is.EqualTo (expected.Octets), $"octets differs for message #{message}{partSpecifier}");
			Assert.That (actual.Lines, Is.EqualTo (expected.Lines), $"lines differs for message #{message}{partSpecifier}");

			if (expected.Message != null) {
				Assert.That (actual.Message, Is.Not.Null, $"message content is null for message #{message}{partSpecifier}");
				AssertMimeOffsets (expected.Message, actual.Message, message, partSpecifier + "/message");
			} else if (expected.Body != null) {
				Assert.That (actual.Body, Is.Not.Null, $"body content is null for message #{message}{partSpecifier}");
				AssertMimeOffsets (expected.Body, actual.Body, message, partSpecifier + "/0");
			} else if (expected.Children != null) {
				Assert.That (actual.Children.Count, Is.EqualTo (expected.Children.Count), $"children count differs for message #{message}{partSpecifier}");
				for (int i = 0; i < expected.Children.Count; i++)
					AssertMimeOffsets (expected.Children[i], actual.Children[i], message, partSpecifier + $".{i}");
			}
		}

		class CustomMimeParser : MimeParser
		{
			public readonly List<MimeOffsets> Offsets = new List<MimeOffsets> ();
			readonly Stack<MimeOffsets> stack = new Stack<MimeOffsets> ();
			bool isMessageBody;

			public CustomMimeParser (ParserOptions options, Stream stream, MimeFormat format, bool persistent = false) : base (options, stream, format, persistent ? MimeContentStorage.Source : MimeContentStorage.Memory)
			{
			}

			public CustomMimeParser (Stream stream, MimeFormat format) : base (stream, format)
			{
			}

			protected override void OnMimeMessageBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				var offsets = new MimeOffsets {
					BeginOffset = beginOffset,
					LineNumber = beginLineNumber
				};

				if (stack.Count > 0) {
					var parentOffsets = stack.Peek ();
					parentOffsets.Message = offsets;
				} else {
					offsets.MboxMarkerOffset = MboxMarkerOffset;
					Offsets.Add (offsets);
				}

				isMessageBody = true;
				stack.Push (offsets);

				base.OnMimeMessageBegin (beginOffset, beginLineNumber, cancellationToken);
			}

			protected override void OnMimeMessageEnd (long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				var offsets = stack.Pop ();
				offsets.Octets = endOffset - headersEndOffset;
				offsets.HeadersEndOffset = headersEndOffset;
				offsets.EndOffset = endOffset;

				base.OnMimeMessageEnd (beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			}

			void OnMimeEntityBegin (ContentType contentType, long beginOffset, int beginLineNumber)
			{
				var offsets = new MimeOffsets {
					MimeType = contentType.MimeType,
					BeginOffset = beginOffset,
					LineNumber = beginLineNumber
				};

				var parentOffsets = stack.Peek ();
				if (isMessageBody) {
					parentOffsets.Body = offsets;
					isMessageBody = false;
				} else {
					parentOffsets.Children ??= new List<MimeOffsets> ();
					parentOffsets.Children.Add (offsets);
				}

				stack.Push (offsets);
			}

			void OnMimeEntityEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines)
			{
				var offsets = stack.Pop ();
				offsets.Octets = endOffset - headersEndOffset;
				offsets.HeadersEndOffset = headersEndOffset;
				offsets.EndOffset = endOffset;
				offsets.Lines = lines;
			}

			protected override void OnMessagePartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				OnMimeEntityBegin (contentType, beginOffset, beginLineNumber);
				base.OnMessagePartBegin (contentType, beginOffset, beginLineNumber, cancellationToken);
			}

			protected override void OnMessagePartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				OnMimeEntityEnd (contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines);
				base.OnMessagePartEnd (contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			}

			protected override void OnMultipartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				OnMimeEntityBegin (contentType, beginOffset, beginLineNumber);
				base.OnMultipartBegin (contentType, beginOffset, beginLineNumber, cancellationToken);
			}

			protected override void OnMultipartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				OnMimeEntityEnd (contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines);
				base.OnMultipartEnd (contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			}

			protected override void OnMimePartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				OnMimeEntityBegin (contentType, beginOffset, beginLineNumber);
				base.OnMimePartBegin (contentType, beginOffset, beginLineNumber, cancellationToken);
			}

			protected override void OnMimePartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				OnMimeEntityEnd (contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines);
				base.OnMimePartEnd (contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			}
		}

		static IEnumerable<string> RoundTripMessageFiles ()
		{
			var dataDir = Path.Combine (TestHelper.ProjectDir, "TestData");
			var sources = new (string Directory, string Pattern)[] {
				("messages", "*.eml"),
				("messages", "*.txt"),
				("compliance", "*.eml"),
				("partial", "*.eml"),
				("openpgp", "*.eml"),
				("dkim", "*.msg"),
				("smime", "*-signed.txt"),
			};

			foreach (var source in sources) {
				var files = Directory.GetFiles (Path.Combine (dataDir, source.Directory), source.Pattern);

				Array.Sort (files, StringComparer.Ordinal);

				foreach (var file in files)
					yield return Path.Combine (source.Directory, Path.GetFileName (file));
			}
		}

		static byte[] GetExpectedRoundTripOutput (string path, out MimeFormat format)
		{
			var expected = File.ReadAllBytes (path);

			// Note: The parser skips over a leading UTF-8 BOM, so it will not be re-serialized.
			if (expected.Length >= 3 && expected[0] == 0xEF && expected[1] == 0xBB && expected[2] == 0xBF)
				expected = expected.AsSpan (3).ToArray ();

			// Note: Files that start with an mbox "From " marker need to be parsed as mbox in order to preserve the marker.
			if (expected.Length >= 5 && expected[0] == (byte) 'F' && expected[1] == (byte) 'r' && expected[2] == (byte) 'o' && expected[3] == (byte) 'm' && expected[4] == (byte) ' ')
				format = MimeFormat.Mbox;
			else
				format = MimeFormat.Entity;

			return expected;
		}

		static void AssertRoundTrip (string fileName, byte[] expected, MemoryStream output)
		{
			var actual = output.ToArray ();
			int i;

			for (i = 0; i < Math.Min (expected.Length, actual.Length); i++) {
				if (actual[i] != expected[i])
					break;
			}

			if (i == expected.Length && i == actual.Length)
				return;

			var start = Math.Max (0, i - 64);
			var strExpected = CharsetUtils.Latin1.GetString (expected, start, Math.Min (128, expected.Length - start));
			var strActual = CharsetUtils.Latin1.GetString (actual, start, Math.Min (128, actual.Length - start));

			Assert.That (strActual, Is.EqualTo (strExpected), $"The re-serialized output of {fileName} differs from the original at offset {i} (expected length {expected.Length}, actual length {actual.Length}).");
		}

		[TestCaseSource (nameof (RoundTripMessageFiles))]
		public void TestRoundTripTestDataMessages (string fileName)
		{
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", fileName);
			var expected = GetExpectedRoundTripOutput (path, out var mimeFormat);
			var parserOptions = new ParserOptions { MaxHeaderLength = 16 * 1024 * 1024 };
			var options = FormatOptions.Default.Clone ();

			options.NewLineFormat = DetectNewLineFormat (path);

			foreach (var persistent in new bool[] { false, true }) {
				using var stream = new MemoryStream (expected, false);
				var parser = new MimeParser (parserOptions, stream, mimeFormat, persistent ? MimeContentStorage.Source : MimeContentStorage.Memory);
				using var output = new MemoryStream ();

				using var message = parser.ParseMessage ();

				if (mimeFormat == MimeFormat.Mbox) {
					var marker = Encoding.UTF8.GetBytes (parser.MboxMarker + options.NewLine);
					output.Write (marker, 0, marker.Length);
				}

				message.WriteTo (options, output);

				Assert.That (parser.IsEndOfStream, Is.True, $"Expected {fileName} to contain only a single message.");
				AssertRoundTrip ($"{fileName} (persistent={persistent})", expected, output);
			}
		}

		[TestCaseSource (nameof (RoundTripMessageFiles))]
		public async Task TestRoundTripTestDataMessagesAsync (string fileName)
		{
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", fileName);
			var expected = GetExpectedRoundTripOutput (path, out var mimeFormat);
			var parserOptions = new ParserOptions { MaxHeaderLength = 16 * 1024 * 1024 };
			var options = FormatOptions.Default.Clone ();

			options.NewLineFormat = DetectNewLineFormat (path);

			foreach (var persistent in new bool[] { false, true }) {
				using var stream = new MemoryStream (expected, false);
				var parser = new MimeParser (parserOptions, stream, mimeFormat, persistent ? MimeContentStorage.Source : MimeContentStorage.Memory);
				using var output = new MemoryStream ();

				using var message = await parser.ParseMessageAsync ();

				if (mimeFormat == MimeFormat.Mbox) {
					var marker = Encoding.UTF8.GetBytes (parser.MboxMarker + options.NewLine);
					await output.WriteAsync (marker, 0, marker.Length);
				}

				await message.WriteToAsync (options, output);

				Assert.That (parser.IsEndOfStream, Is.True, $"Expected {fileName} to contain only a single message.");
				AssertRoundTrip ($"{fileName} (persistent={persistent})", expected, output);
			}
		}

		static void AssertMboxResults (string baseName, string actual, Stream output, List<MimeOffsets> offsets, NewLineFormat newLineFormat)
		{
			// WORKAROUND: Mono's iso-2022-jp decoder breaks on this input in versions <= 3.2.3 but is fixed in 3.2.4+
			string iso2022jp = Encoding.GetEncoding ("iso-2022-jp").GetString (Convert.FromBase64String ("GyRAOjRGI0stGyhK"));
			if (iso2022jp != "佐藤豊")
				actual = actual.Replace (iso2022jp, "佐藤豊");

			var path = Path.Combine (MboxDataDir, baseName + "-summary.txt");
			if (!File.Exists (path))
				File.WriteAllText (path, actual);

			var summary = File.ReadAllText (path).Replace ("\r\n", "\n");
			var expected = new byte[4096];
			var buffer = new byte[4096];
			int nx, n;

			Assert.That (actual, Is.EqualTo (summary), $"Summaries do not match for {baseName}.mbox");

			using (var original = File.OpenRead (Path.Combine (MboxDataDir, baseName + ".mbox.txt"))) {
				int lineNumber = 1, columnNumber = 1;

				output.Position = 0;

				Assert.That (output.Length, Is.EqualTo (original.Length), "The length of the mbox did not match.");

				do {
					nx = original.Read (expected, 0, expected.Length);
					n = output.Read (buffer, 0, nx);

					if (nx == 0)
						break;

					for (int i = 0; i < nx; i++) {
						if (buffer[i] == expected[i]) {
							if (expected[i] == (byte) '\n') {
								columnNumber = 1;
								lineNumber++;
							} else {
								columnNumber++;
							}
							continue;
						}

						var strExpected = CharsetUtils.Latin1.GetString (expected, 0, nx);
						var strActual = CharsetUtils.Latin1.GetString (buffer, 0, n);

						Assert.That (strActual, Is.EqualTo (strExpected), $"The mbox differs at on line {lineNumber}, column {columnNumber}");
					}
				} while (true);
			}

			var jsonSerializer = JsonSerializer.CreateDefault ();

			path = Path.Combine (MboxDataDir, baseName + "." + newLineFormat.ToString ().ToLowerInvariant () + "-offsets.json");
			if (!File.Exists (path)) {
				jsonSerializer.Formatting = Formatting.Indented;

				using (var writer = new StreamWriter (path))
					jsonSerializer.Serialize (writer, offsets);
			}

			using (var reader = new StreamReader (path)) {
				var expectedOffsets = (List<MimeOffsets>) jsonSerializer.Deserialize (reader, typeof (List<MimeOffsets>));

				Assert.That (offsets.Count, Is.EqualTo (expectedOffsets.Count), "message count");

				for (int i = 0; i < expectedOffsets.Count; i++)
					AssertMimeOffsets (expectedOffsets[i], offsets[i], i, string.Empty);
			}
		}

		static void TestMbox (ParserOptions options, string baseName)
		{
			var mbox = Path.Combine (MboxDataDir, baseName + ".mbox.txt");
			using var output = new MemoryBlockStream ();
			var builder = new StringBuilder ();
			NewLineFormat newLineFormat;
			List<MimeOffsets> offsets;

			using (var stream = File.OpenRead (mbox)) {
				var parser = options != null ? new CustomMimeParser (options, stream, MimeFormat.Mbox) : new CustomMimeParser (stream, MimeFormat.Mbox);
				var format = FormatOptions.Default.Clone ();
				int count = 0;

				format.NewLineFormat = newLineFormat = DetectNewLineFormat (mbox);

				Assert.That (parser.MboxMarkerOffset, Is.EqualTo (-1), "Initial MboxMarkerOffset");
				Assert.That (parser.MboxMarker, Is.Null, "Initial MboxMarker");

				while (!parser.IsEndOfStream) {
					using var message = parser.ParseMessage ();

					builder.AppendFormat ("{0}", parser.MboxMarker).Append ('\n');
					if (message.From.Count > 0)
						builder.AppendFormat ("From: {0}", message.From).Append ('\n');
					if (message.To.Count > 0)
						builder.AppendFormat ("To: {0}", message.To).Append ('\n');
					builder.AppendFormat ("Subject: {0}", message.Subject).Append ('\n');
					builder.AppendFormat ("Date: {0}", DateUtils.FormatDate (message.Date)).Append ('\n');
					DumpMimeTree (builder, message);
					builder.Append ('\n');

					var marker = Encoding.UTF8.GetBytes ((count > 0 ? format.NewLine : string.Empty) + parser.MboxMarker + format.NewLine);
					output.Write (marker, 0, marker.Length);
					message.WriteTo (format, output);
					count++;
				}

				offsets = parser.Offsets;
			}

			AssertMboxResults (baseName, builder.ToString (), output, offsets, newLineFormat);
		}

		static async Task TestMboxAsync (ParserOptions options, string baseName)
		{
			var mbox = Path.Combine (MboxDataDir, baseName + ".mbox.txt");
			using var output = new MemoryBlockStream ();
			var builder = new StringBuilder ();
			NewLineFormat newLineFormat;
			List<MimeOffsets> offsets;

			using (var stream = File.OpenRead (mbox)) {
				var parser = options != null ? new CustomMimeParser (options, stream, MimeFormat.Mbox) : new CustomMimeParser (stream, MimeFormat.Mbox);
				var format = FormatOptions.Default.Clone ();
				int count = 0;

				format.NewLineFormat = newLineFormat = DetectNewLineFormat (mbox);

				Assert.That (parser.MboxMarkerOffset, Is.EqualTo (-1), "Initial MboxMarkerOffset");
				Assert.That (parser.MboxMarker, Is.Null, "Initial MboxMarker");

				while (!parser.IsEndOfStream) {
					using var message = await parser.ParseMessageAsync ();

					builder.AppendFormat ("{0}", parser.MboxMarker).Append ('\n');
					if (message.From.Count > 0)
						builder.AppendFormat ("From: {0}", message.From).Append ('\n');
					if (message.To.Count > 0)
						builder.AppendFormat ("To: {0}", message.To).Append ('\n');
					builder.AppendFormat ("Subject: {0}", message.Subject).Append ('\n');
					builder.AppendFormat ("Date: {0}", DateUtils.FormatDate (message.Date)).Append ('\n');
					DumpMimeTree (builder, message);
					builder.Append ('\n');

					var marker = Encoding.UTF8.GetBytes ((count > 0 ? format.NewLine : string.Empty) + parser.MboxMarker + format.NewLine);
					await output.WriteAsync (marker, 0, marker.Length);
					await message.WriteToAsync (format, output);
					count++;
				}

				offsets = parser.Offsets;
			}

			AssertMboxResults (baseName, builder.ToString (), output, offsets, newLineFormat);
		}

		[Test]
		public void TestContentLengthMbox ()
		{
			var options = ParserOptions.Default.Clone ();
			options.RespectContentLength = true;

			TestMbox (options, "content-length");
		}

		[Test]
		public async Task TestContentLengthMboxAsync ()
		{
			var options = ParserOptions.Default.Clone ();
			options.RespectContentLength = true;

			await TestMboxAsync (options, "content-length");
		}

		[Test]
		public void TestIssue1189Mbox ()
		{
			TestMbox (null, "issue1189");
		}

		[Test]
		public async Task TestIssue1189MboxAsync ()
		{
			await TestMboxAsync (null, "issue1189");
		}

		[Test]
		public void TestJwzMbox ()
		{
			TestMbox (null, "jwz");
		}

		[Test]
		public async Task TestJwzMboxAsync ()
		{
			await TestMboxAsync (null, "jwz");
		}

		[Test]
		public void TestJwzPersistentMbox ()
		{
			var summary = File.ReadAllText (Path.Combine (MboxDataDir, "jwz-summary.txt")).Replace ("\r\n", "\n");
			var builder = new StringBuilder ();

			using (var stream = File.OpenRead (Path.Combine (MboxDataDir, "jwz.mbox.txt"))) {
				var parser = new MimeParser (stream, MimeFormat.Mbox, MimeContentStorage.Source);

				Assert.That (parser.MboxMarkerOffset, Is.EqualTo (-1), "Initial MboxMarkerOffset");
				Assert.That (parser.MboxMarker, Is.Null, "Initial MboxMarker");

				while (!parser.IsEndOfStream) {
					using var message = parser.ParseMessage ();

					builder.AppendFormat ("{0}", parser.MboxMarker).Append ('\n');
					if (message.From.Count > 0)
						builder.AppendFormat ("From: {0}", message.From).Append ('\n');
					if (message.To.Count > 0)
						builder.AppendFormat ("To: {0}", message.To).Append ('\n');
					builder.AppendFormat ("Subject: {0}", message.Subject).Append ('\n');
					builder.AppendFormat ("Date: {0}", DateUtils.FormatDate (message.Date)).Append ('\n');
					DumpMimeTree (builder, message);
					builder.Append ('\n');

					// Force the various MimePart objects to write their content streams.
					// The idea is that by forcing the MimeParts to seek in their content,
					// we will test to make sure that the parser correctly deals with it.
					message.WriteTo (Stream.Null);
				}
			}

			string actual = builder.ToString ();

			// WORKAROUND: Mono's iso-2022-jp decoder breaks on this input in versions <= 3.2.3 but is fixed in 3.2.4+
			string iso2022jp = Encoding.GetEncoding ("iso-2022-jp").GetString (Convert.FromBase64String ("GyRAOjRGI0stGyhK"));
			if (iso2022jp != "佐藤豊")
				actual = actual.Replace (iso2022jp, "佐藤豊");

			Assert.That (actual, Is.EqualTo (summary), "Summaries do not match for jwz.mbox");
		}

		[Test]
		public async Task TestJwzPersistentMboxAsync ()
		{
			var summary = File.ReadAllText (Path.Combine (MboxDataDir, "jwz-summary.txt")).Replace ("\r\n", "\n");
			var builder = new StringBuilder ();

			using (var stream = File.OpenRead (Path.Combine (MboxDataDir, "jwz.mbox.txt"))) {
				var parser = new MimeParser (stream, MimeFormat.Mbox, MimeContentStorage.Source);

				Assert.That (parser.MboxMarkerOffset, Is.EqualTo (-1), "Initial MboxMarkerOffset");
				Assert.That (parser.MboxMarker, Is.Null, "Initial MboxMarker");

				while (!parser.IsEndOfStream) {
					using var message = await parser.ParseMessageAsync ();

					builder.AppendFormat ("{0}", parser.MboxMarker).Append ('\n');
					if (message.From.Count > 0)
						builder.AppendFormat ("From: {0}", message.From).Append ('\n');
					if (message.To.Count > 0)
						builder.AppendFormat ("To: {0}", message.To).Append ('\n');
					builder.AppendFormat ("Subject: {0}", message.Subject).Append ('\n');
					builder.AppendFormat ("Date: {0}", DateUtils.FormatDate (message.Date)).Append ('\n');
					DumpMimeTree (builder, message);
					builder.Append ('\n');

					// Force the various MimePart objects to write their content streams.
					// The idea is that by forcing the MimeParts to seek in their content,
					// we will test to make sure that the parser correctly deals with it.
					await message.WriteToAsync (Stream.Null);
				}
			}

			string actual = builder.ToString ();

			// WORKAROUND: Mono's iso-2022-jp decoder breaks on this input in versions <= 3.2.3 but is fixed in 3.2.4+
			string iso2022jp = Encoding.GetEncoding ("iso-2022-jp").GetString (Convert.FromBase64String ("GyRAOjRGI0stGyhK"));
			if (iso2022jp != "佐藤豊")
				actual = actual.Replace (iso2022jp, "佐藤豊");

			Assert.That (actual, Is.EqualTo (summary), "Summaries do not match for jwz.mbox");
		}

		[Test]
		public void TestJapaneseMessage ()
		{
			const string subject = "日本語メールテスト (testing Japanese emails)";
			const string body = "Let's see if both subject and body works fine...\n\n日本語が\n正常に\n送れているか\nテスト.\n";

			using (var stream = File.OpenRead (Path.Combine (MessagesDataDir, "japanese.txt"))) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				Assert.That (message.Subject, Is.EqualTo (subject), "Subject values do not match");
				Assert.That (message.TextBody.Replace ("\r\n", "\n"), Is.EqualTo (body), "Message text does not match.");
			}
		}

		[Test]
		public async Task TestJapaneseMessageAsync ()
		{
			const string subject = "日本語メールテスト (testing Japanese emails)";
			const string body = "Let's see if both subject and body works fine...\n\n日本語が\n正常に\n送れているか\nテスト.\n";

			using (var stream = File.OpenRead (Path.Combine (MessagesDataDir, "japanese.txt"))) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				Assert.That (message.Subject, Is.EqualTo (subject), "Subject values do not match");
				Assert.That (message.TextBody.Replace ("\r\n", "\n"), Is.EqualTo (body), "Message text does not match.");
			}
		}

		[Test]
		public void TestUnmungedFromLines ()
		{
			int count = 0;

			using (var stream = File.OpenRead (Path.Combine (MboxDataDir, "unmunged.mbox.txt"))) {
				var parser = new MimeParser (stream, MimeFormat.Mbox);

				while (!parser.IsEndOfStream) {
					using var message = parser.ParseMessage ();
					var marker = parser.MboxMarker;

					if ((count % 2) == 0) {
						Assert.That (marker.TrimEnd (), Is.EqualTo ("From -"), $"Message #{count}");
					} else {
						Assert.That (marker.TrimEnd (), Is.EqualTo ("From Russia with love"), $"Message #{count}");
					}

					count++;
				}
			}

			Assert.That (count, Is.EqualTo (4), "Expected to find 4 messages.");
		}

		[Test]
		public async Task TestUnmungedFromLinesAsync ()
		{
			int count = 0;

			using (var stream = File.OpenRead (Path.Combine (MboxDataDir, "unmunged.mbox.txt"))) {
				var parser = new MimeParser (stream, MimeFormat.Mbox);

				while (!parser.IsEndOfStream) {
					using var message = await parser.ParseMessageAsync ();
					var marker = parser.MboxMarker;

					if ((count % 2) == 0) {
						Assert.That (marker.TrimEnd (), Is.EqualTo ("From -"), $"Message #{count}");
					} else {
						Assert.That (marker.TrimEnd (), Is.EqualTo ("From Russia with love"), $"Message #{count}");
					}

					count++;
				}
			}

			Assert.That (count, Is.EqualTo (4), "Expected to find 4 messages.");
		}

		[Test]
		public void TestMultipartEpilogueWithText ()
		{
			const string epilogue = "Peter Urka <pcu@umich.edu>\nDept. of Chemistry, Univ. of Michigan\nNewt-thought is right-thought.  Go Newt!\n\n";

			using (var stream = File.OpenRead (Path.Combine (MessagesDataDir, "epilogue.txt"))) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();
				var multipart = message.Body as Multipart;

				Assert.That (multipart.Epilogue.Replace ("\r\n", "\n"), Is.EqualTo (epilogue), "The epilogue does not match");

				Assert.That (multipart.RawEpilogue[0] == (byte) '\r' || multipart.RawEpilogue[0] == (byte) '\n', Is.True,
					"The RawEpilogue does not start with a new-line.");
			}
		}

		[Test]
		public async Task TestMultipartEpilogueWithTextAsync ()
		{
			const string epilogue = "Peter Urka <pcu@umich.edu>\nDept. of Chemistry, Univ. of Michigan\nNewt-thought is right-thought.  Go Newt!\n\n";

			using (var stream = File.OpenRead (Path.Combine (MessagesDataDir, "epilogue.txt"))) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();
				var multipart = message.Body as Multipart;

				Assert.That (multipart.Epilogue.Replace ("\r\n", "\n"), Is.EqualTo (epilogue), "The epilogue does not match");

				Assert.That (multipart.RawEpilogue[0] == (byte) '\r' || multipart.RawEpilogue[0] == (byte) '\n', Is.True,
							   "The RawEpilogue does not start with a new-line.");
			}
		}

		[Test]
		public void TestMissingSubtype ()
		{
			using (var stream = File.OpenRead (Path.Combine (MessagesDataDir, "missing-subtype.txt"))) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();
				var type = message.Body.ContentType;

				Assert.That (type.MediaType, Is.EqualTo ("application"), "The media type is not the default.");
				Assert.That (type.MediaSubtype, Is.EqualTo ("octet-stream"), "The media subtype is not the default.");
				Assert.That (type.Name, Is.EqualTo ("document.xml.gz"), "The parameters do not seem to have been parsed.");
			}
		}

		[Test]
		public async Task TestMissingSubtypeAsync ()
		{
			using (var stream = File.OpenRead (Path.Combine (MessagesDataDir, "missing-subtype.txt"))) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();
				var type = message.Body.ContentType;

				Assert.That (type.MediaType, Is.EqualTo ("application"), "The media type is not the default.");
				Assert.That (type.MediaSubtype, Is.EqualTo ("octet-stream"), "The media subtype is not the default.");
				Assert.That (type.Name, Is.EqualTo ("document.xml.gz"), "The parameters do not seem to have been parsed.");
			}
		}

		[Test]
		public void TestMissingMessageBody ()
		{
			const string text = "Date: Sat, 19 Apr 2014 13:13:23 -0700\r\n" +
				"From: Jeffrey Stedfast <notifications@github.com>\r\n" +
				"Subject: Re: [MimeKit] Allow parsing of message with 0 byte body. (#51)\r\n";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				try {
					var parser = new MimeParser (stream, MimeFormat.Entity);
					using var message = parser.ParseMessage ();
				} catch {
					Assert.Fail ("A message with 0 bytes of content should not fail to parse.");
				}
			}
		}

		[Test]
		public async Task TestMissingMessageBodyAsync ()
		{
			const string text = "Date: Sat, 19 Apr 2014 13:13:23 -0700\r\n" +
				"From: Jeffrey Stedfast <notifications@github.com>\r\n" +
				"Subject: Re: [MimeKit] Allow parsing of message with 0 byte body. (#51)\r\n";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				try {
					var parser = new MimeParser (stream, MimeFormat.Entity);
					using var message = await parser.ParseMessageAsync ();
				} catch {
					Assert.Fail ("A message with 0 bytes of content should not fail to parse.");
				}
			}
		}

		static byte[] GenerateDeeplyNestedRfc822Message (int depth)
		{
			var builder = new StringBuilder ();
			builder.Append ("From: tester1@contoso.com\r\n");
			builder.Append ("To: tester1@contoso.com\r\n");
			builder.Append ("Subject: test of a deeply nested multipart message\r\n");
			builder.Append ($"Date: {DateUtils.FormatDate (DateTimeOffset.Now)}\r\n");
			builder.Append ($"Message-ID: <{MimeUtils.GenerateMessageId ("contoso.com")}>\r\n");
			builder.Append ("MIME-Version: 1.0\r\n");

			var now = DateTimeOffset.Now;

			for (int i = 0; i < depth; i++) {
				builder.Append ($"Content-Type: message/rfc822\r\n");
				builder.Append ("\r\n");
				builder.Append ("From: tester1@contoso.com\r\n");
				builder.Append ("To: tester1@contoso.com\r\n");
				builder.Append ($"Subject: embedded message {i + 1}\r\n");
				builder.Append ($"Date: {DateUtils.FormatDate (now.Subtract (TimeSpan.FromHours (1)))}\r\n");
				builder.Append ($"Message-ID: <{MimeUtils.GenerateMessageId ("contoso.com")}>\r\n");
				builder.Append ("MIME-Version: 1.0\r\n");
			}

			builder.Append ("Content-Type: text/plain; charset=\"us-ascii\"\r\n");
			builder.Append ("Content-Transfer-Encoding: 7bit\r\n");
			builder.Append ("\r\n");
			builder.Append ("This is the innermost part of a deeply nested rfc822 message.\r\n");

			return Encoding.ASCII.GetBytes (builder.ToString ());
		}

		[Test]
		public void TestDeeplyNestedMessageRfc822Parts ()
		{
			const int maxDepth = 64;

			var messageData = GenerateDeeplyNestedRfc822Message (maxDepth);

			using (var stream = new MemoryStream (messageData, false)) {
				var options = ParserOptions.Default.Clone ();
				options.MaxMimeDepth = maxDepth;

				var parser = new MimeParser (options, stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				using (var iter = new MimeIterator (message)) {
					string body = null;
					int depth = -1;

					while (iter.MoveNext ()) {
						if (iter.Current is TextPart text) {
							depth = iter.Depth;
							body = text.Text;
						}
					}

					Assert.That (depth, Is.EqualTo (maxDepth), "The maximum depth did not match.");
					Assert.That (body, Is.Not.Null, "Did not find the message body.");
					Assert.That (body, Is.EqualTo ("This is the innermost part of a deeply nested rfc822 message." + Environment.NewLine), "Message body did not match.");
				}
			}

			using (var stream = new MemoryStream (messageData, false)) {
				var options = ParserOptions.Default.Clone ();
				options.MaxMimeDepth = maxDepth - 1;

				var parser = new MimeParser (options, stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				using (var iter = new MimeIterator (message)) {
					MimePart body = null;
					int depth = -1;

					while (iter.MoveNext ()) {
						if (iter.Current is MimePart part) {
							depth = iter.Depth;
							body = part;
						}
					}

					Assert.That (depth, Is.EqualTo (maxDepth - 1), "The maximum depth did not match.");
					Assert.That (body, Is.Not.Null, "Did not find the message body.");
					Assert.That (body.ContentType.MimeType, Is.EqualTo ("message/rfc822"), "Message body did not match.");
				}
			}
		}

		[Test]
		public async Task TestDeeplyNestedMessageRfc822PartsAsync ()
		{
			const int maxDepth = 64;

			var messageData = GenerateDeeplyNestedRfc822Message (maxDepth);

			using (var stream = new MemoryStream (messageData, false)) {
				var options = ParserOptions.Default.Clone ();
				options.MaxMimeDepth = maxDepth;

				var parser = new MimeParser (options, stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				using (var iter = new MimeIterator (message)) {
					string body = null;
					int depth = -1;

					while (iter.MoveNext ()) {
						if (iter.Current is TextPart text) {
							depth = iter.Depth;
							body = text.Text;
						}
					}

					Assert.That (depth, Is.EqualTo (maxDepth), "The maximum depth did not match.");
					Assert.That (body, Is.Not.Null, "Did not find the message body.");
					Assert.That (body, Is.EqualTo ("This is the innermost part of a deeply nested rfc822 message." + Environment.NewLine), "Message body did not match.");
				}
			}

			using (var stream = new MemoryStream (messageData, false)) {
				var options = ParserOptions.Default.Clone ();
				options.MaxMimeDepth = maxDepth - 1;

				var parser = new MimeParser (options, stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				using (var iter = new MimeIterator (message)) {
					MimePart body = null;
					int depth = -1;

					while (iter.MoveNext ()) {
						if (iter.Current is MimePart part) {
							depth = iter.Depth;
							body = part;
						}
					}

					Assert.That (depth, Is.EqualTo (maxDepth - 1), "The maximum depth did not match.");
					Assert.That (body, Is.Not.Null, "Did not find the message body.");
					Assert.That (body.ContentType.MimeType, Is.EqualTo ("message/rfc822"), "Message body did not match.");
				}
			}
		}

		static byte[] GenerateDeeplyNestedMultipartMessage (int depth)
		{
			var builder = new StringBuilder ();
			builder.Append ("From: tester1@contoso.com\r\n");
			builder.Append ("To: tester1@contoso.com\r\n");
			builder.Append ("Subject: test of a deeply nested multipart message\r\n");
			builder.Append ($"Date: {DateUtils.FormatDate (DateTimeOffset.Now)}\r\n");
			builder.Append ($"Message-ID: <{MimeUtils.GenerateMessageId ("contoso.com")}>\r\n");
			builder.Append ("MIME-Version: 1.0\r\n");

			for (int i = 0; i < depth; i++) {
				builder.Append ($"Content-Type: multipart/mixed;\r\n\tboundary=\"----=_NextPart_000_{i:04}_01CE98CE.6E826F90\"\r\n");
				builder.Append ("\r\n");
				builder.Append ($"------=_NextPart_000_{i:04}_01CE98CE.6E826F90\r\n");
			}

			builder.Append ("Content-Type: text/plain; charset=\"us-ascii\"\r\n");
			builder.Append ("Content-Transfer-Encoding: 7bit\r\n");
			builder.Append ("\r\n");
			builder.Append ("This is the innermost part of a deeply nested multipart message.\r\n");
			builder.Append ("\r\n");

			for (int i = depth - 1; i >= 0; i--)
				builder.Append ($"------=_NextPart_000_{i:04}_01CE98CE.6E826F90--\r\n");

			return Encoding.ASCII.GetBytes (builder.ToString ());
		}

		[Test]
		public void TestDeeplyNestedMultiparts ()
		{
			const int maxDepth = 64;

			var messageData = GenerateDeeplyNestedMultipartMessage (maxDepth);

			using (var stream = new MemoryStream (messageData, false)) {
				var options = ParserOptions.Default.Clone ();
				options.MaxMimeDepth = maxDepth;

				var parser = new MimeParser (options, stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				using (var iter = new MimeIterator (message)) {
					string body = null;
					int depth = -1;

					while (iter.MoveNext ()) {
						if (iter.Current is TextPart text) {
							depth = iter.Depth;
							body = text.Text;
						}
					}

					Assert.That (depth, Is.EqualTo (maxDepth), "The maximum depth did not match.");
					Assert.That (body, Is.Not.Null, "Did not find the message body.");
					Assert.That (body, Is.EqualTo ("This is the innermost part of a deeply nested multipart message." + Environment.NewLine), "Message body did not match.");
				}
			}

			using (var stream = new MemoryStream (messageData, false)) {
				var options = ParserOptions.Default.Clone ();
				options.MaxMimeDepth = maxDepth - 1;

				var parser = new MimeParser (options, stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				using (var iter = new MimeIterator (message)) {
					MimePart body = null;
					int depth = -1;

					while (iter.MoveNext ()) {
						if (iter.Current is MimePart part) {
							depth = iter.Depth;
							body = part;
						}
					}

					Assert.That (depth, Is.EqualTo (maxDepth - 1), "The maximum depth did not match.");
					Assert.That (body, Is.Not.Null, "Did not find the message body.");
					Assert.That (body.ContentType.MimeType, Is.EqualTo ("multipart/mixed"), "Message body did not match.");
				}
			}
		}

		[Test]
		public async Task TestDeeplyNestedMultipartsAsync ()
		{
			const int maxDepth = 64;

			var messageData = GenerateDeeplyNestedMultipartMessage (maxDepth);

			using (var stream = new MemoryStream (messageData, false)) {
				var options = ParserOptions.Default.Clone ();
				options.MaxMimeDepth = maxDepth;

				var parser = new MimeParser (options, stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				using (var iter = new MimeIterator (message)) {
					string body = null;
					int depth = -1;

					while (iter.MoveNext ()) {
						if (iter.Current is TextPart text) {
							depth = iter.Depth;
							body = text.Text;
						}
					}

					Assert.That (depth, Is.EqualTo (maxDepth), "The maximum depth did not match.");
					Assert.That (body, Is.Not.Null, "Did not find the message body.");
					Assert.That (body, Is.EqualTo ("This is the innermost part of a deeply nested multipart message." + Environment.NewLine), "Message body did not match.");
				}
			}

			using (var stream = new MemoryStream (messageData, false)) {
				var options = ParserOptions.Default.Clone ();
				options.MaxMimeDepth = maxDepth - 1;

				var parser = new MimeParser (options, stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				using (var iter = new MimeIterator (message)) {
					MimePart body = null;
					int depth = -1;

					while (iter.MoveNext ()) {
						if (iter.Current is MimePart part) {
							depth = iter.Depth;
							body = part;
						}
					}

					Assert.That (depth, Is.EqualTo (maxDepth - 1), "The maximum depth did not match.");
					Assert.That (body, Is.Not.Null, "Did not find the message body.");
					Assert.That (body.ContentType.MimeType, Is.EqualTo ("multipart/mixed"), "Message body did not match.");
				}
			}
		}

		static string GetEntityTreeSummary (MimeMessage message)
		{
			var builder = new StringBuilder ();

			using (var iter = new MimeIterator (message)) {
				while (iter.MoveNext ())
					builder.Append ($"[{iter.Depth}:{iter.Current.GetType ().Name}]");
			}

			return builder.ToString ();
		}

		// Note: MimeReader.GetEntityType() and ParserOptions.CreateEntity() must apply the exact same
		// comparison against MaxMimeDepth. When they disagree, MimeParser casts the entity returned by
		// CreateEntity() to the type implied by whichever On*Begin() callback fired and throws an
		// InvalidCastException. The TestDeeplyNested* tests only probe MaxMimeDepth values of maxDepth
		// and maxDepth - 1, so they cannot catch an off-by-one; these sweep MaxMimeDepth across the
		// entire truncation boundary and assert that both parsers produce identical trees.
		[TestCase (true, TestName = "TestMaxMimeDepthMatchesLegacyMimeParser(multipart)")]
		[TestCase (false, TestName = "TestMaxMimeDepthMatchesLegacyMimeParser(message/rfc822)")]
		public void TestMaxMimeDepthMatchesLegacyMimeParser (bool multipart)
		{
			const int nesting = 5;

			var messageData = multipart
				? GenerateDeeplyNestedMultipartMessage (nesting)
				: GenerateDeeplyNestedRfc822Message (nesting);

			for (int maxMimeDepth = 0; maxMimeDepth <= nesting + 2; maxMimeDepth++) {
				var options = ParserOptions.Default.Clone ();
				options.MaxMimeDepth = maxMimeDepth;

				string expected, actual;

				using (var stream = new MemoryStream (messageData, false)) {
					var parser = new LegacyMimeParser (options, stream, MimeFormat.Entity);
					using var message = parser.ParseMessage ();

					expected = GetEntityTreeSummary (message);
				}

				using (var stream = new MemoryStream (messageData, false)) {
					var parser = new MimeParser (options, stream, MimeFormat.Entity);
					using var message = parser.ParseMessage ();

					actual = GetEntityTreeSummary (message);
				}

				Assert.That (actual, Is.EqualTo (expected), $"MimeParser did not match LegacyMimeParser at MaxMimeDepth={maxMimeDepth}.");
			}
		}

		[TestCase (true, TestName = "TestMaxMimeDepthMatchesLegacyMimeParserAsync(multipart)")]
		[TestCase (false, TestName = "TestMaxMimeDepthMatchesLegacyMimeParserAsync(message/rfc822)")]
		public async Task TestMaxMimeDepthMatchesLegacyMimeParserAsync (bool multipart)
		{
			const int nesting = 5;

			var messageData = multipart
				? GenerateDeeplyNestedMultipartMessage (nesting)
				: GenerateDeeplyNestedRfc822Message (nesting);

			for (int maxMimeDepth = 0; maxMimeDepth <= nesting + 2; maxMimeDepth++) {
				var options = ParserOptions.Default.Clone ();
				options.MaxMimeDepth = maxMimeDepth;

				string expected, actual;

				using (var stream = new MemoryStream (messageData, false)) {
					var parser = new LegacyMimeParser (options, stream, MimeFormat.Entity);
					using var message = await parser.ParseMessageAsync ();

					expected = GetEntityTreeSummary (message);
				}

				using (var stream = new MemoryStream (messageData, false)) {
					var parser = new MimeParser (options, stream, MimeFormat.Entity);
					using var message = await parser.ParseMessageAsync ();

					actual = GetEntityTreeSummary (message);
				}

				Assert.That (actual, Is.EqualTo (expected), $"MimeParser did not match LegacyMimeParser at MaxMimeDepth={maxMimeDepth}.");
			}
		}
		[Test]
		public void TestIssue358 ()
		{
			// Note: This particular message has a badly folded header value for "x-microsoft-exchange-diagnostics:"
			// which was causing LegacyMimeParser.StepHeaders[Async]() to abort because ReadAhead() already had more than
			// ReadAheadSize bytes buffered, so it assumed it had reached EOF when in fact it had not.
			using (var stream = File.OpenRead (Path.Combine (MessagesDataDir, "issue358.txt"))) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (new Unix2DosFilter ());

					var parser = new MimeParser (filtered, MimeFormat.Entity);
					using var message = parser.ParseMessage ();

					// make sure that the top-level MIME part is a multipart/alternative
					Assert.That (message.Body, Is.InstanceOf<MultipartAlternative> ());
				}
			}
		}

		[Test]
		public async Task TestIssue358Async ()
		{
			// Note: This particular message has a badly folded header value for "x-microsoft-exchange-diagnostics:"
			// which was causing LegacyMimeParser.StepHeaders[Async]() to abort because ReadAhead() already had more than
			// ReadAheadSize bytes buffered, so it assumed it had reached EOF when in fact it had not.
			using (var stream = File.OpenRead (Path.Combine (MessagesDataDir, "issue358.txt"))) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (new Unix2DosFilter ());

					var parser = new MimeParser (filtered, MimeFormat.Entity);
					using var message = await parser.ParseMessageAsync ();

					// make sure that the top-level MIME part is a multipart/alternative
					Assert.That (message.Body, Is.InstanceOf<MultipartAlternative> ());
				}
			}
		}

		[Test]
		public void TestLineCountSingleLine ()
		{
			const string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a message with a single line of text
Message-Id: <123@example.org>
MIME-Version: 1.0
Content-Type: text/plain; charset=us-ascii

This is a single line of text";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new CustomMimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				var lines = parser.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
			}
		}

		[Test]
		public async Task TestLineCountSingleLineAsync ()
		{
			const string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a message with a single line of text
Message-Id: <123@example.org>
MIME-Version: 1.0
Content-Type: text/plain; charset=us-ascii

This is a single line of text";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new CustomMimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				var lines = parser.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
			}
		}

		[Test]
		public void TestLineCountSingleLineCRLF ()
		{
			const string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a message with a single line of text
Message-Id: <123@example.org>
MIME-Version: 1.0
Content-Type: text/plain; charset=us-ascii

This is a single line of text
";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new CustomMimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				var lines = parser.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
			}
		}

		[Test]
		public async Task TestLineCountSingleLineCRLFAsync ()
		{
			const string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a message with a single line of text
Message-Id: <123@example.org>
MIME-Version: 1.0
Content-Type: text/plain; charset=us-ascii

This is a single line of text
";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new CustomMimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				var lines = parser.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
			}
		}

		[Test]
		public void TestLineCountSingleLineInMultipart ()
		{
			const string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a message with a single line of text
Message-Id: <123@example.org>
MIME-Version: 1.0
Content-Type: multipart/mixed; boundary=""boundary-marker""

--boundary-marker
Content-Type: text/plain; charset=us-ascii

This is a single line of text
--boundary-marker
Content-Type: application/octet-stream; name=""attachment.dat""
Content-Disposition: attachment; filename=""attachment.dat""

ABC
--boundary-marker--
";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new CustomMimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				var lines = parser.Offsets[0].Body.Children[0].Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
			}
		}

		[Test]
		public async Task TestLineCountSingleLineInMultipartAsync ()
		{
			const string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a message with a single line of text
Message-Id: <123@example.org>
MIME-Version: 1.0
Content-Type: multipart/mixed; boundary=""boundary-marker""

--boundary-marker
Content-Type: text/plain; charset=us-ascii

This is a single line of text
--boundary-marker
Content-Type: application/octet-stream; name=""attachment.dat""
Content-Disposition: attachment; filename=""attachment.dat""

ABC
--boundary-marker--
";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new CustomMimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				var lines = parser.Offsets[0].Body.Children[0].Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
			}
		}

		[Test]
		public void TestLineCountOneLineOfTextFollowedByBlankLineInMultipart ()
		{
			const string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a message with a single line of text
Message-Id: <123@example.org>
MIME-Version: 1.0
Content-Type: multipart/mixed; boundary=""boundary-marker""

--boundary-marker
Content-Type: text/plain; charset=us-ascii

This is a single line of text followed by a blank line

--boundary-marker
Content-Type: application/octet-stream; name=""attachment.dat""
Content-Disposition: attachment; filename=""attachment.dat""

ABC
--boundary-marker--
";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new CustomMimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				var lines = parser.Offsets[0].Body.Children[0].Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
			}
		}

		[Test]
		public async Task TestLineCountOneLineOfTextFollowedByBlankLineInMultipartAsync ()
		{
			const string text = @"From: mimekit@example.org
To: mimekit@example.org
Subject: This is a message with a single line of text
Message-Id: <123@example.org>
MIME-Version: 1.0
Content-Type: multipart/mixed; boundary=""boundary-marker""

--boundary-marker
Content-Type: text/plain; charset=us-ascii

This is a single line of text followed by a blank line

--boundary-marker
Content-Type: application/octet-stream; name=""attachment.dat""
Content-Disposition: attachment; filename=""attachment.dat""

ABC
--boundary-marker--
";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new CustomMimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				var lines = parser.Offsets[0].Body.Children[0].Lines;

				Assert.That (lines, Is.EqualTo (1), "Line count");
			}
		}

		[Test]
		public void TestLineCountNonTerminatedSingleHeader ()
		{
			const string text = "From: mimekit@example.org";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new CustomMimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				var lines = parser.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (0), "Line count");
			}
		}

		[Test]
		public async Task TestLineCountNonTerminatedSingleHeaderAsync ()
		{
			const string text = "From: mimekit@example.org";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new CustomMimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				var lines = parser.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (0), "Line count");
			}
		}

		[Test]
		public void TestLineCountProperlyTerminatedSingleHeader ()
		{
			const string text = "From: mimekit@example.org\r\n";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new CustomMimeParser (stream, MimeFormat.Entity);
				using var message = parser.ParseMessage ();

				var lines = parser.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (0), "Line count");
			}
		}

		[Test]
		public async Task TestLineCountProperlyTerminatedSingleHeaderAsync ()
		{
			const string text = "From: mimekit@example.org\r\n";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var parser = new CustomMimeParser (stream, MimeFormat.Entity);
				using var message = await parser.ParseMessageAsync ();

				var lines = parser.Offsets[0].Body.Lines;

				Assert.That (lines, Is.EqualTo (0), "Line count");
			}
		}

		internal static MemoryStream CreateIssue991Mbox (out long[] expectedOffsets)
		{
			const string template = @"From: mimekit@example.org
To: mimekit@example.org
Subject: {0}
Message-Id: <1234567890.{1}@example.org>
MIME-Version: 1.0
Content-Type: text/plain; charset=utf-8

";
			const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			const int lineLength = 255;

			var mboxMarker = Encoding.ASCII.GetBytes ("From -\n");
			var memory = new MemoryStream ();

			expectedOffsets = new long[2];

			// Write the first message
			memory.Write (mboxMarker, 0, mboxMarker.Length);

			var headers = string.Format (template, "This message contains long lines that will cause ScanContent() to require a buffer refill", 0);
			var headerBuffer = Encoding.ASCII.GetBytes (headers);
			memory.Write (headerBuffer, 0, headerBuffer.Length);

			int index = 0;

			while (memory.Length <= 4096) {
				char c = Alphabet[index % Alphabet.Length];
				for (int i = 0; i < lineLength && memory.Length < 4096; i++)
					memory.WriteByte ((byte) c);

				if (memory.Length < 4096) {
					memory.WriteByte ((byte) '\n');
				} else {
					// fake mbox marker that is midline
					memory.Write (mboxMarker, 0, mboxMarker.Length);
				}

				index++;
			}

			memory.WriteByte ((byte) '\n');

			expectedOffsets[0] = memory.Length;

			// Write the second message
			memory.Write (mboxMarker, 0, mboxMarker.Length);

			headers = string.Format (template, "This message contains long lines that will cause ScanContent() to require a buffer refill", 1);
			headerBuffer = Encoding.ASCII.GetBytes (headers);
			memory.Write (headerBuffer, 0, headerBuffer.Length);

			while (memory.Length <= expectedOffsets[0] + 4096) {
				char c = Alphabet[index % Alphabet.Length];
				for (int i = 0; i < lineLength; i++)
					memory.WriteByte ((byte) c);

				memory.WriteByte ((byte) '\n');
				index++;
			}

			memory.WriteByte ((byte) '\n');

			expectedOffsets[1] = memory.Length;

			// reset the stream for parsing
			memory.Position = 0;

			return memory;
		}

		[Test]
		public void TestIssue991 ()
		{
			using (var memory = CreateIssue991Mbox (out var expectedOffsets)) {
				var parser = new MimeParser (memory, MimeFormat.Mbox);
				int i = 0;

				while (!parser.IsEndOfStream) {
					using var message = parser.ParseMessage ();

					Assert.That (parser.Position, Is.EqualTo (expectedOffsets[i]), "The parser did not stop at the end of the first message.");
					Assert.That (message.MessageId, Is.EqualTo ($"1234567890.{i}@example.org"));
					i++;
				}
			}
		}

		[Test]
		public async Task TestIssue991Async ()
		{
			using (var memory = CreateIssue991Mbox (out var expectedOffsets)) {
				var parser = new MimeParser (memory, MimeFormat.Mbox);
				int i = 0;

				while (!parser.IsEndOfStream) {
					using var message = await parser.ParseMessageAsync ();

					Assert.That (parser.Position, Is.EqualTo (expectedOffsets[i]), "The parser did not stop at the end of the first message.");
					Assert.That (message.MessageId, Is.EqualTo ($"1234567890.{i}@example.org"));
					i++;
				}
			}
		}

		internal static MemoryStream CreateMboxWithLinesExceedingMaxSmtpLineLength (out long[] expectedOffsets)
		{
			const string template = @"From: mimekit@example.org
To: mimekit@example.org
Subject: {0}
Message-Id: <1234567890.{1}@example.org>
MIME-Version: 1.0
Content-Type: text/plain; charset=utf-8

";
			const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			const int lineLength = 255;

			var mboxMarker = Encoding.ASCII.GetBytes ("From -\n");
			var memory = new MemoryStream ();

			expectedOffsets = new long[2];

			// Write the first message
			memory.Write (mboxMarker, 0, mboxMarker.Length);

			var headers = string.Format (template, "This message contains long lines that will cause ScanContent() to require a buffer refill", 0);
			var headerBuffer = Encoding.ASCII.GetBytes (headers);
			memory.Write (headerBuffer, 0, headerBuffer.Length);

			int index = 0;
			char c;

			while (memory.Length <= 4096 - 1001) {
				c = Alphabet[index % Alphabet.Length];

				for (int i = 0; i < lineLength && memory.Length < 4096 - 1001; i++)
					memory.WriteByte ((byte) c);

				memory.WriteByte ((byte) '\n');
				index++;
			}

			// Write the line that will exceed SmtpMaxLineLength *and* span across read boundaries
			c = Alphabet[index % Alphabet.Length];

			for (int i = 0; memory.Length < 4096; i++)
				memory.WriteByte ((byte) c);

			memory.Write (mboxMarker, 0, mboxMarker.Length);
			index++;

			// Write another line data
			c = Alphabet[index % Alphabet.Length];

			for (int i = 0; i < lineLength; i++)
				memory.WriteByte ((byte) c);

			memory.Write (mboxMarker, 0, mboxMarker.Length);
			index++;

			memory.WriteByte ((byte) '\n');

			expectedOffsets[0] = memory.Length;

			// Write the second message
			memory.Write (mboxMarker, 0, mboxMarker.Length);

			headers = string.Format (template, "This message contains long lines that will cause ScanContent() to require a buffer refill", 1);
			headerBuffer = Encoding.ASCII.GetBytes (headers);
			memory.Write (headerBuffer, 0, headerBuffer.Length);

			while (memory.Length <= expectedOffsets[0] + 4096) {
				c = Alphabet[index % Alphabet.Length];
				for (int i = 0; i < lineLength; i++)
					memory.WriteByte ((byte) c);

				memory.WriteByte ((byte) '\n');
				index++;
			}

			memory.WriteByte ((byte) '\n');

			expectedOffsets[1] = memory.Length;

			// reset the stream for parsing
			memory.Position = 0;

			return memory;
		}

		[Test]
		public void TestMboxWithLinesExceedingMaxSmtpLineLength ()
		{
			using (var memory = CreateMboxWithLinesExceedingMaxSmtpLineLength (out var expectedOffsets)) {
				var parser = new MimeParser (memory, MimeFormat.Mbox);
				int i = 0;

				while (!parser.IsEndOfStream) {
					using var message = parser.ParseMessage ();

					Assert.That (parser.Position, Is.EqualTo (expectedOffsets[i]), "The parser did not stop at the end of the first message.");
					Assert.That (message.MessageId, Is.EqualTo ($"1234567890.{i}@example.org"));
					i++;
				}
			}
		}

		[Test]
		public async Task TestMboxWithLinesExceedingMaxSmtpLineLengthAsync ()
		{
			using (var memory = CreateMboxWithLinesExceedingMaxSmtpLineLength (out var expectedOffsets)) {
				var parser = new MimeParser (memory, MimeFormat.Mbox);
				int i = 0;

				while (!parser.IsEndOfStream) {
					using var message = await parser.ParseMessageAsync ();

					Assert.That (parser.Position, Is.EqualTo (expectedOffsets[i]), "The parser did not stop at the end of the first message.");
					Assert.That (message.MessageId, Is.EqualTo ($"1234567890.{i}@example.org"));
					i++;
				}
			}
		}

		static byte[] CreateMessageWithHugeFoldedHeader (int lines, out byte[] expectedRawValue, out long subjectOffset)
		{
			var value = new StringBuilder ();
			var builder = new StringBuilder ();

			value.Append (" participant0@example.com");
			for (int i = 1; i < lines; i++)
				value.Append (",\r\n participant").Append (i).Append ("@example.com");
			value.Append ("\r\n");

			builder.Append ("From: sender@example.com\r\n");
			builder.Append ("To:").Append (value);
			subjectOffset = builder.Length;
			builder.Append ("Subject: huge header\r\n");
			builder.Append ("\r\n");
			builder.Append ("This is the body.\r\n");

			expectedRawValue = Encoding.ASCII.GetBytes (value.ToString ());

			return Encoding.ASCII.GetBytes (builder.ToString ());
		}

		static void AssertHugeFoldedHeader (MimeMessage message, byte[] expectedRawValue, long subjectOffset, int lines)
		{
			Assert.That (message.Headers.Count, Is.EqualTo (3));
			Assert.That (message.Headers[1].Field, Is.EqualTo ("To"));
			Assert.That (message.Headers[1].RawValue, Is.EqualTo (expectedRawValue), "RawValue");
			Assert.That (message.Headers[2].Field, Is.EqualTo ("Subject"));
			Assert.That (message.Headers[2].Offset, Is.EqualTo (subjectOffset), "Subject Offset");
			Assert.That (message.Subject, Is.EqualTo ("huge header"));
			Assert.That (message.To.Count, Is.EqualTo (lines));
			Assert.That (((TextPart) message.Body).Text, Is.EqualTo ("This is the body." + Environment.NewLine));
		}

		[Test]
		public void TestHugeFoldedHeader ([Values (1, 100, 50000)] int lines)
		{
			var data = CreateMessageWithHugeFoldedHeader (lines, out var expectedRawValue, out var subjectOffset);
			var options = new ParserOptions { MaxHeaderLength = 16 * 1024 * 1024 };

			using (var stream = new MemoryStream (data, false)) {
				var parser = new MimeParser (options, stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				AssertHugeFoldedHeader (message, expectedRawValue, subjectOffset, lines);
			}

			using (var stream = new ChunkedReadStream (data, 7)) {
				var parser = new MimeParser (options, stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				AssertHugeFoldedHeader (message, expectedRawValue, subjectOffset, lines);
			}
		}

		[Test]
		public async Task TestHugeFoldedHeaderAsync ([Values (1, 100, 50000)] int lines)
		{
			var data = CreateMessageWithHugeFoldedHeader (lines, out var expectedRawValue, out var subjectOffset);
			var options = new ParserOptions { MaxHeaderLength = 16 * 1024 * 1024 };

			using (var stream = new MemoryStream (data, false)) {
				var parser = new MimeParser (options, stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				AssertHugeFoldedHeader (message, expectedRawValue, subjectOffset, lines);
			}

			using (var stream = new ChunkedReadStream (data, 7)) {
				var parser = new MimeParser (options, stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				AssertHugeFoldedHeader (message, expectedRawValue, subjectOffset, lines);
			}
		}

		// Regression cases inspired by https://github.com/noxxi/mime-is-broken.
		static readonly string[] CommentedAndEncodedBoundaries = {
			"boundary*=''audit",
			"boundary*='en'audit",
			"boundary*0*=''; boundary*1*=audit",
			"boundary=audit(comment)",
			"boundary=audit\r\n\t(nested (comment); ignored=yes)"
		};

		static byte[] CreateCommentedParameterMessage (string boundary, string delimiter = "audit")
		{
			return Encoding.ASCII.GetBytes ("From: sender@example.com\r\n" +
				"Date: Sat, 31 Oct 2026 10:00:00 -0400\r\nMIME-Version: 1.0\r\n" +
				"Content-Type: multipart/mixed; " + boundary + "\r\n\r\n" +
				"--" + delimiter + "\r\nContent-Type: application/octet-stream; name=file.zip(comment)\r\n" +
				"Content-Disposition: attachment; filename*=''file%2Ezip(comment)\r\n" +
				"Content-Transfer-Encoding: base64\r\n\r\naGVsbG8=\r\n--" + delimiter + "--\r\n");
		}

		static void AssertCommentedParameterMessage (MimeMessage message, string boundary = "audit")
		{
			Assert.That (message.Body, Is.InstanceOf<Multipart> ());
			var multipart = (Multipart) message.Body;
			Assert.That (multipart.Boundary, Is.EqualTo (boundary));
			Assert.That (multipart.Count, Is.EqualTo (1));
			Assert.That (multipart[0], Is.InstanceOf<MimePart> ());
			var part = (MimePart) multipart[0];
			Assert.That (part.ContentType.Name, Is.EqualTo ("file.zip"));
			Assert.That (part.ContentDisposition.FileName, Is.EqualTo ("file.zip"));
			using (var decoded = new MemoryStream ()) {
				part.Content.DecodeTo (decoded);
				Assert.That (decoded.ToArray (), Is.EqualTo (Encoding.ASCII.GetBytes ("hello")));
			}
		}

		[Test]
		public void TestCommentedAndEncodedBoundary ([ValueSource (nameof (CommentedAndEncodedBoundaries))] string boundary, [Values (false, true)] bool chunked)
		{
			using (var stream = CreateStream (CreateCommentedParameterMessage (boundary), chunked))
			using (var message = MimeMessage.Load (stream))
				AssertCommentedParameterMessage (message);
		}

		[Test]
		public async Task TestCommentedAndEncodedBoundaryAsync ([ValueSource (nameof (CommentedAndEncodedBoundaries))] string boundary, [Values (false, true)] bool chunked)
		{
			using (var stream = CreateStream (CreateCommentedParameterMessage (boundary), chunked))
			using (var message = await MimeMessage.LoadAsync (stream))
				AssertCommentedParameterMessage (message);
		}

		static void AssertLiteralParenthesesBoundary (MimeMessage message, bool quoted)
		{
			if (quoted) {
				AssertCommentedParameterMessage (message, "audit(comment)");
			} else {
				Assert.That (message.Body, Is.InstanceOf<Multipart> ());
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Boundary, Is.EqualTo ("audit"));
				Assert.That (multipart.Count, Is.EqualTo (0));
			}
		}

		// Literal-parenthesis boundary variants from https://github.com/noxxi/mime-is-broken require quoting.
		[Test]
		public void TestLiteralParenthesesBoundary ([Values (false, true)] bool quoted, [Values (false, true)] bool chunked)
		{
			string boundary = quoted ? "boundary=\"audit(comment)\"" : "boundary=audit(comment)";

			using (var stream = CreateStream (CreateCommentedParameterMessage (boundary, "audit(comment)"), chunked))
			using (var message = MimeMessage.Load (stream))
				AssertLiteralParenthesesBoundary (message, quoted);
		}

		[Test]
		public async Task TestLiteralParenthesesBoundaryAsync ([Values (false, true)] bool quoted, [Values (false, true)] bool chunked)
		{
			string boundary = quoted ? "boundary=\"audit(comment)\"" : "boundary=audit(comment)";

			using (var stream = CreateStream (CreateCommentedParameterMessage (boundary, "audit(comment)"), chunked))
			using (var message = await MimeMessage.LoadAsync (stream))
				AssertLiteralParenthesesBoundary (message, quoted);
		}

		static void DescribeStructure (StringBuilder builder, MimeEntity entity, int depth)
		{
			builder.Append (' ', depth * 2).Append (entity.ContentType.MimeType).Append (" headers=").Append (entity.Headers.Count).Append ('\n');

			if (entity is Multipart multipart) {
				foreach (var child in multipart)
					DescribeStructure (builder, child, depth + 1);
			} else if (entity is MessagePart rfc822 && rfc822.Message != null) {
				builder.Append (' ', (depth + 1) * 2).Append ("message headers=").Append (rfc822.Message.Headers.Count).Append ('\n');

				if (rfc822.Message.Body != null)
					DescribeStructure (builder, rfc822.Message.Body, depth + 1);
			}
		}

		static string DescribeStructure (MimeMessage message)
		{
			var builder = new StringBuilder ();

			builder.Append ("message headers=").Append (message.Headers.Count).Append ('\n');
			DescribeStructure (builder, message.Body, 0);

			return builder.ToString ();
		}

		// Note: A line made up of nothing but carriage returns is an invalid header, not the blank line that
		// ends the header block. These are fuzzer-found inputs whose structure used to change on each round
		// trip because the newline filter stripped one of those CRs at a time.
		static readonly string[] CarriageReturnOnlyHeaderLineMessages = {
			"Content-Type: message/rfc822\n\r\r\nContent-Type: multipart/mixed; boundary=x\n\n\n--x\n\r",
			"Content-Type: multipart/mixed; boundary=x\n\n--x\nContent-Type: text/plain\n\r\r\r\nX-Header: value\n\nbody\n--x--\n",
		};

		[Test]
		public void TestCarriageReturnOnlyHeaderLineRoundTrip ([ValueSource (nameof (CarriageReturnOnlyHeaderLineMessages))] string text, [Values (false, true)] bool chunked, [Values (NewLineFormat.Unix, NewLineFormat.Dos)] NewLineFormat format)
		{
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = format;

			MimeMessage message;

			using (var stream = CreateStream (Encoding.ASCII.GetBytes (text), chunked))
				message = MimeMessage.Load (stream);

			var expected = DescribeStructure (message);
			byte[] previous = null;

			for (int round = 0; round < 3; round++) {
				byte[] serialized;

				using (var output = new MemoryStream ()) {
					message.WriteTo (options, output);
					serialized = output.ToArray ();
				}

				if (previous != null)
					Assert.That (serialized, Is.EqualTo (previous), $"Output changed on round {round + 1}");

				using (var stream = CreateStream (serialized, chunked))
					message = MimeMessage.Load (stream);

				Assert.That (DescribeStructure (message), Is.EqualTo (expected), $"Structure changed on round {round + 1}");
				previous = serialized;
			}
		}

		[Test]
		public async Task TestCarriageReturnOnlyHeaderLineRoundTripAsync ([ValueSource (nameof (CarriageReturnOnlyHeaderLineMessages))] string text, [Values (false, true)] bool chunked, [Values (NewLineFormat.Unix, NewLineFormat.Dos)] NewLineFormat format)
		{
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = format;

			MimeMessage message;

			using (var stream = CreateStream (Encoding.ASCII.GetBytes (text), chunked))
				message = await MimeMessage.LoadAsync (stream);

			var expected = DescribeStructure (message);
			byte[] previous = null;

			for (int round = 0; round < 3; round++) {
				byte[] serialized;

				using (var output = new MemoryStream ()) {
					await message.WriteToAsync (options, output);
					serialized = output.ToArray ();
				}

				if (previous != null)
					Assert.That (serialized, Is.EqualTo (previous), $"Output changed on round {round + 1}");

				using (var stream = CreateStream (serialized, chunked))
					message = await MimeMessage.LoadAsync (stream);

				Assert.That (DescribeStructure (message), Is.EqualTo (expected), $"Structure changed on round {round + 1}");
				previous = serialized;
			}
		}

		const string MimeDepthLimitMessage = "From: sender@example.org\r\n" +
			"Date: Tue, 12 Nov 2013 09:12:42 -0500\r\n" +
			"Message-Id: <1@example.org>\r\n" +
			"MIME-Version: 1.0\r\n" +
			"Content-Type: multipart/mixed; boundary=\"a\"\r\n" +
			"\r\n" +
			"--a\r\n" +
			"Content-Type: multipart/mixed; boundary=\"b\"\r\n" +
			"\r\n" +
			"--b\r\n" +
			"Content-Type: multipart/mixed; boundary=\"c\"\r\n" +
			"\r\n" +
			"--c\r\n" +
			"Content-Type: text/plain\r\n" +
			"\r\n" +
			"hidden\r\n" +
			"--c--\r\n" +
			"--b\r\n" +
			"Content-Type: message/rfc822\r\n" +
			"\r\n" +
			"From: inner@example.org\r\n" +
			"Date: Tue, 12 Nov 2013 09:12:42 -0500\r\n" +
			"Message-Id: <2@example.org>\r\n" +
			"Subject: also beyond the limit\r\n" +
			"\r\n" +
			"hidden\r\n" +
			"--b--\r\n" +
			"--a--\r\n";

		const string MimeDepthLimitDigestMessage = "From: sender@example.org\r\n" +
			"Date: Tue, 12 Nov 2013 09:12:42 -0500\r\n" +
			"Message-Id: <1@example.org>\r\n" +
			"MIME-Version: 1.0\r\n" +
			"Content-Type: multipart/mixed; boundary=\"a\"\r\n" +
			"\r\n" +
			"--a\r\n" +
			"Content-Type: multipart/digest; boundary=\"b\"\r\n" +
			"\r\n" +
			"--b\r\n" +
			"X-Implied-Type: message/rfc822\r\n" +
			"\r\n" +
			"From: inner@example.org\r\n" +
			"Date: Tue, 12 Nov 2013 09:12:42 -0500\r\n" +
			"Message-Id: <2@example.org>\r\n" +
			"Subject: beyond the limit\r\n" +
			"\r\n" +
			"hidden\r\n" +
			"--b--\r\n" +
			"--a--\r\n";

		static void AssertMimeDepthLimitExceeded (TestMimeComplianceLogger logger, string text, string marker)
		{
			int offset = text.IndexOf (marker, StringComparison.Ordinal);
			int lineNumber = text.Substring (0, offset).Split ('\n').Length;

			Assert.That (logger.Issues, Has.Count.EqualTo (1), string.Join (", ", logger.Issues));
			Assert.That (logger.Issues[0].Violation, Is.EqualTo (MimeComplianceViolation.MimeDepthLimitExceeded));
			Assert.That (logger.Issues[0].StreamOffset, Is.EqualTo (offset), "StreamOffset");
			Assert.That (logger.Issues[0].LineNumber, Is.EqualTo (lineNumber), "LineNumber");
			Assert.That (logger.Issues[0].ColumnNumber, Is.EqualTo (1), "ColumnNumber");
			Assert.That (logger.Issues[0].PositionKind, Is.EqualTo (MimeCompliancePositionKind.ElementStart), "PositionKind");
		}

		static MimeParser CreateMimeDepthLimitParser (string text, int maxMimeDepth, bool chunked, TestMimeComplianceLogger logger)
		{
			var options = ParserOptions.Default.Clone ();
			options.MaxMimeDepth = maxMimeDepth;

			return new MimeParser (options, CreateStream (Encoding.ASCII.GetBytes (text), chunked), MimeFormat.Entity) { ComplianceLogger = logger };
		}

		[Test]
		public void TestMimeComplianceMimeDepthLimitExceeded ([Values (false, true)] bool chunked)
		{
			var logger = new TestMimeComplianceLogger ();

			// At the limit, nothing is flattened and nothing is reported.
			using (var message = CreateMimeDepthLimitParser (MimeDepthLimitMessage, 3, chunked, logger).ParseMessage ())
				Assert.That (logger.Issues, Is.Empty, string.Join (", ", logger.Issues));

			// One below the limit, both the multipart and the message/rfc822 at depth 2 are flattened, but it is only reported once.
			using (var message = CreateMimeDepthLimitParser (MimeDepthLimitMessage, 2, chunked, logger).ParseMessage ()) {
				AssertMimeDepthLimitExceeded (logger, MimeDepthLimitMessage, "Content-Type: multipart/mixed; boundary=\"c\"");
				Assert.That (message.TryGetEntity ("1.1", out var flattened), Is.True);
				Assert.That (flattened, Is.TypeOf<MimePart> ());
			}
		}

		[Test]
		public async Task TestMimeComplianceMimeDepthLimitExceededAsync ([Values (false, true)] bool chunked)
		{
			var logger = new TestMimeComplianceLogger ();

			using (var message = await CreateMimeDepthLimitParser (MimeDepthLimitMessage, 3, chunked, logger).ParseMessageAsync ())
				Assert.That (logger.Issues, Is.Empty, string.Join (", ", logger.Issues));

			using (var message = await CreateMimeDepthLimitParser (MimeDepthLimitMessage, 2, chunked, logger).ParseMessageAsync ()) {
				AssertMimeDepthLimitExceeded (logger, MimeDepthLimitMessage, "Content-Type: multipart/mixed; boundary=\"c\"");
				Assert.That (message.TryGetEntity ("1.1", out var flattened), Is.True);
				Assert.That (flattened, Is.TypeOf<MimePart> ());
			}
		}

		[Test]
		public void TestMimeComplianceMimeDepthLimitExceededImpliedContentType ([Values (false, true)] bool chunked)
		{
			var logger = new TestMimeComplianceLogger ();

			// The message/rfc822 type is implied by the multipart/digest, so the start of its header block is reported.
			using (var message = CreateMimeDepthLimitParser (MimeDepthLimitDigestMessage, 2, chunked, logger).ParseMessage ())
				AssertMimeDepthLimitExceeded (logger, MimeDepthLimitDigestMessage, "X-Implied-Type");
		}

		[Test]
		public async Task TestMimeComplianceMimeDepthLimitExceededImpliedContentTypeAsync ([Values (false, true)] bool chunked)
		{
			var logger = new TestMimeComplianceLogger ();

			using (var message = await CreateMimeDepthLimitParser (MimeDepthLimitDigestMessage, 2, chunked, logger).ParseMessageAsync ())
				AssertMimeDepthLimitExceeded (logger, MimeDepthLimitDigestMessage, "X-Implied-Type");
		}

		static readonly byte[] SplitCrLfMessage = Encoding.ASCII.GetBytes ("From: sender@example.org\r\nDate: Tue, 12 Nov 2013 09:12:42 -0500\r\nMessage-Id: <1@example.org>\r\nMIME-Version: 1.0\r\nContent-Type: text/plain\r\n\r\n" +
			string.Concat (Enumerable.Range (0, 50).Select (i => $"line number {i} of the body with some text\r\n")));

		static void AssertSplitCrLf (MimeMessage message, TestMimeComplianceLogger logger)
		{
			Assert.That (logger.Issues, Is.Empty, string.Join (", ", logger.Issues));
			Assert.That (((MimePart) message.Body).Content.NewLineFormat, Is.EqualTo (NewLineFormat.Dos));
		}

		[Test]
		public void TestCrLfSplitAcrossReadsIsNotBareLinefeed ([Values (1, 2, 3, 5, 7, 11, 13)] int chunkSize)
		{
			var logger = new TestMimeComplianceLogger ();
			var parser = new MimeParser (new ChunkedReadStream (SplitCrLfMessage, chunkSize), MimeFormat.Entity) { ComplianceLogger = logger };

			using (var message = parser.ParseMessage ())
				AssertSplitCrLf (message, logger);
		}

		[Test]
		public async Task TestCrLfSplitAcrossReadsIsNotBareLinefeedAsync ([Values (1, 2, 3, 5, 7, 11, 13)] int chunkSize)
		{
			var logger = new TestMimeComplianceLogger ();
			var parser = new MimeParser (new ChunkedReadStream (SplitCrLfMessage, chunkSize), MimeFormat.Entity) { ComplianceLogger = logger };

			using (var message = await parser.ParseMessageAsync ())
				AssertSplitCrLf (message, logger);
		}

		static readonly byte[] ShortReadMbox = Encoding.ASCII.GetBytes (
			"From sender@example.com Sat Jan  1 00:00:00 2022\n" +
			"Subject: First\n\nbody one\n\n" +
			"From sender@example.com Sat Jan  1 00:00:01 2022\n" +
			"Subject: Second\n\nbody two\n\n" +
			"From sender@example.com Sat Jan  1 00:00:02 2022\r\n" +
			"Subject: Third\r\n\r\nbody three\r\n");

		static readonly byte[] ShortReadCrLfMessage = Encoding.ASCII.GetBytes (
			"From: sender@example.org\r\nSubject: Short reads\r\nMIME-Version: 1.0\r\n" +
			"Content-Type: multipart/mixed; boundary=\"b\"\r\n\r\n" +
			"--b\r\nContent-Type: text/plain\r\n\r\nfirst part\r\n" +
			"--b\r\n\r\nsecond part\r\n" +
			"--b--\r\n");

		static string[] GetShortReadSummaries (List<MimeMessage> messages)
		{
			var summaries = new string[messages.Count];

			for (int i = 0; i < messages.Count; i++) {
				var parts = messages[i].BodyParts.OfType<TextPart> ().Select (part => part.Text);
				summaries[i] = messages[i].Subject + "|" + string.Join ("|", parts) + "|" + messages[i].ToString ();
			}

			return summaries;
		}

		static IMimeParser CreateParser (Stream stream, MimeFormat format, bool legacy)
		{
			return legacy ? new LegacyMimeParser (stream, format) : new MimeParser (stream, format);
		}

		static List<MimeMessage> ParseAll (Stream stream, MimeFormat format, bool legacy)
		{
			var parser = CreateParser (stream, format, legacy);
			var messages = new List<MimeMessage> ();

			while (!parser.IsEndOfStream)
				messages.Add (parser.ParseMessage ());

			return messages;
		}

		static async Task<List<MimeMessage>> ParseAllAsync (Stream stream, MimeFormat format, bool legacy)
		{
			var parser = CreateParser (stream, format, legacy);
			var messages = new List<MimeMessage> ();

			while (!parser.IsEndOfStream)
				messages.Add (await parser.ParseMessageAsync ());

			return messages;
		}

		static void AssertShortReads (List<MimeMessage> expected, List<MimeMessage> actual)
		{
			try {
				Assert.That (GetShortReadSummaries (actual), Is.EqualTo (GetShortReadSummaries (expected)));
			} finally {
				foreach (var message in expected.Concat (actual))
					message.Dispose ();
			}
		}

		[Test]
		public void TestShortReads ([Values (false, true)] bool legacy, [Values (MimeFormat.Mbox, MimeFormat.Entity)] MimeFormat format, [Values (1, 2, 3, 4, 5)] int chunkSize)
		{
			var data = format == MimeFormat.Mbox ? ShortReadMbox : ShortReadCrLfMessage;
			var expected = ParseAll (new MemoryStream (data, false), format, legacy);

			Assert.That (expected.Count, Is.EqualTo (format == MimeFormat.Mbox ? 3 : 1));

			AssertShortReads (expected, ParseAll (new ChunkedReadStream (data, chunkSize), format, legacy));
		}

		[Test]
		public async Task TestShortReadsAsync ([Values (false, true)] bool legacy, [Values (MimeFormat.Mbox, MimeFormat.Entity)] MimeFormat format, [Values (1, 2, 3, 4, 5)] int chunkSize)
		{
			var data = format == MimeFormat.Mbox ? ShortReadMbox : ShortReadCrLfMessage;
			var expected = await ParseAllAsync (new MemoryStream (data, false), format, legacy);

			Assert.That (expected.Count, Is.EqualTo (format == MimeFormat.Mbox ? 3 : 1));

			AssertShortReads (expected, await ParseAllAsync (new ChunkedReadStream (data, chunkSize), format, legacy));
		}

		const string MessagePartMboxMarker = "From sender Sat Jan  1 00:00:00 2000";

		static void AssertMessagePartTextBody (MimeMessage message, string expected)
		{
			Assert.That (message.Subject, Is.EqualTo ("inner"));
			Assert.That (message.Body, Is.InstanceOf<TextPart> ());
			using var content = new MemoryStream ();
			((TextPart) message.Body).Content.DecodeTo (content);
			Assert.That (content.ToArray (), Is.EqualTo (Encoding.ASCII.GetBytes (expected)));
		}

		static async Task AssertMessagePartShortReads (string text, MimeFormat format, bool seekable, bool persistent, bool doAsync, int count, Action<MimeMessage, CustomMimeParser, int> assertMessage)
		{
			var data = Encoding.ASCII.GetBytes (text);
			var serialized = new byte[count][];
			List<MimeOffsets> offsets = null;

			foreach (int chunkSize in new[] { 4096, 7, 1 }) {
				using var memory = new ChunkedReadStream (data, chunkSize);
				using var stream = seekable ? (Stream) memory : new NonSeekableStream (memory);
				if (persistent && !seekable) {
					Assert.Throws<ArgumentException> (() => new CustomMimeParser (new ParserOptions { MaxMimeDepth = 16 }, stream, format, persistent));
					continue;
				}
				var parser = new CustomMimeParser (new ParserOptions { MaxMimeDepth = 16 }, stream, format, persistent);

				for (int i = 0; i < count; i++) {
					Assert.That (parser.IsEndOfStream, Is.False, $"message {i}, chunk size {chunkSize}");
					using var message = doAsync ? await parser.ParseMessageAsync () : parser.ParseMessage ();
					assertMessage (message, parser, i);

					using var output = new MemoryStream ();
					if (doAsync)
						await message.WriteToAsync (UnixFormatOptions, output);
					else
						message.WriteTo (UnixFormatOptions, output);

					if (chunkSize == 4096)
						serialized[i] = output.ToArray ();
					else
						Assert.That (output.ToArray (), Is.EqualTo (serialized[i]), $"serialized message {i}, chunk size {chunkSize}");
				}

				Assert.That (parser.IsEndOfStream, Is.True);
				Assert.That (parser.Position, Is.EqualTo (data.LongLength));
				Assert.That (parser.Offsets.Count, Is.EqualTo (count));
				if (offsets == null) {
					offsets = parser.Offsets;
				} else {
					for (int i = 0; i < count; i++)
						AssertMimeOffsets (offsets[i], parser.Offsets[i], i, "");
				}
			}
		}

		static Task AssertMessagePartImmediateMboxBoundary (bool dos, bool seekable, bool persistent, string marker, bool doAsync)
		{
			string newline = dos ? "\r\n" : "\n";
			string prefix = MessagePartMboxMarker + newline + "Content-Type: message/rfc822" + newline + newline;
			string text = prefix + marker + newline + "Subject: inner" + newline + newline + "body" + newline;

			return AssertMessagePartShortReads (text, MimeFormat.Mbox, seekable, persistent, doAsync, 2, (message, parser, index) => {
				Assert.That (parser.MboxMarkerOffset, Is.EqualTo (index == 0 ? 0 : prefix.Length));
				Assert.That (Encoding.ASCII.GetBytes (parser.MboxMarker), Is.EqualTo (Encoding.ASCII.GetBytes (index == 0 ? MessagePartMboxMarker : marker)));

				if (index == 0) {
					Assert.That (parser.Position, Is.EqualTo (prefix.Length));
					Assert.That (message.Body, Is.InstanceOf<MessagePart> ());
					Assert.That (((MessagePart) message.Body).Message, Is.Null);
				} else {
					AssertMessagePartTextBody (message, "body" + newline);
				}
			});
		}

		[Test]
		public void TestMessagePartImmediateMboxBoundary ([Values (false, true)] bool dos, [Values (false, true)] bool seekable, [Values (false, true)] bool persistent,
			[Values ("From invalid\0preheader", "From sender Sat Jan  1 00:00:01 2000")] string marker)
		{
			AssertMessagePartImmediateMboxBoundary (dos, seekable, persistent, marker, false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestMessagePartImmediateMboxBoundaryAsync ([Values (false, true)] bool dos, [Values (false, true)] bool seekable, [Values (false, true)] bool persistent,
			[Values ("From invalid\0preheader", "From sender Sat Jan  1 00:00:01 2000")] string marker)
		{
			await AssertMessagePartImmediateMboxBoundary (dos, seekable, persistent, marker, true);
		}

		static Task AssertMessagePartImmediateMultipartBoundary (bool dos, bool seekable, bool persistent, bool closing, int boundaryLength, bool doAsync)
		{
			string newline = dos ? "\r\n" : "\n";
			string boundary = new string ('b', boundaryLength);
			string text = "Content-Type: multipart/mixed; boundary=\"" + boundary + "\"" + newline + newline +
				"--" + boundary + newline + "Content-Type: message/rfc822" + newline + newline;
			if (!closing)
				text += "--" + boundary + newline + "Content-Type: text/plain" + newline + newline + "sibling" + newline;
			text += "--" + boundary + "--" + newline;

			return AssertMessagePartShortReads (text, MimeFormat.Entity, seekable, persistent, doAsync, 1, (message, parser, index) => {
				Assert.That (message.Body, Is.InstanceOf<Multipart> ());
				var multipart = (Multipart) message.Body;
				Assert.That (multipart.Count, Is.EqualTo (closing ? 1 : 2));
				Assert.That (multipart[0], Is.InstanceOf<MessagePart> ());
				Assert.That (((MessagePart) multipart[0]).Message, Is.Null);
				if (!closing) {
					Assert.That (multipart[1], Is.InstanceOf<TextPart> ());
					Assert.That (((TextPart) multipart[1]).Text, Is.EqualTo ("sibling"));
				}
			});
		}

		[Test]
		public void TestMessagePartImmediateMultipartBoundary ([Values (false, true)] bool dos, [Values (false, true)] bool seekable, [Values (false, true)] bool persistent,
			[Values (false, true)] bool closing, [Values (1, 140)] int boundaryLength)
		{
			AssertMessagePartImmediateMultipartBoundary (dos, seekable, persistent, closing, boundaryLength, false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestMessagePartImmediateMultipartBoundaryAsync ([Values (false, true)] bool dos, [Values (false, true)] bool seekable, [Values (false, true)] bool persistent,
			[Values (false, true)] bool closing, [Values (1, 140)] int boundaryLength)
		{
			await AssertMessagePartImmediateMultipartBoundary (dos, seekable, persistent, closing, boundaryLength, true);
		}

		static Task AssertMessagePartAtEndOfStream (bool dos, bool seekable, bool persistent, MimeFormat format, bool doAsync)
		{
			string newline = dos ? "\r\n" : "\n";
			string text = "Content-Type: message/rfc822" + newline + newline;
			if (format == MimeFormat.Mbox)
				text = MessagePartMboxMarker + newline + text;

			return AssertMessagePartShortReads (text, format, seekable, persistent, doAsync, 1, (message, parser, index) => {
				Assert.That (message.Body, Is.InstanceOf<MessagePart> ());
				var part = (MessagePart) message.Body;
				if (format == MimeFormat.Mbox) {
					Assert.That (part.Message, Is.Null);
				} else {
					Assert.That (part.Message, Is.Not.Null);
					Assert.That (part.Message.Headers, Is.Empty);
					Assert.That (part.Message.Body, Is.InstanceOf<TextPart> ());
					Assert.That (((TextPart) part.Message.Body).Text, Is.Empty);
				}
			});
		}

		[Test]
		public void TestMessagePartAtEndOfStream ([Values (false, true)] bool dos, [Values (false, true)] bool seekable, [Values (false, true)] bool persistent,
			[Values (MimeFormat.Entity, MimeFormat.Mbox)] MimeFormat format)
		{
			AssertMessagePartAtEndOfStream (dos, seekable, persistent, format, false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestMessagePartAtEndOfStreamAsync ([Values (false, true)] bool dos, [Values (false, true)] bool seekable, [Values (false, true)] bool persistent,
			[Values (MimeFormat.Entity, MimeFormat.Mbox)] MimeFormat format)
		{
			await AssertMessagePartAtEndOfStream (dos, seekable, persistent, format, true);
		}

		static Task AssertMessagePartEmbeddedMessageShortReads (bool dos, bool seekable, bool persistent, MimeFormat format, bool doAsync)
		{
			string newline = dos ? "\r\n" : "\n";
			string text = "Content-Type: message/rfc822" + newline + newline +
				"Subject: inner" + newline + newline + "body" + newline;
			if (format == MimeFormat.Mbox)
				text = MessagePartMboxMarker + newline + text;

			return AssertMessagePartShortReads (text, format, seekable, persistent, doAsync, 1, (message, parser, index) => {
				Assert.That (message.Body, Is.InstanceOf<MessagePart> ());
				var part = (MessagePart) message.Body;
				Assert.That (part.Message, Is.Not.Null);
				AssertMessagePartTextBody (part.Message, "body" + newline);
			});
		}

		[Test]
		public void TestMessagePartEmbeddedMessageShortReads ([Values (false, true)] bool dos, [Values (false, true)] bool seekable, [Values (false, true)] bool persistent,
			[Values (MimeFormat.Entity, MimeFormat.Mbox)] MimeFormat format)
		{
			AssertMessagePartEmbeddedMessageShortReads (dos, seekable, persistent, format, false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestMessagePartEmbeddedMessageShortReadsAsync ([Values (false, true)] bool dos, [Values (false, true)] bool seekable, [Values (false, true)] bool persistent,
			[Values (MimeFormat.Entity, MimeFormat.Mbox)] MimeFormat format)
		{
			await AssertMessagePartEmbeddedMessageShortReads (dos, seekable, persistent, format, true);
		}

		[Test]
		public async Task TestMessagePartBoundaryLookaheadIsBounded ([Values (false, true)] bool doAsync)
		{
			string prefix = MessagePartMboxMarker + "\nContent-Type: message/rfc822\n\n";
			var data = Encoding.ASCII.GetBytes (prefix + "From " + new string ('x', 10000) + "\nSubject: inner\n\nbody\n");
			using var stream = new ThrowingReadStream (data, prefix.Length + 128, 1);
			var parser = new MimeParser (stream, MimeFormat.Mbox);
			using var message = doAsync ? await parser.ParseMessageAsync () : parser.ParseMessage ();

			Assert.That (message.Body, Is.InstanceOf<MessagePart> ());
			Assert.That (((MessagePart) message.Body).Message, Is.Null);
			Assert.That (parser.Position, Is.EqualTo (prefix.Length));
			Assert.That (stream.Position, Is.LessThanOrEqualTo (prefix.Length + 128));
		}

		static Stream CreateStream (byte[] data, bool chunked)
		{
			return chunked ? new ChunkedReadStream (data, 7) : new MemoryStream (data, false);
		}

		static void AssertExcessiveHeaderLength (FormatException ex, long offset, int maxHeaderLength)
		{
			Assert.That (ex.Message, Is.EqualTo ($"Header at offset {offset} exceeds the maximum header length of {maxHeaderLength} bytes."));
		}

		[Test]
		public void TestMaxHeaderLengthBoundary ([Values (false, true)] bool chunked)
		{
			var data = CreateMessageWithHugeFoldedHeader (100, out var expectedRawValue, out var subjectOffset);
			int length = "To:".Length + expectedRawValue.Length;

			// A header that is exactly MaxHeaderLength bytes long is accepted.
			var options = new ParserOptions { MaxHeaderLength = length };

			using (var stream = CreateStream (data, chunked)) {
				var parser = new MimeParser (options, stream, MimeFormat.Entity);
				var message = parser.ParseMessage ();

				AssertHugeFoldedHeader (message, expectedRawValue, subjectOffset, 100);
			}

			// A header that is 1 byte longer than MaxHeaderLength is rejected.
			options.MaxHeaderLength = length - 1;

			using (var stream = CreateStream (data, chunked)) {
				var parser = new MimeParser (options, stream, MimeFormat.Entity);
				var ex = Assert.Throws<FormatException> (() => parser.ParseMessage ());

				AssertExcessiveHeaderLength (ex, 26, length - 1);
			}
		}

		[Test]
		public async Task TestMaxHeaderLengthBoundaryAsync ([Values (false, true)] bool chunked)
		{
			var data = CreateMessageWithHugeFoldedHeader (100, out var expectedRawValue, out var subjectOffset);
			int length = "To:".Length + expectedRawValue.Length;

			// A header that is exactly MaxHeaderLength bytes long is accepted.
			var options = new ParserOptions { MaxHeaderLength = length };

			using (var stream = CreateStream (data, chunked)) {
				var parser = new MimeParser (options, stream, MimeFormat.Entity);
				var message = await parser.ParseMessageAsync ();

				AssertHugeFoldedHeader (message, expectedRawValue, subjectOffset, 100);
			}

			// A header that is 1 byte longer than MaxHeaderLength is rejected.
			options.MaxHeaderLength = length - 1;

			using (var stream = CreateStream (data, chunked)) {
				var parser = new MimeParser (options, stream, MimeFormat.Entity);
				var ex = Assert.ThrowsAsync<FormatException> (() => parser.ParseMessageAsync ());

				AssertExcessiveHeaderLength (ex, 26, length - 1);
			}
		}

		[Test]
		public void TestMaxHeaderLengthExceeded ([Values (false, true)] bool chunked)
		{
			var data = CreateMessageWithHugeFoldedHeader (1000, out _, out _);
			var options = new ParserOptions ();

			using (var stream = CreateStream (data, chunked)) {
				var parser = new MimeParser (options, stream, MimeFormat.Entity);
				var ex = Assert.Throws<FormatException> (() => parser.ParseMessage ());

				AssertExcessiveHeaderLength (ex, 26, 16 * 1024);

				// The parser should have stopped reading shortly after exceeding the limit.
				Assert.That (stream.Position, Is.LessThan (data.Length));
			}
		}

		[Test]
		public async Task TestMaxHeaderLengthExceededAsync ([Values (false, true)] bool chunked)
		{
			var data = CreateMessageWithHugeFoldedHeader (1000, out _, out _);
			var options = new ParserOptions ();

			using (var stream = CreateStream (data, chunked)) {
				var parser = new MimeParser (options, stream, MimeFormat.Entity);
				var ex = Assert.ThrowsAsync<FormatException> (() => parser.ParseMessageAsync ());

				AssertExcessiveHeaderLength (ex, 26, 16 * 1024);

				// The parser should have stopped reading shortly after exceeding the limit.
				Assert.That (stream.Position, Is.LessThan (data.Length));
			}
		}

		const string ShortHeadersMessage = "From: sender@example.com\r\nSubject: hi\r\n\r\nThis is the body.\r\n";

		[Test]
		public void TestMaxHeaderLengthShorterThanFieldName ([Values (false, true)] bool chunked)
		{
			var data = Encoding.ASCII.GetBytes (ShortHeadersMessage);
			var options = new ParserOptions { MaxHeaderLength = 4 };

			using (var stream = CreateStream (data, chunked)) {
				var parser = new MimeParser (options, stream, MimeFormat.Entity);
				var ex = Assert.Throws<FormatException> (() => parser.ParseMessage ());

				AssertExcessiveHeaderLength (ex, 0, 4);
			}
		}

		[Test]
		public async Task TestMaxHeaderLengthShorterThanFieldNameAsync ([Values (false, true)] bool chunked)
		{
			var data = Encoding.ASCII.GetBytes (ShortHeadersMessage);
			var options = new ParserOptions { MaxHeaderLength = 4 };

			using (var stream = CreateStream (data, chunked)) {
				var parser = new MimeParser (options, stream, MimeFormat.Entity);
				var ex = Assert.ThrowsAsync<FormatException> (() => parser.ParseMessageAsync ());

				AssertExcessiveHeaderLength (ex, 0, 4);
			}
		}

		const string CallbackForwardingMbox = "From sender@example.com Mon Jan  1 00:00:00 2024\n" +
			"From: sender@example.com\n" +
			"To: recipient@example.com\n" +
			"Subject: multipart\n" +
			"MIME-Version: 1.0\n" +
			"Content-Type: multipart/mixed; boundary=\"outer\"\n" +
			"\n" +
			"This is the preamble.\n" +
			"--outer\n" +
			"Content-Type: text/plain\n" +
			"\n" +
			"Hello.\n" +
			"--outer\n" +
			"Content-Type: message/rfc822\n" +
			"\n" +
			"From: inner@example.com\n" +
			"Subject: inner\n" +
			"\n" +
			"Inner body.\n" +
			"--outer--\n" +
			"This is the epilogue.\n" +
			"\n" +
			"From sender@example.com Mon Jan  1 00:00:01 2024\n" +
			"From: sender@example.com\n" +
			"Subject: second\n" +
			"\n" +
			"Second body.\n";

		static List<string> ReadCallbackEvents (bool async)
		{
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (CallbackForwardingMbox), false)) {
				var reader = new RecordingMimeReader (stream, MimeFormat.Mbox);

				while (!reader.IsEndOfStream) {
					if (async)
						reader.ReadMessageAsync ().GetAwaiter ().GetResult ();
					else
						reader.ReadMessage ();
				}

				return reader.Events;
			}
		}

		[Test]
		public void TestMimeParserIsNotAMimeReader ()
		{
			// Note: MimeParser used to derive from MimeReader, which exposed ReadMessage(), ReadEntity() and
			// ReadHeaders(). Calling those directly would corrupt the parser's internal state.
			Assert.That (typeof (MimeReader).IsAssignableFrom (typeof (MimeParser)), Is.False);
		}

		[Test]
		public void TestMimeParserForwardsAllCallbacks ()
		{
			var expected = ReadCallbackEvents (false);

			Assert.That (ReadCallbackEvents (true), Is.EqualTo (expected), "MimeReader sync vs async");

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (CallbackForwardingMbox), false)) {
				var parser = new RecordingMimeParser (stream, MimeFormat.Mbox);
				var subjects = new List<string> ();

				while (!parser.IsEndOfStream)
					subjects.Add (parser.ParseMessage ().Subject);

				Assert.That (subjects, Is.EqualTo (new[] { "multipart", "second" }));
				Assert.That (parser.Events, Is.EqualTo (expected));
			}
		}

		[Test]
		public async Task TestMimeParserForwardsAllCallbacksAsync ()
		{
			var expected = ReadCallbackEvents (false);

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (CallbackForwardingMbox), false)) {
				var parser = new RecordingMimeParser (stream, MimeFormat.Mbox);
				var subjects = new List<string> ();

				// Note: RecordingMimeParser only overrides the synchronous callbacks, so this also verifies
				// that the default asynchronous callbacks delegate to their synchronous counterparts.
				while (!parser.IsEndOfStream)
					subjects.Add ((await parser.ParseMessageAsync ()).Subject);

				Assert.That (subjects, Is.EqualTo (new[] { "multipart", "second" }));
				Assert.That (parser.Events, Is.EqualTo (expected));
			}
		}

		// Note: These messages mix multiparts and message/rfc822 parts so that the parser has to switch between
		// the different kinds of entities, resume a parent after a nested child (and parse its following siblings),
		// and unwind several levels at once when an inner multipart is terminated by an ancestor's boundary.
		static readonly string[][] NestedTopologyMessages = {
			new[] { "siblings",
				"From: a@example.org\r\nSubject: siblings\r\nMIME-Version: 1.0\r\nContent-Type: multipart/mixed; boundary=\"outer\"\r\n\r\n" +
				"preamble\r\n" +
				"--outer\r\nContent-Type: text/plain\r\n\r\nfirst\r\n" +
				"--outer\r\nContent-Type: message/rfc822\r\n\r\n" +
					"From: b@example.org\r\nSubject: embedded\r\nContent-Type: multipart/alternative; boundary=\"inner\"\r\n\r\n" +
					"--inner\r\nContent-Type: text/plain\r\n\r\nplain\r\n" +
					"--inner\r\nContent-Type: text/html\r\n\r\n<p>html</p>\r\n" +
					"--inner--\r\ninner epilogue\r\n" +
				"--outer\r\nContent-Type: text/plain\r\n\r\nafter the embedded message\r\n" +
				"--outer--\r\nouter epilogue\r\n"
			},
			new[] { "ancestor-boundary",
				"From: a@example.org\r\nSubject: ancestor boundaries\r\nMIME-Version: 1.0\r\nContent-Type: multipart/mixed; boundary=\"outer\"\r\n\r\n" +
				"--outer\r\nContent-Type: message/rfc822\r\n\r\n" +
					"From: b@example.org\r\nContent-Type: multipart/related; boundary=\"inner\"\r\n\r\n" +
					"--inner\r\nContent-Type: text/plain\r\n\r\nunterminated inner\r\n" +
				"--outer\r\nContent-Type: multipart/alternative; boundary=\"mid\"\r\n\r\n" +
					"--mid\r\nContent-Type: message/rfc822\r\n\r\n" +
						"From: c@example.org\r\nContent-Type: multipart/mixed; boundary=\"deep\"\r\n\r\n" +
						"--deep\r\nContent-Type: text/plain\r\n\r\ndeep part\r\n" +
				"--outer--\r\nepilogue\r\n"
			},
			new[] { "edge-cases",
				"From: a@example.org\r\nSubject: edge cases\r\nMIME-Version: 1.0\r\nContent-Type: multipart/mixed; boundary=\"outer\"\r\n\r\n" +
				"--outer\r\nContent-Type: multipart/digest; boundary=\"digest\"\r\n\r\n" +
					"--digest\r\n\r\nFrom: d1@example.org\r\nSubject: digest 1\r\n\r\ndigest body 1\r\n" +
					"--digest\r\n\r\nFrom: d2@example.org\r\nContent-Type: multipart/mixed; boundary=\"d2\"\r\n\r\n" +
						"--d2\r\nContent-Type: text/plain\r\n\r\nd2 part\r\n--d2--\r\n" +
					"--digest--\r\n" +
				"--outer\r\nContent-Type: message/rfc822\r\n\r\n" +
				"--outer\r\nContent-Type: multipart/mixed\r\n\r\nno boundary parameter\r\n" +
				"--outer\r\nContent-Type: message/rfc822\r\n\r\n" +
					"From: e@example.org\r\nContent-Type: message/rfc822\r\n\r\n" +
						"From: f@example.org\r\nContent-Type: multipart/mixed; boundary=\"f\"\r\n\r\n" +
						"--f\r\nContent-Type: text/plain\r\n\r\nf part\r\n--f--\r\n" +
				"--outer--\r\n"
			},
		};

		static IEnumerable<string> NestedTopologyNames => NestedTopologyMessages.Select (m => m[0]);

		static string GetNestedTopologyMessage (string name) => NestedTopologyMessages.First (m => m[0] == name)[1];

		static string FormatEvents (List<string> events)
		{
			var builder = new StringBuilder ();

			foreach (var e in events)
				builder.Append (e.Replace ("\r", "\\r").Replace ("\n", "\\n")).Append ('\n');

			return builder.ToString ();
		}

		// Note: The expected events were generated by the recursive implementation of MimeReader that preceded
		// the current (non-recursive) one, so they verify that the sequence of callbacks did not change.
		static string GetExpectedNestedTopologyEvents (string name, string actual)
		{
			var path = Path.Combine (MessagesDataDir, $"nested-topology-{name}.events");

			if (!File.Exists (path))
				File.WriteAllText (path, actual);

			return File.ReadAllText (path).Replace ("\r\n", "\n");
		}

		static void AssertNestedTopologyTree (string text)
		{
			string expected, actual;

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				using var message = new LegacyMimeParser (stream, MimeFormat.Entity).ParseMessage ();
				expected = GetEntityTreeSummary (message);
			}

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				using var message = new MimeParser (stream, MimeFormat.Entity).ParseMessage ();
				actual = GetEntityTreeSummary (message);
			}

			Assert.That (actual, Is.EqualTo (expected), "MimeParser did not match LegacyMimeParser.");
		}

		[TestCaseSource (nameof (NestedTopologyNames))]
		public void TestNestedTopology (string name)
		{
			var text = GetNestedTopologyMessage (name);

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var reader = new RecordingMimeReader (stream, MimeFormat.Entity);

				reader.ReadMessage ();

				var actual = FormatEvents (reader.Events);

				Assert.That (actual, Is.EqualTo (GetExpectedNestedTopologyEvents (name, actual)));
				Assert.That (reader.IsEndOfStream, Is.True, "IsEndOfStream");
			}

			AssertNestedTopologyTree (text);
		}

		sealed class YieldingReadStream : MemoryStream
		{
			public int Suspensions;

			public YieldingReadStream (byte[] buffer) : base (buffer, false)
			{
			}

			public override async Task<int> ReadAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
			{
				Suspensions++;
				await Task.Yield ();
				return await base.ReadAsync (buffer, offset, count, cancellationToken);
			}

			public override async ValueTask<int> ReadAsync (Memory<byte> buffer, CancellationToken cancellationToken = default)
			{
				Suspensions++;
				await Task.Yield ();
				return await base.ReadAsync (buffer, cancellationToken);
			}
		}

		// Note: Suspends in each of the entity callbacks so that ReadMessageAsync() genuinely has to resume
		// its parsing state after an asynchronous continuation.
		sealed class SuspendingRecordingMimeReader : RecordingMimeReader
		{
			public int Suspensions;

			public SuspendingRecordingMimeReader (Stream stream, MimeFormat format) : base (stream, format)
			{
			}

			async Task SuspendAsync ()
			{
				Suspensions++;
				await Task.Yield ();
			}

			protected override async Task OnMimeMessageBeginAsync (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				await SuspendAsync ();
				await base.OnMimeMessageBeginAsync (beginOffset, beginLineNumber, cancellationToken);
			}

			protected override async Task OnMimeMessageEndAsync (long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				await SuspendAsync ();
				await base.OnMimeMessageEndAsync (beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			}

			protected override async Task OnMimePartBeginAsync (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				await SuspendAsync ();
				await base.OnMimePartBeginAsync (contentType, beginOffset, beginLineNumber, cancellationToken);
			}

			protected override async Task OnMimePartEndAsync (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				await SuspendAsync ();
				await base.OnMimePartEndAsync (contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			}

			protected override async Task OnMessagePartBeginAsync (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				await SuspendAsync ();
				await base.OnMessagePartBeginAsync (contentType, beginOffset, beginLineNumber, cancellationToken);
			}

			protected override async Task OnMessagePartEndAsync (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				await SuspendAsync ();
				await base.OnMessagePartEndAsync (contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			}

			protected override async Task OnMultipartBeginAsync (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				await SuspendAsync ();
				await base.OnMultipartBeginAsync (contentType, beginOffset, beginLineNumber, cancellationToken);
			}

			protected override async Task OnMultipartBoundaryEndAsync (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken)
			{
				await SuspendAsync ();
				await base.OnMultipartBoundaryEndAsync (beginOffset, lineNumber, endOffset, cancellationToken);
			}

			protected override async Task OnMultipartEndBoundaryEndAsync (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken)
			{
				await SuspendAsync ();
				await base.OnMultipartEndBoundaryEndAsync (beginOffset, lineNumber, endOffset, cancellationToken);
			}

			protected override async Task OnMultipartEndAsync (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				await SuspendAsync ();
				await base.OnMultipartEndAsync (contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			}
		}

		[TestCaseSource (nameof (NestedTopologyNames))]
		public async Task TestNestedTopologyAsync (string name)
		{
			var text = GetNestedTopologyMessage (name);

			using (var stream = new YieldingReadStream (Encoding.ASCII.GetBytes (text))) {
				var reader = new SuspendingRecordingMimeReader (stream, MimeFormat.Entity);

				await reader.ReadMessageAsync ();

				var actual = FormatEvents (reader.Events);

				Assert.That (actual, Is.EqualTo (GetExpectedNestedTopologyEvents (name, actual)));
				Assert.That (reader.IsEndOfStream, Is.True, "IsEndOfStream");
				Assert.That (reader.Suspensions, Is.GreaterThan (0), "reader suspensions");
				Assert.That (stream.Suspensions, Is.GreaterThan (0), "stream suspensions");
			}
		}

		class AsyncMboxMarkerMimeParser : MimeParser
		{
			public readonly List<string> Events = new List<string> ();

			public AsyncMboxMarkerMimeParser (Stream stream) : base (stream, MimeFormat.Mbox)
			{
			}

			protected override Task OnMboxMarkerBeginAsync (long beginOffset, int lineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"BeginAsync({beginOffset},{lineNumber})");
				return base.OnMboxMarkerBeginAsync (beginOffset, lineNumber, cancellationToken);
			}

			protected override Task OnMboxMarkerEndAsync (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken)
			{
				Events.Add ($"EndAsync({beginOffset},{lineNumber},{endOffset})");
				return base.OnMboxMarkerEndAsync (beginOffset, lineNumber, endOffset, cancellationToken);
			}
		}

		[Test]
		public void TestMboxMarkerAsyncCallbacksNotUsedBySyncParse ()
		{
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (CallbackForwardingMbox), false)) {
				var parser = new AsyncMboxMarkerMimeParser (stream);

				while (!parser.IsEndOfStream)
					parser.ParseMessage ();

				Assert.That (parser.Events, Is.Empty);
			}
		}

		[Test]
		public async Task TestMboxMarkerAsyncCallbacksAsync ()
		{
			var expected = ReadCallbackEvents (false)
				.Where (e => e.StartsWith ("OnMboxMarkerBegin(", StringComparison.Ordinal) || e.StartsWith ("OnMboxMarkerEnd(", StringComparison.Ordinal))
				.Select (e => e.Replace ("OnMboxMarkerBegin(", "BeginAsync(").Replace ("OnMboxMarkerEnd(", "EndAsync("))
				.ToList ();

			Assert.That (expected, Has.Count.EqualTo (4));

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (CallbackForwardingMbox), false)) {
				var parser = new AsyncMboxMarkerMimeParser (stream);
				var markers = new List<string> ();

				while (!parser.IsEndOfStream) {
					await parser.ParseMessageAsync ();
					markers.Add (parser.MboxMarker);
				}

				Assert.That (parser.Events, Is.EqualTo (expected));
				Assert.That (markers, Is.EqualTo (new[] { "From sender@example.com Mon Jan  1 00:00:00 2024", "From sender@example.com Mon Jan  1 00:00:01 2024" }));
			}
		}

		[Test]
		public void TestMimeParserForwardsReaderProperties ()
		{
			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes ("From: sender@example.com\r\nThis is not a header\r\n\r\nBody\r\n"), false)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);
				var complianceOptions = new MimeComplianceOptions ();
				var logger = new TestMimeComplianceLogger ();
				var options = new ParserOptions ();

				parser.Options = options;
				parser.ComplianceLogger = logger;
				parser.ComplianceOptions = complianceOptions;

				Assert.That (parser.Options, Is.SameAs (options));
				Assert.That (parser.ComplianceLogger, Is.SameAs (logger));
				Assert.That (parser.ComplianceOptions, Is.SameAs (complianceOptions));
				Assert.That (parser.Position, Is.EqualTo (0));
				Assert.That (parser.IsEndOfStream, Is.False);

				parser.ParseMessage ();

				Assert.That (logger.Issues, Is.Not.Empty);
				Assert.That (parser.Position, Is.EqualTo (stream.Length));
				Assert.That (parser.IsEndOfStream, Is.True);

				Assert.Throws<ArgumentNullException> (() => parser.Options = null);
				Assert.Throws<ArgumentNullException> (() => parser.ComplianceOptions = null);
			}
		}
		class RecordingMimeReader : MimeReader
		{
			public readonly List<string> Events = new List<string> ();

			public RecordingMimeReader (Stream stream, MimeFormat format) : base (stream, format)
			{
			}

			protected override void OnMboxMarkerBegin (long beginOffset, int lineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMboxMarkerBegin({beginOffset},{lineNumber})");
			}

			protected override void OnMboxMarkerRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMboxMarkerRead({Encoding.Latin1.GetString (buffer, startIndex, count)})");
			}

			protected override void OnMboxMarkerEnd (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMboxMarkerEnd({beginOffset},{lineNumber},{endOffset})");
			}

			protected override void OnHeadersBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnHeadersBegin({beginOffset},{beginLineNumber})");
			}

			protected override void OnHeaderRead (Header header, int beginLineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnHeaderRead({header.Field},{Encoding.Latin1.GetString (header.RawValue)},{beginLineNumber})");
			}

			protected override void OnHeadersEnd (long beginOffset, int beginLineNumber, long endOffset, int endLineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnHeadersEnd({beginOffset},{beginLineNumber},{endOffset},{endLineNumber})");
			}

			protected override void OnBodySeparator (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken)
			{
				Events.Add ($"OnBodySeparator({beginOffset},{lineNumber},{endOffset})");
			}

			protected override void OnMimeMessageBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMimeMessageBegin({beginOffset},{beginLineNumber})");
			}

			protected override void OnMimeMessageEnd (long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMimeMessageEnd({beginOffset},{beginLineNumber},{headersEndOffset},{endOffset},{lines})");
			}

			protected override void OnMimePartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMimePartBegin({contentType.MimeType},{beginOffset},{beginLineNumber})");
			}

			protected override void OnMimePartContentBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMimePartContentBegin({beginOffset},{beginLineNumber})");
			}

			protected override void OnMimePartContentRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMimePartContentRead({Encoding.Latin1.GetString (buffer, startIndex, count)})");
			}

			protected override void OnMimePartContentEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, NewLineFormat? newLineFormat, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMimePartContentEnd({beginOffset},{beginLineNumber},{endOffset},{lines},{newLineFormat})");
			}

			protected override void OnMimePartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMimePartEnd({contentType.MimeType},{beginOffset},{beginLineNumber},{headersEndOffset},{endOffset},{lines})");
			}

			protected override void OnMessagePartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMessagePartBegin({contentType.MimeType},{beginOffset},{beginLineNumber})");
			}

			protected override void OnMessagePartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMessagePartEnd({contentType.MimeType},{beginOffset},{beginLineNumber},{headersEndOffset},{endOffset},{lines})");
			}

			protected override void OnMultipartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartBegin({contentType.MimeType},{beginOffset},{beginLineNumber})");
			}

			protected override void OnMultipartBoundaryBegin (long beginOffset, int lineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartBoundaryBegin({beginOffset},{lineNumber})");
			}

			protected override void OnMultipartBoundaryRead (byte[] buffer, int startIndex, int count, long beginOffset, int lineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartBoundaryRead({Encoding.Latin1.GetString (buffer, startIndex, count)},{beginOffset},{lineNumber})");
			}

			protected override void OnMultipartBoundaryEnd (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartBoundaryEnd({beginOffset},{lineNumber},{endOffset})");
			}

			protected override void OnMultipartEndBoundaryBegin (long beginOffset, int lineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartEndBoundaryBegin({beginOffset},{lineNumber})");
			}

			protected override void OnMultipartEndBoundaryRead (byte[] buffer, int startIndex, int count, long beginOffset, int lineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartEndBoundaryRead({Encoding.Latin1.GetString (buffer, startIndex, count)},{beginOffset},{lineNumber})");
			}

			protected override void OnMultipartEndBoundaryEnd (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartEndBoundaryEnd({beginOffset},{lineNumber},{endOffset})");
			}

			protected override void OnMultipartPreambleBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartPreambleBegin({beginOffset},{beginLineNumber})");
			}

			protected override void OnMultipartPreambleRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartPreambleRead({Encoding.Latin1.GetString (buffer, startIndex, count)})");
			}

			protected override void OnMultipartPreambleEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartPreambleEnd({beginOffset},{beginLineNumber},{endOffset},{lines})");
			}

			protected override void OnMultipartEpilogueBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartEpilogueBegin({beginOffset},{beginLineNumber})");
			}

			protected override void OnMultipartEpilogueRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartEpilogueRead({Encoding.Latin1.GetString (buffer, startIndex, count)})");
			}

			protected override void OnMultipartEpilogueEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartEpilogueEnd({beginOffset},{beginLineNumber},{endOffset},{lines})");
			}

			protected override void OnMultipartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartEnd({contentType.MimeType},{beginOffset},{beginLineNumber},{headersEndOffset},{endOffset},{lines})");
			}
		}

		class RecordingMimeParser : MimeParser
		{
			public readonly List<string> Events = new List<string> ();

			public RecordingMimeParser (Stream stream, MimeFormat format) : base (stream, format)
			{
			}

			protected override void OnMboxMarkerBegin (long beginOffset, int lineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMboxMarkerBegin({beginOffset},{lineNumber})");
				base.OnMboxMarkerBegin (beginOffset, lineNumber, cancellationToken);
			}

			protected override void OnMboxMarkerRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMboxMarkerRead({Encoding.Latin1.GetString (buffer, startIndex, count)})");
				base.OnMboxMarkerRead (buffer, startIndex, count, cancellationToken);
			}

			protected override void OnMboxMarkerEnd (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMboxMarkerEnd({beginOffset},{lineNumber},{endOffset})");
				base.OnMboxMarkerEnd (beginOffset, lineNumber, endOffset, cancellationToken);
			}

			protected override void OnHeadersBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnHeadersBegin({beginOffset},{beginLineNumber})");
				base.OnHeadersBegin (beginOffset, beginLineNumber, cancellationToken);
			}

			protected override void OnHeaderRead (Header header, int beginLineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnHeaderRead({header.Field},{Encoding.Latin1.GetString (header.RawValue)},{beginLineNumber})");
				base.OnHeaderRead (header, beginLineNumber, cancellationToken);
			}

			protected override void OnHeadersEnd (long beginOffset, int beginLineNumber, long endOffset, int endLineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnHeadersEnd({beginOffset},{beginLineNumber},{endOffset},{endLineNumber})");
				base.OnHeadersEnd (beginOffset, beginLineNumber, endOffset, endLineNumber, cancellationToken);
			}

			protected override void OnBodySeparator (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken)
			{
				Events.Add ($"OnBodySeparator({beginOffset},{lineNumber},{endOffset})");
				base.OnBodySeparator (beginOffset, lineNumber, endOffset, cancellationToken);
			}

			protected override void OnMimeMessageBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMimeMessageBegin({beginOffset},{beginLineNumber})");
				base.OnMimeMessageBegin (beginOffset, beginLineNumber, cancellationToken);
			}

			protected override void OnMimeMessageEnd (long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMimeMessageEnd({beginOffset},{beginLineNumber},{headersEndOffset},{endOffset},{lines})");
				base.OnMimeMessageEnd (beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			}

			protected override void OnMimePartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMimePartBegin({contentType.MimeType},{beginOffset},{beginLineNumber})");
				base.OnMimePartBegin (contentType, beginOffset, beginLineNumber, cancellationToken);
			}

			protected override void OnMimePartContentBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMimePartContentBegin({beginOffset},{beginLineNumber})");
				base.OnMimePartContentBegin (beginOffset, beginLineNumber, cancellationToken);
			}

			protected override void OnMimePartContentRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMimePartContentRead({Encoding.Latin1.GetString (buffer, startIndex, count)})");
				base.OnMimePartContentRead (buffer, startIndex, count, cancellationToken);
			}

			protected override void OnMimePartContentEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, NewLineFormat? newLineFormat, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMimePartContentEnd({beginOffset},{beginLineNumber},{endOffset},{lines},{newLineFormat})");
				base.OnMimePartContentEnd (beginOffset, beginLineNumber, endOffset, lines, newLineFormat, cancellationToken);
			}

			protected override void OnMimePartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMimePartEnd({contentType.MimeType},{beginOffset},{beginLineNumber},{headersEndOffset},{endOffset},{lines})");
				base.OnMimePartEnd (contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			}

			protected override void OnMessagePartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMessagePartBegin({contentType.MimeType},{beginOffset},{beginLineNumber})");
				base.OnMessagePartBegin (contentType, beginOffset, beginLineNumber, cancellationToken);
			}

			protected override void OnMessagePartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMessagePartEnd({contentType.MimeType},{beginOffset},{beginLineNumber},{headersEndOffset},{endOffset},{lines})");
				base.OnMessagePartEnd (contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			}

			protected override void OnMultipartBegin (ContentType contentType, long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartBegin({contentType.MimeType},{beginOffset},{beginLineNumber})");
				base.OnMultipartBegin (contentType, beginOffset, beginLineNumber, cancellationToken);
			}

			protected override void OnMultipartBoundaryBegin (long beginOffset, int lineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartBoundaryBegin({beginOffset},{lineNumber})");
				base.OnMultipartBoundaryBegin (beginOffset, lineNumber, cancellationToken);
			}

			protected override void OnMultipartBoundaryRead (byte[] buffer, int startIndex, int count, long beginOffset, int lineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartBoundaryRead({Encoding.Latin1.GetString (buffer, startIndex, count)},{beginOffset},{lineNumber})");
				base.OnMultipartBoundaryRead (buffer, startIndex, count, beginOffset, lineNumber, cancellationToken);
			}

			protected override void OnMultipartBoundaryEnd (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartBoundaryEnd({beginOffset},{lineNumber},{endOffset})");
				base.OnMultipartBoundaryEnd (beginOffset, lineNumber, endOffset, cancellationToken);
			}

			protected override void OnMultipartEndBoundaryBegin (long beginOffset, int lineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartEndBoundaryBegin({beginOffset},{lineNumber})");
				base.OnMultipartEndBoundaryBegin (beginOffset, lineNumber, cancellationToken);
			}

			protected override void OnMultipartEndBoundaryRead (byte[] buffer, int startIndex, int count, long beginOffset, int lineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartEndBoundaryRead({Encoding.Latin1.GetString (buffer, startIndex, count)},{beginOffset},{lineNumber})");
				base.OnMultipartEndBoundaryRead (buffer, startIndex, count, beginOffset, lineNumber, cancellationToken);
			}

			protected override void OnMultipartEndBoundaryEnd (long beginOffset, int lineNumber, long endOffset, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartEndBoundaryEnd({beginOffset},{lineNumber},{endOffset})");
				base.OnMultipartEndBoundaryEnd (beginOffset, lineNumber, endOffset, cancellationToken);
			}

			protected override void OnMultipartPreambleBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartPreambleBegin({beginOffset},{beginLineNumber})");
				base.OnMultipartPreambleBegin (beginOffset, beginLineNumber, cancellationToken);
			}

			protected override void OnMultipartPreambleRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartPreambleRead({Encoding.Latin1.GetString (buffer, startIndex, count)})");
				base.OnMultipartPreambleRead (buffer, startIndex, count, cancellationToken);
			}

			protected override void OnMultipartPreambleEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartPreambleEnd({beginOffset},{beginLineNumber},{endOffset},{lines})");
				base.OnMultipartPreambleEnd (beginOffset, beginLineNumber, endOffset, lines, cancellationToken);
			}

			protected override void OnMultipartEpilogueBegin (long beginOffset, int beginLineNumber, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartEpilogueBegin({beginOffset},{beginLineNumber})");
				base.OnMultipartEpilogueBegin (beginOffset, beginLineNumber, cancellationToken);
			}

			protected override void OnMultipartEpilogueRead (byte[] buffer, int startIndex, int count, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartEpilogueRead({Encoding.Latin1.GetString (buffer, startIndex, count)})");
				base.OnMultipartEpilogueRead (buffer, startIndex, count, cancellationToken);
			}

			protected override void OnMultipartEpilogueEnd (long beginOffset, int beginLineNumber, long endOffset, int lines, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartEpilogueEnd({beginOffset},{beginLineNumber},{endOffset},{lines})");
				base.OnMultipartEpilogueEnd (beginOffset, beginLineNumber, endOffset, lines, cancellationToken);
			}

			protected override void OnMultipartEnd (ContentType contentType, long beginOffset, int beginLineNumber, long headersEndOffset, long endOffset, int lines, CancellationToken cancellationToken)
			{
				Events.Add ($"OnMultipartEnd({contentType.MimeType},{beginOffset},{beginLineNumber},{headersEndOffset},{endOffset},{lines})");
				base.OnMultipartEnd (contentType, beginOffset, beginLineNumber, headersEndOffset, endOffset, lines, cancellationToken);
			}
		}
	}
}

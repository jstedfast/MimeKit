//
// TnefWriterTests.cs
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

using MimeKit.Tnef;

using UnitTests.IO;

namespace UnitTests.Tnef {
	[TestFixture]
	public class TnefWriterTests
	{
		static readonly Guid PropertySet = new Guid ("{00062008-0000-0000-C000-000000000046}");
		static readonly byte[] RenderData = new byte[14];

		static TnefPropertyTag CustomTag (int index, TnefPropertyType type)
		{
			return new TnefPropertyTag ((TnefPropertyId) (0x6600 + index), type);
		}

		static byte[] Write (Action<TnefWriter> build, int codepage = 1252)
		{
			using var output = new MemoryStream ();

			using (var writer = new TnefWriter (output, codepage, leaveOpen: true)) {
				build (writer);
				writer.Flush ();
			}

			return output.ToArray ();
		}

		static async Task<byte[]> WriteAsync (Func<TnefWriter, Task> build, int codepage = 1252)
		{
			using var output = new MemoryStream ();

			using (var writer = new TnefWriter (output, codepage, leaveOpen: true)) {
				await build (writer);
				await writer.FlushAsync ();
			}

			return output.ToArray ();
		}

		static TestTnefComplianceLogger AssertConformant (byte[] tnef)
		{
			var logger = new TestTnefComplianceLogger ();

			using (var reader = new TnefReader (new MemoryStream (tnef, false)) { ComplianceLogger = logger }) {
				while (reader.Read ()) {
					switch (reader.Tag) {
					case TnefAttributeTag.MapiProperties:
					case TnefAttributeTag.Attachment:
						var properties = reader.GetPropertyReader ();
						while (properties.ReadNextProperty ())
							properties.ReadProperty ();
						break;
					case TnefAttributeTag.RecipientTable:
						var rows = reader.GetPropertyReader ();
						while (rows.ReadNextRow ()) {
							while (rows.ReadNextProperty ())
								rows.ReadProperty ();
						}
						break;
					}
				}
			}

			Assert.That (logger.Issues, Is.Empty, "compliance issues");

			return logger;
		}

		static TnefPropertySet ReadMessageProperties (byte[] tnef)
		{
			using var reader = new TnefReader (new MemoryStream (tnef, false));

			while (reader.Read ()) {
				if (reader.Tag == TnefAttributeTag.MapiProperties)
					return reader.GetPropertyReader ().ReadPropertySet ();
			}

			throw new AssertionException ("attMsgProps not found");
		}

		static List<(TnefAttributeLevel Level, TnefAttributeTag Tag)> ReadAttributes (byte[] tnef)
		{
			var attributes = new List<(TnefAttributeLevel, TnefAttributeTag)> ();

			using var reader = new TnefReader (new MemoryStream (tnef, false));

			while (reader.Read ())
				attributes.Add ((reader.Level, reader.Tag));

			return attributes;
		}

		#region Constructor

		[Test]
		public void TestArgumentExceptions ()
		{
			using var output = new MemoryStream ();

			Assert.Throws<ArgumentNullException> (() => new TnefWriter (null!));
			Assert.Throws<ArgumentException> (() => new TnefWriter (new MemoryStream (new byte[16], false)));
			Assert.Throws<ArgumentOutOfRangeException> (() => new TnefWriter (output, 1200));
			Assert.Throws<ArgumentOutOfRangeException> (() => new TnefWriter (output, 1201));
			Assert.Throws<ArgumentOutOfRangeException> (() => new TnefWriter (output, 12000));
			Assert.Throws<ArgumentOutOfRangeException> (() => new TnefWriter (output, 12001));
			Assert.Throws<ArgumentOutOfRangeException> (() => new TnefWriter (output, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => new TnefWriter (output, 99999));

			using var writer = new TnefWriter (output);

			Assert.Throws<ArgumentNullException> (() => writer.WriteAttribute (TnefAttributeTag.Subject, (string) null!));
			Assert.Throws<ArgumentNullException> (() => writer.WriteAttribute (TnefAttributeTag.AttachData, (byte[]) null!));
			Assert.Throws<ArgumentNullException> (() => writer.WriteAttribute (TnefAttributeTag.AttachData, null!, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => writer.WriteAttribute (TnefAttributeTag.Body, new byte[1], -1, 1));
			Assert.Throws<ArgumentOutOfRangeException> (() => writer.WriteAttribute (TnefAttributeTag.Body, new byte[1], 0, 2));
			Assert.ThrowsAsync<ArgumentNullException> (() => writer.WriteAttributeAsync (TnefAttributeTag.Subject, (string) null!));
			Assert.ThrowsAsync<ArgumentNullException> (() => writer.WriteAttributeAsync (TnefAttributeTag.AttachData, (byte[]) null!));
			Assert.ThrowsAsync<ArgumentNullException> (() => writer.WriteAttributeAsync (TnefAttributeTag.AttachData, null!, 0, 0));
			Assert.ThrowsAsync<ArgumentOutOfRangeException> (() => writer.WriteAttributeAsync (TnefAttributeTag.Body, new byte[1], -1, 1));
			Assert.ThrowsAsync<ArgumentOutOfRangeException> (() => writer.WriteAttributeAsync (TnefAttributeTag.Body, new byte[1], 0, 2));
		}

		[Test]
		public void TestProperties ()
		{
			using var output = new MemoryStream ();
			using var writer = new TnefWriter (output, 1251, 0x1234);

			Assert.That (writer.Codepage, Is.EqualTo (1251));
			Assert.That (writer.LegacyKey, Is.EqualTo ((ushort) 0x1234));
		}

		[Test]
		public void TestLeaveOpen ()
		{
			var output = new MemoryStream ();

			new TnefWriter (output, leaveOpen: true).Dispose ();
			Assert.That (output.CanWrite, Is.True, "leaveOpen: true");

			new TnefWriter (output).Dispose ();
			Assert.That (output.CanWrite, Is.False, "leaveOpen: false");
		}

		[Test]
		public void TestDisposeTwiceAndUseAfterDispose ()
		{
			using var output = new MemoryStream ();
			var writer = new TnefWriter (output, leaveOpen: true);

			writer.Dispose ();
			writer.Dispose ();

			Assert.Throws<ObjectDisposedException> (() => writer.WriteAttribute (TnefAttributeTag.Subject, "x"));
			Assert.ThrowsAsync<ObjectDisposedException> (() => writer.WriteAttributeAsync (TnefAttributeTag.Subject, "x"));
			Assert.Throws<ObjectDisposedException> (() => writer.OpenAttributeStream (TnefAttributeTag.Body));
			Assert.Throws<ObjectDisposedException> (() => writer.OpenPropertyWriter (TnefAttributeTag.MapiProperties));
			Assert.Throws<ObjectDisposedException> (() => writer.Flush ());
			Assert.ThrowsAsync<ObjectDisposedException> (() => writer.FlushAsync ());
		}

		#endregion

		#region Stream header and attributes

		[Test]
		public void TestEmptyStream ()
		{
			var tnef = Write (writer => { });

			// Signature + LegacyKey + attTnefVersion + attOemCodepage
			Assert.That (tnef.Length, Is.EqualTo (4 + 2 + (9 + 4 + 2) + (9 + 8 + 2)));
			Assert.That (BitConverter.ToInt32 (tnef, 0), Is.EqualTo (0x223E9F78));

			Assert.That (ReadAttributes (tnef), Is.EqualTo (new[] {
				(TnefAttributeLevel.Message, TnefAttributeTag.TnefVersion),
				(TnefAttributeLevel.Message, TnefAttributeTag.OemCodepage)
			}));

			using var reader = new TnefReader (new MemoryStream (tnef, false));

			while (reader.Read ()) { }

			Assert.That (reader.Codepage, Is.EqualTo (1252));
		}

		[Test]
		public void TestHeaderIsWrittenOnDispose ()
		{
			using var output = new MemoryStream ();

			using (var writer = new TnefWriter (output, 1252, 0xBEEF, true)) {
			}

			var tnef = output.ToArray ();

			Assert.That (tnef.Length, Is.EqualTo (40));

			using var reader = new TnefReader (new MemoryStream (tnef, false));

			while (reader.Read ()) { }

			Assert.That (reader.LegacyKey, Is.EqualTo ((ushort) 0xBEEF));
		}

		[Test]
		public void TestByteExactAgainstTnefBuilder ()
		{
			var codepage = new byte[8];

			BitConverter.GetBytes (1252).CopyTo (codepage, 0);

			var expected = new TnefBuilder (legacyKey: 0x0102)
				.WriteTnefVersion ()
				.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.OemCodepage, codepage)
				.WriteMessageClass ("IPM.Microsoft Mail.Note")
				.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Subject, Encoding.ASCII.GetBytes ("Hello\0"))
				.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Priority, new byte[] { 2, 0 })
				.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.DateSent, new byte[] { 0xE8, 0x07, 1, 0, 2, 0, 3, 0, 4, 0, 5, 0, 2, 0 })
				.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData, RenderData)
				.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachData, new byte[] { 0xFF, 0xFE, 0x80 })
				.ToArray ();

			using var output = new MemoryStream ();

			using (var writer = new TnefWriter (output, 1252, 0x0102, true)) {
				writer.WriteAttribute (TnefAttributeTag.MessageClass, "IPM.Microsoft Mail.Note");
				writer.WriteAttribute (TnefAttributeTag.Subject, "Hello");
				writer.WriteAttribute (TnefAttributeTag.Priority, 2);
				writer.WriteAttribute (TnefAttributeTag.DateSent, new DateTime (2024, 1, 2, 3, 4, 5));
				writer.WriteAttribute (TnefAttributeTag.AttachRenderData, RenderData);

				using (var stream = writer.OpenAttributeStream (TnefAttributeTag.AttachData))
					stream.Write (new byte[] { 0xFF, 0xFE, 0x80 }, 0, 3);
			}

			Assert.That (output.ToArray (), Is.EqualTo (expected));
		}

		[Test]
		public async Task TestByteExactAsyncMatchesSync ()
		{
			var date = new DateTime (2024, 1, 2, 3, 4, 5);

			var expected = Write (writer => {
				writer.WriteAttribute (TnefAttributeTag.MessageClass, "IPM.Note");
				writer.WriteAttribute (TnefAttributeTag.Subject, "Hello");
				writer.WriteAttribute (TnefAttributeTag.Priority, 3);
				writer.WriteAttribute (TnefAttributeTag.DateSent, date);
				writer.WriteAttribute (TnefAttributeTag.MessageStatus, new byte[] { 0x20 });
				writer.WriteAttribute (TnefAttributeTag.AttachRenderData, RenderData);
				writer.WriteAttribute (TnefAttributeTag.AttachData, new byte[] { 1, 2, 3, 4 }, 1, 2);
			});

			var actual = await WriteAsync (async writer => {
				await writer.WriteAttributeAsync (TnefAttributeTag.MessageClass, "IPM.Note");
				await writer.WriteAttributeAsync (TnefAttributeTag.Subject, "Hello");
				await writer.WriteAttributeAsync (TnefAttributeTag.Priority, 3);
				await writer.WriteAttributeAsync (TnefAttributeTag.DateSent, date);
				await writer.WriteAttributeAsync (TnefAttributeTag.MessageStatus, new byte[] { 0x20 });
				await writer.WriteAttributeAsync (TnefAttributeTag.AttachRenderData, RenderData);
				await writer.WriteAttributeAsync (TnefAttributeTag.AttachData, new byte[] { 1, 2, 3, 4 }, 1, 2);
			});

			Assert.That (actual, Is.EqualTo (expected));
			AssertConformant (actual);
		}

		[Test]
		public void TestAttributeValues ()
		{
			var date = new DateTime (2023, 12, 31, 23, 59, 58);

			var tnef = Write (writer => {
				writer.WriteAttribute (TnefAttributeTag.MessageClass, "IPM.Note");
				writer.WriteAttribute (TnefAttributeTag.Subject, "Subject \u00e9");
				writer.WriteAttribute (TnefAttributeTag.Body, "Body text");
				writer.WriteAttribute (TnefAttributeTag.Priority, 1);
				writer.WriteAttribute (TnefAttributeTag.AidOwner, -5);
				writer.WriteAttribute (TnefAttributeTag.DateSent, date);
			});

			AssertConformant (tnef);

			using var reader = new TnefReader (new MemoryStream (tnef, false));

			while (reader.Read ()) {
				switch (reader.Tag) {
				case TnefAttributeTag.MessageClass: Assert.That (reader.ReadValueAsBytes (), Is.EqualTo (Encoding.ASCII.GetBytes ("IPM.Note\0"))); break;
				case TnefAttributeTag.Subject: Assert.That (reader.ReadValueAsString (), Is.EqualTo ("Subject \u00e9")); break;
				case TnefAttributeTag.Body: Assert.That (reader.ReadValueAsString (), Is.EqualTo ("Body text")); break;
				case TnefAttributeTag.Priority: Assert.That (reader.ReadValueAsInt16 (), Is.EqualTo (1)); break;
				case TnefAttributeTag.AidOwner: Assert.That (reader.ReadValueAsInt32 (), Is.EqualTo (-5)); break;
				case TnefAttributeTag.DateSent: Assert.That (reader.ReadValueAsDateTime (), Is.EqualTo (date)); break;
				}
			}
		}

		[Test]
		public void TestChecksumWrapsAt16Bits ()
		{
			var payload = new byte[1024];

			payload.AsSpan ().Fill (0xFF);

			var tnef = Write (writer => {
				writer.WriteAttribute (TnefAttributeTag.AttachRenderData, RenderData);
				writer.WriteAttribute (TnefAttributeTag.AttachData, payload);
				writer.WriteAttribute (TnefAttributeTag.AttachTitle, "x");

				using var stream = writer.OpenAttributeStream (TnefAttributeTag.AttachMetaFile);

				for (int i = 0; i < 100; i++)
					stream.Write (payload, 0, payload.Length);
			});

			AssertConformant (tnef);

			Assert.That (BitConverter.ToUInt16 (tnef, tnef.Length - 2), Is.EqualTo ((ushort) ((100 * 1024 * 255) & 0xFFFF)));
		}

		[Test]
		public void TestCodepage ()
		{
			const string subject = "\u041f\u0440\u0438\u0432\u0435\u0442";

			var tnef = Write (writer => {
				writer.WriteAttribute (TnefAttributeTag.Subject, subject);

				using var properties = writer.OpenPropertyWriter (TnefAttributeTag.MapiProperties);

				properties.WritePropertyTag (TnefPropertyTag.SubjectA);
				properties.WriteValue (subject);
			}, 1251);

			var message = TnefMessage.Load (new MemoryStream (tnef, false));

			Assert.That (message.Codepage, Is.EqualTo (1251));
			Assert.That (message.Subject, Is.EqualTo (subject));

			using var reader = new TnefReader (new MemoryStream (tnef, false));

			while (reader.Read ()) {
				if (reader.Tag == TnefAttributeTag.Subject)
					Assert.That (reader.ReadValueAsString (), Is.EqualTo (subject));
			}
		}

		#endregion

		#region Attribute validation

		[Test]
		public void TestInvalidAttributeTags ()
		{
			using var output = new MemoryStream ();
			using var writer = new TnefWriter (output);

			Assert.Throws<ArgumentException> (() => writer.WriteAttribute (TnefAttributeTag.TnefVersion, 0x10000));
			Assert.Throws<ArgumentException> (() => writer.WriteAttribute (TnefAttributeTag.OemCodepage, new byte[8]));
			Assert.Throws<ArgumentException> (() => writer.WriteAttribute (TnefAttributeTag.Null, new byte[1]));
			Assert.Throws<ArgumentException> (() => writer.WriteAttribute ((TnefAttributeTag) 0x00061234, new byte[1]));
			Assert.Throws<ArgumentException> (() => writer.OpenAttributeStream (TnefAttributeTag.TnefVersion));
			Assert.Throws<ArgumentException> (() => writer.OpenPropertyWriter (TnefAttributeTag.Subject));
			Assert.Throws<ArgumentException> (() => writer.OpenPropertyWriter (TnefAttributeTag.AttachData));
		}

		[Test]
		public void TestInvalidAttributeValues ()
		{
			using var output = new MemoryStream ();
			using var writer = new TnefWriter (output);

			// Integer attributes.
			Assert.Throws<ArgumentException> (() => writer.WriteAttribute (TnefAttributeTag.Subject, 5));
			Assert.Throws<ArgumentException> (() => writer.WriteAttribute (TnefAttributeTag.DateSent, 5));
			Assert.Throws<ArgumentException> (() => writer.WriteAttribute (TnefAttributeTag.MessageClass, 5));
			Assert.Throws<ArgumentOutOfRangeException> (() => writer.WriteAttribute (TnefAttributeTag.Priority, 0x8000));
			Assert.Throws<ArgumentOutOfRangeException> (() => writer.WriteAttribute (TnefAttributeTag.Priority, -0x8001));
			Assert.ThrowsAsync<ArgumentOutOfRangeException> (() => writer.WriteAttributeAsync (TnefAttributeTag.Priority, 0x8000));

			// String attributes.
			Assert.Throws<ArgumentException> (() => writer.WriteAttribute (TnefAttributeTag.Priority, "x"));
			Assert.Throws<ArgumentException> (() => writer.WriteAttribute (TnefAttributeTag.DateSent, "x"));
			Assert.Throws<ArgumentException> (() => writer.WriteAttribute (TnefAttributeTag.MessageClass, ""));
			Assert.Throws<ArgumentException> (() => writer.WriteAttribute (TnefAttributeTag.MessageClass, "IPM.\u00e9"));
			Assert.Throws<ArgumentException> (() => writer.WriteAttribute (TnefAttributeTag.MessageClass, "IPM\0Note"));
			Assert.Throws<ArgumentException> (() => writer.WriteAttribute (TnefAttributeTag.MessageClass, new string ('x', 255)));
			Assert.ThrowsAsync<ArgumentException> (() => writer.WriteAttributeAsync (TnefAttributeTag.MessageClass, ""));

			// Date attributes.
			Assert.Throws<ArgumentException> (() => writer.WriteAttribute (TnefAttributeTag.Subject, DateTime.Now));
			Assert.ThrowsAsync<ArgumentException> (() => writer.WriteAttributeAsync (TnefAttributeTag.Subject, DateTime.Now));

			// None of the rejected values should have changed the state of the writer.
			writer.WriteAttribute (TnefAttributeTag.MessageClass, new string ('x', 254));
			writer.Flush ();

			AssertConformant (output.ToArray ());
		}

		[Test]
		public void TestAttributeOrdering ()
		{
			using var output = new MemoryStream ();
			using var writer = new TnefWriter (output);

			// An attachment attribute requires attAttachRenderData first.
			Assert.Throws<InvalidOperationException> (() => writer.WriteAttribute (TnefAttributeTag.AttachTitle, "x"));
			Assert.Throws<InvalidOperationException> (() => writer.OpenPropertyWriter (TnefAttributeTag.Attachment));

			writer.WriteAttribute (TnefAttributeTag.Subject, "x");

			using (var properties = writer.OpenPropertyWriter (TnefAttributeTag.MapiProperties)) {
			}

			// attMsgProps is the last message attribute.
			Assert.Throws<InvalidOperationException> (() => writer.WriteAttribute (TnefAttributeTag.Subject, "y"));
			Assert.Throws<InvalidOperationException> (() => writer.OpenPropertyWriter (TnefAttributeTag.MapiProperties));

			writer.WriteAttribute (TnefAttributeTag.AttachRenderData, RenderData);
			writer.WriteAttribute (TnefAttributeTag.AttachTitle, "a.txt");

			// No message attributes after the first attachment.
			Assert.Throws<InvalidOperationException> (() => writer.WriteAttribute (TnefAttributeTag.Priority, 1));
			Assert.ThrowsAsync<InvalidOperationException> (() => writer.WriteAttributeAsync (TnefAttributeTag.Priority, 1));

			using (var properties = writer.OpenPropertyWriter (TnefAttributeTag.Attachment)) {
			}

			// attAttachment is the last attribute of the attachment.
			Assert.Throws<InvalidOperationException> (() => writer.WriteAttribute (TnefAttributeTag.AttachTitle, "b.txt"));
			Assert.Throws<InvalidOperationException> (() => writer.OpenPropertyWriter (TnefAttributeTag.Attachment));

			writer.WriteAttribute (TnefAttributeTag.AttachRenderData, RenderData);
			writer.WriteAttribute (TnefAttributeTag.AttachTitle, "b.txt");
			writer.Flush ();

			var tnef = output.ToArray ();

			AssertConformant (tnef);
			Assert.That (ReadAttributes (tnef).Skip (2), Is.EqualTo (new[] {
				(TnefAttributeLevel.Message, TnefAttributeTag.Subject),
				(TnefAttributeLevel.Message, TnefAttributeTag.MapiProperties),
				(TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData),
				(TnefAttributeLevel.Attachment, TnefAttributeTag.AttachTitle),
				(TnefAttributeLevel.Attachment, TnefAttributeTag.Attachment),
				(TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData),
				(TnefAttributeLevel.Attachment, TnefAttributeTag.AttachTitle)
			}));
		}

		[Test]
		public void TestOpenChildBlocksOtherAttributes ()
		{
			using var output = new MemoryStream ();
			using var writer = new TnefWriter (output);

			var stream = writer.OpenAttributeStream (TnefAttributeTag.Body);

			Assert.Throws<InvalidOperationException> (() => writer.WriteAttribute (TnefAttributeTag.Subject, "x"));
			Assert.ThrowsAsync<InvalidOperationException> (() => writer.WriteAttributeAsync (TnefAttributeTag.Subject, "x"));
			Assert.Throws<InvalidOperationException> (() => writer.OpenAttributeStream (TnefAttributeTag.Body));
			Assert.Throws<InvalidOperationException> (() => writer.OpenPropertyWriter (TnefAttributeTag.MapiProperties));
			Assert.Throws<InvalidOperationException> (() => writer.Flush ());
			Assert.ThrowsAsync<InvalidOperationException> (() => writer.FlushAsync ());

			stream.Write (Encoding.ASCII.GetBytes ("body\0"), 0, 5);
			stream.Dispose ();

			writer.WriteAttribute (TnefAttributeTag.Subject, "x");
			writer.Flush ();

			var tnef = output.ToArray ();

			AssertConformant (tnef);
			Assert.That (ReadAttributes (tnef).Skip (2).Select (a => a.Tag), Is.EqualTo (new[] { TnefAttributeTag.Body, TnefAttributeTag.Subject }));
		}

		[Test]
		public void TestUndisposedChildIsDiscardedOnDispose ()
		{
			using var output = new MemoryStream ();

			using (var writer = new TnefWriter (output, leaveOpen: true)) {
				writer.WriteAttribute (TnefAttributeTag.Subject, "x");

				var stream = writer.OpenAttributeStream (TnefAttributeTag.Body);

				stream.Write (new byte[] { 1, 2, 3 }, 0, 3);
			}

			var tnef = output.ToArray ();

			AssertConformant (tnef);
			Assert.That (ReadAttributes (tnef).Skip (2).Select (a => a.Tag), Is.EqualTo (new[] { TnefAttributeTag.Subject }));
		}

		[Test]
		public void TestAttributeStream ()
		{
			var data = new byte[100000];

			new Random (42).NextBytes (data);

			var tnef = Write (writer => {
				writer.WriteAttribute (TnefAttributeTag.AttachRenderData, RenderData);

				using var stream = writer.OpenAttributeStream (TnefAttributeTag.AttachData);

				Assert.That (stream.CanWrite, Is.True);
				Assert.That (stream.CanRead, Is.False);
				Assert.That (stream.CanSeek, Is.False);

				stream.Write (data, 0, 1);
				stream.WriteAsync (data, 1, 999).GetAwaiter ().GetResult ();
				stream.Write (data, 1000, data.Length - 1000);
				stream.Flush ();

				Assert.That (stream.Length, Is.EqualTo (data.Length));
			});

			AssertConformant (tnef);

			using var reader = new TnefReader (new MemoryStream (tnef, false));

			while (reader.Read ()) {
				if (reader.Tag == TnefAttributeTag.AttachData)
					Assert.That (reader.ReadValueAsBytes (), Is.EqualTo (data));
			}
		}

		[Test]
		public async Task TestAttributeStreamAsync ()
		{
			var data = new byte[100000];

			new Random (42).NextBytes (data);

			var expected = Write (writer => {
				writer.WriteAttribute (TnefAttributeTag.AttachRenderData, RenderData);

				using (var stream = writer.OpenAttributeStream (TnefAttributeTag.AttachData))
					stream.Write (data, 0, data.Length);

				using (var properties = writer.OpenPropertyWriter (TnefAttributeTag.Attachment)) {
					properties.WritePropertyTag (TnefPropertyTag.AttachLongFilenameW);
					properties.WriteValue ("data.bin");
				}

				writer.WriteAttribute (TnefAttributeTag.AttachRenderData, RenderData);
			});

			var actual = await WriteAsync (async writer => {
				await writer.WriteAttributeAsync (TnefAttributeTag.AttachRenderData, RenderData);

				using (var stream = writer.OpenAttributeStream (TnefAttributeTag.AttachData)) {
					await stream.WriteAsync (data, 0, data.Length);
					await stream.FlushAsync ();
				}

				using (var properties = writer.OpenPropertyWriter (TnefAttributeTag.Attachment)) {
					properties.WritePropertyTag (TnefPropertyTag.AttachLongFilenameW);
					properties.WriteValue ("data.bin");
				}

				// Writing the next attribute writes the buffered ones first.
				await writer.WriteAttributeAsync (TnefAttributeTag.AttachRenderData, RenderData);
			});

			Assert.That (actual, Is.EqualTo (expected));
			AssertConformant (actual);
		}

		[Test]
		public async Task TestAttributeStreamIsWrittenByFlushAsync ()
		{
			var expected = Write (writer => {
				using (var stream = writer.OpenAttributeStream (TnefAttributeTag.Body))
					stream.Write (new byte[] { 1, 2, 3 }, 0, 3);
			});

			var actual = await WriteAsync (writer => {
				using (var stream = writer.OpenAttributeStream (TnefAttributeTag.Body))
					stream.Write (new byte[] { 1, 2, 3 }, 0, 3);

				return Task.CompletedTask;
			});

			Assert.That (actual, Is.EqualTo (expected));
			Assert.That (ReadAttributes (actual).Select (a => a.Tag), Is.EqualTo (new[] { TnefAttributeTag.TnefVersion, TnefAttributeTag.OemCodepage, TnefAttributeTag.Body }));
		}

		[Test]
		public async Task TestAttributeStreamUnsupportedOperations ()
		{
			using var output = new MemoryStream ();
			using var writer = new TnefWriter (output);

			var stream = writer.OpenAttributeStream (TnefAttributeTag.Body);
			var buffer = new byte[16];

			stream.Write (buffer, 0, 3);

			Assert.That (stream.Position, Is.EqualTo (3));
			Assert.Throws<NotSupportedException> (() => stream.Position = 0);
			Assert.Throws<NotSupportedException> (() => stream.Seek (0, SeekOrigin.Begin));
			Assert.Throws<NotSupportedException> (() => stream.SetLength (0));
			Assert.Throws<NotSupportedException> (() => stream.Read (buffer, 0, buffer.Length));

			Assert.Throws<ArgumentNullException> (() => stream.Write (null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => stream.Write (buffer, -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => stream.Write (buffer, 17, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => stream.Write (buffer, 0, -1));
			Assert.Throws<ArgumentOutOfRangeException> (() => stream.Write (buffer, 8, 9));

			using (var cts = new CancellationTokenSource ()) {
				cts.Cancel ();

				Assert.ThrowsAsync<OperationCanceledException> (() => stream.WriteAsync (buffer, 0, 1, cts.Token));
				Assert.ThrowsAsync<OperationCanceledException> (() => stream.FlushAsync (cts.Token));
			}

			stream.Dispose ();
			stream.Dispose ();

			Assert.That (stream.CanWrite, Is.False);
			Assert.Throws<ObjectDisposedException> (() => stream.Write (buffer, 0, 1));
			Assert.Throws<ObjectDisposedException> (() => stream.Flush ());
			Assert.ThrowsAsync<ObjectDisposedException> (() => stream.FlushAsync ());

			await writer.FlushAsync ();

			Assert.That (ReadAttributes (output.ToArray ()).Last ().Tag, Is.EqualTo (TnefAttributeTag.Body));
		}

		[Test]
		public void TestDisposeWithFailingStream ()
		{
			var output = new ThrowingWriteStream ();
			var writer = new TnefWriter (output);

			using (var stream = writer.OpenAttributeStream (TnefAttributeTag.Body))
				stream.Write (new byte[] { 1, 2, 3 }, 0, 3);

			using (var stream = writer.OpenAttributeStream (TnefAttributeTag.Subject))
				stream.Write (new byte[] { 1, 2, 3 }, 0, 3);

			Assert.Throws<IOException> (() => writer.Flush ());
			Assert.ThrowsAsync<IOException> (() => writer.FlushAsync ());

			// Dispose must not throw, even though the queued attributes cannot be written.
			Assert.DoesNotThrow (() => writer.Dispose ());
			Assert.That (output.IsDisposed, Is.True);
		}

		#endregion

		#region Properties

		static readonly (TnefPropertyType Type, object Single, Array Multi)[] PropertyValues = {
			(TnefPropertyType.I2, (short) -2, new short[] { 1, short.MinValue, short.MaxValue }),
			(TnefPropertyType.Long, 0x12345678, new int[] { 1, -1, int.MaxValue }),
			(TnefPropertyType.R4, 1.5f, new float[] { -0.25f, float.MaxValue }),
			(TnefPropertyType.Double, Math.PI, new double[] { 1e300, -2.5 }),
			(TnefPropertyType.Currency, 12345.6789m, new decimal[] { -1.5m, 0m, 922337203685477.5807m }),
			(TnefPropertyType.AppTime, new DateTime (2020, 1, 2, 3, 4, 5), new DateTime[] { new DateTime (1999, 12, 31) }),
			(TnefPropertyType.Error, unchecked((int) 0x8004010F), new int[0]),
			(TnefPropertyType.Boolean, true, new bool[0]),
			(TnefPropertyType.I8, long.MinValue, new long[] { 0, long.MaxValue }),
			(TnefPropertyType.String8, "ascii text", new string[] { "", "a", "ab", "abc", "abcd" }),
			(TnefPropertyType.Unicode, "unicode \u2603 \U0001F600", new string[] { "", "x", "\u00e9\u00e8" }),
			(TnefPropertyType.SysTime, new DateTime (2021, 6, 7, 8, 9, 10, DateTimeKind.Utc), new DateTime[] { new DateTime (1601, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime (2100, 1, 1, 0, 0, 0, DateTimeKind.Utc) }),
			(TnefPropertyType.ClassId, new Guid ("01234567-89ab-cdef-0123-456789abcdef"), new Guid[] { Guid.Empty, PropertySet }),
			(TnefPropertyType.Binary, new byte[] { 1, 2, 3 }, new byte[][] { new byte[0], new byte[] { 1 }, new byte[] { 1, 2, 3, 4, 5 } }),
			(TnefPropertyType.Object, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 0xAA }, new byte[0][]),
		};

		static void WriteNativeValue (TnefPropertyWriter properties, object value)
		{
			switch (value) {
			case short i2: properties.WriteValue (i2); break;
			case int i4: properties.WriteValue (i4); break;
			case float r4: properties.WriteValue (r4); break;
			case double r8: properties.WriteValue (r8); break;
			case decimal currency: properties.WriteValue (currency); break;
			case DateTime date: properties.WriteValue (date); break;
			case bool boolean: properties.WriteValue (boolean); break;
			case long i8: properties.WriteValue (i8); break;
			case Guid guid: properties.WriteValue (guid); break;
			case string text: properties.WriteValue (text); break;
			case byte[] bytes: properties.WriteValue (bytes); break;
			default: throw new NotSupportedException ();
			}
		}

		static void WriteAllPropertyTypes (TnefPropertyWriter properties)
		{
			properties.WritePropertyTag (CustomTag (0, TnefPropertyType.Null));

			for (int i = 0; i < PropertyValues.Length; i++) {
				var (type, single, multi) = PropertyValues[i];

				properties.WritePropertyTag (CustomTag (1 + i, type));
				WriteNativeValue (properties, single);

				if (type is TnefPropertyType.Error or TnefPropertyType.Boolean or TnefPropertyType.Object)
					continue;

				properties.WritePropertyTag (CustomTag (0x40 + i, type | TnefPropertyType.MultiValued));

				foreach (var value in multi)
					WriteNativeValue (properties, value);
			}
		}

		static void AssertAllPropertyTypes (TnefPropertySet properties)
		{
			Assert.That (properties.TryGetValue (CustomTag (0, TnefPropertyType.Null), out var nullProperty), Is.True, "Null");
			Assert.That (nullProperty.Value, Is.Null, "Null value");

			for (int i = 0; i < PropertyValues.Length; i++) {
				var (type, single, multi) = PropertyValues[i];

				Assert.That (properties.TryGetValue (CustomTag (1 + i, type), out var property), Is.True, type.ToString ());
				Assert.That (property.Count, Is.EqualTo (1), $"{type} count");
				Assert.That (property.Value, Is.EqualTo (single), type.ToString ());

				if (type is TnefPropertyType.Error or TnefPropertyType.Boolean or TnefPropertyType.Object)
					continue;

				Assert.That (properties.TryGetValue (CustomTag (0x40 + i, type | TnefPropertyType.MultiValued), out property), Is.True, $"MV {type}");
				Assert.That (property.Count, Is.EqualTo (multi.Length), $"MV {type} count");
				Assert.That (property.Value, Is.EqualTo (multi), $"MV {type}");
			}
		}

		[Test]
		public void TestAllPropertyTypesRoundTrip ()
		{
			var tnef = Write (writer => {
				using var properties = writer.OpenPropertyWriter (TnefAttributeTag.MapiProperties);

				WriteAllPropertyTypes (properties);

				Assert.That (properties.PropertyCount, Is.EqualTo (1 + PropertyValues.Length + PropertyValues.Length - 3));
			});

			AssertConformant (tnef);
			AssertAllPropertyTypes (ReadMessageProperties (tnef));
		}

		[Test]
		public void TestWritePropertyRoundTrip ()
		{
			var original = Write (writer => {
				using var properties = writer.OpenPropertyWriter (TnefAttributeTag.MapiProperties);

				WriteAllPropertyTypes (properties);

				properties.WritePropertyTag (new TnefNameId (PropertySet, 0x8501), TnefPropertyType.Long);
				properties.WriteValue (15);
				properties.WritePropertyTag (new TnefNameId (PropertySet, "Keywords"), TnefPropertyType.Unicode | TnefPropertyType.MultiValued);
				properties.WriteValue ("one");
				properties.WriteValue ("two");
			});

			var set = ReadMessageProperties (original);

			var copy = Write (writer => {
				using var properties = writer.OpenPropertyWriter (TnefAttributeTag.MapiProperties);

				foreach (var property in set)
					properties.WriteProperty (property);
			});

			Assert.That (copy, Is.EqualTo (original));
		}

		[Test]
		public void TestNamedProperties ()
		{
			var byId = new TnefNameId (PropertySet, 0x8501);
			var byName = new TnefNameId (PropertySet, "Custom Name");
			var emptyName = new TnefNameId (PropertySet, "");

			var tnef = Write (writer => {
				using var properties = writer.OpenPropertyWriter (TnefAttributeTag.MapiProperties);

				properties.WritePropertyTag (byId, TnefPropertyType.Long);
				properties.WriteValue (1);
				properties.WritePropertyTag (byName, TnefPropertyType.Unicode);
				properties.WriteValue ("value");
				properties.WritePropertyTag (emptyName, TnefPropertyType.Boolean);
				properties.WriteValue (true);
				properties.WritePropertyTag (byId, TnefPropertyType.Long);
				properties.WriteValue (2);
			});

			AssertConformant (tnef);

			using var reader = new TnefReader (new MemoryStream (tnef, false));
			var ids = new List<TnefPropertyId> ();
			var names = new List<TnefNameId> ();

			while (reader.Read ()) {
				if (reader.Tag != TnefAttributeTag.MapiProperties)
					continue;

				var properties = reader.GetPropertyReader ();

				while (properties.ReadNextProperty ()) {
					Assert.That (properties.Tag.IsNamed, Is.True);
					ids.Add (properties.Tag.Id);
					names.Add (properties.Name!.Value);
				}
			}

			Assert.That (names, Is.EqualTo (new[] { byId, byName, emptyName, byId }));
			Assert.That (ids, Is.EqualTo (new[] { unchecked ((TnefPropertyId) 0x8000), unchecked ((TnefPropertyId) 0x8001), unchecked ((TnefPropertyId) 0x8002), unchecked ((TnefPropertyId) 0x8000) }));
		}

		[Test]
		public void TestNamedPropertyIdsAreSharedByEmbeddedMessages ()
		{
			var name = new TnefNameId (PropertySet, "Shared");

			var tnef = Write (writer => {
				writer.WriteAttribute (TnefAttributeTag.AttachRenderData, RenderData);

				using var properties = writer.OpenPropertyWriter (TnefAttributeTag.Attachment);

				properties.WritePropertyTag (TnefPropertyTag.AttachMethod);
				properties.WriteValue ((int) TnefAttachMethod.EmbeddedMessage);
				properties.WritePropertyTag (TnefPropertyTag.AttachDataObj);

				using var embedded = properties.OpenEmbeddedMessage ();
				using var embeddedProperties = embedded.OpenPropertyWriter (TnefAttributeTag.MapiProperties);

				embeddedProperties.WritePropertyTag (name, TnefPropertyType.Long);
				embeddedProperties.WriteValue (7);
			});

			var message = TnefMessage.Load (new MemoryStream (tnef, false));
			var nested = message.Attachments[0].LoadEmbeddedMessage ();

			Assert.That (nested.Properties.TryGetValue (name, out var property), Is.True);
			Assert.That (property.Value, Is.EqualTo (7));
		}

		[Test]
		public void TestRecipientTable ()
		{
			var tnef = Write (writer => {
				using var recipients = writer.OpenPropertyWriter (TnefAttributeTag.RecipientTable);

				Assert.Throws<InvalidOperationException> (() => recipients.WritePropertyTag (TnefPropertyTag.DisplayNameW));

				recipients.BeginRow ();
				recipients.WritePropertyTag (TnefPropertyTag.DisplayNameW);
				recipients.WriteValue ("Alice");
				recipients.WritePropertyTag (TnefPropertyTag.EmailAddressA);
				recipients.WriteValue ("alice@example.com");
				recipients.WritePropertyTag (TnefPropertyTag.RecipientType);
				recipients.WriteValue ((int) TnefRecipientType.To);
				Assert.That (recipients.PropertyCount, Is.EqualTo (3));

				recipients.BeginRow ();
				Assert.That (recipients.PropertyCount, Is.EqualTo (0));
				recipients.WritePropertyTag (TnefPropertyTag.DisplayNameW);
				recipients.WriteValue ("Bob");
				recipients.WritePropertyTag (TnefPropertyTag.RecipientType);
				recipients.WriteValue ((int) TnefRecipientType.Cc);

				recipients.BeginRow ();

				Assert.That (recipients.RowCount, Is.EqualTo (3));
			});

			AssertConformant (tnef);

			var message = TnefMessage.Load (new MemoryStream (tnef, false));

			Assert.That (message.Recipients, Has.Count.EqualTo (3));
			Assert.That (message.Recipients[0].DisplayName, Is.EqualTo ("Alice"));
			Assert.That (message.Recipients[0].EmailAddress, Is.EqualTo ("alice@example.com"));
			Assert.That (message.Recipients[0].RecipientType, Is.EqualTo (TnefRecipientType.To));
			Assert.That (message.Recipients[1].DisplayName, Is.EqualTo ("Bob"));
			Assert.That (message.Recipients[1].RecipientType, Is.EqualTo (TnefRecipientType.Cc));
			Assert.That (message.Recipients[2].Properties.Count, Is.EqualTo (0));
		}

		[Test]
		public void TestEmptyRecipientTable ()
		{
			var tnef = Write (writer => {
				using var recipients = writer.OpenPropertyWriter (TnefAttributeTag.RecipientTable);

				Assert.That (recipients.RowCount, Is.EqualTo (0));
			});

			AssertConformant (tnef);
			Assert.That (TnefMessage.Load (new MemoryStream (tnef, false)).Recipients, Is.Empty);
		}

		[Test]
		public void TestPropertyWriterValidation ()
		{
			using var output = new MemoryStream ();
			using var writer = new TnefWriter (output);
			using var properties = writer.OpenPropertyWriter (TnefAttributeTag.MapiProperties);

			Assert.Throws<InvalidOperationException> (() => properties.BeginRow ());
			Assert.Throws<InvalidOperationException> (() => properties.WriteValue (1));

			// Invalid property tags.
			Assert.Throws<ArgumentException> (() => properties.WritePropertyTag (CustomTag (0, TnefPropertyType.Unspecified)));
			Assert.Throws<ArgumentException> (() => properties.WritePropertyTag (CustomTag (0, (TnefPropertyType) 0x99)));
			Assert.Throws<ArgumentException> (() => properties.WritePropertyTag (CustomTag (0, TnefPropertyType.Null | TnefPropertyType.MultiValued)));
			Assert.Throws<ArgumentException> (() => properties.WritePropertyTag (CustomTag (0, TnefPropertyType.Boolean | TnefPropertyType.MultiValued)));
			Assert.Throws<ArgumentException> (() => properties.WritePropertyTag (CustomTag (0, TnefPropertyType.Error | TnefPropertyType.MultiValued)));
			Assert.Throws<ArgumentException> (() => properties.WritePropertyTag (CustomTag (0, TnefPropertyType.Object | TnefPropertyType.MultiValued)));
			Assert.Throws<ArgumentException> (() => properties.WritePropertyTag (new TnefPropertyTag (unchecked ((TnefPropertyId) 0x8001), TnefPropertyType.Long)));
			Assert.Throws<ArgumentException> (() => properties.WritePropertyTag (new TnefNameId (PropertySet, 1), TnefPropertyType.Unspecified));

			// Type mismatches.
			properties.WritePropertyTag (TnefPropertyTag.SubjectW);
			Assert.Throws<InvalidOperationException> (() => properties.WriteValue (1));
			Assert.Throws<InvalidOperationException> (() => properties.WriteValue (new byte[1]));
			Assert.Throws<InvalidOperationException> (() => properties.OpenValueStream ());
			Assert.Throws<InvalidOperationException> (() => properties.OpenRtfCompressedStream ());
			Assert.Throws<InvalidOperationException> (() => properties.OpenEmbeddedMessage ());
			Assert.Throws<ArgumentNullException> (() => properties.WriteValue ((string) null!));
			properties.WriteValue ("subject");

			// A single-valued property has exactly one value.
			Assert.Throws<InvalidOperationException> (() => properties.WriteValue ("again"));

			properties.WritePropertyTag (TnefPropertyTag.AttachMethod);
			Assert.Throws<InvalidOperationException> (() => properties.WritePropertyTag (TnefPropertyTag.SubjectW));
			properties.WriteValue (1);

			// Value range checks.
			properties.WritePropertyTag (CustomTag (1, TnefPropertyType.Currency));
			Assert.Throws<ArgumentOutOfRangeException> (() => properties.WriteValue (decimal.MaxValue));
			properties.WriteValue (1m);

			properties.WritePropertyTag (CustomTag (2, TnefPropertyType.SysTime));
			Assert.Throws<ArgumentOutOfRangeException> (() => properties.WriteValue (new DateTime (1600, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
			properties.WriteValue (DateTime.UtcNow);

			properties.WritePropertyTag (TnefPropertyTag.AttachDataObj);
			Assert.Throws<ArgumentException> (() => properties.WriteValue (new byte[15]));
			Assert.Throws<ArgumentNullException> (() => properties.WriteValue ((byte[]) null!));
			Assert.Throws<ArgumentNullException> (() => properties.WriteValue (null!, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => properties.WriteValue (new byte[16], 1, 16));
			properties.WriteValue (new byte[16]);
		}

		[Test]
		public void TestPropertyWriterFaultsWriter ()
		{
			using var output = new MemoryStream ();

			using (var writer = new TnefWriter (output, leaveOpen: true)) {
				writer.WriteAttribute (TnefAttributeTag.Subject, "x");

				var properties = writer.OpenPropertyWriter (TnefAttributeTag.MapiProperties);

				// A single-valued property without a value.
				properties.WritePropertyTag (TnefPropertyTag.SubjectW);
				properties.Dispose ();

				Assert.Throws<ObjectDisposedException> (() => properties.WritePropertyTag (TnefPropertyTag.SubjectW));
				Assert.Throws<InvalidOperationException> (() => writer.WriteAttribute (TnefAttributeTag.AttachRenderData, RenderData));
				Assert.ThrowsAsync<InvalidOperationException> (() => writer.WriteAttributeAsync (TnefAttributeTag.AttachRenderData, RenderData));
				Assert.Throws<InvalidOperationException> (() => writer.Flush ());
			}

			// The faulted attribute is discarded but the attributes before it are written.
			var tnef = output.ToArray ();

			AssertConformant (tnef);
			Assert.That (ReadAttributes (tnef).Skip (2).Select (a => a.Tag), Is.EqualTo (new[] { TnefAttributeTag.Subject }));
		}

		[Test]
		public void TestValueStreamMustBeDisposed ()
		{
			using var output = new MemoryStream ();
			using var writer = new TnefWriter (output);
			var properties = writer.OpenPropertyWriter (TnefAttributeTag.MapiProperties);

			properties.WritePropertyTag (TnefPropertyTag.RtfCompressed);

			var stream = properties.OpenValueStream ();

			Assert.Throws<InvalidOperationException> (() => properties.WritePropertyTag (TnefPropertyTag.SubjectW));
			Assert.Throws<InvalidOperationException> (() => properties.WriteValue (new byte[1]));
			Assert.Throws<InvalidOperationException> (() => properties.OpenValueStream ());

			properties.Dispose ();
			stream.Dispose ();

			Assert.Throws<InvalidOperationException> (() => writer.Flush ());
		}

		[Test]
		public void TestMultiValuedProperties ()
		{
			var tnef = Write (writer => {
				using var properties = writer.OpenPropertyWriter (TnefAttributeTag.MapiProperties);

				properties.WritePropertyTag (CustomTag (0, TnefPropertyType.Binary | TnefPropertyType.MultiValued));

				using (var stream = properties.OpenValueStream ())
					stream.Write (new byte[] { 1, 2, 3, 4, 5 }, 0, 5);

				properties.WriteValue (new byte[] { 6 });

				// An empty multi-valued property.
				properties.WritePropertyTag (CustomTag (1, TnefPropertyType.Long | TnefPropertyType.MultiValued));
			});

			AssertConformant (tnef);

			var set = ReadMessageProperties (tnef);

			Assert.That (set.TryGetValue (CustomTag (0, TnefPropertyType.Binary | TnefPropertyType.MultiValued), out var binary), Is.True);
			Assert.That (binary.Value, Is.EqualTo (new byte[][] { new byte[] { 1, 2, 3, 4, 5 }, new byte[] { 6 } }));
			Assert.That (set.TryGetValue (CustomTag (1, TnefPropertyType.Long | TnefPropertyType.MultiValued), out var empty), Is.True);
			Assert.That (empty.Count, Is.EqualTo (0));
		}

		[Test]
		public void TestWritePropertyValidation ()
		{
			var original = Write (writer => {
				using var properties = writer.OpenPropertyWriter (TnefAttributeTag.MapiProperties);

				properties.WritePropertyTag (TnefPropertyTag.SubjectW);
				properties.WriteValue ("x");
			});

			var property = ReadMessageProperties (original)[0];

			using var output = new MemoryStream ();
			using var writer = new TnefWriter (output);
			using var recipients = writer.OpenPropertyWriter (TnefAttributeTag.RecipientTable);

			Assert.Throws<InvalidOperationException> (() => recipients.WriteProperty (property));

			recipients.BeginRow ();
			recipients.WriteProperty (property);
			Assert.That (recipients.PropertyCount, Is.EqualTo (1));

			Assert.Throws<ArgumentException> (() => recipients.WriteProperty (default));
		}

		#endregion

		#region Attachments, embedded messages and RTF

		[Test]
		public void TestAttachments ()
		{
			var content = Encoding.ASCII.GetBytes ("attachment content");

			var tnef = Write (writer => {
				writer.WriteAttribute (TnefAttributeTag.MessageClass, "IPM.Note");

				using (var properties = writer.OpenPropertyWriter (TnefAttributeTag.MapiProperties)) {
					properties.WritePropertyTag (TnefPropertyTag.SubjectW);
					properties.WriteValue ("With attachments");
				}

				for (int i = 0; i < 3; i++) {
					writer.WriteAttribute (TnefAttributeTag.AttachRenderData, RenderData);
					writer.WriteAttribute (TnefAttributeTag.AttachTitle, $"file{i}.txt");

					using var properties = writer.OpenPropertyWriter (TnefAttributeTag.Attachment);

					properties.WritePropertyTag (TnefPropertyTag.AttachMethod);
					properties.WriteValue ((int) TnefAttachMethod.ByValue);
					properties.WritePropertyTag (TnefPropertyTag.AttachLongFilenameW);
					properties.WriteValue ($"long file name {i}.txt");
					properties.WritePropertyTag (TnefPropertyTag.AttachMimeTagA);
					properties.WriteValue ("text/plain");
					properties.WritePropertyTag (TnefPropertyTag.AttachDataBin);

					using var stream = properties.OpenValueStream ();

					stream.Write (content, 0, content.Length);
					stream.WriteByte ((byte) ('0' + i));
				}
			});

			AssertConformant (tnef);

			var message = TnefMessage.Load (new MemoryStream (tnef, false));

			Assert.That (message.MessageClass, Is.EqualTo ("IPM.Note"));
			Assert.That (message.Subject, Is.EqualTo ("With attachments"));
			Assert.That (message.Attachments, Has.Count.EqualTo (3));

			for (int i = 0; i < 3; i++) {
				var attachment = message.Attachments[i];

				Assert.That (attachment.FileName, Is.EqualTo ($"long file name {i}.txt"));
				Assert.That (attachment.MimeType, Is.EqualTo ("text/plain"));
				Assert.That (attachment.Method, Is.EqualTo (TnefAttachMethod.ByValue));

				using var stream = attachment.OpenRead ();
				using var memory = new MemoryStream ();

				stream.CopyTo (memory);

				Assert.That (memory.ToArray (), Is.EqualTo (content.Concat (new[] { (byte) ('0' + i) }).ToArray ()));
			}
		}

		[Test]
		public void TestEmbeddedMessages ()
		{
			var tnef = Write (writer => {
				writer.WriteAttribute (TnefAttributeTag.Subject, "outer");
				writer.WriteAttribute (TnefAttributeTag.AttachRenderData, RenderData);

				using var properties = writer.OpenPropertyWriter (TnefAttributeTag.Attachment);

				properties.WritePropertyTag (TnefPropertyTag.AttachMethod);
				properties.WriteValue ((int) TnefAttachMethod.EmbeddedMessage);
				properties.WritePropertyTag (TnefPropertyTag.AttachDataObj);

				using (var embedded = properties.OpenEmbeddedMessage (1251)) {
					Assert.That (embedded.Codepage, Is.EqualTo (1251));

					// Writing to the outer property writer is blocked until the embedded message is complete.
					Assert.Throws<InvalidOperationException> (() => properties.WritePropertyTag (TnefPropertyTag.SubjectW));

					embedded.WriteAttribute (TnefAttributeTag.MessageClass, "IPM.Note");
					embedded.WriteAttribute (TnefAttributeTag.Subject, "\u0438\u043d\u043d\u0435\u0440");

					using (var inner = embedded.OpenPropertyWriter (TnefAttributeTag.MapiProperties)) {
						inner.WritePropertyTag (TnefPropertyTag.SubjectA);
						inner.WriteValue ("\u0438\u043d\u043d\u0435\u0440");
					}

					embedded.WriteAttribute (TnefAttributeTag.AttachRenderData, RenderData);

					using var innerAttachment = embedded.OpenPropertyWriter (TnefAttributeTag.Attachment);

					innerAttachment.WritePropertyTag (TnefPropertyTag.AttachMethod);
					innerAttachment.WriteValue ((int) TnefAttachMethod.EmbeddedMessage);
					innerAttachment.WritePropertyTag (TnefPropertyTag.AttachDataObj);

					using var nested = innerAttachment.OpenEmbeddedMessage ();

					Assert.That (nested.Codepage, Is.EqualTo (1251), "inherits the codepage");
					nested.WriteAttribute (TnefAttributeTag.Subject, "nested");
				}

				properties.WritePropertyTag (TnefPropertyTag.AttachLongFilenameW);
				properties.WriteValue ("inner.msg");
			});

			AssertConformant (tnef);

			var message = TnefMessage.Load (new MemoryStream (tnef, false));
			var attachment = message.Attachments.Single ();

			Assert.That (attachment.IsEmbeddedMessage, Is.True);
			Assert.That (attachment.FileName, Is.EqualTo ("inner.msg"));

			var inner = attachment.LoadEmbeddedMessage ();

			Assert.That (inner.Codepage, Is.EqualTo (1251));
			Assert.That (inner.MessageClass, Is.EqualTo ("IPM.Note"));
			Assert.That (inner.Subject, Is.EqualTo ("\u0438\u043d\u043d\u0435\u0440"));

			var nested = inner.Attachments.Single ().LoadEmbeddedMessage ();

			Assert.That (nested.Subject, Is.EqualTo ("nested"));
		}

		[Test]
		public void TestEmbeddedMessageErrorFaultsParents ()
		{
			using var output = new MemoryStream ();
			using var writer = new TnefWriter (output);

			writer.WriteAttribute (TnefAttributeTag.AttachRenderData, RenderData);

			var properties = writer.OpenPropertyWriter (TnefAttributeTag.Attachment);

			properties.WritePropertyTag (TnefPropertyTag.AttachDataObj);

			var embedded = properties.OpenEmbeddedMessage ();
			var inner = embedded.OpenPropertyWriter (TnefAttributeTag.MapiProperties);

			inner.WritePropertyTag (TnefPropertyTag.SubjectW);
			inner.Dispose ();
			embedded.Dispose ();

			Assert.Throws<InvalidOperationException> (() => properties.WritePropertyTag (TnefPropertyTag.AttachMethod));

			properties.Dispose ();

			Assert.Throws<InvalidOperationException> (() => writer.Flush ());
		}

		[Test]
		public void TestEmbeddedMessageInvalidCodepage ()
		{
			using var output = new MemoryStream ();
			using var writer = new TnefWriter (output);

			writer.WriteAttribute (TnefAttributeTag.AttachRenderData, RenderData);

			using var properties = writer.OpenPropertyWriter (TnefAttributeTag.Attachment);

			properties.WritePropertyTag (TnefPropertyTag.AttachDataObj);

			Assert.Throws<ArgumentOutOfRangeException> (() => properties.OpenEmbeddedMessage (1200));

			// The property writer is still usable.
			using (var embedded = properties.OpenEmbeddedMessage ()) {
			}

			Assert.That (properties.PropertyCount, Is.EqualTo (1));
		}

		[TestCase (true)]
		[TestCase (false)]
		public void TestRtfCompressedBody (bool compress)
		{
			var builder = new StringBuilder ("{\\rtf1\\ansi\\ansicpg1252\\deff0{\\fonttbl{\\f0 Arial;}}");

			for (int i = 0; i < 500; i++)
				builder.Append ("\\pard Some text that repeats.\\par\r\n");

			builder.Append ('}');

			var rtf = builder.ToString ();

			var tnef = Write (writer => {
				using var properties = writer.OpenPropertyWriter (TnefAttributeTag.MapiProperties);

				properties.WritePropertyTag (TnefPropertyTag.RtfCompressed);

				using var stream = properties.OpenRtfCompressedStream (compress);
				var bytes = Encoding.ASCII.GetBytes (rtf);

				stream.Write (bytes, 0, bytes.Length);
			});

			AssertConformant (tnef);

			if (compress)
				Assert.That (tnef.Length, Is.LessThan (rtf.Length / 4));

			var message = TnefMessage.Load (new MemoryStream (tnef, false));

			Assert.That (message.RtfBody, Is.Not.Null);
			Assert.That (message.RtfBody!.GetText (), Is.EqualTo (rtf));
		}

		[Test]
		public void TestRtfCompressedMultiValued ()
		{
			var tnef = Write (writer => {
				using var properties = writer.OpenPropertyWriter (TnefAttributeTag.MapiProperties);

				properties.WritePropertyTag (CustomTag (0, TnefPropertyType.Binary | TnefPropertyType.MultiValued));

				for (int i = 0; i < 2; i++) {
					using var stream = properties.OpenRtfCompressedStream ();
					var bytes = Encoding.ASCII.GetBytes ("{\\rtf1 value " + i + "}");

					stream.Write (bytes, 0, bytes.Length);
				}
			});

			AssertConformant (tnef);

			var values = (byte[][]) ReadMessageProperties (tnef)[0].Value!;

			Assert.That (values, Has.Length.EqualTo (2));

			for (int i = 0; i < 2; i++) {
				var filter = new RtfCompressedToRtf ();
				var output = filter.Flush (values[i], 0, values[i].Length, out int index, out int length);

				Assert.That (filter.IsValidCrc32, Is.True);
				Assert.That (Encoding.ASCII.GetString (output, index, length), Is.EqualTo ("{\\rtf1 value " + i + "}"));
			}
		}

		#endregion

		#region Corpus

		static IEnumerable<string> CorpusFiles ()
		{
			var directory = Path.Combine (TestHelper.ProjectDir, "TestData", "tnef");

			return Directory.GetFiles (directory, "*.tnef").Select (Path.GetFileName)!;
		}

		static void CopyProperties (TnefPropertyReader source, TnefPropertyWriter target, bool table)
		{
			if (table) {
				while (source.ReadNextRow ()) {
					target.BeginRow ();

					while (source.ReadNextProperty ())
						target.WriteProperty (source.ReadProperty ());
				}
			} else {
				while (source.ReadNextProperty ())
					target.WriteProperty (source.ReadProperty ());
			}
		}

		static byte[] ReEmit (byte[] tnef)
		{
			using var reader = new TnefReader (new MemoryStream (tnef, false));
			using var output = new MemoryStream ();
			TnefWriter writer = null;

			try {
				while (reader.Read ()) {
					switch (reader.Tag) {
					case TnefAttributeTag.TnefVersion:
					case TnefAttributeTag.OemCodepage:
					case TnefAttributeTag.Null:
						continue;
					}

					// Note: Unknown attributes (e.g. in damaged corpus files) cannot be written.
					if (!Enum.IsDefined (typeof (TnefAttributeTag), reader.Tag))
						continue;

					// Note: The codepage is known once the OemCodepage attribute (if any) has been read.
					writer ??= new TnefWriter (output, reader.Codepage, reader.LegacyKey, true);

					switch (reader.Tag) {
					case TnefAttributeTag.MapiProperties:
					case TnefAttributeTag.Attachment:
					case TnefAttributeTag.RecipientTable:
						using (var properties = writer.OpenPropertyWriter (reader.Tag))
							CopyProperties (reader.GetPropertyReader (), properties, reader.Tag == TnefAttributeTag.RecipientTable);
						break;
					default:
						writer.WriteAttribute (reader.Tag, reader.ReadValueAsBytes ());
						break;
					}
				}

				writer ??= new TnefWriter (output, reader.Codepage, reader.LegacyKey, true);
				writer.Flush ();
			} finally {
				writer?.Dispose ();
			}

			return output.ToArray ();
		}

		static void AssertEquivalent (TnefPropertySet expected, TnefPropertySet actual, string context)
		{
			Assert.That (actual.Count, Is.EqualTo (expected.Count), $"{context}: property count");

			for (int i = 0; i < expected.Count; i++) {
				var e = expected[i];
				var a = actual[i];

				Assert.That (a.Tag.TnefType, Is.EqualTo (e.Tag.TnefType), $"{context}: {e.Tag} type");
				Assert.That (a.Name, Is.EqualTo (e.Name), $"{context}: {e.Tag} name");

				if (!e.Tag.IsNamed)
					Assert.That (a.Tag, Is.EqualTo (e.Tag), $"{context}: tag");

				Assert.That (a.Value, Is.EqualTo (e.Value), $"{context}: {e.Tag} value");
			}
		}

		static void AssertEquivalent (TnefMessage expected, TnefMessage actual, string context)
		{
			Assert.That (actual.Codepage, Is.EqualTo (expected.Codepage), $"{context}: codepage");
			Assert.That (actual.MessageClass, Is.EqualTo (expected.MessageClass), $"{context}: message class");
			AssertEquivalent (expected.Properties, actual.Properties, context);

			Assert.That (actual.Recipients.Count, Is.EqualTo (expected.Recipients.Count), $"{context}: recipients");
			for (int i = 0; i < expected.Recipients.Count; i++)
				AssertEquivalent (expected.Recipients[i].Properties, actual.Recipients[i].Properties, $"{context}: recipient {i}");

			Assert.That (actual.Attachments.Count, Is.EqualTo (expected.Attachments.Count), $"{context}: attachments");
			for (int i = 0; i < expected.Attachments.Count; i++) {
				var e = expected.Attachments[i];
				var a = actual.Attachments[i];

				Assert.That (a.FileName, Is.EqualTo (e.FileName), $"{context}: attachment {i} file name");
				Assert.That (a.IsEmbeddedMessage, Is.EqualTo (e.IsEmbeddedMessage), $"{context}: attachment {i} embedded");
				Assert.That (a.Length, Is.EqualTo (e.Length), $"{context}: attachment {i} length");

				if (e.IsEmbeddedMessage) {
					AssertEquivalent (e.LoadEmbeddedMessage (), a.LoadEmbeddedMessage (), $"{context}/{i}");
				} else if (e.HasContent) {
					using var es = e.OpenRead ();
					using var as_ = a.OpenRead ();
					using var em = new MemoryStream ();
					using var am = new MemoryStream ();

					es.CopyTo (em);
					as_.CopyTo (am);

					Assert.That (am.ToArray (), Is.EqualTo (em.ToArray ()), $"{context}: attachment {i} content");
				}
			}
		}

		[TestCaseSource (nameof (CorpusFiles))]
		public void TestCorpusReEmit (string fileName)
		{
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", "tnef", fileName);
			var original = File.ReadAllBytes (path);
			var copy = ReEmit (original);

			var expected = TnefMessage.Load (new MemoryStream (original, false));
			var actual = TnefMessage.Load (new MemoryStream (copy, false));

			AssertEquivalent (expected, actual, fileName);

			// Note: The writer is deterministic, so re-emitting the output must reproduce it exactly.
			Assert.That (ReEmit (copy), Is.EqualTo (copy), "idempotent");
		}

		#endregion
	}
}

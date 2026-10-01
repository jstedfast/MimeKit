//
// TnefReaderRobustnessTests.cs
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

using MimeKit.Tnef;

namespace UnitTests.Tnef {
	[TestFixture]
	public class TnefReaderRobustnessTests
	{
		[Test]
		public void TestAttributeLengthOverflowDoesNotThrow ()
		{
			// AttributeRawValueStreamOffset + AttributeRawValueLength must not be allowed to
			// produce a negative "bytes remaining" count, which would be handed straight to
			// Buffer.BlockCopy().
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachData, new byte[] { 1, 2, 3, 4 }, int.MaxValue);

			using (var stream = builder.ToStream ()) {
				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					Assert.That (reader.ReadNextAttribute (), Is.True, "ReadNextAttribute");

					var buffer = new byte[1024];

					Assert.DoesNotThrow (() => {
						while (reader.ReadAttributeRawValue (buffer, 0, buffer.Length) > 0)
							;
					});

					Assert.That (reader.ComplianceStatus & TnefComplianceStatus.StreamTruncated, Is.EqualTo (TnefComplianceStatus.StreamTruncated), "ComplianceStatus");
				}
			}
		}

		[Test]
		public void TestInvalidSignatureBehavesAsEndOfStream ()
		{
			var builder = new TnefBuilder (signature: 0x12345678);

			builder.WriteTnefVersion ();
			builder.WriteMessageClass ("IPM.Note");

			using (var stream = builder.ToStream ()) {
				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					Assert.That (reader.ComplianceStatus & TnefComplianceStatus.InvalidTnefSignature, Is.EqualTo (TnefComplianceStatus.InvalidTnefSignature), "ComplianceStatus");
					Assert.That (reader.ReadNextAttribute (), Is.False, "ReadNextAttribute");
					Assert.That (reader.ReadNextAttribute (), Is.False, "ReadNextAttribute (again)");
				}
			}
		}

		[Test]
		public void TestInvalidSignatureThrowsInStrictMode ()
		{
			var builder = new TnefBuilder (signature: 0x12345678);

			builder.WriteTnefVersion ();

			using (var stream = builder.ToStream ()) {
				var ex = Assert.Throws<TnefException> (() => new TnefReader (stream, 0, TnefComplianceMode.Strict));

				Assert.That (ex!.Error, Is.EqualTo (TnefComplianceStatus.InvalidTnefSignature));
			}
		}

		[TestCase (TnefAttributeTag.OemCodepage)]
		[TestCase (TnefAttributeTag.TnefVersion)]
		public void TestTruncatedPeekedAttributeDoesNotThrow (TnefAttributeTag tag)
		{
			// CheckAttributeTag() peeks at the first 4 bytes of the value for attOemCodepage and
			// attTnefVersion, but was invoked outside of the EndOfStreamException handler.
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Message, tag, TnefBuilder.Int32Payload (1252));

			// 6 bytes of header + 1 byte level + 4 byte tag + 4 byte length = 15 bytes.
			using (var stream = builder.ToStream (15)) {
				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					Assert.DoesNotThrow (() => {
						while (reader.ReadNextAttribute ())
							;
					});

					Assert.That (reader.ComplianceStatus & TnefComplianceStatus.StreamTruncated, Is.EqualTo (TnefComplianceStatus.StreamTruncated), "ComplianceStatus");
				}
			}
		}

		[TestCase (TnefAttributeTag.OemCodepage)]
		[TestCase (TnefAttributeTag.TnefVersion)]
		public void TestTruncatedPeekedAttributeThrowsTnefExceptionInStrictMode (TnefAttributeTag tag)
		{
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Message, tag, TnefBuilder.Int32Payload (1252));

			using (var stream = builder.ToStream (15)) {
				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Strict)) {
					Assert.Throws<TnefException> (() => {
						while (reader.ReadNextAttribute ())
							;
					});
				}
			}
		}

		[Test]
		public void TestShortOemCodepageAttributeDoesNotReadPastItsValue ()
		{
			// A 2-byte attOemCodepage value must not cause us to peek at the first 2 bytes of the
			// *next* attribute in order to make up a 32-bit codepage.
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.OemCodepage, new byte[] { 0xE4, 0x04 });
			builder.WriteMessageClass ("IPM.Note");

			using (var stream = builder.ToStream ()) {
				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					while (reader.ReadNextAttribute ())
						;

					Assert.That (reader.ComplianceStatus & TnefComplianceStatus.InvalidAttributeValue, Is.EqualTo (TnefComplianceStatus.InvalidAttributeValue), "ComplianceStatus");
				}
			}
		}

		[Test]
		public void TestShortTnefVersionAttributeDoesNotReadPastItsValue ()
		{
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.TnefVersion, new byte[] { 0x00, 0x00 });
			builder.WriteMessageClass ("IPM.Note");

			using (var stream = builder.ToStream ()) {
				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					while (reader.ReadNextAttribute ())
						;

					Assert.That (reader.ComplianceStatus & TnefComplianceStatus.InvalidAttributeValue, Is.EqualTo (TnefComplianceStatus.InvalidAttributeValue), "ComplianceStatus");
					Assert.That (reader.TnefVersion, Is.EqualTo (0), "TnefVersion");
				}
			}
		}
	}
}

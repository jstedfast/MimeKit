//
// TnefNestingTests.cs
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
	public class TnefNestingTests
	{
		static readonly Guid IID_IMessage = new Guid ("00020D0B-0000-0000-C000-000000000046");

		static readonly TnefPropertyTag AttachMethodTag = new TnefPropertyTag (TnefPropertyId.AttachMethod, TnefPropertyType.Long);
		static readonly TnefPropertyTag AttachDataTag = new TnefPropertyTag (TnefPropertyId.AttachData, TnefPropertyType.Object);

		// Builds a TNEF stream containing a single attachment whose PR_ATTACH_DATA_OBJ property is
		// an embedded TNEF message.
		static MemoryStream BuildEmbeddedMessage (byte[] embedded)
		{
			var value = new byte[16 + embedded.Length];

			IID_IMessage.ToByteArray ().CopyTo (value, 0);
			embedded.CopyTo (value, 16);

			int padding = (4 - (value.Length & 3)) & 3;
			var payload = new byte[24 + value.Length + padding];
			int index = 0;

			WriteInt32 (payload, ref index, 2);                                       // property count
			WriteTag (payload, ref index, AttachMethodTag);                           // PR_ATTACH_METHOD
			WriteInt32 (payload, ref index, (int) TnefAttachMethod.EmbeddedMessage);
			WriteTag (payload, ref index, AttachDataTag);                             // PR_ATTACH_DATA_OBJ
			WriteInt32 (payload, ref index, 1);                                       // value count
			WriteInt32 (payload, ref index, value.Length);                            // value length
			value.CopyTo (payload, index);

			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData, new byte[] { 0, 0 });
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.Attachment, payload);

			return builder.ToStream ();
		}

		static void WriteInt32 (byte[] buffer, ref int index, int value)
		{
			BitConverter.GetBytes (value).CopyTo (buffer, index);
			index += 4;
		}

		// Note: on the wire, a MAPI property tag is a WORD type followed by a WORD id, which is the
		// opposite order of the 32-bit integer representation of a TnefPropertyTag.
		static void WriteTag (byte[] buffer, ref int index, TnefPropertyTag tag)
		{
			BitConverter.GetBytes ((short) tag.TnefType).CopyTo (buffer, index);
			BitConverter.GetBytes ((short) tag.Id).CopyTo (buffer, index + 2);
			index += 4;
		}

		static TnefReader GetEmbeddedMessageReader (TnefReader reader)
		{
			while (reader.ReadNextAttribute ()) {
				if (reader.AttributeTag != TnefAttributeTag.Attachment)
					continue;

				var prop = reader.TnefPropertyReader;

				while (prop.ReadNextProperty ()) {
					if (prop.IsEmbeddedMessage)
						return prop.GetEmbeddedMessageReader ();
				}
			}

			throw new InvalidOperationException ("Failed to locate the embedded message.");
		}

		[Test]
		public void TestDefaultMaxNestingDepth ()
		{
			var inner = new TnefBuilder ();

			inner.WriteTnefVersion ();

			using (var stream = BuildEmbeddedMessage (inner.ToArray ())) {
				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					Assert.That (reader.MaxNestingDepth, Is.EqualTo (TnefReader.DefaultMaxNestingDepth));

					using (var embedded = GetEmbeddedMessageReader (reader)) {
						Assert.That (embedded.MaxNestingDepth, Is.EqualTo (TnefReader.DefaultMaxNestingDepth), "MaxNestingDepth");
						Assert.That (embedded.ReadNextAttribute (), Is.True, "ReadNextAttribute");
						Assert.That (reader.ComplianceStatus & TnefComplianceStatus.NestingTooDeep, Is.EqualTo (TnefComplianceStatus.Compliant), "ComplianceStatus");
					}
				}
			}
		}

		[Test]
		public void TestMaxNestingDepthExceeded ()
		{
			var inner = new TnefBuilder ();

			inner.WriteTnefVersion ();

			using (var stream = BuildEmbeddedMessage (inner.ToArray ())) {
				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					reader.MaxNestingDepth = 0;

					using (var embedded = GetEmbeddedMessageReader (reader)) {
						Assert.That (reader.ComplianceStatus & TnefComplianceStatus.NestingTooDeep, Is.EqualTo (TnefComplianceStatus.NestingTooDeep), "ComplianceStatus");
						Assert.That (embedded.ReadNextAttribute (), Is.False, "ReadNextAttribute");
					}
				}
			}
		}

		[Test]
		public void TestMaxNestingDepthCannotBeNegative ()
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();

			using (var stream = builder.ToStream ()) {
				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					Assert.Throws<ArgumentOutOfRangeException> (() => reader.MaxNestingDepth = -1);
					Assert.That (reader.MaxNestingDepth, Is.EqualTo (TnefReader.DefaultMaxNestingDepth));
				}
			}
		}
	}
}

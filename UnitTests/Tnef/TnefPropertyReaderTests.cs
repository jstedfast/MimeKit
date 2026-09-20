//
// TnefPropertyReaderTests.cs
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
using MimeKit.Tnef;

namespace UnitTests.Tnef {
	[TestFixture]
	public class TnefPropertyReaderTests
	{
		const int TnefSignature = 0x223e9f78;

		static void WriteInt16 (Stream stream, short value)
		{
			stream.WriteByte ((byte) (value & 0xFF));
			stream.WriteByte ((byte) ((value >> 8) & 0xFF));
		}

		static void WriteInt32 (Stream stream, int value)
		{
			stream.WriteByte ((byte) (value & 0xFF));
			stream.WriteByte ((byte) ((value >> 8) & 0xFF));
			stream.WriteByte ((byte) ((value >> 16) & 0xFF));
			stream.WriteByte ((byte) ((value >> 24) & 0xFF));
		}

		static short Checksum (byte[] data)
		{
			int sum = 0;

			for (int i = 0; i < data.Length; i++)
				sum = (sum + data[i]) & 0xFFFF;

			return (short) sum;
		}

		/// <summary>
		/// Builds a minimal TNEF stream containing a single attMAPIProps attribute with a single
		/// MAPI property whose value is prefixed by <paramref name="declaredValueLength"/>.
		/// </summary>
		/// <param name="declaredAttributeLength">
		/// When non-null, overrides the attribute's raw value length with an arbitrary (untrusted) value
		/// instead of the true payload length.
		/// </param>
		static byte[] CreateTnefStream (TnefPropertyId id, TnefPropertyType type, int declaredValueLength, byte[] data, int? declaredAttributeLength = null)
		{
			byte[] attrValue;

			using (var payload = new MemoryStream ()) {
				WriteInt32 (payload, 1);                       // property count
				WriteInt16 (payload, (short) type);            // property type
				WriteInt16 (payload, (short) id);              // property id
				WriteInt32 (payload, 1);                       // value count
				WriteInt32 (payload, declaredValueLength);     // <-- untrusted length prefix
				payload.Write (data, 0, data.Length);          // the (small) amount of real data

				attrValue = payload.ToArray ();
			}

			using (var stream = new MemoryStream ()) {
				WriteInt32 (stream, TnefSignature);
				WriteInt16 (stream, 0); // legacy attachment key

				stream.WriteByte ((byte) TnefAttributeLevel.Message);
				WriteInt32 (stream, (int) TnefAttributeTag.MapiProperties);
				WriteInt32 (stream, declaredAttributeLength ?? attrValue.Length); // <-- untrusted attribute length
				stream.Write (attrValue, 0, attrValue.Length);
				WriteInt16 (stream, Checksum (attrValue));

				return stream.ToArray ();
			}
		}

		static byte[] ReadFirstPropertyAsBytes (byte[] tnef, out TnefComplianceStatus status, out bool readProperty)
		{
			using (var reader = new TnefReader (new MemoryStream (tnef, false), 0, TnefComplianceMode.Loose)) {
				byte[] bytes = null;

				readProperty = false;

				while (reader.ReadNextAttribute ()) {
					var prop = reader.TnefPropertyReader;

					while (prop.ReadNextProperty ()) {
						readProperty = true;
						bytes = prop.ReadValueAsBytes ();
					}
				}

				status = reader.ComplianceStatus;

				return bytes;
			}
		}

		// A huge value-length prefix that cannot possibly fit inside the (small) enclosing attribute
		// must be rejected, and must never be used to size an allocation.
		[TestCase (int.MaxValue)]
		[TestCase (int.MaxValue - 1)]
		[TestCase (int.MaxValue - 2)]
		[TestCase (int.MaxValue - 3)]   // exercises padding arithmetic near the int boundary
		[TestCase (0x40000000)]
		[TestCase (1024 * 1024)]
		public void TestOversizedValueLengthIsRejected (int declaredLength)
		{
			var tnef = CreateTnefStream (TnefPropertyId.AttachData, TnefPropertyType.Binary, declaredLength, new byte[] { 1, 2, 3, 4 });

			Assert.That (tnef.Length, Is.LessThan (128), "the crafted TNEF stream should be tiny");

			byte[] bytes = null;
			TnefComplianceStatus status = TnefComplianceStatus.Compliant;
			bool readProperty = false;

			Assert.DoesNotThrow (() => bytes = ReadFirstPropertyAsBytes (tnef, out status, out readProperty));

			Assert.That (status.HasFlag (TnefComplianceStatus.InvalidPropertyLength), Is.True,
				"an oversized property length should be recorded as a compliance error");

			// Whatever happens, we must never have materialized a buffer larger than the input.
			if (bytes != null)
				Assert.That (bytes.Length, Is.LessThanOrEqualTo (tnef.Length), "allocated buffer is larger than the entire input stream");
		}

		// GetPaddedLength() rounds up to a multiple of 4; verify the arithmetic near int.MaxValue
		// does not overflow into a negative/small value that would slip past the bounds check.
		[TestCase (int.MaxValue)]
		[TestCase (int.MaxValue - 1)]
		[TestCase (int.MaxValue - 2)]
		[TestCase (int.MaxValue - 3)]
		public void TestPaddedLengthOverflowIsRejected (int declaredLength)
		{
			var tnef = CreateTnefStream (TnefPropertyId.AttachData, TnefPropertyType.Object, declaredLength, new byte[] { 1, 2, 3, 4 });

			TnefComplianceStatus status = TnefComplianceStatus.Compliant;
			byte[] bytes = null;
			bool readProperty = false;

			Assert.DoesNotThrow (() => bytes = ReadFirstPropertyAsBytes (tnef, out status, out readProperty));

			Assert.That (status, Is.Not.EqualTo (TnefComplianceStatus.Compliant), "overflowing length must not be treated as compliant");

			if (bytes != null)
				Assert.That (bytes.Length, Is.LessThanOrEqualTo (tnef.Length));
		}

		// A negative length prefix must be rejected rather than flowing into the padding arithmetic.
		[TestCase (-1)]
		[TestCase (int.MinValue)]
		[TestCase (-1024)]
		public void TestNegativeValueLengthIsRejected (int declaredLength)
		{
			var tnef = CreateTnefStream (TnefPropertyId.AttachData, TnefPropertyType.Binary, declaredLength, new byte[] { 1, 2, 3, 4 });

			TnefComplianceStatus status = TnefComplianceStatus.Compliant;
			byte[] bytes = null;
			bool readProperty = false;

			Assert.DoesNotThrow (() => bytes = ReadFirstPropertyAsBytes (tnef, out status, out readProperty));

			Assert.That (status.HasFlag (TnefComplianceStatus.InvalidPropertyLength), Is.True,
				"a negative property length should be recorded as a compliance error");

			if (bytes != null)
				Assert.That (bytes.Length, Is.Zero);
		}

		// String properties go through the same ReadByteArray() path via ReadValueAsString().
		[TestCase (TnefPropertyType.Unicode)]
		[TestCase (TnefPropertyType.String8)]
		public void TestOversizedStringLengthIsRejected (TnefPropertyType type)
		{
			var tnef = CreateTnefStream (TnefPropertyId.AttachTransportName, type, int.MaxValue, new byte[] { 0x41, 0x00, 0x42, 0x00 });

			Assert.DoesNotThrow (() => {
				using (var reader = new TnefReader (new MemoryStream (tnef, false), 0, TnefComplianceMode.Loose)) {
					while (reader.ReadNextAttribute ()) {
						var prop = reader.TnefPropertyReader;

						while (prop.ReadNextProperty ()) {
							var value = prop.ReadValueAsString ();

							Assert.That (value.Length, Is.LessThanOrEqualTo (tnef.Length));
						}
					}
				}
			});
		}

		// The attribute length itself is untrusted. A tiny stream that claims a ~2GB attribute must not
		// cause a ~2GB allocation, even though every individual property length "fits" inside it.
		[Test]
		public void TestOversizedAttributeLengthDoesNotAllocate ()
		{
			var tnef = CreateTnefStream (TnefPropertyId.AttachData, TnefPropertyType.Binary, 0x7FFFFF00, new byte[] { 1, 2, 3, 4 },
				declaredAttributeLength: 0x7FFFFFFF);

			Assert.That (tnef.Length, Is.LessThan (128), "the crafted TNEF stream should be tiny");

			byte[] bytes = null;
			TnefComplianceStatus status = TnefComplianceStatus.Compliant;
			bool readProperty = false;

			Assert.DoesNotThrow (() => bytes = ReadFirstPropertyAsBytes (tnef, out status, out readProperty));

			if (bytes != null) {
				Assert.That (bytes.Length, Is.LessThanOrEqualTo (tnef.Length),
					"a tiny stream must never produce a buffer sized from the declared attribute length");
			}

			Assert.That (status, Is.Not.EqualTo (TnefComplianceStatus.Compliant),
				"an attribute length that exceeds the stream should be recorded as a compliance error");
		}

		// End-to-end: the public entry points named in the report must survive a malicious winmail.dat.
		[Test]
		public void TestConvertToMessageWithOversizedLength ()
		{
			var tnef = CreateTnefStream (TnefPropertyId.AttachData, TnefPropertyType.Binary, int.MaxValue, new byte[] { 1, 2, 3, 4 });
			var part = new TnefPart { Content = new MimeContent (new MemoryStream (tnef, false)) };

			Assert.DoesNotThrow (() => {
				using (var message = part.ConvertToMessage ())
					Assert.That (message, Is.Not.Null);
			});
		}

		[Test]
		public void TestExtractAttachmentsWithOversizedLength ()
		{
			var tnef = CreateTnefStream (TnefPropertyId.AttachData, TnefPropertyType.Binary, int.MaxValue, new byte[] { 1, 2, 3, 4 });
			var part = new TnefPart { Content = new MimeContent (new MemoryStream (tnef, false)) };

			Assert.DoesNotThrow (() => {
				var attachments = part.ExtractAttachments ().ToList ();

				foreach (var attachment in attachments.OfType<MimePart> ()) {
					using (var content = attachment.Content.Open ())
						Assert.That (content.CanRead, Is.True);
				}
			});
		}
	}
}

//
// TnefStreamOffsetTests.cs
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
	public class TnefStreamOffsetTests
	{
		/// <summary>
		/// A forward-only stream that synthesizes a TNEF stream containing two very large
		/// attributes without ever allocating their payloads.
		/// </summary>
		class HugeTnefStream : Stream
		{
			readonly byte[] prologue;
			readonly long[] boundaries;
			long position;

			public HugeTnefStream (long firstPayloadLength, long secondPayloadLength)
			{
				// Note: the payloads are all zeros, so the 16-bit checksum of each attribute is
				// also zero and the stream remains fully compliant.
				using var memory = new MemoryStream ();
				using var writer = new BinaryWriter (memory);

				writer.Write (0x223E9F78);
				writer.Write ((short) 0);
				prologue = memory.ToArray ();

				var attribute = new byte[9];
				attribute[0] = (byte) TnefAttributeLevel.Message;
				BitConverter.GetBytes ((int) TnefAttributeTag.Owner).CopyTo (attribute, 1);

				AttributeHeader = attribute;
				FirstPayloadLength = firstPayloadLength;
				SecondPayloadLength = secondPayloadLength;

				// prologue | attr1 header | attr1 payload | attr1 checksum | attr2 header | attr2 payload | attr2 checksum
				boundaries = new long[7];
				boundaries[0] = prologue.Length;
				boundaries[1] = boundaries[0] + 9;
				boundaries[2] = boundaries[1] + firstPayloadLength;
				boundaries[3] = boundaries[2] + 2;
				boundaries[4] = boundaries[3] + 9;
				boundaries[5] = boundaries[4] + secondPayloadLength;
				boundaries[6] = boundaries[5] + 2;
			}

			byte[] AttributeHeader { get; }

			long FirstPayloadLength { get; }

			long SecondPayloadLength { get; }

			public long TotalLength {
				get { return boundaries[6]; }
			}

			byte ByteAt (long offset)
			{
				if (offset < boundaries[0])
					return prologue[offset];

				if (offset < boundaries[1]) {
					int index = (int) (offset - boundaries[0]);

					return index < 5 ? AttributeHeader[index] : BitConverter.GetBytes ((int) FirstPayloadLength)[index - 5];
				}

				if (offset < boundaries[3])
					return 0;

				if (offset < boundaries[4]) {
					int index = (int) (offset - boundaries[3]);

					return index < 5 ? AttributeHeader[index] : BitConverter.GetBytes ((int) SecondPayloadLength)[index - 5];
				}

				return 0;
			}

			public override bool CanRead {
				get { return true; }
			}

			public override bool CanSeek {
				get { return false; }
			}

			public override bool CanWrite {
				get { return false; }
			}

			public override long Length {
				get { return TotalLength; }
			}

			public override long Position {
				get { return position; }
				set { throw new NotSupportedException (); }
			}

			public override int Read (byte[] buffer, int offset, int count)
			{
				int n = (int) Math.Min (count, TotalLength - position);

				// Note: the payloads are all zeros, so only the small framing regions need to be
				// filled in byte by byte.
				Array.Clear (buffer, offset, n);

				for (int i = 0; i < n; i++) {
					long at = position + i;

					if (at < boundaries[1] || (at >= boundaries[2] && at < boundaries[4]) || at >= boundaries[5])
						buffer[offset + i] = ByteAt (at);
				}

				position += n;

				return n;
			}

			public override void Flush ()
			{
			}

			public override long Seek (long offset, SeekOrigin origin)
			{
				throw new NotSupportedException ();
			}

			public override void SetLength (long value)
			{
				throw new NotSupportedException ();
			}

			public override void Write (byte[] buffer, int offset, int count)
			{
				throw new NotSupportedException ();
			}
		}

		[Test]
		[Explicit ("Reads 2.6GB through the parser; too slow for the default test run.")]
		public void TestStreamOffsetDoesNotWrapPastTwoGigabytes ()
		{
			const long firstPayloadLength = 1600000000;
			const long secondPayloadLength = 1000000000;

			using var stream = new HugeTnefStream (firstPayloadLength, secondPayloadLength);
			using var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose);

			int attributes = 0;

			while (reader.ReadNextAttribute ()) {
				Assert.That (reader.StreamOffset, Is.GreaterThanOrEqualTo (0), "StreamOffset must never go negative");
				attributes++;
			}

			Assert.That (attributes, Is.EqualTo (2), "attribute count");
			Assert.That (reader.StreamOffset, Is.EqualTo (stream.TotalLength), "StreamOffset");
			Assert.That (reader.ComplianceStatus, Is.EqualTo (TnefComplianceStatus.Compliant), "ComplianceStatus");
		}
	}
}

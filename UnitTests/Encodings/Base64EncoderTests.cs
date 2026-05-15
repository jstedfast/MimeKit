//
// Base64EncoderTests.cs
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

using System.Buffers;
using System.Buffers.Text;

using MimeKit;
using MimeKit.Encodings;

namespace UnitTests.Encodings {
	[TestFixture]
	public class Base64EncoderTests : MimeEncoderTestsBase
	{
		static readonly bool DefaultHwAccel = Base64Encoder.EnableHardwareAcceleration;

		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentOutOfRangeException> (() => new Base64Encoder (0));

			AssertArgumentExceptions (new Base64Encoder ());
		}

		[Test]
		public void TestEncoding ()
		{
			var encoder = new Base64Encoder ();

			Assert.That (encoder.Encoding, Is.EqualTo (ContentEncoding.Base64));
		}

		[Test]
		public void TestClone ()
		{
			CloneAndAssert (new Base64Encoder ());
		}

		[Test]
		public void TestReset ()
		{
			ResetAndAssert (new Base64Encoder ());
		}

		[TestCase (true, 4096)]
		[TestCase (false, 4096)]
		[TestCase (true, 1024)]
		[TestCase (false, 1024)]
		[TestCase (true, 16)]
		[TestCase (false, 16)]
		[TestCase (true, 1)]
		[TestCase (false, 1)]
		public void TestEncode (bool enableHwAccel, int bufferSize)
		{
			Base64Encoder.EnableHardwareAcceleration = enableHwAccel;

			try {
				TestEncoder (new Base64Encoder (), "photo.jpg", photo, "photo.b64", bufferSize);
			} finally {
				Base64Encoder.EnableHardwareAcceleration = DefaultHwAccel;
			}
		}

		[TestCase (false)]
		[TestCase (true)]
		public void TestFlush (bool enableHwAccel)
		{
			Base64Encoder.EnableHardwareAcceleration = enableHwAccel;

			try {
				TestEncoderFlush (new Base64Encoder (), "photo.jpg", photo, "photo.b64");
			} finally {
				Base64Encoder.EnableHardwareAcceleration = DefaultHwAccel;
			}
		}

		[Test]
		public void TestSuperLongLineLengths ()
		{
			ReadOnlySpan<byte> loremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. "u8;
			const int MaxLineLength = 998;
			const int PayloadSize = 20649;
			const int ChunkSize = 4096;

			// 20,649 bytes of Lorem ipsum.
			var payload = new byte[PayloadSize];
			int payloadIndex = 0;

			while (payloadIndex < PayloadSize) {
				int chunk = Math.Min (loremIpsum.Length, PayloadSize - payloadIndex);
				loremIpsum.Slice (0, chunk).CopyTo (payload.AsSpan (payloadIndex, chunk));
				payloadIndex += chunk;
			}

			// Pass Base64Encoder 4 KB chunks.
			var encoder = new Base64Encoder (MaxLineLength, overrideMaxLineLengthLimits: true);
			var buffer = new byte[encoder.EstimateOutputLength (ChunkSize)];
			using var base64Stream = new MemoryStream ();
			int startIndex = 0;

			while (startIndex < payload.Length) {
				int length = Math.Min (ChunkSize, payload.Length - startIndex);
				int n = encoder.Encode (payload, startIndex, length, buffer);

				base64Stream.Write (buffer, 0, n);
				startIndex += length;
			}

			int flushed = encoder.Flush (payload, startIndex, 0, buffer);
			base64Stream.Write (buffer, 0, flushed);

			// Decode and compare.
			var utf8 = base64Stream.GetBuffer ().AsSpan (0, (int) base64Stream.Length);
			buffer = new byte[Base64.GetMaxDecodedFromUtf8Length (utf8.Length)];
			var result = Base64.DecodeFromUtf8 (utf8, buffer, out int bytesConsumed, out int bytesWritten, true);
			var decoded = buffer.AsSpan (0, bytesWritten);

			Assert.That (result, Is.EqualTo (OperationStatus.Done), "result");
			Assert.That (bytesConsumed, Is.EqualTo (utf8.Length), "bytesConsumed");
			Assert.That (bytesWritten, Is.EqualTo (payload.Length), "bytesWritten");
			Assert.That (decoded.SequenceEqual (payload), Is.True, "decoded");
		}
	}
}

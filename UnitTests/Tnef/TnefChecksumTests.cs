//
// TnefChecksumTests.cs
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
	public class TnefChecksumTests
	{
		// Note: these cover the scalar tail, the vectorized body, every alignment within a
		// 32-byte vector, and payloads that straddle the reader's 4KB read-ahead boundary.
		static readonly int[] PayloadLengths = {
			0, 1, 2, 3, 4, 5, 7, 8, 9, 15, 16, 17, 23, 24, 31, 32, 33, 47, 63, 64, 65, 95,
			127, 128, 129, 255, 256, 257, 1023, 1024, 1025, 4095, 4096, 4097, 4223, 4224,
			4225, 8191, 8192, 8193, 10000
		};

		static byte[] CreatePayload (int length, int seed)
		{
			var random = new Random (seed);
			var payload = new byte[length];

			random.NextBytes (payload);

			return payload;
		}

		[Test]
		public void TestAttributeChecksumAcceptsEveryPayloadLength ()
		{
			foreach (var length in PayloadLengths) {
				var payload = CreatePayload (length, length);
				var builder = new TnefBuilder ();
				var logger = new TestTnefComplianceLogger ();

				builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Owner, payload);

				using var stream = builder.ToStream ();
				using var reader = new TnefReader (stream) { ComplianceLogger = logger };

				Assert.That (reader.Read (), Is.True, $"Read (length = {length})");

				while (reader.Read ())
					;

				Assert.That (logger.Issues, Is.Empty, $"Issues (length = {length})");
			}
		}

		[Test]
		public async Task TestAttributeChecksumAcceptsEveryPayloadLengthAsync ()
		{
			foreach (var length in PayloadLengths) {
				var payload = CreatePayload (length, length);
				var builder = new TnefBuilder ();
				var logger = new TestTnefComplianceLogger ();

				builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Owner, payload);

				using var stream = builder.ToStream ();
				using var reader = new TnefReader (stream) { ComplianceLogger = logger };

				Assert.That (await reader.ReadAsync (), Is.True, $"ReadAsync (length = {length})");

				while (await reader.ReadAsync ())
					;

				Assert.That (logger.Issues, Is.Empty, $"Issues (length = {length})");
			}
		}

		[Test]
		public void TestAttributeChecksumAcceptsEveryPayloadLengthWhenValueIsRead ()
		{
			var buffer = new byte[73];

			foreach (var length in PayloadLengths) {
				var payload = CreatePayload (length, length);
				var builder = new TnefBuilder ();
				var logger = new TestTnefComplianceLogger ();

				builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Owner, payload);

				using var stream = builder.ToStream ();
				using var reader = new TnefReader (stream) { ComplianceLogger = logger };

				Assert.That (reader.Read (), Is.True, $"Read (length = {length})");

				// Note: reading in 73-byte chunks splits the payload at offsets that are not a
				// multiple of the vector width.
				int total = 0, n;

				using (var value = reader.OpenValueStream ()) {
					while ((n = value.Read (buffer, 0, buffer.Length)) > 0) {
						Assert.That (payload.Skip (total).Take (n), Is.EqualTo (buffer.Take (n)), $"content at {total} (length = {length})");
						total += n;
					}
				}

				Assert.That (total, Is.EqualTo (length), $"bytes read (length = {length})");

				while (reader.Read ())
					;

				Assert.That (logger.Issues, Is.Empty, $"Issues (length = {length})");
			}
		}

		[Test]
		public async Task TestAttributeChecksumAcceptsEveryPayloadLengthWhenValueIsReadAsync ()
		{
			var buffer = new byte[73];

			foreach (var length in PayloadLengths) {
				var payload = CreatePayload (length, length);
				var builder = new TnefBuilder ();
				var logger = new TestTnefComplianceLogger ();

				builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Owner, payload);

				using var stream = builder.ToStream ();
				using var reader = new TnefReader (stream) { ComplianceLogger = logger };

				Assert.That (await reader.ReadAsync (), Is.True, $"ReadAsync (length = {length})");

				int total = 0, n;

				using (var value = reader.OpenValueStream ()) {
					while ((n = await value.ReadAsync (buffer, 0, buffer.Length)) > 0) {
						Assert.That (payload.Skip (total).Take (n), Is.EqualTo (buffer.Take (n)), $"content at {total} (length = {length})");
						total += n;
					}
				}

				Assert.That (total, Is.EqualTo (length), $"bytes read (length = {length})");

				while (await reader.ReadAsync ())
					;

				Assert.That (logger.Issues, Is.Empty, $"Issues (length = {length})");
			}
		}

		[Test]
		public void TestAttributeChecksumDetectsCorruption ()
		{
			foreach (var length in PayloadLengths) {
				var payload = CreatePayload (length, length);
				var builder = new TnefBuilder ();
				var logger = new TestTnefComplianceLogger ();
				short checksum = 0;

				for (int i = 0; i < payload.Length; i++)
					checksum = (short) ((checksum + payload[i]) & 0xFFFF);

				builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Owner, payload, checksum: (short) (checksum + 1));

				using var stream = builder.ToStream ();
				using var reader = new TnefReader (stream) { ComplianceLogger = logger };

				while (reader.Read ())
					;

				Assert.That (logger.Issues.Select (issue => issue.Violation), Has.Member (TnefComplianceViolation.AttributeChecksumMismatch), $"Violation (length = {length})");
			}
		}

		[Test]
		public void TestAttributeChecksumWrapsModulo65536 ()
		{
			// Note: 0xFF * 1024 = 261120, which is 3 full wraps past 65536.
			var payload = new byte[1024];
			var logger = new TestTnefComplianceLogger ();

			for (int i = 0; i < payload.Length; i++)
				payload[i] = 0xFF;

			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Owner, payload);

			using var stream = builder.ToStream ();
			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			while (reader.Read ())
				;

			Assert.That (logger.Issues, Is.Empty, "Issues");
		}
	}
}

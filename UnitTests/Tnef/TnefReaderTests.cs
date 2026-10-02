//
// TnefReaderTests.cs
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
	public class TnefReaderTests
	{
		static readonly string DataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "tnef");

		const int ValidSignature = 0x223e9f78;

		// Writes a well-formed TNEF stream header (signature + legacy key).
		static void WriteHeader (MemoryStream stream, int signature = ValidSignature)
		{
			stream.Write (BitConverter.GetBytes (signature), 0, 4);
			stream.WriteByte (0);
			stream.WriteByte (0);
		}

		// Writes a raw attribute (level + tag + length + value), optionally omitting the checksum.
		static void WriteRawAttribute (MemoryStream stream, TnefAttributeLevel level, TnefAttributeTag tag, int length, byte[] value, bool checksum = true)
		{
			stream.WriteByte ((byte) level);
			stream.Write (BitConverter.GetBytes ((int) tag), 0, 4);
			stream.Write (BitConverter.GetBytes (length), 0, 4);
			stream.Write (value, 0, value.Length);

			if (checksum) {
				short sum = 0;

				for (int i = 0; i < value.Length; i++)
					sum = (short) ((sum + value[i]) & 0xFFFF);

				stream.Write (BitConverter.GetBytes (sum), 0, 2);
			}
		}

		static IEnumerable<TnefComplianceViolation> Violations (TestTnefComplianceLogger logger)
		{
			return logger.Issues.Select (issue => issue.Violation);
		}

		#region Argument validation

		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentNullException> (() => new TnefReader (null));

			var options = new TnefOptions ();

			Assert.Throws<ArgumentOutOfRangeException> (() => options.DefaultCodepage = -1);
			Assert.Throws<ArgumentOutOfRangeException> (() => options.MaxNestingDepth = -1);
			Assert.Throws<ArgumentOutOfRangeException> (() => options.MaxPropertyValueLength = -1);
			Assert.Throws<ArgumentOutOfRangeException> (() => options.MaxTotalDataBytes = -1);

			using var stream = File.OpenRead (Path.Combine (DataDir, "winmail.tnef"));
			using var reader = new TnefReader (stream);

			Assert.Throws<ArgumentOutOfRangeException> (() => reader.MaxComplianceIssuesPerViolation = -1);
		}

		#endregion

		#region Malformed headers and attributes

		[Test]
		public void TestInvalidSignature ()
		{
			var logger = new TestTnefComplianceLogger ();
			using var stream = new MemoryStream ();

			WriteHeader (stream, ValidSignature + 1);
			stream.Position = 0;

			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			Assert.That (reader.Read (), Is.False, "Read");
			Assert.That (Violations (logger), Has.Member (TnefComplianceViolation.InvalidSignature));
		}

		[Test]
		public async Task TestInvalidSignatureAsync ()
		{
			var logger = new TestTnefComplianceLogger ();
			using var stream = new MemoryStream ();

			WriteHeader (stream, ValidSignature + 1);
			stream.Position = 0;

			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			Assert.That (await reader.ReadAsync (), Is.False, "ReadAsync");
			Assert.That (Violations (logger), Has.Member (TnefComplianceViolation.InvalidSignature));
		}

		[Test]
		public void TestTruncatedHeader ()
		{
			var logger = new TestTnefComplianceLogger ();
			using var stream = new MemoryStream ();

			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			Assert.That (reader.Read (), Is.False, "Read");
			Assert.That (Violations (logger), Has.Member (TnefComplianceViolation.TruncatedStream));
		}

		[Test]
		public async Task TestTruncatedHeaderAsync ()
		{
			var logger = new TestTnefComplianceLogger ();
			using var stream = new MemoryStream ();

			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			Assert.That (await reader.ReadAsync (), Is.False, "ReadAsync");
			Assert.That (Violations (logger), Has.Member (TnefComplianceViolation.TruncatedStream));
		}

		[Test]
		public void TestTruncatedHeaderAfterSignature ()
		{
			var logger = new TestTnefComplianceLogger ();
			using var stream = new MemoryStream ();

			stream.Write (BitConverter.GetBytes (ValidSignature), 0, 4);
			stream.WriteByte (0);
			stream.Position = 0;

			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			Assert.That (reader.Read (), Is.False, "Read");
			Assert.That (Violations (logger), Has.Member (TnefComplianceViolation.TruncatedStream));
		}

		[Test]
		public async Task TestTruncatedHeaderAfterSignatureAsync ()
		{
			var logger = new TestTnefComplianceLogger ();
			using var stream = new MemoryStream ();

			stream.Write (BitConverter.GetBytes (ValidSignature), 0, 4);
			stream.WriteByte (0);
			stream.Position = 0;

			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			Assert.That (await reader.ReadAsync (), Is.False, "ReadAsync");
			Assert.That (Violations (logger), Has.Member (TnefComplianceViolation.TruncatedStream));
		}

		[Test]
		public void TestInvalidOemCodepage ()
		{
			var logger = new TestTnefComplianceLogger ();
			var builder = new TnefBuilder ();

			builder.WriteOemCodepage (1);

			using var reader = new TnefReader (builder.ToStream ()) { ComplianceLogger = logger };

			Assert.That (reader.Read (), Is.True, "Read");
			Assert.That (reader.Tag, Is.EqualTo (TnefAttributeTag.OemCodepage), "Tag");
			Assert.That (Violations (logger), Has.Member (TnefComplianceViolation.InvalidMessageCodepage));
		}

		[Test]
		public async Task TestInvalidOemCodepageAsync ()
		{
			var logger = new TestTnefComplianceLogger ();
			var builder = new TnefBuilder ();

			builder.WriteOemCodepage (1);

			using var reader = new TnefReader (builder.ToStream ()) { ComplianceLogger = logger };

			Assert.That (await reader.ReadAsync (), Is.True, "ReadAsync");
			Assert.That (reader.Tag, Is.EqualTo (TnefAttributeTag.OemCodepage), "Tag");
			Assert.That (Violations (logger), Has.Member (TnefComplianceViolation.InvalidMessageCodepage));
		}

		[Test]
		public void TestInvalidTnefVersion ()
		{
			var logger = new TestTnefComplianceLogger ();
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion (1);

			using var reader = new TnefReader (builder.ToStream ()) { ComplianceLogger = logger };

			Assert.That (reader.Read (), Is.True, "Read");
			Assert.That (reader.Tag, Is.EqualTo (TnefAttributeTag.TnefVersion), "Tag");
			Assert.That (Violations (logger), Has.Member (TnefComplianceViolation.UnsupportedVersion));
		}

		[Test]
		public async Task TestInvalidTnefVersionAsync ()
		{
			var logger = new TestTnefComplianceLogger ();
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion (1);

			using var reader = new TnefReader (builder.ToStream ()) { ComplianceLogger = logger };

			Assert.That (await reader.ReadAsync (), Is.True, "ReadAsync");
			Assert.That (reader.Tag, Is.EqualTo (TnefAttributeTag.TnefVersion), "Tag");
			Assert.That (Violations (logger), Has.Member (TnefComplianceViolation.UnsupportedVersion));
		}

		[Test]
		public void TestNegativeAttributeRawValueLength ()
		{
			var logger = new TestTnefComplianceLogger ();
			using var stream = new MemoryStream ();

			WriteHeader (stream);
			WriteRawAttribute (stream, TnefAttributeLevel.Message, TnefAttributeTag.TnefVersion, -4, BitConverter.GetBytes (65536), checksum: false);
			stream.Position = 0;

			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			Assert.That (reader.Read (), Is.False, "Read");
			Assert.That (Violations (logger), Has.Member (TnefComplianceViolation.InvalidAttributeLength));
		}

		[Test]
		public async Task TestNegativeAttributeRawValueLengthAsync ()
		{
			var logger = new TestTnefComplianceLogger ();
			using var stream = new MemoryStream ();

			WriteHeader (stream);
			WriteRawAttribute (stream, TnefAttributeLevel.Message, TnefAttributeTag.TnefVersion, -4, BitConverter.GetBytes (65536), checksum: false);
			stream.Position = 0;

			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			Assert.That (await reader.ReadAsync (), Is.False, "ReadAsync");
			Assert.That (Violations (logger), Has.Member (TnefComplianceViolation.InvalidAttributeLength));
		}

		[Test]
		public void TestReadValueTruncated ()
		{
			var logger = new TestTnefComplianceLogger ();
			using var stream = new MemoryStream ();

			WriteHeader (stream);
			// Declares 28 bytes of value but only supplies 4.
			WriteRawAttribute (stream, TnefAttributeLevel.Message, TnefAttributeTag.MessageId, 28, BitConverter.GetBytes (0xFFFFFFFF), checksum: false);
			stream.Position = 0;

			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			Assert.That (reader.Read (), Is.True, "Read");
			Assert.That (reader.Tag, Is.EqualTo (TnefAttributeTag.MessageId), "Tag");

			var buffer = new byte[28];

			using (var value = reader.OpenValueStream ()) {
				while (value.Read (buffer, 0, buffer.Length) > 0)
					;
			}

			while (reader.Read ())
				;

			Assert.That (Violations (logger), Has.Member (TnefComplianceViolation.TruncatedStream));
		}

		[Test]
		public async Task TestReadValueTruncatedAsync ()
		{
			var logger = new TestTnefComplianceLogger ();
			using var stream = new MemoryStream ();

			WriteHeader (stream);
			WriteRawAttribute (stream, TnefAttributeLevel.Message, TnefAttributeTag.MessageId, 28, BitConverter.GetBytes (0xFFFFFFFF), checksum: false);
			stream.Position = 0;

			using var reader = new TnefReader (stream) { ComplianceLogger = logger };

			Assert.That (await reader.ReadAsync (), Is.True, "ReadAsync");
			Assert.That (reader.Tag, Is.EqualTo (TnefAttributeTag.MessageId), "Tag");

			var buffer = new byte[28];

			using (var value = reader.OpenValueStream ()) {
				while (await value.ReadAsync (buffer, 0, buffer.Length) > 0)
					;
			}

			while (await reader.ReadAsync ())
				;

			Assert.That (Violations (logger), Has.Member (TnefComplianceViolation.TruncatedStream));
		}

		#endregion

		#region Value re-readability

		[Test]
		public void TestFixedWidthValueIsRereadable ()
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();

			using var reader = new TnefReader (builder.ToStream ());

			Assert.That (reader.Read (), Is.True, "Read");
			Assert.That (reader.Tag, Is.EqualTo (TnefAttributeTag.TnefVersion), "Tag");

			var first = reader.ReadValueAsInt32 ();
			var second = reader.ReadValueAsInt32 ();

			Assert.That (second, Is.EqualTo (first), "fixed-width values are re-readable");
		}

		[Test]
		public async Task TestFixedWidthValueIsRereadableAsync ()
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();

			using var reader = new TnefReader (builder.ToStream ());

			Assert.That (await reader.ReadAsync (), Is.True, "ReadAsync");
			Assert.That (reader.Tag, Is.EqualTo (TnefAttributeTag.TnefVersion), "Tag");

			var first = await reader.ReadValueAsInt32Async ();
			var second = await reader.ReadValueAsInt32Async ();

			Assert.That (second, Is.EqualTo (first), "fixed-width values are re-readable");
		}

		[Test]
		public void TestSecondVariableReadThrows ()
		{
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Owner, new byte[] { 1, 2, 3, 4 });

			using var reader = new TnefReader (builder.ToStream ());

			Assert.That (reader.Read (), Is.True, "Read");

			var bytes = reader.ReadValueAsBytes ();

			Assert.That (bytes, Is.EqualTo (new byte[] { 1, 2, 3, 4 }), "bytes");
			Assert.Throws<InvalidOperationException> (() => reader.ReadValueAsString ());
		}

		[Test]
		public async Task TestSecondVariableReadThrowsAsync ()
		{
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Owner, new byte[] { 1, 2, 3, 4 });

			using var reader = new TnefReader (builder.ToStream ());

			Assert.That (await reader.ReadAsync (), Is.True, "ReadAsync");

			var bytes = await reader.ReadValueAsBytesAsync ();

			Assert.That (bytes, Is.EqualTo (new byte[] { 1, 2, 3, 4 }), "bytes");
			Assert.ThrowsAsync<InvalidOperationException> (async () => await reader.ReadValueAsStringAsync ());
		}

		[Test]
		public void TestValueStreamThrowsAfterReaderAdvances ()
		{
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Owner, new byte[] { 1, 2, 3, 4 });
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Owner, new byte[] { 5, 6, 7, 8 });

			using var reader = new TnefReader (builder.ToStream ());

			Assert.That (reader.Read (), Is.True, "Read #1");

			var value = reader.OpenValueStream ();

			Assert.That (reader.Read (), Is.True, "Read #2");
			Assert.Throws<InvalidOperationException> (() => value.Read (new byte[4], 0, 4));
		}

		[Test]
		public async Task TestValueStreamThrowsAfterReaderAdvancesAsync ()
		{
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Owner, new byte[] { 1, 2, 3, 4 });
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Owner, new byte[] { 5, 6, 7, 8 });

			using var reader = new TnefReader (builder.ToStream ());

			Assert.That (await reader.ReadAsync (), Is.True, "ReadAsync #1");

			var value = reader.OpenValueStream ();

			Assert.That (await reader.ReadAsync (), Is.True, "ReadAsync #2");
			Assert.Throws<InvalidOperationException> (() => value.Read (new byte[4], 0, 4));
		}

		#endregion

		#region Compliance issue cap

		[Test]
		public void TestMaxComplianceIssuesPerViolationCap ()
		{
			var logger = new TestTnefComplianceLogger ();
			var builder = new TnefBuilder ();
			var unknown = (TnefAttributeTag) ((int) TnefAttributeType.Byte | 0x1234);

			for (int i = 0; i < 10; i++)
				builder.WriteAttribute (TnefAttributeLevel.Message, unknown, new byte[] { 0 });

			using var reader = new TnefReader (builder.ToStream ()) {
				ComplianceLogger = logger,
				MaxComplianceIssuesPerViolation = 3
			};

			while (reader.Read ())
				;

			Assert.That (logger.Issues.Count (issue => issue.Violation == TnefComplianceViolation.UnknownAttribute), Is.EqualTo (3), "UnknownAttribute count");
			Assert.That (logger.Issues.Count (issue => issue.Violation == TnefComplianceViolation.TooManyComplianceIssues), Is.EqualTo (1), "TooManyComplianceIssues count");
		}

		[Test]
		public async Task TestMaxComplianceIssuesPerViolationCapAsync ()
		{
			var logger = new TestTnefComplianceLogger ();
			var builder = new TnefBuilder ();
			var unknown = (TnefAttributeTag) ((int) TnefAttributeType.Byte | 0x1234);

			for (int i = 0; i < 10; i++)
				builder.WriteAttribute (TnefAttributeLevel.Message, unknown, new byte[] { 0 });

			using var reader = new TnefReader (builder.ToStream ()) {
				ComplianceLogger = logger,
				MaxComplianceIssuesPerViolation = 3
			};

			while (await reader.ReadAsync ())
				;

			Assert.That (logger.Issues.Count (issue => issue.Violation == TnefComplianceViolation.UnknownAttribute), Is.EqualTo (3), "UnknownAttribute count");
			Assert.That (logger.Issues.Count (issue => issue.Violation == TnefComplianceViolation.TooManyComplianceIssues), Is.EqualTo (1), "TooManyComplianceIssues count");
		}

		#endregion

		#region Disposal

		[Test]
		public void TestReadAfterDisposeThrows ()
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();

			var reader = new TnefReader (builder.ToStream ());

			reader.Dispose ();

			Assert.Throws<ObjectDisposedException> (() => reader.Read ());
			Assert.Throws<ObjectDisposedException> (() => reader.ReadValueAsInt32 ());
		}

		[Test]
		public void TestReadAfterDisposeThrowsAsync ()
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();

			var reader = new TnefReader (builder.ToStream ());

			reader.Dispose ();

			Assert.ThrowsAsync<ObjectDisposedException> (async () => await reader.ReadAsync ());
		}

		#endregion

		#region leaveOpen

		[Test]
		public void TestLeaveOpenTrue ()
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();

			var stream = builder.ToStream ();

			using (var reader = new TnefReader (stream, null, leaveOpen: true))
				Assert.That (reader.Read (), Is.True, "Read");

			Assert.DoesNotThrow (() => _ = stream.Position, "the underlying stream should remain open");
			stream.Dispose ();
		}

		[Test]
		public void TestLeaveOpenFalse ()
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();

			var stream = builder.ToStream ();

			using (var reader = new TnefReader (stream))
				Assert.That (reader.Read (), Is.True, "Read");

			Assert.Throws<ObjectDisposedException> (() => _ = stream.Position, "the underlying stream should be disposed");
		}

		#endregion
	}
}

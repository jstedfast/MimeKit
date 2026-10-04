//
// Base64ValidatorTests.cs
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
using System.Runtime.Intrinsics;

using MimeKit;
using MimeKit.Encodings;

namespace UnitTests.Encodings {
	[TestFixture]
	public class Base64ValidatorTests : EncodingValidatorTestsBase
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			AssertArgumentExceptions (new Base64Validator (nullComplianceLogger, MimeComplianceContext.Transport, 0, 1));
		}

		[Test]
		public void TestEncoding ()
		{
			var validator = new Base64Validator (nullComplianceLogger, MimeComplianceContext.Transport, 0, 1);

			Assert.That (validator.Encoding, Is.EqualTo (ContentEncoding.Base64));
		}

		[TestCase ("VGhpcyBpcyB0aGUgcGxhaW4gdGV4dCBtZXNzYWdlIQ==")]
		[TestCase ("VGhpcyBpcyB0aGUgcGxhaW4g\t dGV4dCBtZXNzYWdlIQ==")]
		[TestCase ("VGhpcyBpcyBhIHRleHQgd2hpY2ggaGFzIHRvIGJlIHBhZGRlZCBvbmNlLi4=")]
		[TestCase ("VGhpcyBpcyBhIHRleHQgd2hpY2ggaGFzIHRvIGJlIHBhZGRlZCB0d2ljZQ==")]
		[TestCase ("VGhpcyBpcyBhIHRleHQgd2hpY2ggd2lsbCBub3QgYmUgcGFkZGVk")]
		// Note: RFC 2045, section 6.8 does not require each line to contain complete quantums - line
		// breaks must be ignored by decoders, so a quantum may straddle a line break.
		[TestCase ("VGhpcyBpcy\r\nB0aGUgcGxhaW4gdGV4dCBtZXNzYWdlIQ==")]
		[TestCase ("VGhpcyBpcyB0aGUgcGxhaW4gdGV4dCBtZXNzYWdlIQ=\r\n=")]
		public void TestValidateValidInput (string text)
		{
			var rawData = Encoding.ASCII.GetBytes (text);
			var logger = new TestMimeComplianceLogger ();
			var validator = new Base64Validator (logger, MimeComplianceContext.Transport, 0, 1);

			validator.Write (rawData, 0, rawData.Length);
			validator.Flush ();

			Assert.That (logger.Issues.Count, Is.EqualTo (0));
		}

		static void TestValidateInvalidInput (string text, List<MimeComplianceIssue> issues)
		{
			var rawData = Encoding.ASCII.GetBytes (text);
			var logger = new TestMimeComplianceLogger ();
			var validator = new Base64Validator (logger, MimeComplianceContext.Transport, 0, 1);

			validator.Write (rawData, 0, rawData.Length);
			validator.Flush ();

			AssertInvalidInput (logger, issues);
		}

		[Test]
		public void TestValidateInvalidInput_MultipleInvalidCharacters ()
		{
			// Note: The '%' on line 1 and the '!' on line 3 are not reported: each violation is
			// reported at most once per line so that a line of garbage cannot emit an issue per byte.
			const string text = " &% VGhp\r\ncyBp\r\ncyB0aGUgcGxhaW4g  \tdGV4dCBtZ?!XNzY*WdlIQ==";
			var issues = new List<MimeComplianceIssue> {
				new MimeComplianceIssue (MimeComplianceContext.Transport, MimeComplianceViolation.InvalidBase64Character, 1, 1, 2),
				new MimeComplianceIssue (MimeComplianceContext.Transport, MimeComplianceViolation.InvalidBase64Character, 44, 3, 29),
				new MimeComplianceIssue (MimeComplianceContext.Transport, MimeComplianceViolation.ObsoleteBase64Comment, 50, 3, 35),
			};

			TestValidateInvalidInput (text, issues);
		}

		[Test]
		public void TestValidateInvalidInput_InvalidCharacterIsReportedOncePerLine ()
		{
			// Note: The number of invalid octets on a line is attacker-controlled, so an unthrottled
			// report would let a crafted part emit an issue per byte of content. Only the first
			// invalid octet on each line is reported, and the latch resets at the line break.
			const string text = "????????????????\r\n????????????????\r\n";
			var issues = new List<MimeComplianceIssue> {
				new MimeComplianceIssue (MimeComplianceContext.Transport, MimeComplianceViolation.InvalidBase64Character, 0, 1, 1),
				new MimeComplianceIssue (MimeComplianceContext.Transport, MimeComplianceViolation.InvalidBase64Character, 18, 2, 1),
			};

			TestValidateInvalidInput (text, issues);
		}

		[Test]
		public void TestValidateInvalidInput_ObsoleteCommentIsReportedOncePerLine ()
		{
			const string text = "VGhp***********\r\ncyBp***********\r\n";
			var issues = new List<MimeComplianceIssue> {
				new MimeComplianceIssue (MimeComplianceContext.Transport, MimeComplianceViolation.ObsoleteBase64Comment, 4, 1, 5),
				new MimeComplianceIssue (MimeComplianceContext.Transport, MimeComplianceViolation.ObsoleteBase64Comment, 21, 2, 5),
			};

			TestValidateInvalidInput (text, issues);
		}

		[Test]
		public void TestValidateInvalidInput_InvalidCharacterLatchSpansWriteCalls ()
		{
			// Note: The latch is an instance field rather than a local because a malformed line can
			// straddle any number of Write() calls.
			var rawData = Encoding.ASCII.GetBytes ("????????\r\n????????\r\n");
			var logger = new TestMimeComplianceLogger ();
			var validator = new Base64Validator (logger, MimeComplianceContext.Transport, 0, 1);

			for (int i = 0; i < rawData.Length; i++)
				validator.Write (rawData, i, 1);

			validator.Flush ();

			var issues = new List<MimeComplianceIssue> {
				new MimeComplianceIssue (MimeComplianceContext.Transport, MimeComplianceViolation.InvalidBase64Character, 0, 1, 1),
				new MimeComplianceIssue (MimeComplianceContext.Transport, MimeComplianceViolation.InvalidBase64Character, 10, 2, 1),
			};

			AssertInvalidInput (logger, issues);
		}

		[Test]
		public void TestValidateInvalidInput_IncorrectPadding1 ()
		{
			const string text = "VGhpcyBpcyB0aGUgcGxhaW4gdGV4dCBtZXNzYWdlIQ===";
			var issues = new List<MimeComplianceIssue> {
				new MimeComplianceIssue (MimeComplianceContext.Transport, MimeComplianceViolation.InvalidBase64Padding, 44, 1, 45)
			};

			TestValidateInvalidInput (text, issues);
		}

		[Test]
		public void TestValidateInvalidInput_IncorrectPadding2 ()
		{
			const string text = "VGhpcyBpcyB0aGUgcGxhaW4gdGV4dCBtZXNzYWdlIQ====";
			var issues = new List<MimeComplianceIssue> {
				new MimeComplianceIssue (MimeComplianceContext.Transport, MimeComplianceViolation.InvalidBase64Padding, 44, 1, 45)
			};

			TestValidateInvalidInput (text, issues);
		}

		[Test]
		public void TestValidateInvalidInput_IncorrectPadding3 ()
		{
			const string text = "VGhpcyBpcyB0aGUgcGxhaW4gdGV4dCBtZXNzYWdlIQ=====";
			var issues = new List<MimeComplianceIssue> {
				new MimeComplianceIssue (MimeComplianceContext.Transport, MimeComplianceViolation.InvalidBase64Padding, 44, 1, 45)
			};

			TestValidateInvalidInput (text, issues);
		}

		[Test]
		public void TestValidateInvalidInput_CharactersAfterPadding ()
		{
			const string text = "VGhpcyBpcyB0aGUgcGF5bG9hZCBvZiB0aGUgZmlyc3QgYmFzZTY0LWVuY29kZWQgYmxvY2sgb2Yg\r\ndGV4dC4=\r\nQW5kIHRoaXMgaXMgdGhlIHBheWxvYWQgb2YgdGhlIHNlY29uZCBiYXNlNjQtZW5jb2RlZCBibG9j\r\nayBvZiB0ZXh0Lg==\r\n";
			var issues = new List<MimeComplianceIssue> {
				new MimeComplianceIssue (MimeComplianceContext.Transport, MimeComplianceViolation.Base64CharactersAfterPadding, text.IndexOf ('=') + 3, 3, 1)
			};

			TestValidateInvalidInput (text, issues);
		}

		[Test]
		public void TestValidateInvalidInput_IncompleteQuantum ()
		{
			const string text = "VGhpcyBpcyB0aGUgcGxhaW4gdGV4dCBtZXNzYWdlIQ=\r\n";
			var issues = new List<MimeComplianceIssue> {
				new MimeComplianceIssue (MimeComplianceContext.Transport, MimeComplianceViolation.IncompleteBase64Quantum, 45, 2, 1)
			};

			TestValidateInvalidInput (text, issues);
		}

		[Test]
		public void TestValidateInvalidInput_IncompleteQuantum_NoNewLine ()
		{
			const string text = "VGhpcyBpcyB0aGUgcGxhaW4gdGV4dCBtZXNzYWdlIQ=";
			var issues = new List<MimeComplianceIssue> {
				new MimeComplianceIssue (MimeComplianceContext.Transport, MimeComplianceViolation.IncompleteBase64Quantum, 43, 1, 44)
			};

			TestValidateInvalidInput (text, issues);
		}

		[TestCase (4096)]
		[TestCase (1024)]
		[TestCase (16)]
		[TestCase (1)]
		public void TestValidateBufferSize (int bufferSize)
		{
			var logger = new TestMimeComplianceLogger ();

			TestValidator (logger, new Base64Validator (logger, MimeComplianceContext.Transport, 0, 1), "photo.b64", photo_b64, bufferSize);
		}

		public enum CodePath
		{
			Scalar,
			Vector128,
			Vector256
		}

		static Base64Validator CreateValidator (CodePath path, IMimeComplianceLogger logger, long streamOffset = 0, int lineNumber = 1)
		{
			var validator = new Base64Validator (logger, MimeComplianceContext.Transport, streamOffset, lineNumber);

			switch (path) {
			case CodePath.Vector256:
				if (!Vector256.IsHardwareAccelerated)
					Assert.Ignore ("Vector256 is not hardware accelerated on this host.");

				validator.MaxVectorSize = 32;
				break;
			case CodePath.Vector128:
				if (!Vector128.IsHardwareAccelerated)
					Assert.Ignore ("Vector128 is not hardware accelerated on this host.");

				validator.MaxVectorSize = 16;
				break;
			default:
				validator.MaxVectorSize = 0;
				break;
			}

			return validator;
		}

		static List<MimeComplianceIssue> Validate (CodePath path, byte[] input, int chunkSize, long streamOffset = 0, int lineNumber = 1)
		{
			var logger = new TestMimeComplianceLogger ();
			var validator = CreateValidator (path, logger, streamOffset, lineNumber);

			chunkSize = Math.Max (1, Math.Min (chunkSize, input.Length));

			for (int index = 0; index < input.Length; index += chunkSize)
				validator.Write (input, index, Math.Min (chunkSize, input.Length - index));

			validator.Flush ();

			return logger.Issues;
		}

		static byte[] Wrap (string base64, int lineLength, string newLine, string trailer)
		{
			var builder = new StringBuilder ();

			for (int i = 0; i < base64.Length; i += lineLength) {
				builder.Append (base64, i, Math.Min (lineLength, base64.Length - i));
				builder.Append (newLine);
			}

			builder.Append (trailer);

			return Encoding.ASCII.GetBytes (builder.ToString ());
		}

		static readonly object[] LineFormats = {
			new object[] { 1, "\r\n" },
			new object[] { 3, "\n" },
			new object[] { 5, "\r\n" },
			new object[] { 15, "\r\n" },
			new object[] { 16, "\n" },
			new object[] { 17, "\r\n" },
			new object[] { 31, "\r\n" },
			new object[] { 33, "\n" },
			new object[] { 57, "\r\n" },
			new object[] { 64, "\r\n" },
			new object[] { 72, " \t\r\n" },
			new object[] { 75, "\r\n" },
			new object[] { 75, "\n" },
			new object[] { 76, "\r\n" },
			new object[] { 77, "\r\n" },
			new object[] { 1000, "\r\n" },
			new object[] { int.MaxValue, "" },
		};

		[Test]
		public void TestValidateRandomData ([Values] CodePath path, [ValueSource (nameof (LineFormats))] object[] format)
		{
			int lineLength = (int) format[0];
			var newLine = (string) format[1];
			var random = new Random (lineLength * 31 + newLine.Length);
			int[] chunkSizes = { 1, 3, 15, 16, 17, 31, 32, 33, 77, 4096, int.MaxValue };

			for (int length = 0; length < 1200; length += random.Next (1, 50)) {
				var data = new byte[length];

				random.NextBytes (data);

				var base64 = Convert.ToBase64String (data);

				// Valid content should not report any issues.
				var encoded = Wrap (base64, lineLength, newLine, string.Empty);

				foreach (var chunkSize in chunkSizes) {
					var issues = Validate (path, encoded, chunkSize, 100, 7);

					Assert.That (issues, Is.Empty, $"length={length}, chunkSize={chunkSize}");
				}

				// Append an invalid character so that the line and column tracking can be verified.
				encoded = Wrap (base64.TrimEnd ('='), lineLength, newLine, "?");
				var expected = Validate (CodePath.Scalar, encoded, int.MaxValue, 100, 7);

				Assert.That (expected.Count, Is.GreaterThan (0), $"Reference: length={length}");

				foreach (var chunkSize in chunkSizes)
					Assert.That (Validate (path, encoded, chunkSize, 100, 7), Is.EqualTo (expected), $"length={length}, chunkSize={chunkSize}");
			}
		}

		[Test]
		public void TestValidateRandomDataWithGarbage ([Values] CodePath path)
		{
			const string garbage = " \t\r\n\v\f=*?!-.~\0\u0080\u00ff@[`{:";
			int[] lineLengths = { 3, 17, 57, 75, 76, 77, int.MaxValue };
			int[] chunkSizes = { 1, 7, 16, 33, 100, int.MaxValue };
			var random = new Random (1214);

			for (int iteration = 0; iteration < 500; iteration++) {
				var data = new byte[random.Next (0, 600)];

				random.NextBytes (data);

				int lineLength = lineLengths[random.Next (lineLengths.Length)];
				var wrapped = Encoding.ASCII.GetString (Wrap (Convert.ToBase64String (data), lineLength, "\r\n", string.Empty));
				var positions = new SortedSet<int> ();
				var builder = new StringBuilder ();
				int insertions = random.Next (0, 10);

				for (int i = 0; i < insertions; i++)
					positions.Add (random.Next (0, wrapped.Length + 1));

				int last = 0;
				foreach (var position in positions) {
					builder.Append (wrapped, last, position - last);
					int count = random.Next (1, 4);
					for (int i = 0; i < count; i++)
						builder.Append (garbage[random.Next (garbage.Length)]);
					last = position;
				}
				builder.Append (wrapped, last, wrapped.Length - last);

				var encoded = new byte[builder.Length];
				for (int i = 0; i < builder.Length; i++)
					encoded[i] = (byte) builder[i];

				// The scalar validator is the reference implementation.
				var expected = Validate (CodePath.Scalar, encoded, int.MaxValue);

				foreach (var chunkSize in chunkSizes)
					Assert.That (Validate (path, encoded, chunkSize), Is.EqualTo (expected), $"iteration={iteration}, chunkSize={chunkSize}");
			}
		}

		const string Base64Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

		static bool IssuesEqual (List<MimeComplianceIssue> actual, List<MimeComplianceIssue> expected)
		{
			if (actual.Count != expected.Count)
				return false;

			for (int i = 0; i < actual.Count; i++) {
				if (!actual[i].Equals (expected[i]))
					return false;
			}

			return true;
		}

		static void AssertMatchesScalar (CodePath path, byte[] encoded, int[] chunkSizes, string message)
		{
			// The scalar validator is the reference implementation.
			var expected = Validate (CodePath.Scalar, encoded, int.MaxValue, 100, 7);

			foreach (var chunkSize in chunkSizes) {
				var actual = Validate (path, encoded, chunkSize, 100, 7);

				if (!IssuesEqual (actual, expected))
					Assert.That (actual, Is.EqualTo (expected), $"{message}, chunkSize={chunkSize}");
			}
		}

		[Test]
		public void TestValidateEveryByteAtEveryPosition ([Values] CodePath path, [Values] bool replace)
		{
			// The SIMD kernels classify bytes using range comparisons. Verify that every possible byte value, at every
			// position within (and across) the 16 and 32-byte blocks, is classified exactly like the scalar validator.
			// The input spans multiple lines so that the line and column tracking is verified as well.
			int[] chunkSizes = { 16, 33, int.MaxValue };
			var data = new byte[72];

			new Random (72).NextBytes (data);

			var base64 = Wrap (Convert.ToBase64String (data), 40, "\r\n", string.Empty);

			for (int value = 0; value < 256; value++) {
				for (int position = 0; position < base64.Length; position++) {
					byte[] encoded;

					if (replace) {
						encoded = (byte[]) base64.Clone ();
					} else {
						encoded = new byte[base64.Length + 1];
						Buffer.BlockCopy (base64, 0, encoded, 0, position);
						Buffer.BlockCopy (base64, position, encoded, position + 1, base64.Length - position);
					}

					encoded[position] = (byte) value;

					AssertMatchesScalar (path, encoded, chunkSizes, $"value=0x{value:X2}, position={position}");
				}
			}
		}

		[Test]
		public void TestValidateAllLineLengths ([Values] CodePath path, [Values ("\n", "\r\n", " \t\r\n")] string newLine)
		{
			// Every line contains an invalid character (and an obsolete comment character) at a random position so that the
			// once-per-line latches, the line numbers and the column numbers are verified for every line length.
			int[] dataLengths = { 1, 2, 3, 47, 48, 49, 301 };
			int[] chunkSizes = { 1, 15, 16, 17, 32, 33, 77, int.MaxValue };
			var random = new Random (newLine.Length);

			for (int lineLength = 1; lineLength <= 100; lineLength++) {
				foreach (var dataLength in dataLengths) {
					var data = new byte[dataLength];

					random.NextBytes (data);

					var base64 = Convert.ToBase64String (data);
					var encoded = Wrap (base64, lineLength, newLine, string.Empty);

					foreach (var chunkSize in chunkSizes) {
						var issues = Validate (path, encoded, chunkSize, 100, 7);

						if (issues.Count != 0)
							Assert.That (issues, Is.Empty, $"lineLength={lineLength}, dataLength={dataLength}, chunkSize={chunkSize}");
					}

					var builder = new StringBuilder ();
					var unpadded = base64.TrimEnd ('=');

					for (int i = 0; i < unpadded.Length; i += lineLength) {
						var line = unpadded.Substring (i, Math.Min (lineLength, unpadded.Length - i));

						line = line.Insert (random.Next (line.Length + 1), "?");
						line = line.Insert (random.Next (line.Length + 1), "*");

						builder.Append (line);
						builder.Append (newLine);
					}

					AssertMatchesScalar (path, Encoding.ASCII.GetBytes (builder.ToString ()), chunkSizes, $"lineLength={lineLength}, dataLength={dataLength}");
				}
			}
		}

		static byte[] GenerateRandomBytes (Random random, int length, double density)
		{
			var encoded = new byte[length];

			for (int i = 0; i < length; i++) {
				if (random.NextDouble () < density)
					encoded[i] = (byte) Base64Alphabet[random.Next (Base64Alphabet.Length)];
				else
					encoded[i] = (byte) random.Next (256);
			}

			return encoded;
		}

		[Test]
		public void TestValidateRandomBytes ([Values] CodePath path, [Values (0.0, 0.5, 0.9, 0.99, 0.999)] double density)
		{
			// Base64 alphabet characters interspersed with arbitrary bytes (including '=' padding, line breaks and 8-bit
			// bytes) at the specified density.
			int[] chunkSizes = { 1, 7, 16, 31, 32, 33, 100, int.MaxValue };
			var random = new Random ((int) (density * 1000));

			for (int iteration = 0; iteration < 300; iteration++) {
				var encoded = GenerateRandomBytes (random, random.Next (0, 800), density);

				AssertMatchesScalar (path, encoded, chunkSizes, $"iteration={iteration}");
			}
		}

		static byte[] GenerateEncoded (Random random)
		{
			var data = new byte[random.Next (0, 1000)];

			random.NextBytes (data);

			var newLine = random.Next (3) switch { 0 => "\n", 1 => "\r\n", _ => string.Empty };
			var encoded = Wrap (Convert.ToBase64String (data), random.Next (1, 100), newLine, string.Empty);

			if (encoded.Length > 0 && random.Next (2) == 0) {
				int corruptions = random.Next (1, 10);

				for (int i = 0; i < corruptions; i++)
					encoded[random.Next (encoded.Length)] = (byte) random.Next (256);
			}

			return encoded;
		}

		[Test]
		public unsafe void TestValidateDoesNotAccessMemoryOutOfBounds ([Values] CodePath path, [Values] bool alignEnd)
		{
			// Places each chunk of input immediately before (or after) an inaccessible guard page so that reading even a
			// single byte out of bounds crashes rather than going unnoticed.
			int[] chunkSizes = { 1, 15, 16, 17, 31, 32, 33, 47, 48, 49, 64, 100, 4096 };
			var random = new Random (alignEnd ? 2024 : 2025);

			for (int iteration = 0; iteration < 100; iteration++) {
				var encoded = random.Next (4) == 0 ? GenerateRandomBytes (random, random.Next (0, 800), 0.95) : GenerateEncoded (random);
				var expected = Validate (CodePath.Scalar, encoded, int.MaxValue, 100, 7);

				foreach (var chunkSize in chunkSizes) {
					var logger = new TestMimeComplianceLogger ();
					var validator = CreateValidator (path, logger, 100, 7);
					int size = Math.Max (1, Math.Min (chunkSize, encoded.Length));

					for (int index = 0; index < encoded.Length; index += size) {
						int n = Math.Min (size, encoded.Length - index);

						using var input = new GuardedMemory (n, alignEnd);

						encoded.AsSpan (index, n).CopyTo (new Span<byte> (input.Start, n));

						validator.Write (input.Start, n);
					}

					validator.Flush ();

					Assert.That (logger.Issues, Is.EqualTo (expected), $"iteration={iteration}, chunkSize={chunkSize}");
				}
			}
		}
	}
}

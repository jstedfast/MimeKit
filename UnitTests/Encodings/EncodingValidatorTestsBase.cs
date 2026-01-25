//
// EncodingValidatorTestsBase.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2025 .NET Foundation and Contributors
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
using MimeKit.IO;
using MimeKit.Encodings;
using MimeKit.IO.Filters;

namespace UnitTests.Encodings {
	public abstract class EncodingValidatorTestsBase
	{
		protected static readonly string dataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "encoders");
		protected static readonly MimeReader dummyReader;
		protected static readonly byte[] wikipedia_unix;
		protected static readonly byte[] wikipedia_dos;
		protected static readonly byte[] photo_b64;
		protected static readonly byte[] photo_uu;

		static EncodingValidatorTestsBase ()
		{
			dummyReader = new MimeReader (Stream.Null);

			using (var memory = new MemoryStream ()) {
				using (var filtered = new FilteredStream (memory)) {
					filtered.Add (new Dos2UnixFilter ());

					using (var file = File.OpenRead (Path.Combine (dataDir, "wikipedia.qp")))
						file.CopyTo (filtered, 4096);

					filtered.Flush ();
				}

				wikipedia_unix = memory.ToArray ();
			}

			using (var memory = new MemoryStream ()) {
				using (var filtered = new FilteredStream (memory)) {
					filtered.Add (new Unix2DosFilter ());

					using (var file = File.OpenRead (Path.Combine (dataDir, "wikipedia.qp")))
						file.CopyTo (filtered, 4096);

					filtered.Flush ();
				}

				wikipedia_dos = memory.ToArray ();
			}

			photo_b64 = File.ReadAllBytes (Path.Combine (dataDir, "photo.b64"));
			photo_uu = File.ReadAllBytes (Path.Combine (dataDir, "photo.uu"));
		}

		internal static void AssertArgumentExceptions (IEncodingValidator validator)
		{
			Assert.Throws<ArgumentNullException> (() => validator.Write (null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => validator.Write (Array.Empty<byte> (), -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => validator.Write (new byte[1], 0, 10));
		}

		internal static void AssertInvalidInput (ComplianceMimeReader reader, List<MimeComplianceViolationEventArgs> violations)
		{
			Assert.That (reader.ComplianceViolations.Count, Is.EqualTo (violations.Count), "ComplianceViolations.Count");

			for (int i = 0; i < violations.Count; i++) {
				Assert.That (reader.ComplianceViolations[i].Violation, Is.EqualTo (violations[i].Violation), $"Violation[{i}]");
				Assert.That (reader.ComplianceViolations[i].StreamOffset, Is.EqualTo (violations[i].StreamOffset), $"Violation[{i}].StreamOffset");
				Assert.That (reader.ComplianceViolations[i].LineNumber, Is.EqualTo (violations[i].LineNumber), $"Violation[{i}].LineNumber");
			}
		}

		internal static void TestValidator (ComplianceMimeReader reader, IEncodingValidator validator, string fileName, byte[] rawData, int bufferSize)
		{
			for (int i = 0; i < rawData.Length; i += bufferSize) {
				int n = Math.Min (bufferSize, rawData.Length - i);

				validator.Write (rawData, i, n);
			}

			validator.Flush ();

			Assert.That (reader.ComplianceViolations.Count, Is.EqualTo (0), $"{fileName}: Complete failed");
		}
	}
}

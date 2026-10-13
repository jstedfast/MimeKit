//
// UnicodeNormalizationTests.cs
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
using System.Globalization;
using System.IO.Compression;

using MimeKit.Utils;

namespace UnitTests.Utils {
	[TestFixture]
	public class UnicodeNormalizationTests
	{
		[TestCase ("\uAC00", true, TestName = "TestHangulLV")]
		[TestCase ("\uAC01", true, TestName = "TestHangulLVT")]
		[TestCase ("\u1100\u1161", false, TestName = "TestHangulLPlusV")]
		[TestCase ("\uAC00\u11A8", false, TestName = "TestHangulLVPlusT")]
		[TestCase ("\uAC01\u11A8", true, TestName = "TestHangulLVTPlusT")]
		[TestCase ("\U000113C2\U000113C2", false, TestName = "TestTuluTigalariVowelSign")]
		[TestCase ("\U000113C5", true, TestName = "TestTuluTigalariComposite")]
		public void TestIsNormalizationFormC (string text, bool expected)
		{
			Assert.That (UnicodeNormalization.IsNormalizationFormC (text.AsSpan ()), Is.EqualTo (expected));
		}

		static string ParseNormalizationTestColumn (string column)
		{
			var builder = new StringBuilder ();

			foreach (var hex in column.Split (' ', StringSplitOptions.RemoveEmptyEntries))
				builder.Append (char.ConvertFromUtf32 (int.Parse (hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture)));

			return builder.ToString ();
		}

		// NormalizationTest.txt (UCD 18.0.0) has the format c1;c2;c3;c4;c5 where c2 == NFC(c1) == NFC(c2) == NFC(c3)
		// and c4 == NFC(c4) == NFC(c5). Every code point not listed in c1 (Part 1) is invariant under normalization.
		[Test]
		public void TestNormalizationTestData ()
		{
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", "unicode", "NormalizationTest.txt.gz");
			var listed = new HashSet<int> ();
			int count = 0;

			using (var stream = new GZipStream (File.OpenRead (path), CompressionMode.Decompress))
			using (var reader = new StreamReader (stream, Encoding.ASCII)) {
				string line;

				while ((line = reader.ReadLine ()) != null) {
					if (line.Length == 0 || line[0] == '#' || line[0] == '@')
						continue;

					var columns = line.Split (';');
					var c1 = ParseNormalizationTestColumn (columns[0]);
					var c2 = ParseNormalizationTestColumn (columns[1]);
					var c3 = ParseNormalizationTestColumn (columns[2]);
					var c4 = ParseNormalizationTestColumn (columns[3]);
					var c5 = ParseNormalizationTestColumn (columns[4]);

					Assert.That (UnicodeNormalization.IsNormalizationFormC (c1.AsSpan ()), Is.EqualTo (c1 == c2), $"c1: {line}");
					Assert.That (UnicodeNormalization.IsNormalizationFormC (c2.AsSpan ()), Is.True, $"c2: {line}");
					Assert.That (UnicodeNormalization.IsNormalizationFormC (c3.AsSpan ()), Is.EqualTo (c3 == c2), $"c3: {line}");
					Assert.That (UnicodeNormalization.IsNormalizationFormC (c4.AsSpan ()), Is.True, $"c4: {line}");
					Assert.That (UnicodeNormalization.IsNormalizationFormC (c5.AsSpan ()), Is.EqualTo (c5 == c4), $"c5: {line}");

					if (char.IsSurrogatePair (c1, 0) ? c1.Length == 2 : c1.Length == 1)
						listed.Add (char.ConvertToUtf32 (c1, 0));

					count++;
				}
			}

			Assert.That (count, Is.GreaterThan (19000), "Number of test lines");

			for (int codepoint = 0; codepoint <= 0x10FFFF; codepoint++) {
				if (codepoint >= 0xD800 && codepoint <= 0xDFFF)
					continue;

				if (listed.Contains (codepoint))
					continue;

				var text = char.ConvertFromUtf32 (codepoint);

				Assert.That (UnicodeNormalization.IsNormalizationFormC (text.AsSpan ()), Is.True, $"U+{codepoint:X4}");
			}
		}
	}
}

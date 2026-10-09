//
// YDecoderTests.cs
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

using MimeKit;
using MimeKit.Encodings;

namespace UnitTests.Encodings {
	[TestFixture]
	public class YDecoderTests : MimeDecoderTestsBase
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			AssertArgumentExceptions (new YDecoder ());
		}

		[Test]
		public void TestEncoding ()
		{
			var decoder = new YDecoder ();

			Assert.That (decoder.Encoding, Is.EqualTo (ContentEncoding.Default));
		}

		[Test]
		public void TestClone ()
		{
			CloneAndAssert (new YDecoder (true));
			CloneAndAssert (new YDecoder (false));
		}

		[Test]
		public void TestReset ()
		{
			ResetAndAssert (new YDecoder (true));
			ResetAndAssert (new YDecoder (false));
		}

		static byte[] Decode (YDecoder decoder, byte[] input, int chunkSize)
		{
			var decoded = new List<byte> ();

			for (int index = 0; index < input.Length; index += chunkSize) {
				int count = Math.Min (chunkSize, input.Length - index);
				var output = new byte[decoder.EstimateOutputLength (count)];
				int n = decoder.Decode (input, index, count, output);

				decoded.AddRange (output.AsSpan (0, n).ToArray ());
			}

			return decoded.ToArray ();
		}

		[Test]
		public void TestDecodeYBeginAndYPartSplitAtEveryByte ()
		{
			const string text = "ignored preface\n=ybegin part=1 line=128 size=7 name=test.bin\n=ypart begin=1 end=7\n=@=J=M=}=n=IA\n=yend size=7\ntrailing";
			byte[] expected = { 214, 224, 227, 19, 4, 223, 23 };
			var input = Encoding.ASCII.GetBytes (text);

			Assert.That (Decode (new YDecoder (), input, input.Length), Is.EqualTo (expected), "one-shot");

			for (int chunkSize = 1; chunkSize < input.Length; chunkSize++)
				Assert.That (Decode (new YDecoder (), input, chunkSize), Is.EqualTo (expected), $"chunkSize={chunkSize}");
		}

		[Test]
		public void TestDecodePayloadOnlyCloneContinuesWithSameState ()
		{
			byte[] expected = { 214, 224, 227, 19, 4, 223, 23 };
			var input = Encoding.ASCII.GetBytes ("=@=J=M=}=n=IA\n=yend size=7\n");
			var decoder = new YDecoder (true);
			var output = new byte[decoder.EstimateOutputLength (input.Length)];

			int n = decoder.Decode (input, 0, 6, output);
			var clone = (YDecoder) decoder.Clone ();
			var suffix = new byte[decoder.EstimateOutputLength (input.Length - 6)];
			int m = decoder.Decode (input, 6, input.Length - 6, suffix);
			var cloneOutput = new byte[clone.EstimateOutputLength (input.Length - 6)];
			int cloneLength = clone.Decode (input, 6, input.Length - 6, cloneOutput);

			Array.Copy (suffix, 0, output, n, m);

			Assert.That (output.AsSpan (0, n + m).ToArray (), Is.EqualTo (expected), "continued decode");
			Assert.That (n, Is.EqualTo (3), "prefix length");
			Assert.That (m, Is.EqualTo (4), "suffix length");
			Assert.That (cloneOutput.AsSpan (0, cloneLength).ToArray (), Is.EqualTo (expected.AsSpan (3).ToArray ()), "clone suffix");
			Assert.That (clone.Checksum, Is.EqualTo (decoder.Checksum), "checksum");
		}
	}
}

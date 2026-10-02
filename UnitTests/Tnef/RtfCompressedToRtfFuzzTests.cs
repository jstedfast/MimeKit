//
// RtfCompressedToRtfFuzzTests.cs
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

using System.Buffers.Binary;
using System.Diagnostics;

using MimeKit.Tnef;

namespace UnitTests.Tnef {
	/// <summary>
	/// Feeds deliberately corrupted compressed RTF streams to <see cref="RtfCompressedToRtf"/>.
	/// </summary>
	/// <remarks>
	/// <para>The decompressor copies out of a fixed 4096 byte dictionary using offsets and lengths taken
	/// straight from the input, and sizes its output buffer from two header fields that are equally
	/// untrusted, so it is worth treating as a buffer overrun surface.</para>
	/// <para>A <see cref="MimeKit.IMimeFilter"/> has no way to report a problem to its caller, so the
	/// invariant being asserted here is simply that no input can make it throw, hang, or produce an
	/// unreasonable amount of output.</para>
	/// </remarks>
	[TestFixture]
	public class RtfCompressedToRtfFuzzTests
	{
		// A compressed stream must expand by less than 8x: the densest possible encoding is a flag byte
		// followed by 8 references of 17 bytes each, which is 17 bytes of input for 136 bytes of output.
		const int MaxExpansionFactor = 9;

		const int TimeoutMilliseconds = 30000;

		static byte[] WellFormedStream (int seed, int length)
		{
			var random = new Random (seed);
			var data = new byte[length];

			for (int i = 0; i < data.Length; i++)
				data[i] = (byte) (0x20 + random.Next (0x5F));

			var builder = new RtfCompressedBuilder ();

			builder.WriteLiterals (data);
			builder.WriteReference (0, 17);
			builder.WriteReference (207, 9);
			builder.WriteEndOfStream ();

			return builder.ToArray ();
		}

		static long AssertInvariants (byte[] input, int chunkSize, string message)
		{
			var filter = new RtfCompressedToRtf ();
			var stopwatch = Stopwatch.StartNew ();
			long total = 0;

			try {
				int index = 0;

				while (index < input.Length) {
					int n = Math.Min (chunkSize, input.Length - index);

					filter.Filter (input, index, n, out _, out int outputLength);
					total += outputLength;
					index += n;
				}

				filter.Flush (input, 0, 0, out _, out int flushedLength);
				total += flushedLength;
			} catch (Exception ex) {
				Assert.Fail ($"{message}: threw {ex.GetType ().Name}: {ex.Message}");
			}

			stopwatch.Stop ();

			Assert.That (stopwatch.ElapsedMilliseconds, Is.LessThan (TimeoutMilliseconds), $"{message}: took too long");
			Assert.That (total, Is.LessThanOrEqualTo ((long) input.Length * MaxExpansionFactor + 4096), $"{message}: produced {total} bytes from {input.Length}");

			return total;
		}

		[Test]
		public void TestTruncationAtEveryOffset ()
		{
			// Every field of the header and every token in the body has to tolerate the stream simply
			// stopping part way through it.
			var stream = WellFormedStream (1, 512);

			for (int length = 0; length <= stream.Length; length++) {
				var truncated = new byte[length];

				Buffer.BlockCopy (stream, 0, truncated, 0, length);

				AssertInvariants (truncated, int.MaxValue, $"truncated to {length}");
			}
		}

		[TestCase (0)]
		[TestCase (int.MinValue)]
		[TestCase (int.MaxValue)]
		[TestCase (-1)]
		[TestCase (11)]
		[TestCase (0x40000000)]
		public void TestHostileHeaderFields (int value)
		{
			// COMPSIZE and RAWSIZE drive both the bounds of the stream and the size of the output buffer.
			var stream = WellFormedStream (2, 256);

			for (int field = 0; field < 4; field++) {
				var mutated = (byte[]) stream.Clone ();

				BinaryPrimitives.WriteInt32LittleEndian (mutated.AsSpan (field * 4, 4), value);

				AssertInvariants (mutated, int.MaxValue, $"header field {field} = {value}");
			}
		}

		[Test]
		public void TestRandomBitFlips ()
		{
			// Flipping bits in the body turns literals into references and changes the offsets and lengths
			// those references use, which is the most direct way to aim them out of bounds.
			var random = new Random (20260101);
			var stream = WellFormedStream (3, 1024);
			int productive = 0;

			for (int iteration = 0; iteration < 5000; iteration++) {
				var mutated = (byte[]) stream.Clone ();
				int flips = 1 + random.Next (16);

				for (int i = 0; i < flips; i++)
					mutated[random.Next (mutated.Length)] ^= (byte) (1 << random.Next (8));

				if (AssertInvariants (mutated, int.MaxValue, $"iteration {iteration}") > 0)
					productive++;
			}

			// Guard against the corruption becoming so severe that the decompressor rejects every stream
			// out of hand, which would leave this test passing without exercising anything.
			Assert.That (productive, Is.GreaterThan (2500), "iterations that produced output");
		}

		[Test]
		public void TestRandomBodies ()
		{
			// A body of pure noise is almost entirely references, each one pointing somewhere arbitrary in
			// the dictionary.
			var random = new Random (20260102);
			int productive = 0;

			for (int iteration = 0; iteration < 2000; iteration++) {
				var contents = new byte[1 + random.Next (2048)];

				random.NextBytes (contents);

				var stream = new byte[16 + contents.Length];

				BinaryPrimitives.WriteInt32LittleEndian (stream.AsSpan (0, 4), contents.Length + 12);
				BinaryPrimitives.WriteInt32LittleEndian (stream.AsSpan (4, 4), random.Next ());
				BinaryPrimitives.WriteInt32LittleEndian (stream.AsSpan (8, 4), (int) RtfCompressionMode.Compressed);
				BinaryPrimitives.WriteInt32LittleEndian (stream.AsSpan (12, 4), random.Next ());
				contents.CopyTo (stream.AsSpan (16));

				if (AssertInvariants (stream, int.MaxValue, $"iteration {iteration}") > 0)
					productive++;
			}

			Assert.That (productive, Is.GreaterThan (1000), "iterations that produced output");
		}

		[TestCase (1)]
		[TestCase (2)]
		[TestCase (5)]
		public void TestCorruptedStreamsFedInSmallChunks (int chunkSize)
		{
			// Resuming in the middle of a header field or a control token uses a different code path than
			// consuming one in a single call, so the corrupted streams need to be run through both.
			var random = new Random (20260103);
			var stream = WellFormedStream (4, 300);

			for (int iteration = 0; iteration < 250; iteration++) {
				var mutated = (byte[]) stream.Clone ();
				int flips = 1 + random.Next (8);

				for (int i = 0; i < flips; i++)
					mutated[random.Next (mutated.Length)] ^= (byte) (1 << random.Next (8));

				AssertInvariants (mutated, chunkSize, $"iteration {iteration}");
			}
		}

		[Test]
		public void TestUnknownCompressionTypes ()
		{
			var random = new Random (20260104);
			var stream = WellFormedStream (5, 128);

			for (int iteration = 0; iteration < 500; iteration++) {
				var mutated = (byte[]) stream.Clone ();

				BinaryPrimitives.WriteInt32LittleEndian (mutated.AsSpan (8, 4), random.Next (int.MinValue, int.MaxValue));

				AssertInvariants (mutated, int.MaxValue, $"iteration {iteration}");
			}
		}
	}
}

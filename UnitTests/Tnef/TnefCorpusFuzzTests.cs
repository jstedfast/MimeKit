//
// TnefCorpusFuzzTests.cs
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
	/// <summary>
	/// Mutates the real-world TNEF files in TestData/tnef and asserts that the reader upholds the
	/// same invariants on the result as it does on a hand-crafted malformed stream.
	/// </summary>
	/// <remarks>
	/// <para><see cref="TnefFuzzTests"/> covers malformed streams that were written by hand to target a
	/// specific part of the format. This fixture complements it by starting from input that is known to
	/// be structurally valid and damaging it at random, which reaches combinations that are tedious to
	/// enumerate by hand: a plausible attribute header followed by a corrupt payload, a length field
	/// that disagrees with a checksum that is otherwise correct, and so on.</para>
	/// <para>Every campaign is seeded from a constant, so a failure is reproducible: the assertion
	/// message names the seed file, the mutator and the iteration, and
	/// <see cref="TestReproduceSingleMutation"/> will replay exactly that case.</para>
	/// </remarks>
	[TestFixture]
	public class TnefCorpusFuzzTests
	{
		// Changing this re-rolls every campaign in this fixture. Do not change it casually: the point of
		// a fixed seed is that CI runs the same bytes every time.
		const int BaseSeed = 20260102;

		// Keeps a CI run to a few seconds. The explicit campaign below is the one that goes deep.
		const int IterationsPerSeed = 48;

		// Mutating a megabyte-sized seed is mostly a test of how fast we can copy a megabyte, so the
		// larger corpus files get proportionally fewer iterations.
		const int LargeSeedThreshold = 128 * 1024;
		const int LargeSeedIterations = 8;

		#region Mutators

		public enum Mutator
		{
			/// <summary>Flip individual bits.</summary>
			BitFlip,

			/// <summary>Overwrite a run of bytes with noise.</summary>
			Randomize,

			/// <summary>Overwrite a run of bytes with zeros.</summary>
			ZeroRun,

			/// <summary>Overwrite a run of bytes with 0xFF, which maximizes any length or count field it lands on.</summary>
			SaturateRun,

			/// <summary>Cut the stream short.</summary>
			Truncate,

			/// <summary>Append trailing noise.</summary>
			Extend,

			/// <summary>Delete a run of bytes, shifting everything after it.</summary>
			DeleteRun,

			/// <summary>Insert a run of noise, shifting everything after it.</summary>
			InsertRun,

			/// <summary>Copy one region of the stream over another, splicing real structure into the wrong place.</summary>
			SpliceChunk,

			/// <summary>Swap two equally sized regions.</summary>
			SwapChunks,

			/// <summary>Overwrite a 32-bit field with a hostile value. Targets length, count and offset fields.</summary>
			HostileDword,

			/// <summary>Overwrite a 16-bit field with a hostile value. Targets checksums and property types.</summary>
			HostileWord
		}

		public static IEnumerable<Mutator> Mutators => Enum.GetValues (typeof (Mutator)).Cast<Mutator> ();

		static readonly uint[] HostileDwords = {
			0xFFFFFFFF, 0x7FFFFFFF, 0x80000000, 0xFFFFFFFE, 0x00000000, 0x00000001,
			0x7FFFFFFE, 0x40000000, 0x10000000, 0x0FFFFFFF, 0xF0000000, 0x00FFFFFF
		};

		static readonly ushort[] HostileWords = {
			0xFFFF, 0x7FFF, 0x8000, 0x0000, 0x0001, 0xFFFE, 0x1000, 0x101E, 0x001F, 0x0102
		};

		static int NextRun (Random random, int length)
		{
			// Favour short runs: a handful of damaged bytes is far more likely to leave a stream that
			// still parses most of the way through, which is where the interesting behaviour is.
			int max = Math.Max (1, Math.Min (length, random.Next (4) == 0 ? length : 32));

			return random.Next (1, max + 1);
		}

		/// <summary>
		/// Apply a single mutation to a copy of <paramref name="seed"/>.
		/// </summary>
		public static byte[] Mutate (Mutator mutator, byte[] seed, Random random)
		{
			if (seed.Length == 0)
				return seed;

			byte[] data;
			int offset, count;

			switch (mutator) {
			case Mutator.BitFlip:
				data = (byte[]) seed.Clone ();
				count = random.Next (1, 9);
				for (int i = 0; i < count; i++) {
					offset = random.Next (data.Length);
					data[offset] ^= (byte) (1 << random.Next (8));
				}
				return data;

			case Mutator.Randomize:
				data = (byte[]) seed.Clone ();
				offset = random.Next (data.Length);
				count = NextRun (random, data.Length - offset);
				for (int i = 0; i < count; i++)
					data[offset + i] = (byte) random.Next (256);
				return data;

			case Mutator.ZeroRun:
				data = (byte[]) seed.Clone ();
				offset = random.Next (data.Length);
				count = NextRun (random, data.Length - offset);
				Array.Clear (data, offset, count);
				return data;

			case Mutator.SaturateRun:
				data = (byte[]) seed.Clone ();
				offset = random.Next (data.Length);
				count = NextRun (random, data.Length - offset);
				for (int i = 0; i < count; i++)
					data[offset + i] = 0xFF;
				return data;

			case Mutator.Truncate:
				// Bias towards cutting near the front, where the header and the first attributes live.
				offset = random.Next (4) == 0 ? random.Next (seed.Length) : random.Next (Math.Min (seed.Length, 1024));
				data = new byte[offset];
				Buffer.BlockCopy (seed, 0, data, 0, offset);
				return data;

			case Mutator.Extend:
				count = random.Next (1, 257);
				data = new byte[seed.Length + count];
				Buffer.BlockCopy (seed, 0, data, 0, seed.Length);
				for (int i = 0; i < count; i++)
					data[seed.Length + i] = (byte) random.Next (256);
				return data;

			case Mutator.DeleteRun:
				offset = random.Next (seed.Length);
				count = NextRun (random, seed.Length - offset);
				data = new byte[seed.Length - count];
				Buffer.BlockCopy (seed, 0, data, 0, offset);
				Buffer.BlockCopy (seed, offset + count, data, offset, seed.Length - (offset + count));
				return data;

			case Mutator.InsertRun:
				offset = random.Next (seed.Length + 1);
				count = random.Next (1, 65);
				data = new byte[seed.Length + count];
				Buffer.BlockCopy (seed, 0, data, 0, offset);
				for (int i = 0; i < count; i++)
					data[offset + i] = (byte) random.Next (256);
				Buffer.BlockCopy (seed, offset, data, offset + count, seed.Length - offset);
				return data;

			case Mutator.SpliceChunk:
				data = (byte[]) seed.Clone ();
				int source = random.Next (data.Length);
				int destination = random.Next (data.Length);
				count = NextRun (random, data.Length - Math.Max (source, destination));
				Buffer.BlockCopy (seed, source, data, destination, count);
				return data;

			case Mutator.SwapChunks:
				data = (byte[]) seed.Clone ();
				int first = random.Next (data.Length);
				int second = random.Next (data.Length);
				count = NextRun (random, data.Length - Math.Max (first, second));
				Buffer.BlockCopy (seed, first, data, second, count);
				Buffer.BlockCopy (seed, second, data, first, count);
				return data;

			case Mutator.HostileDword:
				data = (byte[]) seed.Clone ();
				if (data.Length < 4)
					return data;
				offset = random.Next (data.Length - 3);
				uint dword = HostileDwords[random.Next (HostileDwords.Length)];
				data[offset] = (byte) dword;
				data[offset + 1] = (byte) (dword >> 8);
				data[offset + 2] = (byte) (dword >> 16);
				data[offset + 3] = (byte) (dword >> 24);
				return data;

			case Mutator.HostileWord:
				data = (byte[]) seed.Clone ();
				if (data.Length < 2)
					return data;
				offset = random.Next (data.Length - 1);
				ushort word = HostileWords[random.Next (HostileWords.Length)];
				data[offset] = (byte) word;
				data[offset + 1] = (byte) (word >> 8);
				return data;

			default:
				throw new ArgumentOutOfRangeException (nameof (mutator));
			}
		}

		#endregion

		#region Corpus

		static string CorpusDirectory => Path.Combine (TestHelper.ProjectDir, "TestData", "tnef");

		public static IEnumerable<string> SeedFiles ()
		{
			// Ordered so that the set of bytes a given seed is fuzzed with does not depend on the order
			// the file system happens to hand them back in.
			return Directory.EnumerateFiles (CorpusDirectory, "*.tnef").OrderBy (Path.GetFileName, StringComparer.Ordinal);
		}

		public static IEnumerable<TestCaseData> SeedCases ()
		{
			foreach (var path in SeedFiles ())
				yield return new TestCaseData (Path.GetFileName (path)).SetArgDisplayNames (Path.GetFileName (path));
		}

		// Every seed is damaged with the same sequence of bytes regardless of which seeds ran before it.
		static Random CreateRandom (string fileName, int salt)
		{
			int seed = BaseSeed ^ salt;

			foreach (var c in fileName)
				seed = (seed * 31) + c;

			return new Random (seed);
		}

		/// <summary>
		/// Count the attributes a stream yields in Loose mode, swallowing anything it throws.
		/// </summary>
		/// <remarks>
		/// Used only to measure how much of a mutated stream the reader still gets through. A campaign in
		/// which nothing parses is not testing the parser, so the fixtures below assert against this.
		/// </remarks>
		static int CountAttributes (byte[] data)
		{
			int count = 0;

			try {
				using var stream = new MemoryStream (data, false);
				using var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose);

				while (reader.ReadNextAttribute ())
					count++;
			} catch {
				// The invariant assertions are what police this; here we only want the count.
			}

			return count;
		}

		#endregion

		[Test]
		public void TestCorpusIsPresent ()
		{
			// Guards against the campaigns below silently becoming no-ops if the corpus moves.
			var seeds = SeedFiles ().ToList ();

			Assert.That (seeds, Is.Not.Empty, "No TNEF corpus files were found in " + CorpusDirectory);

			foreach (var seed in seeds)
				Assert.That (new FileInfo (seed).Length, Is.GreaterThan (0), seed + " is empty");
		}

		[Test]
		public void TestUnmutatedCorpusParses ()
		{
			// If the pristine corpus did not parse, a productivity assertion below would be measuring the
			// wrong thing.
			foreach (var path in SeedFiles ()) {
				var data = File.ReadAllBytes (path);
				var name = Path.GetFileName (path);

				Assert.That (CountAttributes (data), Is.GreaterThan (0), name + " yielded no attributes before being mutated");

				foreach (var strategy in TnefFuzzTests.Strategies)
					TnefFuzzTests.AssertInvariants (strategy, data, TnefComplianceMode.Loose, name);
			}
		}

		[TestCaseSource (nameof (SeedCases))]
		public void TestMutatedCorpusUpholdsInvariants (string fileName)
		{
			var seed = File.ReadAllBytes (Path.Combine (CorpusDirectory, fileName));
			var strategies = TnefFuzzTests.Strategies.ToArray ();
			var mutators = Mutators.ToArray ();
			var random = CreateRandom (fileName, 0);
			int iterations = seed.Length >= LargeSeedThreshold ? LargeSeedIterations : IterationsPerSeed;
			int productive = 0;

			for (int i = 0; i < iterations; i++) {
				var mutator = mutators[i % mutators.Length];
				var data = Mutate (mutator, seed, random);

				// Cycle through the strategies and both compliance modes rather than running the full
				// cross product, which would multiply the cost by twelve for very little extra reach.
				var strategy = strategies[i % strategies.Length];
				var mode = (i & 1) == 0 ? TnefComplianceMode.Loose : TnefComplianceMode.Strict;
				var what = $"{fileName} [{mutator} #{i}]";

				TnefFuzzTests.AssertInvariants (strategy, data, mode, what);

				if (CountAttributes (data) > 0)
					productive++;
			}

			// A mutation that damages the signature leaves nothing to parse, which is a legitimate outcome
			// but not an interesting one. If almost every iteration ended up there, the campaign has
			// degenerated into a test of the header check and would no longer notice a regression deeper
			// in the reader.
			Assert.That (productive, Is.GreaterThan (iterations / 4), $"only {productive} of {iterations} mutations of {fileName} still parsed; the campaign is not reaching the reader");
		}

		[TestCaseSource (nameof (SeedCases))]
		public void TestTruncatedCorpusUpholdsInvariants (string fileName)
		{
			var seed = File.ReadAllBytes (Path.Combine (CorpusDirectory, fileName));
			var strategies = TnefFuzzTests.Strategies.ToArray ();

			// Truncation deserves exhaustive-ish treatment rather than random sampling: every truncation
			// offset is a distinct "stream ends in the middle of this field" case, and the first kilobyte
			// covers the header plus the first several attributes of every file in the corpus.
			int limit = Math.Min (seed.Length, 1024);
			int step = 1;

			for (int length = 0; length <= limit; length += step) {
				var data = new byte[length];

				Buffer.BlockCopy (seed, 0, data, 0, length);

				var strategy = strategies[length % strategies.Length];
				var mode = (length & 1) == 0 ? TnefComplianceMode.Loose : TnefComplianceMode.Strict;

				TnefFuzzTests.AssertInvariants (strategy, data, mode, $"{fileName} truncated to {length}");
			}
		}

		[TestCaseSource (nameof (SeedCases))]
		public void TestCorpusWithHostileLengthFieldsUpholdsInvariants (string fileName)
		{
			var seed = File.ReadAllBytes (Path.Combine (CorpusDirectory, fileName));
			var strategies = TnefFuzzTests.Strategies.ToArray ();
			var random = CreateRandom (fileName, 0x5EED);
			int iterations = seed.Length >= LargeSeedThreshold ? LargeSeedIterations : IterationsPerSeed;
			int productive = 0;

			// Length and count fields are the ones an attacker actually reaches for, because they are what
			// the reader turns into an allocation. Hammer them specifically rather than relying on a
			// uniformly random offset landing on one.
			for (int i = 0; i < iterations; i++) {
				var data = Mutate ((i & 1) == 0 ? Mutator.HostileDword : Mutator.HostileWord, seed, random);
				var strategy = strategies[i % strategies.Length];
				var mode = (i & 2) == 0 ? TnefComplianceMode.Loose : TnefComplianceMode.Strict;

				TnefFuzzTests.AssertInvariants (strategy, data, mode, $"{fileName} [hostile field #{i}]");

				if (CountAttributes (data) > 0)
					productive++;
			}

			Assert.That (productive, Is.GreaterThan (iterations / 4), $"only {productive} of {iterations} hostile-field mutations of {fileName} still parsed");
		}

		[Test]
		public void TestReproduceSingleMutation ()
		{
			// Replays the first mutation of the first seed. This exists so that a failure reported by the
			// campaigns above can be reduced to a single case by editing the three values below.
			const string fileName = "body.tnef";
			const Mutator mutator = Mutator.BitFlip;
			const int iteration = 0;

			var seed = File.ReadAllBytes (Path.Combine (CorpusDirectory, fileName));
			var random = CreateRandom (fileName, 0);
			byte[] data = null;

			for (int i = 0; i <= iteration; i++)
				data = Mutate (mutator, seed, random);

			foreach (var strategy in TnefFuzzTests.Strategies) {
				TnefFuzzTests.AssertInvariants (strategy, data, TnefComplianceMode.Loose, fileName);
				TnefFuzzTests.AssertInvariants (strategy, data, TnefComplianceMode.Strict, fileName);
			}
		}

		[Test]
		[Explicit ("Long running fuzzing campaign. Run manually.")]
		public void TestLongFuzzingCampaign ()
		{
			const int iterationsPerSeed = 2000;

			var strategies = TnefFuzzTests.Strategies.ToArray ();
			var mutators = Mutators.ToArray ();
			int productive = 0, total = 0;

			foreach (var path in SeedFiles ()) {
				var fileName = Path.GetFileName (path);
				var seed = File.ReadAllBytes (path);
				var random = CreateRandom (fileName, 0x10);

				for (int i = 0; i < iterationsPerSeed; i++) {
					var mutator = mutators[random.Next (mutators.Length)];
					var data = Mutate (mutator, seed, random);

					foreach (var strategy in strategies) {
						foreach (var mode in new[] { TnefComplianceMode.Loose, TnefComplianceMode.Strict })
							TnefFuzzTests.AssertInvariants (strategy, data, mode, $"{fileName} [{mutator} #{i}]");
					}

					total++;

					if (CountAttributes (data) > 0)
						productive++;
				}
			}

			Assert.That (productive, Is.GreaterThan (total / 4), $"only {productive} of {total} mutations still parsed");
		}
	}
}

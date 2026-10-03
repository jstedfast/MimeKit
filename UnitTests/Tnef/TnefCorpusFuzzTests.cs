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

using MimeKit;
using MimeKit.Tnef;

namespace UnitTests.Tnef {
	[TestFixture]
	public class TnefCorpusFuzzTests
	{
		const int BaseSeed = 20260102;
		const int IterationsPerSeed = 48;
		const int LargeSeedThreshold = 128 * 1024;
		const int LargeSeedIterations = 8;

		public enum Mutator
		{
			BitFlip,
			Randomize,
			ZeroRun,
			SaturateRun,
			Truncate,
			Extend,
			DeleteRun,
			InsertRun,
			SpliceChunk,
			SwapChunks,
			HostileDword,
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
			int max = Math.Max (1, Math.Min (length, random.Next (4) == 0 ? length : 32));

			return random.Next (1, max + 1);
		}

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

		static string CorpusDirectory => Path.Combine (TestHelper.ProjectDir, "TestData", "tnef");

		public static IEnumerable<string> SeedFiles ()
		{
			return Directory.EnumerateFiles (CorpusDirectory, "*.tnef").OrderBy (Path.GetFileName, StringComparer.Ordinal);
		}

		public static IEnumerable<TestCaseData> SeedCases ()
		{
			foreach (var path in SeedFiles ())
				yield return new TestCaseData (Path.GetFileName (path)).SetArgDisplayNames (Path.GetFileName (path));
		}

		static Random CreateRandom (string fileName, int salt)
		{
			int seed = BaseSeed ^ salt;

			foreach (var c in fileName)
				seed = (seed * 31) + c;

			return new Random (seed);
		}

		static int CountAttributes (byte[] data)
		{
			int count = 0;

			using var stream = new MemoryStream (data, false);
			using var reader = new TnefReader (stream) { ComplianceLogger = new TestTnefComplianceLogger () };

			while (reader.Read ())
				count++;

			return count;
		}

		static MimeMessage ConvertToMime (byte[] data)
		{
			return TnefConversionTestHelper.ConvertArbitrary (data);
		}

		[Test]
		public void TestCorpusIsPresent ()
		{
			var seeds = SeedFiles ().ToList ();

			Assert.That (seeds, Is.Not.Empty, "No TNEF corpus files were found in " + CorpusDirectory);

			foreach (var seed in seeds)
				Assert.That (new FileInfo (seed).Length, Is.GreaterThan (0), seed + " is empty");
		}

		[Test]
		public void TestUnmutatedCorpusParses ()
		{
			foreach (var path in SeedFiles ()) {
				var data = File.ReadAllBytes (path);
				var name = Path.GetFileName (path);

				Assert.That (CountAttributes (data), Is.GreaterThan (0), name + " yielded no attributes before being mutated");
				TnefFuzzTests.AssertInvariants (data, name);
			}
		}

		[Test]
		public async Task TestUnmutatedCorpusParsesAsync ()
		{
			foreach (var path in SeedFiles ()) {
				var data = File.ReadAllBytes (path);
				var name = Path.GetFileName (path);

				await TnefFuzzTests.AssertInvariantsAsync (data, name).ConfigureAwait (false);
			}
		}

		[TestCaseSource (nameof (SeedCases))]
		public void TestMutatedCorpusUpholdsInvariants (string fileName)
		{
			var seed = File.ReadAllBytes (Path.Combine (CorpusDirectory, fileName));
			var mutators = Mutators.ToArray ();
			var random = CreateRandom (fileName, 0);
			int iterations = seed.Length >= LargeSeedThreshold ? LargeSeedIterations : IterationsPerSeed;
			int productive = 0;

			for (int i = 0; i < iterations; i++) {
				var mutator = mutators[i % mutators.Length];
				var data = Mutate (mutator, seed, random);
				var what = $"{fileName} [{mutator} #{i}]";

				TnefFuzzTests.AssertInvariants (data, what);

				if (CountAttributes (data) > 0)
					productive++;
			}

			Assert.That (productive, Is.GreaterThan (iterations / 4), $"only {productive} of {iterations} mutations of {fileName} still parsed; the campaign is not reaching the reader");
		}

		[TestCaseSource (nameof (SeedCases))]
		public async Task TestMutatedCorpusUpholdsInvariantsAsync (string fileName)
		{
			var seed = File.ReadAllBytes (Path.Combine (CorpusDirectory, fileName));
			var mutators = Mutators.ToArray ();
			var random = CreateRandom (fileName, 0);
			int iterations = seed.Length >= LargeSeedThreshold ? LargeSeedIterations : IterationsPerSeed;

			for (int i = 0; i < iterations; i++) {
				var mutator = mutators[i % mutators.Length];
				var data = Mutate (mutator, seed, random);

				await TnefFuzzTests.AssertInvariantsAsync (data, $"{fileName} [{mutator} #{i}]").ConfigureAwait (false);
			}
		}

		[TestCaseSource (nameof (SeedCases))]
		public void TestTruncatedCorpusUpholdsInvariants (string fileName)
		{
			var seed = File.ReadAllBytes (Path.Combine (CorpusDirectory, fileName));
			int limit = Math.Min (seed.Length, 1024);

			for (int length = 0; length <= limit; length++) {
				var data = new byte[length];

				Buffer.BlockCopy (seed, 0, data, 0, length);
				TnefFuzzTests.AssertInvariants (data, $"{fileName} truncated to {length}");
			}
		}

		[TestCaseSource (nameof (SeedCases))]
		public async Task TestTruncatedCorpusUpholdsInvariantsAsync (string fileName)
		{
			var seed = File.ReadAllBytes (Path.Combine (CorpusDirectory, fileName));
			int limit = Math.Min (seed.Length, 1024);

			for (int length = 0; length <= limit; length++) {
				var data = new byte[length];

				Buffer.BlockCopy (seed, 0, data, 0, length);
				await TnefFuzzTests.AssertInvariantsAsync (data, $"{fileName} truncated to {length}").ConfigureAwait (false);
			}
		}

		[TestCaseSource (nameof (SeedCases))]
		public void TestCorpusWithHostileLengthFieldsUpholdsInvariants (string fileName)
		{
			var seed = File.ReadAllBytes (Path.Combine (CorpusDirectory, fileName));
			var random = CreateRandom (fileName, 0x5EED);
			int iterations = seed.Length >= LargeSeedThreshold ? LargeSeedIterations : IterationsPerSeed;
			int productive = 0;

			for (int i = 0; i < iterations; i++) {
				var data = Mutate ((i & 1) == 0 ? Mutator.HostileDword : Mutator.HostileWord, seed, random);

				TnefFuzzTests.AssertInvariants (data, $"{fileName} [hostile field #{i}]");

				if (CountAttributes (data) > 0)
					productive++;
			}

			Assert.That (productive, Is.GreaterThan (iterations / 4), $"only {productive} of {iterations} hostile-field mutations of {fileName} still parsed");
		}

		[TestCaseSource (nameof (SeedCases))]
		public async Task TestCorpusWithHostileLengthFieldsUpholdsInvariantsAsync (string fileName)
		{
			var seed = File.ReadAllBytes (Path.Combine (CorpusDirectory, fileName));
			var random = CreateRandom (fileName, 0x5EED);
			int iterations = seed.Length >= LargeSeedThreshold ? LargeSeedIterations : IterationsPerSeed;

			for (int i = 0; i < iterations; i++) {
				var data = Mutate ((i & 1) == 0 ? Mutator.HostileDword : Mutator.HostileWord, seed, random);

				await TnefFuzzTests.AssertInvariantsAsync (data, $"{fileName} [hostile field #{i}]").ConfigureAwait (false);
			}
		}

		[Test]
		public void TestReproduceSingleMutation ()
		{
			const string fileName = "body.tnef";
			const Mutator mutator = Mutator.BitFlip;
			const int iteration = 0;

			var seed = File.ReadAllBytes (Path.Combine (CorpusDirectory, fileName));
			var random = CreateRandom (fileName, 0);
			byte[] data = null;

			for (int i = 0; i <= iteration; i++)
				data = Mutate (mutator, seed, random);

			TnefFuzzTests.AssertInvariants (data, fileName);
		}

		[Test]
		public async Task TestReproduceSingleMutationAsync ()
		{
			const string fileName = "body.tnef";
			const Mutator mutator = Mutator.BitFlip;
			const int iteration = 0;

			var seed = File.ReadAllBytes (Path.Combine (CorpusDirectory, fileName));
			var random = CreateRandom (fileName, 0);
			byte[] data = null;

			for (int i = 0; i <= iteration; i++)
				data = Mutate (mutator, seed, random);

			await TnefFuzzTests.AssertInvariantsAsync (data, fileName).ConfigureAwait (false);
		}

		[TestCaseSource (nameof (SeedCases))]
		public void TestMutatedCorpusConvertToMimeUpholdsInvariants (string fileName)
		{
			var seed = File.ReadAllBytes (Path.Combine (CorpusDirectory, fileName));
			var random = CreateRandom (fileName, 0);
			var data = Mutate (Mutator.BitFlip, seed, random);

			Assert.DoesNotThrow (() => ConvertToMime (data).Dispose (), fileName);
		}

		[Test]
		[Explicit ("Long running fuzzing campaign. Run manually.")]
		public void TestLongFuzzingCampaign ()
		{
			const int iterationsPerSeed = 2000;

			var mutators = Mutators.ToArray ();
			int productive = 0, total = 0;

			foreach (var path in SeedFiles ()) {
				var fileName = Path.GetFileName (path);
				var seed = File.ReadAllBytes (path);
				var random = CreateRandom (fileName, 0x10);

				for (int i = 0; i < iterationsPerSeed; i++) {
					var mutator = mutators[random.Next (mutators.Length)];
					var data = Mutate (mutator, seed, random);

					TnefFuzzTests.AssertInvariants (data, $"{fileName} [{mutator} #{i}]");
					total++;

					if (CountAttributes (data) > 0)
						productive++;
				}
			}

			Assert.That (productive, Is.GreaterThan (total / 4), $"only {productive} of {total} mutations still parsed");
		}
	}
}

//
// Program.cs
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

using System.Diagnostics;

using SharpFuzz;

namespace MimeKit.Fuzzing {
	// Entry point for the coverage-guided fuzzing harness.
	//
	// Modes:
	//   (no arguments)                   Run under libfuzzer-dotnet. The target is selected by the
	//                                    MIMEKIT_FUZZ_TARGET environment variable.
	//   --list                           Print "name<TAB>max_len" for every target (consumed by scripts/fuzz.ps1).
	//   --replay <target> <path>...      Run files (or every file in directories) through a target without
	//                                    instrumentation. Used to reproduce crashes and to smoke-test seeds.
	//   --extract-rtf <bin-dir> <rtf-dir> <path>...
	//                                    Extract PidTagRtfCompressed values (and their decompressed RTF) from
	//                                    TNEF files. Used by scripts/fuzz-seed-corpora.ps1.
	//   --generate-dkim-seeds <output-dir> <rsa-key.pem> <path>...
	//                                    Sign messages with DKIM and ARC and prepend a DNS zone with the keys, to
	//                                    seed the dkim-arc-verifier and dmarc-verifier corpora.
	static class Program
	{
		const string TargetVariable = "MIMEKIT_FUZZ_TARGET";
		static readonly TimeSpan SlowInputThreshold = TimeSpan.FromSeconds (2);

		static int Main (string[] args)
		{
			if (args.Length > 0 && args[0] == "--list") {
				foreach (var target in FuzzTargets.All)
					Console.WriteLine ($"{target.Name}\t{target.MaxLength}");
				return 0;
			}

			if (args.Length > 0 && args[0] == "--replay")
				return Replay (args);

			if (args.Length > 0 && args[0] == "--extract-rtf")
				return Helpers.RtfSeedExtractor.Run (args);

			if (args.Length > 0 && args[0] == "--generate-dkim-seeds")
				return Helpers.DkimSeedGenerator.Run (args);

			var name = Environment.GetEnvironmentVariable (TargetVariable);

			if (string.IsNullOrEmpty (name)) {
				Console.Error.WriteLine ($"The {TargetVariable} environment variable must be set to one of: {string.Join (", ", FuzzTargets.All.Select (t => t.Name))}");
				return 2;
			}

			if (!FuzzTargets.TryGet (name, out var fuzzTarget)) {
				Console.Error.WriteLine ($"Unknown fuzz target: {name}");
				return 2;
			}

			fuzzTarget.Initialize ();

			Fuzzer.LibFuzzer.Run (fuzzTarget.Run);

			return 0;
		}

		static int Replay (string[] args)
		{
			if (args.Length < 3) {
				Console.Error.WriteLine ("Usage: MimeKit.Fuzzing --replay <target|all> <file|directory>...");
				return 2;
			}

			IReadOnlyList<IFuzzTarget> targets;

			if (args[1] == "all") {
				targets = FuzzTargets.All;
			} else if (FuzzTargets.TryGet (args[1], out var single)) {
				targets = new[] { single };
			} else {
				Console.Error.WriteLine ($"Unknown fuzz target: {args[1]}");
				return 2;
			}

			var files = new List<string> ();

			for (int i = 2; i < args.Length; i++) {
				if (Directory.Exists (args[i]))
					files.AddRange (Directory.EnumerateFiles (args[i], "*", SearchOption.AllDirectories).OrderBy (f => f, StringComparer.Ordinal));
				else if (File.Exists (args[i]))
					files.Add (args[i]);
				else
					Console.Error.WriteLine ($"warning: {args[i]} does not exist");
			}

			int failures = 0, slow = 0, count = 0;

			foreach (var target in targets) {
				foreach (var file in files) {
					var data = File.ReadAllBytes (file);

					if (data.Length > target.MaxLength)
						continue;

					var stopwatch = Stopwatch.StartNew ();
					count++;

					try {
						target.Run (data);
					} catch (Exception ex) {
						Console.WriteLine ($"FAIL {target.Name} {file}");
						Console.WriteLine (ex);
						failures++;
						continue;
					}

					stopwatch.Stop ();

					if (stopwatch.Elapsed > SlowInputThreshold) {
						Console.WriteLine ($"SLOW {target.Name} {file} ({stopwatch.Elapsed.TotalMilliseconds:F0} ms)");
						slow++;
					}
				}
			}

			Console.WriteLine ($"Replayed {count} input(s): {failures} failure(s), {slow} slow.");

			return failures > 0 ? 1 : 0;
		}
	}
}

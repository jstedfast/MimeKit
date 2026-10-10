//
// RtfSeedExtractor.cs
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
using MimeKit.Fuzzing.Targets;

namespace MimeKit.Fuzzing.Helpers {
	// Extracts the PidTagRtfCompressed property values (and the RTF that they decompress to) from TNEF files so
	// that scripts/fuzz-seed-corpora.ps1 can seed the rtf-compressed, rtf-tokenizer and rtf-converters corpora.
	static class RtfSeedExtractor
	{
		public static int Run (string[] args)
		{
			if (args.Length < 4) {
				Console.Error.WriteLine ("Usage: MimeKit.Fuzzing --extract-rtf <compressed-output-dir> <rtf-output-dir> <file|directory>...");
				return 2;
			}

			Directory.CreateDirectory (args[1]);
			Directory.CreateDirectory (args[2]);

			int count = 0;

			for (int i = 3; i < args.Length; i++) {
				IEnumerable<string> files;

				if (Directory.Exists (args[i]))
					files = Directory.EnumerateFiles (args[i], "*", SearchOption.AllDirectories).OrderBy (f => f, StringComparer.Ordinal);
				else
					files = new[] { args[i] };

				foreach (var file in files) {
					var values = new List<byte[]> ();

					try {
						using (var stream = File.OpenRead (file)) {
							using (var reader = new TnefReader (stream))
								Collect (reader, values, 0);
						}
					} catch (Exception ex) when (ex is TnefException || ex is IOException || ex is UnauthorizedAccessException) {
						continue;
					}

					for (int j = 0; j < values.Count; j++) {
						var name = $"{count:D4}-{Path.GetFileNameWithoutExtension (file)}";
						var rtf = RtfCompressedTarget.Decompress (values[j], values[j].Length, out _, out _);

						File.WriteAllBytes (Path.Combine (args[1], name + ".bin"), values[j]);

						if (rtf.Length > 0)
							File.WriteAllBytes (Path.Combine (args[2], name + ".rtf"), rtf);

						count++;
					}
				}
			}

			Console.WriteLine ($"Extracted {count} PidTagRtfCompressed value(s).");

			return 0;
		}

		static void Collect (TnefReader reader, List<byte[]> values, int depth)
		{
			while (reader.Read ()) {
				if (!TnefWalker.ContainsProperties (reader.Tag))
					continue;

				var properties = reader.GetPropertyReader ();
				bool table = reader.Tag == TnefAttributeTag.RecipientTable;

				if (table) {
					while (properties.ReadNextRow ()) {
						while (properties.ReadNextProperty ())
							CollectProperty (properties, values, depth);
					}
				} else {
					while (properties.ReadNextProperty ())
						CollectProperty (properties, values, depth);
				}
			}
		}

		static void CollectProperty (TnefPropertyReader properties, List<byte[]> values, int depth)
		{
			if (properties.IsEmbeddedMessage && depth < 8) {
				using (var embedded = properties.OpenEmbeddedMessage ())
					Collect (embedded, values, depth + 1);
			} else if (properties.Tag.Id == TnefPropertyId.RtfCompressed && properties.ValueCount > 0) {
				values.Add (properties.ReadValueAsBytes ());
			}
		}
	}
}

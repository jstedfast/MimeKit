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

// Generates MimeKit/Utils/SmtpUtf8AddressValidator.g.cs and MimeKit/Utils/UnicodeNormalization.g.cs from the
// Unicode Character Database.
//
// Usage: dotnet run --project tools/SmtpUtf8TableGenerator [-- <ucd-directory> [<output-directory> [<test-output>]]]
//
// It also generates UnitTests/TestData/unicode/NormalizationTest.txt.gz, the data columns of
// NormalizationTest.txt, which the unit tests use to verify the NFC check.
//
// If no UCD directory is given, the files are downloaded from unicode.org into a cache
// directory under the system temp directory.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.IO.Compression;
using System.Collections.Generic;

namespace SmtpUtf8TableGenerator {
	// Must match SmtpUtf8AddressValidator.PrecisClass
	enum PrecisClass : byte
	{
		Disallowed = 0,
		PValid = 1,
		ContextJ = 2,
		ContextO = 3
	}

	// Must match SmtpUtf8AddressValidator.JoiningType
	enum JoiningType : byte
	{
		NonJoining = 0,
		Left = 1,
		Dual = 2,
		Right = 3,
		Transparent = 4,
		JoinCausing = 5
	}

	// Must match UnicodeNormalization.NormalizationQuickCheck
	enum QuickCheck : byte
	{
		Yes = 0,
		Maybe = 1,
		No = 2
	}

	static class Program
	{
		const string UnicodeVersion = "18.0.0";
		const int MaxCodePoint = 0x10FFFF;
		const int CodePointCount = MaxCodePoint + 1;
		const byte ViramaCombiningClass = 9;

		const string LicenseHeader = @"//
// {0}
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the ""Software""), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
";

		static readonly string[] UcdFiles = {
			"UnicodeData.txt",
			"Scripts.txt",
			"PropList.txt",
			"DerivedCoreProperties.txt",
			"DerivedNormalizationProps.txt",
			"HangulSyllableType.txt",
			"extracted/DerivedJoiningType.txt",
			"NormalizationTest.txt",
		};

		static int Main (string[] args)
		{
			var ucd = args.Length > 0 ? args[0] : Path.Combine (Path.GetTempPath (), "MimeKit-UCD-" + UnicodeVersion);
			var outputDirectory = Path.GetFullPath (Path.Combine (AppContext.BaseDirectory, "..", "..", "..", "..", "..", "MimeKit", "Utils"));
			var testOutput = Path.GetFullPath (Path.Combine (AppContext.BaseDirectory, "..", "..", "..", "..", "..", "UnitTests", "TestData", "unicode", "NormalizationTest.txt.gz"));

			if (args.Length > 1)
				outputDirectory = args[1];

			if (args.Length > 2)
				testOutput = args[2];

			Download (ucd);

			var generalCategory = new string[CodePointCount];
			var combiningClass = new byte[CodePointCount];
			var decompositions = new Dictionary<int, int[]> ();
			Array.Fill (generalCategory, "Cn");

			ParseUnicodeData (Path.Combine (ucd, "UnicodeData.txt"), generalCategory, combiningClass, decompositions);

			var scriptNames = new List<string> { "Unknown", "Common", "Inherited" };
			var scripts = new string[CodePointCount];
			Array.Fill (scripts, "Unknown");

			ParseProperty (Path.Combine (ucd, "Scripts.txt"), (start, end, fields) => {
				for (int cp = start; cp <= end; cp++)
					scripts[cp] = fields[1];
			});

			foreach (var name in scripts.Distinct ().Where (x => !scriptNames.Contains (x)).OrderBy (x => x, StringComparer.Ordinal))
				scriptNames.Add (name);

			var noncharacter = ParseBinaryProperty (Path.Combine (ucd, "PropList.txt"), "Noncharacter_Code_Point");
			var joinControl = ParseBinaryProperty (Path.Combine (ucd, "PropList.txt"), "Join_Control");
			var defaultIgnorable = ParseBinaryProperty (Path.Combine (ucd, "DerivedCoreProperties.txt"), "Default_Ignorable_Code_Point");

			// RFC 8264, Section 9.17: HasCompat is the set of code points for which toNFKC(cp) != cp.
			// A code point whose NFKC_Quick_Check value is No can never occur in NFKC-normalized text,
			// so it always changes when normalized; Yes and Maybe code points are unchanged in isolation.
			var hasCompat = new bool[CodePointCount];
			ParseProperty (Path.Combine (ucd, "DerivedNormalizationProps.txt"), (start, end, fields) => {
				if (fields[1] == "NFKC_QC" && fields.Length > 2 && fields[2] == "N") {
					for (int cp = start; cp <= end; cp++)
						hasCompat[cp] = true;
				}
			});

			// RFC 5892, Section 2.9: OldHangulJamo is the set of code points with a Hangul_Syllable_Type of L, V or T.
			var oldHangulJamo = new bool[CodePointCount];
			ParseProperty (Path.Combine (ucd, "HangulSyllableType.txt"), (start, end, fields) => {
				if (fields[1] == "L" || fields[1] == "V" || fields[1] == "T") {
					for (int cp = start; cp <= end; cp++)
						oldHangulJamo[cp] = true;
				}
			});

			var joiningTypes = new JoiningType[CodePointCount];
			ParseProperty (Path.Combine (ucd, "extracted", "DerivedJoiningType.txt"), (start, end, fields) => {
				var type = fields[1] switch {
					"L" => JoiningType.Left,
					"D" => JoiningType.Dual,
					"R" => JoiningType.Right,
					"T" => JoiningType.Transparent,
					"C" => JoiningType.JoinCausing,
					"U" => JoiningType.NonJoining,
					_ => throw new FormatException ("Unknown Joining_Type: " + fields[1])
				};

				for (int cp = start; cp <= end; cp++)
					joiningTypes[cp] = type;
			});

			var precis = new PrecisClass[CodePointCount];

			for (int cp = 0; cp <= MaxCodePoint; cp++) {
				precis[cp] = GetPrecisIdentifierClass (cp, generalCategory[cp], noncharacter[cp], joinControl[cp], defaultIgnorable[cp], hasCompat[cp], oldHangulJamo[cp]);
			}

			var nfcQuickCheck = new QuickCheck[CodePointCount];
			var fullCompositionExclusion = ParseBinaryProperty (Path.Combine (ucd, "DerivedNormalizationProps.txt"), "Full_Composition_Exclusion");
			ParseProperty (Path.Combine (ucd, "DerivedNormalizationProps.txt"), (start, end, fields) => {
				if (fields[1] == "NFC_QC") {
					var value = fields[2] switch {
						"M" => QuickCheck.Maybe,
						"N" => QuickCheck.No,
						_ => throw new FormatException ("Unknown NFC_QC value: " + fields[2])
					};

					for (int cp = start; cp <= end; cp++)
						nfcQuickCheck[cp] = value;
				}
			});

			var compositions = GetPrimaryComposites (decompositions, fullCompositionExclusion, nfcQuickCheck, combiningClass);
			int maxDecompositionLength = GetMaxDecompositionLength (decompositions, nfcQuickCheck);

			var output = Path.Combine (outputDirectory, "SmtpUtf8AddressValidator.g.cs");
			var source = GenerateValidatorTables (scriptNames, scripts, precis, joiningTypes, combiningClass);

			File.WriteAllText (output, source, new UTF8Encoding (true));
			Console.WriteLine ("Wrote {0}", output);

			output = Path.Combine (outputDirectory, "UnicodeNormalization.g.cs");
			source = GenerateNormalizationTables (combiningClass, nfcQuickCheck, compositions, maxDecompositionLength);

			File.WriteAllText (output, source, new UTF8Encoding (true));
			Console.WriteLine ("Wrote {0}", output);

			WriteNormalizationTest (Path.Combine (ucd, "NormalizationTest.txt"), testOutput);
			Console.WriteLine ("Wrote {0}", testOutput);

			return 0;
		}

		// UAX #15, Section 3.11: a primary composite is a character that has a canonical decomposition mapping
		// and that is not excluded from composition. Each one is listed as (composite, first, second).
		static List<(int Composite, int First, int Second)> GetPrimaryComposites (Dictionary<int, int[]> decompositions, bool[] fullCompositionExclusion, QuickCheck[] nfcQuickCheck, byte[] combiningClass)
		{
			var compositions = new List<(int Composite, int First, int Second)> ();

			// The validator's quick check skips everything below U+0300 without looking it up.
			for (int cp = 0; cp < 0x300; cp++) {
				if (combiningClass[cp] != 0 || nfcQuickCheck[cp] != QuickCheck.Yes)
					throw new InvalidOperationException ($"U+{cp:X4} is not a stable NFC starter.");
			}

			foreach (var pair in decompositions.OrderBy (x => x.Key)) {
				int cp = pair.Key;

				if (fullCompositionExclusion[cp]) {
					// Characters that are excluded from composition can never occur in NFC text.
					if (nfcQuickCheck[cp] != QuickCheck.No)
						throw new InvalidOperationException ($"U+{cp:X4} is excluded from composition but is not NFC_QC=No.");
					continue;
				}

				// Note: A few NFC_QC=Maybe vowel signs (e.g. U+113C5 TULU-TIGALARI VOWEL SIGN AI) are primary composites,
				// but every primary composite is a starter, so a non-starter never expands when decomposed.
				if (combiningClass[cp] != 0)
					throw new InvalidOperationException ($"U+{cp:X4} is a primary composite but is not a starter.");

				if (pair.Value.Length != 2)
					throw new InvalidOperationException ($"U+{cp:X4} is a primary composite but does not decompose into a pair.");

				compositions.Add ((cp, pair.Value[0], pair.Value[1]));
			}

			// The validator fully decomposes a code point by recursively looking up each half in the list of primary
			// composites, which is only correct if no half has a canonical decomposition that is excluded from composition.
			foreach (var (composite, first, second) in compositions) {
				if ((decompositions.ContainsKey (first) && fullCompositionExclusion[first]) || (decompositions.ContainsKey (second) && fullCompositionExclusion[second]))
					throw new InvalidOperationException ($"U+{composite:X4} decomposes into a code point that is excluded from composition.");
			}

			if (compositions.Count > ushort.MaxValue)
				throw new InvalidOperationException ("Too many primary composites to index with a ushort.");

			return compositions;
		}

		// Gets the maximum length of the full canonical decomposition of any code point that can occur in NFC text.
		static int GetMaxDecompositionLength (Dictionary<int, int[]> decompositions, QuickCheck[] nfcQuickCheck)
		{
			// Hangul syllables decompose into at most 3 conjoining jamo.
			int max = 3;

			foreach (var cp in decompositions.Keys) {
				if (nfcQuickCheck[cp] != QuickCheck.No)
					max = Math.Max (max, GetFullDecompositionLength (cp, decompositions));
			}

			return max;
		}

		static int GetFullDecompositionLength (int cp, Dictionary<int, int[]> decompositions)
		{
			if (!decompositions.TryGetValue (cp, out var mapping))
				return 1;

			int length = 0;

			foreach (var c in mapping)
				length += GetFullDecompositionLength (c, decompositions);

			return length;
		}

		// Writes the data columns (c1 through c5) of each test case in NormalizationTest.txt, without the comments.
		static void WriteNormalizationTest (string path, string output)
		{
			var builder = new StringBuilder ();

			foreach (var line in File.ReadLines (path)) {
				if (line.Length == 0 || line[0] == '#' || line[0] == '@')
					continue;

				var fields = line.Split (';');

				builder.Append (string.Join (";", fields, 0, 5));
				builder.Append ('\n');
			}

			Directory.CreateDirectory (Path.GetDirectoryName (output)!);

			using var stream = File.Create (output);
			using var gzip = new GZipStream (stream, CompressionLevel.SmallestSize);
			var bytes = Encoding.ASCII.GetBytes (builder.ToString ());

			gzip.Write (bytes, 0, bytes.Length);
		}

		static void Download (string directory)
		{
			using var client = new HttpClient ();

			foreach (var file in UcdFiles) {
				var path = Path.Combine (directory, file.Replace ('/', Path.DirectorySeparatorChar));

				if (File.Exists (path))
					continue;

				Directory.CreateDirectory (Path.GetDirectoryName (path)!);

				var url = $"https://www.unicode.org/Public/{UnicodeVersion}/ucd/{file}";
				Console.WriteLine ("Downloading {0}", url);

				var content = client.GetByteArrayAsync (url).GetAwaiter ().GetResult ();
				File.WriteAllBytes (path, content);
			}
		}

		static void ParseUnicodeData (string path, string[] generalCategory, byte[] combiningClass, Dictionary<int, int[]> decompositions)
		{
			int rangeStart = -1;

			foreach (var line in File.ReadLines (path)) {
				if (line.Length == 0)
					continue;

				var fields = line.Split (';');
				int cp = Convert.ToInt32 (fields[0], 16);
				var name = fields[1];
				var gc = fields[2];
				var ccc = byte.Parse (fields[3]);

				// Only canonical decomposition mappings are needed; compatibility mappings begin with a <tag>.
				if (fields[5].Length > 0 && fields[5][0] != '<')
					decompositions.Add (cp, fields[5].Split (' ').Select (x => Convert.ToInt32 (x, 16)).ToArray ());

				if (name.EndsWith (", First>", StringComparison.Ordinal)) {
					rangeStart = cp;
					continue;
				}

				int start = name.EndsWith (", Last>", StringComparison.Ordinal) ? rangeStart : cp;

				for (int i = start; i <= cp; i++) {
					generalCategory[i] = gc;
					combiningClass[i] = ccc;
				}

				rangeStart = -1;
			}
		}

		static void ParseProperty (string path, Action<int, int, string[]> callback)
		{
			foreach (var raw in File.ReadLines (path)) {
				int hash = raw.IndexOf ('#');
				var line = hash >= 0 ? raw.Substring (0, hash) : raw;

				if (line.Trim ().Length == 0)
					continue;

				var fields = line.Split (';').Select (x => x.Trim ()).ToArray ();
				var range = fields[0].Split ("..");
				int start = Convert.ToInt32 (range[0], 16);
				int end = range.Length > 1 ? Convert.ToInt32 (range[1], 16) : start;

				callback (start, end, fields);
			}
		}

		static bool[] ParseBinaryProperty (string path, string property)
		{
			var values = new bool[CodePointCount];

			ParseProperty (path, (start, end, fields) => {
				if (fields[1] == property) {
					for (int cp = start; cp <= end; cp++)
						values[cp] = true;
				}
			});

			return values;
		}

		// RFC 8264, Section 8: the PRECIS derived property value for the IdentifierClass, where
		// ID_DIS and DISALLOWED are both treated as Disallowed and UNASSIGNED is also Disallowed.
		static PrecisClass GetPrecisIdentifierClass (int cp, string gc, bool noncharacter, bool joinControl, bool defaultIgnorable, bool hasCompat, bool oldHangulJamo)
		{
			// RFC 8264, Section 9.6 (and RFC 5892, Section 2.6): Exceptions
			switch (cp) {
			case 0x00DF: case 0x03C2: case 0x06FD: case 0x06FE: case 0x0F0B: case 0x3007:
				return PrecisClass.PValid;
			case 0x00B7: case 0x0375: case 0x05F3: case 0x05F4: case 0x30FB:
				return PrecisClass.ContextO;
			case 0x0640: case 0x07FA: case 0x302E: case 0x302F: case 0x303B:
				return PrecisClass.Disallowed;
			}

			if (cp >= 0x0660 && cp <= 0x0669)
				return PrecisClass.ContextO;

			if (cp >= 0x06F0 && cp <= 0x06F9)
				return PrecisClass.ContextO;

			if (cp >= 0x3031 && cp <= 0x3035)
				return PrecisClass.Disallowed;

			// RFC 8264, Section 9.7: BackwardCompatible is currently empty.

			// RFC 8264, Section 9.10: Unassigned
			if (gc == "Cn" && !noncharacter)
				return PrecisClass.Disallowed;

			// RFC 8264, Section 9.11: ASCII7
			if (cp >= 0x21 && cp <= 0x7E)
				return PrecisClass.PValid;

			// RFC 8264, Section 9.8: JoinControl
			if (joinControl)
				return PrecisClass.ContextJ;

			// RFC 8264, Section 9.9: OldHangulJamo
			if (oldHangulJamo)
				return PrecisClass.Disallowed;

			// RFC 8264, Section 9.13: PrecisIgnorableProperties
			if (defaultIgnorable || noncharacter)
				return PrecisClass.Disallowed;

			// RFC 8264, Section 9.12: Controls
			if (gc == "Cc")
				return PrecisClass.Disallowed;

			// RFC 8264, Section 9.17: HasCompat (ID_DIS)
			if (hasCompat)
				return PrecisClass.Disallowed;

			// RFC 8264, Section 9.1: LetterDigits
			switch (gc) {
			case "Ll": case "Lu": case "Lo": case "Nd": case "Lm": case "Mn": case "Mc":
				return PrecisClass.PValid;
			}

			// OtherLetterDigits, Spaces, Symbols and Punctuation are all ID_DIS, everything else is DISALLOWED.
			return PrecisClass.Disallowed;
		}

		static void AppendFileHeader (StringBuilder builder, string fileName, string className, bool isPublic, params string[] usings)
		{
			builder.Append (string.Format (LicenseHeader, fileName).ReplaceLineEndings ("\r\n"));
			builder.Append ("\r\n");
			builder.Append ("// WARNING: This file is auto-generated by tools/SmtpUtf8TableGenerator. DO NOT EDIT!\r\n");
			builder.Append ("\r\n");

			foreach (var ns in usings)
				builder.AppendFormat ("using {0};\r\n", ns);

			builder.Append ("\r\n");
			builder.Append ("namespace MimeKit.Utils {\r\n");
			builder.AppendFormat ("\t{0}static partial class {1}\r\n", isPublic ? "public " : string.Empty, className);
			builder.Append ("\t{\r\n");
		}

		static void AppendFileFooter (StringBuilder builder)
		{
			builder.Append ("\t}\r\n");
			builder.Append ("}\r\n");
		}

		static string GenerateValidatorTables (List<string> scriptNames, string[] scripts, PrecisClass[] precis, JoiningType[] joiningTypes, byte[] combiningClass)
		{
			var scriptIndex = new Dictionary<string, int> ();

			for (int i = 0; i < scriptNames.Count; i++)
				scriptIndex.Add (scriptNames[i], i);

			if (scriptNames.Count > 512)
				throw new InvalidOperationException ("Too many scripts to fit in 9 bits.");

			// Each entry is a little-endian uint: (start << 11) | (script << 2) | precisClass
			var codePointData = new List<uint> ();
			uint previous = uint.MaxValue;

			for (int cp = 0; cp <= MaxCodePoint; cp++) {
				uint value = ((uint) scriptIndex[scripts[cp]] << 2) | (uint) precis[cp];

				if (value != previous) {
					codePointData.Add (((uint) cp << 11) | value);
					previous = value;
				}
			}

			// Each entry is a little-endian uint: (start << 4) | (isVirama << 3) | joiningType
			var joiningData = new List<uint> ();
			previous = uint.MaxValue;

			for (int cp = 0; cp <= MaxCodePoint; cp++) {
				uint value = (combiningClass[cp] == ViramaCombiningClass ? 8u : 0u) | (uint) joiningTypes[cp];

				if (value != previous) {
					joiningData.Add (((uint) cp << 4) | value);
					previous = value;
				}
			}

			var builder = new StringBuilder ();

			AppendFileHeader (builder, "SmtpUtf8AddressValidator.g.cs", "SmtpUtf8AddressValidator", true, "System");
			builder.Append ("\t\t// The version of the Unicode Character Database that the validation tables were generated from.\r\n");
			builder.AppendFormat ("\t\tinternal const string UnicodeVersion = \"{0}\";\r\n", UnicodeVersion);
			builder.Append ("\r\n");
			builder.Append ("\t\tinternal enum UnicodeScript : ushort\r\n");
			builder.Append ("\t\t{\r\n");

			for (int i = 0; i < scriptNames.Count; i++)
				builder.AppendFormat ("\t\t\t{0}{1}\r\n", scriptNames[i], i + 1 < scriptNames.Count ? "," : string.Empty);

			builder.Append ("\t\t}\r\n");
			builder.Append ("\r\n");
			builder.Append ("\t\t// Each entry is a little-endian uint32: (firstCodePoint << 11) | (UnicodeScript << 2) | PrecisClass.\r\n");
			builder.Append ("\t\t// An entry applies to every code point up to (but not including) the next entry's first code point.\r\n");
			builder.AppendFormat ("\t\tconst int CodePointDataCount = {0};\r\n", codePointData.Count);
			builder.Append ("\r\n");
			AppendTable (builder, "CodePointData", codePointData);
			builder.Append ("\r\n");
			builder.Append ("\t\t// Each entry is a little-endian uint32: (firstCodePoint << 4) | (isVirama << 3) | JoiningType.\r\n");
			builder.Append ("\t\t// An entry applies to every code point up to (but not including) the next entry's first code point.\r\n");
			builder.AppendFormat ("\t\tconst int JoiningDataCount = {0};\r\n", joiningData.Count);
			builder.Append ("\r\n");
			AppendTable (builder, "JoiningData", joiningData);
			AppendFileFooter (builder);

			return builder.ToString ();
		}

		static string GenerateNormalizationTables (byte[] combiningClass, QuickCheck[] nfcQuickCheck, List<(int Composite, int First, int Second)> compositions, int maxDecompositionLength)
		{
			// Each entry is a little-endian uint: (start << 10) | (nfcQuickCheck << 8) | combiningClass
			var normalizationData = new List<uint> ();
			uint previous = uint.MaxValue;

			for (int cp = 0; cp <= MaxCodePoint; cp++) {
				uint value = ((uint) nfcQuickCheck[cp] << 8) | combiningClass[cp];

				if (value != previous) {
					normalizationData.Add (((uint) cp << 10) | value);
					previous = value;
				}
			}

			// Each composition is 3 little-endian uints (composite, first, second), sorted by composite.
			var compositionData = new List<uint> ();

			foreach (var composition in compositions) {
				compositionData.Add ((uint) composition.Composite);
				compositionData.Add ((uint) composition.First);
				compositionData.Add ((uint) composition.Second);
			}

			// The indexes of the compositions, sorted by (first, second).
			var compositionPairs = Enumerable.Range (0, compositions.Count)
				.OrderBy (i => compositions[i].First)
				.ThenBy (i => compositions[i].Second)
				.Select (i => (ushort) i)
				.ToList ();

			var builder = new StringBuilder ();

			AppendFileHeader (builder, "UnicodeNormalization.g.cs", "UnicodeNormalization", false, "System");
			builder.Append ("\t\t// The version of the Unicode Character Database that the normalization tables were generated from.\r\n");
			builder.AppendFormat ("\t\tinternal const string UnicodeVersion = \"{0}\";\r\n", UnicodeVersion);
			builder.Append ("\r\n");
			builder.Append ("\t\t// Each entry is a little-endian uint32: (firstCodePoint << 10) | (NormalizationQuickCheck << 8) | Canonical_Combining_Class,\r\n");
			builder.Append ("\t\t// where NormalizationQuickCheck is the NFC_Quick_Check property value.\r\n");
			builder.Append ("\t\t// An entry applies to every code point up to (but not including) the next entry's first code point.\r\n");
			builder.AppendFormat ("\t\tconst int NormalizationDataCount = {0};\r\n", normalizationData.Count);
			builder.Append ("\r\n");
			AppendTable (builder, "NormalizationData", normalizationData);
			builder.Append ("\r\n");
			builder.Append ("\t\t// The maximum length of the full canonical decomposition of a code point that is not NFC_Quick_Check=No.\r\n");
			builder.AppendFormat ("\t\tconst int MaxDecompositionLength = {0};\r\n", maxDecompositionLength);
			builder.Append ("\r\n");
			builder.Append ("\t\t// Each primary composite (excluding Hangul syllables) is 3 little-endian uint32s: the composite and the two\r\n");
			builder.Append ("\t\t// code points of its canonical decomposition mapping. The compositions are sorted by composite.\r\n");
			builder.AppendFormat ("\t\tconst int CompositionCount = {0};\r\n", compositions.Count);
			builder.Append ("\r\n");
			AppendTable (builder, "CompositionData", compositionData);
			builder.Append ("\r\n");
			builder.Append ("\t\t// Each entry is a little-endian uint16 index into CompositionData; the entries are sorted by the two code points\r\n");
			builder.Append ("\t\t// of the canonical decomposition mapping.\r\n");
			AppendUInt16Table (builder, "CompositionPairs", compositionPairs);
			AppendFileFooter (builder);

			return builder.ToString ();
		}

		static void AppendTable (StringBuilder builder, string name, List<uint> entries)
		{
			const int EntriesPerLine = 4;

			builder.AppendFormat ("\t\tstatic ReadOnlySpan<byte> {0} => new byte[] {{\r\n", name);

			for (int i = 0; i < entries.Count; i += EntriesPerLine) {
				builder.Append ("\t\t\t");

				for (int j = i; j < Math.Min (i + EntriesPerLine, entries.Count); j++) {
					uint value = entries[j];

					if (j > i)
						builder.Append (' ');

					builder.AppendFormat ("0x{0:X2}, 0x{1:X2}, 0x{2:X2}, 0x{3:X2},", value & 0xFF, (value >> 8) & 0xFF, (value >> 16) & 0xFF, value >> 24);
				}

				builder.Append ("\r\n");
			}

			builder.Append ("\t\t};\r\n");
		}

		static void AppendUInt16Table (StringBuilder builder, string name, List<ushort> entries)
		{
			const int EntriesPerLine = 8;

			builder.AppendFormat ("\t\tstatic ReadOnlySpan<byte> {0} => new byte[] {{\r\n", name);

			for (int i = 0; i < entries.Count; i += EntriesPerLine) {
				builder.Append ("\t\t\t");

				for (int j = i; j < Math.Min (i + EntriesPerLine, entries.Count); j++) {
					ushort value = entries[j];

					if (j > i)
						builder.Append (' ');

					builder.AppendFormat ("0x{0:X2}, 0x{1:X2},", value & 0xFF, value >> 8);
				}

				builder.Append ("\r\n");
			}

			builder.Append ("\t\t};\r\n");
		}
	}
}

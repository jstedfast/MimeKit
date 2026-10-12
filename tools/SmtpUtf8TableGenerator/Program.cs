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

// Generates MimeKit/Utils/SmtpUtf8AddressValidator.g.cs from the Unicode Character Database.
//
// Usage: dotnet run --project tools/SmtpUtf8TableGenerator [-- <ucd-directory>]
//
// If no UCD directory is given, the files are downloaded from unicode.org into a cache
// directory under the system temp directory.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Http;
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

	static class Program
	{
		const string UnicodeVersion = "18.0.0";
		const int MaxCodePoint = 0x10FFFF;
		const int CodePointCount = MaxCodePoint + 1;
		const byte ViramaCombiningClass = 9;

		const string LicenseHeader = @"//
// SmtpUtf8AddressValidator.g.cs
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
		};

		static int Main (string[] args)
		{
			var ucd = args.Length > 0 ? args[0] : Path.Combine (Path.GetTempPath (), "MimeKit-UCD-" + UnicodeVersion);
			var output = Path.GetFullPath (Path.Combine (AppContext.BaseDirectory, "..", "..", "..", "..", "..", "MimeKit", "Utils", "SmtpUtf8AddressValidator.g.cs"));

			if (args.Length > 1)
				output = args[1];

			Download (ucd);

			var generalCategory = new string[CodePointCount];
			var combiningClass = new byte[CodePointCount];
			Array.Fill (generalCategory, "Cn");

			ParseUnicodeData (Path.Combine (ucd, "UnicodeData.txt"), generalCategory, combiningClass);

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

			var source = Generate (scriptNames, scripts, precis, joiningTypes, combiningClass);

			File.WriteAllText (output, source, new UTF8Encoding (true));
			Console.WriteLine ("Wrote {0}", output);

			return 0;
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

		static void ParseUnicodeData (string path, string[] generalCategory, byte[] combiningClass)
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

		static string Generate (List<string> scriptNames, string[] scripts, PrecisClass[] precis, JoiningType[] joiningTypes, byte[] combiningClass)
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

			builder.Append (LicenseHeader.ReplaceLineEndings ("\r\n"));
			builder.Append ("\r\n");
			builder.Append ("// WARNING: This file is auto-generated by tools/SmtpUtf8TableGenerator. DO NOT EDIT!\r\n");
			builder.Append ("\r\n");
			builder.Append ("using System;\r\n");
			builder.Append ("\r\n");
			builder.Append ("namespace MimeKit.Utils {\r\n");
			builder.Append ("\tpublic static partial class SmtpUtf8AddressValidator\r\n");
			builder.Append ("\t{\r\n");
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
			builder.Append ("\t}\r\n");
			builder.Append ("}\r\n");

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
	}
}

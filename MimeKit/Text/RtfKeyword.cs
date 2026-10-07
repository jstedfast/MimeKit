//
// RtfKeyword.cs
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

using System;

namespace MimeKit.Text {
	/// <summary>
	/// The RTF control words that are understood by the RTF converters.
	/// </summary>
	/// <remarks>
	/// Unless otherwise noted, these are defined by the Microsoft Rich Text Format (RTF) Specification, Version 1.9.1.
	/// Control words that the converters do not need to act upon are deliberately absent: RTF 1.9.1, "Conventions of an
	/// RTF Reader", requires that readers ignore control words they do not understand.
	/// </remarks>
	enum RtfKeyword
	{
		Unknown,

		// Document
		Rtf,
		Ansi,
		Mac,
		Pc,
		Pca,
		Ansicpg,
		Deff,
		Bin,

		// Encapsulated HTML ([MS-OXRTFEX])
		Fromhtml,
		Htmlrtf,
		Htmltag,

		// Destinations
		Fonttbl,
		Colortbl,
		Field,
		Fldinst,
		Fldrslt,
		Upr,
		Ud,
		Nesttableprops,

		// All destinations whose content is never rendered share this one id (see RtfKeywords for the list).
		SkipDestination,

		// Font table
		F,
		Fcharset,
		Cpg,

		// Color table
		Red,
		Green,
		Blue,

		// Character formatting
		Plain,
		B,
		I,
		Ul,
		Ulnone,
		Strike,
		V,
		Super,
		Sub,
		Nosupersub,
		Fs,
		Cf,
		Cb,

		// Unicode
		U,
		Uc,

		// Paragraphs and tables
		Par,
		Pard,
		Line,
		Tab,
		Ql,
		Qc,
		Qr,
		Qj,
		Intbl,
		Cell,
		Row,

		// Special characters
		Emdash,
		Endash,
		Bullet,
		Lquote,
		Rquote,
		Ldblquote,
		Rdblquote,
		Emspace,
		Enspace,
		Qmspace,
		Zwj,
		Zwnj,
		Ltrmark,
		Rtlmark,

		// Attachment placeholders ([MS-OXRTFEX] 2.2.3.4)
		Objattph,
	}

	/// <summary>
	/// Maps control word names to <see cref="RtfKeyword"/> values.
	/// </summary>
	/// <remarks>
	/// The table is sorted ordinally once and searched with a binary search over <see cref="ReadOnlySpan{T}"/> so that
	/// looking up a control word never allocates. Control words are case-sensitive (RTF 1.9.1, "Control Words").
	/// </remarks>
	static class RtfKeywords
	{
		// Note: ValueTuple is not available on net462, so use a simple struct for the initial table.
		readonly struct Entry
		{
			public readonly string Name;
			public readonly RtfKeyword Value;

			public Entry (string name, RtfKeyword value)
			{
				Name = name;
				Value = value;
			}
		}

		static readonly string[] Names;
		static readonly RtfKeyword[] Values;

		static RtfKeywords ()
		{
			var table = new Entry[] {
				new Entry ("rtf", RtfKeyword.Rtf),
				new Entry ("ansi", RtfKeyword.Ansi),
				new Entry ("mac", RtfKeyword.Mac),
				new Entry ("pc", RtfKeyword.Pc),
				new Entry ("pca", RtfKeyword.Pca),
				new Entry ("ansicpg", RtfKeyword.Ansicpg),
				new Entry ("deff", RtfKeyword.Deff),
				new Entry ("bin", RtfKeyword.Bin),

				// [MS-OXRTFEX] 2.1.3.1: control words used to encapsulate HTML within RTF.
				new Entry ("fromhtml", RtfKeyword.Fromhtml),
				new Entry ("htmlrtf", RtfKeyword.Htmlrtf),
				new Entry ("htmltag", RtfKeyword.Htmltag),

				new Entry ("fonttbl", RtfKeyword.Fonttbl),
				new Entry ("colortbl", RtfKeyword.Colortbl),
				new Entry ("field", RtfKeyword.Field),
				new Entry ("fldinst", RtfKeyword.Fldinst),
				new Entry ("fldrslt", RtfKeyword.Fldrslt),
				new Entry ("upr", RtfKeyword.Upr),
				new Entry ("ud", RtfKeyword.Ud),
				// RTF 1.9.1, "Nested Tables": {\*\nesttableprops ... \nestrow} holds the row properties of a nested
				// table *and* the \nestrow that ends the row, so it must be recognized rather than skipped as an
				// unknown \* destination.
				new Entry ("nesttableprops", RtfKeyword.Nesttableprops),

				// Destinations whose content is never rendered (document metadata, page headers/footers,
				// footnotes, annotations, pictures, embedded objects, style/list/revision tables, etc).
				// Many of these are emitted without a leading \* (e.g. {\info ...}, {\pict ...}, {\stylesheet ...}),
				// so they would otherwise leak their text into the output. Skipping them also means that large
				// embedded binary payloads (\pict, \objdata, \datastore, \themedata) are never interpreted.
				new Entry ("aftnsep", RtfKeyword.SkipDestination),
				new Entry ("aftnsepc", RtfKeyword.SkipDestination),
				new Entry ("annotation", RtfKeyword.SkipDestination),
				new Entry ("atnauthor", RtfKeyword.SkipDestination),
				new Entry ("atnid", RtfKeyword.SkipDestination),
				new Entry ("background", RtfKeyword.SkipDestination),
				new Entry ("bkmkend", RtfKeyword.SkipDestination),
				new Entry ("bkmkstart", RtfKeyword.SkipDestination),
				new Entry ("colorschememapping", RtfKeyword.SkipDestination),
				new Entry ("datastore", RtfKeyword.SkipDestination),
				new Entry ("docvar", RtfKeyword.SkipDestination),
				new Entry ("filetbl", RtfKeyword.SkipDestination),
				new Entry ("footer", RtfKeyword.SkipDestination),
				new Entry ("footerf", RtfKeyword.SkipDestination),
				new Entry ("footerl", RtfKeyword.SkipDestination),
				new Entry ("footerr", RtfKeyword.SkipDestination),
				new Entry ("footnote", RtfKeyword.SkipDestination),
				new Entry ("ftnsep", RtfKeyword.SkipDestination),
				new Entry ("ftnsepc", RtfKeyword.SkipDestination),
				new Entry ("generator", RtfKeyword.SkipDestination),
				new Entry ("header", RtfKeyword.SkipDestination),
				new Entry ("headerf", RtfKeyword.SkipDestination),
				new Entry ("headerl", RtfKeyword.SkipDestination),
				new Entry ("headerr", RtfKeyword.SkipDestination),
				new Entry ("info", RtfKeyword.SkipDestination),
				new Entry ("latentstyles", RtfKeyword.SkipDestination),
				new Entry ("listoverridetable", RtfKeyword.SkipDestination),
				new Entry ("listtable", RtfKeyword.SkipDestination),
				new Entry ("mhtmltag", RtfKeyword.SkipDestination),
				new Entry ("mmathPr", RtfKeyword.SkipDestination),
				new Entry ("nonesttables", RtfKeyword.SkipDestination), // RTF 1.9.1, "Nested Tables": a duplicate rendering for readers without nested table support
				new Entry ("nonshppict", RtfKeyword.SkipDestination),
				// Note: \object itself is intentionally *not* skipped. RTF 1.9.1, "Objects": "{\object ... {\*\objdata ...}
				// {\result ...}}" where \result "contains the last updated result of the object" so that readers that
				// do not understand the object can display it. The object's data subgroups are skipped individually
				// (\objdata here; \objclass, \objname, etc. are always written as \* destinations).
				new Entry ("objdata", RtfKeyword.SkipDestination),
				new Entry ("pgdsctbl", RtfKeyword.SkipDestination),
				new Entry ("pict", RtfKeyword.SkipDestination),
				new Entry ("revtbl", RtfKeyword.SkipDestination),
				new Entry ("rsidtbl", RtfKeyword.SkipDestination),
				new Entry ("shpinst", RtfKeyword.SkipDestination),
				new Entry ("stylesheet", RtfKeyword.SkipDestination),
				new Entry ("tc", RtfKeyword.SkipDestination),
				new Entry ("themedata", RtfKeyword.SkipDestination),
				new Entry ("txe", RtfKeyword.SkipDestination),
				new Entry ("userprops", RtfKeyword.SkipDestination),
				new Entry ("xe", RtfKeyword.SkipDestination),
				new Entry ("xmlnstbl", RtfKeyword.SkipDestination),

				new Entry ("f", RtfKeyword.F),
				new Entry ("fcharset", RtfKeyword.Fcharset),
				new Entry ("cpg", RtfKeyword.Cpg),

				new Entry ("red", RtfKeyword.Red),
				new Entry ("green", RtfKeyword.Green),
				new Entry ("blue", RtfKeyword.Blue),

				new Entry ("plain", RtfKeyword.Plain),
				new Entry ("b", RtfKeyword.B),
				new Entry ("i", RtfKeyword.I),
				// RTF 1.9.1, "Font (Character) Formatting Properties": the various underline styles are all
				// rendered as a plain underline.
				new Entry ("ul", RtfKeyword.Ul),
				new Entry ("uld", RtfKeyword.Ul),
				new Entry ("uldash", RtfKeyword.Ul),
				new Entry ("uldashd", RtfKeyword.Ul),
				new Entry ("uldashdd", RtfKeyword.Ul),
				new Entry ("uldb", RtfKeyword.Ul),
				new Entry ("ulhwave", RtfKeyword.Ul),
				new Entry ("ulldash", RtfKeyword.Ul),
				new Entry ("ulth", RtfKeyword.Ul),
				new Entry ("ulthd", RtfKeyword.Ul),
				new Entry ("ulthdash", RtfKeyword.Ul),
				new Entry ("ulthdashd", RtfKeyword.Ul),
				new Entry ("ulthdashdd", RtfKeyword.Ul),
				new Entry ("ulthldash", RtfKeyword.Ul),
				new Entry ("ululdbwave", RtfKeyword.Ul),
				new Entry ("ulw", RtfKeyword.Ul),
				new Entry ("ulwave", RtfKeyword.Ul),
				new Entry ("ulnone", RtfKeyword.Ulnone),
				new Entry ("strike", RtfKeyword.Strike),
				new Entry ("striked", RtfKeyword.Strike),
				new Entry ("v", RtfKeyword.V),
				new Entry ("super", RtfKeyword.Super),
				new Entry ("sub", RtfKeyword.Sub),
				new Entry ("nosupersub", RtfKeyword.Nosupersub),
				new Entry ("fs", RtfKeyword.Fs),
				new Entry ("cf", RtfKeyword.Cf),
				// \cb is the documented background color, but Word ignores it and writes \highlight (or the
				// character shading color \chcbpat) instead, so treat them all as the background color.
				new Entry ("cb", RtfKeyword.Cb),
				new Entry ("chcbpat", RtfKeyword.Cb),
				new Entry ("highlight", RtfKeyword.Cb),

				new Entry ("u", RtfKeyword.U),
				new Entry ("uc", RtfKeyword.Uc),

				// Section and page breaks have no meaning in a reflowable output, so render them as paragraph breaks.
				new Entry ("par", RtfKeyword.Par),
				new Entry ("sect", RtfKeyword.Par),
				new Entry ("page", RtfKeyword.Par),
				new Entry ("pard", RtfKeyword.Pard),
				new Entry ("line", RtfKeyword.Line),
				new Entry ("tab", RtfKeyword.Tab),
				new Entry ("ql", RtfKeyword.Ql),
				new Entry ("qc", RtfKeyword.Qc),
				new Entry ("qr", RtfKeyword.Qr),
				new Entry ("qj", RtfKeyword.Qj),
				new Entry ("intbl", RtfKeyword.Intbl),
				new Entry ("cell", RtfKeyword.Cell),
				new Entry ("nestcell", RtfKeyword.Cell),
				new Entry ("row", RtfKeyword.Row),
				new Entry ("nestrow", RtfKeyword.Row),

				// RTF 1.9.1, "Special Characters".
				new Entry ("emdash", RtfKeyword.Emdash),
				new Entry ("endash", RtfKeyword.Endash),
				new Entry ("bullet", RtfKeyword.Bullet),
				new Entry ("lquote", RtfKeyword.Lquote),
				new Entry ("rquote", RtfKeyword.Rquote),
				new Entry ("ldblquote", RtfKeyword.Ldblquote),
				new Entry ("rdblquote", RtfKeyword.Rdblquote),
				new Entry ("emspace", RtfKeyword.Emspace),
				new Entry ("enspace", RtfKeyword.Enspace),
				new Entry ("qmspace", RtfKeyword.Qmspace),
				new Entry ("zwj", RtfKeyword.Zwj),
				new Entry ("zwnj", RtfKeyword.Zwnj),
				new Entry ("ltrmark", RtfKeyword.Ltrmark),
				new Entry ("rtlmark", RtfKeyword.Rtlmark),

				// [MS-OXRTFEX] 2.2.3.4, "Attachment and RTF Integration": marks where an attachment is rendered.
				new Entry ("objattph", RtfKeyword.Objattph),
			};

			// Note: ordinal order matches the order used by MemoryExtensions.SequenceCompareTo in Lookup.
			Array.Sort (table, (x, y) => string.CompareOrdinal (x.Name, y.Name));

			Names = new string[table.Length];
			Values = new RtfKeyword[table.Length];

			for (int i = 0; i < table.Length; i++) {
				Names[i] = table[i].Name;
				Values[i] = table[i].Value;
			}
		}

		/// <summary>
		/// Look up a control word without allocating.
		/// </summary>
		/// <returns>The keyword identifier or <see cref="RtfKeyword.Unknown"/> if the control word is not known.</returns>
		/// <param name="name">The control word name.</param>
		public static RtfKeyword Lookup (ReadOnlySpan<char> name)
		{
			int min = 0, max = Names.Length - 1;

			while (min <= max) {
				int mid = min + ((max - min) >> 1);
				int cmp = name.SequenceCompareTo (Names[mid].AsSpan ());

				if (cmp == 0)
					return Values[mid];

				if (cmp < 0)
					max = mid - 1;
				else
					min = mid + 1;
			}

			return RtfKeyword.Unknown;
		}
	}
}

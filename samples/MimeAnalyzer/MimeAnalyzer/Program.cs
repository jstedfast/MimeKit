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

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using MimeKit;

namespace MimeAnalyzerExample
{
	/// <summary>
	/// An <see cref="IMimeComplianceLogger"/> that collects the issues reported by the parser.
	/// </summary>
	/// <remarks>
	/// <para>The parser places no limit by default on how many issues a message may produce, so a
	/// logger that retains them needs a limit of its own. See the remarks on
	/// <see cref="IMimeComplianceLogger"/>.</para>
	/// <para>This is a belt-and-braces limit: <see cref="MimeComplianceOptions.MaxIssuesPerViolation"/>
	/// bounds the report at the source, but a logger cannot assume that every caller configures
	/// it.</para>
	/// </remarks>
	class ComplianceCollector : IMimeComplianceLogger
	{
		readonly List<MimeComplianceIssue> issues;
		readonly int limit;

		public ComplianceCollector (int limit)
		{
			issues = new List<MimeComplianceIssue> ();
			this.limit = limit;
		}

		public IReadOnlyList<MimeComplianceIssue> Issues {
			get { return issues; }
		}

		/// <summary>
		/// Get whether the report is incomplete, either because the parser suppressed issues or
		/// because this collector reached its own limit.
		/// </summary>
		public bool Truncated {
			get; private set;
		}

		public void Log (in MimeComplianceIssue issue)
		{
			// Note: This is not a defect in the message, it is the parser saying that it stopped
			// reporting some violation. It is recorded as truncation rather than shown as a
			// diagnostic, because it has no source construct to point at and counting it as a
			// warning would inflate the summary -- and, under --werror, the exit code -- for a
			// message that may be perfectly fine apart from being verbosely wrong in one place.
			if (issue.Violation == MimeComplianceViolation.TooManyComplianceIssues) {
				Truncated = true;
				return;
			}

			if (issues.Count < limit)
				issues.Add (issue);
			else
				Truncated = true;
		}
	}

	/// <summary>
	/// The raw bytes of the input, indexed by line so that a diagnostic can quote the line it
	/// refers to.
	/// </summary>
	/// <remarks>
	/// The input is deliberately kept as bytes rather than decoded into a string. A message that
	/// violates MIME compliance is exactly the sort of message that contains bytes which are not
	/// valid in any single charset, and decoding it would both destroy the evidence and shift the
	/// column numbers that the parser reported.
	/// </remarks>
	class SourceText
	{
		readonly List<int> lineOffsets;
		readonly byte[] content;

		SourceText (byte[] content)
		{
			this.content = content;

			// Note: Line numbers reported by MimeReader are one-based and a line is terminated by a
			// linefeed, so index the offset of the first byte of each line. A bare carriage return
			// does not start a new line - the parser does not treat it as a terminator either.
			lineOffsets = new List<int> { 0 };

			for (int i = 0; i < content.Length; i++) {
				if (content[i] == (byte) '\n' && i + 1 < content.Length)
					lineOffsets.Add (i + 1);
			}
		}

		public static SourceText Load (string fileName)
		{
			return new SourceText (File.ReadAllBytes (fileName));
		}

		/// <summary>
		/// Get the content of the specified one-based line, excluding its line terminator.
		/// </summary>
		public bool TryGetLine (int lineNumber, out ArraySegment<byte> line)
		{
			line = default;

			if (lineNumber < 1 || lineNumber > lineOffsets.Count)
				return false;

			int startIndex = lineOffsets[lineNumber - 1];
			int endIndex = lineNumber < lineOffsets.Count ? lineOffsets[lineNumber] : content.Length;

			// Trim the line terminator so that it is not rendered as an escape sequence on every
			// line of a well-formed message.
			if (endIndex > startIndex && content[endIndex - 1] == (byte) '\n')
				endIndex--;

			if (endIndex > startIndex && content[endIndex - 1] == (byte) '\r')
				endIndex--;

			line = new ArraySegment<byte> (content, startIndex, endIndex - startIndex);

			return true;
		}

		/// <summary>
		/// Map a zero-based index into the content onto a one-based line and column number.
		/// </summary>
		bool TryGetPosition (int index, out int lineNumber, out int columnNumber)
		{
			lineNumber = 0;
			columnNumber = 0;

			if (index < 0 || index >= content.Length)
				return false;

			// Binary search for the last line that begins at or before the index.
			int lo = 0, hi = lineOffsets.Count - 1;

			while (lo < hi) {
				int mid = (lo + hi + 1) / 2;

				if (lineOffsets[mid] <= index)
					lo = mid;
				else
					hi = mid - 1;
			}

			lineNumber = lo + 1;
			columnNumber = (index - lineOffsets[lo]) + 1;

			return true;
		}

		/// <summary>
		/// Narrow an approximate position down to the byte that actually triggered the violation.
		/// </summary>
		/// <remarks>
		/// <para>Some violations are reported with a <see cref="MimeCompliancePositionKind"/> other than
		/// <see cref="MimeCompliancePositionKind.Exact"/>, meaning that the parser gave us the start of
		/// the offending line or element rather than the offending byte. Finding that byte means
		/// re-scanning input that the parser has already moved past, which is why the parser does not
		/// do it - but we only have to pay for it on the issues we are about to print.</para>
		/// <para>Returns <c>false</c> if the position is already exact, if the violation is a property of
		/// the element as a whole (in which case no single byte is to blame), or if the search comes up
		/// empty.</para>
		/// </remarks>
		public bool TryLocateOffendingByte (in MimeComplianceIssue issue, out int lineNumber, out int columnNumber)
		{
			lineNumber = issue.LineNumber;
			columnNumber = issue.ColumnNumber;

			if (issue.PositionKind == MimeCompliancePositionKind.Exact)
				return false;

			Predicate<byte> match;

			switch (issue.Violation) {
			case MimeComplianceViolation.Unexpected8BitBytesInHeader:
			case MimeComplianceViolation.Unexpected8BitBytesInBody:
				// Note: The parser detects these by checking whether the value is valid UTF-8, but the
				// byte where UTF-8 validation *fails* is an artifact of how the following bytes happen
				// to combine and can land in the middle of a run of 8-bit bytes. The first non-ASCII
				// byte is where the charset mistake actually begins.
				match = static c => c > 0x7F;
				break;
			case MimeComplianceViolation.UnexpectedNullBytesInHeader:
			case MimeComplianceViolation.UnexpectedNullBytesInBody:
				match = static c => c == 0;
				break;
			default:
				// The violation describes the element as a whole (an unparsable Content-Type, a repeated
				// header, a missing boundary parameter...), so there is no offending byte to point at.
				return false;
			}

			if (issue.StreamOffset < 0 || issue.StreamOffset >= content.Length)
				return false;

			int startIndex = (int) issue.StreamOffset;
			int endIndex;

			if (issue.PositionKind == MimeCompliancePositionKind.LineStart) {
				endIndex = issue.LineNumber < lineOffsets.Count ? lineOffsets[issue.LineNumber] : content.Length;
			} else {
				// Note: The element may be a header folded over several lines or an entire body part, so
				// scan forward. The detection that produced the issue guarantees that a matching byte
				// exists within the element, so the first match cannot belong to a later one.
				endIndex = content.Length;
			}

			for (int i = startIndex; i < endIndex; i++) {
				if (match (content[i]))
					return TryGetPosition (i, out lineNumber, out columnNumber);
			}

			return false;
		}
	}

	enum DiagnosticLevel
	{
		Note,
		Warning,
		Error
	}

	/// <summary>
	/// Renders compliance issues as compiler-style diagnostics.
	/// </summary>
	class DiagnosticWriter
	{
		const string Ellipsis = "...";
		const int TabWidth = 8;

		readonly TextWriter output;
		readonly bool color;

		public DiagnosticWriter (TextWriter output, bool color)
		{
			this.output = output;
			this.color = color;

			MaxLineWidth = GetDefaultLineWidth ();
		}

		static int GetDefaultLineWidth ()
		{
			try {
				// Leave room for the line-number gutter.
				if (!Console.IsOutputRedirected && Console.WindowWidth > 40)
					return Console.WindowWidth - 12;
			} catch (IOException) {
				// No console is attached.
			}

			return 100;
		}

		/// <summary>
		/// The widest source line that will be quoted before it is elided.
		/// </summary>
		public int MaxLineWidth {
			get; set;
		}

		public bool ShowSourceLine {
			get; set;
		} = true;

		public bool ShowRemarks {
			get; set;
		}

		/// <summary>
		/// The width of the line-number gutter, sized to the largest line number that will be
		/// printed so that every diagnostic for a file lines up.
		/// </summary>
		public int GutterWidth {
			get; set;
		} = 1;

		static DiagnosticLevel GetLevel (MimeComplianceSeverity severity, bool warningsAreErrors)
		{
			switch (severity) {
			case MimeComplianceSeverity.Critical:
				return DiagnosticLevel.Error;
			case MimeComplianceSeverity.Major:
				return warningsAreErrors ? DiagnosticLevel.Error : DiagnosticLevel.Warning;
			default:
				return warningsAreErrors ? DiagnosticLevel.Error : DiagnosticLevel.Note;
			}
		}

		static ConsoleColor GetColor (DiagnosticLevel level)
		{
			switch (level) {
			case DiagnosticLevel.Error:
				return ConsoleColor.Red;
			case DiagnosticLevel.Warning:
				return ConsoleColor.Yellow;
			default:
				return ConsoleColor.Cyan;
			}
		}

		static string GetLabel (DiagnosticLevel level)
		{
			switch (level) {
			case DiagnosticLevel.Error:
				return "error";
			case DiagnosticLevel.Warning:
				return "warning";
			default:
				return "note";
			}
		}

		static ConsoleColor Brighten (ConsoleColor foreground)
		{
			switch (foreground) {
			case ConsoleColor.DarkGray: return ConsoleColor.Gray;
			case ConsoleColor.Gray: return ConsoleColor.White;
			default: return foreground;
			}
		}

		void Write (string text, ConsoleColor foreground, bool bold = false)
		{
			if (!color) {
				output.Write (text);
				return;
			}

			// Note: Console.ForegroundColor is used rather than ANSI escape sequences so that the
			// sample behaves on a plain Windows console as well as on a terminal that understands
			// them.
			var saved = Console.ForegroundColor;

			Console.ForegroundColor = bold ? Brighten (foreground) : foreground;
			output.Write (text);
			Console.ForegroundColor = saved;
		}

		/// <summary>
		/// Render a line of raw message bytes as printable text, and map the one-based column that
		/// the parser reported onto the column of the rendered text.
		/// </summary>
		/// <remarks>
		/// Bytes that are not printable ASCII are rendered as <c>\xNN</c> escapes, which is both
		/// safe to write to a terminal and useful in its own right: several of the violations being
		/// reported are about precisely those bytes. Because an escape occupies more columns than
		/// the byte it stands for, the caret position has to be computed while rendering rather
		/// than taken from the reported column.
		/// </remarks>
		static string RenderLine (ArraySegment<byte> line, int column, out int caretColumn)
		{
			var rendered = new StringBuilder (line.Count);
			var bytes = line.Array;

			caretColumn = 0;

			for (int i = 0; i < line.Count; i++) {
				if (i == column - 1)
					caretColumn = rendered.Length + 1;

				byte c = bytes[line.Offset + i];

				if (c == (byte) '\t') {
					rendered.Append (' ', TabWidth - (rendered.Length % TabWidth));
				} else if (c >= 0x20 && c < 0x7f) {
					rendered.Append ((char) c);
				} else {
					rendered.Append ("\\x").Append (c.ToString ("X2"));
				}
			}

			// A violation may be reported at the line terminator itself (a bare linefeed, for
			// example), which lands one column past the end of the rendered text.
			if (column > 0 && column - 1 >= line.Count)
				caretColumn = rendered.Length + 1;

			return rendered.ToString ();
		}

		/// <summary>
		/// Narrow a rendered line down to a window around the caret.
		/// </summary>
		/// <remarks>
		/// A diagnostic on a very long line - <see cref="MimeComplianceViolation.OversizedLine"/>
		/// being the obvious case, but a single header can also run to several kilobytes - would
		/// otherwise flood the terminal with the very thing being complained about.
		/// </remarks>
		static string Elide (string text, ref int caretColumn, int maxWidth)
		{
			if (text.Length <= maxWidth)
				return text;

			// Budget for an ellipsis at each end so that the result never exceeds maxWidth,
			// whichever end (or both) ends up being trimmed.
			int budget = Math.Max (16, maxWidth - (Ellipsis.Length * 2));

			// Center the window on the caret, then slide it back inside the line.
			int start = Math.Max (0, (caretColumn - 1) - (budget / 2));
			int end = Math.Min (text.Length, start + budget);

			start = Math.Max (0, end - budget);

			var elided = new StringBuilder ();

			if (start > 0)
				elided.Append (Ellipsis);

			elided.Append (text, start, end - start);

			if (end < text.Length)
				elided.Append (Ellipsis);

			if (caretColumn > 0) {
				caretColumn -= start;

				if (start > 0)
					caretColumn += Ellipsis.Length;

				caretColumn = Math.Min (Math.Max (caretColumn, 1), elided.Length + 1);
			}

			return elided.ToString ();
		}

		void WriteGutter (string text)
		{
			Write (text.PadLeft (GutterWidth) + " | ", ConsoleColor.DarkGray);
		}

		static IEnumerable<string> Wrap (string text, int width)
		{
			var words = text.Split (new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			var line = new StringBuilder ();

			foreach (var word in words) {
				if (line.Length > 0 && line.Length + 1 + word.Length > width) {
					yield return line.ToString ();
					line.Clear ();
				}

				if (line.Length > 0)
					line.Append (' ');

				line.Append (word);
			}

			if (line.Length > 0)
				yield return line.ToString ();
		}

		public void Write (string fileName, SourceText source, in MimeComplianceIssue issue, bool warningsAreErrors)
		{
			var level = GetLevel (issue.Severity, warningsAreErrors);
			var foreground = GetColor (level);

			// The parser reports the position it already knew. If that is only the start of the
			// offending line or element, narrow it down to the offending byte ourselves - we are
			// about to print this issue, so we are the ones who should pay for the search.
			if (!source.TryLocateOffendingByte (issue, out int lineNumber, out int columnNumber)) {
				lineNumber = issue.LineNumber;
				columnNumber = issue.ColumnNumber;
			}

			Write (string.Format ("{0}:{1}:{2}: ", fileName, lineNumber, columnNumber), ConsoleColor.Gray, true);
			Write (GetLabel (level) + ": ", foreground, true);
			Write (issue.Description, ConsoleColor.Gray, true);
			Write (" [" + issue.Violation + "]" + Environment.NewLine, ConsoleColor.DarkGray);

			if (ShowSourceLine && source.TryGetLine (lineNumber, out var line)) {
				var text = RenderLine (line, columnNumber, out int caretColumn);

				text = Elide (text, ref caretColumn, MaxLineWidth);

				WriteGutter (lineNumber.ToString ());
				output.WriteLine (text);

				// The parser reports a column of 0 when it cannot attribute the violation to a
				// specific byte, in which case quoting the line is still useful but a caret would
				// be a lie.
				if (caretColumn > 0) {
					WriteGutter (string.Empty);
					Write (new string (' ', caretColumn - 1) + "^" + Environment.NewLine, foreground, true);
				}
			}

			if (ShowRemarks) {
				var categories = issue.Categories != MimeComplianceCategories.None
					? issue.Categories.ToString ()
					: "none";

				WriteGutter (string.Empty);
				Write ("= categories: " + categories + Environment.NewLine, ConsoleColor.DarkGray);

				foreach (var paragraph in Wrap (issue.Remarks, 88)) {
					WriteGutter (string.Empty);
					Write ("= " + paragraph + Environment.NewLine, ConsoleColor.DarkGray);
				}
			}
		}

		public void WriteSummary (string fileName, int errors, int warnings, int notes, bool truncated, bool filtered)
		{
			Write (fileName + ": ", ConsoleColor.Gray, true);

			if (errors == 0 && warnings == 0 && notes == 0) {
				// Distinguish "this message is clean" from "nothing survived the filters", which
				// are very different claims to make about a message.
				var message = filtered ? "no matching compliance issues" : "no compliance issues found";

				Write (message + Environment.NewLine, ConsoleColor.Green, true);
				return;
			}

			var counts = new List<string> ();

			if (errors > 0)
				counts.Add (Pluralize (errors, "error"));
			if (warnings > 0)
				counts.Add (Pluralize (warnings, "warning"));
			if (notes > 0)
				counts.Add (Pluralize (notes, "note"));

			Write (string.Join (", ", counts) + Environment.NewLine, ConsoleColor.Gray, true);

			if (truncated)
				Write (fileName + ": note: issue limit reached, some issues were not reported" + Environment.NewLine, ConsoleColor.DarkGray);
		}

		public static string Pluralize (int count, string noun)
		{
			return count == 1 ? "1 " + noun : count + " " + noun + "s";
		}
	}

	class Options
	{
		public readonly List<string> FileNames = new List<string> ();
		public MimeComplianceContext Context = MimeComplianceContext.Transport;
		public MimeComplianceSeverity MinimumSeverity = MimeComplianceSeverity.Minor;
		public MimeComplianceCategories Categories = MimeComplianceCategories.None;
		public bool WarningsAreErrors;
		public bool ShowSourceLine = true;
		public bool ShowRemarks;
		public bool Color = true;
		public bool Quiet;
		public int MaxIssues = 1000;
		public int MaxIssuesPerViolation;
	}

	static class Program
	{
		const string ProgramName = "MimeAnalyzer";

		static void PrintUsage ()
		{
			Console.WriteLine ("Usage: {0} [options] <file.eml> [<file.eml> ...]", ProgramName);
			Console.WriteLine ();
			Console.WriteLine ("Analyzes messages for MIME compliance violations and reports them as");
			Console.WriteLine ("compiler-style diagnostics.");
			Console.WriteLine ();
			Console.WriteLine ("Options:");
			Console.WriteLine ("  -c, --context <transport|storage>");
			Console.WriteLine ("                                 How the message is being used, which affects how");
			Console.WriteLine ("                                 severely a few violations are rated.");
			Console.WriteLine ("                                 Default: transport.");
			Console.WriteLine ("  -s, --severity <minor|major|critical>");
			Console.WriteLine ("                                 Only report issues at or above this severity.");
			Console.WriteLine ("                                 Default: minor.");
			Console.WriteLine ("      --category <list>          Only report issues in these comma-separated");
			Console.WriteLine ("                                 categories: interoperability, dataloss, security.");
			Console.WriteLine ("  -r, --remarks                  Print the detailed explanation of each issue.");
			Console.WriteLine ("  -W, --werror                   Report every issue as an error.");
			Console.WriteLine ("      --max-issues <n>           Stop collecting after n issues. Default: 1000.");
			Console.WriteLine ("      --max-per-violation <n>    Ask the parser to report each violation at most n");
			Console.WriteLine ("                                 times. Recommended for untrusted messages, which");
			Console.WriteLine ("                                 can be built to produce issues in bulk.");
			Console.WriteLine ("                                 Default: 0, meaning no limit.");
			Console.WriteLine ("      --no-caret                 Do not quote the offending source line.");
			Console.WriteLine ("      --no-color                 Disable colored output.");
			Console.WriteLine ("  -q, --quiet                    Only print the per-file summary.");
			Console.WriteLine ("  -h, --help                     Print this help text.");
			Console.WriteLine ();
			Console.WriteLine ("Exit codes:");
			Console.WriteLine ("  0  no issue was reported as an error");
			Console.WriteLine ("  1  an issue was reported as an error, or a file could not be read");
			Console.WriteLine ("  2  the command line could not be parsed");
		}

		static bool TryParseCategories (string value, out MimeComplianceCategories categories)
		{
			categories = MimeComplianceCategories.None;

			foreach (var token in value.Split (new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) {
				switch (token.Trim ().ToLowerInvariant ()) {
				case "interoperability":
				case "interop":
					categories |= MimeComplianceCategories.Interoperability;
					break;
				case "dataloss":
					categories |= MimeComplianceCategories.DataLoss;
					break;
				case "security":
					categories |= MimeComplianceCategories.Security;
					break;
				default:
					return false;
				}
			}

			return categories != MimeComplianceCategories.None;
		}

		static string GetArgument (string[] args, ref int index, string option)
		{
			if (index + 1 >= args.Length)
				throw new FormatException (string.Format ("The {0} option requires an argument.", option));

			return args[++index];
		}

		static Options ParseOptions (string[] args)
		{
			var options = new Options ();

			for (int i = 0; i < args.Length; i++) {
				var arg = args[i];

				switch (arg) {
				case "-h":
				case "--help":
					return null;
				case "-c":
				case "--context":
					var context = GetArgument (args, ref i, arg);
					if (!Enum.TryParse (context, true, out options.Context) || !Enum.IsDefined (typeof (MimeComplianceContext), options.Context))
						throw new FormatException (string.Format ("Unknown context: {0}", context));
					break;
				case "-s":
				case "--severity":
					var severity = GetArgument (args, ref i, arg);
					if (!Enum.TryParse (severity, true, out options.MinimumSeverity) || !Enum.IsDefined (typeof (MimeComplianceSeverity), options.MinimumSeverity))
						throw new FormatException (string.Format ("Unknown severity: {0}", severity));
					break;
				case "--category":
					var categories = GetArgument (args, ref i, arg);
					if (!TryParseCategories (categories, out options.Categories))
						throw new FormatException (string.Format ("Unknown category in: {0}", categories));
					break;
				case "--max-issues":
					var max = GetArgument (args, ref i, arg);
					if (!int.TryParse (max, out options.MaxIssues) || options.MaxIssues < 1)
						throw new FormatException (string.Format ("Invalid issue limit: {0}", max));
					break;
				case "--max-per-violation":
					var maxPerViolation = GetArgument (args, ref i, arg);
					if (!int.TryParse (maxPerViolation, out options.MaxIssuesPerViolation) || options.MaxIssuesPerViolation < 0)
						throw new FormatException (string.Format ("Invalid per-violation limit: {0}", maxPerViolation));
					break;
				case "-r":
				case "--remarks":
					options.ShowRemarks = true;
					break;
				case "-W":
				case "--werror":
					options.WarningsAreErrors = true;
					break;
				case "--no-caret":
					options.ShowSourceLine = false;
					break;
				case "--no-color":
					options.Color = false;
					break;
				case "-q":
				case "--quiet":
					options.Quiet = true;
					break;
				default:
					if (arg.Length > 1 && arg[0] == '-')
						throw new FormatException (string.Format ("Unknown option: {0}", arg));

					options.FileNames.Add (arg);
					break;
				}
			}

			if (options.FileNames.Count == 0)
				throw new FormatException ("No input files were specified.");

			return options;
		}

		static bool IsColorSupported ()
		{
			// Respect the de-facto NO_COLOR convention and avoid emitting color when the output is
			// being piped into a file or another program.
			if (Environment.GetEnvironmentVariable ("NO_COLOR") != null)
				return false;

			return !Console.IsOutputRedirected;
		}

		static IReadOnlyList<MimeComplianceIssue> Analyze (string fileName, Options options, out bool truncated)
		{
			var collector = new ComplianceCollector (options.MaxIssues);

			using (var stream = File.OpenRead (fileName)) {
				var reader = new MimeReader (stream) {
					ComplianceLogger = collector,
					ComplianceOptions = new MimeComplianceOptions {
						Context = options.Context,
						MaxIssuesPerViolation = options.MaxIssuesPerViolation
					}
				};

				// Note: MimeReader only scans the message, it does not construct a MimeMessage, so
				// this reports compliance issues without the cost of building an object tree.
				reader.ReadMessage ();
			}

			truncated = collector.Truncated;

			return collector.Issues;
		}

		static bool Matches (in MimeComplianceIssue issue, Options options)
		{
			if (issue.Severity < options.MinimumSeverity)
				return false;

			if (options.Categories != MimeComplianceCategories.None && (issue.Categories & options.Categories) == 0)
				return false;

			return true;
		}

		static int Main (string[] args)
		{
			Options options;

			try {
				options = ParseOptions (args);
			} catch (FormatException ex) {
				Console.Error.WriteLine ("{0}: error: {1}", ProgramName, ex.Message);
				Console.Error.WriteLine ();
				PrintUsage ();
				return 2;
			}

			if (options == null) {
				PrintUsage ();
				return 0;
			}

			var writer = new DiagnosticWriter (Console.Out, options.Color && IsColorSupported ()) {
				ShowSourceLine = options.ShowSourceLine,
				ShowRemarks = options.ShowRemarks
			};

			int totalErrors = 0, totalWarnings = 0, totalNotes = 0;
			bool failed = false;

			// Whether any issue was suppressed before it reached the summary, so that a file with
			// no reported issues is not described as clean when it may not be.
			bool filtering = options.MinimumSeverity != MimeComplianceSeverity.Minor ||
				options.Categories != MimeComplianceCategories.None;

			foreach (var fileName in options.FileNames) {
				IReadOnlyList<MimeComplianceIssue> issues;
				SourceText source;
				bool truncated;

				try {
					source = SourceText.Load (fileName);
					issues = Analyze (fileName, options, out truncated);
				} catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) {
					Console.Error.WriteLine ("{0}: error: {1}: {2}", ProgramName, fileName, ex.Message);
					failed = true;
					continue;
				}

				int errors = 0, warnings = 0, notes = 0;
				int widest = 1;

				foreach (var issue in issues) {
					if (!Matches (issue, options))
						continue;

					if (issue.Severity == MimeComplianceSeverity.Critical || options.WarningsAreErrors)
						errors++;
					else if (issue.Severity == MimeComplianceSeverity.Major)
						warnings++;
					else
						notes++;

					// Note: Narrowing an approximate position may move the diagnostic onto a later
					// line (a folded header, for example), so measure the position we will print.
					if (!source.TryLocateOffendingByte (issue, out int lineNumber, out _))
						lineNumber = issue.LineNumber;

					widest = Math.Max (widest, lineNumber.ToString ().Length);
				}

				writer.GutterWidth = widest;

				if (!options.Quiet) {
					foreach (var issue in issues) {
						if (Matches (issue, options))
							writer.Write (fileName, source, issue, options.WarningsAreErrors);
					}

					if (errors + warnings + notes > 0)
						Console.WriteLine ();
				}

				writer.WriteSummary (fileName, errors, warnings, notes, truncated, filtering);

				totalErrors += errors;
				totalWarnings += warnings;
				totalNotes += notes;
			}

			if (options.FileNames.Count > 1) {
				Console.WriteLine ();
				Console.WriteLine ("{0} analyzed: {1}, {2}, {3}",
					DiagnosticWriter.Pluralize (options.FileNames.Count, "file"),
					DiagnosticWriter.Pluralize (totalErrors, "error"),
					DiagnosticWriter.Pluralize (totalWarnings, "warning"),
					DiagnosticWriter.Pluralize (totalNotes, "note"));
			}

			return failed || totalErrors > 0 ? 1 : 0;
		}
	}
}

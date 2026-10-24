//
// EnrichedToHtml.cs
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
using System.Globalization;
using System.Collections.Generic;

namespace MimeKit.Text {
	/// <summary>
	/// A text/enriched to HTML converter.
	/// </summary>
	/// <remarks>
	/// <para>Converts RFC 1896 text/enriched into HTML with character formatting, font families,
	/// colors, relative font sizes, alignment, paragraph indentation, excerpts and language attributes.
	/// This does not interpret the older text/richtext format.</para>
	/// <para>Unknown commands and invalid parameters are ignored, but their text is retained.
	/// Parameter content is never displayed. Unmatched end commands are ignored; mismatched end
	/// commands close intervening commands, and unclosed commands are closed at end of input.
	/// Invalid command syntax is treated as literal text.</para>
	/// <para>Font sizes change in steps of one sixth of the base size, bounded between one sixth
	/// and twelve times that size. Indentation uses four-character steps. Font family parameters
	/// accept letters, digits, spaces, hyphens, underscores and periods; other names are ignored.</para>
	/// <para>CRLF, LF and CR line endings are accepted. Outside nofill, a single line ending becomes
	/// a space and N consecutive line endings become N-1 line breaks. Inside nofill, every line
	/// ending is retained. HTML paragraphs preserve spaces and tabs.</para>
	/// <para>Input text is HTML-escaped and formatting parameters are validated before use.
	/// No external resources are fetched and no links are generated from body text. Headers and footers explicitly
	/// marked as HTML are trusted caller-supplied markup, not sanitized input.</para>
	/// </remarks>
	public class EnrichedToHtml : TextConverter
	{
		int maxElementDepth = 4096;
		int maxParameterLength = 4096;
		int maxInputLength = 16 * 1024 * 1024;
		int maxOutputLength = 64 * 1024 * 1024;

		/// <summary>
		/// Initialize a new instance of the <see cref="EnrichedToHtml"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a text/enriched to HTML converter.
		/// </remarks>
		public EnrichedToHtml ()
		{
		}

		/// <summary>
		/// Get the input format.
		/// </summary>
		/// <remarks>
		/// Gets the input format.
		/// </remarks>
		/// <value>The enriched text format.</value>
		public override TextFormat InputFormat {
			get { return TextFormat.Enriched; }
		}

		/// <summary>
		/// Get the output format.
		/// </summary>
		/// <remarks>
		/// Gets the output format.
		/// </remarks>
		/// <value>The HTML format.</value>
		public override TextFormat OutputFormat {
			get { return TextFormat.Html; }
		}

		/// <summary>
		/// Get or set whether to output only an HTML fragment.
		/// </summary>
		/// <remarks>
		/// When false, the output is wrapped in html and body elements. The default is false.
		/// </remarks>
		/// <value>Whether to omit the document wrapper.</value>
		public bool OutputHtmlFragment {
			get; set;
		}

		/// <summary>
		/// Get or set the header format.
		/// </summary>
		/// <remarks>
		/// Text headers are converted using <see cref="TextToHtml"/>; HTML headers are written verbatim.
		/// </remarks>
		/// <value>The header format.</value>
		public HeaderFooterFormat HeaderFormat {
			get; set;
		}

		/// <summary>
		/// Get or set the footer format.
		/// </summary>
		/// <remarks>
		/// Text footers are converted using <see cref="TextToHtml"/>; HTML footers are written verbatim.
		/// </remarks>
		/// <value>The footer format.</value>
		public HeaderFooterFormat FooterFormat {
			get; set;
		}

		/// <summary>
		/// Get or set the maximum command nesting depth.
		/// </summary>
		/// <remarks>
		/// Counts recognized and unknown commands, including nested parameter commands.
		/// The default is 4096. Exceeding the limit throws <see cref="FormatException"/>.
		/// </remarks>
		/// <value>The maximum command nesting depth.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is less than one.
		/// </exception>
		public int MaxElementDepth {
			get { return maxElementDepth; }
			set {
				if (value < 1)
					throw new ArgumentOutOfRangeException (nameof (value));

				maxElementDepth = value;
			}
		}

		/// <summary>
		/// Get or set the maximum parameter length.
		/// </summary>
		/// <remarks>
		/// Limits the UTF-16 characters in a parameter, including ignored parameter content.
		/// Line endings are counted after normalization to LF. The default is 4096.
		/// Exceeding the limit throws <see cref="FormatException"/>.
		/// </remarks>
		/// <value>The maximum parameter length.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is less than one.
		/// </exception>
		public int MaxParameterLength {
			get { return maxParameterLength; }
			set {
				if (value < 1)
					throw new ArgumentOutOfRangeException (nameof (value));

				maxParameterLength = value;
			}
		}

		/// <summary>
		/// Get or set the maximum input length.
		/// </summary>
		/// <remarks>
		/// Limits all UTF-16 characters read from the input, including commands and suppressed parameters.
		/// Caller-supplied headers and footers are excluded. The default is 16777216.
		/// Exceeding the limit throws <see cref="FormatException"/>.
		/// </remarks>
		/// <value>The maximum input length.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is less than one.
		/// </exception>
		public int MaxInputLength {
			get { return maxInputLength; }
			set {
				if (value < 1)
					throw new ArgumentOutOfRangeException (nameof (value));

				maxInputLength = value;
			}
		}

		/// <summary>
		/// Get or set the maximum output length.
		/// </summary>
		/// <remarks>
		/// Limits all UTF-16 characters written, including HTML markup, escaped text, headers and footers.
		/// The default is 67108864. Exceeding the limit throws <see cref="FormatException"/>.
		/// </remarks>
		/// <value>The maximum output length.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is less than one.
		/// </exception>
		public int MaxOutputLength {
			get { return maxOutputLength; }
			set {
				if (value < 1)
					throw new ArgumentOutOfRangeException (nameof (value));

				maxOutputLength = value;
			}
		}

		sealed class LimitedWriter : TextWriter
		{
			readonly TextWriter destination;
			int remaining;

			public LimitedWriter (TextWriter destination, int limit)
			{
				this.destination = destination;
				remaining = limit;
			}

			public override Encoding Encoding { get { return destination.Encoding; } }

			void Reserve (int count)
			{
				if (count > remaining)
					throw new FormatException ("The enriched text output length limit has been exceeded.");

				remaining -= count;
			}

			public override void Write (char value)
			{
				Reserve (1);
				destination.Write (value);
			}

			public override void Write (string? value)
			{
				if (value == null)
					return;

				Reserve (value.Length);
				destination.Write (value);
			}

			public override void Write (char[] buffer, int index, int count)
			{
				Reserve (count);
				destination.Write (buffer, index, count);
			}

			public override void Flush ()
			{
				destination.Flush ();
			}
		}

		struct Formatting
		{
			public bool Bold, Italic, Underline, NoFill;
			public int Size, Left, Right, FirstLine, Excerpt;
			public string? Font, Color, Language, Alignment;
		}

		sealed class Command : HtmlTagContext
		{
			public readonly string Name;
			public readonly int Index;
			public readonly Formatting Previous;

			public Command (string name, int index, Formatting previous) : base (HtmlTagId.Unknown)
			{
				Name = name;
				Index = index;
				Previous = previous;
			}

			public override string TagName { get { return Name; } }
			public override HtmlAttributeCollection Attributes { get { return HtmlAttributeCollection.Empty; } }
			public override bool IsEmptyElementTag { get { return false; } }
			public override bool IsEndTag { get { return false; } }
		}

		sealed class Converter
		{
			readonly HtmlTagContextStack<Command> contexts = new HtmlTagContextStack<Command> ();
			readonly List<Command> commands = new List<Command> ();
			readonly StringBuilder parameter = new StringBuilder ();
			readonly StringBuilder css = new StringBuilder ();
			readonly char[] input = new char[4096];
			readonly char[] text = new char[1024];
			readonly char[] commandName = new char[60];
			readonly EnrichedToHtml options;
			readonly TextReader reader;
			readonly HtmlWriter html;
			Formatting state, lastLine;
			Command? parameterTarget;
			int inputIndex, inputCount, inputLength, textLength;
			int pendingCharacter = -1, newLines, parameterDepth, parameterLength;
			bool paragraphOpen, spanOpen, invalidParameter, implicitBreak, trailingNewLine;

			public Converter (EnrichedToHtml options, TextReader reader, HtmlWriter html)
			{
				this.options = options;
				this.reader = reader;
				this.html = html;
			}

			int ReadRaw ()
			{
				if (inputIndex == inputCount) {
					int remaining = options.MaxInputLength - inputLength;

					if (remaining == 0) {
						if (reader.Read () != -1)
							throw new FormatException ("The enriched text input length limit has been exceeded.");

						return -1;
					}

					inputCount = reader.Read (input, 0, Math.Min (remaining, input.Length));
					inputLength += inputCount;
					inputIndex = 0;

					if (inputCount == 0)
						return -1;
				}

				return input[inputIndex++];
			}

			int ReadCharacter ()
			{
				int c;

				if (pendingCharacter != -1) {
					c = pendingCharacter;
					pendingCharacter = -1;
				} else {
					c = ReadRaw ();
				}

				if (c != '\r')
					return c;

				c = ReadRaw ();
				if (c != '\n')
					pendingCharacter = c;

				return '\n';
			}

			static bool IsAsciiLetter (char c)
			{
				return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
			}

			static bool IsCommandCharacter (char c)
			{
				return IsAsciiLetter (c) || (c >= '0' && c <= '9') || c == '-';
			}

			static bool IsBlock (string name)
			{
				switch (name) {
				case "center": case "flushleft": case "flushright": case "flushboth":
				case "paraindent": case "nofill": case "excerpt":
					return true;
				default:
					return false;
				}
			}

			string GetStyle ()
			{
				if (css.Length > 0)
					css.Length--;

				return css.ToString ();
			}

			void AppendIndent (string property, long count)
			{
				if (count == 0)
					return;

				css.Append (property).Append (": ");
				css.Append ((count * 4).ToString (CultureInfo.InvariantCulture)).Append ("ch; ");
			}

			void OpenParagraph ()
			{
				if (paragraphOpen)
					return;

				html.WriteStartTag (HtmlTagId.Div);
				css.Clear ();

				if (state.NoFill)
					css.Append ("white-space: pre; ");

				if (state.NoFill && state.Alignment == "justify")
					css.Append ("text-align: left; ");
				else if (state.Alignment != null)
					css.Append ("text-align: ").Append (state.Alignment).Append ("; ");

				AppendIndent ("margin-left", (long) state.Left + state.Excerpt + Math.Max (0, -state.FirstLine));
				AppendIndent ("margin-right", state.Right);
				AppendIndent ("text-indent", state.FirstLine);

				if (css.Length > 0)
					html.WriteAttribute (HtmlAttributeId.Style, GetStyle ());

				paragraphOpen = true;
			}

			void OpenSpan ()
			{
				if (spanOpen)
					return;

				css.Clear ();
				if (state.Bold)
					css.Append ("font-weight: bold; ");

				if (state.Italic)
					css.Append ("font-style: italic; ");

				if (state.Underline)
					css.Append ("text-decoration: underline; ");

				if (state.Font != null)
					css.Append ("font-family: ").Append (state.Font).Append ("; ");

				if (state.Color != null)
					css.Append ("color: ").Append (state.Color).Append ("; ");

				if (state.Size != 0) {
					double size = Math.Max (1.0 / 6, Math.Min (12, 1 + state.Size / 6.0));
					css.Append ("font-size: ").Append (size.ToString ("0.###", CultureInfo.InvariantCulture)).Append ("em; ");
				}

				if (css.Length == 0 && state.Language == null)
					return;

				html.WriteStartTag (HtmlTagId.Span);
				if (css.Length > 0)
					html.WriteAttribute (HtmlAttributeId.Style, GetStyle ());

				if (state.Language != null)
					html.WriteAttribute ("lang", state.Language);

				spanOpen = true;
			}

			void FlushText ()
			{
				if (textLength == 0)
					return;

				OpenParagraph ();
				OpenSpan ();

				html.WriteText (text, 0, textLength);
				textLength = 0;
			}

			void CloseSpan ()
			{
				FlushText ();

				if (spanOpen) {
					html.WriteEndTag (HtmlTagId.Span);
					spanOpen = false;
				}
			}

			void CloseParagraph ()
			{
				CloseSpan ();

				if (paragraphOpen) {
					html.WriteEndTag (HtmlTagId.Div);
					paragraphOpen = false;
				}
			}

			void AppendText (char c)
			{
				implicitBreak = trailingNewLine = false;
				text[textLength++] = c;

				if (textLength == text.Length) {
					// Keep a surrogate pair together when HtmlWriter encodes the buffer.
					if (char.IsHighSurrogate (c)) {
						textLength--;
						FlushText ();
						text[textLength++] = c;
					} else {
						FlushText ();
					}
				}
			}

			void BreakParagraph ()
			{
				if (paragraphOpen || textLength > 0)
					implicitBreak = true;

				CloseParagraph ();
			}

			void FlushNewLines ()
			{
				if (newLines == 0)
					return;

				if (!state.NoFill && newLines == 1) {
					AppendText (' ');
				} else {
					int count = state.NoFill ? newLines : newLines - 1;
					if (implicitBreak)
						count--;

					implicitBreak = false;

					for (int i = 0; i < count; i++) {
						FlushText ();

						if (!paragraphOpen) {
							OpenParagraph ();
							html.WriteEmptyElementTag (HtmlTagId.Br);
						}

						CloseParagraph ();
					}

					trailingNewLine = true;
					lastLine = state;
				}

				newLines = 0;
			}

			void CountParameter (int count)
			{
				if (count > options.MaxParameterLength - parameterLength)
					throw new FormatException ("The enriched text parameter length limit has been exceeded.");

				parameterLength += count;
			}

			void Character (char c)
			{
				if (parameterDepth > 0) {
					CountParameter (1);

					if (parameterTarget != null && !invalidParameter)
						parameter.Append (c);

					return;
				}

				parameterTarget = null;

				if (c == '\n') {
					newLines++;
				} else {
					FlushNewLines ();
					AppendText (c);
				}
			}

			void CheckDepth (int extra)
			{
				if (commands.Count >= options.MaxElementDepth - extra)
					throw new FormatException ("The enriched text command depth limit has been exceeded.");
			}

			void StartCommand (string name)
			{
				CheckDepth (0);

				if (name == "param") {
					parameterDepth = 1;
					parameterLength = 0;
					invalidParameter = false;
					parameter.Clear ();
					return;
				}

				parameterTarget = null;

				if (IsBlock (name))
					BreakParagraph ();
				else
					CloseSpan ();

				var command = new Command (name, commands.Count, state);
				contexts.Push (command);
				commands.Add (command);

				switch (name) {
				case "bold": state.Bold = true; break;
				case "italic": state.Italic = true; break;
				case "underline": state.Underline = true; break;
				case "fixed": state.Font = "monospace"; break;
				case "bigger": state.Size++; break;
				case "smaller": state.Size--; break;
				case "nofill": state.NoFill = true; break;
				case "center": state.Alignment = "center"; break;
				case "flushleft": state.Alignment = "left"; break;
				case "flushright": state.Alignment = "right"; break;
				case "flushboth": state.Alignment = "justify"; break;
				case "excerpt": state.Excerpt++; break;
				case "fontfamily": case "color": case "lang": case "paraindent":
					parameterTarget = command;
					break;
				}
			}

			void CloseCommands (int index)
			{
				while (commands.Count > index) {
					var command = commands[commands.Count - 1];

					if (IsBlock (command.Name))
						BreakParagraph ();
					else
						CloseSpan ();

					state = command.Previous;
					contexts.Pop (command.Name);
					commands.RemoveAt (commands.Count - 1);
				}
			}

			static string? ParseColor (string value)
			{
				switch (value.ToLowerInvariant ()) {
				case "red": case "blue": case "green": case "yellow":
				case "cyan": case "magenta": case "black": case "white":
					return value.ToLowerInvariant ();
				}

				if (value.Length != 14 || value[4] != ',' || value[9] != ',')
					return null;

				int rgb = 0;

				for (int i = 0; i < 3; i++) {
					int component = 0;
					for (int j = 0; j < 4; j++) {
						char c = value[i * 5 + j];
						int digit;

						if (c >= '0' && c <= '9')
							digit = c - '0';
						else if (c >= 'a' && c <= 'f')
							digit = c - 'a' + 10;
						else if (c >= 'A' && c <= 'F')
							digit = c - 'A' + 10;
						else
							return null;

						component = (component << 4) | digit;
					}

					rgb = (rgb << 8) | ((component + 128) / 257);
				}

				return "#" + rgb.ToString ("x6", CultureInfo.InvariantCulture);
			}

			static bool IsFontFamily (string value)
			{
				if (value.Length == 0)
					return false;

				foreach (char c in value) {
					if (!char.IsLetterOrDigit (c) && c != ' ' && c != '-' && c != '_' && c != '.')
						return false;
				}

				return true;
			}

			static bool IsLanguage (string value)
			{
				int length = 0;
				bool first = true;

				foreach (char c in value) {
					if (c == '-') {
						if (length == 0)
							return false;

						first = false;
						length = 0;
					} else {
						if (!IsAsciiLetter (c) && (first || c < '0' || c > '9'))
							return false;

						if (++length > 8)
							return false;
					}
				}

				return length > 0;
			}

			void ApplyIndent (string value)
			{
				int left = 0, right = 0, inward = 0, outward = 0;

				foreach (var item in value.Split (',')) {
					switch (item.Trim ().ToLowerInvariant ()) {
					case "left": left++; break;
					case "right": right++; break;
					case "in": inward++; break;
					case "out": outward++; break;
					default: return;
					}
				}

				if (inward > 0 && outward > 0)
					return;

				state.Left += left;
				state.Right += right;
				state.FirstLine += inward - outward;
			}

			void ApplyParameter ()
			{
				if (parameterTarget == null || invalidParameter)
					return;

				string value = parameter.ToString ().Trim ();

				switch (parameterTarget.Name) {
				case "color":
					state.Color = ParseColor (value) ?? state.Color;
					break;
				case "fontfamily":
					if (IsFontFamily (value))
						state.Font = "'" + value + "'";
					break;
				case "lang":
					if (IsLanguage (value))
						state.Language = value;
					break;
				case "paraindent":
					ApplyIndent (value);
					break;
				}
			}

			void ProcessCommand (string name, bool endTag)
			{
				if (parameterDepth > 0) {
					if (name == "param" && endTag) {
						if (--parameterDepth == 0) {
							ApplyParameter ();
							parameterTarget = null;
							parameter.Clear ();
							return;
						}
					} else if (name == "param") {
						CheckDepth (parameterDepth);
						parameterDepth++;
					}

					invalidParameter = true;
					CountParameter (name.Length + (endTag ? 3 : 2));
					return;
				}

				FlushNewLines ();

				if (endTag) {
					var command = contexts.Peek (name);

					parameterTarget = null;

					if (command != null)
						CloseCommands (command.Index);
				} else {
					StartCommand (name);
				}
			}

			void ReadCommand ()
			{
				int c = ReadCharacter ();

				if (c == '<') {
					if (parameterDepth > 0)
						CountParameter (1);

					Character ('<');
					return;
				}

				bool endTag = c == '/';

				if (endTag)
					c = ReadCharacter ();

				int length = 0;

				while (c != -1 && length < commandName.Length && IsCommandCharacter ((char) c)) {
					commandName[length++] = (char) c;
					c = ReadCharacter ();
				}

				if (length > 0 && c == '>') {
					string name = new string (commandName, 0, length).ToLowerInvariant ();
					ProcessCommand (name, endTag);
					return;
				}

				Character ('<');

				if (endTag)
					Character ('/');

				for (int i = 0; i < length; i++)
					Character (commandName[i]);

				if (c != -1)
					Character ((char) c);
			}

			public void Convert ()
			{
				int c;

				html.WriteStartTag (HtmlTagId.Div);
				html.WriteAttribute (HtmlAttributeId.Style, "white-space: pre-wrap;");

				while ((c = ReadCharacter ()) != -1) {
					if (c == '<')
						ReadCommand ();
					else
						Character ((char) c);
				}

				FlushNewLines ();
				CloseCommands (0);
				CloseParagraph ();

				if (trailingNewLine) {
					state = lastLine;
					OpenParagraph ();
					html.WriteEmptyElementTag (HtmlTagId.Br);
					CloseParagraph ();
				}

				html.WriteEndTag (HtmlTagId.Div);
				html.Flush ();
			}
		}

		static void WriteHeaderFooter (TextWriter writer, string? value, HeaderFooterFormat format)
		{
			if (string.IsNullOrEmpty (value))
				return;

			if (format == HeaderFooterFormat.Text) {
				var converter = new TextToHtml { OutputHtmlFragment = true };

				using (var reader = new StringReader (value))
					converter.Convert (reader, writer);
			} else {
				writer.Write (value);
			}
		}

		/// <summary>
		/// Convert enriched text to HTML.
		/// </summary>
		/// <remarks>
		/// Does not close caller-owned readers or writers. On an I/O error or a limit violation,
		/// the destination may already contain partial output.
		/// </remarks>
		/// <param name="reader">The enriched text reader.</param>
		/// <param name="writer">The HTML writer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="reader"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="writer"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.FormatException">
		/// An input, output, parameter length or command depth limit was exceeded.
		/// </exception>
		public override void Convert (TextReader reader, TextWriter writer)
		{
			if (reader is null)
				throw new ArgumentNullException (nameof (reader));

			if (writer is null)
				throw new ArgumentNullException (nameof (writer));

			using var limited = new LimitedWriter (writer, MaxOutputLength);

			if (!OutputHtmlFragment)
				limited.Write ("<html><body>");

			WriteHeaderFooter (limited, Header, HeaderFormat);

			using (var html = new HtmlWriter (limited, true))
				new Converter (this, reader, html).Convert ();

			WriteHeaderFooter (limited, Footer, FooterFormat);

			if (!OutputHtmlFragment)
				limited.Write ("</body></html>");
		}
	}
}

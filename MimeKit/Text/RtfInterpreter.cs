//
// RtfInterpreter.cs
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
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MimeKit.Text {
	/// <summary>
	/// The destination that the current group's content is directed to.
	/// </summary>
	/// <remarks>
	/// RTF 1.9.1, "Destinations": certain control words at the start of a group change where the group's text goes.
	/// Destinations that are skipped completely (pictures, style sheets, etc) are not represented here because they
	/// are handled by a counter rather than by group state (see <see cref="RtfInterpreter"/>).
	/// </remarks>
	enum RtfDestination : byte
	{
		Normal,
		FontTable,
		ColorTable,
		FieldInstruction,

		// [MS-OXRTFEX] 2.1.3.1.3: the content of \*\htmltag is raw HTML.
		HtmlTag,

		// RTF 1.9.1, "Unicode RTF": {\upr {ansi} {\*\ud {unicode}}}. The ANSI alternative is ignored while the
		// \ud alternative is rendered.
		Upr
	}

	enum RtfAlignment : byte
	{
		Left,
		Center,
		Right,
		Justify
	}

	enum RtfVerticalAlignment : byte
	{
		Baseline,
		Superscript,
		Subscript
	}

	[Flags]
	enum RtfStateFlags : ushort
	{
		None      = 0,
		Bold      = 1 << 0,
		Italic    = 1 << 1,
		Underline = 1 << 2,
		Strike    = 1 << 3,
		Hidden    = 1 << 4,
		InTable   = 1 << 5,

		// [MS-OXRTFEX] 2.1.3.1.3: "The state of the HTMLRTF control word MUST transfer when entering groups and be
		// restored when exiting groups", so it is part of the group state like any other toggle.
		HtmlRtf   = 1 << 6,

		CharacterFormatting = Bold | Italic | Underline | Strike | Hidden
	}

	/// <summary>
	/// The group-scoped RTF state.
	/// </summary>
	/// <remarks>
	/// <para>RTF 1.9.1, "Groups": "Formatting specified within a group affects only the text within that group"
	/// and text after the group inherits the formatting that was in effect before it. The reader therefore saves
	/// this state on '{' and restores it on '}'.</para>
	/// <para>This is deliberately a small value type so that saving and restoring it on group boundaries is O(1)
	/// and so that two states can be compared cheaply in order to fold identical stack entries.</para>
	/// <para>Note that <c>\ucN</c> is group-scoped (RTF 1.9.1, "Unicode RTF"), so <see cref="UnicodeSkip"/> lives here
	/// too.</para>
	/// </remarks>
	struct RtfGroupState : IEquatable<RtfGroupState>
	{
		public RtfDestination Destination;
		public RtfAlignment Alignment;
		public RtfVerticalAlignment VerticalAlignment;
		public RtfStateFlags Flags;
		public int Font;
		public int FontSize;
		public int ForegroundColor;
		public int BackgroundColor;
		public int UnicodeSkip;

		// The target of the HYPERLINK field whose result is being read, if any. RTF 1.9.1, "Fields": the field
		// result is a group, and fields may be nested within another field's result, so the link target must be
		// scoped to the \fldrslt group rather than tracked globally; otherwise, text following a nested field
		// would be attributed to the nested field's link. Compared by reference so that folding stays O(1).
		public string? Hyperlink;

		public readonly bool Bold => (Flags & RtfStateFlags.Bold) != 0;
		public readonly bool Italic => (Flags & RtfStateFlags.Italic) != 0;
		public readonly bool Underline => (Flags & RtfStateFlags.Underline) != 0;
		public readonly bool Strike => (Flags & RtfStateFlags.Strike) != 0;
		public readonly bool Hidden => (Flags & RtfStateFlags.Hidden) != 0;
		public readonly bool InTable => (Flags & RtfStateFlags.InTable) != 0;
		public readonly bool HtmlRtf => (Flags & RtfStateFlags.HtmlRtf) != 0;

		public void SetFlag (RtfStateFlags flag, bool value)
		{
			if (value)
				Flags |= flag;
			else
				Flags &= ~flag;
		}

		public readonly bool Equals (RtfGroupState other)
		{
			return Destination == other.Destination &&
				Alignment == other.Alignment &&
				VerticalAlignment == other.VerticalAlignment &&
				Flags == other.Flags &&
				Font == other.Font &&
				FontSize == other.FontSize &&
				ForegroundColor == other.ForegroundColor &&
				BackgroundColor == other.BackgroundColor &&
				UnicodeSkip == other.UnicodeSkip &&
				ReferenceEquals (Hyperlink, other.Hyperlink);
		}

		public override readonly bool Equals (object? obj)
		{
			return obj is RtfGroupState other && Equals (other);
		}

		public override readonly int GetHashCode ()
		{
			return ((int) Flags << 16) ^ Font ^ (FontSize << 8) ^ ForegroundColor ^ (BackgroundColor << 4) ^ (int) Destination;
		}
	}

	/// <summary>
	/// Receives the content produced by an <see cref="RtfInterpreter"/>.
	/// </summary>
	abstract class RtfContentHandler
	{
		/// <summary>
		/// Called exactly once, before the first content event (or at the end of the input if there is no content).
		/// </summary>
		/// <remarks>
		/// By the time this is called, <see cref="RtfInterpreter.FromHtml"/> is final ([MS-OXRTFEX] 2.2.3.1 limits
		/// recognition to the first 10 tokens), so a handler can use it to decide how to render the document.
		/// </remarks>
		public virtual void OnBegin (RtfInterpreter rtf)
		{
		}

		public virtual void OnText (RtfInterpreter rtf, char[] buffer, int index, int count)
		{
		}

		public virtual void OnParagraph (RtfInterpreter rtf)
		{
		}

		public virtual void OnLineBreak (RtfInterpreter rtf)
		{
		}

		public virtual void OnCell (RtfInterpreter rtf)
		{
		}

		public virtual void OnRow (RtfInterpreter rtf)
		{
		}

		public virtual void OnHtmlTag (RtfInterpreter rtf, char[] buffer, int index, int count)
		{
		}
	}

	/// <summary>
	/// An RTF interpreter that tracks group state, font and color tables and character set decoding and
	/// reports the resulting content to an <see cref="RtfContentHandler"/>.
	/// </summary>
	/// <remarks>
	/// <para>The interpreter is designed to be resilient against hostile input:</para>
	/// <list type="bullet">
	/// <item><description>Groups are tracked using an explicit stack (no recursion). Consecutive nested groups
	/// that share the same state are folded into a single stack entry with a repeat count, so arbitrarily
	/// deep nesting such as <c>{{{{...}}}}</c> uses O(1) memory. Only nested groups with distinct state
	/// consume stack entries and the number of those is limited by <c>maxGroupDepth</c>. Groups that would
	/// exceed that limit are skipped entirely using a single counter.</description></item>
	/// <item><description>Skipped destinations (pictures, objects, style sheets, etc) are skipped using a
	/// single counter without pushing any state.</description></item>
	/// <item><description>The font table and color table are limited by <c>maxFontTableEntries</c> and
	/// <c>maxColorTableEntries</c>. Font names are never stored.</description></item>
	/// <item><description>The number of distinct code page decoders that will be created is bounded.</description></item>
	/// <item><description>Field instructions are truncated to <see cref="MaxFieldInstructionLength"/>.</description></item>
	/// </list>
	/// <para>The interpreter never recurses and never buffers the document; it does a constant amount of work per
	/// token (amortized) and its memory use is bounded by the configured limits.</para>
	/// </remarks>
	sealed class RtfInterpreter
	{
		// Field instructions are only examined for a HYPERLINK target, which never needs to be anywhere near this long.
		internal const int MaxFieldInstructionLength = 2048;

		// Every font can name a different code page (\fcharsetN or \cpgN), so cap the number of decoders that a
		// hostile font table can make us instantiate.
		const int MaxCachedDecoders = 32;

		// RTF 1.9.1, "Character Set": \ansi (Windows-1252) is the default character set.
		const int DefaultCodePage = 1252;

		// RTF 1.9.1, "Font (Character) Formatting Properties": the default \fsN is 24 half-points (12pt).
		internal const int DefaultFontSize = 24;

		// [MS-OXRTFEX] 2.1.3.1.4.2 / 2.2.3.2: \par and \line become CRLF in the extracted HTML.
		static readonly char[] CrLf = { '\r', '\n' };
		static readonly char[] ReplacementChar = { '\uFFFD' };
		static readonly Encoding FallbackEncoding;

		readonly Dictionary<int, Decoder> decoders = new Dictionary<int, Decoder> ();
		readonly Dictionary<int, int> fonts = new Dictionary<int, int> ();
		readonly StringBuilder fieldInstruction = new StringBuilder ();
		readonly List<int> colors = new List<int> ();
		readonly RtfContentHandler handler;
		readonly RtfTokenizer tokenizer;
		readonly int maxFontTableEntries;
		readonly int maxColorTableEntries;
		readonly int maxGroupDepth;
		readonly char[] scratch = new char[2];
		char highSurrogate;
		readonly byte[] bytes = new byte[1024];
		char[] chars = new char[1024];

		// An entry in the folded group stack: State was saved Count times in a row by consecutive '{'s.
		// The count is kept here rather than in RtfGroupState so that it does not leak into the formatting
		// state exposed to handlers or take part in the equality check used to decide whether to fold.
		struct GroupStackEntry
		{
			public RtfGroupState State;
			public long Count;
		}

		GroupStackEntry[] stack;
		int stackCount;
		long depth;

		// Set once the outermost group has been closed. Writers sometimes emit an extra '}' that closes the root
		// group early, so content may follow it; groups there are not the {\rtf1 ...} root and may be skipped.
		bool rootGroupClosed;
		// skipped groups: the nesting level within the group being skipped, and whether reaching the end of
		// that group should also restore a saved state (i.e. whether the skipped group was pushed).
		long skipDepth;
		bool skipPopsGroup;

		RtfGroupState current;

		// RTF 1.9.1, "Destinations": set by \* and applies only to the control word that immediately follows it.
		bool ignorableDestination;

		// RTF 1.9.1, "Unicode RTF": the number of fallback characters still to be skipped after a \uN.
		int unicodeSkipRemaining;
		int documentCodePage = DefaultCodePage;
		int defaultFont;
		bool finished;
		bool begun;

		// [MS-OXRTFEX] 2.2.3.1: \fromhtml is only recognized among the first 10 tokens, all of which must be '{' or
		// control words. This keeps a \fromhtml1 appearing later in an ordinary document from switching modes.
		const int EncapsulationRecognitionTokens = 10;
		int recognitionTokens;
		bool recognitionWindowOpen = true;

		// pending font table entry (committed on ';', the next \f, or the end of the group that the \f appeared in)
		bool pendingFont;
		long pendingFontDepth;
		int pendingFontNumber;
		int pendingFontCharset;
		int pendingFontCodePage;

		// pending color table entry
		bool pendingColor;
		int pendingRed, pendingGreen, pendingBlue;

		// byte decoding: 8-bit bytes are accumulated and decoded together so that multi-byte (DBCS) characters
		// split across several \'hh escapes are decoded correctly.
		int byteCount;
		int bytesCodePage;
		Decoder? defaultDecoder;

		static RtfInterpreter ()
		{
			// Windows-1252 is not available on .NET Core unless the application has registered
			// CodePagesEncodingProvider, so fall back to Latin-1, which is its closest built-in approximation.
			try {
				FallbackEncoding = Encoding.GetEncoding (DefaultCodePage);
			} catch {
				FallbackEncoding = Encoding.GetEncoding (28591);
			}
		}

		public RtfInterpreter (TextReader reader, RtfContentHandler handler, int maxGroupDepth, int maxFontTableEntries, int maxColorTableEntries)
		{
			tokenizer = new RtfTokenizer (reader);
			this.handler = handler;
			this.maxGroupDepth = maxGroupDepth;
			this.maxFontTableEntries = maxFontTableEntries;
			this.maxColorTableEntries = maxColorTableEntries;

			// The stack grows on demand so that small documents do not pay for maxGroupDepth up front.
			stack = new GroupStackEntry[Math.Min (16, maxGroupDepth)];

			// RTF 1.9.1, "Unicode RTF": "the default [value of \ucN] is 1".
			current.UnicodeSkip = 1;

			// RTF 1.9.1, "Font (Character) Formatting Properties": "\fsN Font size in half-points (the default is 24)".
			current.FontSize = DefaultFontSize;
		}

		/// <summary>
		/// Get or set whether encapsulated HTML ([MS-OXRTFEX]) should be extracted.
		/// </summary>
		/// <remarks>
		/// When <see langword="true" /> and the document contains <c>\fromhtml1</c>, the interpreter acts as the
		/// "de-encapsulating RTF reader" described by [MS-OXRTFEX] 2.2.3.2: the content of <c>\*\htmltag</c>
		/// destinations is reported via <see cref="RtfContentHandler.OnHtmlTag"/> and content within
		/// <c>\htmlrtf</c> blocks is suppressed. Otherwise those are ignored and the RTF is rendered as-is.
		/// </remarks>
		public bool ExtractHtml {
			get; set;
		}

		/// <summary>
		/// Get whether the document declared that it was generated from HTML (<c>\fromhtml1</c>).
		/// </summary>
		public bool FromHtml {
			get; private set;
		}

		bool ExtractingHtml {
			get { return ExtractHtml && FromHtml; }
		}

		/// <summary>
		/// Get the current group state.
		/// </summary>
		public ref readonly RtfGroupState State {
			get { return ref current; }
		}

		/// <summary>
		/// Get the hyperlink target parsed from the most recent field instruction, if any.
		/// </summary>
		/// <remarks>
		/// This is only meaningful while reading a field; use <see cref="RtfGroupState.Hyperlink"/> on
		/// <see cref="State"/> to get the link target that applies to the current text.
		/// </remarks>
		public string? FieldHyperlink {
			get; private set;
		}

		/// <summary>
		/// Get the number of folded group stack entries currently in use.
		/// </summary>
		internal int StackEntries {
			get { return stackCount; }
		}

		/// <summary>
		/// Get the current group nesting depth.
		/// </summary>
		internal long Depth {
			get { return depth + skipDepth; }
		}

		/// <summary>
		/// Get the number of font table entries.
		/// </summary>
		internal int FontCount {
			get { return fonts.Count; }
		}

		/// <summary>
		/// Get the number of color table entries.
		/// </summary>
		internal int ColorCount {
			get { return colors.Count; }
		}

		/// <summary>
		/// Get the RGB value of the specified color table entry.
		/// </summary>
		/// <returns>The RGB value (<c>0xRRGGBB</c>) or <c>-1</c> for the automatic or an undefined color.</returns>
		/// <param name="index">The color table index.</param>
		public int GetColor (int index)
		{
			if (index < 0 || index >= colors.Count)
				return -1;

			return colors[index];
		}

		/// <summary>
		/// Process the next token.
		/// </summary>
		/// <returns><see langword="true" /> if a token was processed; otherwise, <see langword="false" /> if the end of the input was reached.</returns>
		public bool Step ()
		{
			if (!tokenizer.ReadNextToken ()) {
				Finish ();
				return false;
			}

			ProcessToken ();

			return true;
		}

		/// <summary>
		/// Asynchronously process the next token.
		/// </summary>
		/// <returns><see langword="true" /> if a token was processed; otherwise, <see langword="false" /> if the end of the input was reached.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		public async Task<bool> StepAsync (CancellationToken cancellationToken = default)
		{
			if (!await tokenizer.ReadNextTokenAsync (cancellationToken).ConfigureAwait (false)) {
				Finish ();
				return false;
			}

			ProcessToken ();

			return true;
		}

		void Finish ()
		{
			if (finished)
				return;

			finished = true;
			FlushBytes ();
			FlushHighSurrogate ();
			Begin ();
		}

		void Begin ()
		{
			if (begun)
				return;

			begun = true;
			recognitionWindowOpen = false;
			handler.OnBegin (this);
		}

		void ProcessToken ()
		{
			var kind = tokenizer.Kind;

			if (recognitionWindowOpen) {
				if ((kind != RtfTokenKind.GroupStart && kind != RtfTokenKind.ControlWord) || ++recognitionTokens > EncapsulationRecognitionTokens) {
					// The recognition window is over, so the handler can now decide how to render the document.
					Begin ();
				}
			}

			// While skipping a group, only the braces matter; everything else (including \binN data, which the
			// tokenizer has already removed) is discarded. This needs O(1) memory regardless of how deeply the
			// skipped content is nested.
			if (skipDepth > 0) {
				if (kind == RtfTokenKind.GroupStart) {
					skipDepth++;
				} else if (kind == RtfTokenKind.GroupEnd) {
					if (--skipDepth == 0 && skipPopsGroup)
						PopGroup ();
				}
				return;
			}

			switch (kind) {
			case RtfTokenKind.Text:
				ignorableDestination = false;
				ProcessText ();
				return;
			case RtfTokenKind.HexChar:
				ignorableDestination = false;
				ProcessHexChar ();
				return;
			case RtfTokenKind.ControlSymbol:
				ProcessControlSymbol ();
				return;
			}

			// Text, hex escapes and some control symbols may contribute bytes to a multi-byte character that is still
			// being accumulated. Every other token ends any such sequence, so decode what we have first.
			FlushBytes ();

			// Only a \uN that is the second half of a surrogate pair may follow a pending high surrogate.
			if (kind != RtfTokenKind.ControlWord || tokenizer.KeywordId != RtfKeyword.U)
				FlushHighSurrogate ();

			switch (kind) {
			case RtfTokenKind.GroupStart:
				// RTF 1.9.1, "Unicode RTF": a group boundary ends the run of \uN fallback characters.
				ignorableDestination = false;
				unicodeSkipRemaining = 0;
				PushGroup ();
				break;
			case RtfTokenKind.GroupEnd:
				ignorableDestination = false;
				unicodeSkipRemaining = 0;
				PopGroup ();
				break;
			case RtfTokenKind.ControlWord:
				ProcessControlWord ();
				break;
			}
		}

		#region Group Stack

		// Save the current state on '{'.
		//
		// Stack folding: rather than pushing one entry per group, consecutive pushes of an identical state share a
		// single entry with a repeat count. Pathological input like 1,000,000 nested '{' therefore needs one entry,
		// while ordinary documents (whose groups typically change at least one property) behave exactly like a
		// normal stack. Only distinct nested states consume entries and those are capped by maxGroupDepth.
		void PushGroup ()
		{
			if (stackCount > 0 && stack[stackCount - 1].State.Equals (current)) {
				// Fold this group into the previous entry since it saved an identical state.
				stack[stackCount - 1].Count++;
				depth++;
				return;
			}

			if (stackCount >= maxGroupDepth) {
				// Too many distinct nested states: skip this group (and everything nested within it) entirely.
				// Nothing was pushed, so the closing '}' must not pop a state (skipPopsGroup = false). Dropping
				// the content is preferable to rendering it with the wrong formatting or throwing.
				skipPopsGroup = false;
				skipDepth = 1;
				return;
			}

			if (stackCount == stack.Length)
				Array.Resize (ref stack, (int) Math.Min ((long) stack.Length * 2, maxGroupDepth));

			ref var entry = ref stack[stackCount++];
			entry.State = current;
			entry.Count = 1;
			depth++;
		}

		// Restore the saved state on '}'.
		void PopGroup ()
		{
			if (stackCount == 0) {
				// Unbalanced '}'. RTF 1.9.1 does not define this, but real-world writers emit it, so be lenient
				// and ignore it rather than failing the whole conversion.
				return;
			}

			var previous = current;

			ref var top = ref stack[stackCount - 1];

			current = top.State;

			if (--top.Count == 0)
				stackCount--;

			depth--;

			if (depth == 0)
				rootGroupClosed = true;

			// RTF 1.9.1, "Font Table": the last font entry may be terminated by the end of its group rather than ';'.
			// Only the end of the group that the \fN appeared in terminates the entry; the end of a group nested
			// within the entry (e.g. {\*\panose ...} or {\*\falt ...}, which may precede \fcharsetN) does not.
			if (pendingFont && depth < pendingFontDepth)
				CommitFont ();

			// RTF 1.9.1, "Fields": {\field {\*\fldinst ...} {\fldrslt ...}}. The instruction is complete once we
			// leave the \fldinst group, which happens before \fldrslt, so the link target is known when the
			// result text is rendered.
			if (previous.Destination == RtfDestination.FieldInstruction && current.Destination != RtfDestination.FieldInstruction)
				FieldHyperlink = ParseHyperlink (fieldInstruction);
		}

		// Skip the rest of the current group. The group's state was already pushed by its '{', so the matching '}'
		// must restore it (skipPopsGroup = true).
		void SkipCurrentGroup ()
		{
			// The outermost {\rtf1 ...} group is never skipped; a destination keyword there is malformed and
			// skipping it would discard the entire document.
			if (depth <= 1 && !rootGroupClosed)
				return;

			skipPopsGroup = true;
			skipDepth = 1;
		}

		#endregion

		#region Character Decoding

		// RTF 1.9.1, "Font Table": maps \fcharsetN to a Windows code page. Charsets 0 (ANSI), 1 (default) and
		// 2 (symbol), as well as any unknown charset, use the document code page (\ansicpgN).
		static int GetCodePageFromCharset (int charset)
		{
			switch (charset) {
			case 77: return 10000;
			case 128: return 932;
			case 129: return 949;
			case 130: return 1361;
			case 134: return 936;
			case 136: return 950;
			case 161: return 1253;
			case 162: return 1254;
			case 163: return 1258;
			case 177: return 1255;
			case 178: return 1256;
			case 186: return 1257;
			case 204: return 1251;
			case 222: return 874;
			case 238: return 1250;
			case 254: return 437;
			case 255: return 850;
			default: return 0; // use the document code page
			}
		}

		// RTF 1.9.1, "Character Set" and "Font Table": 8-bit text is interpreted using the code page of the current
		// font if it specifies one, otherwise using the document code page.
		int GetCurrentCodePage ()
		{
			// [MS-OXRTFEX] 2.2.3.2: text inside an HTMLTAG destination group is always interpreted using the document's
			// default code page, regardless of the current font.
			if (current.Destination == RtfDestination.HtmlTag)
				return documentCodePage;

			if (fonts.TryGetValue (current.Font, out int codepage) && codepage > 0)
				return codepage;

			return documentCodePage;
		}

		Decoder GetDecoder (int codepage)
		{
			if (decoders.TryGetValue (codepage, out var decoder))
				return decoder;

			if (decoders.Count >= MaxCachedDecoders)
				return defaultDecoder ??= FallbackEncoding.GetDecoder ();

			Encoding? encoding = null;

			// Unknown or unavailable code pages fall back to the default. The result (including the failure) is cached
			// so that a document cannot make us repeatedly throw and catch exceptions for the same code page.
			if (codepage > 0) {
				try {
					encoding = Encoding.GetEncoding (codepage);
				} catch {
					encoding = null;
				}
			}

			decoder = (encoding ?? FallbackEncoding).GetDecoder ();
			decoders.Add (codepage, decoder);

			return decoder;
		}

		void DecodeBytes (bool flush)
		{
			var decoder = GetDecoder (bytesCodePage);

			// Ask the decoder how many chars it will produce rather than assuming one char per byte: some decoders
			// (e.g. stateful ISO-2022 or ISCII decoders, or a decoder holding a partial sequence from a previous
			// call) can emit more chars than bytes, and an undersized buffer would make GetChars throw.
			// GetCharCount does not change the decoder's state.
			int maxChars = decoder.GetCharCount (bytes, 0, byteCount, flush);

			if (chars.Length < maxChars)
				chars = new char[Math.Max (maxChars, chars.Length * 2)];

			int count = decoder.GetChars (bytes, 0, byteCount, chars, 0, flush);
			byteCount = 0;

			if (count > 0)
				EmitChars (chars, 0, count);
		}

		void FlushBytes ()
		{
			if (byteCount > 0)
				DecodeBytes (true);
		}

		void AppendByte (byte value, int codepage)
		{
			if (codepage != bytesCodePage) {
				FlushBytes ();
				bytesCodePage = codepage;
			}

			// If the buffer is full, decode without flushing so that the decoder keeps any incomplete lead byte.
			if (byteCount == bytes.Length)
				DecodeBytes (false);

			bytes[byteCount++] = value;
		}

		#endregion

		#region Content

		// RTF 1.9.1, "Font (Character) Formatting Properties": \v is hidden text.
		// [MS-OXRTFEX] 2.2.3.2: when de-encapsulating HTML, the reader "MUST ... Ignore and skip any text and RTF
		// control words that are suppressed by any HTMLRTF control word"; that content only exists for RTF readers.
		bool IsContentSuppressed {
			get { return current.Hidden || (current.HtmlRtf && ExtractingHtml); }
		}

		void EmitChars (char[] buffer, int index, int count)
		{
			FlushHighSurrogate ();

			switch (current.Destination) {
			case RtfDestination.Normal:
				if (!IsContentSuppressed) {
					Begin ();
					handler.OnText (this, buffer, index, count);
				}
				break;
			case RtfDestination.HtmlTag:
				Begin ();
				handler.OnHtmlTag (this, buffer, index, count);
				break;
			case RtfDestination.FieldInstruction:
				count = Math.Min (count, MaxFieldInstructionLength - fieldInstruction.Length);
				if (count > 0)
					fieldInstruction.Append (buffer, index, count);
				break;
			case RtfDestination.FontTable:
				// Font names are not needed, so only look for the ';' that terminates each entry (RTF 1.9.1, "Font
				// Table"). Not storing names is what keeps a huge font table from consuming memory.
				for (int i = 0; i < count; i++) {
					if (buffer[index + i] == ';')
						CommitFont ();
				}
				break;
			case RtfDestination.ColorTable:
				// RTF 1.9.1, "Color Table": each entry is terminated by ';'.
				for (int i = 0; i < count; i++) {
					if (buffer[index + i] == ';')
						CommitColor ();
				}
				break;
			}
		}

		void EmitChar (char c)
		{
			FlushBytes ();
			scratch[0] = c;
			EmitChars (scratch, 0, 1);
		}

		// A high surrogate that is not immediately followed by a low surrogate is malformed UTF-16; replace it with
		// U+FFFD so that the output can always be encoded (strict encoders throw on unpaired surrogates).
		void FlushHighSurrogate ()
		{
			if (highSurrogate == '\0')
				return;

			highSurrogate = '\0';
			EmitChars (ReplacementChar, 0, 1);
		}

		// RTF 1.9.1, "Unicode RTF": characters outside the BMP are written as a pair of \uN control words holding the
		// UTF-16 high and low surrogates. Hold on to a high surrogate until we know whether its pair follows.
		void EmitUnicodeChar (char c)
		{
			FlushBytes ();

			if (char.IsLowSurrogate (c) && highSurrogate != '\0') {
				scratch[0] = highSurrogate;
				scratch[1] = c;
				highSurrogate = '\0';
				EmitChars (scratch, 0, 2);
			} else if (char.IsHighSurrogate (c)) {
				FlushHighSurrogate ();
				highSurrogate = c;
			} else {
				EmitChar (char.IsLowSurrogate (c) ? '\uFFFD' : c);
			}
		}

		void EmitParagraph ()
		{
			FlushBytes ();

			switch (current.Destination) {
			case RtfDestination.Normal:
				if (!IsContentSuppressed) {
					Begin ();
					handler.OnParagraph (this);
				}
				break;
			case RtfDestination.HtmlTag:
				EmitHtmlNewLine ();
				break;
			}
		}

		void EmitLineBreak ()
		{
			FlushBytes ();

			switch (current.Destination) {
			case RtfDestination.Normal:
				if (!IsContentSuppressed) {
					Begin ();
					handler.OnLineBreak (this);
				}
				break;
			case RtfDestination.HtmlTag:
				EmitHtmlNewLine ();
				break;
			}
		}

		// [MS-OXRTFEX] 2.2.3.2: \par and \line within an HTMLTAG destination are converted to CRLF.
		void EmitHtmlNewLine ()
		{
			Begin ();
			handler.OnHtmlTag (this, CrLf, 0, 2);
		}

		void ProcessText ()
		{
			if (current.Destination == RtfDestination.Upr)
				return;

			var buffer = tokenizer.TextBuffer;
			int index = tokenizer.TextIndex;
			int endIndex = index + tokenizer.TextLength;

			// RTF 1.9.1, "Unicode RTF": skip the ANSI fallback for a preceding \uN. Each character counts as one.
			if (unicodeSkipRemaining > 0) {
				int n = Math.Min (unicodeSkipRemaining, endIndex - index);
				unicodeSkipRemaining -= n;
				index += n;
			}

			if (index == endIndex)
				return;

			int codepage = GetCurrentCodePage ();

			for (int i = index; i < endIndex; i++) {
				char c = buffer[i];

				if (c > 0xFF) {
					// The caller chose an InputEncoding other than Latin-1 (e.g. UTF-8), so this character has
					// already been decoded and cannot be a code page byte.
					EmitChar (c);
				} else if (c >= 0x20 || c == '\t') {
					// Raw 8-bit bytes are technically invalid in RTF (which is 7-bit), but writers emit them; they
					// are decoded with the current font's code page exactly like \'hh. Other C0 control characters
					// have no meaning in RTF text and could be harmful in the output, so they are dropped.
					AppendByte ((byte) c, codepage);
				}
			}
		}

		void ProcessHexChar ()
		{
			// RTF 1.9.1, "Unicode RTF": a \'hh escape counts as a single fallback character.
			if (unicodeSkipRemaining > 0) {
				unicodeSkipRemaining--;
				return;
			}

			if (current.Destination == RtfDestination.Upr)
				return;

			AppendByte (tokenizer.HexValue, GetCurrentCodePage ());
		}

		void ProcessControlSymbol ()
		{
			char symbol = tokenizer.Symbol;

			// RTF 1.9.1, "Unicode RTF": a control symbol counts as a single fallback character.
			if (unicodeSkipRemaining > 0) {
				unicodeSkipRemaining--;
				return;
			}

			// RTF 1.9.1, "Destinations": "\*" marks the following destination as one that may be ignored if the
			// reader does not understand it.
			if (symbol == '*') {
				FlushBytes ();
				ignorableDestination = true;
				return;
			}

			ignorableDestination = false;

			switch (symbol) {
			case '\\': case '{': case '}':
				// RTF 1.9.1, "Control Symbols": escaped literal characters. They are treated as bytes rather than
				// characters because in DBCS code pages (e.g. Shift-JIS, \fcharset128) '\' (0x5C), '{' (0x7B) and
				// '}' (0x7D) are valid trail bytes, and writers escape them in the middle of a character.
				if (current.Destination != RtfDestination.Upr)
					AppendByte ((byte) symbol, GetCurrentCodePage ());
				return;
			}

			FlushBytes ();

			if (current.Destination == RtfDestination.Upr)
				return;

			// RTF 1.9.1, "Special Characters": \~ is a nonbreaking space, \_ a nonbreaking hyphen, \- an optional
			// hyphen (not rendered), and \<CR>/\<LF> (reported by the tokenizer as '\n') are equivalent to \par.
			switch (symbol) {
			case '~': EmitChar ('\u00A0'); break;
			case '_':
				// [MS-OXRTFEX] 2.1.3.1.4.2: inside an HTMLTAG destination the encapsulating writer uses \_ for "&shy;".
				EmitChar (current.Destination == RtfDestination.HtmlTag ? '\u00AD' : '\u2011');
				break;
			case '\n': EmitParagraph (); break;
			case '\t': EmitChar ('\t'); break;
			}
		}

		#endregion

		#region Font and Color Tables

		void BeginFont (int number)
		{
			CommitFont ();

			pendingFont = true;
			pendingFontDepth = depth;
			pendingFontNumber = number;
			pendingFontCharset = -1;
			pendingFontCodePage = 0;
		}

		void CommitFont ()
		{
			if (!pendingFont)
				return;

			pendingFont = false;

			// Once the font table is full, new font numbers are ignored (and text in those fonts falls back to the
			// document code page), but existing entries may still be updated since that does not grow the table.
			if (fonts.Count >= maxFontTableEntries && !fonts.ContainsKey (pendingFontNumber))
				return;

			// RTF 1.9.1, "Font Table": \cpgN, when present, takes precedence over the code page implied by \fcharsetN.
			int codepage = pendingFontCodePage > 0 ? pendingFontCodePage : GetCodePageFromCharset (pendingFontCharset);

			fonts[pendingFontNumber] = codepage;
		}

		void CommitColor ()
		{
			// RTF 1.9.1, "Color Table": an entry without any color components (typically the first one, written as
			// a bare ';') denotes the "auto" color, represented here as -1.
			int rgb = pendingColor ? (pendingRed << 16) | (pendingGreen << 8) | pendingBlue : -1;

			pendingColor = false;
			pendingRed = pendingGreen = pendingBlue = 0;

			// Entries beyond the limit are dropped; \cfN/\cbN references to them resolve to "auto".
			if (colors.Count < maxColorTableEntries)
				colors.Add (rgb);
		}

		// RTF 1.9.1, "Color Table": \redN, \greenN and \blueN range from 0 to 255.
		static int ClampColorComponent (int value)
		{
			return value < 0 ? 0 : value > 255 ? 255 : value;
		}

		void ProcessFontTableControlWord (RtfKeyword id, int parameter, bool ignorable)
		{
			switch (id) {
			case RtfKeyword.F:
				BeginFont (parameter);
				break;
			case RtfKeyword.Fcharset:
				pendingFontCharset = parameter;
				break;
			case RtfKeyword.Cpg:
				pendingFontCodePage = parameter;
				break;
			case RtfKeyword.SkipDestination:
				SkipCurrentGroup ();
				break;
			default:
				if (ignorable)
					SkipCurrentGroup ();
				break;
			}
		}

		void ProcessColorTableControlWord (RtfKeyword id, int parameter, bool ignorable)
		{
			switch (id) {
			case RtfKeyword.Red:
				pendingRed = ClampColorComponent (parameter);
				pendingColor = true;
				break;
			case RtfKeyword.Green:
				pendingGreen = ClampColorComponent (parameter);
				pendingColor = true;
				break;
			case RtfKeyword.Blue:
				pendingBlue = ClampColorComponent (parameter);
				pendingColor = true;
				break;
			default:
				if (ignorable)
					SkipCurrentGroup ();
				break;
			}
		}

		#endregion

		#region Fields

		// Only link schemes that are safe to place in an <a href> are allowed. In particular this rejects
		// javascript:, vbscript:, data: and file: URLs that a hostile document could otherwise inject into
		// the HTML output.
		static bool IsAllowedScheme (string url)
		{
			int colon = url.IndexOf (':');

			if (colon <= 0)
				return false;

			var scheme = url.Substring (0, colon);

			return scheme.Equals ("http", StringComparison.OrdinalIgnoreCase) ||
				scheme.Equals ("https", StringComparison.OrdinalIgnoreCase) ||
				scheme.Equals ("mailto", StringComparison.OrdinalIgnoreCase) ||
				scheme.Equals ("ftp", StringComparison.OrdinalIgnoreCase) ||
				scheme.Equals ("tel", StringComparison.OrdinalIgnoreCase);
		}

		// Parses a field instruction of the form: HYPERLINK [switches] "url" [switches]
		//
		// The HYPERLINK field syntax is defined by ECMA-376 Part 1, 17.16.5.25 (HYPERLINK), which RTF 1.9.1,
		// "Fields", defers to for field instructions. Switches are \l (bookmark within this document), \m, \n,
		// \o "tooltip" and \t "target frame".
		internal static string? ParseHyperlink (StringBuilder instruction)
		{
			const string Hyperlink = "HYPERLINK";
			var text = instruction.ToString ();
			int index = 0;

			while (index < text.Length && char.IsWhiteSpace (text[index]))
				index++;

			if (string.Compare (text, index, Hyperlink, 0, Hyperlink.Length, StringComparison.OrdinalIgnoreCase) != 0)
				return null;

			index += Hyperlink.Length;

			while (index < text.Length) {
				while (index < text.Length && char.IsWhiteSpace (text[index]))
					index++;

				if (index >= text.Length)
					break;

				if (text[index] == '\\') {
					// A switch. \l (local bookmark) means this is not an external link.
					if (index + 1 < text.Length && text[index + 1] == 'l')
						return null;

					// ECMA-376 Part 1, 17.16.5.25: only \o "tooltip" and \t "target" take an argument; \m and \n
					// do not, so the text that follows them may be the URL itself.
					bool hasArgument = index + 1 < text.Length && (text[index + 1] == 'o' || text[index + 1] == 't');

					index++;
					while (index < text.Length && !char.IsWhiteSpace (text[index]))
						index++;

					if (!hasArgument)
						continue;

					while (index < text.Length && char.IsWhiteSpace (text[index]))
						index++;

					if (index < text.Length && text[index] == '"') {
						int end = text.IndexOf ('"', index + 1);
						index = end == -1 ? text.Length : end + 1;
					} else {
						while (index < text.Length && !char.IsWhiteSpace (text[index]))
							index++;
					}
					continue;
				}

				string url;

				if (text[index] == '"') {
					int end = text.IndexOf ('"', index + 1);

					if (end == -1)
						end = text.Length;

					url = text.Substring (index + 1, end - (index + 1));
				} else {
					int start = index;

					while (index < text.Length && !char.IsWhiteSpace (text[index]))
						index++;

					url = text.Substring (start, index - start);
				}

				url = url.Trim ();

				// Reject URLs containing control characters, which browsers may strip (e.g. "java\tscript:")
				// in order to defeat the scheme check.
				for (int i = 0; i < url.Length; i++) {
					if (char.IsControl (url[i]))
						return null;
				}

				return IsAllowedScheme (url) ? url : null;
			}

			return null;
		}

		#endregion

		// RTF 1.9.1, "Font (Character) Formatting Properties": \plain resets the character formatting properties
		// to their defaults, which includes the font (reset to \deffN).
		void ResetCharacterFormatting ()
		{
			current.Flags &= ~RtfStateFlags.CharacterFormatting;
			current.VerticalAlignment = RtfVerticalAlignment.Baseline;
			current.FontSize = DefaultFontSize;
			current.ForegroundColor = 0;
			current.BackgroundColor = 0;
			current.Font = defaultFont;
		}

		void ProcessControlWord ()
		{
			bool ignorable = ignorableDestination;
			var id = tokenizer.KeywordId;
			int parameter = tokenizer.Parameter;
			bool hasParameter = tokenizer.HasParameter;

			// RTF 1.9.1, "Control Words": for toggle properties like \b, the control word alone turns the property
			// on and "\b0" turns it off.
			bool toggle = !hasParameter || parameter != 0;

			ignorableDestination = false;

			if (unicodeSkipRemaining > 0) {
				// RTF 1.9.1, "Unicode RTF": a control word counts as a single fallback character.
				unicodeSkipRemaining--;
				return;
			}

			// The font and color tables have their own small vocabularies; e.g. \f inside the font table starts a new
			// entry rather than changing the current font.
			switch (current.Destination) {
			case RtfDestination.FontTable:
				ProcessFontTableControlWord (id, parameter, ignorable);
				return;
			case RtfDestination.ColorTable:
				ProcessColorTableControlWord (id, parameter, ignorable);
				return;
			}

			switch (id) {
			// RTF 1.9.1, "Character Set" and "Code Page Support".
			case RtfKeyword.Ansi: documentCodePage = 1252; break;
			case RtfKeyword.Mac: documentCodePage = 10000; break;
			case RtfKeyword.Pc: documentCodePage = 437; break;
			case RtfKeyword.Pca: documentCodePage = 850; break;
			case RtfKeyword.Ansicpg:
				if (parameter > 0)
					documentCodePage = parameter;
				break;
			case RtfKeyword.Deff:
				// RTF 1.9.1, "Font Table": \deffN names the default font, which is in effect until a \fN.
				defaultFont = parameter;
				current.Font = parameter;
				break;
			case RtfKeyword.Fromhtml:
				// [MS-OXRTFEX] 2.1.3.1.2 / 2.2.3.1: \fromhtml1 among the first 10 tokens means the RTF encapsulates HTML.
				if (recognitionWindowOpen)
					FromHtml = toggle;
				break;
			case RtfKeyword.Htmlrtf:
				// [MS-OXRTFEX] 2.1.3.1.3: \htmlrtf (or \htmlrtf1) begins and \htmlrtf0 ends a block of RTF-only content.
				// Control words inside such a block are still processed (in particular \fN, which 2.2.3.2 says should be
				// tracked); only the content they produce is suppressed (see IsContentSuppressed).
				current.SetFlag (RtfStateFlags.HtmlRtf, toggle);
				break;
			case RtfKeyword.Htmltag:
				// [MS-OXRTFEX] 2.1.3.1.4: {\*\htmltagN ...} contains HTML from the original document. 2.2.3.2 says the
				// HTMLTagParameter (N) should be ignored and the CONTENT copied. When we are rendering the RTF itself,
				// the group is skipped like any other unknown \* destination.
				if (ExtractingHtml)
					current.Destination = RtfDestination.HtmlTag;
				else
					SkipCurrentGroup ();
				break;
			case RtfKeyword.Fonttbl:
				current.Destination = RtfDestination.FontTable;
				pendingFont = false;
				break;
			case RtfKeyword.Colortbl:
				current.Destination = RtfDestination.ColorTable;
				pendingColor = false;
				pendingRed = pendingGreen = pendingBlue = 0;
				break;
			case RtfKeyword.Field:
				// RTF 1.9.1, "Fields": {\field {\*\fldinst instruction} {\fldrslt result}}. Only HYPERLINK fields
				// are interpreted; for all other fields the result text is rendered as-is, as the spec recommends
				// for readers that do not support a field type.
				FieldHyperlink = null;
				break;
			case RtfKeyword.Fldinst:
				current.Destination = RtfDestination.FieldInstruction;
				fieldInstruction.Length = 0;
				break;
			case RtfKeyword.Fldrslt:
				current.Destination = RtfDestination.Normal;
				current.Hyperlink = FieldHyperlink;
				break;
			case RtfKeyword.Upr:
				// RTF 1.9.1, "Unicode RTF": {\upr {ansi version}{\*\ud {unicode version}}}. Ignore the ANSI version.
				current.Destination = RtfDestination.Upr;
				break;
			case RtfKeyword.Ud:
				if (current.Destination == RtfDestination.Upr)
					current.Destination = RtfDestination.Normal;
				break;
			case RtfKeyword.Nesttableprops:
				// Nothing to do: the group's table properties are not rendered, but its \nestrow must still be seen.
				// Some writers also put nested cell content in this group, which Word renders, so do not suppress it.
				break;
			case RtfKeyword.SkipDestination:
				SkipCurrentGroup ();
				break;
			case RtfKeyword.F:
				current.Font = parameter;
				break;
			case RtfKeyword.Plain:
				ResetCharacterFormatting ();
				break;
			// RTF 1.9.1, "Font (Character) Formatting Properties".
			case RtfKeyword.B: current.SetFlag (RtfStateFlags.Bold, toggle); break;
			case RtfKeyword.I: current.SetFlag (RtfStateFlags.Italic, toggle); break;
			case RtfKeyword.Ul: current.SetFlag (RtfStateFlags.Underline, toggle); break;
			case RtfKeyword.Ulnone: current.SetFlag (RtfStateFlags.Underline, false); break;
			case RtfKeyword.Strike: current.SetFlag (RtfStateFlags.Strike, toggle); break;
			case RtfKeyword.V: current.SetFlag (RtfStateFlags.Hidden, toggle); break;
			case RtfKeyword.Super: current.VerticalAlignment = RtfVerticalAlignment.Superscript; break;
			case RtfKeyword.Sub: current.VerticalAlignment = RtfVerticalAlignment.Subscript; break;
			case RtfKeyword.Nosupersub: current.VerticalAlignment = RtfVerticalAlignment.Baseline; break;
			case RtfKeyword.Fs:
				// RTF 1.9.1: \fsN is in half-points and "\fs" with no parameter means the default of 24 (12pt). The
				// value is clamped to Word's maximum of 1638pt so a hostile value cannot produce absurd CSS.
				current.FontSize = hasParameter ? Math.Max (0, Math.Min (parameter, 3276)) : DefaultFontSize;
				break;
			case RtfKeyword.Cf:
				current.ForegroundColor = Math.Max (0, parameter);
				break;
			case RtfKeyword.Cb:
				current.BackgroundColor = Math.Max (0, parameter);
				break;
			case RtfKeyword.U:
				// RTF 1.9.1, "Unicode RTF": \uN is a signed 16-bit value, so code points above 32767 are written as
				// negative numbers (e.g. \u-3913 is U+F0B7). Characters outside the BMP are written as two \uN
				// surrogates (see EmitUnicodeChar). After the character, the next \ucN "characters" are its ANSI
				// fallback and must be skipped.
				if (current.Destination != RtfDestination.Upr) {
					if (parameter < 0)
						parameter += 65536;

					EmitUnicodeChar (parameter >= 0 && parameter <= 0xFFFF ? (char) parameter : '\uFFFD');
				}
				unicodeSkipRemaining = current.UnicodeSkip;
				break;
			case RtfKeyword.Uc:
				current.UnicodeSkip = Math.Max (0, parameter);
				break;
			case RtfKeyword.Par:
				EmitParagraph ();
				break;
			case RtfKeyword.Line:
				EmitLineBreak ();
				break;
			case RtfKeyword.Tab:
				EmitChar ('\t');
				break;
			// RTF 1.9.1, "Paragraph Formatting Properties": \pard resets paragraph properties, including \intbl.
			case RtfKeyword.Pard:
				current.Alignment = RtfAlignment.Left;
				current.SetFlag (RtfStateFlags.InTable, false);
				break;
			case RtfKeyword.Ql: current.Alignment = RtfAlignment.Left; break;
			case RtfKeyword.Qc: current.Alignment = RtfAlignment.Center; break;
			case RtfKeyword.Qr: current.Alignment = RtfAlignment.Right; break;
			case RtfKeyword.Qj: current.Alignment = RtfAlignment.Justify; break;
			case RtfKeyword.Intbl:
				current.SetFlag (RtfStateFlags.InTable, true);
				break;
			// RTF 1.9.1, "Table Definitions": \cell ends a cell and \row ends a row (\nestcell/\nestrow for nested
			// tables, which are flattened).
			case RtfKeyword.Cell:
				if (current.Destination == RtfDestination.Normal && !IsContentSuppressed) {
					Begin ();
					handler.OnCell (this);
				}
				break;
			case RtfKeyword.Row:
				if (current.Destination == RtfDestination.Normal && !IsContentSuppressed) {
					Begin ();
					handler.OnRow (this);
				}
				break;
			// RTF 1.9.1, "Special Characters".
			case RtfKeyword.Emdash: EmitChar ('\u2014'); break;
			case RtfKeyword.Endash: EmitChar ('\u2013'); break;
			case RtfKeyword.Bullet: EmitChar ('\u2022'); break;
			case RtfKeyword.Lquote: EmitChar ('\u2018'); break;
			case RtfKeyword.Rquote: EmitChar ('\u2019'); break;
			case RtfKeyword.Ldblquote: EmitChar ('\u201C'); break;
			case RtfKeyword.Rdblquote: EmitChar ('\u201D'); break;
			case RtfKeyword.Emspace: EmitChar ('\u2003'); break;
			case RtfKeyword.Enspace: EmitChar ('\u2002'); break;
			case RtfKeyword.Qmspace: EmitChar ('\u2005'); break;
			case RtfKeyword.Zwj: EmitChar ('\u200D'); break;
			case RtfKeyword.Zwnj: EmitChar ('\u200C'); break;
			case RtfKeyword.Ltrmark: EmitChar ('\u200E'); break;
			case RtfKeyword.Rtlmark: EmitChar ('\u200F'); break;
			default:
				// RTF 1.9.1, "Destinations": if a reader does not recognize a control word preceded by \*, it should
				// skip the entire group. Unknown control words without \* are simply ignored.
				if (ignorable)
					SkipCurrentGroup ();
				break;
			}
		}
	}
}

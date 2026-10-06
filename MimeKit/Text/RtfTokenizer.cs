//
// RtfTokenizer.cs
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
using System.Threading;
using System.Threading.Tasks;

namespace MimeKit.Text {
	/// <summary>
	/// The kinds of tokens produced by the <see cref="RtfTokenizer"/>.
	/// </summary>
	enum RtfTokenKind
	{
		None,
		GroupStart,
		GroupEnd,
		ControlWord,
		ControlSymbol,
		HexChar,
		Text,
		EndOfFile
	}

	/// <summary>
	/// A streaming RTF tokenizer.
	/// </summary>
	/// <remarks>
	/// <para>The tokenizer is implemented as a resumable, character-at-a-time state machine that never performs
	/// I/O itself. The only difference between <see cref="ReadNextToken"/> and <see cref="ReadNextTokenAsync"/>
	/// is how the input buffer is refilled, which means that the sync and async APIs cannot drift apart.</para>
	/// <para>All state is bounded: control words are truncated to <see cref="MaxKeywordLength"/> characters,
	/// numeric parameters saturate after <see cref="MaxParameterDigits"/> digits, and <c>\binN</c> data is
	/// skipped in a streaming fashion without being buffered.</para>
	/// <para>The token data (<see cref="Keyword"/>, <see cref="TextBuffer"/>, etc) is only valid until the next
	/// call to <see cref="ReadNextToken"/> or <see cref="ReadNextTokenAsync"/>.</para>
	/// <para>The lexical rules implemented here come from the "Conventions of an RTF Reader", "Control Words",
	/// "Control Symbols" and "Groups" sections of the Microsoft Rich Text Format (RTF) Specification, Version 1.9.1
	/// (https://www.microsoft.com/en-us/download/details.aspx?id=10725), which is the last published version.</para>
	/// <para>RTF is a 7-bit format in which 8-bit values are expressed via <c>\'hh</c> escapes, but real-world
	/// writers often emit raw 8-bit bytes. The converters therefore read the input as Latin-1 by default, which maps
	/// every byte to exactly one <see cref="char"/>. This makes character offsets equal to byte offsets (important for
	/// <c>\binN</c>) and lets the interpreter decode raw bytes using the code page of the current font.</para>
	/// </remarks>
	sealed class RtfTokenizer
	{
		/// <summary>
		/// The maximum length of a control word.
		/// </summary>
		/// <remarks>
		/// RTF 1.9.1, "Control Words": "A control word cannot be longer than 32 letters." Longer control words cannot be
		/// valid, so the excess letters are consumed and discarded and the keyword is reported as unknown. This keeps the
		/// keyword buffer fixed-size regardless of input.
		/// </remarks>
		public const int MaxKeywordLength = 32;

		/// <summary>
		/// The maximum number of digits in a control word parameter.
		/// </summary>
		/// <remarks>
		/// RTF 1.9.1, "Control Words": the parameter is a signed 16-bit or 32-bit integer (e.g. <c>\binN</c> is 32-bit).
		/// A 32-bit integer never needs more than 10 digits, so any additional digits are consumed and ignored and the
		/// value saturates rather than overflowing.
		/// </remarks>
		public const int MaxParameterDigits = 10;

		enum ScanState
		{
			Default,
			Backslash,
			Keyword,
			ParameterSign,
			ParameterDigits,
			Hex1,
			Hex2
		}

		readonly char[] keyword = new char[MaxKeywordLength];
		readonly TextReader reader;
		readonly char[] input;
		long binaryRemaining;
		bool keywordTruncated;
		int parameterDigits;
		int keywordLength;
		bool negative;
		long parameter;
		ScanState state;
		int inputIndex;
		int textIndex;
		int textLength;
		int inputEnd;
		int hexValue;
		bool eof;

		/// <summary>
		/// Initialize a new instance of the <see cref="RtfTokenizer"/> class.
		/// </summary>
		/// <param name="reader">The text reader.</param>
		/// <param name="bufferSize">The size of the input buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="reader"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="bufferSize"/> is less than or equal to <c>0</c>.
		/// </exception>
		public RtfTokenizer (TextReader reader, int bufferSize = 4096)
		{
			if (reader is null)
				throw new ArgumentNullException (nameof (reader));

			if (bufferSize <= 0)
				throw new ArgumentOutOfRangeException (nameof (bufferSize));

			input = new char[bufferSize];
			this.reader = reader;
		}

		/// <summary>
		/// Get the kind of the current token.
		/// </summary>
		public RtfTokenKind Kind {
			get; private set;
		}

		/// <summary>
		/// Get the keyword identifier of the current <see cref="RtfTokenKind.ControlWord"/> token.
		/// </summary>
		public RtfKeyword KeywordId {
			get; private set;
		}

		/// <summary>
		/// Get the name of the current <see cref="RtfTokenKind.ControlWord"/> token.
		/// </summary>
		public ReadOnlySpan<char> Keyword {
			get { return new ReadOnlySpan<char> (keyword, 0, keywordLength); }
		}

		/// <summary>
		/// Get whether the current <see cref="RtfTokenKind.ControlWord"/> token was longer than <see cref="MaxKeywordLength"/>.
		/// </summary>
		public bool IsKeywordTruncated {
			get { return keywordTruncated; }
		}

		/// <summary>
		/// Get whether the current <see cref="RtfTokenKind.ControlWord"/> token has a numeric parameter.
		/// </summary>
		public bool HasParameter {
			get; private set;
		}

		/// <summary>
		/// Get the numeric parameter of the current <see cref="RtfTokenKind.ControlWord"/> token.
		/// </summary>
		public int Parameter {
			get; private set;
		}

		/// <summary>
		/// Get the symbol of the current <see cref="RtfTokenKind.ControlSymbol"/> token.
		/// </summary>
		/// <remarks>
		/// A backslash followed by a CR or LF is reported as <c>'\n'</c>.
		/// </remarks>
		public char Symbol {
			get; private set;
		}

		/// <summary>
		/// Get the byte value of the current <see cref="RtfTokenKind.HexChar"/> token.
		/// </summary>
		public byte HexValue {
			get; private set;
		}

		/// <summary>
		/// Get the buffer containing the current <see cref="RtfTokenKind.Text"/> token.
		/// </summary>
		public char[] TextBuffer {
			get { return input; }
		}

		/// <summary>
		/// Get the index into <see cref="TextBuffer"/> of the current <see cref="RtfTokenKind.Text"/> token.
		/// </summary>
		public int TextIndex {
			get { return textIndex; }
		}

		/// <summary>
		/// Get the length of the current <see cref="RtfTokenKind.Text"/> token.
		/// </summary>
		public int TextLength {
			get { return textLength; }
		}

		/// <summary>
		/// Get the current <see cref="RtfTokenKind.Text"/> token.
		/// </summary>
		public ReadOnlySpan<char> Text {
			get { return new ReadOnlySpan<char> (input, textIndex, textLength); }
		}

		// RTF 1.9.1, "Control Words": a control word consists of lowercase ASCII letters. Uppercase letters are also
		// accepted because a few real control words contain them (e.g. \mmathPr from the Office Math extensions).
		static bool IsLetter (char c)
		{
			return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
		}

		static bool IsDigit (char c)
		{
			return c >= '0' && c <= '9';
		}

		static int HexDigitValue (char c)
		{
			if (c >= '0' && c <= '9')
				return c - '0';

			if (c >= 'a' && c <= 'f')
				return c - 'a' + 10;

			if (c >= 'A' && c <= 'F')
				return c - 'A' + 10;

			return -1;
		}

		// The characters that terminate a run of plain text. RTF 1.9.1, "Conventions of an RTF Reader": the
		// reader acts on '{', '}' and '\', and ignores raw CR and LF.
		static bool IsSpecial (char c)
		{
			return c == '{' || c == '}' || c == '\\' || c == '\r' || c == '\n';
		}

		void BeginKeyword (char c)
		{
			keyword[0] = c;
			keywordLength = 1;
			keywordTruncated = false;
			parameterDigits = 0;
			negative = false;
			parameter = 0;
		}

		bool CompleteKeyword ()
		{
			state = ScanState.Default;

			Kind = RtfTokenKind.ControlWord;
			HasParameter = parameterDigits > 0;

			if (negative)
				Parameter = parameter > -(long) int.MinValue ? int.MinValue : (int) -parameter;
			else
				Parameter = parameter > int.MaxValue ? int.MaxValue : (int) parameter;

			KeywordId = keywordTruncated ? RtfKeyword.Unknown : RtfKeywords.Lookup (Keyword);

			// RTF 1.9.1, "Pictures" (\binN): "the N bytes of data following the space [after \binN] are binary data".
			// The data is not RTF and may contain '{', '}' or '\', so it must be skipped here, at the lexical level,
			// rather than by the interpreter. It is skipped in a streaming fashion (see Scan) so that a huge N does
			// not cause us to buffer anything.
			if (KeywordId == RtfKeyword.Bin && Parameter > 0)
				binaryRemaining = Parameter;

			return true;
		}

		bool CompleteHex ()
		{
			state = ScanState.Default;
			Kind = RtfTokenKind.HexChar;
			HexValue = (byte) hexValue;
			return true;
		}

		bool CompleteEndOfFile ()
		{
			// A control word or hex escape can legitimately be terminated by the end of the input, so emit
			// whatever is pending before reporting the end of the file.
			switch (state) {
			case ScanState.Keyword:
			case ScanState.ParameterSign:
			case ScanState.ParameterDigits:
				return CompleteKeyword ();
			case ScanState.Hex2:
				return CompleteHex ();
			}

			state = ScanState.Default;
			Kind = RtfTokenKind.EndOfFile;

			return true;
		}

		// Returns true if a token was produced or false if more input is needed.
		//
		// All scanning state lives in fields, so a token may span any number of buffer refills. This is what allows
		// ReadNextToken and ReadNextTokenAsync to share this one implementation; they differ only in how they refill.
		bool Scan ()
		{
			while (true) {
				if (binaryRemaining > 0) {
					int n = (int) Math.Min (binaryRemaining, inputEnd - inputIndex);

					binaryRemaining -= n;
					inputIndex += n;

					if (binaryRemaining > 0) {
						if (eof) {
							binaryRemaining = 0;
							continue;
						}

						return false;
					}
				}

				if (inputIndex >= inputEnd) {
					if (!eof)
						return false;

					return CompleteEndOfFile ();
				}

				char c = input[inputIndex];
				int value;

				switch (state) {
				case ScanState.Default:
					switch (c) {
					case '{':
						inputIndex++;
						Kind = RtfTokenKind.GroupStart;
						return true;
					case '}':
						inputIndex++;
						Kind = RtfTokenKind.GroupEnd;
						return true;
					case '\\':
						inputIndex++;
						state = ScanState.Backslash;
						break;
					case '\r': case '\n':
						// RTF 1.9.1, "Conventions of an RTF Reader": "The RTF reader should ignore ... carriage returns
						// and line feeds" unless they are escaped (see the Backslash state below).
						inputIndex++;
						break;
					default:
						textIndex = inputIndex++;

						while (inputIndex < inputEnd && !IsSpecial (input[inputIndex]))
							inputIndex++;

						textLength = inputIndex - textIndex;
						Kind = RtfTokenKind.Text;
						return true;
					}
					break;
				case ScanState.Backslash:
					inputIndex++;

					if (IsLetter (c)) {
						BeginKeyword (c);
						state = ScanState.Keyword;
					} else if (c == '\'') {
						// RTF 1.9.1, "Special Characters": \'hh is "a hexadecimal value, based on the specified
						// character set (may be used to identify 8-bit values)".
						state = ScanState.Hex1;
						hexValue = 0;
					} else {
						// RTF 1.9.1, "Control Symbols": a backslash followed by a single non-alphabetic character.
						// "\<CR>" and "\<LF>" are both equivalent to \par, so they are normalized to '\n'.
						state = ScanState.Default;
						Kind = RtfTokenKind.ControlSymbol;
						Symbol = c == '\r' ? '\n' : c;
						return true;
					}
					break;
				case ScanState.Keyword:
					if (IsLetter (c)) {
						if (keywordLength < MaxKeywordLength)
							keyword[keywordLength++] = c;
						else
							keywordTruncated = true;
						inputIndex++;
					} else if (c == '-') {
						// RTF 1.9.1, "Control Words": "a hyphen (-) ... indicates that a numeric parameter follows."
						state = ScanState.ParameterSign;
						negative = true;
						inputIndex++;
					} else if (IsDigit (c)) {
						state = ScanState.ParameterDigits;
					} else {
						// RTF 1.9.1, "Control Words": "A space. ... the space is part of the control word" and is
						// not content. Any other delimiter "terminates the control word but is not actually part of
						// the control word", so it is left in the buffer to be scanned as the start of the next token.
						if (c == ' ')
							inputIndex++;

						return CompleteKeyword ();
					}
					break;
				case ScanState.ParameterSign:
					if (IsDigit (c)) {
						state = ScanState.ParameterDigits;
					} else {
						// A '-' with no digits is malformed; treat it as a control word without a parameter.
						negative = false;
						return CompleteKeyword ();
					}
					break;
				case ScanState.ParameterDigits:
					if (IsDigit (c)) {
						// Note: saturate rather than overflow; excess digits are consumed and ignored.
						if (parameterDigits < MaxParameterDigits) {
							parameter = (parameter * 10) + (c - '0');
							parameterDigits++;
						}
						inputIndex++;
					} else {
						// See the comment in the Keyword state regarding the delimiting space.
						if (c == ' ')
							inputIndex++;

						return CompleteKeyword ();
					}
					break;
				case ScanState.Hex1:
					if ((value = HexDigitValue (c)) != -1) {
						state = ScanState.Hex2;
						hexValue = value;
						inputIndex++;
					} else {
						// Malformed \' escape with no hex digits: drop it, and leave the current character to be
						// scanned normally.
						state = ScanState.Default;
					}
					break;
				case ScanState.Hex2:
					// Malformed \'h escape with a single hex digit: be lenient and use that digit's value. The
					// non-hex character is left to be scanned normally.
					if ((value = HexDigitValue (c)) != -1) {
						hexValue = (hexValue << 4) | value;
						inputIndex++;
					}

					return CompleteHex ();
				}
			}
		}

		void Fill ()
		{
			int nread = reader.Read (input, 0, input.Length);

			inputIndex = 0;
			inputEnd = nread;
			eof = nread <= 0;
		}

		async Task FillAsync (CancellationToken cancellationToken)
		{
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			int nread = await reader.ReadAsync (input.AsMemory (), cancellationToken).ConfigureAwait (false);
#else
			// TextReader.ReadAsync (char[], int, int) does not take a CancellationToken on older frameworks.
			cancellationToken.ThrowIfCancellationRequested ();

			int nread = await reader.ReadAsync (input, 0, input.Length).ConfigureAwait (false);
#endif

			inputIndex = 0;
			inputEnd = nread;
			eof = nread <= 0;
		}

		/// <summary>
		/// Read the next token.
		/// </summary>
		/// <returns><see langword="true" /> if a token was read; otherwise, <see langword="false" /> if the end of the input was reached.</returns>
		public bool ReadNextToken ()
		{
			while (!Scan ())
				Fill ();

			return Kind != RtfTokenKind.EndOfFile;
		}

		/// <summary>
		/// Asynchronously read the next token.
		/// </summary>
		/// <returns><see langword="true" /> if a token was read; otherwise, <see langword="false" /> if the end of the input was reached.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		public async Task<bool> ReadNextTokenAsync (CancellationToken cancellationToken = default)
		{
			while (!Scan ())
				await FillAsync (cancellationToken).ConfigureAwait (false);

			return Kind != RtfTokenKind.EndOfFile;
		}
	}
}

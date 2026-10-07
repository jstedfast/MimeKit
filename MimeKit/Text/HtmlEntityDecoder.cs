//
// HtmlEntityDecoder.cs
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
	/// An HTML entity decoder.
	/// </summary>
	/// <remarks>
	/// An HTML entity decoder.
	/// </remarks>
	public partial class HtmlEntityDecoder
	{
		const int MaxCodePoint = 0x10FFFF;

		readonly char[] pushed;
		readonly int[] states;
		int numericValue;
		bool semicolon;
		bool numeric;
		bool digits;
		byte xbase;
		int index;

		/// <summary>
		/// Initialize a new instance of the <see cref="HtmlEntityDecoder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="HtmlEntityDecoder"/>.
		/// </remarks>
		public HtmlEntityDecoder ()
		{
			pushed = new char[MaxEntityLength];
			states = new int[MaxEntityLength];
		}

		bool PushNumericEntity (char c)
		{
			int v;

			if (xbase == 0) {
				if (c == 'X' || c == 'x') {
					pushed[index++] = c;
					xbase = 16;
					return true;
				}

				xbase = 10;
			}

			if (c == ';') {
				if (!digits)
					return false;

				semicolon = true;
				AppendNumeric (c);
				return true;
			}

			if (c <= '9') {
				if (c < '0')
					return false;

				v = c - '0';
			} else if (xbase == 16) {
				if (c >= 'a' && c <= 'f') {
					v = (c - 'a') + 10;
				} else if (c >= 'A' && c <= 'F') {
					v = (c - 'A') + 10;
				} else {
					return false;
				}
			} else {
				return false;
			}

			// Per the HTML specification, a numeric character reference consumes every digit no matter how many
			// there are. Saturate just above the maximum code point so that the value cannot overflow and is
			// still recognized as out-of-range when it is decoded.
			if (numericValue <= MaxCodePoint)
				numericValue = Math.Min ((numericValue * xbase) + v, MaxCodePoint + 1);

			AppendNumeric (c);
			digits = true;

			return true;
		}

		void AppendNumeric (char c)
		{
			// Only the leading characters of an arbitrarily long run of digits are retained. The raw text is
			// only ever needed when no digits were consumed (e.g. "&#x"), in which case it is always short.
			if (index < MaxEntityLength)
				pushed[index++] = c;
		}

		/// <summary>
		/// Push the specified character into the HTML entity decoder.
		/// </summary>
		/// <remarks>
		/// <para>Pushes the specified character into the HTML entity decoder.</para>
		/// <para>The first character pushed MUST be the '&amp;' character.</para>
		/// </remarks>
		/// <returns><see langword="true" /> if the character was accepted; otherwise, <see langword="false" />.</returns>
		/// <param name="c">The character.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="c"/> is the first character being pushed and was not the '&amp;' character.
		/// </exception>
		public bool Push (char c)
		{
			if (semicolon)
				return false;

			if (index == 0) {
				if (c != '&')
					throw new ArgumentOutOfRangeException (nameof (c), "The first character that is pushed MUST be the '&' character.");

				pushed[index] = '&';
				states[index] = 0;
				index++;
				return true;
			}

			if (numeric)
				return PushNumericEntity (c);

			if (index + 1 > MaxEntityLength)
				return false;

			if (index == 1 && c == '#') {
				pushed[index] = '#';
				states[index] = 0;
				numeric = true;
				index++;
				return true;
			}

			if (!PushNamedEntity (c))
				return false;

			semicolon = c == ';';

			return true;
		}

		// 13.2.5.80 Numeric character reference end state
		string GetNumericEntityValue ()
		{
			if (!digits)
				return new string (pushed, 0, index);

			int state = numericValue;

			switch (state) {
			case 0x00: return "\uFFFD"; // REPLACEMENT CHARACTER
			case 0x80: return "\u20AC"; // EURO SIGN (€)
			case 0x82: return "\u201A"; // SINGLE LOW-9 QUOTATION MARK (‚)
			case 0x83: return "\u0192"; // LATIN SMALL LETTER F WITH HOOK (ƒ)
			case 0x84: return "\u201E"; // DOUBLE LOW-9 QUOTATION MARK („)
			case 0x85: return "\u2026"; // HORIZONTAL ELLIPSIS (…)
			case 0x86: return "\u2020"; // DAGGER (†)
			case 0x87: return "\u2021"; // DOUBLE DAGGER (‡)
			case 0x88: return "\u02C6"; // MODIFIER LETTER CIRCUMFLEX ACCENT (ˆ)
			case 0x89: return "\u2030"; // PER MILLE SIGN (‰)
			case 0x8A: return "\u0160"; // LATIN CAPITAL LETTER S WITH CARON (Š)
			case 0x8B: return "\u2039"; // SINGLE LEFT-POINTING ANGLE QUOTATION MARK (‹)
			case 0x8C: return "\u0152"; // LATIN CAPITAL LIGATURE OE (Œ)
			case 0x8E: return "\u017D"; // LATIN CAPITAL LETTER Z WITH CARON (Ž)
			case 0x91: return "\u2018"; // LEFT SINGLE QUOTATION MARK (‘)
			case 0x92: return "\u2019"; // RIGHT SINGLE QUOTATION MARK (’)
			case 0x93: return "\u201C"; // LEFT DOUBLE QUOTATION MARK (“)
			case 0x94: return "\u201D"; // RIGHT DOUBLE QUOTATION MARK (”)
			case 0x95: return "\u2022"; // BULLET (•)
			case 0x96: return "\u2013"; // EN DASH (–)
			case 0x97: return "\u2014"; // EM DASH (—)
			case 0x98: return "\u02DC"; // SMALL TILDE (˜)
			case 0x99: return "\u2122"; // TRADEMARK SIGN (™)
			case 0x9A: return "\u0161"; // LATIN SMALL LETTER S WITH CARON (š)
			case 0x9B: return "\u203A"; // SINGLE RIGHT-POINTING ANGLE QUOTATION MARK (›)
			case 0x9C: return "\u0153"; // LATIN SMALL LIGATURE OE (œ)
			case 0x9E: return "\u017E"; // LATIN SMALL LETTER Z WITH CARON (ž)
			case 0x9F: return "\u0178"; // LATIN CAPITAL LETTER Y WITH DIAERESIS (Ÿ)
			default:
				// Surrogates and values beyond the Unicode range are replaced. Noncharacters and the remaining
				// control characters are parse errors, but are still emitted as-is.
				if ((state >= 0xD800 && state <= 0xDFFF) || state > MaxCodePoint)
					return "\uFFFD";
				break;
			}

			return char.ConvertFromUtf32 (state);
		}

		// 13.2.5.73 Named character reference state: if the character reference was consumed as part of an
		// attribute, and the last character matched is not ';', and the next input character is either '=' or
		// an ASCII alphanumeric, then the matched characters are flushed as-is (for historical reasons).
		internal string GetAttributeValue (char next)
		{
			if (numeric)
				return GetNumericEntityValue ();

			int matched = index;

			while (matched > 1 && !NamedEntities.ContainsKey (states[matched - 1]))
				matched--;

			if (matched > 1 && pushed[matched - 1] != ';') {
				char c = matched < index ? pushed[matched] : next;

				if (c == '=' || (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
					return new string (pushed, 0, index);
			}

			return GetNamedEntityValue ();
		}

		/// <summary>
		/// Get the decoded entity value.
		/// </summary>
		/// <remarks>
		/// Gets the decoded entity value.
		/// </remarks>
		/// <returns>The value.</returns>
		public string GetValue ()
		{
			return numeric ? GetNumericEntityValue () : GetNamedEntityValue ();
		}

		/// <summary>
		/// Reset the entity decoder.
		/// </summary>
		/// <remarks>
		/// Resets the entity decoder.
		/// </remarks>
		public void Reset ()
		{
			numericValue = 0;
			semicolon = false;
			numeric = false;
			digits = false;
			xbase = 0;
			index = 0;
		}
	}
}

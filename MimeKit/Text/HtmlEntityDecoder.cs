//
// HtmlEntityDecoder.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2024 .NET Foundation and Contributors
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
		readonly char[] pushed;
		readonly int[] states;
		bool semicolon;
		bool numeric;
		byte digits;
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
					states[index] = 0;
					pushed[index] = c;
					xbase = 16;
					index++;
					return true;
				}

				xbase = 10;
			}

			if (c <= '9') {
				if (c < '0')
					return false;

				v = c - '0';
			} else if (xbase == 16) {
				if (c >= 'a') {
					v = (c - 'a') + 10;
				} else if (c >= 'A') {
					v = (c - 'A') + 10;
				} else {
					return false;
				}
			} else {
				return false;
			}

			if (v >= (int) xbase)
				return false;

			int state = states[index - 1];

			// check for overflow
			if (state > int.MaxValue / xbase)
				return false;

			if (state == int.MaxValue / xbase && v > int.MaxValue % xbase)
				return false;

			state = (state * xbase) + v;
			states[index] = state;
			pushed[index] = c;
			digits++;
			index++;

			return true;
		}

		/// <summary>
		/// Push the specified character into the HTML entity decoder.
		/// </summary>
		/// <remarks>
		/// <para>Pushes the specified character into the HTML entity decoder.</para>
		/// <para>The first character pushed MUST be the '&amp;' character.</para>
		/// </remarks>
		/// <returns><c>true</c> if the character was accepted; otherwise, <c>false</c>.</returns>
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

			if (index + 1 > MaxEntityLength)
				return false;

			if (index == 1 && c == '#') {
				pushed[index] = '#';
				states[index] = 0;
				numeric = true;
				index++;
				return true;
			}

			semicolon = c == ';';

			if (numeric) {
				if (c == ';') {
					states[index] = states[index - 1];
					pushed[index] = ';';
					index++;
					return true;
				}

				return PushNumericEntity (c);
			}

			return PushNamedEntity (c);
		}

		string GetNumericEntityValue ()
		{
			if (digits == 0 || !semicolon)
				return new string (pushed, 0, index);

			int state = states[index - 1];

			// the following states are parse errors
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
			case 0x99: return "\u2122"; // TRADE MARK SIGN (™)
			case 0x9A: return "\u0161"; // LATIN SMALL LETTER S WITH CARON (š)
			case 0x9B: return "\u203A"; // SINGLE RIGHT-POINTING ANGLE QUOTATION MARK (›)
			case 0x9C: return "\u0153"; // LATIN SMALL LIGATURE OE (œ)
			case 0x9E: return "\u017E"; // LATIN SMALL LETTER Z WITH CARON (ž)
			case 0x9F: return "\u0178"; // LATIN CAPITAL LETTER Y WITH DIAERESIS (Ÿ)
			case 0x0000B: case 0x0FFFE: case 0x1FFFE: case 0x1FFFF: case 0x2FFFE: case 0x2FFFF: case 0x3FFFE:
			case 0x3FFFF: case 0x4FFFE: case 0x4FFFF: case 0x5FFFE: case 0x5FFFF: case 0x6FFFE: case 0x6FFFF:
			case 0x7FFFE: case 0x7FFFF: case 0x8FFFE: case 0x8FFFF: case 0x9FFFE: case 0x9FFFF: case 0xAFFFE:
			case 0xAFFFF: case 0xBFFFE: case 0xBFFFF: case 0xCFFFE: case 0xCFFFF: case 0xDFFFE: case 0xDFFFF:
			case 0xEFFFE: case 0xEFFFF: case 0xFFFFE: case 0xFFFFF: case 0x10FFFE: case 0x10FFFF:
				// parse error
				return new string (pushed, 0, index);
			default:
				if ((state >= 0xD800 && state <= 0xDFFF) || state > 0x10FFFF) {
					// parse error, emit REPLACEMENT CHARACTER
					return "\uFFFD";
				}

				if ((state >= 0x0001 && state <= 0x0008) || (state >= 0x000D && state <= 0x001F) ||
					(state >= 0x007F && state <= 0x009F) || (state >= 0xFDD0 && state <= 0xFDEF)) {
					return new string (pushed, 0, index);
				}
				break;
			}

			return char.ConvertFromUtf32 (state);
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

		internal string GetPushedInput ()
		{
			return new string (pushed, 0, index);
		}

		/// <summary>
		/// Reset the entity decoder.
		/// </summary>
		/// <remarks>
		/// Resets the entity decoder.
		/// </remarks>
		public void Reset ()
		{
			semicolon = false;
			numeric = false;
			digits = 0;
			xbase = 0;
			index = 0;
		}
	}
}

//
// DateParser.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Jeffrey Stedfast
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
using System.Text;
using System.Collections.Generic;

namespace MimeKit.Utils {
	[Flags]
	enum DateTokenFlags : byte
	{
		None           = 0,
		NonNumeric     = (1 << 0),
		NonWeekday     = (1 << 1),
		NonMonth       = (1 << 2),
		NonTime        = (1 << 3),
		NonAlphaZone   = (1 << 4),
		NonNumericZone = (1 << 5),
		HasColon       = (1 << 6),
		HasSign        = (1 << 7),
	}

	class DateToken
	{
		public DateTokenFlags Flags { get; private set; }
		public int StartIndex { get; private set; }
		public int Length { get; private set; }

		public bool IsNumeric {
			get { return !Flags.HasFlag (DateTokenFlags.NonNumeric); }
		}

		public bool IsWeekday {
			get { return !Flags.HasFlag (DateTokenFlags.NonWeekday); }
		}

		public bool IsMonth {
			get { return !Flags.HasFlag (DateTokenFlags.NonMonth); }
		}

		public bool IsTimeOfDay {
			get { return !Flags.HasFlag (DateTokenFlags.NonTime) && Flags.HasFlag (DateTokenFlags.HasColon); }
		}

		public bool IsNumericZone {
			get { return !Flags.HasFlag (DateTokenFlags.NonNumericZone) && Flags.HasFlag (DateTokenFlags.HasSign); }
		}

		public bool IsAlphaZone {
			get { return !Flags.HasFlag (DateTokenFlags.NonAlphaZone); }
		}

		public bool IsTimeZone {
			get { return IsNumericZone || IsAlphaZone; }
		}

		public DateToken (DateTokenFlags flags, int startIndex, int length)
		{
			StartIndex = startIndex;
			Length = length;
			Flags = flags;
		}
	}

	public static class DateUtils
	{
		const string MonthCharacters = "JanuaryFebruaryMarchAprilMayJuneJulyAugustSeptemberOctoberNovemberDecember";
		const string WeekdayCharacters = "SundayMondayTuesdayWednesdayThursdayFridaySaturday";
		const string AlphaZoneCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		const string NumericZoneCharacters = "+-0123456789";
		const string NumericCharacters = "0123456789";
		const string TimeCharacters = "0123456789:";

		static readonly string[] Months = new string[] {
			"Jan", "Feb", "Mar", "Apr", "May", "Jun",
			"Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
		};

		static readonly string[] WeekDays = new string[] {
			"Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"
		};

		static readonly StringComparer icase = StringComparer.OrdinalIgnoreCase;
		static readonly Dictionary<string, int> timezones;
		static readonly DateTokenFlags[] datetok;

		static DateUtils ()
		{
			timezones = new Dictionary<string, int> () {
				{ "UT",       0 }, { "UTC",      0 }, { "GMT",      0 },
				{ "EDT",   -400 }, { "EST",   -500 },
				{ "CDT",   -500 }, { "CST",   -600 },
				{ "MDT",   -600 }, { "MST",   -700 },
				{ "PDT",   -700 }, { "PST",   -800 },
				// Note: rfc822 got the signs backwards for the military
				// timezones so some sending clients may mistakenly use the
				// wrong values.
				{ "A",      100 }, { "B",      200 }, { "C",      300 },
				{ "D",      400 }, { "E",      500 }, { "F",      600 },
				{ "G",      700 }, { "H",      800 }, { "I",      900 },
				{ "K",     1000 }, { "L",     1100 }, { "M",     1200 },
				{ "N",     -100 }, { "O",     -200 }, { "P",     -300 },
				{ "Q",     -400 }, { "R",     -500 }, { "S",     -600 },
				{ "T",     -700 }, { "U",     -800 }, { "V",     -900 },
				{ "W",    -1000 }, { "X",    -1100 }, { "Y",    -1200 },
				{ "Z",        0 },
			};

			datetok = new DateTokenFlags[256];
			for (int c = 0; c < 256; c++) {
				if (NumericZoneCharacters.IndexOf ((char) c) == -1)
					datetok[c] |= DateTokenFlags.NonNumericZone;
				if (AlphaZoneCharacters.IndexOf ((char) c) == -1)
					datetok[c] |= DateTokenFlags.NonAlphaZone;
				if (WeekdayCharacters.IndexOf ((char) c) == -1)
					datetok[c] |= DateTokenFlags.NonWeekday;
				if (NumericCharacters.IndexOf ((char) c) == -1)
					datetok[c] |= DateTokenFlags.NonNumeric;
				if (MonthCharacters.IndexOf ((char) c) == -1)
					datetok[c] |= DateTokenFlags.NonMonth;
				if (TimeCharacters.IndexOf ((char) c) == -1)
					datetok[c] |= DateTokenFlags.NonTime;
			}

			datetok[':'] |= DateTokenFlags.HasColon;
			datetok['+'] |= DateTokenFlags.HasSign;
			datetok['-'] |= DateTokenFlags.HasSign;
		}

		static bool TryGetWeekday (DateToken token, byte[] text, out DayOfWeek weekday)
		{
			weekday = DayOfWeek.Sunday;

			if (!token.IsWeekday || token.Length < 3)
				return false;

			var name = Encoding.ASCII.GetString (text, token.StartIndex, token.Length);

			if (name.Length > 3)
				name = name.Substring (0, 3);

			for (int day = 0; day < WeekDays.Length; day++) {
				if (icase.Compare (WeekDays[day], name) == 0) {
					weekday = (DayOfWeek) day;
					return true;
				}
			}

			return false;
		}

		static bool TryGetDayOfMonth (DateToken token, byte[] text, out int day)
		{
			int endIndex = token.StartIndex + token.Length;
			int index = token.StartIndex;

			day = 0;

			if (!token.IsNumeric)
				return false;

			if (!ParseUtils.TryParseInt32 (text, ref index, endIndex, out day))
				return false;

			if (day <= 0 || day > 31)
				return false;

			return true;
		}

		static bool TryGetMonth (DateToken token, byte[] text, out int month)
		{
			month = 0;

			if (!token.IsMonth || token.Length < 3)
				return false;

			var name = Encoding.ASCII.GetString (text, token.StartIndex, token.Length);

			if (name.Length > 3)
				name = name.Substring (0, 3);

			for (int i = 0; i < Months.Length; i++) {
				if (icase.Compare (Months[i], name) == 0) {
					month = i + 1;
					return true;
				}
			}

			return false;
		}

		static bool TryGetYear (DateToken token, byte[] text, out int year)
		{
			int endIndex = token.StartIndex + token.Length;
			int index = token.StartIndex;

			year = 0;

			if (!token.IsNumeric)
				return false;

			if (!ParseUtils.TryParseInt32 (text, ref index, endIndex, out year))
				return false;

			if (year < 100)
				year += (year < 70) ? 2000 : 1900;

			if (year < 1969)
				return false;

			return true;
		}

		static bool TryGetTimeOfDay (DateToken token, byte[] text, out int hour, out int minute, out int second)
		{
			int endIndex = token.StartIndex + token.Length;
			int index = token.StartIndex;

			hour = minute = second = 0;

			if (!token.IsTimeOfDay)
				return false;

			if (!ParseUtils.TryParseInt32 (text, ref index, endIndex, out hour) || hour > 23)
				return false;

			if (index >= endIndex || text[index++] != (byte) ':')
				return false;

			if (!ParseUtils.TryParseInt32 (text, ref index, endIndex, out minute) || minute > 59)
				return false;

			// Allow just hh:mm (i.e. w/o the :ss?)
			if (index >= endIndex || text[index++] != (byte) ':')
				return true;

			if (!ParseUtils.TryParseInt32 (text, ref index, endIndex, out second) || second > 59)
				return false;

			if (index < endIndex)
				return false;

			return true;
		}

		static bool TryGetTimeZone (DateToken token, byte[] text, out int tzone)
		{
			tzone = 0;

			if (token.IsNumericZone) {
				if (!token.Flags.HasFlag (DateTokenFlags.HasSign))
					return false;

				int endIndex = token.StartIndex + token.Length;
				int index = token.StartIndex;
				int sign;

				if (text[index] == (byte) '-')
					sign = -1;
				else if (text[index] == (byte) '+')
					sign = 1;
				else
					return false;

				index++;

				if (!ParseUtils.TryParseInt32 (text, ref index, endIndex, out tzone) || index != endIndex)
					return false;

				tzone *= sign;
			} else if (token.IsAlphaZone) {
				if (token.Length > 3)
					return false;

				var name = Encoding.ASCII.GetString (text, token.StartIndex, token.Length);

				if (!timezones.TryGetValue (name, out tzone))
					return false;
			} else {
				return false;
			}

			return true;
		}

		static bool IsTokenDelimeter (byte c)
		{
			return c == (byte) '-' || c == (byte) '/' || c == (byte) ',' || c.IsWhitespace ();
		}

		static IEnumerable<DateToken> TokenizeDate (byte[] text, int startIndex, int length)
		{
			int endIndex = startIndex + length;
			int index = startIndex;
			DateTokenFlags mask;
			int start;

			while (index < endIndex) {
				ParseUtils.SkipWhiteSpace (text, ref index, endIndex);
				if (index >= endIndex)
					break;

				// get the initial mask for this token
				if ((mask = datetok[text[index]]) != DateTokenFlags.None) {
					start = index++;

					// find the end of this token
					while (index < endIndex && !IsTokenDelimeter (text[index]))
						mask |= datetok[text[index++]];

					yield return new DateToken (mask, start, index - start);
				}

				// skip over the token delimeter
				index++;
			}

			yield break;
		}

		static bool TryParseStandardDateFormat (IList<DateToken> tokens, byte[] text, out DateTimeOffset date)
		{
			int day, month, year, tzone;
			int hour, minute, second;
			#pragma warning disable 0219
			bool haveWeekday = false;
			#pragma warning restore 0219
			DayOfWeek weekday;
			int n = 0;

			date = new DateTimeOffset ();

			// we need at least 5 tokens, 6 if we have a weekday
			if (tokens.Count < 5)
				return false;

			// Note: the weekday is not required
			if (TryGetWeekday (tokens[n], text, out weekday)) {
				if (tokens.Count < 6)
					return false;

				haveWeekday = true;
				n++;
			}

			if (!TryGetDayOfMonth (tokens[n++], text, out day))
				return false;

			if (!TryGetMonth (tokens[n++], text, out month))
				return false;

			if (!TryGetYear (tokens[n++], text, out year))
				return false;

			if (!TryGetTimeOfDay (tokens[n++], text, out hour, out minute, out second))
				return false;

			if (!TryGetTimeZone (tokens[n], text, out tzone))
				tzone = 0;

			int minutes = tzone % 100;
			int hours = tzone / 100;

			TimeSpan offset = new TimeSpan (hours, minutes, 0);

			date = new DateTimeOffset (year, month, day, hour, minute, second, offset);

			return true;
		}

		static bool TryParseUnknownDateFormat (IList<DateToken> tokens, byte[] text, out DateTimeOffset date)
		{
			int? day = null, month = null, year = null, tzone = null;
			int hour = 0, minute = 0, second = 0;
			bool haveWeekday = false;
			bool haveTime = false;
			DayOfWeek weekday;
			TimeSpan offset;

			for (int i = 0; i < tokens.Count; i++) {
				int value;

				if (!haveWeekday && tokens[i].IsWeekday) {
					if (TryGetWeekday (tokens[i], text, out weekday)) {
						haveWeekday = true;
						continue;
					}
				}

				if (month == null && tokens[i].IsMonth) {
					if (month == null && TryGetMonth (tokens[i], text, out value)) {
						month = value;
						continue;
					}
				}

				if (!haveTime && tokens[i].IsTimeOfDay) {
					if (TryGetTimeOfDay (tokens[i], text, out hour, out minute, out second)) {
						haveTime = true;
						continue;
					}
				}

				if (tzone == null && tokens[i].IsTimeZone) {
					if (TryGetTimeZone (tokens[i], text, out value)) {
						tzone = value;
						continue;
					}
				}

				if (tokens[i].IsNumeric) {
					if (tokens[i].Length == 4) {
						if (year == null && TryGetYear (tokens[i], text, out value))
							year = value;

						continue;
					}

					if (tokens[i].Length > 2)
						continue;

					// Note: we likely have either YYYY[-/]MM[-/]DD or MM[-/]DD[-/]YY
					int endIndex = tokens[i].StartIndex + tokens[i].Length;
					int index = tokens[i].StartIndex;

					ParseUtils.TryParseInt32 (text, ref index, endIndex, out value);

					if (month == null && value > 0 && value <= 12) {
						month = value;
						continue;
					}

					if (day == null && value > 0 && value <= 31) {
						day = value;
						continue;
					}

					if (year == null && value >= 69) {
						year = 1900 + value;
						continue;
					}
				}

				// WTF is this??
			}

			if (year == null || month == null || day == null) {
				date = new DateTimeOffset ();
				return false;
			}

			if (!haveTime)
				hour = minute = second = 0;

			if (tzone != null) {
				int minutes = tzone.Value % 100;
				int hours = tzone.Value / 100;

				offset = new TimeSpan (hours, minutes, 0);
			} else {
				offset = new TimeSpan (0);
			}

			date = new DateTimeOffset (year.Value, month.Value, day.Value, hour, minute, second, offset);

			return true;
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="System.DateTimeOffset"/> instance.
		/// </summary>
		/// <returns><c>true</c>, if the date was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="date">The parsed date.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static bool TryParseDateTime (byte[] buffer, int startIndex, int length, out DateTimeOffset date)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (length < 0 || startIndex + length > buffer.Length)
				throw new ArgumentOutOfRangeException ("length");

			var tokens = new List<DateToken> (TokenizeDate (buffer, startIndex, length));

			if (TryParseStandardDateFormat (tokens, buffer, out date))
				return true;

			if (TryParseUnknownDateFormat (tokens, buffer, out date))
				return true;

			date = new DateTimeOffset ();

			return false;
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="System.DateTimeOffset"/> instance.
		/// </summary>
		/// <returns><c>true</c>, if the date was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="date">The parsed date.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> is not within the range of the byte array.
		/// </exception>
		public static bool TryParseDateTime (byte[] buffer, int startIndex, out DateTimeOffset date)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			int length = buffer.Length - startIndex;
			var tokens = new List<DateToken> (TokenizeDate (buffer, startIndex, length));

			if (TryParseStandardDateFormat (tokens, buffer, out date))
				return true;

			if (TryParseUnknownDateFormat (tokens, buffer, out date))
				return true;

			date = new DateTimeOffset ();

			return false;
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="System.DateTimeOffset"/> instance.
		/// </summary>
		/// <returns><c>true</c>, if the date was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="date">The parsed date.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		public static bool TryParseDateTime (byte[] buffer, out DateTimeOffset date)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			var tokens = new List<DateToken> (TokenizeDate (buffer, 0, buffer.Length));

			if (TryParseStandardDateFormat (tokens, buffer, out date))
				return true;

			if (TryParseUnknownDateFormat (tokens, buffer, out date))
				return true;

			date = new DateTimeOffset ();

			return false;
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="System.DateTimeOffset"/> instance.
		/// </summary>
		/// <returns><c>true</c>, if the date was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="text">The input text.</param>
		/// <param name="date">The parsed date.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public static bool TryParseDateTime (string text, out DateTimeOffset date)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			var buffer = Encoding.UTF8.GetBytes (text);
			var tokens = new List<DateToken> (TokenizeDate (buffer, 0, buffer.Length));

			if (TryParseStandardDateFormat (tokens, buffer, out date))
				return true;

			if (TryParseUnknownDateFormat (tokens, buffer, out date))
				return true;

			date = new DateTimeOffset ();

			return false;
		}

		/// <summary>
		/// Formats the <see cref="System.DateTimeOffset"/> as an rfc0822 date string.
		/// </summary>
		/// <returns>The formatted string.</returns>
		/// <param name="date">The date.</param>
		public static string ToString (DateTimeOffset date)
		{
			return string.Format ("{0} {1}{2}", date.ToString ("ddd, dd MMM yyyy HH:mm:ss"),
			                      date.Offset.CompareTo (TimeSpan.Zero) < 0 ? "-" : "+",
			                      date.Offset.ToString ("hhmm"));
		}
	}
}

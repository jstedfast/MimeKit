//
// TnefTimeZone.cs
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
using System.Text;
using System.Globalization;

namespace MimeKit.Tnef {
	/// <summary>
	/// A SYSTEMTIME structure ([MS-DTYP] 2.3.13) as used by the time zone rules of [MS-OXOCAL].
	/// </summary>
	readonly struct TnefSystemTime : IEquatable<TnefSystemTime>
	{
		public readonly ushort Year;
		public readonly ushort Month;
		public readonly ushort DayOfWeek;
		public readonly ushort Day;
		public readonly ushort Hour;
		public readonly ushort Minute;
		public readonly ushort Second;
		public readonly ushort Milliseconds;

		public TnefSystemTime (ushort year, ushort month, ushort dayOfWeek, ushort day, ushort hour, ushort minute, ushort second, ushort milliseconds)
		{
			Year = year;
			Month = month;
			DayOfWeek = dayOfWeek;
			Day = day;
			Hour = hour;
			Minute = minute;
			Second = second;
			Milliseconds = milliseconds;
		}

		public static TnefSystemTime Read (byte[] buffer, int index)
		{
			return new TnefSystemTime (
				TnefBinaryReader.ToUInt16 (buffer, index),
				TnefBinaryReader.ToUInt16 (buffer, index + 2),
				TnefBinaryReader.ToUInt16 (buffer, index + 4),
				TnefBinaryReader.ToUInt16 (buffer, index + 6),
				TnefBinaryReader.ToUInt16 (buffer, index + 8),
				TnefBinaryReader.ToUInt16 (buffer, index + 10),
				TnefBinaryReader.ToUInt16 (buffer, index + 12),
				TnefBinaryReader.ToUInt16 (buffer, index + 14));
		}

		public bool Equals (TnefSystemTime other)
		{
			return Year == other.Year && Month == other.Month && DayOfWeek == other.DayOfWeek && Day == other.Day &&
				Hour == other.Hour && Minute == other.Minute && Second == other.Second && Milliseconds == other.Milliseconds;
		}

		public override bool Equals (object? obj)
		{
			return obj is TnefSystemTime other && Equals (other);
		}

		public override int GetHashCode ()
		{
			return (Year << 16) ^ (Month << 12) ^ (DayOfWeek << 8) ^ (Day << 4) ^ (Hour << 10) ^ (Minute << 2) ^ Second;
		}

		// A transition date either uses the "day-in-month" format (Year is 0, Day is the week of the month, 5 meaning
		// the last one, of DayOfWeek) or is an absolute date (Year is not 0, Day is the day of the month).
		public bool IsRelative {
			get { return Year == 0; }
		}

		public bool IsValidTransition {
			get {
				if (Month < 1 || Month > 12 || Hour > 23 || Minute > 59 || Second > 59)
					return false;

				if (IsRelative)
					return DayOfWeek <= 6 && Day >= 1 && Day <= 5;

				return Day >= 1 && Day <= 31;
			}
		}

		/// <summary>
		/// Get the local date and time of the transition in the specified year.
		/// </summary>
		public DateTime GetTransition (int year)
		{
			DateTime date;

			if (IsRelative) {
				var first = new DateTime (year, Month, 1);
				int offset = (DayOfWeek - (int) first.DayOfWeek + 7) % 7;
				int days = DateTime.DaysInMonth (year, Month);
				int day = 1 + offset + (Day - 1) * 7;

				while (day > days)
					day -= 7;

				date = new DateTime (year, Month, day);
			} else {
				date = new DateTime (year, Month, Math.Min ((int) Day, DateTime.DaysInMonth (year, Month)));
			}

			return date.Add (new TimeSpan (Hour, Minute, Second));
		}
	}

	/// <summary>
	/// A time zone as described by a TZRule ([MS-OXOCAL] 2.2.1.41.1) or by PidLidTimeZoneStruct ([MS-OXOCAL] 2.2.1.39).
	/// </summary>
	/// <remarks>
	/// The bias values are in minutes and follow the Windows convention: UTC = local time + bias.
	/// </remarks>
	sealed class TnefTimeZone
	{
		// [MS-OXOCAL] 2.2.1.41.1: TZRULE_FLAG_EFFECTIVE_TZREG marks the rule that is in effect.
		const ushort EffectiveRuleFlag = 0x0002;
		const int TZRuleSize = 66;
		const int MaxBias = 24 * 60;

		public TnefTimeZone (string id, int bias, int standardBias, int daylightBias, TnefSystemTime standardDate, TnefSystemTime daylightDate)
		{
			Id = id;
			Bias = bias;
			StandardBias = standardBias;
			DaylightBias = daylightBias;

			// A time zone that does not observe daylight saving time has a Month of 0 in both dates. Treat a zone that
			// only defines one of the two transitions the same way, since the other one cannot be inferred.
			if (standardDate.Month != 0 && daylightDate.Month != 0) {
				StandardDate = standardDate;
				DaylightDate = daylightDate;
				HasDaylightTime = true;
			}
		}

		public string Id { get; set; }

		public int Bias { get; }

		public int StandardBias { get; }

		public int DaylightBias { get; }

		public TnefSystemTime StandardDate { get; }

		public TnefSystemTime DaylightDate { get; }

		public bool HasDaylightTime { get; }

		// The offsets from UTC, in minutes, as used by iCalendar (local time = UTC + offset).
		public int StandardOffset {
			get { return -(Bias + StandardBias); }
		}

		public int DaylightOffset {
			get { return HasDaylightTime ? -(Bias + DaylightBias) : StandardOffset; }
		}

		static bool IsValidBias (int bias)
		{
			// Note: Math.Abs (int.MinValue) throws, so compare against both bounds.
			return bias >= -MaxBias && bias <= MaxBias;
		}

		static bool IsValid (int bias, int standardBias, int daylightBias, TnefSystemTime standardDate, TnefSystemTime daylightDate)
		{
			if (!IsValidBias (bias) || !IsValidBias (standardBias) || !IsValidBias (daylightBias))
				return false;

			if (standardDate.Month == 0 || daylightDate.Month == 0)
				return true;

			return standardDate.IsValidTransition && daylightDate.IsValidTransition;
		}

		/// <summary>
		/// Parse a PidLidTimeZoneStruct value ([MS-OXOCAL] 2.2.1.39).
		/// </summary>
		public static TnefTimeZone? FromTimeZoneStruct (byte[] buffer, string id)
		{
			// lBias, lStandardBias, lDaylightBias, wStandardYear, stStandardDate, wDaylightYear, stDaylightDate.
			if (buffer.Length < 48)
				return null;

			int bias = TnefBinaryReader.ToInt32 (buffer, 0);
			int standardBias = TnefBinaryReader.ToInt32 (buffer, 4);
			int daylightBias = TnefBinaryReader.ToInt32 (buffer, 8);
			var standardDate = TnefSystemTime.Read (buffer, 14);
			var daylightDate = TnefSystemTime.Read (buffer, 32);

			if (!IsValid (bias, standardBias, daylightBias, standardDate, daylightDate))
				return null;

			return new TnefTimeZone (id, bias, standardBias, daylightBias, standardDate, daylightDate);
		}

		/// <summary>
		/// Parse a TZDefinition structure ([MS-OXOCAL] 2.2.1.41), using its effective TZRule.
		/// </summary>
		public static TnefTimeZone? FromTimeZoneDefinition (byte[] buffer, string? fallbackId)
		{
			// Major Version (0x02), Minor Version, cbHeader, Reserved, cchKeyName, KeyName, cRules, TZRules.
			if (buffer.Length < 6 || buffer[0] != 0x02)
				return null;

			int headerSize = TnefBinaryReader.ToUInt16 (buffer, 2);
			int rulesIndex = 4 + headerSize;

			if (headerSize < 6 || rulesIndex > buffer.Length)
				return null;

			int keyNameLength = TnefBinaryReader.ToUInt16 (buffer, 6);
			string? id = null;

			if (8 + keyNameLength * 2 <= rulesIndex - 2)
				id = Encoding.Unicode.GetString (buffer, 8, keyNameLength * 2).TrimEnd ('\0');

			int ruleCount = TnefBinaryReader.ToUInt16 (buffer, rulesIndex - 2);
			int ruleIndex = -1;

			for (int i = 0; i < ruleCount; i++) {
				int index = rulesIndex + i * TZRuleSize;

				if (index + TZRuleSize > buffer.Length)
					break;

				// The effective rule is the one to use; fall back to the last complete rule.
				ruleIndex = index;

				if ((TnefBinaryReader.ToUInt16 (buffer, index + 4) & EffectiveRuleFlag) != 0)
					break;
			}

			if (ruleIndex == -1)
				return null;

			// TZRule: Major, Minor, Reserved, Flags, wYear, X (14 bytes), lBias, lStandardBias, lDaylightBias,
			// stStandardDate, stDaylightDate.
			int bias = TnefBinaryReader.ToInt32 (buffer, ruleIndex + 22);
			int standardBias = TnefBinaryReader.ToInt32 (buffer, ruleIndex + 26);
			int daylightBias = TnefBinaryReader.ToInt32 (buffer, ruleIndex + 30);
			var standardDate = TnefSystemTime.Read (buffer, ruleIndex + 34);
			var daylightDate = TnefSystemTime.Read (buffer, ruleIndex + 50);

			if (!IsValid (bias, standardBias, daylightBias, standardDate, daylightDate))
				return null;

			if (string.IsNullOrWhiteSpace (id))
				id = fallbackId;

			if (string.IsNullOrWhiteSpace (id))
				id = FormatDefaultId (bias + standardBias);

			return new TnefTimeZone (id!.Trim (), bias, standardBias, daylightBias, standardDate, daylightDate);
		}

		internal static string FormatDefaultId (int bias)
		{
			int offset = -bias;
			char sign = offset < 0 ? '-' : '+';

			offset = Math.Abs (offset);

			return string.Format (CultureInfo.InvariantCulture, "UTC{0}{1:D2}:{2:D2}", sign, offset / 60, offset % 60);
		}

		public bool HasSameRules (TnefTimeZone other)
		{
			if (Bias != other.Bias || StandardBias != other.StandardBias || HasDaylightTime != other.HasDaylightTime)
				return false;

			if (!HasDaylightTime)
				return true;

			return DaylightBias == other.DaylightBias && StandardDate.Equals (other.StandardDate) && DaylightDate.Equals (other.DaylightDate);
		}

		static int ClampYear (int year)
		{
			return Math.Max (2, Math.Min (9998, year));
		}

		public bool IsDaylightTime (DateTime utc)
		{
			if (!HasDaylightTime)
				return false;

			int year = ClampYear (utc.AddMinutes (StandardOffset).Year);

			// Daylight saving time starts at DaylightDate in local standard time and ends at StandardDate in local
			// daylight time.
			var start = DaylightDate.GetTransition (year).AddMinutes (-StandardOffset);
			var end = StandardDate.GetTransition (year).AddMinutes (-DaylightOffset);

			if (start < end)
				return utc >= start && utc < end;

			// Southern hemisphere: daylight saving time spans the end of the year.
			return utc >= start || utc < end;
		}

		public int GetOffset (DateTime utc)
		{
			return IsDaylightTime (utc) ? DaylightOffset : StandardOffset;
		}

		public DateTime ToLocalTime (DateTime utc)
		{
			return DateTime.SpecifyKind (utc.AddMinutes (GetOffset (utc)), DateTimeKind.Unspecified);
		}

		public DateTime ToUniversalTime (DateTime local)
		{
			// A local time that is repeated when daylight saving time ends is interpreted as daylight time. A local
			// time that is skipped when daylight saving time starts is interpreted as standard time.
			if (HasDaylightTime) {
				var daylight = local.AddMinutes (-DaylightOffset);

				if (IsDaylightTime (daylight))
					return DateTime.SpecifyKind (daylight, DateTimeKind.Utc);
			}

			return DateTime.SpecifyKind (local.AddMinutes (-StandardOffset), DateTimeKind.Utc);
		}

		static void WriteObservance (CalendarWriter writer, string name, TnefSystemTime date, int offsetFrom, int offsetTo)
		{
			writer.BeginComponent (name);

			// [MS-OXCICAL] 2.1.3.1.1.19: DTSTART is the transition in the year 1601, written as a local time, and the
			// RRULE describes the yearly transition.
			writer.WriteProperty ("DTSTART", CalendarWriter.FormatDateTime (date.GetTransition (1601), false));

			if (date.IsRelative) {
				int week = date.Day == 5 ? -1 : date.Day;

				writer.WriteProperty ("RRULE", string.Format (CultureInfo.InvariantCulture, "FREQ=YEARLY;BYDAY={0}{1};BYMONTH={2}", week, TnefRecurrence.WeekDays[date.DayOfWeek], date.Month));
			} else {
				writer.WriteProperty ("RRULE", string.Format (CultureInfo.InvariantCulture, "FREQ=YEARLY;BYMONTHDAY={0};BYMONTH={1}", date.Day, date.Month));
			}

			writer.WriteProperty ("TZOFFSETFROM", CalendarWriter.FormatUtcOffset (offsetFrom));
			writer.WriteProperty ("TZOFFSETTO", CalendarWriter.FormatUtcOffset (offsetTo));
			writer.EndComponent (name);
		}

		/// <summary>
		/// Write the time zone as a VTIMEZONE component ([MS-OXCICAL] 2.1.3.1.1.19).
		/// </summary>
		public void WriteTo (CalendarWriter writer)
		{
			writer.BeginComponent ("VTIMEZONE");
			writer.WriteTextProperty ("TZID", Id);

			if (HasDaylightTime) {
				WriteObservance (writer, "STANDARD", StandardDate, DaylightOffset, StandardOffset);
				WriteObservance (writer, "DAYLIGHT", DaylightDate, StandardOffset, DaylightOffset);
			} else {
				var offset = CalendarWriter.FormatUtcOffset (StandardOffset);

				writer.BeginComponent ("STANDARD");
				writer.WriteProperty ("DTSTART", "16010101T000000");
				writer.WriteProperty ("TZOFFSETFROM", offset);
				writer.WriteProperty ("TZOFFSETTO", offset);
				writer.EndComponent ("STANDARD");
			}

			writer.EndComponent ("VTIMEZONE");
		}
	}
}

//
// TnefRecurrence.cs
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
using System.Collections.Generic;

namespace MimeKit.Tnef {
	/// <summary>
	/// Little-endian readers for the binary structures stored in MAPI properties.
	/// </summary>
	static class TnefBinaryReader
	{
		public static ushort ToUInt16 (byte[] buffer, int index)
		{
			return (ushort) (buffer[index] | (buffer[index + 1] << 8));
		}

		public static int ToInt32 (byte[] buffer, int index)
		{
			return buffer[index] | (buffer[index + 1] << 8) | (buffer[index + 2] << 16) | (buffer[index + 3] << 24);
		}

		public static uint ToUInt32 (byte[] buffer, int index)
		{
			return unchecked ((uint) ToInt32 (buffer, index));
		}
	}

	/// <summary>
	/// A bounds-checked cursor over a binary structure.
	/// </summary>
	struct TnefBlobReader
	{
		readonly byte[] buffer;
		int index;

		public TnefBlobReader (byte[] buffer)
		{
			this.buffer = buffer;
			index = 0;
		}

		public int Remaining {
			get { return buffer.Length - index; }
		}

		void Ensure (long count)
		{
			if (count < 0 || count > buffer.Length - index)
				throw new FormatException ("Unexpected end of data.");
		}

		public ushort ReadUInt16 ()
		{
			Ensure (2);

			var value = TnefBinaryReader.ToUInt16 (buffer, index);
			index += 2;

			return value;
		}

		public uint ReadUInt32 ()
		{
			Ensure (4);

			var value = TnefBinaryReader.ToUInt32 (buffer, index);
			index += 4;

			return value;
		}

		public void Skip (long count)
		{
			Ensure (count);
			index += (int) count;
		}

		public string ReadString (int count, Encoding encoding)
		{
			Ensure (count);

			var value = encoding.GetString (buffer, index, count);
			index += count;

			return value.TrimEnd ('\0');
		}

		public uint[] ReadUInt32Array (uint count)
		{
			Ensure (count * 4L);

			var values = new uint[count];

			for (int i = 0; i < values.Length; i++)
				values[i] = ReadUInt32 ();

			return values;
		}
	}

	/// <summary>
	/// An ExceptionInfo structure ([MS-OXOCAL] 2.2.1.44.2), merged with the matching ExtendedException structure
	/// ([MS-OXOCAL] 2.2.1.44.3).
	/// </summary>
	sealed class TnefRecurrenceException
	{
		public const ushort SubjectFlag = 0x0001;
		public const ushort MeetingTypeFlag = 0x0002;
		public const ushort ReminderDeltaFlag = 0x0004;
		public const ushort ReminderFlag = 0x0008;
		public const ushort LocationFlag = 0x0010;
		public const ushort BusyStatusFlag = 0x0020;
		public const ushort AttachmentFlag = 0x0040;
		public const ushort SubTypeFlag = 0x0080;
		public const ushort AppointmentColorFlag = 0x0100;
		public const ushort ExceptionalBodyFlag = 0x0200;

		// All of the date and time values are local times, in minutes since midnight January 1, 1601.
		public uint StartDateTime { get; set; }
		public uint EndDateTime { get; set; }
		public uint OriginalStartDate { get; set; }
		public ushort OverrideFlags { get; set; }
		public string? Subject { get; set; }
		public string? Location { get; set; }
		public int ReminderDelta { get; set; }
		public bool ReminderSet { get; set; }
		public int BusyStatus { get; set; }
		public bool IsAllDay { get; set; }

		public bool HasOverride (ushort flag)
		{
			return (OverrideFlags & flag) != 0;
		}
	}

	/// <summary>
	/// An AppointmentRecurrencePattern structure ([MS-OXOCAL] 2.2.1.44.5) from PidLidAppointmentRecur.
	/// </summary>
	sealed class TnefRecurrence
	{
		internal static readonly string[] WeekDays = { "SU", "MO", "TU", "WE", "TH", "FR", "SA" };

		static readonly DateTime Epoch = new DateTime (1601, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

		// [MS-OXOCAL] 2.2.1.44.1: RecurrencePattern.PatternType values.
		public const ushort PatternDay = 0x0000;
		public const ushort PatternWeek = 0x0001;
		public const ushort PatternMonth = 0x0002;
		public const ushort PatternMonthNth = 0x0003;
		public const ushort PatternMonthEnd = 0x0004;
		public const ushort PatternHjMonth = 0x000A;
		public const ushort PatternHjMonthNth = 0x000B;
		public const ushort PatternHjMonthEnd = 0x000C;

		// [MS-OXOCAL] 2.2.1.44.1: RecurrencePattern.EndType values.
		public const uint EndAfterDate = 0x00002021;
		public const uint EndAfterOccurrences = 0x00002022;

		const uint ReaderVersion = 0x3004;
		const uint ExtendedExceptionVersion = 0x00003009;

		public ushort PatternType { get; private set; }
		public ushort CalendarType { get; private set; }
		public uint FirstDateTime { get; private set; }
		public uint Period { get; private set; }

		// PatternTypeSpecific: the Sa-Su bitmask (Week and MonthNth), the day of the month (Month and MonthEnd) and
		// the week of the month (MonthNth).
		public uint DayMask { get; private set; }
		public uint DayOfMonth { get; private set; }
		public uint WeekOfMonth { get; private set; }

		public uint EndType { get; private set; }
		public uint OccurrenceCount { get; private set; }
		public uint FirstDayOfWeek { get; private set; }
		public uint[] DeletedInstanceDates { get; private set; } = Array.Empty<uint> ();
		public uint[] ModifiedInstanceDates { get; private set; } = Array.Empty<uint> ();
		public uint StartDate { get; private set; }
		public uint EndDate { get; private set; }
		public uint StartTimeOffset { get; private set; }
		public uint EndTimeOffset { get; private set; }
		public List<TnefRecurrenceException> Exceptions { get; } = new List<TnefRecurrenceException> ();

		/// <summary>
		/// Set when the exception data could not be fully parsed.
		/// </summary>
		public string? ExceptionError { get; private set; }

		public static DateTime ToDateTime (uint minutes)
		{
			return Epoch.AddMinutes (minutes);
		}

		public static uint ToMinutes (DateTime value)
		{
			return (uint) ((value.Ticks - Epoch.Ticks) / TimeSpan.TicksPerMinute);
		}

		/// <summary>
		/// Parse an AppointmentRecurrencePattern.
		/// </summary>
		/// <exception cref="FormatException">The data is truncated or invalid.</exception>
		public static TnefRecurrence Parse (byte[] buffer, Encoding encoding)
		{
			var reader = new TnefBlobReader (buffer);
			var recurrence = new TnefRecurrence ();

			// RecurrencePattern ([MS-OXOCAL] 2.2.1.44.1).
			if (reader.ReadUInt16 () != ReaderVersion)
				throw new FormatException ("Unsupported recurrence pattern version.");

			reader.ReadUInt16 (); // WriterVersion
			reader.ReadUInt16 (); // RecurFrequency
			recurrence.PatternType = reader.ReadUInt16 ();
			recurrence.CalendarType = reader.ReadUInt16 ();
			recurrence.FirstDateTime = reader.ReadUInt32 ();
			recurrence.Period = reader.ReadUInt32 ();
			reader.ReadUInt32 (); // SlidingFlag

			switch (recurrence.PatternType) {
			case PatternDay:
				break;
			case PatternWeek:
				recurrence.DayMask = reader.ReadUInt32 ();
				break;
			case PatternMonth:
			case PatternMonthEnd:
			case PatternHjMonth:
			case PatternHjMonthEnd:
				recurrence.DayOfMonth = reader.ReadUInt32 ();
				break;
			case PatternMonthNth:
			case PatternHjMonthNth:
				recurrence.DayMask = reader.ReadUInt32 ();
				recurrence.WeekOfMonth = reader.ReadUInt32 ();
				break;
			default:
				throw new FormatException ($"Unknown recurrence pattern type 0x{recurrence.PatternType:X4}.");
			}

			recurrence.EndType = reader.ReadUInt32 ();
			recurrence.OccurrenceCount = reader.ReadUInt32 ();
			recurrence.FirstDayOfWeek = reader.ReadUInt32 ();
			recurrence.DeletedInstanceDates = reader.ReadUInt32Array (reader.ReadUInt32 ());
			recurrence.ModifiedInstanceDates = reader.ReadUInt32Array (reader.ReadUInt32 ());
			recurrence.StartDate = reader.ReadUInt32 ();
			recurrence.EndDate = reader.ReadUInt32 ();

			// The rest of the AppointmentRecurrencePattern.
			reader.ReadUInt32 (); // ReaderVersion2
			var writerVersion2 = reader.ReadUInt32 ();
			recurrence.StartTimeOffset = reader.ReadUInt32 ();
			recurrence.EndTimeOffset = reader.ReadUInt32 ();

			// The exceptions are optional as far as this converter is concerned: if they cannot be parsed, the
			// recurrence itself can still be exported. Discard any that were read, since the data they were read from
			// is evidently not laid out as expected.
			try {
				ReadExceptions (ref reader, recurrence, writerVersion2, encoding);
			} catch (FormatException ex) {
				recurrence.Exceptions.Clear ();
				recurrence.ExceptionError = ex.Message;
			}

			return recurrence;
		}

		static void ReadExceptions (ref TnefBlobReader reader, TnefRecurrence recurrence, uint writerVersion2, Encoding encoding)
		{
			int count = reader.ReadUInt16 ();

			// ExceptionInfo ([MS-OXOCAL] 2.2.1.44.2).
			for (int i = 0; i < count; i++) {
				var exception = new TnefRecurrenceException {
					StartDateTime = reader.ReadUInt32 (),
					EndDateTime = reader.ReadUInt32 (),
					OriginalStartDate = reader.ReadUInt32 (),
					OverrideFlags = reader.ReadUInt16 ()
				};

				if (exception.HasOverride (TnefRecurrenceException.SubjectFlag)) {
					reader.ReadUInt16 (); // SubjectLength
					exception.Subject = reader.ReadString (reader.ReadUInt16 (), encoding);
				}

				if (exception.HasOverride (TnefRecurrenceException.MeetingTypeFlag))
					reader.ReadUInt32 ();

				if (exception.HasOverride (TnefRecurrenceException.ReminderDeltaFlag))
					exception.ReminderDelta = (int) reader.ReadUInt32 ();

				if (exception.HasOverride (TnefRecurrenceException.ReminderFlag))
					exception.ReminderSet = reader.ReadUInt32 () != 0;

				if (exception.HasOverride (TnefRecurrenceException.LocationFlag)) {
					reader.ReadUInt16 (); // LocationLength
					exception.Location = reader.ReadString (reader.ReadUInt16 (), encoding);
				}

				if (exception.HasOverride (TnefRecurrenceException.BusyStatusFlag))
					exception.BusyStatus = (int) reader.ReadUInt32 ();

				if (exception.HasOverride (TnefRecurrenceException.AttachmentFlag))
					reader.ReadUInt32 ();

				if (exception.HasOverride (TnefRecurrenceException.SubTypeFlag))
					exception.IsAllDay = reader.ReadUInt32 () != 0;

				if (exception.HasOverride (TnefRecurrenceException.AppointmentColorFlag))
					reader.ReadUInt32 ();

				recurrence.Exceptions.Add (exception);
			}

			reader.Skip (reader.ReadUInt32 ()); // ReservedBlock1

			// ExtendedException ([MS-OXOCAL] 2.2.1.44.3): one for each ExceptionInfo, carrying the Unicode subject
			// and location, which are preferred over the 8-bit strings in the ExceptionInfo.
			foreach (var exception in recurrence.Exceptions) {
				if (writerVersion2 >= ExtendedExceptionVersion)
					reader.Skip (reader.ReadUInt32 ()); // ChangeHighlight

				reader.Skip (reader.ReadUInt32 ()); // ReservedBlockEE1

				bool subject = exception.HasOverride (TnefRecurrenceException.SubjectFlag);
				bool location = exception.HasOverride (TnefRecurrenceException.LocationFlag);

				if (!subject && !location)
					continue;

				reader.ReadUInt32 (); // StartDateTime
				reader.ReadUInt32 (); // EndDateTime
				reader.ReadUInt32 (); // OriginalStartDate

				if (subject)
					exception.Subject = reader.ReadString (reader.ReadUInt16 () * 2, Encoding.Unicode);

				if (location)
					exception.Location = reader.ReadString (reader.ReadUInt16 () * 2, Encoding.Unicode);

				reader.Skip (reader.ReadUInt32 ()); // ReservedBlockEE2
			}
		}
	}
}

//
// TnefCalendarRobustnessTests.cs
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

using System.Text;

using MimeKit;
using MimeKit.Tnef;

using static UnitTests.Tnef.TnefCalendarTests;

namespace UnitTests.Tnef {
	// Feeds truncated, mutated and adversarial calendar data through the TNEF calendar export to make sure that bad input
	// is reported as a conversion loss rather than escaping as an exception, and that the output stays bounded.
	[TestFixture]
	public class TnefCalendarRobustnessTests
	{
		static readonly TnefConversionLossKind[] CalendarLossKinds = {
			TnefConversionLossKind.InvalidCalendarData,
			TnefConversionLossKind.UnsupportedCalendarData,
			TnefConversionLossKind.CalendarExceptionLimitExceeded
		};

		static byte[] Mutate (Random random, byte[] data)
		{
			var mutated = (byte[]) data.Clone ();
			int count = random.Next (1, 5);

			for (int i = 0; i < count; i++) {
				int index = random.Next (mutated.Length);

				switch (random.Next (3)) {
				case 0: mutated[index] = (byte) random.Next (256); break;
				case 1: mutated[index] = 0xFF; break;
				default: mutated[index] ^= (byte) (1 << random.Next (8)); break;
				}
			}

			return mutated;
		}

		// Converts the message and checks that the calendar part, if any, is well-formed and that only calendar losses
		// were reported.
		static void AssertConvertsGracefully (TnefBuilder builder, string context, TnefConversionOptions options = null)
		{
			TnefConversionResult result;

			try {
				result = Convert (builder, options);
			} catch (Exception ex) {
				Assert.Fail ($"{context}: {ex.GetType ().Name}: {ex.Message}");
				return;
			}

			using (result) {
				foreach (var loss in result.Losses)
					Assert.That (CalendarLossKinds, Does.Contain (loss.Kind), context);

				var part = GetCalendarPart (result.Message);

				if (part != null) {
					var lines = ReadCalendar (part);

					Assert.That (lines[0], Is.EqualTo ("BEGIN:VCALENDAR"), context);
					Assert.That (lines[lines.Length - 1], Is.EqualTo ("END:VCALENDAR"), context);
					Assert.That (lines.Count (line => line == "BEGIN:VEVENT"), Is.EqualTo (lines.Count (line => line == "END:VEVENT")), context);
				}
			}
		}

		// A weekly recurrence on every day of the week at 10:00 Eastern, starting on 2024-07-01, in which each of the
		// first `count` occurrences is moved to 13:00.
		static byte[] CreateRecurrenceWithExceptions (int count, uint? deletedCount = null, uint startTimeOffset = 600)
		{
			var startDate = new DateTime (2024, 7, 1);

			using (var stream = new MemoryStream ())
			using (var writer = new BinaryWriter (stream)) {
				writer.Write ((ushort) 0x3004);
				writer.Write ((ushort) 0x3004);
				writer.Write ((ushort) 0x200B);
				writer.Write ((ushort) 1); // Week
				writer.Write ((ushort) 0); // Gregorian
				writer.Write (0u); // FirstDateTime
				writer.Write (1u); // Period
				writer.Write (0u); // SlidingFlag
				writer.Write (0x7Fu); // Every day
				writer.Write (0x2022u); // EndAfterOccurrences
				writer.Write ((uint) count + 10);
				writer.Write (0u); // FirstDOW

				writer.Write (deletedCount ?? (uint) count);
				for (int i = 0; i < count; i++)
					writer.Write (ToMinutes (startDate.AddDays (i)));

				writer.Write ((uint) count);
				for (int i = 0; i < count; i++)
					writer.Write (ToMinutes (startDate.AddDays (i)));

				writer.Write (ToMinutes (startDate));
				writer.Write (ToMinutes (startDate.AddDays (count + 10)));

				writer.Write (0x3006u); // ReaderVersion2
				writer.Write (0x3009u); // WriterVersion2
				writer.Write (startTimeOffset);
				writer.Write (startTimeOffset + 60);

				writer.Write ((ushort) count);
				for (int i = 0; i < count; i++) {
					var original = startDate.AddDays (i).AddMinutes (600);
					var moved = original.AddHours (3);

					writer.Write (ToMinutes (moved));
					writer.Write (ToMinutes (moved.AddHours (1)));
					writer.Write (ToMinutes (original));
					writer.Write ((ushort) 0);
				}

				writer.Write (0u); // ReservedBlock1Size

				for (int i = 0; i < count; i++) {
					writer.Write (4u); // ChangeHighlightSize
					writer.Write (0u); // ChangeHighlightValue
					writer.Write (0u); // ReservedBlockEE1Size
				}

				writer.Write (0u); // ReservedBlock2Size
				writer.Flush ();

				return stream.ToArray ();
			}
		}

		[Test]
		public void TestRecurrenceParserTruncation ()
		{
			var recurrence = CreateWeeklyRecurrence ();

			for (int length = 0; length < recurrence.Length; length++) {
				var truncated = new byte[length];

				Array.Copy (recurrence, truncated, length);

				try {
					TnefRecurrence.Parse (truncated, Encoding.ASCII);
				} catch (FormatException) {
				}
			}
		}

		[Test]
		public void TestRecurrenceParserFuzz ()
		{
			var recurrence = CreateWeeklyRecurrence ();
			var random = new Random (20240701);

			for (int i = 0; i < 5000; i++) {
				var mutated = Mutate (random, recurrence);

				try {
					var parsed = TnefRecurrence.Parse (mutated, Encoding.ASCII);

					Assert.That (parsed.Exceptions.Count, Is.LessThanOrEqualTo (ushort.MaxValue));
				} catch (FormatException) {
				}
			}
		}

		[Test]
		public void TestRecurrenceTruncationSweep ()
		{
			var recurrence = CreateWeeklyRecurrence ();

			for (int length = 0; length < recurrence.Length; length++) {
				var truncated = new byte[length];

				Array.Copy (recurrence, truncated, length);

				AssertConvertsGracefully (CreateRecurringMeeting (truncated), $"Truncated to {length} bytes");
			}
		}

		[Test]
		public void TestRecurrenceMutationFuzz ()
		{
			var recurrence = CreateWeeklyRecurrence ();
			var random = new Random (1601);

			for (int i = 0; i < 300; i++) {
				var mutated = Mutate (random, recurrence);

				AssertConvertsGracefully (CreateRecurringMeeting (mutated), $"Iteration {i}: {BitConverter.ToString (mutated)}");
			}
		}

		[Test]
		public void TestTimeZoneDefinitionTruncationAndFuzz ()
		{
			var definition = CreateEasternTimeZoneDefinition ();
			var random = new Random (2007);
			var candidates = new List<byte[]> ();

			for (int length = 0; length < definition.Length; length++) {
				var truncated = new byte[length];

				Array.Copy (definition, truncated, length);
				candidates.Add (truncated);
			}

			for (int i = 0; i < 2000; i++)
				candidates.Add (Mutate (random, definition));

			foreach (var candidate in candidates) {
				var context = BitConverter.ToString (candidate);
				TnefTimeZone zone;

				try {
					zone = TnefTimeZone.FromTimeZoneDefinition (candidate, "Fallback");
				} catch (Exception ex) {
					Assert.Fail ($"{context}: {ex.GetType ().Name}: {ex.Message}");
					return;
				}

				if (zone != null)
					AssertTimeZoneIsUsable (zone, context);
			}
		}

		[Test]
		public void TestTimeZoneStructFuzz ()
		{
			var random = new Random (48);

			for (int i = 0; i < 2000; i++) {
				var structure = new byte[random.Next (0, 64)];

				random.NextBytes (structure);

				// Keep some candidates plausible so that the transition logic is exercised, not just the validation.
				if (structure.Length >= 48 && (i & 1) == 0) {
					BitConverter.GetBytes (random.Next (-1440, 1441)).CopyTo (structure, 0);
					BitConverter.GetBytes (0).CopyTo (structure, 4);
					BitConverter.GetBytes (-60).CopyTo (structure, 8);
					SystemTime (random.Next (0, 13), random.Next (0, 7), random.Next (1, 6), random.Next (0, 24)).CopyTo (structure, 16);
					SystemTime (random.Next (0, 13), random.Next (0, 7), random.Next (1, 6), random.Next (0, 24)).CopyTo (structure, 32);
				}

				var context = BitConverter.ToString (structure);
				TnefTimeZone zone;

				try {
					zone = TnefTimeZone.FromTimeZoneStruct (structure, "Fuzz");
				} catch (Exception ex) {
					Assert.Fail ($"{context}: {ex.GetType ().Name}: {ex.Message}");
					return;
				}

				if (zone != null)
					AssertTimeZoneIsUsable (zone, context);
			}
		}

		static void AssertTimeZoneIsUsable (TnefTimeZone zone, string context)
		{
			try {
				for (int year = 1990; year <= 2040; year += 5) {
					var utc = new DateTime (year, 3, 15, 12, 0, 0, DateTimeKind.Utc);

					zone.GetOffset (utc);
					zone.ToUniversalTime (zone.ToLocalTime (utc));
				}

				using (var stream = new MemoryStream ())
					zone.WriteTo (new CalendarWriter (stream));
			} catch (Exception ex) {
				Assert.Fail ($"{context}: {ex.GetType ().Name}: {ex.Message}");
			}
		}

		[Test]
		public void TestTimeZoneDefinitionMutationEndToEnd ()
		{
			var definition = CreateEasternTimeZoneDefinition ();
			var recurrence = CreateWeeklyRecurrence ();
			var random = new Random (300);

			for (int i = 0; i < 200; i++) {
				var mutated = Mutate (random, definition);

				AssertConvertsGracefully (CreateRecurringMeeting (recurrence, timeZone: mutated), $"Iteration {i}: {BitConverter.ToString (mutated)}");
			}
		}

		static IEnumerable<byte[]> GetMalformedGlobalObjectIds ()
		{
			yield return Array.Empty<byte> ();
			yield return new byte[1];
			yield return new byte[39];

			var id = CreateGlobalObjectId (Encoding.ASCII.GetBytes ("vCal-Uid\x01\x00\x00\x00abc\x00"));
			yield return id;

			foreach (var size in new[] { -1, int.MinValue, int.MaxValue, 0x10000, 12, 13, 8 }) {
				var copy = (byte[]) id.Clone ();
				BitConverter.GetBytes (size).CopyTo (copy, 36);
				yield return copy;
			}

			// A vCal-Uid signature with no room for a UID.
			var truncated = new byte[48];
			Array.Copy (id, truncated, truncated.Length);
			yield return truncated;

			// Instance dates that are not valid dates.
			yield return CreateGlobalObjectId (null, 0xFFFF, 0xFF, 0xFF);
			yield return CreateGlobalObjectId (null, 2024, 2, 31);
		}

		[Test]
		public void TestMalformedGlobalObjectId ()
		{
			int index = 0;

			foreach (var id in GetMalformedGlobalObjectIds ()) {
				var properties = CreateAppointment (new DateTime (2024, 7, 1, 14, 0, 0, DateTimeKind.Utc), new DateTime (2024, 7, 1, 15, 0, 0, DateTimeKind.Utc), out var named);

				named.Binary (PidLidGlobalObjectId, id, TnefPropertySetGuid.Meeting);

				var builder = CreateMessage ("IPM.Schedule.Meeting.Request", properties, Recipient (TnefRecipientType.To, "Bob", "bob@example.com"));
				var context = $"GlobalObjectId #{index++}: {BitConverter.ToString (id)}";

				AssertConvertsGracefully (builder, context);

				using (var result = Convert (builder)) {
					var lines = ReadCalendar (GetCalendarPart (result.Message));
					var uid = lines.Single (line => line.StartsWith ("UID:", StringComparison.Ordinal));

					Assert.That (uid.Length, Is.GreaterThan (4), context);
				}
			}
		}

		[Test]
		public void TestHugeDeletedInstanceCount ()
		{
			var recurrence = CreateRecurrenceWithExceptions (1, deletedCount: 0xFFFFFFFF);

			using (var result = Convert (CreateRecurringMeeting (recurrence, false))) {
				Assert.That (result.Losses.Select (loss => loss.Kind), Is.EqualTo (new[] { TnefConversionLossKind.InvalidCalendarData }), string.Join ("; ", result.Losses.Select (loss => loss.Description)));

				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines.Any (line => line.StartsWith ("RRULE", StringComparison.Ordinal)), Is.False);
			}
		}

		[Test]
		public void TestHugeExceptionCount ()
		{
			// Claims 65535 exceptions, but only has data for one.
			var recurrence = CreateRecurrenceWithExceptions (1);
			var offset = recurrence.Length - (4 + 12 + 4 + 14 + 2);

			Assert.That (BitConverter.ToUInt16 (recurrence, offset), Is.EqualTo (1));
			BitConverter.GetBytes (ushort.MaxValue).CopyTo (recurrence, offset);

			using (var result = Convert (CreateRecurringMeeting (recurrence, false))) {
				Assert.That (result.Losses.Select (loss => loss.Kind), Is.EqualTo (new[] { TnefConversionLossKind.InvalidCalendarData }));

				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines.Count (line => line == "BEGIN:VEVENT"), Is.EqualTo (1));
				Assert.That (lines.Any (line => line.StartsWith ("RRULE", StringComparison.Ordinal)), Is.True);
			}
		}

		[TestCase (0xFFFFFFFFu)]
		[TestCase (0x7FFFFFFFu)]
		[TestCase (0x80000000u)]
		public void TestExtremeStartTimeOffset (uint startTimeOffset)
		{
			AssertConvertsGracefully (CreateRecurringMeeting (CreateRecurrenceWithExceptions (2, startTimeOffset: startTimeOffset), false), $"StartTimeOffset {startTimeOffset}");
		}

		[Test]
		public void TestExtremeDates ()
		{
			var recurrence = CreateWeeklyRecurrence ();

			// Move every date field to the end of time.
			foreach (var offset in new[] { 42, 54, 58, 62, 74 }) {
				var copy = (byte[]) recurrence.Clone ();

				BitConverter.GetBytes (uint.MaxValue).CopyTo (copy, offset);
				AssertConvertsGracefully (CreateRecurringMeeting (copy), $"Offset {offset}");
			}

			var properties = CreateAppointment (DateTime.MaxValue.AddDays (-1), DateTime.MaxValue, out _);

			AssertConvertsGracefully (CreateMessage ("IPM.Appointment", properties), "DateTime.MaxValue");
		}

		static void AssertExceptionAttachmentKept (MimeMessage message)
		{
			var mixed = (Multipart) message.Body;

			Assert.That (mixed.ContentType.MimeType, Is.EqualTo ("multipart/mixed"));
			Assert.That (mixed.Count, Is.EqualTo (2));
			Assert.That (mixed[0].ContentType.MimeType, Is.EqualTo ("text/calendar"));
			Assert.That (mixed[1], Is.InstanceOf<TnefPart> ());
		}

		[Test]
		public void TestMaxCalendarExceptionsZero ()
		{
			var options = new TnefConversionOptions { MaxCalendarExceptions = 0 };

			using (var result = Convert (CreateRecurringMeeting (CreateWeeklyRecurrence ()), options)) {
				Assert.That (result.Losses.Select (loss => loss.Kind), Is.EqualTo (new[] { TnefConversionLossKind.CalendarExceptionLimitExceeded }));

				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines.Count (line => line == "BEGIN:VEVENT"), Is.EqualTo (1));
				Assert.That (lines, Does.Contain ("RRULE:FREQ=WEEKLY;BYDAY=MO,WE;COUNT=10"));

				// The exception that was not exported is kept as an attachment.
				AssertExceptionAttachmentKept (result.Message);
			}
		}

		[Test]
		public void TestMaxCalendarExceptionsSize ()
		{
			var options = new TnefConversionOptions { MaxCalendarExceptionsSize = 10 };

			using (var result = Convert (CreateRecurringMeeting (CreateWeeklyRecurrence ()), options)) {
				var losses = result.Losses.ToArray ();

				Assert.That (losses.Select (loss => loss.Kind), Is.EqualTo (new[] { TnefConversionLossKind.CalendarExceptionLimitExceeded }));
				Assert.That (losses[0].Description, Does.Contain ("MaxCalendarExceptionsSize"));

				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines.Count (line => line == "BEGIN:VEVENT"), Is.EqualTo (1));
				AssertExceptionAttachmentKept (result.Message);
			}
		}

		[Test]
		public void TestManyExceptionsAreCapped ()
		{
			const int count = 2000;

			var recurrence = CreateRecurrenceWithExceptions (count);

			using (var result = Convert (CreateRecurringMeeting (recurrence, false))) {
				var losses = result.Losses.ToArray ();

				Assert.That (losses.Select (loss => loss.Kind), Is.EqualTo (new[] { TnefConversionLossKind.CalendarExceptionLimitExceeded }));
				Assert.That (losses[0].Description, Does.Contain ("MaxCalendarExceptions"));

				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines.Count (line => line == "BEGIN:VEVENT"), Is.EqualTo (1 + TnefConversionOptions.DefaultMaxCalendarExceptions));
				Assert.That (lines.Count (line => line.StartsWith ("RECURRENCE-ID", StringComparison.Ordinal)), Is.EqualTo (TnefConversionOptions.DefaultMaxCalendarExceptions));

				// Modified occurrences are not EXDATEs.
				Assert.That (lines.Any (line => line.StartsWith ("EXDATE", StringComparison.Ordinal)), Is.False);
			}
		}

		[Test]
		public void TestExceptionSizeBudgetBoundsAmplification ()
		{
			const long budget = 256 * 1024;

			// Each exception copies the master's 16 KB description, so 200 exceptions would be over 3 MB.
			var body = new string ('x', 16 * 1024);
			var options = new TnefConversionOptions { MaxCalendarExceptionsSize = budget };
			var recurrence = CreateRecurrenceWithExceptions (200);

			using (var result = Convert (CreateRecurringMeeting (recurrence, false, body: body), options)) {
				Assert.That (result.Losses.Select (loss => loss.Kind), Is.EqualTo (new[] { TnefConversionLossKind.CalendarExceptionLimitExceeded }));

				var part = GetCalendarPart (result.Message);
				var lines = ReadCalendar (part);
				int events = lines.Count (line => line == "BEGIN:VEVENT");

				Assert.That (events, Is.GreaterThan (1));
				Assert.That (events, Is.LessThan (201));

				using (var memory = new MemoryStream ()) {
					part.Content.DecodeTo (memory);

					// The master event plus at most the budget.
					Assert.That (memory.Length, Is.LessThan (budget + 2 * (body.Length + 1024)));
				}
			}
		}

		[Test]
		public void TestControlCharactersCannotInjectContentLines ()
		{
			const string payload = "Sync\r\nEND:VEVENT\r\nBEGIN:VEVENT\rUID:evil\nX-EVIL:1\0\u0007\u001B";

			var properties = CreateAppointment (new DateTime (2024, 7, 1, 14, 0, 0, DateTimeKind.Utc), new DateTime (2024, 7, 1, 15, 0, 0, DateTimeKind.Utc), out var named, payload);

			named.String (PidLidLocation, payload);

			var builder = CreateMessage ("IPM.Schedule.Meeting.Request", properties,
				Recipient (TnefRecipientType.To, "Bob\r\nX-EVIL:1;\"x\":", "bob@example.com"));

			using (var result = Convert (builder)) {
				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines.Count (line => line == "BEGIN:VEVENT"), Is.EqualTo (1));
				Assert.That (lines.Count (line => line == "END:VEVENT"), Is.EqualTo (1));
				Assert.That (lines.Count (line => line.StartsWith ("UID:", StringComparison.Ordinal)), Is.EqualTo (1));
				Assert.That (lines.Any (line => line.StartsWith ("X-EVIL", StringComparison.Ordinal)), Is.False);
				Assert.That (lines.Any (line => line.Any (c => c < 0x20 && c != '\t')), Is.False);
			}
		}

		[Test]
		public void TestCalendarExceptionLimitOptionsValidation ()
		{
			var options = new TnefConversionOptions ();

			Assert.That (options.MaxCalendarExceptions, Is.EqualTo (TnefConversionOptions.DefaultMaxCalendarExceptions));
			Assert.That (options.MaxCalendarExceptionsSize, Is.EqualTo (TnefConversionOptions.DefaultMaxCalendarExceptionsSize));

			Assert.Throws<ArgumentOutOfRangeException> (() => options.MaxCalendarExceptions = -1);
			Assert.Throws<ArgumentOutOfRangeException> (() => options.MaxCalendarExceptionsSize = -1);

			options.MaxCalendarExceptions = 0;
			options.MaxCalendarExceptionsSize = 0;

			var clone = options.Clone ();

			Assert.That (clone.MaxCalendarExceptions, Is.EqualTo (0));
			Assert.That (clone.MaxCalendarExceptionsSize, Is.EqualTo (0));
		}
	}
}

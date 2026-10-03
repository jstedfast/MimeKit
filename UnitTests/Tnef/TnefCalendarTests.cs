//
// TnefCalendarTests.cs
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

namespace UnitTests.Tnef {
	[TestFixture]
	public class TnefCalendarTests
	{
		static readonly Guid IID_IMessage = new Guid ("00020307-0000-0000-C000-000000000046");
		static readonly DateTime Stamp = new DateTime (2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
		internal static readonly DateTime Epoch = new DateTime (1601, 1, 1);

		internal const string EasternId = "Eastern Standard Time";

		// The named property ids ([MS-OXPROPS]).
		const int PidLidAppointmentSequence = 0x8201;
		const int PidLidBusyStatus = 0x8205;
		internal const int PidLidLocation = 0x8208;
		internal const int PidLidAppointmentStartWhole = 0x820D;
		const int PidLidAppointmentEndWhole = 0x820E;
		const int PidLidAppointmentSubType = 0x8215;
		internal const int PidLidAppointmentRecur = 0x8216;
		const int PidLidAppointmentStateFlags = 0x8217;
		const int PidLidExceptionReplaceTime = 0x8228;
		const int PidLidAppointmentProposedStartWhole = 0x8250;
		const int PidLidAppointmentProposedEndWhole = 0x8251;
		const int PidLidAppointmentCounterProposal = 0x8257;
		internal const int PidLidAppointmentTimeZoneDefinitionStartDisplay = 0x825E;
		internal const int PidLidAppointmentTimeZoneDefinitionRecur = 0x8260;
		const int PidLidReminderDelta = 0x8501;
		const int PidLidReminderSet = 0x8503;
		internal const int PidLidGlobalObjectId = 0x0003;

		internal sealed class NamedProperties
		{
			readonly TnefMapiPropertyBuilder properties;
			int index;

			public NamedProperties (TnefMapiPropertyBuilder properties)
			{
				this.properties = properties;
			}

			void Header (TnefPropertyType type, Guid guid, int id)
			{
				var tag = new TnefPropertyTag ((TnefPropertyId) (0x8000 + index++), type);

				properties.WritePropertyHeader (tag, guid, nameId: id);
			}

			public void Time (int id, DateTime utc, Guid? guid = null)
			{
				Header (TnefPropertyType.SysTime, guid ?? TnefPropertySetGuid.Appointment, id);
				properties.WriteRaw (BitConverter.GetBytes (utc.ToFileTimeUtc ()));
			}

			public void Int32 (int id, int value, Guid? guid = null)
			{
				Header (TnefPropertyType.Long, guid ?? TnefPropertySetGuid.Appointment, id);
				properties.WriteRaw (BitConverter.GetBytes (value));
			}

			public void Boolean (int id, bool value, Guid? guid = null)
			{
				Header (TnefPropertyType.Boolean, guid ?? TnefPropertySetGuid.Appointment, id);
				properties.WriteRaw (BitConverter.GetBytes (value ? 1 : 0));
			}

			public void String (int id, string value, Guid? guid = null)
			{
				Header (TnefPropertyType.Unicode, guid ?? TnefPropertySetGuid.Appointment, id);
				properties.WriteValueCount (1);
				properties.WriteUnicodeValue (value);
			}

			public void Binary (int id, byte[] value, Guid? guid = null)
			{
				Header (TnefPropertyType.Binary, guid ?? TnefPropertySetGuid.Appointment, id);
				properties.WriteValueCount (1);
				properties.WriteVariableLengthValue (value);
			}
		}

		internal static TnefMapiPropertyBuilder CreateAppointment (DateTime start, DateTime end, out NamedProperties named, string subject = "Team sync")
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (TnefPropertyTag.SubjectW, subject);
			properties.WriteInt64Property (TnefPropertyTag.ClientSubmitTime, Stamp.ToFileTimeUtc ());

			named = new NamedProperties (properties);
			named.Time (PidLidAppointmentStartWhole, start);
			named.Time (PidLidAppointmentEndWhole, end);

			return properties;
		}

		internal static TnefBuilder CreateMessage (string messageClass, TnefMapiPropertyBuilder properties, params TnefMapiPropertyBuilder[] recipients)
		{
			var builder = new TnefBuilder ().WriteTnefVersion ().WriteMessageClass (messageClass);

			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			if (recipients.Length > 0)
				builder.WriteRecipientTable (recipients);

			return builder;
		}

		internal static TnefMapiPropertyBuilder Recipient (TnefRecipientType type, string name, string address, int? flags = null)
		{
			var row = new TnefMapiPropertyBuilder ();

			row.WriteInt32Property (TnefPropertyTag.RecipientType, (int) type);
			row.WriteStringProperty (TnefPropertyTag.DisplayNameW, name);
			row.WriteStringProperty (TnefPropertyTag.AddrtypeW, "SMTP");
			row.WriteStringProperty (TnefPropertyTag.EmailAddressW, address);

			if (flags.HasValue)
				row.WriteInt32Property (TnefPropertyTag.RecipientFlags, flags.Value);

			return row;
		}

		internal static TnefConversionResult Convert (TnefBuilder builder, TnefConversionOptions options = null)
		{
			using (var tnef = TnefMessage.Load (builder.ToStream ()))
				return tnef.ConvertToMime (options);
		}

		internal static TextPart GetCalendarPart (MimeMessage message)
		{
			foreach (var part in message.BodyParts) {
				if (part is TextPart text && text.ContentType.IsMimeType ("text", "calendar"))
					return text;
			}

			return null;
		}

		// Returns the content lines of the calendar, unfolded.
		internal static string[] ReadCalendar (TextPart part)
		{
			using (var memory = new MemoryStream ()) {
				part.Content.DecodeTo (memory);

				var text = Encoding.UTF8.GetString (memory.ToArray ());

				Assert.That (text, Does.EndWith ("\r\n"));
				Assert.That (text.Replace ("\r\n", string.Empty), Does.Not.Contain ("\n").And.Not.Contain ("\r"));

				foreach (var line in text.Split (new[] { "\r\n" }, StringSplitOptions.None))
					Assert.That (Encoding.UTF8.GetByteCount (line), Is.LessThanOrEqualTo (75), line);

				text = text.Replace ("\r\n ", string.Empty);

				return text.Substring (0, text.Length - 2).Split (new[] { "\r\n" }, StringSplitOptions.None);
			}
		}

		internal static string[] GetComponent (string[] lines, string name, int occurrence = 0)
		{
			int begin = -1;

			for (int i = 0; i < lines.Length; i++) {
				if (lines[i] == "BEGIN:" + name && occurrence-- == 0) {
					begin = i;
					break;
				}
			}

			Assert.That (begin, Is.Not.EqualTo (-1), $"No {name} component");

			int end = Array.IndexOf (lines, "END:" + name, begin);

			return lines.Skip (begin).Take (end - begin + 1).ToArray ();
		}

		internal static byte[] CreateGlobalObjectId (byte[] data = null, int year = 0, int month = 0, int day = 0)
		{
			data ??= Array.Empty<byte> ();

			var id = new byte[40 + data.Length];

			new byte[] { 0x04, 0x00, 0x00, 0x00, 0x82, 0x00, 0xE0, 0x00, 0x74, 0xC5, 0xB7, 0x10, 0x1A, 0x82, 0xE0, 0x08 }.CopyTo (id, 0);
			id[16] = (byte) (year >> 8);
			id[17] = (byte) year;
			id[18] = (byte) month;
			id[19] = (byte) day;

			for (int i = 0; i < 8; i++)
				id[20 + i] = (byte) (0x11 + i);

			BitConverter.GetBytes (data.Length).CopyTo (id, 36);
			data.CopyTo (id, 40);

			return id;
		}

		internal static byte[] SystemTime (int month, int dayOfWeek, int day, int hour)
		{
			var bytes = new byte[16];

			BitConverter.GetBytes ((ushort) month).CopyTo (bytes, 2);
			BitConverter.GetBytes ((ushort) dayOfWeek).CopyTo (bytes, 4);
			BitConverter.GetBytes ((ushort) day).CopyTo (bytes, 6);
			BitConverter.GetBytes ((ushort) hour).CopyTo (bytes, 8);

			return bytes;
		}

		// A TZDefinition ([MS-OXOCAL] 2.2.1.41) for US Eastern time.
		internal static byte[] CreateEasternTimeZoneDefinition (string keyName = EasternId)
		{
			using (var stream = new MemoryStream ())
			using (var writer = new BinaryWriter (stream)) {
				writer.Write ((byte) 0x02);
				writer.Write ((byte) 0x01);
				writer.Write ((ushort) (6 + keyName.Length * 2));
				writer.Write ((ushort) 0x0002);
				writer.Write ((ushort) keyName.Length);
				writer.Write (Encoding.Unicode.GetBytes (keyName));
				writer.Write ((ushort) 1);

				// TZRule
				writer.Write ((byte) 0x02);
				writer.Write ((byte) 0x01);
				writer.Write ((ushort) 0x003E);
				writer.Write ((ushort) 0x0002);
				writer.Write ((ushort) 2007);
				writer.Write (new byte[14]);
				writer.Write (300);
				writer.Write (0);
				writer.Write (-60);
				writer.Write (SystemTime (11, 0, 1, 2));
				writer.Write (SystemTime (3, 0, 2, 2));

				writer.Flush ();

				return stream.ToArray ();
			}
		}

		internal static uint ToMinutes (DateTime local)
		{
			return (uint) (local - Epoch).TotalMinutes;
		}

		// An AppointmentRecurrencePattern ([MS-OXOCAL] 2.2.1.44.5) for a weekly recurrence on Monday and Wednesday at
		// 10:00 for 10 occurrences, starting on 2024-07-01, with July 8 deleted and July 10 moved to 13:00 with a new
		// subject and location.
		internal static byte[] CreateWeeklyRecurrence (ushort calendarType = 0, ushort patternType = 1, bool truncateExceptions = false)
		{
			var startDate = new DateTime (2024, 7, 1);
			var original = new DateTime (2024, 7, 10, 10, 0, 0);
			var moved = new DateTime (2024, 7, 10, 13, 0, 0);

			using (var stream = new MemoryStream ())
			using (var writer = new BinaryWriter (stream)) {
				writer.Write ((ushort) 0x3004);
				writer.Write ((ushort) 0x3004);
				writer.Write ((ushort) 0x200B);
				writer.Write (patternType);
				writer.Write (calendarType);
				writer.Write (0u); // FirstDateTime
				writer.Write (1u); // Period
				writer.Write (0u); // SlidingFlag

				if (patternType == 1) {
					writer.Write (0x0Au); // Monday | Wednesday
				} else {
					writer.Write (10u); // DayOfMonth
				}

				writer.Write (0x2022u); // EndAfterOccurrences
				writer.Write (10u);
				writer.Write (0u); // FirstDOW

				writer.Write (2u);
				writer.Write (ToMinutes (new DateTime (2024, 7, 8)));
				writer.Write (ToMinutes (new DateTime (2024, 7, 10)));

				writer.Write (1u);
				writer.Write (ToMinutes (new DateTime (2024, 7, 10)));

				writer.Write (ToMinutes (startDate));
				writer.Write (ToMinutes (new DateTime (2024, 7, 31)));

				writer.Write (0x3006u); // ReaderVersion2
				writer.Write (0x3009u); // WriterVersion2
				writer.Write (600u); // StartTimeOffset
				writer.Write (660u); // EndTimeOffset

				// ExceptionInfo
				writer.Write ((ushort) 1);
				writer.Write (ToMinutes (moved));
				writer.Write (ToMinutes (moved.AddHours (1)));
				writer.Write (ToMinutes (original));
				writer.Write ((ushort) 0x0011); // Subject | Location
				writer.Write ((ushort) 11);
				writer.Write ((ushort) 10);
				writer.Write (Encoding.ASCII.GetBytes ("Moved sync"));

				if (truncateExceptions) {
					writer.Flush ();
					return stream.ToArray ();
				}

				writer.Write ((ushort) 7);
				writer.Write ((ushort) 6);
				writer.Write (Encoding.ASCII.GetBytes ("Room 2"));
				writer.Write (0u); // ReservedBlock1Size

				// ExtendedException
				writer.Write (4u); // ChangeHighlightSize
				writer.Write (0u); // ChangeHighlightValue
				writer.Write (0u); // ReservedBlockEE1Size
				writer.Write (ToMinutes (moved));
				writer.Write (ToMinutes (moved.AddHours (1)));
				writer.Write (ToMinutes (original));
				writer.Write ((ushort) 10);
				writer.Write (Encoding.Unicode.GetBytes ("Moved s\u00FDnc"));
				writer.Write ((ushort) 6);
				writer.Write (Encoding.Unicode.GetBytes ("Room 2"));
				writer.Write (0u); // ReservedBlockEE2Size
				writer.Write (0u); // ReservedBlock2Size

				writer.Flush ();

				return stream.ToArray ();
			}
		}

		internal static void AddExceptionAttachment (TnefBuilder builder, DateTime replaceTime, string body)
		{
			var embeddedProperties = new TnefMapiPropertyBuilder ();

			embeddedProperties.WriteStringProperty (TnefPropertyTag.BodyW, body);
			new NamedProperties (embeddedProperties).Time (PidLidExceptionReplaceTime, replaceTime);

			var embedded = new TnefBuilder ().WriteTnefVersion ().WriteMapiProperties (TnefAttributeLevel.Message, embeddedProperties).ToArray ();
			var value = new byte[16 + embedded.Length];

			IID_IMessage.ToByteArray ().CopyTo (value, 0);
			embedded.CopyTo (value, 16);

			var properties = new TnefMapiPropertyBuilder ();
			properties.WriteInt32Property (TnefPropertyTag.AttachMethod, (int) TnefAttachMethod.EmbeddedMessage);
			properties.WriteStringProperty (TnefPropertyTag.DisplayNameW, "Moved sync");
			properties.WriteInt32Property (TnefPropertyTag.AttachmentFlags, 0x02);
			properties.WriteBinaryProperty (TnefPropertyTag.AttachDataObj, value);

			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData, new byte[14]);
			builder.WriteMapiProperties (TnefAttributeLevel.Attachment, properties);
		}

		[Test]
		public void TestMeetingRequest ()
		{
			var properties = CreateAppointment (new DateTime (2024, 3, 1, 15, 0, 0, DateTimeKind.Utc), new DateTime (2024, 3, 1, 16, 0, 0, DateTimeKind.Utc), out var named);

			properties.WriteStringProperty (TnefPropertyTag.BodyW, "Agenda;\r\nItem 1, Item 2\r\n");
			properties.WriteInt32Property (TnefPropertyTag.ResponseRequested, 1);
			named.String (PidLidLocation, "Room 1");
			named.Int32 (PidLidBusyStatus, 2);
			named.Int32 (PidLidAppointmentSequence, 3);
			named.Boolean (PidLidReminderSet, true, TnefPropertySetGuid.Common);
			named.Int32 (PidLidReminderDelta, 10, TnefPropertySetGuid.Common);
			named.Binary (PidLidGlobalObjectId, CreateGlobalObjectId (year: 2024, month: 3, day: 1), TnefPropertySetGuid.Meeting);

			var builder = CreateMessage ("IPM.Schedule.Meeting.Request", properties,
				Recipient (TnefRecipientType.To, "Alice", "alice@example.com", 0x03),
				Recipient (TnefRecipientType.To, "Bob", "bob@example.com", 0x01),
				Recipient (TnefRecipientType.Cc, "Carol", "carol@example.com"),
				Recipient (TnefRecipientType.Bcc, "Room 1", "room1@example.com"),
				Recipient (TnefRecipientType.To, "Dave", "dave@example.com", 0x21));

			using (var result = Convert (builder)) {
				Assert.That (result.Losses, Is.Empty);

				var alternative = (MultipartAlternative) result.Message.Body;

				Assert.That (alternative.Count, Is.EqualTo (2));
				Assert.That (alternative[0].ContentType.MimeType, Is.EqualTo ("text/plain"));
				Assert.That (alternative[1].ContentType.MimeType, Is.EqualTo ("text/calendar"));

				var part = (TextPart) alternative[1];

				Assert.That (part.ContentType.Charset, Is.EqualTo ("utf-8"));
				Assert.That (part.ContentType.Parameters["method"], Is.EqualTo ("REQUEST"));

				var expected = new[] {
					"BEGIN:VCALENDAR",
					"METHOD:REQUEST",
					"PRODID:-//.NET Foundation//MimeKit//EN",
					"VERSION:2.0",
					"X-MS-OLK-FORCEINSPECTOROPEN:TRUE",
					"BEGIN:VEVENT",
					"ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=TRUE;CN=Bob:mailto:bob@example.com",
					"ATTENDEE;ROLE=OPT-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=TRUE;CN=Carol:mailto:carol@example.com",
					"ATTENDEE;CUTYPE=RESOURCE;ROLE=NON-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=TRUE;CN=Room 1:mailto:room1@example.com",
					"DESCRIPTION:Agenda\\;\\nItem 1\\, Item 2",
					"DTEND:20240301T160000Z",
					"DTSTAMP:20240102T030405Z",
					"DTSTART:20240301T150000Z",
					"LOCATION:Room 1",
					"ORGANIZER;CN=Alice:mailto:alice@example.com",
					"SEQUENCE:3",
					"SUMMARY:Team sync",
					"TRANSP:OPAQUE",
					"UID:040000008200E00074C5B7101A82E008000000001112131415161718000000000000000000000000",
					"X-MICROSOFT-CDO-BUSYSTATUS:BUSY",
					"BEGIN:VALARM",
					"ACTION:DISPLAY",
					"DESCRIPTION:Reminder",
					"TRIGGER:-PT10M",
					"END:VALARM",
					"END:VEVENT",
					"END:VCALENDAR"
				};

				Assert.That (ReadCalendar (part), Is.EqualTo (expected));
			}
		}

		[Test]
		public void TestCalendarDisabled ()
		{
			var properties = CreateAppointment (new DateTime (2024, 3, 1, 15, 0, 0, DateTimeKind.Utc), new DateTime (2024, 3, 1, 16, 0, 0, DateTimeKind.Utc), out _);

			properties.WriteStringProperty (TnefPropertyTag.BodyW, "Hello");

			var builder = CreateMessage ("IPM.Schedule.Meeting.Request", properties);

			using (var result = Convert (builder, new TnefConversionOptions { GenerateCalendar = false })) {
				Assert.That (result.Message.Body, Is.InstanceOf<TextPart> ());
				Assert.That (result.Message.Body.ContentType.MimeType, Is.EqualTo ("text/plain"));
			}
		}

		[Test]
		public void TestNonCalendarMessageClass ()
		{
			var properties = CreateAppointment (new DateTime (2024, 3, 1, 15, 0, 0, DateTimeKind.Utc), new DateTime (2024, 3, 1, 16, 0, 0, DateTimeKind.Utc), out _);

			properties.WriteStringProperty (TnefPropertyTag.BodyW, "Hello");

			// IPM.AppointmentX is not a subclass of IPM.Appointment.
			foreach (var messageClass in new[] { "IPM.Note", "IPM.AppointmentX", "IPM.Schedule.Meeting.Notification.Forward" }) {
				using (var result = Convert (CreateMessage (messageClass, properties))) {
					Assert.That (GetCalendarPart (result.Message), Is.Null, messageClass);
					Assert.That (result.Losses, Is.Empty, messageClass);
				}
			}
		}

		[Test]
		public void TestCalendarWithoutBody ()
		{
			var properties = CreateAppointment (new DateTime (2024, 3, 1, 15, 0, 0, DateTimeKind.Utc), new DateTime (2024, 3, 1, 16, 0, 0, DateTimeKind.Utc), out _);

			using (var result = Convert (CreateMessage ("IPM.Appointment.Custom", properties))) {
				Assert.That (result.Losses, Is.Empty);
				Assert.That (result.Message.Body, Is.InstanceOf<TextPart> ());

				var lines = ReadCalendar ((TextPart) result.Message.Body);

				Assert.That (lines, Does.Contain ("METHOD:PUBLISH"));
				Assert.That (result.Message.Body.ContentType.Parameters["method"], Is.EqualTo ("PUBLISH"));
				Assert.That (lines.Any (line => line.StartsWith ("ATTENDEE", StringComparison.Ordinal)), Is.False);
			}
		}

		[Test]
		public void TestMissingStartTime ()
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (TnefPropertyTag.BodyW, "Hello");

			using (var result = Convert (CreateMessage ("IPM.Schedule.Meeting.Request", properties))) {
				Assert.That (GetCalendarPart (result.Message), Is.Null);
				Assert.That (result.Losses.Select (loss => loss.Kind), Is.EqualTo (new[] { TnefConversionLossKind.InvalidCalendarData }));
			}
		}

		[Test]
		public void TestGeneratedUidIsStable ()
		{
			string GetUidLine (DateTime start)
			{
				var properties = CreateAppointment (start, start.AddHours (1), out _);

				using (var result = Convert (CreateMessage ("IPM.Appointment", properties)))
					return ReadCalendar (GetCalendarPart (result.Message)).Single (line => line.StartsWith ("UID:", StringComparison.Ordinal));
			}

			var first = GetUidLine (new DateTime (2024, 3, 1, 15, 0, 0, DateTimeKind.Utc));
			var second = GetUidLine (new DateTime (2024, 3, 1, 15, 0, 0, DateTimeKind.Utc));
			var other = GetUidLine (new DateTime (2024, 3, 2, 15, 0, 0, DateTimeKind.Utc));

			Assert.That (Guid.TryParse (first.Substring (4), out _), Is.True, first);
			Assert.That (second, Is.EqualTo (first), "The same appointment should always get the same UID.");
			Assert.That (other, Is.Not.EqualTo (first), "A different appointment should get a different UID.");
		}

		[Test]
		public void TestVCalUid ()
		{
			var properties = CreateAppointment (new DateTime (2024, 3, 1, 15, 0, 0, DateTimeKind.Utc), new DateTime (2024, 3, 1, 16, 0, 0, DateTimeKind.Utc), out var named);
			var data = Encoding.ASCII.GetBytes ("vCal-Uid\u0001\0\0\0abc-123@example.com\0");

			named.Binary (PidLidGlobalObjectId, CreateGlobalObjectId (data), TnefPropertySetGuid.Meeting);

			using (var result = Convert (CreateMessage ("IPM.Schedule.Meeting.Request", properties))) {
				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines, Does.Contain ("UID:abc-123@example.com"));
			}
		}

		[Test]
		public void TestMeetingResponse ()
		{
			var properties = CreateAppointment (new DateTime (2024, 3, 1, 15, 0, 0, DateTimeKind.Utc), new DateTime (2024, 3, 1, 16, 0, 0, DateTimeKind.Utc), out _);

			properties.WriteStringProperty (TnefPropertyTag.BodyW, "See you there");
			properties.WriteStringProperty (TnefPropertyTag.SentRepresentingNameW, "Bob");
			properties.WriteStringProperty (TnefPropertyTag.SentRepresentingAddrtypeW, "SMTP");
			properties.WriteStringProperty (TnefPropertyTag.SentRepresentingEmailAddressW, "bob@example.com");

			var expectations = new[] {
				("IPM.Schedule.Meeting.Resp.Pos", "ACCEPTED"),
				("IPM.Schedule.Meeting.Resp.Tent", "TENTATIVE"),
				("IPM.Schedule.Meeting.Resp.Neg", "DECLINED")
			};

			foreach (var (messageClass, status) in expectations) {
				var builder = CreateMessage (messageClass, properties, Recipient (TnefRecipientType.To, "Alice", "alice@example.com"));

				using (var result = Convert (builder)) {
					var part = GetCalendarPart (result.Message);
					var lines = ReadCalendar (part);

					Assert.That (part.ContentType.Parameters["method"], Is.EqualTo ("REPLY"));
					Assert.That (lines, Does.Contain ("METHOD:REPLY"));
					Assert.That (lines, Does.Contain ("ATTENDEE;PARTSTAT=" + status + ";CN=Bob:mailto:bob@example.com"));
					Assert.That (lines, Does.Contain ("ORGANIZER;CN=Alice:mailto:alice@example.com"));
					Assert.That (lines, Does.Contain ("COMMENT:See you there"));
					Assert.That (lines.Any (line => line.StartsWith ("DESCRIPTION:See", StringComparison.Ordinal)), Is.False);
				}
			}
		}

		[Test]
		public void TestCounterProposal ()
		{
			var properties = CreateAppointment (new DateTime (2024, 3, 1, 15, 0, 0, DateTimeKind.Utc), new DateTime (2024, 3, 1, 16, 0, 0, DateTimeKind.Utc), out var named);

			named.Boolean (PidLidAppointmentCounterProposal, true);
			named.Time (PidLidAppointmentProposedStartWhole, new DateTime (2024, 3, 2, 15, 0, 0, DateTimeKind.Utc));
			named.Time (PidLidAppointmentProposedEndWhole, new DateTime (2024, 3, 2, 16, 30, 0, DateTimeKind.Utc));

			using (var result = Convert (CreateMessage ("IPM.Schedule.Meeting.Resp.Tent", properties))) {
				var part = GetCalendarPart (result.Message);
				var lines = ReadCalendar (part);

				Assert.That (part.ContentType.Parameters["method"], Is.EqualTo ("COUNTER"));
				Assert.That (lines, Does.Contain ("METHOD:COUNTER"));
				Assert.That (lines, Does.Contain ("DTSTART:20240302T150000Z"));
				Assert.That (lines, Does.Contain ("DTEND:20240302T163000Z"));
				Assert.That (lines, Does.Contain ("X-MS-OLK-ORIGINALSTART:20240301T150000Z"));
				Assert.That (lines, Does.Contain ("X-MS-OLK-ORIGINALEND:20240301T160000Z"));
			}
		}

		[Test]
		public void TestMeetingCancellation ()
		{
			var properties = CreateAppointment (new DateTime (2024, 3, 1, 15, 0, 0, DateTimeKind.Utc), new DateTime (2024, 3, 1, 16, 0, 0, DateTimeKind.Utc), out _);

			using (var result = Convert (CreateMessage ("IPM.Schedule.Meeting.Canceled", properties))) {
				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines, Does.Contain ("METHOD:CANCEL"));
				Assert.That (lines, Does.Contain ("STATUS:CANCELLED"));
			}
		}

		[Test]
		public void TestStartDisplayTimeZone ()
		{
			var properties = CreateAppointment (new DateTime (2024, 7, 1, 14, 0, 0, DateTimeKind.Utc), new DateTime (2024, 12, 2, 15, 0, 0, DateTimeKind.Utc), out var named);

			named.Binary (PidLidAppointmentTimeZoneDefinitionStartDisplay, CreateEasternTimeZoneDefinition ());

			using (var result = Convert (CreateMessage ("IPM.Appointment", properties))) {
				Assert.That (result.Losses, Is.Empty);

				var lines = ReadCalendar (GetCalendarPart (result.Message));

				// July is in daylight saving time and December is not.
				Assert.That (lines, Does.Contain ("DTSTART;TZID=Eastern Standard Time:20240701T100000"));
				Assert.That (lines, Does.Contain ("DTEND;TZID=Eastern Standard Time:20241202T100000"));

				var zone = GetComponent (lines, "VTIMEZONE");
				var expected = new[] {
					"BEGIN:VTIMEZONE",
					"TZID:Eastern Standard Time",
					"BEGIN:STANDARD",
					"DTSTART:16011104T020000",
					"RRULE:FREQ=YEARLY;BYDAY=1SU;BYMONTH=11",
					"TZOFFSETFROM:-0400",
					"TZOFFSETTO:-0500",
					"END:STANDARD",
					"BEGIN:DAYLIGHT",
					"DTSTART:16010311T020000",
					"RRULE:FREQ=YEARLY;BYDAY=2SU;BYMONTH=3",
					"TZOFFSETFROM:-0500",
					"TZOFFSETTO:-0400",
					"END:DAYLIGHT",
					"END:VTIMEZONE"
				};

				Assert.That (zone, Is.EqualTo (expected));
				Assert.That (lines.Count (line => line == "BEGIN:VTIMEZONE"), Is.EqualTo (1));
			}
		}

		[Test]
		public void TestAllDayEvent ()
		{
			var properties = CreateAppointment (new DateTime (2024, 7, 4, 4, 0, 0, DateTimeKind.Utc), new DateTime (2024, 7, 5, 4, 0, 0, DateTimeKind.Utc), out var named);

			named.Boolean (PidLidAppointmentSubType, true);
			named.Binary (PidLidAppointmentTimeZoneDefinitionStartDisplay, CreateEasternTimeZoneDefinition ());

			using (var result = Convert (CreateMessage ("IPM.Appointment", properties))) {
				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines, Does.Contain ("DTSTART;VALUE=DATE:20240704"));
				Assert.That (lines, Does.Contain ("DTEND;VALUE=DATE:20240705"));
				Assert.That (lines, Does.Not.Contain ("BEGIN:VTIMEZONE"));
			}
		}

		internal static TnefBuilder CreateRecurringMeeting (byte[] recurrence, bool addException = true, byte[] timeZone = null, string body = null)
		{
			var properties = CreateAppointment (new DateTime (2024, 7, 1, 14, 0, 0, DateTimeKind.Utc), new DateTime (2024, 7, 1, 15, 0, 0, DateTimeKind.Utc), out var named);

			if (body != null)
				properties.WriteStringProperty (TnefPropertyTag.BodyW, body);

			named.Binary (PidLidAppointmentRecur, recurrence);
			named.Binary (PidLidAppointmentTimeZoneDefinitionRecur, timeZone ?? CreateEasternTimeZoneDefinition ());
			named.Int32 (PidLidAppointmentStateFlags, 1);
			named.String (PidLidLocation, "Room 1");

			var builder = CreateMessage ("IPM.Schedule.Meeting.Request", properties,
				Recipient (TnefRecipientType.To, "Alice", "alice@example.com", 0x03),
				Recipient (TnefRecipientType.To, "Bob", "bob@example.com", 0x01));

			if (addException)
				AddExceptionAttachment (builder, new DateTime (2024, 7, 10, 14, 0, 0, DateTimeKind.Utc), "Exception body");

			return builder;
		}

		[Test]
		public void TestRecurringMeetingWithException ()
		{
			using (var result = Convert (CreateRecurringMeeting (CreateWeeklyRecurrence ()))) {
				Assert.That (result.Losses, Is.Empty);

				// The exception attachment is part of the calendar, so the calendar is the only part.
				Assert.That (result.Message.Body, Is.InstanceOf<TextPart> ());
				Assert.That (result.Message.Attachments, Is.Empty);

				var lines = ReadCalendar ((TextPart) result.Message.Body);
				var master = GetComponent (lines, "VEVENT", 0);
				var exception = GetComponent (lines, "VEVENT", 1);

				Assert.That (lines.Count (line => line == "BEGIN:VEVENT"), Is.EqualTo (2));

				Assert.That (master, Does.Contain ("DTSTART;TZID=Eastern Standard Time:20240701T100000"));
				Assert.That (master, Does.Contain ("DTEND;TZID=Eastern Standard Time:20240701T110000"));
				Assert.That (master, Does.Contain ("RRULE:FREQ=WEEKLY;BYDAY=MO,WE;COUNT=10"));
				Assert.That (master, Does.Contain ("EXDATE;TZID=Eastern Standard Time:20240708T100000"));
				Assert.That (master.Count (line => line.StartsWith ("EXDATE", StringComparison.Ordinal)), Is.EqualTo (1));
				Assert.That (master, Does.Contain ("SUMMARY:Team sync"));
				Assert.That (master, Does.Contain ("LOCATION:Room 1"));
				Assert.That (master.Any (line => line.StartsWith ("RECURRENCE-ID", StringComparison.Ordinal)), Is.False);

				Assert.That (exception, Does.Contain ("RECURRENCE-ID;TZID=Eastern Standard Time:20240710T100000"));
				Assert.That (exception, Does.Contain ("DTSTART;TZID=Eastern Standard Time:20240710T130000"));
				Assert.That (exception, Does.Contain ("DTEND;TZID=Eastern Standard Time:20240710T140000"));
				Assert.That (exception, Does.Contain ("SUMMARY:Moved s\u00FDnc"));
				Assert.That (exception, Does.Contain ("LOCATION:Room 2"));
				Assert.That (exception, Does.Contain ("DESCRIPTION:Exception body"));
				Assert.That (exception, Does.Contain ("ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;CN=Bob:mailto:bob@example.com"));
				Assert.That (exception.Any (line => line.StartsWith ("RRULE", StringComparison.Ordinal) || line.StartsWith ("EXDATE", StringComparison.Ordinal)), Is.False);

				// Both events share the UID.
				var uid = master.Single (line => line.StartsWith ("UID:", StringComparison.Ordinal));
				Assert.That (exception, Does.Contain (uid));
			}
		}

		[Test]
		public void TestUnmatchedExceptionAttachmentIsKept ()
		{
			var builder = CreateRecurringMeeting (CreateWeeklyRecurrence (), false);

			AddExceptionAttachment (builder, new DateTime (2024, 7, 17, 14, 0, 0, DateTimeKind.Utc), "Unrelated");

			using (var result = Convert (builder)) {
				var mixed = (Multipart) result.Message.Body;

				Assert.That (mixed.ContentType.MimeType, Is.EqualTo ("multipart/mixed"));
				Assert.That (mixed.Count, Is.EqualTo (2));
				Assert.That (mixed[0].ContentType.MimeType, Is.EqualTo ("text/calendar"));
			}
		}

		[Test]
		public void TestTruncatedExceptions ()
		{
			using (var result = Convert (CreateRecurringMeeting (CreateWeeklyRecurrence (truncateExceptions: true), false))) {
				Assert.That (result.Losses.Select (loss => loss.Kind), Is.EqualTo (new[] { TnefConversionLossKind.InvalidCalendarData }));

				var lines = ReadCalendar (GetCalendarPart (result.Message));

				// The recurrence is still exported, but without the exceptions.
				Assert.That (lines, Does.Contain ("RRULE:FREQ=WEEKLY;BYDAY=MO,WE;COUNT=10"));
				Assert.That (lines.Count (line => line == "BEGIN:VEVENT"), Is.EqualTo (1));
			}
		}

		[Test]
		public void TestMalformedRecurrence ()
		{
			using (var result = Convert (CreateRecurringMeeting (new byte[] { 0x04, 0x30, 0x04, 0x30, 0x0B }, false))) {
				Assert.That (result.Losses.Select (loss => loss.Kind), Is.EqualTo (new[] { TnefConversionLossKind.InvalidCalendarData }));

				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines.Any (line => line.StartsWith ("RRULE", StringComparison.Ordinal)), Is.False);
				Assert.That (lines, Does.Contain ("DTSTART:20240701T140000Z"));
			}
		}

		[Test]
		public void TestNonGregorianRecurrence ()
		{
			// A Hijri monthly pattern.
			using (var result = Convert (CreateRecurringMeeting (CreateWeeklyRecurrence (calendarType: 0x06, patternType: 0x0A), false))) {
				Assert.That (result.Losses.Select (loss => loss.Kind), Is.EqualTo (new[] { TnefConversionLossKind.UnsupportedCalendarData }));

				var lines = ReadCalendar (GetCalendarPart (result.Message));
				var events = lines.Count (line => line == "BEGIN:VEVENT");
				var master = GetComponent (lines, "VEVENT");

				// The VTIMEZONE has RRULEs of its own, so only look at the event.
				Assert.That (events, Is.EqualTo (1));
				Assert.That (master.Any (line => line.StartsWith ("RRULE", StringComparison.Ordinal)), Is.False);
			}
		}

		[Test]
		public void TestMonthlyRecurrence ()
		{
			using (var result = Convert (CreateRecurringMeeting (CreateWeeklyRecurrence (patternType: 0x02), false))) {
				Assert.That (result.Losses, Is.Empty);

				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines, Does.Contain ("RRULE:FREQ=MONTHLY;BYMONTHDAY=10;COUNT=10"));
			}
		}
	}
}

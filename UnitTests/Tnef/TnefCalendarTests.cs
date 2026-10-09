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

			public void Strings (string name, Guid guid, params string[] values)
			{
				var tag = new TnefPropertyTag ((TnefPropertyId) (0x8000 + index++), TnefPropertyType.Unicode | TnefPropertyType.MultiValued);

				properties.WritePropertyHeader (tag, guid, name: name);
				properties.WriteValueCount (values.Length);

				foreach (var value in values)
					properties.WriteUnicodeValue (value);
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
		internal static byte[] CreateEasternTimeZoneDefinition (string keyName = EasternId, int bias = 300)
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
				writer.Write (bias);
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

		internal static TnefBuilder CreateRecurringMeeting (byte[] recurrence, bool addException = true, byte[] timeZone = null, string body = null, Action<NamedProperties> configure = null)
		{
			var properties = CreateAppointment (new DateTime (2024, 7, 1, 14, 0, 0, DateTimeKind.Utc), new DateTime (2024, 7, 1, 15, 0, 0, DateTimeKind.Utc), out var named);

			if (body != null)
				properties.WriteStringProperty (TnefPropertyTag.BodyW, body);

			timeZone ??= CreateEasternTimeZoneDefinition ();

			named.Binary (PidLidAppointmentRecur, recurrence);

			if (timeZone.Length > 0)
				named.Binary (PidLidAppointmentTimeZoneDefinitionRecur, timeZone);

			named.Int32 (PidLidAppointmentStateFlags, 1);
			named.String (PidLidLocation, "Room 1");
			configure?.Invoke (named);

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

		const int PidLidAppointmentDuration = 0x8213;
		const int PidLidIntendedBusyStatus = 0x8224;
		const int PidLidTimeZoneStruct = 0x8233;
		const int PidLidTimeZoneDescription = 0x8234;
		const int PidLidAppointmentTimeZoneDefinitionEndDisplay = 0x825F;
		const int PidLidPrivate = 0x8506;
		const int PidLidIsException = 0x000A;
		const int PidLidStartRecurrenceTime = 0x000E;

		static readonly uint StartDate = ToMinutes (new DateTime (2024, 7, 1));

		// An AppointmentRecurrencePattern ([MS-OXOCAL] 2.2.1.44.5) with the specified pattern, starting on 2024-07-01
		// at 10:00 (local time) and, unless otherwise specified, ending after 10 occurrences.
		static byte[] CreateRecurrence (ushort patternType, uint period, uint[] patternSpecific, ushort calendarType = 0, uint endType = 0x2022, uint firstDayOfWeek = 0, Action<BinaryWriter> writeExceptions = null)
		{
			using (var stream = new MemoryStream ())
			using (var writer = new BinaryWriter (stream)) {
				writer.Write ((ushort) 0x3004);
				writer.Write ((ushort) 0x3004);
				writer.Write ((ushort) 0x200B);
				writer.Write (patternType);
				writer.Write (calendarType);
				writer.Write (0u); // FirstDateTime
				writer.Write (period);
				writer.Write (0u); // SlidingFlag

				foreach (var value in patternSpecific)
					writer.Write (value);

				writer.Write (endType);
				writer.Write (10u); // OccurrenceCount
				writer.Write (firstDayOfWeek);
				writer.Write (0u); // DeletedInstanceCount
				writer.Write (0u); // ModifiedInstanceCount
				writer.Write (StartDate);
				writer.Write (ToMinutes (new DateTime (2024, 7, 31)));
				writer.Write (0x3006u); // ReaderVersion2
				writer.Write (0x3009u); // WriterVersion2
				writer.Write (600u); // StartTimeOffset
				writer.Write (660u); // EndTimeOffset

				if (writeExceptions != null) {
					writeExceptions (writer);
				} else {
					writer.Write ((ushort) 0); // ExceptionCount
					writer.Write (0u); // ReservedBlock1Size
				}

				writer.Flush ();

				return stream.ToArray ();
			}
		}

		static string[] GetMasterEvent (TnefConversionResult result)
		{
			return GetComponent (ReadCalendar (GetCalendarPart (result.Message)), "VEVENT");
		}

		static string GetRecurrenceRule (TnefConversionResult result)
		{
			return GetMasterEvent (result).SingleOrDefault (line => line.StartsWith ("RRULE:", StringComparison.Ordinal));
		}

		[TestCase ((ushort) 0x0000, 2880u, new uint[0], 0u, ExpectedResult = "RRULE:FREQ=DAILY;INTERVAL=2;COUNT=10")]
		[TestCase ((ushort) 0x0001, 2u, new uint[] { 0x02 }, 1u, ExpectedResult = "RRULE:FREQ=WEEKLY;BYDAY=MO;INTERVAL=2;COUNT=10;WKST=MO")]
		[TestCase ((ushort) 0x0001, 2u, new uint[] { 0x02 }, 7u, ExpectedResult = "RRULE:FREQ=WEEKLY;BYDAY=MO;INTERVAL=2;COUNT=10")]
		[TestCase ((ushort) 0x0002, 12u, new uint[] { 10 }, 0u, ExpectedResult = "RRULE:FREQ=YEARLY;BYMONTHDAY=10;BYMONTH=7;COUNT=10")]
		[TestCase ((ushort) 0x0002, 24u, new uint[] { 10 }, 0u, ExpectedResult = "RRULE:FREQ=YEARLY;BYMONTHDAY=10;BYMONTH=7;INTERVAL=2;COUNT=10")]
		[TestCase ((ushort) 0x0002, 1u, new uint[] { 31 }, 0u, ExpectedResult = "RRULE:FREQ=MONTHLY;BYMONTHDAY=-1;COUNT=10")]
		[TestCase ((ushort) 0x0004, 1u, new uint[] { 0 }, 0u, ExpectedResult = "RRULE:FREQ=MONTHLY;BYMONTHDAY=-1;COUNT=10")]
		[TestCase ((ushort) 0x0003, 1u, new uint[] { 0x02, 5 }, 0u, ExpectedResult = "RRULE:FREQ=MONTHLY;BYDAY=MO;BYSETPOS=-1;COUNT=10")]
		[TestCase ((ushort) 0x0003, 2u, new uint[] { 0x14, 2 }, 0u, ExpectedResult = "RRULE:FREQ=MONTHLY;BYDAY=TU,TH;BYSETPOS=2;INTERVAL=2;COUNT=10")]
		[TestCase ((ushort) 0x0003, 12u, new uint[] { 0x02, 1 }, 0u, ExpectedResult = "RRULE:FREQ=YEARLY;BYDAY=MO;BYMONTH=7;BYSETPOS=1;COUNT=10")]
		public string TestRecurrenceRule (ushort patternType, uint period, uint[] patternSpecific, uint firstDayOfWeek)
		{
			var recurrence = CreateRecurrence (patternType, period, patternSpecific, firstDayOfWeek: firstDayOfWeek);

			using (var result = Convert (CreateRecurringMeeting (recurrence, false))) {
				Assert.That (result.Losses, Is.Empty);

				return GetRecurrenceRule (result);
			}
		}

		[TestCase ((ushort) 0x000A, (ushort) 0x0001, new uint[] { 10 }, ExpectedResult = "RRULE:FREQ=MONTHLY;BYMONTHDAY=10;COUNT=10")]
		[TestCase ((ushort) 0x000B, (ushort) 0x0009, new uint[] { 0x02, 5 }, ExpectedResult = "RRULE:FREQ=MONTHLY;BYDAY=MO;BYSETPOS=-1;COUNT=10")]
		[TestCase ((ushort) 0x000C, (ushort) 0x000C, new uint[] { 0 }, ExpectedResult = "RRULE:FREQ=MONTHLY;BYMONTHDAY=-1;COUNT=10")]
		public string TestHijriPatternWithGregorianCalendar (ushort patternType, ushort calendarType, uint[] patternSpecific)
		{
			// [MS-OXCICAL] 2.1.3.2.1: the Hijri patterns are the same as the Gregorian patterns when the calendar is Gregorian.
			var recurrence = CreateRecurrence (patternType, 1, patternSpecific, calendarType);

			using (var result = Convert (CreateRecurringMeeting (recurrence, false))) {
				Assert.That (result.Losses, Is.Empty);

				return GetRecurrenceRule (result);
			}
		}

		[TestCase ((ushort) 0x000A, (ushort) 0x0000, new uint[] { 10 }, Description = "Hijri pattern with the default calendar")]
		[TestCase ((ushort) 0x0001, (ushort) 0x0006, new uint[] { 0x02 }, Description = "Weekly pattern with the Hijri calendar")]
		[TestCase ((ushort) 0x0002, (ushort) 0x000E, new uint[] { 10 }, Description = "Monthly pattern with the lunar calendar")]
		public void TestUnsupportedRecurrenceCalendar (ushort patternType, ushort calendarType, uint[] patternSpecific)
		{
			var recurrence = CreateRecurrence (patternType, 1, patternSpecific, calendarType);

			using (var result = Convert (CreateRecurringMeeting (recurrence, false))) {
				Assert.That (result.Losses.Select (loss => loss.Kind), Is.EqualTo (new[] { TnefConversionLossKind.UnsupportedCalendarData }));
				Assert.That (GetRecurrenceRule (result), Is.Null);
			}
		}

		[TestCase ((ushort) 0x0000, 0u, new uint[0], Description = "Daily with a zero period")]
		[TestCase ((ushort) 0x0000, 1000u, new uint[0], Description = "Daily with a period that is not a whole number of days")]
		[TestCase ((ushort) 0x0001, 1u, new uint[] { 0x80 }, Description = "Weekly without any days")]
		[TestCase ((ushort) 0x0001, 0u, new uint[] { 0x02 }, Description = "Weekly with a zero period")]
		[TestCase ((ushort) 0x0002, 0u, new uint[] { 10 }, Description = "Monthly with a zero period")]
		[TestCase ((ushort) 0x0002, 1u, new uint[] { 0 }, Description = "Monthly on day 0")]
		[TestCase ((ushort) 0x0002, 1u, new uint[] { 32 }, Description = "Monthly on day 32")]
		[TestCase ((ushort) 0x0003, 1u, new uint[] { 0, 1 }, Description = "MonthNth without any days")]
		[TestCase ((ushort) 0x0003, 1u, new uint[] { 0x02, 0 }, Description = "MonthNth in week 0")]
		[TestCase ((ushort) 0x0003, 1u, new uint[] { 0x02, 6 }, Description = "MonthNth in week 6")]
		public void TestInvalidRecurrencePattern (ushort patternType, uint period, uint[] patternSpecific)
		{
			var recurrence = CreateRecurrence (patternType, period, patternSpecific);

			using (var result = Convert (CreateRecurringMeeting (recurrence, false))) {
				Assert.That (result.Losses.Select (loss => loss.Kind), Is.EqualTo (new[] { TnefConversionLossKind.InvalidCalendarData }));
				Assert.That (result.Losses[0].Description, Does.Contain ("The recurrence pattern is not valid"));
				Assert.That (GetRecurrenceRule (result), Is.Null);
			}
		}

		[Test]
		public void TestRecurrenceEndAfterDate ()
		{
			var recurrence = CreateRecurrence (0x0000, 1440, Array.Empty<uint> (), endType: 0x2021);

			using (var result = Convert (CreateRecurringMeeting (recurrence, false))) {
				Assert.That (result.Losses, Is.Empty);

				// UNTIL is the start of the last instance (10:00 EDT) in UTC.
				Assert.That (GetRecurrenceRule (result), Is.EqualTo ("RRULE:FREQ=DAILY;UNTIL=20240731T140000Z"));
			}
		}

		[Test]
		public void TestRecurrenceNeverEnds ()
		{
			var recurrence = CreateRecurrence (0x0000, 1440, Array.Empty<uint> (), endType: 0x2023);

			using (var result = Convert (CreateRecurringMeeting (recurrence, false))) {
				Assert.That (result.Losses, Is.Empty);
				Assert.That (GetRecurrenceRule (result), Is.EqualTo ("RRULE:FREQ=DAILY"));
			}
		}

		[Test]
		public void TestAllDayRecurrence ()
		{
			var builder = CreateRecurringMeeting (CreateWeeklyRecurrence (), configure: named => named.Boolean (PidLidAppointmentSubType, true));

			using (var result = Convert (builder)) {
				Assert.That (result.Losses, Is.Empty);

				var lines = ReadCalendar (GetCalendarPart (result.Message));
				var master = GetComponent (lines, "VEVENT", 0);
				var exception = GetComponent (lines, "VEVENT", 1);

				Assert.That (master, Does.Contain ("DTSTART;VALUE=DATE:20240701"));
				Assert.That (master, Does.Contain ("RRULE:FREQ=WEEKLY;BYDAY=MO,WE;COUNT=10"));
				Assert.That (master, Does.Contain ("EXDATE;VALUE=DATE:20240708"));
				Assert.That (exception, Does.Contain ("RECURRENCE-ID;VALUE=DATE:20240710"));
				Assert.That (exception, Does.Contain ("DTSTART;VALUE=DATE:20240710"));
			}

			var untilRecurrence = CreateRecurrence (0x0000, 1440, Array.Empty<uint> (), endType: 0x2021);

			builder = CreateRecurringMeeting (untilRecurrence, false, configure: named => named.Boolean (PidLidAppointmentSubType, true));

			using (var result = Convert (builder))
				Assert.That (GetRecurrenceRule (result), Is.EqualTo ("RRULE:FREQ=DAILY;UNTIL=20240731"));
		}

		// A PidLidTimeZoneStruct ([MS-OXOCAL] 2.2.1.39) for US Eastern time.
		static byte[] CreateEasternTimeZoneStruct ()
		{
			var structure = new byte[48];

			BitConverter.GetBytes (300).CopyTo (structure, 0);
			BitConverter.GetBytes (0).CopyTo (structure, 4);
			BitConverter.GetBytes (-60).CopyTo (structure, 8);
			SystemTime (11, 0, 1, 2).CopyTo (structure, 14);
			SystemTime (3, 0, 2, 2).CopyTo (structure, 32);

			return structure;
		}

		// A TZID parameter that contains a ':' must be quoted.
		[TestCase (null, "UTC-05:00", "\"UTC-05:00\"")]
		[TestCase (" ", "UTC-05:00", "\"UTC-05:00\"")]
		[TestCase ("Eastern Time ", "Eastern Time", "Eastern Time")]
		public void TestRecurrenceTimeZoneStruct (string description, string expectedId, string expectedParam)
		{
			var builder = CreateRecurringMeeting (CreateRecurrence (0x0000, 1440, Array.Empty<uint> ()), false, Array.Empty<byte> (), configure: named => {
				named.Binary (PidLidTimeZoneStruct, CreateEasternTimeZoneStruct ());

				if (description != null)
					named.String (PidLidTimeZoneDescription, description);
			});

			using (var result = Convert (builder)) {
				Assert.That (result.Losses, Is.Empty);

				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines, Does.Contain ($"DTSTART;TZID={expectedParam}:20240701T100000"));
				Assert.That (lines, Does.Contain ($"TZID:{expectedId}"));
			}
		}

		[Test]
		public void TestRecurrenceWithoutTimeZone ()
		{
			// A PidLidTimeZoneStruct that is too short is ignored.
			var builder = CreateRecurringMeeting (CreateRecurrence (0x0000, 1440, Array.Empty<uint> ()), false, Array.Empty<byte> (), configure: named => named.Binary (PidLidTimeZoneStruct, new byte[47]));

			using (var result = Convert (builder)) {
				Assert.That (result.Losses.Select (loss => loss.Kind), Is.EqualTo (new[] { TnefConversionLossKind.InvalidCalendarData }));

				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines, Does.Contain ("DTSTART;TZID=UTC:20240701T140000"));
				Assert.That (lines, Does.Contain ("RRULE:FREQ=DAILY;COUNT=10"));
			}
		}

		[Test]
		public void TestDisplayTimeZonesWithTheSameRules ()
		{
			var properties = CreateAppointment (new DateTime (2024, 7, 1, 14, 0, 0, DateTimeKind.Utc), new DateTime (2024, 7, 1, 15, 0, 0, DateTimeKind.Utc), out var named);

			named.Binary (PidLidAppointmentTimeZoneDefinitionStartDisplay, CreateEasternTimeZoneDefinition ());
			named.Binary (PidLidAppointmentTimeZoneDefinitionEndDisplay, CreateEasternTimeZoneDefinition ());

			using (var result = Convert (CreateMessage ("IPM.Appointment", properties))) {
				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines, Does.Contain ("DTSTART;TZID=Eastern Standard Time:20240701T100000"));
				Assert.That (lines, Does.Contain ("DTEND;TZID=Eastern Standard Time:20240701T110000"));
				Assert.That (lines.Count (line => line == "BEGIN:VTIMEZONE"), Is.EqualTo (1));
			}
		}

		[Test]
		public void TestDisplayTimeZonesWithTheSameName ()
		{
			var properties = CreateAppointment (new DateTime (2024, 7, 1, 14, 0, 0, DateTimeKind.Utc), new DateTime (2024, 7, 1, 16, 0, 0, DateTimeKind.Utc), out var named);

			// Two different time zones that claim the same name must get different TZIDs.
			named.Binary (PidLidAppointmentTimeZoneDefinitionStartDisplay, CreateEasternTimeZoneDefinition ());
			named.Binary (PidLidAppointmentTimeZoneDefinitionEndDisplay, CreateEasternTimeZoneDefinition (bias: 360));

			using (var result = Convert (CreateMessage ("IPM.Appointment", properties))) {
				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines, Does.Contain ("DTSTART;TZID=Eastern Standard Time:20240701T100000"));
				Assert.That (lines, Does.Contain ("DTEND;TZID=Eastern Standard Time (2):20240701T110000"));
				Assert.That (lines, Does.Contain ("TZID:Eastern Standard Time"));
				Assert.That (lines, Does.Contain ("TZID:Eastern Standard Time (2)"));
				Assert.That (lines.Count (line => line == "BEGIN:VTIMEZONE"), Is.EqualTo (2));
			}
		}

		[Test]
		public void TestRecurrenceIdFromExceptionReplaceTime ()
		{
			var properties = CreateAppointment (new DateTime (2024, 7, 10, 17, 0, 0, DateTimeKind.Utc), new DateTime (2024, 7, 10, 18, 0, 0, DateTimeKind.Utc), out var named);

			named.Binary (PidLidAppointmentTimeZoneDefinitionStartDisplay, CreateEasternTimeZoneDefinition ());
			named.Time (PidLidExceptionReplaceTime, new DateTime (2024, 7, 10, 14, 0, 0, DateTimeKind.Utc));

			using (var result = Convert (CreateMessage ("IPM.Schedule.Meeting.Request", properties))) {
				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines, Does.Contain ("RECURRENCE-ID;TZID=Eastern Standard Time:20240710T100000"));
				Assert.That (lines, Does.Contain ("DTSTART;TZID=Eastern Standard Time:20240710T130000"));
			}
		}

		static string GetRecurrenceIdFromGlobalObjectId (byte[] globalObjectId, int? startRecurrenceTime, bool timeZone, bool allDay = false)
		{
			var properties = CreateAppointment (new DateTime (2024, 7, 10, 17, 0, 0, DateTimeKind.Utc), new DateTime (2024, 7, 10, 18, 0, 0, DateTimeKind.Utc), out var named);

			named.Boolean (PidLidIsException, true, TnefPropertySetGuid.Meeting);
			named.Binary (PidLidGlobalObjectId, globalObjectId, TnefPropertySetGuid.Meeting);

			if (startRecurrenceTime.HasValue)
				named.Int32 (PidLidStartRecurrenceTime, startRecurrenceTime.Value, TnefPropertySetGuid.Meeting);

			if (timeZone)
				named.Binary (PidLidAppointmentTimeZoneDefinitionStartDisplay, CreateEasternTimeZoneDefinition ());

			if (allDay)
				named.Boolean (PidLidAppointmentSubType, true);

			using (var result = Convert (CreateMessage ("IPM.Schedule.Meeting.Request", properties))) {
				var lines = ReadCalendar (GetCalendarPart (result.Message));

				return lines.SingleOrDefault (line => line.StartsWith ("RECURRENCE-ID", StringComparison.Ordinal));
			}
		}

		[Test]
		public void TestRecurrenceIdFromGlobalObjectId ()
		{
			var id = CreateGlobalObjectId (year: 2024, month: 7, day: 10);
			int time = (10 << 12) | (30 << 6) | 15;

			Assert.That (GetRecurrenceIdFromGlobalObjectId (id, time, true), Is.EqualTo ("RECURRENCE-ID;TZID=Eastern Standard Time:20240710T103015"));
			Assert.That (GetRecurrenceIdFromGlobalObjectId (id, time, false), Is.EqualTo ("RECURRENCE-ID:20240710T103015Z"));
			Assert.That (GetRecurrenceIdFromGlobalObjectId (id, null, false), Is.EqualTo ("RECURRENCE-ID:20240710T000000Z"));
			Assert.That (GetRecurrenceIdFromGlobalObjectId (id, time, true, true), Is.EqualTo ("RECURRENCE-ID;VALUE=DATE:20240710"));

			// An invalid PidLidStartRecurrenceTime is ignored.
			Assert.That (GetRecurrenceIdFromGlobalObjectId (id, 24 << 12, false), Is.EqualTo ("RECURRENCE-ID:20240710T000000Z"));
			Assert.That (GetRecurrenceIdFromGlobalObjectId (id, 60 << 6, false), Is.EqualTo ("RECURRENCE-ID:20240710T000000Z"));
			Assert.That (GetRecurrenceIdFromGlobalObjectId (id, 60, false), Is.EqualTo ("RECURRENCE-ID:20240710T000000Z"));
		}

		[TestCase (0, 7, 10)]
		[TestCase (1600, 7, 10)]
		[TestCase (10000, 7, 10)]
		[TestCase (2024, 0, 10)]
		[TestCase (2024, 13, 10)]
		[TestCase (2024, 7, 0)]
		[TestCase (2023, 2, 29)]
		public void TestRecurrenceIdWithInvalidGlobalObjectIdDate (int year, int month, int day)
		{
			var id = CreateGlobalObjectId (year: year, month: month, day: day);

			Assert.That (GetRecurrenceIdFromGlobalObjectId (id, null, true), Is.Null);
		}

		[Test]
		public void TestRecurrenceIdWithShortGlobalObjectId ()
		{
			Assert.That (GetRecurrenceIdFromGlobalObjectId (new byte[19], null, true), Is.Null);
		}

		[Test]
		public void TestVCalUidWithoutValue ()
		{
			var properties = CreateAppointment (new DateTime (2024, 3, 1, 15, 0, 0, DateTimeKind.Utc), new DateTime (2024, 3, 1, 16, 0, 0, DateTimeKind.Utc), out var named);
			var data = Encoding.ASCII.GetBytes ("vCal-Uid\u0001\0\0\0\0");

			named.Binary (PidLidGlobalObjectId, CreateGlobalObjectId (data), TnefPropertySetGuid.Meeting);

			using (var result = Convert (CreateMessage ("IPM.Schedule.Meeting.Request", properties))) {
				var lines = ReadCalendar (GetCalendarPart (result.Message));

				// An empty vCal-Uid falls back to the hex-encoded GlobalObjectId.
				Assert.That (lines.Single (line => line.StartsWith ("UID:", StringComparison.Ordinal)), Does.StartWith ("UID:040000008200E00074C5B7101A82E008"));
			}
		}

		[TestCase (90, "DTEND:20240301T163000Z")]
		[TestCase (0, "DTEND:20240301T150000Z")]
		[TestCase (-1, null)]
		[TestCase (null, null)]
		public void TestAppointmentDuration (int? duration, string expected)
		{
			var properties = new TnefMapiPropertyBuilder ();
			var named = new NamedProperties (properties);

			properties.WriteStringProperty (TnefPropertyTag.SubjectW, "Team sync");
			named.Time (PidLidAppointmentStartWhole, new DateTime (2024, 3, 1, 15, 0, 0, DateTimeKind.Utc));

			if (duration.HasValue)
				named.Int32 (PidLidAppointmentDuration, duration.Value);

			using (var result = Convert (CreateMessage ("IPM.Appointment", properties))) {
				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines.SingleOrDefault (line => line.StartsWith ("DTEND", StringComparison.Ordinal)), Is.EqualTo (expected));
			}
		}

		[TestCase (0, null, "CLASS:PUBLIC")]
		[TestCase (1, null, "CLASS:X-PERSONAL")]
		[TestCase (2, null, "CLASS:PRIVATE")]
		[TestCase (3, null, "CLASS:CONFIDENTIAL")]
		[TestCase (4, true, null)]
		[TestCase (null, true, "CLASS:PRIVATE")]
		[TestCase (null, false, null)]
		[TestCase (0, true, "CLASS:PUBLIC")]
		public void TestSensitivity (int? sensitivity, bool? isPrivate, string expected)
		{
			var properties = CreateAppointment (new DateTime (2024, 3, 1, 15, 0, 0, DateTimeKind.Utc), new DateTime (2024, 3, 1, 16, 0, 0, DateTimeKind.Utc), out var named);

			if (sensitivity.HasValue)
				properties.WriteInt32Property (TnefPropertyTag.Sensitivity, sensitivity.Value);

			if (isPrivate.HasValue)
				named.Boolean (PidLidPrivate, isPrivate.Value, TnefPropertySetGuid.Common);

			using (var result = Convert (CreateMessage ("IPM.Appointment", properties))) {
				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines.SingleOrDefault (line => line.StartsWith ("CLASS:", StringComparison.Ordinal)), Is.EqualTo (expected));
			}
		}

		[TestCase (0, "PRIORITY:9")]
		[TestCase (1, "PRIORITY:5")]
		[TestCase (2, "PRIORITY:1")]
		[TestCase (3, null)]
		public void TestImportance (int importance, string expected)
		{
			var properties = CreateAppointment (new DateTime (2024, 3, 1, 15, 0, 0, DateTimeKind.Utc), new DateTime (2024, 3, 1, 16, 0, 0, DateTimeKind.Utc), out _);

			properties.WriteInt32Property (TnefPropertyTag.Importance, importance);

			using (var result = Convert (CreateMessage ("IPM.Appointment", properties))) {
				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines.SingleOrDefault (line => line.StartsWith ("PRIORITY:", StringComparison.Ordinal)), Is.EqualTo (expected));

				if (expected != null)
					Assert.That (lines, Does.Contain ("X-MICROSOFT-CDO-IMPORTANCE:" + importance));
				else
					Assert.That (lines.Any (line => line.StartsWith ("X-MICROSOFT-CDO-IMPORTANCE", StringComparison.Ordinal)), Is.False);
			}
		}

		[Test]
		public void TestMicrosoftExtensions ()
		{
			var properties = CreateAppointment (new DateTime (2024, 3, 1, 15, 0, 0, DateTimeKind.Utc), new DateTime (2024, 3, 1, 16, 0, 0, DateTimeKind.Utc), out var named);

			properties.WriteInt32Property (TnefPropertyTag.OwnerApptId, 42);
			named.Int32 (PidLidIntendedBusyStatus, 0);
			named.Boolean (PidLidReminderSet, true, TnefPropertySetGuid.Common);
			named.Int32 (PidLidReminderDelta, 0x5AE980E1, TnefPropertySetGuid.Common);
			named.Strings ("Keywords", TnefPropertySetGuid.PublicStrings, "Work", string.Empty, "Urgent, important");

			using (var result = Convert (CreateMessage ("IPM.Schedule.Meeting.Request", properties))) {
				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines, Does.Contain ("X-MICROSOFT-CDO-OWNERAPPTID:42"));
				Assert.That (lines, Does.Contain ("X-MICROSOFT-CDO-INTENDEDSTATUS:FREE"));
				Assert.That (lines, Does.Contain ("CATEGORIES:Work,Urgent\\, important"));

				// The "default" reminder delta means 15 minutes.
				Assert.That (lines, Does.Contain ("TRIGGER:-PT15M"));
			}
		}

		[Test]
		public void TestEmptyCategories ()
		{
			var properties = CreateAppointment (new DateTime (2024, 3, 1, 15, 0, 0, DateTimeKind.Utc), new DateTime (2024, 3, 1, 16, 0, 0, DateTimeKind.Utc), out var named);

			named.Strings ("Keywords", TnefPropertySetGuid.PublicStrings, string.Empty);

			using (var result = Convert (CreateMessage ("IPM.Appointment", properties))) {
				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines.Any (line => line.StartsWith ("CATEGORIES", StringComparison.Ordinal)), Is.False);
			}
		}

		[Test]
		public void TestIntendedBusyStatusIsOnlyForRequests ()
		{
			var properties = CreateAppointment (new DateTime (2024, 3, 1, 15, 0, 0, DateTimeKind.Utc), new DateTime (2024, 3, 1, 16, 0, 0, DateTimeKind.Utc), out var named);

			named.Int32 (PidLidIntendedBusyStatus, 2);

			using (var result = Convert (CreateMessage ("IPM.Appointment", properties))) {
				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines.Any (line => line.StartsWith ("X-MICROSOFT-CDO-INTENDEDSTATUS", StringComparison.Ordinal)), Is.False);
			}
		}

		[Test]
		public void TestPublishedMeetingAttendeeStatus ()
		{
			var properties = CreateAppointment (new DateTime (2024, 3, 1, 15, 0, 0, DateTimeKind.Utc), new DateTime (2024, 3, 1, 16, 0, 0, DateTimeKind.Utc), out var named);

			named.Int32 (PidLidAppointmentStateFlags, 1);

			TnefMapiPropertyBuilder Attendee (string name, int status)
			{
				var row = Recipient (TnefRecipientType.To, name, name.ToLowerInvariant () + "@example.com");

				row.WriteInt32Property (TnefPropertyTag.RecipientTrackStatus, status);

				return row;
			}

			var builder = CreateMessage ("IPM.Appointment", properties,
				Recipient (TnefRecipientType.To, "Alice", "alice@example.com", 0x03),
				Attendee ("Bob", 0),
				Attendee ("Carol", 2),
				Attendee ("Dave", 3),
				Attendee ("Erin", 4));

			using (var result = Convert (builder)) {
				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines, Does.Contain ("METHOD:PUBLISH"));
				Assert.That (lines, Does.Contain ("ORGANIZER;CN=Alice:mailto:alice@example.com"));
				Assert.That (lines, Does.Contain ("ATTENDEE;ROLE=REQ-PARTICIPANT;CN=Bob:mailto:bob@example.com"));
				Assert.That (lines, Does.Contain ("ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=TENTATIVE;CN=Carol:mailto:carol@example.com"));
				Assert.That (lines, Does.Contain ("ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=ACCEPTED;CN=Dave:mailto:dave@example.com"));
				Assert.That (lines, Does.Contain ("ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=DECLINED;CN=Erin:mailto:erin@example.com"));
			}
		}

		[Test]
		public void TestAttendeeAddressSources ()
		{
			var properties = CreateAppointment (new DateTime (2024, 3, 1, 15, 0, 0, DateTimeKind.Utc), new DateTime (2024, 3, 1, 16, 0, 0, DateTimeKind.Utc), out _);

			// An Exchange recipient with a PidTagSmtpAddress.
			var exchange = new TnefMapiPropertyBuilder ();
			exchange.WriteInt32Property (TnefPropertyTag.RecipientType, (int) TnefRecipientType.To);
			exchange.WriteStringProperty (TnefPropertyTag.DisplayNameW, "Eve");
			exchange.WriteStringProperty (TnefPropertyTag.AddrtypeW, "EX");
			exchange.WriteStringProperty (TnefPropertyTag.EmailAddressW, "/o=Example/cn=Recipients/cn=eve");
			exchange.WriteStringProperty (TnefPropertyTag.SmtpAddressW, "eve@example.com");

			// A recipient that only has a PidTagSearchKey.
			var searchKey = new TnefMapiPropertyBuilder ();
			searchKey.WriteInt32Property (TnefPropertyTag.RecipientType, (int) TnefRecipientType.To);
			searchKey.WriteStringProperty (TnefPropertyTag.DisplayNameW, "Frank");
			searchKey.WriteBinaryProperty (TnefPropertyTag.SearchKey, Encoding.ASCII.GetBytes ("SMTP:FRANK@EXAMPLE.COM\0"));

			// An Exchange recipient without any SMTP address.
			var unresolved = new TnefMapiPropertyBuilder ();
			unresolved.WriteInt32Property (TnefPropertyTag.RecipientType, (int) TnefRecipientType.To);
			unresolved.WriteStringProperty (TnefPropertyTag.DisplayNameW, "Grace");
			unresolved.WriteStringProperty (TnefPropertyTag.AddrtypeW, "EX");
			unresolved.WriteBinaryProperty (TnefPropertyTag.SearchKey, Encoding.ASCII.GetBytes ("EX:/O=EXAMPLE\0"));

			using (var result = Convert (CreateMessage ("IPM.Schedule.Meeting.Request", properties, exchange, searchKey, unresolved))) {
				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines, Does.Contain ("ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;CN=Eve:mailto:eve@example.com"));
				Assert.That (lines, Does.Contain ("ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;CN=Frank:mailto:FRANK@EXAMPLE.COM"));
				Assert.That (lines, Does.Contain ("ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;CN=Grace:invalid:nomail"));
			}
		}

		[Test]
		public void TestReplyWithPaddedMessageClass ()
		{
			var properties = CreateAppointment (new DateTime (2024, 3, 1, 15, 0, 0, DateTimeKind.Utc), new DateTime (2024, 3, 1, 16, 0, 0, DateTimeKind.Utc), out _);

			properties.WriteStringProperty (TnefPropertyTag.SentRepresentingNameW, "Bob");
			properties.WriteStringProperty (TnefPropertyTag.SentRepresentingAddrtypeW, "SMTP");
			properties.WriteStringProperty (TnefPropertyTag.SentRepresentingEmailAddressW, "bob@example.com");

			var builder = CreateMessage (" IPM.Schedule.Meeting.Resp.Pos ", properties, Recipient (TnefRecipientType.To, "Alice", "alice@example.com"));

			using (var result = Convert (builder)) {
				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines, Does.Contain ("METHOD:REPLY"));
				Assert.That (lines, Does.Contain ("ATTENDEE;PARTSTAT=ACCEPTED;CN=Bob:mailto:bob@example.com"));
			}
		}

		[Test]
		public void TestReplyWithoutOrganizerRecipient ()
		{
			var properties = CreateAppointment (new DateTime (2024, 3, 1, 15, 0, 0, DateTimeKind.Utc), new DateTime (2024, 3, 1, 16, 0, 0, DateTimeKind.Utc), out _);

			properties.WriteStringProperty (TnefPropertyTag.SentRepresentingNameW, "Bob");

			var builder = CreateMessage ("IPM.Schedule.Meeting.Resp.Neg", properties, Recipient (TnefRecipientType.Cc, "Alice", "alice@example.com"));

			using (var result = Convert (builder)) {
				var lines = ReadCalendar (GetCalendarPart (result.Message));

				// Only a To recipient is the organizer; a sender without an address is "invalid:nomail".
				Assert.That (lines.Any (line => line.StartsWith ("ORGANIZER", StringComparison.Ordinal)), Is.False);
				Assert.That (lines, Does.Contain ("ATTENDEE;PARTSTAT=DECLINED;CN=Bob:invalid:nomail"));
			}
		}

		static void WriteExceptionInfo (BinaryWriter writer, DateTime original, DateTime start, ushort flags, params uint[] values)
		{
			writer.Write (ToMinutes (start));
			writer.Write (ToMinutes (start.AddHours (1)));
			writer.Write (ToMinutes (original));
			writer.Write (flags);

			foreach (var value in values)
				writer.Write (value);
		}

		static void WriteEmptyExtendedExceptions (BinaryWriter writer, int count)
		{
			writer.Write (0u); // ReservedBlock1Size

			for (int i = 0; i < count; i++) {
				writer.Write (0u); // ChangeHighlightSize
				writer.Write (0u); // ReservedBlockEE1Size
			}
		}

		[Test]
		public void TestExceptionOverrides ()
		{
			var recurrence = CreateRecurrence (0x0001, 1, new uint[] { 0x0A }, writeExceptions: writer => {
				writer.Write ((ushort) 3);

				// ReminderDelta | Reminder | BusyStatus: a free occurrence with a 5 minute reminder.
				WriteExceptionInfo (writer, new DateTime (2024, 7, 3, 10, 0, 0), new DateTime (2024, 7, 3, 10, 0, 0), 0x002C, 5, 1, 0);

				// Reminder: an occurrence without a reminder.
				WriteExceptionInfo (writer, new DateTime (2024, 7, 8, 10, 0, 0), new DateTime (2024, 7, 8, 10, 0, 0), 0x0008, 0);

				// Reminder: an occurrence with the default reminder.
				WriteExceptionInfo (writer, new DateTime (2024, 7, 10, 10, 0, 0), new DateTime (2024, 7, 10, 10, 0, 0), 0x0008, 1);

				WriteEmptyExtendedExceptions (writer, 3);
			});

			using (var result = Convert (CreateRecurringMeeting (recurrence, false))) {
				Assert.That (result.Losses, Is.Empty);

				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines.Count (line => line == "BEGIN:VEVENT"), Is.EqualTo (4));
				Assert.That (GetComponent (lines, "VEVENT", 0).Any (line => line.StartsWith ("TRIGGER", StringComparison.Ordinal)), Is.False);

				var free = GetComponent (lines, "VEVENT", 1);
				Assert.That (free, Does.Contain ("RECURRENCE-ID;TZID=Eastern Standard Time:20240703T100000"));
				Assert.That (free, Does.Contain ("TRANSP:TRANSPARENT"));
				Assert.That (free, Does.Contain ("X-MICROSOFT-CDO-BUSYSTATUS:FREE"));
				Assert.That (free, Does.Contain ("TRIGGER:-PT5M"));

				var noReminder = GetComponent (lines, "VEVENT", 2);
				Assert.That (noReminder, Does.Contain ("RECURRENCE-ID;TZID=Eastern Standard Time:20240708T100000"));
				Assert.That (noReminder.Any (line => line.StartsWith ("TRIGGER", StringComparison.Ordinal)), Is.False);

				var defaultReminder = GetComponent (lines, "VEVENT", 3);
				Assert.That (defaultReminder, Does.Contain ("RECURRENCE-ID;TZID=Eastern Standard Time:20240710T100000"));
				Assert.That (defaultReminder, Does.Contain ("TRIGGER:-PT15M"));
			}
		}

		static void AddEmbeddedAttachment (TnefBuilder builder, int attachmentFlags, byte[] embedded, Action<TnefMapiPropertyBuilder> configure = null)
		{
			var value = new byte[16 + embedded.Length];

			IID_IMessage.ToByteArray ().CopyTo (value, 0);
			embedded.CopyTo (value, 16);

			var properties = new TnefMapiPropertyBuilder ();
			properties.WriteInt32Property (TnefPropertyTag.AttachMethod, (int) TnefAttachMethod.EmbeddedMessage);
			properties.WriteStringProperty (TnefPropertyTag.DisplayNameW, "Embedded");
			properties.WriteInt32Property (TnefPropertyTag.AttachmentFlags, attachmentFlags);
			configure?.Invoke (properties);
			properties.WriteBinaryProperty (TnefPropertyTag.AttachDataObj, value);

			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData, new byte[14]);
			builder.WriteMapiProperties (TnefAttributeLevel.Attachment, properties);
		}

		static byte[] CreateEmbeddedMessage (Action<TnefMapiPropertyBuilder, NamedProperties> configure)
		{
			var properties = new TnefMapiPropertyBuilder ();

			configure (properties, new NamedProperties (properties));

			return new TnefBuilder ().WriteTnefVersion ().WriteMapiProperties (TnefAttributeLevel.Message, properties).ToArray ();
		}

		[Test]
		public void TestExceptionAttachmentProperties ()
		{
			var recurrence = CreateRecurrence (0x0001, 1, new uint[] { 0x0A }, writeExceptions: writer => {
				writer.Write ((ushort) 2);
				WriteExceptionInfo (writer, new DateTime (2024, 7, 3, 10, 0, 0), new DateTime (2024, 7, 3, 10, 0, 0), 0);
				WriteExceptionInfo (writer, new DateTime (2024, 7, 10, 10, 0, 0), new DateTime (2024, 7, 10, 12, 0, 0), 0);
				WriteEmptyExtendedExceptions (writer, 2);
			});

			var builder = CreateRecurringMeeting (recurrence, false);

			// An embedded message that is not an exception.
			AddEmbeddedAttachment (builder, 0, CreateEmbeddedMessage ((properties, named) => properties.WriteStringProperty (TnefPropertyTag.SubjectW, "Forwarded")));

			// An exception that is matched by its PidLidExceptionReplaceTime and that overrides everything.
			AddEmbeddedAttachment (builder, 0x02, CreateEmbeddedMessage ((properties, named) => {
				properties.WriteStringProperty (TnefPropertyTag.SubjectW, "Attachment subject");
				properties.WriteStringProperty (TnefPropertyTag.BodyW, "Attachment body");
				named.Time (PidLidExceptionReplaceTime, new DateTime (2024, 7, 3, 14, 0, 0, DateTimeKind.Utc));
				named.String (PidLidLocation, "Room 9");
				named.Int32 (PidLidBusyStatus, 3);
				named.Boolean (PidLidReminderSet, true, TnefPropertySetGuid.Common);
				named.Int32 (PidLidReminderDelta, 20, TnefPropertySetGuid.Common);
				named.Boolean (PidLidAppointmentSubType, false);
				named.Time (PidLidAppointmentStartWhole, new DateTime (2024, 7, 3, 15, 0, 0, DateTimeKind.Utc));
				named.Time (PidLidAppointmentEndWhole, new DateTime (2024, 7, 3, 16, 30, 0, DateTimeKind.Utc));
			}));

			// An exception that is matched by the PidTagExceptionStartTime of the attachment.
			AddEmbeddedAttachment (builder, 0x02, CreateEmbeddedMessage ((properties, named) => {
				properties.WriteStringProperty (TnefPropertyTag.BodyW, "Second body");
				named.Boolean (PidLidReminderSet, false, TnefPropertySetGuid.Common);
			}), properties => properties.WriteInt64Property (TnefPropertyTag.ExceptionStartTime, new DateTime (2024, 7, 10, 12, 0, 0, DateTimeKind.Utc).ToFileTimeUtc ()));

			// An exception that cannot be loaded.
			AddEmbeddedAttachment (builder, 0x02, new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 });

			using (var result = Convert (builder)) {
				Assert.That (result.Losses.Select (loss => loss.Kind), Does.Contain (TnefConversionLossKind.InvalidCalendarData));
				Assert.That (result.Losses.Any (loss => loss.Description.Contains ("could not be loaded")), Is.True);

				var lines = ReadCalendar (GetCalendarPart (result.Message));

				Assert.That (lines.Count (line => line == "BEGIN:VEVENT"), Is.EqualTo (3));

				var first = GetComponent (lines, "VEVENT", 1);
				Assert.That (first, Does.Contain ("RECURRENCE-ID;TZID=Eastern Standard Time:20240703T100000"));
				Assert.That (first, Does.Contain ("SUMMARY:Attachment subject"));
				Assert.That (first, Does.Contain ("DESCRIPTION:Attachment body"));
				Assert.That (first, Does.Contain ("LOCATION:Room 9"));
				Assert.That (first, Does.Contain ("X-MICROSOFT-CDO-BUSYSTATUS:OOF"));
				Assert.That (first, Does.Contain ("TRIGGER:-PT20M"));
				Assert.That (first, Does.Contain ("DTSTART;TZID=Eastern Standard Time:20240703T110000"));
				Assert.That (first, Does.Contain ("DTEND;TZID=Eastern Standard Time:20240703T123000"));

				var second = GetComponent (lines, "VEVENT", 2);
				Assert.That (second, Does.Contain ("RECURRENCE-ID;TZID=Eastern Standard Time:20240710T100000"));
				Assert.That (second, Does.Contain ("DTSTART;TZID=Eastern Standard Time:20240710T120000"));
				Assert.That (second, Does.Contain ("DESCRIPTION:Second body"));
				Assert.That (second, Does.Contain ("SUMMARY:Team sync"));
				Assert.That (second.Any (line => line.StartsWith ("TRIGGER", StringComparison.Ordinal)), Is.False);
			}
		}
	}
}

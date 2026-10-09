//
// TnefCalendarBuilder.cs
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

// Specification map
// -----------------
// [MS-OXCMAIL] 2.1.3.3.8 says that a calendar item (IPM.Appointment) or meeting message (IPM.Schedule.Meeting.*) is
// written with a text/calendar alternative, generated from the Message object as specified by [MS-OXCICAL]. This
// class implements the export direction of [MS-OXCICAL] 2.1.3.1.1 for the properties that TNEF carries:
//
//   iCalendar               Source                                                [MS-OXCICAL] section
//   ----------------------  ----------------------------------------------------  --------------------
//   METHOD                  PidTagMessageClass                                    2.1.3.1.1.16
//   VTIMEZONE               PidLidAppointmentTimeZoneDefinition*, TimeZoneStruct  2.1.3.1.1.19
//   ATTENDEE / ORGANIZER    recipient table, PidTagRecipientFlags                 2.1.3.1.1.20.2, 2.1.3.1.1.20.18
//   CATEGORIES              PidNameKeywords                                       2.1.3.1.1.20.3
//   CLASS                   PidTagSensitivity, PidLidPrivate                      2.1.3.1.1.20.4
//   COMMENT / DESCRIPTION   PidTagBody                                            2.1.3.1.1.20.5, 2.1.3.1.1.20.7
//   DTSTART / DTEND         PidLidAppointmentStartWhole / EndWhole                2.1.3.1.1.20.8, 2.1.3.1.1.20.10
//   DTSTAMP                 PidLidOwnerCriticalChange / AttendeeCriticalChange    2.1.3.1.1.20.9
//   EXDATE / RRULE          PidLidAppointmentRecur                                2.1.3.1.1.20.11, 2.1.3.1.1.20.21
//   LOCATION                PidLidLocation                                        2.1.3.1.1.20.15
//   PRIORITY                PidTagImportance                                      2.1.3.1.1.20.19
//   RECURRENCE-ID           PidLidExceptionReplaceTime, ExceptionInfo             2.1.3.1.1.20.20
//   SEQUENCE                PidLidAppointmentSequence                             2.1.3.1.1.20.22
//   SUMMARY                 PidTagNormalizedSubject                               2.1.3.1.1.20.24
//   TRANSP                  PidLidBusyStatus                                      2.1.3.1.1.20.25
//   UID                     PidLidCleanGlobalObjectId / GlobalObjectId            2.1.3.1.1.20.26
//   X-MICROSOFT-CDO-*       busy status, importance, intended status, owner id    2.1.3.1.1.20.31-35
//   VALARM                  PidLidReminderSet / PidLidReminderDelta               2.1.3.1.1.20.62
//
// Exceptions to a recurring series are written as additional VEVENTs (2.1.3.1.1.20.20), from the ExceptionInfo
// blocks of the recurrence blob, overridden by the exception's embedded Calendar object when one exists.

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;

using MimeKit.IO;

namespace MimeKit.Tnef {
	/// <summary>
	/// Builds an iCalendar object from the properties of a calendar item or meeting message, as specified by
	/// [MS-OXCICAL].
	/// </summary>
	sealed class TnefCalendarBuilder
	{
		const string ProductId = "-//.NET Foundation//MimeKit//EN";

		// [MS-OXOCAL] 2.2.1.10: asfMeeting.
		const int AppointmentStateMeeting = 0x0001;

		// [MS-OXOCAL] 2.2.4.10.1: recipOrganizer and recipExceptionalDeleted.
		const int RecipientOrganizer = 0x0002;
		const int RecipientExceptionalDeleted = 0x0020;

		// [MS-OXCMSG] 2.2.2.18: afException.
		const int AttachmentFlagException = 0x0002;

		// [MS-OXOCAL] 2.2.1.3: the "not set" value of PidLidReminderDelta, which means the default of 15 minutes.
		const int ReminderDeltaDefault = 0x5AE980E1;

		// The vCal-Uid signature of a GlobalObjectId that wraps a UID from an iCalendar object ([MS-OXOCAL] 2.2.1.27).
		static readonly byte[] VCalUid = { (byte) 'v', (byte) 'C', (byte) 'a', (byte) 'l', (byte) '-', (byte) 'U', (byte) 'i', (byte) 'd', 0x01, 0x00, 0x00, 0x00 };

		static readonly string[] BusyStatusNames = { "FREE", "TENTATIVE", "BUSY", "OOF", "WORKINGELSEWHERE" };

		readonly List<CalendarEvent> events = new List<CalendarEvent> ();
		readonly List<TnefTimeZone> timeZones = new List<TnefTimeZone> ();
		readonly HashSet<TnefAttachment> exceptionAttachments = new HashSet<TnefAttachment> ();
		readonly Action<TnefConversionLossKind, string> addLoss;
		readonly CancellationToken cancellationToken;
		readonly Encoding fallbackEncoding;
		readonly TnefMessage tnef;
		readonly long maxExceptionsSize;
		readonly int maxExceptions;
		long exceptionsSize;

		struct CalendarTime
		{
			public DateTime Value;
			public TnefTimeZone? Zone;
			public bool IsDate;
		}

		sealed class CalendarAddress
		{
			public string Address = string.Empty;
			public string? Name;
			public string? CuType;
			public string? Role;
			public string? PartStat;
			public bool? Rsvp;
		}

		sealed class CalendarEvent
		{
			public List<CalendarAddress> Attendees = new List<CalendarAddress> ();
			public CalendarAddress? Organizer;
			public string[]? Categories;
			public string? Class;
			public string? Description;
			public DateTime? Created;
			public CalendarTime? End;
			public DateTime Stamp;
			public CalendarTime Start;
			public List<CalendarTime> ExDates = new List<CalendarTime> ();
			public DateTime? LastModified;
			public string? Location;
			public int? Priority;
			public CalendarTime? RecurrenceId;
			public string? RecurrenceRule;
			public int Sequence;
			public string Summary = string.Empty;
			public string Uid = string.Empty;
			public int? BusyStatus;
			public int? Importance;
			public int? IntendedBusyStatus;
			public int? OwnerAppointmentId;
			public bool? DisallowCounter;
			public CalendarTime? OriginalStart;
			public CalendarTime? OriginalEnd;
			public int? ReminderMinutes;

			public CalendarEvent CloneForException ()
			{
				var clone = (CalendarEvent) MemberwiseClone ();

				clone.ExDates = new List<CalendarTime> ();
				clone.RecurrenceRule = null;
				clone.OriginalStart = null;
				clone.OriginalEnd = null;

				return clone;
			}
		}

		TnefCalendarBuilder (TnefMessage tnef, string method, Encoding fallbackEncoding, int maxExceptions, long maxExceptionsSize, Action<TnefConversionLossKind, string> addLoss, CancellationToken cancellationToken)
		{
			this.cancellationToken = cancellationToken;
			this.maxExceptionsSize = maxExceptionsSize;
			this.fallbackEncoding = fallbackEncoding;
			this.maxExceptions = maxExceptions;
			this.addLoss = addLoss;
			this.tnef = tnef;
			Method = method;
		}

		/// <summary>
		/// Get the iCalendar METHOD, which is also the value of the text/calendar method parameter.
		/// </summary>
		public string Method { get; }

		/// <summary>
		/// Get the exception attachments whose content was written to the calendar.
		/// </summary>
		public ICollection<TnefAttachment> ExceptionAttachments {
			get { return exceptionAttachments; }
		}

		bool IsReply {
			get { return Method == "REPLY" || Method == "COUNTER"; }
		}

		static bool IsMessageClass (string messageClass, string value)
		{
			if (!messageClass.StartsWith (value, StringComparison.OrdinalIgnoreCase))
				return false;

			return messageClass.Length == value.Length || messageClass[value.Length] == '.';
		}

		/// <summary>
		/// Get the iCalendar method for the message class, or <see langword="null"/> if the message is not a
		/// calendar item or meeting message.
		/// </summary>
		internal static string? GetMethod (TnefMessage tnef)
		{
			var messageClass = tnef.MessageClass;

			if (messageClass is null)
				return null;

			// [MS-OXCICAL] 2.1.3.1.1.16: the METHOD depends on the message class.
			if (IsMessageClass (messageClass, "IPM.Appointment"))
				return "PUBLISH";

			if (IsMessageClass (messageClass, "IPM.Schedule.Meeting.Request"))
				return "REQUEST";

			if (IsMessageClass (messageClass, "IPM.Schedule.Meeting.Canceled"))
				return "CANCEL";

			if (IsMessageClass (messageClass, "IPM.Schedule.Meeting.Resp.Pos") || IsMessageClass (messageClass, "IPM.Schedule.Meeting.Resp.Neg"))
				return "REPLY";

			if (IsMessageClass (messageClass, "IPM.Schedule.Meeting.Resp.Tent")) {
				// A tentative response that proposes a new time is a counter proposal.
				return GetBoolean (tnef.Properties, TnefNameId.AppointmentCounterProposal) == true ? "COUNTER" : "REPLY";
			}

			return null;
		}

		static string? GetReplyStatus (string messageClass)
		{
			if (IsMessageClass (messageClass, "IPM.Schedule.Meeting.Resp.Pos"))
				return "ACCEPTED";

			if (IsMessageClass (messageClass, "IPM.Schedule.Meeting.Resp.Tent"))
				return "TENTATIVE";

			if (IsMessageClass (messageClass, "IPM.Schedule.Meeting.Resp.Neg"))
				return "DECLINED";

			return null;
		}

		/// <summary>
		/// Create the calendar for a calendar item or meeting message.
		/// </summary>
		/// <remarks>
		/// Malformed calendar data is reported through <paramref name="addLoss"/> rather than thrown.
		/// </remarks>
		/// <returns>The builder, or <see langword="null"/> if the message is not a calendar item or meeting message, or
		/// if it does not have a start time.</returns>
		public static TnefCalendarBuilder? Create (TnefMessage tnef, Encoding fallbackEncoding, int maxExceptions, long maxExceptionsSize, Action<TnefConversionLossKind, string> addLoss, CancellationToken cancellationToken)
		{
			var method = GetMethod (tnef);

			if (method is null)
				return null;

			if (GetDateTime (tnef.Properties, TnefNameId.AppointmentStartWhole) is null) {
				addLoss (TnefConversionLossKind.InvalidCalendarData, "The calendar item does not have a start time, so no text/calendar part was generated.");
				return null;
			}

			var builder = new TnefCalendarBuilder (tnef, method, fallbackEncoding, maxExceptions, maxExceptionsSize, addLoss, cancellationToken);

			builder.Build ();

			return builder;
		}

		#region Property access

		static bool? GetBoolean (TnefPropertySet properties, TnefNameId name)
		{
			return properties.TryGetValue (name, out var property) && property.TryGetBoolean (out var value) ? value : null;
		}

		static int? GetInt32 (TnefPropertySet properties, TnefNameId name)
		{
			return properties.TryGetValue (name, out var property) && property.TryGetInt32 (out var value) ? value : null;
		}

		static DateTime? GetDateTime (TnefPropertySet properties, TnefNameId name)
		{
			if (!properties.TryGetValue (name, out var property) || !property.TryGetDateTime (out var value))
				return null;

			return DateTime.SpecifyKind (value, DateTimeKind.Utc);
		}

		static string? GetString (TnefPropertySet properties, TnefNameId name)
		{
			return properties.TryGetValue (name, out var property) && property.TryGetString (out var value) ? value.TrimEnd ('\0') : null;
		}

		static byte[]? GetBytes (TnefPropertySet properties, TnefNameId name)
		{
			return properties.TryGetValue (name, out var property) && property.TryGetBytes (out var value) && value.Length > 0 ? value : null;
		}

		static DateTime? GetUtcDateTime (TnefPropertySet properties, TnefPropertyTag tag)
		{
			var value = properties.GetDateTime (tag);

			return value.HasValue ? DateTime.SpecifyKind (value.Value, DateTimeKind.Utc) : null;
		}

		static string? GetSubject (TnefMessage message)
		{
			// [MS-OXCICAL] 2.1.3.1.1.20.24: SUMMARY is exported from PidTagNormalizedSubject; fall back to the subject.
			return message.Properties.GetString (TnefPropertyTag.NormalizedSubjectW)?.TrimEnd ('\0') ?? message.Subject?.TrimEnd ('\0');
		}

		string? GetBodyText (TnefMessage message)
		{
			var body = message.TextBody;

			if (body is null)
				return null;

			string text;

			if (body.Tag.ValueTnefType == TnefPropertyType.Unicode) {
				using (var stream = body.OpenRead ())
				using (var reader = new StreamReader (stream, Encoding.Unicode, false))
					text = reader.ReadToEnd ();
			} else {
				using (var stream = body.OpenRead ())
				using (var reader = new StreamReader (stream, body.Encoding ?? fallbackEncoding, false))
					text = reader.ReadToEnd ();
			}

			text = text.TrimEnd ('\0', '\r', '\n', ' ', '\t');

			return text.Length > 0 ? text : null;
		}

		#endregion

		#region Time zones and times

		TnefTimeZone Register (TnefTimeZone zone)
		{
			foreach (var registered in timeZones) {
				if (ReferenceEquals (registered, zone))
					return zone;

				if (registered.Id.Equals (zone.Id, StringComparison.Ordinal) && registered.HasSameRules (zone))
					return registered;
			}

			// Two different zones with the same name need different TZIDs.
			var id = zone.Id;

			for (int n = 2; Exists (zone.Id); n++)
				zone.Id = string.Format (CultureInfo.InvariantCulture, "{0} ({1})", id, n);

			timeZones.Add (zone);

			return zone;
		}

		bool Exists (string id)
		{
			foreach (var zone in timeZones) {
				if (zone.Id.Equals (id, StringComparison.Ordinal))
					return true;
			}

			return false;
		}

		static TnefTimeZone? GetRecurrenceTimeZone (TnefPropertySet properties)
		{
			var description = GetString (properties, TnefNameId.TimeZoneDescription);

			if (string.IsNullOrWhiteSpace (description))
				description = null;

			// [MS-OXCICAL] 2.1.3.1.1.19: a recurring appointment uses PidLidAppointmentTimeZoneDefinitionRecur, falling
			// back to PidLidTimeZoneStruct (named by PidLidTimeZoneDescription).
			var definition = GetBytes (properties, TnefNameId.AppointmentTimeZoneDefinitionRecur);
			TnefTimeZone? zone = null;

			if (definition != null)
				zone = TnefTimeZone.FromTimeZoneDefinition (definition, description);

			if (zone is null) {
				var structure = GetBytes (properties, TnefNameId.TimeZoneStruct);

				if (structure != null && structure.Length >= 48) {
					var id = description ?? TnefTimeZone.FormatDefaultId (TnefBinaryReader.ToInt32 (structure, 0) + TnefBinaryReader.ToInt32 (structure, 4));

					zone = TnefTimeZone.FromTimeZoneStruct (structure, id.Trim ());
				}
			}

			return zone;
		}

		static TnefTimeZone? GetDisplayTimeZone (TnefPropertySet properties, TnefNameId name)
		{
			var definition = GetBytes (properties, name);

			return definition != null ? TnefTimeZone.FromTimeZoneDefinition (definition, null) : null;
		}

		CalendarTime FromUtc (DateTime utc, TnefTimeZone? zone, bool allDay)
		{
			if (allDay) {
				// An all-day appointment starts and ends at midnight in the organizer's time zone. Without a time zone,
				// the nearest midnight is the best estimate.
				var date = zone != null ? zone.ToLocalTime (utc).Date : utc.AddHours (12).Date;

				return new CalendarTime { Value = date, IsDate = true };
			}

			if (zone is null)
				return new CalendarTime { Value = DateTime.SpecifyKind (utc, DateTimeKind.Utc) };

			zone = Register (zone);

			return new CalendarTime { Value = zone.ToLocalTime (utc), Zone = zone };
		}

		CalendarTime FromLocal (DateTime local, TnefTimeZone zone, bool allDay)
		{
			if (allDay)
				return new CalendarTime { Value = local.Date, IsDate = true };

			return new CalendarTime { Value = DateTime.SpecifyKind (local, DateTimeKind.Unspecified), Zone = Register (zone) };
		}

		#endregion

		#region Addresses

		static string? ResolveAddress (string? smtpAddress, string? emailAddress, string? addrType, string? searchKey)
		{
			bool isSmtp = string.IsNullOrEmpty (addrType) || addrType!.Equals ("SMTP", StringComparison.OrdinalIgnoreCase);
			string? address = null;

			// The same sources as the To/Cc/From headers ([MS-OXCMAIL] 2.1.3.1.1), but failures are not reported again.
			if (isSmtp && !string.IsNullOrEmpty (emailAddress))
				address = emailAddress;
			else if (!string.IsNullOrEmpty (smtpAddress))
				address = smtpAddress;
			else if (isSmtp && searchKey != null && searchKey.StartsWith ("SMTP:", StringComparison.OrdinalIgnoreCase))
				address = searchKey.Substring (5);

			if (string.IsNullOrEmpty (address) || !MailboxAddress.TryParse (address, out var mailbox))
				return null;

			return mailbox.Address;
		}

		static CalendarAddress CreateAddress (string? address, string? name)
		{
			// [MS-OXCICAL] 2.1.3.1.1.20.2: an attendee without an SMTP address is exported as "invalid:nomail".
			return new CalendarAddress {
				Address = address != null ? "mailto:" + address : "invalid:nomail",
				Name = string.IsNullOrWhiteSpace (name) ? null : name!.TrimEnd ('\0')
			};
		}

		static CalendarAddress CreateAddress (TnefRecipient recipient)
		{
			var properties = recipient.Properties;
			var address = ResolveAddress (
				properties.GetString (TnefPropertyTag.SmtpAddressW),
				properties.GetString (TnefPropertyTag.EmailAddressW),
				recipient.AddressType,
				properties.GetString (TnefPropertyTag.SearchKey)?.TrimEnd ('\0'));

			return CreateAddress (address, recipient.DisplayName);
		}

		CalendarAddress? GetSentRepresenting ()
		{
			var properties = tnef.Properties;
			var name = properties.GetString (TnefPropertyTag.SentRepresentingNameW);
			var address = ResolveAddress (
				null,
				properties.GetString (TnefPropertyTag.SentRepresentingEmailAddressW),
				properties.GetString (TnefPropertyTag.SentRepresentingAddrtypeW),
				properties.GetString (TnefPropertyTag.SentRepresentingSearchKey)?.TrimEnd ('\0'));

			if (address is null) {
				name ??= properties.GetString (TnefPropertyTag.SenderNameW);
				address = ResolveAddress (
					properties.GetString (TnefPropertyTag.SenderSmtpAddressW),
					properties.GetString (TnefPropertyTag.SenderEmailAddressW),
					properties.GetString (TnefPropertyTag.SenderAddrtypeW),
					properties.GetString (TnefPropertyTag.SenderSearchKey)?.TrimEnd ('\0'));
			}

			if (address is null && string.IsNullOrWhiteSpace (name))
				return null;

			return CreateAddress (address, name);
		}

		void AddAttendees (CalendarEvent ev)
		{
			var properties = tnef.Properties;

			if (IsReply) {
				// [MS-OXCICAL] 2.1.3.1.1.20.2 and 2.1.3.1.1.20.18: a response is sent by the attendee to the organizer,
				// who is its recipient.
				foreach (var recipient in tnef.Recipients) {
					if (recipient.RecipientType == TnefRecipientType.To) {
						ev.Organizer = CreateAddress (recipient);
						break;
					}
				}

				var attendee = GetSentRepresenting ();

				if (attendee != null) {
					attendee.PartStat = GetReplyStatus (tnef.MessageClass ?? string.Empty);
					ev.Attendees.Add (attendee);
				}

				return;
			}

			// A published appointment only has attendees when it is a meeting.
			if (Method == "PUBLISH" && ((GetInt32 (properties, TnefNameId.AppointmentStateFlags) ?? 0) & AppointmentStateMeeting) == 0)
				return;

			var rsvp = properties.GetBoolean (TnefPropertyTag.ResponseRequested);

			foreach (var recipient in tnef.Recipients) {
				int flags = recipient.Properties.GetInt32 (TnefPropertyTag.RecipientFlags) ?? 0;

				if ((flags & RecipientExceptionalDeleted) != 0)
					continue;

				if ((flags & RecipientOrganizer) != 0 || recipient.RecipientType == TnefRecipientType.Originator) {
					ev.Organizer ??= CreateAddress (recipient);
					continue;
				}

				var attendee = CreateAddress (recipient);

				switch (recipient.RecipientType) {
				case TnefRecipientType.Cc:
					attendee.Role = "OPT-PARTICIPANT";
					break;
				case TnefRecipientType.Bcc:
					// Resources (rooms and equipment) are Bcc recipients of a meeting request.
					attendee.CuType = "RESOURCE";
					attendee.Role = "NON-PARTICIPANT";
					break;
				default:
					attendee.Role = "REQ-PARTICIPANT";
					break;
				}

				if (Method == "PUBLISH") {
					// [MS-OXCICAL] 2.1.3.1.1.20.2.4: PARTSTAT comes from PidTagRecipientTrackStatus.
					switch (recipient.Properties.GetInt32 (TnefPropertyTag.RecipientTrackStatus)) {
					case 2: attendee.PartStat = "TENTATIVE"; break;
					case 3: attendee.PartStat = "ACCEPTED"; break;
					case 4: attendee.PartStat = "DECLINED"; break;
					}
				} else {
					attendee.PartStat = "NEEDS-ACTION";
				}

				attendee.Rsvp = rsvp;
				ev.Attendees.Add (attendee);
			}

			ev.Organizer ??= GetSentRepresenting ();
		}

		#endregion

		#region UID

		static string? GetUid (TnefPropertySet properties)
		{
			var id = GetBytes (properties, TnefNameId.CleanGlobalObjectId) ?? GetBytes (properties, TnefNameId.GlobalObjectId);

			// [MS-OXOCAL] 2.2.1.27: Byte Array ID (16), YH, YL, M, D, Creation Time (8), X (8), Size (4), Data.
			if (id is null || id.Length < 40)
				return null;

			int size = TnefBinaryReader.ToInt32 (id, 36);

			if (size >= VCalUid.Length && size <= id.Length - 40 && StartsWith (id, 40, VCalUid)) {
				// [MS-OXCICAL] 2.1.3.1.1.20.26: the UID of an object that was imported from iCalendar is preserved.
				var uid = Encoding.UTF8.GetString (id, 40 + VCalUid.Length, size - VCalUid.Length).TrimEnd ('\0');

				if (uid.Length > 0)
					return uid;
			}

			// Otherwise, the UID is the hex-encoded GlobalObjectId with the instance date cleared.
			var builder = new StringBuilder (id.Length * 2);

			for (int i = 0; i < id.Length; i++) {
				byte b = i >= 16 && i < 20 ? (byte) 0 : id[i];

				builder.Append (b.ToString ("X2", CultureInfo.InvariantCulture));
			}

			return builder.ToString ();
		}

		// When the object has no GlobalObjectId, derive the UID from properties that identify it so that
		// converting the same TNEF more than once produces the same UID (which lets clients match updates).
		string CreateUid (TnefPropertySet properties, DateTime start, DateTime? end)
		{
			var inv = CultureInfo.InvariantCulture;
			var seed = new StringBuilder ();

			seed.Append (GetSubject (tnef)).Append ('\n');
			seed.Append (start.Ticks.ToString (inv)).Append ('\n');
			seed.Append (end?.Ticks.ToString (inv)).Append ('\n');
			seed.Append (GetUtcDateTime (properties, TnefPropertyTag.CreationTime)?.Ticks.ToString (inv)).Append ('\n');
			seed.Append (properties.GetString (TnefPropertyTag.SentRepresentingEmailAddressW) ?? properties.GetString (TnefPropertyTag.SenderEmailAddressW));

			byte[] hash;

			using (var sha256 = System.Security.Cryptography.SHA256.Create ())
				hash = sha256.ComputeHash (Encoding.UTF8.GetBytes (seed.ToString ()));

			var guid = new byte[16];

			Buffer.BlockCopy (hash, 0, guid, 0, 16);

			// Mark it as a name-based (version 5 style) RFC 4122 UUID.
			guid[7] = (byte) ((guid[7] & 0x0F) | 0x50);
			guid[8] = (byte) ((guid[8] & 0x3F) | 0x80);

			return new Guid (guid).ToString ().ToUpperInvariant ();
		}

		static bool StartsWith (byte[] buffer, int index, byte[] value)
		{
			for (int i = 0; i < value.Length; i++) {
				if (buffer[index + i] != value[i])
					return false;
			}

			return true;
		}

		#endregion

		#region Building

		void Build ()
		{
			var properties = tnef.Properties;
			var start = GetDateTime (properties, TnefNameId.AppointmentStartWhole)!.Value;
			var end = GetDateTime (properties, TnefNameId.AppointmentEndWhole);
			bool allDay = GetBoolean (properties, TnefNameId.AppointmentSubType) == true;
			TnefRecurrence? recurrence = null;
			TnefTimeZone? recurrenceZone = null;

			if (end is null) {
				var duration = GetInt32 (properties, TnefNameId.AppointmentDuration);

				if (duration.HasValue && duration.Value >= 0)
					end = start.AddMinutes (duration.Value);
			}

			var recurrenceBlob = GetBytes (properties, TnefNameId.AppointmentRecur);

			if (recurrenceBlob != null && GetBoolean (properties, TnefNameId.Recurring) != false) {
				try {
					recurrence = TnefRecurrence.Parse (recurrenceBlob, fallbackEncoding);
				} catch (FormatException ex) {
					addLoss (TnefConversionLossKind.InvalidCalendarData, $"The recurrence pattern could not be parsed, so the appointment was exported as a single occurrence: {ex.Message}");
				}
			}

			var ev = new CalendarEvent ();

			ev.Uid = GetUid (properties) ?? CreateUid (properties, start, end);
			ev.Summary = GetSubject (tnef) ?? string.Empty;
			ev.Location = GetString (properties, TnefNameId.Location);
			ev.Description = GetBodyText (tnef);
			ev.Sequence = GetInt32 (properties, TnefNameId.AppointmentSequence) ?? 0;
			ev.Created = GetUtcDateTime (properties, TnefPropertyTag.CreationTime);
			ev.LastModified = GetUtcDateTime (properties, TnefPropertyTag.LastModificationTime);
			ev.Stamp = GetStamp (properties);
			ev.BusyStatus = GetInt32 (properties, TnefNameId.BusyStatus);
			ev.OwnerAppointmentId = properties.GetInt32 (TnefPropertyTag.OwnerApptId);
			ev.DisallowCounter = GetBoolean (properties, TnefNameId.AppointmentNotAllowPropose);

			if (Method == "REQUEST")
				ev.IntendedBusyStatus = GetInt32 (properties, TnefNameId.IntendedBusyStatus);

			if (properties.TryGetValue (TnefNameId.Keywords, out var keywords) && keywords.TryGetValues<string> (out var categories) && categories.Length > 0)
				ev.Categories = categories;

			// [MS-OXCICAL] 2.1.3.1.1.20.4: CLASS from PidTagSensitivity, or PidLidPrivate when it is absent.
			switch (properties.GetInt32 (TnefPropertyTag.Sensitivity)) {
			case 0: ev.Class = "PUBLIC"; break;
			case 1: ev.Class = "X-PERSONAL"; break;
			case 2: ev.Class = "PRIVATE"; break;
			case 3: ev.Class = "CONFIDENTIAL"; break;
			case null:
				if (GetBoolean (properties, TnefNameId.Private) == true)
					ev.Class = "PRIVATE";
				break;
			}

			// [MS-OXCICAL] 2.1.3.1.1.20.19: PRIORITY from PidTagImportance.
			ev.Importance = properties.GetInt32 (TnefPropertyTag.Importance);

			switch (ev.Importance) {
			case 0: ev.Priority = 9; break;
			case 1: ev.Priority = 5; break;
			case 2: ev.Priority = 1; break;
			default: ev.Importance = null; break;
			}

			ev.ReminderMinutes = GetReminder (properties);

			if (recurrence != null) {
				recurrenceZone = GetRecurrenceTimeZone (properties);

				if (recurrenceZone is null) {
					// The recurrence pattern is in local time, which cannot be interpreted without a time zone.
					addLoss (TnefConversionLossKind.InvalidCalendarData, "The recurring appointment does not have a valid time zone, so UTC was assumed.");
					recurrenceZone = new TnefTimeZone ("UTC", 0, 0, 0, default, default);
				}

				ev.Start = FromUtc (start, recurrenceZone, allDay);

				if (end.HasValue)
					ev.End = FromUtc (end.Value, recurrenceZone, allDay);
			} else {
				// [MS-OXCICAL] 2.1.3.1.1.20.10: a single appointment is written in the time zones that it was created in.
				var startZone = GetDisplayTimeZone (properties, TnefNameId.AppointmentTimeZoneDefinitionStartDisplay);
				var endZone = GetDisplayTimeZone (properties, TnefNameId.AppointmentTimeZoneDefinitionEndDisplay) ?? startZone;

				if (Method == "COUNTER") {
					// [MS-OXCICAL] 2.1.3.1.1.20.8 and 2.1.3.1.1.20.10: a counter proposal has the proposed times as
					// DTSTART and DTEND and the original times as X-MS-OLK-ORIGINALSTART and X-MS-OLK-ORIGINALEND.
					var proposedStart = GetDateTime (properties, TnefNameId.AppointmentProposedStartWhole);
					var proposedEnd = GetDateTime (properties, TnefNameId.AppointmentProposedEndWhole);

					if (proposedStart.HasValue) {
						ev.OriginalStart = FromUtc (start, startZone, allDay);

						if (end.HasValue)
							ev.OriginalEnd = FromUtc (end.Value, endZone, allDay);

						start = proposedStart.Value;
						end = proposedEnd ?? end;
					}
				}

				ev.Start = FromUtc (start, startZone, allDay);

				if (end.HasValue)
					ev.End = FromUtc (end.Value, endZone, allDay);

				ev.RecurrenceId = GetRecurrenceId (properties, startZone, allDay);
			}

			AddAttendees (ev);
			events.Add (ev);

			if (recurrence != null)
				AddRecurrence (ev, recurrence, recurrenceZone!, allDay);
		}

		DateTime GetStamp (TnefPropertySet properties)
		{
			// [MS-OXCICAL] 2.1.3.1.1.20.9: DTSTAMP is the time of the last critical change made by the sender.
			var stamp = GetDateTime (properties, IsReply ? TnefNameId.AttendeeCriticalChange : TnefNameId.OwnerCriticalChange)
				?? GetUtcDateTime (properties, TnefPropertyTag.ClientSubmitTime)
				?? GetUtcDateTime (properties, TnefPropertyTag.LastModificationTime);

			return stamp ?? DateTime.UtcNow;
		}

		static int? GetReminder (TnefPropertySet properties)
		{
			// [MS-OXCICAL] 2.1.3.1.1.20.62: a VALARM is exported when PidLidReminderSet is TRUE.
			if (GetBoolean (properties, TnefNameId.ReminderSet) != true)
				return null;

			return NormalizeReminder (GetInt32 (properties, TnefNameId.ReminderDelta) ?? ReminderDeltaDefault);
		}

		static int NormalizeReminder (int minutes)
		{
			if (minutes == ReminderDeltaDefault)
				return 15;

			return Math.Max (0, minutes);
		}

		CalendarTime? GetRecurrenceId (TnefPropertySet properties, TnefTimeZone? startZone, bool allDay)
		{
			// [MS-OXCICAL] 2.1.3.1.1.20.20: a meeting message for a single instance of a recurring series identifies
			// the instance with RECURRENCE-ID, in the time zone of the series.
			var replaceTime = GetDateTime (properties, TnefNameId.ExceptionReplaceTime);
			bool isException = GetBoolean (properties, TnefNameId.IsException) == true;

			if (!replaceTime.HasValue && !isException)
				return null;

			var zone = GetRecurrenceTimeZone (properties) ?? startZone;

			if (replaceTime.HasValue)
				return FromUtc (replaceTime.Value, zone, allDay);

			// Fall back to the instance date in the GlobalObjectId and PidLidStartRecurrenceTime.
			var id = GetBytes (properties, TnefNameId.GlobalObjectId);

			if (id is null || id.Length < 20)
				return null;

			int year = (id[16] << 8) | id[17];
			int month = id[18];
			int day = id[19];

			if (year < 1601 || year > 9999 || month < 1 || month > 12 || day < 1 || day > DateTime.DaysInMonth (year, month))
				return null;

			var date = new DateTime (year, month, day);

			if (allDay)
				return new CalendarTime { Value = date, IsDate = true };

			var time = GetInt32 (properties, TnefNameId.StartRecurrenceTime);

			if (time.HasValue) {
				int hour = (time.Value >> 12) & 0x1F;
				int minute = (time.Value >> 6) & 0x3F;
				int second = time.Value & 0x3F;

				if (hour < 24 && minute < 60 && second < 60)
					date = date.Add (new TimeSpan (hour, minute, second));
			}

			if (zone is null)
				return new CalendarTime { Value = DateTime.SpecifyKind (date, DateTimeKind.Utc) };

			return FromLocal (date, zone, false);
		}

		static bool IsGregorian (ushort calendarType)
		{
			// [MS-OXOCAL] 2.2.1.44.1: the Gregorian calendar types (CAL_DEFAULT, CAL_GREGORIAN and its localized
			// variants), plus the Japanese, Taiwan, Korean and Thai calendars, which use Gregorian months and days.
			switch (calendarType) {
			case 0x00: case 0x01: case 0x02: case 0x03: case 0x04: case 0x05: case 0x07:
			case 0x09: case 0x0A: case 0x0B: case 0x0C:
				return true;
			default:
				return false;
			}
		}

		static void AppendDays (StringBuilder rule, uint mask)
		{
			bool first = true;

			for (int i = 0; i < 7; i++) {
				if ((mask & (1u << i)) == 0)
					continue;

				if (!first)
					rule.Append (',');

				rule.Append (TnefRecurrence.WeekDays[i]);
				first = false;
			}
		}

		string? BuildRecurrenceRule (TnefRecurrence recurrence, TnefTimeZone zone, bool allDay)
		{
			var inv = CultureInfo.InvariantCulture;
			var pattern = recurrence.PatternType;
			var rule = new StringBuilder ("FREQ=");
			uint interval;

			if (pattern >= TnefRecurrence.PatternHjMonth) {
				// [MS-OXCICAL] 2.1.3.2.1: the Hijri patterns need X-MICROSOFT-RRULE unless the calendar is Gregorian.
				if (recurrence.CalendarType == 0 || !IsGregorian (recurrence.CalendarType)) {
					addLoss (TnefConversionLossKind.UnsupportedCalendarData, "The recurrence pattern uses the Hijri calendar, which iCalendar cannot represent, so the appointment was exported as a single occurrence.");
					return null;
				}

				pattern = (ushort) (pattern - TnefRecurrence.PatternHjMonth + TnefRecurrence.PatternMonth);
			} else if (!IsGregorian (recurrence.CalendarType)) {
				addLoss (TnefConversionLossKind.UnsupportedCalendarData, $"The recurrence pattern uses a non-Gregorian calendar (0x{recurrence.CalendarType:X4}), which iCalendar cannot represent, so the appointment was exported as a single occurrence.");
				return null;
			}

			switch (pattern) {
			case TnefRecurrence.PatternDay:
				// The period of a daily recurrence is in minutes.
				if (recurrence.Period == 0 || recurrence.Period % 1440 != 0)
					return InvalidRecurrence ("the daily period is not a whole number of days");

				rule.Append ("DAILY");
				interval = recurrence.Period / 1440;
				break;
			case TnefRecurrence.PatternWeek:
				if (recurrence.Period == 0 || (recurrence.DayMask & 0x7F) == 0)
					return InvalidRecurrence ("the weekly pattern does not have any days");

				rule.Append ("WEEKLY;BYDAY=");
				AppendDays (rule, recurrence.DayMask);
				interval = recurrence.Period;
				break;
			case TnefRecurrence.PatternMonth:
			case TnefRecurrence.PatternMonthEnd:
			case TnefRecurrence.PatternMonthNth:
				if (recurrence.Period == 0)
					return InvalidRecurrence ("the monthly period is zero");

				bool yearly = recurrence.Period % 12 == 0;

				rule.Append (yearly ? "YEARLY" : "MONTHLY");

				if (pattern == TnefRecurrence.PatternMonthNth) {
					if ((recurrence.DayMask & 0x7F) == 0 || recurrence.WeekOfMonth < 1 || recurrence.WeekOfMonth > 5)
						return InvalidRecurrence ("the day of the month is not valid");

					rule.Append (";BYDAY=");
					AppendDays (rule, recurrence.DayMask);
				} else {
					int day = (int) recurrence.DayOfMonth;

					if (pattern == TnefRecurrence.PatternMonthEnd || day == 31)
						day = -1;
					else if (day < 1 || day > 31)
						return InvalidRecurrence ("the day of the month is not valid");

					rule.Append (";BYMONTHDAY=").Append (day.ToString (inv));
				}

				if (yearly)
					rule.Append (";BYMONTH=").Append (TnefRecurrence.ToDateTime (recurrence.StartDate).Month.ToString (inv));

				if (pattern == TnefRecurrence.PatternMonthNth)
					rule.Append (";BYSETPOS=").Append (recurrence.WeekOfMonth == 5 ? "-1" : recurrence.WeekOfMonth.ToString (inv));

				interval = yearly ? recurrence.Period / 12 : recurrence.Period;
				break;
			default:
				return InvalidRecurrence ($"the pattern type 0x{pattern:X4} is not known");
			}

			if (interval != 1)
				rule.Append (";INTERVAL=").Append (interval.ToString (inv));

			switch (recurrence.EndType) {
			case TnefRecurrence.EndAfterOccurrences:
				rule.Append (";COUNT=").Append (recurrence.OccurrenceCount.ToString (inv));
				break;
			case TnefRecurrence.EndAfterDate:
				// UNTIL is the start of the last instance, in UTC unless the appointment is all-day.
				var until = TnefRecurrence.ToDateTime (recurrence.EndDate);

				if (allDay)
					rule.Append (";UNTIL=").Append (CalendarWriter.FormatDate (until));
				else
					rule.Append (";UNTIL=").Append (CalendarWriter.FormatDateTime (zone.ToUniversalTime (until.AddMinutes (recurrence.StartTimeOffset)), true));
				break;
			}

			if (pattern == TnefRecurrence.PatternWeek && recurrence.Period > 1 && recurrence.FirstDayOfWeek < 7)
				rule.Append (";WKST=").Append (TnefRecurrence.WeekDays[recurrence.FirstDayOfWeek]);

			return rule.ToString ();
		}

		string? InvalidRecurrence (string reason)
		{
			addLoss (TnefConversionLossKind.InvalidCalendarData, $"The recurrence pattern is not valid ({reason}), so the appointment was exported as a single occurrence.");
			return null;
		}

		void AddRecurrence (CalendarEvent master, TnefRecurrence recurrence, TnefTimeZone zone, bool allDay)
		{
			master.RecurrenceRule = BuildRecurrenceRule (recurrence, zone, allDay);

			if (master.RecurrenceRule is null)
				return;

			if (recurrence.ExceptionError != null)
				addLoss (TnefConversionLossKind.InvalidCalendarData, $"The exceptions to the recurring appointment could not be parsed: {recurrence.ExceptionError}");

			// [MS-OXCICAL] 2.1.3.1.1.20.11: the deleted instances that are not modified instances become EXDATEs.
			var modifiedDates = new HashSet<DateTime> ();

			foreach (var exception in recurrence.Exceptions)
				modifiedDates.Add (TnefRecurrence.ToDateTime (exception.OriginalStartDate).Date);

			foreach (var deleted in recurrence.DeletedInstanceDates) {
				var date = TnefRecurrence.ToDateTime (deleted).Date;

				if (!modifiedDates.Contains (date))
					master.ExDates.Add (FromLocal (date.AddMinutes (recurrence.StartTimeOffset), zone, allDay));
			}

			if (recurrence.Exceptions.Count == 0)
				return;

			int count = Math.Min (recurrence.Exceptions.Count, maxExceptions);

			if (count < recurrence.Exceptions.Count)
				addLoss (TnefConversionLossKind.CalendarExceptionLimitExceeded, $"The recurring appointment has {recurrence.Exceptions.Count} modified occurrences, but only the first {count} were exported (see TnefConversionOptions.MaxCalendarExceptions).");

			if (count == 0)
				return;

			var attachments = LoadExceptionAttachments ();

			try {
				for (int i = 0; i < count; i++) {
					cancellationToken.ThrowIfCancellationRequested ();

					if (!AddException (master, recurrence.Exceptions[i], zone, allDay, attachments)) {
						addLoss (TnefConversionLossKind.CalendarExceptionLimitExceeded, $"The modified occurrences of the recurring appointment exceed {maxExceptionsSize} bytes, so only the first {i} of {recurrence.Exceptions.Count} were exported (see TnefConversionOptions.MaxCalendarExceptionsSize).");
						break;
					}
				}
			} finally {
				foreach (var attachment in attachments)
					attachment.Message.Dispose ();
			}
		}

		sealed class ExceptionAttachment
		{
			public TnefAttachment Attachment;
			public TnefMessage Message;
			public bool Used;

			public ExceptionAttachment (TnefAttachment attachment, TnefMessage message)
			{
				Attachment = attachment;
				Message = message;
			}
		}

		List<ExceptionAttachment> LoadExceptionAttachments ()
		{
			var attachments = new List<ExceptionAttachment> ();

			foreach (var attachment in tnef.Attachments) {
				int flags = attachment.Properties.GetInt32 (TnefPropertyTag.AttachmentFlags) ?? 0;

				if ((flags & AttachmentFlagException) == 0 || !attachment.IsEmbeddedMessage)
					continue;

				try {
					attachments.Add (new ExceptionAttachment (attachment, attachment.LoadEmbeddedMessage (cancellationToken)));
				} catch (TnefException ex) {
					addLoss (TnefConversionLossKind.InvalidCalendarData, $"An exception to the recurring appointment could not be loaded: {ex.Message}");
				}
			}

			return attachments;
		}

		static ExceptionAttachment? FindExceptionAttachment (List<ExceptionAttachment> attachments, DateTime originalUtc, DateTime startLocal)
		{
			// [MS-OXOCAL] 2.2.10.1.3: the exception's PidLidExceptionReplaceTime is the original start time in UTC.
			foreach (var attachment in attachments) {
				if (attachment.Used)
					continue;

				var replaceTime = GetDateTime (attachment.Message.Properties, TnefNameId.ExceptionReplaceTime);

				if (replaceTime.HasValue && Math.Abs ((replaceTime.Value - originalUtc).Ticks) < TimeSpan.TicksPerMinute)
					return attachment;
			}

			// Otherwise, PidTagExceptionStartTime on the attachment is the start of the exception in local time.
			foreach (var attachment in attachments) {
				if (attachment.Used || attachment.Message.Properties.TryGetValue (TnefNameId.ExceptionReplaceTime, out _))
					continue;

				var startTime = attachment.Attachment.Properties.GetDateTime (TnefPropertyTag.ExceptionStartTime);

				if (startTime.HasValue && Math.Abs ((startTime.Value.Ticks - startLocal.Ticks)) < TimeSpan.TicksPerMinute)
					return attachment;
			}

			return null;
		}

		bool AddException (CalendarEvent master, TnefRecurrenceException exception, TnefTimeZone zone, bool allDay, List<ExceptionAttachment> attachments)
		{
			var ev = master.CloneForException ();
			var originalLocal = TnefRecurrence.ToDateTime (exception.OriginalStartDate);
			var startLocal = TnefRecurrence.ToDateTime (exception.StartDateTime);
			var endLocal = TnefRecurrence.ToDateTime (exception.EndDateTime);
			bool exceptionAllDay = exception.HasOverride (TnefRecurrenceException.SubTypeFlag) ? exception.IsAllDay : allDay;

			// [MS-OXCICAL] 2.1.3.1.1.20.20: the RECURRENCE-ID is the original start, local to the series' time zone, or a
			// DATE when the series is all-day.
			ev.RecurrenceId = FromLocal (originalLocal, zone, allDay);
			ev.Start = FromLocal (startLocal, zone, exceptionAllDay);
			ev.End = FromLocal (endLocal, zone, exceptionAllDay);

			if (exception.HasOverride (TnefRecurrenceException.SubjectFlag))
				ev.Summary = exception.Subject ?? string.Empty;

			if (exception.HasOverride (TnefRecurrenceException.LocationFlag))
				ev.Location = exception.Location;

			if (exception.HasOverride (TnefRecurrenceException.BusyStatusFlag))
				ev.BusyStatus = exception.BusyStatus;

			bool reminder = exception.HasOverride (TnefRecurrenceException.ReminderFlag) ? exception.ReminderSet : master.ReminderMinutes.HasValue;

			if (!reminder)
				ev.ReminderMinutes = null;
			else if (exception.HasOverride (TnefRecurrenceException.ReminderDeltaFlag))
				ev.ReminderMinutes = NormalizeReminder (exception.ReminderDelta);
			else
				ev.ReminderMinutes = master.ReminderMinutes ?? 15;

			// The remaining properties come from the exception's embedded Calendar object, if it has one.
			var attachment = FindExceptionAttachment (attachments, zone.ToUniversalTime (originalLocal), startLocal);

			if (attachment != null) {
				var properties = attachment.Message.Properties;

				if (GetSubject (attachment.Message) is string subject)
					ev.Summary = subject;

				if (GetString (properties, TnefNameId.Location) is string location)
					ev.Location = location;

				if (GetBodyText (attachment.Message) is string description)
					ev.Description = description;

				if (GetInt32 (properties, TnefNameId.BusyStatus) is int busyStatus)
					ev.BusyStatus = busyStatus;

				if (GetBoolean (properties, TnefNameId.ReminderSet) is bool reminderSet)
					ev.ReminderMinutes = reminderSet ? GetReminder (properties) : null;

				if (GetBoolean (properties, TnefNameId.AppointmentSubType) is bool subType)
					exceptionAllDay = subType;

				if (GetDateTime (properties, TnefNameId.AppointmentStartWhole) is DateTime start)
					ev.Start = FromUtc (start, zone, exceptionAllDay);

				if (GetDateTime (properties, TnefNameId.AppointmentEndWhole) is DateTime end)
					ev.End = FromUtc (end, zone, exceptionAllDay);
			}

			// Each exception repeats most of the master's properties, so a few bytes of recurrence data can expand
			// into a very large VEVENT. Measure it before committing to it.
			long size = MeasureEvent (ev);

			if (size > maxExceptionsSize - exceptionsSize)
				return false;

			exceptionsSize += size;

			if (attachment != null) {
				attachment.Used = true;
				exceptionAttachments.Add (attachment.Attachment);
			}

			events.Add (ev);

			return true;
		}

		long MeasureEvent (CalendarEvent ev)
		{
			using (var measure = new MeasuringStream ()) {
				WriteEvent (new CalendarWriter (measure), ev);

				return measure.Length;
			}
		}

		#endregion

		#region Writing

		static void WriteTime (CalendarWriter writer, string name, CalendarTime time)
		{
			writer.BeginProperty (name);

			if (time.IsDate) {
				writer.WriteParameter ("VALUE", "DATE");
				writer.WriteValue (CalendarWriter.FormatDate (time.Value));
			} else if (time.Zone != null) {
				writer.WriteParameter ("TZID", time.Zone.Id);
				writer.WriteValue (CalendarWriter.FormatDateTime (time.Value, false));
			} else {
				writer.WriteValue (CalendarWriter.FormatDateTime (time.Value, true));
			}
		}

		static void WriteExDates (CalendarWriter writer, List<CalendarTime> dates)
		{
			// Each EXDATE is written on its own line, since all of the values of one EXDATE must share a TZID.
			foreach (var date in dates)
				WriteTime (writer, "EXDATE", date);
		}

		static void WriteAddress (CalendarWriter writer, string name, CalendarAddress address)
		{
			writer.BeginProperty (name);

			if (address.CuType != null)
				writer.WriteParameter ("CUTYPE", address.CuType);

			if (address.Role != null)
				writer.WriteParameter ("ROLE", address.Role);

			if (address.PartStat != null)
				writer.WriteParameter ("PARTSTAT", address.PartStat);

			if (address.Rsvp.HasValue)
				writer.WriteParameter ("RSVP", address.Rsvp.Value ? "TRUE" : "FALSE");

			if (address.Name != null)
				writer.WriteParameter ("CN", address.Name);

			writer.WriteValue (address.Address);
		}

		static string FormatInteger (int value)
		{
			return value.ToString (CultureInfo.InvariantCulture);
		}

		void WriteEvent (CalendarWriter writer, CalendarEvent ev)
		{
			writer.BeginComponent ("VEVENT");

			foreach (var attendee in ev.Attendees)
				WriteAddress (writer, "ATTENDEE", attendee);

			if (ev.Categories != null) {
				var categories = new StringBuilder ();

				foreach (var category in ev.Categories) {
					if (string.IsNullOrEmpty (category))
						continue;

					if (categories.Length > 0)
						categories.Append (',');

					CalendarWriter.AppendText (categories, category);
				}

				if (categories.Length > 0)
					writer.WriteProperty ("CATEGORIES", categories.ToString ());
			}

			if (ev.Class != null)
				writer.WriteProperty ("CLASS", ev.Class);

			// [MS-OXCICAL] 2.1.3.1.1.20.5 and 2.1.3.1.1.20.7: the body of a response is a COMMENT, otherwise it is the
			// DESCRIPTION.
			if (ev.Description != null && IsReply)
				writer.WriteTextProperty ("COMMENT", ev.Description);

			if (ev.Created.HasValue)
				writer.WriteProperty ("CREATED", CalendarWriter.FormatDateTime (ev.Created.Value, true));

			if (ev.Description != null && !IsReply)
				writer.WriteTextProperty ("DESCRIPTION", ev.Description);

			if (ev.End.HasValue)
				WriteTime (writer, "DTEND", ev.End.Value);

			writer.WriteProperty ("DTSTAMP", CalendarWriter.FormatDateTime (ev.Stamp, true));
			WriteTime (writer, "DTSTART", ev.Start);
			WriteExDates (writer, ev.ExDates);

			if (ev.LastModified.HasValue)
				writer.WriteProperty ("LAST-MODIFIED", CalendarWriter.FormatDateTime (ev.LastModified.Value, true));

			if (!string.IsNullOrEmpty (ev.Location))
				writer.WriteTextProperty ("LOCATION", ev.Location!);

			if (ev.Organizer != null)
				WriteAddress (writer, "ORGANIZER", ev.Organizer);

			if (ev.Priority.HasValue)
				writer.WriteProperty ("PRIORITY", FormatInteger (ev.Priority.Value));

			if (ev.RecurrenceId.HasValue)
				WriteTime (writer, "RECURRENCE-ID", ev.RecurrenceId.Value);

			if (ev.RecurrenceRule != null)
				writer.WriteProperty ("RRULE", ev.RecurrenceRule);

			writer.WriteProperty ("SEQUENCE", FormatInteger (ev.Sequence));

			if (Method == "CANCEL")
				writer.WriteProperty ("STATUS", "CANCELLED");

			writer.WriteTextProperty ("SUMMARY", ev.Summary);

			// [MS-OXCICAL] 2.1.3.1.1.20.25: only a free appointment is transparent.
			if (ev.BusyStatus.HasValue)
				writer.WriteProperty ("TRANSP", ev.BusyStatus.Value == 0 ? "TRANSPARENT" : "OPAQUE");

			writer.WriteTextProperty ("UID", ev.Uid);

			if (ev.BusyStatus is int busyStatus && busyStatus >= 0 && busyStatus < BusyStatusNames.Length)
				writer.WriteProperty ("X-MICROSOFT-CDO-BUSYSTATUS", BusyStatusNames[busyStatus]);

			if (ev.Importance.HasValue)
				writer.WriteProperty ("X-MICROSOFT-CDO-IMPORTANCE", FormatInteger (ev.Importance.Value));

			if (ev.IntendedBusyStatus is int intended && intended >= 0 && intended < BusyStatusNames.Length)
				writer.WriteProperty ("X-MICROSOFT-CDO-INTENDEDSTATUS", BusyStatusNames[intended]);

			if (ev.OwnerAppointmentId.HasValue)
				writer.WriteProperty ("X-MICROSOFT-CDO-OWNERAPPTID", FormatInteger (ev.OwnerAppointmentId.Value));

			if (ev.DisallowCounter.HasValue)
				writer.WriteProperty ("X-MICROSOFT-DISALLOW-COUNTER", ev.DisallowCounter.Value ? "TRUE" : "FALSE");

			if (ev.OriginalEnd.HasValue)
				WriteTime (writer, "X-MS-OLK-ORIGINALEND", ev.OriginalEnd.Value);

			if (ev.OriginalStart.HasValue)
				WriteTime (writer, "X-MS-OLK-ORIGINALSTART", ev.OriginalStart.Value);

			if (ev.ReminderMinutes is int minutes) {
				writer.BeginComponent ("VALARM");
				writer.WriteProperty ("ACTION", "DISPLAY");
				writer.WriteProperty ("DESCRIPTION", "Reminder");
				writer.WriteProperty ("TRIGGER", minutes == 0 ? "PT0M" : "-PT" + FormatInteger (minutes) + "M");
				writer.EndComponent ("VALARM");
			}

			writer.EndComponent ("VEVENT");
		}

		/// <summary>
		/// Write the iCalendar object to the stream, in UTF-8.
		/// </summary>
		public void WriteTo (Stream stream)
		{
			var writer = new CalendarWriter (stream);

			writer.BeginComponent ("VCALENDAR");
			writer.WriteProperty ("METHOD", Method);
			writer.WriteTextProperty ("PRODID", ProductId);
			writer.WriteProperty ("VERSION", "2.0");
			writer.WriteProperty ("X-MS-OLK-FORCEINSPECTOROPEN", "TRUE");

			foreach (var zone in timeZones)
				zone.WriteTo (writer);

			foreach (var ev in events)
				WriteEvent (writer, ev);

			writer.EndComponent ("VCALENDAR");
		}

		#endregion
	}
}

//
// TnefNameId.cs
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
using System.Globalization;

namespace MimeKit.Tnef {
	/// <summary>
	/// A TNEF name identifier.
	/// </summary>
	/// <remarks>
	/// A TNEF name identifier.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\TnefExamples.cs" region="ReadProperties"/>
	/// </example>
	public readonly struct TnefNameId : IEquatable<TnefNameId>
	{
		readonly TnefNameIdKind kind;
		readonly string? name;
		readonly Guid guid;
		readonly int id;

		#region PSETID_Address

		/// <summary>
		/// The PidLidEmail1AddressType named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidEmail1AddressType</c> named property (PSETID_Address, LID 0x00008082) of type <c>PtypString</c>,
		/// as defined in [MS-OXPROPS] section 2.100.
		/// </remarks>
		public static readonly TnefNameId Email1AddressType = new TnefNameId (TnefPropertySetGuid.Address, 0x8082);

		/// <summary>
		/// The PidLidEmail1DisplayName named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidEmail1DisplayName</c> named property (PSETID_Address, LID 0x00008080) of type <c>PtypString</c>,
		/// as defined in [MS-OXPROPS] section 2.101.
		/// </remarks>
		public static readonly TnefNameId Email1DisplayName = new TnefNameId (TnefPropertySetGuid.Address, 0x8080);

		/// <summary>
		/// The PidLidEmail1EmailAddress named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidEmail1EmailAddress</c> named property (PSETID_Address, LID 0x00008083) of type <c>PtypString</c>,
		/// as defined in [MS-OXPROPS] section 2.102.
		/// </remarks>
		public static readonly TnefNameId Email1EmailAddress = new TnefNameId (TnefPropertySetGuid.Address, 0x8083);

		/// <summary>
		/// The PidLidFileUnder named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidFileUnder</c> named property (PSETID_Address, LID 0x00008005) of type <c>PtypString</c>,
		/// as defined in [MS-OXPROPS] section 2.132.
		/// </remarks>
		public static readonly TnefNameId FileUnder = new TnefNameId (TnefPropertySetGuid.Address, 0x8005);

		#endregion

		#region PSETID_Appointment

		/// <summary>
		/// The PidLidAllAttendeesString named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidAllAttendeesString</c> named property (PSETID_Appointment, LID 0x00008238) of type <c>PtypString</c>,
		/// as defined in [MS-OXPROPS] section 2.5.
		/// </remarks>
		public static readonly TnefNameId AllAttendeesString = new TnefNameId (TnefPropertySetGuid.Appointment, 0x8238);

		/// <summary>
		/// The PidLidAppointmentColor named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidAppointmentColor</c> named property (PSETID_Appointment, LID 0x00008214) of type <c>PtypInteger32</c>,
		/// as defined in [MS-OXPROPS] section 2.9.
		/// </remarks>
		public static readonly TnefNameId AppointmentColor = new TnefNameId (TnefPropertySetGuid.Appointment, 0x8214);

		/// <summary>
		/// The PidLidAppointmentDuration named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidAppointmentDuration</c> named property (PSETID_Appointment, LID 0x00008213) of type <c>PtypInteger32</c>,
		/// as defined in [MS-OXPROPS] section 2.11.
		/// </remarks>
		public static readonly TnefNameId AppointmentDuration = new TnefNameId (TnefPropertySetGuid.Appointment, 0x8213);

		/// <summary>
		/// The PidLidAppointmentEndWhole named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidAppointmentEndWhole</c> named property (PSETID_Appointment, LID 0x0000820E) of type <c>PtypTime</c>,
		/// as defined in [MS-OXPROPS] section 2.14.
		/// </remarks>
		public static readonly TnefNameId AppointmentEndWhole = new TnefNameId (TnefPropertySetGuid.Appointment, 0x820E);

		/// <summary>
		/// The PidLidAppointmentNotAllowPropose named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidAppointmentNotAllowPropose</c> named property (PSETID_Appointment, LID 0x0000825A) of type <c>PtypBoolean</c>,
		/// as defined in [MS-OXPROPS] section 2.17.
		/// </remarks>
		public static readonly TnefNameId AppointmentNotAllowPropose = new TnefNameId (TnefPropertySetGuid.Appointment, 0x825A);

		/// <summary>
		/// The PidLidAppointmentRecur named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidAppointmentRecur</c> named property (PSETID_Appointment, LID 0x00008216) of type <c>PtypBinary</c>,
		/// as defined in [MS-OXPROPS] section 2.22.
		/// </remarks>
		public static readonly TnefNameId AppointmentRecur = new TnefNameId (TnefPropertySetGuid.Appointment, 0x8216);

		/// <summary>
		/// The PidLidAppointmentSequence named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidAppointmentSequence</c> named property (PSETID_Appointment, LID 0x00008201) of type <c>PtypInteger32</c>,
		/// as defined in [MS-OXPROPS] section 2.25.
		/// </remarks>
		public static readonly TnefNameId AppointmentSequence = new TnefNameId (TnefPropertySetGuid.Appointment, 0x8201);

		/// <summary>
		/// The PidLidAppointmentStartWhole named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidAppointmentStartWhole</c> named property (PSETID_Appointment, LID 0x0000820D) of type <c>PtypTime</c>,
		/// as defined in [MS-OXPROPS] section 2.29.
		/// </remarks>
		public static readonly TnefNameId AppointmentStartWhole = new TnefNameId (TnefPropertySetGuid.Appointment, 0x820D);

		/// <summary>
		/// The PidLidAppointmentStateFlags named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidAppointmentStateFlags</c> named property (PSETID_Appointment, LID 0x00008217) of type <c>PtypInteger32</c>,
		/// as defined in [MS-OXPROPS] section 2.30.
		/// </remarks>
		public static readonly TnefNameId AppointmentStateFlags = new TnefNameId (TnefPropertySetGuid.Appointment, 0x8217);

		/// <summary>
		/// The PidLidAppointmentSubType named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidAppointmentSubType</c> named property (PSETID_Appointment, LID 0x00008215) of type <c>PtypBoolean</c>,
		/// as defined in [MS-OXPROPS] section 2.31.
		/// </remarks>
		public static readonly TnefNameId AppointmentSubType = new TnefNameId (TnefPropertySetGuid.Appointment, 0x8215);

		/// <summary>
		/// The PidLidAppointmentTimeZoneDefinitionEndDisplay named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidAppointmentTimeZoneDefinitionEndDisplay</c> named property (PSETID_Appointment, LID 0x0000825F) of type <c>PtypBinary</c>,
		/// as defined in [MS-OXPROPS] section 2.32.
		/// </remarks>
		public static readonly TnefNameId AppointmentTimeZoneDefinitionEndDisplay = new TnefNameId (TnefPropertySetGuid.Appointment, 0x825F);

		/// <summary>
		/// The PidLidAppointmentTimeZoneDefinitionRecur named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidAppointmentTimeZoneDefinitionRecur</c> named property (PSETID_Appointment, LID 0x00008260) of type <c>PtypBinary</c>,
		/// as defined in [MS-OXPROPS] section 2.33.
		/// </remarks>
		public static readonly TnefNameId AppointmentTimeZoneDefinitionRecur = new TnefNameId (TnefPropertySetGuid.Appointment, 0x8260);

		/// <summary>
		/// The PidLidAppointmentTimeZoneDefinitionStartDisplay named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidAppointmentTimeZoneDefinitionStartDisplay</c> named property (PSETID_Appointment, LID 0x0000825E) of type <c>PtypBinary</c>,
		/// as defined in [MS-OXPROPS] section 2.34.
		/// </remarks>
		public static readonly TnefNameId AppointmentTimeZoneDefinitionStartDisplay = new TnefNameId (TnefPropertySetGuid.Appointment, 0x825E);

		/// <summary>
		/// The PidLidBusyStatus named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidBusyStatus</c> named property (PSETID_Appointment, LID 0x00008205) of type <c>PtypInteger32</c>,
		/// as defined in [MS-OXPROPS] section 2.47.
		/// </remarks>
		public static readonly TnefNameId BusyStatus = new TnefNameId (TnefPropertySetGuid.Appointment, 0x8205);

		/// <summary>
		/// The PidLidCcAttendeesString named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidCcAttendeesString</c> named property (PSETID_Appointment, LID 0x0000823C) of type <c>PtypString</c>,
		/// as defined in [MS-OXPROPS] section 2.50.
		/// </remarks>
		public static readonly TnefNameId CcAttendeesString = new TnefNameId (TnefPropertySetGuid.Appointment, 0x823C);

		/// <summary>
		/// The PidLidClipEnd named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidClipEnd</c> named property (PSETID_Appointment, LID 0x00008236) of type <c>PtypTime</c>,
		/// as defined in [MS-OXPROPS] section 2.59.
		/// </remarks>
		public static readonly TnefNameId ClipEnd = new TnefNameId (TnefPropertySetGuid.Appointment, 0x8236);

		/// <summary>
		/// The PidLidClipStart named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidClipStart</c> named property (PSETID_Appointment, LID 0x00008235) of type <c>PtypTime</c>,
		/// as defined in [MS-OXPROPS] section 2.60.
		/// </remarks>
		public static readonly TnefNameId ClipStart = new TnefNameId (TnefPropertySetGuid.Appointment, 0x8235);

		/// <summary>
		/// The PidLidExceptionReplaceTime named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidExceptionReplaceTime</c> named property (PSETID_Appointment, LID 0x00008228) of type <c>PtypTime</c>,
		/// as defined in [MS-OXPROPS] section 2.117.
		/// </remarks>
		public static readonly TnefNameId ExceptionReplaceTime = new TnefNameId (TnefPropertySetGuid.Appointment, 0x8228);

		/// <summary>
		/// The PidLidIntendedBusyStatus named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidIntendedBusyStatus</c> named property (PSETID_Appointment, LID 0x00008224) of type <c>PtypInteger32</c>,
		/// as defined in [MS-OXPROPS] section 2.151.
		/// </remarks>
		public static readonly TnefNameId IntendedBusyStatus = new TnefNameId (TnefPropertySetGuid.Appointment, 0x8224);

		/// <summary>
		/// The PidLidLocation named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidLocation</c> named property (PSETID_Appointment, LID 0x00008208) of type <c>PtypString</c>,
		/// as defined in [MS-OXPROPS] section 2.159.
		/// </remarks>
		public static readonly TnefNameId Location = new TnefNameId (TnefPropertySetGuid.Appointment, 0x8208);

		/// <summary>
		/// The PidLidRecurrencePattern named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidRecurrencePattern</c> named property (PSETID_Appointment, LID 0x00008232) of type <c>PtypString</c>,
		/// as defined in [MS-OXPROPS] section 2.214.
		/// </remarks>
		public static readonly TnefNameId RecurrencePattern = new TnefNameId (TnefPropertySetGuid.Appointment, 0x8232);

		/// <summary>
		/// The PidLidRecurrenceType named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidRecurrenceType</c> named property (PSETID_Appointment, LID 0x00008231) of type <c>PtypInteger32</c>,
		/// as defined in [MS-OXPROPS] section 2.215.
		/// </remarks>
		public static readonly TnefNameId RecurrenceType = new TnefNameId (TnefPropertySetGuid.Appointment, 0x8231);

		/// <summary>
		/// The PidLidRecurring named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidRecurring</c> named property (PSETID_Appointment, LID 0x00008223) of type <c>PtypBoolean</c>,
		/// as defined in [MS-OXPROPS] section 2.216.
		/// </remarks>
		public static readonly TnefNameId Recurring = new TnefNameId (TnefPropertySetGuid.Appointment, 0x8223);

		/// <summary>
		/// The PidLidResponseStatus named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidResponseStatus</c> named property (PSETID_Appointment, LID 0x00008218) of type <c>PtypInteger32</c>,
		/// as defined in [MS-OXPROPS] section 2.231.
		/// </remarks>
		public static readonly TnefNameId ResponseStatus = new TnefNameId (TnefPropertySetGuid.Appointment, 0x8218);

		/// <summary>
		/// The PidLidTimeZoneDescription named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidTimeZoneDescription</c> named property (PSETID_Appointment, LID 0x00008234) of type <c>PtypString</c>,
		/// as defined in [MS-OXPROPS] section 2.341.
		/// </remarks>
		public static readonly TnefNameId TimeZoneDescription = new TnefNameId (TnefPropertySetGuid.Appointment, 0x8234);

		/// <summary>
		/// The PidLidTimeZoneStruct named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidTimeZoneStruct</c> named property (PSETID_Appointment, LID 0x00008233) of type <c>PtypBinary</c>,
		/// as defined in [MS-OXPROPS] section 2.342.
		/// </remarks>
		public static readonly TnefNameId TimeZoneStruct = new TnefNameId (TnefPropertySetGuid.Appointment, 0x8233);

		/// <summary>
		/// The PidLidToAttendeesString named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidToAttendeesString</c> named property (PSETID_Appointment, LID 0x0000823B) of type <c>PtypString</c>,
		/// as defined in [MS-OXPROPS] section 2.343.
		/// </remarks>
		public static readonly TnefNameId ToAttendeesString = new TnefNameId (TnefPropertySetGuid.Appointment, 0x823B);

		#endregion

		#region PSETID_Attachment

		/// <summary>
		/// The PidNameAttachmentMacContentType named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidNameAttachmentMacContentType</c> named property (PSETID_Attachment, name &quot;AttachmentMacContentType&quot;) of type <c>PtypString</c>,
		/// as defined in [MS-OXPROPS] section 2.368.
		/// </remarks>
		public static readonly TnefNameId AttachmentMacContentType = new TnefNameId (TnefPropertySetGuid.Attachment, "AttachmentMacContentType");

		/// <summary>
		/// The PidNameAttachmentMacInfo named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidNameAttachmentMacInfo</c> named property (PSETID_Attachment, name &quot;AttachmentMacInfo&quot;) of type <c>PtypBinary</c>,
		/// as defined in [MS-OXPROPS] section 2.369.
		/// </remarks>
		public static readonly TnefNameId AttachmentMacInfo = new TnefNameId (TnefPropertySetGuid.Attachment, "AttachmentMacInfo");

		/// <summary>
		/// The PidNameAttachmentOriginalPermissionType named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidNameAttachmentOriginalPermissionType</c> named property (PSETID_Attachment, name &quot;AttachmentOriginalPermissionType&quot;) of type <c>PtypInteger32</c>,
		/// as defined in [MS-OXPROPS] section 2.370.
		/// </remarks>
		public static readonly TnefNameId AttachmentOriginalPermissionType = new TnefNameId (TnefPropertySetGuid.Attachment, "AttachmentOriginalPermissionType");

		/// <summary>
		/// The PidNameAttachmentPermissionType named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidNameAttachmentPermissionType</c> named property (PSETID_Attachment, name &quot;AttachmentPermissionType&quot;) of type <c>PtypInteger32</c>,
		/// as defined in [MS-OXPROPS] section 2.371.
		/// </remarks>
		public static readonly TnefNameId AttachmentPermissionType = new TnefNameId (TnefPropertySetGuid.Attachment, "AttachmentPermissionType");

		/// <summary>
		/// The PidNameAttachmentProviderType named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidNameAttachmentProviderType</c> named property (PSETID_Attachment, name &quot;AttachmentProviderType&quot;) of type <c>PtypString</c>,
		/// as defined in [MS-OXPROPS] section 2.372.
		/// </remarks>
		public static readonly TnefNameId AttachmentProviderType = new TnefNameId (TnefPropertySetGuid.Attachment, "AttachmentProviderType");

		#endregion

		#region PSETID_Common

		/// <summary>
		/// The PidLidClassified named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidClassified</c> named property (PSETID_Common, LID 0x000085B5) of type <c>PtypBoolean</c>,
		/// as defined in [MS-OXPROPS] section 2.56.
		/// </remarks>
		public static readonly TnefNameId Classified = new TnefNameId (TnefPropertySetGuid.Common, 0x85B5);

		/// <summary>
		/// The PidLidCommonEnd named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidCommonEnd</c> named property (PSETID_Common, LID 0x00008517) of type <c>PtypTime</c>,
		/// as defined in [MS-OXPROPS] section 2.62.
		/// </remarks>
		public static readonly TnefNameId CommonEnd = new TnefNameId (TnefPropertySetGuid.Common, 0x8517);

		/// <summary>
		/// The PidLidCommonStart named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidCommonStart</c> named property (PSETID_Common, LID 0x00008516) of type <c>PtypTime</c>,
		/// as defined in [MS-OXPROPS] section 2.63.
		/// </remarks>
		public static readonly TnefNameId CommonStart = new TnefNameId (TnefPropertySetGuid.Common, 0x8516);

		/// <summary>
		/// The PidLidCompanies named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidCompanies</c> named property (PSETID_Common, LID 0x00008539) of type <c>PtypMultipleString</c>,
		/// as defined in [MS-OXPROPS] section 2.64.
		/// </remarks>
		public static readonly TnefNameId Companies = new TnefNameId (TnefPropertySetGuid.Common, 0x8539);

		/// <summary>
		/// The PidLidContacts named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidContacts</c> named property (PSETID_Common, LID 0x0000853A) of type <c>PtypMultipleString</c>,
		/// as defined in [MS-OXPROPS] section 2.77.
		/// </remarks>
		public static readonly TnefNameId Contacts = new TnefNameId (TnefPropertySetGuid.Common, 0x853A);

		/// <summary>
		/// The PidLidCurrentVersion named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidCurrentVersion</c> named property (PSETID_Common, LID 0x00008552) of type <c>PtypInteger32</c>,
		/// as defined in [MS-OXPROPS] section 2.88.
		/// </remarks>
		public static readonly TnefNameId CurrentVersion = new TnefNameId (TnefPropertySetGuid.Common, 0x8552);

		/// <summary>
		/// The PidLidCurrentVersionName named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidCurrentVersionName</c> named property (PSETID_Common, LID 0x00008554) of type <c>PtypString</c>,
		/// as defined in [MS-OXPROPS] section 2.89.
		/// </remarks>
		public static readonly TnefNameId CurrentVersionName = new TnefNameId (TnefPropertySetGuid.Common, 0x8554);

		/// <summary>
		/// The PidLidFlagRequest named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidFlagRequest</c> named property (PSETID_Common, LID 0x00008530) of type <c>PtypString</c>,
		/// as defined in [MS-OXPROPS] section 2.136.
		/// </remarks>
		public static readonly TnefNameId FlagRequest = new TnefNameId (TnefPropertySetGuid.Common, 0x8530);

		/// <summary>
		/// The PidLidInternetAccountName named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidInternetAccountName</c> named property (PSETID_Common, LID 0x00008580) of type <c>PtypString</c>,
		/// as defined in [MS-OXPROPS] section 2.152.
		/// </remarks>
		public static readonly TnefNameId InternetAccountName = new TnefNameId (TnefPropertySetGuid.Common, 0x8580);

		/// <summary>
		/// The PidLidInternetAccountStamp named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidInternetAccountStamp</c> named property (PSETID_Common, LID 0x00008581) of type <c>PtypString</c>,
		/// as defined in [MS-OXPROPS] section 2.153.
		/// </remarks>
		public static readonly TnefNameId InternetAccountStamp = new TnefNameId (TnefPropertySetGuid.Common, 0x8581);

		/// <summary>
		/// The PidLidPrivate named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidPrivate</c> named property (PSETID_Common, LID 0x00008506) of type <c>PtypBoolean</c>,
		/// as defined in [MS-OXPROPS] section 2.211.
		/// </remarks>
		public static readonly TnefNameId Private = new TnefNameId (TnefPropertySetGuid.Common, 0x8506);

		/// <summary>
		/// The PidLidReminderDelta named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidReminderDelta</c> named property (PSETID_Common, LID 0x00008501) of type <c>PtypInteger32</c>,
		/// as defined in [MS-OXPROPS] section 2.218.
		/// </remarks>
		public static readonly TnefNameId ReminderDelta = new TnefNameId (TnefPropertySetGuid.Common, 0x8501);

		/// <summary>
		/// The PidLidReminderFileParameter named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidReminderFileParameter</c> named property (PSETID_Common, LID 0x0000851F) of type <c>PtypString</c>,
		/// as defined in [MS-OXPROPS] section 2.219.
		/// </remarks>
		public static readonly TnefNameId ReminderFileParameter = new TnefNameId (TnefPropertySetGuid.Common, 0x851F);

		/// <summary>
		/// The PidLidReminderOverride named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidReminderOverride</c> named property (PSETID_Common, LID 0x0000851C) of type <c>PtypBoolean</c>,
		/// as defined in [MS-OXPROPS] section 2.220.
		/// </remarks>
		public static readonly TnefNameId ReminderOverride = new TnefNameId (TnefPropertySetGuid.Common, 0x851C);

		/// <summary>
		/// The PidLidReminderPlaySound named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidReminderPlaySound</c> named property (PSETID_Common, LID 0x0000851E) of type <c>PtypBoolean</c>,
		/// as defined in [MS-OXPROPS] section 2.221.
		/// </remarks>
		public static readonly TnefNameId ReminderPlaySound = new TnefNameId (TnefPropertySetGuid.Common, 0x851E);

		/// <summary>
		/// The PidLidReminderSet named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidReminderSet</c> named property (PSETID_Common, LID 0x00008503) of type <c>PtypBoolean</c>,
		/// as defined in [MS-OXPROPS] section 2.222.
		/// </remarks>
		public static readonly TnefNameId ReminderSet = new TnefNameId (TnefPropertySetGuid.Common, 0x8503);

		/// <summary>
		/// The PidLidReminderSignalTime named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidReminderSignalTime</c> named property (PSETID_Common, LID 0x00008560) of type <c>PtypTime</c>,
		/// as defined in [MS-OXPROPS] section 2.223.
		/// </remarks>
		public static readonly TnefNameId ReminderSignalTime = new TnefNameId (TnefPropertySetGuid.Common, 0x8560);

		/// <summary>
		/// The PidLidReminderTime named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidReminderTime</c> named property (PSETID_Common, LID 0x00008502) of type <c>PtypTime</c>,
		/// as defined in [MS-OXPROPS] section 2.224.
		/// </remarks>
		public static readonly TnefNameId ReminderTime = new TnefNameId (TnefPropertySetGuid.Common, 0x8502);

		/// <summary>
		/// The PidLidSideEffects named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidSideEffects</c> named property (PSETID_Common, LID 0x00008510) of type <c>PtypInteger32</c>,
		/// as defined in [MS-OXPROPS] section 2.299.
		/// </remarks>
		public static readonly TnefNameId SideEffects = new TnefNameId (TnefPropertySetGuid.Common, 0x8510);

		/// <summary>
		/// The PidLidSmartNoAttach named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidSmartNoAttach</c> named property (PSETID_Common, LID 0x00008514) of type <c>PtypBoolean</c>,
		/// as defined in [MS-OXPROPS] section 2.301.
		/// </remarks>
		public static readonly TnefNameId SmartNoAttach = new TnefNameId (TnefPropertySetGuid.Common, 0x8514);

		/// <summary>
		/// The PidLidTaskMode named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidTaskMode</c> named property (PSETID_Common, LID 0x00008518) of type <c>PtypInteger32</c>,
		/// as defined in [MS-OXPROPS] section 2.324.
		/// </remarks>
		public static readonly TnefNameId TaskMode = new TnefNameId (TnefPropertySetGuid.Common, 0x8518);

		/// <summary>
		/// The PidLidToDoTitle named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidToDoTitle</c> named property (PSETID_Common, LID 0x000085A4) of type <c>PtypString</c>,
		/// as defined in [MS-OXPROPS] section 2.346.
		/// </remarks>
		public static readonly TnefNameId ToDoTitle = new TnefNameId (TnefPropertySetGuid.Common, 0x85A4);

		/// <summary>
		/// The PidLidUseTnef named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidUseTnef</c> named property (PSETID_Common, LID 0x00008582) of type <c>PtypBoolean</c>,
		/// as defined in [MS-OXPROPS] section 2.347. Specifies whether TNEF is to be included on a message
		/// when the message is converted from TNEF to MIME or SMTP format.
		/// </remarks>
		public static readonly TnefNameId UseTnef = new TnefNameId (TnefPropertySetGuid.Common, 0x8582);

		#endregion

		#region PSETID_Meeting

		/// <summary>
		/// The PidLidAttendeeCriticalChange named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidAttendeeCriticalChange</c> named property (PSETID_Meeting, LID 0x00000001) of type <c>PtypTime</c>,
		/// as defined in [MS-OXPROPS] section 2.37.
		/// </remarks>
		public static readonly TnefNameId AttendeeCriticalChange = new TnefNameId (TnefPropertySetGuid.Meeting, 0x0001);

		/// <summary>
		/// The PidLidCleanGlobalObjectId named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidCleanGlobalObjectId</c> named property (PSETID_Meeting, LID 0x00000023) of type <c>PtypBinary</c>,
		/// as defined in [MS-OXPROPS] section 2.57.
		/// </remarks>
		public static readonly TnefNameId CleanGlobalObjectId = new TnefNameId (TnefPropertySetGuid.Meeting, 0x0023);

		/// <summary>
		/// The PidLidGlobalObjectId named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidGlobalObjectId</c> named property (PSETID_Meeting, LID 0x00000003) of type <c>PtypBinary</c>,
		/// as defined in [MS-OXPROPS] section 2.142.
		/// </remarks>
		public static readonly TnefNameId GlobalObjectId = new TnefNameId (TnefPropertySetGuid.Meeting, 0x0003);

		/// <summary>
		/// The PidLidIsException named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidIsException</c> named property (PSETID_Meeting, LID 0x0000000A) of type <c>PtypBoolean</c>,
		/// as defined in [MS-OXPROPS] section 2.155.
		/// </remarks>
		public static readonly TnefNameId IsException = new TnefNameId (TnefPropertySetGuid.Meeting, 0x000A);

		/// <summary>
		/// The PidLidIsRecurring named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidIsRecurring</c> named property (PSETID_Meeting, LID 0x00000005) of type <c>PtypBoolean</c>,
		/// as defined in [MS-OXPROPS] section 2.156.
		/// </remarks>
		public static readonly TnefNameId IsRecurring = new TnefNameId (TnefPropertySetGuid.Meeting, 0x0005);

		/// <summary>
		/// The PidLidMeetingType named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidMeetingType</c> named property (PSETID_Meeting, LID 0x00000026) of type <c>PtypInteger32</c>,
		/// as defined in [MS-OXPROPS] section 2.170.
		/// </remarks>
		public static readonly TnefNameId MeetingType = new TnefNameId (TnefPropertySetGuid.Meeting, 0x0026);

		/// <summary>
		/// The PidLidOwnerCriticalChange named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidOwnerCriticalChange</c> named property (PSETID_Meeting, LID 0x0000001A) of type <c>PtypTime</c>,
		/// as defined in [MS-OXPROPS] section 2.199.
		/// </remarks>
		public static readonly TnefNameId OwnerCriticalChange = new TnefNameId (TnefPropertySetGuid.Meeting, 0x001A);

		/// <summary>
		/// The PidLidStartRecurrenceDate named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidStartRecurrenceDate</c> named property (PSETID_Meeting, LID 0x0000000D) of type <c>PtypInteger32</c>,
		/// as defined in [MS-OXPROPS] section 2.303.
		/// </remarks>
		public static readonly TnefNameId StartRecurrenceDate = new TnefNameId (TnefPropertySetGuid.Meeting, 0x000D);

		/// <summary>
		/// The PidLidStartRecurrenceTime named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidStartRecurrenceTime</c> named property (PSETID_Meeting, LID 0x0000000E) of type <c>PtypInteger32</c>,
		/// as defined in [MS-OXPROPS] section 2.304.
		/// </remarks>
		public static readonly TnefNameId StartRecurrenceTime = new TnefNameId (TnefPropertySetGuid.Meeting, 0x000E);

		#endregion

		#region PS_PUBLIC_STRINGS

		/// <summary>
		/// The PidNameKeywords named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidNameKeywords</c> named property (PS_PUBLIC_STRINGS, name &quot;Keywords&quot;) of type <c>PtypMultipleString</c>,
		/// as defined in [MS-OXPROPS] section 2.451.
		/// </remarks>
		public static readonly TnefNameId Keywords = new TnefNameId (TnefPropertySetGuid.PublicStrings, "Keywords");

		#endregion

		#region PSETID_Task

		/// <summary>
		/// The PidLidPercentComplete named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidPercentComplete</c> named property (PSETID_Task, LID 0x00008102) of type <c>PtypFloating64</c>,
		/// as defined in [MS-OXPROPS] section 2.202.
		/// </remarks>
		public static readonly TnefNameId PercentComplete = new TnefNameId (TnefPropertySetGuid.Task, 0x8102);

		/// <summary>
		/// The PidLidTaskAssigner named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidTaskAssigner</c> named property (PSETID_Task, LID 0x00008121) of type <c>PtypString</c>,
		/// as defined in [MS-OXPROPS] section 2.308.
		/// </remarks>
		public static readonly TnefNameId TaskAssigner = new TnefNameId (TnefPropertySetGuid.Task, 0x8121);

		/// <summary>
		/// The PidLidTaskComplete named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidTaskComplete</c> named property (PSETID_Task, LID 0x0000811C) of type <c>PtypBoolean</c>,
		/// as defined in [MS-OXPROPS] section 2.310.
		/// </remarks>
		public static readonly TnefNameId TaskComplete = new TnefNameId (TnefPropertySetGuid.Task, 0x811C);

		/// <summary>
		/// The PidLidTaskDateCompleted named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidTaskDateCompleted</c> named property (PSETID_Task, LID 0x0000810F) of type <c>PtypTime</c>,
		/// as defined in [MS-OXPROPS] section 2.312.
		/// </remarks>
		public static readonly TnefNameId TaskDateCompleted = new TnefNameId (TnefPropertySetGuid.Task, 0x810F);

		/// <summary>
		/// The PidLidTaskDueDate named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidTaskDueDate</c> named property (PSETID_Task, LID 0x00008105) of type <c>PtypTime</c>,
		/// as defined in [MS-OXPROPS] section 2.314.
		/// </remarks>
		public static readonly TnefNameId TaskDueDate = new TnefNameId (TnefPropertySetGuid.Task, 0x8105);

		/// <summary>
		/// The PidLidTaskOwner named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidTaskOwner</c> named property (PSETID_Task, LID 0x0000811F) of type <c>PtypString</c>,
		/// as defined in [MS-OXPROPS] section 2.328.
		/// </remarks>
		public static readonly TnefNameId TaskOwner = new TnefNameId (TnefPropertySetGuid.Task, 0x811F);

		/// <summary>
		/// The PidLidTaskStartDate named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidTaskStartDate</c> named property (PSETID_Task, LID 0x00008104) of type <c>PtypTime</c>,
		/// as defined in [MS-OXPROPS] section 2.333.
		/// </remarks>
		public static readonly TnefNameId TaskStartDate = new TnefNameId (TnefPropertySetGuid.Task, 0x8104);

		/// <summary>
		/// The PidLidTaskStatus named property.
		/// </summary>
		/// <remarks>
		/// The <c>PidLidTaskStatus</c> named property (PSETID_Task, LID 0x00008101) of type <c>PtypInteger32</c>,
		/// as defined in [MS-OXPROPS] section 2.335.
		/// </remarks>
		public static readonly TnefNameId TaskStatus = new TnefNameId (TnefPropertySetGuid.Task, 0x8101);

		#endregion

		/// <summary>
		/// Get the property set GUID.
		/// </summary>
		/// <remarks>
		/// Gets the property set GUID.
		/// </remarks>
		/// <value>The property set GUID.</value>
		public Guid PropertySetGuid => guid;

		/// <summary>
		/// Get the kind of TNEF name identifier.
		/// </summary>
		/// <remarks>
		/// Gets the kind of TNEF name identifier.
		/// </remarks>
		/// <value>The kind of identifier.</value>
		public TnefNameIdKind Kind => kind;

		/// <summary>
		/// Get the name, if available.
		/// </summary>
		/// <remarks>
		/// If the <see cref="Kind"/> is <see cref="TnefNameIdKind.Name"/>, then this property will be available.
		/// </remarks>
		/// <value>The name.</value>
		public string? Name => name;

		/// <summary>
		/// Get the identifier, if available.
		/// </summary>
		/// <remarks>
		/// If the <see cref="Kind"/> is <see cref="TnefNameIdKind.Id"/>, then this property will be available.
		/// </remarks>
		/// <value>The identifier.</value>
		public int Id => id;

		/// <summary>
		/// Initialize a new instance of the <see cref="TnefNameId"/> struct.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="TnefNameId"/> with the specified integer identifier.
		/// </remarks>
		/// <param name="propertySetGuid">The property set GUID.</param>
		/// <param name="id">The identifier.</param>
		public TnefNameId (Guid propertySetGuid, int id)
		{
			kind = TnefNameIdKind.Id;
			guid = propertySetGuid;
			this.id = id;
			name = null;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="TnefNameId"/> struct.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="TnefNameId"/> with the specified string identifier.
		/// </remarks>
		/// <param name="propertySetGuid">The property set GUID.</param>
		/// <param name="name">The name.</param>
		public TnefNameId (Guid propertySetGuid, string name)
		{
			kind = TnefNameIdKind.Name;
			guid = propertySetGuid;
			this.name = name;
			id = 0;
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="TnefNameId"/>.
		/// </summary>
		/// <remarks>
		/// <para>Returns a <see cref="System.String"/> that represents the current <see cref="TnefNameId"/>.</para>
		/// <para>The string consists of the property set GUID in braces, followed by a colon and either the
		/// hexadecimal property id (for example, <c>{00062004-0000-0000-c000-000000000046}:0x8083</c>) or the
		/// property name (for example, <c>{00020329-0000-0000-c000-000000000046}:Keywords</c>).</para>
		/// </remarks>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="TnefNameId"/>.</returns>
		public override string ToString ()
		{
			if (kind == TnefNameIdKind.Id)
				return guid.ToString ("B") + ":0x" + id.ToString ("X4", CultureInfo.InvariantCulture);

			return guid.ToString ("B") + ":" + name;
		}

		/// <summary>
		/// Serves as a hash function for a <see cref="TnefNameId"/> object.
		/// </summary>
		/// <remarks>
		/// Serves as a hash function for a <see cref="TnefNameId"/> object.
		/// </remarks>
		/// <returns>A hash code for this instance that is suitable for use in hashing algorithms
		/// and data structures such as a hash table.</returns>
		public override int GetHashCode ()
		{
			// Note: if kind is not Id, name is not null
			int hash = kind == TnefNameIdKind.Id ? id : name!.GetHashCode ();

			return kind.GetHashCode () ^ guid.GetHashCode () ^ hash;
		}

		/// <summary>
		/// Determine whether the specified <see cref="System.Object"/> is equal to the current <see cref="TnefNameId"/>.
		/// </summary>
		/// <remarks>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="TnefNameId"/>.
		/// </remarks>
		/// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="TnefNameId"/>.</param>
		/// <returns><see langword="true" /> if the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="TnefNameId"/>; otherwise, <see langword="false" />.</returns>
		public override bool Equals (object? obj)
		{
			return obj is TnefNameId other && Equals (other);
		}

		/// <summary>
		/// Determine whether the specified <see cref="TnefNameId"/> is equal to the current <see cref="TnefNameId"/>.
		/// </summary>
		/// <remarks>
		/// Compares two TNEF name identifiers to determine if they are identical or not.
		/// </remarks>
		/// <param name="other">The <see cref="TnefNameId"/> to compare with the current <see cref="TnefNameId"/>.</param>
		/// <returns><see langword="true" /> if the specified <see cref="TnefNameId"/> is equal to the current
		/// <see cref="TnefNameId"/>; otherwise, <see langword="false" />.</returns>
		public bool Equals (TnefNameId other)
		{
			if (kind != other.kind || guid != other.guid)
				return false;

			return kind is TnefNameIdKind.Id ? other.id == id : other.name == name;
		}

		/// <summary>
		/// Compare two <see cref="TnefNameId"/> objects for equality.
		/// </summary>
		/// <remarks>
		/// Compares two <see cref="TnefNameId"/> objects for equality.
		/// </remarks>
		/// <param name="left">The first object to compare.</param>
		/// <param name="right">The second object to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <see langword="false" />.</returns>
		public static bool operator == (TnefNameId left, TnefNameId right)
		{
			return left.Equals (right);
		}

		/// <summary>
		/// Compare two <see cref="TnefNameId"/> objects for inequality.
		/// </summary>
		/// <remarks>
		/// Compares two <see cref="TnefNameId"/> objects for inequality.
		/// </remarks>
		/// <param name="left">The first object to compare.</param>
		/// <param name="right">The second object to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left"/> and <paramref name="right"/> are unequal; otherwise, <see langword="false" />.</returns>
		public static bool operator != (TnefNameId left, TnefNameId right)
		{
			return !(left == right);
		}
	}
}

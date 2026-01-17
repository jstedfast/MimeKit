//
// TnefNamedProperty.cs
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

namespace MimeKit.Tnef {
	/// <summary>
	/// A named TNEF property.
	/// </summary>
	/// <remarks>
	/// A named TNEF property.
	/// </remarks>
	public struct TnefNamedProperty
	{
		#region <Not Used>

		/// <summary>
		/// The MAPI property PidLidAppointmentExtractTime.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentExtractTime = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x822D), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidAppointmentExtractVersion.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentExtractVersion = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x822C), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidAppointmentStickerId.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentStickerId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8219), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidDeleteAssociatedRequest.
		/// </summary>
		public static readonly TnefNamedProperty DeleteAssociatedRequest = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8225), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidFramesetBody.
		/// </summary>
		public static readonly TnefNamedProperty FramesetBody = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x858A), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidHtmlForm.
		/// </summary>
		public static readonly TnefNamedProperty HtmlForm = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x853E), TnefPropertyType.Boolean);

		#endregion <Not Used>

		#region Auto Archive

		/// <summary>
		/// The MAPI property PidLidAgingDontAgeMe.
		/// </summary>
		public static readonly TnefNamedProperty AgingDontAgeMe = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x850E), TnefPropertyType.Boolean);

		#endregion Auto Archive

		#region Calendar

		/// <summary>
		/// The MAPI property PidLidAppointmentColor.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentColor = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8214), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidAppointmentDuration.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentDuration = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8213), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidAppointmentEndDate.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentEndDate = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8211), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidAppointmentEndTime.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentEndTime = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8210), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidAppointmentEndWhole.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentEndWhole = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x820E), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidAppointmentRecur.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentRecur = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8216), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidAppointmentStartDate.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentStartDate = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8212), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidAppointmentStartTime.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentStartTime = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x820F), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidAppointmentStartWhole.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentStartWhole = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x820D), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidAppointmentSubType.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentSubType = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8215), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidAppointmentTimeZoneDefinitionEndDisplay.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentTimeZoneDefinitionEndDisplay = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x825F), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidAppointmentTimeZoneDefinitionRecur.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentTimeZoneDefinitionRecur = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8260), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidAppointmentTimeZoneDefinitionStartDisplay.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentTimeZoneDefinitionStartDisplay = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x825E), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidBusyStatus.
		/// </summary>
		public static readonly TnefNamedProperty BusyStatus = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8205), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidClipEnd.
		/// </summary>
		public static readonly TnefNamedProperty ClipEnd = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8236), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidClipStart.
		/// </summary>
		public static readonly TnefNamedProperty ClipStart = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8235), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidExceptionReplaceTime.
		/// </summary>
		public static readonly TnefNamedProperty ExceptionReplaceTime = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8228), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidLocation.
		/// </summary>
		public static readonly TnefNamedProperty Location = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8208), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidRecurrencePattern.
		/// </summary>
		public static readonly TnefNamedProperty RecurrencePattern = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8232), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidRecurrenceType.
		/// </summary>
		public static readonly TnefNamedProperty RecurrenceType = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8231), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidRecurring.
		/// </summary>
		public static readonly TnefNamedProperty Recurring = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8223), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidTimeZoneDescription.
		/// </summary>
		public static readonly TnefNamedProperty TimeZoneDescription = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8234), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidTimeZoneStruct.
		/// </summary>
		public static readonly TnefNamedProperty TimeZoneStruct = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8233), TnefPropertyType.Binary);

		#endregion Calendar

		#region Conferencing

		/// <summary>
		/// The MAPI property PidLidAllowExternalCheck.
		/// </summary>
		public static readonly TnefNamedProperty AllowExternalCheck = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8246), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidAutoStartCheck.
		/// </summary>
		public static readonly TnefNamedProperty AutoStartCheck = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8244), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidAutoStartWhen.
		/// </summary>
		public static readonly TnefNamedProperty AutoStartWhen = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8245), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidCollaborateDoc.
		/// </summary>
		public static readonly TnefNamedProperty CollaborateDoc = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8247), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidConferencingCheck.
		/// </summary>
		public static readonly TnefNamedProperty ConferencingCheck = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8240), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidConferencingCheckChanged.
		/// </summary>
		public static readonly TnefNamedProperty ConferencingCheckChanged = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x823F), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidConferencingType.
		/// </summary>
		public static readonly TnefNamedProperty ConferencingType = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8241), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidDirectory.
		/// </summary>
		public static readonly TnefNamedProperty Directory = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8242), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidNetShowUrl.
		/// </summary>
		public static readonly TnefNamedProperty NetShowUrl = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8248), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidOnlinePassword.
		/// </summary>
		public static readonly TnefNamedProperty OnlinePassword = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8249), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidOrganizationAlias.
		/// </summary>
		public static readonly TnefNamedProperty OrganizationAlias = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8243), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSendMeetingAsIcal.
		/// </summary>
		public static readonly TnefNamedProperty SendMeetingAsIcal = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8200), TnefPropertyType.Boolean);

		#endregion Conferencing

		#region Contact Properties

		/// <summary>
		/// The MAPI property PidLidAddressBookProviderArrayType.
		/// </summary>
		public static readonly TnefNamedProperty AddressBookProviderArrayType = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8029), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidAddressCountryCode.
		/// </summary>
		public static readonly TnefNamedProperty AddressCountryCode = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80DD), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidAddressSelection.
		/// </summary>
		public static readonly TnefNamedProperty AddressSelection = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8068), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidAnniversaryEventEntryId.
		/// </summary>
		public static readonly TnefNamedProperty AnniversaryEventEntryId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x804E), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidAutoLog.
		/// </summary>
		public static readonly TnefNamedProperty AutoLog = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8025), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidBirthdayEventEntryId.
		/// </summary>
		public static readonly TnefNamedProperty BirthdayEventEntryId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x804D), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidBusinessCardCardPicture.
		/// </summary>
		public static readonly TnefNamedProperty BusinessCardCardPicture = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8041), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidBusinessCardDisplayDefinition.
		/// </summary>
		public static readonly TnefNamedProperty BusinessCardDisplayDefinition = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8040), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidChildrenString.
		/// </summary>
		public static readonly TnefNamedProperty ChildrenString = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x800C), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidCompanyAndFullName.
		/// </summary>
		public static readonly TnefNamedProperty CompanyAndFullName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8018), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidCompanyLastFirstNoSpace.
		/// </summary>
		public static readonly TnefNamedProperty CompanyLastFirstNoSpace = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8032), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidCompanyLastFirstSpaceOnly.
		/// </summary>
		public static readonly TnefNamedProperty CompanyLastFirstSpaceOnly = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8033), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidConferencingAliasDisplay.
		/// </summary>
		public static readonly TnefNamedProperty ConferencingAliasDisplay = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x805F), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidConferencingBackupServerIndex.
		/// </summary>
		public static readonly TnefNamedProperty ConferencingBackupServerIndex = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8058), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidConferencingDefaultServerIndex.
		/// </summary>
		public static readonly TnefNamedProperty ConferencingDefaultServerIndex = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8057), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidConferencingEmailIndex.
		/// </summary>
		public static readonly TnefNamedProperty ConferencingEmailIndex = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8059), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidConferencingServerDisplay.
		/// </summary>
		public static readonly TnefNamedProperty ConferencingServerDisplay = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8060), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidConferencingServerNamesString.
		/// </summary>
		public static readonly TnefNamedProperty ConferencingServerNamesString = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x805E), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidContactCharacterSet.
		/// </summary>
		public static readonly TnefNamedProperty ContactCharacterSet = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8023), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidContactEmailAddressesString.
		/// </summary>
		public static readonly TnefNamedProperty ContactEmailAddressesString = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x805D), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidContactLinkEntry.
		/// </summary>
		public static readonly TnefNamedProperty ContactLinkEntry = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8585), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidContactLinkName.
		/// </summary>
		public static readonly TnefNamedProperty ContactLinkName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8586), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidContactLinkSearchKey.
		/// </summary>
		public static readonly TnefNamedProperty ContactLinkSearchKey = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8584), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidContactUserField1.
		/// </summary>
		public static readonly TnefNamedProperty ContactUserField1 = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x804F), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidContactUserField2.
		/// </summary>
		public static readonly TnefNamedProperty ContactUserField2 = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8050), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidContactUserField3.
		/// </summary>
		public static readonly TnefNamedProperty ContactUserField3 = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8051), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidContactUserField4.
		/// </summary>
		public static readonly TnefNamedProperty ContactUserField4 = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8052), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidDepartment.
		/// </summary>
		public static readonly TnefNamedProperty Department = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8010), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidDistributionListChecksum.
		/// </summary>
		public static readonly TnefNamedProperty DistributionListChecksum = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x804C), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidDistributionListCountMembers.
		/// </summary>
		public static readonly TnefNamedProperty DistributionListCountMembers = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x804B), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidDistributionListName.
		/// </summary>
		public static readonly TnefNamedProperty DistributionListName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8053), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidDontAgeLog.
		/// </summary>
		public static readonly TnefNamedProperty DontAgeLog = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x802A), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidEmail1AddressType.
		/// </summary>
		public static readonly TnefNamedProperty Email1AddressType = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8082), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidEmail1DisplayName.
		/// </summary>
		public static readonly TnefNamedProperty Email1DisplayName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8080), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidEmail1EmailAddress.
		/// </summary>
		public static readonly TnefNamedProperty Email1EmailAddress = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8083), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidEmail1EmailType.
		/// </summary>
		public static readonly TnefNamedProperty Email1EmailType = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8087), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidEmail1EntryId.
		/// </summary>
		public static readonly TnefNamedProperty Email1EntryId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8081), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidEmail1OriginalDisplayName.
		/// </summary>
		public static readonly TnefNamedProperty Email1OriginalDisplayName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8084), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidEmail1OriginalEntryId.
		/// </summary>
		public static readonly TnefNamedProperty Email1OriginalEntryId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8085), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidEmail1Rtf.
		/// </summary>
		public static readonly TnefNamedProperty Email1Rtf = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8086), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidEmail2AddressType.
		/// </summary>
		public static readonly TnefNamedProperty Email2AddressType = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8092), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidEmail2DisplayName.
		/// </summary>
		public static readonly TnefNamedProperty Email2DisplayName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8090), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidEmail2EmailAddress.
		/// </summary>
		public static readonly TnefNamedProperty Email2EmailAddress = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8093), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidEmail2EmailType.
		/// </summary>
		public static readonly TnefNamedProperty Email2EmailType = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8097), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidEmail2EntryId.
		/// </summary>
		public static readonly TnefNamedProperty Email2EntryId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8091), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidEmail2OriginalDisplayName.
		/// </summary>
		public static readonly TnefNamedProperty Email2OriginalDisplayName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8094), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidEmail2OriginalEntryId.
		/// </summary>
		public static readonly TnefNamedProperty Email2OriginalEntryId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8095), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidEmail2Rtf.
		/// </summary>
		public static readonly TnefNamedProperty Email2Rtf = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8096), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidEmail3AddressType.
		/// </summary>
		public static readonly TnefNamedProperty Email3AddressType = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80A2), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidEmail3DisplayName.
		/// </summary>
		public static readonly TnefNamedProperty Email3DisplayName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80A0), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidEmail3EmailAddress.
		/// </summary>
		public static readonly TnefNamedProperty Email3EmailAddress = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80A3), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidEmail3EmailType.
		/// </summary>
		public static readonly TnefNamedProperty Email3EmailType = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80A7), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidEmail3EntryId.
		/// </summary>
		public static readonly TnefNamedProperty Email3EntryId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80A1), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidEmail3OriginalDisplayName.
		/// </summary>
		public static readonly TnefNamedProperty Email3OriginalDisplayName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80A4), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidEmail3OriginalEntryId.
		/// </summary>
		public static readonly TnefNamedProperty Email3OriginalEntryId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80A5), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidEmail3Rtf.
		/// </summary>
		public static readonly TnefNamedProperty Email3Rtf = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80A6), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidEmailSelection.
		/// </summary>
		public static readonly TnefNamedProperty EmailSelection = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8069), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidFax1AddressType.
		/// </summary>
		public static readonly TnefNamedProperty Fax1AddressType = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80B2), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidFax1DisplayName.
		/// </summary>
		public static readonly TnefNamedProperty Fax1DisplayName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80B0), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidFax1EmailAddress.
		/// </summary>
		public static readonly TnefNamedProperty Fax1EmailAddress = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80B3), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidFax1EmailType.
		/// </summary>
		public static readonly TnefNamedProperty Fax1EmailType = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80B7), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidFax1EntryId.
		/// </summary>
		public static readonly TnefNamedProperty Fax1EntryId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80B1), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidFax1OriginalDisplayName.
		/// </summary>
		public static readonly TnefNamedProperty Fax1OriginalDisplayName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80B4), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidFax1OriginalEntryId.
		/// </summary>
		public static readonly TnefNamedProperty Fax1OriginalEntryId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80B5), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidFax1Rtf.
		/// </summary>
		public static readonly TnefNamedProperty Fax1Rtf = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80B6), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidFax2AddressType.
		/// </summary>
		public static readonly TnefNamedProperty Fax2AddressType = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80C2), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidFax2DisplayName.
		/// </summary>
		public static readonly TnefNamedProperty Fax2DisplayName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80C0), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidFax2EmailAddress.
		/// </summary>
		public static readonly TnefNamedProperty Fax2EmailAddress = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80C3), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidFax2EmailType.
		/// </summary>
		public static readonly TnefNamedProperty Fax2EmailType = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80C7), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidFax2EntryId.
		/// </summary>
		public static readonly TnefNamedProperty Fax2EntryId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80C1), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidFax2OriginalDisplayName.
		/// </summary>
		public static readonly TnefNamedProperty Fax2OriginalDisplayName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80C4), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidFax2OriginalEntryId.
		/// </summary>
		public static readonly TnefNamedProperty Fax2OriginalEntryId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80C5), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidFax2Rtf.
		/// </summary>
		public static readonly TnefNamedProperty Fax2Rtf = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80C6), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidFax3AddressType.
		/// </summary>
		public static readonly TnefNamedProperty Fax3AddressType = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80D2), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidFax3DisplayName.
		/// </summary>
		public static readonly TnefNamedProperty Fax3DisplayName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80D0), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidFax3EmailAddress.
		/// </summary>
		public static readonly TnefNamedProperty Fax3EmailAddress = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80D3), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidFax3EmailType.
		/// </summary>
		public static readonly TnefNamedProperty Fax3EmailType = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80D7), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidFax3EntryId.
		/// </summary>
		public static readonly TnefNamedProperty Fax3EntryId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80D1), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidFax3OriginalDisplayName.
		/// </summary>
		public static readonly TnefNamedProperty Fax3OriginalDisplayName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80D4), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidFax3OriginalEntryId.
		/// </summary>
		public static readonly TnefNamedProperty Fax3OriginalEntryId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80D5), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidFax3Rtf.
		/// </summary>
		public static readonly TnefNamedProperty Fax3Rtf = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80D6), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidFileUnder.
		/// </summary>
		public static readonly TnefNamedProperty FileUnder = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8005), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidFileUnderId.
		/// </summary>
		public static readonly TnefNamedProperty FileUnderId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8006), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidFirstMiddleLastSuffix.
		/// </summary>
		public static readonly TnefNamedProperty FirstMiddleLastSuffix = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8037), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidFPostalAddress.
		/// </summary>
		public static readonly TnefNamedProperty FPostalAddress = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8002), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidFreeBusyLocation.
		/// </summary>
		public static readonly TnefNamedProperty FreeBusyLocation = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80D8), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidFSendPlainText.
		/// </summary>
		public static readonly TnefNamedProperty FSendPlainText = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8001), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidFullNameAndCompany.
		/// </summary>
		public static readonly TnefNamedProperty FullNameAndCompany = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8019), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidHasPicture.
		/// </summary>
		public static readonly TnefNamedProperty HasPicture = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8015), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidHomeAddress.
		/// </summary>
		public static readonly TnefNamedProperty HomeAddress = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x801A), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidHomeAddressCountryCode.
		/// </summary>
		public static readonly TnefNamedProperty HomeAddressCountryCode = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80DA), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidHtml.
		/// </summary>
		public static readonly TnefNamedProperty Html = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x802B), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidInstantMessagingAddress.
		/// </summary>
		public static readonly TnefNamedProperty InstantMessagingAddress = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8062), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidInterConnectBusinessCard.
		/// </summary>
		public static readonly TnefNamedProperty InterConnectBusinessCard = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8042), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidInterConnectBusinessCardFlag.
		/// </summary>
		public static readonly TnefNamedProperty InterConnectBusinessCardFlag = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8043), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidInterConnectBusinessCardLastUpdate.
		/// </summary>
		public static readonly TnefNamedProperty InterConnectBusinessCardLastUpdate = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8044), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidLastFirstAndSuffix.
		/// </summary>
		public static readonly TnefNamedProperty LastFirstAndSuffix = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8036), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidLastFirstNoSpace.
		/// </summary>
		public static readonly TnefNamedProperty LastFirstNoSpace = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8030), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidLastFirstNoSpaceAndSuffix.
		/// </summary>
		public static readonly TnefNamedProperty LastFirstNoSpaceAndSuffix = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8038), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidLastFirstNoSpaceCompany.
		/// </summary>
		public static readonly TnefNamedProperty LastFirstNoSpaceCompany = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8034), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidLastFirstSpaceOnly.
		/// </summary>
		public static readonly TnefNamedProperty LastFirstSpaceOnly = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8031), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidLastFirstSpaceOnlyCompany.
		/// </summary>
		public static readonly TnefNamedProperty LastFirstSpaceOnlyCompany = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8035), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidLastNameAndFirstName.
		/// </summary>
		public static readonly TnefNamedProperty LastNameAndFirstName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8017), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidMeFlag.
		/// </summary>
		public static readonly TnefNamedProperty MeFlag = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8061), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidOtherAddress.
		/// </summary>
		public static readonly TnefNamedProperty OtherAddress = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x801C), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidOtherAddressCountryCode.
		/// </summary>
		public static readonly TnefNamedProperty OtherAddressCountryCode = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80DC), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidPhone1Selection.
		/// </summary>
		public static readonly TnefNamedProperty Phone1Selection = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x806A), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidPhone2Selection.
		/// </summary>
		public static readonly TnefNamedProperty Phone2Selection = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x806B), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidPhone3Selection.
		/// </summary>
		public static readonly TnefNamedProperty Phone3Selection = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x806C), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidPhone4Selection.
		/// </summary>
		public static readonly TnefNamedProperty Phone4Selection = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x806D), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidPhone5Selection.
		/// </summary>
		public static readonly TnefNamedProperty Phone5Selection = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x806E), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidPhone6Selection.
		/// </summary>
		public static readonly TnefNamedProperty Phone6Selection = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x806F), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidPhone7Selection.
		/// </summary>
		public static readonly TnefNamedProperty Phone7Selection = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8070), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidPhone8Selection.
		/// </summary>
		public static readonly TnefNamedProperty Phone8Selection = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8071), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidPostalAddressId.
		/// </summary>
		public static readonly TnefNamedProperty PostalAddressId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8022), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidReferenceEntryId.
		/// </summary>
		public static readonly TnefNamedProperty ReferenceEntryId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x85BD), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidReferredBy.
		/// </summary>
		public static readonly TnefNamedProperty ReferredBy = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x800E), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSelectedAddress.
		/// </summary>
		public static readonly TnefNamedProperty SelectedAddress = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8074), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSelectedEmailAddress.
		/// </summary>
		public static readonly TnefNamedProperty SelectedEmailAddress = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8008), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSelectedOriginalDisplayName.
		/// </summary>
		public static readonly TnefNamedProperty SelectedOriginalDisplayName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8009), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSelectedOriginalEntryId.
		/// </summary>
		public static readonly TnefNamedProperty SelectedOriginalEntryId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x800A), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidSelectedPhone1.
		/// </summary>
		public static readonly TnefNamedProperty SelectedPhone1 = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8076), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSelectedPhone2.
		/// </summary>
		public static readonly TnefNamedProperty SelectedPhone2 = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8077), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSelectedPhone3.
		/// </summary>
		public static readonly TnefNamedProperty SelectedPhone3 = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8078), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSelectedPhone4.
		/// </summary>
		public static readonly TnefNamedProperty SelectedPhone4 = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8079), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSelectedPhone5.
		/// </summary>
		public static readonly TnefNamedProperty SelectedPhone5 = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x807A), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSelectedPhone6.
		/// </summary>
		public static readonly TnefNamedProperty SelectedPhone6 = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x807B), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSelectedPhone7.
		/// </summary>
		public static readonly TnefNamedProperty SelectedPhone7 = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x807C), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSelectedPhone8.
		/// </summary>
		public static readonly TnefNamedProperty SelectedPhone8 = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x807D), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidUserCertificateString.
		/// </summary>
		public static readonly TnefNamedProperty UserCertificateString = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8016), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidWorkAddress.
		/// </summary>
		public static readonly TnefNamedProperty WorkAddress = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x801B), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidWorkAddressCity.
		/// </summary>
		public static readonly TnefNamedProperty WorkAddressCity = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8046), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidWorkAddressCountry.
		/// </summary>
		public static readonly TnefNamedProperty WorkAddressCountry = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8049), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidWorkAddressCountryCode.
		/// </summary>
		public static readonly TnefNamedProperty WorkAddressCountryCode = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x80DB), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidWorkAddressPostalCode.
		/// </summary>
		public static readonly TnefNamedProperty WorkAddressPostalCode = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8048), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidWorkAddressPostOfficeBox.
		/// </summary>
		public static readonly TnefNamedProperty WorkAddressPostOfficeBox = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x804A), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidWorkAddressState.
		/// </summary>
		public static readonly TnefNamedProperty WorkAddressState = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8047), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidWorkAddressStreet.
		/// </summary>
		public static readonly TnefNamedProperty WorkAddressStreet = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x8045), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidYomiCompanyName.
		/// </summary>
		public static readonly TnefNamedProperty YomiCompanyName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x802E), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidYomiFirstName.
		/// </summary>
		public static readonly TnefNamedProperty YomiFirstName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x802C), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidYomiLastName.
		/// </summary>
		public static readonly TnefNamedProperty YomiLastName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Address, 0x802D), TnefPropertyType.Unicode);

		#endregion Contact Properties

		#region Exchange

		/// <summary>
		/// The MAPI property PidLidStorageName.
		/// </summary>
		public static readonly TnefNamedProperty StorageName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Storage, 0x000A), TnefPropertyType.Long);

		#endregion Exchange

		#region Flagging

		/// <summary>
		/// The MAPI property PidLidRequest.
		/// </summary>
		public static readonly TnefNamedProperty Request = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8530), TnefPropertyType.Unicode);

		#endregion Flagging

		#region Freeze/Dry

		/// <summary>
		/// The MAPI property PidLidAutoSaveOriginalItemInfo.
		/// </summary>
		public static readonly TnefNamedProperty AutoSaveOriginalItemInfo = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x85A8), TnefPropertyType.Binary);

		#endregion Freeze/Dry

		#region General Message Properties

		/// <summary>
		/// The MAPI property PidLidBilling.
		/// </summary>
		public static readonly TnefNamedProperty Billing = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8535), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidCategoriesString.
		/// </summary>
		public static readonly TnefNamedProperty CategoriesString = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.PublicStrings, 0x2329), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidClassification.
		/// </summary>
		public static readonly TnefNamedProperty Classification = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x85B6), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidClassificationDescription.
		/// </summary>
		public static readonly TnefNamedProperty ClassificationDescription = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x85B7), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidClassificationGuid.
		/// </summary>
		public static readonly TnefNamedProperty ClassificationGuid = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x85B8), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidClassificationKeep.
		/// </summary>
		public static readonly TnefNamedProperty ClassificationKeep = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x85BA), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidClassified.
		/// </summary>
		public static readonly TnefNamedProperty Classified = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x85B5), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidCommonEnd.
		/// </summary>
		public static readonly TnefNamedProperty CommonEnd = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8517), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidCommonStart.
		/// </summary>
		public static readonly TnefNamedProperty CommonStart = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8516), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidCompaniesString.
		/// </summary>
		public static readonly TnefNamedProperty CompaniesString = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x853B), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidContactsString.
		/// </summary>
		public static readonly TnefNamedProperty ContactsString = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x853C), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidCreator.
		/// </summary>
		public static readonly TnefNamedProperty Creator = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x85BC), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidCurrentVersion.
		/// </summary>
		public static readonly TnefNamedProperty CurrentVersion = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8552), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidFHaveWrittenTracking.
		/// </summary>
		public static readonly TnefNamedProperty FHaveWrittenTracking = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Tracking, 0x8808), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidHeaderItem.
		/// </summary>
		public static readonly TnefNamedProperty HeaderItem = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8578), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidInfoPathFormType.
		/// </summary>
		public static readonly TnefNamedProperty InfoPathFormType = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x85B1), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidInternetAccountName.
		/// </summary>
		public static readonly TnefNamedProperty InternetAccountName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8580), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidInternetAccountStamp.
		/// </summary>
		public static readonly TnefNamedProperty InternetAccountStamp = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8581), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidIsInfoMailPost.
		/// </summary>
		public static readonly TnefNamedProperty IsInfoMailPost = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x859F), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidIsInterpersonalFax.
		/// </summary>
		public static readonly TnefNamedProperty IsInterpersonalFax = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x859B), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidMileage.
		/// </summary>
		public static readonly TnefNamedProperty Mileage = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8534), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidMinimumReadVersion.
		/// </summary>
		public static readonly TnefNamedProperty MinimumReadVersion = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8550), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidMinimumWriteVersion.
		/// </summary>
		public static readonly TnefNamedProperty MinimumWriteVersion = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8551), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidOfflineStatus.
		/// </summary>
		public static readonly TnefNamedProperty OfflineStatus = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x85B9), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidPrivate.
		/// </summary>
		public static readonly TnefNamedProperty Private = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8506), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidRecallTime.
		/// </summary>
		public static readonly TnefNamedProperty RecallTime = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8549), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidRightsManagementAttachmentNumber.
		/// </summary>
		public static readonly TnefNamedProperty RightsManagementAttachmentNumber = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x859D), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidSniffState.
		/// </summary>
		public static readonly TnefNamedProperty SniffState = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x851A), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidSpamOriginalFolder.
		/// </summary>
		public static readonly TnefNamedProperty SpamOriginalFolder = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x859C), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidStampedAccount.
		/// </summary>
		public static readonly TnefNamedProperty StampedAccount = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8588), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidUeberGroup.
		/// </summary>
		public static readonly TnefNamedProperty UeberGroup = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x858D), TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PidLidUnifiedTracking.
		/// </summary>
		public static readonly TnefNamedProperty UnifiedTracking = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Tracking, 0x8809), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidUseInternetZone.
		/// </summary>
		public static readonly TnefNamedProperty UseInternetZone = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8589), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidVerbResponse.
		/// </summary>
		public static readonly TnefNamedProperty VerbResponse = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8524), TnefPropertyType.Unicode);

		#endregion General Message Properties

		#region History Properties

		/// <summary>
		/// The MAPI property PidLidSyncFailures.
		/// </summary>
		public static readonly TnefNamedProperty SyncFailures = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8597), TnefPropertyType.Long);

		#endregion History Properties

		#region IMAP

		/// <summary>
		/// The MAPI property PidLidImapDeleted.
		/// </summary>
		public static readonly TnefNamedProperty ImapDeleted = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8570), TnefPropertyType.Long);

		#endregion IMAP

		#region Journal

		/// <summary>
		/// The MAPI property PidLidLogContactLog.
		/// </summary>
		public static readonly TnefNamedProperty LogContactLog = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Log, 0x870D), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidLogDocumentPosted.
		/// </summary>
		public static readonly TnefNamedProperty LogDocumentPosted = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Log, 0x8711), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidLogDocumentPrinted.
		/// </summary>
		public static readonly TnefNamedProperty LogDocumentPrinted = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Log, 0x870E), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidLogDocumentRouted.
		/// </summary>
		public static readonly TnefNamedProperty LogDocumentRouted = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Log, 0x8710), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidLogDocumentSaved.
		/// </summary>
		public static readonly TnefNamedProperty LogDocumentSaved = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Log, 0x870F), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidLogDuration.
		/// </summary>
		public static readonly TnefNamedProperty LogDuration = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Log, 0x8707), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidLogEnd.
		/// </summary>
		public static readonly TnefNamedProperty LogEnd = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Log, 0x8708), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidLogFlags.
		/// </summary>
		public static readonly TnefNamedProperty LogFlags = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Log, 0x870C), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidLogStart.
		/// </summary>
		public static readonly TnefNamedProperty LogStart = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Log, 0x8706), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidLogStartDate.
		/// </summary>
		public static readonly TnefNamedProperty LogStartDate = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Log, 0x8704), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidLogStartTime.
		/// </summary>
		public static readonly TnefNamedProperty LogStartTime = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Log, 0x8705), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidLogType.
		/// </summary>
		public static readonly TnefNamedProperty LogType = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Log, 0x8700), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidLogTypeDesc.
		/// </summary>
		public static readonly TnefNamedProperty LogTypeDesc = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Log, 0x8712), TnefPropertyType.Unicode);

		#endregion Journal

		#region Meetings

		/// <summary>
		/// The MAPI property PidLidAllAttendeesList.
		/// </summary>
		public static readonly TnefNamedProperty AllAttendeesList = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x001D), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidAllAttendeesString.
		/// </summary>
		public static readonly TnefNamedProperty AllAttendeesString = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8238), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidAppointmentAuxiliaryFlags.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentAuxiliaryFlags = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8207), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidAppointmentCounterProposal.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentCounterProposal = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8257), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidAppointmentLastSequence.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentLastSequence = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8203), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidAppointmentNotAllowPropose.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentNotAllowPropose = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x825A), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidAppointmentOpenViewProposal.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentOpenViewProposal = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x825B), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidAppointmentProposalNum.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentProposalNum = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8259), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidAppointmentProposedDuration.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentProposedDuration = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8256), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidAppointmentProposedEndDate.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentProposedEndDate = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8255), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidAppointmentProposedEndTime.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentProposedEndTime = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8253), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidAppointmentProposedStartDate.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentProposedStartDate = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8254), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidAppointmentProposedStartTime.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentProposedStartTime = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8252), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidAppointmentReplyName.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentReplyName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8230), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidAppointmentReplyTime.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentReplyTime = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8220), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidAppointmentSequence.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentSequence = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8201), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidAppointmentSequenceTime.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentSequenceTime = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8202), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidAppointmentStateFlags.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentStateFlags = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8217), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidAppointmentUnsendableRecipients.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentUnsendableRecipients = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x823D), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidAppointmentUpdateTime.
		/// </summary>
		public static readonly TnefNamedProperty AppointmentUpdateTime = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8226), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidApptProposedEndWhole.
		/// </summary>
		public static readonly TnefNamedProperty ApptProposedEndWhole = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8251), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidApptProposedStartWhole.
		/// </summary>
		public static readonly TnefNamedProperty ApptProposedStartWhole = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8250), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidAttendeeCriticalChange.
		/// </summary>
		public static readonly TnefNamedProperty AttendeeCriticalChange = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x0001), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidAutoFillLocation.
		/// </summary>
		public static readonly TnefNamedProperty AutoFillLocation = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x823A), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidCalendarType.
		/// </summary>
		public static readonly TnefNamedProperty CalendarType = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x001C), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidCCAttendeesString.
		/// </summary>
		public static readonly TnefNamedProperty CCAttendeesString = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x823C), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidChangeHighlight.
		/// </summary>
		public static readonly TnefNamedProperty ChangeHighlight = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8204), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidCleanGlobalObjectId.
		/// </summary>
		public static readonly TnefNamedProperty CleanGlobalObjectId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x0023), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidDayInterval.
		/// </summary>
		public static readonly TnefNamedProperty DayInterval = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x0011), TnefPropertyType.I2);

		/// <summary>
		/// The MAPI property PidLidDayOfMonthMask.
		/// </summary>
		public static readonly TnefNamedProperty DayOfMonthMask = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x0016), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidDayOfWeekMask.
		/// </summary>
		public static readonly TnefNamedProperty DayOfWeekMask = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x0015), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidDayOfWeekPref.
		/// </summary>
		public static readonly TnefNamedProperty DayOfWeekPref = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x0019), TnefPropertyType.I2);

		/// <summary>
		/// The MAPI property PidLidDelegateMail.
		/// </summary>
		public static readonly TnefNamedProperty DelegateMail = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x0009), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidDirtyTimesOrStatus.
		/// </summary>
		public static readonly TnefNamedProperty DirtyTimesOrStatus = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8227), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidEndRecurrenceDate.
		/// </summary>
		public static readonly TnefNamedProperty EndRecurrenceDate = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x000F), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidEndRecurrenceTime.
		/// </summary>
		public static readonly TnefNamedProperty EndRecurrenceTime = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x0010), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidFExceptionalAttendees.
		/// </summary>
		public static readonly TnefNamedProperty FExceptionalAttendees = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x822B), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidFExceptionalBody.
		/// </summary>
		public static readonly TnefNamedProperty FExceptionalBody = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8206), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidFInvited.
		/// </summary>
		public static readonly TnefNamedProperty FInvited = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8229), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidForwardInstance.
		/// </summary>
		public static readonly TnefNamedProperty ForwardInstance = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x820A), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidFOthersAppointment.
		/// </summary>
		public static readonly TnefNamedProperty FOthersAppointment = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x822F), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidGlobalObjectId.
		/// </summary>
		public static readonly TnefNamedProperty GlobalObjectId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x0003), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidIntendedBusyStatus.
		/// </summary>
		public static readonly TnefNamedProperty IntendedBusyStatus = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8224), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidIsException.
		/// </summary>
		public static readonly TnefNamedProperty IsException = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x000A), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidIsRecurring.
		/// </summary>
		public static readonly TnefNamedProperty IsRecurring = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x0005), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidIsSilent.
		/// </summary>
		public static readonly TnefNamedProperty IsSilent = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x0004), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidMeetingType.
		/// </summary>
		public static readonly TnefNamedProperty MeetingType = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x0026), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidMeetingWorkspaceUrl.
		/// </summary>
		public static readonly TnefNamedProperty MeetingWorkspaceUrl = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8209), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidMonthInterval.
		/// </summary>
		public static readonly TnefNamedProperty MonthInterval = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x0013), TnefPropertyType.I2);

		/// <summary>
		/// The MAPI property PidLidMonthOfYearMask.
		/// </summary>
		public static readonly TnefNamedProperty MonthOfYearMask = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x0017), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidNonSendableBCC.
		/// </summary>
		public static readonly TnefNamedProperty NonSendableBCC = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8538), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidNonSendableCC.
		/// </summary>
		public static readonly TnefNamedProperty NonSendableCC = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8537), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidNonSendableTo.
		/// </summary>
		public static readonly TnefNamedProperty NonSendableTo = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8536), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidOldLocation.
		/// </summary>
		public static readonly TnefNamedProperty OldLocation = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x0028), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidOldWhen.
		/// </summary>
		public static readonly TnefNamedProperty OldWhen = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x0027), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidOldWhenEndWhole.
		/// </summary>
		public static readonly TnefNamedProperty OldWhenEndWhole = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x002A), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidOldWhenStartWhole.
		/// </summary>
		public static readonly TnefNamedProperty OldWhenStartWhole = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x0029), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidOptionalAttendees.
		/// </summary>
		public static readonly TnefNamedProperty OptionalAttendees = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x0007), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidOrganizerExceptionReplaceTime.
		/// </summary>
		public static readonly TnefNamedProperty OrganizerExceptionReplaceTime = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x822A), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidOriginalStoreEntryId.
		/// </summary>
		public static readonly TnefNamedProperty OriginalStoreEntryId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8237), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidOwnerCriticalChange.
		/// </summary>
		public static readonly TnefNamedProperty OwnerCriticalChange = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x001A), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidOwnerName.
		/// </summary>
		public static readonly TnefNamedProperty OwnerName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x822E), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidProposedWhenProperty.
		/// </summary>
		public static readonly TnefNamedProperty ProposedWhenProperty = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x0025), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidRequiredAttendees.
		/// </summary>
		public static readonly TnefNamedProperty RequiredAttendees = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x0006), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidResourceAttendees.
		/// </summary>
		public static readonly TnefNamedProperty ResourceAttendees = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x0008), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidResponseState.
		/// </summary>
		public static readonly TnefNamedProperty ResponseState = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x0021), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidResponseStatus.
		/// </summary>
		public static readonly TnefNamedProperty ResponseStatus = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x8218), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidSingleInvite.
		/// </summary>
		public static readonly TnefNamedProperty SingleInvite = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x000B), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidSowFreeBusyFlags.
		/// </summary>
		public static readonly TnefNamedProperty SowFreeBusyFlags = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x823D), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidStartRecurrenceDate.
		/// </summary>
		public static readonly TnefNamedProperty StartRecurrenceDate = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x000D), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidStartRecurrenceTime.
		/// </summary>
		public static readonly TnefNamedProperty StartRecurrenceTime = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x000E), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidTimeZone.
		/// </summary>
		public static readonly TnefNamedProperty TimeZone = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x000C), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidToAttendeesString.
		/// </summary>
		public static readonly TnefNamedProperty ToAttendeesString = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x823B), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidTrustRecipientHighlights.
		/// </summary>
		public static readonly TnefNamedProperty TrustRecipientHighlights = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Appointment, 0x823E), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidWeekInterval.
		/// </summary>
		public static readonly TnefNamedProperty WeekInterval = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x0012), TnefPropertyType.I2);

		/// <summary>
		/// The MAPI property PidLidWhenProperty.
		/// </summary>
		public static readonly TnefNamedProperty WhenProperty = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x0022), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidWhere.
		/// </summary>
		public static readonly TnefNamedProperty Where = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x0002), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidYearInterval.
		/// </summary>
		public static readonly TnefNamedProperty YearInterval = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Meeting, 0x0014), TnefPropertyType.I2);

		#endregion Meetings

		#region Outlook Application

		/// <summary>
		/// The MAPI property PidLidDocumentObjectWordMail.
		/// </summary>
		public static readonly TnefNamedProperty DocumentObjectWordMail = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8587), TnefPropertyType.Boolean);

		#endregion Outlook Application

		#region Reminders

		/// <summary>
		/// The MAPI property PidLidReminderDelta.
		/// </summary>
		public static readonly TnefNamedProperty ReminderDelta = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8501), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidReminderFileParameter.
		/// </summary>
		public static readonly TnefNamedProperty ReminderFileParameter = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x851F), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidReminderNextTime.
		/// </summary>
		public static readonly TnefNamedProperty ReminderNextTime = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8560), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidReminderOverride.
		/// </summary>
		public static readonly TnefNamedProperty ReminderOverride = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x851C), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidReminderPlaySound.
		/// </summary>
		public static readonly TnefNamedProperty ReminderPlaySound = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x851E), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidReminderSet.
		/// </summary>
		public static readonly TnefNamedProperty ReminderSet = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8503), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidReminderTime.
		/// </summary>
		public static readonly TnefNamedProperty ReminderTime = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8502), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidReminderTimeDate.
		/// </summary>
		public static readonly TnefNamedProperty ReminderTimeDate = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8505), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidReminderTimeTime.
		/// </summary>
		public static readonly TnefNamedProperty ReminderTimeTime = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8504), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidReminderType.
		/// </summary>
		public static readonly TnefNamedProperty ReminderType = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x851D), TnefPropertyType.Long);

		#endregion Reminders

		#region Remote Message

		/// <summary>
		/// The MAPI property PidLidRemoteAttachment.
		/// </summary>
		public static readonly TnefNamedProperty RemoteAttachment = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Remote, 0x8F07), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidRemoteEntryId.
		/// </summary>
		public static readonly TnefNamedProperty RemoteEntryId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Remote, 0x8F01), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidRemoteSearchKey.
		/// </summary>
		public static readonly TnefNamedProperty RemoteSearchKey = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Remote, 0x8F06), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidRemoteTransferSize.
		/// </summary>
		public static readonly TnefNamedProperty RemoteTransferSize = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Remote, 0x8F05), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidRemoteTransferTime.
		/// </summary>
		public static readonly TnefNamedProperty RemoteTransferTime = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Remote, 0x8F04), TnefPropertyType.Long);

		#endregion Remote Message

		#region RSS

		/// <summary>
		/// The MAPI property PidLidPostRssChannel.
		/// </summary>
		public static readonly TnefNamedProperty PostRssChannel = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.PostRss, 0x8904), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidPostRssChannelLink.
		/// </summary>
		public static readonly TnefNamedProperty PostRssChannelLink = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.PostRss, 0x8900), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidPostRssItemGuid.
		/// </summary>
		public static readonly TnefNamedProperty PostRssItemGuid = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.PostRss, 0x8903), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidPostRssItemHash.
		/// </summary>
		public static readonly TnefNamedProperty PostRssItemHash = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.PostRss, 0x8902), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidPostRssItemLink.
		/// </summary>
		public static readonly TnefNamedProperty PostRssItemLink = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.PostRss, 0x8901), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidPostRssItemXml.
		/// </summary>
		public static readonly TnefNamedProperty PostRssItemXml = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.PostRss, 0x8905), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidPostRssSubscription.
		/// </summary>
		public static readonly TnefNamedProperty PostRssSubscription = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.PostRss, 0x8906), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingRssHash.
		/// </summary>
		public static readonly TnefNamedProperty SharingRssHash = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A20), TnefPropertyType.Binary);

		#endregion RSS

		#region Run-time configuration

		/// <summary>
		/// The MAPI property PidLidAttachmentsStripped.
		/// </summary>
		public static readonly TnefNamedProperty AttachmentsStripped = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x854A), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidColorSchemeMappingXml.
		/// </summary>
		public static readonly TnefNamedProperty ColorSchemeMappingXml = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x85C3), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidCustomFlag.
		/// </summary>
		public static readonly TnefNamedProperty CustomFlag = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8542), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidCustomPages.
		/// </summary>
		public static readonly TnefNamedProperty CustomPages = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8515), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidFormPropStream.
		/// </summary>
		public static readonly TnefNamedProperty FormPropStream = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x851B), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidFormStorage.
		/// </summary>
		public static readonly TnefNamedProperty FormStorage = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x850F), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidFShouldTnef.
		/// </summary>
		public static readonly TnefNamedProperty FShouldTnef = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x85A5), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidImageAttchmentsCompressionLevel.
		/// </summary>
		public static readonly TnefNamedProperty ImageAttchmentsCompressionLevel = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8593), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidMarkedForDownload.
		/// </summary>
		public static readonly TnefNamedProperty MarkedForDownload = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8571), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidPageDirStream.
		/// </summary>
		public static readonly TnefNamedProperty PageDirStream = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8513), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidPropertyDefinitionStream.
		/// </summary>
		public static readonly TnefNamedProperty PropertyDefinitionStream = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8540), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidRemoteStatus.
		/// </summary>
		public static readonly TnefNamedProperty RemoteStatus = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8511), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidScriptStream.
		/// </summary>
		public static readonly TnefNamedProperty ScriptStream = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8541), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidSideEffects.
		/// </summary>
		public static readonly TnefNamedProperty SideEffects = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8510), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidSmartNoAttach.
		/// </summary>
		public static readonly TnefNamedProperty SmartNoAttach = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8514), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidThemeDataXml.
		/// </summary>
		public static readonly TnefNamedProperty ThemeDataXml = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x85C2), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidUseTnef.
		/// </summary>
		public static readonly TnefNamedProperty UseTnef = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8582), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidVerbStream.
		/// </summary>
		public static readonly TnefNamedProperty VerbStream = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8520), TnefPropertyType.Binary);

		#endregion Run-time configuration

		#region Sharing

		/// <summary>
		/// The MAPI property PidLidSharingAnonymity.
		/// </summary>
		public static readonly TnefNamedProperty SharingAnonymity = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A19), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidSharingAutoPane.
		/// </summary>
		public static readonly TnefNamedProperty SharingAutoPane = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8590), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidSharingBindingEntryId.
		/// </summary>
		public static readonly TnefNamedProperty SharingBindingEntryId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A2D), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidSharingBrowseUrl.
		/// </summary>
		public static readonly TnefNamedProperty SharingBrowseUrl = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A51), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingCapabilities.
		/// </summary>
		public static readonly TnefNamedProperty SharingCapabilities = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A17), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidSharingConfigurationUrl.
		/// </summary>
		public static readonly TnefNamedProperty SharingConfigurationUrl = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A24), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingDataRangeEnd.
		/// </summary>
		public static readonly TnefNamedProperty SharingDataRangeEnd = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A45), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidSharingDataRangeStart.
		/// </summary>
		public static readonly TnefNamedProperty SharingDataRangeStart = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A44), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidSharingDetail.
		/// </summary>
		public static readonly TnefNamedProperty SharingDetail = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A2B), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidSharingEnabled.
		/// </summary>
		public static readonly TnefNamedProperty SharingEnabled = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x858C), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidSharingExternalXml.
		/// </summary>
		public static readonly TnefNamedProperty SharingExternalXml = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A21), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingFilter.
		/// </summary>
		public static readonly TnefNamedProperty SharingFilter = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A13), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidSharingFlags.
		/// </summary>
		public static readonly TnefNamedProperty SharingFlags = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A0A), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidSharingFlavor.
		/// </summary>
		public static readonly TnefNamedProperty SharingFlavor = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A18), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidSharingFolderEntryId.
		/// </summary>
		public static readonly TnefNamedProperty SharingFolderEntryId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A15), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidSharingFooterId.
		/// </summary>
		public static readonly TnefNamedProperty SharingFooterId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8591), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingIndexEntryId.
		/// </summary>
		public static readonly TnefNamedProperty SharingIndexEntryId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A2E), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidSharingInitiatorEntryId.
		/// </summary>
		public static readonly TnefNamedProperty SharingInitiatorEntryId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A09), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidSharingInitiatorName.
		/// </summary>
		public static readonly TnefNamedProperty SharingInitiatorName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A07), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingInitiatorSmtp.
		/// </summary>
		public static readonly TnefNamedProperty SharingInitiatorSmtp = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A08), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingInstanceGuid.
		/// </summary>
		public static readonly TnefNamedProperty SharingInstanceGuid = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A1C), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidSharingLastAutoSyncTime.
		/// </summary>
		public static readonly TnefNamedProperty SharingLastAutoSyncTime = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A55), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidSharingLastSync.
		/// </summary>
		public static readonly TnefNamedProperty SharingLastSync = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A1F), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidSharingLocalComment.
		/// </summary>
		public static readonly TnefNamedProperty SharingLocalComment = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A4D), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingLocalLastModificationTime.
		/// </summary>
		public static readonly TnefNamedProperty SharingLocalLastModificationTime = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A23), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidSharingLocalName.
		/// </summary>
		public static readonly TnefNamedProperty SharingLocalName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A0F), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingLocalPath.
		/// </summary>
		public static readonly TnefNamedProperty SharingLocalPath = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A0E), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingLocalStoreUid.
		/// </summary>
		public static readonly TnefNamedProperty SharingLocalStoreUid = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A49), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingLocalType.
		/// </summary>
		public static readonly TnefNamedProperty SharingLocalType = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A14), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingLocalUid.
		/// </summary>
		public static readonly TnefNamedProperty SharingLocalUid = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A10), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingOriginalMessageEntryId.
		/// </summary>
		public static readonly TnefNamedProperty SharingOriginalMessageEntryId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A29), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidSharingParentBindingEntryId.
		/// </summary>
		public static readonly TnefNamedProperty SharingParentBindingEntryId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A5C), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidSharingParticipants.
		/// </summary>
		public static readonly TnefNamedProperty SharingParticipants = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A1E), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingPermissions.
		/// </summary>
		public static readonly TnefNamedProperty SharingPermissions = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A1B), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidSharingProviderExtension.
		/// </summary>
		public static readonly TnefNamedProperty SharingProviderExtension = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A0B), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingProviderGuid.
		/// </summary>
		public static readonly TnefNamedProperty SharingProviderGuid = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A01), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidSharingProviderName.
		/// </summary>
		public static readonly TnefNamedProperty SharingProviderName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A02), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingProviderUrl.
		/// </summary>
		public static readonly TnefNamedProperty SharingProviderUrl = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A03), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingRangeEnd.
		/// </summary>
		public static readonly TnefNamedProperty SharingRangeEnd = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A47), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidSharingRangeStart.
		/// </summary>
		public static readonly TnefNamedProperty SharingRangeStart = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A46), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidSharingReciprocation.
		/// </summary>
		public static readonly TnefNamedProperty SharingReciprocation = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A1A), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidSharingRemoteByteSize.
		/// </summary>
		public static readonly TnefNamedProperty SharingRemoteByteSize = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A4B), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidSharingRemoteComment.
		/// </summary>
		public static readonly TnefNamedProperty SharingRemoteComment = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A2F), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingRemoteCrc.
		/// </summary>
		public static readonly TnefNamedProperty SharingRemoteCrc = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A4C), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidSharingRemoteLastModificationTime.
		/// </summary>
		public static readonly TnefNamedProperty SharingRemoteLastModificationTime = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A22), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidSharingRemoteMessageCount.
		/// </summary>
		public static readonly TnefNamedProperty SharingRemoteMessageCount = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A4F), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidSharingRemoteName.
		/// </summary>
		public static readonly TnefNamedProperty SharingRemoteName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A05), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingRemotePass.
		/// </summary>
		public static readonly TnefNamedProperty SharingRemotePass = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A0D), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingRemotePath.
		/// </summary>
		public static readonly TnefNamedProperty SharingRemotePath = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A04), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingRemoteStoreUid.
		/// </summary>
		public static readonly TnefNamedProperty SharingRemoteStoreUid = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A48), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingRemoteType.
		/// </summary>
		public static readonly TnefNamedProperty SharingRemoteType = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A1D), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingRemoteUid.
		/// </summary>
		public static readonly TnefNamedProperty SharingRemoteUid = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A06), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingRemoteUser.
		/// </summary>
		public static readonly TnefNamedProperty SharingRemoteUser = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A0C), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingRemoteVersion.
		/// </summary>
		public static readonly TnefNamedProperty SharingRemoteVersion = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A5B), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingResponseTime.
		/// </summary>
		public static readonly TnefNamedProperty SharingResponseTime = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A28), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidSharingResponseType.
		/// </summary>
		public static readonly TnefNamedProperty SharingResponseType = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A27), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidSharingRoamLog.
		/// </summary>
		public static readonly TnefNamedProperty SharingRoamLog = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A4E), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidSharingServerStatus.
		/// </summary>
		public static readonly TnefNamedProperty SharingServerStatus = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x859A), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidSharingServerUrl.
		/// </summary>
		public static readonly TnefNamedProperty SharingServerUrl = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x858E), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingStart.
		/// </summary>
		public static readonly TnefNamedProperty SharingStart = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A25), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidSharingStatus.
		/// </summary>
		public static readonly TnefNamedProperty SharingStatus = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A00), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidSharingStop.
		/// </summary>
		public static readonly TnefNamedProperty SharingStop = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A26), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidSharingSyncInterval.
		/// </summary>
		public static readonly TnefNamedProperty SharingSyncInterval = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A2A), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidSharingTimeToLive.
		/// </summary>
		public static readonly TnefNamedProperty SharingTimeToLive = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A2C), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidSharingTimeToLiveAuto.
		/// </summary>
		public static readonly TnefNamedProperty SharingTimeToLiveAuto = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A56), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidSharingTitle.
		/// </summary>
		public static readonly TnefNamedProperty SharingTitle = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x858F), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingWebUrl.
		/// </summary>
		public static readonly TnefNamedProperty SharingWebUrl = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8596), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingWorkingHoursDays.
		/// </summary>
		public static readonly TnefNamedProperty SharingWorkingHoursDays = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A42), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidSharingWorkingHoursEnd.
		/// </summary>
		public static readonly TnefNamedProperty SharingWorkingHoursEnd = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A41), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidSharingWorkingHoursStart.
		/// </summary>
		public static readonly TnefNamedProperty SharingWorkingHoursStart = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A40), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidSharingWorkingHoursTimeZone.
		/// </summary>
		public static readonly TnefNamedProperty SharingWorkingHoursTimeZone = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A43), TnefPropertyType.Binary);

		#endregion Sharing

		#region Sticky Notes

		/// <summary>
		/// The MAPI property PidLidNoteColor.
		/// </summary>
		public static readonly TnefNamedProperty NoteColor = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Note, 0x8B00), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidNoteHeight.
		/// </summary>
		public static readonly TnefNamedProperty NoteHeight = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Note, 0x8B03), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidNoteOnTop.
		/// </summary>
		public static readonly TnefNamedProperty NoteOnTop = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Note, 0x8B01), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidNoteWidth.
		/// </summary>
		public static readonly TnefNamedProperty NoteWidth = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Note, 0x8B02), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidNoteX.
		/// </summary>
		public static readonly TnefNamedProperty NoteX = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Note, 0x8B04), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidNoteY.
		/// </summary>
		public static readonly TnefNamedProperty NoteY = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Note, 0x8B05), TnefPropertyType.Long);

		#endregion Sticky Notes

		#region Tasks

		/// <summary>
		/// The MAPI property PidLidFlagString.
		/// </summary>
		public static readonly TnefNamedProperty FlagString = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x85C0), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidTaskAccepted.
		/// </summary>
		public static readonly TnefNamedProperty TaskAccepted = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8108), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidTaskActualEffort.
		/// </summary>
		public static readonly TnefNamedProperty TaskActualEffort = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8110), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidTaskCardData.
		/// </summary>
		public static readonly TnefNamedProperty TaskCardData = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x812B), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidTaskComplete.
		/// </summary>
		public static readonly TnefNamedProperty TaskComplete = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x811C), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidTaskCustomFlags.
		/// </summary>
		public static readonly TnefNamedProperty TaskCustomFlags = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8139), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidTaskCustomPriority.
		/// </summary>
		public static readonly TnefNamedProperty TaskCustomPriority = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8138), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidTaskCustomStatus.
		/// </summary>
		public static readonly TnefNamedProperty TaskCustomStatus = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8137), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidTaskDateCompleted.
		/// </summary>
		public static readonly TnefNamedProperty TaskDateCompleted = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x810F), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidTaskDeadOccurrence.
		/// </summary>
		public static readonly TnefNamedProperty TaskDeadOccurrence = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8109), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidTaskDelegateValue.
		/// </summary>
		public static readonly TnefNamedProperty TaskDelegateValue = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x812A), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidTaskDelegator.
		/// </summary>
		public static readonly TnefNamedProperty TaskDelegator = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8121), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidTaskDueDate.
		/// </summary>
		public static readonly TnefNamedProperty TaskDueDate = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8105), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidTaskEstimatedEffort.
		/// </summary>
		public static readonly TnefNamedProperty TaskEstimatedEffort = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8111), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidTaskFCreator.
		/// </summary>
		public static readonly TnefNamedProperty TaskFCreator = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x811E), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidTaskFFixOffline.
		/// </summary>
		public static readonly TnefNamedProperty TaskFFixOffline = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x812C), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidTaskFormUrn.
		/// </summary>
		public static readonly TnefNamedProperty TaskFormUrn = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8132), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidTaskFRecurring.
		/// </summary>
		public static readonly TnefNamedProperty TaskFRecurring = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8126), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidTaskGlobalObjectId.
		/// </summary>
		public static readonly TnefNamedProperty TaskGlobalObjectId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8519), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidTaskHistory.
		/// </summary>
		public static readonly TnefNamedProperty TaskHistory = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x811A), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidTaskLastDelegate.
		/// </summary>
		public static readonly TnefNamedProperty TaskLastDelegate = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8125), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidTaskLastUpdate.
		/// </summary>
		public static readonly TnefNamedProperty TaskLastUpdate = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8115), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidTaskLastUser.
		/// </summary>
		public static readonly TnefNamedProperty TaskLastUser = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8122), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidTaskMode.
		/// </summary>
		public static readonly TnefNamedProperty TaskMode = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x8518), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidTaskMultipleRecipients.
		/// </summary>
		public static readonly TnefNamedProperty TaskMultipleRecipients = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8120), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidTaskMyDelegators.
		/// </summary>
		public static readonly TnefNamedProperty TaskMyDelegators = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8117), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidTaskNoCompute.
		/// </summary>
		public static readonly TnefNamedProperty TaskNoCompute = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8124), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidTaskOrdinal.
		/// </summary>
		public static readonly TnefNamedProperty TaskOrdinal = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8123), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidTaskOriginalRecurring.
		/// </summary>
		public static readonly TnefNamedProperty TaskOriginalRecurring = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x811D), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidTaskOwner.
		/// </summary>
		public static readonly TnefNamedProperty TaskOwner = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x811F), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidTaskOwnership.
		/// </summary>
		public static readonly TnefNamedProperty TaskOwnership = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8129), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidTaskRecurrence.
		/// </summary>
		public static readonly TnefNamedProperty TaskRecurrence = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8116), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidTaskResetReminder.
		/// </summary>
		public static readonly TnefNamedProperty TaskResetReminder = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8107), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidTaskRole.
		/// </summary>
		public static readonly TnefNamedProperty TaskRole = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8127), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidTaskSchedulePriority.
		/// </summary>
		public static readonly TnefNamedProperty TaskSchedulePriority = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x812F), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidTaskStartDate.
		/// </summary>
		public static readonly TnefNamedProperty TaskStartDate = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8104), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidTaskState.
		/// </summary>
		public static readonly TnefNamedProperty TaskState = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8113), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidTaskStatus.
		/// </summary>
		public static readonly TnefNamedProperty TaskStatus = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8101), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidTaskStatusOnComplete.
		/// </summary>
		public static readonly TnefNamedProperty TaskStatusOnComplete = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8119), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidTaskUpdates.
		/// </summary>
		public static readonly TnefNamedProperty TaskUpdates = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x811B), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidTaskVersion.
		/// </summary>
		public static readonly TnefNamedProperty TaskVersion = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8112), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidTaskWebUrl.
		/// </summary>
		public static readonly TnefNamedProperty TaskWebUrl = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8134), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidTeamTask.
		/// </summary>
		public static readonly TnefNamedProperty TeamTask = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Task, 0x8103), TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PidLidToDoOrdinalDate.
		/// </summary>
		public static readonly TnefNamedProperty ToDoOrdinalDate = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x85A0), TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PidLidToDoSubOrdinal.
		/// </summary>
		public static readonly TnefNamedProperty ToDoSubOrdinal = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x85A1), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidToDoTitle.
		/// </summary>
		public static readonly TnefNamedProperty ToDoTitle = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x85A4), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidToDoTitleObjectModel.
		/// </summary>
		public static readonly TnefNamedProperty ToDoTitleObjectModel = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0xFC1F), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidValidFlagStringProof.
		/// </summary>
		public static readonly TnefNamedProperty ValidFlagStringProof = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x85BF), TnefPropertyType.SysTime);

		#endregion Tasks

		#region WSS

		/// <summary>
		/// The MAPI property PidLidSharedItemOwner.
		/// </summary>
		public static readonly TnefNamedProperty SharedItemOwner = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x85C1), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingSyncFlags.
		/// </summary>
		public static readonly TnefNamedProperty SharingSyncFlags = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A60), TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PidLidSharingWssAllFolderIDs.
		/// </summary>
		public static readonly TnefNamedProperty SharingWssAllFolderIDs = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A62), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingWssAlternateUrls.
		/// </summary>
		public static readonly TnefNamedProperty SharingWssAlternateUrls = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A5A), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingWssCachedSchema.
		/// </summary>
		public static readonly TnefNamedProperty SharingWssCachedSchema = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A5D), TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PidLidSharingWssCmd.
		/// </summary>
		public static readonly TnefNamedProperty SharingWssCmd = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A31), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingWssFileRelUrl.
		/// </summary>
		public static readonly TnefNamedProperty SharingWssFileRelUrl = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A58), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingWssFolderID.
		/// </summary>
		public static readonly TnefNamedProperty SharingWssFolderID = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A61), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingWssFolderRelUrl.
		/// </summary>
		public static readonly TnefNamedProperty SharingWssFolderRelUrl = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A57), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingWssListRelUrl.
		/// </summary>
		public static readonly TnefNamedProperty SharingWssListRelUrl = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A32), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingWssServerRelUrl.
		/// </summary>
		public static readonly TnefNamedProperty SharingWssServerRelUrl = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A5F), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingWssSiteName.
		/// </summary>
		public static readonly TnefNamedProperty SharingWssSiteName = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A33), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidSharingWssVer.
		/// </summary>
		public static readonly TnefNamedProperty SharingWssVer = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Sharing, 0x8A30), TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PidLidStsContentTypeId.
		/// </summary>
		public static readonly TnefNamedProperty StsContentTypeId = new TnefNamedProperty (new TnefNameId (TnefPropertySetGuids.Common, 0x85BE), TnefPropertyType.Unicode);

		#endregion WSS

		const short MultiValuedFlag = (short) TnefPropertyType.MultiValued;
		readonly TnefPropertyType type;
		readonly TnefNameId id;

		/// <summary>
		/// Get the property name identifier.
		/// </summary>
		/// <remarks>
		/// Gets the property name identifier.
		/// </remarks>
		/// <value>The identifier.</value>
		public TnefNameId Id {
			get { return id; }
		}

		/// <summary>
		/// Get a value indicating whether the property contains multiple values.
		/// </summary>
		/// <remarks>
		/// Gets a value indicating whether the property contains multiple values.
		/// </remarks>
		/// <value><see langword="true" /> if the property contains multiple values; otherwise, <see langword="false" />.</value>
		public bool IsMultiValued {
			get { return (((short) type) & MultiValuedFlag) != 0; }
		}

		/// <summary>
		/// Get the property's value type (including the multi-valued bit).
		/// </summary>
		/// <remarks>
		/// Gets the property's value type (including the multi-valued bit).
		/// </remarks>
		/// <value>The property's value type.</value>
		public TnefPropertyType TnefType {
			get { return type; }
		}

		/// <summary>
		/// Get the type of the value that the property contains.
		/// </summary>
		/// <remarks>
		/// Gets the type of the value that the property contains.
		/// </remarks>
		/// <value>The type of the value.</value>
		public TnefPropertyType ValueTnefType {
			get { return (TnefPropertyType) (((short) type) & ~MultiValuedFlag); }
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="TnefPropertyTag"/> struct.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="TnefNamedProperty"/> based on a <see cref="TnefNameId"/>
		/// and <see cref="TnefPropertyType"/>.
		/// </remarks>
		/// <param name="id">The property name identifier.</param>
		/// <param name="type">The property type.</param>
		public TnefNamedProperty (TnefNameId id, TnefPropertyType type)
		{
			this.type = type;
			this.id = id;
		}

		/// <summary>
		/// Serves as a hash function for a <see cref="TnefNamedProperty"/> object.
		/// </summary>
		/// <remarks>
		/// Serves as a hash function for a <see cref="TnefNamedProperty"/> object.
		/// </remarks>
		/// <returns>A hash code for this instance that is suitable for use in hashing algorithms
		/// and data structures such as a hash table.</returns>
		public override int GetHashCode ()
		{
			return this.id.GetHashCode () ^ this.type.GetHashCode ();
		}

		/// <summary>
		/// Determine whether the specified <see cref="System.Object"/> is equal to the current <see cref="TnefNamedProperty"/>.
		/// </summary>
		/// <remarks>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="TnefNamedProperty"/>.
		/// </remarks>
		/// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="TnefNamedProperty"/>.</param>
		/// <returns><see langword="true" /> if the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="TnefNamedProperty"/>; otherwise, <see langword="false" />.</returns>
		public override bool Equals (object? obj)
		{
			return obj is TnefNamedProperty prop
				&& prop.Id == Id
				&& prop.TnefType == TnefType;
		}

		/// <summary>
		/// Return a <see cref="System.String"/> that represents the current <see cref="TnefNamedProperty"/>.
		/// </summary>
		/// <remarks>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="TnefNamedProperty"/>.
		/// </remarks>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="TnefNamedProperty"/>.</returns>
		public override string ToString ()
		{
			return $"{Id} ({ValueTnefType})";
		}

		/// <summary>
		/// Return a new <see cref="TnefNamedProperty"/> where <see cref="TnefPropertyType.String8"/> has been changed to <see cref="TnefPropertyType.Unicode"/>.
		/// </summary>
		/// <remarks>
		/// Returns a new <see cref="TnefNamedProperty"/> where <see cref="TnefPropertyType.String8"/> has been changed to <see cref="TnefPropertyType.Unicode"/>.
		/// </remarks>
		/// <returns>The unicode equivalent of the named property.</returns>
		public TnefNamedProperty ToUnicode ()
		{
			if (ValueTnefType == TnefPropertyType.String8)
				return new TnefNamedProperty (id, IsMultiValued ? TnefPropertyType.Unicode | TnefPropertyType.MultiValued : TnefPropertyType.Unicode);

			return this;
		}

		/// <summary>
		/// Compare two <see cref="TnefNamedProperty"/> objects for equality.
		/// </summary>
		/// <remarks>
		/// Compares two <see cref="TnefNamedProperty"/> objects for equality.
		/// </remarks>
		/// <param name="left">The first object to compare.</param>
		/// <param name="right">The second object to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <see langword="false" />.</returns>
		public static bool operator == (TnefNamedProperty left, TnefNamedProperty right)
		{
			return left.Equals (right);
		}

		/// <summary>
		/// Compare two <see cref="TnefNamedProperty"/> objects for inequality.
		/// </summary>
		/// <remarks>
		/// Compares two <see cref="TnefNamedProperty"/> objects for inequality.
		/// </remarks>
		/// <param name="left">The first object to compare.</param>
		/// <param name="right">The second object to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left"/> and <paramref name="right"/> are unequal; otherwise, <see langword="false" />.</returns>
		public static bool operator != (TnefNamedProperty left, TnefNamedProperty right)
		{
			return !(left == right);
		}
	}
}

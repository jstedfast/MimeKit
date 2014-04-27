//
// TnefPropertyTag.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (www.xamarin.com)
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

namespace MimeKit.Tnef {
	public struct TnefPropertyTag
	{
		public static readonly TnefPropertyTag AbDefaultDir = new TnefPropertyTag (TnefPropertyId.AbDefaultDir, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag AbDefaultPab = new TnefPropertyTag (TnefPropertyId.AbDefaultPab, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag AbProviderId = new TnefPropertyTag (TnefPropertyId.AbProviderId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag AbProviders = new TnefPropertyTag (TnefPropertyId.AbProviders, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag AbSearchPath = new TnefPropertyTag (TnefPropertyId.AbSearchPath, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag AbSearchPathUpdate = new TnefPropertyTag (TnefPropertyId.AbSearchPathUpdate, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag Access = new TnefPropertyTag (TnefPropertyId.Access, TnefPropertyType.Long);
		public static readonly TnefPropertyTag AccessLevel = new TnefPropertyTag (TnefPropertyId.AccessLevel, TnefPropertyType.Long);
		public static readonly TnefPropertyTag AccountA = new TnefPropertyTag (TnefPropertyId.Account, TnefPropertyType.String8);
		public static readonly TnefPropertyTag AccountW = new TnefPropertyTag (TnefPropertyId.Account, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag AcknowledgementMode = new TnefPropertyTag (TnefPropertyId.AcknowledgementMode, TnefPropertyType.Long);
		public static readonly TnefPropertyTag AddrtypeA = new TnefPropertyTag (TnefPropertyId.Addrtype, TnefPropertyType.String8);
		public static readonly TnefPropertyTag AddrtypeW = new TnefPropertyTag (TnefPropertyId.Addrtype, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag AlternateRecipient = new TnefPropertyTag (TnefPropertyId.AlternateRecipient, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag AlternateRecipientAllowed = new TnefPropertyTag (TnefPropertyId.AlternateRecipientAllowed, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag AnrA = new TnefPropertyTag (TnefPropertyId.Anr, TnefPropertyType.String8);
		public static readonly TnefPropertyTag AnrW = new TnefPropertyTag (TnefPropertyId.Anr, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag AssistantA = new TnefPropertyTag (TnefPropertyId.Assistant, TnefPropertyType.String8);
		public static readonly TnefPropertyTag AssistantW = new TnefPropertyTag (TnefPropertyId.Assistant, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag AssistantTelephoneNumberA = new TnefPropertyTag (TnefPropertyId.AssistantTelephoneNumber, TnefPropertyType.String8);
		public static readonly TnefPropertyTag AssistantTelephoneNumberW = new TnefPropertyTag (TnefPropertyId.AssistantTelephoneNumber, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag AssocContentCount = new TnefPropertyTag (TnefPropertyId.AssocContentCount, TnefPropertyType.Long);
		public static readonly TnefPropertyTag AttachAdditionalInfo = new TnefPropertyTag (TnefPropertyId.AttachAdditionalInfo, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag AttachContentBaseA = new TnefPropertyTag (TnefPropertyId.AttachContentBase, TnefPropertyType.String8);
		public static readonly TnefPropertyTag AttachContentBaseW = new TnefPropertyTag (TnefPropertyId.AttachContentBase, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag AttachContentIdA = new TnefPropertyTag (TnefPropertyId.AttachContentId, TnefPropertyType.String8);
		public static readonly TnefPropertyTag AttachContentIdW = new TnefPropertyTag (TnefPropertyId.AttachContentId, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag AttachContentLocationA = new TnefPropertyTag (TnefPropertyId.AttachContentLocation, TnefPropertyType.String8);
		public static readonly TnefPropertyTag AttachContentLocationW = new TnefPropertyTag (TnefPropertyId.AttachContentLocation, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag AttachDataBin = new TnefPropertyTag (TnefPropertyId.AttachData, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag AttachDataObj = new TnefPropertyTag (TnefPropertyId.AttachData, TnefPropertyType.Object);
		public static readonly TnefPropertyTag AttachDispositionA = new TnefPropertyTag (TnefPropertyId.AttachDisposition, TnefPropertyType.String8);
		public static readonly TnefPropertyTag AttachDispositionW = new TnefPropertyTag (TnefPropertyId.AttachDisposition, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag AttachEncoding = new TnefPropertyTag (TnefPropertyId.AttachEncoding, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag AttachExtensionA = new TnefPropertyTag (TnefPropertyId.AttachExtension, TnefPropertyType.String8);
		public static readonly TnefPropertyTag AttachExtensionW = new TnefPropertyTag (TnefPropertyId.AttachExtension, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag AttachFilenameA = new TnefPropertyTag (TnefPropertyId.AttachFilename, TnefPropertyType.String8);
		public static readonly TnefPropertyTag AttachFilenameW = new TnefPropertyTag (TnefPropertyId.AttachFilename, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag AttachFlags = new TnefPropertyTag (TnefPropertyId.AttachFlags, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag AttachLongFilenameA = new TnefPropertyTag (TnefPropertyId.AttachLongFilename, TnefPropertyType.String8);
		public static readonly TnefPropertyTag AttachLongFilenameW = new TnefPropertyTag (TnefPropertyId.AttachLongFilename, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag AttachLongPathnameA = new TnefPropertyTag (TnefPropertyId.AttachLongPathname, TnefPropertyType.String8);
		public static readonly TnefPropertyTag AttachLongPathnameW = new TnefPropertyTag (TnefPropertyId.AttachLongPathname, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag AttachmentX400Parameters = new TnefPropertyTag (TnefPropertyId.AttachmentX400Parameters, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag AttachMethod = new TnefPropertyTag (TnefPropertyId.AttachMethod, TnefPropertyType.Long);
		public static readonly TnefPropertyTag AttachMimeSequence = new TnefPropertyTag (TnefPropertyId.AttachMimeSequence, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag AttachMimeTagA = new TnefPropertyTag (TnefPropertyId.AttachMimeTag, TnefPropertyType.String8);
		public static readonly TnefPropertyTag AttachMimeTagW = new TnefPropertyTag (TnefPropertyId.AttachMimeTag, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag AttachNetscapeMacInfo = new TnefPropertyTag (TnefPropertyId.AttachNetscapeMacInfo, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag AttachNum = new TnefPropertyTag (TnefPropertyId.AttachNum, TnefPropertyType.Long);
		public static readonly TnefPropertyTag AttachPathnameA = new TnefPropertyTag (TnefPropertyId.AttachPathname, TnefPropertyType.String8);
		public static readonly TnefPropertyTag AttachPathnameW = new TnefPropertyTag (TnefPropertyId.AttachPathname, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag AttachRendering = new TnefPropertyTag (TnefPropertyId.AttachRendering, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag AttachSize = new TnefPropertyTag (TnefPropertyId.AttachSize, TnefPropertyType.Long);
		public static readonly TnefPropertyTag AttachTag = new TnefPropertyTag (TnefPropertyId.AttachTag, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag AttachTransportNameA = new TnefPropertyTag (TnefPropertyId.AttachTransportName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag AttachTransportNameW = new TnefPropertyTag (TnefPropertyId.AttachTransportName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag AuthorizingUsers = new TnefPropertyTag (TnefPropertyId.AuthorizingUsers, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag AutoForwarded = new TnefPropertyTag (TnefPropertyId.AutoForwarded, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag AutoForwardingCommentA = new TnefPropertyTag (TnefPropertyId.AutoForwardingComment, TnefPropertyType.String8);
		public static readonly TnefPropertyTag AutoForwardingCommentW = new TnefPropertyTag (TnefPropertyId.AutoForwardingComment, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag AutoResponseSuppress = new TnefPropertyTag (TnefPropertyId.AutoResponseSuppress, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag BeeperTelephoneNumberA = new TnefPropertyTag (TnefPropertyId.BeeperTelephoneNumber, TnefPropertyType.String8);
		public static readonly TnefPropertyTag BeeperTelephoneNumberW = new TnefPropertyTag (TnefPropertyId.BeeperTelephoneNumber, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag Birthday = new TnefPropertyTag (TnefPropertyId.Birthday, TnefPropertyType.SysTime);
		public static readonly TnefPropertyTag BodyA = new TnefPropertyTag (TnefPropertyId.Body, TnefPropertyType.String8);
		public static readonly TnefPropertyTag BodyW = new TnefPropertyTag (TnefPropertyId.Body, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag BodyContentIdA = new TnefPropertyTag (TnefPropertyId.BodyContentId, TnefPropertyType.String8);
		public static readonly TnefPropertyTag BodyContentIdW = new TnefPropertyTag (TnefPropertyId.BodyContentId, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag BodyContentLocationA = new TnefPropertyTag (TnefPropertyId.BodyContentLocation, TnefPropertyType.String8);
		public static readonly TnefPropertyTag BodyContentLocationW = new TnefPropertyTag (TnefPropertyId.BodyContentLocation, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag BodyCrc = new TnefPropertyTag (TnefPropertyId.BodyCrc, TnefPropertyType.Long);
		public static readonly TnefPropertyTag BodyHtmlA = new TnefPropertyTag (TnefPropertyId.BodyHtml, TnefPropertyType.String8);
		public static readonly TnefPropertyTag BodyHtmlB = new TnefPropertyTag (TnefPropertyId.BodyHtml, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag BodyHtmlW = new TnefPropertyTag (TnefPropertyId.BodyHtml, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag Business2TelephoneNumberA = new TnefPropertyTag (TnefPropertyId.Business2TelephoneNumber, TnefPropertyType.String8);
		public static readonly TnefPropertyTag Business2TelephoneNumberAMv = new TnefPropertyTag (TnefPropertyId.Business2TelephoneNumber, TnefPropertyType.String8, true);
		public static readonly TnefPropertyTag Business2TelephoneNumberW = new TnefPropertyTag (TnefPropertyId.Business2TelephoneNumber, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag Business2TelephoneNumberWMv = new TnefPropertyTag (TnefPropertyId.Business2TelephoneNumber, TnefPropertyType.Unicode, true);
		public static readonly TnefPropertyTag BusinessAddressCityA = new TnefPropertyTag (TnefPropertyId.BusinessAddressCity, TnefPropertyType.String8);
		public static readonly TnefPropertyTag BusinessAddressCityW = new TnefPropertyTag (TnefPropertyId.BusinessAddressCity, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag BusinessAddressCountryA = new TnefPropertyTag (TnefPropertyId.BusinessAddressCountry, TnefPropertyType.String8);
		public static readonly TnefPropertyTag BusinessAddressCountryW = new TnefPropertyTag (TnefPropertyId.BusinessAddressCountry, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag BusinessAddressPostalCodeA = new TnefPropertyTag (TnefPropertyId.BusinessAddressPostalCode, TnefPropertyType.String8);
		public static readonly TnefPropertyTag BusinessAddressPostalCodeW = new TnefPropertyTag (TnefPropertyId.BusinessAddressPostalCode, TnefPropertyType.Unicode);
		//public static readonly TnefPropertyTag BusinessAddressPostOfficeBox = new TnefPropertyTag (TnefPropertyId.BusinessAddressPostOfficeBox, TnefPropertyType.Unspecified);
		//public static readonly TnefPropertyTag BusinessAddressStateOrProvince = new TnefPropertyTag (TnefPropertyId.BusinessAddressStateOrProvince, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag BusinessAddressStreetA = new TnefPropertyTag (TnefPropertyId.BusinessAddressStreet, TnefPropertyType.String8);
		public static readonly TnefPropertyTag BusinessAddressStreetW = new TnefPropertyTag (TnefPropertyId.BusinessAddressStreet, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag BusinessFaxNumberA = new TnefPropertyTag (TnefPropertyId.BusinessFaxNumber, TnefPropertyType.String8);
		public static readonly TnefPropertyTag BusinessFaxNumberW = new TnefPropertyTag (TnefPropertyId.BusinessFaxNumber, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag BusinessHomePageA = new TnefPropertyTag (TnefPropertyId.BusinessHomePage, TnefPropertyType.String8);
		public static readonly TnefPropertyTag BusinessHomePageW = new TnefPropertyTag (TnefPropertyId.BusinessHomePage, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag CallbackTelephoneNumberA = new TnefPropertyTag (TnefPropertyId.CallbackTelephoneNumber, TnefPropertyType.String8);
		public static readonly TnefPropertyTag CallbackTelephoneNumberW = new TnefPropertyTag (TnefPropertyId.CallbackTelephoneNumber, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag CarTelephoneNumberA = new TnefPropertyTag (TnefPropertyId.CarTelephoneNumber, TnefPropertyType.String8);
		public static readonly TnefPropertyTag CarTelephoneNumberW = new TnefPropertyTag (TnefPropertyId.CarTelephoneNumber, TnefPropertyType.Unicode);
		//public static readonly TnefPropertyTag CellularTelephoneNumber = new TnefPropertyTag (TnefPropertyId.CellularTelephoneNumber, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ChildrensNamesA = new TnefPropertyTag (TnefPropertyId.ChildrensNames, TnefPropertyType.String8, true);
		public static readonly TnefPropertyTag ChildrensNamesW = new TnefPropertyTag (TnefPropertyId.ChildrensNames, TnefPropertyType.Unicode, true);
		public static readonly TnefPropertyTag ClientSubmitTime = new TnefPropertyTag (TnefPropertyId.ClientSubmitTime, TnefPropertyType.SysTime);
		public static readonly TnefPropertyTag CommentA = new TnefPropertyTag (TnefPropertyId.Comment, TnefPropertyType.String8);
		public static readonly TnefPropertyTag CommentW = new TnefPropertyTag (TnefPropertyId.Comment, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag CommonViewsEntryId = new TnefPropertyTag (TnefPropertyId.CommonViewsEntryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag CompanyMainPhoneNumberA = new TnefPropertyTag (TnefPropertyId.CompanyMainPhoneNumber, TnefPropertyType.String8);
		public static readonly TnefPropertyTag CompanyMainPhoneNumberW = new TnefPropertyTag (TnefPropertyId.CompanyMainPhoneNumber, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag CompanyNameA = new TnefPropertyTag (TnefPropertyId.CompanyName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag CompanyNameW = new TnefPropertyTag (TnefPropertyId.CompanyName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag ComputerNetworkNameA = new TnefPropertyTag (TnefPropertyId.ComputerNetworkName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ComputerNetworkNameW = new TnefPropertyTag (TnefPropertyId.ComputerNetworkName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag ContactAddrtypesA = new TnefPropertyTag (TnefPropertyId.ContactAddrtypes, TnefPropertyType.String8, true);
		public static readonly TnefPropertyTag ContactAddrtypesW = new TnefPropertyTag (TnefPropertyId.ContactAddrtypes, TnefPropertyType.Unicode, true);
		public static readonly TnefPropertyTag ContactDefaultAddressIndex = new TnefPropertyTag (TnefPropertyId.ContactDefaultAddressIndex, TnefPropertyType.Long);
		public static readonly TnefPropertyTag ContactEmailAddressesA = new TnefPropertyTag (TnefPropertyId.ContactEmailAddresses, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ContactEmailAddressesW = new TnefPropertyTag (TnefPropertyId.ContactEmailAddresses, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag ContactEntryIds = new TnefPropertyTag (TnefPropertyId.ContactEntryIds, TnefPropertyType.Binary, true);
		public static readonly TnefPropertyTag ContactVersion = new TnefPropertyTag (TnefPropertyId.ContactVersion, TnefPropertyType.ClassId);
		public static readonly TnefPropertyTag ContainerClassA = new TnefPropertyTag (TnefPropertyId.ContainerClass, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ContainerClassW = new TnefPropertyTag (TnefPropertyId.ContainerClass, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag ContainerContents = new TnefPropertyTag (TnefPropertyId.ContainerContents, TnefPropertyType.Object);
		public static readonly TnefPropertyTag ContainerFlags = new TnefPropertyTag (TnefPropertyId.ContainerFlags, TnefPropertyType.Long);
		public static readonly TnefPropertyTag ContainerHierarchy = new TnefPropertyTag (TnefPropertyId.ContainerHierarchy, TnefPropertyType.Object);
		public static readonly TnefPropertyTag ContainerModifyVersion = new TnefPropertyTag (TnefPropertyId.ContainerModifyVersion, TnefPropertyType.I8);
		public static readonly TnefPropertyTag ContentConfidentialityAlgorithmId = new TnefPropertyTag (TnefPropertyId.ContentConfidentialityAlgorithmId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ContentCorrelator = new TnefPropertyTag (TnefPropertyId.ContentCorrelator, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ContentCount = new TnefPropertyTag (TnefPropertyId.ContentCount, TnefPropertyType.Long);
		public static readonly TnefPropertyTag ContentIdentifierA = new TnefPropertyTag (TnefPropertyId.ContentIdentifier, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ContentIdentifierW = new TnefPropertyTag (TnefPropertyId.ContentIdentifier, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag ContentIntegrityCheck = new TnefPropertyTag (TnefPropertyId.ContentIntegrityCheck, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ContentLength = new TnefPropertyTag (TnefPropertyId.ContentLength, TnefPropertyType.Long);
		public static readonly TnefPropertyTag ContentReturnRequested = new TnefPropertyTag (TnefPropertyId.ContentReturnRequested, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag ContentsSortOrder = new TnefPropertyTag (TnefPropertyId.ContentsSortOrder, TnefPropertyType.Long, true);
		public static readonly TnefPropertyTag ContentUnread = new TnefPropertyTag (TnefPropertyId.ContentUnread, TnefPropertyType.Long);
		public static readonly TnefPropertyTag ControlFlags = new TnefPropertyTag (TnefPropertyId.ControlFlags, TnefPropertyType.Long);
		public static readonly TnefPropertyTag ControlId = new TnefPropertyTag (TnefPropertyId.ControlId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ControlStructure = new TnefPropertyTag (TnefPropertyId.ControlStructure, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ControlType = new TnefPropertyTag (TnefPropertyId.ControlType, TnefPropertyType.Long);
		public static readonly TnefPropertyTag ConversationIndex = new TnefPropertyTag (TnefPropertyId.ConversationIndex, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ConversationKey = new TnefPropertyTag (TnefPropertyId.ConversationKey, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ConversationTopicA = new TnefPropertyTag (TnefPropertyId.ConversationTopic, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ConversationTopicW = new TnefPropertyTag (TnefPropertyId.ConversationTopic, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag ConversionEits = new TnefPropertyTag (TnefPropertyId.ConversionEits, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ConversionProhibited = new TnefPropertyTag (TnefPropertyId.ConversionProhibited, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag ConversionWithLossProhibited = new TnefPropertyTag (TnefPropertyId.ConversionWithLossProhibited, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag ConvertedEits = new TnefPropertyTag (TnefPropertyId.ConvertedEits, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag Correlate = new TnefPropertyTag (TnefPropertyId.Correlate, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag CorrelateMtsid = new TnefPropertyTag (TnefPropertyId.CorrelateMtsid, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag CountryA = new TnefPropertyTag (TnefPropertyId.Country, TnefPropertyType.String8);
		public static readonly TnefPropertyTag CountryW = new TnefPropertyTag (TnefPropertyId.Country, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag CreateTemplates = new TnefPropertyTag (TnefPropertyId.CreateTemplates, TnefPropertyType.Object);
		public static readonly TnefPropertyTag CreationTime = new TnefPropertyTag (TnefPropertyId.CreationTime, TnefPropertyType.SysTime);
		public static readonly TnefPropertyTag CreationVersion = new TnefPropertyTag (TnefPropertyId.CreationVersion, TnefPropertyType.I8);
		public static readonly TnefPropertyTag CurrentVersion = new TnefPropertyTag (TnefPropertyId.CurrentVersion, TnefPropertyType.I8);
		public static readonly TnefPropertyTag CustomerIdA = new TnefPropertyTag (TnefPropertyId.CustomerId, TnefPropertyType.String8);
		public static readonly TnefPropertyTag CustomerIdW = new TnefPropertyTag (TnefPropertyId.CustomerId, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag DefaultProfile = new TnefPropertyTag (TnefPropertyId.DefaultProfile, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag DefaultStore = new TnefPropertyTag (TnefPropertyId.DefaultStore, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag DefaultViewEntryId = new TnefPropertyTag (TnefPropertyId.DefaultViewEntryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag DefCreateDl = new TnefPropertyTag (TnefPropertyId.DefCreateDl, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag DefCreateMailuser = new TnefPropertyTag (TnefPropertyId.DefCreateMailuser, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag DeferredDeliveryTime = new TnefPropertyTag (TnefPropertyId.DeferredDeliveryTime, TnefPropertyType.SysTime);
		public static readonly TnefPropertyTag Delegation = new TnefPropertyTag (TnefPropertyId.Delegation, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag DeleteAfterSubmit = new TnefPropertyTag (TnefPropertyId.DeleteAfterSubmit, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag DeliverTime = new TnefPropertyTag (TnefPropertyId.DeliverTime, TnefPropertyType.SysTime);
		public static readonly TnefPropertyTag DeliveryPoint = new TnefPropertyTag (TnefPropertyId.DeliveryPoint, TnefPropertyType.Long);
		public static readonly TnefPropertyTag Deltax = new TnefPropertyTag (TnefPropertyId.Deltax, TnefPropertyType.Long);
		public static readonly TnefPropertyTag Deltay = new TnefPropertyTag (TnefPropertyId.Deltay, TnefPropertyType.Long);
		public static readonly TnefPropertyTag DepartmentNameA = new TnefPropertyTag (TnefPropertyId.DepartmentName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag DepartmentNameW = new TnefPropertyTag (TnefPropertyId.DepartmentName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag Depth = new TnefPropertyTag (TnefPropertyId.Depth, TnefPropertyType.Long);

		public static readonly TnefPropertyTag DetailsTable = new TnefPropertyTag (TnefPropertyId.DetailsTable, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag DiscardReason = new TnefPropertyTag (TnefPropertyId.DiscardReason, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag DiscloseRecipients = new TnefPropertyTag (TnefPropertyId.DiscloseRecipients, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag DisclosureOfRecipients = new TnefPropertyTag (TnefPropertyId.DisclosureOfRecipients, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag DiscreteValues = new TnefPropertyTag (TnefPropertyId.DiscreteValues, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag DiscVal = new TnefPropertyTag (TnefPropertyId.DiscVal, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag DisplayBcc = new TnefPropertyTag (TnefPropertyId.DisplayBcc, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag DisplayCc = new TnefPropertyTag (TnefPropertyId.DisplayCc, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag DisplayName = new TnefPropertyTag (TnefPropertyId.DisplayName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag DisplayNamePrefix = new TnefPropertyTag (TnefPropertyId.DisplayNamePrefix, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag DisplayTo = new TnefPropertyTag (TnefPropertyId.DisplayTo, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag DisplayType = new TnefPropertyTag (TnefPropertyId.DisplayType, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag DlExpansionHistory = new TnefPropertyTag (TnefPropertyId.DlExpansionHistory, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag DlExpansionProhibited = new TnefPropertyTag (TnefPropertyId.DlExpansionProhibited, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag EmailAddress = new TnefPropertyTag (TnefPropertyId.EmailAddress, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag EndDate = new TnefPropertyTag (TnefPropertyId.EndDate, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag EntryId = new TnefPropertyTag (TnefPropertyId.EntryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ExpandBeginTime = new TnefPropertyTag (TnefPropertyId.ExpandBeginTime, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ExpandedBeginTime = new TnefPropertyTag (TnefPropertyId.ExpandedBeginTime, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ExpandedEndTime = new TnefPropertyTag (TnefPropertyId.ExpandedEndTime, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ExpandEndTime = new TnefPropertyTag (TnefPropertyId.ExpandEndTime, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ExpiryTime = new TnefPropertyTag (TnefPropertyId.ExpiryTime, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ExplicitConversion = new TnefPropertyTag (TnefPropertyId.ExplicitConversion, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag FilteringHooks = new TnefPropertyTag (TnefPropertyId.FilteringHooks, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag FinderEntryId = new TnefPropertyTag (TnefPropertyId.FinderEntryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag FolderAssociatedContents = new TnefPropertyTag (TnefPropertyId.FolderAssociatedContents, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag FolderType = new TnefPropertyTag (TnefPropertyId.FolderType, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag FormCategory = new TnefPropertyTag (TnefPropertyId.FormCategory, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag FormCategorySub = new TnefPropertyTag (TnefPropertyId.FormCategorySub, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag FormClsid = new TnefPropertyTag (TnefPropertyId.FormClsid, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag FormContactName = new TnefPropertyTag (TnefPropertyId.FormContactName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag FormDesignerGuid = new TnefPropertyTag (TnefPropertyId.FormDesignerGuid, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag FormDesignerName = new TnefPropertyTag (TnefPropertyId.FormDesignerName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag FormHidden = new TnefPropertyTag (TnefPropertyId.FormHidden, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag FormHostMap = new TnefPropertyTag (TnefPropertyId.FormHostMap, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag FormMessageBehavior = new TnefPropertyTag (TnefPropertyId.FormMessageBehavior, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag FormVersion = new TnefPropertyTag (TnefPropertyId.FormVersion, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag FtpSite = new TnefPropertyTag (TnefPropertyId.FtpSite, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Gender = new TnefPropertyTag (TnefPropertyId.Gender, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Generation = new TnefPropertyTag (TnefPropertyId.Generation, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag GivenName = new TnefPropertyTag (TnefPropertyId.GivenName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag GovernmentIdNumber = new TnefPropertyTag (TnefPropertyId.GovernmentIdNumber, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Hasattach = new TnefPropertyTag (TnefPropertyId.Hasattach, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag HeaderFolderEntryId = new TnefPropertyTag (TnefPropertyId.HeaderFolderEntryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Hobbies = new TnefPropertyTag (TnefPropertyId.Hobbies, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Home2TelephoneNumber = new TnefPropertyTag (TnefPropertyId.Home2TelephoneNumber, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag HomeAddressCity = new TnefPropertyTag (TnefPropertyId.HomeAddressCity, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag HomeAddressCountry = new TnefPropertyTag (TnefPropertyId.HomeAddressCountry, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag HomeAddressPostalCode = new TnefPropertyTag (TnefPropertyId.HomeAddressPostalCode, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag HomeAddressPostOfficeBox = new TnefPropertyTag (TnefPropertyId.HomeAddressPostOfficeBox, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag HomeAddressStateOrProvince = new TnefPropertyTag (TnefPropertyId.HomeAddressStateOrProvince, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag HomeAddressStreet = new TnefPropertyTag (TnefPropertyId.HomeAddressStreet, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag HomeFaxNumber = new TnefPropertyTag (TnefPropertyId.HomeFaxNumber, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag HomeTelephoneNumber = new TnefPropertyTag (TnefPropertyId.HomeTelephoneNumber, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Icon = new TnefPropertyTag (TnefPropertyId.Icon, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag IdentityDisplay = new TnefPropertyTag (TnefPropertyId.IdentityDisplay, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag IdentityEntryId = new TnefPropertyTag (TnefPropertyId.IdentityEntryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag IdentitySearchKey = new TnefPropertyTag (TnefPropertyId.IdentitySearchKey, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ImplicitConversionProhibited = new TnefPropertyTag (TnefPropertyId.ImplicitConversionProhibited, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Importance = new TnefPropertyTag (TnefPropertyId.Importance, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag IncompleteCopy = new TnefPropertyTag (TnefPropertyId.IncompleteCopy, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag INetMailOverrideCharset = new TnefPropertyTag (TnefPropertyId.INetMailOverrideCharset, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag INetMailOverrideFormat = new TnefPropertyTag (TnefPropertyId.INetMailOverrideFormat, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag InitialDetailsPane = new TnefPropertyTag (TnefPropertyId.InitialDetailsPane, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Initials = new TnefPropertyTag (TnefPropertyId.Initials, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag InReplyToId = new TnefPropertyTag (TnefPropertyId.InReplyToId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag InstanceKey = new TnefPropertyTag (TnefPropertyId.InstanceKey, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag InternetApproved = new TnefPropertyTag (TnefPropertyId.InternetApproved, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag InternetArticleNumber = new TnefPropertyTag (TnefPropertyId.InternetArticleNumber, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag InternetControl = new TnefPropertyTag (TnefPropertyId.InternetControl, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag InternetCPID = new TnefPropertyTag (TnefPropertyId.InternetCPID, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag InternetDistribution = new TnefPropertyTag (TnefPropertyId.InternetDistribution, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag InternetFollowupTo = new TnefPropertyTag (TnefPropertyId.InternetFollowupTo, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag InternetLines = new TnefPropertyTag (TnefPropertyId.InternetLines, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag InternetMessageId = new TnefPropertyTag (TnefPropertyId.InternetMessageId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag InternetNewsgroups = new TnefPropertyTag (TnefPropertyId.InternetNewsgroups, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag InternetNntpPath = new TnefPropertyTag (TnefPropertyId.InternetNntpPath, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag InternetOrganization = new TnefPropertyTag (TnefPropertyId.InternetOrganization, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag InternetPrecedence = new TnefPropertyTag (TnefPropertyId.InternetPrecedence, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag InternetReferences = new TnefPropertyTag (TnefPropertyId.InternetReferences, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag IpmId = new TnefPropertyTag (TnefPropertyId.IpmId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag IpmOutboxEntryId = new TnefPropertyTag (TnefPropertyId.IpmOutboxEntryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag IpmOutboxSearchKey = new TnefPropertyTag (TnefPropertyId.IpmOutboxSearchKey, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag IpmReturnRequested = new TnefPropertyTag (TnefPropertyId.IpmReturnRequested, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag IpmSentmailEntryId = new TnefPropertyTag (TnefPropertyId.IpmSentmailEntryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag IpmSentmailSearchKey = new TnefPropertyTag (TnefPropertyId.IpmSentmailSearchKey, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag IpmSubtreeEntryId = new TnefPropertyTag (TnefPropertyId.IpmSubtreeEntryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag IpmSubtreeSearchKey = new TnefPropertyTag (TnefPropertyId.IpmSubtreeSearchKey, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag IpmWastebasketEntryId = new TnefPropertyTag (TnefPropertyId.IpmWastebasketEntryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag IpmWastebasketSearchKey = new TnefPropertyTag (TnefPropertyId.IpmWastebasketSearchKey, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag IsdnNumber = new TnefPropertyTag (TnefPropertyId.IsdnNumber, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Keyword = new TnefPropertyTag (TnefPropertyId.Keyword, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Language = new TnefPropertyTag (TnefPropertyId.Language, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Languages = new TnefPropertyTag (TnefPropertyId.Languages, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag LastModificationTime = new TnefPropertyTag (TnefPropertyId.LastModificationTime, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag LatestDeliveryTime = new TnefPropertyTag (TnefPropertyId.LatestDeliveryTime, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ListHelp = new TnefPropertyTag (TnefPropertyId.ListHelp, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ListSubscribe = new TnefPropertyTag (TnefPropertyId.ListSubscribe, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ListUnsubscribe = new TnefPropertyTag (TnefPropertyId.ListUnsubscribe, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Locality = new TnefPropertyTag (TnefPropertyId.Locality, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag LocallyDelivered = new TnefPropertyTag (TnefPropertyId.LocallyDelivered, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Location = new TnefPropertyTag (TnefPropertyId.Location, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag LockBranchId = new TnefPropertyTag (TnefPropertyId.LockBranchId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag LockDepth = new TnefPropertyTag (TnefPropertyId.LockDepth, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag LockEnlistmentContext = new TnefPropertyTag (TnefPropertyId.LockEnlistmentContext, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag LockExpiryTime = new TnefPropertyTag (TnefPropertyId.LockExpiryTime, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag LockPersistent = new TnefPropertyTag (TnefPropertyId.LockPersistent, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag LockResourceDid = new TnefPropertyTag (TnefPropertyId.LockResourceDid, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag LockResourceFid = new TnefPropertyTag (TnefPropertyId.LockResourceFid, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag LockResourceMid = new TnefPropertyTag (TnefPropertyId.LockResourceMid, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag LockScope = new TnefPropertyTag (TnefPropertyId.LockScope, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag LockTimeout = new TnefPropertyTag (TnefPropertyId.LockTimeout, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag LockType = new TnefPropertyTag (TnefPropertyId.LockType, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag MailPermission = new TnefPropertyTag (TnefPropertyId.MailPermission, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ManagerName = new TnefPropertyTag (TnefPropertyId.ManagerName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag MappingSignature = new TnefPropertyTag (TnefPropertyId.MappingSignature, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag MdbProvider = new TnefPropertyTag (TnefPropertyId.MdbProvider, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag MessageAttachments = new TnefPropertyTag (TnefPropertyId.MessageAttachments, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag MessageCcMe = new TnefPropertyTag (TnefPropertyId.MessageCcMe, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag MessageClass = new TnefPropertyTag (TnefPropertyId.MessageClass, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag MessageCodepage = new TnefPropertyTag (TnefPropertyId.MessageCodepage, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag MessageDeliveryId = new TnefPropertyTag (TnefPropertyId.MessageDeliveryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag MessageDeliveryTime = new TnefPropertyTag (TnefPropertyId.MessageDeliveryTime, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag MessageDownloadTime = new TnefPropertyTag (TnefPropertyId.MessageDownloadTime, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag MessageFlags = new TnefPropertyTag (TnefPropertyId.MessageFlags, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag MessageRecipients = new TnefPropertyTag (TnefPropertyId.MessageRecipients, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag MessageRecipMe = new TnefPropertyTag (TnefPropertyId.MessageRecipMe, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag MessageSecurityLabel = new TnefPropertyTag (TnefPropertyId.MessageSecurityLabel, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag MessageSize = new TnefPropertyTag (TnefPropertyId.MessageSize, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag MessageSubmissionId = new TnefPropertyTag (TnefPropertyId.MessageSubmissionId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag MessageToken = new TnefPropertyTag (TnefPropertyId.MessageToken, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag MessageToMe = new TnefPropertyTag (TnefPropertyId.MessageToMe, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag MhsCommonName = new TnefPropertyTag (TnefPropertyId.MhsCommonName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag MiddleName = new TnefPropertyTag (TnefPropertyId.MiddleName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag MiniIcon = new TnefPropertyTag (TnefPropertyId.MiniIcon, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag MobileTelephoneNumber = new TnefPropertyTag (TnefPropertyId.MobileTelephoneNumber, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ModifyVersion = new TnefPropertyTag (TnefPropertyId.ModifyVersion, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag MsgStatus = new TnefPropertyTag (TnefPropertyId.MsgStatus, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag NdrDiagCode = new TnefPropertyTag (TnefPropertyId.NdrDiagCode, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag NdrReasonCode = new TnefPropertyTag (TnefPropertyId.NdrReasonCode, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag NdrStatusCode = new TnefPropertyTag (TnefPropertyId.NdrStatusCode, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag NewsgroupName = new TnefPropertyTag (TnefPropertyId.NewsgroupName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Nickname = new TnefPropertyTag (TnefPropertyId.Nickname, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag NntpXref = new TnefPropertyTag (TnefPropertyId.NntpXref, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag NonReceiptNotificationRequested = new TnefPropertyTag (TnefPropertyId.NonReceiptNotificationRequested, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag NonReceiptReason = new TnefPropertyTag (TnefPropertyId.NonReceiptReason, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag NormalizedSubject = new TnefPropertyTag (TnefPropertyId.NormalizedSubject, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag NtSecurityDescriptor = new TnefPropertyTag (TnefPropertyId.NtSecurityDescriptor, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Null = new TnefPropertyTag (TnefPropertyId.Null, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ObjectType = new TnefPropertyTag (TnefPropertyId.ObjectType, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ObsoletedIpms = new TnefPropertyTag (TnefPropertyId.ObsoletedIpms, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Office2TelephoneNumber = new TnefPropertyTag (TnefPropertyId.Office2TelephoneNumber, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OfficeLocation = new TnefPropertyTag (TnefPropertyId.OfficeLocation, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OfficeTelephoneNumber = new TnefPropertyTag (TnefPropertyId.OfficeTelephoneNumber, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OofReplyType = new TnefPropertyTag (TnefPropertyId.OofReplyType, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OrganizationalIdNumber = new TnefPropertyTag (TnefPropertyId.OrganizationalIdNumber, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OrigEntryId = new TnefPropertyTag (TnefPropertyId.OrigEntryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalAuthorAddrtype = new TnefPropertyTag (TnefPropertyId.OriginalAuthorAddrtype, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalAuthorEmailAddress = new TnefPropertyTag (TnefPropertyId.OriginalAuthorEmailAddress, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalAuthorEntryId = new TnefPropertyTag (TnefPropertyId.OriginalAuthorEntryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalAuthorName = new TnefPropertyTag (TnefPropertyId.OriginalAuthorName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalAuthorSearchKey = new TnefPropertyTag (TnefPropertyId.OriginalAuthorSearchKey, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalDeliveryTime = new TnefPropertyTag (TnefPropertyId.OriginalDeliveryTime, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalDisplayBcc = new TnefPropertyTag (TnefPropertyId.OriginalDisplayBcc, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalDisplayCc = new TnefPropertyTag (TnefPropertyId.OriginalDisplayCc, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalDisplayName = new TnefPropertyTag (TnefPropertyId.OriginalDisplayName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalDisplayTo = new TnefPropertyTag (TnefPropertyId.OriginalDisplayTo, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalEits = new TnefPropertyTag (TnefPropertyId.OriginalEits, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalEntryId = new TnefPropertyTag (TnefPropertyId.OriginalEntryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginallyIntendedRecipAddrtype = new TnefPropertyTag (TnefPropertyId.OriginallyIntendedRecipAddrtype, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginallyIntendedRecipEmailAddress = new TnefPropertyTag (TnefPropertyId.OriginallyIntendedRecipEmailAddress, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginallyIntendedRecipEntryId = new TnefPropertyTag (TnefPropertyId.OriginallyIntendedRecipEntryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginallyIntendedRecipientName = new TnefPropertyTag (TnefPropertyId.OriginallyIntendedRecipientName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalSearchKey = new TnefPropertyTag (TnefPropertyId.OriginalSearchKey, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalSenderAddrtype = new TnefPropertyTag (TnefPropertyId.OriginalSenderAddrtype, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalSenderEmailAddress = new TnefPropertyTag (TnefPropertyId.OriginalSenderEmailAddress, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalSenderEntryId = new TnefPropertyTag (TnefPropertyId.OriginalSenderEntryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalSenderName = new TnefPropertyTag (TnefPropertyId.OriginalSenderName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalSenderSearchKey = new TnefPropertyTag (TnefPropertyId.OriginalSenderSearchKey, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalSensitivity = new TnefPropertyTag (TnefPropertyId.OriginalSensitivity, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalSentRepresentingAddrtype = new TnefPropertyTag (TnefPropertyId.OriginalSentRepresentingAddrtype, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalSentRepresentingEmailAddress = new TnefPropertyTag (TnefPropertyId.OriginalSentRepresentingEmailAddress, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalSentRepresentingEntryId = new TnefPropertyTag (TnefPropertyId.OriginalSentRepresentingEntryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalSentRepresentingName = new TnefPropertyTag (TnefPropertyId.OriginalSentRepresentingName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalSentRepresentingSearchKey = new TnefPropertyTag (TnefPropertyId.OriginalSentRepresentingSearchKey, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalSubject = new TnefPropertyTag (TnefPropertyId.OriginalSubject, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalSubmitTime = new TnefPropertyTag (TnefPropertyId.OriginalSubmitTime, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginatingMtaCertificate = new TnefPropertyTag (TnefPropertyId.OriginatingMtaCertificate, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginatorAndDlExpansionHistory = new TnefPropertyTag (TnefPropertyId.OriginatorAndDlExpansionHistory, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginatorCertificate = new TnefPropertyTag (TnefPropertyId.OriginatorCertificate, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginatorDeliveryReportRequested = new TnefPropertyTag (TnefPropertyId.OriginatorDeliveryReportRequested, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginatorNonDeliveryReportRequested = new TnefPropertyTag (TnefPropertyId.OriginatorNonDeliveryReportRequested, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginatorRequestedAlternateRecipient = new TnefPropertyTag (TnefPropertyId.OriginatorRequestedAlternateRecipient, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginatorReturnAddress = new TnefPropertyTag (TnefPropertyId.OriginatorReturnAddress, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginCheck = new TnefPropertyTag (TnefPropertyId.OriginCheck, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OrigMessageClass = new TnefPropertyTag (TnefPropertyId.OrigMessageClass, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OtherAddressCity = new TnefPropertyTag (TnefPropertyId.OtherAddressCity, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OtherAddressCountry = new TnefPropertyTag (TnefPropertyId.OtherAddressCountry, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OtherAddressPostalCode = new TnefPropertyTag (TnefPropertyId.OtherAddressPostalCode, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OtherAddressPostOfficeBox = new TnefPropertyTag (TnefPropertyId.OtherAddressPostOfficeBox, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OtherAddressStateOrProvince = new TnefPropertyTag (TnefPropertyId.OtherAddressStateOrProvince, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OtherAddressStreet = new TnefPropertyTag (TnefPropertyId.OtherAddressStreet, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OtherTelephoneNumber = new TnefPropertyTag (TnefPropertyId.OtherTelephoneNumber, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OwnerApptId = new TnefPropertyTag (TnefPropertyId.OwnerApptId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OwnStoreEntryId = new TnefPropertyTag (TnefPropertyId.OwnStoreEntryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag PagerTelephoneNumber = new TnefPropertyTag (TnefPropertyId.PagerTelephoneNumber, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ParentDisplay = new TnefPropertyTag (TnefPropertyId.ParentDisplay, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ParentEntryId = new TnefPropertyTag (TnefPropertyId.ParentEntryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ParentKey = new TnefPropertyTag (TnefPropertyId.ParentKey, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag PersonalHomePage = new TnefPropertyTag (TnefPropertyId.PersonalHomePage, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag PhysicalDeliveryBureauFaxDelivery = new TnefPropertyTag (TnefPropertyId.PhysicalDeliveryBureauFaxDelivery, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag PhysicalDeliveryMode = new TnefPropertyTag (TnefPropertyId.PhysicalDeliveryMode, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag PhysicalDeliveryReportRequest = new TnefPropertyTag (TnefPropertyId.PhysicalDeliveryReportRequest, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag PhysicalForwardingAddress = new TnefPropertyTag (TnefPropertyId.PhysicalForwardingAddress, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag PhysicalForwardingAddressRequested = new TnefPropertyTag (TnefPropertyId.PhysicalForwardingAddressRequested, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag PhysicalForwardingProhibited = new TnefPropertyTag (TnefPropertyId.PhysicalForwardingProhibited, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag PhysicalRenditionAttributes = new TnefPropertyTag (TnefPropertyId.PhysicalRenditionAttributes, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag PostalAddress = new TnefPropertyTag (TnefPropertyId.PostalAddress, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag PostalCode = new TnefPropertyTag (TnefPropertyId.PostalCode, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag PostFolderEntries = new TnefPropertyTag (TnefPropertyId.PostFolderEntries, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag PostFolderNames = new TnefPropertyTag (TnefPropertyId.PostFolderNames, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag PostOfficeBox = new TnefPropertyTag (TnefPropertyId.PostOfficeBox, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag PostReplyDenied = new TnefPropertyTag (TnefPropertyId.PostReplyDenied, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag PostReplyFolderEntries = new TnefPropertyTag (TnefPropertyId.PostReplyFolderEntries, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag PostReplyFolderNames = new TnefPropertyTag (TnefPropertyId.PostReplyFolderNames, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag PreferredByName = new TnefPropertyTag (TnefPropertyId.PreferredByName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Preprocess = new TnefPropertyTag (TnefPropertyId.Preprocess, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag PrimaryCapability = new TnefPropertyTag (TnefPropertyId.PrimaryCapability, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag PrimaryFaxNumber = new TnefPropertyTag (TnefPropertyId.PrimaryFaxNumber, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag PrimaryTelephoneNumber = new TnefPropertyTag (TnefPropertyId.PrimaryTelephoneNumber, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Priority = new TnefPropertyTag (TnefPropertyId.Priority, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Profession = new TnefPropertyTag (TnefPropertyId.Profession, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ProfileName = new TnefPropertyTag (TnefPropertyId.ProfileName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ProofOfDelivery = new TnefPropertyTag (TnefPropertyId.ProofOfDelivery, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ProofOfDeliveryRequested = new TnefPropertyTag (TnefPropertyId.ProofOfDeliveryRequested, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ProofOfSubmission = new TnefPropertyTag (TnefPropertyId.ProofOfSubmission, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ProofOfSubmissionRequested = new TnefPropertyTag (TnefPropertyId.ProofOfSubmissionRequested, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag PropIdSecureMax = new TnefPropertyTag (TnefPropertyId.PropIdSecureMax, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag PropIdSecureMin = new TnefPropertyTag (TnefPropertyId.PropIdSecureMin, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ProviderDisplay = new TnefPropertyTag (TnefPropertyId.ProviderDisplay, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ProviderDllName = new TnefPropertyTag (TnefPropertyId.ProviderDllName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ProviderOrdinal = new TnefPropertyTag (TnefPropertyId.ProviderOrdinal, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ProviderSubmitTime = new TnefPropertyTag (TnefPropertyId.ProviderSubmitTime, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ProviderUid = new TnefPropertyTag (TnefPropertyId.ProviderUid, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Puid = new TnefPropertyTag (TnefPropertyId.Puid, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RadioTelephoneNumber = new TnefPropertyTag (TnefPropertyId.RadioTelephoneNumber, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RcvdRepresentingAddrtype = new TnefPropertyTag (TnefPropertyId.RcvdRepresentingAddrtype, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RcvdRepresentingEmailAddress = new TnefPropertyTag (TnefPropertyId.RcvdRepresentingEmailAddress, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RcvdRepresentingEntryId = new TnefPropertyTag (TnefPropertyId.RcvdRepresentingEntryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RcvdRepresentingName = new TnefPropertyTag (TnefPropertyId.RcvdRepresentingName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RcvdRepresentingSearchKey = new TnefPropertyTag (TnefPropertyId.RcvdRepresentingSearchKey, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ReadReceiptEntryId = new TnefPropertyTag (TnefPropertyId.ReadReceiptEntryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ReadReceiptRequested = new TnefPropertyTag (TnefPropertyId.ReadReceiptRequested, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ReadReceiptSearchKey = new TnefPropertyTag (TnefPropertyId.ReadReceiptSearchKey, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ReceiptTime = new TnefPropertyTag (TnefPropertyId.ReceiptTime, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ReceivedByAddrtype = new TnefPropertyTag (TnefPropertyId.ReceivedByAddrtype, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ReceivedByEmailAddress = new TnefPropertyTag (TnefPropertyId.ReceivedByEmailAddress, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ReceivedByEntryId = new TnefPropertyTag (TnefPropertyId.ReceivedByEntryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ReceivedByName = new TnefPropertyTag (TnefPropertyId.ReceivedByName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ReceivedBySearchKey = new TnefPropertyTag (TnefPropertyId.ReceivedBySearchKey, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ReceiveFolderSettings = new TnefPropertyTag (TnefPropertyId.ReceiveFolderSettings, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RecipientCertificate = new TnefPropertyTag (TnefPropertyId.RecipientCertificate, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RecipientNumberForAdvice = new TnefPropertyTag (TnefPropertyId.RecipientNumberForAdvice, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RecipientReassignmentProhibited = new TnefPropertyTag (TnefPropertyId.RecipientReassignmentProhibited, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RecipientStatus = new TnefPropertyTag (TnefPropertyId.RecipientStatus, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RecipientType = new TnefPropertyTag (TnefPropertyId.RecipientType, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RecordKey = new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RedirectionHistory = new TnefPropertyTag (TnefPropertyId.RedirectionHistory, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ReferredByName = new TnefPropertyTag (TnefPropertyId.ReferredByName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RegisteredMailType = new TnefPropertyTag (TnefPropertyId.RegisteredMailType, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RelatedIpms = new TnefPropertyTag (TnefPropertyId.RelatedIpms, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RemoteProgress = new TnefPropertyTag (TnefPropertyId.RemoteProgress, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RemoteProgressText = new TnefPropertyTag (TnefPropertyId.RemoteProgressText, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RemoteValidateOk = new TnefPropertyTag (TnefPropertyId.RemoteValidateOk, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RenderingPosition = new TnefPropertyTag (TnefPropertyId.RenderingPosition, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ReplyRecipientEntries = new TnefPropertyTag (TnefPropertyId.ReplyRecipientEntries, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ReplyRecipientNames = new TnefPropertyTag (TnefPropertyId.ReplyRecipientNames, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ReplyRequested = new TnefPropertyTag (TnefPropertyId.ReplyRequested, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ReplyTime = new TnefPropertyTag (TnefPropertyId.ReplyTime, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ReportEntryId = new TnefPropertyTag (TnefPropertyId.ReportEntryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ReportingDlName = new TnefPropertyTag (TnefPropertyId.ReportingDlName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ReportingMtaCertificate = new TnefPropertyTag (TnefPropertyId.ReportingMtaCertificate, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ReportName = new TnefPropertyTag (TnefPropertyId.ReportName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ReportSearchKey = new TnefPropertyTag (TnefPropertyId.ReportSearchKey, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ReportTag = new TnefPropertyTag (TnefPropertyId.ReportTag, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ReportText = new TnefPropertyTag (TnefPropertyId.ReportText, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ReportTime = new TnefPropertyTag (TnefPropertyId.ReportTime, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RequestedDeliveryMethod = new TnefPropertyTag (TnefPropertyId.RequestedDeliveryMethod, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ResourceFlags = new TnefPropertyTag (TnefPropertyId.ResourceFlags, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ResourceMethods = new TnefPropertyTag (TnefPropertyId.ResourceMethods, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ResourcePath = new TnefPropertyTag (TnefPropertyId.ResourcePath, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ResourceType = new TnefPropertyTag (TnefPropertyId.ResourceType, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ResponseRequested = new TnefPropertyTag (TnefPropertyId.ResponseRequested, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Responsibility = new TnefPropertyTag (TnefPropertyId.Responsibility, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ReturnedIpm = new TnefPropertyTag (TnefPropertyId.ReturnedIpm, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Rowid = new TnefPropertyTag (TnefPropertyId.Rowid, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RowType = new TnefPropertyTag (TnefPropertyId.RowType, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RtfCompressed = new TnefPropertyTag (TnefPropertyId.RtfCompressed, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RtfInSync = new TnefPropertyTag (TnefPropertyId.RtfInSync, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RtfSyncBodyCount = new TnefPropertyTag (TnefPropertyId.RtfSyncBodyCount, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RtfSyncBodyCrc = new TnefPropertyTag (TnefPropertyId.RtfSyncBodyCrc, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RtfSyncBodyTag = new TnefPropertyTag (TnefPropertyId.RtfSyncBodyTag, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RtfSyncPrefixCount = new TnefPropertyTag (TnefPropertyId.RtfSyncPrefixCount, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RtfSyncTrailingCount = new TnefPropertyTag (TnefPropertyId.RtfSyncTrailingCount, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Search = new TnefPropertyTag (TnefPropertyId.Search, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag SearchKey = new TnefPropertyTag (TnefPropertyId.SearchKey, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Security = new TnefPropertyTag (TnefPropertyId.Security, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Selectable = new TnefPropertyTag (TnefPropertyId.Selectable, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag SenderAddrtype = new TnefPropertyTag (TnefPropertyId.SenderAddrtype, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag SenderEmailAddress = new TnefPropertyTag (TnefPropertyId.SenderEmailAddress, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag SenderEntryId = new TnefPropertyTag (TnefPropertyId.SenderEntryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag SenderName = new TnefPropertyTag (TnefPropertyId.SenderName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag SenderSearchKey = new TnefPropertyTag (TnefPropertyId.SenderSearchKey, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag SendInternetEncoding = new TnefPropertyTag (TnefPropertyId.SendInternetEncoding, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag SendRecallReport = new TnefPropertyTag (TnefPropertyId.SendRecallReport, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag SendRichInfo = new TnefPropertyTag (TnefPropertyId.SendRichInfo, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Sensitivity = new TnefPropertyTag (TnefPropertyId.Sensitivity, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag SentmailEntryId = new TnefPropertyTag (TnefPropertyId.SentmailEntryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag SentRepresentingAddrtype = new TnefPropertyTag (TnefPropertyId.SentRepresentingAddrtype, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag SentRepresentingEmailAddress = new TnefPropertyTag (TnefPropertyId.SentRepresentingEmailAddress, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag SentRepresentingEntryId = new TnefPropertyTag (TnefPropertyId.SentRepresentingEntryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag SentRepresentingName = new TnefPropertyTag (TnefPropertyId.SentRepresentingName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag SentRepresentingSearchKey = new TnefPropertyTag (TnefPropertyId.SentRepresentingSearchKey, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ServiceDeleteFiles = new TnefPropertyTag (TnefPropertyId.ServiceDeleteFiles, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ServiceDllName = new TnefPropertyTag (TnefPropertyId.ServiceDllName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ServiceEntryName = new TnefPropertyTag (TnefPropertyId.ServiceEntryName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ServiceExtraUids = new TnefPropertyTag (TnefPropertyId.ServiceExtraUids, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ServiceName = new TnefPropertyTag (TnefPropertyId.ServiceName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Services = new TnefPropertyTag (TnefPropertyId.Services, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ServiceSupportFiles = new TnefPropertyTag (TnefPropertyId.ServiceSupportFiles, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ServiceUid = new TnefPropertyTag (TnefPropertyId.ServiceUid, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag SevenBitDisplayName = new TnefPropertyTag (TnefPropertyId.SevenBitDisplayName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag SmtpAddress = new TnefPropertyTag (TnefPropertyId.SmtpAddress, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag SpoolerStatus = new TnefPropertyTag (TnefPropertyId.SpoolerStatus, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag SpouseName = new TnefPropertyTag (TnefPropertyId.SpouseName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag StartDate = new TnefPropertyTag (TnefPropertyId.StartDate, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag StateOrProvince = new TnefPropertyTag (TnefPropertyId.StateOrProvince, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Status = new TnefPropertyTag (TnefPropertyId.Status, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag StatusCode = new TnefPropertyTag (TnefPropertyId.StatusCode, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag StatusString = new TnefPropertyTag (TnefPropertyId.StatusString, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag StoreEntryId = new TnefPropertyTag (TnefPropertyId.StoreEntryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag StoreProviders = new TnefPropertyTag (TnefPropertyId.StoreProviders, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag StoreRecordKey = new TnefPropertyTag (TnefPropertyId.StoreRecordKey, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag StoreState = new TnefPropertyTag (TnefPropertyId.StoreState, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag StoreSupportMask = new TnefPropertyTag (TnefPropertyId.StoreSupportMask, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag StreetAddress = new TnefPropertyTag (TnefPropertyId.StreetAddress, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Subfolders = new TnefPropertyTag (TnefPropertyId.Subfolders, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Subject = new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag SubjectIpm = new TnefPropertyTag (TnefPropertyId.SubjectIpm, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag SubjectPrefix = new TnefPropertyTag (TnefPropertyId.SubjectPrefix, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag SubmitFlags = new TnefPropertyTag (TnefPropertyId.SubmitFlags, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Supersedes = new TnefPropertyTag (TnefPropertyId.Supersedes, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag SupplementaryInfo = new TnefPropertyTag (TnefPropertyId.SupplementaryInfo, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Surname = new TnefPropertyTag (TnefPropertyId.Surname, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag TelexNumber = new TnefPropertyTag (TnefPropertyId.TelexNumber, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Templateid = new TnefPropertyTag (TnefPropertyId.Templateid, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Title = new TnefPropertyTag (TnefPropertyId.Title, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag TnefCorrelationKey = new TnefPropertyTag (TnefPropertyId.TnefCorrelationKey, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag TransmitableDisplayName = new TnefPropertyTag (TnefPropertyId.TransmitableDisplayName, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag TransportKey = new TnefPropertyTag (TnefPropertyId.TransportKey, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag TransportMessageHeaders = new TnefPropertyTag (TnefPropertyId.TransportMessageHeaders, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag TransportProviders = new TnefPropertyTag (TnefPropertyId.TransportProviders, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag TransportStatus = new TnefPropertyTag (TnefPropertyId.TransportStatus, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag TtytddPhoneNumber = new TnefPropertyTag (TnefPropertyId.TtytddPhoneNumber, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag TypeOfMtsUser = new TnefPropertyTag (TnefPropertyId.TypeOfMtsUser, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag UserCertificate = new TnefPropertyTag (TnefPropertyId.UserCertificate, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag UserX509Certificate = new TnefPropertyTag (TnefPropertyId.UserX509Certificate, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ValidFolderMask = new TnefPropertyTag (TnefPropertyId.ValidFolderMask, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ViewsEntryId = new TnefPropertyTag (TnefPropertyId.ViewsEntryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag WeddingAnniversary = new TnefPropertyTag (TnefPropertyId.WeddingAnniversary, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag X400ContentType = new TnefPropertyTag (TnefPropertyId.X400ContentType, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag X400DeferredDeliveryCancel = new TnefPropertyTag (TnefPropertyId.X400DeferredDeliveryCancel, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Xpos = new TnefPropertyTag (TnefPropertyId.Xpos, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Ypos = new TnefPropertyTag (TnefPropertyId.Ypos, TnefPropertyType.Unspecified);


		public TnefPropertyId Id {
			get { throw new NotImplementedException (); }
		}

		public bool IsMultiValued {
			get { throw new NotImplementedException (); }
		}

		public bool IsNamed {
			get { throw new NotImplementedException (); }
		}

		public bool IsTnefTypeValid {
			get { throw new NotImplementedException (); }
		}

		public TnefPropertyType TnefType {
			get { throw new NotImplementedException (); }
		}

		public TnefPropertyType ValueTnefType {
			get { throw new NotImplementedException (); }
		}

		public TnefPropertyTag (int tag)
		{
			throw new NotImplementedException ();
		}

		TnefPropertyTag (TnefPropertyId id, TnefPropertyType type, bool multiValue)
		{
			throw new NotImplementedException ();
		}

		public TnefPropertyTag (TnefPropertyId id, TnefPropertyType type) : this (id, type, false)
		{
		}

		public static implicit operator TnefPropertyTag (int tag)
		{
			return new TnefPropertyTag (tag);
		}

		public static implicit operator int (TnefPropertyTag tag)
		{
			throw new NotImplementedException ();
		}

		public override int GetHashCode ()
		{
			return ((int) this).GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			if (!(obj is TnefPropertyTag))
				return false;

			var tag = (TnefPropertyTag) obj;

			return tag.Id == Id && tag.TnefType == TnefType;
		}

		public override string ToString ()
		{
			return string.Format ("[TnefPropertyTag: Id={0}, IsMultiValues={1}, IsNamed={2}, IsTnefTypeValid={3}, TnefType={4}, ValueTnefType={5}]", Id, IsMultiValued, IsNamed, IsTnefTypeValid, TnefType, ValueTnefType);
		}

		public TnefPropertyTag ToUnicode ()
		{
			throw new NotImplementedException ();
		}
	}
}

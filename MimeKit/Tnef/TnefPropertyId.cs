//
// TnefPropertyId.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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
	/// A TNEF property identifier.
	/// </summary>
	/// <remarks>
	/// A TNEF property identifier.
	/// </remarks>
	public enum TnefPropertyId : short {
		/// <summary>
		/// The MAPI property PR_AB_DEFAULT_DIR.
		/// </summary>
		AbDefaultDir                             = 0x3D06,

		/// <summary>
		/// The MAPI property PR_AB_DEFAULT_PAB.
		/// </summary>
		AbDefaultPab                             = 0x3D07,

		/// <summary>
		/// The MAPI property PR_AB_PROVIDER_ID.
		/// </summary>
		AbProviderId                             = 0x3615,

		/// <summary>
		/// The MAPI property PR_AB_PROVIDERS.
		/// </summary>
		AbProviders                              = 0x3D01,

		/// <summary>
		/// The MAPI property PR_AB_SEARCH_PATH.
		/// </summary>
		AbSearchPath                             = 0x3D05,

		/// <summary>
		/// The MAPI property PR_AB_SEARCH_PATH_UPDATE.
		/// </summary>
		AbSearchPathUpdate                       = 0x3D11,

		/// <summary>
		/// The MAPI property PR_ACCESS.
		/// </summary>
		Access                                   = 0x0FF4,

		/// <summary>
		/// The MAPI property PR_ACCESS_LEVEL.
		/// </summary>
		AccessLevel                              = 0x0FF7,

		/// <summary>
		/// The MAPI property PR_ACCOUNT.
		/// </summary>
		Account                                  = 0x3A00,

		/// <summary>
		/// The MAPI property PR_ACKNOWLEDGEMENT_MODE.
		/// </summary>
		AcknowledgementMode                      = 0x0001,

		/// <summary>
		/// The MAPI property PR_ADDRTYPE.
		/// </summary>
		Addrtype                                 = 0x3002,

		/// <summary>
		/// The MAPI property PR_ALTERNATE_RECIPIENT.
		/// </summary>
		AlternateRecipient                       = 0x3A01,

		/// <summary>
		/// The MAPI property PR_ALTERNATE_RECIPIENT_ALLOWED.
		/// </summary>
		AlternateRecipientAllowed                = 0x0002,

		/// <summary>
		/// The MAPI property PR_ANR.
		/// </summary>
		Anr                                      = 0x360C,

		/// <summary>
		/// The MAPI property PR_ASSISTANT.
		/// </summary>
		Assistant                                = 0x3A30,

		/// <summary>
		/// The MAPI property PR_ASSISTANT_TELEPHONE_NUMBER.
		/// </summary>
		AssistantTelephoneNumber                 = 0x3A2E,

		/// <summary>
		/// The MAPI property PR_ASSOC_CONTENT_COUNT.
		/// </summary>
		AssocContentCount                        = 0x3617,

		/// <summary>
		/// The MAPI property PR_ATTACH_ADDITIONAL_INFO.
		/// </summary>
		AttachAdditionalInfo                     = 0x370F,

		/// <summary>
		/// The MAPI property PR_ATTACH_CONTENT_BASE.
		/// </summary>
		AttachContentBase                        = 0x3711,

		/// <summary>
		/// The MAPI property PR_ATTACH_CONTENT_ID.
		/// </summary>
		AttachContentId                          = 0x3712,

		/// <summary>
		/// The MAPI property PR_ATTACH_CONTENT_LOCATION.
		/// </summary>
		AttachContentLocation                    = 0x3713,

		/// <summary>
		/// The MAPI property PR_ATTACH_DATA_BIN or PR_ATTACH_DATA_OBJ.
		/// </summary>
		AttachData                               = 0x3701,

		/// <summary>
		/// The MAPI property PR_ATTACH_DISPOSITION.
		/// </summary>
		AttachDisposition                        = 0x3716,

		/// <summary>
		/// The MAPI property PR_ATTACH_ENCODING.
		/// </summary>
		AttachEncoding                           = 0x3702,

		/// <summary>
		/// The MAPI property PR_ATTACH_EXTENSION.
		/// </summary>
		AttachExtension                          = 0x3703,

		/// <summary>
		/// The MAPI property PR_ATTACH_FILENAME.
		/// </summary>
		AttachFilename                           = 0x3704,

		/// <summary>
		/// The MAPI property PR_ATTACH_FLAGS.
		/// </summary>
		AttachFlags                              = 0x3714,

		/// <summary>
		/// The MAPI property PR_ATTACH_LONG_FILENAME.
		/// </summary>
		AttachLongFilename                       = 0x3707,

		/// <summary>
		/// The MAPI property PR_ATTACH_LONG_PATHNAME.
		/// </summary>
		AttachLongPathname                       = 0x370D,

		/// <summary>
		/// The MAPI property PR_ATTACHMENT_X400_PARAMETERS.
		/// </summary>
		AttachmentX400Parameters                 = 0x3700,

		/// <summary>
		/// The MAPI property PR_ATTACH_METHOD.
		/// </summary>
		AttachMethod                             = 0x3705,

		/// <summary>
		/// The MAPI property PR_ATTACH_MIME_SEQUENCE.
		/// </summary>
		AttachMimeSequence                       = 0x3710,

		/// <summary>
		/// The MAPI property PR_ATTACH_MIME_TAG.
		/// </summary>
		AttachMimeTag                            = 0x370E,

		/// <summary>
		/// The MAPI property PR_ATTACH_NETSCAPE_MAC_INFO.
		/// </summary>
		AttachNetscapeMacInfo                    = 0x3715,

		/// <summary>
		/// The MAPI property PR_ATTACH_NUM.
		/// </summary>
		AttachNum                                = 0x0E21,

		/// <summary>
		/// The MAPI property PR_ATTACH_PATHNAME.
		/// </summary>
		AttachPathname                           = 0x3708,

		/// <summary>
		/// The MAPI property PR_ATTACH_RENDERING.
		/// </summary>
		AttachRendering                          = 0x3709,

		/// <summary>
		/// The MAPI property PR_ATTACH_SIZE.
		/// </summary>
		AttachSize                               = 0x0E20,

		/// <summary>
		/// The MAPI property PR_ATTACH_TAG.
		/// </summary>
		AttachTag                                = 0x370A,

		/// <summary>
		/// The MAPI property PR_ATTACH_TRANSPORT_NAME.
		/// </summary>
		AttachTransportName                      = 0x370C,

		/// <summary>
		/// The MAPI property PR_AUTHORIZING_USERS.
		/// </summary>
		AuthorizingUsers                         = 0x0003,

		/// <summary>
		/// The MAPI property PR_AUTO_FORWARDED.
		/// </summary>
		AutoForwarded                            = 0x0005,

		/// <summary>
		/// The MAPI property PR_AUTO_FORWARDING_COMMENT.
		/// </summary>
		AutoForwardingComment                    = 0x0004,

		/// <summary>
		/// The MAPI property PR_AUTO_RESPONSE_SUPPRESS.
		/// </summary>
		AutoResponseSuppress                     = 0x3FDF,

		/// <summary>
		/// The MAPI property PR_BEEPER_TELEPHONE_NUMBER.
		/// </summary>
		BeeperTelephoneNumber                    = 0x3A21,

		/// <summary>
		/// The MAPI property PR_BIRTHDAY.
		/// </summary>
		Birthday                                 = 0x3A42,

		/// <summary>
		/// The MAPI property PR_BODY.
		/// </summary>
		Body                                     = 0x1000,

		/// <summary>
		/// The MAPI property PR_BODY_CONTENT_ID.
		/// </summary>
		BodyContentId                            = 0x1015,

		/// <summary>
		/// The MAPI property PR_BODY_CONTENT_LOCATION.
		/// </summary>
		BodyContentLocation                      = 0x1014,

		/// <summary>
		/// The MAPI property PR_BODY_CRC.
		/// </summary>
		BodyCrc                                  = 0x0E1C,

		/// <summary>
		/// The MAPI property PR_BODY_HTML.
		/// </summary>
		BodyHtml                                 = 0x1013,

		/// <summary>
		/// The MAPI property PR_BUSINESS2_TELEPHONE_NUMBER.
		/// </summary>
		Business2TelephoneNumber                 = 0x3A1B,

		/// <summary>
		/// The MAPI property PR_BUSINESS_ADDRESS_CITY.
		/// </summary>
		BusinessAddressCity                      = 0x3A27,

		/// <summary>
		/// The MAPI property PR_BUSINESS_ADDRESS_COUNTRY.
		/// </summary>
		BusinessAddressCountry                   = 0x3A26,

		/// <summary>
		/// The MAPI property PR_BUSINESS_ADDRESS_POSTAL_CODE.
		/// </summary>
		BusinessAddressPostalCode                = 0x3A2A,

		/// <summary>
		/// The MAPI property PR_BUSINESS_ADDRESS_POST_OFFICE_BOX.
		/// </summary>
		BusinessAddressPostOfficeBox             = 0x3A2B,

		/// <summary>
		/// The MAPI property PR_BUSINESS_ADDRESS_STATE_OR_PROVINCE.
		/// </summary>
		BusinessAddressStateOrProvince           = 0x3A28,

		/// <summary>
		/// The MAPI property PR_BUSINESS_ADDRESS_STREET.
		/// </summary>
		BusinessAddressStreet                    = 0x3A29,

		/// <summary>
		/// The MAPI property PR_BUSINESS_FAX_NUMBER.
		/// </summary>
		BusinessFaxNumber                        = 0x3A24,

		/// <summary>
		/// The MAPI property PR_BUSINESS_HOME_PAGE.
		/// </summary>
		BusinessHomePage                         = 0x3A51,

		/// <summary>
		/// The MAPI property PR_CALLBACK_TELEPHONE_NUMBER.
		/// </summary>
		CallbackTelephoneNumber                  = 0x3A02,

		/// <summary>
		/// The MAPI property PR_CAR_TELEPHONE_NUMBER.
		/// </summary>
		CarTelephoneNumber                       = 0x3A1E,

		/// <summary>
		/// The MAPI property PR_CELLULAR_TELEPHONE_NUMBER.
		/// </summary>
		CellularTelephoneNumber                  = 0x3A1C,

		/// <summary>
		/// The MAPI property PR_CHILDRENS_NAMES.
		/// </summary>
		ChildrensNames                           = 0x3A58,

		/// <summary>
		/// The MAPI property PR_CLIENT_SUBMIT_TIME.
		/// </summary>
		ClientSubmitTime                         = 0x0039,

		/// <summary>
		/// The MAPI property PR_COMMENT.
		/// </summary>
		Comment                                  = 0x3004,

		/// <summary>
		/// The MAPI property PR_COMMON_VIEWS_ENTRYID.
		/// </summary>
		CommonViewsEntryId                       = 0x35E6,

		/// <summary>
		/// The MAPI property PR_COMPANY_MAIN_PHONE_NUMBER.
		/// </summary>
		CompanyMainPhoneNumber                   = 0x3A57,

		/// <summary>
		/// The MAPI property PR_COMPANY_NAME.
		/// </summary>
		CompanyName                              = 0x3A16,

		/// <summary>
		/// The MAPI property PR_COMPUTER_NETWORK_NAME.
		/// </summary>
		ComputerNetworkName                      = 0x3A49,

		/// <summary>
		/// The MAPI property PR_CONTACT_ADDRTYPES.
		/// </summary>
		ContactAddrtypes                         = 0x3A54,

		/// <summary>
		/// The MAPI property PR_CONTACT_DEFAULT_ADDRESS_INDEX.
		/// </summary>
		ContactDefaultAddressIndex               = 0x3A55,

		/// <summary>
		/// The MAPI property PR_CONTACT_EMAIL_ADDRESSES.
		/// </summary>
		ContactEmailAddresses                    = 0x3A56,

		/// <summary>
		/// The MAPI property PR_CONTACT_ENTRYIDS.
		/// </summary>
		ContactEntryIds                          = 0x3A53,

		/// <summary>
		/// The MAPI property PR_CONTACT_VERSION.
		/// </summary>
		ContactVersion                           = 0x3A52,

		/// <summary>
		/// The MAPI property PR_CONTAINER_CLASS.
		/// </summary>
		ContainerClass                           = 0x3613,

		/// <summary>
		/// The MAPI property PR_CONTAINER_CONTENTS.
		/// </summary>
		ContainerContents                        = 0x360F,

		/// <summary>
		/// The MAPI property PR_CONTAINER_FLAGS.
		/// </summary>
		ContainerFlags                           = 0x3600,

		/// <summary>
		/// The MAPI property PR_CONTAINER_HIERARCHY.
		/// </summary>
		ContainerHierarchy                       = 0x360E,

		/// <summary>
		/// The MAPI property PR_CONTAINER_MODIFY_VERSION.
		/// </summary>
		ContainerModifyVersion                   = 0x3614,

		/// <summary>
		/// The MAPI property PR_CONTENT_CONFIDENTIALITY_ALGORITHM_ID.
		/// </summary>
		ContentConfidentialityAlgorithmId        = 0x0006,

		/// <summary>
		/// The MAPI property PR_CONTENT_CORRELATOR.
		/// </summary>
		ContentCorrelator                        = 0x0007,

		/// <summary>
		/// The MAPI property PR_CONTENT_COUNT.
		/// </summary>
		ContentCount                             = 0x3602,

		/// <summary>
		/// The MAPI property PR_CONTENT_IDENTIFIER.
		/// </summary>
		ContentIdentifier                        = 0x0008,

		/// <summary>
		/// The MAPI property PR_CONTENT_INTEGRITY_CHECK.
		/// </summary>
		ContentIntegrityCheck                    = 0x0C00,

		/// <summary>
		/// The MAPI property PR_CONTENT_LENGTH.
		/// </summary>
		ContentLength                            = 0x0009,

		/// <summary>
		/// The MAPI property PR_CONTENT_RETURN_REQUESTED.
		/// </summary>
		ContentReturnRequested                   = 0x000A,

		/// <summary>
		/// The MAPI property PR_CONTENTS_SORT_ORDER.
		/// </summary>
		ContentsSortOrder                        = 0x360D,

		/// <summary>
		/// The MAPI property PR_CONTENT_UNREAD.
		/// </summary>
		ContentUnread                            = 0x3603,

		/// <summary>
		/// The MAPI property PR_CONTROL_FLAGS.
		/// </summary>
		ControlFlags                             = 0x3F00,

		/// <summary>
		/// The MAPI property PR_CONTROL_ID.
		/// </summary>
		ControlId                                = 0x3F07,

		/// <summary>
		/// The MAPI property PR_CONTROL_STRUCTURE.
		/// </summary>
		ControlStructure                         = 0x3F01,

		/// <summary>
		/// The MAPI property PR_CONTROL_TYPE.
		/// </summary>
		ControlType                              = 0x3F02,

		/// <summary>
		/// The MAPI property PR_CONVERSATION_INDEX.
		/// </summary>
		ConversationIndex                        = 0x0071,

		/// <summary>
		/// The MAPI property PR_CONVERSATION_KEY.
		/// </summary>
		ConversationKey                          = 0x000B,

		/// <summary>
		/// The MAPI property PR_CONVERSATION_TOPIC.
		/// </summary>
		ConversationTopic                        = 0x0070,

		/// <summary>
		/// The MAPI property PR_CONVERSATION_EITS.
		/// </summary>
		ConversionEits                           = 0x000C,

		/// <summary>
		/// The MAPI property PR_CONVERSION_PROHIBITED.
		/// </summary>
		ConversionProhibited                     = 0x3A03,

		/// <summary>
		/// The MAPI property PR_CONVERSION_WITH_LOSS_PROHIBITED.
		/// </summary>
		ConversionWithLossProhibited             = 0x000D,

		/// <summary>
		/// The MAPI property PR_CONVERTED_EITS.
		/// </summary>
		ConvertedEits                            = 0x000E,

		/// <summary>
		/// The MAPI property PR_CORRELATE.
		/// </summary>
		Correlate                                = 0x0E0C,

		/// <summary>
		/// The MAPI property PR_CORRELATE_MTSID.
		/// </summary>
		CorrelateMtsid                           = 0x0E0D,

		/// <summary>
		/// The MAPI property PR_COUNTRY.
		/// </summary>
		Country                                  = 0x3A26,

		/// <summary>
		/// The MAPI property PR_CREATE_TEMPLATES.
		/// </summary>
		CreateTemplates                          = 0x3604,

		/// <summary>
		/// The MAPI property PR_CREATION_TIME.
		/// </summary>
		CreationTime                             = 0x3007,

		/// <summary>
		/// The MAPI property PR_CREATION_VERSION.
		/// </summary>
		CreationVersion                          = 0x0E19,

		/// <summary>
		/// The MAPI property PR_CURRENT_VERSION.
		/// </summary>
		CurrentVersion                           = 0x0E00,

		/// <summary>
		/// The MAPI property PR_CUSTOMER_ID.
		/// </summary>
		CustomerId                               = 0x3A4A,

		/// <summary>
		/// The MAPI property PR_DEFAULT_PROFILE.
		/// </summary>
		DefaultProfile                           = 0x3D04,

		/// <summary>
		/// The MAPI property PR_DEFAULT_STORE.
		/// </summary>
		DefaultStore                             = 0x3400,

		/// <summary>
		/// The MAPI property PR_DEFAULT_VIEW_ENTRYID.
		/// </summary>
		DefaultViewEntryId                       = 0x3616,

		/// <summary>
		/// The MAPI property PR_DEF_CREATE_DL.
		/// </summary>
		DefCreateDl                              = 0x3611,

		/// <summary>
		/// The MAPI property PR_DEF_CREATE_MAILUSER.
		/// </summary>
		DefCreateMailuser                        = 0x3612,

		/// <summary>
		/// The MAPI property PR_DEFERRED_DELIVERY_TIME.
		/// </summary>
		DeferredDeliveryTime                     = 0x000F,

		/// <summary>
		/// The MAPI property PR_DELEGATION.
		/// </summary>
		Delegation                               = 0x007E,

		/// <summary>
		/// The MAPI property PR_DELETE_AFTER_SUBMIT.
		/// </summary>
		DeleteAfterSubmit                        = 0x0E01,

		/// <summary>
		/// The MAPI property PR_DELIVER_TIME.
		/// </summary>
		DeliverTime                              = 0x0010,

		/// <summary>
		/// The MAPI property PR_DELIVERY_POINT.
		/// </summary>
		DeliveryPoint                            = 0x0C07,

		/// <summary>
		/// The MAPI property PR_DELTAX.
		/// </summary>
		Deltax                                   = 0x3F03,

		/// <summary>
		/// The MAPI property PR_DELTAY.
		/// </summary>
		Deltay                                   = 0x3F04,

		/// <summary>
		/// The MAPI property PR_DEPARTMENT_NAME.
		/// </summary>
		DepartmentName                           = 0x3A18,

		/// <summary>
		/// The MAPI property PR_DEPTH.
		/// </summary>
		Depth                                    = 0x3005,

		/// <summary>
		/// The MAPI property PR_DETAILS_TABLE.
		/// </summary>
		DetailsTable                             = 0x3605,

		/// <summary>
		/// The MAPI property PR_DISCARD_REASON.
		/// </summary>
		DiscardReason                            = 0x0011,

		/// <summary>
		/// The MAPI property PR_DISCLOSE_RECIPIENTS.
		/// </summary>
		DiscloseRecipients                       = 0x3A04,

		/// <summary>
		/// The MAPI property PR_DISCLOSURE_OF_RECIPIENTS.
		/// </summary>
		DisclosureOfRecipients                   = 0x0012,

		/// <summary>
		/// The MAPI property PR_DISCRETE_VALUES.
		/// </summary>
		DiscreteValues                           = 0x0E0E,

		/// <summary>
		/// The MAPI property PR_DISC_VAL.
		/// </summary>
		DiscVal                                  = 0x004A,

		/// <summary>
		/// The MAPI property PR_DISPLAY_BCC.
		/// </summary>
		DisplayBcc                               = 0x0E02,

		/// <summary>
		/// The MAPI property PR_DISPLAY_CC.
		/// </summary>
		DisplayCc                                = 0x0E03,

		/// <summary>
		/// The MAPI property PR_DISPLAY_NAME.
		/// </summary>
		DisplayName                              = 0x3001,

		/// <summary>
		/// The MAPI property PR_DISPLAY_NAME_PREFIX.
		/// </summary>
		DisplayNamePrefix                        = 0x3A45,

		/// <summary>
		/// The MAPI property PR_DISPLAY_TO.
		/// </summary>
		DisplayTo                                = 0x0E04,

		/// <summary>
		/// The MAPI property PR_DISPLAY_TYPE.
		/// </summary>
		DisplayType                              = 0x3900,

		/// <summary>
		/// The MAPI property PR_DL_EXPANSION_HISTORY.
		/// </summary>
		DlExpansionHistory                       = 0x0013,

		/// <summary>
		/// The MAPI property PR_DL_EXPANSION_PROHIBITED.
		/// </summary>
		DlExpansionProhibited                    = 0x0014,

		/// <summary>
		/// The MAPI property PR_EMAIL_ADDRESS.
		/// </summary>
		EmailAddress                             = 0x3003,

		/// <summary>
		/// The MAPI property PR_END_DATE.
		/// </summary>
		EndDate                                  = 0x0061,

		/// <summary>
		/// The MAPI property PR_ENTRYID.
		/// </summary>
		EntryId                                  = 0x0FFF,

		/// <summary>
		/// The MAPI property PR_EXPAND_BEGIN_TIME.
		/// </summary>
		ExpandBeginTime                          = 0x3618,

		/// <summary>
		/// The MAPI property PR_EXPANDED_BEGIN_TIME.
		/// </summary>
		ExpandedBeginTime                        = 0x361A,

		/// <summary>
		/// The MAPI property PR_EXPANDED_END_TIME.
		/// </summary>
		ExpandedEndTime                          = 0x361B,

		/// <summary>
		/// The MAPI property PR_EXPAND_END_TIME.
		/// </summary>
		ExpandEndTime                            = 0x3619,

		/// <summary>
		/// The MAPI property PR_EXPIRY_TIME.
		/// </summary>
		ExpiryTime                               = 0x0015,

		/// <summary>
		/// The MAPI property PR_EXPLICIT_CONVERSION.
		/// </summary>
		ExplicitConversion                       = 0x0C01,

		/// <summary>
		/// The MAPI property PR_FILTERING_HOOKS.
		/// </summary>
		FilteringHooks                           = 0x3D08,

		/// <summary>
		/// The MAPI property PR_FINDER_ENTRYID.
		/// </summary>
		FinderEntryId                            = 0x35E7,

		/// <summary>
		/// The MAPI property PR_FOLDER_ASSOCIATED_CONTENTS.
		/// </summary>
		FolderAssociatedContents                 = 0x3610,

		/// <summary>
		/// The MAPI property PR_FOLDER_TYPE.
		/// </summary>
		FolderType                               = 0x3601,

		/// <summary>
		/// The MAPI property PR_FORM_CATEGORY.
		/// </summary>
		FormCategory                             = 0x3304,

		/// <summary>
		/// The MAPI property PR_FORM_CATEGORY_SUB.
		/// </summary>
		FormCategorySub                          = 0x3305,

		/// <summary>
		/// The MAPI property PR_FORM_CLSID.
		/// </summary>
		FormClsid                                = 0x3302,

		/// <summary>
		/// The MAPI property PR_FORM_CONTACT_NAME.
		/// </summary>
		FormContactName                          = 0x3303,

		/// <summary>
		/// The MAPI property PR_FORM_DESIGNER_GUID.
		/// </summary>
		FormDesignerGuid                         = 0x3309,

		/// <summary>
		/// The MAPI property PR_FORM_DESIGNER_NAME.
		/// </summary>
		FormDesignerName                         = 0x3308,

		/// <summary>
		/// The MAPI property PR_FORM_HIDDEN.
		/// </summary>
		FormHidden                               = 0x3307,

		/// <summary>
		/// The MAPI property PR_FORM_HOST_MAP.
		/// </summary>
		FormHostMap                              = 0x3306,

		/// <summary>
		/// The MAPI property PR_FORM_MESSAGE_BEHAVIOR.
		/// </summary>
		FormMessageBehavior                      = 0x330A,

		/// <summary>
		/// The MAPI property PR_FORM_VERSION.
		/// </summary>
		FormVersion                              = 0x3301,

		/// <summary>
		/// The MAPI property PR_FTP_SITE.
		/// </summary>
		FtpSite                                  = 0x3A4C,

		/// <summary>
		/// The MAPI property PR_GENDER.
		/// </summary>
		Gender                                   = 0x3A4D,

		/// <summary>
		/// The MAPI property PR_GENERATION.
		/// </summary>
		Generation                               = 0x3A05,

		/// <summary>
		/// The MAPI property PR_GIVEN_NAME.
		/// </summary>
		GivenName                                = 0x3A06,

		/// <summary>
		/// The MAPI property PR_GOVERNMENT_ID_NUMBER.
		/// </summary>
		GovernmentIdNumber                       = 0x3A07,

		/// <summary>
		/// The MAPI property PR_HASTTACH.
		/// </summary>
		Hasattach                                = 0x0E1B,

		/// <summary>
		/// The MAPI property PR_HEADER_FOLDER_ENTRYID.
		/// </summary>
		HeaderFolderEntryId                      = 0x3E0A,

		/// <summary>
		/// The MAPI property PR_HOBBIES.
		/// </summary>
		Hobbies                                  = 0x3A43,

		/// <summary>
		/// The MAPI property PR_HOME2_TELEPHONE_NUMBER.
		/// </summary>
		Home2TelephoneNumber                     = 0x3A2F,

		/// <summary>
		/// The MAPI property PR_HOME_ADDRESS_CITY.
		/// </summary>
		HomeAddressCity                          = 0x3A59,

		/// <summary>
		/// The MAPI property PR_HOME_ADDRESS_COUNTRY.
		/// </summary>
		HomeAddressCountry                       = 0x3A5A,

		/// <summary>
		/// The MAPI property PR_HOME_ADDRESS_POSTAL_CODE.
		/// </summary>
		HomeAddressPostalCode                    = 0x3A5B,

		/// <summary>
		/// The MAPI property PR_HOME_ADDRESS_POST_OFFICE_BOX.
		/// </summary>
		HomeAddressPostOfficeBox                 = 0x3A5E,

		/// <summary>
		/// The MAPI property PR_HOME_ADDRESS_STATE_OR_PROVINCE.
		/// </summary>
		HomeAddressStateOrProvince               = 0x3A5C,

		/// <summary>
		/// The MAPI property PR_HOME_ADDRESS_STREET.
		/// </summary>
		HomeAddressStreet                        = 0x3A5D,

		/// <summary>
		/// The MAPI property PR_HOME_FAX_NUMBER.
		/// </summary>
		HomeFaxNumber                            = 0x3A25,

		/// <summary>
		/// The MAPI property PR_HOME_TELEPHONE_NUMBER.
		/// </summary>
		HomeTelephoneNumber                      = 0x3A09,

		/// <summary>
		/// The MAPI property PR_ICON.
		/// </summary>
		Icon                                     = 0x0FFD,

		/// <summary>
		/// The MAPI property PR_IDENTITY_DISPLAY.
		/// </summary>
		IdentityDisplay                          = 0x3E00,

		/// <summary>
		/// The MAPI property PR_IDENTITY_ENTRYID.
		/// </summary>
		IdentityEntryId                          = 0x3E01,

		/// <summary>
		/// The MAPI property PR_IDENTITY_SEARCH_KEY.
		/// </summary>
		IdentitySearchKey                        = 0x3E05,

		/// <summary>
		/// The MAPI property PR_IMPLICIT_CONVERSION_PROHIBITED.
		/// </summary>
		ImplicitConversionProhibited             = 0x0016,

		/// <summary>
		/// The MAPI property PR_IMPORTANCE.
		/// </summary>
		Importance                               = 0x0017,

		/// <summary>
		/// The MAPI property PR_INCOMPLETE_COPY.
		/// </summary>
		IncompleteCopy                           = 0x0035,

		/// <summary>
		/// The Internet mail override charset.
		/// </summary>
		INetMailOverrideCharset                  = 0x5903,

		/// <summary>
		/// The Internet mail override format.
		/// </summary>
		INetMailOverrideFormat                   = 0x5902,

		/// <summary>
		/// The MAPI property PR_INITIAL_DETAILS_PANE.
		/// </summary>
		InitialDetailsPane                       = 0x3F08,

		/// <summary>
		/// The MAPI property PR_INITIALS.
		/// </summary>
		Initials                                 = 0x3A0A,

		/// <summary>
		/// The MAPI property PR_IN_REPLY_TO_ID.
		/// </summary>
		InReplyToId                              = 0x1042,

		/// <summary>
		/// The MAPI property PR_INSTANCE_KEY.
		/// </summary>
		InstanceKey                              = 0x0FF6,

		/// <summary>
		/// The MAPI property PR_INTERNET_APPROVED.
		/// </summary>
		InternetApproved                         = 0x1030,

		/// <summary>
		/// The MAPI property PR_INTERNET_ARTICLE_NUMBER.
		/// </summary>
		InternetArticleNumber                    = 0x0E23,

		/// <summary>
		/// The MAPI property PR_INTERNET_CONTROL.
		/// </summary>
		InternetControl                          = 0x1031,

		/// <summary>
		/// The MAPI property PR_INTERNET_CPID.
		/// </summary>
		InternetCPID                             = 0x3FDE,

		/// <summary>
		/// The MAPI property PR_INTERNET_DISTRIBUTION.
		/// </summary>
		InternetDistribution                     = 0x1032,

		/// <summary>
		/// The MAPI property PR_INTERNET_FOLLOWUP_TO.
		/// </summary>
		InternetFollowupTo                       = 0x1033,

		/// <summary>
		/// The MAPI property PR_INTERNET_LINES.
		/// </summary>
		InternetLines                            = 0x1034,

		/// <summary>
		/// The MAPI property PR_INTERNET_MESSAGE_ID.
		/// </summary>
		InternetMessageId                        = 0x1035,

		/// <summary>
		/// The MAPI property PR_INTERNET_NEWSGROUPS.
		/// </summary>
		InternetNewsgroups                       = 0x1036,

		/// <summary>
		/// The MAPI property PR_INTERNET_NNTP_PATH.
		/// </summary>
		InternetNntpPath                         = 0x1038,

		/// <summary>
		/// The MAPI property PR_INTERNET_ORGANIZATION.
		/// </summary>
		InternetOrganization                     = 0x1037,

		/// <summary>
		/// The MAPI property PR_INTERNET_PRECEDENCE.
		/// </summary>
		InternetPrecedence                       = 0x1041,

		/// <summary>
		/// The MAPI property PR_INTERNET_REFERENCES.
		/// </summary>
		InternetReferences                       = 0x1039,

		/// <summary>
		/// The MAPI property PR_IPM_ID.
		/// </summary>
		IpmId                                    = 0x0018,

		/// <summary>
		/// The MAPI property PR_IPM_OUTBOX_ENTRYID.
		/// </summary>
		IpmOutboxEntryId                         = 0x35E2,

		/// <summary>
		/// The MAPI property PR_IPM_OUTBOX_SEARCH_KEY.
		/// </summary>
		IpmOutboxSearchKey                       = 0x3411,

		/// <summary>
		/// The MAPI property PR_IPM_RETURN_REQUESTED.
		/// </summary>
		IpmReturnRequested                       = 0x0C02,

		/// <summary>
		/// The MAPI property PR_IPM_SENTMAIL_ENTRYID.
		/// </summary>
		IpmSentmailEntryId                       = 0x35E4,

		/// <summary>
		/// The MAPI property PR_IPM_SENTMAIL_SEARCH_KEY.
		/// </summary>
		IpmSentmailSearchKey                     = 0x3413,

		/// <summary>
		/// The MAPI property PR_IPM_SUBTREE_ENTRYID.
		/// </summary>
		IpmSubtreeEntryId                        = 0x35E0,

		/// <summary>
		/// The MAPI property PR_IPM_SUBTREE_SEARCH_KEY.
		/// </summary>
		IpmSubtreeSearchKey                      = 0x3410,

		/// <summary>
		/// The MAPI property PR_IPM_WASTEBASKET_ENTRYID.
		/// </summary>
		IpmWastebasketEntryId                    = 0x35E3,

		/// <summary>
		/// The MAPI property PR_IPM_WASTEBASKET_SEARCH_KEY.
		/// </summary>
		IpmWastebasketSearchKey                  = 0x3412,

		/// <summary>
		/// The MAPI property PR_ISDN_NUMBER.
		/// </summary>
		IsdnNumber                               = 0x3A2D,

		/// <summary>
		/// The MAPI property PR_KEYWORD.
		/// </summary>
		Keyword                                  = 0x3A0B,

		/// <summary>
		/// The MAPI property PR_LANGUAGE.
		/// </summary>
		Language                                 = 0x3A0C,

		/// <summary>
		/// The MAPI property PR_LANGUAGES.
		/// </summary>
		Languages                                = 0x002F,

		/// <summary>
		/// The MAPI property PR_LAST_MODIFICATION_TIME.
		/// </summary>
		LastModificationTime                     = 0x3008,

		/// <summary>
		/// The MAPI property PR_LATEST_DELIVERY_TIME.
		/// </summary>
		LatestDeliveryTime                       = 0x0019,

		/// <summary>
		/// The MAPI property PR_LIST_HELP.
		/// </summary>
		ListHelp                                 = 0x1043,

		/// <summary>
		/// The MAPI property PR_LIST_SUBSCRIBE.
		/// </summary>
		ListSubscribe                            = 0x1044,

		/// <summary>
		/// The MAPI property PR_LIST_UNSUBSCRIBE.
		/// </summary>
		ListUnsubscribe                          = 0x1045,

		/// <summary>
		/// The MAPI property PR_LOCALITY.
		/// </summary>
		Locality                                 = 0x3A27,

		/// <summary>
		/// The MAPI property PR_LOCALLY_DELIVERED.
		/// </summary>
		LocallyDelivered                         = 0x6745,

		/// <summary>
		/// The MAPI property PR_LOCATION.
		/// </summary>
		Location                                 = 0x3A0D,

		/// <summary>
		/// The MAPI property PR_LOCK_BRANCH_ID.
		/// </summary>
		LockBranchId                             = 0x3800,

		/// <summary>
		/// The MAPI property PR_LOCK_DEPTH.
		/// </summary>
		LockDepth                                = 0x3808,

		/// <summary>
		/// The MAPI property PR_LOCK_ENLISTMENT_CONTEXT.
		/// </summary>
		LockEnlistmentContext                    = 0x3804,

		/// <summary>
		/// The MAPI property PR_LOCK_EXPIRY_TIME.
		/// </summary>
		LockExpiryTime                           = 0x380A,

		/// <summary>
		/// The MAPI property PR_LOCK_PERSISTENT.
		/// </summary>
		LockPersistent                           = 0x3807,

		/// <summary>
		/// The MAPI property PR_LOCK_RESOURCE_DID.
		/// </summary>
		LockResourceDid                          = 0x3802,

		/// <summary>
		/// The MAPI property PR_LOCK_RESOURCE_FID.
		/// </summary>
		LockResourceFid                          = 0x3801,

		/// <summary>
		/// The MAPI property PR_LOCK_RESOURCE_MID.
		/// </summary>
		LockResourceMid                          = 0x3803,

		/// <summary>
		/// The MAPI property PR_LOCK_SCOPE.
		/// </summary>
		LockScope                                = 0x3806,

		/// <summary>
		/// The MAPI property PR_LOCK_TIMEOUT.
		/// </summary>
		LockTimeout                              = 0x3809,

		/// <summary>
		/// The MAPI property PR_LOCK_TYPE.
		/// </summary>
		LockType                                 = 0x3805,

		/// <summary>
		/// The MAPI property PR_MAIL_PERMISSION.
		/// </summary>
		MailPermission                           = 0x3A0E,

		/// <summary>
		/// The MAPI property PR_MANAGER_NAME.
		/// </summary>
		ManagerName                              = 0x3A4E,

		/// <summary>
		/// The MAPI property PR_MAPPING_SIGNATURE.
		/// </summary>
		MappingSignature                         = 0x0FF8,

		/// <summary>
		/// The MAPI property PR_MDB_PROVIDER.
		/// </summary>
		MdbProvider                              = 0x3414,

		/// <summary>
		/// The MAPI property PR_MESSAGE_ATTACHMENTS.
		/// </summary>
		MessageAttachments                       = 0x0E13,

		/// <summary>
		/// The MAPI property PR_MESSAGE_CC_ME.
		/// </summary>
		MessageCcMe                              = 0x0058,

		/// <summary>
		/// The MAPI property PR_MESSAGE_CLASS.
		/// </summary>
		MessageClass                             = 0x001A,

		/// <summary>
		/// The MAPI property PR_MESSAGE_CODEPAGE.
		/// </summary>
		MessageCodepage                          = 0x3FFD,

		/// <summary>
		/// The MAPI property PR_MESSAGE_DELIVERY_ID.
		/// </summary>
		MessageDeliveryId                        = 0x001B,

		/// <summary>
		/// The MAPI property PR_MESSAGE_DELIVERY_TIME.
		/// </summary>
		MessageDeliveryTime                      = 0x0E06,

		/// <summary>
		/// The MAPI property PR_MESSAGE_DOWNLOAD_TIME.
		/// </summary>
		MessageDownloadTime                      = 0x0E18,

		/// <summary>
		/// The MAPI property PR_MESSAGE_FLAGS.
		/// </summary>
		MessageFlags                             = 0x0E07,

		/// <summary>
		/// The MAPI property PR_MESSAGE_RECIPIENTS.
		/// </summary>
		MessageRecipients                        = 0x0E12,

		/// <summary>
		/// The MAPI property PR_MESSAGE_RECIP_ME.
		/// </summary>
		MessageRecipMe                           = 0x0059,

		/// <summary>
		/// The MAPI property PR_MESSAGE_SECURITY_LABEL.
		/// </summary>
		MessageSecurityLabel                     = 0x001E,

		/// <summary>
		/// The MAPI property PR_MESSAGE_SIZE.
		/// </summary>
		MessageSize                              = 0x0E08,

		/// <summary>
		/// The MAPI property PR_MESSAGE_SUBMISSION_ID.
		/// </summary>
		MessageSubmissionId                      = 0x0047,

		/// <summary>
		/// The MAPI property PR_MESSAGE_TOKEN.
		/// </summary>
		MessageToken                             = 0x0C03,

		/// <summary>
		/// The MAPI property PR_MESSAGE_TO_ME.
		/// </summary>
		MessageToMe                              = 0x0057,

		/// <summary>
		/// The MAPI property PR_MHS_COMMON_NAME.
		/// </summary>
		MhsCommonName                            = 0x3A0F,

		/// <summary>
		/// The MAPI property PR_MIDDLE_NAME.
		/// </summary>
		MiddleName                               = 0x3A44,

		/// <summary>
		/// The MAPI property PR_MINI_ICON.
		/// </summary>
		MiniIcon                                 = 0x0FFC,

		/// <summary>
		/// The MAPI property PR_MOBILE_TELEPHONE_NUMBER.
		/// </summary>
		MobileTelephoneNumber                    = 0x3A1C,

		/// <summary>
		/// The MAPI property PR_MODIFY_VERSION.
		/// </summary>
		ModifyVersion                            = 0x0E1A,

		/// <summary>
		/// The MAPI property PR_MSG_STATUS.
		/// </summary>
		MsgStatus                                = 0x0E17,

		/// <summary>
		/// The MAPI property PR_NDR_DIAG_CODE.
		/// </summary>
		NdrDiagCode                              = 0x0C05,

		/// <summary>
		/// The MAPI property PR_NDR_REASON_CODE.
		/// </summary>
		NdrReasonCode                            = 0x0C04,

		/// <summary>
		/// The MAPI property PR_NDR_STATUS_CODE.
		/// </summary>
		NdrStatusCode                            = 0x0C20,

		/// <summary>
		/// The MAPI property PR_NEWSGROUP_NAME.
		/// </summary>
		NewsgroupName                            = 0x0E24,

		/// <summary>
		/// The MAPI property PR_NICKNAME.
		/// </summary>
		Nickname                                 = 0x3A4F,

		/// <summary>
		/// The MAPI property PR_NNTP_XREF.
		/// </summary>
		NntpXref                                 = 0x1040,

		/// <summary>
		/// The MAPI property PR_NON_RECEIPT_NOTIFICATION_REQUESTED.
		/// </summary>
		NonReceiptNotificationRequested          = 0x0C06,

		/// <summary>
		/// The MAPI property PR_NON_RECEIPT_REASON.
		/// </summary>
		NonReceiptReason                         = 0x003E,

		/// <summary>
		/// The MAPI property PR_NORMALIZED_SUBJECT.
		/// </summary>
		NormalizedSubject                        = 0x0E1D,

		/// <summary>
		/// The MAPI property PR_NT_SECURITY_DESCRIPTOR.
		/// </summary>
		NtSecurityDescriptor                     = 0x0E27,

		/// <summary>
		/// The MAPI property PR_NULL.
		/// </summary>
		Null                                     = 0x0000,

		/// <summary>
		/// The MAPI property PR_OBJECT_TYPE.
		/// </summary>
		ObjectType                               = 0x0FFE,

		/// <summary>
		/// The MAPI property PR_OBSOLETE_IPMS.
		/// </summary>
		ObsoletedIpms                            = 0x001F,

		/// <summary>
		/// The MAPI property PR_OFFICE2_TELEPHONE_NUMBER.
		/// </summary>
		Office2TelephoneNumber                   = 0x3A1B,

		/// <summary>
		/// The MAPI property PR_OFFICE_LOCATION.
		/// </summary>
		OfficeLocation                           = 0x3A19,

		/// <summary>
		/// The MAPI property PR_OFFICE_TELEPHONE_NUMBER.
		/// </summary>
		OfficeTelephoneNumber                    = 0x3A08,

		/// <summary>
		/// The MAPI property PR_OOF_REPLY_TIME.
		/// </summary>
		OofReplyType                             = 0x4080,

		/// <summary>
		/// The MAPI property PR_ORGANIZATIONAL_ID_NUMBER.
		/// </summary>
		OrganizationalIdNumber                   = 0x3A10,

		/// <summary>
		/// The MAPI property PR_ORIG_ENTRYID.
		/// </summary>
		OrigEntryId                              = 0x300F,

		/// <summary>
		/// The MAPI property PR_ORIGINAL_AUTHOR_ADDRTYPE.
		/// </summary>
		OriginalAuthorAddrtype                   = 0x0079,

		/// <summary>
		/// The MAPI property PR_ORIGINAL_AUTHOR_EMAIL_ADDRESS.
		/// </summary>
		OriginalAuthorEmailAddress               = 0x007A,

		/// <summary>
		/// The MAPI property PR_ORIGINAL_AUTHOR_ENTRYID.
		/// </summary>
		OriginalAuthorEntryId                    = 0x004C,

		/// <summary>
		/// The MAPI property PR_ORIGINAL_AUTHOR_NAME.
		/// </summary>
		OriginalAuthorName                       = 0x004D,

		/// <summary>
		/// The MAPI property PR_ORIGINAL_AUTHOR_SEARCH_KEY.
		/// </summary>
		OriginalAuthorSearchKey                  = 0x0056,

		/// <summary>
		/// The MAPI property PR_ORIGINAL_DELIVERY_TIME.
		/// </summary>
		OriginalDeliveryTime                     = 0x0055,

		/// <summary>
		/// The MAPI property PR_ORIGINAL_DISPLAY_BCC.
		/// </summary>
		OriginalDisplayBcc                       = 0x0072,

		/// <summary>
		/// The MAPI property PR_ORIGINAL_DISPLAY_CC.
		/// </summary>
		OriginalDisplayCc                        = 0x0073,

		/// <summary>
		/// The MAPI property PR_ORIGINAL_DISPLAY_NAME.
		/// </summary>
		OriginalDisplayName                      = 0x3A13,

		/// <summary>
		/// The MAPI property PR_ORIGINAL_DISPLAY_TO.
		/// </summary>
		OriginalDisplayTo                        = 0x0074,

		/// <summary>
		/// The MAPI property PR_ORIGINAL_EITS.
		/// </summary>
		OriginalEits                             = 0x0021,

		/// <summary>
		/// The MAPI property PR_ORIGINAL_ENTRYID.
		/// </summary>
		OriginalEntryId                          = 0x3A12,

		/// <summary>
		/// The MAPI property PR_ORIGINALLY_INTENDED_RECIP_ADRTYPE.
		/// </summary>
		OriginallyIntendedRecipAddrtype          = 0x007B,

		/// <summary>
		/// The MAPI property PR_ORIGINALLY_INTENDED_RECIP_EMAIL_ADDRESS.
		/// </summary>
		OriginallyIntendedRecipEmailAddress      = 0x007C,

		/// <summary>
		/// The MAPI property PR_ORIGINALLY_INTENDED_RECIP_ENTRYID.
		/// </summary>
		OriginallyIntendedRecipEntryId           = 0x1012,

		/// <summary>
		/// The MAPI property PR_ORIGINALLY_INTENDED_RECIPIENT_NAME.
		/// </summary>
		OriginallyIntendedRecipientName          = 0x0020,

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SEARCH_KEY.
		/// </summary>
		OriginalSearchKey                        = 0x3A14,

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENDER_ADDRTYPE.
		/// </summary>
		OriginalSenderAddrtype                   = 0x0066,

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENDER_EMAIL_ADDRESS.
		/// </summary>
		OriginalSenderEmailAddress               = 0x0067,

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENDER_ENTRYID.
		/// </summary>
		OriginalSenderEntryId                    = 0x005B,

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENDER_NAME.
		/// </summary>
		OriginalSenderName                       = 0x005A,

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENDER_SEARCH_KEY.
		/// </summary>
		OriginalSenderSearchKey                  = 0x005C,

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENSITIVITY.
		/// </summary>
		OriginalSensitivity                      = 0x002E,

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENT_REPRESENTING_ADDRTYPE.
		/// </summary>
		OriginalSentRepresentingAddrtype         = 0x0068,

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENT_REPRESENTING_EMAIL_ADDRESS.
		/// </summary>
		OriginalSentRepresentingEmailAddress     = 0x0069,

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENT_REPRESENTING_ENTRYID.
		/// </summary>
		OriginalSentRepresentingEntryId          = 0x005E,

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENT_REPRESENTING_NAME.
		/// </summary>
		OriginalSentRepresentingName             = 0x005D,

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENT_REPRESENTING_SEARCH_KEY.
		/// </summary>
		OriginalSentRepresentingSearchKey        = 0x005F,

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SUBJECT.
		/// </summary>
		OriginalSubject                          = 0x0049,

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SUBMIT_TIME.
		/// </summary>
		OriginalSubmitTime                       = 0x004E,

		/// <summary>
		/// The MAPI property PR_ORIGINATING_MTA_CERTIFICATE.
		/// </summary>
		OriginatingMtaCertificate                = 0x0E25,

		/// <summary>
		/// The MAPI property PR_ORIGINATOR_AND_DL_EXPANSION_HISTORY.
		/// </summary>
		OriginatorAndDlExpansionHistory          = 0x1002,

		/// <summary>
		/// The MAPI property PR_ORIGINATOR_CERTIFICATE.
		/// </summary>
		OriginatorCertificate                    = 0x0022,

		/// <summary>
		/// The MAPI property PR_ORIGINATOR_DELIVERY_REPORT_REQUESTED.
		/// </summary>
		OriginatorDeliveryReportRequested        = 0x0023,

		/// <summary>
		/// The MAPI property PR_ORIGINATOR_NON_DELIVERY_REPORT_REQUESTED.
		/// </summary>
		OriginatorNonDeliveryReportRequested     = 0x0C08,

		/// <summary>
		/// The MAPI property PR_ORIGINATOR_REQUESTED_ALTERNATE_RECIPIENT.
		/// </summary>
		OriginatorRequestedAlternateRecipient    = 0x0C09,

		/// <summary>
		/// The MAPI property PR_ORIGINATOR_RETURN_ADDRESS.
		/// </summary>
		OriginatorReturnAddress                  = 0x0024,

		/// <summary>
		/// The MAPI property PR_ORIGIN_CHECK.
		/// </summary>
		OriginCheck                              = 0x0027,

		/// <summary>
		/// The MAPI property PR_ORIG_MESSAGE_CLASS.
		/// </summary>
		OrigMessageClass                         = 0x004B,

		/// <summary>
		/// The MAPI property PR_OTHER_ADDRESS_CITY.
		/// </summary>
		OtherAddressCity                         = 0x3A5F,

		/// <summary>
		/// The MAPI property PR_OTHER_ADDRESS_COUNTRY.
		/// </summary>
		OtherAddressCountry                      = 0x3A60,

		/// <summary>
		/// The MAPI property PR_OTHER_ADDRESS_POSTAL_CODE.
		/// </summary>
		OtherAddressPostalCode                   = 0x3A61,

		/// <summary>
		/// The MAPI property PR_OTHER_ADDRESS_POST_OFFICE_BOX.
		/// </summary>
		OtherAddressPostOfficeBox                = 0x3A64,

		/// <summary>
		/// The MAPI property PR_OTHER_ADDRESS_STATE_OR_PROVINCE.
		/// </summary>
		OtherAddressStateOrProvince              = 0x3A62,

		/// <summary>
		/// The MAPI property PR_OTHER_ADDRESS_STREET.
		/// </summary>
		OtherAddressStreet                       = 0x3A63,

		/// <summary>
		/// The MAPI property PR_OTHER_TELEPHONE_NUMBER.
		/// </summary>
		OtherTelephoneNumber                     = 0x3A1F,

		/// <summary>
		/// The MAPI property PR_OWNER_APPT_ID.
		/// </summary>
		OwnerApptId                              = 0x0062,

		/// <summary>
		/// The MAPI property PR_OWN_STORE_ENTRYID.
		/// </summary>
		OwnStoreEntryId                          = 0x3E06,

		/// <summary>
		/// The MAPI property PR_PAGER_TELEPHONE_NUMBER.
		/// </summary>
		PagerTelephoneNumber                     = 0x3A21,

		/// <summary>
		/// The MAPI property PR_PARENT_DISPLAY.
		/// </summary>
		ParentDisplay                            = 0x0E05,

		/// <summary>
		/// The MAPI property PR_PARENT_ENTRYID.
		/// </summary>
		ParentEntryId                            = 0x0E09,

		/// <summary>
		/// The MAPI property PR_PARENT_KEY.
		/// </summary>
		ParentKey                                = 0x0025,

		/// <summary>
		/// The MAPI property PR_PERSONAL_HOME_PAGE.
		/// </summary>
		PersonalHomePage                         = 0x3A50,

		/// <summary>
		/// The MAPI property PR_PHYSICAL_DELIVERY_BUREAU_FAX_DELIVERY.
		/// </summary>
		PhysicalDeliveryBureauFaxDelivery        = 0x0C0A,

		/// <summary>
		/// The MAPI property PR_PHYSICAL_DELIVERY_MODE.
		/// </summary>
		PhysicalDeliveryMode                     = 0x0C0B,

		/// <summary>
		/// The MAPI property PR_PHYSICAL_DELIVERY_REPORT_REQUEST.
		/// </summary>
		PhysicalDeliveryReportRequest            = 0x0C0C,

		/// <summary>
		/// The MAPI property PR_PHYSICAL_FORWARDING_ADDRESS.
		/// </summary>
		PhysicalForwardingAddress                = 0x0C0D,

		/// <summary>
		/// The MAPI property PR_PHYSICAL_FORWARDING_ADDRESS_REQUESTED.
		/// </summary>
		PhysicalForwardingAddressRequested       = 0x0C0E,

		/// <summary>
		/// The MAPI property PR_PHYSICAL_FORWARDING_PROHIBITED.
		/// </summary>
		PhysicalForwardingProhibited             = 0x0C0F,

		/// <summary>
		/// The MAPI property PR_PHYSICAL_RENDITION_ATTRIBUTES.
		/// </summary>
		PhysicalRenditionAttributes              = 0x0C10,

		/// <summary>
		/// The MAPI property PR_POSTAL_ADDRESS.
		/// </summary>
		PostalAddress                            = 0x3A15,

		/// <summary>
		/// The MAPI property PR_POSTAL_CODE.
		/// </summary>
		PostalCode                               = 0x3A2A,

		/// <summary>
		/// The MAPI property PR_POST_FOLDER_ENTRIES.
		/// </summary>
		PostFolderEntries                        = 0x103B,

		/// <summary>
		/// The MAPI property PR_POST_FOLDER_NAMES.
		/// </summary>
		PostFolderNames                          = 0x103C,

		/// <summary>
		/// The MAPI property PR_POST_OFFICE_BOX.
		/// </summary>
		PostOfficeBox                            = 0x3A2B,

		/// <summary>
		/// The MAPI property PR_POST_REPLY_DENIED.
		/// </summary>
		PostReplyDenied                          = 0x103F,

		/// <summary>
		/// The MAPI property PR_POST_REPLY_FOLDER_ENTRIES.
		/// </summary>
		PostReplyFolderEntries                   = 0x103D,

		/// <summary>
		/// The MAPI property PR_POST_REPLY_FOLDER_NAMES.
		/// </summary>
		PostReplyFolderNames                     = 0x103E,

		/// <summary>
		/// The MAPI property PR_PREFERRED_BY_NAME.
		/// </summary>
		PreferredByName                          = 0x3A47,

		/// <summary>
		/// The MAPI property PR_PREPROCESS.
		/// </summary>
		Preprocess                               = 0x0E22,

		/// <summary>
		/// The MAPI property PR_PRIMARY_CAPABILITY.
		/// </summary>
		PrimaryCapability                        = 0x3904,

		/// <summary>
		/// The MAPI property PR_PRIMARY_FAX_NUMBER.
		/// </summary>
		PrimaryFaxNumber                         = 0x3A23,

		/// <summary>
		/// The MAPI property PR_PRIMARY_TELEPHONE_NUMBER.
		/// </summary>
		PrimaryTelephoneNumber                   = 0x3A1A,

		/// <summary>
		/// The MAPI property PR_PRIORITY.
		/// </summary>
		Priority                                 = 0x0026,

		/// <summary>
		/// The MAPI property PR_PROFESSION.
		/// </summary>
		Profession                               = 0x3A46,

		/// <summary>
		/// The MAPI property PR_PROFILE_NAME.
		/// </summary>
		ProfileName                              = 0x3D12,

		/// <summary>
		/// The MAPI property PR_PROOF_OF_DELIVERY.
		/// </summary>
		ProofOfDelivery                          = 0x0C11,

		/// <summary>
		/// The MAPI property PR_PROOF_OF_DELIVERY_REQUESTED.
		/// </summary>
		ProofOfDeliveryRequested                 = 0x0C12,

		/// <summary>
		/// The MAPI property PR_PROOF_OF_SUBMISSION.
		/// </summary>
		ProofOfSubmission                        = 0x0E26,

		/// <summary>
		/// The MAPI property PR_PROOF_OF_SUBMISSION_REQUESTED.
		/// </summary>
		ProofOfSubmissionRequested               = 0x0028,

		/// <summary>
		/// The MAPI property PR_PROP_ID_SECURE_MAX.
		/// </summary>
		PropIdSecureMax                          = 0x67FF,

		/// <summary>
		/// The MAPI property PR_PROP_ID_SECURE_MIN.
		/// </summary>
		PropIdSecureMin                          = 0x67F0,

		/// <summary>
		/// The MAPI property PR_PROVIDER_DISPLAY.
		/// </summary>
		ProviderDisplay                          = 0x3006,

		/// <summary>
		/// The MAPI property PR_PROVIDER_DLL_NAME.
		/// </summary>
		ProviderDllName                          = 0x300A,

		/// <summary>
		/// The MAPI property PR_PROVIDER_ORDINAL.
		/// </summary>
		ProviderOrdinal                          = 0x300D,

		/// <summary>
		/// The MAPI property PR_PROVIDER_SUBMIT_TIME.
		/// </summary>
		ProviderSubmitTime                       = 0x0048,

		/// <summary>
		/// The MAPI property PR_PROVIDER_UID.
		/// </summary>
		ProviderUid                              = 0x300C,

		/// <summary>
		/// The MAPI property PR_PUID.
		/// </summary>
		Puid                                     = 0x300E,

		/// <summary>
		/// The MAPI property PR_RADIO_TELEPHONE_NUMBER.
		/// </summary>
		RadioTelephoneNumber                     = 0x3A1D,

		/// <summary>
		/// The MAPI property PR_RCVD_REPRESENTING_ADDRTYPE.
		/// </summary>
		RcvdRepresentingAddrtype                 = 0x0077,

		/// <summary>
		/// The MAPI property PR_RCVD_REPRESENTING_EMAIL_ADDRESS.
		/// </summary>
		RcvdRepresentingEmailAddress             = 0x0078,

		/// <summary>
		/// The MAPI property PR_RCVD_REPRESENTING_ENTRYID.
		/// </summary>
		RcvdRepresentingEntryId                  = 0x0043,

		/// <summary>
		/// The MAPI property PR_RCVD_REPRESENTING_NAME.
		/// </summary>
		RcvdRepresentingName                     = 0x0044,

		/// <summary>
		/// The MAPI property PR_RCVD_REPRESENTING_SEARCH_KEY.
		/// </summary>
		RcvdRepresentingSearchKey                = 0x0052,

		/// <summary>
		/// The MAPI property PR_READ_RECEIPT_ENTRYID.
		/// </summary>
		ReadReceiptEntryId                       = 0x0046,

		/// <summary>
		/// The MAPI property PR_READ_RECEIPT_REQUESTED.
		/// </summary>
		ReadReceiptRequested                     = 0x0029,

		/// <summary>
		/// The MAPI property PR_READ_RECEIPT_SEARCH_KEY.
		/// </summary>
		ReadReceiptSearchKey                     = 0x0053,

		/// <summary>
		/// The MAPI property PR_RECEIPT_TIME.
		/// </summary>
		ReceiptTime                              = 0x002A,

		/// <summary>
		/// The MAPI property PR_RECEIVED_BY_ADDRTYPE.
		/// </summary>
		ReceivedByAddrtype                       = 0x0075,

		/// <summary>
		/// The MAPI property PR_RECEIVED_BY_EMAIL_ADDRESS.
		/// </summary>
		ReceivedByEmailAddress                   = 0x0076,

		/// <summary>
		/// The MAPI property PR_RECEIVED_BY_ENTRYID.
		/// </summary>
		ReceivedByEntryId                        = 0x003F,

		/// <summary>
		/// The MAPI property PR_RECEIVED_BY_NAME.
		/// </summary>
		ReceivedByName                           = 0x0040,

		/// <summary>
		/// The MAPI property PR_RECEIVED_BY_SEARCH_KEY.
		/// </summary>
		ReceivedBySearchKey                      = 0x0051,

		/// <summary>
		/// The MAPI property PR_RECEIVE_FOLDER_SETTINGS.
		/// </summary>
		ReceiveFolderSettings                    = 0x3415,

		/// <summary>
		/// The MAPI property PR_RECIPIENT_CERTIFICATE.
		/// </summary>
		RecipientCertificate                     = 0x0C13,

		/// <summary>
		/// The MAPI property PR_RECIPIENT_NUMBER_FOR_ADVICE.
		/// </summary>
		RecipientNumberForAdvice                 = 0x0C14,

		/// <summary>
		/// The MAPI property PR_RECIPIENT_REASSIGNMENT_PROHIBITED.
		/// </summary>
		RecipientReassignmentProhibited          = 0x002B,

		/// <summary>
		/// The MAPI property PR_RECIPIENT_STATUS.
		/// </summary>
		RecipientStatus                          = 0x0E15,

		/// <summary>
		/// The MAPI property PR_RECIPIENT_TYPE.
		/// </summary>
		RecipientType                            = 0x0C15,

		/// <summary>
		/// The MAPI property PR_RECORD_KEY.
		/// </summary>
		RecordKey                                = 0x0FF9,

		/// <summary>
		/// The MAPI property PR_REDIRECTION_HISTORY.
		/// </summary>
		RedirectionHistory                       = 0x002C,

		/// <summary>
		/// The MAPI property PR_REFERRED_BY_NAME.
		/// </summary>
		ReferredByName                           = 0x3A47,

		/// <summary>
		/// The MAPI property PR_REGISTERED_MAIL_TYPE.
		/// </summary>
		RegisteredMailType                       = 0x0C16,

		/// <summary>
		/// The MAPI property PR_RELATED_IPMS.
		/// </summary>
		RelatedIpms                              = 0x002D,

		/// <summary>
		/// The MAPI property PR_REMOTE_PROGRESS.
		/// </summary>
		RemoteProgress                           = 0x3E0B,

		/// <summary>
		/// The MAPI property PR_REMOTE_PROGRESS_TEXT.
		/// </summary>
		RemoteProgressText                       = 0x3E0C,

		/// <summary>
		/// The MAPI property PR_REMOTE_VALIDATE_OK.
		/// </summary>
		RemoteValidateOk                         = 0x3E0D,

		/// <summary>
		/// The MAPI property PR_RENDERING_POSITION.
		/// </summary>
		RenderingPosition                        = 0x370B,

		/// <summary>
		/// The MAPI property PR_REPLY_RECIPIENT_ENTRIES.
		/// </summary>
		ReplyRecipientEntries                    = 0x004F,

		/// <summary>
		/// The MAPI property PR_REPLY_RECIPIENT_NAMES.
		/// </summary>
		ReplyRecipientNames                      = 0x0050,

		/// <summary>
		/// The MAPI property PR_REPLY_REQUESTED.
		/// </summary>
		ReplyRequested                           = 0x0C17,

		/// <summary>
		/// The MAPI property PR_REPLY_TIME.
		/// </summary>
		ReplyTime                                = 0x0030,

		/// <summary>
		/// The MAPI property PR_REPORT_ENTRYID.
		/// </summary>
		ReportEntryId                            = 0x0045,

		/// <summary>
		/// The MAPI property PR_REPORTING_DLL_NAME.
		/// </summary>
		ReportingDlName                          = 0x1003,

		/// <summary>
		/// The MAPI property PR_REPORTING_MTA_CERTIFICATE.
		/// </summary>
		ReportingMtaCertificate                  = 0x1004,

		/// <summary>
		/// The MAPI property PR_REPORT_NAME.
		/// </summary>
		ReportName                               = 0x003A,

		/// <summary>
		/// The MAPI property PR_REPORT_SEARCH_KEY.
		/// </summary>
		ReportSearchKey                          = 0x0054,

		/// <summary>
		/// The MAPI property PR_REPORT_TAG.
		/// </summary>
		ReportTag                                = 0x0031,

		/// <summary>
		/// The MAPI property PR_REPORT_TEXT.
		/// </summary>
		ReportText                               = 0x1001,

		/// <summary>
		/// The MAPI property PR_REPORT_TIME.
		/// </summary>
		ReportTime                               = 0x0032,

		/// <summary>
		/// The MAPI property PR_REQUESTED_DELIVERY_METHOD.
		/// </summary>
		RequestedDeliveryMethod                  = 0x0C18,

		/// <summary>
		/// The MAPI property PR_RESOURCE_FLAGS.
		/// </summary>
		ResourceFlags                            = 0x3009,

		/// <summary>
		/// The MAPI property PR_RESOURCE_METHODS.
		/// </summary>
		ResourceMethods                          = 0x3E02,

		/// <summary>
		/// The MAPI property PR_RESOURCE_PATH.
		/// </summary>
		ResourcePath                             = 0x3E07,

		/// <summary>
		/// The MAPI property PR_RESOURCE_TYPE.
		/// </summary>
		ResourceType                             = 0x3E03,

		/// <summary>
		/// The MAPI property PR_RESPONSE_REQUESTED.
		/// </summary>
		ResponseRequested                        = 0x0063,

		/// <summary>
		/// The MAPI property PR_RESPONSIBILITY.
		/// </summary>
		Responsibility                           = 0x0E0F,

		/// <summary>
		/// The MAPI property PR_RETURNED_IPM.
		/// </summary>
		ReturnedIpm                              = 0x0033,

		/// <summary>
		/// The MAPI property PR_ROWID.
		/// </summary>
		Rowid                                    = 0x3000,

		/// <summary>
		/// The MAPI property PR_ROW_TYPE.
		/// </summary>
		RowType                                  = 0x0FF5,

		/// <summary>
		/// The MAPI property PR_RTF_COMPRESSED.
		/// </summary>
		RtfCompressed                            = 0x1009,

		/// <summary>
		/// The MAPI property PR_RTF_IN_SYNC.
		/// </summary>
		RtfInSync                                = 0x0E1F,

		/// <summary>
		/// The MAPI property PR_SYNC_BODY_COUNT.
		/// </summary>
		RtfSyncBodyCount                         = 0x1007,

		/// <summary>
		/// The MAPI property PR_RTF_SYNC_BODY_CRC.
		/// </summary>
		RtfSyncBodyCrc                           = 0x1006,

		/// <summary>
		/// The MAPI property PR_RTF_SYNC_BODY_TAG.
		/// </summary>
		RtfSyncBodyTag                           = 0x1008,

		/// <summary>
		/// The MAPI property PR_RTF_SYNC_PREFIX_COUNT.
		/// </summary>
		RtfSyncPrefixCount                       = 0x1010,

		/// <summary>
		/// The MAPI property PR_RTF_SYNC_TRAILING_COUNT.
		/// </summary>
		RtfSyncTrailingCount                     = 0x1011,

		/// <summary>
		/// The MAPI property PR_SEARCH.
		/// </summary>
		Search                                   = 0x3607,

		/// <summary>
		/// The MAPI property PR_SEARCH_KEY.
		/// </summary>
		SearchKey                                = 0x300B,

		/// <summary>
		/// The MAPI property PR_SECURITY.
		/// </summary>
		Security                                 = 0x0034,

		/// <summary>
		/// The MAPI property PR_SELECTABLE.
		/// </summary>
		Selectable                               = 0x3609,

		/// <summary>
		/// The MAPI property PR_SENDER_ADDRTYPE.
		/// </summary>
		SenderAddrtype                           = 0x0C1E,

		/// <summary>
		/// The MAPI property PR_SENDER_EMAIL_ADDRESS.
		/// </summary>
		SenderEmailAddress                       = 0x0C1F,

		/// <summary>
		/// The MAPI property PR_SENDER_ENTRYID.
		/// </summary>
		SenderEntryId                            = 0x0C19,

		/// <summary>
		/// The MAPI property PR_SENDER_NAME.
		/// </summary>
		SenderName                               = 0x0C1A,

		/// <summary>
		/// The MAPI property PR_SENDER_SEARCH_KEY.
		/// </summary>
		SenderSearchKey                          = 0x0C1D,

		/// <summary>
		/// The MAPI property PR_SEND_INTERNET_ENCODING.
		/// </summary>
		SendInternetEncoding                     = 0x3A71,

		/// <summary>
		/// The MAPI property PR_SEND_RECALL_REPORT.
		/// </summary>
		SendRecallReport                         = 0x6803,

		/// <summary>
		/// The MAPI property PR_SEND_RICH_INFO.
		/// </summary>
		SendRichInfo                             = 0x3A40,

		/// <summary>
		/// The MAPI property PR_SENSITIVITY.
		/// </summary>
		Sensitivity                              = 0x0036,

		/// <summary>
		/// The MAPI property PR_SENTMAIL_ENTRYID.
		/// </summary>
		SentmailEntryId                          = 0x0E0A,

		/// <summary>
		/// The MAPI property PR_SENT_REPRESENTING_ADDRTYPE.
		/// </summary>
		SentRepresentingAddrtype                 = 0x0064,

		/// <summary>
		/// The MAPI property PR_SENT_REPRESENTING_EMAIL_ADDRESS.
		/// </summary>
		SentRepresentingEmailAddress             = 0x0065,

		/// <summary>
		/// The MAPI property PR_SENT_REPRESENTING_ENTRYID.
		/// </summary>
		SentRepresentingEntryId                  = 0x0041,

		/// <summary>
		/// The MAPI property PR_SENT_REPRESENTING_NAME.
		/// </summary>
		SentRepresentingName                     = 0x0042,

		/// <summary>
		/// The MAPI property PR_SENT_REPRESENTING_SEARCH_KEY.
		/// </summary>
		SentRepresentingSearchKey                = 0x003B,

		/// <summary>
		/// The MAPI property PR_SERVICE_DELETE_FILES.
		/// </summary>
		ServiceDeleteFiles                       = 0x3D10,

		/// <summary>
		/// The MAPI property PR_SERVICE_DLL_NAME.
		/// </summary>
		ServiceDllName                           = 0x3D0A,

		/// <summary>
		/// The MAPI property PR_SERVICE_ENTRY_NAME.
		/// </summary>
		ServiceEntryName                         = 0x3D0B,

		/// <summary>
		/// The MAPI property PR_SERVICE_EXTRA_UIDS.
		/// </summary>
		ServiceExtraUids                         = 0x3D0D,

		/// <summary>
		/// The MAPI property PR_SERVICE_NAME.
		/// </summary>
		ServiceName                              = 0x3D09,

		/// <summary>
		/// The MAPI property PR_SERVICES.
		/// </summary>
		Services                                 = 0x3D0E,

		/// <summary>
		/// The MAPI property PR_SERVICE_SUPPORT_FILES.
		/// </summary>
		ServiceSupportFiles                      = 0x3D0F,

		/// <summary>
		/// The MAPI property PR_SERVICE_UID.
		/// </summary>
		ServiceUid                               = 0x3D0C,

		/// <summary>
		/// The MAPI property PR_SEVEN_BIT_DISPLAY_NAME.
		/// </summary>
		SevenBitDisplayName                      = 0x39FF,

		/// <summary>
		/// The MAPI property PR_SMTP_ADDRESS.
		/// </summary>
		SmtpAddress                              = 0x39FE,

		/// <summary>
		/// The MAPI property PR_SPOOLER_STATUS.
		/// </summary>
		SpoolerStatus                            = 0x0E10,

		/// <summary>
		/// The MAPI property PR_SPOUSE_NAME.
		/// </summary>
		SpouseName                               = 0x3A48,

		/// <summary>
		/// The MAPI property PR_START_DATE.
		/// </summary>
		StartDate                                = 0x0060,

		/// <summary>
		/// The MAPI property PR_STATE_OR_PROVINCE.
		/// </summary>
		StateOrProvince                          = 0x3A28,

		/// <summary>
		/// The MAPI property PR_STATUS.
		/// </summary>
		Status                                   = 0x360B,

		/// <summary>
		/// The MAPI property PR_STATUS_CODE.
		/// </summary>
		StatusCode                               = 0x3E04,

		/// <summary>
		/// The MAPI property PR_STATUS_STRING.
		/// </summary>
		StatusString                             = 0x3E08,

		/// <summary>
		/// The MAPI property PR_STORE_ENTRYID.
		/// </summary>
		StoreEntryId                             = 0x0FFB,

		/// <summary>
		/// The MAPI property PR_STORE_PROVIDERS.
		/// </summary>
		StoreProviders                           = 0x3D00,

		/// <summary>
		/// The MAPI property PR_STORE_RECORD_KEY.
		/// </summary>
		StoreRecordKey                           = 0x0FFA,

		/// <summary>
		/// The MAPI property PR_STORE_STATE.
		/// </summary>
		StoreState                               = 0x340E,

		/// <summary>
		/// The MAPI property PR_STORE_SUPPORT_MASK.
		/// </summary>
		StoreSupportMask                         = 0x340D,

		/// <summary>
		/// The MAPI property PR_STREET_ADDRESS.
		/// </summary>
		StreetAddress                            = 0x3A29,

		/// <summary>
		/// The MAPI property PR_SUBFOLDERS.
		/// </summary>
		Subfolders                               = 0x360A,

		/// <summary>
		/// The MAPI property PR_SUBJECT.
		/// </summary>
		Subject                                  = 0x0037,

		/// <summary>
		/// The MAPI property PR_SUBJECT_IPM.
		/// </summary>
		SubjectIpm                               = 0x0038,

		/// <summary>
		/// The MAPI property PR_SUBJECT_PREFIX.
		/// </summary>
		SubjectPrefix                            = 0x003D,

		/// <summary>
		/// The MAPI property PR_SUBMIT_FLAGS.
		/// </summary>
		SubmitFlags                              = 0x0E14,

		/// <summary>
		/// The MAPI property PR_SUPERSEDES.
		/// </summary>
		Supersedes                               = 0x103A,

		/// <summary>
		/// The MAPI property PR_SUPPLEMENTARY_INFO.
		/// </summary>
		SupplementaryInfo                        = 0x0C1B,

		/// <summary>
		/// The MAPI property PR_SURNAME.
		/// </summary>
		Surname                                  = 0x3A11,

		/// <summary>
		/// The MAPI property PR_TELEX_NUMBER.
		/// </summary>
		TelexNumber                              = 0x3A2C,

		/// <summary>
		/// The MAPI property PR_TEMPLATEID.
		/// </summary>
		Templateid                               = 0x3902,

		/// <summary>
		/// The MAPI property PR_TITLE.
		/// </summary>
		Title                                    = 0x3A17,

		/// <summary>
		/// The MAPI property PR_TNEF_CORRELATION_KEY.
		/// </summary>
		TnefCorrelationKey                       = 0x007F,

		/// <summary>
		/// The MAPI property PR_TRANSMITABLE_DISPLAY_NAME.
		/// </summary>
		TransmitableDisplayName                  = 0x3A20,

		/// <summary>
		/// The MAPI property PR_TRANSPORT_KEY.
		/// </summary>
		TransportKey                             = 0x0E16,

		/// <summary>
		/// The MAPI property PR_TRANSPORT_MESSAGE_HEADERS.
		/// </summary>
		TransportMessageHeaders                  = 0x007D,

		/// <summary>
		/// The MAPI property PR_TRANSPORT_PROVIDERS.
		/// </summary>
		TransportProviders                       = 0x3D02,

		/// <summary>
		/// The MAPI property PR_TRANSPORT_STATUS.
		/// </summary>
		TransportStatus                          = 0x0E11,

		/// <summary>
		/// The MAPI property PR_TTYTDD_PHONE_NUMBER.
		/// </summary>
		TtytddPhoneNumber                        = 0x3A4B,

		/// <summary>
		/// The MAPI property PR_TYPE_OF_MTS_USER.
		/// </summary>
		TypeOfMtsUser                            = 0x0C1C,

		/// <summary>
		/// The MAPI property PR_USER_CERTIFICATE.
		/// </summary>
		UserCertificate                          = 0x3A22,

		/// <summary>
		/// The MAPI property PR_USER_X509_CERTIFICATE.
		/// </summary>
		UserX509Certificate                      = 0x3A70,

		/// <summary>
		/// The MAPI property PR_VALID_FOLDER_MASK.
		/// </summary>
		ValidFolderMask                          = 0x35DF,

		/// <summary>
		/// The MAPI property PR_VIEWS_ENTRYID.
		/// </summary>
		ViewsEntryId                             = 0x35E5,

		/// <summary>
		/// The MAPI property PR_WEDDING_ANNIVERSARY.
		/// </summary>
		WeddingAnniversary                       = 0x3A41,

		/// <summary>
		/// The MAPI property PR_X400_CONTENT_TYPE.
		/// </summary>
		X400ContentType                          = 0x003C,

		/// <summary>
		/// The MAPI property PR_X400_DEFERRED_DELIVERY_CANCEL.
		/// </summary>
		X400DeferredDeliveryCancel               = 0x3E09,

		/// <summary>
		/// The MAPI property PR_XPOS.
		/// </summary>
		Xpos                                     = 0x3F05,

		/// <summary>
		/// The MAPI property PR_YPOS.
		/// </summary>
		Ypos                                     = 0x3F06,
	}
}

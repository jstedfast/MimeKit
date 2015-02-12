//
// TnefPropertyTag.cs
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
	/// A TNEF property tag.
	/// </summary>
	/// <remarks>
	/// A TNEF property tag.
	/// </remarks>
	public struct TnefPropertyTag
	{
		/// <summary>
		/// The MAPI property PR_AB_DEFAULT_DIR.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_AB_DEFAULT_DIR.
		/// </remarks>
		public static readonly TnefPropertyTag AbDefaultDir = new TnefPropertyTag (TnefPropertyId.AbDefaultDir, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_AB_DEFAULT_PAB.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_AB_DEFAULT_PAD.
		/// </remarks>
		public static readonly TnefPropertyTag AbDefaultPab = new TnefPropertyTag (TnefPropertyId.AbDefaultPab, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_AB_PROVIDER_ID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_AB_PROVIDER_ID.
		/// </remarks>
		public static readonly TnefPropertyTag AbProviderId = new TnefPropertyTag (TnefPropertyId.AbProviderId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_AB_PROVIDERS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_AB_PROVIDERS.
		/// </remarks>
		public static readonly TnefPropertyTag AbProviders = new TnefPropertyTag (TnefPropertyId.AbProviders, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_AB_SEARCH_PATH.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_AB_SEARCH_PATH.
		/// </remarks>
		public static readonly TnefPropertyTag AbSearchPath = new TnefPropertyTag (TnefPropertyId.AbSearchPath, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_AB_SEARCH_PATH_UPDATE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_AB_SEARCH_PATH_UPDATE.
		/// </remarks>
		public static readonly TnefPropertyTag AbSearchPathUpdate = new TnefPropertyTag (TnefPropertyId.AbSearchPathUpdate, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_ACCESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ACCESS.
		/// </remarks>
		public static readonly TnefPropertyTag Access = new TnefPropertyTag (TnefPropertyId.Access, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_ACCESS_LEVEL.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ACCESS_LEVEL.
		/// </remarks>
		public static readonly TnefPropertyTag AccessLevel = new TnefPropertyTag (TnefPropertyId.AccessLevel, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_ACCOUNT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ACCOUNT.
		/// </remarks>
		public static readonly TnefPropertyTag AccountA = new TnefPropertyTag (TnefPropertyId.Account, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ACCOUNT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ACCOUNT.
		/// </remarks>
		public static readonly TnefPropertyTag AccountW = new TnefPropertyTag (TnefPropertyId.Account, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ACKNOWLEDGEMENT_MODE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ACKNOWLEDGEMENT_MODE.
		/// </remarks>
		public static readonly TnefPropertyTag AcknowledgementMode = new TnefPropertyTag (TnefPropertyId.AcknowledgementMode, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_ADDRTYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ADDRTYPE.
		/// </remarks>
		public static readonly TnefPropertyTag AddrtypeA = new TnefPropertyTag (TnefPropertyId.Addrtype, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ADDRTYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ADDRTYPE.
		/// </remarks>
		public static readonly TnefPropertyTag AddrtypeW = new TnefPropertyTag (TnefPropertyId.Addrtype, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ALTERNATE_RECIPIENT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ALTERNATE_RECIPIENT.
		/// </remarks>
		public static readonly TnefPropertyTag AlternateRecipient = new TnefPropertyTag (TnefPropertyId.AlternateRecipient, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_ALTERNATE_RECIPIENT_ALLOWED.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ALTERNATE_RECIPIENT_ALLOWED.
		/// </remarks>
		public static readonly TnefPropertyTag AlternateRecipientAllowed = new TnefPropertyTag (TnefPropertyId.AlternateRecipientAllowed, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_ANR.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ANR.
		/// </remarks>
		public static readonly TnefPropertyTag AnrA = new TnefPropertyTag (TnefPropertyId.Anr, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ANR.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ANR.
		/// </remarks>
		public static readonly TnefPropertyTag AnrW = new TnefPropertyTag (TnefPropertyId.Anr, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ASSISTANT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ASSISTANT.
		/// </remarks>
		public static readonly TnefPropertyTag AssistantA = new TnefPropertyTag (TnefPropertyId.Assistant, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ASSISTANT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ASSISTANT.
		/// </remarks>
		public static readonly TnefPropertyTag AssistantW = new TnefPropertyTag (TnefPropertyId.Assistant, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ASSISTANT_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ASSISTANT_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag AssistantTelephoneNumberA = new TnefPropertyTag (TnefPropertyId.AssistantTelephoneNumber, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ASSISTANT_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ASSISTANT_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag AssistantTelephoneNumberW = new TnefPropertyTag (TnefPropertyId.AssistantTelephoneNumber, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ASSOC_CONTENT_COUNT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ASSOC_CONTENT_COUNT.
		/// </remarks>
		public static readonly TnefPropertyTag AssocContentCount = new TnefPropertyTag (TnefPropertyId.AssocContentCount, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_ATTACH_ADDITIONAL_INFO.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_ADDITIONAL_INFO.
		/// </remarks>
		public static readonly TnefPropertyTag AttachAdditionalInfo = new TnefPropertyTag (TnefPropertyId.AttachAdditionalInfo, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_ATTACH_CONTENT_BASE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_CONTENT_BASE.
		/// </remarks>
		public static readonly TnefPropertyTag AttachContentBaseA = new TnefPropertyTag (TnefPropertyId.AttachContentBase, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ATTACH_CONTENT_BASE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_CONTENT_BASE.
		/// </remarks>
		public static readonly TnefPropertyTag AttachContentBaseW = new TnefPropertyTag (TnefPropertyId.AttachContentBase, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ATTACH_CONTENT_ID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_CONTENT_ID.
		/// </remarks>
		public static readonly TnefPropertyTag AttachContentIdA = new TnefPropertyTag (TnefPropertyId.AttachContentId, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ATTACH_CONTENT_ID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_CONTENT_ID.
		/// </remarks>
		public static readonly TnefPropertyTag AttachContentIdW = new TnefPropertyTag (TnefPropertyId.AttachContentId, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ATTACH_CONTENT_LOCATION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_CONTENT_LOCATION.
		/// </remarks>
		public static readonly TnefPropertyTag AttachContentLocationA = new TnefPropertyTag (TnefPropertyId.AttachContentLocation, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ATTACH_CONTENT_LOCATION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_CONTENT_LOCATION.
		/// </remarks>
		public static readonly TnefPropertyTag AttachContentLocationW = new TnefPropertyTag (TnefPropertyId.AttachContentLocation, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ATTACH_DATA.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_DATA.
		/// </remarks>
		public static readonly TnefPropertyTag AttachDataBin = new TnefPropertyTag (TnefPropertyId.AttachData, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_ATTACH_DATA.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_DATA.
		/// </remarks>
		public static readonly TnefPropertyTag AttachDataObj = new TnefPropertyTag (TnefPropertyId.AttachData, TnefPropertyType.Object);

		/// <summary>
		/// The MAPI property PR_ATTACH_DISPOSITION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_DISPOSITION.
		/// </remarks>
		public static readonly TnefPropertyTag AttachDispositionA = new TnefPropertyTag (TnefPropertyId.AttachDisposition, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ATTACH_DISPOSITION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_DISPOSITION.
		/// </remarks>
		public static readonly TnefPropertyTag AttachDispositionW = new TnefPropertyTag (TnefPropertyId.AttachDisposition, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ATTACH_ENCODING.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_ENCODING.
		/// </remarks>
		public static readonly TnefPropertyTag AttachEncoding = new TnefPropertyTag (TnefPropertyId.AttachEncoding, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_ATTACH_EXTENSION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_EXTENSION.
		/// </remarks>
		public static readonly TnefPropertyTag AttachExtensionA = new TnefPropertyTag (TnefPropertyId.AttachExtension, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ATTACH_EXTENSION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_EXTENSION.
		/// </remarks>
		public static readonly TnefPropertyTag AttachExtensionW = new TnefPropertyTag (TnefPropertyId.AttachExtension, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ATTACH_FILENAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_FILENAME.
		/// </remarks>
		public static readonly TnefPropertyTag AttachFilenameA = new TnefPropertyTag (TnefPropertyId.AttachFilename, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ATTACH_FILENAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_FILENAME.
		/// </remarks>
		public static readonly TnefPropertyTag AttachFilenameW = new TnefPropertyTag (TnefPropertyId.AttachFilename, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ATTACH_FLAGS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_FLAGS.
		/// </remarks>
		public static readonly TnefPropertyTag AttachFlags = new TnefPropertyTag (TnefPropertyId.AttachFlags, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_ATTACH_LONG_FILENAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_LONG_FILENAME.
		/// </remarks>
		public static readonly TnefPropertyTag AttachLongFilenameA = new TnefPropertyTag (TnefPropertyId.AttachLongFilename, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ATTACH_LONG_FILENAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_LONG_FILENAME.
		/// </remarks>
		public static readonly TnefPropertyTag AttachLongFilenameW = new TnefPropertyTag (TnefPropertyId.AttachLongFilename, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ATTACH_LONG_PATHNAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_LONG_PATHNAME.
		/// </remarks>
		public static readonly TnefPropertyTag AttachLongPathnameA = new TnefPropertyTag (TnefPropertyId.AttachLongPathname, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ATTACH_LONG_PATHNAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_LONG_PATHNAME.
		/// </remarks>
		public static readonly TnefPropertyTag AttachLongPathnameW = new TnefPropertyTag (TnefPropertyId.AttachLongPathname, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ATTACHMENT_X400_PARAMETERS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACHMENT_X400_PARAMETERS.
		/// </remarks>
		public static readonly TnefPropertyTag AttachmentX400Parameters = new TnefPropertyTag (TnefPropertyId.AttachmentX400Parameters, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_ATTACH_METHOD.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_METHOD.
		/// </remarks>
		public static readonly TnefPropertyTag AttachMethod = new TnefPropertyTag (TnefPropertyId.AttachMethod, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_ATTACH_MIME_SEQUENCE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_MIME_SEQUENCE.
		/// </remarks>
		public static readonly TnefPropertyTag AttachMimeSequence = new TnefPropertyTag (TnefPropertyId.AttachMimeSequence, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_ATTACH_MIME_TAG.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_MIME_TAG.
		/// </remarks>
		public static readonly TnefPropertyTag AttachMimeTagA = new TnefPropertyTag (TnefPropertyId.AttachMimeTag, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ATTACH_MIME_TAG.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_MIME_TAG.
		/// </remarks>
		public static readonly TnefPropertyTag AttachMimeTagW = new TnefPropertyTag (TnefPropertyId.AttachMimeTag, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ATTACH_NETSCAPE_MAC_INFO.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_NETSCAPE_MAC_INFO.
		/// </remarks>
		public static readonly TnefPropertyTag AttachNetscapeMacInfo = new TnefPropertyTag (TnefPropertyId.AttachNetscapeMacInfo, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_ATTACH_NUM.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_NUM.
		/// </remarks>
		public static readonly TnefPropertyTag AttachNum = new TnefPropertyTag (TnefPropertyId.AttachNum, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_ATTACH_PATHNAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_PATHNAME.
		/// </remarks>
		public static readonly TnefPropertyTag AttachPathnameA = new TnefPropertyTag (TnefPropertyId.AttachPathname, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ATTACH_PATHNAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_PATHNAME.
		/// </remarks>
		public static readonly TnefPropertyTag AttachPathnameW = new TnefPropertyTag (TnefPropertyId.AttachPathname, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ATTACH_RENDERING.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_RENDERING.
		/// </remarks>
		public static readonly TnefPropertyTag AttachRendering = new TnefPropertyTag (TnefPropertyId.AttachRendering, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_ATTACH_SIZE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_SIZE.
		/// </remarks>
		public static readonly TnefPropertyTag AttachSize = new TnefPropertyTag (TnefPropertyId.AttachSize, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_ATTACH_TAG.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_TAG.
		/// </remarks>
		public static readonly TnefPropertyTag AttachTag = new TnefPropertyTag (TnefPropertyId.AttachTag, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_ATTACH_TRANSPORT_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_TRANSPORT_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag AttachTransportNameA = new TnefPropertyTag (TnefPropertyId.AttachTransportName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ATTACH_TRANSPORT_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ATTACH_TRANSPORT_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag AttachTransportNameW = new TnefPropertyTag (TnefPropertyId.AttachTransportName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_AUTHORIZING_USERS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_AUTHORIZING_USERS.
		/// </remarks>
		public static readonly TnefPropertyTag AuthorizingUsers = new TnefPropertyTag (TnefPropertyId.AuthorizingUsers, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_AUTOFORWARDED.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_AUTOFORWARDED.
		/// </remarks>
		public static readonly TnefPropertyTag AutoForwarded = new TnefPropertyTag (TnefPropertyId.AutoForwarded, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_AUTOFORWARDING_COMMENT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_AUTOFORWARDING_COMMENT.
		/// </remarks>
		public static readonly TnefPropertyTag AutoForwardingCommentA = new TnefPropertyTag (TnefPropertyId.AutoForwardingComment, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_AUTOFORWARDING_COMMENT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_AUTOFORWARDING_COMMENT.
		/// </remarks>
		public static readonly TnefPropertyTag AutoForwardingCommentW = new TnefPropertyTag (TnefPropertyId.AutoForwardingComment, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_AUTORESPONSE_SUPPRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_AUTORESPONSE_SUPPRESS.
		/// </remarks>
		public static readonly TnefPropertyTag AutoResponseSuppress = new TnefPropertyTag (TnefPropertyId.AutoResponseSuppress, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_BEEPER_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BEEPER_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag BeeperTelephoneNumberA = new TnefPropertyTag (TnefPropertyId.BeeperTelephoneNumber, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_BEEPER_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BEEPER_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag BeeperTelephoneNumberW = new TnefPropertyTag (TnefPropertyId.BeeperTelephoneNumber, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_BIRTHDAY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BIRTHDAY.
		/// </remarks>
		public static readonly TnefPropertyTag Birthday = new TnefPropertyTag (TnefPropertyId.Birthday, TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PR_BODY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BODY.
		/// </remarks>
		public static readonly TnefPropertyTag BodyA = new TnefPropertyTag (TnefPropertyId.Body, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_BODY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BODY.
		/// </remarks>
		public static readonly TnefPropertyTag BodyW = new TnefPropertyTag (TnefPropertyId.Body, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_BODY_CONTENT_ID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BODY_CONTENT_ID.
		/// </remarks>
		public static readonly TnefPropertyTag BodyContentIdA = new TnefPropertyTag (TnefPropertyId.BodyContentId, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_BODY_CONTENT_ID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BODY_CONTENT_ID.
		/// </remarks>
		public static readonly TnefPropertyTag BodyContentIdW = new TnefPropertyTag (TnefPropertyId.BodyContentId, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_BODY_CONTENT_LOCATION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BODY_CONTENT_LOCATION.
		/// </remarks>
		public static readonly TnefPropertyTag BodyContentLocationA = new TnefPropertyTag (TnefPropertyId.BodyContentLocation, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_BODY_CONTENT_LOCATION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BODY_CONTENT_LOCATION.
		/// </remarks>
		public static readonly TnefPropertyTag BodyContentLocationW = new TnefPropertyTag (TnefPropertyId.BodyContentLocation, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_BODY_CRC.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BODY_CRC.
		/// </remarks>
		public static readonly TnefPropertyTag BodyCrc = new TnefPropertyTag (TnefPropertyId.BodyCrc, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_BODY_HTML.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BODY_HTML.
		/// </remarks>
		public static readonly TnefPropertyTag BodyHtmlA = new TnefPropertyTag (TnefPropertyId.BodyHtml, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_BODY_HTML.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BODY_HTML.
		/// </remarks>
		public static readonly TnefPropertyTag BodyHtmlB = new TnefPropertyTag (TnefPropertyId.BodyHtml, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_BODY_HTML.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BODY_HTML.
		/// </remarks>
		public static readonly TnefPropertyTag BodyHtmlW = new TnefPropertyTag (TnefPropertyId.BodyHtml, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_BUSINESS2_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BUSINESS2_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag Business2TelephoneNumberA = new TnefPropertyTag (TnefPropertyId.Business2TelephoneNumber, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_BUSINESS2_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BUSINESS2_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag Business2TelephoneNumberAMv = new TnefPropertyTag (TnefPropertyId.Business2TelephoneNumber, TnefPropertyType.String8, true);

		/// <summary>
		/// The MAPI property PR_BUSINESS2_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BUSINESS2_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag Business2TelephoneNumberW = new TnefPropertyTag (TnefPropertyId.Business2TelephoneNumber, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_BUSINESS2_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BUSINESS2_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag Business2TelephoneNumberWMv = new TnefPropertyTag (TnefPropertyId.Business2TelephoneNumber, TnefPropertyType.Unicode, true);

		/// <summary>
		/// The MAPI property PR_BUSINESS_ADDRESS_CITY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BUSINESS_ADDRESS_CITY.
		/// </remarks>
		public static readonly TnefPropertyTag BusinessAddressCityA = new TnefPropertyTag (TnefPropertyId.BusinessAddressCity, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_BUSINESS_ADDRESS_CITY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BUSINESS_ADDRESS_CITY.
		/// </remarks>
		public static readonly TnefPropertyTag BusinessAddressCityW = new TnefPropertyTag (TnefPropertyId.BusinessAddressCity, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_BUSINESS_ADDRESS_COUNTRY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BUSINESS_ADDRESS_COUNTRY.
		/// </remarks>
		public static readonly TnefPropertyTag BusinessAddressCountryA = new TnefPropertyTag (TnefPropertyId.BusinessAddressCountry, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_BUSINESS_ADDRESS_COUNTRY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BUSINESS_ADDRESS_COUNTRY.
		/// </remarks>
		public static readonly TnefPropertyTag BusinessAddressCountryW = new TnefPropertyTag (TnefPropertyId.BusinessAddressCountry, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_BUSINESS_ADDRESS_POSTAL_CODE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BUSINESS_ADDRESS_POSTAL_CODE.
		/// </remarks>
		public static readonly TnefPropertyTag BusinessAddressPostalCodeA = new TnefPropertyTag (TnefPropertyId.BusinessAddressPostalCode, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_BUSINESS_ADDRESS_POSTAL_CODE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BUSINESS_ADDRESS_POSTAL_CODE.
		/// </remarks>
		public static readonly TnefPropertyTag BusinessAddressPostalCodeW = new TnefPropertyTag (TnefPropertyId.BusinessAddressPostalCode, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_BUSINESS_ADDRESS_POSTAL_STREET.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BUSINESS_ADDRESS_POSTAL_STREET.
		/// </remarks>
		public static readonly TnefPropertyTag BusinessAddressStreetA = new TnefPropertyTag (TnefPropertyId.BusinessAddressStreet, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_BUSINESS_ADDRESS_POSTAL_STREET.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BUSINESS_ADDRESS_POSTAL_STREET.
		/// </remarks>
		public static readonly TnefPropertyTag BusinessAddressStreetW = new TnefPropertyTag (TnefPropertyId.BusinessAddressStreet, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_BUSINESS_FAX_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BUSINESS_FAX_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag BusinessFaxNumberA = new TnefPropertyTag (TnefPropertyId.BusinessFaxNumber, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_BUSINESS_FAX_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BUSINESS_FAX_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag BusinessFaxNumberW = new TnefPropertyTag (TnefPropertyId.BusinessFaxNumber, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_BUSINESS_HOME_PAGE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BUSINESS_HOME_PAGE.
		/// </remarks>
		public static readonly TnefPropertyTag BusinessHomePageA = new TnefPropertyTag (TnefPropertyId.BusinessHomePage, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_BUSINESS_HOME_PAGE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_BUSINESS_HOME_PAGE.
		/// </remarks>
		public static readonly TnefPropertyTag BusinessHomePageW = new TnefPropertyTag (TnefPropertyId.BusinessHomePage, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_CALLBACK_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CALLBACK_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag CallbackTelephoneNumberA = new TnefPropertyTag (TnefPropertyId.CallbackTelephoneNumber, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_CALLBACK_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CALLBACK_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag CallbackTelephoneNumberW = new TnefPropertyTag (TnefPropertyId.CallbackTelephoneNumber, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_CAR_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CAR_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag CarTelephoneNumberA = new TnefPropertyTag (TnefPropertyId.CarTelephoneNumber, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_CAR_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CAR_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag CarTelephoneNumberW = new TnefPropertyTag (TnefPropertyId.CarTelephoneNumber, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_CHILDRENS_NAMES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CHILDRENS_NAMES.
		/// </remarks>
		public static readonly TnefPropertyTag ChildrensNamesA = new TnefPropertyTag (TnefPropertyId.ChildrensNames, TnefPropertyType.String8, true);

		/// <summary>
		/// The MAPI property PR_CHILDRENS_NAMES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CHILDRENS_NAMES.
		/// </remarks>
		public static readonly TnefPropertyTag ChildrensNamesW = new TnefPropertyTag (TnefPropertyId.ChildrensNames, TnefPropertyType.Unicode, true);

		/// <summary>
		/// The MAPI property PR_CLIENT_SUBMIT_TIME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CLIENT_SUBMIT_TIME.
		/// </remarks>
		public static readonly TnefPropertyTag ClientSubmitTime = new TnefPropertyTag (TnefPropertyId.ClientSubmitTime, TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PR_COMMENT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_COMMENT.
		/// </remarks>
		public static readonly TnefPropertyTag CommentA = new TnefPropertyTag (TnefPropertyId.Comment, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_COMMENT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_COMMENT.
		/// </remarks>
		public static readonly TnefPropertyTag CommentW = new TnefPropertyTag (TnefPropertyId.Comment, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_COMMON_VIEWS_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_COMMON_VIEWS_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag CommonViewsEntryId = new TnefPropertyTag (TnefPropertyId.CommonViewsEntryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_COMPANY_MAIN_PHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_COMPANY_MAIN_PHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag CompanyMainPhoneNumberA = new TnefPropertyTag (TnefPropertyId.CompanyMainPhoneNumber, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_COMPANY_MAIN_PHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_COMPANY_MAIN_PHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag CompanyMainPhoneNumberW = new TnefPropertyTag (TnefPropertyId.CompanyMainPhoneNumber, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_COMPANY_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_COMPANY_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag CompanyNameA = new TnefPropertyTag (TnefPropertyId.CompanyName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_COMPANY_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_COMPANY_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag CompanyNameW = new TnefPropertyTag (TnefPropertyId.CompanyName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_COMPUTER_NETWORK_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_COMPUTER_NETWORK_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag ComputerNetworkNameA = new TnefPropertyTag (TnefPropertyId.ComputerNetworkName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_COMPUTER_NETWORK_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_COMPUTER_NETWORK_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag ComputerNetworkNameW = new TnefPropertyTag (TnefPropertyId.ComputerNetworkName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_CONTACT_ADDRTYPES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTACT_ADDRTYPES.
		/// </remarks>
		public static readonly TnefPropertyTag ContactAddrtypesA = new TnefPropertyTag (TnefPropertyId.ContactAddrtypes, TnefPropertyType.String8, true);

		/// <summary>
		/// The MAPI property PR_CONTACT_ADDRTYPES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTACT_ADDRTYPES.
		/// </remarks>
		public static readonly TnefPropertyTag ContactAddrtypesW = new TnefPropertyTag (TnefPropertyId.ContactAddrtypes, TnefPropertyType.Unicode, true);

		/// <summary>
		/// The MAPI property PR_CONTACT_DEFAULT_ADDRESS_INDEX.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTACT_DEFAULT_ADDRESS_INDEX.
		/// </remarks>
		public static readonly TnefPropertyTag ContactDefaultAddressIndex = new TnefPropertyTag (TnefPropertyId.ContactDefaultAddressIndex, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_CONTACT_EMAIL_ADDRESSES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTACT_EMAIL_ADDRESSES.
		/// </remarks>
		public static readonly TnefPropertyTag ContactEmailAddressesA = new TnefPropertyTag (TnefPropertyId.ContactEmailAddresses, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_CONTACT_EMAIL_ADDRESSES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTACT_EMAIL_ADDRESSES.
		/// </remarks>
		public static readonly TnefPropertyTag ContactEmailAddressesW = new TnefPropertyTag (TnefPropertyId.ContactEmailAddresses, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_CONTACT_ENTRYIDS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTACT_ENTRYIDS.
		/// </remarks>
		public static readonly TnefPropertyTag ContactEntryIds = new TnefPropertyTag (TnefPropertyId.ContactEntryIds, TnefPropertyType.Binary, true);

		/// <summary>
		/// The MAPI property PR_CONTACT_VERSION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTACT_VERSION.
		/// </remarks>
		public static readonly TnefPropertyTag ContactVersion = new TnefPropertyTag (TnefPropertyId.ContactVersion, TnefPropertyType.ClassId);

		/// <summary>
		/// The MAPI property PR_CONTAINER_CLASS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTAINER_CLASS.
		/// </remarks>
		public static readonly TnefPropertyTag ContainerClassA = new TnefPropertyTag (TnefPropertyId.ContainerClass, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_CONTAINER_CLASS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTAINER_CLASS.
		/// </remarks>
		public static readonly TnefPropertyTag ContainerClassW = new TnefPropertyTag (TnefPropertyId.ContainerClass, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_CONTAINER_CONTENTS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTAINER_CONTENTS.
		/// </remarks>
		public static readonly TnefPropertyTag ContainerContents = new TnefPropertyTag (TnefPropertyId.ContainerContents, TnefPropertyType.Object);

		/// <summary>
		/// The MAPI property PR_CONTAINER_FLAGS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTAINER_FLAGS.
		/// </remarks>
		public static readonly TnefPropertyTag ContainerFlags = new TnefPropertyTag (TnefPropertyId.ContainerFlags, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_CONTAINER_HIERARCHY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTAINER_HIERARCHY.
		/// </remarks>
		public static readonly TnefPropertyTag ContainerHierarchy = new TnefPropertyTag (TnefPropertyId.ContainerHierarchy, TnefPropertyType.Object);

		/// <summary>
		/// The MAPI property PR_CONTAINER_MODIFY_VERSION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTAINER_MODIFY_VERSION.
		/// </remarks>
		public static readonly TnefPropertyTag ContainerModifyVersion = new TnefPropertyTag (TnefPropertyId.ContainerModifyVersion, TnefPropertyType.I8);

		/// <summary>
		/// The MAPI property PR_CONTENT_CONFIDENTIALITY_ALGORITHM_ID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTENT_CONFIDENTIALITY_ALGORITHM_ID.
		/// </remarks>
		public static readonly TnefPropertyTag ContentConfidentialityAlgorithmId = new TnefPropertyTag (TnefPropertyId.ContentConfidentialityAlgorithmId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_CONTENT_CORRELATOR.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTENT_CORRELATOR.
		/// </remarks>
		public static readonly TnefPropertyTag ContentCorrelator = new TnefPropertyTag (TnefPropertyId.ContentCorrelator, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_CONTENT_COUNT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTENT_COUNT.
		/// </remarks>
		public static readonly TnefPropertyTag ContentCount = new TnefPropertyTag (TnefPropertyId.ContentCount, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_CONTENT_IDENTIFIER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTENT_IDENTIFIER.
		/// </remarks>
		public static readonly TnefPropertyTag ContentIdentifierA = new TnefPropertyTag (TnefPropertyId.ContentIdentifier, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_CONTENT_IDENTIFIER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTENT_IDENTIFIER.
		/// </remarks>
		public static readonly TnefPropertyTag ContentIdentifierW = new TnefPropertyTag (TnefPropertyId.ContentIdentifier, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_CONTENT_INTEGRITY_CHECK.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTENT_INTEGRITY_CHECK.
		/// </remarks>
		public static readonly TnefPropertyTag ContentIntegrityCheck = new TnefPropertyTag (TnefPropertyId.ContentIntegrityCheck, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_CONTENT_LENGTH.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTENT_LENGTH.
		/// </remarks>
		public static readonly TnefPropertyTag ContentLength = new TnefPropertyTag (TnefPropertyId.ContentLength, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_CONTENT_RETURN_REQUESTED.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTENT_RETURN_REQUESTED.
		/// </remarks>
		public static readonly TnefPropertyTag ContentReturnRequested = new TnefPropertyTag (TnefPropertyId.ContentReturnRequested, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_CONTENTS_SORT_ORDER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTENTS_SORT_ORDER.
		/// </remarks>
		public static readonly TnefPropertyTag ContentsSortOrder = new TnefPropertyTag (TnefPropertyId.ContentsSortOrder, TnefPropertyType.Long, true);

		/// <summary>
		/// The MAPI property PR_CONTENT_UNREAD.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTENT_UNREAD.
		/// </remarks>
		public static readonly TnefPropertyTag ContentUnread = new TnefPropertyTag (TnefPropertyId.ContentUnread, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_CONTROL_FLAGS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTROL_FLAGS.
		/// </remarks>
		public static readonly TnefPropertyTag ControlFlags = new TnefPropertyTag (TnefPropertyId.ControlFlags, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_CONTROL_ID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTROL_ID.
		/// </remarks>
		public static readonly TnefPropertyTag ControlId = new TnefPropertyTag (TnefPropertyId.ControlId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_CONTROL_STRUCTURE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTROL_STRUCTURE.
		/// </remarks>
		public static readonly TnefPropertyTag ControlStructure = new TnefPropertyTag (TnefPropertyId.ControlStructure, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_CONTROL_TYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONTROL_TYPE.
		/// </remarks>
		public static readonly TnefPropertyTag ControlType = new TnefPropertyTag (TnefPropertyId.ControlType, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_CONVERSATION_INDEX.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONVERSATION_INDEX.
		/// </remarks>
		public static readonly TnefPropertyTag ConversationIndex = new TnefPropertyTag (TnefPropertyId.ConversationIndex, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_CONVERSATION_KEY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONVERSATION_KEY.
		/// </remarks>
		public static readonly TnefPropertyTag ConversationKey = new TnefPropertyTag (TnefPropertyId.ConversationKey, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_CONVERSATION_TOPIC.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONVERSATION_TOPIC.
		/// </remarks>
		public static readonly TnefPropertyTag ConversationTopicA = new TnefPropertyTag (TnefPropertyId.ConversationTopic, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_CONVERSATION_TOPIC.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONVERSATION_TOPIC.
		/// </remarks>
		public static readonly TnefPropertyTag ConversationTopicW = new TnefPropertyTag (TnefPropertyId.ConversationTopic, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_CONVERSION_EITS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONVERSION_EITS.
		/// </remarks>
		public static readonly TnefPropertyTag ConversionEits = new TnefPropertyTag (TnefPropertyId.ConversionEits, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_CONVERSION_PROHIBITED.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONVERSION_PROHIBITED.
		/// </remarks>
		public static readonly TnefPropertyTag ConversionProhibited = new TnefPropertyTag (TnefPropertyId.ConversionProhibited, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_CONVERSION_WITH_LOSS_PROHIBITED.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONVERSION_WITH_LOSS_PROHIBITED.
		/// </remarks>
		public static readonly TnefPropertyTag ConversionWithLossProhibited = new TnefPropertyTag (TnefPropertyId.ConversionWithLossProhibited, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_CONVERTED_EITS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CONVERTED_EITS.
		/// </remarks>
		public static readonly TnefPropertyTag ConvertedEits = new TnefPropertyTag (TnefPropertyId.ConvertedEits, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_CORRELATE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CORRELATE.
		/// </remarks>
		public static readonly TnefPropertyTag Correlate = new TnefPropertyTag (TnefPropertyId.Correlate, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_CORRELATE_MTSID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CORRELATE_MTSID.
		/// </remarks>
		public static readonly TnefPropertyTag CorrelateMtsid = new TnefPropertyTag (TnefPropertyId.CorrelateMtsid, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_COUNTRY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_COUNTRY.
		/// </remarks>
		public static readonly TnefPropertyTag CountryA = new TnefPropertyTag (TnefPropertyId.Country, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_COUNTRY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_COUNTRY.
		/// </remarks>
		public static readonly TnefPropertyTag CountryW = new TnefPropertyTag (TnefPropertyId.Country, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_CREATE_TEMPLATES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CREATE_TEMPLATES.
		/// </remarks>
		public static readonly TnefPropertyTag CreateTemplates = new TnefPropertyTag (TnefPropertyId.CreateTemplates, TnefPropertyType.Object);

		/// <summary>
		/// The MAPI property PR_CREATION_TIME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CREATION_TIME.
		/// </remarks>
		public static readonly TnefPropertyTag CreationTime = new TnefPropertyTag (TnefPropertyId.CreationTime, TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PR_CREATION_VERSION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CREATION_VERSION.
		/// </remarks>
		public static readonly TnefPropertyTag CreationVersion = new TnefPropertyTag (TnefPropertyId.CreationVersion, TnefPropertyType.I8);

		/// <summary>
		/// The MAPI property PR_CURRENT_VERSION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CURRENT_VERSION.
		/// </remarks>
		public static readonly TnefPropertyTag CurrentVersion = new TnefPropertyTag (TnefPropertyId.CurrentVersion, TnefPropertyType.I8);

		/// <summary>
		/// The MAPI property PR_CUSTOMER_ID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CUSTOMER_ID.
		/// </remarks>
		public static readonly TnefPropertyTag CustomerIdA = new TnefPropertyTag (TnefPropertyId.CustomerId, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_CUSTOMER_ID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_CUSTOMER_ID.
		/// </remarks>
		public static readonly TnefPropertyTag CustomerIdW = new TnefPropertyTag (TnefPropertyId.CustomerId, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_DEFAULT_PROFILE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DEFAULT_PROFILE.
		/// </remarks>
		public static readonly TnefPropertyTag DefaultProfile = new TnefPropertyTag (TnefPropertyId.DefaultProfile, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_DEFAULT_STORE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DEFAULT_STORE.
		/// </remarks>
		public static readonly TnefPropertyTag DefaultStore = new TnefPropertyTag (TnefPropertyId.DefaultStore, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_DEFAULT_VIEW_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DEFAULT_VIEW_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag DefaultViewEntryId = new TnefPropertyTag (TnefPropertyId.DefaultViewEntryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_DEF_CREATE_DL.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DEF_CREATE_DL.
		/// </remarks>
		public static readonly TnefPropertyTag DefCreateDl = new TnefPropertyTag (TnefPropertyId.DefCreateDl, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_DEF_CREATE_MAILUSER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DEF_CREATE_MAILUSER.
		/// </remarks>
		public static readonly TnefPropertyTag DefCreateMailuser = new TnefPropertyTag (TnefPropertyId.DefCreateMailuser, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_DEFERRED_DELIVERY_TIME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DEFERRED_DELIVERY_TIME.
		/// </remarks>
		public static readonly TnefPropertyTag DeferredDeliveryTime = new TnefPropertyTag (TnefPropertyId.DeferredDeliveryTime, TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PR_DELEGATION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DELEGATION.
		/// </remarks>
		public static readonly TnefPropertyTag Delegation = new TnefPropertyTag (TnefPropertyId.Delegation, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_DELETE_AFTER_SUBMIT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DELETE_AFTER_SUBMIT.
		/// </remarks>
		public static readonly TnefPropertyTag DeleteAfterSubmit = new TnefPropertyTag (TnefPropertyId.DeleteAfterSubmit, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_DELIVER_TIME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DELIVER_TIME.
		/// </remarks>
		public static readonly TnefPropertyTag DeliverTime = new TnefPropertyTag (TnefPropertyId.DeliverTime, TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PR_DELIVERY_POINT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DELIVERY_POINT.
		/// </remarks>
		public static readonly TnefPropertyTag DeliveryPoint = new TnefPropertyTag (TnefPropertyId.DeliveryPoint, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_DELTAX.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DELTAX.
		/// </remarks>
		public static readonly TnefPropertyTag Deltax = new TnefPropertyTag (TnefPropertyId.Deltax, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_DELTAY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DELTAY.
		/// </remarks>
		public static readonly TnefPropertyTag Deltay = new TnefPropertyTag (TnefPropertyId.Deltay, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_DEPARTMENT_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DEPARTMENT_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag DepartmentNameA = new TnefPropertyTag (TnefPropertyId.DepartmentName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_DEPARTMENT_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DEPARTMENT_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag DepartmentNameW = new TnefPropertyTag (TnefPropertyId.DepartmentName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_DEPTH.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DEPTH.
		/// </remarks>
		public static readonly TnefPropertyTag Depth = new TnefPropertyTag (TnefPropertyId.Depth, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_DETAILS_TABLE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DETAILS_TABLE.
		/// </remarks>
		public static readonly TnefPropertyTag DetailsTable = new TnefPropertyTag (TnefPropertyId.DetailsTable, TnefPropertyType.Object);

		/// <summary>
		/// The MAPI property PR_DISCARD_REASON.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DISCARD_REASON.
		/// </remarks>
		public static readonly TnefPropertyTag DiscardReason = new TnefPropertyTag (TnefPropertyId.DiscardReason, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_DISCLOSE_RECIPIENTS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DISCLOSE_RECIPIENTS.
		/// </remarks>
		public static readonly TnefPropertyTag DiscloseRecipients = new TnefPropertyTag (TnefPropertyId.DiscloseRecipients, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_DISCLOSURE_OF_RECIPIENTS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DISCLOSURE_OF_RECIPIENTS.
		/// </remarks>
		public static readonly TnefPropertyTag DisclosureOfRecipients = new TnefPropertyTag (TnefPropertyId.DisclosureOfRecipients, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_DISCRETE_VALUES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DISCRETE_VALUES.
		/// </remarks>
		public static readonly TnefPropertyTag DiscreteValues = new TnefPropertyTag (TnefPropertyId.DiscreteValues, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_DISC_VAL.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DISC_VAL.
		/// </remarks>
		public static readonly TnefPropertyTag DiscVal = new TnefPropertyTag (TnefPropertyId.DiscVal, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_DISPLAY_BCC.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DISPLAY_BCC.
		/// </remarks>
		public static readonly TnefPropertyTag DisplayBccA = new TnefPropertyTag (TnefPropertyId.DisplayBcc, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_DISPLAY_BCC.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DISPLAY_BCC.
		/// </remarks>
		public static readonly TnefPropertyTag DisplayBccW = new TnefPropertyTag (TnefPropertyId.DisplayBcc, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_DISPLAY_CC.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DISPLAY_CC.
		/// </remarks>
		public static readonly TnefPropertyTag DisplayCcA = new TnefPropertyTag (TnefPropertyId.DisplayCc, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_DISPLAY_CC.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DISPLAY_CC.
		/// </remarks>
		public static readonly TnefPropertyTag DisplayCcW = new TnefPropertyTag (TnefPropertyId.DisplayCc, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_DISPLAY_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DISPLAY_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag DisplayNameA = new TnefPropertyTag (TnefPropertyId.DisplayName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_DISPLAY_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DISPLAY_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag DisplayNameW = new TnefPropertyTag (TnefPropertyId.DisplayName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_DISPLAY_NAME_PREFIX.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DISPLAY_NAME_PREFIX.
		/// </remarks>
		public static readonly TnefPropertyTag DisplayNamePrefixA = new TnefPropertyTag (TnefPropertyId.DisplayNamePrefix, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_DISPLAY_NAME_PREFIX.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DISPLAY_NAME_PREFIX.
		/// </remarks>
		public static readonly TnefPropertyTag DisplayNamePrefixW = new TnefPropertyTag (TnefPropertyId.DisplayNamePrefix, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_DISPLAY_TO.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DISPLAY_TO.
		/// </remarks>
		public static readonly TnefPropertyTag DisplayToA = new TnefPropertyTag (TnefPropertyId.DisplayTo, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_DISPLAY_TO.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DISPLAY_TO.
		/// </remarks>
		public static readonly TnefPropertyTag DisplayToW = new TnefPropertyTag (TnefPropertyId.DisplayTo, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_DISPLAY_TYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DISPLAY_TYPE.
		/// </remarks>
		public static readonly TnefPropertyTag DisplayType = new TnefPropertyTag (TnefPropertyId.DisplayType, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_DL_EXPANSION_HISTORY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DL_EXPANSION_HISTORY.
		/// </remarks>
		public static readonly TnefPropertyTag DlExpansionHistory = new TnefPropertyTag (TnefPropertyId.DlExpansionHistory, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_DL_EXPANSION_PROHIBITED.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_DL_EXPANSION_PROHIBITED.
		/// </remarks>
		public static readonly TnefPropertyTag DlExpansionProhibited = new TnefPropertyTag (TnefPropertyId.DlExpansionProhibited, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_EMAIL_ADDRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_EMAIL_ADDRESS.
		/// </remarks>
		public static readonly TnefPropertyTag EmailAddressA = new TnefPropertyTag (TnefPropertyId.EmailAddress, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_EMAIL_ADDRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_EMAIL_ADDRESS.
		/// </remarks>
		public static readonly TnefPropertyTag EmailAddressW = new TnefPropertyTag (TnefPropertyId.EmailAddress, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_END_DATE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_END_DATE.
		/// </remarks>
		public static readonly TnefPropertyTag EndDate = new TnefPropertyTag (TnefPropertyId.EndDate, TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PR_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag EntryId = new TnefPropertyTag (TnefPropertyId.EntryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_EXPAND_BEGIN_TIME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_EXPAND_BEGIN_TIME.
		/// </remarks>
		public static readonly TnefPropertyTag ExpandBeginTime = new TnefPropertyTag (TnefPropertyId.ExpandBeginTime, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_EXPANDED_BEGIN_TIME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_EXPANDED_BEGIN_TIME.
		/// </remarks>
		public static readonly TnefPropertyTag ExpandedBeginTime = new TnefPropertyTag (TnefPropertyId.ExpandedBeginTime, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_EXPANDED_END_TIME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_EXPANDED_END_TIME.
		/// </remarks>
		public static readonly TnefPropertyTag ExpandedEndTime = new TnefPropertyTag (TnefPropertyId.ExpandedEndTime, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_EXPAND_END_TIME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_EXPAND_END_TIME.
		/// </remarks>
		public static readonly TnefPropertyTag ExpandEndTime = new TnefPropertyTag (TnefPropertyId.ExpandEndTime, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_EXPIRY_TIME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_EXPIRY_TIME.
		/// </remarks>
		public static readonly TnefPropertyTag ExpiryTime = new TnefPropertyTag (TnefPropertyId.ExpiryTime, TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PR_EXPLICIT_CONVERSION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_EXPLICIT_CONVERSION.
		/// </remarks>
		public static readonly TnefPropertyTag ExplicitConversion = new TnefPropertyTag (TnefPropertyId.ExplicitConversion, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_FILTERING_HOOKS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_FILTERING_HOOKS.
		/// </remarks>
		public static readonly TnefPropertyTag FilteringHooks = new TnefPropertyTag (TnefPropertyId.FilteringHooks, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_FINDER_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_FINDER_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag FinderEntryId = new TnefPropertyTag (TnefPropertyId.FinderEntryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_FOLDER_ASSOCIATED_CONTENTS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_FOLDER_ASSOCIATED_CONTENTS.
		/// </remarks>
		public static readonly TnefPropertyTag FolderAssociatedContents = new TnefPropertyTag (TnefPropertyId.FolderAssociatedContents, TnefPropertyType.Object);

		/// <summary>
		/// The MAPI property PR_FOLDER_TYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_FOLDER_TYPE.
		/// </remarks>
		public static readonly TnefPropertyTag FolderType = new TnefPropertyTag (TnefPropertyId.FolderType, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_FORM_CATEGORY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_FORM_CATEGORY.
		/// </remarks>
		public static readonly TnefPropertyTag FormCategoryA = new TnefPropertyTag (TnefPropertyId.FormCategory, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_FORM_CATEGORY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_FORM_CATEGORY.
		/// </remarks>
		public static readonly TnefPropertyTag FormCategoryW = new TnefPropertyTag (TnefPropertyId.FormCategory, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_FORM_CATEGORY_SUB.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_FORM_CATEGORY_SUB.
		/// </remarks>
		public static readonly TnefPropertyTag FormCategorySubA = new TnefPropertyTag (TnefPropertyId.FormCategorySub, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_FORM_CATEGORY_SUB.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_FORM_CATEGORY_SUB.
		/// </remarks>
		public static readonly TnefPropertyTag FormCategorySubW = new TnefPropertyTag (TnefPropertyId.FormCategorySub, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_FORM_CLSID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_FORM_CLSID.
		/// </remarks>
		public static readonly TnefPropertyTag FormClsid = new TnefPropertyTag (TnefPropertyId.FormClsid, TnefPropertyType.ClassId);

		/// <summary>
		/// The MAPI property PR_FORM_CONTACT_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_FORM_CONTACT_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag FormContactNameA = new TnefPropertyTag (TnefPropertyId.FormContactName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_FORM_CONTACT_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_FORM_CONTACT_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag FormContactNameW = new TnefPropertyTag (TnefPropertyId.FormContactName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_FORM_DESIGNER_GUID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_FORM_DESIGNER_GUID.
		/// </remarks>
		public static readonly TnefPropertyTag FormDesignerGuid = new TnefPropertyTag (TnefPropertyId.FormDesignerGuid, TnefPropertyType.ClassId);

		/// <summary>
		/// The MAPI property PR_FORM_DESIGNER_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_FORM_DESIGNER_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag FormDesignerNameA = new TnefPropertyTag (TnefPropertyId.FormDesignerName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_FORM_DESIGNER_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_FORM_DESIGNER_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag FormDesignerNameW = new TnefPropertyTag (TnefPropertyId.FormDesignerName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_FORM_HIDDEN.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_FORM_HIDDEN.
		/// </remarks>
		public static readonly TnefPropertyTag FormHidden = new TnefPropertyTag (TnefPropertyId.FormHidden, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_FORM_HOST_MAP.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_FORM_HOST_MAP.
		/// </remarks>
		public static readonly TnefPropertyTag FormHostMap = new TnefPropertyTag (TnefPropertyId.FormHostMap, TnefPropertyType.Long, true);

		/// <summary>
		/// The MAPI property PR_FORM_MESSAGE_BEHAVIOR.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_FORM_MESSAGE_BEHAVIOR.
		/// </remarks>
		public static readonly TnefPropertyTag FormMessageBehavior = new TnefPropertyTag (TnefPropertyId.FormMessageBehavior, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_FORM_VERSION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_FORM_VERSION.
		/// </remarks>
		public static readonly TnefPropertyTag FormVersionA = new TnefPropertyTag (TnefPropertyId.FormVersion, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_FORM_VERSION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_FORM_VERSION.
		/// </remarks>
		public static readonly TnefPropertyTag FormVersionW = new TnefPropertyTag (TnefPropertyId.FormVersion, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_FTP_SITE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_FTP_SITE.
		/// </remarks>
		public static readonly TnefPropertyTag FtpSiteA = new TnefPropertyTag (TnefPropertyId.FtpSite, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_FTP_SITE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_FTP_SITE.
		/// </remarks>
		public static readonly TnefPropertyTag FtpSiteW = new TnefPropertyTag (TnefPropertyId.FtpSite, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_GENDER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_GENDER.
		/// </remarks>
		public static readonly TnefPropertyTag Gender = new TnefPropertyTag (TnefPropertyId.Gender, TnefPropertyType.I2);

		/// <summary>
		/// The MAPI property PR_GENERATION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_GENERATION.
		/// </remarks>
		public static readonly TnefPropertyTag GenerationA = new TnefPropertyTag (TnefPropertyId.Generation, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_GENERATION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_GENERATION.
		/// </remarks>
		public static readonly TnefPropertyTag GenerationW = new TnefPropertyTag (TnefPropertyId.Generation, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_GIVEN_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_GIVEN_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag GivenNameA = new TnefPropertyTag (TnefPropertyId.GivenName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_GIVEN_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_GIVEN_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag GivenNameW = new TnefPropertyTag (TnefPropertyId.GivenName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_GOVERNMENT_ID_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_GOVERNMENT_ID_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag GovernmentIdNumberA = new TnefPropertyTag (TnefPropertyId.GovernmentIdNumber, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_GOVERNMENT_ID_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_GOVERNMENT_ID_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag GovernmentIdNumberW = new TnefPropertyTag (TnefPropertyId.GovernmentIdNumber, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_HASATTACH.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_HASATTACH.
		/// </remarks>
		public static readonly TnefPropertyTag Hasattach = new TnefPropertyTag (TnefPropertyId.Hasattach, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_HEADER_FOLDER_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_HEADER_FOLDER_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag HeaderFolderEntryId = new TnefPropertyTag (TnefPropertyId.HeaderFolderEntryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_HOBBIES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_HOBBIES.
		/// </remarks>
		public static readonly TnefPropertyTag HobbiesA = new TnefPropertyTag (TnefPropertyId.Hobbies, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_HOBBIES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_HOBBIES.
		/// </remarks>
		public static readonly TnefPropertyTag HobbiesW = new TnefPropertyTag (TnefPropertyId.Hobbies, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_HOME2_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_HOME2_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag Home2TelephoneNumberA = new TnefPropertyTag (TnefPropertyId.Home2TelephoneNumber, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_HOME2_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_HOME2_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag Home2TelephoneNumberAMv = new TnefPropertyTag (TnefPropertyId.Home2TelephoneNumber, TnefPropertyType.String8, true);

		/// <summary>
		/// The MAPI property PR_HOME2_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_HOME2_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag Home2TelephoneNumberW = new TnefPropertyTag (TnefPropertyId.Home2TelephoneNumber, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_HOME2_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_HOME2_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag Home2TelephoneNumberWMv = new TnefPropertyTag (TnefPropertyId.Home2TelephoneNumber, TnefPropertyType.Unicode, true);

		/// <summary>
		/// The MAPI property PR_HOME_ADDRESS_CITY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_HOME_ADDRESS_CITY.
		/// </remarks>
		public static readonly TnefPropertyTag HomeAddressCityA = new TnefPropertyTag (TnefPropertyId.HomeAddressCity, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_HOME_ADDRESS_CITY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_HOME_ADDRESS_CITY.
		/// </remarks>
		public static readonly TnefPropertyTag HomeAddressCityW = new TnefPropertyTag (TnefPropertyId.HomeAddressCity, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_HOME_ADDRESS_COUNTRY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_HOME_ADDRESS_COUNTRY.
		/// </remarks>
		public static readonly TnefPropertyTag HomeAddressCountryA = new TnefPropertyTag (TnefPropertyId.HomeAddressCountry, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_HOME_ADDRESS_COUNTRY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_HOME_ADDRESS_COUNTRY.
		/// </remarks>
		public static readonly TnefPropertyTag HomeAddressCountryW = new TnefPropertyTag (TnefPropertyId.HomeAddressCountry, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_HOME_ADDRESS_POSTAL_CODE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_HOME_ADDRESS_POSTAL_CODE.
		/// </remarks>
		public static readonly TnefPropertyTag HomeAddressPostalCodeA = new TnefPropertyTag (TnefPropertyId.HomeAddressPostalCode, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_HOME_ADDRESS_POSTAL_CODE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_HOME_ADDRESS_POSTAL_CODE.
		/// </remarks>
		public static readonly TnefPropertyTag HomeAddressPostalCodeW = new TnefPropertyTag (TnefPropertyId.HomeAddressPostalCode, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_HOME_ADDRESS_POST_OFFICE_BOX.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_HOME_ADDRESS_POST_OFFICE_BOX.
		/// </remarks>
		public static readonly TnefPropertyTag HomeAddressPostOfficeBoxA = new TnefPropertyTag (TnefPropertyId.HomeAddressPostOfficeBox, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_HOME_ADDRESS_POST_OFFICE_BOX.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_HOME_ADDRESS_POST_OFFICE_BOX.
		/// </remarks>
		public static readonly TnefPropertyTag HomeAddressPostOfficeBoxW = new TnefPropertyTag (TnefPropertyId.HomeAddressPostOfficeBox, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_HOME_ADDRESS_STATE_OR_PROVINCE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_HOME_ADDRESS_STATE_OR_PROVINCE.
		/// </remarks>
		public static readonly TnefPropertyTag HomeAddressStateOrProvinceA = new TnefPropertyTag (TnefPropertyId.HomeAddressStateOrProvince, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_HOME_ADDRESS_STATE_OR_PROVINCE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_HOME_ADDRESS_STATE_OR_PROVINCE.
		/// </remarks>
		public static readonly TnefPropertyTag HomeAddressStateOrProvinceW = new TnefPropertyTag (TnefPropertyId.HomeAddressStateOrProvince, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_HOME_ADDRESS_STREET.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_HOME_ADDRESS_STREET.
		/// </remarks>
		public static readonly TnefPropertyTag HomeAddressStreetA = new TnefPropertyTag (TnefPropertyId.HomeAddressStreet, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_HOME_ADDRESS_STREET.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_HOME_ADDRESS_STREET.
		/// </remarks>
		public static readonly TnefPropertyTag HomeAddressStreetW = new TnefPropertyTag (TnefPropertyId.HomeAddressStreet, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_HOME_FAX_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_HOME_FAX_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag HomeFaxNumberA = new TnefPropertyTag (TnefPropertyId.HomeFaxNumber, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_HOME_FAX_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_HOME_FAX_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag HomeFaxNumberW = new TnefPropertyTag (TnefPropertyId.HomeFaxNumber, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_HOME_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_HOME_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag HomeTelephoneNumberA = new TnefPropertyTag (TnefPropertyId.HomeTelephoneNumber, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_HOME_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_HOME_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag HomeTelephoneNumberW = new TnefPropertyTag (TnefPropertyId.HomeTelephoneNumber, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ICON.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ICON.
		/// </remarks>
		public static readonly TnefPropertyTag Icon = new TnefPropertyTag (TnefPropertyId.Icon, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_IDENTITY_DISPLAY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_IDENTITY_DISPLAY.
		/// </remarks>
		public static readonly TnefPropertyTag IdentityDisplayA = new TnefPropertyTag (TnefPropertyId.IdentityDisplay, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_IDENTITY_DISPLAY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_IDENTITY_DISPLAY.
		/// </remarks>
		public static readonly TnefPropertyTag IdentityDisplayW = new TnefPropertyTag (TnefPropertyId.IdentityDisplay, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_IDENTITY_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_IDENTITY_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag IdentityEntryId = new TnefPropertyTag (TnefPropertyId.IdentityEntryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_IDENTITY_SEARCH_KEY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_IDENTITY_SEARCH_KEY.
		/// </remarks>
		public static readonly TnefPropertyTag IdentitySearchKey = new TnefPropertyTag (TnefPropertyId.IdentitySearchKey, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_IMPLICIT_CONVERSION_PROHIBITED.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_IMPLICIT_CONVERSION_PROHIBITED.
		/// </remarks>
		public static readonly TnefPropertyTag ImplicitConversionProhibited = new TnefPropertyTag (TnefPropertyId.ImplicitConversionProhibited, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_IMPORTANCE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_IMPORTANCE.
		/// </remarks>
		public static readonly TnefPropertyTag Importance = new TnefPropertyTag (TnefPropertyId.Importance, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_INCOMPLETE_COPY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INCOMPLETE_COPY.
		/// </remarks>
		public static readonly TnefPropertyTag IncompleteCopy = new TnefPropertyTag (TnefPropertyId.IncompleteCopy, TnefPropertyType.Boolean);

		/// <summary>
		/// The Internet mail override charset.
		/// </summary>
		/// <remarks>
		/// The Internet mail override charset.
		/// </remarks>
		public static readonly TnefPropertyTag INetMailOverrideCharset = new TnefPropertyTag (TnefPropertyId.INetMailOverrideCharset, TnefPropertyType.Unspecified);

		/// <summary>
		/// The Internet mail override format.
		/// </summary>
		/// <remarks>
		/// The Internet mail override format.
		/// </remarks>
		public static readonly TnefPropertyTag INetMailOverrideFormat = new TnefPropertyTag (TnefPropertyId.INetMailOverrideFormat, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_INITIAL_DETAILS_PANE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INITIAL_DETAILS_PANE.
		/// </remarks>
		public static readonly TnefPropertyTag InitialDetailsPane = new TnefPropertyTag (TnefPropertyId.InitialDetailsPane, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_INITIALS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INITIALS.
		/// </remarks>
		public static readonly TnefPropertyTag InitialsA = new TnefPropertyTag (TnefPropertyId.Initials, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_INITIALS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INITIALS.
		/// </remarks>
		public static readonly TnefPropertyTag InitialsW = new TnefPropertyTag (TnefPropertyId.Initials, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_IN_REPLY_TO_ID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_IN_REPLY_TO_ID.
		/// </remarks>
		public static readonly TnefPropertyTag InReplyToIdA = new TnefPropertyTag (TnefPropertyId.InReplyToId, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_IN_REPLY_TO_ID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_IN_REPLY_TO_ID.
		/// </remarks>
		public static readonly TnefPropertyTag InReplyToIdW = new TnefPropertyTag (TnefPropertyId.InReplyToId, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_INSTANCE_KEY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INSTANCE_KEY.
		/// </remarks>
		public static readonly TnefPropertyTag InstanceKey = new TnefPropertyTag (TnefPropertyId.InstanceKey, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_INTERNET_APPROVED.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INTERNET_APPROVED.
		/// </remarks>
		public static readonly TnefPropertyTag InternetApprovedA = new TnefPropertyTag (TnefPropertyId.InternetApproved, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_INTERNET_APPROVED.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INTERNET_APPROVED.
		/// </remarks>
		public static readonly TnefPropertyTag InternetApprovedW = new TnefPropertyTag (TnefPropertyId.InternetApproved, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_INTERNET_ARTICLE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INTERNET_ARTICLE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag InternetArticleNumber = new TnefPropertyTag (TnefPropertyId.InternetArticleNumber, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_INTERNET_CONTROL.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INTERNET_CONTROL.
		/// </remarks>
		public static readonly TnefPropertyTag InternetControlA = new TnefPropertyTag (TnefPropertyId.InternetControl, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_INTERNET_CONTROL.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INTERNET_CONTROL.
		/// </remarks>
		public static readonly TnefPropertyTag InternetControlW = new TnefPropertyTag (TnefPropertyId.InternetControl, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_INTERNET_CPID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INTERNET_CPID.
		/// </remarks>
		public static readonly TnefPropertyTag InternetCPID = new TnefPropertyTag (TnefPropertyId.InternetCPID, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_INTERNET_DISTRIBUTION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INTERNET_DISTRIBUTION.
		/// </remarks>
		public static readonly TnefPropertyTag InternetDistributionA = new TnefPropertyTag (TnefPropertyId.InternetDistribution, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_INTERNET_DISTRIBUTION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INTERNET_DISTRIBUTION.
		/// </remarks>
		public static readonly TnefPropertyTag InternetDistributionW = new TnefPropertyTag (TnefPropertyId.InternetDistribution, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_INTERNET_FOLLOWUP_TO.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INTERNET_FOLLOWUP_TO.
		/// </remarks>
		public static readonly TnefPropertyTag InternetFollowupToA = new TnefPropertyTag (TnefPropertyId.InternetFollowupTo, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_INTERNET_FOLLOWUP_TO.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INTERNET_FOLLOWUP_TO.
		/// </remarks>
		public static readonly TnefPropertyTag InternetFollowupToW = new TnefPropertyTag (TnefPropertyId.InternetFollowupTo, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_INTERNET_LINES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INTERNET_LINES.
		/// </remarks>
		public static readonly TnefPropertyTag InternetLines = new TnefPropertyTag (TnefPropertyId.InternetLines, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_INTERNET_MESSAGE_ID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INTERNET_MESSAGE_ID.
		/// </remarks>
		public static readonly TnefPropertyTag InternetMessageIdA = new TnefPropertyTag (TnefPropertyId.InternetMessageId, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_INTERNET_MESSAGE_ID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INTERNET_MESSAGE_ID.
		/// </remarks>
		public static readonly TnefPropertyTag InternetMessageIdW = new TnefPropertyTag (TnefPropertyId.InternetMessageId, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_INTERNET_NEWSGROUPS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INTERNET_NEWSGROUPS.
		/// </remarks>
		public static readonly TnefPropertyTag InternetNewsgroupsA = new TnefPropertyTag (TnefPropertyId.InternetNewsgroups, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_INTERNET_NEWSGROUPS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INTERNET_NEWSGROUPS.
		/// </remarks>
		public static readonly TnefPropertyTag InternetNewsgroupsW = new TnefPropertyTag (TnefPropertyId.InternetNewsgroups, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_INTERNET_NNTP_PATH.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INTERNET_NNTP_PATH.
		/// </remarks>
		public static readonly TnefPropertyTag InternetNntpPathA = new TnefPropertyTag (TnefPropertyId.InternetNntpPath, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_INTERNET_NNTP_PATH.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INTERNET_NNTP_PATH.
		/// </remarks>
		public static readonly TnefPropertyTag InternetNntpPathW = new TnefPropertyTag (TnefPropertyId.InternetNntpPath, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_INTERNET_ORGANIZATION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INTERNET_ORGANIZATION.
		/// </remarks>
		public static readonly TnefPropertyTag InternetOrganizationA = new TnefPropertyTag (TnefPropertyId.InternetOrganization, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_INTERNET_ORGANIZATION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INTERNET_ORGANIZATION.
		/// </remarks>
		public static readonly TnefPropertyTag InternetOrganizationW = new TnefPropertyTag (TnefPropertyId.InternetOrganization, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_INTERNET_PRECEDENCE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INTERNET_PRECEDENCE.
		/// </remarks>
		public static readonly TnefPropertyTag InternetPrecedenceA = new TnefPropertyTag (TnefPropertyId.InternetPrecedence, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_INTERNET_PRECEDENCE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INTERNET_PRECEDENCE.
		/// </remarks>
		public static readonly TnefPropertyTag InternetPrecedenceW = new TnefPropertyTag (TnefPropertyId.InternetPrecedence, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_INTERNET_REFERENCES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INTERNET_REFERENCES.
		/// </remarks>
		public static readonly TnefPropertyTag InternetReferencesA = new TnefPropertyTag (TnefPropertyId.InternetReferences, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_INTERNET_REFERENCES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_INTERNET_REFERENCES.
		/// </remarks>
		public static readonly TnefPropertyTag InternetReferencesW = new TnefPropertyTag (TnefPropertyId.InternetReferences, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_IPM_ID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_IPM_ID.
		/// </remarks>
		public static readonly TnefPropertyTag IpmId = new TnefPropertyTag (TnefPropertyId.IpmId, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_IPM_OUTBOX_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_IPM_OUTBOX_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag IpmOutboxEntryId = new TnefPropertyTag (TnefPropertyId.IpmOutboxEntryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_IPM_OUTBOX_SEARCH_KEY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_IPM_OUTBOX_SEARCH_KEY.
		/// </remarks>
		public static readonly TnefPropertyTag IpmOutboxSearchKey = new TnefPropertyTag (TnefPropertyId.IpmOutboxSearchKey, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_IPM_RETURN_REQUESTED.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_IPM_RETURN_REQUESTED.
		/// </remarks>
		public static readonly TnefPropertyTag IpmReturnRequested = new TnefPropertyTag (TnefPropertyId.IpmReturnRequested, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_IPM_SENTMAIL_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_IPM_SENTMAIL_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag IpmSentmailEntryId = new TnefPropertyTag (TnefPropertyId.IpmSentmailEntryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_IPM_SENTMAIL_SEARCH_KEY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_IPM_SENTMAIL_SEARCH_KEY.
		/// </remarks>
		public static readonly TnefPropertyTag IpmSentmailSearchKey = new TnefPropertyTag (TnefPropertyId.IpmSentmailSearchKey, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_IPM_SUBTREE_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_IPM_SUBTREE_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag IpmSubtreeEntryId = new TnefPropertyTag (TnefPropertyId.IpmSubtreeEntryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_IPM_SUBTREE_SEARCH_KEY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_IPM_SUBTREE_SEARCH_KEY.
		/// </remarks>
		public static readonly TnefPropertyTag IpmSubtreeSearchKey = new TnefPropertyTag (TnefPropertyId.IpmSubtreeSearchKey, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_IPM_WASTEBASKET_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_IPM_WASTEBASKET_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag IpmWastebasketEntryId = new TnefPropertyTag (TnefPropertyId.IpmWastebasketEntryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_IPM_WASTEBASKET_SEARCH_KEY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_IPM_WASTEBASKET_SEARCH_KEY.
		/// </remarks>
		public static readonly TnefPropertyTag IpmWastebasketSearchKey = new TnefPropertyTag (TnefPropertyId.IpmWastebasketSearchKey, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_ISDN_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ISDN_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag IsdnNumberA = new TnefPropertyTag (TnefPropertyId.IsdnNumber, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ISDN_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ISDN_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag IsdnNumberW = new TnefPropertyTag (TnefPropertyId.IsdnNumber, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_KEYWORD.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_KEYWORD.
		/// </remarks>
		public static readonly TnefPropertyTag KeywordA = new TnefPropertyTag (TnefPropertyId.Keyword, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_KEYWORD.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_KEYWORD.
		/// </remarks>
		public static readonly TnefPropertyTag KeywordW = new TnefPropertyTag (TnefPropertyId.Keyword, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_LANGUAGE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LANGUAGE.
		/// </remarks>
		public static readonly TnefPropertyTag LanguageA = new TnefPropertyTag (TnefPropertyId.Language, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_LANGUAGE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LANGUAGE.
		/// </remarks>
		public static readonly TnefPropertyTag LanguageW = new TnefPropertyTag (TnefPropertyId.Language, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_LANGUAGES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LANGUAGES.
		/// </remarks>
		public static readonly TnefPropertyTag LanguagesA = new TnefPropertyTag (TnefPropertyId.Languages, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_LANGUAGES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LANGUAGES.
		/// </remarks>
		public static readonly TnefPropertyTag LanguagesW = new TnefPropertyTag (TnefPropertyId.Languages, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_LAST_MODIFICATION_TIME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LAST_MODIFICATION_TIME.
		/// </remarks>
		public static readonly TnefPropertyTag LastModificationTime = new TnefPropertyTag (TnefPropertyId.LastModificationTime, TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PR_LATEST_DELIVERY_TIME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LATEST_DELIVERY_TIME.
		/// </remarks>
		public static readonly TnefPropertyTag LatestDeliveryTime = new TnefPropertyTag (TnefPropertyId.LatestDeliveryTime, TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PR_LIST_HELP.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LIST_HELP.
		/// </remarks>
		public static readonly TnefPropertyTag ListHelpA = new TnefPropertyTag (TnefPropertyId.ListHelp, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_LIST_HELP.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LIST_HELP.
		/// </remarks>
		public static readonly TnefPropertyTag ListHelpW = new TnefPropertyTag (TnefPropertyId.ListHelp, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_LIST_SUBSCRIBE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LIST_SUBSCRIBE.
		/// </remarks>
		public static readonly TnefPropertyTag ListSubscribeA = new TnefPropertyTag (TnefPropertyId.ListSubscribe, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_LIST_SUBSCRIBE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LIST_SUBSCRIBE.
		/// </remarks>
		public static readonly TnefPropertyTag ListSubscribeW = new TnefPropertyTag (TnefPropertyId.ListSubscribe, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_LIST_UNSUBSCRIBE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LIST_UNSUBSCRIBE.
		/// </remarks>
		public static readonly TnefPropertyTag ListUnsubscribeA = new TnefPropertyTag (TnefPropertyId.ListUnsubscribe, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_LIST_UNSUBSCRIBE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LIST_UNSUBSCRIBE.
		/// </remarks>
		public static readonly TnefPropertyTag ListUnsubscribeW = new TnefPropertyTag (TnefPropertyId.ListUnsubscribe, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_LOCALITY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LOCALITY.
		/// </remarks>
		public static readonly TnefPropertyTag LocalityA = new TnefPropertyTag (TnefPropertyId.Locality, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_LOCALITY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LOCALITY.
		/// </remarks>
		public static readonly TnefPropertyTag LocalityW = new TnefPropertyTag (TnefPropertyId.Locality, TnefPropertyType.Unicode);

		//public static readonly TnefPropertyTag LocallyDelivered = new TnefPropertyTag (TnefPropertyId.LocallyDelivered, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_LOCATION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LOCATION.
		/// </remarks>
		public static readonly TnefPropertyTag LocationA = new TnefPropertyTag (TnefPropertyId.Location, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_LOCATION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LOCATION.
		/// </remarks>
		public static readonly TnefPropertyTag LocationW = new TnefPropertyTag (TnefPropertyId.Location, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_LOCK_BRANCH_ID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LOCK_BRANCH_ID.
		/// </remarks>
		public static readonly TnefPropertyTag LockBranchId = new TnefPropertyTag (TnefPropertyId.LockBranchId, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_LOCK_DEPTH.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LOCK_DEPTH.
		/// </remarks>
		public static readonly TnefPropertyTag LockDepth = new TnefPropertyTag (TnefPropertyId.LockDepth, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_LOCK_ENLISTMENT_CONTEXT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LOCK_ENLISTMENT_CONTEXT.
		/// </remarks>
		public static readonly TnefPropertyTag LockEnlistmentContext = new TnefPropertyTag (TnefPropertyId.LockEnlistmentContext, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_LOCK_EXPIRY_TIME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LOCK_EXPIRY_TIME.
		/// </remarks>
		public static readonly TnefPropertyTag LockExpiryTime = new TnefPropertyTag (TnefPropertyId.LockExpiryTime, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_LOCK_PERSISTENT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LOCK_PERSISTENT.
		/// </remarks>
		public static readonly TnefPropertyTag LockPersistent = new TnefPropertyTag (TnefPropertyId.LockPersistent, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_LOCK_RESOURCE_DID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LOCK_RESOURCE_DID.
		/// </remarks>
		public static readonly TnefPropertyTag LockResourceDid = new TnefPropertyTag (TnefPropertyId.LockResourceDid, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_LOCK_RESOURCE_FID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LOCK_RESOURCE_FID.
		/// </remarks>
		public static readonly TnefPropertyTag LockResourceFid = new TnefPropertyTag (TnefPropertyId.LockResourceFid, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_LOCK_RESOURCE_MID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LOCK_RESOURCE_MID.
		/// </remarks>
		public static readonly TnefPropertyTag LockResourceMid = new TnefPropertyTag (TnefPropertyId.LockResourceMid, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_LOCK_SCOPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LOCK_SCOPE.
		/// </remarks>
		public static readonly TnefPropertyTag LockScope = new TnefPropertyTag (TnefPropertyId.LockScope, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_LOCK_TIMEOUT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LOCK_TIMEOUT.
		/// </remarks>
		public static readonly TnefPropertyTag LockTimeout = new TnefPropertyTag (TnefPropertyId.LockTimeout, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_LOCK_TYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_LOCK_TYPE.
		/// </remarks>
		public static readonly TnefPropertyTag LockType = new TnefPropertyTag (TnefPropertyId.LockType, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_MAIL_PERMISSION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MAIL_PERMISSION.
		/// </remarks>
		public static readonly TnefPropertyTag MailPermission = new TnefPropertyTag (TnefPropertyId.MailPermission, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_MANAGER_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MANAGER_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag ManagerNameA = new TnefPropertyTag (TnefPropertyId.ManagerName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_MANAGER_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MANAGER_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag ManagerNameW = new TnefPropertyTag (TnefPropertyId.ManagerName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_MAPPING_SIGNATURE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MAPPING_SIGNATURE.
		/// </remarks>
		public static readonly TnefPropertyTag MappingSignature = new TnefPropertyTag (TnefPropertyId.MappingSignature, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_MDB_PROVIDER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MDB_PROVIDER.
		/// </remarks>
		public static readonly TnefPropertyTag MdbProvider = new TnefPropertyTag (TnefPropertyId.MdbProvider, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_MESSAGE_ATTACHMENTS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MESSAGE_ATTACHMENTS.
		/// </remarks>
		public static readonly TnefPropertyTag MessageAttachments = new TnefPropertyTag (TnefPropertyId.MessageAttachments, TnefPropertyType.Object);

		/// <summary>
		/// The MAPI property PR_MESSAGE_CC_ME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MESSAGE_CC_ME.
		/// </remarks>
		public static readonly TnefPropertyTag MessageCcMe = new TnefPropertyTag (TnefPropertyId.MessageCcMe, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_MESSAGE_CLASS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MESSAGE_CLASS.
		/// </remarks>
		public static readonly TnefPropertyTag MessageClassA = new TnefPropertyTag (TnefPropertyId.MessageClass, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_MESSAGE_CLASS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MESSAGE_CLASS.
		/// </remarks>
		public static readonly TnefPropertyTag MessageClassW = new TnefPropertyTag (TnefPropertyId.MessageClass, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_MESSAGE_CODEPAGE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MESSAGE_CODEPAGE.
		/// </remarks>
		public static readonly TnefPropertyTag MessageCodepage = new TnefPropertyTag (TnefPropertyId.MessageCodepage, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_MESSAGE_DELIVERY_ID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MESSAGE_DELIVERY_ID.
		/// </remarks>
		public static readonly TnefPropertyTag MessageDeliveryId = new TnefPropertyTag (TnefPropertyId.MessageDeliveryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_MESSAGE_DELIVERY_TIME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MESSAGE_DELIVERY_TIME.
		/// </remarks>
		public static readonly TnefPropertyTag MessageDeliveryTime = new TnefPropertyTag (TnefPropertyId.MessageDeliveryTime, TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PR_MESSAGE_DOWNLOAD_TIME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MESSAGE_DOWNLOAD_TIME.
		/// </remarks>
		public static readonly TnefPropertyTag MessageDownloadTime = new TnefPropertyTag (TnefPropertyId.MessageDownloadTime, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_MESSAGE_FLAGS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MESSAGE_FLAGS.
		/// </remarks>
		public static readonly TnefPropertyTag MessageFlags = new TnefPropertyTag (TnefPropertyId.MessageFlags, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_MESSAGE_RECIPIENTS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MESSAGE_RECIPIENTS.
		/// </remarks>
		public static readonly TnefPropertyTag MessageRecipients = new TnefPropertyTag (TnefPropertyId.MessageRecipients, TnefPropertyType.Object);

		/// <summary>
		/// The MAPI property PR_MESSAGE_RECIP_ME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MESSAGE_RECIP_ME.
		/// </remarks>
		public static readonly TnefPropertyTag MessageRecipMe = new TnefPropertyTag (TnefPropertyId.MessageRecipMe, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_MESSAGE_SECURITY_LABEL.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MESSAGE_SECURITY_LABEL.
		/// </remarks>
		public static readonly TnefPropertyTag MessageSecurityLabel = new TnefPropertyTag (TnefPropertyId.MessageSecurityLabel, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_MESSAGE_SIZE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MESSAGE_SIZE.
		/// </remarks>
		public static readonly TnefPropertyTag MessageSize = new TnefPropertyTag (TnefPropertyId.MessageSize, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_MESSAGE_SUBMISSION_ID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MESSAGE_SUBMISSION_ID.
		/// </remarks>
		public static readonly TnefPropertyTag MessageSubmissionId = new TnefPropertyTag (TnefPropertyId.MessageSubmissionId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_MESSAGE_TOKEN.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MESSAGE_TOKEN.
		/// </remarks>
		public static readonly TnefPropertyTag MessageToken = new TnefPropertyTag (TnefPropertyId.MessageToken, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_MESSAGE_TO_ME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MESSAGE_TO_ME.
		/// </remarks>
		public static readonly TnefPropertyTag MessageToMe = new TnefPropertyTag (TnefPropertyId.MessageToMe, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_MHS_COMMON_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MHS_COMMON_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag MhsCommonNameA = new TnefPropertyTag (TnefPropertyId.MhsCommonName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_MHS_COMMON_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MHS_COMMON_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag MhsCommonNameW = new TnefPropertyTag (TnefPropertyId.MhsCommonName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_MIDDLE_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MIDDLE_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag MiddleNameA = new TnefPropertyTag (TnefPropertyId.MiddleName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_MIDDLE_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MIDDLE_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag MiddleNameW = new TnefPropertyTag (TnefPropertyId.MiddleName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_MINI_ICON.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MINI_ICON.
		/// </remarks>
		public static readonly TnefPropertyTag MiniIcon = new TnefPropertyTag (TnefPropertyId.MiniIcon, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_MOBILE_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MOBILE_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag MobileTelephoneNumberA = new TnefPropertyTag (TnefPropertyId.MobileTelephoneNumber, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_MOBILE_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MOBILE_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag MobileTelephoneNumberW = new TnefPropertyTag (TnefPropertyId.MobileTelephoneNumber, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_MODIFY_VERSION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MODIFY_VERSION.
		/// </remarks>
		public static readonly TnefPropertyTag ModifyVersion = new TnefPropertyTag (TnefPropertyId.ModifyVersion, TnefPropertyType.I8);

		/// <summary>
		/// The MAPI property PR_MSG_STATUS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_MSG_STATUS.
		/// </remarks>
		public static readonly TnefPropertyTag MsgStatus = new TnefPropertyTag (TnefPropertyId.MsgStatus, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_NDR_DIAG_CODE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_NDR_DIAG_CODE.
		/// </remarks>
		public static readonly TnefPropertyTag NdrDiagCode = new TnefPropertyTag (TnefPropertyId.NdrDiagCode, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_NDR_REASON_CODE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_NDR_REASON_CODE.
		/// </remarks>
		public static readonly TnefPropertyTag NdrReasonCode = new TnefPropertyTag (TnefPropertyId.NdrReasonCode, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_NDR_STATUS_CODE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_NDR_STATUS_CODE.
		/// </remarks>
		public static readonly TnefPropertyTag NdrStatusCode = new TnefPropertyTag (TnefPropertyId.NdrStatusCode, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_NEWSGROUP_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_NEWSGROUP_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag NewsgroupNameA = new TnefPropertyTag (TnefPropertyId.NewsgroupName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_NEWSGROUP_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_NEWSGROUP_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag NewsgroupNameW = new TnefPropertyTag (TnefPropertyId.NewsgroupName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_NICKNAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_NICKNAME.
		/// </remarks>
		public static readonly TnefPropertyTag NicknameA = new TnefPropertyTag (TnefPropertyId.Nickname, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_NICKNAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_NICKNAME.
		/// </remarks>
		public static readonly TnefPropertyTag NicknameW = new TnefPropertyTag (TnefPropertyId.Nickname, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_NNTP_XREF.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_NNTP_XREF.
		/// </remarks>
		public static readonly TnefPropertyTag NntpXrefA = new TnefPropertyTag (TnefPropertyId.NntpXref, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_NNTP_XREF.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_NNTP_XREF.
		/// </remarks>
		public static readonly TnefPropertyTag NntpXrefW = new TnefPropertyTag (TnefPropertyId.NntpXref, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_NON_RECEIPT_NOTIFICATION_REQUESTED.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_NON_RECEIPT_NOTIFICATION_REQUESTED.
		/// </remarks>
		public static readonly TnefPropertyTag NonReceiptNotificationRequested = new TnefPropertyTag (TnefPropertyId.NonReceiptNotificationRequested, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_NON_RECEIPT_REASON.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_NON_RECEIPT_REASON.
		/// </remarks>
		public static readonly TnefPropertyTag NonReceiptReason = new TnefPropertyTag (TnefPropertyId.NonReceiptReason, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_NORMALIZED_SUBJECT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_NORMALIZED_SUBJECT.
		/// </remarks>
		public static readonly TnefPropertyTag NormalizedSubjectA = new TnefPropertyTag (TnefPropertyId.NormalizedSubject, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_NORMALIZED_SUBJECT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_NORMALIZED_SUBJECT.
		/// </remarks>
		public static readonly TnefPropertyTag NormalizedSubjectW = new TnefPropertyTag (TnefPropertyId.NormalizedSubject, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_NT_SECURITY_DESCRIPTOR.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_NT_SECURITY_DESCRIPTOR.
		/// </remarks>
		public static readonly TnefPropertyTag NtSecurityDescriptor = new TnefPropertyTag (TnefPropertyId.NtSecurityDescriptor, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_NULL.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_NULL.
		/// </remarks>
		public static readonly TnefPropertyTag Null = new TnefPropertyTag (TnefPropertyId.Null, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_OBJECT_TYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_OBJECT_TYPE.
		/// </remarks>
		public static readonly TnefPropertyTag ObjectType = new TnefPropertyTag (TnefPropertyId.ObjectType, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_OBSOLETE_IPMS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_OBSOLETE_IPMS.
		/// </remarks>
		public static readonly TnefPropertyTag ObsoletedIpms = new TnefPropertyTag (TnefPropertyId.ObsoletedIpms, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_OFFICE2_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_OFFICE2_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag Office2TelephoneNumberA = new TnefPropertyTag (TnefPropertyId.Office2TelephoneNumber, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_OFFICE2_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_OFFICE2_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag Office2TelephoneNumberW = new TnefPropertyTag (TnefPropertyId.Office2TelephoneNumber, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_OFFICE_LOCATION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_OFFICE_LOCATION.
		/// </remarks>
		public static readonly TnefPropertyTag OfficeLocationA = new TnefPropertyTag (TnefPropertyId.OfficeLocation, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_OFFICE_LOCATION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_OFFICE_LOCATION.
		/// </remarks>
		public static readonly TnefPropertyTag OfficeLocationW = new TnefPropertyTag (TnefPropertyId.OfficeLocation, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_OFFICE_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_OFFICE_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag OfficeTelephoneNumberA = new TnefPropertyTag (TnefPropertyId.OfficeTelephoneNumber, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_OFFICE_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_OFFICE_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag OfficeTelephoneNumberW = new TnefPropertyTag (TnefPropertyId.OfficeTelephoneNumber, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_OOF_REPLY_TYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_OOF_REPLY_TYPE.
		/// </remarks>
		public static readonly TnefPropertyTag OofReplyType = new TnefPropertyTag (TnefPropertyId.OofReplyType, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_ORGANIZATIONAL_ID_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORGANIZATIONAL_ID_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag OrganizationalIdNumberA = new TnefPropertyTag (TnefPropertyId.OrganizationalIdNumber, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ORGANIZATIONAL_ID_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORGANIZATIONAL_ID_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag OrganizationalIdNumberW = new TnefPropertyTag (TnefPropertyId.OrganizationalIdNumber, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ORIG_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIG_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag OrigEntryId = new TnefPropertyTag (TnefPropertyId.OrigEntryId, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_AUTHOR_ADDRTYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_AUTHOR_ADDRTYPE.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalAuthorAddrtypeA = new TnefPropertyTag (TnefPropertyId.OriginalAuthorAddrtype, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_AUTHOR_ADDRTYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_AUTHOR_ADDRTYPE.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalAuthorAddrtypeW = new TnefPropertyTag (TnefPropertyId.OriginalAuthorAddrtype, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_AUTHOR_EMAIL_ADDRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_AUTHOR_EMAIL_ADDRESS.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalAuthorEmailAddressA = new TnefPropertyTag (TnefPropertyId.OriginalAuthorEmailAddress, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_AUTHOR_EMAIL_ADDRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_AUTHOR_EMAIL_ADDRESS.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalAuthorEmailAddressW = new TnefPropertyTag (TnefPropertyId.OriginalAuthorEmailAddress, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_AUTHOR_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_AUTHOR_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalAuthorEntryId = new TnefPropertyTag (TnefPropertyId.OriginalAuthorEntryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_AUTHOR_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_AUTHOR_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalAuthorNameA = new TnefPropertyTag (TnefPropertyId.OriginalAuthorName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_AUTHOR_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_AUTHOR_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalAuthorNameW = new TnefPropertyTag (TnefPropertyId.OriginalAuthorName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_AUTHOR_SEARCH_KEY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_AUTHOR_SEARCH_KEY.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalAuthorSearchKey = new TnefPropertyTag (TnefPropertyId.OriginalAuthorSearchKey, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_DELIVERY_TIME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_DELIVERY_TIME.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalDeliveryTime = new TnefPropertyTag (TnefPropertyId.OriginalDeliveryTime, TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_DISPLAY_BCC.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_DISPLAY_BCC.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalDisplayBccA = new TnefPropertyTag (TnefPropertyId.OriginalDisplayBcc, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_DISPLAY_BCC.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_DISPLAY_BCC.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalDisplayBccW = new TnefPropertyTag (TnefPropertyId.OriginalDisplayBcc, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_DISPLAY_CC.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_DISPLAY_CC.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalDisplayCcA = new TnefPropertyTag (TnefPropertyId.OriginalDisplayCc, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_DISPLAY_CC.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_DISPLAY_CC.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalDisplayCcW = new TnefPropertyTag (TnefPropertyId.OriginalDisplayCc, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_DISPLAY_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_DISPLAY_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalDisplayNameA = new TnefPropertyTag (TnefPropertyId.OriginalDisplayName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_DISPLAY_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_DISPLAY_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalDisplayNameW = new TnefPropertyTag (TnefPropertyId.OriginalDisplayName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_DISPLAY_TO.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_DISPLAY_TO.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalDisplayToA = new TnefPropertyTag (TnefPropertyId.OriginalDisplayTo, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_DISPLAY_TO.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_DISPLAY_TO.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalDisplayToW = new TnefPropertyTag (TnefPropertyId.OriginalDisplayTo, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_EITS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_EITS.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalEits = new TnefPropertyTag (TnefPropertyId.OriginalEits, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalEntryId = new TnefPropertyTag (TnefPropertyId.OriginalEntryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_ORIGINALLY_INTENDED_RECIP_ADDRTYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINALLY_INTENDED_RECIP_ADDRTYPE.
		/// </remarks>
		public static readonly TnefPropertyTag OriginallyIntendedRecipAddrtypeA = new TnefPropertyTag (TnefPropertyId.OriginallyIntendedRecipAddrtype, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ORIGINALLY_INTENDED_RECIP_ADDRTYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINALLY_INTENDED_RECIP_ADDRTYPE.
		/// </remarks>
		public static readonly TnefPropertyTag OriginallyIntendedRecipAddrtypeW = new TnefPropertyTag (TnefPropertyId.OriginallyIntendedRecipAddrtype, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ORIGINALLY_INTENDED_RECIP_EMAIL_ADDRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINALLY_INTENDED_RECIP_EMAIL_ADDRESS.
		/// </remarks>
		public static readonly TnefPropertyTag OriginallyIntendedRecipEmailAddressA = new TnefPropertyTag (TnefPropertyId.OriginallyIntendedRecipEmailAddress, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ORIGINALLY_INTENDED_RECIP_EMAIL_ADDRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINALLY_INTENDED_RECIP_EMAIL_ADDRESS.
		/// </remarks>
		public static readonly TnefPropertyTag OriginallyIntendedRecipEmailAddressW = new TnefPropertyTag (TnefPropertyId.OriginallyIntendedRecipEmailAddress, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ORIGINALLY_INTENDED_RECIP_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINALLY_INTENDED_RECIP_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag OriginallyIntendedRecipEntryId = new TnefPropertyTag (TnefPropertyId.OriginallyIntendedRecipEntryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_ORIGINALLY_INTENDED_RECIPIENT_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINALLY_INTENDED_RECIPIENT_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag OriginallyIntendedRecipientName = new TnefPropertyTag (TnefPropertyId.OriginallyIntendedRecipientName, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SEARCH_KEY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_SEARCH_KEY.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalSearchKey = new TnefPropertyTag (TnefPropertyId.OriginalSearchKey, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENDER_ADDRTYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_SENDER_ADDRTYPE.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalSenderAddrtypeA = new TnefPropertyTag (TnefPropertyId.OriginalSenderAddrtype, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENDER_ADDRTYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_SENDER_ADDRTYPE.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalSenderAddrtypeW = new TnefPropertyTag (TnefPropertyId.OriginalSenderAddrtype, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENDER_EMAIL_ADDRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_SENDER_EMAIL_ADDRESS.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalSenderEmailAddressA = new TnefPropertyTag (TnefPropertyId.OriginalSenderEmailAddress, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENDER_EMAIL_ADDRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_SENDER_EMAIL_ADDRESS.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalSenderEmailAddressW = new TnefPropertyTag (TnefPropertyId.OriginalSenderEmailAddress, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENDER_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_SENDER_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalSenderEntryId = new TnefPropertyTag (TnefPropertyId.OriginalSenderEntryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENDER_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_SENDER_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalSenderNameA = new TnefPropertyTag (TnefPropertyId.OriginalSenderName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENDER_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_SENDER_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalSenderNameW = new TnefPropertyTag (TnefPropertyId.OriginalSenderName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENDER_SEARCH_KEY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_SENDER_SEARCH_KEY.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalSenderSearchKey = new TnefPropertyTag (TnefPropertyId.OriginalSenderSearchKey, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENSITIVITY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_SENSITIVITY.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalSensitivity = new TnefPropertyTag (TnefPropertyId.OriginalSensitivity, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENT_REPRESENTING_ADDRTYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_SENT_REPRESENTING_ADDRTYPE.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalSentRepresentingAddrtypeA = new TnefPropertyTag (TnefPropertyId.OriginalSentRepresentingAddrtype, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENT_REPRESENTING_ADDRTYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_SENT_REPRESENTING_ADDRTYPE.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalSentRepresentingAddrtypeW = new TnefPropertyTag (TnefPropertyId.OriginalSentRepresentingAddrtype, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENT_REPRESENTING_EMAIL_ADDRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_SENT_REPRESENTING_EMAIL_ADDRESS.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalSentRepresentingEmailAddressA = new TnefPropertyTag (TnefPropertyId.OriginalSentRepresentingEmailAddress, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENT_REPRESENTING_EMAIL_ADDRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_SENT_REPRESENTING_EMAIL_ADDRESS.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalSentRepresentingEmailAddressW = new TnefPropertyTag (TnefPropertyId.OriginalSentRepresentingEmailAddress, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENT_REPRESENTING_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_SENT_REPRESENTING_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalSentRepresentingEntryId = new TnefPropertyTag (TnefPropertyId.OriginalSentRepresentingEntryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENT_REPRESENTING_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_SENT_REPRESENTING_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalSentRepresentingNameA = new TnefPropertyTag (TnefPropertyId.OriginalSentRepresentingName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENT_REPRESENTING_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_SENT_REPRESENTING_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalSentRepresentingNameW = new TnefPropertyTag (TnefPropertyId.OriginalSentRepresentingName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SENT_REPRESENTING_SEARCH_KEY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_SENT_REPRESENTING_SEARCH_KEY.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalSentRepresentingSearchKey = new TnefPropertyTag (TnefPropertyId.OriginalSentRepresentingSearchKey, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SUBJECT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_SUBJECT.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalSubjectA = new TnefPropertyTag (TnefPropertyId.OriginalSubject, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SUBJECT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_SUBJECT.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalSubjectW = new TnefPropertyTag (TnefPropertyId.OriginalSubject, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_ORIGINAL_SUBMIT_TIME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINAL_SUBMIT_TIME.
		/// </remarks>
		public static readonly TnefPropertyTag OriginalSubmitTime = new TnefPropertyTag (TnefPropertyId.OriginalSubmitTime, TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PR_ORIGINATING_MTA_CERTIFICATE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINATING_MTA_CERTIFICATE.
		/// </remarks>
		public static readonly TnefPropertyTag OriginatingMtaCertificate = new TnefPropertyTag (TnefPropertyId.OriginatingMtaCertificate, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_ORIGINATOR_AND_DL_EXPANSION_HISTORY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINATOR_AND_DL_EXPANSION_HISTORY.
		/// </remarks>
		public static readonly TnefPropertyTag OriginatorAndDlExpansionHistory = new TnefPropertyTag (TnefPropertyId.OriginatorAndDlExpansionHistory, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_ORIGINATOR_CERTIFICATE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINATOR_CERTIFICATE.
		/// </remarks>
		public static readonly TnefPropertyTag OriginatorCertificate = new TnefPropertyTag (TnefPropertyId.OriginatorCertificate, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_ORIGINATOR_DELIVERY_REPORT_REQUESTED.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINATOR_DELIVERY_REPORT_REQUESTED.
		/// </remarks>
		public static readonly TnefPropertyTag OriginatorDeliveryReportRequested = new TnefPropertyTag (TnefPropertyId.OriginatorDeliveryReportRequested, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_ORIGINATOR_NON_DELIVERY_REPORT_REQUESTED.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINATOR_NON_DELIVERY_REPORT_REQUESTED.
		/// </remarks>
		public static readonly TnefPropertyTag OriginatorNonDeliveryReportRequested = new TnefPropertyTag (TnefPropertyId.OriginatorNonDeliveryReportRequested, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_ORIGINATOR_REQUESTED_ALTERNATE_RECIPIENT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINATOR_REQUESTED_ALTERNATE_RECIPIENT.
		/// </remarks>
		public static readonly TnefPropertyTag OriginatorRequestedAlternateRecipient = new TnefPropertyTag (TnefPropertyId.OriginatorRequestedAlternateRecipient, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_ORIGINATOR_RETURN_ADDRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGINATOR_RETURN_ADDRESS.
		/// </remarks>
		public static readonly TnefPropertyTag OriginatorReturnAddress = new TnefPropertyTag (TnefPropertyId.OriginatorReturnAddress, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_ORIGIN_CHECK.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIGIN_CHECK.
		/// </remarks>
		public static readonly TnefPropertyTag OriginCheck = new TnefPropertyTag (TnefPropertyId.OriginCheck, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_ORIG_MESSAGE_CLASS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIG_MESSAGE_CLASS.
		/// </remarks>
		public static readonly TnefPropertyTag OrigMessageClassA = new TnefPropertyTag (TnefPropertyId.OrigMessageClass, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_ORIG_MESSAGE_CLASS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ORIG_MESSAGE_CLASS.
		/// </remarks>
		public static readonly TnefPropertyTag OrigMessageClassW = new TnefPropertyTag (TnefPropertyId.OrigMessageClass, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_OTHER_ADDRESS_CITY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_OTHER_ADDRESS_CITY.
		/// </remarks>
		public static readonly TnefPropertyTag OtherAddressCityA = new TnefPropertyTag (TnefPropertyId.OtherAddressCity, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_OTHER_ADDRESS_CITY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_OTHER_ADDRESS_CITY.
		/// </remarks>
		public static readonly TnefPropertyTag OtherAddressCityW = new TnefPropertyTag (TnefPropertyId.OtherAddressCity, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_OTHER_ADDRESS_COUNTRY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_OTHER_ADDRESS_COUNTRY.
		/// </remarks>
		public static readonly TnefPropertyTag OtherAddressCountryA = new TnefPropertyTag (TnefPropertyId.OtherAddressCountry, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_OTHER_ADDRESS_COUNTRY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_OTHER_ADDRESS_COUNTRY.
		/// </remarks>
		public static readonly TnefPropertyTag OtherAddressCountryW = new TnefPropertyTag (TnefPropertyId.OtherAddressCountry, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_OTHER_ADDRESS_POSTAL_CODE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_OTHER_ADDRESS_POSTAL_CODE.
		/// </remarks>
		public static readonly TnefPropertyTag OtherAddressPostalCodeA = new TnefPropertyTag (TnefPropertyId.OtherAddressPostalCode, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_OTHER_ADDRESS_POSTAL_CODE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_OTHER_ADDRESS_POSTAL_CODE.
		/// </remarks>
		public static readonly TnefPropertyTag OtherAddressPostalCodeW = new TnefPropertyTag (TnefPropertyId.OtherAddressPostalCode, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_OTHER_ADDRESS_POST_OFFICE_BOX.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_OTHER_ADDRESS_POST_OFFICE_BOX.
		/// </remarks>
		public static readonly TnefPropertyTag OtherAddressPostOfficeBoxA = new TnefPropertyTag (TnefPropertyId.OtherAddressPostOfficeBox, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_OTHER_ADDRESS_POST_OFFICE_BOX.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_OTHER_ADDRESS_POST_OFFICE_BOX.
		/// </remarks>
		public static readonly TnefPropertyTag OtherAddressPostOfficeBoxW = new TnefPropertyTag (TnefPropertyId.OtherAddressPostOfficeBox, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_OTHER_ADDRESS_STATE_OR_PROVINCE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_OTHER_ADDRESS_STATE_OR_PROVINCE.
		/// </remarks>
		public static readonly TnefPropertyTag OtherAddressStateOrProvinceA = new TnefPropertyTag (TnefPropertyId.OtherAddressStateOrProvince, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_OTHER_ADDRESS_STATE_OR_PROVINCE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_OTHER_ADDRESS_STATE_OR_PROVINCE.
		/// </remarks>
		public static readonly TnefPropertyTag OtherAddressStateOrProvinceW = new TnefPropertyTag (TnefPropertyId.OtherAddressStateOrProvince, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_OTHER_ADDRESS_STREET.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_OTHER_ADDRESS_STREET.
		/// </remarks>
		public static readonly TnefPropertyTag OtherAddressStreetA = new TnefPropertyTag (TnefPropertyId.OtherAddressStreet, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_OTHER_ADDRESS_STREET.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_OTHER_ADDRESS_STREET.
		/// </remarks>
		public static readonly TnefPropertyTag OtherAddressStreetW = new TnefPropertyTag (TnefPropertyId.OtherAddressStreet, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_OTHER_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_OTHER_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag OtherTelephoneNumberA = new TnefPropertyTag (TnefPropertyId.OtherTelephoneNumber, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_OTHER_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_OTHER_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag OtherTelephoneNumberW = new TnefPropertyTag (TnefPropertyId.OtherTelephoneNumber, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_OWNER_APPT_ID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_OWNER_APPT_ID.
		/// </remarks>
		public static readonly TnefPropertyTag OwnerApptId = new TnefPropertyTag (TnefPropertyId.OwnerApptId, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_OWN_STORE_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_OWN_STORE_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag OwnStoreEntryId = new TnefPropertyTag (TnefPropertyId.OwnStoreEntryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_PAGER_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PAGER_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag PagerTelephoneNumberA = new TnefPropertyTag (TnefPropertyId.PagerTelephoneNumber, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_PAGER_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PAGER_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag PagerTelephoneNumberW = new TnefPropertyTag (TnefPropertyId.PagerTelephoneNumber, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_PARENT_DISPLAY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PARENT_DISPLAY.
		/// </remarks>
		public static readonly TnefPropertyTag ParentDisplayA = new TnefPropertyTag (TnefPropertyId.ParentDisplay, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_PARENT_DISPLAY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PARENT_DISPLAY.
		/// </remarks>
		public static readonly TnefPropertyTag ParentDisplayW = new TnefPropertyTag (TnefPropertyId.ParentDisplay, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_PARENT_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PARENT_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag ParentEntryId = new TnefPropertyTag (TnefPropertyId.ParentEntryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_PARENT_KEY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PARENT_KEY.
		/// </remarks>
		public static readonly TnefPropertyTag ParentKey = new TnefPropertyTag (TnefPropertyId.ParentKey, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_PERSONAL_HOME_PAGE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PERSONAL_HOME_PAGE.
		/// </remarks>
		public static readonly TnefPropertyTag PersonalHomePageA = new TnefPropertyTag (TnefPropertyId.PersonalHomePage, TnefPropertyType.String8);
		/// <summary>
		/// The MAPI property PR_PERSONAL_HOME_PAGE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PERSONAL_HOME_PAGE.
		/// </remarks>
		public static readonly TnefPropertyTag PersonalHomePageW = new TnefPropertyTag (TnefPropertyId.PersonalHomePage, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_PHYSICAL_DELIVERY_BUREAU_FAX_DELIVERY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PHYSICAL_DELIVERY_BUREAU_FAX_DELIVERY.
		/// </remarks>
		public static readonly TnefPropertyTag PhysicalDeliveryBureauFaxDelivery = new TnefPropertyTag (TnefPropertyId.PhysicalDeliveryBureauFaxDelivery, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_PHYSICAL_DELIVERY_MODE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PHYSICAL_DELIVERY_MODE.
		/// </remarks>
		public static readonly TnefPropertyTag PhysicalDeliveryMode = new TnefPropertyTag (TnefPropertyId.PhysicalDeliveryMode, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_PHYSICAL_DELIVERY_REPORT_REQUEST.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PHYSICAL_DELIVERY_REPORT_REQUEST.
		/// </remarks>
		public static readonly TnefPropertyTag PhysicalDeliveryReportRequest = new TnefPropertyTag (TnefPropertyId.PhysicalDeliveryReportRequest, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_PHYSICAL_FORWARDING_ADDRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PHYSICAL_FORWARDING_ADDRESS.
		/// </remarks>
		public static readonly TnefPropertyTag PhysicalForwardingAddress = new TnefPropertyTag (TnefPropertyId.PhysicalForwardingAddress, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_PHYSICAL_FORWARDING_ADDRESS_REQUESTED.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PHYSICAL_FORWARDING_ADDRESS_REQUESTED.
		/// </remarks>
		public static readonly TnefPropertyTag PhysicalForwardingAddressRequested = new TnefPropertyTag (TnefPropertyId.PhysicalForwardingAddressRequested, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_PHYSICAL_FORWARDING_PROHIBITED.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PHYSICAL_FORWARDING_PROHIBITED.
		/// </remarks>
		public static readonly TnefPropertyTag PhysicalForwardingProhibited = new TnefPropertyTag (TnefPropertyId.PhysicalForwardingProhibited, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_PHYSICAL_RENDITION_ATTRIBUTES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PHYSICAL_RENDITION_ATTRIBUTES.
		/// </remarks>
		public static readonly TnefPropertyTag PhysicalRenditionAttributes = new TnefPropertyTag (TnefPropertyId.PhysicalRenditionAttributes, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_POSTAL_ADDRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_POSTAL_ADDRESS.
		/// </remarks>
		public static readonly TnefPropertyTag PostalAddressA = new TnefPropertyTag (TnefPropertyId.PostalAddress, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_POSTAL_ADDRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_POSTAL_ADDRESS.
		/// </remarks>
		public static readonly TnefPropertyTag PostalAddressW = new TnefPropertyTag (TnefPropertyId.PostalAddress, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_POSTAL_CODE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_POSTAL_CODE.
		/// </remarks>
		public static readonly TnefPropertyTag PostalCodeA = new TnefPropertyTag (TnefPropertyId.PostalCode, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_POSTAL_CODE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_POSTAL_CODE.
		/// </remarks>
		public static readonly TnefPropertyTag PostalCodeW = new TnefPropertyTag (TnefPropertyId.PostalCode, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_POST_FOLDER_ENTRIES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_POST_FOLDER_ENTRIES.
		/// </remarks>
		public static readonly TnefPropertyTag PostFolderEntries = new TnefPropertyTag (TnefPropertyId.PostFolderEntries, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_POST_FOLDER_NAMES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_POST_FOLDER_NAMES.
		/// </remarks>
		public static readonly TnefPropertyTag PostFolderNamesA = new TnefPropertyTag (TnefPropertyId.PostFolderNames, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_POST_FOLDER_NAMES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_POST_FOLDER_NAMES.
		/// </remarks>
		public static readonly TnefPropertyTag PostFolderNamesW = new TnefPropertyTag (TnefPropertyId.PostFolderNames, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_POST_OFFICE_BOX.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_POST_OFFICE_BOX.
		/// </remarks>
		public static readonly TnefPropertyTag PostOfficeBoxA = new TnefPropertyTag (TnefPropertyId.PostOfficeBox, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_POST_OFFICE_BOX.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_POST_OFFICE_BOX.
		/// </remarks>
		public static readonly TnefPropertyTag PostOfficeBoxW = new TnefPropertyTag (TnefPropertyId.PostOfficeBox, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_POST_REPLY_DENIED.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_POST_REPLY_DENIED.
		/// </remarks>
		public static readonly TnefPropertyTag PostReplyDenied = new TnefPropertyTag (TnefPropertyId.PostReplyDenied, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_POST_REPLY_FOLDER_ENTRIES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_POST_REPLY_FOLDER_ENTRIES.
		/// </remarks>
		public static readonly TnefPropertyTag PostReplyFolderEntries = new TnefPropertyTag (TnefPropertyId.PostReplyFolderEntries, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_POST_REPLY_FOLDER_NAMES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_POST_REPLY_FOLDER_NAMES.
		/// </remarks>
		public static readonly TnefPropertyTag PostReplyFolderNamesA = new TnefPropertyTag (TnefPropertyId.PostReplyFolderNames, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_POST_REPLY_FOLDER_NAMES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_POST_REPLY_FOLDER_NAMES.
		/// </remarks>
		public static readonly TnefPropertyTag PostReplyFolderNamesW = new TnefPropertyTag (TnefPropertyId.PostReplyFolderNames, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_PREFERRED_BY_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PREFERRED_BY_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag PreferredByNameA = new TnefPropertyTag (TnefPropertyId.PreferredByName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_PREFERRED_BY_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PREFERRED_BY_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag PreferredByNameW = new TnefPropertyTag (TnefPropertyId.PreferredByName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_PREPROCESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PREPROCESS.
		/// </remarks>
		public static readonly TnefPropertyTag Preprocess = new TnefPropertyTag (TnefPropertyId.Preprocess, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_PRIMARY_CAPABILITY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PRIMARY_CAPABILITY.
		/// </remarks>
		public static readonly TnefPropertyTag PrimaryCapability = new TnefPropertyTag (TnefPropertyId.PrimaryCapability, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_PRIMARY_FAX_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PRIMARY_FAX_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag PrimaryFaxNumberA = new TnefPropertyTag (TnefPropertyId.PrimaryFaxNumber, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_PRIMARY_FAX_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PRIMARY_FAX_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag PrimaryFaxNumberW = new TnefPropertyTag (TnefPropertyId.PrimaryFaxNumber, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_PRIMARY_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PRIMARY_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag PrimaryTelephoneNumberA = new TnefPropertyTag (TnefPropertyId.PrimaryTelephoneNumber, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_PRIMARY_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PRIMARY_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag PrimaryTelephoneNumberW = new TnefPropertyTag (TnefPropertyId.PrimaryTelephoneNumber, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_PRIORITY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PRIORITY.
		/// </remarks>
		public static readonly TnefPropertyTag Priority = new TnefPropertyTag (TnefPropertyId.Priority, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_PROFESSION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PROFESSION.
		/// </remarks>
		public static readonly TnefPropertyTag ProfessionA = new TnefPropertyTag (TnefPropertyId.Profession, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_PROFESSION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PROFESSION.
		/// </remarks>
		public static readonly TnefPropertyTag ProfessionW = new TnefPropertyTag (TnefPropertyId.Profession, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_PROFILE_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PROFILE_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag ProfileNameA = new TnefPropertyTag (TnefPropertyId.ProfileName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_PROFILE_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PROFILE_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag ProfileNameW = new TnefPropertyTag (TnefPropertyId.ProfileName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_PROOF_OF_DELIVERY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PROOF_OF_DELIVERY.
		/// </remarks>
		public static readonly TnefPropertyTag ProofOfDelivery = new TnefPropertyTag (TnefPropertyId.ProofOfDelivery, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_PROOF_OF_DELIVERY_REQUESTED.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PROOF_OF_DELIVERY_REQUESTED.
		/// </remarks>
		public static readonly TnefPropertyTag ProofOfDeliveryRequested = new TnefPropertyTag (TnefPropertyId.ProofOfDeliveryRequested, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_PROOF_OF_SUBMISSION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PROOF_OF_SUBMISSION.
		/// </remarks>
		public static readonly TnefPropertyTag ProofOfSubmission = new TnefPropertyTag (TnefPropertyId.ProofOfSubmission, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_PROOF_OF_SUBMISSION_REQUESTED.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PROOF_OF_SUBMISSION_REQUESTED.
		/// </remarks>
		public static readonly TnefPropertyTag ProofOfSubmissionRequested = new TnefPropertyTag (TnefPropertyId.ProofOfSubmissionRequested, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_PROVIDER_DISPLAY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PROVIDER_DISPLAY.
		/// </remarks>
		public static readonly TnefPropertyTag ProviderDisplayA = new TnefPropertyTag (TnefPropertyId.ProviderDisplay, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_PROVIDER_DISPLAY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PROVIDER_DISPLAY.
		/// </remarks>
		public static readonly TnefPropertyTag ProviderDisplayW = new TnefPropertyTag (TnefPropertyId.ProviderDisplay, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_PROVIDER_DLL_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PROVIDER_DLL_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag ProviderDllNameA = new TnefPropertyTag (TnefPropertyId.ProviderDllName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_PROVIDER_DLL_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PROVIDER_DLL_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag ProviderDllNameW = new TnefPropertyTag (TnefPropertyId.ProviderDllName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_PROVIDER_ORDINAL.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PROVIDER_ORDINAL.
		/// </remarks>
		public static readonly TnefPropertyTag ProviderOrdinal = new TnefPropertyTag (TnefPropertyId.ProviderOrdinal, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_PROVIDER_SUBMIT_TIME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PROVIDER_SUBMIT_TIME.
		/// </remarks>
		public static readonly TnefPropertyTag ProviderSubmitTime = new TnefPropertyTag (TnefPropertyId.ProviderSubmitTime, TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PR_PROVIDER_UID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PROVIDER_UID.
		/// </remarks>
		public static readonly TnefPropertyTag ProviderUid = new TnefPropertyTag (TnefPropertyId.ProviderUid, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_PUID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_PUID.
		/// </remarks>
		public static readonly TnefPropertyTag Puid = new TnefPropertyTag (TnefPropertyId.Puid, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_RADIO_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RADIO_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag RadioTelephoneNumberA = new TnefPropertyTag (TnefPropertyId.RadioTelephoneNumber, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_RADIO_TELEPHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RADIO_TELEPHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag RadioTelephoneNumberW = new TnefPropertyTag (TnefPropertyId.RadioTelephoneNumber, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_RCVD_REPRESENTING_ADDRTYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RCVD_REPRESENTING_ADDRTYPE.
		/// </remarks>
		public static readonly TnefPropertyTag RcvdRepresentingAddrtypeA = new TnefPropertyTag (TnefPropertyId.RcvdRepresentingAddrtype, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_RCVD_REPRESENTING_ADDRTYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RCVD_REPRESENTING_ADDRTYPE.
		/// </remarks>
		public static readonly TnefPropertyTag RcvdRepresentingAddrtypeW = new TnefPropertyTag (TnefPropertyId.RcvdRepresentingAddrtype, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_RCVD_REPRESENTING_EMAIL_ADDRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RCVD_REPRESENTING_EMAIL_ADDRESS.
		/// </remarks>
		public static readonly TnefPropertyTag RcvdRepresentingEmailAddressA = new TnefPropertyTag (TnefPropertyId.RcvdRepresentingEmailAddress, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_RCVD_REPRESENTING_EMAIL_ADDRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RCVD_REPRESENTING_EMAIL_ADDRESS.
		/// </remarks>
		public static readonly TnefPropertyTag RcvdRepresentingEmailAddressW = new TnefPropertyTag (TnefPropertyId.RcvdRepresentingEmailAddress, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_RCVD_REPRESENTING_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RCVD_REPRESENTING_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag RcvdRepresentingEntryId = new TnefPropertyTag (TnefPropertyId.RcvdRepresentingEntryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_RCVD_REPRESENTING_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RCVD_REPRESENTING_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag RcvdRepresentingNameA = new TnefPropertyTag (TnefPropertyId.RcvdRepresentingName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_RCVD_REPRESENTING_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RCVD_REPRESENTING_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag RcvdRepresentingNameW = new TnefPropertyTag (TnefPropertyId.RcvdRepresentingName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_RCVD_REPRESENTING_SEARCH_KEY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RCVD_REPRESENTING_SEARCH_KEY.
		/// </remarks>
		public static readonly TnefPropertyTag RcvdRepresentingSearchKey = new TnefPropertyTag (TnefPropertyId.RcvdRepresentingSearchKey, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_READ_RECEIPT_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_READ_RECEIPT_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag ReadReceiptEntryId = new TnefPropertyTag (TnefPropertyId.ReadReceiptEntryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_READ_RECEIPT_REQUESTED.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_READ_RECEIPT_REQUESTED.
		/// </remarks>
		public static readonly TnefPropertyTag ReadReceiptRequested = new TnefPropertyTag (TnefPropertyId.ReadReceiptRequested, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_READ_RECEIPT_SEARCH_KEY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_READ_RECEIPT_SEARCH_KEY.
		/// </remarks>
		public static readonly TnefPropertyTag ReadReceiptSearchKey = new TnefPropertyTag (TnefPropertyId.ReadReceiptSearchKey, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_RECEIPT_TIME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RECEIPT_TIME.
		/// </remarks>
		public static readonly TnefPropertyTag ReceiptTime = new TnefPropertyTag (TnefPropertyId.ReceiptTime, TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PR_RECEIVED_BY_ADDRTYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RECEIVED_BY_ADDRTYPE.
		/// </remarks>
		public static readonly TnefPropertyTag ReceivedByAddrtypeA = new TnefPropertyTag (TnefPropertyId.ReceivedByAddrtype, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_RECEIVED_BY_ADDRTYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RECEIVED_BY_ADDRTYPE.
		/// </remarks>
		public static readonly TnefPropertyTag ReceivedByAddrtypeW = new TnefPropertyTag (TnefPropertyId.ReceivedByAddrtype, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_RECEIVED_BY_EMAIL_ADDRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RECEIVED_BY_EMAIL_ADDRESS.
		/// </remarks>
		public static readonly TnefPropertyTag ReceivedByEmailAddressA = new TnefPropertyTag (TnefPropertyId.ReceivedByEmailAddress, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_RECEIVED_BY_EMAIL_ADDRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RECEIVED_BY_EMAIL_ADDRESS.
		/// </remarks>
		public static readonly TnefPropertyTag ReceivedByEmailAddressW = new TnefPropertyTag (TnefPropertyId.ReceivedByEmailAddress, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_RECEIVED_BY_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RECEIVED_BY_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag ReceivedByEntryId = new TnefPropertyTag (TnefPropertyId.ReceivedByEntryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_RECEIVED_BY_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RECEIVED_BY_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag ReceivedByNameA = new TnefPropertyTag (TnefPropertyId.ReceivedByName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_RECEIVED_BY_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RECEIVED_BY_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag ReceivedByNameW = new TnefPropertyTag (TnefPropertyId.ReceivedByName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_RECEIVED_BY_SEARCH_KEY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RECEIVED_BY_SEARCH_KEY.
		/// </remarks>
		public static readonly TnefPropertyTag ReceivedBySearchKey = new TnefPropertyTag (TnefPropertyId.ReceivedBySearchKey, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_RECEIVE_FOLDER_SETTINGS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RECEIVE_FOLDER_SETTINGS.
		/// </remarks>
		public static readonly TnefPropertyTag ReceiveFolderSettings = new TnefPropertyTag (TnefPropertyId.ReceiveFolderSettings, TnefPropertyType.Object);

		/// <summary>
		/// The MAPI property PR_RECIPIENT_CERTIFICATE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RECIPIENT_CERTIFICATE.
		/// </remarks>
		public static readonly TnefPropertyTag RecipientCertificate = new TnefPropertyTag (TnefPropertyId.RecipientCertificate, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_RECIPIENT_NUMBER_FOR_ADVICE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RECIPIENT_NUMBER_FOR_ADVICE.
		/// </remarks>
		public static readonly TnefPropertyTag RecipientNumberForAdviceA = new TnefPropertyTag (TnefPropertyId.RecipientNumberForAdvice, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_RECIPIENT_NUMBER_FOR_ADVICE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RECIPIENT_NUMBER_FOR_ADVICE.
		/// </remarks>
		public static readonly TnefPropertyTag RecipientNumberForAdviceW = new TnefPropertyTag (TnefPropertyId.RecipientNumberForAdvice, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_RECIPIENT_REASSIGNMENT_PROHIBITED.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RECIPIENT_REASSIGNMENT_PROHIBITED.
		/// </remarks>
		public static readonly TnefPropertyTag RecipientReassignmentProhibited = new TnefPropertyTag (TnefPropertyId.RecipientReassignmentProhibited, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_RECIPIENT_STATUS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RECIPIENT_STATUS.
		/// </remarks>
		public static readonly TnefPropertyTag RecipientStatus = new TnefPropertyTag (TnefPropertyId.RecipientStatus, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_RECIPIENT_TYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RECIPIENT_TYPE.
		/// </remarks>
		public static readonly TnefPropertyTag RecipientType = new TnefPropertyTag (TnefPropertyId.RecipientType, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_REDIRECTION_HISTORY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_REDIRECTION_HISTORY.
		/// </remarks>
		public static readonly TnefPropertyTag RedirectionHistory = new TnefPropertyTag (TnefPropertyId.RedirectionHistory, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_REFERRED_BY_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_REFERRED_BY_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag ReferredByNameA = new TnefPropertyTag (TnefPropertyId.ReferredByName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_REFERRED_BY_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_REFERRED_BY_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag ReferredByNameW = new TnefPropertyTag (TnefPropertyId.ReferredByName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_REGISTERED_MAIL_TYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_REGISTERED_MAIL_TYPE.
		/// </remarks>
		public static readonly TnefPropertyTag RegisteredMailType = new TnefPropertyTag (TnefPropertyId.RegisteredMailType, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_RELATED_IPMS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RELATED_IPMS.
		/// </remarks>
		public static readonly TnefPropertyTag RelatedIpms = new TnefPropertyTag (TnefPropertyId.RelatedIpms, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_REMOTE_PROGRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_REMOTE_PROGRESS.
		/// </remarks>
		public static readonly TnefPropertyTag RemoteProgress = new TnefPropertyTag (TnefPropertyId.RemoteProgress, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_REMOTE_PROGRESS_TEXT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_REMOTE_PROGRESS_TEXT.
		/// </remarks>
		public static readonly TnefPropertyTag RemoteProgressTextA = new TnefPropertyTag (TnefPropertyId.RemoteProgressText, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_REMOTE_PROGRESS_TEXT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_REMOTE_PROGRESS_TEXT.
		/// </remarks>
		public static readonly TnefPropertyTag RemoteProgressTextW = new TnefPropertyTag (TnefPropertyId.RemoteProgressText, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_REMOTE_VALIDATE_OK.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_REMOTE_VALIDATE_OK.
		/// </remarks>
		public static readonly TnefPropertyTag RemoteValidateOk = new TnefPropertyTag (TnefPropertyId.RemoteValidateOk, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_RENDERING_POSITION.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RENDERING_POSITION.
		/// </remarks>
		public static readonly TnefPropertyTag RenderingPosition = new TnefPropertyTag (TnefPropertyId.RenderingPosition, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_REPLY_RECIPIENT_ENTRIES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_REPLY_RECIPIENT_ENTRIES.
		/// </remarks>
		public static readonly TnefPropertyTag ReplyRecipientEntries = new TnefPropertyTag (TnefPropertyId.ReplyRecipientEntries, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_REPLY_RECIPIENT_NAMES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_REPLY_RECIPIENT_NAMES.
		/// </remarks>
		public static readonly TnefPropertyTag ReplyRecipientNamesA = new TnefPropertyTag (TnefPropertyId.ReplyRecipientNames, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_REPLY_RECIPIENT_NAMES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_REPLY_RECIPIENT_NAMES.
		/// </remarks>
		public static readonly TnefPropertyTag ReplyRecipientNamesW = new TnefPropertyTag (TnefPropertyId.ReplyRecipientNames, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_REPLY_REQUESTED.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_REPLY_REQUESTED.
		/// </remarks>
		public static readonly TnefPropertyTag ReplyRequested = new TnefPropertyTag (TnefPropertyId.ReplyRequested, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_REPLY_TIME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_REPLY_TIME.
		/// </remarks>
		public static readonly TnefPropertyTag ReplyTime = new TnefPropertyTag (TnefPropertyId.ReplyTime, TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PR_REPORT_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_REPORT_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag ReportEntryId = new TnefPropertyTag (TnefPropertyId.ReportEntryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_REPORTING_DL_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_REPORTING_DL_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag ReportingDlName = new TnefPropertyTag (TnefPropertyId.ReportingDlName, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_REPORTING_MTA_CERTIFICATE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_REPORTING_MTA_CERTIFICATE.
		/// </remarks>
		public static readonly TnefPropertyTag ReportingMtaCertificate = new TnefPropertyTag (TnefPropertyId.ReportingMtaCertificate, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_REPORT_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_REPORT_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag ReportNameA = new TnefPropertyTag (TnefPropertyId.ReportName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_REPORT_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_REPORT_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag ReportNameW = new TnefPropertyTag (TnefPropertyId.ReportName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_REPORT_SEARCH_KEY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_REPORT_SEARCH_KEY.
		/// </remarks>
		public static readonly TnefPropertyTag ReportSearchKey = new TnefPropertyTag (TnefPropertyId.ReportSearchKey, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_REPORT_TAG.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_REPORT_TAG.
		/// </remarks>
		public static readonly TnefPropertyTag ReportTag = new TnefPropertyTag (TnefPropertyId.ReportTag, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_REPORT_TEXT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_REPORT_TEXT.
		/// </remarks>
		public static readonly TnefPropertyTag ReportTextA = new TnefPropertyTag (TnefPropertyId.ReportText, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_REPORT_TEXT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_REPORT_TEXT.
		/// </remarks>
		public static readonly TnefPropertyTag ReportTextW = new TnefPropertyTag (TnefPropertyId.ReportText, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_REPORT_TIME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_REPORT_TIME.
		/// </remarks>
		public static readonly TnefPropertyTag ReportTime = new TnefPropertyTag (TnefPropertyId.ReportTime, TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PR_REQUESTED_DELIVERY_METHOD.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_REQUESTED_DELIVERY_METHOD.
		/// </remarks>
		public static readonly TnefPropertyTag RequestedDeliveryMethod = new TnefPropertyTag (TnefPropertyId.RequestedDeliveryMethod, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_RESOURCE_FLAGS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RESOURCE_FLAGS.
		/// </remarks>
		public static readonly TnefPropertyTag ResourceFlags = new TnefPropertyTag (TnefPropertyId.ResourceFlags, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_RESOURCE_METHODS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RESOURCE_METHODS.
		/// </remarks>
		public static readonly TnefPropertyTag ResourceMethods = new TnefPropertyTag (TnefPropertyId.ResourceMethods, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_RESOURCE_PATH.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RESOURCE_PATH.
		/// </remarks>
		public static readonly TnefPropertyTag ResourcePathA = new TnefPropertyTag (TnefPropertyId.ResourcePath, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_RESOURCE_PATH.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RESOURCE_PATH.
		/// </remarks>
		public static readonly TnefPropertyTag ResourcePathW = new TnefPropertyTag (TnefPropertyId.ResourcePath, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_RESOURCE_TYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RESOURCE_TYPE.
		/// </remarks>
		public static readonly TnefPropertyTag ResourceType = new TnefPropertyTag (TnefPropertyId.ResourceType, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_RESPONSE_REQUESTED.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RESPONSE_REQUESTED.
		/// </remarks>
		public static readonly TnefPropertyTag ResponseRequested = new TnefPropertyTag (TnefPropertyId.ResponseRequested, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_RESPONSIBILITY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RESPONSIBILITY.
		/// </remarks>
		public static readonly TnefPropertyTag Responsibility = new TnefPropertyTag (TnefPropertyId.Responsibility, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_RETURNED_IPM.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RETURNED_IPM.
		/// </remarks>
		public static readonly TnefPropertyTag ReturnedIpm = new TnefPropertyTag (TnefPropertyId.ReturnedIpm, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_ROWID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ROWID.
		/// </remarks>
		public static readonly TnefPropertyTag Rowid = new TnefPropertyTag (TnefPropertyId.Rowid, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_ROW_TYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_ROW_TYPE.
		/// </remarks>
		public static readonly TnefPropertyTag RowType = new TnefPropertyTag (TnefPropertyId.RowType, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_RTF_COMPRESSED.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RTF_COMPRESSED.
		/// </remarks>
		public static readonly TnefPropertyTag RtfCompressed = new TnefPropertyTag (TnefPropertyId.RtfCompressed, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_RTF_IN_SYNC.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RTF_IN_SYNC.
		/// </remarks>
		public static readonly TnefPropertyTag RtfInSync = new TnefPropertyTag (TnefPropertyId.RtfInSync, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_RTF_SYNC_BODY_COUNT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RTF_SYNC_BODY_COUNT.
		/// </remarks>
		public static readonly TnefPropertyTag RtfSyncBodyCount = new TnefPropertyTag (TnefPropertyId.RtfSyncBodyCount, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_RTF_SYNC_BODY_CRC.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RTF_SYNC_BODY_CRC.
		/// </remarks>
		public static readonly TnefPropertyTag RtfSyncBodyCrc = new TnefPropertyTag (TnefPropertyId.RtfSyncBodyCrc, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_RTF_SYNC_BODY_TAG.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RTF_SYNC_BODY_TAG.
		/// </remarks>
		public static readonly TnefPropertyTag RtfSyncBodyTagA = new TnefPropertyTag (TnefPropertyId.RtfSyncBodyTag, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_RTF_SYNC_BODY_TAG.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RTF_SYNC_BODY_TAG.
		/// </remarks>
		public static readonly TnefPropertyTag RtfSyncBodyTagW = new TnefPropertyTag (TnefPropertyId.RtfSyncBodyTag, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_RTF_SYNC_PREFIX_COUNT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RTF_SYNC_PREFIX_COUNT.
		/// </remarks>
		public static readonly TnefPropertyTag RtfSyncPrefixCount = new TnefPropertyTag (TnefPropertyId.RtfSyncPrefixCount, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_RTF_SYNC_TRAILING_COUNT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_RTF_SYNC_TRAILING_COUNT.
		/// </remarks>
		public static readonly TnefPropertyTag RtfSyncTrailingCount = new TnefPropertyTag (TnefPropertyId.RtfSyncTrailingCount, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_SEARCH.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SEARCH.
		/// </remarks>
		public static readonly TnefPropertyTag Search = new TnefPropertyTag (TnefPropertyId.Search, TnefPropertyType.Object);

		/// <summary>
		/// The MAPI property PR_SEARCH_KEY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SEARCH_KEY.
		/// </remarks>
		public static readonly TnefPropertyTag SearchKey = new TnefPropertyTag (TnefPropertyId.SearchKey, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_SECURITY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SECURITY.
		/// </remarks>
		public static readonly TnefPropertyTag Security = new TnefPropertyTag (TnefPropertyId.Security, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_SELECTABLE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SELECTABLE.
		/// </remarks>
		public static readonly TnefPropertyTag Selectable = new TnefPropertyTag (TnefPropertyId.Selectable, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_SENDER_ADDRTYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SENDER_ADDRTYPE.
		/// </remarks>
		public static readonly TnefPropertyTag SenderAddrtypeA = new TnefPropertyTag (TnefPropertyId.SenderAddrtype, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_SENDER_ADDRTYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SENDER_ADDRTYPE.
		/// </remarks>
		public static readonly TnefPropertyTag SenderAddrtypeW = new TnefPropertyTag (TnefPropertyId.SenderAddrtype, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_SENDER_EMAIL_ADDRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SENDER_EMAIL_ADDRESS.
		/// </remarks>
		public static readonly TnefPropertyTag SenderEmailAddressA = new TnefPropertyTag (TnefPropertyId.SenderEmailAddress, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_SENDER_EMAIL_ADDRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SENDER_EMAIL_ADDRESS.
		/// </remarks>
		public static readonly TnefPropertyTag SenderEmailAddressW = new TnefPropertyTag (TnefPropertyId.SenderEmailAddress, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_SENDER_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SENDER_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag SenderEntryId = new TnefPropertyTag (TnefPropertyId.SenderEntryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_SENDER_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SENDER_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag SenderNameA = new TnefPropertyTag (TnefPropertyId.SenderName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_SENDER_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SENDER_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag SenderNameW = new TnefPropertyTag (TnefPropertyId.SenderName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_SENDER_SEARCH_KEY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SENDER_SEARCH_KEY.
		/// </remarks>
		public static readonly TnefPropertyTag SenderSearchKey = new TnefPropertyTag (TnefPropertyId.SenderSearchKey, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_SEND_INTERNET_ENCODING.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SEND_INTERNET_ENCODING.
		/// </remarks>
		public static readonly TnefPropertyTag SendInternetEncoding = new TnefPropertyTag (TnefPropertyId.SendInternetEncoding, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_SEND_RECALL_REPORT
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SEND_RECALL_REPORT.
		/// </remarks>
		public static readonly TnefPropertyTag SendRecallReport = new TnefPropertyTag (TnefPropertyId.SendRecallReport, TnefPropertyType.Unspecified);

		/// <summary>
		/// The MAPI property PR_SEND_RICH_INFO.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SEND_RICH_INFO.
		/// </remarks>
		public static readonly TnefPropertyTag SendRichInfo = new TnefPropertyTag (TnefPropertyId.SendRichInfo, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_SENSITIVITY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SENSITIVITY.
		/// </remarks>
		public static readonly TnefPropertyTag Sensitivity = new TnefPropertyTag (TnefPropertyId.Sensitivity, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_SENTMAIL_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SENTMAIL_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag SentmailEntryId = new TnefPropertyTag (TnefPropertyId.SentmailEntryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_SENT_REPRESENTING_ADDRTYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SENT_REPRESENTING_ADDRTYPE.
		/// </remarks>
		public static readonly TnefPropertyTag SentRepresentingAddrtypeA = new TnefPropertyTag (TnefPropertyId.SentRepresentingAddrtype, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_SENT_REPRESENTING_ADDRTYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SENT_REPRESENTING_ADDRTYPE.
		/// </remarks>
		public static readonly TnefPropertyTag SentRepresentingAddrtypeW = new TnefPropertyTag (TnefPropertyId.SentRepresentingAddrtype, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_SENT_REPRESENTING_EMAIL_ADDRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SENT_REPRESENTING_EMAIL_ADDRESS.
		/// </remarks>
		public static readonly TnefPropertyTag SentRepresentingEmailAddressA = new TnefPropertyTag (TnefPropertyId.SentRepresentingEmailAddress, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_SENT_REPRESENTING_EMAIL_ADDRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SENT_REPRESENTING_EMAIL_ADDRESS.
		/// </remarks>
		public static readonly TnefPropertyTag SentRepresentingEmailAddressW = new TnefPropertyTag (TnefPropertyId.SentRepresentingEmailAddress, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_SENT_REPRESENTING_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SENT_REPRESENTING_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag SentRepresentingEntryId = new TnefPropertyTag (TnefPropertyId.SentRepresentingEntryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_SENT_REPRESENTING_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SENT_REPRESENTING_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag SentRepresentingNameA = new TnefPropertyTag (TnefPropertyId.SentRepresentingName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_SENT_REPRESENTING_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SENT_REPRESENTING_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag SentRepresentingNameW = new TnefPropertyTag (TnefPropertyId.SentRepresentingName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_SENT_REPRESENTING_SEARCH_KEY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SENT_REPRESENTING_SEARCH_KEY.
		/// </remarks>
		public static readonly TnefPropertyTag SentRepresentingSearchKey = new TnefPropertyTag (TnefPropertyId.SentRepresentingSearchKey, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_SERVICE_DELETE_FILES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SERVICE_DELETE_FILES.
		/// </remarks>
		public static readonly TnefPropertyTag ServiceDeleteFilesA = new TnefPropertyTag (TnefPropertyId.ServiceDeleteFiles, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_SERVICE_DELETE_FILES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SERVICE_DELETE_FILES.
		/// </remarks>
		public static readonly TnefPropertyTag ServiceDeleteFilesW = new TnefPropertyTag (TnefPropertyId.ServiceDeleteFiles, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_SERVICE_DLL_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SERVICE_DLL_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag ServiceDllNameA = new TnefPropertyTag (TnefPropertyId.ServiceDllName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_SERVICE_DLL_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SERVICE_DLL_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag ServiceDllNameW = new TnefPropertyTag (TnefPropertyId.ServiceDllName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_SERVICE_ENTRY_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SERVICE_ENTRY_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag ServiceEntryName = new TnefPropertyTag (TnefPropertyId.ServiceEntryName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_SERVICE_EXTRA_UIDS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SERVICE_EXTRA_UIDS.
		/// </remarks>
		public static readonly TnefPropertyTag ServiceExtraUids = new TnefPropertyTag (TnefPropertyId.ServiceExtraUids, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_SERVICE_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SERVICE_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag ServiceNameA = new TnefPropertyTag (TnefPropertyId.ServiceName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_SERVICE_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SERVICE_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag ServiceNameW = new TnefPropertyTag (TnefPropertyId.ServiceName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_SERVICES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SERVICES.
		/// </remarks>
		public static readonly TnefPropertyTag Services = new TnefPropertyTag (TnefPropertyId.Services, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_SERVICE_SUPPORT_FILES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SERVICE_SUPPORT_FILES.
		/// </remarks>
		public static readonly TnefPropertyTag ServiceSupportFilesA = new TnefPropertyTag (TnefPropertyId.ServiceSupportFiles, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_SERVICE_SUPPORT_FILES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SERVICE_SUPPORT_FILES.
		/// </remarks>
		public static readonly TnefPropertyTag ServiceSupportFilesW = new TnefPropertyTag (TnefPropertyId.ServiceSupportFiles, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_SERVICE_UID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SERVICE_UID.
		/// </remarks>
		public static readonly TnefPropertyTag ServiceUid = new TnefPropertyTag (TnefPropertyId.ServiceUid, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_SEVEN_BIT_DISPLAY_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SEVEN_BIT_DISPLAY_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag SevenBitDisplayName = new TnefPropertyTag (TnefPropertyId.SevenBitDisplayName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_SMTP_ADDRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SMTP_ADDRESS.
		/// </remarks>
		public static readonly TnefPropertyTag SmtpAddressA = new TnefPropertyTag (TnefPropertyId.SmtpAddress, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_SMTP_ADDRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SMTP_ADDRESS.
		/// </remarks>
		public static readonly TnefPropertyTag SmtpAddressW = new TnefPropertyTag (TnefPropertyId.SmtpAddress, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_SPOOLER_STATUS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SPOOLER_STATUS.
		/// </remarks>
		public static readonly TnefPropertyTag SpoolerStatus = new TnefPropertyTag (TnefPropertyId.SpoolerStatus, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_SPOUSE_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SPOUSE_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag SpouseNameA = new TnefPropertyTag (TnefPropertyId.SpouseName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_SPOUSE_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SPOUSE_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag SpouseNameW = new TnefPropertyTag (TnefPropertyId.SpouseName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_START_DATE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_START_DATE.
		/// </remarks>
		public static readonly TnefPropertyTag StartDate = new TnefPropertyTag (TnefPropertyId.StartDate, TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PR_STATE_OR_PROVINCE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_STATE_OR_PROVINCE.
		/// </remarks>
		public static readonly TnefPropertyTag StateOrProvinceA = new TnefPropertyTag (TnefPropertyId.StateOrProvince, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_STATE_OR_PROVINCE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_STATE_OR_PROVINCE.
		/// </remarks>
		public static readonly TnefPropertyTag StateOrProvinceW = new TnefPropertyTag (TnefPropertyId.StateOrProvince, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_STATUS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_STATUS.
		/// </remarks>
		public static readonly TnefPropertyTag Status = new TnefPropertyTag (TnefPropertyId.Status, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_STATUS_CODE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_STATUS_CODE.
		/// </remarks>
		public static readonly TnefPropertyTag StatusCode = new TnefPropertyTag (TnefPropertyId.StatusCode, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_STATUS_STRING.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_STATUS_STRING.
		/// </remarks>
		public static readonly TnefPropertyTag StatusStringA = new TnefPropertyTag (TnefPropertyId.StatusString, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_STATUS_STRING.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_STATUS_STRING.
		/// </remarks>
		public static readonly TnefPropertyTag StatusStringW = new TnefPropertyTag (TnefPropertyId.StatusString, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_STORE_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_STORE_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag StoreEntryId = new TnefPropertyTag (TnefPropertyId.StoreEntryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_STORE_PROVIDERS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_STORE_PROVIDERS.
		/// </remarks>
		public static readonly TnefPropertyTag StoreProviders = new TnefPropertyTag (TnefPropertyId.StoreProviders, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_STORE_RECORD_KEY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_STORE_RECORD_KEY.
		/// </remarks>
		public static readonly TnefPropertyTag StoreRecordKey = new TnefPropertyTag (TnefPropertyId.StoreRecordKey, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_STORE_STATE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_STORE_STATE.
		/// </remarks>
		public static readonly TnefPropertyTag StoreState = new TnefPropertyTag (TnefPropertyId.StoreState, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_STORE_SUPPORT_MASK.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_STORE_SUPPORT_MASK.
		/// </remarks>
		public static readonly TnefPropertyTag StoreSupportMask = new TnefPropertyTag (TnefPropertyId.StoreSupportMask, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_STREET_ADDRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_STREET_ADDRESS.
		/// </remarks>
		public static readonly TnefPropertyTag StreetAddressA = new TnefPropertyTag (TnefPropertyId.StreetAddress, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_STREET_ADDRESS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_STREET_ADDRESS.
		/// </remarks>
		public static readonly TnefPropertyTag StreetAddressW = new TnefPropertyTag (TnefPropertyId.StreetAddress, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_SUBFOLDERS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SUBFOLDERS.
		/// </remarks>
		public static readonly TnefPropertyTag Subfolders = new TnefPropertyTag (TnefPropertyId.Subfolders, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_SUBJECT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SUBJECT.
		/// </remarks>
		public static readonly TnefPropertyTag SubjectA = new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_SUBJECT.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SUBJECT.
		/// </remarks>
		public static readonly TnefPropertyTag SubjectW = new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_SUBJECT_IPM.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SUBJECT_IPM.
		/// </remarks>
		public static readonly TnefPropertyTag SubjectIpm = new TnefPropertyTag (TnefPropertyId.SubjectIpm, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_SUBJECT_PREFIX.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SUBJECT_PREFIX.
		/// </remarks>
		public static readonly TnefPropertyTag SubjectPrefixA = new TnefPropertyTag (TnefPropertyId.SubjectPrefix, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_SUBJECT_PREFIX.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SUBJECT_PREFIX.
		/// </remarks>
		public static readonly TnefPropertyTag SubjectPrefixW = new TnefPropertyTag (TnefPropertyId.SubjectPrefix, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_SUBMIT_FLAGS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SUBMIT_FLAGS.
		/// </remarks>
		public static readonly TnefPropertyTag SubmitFlags = new TnefPropertyTag (TnefPropertyId.SubmitFlags, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_SUPERSEDES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SUPERSEDES.
		/// </remarks>
		public static readonly TnefPropertyTag SupersedesA = new TnefPropertyTag (TnefPropertyId.Supersedes, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_SUPERSEDES.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SUPERSEDES.
		/// </remarks>
		public static readonly TnefPropertyTag SupersedesW = new TnefPropertyTag (TnefPropertyId.Supersedes, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_SUPPLEMENTARY_INFO.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SUPPLEMENTARY_INFO.
		/// </remarks>
		public static readonly TnefPropertyTag SupplementaryInfoA = new TnefPropertyTag (TnefPropertyId.SupplementaryInfo, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_SUPPLEMENTARY_INFO.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SUPPLEMENTARY_INFO.
		/// </remarks>
		public static readonly TnefPropertyTag SupplementaryInfoW = new TnefPropertyTag (TnefPropertyId.SupplementaryInfo, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_SURNAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SURNAME.
		/// </remarks>
		public static readonly TnefPropertyTag SurnameA = new TnefPropertyTag (TnefPropertyId.Surname, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_SURNAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_SURNAME.
		/// </remarks>
		public static readonly TnefPropertyTag SurnameW = new TnefPropertyTag (TnefPropertyId.Surname, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_TELEX_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_TELEX_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag TelexNumberA = new TnefPropertyTag (TnefPropertyId.TelexNumber, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_TELEX_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_TELEX_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag TelexNumberW = new TnefPropertyTag (TnefPropertyId.TelexNumber, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_TEMPLATEID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_TEMPLATEID.
		/// </remarks>
		public static readonly TnefPropertyTag Templateid = new TnefPropertyTag (TnefPropertyId.Templateid, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_TITLE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_TITLE.
		/// </remarks>
		public static readonly TnefPropertyTag TitleA = new TnefPropertyTag (TnefPropertyId.Title, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_TITLE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_TITLE.
		/// </remarks>
		public static readonly TnefPropertyTag TitleW = new TnefPropertyTag (TnefPropertyId.Title, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_TNEF_CORRELATION_KEY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_TNEF_CORRELATION_KEY.
		/// </remarks>
		public static readonly TnefPropertyTag TnefCorrelationKey = new TnefPropertyTag (TnefPropertyId.TnefCorrelationKey, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_TRANSMITABLE_DISPLAY_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_TRANSMITABLE_DISPLAY_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag TransmitableDisplayNameA = new TnefPropertyTag (TnefPropertyId.TransmitableDisplayName, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_TRANSMITABLE_DISPLAY_NAME.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_TRANSMITABLE_DISPLAY_NAME.
		/// </remarks>
		public static readonly TnefPropertyTag TransmitableDisplayNameW = new TnefPropertyTag (TnefPropertyId.TransmitableDisplayName, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_TRANSPORT_KEY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_TRANSPORT_KEY.
		/// </remarks>
		public static readonly TnefPropertyTag TransportKey = new TnefPropertyTag (TnefPropertyId.TransportKey, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_TRANSPORT_MESSAGE_HEADERS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_TRANSPORT_MESSAGE_HEADERS.
		/// </remarks>
		public static readonly TnefPropertyTag TransportMessageHeadersA = new TnefPropertyTag (TnefPropertyId.TransportMessageHeaders, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_TRANSPORT_MESSAGE_HEADERS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_TRANSPORT_MESSAGE_HEADERS.
		/// </remarks>
		public static readonly TnefPropertyTag TransportMessageHeadersW = new TnefPropertyTag (TnefPropertyId.TransportMessageHeaders, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_TRANSPORT_PROVIDERS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_TRANSPORT_PROVIDERS.
		/// </remarks>
		public static readonly TnefPropertyTag TransportProviders = new TnefPropertyTag (TnefPropertyId.TransportProviders, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_TRANSPORT_STATUS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_TRANSPORT_STATUS.
		/// </remarks>
		public static readonly TnefPropertyTag TransportStatus = new TnefPropertyTag (TnefPropertyId.TransportStatus, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_TTYDD_PHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_TTYDD_PHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag TtytddPhoneNumberA = new TnefPropertyTag (TnefPropertyId.TtytddPhoneNumber, TnefPropertyType.String8);

		/// <summary>
		/// The MAPI property PR_TTYDD_PHONE_NUMBER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_TTYDD_PHONE_NUMBER.
		/// </remarks>
		public static readonly TnefPropertyTag TtytddPhoneNumberW = new TnefPropertyTag (TnefPropertyId.TtytddPhoneNumber, TnefPropertyType.Unicode);

		/// <summary>
		/// The MAPI property PR_TYPE_OF_MTS_USER.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_TYPE_OF_MTS_USER.
		/// </remarks>
		public static readonly TnefPropertyTag TypeOfMtsUser = new TnefPropertyTag (TnefPropertyId.TypeOfMtsUser, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_USER_CERTIFICATE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_USER_CERTIFICATE.
		/// </remarks>
		public static readonly TnefPropertyTag UserCertificate = new TnefPropertyTag (TnefPropertyId.UserCertificate, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_USER_X509_CERTIFICATE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_USER_X509_CERTIFICATE.
		/// </remarks>
		public static readonly TnefPropertyTag UserX509Certificate = new TnefPropertyTag (TnefPropertyId.UserX509Certificate, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_VALID_FOLDER_MASK.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_VALID_FOLDER_MASK.
		/// </remarks>
		public static readonly TnefPropertyTag ValidFolderMask = new TnefPropertyTag (TnefPropertyId.ValidFolderMask, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_VIEWS_ENTRYID.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_VIEWS_ENTRYID.
		/// </remarks>
		public static readonly TnefPropertyTag ViewsEntryId = new TnefPropertyTag (TnefPropertyId.ViewsEntryId, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_WEDDING_ANNIVERSARY.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_WEDDING_ANNIVERSARY.
		/// </remarks>
		public static readonly TnefPropertyTag WeddingAnniversary = new TnefPropertyTag (TnefPropertyId.WeddingAnniversary, TnefPropertyType.SysTime);

		/// <summary>
		/// The MAPI property PR_X400_CONTENT_TYPE.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_X400_CONTENT_TYPE.
		/// </remarks>
		public static readonly TnefPropertyTag X400ContentType = new TnefPropertyTag (TnefPropertyId.X400ContentType, TnefPropertyType.Binary);

		/// <summary>
		/// The MAPI property PR_X400_DEFERRED_DELIVERY_CANCEL.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_X400_DEFERRED_DELIVERY_CANCEL.
		/// </remarks>
		public static readonly TnefPropertyTag X400DeferredDeliveryCancel = new TnefPropertyTag (TnefPropertyId.X400DeferredDeliveryCancel, TnefPropertyType.Boolean);

		/// <summary>
		/// The MAPI property PR_XPOS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_XPOS.
		/// </remarks>
		public static readonly TnefPropertyTag Xpos = new TnefPropertyTag (TnefPropertyId.Xpos, TnefPropertyType.Long);

		/// <summary>
		/// The MAPI property PR_YPOS.
		/// </summary>
		/// <remarks>
		/// The MAPI property PR_YPOS.
		/// </remarks>
		public static readonly TnefPropertyTag Ypos = new TnefPropertyTag (TnefPropertyId.Ypos, TnefPropertyType.Long);

		const TnefPropertyId NamedMin = unchecked ((TnefPropertyId) 0x8000);
		const TnefPropertyId NamedMax = unchecked ((TnefPropertyId) 0xFFFE);
		const short MultiValuedFlag = (short) TnefPropertyType.MultiValued;
		readonly TnefPropertyType type;
		readonly TnefPropertyId id;

		/// <summary>
		/// Gets the property identifier.
		/// </summary>
		/// <remarks>
		/// Gets the property identifier.
		/// </remarks>
		/// <value>The identifier.</value>
		public TnefPropertyId Id {
			get { return id; }
		}

		/// <summary>
		/// Gets a value indicating whether or not the property contains multiple values.
		/// </summary>
		/// <remarks>
		/// Gets a value indicating whether or not the property contains multiple values.
		/// </remarks>
		/// <value><c>true</c> if the property contains multiple values; otherwise, <c>false</c>.</value>
		public bool IsMultiValued {
			get { return (((short) type) & MultiValuedFlag) != 0; }
		}

		/// <summary>
		/// Gets a value indicating whether or not the property has a special name.
		/// </summary>
		/// <remarks>
		/// Gets a value indicating whether or not the property has a special name.
		/// </remarks>
		/// <value><c>true</c> if the property has a special name; otherwise, <c>false</c>.</value>
		public bool IsNamed {
			get { return id >= NamedMin && id <= NamedMax; }
		}

		/// <summary>
		/// Gets a value indicating whether the property value type is valid.
		/// </summary>
		/// <remarks>
		/// Gets a value indicating whether the property value type is valid.
		/// </remarks>
		/// <value><c>true</c> if the property value type is valid; otherwise, <c>false</c>.</value>
		public bool IsTnefTypeValid {
			get {
				switch (ValueTnefType) {
				case TnefPropertyType.Unspecified:
				case TnefPropertyType.Null:
				case TnefPropertyType.I2:
				case TnefPropertyType.Long:
				case TnefPropertyType.R4:
				case TnefPropertyType.Double:
				case TnefPropertyType.Currency:
				case TnefPropertyType.AppTime:
				case TnefPropertyType.Error:
				case TnefPropertyType.Boolean:
				case TnefPropertyType.Object:
				case TnefPropertyType.I8:
				case TnefPropertyType.String8:
				case TnefPropertyType.Unicode:
				case TnefPropertyType.SysTime:
				case TnefPropertyType.ClassId:
				case TnefPropertyType.Binary:
					return true;
				default:
					return false;
				}
			}
		}

		/// <summary>
		/// Gets the property's value type (including the multi-valued bit).
		/// </summary>
		/// <remarks>
		/// Gets the property's value type (including the multi-valued bit).
		/// </remarks>
		/// <value>The property's value type.</value>
		public TnefPropertyType TnefType {
			get { return type; }
		}

		/// <summary>
		/// Gets the type of the value that the property contains.
		/// </summary>
		/// <remarks>
		/// Gets the type of the value that the property contains.
		/// </remarks>
		/// <value>The type of the value.</value>
		public TnefPropertyType ValueTnefType {
			get { return (TnefPropertyType) (((short) type) & ~MultiValuedFlag); }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Tnef.TnefPropertyTag"/> struct.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="TnefPropertyTag"/> based on a 32-bit integer tag as read from
		/// a TNEF stream.
		/// </remarks>
		/// <param name="tag">The property tag.</param>
		public TnefPropertyTag (int tag)
		{
			type = (TnefPropertyType) ((tag >> 16) & 0xFFFF);
			id = (TnefPropertyId) (tag & 0xFFFF);
		}

		TnefPropertyTag (TnefPropertyId id, TnefPropertyType type, bool multiValue)
		{
			this.type = (TnefPropertyType) (((ushort) type) | (multiValue ? MultiValuedFlag : 0));
			this.id = id;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Tnef.TnefPropertyTag"/> struct.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="TnefPropertyTag"/> based on a <see cref="TnefPropertyId"/>
		/// and <see cref="TnefPropertyType"/>.
		/// </remarks>
		/// <param name="id">The property identifier.</param>
		/// <param name="type">The property type.</param>
		public TnefPropertyTag (TnefPropertyId id, TnefPropertyType type)
		{
			this.type = type;
			this.id = id;
		}

		/// <summary>
		/// Casts an integer tag value into a TNEF property tag.
		/// </summary>
		/// <remarks>
		/// Casts an integer tag value into a TNEF property tag.
		/// </remarks>
		/// <returns>A <see cref="TnefPropertyTag"/> that represents the integer tag value.</returns>
		/// <param name="tag">The integer tag value.</param>
		public static implicit operator TnefPropertyTag (int tag)
		{
			return new TnefPropertyTag (tag);
		}

		/// <summary>
		/// Casts a TNEF property tag into a 32-bit integer value.
		/// </summary>
		/// <remarks>
		/// Casts a TNEF property tag into a 32-bit integer value.
		/// </remarks>
		/// <returns>A 32-bit integer value representing the TNEF property tag.</returns>
		/// <param name="tag">The TNEF property tag.</param>
		public static implicit operator int (TnefPropertyTag tag)
		{
			return (((ushort) tag.TnefType) << 16) | ((ushort) tag.Id);
		}

		/// <summary>
		/// Serves as a hash function for a <see cref="MimeKit.Tnef.TnefPropertyTag"/> object.
		/// </summary>
		/// <remarks>
		/// Serves as a hash function for a <see cref="MimeKit.Tnef.TnefPropertyTag"/> object.
		/// </remarks>
		/// <returns>A hash code for this instance that is suitable for use in hashing algorithms
		/// and data structures such as a hash table.</returns>
		public override int GetHashCode ()
		{
			return ((int) this).GetHashCode ();
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="MimeKit.Tnef.TnefPropertyTag"/>.
		/// </summary>
		/// <remarks>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="MimeKit.Tnef.TnefPropertyTag"/>.
		/// </remarks>
		/// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="MimeKit.Tnef.TnefPropertyTag"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="MimeKit.Tnef.TnefPropertyTag"/>; otherwise, <c>false</c>.</returns>
		public override bool Equals (object obj)
		{
			if (!(obj is TnefPropertyTag))
				return false;

			var tag = (TnefPropertyTag) obj;

			return tag.Id == Id && tag.TnefType == TnefType;
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="MimeKit.Tnef.TnefPropertyTag"/>.
		/// </summary>
		/// <remarks>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="MimeKit.Tnef.TnefPropertyTag"/>.
		/// </remarks>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="MimeKit.Tnef.TnefPropertyTag"/>.</returns>
		public override string ToString ()
		{
			return string.Format ("{0} ({1})", Id, ValueTnefType);
		}

		/// <summary>
		/// Returns a new <see cref="TnefPropertyTag"/> where the type has been changed to <see cref="TnefPropertyType.Unicode"/>.
		/// </summary>
		/// <remarks>
		/// Returns a new <see cref="TnefPropertyTag"/> where the type has been changed to <see cref="TnefPropertyType.Unicode"/>.
		/// </remarks>
		/// <returns>The unicode equivalent of the property tag.</returns>
		public TnefPropertyTag ToUnicode ()
		{
			var unicode = (TnefPropertyType) ((((short) type) & MultiValuedFlag) | ((short) TnefPropertyType.Unicode));

			return new TnefPropertyTag (id, unicode);
		}
	}
}

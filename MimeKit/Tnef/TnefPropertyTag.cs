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

		//public static readonly TnefPropertyTag BusinessAddressPostOfficeBox = new TnefPropertyTag (TnefPropertyId.BusinessAddressPostOfficeBox, TnefPropertyType.Unspecified);
		//public static readonly TnefPropertyTag BusinessAddressStateOrProvince = new TnefPropertyTag (TnefPropertyId.BusinessAddressStateOrProvince, TnefPropertyType.Unspecified);

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
		public static readonly TnefPropertyTag DetailsTable = new TnefPropertyTag (TnefPropertyId.DetailsTable, TnefPropertyType.Object);
		public static readonly TnefPropertyTag DiscardReason = new TnefPropertyTag (TnefPropertyId.DiscardReason, TnefPropertyType.Long);
		public static readonly TnefPropertyTag DiscloseRecipients = new TnefPropertyTag (TnefPropertyId.DiscloseRecipients, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag DisclosureOfRecipients = new TnefPropertyTag (TnefPropertyId.DisclosureOfRecipients, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag DiscreteValues = new TnefPropertyTag (TnefPropertyId.DiscreteValues, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag DiscVal = new TnefPropertyTag (TnefPropertyId.DiscVal, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag DisplayBccA = new TnefPropertyTag (TnefPropertyId.DisplayBcc, TnefPropertyType.String8);
		public static readonly TnefPropertyTag DisplayBccW = new TnefPropertyTag (TnefPropertyId.DisplayBcc, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag DisplayCcA = new TnefPropertyTag (TnefPropertyId.DisplayCc, TnefPropertyType.String8);
		public static readonly TnefPropertyTag DisplayCcW = new TnefPropertyTag (TnefPropertyId.DisplayCc, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag DisplayNameA = new TnefPropertyTag (TnefPropertyId.DisplayName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag DisplayNameW = new TnefPropertyTag (TnefPropertyId.DisplayName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag DisplayNamePrefixA = new TnefPropertyTag (TnefPropertyId.DisplayNamePrefix, TnefPropertyType.String8);
		public static readonly TnefPropertyTag DisplayNamePrefixW = new TnefPropertyTag (TnefPropertyId.DisplayNamePrefix, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag DisplayToA = new TnefPropertyTag (TnefPropertyId.DisplayTo, TnefPropertyType.String8);
		public static readonly TnefPropertyTag DisplayToW = new TnefPropertyTag (TnefPropertyId.DisplayTo, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag DisplayType = new TnefPropertyTag (TnefPropertyId.DisplayType, TnefPropertyType.Long);
		public static readonly TnefPropertyTag DlExpansionHistory = new TnefPropertyTag (TnefPropertyId.DlExpansionHistory, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag DlExpansionProhibited = new TnefPropertyTag (TnefPropertyId.DlExpansionProhibited, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag EmailAddressA = new TnefPropertyTag (TnefPropertyId.EmailAddress, TnefPropertyType.String8);
		public static readonly TnefPropertyTag EmailAddressW = new TnefPropertyTag (TnefPropertyId.EmailAddress, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag EndDate = new TnefPropertyTag (TnefPropertyId.EndDate, TnefPropertyType.SysTime);
		public static readonly TnefPropertyTag EntryId = new TnefPropertyTag (TnefPropertyId.EntryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ExpandBeginTime = new TnefPropertyTag (TnefPropertyId.ExpandBeginTime, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ExpandedBeginTime = new TnefPropertyTag (TnefPropertyId.ExpandedBeginTime, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ExpandedEndTime = new TnefPropertyTag (TnefPropertyId.ExpandedEndTime, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ExpandEndTime = new TnefPropertyTag (TnefPropertyId.ExpandEndTime, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ExpiryTime = new TnefPropertyTag (TnefPropertyId.ExpiryTime, TnefPropertyType.SysTime);
		public static readonly TnefPropertyTag ExplicitConversion = new TnefPropertyTag (TnefPropertyId.ExplicitConversion, TnefPropertyType.Long);
		public static readonly TnefPropertyTag FilteringHooks = new TnefPropertyTag (TnefPropertyId.FilteringHooks, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag FinderEntryId = new TnefPropertyTag (TnefPropertyId.FinderEntryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag FolderAssociatedContents = new TnefPropertyTag (TnefPropertyId.FolderAssociatedContents, TnefPropertyType.Object);
		public static readonly TnefPropertyTag FolderType = new TnefPropertyTag (TnefPropertyId.FolderType, TnefPropertyType.Long);
		public static readonly TnefPropertyTag FormCategoryA = new TnefPropertyTag (TnefPropertyId.FormCategory, TnefPropertyType.String8);
		public static readonly TnefPropertyTag FormCategoryW = new TnefPropertyTag (TnefPropertyId.FormCategory, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag FormCategorySubA = new TnefPropertyTag (TnefPropertyId.FormCategorySub, TnefPropertyType.String8);
		public static readonly TnefPropertyTag FormCategorySubW = new TnefPropertyTag (TnefPropertyId.FormCategorySub, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag FormClsid = new TnefPropertyTag (TnefPropertyId.FormClsid, TnefPropertyType.ClassId);
		public static readonly TnefPropertyTag FormContactNameA = new TnefPropertyTag (TnefPropertyId.FormContactName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag FormContactNameW = new TnefPropertyTag (TnefPropertyId.FormContactName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag FormDesignerGuid = new TnefPropertyTag (TnefPropertyId.FormDesignerGuid, TnefPropertyType.ClassId);
		public static readonly TnefPropertyTag FormDesignerNameA = new TnefPropertyTag (TnefPropertyId.FormDesignerName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag FormDesignerNameW = new TnefPropertyTag (TnefPropertyId.FormDesignerName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag FormHidden = new TnefPropertyTag (TnefPropertyId.FormHidden, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag FormHostMap = new TnefPropertyTag (TnefPropertyId.FormHostMap, TnefPropertyType.Long, true);
		public static readonly TnefPropertyTag FormMessageBehavior = new TnefPropertyTag (TnefPropertyId.FormMessageBehavior, TnefPropertyType.Long);
		public static readonly TnefPropertyTag FormVersionA = new TnefPropertyTag (TnefPropertyId.FormVersion, TnefPropertyType.String8);
		public static readonly TnefPropertyTag FormVersionW = new TnefPropertyTag (TnefPropertyId.FormVersion, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag FtpSiteA = new TnefPropertyTag (TnefPropertyId.FtpSite, TnefPropertyType.String8);
		public static readonly TnefPropertyTag FtpSiteW = new TnefPropertyTag (TnefPropertyId.FtpSite, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag Gender = new TnefPropertyTag (TnefPropertyId.Gender, TnefPropertyType.I2);
		public static readonly TnefPropertyTag GenerationA = new TnefPropertyTag (TnefPropertyId.Generation, TnefPropertyType.String8);
		public static readonly TnefPropertyTag GenerationW = new TnefPropertyTag (TnefPropertyId.Generation, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag GivenNameA = new TnefPropertyTag (TnefPropertyId.GivenName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag GivenNameW = new TnefPropertyTag (TnefPropertyId.GivenName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag GovernmentIdNumberA = new TnefPropertyTag (TnefPropertyId.GovernmentIdNumber, TnefPropertyType.String8);
		public static readonly TnefPropertyTag GovernmentIdNumberW = new TnefPropertyTag (TnefPropertyId.GovernmentIdNumber, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag Hasattach = new TnefPropertyTag (TnefPropertyId.Hasattach, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag HeaderFolderEntryId = new TnefPropertyTag (TnefPropertyId.HeaderFolderEntryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag HobbiesA = new TnefPropertyTag (TnefPropertyId.Hobbies, TnefPropertyType.String8);
		public static readonly TnefPropertyTag HobbiesW = new TnefPropertyTag (TnefPropertyId.Hobbies, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag Home2TelephoneNumberA = new TnefPropertyTag (TnefPropertyId.Home2TelephoneNumber, TnefPropertyType.String8);
		public static readonly TnefPropertyTag Home2TelephoneNumberAMv = new TnefPropertyTag (TnefPropertyId.Home2TelephoneNumber, TnefPropertyType.String8, true);
		public static readonly TnefPropertyTag Home2TelephoneNumberW = new TnefPropertyTag (TnefPropertyId.Home2TelephoneNumber, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag Home2TelephoneNumberWMv = new TnefPropertyTag (TnefPropertyId.Home2TelephoneNumber, TnefPropertyType.Unicode, true);
		public static readonly TnefPropertyTag HomeAddressCityA = new TnefPropertyTag (TnefPropertyId.HomeAddressCity, TnefPropertyType.String8);
		public static readonly TnefPropertyTag HomeAddressCityW = new TnefPropertyTag (TnefPropertyId.HomeAddressCity, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag HomeAddressCountryA = new TnefPropertyTag (TnefPropertyId.HomeAddressCountry, TnefPropertyType.String8);
		public static readonly TnefPropertyTag HomeAddressCountryW = new TnefPropertyTag (TnefPropertyId.HomeAddressCountry, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag HomeAddressPostalCodeA = new TnefPropertyTag (TnefPropertyId.HomeAddressPostalCode, TnefPropertyType.String8);
		public static readonly TnefPropertyTag HomeAddressPostalCodeW = new TnefPropertyTag (TnefPropertyId.HomeAddressPostalCode, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag HomeAddressPostOfficeBoxA = new TnefPropertyTag (TnefPropertyId.HomeAddressPostOfficeBox, TnefPropertyType.String8);
		public static readonly TnefPropertyTag HomeAddressPostOfficeBoxW = new TnefPropertyTag (TnefPropertyId.HomeAddressPostOfficeBox, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag HomeAddressStateOrProvinceA = new TnefPropertyTag (TnefPropertyId.HomeAddressStateOrProvince, TnefPropertyType.String8);
		public static readonly TnefPropertyTag HomeAddressStateOrProvinceW = new TnefPropertyTag (TnefPropertyId.HomeAddressStateOrProvince, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag HomeAddressStreetA = new TnefPropertyTag (TnefPropertyId.HomeAddressStreet, TnefPropertyType.String8);
		public static readonly TnefPropertyTag HomeAddressStreetW = new TnefPropertyTag (TnefPropertyId.HomeAddressStreet, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag HomeFaxNumberA = new TnefPropertyTag (TnefPropertyId.HomeFaxNumber, TnefPropertyType.String8);
		public static readonly TnefPropertyTag HomeFaxNumberW = new TnefPropertyTag (TnefPropertyId.HomeFaxNumber, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag HomeTelephoneNumberA = new TnefPropertyTag (TnefPropertyId.HomeTelephoneNumber, TnefPropertyType.String8);
		public static readonly TnefPropertyTag HomeTelephoneNumberW = new TnefPropertyTag (TnefPropertyId.HomeTelephoneNumber, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag Icon = new TnefPropertyTag (TnefPropertyId.Icon, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag IdentityDisplayA = new TnefPropertyTag (TnefPropertyId.IdentityDisplay, TnefPropertyType.String8);
		public static readonly TnefPropertyTag IdentityDisplayW = new TnefPropertyTag (TnefPropertyId.IdentityDisplay, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag IdentityEntryId = new TnefPropertyTag (TnefPropertyId.IdentityEntryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag IdentitySearchKey = new TnefPropertyTag (TnefPropertyId.IdentitySearchKey, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ImplicitConversionProhibited = new TnefPropertyTag (TnefPropertyId.ImplicitConversionProhibited, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag Importance = new TnefPropertyTag (TnefPropertyId.Importance, TnefPropertyType.Long);
		public static readonly TnefPropertyTag IncompleteCopy = new TnefPropertyTag (TnefPropertyId.IncompleteCopy, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag INetMailOverrideCharset = new TnefPropertyTag (TnefPropertyId.INetMailOverrideCharset, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag INetMailOverrideFormat = new TnefPropertyTag (TnefPropertyId.INetMailOverrideFormat, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag InitialDetailsPane = new TnefPropertyTag (TnefPropertyId.InitialDetailsPane, TnefPropertyType.Long);
		public static readonly TnefPropertyTag InitialsA = new TnefPropertyTag (TnefPropertyId.Initials, TnefPropertyType.String8);
		public static readonly TnefPropertyTag InitialsW = new TnefPropertyTag (TnefPropertyId.Initials, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag InReplyToIdA = new TnefPropertyTag (TnefPropertyId.InReplyToId, TnefPropertyType.String8);
		public static readonly TnefPropertyTag InReplyToIdW = new TnefPropertyTag (TnefPropertyId.InReplyToId, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag InstanceKey = new TnefPropertyTag (TnefPropertyId.InstanceKey, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag InternetApprovedA = new TnefPropertyTag (TnefPropertyId.InternetApproved, TnefPropertyType.String8);
		public static readonly TnefPropertyTag InternetApprovedW = new TnefPropertyTag (TnefPropertyId.InternetApproved, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag InternetArticleNumber = new TnefPropertyTag (TnefPropertyId.InternetArticleNumber, TnefPropertyType.Long);
		public static readonly TnefPropertyTag InternetControlA = new TnefPropertyTag (TnefPropertyId.InternetControl, TnefPropertyType.String8);
		public static readonly TnefPropertyTag InternetControlW = new TnefPropertyTag (TnefPropertyId.InternetControl, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag InternetCPID = new TnefPropertyTag (TnefPropertyId.InternetCPID, TnefPropertyType.Long);
		public static readonly TnefPropertyTag InternetDistributionA = new TnefPropertyTag (TnefPropertyId.InternetDistribution, TnefPropertyType.String8);
		public static readonly TnefPropertyTag InternetDistributionW = new TnefPropertyTag (TnefPropertyId.InternetDistribution, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag InternetFollowupToA = new TnefPropertyTag (TnefPropertyId.InternetFollowupTo, TnefPropertyType.String8);
		public static readonly TnefPropertyTag InternetFollowupToW = new TnefPropertyTag (TnefPropertyId.InternetFollowupTo, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag InternetLines = new TnefPropertyTag (TnefPropertyId.InternetLines, TnefPropertyType.Long);
		public static readonly TnefPropertyTag InternetMessageIdA = new TnefPropertyTag (TnefPropertyId.InternetMessageId, TnefPropertyType.String8);
		public static readonly TnefPropertyTag InternetMessageIdW = new TnefPropertyTag (TnefPropertyId.InternetMessageId, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag InternetNewsgroupsA = new TnefPropertyTag (TnefPropertyId.InternetNewsgroups, TnefPropertyType.String8);
		public static readonly TnefPropertyTag InternetNewsgroupsW = new TnefPropertyTag (TnefPropertyId.InternetNewsgroups, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag InternetNntpPathA = new TnefPropertyTag (TnefPropertyId.InternetNntpPath, TnefPropertyType.String8);
		public static readonly TnefPropertyTag InternetNntpPathW = new TnefPropertyTag (TnefPropertyId.InternetNntpPath, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag InternetOrganizationA = new TnefPropertyTag (TnefPropertyId.InternetOrganization, TnefPropertyType.String8);
		public static readonly TnefPropertyTag InternetOrganizationW = new TnefPropertyTag (TnefPropertyId.InternetOrganization, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag InternetPrecedenceA = new TnefPropertyTag (TnefPropertyId.InternetPrecedence, TnefPropertyType.String8);
		public static readonly TnefPropertyTag InternetPrecedenceW = new TnefPropertyTag (TnefPropertyId.InternetPrecedence, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag InternetReferencesA = new TnefPropertyTag (TnefPropertyId.InternetReferences, TnefPropertyType.String8);
		public static readonly TnefPropertyTag InternetReferencesW = new TnefPropertyTag (TnefPropertyId.InternetReferences, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag IpmId = new TnefPropertyTag (TnefPropertyId.IpmId, TnefPropertyType.Long);
		public static readonly TnefPropertyTag IpmOutboxEntryId = new TnefPropertyTag (TnefPropertyId.IpmOutboxEntryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag IpmOutboxSearchKey = new TnefPropertyTag (TnefPropertyId.IpmOutboxSearchKey, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag IpmReturnRequested = new TnefPropertyTag (TnefPropertyId.IpmReturnRequested, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag IpmSentmailEntryId = new TnefPropertyTag (TnefPropertyId.IpmSentmailEntryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag IpmSentmailSearchKey = new TnefPropertyTag (TnefPropertyId.IpmSentmailSearchKey, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag IpmSubtreeEntryId = new TnefPropertyTag (TnefPropertyId.IpmSubtreeEntryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag IpmSubtreeSearchKey = new TnefPropertyTag (TnefPropertyId.IpmSubtreeSearchKey, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag IpmWastebasketEntryId = new TnefPropertyTag (TnefPropertyId.IpmWastebasketEntryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag IpmWastebasketSearchKey = new TnefPropertyTag (TnefPropertyId.IpmWastebasketSearchKey, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag IsdnNumberA = new TnefPropertyTag (TnefPropertyId.IsdnNumber, TnefPropertyType.String8);
		public static readonly TnefPropertyTag IsdnNumberW = new TnefPropertyTag (TnefPropertyId.IsdnNumber, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag KeywordA = new TnefPropertyTag (TnefPropertyId.Keyword, TnefPropertyType.String8);
		public static readonly TnefPropertyTag KeywordW = new TnefPropertyTag (TnefPropertyId.Keyword, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag LanguageA = new TnefPropertyTag (TnefPropertyId.Language, TnefPropertyType.String8);
		public static readonly TnefPropertyTag LanguageW = new TnefPropertyTag (TnefPropertyId.Language, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag LanguagesA = new TnefPropertyTag (TnefPropertyId.Languages, TnefPropertyType.String8);
		public static readonly TnefPropertyTag LanguagesW = new TnefPropertyTag (TnefPropertyId.Languages, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag LastModificationTime = new TnefPropertyTag (TnefPropertyId.LastModificationTime, TnefPropertyType.SysTime);
		public static readonly TnefPropertyTag LatestDeliveryTime = new TnefPropertyTag (TnefPropertyId.LatestDeliveryTime, TnefPropertyType.SysTime);
		public static readonly TnefPropertyTag ListHelpA = new TnefPropertyTag (TnefPropertyId.ListHelp, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ListHelpW = new TnefPropertyTag (TnefPropertyId.ListHelp, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag ListSubscribeA = new TnefPropertyTag (TnefPropertyId.ListSubscribe, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ListSubscribeW = new TnefPropertyTag (TnefPropertyId.ListSubscribe, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag ListUnsubscribeA = new TnefPropertyTag (TnefPropertyId.ListUnsubscribe, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ListUnsubscribeW = new TnefPropertyTag (TnefPropertyId.ListUnsubscribe, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag LocalityA = new TnefPropertyTag (TnefPropertyId.Locality, TnefPropertyType.String8);
		public static readonly TnefPropertyTag LocalityW = new TnefPropertyTag (TnefPropertyId.Locality, TnefPropertyType.Unicode);
		//public static readonly TnefPropertyTag LocallyDelivered = new TnefPropertyTag (TnefPropertyId.LocallyDelivered, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag LocationA = new TnefPropertyTag (TnefPropertyId.Location, TnefPropertyType.String8);
		public static readonly TnefPropertyTag LocationW = new TnefPropertyTag (TnefPropertyId.Location, TnefPropertyType.Unicode);
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
		public static readonly TnefPropertyTag MailPermission = new TnefPropertyTag (TnefPropertyId.MailPermission, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag ManagerNameA = new TnefPropertyTag (TnefPropertyId.ManagerName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ManagerNameW = new TnefPropertyTag (TnefPropertyId.ManagerName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag MappingSignature = new TnefPropertyTag (TnefPropertyId.MappingSignature, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag MdbProvider = new TnefPropertyTag (TnefPropertyId.MdbProvider, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag MessageAttachments = new TnefPropertyTag (TnefPropertyId.MessageAttachments, TnefPropertyType.Object);
		public static readonly TnefPropertyTag MessageCcMe = new TnefPropertyTag (TnefPropertyId.MessageCcMe, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag MessageClassA = new TnefPropertyTag (TnefPropertyId.MessageClass, TnefPropertyType.String8);
		public static readonly TnefPropertyTag MessageClassW = new TnefPropertyTag (TnefPropertyId.MessageClass, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag MessageCodepage = new TnefPropertyTag (TnefPropertyId.MessageCodepage, TnefPropertyType.Long);
		public static readonly TnefPropertyTag MessageDeliveryId = new TnefPropertyTag (TnefPropertyId.MessageDeliveryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag MessageDeliveryTime = new TnefPropertyTag (TnefPropertyId.MessageDeliveryTime, TnefPropertyType.SysTime);
		public static readonly TnefPropertyTag MessageDownloadTime = new TnefPropertyTag (TnefPropertyId.MessageDownloadTime, TnefPropertyType.Long);
		public static readonly TnefPropertyTag MessageFlags = new TnefPropertyTag (TnefPropertyId.MessageFlags, TnefPropertyType.Long);
		public static readonly TnefPropertyTag MessageRecipients = new TnefPropertyTag (TnefPropertyId.MessageRecipients, TnefPropertyType.Object);
		public static readonly TnefPropertyTag MessageRecipMe = new TnefPropertyTag (TnefPropertyId.MessageRecipMe, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag MessageSecurityLabel = new TnefPropertyTag (TnefPropertyId.MessageSecurityLabel, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag MessageSize = new TnefPropertyTag (TnefPropertyId.MessageSize, TnefPropertyType.Long);
		public static readonly TnefPropertyTag MessageSubmissionId = new TnefPropertyTag (TnefPropertyId.MessageSubmissionId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag MessageToken = new TnefPropertyTag (TnefPropertyId.MessageToken, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag MessageToMe = new TnefPropertyTag (TnefPropertyId.MessageToMe, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag MhsCommonNameA = new TnefPropertyTag (TnefPropertyId.MhsCommonName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag MhsCommonNameW = new TnefPropertyTag (TnefPropertyId.MhsCommonName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag MiddleNameA = new TnefPropertyTag (TnefPropertyId.MiddleName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag MiddleNameW = new TnefPropertyTag (TnefPropertyId.MiddleName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag MiniIcon = new TnefPropertyTag (TnefPropertyId.MiniIcon, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag MobileTelephoneNumberA = new TnefPropertyTag (TnefPropertyId.MobileTelephoneNumber, TnefPropertyType.String8);
		public static readonly TnefPropertyTag MobileTelephoneNumberW = new TnefPropertyTag (TnefPropertyId.MobileTelephoneNumber, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag ModifyVersion = new TnefPropertyTag (TnefPropertyId.ModifyVersion, TnefPropertyType.I8);
		public static readonly TnefPropertyTag MsgStatus = new TnefPropertyTag (TnefPropertyId.MsgStatus, TnefPropertyType.Long);
		public static readonly TnefPropertyTag NdrDiagCode = new TnefPropertyTag (TnefPropertyId.NdrDiagCode, TnefPropertyType.Long);
		public static readonly TnefPropertyTag NdrReasonCode = new TnefPropertyTag (TnefPropertyId.NdrReasonCode, TnefPropertyType.Long);
		public static readonly TnefPropertyTag NdrStatusCode = new TnefPropertyTag (TnefPropertyId.NdrStatusCode, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag NewsgroupNameA = new TnefPropertyTag (TnefPropertyId.NewsgroupName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag NewsgroupNameW = new TnefPropertyTag (TnefPropertyId.NewsgroupName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag NicknameA = new TnefPropertyTag (TnefPropertyId.Nickname, TnefPropertyType.String8);
		public static readonly TnefPropertyTag NicknameW = new TnefPropertyTag (TnefPropertyId.Nickname, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag NntpXrefA = new TnefPropertyTag (TnefPropertyId.NntpXref, TnefPropertyType.String8);
		public static readonly TnefPropertyTag NntpXrefW = new TnefPropertyTag (TnefPropertyId.NntpXref, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag NonReceiptNotificationRequested = new TnefPropertyTag (TnefPropertyId.NonReceiptNotificationRequested, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag NonReceiptReason = new TnefPropertyTag (TnefPropertyId.NonReceiptReason, TnefPropertyType.Long);
		public static readonly TnefPropertyTag NormalizedSubjectA = new TnefPropertyTag (TnefPropertyId.NormalizedSubject, TnefPropertyType.String8);
		public static readonly TnefPropertyTag NormalizedSubjectW = new TnefPropertyTag (TnefPropertyId.NormalizedSubject, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag NtSecurityDescriptor = new TnefPropertyTag (TnefPropertyId.NtSecurityDescriptor, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag Null = new TnefPropertyTag (TnefPropertyId.Null, TnefPropertyType.Long);
		public static readonly TnefPropertyTag ObjectType = new TnefPropertyTag (TnefPropertyId.ObjectType, TnefPropertyType.Long);
		public static readonly TnefPropertyTag ObsoletedIpms = new TnefPropertyTag (TnefPropertyId.ObsoletedIpms, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag Office2TelephoneNumberA = new TnefPropertyTag (TnefPropertyId.Office2TelephoneNumber, TnefPropertyType.String8);
		public static readonly TnefPropertyTag Office2TelephoneNumberW = new TnefPropertyTag (TnefPropertyId.Office2TelephoneNumber, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OfficeLocationA = new TnefPropertyTag (TnefPropertyId.OfficeLocation, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OfficeLocationW = new TnefPropertyTag (TnefPropertyId.OfficeLocation, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OfficeTelephoneNumberA = new TnefPropertyTag (TnefPropertyId.OfficeTelephoneNumber, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OfficeTelephoneNumberW = new TnefPropertyTag (TnefPropertyId.OfficeTelephoneNumber, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OofReplyType = new TnefPropertyTag (TnefPropertyId.OofReplyType, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OrganizationalIdNumberA = new TnefPropertyTag (TnefPropertyId.OrganizationalIdNumber, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OrganizationalIdNumberW = new TnefPropertyTag (TnefPropertyId.OrganizationalIdNumber, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OrigEntryId = new TnefPropertyTag (TnefPropertyId.OrigEntryId, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag OriginalAuthorAddrtypeA = new TnefPropertyTag (TnefPropertyId.OriginalAuthorAddrtype, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OriginalAuthorAddrtypeW = new TnefPropertyTag (TnefPropertyId.OriginalAuthorAddrtype, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OriginalAuthorEmailAddressA = new TnefPropertyTag (TnefPropertyId.OriginalAuthorEmailAddress, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OriginalAuthorEmailAddressW = new TnefPropertyTag (TnefPropertyId.OriginalAuthorEmailAddress, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OriginalAuthorEntryId = new TnefPropertyTag (TnefPropertyId.OriginalAuthorEntryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag OriginalAuthorNameA = new TnefPropertyTag (TnefPropertyId.OriginalAuthorName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OriginalAuthorNameW = new TnefPropertyTag (TnefPropertyId.OriginalAuthorName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OriginalAuthorSearchKey = new TnefPropertyTag (TnefPropertyId.OriginalAuthorSearchKey, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag OriginalDeliveryTime = new TnefPropertyTag (TnefPropertyId.OriginalDeliveryTime, TnefPropertyType.SysTime);
		public static readonly TnefPropertyTag OriginalDisplayBccA = new TnefPropertyTag (TnefPropertyId.OriginalDisplayBcc, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OriginalDisplayBccW = new TnefPropertyTag (TnefPropertyId.OriginalDisplayBcc, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OriginalDisplayCcA = new TnefPropertyTag (TnefPropertyId.OriginalDisplayCc, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OriginalDisplayCcW = new TnefPropertyTag (TnefPropertyId.OriginalDisplayCc, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OriginalDisplayNameA = new TnefPropertyTag (TnefPropertyId.OriginalDisplayName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OriginalDisplayNameW = new TnefPropertyTag (TnefPropertyId.OriginalDisplayName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OriginalDisplayToA = new TnefPropertyTag (TnefPropertyId.OriginalDisplayTo, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OriginalDisplayToW = new TnefPropertyTag (TnefPropertyId.OriginalDisplayTo, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OriginalEits = new TnefPropertyTag (TnefPropertyId.OriginalEits, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag OriginalEntryId = new TnefPropertyTag (TnefPropertyId.OriginalEntryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag OriginallyIntendedRecipAddrtypeA = new TnefPropertyTag (TnefPropertyId.OriginallyIntendedRecipAddrtype, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OriginallyIntendedRecipAddrtypeW = new TnefPropertyTag (TnefPropertyId.OriginallyIntendedRecipAddrtype, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OriginallyIntendedRecipEmailAddressA = new TnefPropertyTag (TnefPropertyId.OriginallyIntendedRecipEmailAddress, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OriginallyIntendedRecipEmailAddressW = new TnefPropertyTag (TnefPropertyId.OriginallyIntendedRecipEmailAddress, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OriginallyIntendedRecipEntryId = new TnefPropertyTag (TnefPropertyId.OriginallyIntendedRecipEntryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag OriginallyIntendedRecipientName = new TnefPropertyTag (TnefPropertyId.OriginallyIntendedRecipientName, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag OriginalSearchKey = new TnefPropertyTag (TnefPropertyId.OriginalSearchKey, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag OriginalSenderAddrtypeA = new TnefPropertyTag (TnefPropertyId.OriginalSenderAddrtype, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OriginalSenderAddrtypeW = new TnefPropertyTag (TnefPropertyId.OriginalSenderAddrtype, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OriginalSenderEmailAddressA = new TnefPropertyTag (TnefPropertyId.OriginalSenderEmailAddress, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OriginalSenderEmailAddressW = new TnefPropertyTag (TnefPropertyId.OriginalSenderEmailAddress, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OriginalSenderEntryId = new TnefPropertyTag (TnefPropertyId.OriginalSenderEntryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag OriginalSenderNameA = new TnefPropertyTag (TnefPropertyId.OriginalSenderName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OriginalSenderNameW = new TnefPropertyTag (TnefPropertyId.OriginalSenderName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OriginalSenderSearchKey = new TnefPropertyTag (TnefPropertyId.OriginalSenderSearchKey, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag OriginalSensitivity = new TnefPropertyTag (TnefPropertyId.OriginalSensitivity, TnefPropertyType.Long);
		public static readonly TnefPropertyTag OriginalSentRepresentingAddrtypeA = new TnefPropertyTag (TnefPropertyId.OriginalSentRepresentingAddrtype, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OriginalSentRepresentingAddrtypeW = new TnefPropertyTag (TnefPropertyId.OriginalSentRepresentingAddrtype, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OriginalSentRepresentingEmailAddressA = new TnefPropertyTag (TnefPropertyId.OriginalSentRepresentingEmailAddress, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OriginalSentRepresentingEmailAddressW = new TnefPropertyTag (TnefPropertyId.OriginalSentRepresentingEmailAddress, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OriginalSentRepresentingEntryId = new TnefPropertyTag (TnefPropertyId.OriginalSentRepresentingEntryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag OriginalSentRepresentingNameA = new TnefPropertyTag (TnefPropertyId.OriginalSentRepresentingName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OriginalSentRepresentingNameW = new TnefPropertyTag (TnefPropertyId.OriginalSentRepresentingName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OriginalSentRepresentingSearchKey = new TnefPropertyTag (TnefPropertyId.OriginalSentRepresentingSearchKey, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag OriginalSubjectA = new TnefPropertyTag (TnefPropertyId.OriginalSubject, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OriginalSubjectW = new TnefPropertyTag (TnefPropertyId.OriginalSubject, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OriginalSubmitTime = new TnefPropertyTag (TnefPropertyId.OriginalSubmitTime, TnefPropertyType.SysTime);
		public static readonly TnefPropertyTag OriginatingMtaCertificate = new TnefPropertyTag (TnefPropertyId.OriginatingMtaCertificate, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag OriginatorAndDlExpansionHistory = new TnefPropertyTag (TnefPropertyId.OriginatorAndDlExpansionHistory, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag OriginatorCertificate = new TnefPropertyTag (TnefPropertyId.OriginatorCertificate, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag OriginatorDeliveryReportRequested = new TnefPropertyTag (TnefPropertyId.OriginatorDeliveryReportRequested, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag OriginatorNonDeliveryReportRequested = new TnefPropertyTag (TnefPropertyId.OriginatorNonDeliveryReportRequested, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag OriginatorRequestedAlternateRecipient = new TnefPropertyTag (TnefPropertyId.OriginatorRequestedAlternateRecipient, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag OriginatorReturnAddress = new TnefPropertyTag (TnefPropertyId.OriginatorReturnAddress, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag OriginCheck = new TnefPropertyTag (TnefPropertyId.OriginCheck, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag OrigMessageClassA = new TnefPropertyTag (TnefPropertyId.OrigMessageClass, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OrigMessageClassW = new TnefPropertyTag (TnefPropertyId.OrigMessageClass, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OtherAddressCityA = new TnefPropertyTag (TnefPropertyId.OtherAddressCity, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OtherAddressCityW = new TnefPropertyTag (TnefPropertyId.OtherAddressCity, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OtherAddressCountryA = new TnefPropertyTag (TnefPropertyId.OtherAddressCountry, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OtherAddressCountryW = new TnefPropertyTag (TnefPropertyId.OtherAddressCountry, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OtherAddressPostalCodeA = new TnefPropertyTag (TnefPropertyId.OtherAddressPostalCode, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OtherAddressPostalCodeW = new TnefPropertyTag (TnefPropertyId.OtherAddressPostalCode, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OtherAddressPostOfficeBoxA = new TnefPropertyTag (TnefPropertyId.OtherAddressPostOfficeBox, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OtherAddressPostOfficeBoxW = new TnefPropertyTag (TnefPropertyId.OtherAddressPostOfficeBox, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OtherAddressStateOrProvinceA = new TnefPropertyTag (TnefPropertyId.OtherAddressStateOrProvince, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OtherAddressStateOrProvinceW = new TnefPropertyTag (TnefPropertyId.OtherAddressStateOrProvince, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OtherAddressStreetA = new TnefPropertyTag (TnefPropertyId.OtherAddressStreet, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OtherAddressStreetW = new TnefPropertyTag (TnefPropertyId.OtherAddressStreet, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OtherTelephoneNumberA = new TnefPropertyTag (TnefPropertyId.OtherTelephoneNumber, TnefPropertyType.String8);
		public static readonly TnefPropertyTag OtherTelephoneNumberW = new TnefPropertyTag (TnefPropertyId.OtherTelephoneNumber, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag OwnerApptId = new TnefPropertyTag (TnefPropertyId.OwnerApptId, TnefPropertyType.Long);
		public static readonly TnefPropertyTag OwnStoreEntryId = new TnefPropertyTag (TnefPropertyId.OwnStoreEntryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag PagerTelephoneNumberA = new TnefPropertyTag (TnefPropertyId.PagerTelephoneNumber, TnefPropertyType.String8);
		public static readonly TnefPropertyTag PagerTelephoneNumberW = new TnefPropertyTag (TnefPropertyId.PagerTelephoneNumber, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag ParentDisplayA = new TnefPropertyTag (TnefPropertyId.ParentDisplay, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ParentDisplayW = new TnefPropertyTag (TnefPropertyId.ParentDisplay, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag ParentEntryId = new TnefPropertyTag (TnefPropertyId.ParentEntryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ParentKey = new TnefPropertyTag (TnefPropertyId.ParentKey, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag PersonalHomePageA = new TnefPropertyTag (TnefPropertyId.PersonalHomePage, TnefPropertyType.String8);
		public static readonly TnefPropertyTag PersonalHomePageW = new TnefPropertyTag (TnefPropertyId.PersonalHomePage, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag PhysicalDeliveryBureauFaxDelivery = new TnefPropertyTag (TnefPropertyId.PhysicalDeliveryBureauFaxDelivery, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag PhysicalDeliveryMode = new TnefPropertyTag (TnefPropertyId.PhysicalDeliveryMode, TnefPropertyType.Long);
		public static readonly TnefPropertyTag PhysicalDeliveryReportRequest = new TnefPropertyTag (TnefPropertyId.PhysicalDeliveryReportRequest, TnefPropertyType.Long);
		public static readonly TnefPropertyTag PhysicalForwardingAddress = new TnefPropertyTag (TnefPropertyId.PhysicalForwardingAddress, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag PhysicalForwardingAddressRequested = new TnefPropertyTag (TnefPropertyId.PhysicalForwardingAddressRequested, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag PhysicalForwardingProhibited = new TnefPropertyTag (TnefPropertyId.PhysicalForwardingProhibited, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag PhysicalRenditionAttributes = new TnefPropertyTag (TnefPropertyId.PhysicalRenditionAttributes, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag PostalAddressA = new TnefPropertyTag (TnefPropertyId.PostalAddress, TnefPropertyType.String8);
		public static readonly TnefPropertyTag PostalAddressW = new TnefPropertyTag (TnefPropertyId.PostalAddress, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag PostalCodeA = new TnefPropertyTag (TnefPropertyId.PostalCode, TnefPropertyType.String8);
		public static readonly TnefPropertyTag PostalCodeW = new TnefPropertyTag (TnefPropertyId.PostalCode, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag PostFolderEntries = new TnefPropertyTag (TnefPropertyId.PostFolderEntries, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag PostFolderNamesA = new TnefPropertyTag (TnefPropertyId.PostFolderNames, TnefPropertyType.String8);
		public static readonly TnefPropertyTag PostFolderNamesW = new TnefPropertyTag (TnefPropertyId.PostFolderNames, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag PostOfficeBoxA = new TnefPropertyTag (TnefPropertyId.PostOfficeBox, TnefPropertyType.String8);
		public static readonly TnefPropertyTag PostOfficeBoxW = new TnefPropertyTag (TnefPropertyId.PostOfficeBox, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag PostReplyDenied = new TnefPropertyTag (TnefPropertyId.PostReplyDenied, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag PostReplyFolderEntries = new TnefPropertyTag (TnefPropertyId.PostReplyFolderEntries, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag PostReplyFolderNamesA = new TnefPropertyTag (TnefPropertyId.PostReplyFolderNames, TnefPropertyType.String8);
		public static readonly TnefPropertyTag PostReplyFolderNamesW = new TnefPropertyTag (TnefPropertyId.PostReplyFolderNames, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag PreferredByNameA = new TnefPropertyTag (TnefPropertyId.PreferredByName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag PreferredByNameW = new TnefPropertyTag (TnefPropertyId.PreferredByName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag Preprocess = new TnefPropertyTag (TnefPropertyId.Preprocess, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag PrimaryCapability = new TnefPropertyTag (TnefPropertyId.PrimaryCapability, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag PrimaryFaxNumber = new TnefPropertyTag (TnefPropertyId.PrimaryFaxNumber, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag PrimaryTelephoneNumberA = new TnefPropertyTag (TnefPropertyId.PrimaryTelephoneNumber, TnefPropertyType.String8);
		public static readonly TnefPropertyTag PrimaryTelephoneNumberW = new TnefPropertyTag (TnefPropertyId.PrimaryTelephoneNumber, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag Priority = new TnefPropertyTag (TnefPropertyId.Priority, TnefPropertyType.Long);
		public static readonly TnefPropertyTag ProfessionA = new TnefPropertyTag (TnefPropertyId.Profession, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ProfessionW = new TnefPropertyTag (TnefPropertyId.Profession, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag ProfileNameA = new TnefPropertyTag (TnefPropertyId.ProfileName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ProfileNameW = new TnefPropertyTag (TnefPropertyId.ProfileName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag ProofOfDelivery = new TnefPropertyTag (TnefPropertyId.ProofOfDelivery, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ProofOfDeliveryRequested = new TnefPropertyTag (TnefPropertyId.ProofOfDeliveryRequested, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag ProofOfSubmission = new TnefPropertyTag (TnefPropertyId.ProofOfSubmission, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ProofOfSubmissionRequested = new TnefPropertyTag (TnefPropertyId.ProofOfSubmissionRequested, TnefPropertyType.Boolean);
		//public static readonly TnefPropertyTag PropIdSecureMax = new TnefPropertyTag (TnefPropertyId.PropIdSecureMax, TnefPropertyType.Unspecified);
		//public static readonly TnefPropertyTag PropIdSecureMin = new TnefPropertyTag (TnefPropertyId.PropIdSecureMin, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag ProviderDisplayA = new TnefPropertyTag (TnefPropertyId.ProviderDisplay, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ProviderDisplayW = new TnefPropertyTag (TnefPropertyId.ProviderDisplay, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag ProviderDllNameA = new TnefPropertyTag (TnefPropertyId.ProviderDllName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ProviderDllNameW = new TnefPropertyTag (TnefPropertyId.ProviderDllName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag ProviderOrdinal = new TnefPropertyTag (TnefPropertyId.ProviderOrdinal, TnefPropertyType.Long);
		public static readonly TnefPropertyTag ProviderSubmitTime = new TnefPropertyTag (TnefPropertyId.ProviderSubmitTime, TnefPropertyType.SysTime);
		public static readonly TnefPropertyTag ProviderUid = new TnefPropertyTag (TnefPropertyId.ProviderUid, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag Puid = new TnefPropertyTag (TnefPropertyId.Puid, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag RadioTelephoneNumberA = new TnefPropertyTag (TnefPropertyId.RadioTelephoneNumber, TnefPropertyType.String8);
		public static readonly TnefPropertyTag RadioTelephoneNumberW = new TnefPropertyTag (TnefPropertyId.RadioTelephoneNumber, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag RcvdRepresentingAddrtypeA = new TnefPropertyTag (TnefPropertyId.RcvdRepresentingAddrtype, TnefPropertyType.String8);
		public static readonly TnefPropertyTag RcvdRepresentingAddrtypeW = new TnefPropertyTag (TnefPropertyId.RcvdRepresentingAddrtype, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag RcvdRepresentingEmailAddressA = new TnefPropertyTag (TnefPropertyId.RcvdRepresentingEmailAddress, TnefPropertyType.String8);
		public static readonly TnefPropertyTag RcvdRepresentingEmailAddressW = new TnefPropertyTag (TnefPropertyId.RcvdRepresentingEmailAddress, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag RcvdRepresentingEntryId = new TnefPropertyTag (TnefPropertyId.RcvdRepresentingEntryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag RcvdRepresentingNameA = new TnefPropertyTag (TnefPropertyId.RcvdRepresentingName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag RcvdRepresentingNameW = new TnefPropertyTag (TnefPropertyId.RcvdRepresentingName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag RcvdRepresentingSearchKey = new TnefPropertyTag (TnefPropertyId.RcvdRepresentingSearchKey, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ReadReceiptEntryId = new TnefPropertyTag (TnefPropertyId.ReadReceiptEntryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ReadReceiptRequested = new TnefPropertyTag (TnefPropertyId.ReadReceiptRequested, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag ReadReceiptSearchKey = new TnefPropertyTag (TnefPropertyId.ReadReceiptSearchKey, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ReceiptTime = new TnefPropertyTag (TnefPropertyId.ReceiptTime, TnefPropertyType.SysTime);
		public static readonly TnefPropertyTag ReceivedByAddrtypeA = new TnefPropertyTag (TnefPropertyId.ReceivedByAddrtype, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ReceivedByAddrtypeW = new TnefPropertyTag (TnefPropertyId.ReceivedByAddrtype, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag ReceivedByEmailAddressA = new TnefPropertyTag (TnefPropertyId.ReceivedByEmailAddress, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ReceivedByEmailAddressW = new TnefPropertyTag (TnefPropertyId.ReceivedByEmailAddress, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag ReceivedByEntryId = new TnefPropertyTag (TnefPropertyId.ReceivedByEntryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ReceivedByNameA = new TnefPropertyTag (TnefPropertyId.ReceivedByName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ReceivedByNameW = new TnefPropertyTag (TnefPropertyId.ReceivedByName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag ReceivedBySearchKey = new TnefPropertyTag (TnefPropertyId.ReceivedBySearchKey, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ReceiveFolderSettings = new TnefPropertyTag (TnefPropertyId.ReceiveFolderSettings, TnefPropertyType.Object);
		public static readonly TnefPropertyTag RecipientCertificate = new TnefPropertyTag (TnefPropertyId.RecipientCertificate, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag RecipientNumberForAdviceA = new TnefPropertyTag (TnefPropertyId.RecipientNumberForAdvice, TnefPropertyType.String8);
		public static readonly TnefPropertyTag RecipientNumberForAdviceW = new TnefPropertyTag (TnefPropertyId.RecipientNumberForAdvice, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag RecipientReassignmentProhibited = new TnefPropertyTag (TnefPropertyId.RecipientReassignmentProhibited, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag RecipientStatus = new TnefPropertyTag (TnefPropertyId.RecipientStatus, TnefPropertyType.Long);
		public static readonly TnefPropertyTag RecipientType = new TnefPropertyTag (TnefPropertyId.RecipientType, TnefPropertyType.Long);
		public static readonly TnefPropertyTag RecordKey = new TnefPropertyTag (TnefPropertyId.RecordKey, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag RedirectionHistory = new TnefPropertyTag (TnefPropertyId.RedirectionHistory, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ReferredByNameA = new TnefPropertyTag (TnefPropertyId.ReferredByName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ReferredByNameW = new TnefPropertyTag (TnefPropertyId.ReferredByName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag RegisteredMailType = new TnefPropertyTag (TnefPropertyId.RegisteredMailType, TnefPropertyType.Long);
		public static readonly TnefPropertyTag RelatedIpms = new TnefPropertyTag (TnefPropertyId.RelatedIpms, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag RemoteProgress = new TnefPropertyTag (TnefPropertyId.RemoteProgress, TnefPropertyType.Long);
		public static readonly TnefPropertyTag RemoteProgressTextA = new TnefPropertyTag (TnefPropertyId.RemoteProgressText, TnefPropertyType.String8);
		public static readonly TnefPropertyTag RemoteProgressTextW = new TnefPropertyTag (TnefPropertyId.RemoteProgressText, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag RemoteValidateOk = new TnefPropertyTag (TnefPropertyId.RemoteValidateOk, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag RenderingPosition = new TnefPropertyTag (TnefPropertyId.RenderingPosition, TnefPropertyType.Long);
		public static readonly TnefPropertyTag ReplyRecipientEntries = new TnefPropertyTag (TnefPropertyId.ReplyRecipientEntries, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ReplyRecipientNamesA = new TnefPropertyTag (TnefPropertyId.ReplyRecipientNames, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ReplyRecipientNamesW = new TnefPropertyTag (TnefPropertyId.ReplyRecipientNames, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag ReplyRequested = new TnefPropertyTag (TnefPropertyId.ReplyRequested, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag ReplyTime = new TnefPropertyTag (TnefPropertyId.ReplyTime, TnefPropertyType.SysTime);
		public static readonly TnefPropertyTag ReportEntryId = new TnefPropertyTag (TnefPropertyId.ReportEntryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ReportingDlName = new TnefPropertyTag (TnefPropertyId.ReportingDlName, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ReportingMtaCertificate = new TnefPropertyTag (TnefPropertyId.ReportingMtaCertificate, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ReportNameA = new TnefPropertyTag (TnefPropertyId.ReportName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ReportNameW = new TnefPropertyTag (TnefPropertyId.ReportName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag ReportSearchKey = new TnefPropertyTag (TnefPropertyId.ReportSearchKey, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ReportTag = new TnefPropertyTag (TnefPropertyId.ReportTag, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ReportTextA = new TnefPropertyTag (TnefPropertyId.ReportText, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ReportTextW = new TnefPropertyTag (TnefPropertyId.ReportText, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag ReportTime = new TnefPropertyTag (TnefPropertyId.ReportTime, TnefPropertyType.SysTime);
		public static readonly TnefPropertyTag RequestedDeliveryMethod = new TnefPropertyTag (TnefPropertyId.RequestedDeliveryMethod, TnefPropertyType.Long);
		public static readonly TnefPropertyTag ResourceFlags = new TnefPropertyTag (TnefPropertyId.ResourceFlags, TnefPropertyType.Long);
		public static readonly TnefPropertyTag ResourceMethods = new TnefPropertyTag (TnefPropertyId.ResourceMethods, TnefPropertyType.Long);
		public static readonly TnefPropertyTag ResourcePathA = new TnefPropertyTag (TnefPropertyId.ResourcePath, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ResourcePathW = new TnefPropertyTag (TnefPropertyId.ResourcePath, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag ResourceType = new TnefPropertyTag (TnefPropertyId.ResourceType, TnefPropertyType.Long);
		public static readonly TnefPropertyTag ResponseRequested = new TnefPropertyTag (TnefPropertyId.ResponseRequested, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag Responsibility = new TnefPropertyTag (TnefPropertyId.Responsibility, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag ReturnedIpm = new TnefPropertyTag (TnefPropertyId.ReturnedIpm, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag Rowid = new TnefPropertyTag (TnefPropertyId.Rowid, TnefPropertyType.Long);
		public static readonly TnefPropertyTag RowType = new TnefPropertyTag (TnefPropertyId.RowType, TnefPropertyType.Long);
		public static readonly TnefPropertyTag RtfCompressed = new TnefPropertyTag (TnefPropertyId.RtfCompressed, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag RtfInSync = new TnefPropertyTag (TnefPropertyId.RtfInSync, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag RtfSyncBodyCount = new TnefPropertyTag (TnefPropertyId.RtfSyncBodyCount, TnefPropertyType.Long);
		public static readonly TnefPropertyTag RtfSyncBodyCrc = new TnefPropertyTag (TnefPropertyId.RtfSyncBodyCrc, TnefPropertyType.Long);
		public static readonly TnefPropertyTag RtfSyncBodyTagA = new TnefPropertyTag (TnefPropertyId.RtfSyncBodyTag, TnefPropertyType.String8);
		public static readonly TnefPropertyTag RtfSyncBodyTagW = new TnefPropertyTag (TnefPropertyId.RtfSyncBodyTag, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag RtfSyncPrefixCount = new TnefPropertyTag (TnefPropertyId.RtfSyncPrefixCount, TnefPropertyType.Long);
		public static readonly TnefPropertyTag RtfSyncTrailingCount = new TnefPropertyTag (TnefPropertyId.RtfSyncTrailingCount, TnefPropertyType.Long);
		public static readonly TnefPropertyTag Search = new TnefPropertyTag (TnefPropertyId.Search, TnefPropertyType.Object);
		public static readonly TnefPropertyTag SearchKey = new TnefPropertyTag (TnefPropertyId.SearchKey, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag Security = new TnefPropertyTag (TnefPropertyId.Security, TnefPropertyType.Long);
		public static readonly TnefPropertyTag Selectable = new TnefPropertyTag (TnefPropertyId.Selectable, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag SenderAddrtypeA = new TnefPropertyTag (TnefPropertyId.SenderAddrtype, TnefPropertyType.String8);
		public static readonly TnefPropertyTag SenderAddrtypeW = new TnefPropertyTag (TnefPropertyId.SenderAddrtype, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag SenderEmailAddressA = new TnefPropertyTag (TnefPropertyId.SenderEmailAddress, TnefPropertyType.String8);
		public static readonly TnefPropertyTag SenderEmailAddressW = new TnefPropertyTag (TnefPropertyId.SenderEmailAddress, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag SenderEntryId = new TnefPropertyTag (TnefPropertyId.SenderEntryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag SenderNameA = new TnefPropertyTag (TnefPropertyId.SenderName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag SenderNameW = new TnefPropertyTag (TnefPropertyId.SenderName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag SenderSearchKey = new TnefPropertyTag (TnefPropertyId.SenderSearchKey, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag SendInternetEncoding = new TnefPropertyTag (TnefPropertyId.SendInternetEncoding, TnefPropertyType.Long);
		public static readonly TnefPropertyTag SendRecallReport = new TnefPropertyTag (TnefPropertyId.SendRecallReport, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag SendRichInfo = new TnefPropertyTag (TnefPropertyId.SendRichInfo, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag Sensitivity = new TnefPropertyTag (TnefPropertyId.Sensitivity, TnefPropertyType.Long);
		public static readonly TnefPropertyTag SentmailEntryId = new TnefPropertyTag (TnefPropertyId.SentmailEntryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag SentRepresentingAddrtypeA = new TnefPropertyTag (TnefPropertyId.SentRepresentingAddrtype, TnefPropertyType.String8);
		public static readonly TnefPropertyTag SentRepresentingAddrtypeW = new TnefPropertyTag (TnefPropertyId.SentRepresentingAddrtype, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag SentRepresentingEmailAddressA = new TnefPropertyTag (TnefPropertyId.SentRepresentingEmailAddress, TnefPropertyType.String8);
		public static readonly TnefPropertyTag SentRepresentingEmailAddressW = new TnefPropertyTag (TnefPropertyId.SentRepresentingEmailAddress, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag SentRepresentingEntryId = new TnefPropertyTag (TnefPropertyId.SentRepresentingEntryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag SentRepresentingNameA = new TnefPropertyTag (TnefPropertyId.SentRepresentingName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag SentRepresentingNameW = new TnefPropertyTag (TnefPropertyId.SentRepresentingName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag SentRepresentingSearchKey = new TnefPropertyTag (TnefPropertyId.SentRepresentingSearchKey, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ServiceDeleteFilesA = new TnefPropertyTag (TnefPropertyId.ServiceDeleteFiles, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ServiceDeleteFilesW = new TnefPropertyTag (TnefPropertyId.ServiceDeleteFiles, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag ServiceDllNameA = new TnefPropertyTag (TnefPropertyId.ServiceDllName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ServiceDllNameW = new TnefPropertyTag (TnefPropertyId.ServiceDllName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag ServiceEntryName = new TnefPropertyTag (TnefPropertyId.ServiceEntryName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ServiceExtraUids = new TnefPropertyTag (TnefPropertyId.ServiceExtraUids, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ServiceNameA = new TnefPropertyTag (TnefPropertyId.ServiceName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ServiceNameW = new TnefPropertyTag (TnefPropertyId.ServiceName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag Services = new TnefPropertyTag (TnefPropertyId.Services, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ServiceSupportFilesA = new TnefPropertyTag (TnefPropertyId.ServiceSupportFiles, TnefPropertyType.String8);
		public static readonly TnefPropertyTag ServiceSupportFilesW = new TnefPropertyTag (TnefPropertyId.ServiceSupportFiles, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag ServiceUid = new TnefPropertyTag (TnefPropertyId.ServiceUid, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag SevenBitDisplayName = new TnefPropertyTag (TnefPropertyId.SevenBitDisplayName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag SmtpAddressA = new TnefPropertyTag (TnefPropertyId.SmtpAddress, TnefPropertyType.String8);
		public static readonly TnefPropertyTag SmtpAddressW = new TnefPropertyTag (TnefPropertyId.SmtpAddress, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag SpoolerStatus = new TnefPropertyTag (TnefPropertyId.SpoolerStatus, TnefPropertyType.Long);
		public static readonly TnefPropertyTag SpouseNameA = new TnefPropertyTag (TnefPropertyId.SpouseName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag SpouseNameW = new TnefPropertyTag (TnefPropertyId.SpouseName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag StartDate = new TnefPropertyTag (TnefPropertyId.StartDate, TnefPropertyType.SysTime);
		public static readonly TnefPropertyTag StateOrProvinceA = new TnefPropertyTag (TnefPropertyId.StateOrProvince, TnefPropertyType.String8);
		public static readonly TnefPropertyTag StateOrProvinceW = new TnefPropertyTag (TnefPropertyId.StateOrProvince, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag Status = new TnefPropertyTag (TnefPropertyId.Status, TnefPropertyType.Long);
		public static readonly TnefPropertyTag StatusCode = new TnefPropertyTag (TnefPropertyId.StatusCode, TnefPropertyType.Long);
		public static readonly TnefPropertyTag StatusStringA = new TnefPropertyTag (TnefPropertyId.StatusString, TnefPropertyType.String8);
		public static readonly TnefPropertyTag StatusStringW = new TnefPropertyTag (TnefPropertyId.StatusString, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag StoreEntryId = new TnefPropertyTag (TnefPropertyId.StoreEntryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag StoreProviders = new TnefPropertyTag (TnefPropertyId.StoreProviders, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag StoreRecordKey = new TnefPropertyTag (TnefPropertyId.StoreRecordKey, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag StoreState = new TnefPropertyTag (TnefPropertyId.StoreState, TnefPropertyType.Long);
		public static readonly TnefPropertyTag StoreSupportMask = new TnefPropertyTag (TnefPropertyId.StoreSupportMask, TnefPropertyType.Long);
		public static readonly TnefPropertyTag StreetAddressA = new TnefPropertyTag (TnefPropertyId.StreetAddress, TnefPropertyType.String8);
		public static readonly TnefPropertyTag StreetAddressW = new TnefPropertyTag (TnefPropertyId.StreetAddress, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag Subfolders = new TnefPropertyTag (TnefPropertyId.Subfolders, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag SubjectA = new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.String8);
		public static readonly TnefPropertyTag SubjectW = new TnefPropertyTag (TnefPropertyId.Subject, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag SubjectIpm = new TnefPropertyTag (TnefPropertyId.SubjectIpm, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag SubjectPrefixA = new TnefPropertyTag (TnefPropertyId.SubjectPrefix, TnefPropertyType.String8);
		public static readonly TnefPropertyTag SubjectPrefixW = new TnefPropertyTag (TnefPropertyId.SubjectPrefix, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag SubmitFlags = new TnefPropertyTag (TnefPropertyId.SubmitFlags, TnefPropertyType.Long);
		public static readonly TnefPropertyTag SupersedesA = new TnefPropertyTag (TnefPropertyId.Supersedes, TnefPropertyType.String8);
		public static readonly TnefPropertyTag SupersedesW = new TnefPropertyTag (TnefPropertyId.Supersedes, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag SupplementaryInfoA = new TnefPropertyTag (TnefPropertyId.SupplementaryInfo, TnefPropertyType.String8);
		public static readonly TnefPropertyTag SupplementaryInfoW = new TnefPropertyTag (TnefPropertyId.SupplementaryInfo, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag SurnameA = new TnefPropertyTag (TnefPropertyId.Surname, TnefPropertyType.String8);
		public static readonly TnefPropertyTag SurnameW = new TnefPropertyTag (TnefPropertyId.Surname, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag TelexNumberA = new TnefPropertyTag (TnefPropertyId.TelexNumber, TnefPropertyType.String8);
		public static readonly TnefPropertyTag TelexNumberW = new TnefPropertyTag (TnefPropertyId.TelexNumber, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag Templateid = new TnefPropertyTag (TnefPropertyId.Templateid, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag TitleA = new TnefPropertyTag (TnefPropertyId.Title, TnefPropertyType.String8);
		public static readonly TnefPropertyTag TitleW = new TnefPropertyTag (TnefPropertyId.Title, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag TnefCorrelationKey = new TnefPropertyTag (TnefPropertyId.TnefCorrelationKey, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag TransmitableDisplayNameA = new TnefPropertyTag (TnefPropertyId.TransmitableDisplayName, TnefPropertyType.String8);
		public static readonly TnefPropertyTag TransmitableDisplayNameW = new TnefPropertyTag (TnefPropertyId.TransmitableDisplayName, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag TransportKey = new TnefPropertyTag (TnefPropertyId.TransportKey, TnefPropertyType.Long);
		public static readonly TnefPropertyTag TransportMessageHeadersA = new TnefPropertyTag (TnefPropertyId.TransportMessageHeaders, TnefPropertyType.String8);
		public static readonly TnefPropertyTag TransportMessageHeadersW = new TnefPropertyTag (TnefPropertyId.TransportMessageHeaders, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag TransportProviders = new TnefPropertyTag (TnefPropertyId.TransportProviders, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag TransportStatus = new TnefPropertyTag (TnefPropertyId.TransportStatus, TnefPropertyType.Long);
		public static readonly TnefPropertyTag TtytddPhoneNumberA = new TnefPropertyTag (TnefPropertyId.TtytddPhoneNumber, TnefPropertyType.String8);
		public static readonly TnefPropertyTag TtytddPhoneNumberW = new TnefPropertyTag (TnefPropertyId.TtytddPhoneNumber, TnefPropertyType.Unicode);
		public static readonly TnefPropertyTag TypeOfMtsUser = new TnefPropertyTag (TnefPropertyId.TypeOfMtsUser, TnefPropertyType.Long);
		public static readonly TnefPropertyTag UserCertificate = new TnefPropertyTag (TnefPropertyId.UserCertificate, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag UserX509Certificate = new TnefPropertyTag (TnefPropertyId.UserX509Certificate, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag ValidFolderMask = new TnefPropertyTag (TnefPropertyId.ValidFolderMask, TnefPropertyType.Long);
		public static readonly TnefPropertyTag ViewsEntryId = new TnefPropertyTag (TnefPropertyId.ViewsEntryId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag WeddingAnniversary = new TnefPropertyTag (TnefPropertyId.WeddingAnniversary, TnefPropertyType.SysTime);
		public static readonly TnefPropertyTag X400ContentType = new TnefPropertyTag (TnefPropertyId.X400ContentType, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag X400DeferredDeliveryCancel = new TnefPropertyTag (TnefPropertyId.X400DeferredDeliveryCancel, TnefPropertyType.Boolean);
		public static readonly TnefPropertyTag Xpos = new TnefPropertyTag (TnefPropertyId.Xpos, TnefPropertyType.Long);
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

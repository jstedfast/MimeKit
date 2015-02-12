//
// TnefAttributeTag.cs
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
	enum TnefAttributeType {
		Triples = 0x00000000,
		String  = 0x00010000,
		Text    = 0x00020000,
		Date    = 0x00030000,
		Short   = 0x00040000,
		Long    = 0x00050000,
		Byte    = 0x00060000,
		Word    = 0x00070000,
		DWord   = 0x00080000,
		Max     = 0x00090000
	}

	/// <summary>
	/// A TNEF attribute tag.
	/// </summary>
	/// <remarks>
	/// A TNEF attribute tag.
	/// </remarks>
	public enum TnefAttributeTag {
		/// <summary>
		/// A Null TNEF attribute.
		/// </summary>
		Null                    = TnefAttributeType.Triples | 0x0000,

		/// <summary>
		/// The Owner TNEF attribute.
		/// </summary>
		Owner                   = TnefAttributeType.Byte    | 0x0000,

		/// <summary>
		/// The SentFor TNEF attribute.
		/// </summary>
		SentFor                 = TnefAttributeType.Byte    | 0x0001,

		/// <summary>
		/// The Delegate TNEF attribute.
		/// </summary>
		Delegate                = TnefAttributeType.Byte    | 0x0002,

		/// <summary>
		/// The OriginalMessageClass TNEF attribute.
		/// </summary>
		OriginalMessageClass    = TnefAttributeType.Word    | 0x0006,

		/// <summary>
		/// The DateStart TNEF attribute.
		/// </summary>
		DateStart               = TnefAttributeType.Date    | 0x0006,

		/// <summary>
		/// The DateEnd TNEF attribute.
		/// </summary>
		DateEnd                 = TnefAttributeType.Date    | 0x0007,

		/// <summary>
		/// The AidOwner TNEF attribute.
		/// </summary>
		AidOwner                = TnefAttributeType.Long    | 0x0008,

		/// <summary>
		/// The RequestResponse TNEF attribute.
		/// </summary>
		RequestResponse         = TnefAttributeType.Short   | 0x0009,

		/// <summary>
		/// The From TNEF attribute.
		/// </summary>
		From                    = TnefAttributeType.Triples | 0x8000,

		/// <summary>
		/// The Subject TNEF attribute.
		/// </summary>
		Subject                 = TnefAttributeType.String  | 0x8004,

		/// <summary>
		/// The DateSent TNEF attribute.
		/// </summary>
		DateSent                = TnefAttributeType.Date    | 0x8005,

		/// <summary>
		/// The DateReceived TNEF attribute.
		/// </summary>
		DateReceived            = TnefAttributeType.Date    | 0x8006,

		/// <summary>
		/// The MessageStatus TNEF attribute.
		/// </summary>
		MessageStatus           = TnefAttributeType.Byte    | 0x8007,

		/// <summary>
		/// The MessageClass TNEF attribute.
		/// </summary>
		MessageClass            = TnefAttributeType.Word    | 0x8008,

		/// <summary>
		/// The MessageId TNEF attribute.
		/// </summary>
		MessageId               = TnefAttributeType.String  | 0x8009,

		/// <summary>
		/// The ParentId TNEF attribute.
		/// </summary>
		ParentId                = TnefAttributeType.String  | 0x800A,

		/// <summary>
		/// The ConversationId TNEF attribute.
		/// </summary>
		ConversationId          = TnefAttributeType.String  | 0x800B,

		/// <summary>
		/// The Body TNEF attribute.
		/// </summary>
		Body                    = TnefAttributeType.Text    | 0x800C,

		/// <summary>
		/// The Priority TNEF attribute.
		/// </summary>
		Priority                = TnefAttributeType.Short   | 0x800D,

		/// <summary>
		/// The AttachData TNEF attribute.
		/// </summary>
		AttachData              = TnefAttributeType.Byte    | 0x800F,

		/// <summary>
		/// The AttachTitle TNEF attribute.
		/// </summary>
		AttachTitle             = TnefAttributeType.String  | 0x8010,

		/// <summary>
		/// The AttachMetaFile TNEF attribute.
		/// </summary>
		AttachMetaFile          = TnefAttributeType.Byte    | 0x8011,

		/// <summary>
		/// The AttachCreateDate TNEF attribute.
		/// </summary>
		AttachCreateDate        = TnefAttributeType.Date    | 0x8012,

		/// <summary>
		/// The AttachModifyDate TNEF attribute.
		/// </summary>
		AttachModifyDate        = TnefAttributeType.Date    | 0x8013,

		/// <summary>
		/// The DateModified TNEF attribute.
		/// </summary>
		DateModified            = TnefAttributeType.Date    | 0x8020,

		/// <summary>
		/// The AttachTransportFilename TNEF attribute.
		/// </summary>
		AttachTransportFilename = TnefAttributeType.Byte    | 0x9001,

		/// <summary>
		/// The AttachRenderData TNEF attribute.
		/// </summary>
		AttachRenderData        = TnefAttributeType.Byte    | 0x9002,

		/// <summary>
		/// The MapiProperties TNEF attribute.
		/// </summary>
		MapiProperties          = TnefAttributeType.Byte    | 0x9003,

		/// <summary>
		/// The RecipientTable TNEF attribute.
		/// </summary>
		RecipientTable          = TnefAttributeType.Byte    | 0x9004,

		/// <summary>
		/// The Attachment TNEF attribute.
		/// </summary>
		Attachment              = TnefAttributeType.Byte    | 0x9005,

		/// <summary>
		/// The TnefVersion TNEF attribute.
		/// </summary>
		TnefVersion             = TnefAttributeType.DWord   | 0x9006,

		/// <summary>
		/// The OemCodepage TNEF attribute.
		/// </summary>
		OemCodepage             = TnefAttributeType.Byte    | 0x9007,
	}
}

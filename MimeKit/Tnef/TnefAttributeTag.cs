//
// TnefAttributeTag.cs
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

	public enum TnefAttributeTag {
		Null                    = TnefAttributeType.Triples | 0x0000,
		Owner                   = TnefAttributeType.Byte    | 0x0000,
		SentFor                 = TnefAttributeType.Byte    | 0x0001,
		Delegate                = TnefAttributeType.Byte    | 0x0002,
		OriginalMessageClass    = TnefAttributeType.Word    | 0x0006,
		DateStart               = TnefAttributeType.Date    | 0x0006,
		DateEnd                 = TnefAttributeType.Date    | 0x0007,
		AidOwner                = TnefAttributeType.Long    | 0x0008,
		RequestResponse         = TnefAttributeType.Short   | 0x0009,
		From                    = TnefAttributeType.Triples | 0x8000,
		Subject                 = TnefAttributeType.String  | 0x8004,
		DateSent                = TnefAttributeType.Date    | 0x8005,
		DateReceived            = TnefAttributeType.Date    | 0x8006,
		MessageStatus           = TnefAttributeType.Byte    | 0x8007,
		MessageClass            = TnefAttributeType.Word    | 0x8008,
		MessageId               = TnefAttributeType.String  | 0x8009,
		ParentId                = TnefAttributeType.String  | 0x800A,
		ConversationId          = TnefAttributeType.String  | 0x800B,
		Body                    = TnefAttributeType.Text    | 0x800C,
		Priority                = TnefAttributeType.Short   | 0x800D,
		AttachData              = TnefAttributeType.Byte    | 0x800F,
		AttachTitle             = TnefAttributeType.String  | 0x8010,
		AttachMetaFile          = TnefAttributeType.Byte    | 0x8011,
		AttachCreateDate        = TnefAttributeType.Date    | 0x8012,
		AttachModifyDate        = TnefAttributeType.Date    | 0x8013,
		DateModified            = TnefAttributeType.Date    | 0x8020,
		AttachTransportFilename = TnefAttributeType.Byte    | 0x9001,
		AttachRenderData        = TnefAttributeType.Byte    | 0x9002,
		MapiProperties          = TnefAttributeType.Byte    | 0x9003,
		RecipientTable          = TnefAttributeType.Byte    | 0x9004,
		Attachment              = TnefAttributeType.Byte    | 0x9005,
		TnefVersion             = TnefAttributeType.DWord   | 0x9006,
		OemCodepage             = TnefAttributeType.Byte    | 0x9007,
	}
}

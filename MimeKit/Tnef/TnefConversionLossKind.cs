//
// TnefConversionLossKind.cs
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
	/// The kind of information that was lost when converting a TNEF message to MIME.
	/// </summary>
	/// <remarks>
	/// The kind of information that was lost when converting a <see cref="TnefMessage"/> to MIME.
	/// </remarks>
	public enum TnefConversionLossKind
	{
		/// <summary>
		/// The <see cref="TnefPropertyId.MimeSkeleton"/> could not be parsed or did not match the message's bodies and
		/// attachments, so the MIME message was built from the message's properties instead.
		/// </summary>
		InvalidMimeSkeleton,

		/// <summary>
		/// The address of a sender or recipient could not be converted to an Internet mailbox address and was dropped.
		/// </summary>
		UnparsableRecipient,

		/// <summary>
		/// A message identifier (such as the <see cref="TnefPropertyId.InternetMessageId"/>) was not a valid
		/// msg-id and was dropped.
		/// </summary>
		InvalidMessageId,

		/// <summary>
		/// An attachment uses an attachment method that refers to content outside of the TNEF stream, such as
		/// <c>afByReference</c>, and was dropped.
		/// </summary>
		UnsupportedAttachMethod,

		/// <summary>
		/// An attachment did not have any content and was dropped.
		/// </summary>
		AttachmentWithoutContent,

		/// <summary>
		/// A header could not be converted and was dropped.
		/// </summary>
		InvalidHeader,

		/// <summary>
		/// An embedded message could not be loaded and was converted to an ordinary attachment.
		/// </summary>
		InvalidEmbeddedMessage,

		/// <summary>
		/// The <see cref="TnefPropertyId.RtfCompressed"/> body has a compression type other than
		/// <see cref="RtfCompressionMode.Compressed"/> or <see cref="RtfCompressionMode.Uncompressed"/>, so it could
		/// not be decoded and was dropped.
		/// </summary>
		InvalidRtfBody,

		/// <summary>
		/// The CRC of the <see cref="TnefPropertyId.RtfCompressed"/> body does not match its content. The RTF body was
		/// converted anyway, but may be corrupt.
		/// </summary>
		RtfChecksumMismatch
	}
}

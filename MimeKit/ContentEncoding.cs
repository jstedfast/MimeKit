// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MimeKit {
	/// <summary>
	/// An enumeration of all supported content transfer encodings.
	/// </summary>
	/// <remarks>
	/// Some older mail software is unable to properly deal with
	/// data outside of the ASCII range, so it is sometimes
	/// necessary to encode the content of MIME entities.
	/// </remarks>
	/// <seealso cref="MimeKit.MimePart.ContentTransferEncoding"/>
	public enum ContentEncoding {
		/// <summary>
		/// The default encoding (aka no encoding at all).
		/// </summary>
		Default,

		/// <summary>
		/// The 7bit content transfer encoding. This encoding should be restricted to textual content
		/// in the US-ASCII range.
		/// </summary>
		SevenBit,

		/// <summary>
		/// The 8bit content transfer encoding. This encoding should be restricted to textual content
		/// outside of the US-ASCII range but may not be supported by all transport services such as
		/// older SMTP servers that do not support the 8BITMIME extension.
		/// </summary>
		EightBit,

		/// <summary>
		/// The binary content transfer encoding. This encoding is simply unencoded binary data. Typically
		/// not supported by standard message transport services such as SMTP.
		/// </summary>
		Binary,

		/// <summary>
		/// The base64 content transfer encoding. This encoding is typically used for encoding binary data
		/// or textual content in a largely 8bit charset encoding and is supported by all message transport
		/// services.
		/// </summary>
		Base64,

		/// <summary>
		/// The quoted-printable content transfer encoding. This encoding is used for textual content that
		/// is in a charset that has a minority of characters outside of the US-ASCII range (such as
		/// ISO-8859-1 and other single-byte charset encodings) and is supported by all message transport
		/// services.
		/// </summary>
		QuotedPrintable,

		/// <summary>
		/// The uuencode content transfer encoding. This is an obsolete encoding meant for encoding binary
		/// data and has largely been superseded by <see cref="Base64"/>.
		/// </summary>
		UUEncode,
	}
}

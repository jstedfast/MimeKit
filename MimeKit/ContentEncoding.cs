//
// ContentEncoding.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc.
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

namespace MimeKit {
	/// <summary>
	/// An enumeration of all supported content transfer encodings.
	/// <seealso cref="MimeKit.MimePart.ContentTransferEncoding"/>.
	/// </summary>
	/// <remarks>
	/// Some older mail software is unable to properly deal with
	/// data outside of the ASCII range, so it is sometimes
	/// necessary to encode the content of MIME entities.
	/// </remarks>
	public enum ContentEncoding {
		/// <summary>
		/// The default encoding (aka no encoding at all).
		/// </summary>
		Default,

		/// <summary>
		/// The 7bit content transfer encoding.
		/// </summary>
		/// <remarks>
		/// This encoding should be restricted to textual content
		/// in the US-ASCII range.
		/// </remarks>
		SevenBit,

		/// <summary>
		/// The 8bit content transfer encoding.
		/// </summary>
		/// <remarks>
		/// This encoding should be restricted to textual content
		/// outside of the US-ASCII range but may not be supported
		/// by all transport services such as older SMTP servers
		/// that do not support the 8BITMIME extension.
		/// </remarks>
		EightBit,

		/// <summary>
		/// The binary content transfer encoding.
		/// </summary>
		/// <remarks>
		/// This encoding is simply unencoded binary data. Typically not
		/// supported by standard message transport services such as SMTP.
		/// </remarks>
		Binary,

		/// <summary>
		/// The base64 content transfer encoding.
		/// <seealso cref="MimeKit.Encodings.Base64Encoder"/>.
		/// </summary>
		/// <remarks>
		/// This encoding is typically used for encoding binary data
		/// or textual content in a largely 8bit charset encoding and
		/// is supported by all message transport services.
		/// </remarks>
		Base64,

		/// <summary>
		/// The quoted printable content transfer encoding.
		/// <seealso cref="MimeKit.Encodings.QuotedPrintableEncoder"/>.
		/// </summary>
		/// <remarks>
		/// This encoding is used for textual content that is in a charset
		/// that has a minority of characters outside of the US-ASCII range
		/// (such as ISO-8859-1 and other single-byte charset encodings) and
		/// is supported by all message transport services.
		/// </remarks>
		QuotedPrintable,

		/// <summary>
		/// The uuencode content transfer encoding.
		/// <seealso cref="MimeKit.Encodings.UUEncoder"/>.
		/// </summary>
		/// <remarks>
		/// This is an obsolete encoding meant for encoding binary
		/// data and has largely been superceeded by <see cref="Base64"/>.
		/// </remarks>
		UUEncode,
	}
}

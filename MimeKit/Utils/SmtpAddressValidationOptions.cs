//
// SmtpAddressValidationOptions.cs
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

using System;

namespace MimeKit.Utils {
	/// <summary>
	/// Options for SMTP address validation.
	/// </summary>
	/// <remarks>
	/// Options that change the behavior of the <see cref="SmtpAddressValidator"/>.
	/// </remarks>
	[Flags]
	public enum SmtpAddressValidationOptions
	{
		/// <summary>
		/// Validate the address according to the <c>Mailbox</c> syntax defined in
		/// <a href="https://www.rfc-editor.org/rfc/rfc5321#section-4.1.2">rfc5321</a> without enforcing any
		/// length limits.
		/// </summary>
		None = 0,

		/// <summary>
		/// <para>Allow a quoted local-part to contain Japanese text encoded using the ISO-2022-JP character encoding
		/// defined in <a href="https://www.rfc-editor.org/rfc/rfc1468">rfc1468</a>, such as
		/// <c>"\u001B$BF|K\\8l\u001B(B"@example.jp</c>.</para>
		/// <para>Some legacy Japanese mail clients produce such addresses even though they are not valid according to
		/// any standard since the escape sequences used by ISO-2022-JP begin with the <c>ESC</c> (U+001B) control
		/// character, which is not permitted in an address.</para>
		/// <para>When this option is set, a quoted local-part may switch to JIS X 0201-Roman (<c>ESC ( J</c>) or
		/// JIS X 0208 (<c>ESC $ @</c> or <c>ESC $ B</c>) and back to ASCII (<c>ESC ( B</c>). The local-part must end
		/// in ASCII, each JIS X 0208 character must consist of two bytes in the range 0x21-0x7E, and any <c>"</c>
		/// or <c>\</c> byte must be escaped using a quoted-pair, just as it would be anywhere else in a quoted
		/// local-part. Any other use of the <c>ESC</c> character or a malformed escape sequence is rejected with
		/// <see cref="SmtpAddressError.Syntax"/>.</para>
		/// <para>Only the syntax of the escape sequences is checked; the JIS X 0208 characters are not decoded, so
		/// a byte pair that does not correspond to an assigned character is not detected.</para>
		/// <para>ISO-2022-JP text is not permitted in an unquoted local-part since the <c>ESC ( B</c> escape sequence
		/// that must end the text contains a <c>(</c>, which would begin a comment.</para>
		/// <para>Since <see cref="MailboxAddress"/> does not permit control characters within a quoted local-part,
		/// such addresses can only be validated using the <see cref="string"/> overloads of
		/// <see cref="SmtpAddressValidator"/>.</para>
		/// </summary>
		AllowIso2022JpLocalPart = 1 << 0,

		/// <summary>
		/// <para>Enforce the length limits defined in <a href="https://www.rfc-editor.org/rfc/rfc5321#section-4.5.3.1">rfc5321</a>.</para>
		/// <list type="bullet">
		/// <item>The local-part must not exceed 64 octets (<see cref="SmtpAddressError.LocalPartTooLong"/>).</item>
		/// <item>Each label of the domain must not exceed 63 octets (<see cref="SmtpAddressError.LabelTooLong"/>).</item>
		/// <item>The domain must not exceed 253 octets (<see cref="SmtpAddressError.DomainTooLong"/>).</item>
		/// <item>The address must not exceed 254 octets, which is the 256-octet limit on a reverse-path or
		/// forward-path minus the angle brackets (<see cref="SmtpAddressError.AddressTooLong"/>).</item>
		/// </list>
		/// <para>The label and domain limits do not apply to address literals. The local-part length of an address
		/// that contains ISO-2022-JP text includes the escape sequences.</para>
		/// </summary>
		EnforceLengthLimits = 1 << 1
	}
}

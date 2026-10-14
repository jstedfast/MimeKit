//
// SmtpAddressError.cs
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

namespace MimeKit.Utils {
	/// <summary>
	/// The reason that an address failed SMTP address validation.
	/// </summary>
	/// <remarks>
	/// <para>The reason that an address failed validation by the <see cref="SmtpAddressValidator"/>.</para>
	/// <para>When an address violates more than one rule, the first violation found is reported. The
	/// rules are checked in the order that they are declared in this enum.</para>
	/// </remarks>
	public enum SmtpAddressError
	{
		/// <summary>
		/// The address is valid.
		/// </summary>
		None,

		/// <summary>
		/// The address does not match the <c>Mailbox</c> syntax defined in
		/// <a href="https://www.rfc-editor.org/rfc/rfc5321#section-4.1.2">rfc5321</a>.
		/// </summary>
		/// <remarks>
		/// An address that would match the <c>Mailbox</c> syntax if it were not for its non-ASCII characters
		/// is reported as <see cref="NonAscii"/> instead.
		/// </remarks>
		Syntax,

		/// <summary>
		/// The address contains non-ASCII characters and can only be used with the <c>SMTPUTF8</c> extension
		/// defined in <a href="https://www.rfc-editor.org/rfc/rfc6531">rfc6531</a>.
		/// </summary>
		/// <remarks>
		/// <para>Such an address can be validated using the <see cref="SmtpUtf8AddressValidator"/>.</para>
		/// <para>If the only non-ASCII characters are in the domain, the address can be sent without the
		/// <c>SMTPUTF8</c> extension by first encoding each internationalized label of the domain as an A-label
		/// (e.g. by using <see cref="MailboxAddress.GetAddress(bool)"/>).</para>
		/// </remarks>
		NonAscii,

		/// <summary>
		/// The local-part exceeds 64 octets.
		/// </summary>
		/// <remarks>
		/// This error is only reported when <see cref="SmtpAddressValidationOptions.EnforceLengthLimits"/> is used.
		/// </remarks>
		LocalPartTooLong,

		/// <summary>
		/// A label in the domain exceeds 63 octets.
		/// </summary>
		/// <remarks>
		/// This error is only reported when <see cref="SmtpAddressValidationOptions.EnforceLengthLimits"/> is used.
		/// </remarks>
		LabelTooLong,

		/// <summary>
		/// The domain exceeds 253 octets.
		/// </summary>
		/// <remarks>
		/// This error is only reported when <see cref="SmtpAddressValidationOptions.EnforceLengthLimits"/> is used.
		/// </remarks>
		DomainTooLong,

		/// <summary>
		/// The address exceeds 254 octets.
		/// </summary>
		/// <remarks>
		/// This error is only reported when <see cref="SmtpAddressValidationOptions.EnforceLengthLimits"/> is used.
		/// </remarks>
		AddressTooLong
	}
}

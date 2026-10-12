//
// SmtpUtf8AddressError.cs
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
	/// The reason that an address failed SMTPUTF8 address validation.
	/// </summary>
	/// <remarks>
	/// <para>The reason that an address failed validation by the <see cref="SmtpUtf8AddressValidator"/>.</para>
	/// <para>When an address violates more than one rule, the first violation found is reported. The
	/// rules are checked in the order that they are declared in this enum.</para>
	/// </remarks>
	public enum SmtpUtf8AddressError
	{
		/// <summary>
		/// The address is valid.
		/// </summary>
		None,

		/// <summary>
		/// The address does not match the <c>Mailbox</c> syntax defined in
		/// <a href="https://www.rfc-editor.org/rfc/rfc5321#section-4.1.2">rfc5321</a>
		/// as extended by <a href="https://www.rfc-editor.org/rfc/rfc6531#section-3.3">rfc6531</a>.
		/// </summary>
		Syntax,

		/// <summary>
		/// The local-part exceeds 64 octets when encoded in UTF-8.
		/// </summary>
		/// <remarks>
		/// This error is only reported when <see cref="SmtpUtf8AddressValidationOptions.EnforceLengthLimits"/> is used.
		/// </remarks>
		LocalPartTooLong,

		/// <summary>
		/// A label in the domain exceeds 63 octets when encoded as an A-label.
		/// </summary>
		/// <remarks>
		/// This error is only reported when <see cref="SmtpUtf8AddressValidationOptions.EnforceLengthLimits"/> is used.
		/// </remarks>
		LabelTooLong,

		/// <summary>
		/// The domain exceeds 253 octets when each of its labels is encoded as an A-label.
		/// </summary>
		/// <remarks>
		/// This error is only reported when <see cref="SmtpUtf8AddressValidationOptions.EnforceLengthLimits"/> is used.
		/// </remarks>
		DomainTooLong,

		/// <summary>
		/// The address exceeds 254 octets when encoded in UTF-8.
		/// </summary>
		/// <remarks>
		/// This error is only reported when <see cref="SmtpUtf8AddressValidationOptions.EnforceLengthLimits"/> is used.
		/// </remarks>
		AddressTooLong,

		/// <summary>
		/// An atom in the local-part or a label in the domain begins with the ACE prefix (<c>xn--</c>).
		/// </summary>
		ALabel,

		/// <summary>
		/// The address contains a code point that is not permitted by the PRECIS IdentifierClass
		/// (<a href="https://www.rfc-editor.org/rfc/rfc8264">rfc8264</a>) and is not U+0020 SPACE.
		/// </summary>
		DisallowedCodePoint,

		/// <summary>
		/// The address contains a code point that is only permitted in certain contexts (such as
		/// U+200D ZERO WIDTH JOINER or U+00B7 MIDDLE DOT) and the context rule defined in
		/// <a href="https://www.rfc-editor.org/rfc/rfc5892#appendix-A">rfc5892</a> is not satisfied.
		/// </summary>
		ContextRule,

		/// <summary>
		/// The address contains code points from more than one non-ASCII script.
		/// </summary>
		MixedScripts
	}
}

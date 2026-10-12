//
// SmtpUtf8AddressValidationOptions.cs
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
	/// Options for SMTPUTF8 address validation.
	/// </summary>
	/// <remarks>
	/// Options that change the behavior of the <see cref="SmtpUtf8AddressValidator"/>.
	/// </remarks>
	[Flags]
	public enum SmtpUtf8AddressValidationOptions
	{
		/// <summary>
		/// Validate the address exactly as written. A domain label that is an A-label (i.e. begins with
		/// <c>xn--</c>) is rejected with <see cref="SmtpUtf8AddressError.ALabel"/>.
		/// </summary>
		None = 0,

		/// <summary>
		/// <para>Decode each A-label in the domain into its Unicode form (U-label) before validating the address.</para>
		/// <para>The decoded domain is subject to all of the code point, contextual and script rules, which makes
		/// this option useful for checking whether an address that will be displayed to a user (in its decoded
		/// form) might be an attempt to spoof another address.</para>
		/// <para>A-labels are decoded using MimeKit's own implementation of the Punycode algorithm described in
		/// <a href="https://www.rfc-editor.org/rfc/rfc3492">rfc3492</a> rather than
		/// <see cref="System.Globalization.IdnMapping"/>, so the results are the same on every platform and
		/// target framework. A domain label is rejected with <see cref="SmtpUtf8AddressError.ALabel"/> if it is
		/// longer than 63 characters, is not valid Punycode, is not in canonical form (i.e. re-encoding the
		/// decoded label does not reproduce the original A-label), decodes to a surrogate or to only ASCII
		/// characters, or decodes to a U-label that begins or ends with a hyphen or that has hyphens in both
		/// the third and fourth positions. Any other problem with the decoded U-label is reported in the same way
		/// as it would be for a U-label that was not encoded (e.g. <c>xn--a</c> decodes to U+0080, which is
		/// reported as <see cref="SmtpUtf8AddressError.DisallowedCodePoint"/>).</para>
		/// <para>Atoms in the local-part that begin with <c>xn--</c> are not IDN-encoded and are still rejected
		/// with <see cref="SmtpUtf8AddressError.ALabel"/>.</para>
		/// </summary>
		DecodeIdn = 1 << 0,

		/// <summary>
		/// <para>Treat ASCII letters as belonging to the Latin script when checking whether an address mixes scripts.</para>
		/// <para>The rules defined by draft-ietf-mailmaint-smtputf8-syntax disregard ASCII when counting scripts,
		/// which means that an address such as <c>paypal@рaypal.com</c> (where the first letter of the domain is
		/// U+0440 CYRILLIC SMALL LETTER ER) is considered valid. When this option is set, such an address is
		/// rejected with <see cref="SmtpUtf8AddressError.MixedScripts"/>.</para>
		/// <para>When this option is set, ASCII letters are counted separately for the local-part and for each label
		/// of the domain, so addresses such as <c>иван@example.com</c> and <c>user@пример.com</c> are still valid.
		/// The non-ASCII characters of the address must still all belong to a single script, so this option never
		/// accepts an address that would otherwise be rejected. However, this option rejects labels that
		/// legitimately mix ASCII letters with a non-Latin script, such as <c>info@москва-shop.ru</c>.</para>
		/// </summary>
		AsciiIsLatin = 1 << 1,

		/// <summary>
		/// <para>Enforce the length limits defined in <a href="https://www.rfc-editor.org/rfc/rfc5321#section-4.5.3.1">rfc5321</a>
		/// as extended by <a href="https://www.rfc-editor.org/rfc/rfc6531#section-3.3">rfc6531</a>.</para>
		/// <list type="bullet">
		/// <item>The local-part must not exceed 64 octets when encoded in UTF-8
		/// (<see cref="SmtpUtf8AddressError.LocalPartTooLong"/>).</item>
		/// <item>Each label of the domain must not exceed 63 octets when encoded as an A-label
		/// (<see cref="SmtpUtf8AddressError.LabelTooLong"/>).</item>
		/// <item>The domain must not exceed 253 octets when each of its labels is encoded as an A-label
		/// (<see cref="SmtpUtf8AddressError.DomainTooLong"/>).</item>
		/// <item>The address must not exceed 254 octets when encoded in UTF-8, which is the 256-octet limit
		/// on a reverse-path or forward-path minus the angle brackets (<see cref="SmtpUtf8AddressError.AddressTooLong"/>).</item>
		/// </list>
		/// <para>The label and domain limits do not apply to address literals.</para>
		/// </summary>
		EnforceLengthLimits = 1 << 2
	}
}
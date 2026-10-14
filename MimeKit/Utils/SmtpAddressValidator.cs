//
// SmtpAddressValidator.cs
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
	/// A validator for (non-internationalized) SMTP email addresses.
	/// </summary>
	/// <remarks>
	/// <para>Validates email addresses that will be used in the <c>MAIL FROM</c> and <c>RCPT TO</c> commands of an
	/// SMTP transaction that does not use the <c>SMTPUTF8</c> extension.</para>
	/// <para>An address is valid if and only if it matches the <c>Mailbox</c> syntax defined in
	/// <a href="https://www.rfc-editor.org/rfc/rfc5321#section-4.1.2">rfc5321</a>, which only permits ASCII
	/// characters. An internationalized domain must therefore be expressed using A-labels (such as
	/// <c>user@xn--gr-zia.example.com</c>), which are permitted. An address that would be valid if it were not
	/// for its non-ASCII characters is rejected with <see cref="SmtpAddressError.NonAscii"/>, which indicates that
	/// the address requires the <c>SMTPUTF8</c> extension. Such addresses can be validated using the
	/// <see cref="SmtpUtf8AddressValidator"/>.</para>
	/// <para>The length limits defined in rfc5321 are only enforced when
	/// <see cref="SmtpAddressValidationOptions.EnforceLengthLimits"/> is specified.</para>
	/// </remarks>
	public static class SmtpAddressValidator
	{
		/// <summary>
		/// Check whether an address is a valid SMTP address.
		/// </summary>
		/// <remarks>
		/// <para>Checks whether an <c>addr-spec</c> (such as <c>user@example.com</c>) is a valid SMTP address
		/// as defined by <a href="https://www.rfc-editor.org/rfc/rfc5321#section-4.1.2">rfc5321</a>.</para>
		/// <para>The address must not include a display name, angle brackets, comments, or folding whitespace.</para>
		/// </remarks>
		/// <returns><see langword="true" /> if the address is valid; otherwise, <see langword="false" />.</returns>
		/// <param name="address">The address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="address"/> is <see langword="null"/>.
		/// </exception>
		public static bool IsValid (string address)
		{
			return TryValidate (address, SmtpAddressValidationOptions.None, out _);
		}

		/// <summary>
		/// Check whether an address is a valid SMTP address.
		/// </summary>
		/// <remarks>
		/// <para>Checks whether an <c>addr-spec</c> (such as <c>user@example.com</c>) is a valid SMTP address
		/// as defined by <a href="https://www.rfc-editor.org/rfc/rfc5321#section-4.1.2">rfc5321</a>,
		/// using the specified options.</para>
		/// <para>The address must not include a display name, angle brackets, comments, or folding whitespace.</para>
		/// </remarks>
		/// <returns><see langword="true" /> if the address is valid; otherwise, <see langword="false" />.</returns>
		/// <param name="address">The address.</param>
		/// <param name="options">The validation options.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="address"/> is <see langword="null"/>.
		/// </exception>
		public static bool IsValid (string address, SmtpAddressValidationOptions options)
		{
			return TryValidate (address, options, out _);
		}

		/// <summary>
		/// Check whether a mailbox address is a valid SMTP address.
		/// </summary>
		/// <remarks>
		/// <para>Checks whether the <see cref="MailboxAddress.Address"/> of the mailbox is a valid SMTP address
		/// as defined by <a href="https://www.rfc-editor.org/rfc/rfc5321#section-4.1.2">rfc5321</a>.
		/// The display name and the route of the mailbox are not validated.</para>
		/// <note type="note">MimeKit decodes an IDN-encoded domain (such as <c>xn--gr-zia.example.com</c>) into its
		/// Unicode form when it parses an address, so such a domain is reported as
		/// <see cref="SmtpAddressError.NonAscii"/>. To validate the form of the address that would be used in an
		/// SMTP transaction that does not use the <c>SMTPUTF8</c> extension, validate the string returned by
		/// <see cref="MailboxAddress.GetAddress(bool)"/> with a value of <see langword="true"/> instead.</note>
		/// </remarks>
		/// <returns><see langword="true" /> if the address is valid; otherwise, <see langword="false" />.</returns>
		/// <param name="mailbox">The mailbox address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailbox"/> is <see langword="null"/>.
		/// </exception>
		public static bool IsValid (MailboxAddress mailbox)
		{
			return TryValidate (mailbox, SmtpAddressValidationOptions.None, out _);
		}

		/// <summary>
		/// Check whether a mailbox address is a valid SMTP address.
		/// </summary>
		/// <remarks>
		/// <para>Checks whether the <see cref="MailboxAddress.Address"/> of the mailbox is a valid SMTP address
		/// as defined by <a href="https://www.rfc-editor.org/rfc/rfc5321#section-4.1.2">rfc5321</a>,
		/// using the specified options. The display name and the route of the mailbox are not validated.</para>
		/// <note type="note">MimeKit decodes an IDN-encoded domain (such as <c>xn--gr-zia.example.com</c>) into its
		/// Unicode form when it parses an address, so such a domain is reported as
		/// <see cref="SmtpAddressError.NonAscii"/>. To validate the form of the address that would be used in an
		/// SMTP transaction that does not use the <c>SMTPUTF8</c> extension, validate the string returned by
		/// <see cref="MailboxAddress.GetAddress(bool)"/> with a value of <see langword="true"/> instead.</note>
		/// </remarks>
		/// <returns><see langword="true" /> if the address is valid; otherwise, <see langword="false" />.</returns>
		/// <param name="mailbox">The mailbox address.</param>
		/// <param name="options">The validation options.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailbox"/> is <see langword="null"/>.
		/// </exception>
		public static bool IsValid (MailboxAddress mailbox, SmtpAddressValidationOptions options)
		{
			return TryValidate (mailbox, options, out _);
		}

		/// <summary>
		/// Validate an address as an SMTP address.
		/// </summary>
		/// <remarks>
		/// <para>Validates an <c>addr-spec</c> (such as <c>user@example.com</c>) as an SMTP address as defined by
		/// <a href="https://www.rfc-editor.org/rfc/rfc5321#section-4.1.2">rfc5321</a>.</para>
		/// <para>The address must not include a display name, angle brackets, comments, or folding whitespace.</para>
		/// </remarks>
		/// <returns><see langword="true" /> if the address is valid; otherwise, <see langword="false" />.</returns>
		/// <param name="address">The address.</param>
		/// <param name="error">The reason that the address is invalid or <see cref="SmtpAddressError.None"/> if it is valid.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="address"/> is <see langword="null"/>.
		/// </exception>
		public static bool TryValidate (string address, out SmtpAddressError error)
		{
			return TryValidate (address, SmtpAddressValidationOptions.None, out error);
		}

		/// <summary>
		/// Validate an address as an SMTP address.
		/// </summary>
		/// <remarks>
		/// <para>Validates an <c>addr-spec</c> (such as <c>user@example.com</c>) as an SMTP address as defined by
		/// <a href="https://www.rfc-editor.org/rfc/rfc5321#section-4.1.2">rfc5321</a>, using the specified options.</para>
		/// <para>The address must not include a display name, angle brackets, comments, or folding whitespace.</para>
		/// </remarks>
		/// <returns><see langword="true" /> if the address is valid; otherwise, <see langword="false" />.</returns>
		/// <param name="address">The address.</param>
		/// <param name="options">The validation options.</param>
		/// <param name="error">The reason that the address is invalid or <see cref="SmtpAddressError.None"/> if it is valid.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="address"/> is <see langword="null"/>.
		/// </exception>
		public static bool TryValidate (string address, SmtpAddressValidationOptions options, out SmtpAddressError error)
		{
			if (address is null)
				throw new ArgumentNullException (nameof (address));

			error = Validate (address, options);

			return error == SmtpAddressError.None;
		}

		/// <summary>
		/// Validate a mailbox address as an SMTP address.
		/// </summary>
		/// <remarks>
		/// <para>Validates the <see cref="MailboxAddress.Address"/> of the mailbox as an SMTP address as defined by
		/// <a href="https://www.rfc-editor.org/rfc/rfc5321#section-4.1.2">rfc5321</a>.
		/// The display name and the route of the mailbox are not validated.</para>
		/// <note type="note">MimeKit decodes an IDN-encoded domain (such as <c>xn--gr-zia.example.com</c>) into its
		/// Unicode form when it parses an address, so such a domain is reported as
		/// <see cref="SmtpAddressError.NonAscii"/>. To validate the form of the address that would be used in an
		/// SMTP transaction that does not use the <c>SMTPUTF8</c> extension, validate the string returned by
		/// <see cref="MailboxAddress.GetAddress(bool)"/> with a value of <see langword="true"/> instead.</note>
		/// </remarks>
		/// <returns><see langword="true" /> if the address is valid; otherwise, <see langword="false" />.</returns>
		/// <param name="mailbox">The mailbox address.</param>
		/// <param name="error">The reason that the address is invalid or <see cref="SmtpAddressError.None"/> if it is valid.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailbox"/> is <see langword="null"/>.
		/// </exception>
		public static bool TryValidate (MailboxAddress mailbox, out SmtpAddressError error)
		{
			return TryValidate (mailbox, SmtpAddressValidationOptions.None, out error);
		}

		/// <summary>
		/// Validate a mailbox address as an SMTP address.
		/// </summary>
		/// <remarks>
		/// <para>Validates the <see cref="MailboxAddress.Address"/> of the mailbox as an SMTP address as defined by
		/// <a href="https://www.rfc-editor.org/rfc/rfc5321#section-4.1.2">rfc5321</a>, using the specified options.
		/// The display name and the route of the mailbox are not validated.</para>
		/// <note type="note">MimeKit decodes an IDN-encoded domain (such as <c>xn--gr-zia.example.com</c>) into its
		/// Unicode form when it parses an address, so such a domain is reported as
		/// <see cref="SmtpAddressError.NonAscii"/>. To validate the form of the address that would be used in an
		/// SMTP transaction that does not use the <c>SMTPUTF8</c> extension, validate the string returned by
		/// <see cref="MailboxAddress.GetAddress(bool)"/> with a value of <see langword="true"/> instead.</note>
		/// </remarks>
		/// <returns><see langword="true" /> if the address is valid; otherwise, <see langword="false" />.</returns>
		/// <param name="mailbox">The mailbox address.</param>
		/// <param name="options">The validation options.</param>
		/// <param name="error">The reason that the address is invalid or <see cref="SmtpAddressError.None"/> if it is valid.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailbox"/> is <see langword="null"/>.
		/// </exception>
		public static bool TryValidate (MailboxAddress mailbox, SmtpAddressValidationOptions options, out SmtpAddressError error)
		{
			if (mailbox is null)
				throw new ArgumentNullException (nameof (mailbox));

			error = Validate (mailbox.Address, options);

			return error == SmtpAddressError.None;
		}

		static SmtpAddressError Validate (string address, SmtpAddressValidationOptions options)
		{
			bool allowIso2022Jp = (options & SmtpAddressValidationOptions.AllowIso2022JpLocalPart) != 0;

			// Note: The syntax permits UTF8-non-ascii (as extended by rfc6531) so that an address that is only
			// invalid because of its non-ASCII characters can be reported as requiring SMTPUTF8.
			if (!SmtpAddressSyntax.ValidateSyntax (address, allowIso2022Jp, out _, out _, out int domainIndex))
				return SmtpAddressError.Syntax;

			for (int i = 0; i < address.Length; i++) {
				if (address[i] >= 0x80)
					return SmtpAddressError.NonAscii;
			}

			if ((options & SmtpAddressValidationOptions.EnforceLengthLimits) != 0) {
				// Note: Since the address is ASCII, each character is a single octet and each label is its own A-label.
				switch (SmtpUtf8AddressValidator.ValidateLengths (address, domainIndex)) {
				case SmtpUtf8AddressError.LocalPartTooLong: return SmtpAddressError.LocalPartTooLong;
				case SmtpUtf8AddressError.LabelTooLong: return SmtpAddressError.LabelTooLong;
				case SmtpUtf8AddressError.DomainTooLong: return SmtpAddressError.DomainTooLong;
				case SmtpUtf8AddressError.AddressTooLong: return SmtpAddressError.AddressTooLong;
				}
			}

			return SmtpAddressError.None;
		}
	}
}

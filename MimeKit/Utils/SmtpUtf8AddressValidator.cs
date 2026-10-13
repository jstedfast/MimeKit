//
// SmtpUtf8AddressValidator.cs
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
using System.Buffers;
using System.Buffers.Binary;

namespace MimeKit.Utils {
	/// <summary>
	/// A validator for internationalized (SMTPUTF8) email addresses.
	/// </summary>
	/// <remarks>
	/// <para>Validates email addresses according to the rules defined in
	/// <a href="https://datatracker.ietf.org/doc/draft-ietf-mailmaint-smtputf8-syntax/">draft-ietf-mailmaint-smtputf8-syntax</a>,
	/// which restricts the code points that may be used in an internationalized email address in order to
	/// prevent addresses that render identically to (or deceptively similar to) other addresses.</para>
	/// <para>An address is valid if and only if:</para>
	/// <list type="number">
	/// <item>It matches the <c>Mailbox</c> syntax defined in
	/// <a href="https://www.rfc-editor.org/rfc/rfc5321#section-4.1.2">rfc5321</a> as extended by
	/// <a href="https://www.rfc-editor.org/rfc/rfc6531#section-3.3">rfc6531</a>.</item>
	/// <item>No atom in the local-part and no label in the domain is an A-label (i.e. begins with <c>xn--</c>).</item>
	/// <item>Every code point is either U+0020 SPACE or is permitted by the PRECIS IdentifierClass defined in
	/// <a href="https://www.rfc-editor.org/rfc/rfc8264">rfc8264</a>, including the contextual rules for
	/// CONTEXTJ and CONTEXTO code points defined in
	/// <a href="https://www.rfc-editor.org/rfc/rfc5892#appendix-A">rfc5892</a>.</item>
	/// <item>The address is in Unicode Normalization Form C (NFC) as defined by
	/// <a href="https://www.unicode.org/reports/tr15/">Unicode Standard Annex #15</a>.</item>
	/// <item>The code points belong to no more than one non-ASCII script, where code points in the
	/// <c>Common</c> and <c>Inherited</c> scripts do not count toward that limit.</item>
	/// </list>
	/// <para>The length limits
	/// defined in rfc5321 are only enforced when <see cref="SmtpUtf8AddressValidationOptions.EnforceLengthLimits"/>
	/// is specified.</para>
	/// <para>This validator has two primary uses:</para>
	/// <list type="bullet">
	/// <item>Validating addresses that will be used in their internationalized (UTF-8) form, such as the
	/// addresses in the <c>MAIL FROM</c> and <c>RCPT TO</c> commands of an SMTP transaction that uses the
	/// <c>SMTPUTF8</c> extension, or addresses that are being created or registered. SMTPUTF8 addresses must
	/// use U-labels, so by default an IDN-encoded domain such as <c>user@xn--gr-zia.example.com</c> is
	/// rejected (even though it is the correct way to express an internationalized domain in a context that
	/// only supports ASCII).</item>
	/// <item>Detecting addresses that may be an attempt to spoof another address, such as addresses that
	/// mix scripts or that contain invisible or otherwise problematic code points, so that an application
	/// can warn the user. For this purpose, use <see cref="SmtpUtf8AddressValidationOptions.DecodeIdn"/> so
	/// that IDN-encoded domains are decoded into the Unicode form that would be displayed to the user and
	/// then validated. Consider also using <see cref="SmtpUtf8AddressValidationOptions.AsciiIsLatin"/>, which
	/// detects ASCII letters mixed with look-alike letters from another script (such as Cyrillic).</item>
	/// </list>
	/// </remarks>
	public static partial class SmtpUtf8AddressValidator
	{
		enum PrecisClass : byte
		{
			Disallowed = 0,
			PValid = 1,
			ContextJ = 2,
			ContextO = 3
		}

		enum JoiningType : byte
		{
			NonJoining = 0,
			Left = 1,
			Dual = 2,
			Right = 3,
			Transparent = 4,
			JoinCausing = 5
		}

		const int ZeroWidthNonJoiner = 0x200C;
		const int ZeroWidthJoiner = 0x200D;

		/// <summary>
		/// Check whether an address is a valid SMTPUTF8 address.
		/// </summary>
		/// <remarks>
		/// <para>Checks whether an <c>addr-spec</c> (such as <c>user@example.com</c>) is a valid
		/// SMTPUTF8 address as defined by
		/// <a href="https://datatracker.ietf.org/doc/draft-ietf-mailmaint-smtputf8-syntax/">draft-ietf-mailmaint-smtputf8-syntax</a>.</para>
		/// <para>The address must not include a display name, angle brackets, comments, or folding whitespace.</para>
		/// </remarks>
		/// <returns><see langword="true" /> if the address is valid; otherwise, <see langword="false" />.</returns>
		/// <param name="address">The address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="address"/> is <see langword="null"/>.
		/// </exception>
		public static bool IsValid (string address)
		{
			return TryValidate (address, SmtpUtf8AddressValidationOptions.None, out _);
		}

		/// <summary>
		/// Check whether an address is a valid SMTPUTF8 address.
		/// </summary>
		/// <remarks>
		/// <para>Checks whether an <c>addr-spec</c> (such as <c>user@example.com</c>) is a valid
		/// SMTPUTF8 address as defined by
		/// <a href="https://datatracker.ietf.org/doc/draft-ietf-mailmaint-smtputf8-syntax/">draft-ietf-mailmaint-smtputf8-syntax</a>,
		/// using the specified options.</para>
		/// <para>The address must not include a display name, angle brackets, comments, or folding whitespace.</para>
		/// </remarks>
		/// <returns><see langword="true" /> if the address is valid; otherwise, <see langword="false" />.</returns>
		/// <param name="address">The address.</param>
		/// <param name="options">The validation options.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="address"/> is <see langword="null"/>.
		/// </exception>
		public static bool IsValid (string address, SmtpUtf8AddressValidationOptions options)
		{
			return TryValidate (address, options, out _);
		}

		/// <summary>
		/// Check whether a mailbox address is a valid SMTPUTF8 address.
		/// </summary>
		/// <remarks>
		/// <para>Checks whether the <see cref="MailboxAddress.Address"/> of the mailbox is a valid
		/// SMTPUTF8 address as defined by
		/// <a href="https://datatracker.ietf.org/doc/draft-ietf-mailmaint-smtputf8-syntax/">draft-ietf-mailmaint-smtputf8-syntax</a>.
		/// The display name and the route of the mailbox are not validated.</para>
		/// <note type="note">MimeKit decodes an IDN-encoded domain (such as <c>xn--gr-zia.example.com</c>) into its
		/// Unicode form when it parses an address, so this method validates the Unicode form of the domain rather
		/// than the A-label. All of the code point, contextual and script rules are still applied to the decoded
		/// domain, and a domain that cannot be decoded is left in its encoded form and is rejected. To determine
		/// whether the original text contained an A-label, validate the original address string instead.</note>
		/// </remarks>
		/// <returns><see langword="true" /> if the address is valid; otherwise, <see langword="false" />.</returns>
		/// <param name="mailbox">The mailbox address.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailbox"/> is <see langword="null"/>.
		/// </exception>
		public static bool IsValid (MailboxAddress mailbox)
		{
			return TryValidate (mailbox, SmtpUtf8AddressValidationOptions.None, out _);
		}

		/// <summary>
		/// Check whether a mailbox address is a valid SMTPUTF8 address.
		/// </summary>
		/// <remarks>
		/// <para>Checks whether the <see cref="MailboxAddress.Address"/> of the mailbox is a valid
		/// SMTPUTF8 address as defined by
		/// <a href="https://datatracker.ietf.org/doc/draft-ietf-mailmaint-smtputf8-syntax/">draft-ietf-mailmaint-smtputf8-syntax</a>,
		/// using the specified options. The display name and the route of the mailbox are not validated.</para>
		/// <note type="note">MimeKit decodes an IDN-encoded domain (such as <c>xn--gr-zia.example.com</c>) into its
		/// Unicode form when it parses an address, so this method validates the Unicode form of the domain rather
		/// than the A-label. All of the code point, contextual and script rules are still applied to the decoded
		/// domain, and a domain that cannot be decoded is left in its encoded form and is rejected. To determine
		/// whether the original text contained an A-label, validate the original address string instead.</note>
		/// </remarks>
		/// <returns><see langword="true" /> if the address is valid; otherwise, <see langword="false" />.</returns>
		/// <param name="mailbox">The mailbox address.</param>
		/// <param name="options">The validation options.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailbox"/> is <see langword="null"/>.
		/// </exception>
		public static bool IsValid (MailboxAddress mailbox, SmtpUtf8AddressValidationOptions options)
		{
			return TryValidate (mailbox, options, out _);
		}

		/// <summary>
		/// Validate an address as an SMTPUTF8 address.
		/// </summary>
		/// <remarks>
		/// <para>Validates an <c>addr-spec</c> (such as <c>user@example.com</c>) as an SMTPUTF8 address as defined by
		/// <a href="https://datatracker.ietf.org/doc/draft-ietf-mailmaint-smtputf8-syntax/">draft-ietf-mailmaint-smtputf8-syntax</a>.</para>
		/// <para>The address must not include a display name, angle brackets, comments, or folding whitespace.</para>
		/// </remarks>
		/// <returns><see langword="true" /> if the address is valid; otherwise, <see langword="false" />.</returns>
		/// <param name="address">The address.</param>
		/// <param name="error">The reason that the address is invalid or <see cref="SmtpUtf8AddressError.None"/> if it is valid.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="address"/> is <see langword="null"/>.
		/// </exception>
		public static bool TryValidate (string address, out SmtpUtf8AddressError error)
		{
			return TryValidate (address, SmtpUtf8AddressValidationOptions.None, out error);
		}

		/// <summary>
		/// Validate an address as an SMTPUTF8 address.
		/// </summary>
		/// <remarks>
		/// <para>Validates an <c>addr-spec</c> (such as <c>user@example.com</c>) as an SMTPUTF8 address as defined by
		/// <a href="https://datatracker.ietf.org/doc/draft-ietf-mailmaint-smtputf8-syntax/">draft-ietf-mailmaint-smtputf8-syntax</a>,
		/// using the specified options.</para>
		/// <para>The address must not include a display name, angle brackets, comments, or folding whitespace.</para>
		/// </remarks>
		/// <returns><see langword="true" /> if the address is valid; otherwise, <see langword="false" />.</returns>
		/// <param name="address">The address.</param>
		/// <param name="options">The validation options.</param>
		/// <param name="error">The reason that the address is invalid or <see cref="SmtpUtf8AddressError.None"/> if it is valid.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="address"/> is <see langword="null"/>.
		/// </exception>
		public static bool TryValidate (string address, SmtpUtf8AddressValidationOptions options, out SmtpUtf8AddressError error)
		{
			if (address is null)
				throw new ArgumentNullException (nameof (address));

			error = Validate (address, options);

			return error == SmtpUtf8AddressError.None;
		}

		/// <summary>
		/// Validate a mailbox address as an SMTPUTF8 address.
		/// </summary>
		/// <remarks>
		/// <para>Validates the <see cref="MailboxAddress.Address"/> of the mailbox as an SMTPUTF8 address as defined by
		/// <a href="https://datatracker.ietf.org/doc/draft-ietf-mailmaint-smtputf8-syntax/">draft-ietf-mailmaint-smtputf8-syntax</a>.
		/// The display name and the route of the mailbox are not validated.</para>
		/// <note type="note">MimeKit decodes an IDN-encoded domain (such as <c>xn--gr-zia.example.com</c>) into its
		/// Unicode form when it parses an address, so this method validates the Unicode form of the domain rather
		/// than the A-label. All of the code point, contextual and script rules are still applied to the decoded
		/// domain, and a domain that cannot be decoded is left in its encoded form and is rejected with
		/// <see cref="SmtpUtf8AddressError.ALabel"/>. To determine whether the original text contained an A-label,
		/// validate the original address string instead.</note>
		/// </remarks>
		/// <returns><see langword="true" /> if the address is valid; otherwise, <see langword="false" />.</returns>
		/// <param name="mailbox">The mailbox address.</param>
		/// <param name="error">The reason that the address is invalid or <see cref="SmtpUtf8AddressError.None"/> if it is valid.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailbox"/> is <see langword="null"/>.
		/// </exception>
		public static bool TryValidate (MailboxAddress mailbox, out SmtpUtf8AddressError error)
		{
			return TryValidate (mailbox, SmtpUtf8AddressValidationOptions.None, out error);
		}

		/// <summary>
		/// Validate a mailbox address as an SMTPUTF8 address.
		/// </summary>
		/// <remarks>
		/// <para>Validates the <see cref="MailboxAddress.Address"/> of the mailbox as an SMTPUTF8 address as defined by
		/// <a href="https://datatracker.ietf.org/doc/draft-ietf-mailmaint-smtputf8-syntax/">draft-ietf-mailmaint-smtputf8-syntax</a>,
		/// using the specified options. The display name and the route of the mailbox are not validated.</para>
		/// <note type="note">MimeKit decodes an IDN-encoded domain (such as <c>xn--gr-zia.example.com</c>) into its
		/// Unicode form when it parses an address, so this method validates the Unicode form of the domain rather
		/// than the A-label. All of the code point, contextual and script rules are still applied to the decoded
		/// domain, and a domain that cannot be decoded is left in its encoded form and is rejected with
		/// <see cref="SmtpUtf8AddressError.ALabel"/>. To determine whether the original text contained an A-label,
		/// validate the original address string instead.</note>
		/// </remarks>
		/// <returns><see langword="true" /> if the address is valid; otherwise, <see langword="false" />.</returns>
		/// <param name="mailbox">The mailbox address.</param>
		/// <param name="options">The validation options.</param>
		/// <param name="error">The reason that the address is invalid or <see cref="SmtpUtf8AddressError.None"/> if it is valid.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="mailbox"/> is <see langword="null"/>.
		/// </exception>
		public static bool TryValidate (MailboxAddress mailbox, SmtpUtf8AddressValidationOptions options, out SmtpUtf8AddressError error)
		{
			if (mailbox is null)
				throw new ArgumentNullException (nameof (mailbox));

			error = Validate (mailbox.Address, options);

			return error == SmtpUtf8AddressError.None;
		}

		static SmtpUtf8AddressError Validate (string address, SmtpUtf8AddressValidationOptions options)
		{
			if (!ValidateSyntax (address, out bool localPartALabel, out bool domainALabel, out int domainIndex))
				return SmtpUtf8AddressError.Syntax;

			if ((options & SmtpUtf8AddressValidationOptions.EnforceLengthLimits) != 0) {
				var error = ValidateLengths (address, domainIndex);

				if (error != SmtpUtf8AddressError.None)
					return error;
			}

			if (localPartALabel)
				return SmtpUtf8AddressError.ALabel;

			if (domainALabel) {
				if ((options & SmtpUtf8AddressValidationOptions.DecodeIdn) == 0)
					return SmtpUtf8AddressError.ALabel;

				return ValidateDecodedAddress (address, domainIndex, options);
			}

			return ValidateCodePoints (address.AsSpan (), domainIndex, options);
		}

		static SmtpUtf8AddressError ValidateDecodedAddress (string address, int domainIndex, SmtpUtf8AddressValidationOptions options)
		{
			// Each decoded code point consumes at least one character of its A-label and needs at most two UTF-16
			// code units, so the decoded address can be at most twice as long as the original.
			const int MaxStackBufferSize = 512;
			int bufferSize = domainIndex + (address.Length - domainIndex) * 2;
			char[]? rented = null;

			Span<char> buffer = bufferSize <= MaxStackBufferSize ? stackalloc char[MaxStackBufferSize] : (rented = ArrayPool<char>.Shared.Rent (bufferSize));

			try {
				if (!TryDecodeDomain (address, domainIndex, buffer, out int length))
					return SmtpUtf8AddressError.ALabel;

				return ValidateCodePoints (buffer.Slice (0, length), domainIndex, options);
			} finally {
				if (rented != null)
					ArrayPool<char>.Shared.Return (rented);
			}
		}

		// Decodes each A-label in the domain into a U-label, copying the rest of the address as-is.
		static bool TryDecodeDomain (string address, int domainIndex, Span<char> output, out int length)
		{
			int index = domainIndex;

			address.AsSpan (0, domainIndex).CopyTo (output);
			length = domainIndex;

			do {
				int startIndex = index;

				while (index < address.Length && address[index] != '.')
					index++;

				if (HasAcePrefix (address, startIndex, index)) {
					if (!TryDecodeALabel (address, startIndex, index, output.Slice (length), out int written))
						return false;

					length += written;
				} else {
					address.AsSpan (startIndex, index - startIndex).CopyTo (output.Slice (length));
					length += index - startIndex;
				}

				if (index < address.Length)
					output[length++] = address[index++];
			} while (index < address.Length);

			return true;
		}

		// Decodes an A-label into a U-label.
		//
		// Note: System.Globalization.IdnMapping is deliberately not used for this because its behavior depends on
		// the platform, which would make the result of validating an address platform-dependent as well:
		//
		// * On the .NET Framework, IdnMapping uses the Windows NLS APIs, which implement IDNA2003 rather than
		//   IDNA2008 (for example, it maps U+00DF LATIN SMALL LETTER SHARP S to "ss"). This is why MimeKit
		//   introduced the IPunycode interface (see https://github.com/jstedfast/MimeKit/issues/801).
		// * On .NET Core and .NET 5+, IdnMapping uses ICU, so its behavior depends on the version of ICU that is
		//   installed. Older versions of ICU reject labels that newer versions accept
		//   (see https://github.com/dotnet/runtime/issues/107900).
		// * In globalization-invariant mode, IdnMapping uses a separate managed implementation that does not
		//   behave the same as the ICU implementation (see https://github.com/dotnet/runtime/issues/37349).
		// * IdnMapping applies the UTS #46 mapping and validity rules rather than the rules defined by
		//   draft-ietf-mailmaint-smtputf8-syntax, and it does not expose the UTS #46 processing options needed
		//   to control them (see https://github.com/dotnet/runtime/issues/101887).
		//
		// Instead, the A-label is decoded using the Punycode algorithm (RFC 3492, Section 6.2), which is the same on
		// every platform, and the resulting U-label is then subject to the same code point, contextual and script
		// rules as any other label. This also avoids allocating strings.
		//
		// To prevent an A-label from sneaking past the validator by decoding to something other than what it
		// appears to encode, the A-label must:
		//
		// * Not exceed 63 octets (RFC 5890, Section 2.3.2.1).
		// * Contain only ASCII characters and must decode without overflow to valid Unicode scalar values.
		// * Decode to a label that contains at least one non-ASCII code point (RFC 5890, Section 2.3.2.1).
		// * Decode to a label that neither begins nor ends with a hyphen and that does not contain hyphens in both
		//   the third and fourth positions (RFC 5891, Section 4.2.3.1).
		// * Be exactly what encoding the decoded label produces, ignoring case (RFC 5891, Section 5.4).
		static bool TryDecodeALabel (string text, int startIndex, int endIndex, Span<char> output, out int written)
		{
			const int AcePrefixLength = 4;
			Span<int> label = stackalloc int[MaxLabelLength];
			bool nonAscii = false;

			written = 0;

			if (endIndex - startIndex > MaxLabelLength)
				return false;

			var punycode = text.AsSpan (startIndex + AcePrefixLength, endIndex - startIndex - AcePrefixLength);

			if (!Rfc3492.TryDecode (punycode, label, out int count))
				return false;

			for (int j = 0; j < count; j++) {
				if (label[j] >= 0x80) {
					nonAscii = true;
					break;
				}
			}

			if (!nonAscii)
				return false;

			if (label[0] == '-' || label[count - 1] == '-' || (count >= 4 && label[2] == '-' && label[3] == '-'))
				return false;

			Span<char> encoded = stackalloc char[MaxLabelLength];
			int encodedLength = Rfc3492.Encode (label.Slice (0, count), encoded);

			if (encodedLength != punycode.Length)
				return false;

			for (int j = 0; j < encodedLength; j++) {
				char c = punycode[j];

				if (encoded[j] != c && encoded[j] != (char) (c | 0x20))
					return false;
			}

			for (int j = 0; j < count; j++) {
				int codePoint = label[j];

				if (codePoint >= 0x10000) {
					if (written + 2 > output.Length)
						return false;

					codePoint -= 0x10000;
					output[written++] = (char) (0xD800 + (codePoint >> 10));
					output[written++] = (char) (0xDC00 + (codePoint & 0x3FF));
				} else {
					if (written == output.Length)
						return false;

					output[written++] = (char) codePoint;
				}
			}

			return true;
		}

		#region Length Limits

		// RFC 5321, Section 4.5.3.1.1: The maximum total length of a user name or other local-part is 64 octets.
		const int MaxLocalPartLength = 64;

		// RFC 1035, Section 2.3.4 and RFC 5890, Section 2.3.2.1: Labels are limited to 63 octets (in A-label form).
		const int MaxLabelLength = 63;

		// RFC 5321, Section 4.5.3.1.2 limits a domain to 255 octets, but DNS (RFC 1035, Section 2.3.4) limits the
		// textual representation of a domain name (without the trailing dot) to 253 octets.
		const int MaxDomainLength = 253;

		// RFC 5321, Section 4.5.3.1.3: The maximum total length of a reverse-path or forward-path is 256 octets
		// (including the punctuation and element separators), which leaves 254 octets for the address once the
		// angle brackets are accounted for.
		const int MaxAddressLength = 254;

		// RFC 6531, Section 3.3: The length limits are in octets, not characters, so a UTF-8 local-part is limited
		// to 64 octets of UTF-8. Domain labels are measured in their A-label form (RFC 5890, Section 4.2).
		static SmtpUtf8AddressError ValidateLengths (string address, int domainIndex)
		{
			// Note: domainIndex - 1 is the index of the '@'.
			if (GetUtf8Length (address, 0, domainIndex - 1, MaxLocalPartLength) > MaxLocalPartLength)
				return SmtpUtf8AddressError.LocalPartTooLong;

			if (address[domainIndex] != '[') {
				int domainLength = 0;
				int index = domainIndex;

				do {
					int startIndex = index;

					while (index < address.Length && address[index] != '.')
						index++;

					int labelLength = GetALabelLength (address, startIndex, index);

					if (labelLength > MaxLabelLength)
						return SmtpUtf8AddressError.LabelTooLong;

					domainLength += labelLength;

					if (index < address.Length) {
						domainLength++;
						index++;
					}
				} while (index < address.Length);

				if (domainLength > MaxDomainLength)
					return SmtpUtf8AddressError.DomainTooLong;
			}

			if (GetUtf8Length (address, 0, address.Length, MaxAddressLength) > MaxAddressLength)
				return SmtpUtf8AddressError.AddressTooLong;

			return SmtpUtf8AddressError.None;
		}

		// Gets the number of octets needed to encode the text in UTF-8, stopping early once the limit has been exceeded.
		static int GetUtf8Length (string text, int startIndex, int endIndex, int limit)
		{
			int length = 0;

			for (int i = startIndex; i < endIndex && length <= limit; i++) {
				char c = text[i];

				if (c < 0x80) {
					length++;
				} else if (c < 0x800) {
					length += 2;
				} else if (char.IsHighSurrogate (c) && i + 1 < endIndex && char.IsLowSurrogate (text[i + 1])) {
					length += 4;
					i++;
				} else {
					length += 3;
				}
			}

			return length;
		}

		// Gets the length of a domain label in its A-label form (i.e. "xn--" followed by the Punycode encoding of the
		// label as defined in RFC 3492, Section 6.3) without actually encoding it. A label that is entirely ASCII
		// (including a label that is already an A-label) is its own A-label form.
		static int GetALabelLength (string text, int startIndex, int endIndex)
		{
			const int AcePrefixLength = 4;
			int codePoints = 0;
			int basic = 0;

			for (int i = startIndex; i < endIndex; i++) {
				if (text[i] < 0x80) {
					basic++;
				} else {
					if (char.IsHighSurrogate (text[i]) && i + 1 < endIndex && char.IsLowSurrogate (text[i + 1]))
						i++;

					codePoints++;
				}
			}

			if (codePoints == 0)
				return basic;

			// Every code point produces at least one octet of output, so a label with too many code points is
			// too long no matter how it encodes. This also bounds the quadratic encoding loop below.
			int total = basic + codePoints;

			if (AcePrefixLength + total + (basic > 0 ? 1 : 0) > MaxLabelLength)
				return AcePrefixLength + total + (basic > 0 ? 1 : 0);

			Span<int> label = stackalloc int[MaxLabelLength];
			int count = 0;

			for (int i = startIndex; i < endIndex; i++) {
				label[count++] = GetCodePoint (text.AsSpan (), i, out int length);
				i += length - 1;
			}

			return AcePrefixLength + Rfc3492.GetEncodedLength (label.Slice (0, count));
		}

		#endregion

		#region Syntax

		// RFC 5321, Section 4.1.2:
		//
		// Mailbox = Local-part "@" ( Domain / address-literal )
		//
		// RFC 6531 extends atext, qtextSMTP and sub-domain to allow UTF8-non-ascii.
		static bool ValidateSyntax (string address, out bool localPartALabel, out bool domainALabel, out int domainIndex)
		{
			int index = 0;

			localPartALabel = false;
			domainALabel = false;
			domainIndex = -1;

			if (address.Length > 0 && address[0] == '"') {
				if (!SkipQuotedString (address, ref index))
					return false;
			} else {
				if (!SkipDotString (address, ref index, ref localPartALabel))
					return false;
			}

			if (index >= address.Length || address[index] != '@')
				return false;

			domainIndex = ++index;

			if (index < address.Length && address[index] == '[') {
				if (!SkipAddressLiteral (address, ref index))
					return false;
			} else {
				if (!SkipDomain (address, ref index, ref domainALabel))
					return false;
			}

			return index == address.Length;
		}

		static bool IsAsciiLetter (char c)
		{
			return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
		}

		static bool IsAsciiLetterOrDigit (char c)
		{
			return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9');
		}

		static bool IsHexDigit (char c)
		{
			return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');
		}

		// RFC 5322: atext = ALPHA / DIGIT / "!" / "#" / "$" / "%" / "&" / "'" / "*" / "+" / "-" / "/" / "=" / "?" / "^" / "_" / "`" / "{" / "|" / "}" / "~"
		// RFC 6531: atext =/ UTF8-non-ascii
		static bool IsAtext (char c)
		{
			if (c >= 0x80 || IsAsciiLetterOrDigit (c))
				return true;

			switch (c) {
			case '!': case '#': case '$': case '%': case '&': case '\'': case '*': case '+': case '-':
			case '/': case '=': case '?': case '^': case '_': case '`': case '{': case '|': case '}': case '~':
				return true;
			default:
				return false;
			}
		}

		// RFC 5890, Section 2.3.1: an A-label begins with the (case-insensitive) ACE prefix "xn--".
		static bool HasAcePrefix (string text, int startIndex, int endIndex)
		{
			return endIndex - startIndex >= 4 &&
				(text[startIndex] == 'x' || text[startIndex] == 'X') &&
				(text[startIndex + 1] == 'n' || text[startIndex + 1] == 'N') &&
				text[startIndex + 2] == '-' && text[startIndex + 3] == '-';
		}

		// Dot-string = Atom *("."  Atom)
		// Atom       = 1*atext
		static bool SkipDotString (string text, ref int index, ref bool alabel)
		{
			do {
				int startIndex = index;

				while (index < text.Length && IsAtext (text[index]))
					index++;

				if (index == startIndex)
					return false;

				if (HasAcePrefix (text, startIndex, index))
					alabel = true;

				if (index >= text.Length || text[index] != '.')
					return true;

				index++;
			} while (true);
		}

		// Quoted-string   = DQUOTE *QcontentSMTP DQUOTE
		// QcontentSMTP    = qtextSMTP / quoted-pairSMTP
		// quoted-pairSMTP = %d92 %d32-126
		// qtextSMTP       = %d32-33 / %d35-91 / %d93-126 / UTF8-non-ascii
		static bool SkipQuotedString (string text, ref int index)
		{
			// skip over the opening DQUOTE
			index++;

			while (index < text.Length) {
				char c = text[index++];

				if (c == '"')
					return true;

				if (c == '\\') {
					if (index >= text.Length || text[index] < 32 || text[index] > 126)
						return false;

					index++;
				} else if (c < 32 || c == 127) {
					return false;
				}
			}

			return false;
		}

		// Domain     = sub-domain *("." sub-domain)
		// sub-domain = Let-dig [Ldh-str]
		// Let-dig    = ALPHA / DIGIT
		// Ldh-str    = *( ALPHA / DIGIT / "-" ) Let-dig
		//
		// RFC 6531: sub-domain =/ U-label
		static bool SkipDomain (string text, ref int index, ref bool alabel)
		{
			do {
				int startIndex = index;

				while (index < text.Length && (text[index] >= 0x80 || text[index] == '-' || IsAsciiLetterOrDigit (text[index])))
					index++;

				if (index == startIndex || text[startIndex] == '-' || text[index - 1] == '-')
					return false;

				if (HasAcePrefix (text, startIndex, index))
					alabel = true;

				if (index >= text.Length || text[index] != '.')
					return true;

				index++;
			} while (true);
		}

		// address-literal  = "[" ( IPv4-address-literal / IPv6-address-literal / General-address-literal ) "]"
		static bool SkipAddressLiteral (string text, ref int index)
		{
			int startIndex = index + 1;
			int endIndex = text.IndexOf (']', startIndex);

			if (endIndex == -1)
				return false;

			index = endIndex + 1;

			if (endIndex - startIndex >= 5 && string.Compare (text, startIndex, "IPv6:", 0, 5, StringComparison.OrdinalIgnoreCase) == 0)
				return IsIPv6Address (text, startIndex + 5, endIndex);

			int colon = text.IndexOf (':', startIndex, endIndex - startIndex);

			if (colon != -1)
				return IsGeneralAddressLiteral (text, startIndex, colon, endIndex);

			return IsIPv4Address (text, startIndex, endIndex);
		}

		// IPv4-address-literal = Snum 3("."  Snum)
		// Snum                 = 1*3DIGIT ; representing a decimal integer value in the range 0 through 255
		static bool IsIPv4Address (string text, int startIndex, int endIndex)
		{
			int index = startIndex;

			for (int i = 0; i < 4; i++) {
				if (i > 0) {
					if (index >= endIndex || text[index] != '.')
						return false;

					index++;
				}

				int numberIndex = index;
				int value = 0;

				while (index < endIndex && index - numberIndex < 3 && text[index] >= '0' && text[index] <= '9')
					value = (value * 10) + (text[index++] - '0');

				if (index == numberIndex || value > 255)
					return false;
			}

			return index == endIndex;
		}

		// IPv6-addr   = IPv6-full / IPv6-comp / IPv6v4-full / IPv6v4-comp
		// IPv6-hex    = 1*4HEXDIG
		// IPv6-full   = IPv6-hex 7(":" IPv6-hex)
		// IPv6-comp   = [IPv6-hex *5(":" IPv6-hex)] "::" [IPv6-hex *5(":" IPv6-hex)]
		//             ; The "::" represents at least 2 16-bit groups of zeros. No more than 6 groups in addition to the "::" may be present.
		// IPv6v4-full = IPv6-hex 5(":" IPv6-hex) ":" IPv4-address-literal
		// IPv6v4-comp = [IPv6-hex *3(":" IPv6-hex)] "::" [IPv6-hex *3(":" IPv6-hex) ":"] IPv4-address-literal
		//             ; The "::" represents at least 2 16-bit groups of zeros. No more than 4 groups in addition to the "::" and IPv4-address-literal may be present.
		static bool IsIPv6Address (string text, int startIndex, int endIndex)
		{
			int compressed = text.IndexOf ("::", startIndex, endIndex - startIndex, StringComparison.Ordinal);
			bool ipv4 = false;
			int groups = 0;

			if (compressed == -1) {
				if (!TryParseIPv6Groups (text, startIndex, endIndex, true, ref groups, ref ipv4))
					return false;

				return groups == (ipv4 ? 6 : 8);
			}

			// only a single "::" is allowed
			if (compressed + 2 < endIndex && text.IndexOf (':', compressed + 2, endIndex - (compressed + 2)) == compressed + 2)
				return false;

			if (compressed > startIndex && !TryParseIPv6Groups (text, startIndex, compressed, false, ref groups, ref ipv4))
				return false;

			if (compressed + 2 < endIndex && !TryParseIPv6Groups (text, compressed + 2, endIndex, true, ref groups, ref ipv4))
				return false;

			return groups <= (ipv4 ? 4 : 6);
		}

		static bool TryParseIPv6Groups (string text, int startIndex, int endIndex, bool allowIPv4, ref int groups, ref bool ipv4)
		{
			int index = startIndex;

			do {
				int groupIndex = index;

				while (index < endIndex && index - groupIndex < 4 && IsHexDigit (text[index]))
					index++;

				if (allowIPv4 && index < endIndex && text[index] == '.') {
					// the IPv4-address-literal must be the last component
					ipv4 = true;

					return IsIPv4Address (text, groupIndex, endIndex);
				}

				if (index == groupIndex)
					return false;

				groups++;

				if (index == endIndex)
					return true;

				if (text[index] != ':')
					return false;

				index++;
			} while (index < endIndex);

			// trailing ':'
			return false;
		}

		// General-address-literal = Standardized-tag ":" 1*dcontent
		// Standardized-tag        = Ldh-str
		// dcontent                = %d33-90 / %d94-126
		static bool IsGeneralAddressLiteral (string text, int startIndex, int colon, int endIndex)
		{
			if (colon == startIndex || text[colon - 1] == '-')
				return false;

			for (int i = startIndex; i < colon; i++) {
				if (text[i] != '-' && !IsAsciiLetterOrDigit (text[i]))
					return false;
			}

			if (colon + 1 == endIndex)
				return false;

			for (int i = colon + 1; i < endIndex; i++) {
				char c = text[i];

				if (c < 33 || c > 126 || c == '[' || c == '\\')
					return false;
			}

			return true;
		}

		#endregion Syntax

		#region Code Points

		static int GetCodePoint (ReadOnlySpan<char> text, int index, out int length)
		{
			char c = text[index];

			if (char.IsHighSurrogate (c) && index + 1 < text.Length && char.IsLowSurrogate (text[index + 1])) {
				length = 2;
				return char.ConvertToUtf32 (c, text[index + 1]);
			}

			// Note: a lone surrogate is returned as-is; the generated tables classify it as Disallowed.
			length = 1;

			return c;
		}

		static int GetPreviousCodePoint (ReadOnlySpan<char> text, int index, out int length)
		{
			char c = text[index - 1];

			if (char.IsLowSurrogate (c) && index - 2 >= 0 && char.IsHighSurrogate (text[index - 2])) {
				length = 2;
				return char.ConvertToUtf32 (text[index - 2], c);
			}

			length = 1;

			return c;
		}

		static uint LookupEntry (ReadOnlySpan<byte> table, int count, int shift, int codePoint)
		{
			// Find the last entry whose first code point is <= codePoint. The first entry always starts at U+0000.
			int low = 0, high = count - 1;

			while (low < high) {
				int mid = (low + high + 1) >> 1;
				uint entry = BinaryPrimitives.ReadUInt32LittleEndian (table.Slice (mid * 4, 4));

				if ((int) (entry >> shift) <= codePoint)
					low = mid;
				else
					high = mid - 1;
			}

			return BinaryPrimitives.ReadUInt32LittleEndian (table.Slice (low * 4, 4));
		}

		static PrecisClass GetPrecisClass (int codePoint, out UnicodeScript script)
		{
			uint entry = LookupEntry (CodePointData, CodePointDataCount, 11, codePoint);

			script = (UnicodeScript) ((entry >> 2) & 0x1FF);

			return (PrecisClass) (entry & 0x3);
		}

		static UnicodeScript GetScript (int codePoint)
		{
			GetPrecisClass (codePoint, out var script);

			return script;
		}

		static JoiningType GetJoiningType (int codePoint)
		{
			uint entry = LookupEntry (JoiningData, JoiningDataCount, 4, codePoint);

			return (JoiningType) (entry & 0x7);
		}

		static bool IsVirama (int codePoint)
		{
			uint entry = LookupEntry (JoiningData, JoiningDataCount, 4, codePoint);

			return (entry & 0x8) != 0;
		}

		static SmtpUtf8AddressError ValidateCodePoints (ReadOnlySpan<char> address, int domainIndex, SmtpUtf8AddressValidationOptions options)
		{
			bool asciiIsLatin = (options & SmtpUtf8AddressValidationOptions.AsciiIsLatin) != 0;
			var segmentScript = UnicodeScript.Unknown;
			var firstScript = UnicodeScript.Unknown;
			bool hasHiraganaKatakanaOrHan = false;
			bool hasExtendedArabicIndicDigits = false;
			bool hasArabicIndicDigits = false;
			bool hasContextRules = false;
			bool checkNormalization = false;
			bool mixedScripts = false;
			int index = 0;

			while (index < address.Length) {
				int codePoint = GetCodePoint (address, index, out int length);
				UnicodeScript script;

				// When ASCII letters are treated as Latin, they are counted separately for the local-part and
				// for each label of the domain so that (for example) a Cyrillic local-part or a Cyrillic label
				// under an ASCII top-level domain is not considered to be mixing scripts. The non-ASCII code
				// points must still all belong to a single script across the whole address so that this option
				// only ever rejects more addresses.
				if (asciiIsLatin && index >= domainIndex && (index == domainIndex || address[index] == '.'))
					segmentScript = UnicodeScript.Unknown;

				index += length;

				if (codePoint < 0x80) {
					// Note: The syntax rules only allow printable ASCII, all of which is PVALID except for SPACE,
					// which is explicitly permitted by draft-ietf-mailmaint-smtputf8-syntax. ASCII is also ignored
					// when counting scripts unless ASCII letters are to be treated as Latin.
					if (!asciiIsLatin || !IsAsciiLetter ((char) codePoint))
						continue;

					script = UnicodeScript.Latin;
				} else {
					var precis = GetPrecisClass (codePoint, out script);

					if (precis == PrecisClass.Disallowed)
						return SmtpUtf8AddressError.DisallowedCodePoint;

					if (precis != PrecisClass.PValid)
						hasContextRules = true;

					if (codePoint >= UnicodeNormalization.FirstNormalizationCodePoint)
						checkNormalization = true;

					if (codePoint >= 0x0660 && codePoint <= 0x0669)
						hasArabicIndicDigits = true;
					else if (codePoint >= 0x06F0 && codePoint <= 0x06F9)
						hasExtendedArabicIndicDigits = true;
				}

				switch (script) {
				case UnicodeScript.Common:
				case UnicodeScript.Inherited:
					break;
				default:
					if (script == UnicodeScript.Hiragana || script == UnicodeScript.Katakana || script == UnicodeScript.Han)
						hasHiraganaKatakanaOrHan = true;

					if (codePoint >= 0x80) {
						if (firstScript == UnicodeScript.Unknown)
							firstScript = script;
						else if (script != firstScript)
							mixedScripts = true;
					}

					if (asciiIsLatin) {
						if (segmentScript == UnicodeScript.Unknown)
							segmentScript = script;
						else if (script != segmentScript)
							mixedScripts = true;
					}
					break;
				}
			}

			// Note: Every code point that is NFC_Quick_Check=No also has a compatibility decomposition and so has already
			// been rejected by the PRECIS IdentifierClass, but sequences of code points may still not be normalized.
			if (checkNormalization && !UnicodeNormalization.IsNormalizationFormC (address))
				return SmtpUtf8AddressError.NotNormalized;

			if (hasContextRules) {
				// RFC 5892, Appendix A.8 and A.9: Arabic-Indic digits and Extended Arabic-Indic digits must not be mixed.
				if (hasArabicIndicDigits && hasExtendedArabicIndicDigits)
					return SmtpUtf8AddressError.ContextRule;

				index = 0;

				while (index < address.Length) {
					int codePoint = GetCodePoint (address, index, out int length);

					if (IsContextual (codePoint) && !IsContextSatisfied (address, index, length, codePoint, hasHiraganaKatakanaOrHan))
						return SmtpUtf8AddressError.ContextRule;

					index += length;
				}
			}

			return mixedScripts ? SmtpUtf8AddressError.MixedScripts : SmtpUtf8AddressError.None;
		}

		// Note: These are all of the CONTEXTJ and CONTEXTO code points except for the Arabic-Indic digits
		// (RFC 5892, Appendix A.8 and A.9), which are handled separately.
		static bool IsContextual (int codePoint)
		{
			switch (codePoint) {
			case ZeroWidthNonJoiner: case ZeroWidthJoiner:
			case 0x00B7: case 0x0375: case 0x05F3: case 0x05F4: case 0x30FB:
				return true;
			default:
				return false;
			}
		}

		// RFC 5892, Appendix A: the contextual rules for CONTEXTJ and CONTEXTO code points.
		static bool IsContextSatisfied (ReadOnlySpan<char> text, int index, int length, int codePoint, bool hasHiraganaKatakanaOrHan)
		{
			int before = index > 0 ? GetPreviousCodePoint (text, index, out _) : -1;
			int afterIndex = index + length;
			int after = afterIndex < text.Length ? GetCodePoint (text, afterIndex, out _) : -1;

			switch (codePoint) {
			case ZeroWidthNonJoiner:
				// Appendix A.1: If Canonical_Combining_Class(Before(cp)) .eq. Virama Then True;
				if (before != -1 && IsVirama (before))
					return true;

				// If RegExpMatch((Joining_Type:{L,D})(Joining_Type:T)*\u200C(Joining_Type:T)*(Joining_Type:{R,D})) Then True;
				return IsJoiningContextSatisfied (text, index, afterIndex);
			case ZeroWidthJoiner:
				// Appendix A.2: If Canonical_Combining_Class(Before(cp)) .eq. Virama Then True;
				return before != -1 && IsVirama (before);
			case 0x00B7:
				// Appendix A.3: If Before(cp) .eq. U+006C And After(cp) .eq. U+006C Then True;
				// Note: Unlike DNS, email addresses are not case-folded, so U+004C is accepted on either side as
				// well (e.g. "CEL·LA" or "Cel·La"), as recommended by the author of draft-ietf-mailmaint-smtputf8-syntax.
				return (before == 0x006C || before == 0x004C) && (after == 0x006C || after == 0x004C);
			case 0x0375:
				// Appendix A.4: If Script(After(cp)) .eq. Greek Then True;
				return after != -1 && GetScript (after) == UnicodeScript.Greek;
			case 0x05F3:
			case 0x05F4:
				// Appendix A.5 and A.6: If Script(Before(cp)) .eq. Hebrew Then True;
				return before != -1 && GetScript (before) == UnicodeScript.Hebrew;
			default:
				// RFC 5892, Appendix A.7: Script(cp) .in. {Hiragana, Katakana, Han} for some code point in the address.
				return hasHiraganaKatakanaOrHan;
			}
		}

		static bool IsJoiningContextSatisfied (ReadOnlySpan<char> text, int index, int afterIndex)
		{
			JoiningType type;
			bool matched = false;

			// (Joining_Type:{L,D})(Joining_Type:T)*
			while (index > 0) {
				int codePoint = GetPreviousCodePoint (text, index, out int length);

				type = GetJoiningType (codePoint);
				index -= length;

				if (type != JoiningType.Transparent) {
					matched = type == JoiningType.Left || type == JoiningType.Dual;
					break;
				}
			}

			if (!matched)
				return false;

			// (Joining_Type:T)*(Joining_Type:{R,D})
			index = afterIndex;

			while (index < text.Length) {
				int codePoint = GetCodePoint (text, index, out int length);

				type = GetJoiningType (codePoint);
				index += length;

				if (type != JoiningType.Transparent)
					return type == JoiningType.Right || type == JoiningType.Dual;
			}

			return false;
		}

		#endregion Code Points
	}
}

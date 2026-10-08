//
// DnsDomainName.cs
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
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace MimeKit.Cryptography {
	/// <summary>
	/// Helpers for validating and normalizing DNS domain names taken from untrusted input (e.g. the d= and s=
	/// tags of a DKIM-Signature) before they are handed to an <see cref="IDnsResolver"/>.
	/// </summary>
	static class DnsDomainName
	{
		internal const int MaxLength = 253;
		internal const int MaxLabelLength = 63;

		static bool IsLabelChar (char c)
		{
			// Note: underscores are not valid in host names, but they are used extensively in DNS names
			// for service records such as "_domainkey" and "_dmarc" and are found in DKIM selectors.
			return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-' || c == '_';
		}

		static bool IsAscii (string value)
		{
			for (int i = 0; i < value.Length; i++) {
				if (value[i] > 127)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Try to normalize a domain name.
		/// </summary>
		/// <remarks>
		/// Converts any U-labels into A-labels, lowercases the result and validates the label syntax and lengths.
		/// A single trailing dot is permitted and removed.
		/// </remarks>
		/// <returns><see langword="true" /> if the domain name is valid; otherwise, <see langword="false" />.</returns>
		/// <param name="domain">The domain name.</param>
		/// <param name="normalized">The normalized domain name.</param>
		public static bool TryNormalize (string? domain, [NotNullWhen (true)] out string? normalized)
		{
			normalized = null;

			if (string.IsNullOrEmpty (domain))
				return false;

			if (domain![domain.Length - 1] == '.')
				domain = domain.Substring (0, domain.Length - 1);

			if (domain.Length == 0)
				return false;

			if (!IsAscii (domain)) {
				try {
					domain = new IdnMapping ().GetAscii (domain);
				} catch (ArgumentException) {
					return false;
				}
			}

			if (domain.Length > MaxLength)
				return false;

			int labelLength = 0;

			for (int i = 0; i < domain.Length; i++) {
				char c = domain[i];

				if (c == '.') {
					if (labelLength == 0)
						return false;

					labelLength = 0;
				} else if (IsLabelChar (c)) {
					if (++labelLength > MaxLabelLength)
						return false;
				} else {
					return false;
				}
			}

			if (labelLength == 0)
				return false;

			normalized = domain.ToLowerInvariant ();

			return true;
		}

		/// <summary>
		/// Try to combine a relative name with a domain name.
		/// </summary>
		/// <remarks>
		/// Combines a relative name (e.g. <c>"selector._domainkey"</c>) with a normalized domain name and validates
		/// that the result does not exceed the maximum length of a domain name.
		/// </remarks>
		/// <returns><see langword="true" /> if the combined name is valid; otherwise, <see langword="false" />.</returns>
		/// <param name="prefix">The normalized prefix.</param>
		/// <param name="domain">The normalized domain name.</param>
		/// <param name="name">The combined domain name.</param>
		public static bool TryCombine (string prefix, string domain, [NotNullWhen (true)] out string? name)
		{
			if (prefix.Length + 1 + domain.Length > MaxLength) {
				name = null;
				return false;
			}

			name = prefix + "." + domain;

			return true;
		}
	}
}

//
// DkimVerifier.cs
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
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A DKIM-Signature verifier.
	/// </summary>
	/// <remarks>
	/// Verifies DomainKeys Identified Mail (DKIM) signatures.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
	/// </example>
	public class DkimVerifier : DkimVerifierBase
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="DkimVerifier"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="DkimVerifier"/>.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
		/// </example>
		/// <param name="resolver">The DNS resolver used to look up the public keys.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="resolver"/> is <see langword="null"/>.
		/// </exception>
		public DkimVerifier (IDnsResolver resolver) : base (resolver)
		{
		}

		static void ValidateDkimSignatureParameters (Dictionary<string, string> parameters, out DkimSignatureAlgorithm algorithm, out DkimCanonicalizationAlgorithm headerAlgorithm,
			out DkimCanonicalizationAlgorithm bodyAlgorithm, out string d, out string s, out string q, out string[] headers, out string bh, out string b, out int maxLength)
		{
			bool containsFrom = false;

			if (!parameters.TryGetValue ("v", out string? v))
				throw new FormatException ("Malformed DKIM-Signature header: no version parameter detected.");

			if (v != "1")
				throw new FormatException (string.Format ("Unrecognized DKIM-Signature version: v={0}", v));

			ValidateCommonSignatureParameters ("DKIM-Signature", parameters, out algorithm, out headerAlgorithm, out bodyAlgorithm, out d, out s, out q, out headers, out bh, out b, out maxLength);

			for (int i = 0; i < headers.Length; i++) {
				if (headers[i].Equals ("from", StringComparison.OrdinalIgnoreCase)) {
					containsFrom = true;
					break;
				}
			}

			if (!containsFrom)
				throw new FormatException ("Malformed DKIM-Signature header: From header not signed.");

			if (parameters.TryGetValue ("i", out string? id)) {
				int at;

				if ((at = id.LastIndexOf ('@')) == -1)
					throw new FormatException ("Malformed DKIM-Signature header: no @ in the AUID value.");

				var ident = id.AsSpan (at + 1);

				if (!ident.Equals (d.AsSpan (), StringComparison.OrdinalIgnoreCase) && !ident.EndsWith (("." + d).AsSpan (), StringComparison.OrdinalIgnoreCase))
					throw new FormatException ("Invalid DKIM-Signature header: the domain in the AUID does not match the domain parameter.");
			}
		}

		static bool IsSameDomain (string auid, string domain)
		{
			var auidDomain = auid.AsSpan (auid.LastIndexOf ('@') + 1);

			return auidDomain.Equals (domain.AsSpan (), StringComparison.OrdinalIgnoreCase);
		}

		DkimSignatureInfo? PrepareVerification (FormatOptions options, MimeMessage message, Header dkimSignature)
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			if (message == null)
				throw new ArgumentNullException (nameof (message));

			if (dkimSignature == null)
				throw new ArgumentNullException (nameof (dkimSignature));

			if (dkimSignature.Id != HeaderId.DkimSignature)
				throw new ArgumentException ("The signature parameter MUST be a DKIM-Signature header.", nameof (dkimSignature));

			var parameters = ParseParameterTags (dkimSignature.Id, dkimSignature.Value);
			DkimCanonicalizationAlgorithm headerAlgorithm, bodyAlgorithm;
			DkimSignatureAlgorithm signatureAlgorithm;
			string d, s, q, bh, b;
			string[] headers;
			int maxLength;

			ValidateDkimSignatureParameters (parameters, out signatureAlgorithm, out headerAlgorithm, out bodyAlgorithm,
				out d, out s, out q, out headers, out bh, out b, out maxLength);

			if (!IsEnabled (signatureAlgorithm))
				return null;

			options = options.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;

			// first check the body hash (if that's invalid, then the entire signature is invalid)
			if (!VerifyBodyHash (options, message, signatureAlgorithm, bodyAlgorithm, maxLength, bh))
				return null;

			parameters.TryGetValue ("i", out var auid);

			return new DkimSignatureInfo (options, dkimSignature, signatureAlgorithm, d, s, q, b) {
				HeaderAlgorithm = headerAlgorithm,
				AgentOrUserIdentifier = auid,
				Headers = headers
			};
		}

		bool CompleteVerification (DkimSignatureInfo info, MimeMessage message, DkimPublicKeyLookupResult lookup)
		{
			if (!TryGetVerificationKey (lookup, out var key))
				return false;

			// If the key record specifies the "s" flag, the domain in the i= tag MUST NOT be a subdomain of d=.
			// (RFC 6376, Section 3.6.1)
			if (lookup.Record!.IsStrict && info.AgentOrUserIdentifier != null && !IsSameDomain (info.AgentOrUserIdentifier, info.Domain))
				return false;

			return VerifySignature (info.Options, message, info.Header, info.SignatureAlgorithm, key, info.Headers!, info.HeaderAlgorithm, info.Signature);
		}

		/// <summary>
		/// Verify the specified DKIM-Signature header.
		/// </summary>
		/// <remarks>
		/// Verifies the specified DKIM-Signature header.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
		/// </example>
		/// <returns><see langword="true" /> if the DKIM-Signature is valid; otherwise, <see langword="false" />.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="message">The message to verify.</param>
		/// <param name="dkimSignature">The DKIM-Signature header.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="message"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="dkimSignature"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="dkimSignature"/> is not a DKIM-Signature header.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The DKIM-Signature header value is malformed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public bool Verify (FormatOptions options, MimeMessage message, Header dkimSignature, CancellationToken cancellationToken = default)
		{
			var info = PrepareVerification (options, message, dkimSignature);

			if (info == null)
				return false;

			var lookup = LookupPublicKey (info, cancellationToken);

			return CompleteVerification (info, message, lookup);
		}

		/// <summary>
		/// Asynchronously verify the specified DKIM-Signature header.
		/// </summary>
		/// <remarks>
		/// Verifies the specified DKIM-Signature header.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
		/// </example>
		/// <returns><see langword="true" /> if the DKIM-Signature is valid; otherwise, <see langword="false" />.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="message">The message to verify.</param>
		/// <param name="dkimSignature">The DKIM-Signature header.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="message"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="dkimSignature"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="dkimSignature"/> is not a DKIM-Signature header.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The DKIM-Signature header value is malformed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public async Task<bool> VerifyAsync (FormatOptions options, MimeMessage message, Header dkimSignature, CancellationToken cancellationToken = default)
		{
			var info = PrepareVerification (options, message, dkimSignature);

			if (info == null)
				return false;

			var lookup = await LookupPublicKeyAsync (info, cancellationToken).ConfigureAwait (false);

			return CompleteVerification (info, message, lookup);
		}

		/// <summary>
		/// Verify the specified DKIM-Signature header.
		/// </summary>
		/// <remarks>
		/// Verifies the specified DKIM-Signature header.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
		/// </example>
		/// <returns><see langword="true" /> if the DKIM-Signature is valid; otherwise, <see langword="false" />.</returns>
		/// <param name="message">The message to verify.</param>
		/// <param name="dkimSignature">The DKIM-Signature header.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="message"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="dkimSignature"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="dkimSignature"/> is not a DKIM-Signature header.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The DKIM-Signature header value is malformed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public bool Verify (MimeMessage message, Header dkimSignature, CancellationToken cancellationToken = default)
		{
			return Verify (FormatOptions.Default, message, dkimSignature, cancellationToken);
		}

		/// <summary>
		/// Asynchronously verify the specified DKIM-Signature header.
		/// </summary>
		/// <remarks>
		/// Verifies the specified DKIM-Signature header.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
		/// </example>
		/// <returns><see langword="true" /> if the DKIM-Signature is valid; otherwise, <see langword="false" />.</returns>
		/// <param name="message">The message to verify.</param>
		/// <param name="dkimSignature">The DKIM-Signature header.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="message"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="dkimSignature"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="dkimSignature"/> is not a DKIM-Signature header.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The DKIM-Signature header value is malformed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public Task<bool> VerifyAsync (MimeMessage message, Header dkimSignature, CancellationToken cancellationToken = default)
		{
			return VerifyAsync (FormatOptions.Default, message, dkimSignature, cancellationToken);
		}
	}
}

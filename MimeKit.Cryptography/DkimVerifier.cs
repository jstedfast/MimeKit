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
using System.Globalization;
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
		const int DefaultMaxSignatures = 10;

		int maxSignatures = DefaultMaxSignatures;

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

		/// <summary>
		/// Get or set the maximum number of DKIM-Signature headers to verify per message.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the maximum number of DKIM-Signature headers that
		/// <see cref="Verify(FormatOptions, MimeMessage, CancellationToken)"/> will verify.</para>
		/// <para>Each signature may require a DNS query and hashing the message body, so this limit protects
		/// against messages crafted with a large number of signatures. DKIM-Signature headers beyond the limit are
		/// ignored (<a href="https://www.rfc-editor.org/rfc/rfc6376#section-8.4">RFC 6376, Section 8.4</a>).</para>
		/// <para>The default value is <c>10</c>.</para>
		/// </remarks>
		/// <value>The maximum number of signatures to verify.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is less than <c>1</c>.
		/// </exception>
		public int MaxSignatures {
			get { return maxSignatures; }
			set {
				if (value < 1)
					throw new ArgumentOutOfRangeException (nameof (value));

				maxSignatures = value;
			}
		}

		static void ValidateDkimSignatureParameters (Dictionary<string, string> parameters, out DkimSignatureAlgorithm algorithm, out DkimCanonicalizationAlgorithm headerAlgorithm,
			out DkimCanonicalizationAlgorithm bodyAlgorithm, out string d, out string s, out string q, out string[] headers, out string bh, out string b, out int maxLength, out long? expiration)
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

			if (parameters.TryGetValue ("x", out string? x)) {
				if (!long.TryParse (x, NumberStyles.None, CultureInfo.InvariantCulture, out long value))
					throw new FormatException (string.Format ("Malformed DKIM-Signature header: invalid expiration parameter: x={0}.", x));

				// The value of the "x=" tag MUST be greater than the value of the "t=" tag if both are present.
				// (RFC 6376, Section 3.5)
				if (parameters.TryGetValue ("t", out string? t) && long.TryParse (t, NumberStyles.None, CultureInfo.InvariantCulture, out long timestamp) && value < timestamp)
					throw new FormatException ("Invalid DKIM-Signature header: the expiration parameter precedes the timestamp parameter.");

				expiration = value;
			} else {
				expiration = null;
			}
		}

		static bool IsSameDomain (string auid, string domain)
		{
			var auidDomain = auid.AsSpan (auid.LastIndexOf ('@') + 1);

			return auidDomain.Equals (domain.AsSpan (), StringComparison.OrdinalIgnoreCase);
		}

		static void ValidateArguments (FormatOptions options, MimeMessage message, Header dkimSignature)
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			if (message == null)
				throw new ArgumentNullException (nameof (message));

			if (dkimSignature == null)
				throw new ArgumentNullException (nameof (dkimSignature));

			if (dkimSignature.Id != HeaderId.DkimSignature)
				throw new ArgumentException ("The signature parameter MUST be a DKIM-Signature header.", nameof (dkimSignature));
		}

		static void SetResult (DkimSignatureValidationResult result, DkimSignatureStatus status, string? reason, Exception? exception = null)
		{
			result.Exception = exception;
			result.Status = status;
			result.Reason = reason;
		}

		DkimSignatureInfo? PrepareVerification (FormatOptions options, MimeMessage message, DkimSignatureValidationResult result)
		{
			var dkimSignature = result.Header!;

			try {
				var parameters = ParseParameterTags (dkimSignature.Id, dkimSignature.Value);
				DkimCanonicalizationAlgorithm headerAlgorithm, bodyAlgorithm;
				DkimSignatureAlgorithm signatureAlgorithm;
				string d, s, q, bh, b;
				string[] headers;
				long? expiration;
				int maxLength;

				// Populate whatever we can before validating so that callers can still identify the signer
				// of a malformed signature.
				parameters.TryGetValue ("d", out var domain);
				parameters.TryGetValue ("s", out var selector);
				parameters.TryGetValue ("i", out var auid);
				parameters.TryGetValue ("b", out var signature);

				result.AgentOrUserIdentifier = auid;
				result.Signature = signature;
				result.Selector = selector;
				result.Domain = domain;

				ValidateDkimSignatureParameters (parameters, out signatureAlgorithm, out headerAlgorithm, out bodyAlgorithm,
					out d, out s, out q, out headers, out bh, out b, out maxLength, out expiration);

				result.SignatureAlgorithm = signatureAlgorithm;

				if (!IsEnabled (signatureAlgorithm)) {
					SetResult (result, DkimSignatureStatus.Policy, "signature algorithm disabled");
					return null;
				}

				if (expiration.HasValue && expiration.Value < DateTimeOffset.UtcNow.ToUnixTimeSeconds ()) {
					SetResult (result, DkimSignatureStatus.PermError, "signature expired");
					return null;
				}

				options = options.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				// first check the body hash (if that's invalid, then the entire signature is invalid)
				if (!VerifyBodyHash (options, message, signatureAlgorithm, bodyAlgorithm, maxLength, bh)) {
					SetResult (result, DkimSignatureStatus.Fail, "body hash did not verify");
					return null;
				}

				return new DkimSignatureInfo (options, dkimSignature, signatureAlgorithm, d, s, q, b) {
					HeaderAlgorithm = headerAlgorithm,
					AgentOrUserIdentifier = auid,
					Headers = headers
				};
			} catch (OperationCanceledException) {
				throw;
			} catch (Exception ex) {
				SetResult (result, DkimSignatureStatus.PermError, ex.Message, ex);
				return null;
			}
		}

		void CompleteVerification (DkimSignatureInfo info, MimeMessage message, DkimPublicKeyLookupResult lookup, DkimSignatureValidationResult result)
		{
			var status = GetVerificationKey (lookup, out var key, out var reason);

			if (status != DkimSignatureStatus.Pass) {
				SetResult (result, status, reason, lookup.Exception);
				return;
			}

			// If the key record specifies the "s" flag, the domain in the i= tag MUST NOT be a subdomain of d=.
			// (RFC 6376, Section 3.6.1)
			if (lookup.Record!.IsStrict && info.AgentOrUserIdentifier != null && !IsSameDomain (info.AgentOrUserIdentifier, info.Domain)) {
				SetResult (result, DkimSignatureStatus.PermError, "identity domain does not match signing domain");
				return;
			}

			try {
				if (VerifySignature (info.Options, message, info.Header, info.SignatureAlgorithm, key!, info.Headers!, info.HeaderAlgorithm, info.Signature))
					SetResult (result, DkimSignatureStatus.Pass, null);
				else
					SetResult (result, DkimSignatureStatus.Fail, "signature did not verify");
			} catch (OperationCanceledException) {
				throw;
			} catch (Exception ex) {
				SetResult (result, DkimSignatureStatus.PermError, ex.Message, ex);
			}
		}

		/// <summary>
		/// Verify the specified DKIM-Signature header.
		/// </summary>
		/// <remarks>
		/// <para>Verifies the specified DKIM-Signature header.</para>
		/// <para>Malformed signatures, DNS lookup failures and invalid public keys do not throw exceptions; they are
		/// reported via the <see cref="DkimSignatureValidationResult.Status"/> of the returned result.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
		/// </example>
		/// <returns>The result of verifying the DKIM-Signature.</returns>
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
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public DkimSignatureValidationResult Verify (FormatOptions options, MimeMessage message, Header dkimSignature, CancellationToken cancellationToken = default)
		{
			ValidateArguments (options, message, dkimSignature);

			var result = new DkimSignatureValidationResult (dkimSignature);
			var info = PrepareVerification (options, message, result);

			if (info != null) {
				var lookup = LookupPublicKey (info, cancellationToken);

				CompleteVerification (info, message, lookup, result);
			}

			return result;
		}

		/// <summary>
		/// Asynchronously verify the specified DKIM-Signature header.
		/// </summary>
		/// <remarks>
		/// <para>Verifies the specified DKIM-Signature header.</para>
		/// <para>Malformed signatures, DNS lookup failures and invalid public keys do not throw exceptions; they are
		/// reported via the <see cref="DkimSignatureValidationResult.Status"/> of the returned result.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
		/// </example>
		/// <returns>The result of verifying the DKIM-Signature.</returns>
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
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public async Task<DkimSignatureValidationResult> VerifyAsync (FormatOptions options, MimeMessage message, Header dkimSignature, CancellationToken cancellationToken = default)
		{
			ValidateArguments (options, message, dkimSignature);

			var result = new DkimSignatureValidationResult (dkimSignature);
			var info = PrepareVerification (options, message, result);

			if (info != null) {
				var lookup = await LookupPublicKeyAsync (info, cancellationToken).ConfigureAwait (false);

				CompleteVerification (info, message, lookup, result);
			}

			return result;
		}

		/// <summary>
		/// Verify the specified DKIM-Signature header.
		/// </summary>
		/// <remarks>
		/// <para>Verifies the specified DKIM-Signature header.</para>
		/// <para>Malformed signatures, DNS lookup failures and invalid public keys do not throw exceptions; they are
		/// reported via the <see cref="DkimSignatureValidationResult.Status"/> of the returned result.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
		/// </example>
		/// <returns>The result of verifying the DKIM-Signature.</returns>
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
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public DkimSignatureValidationResult Verify (MimeMessage message, Header dkimSignature, CancellationToken cancellationToken = default)
		{
			return Verify (FormatOptions.Default, message, dkimSignature, cancellationToken);
		}

		/// <summary>
		/// Asynchronously verify the specified DKIM-Signature header.
		/// </summary>
		/// <remarks>
		/// <para>Verifies the specified DKIM-Signature header.</para>
		/// <para>Malformed signatures, DNS lookup failures and invalid public keys do not throw exceptions; they are
		/// reported via the <see cref="DkimSignatureValidationResult.Status"/> of the returned result.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
		/// </example>
		/// <returns>The result of verifying the DKIM-Signature.</returns>
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
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public Task<DkimSignatureValidationResult> VerifyAsync (MimeMessage message, Header dkimSignature, CancellationToken cancellationToken = default)
		{
			return VerifyAsync (FormatOptions.Default, message, dkimSignature, cancellationToken);
		}

		static List<Header> GetSignatureHeaders (FormatOptions options, MimeMessage message, int max)
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			if (message == null)
				throw new ArgumentNullException (nameof (message));

			var signatures = new List<Header> ();

			foreach (var header in message.Headers) {
				if (header.Id != HeaderId.DkimSignature)
					continue;

				if (signatures.Count == max)
					break;

				signatures.Add (header);
			}

			return signatures;
		}

		/// <summary>
		/// Verify all of the DKIM-Signature headers in the message.
		/// </summary>
		/// <remarks>
		/// <para>Verifies each of the DKIM-Signature headers in the message, in the order in which they appear,
		/// up to a maximum of <see cref="MaxSignatures"/>.</para>
		/// <para>Malformed signatures, DNS lookup failures and invalid public keys do not throw exceptions; they are
		/// reported via the <see cref="DkimSignatureValidationResult.Status"/> of each result.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
		/// </example>
		/// <returns>The results of verifying each DKIM-Signature; an empty array if the message is not signed.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="message">The message to verify.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="message"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public DkimSignatureValidationResult[] Verify (FormatOptions options, MimeMessage message, CancellationToken cancellationToken = default)
		{
			var signatures = GetSignatureHeaders (options, message, MaxSignatures);
			var results = new DkimSignatureValidationResult[signatures.Count];

			for (int i = 0; i < signatures.Count; i++)
				results[i] = Verify (options, message, signatures[i], cancellationToken);

			return results;
		}

		/// <summary>
		/// Asynchronously verify all of the DKIM-Signature headers in the message.
		/// </summary>
		/// <remarks>
		/// <para>Verifies each of the DKIM-Signature headers in the message, in the order in which they appear,
		/// up to a maximum of <see cref="MaxSignatures"/>.</para>
		/// <para>Malformed signatures, DNS lookup failures and invalid public keys do not throw exceptions; they are
		/// reported via the <see cref="DkimSignatureValidationResult.Status"/> of each result.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
		/// </example>
		/// <returns>The results of verifying each DKIM-Signature; an empty array if the message is not signed.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="message">The message to verify.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="message"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public async Task<DkimSignatureValidationResult[]> VerifyAsync (FormatOptions options, MimeMessage message, CancellationToken cancellationToken = default)
		{
			var signatures = GetSignatureHeaders (options, message, MaxSignatures);
			var results = new DkimSignatureValidationResult[signatures.Count];

			for (int i = 0; i < signatures.Count; i++)
				results[i] = await VerifyAsync (options, message, signatures[i], cancellationToken).ConfigureAwait (false);

			return results;
		}

		/// <summary>
		/// Verify all of the DKIM-Signature headers in the message.
		/// </summary>
		/// <remarks>
		/// <para>Verifies each of the DKIM-Signature headers in the message, in the order in which they appear,
		/// up to a maximum of <see cref="MaxSignatures"/>.</para>
		/// <para>Malformed signatures, DNS lookup failures and invalid public keys do not throw exceptions; they are
		/// reported via the <see cref="DkimSignatureValidationResult.Status"/> of each result.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
		/// </example>
		/// <returns>The results of verifying each DKIM-Signature; an empty array if the message is not signed.</returns>
		/// <param name="message">The message to verify.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="message"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public DkimSignatureValidationResult[] Verify (MimeMessage message, CancellationToken cancellationToken = default)
		{
			return Verify (FormatOptions.Default, message, cancellationToken);
		}

		/// <summary>
		/// Asynchronously verify all of the DKIM-Signature headers in the message.
		/// </summary>
		/// <remarks>
		/// <para>Verifies each of the DKIM-Signature headers in the message, in the order in which they appear,
		/// up to a maximum of <see cref="MaxSignatures"/>.</para>
		/// <para>Malformed signatures, DNS lookup failures and invalid public keys do not throw exceptions; they are
		/// reported via the <see cref="DkimSignatureValidationResult.Status"/> of each result.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
		/// </example>
		/// <returns>The results of verifying each DKIM-Signature; an empty array if the message is not signed.</returns>
		/// <param name="message">The message to verify.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="message"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public Task<DkimSignatureValidationResult[]> VerifyAsync (MimeMessage message, CancellationToken cancellationToken = default)
		{
			return VerifyAsync (FormatOptions.Default, message, cancellationToken);
		}
	}
}
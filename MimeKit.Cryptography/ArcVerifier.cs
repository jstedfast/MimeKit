//
// ArcVerifier.cs
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
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;

using MimeKit.IO;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An ARC signature validation result.
	/// </summary>
	/// <remarks>
	/// An ARC signature validation result.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\ArcVerifierExample.cs" />
	/// </example>
	public enum ArcSignatureValidationResult
	{
		/// <summary>
		/// No signatures to validate.
		/// </summary>
		None,

		/// <summary>
		/// The validation passed.
		/// </summary>
		Pass,

		/// <summary>
		/// The validation failed.
		/// </summary>
		Fail
	}

	/// <summary>
	/// An enumeration of possible ARC validation errors.
	/// </summary>
	/// <remarks>
	/// An enumeration of possible ARC validation errors.
	/// </remarks>
	[Flags]
	public enum ArcValidationErrors
	{
		/// <summary>
		/// No errors.
		/// </summary>
		None                               = 0,

		/// <summary>
		/// One or more duplicate ARC-Authentication-Results headers exist.
		/// </summary>
		DuplicateArcAuthenticationResults  = 1 << 0,

		/// <summary>
		/// One or more duplicate ARC-Message-Signature headers exist.
		/// </summary>
		DuplicateArcMessageSignature       = 1 << 1,

		/// <summary>
		/// One or more duplicate ARC-Seal headers exist.
		/// </summary>
		DuplicateArcSeal                   = 1 << 2,

		/// <summary>
		/// One or more ARC-Authentication-Results headers are missing.
		/// </summary>
		MissingArcAuthenticationResults    = 1 << 3,

		/// <summary>
		/// One or more ARC-Message-Signature headers are missing.
		/// </summary>
		MissingArcMessageSignature         = 1 << 4,

		/// <summary>
		/// One or more ARC-Seal headers are missing.
		/// </summary>
		MissingArcSeal                     = 1 << 5,

		/// <summary>
		/// One or more ARC-Authentication-Results headers could not be parsed.
		/// </summary>
		InvalidArcAuthenticationResults    = 1 << 6,

		/// <summary>
		/// One or more ARC-Message-Signature headers could not be parsed.
		/// </summary>
		InvalidArcMessageSignature         = 1 << 7,

		/// <summary>
		/// One or more ARC-Seal headers could not be parsed.
		/// </summary>
		InvalidArcSeal                     = 1 << 8,

		/// <summary>
		/// One or more ARC-Seal headers have an invalid <c>cv</c> value.
		/// </summary>
		InvalidArcSealChainValidationValue = 1 << 9,

		/// <summary>
		/// One or more ARC-Seal headers are missing a <c>cv</c> value.
		/// </summary>
		MissingArcSealChainValidationValue = 1 << 10,

		/// <summary>
		/// Validation failed for the most recent ARC-Message-Signature header.
		/// </summary>
		MessageSignatureValidationFailed   = 1 << 11,

		/// <summary>
		/// Validation failed for one or more of the ARC-Seal headers.
		/// </summary>
		SealValidationFailed               = 1 << 12,

		/// <summary>
		/// The public key for one or more of the ARC-Message-Signature or ARC-Seal headers could not be
		/// retrieved due to a temporary DNS failure.
		/// </summary>
		/// <remarks>
		/// <para>All errors, including temporary DNS failures, result in a chain validation status of
		/// <see cref="ArcSignatureValidationResult.Fail"/>, as required by
		/// <a href="https://www.rfc-editor.org/rfc/rfc8617#section-5.2.1">RFC 8617, Section 5.2.1</a>.</para>
		/// <para>When this flag is set, the failure may not be reproducible, so a receiver may wish to
		/// temporarily reject (defer) the message and verify it again later rather than acting on the
		/// failed result or sealing the message with <c>cv=fail</c>.</para>
		/// </remarks>
		DnsTemporaryFailure                = 1 << 13
	}

	/// <summary>
	/// An ARC header validation result.
	/// </summary>
	/// <remarks>
	/// Represents an ARC header and its signature validation result.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\ArcVerifierExample.cs" />
	/// </example>
	public class ArcHeaderValidationResult
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="ArcHeaderValidationResult"/> class.
		/// </summary>
		/// <param name="header">The ARC header.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="header"/> is <see langword="null"/>.
		/// </exception>
		internal ArcHeaderValidationResult (Header header)
		{
			if (header == null)
				throw new ArgumentNullException (nameof (header));

			Header = header;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="ArcHeaderValidationResult"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="ArcHeaderValidationResult"/>.
		/// </remarks>
		/// <param name="header">The ARC header.</param>
		/// <param name="signature">The signature validation result.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="header"/> is <see langword="null"/>.
		/// </exception>
		public ArcHeaderValidationResult (Header header, ArcSignatureValidationResult signature) : this (header)
		{
			Signature = signature;
		}

		/// <summary>
		/// Get the signature validation result.
		/// </summary>
		/// <remarks>
		/// Gets the signature validation result.
		/// </remarks>
		/// <value>The signature validation result.</value>
		public ArcSignatureValidationResult Signature {
			get; internal set;
		}

		/// <summary>
		/// Get the ARC header.
		/// </summary>
		/// <remarks>
		/// Gets the ARC header.
		/// </remarks>
		/// <value>The ARC header.</value>
		public Header Header {
			get; private set;
		}

		/// <summary>
		/// Get a description of why the signature did not pass.
		/// </summary>
		/// <remarks>
		/// Gets a short, human-readable description of why the signature did not pass.
		/// </remarks>
		/// <value>The reason or <see langword="null"/> if the signature passed or was not verified.</value>
		public string? Reason {
			get; internal set;
		}

		/// <summary>
		/// Get the exception that caused the verification to fail, if any.
		/// </summary>
		/// <remarks>
		/// Gets the exception that caused the verification to fail, such as a <see cref="FormatException"/>
		/// for a malformed header or the exception thrown by the <see cref="IDnsResolver"/>.
		/// </remarks>
		/// <value>The exception or <see langword="null"/>.</value>
		public Exception? Exception {
			get; internal set;
		}
	}

	/// <summary>
	/// An ARC validation result.
	/// </summary>
	/// <remarks>
	/// <para>Represents the results of <a href="Overload_MimeKit_Cryptography_ArcVerifier_Verify">ArcVerifier.Verify</a>
	/// or <a href="Overload_MimeKit_Cryptography_ArcVerifier_VerifyAsync">ArcVerifier.VerifyAsync</a>.</para>
	/// <para>If no ARC headers are found on the <see cref="MimeMessage"/>, then the <see cref="Chain"/> result will be
	/// <see cref="ArcSignatureValidationResult.None"/> and both <see cref="MessageSignature"/> and <see cref="Seals"/>
	/// will be <see langword="null"/>.</para>
	/// <para>If ARC headers are found on the <see cref="MimeMessage"/> but could not be parsed, then the
	/// <see cref="Chain"/> result will be <see cref="ArcSignatureValidationResult.Fail"/> and both
	/// <see cref="MessageSignature"/> and <see cref="Seals"/> will be <see langword="null"/>.</para>
	/// <para>As required by <a href="https://www.rfc-editor.org/rfc/rfc8617#section-5.2.1">RFC 8617, Section 5.2.1</a>,
	/// any error encountered while validating the ARC chain, including DNS failures, results in a <see cref="Chain"/>
	/// result of <see cref="ArcSignatureValidationResult.Fail"/>. To distinguish failures that might succeed if
	/// retried later, check <see cref="ChainErrors"/> for <see cref="ArcValidationErrors.DnsTemporaryFailure"/>.
	/// Additional details are available via the <see cref="ArcHeaderValidationResult.Reason"/> and
	/// <see cref="ArcHeaderValidationResult.Exception"/> properties of <see cref="MessageSignature"/> and
	/// <see cref="Seals"/>.</para>
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\ArcVerifierExample.cs" />
	/// </example>
	public class ArcValidationResult
	{
		internal ArcValidationResult ()
		{
			Chain = ArcSignatureValidationResult.None;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="ArcValidationResult"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="ArcValidationResult"/>.
		/// </remarks>
		/// <param name="chain">The signature validation results of the entire chain.</param>
		/// <param name="messageSignature">The validation results for the ARC-Message-Signature header.</param>
		/// <param name="seals">The validation results for the ARC-Seal headers.</param>
		public ArcValidationResult (ArcSignatureValidationResult chain, ArcHeaderValidationResult messageSignature, ArcHeaderValidationResult[] seals)
		{
			MessageSignature = messageSignature;
			Seals = seals;
			Chain = chain;
		}

		/// <summary>
		/// Get the validation results for the ARC-Message-Signature header.
		/// </summary>
		/// <remarks>
		/// Gets the validation results for the ARC-Message-Signature header.
		/// </remarks>
		/// <value>The validation results for the ARC-Message-Signature header or <see langword="null"/>
		/// if the ARC-Message-Signature header was not found.</value>
		public ArcHeaderValidationResult? MessageSignature {
			get; internal set;
		}

		/// <summary>
		/// Get the validation results for each of the ARC-Seal headers.
		/// </summary>
		/// <remarks>
		/// Gets the validation results for each of the ARC-Seal headers in
		/// their instance order.
		/// </remarks>
		/// <value>The array of validation results for the ARC-Seal headers or <see langword="null"/>
		/// if no ARC-Seal headers were found.</value>
		public ArcHeaderValidationResult[]? Seals {
			get; internal set;
		}

		/// <summary>
		/// Get the signature validation results of the entire chain.
		/// </summary>
		/// <remarks>
		/// Gets the signature validation results of the entire chain.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\ArcVerifierExample.cs" />
		/// </example>
		/// <value>The signature validation results of the entire chain.</value>
		public ArcSignatureValidationResult Chain {
			get; internal set;
		}

		/// <summary>
		/// Get the chain validation errors.
		/// </summary>
		/// <remarks>
		/// Gets the chain validation errors.
		/// </remarks>
		/// <value>The chain validation errors.</value>
		public ArcValidationErrors ChainErrors {
			get; internal set;
		}
	}

	class ArcHeaderSet
	{
		public Header? ArcAuthenticationResult { get; private set; }

		public Dictionary<string, string>? ArcMessageSignatureParameters { get; private set; }
		public Header? ArcMessageSignature { get; private set; }

		public Dictionary<string, string>? ArcSealParameters { get; private set; }
		public Header? ArcSeal { get; private set; }

		public bool Add (Header header, Dictionary<string, string>? parameters)
		{
			switch (header.Id) {
			case HeaderId.ArcAuthenticationResults:
				if (ArcAuthenticationResult != null)
					return false;

				ArcAuthenticationResult = header;
				break;
			case HeaderId.ArcMessageSignature:
				if (ArcMessageSignature != null)
					return false;

				Debug.Assert (parameters != null);

				ArcMessageSignatureParameters = parameters;
				ArcMessageSignature = header;
				break;
			case HeaderId.ArcSeal:
				if (ArcSeal != null)
					return false;

				Debug.Assert (parameters != null);

				ArcSealParameters = parameters;
				ArcSeal = header;
				break;
			default:
				return false;
			}

			return true;
		}
	}

	/// <summary>
	/// An ARC verifier.
	/// </summary>
	/// <remarks>
	/// Validates Authenticated Received Chains.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\ArcVerifierExample.cs" />
	/// </example>
	public class ArcVerifier : DkimVerifierBase
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="ArcVerifier"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="ArcVerifier"/>.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\ArcVerifierExample.cs" />
		/// </example>
		/// <param name="resolver">The DNS resolver used to look up the public keys.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="resolver"/> is <see langword="null"/>.
		/// </exception>
		public ArcVerifier (IDnsResolver resolver) : base (resolver)
		{
		}

		static void ValidateArcMessageSignatureParameters (Dictionary<string, string> parameters, out DkimSignatureAlgorithm algorithm, out DkimCanonicalizationAlgorithm headerAlgorithm,
			out DkimCanonicalizationAlgorithm bodyAlgorithm, out string d, out string s, out string q, out string[] headers, out string bh, out string b, out int maxLength)
		{
			ValidateCommonSignatureParameters ("ARC-Message-Signature", parameters, out algorithm, out headerAlgorithm, out bodyAlgorithm, out d, out s, out q, out headers, out bh, out b, out maxLength);
		}

		static void ValidateArcSealParameters (Dictionary<string, string> parameters, out DkimSignatureAlgorithm algorithm, out string d, out string s, out string q, out string b)
		{
			ValidateCommonParameters ("ARC-Seal", parameters, out algorithm, out d, out s, out q, out b);

			if (parameters.TryGetValue ("h", out _))
				throw new FormatException ("Malformed ARC-Seal header: the 'h' parameter tag is not allowed.");
		}

		static DkimSignatureStatus SetHeaderResult (ArcHeaderValidationResult result, DkimSignatureStatus status, string? reason, Exception? exception = null)
		{
			result.Signature = status == DkimSignatureStatus.Pass ? ArcSignatureValidationResult.Pass : ArcSignatureValidationResult.Fail;
			result.Exception = exception;
			result.Reason = reason;

			return status;
		}

		DkimSignatureInfo? PrepareArcMessageSignatureVerification (FormatOptions options, MimeMessage message, ArcHeaderSet set, ArcHeaderValidationResult result)
		{
			DkimCanonicalizationAlgorithm headerAlgorithm, bodyAlgorithm;
			DkimSignatureAlgorithm signatureAlgorithm;
			string d, s, q, bh, b;
			string[] headers;
			int maxLength;

			try {
				ValidateArcMessageSignatureParameters (set.ArcMessageSignatureParameters!, out signatureAlgorithm, out headerAlgorithm, out bodyAlgorithm,
					out d, out s, out q, out headers, out bh, out b, out maxLength);

				if (!IsEnabled (signatureAlgorithm)) {
					SetHeaderResult (result, DkimSignatureStatus.Policy, "signature algorithm disabled");
					return null;
				}

				options = options.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				// first check the body hash (if that's invalid, then the entire signature is invalid)
				if (!VerifyBodyHash (options, message, signatureAlgorithm, bodyAlgorithm, maxLength, bh)) {
					SetHeaderResult (result, DkimSignatureStatus.Fail, "body hash did not verify");
					return null;
				}

				return new DkimSignatureInfo (options, set.ArcMessageSignature!, signatureAlgorithm, d, s, q, b) {
					HeaderAlgorithm = headerAlgorithm,
					Headers = headers
				};
			} catch (OperationCanceledException) {
				throw;
			} catch (Exception ex) {
				SetHeaderResult (result, DkimSignatureStatus.PermError, ex.Message, ex);
				return null;
			}
		}

		DkimSignatureStatus CompleteArcMessageSignatureVerification (DkimSignatureInfo info, MimeMessage message, DkimPublicKeyLookupResult lookup, ArcHeaderValidationResult result)
		{
			var status = GetVerificationKey (lookup, out var key, out var reason);

			if (status != DkimSignatureStatus.Pass)
				return SetHeaderResult (result, status, reason, lookup.Exception);

			try {
				if (VerifySignature (info.Options, message, info.Header, info.SignatureAlgorithm, key!, info.Headers!, info.HeaderAlgorithm, info.Signature))
					return SetHeaderResult (result, DkimSignatureStatus.Pass, null);

				return SetHeaderResult (result, DkimSignatureStatus.Fail, "signature did not verify");
			} catch (OperationCanceledException) {
				throw;
			} catch (Exception ex) {
				return SetHeaderResult (result, DkimSignatureStatus.PermError, ex.Message, ex);
			}
		}

		DkimSignatureStatus VerifyArcMessageSignature (FormatOptions options, MimeMessage message, ArcHeaderSet set, ArcHeaderValidationResult result, CancellationToken cancellationToken)
		{
			var info = PrepareArcMessageSignatureVerification (options, message, set, result);

			if (info == null)
				return DkimSignatureStatus.Fail;

			var lookup = LookupPublicKey (info, cancellationToken);

			return CompleteArcMessageSignatureVerification (info, message, lookup, result);
		}

		async Task<DkimSignatureStatus> VerifyArcMessageSignatureAsync (FormatOptions options, MimeMessage message, ArcHeaderSet set, ArcHeaderValidationResult result, CancellationToken cancellationToken)
		{
			var info = PrepareArcMessageSignatureVerification (options, message, set, result);

			if (info == null)
				return DkimSignatureStatus.Fail;

			var lookup = await LookupPublicKeyAsync (info, cancellationToken).ConfigureAwait (false);

			return CompleteArcMessageSignatureVerification (info, message, lookup, result);
		}

		DkimSignatureInfo? PrepareArcSealVerification (FormatOptions options, ArcHeaderSet[] sets, int i, ArcHeaderValidationResult result)
		{
			DkimSignatureAlgorithm algorithm;
			string d, s, q, b;

			try {
				ValidateArcSealParameters (sets[i].ArcSealParameters!, out algorithm, out d, out s, out q, out b);
			} catch (Exception ex) {
				SetHeaderResult (result, DkimSignatureStatus.PermError, ex.Message, ex);
				return null;
			}

			if (!IsEnabled (algorithm)) {
				SetHeaderResult (result, DkimSignatureStatus.Policy, "signature algorithm disabled");
				return null;
			}

			options = options.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;

			return new DkimSignatureInfo (options, sets[i].ArcSeal!, algorithm, d, s, q, b);
		}

		DkimSignatureStatus VerifyArcSeal (FormatOptions options, ArcHeaderSet[] sets, int i, ArcHeaderValidationResult result, CancellationToken cancellationToken)
		{
			var info = PrepareArcSealVerification (options, sets, i, result);

			if (info == null)
				return DkimSignatureStatus.Fail;

			var lookup = LookupPublicKey (info, cancellationToken);

			return CompleteArcSealVerification (info, sets, i, lookup, result, cancellationToken);
		}

		async Task<DkimSignatureStatus> VerifyArcSealAsync (FormatOptions options, ArcHeaderSet[] sets, int i, ArcHeaderValidationResult result, CancellationToken cancellationToken)
		{
			var info = PrepareArcSealVerification (options, sets, i, result);

			if (info == null)
				return DkimSignatureStatus.Fail;

			var lookup = await LookupPublicKeyAsync (info, cancellationToken).ConfigureAwait (false);

			return CompleteArcSealVerification (info, sets, i, lookup, result, cancellationToken);
		}

		DkimSignatureStatus CompleteArcSealVerification (DkimSignatureInfo info, ArcHeaderSet[] sets, int i, DkimPublicKeyLookupResult lookup, ArcHeaderValidationResult result, CancellationToken cancellationToken)
		{
			var status = GetVerificationKey (lookup, out var key, out var reason);

			if (status != DkimSignatureStatus.Pass)
				return SetHeaderResult (result, status, reason, lookup.Exception);

			var options = info.Options;

			try {
				using (var stream = new DkimSignatureStream (CreateVerifyContext (info.SignatureAlgorithm, key!))) {
					using (var filtered = new FilteredStream (stream)) {
						filtered.Add (options.CreateNewLineFilter ());

						for (int j = 0; j < i; j++) {
							WriteHeaderRelaxed (options, filtered, sets[j].ArcAuthenticationResult!, false);
							WriteHeaderRelaxed (options, filtered, sets[j].ArcMessageSignature!, false);
							WriteHeaderRelaxed (options, filtered, sets[j].ArcSeal!, false);
						}

						WriteHeaderRelaxed (options, filtered, sets[i].ArcAuthenticationResult!, false);
						WriteHeaderRelaxed (options, filtered, sets[i].ArcMessageSignature!, false);

						// now include the ARC-Seal header that we are verifying,
						// but only after removing the "b=" signature value.
						var seal = GetSignedSignatureHeader (sets[i].ArcSeal!);

						WriteHeaderRelaxed (options, filtered, seal, true);

						filtered.Flush (cancellationToken);
					}

					if (stream.VerifySignature (info.Signature))
						return SetHeaderResult (result, DkimSignatureStatus.Pass, null);

					return SetHeaderResult (result, DkimSignatureStatus.Fail, "signature did not verify");
				}
			} catch (OperationCanceledException) {
				throw;
			} catch (Exception ex) {
				return SetHeaderResult (result, DkimSignatureStatus.PermError, ex.Message, ex);
			}
		}

		internal static ArcSignatureValidationResult GetArcHeaderSets (MimeMessage message, bool throwOnError, out ArcHeaderSet[] sets, out int count, out ArcValidationErrors errors)
		{
			ArcHeaderSet set;

			errors = ArcValidationErrors.None;
			sets = new ArcHeaderSet[50];
			count = 0;

			for (int i = 0; i < message.Headers.Count; i++) {
				Dictionary<string, string>? parameters = null;
				var header = message.Headers[i];
				int instance = 0;
				string? value;

				switch (header.Id) {
				case HeaderId.ArcAuthenticationResults:
					if (!AuthenticationResults.TryParse (header.RawValue, out AuthenticationResults? authres)) {
						if (throwOnError)
							throw new FormatException ("Invalid ARC-Authentication-Results header.");

						errors |= ArcValidationErrors.InvalidArcAuthenticationResults;
						break;
					}

					if (!authres.Instance.HasValue) {
						if (throwOnError)
							throw new FormatException ("Missing instance tag in ARC-Authentication-Results header.");

						errors |= ArcValidationErrors.InvalidArcAuthenticationResults;
						break;
					}

					instance = authres.Instance.Value;

					if (instance < 1 || instance > 50) {
						if (throwOnError)
							throw new FormatException (string.Format (CultureInfo.InvariantCulture, "Invalid instance tag in ARC-Authentication-Results header: i={0}", instance));

						errors |= ArcValidationErrors.InvalidArcAuthenticationResults;
						instance = 0;
						break;
					}
					break;
				case HeaderId.ArcMessageSignature:
				case HeaderId.ArcSeal:
					try {
						parameters = ParseParameterTags (header.Id, header.Value);
					} catch {
						if (throwOnError)
							throw;

						if (header.Id == HeaderId.ArcMessageSignature)
							errors |= ArcValidationErrors.InvalidArcMessageSignature;
						else
							errors |= ArcValidationErrors.InvalidArcSeal;

						break;
					}

					if (!parameters.TryGetValue ("i", out value)) {
						if (throwOnError)
							throw new FormatException (string.Format (CultureInfo.InvariantCulture, "Missing instance tag in {0} header.", header.Id.ToHeaderName ()));

						if (header.Id == HeaderId.ArcMessageSignature)
							errors |= ArcValidationErrors.InvalidArcMessageSignature;
						else
							errors |= ArcValidationErrors.InvalidArcSeal;

						break;
					}

					if (!int.TryParse (value, NumberStyles.None, CultureInfo.InvariantCulture, out instance) || instance < 1 || instance > 50) {
						if (throwOnError)
							throw new FormatException (string.Format (CultureInfo.InvariantCulture, "Invalid instance tag in {0} header: i={1}", header.Id.ToHeaderName (), value));

						if (header.Id == HeaderId.ArcMessageSignature)
							errors |= ArcValidationErrors.InvalidArcMessageSignature;
						else
							errors |= ArcValidationErrors.InvalidArcSeal;

						instance = 0;
						break;
					}
					break;
				}

				if (instance == 0)
					continue;

				set = sets[instance - 1];
				if (set == null)
					sets[instance - 1] = set = new ArcHeaderSet ();

				if (!set.Add (header, parameters)) {
					if (throwOnError)
						throw new FormatException (string.Format (CultureInfo.InvariantCulture, "Duplicate {0} header for i={1}", header.Id.ToHeaderName (), instance));

					switch (header.Id) {
					case HeaderId.ArcAuthenticationResults:
						errors |= ArcValidationErrors.DuplicateArcAuthenticationResults;
						break;
					case HeaderId.ArcMessageSignature:
						errors |= ArcValidationErrors.DuplicateArcMessageSignature;
						break;
					case HeaderId.ArcSeal:
						errors |= ArcValidationErrors.DuplicateArcSeal;
						break;
					}
				}

				if (instance > count)
					count = instance;
			}

			if (count == 0) {
				// there are no ARC sets
				return ArcSignatureValidationResult.None;
			}

			// verify that all ARC sets are complete
			for (int i = 0; i < count; i++) {
				set = sets[i];

				if (set == null) {
					if (throwOnError)
						throw new FormatException (string.Format (CultureInfo.InvariantCulture, "Missing ARC headers for i={0}", i + 1));

					if ((errors & ArcValidationErrors.InvalidArcAuthenticationResults) == 0)
						errors |= ArcValidationErrors.MissingArcAuthenticationResults;
					if ((errors & ArcValidationErrors.InvalidArcMessageSignature) == 0)
						errors |= ArcValidationErrors.MissingArcMessageSignature;
					if ((errors & ArcValidationErrors.InvalidArcSeal) == 0)
						errors |= ArcValidationErrors.MissingArcSeal;
					continue;
				}

				if (set.ArcAuthenticationResult == null) {
					if (throwOnError)
						throw new FormatException (string.Format (CultureInfo.InvariantCulture, "Missing ARC-Authentication-Results header for i={0}", i + 1));

					if ((errors & ArcValidationErrors.InvalidArcAuthenticationResults) == 0)
						errors |= ArcValidationErrors.MissingArcAuthenticationResults;
				}

				if (set.ArcMessageSignature == null) {
					if (throwOnError)
						throw new FormatException (string.Format (CultureInfo.InvariantCulture, "Missing ARC-Message-Signature header for i={0}", i + 1));

					if ((errors & ArcValidationErrors.InvalidArcMessageSignature) == 0)
						errors |= ArcValidationErrors.MissingArcMessageSignature;
				}

				if (set.ArcSeal == null) {
					if (throwOnError)
						throw new FormatException (string.Format (CultureInfo.InvariantCulture, "Missing ARC-Seal header for i={0}", i + 1));

					if ((errors & ArcValidationErrors.InvalidArcSeal) == 0)
						errors |= ArcValidationErrors.MissingArcSeal;
					continue;
				}

				if (!set.ArcSealParameters!.TryGetValue ("cv", out string? cv)) {
					if (throwOnError)
						throw new FormatException (string.Format (CultureInfo.InvariantCulture, "Missing chain validation tag in ARC-Seal header for i={0}.", i + 1));

					errors |= ArcValidationErrors.MissingArcSealChainValidationValue;
					continue;
				}

				// The "cv" value for all ARC-Seal header fields MUST NOT be
				// "fail". For ARC Sets with instance values > 1, the values
				// MUST be "pass". For the ARC Set with instance value = 1, the
				// value MUST be "none".
				if (!cv.Equals (i == 0 ? "none" : "pass", StringComparison.Ordinal))
					errors |= ArcValidationErrors.InvalidArcSealChainValidationValue;
			}

			return errors == ArcValidationErrors.None ? ArcSignatureValidationResult.Pass : ArcSignatureValidationResult.Fail;
		}

		static bool PrepareChainVerification (FormatOptions options, MimeMessage message, out ArcValidationResult result, out ArcHeaderSet[] sets, out int count)
		{
			const ArcValidationErrors ArcSealCvParamErrors = ArcValidationErrors.InvalidArcSealChainValidationValue | ArcValidationErrors.MissingArcSealChainValidationValue;

			if (options == null)
				throw new ArgumentNullException (nameof (options));

			if (message == null)
				throw new ArgumentNullException (nameof (message));

			result = new ArcValidationResult ();

			switch (GetArcHeaderSets (message, false, out sets, out count, out var errors)) {
			case ArcSignatureValidationResult.None: return false;
			case ArcSignatureValidationResult.Fail:
				result.Chain = ArcSignatureValidationResult.Fail;
				result.ChainErrors = errors;

				// If the only error(s) are invalid or missing 'cv' values, ignore the errors for now.
				if ((errors & ~ArcSealCvParamErrors) == 0)
					break;

				return false;
			default:
				result.Chain = ArcSignatureValidationResult.Pass;
				break;
			}

			int newest = count - 1;

			result.Seals = new ArcHeaderValidationResult[count];
			result.MessageSignature = new ArcHeaderValidationResult (sets[newest].ArcMessageSignature!);

			return true;
		}

		static void SetChainResult (ArcValidationResult result, DkimSignatureStatus status, ArcValidationErrors error)
		{
			if (status == DkimSignatureStatus.Pass)
				return;

			// Note: All errors, including DNS failures, are considered permanent and result in a chain
			// validation status of "fail" (RFC 8617, Section 5.2.1). However, temporary DNS failures are
			// flagged so that callers can choose to defer the message instead.
			if (status == DkimSignatureStatus.TempError)
				result.ChainErrors |= ArcValidationErrors.DnsTemporaryFailure;

			result.Chain = ArcSignatureValidationResult.Fail;
			result.ChainErrors |= error;
		}

		/// <summary>
		/// Verify the ARC signature chain.
		/// </summary>
		/// <remarks>
		/// Verifies the ARC signature chain.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\ArcVerifierExample.cs" />
		/// </example>
		/// <returns>The ARC validation result.</returns>
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
		public ArcValidationResult Verify (FormatOptions options, MimeMessage message, CancellationToken cancellationToken = default)
		{
			if (!PrepareChainVerification (options, message, out var result, out var sets, out int count))
				return result;

			int newest = count - 1;
			DkimSignatureStatus status;

			// validate the most recent Arc-Message-Signature
			status = VerifyArcMessageSignature (options, message, sets[newest], result.MessageSignature!, cancellationToken);
			SetChainResult (result, status, ArcValidationErrors.MessageSignatureValidationFailed);

			// validate all Arc-Seals starting with the most recent and proceeding to the oldest
			for (int i = newest; i >= 0; i--) {
				result.Seals![i] = new ArcHeaderValidationResult (sets[i].ArcSeal!);

				status = VerifyArcSeal (options, sets, i, result.Seals[i], cancellationToken);
				SetChainResult (result, status, ArcValidationErrors.SealValidationFailed);
			}

			return result;
		}

		/// <summary>
		/// Asynchronously verify the ARC signature chain.
		/// </summary>
		/// <remarks>
		/// Asynchronously verifies the ARC signature chain.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\ArcVerifierExample.cs" />
		/// </example>
		/// <returns>The ARC validation result.</returns>
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
		public async Task<ArcValidationResult> VerifyAsync (FormatOptions options, MimeMessage message, CancellationToken cancellationToken = default)
		{
			if (!PrepareChainVerification (options, message, out var result, out var sets, out int count))
				return result;

			int newest = count - 1;
			DkimSignatureStatus status;

			// validate the most recent Arc-Message-Signature
			status = await VerifyArcMessageSignatureAsync (options, message, sets[newest], result.MessageSignature!, cancellationToken).ConfigureAwait (false);
			SetChainResult (result, status, ArcValidationErrors.MessageSignatureValidationFailed);

			// validate all Arc-Seals starting with the most recent and proceeding to the oldest
			for (int i = newest; i >= 0; i--) {
				result.Seals![i] = new ArcHeaderValidationResult (sets[i].ArcSeal!);

				status = await VerifyArcSealAsync (options, sets, i, result.Seals[i], cancellationToken).ConfigureAwait (false);
				SetChainResult (result, status, ArcValidationErrors.SealValidationFailed);
			}

			return result;
		}

		/// <summary>
		/// Verify the ARC signature chain.
		/// </summary>
		/// <remarks>
		/// Verifies the ARC signature chain.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\ArcVerifierExample.cs" />
		/// </example>
		/// <returns>The ARC validation result.</returns>
		/// <param name="message">The message to verify.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="message"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public ArcValidationResult Verify (MimeMessage message, CancellationToken cancellationToken = default)
		{
			return Verify (FormatOptions.Default, message, cancellationToken);
		}

		/// <summary>
		/// Asynchronously verify the ARC signature chain.
		/// </summary>
		/// <remarks>
		/// Asynchronously verifies the ARC signature chain.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\ArcVerifierExample.cs" />
		/// </example>
		/// <returns>The ARC validation result.</returns>
		/// <param name="message">The message to verify.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="message"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public Task<ArcValidationResult> VerifyAsync (MimeMessage message, CancellationToken cancellationToken = default)
		{
			return VerifyAsync (FormatOptions.Default, message, cancellationToken);
		}
	}
}

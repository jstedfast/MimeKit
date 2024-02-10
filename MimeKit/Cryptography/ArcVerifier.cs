//
// ArcVerifier.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2024 .NET Foundation and Contributors
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

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

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
		SealValidationFailed               = 1 << 12
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
		/// <paramref name="header"/> is <c>null</c>.
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
		/// <paramref name="header"/> is <c>null</c>.
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
	}

	/// <summary>
	/// An ARC validation result.
	/// </summary>
	/// <remarks>
	/// <para>Represents the results of <a href="Overload_MimeKit_Cryptography_ArcVerifier_Verify">ArcVerifier.Verify</a>
	/// or <a href="Overload_MimeKit_Cryptography_ArcVerifier_VerifyAsync">ArcVerifier.VerifyAsync</a>.</para>
	/// <para>If no ARC headers are found on the <see cref="MimeMessage"/>, then the <see cref="Chain"/> result will be
	/// <see cref="ArcSignatureValidationResult.None"/> and both <see cref="MessageSignature"/> and <see cref="Seals"/>
	/// will be <c>null</c>.</para>
	/// <para>If ARC headers are found on the <see cref="MimeMessage"/> but could not be parsed, then the
	/// <see cref="Chain"/> result will be <see cref="ArcSignatureValidationResult.Fail"/> and both
	/// <see cref="MessageSignature"/> and <see cref="Seals"/> will be <c>null</c>.</para>
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
		/// <value>The validation results for the ARC-Message-Signature header or <c>null</c>
		/// if the ARC-Message-Signature header was not found.</value>
		public ArcHeaderValidationResult MessageSignature {
			get; internal set;
		}

		/// <summary>
		/// Get the validation results for each of the ARC-Seal headers.
		/// </summary>
		/// <remarks>
		/// Gets the validation results for each of the ARC-Seal headers in
		/// their instance order.
		/// </remarks>
		/// <value>The array of validation results for the ARC-Seal headers or <c>null</c>
		/// if no ARC-Seal headers were found.</value>
		public ArcHeaderValidationResult[] Seals {
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
		public Header ArcAuthenticationResult { get; private set; }

		public Dictionary<string, string> ArcMessageSignatureParameters { get; private set; }
		public Header ArcMessageSignature { get; private set; }

		public Dictionary<string, string> ArcSealParameters { get; private set; }
		public Header ArcSeal { get; private set; }

		public bool Add (Header header, Dictionary<string, string> parameters)
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

				ArcMessageSignatureParameters = parameters;
				ArcMessageSignature = header;
				break;
			case HeaderId.ArcSeal:
				if (ArcSeal != null)
					return false;

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
		/// <param name="publicKeyLocator">The public key locator.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="publicKeyLocator"/> is <c>null</c>.
		/// </exception>
		public ArcVerifier (IDkimPublicKeyLocator publicKeyLocator) : base (publicKeyLocator)
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

		async Task<bool> VerifyArcMessageSignatureAsync (FormatOptions options, MimeMessage message, Header arcSignature, Dictionary<string, string> parameters, bool doAsync, CancellationToken cancellationToken)
		{
			DkimCanonicalizationAlgorithm headerAlgorithm, bodyAlgorithm;
			DkimSignatureAlgorithm signatureAlgorithm;
			AsymmetricKeyParameter key;
			string d, s, q, bh, b;
			string[] headers;
			int maxLength;

			ValidateArcMessageSignatureParameters (parameters, out signatureAlgorithm, out headerAlgorithm, out bodyAlgorithm,
				out d, out s, out q, out headers, out bh, out b, out maxLength);

			if (!IsEnabled (signatureAlgorithm))
				return false;

			options = options.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;

			// first check the body hash (if that's invalid, then the entire signature is invalid)
			if (!VerifyBodyHash (options, message, signatureAlgorithm, bodyAlgorithm, maxLength, bh))
				return false;

			if (doAsync)
				key = await PublicKeyLocator.LocatePublicKeyAsync (q, d, s, cancellationToken).ConfigureAwait (false);
			else
				key = PublicKeyLocator.LocatePublicKey (q, d, s, cancellationToken);

			if ((key is RsaKeyParameters rsa) && rsa.Modulus.BitLength < MinimumRsaKeyLength)
				return false;

			return VerifySignature (options, message, arcSignature, signatureAlgorithm, key, headers, headerAlgorithm, b);
		}

		async Task<bool> VerifyArcSealAsync (FormatOptions options, ArcHeaderSet[] sets, int i, bool doAsync, CancellationToken cancellationToken)
		{
			DkimSignatureAlgorithm algorithm;
			AsymmetricKeyParameter key;
			string d, s, q, b;

			ValidateArcSealParameters (sets[i].ArcSealParameters, out algorithm, out d, out s, out q, out b);

			if (!IsEnabled (algorithm))
				return false;

			if (doAsync)
				key = await PublicKeyLocator.LocatePublicKeyAsync (q, d, s, cancellationToken).ConfigureAwait (false);
			else
				key = PublicKeyLocator.LocatePublicKey (q, d, s, cancellationToken);

			if ((key is RsaKeyParameters rsa) && rsa.Modulus.BitLength < MinimumRsaKeyLength)
				return false;

			options = options.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;

			using (var stream = new DkimSignatureStream (CreateVerifyContext (algorithm, key))) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (options.CreateNewLineFilter ());

					for (int j = 0; j < i; j++) {
						WriteHeaderRelaxed (options, filtered, sets[j].ArcAuthenticationResult, false);
						WriteHeaderRelaxed (options, filtered, sets[j].ArcMessageSignature, false);
						WriteHeaderRelaxed (options, filtered, sets[j].ArcSeal, false);
					}

					WriteHeaderRelaxed (options, filtered, sets[i].ArcAuthenticationResult, false);
					WriteHeaderRelaxed (options, filtered, sets[i].ArcMessageSignature, false);

					// now include the ARC-Seal header that we are verifying,
					// but only after removing the "b=" signature value.
					var seal = GetSignedSignatureHeader (sets[i].ArcSeal);

					WriteHeaderRelaxed (options, filtered, seal, true);

					filtered.Flush (cancellationToken);
				}

				return stream.VerifySignature (b);
			}
		}

		internal static ArcSignatureValidationResult GetArcHeaderSets (MimeMessage message, bool throwOnError, out ArcHeaderSet[] sets, out int count, out ArcValidationErrors errors)
		{
			ArcHeaderSet set;

			errors = ArcValidationErrors.None;
			sets = new ArcHeaderSet[50];
			count = 0;

			for (int i = 0; i < message.Headers.Count; i++) {
				Dictionary<string, string> parameters = null;
				var header = message.Headers[i];
				int instance = 0;
				string value;

				switch (header.Id) {
				case HeaderId.ArcAuthenticationResults:
					if (!AuthenticationResults.TryParse (header.RawValue, out AuthenticationResults authres)) {
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

				if (!set.ArcSealParameters.TryGetValue ("cv", out string cv)) {
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

		async Task<ArcValidationResult> VerifyAsync (FormatOptions options, MimeMessage message, bool doAsync, CancellationToken cancellationToken)
		{
			const ArcValidationErrors ArcSealCvParamErrors = ArcValidationErrors.InvalidArcSealChainValidationValue | ArcValidationErrors.MissingArcSealChainValidationValue;

			if (options == null)
				throw new ArgumentNullException (nameof (options));

			if (message == null)
				throw new ArgumentNullException (nameof (message));

			var result = new ArcValidationResult ();

			switch (GetArcHeaderSets (message, false, out ArcHeaderSet[] sets, out int count, out var errors)) {
			case ArcSignatureValidationResult.None: return result;
			case ArcSignatureValidationResult.Fail:
				result.Chain = ArcSignatureValidationResult.Fail;
				result.ChainErrors = errors;

				// If the only error(s) are invalid or missing 'cv' values, ignore the errors for now.
				if ((errors & ~ArcSealCvParamErrors) == 0)
					break;

				return result;
			default:
				result.Chain = ArcSignatureValidationResult.Pass;
				break;
			}

			int newest = count - 1;

			result.Seals = new ArcHeaderValidationResult[count];

			// validate the most recent Arc-Message-Signature
			try {
				var parameters = sets[newest].ArcMessageSignatureParameters;
				var header = sets[newest].ArcMessageSignature;

				result.MessageSignature = new ArcHeaderValidationResult (header);

				if (await VerifyArcMessageSignatureAsync (options, message, header, parameters, doAsync, cancellationToken).ConfigureAwait (false)) {
					result.MessageSignature.Signature = ArcSignatureValidationResult.Pass;
				} else {
					result.MessageSignature.Signature = ArcSignatureValidationResult.Fail;
					result.ChainErrors |= ArcValidationErrors.MessageSignatureValidationFailed;
					result.Chain = ArcSignatureValidationResult.Fail;
				}
			} catch {
				result.MessageSignature.Signature = ArcSignatureValidationResult.Fail;
				result.ChainErrors |= ArcValidationErrors.MessageSignatureValidationFailed;
				result.Chain = ArcSignatureValidationResult.Fail;
			}

			// validate all Arc-Seals starting with the most recent and proceeding to the oldest
			for (int i = newest; i >= 0; i--) {
				result.Seals[i] = new ArcHeaderValidationResult (sets[i].ArcSeal);

				try {
					if (await VerifyArcSealAsync (options, sets, i, doAsync, cancellationToken).ConfigureAwait (false)) {
						result.Seals[i].Signature = ArcSignatureValidationResult.Pass;
					} else {
						result.Seals[i].Signature = ArcSignatureValidationResult.Fail;
						result.ChainErrors |= ArcValidationErrors.SealValidationFailed;
						result.Chain = ArcSignatureValidationResult.Fail;
					}
				} catch {
					result.Seals[i].Signature = ArcSignatureValidationResult.Fail;
					result.ChainErrors |= ArcValidationErrors.SealValidationFailed;
					result.Chain = ArcSignatureValidationResult.Fail;
				}
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
		/// <param name="options">The formatting options.</param>
		/// <param name="message">The message to verify.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="message"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public ArcValidationResult Verify (FormatOptions options, MimeMessage message, CancellationToken cancellationToken = default)
		{
			return VerifyAsync (options, message, false, cancellationToken).GetAwaiter ().GetResult ();
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
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="message"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public Task<ArcValidationResult> VerifyAsync (FormatOptions options, MimeMessage message, CancellationToken cancellationToken = default)
		{
			return VerifyAsync (options, message, true, cancellationToken);
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
		/// <paramref name="message"/> is <c>null</c>.
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
		/// <paramref name="message"/> is <c>null</c>.
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

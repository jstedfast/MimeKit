//
// ArcVerifier.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2019 Xamarin Inc. (www.xamarin.com)
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

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;

using MimeKit;
using MimeKit.IO;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An ARC validation result.
	/// </summary>
	/// <remarks>
	/// An ARC validation result.
	/// </remarks>
	public enum ArcValidationResult
	{
		/// <summary>
		/// No validation was performed.
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
	/// An ARC verifier.
	/// </summary>
	/// <remarks>
	/// Validates Authenticated Received Chains.
	/// </remarks>
	public class ArcVerifier
	{
		readonly IDkimPublicKeyLocator publicKeyLocator;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.ArcVerifier"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="ArcVerifier"/>.
		/// </remarks>
		/// <param name="publicKeyLocator">The public key locator.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="publicKeyLocator"/> is <c>null</c>.
		/// </exception>
		public ArcVerifier (IDkimPublicKeyLocator publicKeyLocator)
		{
			if (publicKeyLocator == null)
				throw new ArgumentNullException (nameof (publicKeyLocator));

			this.publicKeyLocator = publicKeyLocator;
		}

		class ArcHeaderSet
		{
			public Dictionary<string, string> ArcAuthenticationResultParameters { get; private set; }
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

					ArcAuthenticationResultParameters = parameters;
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

		static void ValidateArcMessageSignatureParameters (IDictionary<string, string> parameters, out DkimSignatureAlgorithm algorithm, out DkimCanonicalizationAlgorithm headerAlgorithm,
			out DkimCanonicalizationAlgorithm bodyAlgorithm, out string d, out string s, out string q, out string[] headers, out string bh, out string b, out int maxLength)
		{
			DkimVerifier.ValidateCommonSignatureParameters ("ARC-Message-Signature", parameters, out algorithm, out headerAlgorithm, out bodyAlgorithm, out d, out s, out q, out headers, out bh, out b, out maxLength);
		}

		static void ValidateArcSealParameters (IDictionary<string, string> parameters, out DkimSignatureAlgorithm algorithm, out string d, out string s, out string q, out string b)
		{
			DkimVerifier.ValidateCommonParameters ("ARC-Seal", parameters, out algorithm, out d, out s, out q, out b);

			if (parameters.TryGetValue ("h", out string h))
				throw new FormatException (string.Format ("Malformed ARC-Seal header: the 'h' parameter tag is not allowed."));
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

			if (doAsync)
				key = await publicKeyLocator.LocatePublicKeyAsync (q, d, s, cancellationToken).ConfigureAwait (false);
			else
				key = publicKeyLocator.LocatePublicKey (q, d, s, cancellationToken);

			if (!(key is RsaKeyParameters rsa) || rsa.Modulus.BitLength < 1024)
				return false;

			options = options.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;

			// first check the body hash (if that's invalid, then the entire signature is invalid)
			var hash = Convert.ToBase64String (message.HashBody (options, signatureAlgorithm, bodyAlgorithm, maxLength));

			if (hash != bh)
				return false;

			using (var stream = new DkimSignatureStream (DkimVerifier.GetDigestSigner (signatureAlgorithm, key))) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (options.CreateNewLineFilter ());

					DkimVerifier.WriteHeaders (options, message, headers, headerAlgorithm, filtered);

					// now include the ARC-Message-Signature header that we are verifying,
					// but only after removing the "b=" signature value.
					var header = DkimVerifier.GetSignedSignatureHeader (arcSignature);

					switch (headerAlgorithm) {
					case DkimCanonicalizationAlgorithm.Relaxed:
						DkimVerifier.WriteHeaderRelaxed (options, filtered, header, true);
						break;
					default:
						DkimVerifier.WriteHeaderSimple (options, filtered, header, true);
						break;
					}

					filtered.Flush ();
				}

				return stream.VerifySignature (b);
			}
		}

		async Task<bool> VerifyArcSealAsync (FormatOptions options, ArcHeaderSet[] sets, int i, bool doAsync, CancellationToken cancellationToken)
		{
			DkimSignatureAlgorithm algorithm;
			AsymmetricKeyParameter key;
			string d, s, q, b;

			ValidateArcSealParameters (sets[i].ArcSealParameters, out algorithm, out d, out s, out q, out b);

			if (doAsync)
				key = await publicKeyLocator.LocatePublicKeyAsync (q, d, s, cancellationToken).ConfigureAwait (false);
			else
				key = publicKeyLocator.LocatePublicKey (q, d, s, cancellationToken);

			if (!(key is RsaKeyParameters rsa) || rsa.Modulus.BitLength < 1024)
				return false;

			options = options.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;

			using (var stream = new DkimSignatureStream (DkimVerifier.GetDigestSigner (algorithm, key))) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (options.CreateNewLineFilter ());

					for (int j = 0; j < i; j++) {
						DkimVerifier.WriteHeaderRelaxed (options, filtered, sets[j].ArcAuthenticationResult, false);
						DkimVerifier.WriteHeaderRelaxed (options, filtered, sets[j].ArcMessageSignature, false);
						DkimVerifier.WriteHeaderRelaxed (options, filtered, sets[j].ArcSeal, false);
					}

					DkimVerifier.WriteHeaderRelaxed (options, filtered, sets[i].ArcAuthenticationResult, false);
					DkimVerifier.WriteHeaderRelaxed (options, filtered, sets[i].ArcMessageSignature, false);

					// now include the ARC-Seal header that we are verifying,
					// but only after removing the "b=" signature value.
					var seal = DkimVerifier.GetSignedSignatureHeader (sets[i].ArcSeal);

					DkimVerifier.WriteHeaderRelaxed (options, filtered, seal, true);

					filtered.Flush ();
				}

				return stream.VerifySignature (b);
			}
		}

		async Task<ArcValidationResult> VerifyAsync (FormatOptions options, MimeMessage message, bool doAsync, CancellationToken cancellationToken)
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			if (message == null)
				throw new ArgumentNullException(nameof(message));

			var sets = new ArcHeaderSet[50];
			ArcHeaderSet set;
			int newest = -1;

			for (int i = 0; i < message.Headers.Count; i++) {
				Dictionary<string, string> parameters = null;
				var header = message.Headers[i];
				string value;
				int inst;

				switch (header.Id) {
				case HeaderId.ArcAuthenticationResults:
				case HeaderId.ArcMessageSignature:
				case HeaderId.ArcSeal:
					try {
						parameters = DkimVerifier.ParseParameterTags (header.Id, header.Value);
					} catch {
						return ArcValidationResult.Fail;
					}
					break;
				}

				if (parameters == null)
					continue;

				if (!parameters.TryGetValue ("i", out value))
					return ArcValidationResult.Fail;

				if (!int.TryParse (value, NumberStyles.Integer, CultureInfo.InvariantCulture, out inst) || inst < 1 || inst > 50)
					return ArcValidationResult.Fail;

				inst--;

				set = sets[inst];
				if (set == null)
					sets[inst] = set = new ArcHeaderSet ();

				if (!set.Add (header, parameters))
					return ArcValidationResult.Fail;

				if (inst >= newest)
					newest = inst;
			}

			if (newest == -1) {
				// there are no ARC sets
				return ArcValidationResult.None;
			}

			// verify that all ARC sets are complete
			for (int i = 0; i <= newest; i++) {
				set = sets[i];

				if (sets == null || set.ArcAuthenticationResult == null || set.ArcMessageSignature == null || set.ArcSeal == null)
					return ArcValidationResult.Fail;

				if (!set.ArcSealParameters.TryGetValue ("cv", out string cv))
					return ArcValidationResult.Fail;

				// The "cv" value for all ARC-Seal header fields MUST NOT be
				// "fail". For ARC Sets with instance values > 1, the values
				// MUST be "pass". For the ARC Set with instance value = 1, the
				// value MUST be "none".
				if (!cv.Equals (i == 0 ? "none" : "pass", StringComparison.Ordinal))
					return ArcValidationResult.Fail;
			}

			// validate the most recent Arc-Message-Signature
			try {
				var parameters = sets[newest].ArcMessageSignatureParameters;
				var header = sets[newest].ArcMessageSignature;

				if (!await VerifyArcMessageSignatureAsync (options, message, header, parameters, doAsync, cancellationToken).ConfigureAwait (false))
					return ArcValidationResult.Fail;
			} catch {
				return ArcValidationResult.Fail;
			}

			// validate all Arc-Seals starting with the most recent and proceeding to the oldest
			for (int i = newest; i >= 0; i--) {
				try {
					if (!await VerifyArcSealAsync (options, sets, i, doAsync, cancellationToken).ConfigureAwait (false))
						return ArcValidationResult.Fail;
				} catch {
					return ArcValidationResult.Fail;
				}
			}

			return ArcValidationResult.Pass;
		}

		/// <summary>
		/// Verify the ARC signature chain.
		/// </summary>
		/// <remarks>
		/// Verifies the ARC signature chain.
		/// </remarks>
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
		public ArcValidationResult Verify (FormatOptions options, MimeMessage message, CancellationToken cancellationToken = default (CancellationToken))
		{
			return VerifyAsync (options, message, false, cancellationToken).GetAwaiter ().GetResult ();
		}

		/// <summary>
		/// Asynchronously verify the ARC signature chain.
		/// </summary>
		/// <remarks>
		/// Asynchronously verifies the ARC signature chain.
		/// </remarks>
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
		public Task<ArcValidationResult> VerifyAsync (FormatOptions options, MimeMessage message, CancellationToken cancellationToken = default (CancellationToken))
		{
			return VerifyAsync (options, message, true, cancellationToken);
		}

		/// <summary>
		/// Verify the ARC signature chain.
		/// </summary>
		/// <remarks>
		/// Verifies the ARC signature chain.
		/// </remarks>
		/// <returns>The ARC validation result.</returns>
		/// <param name="message">The message to verify.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="message"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public ArcValidationResult Verify (MimeMessage message, CancellationToken cancellationToken = default (CancellationToken))
		{
			return Verify (FormatOptions.Default, message, cancellationToken);
		}

		/// <summary>
		/// Asynchronously verify the ARC signature chain.
		/// </summary>
		/// <remarks>
		/// Asynchronously verifies the ARC signature chain.
		/// </remarks>
		/// <returns>The ARC validation result.</returns>
		/// <param name="message">The message to verify.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="message"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public Task<ArcValidationResult> VerifyAsync (MimeMessage message, CancellationToken cancellationToken = default (CancellationToken))
		{
			return VerifyAsync (FormatOptions.Default, message, cancellationToken);
		}
	}
}

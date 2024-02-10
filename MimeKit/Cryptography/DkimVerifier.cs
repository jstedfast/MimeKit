//
// DkimVerifier.cs
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
using System.Threading.Tasks;
using System.Collections.Generic;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

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
		/// <param name="publicKeyLocator">The public key locator.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="publicKeyLocator"/> is <c>null</c>.
		/// </exception>
		public DkimVerifier (IDkimPublicKeyLocator publicKeyLocator) : base (publicKeyLocator)
		{
		}

		static void ValidateDkimSignatureParameters (Dictionary<string, string> parameters, out DkimSignatureAlgorithm algorithm, out DkimCanonicalizationAlgorithm headerAlgorithm,
			out DkimCanonicalizationAlgorithm bodyAlgorithm, out string d, out string s, out string q, out string[] headers, out string bh, out string b, out int maxLength)
		{
			bool containsFrom = false;

			if (!parameters.TryGetValue ("v", out string v))
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

			if (parameters.TryGetValue ("i", out string id)) {
				int at;

				if ((at = id.LastIndexOf ('@')) == -1)
					throw new FormatException ("Malformed DKIM-Signature header: no @ in the AUID value.");

				var ident = id.AsSpan (at + 1);

				if (!ident.Equals (d.AsSpan (), StringComparison.OrdinalIgnoreCase) && !ident.EndsWith (("." + d).AsSpan (), StringComparison.OrdinalIgnoreCase))
					throw new FormatException ("Invalid DKIM-Signature header: the domain in the AUID does not match the domain parameter.");
			}
		}

		async Task<bool> VerifyAsync (FormatOptions options, MimeMessage message, Header dkimSignature, bool doAsync, CancellationToken cancellationToken)
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
			AsymmetricKeyParameter key;
			string d, s, q, bh, b;
			string[] headers;
			int maxLength;

			ValidateDkimSignatureParameters (parameters, out signatureAlgorithm, out headerAlgorithm, out bodyAlgorithm,
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

			return VerifySignature (options, message, dkimSignature, signatureAlgorithm, key, headers, headerAlgorithm, b);
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
		/// <returns><c>true</c> if the DKIM-Signature is valid; otherwise, <c>false</c>.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="message">The message to verify.</param>
		/// <param name="dkimSignature">The DKIM-Signature header.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="message"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="dkimSignature"/> is <c>null</c>.</para>
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
			return VerifyAsync (options, message, dkimSignature, false, cancellationToken).GetAwaiter ().GetResult ();
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
		/// <returns><c>true</c> if the DKIM-Signature is valid; otherwise, <c>false</c>.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="message">The message to verify.</param>
		/// <param name="dkimSignature">The DKIM-Signature header.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="message"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="dkimSignature"/> is <c>null</c>.</para>
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
		public Task<bool> VerifyAsync (FormatOptions options, MimeMessage message, Header dkimSignature, CancellationToken cancellationToken = default)
		{
			return VerifyAsync (options, message, dkimSignature, true, cancellationToken);
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
		/// <returns><c>true</c> if the DKIM-Signature is valid; otherwise, <c>false</c>.</returns>
		/// <param name="message">The message to verify.</param>
		/// <param name="dkimSignature">The DKIM-Signature header.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="message"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="dkimSignature"/> is <c>null</c>.</para>
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
		/// <returns><c>true</c> if the DKIM-Signature is valid; otherwise, <c>false</c>.</returns>
		/// <param name="message">The message to verify.</param>
		/// <param name="dkimSignature">The DKIM-Signature header.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="message"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="dkimSignature"/> is <c>null</c>.</para>
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

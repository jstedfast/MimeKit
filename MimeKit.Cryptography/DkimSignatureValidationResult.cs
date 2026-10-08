//
// DkimSignatureValidationResult.cs
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// The result of verifying a DKIM-Signature header.
	/// </summary>
	/// <remarks>
	/// The result of verifying a DKIM-Signature header, as returned by
	/// <see cref="DkimVerifier.Verify(FormatOptions, MimeMessage, Header, System.Threading.CancellationToken)"/>.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
	/// </example>
	public sealed class DkimSignatureValidationResult
	{
		internal DkimSignatureValidationResult (Header header)
		{
			Header = header;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="DkimSignatureValidationResult"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="DkimSignatureValidationResult"/> for a DKIM-Signature that was verified by
		/// some other means, such as an upstream MTA that recorded its results in an Authentication-Results header.</para>
		/// <para>This is useful for passing DKIM results to
		/// <see cref="DmarcVerifier.Verify(MimeMessage, System.Collections.Generic.IEnumerable{DkimSignatureValidationResult}, SpfCheckResult, System.Threading.CancellationToken)"/>.</para>
		/// </remarks>
		/// <param name="status">The verification status.</param>
		/// <param name="domain">The signing domain (the value of the signature's <c>d=</c> tag).</param>
		/// <param name="selector">The selector (the value of the signature's <c>s=</c> tag).</param>
		/// <param name="header">The DKIM-Signature header, if available.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="status"/> is not a valid <see cref="DkimSignatureStatus"/>.
		/// </exception>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="domain"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="selector"/> is <see langword="null"/>.</para>
		/// </exception>
		public DkimSignatureValidationResult (DkimSignatureStatus status, string domain, string selector, Header? header = null)
		{
			if (status < DkimSignatureStatus.Pass || status > DkimSignatureStatus.PermError)
				throw new ArgumentOutOfRangeException (nameof (status));

			if (domain is null)
				throw new ArgumentNullException (nameof (domain));

			if (selector is null)
				throw new ArgumentNullException (nameof (selector));

			Selector = selector;
			Domain = domain;
			Status = status;
			Header = header;
		}

		/// <summary>
		/// Get the DKIM-Signature header that was verified.
		/// </summary>
		/// <remarks>
		/// Gets the DKIM-Signature header that was verified.
		/// </remarks>
		/// <value>The DKIM-Signature header or <see langword="null"/> if the result was created without one.</value>
		public Header? Header {
			get;
		}

		/// <summary>
		/// Get the verification status.
		/// </summary>
		/// <remarks>
		/// Gets the verification status.
		/// </remarks>
		/// <value>The verification status.</value>
		public DkimSignatureStatus Status {
			get; internal set;
		}

		/// <summary>
		/// Get the signing domain.
		/// </summary>
		/// <remarks>
		/// Gets the value of the signature's <c>d=</c> tag, if it could be parsed.
		/// </remarks>
		/// <value>The signing domain or <see langword="null"/> if it could not be determined.</value>
		public string? Domain {
			get; internal set;
		}

		/// <summary>
		/// Get the selector.
		/// </summary>
		/// <remarks>
		/// Gets the value of the signature's <c>s=</c> tag, if it could be parsed.
		/// </remarks>
		/// <value>The selector or <see langword="null"/> if it could not be determined.</value>
		public string? Selector {
			get; internal set;
		}

		/// <summary>
		/// Get the agent or user identifier.
		/// </summary>
		/// <remarks>
		/// Gets the value of the signature's <c>i=</c> tag, if present.
		/// </remarks>
		/// <value>The agent or user identifier or <see langword="null"/> if it was not specified.</value>
		public string? AgentOrUserIdentifier {
			get; internal set;
		}

		/// <summary>
		/// Get the signature algorithm.
		/// </summary>
		/// <remarks>
		/// Gets the signature algorithm specified by the signature's <c>a=</c> tag, if it could be parsed.
		/// </remarks>
		/// <value>The signature algorithm or <see langword="null"/> if it could not be determined.</value>
		public DkimSignatureAlgorithm? SignatureAlgorithm {
			get; internal set;
		}

		/// <summary>
		/// Get a description of why the signature did not pass.
		/// </summary>
		/// <remarks>
		/// Gets a short, human-readable description of why the signature did not pass, suitable for use as
		/// the <c>reason</c> in an Authentication-Results header.
		/// </remarks>
		/// <value>The reason or <see langword="null"/> if the signature passed.</value>
		public string? Reason {
			get; internal set;
		}

		/// <summary>
		/// Get the exception that caused the verification to fail, if any.
		/// </summary>
		/// <remarks>
		/// Gets the exception that caused the verification to fail, such as a <see cref="FormatException"/>
		/// for a malformed DKIM-Signature header or the exception thrown by the <see cref="IDnsResolver"/>.
		/// </remarks>
		/// <value>The exception or <see langword="null"/>.</value>
		public Exception? Exception {
			get; internal set;
		}

		internal string? Signature {
			get; set;
		}

		static string GetStatusValue (DkimSignatureStatus status)
		{
			switch (status) {
			case DkimSignatureStatus.Pass: return "pass";
			case DkimSignatureStatus.Fail: return "fail";
			case DkimSignatureStatus.Policy: return "policy";
			case DkimSignatureStatus.TempError: return "temperror";
			default: return "permerror";
			}
		}

		internal static string GetAlgorithmName (DkimSignatureAlgorithm algorithm)
		{
			switch (algorithm) {
			case DkimSignatureAlgorithm.Ed25519Sha256: return "ed25519-sha256";
			case DkimSignatureAlgorithm.RsaSha1: return "rsa-sha1";
			default: return "rsa-sha256";
			}
		}

		/// <summary>
		/// Create an <see cref="AuthenticationMethodResult"/> for this result.
		/// </summary>
		/// <remarks>
		/// <para>Creates a <c>dkim</c> <see cref="AuthenticationMethodResult"/> suitable for adding to an
		/// Authentication-Results header, as described in
		/// <a href="https://www.rfc-editor.org/rfc/rfc8601#section-2.7.1">RFC 8601, Section 2.7.1</a>.</para>
		/// <para>The result includes the <c>header.d</c>, <c>header.i</c>, <c>header.s</c>, <c>header.a</c>
		/// and <c>header.b</c> properties when they are known. The <c>header.b</c> property contains the first
		/// 8 characters of the signature, as described in
		/// <a href="https://www.rfc-editor.org/rfc/rfc6008">RFC 6008</a>.</para>
		/// </remarks>
		/// <returns>The authentication method result.</returns>
		public AuthenticationMethodResult ToAuthenticationMethodResult ()
		{
			var result = new AuthenticationMethodResult ("dkim", GetStatusValue (Status)) {
				Reason = Status != DkimSignatureStatus.Pass ? Reason : null
			};

			if (Domain != null)
				result.Properties.Add (new AuthenticationMethodProperty ("header", "d", Domain));

			if (AgentOrUserIdentifier != null)
				result.Properties.Add (new AuthenticationMethodProperty ("header", "i", AgentOrUserIdentifier));

			if (Selector != null)
				result.Properties.Add (new AuthenticationMethodProperty ("header", "s", Selector));

			if (SignatureAlgorithm.HasValue)
				result.Properties.Add (new AuthenticationMethodProperty ("header", "a", GetAlgorithmName (SignatureAlgorithm.Value)));

			if (!string.IsNullOrEmpty (Signature))
				result.Properties.Add (new AuthenticationMethodProperty ("header", "b", Signature!.Length > 8 ? Signature.Substring (0, 8) : Signature));

			return result;
		}
	}
}
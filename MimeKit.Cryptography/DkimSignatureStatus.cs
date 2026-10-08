//
// DkimSignatureStatus.cs
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// The result of verifying a DKIM-Signature header.
	/// </summary>
	/// <remarks>
	/// <para>The result of verifying a DKIM-Signature header.</para>
	/// <para>The values correspond to the DKIM result codes defined in
	/// <a href="https://www.rfc-editor.org/rfc/rfc8601#section-2.7.1">RFC 8601, Section 2.7.1</a>
	/// for use in the Authentication-Results header.</para>
	/// </remarks>
	/// <seealso cref="DkimSignatureValidationResult"/>
	public enum DkimSignatureStatus
	{
		/// <summary>
		/// The signature was verified successfully.
		/// </summary>
		Pass,

		/// <summary>
		/// The signature could not be verified because the body hash or the header signature did not match.
		/// </summary>
		Fail,

		/// <summary>
		/// The signature was syntactically valid, but was rejected by local policy, for example because the
		/// signature algorithm is disabled or the public key is shorter than the
		/// <see cref="DkimVerifierBase.MinimumRsaKeyLength"/>.
		/// </summary>
		Policy,

		/// <summary>
		/// The signature could not be verified due to a temporary error, such as a DNS failure while looking
		/// up the public key. A later attempt may produce a final result.
		/// </summary>
		TempError,

		/// <summary>
		/// The signature could not be verified due to a permanent error, such as a malformed DKIM-Signature
		/// header, an expired signature or a missing, revoked or invalid public key.
		/// </summary>
		PermError
	}
}
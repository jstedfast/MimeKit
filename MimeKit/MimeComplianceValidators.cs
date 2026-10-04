//
// MimeComplianceValidators.cs
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

namespace MimeKit {
	/// <summary>
	/// The set of MIME compliance validators that a <see cref="MimeReader"/> may run.
	/// </summary>
	/// <remarks>
	/// <para>A validator performs a deeper inspection of a particular kind of content than the
	/// structural checks that the <see cref="MimeReader"/> always performs, such as checking that the
	/// content of a base64-encoded part is actually valid base64 or that an address header is
	/// syntactically valid.</para>
	/// <para>These values are combined to form <see cref="MimeComplianceOptions.EnabledValidators"/>,
	/// which allows individual validators to be switched off, for example if one of them proves to be
	/// too expensive for, or misbehaves on, the messages that a particular deployment sees.</para>
	/// </remarks>
	[Flags]
	public enum MimeComplianceValidators
	{
		/// <summary>
		/// No validators.
		/// </summary>
		None            = 0,

		/// <summary>
		/// The validator for content that uses the base64 Content-Transfer-Encoding.
		/// </summary>
		Base64          = 1 << 0,

		/// <summary>
		/// The validator for content that uses the quoted-printable Content-Transfer-Encoding.
		/// </summary>
		QuotedPrintable = 1 << 1,

		/// <summary>
		/// The validator for content that uses the uuencode Content-Transfer-Encoding.
		/// </summary>
		UUEncode        = 1 << 2,

		/// <summary>
		/// The validator for the values of address headers such as From, To and Cc.
		/// </summary>
		Address         = 1 << 3,

		/// <summary>
		/// All validators.
		/// </summary>
		All             = Base64 | QuotedPrintable | UUEncode | Address
	}
}

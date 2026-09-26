//
// MimeComplianceSeverity.cs
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

namespace MimeKit {
	/// <summary>
	/// An enumeration of MIME compliance violation severities.
	/// </summary>
	/// <remarks>
	/// <para>Indicates how much practical harm a <see cref="MimeComplianceViolation"/> is likely to
	/// cause, allowing an application to triage compliance issues rather than treating them all
	/// equally.</para>
	/// <para>The values are ordered from least to most severe, so they may be compared using the
	/// relational operators. For example, <c>issue.Severity &gt;= MimeComplianceSeverity.Major</c>
	/// will match everything except <see cref="Minor"/> violations.</para>
	/// <note type="note">Severity depends on how the message is being used. A few violations are
	/// requirements of the channel a message travels over rather than of the message itself, and are
	/// therefore rated lower in <see cref="MimeComplianceContext.Storage"/> than in
	/// <see cref="MimeComplianceContext.Transport"/>. See <see cref="MimeComplianceContext"/> for
	/// details.</note>
	/// </remarks>
	public enum MimeComplianceSeverity
	{
		/// <summary>
		/// The violation is technically non-compliant but is tolerated by nearly all software.
		/// </summary>
		/// <remarks>
		/// These violations are unlikely to cause any difference in how the message is interpreted.
		/// Applications that only care about interoperability can generally ignore them.
		/// </remarks>
		Minor = 0,

		/// <summary>
		/// The violation is likely to cause interoperability problems.
		/// </summary>
		/// <remarks>
		/// These violations introduce ambiguity that different MIME parser implementations may
		/// resolve differently, or indicate content that may fail to decode correctly, which can
		/// result in the message being rendered incorrectly or in content being lost.
		/// </remarks>
		Major = 1,

		/// <summary>
		/// The violation is a known vector for content smuggling.
		/// </summary>
		/// <remarks>
		/// <para>These violations allow a message to be interpreted differently by different MIME
		/// parser implementations in ways that have been used to slip malicious content past content
		/// scanners and other security filters. A scanner and an end-user's mail client disagreeing
		/// about the structure or content of a message is precisely the condition such attacks rely
		/// on.</para>
		/// <note type="warning">A critical violation does not mean the message is malicious, only
		/// that it exhibits a property that malicious messages are known to abuse. Messages
		/// containing critical violations warrant closer scrutiny.</note>
		/// </remarks>
		Critical = 2
	}
}

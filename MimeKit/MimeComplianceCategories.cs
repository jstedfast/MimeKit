//
// MimeComplianceCategories.cs
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
	/// An enumeration of the kinds of harm that a MIME compliance violation may cause.
	/// </summary>
	/// <remarks>
	/// <para>Where <see cref="MimeComplianceSeverity"/> rates <i>how much</i> harm a violation is
	/// likely to cause, the category describes <i>what kind</i> of harm it is. The two are
	/// independent: a violation may be harmless to one consumer and serious to another depending on
	/// what that consumer is trying to do.</para>
	/// <para>A violation may belong to more than one category, so this is a bit field. For example,
	/// <see cref="MimeComplianceViolation.ObsoleteBase64Comment"/> is both
	/// <see cref="DataLoss"/> (the comment text is absorbed as base64 data, silently corrupting
	/// everything that follows it) and <see cref="Security"/> (a decoder that skips the comment and
	/// one that does not will derive different content from the same bytes).</para>
	/// <para>Categories are intended for filtering. A program that lints outgoing mail cares about
	/// <see cref="Interoperability"/>, a mail gateway cares about <see cref="Security"/>, and an
	/// archiver cares about <see cref="DataLoss"/>. Each can select the issues relevant to it
	/// without having to enumerate individual violations.</para>
	/// <para>Every category describes a <i>kind of harm</i>. There is deliberately no category for
	/// harmless deviations, because <see cref="MimeReader"/> only reports deviations that are worth
	/// reporting, and "safe to ignore" is already expressed by
	/// <see cref="MimeComplianceSeverity.Minor"/>.</para>
	/// <note type="note">These ratings reflect practical experience with mail software found in the
	/// wild rather than anything stated by the specifications themselves.</note>
	/// </remarks>
	/// <example>
	/// <code language="c#">
	/// foreach (var issue in logger.Issues) {
	///     if ((issue.Categories &amp; MimeComplianceCategories.Security) != 0)
	///         Console.WriteLine ("Possible evasion attempt: {0}", issue);
	/// }
	/// </code>
	/// </example>
	[Flags]
	public enum MimeComplianceCategories
	{
		/// <summary>
		/// No category.
		/// </summary>
		/// <remarks>
		/// The violation has not been categorized. Every <see cref="MimeComplianceViolation"/> maps
		/// to at least one real category, so this exists only so that <c>default</c> and the result
		/// of masking are meaningful.
		/// </remarks>
		None = 0,

		/// <summary>
		/// Other software may reject, mangle, or render the message differently.
		/// </summary>
		/// <remarks>
		/// The message is likely to be handled inconsistently by mail software in the wild. It may
		/// be rejected outright by a strict server, rendered incorrectly, or silently repaired in a
		/// way that changes it. This is the category to filter on when checking a message that is
		/// about to be sent.
		/// </remarks>
		Interoperability = 1 << 0,

		/// <summary>
		/// Content may be silently lost or corrupted when the message is decoded.
		/// </summary>
		/// <remarks>
		/// Decoding the message does not round-trip: bytes are dropped, altered, or misinterpreted,
		/// usually without any error being raised. This is the category to filter on when archiving
		/// or indexing messages, where a silent corruption is permanent.
		/// </remarks>
		DataLoss = 1 << 1,

		/// <summary>
		/// The construct can be used to make two pieces of software disagree.
		/// </summary>
		/// <remarks>
		/// <para>The message is ambiguous in a way that different implementations resolve
		/// differently, which is the basis of MIME content smuggling. An attacker can craft a
		/// message such that a content scanner sees one thing and the recipient's mail client sees
		/// another, allowing malicious content past the scanner.</para>
		/// <para>This is the category to filter on in a mail gateway, a spam filter, or an antivirus
		/// scanner. Note that a violation in this category is not evidence of an attack by itself,
		/// as broken mail software produces the same constructs by accident.</para>
		/// </remarks>
		Security = 1 << 2
	}
}

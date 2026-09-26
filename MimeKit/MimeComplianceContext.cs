//
// MimeComplianceContext.cs
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
	/// An enumeration of the contexts that a MIME message may be used in.
	/// </summary>
	/// <remarks>
	/// <para>A handful of MIME compliance violations are requirements of the channel that a message
	/// travels over rather than of the message itself. Such violations are serious when a message is
	/// transmitted over the network but routine and harmless when a message is read from local disk
	/// storage.</para>
	/// <para>The affected violations are
	/// <see cref="MimeComplianceViolation.BareLinefeedInHeader"/>,
	/// <see cref="MimeComplianceViolation.BareLinefeedInBody"/> and
	/// <see cref="MimeComplianceViolation.InvalidWrapping"/>. Every other violation is rated the
	/// same in both contexts.</para>
	/// <note type="note">This does not change which violations are reported, only how severe they
	/// are considered to be. Use
	/// <see cref="MimeComplianceIssue.GetSeverity(MimeComplianceViolation,MimeComplianceContext)"/>
	/// to rate a violation for a particular context.</note>
	/// </remarks>
	public enum MimeComplianceContext
	{
		/// <summary>
		/// The message is being transmitted over the network.
		/// </summary>
		/// <remarks>
		/// <para>The message is being sent or received via a protocol such as SMTP, POP3 or IMAP,
		/// where the Internet Message Format requirements apply in full. Lines must be terminated
		/// with a &lt;CR&gt;&lt;LF&gt; sequence and must not exceed the SMTP line length limit.</para>
		/// <para>This is the stricter of the two contexts and is the one assumed by
		/// <see cref="MimeComplianceIssue.Severity"/>.</para>
		/// </remarks>
		Transport,

		/// <summary>
		/// The message is being read from local storage.
		/// </summary>
		/// <remarks>
		/// The message is being read from a local message store such as an mbox file or a Maildir,
		/// where the conventions of the host operating system apply. Most notably, messages stored
		/// on UNIX systems routinely use a bare linefeed to terminate lines, which is expected
		/// rather than exceptional.
		/// </remarks>
		Storage
	}
}

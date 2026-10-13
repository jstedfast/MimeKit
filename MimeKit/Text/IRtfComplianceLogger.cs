//
// IRtfComplianceLogger.cs
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

namespace MimeKit.Text {
	/// <summary>
	/// An interface for recording RTF compliance violations.
	/// </summary>
	/// <remarks>
	/// <para>Implementations of this interface are intended to capture and record information about
	/// RTF compliance issues detected while converting an RTF document. This can be used for
	/// diagnostics, auditing, or reporting purposes in systems that process RTF content.</para>
	/// <para>No limit is placed by default on how many issues a single document may produce. When the
	/// documents being converted are untrusted, configure a per-violation limit on the converter to
	/// bound the report. An implementation that retains issues rather than summarizing them should
	/// impose its own limit as well, since it cannot assume that every caller configures one.</para>
	/// </remarks>
	public interface IRtfComplianceLogger
	{
		/// <summary>
		/// Log an RTF compliance violation.
		/// </summary>
		/// <remarks>
		/// <para>Logs an RTF compliance violation.</para>
		/// <para>This is called during conversion, so an implementation that does significant work here
		/// will slow conversion down, and one that throws will abort it. Throwing is the supported way to
		/// stop converting at the first violation.</para>
		/// </remarks>
		/// <param name="issue">The RTF compliance issue that was detected.</param>
		void Log (in RtfComplianceIssue issue);
	}
}
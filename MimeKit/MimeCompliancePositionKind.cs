//
// MimeCompliancePositionKind.cs
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
	/// An enumeration of the things that the position of a MIME compliance issue may refer to.
	/// </summary>
	/// <remarks>
	/// <para>Not every violation can be pinned to a single byte. Some are a property of an entire
	/// header or body part rather than of one position within it, and others could only be narrowed
	/// down further by re-scanning input that the parser has already moved past.</para>
	/// <para>Rather than pay that cost on every parse, <see cref="MimeReader"/> reports the position
	/// it already knows and records what that position refers to. A tool that renders diagnostics
	/// can then narrow the position down itself, but only for the issues it actually displays.</para>
	/// </remarks>
	public enum MimeCompliancePositionKind
	{
		/// <summary>
		/// The position is that of the byte that triggered the violation.
		/// </summary>
		/// <remarks>
		/// <see cref="MimeComplianceIssue.LineNumber"/> and <see cref="MimeComplianceIssue.ColumnNumber"/>
		/// identify the offending byte itself, so no further searching is needed.
		/// </remarks>
		Exact,

		/// <summary>
		/// The position is the start of the line that the violation was found on.
		/// </summary>
		/// <remarks>
		/// The violation lies somewhere on this line, but its exact column was not determined.
		/// A tool that wants to point at the offending byte should search from this position to
		/// the end of the line.
		/// </remarks>
		LineStart,

		/// <summary>
		/// The position is the start of the header or body part that the violation was found in.
		/// </summary>
		/// <remarks>
		/// <para>The violation is either a property of the element as a whole, or lies somewhere
		/// within it at a position that was not determined.</para>
		/// <para>A tool that wants to point at the offending byte should search from this position
		/// to the end of the element. Note that a header may be folded over several lines, so the
		/// offending byte is not necessarily on <see cref="MimeComplianceIssue.LineNumber"/>.</para>
		/// </remarks>
		ElementStart
	}
}

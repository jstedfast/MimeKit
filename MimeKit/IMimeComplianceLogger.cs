//
// IMimeComplianceLogger.cs
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
	/// An interface for recording MIME compliance violations.
	/// </summary>
	/// <remarks>
	/// Implementations of this interface are intended to capture and record information about MIME
	/// compliance issues detected during parsing. This can be used for diagnostics, auditing, or
	/// reporting purposes in systems that process MIME data.
	/// </remarks>
	public interface IMimeComplianceLogger
	{
		/// <summary>
		/// Log a MIME compliance violation.
		/// </summary>
		/// <remarks>
		/// Logs a MIME compliance violation.
		/// </remarks>
		/// <param name="violation">The specific MIME compliance violation that occurred.</param>
		/// <param name="streamOffset">The offset within the stream where the violation was found.</param>
		/// <param name="lineNumber">The line number within the MIME message where the violation was found.</param>
		/// <param name="columnNumber">The column number within the MIME message where the violation was found.</param>
		void Log (MimeComplianceViolation violation, long streamOffset, int lineNumber, int columnNumber = -1);
	}
}

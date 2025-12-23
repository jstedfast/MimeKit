//
// MimeComplianceViolationEventArgs.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2025 .NET Foundation and Contributors
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
	/// Provides data for events that report a MIME compliance violation detected during parsing or processing operations.
	/// </summary>
	/// <remarks>
	/// This event argument class supplies detailed information about the specific compliance violation, including its type,
	/// a descriptive message, and the exact location within the input stream where the violation occurred. This information
	/// can be used for logging, diagnostics, or to implement custom handling of MIME compliance issues.
	/// </remarks>
	public class MimeComplianceViolationEventArgs : EventArgs
	{
		/// <summary>
		/// Get the MIME compliance violation detected during message processing.
		/// </summary>
		/// <remarks>
		/// Gets the MIME compliance violation detected during message processing.
		/// </remarks>
		/// <value>The MIME compliance violation.</value>
		public MimeComplianceViolation Violation { get; private set; }

		/// <summary>
		/// Get the offset within the input stream where the violation was detected.
		/// </summary>
		/// <remarks>
		/// Gets the offset within the input stream where the violation was detected.
		/// </remarks>
		/// <value>The offset within the input stream where the violation was detected.</value>
		public long StreamOffset { get; private set; }

		/// <summary>
		/// Get the line number within the input stream where the violation was detected.
		/// </summary>
		/// <remarks>
		/// Gets the line number within the input stream where the violation was detected.
		/// </remarks>
		/// <value>The line number within the input stream where the violation was detected.</value>
		public int LineNumber { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeComplianceViolationEventArgs"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimeComplianceViolationEventArgs"/>.
		/// </remarks>
		/// <param name="violation">The MIME compliance violation.</param>
		/// <param name="streamOffset">The offset within the input stream where the violation was detected.</param>
		/// <param name="lineNumber">The line number within the input stream where the violation was detected.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="streamOffset"/> is less than <c>0</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="lineNumber"/> is less than <c>0</c>.</para>
		/// </exception>
		public MimeComplianceViolationEventArgs (MimeComplianceViolation violation, long streamOffset, int lineNumber)
		{
			if (streamOffset < 0)
				throw new ArgumentOutOfRangeException (nameof (streamOffset));

			if (lineNumber < 0)
				throw new ArgumentOutOfRangeException (nameof (lineNumber));

			Violation = violation;
			StreamOffset = streamOffset;
			LineNumber = lineNumber;
		}
	}
}

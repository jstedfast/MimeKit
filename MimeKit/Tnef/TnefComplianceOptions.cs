//
// TnefComplianceOptions.cs
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

namespace MimeKit.Tnef {
	/// <summary>
	/// Options that control how a <see cref="TnefReader"/> reports TNEF compliance violations.
	/// </summary>
	/// <remarks>
	/// These options only have an effect when a <see cref="TnefReader.ComplianceLogger"/> has been set. When no logger is set,
	/// no compliance checks are performed at all.
	/// </remarks>
	public class TnefComplianceOptions
	{
		int maxIssuesPerViolation;

		/// <summary>
		/// The default compliance options.
		/// </summary>
		/// <remarks>
		/// <para>A <see cref="TnefReader"/> starts out with a copy of these options. Changing them therefore affects
		/// every <see cref="TnefReader"/> that is created afterward, which makes this a convenient place to
		/// apply application-wide configuration at startup.</para>
		/// <note type="note">These options are not thread-safe. They should be configured once,
		/// before any <see cref="TnefReader"/> is created, and not changed afterward.</note>
		/// </remarks>
		public static readonly TnefComplianceOptions Default = new TnefComplianceOptions ();

		/// <summary>
		/// Initialize a new instance of the <see cref="TnefComplianceOptions"/> class.
		/// </summary>
		/// <remarks>
		/// By default, no limit is placed on the number of issues reported for each violation.
		/// </remarks>
		public TnefComplianceOptions ()
		{
		}

		/// <summary>
		/// Get or set the maximum number of times that each compliance violation may be reported.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the maximum number of times that each compliance violation may be
		/// reported while reading a single TNEF stream. The default is <c>0</c>, which means that no limit is applied.</para>
		/// <para>The number of issues a TNEF stream can produce is bounded only by its size, and the
		/// densest forms cost only a few bytes per issue. Setting a limit is recommended when
		/// reading untrusted streams.</para>
		/// <para>The limit is per violation rather than a single total, so that a flood of cheap,
		/// harmless violations cannot crowd out the ones that follow it.</para>
		/// <para>When some violation reaches the limit, a single
		/// <see cref="TnefComplianceViolation.TooManyComplianceIssues"/> issue is reported so that the
		/// report is never silently incomplete.</para>
		/// </remarks>
		/// <value>The maximum number of times that each violation may be reported, or <c>0</c> for no limit.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is negative.
		/// </exception>
		public int MaxIssuesPerViolation {
			get { return maxIssuesPerViolation; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException (nameof (value));

				maxIssuesPerViolation = value;
			}
		}

		/// <summary>
		/// Clones an instance of <see cref="TnefComplianceOptions"/>.
		/// </summary>
		/// <remarks>
		/// Clones a set of options, allowing you to change a specific option
		/// without requiring you to change the original.
		/// </remarks>
		/// <returns>An identical copy of the current instance.</returns>
		public TnefComplianceOptions Clone ()
		{
			return new TnefComplianceOptions {
				maxIssuesPerViolation = maxIssuesPerViolation
			};
		}
	}
}

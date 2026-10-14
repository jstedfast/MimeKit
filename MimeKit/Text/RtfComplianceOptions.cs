//
// RtfComplianceOptions.cs
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

namespace MimeKit.Text {
	/// <summary>
	/// Options that control how an <see cref="RtfToText"/> or <see cref="RtfToHtml"/> converter reports RTF compliance violations.
	/// </summary>
	/// <remarks>
	/// These options only have an effect when a <c>ComplianceLogger</c> has been set. When no logger is set,
	/// no compliance checks are performed at all.
	/// </remarks>
	public class RtfComplianceOptions
	{
		int maxIssuesPerViolation;

		/// <summary>
		/// The default compliance options.
		/// </summary>
		/// <remarks>
		/// <para>A converter starts out with a copy of these options. Changing them therefore affects
		/// every converter that is created afterward, which makes this a convenient place to
		/// apply application-wide configuration at startup.</para>
		/// <note type="note">These options are not thread-safe. They should be configured once,
		/// before any converter is created, and not changed afterward.</note>
		/// </remarks>
		public static readonly RtfComplianceOptions Default = new RtfComplianceOptions ();

		/// <summary>
		/// Initialize a new instance of the <see cref="RtfComplianceOptions"/> class.
		/// </summary>
		/// <remarks>
		/// By default, no limit is placed on the number of issues reported for each violation.
		/// </remarks>
		public RtfComplianceOptions ()
		{
		}

		/// <summary>
		/// Get or set the maximum number of times that each compliance violation may be reported.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the maximum number of times that each compliance violation may be
		/// reported while converting a single document. The default is <c>0</c>, which means that no limit is applied.</para>
		/// <para>The number of issues an RTF document can produce is bounded only by its size, and the
		/// densest forms cost only a few bytes per issue. Setting a limit is recommended when
		/// converting untrusted documents.</para>
		/// <para>The limit is per violation rather than a single total, so that a flood of cheap,
		/// harmless violations cannot crowd out the ones that follow it.</para>
		/// <para>When some violation reaches the limit, a single
		/// <see cref="RtfComplianceViolation.TooManyComplianceIssues"/> issue is reported so that the
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
		/// Clones an instance of <see cref="RtfComplianceOptions"/>.
		/// </summary>
		/// <remarks>
		/// Clones a set of options, allowing you to change a specific option
		/// without requiring you to change the original.
		/// </remarks>
		/// <returns>An identical copy of the current instance.</returns>
		public RtfComplianceOptions Clone ()
		{
			return new RtfComplianceOptions {
				maxIssuesPerViolation = maxIssuesPerViolation
			};
		}
	}
}

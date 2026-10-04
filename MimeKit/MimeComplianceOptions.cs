//
// MimeComplianceOptions.cs
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
	/// Options that control how a <see cref="MimeReader"/> reports MIME compliance violations.
	/// </summary>
	/// <remarks>
	/// <para>These options only have an effect when a <see cref="MimeReader.ComplianceLogger"/> has
	/// been set. When no logger is set, no compliance checks are performed at all.</para>
	/// <para>The options are read at the start of each parse operation, so changes made while a
	/// message is being parsed take effect from the next parse operation.</para>
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\MimeReaderExamples.cs" region="ComplianceLogger"/>
	/// </example>
	public class MimeComplianceOptions
	{
		int maxIssuesPerViolation;

		/// <summary>
		/// The default compliance options.
		/// </summary>
		/// <remarks>
		/// <para>A <see cref="MimeReader"/> starts out with a copy of these options. Changing them
		/// therefore affects every <see cref="MimeReader"/> that is created afterward, which makes
		/// this a convenient place to apply application-wide configuration, such as disabling a
		/// validator, at startup.</para>
		/// <note type="note">These options are not thread-safe. They should be configured once,
		/// before any <see cref="MimeReader"/> is created, and not changed afterward.</note>
		/// </remarks>
		public static readonly MimeComplianceOptions Default = new MimeComplianceOptions ();

		/// <summary>
		/// Initialize a new instance of the <see cref="MimeComplianceOptions"/> class.
		/// </summary>
		/// <remarks>
		/// By default, the context is <see cref="MimeComplianceContext.Transport"/>, no limit is
		/// placed on the number of issues reported for each violation, and all validators are enabled.
		/// </remarks>
		public MimeComplianceOptions ()
		{
			Context = MimeComplianceContext.Transport;
			EnabledValidators = MimeComplianceValidators.All;
		}

		/// <summary>
		/// Get or set the context that the message is being used in.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the context that the message is being used in.</para>
		/// <para>A handful of MIME compliance violations are requirements of the channel that a
		/// message travels over rather than of the message itself, and are therefore rated lower in
		/// <see cref="MimeComplianceContext.Storage"/> than in
		/// <see cref="MimeComplianceContext.Transport"/>. Set this to
		/// <see cref="MimeComplianceContext.Storage"/> when parsing messages that were read from a
		/// local message store such as an mbox file or a Maildir.</para>
		/// <para>This does not change which violations are reported, only the
		/// <see cref="MimeComplianceIssue.Severity"/> that they are reported with.</para>
		/// </remarks>
		/// <value>The MIME compliance context.</value>
		public MimeComplianceContext Context {
			get; set;
		}

		/// <summary>
		/// Get or set the validators that should be run.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the validators that should be run. The default is
		/// <see cref="MimeComplianceValidators.All"/>.</para>
		/// <para>Each validator inspects a particular kind of content more deeply than the structural
		/// checks that the <see cref="MimeReader"/> always performs. Disabling a validator stops the
		/// violations that it detects from being reported, but has no effect on how the message is
		/// parsed.</para>
		/// <para>This exists so that a validator can be switched off without having to disable
		/// compliance reporting altogether, for example if it turns out to be too expensive for, or to
		/// misbehave on, the messages that a particular deployment sees.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeReaderExamples.cs" region="ComplianceLogger"/>
		/// </example>
		/// <value>The enabled validators.</value>
		public MimeComplianceValidators EnabledValidators {
			get; set;
		}

		/// <summary>
		/// Get or set the maximum number of times that each compliance violation may be reported.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the maximum number of times that each compliance violation may be
		/// reported during a single parse operation. The default is <c>0</c>, which means that no limit
		/// is applied.</para>
		/// <para>The number of issues a message can produce is bounded only by its size, and the
		/// densest forms cost only a few bytes per issue, so a message built for the purpose can
		/// produce issues far faster than anything downstream can afford to record them. Setting a
		/// limit is recommended when parsing untrusted messages.</para>
		/// <para>The limit is per violation rather than a single total for the message. A total budget
		/// would itself be exploitable: a message could open with a flood of cheap, harmless violations
		/// and place the interesting ones after the point where the budget is known to run out. Giving
		/// each violation its own budget means that no violation can crowd out any other, while still
		/// bounding the total at the number of distinct violations times this value.</para>
		/// <para>When some violation reaches the limit, a single
		/// <see cref="MimeComplianceViolation.TooManyComplianceIssues"/> issue is reported so that the
		/// report is never silently incomplete.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeReaderExamples.cs" region="ComplianceLogger"/>
		/// </example>
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
		/// Clones an instance of <see cref="MimeComplianceOptions"/>.
		/// </summary>
		/// <remarks>
		/// Clones a set of options, allowing you to change a specific option
		/// without requiring you to change the original.
		/// </remarks>
		/// <returns>An identical copy of the current instance.</returns>
		public MimeComplianceOptions Clone ()
		{
			return new MimeComplianceOptions {
				Context = Context,
				EnabledValidators = EnabledValidators,
				maxIssuesPerViolation = maxIssuesPerViolation
			};
		}
	}
}

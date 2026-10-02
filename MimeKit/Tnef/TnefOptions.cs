//
// TnefOptions.cs
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
	/// Options that control how a TNEF stream is read.
	/// </summary>
	/// <remarks>
	/// <para>Options that control how a TNEF stream is read.</para>
	/// <para>The options are consulted while the stream is being read, so they should not be modified
	/// while a <see cref="TnefReader"/> that uses them is in use.</para>
	/// </remarks>
	public class TnefOptions
	{
		/// <summary>
		/// The default maximum nesting depth of embedded messages.
		/// </summary>
		/// <remarks>
		/// The default maximum nesting depth of embedded messages.
		/// </remarks>
		public const int DefaultMaxNestingDepth = 32;

		/// <summary>
		/// The default TNEF options.
		/// </summary>
		/// <remarks>
		/// <para>If a <see langword="null"/> <see cref="TnefOptions"/> is passed to any of the TNEF APIs,
		/// then the default options will be used.</para>
		/// </remarks>
		public static readonly TnefOptions Default = new TnefOptions ();

		int defaultCodepage;
		int maxNestingDepth = DefaultMaxNestingDepth;

		/// <summary>
		/// Initialize a new instance of the <see cref="TnefOptions"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new set of TNEF options with the default values.
		/// </remarks>
		public TnefOptions ()
		{
		}

		/// <summary>
		/// Get or set the codepage to use for 8-bit strings when the stream does not specify one.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the codepage to use for decoding 8-bit strings until the TNEF stream specifies
		/// its own codepage using the <see cref="TnefAttributeTag.OemCodepage"/> attribute.</para>
		/// <para>A value of <c>0</c> means windows-1252. If the requested codepage is not available on the
		/// host (which is common on non-Windows platforms unless the application has registered the
		/// <c>System.Text.Encoding.CodePages</c> provider), a fallback encoding will be used instead.</para>
		/// </remarks>
		/// <value>The default codepage.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is negative.
		/// </exception>
		public int DefaultCodepage {
			get { return defaultCodepage; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException (nameof (value));

				defaultCodepage = value;
			}
		}

		/// <summary>
		/// Get or set the maximum nesting depth of embedded messages.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the maximum depth of embedded messages that will be read before the TNEF
		/// stream is assumed to be maliciously formed.</para>
		/// <para>When the limit is exceeded, a <see cref="TnefComplianceViolation.NestingTooDeep"/> issue
		/// is reported and the embedded message is treated as empty.</para>
		/// </remarks>
		/// <value>The maximum nesting depth. The default is <see cref="DefaultMaxNestingDepth"/>.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is negative.
		/// </exception>
		public int MaxNestingDepth {
			get { return maxNestingDepth; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException (nameof (value));

				maxNestingDepth = value;
			}
		}

		/// <summary>
		/// Clone an instance of <see cref="TnefOptions"/>.
		/// </summary>
		/// <remarks>
		/// Clones a set of options, allowing you to change a specific option
		/// without requiring you to change the original.
		/// </remarks>
		/// <returns>An identical copy of the current instance.</returns>
		public TnefOptions Clone ()
		{
			return new TnefOptions {
				defaultCodepage = defaultCodepage,
				maxNestingDepth = maxNestingDepth
			};
		}
	}
}

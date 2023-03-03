// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MimeKit {
	/// <summary>
	/// The method to use for encoding Content-Type and Content-Disposition parameter values.
	/// </summary>
	/// <remarks>
	/// The MIME specifications specify that the proper method for encoding Content-Type and
	/// Content-Disposition parameter values is the method described in
	/// <a href="https://tools.ietf.org/html/rfc2231">rfc2231</a>. However, it is common for
	/// some older email clients to improperly encode using the method described in
	/// <a href="https://tools.ietf.org/html/rfc2047">rfc2047</a> instead.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\ParameterExamples.cs" region="OverrideAllParameterEncodings"/>
	/// </example>
	public enum ParameterEncodingMethod {
		/// <summary>
		/// Use the default encoding method set on the <see cref="FormatOptions"/>.
		/// </summary>
		Default = 0,

		/// <summary>
		/// Use the encoding method described in rfc2231.
		/// </summary>
		Rfc2231  = (1 << 0),

		/// <summary>
		/// Use the encoding method described in rfc2047 (for compatibility with older,
		/// non-rfc-compliant email clients).
		/// </summary>
		Rfc2047  = (1 << 1)
	}
}

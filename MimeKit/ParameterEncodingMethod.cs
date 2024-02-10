//
// ParameterEncodingMethod.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2024 .NET Foundation and Contributors
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

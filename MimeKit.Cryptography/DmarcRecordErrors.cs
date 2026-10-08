//
// DmarcRecordErrors.cs
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// The syntax errors found while parsing a DMARC Policy Record.
	/// </summary>
	/// <remarks>
	/// <para>As required by <a href="https://www.rfc-editor.org/rfc/rfc9989#section-4.8">RFC 9989, Section 4.8</a>,
	/// syntax errors in a DMARC Policy Record are discarded in favor of default values. These flags record which
	/// errors were discarded, so that they can be logged or included in reports.</para>
	/// </remarks>
	/// <seealso cref="DmarcRecord.Errors"/>
	[Flags]
	public enum DmarcRecordErrors
	{
		/// <summary>
		/// No errors.
		/// </summary>
		None = 0,

		/// <summary>
		/// A tag was not of the form <c>name=value</c>, or its name contained characters other than letters.
		/// </summary>
		MalformedTag = 1 << 0,

		/// <summary>
		/// A tag appeared more than once. The first occurrence is used.
		/// </summary>
		DuplicateTag = 1 << 1,

		/// <summary>
		/// The value of the <c>p</c> tag was invalid.
		/// </summary>
		InvalidPolicy = 1 << 2,

		/// <summary>
		/// The value of the <c>sp</c> tag was invalid.
		/// </summary>
		InvalidSubdomainPolicy = 1 << 3,

		/// <summary>
		/// The value of the <c>np</c> tag was invalid.
		/// </summary>
		InvalidNonExistentSubdomainPolicy = 1 << 4,

		/// <summary>
		/// The value of the <c>psd</c> tag was invalid.
		/// </summary>
		InvalidPublicSuffixDomain = 1 << 5,

		/// <summary>
		/// The value of the <c>t</c> tag was invalid.
		/// </summary>
		InvalidTesting = 1 << 6,

		/// <summary>
		/// The value of the <c>adkim</c> tag was invalid.
		/// </summary>
		InvalidDkimAlignment = 1 << 7,

		/// <summary>
		/// The value of the <c>aspf</c> tag was invalid.
		/// </summary>
		InvalidSpfAlignment = 1 << 8,

		/// <summary>
		/// The <c>rua</c> tag was empty or contained at least one invalid URI.
		/// </summary>
		InvalidAggregateReportUri = 1 << 9,

		/// <summary>
		/// The <c>ruf</c> tag was empty or contained at least one invalid URI.
		/// </summary>
		InvalidFailureReportUri = 1 << 10,

		/// <summary>
		/// The value of the <c>fo</c> tag was invalid.
		/// </summary>
		InvalidFailureReportingOptions = 1 << 11
	}
}

//
// DmarcAlignmentMode.cs
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// A DMARC identifier alignment mode.
	/// </summary>
	/// <remarks>
	/// <para>Specifies how strictly an authenticated identifier must match the Author Domain, as specified by
	/// the <c>adkim</c> and <c>aspf</c> tags of a DMARC Policy Record.</para>
	/// <para>See <a href="https://www.rfc-editor.org/rfc/rfc9989#section-4.4">RFC 9989, Section 4.4</a>
	/// for details.</para>
	/// </remarks>
	/// <seealso cref="DmarcRecord"/>
	public enum DmarcAlignmentMode
	{
		/// <summary>
		/// The authenticated identifier and the Author Domain must have the same Organizational Domain.
		/// </summary>
		Relaxed,

		/// <summary>
		/// The authenticated identifier and the Author Domain must be identical.
		/// </summary>
		Strict
	}
}

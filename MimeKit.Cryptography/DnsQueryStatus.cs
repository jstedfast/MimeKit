//
// DnsQueryStatus.cs
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
	/// The outcome of a DNS query.
	/// </summary>
	/// <remarks>
	/// The outcome of a DNS query performed by an <see cref="IDnsResolver"/>.
	/// </remarks>
	/// <seealso cref="DnsTxtResponse"/>
	/// <seealso cref="IDnsResolver"/>
	public enum DnsQueryStatus
	{
		/// <summary>
		/// The query completed successfully (the DNS response code was <c>NOERROR</c>).
		/// </summary>
		/// <remarks>
		/// A successful response may contain no records at all if the domain name exists but has no records of
		/// the requested type (sometimes referred to as a <c>NODATA</c> response).
		/// </remarks>
		Success,

		/// <summary>
		/// The domain name does not exist (the DNS response code was <c>NXDOMAIN</c>).
		/// </summary>
		NonExistentDomain,

		/// <summary>
		/// The query could not be completed, for example due to a timeout, a network error or a <c>SERVFAIL</c>
		/// response. Retrying the query later may succeed.
		/// </summary>
		TemporaryFailure
	}
}

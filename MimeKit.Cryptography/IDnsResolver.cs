//
// IDnsResolver.cs
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

using System.Threading;
using System.Threading.Tasks;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An interface for a DNS resolver.
	/// </summary>
	/// <remarks>
	/// <para>An interface for a DNS resolver that is used by the <see cref="DkimVerifier"/> and
	/// <see cref="ArcVerifier"/> to look up the public keys needed to verify signatures.</para>
	/// <para>Since MimeKit itself does not implement DNS, it is up to the client to implement the DNS
	/// queries. MimeKit takes care of parsing the records that are returned, so an implementation only
	/// needs to perform the query and report the outcome.</para>
	/// <para>Implementations are encouraged to cache responses, since the same records are often
	/// requested many times when verifying a large number of messages.</para>
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
	/// </example>
	/// <example>
	/// <code language="c#" source="Examples\ArcVerifierExample.cs" />
	/// </example>
	/// <seealso cref="ArcVerifier"/>
	/// <seealso cref="DkimVerifier"/>
	public interface IDnsResolver
	{
		/// <summary>
		/// Query the DNS TXT records for the specified domain name.
		/// </summary>
		/// <remarks>
		/// <para>Queries the DNS TXT records for the specified domain name.</para>
		/// <para>Each TXT record in the response is made up of one or more character-strings which
		/// MUST be concatenated, without any separator, into a single string before being returned in
		/// <see cref="DnsTxtResponse.Records"/>.</para>
		/// <para>Implementations should return a <see cref="DnsTxtResponse"/> with a
		/// <see cref="DnsTxtResponse.Status"/> that reflects the outcome of the query rather than throw an
		/// exception. In particular, it is important to distinguish a domain name that does not exist
		/// (<see cref="DnsQueryStatus.NonExistentDomain"/>) from a domain name that exists but has no TXT
		/// records (<see cref="DnsQueryStatus.Success"/> with an empty list of records) and from a query
		/// that could not be completed (<see cref="DnsQueryStatus.TemporaryFailure"/>). Any exception other
		/// than an <see cref="System.OperationCanceledException"/> is treated as a
		/// <see cref="DnsQueryStatus.TemporaryFailure"/>.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
		/// </example>
		/// <returns>The DNS response.</returns>
		/// <param name="domain">The fully-qualified domain name to query, in ASCII (A-label) form.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		DnsTxtResponse QueryTxt (string domain, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously query the DNS TXT records for the specified domain name.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously queries the DNS TXT records for the specified domain name.</para>
		/// <para>Each TXT record in the response is made up of one or more character-strings which
		/// MUST be concatenated, without any separator, into a single string before being returned in
		/// <see cref="DnsTxtResponse.Records"/>.</para>
		/// <para>Implementations should return a <see cref="DnsTxtResponse"/> with a
		/// <see cref="DnsTxtResponse.Status"/> that reflects the outcome of the query rather than throw an
		/// exception. In particular, it is important to distinguish a domain name that does not exist
		/// (<see cref="DnsQueryStatus.NonExistentDomain"/>) from a domain name that exists but has no TXT
		/// records (<see cref="DnsQueryStatus.Success"/> with an empty list of records) and from a query
		/// that could not be completed (<see cref="DnsQueryStatus.TemporaryFailure"/>). Any exception other
		/// than an <see cref="System.OperationCanceledException"/> is treated as a
		/// <see cref="DnsQueryStatus.TemporaryFailure"/>.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
		/// </example>
		/// <returns>The DNS response.</returns>
		/// <param name="domain">The fully-qualified domain name to query, in ASCII (A-label) form.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		Task<DnsTxtResponse> QueryTxtAsync (string domain, CancellationToken cancellationToken = default);
	}
}

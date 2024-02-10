//
// IDkimPublicKeyLocator.cs
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

using System.Threading;
using System.Threading.Tasks;

using Org.BouncyCastle.Crypto;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An interface for a service which locates and retrieves DKIM public keys (probably via DNS).
	/// </summary>
	/// <remarks>
	/// <para>An interface for a service which locates and retrieves DKIM public keys (probably via DNS).</para>
	/// <para>Since MimeKit itself does not implement DNS, it is up to the client to implement public key lookups
	/// via DNS.</para>
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
	/// </example>
	/// <example>
	/// <code language="c#" source="Examples\ArcVerifierExample.cs" />
	/// </example>
	/// <seealso cref="DkimPublicKeyLocatorBase"/>
	/// <seealso cref="ArcVerifier"/>
	/// <seealso cref="DkimVerifier"/>
	public interface IDkimPublicKeyLocator
	{
		/// <summary>
		/// Locate and retrieve the public key for the given domain and selector.
		/// </summary>
		/// <remarks>
		/// <para>Locates and retrieves the public key for the given domain and selector.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
		/// </example>
		/// <example>
		/// <code language="c#" source="Examples\ArcVerifierExample.cs" />
		/// </example>
		/// <seealso cref="ArcVerifier"/>
		/// <seealso cref="DkimVerifier"/>
		/// <returns>The public key.</returns>
		/// <param name="methods">A colon-separated list of query methods used to retrieve the public key. The default is <c>"dns/txt"</c>.</param>
		/// <param name="domain">The domain.</param>
		/// <param name="selector">The selector.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		AsymmetricKeyParameter LocatePublicKey (string methods, string domain, string selector, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously locate and retrieve the public key for the given domain and selector.
		/// </summary>
		/// <remarks>
		/// <para>Locates and retrieves the public key for the given domain and selector.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
		/// </example>
		/// <example>
		/// <code language="c#" source="Examples\ArcVerifierExample.cs" />
		/// </example>
		/// <seealso cref="ArcVerifier"/>
		/// <seealso cref="DkimVerifier"/>
		/// <returns>The public key.</returns>
		/// <param name="methods">A colon-separated list of query methods used to retrieve the public key. The default is <c>"dns/txt"</c>.</param>
		/// <param name="domain">The domain.</param>
		/// <param name="selector">The selector.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		Task<AsymmetricKeyParameter> LocatePublicKeyAsync (string methods, string domain, string selector, CancellationToken cancellationToken = default);
	}
}

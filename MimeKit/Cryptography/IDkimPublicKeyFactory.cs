//
// IDkimPublicKeyFactory.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2025 .NET Foundation and Contributors
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
	/// An interface for a factory that creates <see cref="IDkimPublicKey"/> instances.
	/// </summary>
	/// <remarks>
	/// An interface for a factory that creates <see cref="IDkimPublicKey"/> instances.
	/// </remarks>
	public interface IDkimPublicKeyFactory
	{
		/// <summary>
		/// Create a new instance of an <see cref="IDkimPublicKey"/>.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new instance of an <see cref="IDkimPublicKey"/> based on the parameters provided.</para>
		/// <para>The <paramref name="algorithm"/> string should be the <c>k</c> parameter value from a DNS DKIM TXT
		/// record while the <paramref name="keyData"/> string should be the <c>p</c> parameter value which in general
		/// will be the base64 encoded key data.</para>
		/// </remarks>
		/// <param name="algorithm">The public key algorithm.</param>
		/// <param name="keyData">The base64 encoded public key data.</param>
		/// <returns>A new instance of an <see cref="IDkimPublicKey"/>.</returns>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="algorithm"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="keyData"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The <paramref name="algorithm"/> is not supported.
		/// </exception>
		IDkimPublicKey CreatePublicKey (string algorithm, string keyData);

		/// <summary>
		/// Create a new instance of an <see cref="IDkimPublicKey"/>.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new instance of an <see cref="IDkimPublicKey"/> based on the parameters provided.</para>
		/// <para>The <paramref name="algorithm"/> string should be the <c>k</c> parameter value from a DNS DKIM TXT
		/// record while the <paramref name="keyData"/> string should be the <c>p</c> parameter value which in general
		/// will be the base64 encoded key data.</para>
		/// </remarks>
		/// <param name="algorithm">The public key algorithm.</param>
		/// <param name="keyData">The base64 encoded public key data.</param>
		/// <returns>A new instance of an <see cref="IDkimPublicKey"/>.</returns>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="algorithm"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="keyData"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The <paramref name="algorithm"/> is not supported.
		/// </exception>
		IDkimPublicKey CreatePublicKey (DkimPublicKeyAlgorithm algorithm, string keyData);
	}
}

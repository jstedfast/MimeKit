//
// IDkimSignatureContext.cs
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

using System;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An interface for a context used for generating and verifying DKIM signatures.
	/// </summary>
	/// <remarks>
	/// Represents a context used for generating and verifying DKIM signatures.
	/// </remarks>
	public interface IDkimSignatureContext : IDisposable
	{
		/// <summary>
		/// Update the signature context.
		/// </summary>
		/// <remarks>
		/// Updates the internal hash state of the signature context with the contents of the buffer.
		/// </remarks>
		/// <param name="buffer">The buffer.</param>
		/// <param name="offset">The offset into the buffer.</param>
		/// <param name="length">The length of the content within the buffer.</param>
		void Update (byte[] buffer, int offset, int length);

		/// <summary>
		/// Generate the signature.
		/// </summary>
		/// <remarks>
		/// Generates the signature for the data that has been hashed by previous calls to <see cref="Update"/>.
		/// </remarks>
		/// <returns>The signature.</returns>
		byte[] GenerateSignature ();

		/// <summary>
		/// Verify the signature.
		/// </summary>
		/// <remarks>
		/// Verifies the signature for the data that has been hashed by previous calls to <see cref="Update"/>.
		/// </remarks>
		/// <returns><c>true</c> if the signature is valid; otherwise, <c>false</c>.</returns>
		bool VerifySignature (byte[] signature);
	}
}

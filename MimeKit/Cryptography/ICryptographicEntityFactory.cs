//
// ICryptographicEntityFactory.cs
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
	/// A factory interface for creating cryptographic MIME entities.
	/// </summary>
	/// <remarks>
	/// A factory interface for creating cryptographic MIME entities.
	/// </remarks>
	interface ICryptographicEntityFactory
	{
		/// <summary>
		/// Creates a new entity representing an application/pgp-encrypted <see cref="MimePart"/>.
		/// </summary>
		/// <remarks>
		/// Creates a new entity representing an application/pgp-encrypted <see cref="MimePart"/>.
		/// </remarks>
		/// <param name="args">The constructor arguments.</param>
		/// <returns>A new instance of an entity representing an application/pgp-encrypted.</returns>
		MimePart CreateApplicationPgpEncrypted (MimeEntityConstructorArgs args);

		/// <summary>
		/// Creates a new entity representing an application/pgp-signature <see cref="MimePart"/>.
		/// </summary>
		/// <remarks>
		/// Creates a new entity representing an application/pgp-signature <see cref="MimePart"/>.
		/// </remarks>
		/// <param name="args">The constructor arguments.</param>
		/// <returns>A new instance of an entity representing an application/pgp-signature.</returns>
		MimePart CreateApplicationPgpSignature (MimeEntityConstructorArgs args);

		/// <summary>
		/// Creates a new entity representing an application/pkcs7-mime <see cref="MimePart"/>.
		/// </summary>
		/// <remarks>
		/// Creates a new entity representing an application/pkcs7-mime <see cref="MimePart"/>.
		/// </remarks>
		/// <param name="args">The constructor arguments.</param>
		/// <returns>A new instance of an entity representing an application/pkcs7-mime.</returns>
		MimePart CreateApplicationPkcs7Mime (MimeEntityConstructorArgs args);

		/// <summary>
		/// Creates a new entity representing an application/pkcs7-signature <see cref="MimePart"/>.
		/// </summary>
		/// <remarks>
		/// Creates a new entity representing an application/pkcs7-signature <see cref="MimePart"/>.
		/// </remarks>
		/// <param name="args">The constructor arguments.</param>
		/// <returns>A new instance of an entity representing an application/pkcs7-signature.</returns>
		MimePart CreateApplicationPkcs7Signature (MimeEntityConstructorArgs args);

		/// <summary>
		/// Creates a new entity representing a multipart/encrypted <see cref="Multipart"/>.
		/// </summary>
		/// <remarks>
		/// Creates a new entity representing a multipart/encrypted <see cref="Multipart"/>.
		/// </remarks>
		/// <param name="args">The constructor arguments.</param>
		/// <returns>A new instance of an entity representing a multipart/encrypted.</returns>
		Multipart CreateMultipartEncrypted (MimeEntityConstructorArgs args);
		
		/// <summary>
		/// Creates a new entity representing a multipart/signed <see cref="Multipart"/>.
		/// </summary>
		/// <remarks>
		/// Creates a new entity representing a multipart/signed <see cref="Multipart"/>.
		/// </remarks>
		/// <param name="args">The constructor arguments.</param>
		/// <returns>A new instance of an entity representing a multipart/signed.</returns>
		Multipart CreateMultipartSigned (MimeEntityConstructorArgs args);
	}
}

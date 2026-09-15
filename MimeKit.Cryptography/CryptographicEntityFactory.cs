//
// CryptographicEntityFactory.cs
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
	/// A factory for creating cryptographic MIME entities.
	/// </summary>
	/// <remarks>
	/// A factory for creating cryptographic MIME entities.
	/// </remarks>
	class CryptographicEntityFactory : ICryptographicEntityFactory
	{
		internal static ICryptographicEntityFactory Instance { get; } = new CryptographicEntityFactory ();

		/// <summary>
		/// Creates a new <see cref="ApplicationPgpEncrypted"/> entity.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="ApplicationPgpEncrypted"/> entity.
		/// </remarks>
		/// <param name="args">The constructor arguments.</param>
		/// <returns>A new instance of <see cref="ApplicationPgpEncrypted"/>.</returns>
		public MimePart CreateApplicationPgpEncrypted (MimeEntityConstructorArgs args)
		{
			return new ApplicationPgpEncrypted (args);
		}

		/// <summary>
		/// Creates a new <see cref="ApplicationPgpSignature"/> entity.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="ApplicationPgpSignature"/> entity.
		/// </remarks>
		/// <param name="args">The constructor arguments.</param>
		/// <returns>A new instance of <see cref="ApplicationPgpSignature"/>.</returns>
		public MimePart CreateApplicationPgpSignature (MimeEntityConstructorArgs args)
		{
			return new ApplicationPgpSignature (args);
		}

		/// <summary>
		/// Creates a new <see cref="ApplicationPkcs7Mime"/> entity.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="ApplicationPkcs7Mime"/> entity.
		/// </remarks>
		/// <param name="args">The constructor arguments.</param>
		/// <returns>A new instance of <see cref="ApplicationPkcs7Mime"/>.</returns>
		public MimePart CreateApplicationPkcs7Mime (MimeEntityConstructorArgs args)
		{
			return new ApplicationPkcs7Mime (args);
		}

		/// <summary>
		/// Creates a new <see cref="ApplicationPkcs7Signature"/> entity.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="ApplicationPkcs7Signature"/> entity.
		/// </remarks>
		/// <param name="args">The constructor arguments.</param>
		/// <returns>A new instance of <see cref="ApplicationPkcs7Signature"/>.</returns>
		public MimePart CreateApplicationPkcs7Signature (MimeEntityConstructorArgs args)
		{
			return new ApplicationPkcs7Signature (args);
		}
		
		/// <summary>
		/// Creates a new <see cref="MultipartEncrypted"/> entity.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MultipartEncrypted"/> entity.
		/// </remarks>
		/// <param name="args">The constructor arguments.</param>
		/// <returns>A new instance of <see cref="MultipartEncrypted"/>.</returns>
		public Multipart CreateMultipartEncrypted (MimeEntityConstructorArgs args)
		{
			return new MultipartEncrypted (args);
		}
		
		/// <summary>
		/// Creates a new <see cref="MultipartSigned"/> entity.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MultipartSigned"/> entity.
		/// </remarks>
		/// <param name="args">The constructor arguments.</param>
		/// <returns>A new instance of <see cref="MultipartSigned"/>.</returns>
		public Multipart CreateMultipartSigned (MimeEntityConstructorArgs args)
		{
			return new MultipartSigned (args);
		}
	}
}

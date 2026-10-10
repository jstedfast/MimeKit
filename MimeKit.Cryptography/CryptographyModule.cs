//
// CryptographyModule.cs
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// The cryptography module.
	/// </summary>
	/// <remarks>
	/// The cryptography module.
	/// </remarks>
	public static class CryptographyModule
	{
		static int initialized = 0;

		/// <summary>
		/// Initializes the cryptography module.
		/// </summary>
		/// <remarks>
		/// <para>Initializes the cryptography module by registering the cryptographic entity factory with
		/// <see cref="ParserOptions"/> so that the parser will construct cryptographic MIME entities such as
		/// <see cref="MultipartSigned"/>, <see cref="MultipartEncrypted"/> and <see cref="ApplicationPkcs7Mime"/>.
		/// Without it, those entities will be parsed as a plain <see cref="Multipart"/> or <see cref="MimePart"/>.</para>
		/// <para>This method is called automatically the first time the <see cref="CryptographyContext"/> class is used
		/// (for example, via <see cref="CryptographyContext.Register(System.Type)"/>), but applications should call it
		/// during startup if they might parse messages before then.</para>
		/// <para>It is safe to call this method more than once.</para>
		/// </remarks>
		public static void Initialize ()
		{
			if (Interlocked.CompareExchange (ref initialized, 1, 0) == 0)
				ParserOptions.Register (CryptographicEntityFactory.Instance);
		}
	}
}

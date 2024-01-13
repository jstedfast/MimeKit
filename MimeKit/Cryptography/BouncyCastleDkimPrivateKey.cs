//
// BouncyCastleDkimPrivateKey.cs
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
using System.IO;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A DKIM private key implemented using BouncyCastle.
	/// </summary>
	/// <remarks>
	/// A DKIM private key implemented using BouncyCastle.
	/// </remarks>
	public class BouncyCastleDkimPrivateKey : BouncyCastleDkimKey, IDkimPrivateKey
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BouncyCastleDkimPrivateKey"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="BouncyCastleDkimPrivateKey"/>.
		/// </remarks>
		/// <param name="key">The private key.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="key"/> is not a private key.
		/// </exception>
		public BouncyCastleDkimPrivateKey (AsymmetricKeyParameter key)
		{
			if (key is null)
				throw new ArgumentNullException (nameof (key));

			if (!key.IsPrivate)
				throw new ArgumentException ("The key must be a private key.", nameof (key));

			Key = key;
		}

		/// <summary>
		/// Create a DKIM signature context suitable for signing.
		/// </summary>
		/// <remarks>
		/// Creates a DKIM signature context suitable for signing.
		/// </remarks>
		/// <param name="algorithm">The DKIM signature algorithm.</param>
		/// <returns>The DKIM signature context.</returns>
		/// <exception cref="System.NotSupportedException">
		/// The specified <paramref name="algorithm"/> is not supported.
		/// </exception>
		public IDkimSignatureContext CreateSigningContext (DkimSignatureAlgorithm algorithm)
		{
			return CreateSignatureContext (algorithm, true);
		}

		static AsymmetricKeyParameter LoadPrivateKey (Stream stream)
		{
			AsymmetricKeyParameter key = null;

			using (var reader = new StreamReader (stream)) {
				var pem = new PemReader (reader);

				var keyObject = pem.ReadObject ();

				if (keyObject is AsymmetricCipherKeyPair pair) {
					key = pair.Private;
				} else if (keyObject is AsymmetricKeyParameter param) {
					key = param;
				}
			}

			if (key == null || !key.IsPrivate)
				throw new FormatException ("Private key not found.");

			return key;
		}

		/// <summary>
		/// Load a private key from the specified stream.
		/// </summary>
		/// <remarks>
		/// Loads a private key from the specified stream.
		/// </remarks>
		/// <param name="stream">A stream containing the private DKIM key data.</param>
		/// <returns>A <see cref="BouncyCastleDkimPrivateKey"/>.</returns>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The stream did not contain a private key in PEM format.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static BouncyCastleDkimPrivateKey Load (Stream stream)
		{
			if (stream is null)
				throw new ArgumentNullException (nameof (stream));

			var key = LoadPrivateKey (stream);

			return new BouncyCastleDkimPrivateKey (key);
		}

		/// <summary>
		/// Load a private key from the specified file.
		/// </summary>
		/// <remarks>
		/// Loads a private key from the specified file.
		/// </remarks>
		/// <param name="fileName">A file containing the private DKIM key data.</param>
		/// <returns>A <see cref="BouncyCastleDkimPrivateKey"/>.</returns>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The stream did not contain a private key in PEM format.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static BouncyCastleDkimPrivateKey Load (string fileName)
		{
			if (fileName is null)
				throw new ArgumentNullException (nameof (fileName));

			using (var stream = File.OpenRead (fileName))
				return Load (stream);
		}
	}
}

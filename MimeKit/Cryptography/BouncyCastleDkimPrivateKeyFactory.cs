//
// BouncyCastleDkimPrivateKeyFactory.cs
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
	/// A DKIM private key factory implemented using BouncyCastle.
	/// </summary>
	/// <remarks>
	/// A DKIM private key factory implemented using BouncyCastle.
	/// </remarks>
	public class BouncyCastleDkimPrivateKeyFactory : IDkimPrivateKeyFactory
	{
		/// <summary>
		/// Instantiates a new instance of the <see cref="BouncyCastleDkimPrivateKeyFactory"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="BouncyCastleDkimPrivateKeyFactory"/>.
		/// </remarks>
		public BouncyCastleDkimPrivateKeyFactory ()
		{
		}

		/// <summary>
		/// Create a new <see cref="IDkimPrivateKey"/> instance.
		/// </summary>
		/// <param name="key">The private key.</param>
		/// <returns>A new <see cref="IDkimPrivateKey"/> based on the private key provided.</returns>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="key"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="key"/> is not a private key.
		/// </exception>
		public IDkimPrivateKey Create (AsymmetricKeyParameter key)
		{
			return new BouncyCastleDkimPrivateKey (key);
		}

		/// <summary>
		/// Load a private key for use with DKIM and ARC signing.
		/// </summary>
		/// <remarks>
		/// Loads a private key for use with DKIM and ARC signing.
		/// </remarks>
		/// <param name="fileName">The file name containing the PEM-formatted private key.</param>
		/// <returns>A new instance of an <see cref="IDkimPrivateKey"/>.</returns>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.
		/// </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// <paramref name="fileName"/> is an invalid file path.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file path could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The <paramref name="fileName"/> is not in the correct format.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public IDkimPrivateKey LoadPrivateKey (string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException (nameof (fileName));

			if (fileName.Length == 0)
				throw new ArgumentException ("The file name cannot be empty.", nameof (fileName));

			using (var stream = File.OpenRead (fileName))
				return LoadPrivateKey (stream);
		}

		/// <summary>
		/// Load a private key for use with DKIM and ARC signing.
		/// </summary>
		/// <remarks>
		/// Loads a private key for use with DKIM and ARC signing.
		/// </remarks>
		/// <param name="stream">The stream containing the PEM-formatted private key.</param>
		/// <returns>A new instance of an <see cref="IDkimPrivateKey"/>.</returns>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The <paramref name="stream"/> is not in the correct format.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public IDkimPrivateKey LoadPrivateKey (Stream stream)
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

			return new BouncyCastleDkimPrivateKey (key);
		}
	}
}

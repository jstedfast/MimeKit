//
// IDkimPrivateKeyFactory.cs
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

using System.IO;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An interface for a factory that creates <see cref="IDkimPrivateKey"/> instances.
	/// </summary>
	/// <remarks>
	/// An interface for a factory that creates <see cref="IDkimPrivateKey"/> instances.
	/// </remarks>
	public interface IDkimPrivateKeyFactory
	{
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
		IDkimPrivateKey LoadPrivateKey (string fileName);

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
		IDkimPrivateKey LoadPrivateKey (Stream stream);
	}
}

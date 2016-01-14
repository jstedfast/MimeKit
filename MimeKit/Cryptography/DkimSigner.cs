//
// DkimSigner.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2016 Xamarin Inc. (www.xamarin.com)
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
	/// A DKIM signer.
	/// </summary>
	/// <remarks>
	/// A DKIM signer.
	/// </remarks>
	public class DkimSigner
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.DkimSigner"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="DkimSigner"/>.
		/// </remarks>
		/// <param name="key">The signer's private key.</param>
		/// <param name="domain">The domain that the signer represents.</param>
		/// <param name="selector">The selector subdividing the domain.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="key"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="domain"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="selector"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="key"/> is not a private key.
		/// </exception>
		public DkimSigner (AsymmetricKeyParameter key, string domain, string selector)
		{
			if (key == null)
				throw new ArgumentNullException ("key");

			if (domain == null)
				throw new ArgumentNullException ("domain");

			if (selector == null)
				throw new ArgumentNullException ("selector");

			if (!key.IsPrivate)
				throw new ArgumentException ("The key must be a private key.", "key");

			SignatureAlgorithm = DkimSignatureAlgorithm.RsaSha256;
			Selector = selector;
			PrivateKey = key;
			Domain = domain;
		}

#if !PORTABLE
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.DkimSigner"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="DkimSigner"/>.
		/// </remarks>
		/// <param name="fileName">The file containing the private key.</param>
		/// <param name="domain">The domain that the signer represents.</param>
		/// <param name="selector">The selector subdividing the domain.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="domain"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="selector"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters as defined by
		/// <see cref="System.IO.Path.InvalidPathChars"/>.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The file did not contain a private key.
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
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public DkimSigner (string fileName, string domain, string selector)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			if (fileName.Length == 0)
				throw new ArgumentException ("The file name cannot be empty.", "fileName");

			if (domain == null)
				throw new ArgumentNullException ("domain");

			if (selector == null)
				throw new ArgumentNullException ("selector");

			AsymmetricKeyParameter key;

			using (var stream = File.OpenRead (fileName)) {
				using (var reader = new StreamReader (stream)) {
					var pem = new PemReader (reader);

					var keyObject = pem.ReadObject ();

					if (keyObject != null) {
						key = keyObject as AsymmetricKeyParameter;

						if (key == null) {
							var pair = keyObject as AsymmetricCipherKeyPair;

							if (pair != null)
								key = pair.Private;
						}
					}
				}
			}

			if (key == null || !key.IsPrivate)
				throw new FormatException ("Private key not found.");

			SignatureAlgorithm = DkimSignatureAlgorithm.RsaSha256;
			PrivateKey = key;
			Selector = selector;
			Domain = domain;
		}
#endif

		/// <summary>
		/// Gets the private key.
		/// </summary>
		/// <remarks>
		/// The private key used for signing.
		/// </remarks>
		/// <value>The private key.</value>
		public AsymmetricKeyParameter PrivateKey {
			get; private set;
		}

		/// <summary>
		/// Get the domain that the signer represents.
		/// </summary>
		/// <remarks>
		/// Gets the domain that the signer represents.
		/// </remarks>
		/// <value>The domain.</value>
		public string Domain {
			get; private set;
		}

		/// <summary>
		/// Get the selector subdividing the domain.
		/// </summary>
		/// <remarks>
		/// Gets the selector subdividing the domain.
		/// </remarks>
		/// <value>The selector.</value>
		public string Selector {
			get; private set;
		}

		/// <summary>
		/// Get or set the agent or user identifier.
		/// </summary>
		/// <remarks>
		/// Gets or sets the agent or user identifier.
		/// </remarks>
		/// <value>The agent or user identifier.</value>
		public string AgentOrUserIdentifier {
			get; set;
		}

		/// <summary>
		/// Get or set the algorithm to use for signing.
		/// </summary>
		/// <remarks>
		/// Gets or sets the algorithm to use for signing.
		/// </remarks>
		/// <value>The signature algorithm.</value>
		public DkimSignatureAlgorithm SignatureAlgorithm {
			get; set;
		}

		/// <summary>
		/// Get or set the public key query method.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the public key query method.</para>
		/// <para>The value should be a colon-separated list of query methods used to
		/// retrieve the public key (plain-text; OPTIONAL, default is "dns/txt"). Each
		/// query method is of the form "type[/options]", where the syntax and
		/// semantics of the options depend on the type and specified options.</para>
		/// </remarks>
		/// <value>The public key query method.</value>
		public string QueryMethod {
			get; set;
		}
	}
}

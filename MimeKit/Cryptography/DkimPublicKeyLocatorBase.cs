//
// DkimPublicKeyLocatorBase.cs
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

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Parameters;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A base class for implemnentations of <see cref="IDkimPublicKeyLocator"/>.
	/// </summary>
	/// <remarks>
	/// The <see cref="DkimPublicKeyLocatorBase"/> class provides a helpful
	/// method for parsing DNS TXT records in order to extract the public key.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
	/// </example>
	/// <example>
	/// <code language="c#" source="Examples\ArcVerifierExample.cs" />
	/// </example>
	public abstract class DkimPublicKeyLocatorBase : IDkimPublicKeyLocator
	{
		/// <summary>
		/// Get the public key from a DNS TXT record.
		/// </summary>
		/// <remarks>
		/// Gets the public key from a DNS TXT record.
		/// </remarks>
		/// <param name="txt">The DNS TXT record.</param>
		/// <returns>The public key.</returns>
		/// <exception cref="System.ArgumentNullException">
		/// The <paramref name="txt"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ParseException">
		/// There was an error parsing the DNS TXT record.
		/// </exception>
		protected static AsymmetricKeyParameter GetPublicKey (string txt)
		{
			AsymmetricKeyParameter pubkey;
			string k = "rsa", p = null;
			int index = 0;

			if (txt == null)
				throw new ArgumentNullException (nameof (txt));

			// parse the response (will look something like: "k=rsa; p=<base64>")
			while (index < txt.Length) {
				while (index < txt.Length && char.IsWhiteSpace (txt[index]))
					index++;

				if (index == txt.Length)
					break;

				// find the end of the key
				int startIndex = index;
				while (index < txt.Length && txt[index] != '=')
					index++;

				if (index == txt.Length)
					break;

				var key = txt.AsSpan (startIndex, index - startIndex);

				// skip over the '='
				index++;

				// find the end of the value
				startIndex = index;
				while (index < txt.Length && txt[index] != ';')
					index++;

				var value = txt.Substring (startIndex, index - startIndex);

				if (key.SequenceEqual ("k".AsSpan ())) {
					switch (value) {
					case "rsa": case "ed25519": k = value; break;
					default: throw new ParseException ($"Unknown public key algorithm: {value}", startIndex, index);
					}
				} else if (key.SequenceEqual ("p".AsSpan ())) {
					p = value.Replace (" ", "");
				}

				// skip over the ';'
				index++;
			}

			if (p != null) {
				if (k == "ed25519") {
					var decoded = Convert.FromBase64String (p);

					return new Ed25519PublicKeyParameters (decoded, 0);
				}

				var data = "-----BEGIN PUBLIC KEY-----\r\n" + p + "\r\n-----END PUBLIC KEY-----\r\n";
				var rawData = Encoding.ASCII.GetBytes (data);

				using (var stream = new MemoryStream (rawData, false)) {
					using (var reader = new StreamReader (stream)) {
						var pem = new PemReader (reader);

						pubkey = pem.ReadObject () as AsymmetricKeyParameter;

						if (pubkey != null)
							return pubkey;
					}
				}
			}

			throw new ParseException ("Public key parameters not found in DNS TXT record.", 0, txt.Length);
		}

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
		public abstract AsymmetricKeyParameter LocatePublicKey (string methods, string domain, string selector, CancellationToken cancellationToken = default);

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
		public abstract Task<AsymmetricKeyParameter> LocatePublicKeyAsync (string methods, string domain, string selector, CancellationToken cancellationToken = default);
	}
}

//
// DkimPublicKeyLocator.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2019 Xamarin Inc. (www.xamarin.com)
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
using System.Collections.Generic;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Parameters;

using MimeKit.Cryptography;

namespace UnitTests.Cryptography
{
	class DkimPublicKeyLocator : IDkimPublicKeyLocator
	{
		readonly Dictionary<string, string> keys;

		public DkimPublicKeyLocator ()
		{
			keys = new Dictionary<string, string> ();
		}

		public void Add (string key, string value)
		{
			keys.Add (key, value);
		}

		public AsymmetricKeyParameter LocatePublicKey (string methods, string domain, string selector, CancellationToken cancellationToken = default (CancellationToken))
		{
			var query = selector + "._domainkey." + domain;

			if (keys.TryGetValue (query, out string txt)) {
				AsymmetricKeyParameter pubkey;
				string k = null, p = null;
				int index = 0;

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

					var key = txt.Substring (startIndex, index - startIndex);

					// skip over the '='
					index++;

					// find the end of the value
					startIndex = index;
					while (index < txt.Length && txt[index] != ';')
						index++;

					var value = txt.Substring (startIndex, index - startIndex).Replace (" ", "");

					switch (key) {
					case "k": k = value; break;
					case "p": p = value; break;
					}

					// skip over the ';'
					index++;
				}

				if (k != null && p != null) {
					if (k == "rsa") {
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
					} else if (k == "ed25519") {
						var decoded = Convert.FromBase64String (p);

						return new Ed25519PublicKeyParameters (decoded, 0);
					}
				}
			}

			throw new Exception (string.Format ("Failed to look up public key for: {0}", domain));
		}

		public Task<AsymmetricKeyParameter> LocatePublicKeyAsync (string methods, string domain, string selector, CancellationToken cancellationToken = default (CancellationToken))
		{
			return Task.FromResult (LocatePublicKey (methods, domain, selector, cancellationToken));
		}
	}
}

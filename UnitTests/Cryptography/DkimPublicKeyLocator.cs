//
// DkimPublicKeyLocator.cs
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

using Org.BouncyCastle.Crypto;

using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	class DkimPublicKeyLocator : DkimPublicKeyLocatorBase
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

		public override AsymmetricKeyParameter LocatePublicKey (string methods, string domain, string selector, CancellationToken cancellationToken = default)
		{
			var query = selector + "._domainkey." + domain;

			if (keys.TryGetValue (query, out string txt))
				return GetPublicKey (txt);

			throw new Exception (string.Format ("Failed to look up public key for: {0}", domain));
		}

		public override Task<AsymmetricKeyParameter> LocatePublicKeyAsync (string methods, string domain, string selector, CancellationToken cancellationToken = default)
		{
			return Task.FromResult (LocatePublicKey (methods, domain, selector, cancellationToken));
		}
	}
}

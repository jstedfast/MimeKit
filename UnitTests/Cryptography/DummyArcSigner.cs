//
// DummyArcSigner.cs
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

using MimeKit;
using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	public class DummyArcSigner : ArcSigner
	{
		public DummyArcSigner (Stream stream, string domain, string selector, DkimSignatureAlgorithm algorithm = DkimSignatureAlgorithm.RsaSha256) : base (stream, domain, selector, algorithm)
		{
		}

		public DummyArcSigner (string fileName, string domain, string selector, DkimSignatureAlgorithm algorithm = DkimSignatureAlgorithm.RsaSha256) : base (fileName, domain, selector, algorithm)
		{
		}

		public DummyArcSigner (AsymmetricKeyParameter key, string domain, string selector, DkimSignatureAlgorithm algorithm = DkimSignatureAlgorithm.RsaSha256) : base (key, domain, selector, algorithm)
		{
		}

		public IDkimPublicKeyLocator PublicKeyLocator {
			get; set;
		}

		public string SrvId {
			get; set;
		}

		public long Timestamp {
			get; set;
		}

		protected override AuthenticationResults GenerateArcAuthenticationResults (FormatOptions options, MimeMessage message, CancellationToken cancellationToken)
		{
			var results = new AuthenticationResults (SrvId);

			for (int i = 0; i < message.Headers.Count; i++) {
				var header = message.Headers[i];

				if (header.Id != HeaderId.AuthenticationResults)
					continue;

				if (!AuthenticationResults.TryParse (header.RawValue, out AuthenticationResults authres))
					continue;

				if (authres.AuthenticationServiceIdentifier != SrvId)
					continue;

				foreach (var result in authres.Results) {
					if (!results.Results.Any (r => r.Method == result.Method))
						results.Results.Add (result);
				}
			}

			return results;
		}

		protected override Task<AuthenticationResults> GenerateArcAuthenticationResultsAsync (FormatOptions options, MimeMessage message, CancellationToken cancellationToken)
		{
			return Task.FromResult (GenerateArcAuthenticationResults (options, message, cancellationToken));
		}

		protected override long GetTimestamp ()
		{
			base.GetTimestamp ();

			return Timestamp;
		}
	}
}

//
// BouncyCastleSecureMimeContextTests.cs
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

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.Smime;

using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class BouncyCastleSecureMimeContextTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentNullException> (() => BouncyCastleSecureMimeContext.TryGetDigestAlgorithm (null, out _), "TryGetDIgestAlgorithm");
			Assert.Throws<ArgumentNullException> (() => BouncyCastleSecureMimeContext.TryGetEncryptionAlgorithm (null, out _), "TryGetEncryptionAlgorithm");
		}

		[Test]
		public void TestTryGetEncryptionAlgorithm ()
		{
			EncryptionAlgorithm algorithm;

			Assert.That (BouncyCastleSecureMimeContext.TryGetEncryptionAlgorithm (new AlgorithmIdentifier (SecureMimeContext.Blowfish, DerNull.Instance), out algorithm), Is.True);
			Assert.That (algorithm, Is.EqualTo (EncryptionAlgorithm.Blowfish));

			Assert.That (BouncyCastleSecureMimeContext.TryGetEncryptionAlgorithm (new AlgorithmIdentifier (SecureMimeContext.Twofish, DerNull.Instance), out algorithm), Is.True);
			Assert.That (algorithm, Is.EqualTo (EncryptionAlgorithm.Twofish));

			Assert.That (BouncyCastleSecureMimeContext.TryGetEncryptionAlgorithm (new AlgorithmIdentifier (SmimeCapabilities.IdeaCbc, DerNull.Instance), out algorithm), Is.True);
			Assert.That (algorithm, Is.EqualTo (EncryptionAlgorithm.Idea));
		}
	}
}

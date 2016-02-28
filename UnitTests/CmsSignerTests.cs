//
// CmsSignerTests.cs
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
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

using NUnit.Framework;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;

using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

using MimeKit.Cryptography;

namespace UnitTests {
	[TestFixture]
	public class CmsSignerTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var signer = new CmsSigner (Path.Combine ("..", "..", "TestData", "smime", "smime.p12"), "no.secret");

			Assert.Throws<ArgumentNullException> (() => new CmsSigner ((IEnumerable<X509Certificate>) null, signer.PrivateKey));
			Assert.Throws<ArgumentNullException> (() => new CmsSigner (signer.CertificateChain, null));

			Assert.Throws<ArgumentNullException> (() => new CmsSigner ((X509Certificate) null, signer.PrivateKey));
			Assert.Throws<ArgumentNullException> (() => new CmsSigner (signer.Certificate, null));

			Assert.Throws<ArgumentNullException> (() => new CmsSigner ((X509Certificate2) null));

			Assert.Throws<ArgumentNullException> (() => new CmsSigner ((Stream) null, "password"));
			Assert.Throws<ArgumentNullException> (() => new CmsSigner (Stream.Null, null));

			Assert.Throws<ArgumentNullException> (() => new CmsSigner ((string) null, "password"));
			Assert.Throws<ArgumentNullException> (() => new CmsSigner ("fileName", null));
		}
	}
}

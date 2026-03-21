//
// DigitalSignatureVerifyExceptionTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
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

using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class DigitalSignatureVerifyExceptionTests
	{
		[Test]
		public void TestConstructors ()
		{
			Exception innerException = new Exception ("message");
			DigitalSignatureVerifyException ex;

			ex = new DigitalSignatureVerifyException ("message");
			Assert.That (ex.KeyId, Is.Null, "new DigitalSignatureVerifyException (message)");

			ex = new DigitalSignatureVerifyException ("message", innerException);
			Assert.That (ex.KeyId, Is.Null, "new DigitalSignatureVerifyException (message, innerException)");

			ex = new DigitalSignatureVerifyException (12345, "message");
			Assert.That (ex.KeyId, Is.EqualTo (12345), "new DigitalSignatureVerifyException (keyId, message)");

			ex = new DigitalSignatureVerifyException (12345, "message", innerException);
			Assert.That (ex.KeyId, Is.EqualTo (12345), "new DigitalSignatureVerifyException (keyId, message, innerException)");
		}
	}
}

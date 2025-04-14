//
// ExceptionTests.cs
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

#if NETFRAMEWORK

#pragma warning disable SYSLIB0011

using System.Runtime.Serialization.Formatters.Binary;

using MimeKit;
using MimeKit.Tnef;
using MimeKit.Cryptography;

namespace UnitTests {
	[TestFixture]
	public class ExceptionTests
	{
		[Test]
		public void TestParseException ()
		{
			try {
				var expected = new ParseException ("Message", 17, 22);

				using (var stream = new MemoryStream ()) {
					var formatter = new BinaryFormatter ();
					formatter.Serialize (stream, expected);
					stream.Position = 0;

					var ex = (ParseException) formatter.Deserialize (stream);
					Assert.That (ex.TokenIndex, Is.EqualTo (expected.TokenIndex), "Unexpected TokenIndex.");
					Assert.That (ex.ErrorIndex, Is.EqualTo (expected.ErrorIndex), "Unexpected ErrorIndex.");
				}
			} catch (NotSupportedException) {
				Assert.Ignore ("BinaryFormatter is no longer supported in .NET Core.");
			}
		}

		[Test]
		public void TestCertificateNotFoundException ()
		{
			try {
				var expected = new CertificateNotFoundException (new MailboxAddress ("Unit Tests", "example@mimekit.net"), "Message");

				using (var stream = new MemoryStream ()) {
					var formatter = new BinaryFormatter ();
					formatter.Serialize (stream, expected);
					stream.Position = 0;

					var ex = (CertificateNotFoundException) formatter.Deserialize (stream);
					Assert.That (ex.Mailbox, Is.EqualTo (expected.Mailbox), "Unexpected Mailbox.");
				}
			} catch (NotSupportedException) {
				Assert.Ignore ("BinaryFormatter is no longer supported in .NET Core.");
			}
		}

		[Test]
		public void TestDigitalSignatureVerifyException ()
		{
			try {
				var expected = new DigitalSignatureVerifyException (0xdeadbeef, "Message", new Exception ("InnerException"));

				using (var stream = new MemoryStream ()) {
					var formatter = new BinaryFormatter ();
					formatter.Serialize (stream, expected);
					stream.Position = 0;

					var ex = (DigitalSignatureVerifyException) formatter.Deserialize (stream);
					Assert.That (ex.KeyId, Is.EqualTo (expected.KeyId), "Unexpected KeyId.");
				}

				expected = new DigitalSignatureVerifyException ("Message", new Exception ("InnerException"));

				using (var stream = new MemoryStream ()) {
					var formatter = new BinaryFormatter ();
					formatter.Serialize (stream, expected);
					stream.Position = 0;

					var ex = (DigitalSignatureVerifyException) formatter.Deserialize (stream);
					Assert.That (ex.KeyId, Is.EqualTo (expected.KeyId), "Unexpected KeyId.");
				}

				expected = new DigitalSignatureVerifyException (0xdeadbeef, "Message");

				using (var stream = new MemoryStream ()) {
					var formatter = new BinaryFormatter ();
					formatter.Serialize (stream, expected);
					stream.Position = 0;

					var ex = (DigitalSignatureVerifyException) formatter.Deserialize (stream);
					Assert.That (ex.KeyId, Is.EqualTo (expected.KeyId), "Unexpected KeyId.");
				}

				expected = new DigitalSignatureVerifyException ("Message");

				using (var stream = new MemoryStream ()) {
					var formatter = new BinaryFormatter ();
					formatter.Serialize (stream, expected);
					stream.Position = 0;

					var ex = (DigitalSignatureVerifyException) formatter.Deserialize (stream);
					Assert.That (ex.KeyId, Is.EqualTo (expected.KeyId), "Unexpected KeyId.");
				}
			} catch (NotSupportedException) {
				Assert.Ignore ("BinaryFormatter is no longer supported in .NET Core.");
			}
		}

		static void TestPrivateKeyNotFoundException (PrivateKeyNotFoundException expected)
		{
			try {
				using (var stream = new MemoryStream ()) {
					var formatter = new BinaryFormatter ();
					formatter.Serialize (stream, expected);
					stream.Position = 0;

					var ex = (PrivateKeyNotFoundException) formatter.Deserialize (stream);
					Assert.That (ex.KeyId, Is.EqualTo (expected.KeyId), "Unexpected KeyId.");
				}
			} catch (NotSupportedException) {
				Assert.Ignore ("BinaryFormatter is no longer supported in .NET Core.");
			}
		}

		[Test]
		public void TestPrivateKeyNotFoundException ()
		{
			TestPrivateKeyNotFoundException (new PrivateKeyNotFoundException (new MailboxAddress ("Unit Tests", "example@mimekit.net"), "Message"));
			TestPrivateKeyNotFoundException (new PrivateKeyNotFoundException ("DEADBEEF", "Message"));
			TestPrivateKeyNotFoundException (new PrivateKeyNotFoundException (0xdeadbeef, "Message"));

			Assert.Throws<ArgumentNullException> (() => new PrivateKeyNotFoundException ((string) null, "Message"));
			Assert.Throws<ArgumentNullException> (() => new PrivateKeyNotFoundException ((MailboxAddress) null, "Message"));
		}

		[Test]
		public void TestPublicKeyNotFoundException ()
		{
			try {
				var expected = new PublicKeyNotFoundException (new MailboxAddress ("Unit Tests", "example@mimekit.net"), "Message");

				using (var stream = new MemoryStream ()) {
					var formatter = new BinaryFormatter ();
					formatter.Serialize (stream, expected);
					stream.Position = 0;

					var ex = (PublicKeyNotFoundException) formatter.Deserialize (stream);
					Assert.That (ex.Mailbox, Is.EqualTo (expected.Mailbox), "Unexpected Mailbox.");
				}
			} catch (NotSupportedException) {
				Assert.Ignore ("BinaryFormatter is no longer supported in .NET Core.");
			}
		}

		[Test]
		public void TestTnefException ()
		{
			try {
				var expected = new TnefException (TnefComplianceStatus.AttributeOverflow, "Message", new Exception ("InnerException"));

				using (var stream = new MemoryStream ()) {
					var formatter = new BinaryFormatter ();
					formatter.Serialize (stream, expected);
					stream.Position = 0;

					var ex = (TnefException) formatter.Deserialize (stream);
					Assert.That (ex.Error, Is.EqualTo (expected.Error), "Unexpected Error.");
				}

				expected = new TnefException (TnefComplianceStatus.AttributeOverflow, "Message");

				using (var stream = new MemoryStream ()) {
					var formatter = new BinaryFormatter ();
					formatter.Serialize (stream, expected);
					stream.Position = 0;

					var ex = (TnefException) formatter.Deserialize (stream);
					Assert.That (ex.Error, Is.EqualTo (expected.Error), "Unexpected Error.");
				}
			} catch (NotSupportedException) {
				Assert.Ignore ("BinaryFormatter is no longer supported in .NET Core.");
			}
		}
	}
}

#pragma warning restore SYSLIB0011

#endif // NETFRAMEWORK || NET6_0

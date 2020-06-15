//
// ExceptionTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 .NET Foundation and Contributors
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
using System.Runtime.Serialization.Formatters.Binary;

using NUnit.Framework;

using MimeKit;
using MimeKit.Tnef;
using MimeKit.Cryptography;

namespace UnitTests
{
	[TestFixture]
	public class ExceptionTests
	{
		[Test]
		public void TestParseException ()
		{
			var expected = new ParseException ("Message", 17, 22);

			using (var stream = new MemoryStream ()) {
				var formatter = new BinaryFormatter ();
				formatter.Serialize (stream, expected);
				stream.Position = 0;

				var ex = (ParseException) formatter.Deserialize (stream);
				Assert.AreEqual (expected.TokenIndex, ex.TokenIndex, "Unexpected TokenIndex.");
				Assert.AreEqual (expected.ErrorIndex, ex.ErrorIndex, "Unexpected ErrorIndex.");
			}
		}

		[Test]
		public void TestCertificateNotFoundException ()
		{
			var expected = new CertificateNotFoundException (new MailboxAddress ("Unit Tests", "example@mimekit.net"), "Message");

			using (var stream = new MemoryStream ()) {
				var formatter = new BinaryFormatter ();
				formatter.Serialize (stream, expected);
				stream.Position = 0;

				var ex = (CertificateNotFoundException) formatter.Deserialize (stream);
				Assert.IsTrue (expected.Mailbox.Equals (ex.Mailbox), "Unexpected Mailbox.");
			}
		}

		[Test]
		public void TestDigitalSignatureVerifyException ()
		{
			var expected = new DigitalSignatureVerifyException (0xdeadbeef, "Message", new Exception ("InnerException"));

			using (var stream = new MemoryStream ()) {
				var formatter = new BinaryFormatter ();
				formatter.Serialize (stream, expected);
				stream.Position = 0;

				var ex = (DigitalSignatureVerifyException) formatter.Deserialize (stream);
				Assert.AreEqual (expected.KeyId, ex.KeyId, "Unexpected KeyId.");
			}

			expected = new DigitalSignatureVerifyException ("Message", new Exception ("InnerException"));

			using (var stream = new MemoryStream ()) {
				var formatter = new BinaryFormatter ();
				formatter.Serialize (stream, expected);
				stream.Position = 0;

				var ex = (DigitalSignatureVerifyException) formatter.Deserialize (stream);
				Assert.AreEqual (expected.KeyId, ex.KeyId, "Unexpected KeyId.");
			}

			expected = new DigitalSignatureVerifyException (0xdeadbeef, "Message");

			using (var stream = new MemoryStream ()) {
				var formatter = new BinaryFormatter ();
				formatter.Serialize (stream, expected);
				stream.Position = 0;

				var ex = (DigitalSignatureVerifyException) formatter.Deserialize (stream);
				Assert.AreEqual (expected.KeyId, ex.KeyId, "Unexpected KeyId.");
			}

			expected = new DigitalSignatureVerifyException ("Message");

			using (var stream = new MemoryStream ()) {
				var formatter = new BinaryFormatter ();
				formatter.Serialize (stream, expected);
				stream.Position = 0;

				var ex = (DigitalSignatureVerifyException) formatter.Deserialize (stream);
				Assert.AreEqual (expected.KeyId, ex.KeyId, "Unexpected KeyId.");
			}
		}

		static void TestPrivateKeyNotFoundException (PrivateKeyNotFoundException expected)
		{
			using (var stream = new MemoryStream ()) {
				var formatter = new BinaryFormatter ();
				formatter.Serialize (stream, expected);
				stream.Position = 0;

				var ex = (PrivateKeyNotFoundException) formatter.Deserialize (stream);
				Assert.AreEqual (expected.KeyId, ex.KeyId, "Unexpected KeyId.");
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
			var expected = new PublicKeyNotFoundException (new MailboxAddress ("Unit Tests", "example@mimekit.net"), "Message");

			using (var stream = new MemoryStream ()) {
				var formatter = new BinaryFormatter ();
				formatter.Serialize (stream, expected);
				stream.Position = 0;

				var ex = (PublicKeyNotFoundException) formatter.Deserialize (stream);
				Assert.IsTrue (expected.Mailbox.Equals (ex.Mailbox), "Unexpected Mailbox.");
			}
		}

		[Test]
		public void TestTnefException ()
		{
			var expected = new TnefException (TnefComplianceStatus.AttributeOverflow, "Message", new Exception ("InnerException"));

			using (var stream = new MemoryStream ()) {
				var formatter = new BinaryFormatter ();
				formatter.Serialize (stream, expected);
				stream.Position = 0;

				var ex = (TnefException) formatter.Deserialize (stream);
				Assert.AreEqual (expected.Error, ex.Error, "Unexpected Error.");
			}

			expected = new TnefException (TnefComplianceStatus.AttributeOverflow, "Message");

			using (var stream = new MemoryStream ()) {
				var formatter = new BinaryFormatter ();
				formatter.Serialize (stream, expected);
				stream.Position = 0;

				var ex = (TnefException) formatter.Deserialize (stream);
				Assert.AreEqual (expected.Error, ex.Error, "Unexpected Error.");
			}
		}
	}
}

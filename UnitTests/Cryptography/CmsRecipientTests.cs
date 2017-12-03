//
// CmsRecipientTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2017 Xamarin Inc. (www.xamarin.com)
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
using System.Security.Cryptography.X509Certificates;

using NUnit.Framework;

using Org.BouncyCastle.X509;

using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class CmsRecipientTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentNullException> (() => new CmsRecipient ((X509Certificate2) null));
			Assert.Throws<ArgumentNullException> (() => new CmsRecipient ((X509Certificate) null));
			Assert.Throws<ArgumentNullException> (() => new CmsRecipient ((Stream) null));
			Assert.Throws<ArgumentNullException> (() => new CmsRecipient ((string) null));

			var recipients = new CmsRecipientCollection ();
			Assert.AreEqual (0, recipients.Count);
			Assert.IsFalse (recipients.IsReadOnly);
			Assert.Throws<ArgumentNullException> (() => recipients.Add (null));
			Assert.Throws<ArgumentNullException> (() => recipients.Contains (null));
			Assert.Throws<ArgumentNullException> (() => recipients.CopyTo (null, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => recipients.CopyTo (new CmsRecipient[1], -1));
			Assert.Throws<ArgumentOutOfRangeException> (() => recipients.CopyTo (new CmsRecipient[1], 2));
			Assert.Throws<ArgumentNullException> (() => recipients.Remove (null));
		}

		static void AssertDefaultValues (CmsRecipient recipient, X509Certificate certificate)
		{
			Assert.AreEqual (certificate, recipient.Certificate);
			Assert.AreEqual (1, recipient.EncryptionAlgorithms.Length);
			Assert.AreEqual (EncryptionAlgorithm.TripleDes, recipient.EncryptionAlgorithms[0]);
			Assert.AreEqual (SubjectIdentifierType.IssuerAndSerialNumber, recipient.RecipientIdentifierType);
		}

		[Test]
		public void TestDefaultValues ()
		{
			var path = Path.Combine ("..", "..", "TestData", "smime", "certificate-authority.crt");
			var recipient = new CmsRecipient (path);
			var certificate = recipient.Certificate;

			AssertDefaultValues (recipient, certificate);

			using (var stream = File.OpenRead (path))
				recipient = new CmsRecipient (stream);

			AssertDefaultValues (recipient, certificate);

			recipient = new CmsRecipient (certificate);

			AssertDefaultValues (recipient, certificate);

			recipient = new CmsRecipient (new X509Certificate2 (path));

			AssertDefaultValues (recipient, certificate);
		}
	}
}

//
// CmsRecipientTests.cs
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

using System.Security.Cryptography.X509Certificates;

using MimeKit.Cryptography;

using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

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

			Assert.That (recipients.Count, Is.EqualTo (0));
			Assert.That (recipients.IsReadOnly, Is.False);
			Assert.Throws<ArgumentNullException> (() => recipients.Add (null));
			Assert.Throws<ArgumentNullException> (() => recipients.Contains (null));
			Assert.Throws<ArgumentNullException> (() => recipients.CopyTo (null, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => recipients.CopyTo (new CmsRecipient[1], -1));
			Assert.Throws<ArgumentOutOfRangeException> (() => recipients.CopyTo (new CmsRecipient[1], 2));
			Assert.Throws<ArgumentNullException> (() => recipients.Remove (null));
		}

		static void AssertDefaultValues (CmsRecipient recipient, X509Certificate certificate)
		{
			Assert.That (recipient.Certificate, Is.EqualTo (certificate), "Certificate");
			Assert.That (recipient.EncryptionAlgorithms.Length, Is.EqualTo (1), "EncryptionAlgorithms");
			Assert.That (recipient.EncryptionAlgorithms[0], Is.EqualTo (EncryptionAlgorithm.TripleDes), "EncryptionAlgorithm");
			Assert.That (recipient.RecipientIdentifierType, Is.EqualTo (SubjectIdentifierType.IssuerAndSerialNumber), "RecipientIdentifierType");
			Assert.That (recipient.RsaEncryptionPadding, Is.Null, "RsaEncryptionPadding");
		}

		[Test]
		public void TestDefaultValues ()
		{
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "StartComCertificationAuthority.crt");
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

		[Test]
		public void TestRecipientIdentifierType ()
		{
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "StartComCertificationAuthority.crt");
			var recipient = new CmsRecipient (path, SubjectIdentifierType.SubjectKeyIdentifier);
			var certificate = recipient.Certificate;

			Assert.That (recipient.RecipientIdentifierType, Is.EqualTo (SubjectIdentifierType.SubjectKeyIdentifier));

			using (var stream = File.OpenRead (path))
				recipient = new CmsRecipient (stream, SubjectIdentifierType.SubjectKeyIdentifier);
			Assert.That (recipient.RecipientIdentifierType, Is.EqualTo (SubjectIdentifierType.SubjectKeyIdentifier));

			recipient = new CmsRecipient (certificate, SubjectIdentifierType.SubjectKeyIdentifier);
			Assert.That (recipient.RecipientIdentifierType, Is.EqualTo (SubjectIdentifierType.SubjectKeyIdentifier));

			recipient = new CmsRecipient (new X509Certificate2 (File.ReadAllBytes (path)), SubjectIdentifierType.SubjectKeyIdentifier);
			Assert.That (recipient.RecipientIdentifierType, Is.EqualTo (SubjectIdentifierType.SubjectKeyIdentifier));
		}

		[Test]
		public void TestCollectionAddRemove ()
		{
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "StartComCertificationAuthority.crt");
			var recipients = new CmsRecipientCollection ();
			var recipient = new CmsRecipient (path);
			var array = new CmsRecipient[1];

			Assert.That (recipients.Contains (recipient), Is.False, "Contains: False");
			Assert.That (recipients.Remove (recipient), Is.False, "Remove: False");

			recipients.Add (recipient);

			Assert.That (recipients.Count, Is.EqualTo (1), "Count");
			Assert.That (recipients.Contains (recipient), Is.True, "Contains: True");

			recipients.CopyTo (array, 0);
			Assert.That (array[0], Is.EqualTo (recipient), "CopyTo");

			Assert.That (recipients.Remove (recipient), Is.True, "Remove: True");

			Assert.That (recipients.Count, Is.EqualTo (0), "Count");

			recipients.Clear ();
		}
	}
}

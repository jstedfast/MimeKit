//
// ApplicationPkcs7MimeTests.cs
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
using System.Collections.Generic;

using NUnit.Framework;

using Org.BouncyCastle.X509;

using MimeKit;
using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class ApplicationPkcs7MimeTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", "smime", "smime.p12");
			var entity = new TextPart ("plain") { Text = "This is some text..." };
			var mailbox = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");
			var recipients = new CmsRecipientCollection ();
			var signer = new CmsSigner (path, "no.secret");
			var mailboxes = new [] { mailbox };

			recipients.Add (new CmsRecipient (signer.Certificate));

			using (var ctx = new TemporarySecureMimeContext ()) {
				ctx.Import (path, "no.secret");

				// Compress
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Compress (null, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Compress (ctx, null));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Compress (null));

				// Encrypt
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Encrypt (null, mailboxes, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Encrypt (null, recipients, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Encrypt (ctx, (IEnumerable<MailboxAddress>) null, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Encrypt (ctx, (CmsRecipientCollection) null, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Encrypt (ctx, recipients, null));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Encrypt (ctx, mailboxes, null));

				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Encrypt ((IEnumerable<MailboxAddress>) null, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Encrypt ((CmsRecipientCollection) null, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Encrypt (recipients, null));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Encrypt (mailboxes, null));

				// Sign
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Sign (null, mailbox, DigestAlgorithm.Sha1, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Sign (null, signer, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Sign (ctx, (MailboxAddress) null, DigestAlgorithm.Sha1, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Sign (ctx, (CmsSigner) null, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Sign (ctx, mailbox, DigestAlgorithm.Sha1, null));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Sign (ctx, signer, null));

				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Sign ((MailboxAddress) null, DigestAlgorithm.Sha1, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Sign ((CmsSigner) null, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Sign (mailbox, DigestAlgorithm.Sha1, null));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.Sign (signer, null));

				// SignAndEncrypt
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (null, mailbox, DigestAlgorithm.Sha1, mailboxes, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (ctx, null, DigestAlgorithm.Sha1, mailboxes, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (ctx, mailbox, DigestAlgorithm.Sha1, null, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (ctx, mailbox, DigestAlgorithm.Sha1, mailboxes, null));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (null, DigestAlgorithm.Sha1, mailboxes, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (mailbox, DigestAlgorithm.Sha1, null, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (mailbox, DigestAlgorithm.Sha1, mailboxes, null));

				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (null, signer, recipients, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (ctx, null, recipients, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (ctx, signer, null, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (ctx, signer, recipients, null));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (null, recipients, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (signer, null, entity));
				Assert.Throws<ArgumentNullException> (() => ApplicationPkcs7Mime.SignAndEncrypt (signer, recipients, null));

				var compressed = ApplicationPkcs7Mime.Compress (ctx, entity);
				var encrypted = ApplicationPkcs7Mime.Encrypt (recipients, entity);
				var signed = ApplicationPkcs7Mime.Sign (signer, entity);

				// Decompress
				Assert.Throws<ArgumentNullException> (() => compressed.Decompress (null));
				Assert.Throws<InvalidOperationException> (() => encrypted.Decompress (ctx));
				Assert.Throws<InvalidOperationException> (() => signed.Decompress (ctx));

				// Decrypt
				Assert.Throws<ArgumentNullException> (() => encrypted.Decrypt (null));
				Assert.Throws<InvalidOperationException> (() => compressed.Decrypt (ctx));
				Assert.Throws<InvalidOperationException> (() => signed.Decrypt (ctx));

				// Verify
				Assert.Throws<ArgumentNullException> (() => {
					MimeEntity mime;

					signed.Verify (null, out mime);
				});
				Assert.Throws<InvalidOperationException> (() => {
					MimeEntity mime;

					compressed.Verify (ctx, out mime);
				});
				Assert.Throws<InvalidOperationException> (() => {
					MimeEntity mime;

					encrypted.Verify (ctx, out mime);
				});
			}
		}
	}
}

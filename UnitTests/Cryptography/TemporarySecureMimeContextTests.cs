//
// TemporarySecureMimeContextTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2023 .NET Foundation and Contributors
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

using MimeKit;
using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class TemporarySecureMimeContextTests
	{
		[Test]
		public void TestImportX509Certificate2 ()
		{
			var dataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "smime");
			var certificate = new X509Certificate2 (Path.Combine (dataDir, "smime.pfx"), "no.secret", X509KeyStorageFlags.Exportable);

			using (var ctx = new TemporarySecureMimeContext ()) {
				var secure = new SecureMailboxAddress ("MimeKit UnitTests", "mimekit@example.com", certificate.Thumbprint);
				var mailbox = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");

				ctx.Import (certificate);

				// Check that the certificate exists in the context
				Assert.IsTrue (ctx.CanSign (mailbox), "CanSign(MailboxAddress)");
				Assert.IsTrue (ctx.CanEncrypt (mailbox), "CanEncrypt(MailboxAddress)");
				Assert.IsTrue (ctx.CanSign (secure), "CanSign(SecureMailboxAddress)");
				Assert.IsTrue (ctx.CanEncrypt (secure), "CanEncrypt(SecureMailboxAddress)");
			}
		}

		[Test]
		public async Task TestImportX509Certificate2Async ()
		{
			var dataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "smime");
			var certificate = new X509Certificate2 (Path.Combine (dataDir, "smime.pfx"), "no.secret", X509KeyStorageFlags.Exportable);

			using (var ctx = new TemporarySecureMimeContext ()) {
				var secure = new SecureMailboxAddress ("MimeKit UnitTests", "mimekit@example.com", certificate.Thumbprint);
				var mailbox = new MailboxAddress ("MimeKit UnitTests", "mimekit@example.com");

				await ctx.ImportAsync (certificate);

				// Check that the certificate exists in the context
				Assert.IsTrue (await ctx.CanSignAsync (mailbox), "CanSign(MailboxAddress)");
				Assert.IsTrue (await ctx.CanEncryptAsync (mailbox), "CanEncrypt(MailboxAddress)");
				Assert.IsTrue (await ctx.CanSignAsync (secure), "CanSign(SecureMailboxAddress)");
				Assert.IsTrue (await ctx.CanEncryptAsync (secure), "CanEncrypt(SecureMailboxAddress)");
			}
		}
	}
}

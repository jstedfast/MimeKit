//
// CryptographyContextTests.cs
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

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.Smime;
using MimeKit;

namespace UnitTests.Cryptography
{
	[TestFixture]
	public class CryptographyContextTests
	{
		class TestableCryptographyContext : TemporarySecureMimeContext
		{
			public void SetEncryptionAlgorithmRank (EncryptionAlgorithm[] rankedAlgorithms)
			{
				EncryptionAlgorithmRank = rankedAlgorithms;
			}

			public void SetDigestAlgorithmRank (DigestAlgorithm[] rankedAlgorithms)
			{
				DigestAlgorithmRank = rankedAlgorithms;
			}
		}

		class NoParameterlessCtorContext : TemporarySecureMimeContext
		{
			public NoParameterlessCtorContext (int arg1, string arg2) : base ()
			{
			}
		}

		class UnknownCryprographyContext : CryptographyContext
		{
			public override string SignatureProtocol => throw new NotImplementedException ();

			public override string EncryptionProtocol => throw new NotImplementedException ();

			public override string KeyExchangeProtocol => throw new NotImplementedException ();

			public override bool CanEncrypt (MailboxAddress mailbox, CancellationToken cancellationToken = default (CancellationToken))
			{
				throw new NotImplementedException ();
			}

			public override bool CanSign (MailboxAddress signer, CancellationToken cancellationToken = default (CancellationToken))
			{
				throw new NotImplementedException ();
			}

			public override MimeEntity Decrypt (Stream encryptedData, CancellationToken cancellationToken = default (CancellationToken))
			{
				throw new NotImplementedException ();
			}

			public override Task<MimeEntity> DecryptAsync (Stream encryptedData, CancellationToken cancellationToken = default (CancellationToken))
			{
				throw new NotImplementedException ();
			}

			public override MimePart Encrypt (IEnumerable<MailboxAddress> recipients, Stream content, CancellationToken cancellationToken = default (CancellationToken))
			{
				throw new NotImplementedException ();
			}

			public override Task<MimePart> EncryptAsync (IEnumerable<MailboxAddress> recipients, Stream content, CancellationToken cancellationToken = default (CancellationToken))
			{
				throw new NotImplementedException ();
			}

			public override MimePart Export (IEnumerable<MailboxAddress> mailboxes, CancellationToken cancellationToken = default (CancellationToken))
			{
				throw new NotImplementedException ();
			}

			public override Task<MimePart> ExportAsync (IEnumerable<MailboxAddress> mailboxes, CancellationToken cancellationToken = default (CancellationToken))
			{
				throw new NotImplementedException ();
			}

			public override DigestAlgorithm GetDigestAlgorithm (string micalg)
			{
				throw new NotImplementedException ();
			}

			public override string GetDigestAlgorithmName (DigestAlgorithm micalg)
			{
				throw new NotImplementedException ();
			}

			public override void Import (Stream stream, CancellationToken cancellationToken = default (CancellationToken))
			{
				throw new NotImplementedException ();
			}

			public override Task ImportAsync (Stream stream, CancellationToken cancellationToken = default (CancellationToken))
			{
				throw new NotImplementedException ();
			}

			public override MimePart Sign (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content, CancellationToken cancellationToken = default (CancellationToken))
			{
				throw new NotImplementedException ();
			}

			public override Task<MimePart> SignAsync (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content, CancellationToken cancellationToken = default (CancellationToken))
			{
				throw new NotImplementedException ();
			}

			public override bool Supports (string protocol)
			{
				throw new NotImplementedException ();
			}

			public override DigitalSignatureCollection Verify (Stream content, Stream signatureData, CancellationToken cancellationToken = default (CancellationToken))
			{
				throw new NotImplementedException ();
			}

			public override Task<DigitalSignatureCollection> VerifyAsync (Stream content, Stream signatureData, CancellationToken cancellationToken = default (CancellationToken))
			{
				throw new NotImplementedException ();
			}
		}

		[Test]
		public void TestArgumentExceptions ()
		{
			var ctx = new TestableCryptographyContext ();

			Assert.Throws<ArgumentNullException> (() => ctx.SetEncryptionAlgorithmRank (null));
			Assert.Throws<ArgumentException> (() => ctx.SetEncryptionAlgorithmRank (Array.Empty<EncryptionAlgorithm> ()));

			Assert.Throws<ArgumentNullException> (() => ctx.SetDigestAlgorithmRank (null));
			Assert.Throws<ArgumentException> (() => ctx.SetDigestAlgorithmRank (Array.Empty<DigestAlgorithm> ()));

			Assert.Throws<ArgumentException> (() => CryptographyContext.Register (typeof (NoParameterlessCtorContext)));
			Assert.Throws<ArgumentException> (() => CryptographyContext.Register (typeof (UnknownCryprographyContext)));
		}

		[Test]
		public void TestEnableDisableEncryptionAlgorithms ()
		{
			const EncryptionAlgorithm algorithm = EncryptionAlgorithm.TripleDes;
			var ctx = new TemporarySecureMimeContext ();
			EncryptionAlgorithm[] algorithms;
			int previousLength;

			Assert.That (ctx.IsEnabled (algorithm), Is.True, $"{algorithm} is enabled by default");

			algorithms = ctx.EnabledEncryptionAlgorithms;
			Assert.That (algorithms, Has.Length.GreaterThanOrEqualTo (1), "At least 1 algorithm enabled by default");
			Assert.That (algorithms, Contains.Item (algorithm), $"{algorithm} should be in the list of enabled algorithms");

			previousLength = algorithms.Length;
			ctx.Disable (algorithm);

			Assert.That (ctx.IsEnabled (algorithm), Is.False, $"{algorithm} should be disabled");

			algorithms = ctx.EnabledEncryptionAlgorithms;
			Assert.That (algorithms, Has.Length.EqualTo (previousLength - 1), "There should be 1 less enabled algorithm");
			Assert.That (algorithms, Does.Not.Contain (algorithm), $"{algorithm} should no longer be in the enabled list");
		}

		[Test]
		public void TestEnableDisableDigestAlgorithms ()
		{
			const DigestAlgorithm algorithm = DigestAlgorithm.Sha1;
			var ctx = new TemporarySecureMimeContext ();
			DigestAlgorithm[] algorithms;
			int previousLength;

			Assert.That (ctx.IsEnabled (algorithm), Is.True, $"{algorithm} is enabled by default");

			algorithms = ctx.EnabledDigestAlgorithms;
			Assert.That (algorithms, Has.Length.GreaterThanOrEqualTo (1), "At least 1 algorithm enabled by default");
			Assert.That (algorithms, Contains.Item (algorithm), $"{algorithm} should be in the list of enabled algorithms");

			previousLength = algorithms.Length;
			ctx.Disable (algorithm);

			Assert.That (ctx.IsEnabled (algorithm), Is.False, $"{algorithm} should be disabled");

			algorithms = ctx.EnabledDigestAlgorithms;
			Assert.That (algorithms, Has.Length.EqualTo (previousLength - 1), "There should be 1 less enabled algorithm");
			Assert.That (algorithms, Does.Not.Contain (algorithm), $"{algorithm} should no longer be in the enabled list");
		}
	}
}

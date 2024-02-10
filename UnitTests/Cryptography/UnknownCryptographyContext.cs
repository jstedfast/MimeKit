//
// UnknownCryptographyContext.cs
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

using MimeKit;
using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	class UnknownCryptographyContext : CryptographyContext
	{
		public override string SignatureProtocol => throw new System.NotImplementedException ();

		public override string EncryptionProtocol => throw new System.NotImplementedException ();

		public override string KeyExchangeProtocol => throw new System.NotImplementedException ();

		public override bool CanEncrypt (MailboxAddress mailbox, CancellationToken cancellationToken = default)
		{
			throw new System.NotImplementedException ();
		}

		public override bool CanSign (MailboxAddress signer, CancellationToken cancellationToken = default)
		{
			throw new System.NotImplementedException ();
		}

		public override MimeEntity Decrypt (Stream encryptedData, CancellationToken cancellationToken = default)
		{
			throw new System.NotImplementedException ();
		}

		public override Task<MimeEntity> DecryptAsync (Stream encryptedData, CancellationToken cancellationToken = default)
		{
			throw new System.NotImplementedException ();
		}

		public override MimePart Encrypt (IEnumerable<MailboxAddress> recipients, Stream content, CancellationToken cancellationToken = default)
		{
			throw new System.NotImplementedException ();
		}

		public override Task<MimePart> EncryptAsync (IEnumerable<MailboxAddress> recipients, Stream content, CancellationToken cancellationToken = default)
		{
			throw new System.NotImplementedException ();
		}

		public override MimePart Export (IEnumerable<MailboxAddress> mailboxes, CancellationToken cancellationToken = default)
		{
			throw new System.NotImplementedException ();
		}

		public override Task<MimePart> ExportAsync (IEnumerable<MailboxAddress> mailboxes, CancellationToken cancellationToken = default)
		{
			throw new System.NotImplementedException ();
		}

		public override DigestAlgorithm GetDigestAlgorithm (string micalg)
		{
			throw new System.NotImplementedException ();
		}

		public override string GetDigestAlgorithmName (DigestAlgorithm micalg)
		{
			throw new System.NotImplementedException ();
		}

		public override void Import (Stream stream, CancellationToken cancellationToken = default)
		{
			throw new System.NotImplementedException ();
		}

		public override Task ImportAsync (Stream stream, CancellationToken cancellationToken = default)
		{
			throw new System.NotImplementedException ();
		}

		public override MimePart Sign (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content, CancellationToken cancellationToken = default)
		{
			throw new System.NotImplementedException ();
		}

		public override Task<MimePart> SignAsync (MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content, CancellationToken cancellationToken = default)
		{
			throw new System.NotImplementedException ();
		}

		public override bool Supports (string protocol)
		{
			throw new System.NotImplementedException ();
		}

		public override DigitalSignatureCollection Verify (Stream content, Stream signatureData, CancellationToken cancellationToken = default)
		{
			throw new System.NotImplementedException ();
		}

		public override Task<DigitalSignatureCollection> VerifyAsync (Stream content, Stream signatureData, CancellationToken cancellationToken = default)
		{
			throw new System.NotImplementedException ();
		}
	}
}

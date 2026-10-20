//
// BouncyCastleSecureMimeContextTests.cs
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

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.Smime;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;

using MimeKit;
using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class BouncyCastleSecureMimeContextTests
	{
		static byte[] Compress (byte[] content)
		{
			var generator = new CmsCompressedDataGenerator ();
			return generator.Generate (new CmsProcessableByteArray (content), CmsCompressedDataGenerator.ZLib).GetEncoded ();
		}

		static async Task AssertDecompressionLimitAsync (bool doAsync, bool entity, int delta)
		{
			var content = System.Text.Encoding.ASCII.GetBytes ("Content-Type: text/plain\r\n\r\n" + new string ('A', 8192));
			using var input = new MemoryStream (Compress (content));
			using var output = new MemoryStream ();
			using var ctx = new TemporarySecureMimeContext ();
			Assert.That (ctx.MaxDecompressedSize, Is.EqualTo (64 * 1024 * 1024));
			ctx.MaxDecompressedSize = content.Length + delta;

			async Task DecompressAsync ()
			{
				if (entity) {
					using var decompressed = doAsync ? await ctx.DecompressAsync (input) : ctx.Decompress (input);
					Assert.That (((TextPart) decompressed).Text, Is.EqualTo (new string ('A', 8192)));
				} else {
					if (doAsync)
						await ctx.DecompressToAsync (input, output);
					else
						ctx.DecompressTo (input, output);
					Assert.That (output.ToArray (), Is.EqualTo (content));
				}
			}

			if (delta < 0) {
				Assert.ThrowsAsync<FormatException> (DecompressAsync);
				Assert.That (output.Length, Is.LessThanOrEqualTo (ctx.MaxDecompressedSize));
			} else {
				await DecompressAsync ();
			}
		}

		[Test]
		public void TestDecompressionLimit ([Values (false, true)] bool entity, [Values (-1, 0, 1)] int delta)
		{
			AssertDecompressionLimitAsync (false, entity, delta).GetAwaiter ().GetResult ();
		}

		[Test]
		public Task TestDecompressionLimitAsync ([Values (false, true)] bool entity, [Values (-1, 0, 1)] int delta)
		{
			return AssertDecompressionLimitAsync (true, entity, delta);
		}

		[Test]
		public void TestDefaultDecompressionLimit ()
		{
			using var ctx = new TemporarySecureMimeContext ();
			using var input = new MemoryStream (Compress (new byte[64 * 1024 * 1024 + 1]));
			Assert.Throws<FormatException> (() => ctx.DecompressTo (input, Stream.Null));
		}

		[Test]
		public void TestDefaultDecompressionLimitAsync ()
		{
			using var ctx = new TemporarySecureMimeContext ();
			using var input = new MemoryStream (Compress (new byte[64 * 1024 * 1024 + 1]));
			Assert.ThrowsAsync<FormatException> (() => ctx.DecompressToAsync (input, Stream.Null));
		}

		sealed class CancelingOutputStream : MemoryStream
		{
			readonly CancellationTokenSource source;

			public CancelingOutputStream (CancellationTokenSource source)
			{
				this.source = source;
			}

			public override void Write (byte[] buffer, int offset, int count)
			{
				base.Write (buffer, offset, count);
				source.Cancel ();
			}
		}

		[Test]
		public void TestDecompressionCancellation ()
		{
			using var ctx = new TemporarySecureMimeContext ();
			using var source = new CancellationTokenSource ();
			using var input = new MemoryStream (Compress (new byte[16384]));
			using var output = new CancelingOutputStream (source);
			Assert.Throws<OperationCanceledException> (() => ctx.DecompressTo (input, output, source.Token));
			Assert.That (output.Length, Is.EqualTo (4096));
		}

		[Test]
		public void TestDecompressionCancellationAsync ()
		{
			using var ctx = new TemporarySecureMimeContext ();
			using var source = new CancellationTokenSource ();
			using var input = new MemoryStream (Compress (new byte[16384]));
			using var output = new CancelingOutputStream (source);
			Assert.ThrowsAsync<OperationCanceledException> (() => ctx.DecompressToAsync (input, output, source.Token));
			Assert.That (output.Length, Is.EqualTo (4096));
		}

		static X509Certificate CreateCertificateWithDistributionPoint (bool selfSigned, string location)
		{
			var generator = new X509V3CertificateGenerator ();
			var subject = new X509Name (selfSigned ? "CN=Untrusted" : "CN=Trusted");
			generator.SetSubjectDN (subject);
			generator.SetIssuerDN (selfSigned ? subject : SecureMimeTestsBase.RootCertificate.SubjectDN);
			generator.SetSerialNumber (BigInteger.ValueOf (selfSigned ? 1001 : 1002));
			generator.SetNotBefore (DateTime.UtcNow.AddDays (-1));
			generator.SetNotAfter (DateTime.UtcNow.AddDays (1));
			generator.SetPublicKey (SecureMimeTestsBase.RootCertificate.GetPublicKey ());
			generator.AddExtension (X509Extensions.BasicConstraints, true, new BasicConstraints (false));
			generator.AddExtension (X509Extensions.KeyUsage, true, new KeyUsage (KeyUsage.DigitalSignature | KeyUsage.KeyEncipherment));
			var names = new GeneralNames (new GeneralName (GeneralName.UniformResourceIdentifier, location));
			var point = new DistributionPoint (new DistributionPointName (names), null, null);
			generator.AddExtension (X509Extensions.CrlDistributionPoints, false, new CrlDistPoint (new[] { point }));
			return generator.Generate (new Asn1SignatureFactory ("SHA256WITHRSA", SecureMimeTestsBase.RootKey));
		}

		sealed class RecordingCrlHandler : HttpMessageHandler
		{
			public readonly List<Uri> Requests = new List<Uri> ();

			protected override Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken)
			{
				Requests.Add (request.RequestUri);
				var crl = SecureMimeTestsBase.CurrentCrls[0];
				return Task.FromResult (new HttpResponseMessage (System.Net.HttpStatusCode.OK) {
					Content = new ByteArrayContent (crl.GetEncoded ())
				});
			}
		}

		sealed class CrlTestContext : TemporarySecureMimeContext
		{
			readonly HttpClient client;

			public CrlTestContext (HttpMessageHandler handler)
			{
				client = new HttpClient (handler);
				CheckCertificateRevocation = true;
			}

			protected override HttpClient HttpClient => client;

			protected override void Dispose (bool disposing)
			{
				if (disposing)
					client.Dispose ();
				base.Dispose (disposing);
			}
		}

		static async Task AssertCrlRetrievalRequiresTrustedPathAsync (bool doAsync, bool trusted, bool encrypt)
		{
			var leaf = CreateCertificateWithDistributionPoint (false, "http://127.0.0.1/trusted-chain.crl");
			var extra = CreateCertificateWithDistributionPoint (true, "http://127.0.0.1/unrelated.crl");
			using var handler = new RecordingCrlHandler ();
			using var ctx = new CrlTestContext (handler);
			// An unrelated certificate in the local store must not trigger retrieval either.
			ctx.Import (extra, true);
			if (trusted)
				ctx.Import (SecureMimeTestsBase.RootCertificate, true);
			var payload = System.Text.Encoding.ASCII.GetBytes ("Content-Type: text/plain\r\n\r\ndata");
			using var content = new MemoryStream (payload);

			if (encrypt) {
				var recipients = new CmsRecipientCollection { new CmsRecipient (leaf) };
				async Task EncryptAsync ()
				{
					using var encrypted = doAsync ? await ctx.EncryptAsync (recipients, content) : ctx.Encrypt (recipients, content);
				}
				if (trusted)
					await EncryptAsync ();
				else
					Assert.ThrowsAsync<CmsEnvelopeException> (EncryptAsync);
			} else {
				var generator = new CmsSignedDataGenerator ();
				generator.AddSigner (SecureMimeTestsBase.RootKey, leaf, CmsSignedGenerator.DigestSha256);
				generator.AddCertificates (CollectionUtilities.CreateStore (new[] { leaf, extra }));
				using var data = new MemoryStream (generator.Generate (new CmsProcessableByteArray (payload), false).GetEncoded ());
				var signatures = doAsync ? await ctx.VerifyAsync (content, data) : ctx.Verify (content, data);
				var signature = (SecureMimeDigitalSignature) signatures[0];
				Assert.That (signature.Verify (true), Is.True);
				if (trusted)
					Assert.That (signature.Verify (), Is.True);
				else
					Assert.Throws<DigitalSignatureVerifyException> (() => signature.Verify ());
			}

			Assert.That (handler.Requests, Is.EqualTo (trusted ? new[] { new Uri ("http://127.0.0.1/trusted-chain.crl") } : Array.Empty<Uri> ()));
		}

		[Test]
		public void TestCrlRetrievalRequiresTrustedPath ([Values (false, true)] bool trusted, [Values (false, true)] bool encrypt)
		{
			AssertCrlRetrievalRequiresTrustedPathAsync (false, trusted, encrypt).GetAwaiter ().GetResult ();
		}

		[Test]
		public Task TestCrlRetrievalRequiresTrustedPathAsync ([Values (false, true)] bool trusted, [Values (false, true)] bool encrypt)
		{
			return AssertCrlRetrievalRequiresTrustedPathAsync (true, trusted, encrypt);
		}

		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentNullException> (() => BouncyCastleSecureMimeContext.TryGetDigestAlgorithm (null, out _), "TryGetDIgestAlgorithm");
			Assert.Throws<ArgumentNullException> (() => BouncyCastleSecureMimeContext.TryGetEncryptionAlgorithm (null, out _), "TryGetEncryptionAlgorithm");
		}

		[Test]
		public void TestTryGetEncryptionAlgorithm ()
		{
			EncryptionAlgorithm algorithm;

			Assert.That (BouncyCastleSecureMimeContext.TryGetEncryptionAlgorithm (new AlgorithmIdentifier (SecureMimeContext.Blowfish, DerNull.Instance), out algorithm), Is.True);
			Assert.That (algorithm, Is.EqualTo (EncryptionAlgorithm.Blowfish));

			Assert.That (BouncyCastleSecureMimeContext.TryGetEncryptionAlgorithm (new AlgorithmIdentifier (SecureMimeContext.Twofish, DerNull.Instance), out algorithm), Is.True);
			Assert.That (algorithm, Is.EqualTo (EncryptionAlgorithm.Twofish));

			Assert.That (BouncyCastleSecureMimeContext.TryGetEncryptionAlgorithm (new AlgorithmIdentifier (SmimeCapabilities.IdeaCbc, DerNull.Instance), out algorithm), Is.True);
			Assert.That (algorithm, Is.EqualTo (EncryptionAlgorithm.Idea));

			Assert.That (BouncyCastleSecureMimeContext.TryGetEncryptionAlgorithm (new AlgorithmIdentifier (new DerObjectIdentifier (CmsEnvelopedGenerator.RC2Cbc), DerInteger.ValueOf (128)), out algorithm), Is.True);
			Assert.That (algorithm, Is.EqualTo (EncryptionAlgorithm.RC2128));

			Assert.That (BouncyCastleSecureMimeContext.TryGetEncryptionAlgorithm (new AlgorithmIdentifier (new DerObjectIdentifier (CmsEnvelopedGenerator.RC2Cbc), DerInteger.ValueOf (64)), out algorithm), Is.True);
			Assert.That (algorithm, Is.EqualTo (EncryptionAlgorithm.RC264));

			Assert.That (BouncyCastleSecureMimeContext.TryGetEncryptionAlgorithm (new AlgorithmIdentifier (new DerObjectIdentifier (CmsEnvelopedGenerator.RC2Cbc), DerInteger.ValueOf (40)), out algorithm), Is.True);
			Assert.That (algorithm, Is.EqualTo (EncryptionAlgorithm.RC240));

			Assert.That (BouncyCastleSecureMimeContext.TryGetEncryptionAlgorithm (new AlgorithmIdentifier (new DerObjectIdentifier (CmsEnvelopedGenerator.RC2Cbc), new DerSequence (DerInteger.ValueOf (58))), out algorithm), Is.True);
			Assert.That (algorithm, Is.EqualTo (EncryptionAlgorithm.RC2128));

			Assert.That (BouncyCastleSecureMimeContext.TryGetEncryptionAlgorithm (new AlgorithmIdentifier (new DerObjectIdentifier (CmsEnvelopedGenerator.RC2Cbc), new DerSequence (DerInteger.ValueOf (120))), out algorithm), Is.True);
			Assert.That (algorithm, Is.EqualTo (EncryptionAlgorithm.RC264));

			Assert.That (BouncyCastleSecureMimeContext.TryGetEncryptionAlgorithm (new AlgorithmIdentifier (new DerObjectIdentifier (CmsEnvelopedGenerator.RC2Cbc), new DerSequence (DerInteger.ValueOf (160))), out algorithm), Is.True);
			Assert.That (algorithm, Is.EqualTo (EncryptionAlgorithm.RC240));

			Assert.That (BouncyCastleSecureMimeContext.TryGetEncryptionAlgorithm (new AlgorithmIdentifier (new DerObjectIdentifier (CmsEnvelopedGenerator.RC2Cbc), 			DerInteger.ValueOf (1)), out algorithm), Is.False);
		}
	}
}

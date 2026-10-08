//
// DkimTests.cs
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

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto.Parameters;

using MimeKit;
using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class DkimTests
	{
		static readonly AsymmetricKeyParameter GMailDkimPublicKey;
		static readonly AsymmetricKeyParameter Ed25519PrivateKey;
		static readonly AsymmetricCipherKeyPair DkimKeys;

		class DummyDnsResolver : IDnsResolver
		{
			readonly DnsTxtResponse response;

			public DummyDnsResolver (AsymmetricKeyParameter publicKey)
			{
				string txt;

				if (publicKey is Ed25519PublicKeyParameters ed25519)
					txt = "v=DKIM1; k=ed25519; p=" + Convert.ToBase64String (ed25519.GetEncoded ());
				else
					txt = "v=DKIM1; k=rsa; p=" + Convert.ToBase64String (SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo (publicKey).GetEncoded ());

				response = new DnsTxtResponse (new[] { txt });
			}

			public DnsTxtResponse QueryTxt (string domain, CancellationToken cancellationToken = default)
			{
				return response;
			}

			public Task<DnsTxtResponse> QueryTxtAsync (string domain, CancellationToken cancellationToken = default)
			{
				return Task.FromResult (response);
			}
		}

		static DkimTests ()
		{
			using (var reader = new PemReader (new StreamReader (Path.Combine (TestHelper.ProjectDir, "TestData", "dkim", "example.pem"))))
				DkimKeys = reader.ReadObject () as AsymmetricCipherKeyPair;

			// Note: you can use http://dkimcore.org/tools/dkimrecordcheck.html to get public keys manually
			using (var reader = new PemReader (new StreamReader (Path.Combine (TestHelper.ProjectDir, "TestData", "dkim", "gmail.pub"))))
				GMailDkimPublicKey = reader.ReadObject () as AsymmetricKeyParameter;

			var rawData = Convert.FromBase64String ("nWGxne/9WmC6hEr0kuwsxERJxWl7MmkZcDusAxyuf2A=");
			Ed25519PrivateKey = new Ed25519PrivateKeyParameters (rawData, 0);
		}

		static DkimSigner CreateSigner (DkimSignatureAlgorithm algorithm, DkimCanonicalizationAlgorithm headerAlgorithm, DkimCanonicalizationAlgorithm bodyAlgorithm)
		{
			return new DkimSigner (Path.Combine (TestHelper.ProjectDir, "TestData", "dkim", "example.pem"), "example.com", "1433868189.example") {
				BodyCanonicalizationAlgorithm = bodyAlgorithm,
				HeaderCanonicalizationAlgorithm = headerAlgorithm,
				SignatureAlgorithm = algorithm,
				AgentOrUserIdentifier = "@eng.example.com",
				QueryMethod = "dns/txt"
			};
		}

		const string ExampleKeyName = "1433868189.example._domainkey.example.com";

		static string ExamplePublicKey {
			get { return Convert.ToBase64String (SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo (DkimKeys.Public).GetEncoded ()); }
		}

		static MimeMessage CreateSignedMessage (out Header dkim)
		{
			var signer = CreateSigner (DkimSignatureAlgorithm.RsaSha256, DkimCanonicalizationAlgorithm.Relaxed, DkimCanonicalizationAlgorithm.Relaxed);
			var message = new MimeMessage ();

			message.From.Add (new MailboxAddress ("", "mimekit@example.com"));
			message.To.Add (new MailboxAddress ("", "mimekit@example.com"));
			message.Subject = "Key lookup test";
			message.Date = new DateTimeOffset (2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
			message.Body = new TextPart ("plain") { Text = "This is the body." };

			signer.Sign (message, new HeaderId[] { HeaderId.From, HeaderId.To, HeaderId.Subject, HeaderId.Date });

			dkim = message.Headers[0];

			return message;
		}

		static async Task AssertVerifyAsync (MockDnsResolver resolver, bool expected, string description, Action<Header> modify = null)
		{
			var verifier = new DkimVerifier (resolver);

			using var message = CreateSignedMessage (out var dkim);

			modify?.Invoke (dkim);

			Assert.That (verifier.Verify (message, dkim), Is.EqualTo (expected), description);
			Assert.That (await verifier.VerifyAsync (message, dkim), Is.EqualTo (expected), description + " (async)");
		}

		[Test]
		public async Task TestVerifyKeyLookup ()
		{
			var resolver = new MockDnsResolver ();
			resolver.Add (ExampleKeyName, "v=DKIM1; k=rsa; p=" + ExamplePublicKey);

			await AssertVerifyAsync (resolver, true, "valid key");
			Assert.That (resolver.Queries, Is.EqualTo (new[] { ExampleKeyName, ExampleKeyName }));
		}

		[Test]
		public async Task TestVerifyKeyLookupSkipsInvalidRecords ()
		{
			var resolver = new MockDnsResolver ();
			resolver.Add (ExampleKeyName, "this is not a dkim record");
			resolver.Add (ExampleKeyName, "v=DKIM1; k=ed25519; p=11qYAYKxCrfVS/7TyWQHOg7hcvPapiMlrwIaaPcHURo=");
			resolver.Add (ExampleKeyName, "v=DKIM1; k=rsa; p=" + ExamplePublicKey);

			await AssertVerifyAsync (resolver, true, "valid key after invalid and incompatible records");
		}

		[Test]
		public async Task TestVerifyKeyLookupNonExistentDomain ()
		{
			await AssertVerifyAsync (new MockDnsResolver (), false, "NXDOMAIN");
		}

		[Test]
		public async Task TestVerifyKeyLookupNoRecords ()
		{
			var resolver = new MockDnsResolver ();
			resolver.AddFailure (ExampleKeyName, DnsQueryStatus.Success);

			await AssertVerifyAsync (resolver, false, "NODATA");
		}

		[Test]
		public async Task TestVerifyKeyLookupTemporaryFailure ()
		{
			var resolver = new MockDnsResolver ();
			resolver.AddFailure (ExampleKeyName, DnsQueryStatus.TemporaryFailure);

			await AssertVerifyAsync (resolver, false, "SERVFAIL");
		}

		[Test]
		public async Task TestVerifyKeyLookupResolverThrows ()
		{
			var resolver = new MockDnsResolver ();
			resolver.AddException (ExampleKeyName, new IOException ("network unreachable"));

			await AssertVerifyAsync (resolver, false, "resolver exception");
		}

		[Test]
		public void TestVerifyKeyLookupCancelled ()
		{
			var resolver = new MockDnsResolver ();
			resolver.AddException (ExampleKeyName, new OperationCanceledException ());

			var verifier = new DkimVerifier (resolver);
			using var message = CreateSignedMessage (out var dkim);

			Assert.Throws<OperationCanceledException> (() => verifier.Verify (message, dkim));
			Assert.ThrowsAsync<OperationCanceledException> (() => verifier.VerifyAsync (message, dkim));
		}

		[Test]
		public async Task TestVerifyKeyLookupRevoked ()
		{
			var resolver = new MockDnsResolver ();
			resolver.Add (ExampleKeyName, "v=DKIM1; k=rsa; p=");

			await AssertVerifyAsync (resolver, false, "revoked key");
		}

		[Test]
		public async Task TestVerifyKeyLookupKeyTypeMismatch ()
		{
			var resolver = new MockDnsResolver ();
			resolver.Add (ExampleKeyName, "v=DKIM1; k=ed25519; p=11qYAYKxCrfVS/7TyWQHOg7hcvPapiMlrwIaaPcHURo=");

			await AssertVerifyAsync (resolver, false, "key type mismatch");
		}

		[Test]
		public async Task TestVerifyKeyLookupHashAlgorithmMismatch ()
		{
			var resolver = new MockDnsResolver ();
			resolver.Add (ExampleKeyName, "v=DKIM1; k=rsa; h=sha1; p=" + ExamplePublicKey);

			await AssertVerifyAsync (resolver, false, "h=sha1 does not permit rsa-sha256");
		}

		[Test]
		public async Task TestVerifyKeyLookupStrictFlag ()
		{
			var resolver = new MockDnsResolver ();
			resolver.Add (ExampleKeyName, "v=DKIM1; k=rsa; t=s; p=" + ExamplePublicKey);

			// The signer uses i=@eng.example.com which is a subdomain of d=example.com
			await AssertVerifyAsync (resolver, false, "t=s with a subdomain AUID");
		}

		[Test]
		public async Task TestVerifyKeyLookupUnsupportedQueryMethod ()
		{
			var resolver = new MockDnsResolver ();
			resolver.Add (ExampleKeyName, "v=DKIM1; k=rsa; p=" + ExamplePublicKey);

			await AssertVerifyAsync (resolver, false, "q=dns/other", dkim => dkim.Value = dkim.Value.Replace ("q=dns/txt", "q=dns/other"));
			Assert.That (resolver.Queries, Is.Empty);
		}

		[Test]
		public void TestDkimSignerCtors ()
		{
			Assert.DoesNotThrow (() => {
				var signer = new DkimSigner (Path.Combine (TestHelper.ProjectDir, "TestData", "dkim", "example.pem"), "example.com", "1433868189.example") {
					SignatureAlgorithm = DkimSignatureAlgorithm.RsaSha256,
					AgentOrUserIdentifier = "@eng.example.com",
					QueryMethod = "dns/txt"
				};
			});

			Assert.DoesNotThrow (() => {
				var signer = new DkimSigner (DkimKeys.Private, "example.com", "1433868189.example") {
					SignatureAlgorithm = DkimSignatureAlgorithm.RsaSha256,
					AgentOrUserIdentifier = "@eng.example.com",
					QueryMethod = "dns/txt"
				};
			});
		}

		[Test]
		public void TestDkimSignerDefaults ()
		{
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", "dkim", "example.pem");
			DkimSigner signer;

			signer = new DkimSigner (DkimKeys.Private, "example.com", "1433868189.example");
			Assert.That (signer.SignatureAlgorithm, Is.EqualTo (DkimSignatureAlgorithm.RsaSha256), "SignatureAlgorithm #1");

			signer = new DkimSigner (path, "example.com", "1433868189.example");
			Assert.That (signer.SignatureAlgorithm, Is.EqualTo (DkimSignatureAlgorithm.RsaSha256), "SignatureAlgorithm #2");

			using (var stream = File.OpenRead (path)) {
				signer = new DkimSigner (stream, "example.com", "1433868189.example");
				Assert.That (signer.SignatureAlgorithm, Is.EqualTo (DkimSignatureAlgorithm.RsaSha256), "SignatureAlgorithm #3");
			}
		}

		[Test]
		public void TestDkimVerifierDefaults ()
		{
			var verifier = new DkimVerifier (new DummyDnsResolver (DkimKeys.Public));

			Assert.That (verifier.MinimumRsaKeyLength, Is.EqualTo (1024), "MinimumRsaKeyLength");
			Assert.That (verifier.IsEnabled (DkimSignatureAlgorithm.RsaSha1), Is.False, "rsa-sha1");
			Assert.That (verifier.IsEnabled (DkimSignatureAlgorithm.RsaSha256), Is.True, "rsa-sha256");
		}

		[Test]
		public void TestDkimVerifierEnableDisable ()
		{
			var verifier = new DkimVerifier (new DummyDnsResolver (DkimKeys.Public));

			Assert.That (verifier.IsEnabled (DkimSignatureAlgorithm.RsaSha1), Is.False, "initial value");

			verifier.Enable (DkimSignatureAlgorithm.RsaSha1);
			Assert.That (verifier.IsEnabled (DkimSignatureAlgorithm.RsaSha1), Is.True, "rsa-sha1 enabled");

			verifier.Disable (DkimSignatureAlgorithm.RsaSha1);
			Assert.That (verifier.IsEnabled (DkimSignatureAlgorithm.RsaSha1), Is.False, "rsa-sha1 disabled");
		}

		[Test]
		public void TestDkimHashStream ()
		{
			var buffer = new byte[128];

			using (var stream = new DkimHashStream (DkimSignatureAlgorithm.RsaSha1)) {
				Assert.That (stream.CanRead, Is.False);
				Assert.That (stream.CanWrite, Is.True);
				Assert.That (stream.CanSeek, Is.False);
				Assert.That (stream.CanTimeout, Is.False);

				Assert.Throws<NotSupportedException> (() => stream.Read (buffer, 0, buffer.Length));

				Assert.Throws<ArgumentNullException> (() => stream.Write (null, 0, 0));
				Assert.Throws<ArgumentOutOfRangeException> (() => stream.Write (buffer, -1, 0));
				Assert.Throws<ArgumentOutOfRangeException> (() => stream.Write (buffer, 0, -1));

				Assert.That (stream.Position, Is.EqualTo (0));
				Assert.That (stream.Length, Is.EqualTo (0));

				Assert.Throws<NotSupportedException> (() => stream.Position = 64);

				Assert.Throws<NotSupportedException> (() => stream.Seek (64, SeekOrigin.Begin));
				Assert.Throws<NotSupportedException> (() => stream.SetLength (256));

				stream.Flush ();
			}
		}

		[Test]
		public void TestDkimSignatureStream ()
		{
			var signer = CreateSigner (DkimSignatureAlgorithm.RsaSha1, DkimCanonicalizationAlgorithm.Simple, DkimCanonicalizationAlgorithm.Simple);
			var buffer = new byte[128];

			Assert.Throws<ArgumentNullException> (() => new DkimSignatureStream (null));

			using (var stream = new DkimSignatureStream (signer.CreateSigningContext ())) {
				Assert.That (stream.CanRead, Is.False);
				Assert.That (stream.CanWrite, Is.True);
				Assert.That (stream.CanSeek, Is.False);
				Assert.That (stream.CanTimeout, Is.False);

				Assert.Throws<NotSupportedException> (() => stream.Read (buffer, 0, buffer.Length));

				Assert.Throws<ArgumentNullException> (() => stream.Write (null, 0, 0));
				Assert.Throws<ArgumentOutOfRangeException> (() => stream.Write (buffer, -1, 0));
				Assert.Throws<ArgumentOutOfRangeException> (() => stream.Write (buffer, 0, -1));

				Assert.That (stream.Position, Is.EqualTo (0));
				Assert.That (stream.Length, Is.EqualTo (0));

				Assert.Throws<NotSupportedException> (() => stream.Position = 64);

				Assert.Throws<NotSupportedException> (() => stream.Seek (64, SeekOrigin.Begin));
				Assert.Throws<NotSupportedException> (() => stream.SetLength (256));

				Assert.Throws<ArgumentNullException> (() => stream.VerifySignature (null));

				stream.Flush ();
			}
		}

		[Test]
		public void TestDkimSignaturesExpirationHeaderValue ()
		{
			var signer = CreateSigner (DkimSignatureAlgorithm.RsaSha1, DkimCanonicalizationAlgorithm.Simple, DkimCanonicalizationAlgorithm.Simple);
			signer.SignaturesExpireAfter = TimeSpan.FromDays (1);
			
			var headers = new [] { HeaderId.From, HeaderId.To, HeaderId.Subject, HeaderId.Date };
			var message = new MimeMessage ();

			message.From.Add (new MailboxAddress ("", "mimekit@example.com"));
			message.To.Add (new MailboxAddress ("", "mimekit@example.com"));
			message.Subject = "This is an empty message";
			message.Date = DateTimeOffset.Now;

			message.Body = new TextPart ("plain") { Text = "" };

			message.Prepare (EncodingConstraint.SevenBit);

			signer.Sign (message, headers);
			
			var headerValue = message.Headers[HeaderId.DkimSignature];
			
			var parameters = headerValue.Split (';');
			long? timestamp = null, expiration = null;

			for (int i = 0; i < parameters.Length; i++) {
				var param = parameters[i].Trim ();

				if (param.StartsWith ("t=", StringComparison.Ordinal)) {
					timestamp = long.Parse (param.Substring (3));
				}
				
				if (param.StartsWith ("x=", StringComparison.Ordinal)) {
					expiration =  long.Parse (param.Substring (3));
				}
			}

			Assert.That (timestamp, Is.Not.Null, "Timestamp should not be null.");
			Assert.That (expiration, Is.Not.Null, "Signature expiration should not be null.");
			var diff = expiration - timestamp;
			Assert.That (diff, Is.EqualTo (TimeSpan.FromDays(1).TotalSeconds), "Difference between timestamp and signature expiration should have expected value.");
		}

		static void VerifyDkimBodyHash (MimeMessage message, DkimSignatureAlgorithm algorithm, string expectedHash)
		{
			var value = message.Headers[HeaderId.DkimSignature];
			var parameters = value.Split (';');
			string hash = null;

			for (int i = 0; i < parameters.Length; i++) {
				var param = parameters[i].Trim ();

				if (param.StartsWith ("bh=", StringComparison.Ordinal)) {
					hash = param.Substring (3);
					break;
				}
			}

			Assert.That (hash, Is.EqualTo (expectedHash), $"The {algorithm.ToString ().ToUpperInvariant ().Substring (3)} hash does not match the expected value.");
		}

		static void TestEmptyBody (DkimSignatureAlgorithm signatureAlgorithm, DkimCanonicalizationAlgorithm bodyAlgorithm, string expectedHash)
		{
			var signer = CreateSigner (signatureAlgorithm, DkimCanonicalizationAlgorithm.Simple, bodyAlgorithm);
			var headers = new [] { HeaderId.From, HeaderId.To, HeaderId.Subject, HeaderId.Date };
			var verifier = new DkimVerifier (new DummyDnsResolver (DkimKeys.Public));
			var message = new MimeMessage ();

			message.From.Add (new MailboxAddress ("", "mimekit@example.com"));
			message.To.Add (new MailboxAddress ("", "mimekit@example.com"));
			message.Subject = "This is an empty message";
			message.Date = DateTimeOffset.Now;

			message.Body = new TextPart ("plain") { Text = "" };

			message.Prepare (EncodingConstraint.SevenBit);

			signer.Sign (message, headers);

			VerifyDkimBodyHash (message, signatureAlgorithm, expectedHash);

			var dkim = message.Headers[0];

			if (signatureAlgorithm == DkimSignatureAlgorithm.RsaSha1) {
				Assert.That (verifier.Verify (message, dkim), Is.False, "DKIM-Signature using rsa-sha1 should not verify.");

				// now enable rsa-sha1 to verify again, this time it should pass...
				verifier.Enable (DkimSignatureAlgorithm.RsaSha1);
			}

			Assert.That (verifier.Verify (message, dkim), Is.True, "Failed to verify DKIM-Signature.");
		}

		[Test]
		public void TestArgumentExceptions ()
		{
			var resolver = new DummyDnsResolver (DkimKeys.Public);
			var verifier = new DkimVerifier (resolver);
			var dkimHeader = new Header (HeaderId.DkimSignature, "value");
			var arcHeader = new Header (HeaderId.ArcMessageSignature, "value");
			var options = FormatOptions.Default;
			var message = new MimeMessage ();
			DkimSigner signer;

			Assert.Throws<ArgumentNullException> (() => new DkimSigner ((AsymmetricKeyParameter) null, "domain", "selector"));
			Assert.Throws<ArgumentException> (() => new DkimSigner (DkimKeys.Public, "domain", "selector"));
			Assert.Throws<ArgumentNullException> (() => new DkimSigner (DkimKeys.Private, null, "selector"));
			Assert.Throws<ArgumentNullException> (() => new DkimSigner (DkimKeys.Private, "domain", null));
			Assert.Throws<ArgumentNullException> (() => new DkimSigner ((string) null, "domain", "selector"));
			Assert.Throws<ArgumentNullException> (() => new DkimSigner ("fileName", null, "selector"));
			Assert.Throws<ArgumentNullException> (() => new DkimSigner ("fileName", "domain", null));
			Assert.Throws<ArgumentException> (() => new DkimSigner (string.Empty, "domain", "selector"));
			Assert.Throws<ArgumentNullException> (() => new DkimSigner ((Stream) null, "domain", "selector"));
			using (var stream = File.OpenRead (Path.Combine (TestHelper.ProjectDir, "TestData", "dkim", "example.pem"))) {
				Assert.Throws<ArgumentNullException> (() => new DkimSigner (stream, null, "selector"));
				Assert.Throws<ArgumentNullException> (() => new DkimSigner (stream, "domain", null));

				signer = new DkimSigner (stream, "example.com", "1433868189.example") {
					SignatureAlgorithm = DkimSignatureAlgorithm.RsaSha1,
					AgentOrUserIdentifier = "@eng.example.com",
					QueryMethod = "dns/txt",
				};
			}

			Assert.Throws<ArgumentNullException> (() => signer.Sign (null, new HeaderId[] { HeaderId.From }));
			Assert.Throws<ArgumentNullException> (() => signer.Sign (message, (IList<HeaderId>) null));
			Assert.Throws<ArgumentException> (() => signer.Sign (message, new HeaderId[] { HeaderId.Unknown, HeaderId.From }));
			Assert.Throws<ArgumentException> (() => signer.Sign (message, new HeaderId[] { HeaderId.Received, HeaderId.From }));
			Assert.Throws<ArgumentException> (() => signer.Sign (message, new HeaderId[] { HeaderId.ContentType }));
			Assert.Throws<ArgumentNullException> (() => signer.Sign (null, new string[] { "From" }));
			Assert.Throws<ArgumentNullException> (() => signer.Sign (message, (IList<string>) null));
			Assert.Throws<ArgumentException> (() => signer.Sign (message, new string[] { "", "From" }));
			Assert.Throws<ArgumentException> (() => signer.Sign (message, new string[] { null, "From" }));
			Assert.Throws<ArgumentException> (() => signer.Sign (message, new string[] { "Received", "From" }));
			Assert.Throws<ArgumentException> (() => signer.Sign (message, new string[] { "Content-Type" }));

			Assert.Throws<ArgumentNullException> (() => signer.Sign (null, message, new HeaderId[] { HeaderId.From }));
			Assert.Throws<ArgumentNullException> (() => signer.Sign (options, null, new HeaderId[] { HeaderId.From }));
			Assert.Throws<ArgumentException> (() => signer.Sign (options, message, new HeaderId[] { HeaderId.From, HeaderId.Unknown }));
			Assert.Throws<ArgumentNullException> (() => signer.Sign (options, message, (IList<HeaderId>) null));

			Assert.Throws<ArgumentNullException> (() => signer.Sign (null, message, new string[] { "From" }));
			Assert.Throws<ArgumentNullException> (() => signer.Sign (options, null, new string[] { "From" }));
			Assert.Throws<ArgumentException> (() => signer.Sign (options, message, new string[] { "From", null }));
			Assert.Throws<ArgumentNullException> (() => signer.Sign (options, message, (IList<string>) null));

			Assert.Throws<ArgumentNullException> (() => new DkimVerifier (null));

			Assert.Throws<ArgumentNullException> (() => verifier.Verify (null, dkimHeader));
			Assert.Throws<ArgumentNullException> (() => verifier.Verify (message, null));
			Assert.Throws<ArgumentNullException> (() => verifier.Verify (null, message, dkimHeader));
			Assert.Throws<ArgumentNullException> (() => verifier.Verify (FormatOptions.Default, null, dkimHeader));
			Assert.Throws<ArgumentNullException> (() => verifier.Verify (FormatOptions.Default, message, null));
			Assert.Throws<ArgumentException> (() => verifier.Verify (FormatOptions.Default, message, arcHeader));

			Assert.ThrowsAsync<ArgumentNullException> (async () => await verifier.VerifyAsync (null, dkimHeader));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await verifier.VerifyAsync (message, null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await verifier.VerifyAsync (null, message, dkimHeader));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await verifier.VerifyAsync (FormatOptions.Default, null, dkimHeader));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await verifier.VerifyAsync (FormatOptions.Default, message, null));
			Assert.ThrowsAsync<ArgumentException> (async () => await verifier.VerifyAsync (FormatOptions.Default, message, arcHeader));
		}

		[Test]
		public void TestFormatExceptions ()
		{
			using var message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "dkim", "gmail.msg"));
			var verifier = new DkimVerifier (new DummyDnsResolver (DkimKeys.Public));
			var index = message.Headers.IndexOf (HeaderId.DkimSignature);
			var dkim = message.Headers[index];
			var original = dkim.Value;

			// first, remove the 'v' tag and its value
			dkim.Value = dkim.Value.Substring (4);

			Assert.Throws<FormatException> (() => verifier.Verify (message, dkim), "Expected FormatException for missing v=1;");

			// add back a 'v' tag with an invalid value
			dkim.Value = "v=x; " + dkim.Value;

			Assert.Throws<FormatException> (() => verifier.Verify (message, dkim), "Expected FormatException for v=x;");

			// remove "from:"
			dkim.Value = original.Replace ("from:", "");

			Assert.Throws<FormatException> (() => verifier.Verify (message, dkim), "Expected FormatException for missing from header");

			// add an invalid i= value w/o an '@'
			dkim.Value = "i=1; " + original;

			Assert.Throws<FormatException> (() => verifier.Verify (message, dkim), "Expected FormatException for an invalid i= value (missing '@')");

			// add an invalid i= value that does not match the domain
			dkim.Value = "i=user@domain; " + original;

			Assert.Throws<FormatException> (() => verifier.Verify (message, dkim), "Expected FormatException for an invalid i= that does not contain the domain");

			// add an invalid l= value
			dkim.Value = "l=abc; " + original;

			Assert.Throws<FormatException> (() => verifier.Verify (message, dkim), "Expected FormatException for an invalid l= value");

			// set an invalid body canonicalization algorithm
			dkim.Value = original.Replace ("c=relaxed/relaxed;", "c=simple/complex;");

			Assert.Throws<FormatException> (() => verifier.Verify (message, dkim), "Expected FormatException for an invalid body canonicalization value");

			// set an invalid c= value
			dkim.Value = original.Replace ("c=relaxed/relaxed;", "c=;");

			Assert.Throws<FormatException> (() => verifier.Verify (message, dkim), "Expected FormatException for an invalid c= value (empty)");

			// set an invalid c= value
			dkim.Value = original.Replace ("c=relaxed/relaxed;", "c=relaxed/relaxed/extra;");

			Assert.Throws<FormatException> (() => verifier.Verify (message, dkim), "Expected FormatException for an invalid c= value (3 values)");
		}

		[Test]
		public void TestEmptySimpleBodyRsaSha1 ()
		{
			TestEmptyBody (DkimSignatureAlgorithm.RsaSha1, DkimCanonicalizationAlgorithm.Simple, "uoq1oCgLlTqpdDX/iUbLy7J1Wic=");
		}

		[Test]
		public void TestEmptySimpleBodyRsaSha256 ()
		{
			TestEmptyBody (DkimSignatureAlgorithm.RsaSha256, DkimCanonicalizationAlgorithm.Simple, "frcCV1k9oG9oKj3dpUqdJg1PxRT2RSN/XKdLCPjaYaY=");
		}

		[Test]
		public void TestEmptyRelaxedBodyRsaSha1 ()
		{
			TestEmptyBody (DkimSignatureAlgorithm.RsaSha1, DkimCanonicalizationAlgorithm.Relaxed, "2jmj7l5rSw0yVb/vlWAYkK/YBwk=");
		}

		[Test]
		public void TestEmptyRelaxedBodyRsaSha256 ()
		{
			TestEmptyBody (DkimSignatureAlgorithm.RsaSha256, DkimCanonicalizationAlgorithm.Relaxed, "47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=");
		}

		static void TestUnicode (DkimSignatureAlgorithm signatureAlgorithm, DkimCanonicalizationAlgorithm bodyAlgorithm, string expectedHash)
		{
			var signer = CreateSigner (signatureAlgorithm, DkimCanonicalizationAlgorithm.Simple, bodyAlgorithm);
			var headers = new [] { HeaderId.From, HeaderId.To, HeaderId.Subject, HeaderId.Date };
			var verifier = new DkimVerifier (new DummyDnsResolver (DkimKeys.Public));
			var message = new MimeMessage ();

			message.From.Add (new MailboxAddress ("", "mimekit@example.com"));
			message.To.Add (new MailboxAddress ("", "mimekit@example.com"));
			message.Subject = "This is a unicode message";
			message.Date = DateTimeOffset.Now;

			var builder = new BodyBuilder {
				TextBody = " تست  ",
				HtmlBody = "  <div> تست </div> "
			};
			message.Body = builder.ToMessageBody ();

			((Multipart) message.Body).Boundary = "=-MultipartAlternativeBoundary";
			((Multipart) message.Body)[1].ContentId = null;

			message.Prepare (EncodingConstraint.EightBit);

			signer.Sign (message, headers);

			var dkim = message.Headers[0];

			VerifyDkimBodyHash (message, signatureAlgorithm, expectedHash);

			if (signatureAlgorithm == DkimSignatureAlgorithm.RsaSha1) {
				Assert.That (verifier.Verify (message, dkim), Is.False, "DKIM-Signature using rsa-sha1 should not verify.");

				// now enable rsa-sha1 to verify again, this time it should pass...
				verifier.Enable (DkimSignatureAlgorithm.RsaSha1);
			}

			Assert.That (verifier.Verify (message, dkim), Is.True, "Failed to verify DKIM-Signature.");
		}

		[Test]
		public void TestUnicodeSimpleBodyRsaSha1 ()
		{
			TestUnicode (DkimSignatureAlgorithm.RsaSha1, DkimCanonicalizationAlgorithm.Simple, "6GV1ZoyaprYbwRLXsr5+8zY5Jh0=");
		}

		[Test]
		public void TestUnicodeSimpleBodyRsaSha256 ()
		{
			TestUnicode (DkimSignatureAlgorithm.RsaSha256, DkimCanonicalizationAlgorithm.Simple, "BuW/GpCA9rAVDfStp0Dc2duuFhmwcxhy5jOeL+Xn+ew=");
		}

		[Test]
		public void TestUnicodeRelaxedBodyRsaSha1 ()
		{
			TestUnicode (DkimSignatureAlgorithm.RsaSha1, DkimCanonicalizationAlgorithm.Relaxed, "bbT6nP0aAiAP5OMguA+mHgpzgh4=");
		}

		[Test]
		public void TestUnicodeRelaxedBodyRsaSha256 ()
		{
			TestUnicode (DkimSignatureAlgorithm.RsaSha256, DkimCanonicalizationAlgorithm.Relaxed, "PEaN3fYH5NdIg4QzgaSS+ceYlSMRnYbqCPMxncx6gy0=");
		}

		[Test]
		public void TestVerifyGoogleMailDkimSignature ()
		{
			using var message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "dkim", "gmail.msg"));
			int index = message.Headers.IndexOf (HeaderId.DkimSignature);
			var resolver = new DummyDnsResolver (GMailDkimPublicKey);
			var verifier = new DkimVerifier (resolver);

			Assert.That (verifier.Verify (message, message.Headers[index]), Is.True, "Failed to verify GMail signature.");
		}

		[Test]
		public async Task TestVerifyGoogleMailDkimSignatureAsync ()
		{
			using var message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "dkim", "gmail.msg"));
			int index = message.Headers.IndexOf (HeaderId.DkimSignature);
			var resolver = new DummyDnsResolver (GMailDkimPublicKey);
			var verifier = new DkimVerifier (resolver);

			Assert.That (await verifier.VerifyAsync (message, message.Headers[index]), Is.True, "Failed to verify GMail signature.");
		}

		[Test]
		public void TestVerifyGoogleMultipartRelatedDkimSignature ()
		{
			using var message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "dkim", "related.msg"));
			int index = message.Headers.IndexOf (HeaderId.DkimSignature);
			var resolver = new DummyDnsResolver (GMailDkimPublicKey);
			var verifier = new DkimVerifier (resolver);

			Assert.That (verifier.Verify (message, message.Headers[index]), Is.True, "Failed to verify GMail signature.");
		}

		[Test]
		public async Task TestVerifyGoogleMultipartRelatedDkimSignatureAsync ()
		{
			using var message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "dkim", "related.msg"));
			int index = message.Headers.IndexOf (HeaderId.DkimSignature);
			var resolver = new DummyDnsResolver (GMailDkimPublicKey);
			var verifier = new DkimVerifier (resolver);

			Assert.That (await verifier.VerifyAsync (message, message.Headers[index]), Is.True, "Failed to verify GMail signature.");
		}

		[Test]
		public void TestVerifyGoogleMultipartWithoutEndBoundaryDkimSignature ()
		{
			using var message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "dkim", "multipart-no-end-boundary.msg"));
			int index = message.Headers.IndexOf (HeaderId.DkimSignature);
			var resolver = new DummyDnsResolver (GMailDkimPublicKey);
			var verifier = new DkimVerifier (resolver);

			Assert.That (verifier.Verify (message, message.Headers[index]), Is.True, "Failed to verify GMail signature.");
		}

		[Test]
		public async Task TestVerifyGoogleMultipartWithoutEndBoundaryDkimSignatureAsync ()
		{
			using var message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "dkim", "multipart-no-end-boundary.msg"));
			int index = message.Headers.IndexOf (HeaderId.DkimSignature);
			var resolver = new DummyDnsResolver (GMailDkimPublicKey);
			var verifier = new DkimVerifier (resolver);

			Assert.That (await verifier.VerifyAsync (message, message.Headers[index]), Is.True, "Failed to verify GMail signature.");
		}

		[Test]
		public void TestSignRfc8463Example ()
		{
			using var message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "dkim", "rfc8463-example.msg"));
			var signer = new DkimSigner (Ed25519PrivateKey, "football.example.com", "brisbane", DkimSignatureAlgorithm.Ed25519Sha256) {
				HeaderCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Relaxed,
				BodyCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Relaxed,
				AgentOrUserIdentifier = "@football.example.com"
			};
			var headers = new string[] { "from", "to", "subject", "date", "message-id", "from", "subject", "date" };

			signer.Sign (message, headers);

			int index = message.Headers.IndexOf (HeaderId.DkimSignature);
			var resolver = new MockDnsResolver ();
			var verifier = new DkimVerifier (resolver);
			var dkim = message.Headers[index];

			resolver.Add ("brisbane._domainkey.football.example.com", "v=DKIM1; k=ed25519; p=11qYAYKxCrfVS/7TyWQHOg7hcvPapiMlrwIaaPcHURo=");
			resolver.Add ("test._domainkey.football.example.com", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Assert.That (verifier.Verify (message, dkim), Is.True, "Failed to verify ed25519-sha256");
		}

		[Test]
		public void TestVerifyRfc8463Example ()
		{
			using var message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "dkim", "rfc8463-example.msg"));
			var resolver = new MockDnsResolver ();
			var verifier = new DkimVerifier (resolver);
			int index;

			resolver.Add ("brisbane._domainkey.football.example.com", "v=DKIM1; k=ed25519; p=11qYAYKxCrfVS/7TyWQHOg7hcvPapiMlrwIaaPcHURo=");
			resolver.Add ("test._domainkey.football.example.com", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			// the last DKIM-Signature uses rsa-sha256
			index = message.Headers.LastIndexOf (HeaderId.DkimSignature);
			Assert.That (verifier.Verify (message, message.Headers[index]), Is.True, "Failed to verify rsa-sha256");

			// the first DKIM-Signature uses ed25519-sha256
			index = message.Headers.IndexOf (HeaderId.DkimSignature);
			Assert.That (verifier.Verify (message, message.Headers[index]), Is.True, "Failed to verify ed25519-sha256");
		}

		[Test]
		public async Task TestVerifyRfc8463ExampleAsync ()
		{
			using var message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "dkim", "rfc8463-example.msg"));
			var resolver = new MockDnsResolver ();
			var verifier = new DkimVerifier (resolver);
			int index;

			resolver.Add ("brisbane._domainkey.football.example.com", "v=DKIM1; k=ed25519; p=11qYAYKxCrfVS/7TyWQHOg7hcvPapiMlrwIaaPcHURo=");
			resolver.Add ("test._domainkey.football.example.com", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			// the last DKIM-Signature uses rsa-sha256
			index = message.Headers.LastIndexOf (HeaderId.DkimSignature);
			Assert.That (await verifier.VerifyAsync (message, message.Headers[index]), Is.True, "Failed to verify rsa-sha256");

			// the first DKIM-Signature uses ed25519-sha256
			index = message.Headers.IndexOf (HeaderId.DkimSignature);
			Assert.That (await verifier.VerifyAsync (message, message.Headers[index]), Is.True, "Failed to verify ed25519-sha256");
		}

		static void TestDkimSignVerify (MimeMessage message, DkimSignatureAlgorithm signatureAlgorithm, DkimCanonicalizationAlgorithm headerAlgorithm, DkimCanonicalizationAlgorithm bodyAlgorithm)
		{
			var headers = new HeaderId[] { HeaderId.From, HeaderId.Subject, HeaderId.Date };
			var verifier = new DkimVerifier (new DummyDnsResolver (DkimKeys.Public));
			var signer = CreateSigner (signatureAlgorithm, headerAlgorithm, bodyAlgorithm);

			signer.Sign (message, headers);

			var dkim = message.Headers[0];

			if (signatureAlgorithm == DkimSignatureAlgorithm.RsaSha1) {
				Assert.That (verifier.Verify (message, dkim), Is.False, "DKIM-Signature using rsa-sha1 should not verify.");

				// now enable rsa-sha1 to verify again, this time it should pass...
				verifier.Enable (DkimSignatureAlgorithm.RsaSha1);
			}

			Assert.That (verifier.Verify (message, dkim), Is.True, "Failed to verify DKIM-Signature.");

			message.Headers.RemoveAt (0);
		}

		[Test]
		//[Ignore]
		public void TestDkimSignVerifyJwzMbox ()
		{
			using (var stream = File.OpenRead (Path.Combine (TestHelper.ProjectDir, "TestData", "mbox", "jwz.mbox.txt"))) {
				var parser = new MimeParser (stream, MimeFormat.Mbox);
				int i = 0;

				while (!parser.IsEndOfStream && i < 10) {
					using (var message = parser.ParseMessage ()) {
						TestDkimSignVerify (message, DkimSignatureAlgorithm.RsaSha1,
							DkimCanonicalizationAlgorithm.Relaxed,
							DkimCanonicalizationAlgorithm.Relaxed);

						TestDkimSignVerify (message, DkimSignatureAlgorithm.RsaSha256,
							DkimCanonicalizationAlgorithm.Relaxed,
							DkimCanonicalizationAlgorithm.Simple);

						TestDkimSignVerify (message, DkimSignatureAlgorithm.RsaSha1,
							DkimCanonicalizationAlgorithm.Simple,
							DkimCanonicalizationAlgorithm.Relaxed);

						TestDkimSignVerify (message, DkimSignatureAlgorithm.RsaSha256,
							DkimCanonicalizationAlgorithm.Simple,
							DkimCanonicalizationAlgorithm.Simple);
					}

					i++;
				}
			}
		}
	}
}

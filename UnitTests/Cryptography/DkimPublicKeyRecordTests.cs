//
// DkimPublicKeyRecordTests.cs
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

using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;

using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class DkimPublicKeyRecordTests
	{
		const string RsaKey = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3idY6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lxj+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB";
		const string Ed25519Key = "11qYAYKxCrfVS/7TyWQHOg7hcvPapiMlrwIaaPcHURo=";

		[TestCase ("", TestName = "TestTryParseEmpty")]
		[TestCase ("     ", TestName = "TestTryParseWhiteSpace")]
		[TestCase ("v=DKIM1; x=abc; y=def", TestName = "TestTryParseNoKeyOrPublicKey")]
		[TestCase ("v=DKIM1; k=rsa", TestName = "TestTryParseNoPublicKey")]
		[TestCase ("v=DKIM1; k=dummy; p=" + RsaKey, TestName = "TestTryParseUnknownKeyType")]
		[TestCase ("v=DKIM2; p=" + RsaKey, TestName = "TestTryParseUnknownVersion")]
		[TestCase ("k=rsa; v=DKIM1; p=" + RsaKey, TestName = "TestTryParseVersionNotFirst")]
		[TestCase ("v=DKIM1; p=" + RsaKey + "; p=" + RsaKey, TestName = "TestTryParseDuplicateTags")]
		[TestCase ("v=DKIM1; s=other; p=" + RsaKey, TestName = "TestTryParseNonEmailService")]
		[TestCase ("v=DKIM1; p=not*base64", TestName = "TestTryParseInvalidBase64")]
		[TestCase ("v=DKIM1; p=AAAA", TestName = "TestTryParseInvalidRsaKey")]
		[TestCase ("v=DKIM1; k=ed25519; p=" + RsaKey, TestName = "TestTryParseInvalidEd25519KeyLength")]
		[TestCase ("v=DKIM1; 1=abc; p=" + RsaKey, TestName = "TestTryParseInvalidTagName")]
		[TestCase ("v=DKIM1; k; p=" + RsaKey, TestName = "TestTryParseMissingEquals")]
		public void TestTryParseInvalid (string text)
		{
			Assert.That (DkimPublicKeyRecord.TryParse (text, out var record), Is.False);
			Assert.That (record, Is.Null);
		}

		[Test]
		public void TestTryParseMissingKeyTypeDefaultsToRsa ()
		{
			// Note: the p= value has been folded with whitespace which should be ignored.
			var text = "v=DKIM1; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB";

			Assert.That (DkimPublicKeyRecord.TryParse (text, out var record), Is.True);
			Assert.That (record.KeyType, Is.EqualTo (DkimKeyType.Rsa));
			Assert.That (record.PublicKey, Is.InstanceOf<RsaKeyParameters> ());
			Assert.That (record.IsRevoked, Is.False);
			Assert.That (record.IsTesting, Is.False);
			Assert.That (record.IsStrict, Is.False);
			Assert.That (record.HashAlgorithms, Is.Null);
			Assert.That (record.IsCompatible (DkimSignatureAlgorithm.RsaSha256), Is.True);
			Assert.That (record.IsCompatible (DkimSignatureAlgorithm.RsaSha1), Is.True);
			Assert.That (record.IsCompatible (DkimSignatureAlgorithm.Ed25519Sha256), Is.False);
		}

		[Test]
		public void TestTryParseWithoutVersion ()
		{
			Assert.That (DkimPublicKeyRecord.TryParse ("k=rsa; p=" + RsaKey, out var record), Is.True);
			Assert.That (record.PublicKey, Is.InstanceOf<RsaKeyParameters> ());
		}

		[Test]
		public void TestTryParseEmptyTagSpecs ()
		{
			Assert.That (DkimPublicKeyRecord.TryParse (";; v=DKIM1 ;; k = rsa ; p = " + RsaKey + " ;", out var record), Is.True);
			Assert.That (record.PublicKey, Is.InstanceOf<RsaKeyParameters> ());
		}

		[Test]
		public void TestTryParsePkcs1RsaPublicKey ()
		{
			var spki = (RsaKeyParameters) PublicKeyFactory.CreateKey (Convert.FromBase64String (RsaKey));
			var pkcs1 = new RsaPublicKeyStructure (spki.Modulus, spki.Exponent).GetEncoded ();

			Assert.That (DkimPublicKeyRecord.TryParse ("v=DKIM1; k=rsa; p=" + Convert.ToBase64String (pkcs1), out var record), Is.True);

			var rsa = (RsaKeyParameters) record.PublicKey;

			Assert.That (rsa.Modulus, Is.EqualTo (spki.Modulus));
			Assert.That (rsa.Exponent, Is.EqualTo (spki.Exponent));
		}

		[Test]
		public void TestTryParseEd25519 ()
		{
			Assert.That (DkimPublicKeyRecord.TryParse ("v=DKIM1; k=ed25519; p=" + Ed25519Key, out var record), Is.True);
			Assert.That (record.KeyType, Is.EqualTo (DkimKeyType.Ed25519));
			Assert.That (record.PublicKey, Is.InstanceOf<Ed25519PublicKeyParameters> ());
			Assert.That (record.IsCompatible (DkimSignatureAlgorithm.Ed25519Sha256), Is.True);
			Assert.That (record.IsCompatible (DkimSignatureAlgorithm.RsaSha256), Is.False);
		}

		[Test]
		public void TestTryParseRevoked ()
		{
			Assert.That (DkimPublicKeyRecord.TryParse ("v=DKIM1; k=rsa; p=", out var record), Is.True);
			Assert.That (record.IsRevoked, Is.True);
			Assert.That (record.PublicKey, Is.Null);
		}

		[Test]
		public void TestTryParseHashAlgorithms ()
		{
			Assert.That (DkimPublicKeyRecord.TryParse ("v=DKIM1; h=sha256 : future; p=" + RsaKey, out var record), Is.True);
			Assert.That (record.HashAlgorithms, Is.EqualTo (new[] { "sha256", "future" }));
			Assert.That (record.IsCompatible (DkimSignatureAlgorithm.RsaSha256), Is.True);
			Assert.That (record.IsCompatible (DkimSignatureAlgorithm.RsaSha1), Is.False);
		}

		[Test]
		public void TestTryParseServiceTypes ()
		{
			Assert.That (DkimPublicKeyRecord.TryParse ("v=DKIM1; s=*; p=" + RsaKey, out _), Is.True);
			Assert.That (DkimPublicKeyRecord.TryParse ("v=DKIM1; s=other:email; p=" + RsaKey, out _), Is.True);
		}

		[Test]
		public void TestTryParseFlags ()
		{
			Assert.That (DkimPublicKeyRecord.TryParse ("v=DKIM1; t=y:s:unknown; p=" + RsaKey, out var record), Is.True);
			Assert.That (record.IsTesting, Is.True);
			Assert.That (record.IsStrict, Is.True);
		}

		[Test]
		public void TestTagValueListMaxTags ()
		{
			var text = string.Concat (Enumerable.Range (0, TagValueList.MaxTags).Select (i => $"t{i}=v;"));

			Assert.That (TagValueList.TryParse (text, out var tags), Is.True);
			Assert.That (tags, Has.Count.EqualTo (TagValueList.MaxTags));

			Assert.That (TagValueList.TryParse (text + "x=y", out tags), Is.False);
		}

		[Test]
		public void TestTagValueListSplitColonList ()
		{
			Assert.That (TagValueList.SplitColonList (" a : b ::c: "), Is.EqualTo (new[] { "a", "b", "c" }));
			Assert.That (TagValueList.SplitColonList (string.Empty), Is.Empty);
		}

		[TestCase ("Example.COM", "example.com")]
		[TestCase ("example.com.", "example.com")]
		[TestCase ("sel_1._domainkey.example.com", "sel_1._domainkey.example.com")]
		[TestCase ("b\u00FCcher.example", "xn--bcher-kva.example")]
		public void TestDnsDomainNameTryNormalize (string domain, string expected)
		{
			Assert.That (DnsDomainName.TryNormalize (domain, out var normalized), Is.True);
			Assert.That (normalized, Is.EqualTo (expected));
		}

		[TestCase (null)]
		[TestCase ("")]
		[TestCase (".")]
		[TestCase ("example..com")]
		[TestCase (".example.com")]
		[TestCase ("exa mple.com")]
		[TestCase ("example.com/")]
		public void TestDnsDomainNameTryNormalizeInvalid (string domain)
		{
			Assert.That (DnsDomainName.TryNormalize (domain, out var normalized), Is.False);
			Assert.That (normalized, Is.Null);
		}

		[Test]
		public void TestDnsDomainNameLengthLimits ()
		{
			var label63 = new string ('a', 63);

			Assert.That (DnsDomainName.TryNormalize (label63 + ".com", out _), Is.True);
			Assert.That (DnsDomainName.TryNormalize (label63 + "a.com", out _), Is.False);

			// 4 * 63 + 3 dots = 255 characters
			var tooLong = string.Join (".", label63, label63, label63, label63);

			Assert.That (DnsDomainName.TryNormalize (tooLong, out _), Is.False);

			var domain = string.Join (".", label63, label63, label63, new string ('a', 59)); // 251 characters

			Assert.That (DnsDomainName.TryNormalize (domain, out _), Is.True);
			Assert.That (DnsDomainName.TryCombine ("a", domain, out var name), Is.True);
			Assert.That (name, Has.Length.EqualTo (253));
			Assert.That (DnsDomainName.TryCombine ("ab", domain, out name), Is.False);
			Assert.That (name, Is.Null);
		}

		[Test]
		public void TestDnsTxtResponse ()
		{
			var response = new DnsTxtResponse (new[] { "a", "b" });

			Assert.That (response.Status, Is.EqualTo (DnsQueryStatus.Success));
			Assert.That (response.Records, Is.EqualTo (new[] { "a", "b" }));

			response = new DnsTxtResponse (DnsQueryStatus.NonExistentDomain);

			Assert.That (response.Status, Is.EqualTo (DnsQueryStatus.NonExistentDomain));
			Assert.That (response.Records, Is.Empty);

			Assert.Throws<ArgumentNullException> (() => new DnsTxtResponse ((IEnumerable<string>) null));
			Assert.Throws<ArgumentException> (() => new DnsTxtResponse (new string[] { "a", null }));
			Assert.Throws<ArgumentOutOfRangeException> (() => new DnsTxtResponse ((DnsQueryStatus) 42));
		}
	}
}

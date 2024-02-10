//
// AuthenticationResultsTests.cs
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

using System.Text;

using MimeKit;
using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class AuthenticationResultsTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			AuthenticationResults authres;
			var buffer = new byte[16];

			Assert.Throws<ArgumentNullException> (() => new AuthenticationResults (null));

			Assert.Throws<ArgumentNullException> (() => new AuthenticationMethodResult (null));
			Assert.Throws<ArgumentNullException> (() => new AuthenticationMethodResult (null, "result"));
			Assert.Throws<ArgumentNullException> (() => new AuthenticationMethodResult ("method", null));

			Assert.Throws<ArgumentNullException> (() => new AuthenticationMethodProperty (null, "property", "value"));
			Assert.Throws<ArgumentNullException> (() => new AuthenticationMethodProperty ("ptype", null, "value"));
			Assert.Throws<ArgumentNullException> (() => new AuthenticationMethodProperty ("ptype", "Property", null));

			Assert.Throws<ArgumentNullException> (() => AuthenticationResults.Parse (null));
			Assert.Throws<ArgumentNullException> (() => AuthenticationResults.Parse (null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => AuthenticationResults.Parse (buffer, -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => AuthenticationResults.Parse (buffer, 0, -1));

			Assert.Throws<ArgumentNullException> (() => AuthenticationResults.TryParse (null, out authres));
			Assert.Throws<ArgumentNullException> (() => AuthenticationResults.TryParse (null, 0, 0, out authres));
			Assert.Throws<ArgumentOutOfRangeException> (() => AuthenticationResults.TryParse (buffer, -1, 0, out authres));
			Assert.Throws<ArgumentOutOfRangeException> (() => AuthenticationResults.TryParse (buffer, 0, -1, out authres));
		}

		[Test]
		public void TestEncodeLongAuthServId ()
		{
			const string authservid = "this-is-a-really-really-really-long-authserv-identifier-that-is-78-octets-long";
			const string expected = "Authentication-Results:\n\t" + authservid + ";\n\tdkim=pass; spf=pass\n";
			var encoded = new StringBuilder ("Authentication-Results:");
			var authres = new AuthenticationResults (authservid);
			var options = FormatOptions.Default.Clone ();

			authres.Results.Add (new AuthenticationMethodResult ("dkim", "pass"));
			authres.Results.Add (new AuthenticationMethodResult ("spf", "pass"));

			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, encoded.Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestEncodeLongAuthServIdWithVersion ()
		{
			const string authservid = "this-is-a-really-really-really-long-authserv-identifier-that-is-78-octets-long";
			const string expected = "Authentication-Results:\n\t" + authservid + "\n\t1; dkim=pass; spf=pass\n";
			var encoded = new StringBuilder ("Authentication-Results:");
			var authres = new AuthenticationResults (authservid);
			var options = FormatOptions.Default.Clone ();

			authres.Results.Add (new AuthenticationMethodResult ("dkim", "pass"));
			authres.Results.Add (new AuthenticationMethodResult ("spf", "pass"));
			authres.Version = 1;

			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, encoded.Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestEncodeLongResultMethod ()
		{
			const string expected = "Authentication-Results: lists.example.com 1;\n\treally-long-method-name=really-long-value\n";
			var encoded = new StringBuilder ("Authentication-Results:");
			var authres = new AuthenticationResults ("lists.example.com");
			var options = FormatOptions.Default.Clone ();

			authres.Results.Add (new AuthenticationMethodResult ("really-long-method-name", "really-long-value"));
			authres.Version = 1;

			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, encoded.Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestEncodeLongResultMethodWithVersion ()
		{
			const string expected = "Authentication-Results: lists.example.com 1;\n\treally-long-method-name/1=really-long-value\n";
			var encoded = new StringBuilder ("Authentication-Results:");
			var authres = new AuthenticationResults ("lists.example.com");
			var options = FormatOptions.Default.Clone ();

			authres.Results.Add (new AuthenticationMethodResult ("really-long-method-name", "really-long-value") {
				Version = 1
			});
			authres.Version = 1;

			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, encoded.Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestEncodeQuotedPropertyValue ()
		{
			const string expected = "Authentication-Results: lists.example.com 1;\n\tfoo=pass (2 of 3 tests OK) ptype.prop=\"value1;value2\"\n";
			var encoded = new StringBuilder ("Authentication-Results:");
			var authres = new AuthenticationResults ("lists.example.com");
			var options = FormatOptions.Default.Clone ();

			authres.Results.Add (new AuthenticationMethodResult ("foo", "pass"));
			authres.Results[0].ResultComment = "2 of 3 tests OK";
			authres.Results[0].Properties.Add (new AuthenticationMethodProperty ("ptype", "prop", "value1;value2"));
			authres.Version = 1;

			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, encoded.Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestEncodeLongResultMethodWithVersionAndComment ()
		{
			const string expected = "Authentication-Results: lists.example.com 1;\n\treally-long-method-name/1=really-long-value\n\t(this is a really long result comment)\n";
			var encoded = new StringBuilder ("Authentication-Results:");
			var authres = new AuthenticationResults ("lists.example.com");
			var options = FormatOptions.Default.Clone ();

			authres.Results.Add (new AuthenticationMethodResult ("really-long-method-name", "really-long-value") {
				ResultComment = "this is a really long result comment",
				Version = 1
			});
			authres.Version = 1;

			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, encoded.Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestEncodeLongResultMethodWithVersionAndCommentAndReason ()
		{
			const string expected = "Authentication-Results: lists.example.com 1;\n\treally-really-really-long-method-name/214748367=\n\treally-really-really-long-value (this is a really really long result comment)\n\treason=\"this is a really really really long reason\"\n\tthis-is-a-really-really-long-ptype.this-is-a-really-really-long-property=\n\tthis-is-a-really-really-long-value\n";
			var encoded = new StringBuilder ("Authentication-Results:");
			var authres = new AuthenticationResults ("lists.example.com");
			var options = FormatOptions.Default.Clone ();

			authres.Results.Add (new AuthenticationMethodResult ("really-really-really-long-method-name", "really-really-really-long-value") {
				ResultComment = "this is a really really long result comment",
				Reason = "this is a really really really long reason",
				Version = 214748367
			});
			authres.Results[0].Properties.Add (new AuthenticationMethodProperty ("this-is-a-really-really-long-ptype", "this-is-a-really-really-long-property", "this-is-a-really-really-long-value"));
			authres.Version = 1;

			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, encoded.Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestEncodeLongOffice365RandomDomainTokensAndAction ()
		{
			const string expected = "Authentication-Results: lists.example.com 1;\n\tspf=fail (sender IP is 1.1.1.1) smtp.mailfrom=eu-west-1.amazonses.com;\n\treally-really-long-receivingdomain.com; dkim=pass (signature was verified)\n\theader.d=domain.com; another-really-really-long-receivingdomain.com;\n\tdmarc=bestguesspass action=\"none\" header.from=domain.com\n";
			var encoded = new StringBuilder ("Authentication-Results:");
			var authres = new AuthenticationResults ("lists.example.com");
			var options = FormatOptions.Default.Clone ();

			authres.Results.Add (new AuthenticationMethodResult ("spf", "fail") {
				ResultComment = "sender IP is 1.1.1.1",
			});
			authres.Results[0].Properties.Add (new AuthenticationMethodProperty ("smtp", "mailfrom", "eu-west-1.amazonses.com"));
			authres.Results.Add (new AuthenticationMethodResult ("dkim", "pass") {
				Office365AuthenticationServiceIdentifier = "really-really-long-receivingdomain.com",
				ResultComment = "signature was verified",
			});
			authres.Results[1].Properties.Add (new AuthenticationMethodProperty ("header", "d", "domain.com"));
			authres.Results.Add (new AuthenticationMethodResult ("dmarc", "bestguesspass") {
				Office365AuthenticationServiceIdentifier = "another-really-really-long-receivingdomain.com",
				Action = "none",
			});
			authres.Results[2].Properties.Add (new AuthenticationMethodProperty ("header", "from", "domain.com"));
			authres.Version = 1;

			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, encoded.Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestParseArcAuthenticationResults ()
		{
			const string input = "i=1; example.com; foo=pass";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.That (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres), Is.True);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.EqualTo ("example.com"), "authserv-id");
			Assert.That (authres.Instance.Value, Is.EqualTo (1), "instance");
			Assert.That (authres.Results.Count, Is.EqualTo (1), "methods");
			Assert.That (authres.Results[0].Method, Is.EqualTo ("foo"));
			Assert.That (authres.Results[0].Result, Is.EqualTo ("pass"));

			Assert.That (authres.ToString (), Is.EqualTo (input));

			const string expected = " i=1; example.com; foo=pass\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "ARC-Authentication-Results:".Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestParseAuthServId ()
		{
			var buffer = Encoding.ASCII.GetBytes ("example.org");
			AuthenticationResults authres;

			Assert.That (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres), Is.True);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.EqualTo ("example.org"), "authserv-id");
			Assert.That (authres.ToString (), Is.EqualTo ("example.org; none"));

			authres = AuthenticationResults.Parse (buffer, 0, buffer.Length);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.EqualTo ("example.org"), "authserv-id");
			Assert.That (authres.ToString (), Is.EqualTo ("example.org; none"));

			authres = AuthenticationResults.Parse (buffer);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.EqualTo ("example.org"), "authserv-id");
			Assert.That (authres.ToString (), Is.EqualTo ("example.org; none"));

			const string expected = " example.org; none\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestParseAuthServIdSemicolon ()
		{
			var buffer = Encoding.ASCII.GetBytes ("example.org;");
			AuthenticationResults authres;

			Assert.That (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres), Is.True);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.EqualTo ("example.org"), "authserv-id");

			Assert.That (authres.ToString (), Is.EqualTo ("example.org; none"));

			const string expected = " example.org; none\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestParseAuthServIdWithVersion ()
		{
			const string input = "example.org 1";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.That (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres), Is.True);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.EqualTo ("example.org"), "authserv-id");
			Assert.That (authres.Version.Value, Is.EqualTo (1), "authres-version");

			Assert.That (authres.ToString (), Is.EqualTo ("example.org 1; none"));

			const string expected = " example.org 1; none\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestParseAuthServIdWithVersionAndSemicolon ()
		{
			var buffer = Encoding.ASCII.GetBytes ("example.org 1;");
			AuthenticationResults authres;

			Assert.That (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres), Is.True);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.EqualTo ("example.org"), "authserv-id");
			Assert.That (authres.Version.Value, Is.EqualTo (1), "authres-version");

			Assert.That (authres.ToString (), Is.EqualTo ("example.org 1; none"));

			const string expected = " example.org 1; none\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestParseNoAuthServId ()
		{
			const string input = "spf=fail (sender IP is 1.1.1.1) smtp.mailfrom=eu-west-1.amazonses.com; dkim=pass (signature was verified) header.d=domain.com; dmarc=bestguesspass header.from=domain.com";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.That (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres), Is.True);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.Null, "authserv-id");
			Assert.That (authres.Results.Count, Is.EqualTo (3), "methods");
			Assert.That (authres.Results[0].Method, Is.EqualTo ("spf"));
			Assert.That (authres.Results[0].Result, Is.EqualTo ("fail"));
			Assert.That (authres.Results[0].ResultComment, Is.EqualTo ("sender IP is 1.1.1.1"));
			Assert.That (authres.Results[0].Properties.Count, Is.EqualTo (1), "spf properties");
			Assert.That (authres.Results[0].Properties[0].PropertyType, Is.EqualTo ("smtp"));
			Assert.That (authres.Results[0].Properties[0].Property, Is.EqualTo ("mailfrom"));
			Assert.That (authres.Results[0].Properties[0].Value, Is.EqualTo ("eu-west-1.amazonses.com"));

			Assert.That (authres.Results[1].Method, Is.EqualTo ("dkim"));
			Assert.That (authres.Results[1].Result, Is.EqualTo ("pass"));
			Assert.That (authres.Results[1].ResultComment, Is.EqualTo ("signature was verified"));
			Assert.That (authres.Results[1].Properties.Count, Is.EqualTo (1), "dkim properties");
			Assert.That (authres.Results[1].Properties[0].PropertyType, Is.EqualTo ("header"));
			Assert.That (authres.Results[1].Properties[0].Property, Is.EqualTo ("d"));
			Assert.That (authres.Results[1].Properties[0].Value, Is.EqualTo ("domain.com"));

			Assert.That (authres.Results[2].Method, Is.EqualTo ("dmarc"));
			Assert.That (authres.Results[2].Result, Is.EqualTo ("bestguesspass"));
			Assert.That (authres.Results[2].ResultComment, Is.EqualTo (null));
			Assert.That (authres.Results[2].Properties.Count, Is.EqualTo (1), "dmarc properties");
			Assert.That (authres.Results[2].Properties[0].PropertyType, Is.EqualTo ("header"));
			Assert.That (authres.Results[2].Properties[0].Property, Is.EqualTo ("from"));
			Assert.That (authres.Results[2].Properties[0].Value, Is.EqualTo ("domain.com"));

			Assert.That (authres.ToString (), Is.EqualTo (input));

			const string expected = "\n\tspf=fail (sender IP is 1.1.1.1) smtp.mailfrom=eu-west-1.amazonses.com;\n\tdkim=pass (signature was verified) header.d=domain.com;\n\tdmarc=bestguesspass header.from=domain.com\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestParseNoResults ()
		{
			var buffer = Encoding.ASCII.GetBytes ("example.org 1; none");
			AuthenticationResults authres;

			Assert.That (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres), Is.True);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.EqualTo ("example.org"), "authserv-id");
			Assert.That (authres.Version.Value, Is.EqualTo (1), "authres-version");
			Assert.That (authres.Results.Count, Is.EqualTo (0), "no-results");

			Assert.That (authres.ToString (), Is.EqualTo ("example.org 1; none"));

			const string expected = " example.org 1; none\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestParseSimple ()
		{
			const string input = "example.com; foo=pass";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.That (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres), Is.True);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.EqualTo ("example.com"), "authserv-id");
			Assert.That (authres.Results.Count, Is.EqualTo (1), "methods");
			Assert.That (authres.Results[0].Method, Is.EqualTo ("foo"));
			Assert.That (authres.Results[0].Result, Is.EqualTo ("pass"));

			Assert.That (authres.ToString (), Is.EqualTo (input));

			const string expected = " example.com; foo=pass\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestParseSimpleWithComment ()
		{
			const string input = "example.com; foo=pass (2 of 3 tests OK)";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.That (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres), Is.True);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.EqualTo ("example.com"), "authserv-id");
			Assert.That (authres.Results.Count, Is.EqualTo (1), "methods");
			Assert.That (authres.Results[0].Method, Is.EqualTo ("foo"));
			Assert.That (authres.Results[0].Result, Is.EqualTo ("pass"));
			Assert.That (authres.Results[0].ResultComment, Is.EqualTo ("2 of 3 tests OK"));

			Assert.That (authres.ToString (), Is.EqualTo (input));

			const string expected = " example.com; foo=pass (2 of 3 tests OK)\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestParseSimpleWithProperty1 ()
		{
			const string input = "example.com; spf=pass smtp.mailfrom=example.net";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.That (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres), Is.True);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.EqualTo ("example.com"), "authserv-id");
			Assert.That (authres.Results.Count, Is.EqualTo (1), "methods");
			Assert.That (authres.Results[0].Method, Is.EqualTo ("spf"));
			Assert.That (authres.Results[0].Result, Is.EqualTo ("pass"));
			Assert.That (authres.Results[0].Properties.Count, Is.EqualTo (1), "properties");
			Assert.That (authres.Results[0].Properties[0].PropertyType, Is.EqualTo ("smtp"));
			Assert.That (authres.Results[0].Properties[0].Property, Is.EqualTo ("mailfrom"));
			Assert.That (authres.Results[0].Properties[0].Value, Is.EqualTo ("example.net"));

			Assert.That (authres.ToString (), Is.EqualTo (input));

			const string expected = " example.com; spf=pass smtp.mailfrom=example.net\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestParseSimpleWithProperty2 ()
		{
			const string input = "example.com; spf=pass smtp.mailfrom=@example.net";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.That (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres), Is.True);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.EqualTo ("example.com"), "authserv-id");
			Assert.That (authres.Results.Count, Is.EqualTo (1), "methods");
			Assert.That (authres.Results[0].Method, Is.EqualTo ("spf"));
			Assert.That (authres.Results[0].Result, Is.EqualTo ("pass"));
			Assert.That (authres.Results[0].Properties.Count, Is.EqualTo (1), "properties");
			Assert.That (authres.Results[0].Properties[0].PropertyType, Is.EqualTo ("smtp"));
			Assert.That (authres.Results[0].Properties[0].Property, Is.EqualTo ("mailfrom"));
			Assert.That (authres.Results[0].Properties[0].Value, Is.EqualTo ("@example.net"));

			Assert.That (authres.ToString (), Is.EqualTo (input));

			const string expected = " example.com; spf=pass smtp.mailfrom=@example.net\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestParseSimpleWithProperty3 ()
		{
			const string input = "example.com; spf=pass smtp.mailfrom=local-part@example.net";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.That (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres), Is.True);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.EqualTo ("example.com"), "authserv-id");
			Assert.That (authres.Results.Count, Is.EqualTo (1), "methods");
			Assert.That (authres.Results[0].Method, Is.EqualTo ("spf"));
			Assert.That (authres.Results[0].Result, Is.EqualTo ("pass"));
			Assert.That (authres.Results[0].Properties.Count, Is.EqualTo (1), "properties");
			Assert.That (authres.Results[0].Properties[0].PropertyType, Is.EqualTo ("smtp"));
			Assert.That (authres.Results[0].Properties[0].Property, Is.EqualTo ("mailfrom"));
			Assert.That (authres.Results[0].Properties[0].Value, Is.EqualTo ("local-part@example.net"));

			Assert.That (authres.ToString (), Is.EqualTo (input));

			const string expected = " example.com;\n\tspf=pass smtp.mailfrom=local-part@example.net\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestParseSimpleWithQuotedPropertyValue ()
		{
			const string input = "example.com; method=pass ptype.prop=\"value1;value2\"";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.That (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres), Is.True);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.EqualTo ("example.com"), "authserv-id");
			Assert.That (authres.Results.Count, Is.EqualTo (1), "methods");
			Assert.That (authres.Results[0].Method, Is.EqualTo ("method"));
			Assert.That (authres.Results[0].Result, Is.EqualTo ("pass"));
			Assert.That (authres.Results[0].Properties.Count, Is.EqualTo (1), "properties");
			Assert.That (authres.Results[0].Properties[0].PropertyType, Is.EqualTo ("ptype"));
			Assert.That (authres.Results[0].Properties[0].Property, Is.EqualTo ("prop"));
			Assert.That (authres.Results[0].Properties[0].Value, Is.EqualTo ("value1;value2"));

			Assert.That (authres.ToString (), Is.EqualTo (input));

			const string expected = " example.com; method=pass ptype.prop=\"value1;value2\"\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestParseSimpleWithReason ()
		{
			const string input = "example.com; spf=pass reason=good";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.That (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres), Is.True);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.EqualTo ("example.com"), "authserv-id");
			Assert.That (authres.Results.Count, Is.EqualTo (1), "methods");
			Assert.That (authres.Results[0].Method, Is.EqualTo ("spf"));
			Assert.That (authres.Results[0].Result, Is.EqualTo ("pass"));
			Assert.That (authres.Results[0].Reason, Is.EqualTo ("good"));
			Assert.That (authres.Results[0].Properties.Count, Is.EqualTo (0), "properties");

			Assert.That (authres.ToString (), Is.EqualTo ("example.com; spf=pass reason=\"good\""));

			const string expected = " example.com; spf=pass reason=\"good\"\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestParseSimpleWithReasonSemiColon ()
		{
			const string input = "example.com; spf=pass reason=good; ";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.That (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres), Is.True);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.EqualTo ("example.com"), "authserv-id");
			Assert.That (authres.Results.Count, Is.EqualTo (1), "methods");
			Assert.That (authres.Results[0].Method, Is.EqualTo ("spf"));
			Assert.That (authres.Results[0].Result, Is.EqualTo ("pass"));
			Assert.That (authres.Results[0].Reason, Is.EqualTo ("good"));
			Assert.That (authres.Results[0].Properties.Count, Is.EqualTo (0), "properties");

			Assert.That (authres.ToString (), Is.EqualTo ("example.com; spf=pass reason=\"good\""));

			const string expected = " example.com; spf=pass reason=\"good\"\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestParseSimpleWithQuotedReason ()
		{
			const string input = "example.com; spf=pass reason=\"good stuff\"";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.That (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres), Is.True);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.EqualTo ("example.com"), "authserv-id");
			Assert.That (authres.Results.Count, Is.EqualTo (1), "methods");
			Assert.That (authres.Results[0].Method, Is.EqualTo ("spf"));
			Assert.That (authres.Results[0].Result, Is.EqualTo ("pass"));
			Assert.That (authres.Results[0].Reason, Is.EqualTo ("good stuff"));
			Assert.That (authres.Results[0].Properties.Count, Is.EqualTo (0), "properties");

			Assert.That (authres.ToString (), Is.EqualTo (input));

			const string expected = " example.com; spf=pass reason=\"good stuff\"\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestParseSimpleWithQuotedReasonSemiColon ()
		{
			const string input = "example.com; spf=pass reason=\"good stuff\"";
			var buffer = Encoding.ASCII.GetBytes (input + "; ");
			AuthenticationResults authres;

			Assert.That (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres), Is.True);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.EqualTo ("example.com"), "authserv-id");
			Assert.That (authres.Results.Count, Is.EqualTo (1), "methods");
			Assert.That (authres.Results[0].Method, Is.EqualTo ("spf"));
			Assert.That (authres.Results[0].Result, Is.EqualTo ("pass"));
			Assert.That (authres.Results[0].Reason, Is.EqualTo ("good stuff"));
			Assert.That (authres.Results[0].Properties.Count, Is.EqualTo (0), "properties");

			Assert.That (authres.ToString (), Is.EqualTo (input));

			const string expected = " example.com; spf=pass reason=\"good stuff\"\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestParseMethodWithMultipleProperties ()
		{
			const string input = "example.com; spf=pass ptype1.prop1=value1 ptype2.prop2=value2";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.That (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres), Is.True);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.EqualTo ("example.com"), "authserv-id");
			Assert.That (authres.Results.Count, Is.EqualTo (1), "methods");
			Assert.That (authres.Results[0].Method, Is.EqualTo ("spf"));
			Assert.That (authres.Results[0].Result, Is.EqualTo ("pass"));
			Assert.That (authres.Results[0].Properties.Count, Is.EqualTo (2), "properties");
			Assert.That (authres.Results[0].Properties[0].PropertyType, Is.EqualTo ("ptype1"));
			Assert.That (authres.Results[0].Properties[0].Property, Is.EqualTo ("prop1"));
			Assert.That (authres.Results[0].Properties[0].Value, Is.EqualTo ("value1"));
			Assert.That (authres.Results[0].Properties[1].PropertyType, Is.EqualTo ("ptype2"));
			Assert.That (authres.Results[0].Properties[1].Property, Is.EqualTo ("prop2"));
			Assert.That (authres.Results[0].Properties[1].Value, Is.EqualTo ("value2"));

			Assert.That (authres.ToString (), Is.EqualTo (input));

			const string expected = " example.com;\n\tspf=pass ptype1.prop1=value1 ptype2.prop2=value2\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestParseMultipleMethods ()
		{
			const string input = "example.com; auth=pass (cram-md5) smtp.auth=sender@example.net; spf=pass smtp.mailfrom=example.net; sender-id=pass header.from=example.net";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.That (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres), Is.True);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.EqualTo ("example.com"), "authserv-id");
			Assert.That (authres.Results.Count, Is.EqualTo (3), "methods");
			Assert.That (authres.Results[0].Method, Is.EqualTo ("auth"));
			Assert.That (authres.Results[0].Result, Is.EqualTo ("pass"));
			Assert.That (authres.Results[0].ResultComment, Is.EqualTo ("cram-md5"));
			Assert.That (authres.Results[0].Properties.Count, Is.EqualTo (1), "auth properties");
			Assert.That (authres.Results[0].Properties[0].PropertyType, Is.EqualTo ("smtp"));
			Assert.That (authres.Results[0].Properties[0].Property, Is.EqualTo ("auth"));
			Assert.That (authres.Results[0].Properties[0].Value, Is.EqualTo ("sender@example.net"));
			Assert.That (authres.Results[1].Method, Is.EqualTo ("spf"));
			Assert.That (authres.Results[1].Result, Is.EqualTo ("pass"));
			Assert.That (authres.Results[1].Properties.Count, Is.EqualTo (1), "spf properties");
			Assert.That (authres.Results[1].Properties[0].PropertyType, Is.EqualTo ("smtp"));
			Assert.That (authres.Results[1].Properties[0].Property, Is.EqualTo ("mailfrom"));
			Assert.That (authres.Results[1].Properties[0].Value, Is.EqualTo ("example.net"));
			Assert.That (authres.Results[2].Method, Is.EqualTo ("sender-id"));
			Assert.That (authres.Results[2].Result, Is.EqualTo ("pass"));
			Assert.That (authres.Results[2].Properties.Count, Is.EqualTo (1), "sender-id properties");
			Assert.That (authres.Results[2].Properties[0].PropertyType, Is.EqualTo ("header"));
			Assert.That (authres.Results[2].Properties[0].Property, Is.EqualTo ("from"));
			Assert.That (authres.Results[2].Properties[0].Value, Is.EqualTo ("example.net"));

			Assert.That (authres.ToString (), Is.EqualTo (input));

			const string expected = " example.com;\n\tauth=pass (cram-md5) smtp.auth=sender@example.net;\n\tspf=pass smtp.mailfrom=example.net; sender-id=pass header.from=example.net\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestParseMultipleMethodsWithReasons ()
		{
			const string input = "example.com; dkim=pass reason=\"good signature\" header.i=@mail-router.example.net; dkim=fail reason=\"bad signature\" header.i=@newyork.example.com";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.That (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres), Is.True);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.EqualTo ("example.com"), "authserv-id");
			Assert.That (authres.Results.Count, Is.EqualTo (2), "methods");
			Assert.That (authres.Results[0].Method, Is.EqualTo ("dkim"));
			Assert.That (authres.Results[0].Result, Is.EqualTo ("pass"));
			Assert.That (authres.Results[0].Reason, Is.EqualTo ("good signature"));
			Assert.That (authres.Results[0].Properties.Count, Is.EqualTo (1), "dkim properties");
			Assert.That (authres.Results[0].Properties[0].PropertyType, Is.EqualTo ("header"));
			Assert.That (authres.Results[0].Properties[0].Property, Is.EqualTo ("i"));
			Assert.That (authres.Results[0].Properties[0].Value, Is.EqualTo ("@mail-router.example.net"));
			Assert.That (authres.Results[1].Method, Is.EqualTo ("dkim"));
			Assert.That (authres.Results[1].Result, Is.EqualTo ("fail"));
			Assert.That (authres.Results[1].Reason, Is.EqualTo ("bad signature"));
			Assert.That (authres.Results[1].Properties.Count, Is.EqualTo (1), "dkim properties");
			Assert.That (authres.Results[1].Properties[0].PropertyType, Is.EqualTo ("header"));
			Assert.That (authres.Results[1].Properties[0].Property, Is.EqualTo ("i"));
			Assert.That (authres.Results[1].Properties[0].Value, Is.EqualTo ("@newyork.example.com"));

			Assert.That (authres.ToString (), Is.EqualTo (input));

			const string expected = " example.com;\n\tdkim=pass reason=\"good signature\" header.i=@mail-router.example.net;\n\tdkim=fail reason=\"bad signature\" header.i=@newyork.example.com\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestParseHeavilyCommentedExample ()
		{
			var buffer = Encoding.ASCII.GetBytes ("foo.example.net (foobar) 1 (baz); dkim (Because I like it) / 1 (One yay) = (wait for it) fail policy (A dot can go here) . (like that) expired (this surprised me) = (as I wasn't expecting it) 1362471462");
			AuthenticationResults authres;

			Assert.That (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres), Is.True);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.EqualTo ("foo.example.net"), "authserv-id");
			Assert.That (authres.Version.Value, Is.EqualTo (1), "authres-version");
			Assert.That (authres.Results.Count, Is.EqualTo (1), "methods");
			Assert.That (authres.Results[0].Method, Is.EqualTo ("dkim"));
			Assert.That (authres.Results[0].Version.Value, Is.EqualTo (1), "method-version");
			Assert.That (authres.Results[0].Result, Is.EqualTo ("fail"));
			Assert.That (authres.Results[0].Properties.Count, Is.EqualTo (1), "dkim properties");
			Assert.That (authres.Results[0].Properties[0].PropertyType, Is.EqualTo ("policy"));
			Assert.That (authres.Results[0].Properties[0].Property, Is.EqualTo ("expired"));
			Assert.That (authres.Results[0].Properties[0].Value, Is.EqualTo ("1362471462"));

			Assert.That (authres.ToString (), Is.EqualTo ("foo.example.net 1; dkim/1=fail policy.expired=1362471462"));

			const string expected = " foo.example.net 1;\n\tdkim/1=fail policy.expired=1362471462\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		// Tests work-around for https://github.com/jstedfast/MimeKit/issues/518
		[Test]
		public void TestParseMethodPropertyValueWithSlash ()
		{
			const string input = "i=2; test.com; dkim=pass header.d=test.com header.s=selector1 header.b=Iww3/TIUS; dmarc=pass (policy=reject) header.from=test.com; spf=pass (test.com: domain of no-reply@test.com designates 1.1.1.1 as permitted sender) smtp.mailfrom=no-reply@test.com";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.That (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres), Is.True);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.EqualTo ("test.com"), "authserv-id");
			Assert.That (authres.Results.Count, Is.EqualTo (3), "methods");
			Assert.That (authres.Results[0].Method, Is.EqualTo ("dkim"));
			Assert.That (authres.Results[0].Result, Is.EqualTo ("pass"));
			Assert.That (authres.Results[0].Properties.Count, Is.EqualTo (3), "dkim properties");
			Assert.That (authres.Results[0].Properties[0].PropertyType, Is.EqualTo ("header"));
			Assert.That (authres.Results[0].Properties[0].Property, Is.EqualTo ("d"));
			Assert.That (authres.Results[0].Properties[0].Value, Is.EqualTo ("test.com"));
			Assert.That (authres.Results[0].Properties[1].PropertyType, Is.EqualTo ("header"));
			Assert.That (authres.Results[0].Properties[1].Property, Is.EqualTo ("s"));
			Assert.That (authres.Results[0].Properties[1].Value, Is.EqualTo ("selector1"));
			Assert.That (authres.Results[0].Properties[2].PropertyType, Is.EqualTo ("header"));
			Assert.That (authres.Results[0].Properties[2].Property, Is.EqualTo ("b"));
			Assert.That (authres.Results[0].Properties[2].Value, Is.EqualTo ("Iww3/TIUS"));

			Assert.That (authres.Results[1].Method, Is.EqualTo ("dmarc"));
			Assert.That (authres.Results[1].Result, Is.EqualTo ("pass"));
			Assert.That (authres.Results[1].ResultComment, Is.EqualTo ("policy=reject"));
			Assert.That (authres.Results[1].Properties.Count, Is.EqualTo (1), "dmarc properties");
			Assert.That (authres.Results[1].Properties[0].PropertyType, Is.EqualTo ("header"));
			Assert.That (authres.Results[1].Properties[0].Property, Is.EqualTo ("from"));
			Assert.That (authres.Results[1].Properties[0].Value, Is.EqualTo ("test.com"));

			Assert.That (authres.Results[2].Method, Is.EqualTo ("spf"));
			Assert.That (authres.Results[2].Result, Is.EqualTo ("pass"));
			Assert.That (authres.Results[2].ResultComment, Is.EqualTo ("test.com: domain of no-reply@test.com designates 1.1.1.1 as permitted sender"));
			Assert.That (authres.Results[2].Properties.Count, Is.EqualTo (1), "spf properties");
			Assert.That (authres.Results[2].Properties[0].PropertyType, Is.EqualTo ("smtp"));
			Assert.That (authres.Results[2].Properties[0].Property, Is.EqualTo ("mailfrom"));
			Assert.That (authres.Results[2].Properties[0].Value, Is.EqualTo ("no-reply@test.com"));

			Assert.That (authres.ToString (), Is.EqualTo (input));

			const string expected = " i=2; test.com;\n\tdkim=pass header.d=test.com header.s=selector1 header.b=Iww3/TIUS;\n\tdmarc=pass (policy=reject) header.from=test.com; spf=pass\n\t(test.com: domain of no-reply@test.com designates 1.1.1.1 as permitted sender)\n\tsmtp.mailfrom=no-reply@test.com\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		// Tests work-around for https://github.com/jstedfast/MimeKit/issues/490
		[Test]
		public void TestParseOffice365RandomDomainTokensAndAction ()
		{
			var buffer = Encoding.ASCII.GetBytes ("spf=fail (sender IP is 1.1.1.1) smtp.mailfrom=eu-west-1.amazonses.com; receivingdomain.com; dkim=pass (signature was verified) header.d=domain.com;domain1.com; dmarc=bestguesspass action=none header.from=domain.com;");
			AuthenticationResults authres;

			Assert.That (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres), Is.True);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.Null, "authserv-id");
			Assert.That (authres.Results.Count, Is.EqualTo (3), "methods");
			Assert.That (authres.Results[0].Method, Is.EqualTo ("spf"));
			Assert.That (authres.Results[0].Result, Is.EqualTo ("fail"));
			Assert.That (authres.Results[0].ResultComment, Is.EqualTo ("sender IP is 1.1.1.1"));
			Assert.That (authres.Results[0].Properties.Count, Is.EqualTo (1), "spf properties");
			Assert.That (authres.Results[0].Properties[0].PropertyType, Is.EqualTo ("smtp"));
			Assert.That (authres.Results[0].Properties[0].Property, Is.EqualTo ("mailfrom"));
			Assert.That (authres.Results[0].Properties[0].Value, Is.EqualTo ("eu-west-1.amazonses.com"));

			Assert.That (authres.Results[1].Office365AuthenticationServiceIdentifier, Is.EqualTo ("receivingdomain.com"));
			Assert.That (authres.Results[1].Method, Is.EqualTo ("dkim"));
			Assert.That (authres.Results[1].Result, Is.EqualTo ("pass"));
			Assert.That (authres.Results[1].ResultComment, Is.EqualTo ("signature was verified"));
			Assert.That (authres.Results[1].Properties.Count, Is.EqualTo (1), "dkim properties");
			Assert.That (authres.Results[1].Properties[0].PropertyType, Is.EqualTo ("header"));
			Assert.That (authres.Results[1].Properties[0].Property, Is.EqualTo ("d"));
			Assert.That (authres.Results[1].Properties[0].Value, Is.EqualTo ("domain.com"));

			Assert.That (authres.Results[2].Office365AuthenticationServiceIdentifier, Is.EqualTo ("domain1.com"));
			Assert.That (authres.Results[2].Method, Is.EqualTo ("dmarc"));
			Assert.That (authres.Results[2].Result, Is.EqualTo ("bestguesspass"));
			Assert.That (authres.Results[2].ResultComment, Is.EqualTo (null));
			Assert.That (authres.Results[2].Action, Is.EqualTo ("none"));
			Assert.That (authres.Results[2].Properties.Count, Is.EqualTo (1), "dmarc properties");
			Assert.That (authres.Results[2].Properties[0].PropertyType, Is.EqualTo ("header"));
			Assert.That (authres.Results[2].Properties[0].Property, Is.EqualTo ("from"));
			Assert.That (authres.Results[2].Properties[0].Value, Is.EqualTo ("domain.com"));

			Assert.That (authres.ToString (), Is.EqualTo ("spf=fail (sender IP is 1.1.1.1) smtp.mailfrom=eu-west-1.amazonses.com; receivingdomain.com; dkim=pass (signature was verified) header.d=domain.com; domain1.com; dmarc=bestguesspass action=\"none\" header.from=domain.com"));

			const string expected = "\n\tspf=fail (sender IP is 1.1.1.1) smtp.mailfrom=eu-west-1.amazonses.com;\n\treceivingdomain.com; dkim=pass (signature was verified) header.d=domain.com;\n\tdomain1.com; dmarc=bestguesspass action=\"none\" header.from=domain.com\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		// Tests work-around for https://github.com/jstedfast/MimeKit/issues/527
		[Test]
		public void TestParseOffice365RandomDomainTokensAndEmptyPropertyValue ()
		{
			const string input = "spf=temperror (sender IP is 1.1.1.1) smtp.helo=tes.test.ru; mydomain.com; dkim=none (message not signed) header.d=none;mydomain.com; dmarc=none action=none header.from=;";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.That (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres), Is.True);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.EqualTo (null), "authserv-id");
			Assert.That (authres.Results.Count, Is.EqualTo (3), "methods");
			Assert.That (authres.Results[0].Method, Is.EqualTo ("spf"));
			Assert.That (authres.Results[0].Result, Is.EqualTo ("temperror"));
			Assert.That (authres.Results[0].ResultComment, Is.EqualTo ("sender IP is 1.1.1.1"));
			Assert.That (authres.Results[0].Properties.Count, Is.EqualTo (1), "spf properties");
			Assert.That (authres.Results[0].Properties[0].PropertyType, Is.EqualTo ("smtp"));
			Assert.That (authres.Results[0].Properties[0].Property, Is.EqualTo ("helo"));
			Assert.That (authres.Results[0].Properties[0].Value, Is.EqualTo ("tes.test.ru"));

			Assert.That (authres.Results[1].Office365AuthenticationServiceIdentifier, Is.EqualTo ("mydomain.com"));
			Assert.That (authres.Results[1].Method, Is.EqualTo ("dkim"));
			Assert.That (authres.Results[1].Result, Is.EqualTo ("none"));
			Assert.That (authres.Results[1].ResultComment, Is.EqualTo ("message not signed"));
			Assert.That (authres.Results[1].Properties.Count, Is.EqualTo (1), "dkim properties");
			Assert.That (authres.Results[1].Properties[0].PropertyType, Is.EqualTo ("header"));
			Assert.That (authres.Results[1].Properties[0].Property, Is.EqualTo ("d"));
			Assert.That (authres.Results[1].Properties[0].Value, Is.EqualTo ("none"));

			Assert.That (authres.Results[2].Office365AuthenticationServiceIdentifier, Is.EqualTo ("mydomain.com"));
			Assert.That (authres.Results[2].Method, Is.EqualTo ("dmarc"));
			Assert.That (authres.Results[2].Result, Is.EqualTo ("none"));
			Assert.That (authres.Results[2].ResultComment, Is.EqualTo (null));
			Assert.That (authres.Results[2].Action, Is.EqualTo ("none"));
			Assert.That (authres.Results[2].Properties.Count, Is.EqualTo (1), "dmarc properties");
			Assert.That (authres.Results[2].Properties[0].PropertyType, Is.EqualTo ("header"));
			Assert.That (authres.Results[2].Properties[0].Property, Is.EqualTo ("from"));
			Assert.That (authres.Results[2].Properties[0].Value, Is.EqualTo (""));

			Assert.That (authres.ToString (), Is.EqualTo ("spf=temperror (sender IP is 1.1.1.1) smtp.helo=tes.test.ru; mydomain.com; dkim=none (message not signed) header.d=none; mydomain.com; dmarc=none action=\"none\" header.from="));

			const string expected = "\n\tspf=temperror (sender IP is 1.1.1.1) smtp.helo=tes.test.ru;\n\tmydomain.com; dkim=none (message not signed) header.d=none;\n\tmydomain.com; dmarc=none action=\"none\" header.from=\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		// Tests work-around for https://github.com/jstedfast/MimeKit/issues/584
		[Test]
		public void TestParseMethodResultWithUnderscore ()
		{
			const string input = " atlas122.free.mail.gq1.yahoo.com; dkim=dkim_pass header.i=@news.aegeanair.com header.s=@aegeanair2; spf=pass smtp.mailfrom=news.aegeanair.com; dmarc=success(p=REJECT) header.from=news.aegeanair.com;";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.That (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres), Is.True);
			Assert.That (authres.AuthenticationServiceIdentifier, Is.EqualTo ("atlas122.free.mail.gq1.yahoo.com"), "authserv-id");
			Assert.That (authres.Results.Count, Is.EqualTo (3), "methods");
			Assert.That (authres.Results[0].Method, Is.EqualTo ("dkim"));
			Assert.That (authres.Results[0].Result, Is.EqualTo ("dkim_pass"));
			Assert.That (authres.Results[0].ResultComment, Is.EqualTo (null));
			Assert.That (authres.Results[0].Properties.Count, Is.EqualTo (2), "dkim properties");
			Assert.That (authres.Results[0].Properties[0].PropertyType, Is.EqualTo ("header"));
			Assert.That (authres.Results[0].Properties[0].Property, Is.EqualTo ("i"));
			Assert.That (authres.Results[0].Properties[0].Value, Is.EqualTo ("@news.aegeanair.com"));
			Assert.That (authres.Results[0].Properties[1].PropertyType, Is.EqualTo ("header"));
			Assert.That (authres.Results[0].Properties[1].Property, Is.EqualTo ("s"));
			Assert.That (authres.Results[0].Properties[1].Value, Is.EqualTo ("@aegeanair2"));

			Assert.That (authres.Results[1].Method, Is.EqualTo ("spf"));
			Assert.That (authres.Results[1].Result, Is.EqualTo ("pass"));
			Assert.That (authres.Results[1].ResultComment, Is.EqualTo (null));
			Assert.That (authres.Results[1].Properties.Count, Is.EqualTo (1), "spf properties");
			Assert.That (authres.Results[1].Properties[0].PropertyType, Is.EqualTo ("smtp"));
			Assert.That (authres.Results[1].Properties[0].Property, Is.EqualTo ("mailfrom"));
			Assert.That (authres.Results[1].Properties[0].Value, Is.EqualTo ("news.aegeanair.com"));

			Assert.That (authres.Results[2].Method, Is.EqualTo ("dmarc"));
			Assert.That (authres.Results[2].Result, Is.EqualTo ("success"));
			Assert.That (authres.Results[2].ResultComment, Is.EqualTo ("p=REJECT"));
			Assert.That (authres.Results[2].Properties.Count, Is.EqualTo (1), "dmarc properties");
			Assert.That (authres.Results[2].Properties[0].PropertyType, Is.EqualTo ("header"));
			Assert.That (authres.Results[2].Properties[0].Property, Is.EqualTo ("from"));
			Assert.That (authres.Results[2].Properties[0].Value, Is.EqualTo ("news.aegeanair.com"));

			Assert.That (authres.ToString (), Is.EqualTo ("atlas122.free.mail.gq1.yahoo.com; dkim=dkim_pass header.i=@news.aegeanair.com header.s=@aegeanair2; spf=pass smtp.mailfrom=news.aegeanair.com; dmarc=success (p=REJECT) header.from=news.aegeanair.com"));

			const string expected = " atlas122.free.mail.gq1.yahoo.com;\n\tdkim=dkim_pass header.i=@news.aegeanair.com header.s=@aegeanair2;\n\tspf=pass smtp.mailfrom=news.aegeanair.com;\n\tdmarc=success (p=REJECT) header.from=news.aegeanair.com\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		// Tests work-around for https://github.com/jstedfast/MimeKit/issues/590
		[Test]
		public void TestParsePropertyWithEqualSignInValue ()
		{
			const string input = "i=1; relay.mailrelay.com; dkim=pass header.d=domaina.com header.s=sfdc header.b=abcefg; dmarc=pass (policy=quarantine) header.from=domaina.com; spf=pass (relay.mailrelay.com: domain of support=domaina.com__0-1q6woix34obtbu@823lwd90ky2ahf.mail_sender.com designates 1.1.1.1 as permitted sender) smtp.mailfrom=support=domaina.com__0-1q6woix34obtbu@823lwd90ky2ahf.mail_sender.com";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.That (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres), Is.True);
			Assert.That (authres.Instance.Value, Is.EqualTo (1), "i");
			Assert.That (authres.AuthenticationServiceIdentifier, Is.EqualTo ("relay.mailrelay.com"), "authserv-id");
			Assert.That (authres.Results.Count, Is.EqualTo (3), "methods");
			Assert.That (authres.Results[0].Method, Is.EqualTo ("dkim"));
			Assert.That (authres.Results[0].Result, Is.EqualTo ("pass"));
			Assert.That (authres.Results[0].ResultComment, Is.EqualTo (null));
			Assert.That (authres.Results[0].Properties.Count, Is.EqualTo (3), "dkim properties");
			Assert.That (authres.Results[0].Properties[0].PropertyType, Is.EqualTo ("header"));
			Assert.That (authres.Results[0].Properties[0].Property, Is.EqualTo ("d"));
			Assert.That (authres.Results[0].Properties[0].Value, Is.EqualTo ("domaina.com"));
			Assert.That (authres.Results[0].Properties[1].PropertyType, Is.EqualTo ("header"));
			Assert.That (authres.Results[0].Properties[1].Property, Is.EqualTo ("s"));
			Assert.That (authres.Results[0].Properties[1].Value, Is.EqualTo ("sfdc"));
			Assert.That (authres.Results[0].Properties[2].PropertyType, Is.EqualTo ("header"));
			Assert.That (authres.Results[0].Properties[2].Property, Is.EqualTo ("b"));
			Assert.That (authres.Results[0].Properties[2].Value, Is.EqualTo ("abcefg"));

			Assert.That (authres.Results[1].Method, Is.EqualTo ("dmarc"));
			Assert.That (authres.Results[1].Result, Is.EqualTo ("pass"));
			Assert.That (authres.Results[1].ResultComment, Is.EqualTo ("policy=quarantine"));
			Assert.That (authres.Results[1].Properties.Count, Is.EqualTo (1), "spf properties");
			Assert.That (authres.Results[1].Properties[0].PropertyType, Is.EqualTo ("header"));
			Assert.That (authres.Results[1].Properties[0].Property, Is.EqualTo ("from"));
			Assert.That (authres.Results[1].Properties[0].Value, Is.EqualTo ("domaina.com"));

			Assert.That (authres.Results[2].Method, Is.EqualTo ("spf"));
			Assert.That (authres.Results[2].Result, Is.EqualTo ("pass"));
			Assert.That (authres.Results[2].ResultComment, Is.EqualTo ("relay.mailrelay.com: domain of support=domaina.com__0-1q6woix34obtbu@823lwd90ky2ahf.mail_sender.com designates 1.1.1.1 as permitted sender"));
			Assert.That (authres.Results[2].Properties.Count, Is.EqualTo (1), "dmarc properties");
			Assert.That (authres.Results[2].Properties[0].PropertyType, Is.EqualTo ("smtp"));
			Assert.That (authres.Results[2].Properties[0].Property, Is.EqualTo ("mailfrom"));
			Assert.That (authres.Results[2].Properties[0].Value, Is.EqualTo ("support=domaina.com__0-1q6woix34obtbu@823lwd90ky2ahf.mail_sender.com"));

			Assert.That (authres.ToString (), Is.EqualTo ("i=1; relay.mailrelay.com; dkim=pass header.d=domaina.com header.s=sfdc header.b=abcefg; dmarc=pass (policy=quarantine) header.from=domaina.com; spf=pass (relay.mailrelay.com: domain of support=domaina.com__0-1q6woix34obtbu@823lwd90ky2ahf.mail_sender.com designates 1.1.1.1 as permitted sender) smtp.mailfrom=support=domaina.com__0-1q6woix34obtbu@823lwd90ky2ahf.mail_sender.com"));

			const string expected = " i=1; relay.mailrelay.com;\n\tdkim=pass header.d=domaina.com header.s=sfdc header.b=abcefg;\n\tdmarc=pass (policy=quarantine) header.from=domaina.com; spf=pass\n\t(relay.mailrelay.com: domain of support=domaina.com__0-1q6woix34obtbu@823lwd90ky2ahf.mail_sender.com designates 1.1.1.1 as permitted sender)\n\tsmtp.mailfrom=\n\tsupport=domaina.com__0-1q6woix34obtbu@823lwd90ky2ahf.mail_sender.com\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.That (encoded.ToString (), Is.EqualTo (expected));
		}

		static void AssertParseFailure (string input, int tokenIndex, int errorIndex)
		{
			var buffer = Encoding.ASCII.GetBytes (input);

			Assert.That (AuthenticationResults.TryParse (buffer, out AuthenticationResults authres), Is.False);

			try {
				AuthenticationResults.Parse (buffer);
				Assert.Fail ("Expected parse error.");
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "TokenIndex");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "ErrorIndex");
			}

			try {
				AuthenticationResults.Parse (buffer, 0, buffer.Length);
				Assert.Fail ("Expected parse error.");
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "TokenIndex");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "ErrorIndex");
			}
		}

		[Test]
		public void TestParseFailureAuthServIdIncompleteQString ()
		{
			AssertParseFailure (" \"quoted-authserv-id", 1, 20);
		}

		[Test]
		public void TestParseFailureIncompleteComment ()
		{
			AssertParseFailure (" (truncated comment", 1, 19);
		}

		[Test]
		public void TestParseFailureIncompleteCommentAfterAuthServId ()
		{
			AssertParseFailure (" authserv-id (truncated comment", 13, 31);
		}

		[Test]
		public void TestParseFailureIncompleteCommentAfterAuthServIdVersion ()
		{
			AssertParseFailure (" authserv-id 1 (truncated comment", 15, 33);
		}

		[Test]
		public void TestParseFailureIncompleteCommentAfterInstanceEquals ()
		{
			AssertParseFailure (" i= (truncated comment", 4, 22);
		}

		[Test]
		public void TestParseFailureIncompleteCommentAfterInstanceEqualsValue ()
		{
			AssertParseFailure (" i=1 (truncated comment", 5, 23);
		}

		[Test]
		public void TestParseFailureIncompleteCommentAfterInstanceEqualsValueSemiColon ()
		{
			AssertParseFailure (" i=1; (truncated comment", 6, 24);
		}

		[Test]
		public void TestParseFailureIncompleteCommentBeforeMethod ()
		{
			AssertParseFailure (" authserv-id; (incomplete comment", 14, 33);
		}

		[Test]
		public void TestParseFailureIncompleteCommentAfterMethod ()
		{
			AssertParseFailure (" authserv-id; method (incomplete comment", 21, 40);
		}

		[Test]
		public void TestParseFailureIncompleteCommentAfterMethodEquals ()
		{
			AssertParseFailure (" authserv-id; method= (incomplete comment", 22, 41);
		}

		[Test]
		public void TestParseFailureIncompleteCommentAfterMethodEqualsResult ()
		{
			AssertParseFailure (" authserv-id; method=result (incomplete comment", 28, 47);
		}

		[Test]
		public void TestParseFailureIncompleteCommentAfterMethodEqualsResultComment ()
		{
			AssertParseFailure (" authserv-id; method=result (comment) (incomplete comment", 38, 57);
		}

		[Test]
		public void TestParseFailureIncompleteCommentAfterMethodSlash ()
		{
			AssertParseFailure (" authserv-id; method/ (incomplete comment", 22, 41);
		}

		[Test]
		public void TestParseFailureIncompleteCommentAfterMethodVersion ()
		{
			AssertParseFailure (" authserv-id; method/1 (incomplete comment", 23, 42);
		}

		[Test]
		public void TestParseFailureIncompleteCommentAfterMethodVersionEquals ()
		{
			AssertParseFailure (" authserv-id; method/1= (incomplete comment", 24, 43);
		}

		[Test]
		public void TestParseFailureIncompleteCommentAfterReason ()
		{
			AssertParseFailure ("authserv-id; method=pass reason (truncated comment", 32, 50);
		}

		[Test]
		public void TestParseFailureIncompleteCommentAfterReasonEquals ()
		{
			AssertParseFailure ("authserv-id; method=pass reason= (truncated comment", 33, 51);
		}

		[Test]
		public void TestParseFailureIncompleteCommentAfterReasonEqualsValue ()
		{
			AssertParseFailure ("authserv-id; method=pass reason=value (truncated comment", 38, 56);
		}

		[Test]
		public void TestParseFailureIncompleteCommentAfterPType ()
		{
			AssertParseFailure ("authserv-id; method=pass ptype (truncated comment", 31, 49);
		}

		[Test]
		public void TestParseFailureIncompleteCommentAfterPTypeDot ()
		{
			AssertParseFailure ("authserv-id; method=pass ptype. (truncated comment", 32, 50);
		}

		[Test]
		public void TestParseFailureIncompleteCommentAfterPTypeDotProp ()
		{
			AssertParseFailure ("authserv-id; method=pass ptype.prop (truncated comment", 36, 54);
		}

		[Test]
		public void TestParseFailureIncompleteCommentAfterPTypeDotPropEquals ()
		{
			AssertParseFailure ("authserv-id; method=pass ptype.prop= (truncated comment", 37, 55);
		}

		[Test]
		public void TestParseFailureIncompleteCommentAfterPTypeDotPropEqualsValue ()
		{
			AssertParseFailure ("authserv-id; method=pass ptype.prop=value (truncated comment", 42, 60);
		}

		[Test]
		public void TestParseFailureIncompleteArcInstance ()
		{
			AssertParseFailure ("i=", 2, 2);
		}

		[Test]
		public void TestParseFailureInvalidArcInstance ()
		{
			AssertParseFailure ("i=abc; authserv-id", 2, 2);
		}

		[Test]
		public void TestParseFailureUnexpectedTokenAfterArcInstance ()
		{
			AssertParseFailure ("i=1: authserv-id", 3, 3);
		}

		[Test]
		public void TestParseFailureOnlyArcInstance ()
		{
			AssertParseFailure ("i=5", 2, 3);
		}

		[Test]
		public void TestParseFailureOnlyArcInstanceSemicolon ()
		{
			AssertParseFailure ("i=5;", 4, 4);
		}

		[Test]
		public void TestParseFailureMultipleLeadingArcInstance ()
		{
			AssertParseFailure ("i=5; i=1", 5, 6);
		}

		[Test]
		public void TestParseFailureInvalidVersion ()
		{
			AssertParseFailure ("authserv-id x", 12, 12);
		}

		[Test]
		public void TestParseFailureInvalidTokenAfterVersion ()
		{
			AssertParseFailure ("authserv-id 1 x", 14, 14);
		}

		[Test]
		public void TestParseFailureInvalidMethod1 ()
		{
			AssertParseFailure ("authserv-id; .", 13, 13);
		}

		[Test]
		public void TestParseFailureInvalidMethod2 ()
		{
			AssertParseFailure ("authserv-id; abc", 13, 16);
		}

		[Test]
		public void TestParseFailureInvalidMethod3 ()
		{
			AssertParseFailure ("authserv-id; abc def", 13, 17);
		}

		[Test]
		public void TestParseFailureInvalidMethodVersion1 ()
		{
			AssertParseFailure ("authserv-id; abc/1 ", 13, 19);
		}

		[Test]
		public void TestParseFailureInvalidMethodVersion2 ()
		{
			AssertParseFailure ("authserv-id; abc/1.0=pass", 13, 18);
		}

		[Test]
		public void TestParseFailureInvalidMethodVersion3 ()
		{
			AssertParseFailure ("authserv-id; abc/def=pass", 17, 17);
		}

		[Test]
		public void TestParseFailureIncompleteMethod ()
		{
			AssertParseFailure ("authserv-id; abc=", 13, 17);
		}

		[Test]
		public void TestParseFailureMethodEqualNonKeyword ()
		{
			AssertParseFailure ("authserv-id; abc=.", 17, 17);
		}

		[Test]
		public void TestParseFailureNoResultWithMore ()
		{
			AssertParseFailure ("authserv-id; none; method=pass", 13, 17);
		}

		[Test]
		public void TestParseFailureNoResultAfterMethods ()
		{
			AssertParseFailure ("authserv-id; method=pass; none", 26, 30);
		}

		[Test]
		public void TestParseFailureIncompleteResultComment ()
		{
			AssertParseFailure ("authserv-id; method=pass (truncated comment", 25, 43);
		}

		[Test]
		public void TestParseFailureInvalidTokenAfterResult ()
		{
			AssertParseFailure ("authserv-id; method=pass .", 25, 25);
		}

		[Test]
		public void TestParseFailureIncompleteReason1 ()
		{
			AssertParseFailure ("authserv-id; method=pass reason", 25, 31);
		}

		[Test]
		public void TestParseFailureIncompleteReason2 ()
		{
			AssertParseFailure ("authserv-id; method=pass reason=", 32, 32);
		}

		[Test]
		public void TestParseFailureIncompleteReason3 ()
		{
			AssertParseFailure ("authserv-id; method=pass reason=\"this is some text", 32, 50);
		}

		[Test]
		public void TestParseFailureIncompleteReason4 ()
		{
			AssertParseFailure ("authserv-id; method=pass reason=;", 32, 32);
		}

		[Test]
		public void TestParseFailureInvalidReason ()
		{
			AssertParseFailure ("authserv-id; method=pass reason .", 25, 32);
		}

		[Test]
		public void TestParseFailureInvalidPropTypeAfterReason ()
		{
			AssertParseFailure ("authserv-id; method=pass reason=\"because I said so\" .;", 52, 52);
		}

		[Test]
		public void TestParseFailureIncompleteProperty1 ()
		{
			AssertParseFailure ("authserv-id; method=pass ptype", 25, 30);
		}

		[Test]
		public void TestParseFailureIncompleteProperty2 ()
		{
			AssertParseFailure ("authserv-id; method=pass ptype.", 25, 31);
		}

		[Test]
		public void TestParseFailureIncompleteProperty3 ()
		{
			AssertParseFailure ("authserv-id; method=pass ptype.prop", 25, 35);
		}

		[Test]
		public void TestParseFailureIncompleteProperty4 ()
		{
			AssertParseFailure ("authserv-id; method=pass ptype.prop=", 25, 36);
		}

		[Test]
		public void TestParseFailureIncompleteProperty5 ()
		{
			AssertParseFailure ("authserv-id; method=pass ptype.prop=\"incomplete qstring", 25, 55);
		}

		// Note: TestParseFailureIncompleteProperty6 is commented out because of
		// https://github.com/jstedfast/MimeKit/issues/527 where we have "header.from=;"

		//[Test]
		//public void TestParseFailureIncompleteProperty6 ()
		//{
		//	AssertParseFailure ("authserv-id; method=pass ptype.prop=;", 25, 36);
		//}

		[Test]
		public void TestParseFailureInvalidProperty1 ()
		{
			AssertParseFailure ("authserv-id; method=pass ptype;", 25, 30);
		}

		[Test]
		public void TestParseFailureInvalidProperty2 ()
		{
			AssertParseFailure ("authserv-id; method=pass ptype.prop;", 25, 35);
		}

		[Test]
		public void TestParseFailureInvalidProperty3 ()
		{
			AssertParseFailure ("authserv-id; method=pass ptype.prop=value .", 42, 42);
		}

		[Test]
		public void TestParseFailureInvalidProperty4 ()
		{
			AssertParseFailure ("authserv-id; method=pass ptype..", 31, 31);
		}

		[Test]
		public void TestParseFailureInvalidOffice365AuthServId ()
		{
			AssertParseFailure ("authserv-id; method=pass ptype.prop=pvalue; invalid.office365.domain..; method=pass", 44, 69);
		}

		[Test]
		public void TestParseFailureTruncatedOffice365AuthServId ()
		{
			AssertParseFailure ("authserv-id; method=pass ptype.prop=pvalue; truncated.office365.domain", 44, 70);
		}

		[Test]
		public void TestParseFailureUnexpectedTokenAfterOffice365AuthServId ()
		{
			AssertParseFailure ("authserv-id; method=pass ptype.prop=pvalue; office365.domain :", 61, 61);
		}
	}
}

//
// AuthenticationResultsTests.cs
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
using System.Text;

using NUnit.Framework;

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

			Assert.AreEqual (expected, encoded.ToString ());
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

			Assert.AreEqual (expected, encoded.ToString ());
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

			Assert.AreEqual (expected, encoded.ToString ());
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

			Assert.AreEqual (expected, encoded.ToString ());
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

			Assert.AreEqual (expected, encoded.ToString ());
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

			Assert.AreEqual (expected, encoded.ToString ());
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

			Assert.AreEqual (expected, encoded.ToString ());
		}

		[Test]
		public void TestParseArcAuthenticationResults ()
		{
			const string input = "i=1; example.com; foo=pass";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.IsTrue (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres));
			Assert.AreEqual ("example.com", authres.AuthenticationServiceIdentifier, "authserv-id");
			Assert.AreEqual (1, authres.Instance.Value, "instance");
			Assert.AreEqual (1, authres.Results.Count, "methods");
			Assert.AreEqual ("foo", authres.Results[0].Method);
			Assert.AreEqual ("pass", authres.Results[0].Result);

			Assert.AreEqual (input, authres.ToString ());

			const string expected = " i=1; example.com; foo=pass\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "ARC-Authentication-Results:".Length);

			Assert.AreEqual (expected, encoded.ToString ());
		}

		[Test]
		public void TestParseAuthServId ()
		{
			var buffer = Encoding.ASCII.GetBytes ("example.org");
			AuthenticationResults authres;

			Assert.IsTrue (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres));
			Assert.AreEqual ("example.org", authres.AuthenticationServiceIdentifier, "authserv-id");

			Assert.AreEqual ("example.org; none", authres.ToString ());

			const string expected = " example.org; none\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.AreEqual (expected, encoded.ToString ());
		}

		[Test]
		public void TestParseAuthServIdSemicolon ()
		{
			var buffer = Encoding.ASCII.GetBytes ("example.org;");
			AuthenticationResults authres;

			Assert.IsTrue (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres));
			Assert.AreEqual ("example.org", authres.AuthenticationServiceIdentifier, "authserv-id");

			Assert.AreEqual ("example.org; none", authres.ToString ());

			const string expected = " example.org; none\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.AreEqual (expected, encoded.ToString ());
		}

		[Test]
		public void TestParseAuthServIdWithVersion ()
		{
			const string input = "example.org 1";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.IsTrue (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres));
			Assert.AreEqual ("example.org", authres.AuthenticationServiceIdentifier, "authserv-id");
			Assert.AreEqual (1, authres.Version.Value, "authres-version");

			Assert.AreEqual ("example.org 1; none", authres.ToString ());

			const string expected = " example.org 1; none\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.AreEqual (expected, encoded.ToString ());
		}

		[Test]
		public void TestParseAuthServIdWithVersionAndSemicolon ()
		{
			var buffer = Encoding.ASCII.GetBytes ("example.org 1;");
			AuthenticationResults authres;

			Assert.IsTrue (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres));
			Assert.AreEqual ("example.org", authres.AuthenticationServiceIdentifier, "authserv-id");
			Assert.AreEqual (1, authres.Version.Value, "authres-version");

			Assert.AreEqual ("example.org 1; none", authres.ToString ());

			const string expected = " example.org 1; none\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.AreEqual (expected, encoded.ToString ());
		}

		[Test]
		public void TestParseNoAuthServId ()
		{
			const string input = "spf=fail (sender IP is 1.1.1.1) smtp.mailfrom=eu-west-1.amazonses.com; dkim=pass (signature was verified) header.d=domain.com; dmarc=bestguesspass header.from=domain.com";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.IsTrue (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres));
			Assert.IsNull (authres.AuthenticationServiceIdentifier, "authserv-id");
			Assert.AreEqual (3, authres.Results.Count, "methods");
			Assert.AreEqual ("spf", authres.Results[0].Method);
			Assert.AreEqual ("fail", authres.Results[0].Result);
			Assert.AreEqual ("sender IP is 1.1.1.1", authres.Results[0].ResultComment);
			Assert.AreEqual (1, authres.Results[0].Properties.Count, "spf properties");
			Assert.AreEqual ("smtp", authres.Results[0].Properties[0].PropertyType);
			Assert.AreEqual ("mailfrom", authres.Results[0].Properties[0].Property);
			Assert.AreEqual ("eu-west-1.amazonses.com", authres.Results[0].Properties[0].Value);

			Assert.AreEqual ("dkim", authres.Results[1].Method);
			Assert.AreEqual ("pass", authres.Results[1].Result);
			Assert.AreEqual ("signature was verified", authres.Results[1].ResultComment);
			Assert.AreEqual (1, authres.Results[1].Properties.Count, "dkim properties");
			Assert.AreEqual ("header", authres.Results[1].Properties[0].PropertyType);
			Assert.AreEqual ("d", authres.Results[1].Properties[0].Property);
			Assert.AreEqual ("domain.com", authres.Results[1].Properties[0].Value);

			Assert.AreEqual ("dmarc", authres.Results[2].Method);
			Assert.AreEqual ("bestguesspass", authres.Results[2].Result);
			Assert.AreEqual (null, authres.Results[2].ResultComment);
			Assert.AreEqual (1, authres.Results[2].Properties.Count, "dmarc properties");
			Assert.AreEqual ("header", authres.Results[2].Properties[0].PropertyType);
			Assert.AreEqual ("from", authres.Results[2].Properties[0].Property);
			Assert.AreEqual ("domain.com", authres.Results[2].Properties[0].Value);

			Assert.AreEqual (input, authres.ToString ());

			const string expected = "\n\tspf=fail (sender IP is 1.1.1.1) smtp.mailfrom=eu-west-1.amazonses.com;\n\tdkim=pass (signature was verified) header.d=domain.com;\n\tdmarc=bestguesspass header.from=domain.com\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.AreEqual (expected, encoded.ToString ());
		}

		[Test]
		public void TestParseNoResults ()
		{
			var buffer = Encoding.ASCII.GetBytes ("example.org 1; none");
			AuthenticationResults authres;

			Assert.IsTrue (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres));
			Assert.AreEqual ("example.org", authres.AuthenticationServiceIdentifier, "authserv-id");
			Assert.AreEqual (1, authres.Version.Value, "authres-version");
			Assert.AreEqual (0, authres.Results.Count, "no-results");

			Assert.AreEqual ("example.org 1; none", authres.ToString ());

			const string expected = " example.org 1; none\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.AreEqual (expected, encoded.ToString ());
		}

		[Test]
		public void TestParseSimple ()
		{
			const string input = "example.com; foo=pass";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.IsTrue (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres));
			Assert.AreEqual ("example.com", authres.AuthenticationServiceIdentifier, "authserv-id");
			Assert.AreEqual (1, authres.Results.Count, "methods");
			Assert.AreEqual ("foo", authres.Results[0].Method);
			Assert.AreEqual ("pass", authres.Results[0].Result);

			Assert.AreEqual (input, authres.ToString ());

			const string expected = " example.com; foo=pass\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.AreEqual (expected, encoded.ToString ());
		}

		[Test]
		public void TestParseSimpleWithComment ()
		{
			const string input = "example.com; foo=pass (2 of 3 tests OK)";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.IsTrue (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres));
			Assert.AreEqual ("example.com", authres.AuthenticationServiceIdentifier, "authserv-id");
			Assert.AreEqual (1, authres.Results.Count, "methods");
			Assert.AreEqual ("foo", authres.Results[0].Method);
			Assert.AreEqual ("pass", authres.Results[0].Result);
			Assert.AreEqual ("2 of 3 tests OK", authres.Results[0].ResultComment);

			Assert.AreEqual (input, authres.ToString ());

			const string expected = " example.com; foo=pass (2 of 3 tests OK)\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.AreEqual (expected, encoded.ToString ());
		}

		[Test]
		public void TestParseSimpleWithProperty1 ()
		{
			const string input = "example.com; spf=pass smtp.mailfrom=example.net";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.IsTrue (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres));
			Assert.AreEqual ("example.com", authres.AuthenticationServiceIdentifier, "authserv-id");
			Assert.AreEqual (1, authres.Results.Count, "methods");
			Assert.AreEqual ("spf", authres.Results[0].Method);
			Assert.AreEqual ("pass", authres.Results[0].Result);
			Assert.AreEqual (1, authres.Results[0].Properties.Count, "properties");
			Assert.AreEqual ("smtp", authres.Results[0].Properties[0].PropertyType);
			Assert.AreEqual ("mailfrom", authres.Results[0].Properties[0].Property);
			Assert.AreEqual ("example.net", authres.Results[0].Properties[0].Value);

			Assert.AreEqual (input, authres.ToString ());

			const string expected = " example.com; spf=pass smtp.mailfrom=example.net\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.AreEqual (expected, encoded.ToString ());
		}

		[Test]
		public void TestParseSimpleWithProperty2 ()
		{
			const string input = "example.com; spf=pass smtp.mailfrom=@example.net";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.IsTrue (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres));
			Assert.AreEqual ("example.com", authres.AuthenticationServiceIdentifier, "authserv-id");
			Assert.AreEqual (1, authres.Results.Count, "methods");
			Assert.AreEqual ("spf", authres.Results[0].Method);
			Assert.AreEqual ("pass", authres.Results[0].Result);
			Assert.AreEqual (1, authres.Results[0].Properties.Count, "properties");
			Assert.AreEqual ("smtp", authres.Results[0].Properties[0].PropertyType);
			Assert.AreEqual ("mailfrom", authres.Results[0].Properties[0].Property);
			Assert.AreEqual ("@example.net", authres.Results[0].Properties[0].Value);

			Assert.AreEqual (input, authres.ToString ());

			const string expected = " example.com; spf=pass smtp.mailfrom=@example.net\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.AreEqual (expected, encoded.ToString ());
		}

		[Test]
		public void TestParseSimpleWithProperty3 ()
		{
			const string input = "example.com; spf=pass smtp.mailfrom=local-part@example.net";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.IsTrue (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres));
			Assert.AreEqual ("example.com", authres.AuthenticationServiceIdentifier, "authserv-id");
			Assert.AreEqual (1, authres.Results.Count, "methods");
			Assert.AreEqual ("spf", authres.Results[0].Method);
			Assert.AreEqual ("pass", authres.Results[0].Result);
			Assert.AreEqual (1, authres.Results[0].Properties.Count, "properties");
			Assert.AreEqual ("smtp", authres.Results[0].Properties[0].PropertyType);
			Assert.AreEqual ("mailfrom", authres.Results[0].Properties[0].Property);
			Assert.AreEqual ("local-part@example.net", authres.Results[0].Properties[0].Value);

			Assert.AreEqual (input, authres.ToString ());

			const string expected = " example.com;\n\tspf=pass smtp.mailfrom=local-part@example.net\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.AreEqual (expected, encoded.ToString ());
		}

		[Test]
		public void TestParseSimpleWithQuotedPropertyValue ()
		{
			const string input = "example.com; method=pass ptype.prop=\"value1;value2\"";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.IsTrue (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres));
			Assert.AreEqual ("example.com", authres.AuthenticationServiceIdentifier, "authserv-id");
			Assert.AreEqual (1, authres.Results.Count, "methods");
			Assert.AreEqual ("method", authres.Results[0].Method);
			Assert.AreEqual ("pass", authres.Results[0].Result);
			Assert.AreEqual (1, authres.Results[0].Properties.Count, "properties");
			Assert.AreEqual ("ptype", authres.Results[0].Properties[0].PropertyType);
			Assert.AreEqual ("prop", authres.Results[0].Properties[0].Property);
			Assert.AreEqual ("value1;value2", authres.Results[0].Properties[0].Value);

			Assert.AreEqual (input, authres.ToString ());

			const string expected = " example.com; method=pass ptype.prop=\"value1;value2\"\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.AreEqual (expected, encoded.ToString ());
		}

		[Test]
		public void TestParseSimpleWithReason ()
		{
			const string input = "example.com; spf=pass reason=good";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.IsTrue (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres));
			Assert.AreEqual ("example.com", authres.AuthenticationServiceIdentifier, "authserv-id");
			Assert.AreEqual (1, authres.Results.Count, "methods");
			Assert.AreEqual ("spf", authres.Results[0].Method);
			Assert.AreEqual ("pass", authres.Results[0].Result);
			Assert.AreEqual ("good", authres.Results[0].Reason);
			Assert.AreEqual (0, authres.Results[0].Properties.Count, "properties");

			Assert.AreEqual ("example.com; spf=pass reason=\"good\"", authres.ToString ());

			const string expected = " example.com; spf=pass reason=\"good\"\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.AreEqual (expected, encoded.ToString ());
		}

		[Test]
		public void TestParseSimpleWithReasonSemiColon ()
		{
			const string input = "example.com; spf=pass reason=good; ";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.IsTrue (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres));
			Assert.AreEqual ("example.com", authres.AuthenticationServiceIdentifier, "authserv-id");
			Assert.AreEqual (1, authres.Results.Count, "methods");
			Assert.AreEqual ("spf", authres.Results[0].Method);
			Assert.AreEqual ("pass", authres.Results[0].Result);
			Assert.AreEqual ("good", authres.Results[0].Reason);
			Assert.AreEqual (0, authres.Results[0].Properties.Count, "properties");

			Assert.AreEqual ("example.com; spf=pass reason=\"good\"", authres.ToString ());

			const string expected = " example.com; spf=pass reason=\"good\"\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.AreEqual (expected, encoded.ToString ());
		}

		[Test]
		public void TestParseSimpleWithQuotedReason ()
		{
			const string input = "example.com; spf=pass reason=\"good stuff\"";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.IsTrue (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres));
			Assert.AreEqual ("example.com", authres.AuthenticationServiceIdentifier, "authserv-id");
			Assert.AreEqual (1, authres.Results.Count, "methods");
			Assert.AreEqual ("spf", authres.Results[0].Method);
			Assert.AreEqual ("pass", authres.Results[0].Result);
			Assert.AreEqual ("good stuff", authres.Results[0].Reason);
			Assert.AreEqual (0, authres.Results[0].Properties.Count, "properties");

			Assert.AreEqual (input, authres.ToString ());

			const string expected = " example.com; spf=pass reason=\"good stuff\"\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.AreEqual (expected, encoded.ToString ());
		}

		[Test]
		public void TestParseSimpleWithQuotedReasonSemiColon ()
		{
			const string input = "example.com; spf=pass reason=\"good stuff\"";
			var buffer = Encoding.ASCII.GetBytes (input + "; ");
			AuthenticationResults authres;

			Assert.IsTrue (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres));
			Assert.AreEqual ("example.com", authres.AuthenticationServiceIdentifier, "authserv-id");
			Assert.AreEqual (1, authres.Results.Count, "methods");
			Assert.AreEqual ("spf", authres.Results[0].Method);
			Assert.AreEqual ("pass", authres.Results[0].Result);
			Assert.AreEqual ("good stuff", authres.Results[0].Reason);
			Assert.AreEqual (0, authres.Results[0].Properties.Count, "properties");

			Assert.AreEqual (input, authres.ToString ());

			const string expected = " example.com; spf=pass reason=\"good stuff\"\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.AreEqual (expected, encoded.ToString ());
		}

		[Test]
		public void TestParseMethodWithMultipleProperties ()
		{
			const string input = "example.com; spf=pass ptype1.prop1=value1 ptype2.prop2=value2";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.IsTrue (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres));
			Assert.AreEqual ("example.com", authres.AuthenticationServiceIdentifier, "authserv-id");
			Assert.AreEqual (1, authres.Results.Count, "methods");
			Assert.AreEqual ("spf", authres.Results[0].Method);
			Assert.AreEqual ("pass", authres.Results[0].Result);
			Assert.AreEqual (2, authres.Results[0].Properties.Count, "properties");
			Assert.AreEqual ("ptype1", authres.Results[0].Properties[0].PropertyType);
			Assert.AreEqual ("prop1", authres.Results[0].Properties[0].Property);
			Assert.AreEqual ("value1", authres.Results[0].Properties[0].Value);
			Assert.AreEqual ("ptype2", authres.Results[0].Properties[1].PropertyType);
			Assert.AreEqual ("prop2", authres.Results[0].Properties[1].Property);
			Assert.AreEqual ("value2", authres.Results[0].Properties[1].Value);

			Assert.AreEqual (input, authres.ToString ());

			const string expected = " example.com;\n\tspf=pass ptype1.prop1=value1 ptype2.prop2=value2\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.AreEqual (expected, encoded.ToString ());
		}

		[Test]
		public void TestParseMultipleMethods ()
		{
			const string input = "example.com; auth=pass (cram-md5) smtp.auth=sender@example.net; spf=pass smtp.mailfrom=example.net; sender-id=pass header.from=example.net";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.IsTrue (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres));
			Assert.AreEqual ("example.com", authres.AuthenticationServiceIdentifier, "authserv-id");
			Assert.AreEqual (3, authres.Results.Count, "methods");
			Assert.AreEqual ("auth", authres.Results[0].Method);
			Assert.AreEqual ("pass", authres.Results[0].Result);
			Assert.AreEqual ("cram-md5", authres.Results[0].ResultComment);
			Assert.AreEqual (1, authres.Results[0].Properties.Count, "auth properties");
			Assert.AreEqual ("smtp", authres.Results[0].Properties[0].PropertyType);
			Assert.AreEqual ("auth", authres.Results[0].Properties[0].Property);
			Assert.AreEqual ("sender@example.net", authres.Results[0].Properties[0].Value);
			Assert.AreEqual ("spf", authres.Results[1].Method);
			Assert.AreEqual ("pass", authres.Results[1].Result);
			Assert.AreEqual (1, authres.Results[1].Properties.Count, "spf properties");
			Assert.AreEqual ("smtp", authres.Results[1].Properties[0].PropertyType);
			Assert.AreEqual ("mailfrom", authres.Results[1].Properties[0].Property);
			Assert.AreEqual ("example.net", authres.Results[1].Properties[0].Value);
			Assert.AreEqual ("sender-id", authres.Results[2].Method);
			Assert.AreEqual ("pass", authres.Results[2].Result);
			Assert.AreEqual (1, authres.Results[2].Properties.Count, "sender-id properties");
			Assert.AreEqual ("header", authres.Results[2].Properties[0].PropertyType);
			Assert.AreEqual ("from", authres.Results[2].Properties[0].Property);
			Assert.AreEqual ("example.net", authres.Results[2].Properties[0].Value);

			Assert.AreEqual (input, authres.ToString ());

			const string expected = " example.com;\n\tauth=pass (cram-md5) smtp.auth=sender@example.net;\n\tspf=pass smtp.mailfrom=example.net; sender-id=pass header.from=example.net\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.AreEqual (expected, encoded.ToString ());
		}

		[Test]
		public void TestParseMultipleMethodsWithReasons ()
		{
			const string input = "example.com; dkim=pass reason=\"good signature\" header.i=@mail-router.example.net; dkim=fail reason=\"bad signature\" header.i=@newyork.example.com";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.IsTrue (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres));
			Assert.AreEqual ("example.com", authres.AuthenticationServiceIdentifier, "authserv-id");
			Assert.AreEqual (2, authres.Results.Count, "methods");
			Assert.AreEqual ("dkim", authres.Results[0].Method);
			Assert.AreEqual ("pass", authres.Results[0].Result);
			Assert.AreEqual ("good signature", authres.Results[0].Reason);
			Assert.AreEqual (1, authres.Results[0].Properties.Count, "dkim properties");
			Assert.AreEqual ("header", authres.Results[0].Properties[0].PropertyType);
			Assert.AreEqual ("i", authres.Results[0].Properties[0].Property);
			Assert.AreEqual ("@mail-router.example.net", authres.Results[0].Properties[0].Value);
			Assert.AreEqual ("dkim", authres.Results[1].Method);
			Assert.AreEqual ("fail", authres.Results[1].Result);
			Assert.AreEqual ("bad signature", authres.Results[1].Reason);
			Assert.AreEqual (1, authres.Results[1].Properties.Count, "dkim properties");
			Assert.AreEqual ("header", authres.Results[1].Properties[0].PropertyType);
			Assert.AreEqual ("i", authres.Results[1].Properties[0].Property);
			Assert.AreEqual ("@newyork.example.com", authres.Results[1].Properties[0].Value);

			Assert.AreEqual (input, authres.ToString ());

			const string expected = " example.com;\n\tdkim=pass reason=\"good signature\" header.i=@mail-router.example.net;\n\tdkim=fail reason=\"bad signature\" header.i=@newyork.example.com\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.AreEqual (expected, encoded.ToString ());
		}

		[Test]
		public void TestParseHeavilyCommentedExample ()
		{
			var buffer = Encoding.ASCII.GetBytes ("foo.example.net (foobar) 1 (baz); dkim (Because I like it) / 1 (One yay) = (wait for it) fail policy (A dot can go here) . (like that) expired (this surprised me) = (as I wasn't expecting it) 1362471462");
			AuthenticationResults authres;

			Assert.IsTrue (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres));
			Assert.AreEqual ("foo.example.net", authres.AuthenticationServiceIdentifier, "authserv-id");
			Assert.AreEqual (1, authres.Version.Value, "authres-version");
			Assert.AreEqual (1, authres.Results.Count, "methods");
			Assert.AreEqual ("dkim", authres.Results[0].Method);
			Assert.AreEqual (1, authres.Results[0].Version.Value, "method-version");
			Assert.AreEqual ("fail", authres.Results[0].Result);
			Assert.AreEqual (1, authres.Results[0].Properties.Count, "dkim properties");
			Assert.AreEqual ("policy", authres.Results[0].Properties[0].PropertyType);
			Assert.AreEqual ("expired", authres.Results[0].Properties[0].Property);
			Assert.AreEqual ("1362471462", authres.Results[0].Properties[0].Value);

			Assert.AreEqual ("foo.example.net 1; dkim/1=fail policy.expired=1362471462", authres.ToString ());

			const string expected = " foo.example.net 1;\n\tdkim/1=fail policy.expired=1362471462\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.AreEqual (expected, encoded.ToString ());
		}

		// Tests work-around for https://github.com/jstedfast/MimeKit/issues/518
		[Test]
		public void TestParseMethodPropertyValueWithSlash ()
		{
			const string input = "i=2; test.com; dkim=pass header.d=test.com header.s=selector1 header.b=Iww3/TIUS; dmarc=pass (policy=reject) header.from=test.com; spf=pass (test.com: domain of no-reply@test.com designates 1.1.1.1 as permitted sender) smtp.mailfrom=no-reply@test.com";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.IsTrue (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres));
			Assert.AreEqual ("test.com", authres.AuthenticationServiceIdentifier, "authserv-id");
			Assert.AreEqual (3, authres.Results.Count, "methods");
			Assert.AreEqual ("dkim", authres.Results[0].Method);
			Assert.AreEqual ("pass", authres.Results[0].Result);
			Assert.AreEqual (3, authres.Results[0].Properties.Count, "dkim properties");
			Assert.AreEqual ("header", authres.Results[0].Properties[0].PropertyType);
			Assert.AreEqual ("d", authres.Results[0].Properties[0].Property);
			Assert.AreEqual ("test.com", authres.Results[0].Properties[0].Value);
			Assert.AreEqual ("header", authres.Results[0].Properties[1].PropertyType);
			Assert.AreEqual ("s", authres.Results[0].Properties[1].Property);
			Assert.AreEqual ("selector1", authres.Results[0].Properties[1].Value);
			Assert.AreEqual ("header", authres.Results[0].Properties[2].PropertyType);
			Assert.AreEqual ("b", authres.Results[0].Properties[2].Property);
			Assert.AreEqual ("Iww3/TIUS", authres.Results[0].Properties[2].Value);

			Assert.AreEqual ("dmarc", authres.Results[1].Method);
			Assert.AreEqual ("pass", authres.Results[1].Result);
			Assert.AreEqual ("policy=reject", authres.Results[1].ResultComment);
			Assert.AreEqual (1, authres.Results[1].Properties.Count, "dmarc properties");
			Assert.AreEqual ("header", authres.Results[1].Properties[0].PropertyType);
			Assert.AreEqual ("from", authres.Results[1].Properties[0].Property);
			Assert.AreEqual ("test.com", authres.Results[1].Properties[0].Value);

			Assert.AreEqual ("spf", authres.Results[2].Method);
			Assert.AreEqual ("pass", authres.Results[2].Result);
			Assert.AreEqual ("test.com: domain of no-reply@test.com designates 1.1.1.1 as permitted sender", authres.Results[2].ResultComment);
			Assert.AreEqual (1, authres.Results[2].Properties.Count, "spf properties");
			Assert.AreEqual ("smtp", authres.Results[2].Properties[0].PropertyType);
			Assert.AreEqual ("mailfrom", authres.Results[2].Properties[0].Property);
			Assert.AreEqual ("no-reply@test.com", authres.Results[2].Properties[0].Value);

			Assert.AreEqual (input, authres.ToString ());

			const string expected = " i=2; test.com;\n\tdkim=pass header.d=test.com header.s=selector1 header.b=Iww3/TIUS;\n\tdmarc=pass (policy=reject) header.from=test.com; spf=pass\n\t(test.com: domain of no-reply@test.com designates 1.1.1.1 as permitted sender)\n\tsmtp.mailfrom=no-reply@test.com\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.AreEqual (expected, encoded.ToString ());
		}

		// Tests work-around for https://github.com/jstedfast/MimeKit/issues/490
		[Test]
		public void TestParseOffice365RandomDomainTokensAndAction ()
		{
			var buffer = Encoding.ASCII.GetBytes ("spf=fail (sender IP is 1.1.1.1) smtp.mailfrom=eu-west-1.amazonses.com; receivingdomain.com; dkim=pass (signature was verified) header.d=domain.com;domain1.com; dmarc=bestguesspass action=none header.from=domain.com;");
			AuthenticationResults authres;

			Assert.IsTrue (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres));
			Assert.IsNull (authres.AuthenticationServiceIdentifier, "authserv-id");
			Assert.AreEqual (3, authres.Results.Count, "methods");
			Assert.AreEqual ("spf", authres.Results[0].Method);
			Assert.AreEqual ("fail", authres.Results[0].Result);
			Assert.AreEqual ("sender IP is 1.1.1.1", authres.Results[0].ResultComment);
			Assert.AreEqual (1, authres.Results[0].Properties.Count, "spf properties");
			Assert.AreEqual ("smtp", authres.Results[0].Properties[0].PropertyType);
			Assert.AreEqual ("mailfrom", authres.Results[0].Properties[0].Property);
			Assert.AreEqual ("eu-west-1.amazonses.com", authres.Results[0].Properties[0].Value);

			Assert.AreEqual ("receivingdomain.com", authres.Results[1].Office365AuthenticationServiceIdentifier);
			Assert.AreEqual ("dkim", authres.Results[1].Method);
			Assert.AreEqual ("pass", authres.Results[1].Result);
			Assert.AreEqual ("signature was verified", authres.Results[1].ResultComment);
			Assert.AreEqual (1, authres.Results[1].Properties.Count, "dkim properties");
			Assert.AreEqual ("header", authres.Results[1].Properties[0].PropertyType);
			Assert.AreEqual ("d", authres.Results[1].Properties[0].Property);
			Assert.AreEqual ("domain.com", authres.Results[1].Properties[0].Value);

			Assert.AreEqual ("domain1.com", authres.Results[2].Office365AuthenticationServiceIdentifier);
			Assert.AreEqual ("dmarc", authres.Results[2].Method);
			Assert.AreEqual ("bestguesspass", authres.Results[2].Result);
			Assert.AreEqual (null, authres.Results[2].ResultComment);
			Assert.AreEqual ("none", authres.Results[2].Action);
			Assert.AreEqual (1, authres.Results[2].Properties.Count, "dmarc properties");
			Assert.AreEqual ("header", authres.Results[2].Properties[0].PropertyType);
			Assert.AreEqual ("from", authres.Results[2].Properties[0].Property);
			Assert.AreEqual ("domain.com", authres.Results[2].Properties[0].Value);

			Assert.AreEqual ("spf=fail (sender IP is 1.1.1.1) smtp.mailfrom=eu-west-1.amazonses.com; receivingdomain.com; dkim=pass (signature was verified) header.d=domain.com; domain1.com; dmarc=bestguesspass action=\"none\" header.from=domain.com", authres.ToString ());

			const string expected = "\n\tspf=fail (sender IP is 1.1.1.1) smtp.mailfrom=eu-west-1.amazonses.com;\n\treceivingdomain.com; dkim=pass (signature was verified) header.d=domain.com;\n\tdomain1.com; dmarc=bestguesspass action=\"none\" header.from=domain.com\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.AreEqual (expected, encoded.ToString ());
		}

		// Tests work-around for https://github.com/jstedfast/MimeKit/issues/527
		[Test]
		public void TestParseOffice365RandomDomainTokensAndEmptyPropertyValue ()
		{
			const string input = "spf=temperror (sender IP is 1.1.1.1) smtp.helo=tes.test.ru; mydomain.com; dkim=none (message not signed) header.d=none;mydomain.com; dmarc=none action=none header.from=;";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.IsTrue (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres));
			Assert.AreEqual (null, authres.AuthenticationServiceIdentifier, "authserv-id");
			Assert.AreEqual (3, authres.Results.Count, "methods");
			Assert.AreEqual ("spf", authres.Results[0].Method);
			Assert.AreEqual ("temperror", authres.Results[0].Result);
			Assert.AreEqual ("sender IP is 1.1.1.1", authres.Results[0].ResultComment);
			Assert.AreEqual (1, authres.Results[0].Properties.Count, "spf properties");
			Assert.AreEqual ("smtp", authres.Results[0].Properties[0].PropertyType);
			Assert.AreEqual ("helo", authres.Results[0].Properties[0].Property);
			Assert.AreEqual ("tes.test.ru", authres.Results[0].Properties[0].Value);

			Assert.AreEqual ("mydomain.com", authres.Results[1].Office365AuthenticationServiceIdentifier);
			Assert.AreEqual ("dkim", authres.Results[1].Method);
			Assert.AreEqual ("none", authres.Results[1].Result);
			Assert.AreEqual ("message not signed", authres.Results[1].ResultComment);
			Assert.AreEqual (1, authres.Results[1].Properties.Count, "dkim properties");
			Assert.AreEqual ("header", authres.Results[1].Properties[0].PropertyType);
			Assert.AreEqual ("d", authres.Results[1].Properties[0].Property);
			Assert.AreEqual ("none", authres.Results[1].Properties[0].Value);

			Assert.AreEqual ("mydomain.com", authres.Results[2].Office365AuthenticationServiceIdentifier);
			Assert.AreEqual ("dmarc", authres.Results[2].Method);
			Assert.AreEqual ("none", authres.Results[2].Result);
			Assert.AreEqual (null, authres.Results[2].ResultComment);
			Assert.AreEqual ("none", authres.Results[2].Action);
			Assert.AreEqual (1, authres.Results[2].Properties.Count, "dmarc properties");
			Assert.AreEqual ("header", authres.Results[2].Properties[0].PropertyType);
			Assert.AreEqual ("from", authres.Results[2].Properties[0].Property);
			Assert.AreEqual ("", authres.Results[2].Properties[0].Value);

			Assert.AreEqual ("spf=temperror (sender IP is 1.1.1.1) smtp.helo=tes.test.ru; mydomain.com; dkim=none (message not signed) header.d=none; mydomain.com; dmarc=none action=\"none\" header.from=", authres.ToString ());

			const string expected = "\n\tspf=temperror (sender IP is 1.1.1.1) smtp.helo=tes.test.ru;\n\tmydomain.com; dkim=none (message not signed) header.d=none;\n\tmydomain.com; dmarc=none action=\"none\" header.from=\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.AreEqual (expected, encoded.ToString ());
		}

		// Tests work-around for https://github.com/jstedfast/MimeKit/issues/584
		[Test]
		public void TestParseMethodResultWithUnderscore ()
		{
			const string input = " atlas122.free.mail.gq1.yahoo.com; dkim=dkim_pass header.i=@news.aegeanair.com header.s=@aegeanair2; spf=pass smtp.mailfrom=news.aegeanair.com; dmarc=success(p=REJECT) header.from=news.aegeanair.com;";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.IsTrue (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres));
			Assert.AreEqual ("atlas122.free.mail.gq1.yahoo.com", authres.AuthenticationServiceIdentifier, "authserv-id");
			Assert.AreEqual (3, authres.Results.Count, "methods");
			Assert.AreEqual ("dkim", authres.Results[0].Method);
			Assert.AreEqual ("dkim_pass", authres.Results[0].Result);
			Assert.AreEqual (null, authres.Results[0].ResultComment);
			Assert.AreEqual (2, authres.Results[0].Properties.Count, "dkim properties");
			Assert.AreEqual ("header", authres.Results[0].Properties[0].PropertyType);
			Assert.AreEqual ("i", authres.Results[0].Properties[0].Property);
			Assert.AreEqual ("@news.aegeanair.com", authres.Results[0].Properties[0].Value);
			Assert.AreEqual ("header", authres.Results[0].Properties[1].PropertyType);
			Assert.AreEqual ("s", authres.Results[0].Properties[1].Property);
			Assert.AreEqual ("@aegeanair2", authres.Results[0].Properties[1].Value);

			Assert.AreEqual ("spf", authres.Results[1].Method);
			Assert.AreEqual ("pass", authres.Results[1].Result);
			Assert.AreEqual (null, authres.Results[1].ResultComment);
			Assert.AreEqual (1, authres.Results[1].Properties.Count, "spf properties");
			Assert.AreEqual ("smtp", authres.Results[1].Properties[0].PropertyType);
			Assert.AreEqual ("mailfrom", authres.Results[1].Properties[0].Property);
			Assert.AreEqual ("news.aegeanair.com", authres.Results[1].Properties[0].Value);

			Assert.AreEqual ("dmarc", authres.Results[2].Method);
			Assert.AreEqual ("success", authres.Results[2].Result);
			Assert.AreEqual ("p=REJECT", authres.Results[2].ResultComment);
			Assert.AreEqual (1, authres.Results[2].Properties.Count, "dmarc properties");
			Assert.AreEqual ("header", authres.Results[2].Properties[0].PropertyType);
			Assert.AreEqual ("from", authres.Results[2].Properties[0].Property);
			Assert.AreEqual ("news.aegeanair.com", authres.Results[2].Properties[0].Value);

			Assert.AreEqual ("atlas122.free.mail.gq1.yahoo.com; dkim=dkim_pass header.i=@news.aegeanair.com header.s=@aegeanair2; spf=pass smtp.mailfrom=news.aegeanair.com; dmarc=success (p=REJECT) header.from=news.aegeanair.com", authres.ToString ());

			const string expected = " atlas122.free.mail.gq1.yahoo.com;\n\tdkim=dkim_pass header.i=@news.aegeanair.com header.s=@aegeanair2;\n\tspf=pass smtp.mailfrom=news.aegeanair.com;\n\tdmarc=success (p=REJECT) header.from=news.aegeanair.com\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.AreEqual (expected, encoded.ToString ());
		}

		// Tests work-around for https://github.com/jstedfast/MimeKit/issues/590
		[Test]
		public void TestParsePropertyWithEqualSignInValue ()
		{
			const string input = "i=1; relay.mailrelay.com; dkim=pass header.d=domaina.com header.s=sfdc header.b=abcefg; dmarc=pass (policy=quarantine) header.from=domaina.com; spf=pass (relay.mailrelay.com: domain of support=domaina.com__0-1q6woix34obtbu@823lwd90ky2ahf.mail_sender.com designates 1.1.1.1 as permitted sender) smtp.mailfrom=support=domaina.com__0-1q6woix34obtbu@823lwd90ky2ahf.mail_sender.com";
			var buffer = Encoding.ASCII.GetBytes (input);
			AuthenticationResults authres;

			Assert.IsTrue (AuthenticationResults.TryParse (buffer, 0, buffer.Length, out authres));
			Assert.AreEqual (1, authres.Instance.Value, "i");
			Assert.AreEqual ("relay.mailrelay.com", authres.AuthenticationServiceIdentifier, "authserv-id");
			Assert.AreEqual (3, authres.Results.Count, "methods");
			Assert.AreEqual ("dkim", authres.Results[0].Method);
			Assert.AreEqual ("pass", authres.Results[0].Result);
			Assert.AreEqual (null, authres.Results[0].ResultComment);
			Assert.AreEqual (3, authres.Results[0].Properties.Count, "dkim properties");
			Assert.AreEqual ("header", authres.Results[0].Properties[0].PropertyType);
			Assert.AreEqual ("d", authres.Results[0].Properties[0].Property);
			Assert.AreEqual ("domaina.com", authres.Results[0].Properties[0].Value);
			Assert.AreEqual ("header", authres.Results[0].Properties[1].PropertyType);
			Assert.AreEqual ("s", authres.Results[0].Properties[1].Property);
			Assert.AreEqual ("sfdc", authres.Results[0].Properties[1].Value);
			Assert.AreEqual ("header", authres.Results[0].Properties[2].PropertyType);
			Assert.AreEqual ("b", authres.Results[0].Properties[2].Property);
			Assert.AreEqual ("abcefg", authres.Results[0].Properties[2].Value);

			Assert.AreEqual ("dmarc", authres.Results[1].Method);
			Assert.AreEqual ("pass", authres.Results[1].Result);
			Assert.AreEqual ("policy=quarantine", authres.Results[1].ResultComment);
			Assert.AreEqual (1, authres.Results[1].Properties.Count, "spf properties");
			Assert.AreEqual ("header", authres.Results[1].Properties[0].PropertyType);
			Assert.AreEqual ("from", authres.Results[1].Properties[0].Property);
			Assert.AreEqual ("domaina.com", authres.Results[1].Properties[0].Value);

			Assert.AreEqual ("spf", authres.Results[2].Method);
			Assert.AreEqual ("pass", authres.Results[2].Result);
			Assert.AreEqual ("relay.mailrelay.com: domain of support=domaina.com__0-1q6woix34obtbu@823lwd90ky2ahf.mail_sender.com designates 1.1.1.1 as permitted sender", authres.Results[2].ResultComment);
			Assert.AreEqual (1, authres.Results[2].Properties.Count, "dmarc properties");
			Assert.AreEqual ("smtp", authres.Results[2].Properties[0].PropertyType);
			Assert.AreEqual ("mailfrom", authres.Results[2].Properties[0].Property);
			Assert.AreEqual ("support=domaina.com__0-1q6woix34obtbu@823lwd90ky2ahf.mail_sender.com", authres.Results[2].Properties[0].Value);

			Assert.AreEqual ("i=1; relay.mailrelay.com; dkim=pass header.d=domaina.com header.s=sfdc header.b=abcefg; dmarc=pass (policy=quarantine) header.from=domaina.com; spf=pass (relay.mailrelay.com: domain of support=domaina.com__0-1q6woix34obtbu@823lwd90ky2ahf.mail_sender.com designates 1.1.1.1 as permitted sender) smtp.mailfrom=support=domaina.com__0-1q6woix34obtbu@823lwd90ky2ahf.mail_sender.com", authres.ToString ());

			const string expected = " i=1; relay.mailrelay.com;\n\tdkim=pass header.d=domaina.com header.s=sfdc header.b=abcefg;\n\tdmarc=pass (policy=quarantine) header.from=domaina.com; spf=pass\n\t(relay.mailrelay.com: domain of support=domaina.com__0-1q6woix34obtbu@823lwd90ky2ahf.mail_sender.com designates 1.1.1.1 as permitted sender)\n\tsmtp.mailfrom=\n\tsupport=domaina.com__0-1q6woix34obtbu@823lwd90ky2ahf.mail_sender.com\n";
			var encoded = new StringBuilder ();
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Unix;

			authres.Encode (options, encoded, "Authentication-Results:".Length);

			Assert.AreEqual (expected, encoded.ToString ());
		}

		static void AssertParseFailure (string input, int tokenIndex, int errorIndex)
		{
			var buffer = Encoding.ASCII.GetBytes (input);

			Assert.IsFalse (AuthenticationResults.TryParse (buffer, out AuthenticationResults authres));

			try {
				AuthenticationResults.Parse (buffer);
				Assert.Fail ("Expected parse error.");
			} catch (ParseException ex) {
				Assert.AreEqual (tokenIndex, ex.TokenIndex, "TokenIndex");
				Assert.AreEqual (errorIndex, ex.ErrorIndex, "ErrorIndex");
			}

			try {
				AuthenticationResults.Parse (buffer, 0, buffer.Length);
				Assert.Fail ("Expected parse error.");
			} catch (ParseException ex) {
				Assert.AreEqual (tokenIndex, ex.TokenIndex, "TokenIndex");
				Assert.AreEqual (errorIndex, ex.ErrorIndex, "ErrorIndex");
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

		// Note: TestParseFailureIncompleteProperty4 and 5 are commented out because of
		// https://github.com/jstedfast/MimeKit/issues/527 where we have "header.from=;"

		//[Test]
		//public void TestParseFailureIncompleteProperty4 ()
		//{
		//	AssertParseFailure ("authserv-id; method=pass ptype.prop=", 25, 36);
		//}

		//[Test]
		//public void TestParseFailureIncompleteProperty5 ()
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

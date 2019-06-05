//
// AuthenticationResultsTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2019 Xamarin Inc. (www.xamarin.com)
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
		public void TestParseFailureUnknownLeadingMethod ()
		{
			AssertParseFailure ("x=5; authserv-id", 0, 1);
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
			AssertParseFailure ("authserv-id; abc/1.0=pass", 13, 18);
		}

		[Test]
		public void TestParseFailureInvalidMethodVersion2 ()
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
			AssertParseFailure ("authserv-id; method=pass ptype.prop=;", 25, 36);
		}

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
	}
}

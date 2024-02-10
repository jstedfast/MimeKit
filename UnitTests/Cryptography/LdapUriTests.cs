//
// LdapUriTests.cs
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

#if ENABLE_LDAP
using System.DirectoryServices.Protocols;

using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class LdapUriTests
	{
		[Test]
		public void TestParseEmptyString ()
		{
			LdapUri uri;

			Assert.IsFalse (LdapUri.TryParse ("", out uri));
		}

		[Test]
		public void TestParseLdapColon ()
		{
			LdapUri uri;

			Assert.IsFalse (LdapUri.TryParse ("ldap:", out uri));
		}

		[Test]
		public void TestParseLdapColonSlash ()
		{
			LdapUri uri;

			Assert.IsFalse (LdapUri.TryParse ("ldap:/", out uri));
		}

		[Test]
		public void TestParseLdapColonSlashSlash ()
		{
			LdapUri uri;

			Assert.IsTrue (LdapUri.TryParse ("ldap://", out uri));
			Assert.AreEqual ("ldap", uri.Scheme, "Scheme");
			Assert.AreEqual (string.Empty, uri.Host, "Host");
			Assert.AreEqual (0, uri.Port, "Port");
			Assert.AreEqual (string.Empty, uri.DistinguishedName, "DistinguishedName");
			Assert.AreEqual ("*", uri.Attributes[0], "Attributes");
			Assert.AreEqual (SearchScope.Base, uri.Scope, "Scope");
			Assert.AreEqual ("(objectClass=*)", uri.Filter, "Filter");
			Assert.AreEqual (null, uri.Extensions, "Extensions");
		}

		[Test]
		public void TestParseLdapHostName ()
		{
			LdapUri uri;

			Assert.IsTrue (LdapUri.TryParse ("ldap://ldap.itd.umich.edu", out uri));
			Assert.AreEqual ("ldap", uri.Scheme, "Scheme");
			Assert.AreEqual ("ldap.itd.umich.edu", uri.Host, "Host");
			Assert.AreEqual (0, uri.Port, "Port");
			Assert.AreEqual (string.Empty, uri.DistinguishedName, "DistinguishedName");
			Assert.AreEqual ("*", uri.Attributes[0], "Attributes");
			Assert.AreEqual (SearchScope.Base, uri.Scope, "Scope");
			Assert.AreEqual ("(objectClass=*)", uri.Filter, "Filter");
			Assert.AreEqual (null, uri.Extensions, "Extensions");
		}

		[Test]
		public void TestParseLdapHostNameColon ()
		{
			LdapUri uri;

			Assert.IsFalse (LdapUri.TryParse ("ldap://ldap.itd.umich.edu:", out uri));
		}

		[Test]
		public void TestParseLdapInvalidPort1 ()
		{
			LdapUri uri;

			Assert.IsFalse (LdapUri.TryParse ("ldap://ldap.itd.umich.edu:XYZ", out uri));
		}

		[Test]
		public void TestParseLdapInvalidPort2 ()
		{
			LdapUri uri;

			Assert.IsFalse (LdapUri.TryParse ("ldap://ldap.itd.umich.edu:65537", out uri));
		}

		[Test]
		public void TestParseLdapHostNamePort ()
		{
			LdapUri uri;

			Assert.IsTrue (LdapUri.TryParse ("ldap://ldap.itd.umich.edu:999", out uri));
			Assert.AreEqual ("ldap", uri.Scheme, "Scheme");
			Assert.AreEqual ("ldap.itd.umich.edu", uri.Host, "Host");
			Assert.AreEqual (999, uri.Port, "Port");
			Assert.AreEqual (string.Empty, uri.DistinguishedName, "DistinguishedName");
			Assert.AreEqual ("*", uri.Attributes[0], "Attributes");
			Assert.AreEqual (SearchScope.Base, uri.Scope, "Scope");
			Assert.AreEqual ("(objectClass=*)", uri.Filter, "Filter");
			Assert.AreEqual (null, uri.Extensions, "Extensions");
		}

		[Test]
		public void TestParseLdapHostNamePortSlash ()
		{
			LdapUri uri;

			Assert.IsTrue (LdapUri.TryParse ("ldap://ldap.itd.umich.edu:999/", out uri));
			Assert.AreEqual ("ldap", uri.Scheme, "Scheme");
			Assert.AreEqual ("ldap.itd.umich.edu", uri.Host, "Host");
			Assert.AreEqual (999, uri.Port, "Port");
			Assert.AreEqual (string.Empty, uri.DistinguishedName, "DistinguishedName");
			Assert.AreEqual ("*", uri.Attributes[0], "Attributes");
			Assert.AreEqual (SearchScope.Base, uri.Scope, "Scope");
			Assert.AreEqual ("(objectClass=*)", uri.Filter, "Filter");
			Assert.AreEqual (null, uri.Extensions, "Extensions");
		}

		[Test]
		public void TestParseRfc2255Example1 ()
		{
			LdapUri uri;

			Assert.IsTrue (LdapUri.TryParse ("ldap:///o=University%20of%20Michigan,c=US", out uri));
			Assert.AreEqual ("ldap", uri.Scheme, "Scheme");
			Assert.AreEqual (string.Empty, uri.Host, "Host");
			Assert.AreEqual (0, uri.Port, "Port");
			Assert.AreEqual ("o=University of Michigan,c=US", uri.DistinguishedName, "DistinguishedName");
			Assert.AreEqual ("*", uri.Attributes[0], "Attributes");
			Assert.AreEqual (SearchScope.Base, uri.Scope, "Scope");
			Assert.AreEqual ("(objectClass=*)", uri.Filter, "Filter");
			Assert.AreEqual (null, uri.Extensions, "Extensions");
		}

		[Test]
		public void TestParseRfc2255Example2 ()
		{
			LdapUri uri;

			Assert.IsTrue (LdapUri.TryParse ("ldap://ldap.itd.umich.edu/o=University%20of%20Michigan,c=US", out uri));
			Assert.AreEqual ("ldap", uri.Scheme, "Scheme");
			Assert.AreEqual ("ldap.itd.umich.edu", uri.Host, "Host");
			Assert.AreEqual (0, uri.Port, "Port");
			Assert.AreEqual ("o=University of Michigan,c=US", uri.DistinguishedName, "DistinguishedName");
			Assert.AreEqual ("*", uri.Attributes[0], "Attributes");
			Assert.AreEqual (SearchScope.Base, uri.Scope, "Scope");
			Assert.AreEqual ("(objectClass=*)", uri.Filter, "Filter");
			Assert.AreEqual (null, uri.Extensions, "Extensions");
		}

		[Test]
		public void TestParseRfc2255Example3 ()
		{
			LdapUri uri;

			Assert.IsTrue (LdapUri.TryParse ("ldap://ldap.itd.umich.edu/o=University%20of%20Michigan,c=US?postalAddress", out uri));
			Assert.AreEqual ("ldap", uri.Scheme, "Scheme");
			Assert.AreEqual ("ldap.itd.umich.edu", uri.Host, "Host");
			Assert.AreEqual (0, uri.Port, "Port");
			Assert.AreEqual ("o=University of Michigan,c=US", uri.DistinguishedName, "DistinguishedName");
			Assert.AreEqual ("postalAddress", uri.Attributes[0], "Attributes");
			Assert.AreEqual (SearchScope.Base, uri.Scope, "Scope");
			Assert.AreEqual ("(objectClass=*)", uri.Filter, "Filter");
			Assert.AreEqual (null, uri.Extensions, "Extensions");
		}

		[Test]
		public void TestParseRfc2255Example4 ()
		{
			LdapUri uri;

			Assert.IsTrue (LdapUri.TryParse ("ldap://host.com:6666/o=University%20of%20Michigan,c=US??sub?(cn=Babs%20Jensen)", out uri));
			Assert.AreEqual ("ldap", uri.Scheme, "Scheme");
			Assert.AreEqual ("host.com", uri.Host, "Host");
			Assert.AreEqual (6666, uri.Port, "Port");
			Assert.AreEqual ("o=University of Michigan,c=US", uri.DistinguishedName, "DistinguishedName");
			Assert.AreEqual ("*", uri.Attributes[0], "Attributes");
			Assert.AreEqual (SearchScope.Subtree, uri.Scope, "Scope");
			Assert.AreEqual ("(cn=Babs Jensen)", uri.Filter, "Filter");
			Assert.AreEqual (null, uri.Extensions, "Extensions");
		}

		[Test]
		public void TestParseRfc2255Example5 ()
		{
			LdapUri uri;

			Assert.IsTrue (LdapUri.TryParse ("ldap://ldap.itd.umich.edu/c=GB?objectClass?one", out uri));
			Assert.AreEqual ("ldap", uri.Scheme, "Scheme");
			Assert.AreEqual ("ldap.itd.umich.edu", uri.Host, "Host");
			Assert.AreEqual (0, uri.Port, "Port");
			Assert.AreEqual ("c=GB", uri.DistinguishedName, "DistinguishedName");
			Assert.AreEqual ("objectClass", uri.Attributes[0], "Attributes");
			Assert.AreEqual (SearchScope.OneLevel, uri.Scope, "Scope");
			Assert.AreEqual ("(objectClass=*)", uri.Filter, "Filter");
			Assert.AreEqual (null, uri.Extensions, "Extensions");
		}

		[Test]
		public void TestParseRfc2255Example6 ()
		{
			LdapUri uri;

			Assert.IsTrue (LdapUri.TryParse ("ldap://ldap.question.com/o=Question%3f,c=US?mail", out uri));
			Assert.AreEqual ("ldap", uri.Scheme, "Scheme");
			Assert.AreEqual ("ldap.question.com", uri.Host, "Host");
			Assert.AreEqual (0, uri.Port, "Port");
			Assert.AreEqual ("o=Question?,c=US", uri.DistinguishedName, "DistinguishedName");
			Assert.AreEqual ("mail", uri.Attributes[0], "Attributes");
			Assert.AreEqual (SearchScope.Base, uri.Scope, "Scope");
			Assert.AreEqual ("(objectClass=*)", uri.Filter, "Filter");
			Assert.AreEqual (null, uri.Extensions, "Extensions");
		}

		[Test]
		public void TestParseRfc2255Example7 ()
		{
			LdapUri uri;

			Assert.IsTrue (LdapUri.TryParse ("ldap://ldap.netscape.com/o=Babsco,c=US??(int=%5c00%5c00%5c00%5c04)", out uri));
			Assert.AreEqual ("ldap", uri.Scheme, "Scheme");
			Assert.AreEqual ("ldap.netscape.com", uri.Host, "Host");
			Assert.AreEqual (0, uri.Port, "Port");
			Assert.AreEqual ("o=Babsco,c=US", uri.DistinguishedName, "DistinguishedName");
			Assert.AreEqual ("*", uri.Attributes[0], "Attributes");
			Assert.AreEqual (SearchScope.Base, uri.Scope, "Scope");
			Assert.AreEqual ("(int=\\00\\00\\00\\04)", uri.Filter, "Filter");
			Assert.AreEqual (null, uri.Extensions, "Extensions");
		}

		[Test]
		public void TestParseRfc2255Example8 ()
		{
			LdapUri uri;

			Assert.IsTrue (LdapUri.TryParse ("ldap:///??sub??bindname=cn=Manager%2co=Foo", out uri));
			Assert.AreEqual ("ldap", uri.Scheme, "Scheme");
			Assert.AreEqual (string.Empty, uri.Host, "Host");
			Assert.AreEqual (0, uri.Port, "Port");
			Assert.AreEqual (string.Empty, uri.DistinguishedName, "DistinguishedName");
			Assert.AreEqual ("*", uri.Attributes[0], "Attributes");
			Assert.AreEqual (SearchScope.Subtree, uri.Scope, "Scope");
			Assert.AreEqual ("(objectClass=*)", uri.Filter, "Filter");
			Assert.AreEqual ("bindname=cn=Manager,o=Foo", uri.Extensions[0], "Extensions");
		}

		[Test]
		public void TestParseRfc2255Example9 ()
		{
			LdapUri uri;

			Assert.IsTrue (LdapUri.TryParse ("ldap:///??sub??!bindname=cn=Manager%2co=Foo", out uri));
			Assert.AreEqual ("ldap", uri.Scheme, "Scheme");
			Assert.AreEqual (string.Empty, uri.Host, "Host");
			Assert.AreEqual (0, uri.Port, "Port");
			Assert.AreEqual (string.Empty, uri.DistinguishedName, "DistinguishedName");
			Assert.AreEqual ("*", uri.Attributes[0], "Attributes");
			Assert.AreEqual (SearchScope.Subtree, uri.Scope, "Scope");
			Assert.AreEqual ("(objectClass=*)", uri.Filter, "Filter");
			Assert.AreEqual ("!bindname=cn=Manager,o=Foo", uri.Extensions[0], "Extensions");
		}
	}
}
#endif

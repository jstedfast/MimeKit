//
// LdapUri.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2018 Xamarin Inc. (www.xamarin.com)
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
using System.Linq;
using System.DirectoryServices.Protocols;

namespace MimeKit.Cryptography
{
	class LdapUri
	{
		static readonly char[] EndOfHostPort = { ':', '/' };
		static readonly char[] Comma = { ',' };

		const string DefaultFilter = "(objectClass=*)";

		public string Scheme { get; private set; }
		public string Host { get; private set; }
		public int Port { get; private set; }
		public string DistinguishedName { get; private set; }
		public string[] Attributes { get; private set; }
		public SearchScope Scope { get; private set; }
		public string Filter { get; private set; }
		public string[] Extensions { get; private set; }

		public LdapUri (string scheme)
		{
			Scheme = scheme;
			Host = string.Empty;
			DistinguishedName = string.Empty;
			Attributes = new string[] { "*" };
			Scope = SearchScope.Base;
			Filter = DefaultFilter;
		}

		public static bool TryParse (string location, out LdapUri uri)
		{
			// https://www.ietf.org/rfc/rfc2255.txt
			int startIndex, index;
			string value;

			uri = null;

			// parse the scheme
			if ((index = location.IndexOf (':')) == -1 || index + 2 >= location.Length || location[index + 1] != '/' || location[index + 2] != '/')
				return false;

			uri = new LdapUri (location.Substring (0, index));

			if ((startIndex = index + 3) >= location.Length)
				return true;

			// parse the hostname
			if ((index = location.IndexOfAny (EndOfHostPort, startIndex)) == -1)
				index = location.Length;

			uri.Host = location.Substring (startIndex, index - startIndex);

			if (index < location.Length && location[index] == ':') {
				if ((startIndex = index + 1) >= location.Length)
					return false;

				// parse the port
				if ((index = location.IndexOf ('/', startIndex)) == -1)
					index = location.Length;

				value = location.Substring (startIndex, index - startIndex);
				if (!ushort.TryParse (value, out ushort port))
					return false;

				uri.Port = port;
			}

			if ((startIndex = index + 1) >= location.Length)
				return true;

			// parse the distinguished-name
			if ((index = location.IndexOf ('?')) == -1)
				index = location.Length;

			value = location.Substring (startIndex, index - startIndex);
			uri.DistinguishedName = Uri.UnescapeDataString (value);

			if ((startIndex = index + 1) >= location.Length)
				return true;

			// parse the attributes
			if ((index = location.IndexOf ('?', startIndex)) == -1)
				index = location.Length;

			if (index > startIndex) {
				value = location.Substring (startIndex, index - startIndex);
				uri.Attributes = value.Split (Comma, StringSplitOptions.RemoveEmptyEntries).Select (attr => Uri.UnescapeDataString (attr)).ToArray ();
			}

			if ((startIndex = index + 1) >= location.Length)
				return true;

			// parse the scope
			if ((index = location.IndexOf ('?', startIndex)) == -1)
				index = location.Length;

			value = location.Substring (startIndex, index - startIndex);
			switch (value.ToLowerInvariant ()) {
			case "base": uri.Scope = SearchScope.Base; break;
			case "one": uri.Scope = SearchScope.OneLevel; break;
			case "sub": uri.Scope = SearchScope.Subtree; break;
			default:
				// Note: Assuming that Example #7 in rfc5522 is correctly formed, then
				// we need to backtrack and parse this as the filter instead.
				index = startIndex - 1;
				break;
			}

			if ((startIndex = index + 1) >= location.Length)
				return true;

			// parse the filter
			if ((index = location.IndexOf ('?', startIndex)) == -1)
				index = location.Length;

			if (index > startIndex) {
				value = location.Substring (startIndex, index - startIndex);
				uri.Filter = Uri.UnescapeDataString (value);
			}

			if ((startIndex = index + 1) >= location.Length)
				return true;

			index = location.Length;

			value = location.Substring (startIndex, index - startIndex);
			uri.Extensions = value.Split (Comma, StringSplitOptions.RemoveEmptyEntries).Select (extn => Uri.UnescapeDataString (extn)).ToArray ();

			return true;
		}
	}
}

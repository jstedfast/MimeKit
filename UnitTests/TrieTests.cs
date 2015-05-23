﻿//
// TrieTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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

using MimeKit.Text;

using NUnit.Framework;

namespace UnitTests {
	[TestFixture]
	public class TrieTests
	{
		static readonly string[] TriePatterns = {
			"news://",
			"nntp://",
			"telnet://",
			"file://",
			"ftp://",
			"http://",
			"https://",
			"http://www.",
			"www.",
			"ftp.",
			"mailto:",
			"@"
		};
		static readonly string[] TestCases = {
			"apple developer portal is at http://developer.apple.com",
			"make sure greedy matching works http://www.xamarin.com",
			"or, feel free to email me at jeff@xamarin.com",
			"don't forget to check out www.xamarin.com",
			"I've attached a file (file:///cvs/gmime/gmime/gtrie.c)",
		};

		[Test]
		public void TestTrie ()
		{
			var trie = new Trie (true);
			string pattern;

			for (int i = 0; i < TriePatterns.Length; i++)
				trie.Add (TriePatterns[i]);

			for (int i = 0; i < TestCases.Length; i++) {
				int index = trie.Search (TestCases[i].ToCharArray (), out pattern);
				string substr;

				Assert.IsTrue (index != -1, "Search failed for {0}", TestCases[i]);

				substr = TestCases[i].Substring (index);

				Assert.IsTrue (substr.StartsWith (pattern, StringComparison.OrdinalIgnoreCase), "Search returned wrong index for {0}", TestCases[i]);
			}
		}
	}
}

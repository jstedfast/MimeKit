//
// TrieTests.cs
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

using MimeKit.Text;

namespace UnitTests.Text {
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
		public void TestArgumentExceptions ()
		{
			var text = TestCases[0].ToCharArray ();
			var trie = new Trie ();
			string pattern;

			Assert.Throws<ArgumentNullException> (() => trie.Add (null));
			Assert.Throws<ArgumentException> (() => trie.Add (string.Empty));

			for (int i = 0; i < TriePatterns.Length; i++)
				trie.Add (TriePatterns[i]);

			Assert.Throws<ArgumentNullException> (() => trie.Search (null, out pattern));
			Assert.Throws<ArgumentNullException> (() => trie.Search (null, 0, out pattern));
			Assert.Throws<ArgumentNullException> (() => trie.Search (null, 0, 0, out pattern));

			Assert.Throws<ArgumentOutOfRangeException> (() => trie.Search (text, -1, out pattern));
			Assert.Throws<ArgumentOutOfRangeException> (() => trie.Search (text, -1, text.Length, out pattern));
			Assert.Throws<ArgumentOutOfRangeException> (() => trie.Search (text, 0, -1, out pattern));
		}

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

				Assert.That (index != -1, Is.True, $"Search failed for {TestCases[i]}");

				substr = TestCases[i].Substring (index);

				Assert.That (substr.StartsWith (pattern, StringComparison.OrdinalIgnoreCase), Is.True, $"Search returned wrong index for {TestCases[i]}");
			}
		}
	}
}

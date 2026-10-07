//
// TrieTests.cs
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

using System.Globalization;

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

		static Trie CreateTrie (bool ignoreCase, params string[] patterns)
		{
			var trie = new Trie (ignoreCase);

			foreach (var pattern in patterns)
				trie.Add (pattern);

			return trie;
		}

		static void AssertSearch (Trie trie, string text, int expectedIndex, string expectedPattern)
		{
			int index = trie.Search (text.ToCharArray (), out var pattern);

			Assert.That (index, Is.EqualTo (expectedIndex), $"index for \"{text}\"");
			Assert.That (pattern, Is.EqualTo (expectedPattern), $"pattern for \"{text}\"");
		}

		[Test]
		public void TestMismatchAfterPartialMatch ()
		{
			var trie = CreateTrie (true, TriePatterns);

			AssertSearch (trie, "wwww.example.com", 1, "www.");
			AssertSearch (trie, "sftp.example.com", 1, "ftp.");
			AssertSearch (trie, "hhttp://example.com", 1, "http://");
			AssertSearch (trie, "http://wwww.example.com", 0, "http://");
		}

		[Test]
		public void TestMatchWithinFailedPrefix ()
		{
			var trie = CreateTrie (false, "abcd", "bc");

			AssertSearch (trie, "abcx", 1, "bc");
			AssertSearch (trie, "abcd", 0, "abcd");
			AssertSearch (trie, "xxabx", -1, null);
		}

		[Test]
		public void TestLeftmostLongest ()
		{
			var trie = CreateTrie (false, "b", "abc", "ab", "bcde");

			AssertSearch (trie, "abcde", 0, "abc");
			AssertSearch (trie, "abx", 0, "ab");
			AssertSearch (trie, "xbcdex", 1, "bcde");
			AssertSearch (trie, "xbcdx", 1, "b");
		}

		[Test]
		public void TestCultureInvariantIgnoreCase ()
		{
			var culture = CultureInfo.CurrentCulture;

			try {
				CultureInfo.CurrentCulture = new CultureInfo ("tr-TR");

				var trie = CreateTrie (true, "file://", "mailto:");

				AssertSearch (trie, "FILE://", 0, "file://");
				AssertSearch (trie, "MAILTO:", 0, "mailto:");
				AssertSearch (trie, "f\u0130le://", -1, null);
				AssertSearch (trie, "f\u0131le://", -1, null);
			} finally {
				CultureInfo.CurrentCulture = culture;
			}
		}

		static int NaiveSearch (string text, string[] patterns, bool ignoreCase, out string pattern)
		{
			var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

			pattern = null;

			for (int i = 0; i < text.Length; i++) {
				foreach (var p in patterns) {
					if (i + p.Length <= text.Length && string.Compare (text, i, p, 0, p.Length, comparison) == 0 && (pattern is null || p.Length > pattern.Length))
						pattern = p;
				}

				if (pattern != null)
					return i;
			}

			return -1;
		}

		[Test]
		public void TestRandomizedAgainstNaiveSearch ()
		{
			var random = new Random (42);

			for (int iteration = 0; iteration < 2000; iteration++) {
				var patterns = new string[random.Next (1, 6)];

				for (int i = 0; i < patterns.Length; i++) {
					var chars = new char[random.Next (1, 6)];

					for (int j = 0; j < chars.Length; j++)
						chars[j] = "abcAB"[random.Next (5)];

					patterns[i] = new string (chars);
				}

				patterns = patterns.Distinct (StringComparer.OrdinalIgnoreCase).ToArray ();

				bool ignoreCase = random.Next (2) == 0;
				var trie = CreateTrie (ignoreCase, patterns);
				var text = new char[random.Next (0, 30)];

				for (int j = 0; j < text.Length; j++)
					text[j] = "abcAB"[random.Next (5)];

				var input = new string (text);
				int expected = NaiveSearch (input, patterns, ignoreCase, out var expectedPattern);
				int actual = trie.Search (text, out var actualPattern);

				Assert.That (actual, Is.EqualTo (expected), $"index for \"{input}\" with patterns [{string.Join (", ", patterns)}] (ignoreCase={ignoreCase})");

				if (expected != -1)
					Assert.That (actualPattern, Is.EqualTo (expectedPattern).Using ((IEqualityComparer<string>) (ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)), $"pattern for \"{input}\" with patterns [{string.Join (", ", patterns)}] (ignoreCase={ignoreCase})");
			}
		}
	}
}

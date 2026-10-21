//
// AddressParserBenchmarks.cs
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

using System.Linq;
using System.Text;

using MimeKit;

using BenchmarkDotNet.Attributes;

namespace Benchmarks {
	[MemoryDiagnoser]
	public class AddressParserBenchmarks
	{
		string text;
		byte[] buffer;

		[Params ("Single", "Bare", "Recipients", "Utf8", "Encoded", "Groups", "Broken", "LongAtoms", "Quoted", "Comment",
			"UnicodeAddr", "QuotedLocal", "CFWSDomain", "TrailingDot", "Utf8Cjk", "UnknownCharset", "QuotedUtf8", "QuotedEncoded",
			"LongName", "LongUtf8", "LongEquals", "CommentNames", "CommentTabs", "CommentUtf8", "CommentEncoded",
			"LongComment", "LongCommentUtf8", "CommentUtf8Tabs", "CommentEncodedTabs", "CommentUnknownCharsetTabs",
			"LongCommentUtf8Tabs", "LongCommentEqualsTabs", "EscapedName", "FoldedName", "LongQuotedName", "LongQuotedUtf8", "LongQuotedEquals")]
		public string Input { get; set; }

		[GlobalSetup]
		public void Setup ()
		{
			text = Input switch {
				"Single" => "Joe Sixpack <joe@example.com>",
				"Bare" => "first.last@example.com",
				"Recipients" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"User {i} <u{i}@example.com>")),
				"Utf8" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"Ren\u00e9e M\u00fcller {i} <u{i}@example.com>")),
				"Utf8Cjk" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"\u674e\u5c0f\u9f8d {i} <u{i}@example.com>")),
				"Encoded" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"=?utf-8?q?Ren=C3=A9e?= =?utf-8?q?_M=C3=BCller?= <u{i}@example.com>")),
				"UnknownCharset" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"=?unknown?q?Ren=C3=A9e?= <u{i}@example.com>")),
				"Groups" => string.Join (", ", Enumerable.Range (0, 20).Select (i => $"Group {i}: a{i}@example.com, b{i}@example.com;")),
				"Broken" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"<<<u{i}@example.com>>>")),
				"LongAtoms" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"{new string ('a', 48)}{i}@{new string ('b', 48)}.example.com")),
				"Quoted" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"\"Last, First {i}\" <u{i}@example.com>")),
				"QuotedUtf8" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"\"Ren\u00e9e\\\" M\u00fcller\" <u{i}@example.com>")),
				"QuotedEncoded" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"\"=?utf-8?b?IlxOYW1lIg==?=\" <u{i}@example.com>")),
				"EscapedName" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"\"First\\\" Last\\\\ {i}\" <u{i}@example.com>")),
				"FoldedName" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"\"First\r\n Last {i}\" <u{i}@example.com>")),
				"LongQuotedName" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"\"{new string ('x', 1024)}\" <u{i}@example.com>")),
				"LongQuotedUtf8" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"\"{new string ('x', 1024)}\u00e9\" <u{i}@example.com>")),
				"LongQuotedEquals" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"\"{new string ('x', 1024)}=?\" <u{i}@example.com>")),
				"LongName" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"{new string ('x', 1024)} <u{i}@example.com>")),
				"LongUtf8" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"{new string ('x', 1024)}\u00e9 <u{i}@example.com>")),
				"LongEquals" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"{new string ('x', 1024)}=? <u{i}@example.com>")),
				"Comment" => "(" + new string ('x', 32768) + ") a@example.com",
				"CommentNames" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"u{i}@example.com (User {i})")),
				"CommentTabs" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"u{i}@example.com (\tFirst\t Last {i}\t)")),
				"CommentUtf8" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"u{i}@example.com (Ren\u00e9e M\u00fcller {i})")),
				"CommentEncoded" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"u{i}@example.com (=?utf-8?q?Ren=C3=A9e?= =?utf-8?q?_M=C3=BCller?=)")),
				"CommentUtf8Tabs" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"u{i}@example.com (\tRen\u00e9e\tM\u00fcller {i}\t)")),
				"CommentEncodedTabs" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"u{i}@example.com (=?utf-8?q?Ren=C3=A9e?=\t=?utf-8?q?_M=C3=BCller?=)")),
				"CommentUnknownCharsetTabs" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"u{i}@example.com (\t=?unknown?q?Ren=C3=A9e?=\t)")),
				"LongCommentUtf8Tabs" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"u{i}@example.com ({new string ('x', 1024)}\t\u00e9)")),
				"LongCommentEqualsTabs" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"u{i}@example.com ({new string ('x', 1024)}\t=?)")),
				"LongComment" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"u{i}@example.com ({new string ('x', 1024)})")),
				"LongCommentUtf8" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"u{i}@example.com ({new string ('x', 1024)}\u00e9)")),
				"UnicodeAddr" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"\u00e9{i}@\u4e2d.example")),
				"QuotedLocal" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"\"first last {i}\"@example.com")),
				"CFWSDomain" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"a{i}@example (c) . com")),
				"TrailingDot" => string.Join (", ", Enumerable.Range (0, 100).Select (i => $"a{i}@example.com.")),
				_ => throw new System.InvalidOperationException ("Unknown address parser benchmark input.")
			};
			buffer = Encoding.UTF8.GetBytes (text);
		}

		[Benchmark]
		public InternetAddressList ParseBytes ()
		{
			return InternetAddressList.Parse (buffer);
		}

		[Benchmark]
		public InternetAddressList ParseString ()
		{
			return InternetAddressList.Parse (text);
		}
	}
}

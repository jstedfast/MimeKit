//
// HtmlBenchmarks.cs
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

using System;
using System.IO;
using System.Globalization;
using System.Text;

using BenchmarkDotNet.Attributes;

using MimeKit.Text;

namespace Benchmarks.Text {
	[MemoryDiagnoser]
	public class HtmlFormattingScalingBenchmarks
	{
		[Params (256, 512, 1024)]
		public int Sections { get; set; }

		string html = string.Empty;

		static int Read (HtmlTokenizer tokenizer)
		{
			int count = 0;
			while (tokenizer.ReadNextToken (out _))
				count++;
			return count;
		}
		[GlobalSetup]
		public void Setup ()
		{
			var builder = new StringBuilder ();
			// Fifteen distinct formatting entries stay open; each section uses the sixteenth slot.
			for (int i = 0; i < 15; i++)
				builder.Append ("<i title='").Append (i.ToString (CultureInfo.InvariantCulture)).Append ("'>");
			for (int i = 0; i < Sections; i++)
				builder.Append ("<b>word</b>");
			for (int i = 0; i < 15; i++)
				builder.Append ("</i>");
			html = builder.ToString ();

			var tokenizer = new HtmlTokenizer (new StringReader (html));
			var tracker = tokenizer.TreeConstruction.EnableFormattingTracker ();
			if (Read (tokenizer) != 30 + 3 * Sections || tracker.Exceeded || tracker.PeakCount != 16)
				throw new InvalidOperationException ("Unexpected occupancy, fallback or token count in scaling workload.");
		}

		[Benchmark (Baseline = true)]
		public int RecoveryDisabled () => Read (new HtmlTokenizer (new StringReader (html)));

		[Benchmark]
		public int RecoveryEnabled () => Read (new HtmlTokenizer (new StringReader (html)) { EnableFormattingRecovery = true });
	}

	[MemoryDiagnoser]
	public class HtmlFormattingNewsletterBenchmarks
	{
		string html = string.Empty;

		[GlobalSetup]
		public void Setup ()
		{
			html = File.ReadAllText (Path.Combine (BenchmarkHelper.UnitTestsDir, "TestData", "text", "planet-fitness.html"));
			new HtmlToHtml { EnableFormattingRecovery = true, LimitExceededBehavior = HtmlLimitExceededBehavior.Throw }.Convert (html);
		}

		[Benchmark (Baseline = true)]
		public string RecoveryDisabled () => new HtmlToHtml ().Convert (html);

		[Benchmark]
		public string RecoveryEnabled () => new HtmlToHtml { EnableFormattingRecovery = true }.Convert (html);
	}

	/// <summary>
	/// Benchmarks for the HTML tokenizer and the HTML-to-HTML converter.
	/// </summary>
	/// <remarks>
	/// These are primarily useful for tracking memory allocations: <see cref="HtmlToHtml"/> is commonly used to
	/// sanitize the HTML body of every message that a server processes, so the garbage that it produces per
	/// character of input matters as much as raw throughput.
	/// </remarks>
	[MemoryDiagnoser]
	public class HtmlBenchmarks
	{
		static readonly string HtmlDataDir = Path.Combine (BenchmarkHelper.UnitTestsDir, "TestData", "html");

		[Params ("test.html", "Gimhae_Kim_clan.utf-8.html")]
		public string FileName { get; set; } = string.Empty;

		string html = string.Empty;

		[GlobalSetup]
		public void GlobalSetup ()
		{
			html = File.ReadAllText (Path.Combine (HtmlDataDir, FileName));
		}

		[Benchmark]
		public int HtmlTokenizer_ReadAllTokens ()
		{
			var tokenizer = new HtmlTokenizer (new StringReader (html));
			int count = 0;

			while (tokenizer.ReadNextToken (out _))
				count++;

			return count;
		}

		[Benchmark]
		public void HtmlToHtml_ConvertToTextWriter ()
		{
			new HtmlToHtml ().Convert (new StringReader (html), TextWriter.Null);
		}

		[Benchmark]
		public string HtmlToHtml_ConvertToString ()
		{
			return new HtmlToHtml ().Convert (html);
		}

		[Benchmark]
		public void HtmlToHtml_ConvertWithCallback ()
		{
			var converter = new HtmlToHtml {
				FilterComments = true,
				HtmlTagCallback = (ctx, writer) => {
					if (ctx.TagId == HtmlTagId.Script || ctx.TagId == HtmlTagId.Style) {
						ctx.SuppressInnerContent = true;
						ctx.DeleteEndTag = true;
						return;
					}

					ctx.WriteTag (writer, true);
				}
			};

			converter.Convert (new StringReader (html), TextWriter.Null);
		}
	}
}

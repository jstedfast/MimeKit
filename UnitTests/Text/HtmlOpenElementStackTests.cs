//
// HtmlOpenElementStackTests.cs
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

using MimeKit.Text;

using ElementFlags = MimeKit.Text.HtmlOpenElementStack.ElementFlags;

namespace UnitTests.Text {
	[TestFixture]
	public class HtmlOpenElementStackTests
	{
		[Test]
		public void TestRemoveEntryRebuildsScopeIndexes ()
		{
			var stack = new HtmlOpenElementStack (32);

			stack.Push ("b", HtmlTagId.B, HtmlNamespace.Html, ElementFlags.None);
			stack.Push ("form", HtmlTagId.Form, HtmlNamespace.Html, ElementFlags.Special);
			stack.Push ("svg", HtmlTagId.Unknown, HtmlNamespace.Svg, ElementFlags.None);
			stack.Push ("mi", HtmlTagId.Unknown, HtmlNamespace.MathML, ElementFlags.Scope | ElementFlags.MathMLTextIntegrationPoint);

			Assert.That (stack.TopSpecial, Is.EqualTo (1));
			Assert.That (stack.TopScope, Is.EqualTo (3));
			Assert.That (stack.TopHtml, Is.EqualTo (1));
			Assert.That (stack.TopHtmlOrIntegrationPoint, Is.EqualTo (3));

			stack.RemoveEntry (1);

			Assert.That (stack.Count, Is.EqualTo (3));
			Assert.That (stack.IndexOf (HtmlTagId.Form), Is.EqualTo (-1));
			Assert.That (stack.IndexOf (HtmlNamespace.Svg, HtmlTagId.Unknown, "svg"), Is.EqualTo (1));
			Assert.That (stack.TopSpecial, Is.EqualTo (-1));
			Assert.That (stack.TopScope, Is.EqualTo (2));
			Assert.That (stack.TopHtml, Is.EqualTo (0));
			Assert.That (stack.TopHtmlOrIntegrationPoint, Is.EqualTo (2));

			stack.PopThrough (2);

			Assert.That (stack.TopScope, Is.EqualTo (-1));
			Assert.That (stack.TopHtmlOrIntegrationPoint, Is.EqualTo (0));
		}

		[Test]
		public void TestRemoveEntryFoldsAdjacentRuns ()
		{
			var stack = new HtmlOpenElementStack (32);

			stack.Push ("b", HtmlTagId.B, HtmlNamespace.Html, ElementFlags.None);
			stack.Push ("form", HtmlTagId.Form, HtmlNamespace.Html, ElementFlags.Special);
			stack.Push ("b", HtmlTagId.B, HtmlNamespace.Html, ElementFlags.None);
			stack.Push ("b", HtmlTagId.B, HtmlNamespace.Html, ElementFlags.None);
			stack.RemoveEntry (1);

			Assert.That (stack.Count, Is.EqualTo (1));
			Assert.That (stack[0].Count, Is.EqualTo (3));
			Assert.That (stack.IndexOf (HtmlTagId.B), Is.EqualTo (0));
			Assert.That (stack.TopSpecial, Is.EqualTo (-1));

			for (int i = 0; i < 3; i++)
				stack.PopThrough (0);

			Assert.That (stack.Count, Is.EqualTo (0));
			Assert.That (stack.IndexOf (HtmlTagId.B), Is.EqualTo (-1));
		}

		[Test]
		public void TestReplaceRangeRebuildsIndexes ()
		{
			var stack = new HtmlOpenElementStack (32) { TrackClosedElements = true };

			stack.Push ("b", HtmlTagId.B, HtmlNamespace.Html, ElementFlags.None);
			stack.Push ("b", HtmlTagId.B, HtmlNamespace.Html, ElementFlags.None);
			stack.Push ("div", HtmlTagId.Div, HtmlNamespace.Html, ElementFlags.Special);
			stack.Push ("div", HtmlTagId.Div, HtmlNamespace.Html, ElementFlags.Special);
			stack.Push ("svg", HtmlTagId.Unknown, HtmlNamespace.Svg, ElementFlags.None);
			stack.Push ("mi", HtmlTagId.Unknown, HtmlNamespace.MathML, ElementFlags.Scope | ElementFlags.MathMLTextIntegrationPoint);

			var formatting = stack[0];
			var block = stack[1];
			formatting.Count = block.Count = 1;
			stack.ReplaceRange (0, 2, new[] { formatting, block, formatting, block }, 4);

			Assert.That (stack.Count, Is.EqualTo (6));
			Assert.That (stack.IndexOf (HtmlTagId.B), Is.EqualTo (2));
			Assert.That (stack[2].PrevSameName, Is.EqualTo (0));
			Assert.That (stack.IndexOf (HtmlTagId.Div), Is.EqualTo (3));
			Assert.That (stack[3].PrevSameName, Is.EqualTo (1));
			Assert.That (stack.IndexOf (HtmlNamespace.Svg, HtmlTagId.Unknown, "svg"), Is.EqualTo (4));
			Assert.That (stack.TopSpecial, Is.EqualTo (3));
			Assert.That (stack.TopScope, Is.EqualTo (5));
			Assert.That (stack.TopHtml, Is.EqualTo (3));
			Assert.That (stack.TopHtmlOrIntegrationPoint, Is.EqualTo (5));
			Assert.That (stack.ClosedElements, Is.Empty);

			stack.PopThrough (2);

			Assert.That (stack.Count, Is.EqualTo (2));
			Assert.That (stack.IndexOf (HtmlTagId.B), Is.EqualTo (0));
			Assert.That (stack.TopSpecial, Is.EqualTo (1));
			Assert.That (stack.TopScope, Is.EqualTo (-1));
			Assert.That (stack.ClosedElements.Select (element => element.Name), Is.EqualTo (new[] { "mi", "svg", "div" }));
		}

		[Test]
		public void TestReplaceRangeFoldsAdjacentRuns ()
		{
			var stack = new HtmlOpenElementStack (32);

			stack.Push ("div", HtmlTagId.Div, HtmlNamespace.Html, ElementFlags.Special);
			stack.Push ("b", HtmlTagId.B, HtmlNamespace.Html, ElementFlags.None);
			stack.Push ("div", HtmlTagId.Div, HtmlNamespace.Html, ElementFlags.Special);
			stack.Push ("div", HtmlTagId.Div, HtmlNamespace.Html, ElementFlags.Special);
			stack.Push ("svg", HtmlTagId.Unknown, HtmlNamespace.Svg, ElementFlags.None);

			var formatting = stack[1];
			var block = stack[2];
			stack.ReplaceRange (1, 2, new[] { block, formatting }, 2);

			Assert.That (stack.Count, Is.EqualTo (3));
			Assert.That (stack[0].Count, Is.EqualTo (3));
			Assert.That (stack.TopSpecial, Is.EqualTo (0));
			Assert.That (stack.IndexOf (HtmlTagId.B), Is.EqualTo (1));
			Assert.That (stack.IndexOf (HtmlNamespace.Svg, HtmlTagId.Unknown, "svg"), Is.EqualTo (2));
		}

		[Test]
		public void TestReplaceRangeDepthLimit ()
		{
			var stack = new HtmlOpenElementStack (3);

			stack.Push ("b", HtmlTagId.B, HtmlNamespace.Html, ElementFlags.None);
			stack.Push ("b", HtmlTagId.B, HtmlNamespace.Html, ElementFlags.None);
			stack.Push ("div", HtmlTagId.Div, HtmlNamespace.Html, ElementFlags.Special);
			stack.Push ("div", HtmlTagId.Div, HtmlNamespace.Html, ElementFlags.Special);
			stack.Push ("svg", HtmlTagId.Unknown, HtmlNamespace.Svg, ElementFlags.None);

			var formatting = stack[0];
			var block = stack[1];
			formatting.Count = block.Count = 1;
			stack.ReplaceRange (0, 2, new[] { formatting, block, formatting, block }, 4);

			Assert.That (stack.DepthExceeded, Is.True);
			Assert.That (stack.Count, Is.EqualTo (3));
			Assert.That (stack[2].Count, Is.EqualTo (1));
			Assert.That (stack.IndexOf (HtmlNamespace.Svg, HtmlTagId.Unknown, "svg"), Is.EqualTo (-1));
		}

		[Test]
		public void TestDepthLimitAndFolding ()
		{
			var stack = new HtmlOpenElementStack (1);

			stack.Push ("b", HtmlTagId.B, HtmlNamespace.Html, ElementFlags.None);
			stack.Push ("b", HtmlTagId.B, HtmlNamespace.Html, ElementFlags.None);

			Assert.That (stack.DepthExceeded, Is.False);
			Assert.That (stack.Count, Is.EqualTo (1));
			Assert.That (stack[0].Count, Is.EqualTo (2));

			stack.Push ("i", HtmlTagId.I, HtmlNamespace.Html, ElementFlags.None);

			Assert.That (stack.DepthExceeded, Is.True);
			Assert.That (stack.IndexOf (HtmlTagId.I), Is.EqualTo (-1));
			Assert.That (stack.Count, Is.EqualTo (1));
		}

		[Test]
		public void TestIndexesAcrossChunks ()
		{
			var stack = new HtmlOpenElementStack (512);

			for (int i = 0; i < 300; i++) {
				var id = i % 2 == 0 ? HtmlTagId.B : HtmlTagId.I;

				stack.Push (id.ToHtmlTagName (), id, HtmlNamespace.Html, ElementFlags.None);
			}

			Assert.That (stack.Count, Is.EqualTo (300));
			Assert.That (stack.IndexOf (HtmlTagId.B), Is.EqualTo (298));
			Assert.That (stack.IndexOf (HtmlTagId.I), Is.EqualTo (299));

			stack.PopThrough (63);

			Assert.That (stack.Count, Is.EqualTo (63));
			Assert.That (stack.IndexOf (HtmlTagId.B), Is.EqualTo (62));
			Assert.That (stack.IndexOf (HtmlTagId.I), Is.EqualTo (61));

			stack.PopAbove (-1);

			Assert.That (stack.Count, Is.EqualTo (0));
			Assert.That (stack.TopHtml, Is.EqualTo (-1));
			Assert.That (stack.IndexOf (HtmlTagId.B), Is.EqualTo (-1));
			Assert.That (stack.IndexOf (HtmlTagId.I), Is.EqualTo (-1));
		}
	}
}

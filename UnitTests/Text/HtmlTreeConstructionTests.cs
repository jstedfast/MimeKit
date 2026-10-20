//
// HtmlTreeConstructionTests.cs
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

namespace UnitTests.Text {
	[TestFixture]
	public class HtmlTreeConstructionTests
	{
		[TestCase ("style", HtmlTokenizerState.RawText)]
		[TestCase ("xmp", HtmlTokenizerState.RawText)]
		[TestCase ("iframe", HtmlTokenizerState.RawText)]
		[TestCase ("noembed", HtmlTokenizerState.RawText)]
		[TestCase ("noframes", HtmlTokenizerState.RawText)]
		[TestCase ("title", HtmlTokenizerState.RcData)]
		[TestCase ("textarea", HtmlTokenizerState.RcData)]
		[TestCase ("script", HtmlTokenizerState.ScriptData)]
		[TestCase ("plaintext", HtmlTokenizerState.PlainText)]
		[TestCase ("div", HtmlTokenizerState.Data)]
		public void TestContentState (string name, HtmlTokenizerState expected)
		{
			foreach (bool selfClosing in new[] { false, true }) {
				var tree = new HtmlTreeConstruction (32);
				var result = tree.ProcessTag (new HtmlTagToken (name, Array.Empty<HtmlAttribute> (), selfClosing));

				Assert.That (result.TokenizerState, Is.EqualTo (expected));
				Assert.That (result.Disposition, Is.EqualTo (HtmlTreeConstruction.TagDisposition.Normal));
				Assert.That (result.BrokeOutOfForeignContent, Is.False);
				Assert.That (tree.LastTag, Is.EqualTo (result));
			}
		}

		[Test]
		public void TestNoScriptState ([Values (false, true)] bool scriptingEnabled)
		{
			var tree = new HtmlTreeConstruction (32) { ScriptingEnabled = scriptingEnabled };
			var result = tree.ProcessTag (new HtmlTagToken ("noscript", false));

			Assert.That (result.TokenizerState, Is.EqualTo (scriptingEnabled ? HtmlTokenizerState.RawText : HtmlTokenizerState.Data));
			Assert.That (tree.IsNoScriptOpen, Is.True);

			result = tree.ProcessTag (new HtmlTagToken ("noscript", true));

			Assert.That (result.TokenizerState, Is.EqualTo (HtmlTokenizerState.Data));
			Assert.That (tree.IsNoScriptOpen, Is.False);
		}

		[Test]
		public void TestForeignContentState ()
		{
			var tree = new HtmlTreeConstruction (32);

			tree.ProcessTag (new HtmlTagToken ("svg", false));
			var result = tree.ProcessTag (new HtmlTagToken ("style", false));

			Assert.That (tree.IsInForeignContent, Is.True);
			Assert.That (result.TokenizerState, Is.EqualTo (HtmlTokenizerState.Data));
			Assert.That (result.Disposition, Is.EqualTo (HtmlTreeConstruction.TagDisposition.Normal));

			tree.ProcessTag (new HtmlTagToken ("style", true));
			tree.ProcessTag (new HtmlTagToken ("foreignObject", false));
			result = tree.ProcessTag (new HtmlTagToken ("style", false));

			Assert.That (tree.IsInForeignContent, Is.False);
			Assert.That (result.TokenizerState, Is.EqualTo (HtmlTokenizerState.RawText));
		}

		[Test]
		public void TestFramesetDispositions ()
		{
			var tree = new HtmlTreeConstruction (32);

			tree.ProcessTag (new HtmlTagToken ("frameset", false));
			var result = tree.ProcessTag (new HtmlTagToken ("style", false));

			Assert.That (tree.IsInFrameset, Is.True);
			Assert.That (result.Disposition, Is.EqualTo (HtmlTreeConstruction.TagDisposition.Ignored));
			Assert.That (result.TokenizerState, Is.EqualTo (HtmlTokenizerState.Data));

			result = tree.ProcessTag (new HtmlTagToken ("noframes", false));

			Assert.That (result.Disposition, Is.EqualTo (HtmlTreeConstruction.TagDisposition.Normal));
			Assert.That (result.TokenizerState, Is.EqualTo (HtmlTokenizerState.RawText));
		}

		[Test]
		public void TestMergedAttributes ()
		{
			var tree = new HtmlTreeConstruction (32);

			tree.ProcessTag (new HtmlTagToken ("body", false));
			var tag = new HtmlTagToken ("body", new[] { new HtmlAttribute ("class", "example") }, false);
			var result = tree.ProcessTag (tag);

			Assert.That (result.Disposition, Is.EqualTo (HtmlTreeConstruction.TagDisposition.AttributesMerged));
			Assert.That (result.TokenizerState, Is.EqualTo (HtmlTokenizerState.Data));

			result = tree.ProcessTag (new HtmlTagToken ("body", true));

			Assert.That (result.Disposition, Is.EqualTo (HtmlTreeConstruction.TagDisposition.Normal));
		}

		[Test]
		public void TestIgnoredEndTag ()
		{
			var tree = new HtmlTreeConstruction (32);

			tree.ProcessTag (new HtmlTagToken ("math", false));
			tree.ProcessTag (new HtmlTagToken ("mi", false));
			tree.ProcessTag (new HtmlTagToken ("span", false));
			var result = tree.ProcessTag (new HtmlTagToken ("mi", true));

			Assert.That (result.Disposition, Is.EqualTo (HtmlTreeConstruction.TagDisposition.Ignored));
			Assert.That (result.TokenizerState, Is.EqualTo (HtmlTokenizerState.Data));

			result = tree.ProcessTag (new HtmlTagToken ("span", true));

			Assert.That (result.Disposition, Is.EqualTo (HtmlTreeConstruction.TagDisposition.Normal));
			Assert.That (tree.IsInForeignContent, Is.True);
		}

		[Test]
		public void TestImplicitClosures ()
		{
			var tree = new HtmlTreeConstruction (32) { TrackClosedElements = true };

			tree.ProcessTag (new HtmlTagToken ("svg", false));
			tree.ProcessTag (new HtmlTagToken ("g", false));
			tree.ProcessTag (new HtmlTagToken ("g", false));
			var result = tree.ProcessTag (new HtmlTagToken ("div", false));

			Assert.That (result.BrokeOutOfForeignContent, Is.True);
			Assert.That (tree.IsInForeignContent, Is.False);
			Assert.That (tree.ClosedElements.Count, Is.EqualTo (2));
			Assert.That (tree.ClosedElements[0].Name, Is.EqualTo ("g"));
			Assert.That (tree.ClosedElements[0].Count, Is.EqualTo (2));
			Assert.That (tree.ClosedElements[0].IsForeign, Is.True);
			Assert.That (tree.ClosedElements[1].Name, Is.EqualTo ("svg"));

			result = tree.ProcessTag (new HtmlTagToken ("div", true));

			Assert.That (result.BrokeOutOfForeignContent, Is.False);
			Assert.That (tree.ClosedElements, Is.Empty);
		}

		[Test]
		public void TestFormRemovalPreservesForeignElements ()
		{
			var tree = new HtmlTreeConstruction (32) { TrackClosedElements = true };

			tree.ProcessTag (new HtmlTagToken ("form", false));
			tree.ProcessTag (new HtmlTagToken ("svg", false));
			tree.ProcessTag (new HtmlTagToken ("style", false));
			var result = tree.ProcessTag (new HtmlTagToken ("form", true));

			Assert.That (result.Disposition, Is.EqualTo (HtmlTreeConstruction.TagDisposition.Normal));
			Assert.That (tree.IsInForeignContent, Is.True);
			Assert.That (tree.ClosedElements, Is.Empty);

			result = tree.ProcessTag (new HtmlTagToken ("style", false));

			Assert.That (result.TokenizerState, Is.EqualTo (HtmlTokenizerState.Data));
		}

		[Test]
		public void TestPoppedFormPointerDoesNotReferToReusedSlot ()
		{
			var tree = new HtmlTreeConstruction (32);

			tree.ProcessTag (new HtmlTagToken ("div", false));
			tree.ProcessTag (new HtmlTagToken ("form", false));
			tree.ProcessTag (new HtmlTagToken ("div", true));
			tree.ProcessTag (new HtmlTagToken ("section", false));
			tree.ProcessTag (new HtmlTagToken ("svg", false));
			tree.ProcessTag (new HtmlTagToken ("form", true));

			Assert.That (tree.IsInForeignContent, Is.True);
			Assert.That (tree.ProcessTag (new HtmlTagToken ("style", false)).TokenizerState, Is.EqualTo (HtmlTokenizerState.Data));
		}

		[Test]
		public void TestDepthLimitOverridesContentState ()
		{
			var tree = new HtmlTreeConstruction (1);

			tree.ProcessTag (new HtmlTagToken ("div", false));
			var result = tree.ProcessTag (new HtmlTagToken ("style", false));

			Assert.That (tree.DepthExceeded, Is.True);
			Assert.That (result.TokenizerState, Is.EqualTo (HtmlTokenizerState.PlainText));
		}
	}
}

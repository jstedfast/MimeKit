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
		[TestCase ("noscript", HtmlTokenizerState.Data)]
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
		public void TestPrototypeReconstructsAtMathTextIntegrationPoint ([Values (false, true)] bool explicitlyClosed)
		{
			var tree = new HtmlTreeConstruction (32);
			tree.EnableFormattingTracker ();

			foreach (var name in new[] { "math", "mtext", "p", "i" })
				tree.ProcessTag (new HtmlTagToken (name, false));

			if (explicitlyClosed)
				tree.ProcessTag (new HtmlTagToken ("i", true));

			tree.ProcessTag (new HtmlTagToken ("p", true));

			Assert.That (tree.IsInForeignContent, Is.True);

			tree.ProcessCharacters (" ".AsSpan ());

			Assert.That (tree.IsInForeignContent, Is.EqualTo (explicitlyClosed));
		}

		[Test]
		public void TestPrototypeFormattingCannotEscapeCell ()
		{
			var tree = new HtmlTreeConstruction (32);
			tree.EnableFormattingTracker ();

			foreach (var name in new[] { "math", "mtext", "table", "td", "i" })
				tree.ProcessTag (new HtmlTagToken (name, false));

			tree.ProcessTag (new HtmlTagToken ("table", true));
			tree.ProcessCharacters ("text".AsSpan ());

			Assert.That (tree.IsInForeignContent, Is.True);
		}

		[Test]
		public void TestPrototypePreservesBlockOnFormattingClose ([Values ("p", "div")] string block)
		{
			var tree = new HtmlTreeConstruction (32) { TrackClosedElements = true };

			tree.EnableFormattingTracker ();

			foreach (var name in new[] { "math", "mtext", "font", "font", block })
				tree.ProcessTag (new HtmlTagToken (name, false));

			// Remove only the inner font; neither the surviving block nor the outer font is closed.
			tree.ProcessTag (new HtmlTagToken ("font", true));

			Assert.That (tree.FormattingRecoveryExceeded, Is.False);
			Assert.That (tree.ClosedElements, Is.Empty);

			tree.ProcessTag (new HtmlTagToken (block, true));

			Assert.That (tree.IsInForeignContent, Is.False);

			tree.ProcessTag (new HtmlTagToken ("font", true));
			tree.ProcessCharacters ("text".AsSpan ());

			Assert.That (tree.IsInForeignContent, Is.True);
		}

		[Test]
		public void TestPrototypePreservesTwoBlocks ([Values ("li", "center")] string block)
		{
			var tree = new HtmlTreeConstruction (32) { TrackClosedElements = true };
			tree.EnableFormattingTracker ();

			foreach (var name in new[] { "math", "mtext", "font", block, "p" })
				tree.ProcessTag (new HtmlTagToken (name, false));

			tree.ProcessTag (new HtmlTagToken ("font", true));

			Assert.That (tree.FormattingRecoveryExceeded, Is.False);
			Assert.That (tree.ClosedElements, Is.Empty);

			tree.ProcessTag (new HtmlTagToken (block, true));

			Assert.That (tree.ClosedElements.Select (e => e.Name), Is.EqualTo (new[] { "p" }));

			tree.ProcessCharacters ("more".AsSpan ());

			Assert.That (tree.IsInForeignContent, Is.True);
		}

		[Test]
		public void TestPrototypeReconstructsPoppedInlineDescendant ()
		{
			var tree = new HtmlTreeConstruction (32) { TrackClosedElements = true };

			tree.EnableFormattingTracker ();

			foreach (var name in new[] { "math", "mtext", "a", "p", "font" })
				tree.ProcessTag (new HtmlTagToken (name, false));

			tree.ProcessTag (new HtmlTagToken ("a", true));

			Assert.That (tree.FormattingRecoveryExceeded, Is.False);
			Assert.That (tree.ClosedElements.Select (e => e.Name), Is.EqualTo (new[] { "font" }));

			tree.ProcessTag (new HtmlTagToken ("p", true));

			Assert.That (tree.IsInForeignContent, Is.True);

			tree.ProcessCharacters ("text".AsSpan ());

			Assert.That (tree.IsInForeignContent, Is.False);

			tree.ProcessTag (new HtmlTagToken ("font", true));

			Assert.That (tree.IsInForeignContent, Is.True);
		}

		[Test]
		public void TestPrototypeNestedAnchorRestoresFormattingBeforeOpeningReplacement ()
		{
			var tree = new HtmlTreeConstruction (32) { TrackClosedElements = true };

			tree.EnableFormattingTracker ();

			foreach (var name in new[] { "math", "mtext", "a", "p", "font", "a" })
				tree.ProcessTag (new HtmlTagToken (name, false));

			Assert.That (tree.FormattingRecoveryExceeded, Is.False);
			Assert.That (tree.ClosedElements.Select (e => e.Name), Is.EqualTo (new[] { "font" }));

			// Closing the paragraph must pop the replacement anchor above the reconstructed font.
			tree.ProcessTag (new HtmlTagToken ("p", true));

			Assert.That (tree.ClosedElements.Select (e => e.Name), Is.EqualTo (new[] { "a", "font" }));
			Assert.That (tree.IsInForeignContent, Is.True);
		}

		[Test]
		public void TestPrototypeFoldedSpansCloseWithoutClosingParagraph ()
		{
			var tree = new HtmlTreeConstruction (32) { TrackClosedElements = true };

			tree.EnableFormattingTracker ();

			foreach (var name in new[] { "math", "mtext", "font", "p", "span", "span", "span" })
				tree.ProcessTag (new HtmlTagToken (name, false));

			tree.ProcessTag (new HtmlTagToken ("font", true));

			Assert.That (tree.FormattingRecoveryExceeded, Is.False);
			Assert.That (tree.ClosedElements.Select (e => e.Name), Is.EqualTo (new[] { "span" }));
			Assert.That (tree.ClosedElements[0].Count, Is.EqualTo (3));
			Assert.That (tree.IsInForeignContent, Is.False);

			tree.ProcessTag (new HtmlTagToken ("p", true));
			tree.ProcessCharacters ("text".AsSpan ());

			Assert.That (tree.IsInForeignContent, Is.True);
		}

		[Test]
		public void TestAdoptionRetainsFoldedFormattingAcrossBlocks ()
		{
			var tree = new HtmlTreeConstruction (32) { TrackClosedElements = true };

			tree.EnableFormattingTracker ();

			foreach (var name in new[] { "math", "mtext", "i", "a", "i", "i", "div", "div", "h2", "svg", "g" })
				tree.ProcessTag (new HtmlTagToken (name, false));

			tree.ProcessTag (new HtmlTagToken ("a", true));

			Assert.That (tree.FormattingRecoveryExceeded, Is.False);
			Assert.That (tree.ClosedElements.Select (e => e.Name), Is.EqualTo (new[] { "g", "svg" }));

			tree.ProcessTag (new HtmlTagToken ("h2", true));
			tree.ProcessTag (new HtmlTagToken ("div", true));
			tree.ProcessTag (new HtmlTagToken ("div", true));

			// Removing the anchor merged its italic descendants with the preceding italic.
			for (int i = 0; i < 3; i++) {
				Assert.That (tree.IsInForeignContent, Is.False);

				tree.ProcessTag (new HtmlTagToken ("i", true));
			}

			Assert.That (tree.FormattingRecoveryExceeded, Is.False);

			tree.ProcessCharacters ("text".AsSpan ());

			Assert.That (tree.IsInForeignContent, Is.True);
		}

		[TestCase (7, false)]
		[TestCase (8, true)]
		public void TestNestedAnchorCountsPhysicalBlocks (int count, bool fallback)
		{
			var tree = new HtmlTreeConstruction (32);

			tree.EnableFormattingTracker ();
			tree.ProcessTag (new HtmlTagToken ("a", false));

			for (int i = 0; i < count; i++)
				tree.ProcessTag (new HtmlTagToken ("div", false));

			tree.ProcessTag (new HtmlTagToken ("a", false));

			Assert.That (tree.FormattingRecoveryExceeded, Is.EqualTo (fallback));
		}

		[Test]
		public void TestPrototypeDoesNotDiscardAdditionalBlockDescendants ()
		{
			var tree = new HtmlTreeConstruction (32);
			var tracker = tree.EnableFormattingTracker ();

			foreach (var name in new[] { "font", "p", "span", "small" })
				tree.ProcessTag (new HtmlTagToken (name, false));

			tree.ProcessTag (new HtmlTagToken ("font", true));

			Assert.That (tracker.Reason, Is.EqualTo (HtmlFormattingTracker.StopReason.ComplexAdoption));
		}

		static void ReadToEnd (HtmlTokenizer tokenizer)
		{
			while (tokenizer.ReadNextToken (out _)) {
				// Reading advances the formatting tracker; these tests inspect its final state, not the tokens.
			}
		}

		[Test]
		public void TestPrototypeComparesParsedFormattingAttributes ()
		{
			const string equal = "<font COLOR=red size=2><font size='2' color='red'><font color=r&#101;d size=2><font size=2 color=red COLOR=blue>";

			foreach (var extra in new[] { "", "<font color=RED size=2>", "<font color=red size=2 title>" }) {
				var tokenizer = new HtmlTokenizer (new StringReader (equal + extra));
				var tracker = tokenizer.TreeConstruction.EnableFormattingTracker (3);

				ReadToEnd (tokenizer);

				Assert.That (tracker.Reason, Is.EqualTo (extra.Length == 0 ? HtmlFormattingTracker.StopReason.None : HtmlFormattingTracker.StopReason.Capacity));
			}
		}

		[Test]
		public void TestPrototypeDistinguishesCapacityFromUnsupportedRecovery ()
		{
			foreach (int capacity in new[] { 2, 8 }) {
				var tokenizer = new HtmlTokenizer (new StringReader (string.Concat (Enumerable.Repeat ("<i a b c d e>", 4))));
				var tracker = tokenizer.TreeConstruction.EnableFormattingTracker (capacity);

				ReadToEnd (tokenizer);

				Assert.That (tracker.PeakCount, Is.EqualTo (capacity == 2 ? 2 : 3));
				Assert.That (tracker.Reason, Is.EqualTo (capacity == 2 ? HtmlFormattingTracker.StopReason.Capacity : HtmlFormattingTracker.StopReason.AttributeEquivalence));
			}
		}

		[Test]
		public void TestPrototypeBudgetHonorsPolicy ([Values (HtmlLimitExceededBehavior.Throw, HtmlLimitExceededBehavior.LiteralText)] HtmlLimitExceededBehavior policy)
		{
			var tokenizer = new HtmlTokenizer (new StringReader (string.Concat (Enumerable.Repeat ("<i a b c d e>", 4)) + "<p>tail")) {
				EnableFormattingRecovery = true,
				LimitExceededBehavior = policy,
			};
			var logger = new TestHtmlComplianceLogger ();

			tokenizer.ComplianceLogger = logger;

			for (int i = 0; i < 3; i++)
				Assert.That (tokenizer.ReadNextToken (out _), Is.True);

			if (policy == HtmlLimitExceededBehavior.Throw) {
				Assert.Throws<FormatException> (() => tokenizer.ReadNextToken (out _));
			} else {
				Assert.That (tokenizer.ReadNextToken (out _), Is.True);
				Assert.That (tokenizer.ReadNextToken (out var tail), Is.True);
				Assert.That (((HtmlDataToken) tail).Data, Is.EqualTo ("<p>tail"));
			}

			Assert.That (logger.Issues.Select (x => x.Violation), Is.EqualTo (new[] { HtmlComplianceViolation.FormattingRecoveryLimitExceeded }));
			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
		}

		[Test]
		public void TestPrototypeReconstructionDepthLimit ([Values ("x", "&#32;")] string run,
			[Values (HtmlLimitExceededBehavior.Throw, HtmlLimitExceededBehavior.LiteralText)] HtmlLimitExceededBehavior behavior)
		{
			var tokenizer = new HtmlTokenizer (new StringReader ("<p><i></p><div><section>" + run + "<br>")) {
				MaxElementDepth = 2,
				LimitExceededBehavior = behavior,
				DecodeCharacterReferences = true,
				EnableFormattingRecovery = true
			};
			var logger = new TestHtmlComplianceLogger ();

			tokenizer.ComplianceLogger = logger;

			for (int i = 0; i < 5; i++)
				Assert.That (tokenizer.ReadNextToken (out _), Is.True);

			if (behavior == HtmlLimitExceededBehavior.Throw) {
				Assert.Throws<FormatException> (() => tokenizer.ReadNextToken (out _));
			} else {
				var tail = new System.Text.StringBuilder ();

				while (tokenizer.ReadNextToken (out var token))
					tail.Append (((HtmlDataToken) token).Data);

				Assert.That (tail.ToString (), Is.EqualTo ((run == "x" ? "x" : " ") + "<br>"));
			}

			Assert.That (logger.Issues.Count (x => x.Violation == HtmlComplianceViolation.ElementDepthLimitExceeded), Is.EqualTo (1));
			Assert.That (tokenizer.ReadNextToken (out _), Is.False);
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

//
// MimeBoundaryIndexTests.cs
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

using System.Text;

using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class MimeBoundaryIndexTests
	{
		static void AssertLookup (MimeBoundaryIndex index, Stack<MimeBoundary> boundaries, string text)
		{
			var buffer = Encoding.UTF8.GetBytes ("prefix" + text + "suffix");
			int length = buffer.Length - 12;
			MimeBoundary expected = null;
			bool expectedFinal = false;

			foreach (var boundary in boundaries) {
				if ((length == boundary.Length || length == boundary.FinalLength) &&
					buffer.AsSpan (6, length).SequenceEqual (boundary.Marker.AsSpan (0, length))) {
					expected = boundary;
					expectedFinal = length == boundary.FinalLength;
					break;
				}
			}

			bool found = index.TryGetBoundary (buffer, 6, length, out var actual, out bool final);

			Assert.That (found, Is.EqualTo (expected != null), text);
			Assert.That (actual, Is.SameAs (expected), text);
			Assert.That (final, Is.EqualTo (expectedFinal), text);
		}

		[Test]
		public void TestLookupMatchesBoundaryStack ()
		{
			var names = new[] { "b", "b--", "b----", "", "-", "--", "\u00e9", "b b", "b\t", new string ('x', 100), "other" };
			var boundaries = new Stack<MimeBoundary> ();
			var index = new MimeBoundaryIndex ();
			var random = new Random (27531);

			for (int i = 0; i < 2000; i++) {
				if (boundaries.Count == 0 || (boundaries.Count < 128 && random.Next (2) == 0)) {
					var boundary = new MimeBoundary (names[random.Next (names.Length)], boundaries.Count > 0 ? boundaries.Peek () : null);

					boundaries.Push (boundary);
					index.Add (boundary);
				} else {
					index.Remove (boundaries.Pop ());
				}

				foreach (string name in names) {
					AssertLookup (index, boundaries, "--" + name);
					AssertLookup (index, boundaries, "--" + name + "--");
				}

				AssertLookup (index, boundaries, "--unknown");
			}

			while (boundaries.Count > 0)
				index.Remove (boundaries.Pop ());

			AssertLookup (index, boundaries, "--b");
			AssertLookup (index, boundaries, "--b--");
		}

		[Test]
		public void TestClear ()
		{
			var index = new MimeBoundaryIndex ();
			var outer = new MimeBoundary ("b", null);
			var inner = new MimeBoundary ("b", outer);

			index.Add (outer);
			index.Add (inner);
			index.Clear ();
			AssertLookup (index, new Stack<MimeBoundary> (), "--b");

			index.Add (outer);
			index.Remove (outer);
			AssertLookup (index, new Stack<MimeBoundary> (), "--b--");
		}

		[Test]
		public void TestLookupDoesNotAllocate ()
		{
			var index = new MimeBoundaryIndex ();
			var boundaries = new Stack<MimeBoundary> ();
			string prefix = new string ('b', 60);
			var missing = Encoding.ASCII.GetBytes ("--" + prefix + "999999");
			var closing = Encoding.ASCII.GetBytes ("--" + prefix + "000064--");

			for (int i = 0; i < 128; i++) {
				var boundary = new MimeBoundary (prefix + i.ToString ("D6"), boundaries.Count > 0 ? boundaries.Peek () : null);

				boundaries.Push (boundary);
				index.Add (boundary);
			}

			for (int i = 0; i < 100; i++) {
				index.TryGetBoundary (missing, 0, missing.Length, out _, out _);
				index.TryGetBoundary (closing, 0, closing.Length, out _, out _);
			}

			long before = GC.GetAllocatedBytesForCurrentThread ();
			int matches = 0;
			for (int i = 0; i < 1000; i++) {
				if (index.TryGetBoundary (missing, 0, missing.Length, out _, out _))
					matches++;
				if (index.TryGetBoundary (closing, 0, closing.Length, out _, out bool final) && final)
					matches++;
			}
			long allocated = GC.GetAllocatedBytesForCurrentThread () - before;

			Assert.That (matches, Is.EqualTo (1000));
			if (!TestHelper.IsCodeCoverageEnabled)
				Assert.That (allocated, Is.Zero);
		}
	}
}

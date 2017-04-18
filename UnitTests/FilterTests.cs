//
// FilterTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2017 Xamarin Inc. (www.xamarin.com)
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
using System.Text;

using NUnit.Framework;

using MimeKit;
using MimeKit.IO;
using MimeKit.IO.Filters;

namespace UnitTests
{
	[TestFixture]
	public class FilterTests
	{
		static void TestArgumentExceptions (IMimeFilter filter)
		{
			int outputIndex, outputLength;
			var buffer = new byte[10];

			Assert.Throws<ArgumentNullException> (() => filter.Filter (null, 0, 0, out outputIndex, out outputLength));
			Assert.Throws<ArgumentOutOfRangeException> (() => filter.Filter (buffer, -1, 0, out outputIndex, out outputLength));
			Assert.Throws<ArgumentOutOfRangeException> (() => filter.Filter (buffer, 0, 20, out outputIndex, out outputLength));

			Assert.Throws<ArgumentNullException> (() => filter.Flush (null, 0, 0, out outputIndex, out outputLength));
			Assert.Throws<ArgumentOutOfRangeException> (() => filter.Flush (buffer, -1, 0, out outputIndex, out outputLength));
			Assert.Throws<ArgumentOutOfRangeException> (() => filter.Flush (buffer, 0, 20, out outputIndex, out outputLength));
		}

		[Test]
		public void TestArmoredFromFilter ()
		{
			const string text = "This text is meant to test that the filter will armor lines beginning with\nFrom (like mbox).\n";
			const string expected = "This text is meant to test that the filter will armor lines beginning with\n=46rom (like mbox).\n";
			var filter = new ArmoredFromFilter ();

			TestArgumentExceptions (filter);

			using (var stream = new MemoryStream ()) {
				using (var filtered = new FilteredStream (stream)) {
					int fromIndex = text.IndexOf ("\nFrom ", StringComparison.Ordinal);
					var buffer = Encoding.UTF8.GetBytes (text);

					filtered.Add (filter);

					// write out a buffer where the end boundary falls in the middle of "From "
					int endIndex = fromIndex + 3;
					filtered.Write (buffer, 0, endIndex);

					// write out the rest
					filtered.Write (buffer, endIndex, buffer.Length - endIndex);
					filtered.Flush ();

					var actual = Encoding.UTF8.GetString (stream.GetBuffer (), 0, (int) stream.Length);

					Assert.AreEqual (expected, actual, "From armoring failed when end boundary falls in the middle of From.");
				}
			}
		}

		[Test]
		public void TestBestEncodingFilter ()
		{
			const string fromLines = "This text is meant to test that the filter will armor lines beginning with\nFrom (like mbox). And let's have another\nFrom for good measure, shall we?\n";
			const string ascii = "This is some ascii text to make sure that\nthe filter returns 7bit encoding...\n";
			const string french = "Wikipédia est un projet d’encyclopédie collective en ligne, universelle, multilingue et fonctionnant sur le principe du wiki. Wikipédia a pour objectif d’offrir un contenu librement réutilisable, objectif et vérifiable, que chacun peut modifier et améliorer.\n\nTous les rédacteurs des articles de Wikipédia sont bénévoles. Ils coordonnent leurs efforts au sein d'une communauté collaborative, sans dirigeant.";
			var filter = new BestEncodingFilter ();

			TestArgumentExceptions (filter);

			Assert.Throws<ArgumentOutOfRangeException> (() => filter.GetBestEncoding (EncodingConstraint.SevenBit, 10));

			// Test ASCII text
			using (var stream = new MemoryStream ()) {
				using (var filtered = new FilteredStream (stream)) {
					var buffer = Encoding.UTF8.GetBytes (ascii);
					ContentEncoding encoding;

					filtered.Add (filter);

					filtered.Write (buffer, 0, buffer.Length);
					filtered.Flush ();

					encoding = filter.GetBestEncoding (EncodingConstraint.SevenBit);
					Assert.AreEqual (ContentEncoding.SevenBit, encoding, "ASCII 7bit constraint.");

					encoding = filter.GetBestEncoding (EncodingConstraint.EightBit);
					Assert.AreEqual (ContentEncoding.SevenBit, encoding, "ASCII 8bit constraint.");

					encoding = filter.GetBestEncoding (EncodingConstraint.None);
					Assert.AreEqual (ContentEncoding.SevenBit, encoding, "ASCII no constraint.");
				}
			}

			filter.Reset ();

			// Test ASCII text with a line beginning with "From "
			using (var stream = new MemoryStream ()) {
				using (var filtered = new FilteredStream (stream)) {
					int fromIndex = fromLines.IndexOf ("\nFrom ", StringComparison.Ordinal);
					var buffer = Encoding.UTF8.GetBytes (fromLines);
					ContentEncoding encoding;

					filtered.Add (filter);

					// write out a buffer where the end boundary falls in the middle of "From "
					int endIndex = fromIndex + 3;
					filtered.Write (buffer, 0, endIndex);

					// write out the rest
					filtered.Write (buffer, endIndex, buffer.Length - endIndex);
					filtered.Flush ();

					encoding = filter.GetBestEncoding (EncodingConstraint.SevenBit);
					Assert.AreEqual (ContentEncoding.QuotedPrintable, encoding, "From-line 7bit constraint.");

					encoding = filter.GetBestEncoding (EncodingConstraint.EightBit);
					Assert.AreEqual (ContentEncoding.QuotedPrintable, encoding, "From-line 8bit constraint.");

					encoding = filter.GetBestEncoding (EncodingConstraint.None);
					Assert.AreEqual (ContentEncoding.QuotedPrintable, encoding, "From-line no constraint.");
				}
			}

			filter.Reset ();

			// Test some French Latin1 text
			using (var stream = new MemoryStream ()) {
				using (var filtered = new FilteredStream (stream)) {
					var buffer = Encoding.UTF8.GetBytes (french);
					ContentEncoding encoding;

					filtered.Add (filter);

					// We'll write only 60 chars at first to not exceed the 78 char max
					filtered.Write (buffer, 0, 60);
					filtered.Flush ();

					encoding = filter.GetBestEncoding (EncodingConstraint.SevenBit);
					Assert.AreEqual (ContentEncoding.QuotedPrintable, encoding, "French 7bit constraint.");

					encoding = filter.GetBestEncoding (EncodingConstraint.EightBit);
					Assert.AreEqual (ContentEncoding.EightBit, encoding, "French 8bit constraint.");

					encoding = filter.GetBestEncoding (EncodingConstraint.None);
					Assert.AreEqual (ContentEncoding.EightBit, encoding, "French no constraint.");

					filter.Reset ();

					// write the entire French text this time (longest line exceeds 78 chars)
					filtered.Write (buffer, 0, buffer.Length);
					filtered.Flush ();

					encoding = filter.GetBestEncoding (EncodingConstraint.SevenBit);
					Assert.AreEqual (ContentEncoding.QuotedPrintable, encoding, "French (long lines) 7bit constraint.");

					encoding = filter.GetBestEncoding (EncodingConstraint.EightBit);
					Assert.AreEqual (ContentEncoding.QuotedPrintable, encoding, "French (long lines) 8bit constraint.");

					encoding = filter.GetBestEncoding (EncodingConstraint.None);
					Assert.AreEqual (ContentEncoding.QuotedPrintable, encoding, "French (long lines) no constraint.");
				}
			}

		}
	}
}

//
// FilterTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 .NET Foundation and Contributors
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
using MimeKit.Cryptography;
using MimeKit.Utils;

namespace UnitTests.IO.Filters {
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
			const string text = "This text is meant to test that the filter will armor lines beginning with\nFrom (like mbox). And let's add another\nFrom line for good measure, shall we?\n";
			const string expected = "This text is meant to test that the filter will armor lines beginning with\n=46rom (like mbox). And let's add another\n=46rom line for good measure, shall we?\n";
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
			const string fromLines = "This text is meant to test that the filter will armor lines beginning with\nFrom (like mbox).\n";
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

					Assert.IsFalse (filtered.CanTimeout, "CanTimeout");
					Assert.Throws<InvalidOperationException> (() => { var x = filtered.ReadTimeout; });
					Assert.Throws<InvalidOperationException> (() => { var x = filtered.WriteTimeout; });
					Assert.Throws<InvalidOperationException> (() => filtered.ReadTimeout = 50);
					Assert.Throws<InvalidOperationException> (() => filtered.WriteTimeout = 50);
					Assert.Throws<NotSupportedException> (() => { long x = filtered.Length; });
					Assert.Throws<NotSupportedException> (() => filtered.SetLength (100));
					Assert.Throws<NotSupportedException> (() => { long x = filtered.Position; });
					Assert.Throws<NotSupportedException> (() => filtered.Position = 0);

					Assert.Throws<ArgumentNullException> (() => filtered.Add (null));
					Assert.Throws<ArgumentNullException> (() => filtered.Contains (null));
					Assert.Throws<ArgumentNullException> (() => filtered.Remove (null));

					filtered.Add (filter);

					Assert.IsTrue (filtered.Contains (filter), "Contains");

					filtered.Write (buffer, 0, buffer.Length);
					filtered.Flush ();

					encoding = filter.GetBestEncoding (EncodingConstraint.SevenBit);
					Assert.AreEqual (ContentEncoding.SevenBit, encoding, "ASCII 7bit constraint.");

					encoding = filter.GetBestEncoding (EncodingConstraint.EightBit);
					Assert.AreEqual (ContentEncoding.SevenBit, encoding, "ASCII 8bit constraint.");

					encoding = filter.GetBestEncoding (EncodingConstraint.None);
					Assert.AreEqual (ContentEncoding.SevenBit, encoding, "ASCII no constraint.");

					Assert.IsTrue (filtered.Remove (filter), "Remove");
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

			filter.Reset ();

			// Test 78 character line length with CRLF
			using (var stream = new MemoryStream ()) {
				using (var filtered = new FilteredStream (stream)) {
					var buffer = Encoding.ASCII.GetBytes ("abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz\r\nabc\r\n");
					ContentEncoding encoding;

					filtered.Add (filter);

					filtered.Write (buffer, 0, buffer.Length);
					filtered.Flush ();

					encoding = filter.GetBestEncoding (EncodingConstraint.SevenBit, 78);
					Assert.AreEqual (ContentEncoding.SevenBit, encoding, "78-character line; 7bit constraint.");

					encoding = filter.GetBestEncoding (EncodingConstraint.EightBit, 78);
					Assert.AreEqual (ContentEncoding.SevenBit, encoding, "78-character line; 8bit constraint.");

					encoding = filter.GetBestEncoding (EncodingConstraint.None, 78);
					Assert.AreEqual (ContentEncoding.SevenBit, encoding, "78-character line; no constraint.");
				}
			}
		}

		[Test]
		public void TestPassThroughFilter ()
		{
			var filter = new PassThroughFilter ();
			int outputIndex, outputLength;
			var buffer = new byte[10];

			Assert.AreEqual (buffer, filter.Filter (buffer, 1, buffer.Length - 2, out outputIndex, out outputLength));
			Assert.AreEqual (1, outputIndex, "outputIndex");
			Assert.AreEqual (buffer.Length - 2, outputLength, "outputLength");

			Assert.AreEqual (buffer, filter.Flush (buffer, 1, buffer.Length - 2, out outputIndex, out outputLength));
			Assert.AreEqual (1, outputIndex, "outputIndex");
			Assert.AreEqual (buffer.Length - 2, outputLength, "outputLength");

			filter.Reset ();
		}

		[Test]
		public void TestCharsetFilter ()
		{
			const string french = "Wikipédia est un projet d’encyclopédie collective en ligne, universelle, multilingue et fonctionnant sur le principe du wiki. Wikipédia a pour objectif d’offrir un contenu librement réutilisable, objectif et vérifiable, que chacun peut modifier et améliorer.\n\nTous les rédacteurs des articles de Wikipédia sont bénévoles. Ils coordonnent leurs efforts au sein d'une communauté collaborative, sans dirigeant.";
			CharsetFilter filter;

			Assert.Throws<ArgumentNullException> (() => new CharsetFilter (null, "iso-8859-1"));
			Assert.Throws<ArgumentNullException> (() => new CharsetFilter ("iso-8859-1", null));
			Assert.Throws<NotSupportedException> (() => new CharsetFilter ("bogus charset", "iso-8859-1"));
			Assert.Throws<NotSupportedException> (() => new CharsetFilter ("iso-8859-1", "bogus charset"));

			Assert.Throws<ArgumentNullException> (() => new CharsetFilter (null, Encoding.UTF8));
			Assert.Throws<ArgumentNullException> (() => new CharsetFilter (Encoding.UTF8, null));

			Assert.Throws<ArgumentOutOfRangeException> (() => new CharsetFilter (-1, 28591));
			Assert.Throws<ArgumentOutOfRangeException> (() => new CharsetFilter (28591, -1));

			filter = new CharsetFilter (Encoding.UTF8, CharsetUtils.Latin1);

			TestArgumentExceptions (filter);
			filter.Reset ();

			// Try converting, no fallback
			using (var stream = new MemoryStream (Encoding.UTF8.GetBytes (french))) {
				using (var filtered = new FilteredStream (stream)) {
					var expected = Encoding.GetEncoding ("iso-8859-15").GetBytes (french);
					var buffer = new byte[1024];
					int length;

					filtered.Add (new CharsetFilter ("utf-8", "iso-8859-15"));

					length = filtered.Read (buffer, 0, expected.Length / 2);
					length += filtered.Read (buffer, expected.Length / 2, buffer.Length - (expected.Length / 2));

					// Note: this Flush() should do nothing but test a code-path
					filtered.Flush ();

					Assert.AreEqual (expected.Length, length, "iso-8859-15 length");
				}
			}

			// Try converting with fallback (at least 1 char does not fit within iso-8859-1)
			using (var stream = new MemoryStream (Encoding.UTF8.GetBytes (french))) {
				using (var filtered = new FilteredStream (stream)) {
					var utf8 = Encoding.GetEncoding ("utf-8", new EncoderReplacementFallback ("?"), new DecoderReplacementFallback ("?"));
					var latin1 = Encoding.GetEncoding ("iso-8859-1", new EncoderReplacementFallback ("?"), new DecoderReplacementFallback ("?"));
					var expected = latin1.GetBytes (french);
					var buffer = new byte[1024];
					int length;

					filtered.Add (new CharsetFilter (utf8, latin1));

					length = filtered.Read (buffer, 0, expected.Length / 2);
					length += filtered.Read (buffer, expected.Length / 2, buffer.Length - (expected.Length / 2));

					// Note: this Flush() should do nothing but test a code-path
					filtered.Flush ();

					Assert.AreEqual (expected.Length, length, "iso-8859-1 length");
				}
			}
		}

		static void TestOpenPgpBlockFilter (OpenPgpBlockFilter filter, byte[] buffer, string expected, int increment)
		{
			using (var stream = new MemoryStream ()) {
				using (var filtered = new FilteredStream (stream)) {
					int startIndex = 0;

					filtered.Add (filter);

					while (startIndex < buffer.Length) {
						int n = Math.Min (buffer.Length - startIndex, increment);

						filtered.Write (buffer, startIndex, n);
						startIndex += n;
					}

					filtered.Flush ();

					var actual = Encoding.UTF8.GetString (stream.GetBuffer (), 0, (int) stream.Length);

					Assert.AreEqual (expected, actual, "increment of {0} failed.", increment);
				}
			}
		}

		[Test]
		public void TestOpenPgpBlockFilter ()
		{
			const string input = "% cat sample\nThis is a sample.\n\nThis is a sample text file.  I created it with an editor.  If it were\nan actual message, it would contain some useful information.\n\nThis has been a sample.\n% pgp -eat sample john\nPretty Good Privacy(tm) 2.6.2 - Public-key encryption for the masses.\n(c) 1990-1994 Philip Zimmermann, Phil's Pretty Good Software. 11 Oct 94\nUses the RSAREF(tm) Toolkit, which is copyright RSA Data Security, Inc.\nDistributed by the Massachusetts Institute of Technology.\nExport of this software may be restricted by the U.S. government.\nCurrent time: 1996/11/09 13:10 GMT\n\n\nRecipients' public key(s) will be used to encrypt. \nKey for user ID: John E Doe <jd@somewhere.net>\n1024-bit key, Key ID F4DD25F1, created 1996/11/07\n\nWARNING:  Because this public key is not certified with a trusted\nsignature, it is not known with high confidence that this public key\nactually belongs to: \"John E Doe <jd@somewhere.net>\".\n\nAre you sure you want to use this public key (y/N)? y\n.\nTransport armor file: sample.asc\n% cat sample.asc\n-----BEGIN PGP MESSAGE-----\nVersion: 2.6.2\n\nhIwD1vwet/TdJfEBBACdcCPkNI3kRwYqtHUyfpvVAY5rt+Lb9P6EztNd4sYq9egV\nCZjfqcCn36XZmYPbbO6nZbl992kPRFzTgCRszKNPtlk6Wa93AqXs3KCZp+4emXQh\n7moE+XTf4QUGJZ2L3w/sSNs5WFkZRIbto0ivK1aRlX1XTqhPqo9HbgEfElBVUaYA\nAACQEWaOS3/h6BVLHTfXaK20vmLcg9BUisB5RDvYGLZv9XFwHMMjctFJJQYnWIOp\n+7LLkmNO5fE48rWh0EOAwjAeduGzJGQb4yiE7OlxoESmmTJQ+qO1K2nDz8Stk3a6\nWvAQJrpEUY7Og8QGlQQRPKl2F++j6XbIhZ27OeYqJp+vgylUd874KDMCcTrzF3ph\n/Qfi\n=xTV9\n-----END PGP MESSAGE-----\n%\n";
			const string expected = "-----BEGIN PGP MESSAGE-----\nVersion: 2.6.2\n\nhIwD1vwet/TdJfEBBACdcCPkNI3kRwYqtHUyfpvVAY5rt+Lb9P6EztNd4sYq9egV\nCZjfqcCn36XZmYPbbO6nZbl992kPRFzTgCRszKNPtlk6Wa93AqXs3KCZp+4emXQh\n7moE+XTf4QUGJZ2L3w/sSNs5WFkZRIbto0ivK1aRlX1XTqhPqo9HbgEfElBVUaYA\nAACQEWaOS3/h6BVLHTfXaK20vmLcg9BUisB5RDvYGLZv9XFwHMMjctFJJQYnWIOp\n+7LLkmNO5fE48rWh0EOAwjAeduGzJGQb4yiE7OlxoESmmTJQ+qO1K2nDz8Stk3a6\nWvAQJrpEUY7Og8QGlQQRPKl2F++j6XbIhZ27OeYqJp+vgylUd874KDMCcTrzF3ph\n/Qfi\n=xTV9\n-----END PGP MESSAGE-----\n";
			var filter = new OpenPgpBlockFilter ("-----BEGIN PGP MESSAGE-----", "-----END PGP MESSAGE-----");
			var buffer = Encoding.UTF8.GetBytes (input);

			//Assert.Throws<ArgumentNullException> (() => new OpenPgpBlockFilter (null, "marker"));
			//Assert.Throws<ArgumentNullException> (() => new OpenPgpBlockFilter ("marker", null));

			TestOpenPgpBlockFilter (filter, buffer, expected, 20);

			// Make sure that resetting state works
			filter.Reset ();
			TestOpenPgpBlockFilter (filter, buffer, expected, 21);
		}
	}
}

//
// FilterTests.cs
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

using System.Text;

using MimeKit;
using MimeKit.IO;
using MimeKit.Utils;
using MimeKit.IO.Filters;
using MimeKit.Cryptography;

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

					Assert.That (actual, Is.EqualTo (expected), "From armoring failed when end boundary falls in the middle of From.");
				}
			}
		}

		[Test]
		public void TestMboxFromFilter ()
		{
			const string text = "This text is meant to test that the filter will armor lines beginning with\nFrom (like mbox). And let's add another\nFrom line for good measure, shall we?\n";
			const string expected = "This text is meant to test that the filter will armor lines beginning with\n>From (like mbox). And let's add another\n>From line for good measure, shall we?\n";
			var filter = new MboxFromFilter ();

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

					Assert.That (actual, Is.EqualTo (expected), "From armoring failed when end boundary falls in the middle of From.");
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

					Assert.That (filtered.CanTimeout, Is.False, "CanTimeout");
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

					Assert.That (filtered.Contains (filter), Is.True, "Contains");

					filtered.Write (buffer, 0, buffer.Length);
					filtered.Flush ();

					encoding = filter.GetBestEncoding (EncodingConstraint.SevenBit);
					Assert.That (encoding, Is.EqualTo (ContentEncoding.SevenBit), "ASCII 7bit constraint.");

					encoding = filter.GetBestEncoding (EncodingConstraint.EightBit);
					Assert.That (encoding, Is.EqualTo (ContentEncoding.SevenBit), "ASCII 8bit constraint.");

					encoding = filter.GetBestEncoding (EncodingConstraint.None);
					Assert.That (encoding, Is.EqualTo (ContentEncoding.SevenBit), "ASCII no constraint.");

					Assert.That (filtered.Remove (filter), Is.True, "Remove");
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
					Assert.That (encoding, Is.EqualTo (ContentEncoding.QuotedPrintable), "From-line 7bit constraint.");

					encoding = filter.GetBestEncoding (EncodingConstraint.EightBit);
					Assert.That (encoding, Is.EqualTo (ContentEncoding.QuotedPrintable), "From-line 8bit constraint.");

					encoding = filter.GetBestEncoding (EncodingConstraint.None);
					Assert.That (encoding, Is.EqualTo (ContentEncoding.QuotedPrintable), "From-line no constraint.");
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
					Assert.That (encoding, Is.EqualTo (ContentEncoding.QuotedPrintable), "French 7bit constraint.");

					encoding = filter.GetBestEncoding (EncodingConstraint.EightBit);
					Assert.That (encoding, Is.EqualTo (ContentEncoding.EightBit), "French 8bit constraint.");

					encoding = filter.GetBestEncoding (EncodingConstraint.None);
					Assert.That (encoding, Is.EqualTo (ContentEncoding.EightBit), "French no constraint.");

					filter.Reset ();

					// write the entire French text this time (longest line exceeds 78 chars)
					filtered.Write (buffer, 0, buffer.Length);
					filtered.Flush ();

					encoding = filter.GetBestEncoding (EncodingConstraint.SevenBit);
					Assert.That (encoding, Is.EqualTo (ContentEncoding.QuotedPrintable), "French (long lines) 7bit constraint.");

					encoding = filter.GetBestEncoding (EncodingConstraint.EightBit);
					Assert.That (encoding, Is.EqualTo (ContentEncoding.QuotedPrintable), "French (long lines) 8bit constraint.");

					encoding = filter.GetBestEncoding (EncodingConstraint.None);
					Assert.That (encoding, Is.EqualTo (ContentEncoding.QuotedPrintable), "French (long lines) no constraint.");
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
					Assert.That (encoding, Is.EqualTo (ContentEncoding.SevenBit), "78-character line; 7bit constraint.");

					encoding = filter.GetBestEncoding (EncodingConstraint.EightBit, 78);
					Assert.That (encoding, Is.EqualTo (ContentEncoding.SevenBit), "78-character line; 8bit constraint.");

					encoding = filter.GetBestEncoding (EncodingConstraint.None, 78);
					Assert.That (encoding, Is.EqualTo (ContentEncoding.SevenBit), "78-character line; no constraint.");
				}
			}
		}

		[Test]
		public void TestPassThroughFilter ()
		{
			var filter = new PassThroughFilter ();
			int outputIndex, outputLength;
			var buffer = new byte[10];

			Assert.That (filter.Filter (buffer, 1, buffer.Length - 2, out outputIndex, out outputLength), Is.EqualTo (buffer));
			Assert.That (outputIndex, Is.EqualTo (1), "outputIndex");
			Assert.That (outputLength, Is.EqualTo (buffer.Length - 2), "outputLength");

			Assert.That (filter.Flush (buffer, 1, buffer.Length - 2, out outputIndex, out outputLength), Is.EqualTo (buffer));
			Assert.That (outputIndex, Is.EqualTo (1), "outputIndex");
			Assert.That (outputLength, Is.EqualTo (buffer.Length - 2), "outputLength");

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

					Assert.That (length, Is.EqualTo (expected.Length), "iso-8859-15 length");
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

					Assert.That (length, Is.EqualTo (expected.Length), "iso-8859-1 length");
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

					Assert.That (actual, Is.EqualTo (expected), $"increment of {increment} failed.");
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

		static void TestUnix2DosFilter (string text, string expected, bool ensureNewLine)
		{
			var filter = new Unix2DosFilter (ensureNewLine);

			TestArgumentExceptions (filter);

			using (var stream = new MemoryStream ()) {
				using (var filtered = new FilteredStream (stream)) {
					var buffer = Encoding.UTF8.GetBytes (text);

					filtered.Add (filter);
					filtered.Write (buffer, 0, buffer.Length);
					filtered.Flush ();
					filtered.Flush ();

					var actual = Encoding.UTF8.GetString (stream.GetBuffer (), 0, (int) stream.Length);

					Assert.That (actual, Is.EqualTo (expected), $"unix2dos failed. EnsureNewLine = {ensureNewLine}");
				}
			}
		}

		[Test]
		public void TestUnix2DosFilterSimple ()
		{
			const string text = "This text is meant to test that the filter will convert unix line endings to dos.\nHere's a second line of text.\nAnd one more line for good measure, shall we?";
			const string expected = "This text is meant to test that the filter will convert unix line endings to dos.\r\nHere's a second line of text.\r\nAnd one more line for good measure, shall we?";

			TestUnix2DosFilter (text, expected, false);
			TestUnix2DosFilter (text, expected + "\r\n", true);
		}

		[Test]
		public void TestUnix2DosFilterMixedLineEndings ()
		{
			const string text = "This text is meant to test that the filter will convert unix line endings to dos.\nHere's a second line of text.\r\nAnd one more line for good measure, shall we?\r";
			const string expected = "This text is meant to test that the filter will convert unix line endings to dos.\r\nHere's a second line of text.\r\nAnd one more line for good measure, shall we?\r";

			TestUnix2DosFilter (text, expected, false);
			TestUnix2DosFilter (text, expected + '\n', true);
		}

		static void TestDos2UnixFilter (string text, string expected, bool ensureNewLine)
		{
			var filter = new Dos2UnixFilter (ensureNewLine);

			TestArgumentExceptions (filter);

			using (var stream = new MemoryStream ()) {
				using (var filtered = new FilteredStream (stream)) {
					var buffer = Encoding.UTF8.GetBytes (text);

					filtered.Add (filter);
					filtered.Write (buffer, 0, buffer.Length);
					filtered.Flush ();
					filtered.Flush ();

					var actual = Encoding.UTF8.GetString (stream.GetBuffer (), 0, (int) stream.Length);

					Assert.That (actual, Is.EqualTo (expected), $"dos2unix failed. EnsureNewLine = {ensureNewLine}");
				}
			}
		}

		[Test]
		public void TestDos2UnixFilterSimple ()
		{
			const string text = "This text is meant to test that the filter will convert dos line endings to unix.\r\nHere's a second line of text.\r\nAnd one more line for good measure, shall we?";
			const string expected = "This text is meant to test that the filter will convert dos line endings to unix.\nHere's a second line of text.\nAnd one more line for good measure, shall we?";

			TestDos2UnixFilter (text, expected, false);
			TestDos2UnixFilter (text, expected + '\n', true);
		}

		[Test]
		public void TestDos2UnixFilterMixedLineEndings ()
		{
			const string text = "This text is meant to test that the filter will convert dos line endings to unix.\nHere's a second line of text.\r\nAnd one more line for good measure, shall we?\n";
			const string expected = "This text is meant to test that the filter will convert dos line endings to unix.\nHere's a second line of text.\nAnd one more line for good measure, shall we?\n";

			TestDos2UnixFilter (text, expected, false);
			TestDos2UnixFilter (text, expected, true);
		}
	}
}

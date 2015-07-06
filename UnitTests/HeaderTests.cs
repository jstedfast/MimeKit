//
// HeaderTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2014 Xamarin Inc. (www.xamarin.com)
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
using System.Text;
using NUnit.Framework;

using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class HeaderTests
	{
		static string ByteArrayToString (byte[] text)
		{
			var builder = new StringBuilder ();

			for (int i = 0; i < text.Length; i++)
				builder.Append ((char) text[i]);

			return builder.ToString ();
		}

		static int GetMaxLineLength (string text)
		{
			int current = 0;
			int max = 0;

			for (int i = 0; i < text.Length; i++) {
				if (text[i] == '\r' && text[i + 1] == '\n')
					i++;

				if (text[i] == '\n') {
					max = Math.Max (max, current);
					current = 0;
				} else {
					current++;
				}
			}

			return max;
		}

		[Test]
		public void TestAddressHeaderFolding ()
		{
			var expected = " Jeffrey Stedfast <jeff@xamarin.com>, \"Jeffrey A. Stedfast\"" + FormatOptions.Default.NewLine +
				"\t<jeff@xamarin.com>, \"Dr. Gregory House, M.D.\"" + FormatOptions.Default.NewLine +
				"\t<house@princeton-plainsboro-hospital.com>" + FormatOptions.Default.NewLine;
			var header = new Header ("To", "Jeffrey Stedfast <jeff@xamarin.com>, \"Jeffrey A. Stedfast\" <jeff@xamarin.com>, \"Dr. Gregory House, M.D.\" <house@princeton-plainsboro-hospital.com>");
			var raw = ByteArrayToString (header.RawValue);

			Assert.IsTrue (raw[raw.Length - 1] == '\n', "The RawValue does not end with a new line.");

			Assert.IsTrue (GetMaxLineLength (raw) < FormatOptions.Default.MaxLineLength, "The RawValue is not folded properly.");
			Assert.AreEqual (expected, raw, "The folded address header does not match the expected value.");
		}

		[Test]
		public void TestMessageIdHeaderFolding ()
		{
			var header = new Header ("Message-Id", string.Format ("<{0}@princeton-plainsboro-hospital.com>", Guid.NewGuid ()));
			var expected = " " + header.Value + FormatOptions.Default.NewLine;
			var raw = ByteArrayToString (header.RawValue);

			Assert.IsTrue (raw[raw.Length - 1] == '\n', "The RawValue does not end with a new line.");

			Assert.AreEqual (expected, raw, "The folded Message-Id header does not match the expected value.");
		}

		static readonly string[] ReceivedHeaderValues = {
			" from thumper.bellcore.com by greenbush.bellcore.com (4.1/4.7)" + FormatOptions.Default.NewLine + "\tid <AA01648> for nsb; Fri, 29 Nov 91 07:13:33 EST",
			" from joyce.cs.su.oz.au by thumper.bellcore.com (4.1/4.7)" + FormatOptions.Default.NewLine + "\tid <AA11898> for nsb@greenbush; Fri, 29 Nov 91 07:11:57 EST",
			" from Messages.8.5.N.CUILIB.3.45.SNAP.NOT.LINKED.greenbush.galaxy.sun4.41" + FormatOptions.Default.NewLine + "\tvia MS.5.6.greenbush.galaxy.sun4_41; Fri, 12 Jun 1992 13:29:05 -0400 (EDT)",
			" from sqhilton.pc.cs.cmu.edu by po3.andrew.cmu.edu (5.54/3.15)" + FormatOptions.Default.NewLine + "\tid <AA21478> for beatty@cosmos.vlsi.cs.cmu.edu; Wed, 26 Aug 92 22:14:07 EDT",
		};

		[Test]
		public void TestReceivedHeaderFolding ()
		{
			var header = new Header ("Received", "");

			foreach (var received in ReceivedHeaderValues) {
				header.SetValue (Encoding.ASCII, received.Replace (FormatOptions.Default.NewLine + "\t", " ").Trim ());

				var raw = ByteArrayToString (header.RawValue);

				Assert.IsTrue (raw[raw.Length - 1] == '\n', "The RawValue does not end with a new line.");

				Assert.AreEqual (received + FormatOptions.Default.NewLine, raw, "The folded Received header does not match the expected value.");
			}
		}

		[Test]
		public void TestReferencesHeaderFolding ()
		{
			var expected = new StringBuilder ();

			expected.AppendFormat (" <{0}@princeton-plainsboro-hospital.com>", Guid.NewGuid ());
			for (int i = 0; i < 5; i++)
				expected.AppendFormat ("{0}\t<{1}@princeton-plainsboro-hospital.com>", FormatOptions.Default.NewLine, Guid.NewGuid ());

			expected.Append (FormatOptions.Default.NewLine);

			var header = new Header ("References", expected.ToString ());
			var raw = ByteArrayToString (header.RawValue);

			Assert.IsTrue (raw[raw.Length - 1] == '\n', "The RawValue does not end with a new line.");

			Assert.AreEqual (expected.ToString (), raw, "The folded References header does not match the expected value.");
		}

		[Test]
		public void TestDkimSignatureHeaderFolding ()
		{
			var header = new Header ("UTF-8", "DKIM-Signature", "v=1; a=rsa-sha256; c=simple/simple; d=maillist.codeproject.com; s=mail; t=1435835767; bh=tiafHSAvEg4GPJlbkR6e7qr1oydTj+ZXs392TcHwwvs=; h=MIME-Version:From:To:Date:Subject:Content-Type:Content-Transfer-Encoding:Message-Id; b=Qtgo0bWwT0H18CxD2+ey8/382791TBNYtZ8VOLlXxxsbw5fab8uEo53o5tPun6kNx4khmJx/yWowvrCOAcMoqgNO7Hb7JB8NR7eNyOvtLKCG34AfDZyHNcTZHR/QnBpRKHssu5w2CQDUAjKnuGKRW95LCMMX3r924dErZOJnGhs=");
			var expected = " v=1; a=rsa-sha256; c=simple/simple;\n\td=maillist.codeproject.com; s=mail; t=1435835767;\n\tbh=tiafHSAvEg4GPJlbkR6e7qr1oydTj+ZXs392TcHwwvs=;\n\th=MIME-Version:From:To:Date:Subject:Content-Type:Content-Transfer-Encoding:\n\tMessage-Id;\n\tb=Qtgo0bWwT0H18CxD2+ey8/382791TBNYtZ8VOLlXxxsbw5fab8uEo53o5tPun6kNx4khmJx/yWo\n\twvrCOAcMoqgNO7Hb7JB8NR7eNyOvtLKCG34AfDZyHNcTZHR/QnBpRKHssu5w2CQDUAjKnuGKRW95L\n\tCMMX3r924dErZOJnGhs=\n";
			var raw = ByteArrayToString (header.RawValue);

			expected = expected.Replace ("\n", Environment.NewLine);

			Assert.AreEqual (expected, raw, "The RawValue does not match the expected value.");
		}

		[Test]
		public void TestUnstructuredHeaderFolding ()
		{
			var header = new Header ("Subject", "This is a subject value that should be long enough to force line wrapping to keep the line length under the 78 character limit.");
			var raw = ByteArrayToString (header.RawValue);

			Assert.IsTrue (raw[raw.Length - 1] == '\n', "The RawValue does not end with a new line.");

			Assert.IsTrue (GetMaxLineLength (raw) < FormatOptions.Default.MaxLineLength, "The RawValue is not folded properly.");

			var unfolded = Header.Unfold (raw);
			Assert.AreEqual (header.Value, unfolded, "Unfolded header does not match the original header value.");
		}

		[Test]
		public void TestSimpleInternationalizedUnstructuredHeaderFolding ()
		{
			var options = FormatOptions.Default.Clone ();
			string original, folded, unfolded;

			options.International = true;

			original = "This is a subject value that should be long enough to force line wrapping to keep the line length under the 78 character limit.";
			folded = Header.Fold (options, "Subject", original);
			unfolded = Header.Unfold (folded);

			Assert.IsTrue (folded[folded.Length - 1] == '\n', "The folded header does not end with a new line.");
			Assert.IsTrue (GetMaxLineLength (folded) < FormatOptions.Default.MaxLineLength, "The raw header value is not folded properly. ");
			Assert.AreEqual (original, unfolded, "Unfolded header does not match the original header value.");
		}

		[Test]
		public void TestArabicInternationalizedUnstructuredHeaderFolding ()
		{
			var options = FormatOptions.Default.Clone ();
			string original, folded, unfolded;

			options.International = true;

			original = "هل تتكلم اللغة الإنجليزية /العربية؟" + "هل تتكلم اللغة الإنجليزية /العربية؟" + "هل تتكلم اللغة الإنجليزية /العربية؟" + "هل تتكلم اللغة الإنجليزية /العربية؟" + "هل تتكلم اللغة الإنجليزية /العربية؟";
			folded = Header.Fold (options, "Subject", original);
			unfolded = Header.Unfold (folded);

			Assert.IsTrue (folded[folded.Length - 1] == '\n', "The folded header does not end with a new line.");
			Assert.IsTrue (GetMaxLineLength (folded) < FormatOptions.Default.MaxLineLength, "The raw header value is not folded properly. ");
			Assert.AreEqual (original, unfolded, "Unfolded header does not match the original header value.");
		}

		[Test]
		public void TestJapaneseInternationalizedUnstructuredHeaderFolding ()
		{
			var options = FormatOptions.Default.Clone ();
			string original, folded, unfolded;

			options.International = true;

			original = "狂ったこの世で狂うなら気は確かだ。" + "狂ったこの世で狂うなら気は確かだ。" + "狂ったこの世で狂うなら気は確かだ。" + "狂ったこの世で狂うなら気は確かだ。";
			folded = Header.Fold (options, "Subject", original);
			unfolded = Header.Unfold (folded).Replace (" ", "");

			Assert.IsTrue (folded[folded.Length - 1] == '\n', "The folded header does not end with a new line.");
			Assert.IsTrue (GetMaxLineLength (folded) < FormatOptions.Default.MaxLineLength, "The raw header value is not folded properly. ");
			Assert.AreEqual (original, unfolded, "Unfolded header does not match the original header value.");
		}

		[Test]
		public void TestReplacingHeaders ()
		{
			const string ReplacedContentType = "text/plain; charset=iso-8859-1; name=body.txt";
			const string ReplacedContentDisposition = "inline; filename=body.txt";
			const string ReplacedContentLocation = "http://www.example.com/location";
			const string ReplacedContentId = "<content.id.2@localhost>";
			var headers = new HeaderList ();

			headers.Add ("Content-Disposition", "attachment");
			headers.Add ("Content-Id", "<content-id.1@localhost>");
			headers.Add ("Content-Location", "C:\\location");
			headers.Insert (0, "Content-Type", "text/plain");

			Assert.IsTrue (headers.Contains (HeaderId.ContentType), "Expected the list of headers to contain HeaderId.ContentType.");
			Assert.IsTrue (headers.Contains ("Content-Type"), "Expected the list of headers to contain a Content-Type header.");
			Assert.AreEqual (0, headers.LastIndexOf (HeaderId.ContentType), "Expected the Content-Type header to be the first header.");

			headers.Replace ("Content-Disposition", ReplacedContentDisposition);
			Assert.AreEqual (4, headers.Count, "Unexpected number of headers after replacing Content-Disposition.");
			Assert.AreEqual (ReplacedContentDisposition, headers["Content-Disposition"], "Content-Disposition has unexpected value after replacing it.");
			Assert.AreEqual (1, headers.IndexOf ("Content-Disposition"), "Replaced Content-Disposition not in the expected position.");

			headers.Replace (HeaderId.ContentType, "UTF-8", ReplacedContentType);
			Assert.AreEqual (4, headers.Count, "Unexpected number of headers after replacing Content-Type.");
			Assert.AreEqual (ReplacedContentType, headers["Content-Type"], "Content-Type has unexpected value after replacing it.");
			Assert.AreEqual (0, headers.IndexOf ("Content-Type"), "Replaced Content-Type not in the expected position.");

			headers.Replace (HeaderId.ContentId, Encoding.UTF8, ReplacedContentId);
			Assert.AreEqual (4, headers.Count, "Unexpected number of headers after replacing Content-Id.");
			Assert.AreEqual (ReplacedContentId, headers["Content-Id"], "Content-Id has unexpected value after replacing it.");
			Assert.AreEqual (2, headers.IndexOf ("Content-Id"), "Replaced Content-Id not in the expected position.");

			headers.Replace ("Content-Location", Encoding.UTF8, ReplacedContentLocation);
			Assert.AreEqual (4, headers.Count, "Unexpected number of headers after replacing Content-Location.");
			Assert.AreEqual (ReplacedContentLocation, headers["Content-Location"], "Content-Location has unexpected value after replacing it.");
			Assert.AreEqual (3, headers.IndexOf ("Content-Location"), "Replaced Content-Location not in the expected position.");

			headers.RemoveAll ("Content-Location");
			Assert.AreEqual (3, headers.Count, "Unexpected number of headers after removing Content-Location.");
		}

		[Test]
		public void TestReplacingMultipleHeaders ()
		{
			const string CombinedRecpients = "first@localhost, second@localhost, third@localhost";
			var headers = new HeaderList ();

			headers.Add ("From", "sender@localhost");
			headers.Add ("To", "first@localhost");
			headers.Add ("To", "second@localhost");
			headers.Add ("To", "third@localhost");
			headers.Add ("Cc", "carbon.copy@localhost");

			headers.Replace ("To", CombinedRecpients);
			Assert.AreEqual (3, headers.Count, "Unexpected number of headers after replacing To header.");
			Assert.AreEqual (CombinedRecpients, headers["To"], "To header has unexpected value after being replaced.");
			Assert.AreEqual (1, headers.IndexOf ("To"), "Replaced To header not in the expected position.");
			Assert.AreEqual (0, headers.IndexOf ("From"), "From header not in the expected position.");
			Assert.AreEqual (2, headers.IndexOf ("Cc"), "Cc header not in the expected position.");
		}

		[Test]
		public void TestToHeaderId ()
		{
			HeaderId parsed;
			string name;

			foreach (HeaderId value in Enum.GetValues (typeof (HeaderId))) {
				if (value == HeaderId.Unknown)
					continue;

				name = value.ToHeaderName ().ToUpperInvariant ();
				parsed = name.ToHeaderId ();

				Assert.AreEqual (value, parsed, "Failed to parse the HeaderId value for {0}", value);
			}

			parsed = "X-MadeUp-Header".ToHeaderId ();

			Assert.AreEqual (HeaderId.Unknown, parsed, "Failed to parse the made-up header value");
		}
	}
}

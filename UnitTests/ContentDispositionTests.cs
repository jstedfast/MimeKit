//
// ContentDispositionTests.cs
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

namespace UnitTests {
	[TestFixture]
	public class ContentDispositionTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var disposition = new ContentDisposition ();

			Assert.Throws<ArgumentNullException> (() => disposition.Disposition = null, "Setting the disposition to null value should throw.");
			Assert.Throws<ArgumentException> (() => disposition.Disposition = string.Empty, "Setting the disposition to an empty value should throw.");
			Assert.Throws<ArgumentException> (() => disposition.Disposition = "žádost", "Setting the disposition to a non-ascii value should throw.");
			Assert.Throws<ArgumentException> (() => disposition.Disposition = "two atoms", "Setting the disposition to multiple atom tokens should throw.");

			Assert.Throws<ArgumentNullException> (() => disposition.ToString (null, Encoding.UTF8, true));
			Assert.Throws<ArgumentNullException> (() => disposition.ToString (FormatOptions.Default, null, true));
		}

		[Test]
		public void TestClone ()
		{
			var original = new ContentDisposition {
				CreationDate = DateTimeOffset.Now,
				ModificationDate = DateTimeOffset.Now,
				ReadDate = DateTimeOffset.Now,
				FileName = "clone-me.txt",
				Size = 10
			};
			var clone = original.Clone ();

			Assert.That (clone.Disposition, Is.EqualTo (original.Disposition), "Disposition");
			Assert.That (clone.Parameters.Count, Is.EqualTo (original.Parameters.Count), "Parameters.Count");
			Assert.That (clone.CreationDate, Is.EqualTo (original.CreationDate), "CreationDate");
			Assert.That (clone.ModificationDate, Is.EqualTo (original.ModificationDate), "ModificationDate");
			Assert.That (clone.ReadDate, Is.EqualTo (original.ReadDate), "ReadDate");
			Assert.That (clone.FileName, Is.EqualTo (original.FileName), "FileName");
			Assert.That (clone.Size, Is.EqualTo (original.Size), "Size");
		}

		[Test]
		public void TestChangedEvents ()
		{
			var timestamp = new DateTimeOffset (2022, 9, 9, 7, 41, 23, new TimeSpan (-4, 0, 0));
			var disposition = new ContentDisposition (ContentDisposition.Attachment);
			int changed = 0;

			disposition.Changed += (sender, args) => { changed++; };

			disposition.Disposition = ContentDisposition.Attachment;
			Assert.That (changed, Is.EqualTo (0), "Setting the same Disposition value should not emit the Changed event");

			disposition.Disposition = ContentDisposition.Inline;
			Assert.That (changed, Is.EqualTo (1), "Setting a different Disposition value SHOULD emit the Changed event");
			changed = 0;

			disposition.FileName = "filename.txt";
			Assert.That (changed, Is.EqualTo (1), "Setting an initial FileName value SHOULD emit the Changed event");
			changed = 0;

			disposition.FileName = "filename.txt";
			Assert.That (changed, Is.EqualTo (0), "Setting the same FileName value should not emit the Changed event");

			disposition.FileName = "filename.pdf";
			Assert.That (changed, Is.EqualTo (1), "Setting a different FileName value SHOULD emit the Changed event");
			changed = 0;

			disposition.FileName = null;
			Assert.That (changed, Is.EqualTo (1), "Removing the FileName SHOULD emit the Changed event");
			changed = 0;

			disposition.CreationDate = timestamp;
			Assert.That (changed, Is.EqualTo (1), "Setting an initial CreationDate value SHOULD emit the Changed event");
			changed = 0;

			disposition.CreationDate = timestamp;
			Assert.That (changed, Is.EqualTo (0), "Setting the same CreationDate value should not emit the Changed event");

			disposition.CreationDate = DateTimeOffset.Now;
			Assert.That (changed, Is.EqualTo (1), "Setting a different CreationDate value SHOULD emit the Changed event");
			changed = 0;

			disposition.CreationDate = null;
			Assert.That (changed, Is.EqualTo (1), "Removing the CreationDate SHOULD emit the Changed event");
			changed = 0;

			disposition.ModificationDate = timestamp;
			Assert.That (changed, Is.EqualTo (1), "Setting an initial ModificationDate value SHOULD emit the Changed event");
			changed = 0;

			disposition.ModificationDate = timestamp;
			Assert.That (changed, Is.EqualTo (0), "Setting the same ModificationDate value should not emit the Changed event");

			disposition.ModificationDate = DateTimeOffset.Now;
			Assert.That (changed, Is.EqualTo (1), "Setting a different ModificationDate value SHOULD emit the Changed event");
			changed = 0;

			disposition.ModificationDate = null;
			Assert.That (changed, Is.EqualTo (1), "Removing the ModificationDate SHOULD emit the Changed event");
			changed = 0;

			disposition.ReadDate = timestamp;
			Assert.That (changed, Is.EqualTo (1), "Setting an initial ReadDate value SHOULD emit the Changed event");
			changed = 0;

			disposition.ReadDate = timestamp;
			Assert.That (changed, Is.EqualTo (0), "Setting the same ReadDate value should not emit the Changed event");

			disposition.ReadDate = DateTimeOffset.Now;
			Assert.That (changed, Is.EqualTo (1), "Setting a different ReadDate value SHOULD emit the Changed event");
			changed = 0;

			disposition.ReadDate = null;
			Assert.That (changed, Is.EqualTo (1), "Removing the ReadDate SHOULD emit the Changed event");
			changed = 0;

			disposition.Size = 1024;
			Assert.That (changed, Is.EqualTo (1), "Setting an initial Size value SHOULD emit the Changed event");
			changed = 0;

			disposition.Size = 1024;
			Assert.That (changed, Is.EqualTo (0), "Setting the same Size value should not emit the Changed event");

			disposition.Size = 2048;
			Assert.That (changed, Is.EqualTo (1), "Setting a different Size value SHOULD emit the Changed event");
			changed = 0;

			disposition.Size = null;
			Assert.That (changed, Is.EqualTo (1), "Removing the Size SHOULD emit the Changed event");
			changed = 0;
		}

		static void AssertParseResults (ContentDisposition disposition, ContentDisposition expected)
		{
			if (expected == null) {
				Assert.That (disposition, Is.Null);
				return;
			}

			Assert.That (disposition.Disposition, Is.EqualTo (expected.Disposition), "Disposition");
			Assert.That (disposition.Parameters.Count, Is.EqualTo (expected.Parameters.Count), "Parameter count");

			for (int i = 0; i < expected.Parameters.Count; i++) {
				var encoding = expected.Parameters[i].Encoding;
				var value = expected.Parameters[i].Value;
				var name = expected.Parameters[i].Name;

				Assert.That (disposition.Parameters[i].Name, Is.EqualTo (name));
				Assert.That (disposition.Parameters[i].Encoding.EncodingName, Is.EqualTo (encoding.EncodingName));
				Assert.That (disposition.Parameters[i].Value, Is.EqualTo (value));
				Assert.That (disposition.Parameters.Contains (name), Is.True);
				Assert.That (disposition.Parameters[name], Is.EqualTo (expected.Parameters[name]));
			}
		}

		static void AssertParse (string text, ContentDisposition expected, bool result = true, int tokenIndex = -1, int errorIndex = -1)
		{
			var buffer = Encoding.UTF8.GetBytes (text);
			var options = ParserOptions.Default;
			ContentDisposition disposition;

			Assert.That (ContentDisposition.TryParse (text, out disposition), Is.EqualTo (result), $"Unexpected result for TryParse: {text}");
			AssertParseResults (disposition, expected);

			Assert.That (ContentDisposition.TryParse (options, text, out disposition), Is.EqualTo (result), $"Unexpected result for TryParse: {text}");
			AssertParseResults (disposition, expected);

			Assert.That (ContentDisposition.TryParse (buffer, out disposition), Is.EqualTo (result), $"Unexpected result for TryParse: {text}");
			AssertParseResults (disposition, expected);

			Assert.That (ContentDisposition.TryParse (options, buffer, out disposition), Is.EqualTo (result), $"Unexpected result for TryParse: {text}");
			AssertParseResults (disposition, expected);

			Assert.That (ContentDisposition.TryParse (buffer, 0, out disposition), Is.EqualTo (result), $"Unexpected result for TryParse: {text}");
			AssertParseResults (disposition, expected);

			Assert.That (ContentDisposition.TryParse (options, buffer, 0, out disposition), Is.EqualTo (result), $"Unexpected result for TryParse: {text}");
			AssertParseResults (disposition, expected);

			Assert.That (ContentDisposition.TryParse (buffer, 0, buffer.Length, out disposition), Is.EqualTo (result), $"Unexpected result for TryParse: {text}");
			AssertParseResults (disposition, expected);

			Assert.That (ContentDisposition.TryParse (options, buffer, 0, buffer.Length, out disposition), Is.EqualTo (result), $"Unexpected result for TryParse: {text}");
			AssertParseResults (disposition, expected);

			try {
				disposition = ContentDisposition.Parse (text);
				if (tokenIndex != -1 && errorIndex != -1)
					Assert.Fail ($"Parsing \"{text}\" should have failed.");
				AssertParseResults (disposition, expected);
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "Unexpected token index");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "Unexpected error index");
			} catch (Exception e) {
				Assert.Fail ($"Unexpected exception: {e}");
			}

			try {
				disposition = ContentDisposition.Parse (options, text);
				if (tokenIndex != -1 && errorIndex != -1)
					Assert.Fail ($"Parsing \"{text}\" should have failed.");
				AssertParseResults (disposition, expected);
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "Unexpected token index");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "Unexpected error index");
			} catch (Exception e) {
				Assert.Fail ($"Unexpected exception: {e}");
			}

			try {
				disposition = ContentDisposition.Parse (buffer);
				if (tokenIndex != -1 && errorIndex != -1)
					Assert.Fail ($"Parsing \"{text}\" should have failed.");
				AssertParseResults (disposition, expected);
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "Unexpected token index");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "Unexpected error index");
			} catch (Exception e) {
				Assert.Fail ($"Unexpected exception: {e}");
			}

			try {
				disposition = ContentDisposition.Parse (options, buffer);
				if (tokenIndex != -1 && errorIndex != -1)
					Assert.Fail ($"Parsing \"{text}\" should have failed.");
				AssertParseResults (disposition, expected);
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "Unexpected token index");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "Unexpected error index");
			} catch (Exception e) {
				Assert.Fail ($"Unexpected exception: {e}");
			}

			try {
				disposition = ContentDisposition.Parse (buffer, 0);
				if (tokenIndex != -1 && errorIndex != -1)
					Assert.Fail ($"Parsing \"{text}\" should have failed.");
				AssertParseResults (disposition, expected);
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "Unexpected token index");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "Unexpected error index");
			} catch (Exception e) {
				Assert.Fail ($"Unexpected exception: {e}");
			}

			try {
				disposition = ContentDisposition.Parse (options, buffer, 0);
				if (tokenIndex != -1 && errorIndex != -1)
					Assert.Fail ($"Parsing \"{text}\" should have failed.");
				AssertParseResults (disposition, expected);
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "Unexpected token index");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "Unexpected error index");
			} catch (Exception e) {
				Assert.Fail ($"Unexpected exception: {e}");
			}

			try {
				disposition = ContentDisposition.Parse (buffer, 0, buffer.Length);
				if (tokenIndex != -1 && errorIndex != -1)
					Assert.Fail ($"Parsing \"{text}\" should have failed.");
				AssertParseResults (disposition, expected);
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "Unexpected token index");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "Unexpected error index");
			} catch (Exception e) {
				Assert.Fail ($"Unexpected exception: {e}");
			}

			try {
				disposition = ContentDisposition.Parse (options, buffer, 0, buffer.Length);
				if (tokenIndex != -1 && errorIndex != -1)
					Assert.Fail ($"Parsing \"{text}\" should have failed.");
				AssertParseResults (disposition, expected);
			} catch (ParseException ex) {
				Assert.That (ex.TokenIndex, Is.EqualTo (tokenIndex), "Unexpected token index");
				Assert.That (ex.ErrorIndex, Is.EqualTo (errorIndex), "Unexpected error index");
			} catch (Exception e) {
				Assert.Fail ($"Unexpected exception: {e}");
			}
		}

		[Test]
		public void TestEmptyValue ()
		{
			const string text = " ";

			AssertParse (text, null, false, 1, 1);
		}

		[Test]
		public void TestMultipleParametersWithIdenticalNames ()
		{
			const string text1 = "inline;\n filename=\"Filename.doc\";\n filename*0*=UTF-8''UnicodeFile;\n filename*1*=name.doc";
			const string text2 = "inline;\n filename*0*=UTF-8''UnicodeFile;\n filename*1*=name.doc;\n filename=\"Filename.doc\"";
			const string text3 = "inline;\n filename*0*=UTF-8''UnicodeFile;\n filename=\"Filename.doc\";\n filename*1*=name.doc";
			var expected = new ContentDisposition ("inline");

			expected.Parameters.Add ("filename", "UnicodeFilename.doc");

			AssertParse (text1, expected);
			AssertParse (text2, expected);
			AssertParse (text3, expected);
		}

		[Test]
		public void TestNonExistentDispositionValueWithParameterValues ()
		{
			const string text = " ; filename=\"test.txt\"";
			const string filename = "test.txt";
			var expected = new ContentDisposition ("attachment");

			expected.Parameters.Add ("filename", filename);

			AssertParse (text, expected, true, 1, 1);
		}

		[Test]
		public void TestMistakenlyQuotedDispositionValue ()
		{
			const string text = "\"inline\"; filename=\"test.txt\"";
			const string filename = "test.txt";
			var expected = new ContentDisposition ("inline");

			expected.Parameters.Add ("filename", filename);

			AssertParse (text, expected, true, 0, 0);
		}

		[Test]
		public void TestMistakenlyQuotedEncodedParameterValues ()
		{
			const string text = "attachment;\n filename*0*=\"ISO-8859-2''%C8%50%50%20%2D%20%BE%E1%64%6F%73%74%20%6F%20%61%6B%63%65\";\n " +
				"filename*1*=\"%70%74%61%63%69%20%73%6D%6C%6F%75%76%79%20%31%32%2E%31%32%2E\";\n " +
				"filename*2*=\"%64%6F%63\"";
			const string filename = "ČPP - žádost o akceptaci smlouvy 12.12.doc";
			var expected = new ContentDisposition ("attachment");

			expected.Parameters.Add (Encoding.GetEncoding ("ISO-8859-2"), "filename", filename);

			AssertParse (text, expected);
		}

		[Test]
		public void TestFoldedQuotedFilenameParameterValue ()
		{
			const string text = "attachment; \r\n\tfilename=\"CR_A-EXCG-2020-0008 - Addition of UAT Email Domain in CMMP-GCN Connector\r\n\t.docx\"\r\n";
			const string filename = "CR_A-EXCG-2020-0008 - Addition of UAT Email Domain in CMMP-GCN Connector\t.docx";
			var expected = new ContentDisposition ("attachment");

			expected.Parameters.Add ("filename", filename);

			AssertParse (text, expected);
		}

		[Test]
		public void TestFoldedUnquotedFilenameParameterValue ()
		{
			const string text = " attachment; filename=Partnership Marketing Agreement\n\tForm - Mega Brands - Easter Toys - Week 11.pdf";
			const string filename = "Partnership Marketing Agreement\tForm - Mega Brands - Easter Toys - Week 11.pdf";
			var expected = new ContentDisposition ("attachment");

			expected.Parameters.Add ("filename", filename);

			AssertParse (text, expected);
		}

		[Test]
		public void TestInvalidDisposition ()
		{
			const string text = "\attachment";

			AssertParse (text, null, false, 0, 0);
		}

		[Test]
		public void TestInvalidDataAfterMDisposition ()
		{
			var expected = new ContentDisposition ("attachment");
			const string text = "attachment x";

			// TryParse will return false but will have a value to use
			AssertParse (text, expected, false, 11, 11);
		}

		[Test]
		public void TestChineseFilename ()
		{
			const string expected = " attachment;\n\tfilename*=gb18030''%B2%E2%CA%D4%CE%C4%B1%BE.txt\n";
			var disposition = new ContentDisposition (ContentDisposition.Attachment);
			disposition.Parameters.Add ("GB18030", "filename", "测试文本.txt");

			var format = FormatOptions.Default.Clone ();
			format.NewLineFormat = NewLineFormat.Unix;

			var encoded = disposition.Encode (format, Encoding.UTF8);
			Parameter param;

			Assert.That (encoded, Is.EqualTo (expected), "The encoded Chinese filename parameter does not match the expected value.");
			Assert.That (ContentDisposition.TryParse (encoded, out disposition), Is.True, "Failed to parse Content-Disposition");
			Assert.That (disposition.FileName, Is.EqualTo ("测试文本.txt"), "The decoded Chinese filename does not match.");
			Assert.That (disposition.Parameters.TryGetValue ("filename", out param), Is.True, "Failed to locate filename parameter.");
			Assert.That (param.Encoding.HeaderName.ToLowerInvariant (), Is.EqualTo ("gb18030"), "The filename encoding did not match.");
		}

		[Test]
		public void TestChineseFilename2047 ()
		{
			const string expected = " attachment; filename=\"=?gb18030?b?suLK1M7Esb4udHh0?=\"\n";
			var disposition = new ContentDisposition (ContentDisposition.Attachment);
			disposition.Parameters.Add ("GB18030", "filename", "测试文本.txt");

			var format = FormatOptions.Default.Clone ();
			format.ParameterEncodingMethod = ParameterEncodingMethod.Rfc2047;
			format.NewLineFormat = NewLineFormat.Unix;

			var encoded = disposition.Encode (format, Encoding.UTF8);
			Parameter param;

			Assert.That (encoded, Is.EqualTo (expected), "The encoded Chinese filename parameter does not match the expected value.");
			Assert.That (ContentDisposition.TryParse (encoded, out disposition), Is.True, "Failed to parse Content-Disposition");
			Assert.That (disposition.FileName, Is.EqualTo ("测试文本.txt"), "The decoded Chinese filename does not match.");
			Assert.That (disposition.Parameters.TryGetValue ("filename", out param), Is.True, "Failed to locate filename parameter.");
			Assert.That (param.Encoding.HeaderName.ToLowerInvariant (), Is.EqualTo ("gb18030"), "The filename encoding did not match.");
		}

		[Test]
		public void TestIssue239 ()
		{
			const string text = " attachment; size=1049971;\n\tfilename*=\"utf-8''SBD%20%C5%A0kodov%C3%A1k%2Ejpg\"";
			const string filename = "SBD Škodovák.jpg";
			var expected = new ContentDisposition ("attachment");

			expected.Parameters.Add ("size", "1049971");
			expected.Parameters.Add ("filename", filename);

			AssertParse (text, expected);
		}

		[Test]
		public void TestFormData ()
		{
			const string text = "form-data; filename=\"form.txt\"";
			var buffer = Encoding.ASCII.GetBytes (text);
			ContentDisposition disposition;

			Assert.That (ContentDisposition.TryParse (text, out disposition), Is.True, "Failed to parse Content-Disposition");
			Assert.That (disposition.Disposition, Is.EqualTo ("form-data"), "The disposition values do not match.");
			Assert.That (disposition.FileName, Is.EqualTo ("form.txt"), "The filename value does not match.");

			Assert.That (ContentDisposition.TryParse (buffer, 0, buffer.Length, out disposition), Is.True, "Failed to parse Content-Disposition");
			Assert.That (disposition.Disposition, Is.EqualTo ("form-data"), "The disposition values do not match.");
			Assert.That (disposition.FileName, Is.EqualTo ("form.txt"), "The filename value does not match.");

			Assert.That (ContentDisposition.TryParse (buffer, 0, out disposition), Is.True, "Failed to parse Content-Disposition");
			Assert.That (disposition.Disposition, Is.EqualTo ("form-data"), "The disposition values do not match.");
			Assert.That (disposition.FileName, Is.EqualTo ("form.txt"), "The filename value does not match.");

			Assert.That (ContentDisposition.TryParse (buffer, out disposition), Is.True, "Failed to parse Content-Disposition");
			Assert.That (disposition.Disposition, Is.EqualTo ("form-data"), "The disposition values do not match.");
			Assert.That (disposition.FileName, Is.EqualTo ("form.txt"), "The filename value does not match.");
		}

		[Test]
		public void TestDispositionParameters ()
		{
			const string expected = "Content-Disposition: attachment; filename=document.doc;\n" +
				"\tcreation-date=\"Sat, 04 Jan 1997 15:22:17 -0400\";\n" +
				"\tmodification-date=\"Thu, 04 Jan 2007 15:22:17 -0400\";\n" +
				"\tread-date=\"Wed, 04 Jan 2012 15:22:17 -0400\"; size=37001";
			var ctime = new DateTimeOffset (1997, 1, 4, 15, 22, 17, new TimeSpan (-4, 0, 0));
			var mtime = new DateTimeOffset (2007, 1, 4, 15, 22, 17, new TimeSpan (-4, 0, 0));
			var atime = new DateTimeOffset (2012, 1, 4, 15, 22, 17, new TimeSpan (-4, 0, 0));
			var disposition = new ContentDisposition ();
			var format = FormatOptions.Default.Clone ();
			const long size = 37001;
			Parameter param;
			string encoded;

			format.NewLineFormat = NewLineFormat.Unix;

			Assert.That (disposition.Disposition, Is.EqualTo (ContentDisposition.Attachment), "The disposition should be 'attachment'.");
			Assert.That (disposition.IsAttachment, Is.True, "IsAttachment should be true by default.");

			Assert.That (disposition.FileName, Is.Null, "The filename should default to null.");
			Assert.That (disposition.CreationDate, Is.Null, "The creation-date should default to null.");
			Assert.That (disposition.ModificationDate, Is.Null, "The modification-date should default to null.");
			Assert.That (disposition.ReadDate, Is.Null, "The read-date should default to null.");
			Assert.That (disposition.Size, Is.Null, "The size should default to null.");

			disposition.FileName = "document.doc";
			disposition.CreationDate = ctime;
			disposition.ModificationDate = mtime;
			disposition.ReadDate = atime;
			disposition.Size = size;

			encoded = disposition.ToString (format, Encoding.UTF8, true);

			Assert.That (encoded, Is.EqualTo (expected), "The encoded Content-Disposition does not match.");

			disposition = ContentDisposition.Parse (encoded.Substring ("Content-Disposition:".Length));

			Assert.That (disposition.FileName, Is.EqualTo ("document.doc"), "The filename parameter does not match.");
			Assert.That (disposition.CreationDate, Is.EqualTo (ctime), "The creation-date parameter does not match.");
			Assert.That (disposition.ModificationDate, Is.EqualTo (mtime), "The modification-date parameter does not match.");
			Assert.That (disposition.ReadDate, Is.EqualTo (atime), "The read-date parameter does not match.");
			Assert.That (disposition.Size, Is.EqualTo (size), "The size parameter does not match.");

			disposition.CreationDate = null;
			Assert.That (disposition.Parameters.TryGetValue ("creation-date", out param), Is.False, "The creation-date parameter should have been removed.");

			disposition.ModificationDate = null;
			Assert.That (disposition.Parameters.TryGetValue ("modification-date", out param), Is.False, "The modification-date parameter should have been removed.");

			disposition.ReadDate = null;
			Assert.That (disposition.Parameters.TryGetValue ("read-date", out param), Is.False, "The read-date parameter should have been removed.");

			disposition.FileName = null;
			Assert.That (disposition.Parameters.TryGetValue ("filename", out param), Is.False, "The filename parameter should have been removed.");

			disposition.Size = null;
			Assert.That (disposition.Parameters.TryGetValue ("size", out param), Is.False, "The size parameter should have been removed.");

			disposition.IsAttachment = false;
			Assert.That (disposition.Disposition, Is.EqualTo (ContentDisposition.Inline), "The disposition should be 'inline'.");
			Assert.That (disposition.IsAttachment, Is.False, "IsAttachment should be false.");
		}

		[Test]
		public void TestToString ()
		{
			const string expected = "Content-Disposition: attachment; filename=\"filename.txt\"; creation-date=\"Fri, 09 Sep 2022 07:41:23 -0400\"; modification-date=\"Fri, 09 Sep 2022 07:41:23 -0400\"; size=\"2048\"";
			var timestamp = new DateTimeOffset (2022, 9, 9, 7, 41, 23, new TimeSpan (-4, 0, 0));
			ContentDisposition disposition = new ContentDisposition {
				Disposition = ContentDisposition.Attachment,
				FileName = "filename.txt",
				CreationDate = timestamp,
				ModificationDate = timestamp,
				Size = 2048
			};
			var value = disposition.ToString ();

			Assert.That (value, Is.EqualTo (expected));

			value = disposition.ToString (Encoding.UTF8, false);

			Assert.That (value, Is.EqualTo (expected));
		}
	}
}

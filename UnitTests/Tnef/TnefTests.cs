//
// TnefTests.cs
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

using System.Globalization;

using MimeKit;
using MimeKit.Tnef;

namespace UnitTests.Tnef {
	[TestFixture]
	public class TnefTests
	{
		static string CorpusDirectory => Path.Combine (TestHelper.ProjectDir, "TestData", "tnef");

		[Test]
		public void TestArgumentExceptions ()
		{
			var tnef = new TnefPart ();

			Assert.Throws<ArgumentNullException> (() => tnef.Accept (null));
		}

		#region Reader-level corpus walk

		public static IEnumerable<TestCaseData> CorpusCases ()
		{
			foreach (var path in Directory.EnumerateFiles (CorpusDirectory, "*.tnef").OrderBy (Path.GetFileName, StringComparer.Ordinal))
				yield return new TestCaseData (Path.GetFileName (path)).SetArgDisplayNames (Path.GetFileName (path));
		}

		// The set of compliance violations the new reader is expected to report for each corpus file.
		static TnefComplianceViolation[] ExpectedViolations (string fileName)
		{
			switch (fileName) {
			case "garbage-at-end.tnef":
				return new[] { TnefComplianceViolation.TruncatedStream };
			case "panic.tnef":
				return new[] {
					TnefComplianceViolation.InvalidAttributeLevel,
					TnefComplianceViolation.UnknownAttribute,
					TnefComplianceViolation.TruncatedStream
				};
			default:
				return Array.Empty<TnefComplianceViolation> ();
			}
		}

		static bool ContainsProperties (TnefAttributeTag tag)
		{
			switch (tag) {
			case TnefAttributeTag.MapiProperties:
			case TnefAttributeTag.Attachment:
			case TnefAttributeTag.RecipientTable:
				return true;
			default:
				return false;
			}
		}

		static bool IsVariableLength (TnefPropertyType type)
		{
			switch (type) {
			case TnefPropertyType.Unicode:
			case TnefPropertyType.String8:
			case TnefPropertyType.Binary:
			case TnefPropertyType.Object:
				return true;
			default:
				return false;
			}
		}

		static int WalkPropertyValues (TnefPropertyReader properties)
		{
			int embedded = 0;

			if (properties.ValueCount == 0)
				return 0;

			do {
				if (properties.IsEmbeddedMessage) {
					embedded++;

					using (var child = properties.OpenEmbeddedMessage ())
						embedded += WalkReader (child);
				} else if (IsVariableLength (properties.PropertyType)) {
					using (var stream = properties.OpenValueStream ())
						stream.CopyTo (Stream.Null);
				} else {
					GC.KeepAlive (properties.Tag);
				}
			} while (properties.ReadNextValue ());

			return embedded;
		}

		static int WalkProperties (TnefPropertyReader properties, bool isTable)
		{
			int embedded = 0;

			if (isTable) {
				while (properties.ReadNextRow ()) {
					while (properties.ReadNextProperty ())
						embedded += WalkPropertyValues (properties);
				}
			} else {
				while (properties.ReadNextProperty ())
					embedded += WalkPropertyValues (properties);
			}

			return embedded;
		}

		static int WalkReader (TnefReader reader)
		{
			int embedded = 0;

			while (reader.Read ()) {
				if (ContainsProperties (reader.Tag)) {
					var properties = reader.GetPropertyReader ();

					embedded += WalkProperties (properties, reader.Tag == TnefAttributeTag.RecipientTable);
				} else {
					using (var stream = reader.OpenValueStream ())
						stream.CopyTo (Stream.Null);
				}
			}

			return embedded;
		}

		static async Task<int> WalkPropertyValuesAsync (TnefPropertyReader properties)
		{
			int embedded = 0;

			if (properties.ValueCount == 0)
				return 0;

			do {
				if (properties.IsEmbeddedMessage) {
					embedded++;

					using (var child = properties.OpenEmbeddedMessage ())
						embedded += await WalkReaderAsync (child).ConfigureAwait (false);
				} else if (IsVariableLength (properties.PropertyType)) {
					using (var stream = properties.OpenValueStream ())
						await stream.CopyToAsync (Stream.Null).ConfigureAwait (false);
				} else {
					GC.KeepAlive (properties.Tag);
				}
			} while (await properties.ReadNextValueAsync ().ConfigureAwait (false));

			return embedded;
		}

		static async Task<int> WalkPropertiesAsync (TnefPropertyReader properties, bool isTable)
		{
			int embedded = 0;

			if (isTable) {
				while (await properties.ReadNextRowAsync ().ConfigureAwait (false)) {
					while (await properties.ReadNextPropertyAsync ().ConfigureAwait (false))
						embedded += await WalkPropertyValuesAsync (properties).ConfigureAwait (false);
				}
			} else {
				while (await properties.ReadNextPropertyAsync ().ConfigureAwait (false))
					embedded += await WalkPropertyValuesAsync (properties).ConfigureAwait (false);
			}

			return embedded;
		}

		static async Task<int> WalkReaderAsync (TnefReader reader)
		{
			int embedded = 0;

			while (await reader.ReadAsync ().ConfigureAwait (false)) {
				if (ContainsProperties (reader.Tag)) {
					var properties = reader.GetPropertyReader ();

					embedded += await WalkPropertiesAsync (properties, reader.Tag == TnefAttributeTag.RecipientTable).ConfigureAwait (false);
				} else {
					using (var stream = reader.OpenValueStream ())
						await stream.CopyToAsync (Stream.Null).ConfigureAwait (false);
				}
			}

			return embedded;
		}

		[TestCaseSource (nameof (CorpusCases))]
		public void TestReaderWalksCorpusFile (string fileName)
		{
			var logger = new TestTnefComplianceLogger ();

			using (var stream = File.OpenRead (Path.Combine (CorpusDirectory, fileName)))
			using (var reader = new TnefReader (stream) { ComplianceLogger = logger })
				WalkReader (reader);

			var actual = logger.Issues.Select (issue => issue.Violation).Distinct ().ToArray ();

			Assert.That (actual, Is.EquivalentTo (ExpectedViolations (fileName)), fileName);
		}

		[TestCaseSource (nameof (CorpusCases))]
		public async Task TestReaderWalksCorpusFileAsync (string fileName)
		{
			var logger = new TestTnefComplianceLogger ();

			using (var stream = File.OpenRead (Path.Combine (CorpusDirectory, fileName)))
			using (var reader = new TnefReader (stream) { ComplianceLogger = logger })
				await WalkReaderAsync (reader).ConfigureAwait (false);

			var actual = logger.Issues.Select (issue => issue.Violation).Distinct ().ToArray ();

			Assert.That (actual, Is.EquivalentTo (ExpectedViolations (fileName)), fileName);
		}

		[Test]
		public void TestChristmasEmbeddedMessages ()
		{
			var logger = new TestTnefComplianceLogger ();
			int embedded;

			using (var stream = File.OpenRead (Path.Combine (CorpusDirectory, "christmas.tnef")))
			using (var reader = new TnefReader (stream) { ComplianceLogger = logger })
				embedded = WalkReader (reader);

			Assert.That (embedded, Is.EqualTo (2), "christmas.tnef should contain 2 embedded messages");
			Assert.That (logger.Issues, Is.Empty, "christmas.tnef should not log any compliance issues");
		}

		[Test]
		public async Task TestChristmasEmbeddedMessagesAsync ()
		{
			var logger = new TestTnefComplianceLogger ();
			int embedded;

			using (var stream = File.OpenRead (Path.Combine (CorpusDirectory, "christmas.tnef")))
			using (var reader = new TnefReader (stream) { ComplianceLogger = logger })
				embedded = await WalkReaderAsync (reader).ConfigureAwait (false);

			Assert.That (embedded, Is.EqualTo (2), "christmas.tnef should contain 2 embedded messages");
			Assert.That (logger.Issues, Is.Empty, "christmas.tnef should not log any compliance issues");
		}

		#endregion

		#region Conversion-level tests

		static MimeMessage ConvertToMessage (string path)
		{
			using var stream = File.OpenRead (path);

			return TnefConversionTestHelper.Convert (stream);
		}

		static MimeMessage ConvertToMessage (TnefPart tnef)
		{
			return TnefConversionTestHelper.Convert (tnef);
		}

		static byte[] ReadAllBytes (Stream stream, bool text)
		{
			using (var memory = new MemoryStream ()) {
				using (var filtered = new MimeKit.IO.FilteredStream (memory)) {
					if (text)
						filtered.Add (new MimeKit.IO.Filters.Dos2UnixFilter (true));
					stream.CopyTo (filtered, 4096);
					filtered.Flush ();

					return memory.ToArray ();
				}
			}
		}

		static void TestTnefParser (string baseFileName)
		{
			var path = Path.Combine (CorpusDirectory, baseFileName);
			using var message = ConvertToMessage (path + ".tnef");
			var tnefName = Path.GetFileName (path + ".tnef");
			var names = File.ReadAllLines (path + ".list");
			var attachments = message.BodyParts.ToList ();

			// Step 1: make sure we've extracted the body and all the attachments. The .list files name the bodies
			// body.txt, body.rtf and body.html; they are converted to TextParts without a file name.
			foreach (var name in names) {
				bool found = false;

				foreach (var part in attachments.OfType<MimePart> ()) {
					if (part is TextPart && string.IsNullOrEmpty (part.FileName)) {
						var basename = Path.GetFileNameWithoutExtension (name);
						var extension = Path.GetExtension (name);
						string subtype;

						switch (extension) {
						case ".html": subtype = "html"; break;
						case ".rtf": subtype = "rtf"; break;
						default: subtype = "plain"; break;
						}

						if (basename == "body" && part.ContentType.IsMimeType ("text", subtype)) {
							found = true;
							break;
						}
					} else if (part.FileName == name) {
						found = true;
						break;
					}
				}

				if (!found)
					Assert.Fail ($"Failed to locate attachment: {name}");
			}

			// Step 2: verify that the content of the extracted attachments matches up with the expected content
			byte[] expectedData, actualData;
			int untitled = 1;

			foreach (var part in attachments.OfType<MimePart> ()) {
				var isText = false;
				string fileName;

				if (part is TextPart text && string.IsNullOrEmpty (part.FileName)) {
					if (text.IsHtml)
						fileName = "message.html";
					else if (text.IsRichText)
						fileName = "message.rtf";
					else
						fileName = "message.txt";

					isText = true;
				} else if (part.FileName == "Untitled Attachment") {
					// special case for winmail.tnef and christmas.tnef
					fileName = string.Format (CultureInfo.InvariantCulture, "Untitled Attachment.{0}", untitled++);
				} else {
					var extension = Path.GetExtension (part.FileName);

					switch (extension) {
					case ".cfg":
					case ".dat":
					case ".htm":
					case ".ini":
					case ".src":
						isText = true;
						break;
					case "":
						isText = part.FileName == "AUTHORS" || part.FileName == "README";
						break;
					}

					fileName = part.FileName;
				}

				var file = Path.Combine (path, fileName);

				if (!File.Exists (file))
					continue;

				using (var stream = File.OpenRead (file))
					expectedData = ReadAllBytes (stream, isText);

				using (var stream = part.Content.Open ())
					actualData = ReadAllBytes (stream, isText);

				Assert.That (actualData.Length, Is.EqualTo (expectedData.Length), $"{tnefName}: {fileName} content length does not match");
				for (int i = 0; i < expectedData.Length; i++)
					Assert.That (actualData[i], Is.EqualTo (expectedData[i]), $"{tnefName}: {fileName} content differs at index {i}");
			}
		}

		[Test]
		public void TestAttachments ()
		{
			TestTnefParser ("attachments");
		}

		[Test]
		public void TestBody ()
		{
			TestTnefParser ("body");
		}

		[Test]
		public void TestChristmas ()
		{
			TestTnefParser ("christmas");
		}

		[Test]
		public void TestDataBeforeName ()
		{
			TestTnefParser ("data-before-name");
		}

		[Test]
		public void TestGarbageAtEnd ()
		{
			TestTnefParser ("garbage-at-end");
		}

		[Test]
		public void TestLongFileName ()
		{
			TestTnefParser ("long-filename");
		}

		[Test]
		public void TestMapiAttachDataObj ()
		{
			TestTnefParser ("MAPI_ATTACH_DATA_OBJ");
		}

		[Test]
		public void TestMapiObject ()
		{
			TestTnefParser ("MAPI_OBJECT");
		}

		[Test]
		public void TestMissingFileNames ()
		{
			TestTnefParser ("missing-filenames");
		}

		[Test]
		public void TestMultiNameProperty ()
		{
			TestTnefParser ("multi-name-property");
		}

		[Test]
		public void TestMultiValueAttribute ()
		{
			TestTnefParser ("multi-value-attribute");
		}

		[Test]
		public void TestOneFile ()
		{
			TestTnefParser ("one-file");
		}

		[Test]
		public void TestPanic ()
		{
			TestTnefParser ("panic");
		}

		[Test]
		public void TestRtf ()
		{
			TestTnefParser ("rtf");
		}

		[Test]
		public void TestTriples ()
		{
			TestTnefParser ("triples");
		}

		[Test]
		public void TestTwoFiles ()
		{
			TestTnefParser ("two-files");
		}

		[Test]
		public void TestUnicodeMapiAttrName ()
		{
			TestTnefParser ("unicode-mapi-attr-name");
		}

		[Test]
		public void TestUnicodeMapiAttr ()
		{
			TestTnefParser ("unicode-mapi-attr");
		}

		[Test]
		public void TestWinMail ()
		{
			TestTnefParser ("winmail");
		}

		[Test]
		public void TestExtractedCharset ()
		{
			const string expected = "<html>\r\n<head>\r\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=koi8-r\">\r\n<style type=\"text/css\" style=\"display:none;\"><!-- P {margin-top:0;margin-bottom:0;} --></style>\r\n</head>\r\n<body dir=\"ltr\">\r\n<div id=\"divtagdefaultwrapper\" style=\"font-size:12pt;color:#000000;font-family:Calibri,Helvetica,sans-serif;\" dir=\"ltr\">\r\n<p>шостий</p>\r\n<p><br>\r\n</p>\r\n<p>{EMAILSIGNATURE}</p>\r\n<p><br>\r\n</p>\r\n<div id=\"Signature\"><br>\r\n<font color=\"#888888\" face=\"Arial, Helvetica, Helvetica, Geneva, Sans-Serif\" style=\"font-size: 10pt;\"><br>\r\n<font color=\"#888888\" face=\"Arial, Helvetica, Helvetica, Geneva, Sans-Serif\" style=\"font-size: 12pt;\"><b>RR Test 1</b></font>\r\n</font>\r\n<p><font color=\"#888888\" face=\"Arial, Helvetica, Helvetica, Geneva, Sans-Serif\" style=\"font-size: 10pt;\">&nbsp;</font></p>\r\n</div>\r\n</div>\r\n</body>\r\n</html>\r\n";
			using var message = MimeMessage.Load (Path.Combine (CorpusDirectory, "ukr.eml"));
			var tnef = message.BodyParts.OfType<TnefPart> ().FirstOrDefault ();
			using var extracted = ConvertToMessage (tnef);

			Assert.That (extracted.Body, Is.InstanceOf<TextPart> ());

			var text = (TextPart) extracted.Body;

			Assert.That (text.IsHtml, Is.True);

			var html = text.Text;

			Assert.That (text.ContentType.Charset, Is.EqualTo ("koi8-r"));
			Assert.That (html, Is.EqualTo (expected.Replace ("\r\n", Environment.NewLine)));
		}

		[Test]
		public void TestRichTextEml ()
		{
			using var message = MimeMessage.Load (Path.Combine (CorpusDirectory, "rich-text.eml"));
			var tnef = message.BodyParts.OfType<TnefPart> ().FirstOrDefault ();
			var mtime = new DateTimeOffset (2018, 12, 15, 10, 17, 38, TimeSpan.Zero);
			using var extracted = ConvertToMessage (tnef);

			Assert.That (extracted.Subject, Is.Null, "Subject");
			Assert.That (extracted.Date, Is.EqualTo (DateTimeOffset.MinValue), "Date");
			Assert.That (extracted.MessageId, Is.EqualTo ("DM5PR21MB0828DA2B8C88048BC03EFFA6CFA20@DM5PR21MB0828.namprd21.prod.outlook.com"), "Message-Id");

			Assert.That (extracted.Body, Is.InstanceOf<Multipart> ());
			var multipart = (Multipart) extracted.Body;

			Assert.That (multipart.Count, Is.EqualTo (4));

			Assert.That (multipart[0], Is.InstanceOf<TextPart> ());
			Assert.That (multipart[1], Is.InstanceOf<MimePart> ());
			Assert.That (multipart[2], Is.InstanceOf<TnefPart> ());
			Assert.That (multipart[3], Is.InstanceOf<TnefPart> ());

			var rtf = (TextPart) multipart[0];
			Assert.That (rtf.ContentType.MimeType, Is.EqualTo ("text/rtf"), "MimeType");

			var kitten = (MimePart) multipart[1];
			Assert.That (kitten.ContentType.MimeType, Is.EqualTo ("application/octet-stream"), "MimeType");
			Assert.That (kitten.FileName, Is.EqualTo ("kitten-playing-with-a-christmas-tree.jpg"), "FileName");

			// The task and the appointment are embedded messages. Each has both a legacy attAttachData attribute and a
			// PidTagAttachDataObject property; they are a single attachment, which is kept as an application/ms-tnef part
			// because ConvertEmbeddedMessages is false by default.
			var task = (MimePart) multipart[2];
			Assert.That (task.ContentType.MimeType, Is.EqualTo ("application/ms-tnef"), "MimeType");
			Assert.That (task.ContentType.Name, Is.EqualTo ("Build a train table"), "Name");
			Assert.That (task.ContentDisposition.Disposition, Is.EqualTo ("attachment"), "Disposition");
			Assert.That (task.ContentDisposition.FileName, Is.EqualTo ("Untitled Attachment"), "FileName");
			Assert.That (task.ContentDisposition.ModificationDate, Is.EqualTo (mtime), "ModificationDate");
			Assert.That (task.ContentDisposition.Size, Is.EqualTo (9217), "Size");

			var appointment = (MimePart) multipart[3];
			Assert.That (appointment.ContentType.MimeType, Is.EqualTo ("application/ms-tnef"), "MimeType");
			Assert.That (appointment.ContentType.Name, Is.EqualTo ("Christmas Celebration!"), "Name");
			Assert.That (appointment.ContentDisposition.Disposition, Is.EqualTo ("attachment"), "Disposition");
			Assert.That (appointment.ContentDisposition.FileName, Is.EqualTo ("Untitled Attachment"), "FileName");
			Assert.That (appointment.ContentDisposition.ModificationDate, Is.EqualTo (mtime), "ModificationDate");
			Assert.That (appointment.ContentDisposition.Size, Is.EqualTo (387453), "Size");
		}

		#endregion
	}
}

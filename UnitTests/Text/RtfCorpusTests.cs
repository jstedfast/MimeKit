//
// RtfCorpusTests.cs
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

using MimeKit.Text;

namespace UnitTests.Text {
	// Runs every document in TestData/rtf/<source>/ (see the README.md in each source directory for
	// provenance and licensing) through the RTF converters and compares the results against the
	// expected output checked in next to each document (<name>.txt and <name>.html).
	//
	// To regenerate the expected output after an intentional behavior change, delete the stale
	// <name>.txt / <name>.html files and run the tests once; missing files are written out and the
	// test is reported as inconclusive so that the new output can be reviewed before committing.
	[TestFixture]
	public class RtfCorpusTests
	{
		static readonly string CorpusDir = Path.Combine (TestHelper.ProjectDir, "TestData", "rtf");

		static IEnumerable<string> CorpusFiles ()
		{
			return Directory.GetFiles (CorpusDir, "*.rtf", SearchOption.AllDirectories)
				.Select (path => Path.GetRelativePath (CorpusDir, path).Replace ('\\', '/'))
				.OrderBy (path => path, StringComparer.Ordinal);
		}

		static string Normalize (string text)
		{
			return text.Replace ("\r\n", "\n");
		}

		static void AssertExpectedOutput (string path, string actual)
		{
			if (!File.Exists (path)) {
				File.WriteAllText (path, actual, new UTF8Encoding (false));
				Assert.Inconclusive ($"Generated {Path.GetFileName (path)}; review it and re-run the tests.");
			}

			var expected = File.ReadAllText (path, Encoding.UTF8);

			Assert.That (Normalize (actual), Is.EqualTo (Normalize (expected)), path);
		}

		[TestCaseSource (nameof (CorpusFiles))]
		public void TestRtfToText (string name)
		{
			var path = Path.Combine (CorpusDir, name);
			var converter = new RtfToText ();
			string actual;

			// The Stream overload exercises the default ISO-8859-1 InputEncoding, which maps every RTF byte 1:1.
			using (var input = File.OpenRead (path)) {
				using (var output = new MemoryStream ()) {
					// Use an encoding without a preamble so that the output does not start with a BOM.
					converter.OutputEncoding = new UTF8Encoding (false);
					converter.Convert (input, output);
					actual = Encoding.UTF8.GetString (output.GetBuffer (), 0, (int) output.Length);
				}
			}

			AssertExpectedOutput (Path.ChangeExtension (path, ".txt"), actual);
		}

		[TestCaseSource (nameof (CorpusFiles))]
		public void TestRtfToHtml (string name)
		{
			var path = Path.Combine (CorpusDir, name);
			var converter = new RtfToHtml ();
			string actual;

			using (var reader = new StreamReader (path, Encoding.Latin1, false))
				actual = converter.Convert (reader.ReadToEnd ());

			AssertExpectedOutput (Path.ChangeExtension (path, ".html"), actual);
		}

		sealed class RecordingHandler : RtfContentHandler
		{
			public readonly StringBuilder Output = new StringBuilder ();

			public override void OnBegin (RtfInterpreter rtf)
			{
				Output.Append (rtf.FromHtml ? "[begin:html]" : "[begin]");
			}

			public override void OnText (RtfInterpreter rtf, char[] buffer, int index, int count)
			{
				if (rtf.FieldHyperlink != null)
					Output.Append ("[href=").Append (rtf.FieldHyperlink).Append (']');
				Output.Append (buffer, index, count);
			}

			public override void OnParagraph (RtfInterpreter rtf)
			{
				Output.Append ("[par]");
			}

			public override void OnLineBreak (RtfInterpreter rtf)
			{
				Output.Append ("[line]");
			}

			public override void OnCell (RtfInterpreter rtf)
			{
				Output.Append ("[cell]");
			}

			public override void OnRow (RtfInterpreter rtf)
			{
				Output.Append ("[row]");
			}

			public override void OnHtmlTag (RtfInterpreter rtf, char[] buffer, int index, int count)
			{
				Output.Append ("<<").Append (buffer, index, count).Append (">>");
			}
		}

		static string Interpret (string path)
		{
			var handler = new RecordingHandler ();

			using (var reader = new StreamReader (path, Encoding.Latin1, false)) {
				var interpreter = new RtfInterpreter (reader, handler, 4096, 4096, 4096) { ExtractHtml = true };

				while (interpreter.Step ())
					;
			}

			return handler.Output.ToString ();
		}

		static async Task<string> InterpretAsync (string path)
		{
			var handler = new RecordingHandler ();

			using (var reader = new StreamReader (path, Encoding.Latin1, false)) {
				var interpreter = new RtfInterpreter (reader, handler, 4096, 4096, 4096) { ExtractHtml = true };

				while (await interpreter.StepAsync ())
					;
			}

			return handler.Output.ToString ();
		}

		[TestCaseSource (nameof (CorpusFiles))]
		public async Task TestInterpreterSyncAsyncParity (string name)
		{
			// The async code path is a hand-maintained mirror of the sync one, so it must produce identical events.
			var path = Path.Combine (CorpusDir, name);
			var expected = Interpret (path);
			var actual = await InterpretAsync (path);

			Assert.That (actual, Is.EqualTo (expected));
		}
	}
}

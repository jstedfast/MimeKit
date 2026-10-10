//
// RtfConvertersTarget.cs
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

using MimeKit.Fuzzing.Helpers;
using MimeKit.Text;

namespace MimeKit.Fuzzing.Targets {
	// Converts the input with RtfToText and RtfToHtml, checking that:
	//
	// * the string, TextReader (one char at a time) and Stream overloads produce identical output;
	// * the output size is bounded by the input size;
	// * rendered HTML only uses the fixed vocabulary of elements and attributes RtfToHtml generates;
	// * attachment placeholders are reported in order and match CountObjectPlaceholders.
	sealed class RtfConvertersTarget : IFuzzTarget
	{
		public string Name => "rtf-converters";

		public int MaxLength => 1024 * 1024;

		public void Run (ReadOnlySpan<byte> data)
		{
			var input = data.ToArray ();
			var text = Fuzz.Latin1.GetString (input);
			uint selector = Fuzz.Selector (data);
			bool smallLimits = (selector & 1) != 0;
			int limit = smallLimits ? 8 : 4096;

			CheckText (input, text, smallLimits ? 16 : 4096, limit);
			CheckHtml (input, text, smallLimits ? 16 : 4096, limit, (selector & 2) != 0, (selector & 4) != 0);
		}

		static void CheckText (byte[] input, string text, int depth, int limit)
		{
			int placeholders = 0;
			var converter = new RtfToText {
				MaxGroupDepth = depth,
				MaxFontTableEntries = limit,
				MaxColorTableEntries = limit,
				InputEncoding = Fuzz.Latin1,
				OutputEncoding = Fuzz.Utf8,
				DetectEncodingFromByteOrderMark = false,
			};

			converter.AttachmentPlaceholderCallback = (index, writer) => {
				Fuzz.Check (index == placeholders, $"RtfToText reported placeholder {index}, expected {placeholders}");
				placeholders++;
				writer.Write ("[a]");
			};

			var output = converter.Convert (text);
			int count = placeholders;

			placeholders = 0;
			var oneChar = Convert (converter, new OneCharTextReader (text));
			Fuzz.Equal (output, oneChar, "RtfToText string vs one char at a time");
			Fuzz.Check (placeholders == count, "RtfToText reported a different number of placeholders");

			placeholders = 0;
			var stream = Convert (converter, input);
			Fuzz.Equal (output, stream, "RtfToText string vs Stream");

			Fuzz.Check (output.Length <= 4L * input.Length + 256, $"RtfToText produced {output.Length} chars from {input.Length} bytes");
		}

		static void CheckHtml (byte[] input, string text, int depth, int limit, bool extract, bool fragment)
		{
			int placeholders = 0;
			var converter = new RtfToHtml {
				MaxGroupDepth = depth,
				MaxFontTableEntries = limit,
				MaxColorTableEntries = limit,
				MaxElementDepth = limit,
				ExtractEncapsulatedHtml = extract,
				OutputHtmlFragment = fragment,
				InputEncoding = Fuzz.Latin1,
				OutputEncoding = Fuzz.Utf8,
				DetectEncodingFromByteOrderMark = false,
			};

			converter.AttachmentPlaceholderCallback = (index, writer) => {
				Fuzz.Check (index == placeholders, $"RtfToHtml reported placeholder {index}, expected {placeholders}");
				placeholders++;
				writer.WriteEmptyElementTag ("img");
				writer.WriteAttribute ("src", "cid:" + index);
			};

			var output = converter.Convert (text);
			int count = placeholders;

			placeholders = 0;
			var oneChar = Convert (converter, new OneCharTextReader (text));
			Fuzz.Equal (output, oneChar, "RtfToHtml string vs one char at a time");
			Fuzz.Check (placeholders == count, "RtfToHtml reported a different number of placeholders");

			placeholders = 0;
			var stream = Convert (converter, input);
			Fuzz.Equal (output, stream, "RtfToHtml string vs Stream");

			Fuzz.Check (output.Length <= 64L * input.Length + 4096, $"RtfToHtml produced {output.Length} chars from {input.Length} bytes");

			bool extracts = converter.ExtractsEncapsulatedHtml (new StringReader (text), CancellationToken.None);
			int expected = converter.CountObjectPlaceholders (new StringReader (text), CancellationToken.None);

			Fuzz.Check (extracts == (expected == -1), $"ExtractsEncapsulatedHtml is {extracts} but CountObjectPlaceholders returned {expected}");
			Fuzz.Check (!extracts || extract, "ExtractsEncapsulatedHtml is true even though ExtractEncapsulatedHtml is false");

			if (!extracts) {
				Fuzz.Check (count == expected, $"RtfToHtml reported {count} placeholders but CountObjectPlaceholders returned {expected}");
				GeneratedHtmlChecker.Check (output, fragment);
			}
		}

		static string Convert (TextConverter converter, TextReader reader)
		{
			using var writer = new StringWriter ();

			converter.Convert (reader, writer);

			return writer.ToString ();
		}

		static string Convert (TextConverter converter, byte[] input)
		{
			using var output = new MemoryStream ();

			converter.Convert (new MemoryStream (input, false), output);

			return Fuzz.Utf8.GetString (output.ToArray ());
		}
	}
}

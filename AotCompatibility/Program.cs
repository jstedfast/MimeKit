//
// Program.cs
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
using MimeKit.Cryptography;
using MimeKit.Encodings;
using MimeKit.Tnef;
using MimeKit.Text;

namespace AotCompatibility {
    class Program
    {
        static int Main (string[] args)
        {
            var location = System.AppContext.BaseDirectory;
            var dir = Path.GetDirectoryName (location);

            while (Path.GetFileName (dir) != "AotCompatibility")
                dir = Path.GetFullPath (Path.Combine (dir!, ".."));

            dir = Path.Combine (dir!, "..", "UnitTests", "TestData");

            try {
                Encoding.RegisterProvider (CodePagesEncodingProvider.Instance);
                CryptographyModule.Initialize ();

                var htmlToText = new HtmlToText ();
                var plainText = htmlToText.Convert ("<h1>Title</h1><ul><li>one<li>two</ul>");
                var expectedText = "# Title" + Environment.NewLine + Environment.NewLine +
                    "- one" + Environment.NewLine + "- two";

                if (plainText != expectedText)
                    throw new InvalidOperationException ("Unexpected HTML to plain text conversion.");

                var enrichedToHtml = new EnrichedToHtml { OutputHtmlFragment = true };
                var enrichedHtml = enrichedToHtml.Convert ("<bold><color><param>red</param>Hello</color></bold>");

                if (enrichedHtml != "<div style=\"white-space: pre-wrap;\"><div><span style=\"font-weight: bold; color: red;\">Hello</span></div></div>")
                    throw new InvalidOperationException ("Unexpected enriched text to HTML conversion.");

                const string addresses = "Friends: Example <user@example.com>, \u00e9 <other@example.com>;";
                var fromString = InternetAddressList.Parse (addresses);
                var fromBytes = InternetAddressList.Parse (Encoding.UTF8.GetBytes (addresses));

                if (fromString.ToString () != fromBytes.ToString ())
                    throw new InvalidOperationException ("String and byte address parsing should agree.");

                using (var encoded = new MemoryStream ()) {
                    using var source = new MemoryStream (Encoding.ASCII.GetBytes ("123456789"));
                    using var writer = new YEncWriter (encoded, leaveOpen: true);
                    var header = new YEncHeader ("test.bin", 9);

                    if (writer.Write (header, source) != 0xcbf43926)
                        throw new InvalidOperationException ("Unexpected yEnc checksum.");

                    source.Position = 0;
                    writer.WriteAsync (header, source).GetAwaiter ().GetResult ();
                    encoded.Position = 0;
                    using var reader = new YEncReader (encoded, leaveOpen: true);
                    if (!reader.ReadNext ())
                        throw new InvalidOperationException ("Missing yEnc header.");
                    reader.DecodeTo (Stream.Null);
                    if (!reader.ReadNextAsync ().GetAwaiter ().GetResult ())
                        throw new InvalidOperationException ("Missing async yEnc header.");
                    reader.DecodeToAsync (Stream.Null).GetAwaiter ().GetResult ();
                    if (reader.Checksum != 0xcbf43926 || reader.ReadNext ())
                        throw new InvalidOperationException ("Unexpected yEnc result.");
                }

                var path = Path.Combine (dir, "smime", "thunderbird-signed.txt");
                var message = MimeMessage.Load (path);

                path = Path.Combine (dir, "tnef", "attachments.tnef");

                using (var stream = File.OpenRead (path)) {
                    using var tnef = TnefMessage.Load (stream);
                    using var result = tnef.ConvertToMime ();

                    if (result.Message.Attachments.Count () == 0)
                        throw new InvalidOperationException ("Expected the TNEF message to contain attachments.");
                }

                return 0;
            } catch (Exception ex) {
                Console.WriteLine (ex);
                return -1;
            }
        }
    }
}
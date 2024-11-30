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

                var path = Path.Combine (dir, "smime", "thunderbird-signed.txt");
                var message = MimeMessage.Load (path);

                return 0;
            } catch (Exception ex) {
                Console.WriteLine (ex);
                return -1;
            }
        }
    }
}
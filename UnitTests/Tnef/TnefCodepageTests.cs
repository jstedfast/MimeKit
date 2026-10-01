//
// TnefCodepageTests.cs
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

using MimeKit.Tnef;

namespace UnitTests.Tnef {
	[TestFixture]
	public class TnefCodepageTests
	{
		static readonly string DataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "tnef");

		// A codepage that is not supported even when the CodePagesEncodingProvider has been registered.
		const int UnsupportedCodepage = 65123;

		[Test]
		public void TestUnsupportedDefaultMessageCodepage ()
		{
			// Constructing a TnefReader with a codepage that the host cannot provide must degrade
			// gracefully rather than throwing. On Linux, *most* Windows codepages are unavailable
			// unless the consuming application has registered the CodePagesEncodingProvider.
			using (var stream = File.OpenRead (Path.Combine (DataDir, "winmail.tnef"))) {
				using (var reader = new TnefReader (stream, UnsupportedCodepage, TnefComplianceMode.Loose)) {
					Assert.That (reader.MessageCodepage, Is.Not.EqualTo (UnsupportedCodepage), "MessageCodepage");

					while (reader.ReadNextAttribute ())
						;
				}
			}
		}

		[Test]
		public void TestUnsupportedOemCodepageIsRecoverableInLooseMode ()
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (UnsupportedCodepage);
			builder.WriteMessageClass ("IPM.Note");

			using (var stream = builder.ToStream ()) {
				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					while (reader.ReadNextAttribute ())
						;

					Assert.That (reader.ComplianceStatus & TnefComplianceStatus.InvalidMessageCodepage, Is.EqualTo (TnefComplianceStatus.InvalidMessageCodepage), "ComplianceStatus");
					Assert.That (reader.MessageCodepage, Is.Not.EqualTo (UnsupportedCodepage), "MessageCodepage");
				}
			}
		}

		[Test]
		public void TestUnsupportedOemCodepageThrowsInStrictMode ()
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (UnsupportedCodepage);

			using (var stream = builder.ToStream ()) {
				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Strict)) {
					Assert.Throws<TnefException> (() => {
						while (reader.ReadNextAttribute ())
							;
					});
				}
			}
		}

		[Test]
		public void TestMessageCodepageIsAlwaysSupported ()
		{
			// Whatever MessageCodepage ends up reporting, Encoding.GetEncoding() must be able to
			// resolve it; TnefPart relies on this when decoding PidTagBody and PidTagHtml.
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (UnsupportedCodepage);

			using (var stream = builder.ToStream ()) {
				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					while (reader.ReadNextAttribute ())
						;

					Assert.DoesNotThrow (() => System.Text.Encoding.GetEncoding (reader.MessageCodepage));
				}
			}
		}
	}
}

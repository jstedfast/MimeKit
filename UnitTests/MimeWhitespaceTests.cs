//
// MimeWhitespaceTests.cs
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

using MimeKit;
using MimeKit.Cryptography;
using MimeKit.Utils;

namespace UnitTests {
	[TestFixture]
	public class MimeWhitespaceTests
	{
		[TestCase ("\u00a0")]
		[TestCase ("\u2008")]
		[TestCase ("\u3000")]
		[TestCase ("\v")]
		[TestCase ("\f")]
		public void TestHeaderPreservesNonLinearWhitespace (string padding)
		{
			var value = padding + "text" + padding;
			var header = new Header ("Subject", value);

			Assert.That (Header.Unfold (" \t" + value + "\r\n"), Is.EqualTo (value), "Unfold");
			Assert.That (header.Value, Is.EqualTo (value), "SetValue");

			var encoded = Convert.ToBase64String (Encoding.UTF8.GetBytes (value));
			header.SetRawValue (Encoding.ASCII.GetBytes (" =?utf-8?b?" + encoded + "?=\r\n"));
			Assert.That (header.Value, Is.EqualTo (value), "Decoded Value");
			Assert.That (header.GetValue (Encoding.UTF8), Is.EqualTo (value), "GetValue");

			using var part = new MimePart ();
			part.ContentDescription = value;
			part.ContentMd5 = value;
			Assert.That (part.ContentDescription, Is.EqualTo (value), "Description");
			Assert.That (part.ContentMd5, Is.EqualTo (value), "MD5");

			part.Headers[HeaderId.ContentDescription] = value;
			part.Headers[HeaderId.ContentMd5] = value;
			Assert.That (part.ContentDescription, Is.EqualTo (value), "Parsed description");
			Assert.That (part.ContentMd5, Is.EqualTo (value), "Parsed MD5");
		}

		[TestCase ("\u00a0")]
		[TestCase ("\u2008")]
		[TestCase ("\u3000")]
		public void TestDisplayNamesPreserveUnicodeWhitespace (string padding)
		{
			var name = padding + "Name" + padding;
			var mailbox = new MailboxAddress (name, "a@example.com");
			Assert.That (mailbox.Name, Is.EqualTo (name), "Constructor");
			mailbox.Name = " \t" + name + " \t";
			Assert.That (mailbox.Name, Is.EqualTo (name), "Setter");

			foreach (var text in new[] { "\"" + name + "\" <a@example.com>", "a@example.com (" + name + ")" }) {
				Assert.That (MailboxAddress.Parse (text).Name, Is.EqualTo (name), "String parser");
				Assert.That (MailboxAddress.Parse (Encoding.UTF8.GetBytes (text)).Name, Is.EqualTo (name), "Byte parser");
			}
		}

		[Test]
		public void TestInternationalDisplayNameFolding ([Values (false, true)] bool group)
		{
			var name = new string ('A', 30) + "\u00a0" + new string ('B', 30) + "\u2008" + new string ('C', 30);
			InternetAddress address = group ? new GroupAddress (name) : new MailboxAddress (name, "a@example.com");
			var options = FormatOptions.Default.Clone ();
			options.International = true;

			var encoded = address.ToString (options, true);
			Assert.That (InternetAddress.Parse (encoded).Name, Is.EqualTo (name));
		}

		static async Task AssertParameterRoundTrip (bool doAsync)
		{
			using var multipart = new Multipart ("mixed") { Boundary = " boundary" };
			using var stream = new MemoryStream ();
			var part = new MimePart { FileName = " \u00a0report.txt\u3000 " };
			multipart.Add (part);

			Assert.That (multipart.Boundary, Is.EqualTo (" boundary"), "Boundary setter");
			Assert.That (part.FileName, Is.EqualTo (part.ContentDisposition.FileName), "Filename getter");
			part.ContentDisposition = null;
			Assert.That (part.FileName, Is.EqualTo (part.ContentType.Name), "Filename fallback");

			if (doAsync)
				await multipart.WriteToAsync (stream);
			else
				multipart.WriteTo (stream);
			stream.Position = 0;

			using var entity = doAsync ? await MimeEntity.LoadAsync (stream) : MimeEntity.Load (stream);
			var parsed = (Multipart) entity;
			Assert.That (parsed.Boundary, Is.EqualTo (" boundary"), "Parsed boundary");
			Assert.That (parsed.Count, Is.EqualTo (1), "Child count");
			Assert.That (((MimePart) parsed[0]).FileName, Is.EqualTo (part.FileName), "Parsed filename");
		}

		[Test]
		public void TestParameterRoundTrip ()
		{
			AssertParameterRoundTrip (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public Task TestParameterRoundTripAsync ()
		{
			return AssertParameterRoundTrip (true);
		}

		[TestCase ("\u00a0")]
		[TestCase ("\u2008")]
		[TestCase ("\u3000")]
		[TestCase ("\v")]
		[TestCase ("\f")]
		public void TestTokensDoNotIgnoreNonLinearWhitespace (string padding)
		{
			Assert.That (MimeUtils.TryParse (padding + "base64", out ContentEncoding encoding), Is.False, "Leading CTE");
			Assert.That (encoding, Is.EqualTo (ContentEncoding.Unspecified));
			Assert.That (MimeUtils.TryParse ("base64" + padding + "junk", out encoding), Is.False, "CTE delimiter");
			Assert.That (MimeUtils.TryParse (" \tbase64\r\n", out encoding), Is.True, "Valid CTE");

			using var part = new TextPart ("plain");
			part.ContentType.Parameters["format"] = padding + "flowed" + padding;
			Assert.That (part.IsFlowed, Is.False, "IsFlowed");
			Assert.That (part.Format, Is.EqualTo (MimeKit.Text.TextFormat.Plain), "Format");

			Assert.That (ContentDisposition.TryParse ("\"" + padding + "inline" + padding + "\"", out var disposition), Is.True);
			Assert.That (disposition.Disposition, Is.EqualTo (ContentDisposition.Attachment), "Invalid disposition fallback");

			using var report = new MultipartReport ("delivery-status");
			report.ReportType = padding + "delivery-status" + padding;
			Assert.That (report.ReportType, Is.EqualTo (padding + "delivery-status" + padding));

			using var message = new MimeMessage ();
			message.Headers.Add (HeaderId.Importance, padding + "high" + padding);
			message.Headers.Add (HeaderId.Priority, padding + "urgent" + padding);
			Assert.That (message.Importance, Is.EqualTo (MessageImportance.Normal));
			Assert.That (message.Priority, Is.EqualTo (MessagePriority.Normal));
		}

		[TestCase ("\v")]
		[TestCase ("\f")]
		public void TestNumericTokensRejectNonLinearWhitespace (string padding)
		{
			using var partial = new MessagePartial ("id", 1, 2);
			partial.ContentType.Parameters["number"] = padding + "1" + padding;
			partial.ContentType.Parameters["total"] = padding + "2" + padding;
			Assert.That (partial.Number, Is.Null);
			Assert.That (partial.Total, Is.Null);

			using var part = new MimePart ();
			part.Headers.Add (HeaderId.ContentDuration, padding + "5" + padding);
			Assert.That (part.ContentDuration, Is.Null);
		}

		[Test]
		public void TestAsciiWhitespacePadding ()
		{
			using var partial = new MessagePartial ("id", 1, 2);
			partial.ContentType.Parameters["number"] = " \t1\r\n ";
			partial.ContentType.Parameters["total"] = " \t2\r\n ";
			Assert.That (partial.Number, Is.EqualTo (1));
			Assert.That (partial.Total, Is.EqualTo (2));

			using var part = new TextPart ("plain");
			part.Headers.Add (HeaderId.ContentDuration, " \t5\r\n ");
			part.ContentType.Parameters["format"] = " \tflowed ";
			Assert.That (part.ContentDuration, Is.EqualTo (5));
			Assert.That (part.IsFlowed, Is.True);
		}

		[TestCase ("\u00a0text\u00a0")]
		[TestCase ("\u00a0text\u00a0\\ ")]
		public void TestAuthenticationCommentPreservesUnicodeWhitespace (string comment)
		{
			var results = AuthenticationResults.Parse (Encoding.UTF8.GetBytes ("example.com; spf=pass (" + comment + ")"));
			Assert.That (results.Results[0].ResultComment, Is.EqualTo ("\u00a0text\u00a0"));
		}

		[Test]
		public void TestContentBaseDoesNotTrimUnicodeWhitespace ()
		{
			using var part = new MimePart ();
			part.Headers.Add (HeaderId.ContentBase, "\u00a0https://example.com/\u00a0");
			Assert.That (part.ContentBase, Is.Null);
		}

		[Test]
		public void TestSignatureProtocolRejectsUnicodeWhitespace ()
		{
			using var ctx = new TemporarySecureMimeContext ();
			using var signed = new MultipartSigned ();
			signed.ContentType.Parameters["protocol"] = "\u00a0application/pkcs7-signature\u00a0";
			Assert.Throws<NotSupportedException> (() => signed.Verify (ctx));
		}

		[Test]
		public void TestSignatureProtocolRejectsUnicodeWhitespaceAsync ()
		{
			using var ctx = new TemporarySecureMimeContext ();
			using var signed = new MultipartSigned ();
			signed.ContentType.Parameters["protocol"] = "\u00a0application/pkcs7-signature\u00a0";
			Assert.ThrowsAsync<NotSupportedException> (() => signed.VerifyAsync (ctx));
		}
	}
}

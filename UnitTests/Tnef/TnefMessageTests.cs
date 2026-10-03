//
// TnefMessageTests.cs
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
using System.Globalization;

using MimeKit.IO;
using MimeKit.IO.Filters;
using MimeKit.Tnef;

namespace UnitTests.Tnef {
	[TestFixture]
	public class TnefMessageTests
	{
		static readonly string CorpusDirectory = Path.Combine (TestHelper.ProjectDir, "TestData", "tnef");
		static readonly Guid IID_IMessage = new Guid ("00020307-0000-0000-C000-000000000046");
		static readonly byte[] OneOffProviderUid = { 0x81, 0x2B, 0x1F, 0xA4, 0xBE, 0xA3, 0x10, 0x19, 0x9D, 0x6E, 0x00, 0xDD, 0x01, 0x0F, 0x54, 0x02 };

		#region Helpers

		static async Task<TnefMessage> LoadAsync (Stream stream, bool async, TnefOptions options = null, TestTnefComplianceLogger logger = null)
		{
			using (var reader = new TnefReader (stream, options) { ComplianceLogger = logger })
				return async ? await TnefMessage.LoadAsync (reader) : TnefMessage.Load (reader);
		}

		static Task<TnefMessage> LoadAsync (TnefBuilder builder, bool async, TnefOptions options = null, TestTnefComplianceLogger logger = null)
		{
			return LoadAsync (builder.ToStream (), async, options, logger);
		}

		static Task<TnefMessage> LoadEmbeddedMessageAsync (TnefAttachment attachment, bool async)
		{
			return async ? attachment.LoadEmbeddedMessageAsync () : Task.FromResult (attachment.LoadEmbeddedMessage ());
		}

		static IEnumerable<TnefComplianceViolation> Violations (TestTnefComplianceLogger logger)
		{
			return logger.Issues.Select (issue => issue.Violation);
		}

		static byte[] ReadAll (Stream stream)
		{
			using (var memory = new MemoryStream ()) {
				stream.CopyTo (memory);
				return memory.ToArray ();
			}
		}

		static byte[] ReadAll (Stream stream, bool text)
		{
			using (var memory = new MemoryStream ()) {
				using (var filtered = new FilteredStream (memory)) {
					if (text)
						filtered.Add (new Dos2UnixFilter (true));
					stream.CopyTo (filtered, 4096);
					filtered.Flush ();

					return memory.ToArray ();
				}
			}
		}

		static byte[] Concat (params byte[][] arrays)
		{
			var stream = new MemoryStream ();

			foreach (var array in arrays)
				stream.Write (array, 0, array.Length);

			return stream.ToArray ();
		}

		static byte[] Int16Payload (int value)
		{
			return new byte[] { (byte) (value & 0xFF), (byte) ((value >> 8) & 0xFF) };
		}

		// An 8-bit, NUL-terminated legacy string attribute value.
		static byte[] StringPayload (string value)
		{
			return Encoding.ASCII.GetBytes (value + "\0");
		}

		static byte[] DatePayload (DateTime value)
		{
			return Concat (
				Int16Payload (value.Year), Int16Payload (value.Month), Int16Payload (value.Day),
				Int16Payload (value.Hour), Int16Payload (value.Minute), Int16Payload (value.Second),
				Int16Payload ((int) value.DayOfWeek));
		}

		// The attFrom value: a one-off TRP structure followed by a null TRP ([MS-OXTNEF] section 2.1.3.3.1).
		static byte[] FromPayload (string name, string address)
		{
			var nameBytes = StringPayload (name);
			var addressBytes = StringPayload (address);

			return Concat (
				Int16Payload (0x0004), Int16Payload (8 + nameBytes.Length + addressBytes.Length),
				Int16Payload (nameBytes.Length), Int16Payload (addressBytes.Length),
				nameBytes, addressBytes, new byte[8]);
		}

		static byte[] OwnerPayload (string name, string address)
		{
			var nameBytes = StringPayload (name);
			var addressBytes = StringPayload (address);

			return Concat (Int16Payload (nameBytes.Length), nameBytes, Int16Payload (addressBytes.Length), addressBytes);
		}

		static byte[] RenderDataPayload (int type = 1, int position = -1, int flags = 0)
		{
			return Concat (Int16Payload (type), TnefBuilder.Int32Payload (position), Int16Payload (0), Int16Payload (0), TnefBuilder.Int32Payload (flags));
		}

		static byte[] OneOffEntryId (string name, string addressType, string address)
		{
			return Concat (new byte[4], OneOffProviderUid, new byte[] { 0, 0, 0, 0x80 },
				Encoding.Unicode.GetBytes (name + "\0"), Encoding.Unicode.GetBytes (addressType + "\0"), Encoding.Unicode.GetBytes (address + "\0"));
		}

		static TnefBuilder CreateAttachment (TnefBuilder builder, string fileName, byte[] content)
		{
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData, RenderDataPayload ());
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachTitle, StringPayload (fileName));
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachData, content);

			return builder;
		}

		static byte[] CreateEmbeddedMessage (string subject, byte[] nested = null)
		{
			var builder = new TnefBuilder ();

			builder.WriteTnefVersion ();
			builder.WriteMessageClass ("IPM.Note");
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Subject, StringPayload (subject));

			if (nested != null)
				WriteEmbeddedAttachment (builder, nested);

			return builder.ToArray ();
		}

		static void WriteEmbeddedAttachment (TnefBuilder builder, byte[] message)
		{
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteBinaryProperty (TnefPropertyTag.AttachDataObj, Concat (IID_IMessage.ToByteArray (), message));
			properties.WriteInt32Property (TnefPropertyTag.AttachMethod, (int) TnefAttachMethod.EmbeddedMessage);

			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData, RenderDataPayload ());
			builder.WriteMapiProperties (TnefAttributeLevel.Attachment, properties);
		}

		#endregion

		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentNullException> (() => TnefMessage.Load ((Stream) null));
			Assert.Throws<ArgumentNullException> (() => TnefMessage.Load ((TnefReader) null));
			Assert.ThrowsAsync<ArgumentNullException> (() => TnefMessage.LoadAsync ((Stream) null));
			Assert.ThrowsAsync<ArgumentNullException> (() => TnefMessage.LoadAsync ((TnefReader) null));
		}

		#region Corpus

		public static IEnumerable<TestCaseData> CorpusCases ()
		{
			foreach (var path in Directory.EnumerateFiles (CorpusDirectory, "*.tnef").OrderBy (Path.GetFileName, StringComparer.Ordinal))
				yield return new TestCaseData (Path.GetFileName (path)).SetArgDisplayNames (Path.GetFileName (path));
		}

		static bool IsTextFile (string fileName)
		{
			switch (Path.GetExtension (fileName)) {
			case ".cfg": case ".dat": case ".htm": case ".ini": case ".src": case ".html": case ".rtf": case ".txt":
				return true;
			case "":
				return fileName == "AUTHORS" || fileName == "README";
			default:
				return false;
			}
		}

		static void AssertContent (string directory, string fileName, Stream actual, string description)
		{
			var path = Path.Combine (directory, fileName);

			if (!File.Exists (path))
				return;

			bool text = IsTextFile (fileName);
			byte[] expected;

			using (var stream = File.OpenRead (path))
				expected = ReadAll (stream, text);

			var bytes = ReadAll (actual, text);

			Assert.That (bytes, Is.EqualTo (expected), description);
		}

		static async Task<int> CountEmbeddedMessagesAsync (TnefMessage message, bool async)
		{
			int count = 0;

			foreach (var attachment in message.Attachments.Where (a => a.IsEmbeddedMessage)) {
				using (var embedded = await LoadEmbeddedMessageAsync (attachment, async)) {
					Assert.That (embedded.MessageClass, Is.Not.Null);
					count += 1 + await CountEmbeddedMessagesAsync (embedded, async);
				}
			}

			return count;
		}

		async Task RunTestCorpusAsync (string fileName, bool async)
		{
			var baseName = Path.GetFileNameWithoutExtension (fileName);
			var directory = Path.Combine (CorpusDirectory, baseName);
			var names = File.ReadAllLines (Path.Combine (CorpusDirectory, baseName + ".list"));
			var logger = new TestTnefComplianceLogger ();

			using (var stream = File.OpenRead (Path.Combine (CorpusDirectory, fileName))) {
				using (var message = await LoadAsync (stream, async, null, logger)) {
					// Every named attachment in the expectation list must be present.
					foreach (var name in names) {
						if (name.StartsWith ("body.", StringComparison.Ordinal) || name == "Untitled Attachment")
							continue;

						Assert.That (message.Attachments.Any (a => a.FileName == name), Is.True, $"{fileName}: missing attachment {name}");
					}

					// The bodies must match the expected (decoded) content.
					if (names.Contains ("body.rtf")) {
						Assert.That (message.RtfBody, Is.Not.Null, $"{fileName}: RtfBody");
						using (var content = message.RtfBody.OpenDecodedRead ())
							AssertContent (directory, "message.rtf", content, $"{fileName}: message.rtf");
					}

					if (names.Contains ("body.html")) {
						Assert.That (message.HtmlBody, Is.Not.Null, $"{fileName}: HtmlBody");
						using (var content = message.HtmlBody.OpenRead ())
							AssertContent (directory, "message.html", content, $"{fileName}: message.html");
					}

					int embedded = message.Attachments.Count (a => a.IsEmbeddedMessage);

					Assert.That (embedded, Is.EqualTo (fileName == "christmas.tnef" ? 2 : 0), $"{fileName}: embedded messages");

					// Each attachment without a file name is named "Untitled Attachment" and its expected content is saved as
					// "Untitled Attachment.N".
					int untitled = 1;

					// The attachment content must match the expected content.
					foreach (var attachment in message.Attachments) {
						if (attachment.IsEmbeddedMessage || string.IsNullOrEmpty (attachment.FileName))
							continue;

						var expectedName = attachment.FileName;

						if (expectedName == "Untitled Attachment")
							expectedName = string.Format (CultureInfo.InvariantCulture, "Untitled Attachment.{0}", untitled++);

						using (var content = attachment.OpenRead ())
							AssertContent (directory, expectedName, content, $"{fileName}: {attachment.FileName}");
					}

					await CountEmbeddedMessagesAsync (message, async);
				}
			}
		}

		[TestCaseSource (nameof (CorpusCases))]
		public Task TestCorpus (string fileName) => RunTestCorpusAsync (fileName, false);

		[TestCaseSource (nameof (CorpusCases))]
		public Task TestCorpusAsync (string fileName) => RunTestCorpusAsync (fileName, true);

		async Task RunTestChristmasEmbeddedMessagesAsync (bool async)
		{
			var logger = new TestTnefComplianceLogger ();

			using (var stream = File.OpenRead (Path.Combine (CorpusDirectory, "christmas.tnef"))) {
				using (var message = await LoadAsync (stream, async, null, logger)) {
					Assert.That (await CountEmbeddedMessagesAsync (message, async), Is.EqualTo (2));
					Assert.That (logger.Issues, Is.Empty);
				}
			}
		}

		[Test]
		public Task TestChristmasEmbeddedMessages () => RunTestChristmasEmbeddedMessagesAsync (false);

		[Test]
		public Task TestChristmasEmbeddedMessagesAsync () => RunTestChristmasEmbeddedMessagesAsync (true);

		#endregion

		#region Legacy attributes

		async Task RunTestLegacyMessageAttributesAsync (bool async)
		{
			var sent = new DateTime (2024, 3, 15, 10, 30, 0);
			var received = new DateTime (2024, 3, 15, 10, 31, 5);
			var logger = new TestTnefComplianceLogger ();
			var builder = new TnefBuilder (legacyKey: 0x1234);

			builder.WriteTnefVersion ();
			builder.WriteOemCodepage (1252);
			builder.WriteMessageClass ("IPM.Microsoft Mail.Note");
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.From, FromPayload ("Alice", "SMTP:alice@example.com"));
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Subject, StringPayload ("Hello"));
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.MessageId, StringPayload ("0A1bFF"));
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.DateSent, DatePayload (sent));
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.DateReceived, DatePayload (received));
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Priority, Int16Payload (1));
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.MessageStatus, new byte[] { 0x20 | 0x80 });
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.RequestResponse, Int16Payload (1));
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Owner, OwnerPayload ("Bob", "SMTP:bob@example.com"));
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.AidOwner, TnefBuilder.Int32Payload (42));

			using (var message = await LoadAsync (builder, async, null, logger)) {
				var properties = message.Properties;

				Assert.That (logger.Issues, Is.Empty);
				Assert.That (message.Codepage, Is.EqualTo (1252));
				Assert.That (message.LegacyKey, Is.EqualTo (0x1234));
				Assert.That (message.MessageClass, Is.EqualTo ("IPM.Note"));
				Assert.That (message.Subject, Is.EqualTo ("Hello"));
				Assert.That (properties.GetBytes (TnefPropertyTag.SearchKey), Is.EqualTo (new byte[] { 0x0A, 0x1B, 0xFF }));
				Assert.That (properties.GetDateTime (TnefPropertyTag.ClientSubmitTime), Is.EqualTo (sent));
				Assert.That (properties.GetDateTime (TnefPropertyTag.MessageDeliveryTime), Is.EqualTo (received));
				Assert.That (properties.GetInt32 (TnefPropertyTag.Importance), Is.EqualTo (2));
				Assert.That (properties.GetInt32 (TnefPropertyTag.MessageFlags), Is.EqualTo (0x01 | 0x02 | 0x10));
				Assert.That (properties.GetBoolean (TnefPropertyTag.ResponseRequested), Is.True);
				Assert.That (properties.GetInt32 (TnefPropertyTag.OwnerApptId), Is.EqualTo (42));

				Assert.That (properties.GetString (TnefPropertyTag.SenderNameW), Is.EqualTo ("Alice"));
				Assert.That (properties.GetString (TnefPropertyTag.SenderAddrtypeW), Is.EqualTo ("SMTP"));
				Assert.That (properties.GetString (TnefPropertyTag.SenderEmailAddressW), Is.EqualTo ("alice@example.com"));
				Assert.That (properties.GetBytes (TnefPropertyTag.SenderEntryId), Is.EqualTo (OneOffEntryId ("Alice", "SMTP", "alice@example.com")));

				Assert.That (properties.GetString (TnefPropertyTag.SentRepresentingNameW), Is.EqualTo ("Bob"));
				Assert.That (properties.GetString (TnefPropertyTag.SentRepresentingAddrtypeW), Is.EqualTo ("SMTP"));
				Assert.That (properties.GetString (TnefPropertyTag.SentRepresentingEmailAddressW), Is.EqualTo ("bob@example.com"));
				Assert.That (properties.GetBytes (TnefPropertyTag.SentRepresentingEntryId), Is.EqualTo (OneOffEntryId ("Bob", "SMTP", "bob@example.com")));
				Assert.That (properties.GetString (TnefPropertyTag.RcvdRepresentingNameW), Is.Null);

				Assert.That (properties.TryGetValue (TnefPropertyTag.SubjectW, out var subject), Is.True);
				Assert.That (subject.Tag, Is.EqualTo (TnefPropertyTag.SubjectW));
			}
		}

		[Test]
		public Task TestLegacyMessageAttributes () => RunTestLegacyMessageAttributesAsync (false);

		[Test]
		public Task TestLegacyMessageAttributesAsync () => RunTestLegacyMessageAttributesAsync (true);

		async Task RunTestOwnerOfMeetingResponseAsync (bool async)
		{
			var builder = new TnefBuilder ();

			// Note: attOwner precedes the message class, so it must not be applied until the message has been read.
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Owner, OwnerPayload ("Bob", "SMTP:bob@example.com"));
			builder.WriteMessageClass ("IPM.Microsoft Schedule.MtgRespP");

			using (var message = await LoadAsync (builder, async)) {
				var properties = message.Properties;

				Assert.That (message.MessageClass, Is.EqualTo ("IPM.Schedule.Meeting.Resp.Pos"));
				Assert.That (properties.GetString (TnefPropertyTag.RcvdRepresentingNameW), Is.EqualTo ("Bob"));
				Assert.That (properties.GetString (TnefPropertyTag.RcvdRepresentingEmailAddressW), Is.EqualTo ("bob@example.com"));
				Assert.That (properties.GetBytes (TnefPropertyTag.RcvdRepresentingEntryId), Is.EqualTo (OneOffEntryId ("Bob", "SMTP", "bob@example.com")));
				Assert.That (properties.GetString (TnefPropertyTag.SentRepresentingNameW), Is.Null);
			}
		}

		[Test]
		public Task TestOwnerOfMeetingResponse () => RunTestOwnerOfMeetingResponseAsync (false);

		[Test]
		public Task TestOwnerOfMeetingResponseAsync () => RunTestOwnerOfMeetingResponseAsync (true);

		[TestCase ("Microsoft Mail v3.0 IPM.Microsoft Mail.Read Receipt", "Report.IPM.Note.IPNRN")]
		[TestCase ("IPM.Microsoft Mail.Non-Delivery", "Report.IPM.Note.NDR")]
		[TestCase ("IPM.Microsoft Schedule.MtgRespN", "IPM.Schedule.Meeting.Resp.Neg")]
		[TestCase ("IPM.Microsoft Schedule.MtgRespA", "IPM.Schedule.Meeting.Resp.Tent")]
		[TestCase ("IPM.Microsoft Schedule.MtgReq", "IPM.Schedule.Meeting.Request")]
		[TestCase ("IPM.Microsoft Schedule.MtgCncl", "IPM.Schedule.Meeting.Canceled")]
		[TestCase ("IPM.Note.Custom", "IPM.Note.Custom")]
		public void TestMessageClassTranslation (string legacy, string expected)
		{
			var builder = new TnefBuilder ();

			builder.WriteMessageClass (legacy);

			using (var message = TnefMessage.Load (builder.ToStream ()))
				Assert.That (message.MessageClass, Is.EqualTo (expected));
		}

		[TestCase (1, 2)]
		[TestCase (2, 1)]
		[TestCase (3, 0)]
		public void TestPriority (int priority, int importance)
		{
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Priority, Int16Payload (priority));

			using (var message = TnefMessage.Load (builder.ToStream ()))
				Assert.That (message.Properties.GetInt32 (TnefPropertyTag.Importance), Is.EqualTo (importance));
		}

		async Task RunTestInvalidLegacyValuesAsync (bool async)
		{
			var logger = new TestTnefComplianceLogger ();
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Priority, Int16Payload (7));
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.MessageId, StringPayload ("ABC"));
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.ConversationId, StringPayload ("not hex"));
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.MessageStatus, Array.Empty<byte> ());
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.From, new byte[] { 1, 0, 0, 0 });
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Owner, new byte[] { 0xFF, 0x00, 1, 2 });
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Subject, StringPayload ("still loaded"));

			using (var message = await LoadAsync (builder, async, null, logger)) {
				var properties = message.Properties;

				Assert.That (message.Subject, Is.EqualTo ("still loaded"));
				Assert.That (properties.GetInt32 (TnefPropertyTag.Importance), Is.Null);
				Assert.That (properties.GetBytes (TnefPropertyTag.SearchKey), Is.Null);
				Assert.That (properties.GetInt32 (TnefPropertyTag.MessageFlags), Is.Null);
				Assert.That (properties.GetBytes (TnefPropertyTag.SenderEntryId), Is.Null);
				Assert.That (properties.GetBytes (TnefPropertyTag.SentRepresentingEntryId), Is.Null);
			}

			Assert.That (Violations (logger).Count (v => v == TnefComplianceViolation.InvalidAttributeValue), Is.EqualTo (5));
		}

		[Test]
		public Task TestInvalidLegacyValues () => RunTestInvalidLegacyValuesAsync (false);

		[Test]
		public Task TestInvalidLegacyValuesAsync () => RunTestInvalidLegacyValuesAsync (true);

		// A truncated date attribute must not report the value of the previous date attribute.
		async Task RunTestTruncatedDateAsync (bool async)
		{
			var sent = new DateTime (2024, 3, 15, 10, 30, 0);
			var logger = new TestTnefComplianceLogger ();
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.DateSent, DatePayload (sent));
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.DateReceived, new byte[4]);

			using (var message = await LoadAsync (builder, async, null, logger)) {
				Assert.That (message.Properties.GetDateTime (TnefPropertyTag.ClientSubmitTime), Is.EqualTo (sent));
				Assert.That (message.Properties.GetDateTime (TnefPropertyTag.MessageDeliveryTime), Is.Null);
			}

			Assert.That (Violations (logger), Does.Contain (TnefComplianceViolation.InvalidAttributeValue));
		}

		[Test]
		public Task TestTruncatedDate () => RunTestTruncatedDateAsync (false);

		[Test]
		public Task TestTruncatedDateAsync () => RunTestTruncatedDateAsync (true);

		#endregion

		#region MAPI properties and bodies

		async Task RunTestMapiPropertiesTakePrecedenceAsync (bool async)
		{
			var builder = new TnefBuilder ();
			var properties = new TnefMapiPropertyBuilder ();

			properties.WriteStringProperty (TnefPropertyTag.SubjectW, "MAPI subject");
			properties.WriteInt32Property (TnefPropertyTag.Importance, 0);

			// Note: The legacy attributes are processed both before and after the MAPI properties.
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Subject, StringPayload ("Legacy subject"));
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Priority, Int16Payload (1));

			using (var message = await LoadAsync (builder, async)) {
				Assert.That (message.Subject, Is.EqualTo ("MAPI subject"));
				Assert.That (message.Properties.GetInt32 (TnefPropertyTag.Importance), Is.EqualTo (0));
				Assert.That (message.Properties.Count (p => p.Tag.Id == TnefPropertyId.Subject), Is.EqualTo (1));
			}
		}

		[Test]
		public Task TestMapiPropertiesTakePrecedence () => RunTestMapiPropertiesTakePrecedenceAsync (false);

		[Test]
		public Task TestMapiPropertiesTakePrecedenceAsync () => RunTestMapiPropertiesTakePrecedenceAsync (true);

		async Task RunTestBodiesAsync (bool async)
		{
			var rtf = new RtfCompressedBuilder ().WriteLiterals (Encoding.ASCII.GetBytes ("{\\rtf1 Hello}")).WriteEndOfStream ();
			var html = Encoding.UTF8.GetBytes ("<p>caf\u00e9</p>");
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();

			properties.WriteStringProperty (TnefPropertyTag.BodyW, "MAPI body\0\0");
			properties.WriteStringProperty (TnefPropertyTag.BodyW, "Second body");
			properties.WriteBinaryProperty (TnefPropertyTag.BodyHtmlB, html);
			properties.WriteBinaryProperty (TnefPropertyTag.RtfCompressed, rtf.ToArray ());
			properties.WriteInt32Property (TnefPropertyTag.InternetCodepage, 65001);

			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Body, StringPayload ("Legacy body"));
			builder.WriteMapiProperties (TnefAttributeLevel.Message, properties);

			using (var message = await LoadAsync (builder, async)) {
				Assert.That (message.TextBody, Is.Not.Null);
				Assert.That (message.TextBody.Format, Is.EqualTo (TnefMessageBodyFormat.Text));
				Assert.That (message.TextBody.Tag, Is.EqualTo (TnefPropertyTag.BodyW));
				Assert.That (message.TextBody.Encoding, Is.EqualTo (Encoding.Unicode));
				Assert.That (message.TextBody.GetText (), Is.EqualTo ("MAPI body"));
				Assert.That (message.TextBody.Length, Is.EqualTo (18));

				Assert.That (message.HtmlBody, Is.Not.Null);
				Assert.That (message.HtmlBody.Format, Is.EqualTo (TnefMessageBodyFormat.Html));
				Assert.That (message.HtmlBody.Encoding.CodePage, Is.EqualTo (65001));
				Assert.That (message.HtmlBody.GetText (), Is.EqualTo ("<p>caf\u00e9</p>"));
				using (var stream = message.HtmlBody.OpenRead ())
					Assert.That (ReadAll (stream), Is.EqualTo (html));

				Assert.That (message.RtfBody, Is.Not.Null);
				Assert.That (message.RtfBody.Format, Is.EqualTo (TnefMessageBodyFormat.CompressedRtf));
				Assert.That (message.RtfBody.Encoding, Is.Null);
				using (var stream = message.RtfBody.OpenRead ())
					Assert.That (ReadAll (stream), Is.EqualTo (rtf.ToArray ()));
				using (var stream = message.RtfBody.OpenDecodedRead ())
					Assert.That (ReadAll (stream), Is.EqualTo (rtf.GetDecompressed ()));
				Assert.That (message.RtfBody.GetText (), Is.EqualTo ("{\\rtf1 Hello}"));

				// The bodies are not included in the properties.
				Assert.That (message.Properties.Any (p => p.Tag.Id == TnefPropertyId.Body || p.Tag.Id == TnefPropertyId.BodyHtml || p.Tag.Id == TnefPropertyId.RtfCompressed), Is.False);
				Assert.That (message.Properties.GetInt32 (TnefPropertyTag.InternetCodepage), Is.EqualTo (65001));
			}
		}

		[Test]
		public Task TestBodies () => RunTestBodiesAsync (false);

		[Test]
		public Task TestBodiesAsync () => RunTestBodiesAsync (true);

		async Task RunTestLegacyBodyAsync (bool async)
		{
			var builder = new TnefBuilder ();

			builder.WriteOemCodepage (1252);
			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Body, Encoding.GetEncoding (1252).GetBytes ("Caf\u00e9\0"));

			using (var message = await LoadAsync (builder, async)) {
				Assert.That (message.TextBody, Is.Not.Null);
				Assert.That (message.TextBody.Tag, Is.EqualTo (TnefPropertyTag.BodyA));
				Assert.That (message.TextBody.Encoding.CodePage, Is.EqualTo (1252));
				Assert.That (message.TextBody.GetText (), Is.EqualTo ("Caf\u00e9"));
				Assert.That (message.HtmlBody, Is.Null);
				Assert.That (message.RtfBody, Is.Null);
			}
		}

		[Test]
		public Task TestLegacyBody () => RunTestLegacyBodyAsync (false);

		[Test]
		public Task TestLegacyBodyAsync () => RunTestLegacyBodyAsync (true);

		async Task RunTestRecipientsAsync (bool async)
		{
			var to = new TnefMapiPropertyBuilder ();
			var bcc = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();

			to.WriteStringProperty (TnefPropertyTag.DisplayNameW, "Alice");
			to.WriteStringProperty (TnefPropertyTag.EmailAddressW, "alice@example.com");
			to.WriteStringProperty (TnefPropertyTag.AddrtypeW, "SMTP");
			to.WriteInt32Property (TnefPropertyTag.RecipientType, 1);

			// Note: The high bits of PidTagRecipientType are flags.
			bcc.WriteStringProperty (TnefPropertyTag.DisplayNameW, "Bob");
			bcc.WriteInt32Property (TnefPropertyTag.RecipientType, 0x10000003);

			builder.WriteRecipientTable (to, bcc);

			using (var message = await LoadAsync (builder, async)) {
				Assert.That (message.Recipients, Has.Count.EqualTo (2));

				Assert.That (message.Recipients[0].RecipientType, Is.EqualTo (TnefRecipientType.To));
				Assert.That (message.Recipients[0].DisplayName, Is.EqualTo ("Alice"));
				Assert.That (message.Recipients[0].EmailAddress, Is.EqualTo ("alice@example.com"));
				Assert.That (message.Recipients[0].AddressType, Is.EqualTo ("SMTP"));
				Assert.That (message.Recipients[0].Properties, Has.Count.EqualTo (4));

				Assert.That (message.Recipients[1].RecipientType, Is.EqualTo (TnefRecipientType.Bcc));
				Assert.That (message.Recipients[1].DisplayName, Is.EqualTo ("Bob"));
				Assert.That (message.Recipients[1].EmailAddress, Is.Null);
			}
		}

		[Test]
		public Task TestRecipients () => RunTestRecipientsAsync (false);

		[Test]
		public Task TestRecipientsAsync () => RunTestRecipientsAsync (true);

		#endregion

		#region Attachments

		async Task RunTestAttachmentsAsync (bool async)
		{
			var created = new DateTime (2024, 1, 2, 3, 4, 5);
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();

			properties.WriteStringProperty (TnefPropertyTag.AttachLongFilenameW, "long file name.txt");
			properties.WriteStringProperty (TnefPropertyTag.AttachMimeTagW, "text/plain");
			properties.WriteStringProperty (TnefPropertyTag.AttachContentIdW, "part1@example.com");
			properties.WriteInt32Property (TnefPropertyTag.AttachMethod, (int) TnefAttachMethod.ByValue);
			properties.WriteInt32Property (TnefPropertyTag.AttachFlags, (int) TnefAttachFlags.RenderedInBody);

			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Subject, StringPayload ("Attachments"));
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData, RenderDataPayload (2, 17, 1));
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachTitle, StringPayload ("LONGFI~1.TXT"));
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachCreateDate, DatePayload (created));
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachData, Encoding.ASCII.GetBytes ("first"));
			builder.WriteMapiProperties (TnefAttributeLevel.Attachment, properties);
			CreateAttachment (builder, "second.bin", new byte[] { 1, 2, 3 });

			using (var message = await LoadAsync (builder, async)) {
				Assert.That (message.Attachments, Has.Count.EqualTo (2));

				var first = message.Attachments[0];
				Assert.That (first.FileName, Is.EqualTo ("long file name.txt"));
				Assert.That (first.MimeType, Is.EqualTo ("text/plain"));
				Assert.That (first.ContentId, Is.EqualTo ("part1@example.com"));
				Assert.That (first.Method, Is.EqualTo (TnefAttachMethod.ByValue));
				Assert.That (first.Flags, Is.EqualTo (TnefAttachFlags.RenderedInBody));
				Assert.That (first.IsEmbeddedMessage, Is.False);
				Assert.That (first.HasContent, Is.True);
				Assert.That (first.Length, Is.EqualTo (5));
				Assert.That (first.Properties.GetInt32 (TnefPropertyTag.RenderingPosition), Is.EqualTo (17));
				Assert.That (first.Properties.GetBytes (TnefPropertyTag.AttachTag), Is.Not.Null);
				Assert.That (first.Properties.GetBytes (TnefPropertyTag.AttachEncoding), Is.Not.Null);
				Assert.That (first.Properties.GetDateTime (TnefPropertyTag.CreationTime), Is.EqualTo (created));
				Assert.That (first.Properties.Any (p => p.Tag.Id == TnefPropertyId.AttachData), Is.False);
				using (var stream = first.OpenRead ())
					Assert.That (Encoding.ASCII.GetString (ReadAll (stream)), Is.EqualTo ("first"));

				var second = message.Attachments[1];
				Assert.That (second.FileName, Is.EqualTo ("second.bin"));
				Assert.That (second.MimeType, Is.Null);
				Assert.That (second.Method, Is.EqualTo (TnefAttachMethod.ByValue));
				Assert.That (second.Properties.GetBytes (TnefPropertyTag.AttachTag), Is.Null);
				using (var stream = second.OpenRead ())
					Assert.That (ReadAll (stream), Is.EqualTo (new byte[] { 1, 2, 3 }));

				Assert.Throws<InvalidOperationException> (() => second.LoadEmbeddedMessage ());
				Assert.ThrowsAsync<InvalidOperationException> (() => second.LoadEmbeddedMessageAsync ());
			}
		}

		[Test]
		public Task TestAttachments () => RunTestAttachmentsAsync (false);

		[Test]
		public Task TestAttachmentsAsync () => RunTestAttachmentsAsync (true);

		async Task RunTestMapiAttachDataTakesPrecedenceAsync (bool async)
		{
			var properties = new TnefMapiPropertyBuilder ();
			var builder = new TnefBuilder ();

			properties.WriteBinaryProperty (TnefPropertyTag.AttachDataBin, Encoding.ASCII.GetBytes ("MAPI"));
			properties.WriteBinaryProperty (TnefPropertyTag.AttachDataBin, Encoding.ASCII.GetBytes ("ignored"));

			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachRenderData, RenderDataPayload ());
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachData, Encoding.ASCII.GetBytes ("legacy"));
			builder.WriteMapiProperties (TnefAttributeLevel.Attachment, properties);

			using (var message = await LoadAsync (builder, async)) {
				Assert.That (message.Attachments, Has.Count.EqualTo (1));
				using (var stream = message.Attachments[0].OpenRead ())
					Assert.That (Encoding.ASCII.GetString (ReadAll (stream)), Is.EqualTo ("MAPI"));
			}
		}

		[Test]
		public Task TestMapiAttachDataTakesPrecedence () => RunTestMapiAttachDataTakesPrecedenceAsync (false);

		[Test]
		public Task TestMapiAttachDataTakesPrecedenceAsync () => RunTestMapiAttachDataTakesPrecedenceAsync (true);

		async Task RunTestImplicitAttachmentAsync (bool async)
		{
			var builder = new TnefBuilder ();

			// Note: There is no attAttachRenddata attribute to begin the attachment.
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachTitle, StringPayload ("orphan.txt"));
			builder.WriteAttribute (TnefAttributeLevel.Attachment, TnefAttributeTag.AttachData, Encoding.ASCII.GetBytes ("data"));

			using (var message = await LoadAsync (builder, async)) {
				Assert.That (message.Attachments, Has.Count.EqualTo (1));
				Assert.That (message.Attachments[0].FileName, Is.EqualTo ("orphan.txt"));
				Assert.That (message.Attachments[0].Length, Is.EqualTo (4));
			}
		}

		[Test]
		public Task TestImplicitAttachment () => RunTestImplicitAttachmentAsync (false);

		[Test]
		public Task TestImplicitAttachmentAsync () => RunTestImplicitAttachmentAsync (true);

		async Task RunTestMaxAttachmentsAsync (bool async)
		{
			var options = new TnefOptions { MaxAttachments = 2 };
			var logger = new TestTnefComplianceLogger ();
			var builder = new TnefBuilder ();

			for (int i = 0; i < 4; i++)
				CreateAttachment (builder, $"file{i}.bin", new byte[] { (byte) i });

			using (var message = await LoadAsync (builder, async, options, logger)) {
				Assert.That (message.Attachments.Select (a => a.FileName), Is.EqualTo (new[] { "file0.bin", "file1.bin" }));
			}

			Assert.That (Violations (logger), Is.EqualTo (new[] { TnefComplianceViolation.TooManyAttachments }));
		}

		[Test]
		public Task TestMaxAttachments () => RunTestMaxAttachmentsAsync (false);

		[Test]
		public Task TestMaxAttachmentsAsync () => RunTestMaxAttachmentsAsync (true);

		async Task RunTestDataSizeLimitAsync (bool async)
		{
			// Note: the budget also covers the buffered attachment titles and rendering data.
			var options = new TnefOptions { MaxTotalDataBytes = 150 };
			var logger = new TestTnefComplianceLogger ();
			var builder = new TnefBuilder ();

			CreateAttachment (builder, "small.bin", new byte[100]);
			CreateAttachment (builder, "large.bin", new byte[100]);

			using (var message = await LoadAsync (builder, async, options, logger)) {
				Assert.That (message.Attachments, Has.Count.EqualTo (2));
				Assert.That (message.Attachments[0].HasContent, Is.True);
				Assert.That (message.Attachments[0].Length, Is.EqualTo (100));

				var large = message.Attachments[1];
				Assert.That (large.FileName, Is.EqualTo ("large.bin"));
				Assert.That (large.HasContent, Is.False);
				Assert.That (large.Length, Is.EqualTo (0));
				using (var stream = large.OpenRead ())
					Assert.That (ReadAll (stream), Is.Empty);
			}

			Assert.That (Violations (logger), Does.Contain (TnefComplianceViolation.DataSizeLimitExceeded));
		}

		[Test]
		public Task TestDataSizeLimit () => RunTestDataSizeLimitAsync (false);

		[Test]
		public Task TestDataSizeLimitAsync () => RunTestDataSizeLimitAsync (true);

		async Task RunTestEmbeddedMessageAsync (bool async)
		{
			var inner = CreateEmbeddedMessage ("Inner", CreateEmbeddedMessage ("Innermost"));
			var logger = new TestTnefComplianceLogger ();
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Subject, StringPayload ("Outer"));
			WriteEmbeddedAttachment (builder, inner);

			using (var message = await LoadAsync (builder, async, null, logger)) {
				var attachment = message.Attachments.Single ();

				Assert.That (attachment.IsEmbeddedMessage, Is.True);
				Assert.That (attachment.Method, Is.EqualTo (TnefAttachMethod.EmbeddedMessage));
				Assert.That (attachment.Length, Is.EqualTo (inner.Length));
				using (var stream = attachment.OpenRead ())
					Assert.That (ReadAll (stream), Is.EqualTo (inner));

				using (var embedded = await LoadEmbeddedMessageAsync (attachment, async)) {
					Assert.That (embedded.Subject, Is.EqualTo ("Inner"));

					using (var innermost = await LoadEmbeddedMessageAsync (embedded.Attachments.Single (), async))
						Assert.That (innermost.Subject, Is.EqualTo ("Innermost"));
				}

				// The embedded message may be loaded any number of times.
				using (var embedded = await LoadEmbeddedMessageAsync (attachment, async))
					Assert.That (embedded.Subject, Is.EqualTo ("Inner"));
			}

			Assert.That (logger.Issues, Is.Empty);
		}

		[Test]
		public Task TestEmbeddedMessage () => RunTestEmbeddedMessageAsync (false);

		[Test]
		public Task TestEmbeddedMessageAsync () => RunTestEmbeddedMessageAsync (true);

		async Task RunTestEmbeddedMessageNestingTooDeepAsync (bool async)
		{
			var inner = CreateEmbeddedMessage ("Inner", CreateEmbeddedMessage ("Innermost"));
			var options = new TnefOptions { MaxNestingDepth = 1 };
			var logger = new TestTnefComplianceLogger ();
			var builder = new TnefBuilder ();

			WriteEmbeddedAttachment (builder, inner);

			using (var message = await LoadAsync (builder, async, options, logger)) {
				using (var embedded = await LoadEmbeddedMessageAsync (message.Attachments.Single (), async)) {
					Assert.That (embedded.Subject, Is.EqualTo ("Inner"));
					Assert.That (logger.Issues, Is.Empty);

					using (var innermost = await LoadEmbeddedMessageAsync (embedded.Attachments.Single (), async)) {
						Assert.That (innermost.Subject, Is.Null);
						Assert.That (innermost.Properties, Is.Empty);
						Assert.That (innermost.Attachments, Is.Empty);
					}
				}
			}

			Assert.That (Violations (logger), Is.EqualTo (new[] { TnefComplianceViolation.NestingTooDeep }));
			Assert.That (logger.Issues[0].StreamOffset, Is.GreaterThan (0));
		}

		[Test]
		public Task TestEmbeddedMessageNestingTooDeep () => RunTestEmbeddedMessageNestingTooDeepAsync (false);

		[Test]
		public Task TestEmbeddedMessageNestingTooDeepAsync () => RunTestEmbeddedMessageNestingTooDeepAsync (true);

		[Test]
		public void TestDispose ()
		{
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Body, StringPayload ("body"));
			CreateAttachment (builder, "file.bin", new byte[] { 1 });

			var message = TnefMessage.Load (builder.ToStream ());
			var attachment = message.Attachments[0];
			var body = message.TextBody;

			message.Dispose ();
			message.Dispose ();

			Assert.Throws<ObjectDisposedException> (() => attachment.OpenRead ());
			Assert.Throws<ObjectDisposedException> (() => _ = attachment.Length);
			Assert.Throws<ObjectDisposedException> (() => body.OpenRead ());
			Assert.Throws<ObjectDisposedException> (() => body.GetText ());
		}

		#endregion

		#region Errors

		[Test]
		public void TestInvalidSignature ()
		{
			var builder = new TnefBuilder (signature: 0x12345678);

			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Subject, StringPayload ("Hello"));

			var ex = Assert.Throws<TnefException> (() => TnefMessage.Load (builder.ToStream ()));
			Assert.That (ex.Violation, Is.EqualTo (TnefComplianceViolation.InvalidSignature));

			ex = Assert.ThrowsAsync<TnefException> (() => TnefMessage.LoadAsync (builder.ToStream ()));
			Assert.That (ex.Violation, Is.EqualTo (TnefComplianceViolation.InvalidSignature));
		}

		[Test]
		public void TestReaderAlreadyStarted ()
		{
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Subject, StringPayload ("Hello"));

			using (var reader = new TnefReader (builder.ToStream ())) {
				Assert.That (reader.Read (), Is.True);

				Assert.Throws<InvalidOperationException> (() => TnefMessage.Load (reader));
				Assert.ThrowsAsync<InvalidOperationException> (() => TnefMessage.LoadAsync (reader));
			}
		}

		[Test]
		public void TestLoadLeavesStreamOpen ()
		{
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Subject, StringPayload ("Hello"));

			using (var stream = builder.ToStream ()) {
				using (var message = TnefMessage.Load (stream))
					Assert.That (message.Subject, Is.EqualTo ("Hello"));

				Assert.That (stream.CanRead, Is.True);
			}
		}

		[Test]
		public void TestCancellation ()
		{
			var builder = new TnefBuilder ();

			builder.WriteAttribute (TnefAttributeLevel.Message, TnefAttributeTag.Subject, StringPayload ("Hello"));

			using (var cts = new CancellationTokenSource ()) {
				cts.Cancel ();

				Assert.Throws<OperationCanceledException> (() => TnefMessage.Load (builder.ToStream (), null, cts.Token));
				Assert.CatchAsync<OperationCanceledException> (() => TnefMessage.LoadAsync (builder.ToStream (), null, cts.Token));
			}
		}

		#endregion
	}
}

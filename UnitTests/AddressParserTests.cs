//
// AddressParserTests.cs
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

using System.Buffers;
using System.Text;

using MimeKit;
using MimeKit.Utils;

namespace UnitTests {
	[TestFixture]
	public class AddressParserTests
	{
		sealed class TestEncodingProvider : EncodingProvider
		{
			public Encoding Utf8Encoding;
			public Encoding Latin1Encoding;
			public int Utf8Requests;
			public int Utf8Failures;

			public override Encoding GetEncoding (int codepage)
			{
				if (codepage == 65001 && Utf8Encoding != null) {
					Utf8Requests++;

					if (Utf8Failures > 0) {
						Utf8Failures--;
						throw new NotSupportedException ("Temporary encoding lookup failure.");
					}

					return Utf8Encoding;
				}

				return codepage == 28591 ? Latin1Encoding : null;
			}

			public override Encoding GetEncoding (string name)
			{
				return null;
			}

			public void Reset ()
			{
				Utf8Encoding = Latin1Encoding = null;
				Utf8Requests = Utf8Failures = 0;
			}
		}

		static readonly TestEncodingProvider FirstEncodingProvider = new TestEncodingProvider ();
		static readonly TestEncodingProvider SecondEncodingProvider = new TestEncodingProvider ();

		[Test]
		[NonParallelizable]
		public void TestEncodedUtf8NamesSkipDecoderEncodingLookup ([Values (false, true)] bool byteInput)
		{
			RegisterTestEncodingProvider ();
			FirstEncodingProvider.Utf8Encoding = new UTF8Encoding (false, true);

			try {
				var addresses = ParseAddresses ("=?utf-8?q?Ren=C3=A9e?= <a@example.com>, =?utf-8?q?Andr=C3=A9?= <b@example.com>", byteInput);

				Assert.That (addresses[0].Name, Is.EqualTo ("Ren\u00e9e"));
				Assert.That (addresses[1].Name, Is.EqualTo ("Andr\u00e9"));
				Assert.That (FirstEncodingProvider.Utf8Requests, Is.EqualTo (1));
			} finally {
				FirstEncodingProvider.Reset ();
			}
		}

		[Test]
		[NonParallelizable]
		public void TestEncodedNameUsesUtf8ReplacementFallback ([Values (false, true)] bool byteInput)
		{
			RegisterTestEncodingProvider ();
			FirstEncodingProvider.Utf8Encoding = (Encoding) new UTF8Encoding ().Clone ();
			FirstEncodingProvider.Utf8Encoding.DecoderFallback = new DecoderReplacementFallback ("[invalid]");

			try {
				var addresses = ParseAddresses ("=?utf-8?q?Ren=C3=A9e=FF?= <a@example.com>", byteInput);

				Assert.That (addresses[0].Name, Is.EqualTo ("Ren\u00e9e[invalid]"));
			} finally {
				FirstEncodingProvider.Reset ();
			}
		}

		[Test]
		[NonParallelizable]
		public void TestEncodedNameUsesUtf8ExceptionFallback ([Values (false, true)] bool byteInput)
		{
			RegisterTestEncodingProvider ();
			FirstEncodingProvider.Utf8Encoding = new UTF8Encoding (false, true);

			try {
				Assert.Throws<DecoderFallbackException> (() => ParseAddresses ("=?utf-8?q?Ren=C3=A9e=FF?= <a@example.com>", byteInput));
			} finally {
				FirstEncodingProvider.Reset ();
			}
		}

		static void RegisterTestEncodingProvider ()
		{
			CharsetUtils.GetEncodingOrDefault (65001, Encoding.UTF8);
			Encoding.RegisterProvider (FirstEncodingProvider);
		}

		static InternetAddressList ParseAddresses (string text, bool byteInput)
		{
			return byteInput ? InternetAddressList.Parse (Encoding.UTF8.GetBytes (text)) : InternetAddressList.Parse (text);
		}

		[Test]
		[NonParallelizable]
		public void TestReusesEncodingWithinParse (
			[Values ("First <a@example.com>, Second <b@example.com>",
				"Group: First <a@example.com>, Second <b@example.com>;",
				"Outer: Inner: First <a@example.com>;, Second <b@example.com>;")] string text,
			[Values (false, true)] bool byteInput)
		{
			RegisterTestEncodingProvider ();
			FirstEncodingProvider.Utf8Encoding = new UTF8Encoding (false, true);

			try {
				ParseAddresses (text, byteInput);

				Assert.That (FirstEncodingProvider.Utf8Requests, Is.EqualTo (1));
			} finally {
				FirstEncodingProvider.Reset ();
			}
		}

		[Test]
		[NonParallelizable]
		public void TestEncodingProviderRegistrationBetweenParses ([Values (false, true)] bool byteInput)
		{
			const string text = "First <a@example.com>, Second <b@example.com>";
			RegisterTestEncodingProvider ();
			FirstEncodingProvider.Utf8Encoding = new UTF8Encoding (false, true);

			try {
				var firstEncoding = FirstEncodingProvider.Utf8Encoding;
				var first = ParseAddresses (text, byteInput);
				FirstEncodingProvider.Utf8Encoding = null;
				SecondEncodingProvider.Utf8Encoding = new UTF8Encoding (true, false);
				Encoding.RegisterProvider (SecondEncodingProvider);
				var second = ParseAddresses (text, byteInput);

				foreach (var address in first)
					Assert.That (address.Encoding, Is.SameAs (firstEncoding));
				foreach (var address in second)
					Assert.That (address.Encoding, Is.SameAs (SecondEncodingProvider.Utf8Encoding));
			} finally {
				FirstEncodingProvider.Reset ();
				SecondEncodingProvider.Reset ();
			}
		}

		[Test]
		[NonParallelizable]
		public void TestFailedEncodingLookupIsRetried ([Values (false, true)] bool byteInput)
		{
			const string text = "First <a@example.com>, Second <b@example.com>, Third <c@example.com>";
			RegisterTestEncodingProvider ();
			FirstEncodingProvider.Utf8Encoding = new UTF8Encoding (false, true);
			FirstEncodingProvider.Utf8Failures = 1;

			try {
				var addresses = ParseAddresses (text, byteInput);

				Assert.That (addresses[0].Encoding, Is.SameAs (Encoding.UTF8));
				Assert.That (addresses[1].Encoding, Is.SameAs (FirstEncodingProvider.Utf8Encoding));
				Assert.That (addresses[2].Encoding, Is.SameAs (FirstEncodingProvider.Utf8Encoding));
				Assert.That (FirstEncodingProvider.Utf8Requests, Is.EqualTo (2));
			} finally {
				FirstEncodingProvider.Reset ();
			}
		}

		[Test]
		[NonParallelizable]
		public void TestEncodingCacheUsesRequestedCodepage ([Values (false, true)] bool byteInput)
		{
			const string text = "=?iso-8859-1?q?First?= <a@example.com>, Second <b@example.com>";
			RegisterTestEncodingProvider ();
			FirstEncodingProvider.Utf8Encoding = new UTF8Encoding (false, true);
			FirstEncodingProvider.Latin1Encoding = new UTF8Encoding (true, false);

			try {
				var addresses = ParseAddresses (text, byteInput);

				Assert.That (addresses[0].Name, Is.EqualTo ("First"));
				Assert.That (addresses[0].Encoding, Is.SameAs (FirstEncodingProvider.Latin1Encoding));
				Assert.That (addresses[1].Encoding, Is.SameAs (FirstEncodingProvider.Utf8Encoding));
			} finally {
				FirstEncodingProvider.Reset ();
			}
		}

		static string Describe (InternetAddress address)
		{
			var description = address.GetType ().Name + "|" + address.Name + "|" + address.Encoding.CodePage;

			if (address is MailboxAddress mailbox)
				return description + "|" + mailbox.Address + "|" + string.Join (",", mailbox.Route);

			foreach (var member in ((GroupAddress) address).Members)
				description += "[" + Describe (member) + "]";

			return description;
		}

		static string Describe (InternetAddressList addresses)
		{
			return string.Join (";", addresses.Select (Describe));
		}

		static string ParseOutcome (Func<string> parse)
		{
			try {
				return parse ();
			} catch (ParseException ex) {
				return ex.Message + "|" + ex.TokenIndex + "|" + ex.ErrorIndex + "|" + ex.InnerException?.GetType ().Name;
			}
		}

		[Test]
		public void TestStringAndByteInputEquivalence (
			[Values (RfcComplianceMode.Strict, RfcComplianceMode.Loose, RfcComplianceMode.Looser)] RfcComplianceMode compliance,
			[Values (RfcComplianceMode.Strict, RfcComplianceMode.Loose)] RfcComplianceMode encodedWords,
			[Values (false, true)] bool unquotedCommas)
		{
			var inputs = new List<string> {
				"", " \t\r\n", "(ignored) a@example.com", "a@example.com (A\tB)",
				"\"First\\ Last\" <a@example.com>", "\"A\r\n B\" <a@example.com>",
				"Andr\u00e9 <\u00e9@b\u00fccher.example>", "\U0001f600 <a@example.com>",
				"\ud800 <a@example.com>", "\udc00 <a@example.com>", "\ud800\\\udc00 <a@example.com>",
				"\"\ud800\\\udc00\" <a@example.com>", "\ud800@example.com", "\udc00@example.com",
				"a@\ud800.example", "a@example.com (\ud800\\\udc00)",
				"a@example.com (\u00e9)", "=?iso-8859-1?q?Andr=E9?= <a@example.com>",
				"=?utf-8?q?First?= \t =?utf-8?q?_Last?= <a@example.com>",
				"\"=?utf-8?b?IlxOYW1lIg==?=\" <a@example.com>", "a@example.com (=?utf-8?q?A_B?=)",
				"Name <@\u00e9.example,@b.example:\u00e9@example.com>",
				"\u00e9: \U0001f600: \u00e9@example.com;;", "\u00e9: a@example.com",
				"\"\u00e9\" <a@", "\u00e9 <a@[\u00e9]>", "\u00e9 <a@example.com\u00a0>",
				"\u00e9 <a@example.com> \U0001f600", "\u00e9 <a@example.com> (unterminated",
				"\"bad\0name\" <a@example.com>", "\u00e9: \"bad\\\nname\" <a@example.com>;",
				"a\\@b@example.com", "a..b@example.com", "\"Unclosed <a@example.com>",
				"first@example.com, \"Last, First\" (no address), last@example.com"
			};

			var random = new Random (941);
			string[] fragments = { "\"", "\\", "(", ")", ":", ";", ",", "<", ">", "@", ".", " ", "\t", "\r\n", "=?", "?=", "\ud800", "\udc00", "\U0001f600", "\u00e9", "\0" };
			int seedCount = inputs.Count;

			for (int i = 0; i < 512; i++) {
				string input = inputs[random.Next (seedCount)];
				int position = random.Next (input.Length + 1);
				string fragment = i < 128 ? ((char) i).ToString () : fragments[random.Next (fragments.Length)];

				inputs.Add (input.Insert (position, fragment));
				inputs.Add ("\"" + (char) random.Next (0x10000) + "\" <a@example.com>");
			}

			foreach (bool allowLocal in new[] { false, true }) {
				foreach (var charset in new[] { Encoding.UTF8, Encoding.GetEncoding (1252) }) {
					var options = new ParserOptions {
						AddressParserComplianceMode = compliance,
						Rfc2047ComplianceMode = encodedWords,
						AllowUnquotedCommasInAddresses = unquotedCommas,
						AllowAddressesWithoutDomain = allowLocal,
						CharsetEncoding = charset
					};

					foreach (var input in inputs) {
						var bytes = Encoding.UTF8.GetBytes (input);
						string context = $"{compliance}, {allowLocal}, {charset.CodePage}, {input}";

						Assert.That (InternetAddress.TryParse (options, input, out var address), Is.EqualTo (InternetAddress.TryParse (options, bytes, out var byteAddress)), context);
						Assert.That (address == null ? null : Describe (address), Is.EqualTo (byteAddress == null ? null : Describe (byteAddress)), context);
						Assert.That (MailboxAddress.TryParse (options, input, out var mailbox), Is.EqualTo (MailboxAddress.TryParse (options, bytes, out var byteMailbox)), context);
						Assert.That (mailbox == null ? null : Describe (mailbox), Is.EqualTo (byteMailbox == null ? null : Describe (byteMailbox)), context);
						Assert.That (GroupAddress.TryParse (options, input, out var group), Is.EqualTo (GroupAddress.TryParse (options, bytes, out var byteGroup)), context);
						Assert.That (group == null ? null : Describe (group), Is.EqualTo (byteGroup == null ? null : Describe (byteGroup)), context);
						Assert.That (InternetAddressList.TryParse (options, input, out var list), Is.EqualTo (InternetAddressList.TryParse (options, bytes, out var byteList)), context);
						Assert.That (list == null ? null : Describe (list), Is.EqualTo (byteList == null ? null : Describe (byteList)), context);

						Assert.That (ParseOutcome (() => Describe (InternetAddress.Parse (options, input))), Is.EqualTo (ParseOutcome (() => Describe (InternetAddress.Parse (options, bytes)))), context);
						Assert.That (ParseOutcome (() => Describe (MailboxAddress.Parse (options, input))), Is.EqualTo (ParseOutcome (() => Describe (MailboxAddress.Parse (options, bytes)))), context);
						Assert.That (ParseOutcome (() => Describe (GroupAddress.Parse (options, input))), Is.EqualTo (ParseOutcome (() => Describe (GroupAddress.Parse (options, bytes)))), context);
						Assert.That (ParseOutcome (() => Describe (InternetAddressList.Parse (options, input))), Is.EqualTo (ParseOutcome (() => Describe (InternetAddressList.Parse (options, bytes)))), context);
					}
				}
			}
		}

		static long MeasureParsingAllocation (string input)
		{
			if (TestHelper.IsCodeCoverageEnabled)
				Assert.Ignore ("The code-coverage recorder allocates memory, so allocation budgets cannot be measured.");

			long before = GC.GetAllocatedBytesForCurrentThread ();

			for (int i = 0; i < 10; i++)
				InternetAddressList.Parse (input);

			return GC.GetAllocatedBytesForCurrentThread () - before;
		}

		static long MeasureAllocation<T> (Func<T> action)
		{
			if (TestHelper.IsCodeCoverageEnabled)
				Assert.Ignore ("The code-coverage recorder allocates memory, so allocation budgets cannot be measured.");

			long before = GC.GetAllocatedBytesForCurrentThread ();

			for (int i = 0; i < 10; i++)
				action ();

			return GC.GetAllocatedBytesForCurrentThread () - before;
		}

		static string DecodeCommentWithRfc2047 (ParserOptions options, byte[] bytes, int startIndex, int length)
		{
			var buffer = new byte[length];

			for (int i = 0; i < length; i++)
				buffer[i] = bytes[startIndex + i] == (byte) '\t' ? (byte) ' ' : bytes[startIndex + i];

			return Rfc2047.DecodePhrase (options, buffer, 0, length).Trim ();
		}

		[TestCase ("")]
		[TestCase ("Joe Sixpack")]
		[TestCase (" First  Last ")]
		[TestCase ("A name with more than thirty-two ASCII characters")]
		public void TestPlainAsciiByteNameOnlyAllocatesResult (string name)
		{
			const string surrounding = "=?utf-8?q?ignored?=\"\\\t\r\n";
			var bytes = Encoding.UTF8.GetBytes (surrounding + name + surrounding);
			var input = new ByteArrayAddressInput (bytes);
			Func<string> decodeName = () => input.DecodePhrase (ParserOptions.Default, surrounding.Length, name.Length, out _);
			Func<string> decodeAscii = () => Encoding.UTF8.GetString (bytes, surrounding.Length, name.Length);

			Assert.That (decodeName (), Is.EqualTo (name));
			MeasureAllocation (decodeName);
			MeasureAllocation (decodeAscii);

			Assert.That (MeasureAllocation (decodeName), Is.EqualTo (MeasureAllocation (decodeAscii)));
		}

		[TestCase ("\"\"")]
		[TestCase ("\"Joe Sixpack\"")]
		[TestCase ("\"First\\\" Last\"")]
		[TestCase ("First\\\\Last")]
		[TestCase ("First\tLast")]
		[TestCase ("First\\\r\n Last")]
		[TestCase ("\"A name with more than thirty-two ASCII characters\"")]
		public void TestQuotedAsciiByteNameOnlyAllocatesResult (string name)
		{
			const string surrounding = "=?utf-8?q?ignored?=\"";
			var bytes = Encoding.UTF8.GetBytes (surrounding + name + surrounding);
			var unquoted = MimeUtils.Unquote (bytes, surrounding.Length, name.Length, true);
			var input = new ByteArrayAddressInput (bytes);
			Func<string> decodeName = () => input.DecodePhrase (ParserOptions.Default, surrounding.Length, name.Length, out _);
			Func<string> decodeAscii = () => Encoding.UTF8.GetString (unquoted);

			Assert.That (decodeName (), Is.EqualTo (decodeAscii ()));
			MeasureAllocation (decodeName);
			MeasureAllocation (decodeAscii);

			Assert.That (MeasureAllocation (decodeName), Is.LessThanOrEqualTo (MeasureAllocation (decodeAscii)));
		}

		[TestCase ("\"Ren\u00e9e\\\" M\u00fcller\"", false)]
		[TestCase ("\"Ren\u00e9e\\\" M\u00fcller\"", true)]
		[TestCase ("\"=?utf-8?b?IlxOYW1lIg==?=\"", false)]
		[TestCase ("\"=?utf-8?b?IlxOYW1lIg==?=\"", true)]
		[TestCase ("\"=?unknown?q?Ren=C3=A9e?=\"", false)]
		[TestCase ("\"=?unknown?q?Ren=C3=A9e?=\"", true)]
		public void TestQuotedNameAvoidsCopy (string name, bool byteInput)
		{
			const string surrounding = "=?ignored?=";
			string text = surrounding + name + surrounding;
			var bytes = Encoding.UTF8.GetBytes (text);
			var unquoted = MimeUtils.Unquote (bytes, surrounding.Length, bytes.Length - surrounding.Length * 2, true);
			var byteSource = new ByteArrayAddressInput (bytes);
			var stringSource = new StringAddressInput (text);
			Func<string> decodeName = byteInput
				? () => byteSource.DecodePhrase (ParserOptions.Default, surrounding.Length, bytes.Length - surrounding.Length * 2, out _)
				: () => stringSource.DecodePhrase (ParserOptions.Default, surrounding.Length, name.Length, out _);
			Func<string> decodePhrase = () => Rfc2047.DecodePhrase (ParserOptions.Default, unquoted, 0, unquoted.Length);

			Assert.That (decodeName (), Is.EqualTo (decodePhrase ()));
			MeasureAllocation (decodeName);
			MeasureAllocation (decodePhrase);

			Assert.That (MeasureAllocation (decodeName), Is.EqualTo (MeasureAllocation (decodePhrase)));
		}

		[TestCase ("\"First\\\" Last\"", "First\" Last")]
		[TestCase ("\"Ren\u00e9e M\u00fcller\"", "Ren\u00e9e M\u00fcller")]
		[TestCase ("\"=?utf-8?q?First_Last?=\"", "First Last")]
		public void TestQuotedNameUsesOnlyWrittenBytes (string name, string expected)
		{
			var bytes = Encoding.UTF8.GetBytes (name);
			var buffer = ArrayPool<byte>.Shared.Rent (bytes.Length);
			Array.Fill (buffer, (byte) 0xff);
			ArrayPool<byte>.Shared.Return (buffer);

			var input = new ByteArrayAddressInput (bytes);
			string result = input.DecodePhrase (ParserOptions.Default, 0, bytes.Length, out int codepage);

			buffer = ArrayPool<byte>.Shared.Rent (bytes.Length);
			try {
				Array.Fill (buffer, (byte) 0xff);
				Assert.That (result, Is.EqualTo (expected));
				Assert.That (codepage, Is.EqualTo (65001));
			} finally {
				ArrayPool<byte>.Shared.Return (buffer);
			}
		}

		[TestCase ("")]
		[TestCase ("Joe Sixpack")]
		[TestCase (" First\t Last ")]
		[TestCase (" First\r\n Last ")]
		[TestCase ("A comment with more than thirty-two ASCII characters")]
		public void TestPlainAsciiByteCommentAvoidsDecoder (string name)
		{
			const string surrounding = "=?utf-8?q?ignored?=\t";
			var bytes = Encoding.UTF8.GetBytes (surrounding + name + surrounding);
			var input = new ByteArrayAddressInput (bytes);
			Func<string> decodeName = () => input.DecodeComment (ParserOptions.Default, surrounding.Length, name.Length);
			Func<string> decodeAscii = () => Encoding.UTF8.GetString (bytes, surrounding.Length, name.Length).Replace ('\t', ' ').Trim ();

			Assert.That (decodeName (), Is.EqualTo (decodeAscii ()));
			MeasureAllocation (decodeName);
			MeasureAllocation (decodeAscii);

			Assert.That (MeasureAllocation (decodeName), Is.LessThanOrEqualTo (MeasureAllocation (decodeAscii)));
		}

		[Test]
		public void TestCommentWithTabsAvoidsCopy (
			[Values ("\tRen\u00e9e\tM\u00fcller\t", "=?utf-8?q?First?=\t=?utf-8?q?_Last?=", "\t=?unknown?q?Ren=C3=A9e?=\t", "First\t=\tLast")] string name,
			[Values (0, 128, 129, 1024)] int paddedLength,
			[Values (false, true)] bool byteInput)
		{
			const string surrounding = "=?ignored?=";
			if (paddedLength > 0)
				name = new string ('x', paddedLength - Encoding.UTF8.GetByteCount (name) - 1) + " " + name;

			string text = surrounding + name + surrounding;
			var bytes = Encoding.UTF8.GetBytes (text);
			var normalized = Encoding.UTF8.GetBytes (name.Replace ('\t', ' '));
			var byteSource = new ByteArrayAddressInput (bytes);
			var stringSource = new StringAddressInput (text);
			Func<string> decodeName = byteInput
				? () => byteSource.DecodeComment (ParserOptions.Default, surrounding.Length, bytes.Length - surrounding.Length * 2)
				: () => stringSource.DecodeComment (ParserOptions.Default, surrounding.Length, name.Length);
			Func<string> decodePhrase = () => Rfc2047.DecodePhrase (ParserOptions.Default, normalized, 0, normalized.Length).Trim ();

			Assert.That (decodeName (), Is.EqualTo (decodePhrase ()));
			MeasureAllocation (decodeName);
			MeasureAllocation (decodePhrase);

			Assert.That (MeasureAllocation (decodeName), Is.EqualTo (MeasureAllocation (decodePhrase)));
		}

		[Test]
		public void TestCommentWithTabsUsesOnlyWrittenBytes (
			[Values ("Ren\u00e9e\tM\u00fcller", "=?utf-8?q?First?=\t=?utf-8?q?_Last?=", "First\t=\tLast")] string name,
			[Values (0, 127, 128, 129, 1024)] int paddedLength,
			[Values (false, true)] bool byteInput)
		{
			if (paddedLength > 0)
				name = new string ('x', paddedLength - Encoding.UTF8.GetByteCount (name) - 1) + " " + name;

			var bytes = Encoding.UTF8.GetBytes (name);
			string expected = DecodeCommentWithRfc2047 (ParserOptions.Default, bytes, 0, bytes.Length);
			var buffer = ArrayPool<byte>.Shared.Rent (bytes.Length);
			var other = ArrayPool<byte>.Shared.Rent (bytes.Length);
			Array.Fill (buffer, (byte) 0xff);
			Array.Fill (other, (byte) 0xff);
			ArrayPool<byte>.Shared.Return (buffer);
			ArrayPool<byte>.Shared.Return (other);

			string result = byteInput
				? new ByteArrayAddressInput (bytes).DecodeComment (ParserOptions.Default, 0, bytes.Length)
				: new StringAddressInput (name).DecodeComment (ParserOptions.Default, 0, name.Length);

			buffer = ArrayPool<byte>.Shared.Rent (bytes.Length);
			other = ArrayPool<byte>.Shared.Rent (bytes.Length);
			try {
				Array.Fill (buffer, (byte) 0xff);
				Array.Fill (other, (byte) 0xff);
				Assert.That (result, Is.EqualTo (expected));
				Assert.That (bytes, Is.EqualTo (Encoding.UTF8.GetBytes (name)));
			} finally {
				ArrayPool<byte>.Shared.Return (buffer);
				ArrayPool<byte>.Shared.Return (other);
			}
		}

		[TestCase ("")]
		[TestCase (" Joe  Sixpack ")]
		[TestCase ("\tFirst\t Last\t")]
		[TestCase ("\r\nFirst\r\n Last\r\n")]
		[TestCase ("\v\fFirst\v\fLast\v\f")]
		[TestCase ("\"First\\\" Last\"")]
		[TestCase ("First (Middle) Last")]
		[TestCase ("First\0Last")]
		[TestCase ("First\u007fLast")]
		[TestCase ("First=Last")]
		[TestCase ("\u00a0Ren\u00e9e\u00a0")]
		[TestCase ("\tRen\u00e9e\tM\u00fcller\t")]
		[TestCase ("First\t=\tLast")]
		[TestCase ("=?utf-8?q?First?=\t=?utf-8?q?_Last?=")]
		[TestCase ("=?iso-8859-1?q?Ren=E9e?=")]
		[TestCase ("=?unknown?q?Ren=C3=A9e?=")]
		[TestCase ("=?utf-8?b?IlxOYW1lIg==?=")]
		[TestCase ("=?utf-8?q?incomplete")]
		public void TestCommentDecodingPreservesWhitespaceAndSlice (string name)
		{
			const string surrounding = "\u00e9=?\t";
			var bytes = Encoding.UTF8.GetBytes (surrounding + name + surrounding);
			var original = (byte[]) bytes.Clone ();
			int startIndex = Encoding.UTF8.GetByteCount (surrounding);
			int length = bytes.Length - startIndex * 2;
			var input = new ByteArrayAddressInput (bytes);

			foreach (var compliance in new[] { RfcComplianceMode.Strict, RfcComplianceMode.Loose }) {
				var options = new ParserOptions { Rfc2047ComplianceMode = compliance };

				Assert.That (input.DecodeComment (options, startIndex, length), Is.EqualTo (DecodeCommentWithRfc2047 (options, bytes, startIndex, length)));
				Assert.That (bytes, Is.EqualTo (original));
			}
		}

		[TestCase ("Ren\u00e9e M\u00fcller", true)]
		[TestCase ("=?utf-8?q?Ren=C3=A9e?= =?utf-8?q?_M=C3=BCller?=", true)]
		[TestCase ("=?utf-8?q?Ren=C3=A9e?= =?utf-8?q?_M=C3=BCller?=", false)]
		public void TestUnquotedNameAvoidsCopy (string name, bool byteInput)
		{
			var bytes = Encoding.UTF8.GetBytes (name);
			var input = new ByteArrayAddressInput (bytes);
			var text = new StringAddressInput (name);
			Func<string> decodeName = byteInput
				? () => input.DecodePhrase (ParserOptions.Default, 0, bytes.Length, out _)
				: () => text.DecodePhrase (ParserOptions.Default, 0, name.Length, out _);
			Func<string> decodePhrase = () => Rfc2047.DecodePhrase (ParserOptions.Default, bytes, 0, bytes.Length);

			Assert.That (decodeName (), Is.EqualTo (decodePhrase ()));
			MeasureAllocation (decodeName);
			MeasureAllocation (decodePhrase);

			Assert.That (MeasureAllocation (decodeName), Is.EqualTo (MeasureAllocation (decodePhrase)));
		}

		[Test]
		public void TestByteNameDecodingScanBoundaries ([Values (31, 32, 33, 63, 64, 65, 1024)] int length, [Values (false, true)] bool quoted)
		{
			var bytes = Encoding.UTF8.GetBytes ("\u00e9=\"" + new string ('x', length) + "\"=\u00e9");
			var input = new ByteArrayAddressInput (bytes);
			int startIndex = quoted ? 3 : 4;
			int count = quoted ? length + 2 : length;

			foreach (var charset in new[] { Encoding.UTF8, Encoding.GetEncoding (1252) }) {
				foreach (var compliance in new[] { RfcComplianceMode.Strict, RfcComplianceMode.Loose }) {
					var options = new ParserOptions { CharsetEncoding = charset, Rfc2047ComplianceMode = compliance };

					foreach (int offset in new[] { 0, length / 2, length - 1 }) {
						for (int value = 0; value < 256; value++) {
							bytes[4 + offset] = (byte) value;
							var unquoted = MimeUtils.Unquote (bytes, startIndex, count, true);
							string expected = Rfc2047.DecodePhrase (options, unquoted, 0, unquoted.Length, out int expectedCodepage);
							string actual = input.DecodePhrase (options, startIndex, count, out int codepage);

							Assert.That (actual, Is.EqualTo (expected), $"Offset {offset}, byte {value}");
							Assert.That (codepage, Is.EqualTo (expectedCodepage), $"Offset {offset}, byte {value}");
							Assert.That (input.DecodeComment (options, startIndex, count), Is.EqualTo (DecodeCommentWithRfc2047 (options, bytes, startIndex, count)), $"Comment offset {offset}, byte {value}");
						}

						bytes[4 + offset] = (byte) 'x';
					}
				}
			}
		}

		[TestCase ("")]
		[TestCase ("Joe Sixpack")]
		[TestCase (" First  Last ")]
		[TestCase ("First=Last")]
		[TestCase ("First\0Last")]
		[TestCase ("First\u007fLast")]
		[TestCase ("Ren\u00e9e M\u00fcller")]
		[TestCase ("=?utf-8?q?Ren=C3=A9e?= =?utf-8?q?_M=C3=BCller?=")]
		[TestCase ("=?utf-8?b?IlxOYW1lIg==?=")]
		[TestCase ("\"=?utf-8?b?IlxOYW1lIg==?=\"")]
		[TestCase ("\"=?iso-8859-1?q?Ren=E9e?=\"")]
		[TestCase ("\"=\\?utf-8?q?First?=\"")]
		[TestCase ("\"=?utf-8?q?First?=\r\n\t=?utf-8?q?_Last?=\"")]
		[TestCase ("\"\"")]
		[TestCase ("\"First\\\"Last\\\\\"")]
		[TestCase ("=?unknown?q?Ren=C3=A9e?=")]
		[TestCase ("=?iso-8859-1?q?Ren=E9e?=")]
		[TestCase ("=?utf-8?q?incomplete")]
		[TestCase ("\"First\\\" Last\"")]
		[TestCase ("First\\\\Last")]
		[TestCase ("First\tLast")]
		[TestCase ("First\r\n Last")]
		public void TestNameDecodingPreservesUnquoteOrderAndSlice (string name)
		{
			const string surrounding = "\"\\\t\r\n";
			var bytes = Encoding.UTF8.GetBytes (surrounding + name + surrounding);
			var original = (byte[]) bytes.Clone ();
			int length = bytes.Length - surrounding.Length * 2;
			var input = new ByteArrayAddressInput (bytes);

			foreach (var compliance in new[] { RfcComplianceMode.Strict, RfcComplianceMode.Loose }) {
				var options = new ParserOptions { Rfc2047ComplianceMode = compliance };
				var unquoted = MimeUtils.Unquote (bytes, surrounding.Length, length, true);
				var expected = Rfc2047.DecodePhrase (options, unquoted, 0, unquoted.Length, out int expectedCodepage);
				var actual = input.DecodePhrase (options, surrounding.Length, length, out int codepage);

				Assert.That (actual, Is.EqualTo (expected));
				Assert.That (codepage, Is.EqualTo (expectedCodepage));
				Assert.That (bytes, Is.EqualTo (original));
			}
		}

		[Test]
		public void TestContiguousAddrSpecAvoidsIntermediateStrings ([Values (false, true)] bool byteInput)
		{
			const string contiguous = "first.last@example.com";
			const string commented = "first.last@(ignored)example.com";
			var contiguousBytes = Encoding.UTF8.GetBytes (contiguous);
			var commentedBytes = Encoding.UTF8.GetBytes (commented);
			Func<MailboxAddress> parseContiguous = byteInput ? () => MailboxAddress.Parse (contiguousBytes) : () => MailboxAddress.Parse (contiguous);
			Func<MailboxAddress> parseCommented = byteInput ? () => MailboxAddress.Parse (commentedBytes) : () => MailboxAddress.Parse (commented);

			Assert.That (Describe (parseContiguous ()), Is.EqualTo (Describe (parseCommented ())));
			MeasureAllocation (parseContiguous);
			MeasureAllocation (parseCommented);

			Assert.That (MeasureAllocation (parseContiguous), Is.LessThanOrEqualTo (MeasureAllocation (parseCommented) - 1280));
		}

		[TestCase ("first.last@example.com", "first.last@example.com")]
		[TestCase ("first (comment) . last @ example . com", "first.last@example.com")]
		[TestCase ("\"quoted local\"@example.com", "\"quoted local\"@example.com")]
		[TestCase ("local@example.com.", "local@example.com")]
		[TestCase ("local@example.com (name)", "local@example.com")]
		[TestCase ("local@example. com", "local@example.com")]
		[TestCase ("local@exa (comment) . mple", "local@exa.mple")]
		[TestCase ("local@[127.0.0.1]", "local@[127.0.0.1]")]
		[TestCase ("\u00e9@example.com", "\u00e9@example.com")]
		[TestCase ("local@b\u00fccher.example", "local@b\u00fccher.example")]
		public void TestContiguousAddrSpecAndFallback (string text, string expected)
		{
			var bytes = Encoding.UTF8.GetBytes ("###" + text + "###");

			foreach (var compliance in new[] { RfcComplianceMode.Strict, RfcComplianceMode.Loose, RfcComplianceMode.Looser }) {
				var options = new ParserOptions { AddressParserComplianceMode = compliance };
				var fromString = MailboxAddress.Parse (options, text);
				var fromBytes = MailboxAddress.Parse (options, bytes, 3, bytes.Length - 6);

				Assert.That (fromString.Address, Is.EqualTo (expected));
				Assert.That (Describe (fromString), Is.EqualTo (Describe (fromBytes)));
			}
		}

		[TestCase ("local@example.com", "@", "local", 5, -1)]
		[TestCase ("local@example.com", "c", "local@example", 14, 5)]
		[TestCase ("local@example..com", ".", "local@example", 14, 5)]
		[TestCase ("local@example.com,next", ",", "local@example.com", 17, 5)]
		public void TestContiguousAddrSpecSentinels (string text, string terminators, string expected, int position, int at)
		{
			var sentinels = Encoding.ASCII.GetBytes (terminators);
			var bytes = Encoding.ASCII.GetBytes (text);
			var stringParser = AddressParser.Create (ParserOptions.Default, text);
			var byteParser = AddressParser.Create (ParserOptions.Default, bytes, 0, bytes.Length);

			Assert.That (stringParser.TryParseAddrspec (sentinels, RfcComplianceMode.Strict, true, out var fromString, out int stringAt), Is.True);
			Assert.That (byteParser.TryParseAddrspec (sentinels, RfcComplianceMode.Strict, true, out var fromBytes, out int byteAt), Is.True);
			Assert.That (fromString, Is.EqualTo (expected));
			Assert.That (fromBytes, Is.EqualTo (expected));
			Assert.That (stringParser.Position, Is.EqualTo (position));
			Assert.That (byteParser.Position, Is.EqualTo (position));
			Assert.That (stringAt, Is.EqualTo (at));
			Assert.That (byteAt, Is.EqualTo (at));
		}

		[Test]
		public void TestStringParsingDoesNotTranscodeIgnoredInput ()
		{
			const string shortInput = "(ignored) a@example.com";
			string longInput = "(" + new string ('x', 32768) + ") a@example.com";

			Assert.That (Describe (InternetAddressList.Parse (longInput)), Is.EqualTo (Describe (InternetAddressList.Parse (shortInput))));
			MeasureParsingAllocation (shortInput);
			MeasureParsingAllocation (longInput);

			Assert.That (MeasureParsingAllocation (longInput), Is.LessThanOrEqualTo (MeasureParsingAllocation (shortInput) + 1024));
		}

		[Test]
		public void TestPrefixVersusCompleteInput ()
		{
			const string text = "a@example.com, b@example.com";
			var buffer = Encoding.UTF8.GetBytes ("###" + text + "###");
			int endIndex = buffer.Length - 3;
			int index = 3;

			Assert.That (InternetAddress.TryParse (AddressParserFlags.TryParse, ParserOptions.Default, buffer, ref index, endIndex, 0, out var address), Is.True);
			Assert.That (((MailboxAddress) address).Address, Is.EqualTo ("a@example.com"));
			Assert.That (index, Is.EqualTo (3 + text.IndexOf (',')));
			Assert.That (InternetAddress.TryParse (buffer, 3, text.Length, out _), Is.False);
			Assert.That (MailboxAddress.TryParse (buffer, 3, text.Length, out _), Is.False);
			Assert.That (InternetAddressList.Parse (buffer, 3, text.Length), Has.Count.EqualTo (2));
		}

		[Test]
		public void TestStrictUnterminatedGroupTryParseVersusParse ()
		{
			const string text = "Friends: a@example.com";
			var options = new ParserOptions { AddressParserComplianceMode = RfcComplianceMode.Strict };
			var buffer = Encoding.UTF8.GetBytes ("###" + text + "###");

			Assert.That (InternetAddressList.TryParse (options, buffer, 3, text.Length, out var list), Is.True);
			Assert.That (((GroupAddress) list[0]).Members, Has.Count.EqualTo (1));
			Assert.That (GroupAddress.TryParse (options, buffer, 3, text.Length, out var group), Is.True);
			Assert.That (group.Members, Has.Count.EqualTo (1));

			var ex = Assert.Throws<ParseException> (() => InternetAddressList.Parse (options, buffer, 3, text.Length));
			Assert.That (ex.TokenIndex, Is.EqualTo (3));
			Assert.That (ex.ErrorIndex, Is.EqualTo (3 + text.Length));
			ex = Assert.Throws<ParseException> (() => GroupAddress.Parse (options, buffer, 3, text.Length));
			Assert.That (ex.TokenIndex, Is.EqualTo (3));
			Assert.That (ex.ErrorIndex, Is.EqualTo (3 + text.Length));
		}

		[Test]
		public void TestInternalRecoveryVersusPublicFailure ()
		{
			const string text = "first@example.com, \"Last, First\" (no address), last@example.com";
			var options = new ParserOptions {
				AddressParserComplianceMode = RfcComplianceMode.Strict,
				AllowAddressesWithoutDomain = false
			};
			var buffer = Encoding.UTF8.GetBytes ("###" + text + "###");
			int index = 3;

			Assert.That (InternetAddressList.TryParse (options, buffer, 3, text.Length, out var list), Is.False);
			Assert.That (list, Is.Null);
			Assert.That (InternetAddressList.TryParse (AddressParserFlags.InternalTryParse, options, buffer, ref index, 3 + text.Length, false, 0, out var addresses), Is.True);
			Assert.That (index, Is.EqualTo (3 + text.Length));
			Assert.That (addresses, Has.Count.EqualTo (2));
			Assert.That (((MailboxAddress) addresses[0]).Address, Is.EqualTo ("first@example.com"));
			Assert.That (((MailboxAddress) addresses[1]).Address, Is.EqualTo ("last@example.com"));
		}

		[Test]
		public void TestGroupListLeavesTerminator ()
		{
			const string text = "a@example.com;ignored@example.com";
			var buffer = Encoding.UTF8.GetBytes (text);
			int index = 0;

			Assert.That (InternetAddressList.TryParse (AddressParserFlags.TryParse, ParserOptions.Default, buffer, ref index, buffer.Length, true, 1, out var addresses), Is.True);
			Assert.That (addresses, Has.Count.EqualTo (1));
			Assert.That (index, Is.EqualTo (text.IndexOf (';')));
		}

		[Test]
		public void TestNestedGroupsAndEncodedNames ()
		{
			const string text = "Outer: =?iso-8859-1?q?Andr=E9?=: =?utf-8?b?IlxOYW1lIg==?= <a@xn--bcher-kva.example>;;";
			var options = new ParserOptions { MaxAddressGroupDepth = 2 };
			var list = InternetAddressList.Parse (options, text);
			var outer = (GroupAddress) list[0];
			var inner = (GroupAddress) outer.Members[0];
			var mailbox = (MailboxAddress) inner.Members[0];

			Assert.That (outer.Name, Is.EqualTo ("Outer"));
			Assert.That (inner.Name, Is.EqualTo ("Andr\u00e9"));
			Assert.That (inner.Encoding.CodePage, Is.EqualTo (28591));
			Assert.That (mailbox.Name, Is.EqualTo ("\"\\Name\""));
			Assert.That (mailbox.Address, Is.EqualTo ("a@xn--bcher-kva.example"));
		}

		[TestCase (RfcComplianceMode.Strict, false)]
		[TestCase (RfcComplianceMode.Loose, true)]
		[TestCase (RfcComplianceMode.Looser, true)]
		public void TestNonUtf8LocalPart (RfcComplianceMode compliance, bool expected)
		{
			var buffer = new byte[] { 0xe9, (byte) '@', (byte) 'a' };
			var options = new ParserOptions { AddressParserComplianceMode = compliance };

			Assert.That (MailboxAddress.TryParse (options, buffer, out var mailbox), Is.EqualTo (expected));
			if (expected)
				Assert.That (mailbox.Address, Is.EqualTo ("\u00e9@a"));
			else
				Assert.That (mailbox, Is.Null);
		}

		[Test]
		public void TestStringErrorOffsetsAreUtf8ByteOffsets ()
		{
			const string text = "\"\u00e9\" <a@";
			var options = new ParserOptions { AddressParserComplianceMode = RfcComplianceMode.Strict };
			var buffer = Encoding.UTF8.GetBytes (text);
			var fromString = Assert.Throws<ParseException> (() => InternetAddress.Parse (options, text));
			var fromBytes = Assert.Throws<ParseException> (() => InternetAddress.Parse (options, buffer));

			Assert.That (fromString.TokenIndex, Is.EqualTo (6));
			Assert.That (fromString.ErrorIndex, Is.EqualTo (buffer.Length));
			Assert.That (fromString.TokenIndex, Is.EqualTo (fromBytes.TokenIndex));
			Assert.That (fromString.ErrorIndex, Is.EqualTo (fromBytes.ErrorIndex));
		}

		[TestCase (false)]
		[TestCase (true)]
		public void TestFailurePreservesCursor (bool throwOnError)
		{
			const string text = "Name <a@";
			var buffer = Encoding.UTF8.GetBytes ("###" + text + "###");
			var options = new ParserOptions { AddressParserComplianceMode = RfcComplianceMode.Strict };
			int index = 3;

			if (throwOnError) {
				var ex = Assert.Throws<ParseException> (() => InternetAddress.TryParse (AddressParserFlags.Parse, options, buffer, ref index, 3 + text.Length, 0, out _));
				Assert.That (ex.ErrorIndex, Is.EqualTo (3 + text.Length));
			} else {
				Assert.That (InternetAddress.TryParse (AddressParserFlags.TryParse, options, buffer, ref index, 3 + text.Length, 0, out var address), Is.False);
				Assert.That (address, Is.Null);
			}

			Assert.That (index, Is.EqualTo (3 + text.Length));
		}

		[TestCase ("", "", 65001, false)]
		[TestCase ("", "", 65001, true)]
		[TestCase ("\"Last, First\"", "Last, First", 65001, false)]
		[TestCase ("\"Last, First\"", "Last, First", 65001, true)]
		[TestCase ("=?iso-8859-1?q?Andr=E9?=", "Andr\u00e9", 28591, false)]
		[TestCase ("=?iso-8859-1?q?Andr=E9?=", "Andr\u00e9", 28591, true)]
		[TestCase ("\"=?utf-8?b?IlxOYW1lIg==?=\"", "\"\\Name\"", 65001, false)]
		[TestCase ("\"=?utf-8?b?IlxOYW1lIg==?=\"", "\"\\Name\"", 65001, true)]
		[TestCase ("=?utf-8?q?First?= \t =?utf-8?q?_Last?=", "First Last", 65001, false)]
		[TestCase ("=?utf-8?q?First?= \t =?utf-8?q?_Last?=", "First Last", 65001, true)]
		[TestCase ("\"Unclosed", "Unclosed", 65001, false)]
		[TestCase ("\"Unclosed", "Unclosed", 65001, true)]
		public void TestPhraseDecodingForMailboxAndGroup (string phrase, string name, int codepage, bool group)
		{
			string text = group ? phrase + ": a@example.com;" : phrase + " <a@example.com>";
			var address = InternetAddress.Parse (text);

			Assert.That (address.Name, Is.EqualTo (name));
			Assert.That (address.Encoding.CodePage, Is.EqualTo (codepage));
			Assert.That (address is GroupAddress, Is.EqualTo (group));
			if (group)
				Assert.That (((MailboxAddress) ((GroupAddress) address).Members[0]).Address, Is.EqualTo ("a@example.com"));
			else
				Assert.That (((MailboxAddress) address).Address, Is.EqualTo ("a@example.com"));
		}

		[TestCase (RfcComplianceMode.Strict)]
		[TestCase (RfcComplianceMode.Loose)]
		[TestCase (RfcComplianceMode.Looser)]
		public void TestAddrSpecReclassifiedAsDisplayName (RfcComplianceMode compliance)
		{
			const string text = "name@example.com (Comment) <actual@example.com>";
			var options = new ParserOptions { AddressParserComplianceMode = compliance };

			if (compliance == RfcComplianceMode.Strict) {
				Assert.That (InternetAddress.TryParse (options, text, out _), Is.False);
				var ex = Assert.Throws<ParseException> (() => InternetAddress.Parse (options, text));
				Assert.That (ex.TokenIndex, Is.EqualTo (0));
				Assert.That (ex.ErrorIndex, Is.EqualTo (text.IndexOf ('<')));
			} else {
				var address = (MailboxAddress) InternetAddress.Parse (options, text);
				Assert.That (address.Name, Is.EqualTo ("name@example.com (Comment)"));
				Assert.That (address.Address, Is.EqualTo ("actual@example.com"));
			}
		}

		[Test]
		public void TestInternalGroupRecoveryLeavesGroupTerminator ()
		{
			const string text = "\"bad, name\" (no address); next@example.com";
			var buffer = Encoding.UTF8.GetBytes (text);
			var options = new ParserOptions {
				AddressParserComplianceMode = RfcComplianceMode.Strict,
				AllowAddressesWithoutDomain = false
			};
			int index = 0;

			Assert.That (InternetAddressList.TryParse (AddressParserFlags.InternalTryParse, options, buffer, ref index, buffer.Length, true, 1, out var members), Is.True);
			Assert.That (members, Is.Empty);
			Assert.That (index, Is.EqualTo (text.IndexOf (';')));
		}
	}
}

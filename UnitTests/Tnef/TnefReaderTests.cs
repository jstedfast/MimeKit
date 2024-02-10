//
// TnefReaderTests.cs
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

using MimeKit.Tnef;

namespace UnitTests.Tnef {
	[TestFixture]
	public class TnefReaderTests
	{
		static readonly string DataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "tnef");

		[Test]
		public void TestArgumentExceptions ()
		{
			using (var stream = File.OpenRead (Path.Combine (DataDir, "winmail.tnef"))) {
				Assert.Throws<ArgumentNullException> (() => new TnefReader (null, 0, TnefComplianceMode.Strict));
				Assert.Throws<ArgumentOutOfRangeException> (() => new TnefReader (stream, -1, TnefComplianceMode.Strict));

				using (var reader = new TnefReader (stream, 1252, TnefComplianceMode.Strict)) {
					var buffer = new byte[16];

					Assert.Throws<ArgumentNullException> (() => reader.ReadAttributeRawValue (null, 0, buffer.Length));
					Assert.Throws<ArgumentOutOfRangeException> (() => reader.ReadAttributeRawValue (buffer, -1, buffer.Length));
					Assert.Throws<ArgumentOutOfRangeException> (() => reader.ReadAttributeRawValue (buffer, 0, -1));
				}
			}
		}

		[Test]
		public void TestSetComplianceError ()
		{
			using (var stream = File.OpenRead (Path.Combine (DataDir, "winmail.tnef"))) {
				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Strict)) {
					foreach (TnefComplianceStatus error in Enum.GetValues (typeof (TnefComplianceStatus))) {
						if (error == TnefComplianceStatus.Compliant) {
							Assert.DoesNotThrow (() => reader.SetComplianceError (error));
						} else {
							Assert.Throws<TnefException> (() => reader.SetComplianceError (error));
						}
					}
				}
			}
		}

		[Test]
		public void TestTruncatedHeader ()
		{
			using (var stream = new MemoryStream ()) {
				Assert.Throws<TnefException> (() => new TnefReader (stream, 0, TnefComplianceMode.Strict));

				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					Assert.That (reader.ComplianceStatus, Is.EqualTo (TnefComplianceStatus.StreamTruncated));

					reader.ResetComplianceStatus ();
					Assert.That (reader.ComplianceStatus, Is.EqualTo (TnefComplianceStatus.Compliant));
				}
			}
		}

		[Test]
		public void TestTruncatedHeaderAfterSignature ()
		{
			using (var stream = new MemoryStream ()) {
				var invalidSignature = BitConverter.GetBytes (0x223e9f78);

				stream.Write (invalidSignature, 0, invalidSignature.Length);
				stream.WriteByte (0);

				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					Assert.That (reader.ComplianceStatus, Is.EqualTo (TnefComplianceStatus.StreamTruncated));
				}
			}
		}

		[Test]
		public void TestInvalidSignatureLoose ()
		{
			using (var stream = new MemoryStream ()) {
				var invalidSignature = BitConverter.GetBytes (0x223e9f79);

				stream.Write (invalidSignature, 0, invalidSignature.Length);
				stream.WriteByte (0);
				stream.WriteByte (0);
				stream.Position = 0;

				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					Assert.That (reader.ComplianceStatus, Is.EqualTo (TnefComplianceStatus.InvalidTnefSignature));
				}
			}
		}

		[Test]
		public void TestInvalidSignatureStrict ()
		{
			using (var stream = new MemoryStream ()) {
				var invalidSignature = BitConverter.GetBytes (0x223e9f79);
				TnefReader reader;

				stream.Write (invalidSignature, 0, invalidSignature.Length);
				stream.WriteByte (0);
				stream.WriteByte (0);
				stream.Position = 0;

				try {
					reader = new TnefReader (stream, 0, TnefComplianceMode.Strict);
					Assert.Fail ("new TnefReader should have thrown TnefException");
				} catch (TnefException ex) {
					Assert.That (ex.Error, Is.EqualTo (TnefComplianceStatus.InvalidTnefSignature), "Error");
				} catch (Exception ex) {
					Assert.Fail ($"new TnefReader should have thrown TnefException, not {ex}");
				}
			}
		}

		[Test]
		public void TestInvalidOemCodepageLoose ()
		{
			using (var stream = new MemoryStream ()) {
				stream.Write (BitConverter.GetBytes (0x223e9f78), 0, 4);
				stream.WriteByte (0);
				stream.WriteByte (0);
				stream.WriteByte ((byte) TnefAttributeLevel.Message);
				stream.Write (BitConverter.GetBytes ((int) TnefAttributeTag.OemCodepage), 0, 4);
				stream.Write (BitConverter.GetBytes (4), 0, 4);
				stream.Write (BitConverter.GetBytes (1), 0, 4);

				stream.Position = 0;

				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					Assert.That (reader.ReadNextAttribute (), Is.True, "ReadNextAttribute");
					Assert.That (reader.AttributeTag, Is.EqualTo (TnefAttributeTag.OemCodepage), "AttributeTag");
					Assert.That (reader.ComplianceStatus, Is.EqualTo (TnefComplianceStatus.InvalidMessageCodepage));
				}
			}
		}

		[Test]
		public void TestInvalidOemCodepageStrict ()
		{
			using (var stream = new MemoryStream ()) {
				stream.Write (BitConverter.GetBytes (0x223e9f78), 0, 4);
				stream.WriteByte (0);
				stream.WriteByte (0);
				stream.WriteByte ((byte) TnefAttributeLevel.Message);
				stream.Write (BitConverter.GetBytes ((int) TnefAttributeTag.OemCodepage), 0, 4);
				stream.Write (BitConverter.GetBytes (4), 0, 4);
				stream.Write (BitConverter.GetBytes (1), 0, 4);

				stream.Position = 0;

				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Strict)) {
					try {
						reader.ReadNextAttribute ();
						Assert.Fail ("ReadNextAttribute should have thrown TnefException");
					} catch (TnefException ex) {
						Assert.That (ex.Error, Is.EqualTo (TnefComplianceStatus.InvalidMessageCodepage), "Error");
					} catch (Exception ex) {
						Assert.Fail ($"ReadNextAttribute should have thrown TnefException, not {ex}");
					}
				}
			}
		}

		[Test]
		public void TestInvalidTnefVersionLoose ()
		{
			using (var stream = new MemoryStream ()) {
				stream.Write (BitConverter.GetBytes (0x223e9f78), 0, 4);
				stream.WriteByte (0);
				stream.WriteByte (0);
				stream.WriteByte ((byte) TnefAttributeLevel.Message);
				stream.Write (BitConverter.GetBytes ((int) TnefAttributeTag.TnefVersion), 0, 4);
				stream.Write (BitConverter.GetBytes (4), 0, 4);
				stream.Write (BitConverter.GetBytes (1), 0, 4);

				stream.Position = 0;

				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					Assert.That (reader.ReadNextAttribute (), Is.True, "ReadNextAttribute");
					Assert.That (reader.AttributeTag, Is.EqualTo (TnefAttributeTag.TnefVersion), "AttributeTag");
					reader.TnefPropertyReader.ReadValueAsInt32 ();
					Assert.That (reader.TnefVersion, Is.EqualTo (1), "TnefVersion");
					Assert.That (reader.ComplianceStatus, Is.EqualTo (TnefComplianceStatus.InvalidTnefVersion));
				}
			}
		}

		[Test]
		public void TestInvalidTnefVersionStrict ()
		{
			using (var stream = new MemoryStream ()) {
				stream.Write (BitConverter.GetBytes (0x223e9f78), 0, 4);
				stream.WriteByte (0);
				stream.WriteByte (0);
				stream.WriteByte ((byte) TnefAttributeLevel.Message);
				stream.Write (BitConverter.GetBytes ((int) TnefAttributeTag.TnefVersion), 0, 4);
				stream.Write (BitConverter.GetBytes (4), 0, 4);
				stream.Write (BitConverter.GetBytes (1), 0, 4);

				stream.Position = 0;

				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Strict)) {
					try {
						reader.ReadNextAttribute ();
						Assert.Fail ("ReadNextAttribute should have thrown TnefException");
					} catch (TnefException ex) {
						Assert.That (ex.Error, Is.EqualTo (TnefComplianceStatus.InvalidTnefVersion), "Error");
					} catch (Exception ex) {
						Assert.Fail ($"ReadNextAttribute should have thrown TnefException, not {ex}");
					}
				}
			}
		}

		[Test]
		public void TestNegativeAttributeRawValueLengthLoose ()
		{
			using (var stream = new MemoryStream ()) {
				stream.Write (BitConverter.GetBytes (0x223e9f78), 0, 4);
				stream.WriteByte (0);
				stream.WriteByte (0);
				stream.WriteByte ((byte) TnefAttributeLevel.Message);
				stream.Write (BitConverter.GetBytes ((int) TnefAttributeTag.TnefVersion), 0, 4);
				stream.Write (BitConverter.GetBytes (-4), 0, 4);
				stream.Write (BitConverter.GetBytes (65536), 0, 4);
				stream.WriteByte ((byte) TnefAttributeLevel.Message);
				stream.Write (BitConverter.GetBytes ((int) TnefAttributeTag.OemCodepage), 0, 4);
				stream.Write (BitConverter.GetBytes (4), 0, 4);
				stream.Write (BitConverter.GetBytes (28591), 0, 4);
				stream.Position = 0;

				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					Assert.That (reader.ReadNextAttribute (), Is.False, "ReadNextAttribute");
					Assert.That (reader.AttributeTag, Is.EqualTo (TnefAttributeTag.TnefVersion), "AttributeTag");
					Assert.That (reader.ComplianceStatus, Is.EqualTo (TnefComplianceStatus.InvalidAttributeLength));
				}
			}
		}

		[Test]
		public void TestNegativeAttributeRawValueLengthStrict ()
		{
			using (var stream = new MemoryStream ()) {
				stream.Write (BitConverter.GetBytes (0x223e9f78), 0, 4);
				stream.WriteByte (0);
				stream.WriteByte (0);
				stream.WriteByte ((byte) TnefAttributeLevel.Message);
				stream.Write (BitConverter.GetBytes ((int) TnefAttributeTag.TnefVersion), 0, 4);
				stream.Write (BitConverter.GetBytes (-4), 0, 4);
				stream.Write (BitConverter.GetBytes (65536), 0, 4);
				stream.WriteByte ((byte) TnefAttributeLevel.Message);
				stream.Write (BitConverter.GetBytes ((int) TnefAttributeTag.OemCodepage), 0, 4);
				stream.Write (BitConverter.GetBytes (4), 0, 4);
				stream.Write (BitConverter.GetBytes (28591), 0, 4);
				stream.Position = 0;

				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Strict)) {
					try {
						reader.ReadNextAttribute ();
						Assert.Fail ("ReadNextAttribute should have thrown TnefException");
					} catch (TnefException ex) {
						Assert.That (ex.Error, Is.EqualTo (TnefComplianceStatus.InvalidAttributeLength), "Error");
					} catch (Exception ex) {
						Assert.Fail ($"ReadNextAttribute should have thrown TnefException, not {ex}");
					}
				}
			}
		}

		[Test]
		public void TestReadAfterClose ()
		{
			using (var stream = new MemoryStream ()) {
				stream.Write (BitConverter.GetBytes (0x223e9f78), 0, 4);
				stream.WriteByte (0);
				stream.WriteByte (0);
				stream.WriteByte ((byte) TnefAttributeLevel.Message);
				stream.Write (BitConverter.GetBytes ((int) TnefAttributeTag.TnefVersion), 0, 4);
				stream.Write (BitConverter.GetBytes (28), 0, 4);
				stream.Write (BitConverter.GetBytes (65536), 0, 4);
				stream.Position = 0;

				var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose);
				reader.Close ();

				Assert.Throws<ObjectDisposedException> (() => reader.ReadNextAttribute ());
			}
		}

		[Test]
		public void TestReadAttributeRawValueTruncatedLoose ()
		{
			using (var stream = new MemoryStream ()) {
				stream.Write (BitConverter.GetBytes (0x223e9f78), 0, 4);
				stream.WriteByte (0);
				stream.WriteByte (0);
				stream.WriteByte ((byte) TnefAttributeLevel.Message);
				stream.Write (BitConverter.GetBytes ((int) TnefAttributeTag.MessageId), 0, 4);
				stream.Write (BitConverter.GetBytes (28), 0, 4);
				stream.Write (BitConverter.GetBytes (0xFFFFFFFF), 0, 4);
				stream.Position = 0;

				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					Assert.That (reader.ReadNextAttribute (), Is.True, "ReadNextAttribute");
					Assert.That (reader.AttributeTag, Is.EqualTo (TnefAttributeTag.MessageId), "AttributeTag");

					var buffer = new byte[28];
					int nread, n = 0;

					do {
						if ((nread = reader.ReadAttributeRawValue (buffer, n, buffer.Length - n)) == 0)
							break;

						n += nread;
					} while (n < 28);

					Assert.That (reader.ComplianceStatus, Is.EqualTo (TnefComplianceStatus.StreamTruncated));
				}
			}
		}

		[Test]
		public void TestReadAttributeRawValueTruncatedStrict ()
		{
			using (var stream = new MemoryStream ()) {
				stream.Write (BitConverter.GetBytes (0x223e9f78), 0, 4);
				stream.WriteByte (0);
				stream.WriteByte (0);
				stream.WriteByte ((byte) TnefAttributeLevel.Message);
				stream.Write (BitConverter.GetBytes ((int) TnefAttributeTag.MessageId), 0, 4);
				stream.Write (BitConverter.GetBytes (28), 0, 4);
				stream.Write (BitConverter.GetBytes (0xFFFFFFFF), 0, 4);
				stream.Position = 0;

				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Strict)) {
					Assert.That (reader.ReadNextAttribute (), Is.True, "ReadNextAttribute");
					Assert.That (reader.AttributeTag, Is.EqualTo (TnefAttributeTag.MessageId), "AttributeTag");

					var buffer = new byte[28];
					int n;

					n = reader.ReadAttributeRawValue (buffer, 0, buffer.Length);

					try {
						reader.ReadAttributeRawValue (buffer, n, buffer.Length - n);
						Assert.Fail ("ReadAttributeRawValue should have thrown TnefException");
					} catch (TnefException ex) {
						Assert.That (ex.Error, Is.EqualTo (TnefComplianceStatus.StreamTruncated), "Error");
					} catch (Exception ex) {
						Assert.Fail ($"ReadAttributeRawValue should have thrown TnefException, not {ex}");
					}
				}
			}
		}

		[Test]
		public void TestReadInt32 ()
		{
			using (var stream = new MemoryStream ()) {
				var signature = BitConverter.GetBytes (0x223e9f78);

				stream.Write (signature, 0, signature.Length);
				stream.WriteByte (0);
				stream.WriteByte (0);

				var buffer = BitConverter.GetBytes (1060);
				stream.Write (buffer, 0, buffer.Length);
				stream.Position = 0;

				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					var value = reader.ReadInt32 ();

					Assert.That (value, Is.EqualTo (1060));
				}
			}
		}

		[Test]
		public void TestReadInt64 ()
		{
			using (var stream = new MemoryStream ()) {
				var signature = BitConverter.GetBytes (0x223e9f78);

				stream.Write (signature, 0, signature.Length);
				stream.WriteByte (0);
				stream.WriteByte (0);

				var buffer = BitConverter.GetBytes ((long) 1060);
				stream.Write (buffer, 0, buffer.Length);
				stream.Position = 0;

				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					var value = reader.ReadInt64 ();

					Assert.That (value, Is.EqualTo (1060));
				}
			}
		}

		[Test]
		public void TestReadDouble ()
		{
			using (var stream = new MemoryStream ()) {
				var signature = BitConverter.GetBytes (0x223e9f78);

				stream.Write (signature, 0, signature.Length);
				stream.WriteByte (0);
				stream.WriteByte (0);

				var buffer = BitConverter.GetBytes (1024.1024);
				stream.Write (buffer, 0, buffer.Length);
				stream.Position = 0;

				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					var value = reader.ReadDouble ();

					Assert.That (value, Is.EqualTo (1024.1024));
				}
			}
		}

		[Test]
		public void TestReadSingle ()
		{
			using (var stream = new MemoryStream ()) {
				var signature = BitConverter.GetBytes (0x223e9f78);

				stream.Write (signature, 0, signature.Length);
				stream.WriteByte (0);
				stream.WriteByte (0);

				var buffer = BitConverter.GetBytes ((float) 1024.1024);
				stream.Write (buffer, 0, buffer.Length);
				stream.Position = 0;

				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					var value = reader.ReadSingle ();

					Assert.That (value, Is.EqualTo ((float) 1024.1024));
				}
			}
		}

		[Test]
		public void TestSeekTruncatedLoose ()
		{
			using (var stream = new MemoryStream ()) {
				stream.Write (BitConverter.GetBytes (0x223e9f78), 0, 4);
				stream.WriteByte (0);
				stream.WriteByte (0);
				stream.WriteByte ((byte) TnefAttributeLevel.Message);
				stream.Write (BitConverter.GetBytes ((int) TnefAttributeTag.TnefVersion), 0, 4);
				stream.Write (BitConverter.GetBytes (4), 0, 4);
				stream.Write (BitConverter.GetBytes (65536), 0, 4);
				stream.WriteByte ((byte) TnefAttributeLevel.Message);
				stream.Write (BitConverter.GetBytes ((int) TnefAttributeTag.OemCodepage), 0, 4);
				stream.Write (BitConverter.GetBytes (4), 0, 4);
				stream.Write (BitConverter.GetBytes (28591), 0, 4);
				stream.Position = 0;

				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
					Assert.That (reader.Seek (64), Is.False, "Seek");
					Assert.That (reader.ComplianceStatus, Is.EqualTo (TnefComplianceStatus.StreamTruncated));
				}
			}
		}

		[Test]
		public void TestSeekTruncatedStrict ()
		{
			using (var stream = new MemoryStream ()) {
				stream.Write (BitConverter.GetBytes (0x223e9f78), 0, 4);
				stream.WriteByte (0);
				stream.WriteByte (0);
				stream.WriteByte ((byte) TnefAttributeLevel.Message);
				stream.Write (BitConverter.GetBytes ((int) TnefAttributeTag.TnefVersion), 0, 4);
				stream.Write (BitConverter.GetBytes (4), 0, 4);
				stream.Write (BitConverter.GetBytes (65536), 0, 4);
				stream.WriteByte ((byte) TnefAttributeLevel.Message);
				stream.Write (BitConverter.GetBytes ((int) TnefAttributeTag.OemCodepage), 0, 4);
				stream.Write (BitConverter.GetBytes (4), 0, 4);
				stream.Write (BitConverter.GetBytes (28591), 0, 4);
				stream.Position = 0;

				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Strict)) {
					try {
						reader.Seek (64);
						Assert.Fail ("Seek should have thrown TnefException");
					} catch (TnefException ex) {
						Assert.That (ex.Error, Is.EqualTo (TnefComplianceStatus.StreamTruncated), "Error");
					} catch (Exception ex) {
						Assert.Fail ($"Seek should have thrown TnefException, not {ex}");
					}
				}
			}
		}
	}
}

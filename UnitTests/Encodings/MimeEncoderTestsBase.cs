//
// MimeEncoderTestsBase.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2025 .NET Foundation and Contributors
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
using System.Reflection;
using System.Security.Cryptography;

using MimeKit;
using MimeKit.IO;
using MimeKit.Utils;
using MimeKit.Encodings;
using MimeKit.IO.Filters;

namespace UnitTests.Encodings
{
	public class MimeEncoderTestsBase
	{
		protected static readonly string dataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "encoders");
		protected static readonly byte[] wikipedia_unix;
		protected static readonly byte[] wikipedia_dos;
		protected static readonly byte[] photo;

		static MimeEncoderTestsBase ()
		{
			using (var memory = new MemoryStream ()) {
				using (var filtered = new FilteredStream (memory)) {
					filtered.Add (new Dos2UnixFilter ());

					using (var file = File.OpenRead (Path.Combine (dataDir, "wikipedia.txt")))
						file.CopyTo (filtered, 4096);

					filtered.Flush ();
				}

				wikipedia_unix = memory.ToArray ();
			}

			using (var memory = new MemoryStream ()) {
				using (var filtered = new FilteredStream (memory)) {
					filtered.Add (new Unix2DosFilter ());

					using (var file = File.OpenRead (Path.Combine (dataDir, "wikipedia.txt")))
						file.CopyTo (filtered, 4096);

					filtered.Flush ();
				}

				wikipedia_dos = memory.ToArray ();
			}

			photo = File.ReadAllBytes (Path.Combine (dataDir, "photo.jpg"));
		}

		protected static void AssertArgumentExceptions (IMimeEncoder encoder)
		{
			var output = Array.Empty<byte> ();

			Assert.Throws<ArgumentNullException> (() => encoder.Encode (null, 0, 0, output));
			Assert.Throws<ArgumentOutOfRangeException> (() => encoder.Encode (Array.Empty<byte> (), -1, 0, output));
			Assert.Throws<ArgumentOutOfRangeException> (() => encoder.Encode (new byte[1], 0, 10, output));
			Assert.Throws<ArgumentNullException> (() => encoder.Encode (new byte[1], 0, 1, null));
			Assert.Throws<ArgumentException> (() => encoder.Encode (new byte[1], 0, 1, output));

			Assert.Throws<ArgumentNullException> (() => encoder.Flush (null, 0, 0, output));
			Assert.Throws<ArgumentOutOfRangeException> (() => encoder.Flush (Array.Empty<byte> (), -1, 0, output));
			Assert.Throws<ArgumentOutOfRangeException> (() => encoder.Flush (new byte[1], 0, 10, output));
			Assert.Throws<ArgumentNullException> (() => encoder.Flush (new byte[1], 0, 1, null));
			Assert.Throws<ArgumentException> (() => encoder.Flush (new byte[1], 0, 1, output));
		}

		static object GetEnumValue (Type type, int value)
		{
			return Enum.Parse (type, value.ToString ());
		}

		static void SetRandomState (IMimeEncoder encoder)
		{
			var random = new Random ();

			foreach (var field in encoder.GetType ().GetFields (BindingFlags.NonPublic | BindingFlags.Instance)) {
				if ((field.Attributes & FieldAttributes.InitOnly) != 0)
					continue;

				if (field.FieldType.IsEnum) {
					field.SetValue (encoder, GetEnumValue (field.FieldType, random.Next (1, 255)));
				} else if (field.FieldType == typeof (int)) {
					field.SetValue (encoder, random.Next (1, int.MaxValue));
				} else if (field.FieldType == typeof (uint)) {
					field.SetValue (encoder, (uint) random.Next (1, int.MaxValue));
				} else if (field.FieldType == typeof (bool)) {
					field.SetValue (encoder, true);
				} else if (field.FieldType == typeof (byte)) {
					field.SetValue (encoder, (byte) random.Next (1, 255));
				} else if (field.FieldType == typeof (short)) {
					field.SetValue (encoder, (short) random.Next (1, short.MaxValue));
				} else if (field.FieldType == typeof (Crc32)) {
					var crc = (Crc32) field.GetValue (encoder);
					var buf = new byte[100];

					using (var rng = RandomNumberGenerator.Create ())
						rng.GetBytes (buf);

					crc.Update (buf, 0, buf.Length);
				} else {
					Assert.Fail ($"Unknown field type: {encoder.GetType ().Name}.{field.Name}");
				}
			}
		}

		static void AssertState (IMimeEncoder encoder, IMimeEncoder clone)
		{
			foreach (var field in encoder.GetType ().GetFields (BindingFlags.NonPublic | BindingFlags.Instance)) {
				var expected = field.GetValue (encoder);
				var actual = field.GetValue (clone);

				if (expected.GetType () == typeof (Crc32)) {
					var crc0 = (Crc32) expected;
					var crc1 = (Crc32) actual;

					Assert.That (crc1.Checksum, Is.EqualTo (crc0.Checksum), $"The cloned {encoder.GetType ().Name}.{field.Name} does not match.");
				} else {
					Assert.That (actual, Is.EqualTo (expected), $"The cloned {encoder.GetType ().Name}.{field.Name} does not match.");
				}
			}
		}

		protected static void CloneAndAssert (IMimeEncoder encoder)
		{
			// first, set some random state
			SetRandomState (encoder);

			var clone = encoder.Clone ();

			Assert.That (clone.Encoding, Is.EqualTo (encoder.Encoding));

			AssertState (encoder, clone);
		}

		protected static void ResetAndAssert (IMimeEncoder encoder)
		{
			// clone the encoder at it's initial state
			var clone = encoder.Clone ();

			// set some random state on the clone
			SetRandomState (clone);

			// reset the clone and make sure it matches
			clone.Reset ();

			AssertState (encoder, clone);
		}

		protected static void TestEncoder (IMimeEncoder encoder, string fileName, byte[] rawData, string encodedFile, int bufferSize)
		{
			int n;

			using (var original = new MemoryStream ()) {
				using (var file = File.OpenRead (Path.Combine (dataDir, encodedFile))) {
					using (var filtered = new FilteredStream (original)) {
						filtered.Add (new Dos2UnixFilter ());
						file.CopyTo (filtered, 4096);
						filtered.Flush ();
					}
				}

				using (var encoded = new MemoryStream ()) {
					if (encoder.Encoding == ContentEncoding.UUEncode) {
						var begin = Encoding.ASCII.GetBytes ($"begin 644 {fileName}\n");
						encoded.Write (begin, 0, begin.Length);
					}

					using (var filtered = new FilteredStream (encoded)) {
						filtered.Add (new EncoderFilter (encoder));

						using (var memory = new MemoryStream (rawData, false)) {
							var buffer = new byte[bufferSize];

							while ((n = memory.Read (buffer, 0, bufferSize)) > 0)
								filtered.Write (buffer, 0, n);
						}

						filtered.Flush ();
					}

					if (encoder.Encoding == ContentEncoding.UUEncode) {
						var end = Encoding.ASCII.GetBytes ("end\n");
						encoded.Write (end, 0, end.Length);
					}

					var expectedLength = (int) original.Length;
					var expected = original.GetBuffer ();
					var actual = encoded.GetBuffer ();

					Assert.That (encoded.Length, Is.EqualTo (expectedLength), "Encoded length is incorrect.");

					for (int i = 0; i < expectedLength; i++)
						Assert.That (actual[i], Is.EqualTo (expected[i]), $"The byte at offset {i} does not match.");
				}
			}
		}

		protected static void TestEncoderFlush (IMimeEncoder encoder, string fileName, byte[] rawData, string encodedFile)
		{
			using (var original = new MemoryStream ()) {
				using (var file = File.OpenRead (Path.Combine (dataDir, encodedFile))) {
					using (var filtered = new FilteredStream (original)) {
						filtered.Add (new Dos2UnixFilter ());
						file.CopyTo (filtered, 4096);
						filtered.Flush ();
					}
				}

				int outputLength = encoder.EstimateOutputLength (rawData.Length);
				byte[] encoded, begin, end;

				if (encoder.Encoding == ContentEncoding.UUEncode) {
					begin = Encoding.ASCII.GetBytes ($"begin 644 {fileName}\n");
					end = Encoding.ASCII.GetBytes ("end\n");
					outputLength += begin.Length + end.Length;
				} else {
					begin = Array.Empty<byte> ();
					end = Array.Empty<byte> ();
				}

				encoded = new byte[outputLength];

				int encodedLength = encoder.Flush (rawData, 0, rawData.Length, encoded);

				if (begin.Length > 0) {
					// shift the encoded data to the right to make room for the "begin" line
					Buffer.BlockCopy (encoded, 0, encoded, begin.Length, encodedLength);
					Buffer.BlockCopy (begin, 0, encoded, 0, begin.Length);
					encodedLength += begin.Length;
				}

				if (end.Length > 0) {
					// append the "end" line to the end of the encoded data
					Buffer.BlockCopy (end, 0, encoded, encodedLength, end.Length);
					encodedLength += end.Length;
				}

				int expectedLength = (int) original.Length;
				var expected = original.GetBuffer ();

				Assert.That (encodedLength, Is.EqualTo (expectedLength), "Encoded length is incorrect.");

				for (int i = 0; i < expectedLength; i++)
					Assert.That (encoded[i], Is.EqualTo (expected[i]), $"The byte at offset {i} does not match.");
			}
		}
	}
}

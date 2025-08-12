//
// MimeDecoderTestsBase.cs
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

namespace UnitTests.Encodings {
	public class MimeDecoderTestsBase
	{
		protected static readonly string dataDir = Path.Combine (TestHelper.ProjectDir, "TestData", "encoders");
		protected static readonly byte[] wikipedia_unix;
		protected static readonly byte[] wikipedia_dos;
		protected static readonly byte[] photo;

		static MimeDecoderTestsBase ()
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

		protected static void AssertArgumentExceptions (IMimeDecoder decoder)
		{
			var output = Array.Empty<byte> ();

			Assert.Throws<ArgumentNullException> (() => decoder.Decode (null, 0, 0, output));
			Assert.Throws<ArgumentOutOfRangeException> (() => decoder.Decode (Array.Empty<byte> (), -1, 0, output));
			Assert.Throws<ArgumentOutOfRangeException> (() => decoder.Decode (new byte[1], 0, 10, output));
			Assert.Throws<ArgumentNullException> (() => decoder.Decode (new byte[1], 0, 1, null));
			Assert.Throws<ArgumentException> (() => decoder.Decode (new byte[1], 0, 1, output));
		}

		static object GetEnumValue (Type type, int value)
		{
			return Enum.Parse (type, value.ToString ());
		}

		static void SetRandomState (IMimeDecoder decoder)
		{
			var random = new Random ();

			foreach (var field in decoder.GetType ().GetFields (BindingFlags.NonPublic | BindingFlags.Instance)) {
				if ((field.Attributes & FieldAttributes.InitOnly) != 0)
					continue;

				if (field.FieldType.IsEnum) {
					field.SetValue (decoder, GetEnumValue (field.FieldType, random.Next (1, 255)));
				} else if (field.FieldType == typeof (int)) {
					field.SetValue (decoder, random.Next (1, int.MaxValue));
				} else if (field.FieldType == typeof (uint)) {
					field.SetValue (decoder, (uint) random.Next (1, int.MaxValue));
				} else if (field.FieldType == typeof (bool)) {
					field.SetValue (decoder, true);
				} else if (field.FieldType == typeof (byte)) {
					field.SetValue (decoder, (byte) random.Next (1, 255));
				} else if (field.FieldType == typeof (short)) {
					field.SetValue (decoder, (short) random.Next (1, short.MaxValue));
				} else if (field.FieldType == typeof (Crc32)) {
					var crc = (Crc32) field.GetValue (decoder);
					var buf = new byte[100];

					using (var rng = RandomNumberGenerator.Create ())
						rng.GetBytes (buf);

					crc.Update (buf, 0, buf.Length);
				} else {
					Assert.Fail ($"Unknown field type: {decoder.GetType ().Name}.{field.Name}");
				}
			}
		}

		static void AssertState (IMimeDecoder decoder, IMimeDecoder clone)
		{
			foreach (var field in decoder.GetType ().GetFields (BindingFlags.NonPublic | BindingFlags.Instance)) {
				var expected = field.GetValue (decoder);
				var actual = field.GetValue (clone);

				if (expected.GetType () == typeof (Crc32)) {
					var crc0 = (Crc32) expected;
					var crc1 = (Crc32) actual;

					Assert.That (crc1.Checksum, Is.EqualTo (crc0.Checksum), $"The cloned {decoder.GetType ().Name}.{field.Name} does not match.");
				} else {
					Assert.That (actual, Is.EqualTo (expected), $"The cloned {decoder.GetType ().Name}.{field.Name} does not match.");
				}
			}
		}

		protected static void CloneAndAssert (IMimeDecoder decoder)
		{
			// first, set some random state
			SetRandomState (decoder);

			var clone = decoder.Clone ();

			AssertState (decoder, clone);
		}

		protected static void ResetAndAssert (IMimeDecoder decoder)
		{
			// clone the decoder at it's initial state
			var clone = decoder.Clone ();

			// set some random state on the clone
			SetRandomState (clone);

			// reset the clone and make sure it matches
			clone.Reset ();

			AssertState (decoder, clone);
		}

		protected static void TestDecoder (IMimeDecoder decoder, byte[] rawData, string encodedFile, int bufferSize, bool unix = false)
		{
			int n;

			using (var decoded = new MemoryStream ()) {
				using (var filtered = new FilteredStream (decoded)) {
					filtered.Add (new DecoderFilter (decoder));

					if (unix)
						filtered.Add (new Dos2UnixFilter ());

					using (var file = File.OpenRead (Path.Combine (dataDir, encodedFile))) {
						var buffer = new byte[bufferSize];

						while ((n = file.Read (buffer, 0, bufferSize)) > 0)
							filtered.Write (buffer, 0, n);
					}

					filtered.Flush ();
				}

				var buf = decoded.GetBuffer ();
				n = rawData.Length;

				Assert.That (decoded.Length, Is.EqualTo (rawData.Length), "Decoded length is incorrect.");

				for (int i = 0; i < n; i++)
					Assert.That (buf[i], Is.EqualTo (rawData[i]), $"The byte at offset {i} does not match.");
			}
		}
	}
}

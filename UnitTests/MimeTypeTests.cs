//
// MimeTypeTests.cs
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

using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class MimeTypeTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentNullException> (() => MimeTypes.GetMimeType (null));
			Assert.Throws<ArgumentNullException> (() => MimeTypes.Register (null, ".ext"));
			Assert.Throws<ArgumentException> (() => MimeTypes.Register (string.Empty, ".ext"));
			Assert.Throws<ArgumentNullException> (() => MimeTypes.Register ("text/plain", null));
			Assert.Throws<ArgumentException> (() => MimeTypes.Register ("text/plain", string.Empty));
			Assert.Throws<ArgumentNullException> (() => MimeTypes.TryGetExtension (null, out _));
		}

		[Test]
		public void TestGetMimeTypeNullFileName ()
		{
			Assert.Throws<ArgumentNullException> (() => MimeTypes.GetMimeType (null));
		}

		[Test]
		public void TestGetMimeTypeNoFileExtension ()
		{
			Assert.That (MimeTypes.GetMimeType ("filename"), Is.EqualTo ("application/octet-stream"));
		}

		[Test]
		public void TestGetMimeTypeFileNameDot ()
		{
			Assert.That (MimeTypes.GetMimeType ("filename."), Is.EqualTo ("application/octet-stream"));
		}

		[Test]
		public void TestGetMimeTypeFileExtensionTxt ()
		{
			Assert.That (MimeTypes.GetMimeType ("filename.txt"), Is.EqualTo ("text/plain"));
		}

		[Test]
		public void TestGetMimeTypeFileExtensionCsv ()
		{
			Assert.That (MimeTypes.GetMimeType ("filename.csv"), Is.EqualTo ("text/csv"));
		}

		[TestCase (".3mf", "model/3mf")]
		[TestCase (".apng", "image/apng")]
		[TestCase (".avif", "image/avif")]
		[TestCase (".cbor", "application/cbor")]
		[TestCase (".flac", "audio/flac")]
		[TestCase (".geojson", "application/geo+json")]
		[TestCase (".glb", "model/gltf-binary")]
		[TestCase (".gltf", "model/gltf+json")]
		[TestCase (".gz", "application/gzip")]
		[TestCase (".heic", "image/heic")]
		[TestCase (".heif", "image/heif")]
		[TestCase (".jsonld", "application/ld+json")]
		[TestCase (".jxl", "image/jxl")]
		[TestCase (".mkv", "video/matroska")]
		[TestCase (".obj", "model/obj")]
		[TestCase (".parquet", "application/vnd.apache.parquet")]
		[TestCase (".stl", "model/stl")]
		[TestCase (".toml", "application/toml")]
		[TestCase (".vtt", "text/vtt")]
		[TestCase (".wasm", "application/wasm")]
		[TestCase (".webmanifest", "application/manifest+json")]
		[TestCase (".yaml", "application/yaml")]
		[TestCase (".zst", "application/zstd")]
		public void TestRegisteredMimeType (string extension, string mimeType)
		{
			Assert.That (MimeTypes.GetMimeType ("filename" + extension), Is.EqualTo (mimeType));
			Assert.That (MimeTypes.GetMimeType ("filename" + extension.ToUpperInvariant ()), Is.EqualTo (mimeType));
			Assert.That (MimeTypes.TryGetExtension (mimeType, out var actual), Is.True);
			Assert.That (actual, Is.EqualTo (extension));
			Assert.That (MimeTypes.TryGetExtension (mimeType.ToUpperInvariant (), out actual), Is.True);
			Assert.That (actual, Is.EqualTo (extension));
		}

		[TestCase (".yml", "application/yaml", ".yaml")]
		[TestCase (".gzip", "application/gzip", ".gz")]
		public void TestAlternateExtension (string extension, string mimeType, string preferredExtension)
		{
			Assert.That (MimeTypes.GetMimeType ("filename" + extension), Is.EqualTo (mimeType));
			Assert.That (MimeTypes.GetMimeType ("filename" + extension.ToUpperInvariant ()), Is.EqualTo (mimeType));
			Assert.That (MimeTypes.TryGetExtension (mimeType, out var actual), Is.True);
			Assert.That (actual, Is.EqualTo (preferredExtension));
		}

		[TestCase ("application/x-gzip", ".gz")]
		[TestCase ("audio/x-flac", ".flac")]
		[TestCase ("video/x-matroska", ".mkv")]
		[TestCase ("application/sla", ".stl")]
		public void TestLegacyMimeTypeAlias (string mimeType, string extension)
		{
			Assert.That (MimeTypes.TryGetExtension (mimeType, out var actual), Is.True);
			Assert.That (actual, Is.EqualTo (extension));
			Assert.That (MimeTypes.TryGetExtension (mimeType.ToUpperInvariant (), out actual), Is.True);
			Assert.That (actual, Is.EqualTo (extension));
		}

		[Test]
		public void TestTryGetExtensionTextPlain ()
		{
			Assert.That (MimeTypes.TryGetExtension ("text/plain", out var extension), Is.True);
			Assert.That (extension, Is.EqualTo (".txt"));
		}

		[Test]
		public void TestTryGetExtensionUnknownMimeType ()
		{
			Assert.That (MimeTypes.TryGetExtension ("application/x-vnd.fake-mime-type", out _), Is.False);
		}

		[Test]
		public void TestMimeTypeRegister ()
		{
			Assert.That (MimeTypes.GetMimeType ("filename.bogus"), Is.EqualTo ("application/octet-stream"));
			Assert.That (MimeTypes.TryGetExtension ("application/vnd.bogus", out _), Is.False);

			MimeTypes.Register ("application/vnd.bogus", ".bogus");

			Assert.That (MimeTypes.GetMimeType ("filename.bogus"), Is.EqualTo ("application/vnd.bogus"));
			Assert.That (MimeTypes.TryGetExtension ("application/vnd.bogus", out var extension), Is.True);
			Assert.That (extension, Is.EqualTo (".bogus"));
		}

		[Test]
		public void TestMimeTypeRegisterAddsLeadingDotToExtension ()
		{
			MimeTypes.Register ("application/vnd.no-dot", "nodot");

			Assert.That (MimeTypes.GetMimeType ("filename.nodot"), Is.EqualTo ("application/vnd.no-dot"));
			Assert.That (MimeTypes.TryGetExtension ("application/vnd.no-dot", out var extension), Is.True);
			Assert.That (extension, Is.EqualTo (".nodot"));
		}
	}
}

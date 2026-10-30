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
		[TestCase (".aiff", "audio/x-aiff")]
		[TestCase (".apng", "image/apng")]
		[TestCase (".arrow", "application/vnd.apache.arrow.file")]
		[TestCase (".avi", "video/x-msvideo")]
		[TestCase (".avif", "image/avif")]
		[TestCase (".cbor", "application/cbor")]
		[TestCase (".class", "application/java-vm")]
		[TestCase (".dart", "application/vnd.dart")]
		[TestCase (".deb", "application/vnd.debian.binary-package")]
		[TestCase (".dwg", "image/vnd.dwg")]
		[TestCase (".dxf", "image/vnd.dxf")]
		[TestCase (".eot", "application/vnd.ms-fontobject")]
		[TestCase (".flac", "audio/flac")]
		[TestCase (".geojson", "application/geo+json")]
		[TestCase (".glb", "model/gltf-binary")]
		[TestCase (".gltf", "model/gltf+json")]
		[TestCase (".gz", "application/gzip")]
		[TestCase (".heic", "image/heic")]
		[TestCase (".heics", "image/heic-sequence")]
		[TestCase (".heif", "image/heif")]
		[TestCase (".heifs", "image/heif-sequence")]
		[TestCase (".hqx", "application/mac-binhex40")]
		[TestCase (".jp2", "image/jp2")]
		[TestCase (".jpf", "image/jpx")]
		[TestCase (".jsonld", "application/ld+json")]
		[TestCase (".jxl", "image/jxl")]
		[TestCase (".m3u", "audio/x-mpegurl")]
		[TestCase (".m3u8", "application/vnd.apple.mpegurl")]
		[TestCase (".mif", "application/vnd.mif")]
		[TestCase (".mka", "audio/matroska")]
		[TestCase (".mkv", "video/matroska")]
		[TestCase (".obj", "model/obj")]
		[TestCase (".odf", "application/vnd.oasis.opendocument.formula")]
		[TestCase (".odg", "application/vnd.oasis.opendocument.graphics")]
		[TestCase (".otg", "application/vnd.oasis.opendocument.graphics-template")]
		[TestCase (".otp", "application/vnd.oasis.opendocument.presentation-template")]
		[TestCase (".ots", "application/vnd.oasis.opendocument.spreadsheet-template")]
		[TestCase (".ott", "application/vnd.oasis.opendocument.text-template")]
		[TestCase (".p12", "application/pkcs12")]
		[TestCase (".parquet", "application/vnd.apache.parquet")]
		[TestCase (".pcl", "application/vnd.hp-PCL")]
		[TestCase (".pnm", "image/x-portable-anymap")]
		[TestCase (".psd", "image/vnd.adobe.photoshop")]
		[TestCase (".py", "text/x-script.python")]
		[TestCase (".roff", "text/troff")]
		[TestCase (".smil", "application/smil+xml")]
		[TestCase (".sqlite3", "application/vnd.sqlite3")]
		[TestCase (".step", "model/step")]
		[TestCase (".stl", "model/stl")]
		[TestCase (".toml", "application/toml")]
		[TestCase (".vsd", "application/vnd.visio")]
		[TestCase (".vtt", "text/vtt")]
		[TestCase (".wasm", "application/wasm")]
		[TestCase (".webmanifest", "application/manifest+json")]
		[TestCase (".wpd", "application/vnd.wordperfect")]
		[TestCase (".wrl", "model/vrml")]
		[TestCase (".xls", "application/vnd.ms-excel")]
		[TestCase (".xpm", "image/x-xpixmap")]
		[TestCase (".yaml", "application/yaml")]
		[TestCase (".zst", "application/zstd")]
		public void TestMimeTypeMapping (string extension, string mimeType)
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
		[TestCase (".aif", "audio/x-aiff", ".aiff")]
		[TestCase (".aifc", "audio/x-aiff", ".aiff")]
		[TestCase (".asx", "video/x-ms-asf", ".asf")]
		[TestCase (".au", "audio/basic", ".snd")]
		[TestCase (".jpg2", "image/jp2", ".jp2")]
		[TestCase (".jpx", "image/jpx", ".jpf")]
		[TestCase (".man", "text/troff", ".roff")]
		[TestCase (".me", "text/troff", ".roff")]
		[TestCase (".ms", "text/troff", ".roff")]
		[TestCase (".opus", "audio/ogg", ".ogg")]
		[TestCase (".pfx", "application/pkcs12", ".p12")]
		[TestCase (".pm", "image/x-xpixmap", ".xpm")]
		[TestCase (".smi", "application/smil+xml", ".smil")]
		[TestCase (".sml", "application/smil+xml", ".smil")]
		[TestCase (".sqlite", "application/vnd.sqlite3", ".sqlite3")]
		[TestCase (".stp", "model/step", ".step")]
		[TestCase (".t", "text/troff", ".roff")]
		[TestCase (".tr", "text/troff", ".roff")]
		[TestCase (".udeb", "application/vnd.debian.binary-package", ".deb")]
		[TestCase (".vrml", "model/vrml", ".wrl")]
		[TestCase (".vss", "application/vnd.visio", ".vsd")]
		[TestCase (".vst", "application/vnd.visio", ".vsd")]
		[TestCase (".vsw", "application/vnd.visio", ".vsd")]
		[TestCase (".w60", "application/vnd.wordperfect", ".wpd")]
		[TestCase (".w61", "application/vnd.wordperfect", ".wpd")]
		[TestCase (".wp", "application/vnd.wordperfect", ".wpd")]
		[TestCase (".wp5", "application/vnd.wordperfect", ".wpd")]
		[TestCase (".wp6", "application/vnd.wordperfect", ".wpd")]
		[TestCase (".wrz", "model/vrml", ".wrl")]
		[TestCase (".wsc", "text/scriptlet", ".sct")]
		[TestCase (".xl", "application/vnd.ms-excel", ".xls")]
		[TestCase (".xlb", "application/vnd.ms-excel", ".xls")]
		[TestCase (".xlc", "application/vnd.ms-excel", ".xls")]
		[TestCase (".xld", "application/vnd.ms-excel", ".xls")]
		[TestCase (".xlk", "application/vnd.ms-excel", ".xls")]
		[TestCase (".xll", "application/vnd.ms-excel", ".xls")]
		[TestCase (".xlm", "application/vnd.ms-excel", ".xls")]
		[TestCase (".xlv", "application/vnd.ms-excel", ".xls")]
		[TestCase (".xlw", "application/vnd.ms-excel", ".xls")]
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
		[TestCase ("application/acad", ".dwg")]
		[TestCase ("application/binhex", ".hqx")]
		[TestCase ("application/dart", ".dart")]
		[TestCase ("application/dxf", ".dxf")]
		[TestCase ("application/excel", ".xls")]
		[TestCase ("application/pkcs-12", ".p12")]
		[TestCase ("application/smil", ".smi")]
		[TestCase ("application/step", ".step")]
		[TestCase ("application/vnd.bw-fontobject", ".eot")]
		[TestCase ("application/wordperfect", ".wp5")]
		[TestCase ("application/wordperfect6.0", ".w60")]
		[TestCase ("application/wordperfect6.1", ".w61")]
		[TestCase ("application/x-deb", ".deb")]
		[TestCase ("application/x-debian-package", ".deb")]
		[TestCase ("application/x-java-applet", ".class")]
		[TestCase ("application/x-mif", ".mif")]
		[TestCase ("application/x-mplayer2", ".asx")]
		[TestCase ("application/x-pcl", ".pcl")]
		[TestCase ("application/x-pkcs12", ".pfx")]
		[TestCase ("application/x-portable-anymap", ".pnm")]
		[TestCase ("application/x-sqlite3", ".sqlite3")]
		[TestCase ("application/x-troff", ".roff")]
		[TestCase ("application/x-troff-man", ".man")]
		[TestCase ("application/x-troff-me", ".me")]
		[TestCase ("application/x-troff-ms", ".ms")]
		[TestCase ("application/x-visio", ".vsd")]
		[TestCase ("application/x-vrml", ".vrml")]
		[TestCase ("application/x-world", ".svr")]
		[TestCase ("audio/aiff", ".aiff")]
		[TestCase ("audio/opus", ".opus")]
		[TestCase ("audio/x-au", ".au")]
		[TestCase ("audio/x-matroska", ".mka")]
		[TestCase ("audio/x-mpequrl", ".m3u")]
		[TestCase ("image/xpm", ".xpm")]
		[TestCase ("text/scriplet", ".wsc")]
		[TestCase ("text/x-script.phyton", ".py")]
		[TestCase ("video/avi", ".avi")]
		public void TestLegacyMimeTypeAlias (string mimeType, string extension)
		{
			Assert.That (MimeTypes.TryGetExtension (mimeType, out var actual), Is.True);
			Assert.That (actual, Is.EqualTo (extension));
			Assert.That (MimeTypes.TryGetExtension (mimeType.ToUpperInvariant (), out actual), Is.True);
			Assert.That (actual, Is.EqualTo (extension));
		}

		[TestCase (".ts", "application/typescript")]
		[TestCase (".rpm", "audio/x-pn-realaudio-plugin")]
		[TestCase (".xml", "text/xml")]
		[TestCase (".rtf", "text/rtf")]
		[TestCase (".db", "application/octet-stream")]
		public void TestPreservedMimeTypeMapping (string extension, string mimeType)
		{
			Assert.That (MimeTypes.GetMimeType ("filename" + extension), Is.EqualTo (mimeType));
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

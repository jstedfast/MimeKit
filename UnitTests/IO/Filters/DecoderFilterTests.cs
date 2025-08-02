//
// DecoderFilterTests.cs
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

using MimeKit;
using MimeKit.IO.Filters;

namespace UnitTests.IO.Filters {
	[TestFixture]
	public class DecoderFilterTests
	{
		static void AssertIsDecoderFilter (ContentEncoding encoding, ContentEncoding expected)
		{
			var filter = DecoderFilter.Create (encoding);

			Assert.That (filter, Is.InstanceOf<DecoderFilter> (), $"Expected DecoderFilter for ContentEncoding.{encoding}");

			var decoder = (DecoderFilter) filter;

			Assert.That (decoder.Encoding, Is.EqualTo (expected), $"Expected decoder's Encoding to be ContentEncoding.{expected}");
		}

		static void AssertIsDecoderFilter (string encoding, ContentEncoding expected)
		{
			var filter = DecoderFilter.Create (encoding);

			Assert.That (filter, Is.InstanceOf<DecoderFilter> (), $"Expected DecoderFilter for \"{encoding}\"");

			var decoder = (DecoderFilter) filter;

			Assert.That (decoder.Encoding, Is.EqualTo (expected), $"Expected decoder's Encoding to be ContentEncoding.{expected}");
		}

		[Test]
		public void TestCreate ()
		{
			Assert.Throws<ArgumentNullException> (() => DecoderFilter.Create (null));

			AssertIsDecoderFilter (ContentEncoding.Base64, ContentEncoding.Base64);
			AssertIsDecoderFilter ("base64", ContentEncoding.Base64);

			Assert.That (DecoderFilter.Create (ContentEncoding.Binary), Is.InstanceOf<PassThroughFilter> ());
			Assert.That (DecoderFilter.Create ("binary"), Is.InstanceOf<PassThroughFilter> ());

			Assert.That (DecoderFilter.Create ((ContentEncoding) (-1)), Is.InstanceOf<PassThroughFilter> ());
			Assert.That (DecoderFilter.Create (ContentEncoding.Default), Is.InstanceOf<PassThroughFilter> ());
			Assert.That (DecoderFilter.Create ("x-invalid"), Is.InstanceOf<PassThroughFilter> ());

			Assert.That (DecoderFilter.Create (ContentEncoding.EightBit), Is.InstanceOf<PassThroughFilter> ());
			Assert.That (DecoderFilter.Create ("8bit"), Is.InstanceOf<PassThroughFilter> ());

			AssertIsDecoderFilter (ContentEncoding.QuotedPrintable, ContentEncoding.QuotedPrintable);
			AssertIsDecoderFilter ("quoted-printable", ContentEncoding.QuotedPrintable);

			Assert.That (DecoderFilter.Create (ContentEncoding.SevenBit), Is.InstanceOf<PassThroughFilter> ());
			Assert.That (DecoderFilter.Create ("7bit"), Is.InstanceOf<PassThroughFilter> ());

			AssertIsDecoderFilter (ContentEncoding.UUEncode, ContentEncoding.UUEncode);
			AssertIsDecoderFilter ("x-uuencode", ContentEncoding.UUEncode);
			AssertIsDecoderFilter ("uuencode", ContentEncoding.UUEncode);
		}
	}
}

//
// EncoderFilterTests.cs
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
	public class EncoderFilterTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentNullException> (() => EncoderFilter.Create (null));
		}

		static void AssertIsEncoderFilter (ContentEncoding encoding, ContentEncoding expected)
		{
			var filter = EncoderFilter.Create (encoding);

			Assert.That (filter, Is.InstanceOf<EncoderFilter> (), $"Expected EncoderFilter for ContentEncoding.{encoding}");

			var encoder = (EncoderFilter) filter;

			Assert.That (encoder.Encoding, Is.EqualTo (expected), $"Expected encoder's Encoding to be ContentEncoding.{expected}");
		}

		static void AssertIsEncoderFilter (string encoding, ContentEncoding expected)
		{
			var filter = EncoderFilter.Create (encoding);

			Assert.That (filter, Is.InstanceOf<EncoderFilter> (), $"Expected EncoderFilter for \"{encoding}\"");

			var encoder = (EncoderFilter) filter;

			Assert.That (encoder.Encoding, Is.EqualTo (expected), $"Expected encoder's Encoding to be ContentEncoding.{expected}");
		}

		[Test]
		public void TestCreate ()
		{
			Assert.Throws<ArgumentNullException> (() => EncoderFilter.Create (null));

			AssertIsEncoderFilter (ContentEncoding.Base64, ContentEncoding.Base64);
			AssertIsEncoderFilter ("base64", ContentEncoding.Base64);

			Assert.That (EncoderFilter.Create (ContentEncoding.Binary), Is.InstanceOf<PassThroughFilter> ());
			Assert.That (EncoderFilter.Create ("binary"), Is.InstanceOf<PassThroughFilter> ());

			Assert.That (EncoderFilter.Create ((ContentEncoding) (-1)), Is.InstanceOf<PassThroughFilter> ());
			Assert.That (EncoderFilter.Create (ContentEncoding.Default), Is.InstanceOf<PassThroughFilter> ());
			Assert.That (EncoderFilter.Create ("x-invalid"), Is.InstanceOf<PassThroughFilter> ());

			Assert.That (EncoderFilter.Create (ContentEncoding.EightBit), Is.InstanceOf<PassThroughFilter> ());
			Assert.That (EncoderFilter.Create ("8bit"), Is.InstanceOf<PassThroughFilter> ());

			AssertIsEncoderFilter (ContentEncoding.QuotedPrintable, ContentEncoding.QuotedPrintable);
			AssertIsEncoderFilter ("quoted-printable", ContentEncoding.QuotedPrintable);

			Assert.That (EncoderFilter.Create (ContentEncoding.SevenBit), Is.InstanceOf<PassThroughFilter> ());
			Assert.That (EncoderFilter.Create ("7bit"), Is.InstanceOf<PassThroughFilter> ());

			AssertIsEncoderFilter (ContentEncoding.UUEncode, ContentEncoding.UUEncode);
			AssertIsEncoderFilter ("x-uuencode", ContentEncoding.UUEncode);
			AssertIsEncoderFilter ("uuencode", ContentEncoding.UUEncode);
		}
	}
}

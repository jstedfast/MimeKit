//
// FilterTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2016 Xamarin Inc. (www.xamarin.com)
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

using System;

using NUnit.Framework;

using MimeKit;
using MimeKit.IO.Filters;
using MimeKit.Cryptography;

namespace UnitTests {
	[TestFixture]
	public class FilterTests
	{
		static void AssertInvalidArguments (IMimeFilter filter)
		{
			int outputIndex, outputLength;
			var input = new byte[1024];
			ArgumentException ex;

			// Filter
			Assert.Throws<ArgumentNullException> (() => filter.Filter (null, 0, 0, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentNullException when input was null.", filter.GetType ().Name);

			ex = Assert.Throws<ArgumentOutOfRangeException> (() => filter.Filter (input, -1, 0, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentOutOfRangeException when startIndex was -1.", filter.GetType ().Name);
			Assert.AreEqual ("startIndex", ex.ParamName);

			ex = Assert.Throws<ArgumentOutOfRangeException> (() => filter.Filter (input, 0, -1, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentOutOfRangeException when length was -1.", filter.GetType ().Name);
			Assert.AreEqual ("length", ex.ParamName);

			ex = Assert.Throws<ArgumentOutOfRangeException> (() => filter.Filter (input, 1025, 0, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentOutOfRangeException when startIndex was > 1024.", filter.GetType ().Name);
			Assert.AreEqual ("startIndex", ex.ParamName);

			ex = Assert.Throws<ArgumentOutOfRangeException> (() => filter.Filter (input, 0, 1025, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentOutOfRangeException when length was > 1024.", filter.GetType ().Name);
			Assert.AreEqual ("length", ex.ParamName);

			// Flush
			Assert.Throws<ArgumentNullException> (() => filter.Flush (null, 0, 0, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentNullException when input was null.", filter.GetType ().Name);

			ex = Assert.Throws<ArgumentOutOfRangeException> (() => filter.Flush (input, -1, 0, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentOutOfRangeException when startIndex was -1.", filter.GetType ().Name);
			Assert.AreEqual ("startIndex", ex.ParamName);

			ex = Assert.Throws<ArgumentOutOfRangeException> (() => filter.Flush (input, 0, -1, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentOutOfRangeException when length was -1.", filter.GetType ().Name);
			Assert.AreEqual ("length", ex.ParamName);

			ex = Assert.Throws<ArgumentOutOfRangeException> (() => filter.Flush (input, 1025, 0, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentOutOfRangeException when startIndex was > 1024.", filter.GetType ().Name);
			Assert.AreEqual ("startIndex", ex.ParamName);

			ex = Assert.Throws<ArgumentOutOfRangeException> (() => filter.Flush (input, 0, 1025, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentOutOfRangeException when length was > 1024.", filter.GetType ().Name);
			Assert.AreEqual ("length", ex.ParamName);
		}

		[Test]
		public void TestInvalidArguments ()
		{
			AssertInvalidArguments (new Dos2UnixFilter ());
			AssertInvalidArguments (new Unix2DosFilter ());
			AssertInvalidArguments (new ArmoredFromFilter ());
			AssertInvalidArguments (new BestEncodingFilter ());
			AssertInvalidArguments (new CharsetFilter ("iso-8859-1", "utf-8"));
			AssertInvalidArguments (DecoderFilter.Create (ContentEncoding.Base64));
			AssertInvalidArguments (EncoderFilter.Create (ContentEncoding.Base64));
			AssertInvalidArguments (DecoderFilter.Create (ContentEncoding.QuotedPrintable));
			AssertInvalidArguments (EncoderFilter.Create (ContentEncoding.QuotedPrintable));
			AssertInvalidArguments (DecoderFilter.Create (ContentEncoding.UUEncode));
			AssertInvalidArguments (EncoderFilter.Create (ContentEncoding.UUEncode));
			AssertInvalidArguments (new TrailingWhitespaceFilter ());
			AssertInvalidArguments (new DkimRelaxedBodyFilter ());
			AssertInvalidArguments (new DkimSimpleBodyFilter ());
		}
	}
}

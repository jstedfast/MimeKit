//
// PunycodeTests.cs
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

using System.Globalization;

using MimeKit.Encodings;

namespace UnitTests.Encodings {
	[TestFixture]
	public class PunycodeTests
	{
		readonly Punycode punycode = new Punycode (new IdnMapping ());

		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentNullException> (() => new Punycode (null));
		}

		[TestCase ("abc.org", ExpectedResult = "abc.org")]
		[TestCase ("my_company.com", ExpectedResult = "my_company.com")]
		[TestCase ("bücher.com", ExpectedResult = "xn--bcher-kva.com")]
		[TestCase ("мойдомен.рф", ExpectedResult = "xn--d1acklchcc.xn--p1ai")]
		[TestCase ("παράδειγμα.δοκιμή", ExpectedResult = "xn--hxajbheg2az3al.xn--jxalpdlp")]
		[TestCase ("mycharity。org", ExpectedResult = "mycharity.org")]
		public object TestEncode (string value)
		{
			return punycode.Encode (value);
		}

		[TestCase ("user@abc.org", 5, ExpectedResult = "abc.org")]
		[TestCase ("user@my_company.com", 5, ExpectedResult = "my_company.com")]
		[TestCase ("user@bücher.com", 5, ExpectedResult = "xn--bcher-kva.com")]
		[TestCase ("user@мойдомен.рф", 5, ExpectedResult = "xn--d1acklchcc.xn--p1ai")]
		[TestCase ("user@παράδειγμα.δοκιμή", 5, ExpectedResult = "xn--hxajbheg2az3al.xn--jxalpdlp")]
		[TestCase ("user@mycharity。org", 5, ExpectedResult = "mycharity.org")]
		public object TestEncodeIndex (string value, int index)
		{
			return punycode.Encode (value, index);
		}

		[TestCase ("(user@abc.org)", 6, 7, ExpectedResult = "abc.org")]
		[TestCase ("(user@my_company.com)", 6, 14, ExpectedResult = "my_company.com")]
		[TestCase ("(user@bücher.com)", 6, 10, ExpectedResult = "xn--bcher-kva.com")]
		[TestCase ("(user@мойдомен.рф)", 6, 11, ExpectedResult = "xn--d1acklchcc.xn--p1ai")]
		[TestCase ("(user@παράδειγμα.δοκιμή)", 6, 17, ExpectedResult = "xn--hxajbheg2az3al.xn--jxalpdlp")]
		[TestCase ("(user@mycharity。org)", 6, 13, ExpectedResult = "mycharity.org")]
		public object TestEncodeIndexCount (string value, int index, int count)
		{
			return punycode.Encode (value, index, count);
		}

		[TestCase ("abc.org", ExpectedResult = "abc.org")]
		[TestCase ("my_company.com", ExpectedResult = "my_company.com")]
		[TestCase ("xn--bcher-kva.com", ExpectedResult = "bücher.com")]
		[TestCase ("xn--d1acklchcc.xn--p1ai", ExpectedResult = "мойдомен.рф")]
		[TestCase ("xn--hxajbheg2az3al.xn--jxalpdlp", ExpectedResult = "παράδειγμα.δοκιμή")]
		public object TestDecode (string value)
		{
			return punycode.Decode (value);
		}

		[TestCase ("user@abc.org", 5, ExpectedResult = "abc.org")]
		[TestCase ("user@my_company.com", 5, ExpectedResult = "my_company.com")]
		[TestCase ("user@xn--bcher-kva.com", 5, ExpectedResult = "bücher.com")]
		[TestCase ("user@xn--d1acklchcc.xn--p1ai", 5, ExpectedResult = "мойдомен.рф")]
		[TestCase ("user@xn--hxajbheg2az3al.xn--jxalpdlp", 5, ExpectedResult = "παράδειγμα.δοκιμή")]
		public object TestDecodeIndex (string value, int index)
		{
			return punycode.Decode (value, index);
		}

		[TestCase ("(user@abc.org)", 6, 7, ExpectedResult = "abc.org")]
		[TestCase ("(user@my_company.com)", 6, 14, ExpectedResult = "my_company.com")]
		[TestCase ("(user@xn--bcher-kva.com)", 6, 17, ExpectedResult = "bücher.com")]
		[TestCase ("(user@xn--d1acklchcc.xn--p1ai)", 6, 23, ExpectedResult = "мойдомен.рф")]
		[TestCase ("(user@xn--hxajbheg2az3al.xn--jxalpdlp)", 6, 31, ExpectedResult = "παράδειγμα.δοκιμή")]
		public object TestDecodeIndexCount (string value, int index, int count)
		{
			return punycode.Decode (value, index, count);
		}
	}
}

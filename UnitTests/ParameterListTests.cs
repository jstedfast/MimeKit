//
// ParameterListTests.cs
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
using System.Text;

using NUnit.Framework;

using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class ParameterListTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			const string invalid = "X-测试文本";
			var list = new ParameterList ();
			Parameter param;
			string value;

			// Add
			Assert.Throws<ArgumentNullException> (() => list.Add ((Encoding) null, "name", "value"));
			Assert.Throws<ArgumentNullException> (() => list.Add (Encoding.UTF8, null, "value"));
			Assert.Throws<ArgumentException> (() => list.Add (Encoding.UTF8, string.Empty, "value"));
			Assert.Throws<ArgumentException> (() => list.Add (Encoding.UTF8, invalid, "value"));
			Assert.Throws<ArgumentNullException> (() => list.Add (Encoding.UTF8, "name", null));
			Assert.Throws<ArgumentNullException> (() => list.Add ((string) null, "name", "value"));
			Assert.Throws<ArgumentNullException> (() => list.Add ("utf-8", null, "value"));
			Assert.Throws<ArgumentException> (() => list.Add ("utf-8", string.Empty, "value"));
			Assert.Throws<ArgumentException> (() => list.Add ("utf-8", invalid, "value"));
			Assert.Throws<ArgumentNullException> (() => list.Add ("utf-8", "name", null));
			Assert.Throws<ArgumentNullException> (() => list.Add (null, "value"));
			Assert.Throws<ArgumentException> (() => list.Add (string.Empty, "value"));
			Assert.Throws<ArgumentException> (() => list.Add (invalid, "value"));
			Assert.Throws<ArgumentNullException> (() => list.Add ("name", null));
			Assert.Throws<ArgumentNullException> (() => list.Add (null));

			// Contains
			Assert.Throws<ArgumentNullException> (() => list.Contains ((Parameter) null));
			Assert.Throws<ArgumentNullException> (() => list.Contains ((string) null));

			// CopyTo
			Assert.Throws<ArgumentOutOfRangeException> (() => list.CopyTo (new Parameter[0], -1));
			Assert.Throws<ArgumentNullException> (() => list.CopyTo (null, 0));

			// IndexOf
			Assert.Throws<ArgumentNullException> (() => list.IndexOf ((Parameter) null));
			Assert.Throws<ArgumentNullException> (() => list.IndexOf ((string) null));

			// Insert
			list.Add ("name", "value");
			Assert.Throws<ArgumentOutOfRangeException> (() => list.Insert (-1, new Parameter ("name", "value")));
			Assert.Throws<ArgumentOutOfRangeException> (() => list.Insert (-1, "field", "value"));
			Assert.Throws<ArgumentNullException> (() => list.Insert (0, null, "value"));
			Assert.Throws<ArgumentException> (() => list.Insert (0, string.Empty, "value"));
			Assert.Throws<ArgumentException> (() => list.Insert (0, invalid, "value"));
			Assert.Throws<ArgumentNullException> (() => list.Insert (0, "name", null));
			Assert.Throws<ArgumentNullException> (() => list.Insert (0, null));

			// Remove
			Assert.Throws<ArgumentNullException> (() => list.Remove ((Parameter) null));
			Assert.Throws<ArgumentNullException> (() => list.Remove ((string) null));

			// RemoveAt
			Assert.Throws<ArgumentOutOfRangeException> (() => list.RemoveAt (-1));

			// TryGetValue
			Assert.Throws<ArgumentNullException> (() => list.TryGetValue (null, out param));
			Assert.Throws<ArgumentNullException> (() => list.TryGetValue (null, out value));
		}
	}
}

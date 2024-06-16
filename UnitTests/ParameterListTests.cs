//
// ParameterListTests.cs
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

using System.Text;
using System.Collections;

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

			list.Add ("name", "x-value");
			Assert.Throws<ArgumentException> (() => list.Add ("name", "value"));
			Assert.Throws<ArgumentException> (() => list.Add (new Parameter ("name", "value")));
			list.Clear ();

			// Contains
			Assert.Throws<ArgumentNullException> (() => list.Contains ((Parameter) null));
			Assert.Throws<ArgumentNullException> (() => list.Contains ((string) null));

			// CopyTo
			Assert.Throws<ArgumentOutOfRangeException> (() => list.CopyTo (Array.Empty<Parameter> (), -1));
			Assert.Throws<ArgumentNullException> (() => list.CopyTo (null, 0));

			// IndexOf
			Assert.Throws<ArgumentNullException> (() => list.IndexOf ((Parameter) null));
			Assert.Throws<ArgumentNullException> (() => list.IndexOf ((string) null));

			// Insert
			list.Add ("x-name", "value");
			Assert.Throws<ArgumentOutOfRangeException> (() => list.Insert (-1, new Parameter ("name", "value")));
			Assert.Throws<ArgumentOutOfRangeException> (() => list.Insert (-1, "field", "value"));
			Assert.Throws<ArgumentNullException> (() => list.Insert (0, null, "value"));
			Assert.Throws<ArgumentException> (() => list.Insert (0, string.Empty, "value"));
			Assert.Throws<ArgumentException> (() => list.Insert (0, invalid, "value"));
			Assert.Throws<ArgumentNullException> (() => list.Insert (0, "name", null));
			Assert.Throws<ArgumentNullException> (() => list.Insert (0, null));
			Assert.Throws<ArgumentException> (() => list.Insert (0, "x-name", "x-value"));
			Assert.Throws<ArgumentException> (() => list.Insert (0, new Parameter ("x-name", "x-value")));
			list.Clear ();

			// Remove
			Assert.Throws<ArgumentNullException> (() => list.Remove ((Parameter) null));
			Assert.Throws<ArgumentNullException> (() => list.Remove ((string) null));

			// RemoveAt
			Assert.Throws<ArgumentOutOfRangeException> (() => list.RemoveAt (-1));

			// TryGetValue
			Assert.Throws<ArgumentNullException> (() => list.TryGetValue (null, out param));
			Assert.Throws<ArgumentNullException> (() => list.TryGetValue (null, out value));

			// Indexers
			list.Add ("name", "value");
			list.Add ("x-name", "x-value");
			Assert.Throws<ArgumentOutOfRangeException> (() => list[-1] = new Parameter ("name", "value"));
			Assert.Throws<ArgumentOutOfRangeException> (() => param = list[-1]);
			Assert.Throws<ArgumentNullException> (() => list[0] = null);
			Assert.Throws<ArgumentNullException> (() => list[null] = "value");
			Assert.Throws<ArgumentNullException> (() => value = list[null]);
			Assert.Throws<ArgumentNullException> (() => list["name"] = null);
			Assert.Throws<ArgumentException> (() => list[1] = new Parameter ("name", "value"));
			list.Clear ();
		}

		[Test]
		public void TestBasicFunctionality ()
		{
			var xyz = new Parameter ("xyz", "3");
			var list = new ParameterList ();
			Parameter parameter;
			string value;
			int index;

			Assert.That (list.IsReadOnly, Is.False, "IsReadOnly");
			Assert.That (list.Count, Is.EqualTo (0));

			list.Add (new Parameter ("abc", "0"));
			list.Add (Encoding.UTF8, "def", "1");
			list.Add ("ghi", "2");

			Assert.That (list.Count, Is.EqualTo (3), "Count");
			Assert.That (list.Contains ("xyz"), Is.False);
			Assert.That (list.Contains (list[0]), Is.True);
			Assert.That (list.Contains ("aBc"), Is.True);
			Assert.That (list.Contains ("DEf"), Is.True);
			Assert.That (list.Contains ("gHI"), Is.True);
			Assert.That (list.IndexOf ("xyz"), Is.EqualTo (-1));
			Assert.That (list.IndexOf ("aBc"), Is.EqualTo (0));
			Assert.That (list.IndexOf ("dEF"), Is.EqualTo (1));
			Assert.That (list.IndexOf ("Ghi"), Is.EqualTo (2));
			Assert.That (list.IndexOf (xyz), Is.EqualTo (-1));
			Assert.That (list.IndexOf (list[0]), Is.EqualTo (0));
			Assert.That (list.IndexOf (list[1]), Is.EqualTo (1));
			Assert.That (list.IndexOf (list[2]), Is.EqualTo (2));
			Assert.That (list[0].Name, Is.EqualTo ("abc"));
			Assert.That (list[1].Name, Is.EqualTo ("def"));
			Assert.That (list[2].Name, Is.EqualTo ("ghi"));
			Assert.That (list["AbC"], Is.EqualTo ("0"));
			Assert.That (list["dEf"], Is.EqualTo ("1"));
			Assert.That (list["GHi"], Is.EqualTo ("2"));

			Assert.That (list.TryGetValue ("Abc", out parameter), Is.True);
			Assert.That (parameter.Name, Is.EqualTo ("abc"));
			Assert.That (list.TryGetValue ("Abc", out value), Is.True);
			Assert.That (value, Is.EqualTo ("0"));

			Assert.That (list.Remove (xyz), Is.False, "Remove");
			list.Insert (0, xyz);
			Assert.That (list.Remove (xyz), Is.True, "Remove");

			Assert.That (list.Remove ("xyz"), Is.False, "Remove");
			list.Insert (0, xyz);
			Assert.That (list.Remove ("xyz"), Is.True, "Remove");

			var array = new Parameter[list.Count];
			list.CopyTo (array, 0);
			Assert.That (array[0].Name, Is.EqualTo ("abc"));
			Assert.That (array[1].Name, Is.EqualTo ("def"));
			Assert.That (array[2].Name, Is.EqualTo ("ghi"));

			index = 0;
			foreach (var param in list) {
				Assert.That (param, Is.EqualTo (array[index]));
				index++;
			}

			index = 0;
			foreach (Parameter param in (IEnumerable) list) {
				Assert.That (param, Is.EqualTo (array[index]));
				index++;
			}

			list.Clear ();
			Assert.That (list.Count, Is.EqualTo (0), "Clear");

			list.Add ("xyz", "3");
			list.Insert (0, array[2]);
			list.Insert (0, array[1].Name, array[1].Value);
			list.Insert (0, array[0]);

			Assert.That (list.Count, Is.EqualTo (4));
			Assert.That (list[0].Name, Is.EqualTo ("abc"));
			Assert.That (list[1].Name, Is.EqualTo ("def"));
			Assert.That (list[2].Name, Is.EqualTo ("ghi"));
			Assert.That (list[3].Name, Is.EqualTo ("xyz"));
			Assert.That (list["AbC"], Is.EqualTo ("0"));
			Assert.That (list["dEf"], Is.EqualTo ("1"));
			Assert.That (list["GHi"], Is.EqualTo ("2"));
			Assert.That (list["XYZ"], Is.EqualTo ("3"));

			list.RemoveAt (3);
			Assert.That (list.Count, Is.EqualTo (3));

			Assert.That (list.ToString (), Is.EqualTo ("; abc=\"0\"; def=\"1\"; ghi=\"2\""));

			list[0] = new Parameter ("abc", "replaced");

			Assert.That (list.ToString (), Is.EqualTo ("; abc=\"replaced\"; def=\"1\"; ghi=\"2\""));

			list[0] = new Parameter ("xxx", "0");

			Assert.That (list.ToString (), Is.EqualTo ("; xxx=\"0\"; def=\"1\"; ghi=\"2\""));
		}

		[Test]
		public void TestParseRfc2231ParemeterValueWithoutCharsetDeclaration ()
		{
			const string text = "name*0*=This%20is%20some%20encoded%20ascii%20text";
			const string expected = "This is some encoded ascii text";
			var options = ParserOptions.Default.Clone ();
			var input = Encoding.ASCII.GetBytes (text);
			var index = 0;

			Assert.That (ParameterList.TryParse (options, input, ref index, input.Length, false, out var paramList), Is.True);
			Assert.That (paramList["name"], Is.EqualTo (expected));
		}

		[Test]
		public void TestParseRfc2231ParemeterValueWithIncompleteCharsetDeclaration ()
		{
			const string text = "name*0*=us-ascii'This%20is%20some%20encoded%20ascii%20text";
			const string expected = "us-ascii'This is some encoded ascii text";
			var options = ParserOptions.Default.Clone ();
			var input = Encoding.ASCII.GetBytes (text);
			var index = 0;

			Assert.That (ParameterList.TryParse (options, input, ref index, input.Length, false, out var paramList), Is.True);
			Assert.That (paramList["name"], Is.EqualTo (expected));
		}

		[Test]
		public void TestParseRfc2231ParemeterValueWithUnsupportedCharset ()
		{
			const string text = "name*0*=x-unsupported-charset''This%20is%20some%20encoded%20ascii%20text";
			const string expected = "This is some encoded ascii text";
			var options = ParserOptions.Default.Clone ();
			var input = Encoding.ASCII.GetBytes (text);
			var index = 0;

			Assert.That (ParameterList.TryParse (options, input, ref index, input.Length, false, out var paramList), Is.True);
			Assert.That (paramList["name"], Is.EqualTo (expected));
		}
	}
}

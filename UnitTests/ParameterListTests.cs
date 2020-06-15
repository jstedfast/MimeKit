//
// ParameterListTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 .NET Foundation and Contributors
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

			list.Add ("name", "x-value");
			Assert.Throws<ArgumentException> (() => list.Add ("name", "value"));
			Assert.Throws<ArgumentException> (() => list.Add (new Parameter ("name", "value")));
			list.Clear ();

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
			var list = new ParameterList ();
			Parameter parameter;
			string value;
			int index;

			Assert.IsFalse (list.IsReadOnly, "IsReadOnly");
			Assert.AreEqual (0, list.Count);

			list.Add (new Parameter ("abc", "0"));
			list.Add (Encoding.UTF8, "def", "1");
			list.Add ("ghi", "2");

			Assert.AreEqual (3, list.Count, "Count");
			Assert.IsTrue (list.Contains (list[0]));
			Assert.IsTrue (list.Contains ("aBc"));
			Assert.IsTrue (list.Contains ("DEf"));
			Assert.IsTrue (list.Contains ("gHI"));
			Assert.AreEqual (0, list.IndexOf ("aBc"));
			Assert.AreEqual (1, list.IndexOf ("dEF"));
			Assert.AreEqual (2, list.IndexOf ("Ghi"));
			Assert.AreEqual ("abc", list[0].Name);
			Assert.AreEqual ("def", list[1].Name);
			Assert.AreEqual ("ghi", list[2].Name);
			Assert.AreEqual ("0", list["AbC"]);
			Assert.AreEqual ("1", list["dEf"]);
			Assert.AreEqual ("2", list["GHi"]);

			Assert.IsTrue (list.TryGetValue ("Abc", out parameter));
			Assert.AreEqual ("abc", parameter.Name);
			Assert.IsTrue (list.TryGetValue ("Abc", out value));
			Assert.AreEqual ("0", value);

			Assert.IsFalse (list.Remove ("xyz"), "Remove");
			list.Insert (0, new Parameter ("xyz", "3"));
			Assert.IsTrue (list.Remove ("xyz"), "Remove");

			var array = new Parameter[list.Count];
			list.CopyTo (array, 0);
			Assert.AreEqual ("abc", array[0].Name);
			Assert.AreEqual ("def", array[1].Name);
			Assert.AreEqual ("ghi", array[2].Name);

			index = 0;
			foreach (var param in list) {
				Assert.AreEqual (array[index], param);
				index++;
			}

			list.Clear ();
			Assert.AreEqual (0, list.Count, "Clear");

			list.Add ("xyz", "3");
			list.Insert (0, array[2]);
			list.Insert (0, array[1].Name, array[1].Value);
			list.Insert (0, array[0]);

			Assert.AreEqual (4, list.Count);
			Assert.AreEqual ("abc", list[0].Name);
			Assert.AreEqual ("def", list[1].Name);
			Assert.AreEqual ("ghi", list[2].Name);
			Assert.AreEqual ("xyz", list[3].Name);
			Assert.AreEqual ("0", list["AbC"]);
			Assert.AreEqual ("1", list["dEf"]);
			Assert.AreEqual ("2", list["GHi"]);
			Assert.AreEqual ("3", list["XYZ"]);

			list.RemoveAt (3);
			Assert.AreEqual (3, list.Count);

			Assert.AreEqual ("; abc=\"0\"; def=\"1\"; ghi=\"2\"", list.ToString ());

			list[0] = new Parameter ("abc", "replaced");

			Assert.AreEqual ("; abc=\"replaced\"; def=\"1\"; ghi=\"2\"", list.ToString ());

			list[0] = new Parameter ("xxx", "0");

			Assert.AreEqual ("; xxx=\"0\"; def=\"1\"; ghi=\"2\"", list.ToString ());
		}
	}
}

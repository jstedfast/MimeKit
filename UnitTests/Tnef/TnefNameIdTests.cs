//
// TnefNameIdTests.cs
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

using MimeKit.Tnef;

namespace UnitTests.Tnef {
	[TestFixture]
	public class TnefNameIdTests
	{
		[Test]
		public void TestConstructors ()
		{
			var guid = Guid.NewGuid ();
			var tnef1 = new TnefNameId (guid, 17);

			Assert.That (tnef1.Kind, Is.EqualTo (TnefNameIdKind.Id), "Kind Id");
			Assert.That (tnef1.PropertySetGuid, Is.EqualTo (guid), "PropertySetGuid Id");
			Assert.That (tnef1.Id, Is.EqualTo (17), "Id");

			tnef1 = new TnefNameId (guid, "name");
			Assert.That (tnef1.Kind, Is.EqualTo (TnefNameIdKind.Name), "Kind Name");
			Assert.That (tnef1.PropertySetGuid, Is.EqualTo (guid), "PropertySetGuid Name");
			Assert.That (tnef1.Name, Is.EqualTo ("name"), "Name");
		}

		[Test]
		public void TestEqualityOfIdAndName ()
		{
			var guid = Guid.NewGuid ();
			var tnef1 = new TnefNameId (guid, 17);
			var tnef2 = new TnefNameId (guid, "name");

			Assert.That (tnef2.GetHashCode (), Is.Not.EqualTo (tnef1.GetHashCode ()), "GetHashCode Name vs Id");
			Assert.That (tnef2, Is.Not.EqualTo (tnef1), "Equal Name vs Id");

			Assert.That (tnef1 == tnef2, Is.False, "==");
			Assert.That (tnef1 != tnef2, Is.True, "!=");
		}

		[Test]
		public void TestEqualityById ()
		{
			var guid = Guid.NewGuid ();
			var tnef1 = new TnefNameId (guid, 17);
			var tnef2 = new TnefNameId (guid, 17);

			Assert.That (tnef2.GetHashCode (), Is.EqualTo (tnef1.GetHashCode ()), "GetHashCode");
			Assert.That (tnef2, Is.EqualTo (tnef1), "Equals");
			Assert.That ((object) tnef2, Is.EqualTo ((object) tnef1), "Equals (object)");

			Assert.That (tnef1 == tnef2, Is.True, "==");
			Assert.That (tnef1 != tnef2, Is.False, "!=");
		}

		[Test]
		public void TestEqualityByName ()
		{
			var guid = Guid.NewGuid ();
			var tnef1 = new TnefNameId (guid, "name");
			var tnef2 = new TnefNameId (guid, "name");

			Assert.That (tnef2.GetHashCode (), Is.EqualTo (tnef1.GetHashCode ()), "GetHashCode");
			Assert.That (tnef2, Is.EqualTo (tnef1), "Equals");
			Assert.That ((object) tnef2, Is.EqualTo ((object) tnef1), "Equals (object)");

			Assert.That (tnef1 == tnef2, Is.True, "==");
			Assert.That (tnef1 != tnef2, Is.False, "!=");
		}
	}
}

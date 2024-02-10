//
// RsaSignaturePaddingTests.cs
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

using System.Reflection;

using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class RsaSignaturePaddingTests
	{
		[Test]
		public void TestEquality ()
		{
			var pkcs1 = RsaSignaturePadding.Pkcs1;
			var pss = RsaSignaturePadding.Pss;

			Assert.That (pkcs1, Is.EqualTo (RsaSignaturePadding.Pkcs1), "Pkcs1 Equals Pkcs1");
			Assert.That (pss, Is.EqualTo (RsaSignaturePadding.Pss), "Pss Equals Pss");

			Assert.That (RsaSignaturePadding.Pss, Is.Not.EqualTo (RsaSignaturePadding.Pkcs1), "Pkcs1 !Equals Pss");
			Assert.That (RsaSignaturePadding.Pkcs1, Is.Not.EqualTo (RsaSignaturePadding.Pss), "Pss !Equals Pkcs1");

			Assert.That (new object (), Is.Not.EqualTo (RsaSignaturePadding.Pkcs1), "Pkcs1 !Equals object");
			Assert.That (new object (), Is.Not.EqualTo (RsaSignaturePadding.Pss), "Pss !Equals object");

			Assert.That (pkcs1 == RsaSignaturePadding.Pkcs1, Is.True, "Pkcs1 == Pkcs1");
			Assert.That (pss == RsaSignaturePadding.Pss, Is.True, "Pss == Pss");
			Assert.That (pkcs1 == pss, Is.False, "Pkcs1 == Pss");
			Assert.That (pss == pkcs1, Is.False, "Pss == Pkcs1");
			Assert.That (pkcs1 == null, Is.False, "Pkcs1 == null");
			Assert.That (null == pkcs1, Is.False, "null == Pkcs1");

			Assert.That (pkcs1 != RsaSignaturePadding.Pkcs1, Is.False, "Pkcs1 != Pkcs1");
			Assert.That (pss != RsaSignaturePadding.Pss, Is.False, "Pss != Pss");
			Assert.That (pkcs1 != pss, Is.True, "Pkcs1 != Pss");
			Assert.That (pss != pkcs1, Is.True, "Pss != Pkcs1");
			Assert.That (pkcs1 != null, Is.True, "Pkcs1 != null");
			Assert.That (null != pkcs1, Is.True, "null != Pkcs1");
		}

		[Test]
		public void TestGetHashCode ()
		{
			var hashCodes = new Dictionary<int, RsaSignaturePadding> ();

			foreach (var field in typeof (RsaSignaturePadding).GetFields (BindingFlags.Public | BindingFlags.Static)) {
				if (field.FieldType != typeof (RsaSignaturePadding))
					continue;

				var padding = (RsaSignaturePadding) field.GetValue (null);
				int hashCode = padding.GetHashCode ();

				if (hashCodes.TryGetValue (hashCode, out var other))
					Assert.Fail ($"{padding.Scheme} shares the same hash code as {other.Scheme}");

				hashCodes.Add (hashCode, padding);
			}
		}

		[Test]
		public void TestToString ()
		{
			Assert.That (RsaSignaturePadding.Pkcs1.ToString (), Is.EqualTo ("Pkcs1"), "Pkcs1");
			Assert.That (RsaSignaturePadding.Pss.ToString (), Is.EqualTo ("Pss"), "Pss");
		}
	}
}

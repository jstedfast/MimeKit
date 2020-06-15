//
// RsaSignaturePaddingTests.cs
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
using System.Reflection;
using System.Collections.Generic;

using NUnit.Framework;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;

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

			Assert.AreEqual (RsaSignaturePadding.Pkcs1, pkcs1, "Pkcs1 Equals Pkcs1");
			Assert.AreEqual (RsaSignaturePadding.Pss, pss, "Pss Equals Pss");

			Assert.AreNotEqual (RsaSignaturePadding.Pkcs1, RsaSignaturePadding.Pss, "Pkcs1 !Equals Pss");
			Assert.AreNotEqual (RsaSignaturePadding.Pss, RsaSignaturePadding.Pkcs1, "Pss !Equals Pkcs1");

			Assert.AreNotEqual (RsaSignaturePadding.Pkcs1, new object (), "Pkcs1 !Equals object");
			Assert.AreNotEqual (RsaSignaturePadding.Pss, new object (), "Pss !Equals object");

			Assert.IsTrue (pkcs1 == RsaSignaturePadding.Pkcs1, "Pkcs1 == Pkcs1");
			Assert.IsTrue (pss == RsaSignaturePadding.Pss, "Pss == Pss");
			Assert.IsFalse (pkcs1 == pss, "Pkcs1 == Pss");
			Assert.IsFalse (pss == pkcs1, "Pss == Pkcs1");
			Assert.IsFalse (pkcs1 == null, "Pkcs1 == null");
			Assert.IsFalse (null == pkcs1, "null == Pkcs1");

			Assert.IsFalse (pkcs1 != RsaSignaturePadding.Pkcs1, "Pkcs1 != Pkcs1");
			Assert.IsFalse (pss != RsaSignaturePadding.Pss, "Pss != Pss");
			Assert.IsTrue (pkcs1 != pss, "Pkcs1 != Pss");
			Assert.IsTrue (pss != pkcs1, "Pss != Pkcs1");
			Assert.IsTrue (pkcs1 != null, "Pkcs1 != null");
			Assert.IsTrue (null != pkcs1, "null != Pkcs1");
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
			Assert.AreEqual ("Pkcs1", RsaSignaturePadding.Pkcs1.ToString (), "Pkcs1");
			Assert.AreEqual ("Pss", RsaSignaturePadding.Pss.ToString (), "Pss");
		}
	}
}

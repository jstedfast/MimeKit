﻿//
// MimeTypeTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 Xamarin Inc. (www.xamarin.com)
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

namespace UnitTests {
	[TestFixture]
	public class MimeTypeTests
	{
		[Test]
		public void TestGetMimeTypeNullFileName ()
		{
			Assert.Throws<ArgumentNullException> (() => MimeTypes.GetMimeType (null));
		}

		[Test]
		public void TestGetMimeTypeNoFileExtension ()
		{
			Assert.AreEqual ("application/octet-stream", MimeTypes.GetMimeType ("filename"));
		}

		[Test]
		public void TestGetMimeTypeFileNameDot ()
		{
			Assert.AreEqual ("application/octet-stream", MimeTypes.GetMimeType ("filename."));
		}

		[Test]
		public void TestGetMimeTypeFileExtensionTxt ()
		{
			Assert.AreEqual ("text/plain", MimeTypes.GetMimeType ("filename.txt"));
		}

		[Test]
		public void TestGetMimeTypeFileExtensionCsv ()
		{
			Assert.AreEqual ("text/csv", MimeTypes.GetMimeType ("filename.csv"));
		}

		[Test]
		public void TestTryGetExtensionTextPlain ()
		{
			string extension;

			Assert.IsTrue (MimeTypes.TryGetExtension ("text/plain", out extension));
			Assert.AreEqual (".txt", extension);
		}

		[Test]
		public void TestTryGetExtensionUnknownMimeType ()
		{
			string extension;

			Assert.IsFalse (MimeTypes.TryGetExtension ("application/x-vnd.fake-mime-type", out extension));
		}
	}
}

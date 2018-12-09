//
// TnefReaderTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2017 Xamarin Inc. (www.xamarin.com)
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
using System.IO;

using NUnit.Framework;

using MimeKit.Tnef;

namespace UnitTests.Tnef {
	[TestFixture]
	public class TnefReaderTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			using (var stream = File.OpenRead ("../../TestData/tnef/winmail.tnef")) {
				Assert.Throws<ArgumentNullException> (() => new TnefReader (null, 0, TnefComplianceMode.Strict));
				Assert.Throws<ArgumentOutOfRangeException> (() => new TnefReader (stream, -1, TnefComplianceMode.Strict));

				using (var reader = new TnefReader (stream, 1252, TnefComplianceMode.Strict)) {
					var buffer = new byte[16];

					Assert.Throws<ArgumentNullException> (() => reader.ReadAttributeRawValue (null, 0, buffer.Length));
					Assert.Throws<ArgumentOutOfRangeException> (() => reader.ReadAttributeRawValue (buffer, -1, buffer.Length));
					Assert.Throws<ArgumentOutOfRangeException> (() => reader.ReadAttributeRawValue (buffer, 0, -1));
				}
			}
		}

		[Test]
		public void TestSetComplianceError ()
		{
			using (var stream = File.OpenRead ("../../TestData/tnef/winmail.tnef")) {
				using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Strict)) {
					foreach (TnefComplianceStatus error in Enum.GetValues (typeof (TnefComplianceStatus))) {
						if (error == TnefComplianceStatus.Compliant) {
							Assert.DoesNotThrow (() => reader.SetComplianceError (error));
						} else {
							Assert.Throws<TnefException> (() => reader.SetComplianceError (error));
						}
					}
				}
			}
		}

		[Test]
		public void TestTruncatedHeader ()
		{
			using (var stream = new MemoryStream ()) {
				Assert.Throws<TnefException> (() => new TnefReader (stream, 0, TnefComplianceMode.Strict));

				var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose);
				Assert.AreEqual (TnefComplianceStatus.StreamTruncated, reader.ComplianceStatus);
			}
		}
	}
}

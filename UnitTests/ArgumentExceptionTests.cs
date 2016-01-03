//
// ArgumentExceptionTests.cs
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
using System.Reflection;

using NUnit.Framework;

using MimeKit;
using MimeKit.IO;
using MimeKit.IO.Filters;
using MimeKit.Cryptography;

namespace UnitTests {
	[TestFixture]
	public class ArgumentExceptionTests
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
		public void TestFilterArguments ()
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

		static void AssertParseArguments (Type type)
		{
			const string text = "this is a dummy text buffer";
			var options = ParserOptions.Default;
			var buffer = new byte[1024];

			foreach (var method in type.GetMethods (BindingFlags.Public | BindingFlags.Static)) {
				if (method.Name != "Parse")
					continue;

				var parameters = method.GetParameters ();
				var args = new object[parameters.Length];
				TargetInvocationException tie;
				ArgumentException ex;
				int bufferIndex = 0;
				int idx = 0;
				int length;

				if (parameters[idx].ParameterType == typeof (ParserOptions))
					args[idx++] = null;

				// this is either a byte[] or string buffer
				bufferIndex = idx;
				if (parameters[idx].ParameterType == typeof (byte[])) {
					length = buffer.Length;
					args[idx++] = buffer;
				} else {
					length = text.Length;
					args[idx++] = text;
				}

				for (int i = idx; i < parameters.Length; i++) {
					switch (parameters[i].Name) {
					case "startIndex": args[i] = 0; break;
					case "length": args[i] = length; break;
					default:
						Assert.Fail ("Unknown parameter: {0} for {1}.Parse", parameters[i].Name, type.Name);
						break;
					}
				}

				if (bufferIndex == 1) {
					tie = Assert.Throws<TargetInvocationException> (() => method.Invoke (null, args),
						"{0}.Parse did not throw an exception when options was null.", type.Name);
					Assert.IsInstanceOf<ArgumentNullException> (tie.InnerException);
					ex = (ArgumentException) tie.InnerException;
					Assert.AreEqual ("options", ex.ParamName);

					args[0] = options;
				}

				var buf = args[bufferIndex];
				args[bufferIndex] = null;
				tie = Assert.Throws<TargetInvocationException> (() => method.Invoke (null, args),
					"{0}.Parse did not throw an exception when {1} was null.", type.Name, parameters[bufferIndex].Name);
				Assert.IsInstanceOf<ArgumentNullException> (tie.InnerException);
				ex = (ArgumentException) tie.InnerException;
				Assert.AreEqual (parameters[bufferIndex].Name, ex.ParamName);
				args[bufferIndex] = buf;

				if (idx < parameters.Length) {
					// startIndex
					args[idx] = -1;

					tie = Assert.Throws<TargetInvocationException> (() => method.Invoke (null, args),
						"{0}.Parse did not throw ArgumentOutOfRangeException when {1} was -1.", type.Name, parameters[idx].Name);
					Assert.IsInstanceOf<ArgumentOutOfRangeException> (tie.InnerException);
					ex = (ArgumentException) tie.InnerException;
					Assert.AreEqual (parameters[idx].Name, ex.ParamName);

					args[idx] = length + 1;

					tie = Assert.Throws<TargetInvocationException> (() => method.Invoke (null, args),
						"{0}.Parse did not throw an exception when {1} was > length.", type.Name, parameters[idx].Name);
					Assert.IsInstanceOf<ArgumentOutOfRangeException> (tie.InnerException);
					ex = (ArgumentException) tie.InnerException;
					Assert.AreEqual (parameters[idx].Name, ex.ParamName);

					args[idx++] = 0;
				}

				if (idx < parameters.Length) {
					// length
					args[idx] = -1;

					tie = Assert.Throws<TargetInvocationException> (() => method.Invoke (null, args),
						"{0}.Parse did not throw an exception when {1} was -1.", type.Name, parameters[idx].Name);
					Assert.IsInstanceOf<ArgumentOutOfRangeException> (tie.InnerException);
					ex = (ArgumentException) tie.InnerException;
					Assert.AreEqual (parameters[idx].Name, ex.ParamName);

					args[idx] = length + 1;

					tie = Assert.Throws<TargetInvocationException> (() => method.Invoke (null, args),
						"{0}.Parse did not throw an exception when {1} was > length.", type.Name, parameters[idx].Name);
					Assert.IsInstanceOf<ArgumentOutOfRangeException> (tie.InnerException);
					ex = (ArgumentException) tie.InnerException;
					Assert.AreEqual (parameters[idx].Name, ex.ParamName);

					idx++;
				}
			}
		}

		[Test]
		public void TestParseArguments ()
		{
			AssertParseArguments (typeof (GroupAddress));
			AssertParseArguments (typeof (MailboxAddress));
			AssertParseArguments (typeof (InternetAddress));
			AssertParseArguments (typeof (InternetAddressList));

			AssertParseArguments (typeof (ContentDisposition));
			AssertParseArguments (typeof (ContentType));
		}
	}
}

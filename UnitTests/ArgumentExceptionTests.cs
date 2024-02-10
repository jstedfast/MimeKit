//
// ArgumentExceptionTests.cs
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

using MimeKit;
using MimeKit.IO;
using MimeKit.Utils;
using MimeKit.IO.Filters;
using MimeKit.Cryptography;

namespace UnitTests {
	[TestFixture]
	public class ArgumentExceptionTests
	{
		static void AssertFilterArguments (IMimeFilter filter)
		{
			int outputIndex, outputLength;
			var input = new byte[1024];
			ArgumentException ex;

			// Filter
			Assert.Throws<ArgumentNullException> (() => filter.Filter (null, 0, 0, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentNullException when input was null.", filter.GetType ().Name);

			ex = Assert.Throws<ArgumentOutOfRangeException> (() => filter.Filter (input, -1, 0, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentOutOfRangeException when startIndex was -1.", filter.GetType ().Name);
			Assert.That (ex.ParamName, Is.EqualTo ("startIndex"));

			ex = Assert.Throws<ArgumentOutOfRangeException> (() => filter.Filter (input, 0, -1, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentOutOfRangeException when length was -1.", filter.GetType ().Name);
			Assert.That (ex.ParamName, Is.EqualTo ("length"));

			ex = Assert.Throws<ArgumentOutOfRangeException> (() => filter.Filter (input, 1025, 0, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentOutOfRangeException when startIndex was > 1024.", filter.GetType ().Name);
			Assert.That (ex.ParamName, Is.EqualTo ("startIndex"));

			ex = Assert.Throws<ArgumentOutOfRangeException> (() => filter.Filter (input, 0, 1025, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentOutOfRangeException when length was > 1024.", filter.GetType ().Name);
			Assert.That (ex.ParamName, Is.EqualTo ("length"));

			// Flush
			Assert.Throws<ArgumentNullException> (() => filter.Flush (null, 0, 0, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentNullException when input was null.", filter.GetType ().Name);

			ex = Assert.Throws<ArgumentOutOfRangeException> (() => filter.Flush (input, -1, 0, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentOutOfRangeException when startIndex was -1.", filter.GetType ().Name);
			Assert.That (ex.ParamName, Is.EqualTo ("startIndex"));

			ex = Assert.Throws<ArgumentOutOfRangeException> (() => filter.Flush (input, 0, -1, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentOutOfRangeException when length was -1.", filter.GetType ().Name);
			Assert.That (ex.ParamName, Is.EqualTo ("length"));

			ex = Assert.Throws<ArgumentOutOfRangeException> (() => filter.Flush (input, 1025, 0, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentOutOfRangeException when startIndex was > 1024.", filter.GetType ().Name);
			Assert.That (ex.ParamName, Is.EqualTo ("startIndex"));

			ex = Assert.Throws<ArgumentOutOfRangeException> (() => filter.Flush (input, 0, 1025, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentOutOfRangeException when length was > 1024.", filter.GetType ().Name);
			Assert.That (ex.ParamName, Is.EqualTo ("length"));

			filter.Reset ();
		}

		[Test]
		public void TestFilterArguments ()
		{
			Assert.Throws<ArgumentNullException> (() => new EncoderFilter (null));
			Assert.Throws<ArgumentNullException> (() => new DecoderFilter (null));

			AssertFilterArguments (new Dos2UnixFilter ());
			AssertFilterArguments (new Unix2DosFilter ());
			AssertFilterArguments (new MboxFromFilter ());
			AssertFilterArguments (new ArmoredFromFilter ());
			AssertFilterArguments (new BestEncodingFilter ());
			AssertFilterArguments (new CharsetFilter ("iso-8859-1", "utf-8"));
			AssertFilterArguments (DecoderFilter.Create (ContentEncoding.Base64));
			AssertFilterArguments (EncoderFilter.Create (ContentEncoding.Base64));
			AssertFilterArguments (DecoderFilter.Create (ContentEncoding.QuotedPrintable));
			AssertFilterArguments (EncoderFilter.Create (ContentEncoding.QuotedPrintable));
			AssertFilterArguments (DecoderFilter.Create (ContentEncoding.UUEncode));
			AssertFilterArguments (EncoderFilter.Create (ContentEncoding.UUEncode));
			AssertFilterArguments (new TrailingWhitespaceFilter ());
			AssertFilterArguments (new DkimRelaxedBodyFilter ());
			AssertFilterArguments (new DkimSimpleBodyFilter ());
		}

		static void AssertParseArguments (Type type)
		{
			const string text = "this is a dummy text buffer";
			var options = ParserOptions.Default;
			var buffer = new byte[1024];

			foreach (var method in type.GetMethods (BindingFlags.Public | BindingFlags.Static)) {
				if (method.Name != "Parse" && method.Name != "TryParse")
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

				if (idx < parameters.Length && parameters[idx].ParameterType == typeof (int)) {
					// startIndex
					args[idx++] = 0;
				}

				if (idx < parameters.Length && parameters[idx].ParameterType == typeof (int)) {
					// length
					args[idx++] = length;
				}

				if (bufferIndex == 1) {
					tie = Assert.Throws<TargetInvocationException> (() => method.Invoke (null, args),
						"{0}.{1} did not throw an exception when options was null.", type.Name, method.Name);
					Assert.That (tie.InnerException, Is.InstanceOf<ArgumentNullException> ());
					ex = (ArgumentException) tie.InnerException;
					Assert.That (ex.ParamName, Is.EqualTo ("options"));

					args[0] = options;
				}

				var buf = args[bufferIndex];
				args[bufferIndex] = null;
				tie = Assert.Throws<TargetInvocationException> (() => method.Invoke (null, args),
					"{0}.{1} did not throw an exception when {2} was null.", type.Name, method.Name, parameters[bufferIndex].Name);
				Assert.That (tie.InnerException, Is.InstanceOf<ArgumentNullException> ());
				ex = (ArgumentException) tie.InnerException;
				Assert.That (ex.ParamName, Is.EqualTo (parameters[bufferIndex].Name));
				args[bufferIndex] = buf;

				idx = bufferIndex + 1;
				if (idx < parameters.Length && parameters[idx].ParameterType == typeof (int)) {
					// startIndex
					args[idx] = -1;

					tie = Assert.Throws<TargetInvocationException> (() => method.Invoke (null, args),
						"{0}.{1} did not throw ArgumentOutOfRangeException when {2} was -1.", type.Name, method.Name, parameters[idx].Name);
					Assert.That (tie.InnerException, Is.InstanceOf<ArgumentOutOfRangeException> ());
					ex = (ArgumentException) tie.InnerException;
					Assert.That (ex.ParamName, Is.EqualTo (parameters[idx].Name));

					args[idx] = length + 1;

					tie = Assert.Throws<TargetInvocationException> (() => method.Invoke (null, args),
						"{0}.{1} did not throw an exception when {2} was > length.", type.Name, method.Name, parameters[idx].Name);
					Assert.That(tie.InnerException, Is.InstanceOf<ArgumentOutOfRangeException>());
					ex = (ArgumentException) tie.InnerException;
					Assert.That (ex.ParamName, Is.EqualTo (parameters[idx].Name));

					args[idx++] = 0;
				}

				if (idx < parameters.Length && parameters[idx].ParameterType == typeof (int)) {
					// length
					args[idx] = -1;

					tie = Assert.Throws<TargetInvocationException> (() => method.Invoke (null, args),
						"{0}.{1} did not throw an exception when {2} was -1.", type.Name, method.Name, parameters[idx].Name);
					Assert.That(tie.InnerException, Is.InstanceOf<ArgumentOutOfRangeException>());
					ex = (ArgumentException) tie.InnerException;
					Assert.That (ex.ParamName, Is.EqualTo (parameters[idx].Name));

					args[idx] = length + 1;

					tie = Assert.Throws<TargetInvocationException> (() => method.Invoke (null, args),
						"{0}.{1} did not throw an exception when {2} was > length.", type.Name, method.Name, parameters[idx].Name);
					Assert.That(tie.InnerException, Is.InstanceOf<ArgumentOutOfRangeException>());
					ex = (ArgumentException) tie.InnerException;
					Assert.That (ex.ParamName, Is.EqualTo (parameters[idx].Name));

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
			AssertParseArguments (typeof (DomainList));
			AssertParseArguments (typeof (Header));

			AssertParseArguments (typeof (DateUtils));
			AssertParseArguments (typeof (MimeUtils));
		}

		[Test]
		public void TestBufferPoolArguments ()
		{
			Assert.Throws<ArgumentOutOfRangeException> (() => new BufferPool (-1, 16));
			Assert.Throws<ArgumentOutOfRangeException> (() => new BufferPool (1024, -1));

			var pool = new BufferPool (16, 2);
			Assert.Throws<ArgumentNullException> (() => pool.Return (null));
			Assert.Throws<ArgumentException> (() => pool.Return (new byte[8]));
		}

		static void AssertStreamArguments (Stream stream)
		{
			var buffer = new byte[1024];
			ArgumentException ex;

			if (stream.CanRead) {
				ex = Assert.Throws<ArgumentNullException> (() => stream.Read (null, 0, 0),
					"{0}.Read() does not throw an ArgumentNullException when buffer is null.", stream.GetType ().Name);
				Assert.That (ex.ParamName, Is.EqualTo ("buffer"));

				ex = Assert.Throws<ArgumentOutOfRangeException> (() => stream.Read (buffer, -1, 0),
					"{0}.Read() does not throw an ArgumentOutOfRangeException when offset is -1.", stream.GetType ().Name);
				Assert.That (ex.ParamName, Is.EqualTo ("offset"));

				ex = Assert.Throws<ArgumentOutOfRangeException> (() => stream.Read (buffer, buffer.Length + 1, 0),
					"{0}.Read() does not throw an ArgumentOutOfRangeException when offset > buffer length.", stream.GetType ().Name);
				Assert.That (ex.ParamName, Is.EqualTo ("offset"));

				ex = Assert.Throws<ArgumentOutOfRangeException> (() => stream.Read (buffer, 0, -1),
					"{0}.Read() does not throw an ArgumentOutOfRangeException when count is -1.", stream.GetType ().Name);
				Assert.That (ex.ParamName, Is.EqualTo ("count"));

				ex = Assert.Throws<ArgumentOutOfRangeException> (() => stream.Read (buffer, 0, buffer.Length + 1),
					"{0}.Read() does not throw an ArgumentOutOfRangeException when count > buffer length.", stream.GetType ().Name);
				Assert.That (ex.ParamName, Is.EqualTo ("count"));

				ex = Assert.ThrowsAsync<ArgumentNullException> (async () => await stream.ReadAsync (null, 0, 0),
					"{0}.ReadAsync() does not throw an ArgumentNullException when buffer is null.", stream.GetType ().Name);
				Assert.That (ex.ParamName, Is.EqualTo ("buffer"));

				ex = Assert.ThrowsAsync<ArgumentOutOfRangeException> (async () => await stream.ReadAsync (buffer, -1, 0),
					"{0}.ReadAsync() does not throw an ArgumentOutOfRangeException when offset is -1.", stream.GetType ().Name);
				Assert.That (ex.ParamName, Is.EqualTo ("offset"));

				ex = Assert.ThrowsAsync<ArgumentOutOfRangeException> (async () => await stream.ReadAsync (buffer, buffer.Length + 1, 0),
					"{0}.ReadAsync() does not throw an ArgumentOutOfRangeException when offset > buffer length.", stream.GetType ().Name);
				Assert.That (ex.ParamName, Is.EqualTo ("offset"));

				ex = Assert.ThrowsAsync<ArgumentOutOfRangeException> (async () => await stream.ReadAsync (buffer, 0, -1),
					"{0}.ReadAsync() does not throw an ArgumentOutOfRangeException when count is -1.", stream.GetType ().Name);
				Assert.That (ex.ParamName, Is.EqualTo ("count"));

				ex = Assert.ThrowsAsync<ArgumentOutOfRangeException> (async () => await stream.ReadAsync (buffer, 0, buffer.Length + 1),
					"{0}.ReadAsync() does not throw an ArgumentOutOfRangeException when count > buffer length.", stream.GetType ().Name);
				Assert.That (ex.ParamName, Is.EqualTo ("count"));
			} else {
				Assert.Throws<NotSupportedException> (() => stream.Read (buffer, 0, buffer.Length),
					"{0}.Read() does not throw a NotSupportedException when CanRead is false.", stream.GetType ().Name);

				Assert.ThrowsAsync<NotSupportedException> (async () => await stream.ReadAsync (buffer, 0, buffer.Length),
					"{0}.ReadAsync() does not throw a NotSupportedException when CanRead is false.", stream.GetType ().Name);
			}

			if (stream.CanWrite) {
				ex = Assert.Throws<ArgumentNullException> (() => stream.Write (null, 0, 0),
					"{0}.Write() does not throw an ArgumentNullException when buffer is null.", stream.GetType ().Name);
				Assert.That (ex.ParamName, Is.EqualTo ("buffer"));

				ex = Assert.Throws<ArgumentOutOfRangeException> (() => stream.Write (buffer, -1, 0),
					"{0}.Write() does not throw an ArgumentOutOfRangeException when offset is -1.", stream.GetType ().Name);
				Assert.That (ex.ParamName, Is.EqualTo ("offset"));

				ex = Assert.Throws<ArgumentOutOfRangeException> (() => stream.Write (buffer, buffer.Length + 1, 0),
					"{0}.Write() does not throw an ArgumentOutOfRangeException when offset > buffer length.", stream.GetType ().Name);
				Assert.That (ex.ParamName, Is.EqualTo ("offset"));

				ex = Assert.Throws<ArgumentOutOfRangeException> (() => stream.Write (buffer, 0, -1),
					"{0}.Write() does not throw an ArgumentOutOfRangeException when count is -1.", stream.GetType ().Name);
				Assert.That (ex.ParamName, Is.EqualTo ("count"));

				ex = Assert.Throws<ArgumentOutOfRangeException> (() => stream.Write (buffer, 0, buffer.Length + 1),
					"{0}.Write() does not throw an ArgumentOutOfRangeException when count > buffer length.", stream.GetType ().Name);
				Assert.That (ex.ParamName, Is.EqualTo ("count"));

				ex = Assert.ThrowsAsync<ArgumentNullException> (async () => await stream.WriteAsync (null, 0, 0),
					"{0}.WriteAsync() does not throw an ArgumentNullException when buffer is null.", stream.GetType ().Name);
				Assert.That (ex.ParamName, Is.EqualTo ("buffer"));

				ex = Assert.ThrowsAsync<ArgumentOutOfRangeException> (async () => await stream.WriteAsync (buffer, -1, 0),
					"{0}.WriteAsync() does not throw an ArgumentOutOfRangeException when offset is -1.", stream.GetType ().Name);
				Assert.That (ex.ParamName, Is.EqualTo ("offset"));

				ex = Assert.ThrowsAsync<ArgumentOutOfRangeException> (async () => await stream.WriteAsync (buffer, buffer.Length + 1, 0),
					"{0}.WriteAsync() does not throw an ArgumentOutOfRangeException when offset > buffer length.", stream.GetType ().Name);
				Assert.That (ex.ParamName, Is.EqualTo ("offset"));

				ex = Assert.ThrowsAsync<ArgumentOutOfRangeException> (async () => await stream.WriteAsync (buffer, 0, -1),
					"{0}.WriteAsync() does not throw an ArgumentOutOfRangeException when count is -1.", stream.GetType ().Name);
				Assert.That (ex.ParamName, Is.EqualTo ("count"));

				ex = Assert.ThrowsAsync<ArgumentOutOfRangeException> (async () => await stream.WriteAsync (buffer, 0, buffer.Length + 1),
					"{0}.WriteAsync() does not throw an ArgumentOutOfRangeException when count > buffer length.", stream.GetType ().Name);
				Assert.That (ex.ParamName, Is.EqualTo ("count"));
			} else {
				Assert.Throws<NotSupportedException> (() => stream.Write (buffer, 0, buffer.Length),
					"{0}.Write() does not throw a NotSupportedException when CanWrite is false.", stream.GetType ().Name);

				Assert.ThrowsAsync<NotSupportedException> (async () => await stream.WriteAsync (buffer, 0, buffer.Length),
					"{0}.WriteAsync() does not throw a NotSupportedException when CanWrite is false.", stream.GetType ().Name);
			}

			if (stream.CanSeek) {
				ex = Assert.Throws<ArgumentOutOfRangeException> (() => stream.Seek (0, (SeekOrigin) 255),
					"{0}.Seek() does not throw an ArgumentOutOfRangeException when origin is invalid.", stream.GetType ().Name);
				Assert.That (ex.ParamName, Is.EqualTo ("origin"));
			} else {
				Assert.Throws<NotSupportedException> (() => stream.Seek (0, SeekOrigin.Begin),
					"{0}.Seek() does not throw a NotSupportedException when CanSeek is false.", stream.GetType ().Name);
			}
		}

		[Test]
		public void TestStreamArguments ()
		{
			using (var stream = new MeasuringStream ())
				AssertStreamArguments (stream);

			using (var stream = new MemoryBlockStream ())
				AssertStreamArguments (stream);

			using (var memory = new MemoryStream ()) {
				Assert.Throws<ArgumentNullException> (() => new FilteredStream (null));

				using (var stream = new FilteredStream (memory))
					AssertStreamArguments (stream);
			}

			using (var memory = new MemoryStream ()) {
				Assert.Throws<ArgumentNullException> (() => new BoundStream (null, 0, 10, true));
				Assert.Throws<ArgumentOutOfRangeException> (() => new BoundStream (memory, -1, 10, true));
				Assert.Throws<ArgumentOutOfRangeException> (() => new BoundStream (memory, 5, 1, true));

				using (var stream = new BoundStream (memory, 0, -1, true))
					AssertStreamArguments (stream);
			}

			using (var memory = new MemoryStream ()) {
				using (var stream = new ChainedStream ()) {
					stream.Add (memory);

					Assert.Throws<ArgumentNullException> (() => stream.Add (null));

					AssertStreamArguments (stream);
				}
			}
		}

		[Test]
		public void TestAcceptArguments ()
		{
			Assert.Throws<ArgumentNullException> (() => new MessagePart ().Accept (null));
		}

		[Test]
		public void TestCrc32Arguments ()
		{
			var crc32 = new Crc32 ();
			var buffer = new byte[10];

			Assert.Throws<ArgumentNullException> (() => crc32.Update (null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => crc32.Update (buffer, -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => crc32.Update (buffer, 0, 20));
		}

		[Test]
		public void TestMimeMessageBeginEventArgs ()
		{
			Assert.Throws<ArgumentNullException> (() => new MimeMessageBeginEventArgs (null));
			Assert.Throws<ArgumentNullException> (() => new MimeMessageBeginEventArgs (null, new MessagePart ()));
			Assert.Throws<ArgumentNullException> (() => new MimeMessageBeginEventArgs (new MimeMessage (), null));
		}

		[Test]
		public void TestMimeMessageEndEventArgs ()
		{
			Assert.Throws<ArgumentNullException> (() => new MimeMessageEndEventArgs (null));
			Assert.Throws<ArgumentNullException> (() => new MimeMessageEndEventArgs (null, new MessagePart ()));
			Assert.Throws<ArgumentNullException> (() => new MimeMessageEndEventArgs (new MimeMessage (), null));
		}

		[Test]
		public void TestMimeEntityBeginEventArgs ()
		{
			Assert.Throws<ArgumentNullException> (() => new MimeEntityBeginEventArgs (null));
			Assert.Throws<ArgumentNullException> (() => new MimeEntityBeginEventArgs (null, new Multipart ()));
			Assert.Throws<ArgumentNullException> (() => new MimeEntityBeginEventArgs (new MimePart (), null));
		}

		[Test]
		public void TestMimeEntityEndEventArgs ()
		{
			Assert.Throws<ArgumentNullException> (() => new MimeEntityEndEventArgs (null));
			Assert.Throws<ArgumentNullException> (() => new MimeEntityEndEventArgs (null, new Multipart ()));
			Assert.Throws<ArgumentNullException> (() => new MimeEntityEndEventArgs (new MimePart (), null));
		}
	}
}

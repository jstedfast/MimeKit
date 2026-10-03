//
// TnefConversionTestHelper.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
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


using MimeKit;
using MimeKit.Tnef;

namespace UnitTests.Tnef {
	static class TnefConversionTestHelper
	{
		public static MimeMessage Convert (TnefMessage tnef, TnefConversionOptions options = null)
		{
			// The converted message is independent of the TnefMessage and the result only owns the message,
			// so returning the message without disposing the result is safe.
			return tnef.ConvertToMime (options).Message;
		}

		public static MimeMessage Convert (TnefPart part, TnefConversionOptions options = null)
		{
			using var tnef = part.LoadTnefMessage ();

			return Convert (tnef, options);
		}

		public static MimeMessage Convert (Stream stream, TnefConversionOptions options = null)
		{
			using var tnef = TnefMessage.Load (stream);

			return Convert (tnef, options);
		}

		public static MimeMessage Convert (TnefReader reader, TnefConversionOptions options = null)
		{
			using var tnef = TnefMessage.Load (reader);

			return Convert (tnef, options);
		}

		// Arbitrary (fuzzed) input: the only documented failure is a stream that does not start with the TNEF signature.
		public static MimeMessage ConvertArbitrary (byte[] data)
		{
			try {
				using var stream = new MemoryStream (data, false);

				return Convert (stream);
			} catch (TnefException ex) when (ex.Violation == TnefComplianceViolation.InvalidSignature) {
				return new MimeMessage ();
			}
		}
	}
}
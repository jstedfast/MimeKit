//
// ArgumentValidator.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2025 .NET Foundation and Contributors
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
using System.Diagnostics.CodeAnalysis;

namespace MimeKit.Utils {
	static class ArgumentValidator
	{
		public static bool TryValidate ([NotNullWhen (true)] ParserOptions? options, [NotNullWhen (true)] byte[]? buffer, int startIndex, int length)
		{
			if (options is null)
				return false;

			if (buffer is null)
				return false;

			if (startIndex < 0 || startIndex > buffer.Length)
				return false;

			if (length < 0 || length > (buffer.Length - startIndex))
				return false;

			return true;
		}

		public static bool TryValidate ([NotNullWhen (true)] ParserOptions? options, [NotNullWhen (true)] byte[]? buffer, int startIndex)
		{
			if (options is null)
				return false;

			if (buffer is null)
				return false;

			if (startIndex < 0 || startIndex > buffer.Length)
				return false;

			return true;
		}

		public static bool TryValidate ([NotNullWhen (true)] ParserOptions? options, [NotNullWhen (true)] byte[]? buffer)
		{
			if (options is null)
				return false;

			if (buffer is null)
				return false;

			return true;
		}

		public static bool TryValidate ([NotNullWhen (true)] ParserOptions? options, [NotNullWhen (true)] string? text)
		{
			if (options is null)
				return false;

			if (text is null)
				return false;

			return true;
		}

		public static bool TryValidate ([NotNullWhen (true)] byte[]? buffer, int startIndex, int length)
		{
			if (buffer is null)
				return false;

			if (startIndex < 0 || startIndex > buffer.Length)
				return false;

			if (length < 0 || length > (buffer.Length - startIndex))
				return false;

			return true;
		}

		public static bool TryValidate ([NotNullWhen (true)] byte[]? buffer, int startIndex)
		{
			if (buffer is null)
				return false;

			if (startIndex < 0 || startIndex > buffer.Length)
				return false;

			return true;
		}

		public static void Validate ([NotNull] ParserOptions? options, [NotNull] byte[]? buffer, int startIndex, int length)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (buffer is null)
				throw new ArgumentNullException (nameof (buffer));

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (length < 0 || length > (buffer.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (length));
		}

		public static void Validate ([NotNull] ParserOptions? options, [NotNull] byte[]? buffer, int startIndex)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (buffer is null)
				throw new ArgumentNullException (nameof (buffer));

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));
		}

		public static void Validate ([NotNull] ParserOptions? options, [NotNull] byte[]? buffer)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (buffer is null)
				throw new ArgumentNullException (nameof (buffer));
		}

		public static void Validate ([NotNull] ParserOptions? options, [NotNull] string? text)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (text is null)
				throw new ArgumentNullException (nameof (text));
		}

		public static void Validate ([NotNull] byte[]? buffer, int startIndex, int length)
		{
			if (buffer is null)
				throw new ArgumentNullException (nameof (buffer));

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (length < 0 || length > (buffer.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (length));
		}
	}
}

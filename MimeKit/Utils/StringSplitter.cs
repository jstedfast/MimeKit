//
// StringSplitter.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2022 .NET Foundation and Contributors
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

namespace MimeKit.Utils {
	internal ref struct StringSplitter
	{
		private readonly ReadOnlySpan<char> text;
		private readonly char seperator;
		private int position;

		public StringSplitter (ReadOnlySpan<char> text, char seperator)
		{
			this.text = text;
			this.seperator = seperator;
			this.position = 0;
		}

		public bool TryReadNext (out ReadOnlySpan<char> item)
		{
			if (IsEof) {
				item = default;
				return false;
			}

			int start = position;

			int seperatorIndex = text.Slice (position).IndexOf (seperator);

			if (seperatorIndex > -1) {
				position += seperatorIndex + 1;

				item = text.Slice (start, seperatorIndex);
			} else {
				position = text.Length;

				item = text.Slice (start);
			}

			return true;
		}

		public bool IsEof => position == text.Length;
	}
}

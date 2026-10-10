//
// OneCharTextReader.cs
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

namespace MimeKit.Fuzzing.Helpers {
	// A TextReader that returns at most one character per Read call, used to verify that consumers produce
	// the same output regardless of how their input is chunked.
	sealed class OneCharTextReader : TextReader
	{
		readonly string text;
		int index;

		public OneCharTextReader (string text)
		{
			this.text = text;
		}

		public override int Peek ()
		{
			return index < text.Length ? text[index] : -1;
		}

		public override int Read ()
		{
			return index < text.Length ? text[index++] : -1;
		}

		public override int Read (char[] buffer, int index, int count)
		{
			return Read (buffer.AsSpan (index, count));
		}

		public override int Read (Span<char> buffer)
		{
			if (buffer.Length == 0 || index >= text.Length)
				return 0;

			buffer[0] = text[index++];

			return 1;
		}

		public override Task<int> ReadAsync (char[] buffer, int index, int count)
		{
			return Task.FromResult (Read (buffer, index, count));
		}

		public override ValueTask<int> ReadAsync (Memory<char> buffer, CancellationToken cancellationToken = default)
		{
			return new ValueTask<int> (Read (buffer.Span));
		}

		// Note: ReadBlock/ReadBlockAsync are deliberately not overridden; the base implementations loop over
		// Read until the buffer is full, which is the documented contract.
	}
}

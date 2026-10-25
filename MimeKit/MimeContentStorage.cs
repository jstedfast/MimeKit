//
// MimeContentStorage.cs
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

namespace MimeKit {
	/// <summary>
	/// The storage used for parsed MIME part content.
	/// </summary>
	/// <remarks>
	/// The input stream remains owned by the caller in both modes.
	/// </remarks>
	public enum MimeContentStorage {
		/// <summary>
		/// Copy MIME part content into memory. This is the default and allows the
		/// caller to close or modify the input stream after parsing.
		/// </summary>
		Memory = 0,

		/// <summary>
		/// Reference MIME part content in the seekable input stream instead of copying it.
		/// The caller must keep the stream open and its contents unchanged while the
		/// parsed content is in use. Disposing parsed messages or entities does not close the stream.
		/// </summary>
		Source = 1
	}
}

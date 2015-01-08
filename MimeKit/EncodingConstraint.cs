//
// EncodingConstraint.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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
	/// A content encoding constraint.
	/// </summary>
	/// <remarks>
	/// Not all message transports support binary or 8-bit data, so it becomes
	/// necessary to constrain the content encoding to a subset of the possible
	/// Content-Transfer-Encoding values.
	/// </remarks>
	public enum EncodingConstraint {
		/// <summary>
		/// There are no encoding constraints, the content may contain any byte.
		/// </summary>
		None,

		/// <summary>
		/// The content may contain bytes with the high bit set, but must not contain any zero-bytes.
		/// </summary>
		EightBit,

		/// <summary>
		/// The content may only contain bytes within the 7-bit ASCII range.
		/// </summary>
		SevenBit,
	}
}

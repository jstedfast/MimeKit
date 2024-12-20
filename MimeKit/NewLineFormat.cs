//
// NewLineFormat.cs
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

namespace MimeKit {
	/// <summary>
	/// A New-Line format.
	/// </summary>
	/// <remarks>
	/// There are two commonly used line-endings used by modern Operating Systems.
	/// Unix-based systems such as Linux and macOS use a single character (<c>'\n'</c> aka LF)
	/// to represent the end of line where-as Windows (or DOS) uses a sequence of two
	/// characters (<c>"\r\n"</c> aka CRLF). Most text-based network protocols such as SMTP,
	/// POP3, and IMAP use the CRLF sequence as well.
	/// </remarks>
	public enum NewLineFormat : byte {
		/// <summary>
		/// The Unix New-Line format (<c>"\n"</c>).
		/// </summary>
		Unix,

		/// <summary>
		/// The DOS New-Line format (<c>"\r\n"</c>).
		/// </summary>
		Dos,

		/// <summary>
		/// A mixed New-Line format where some lines use Unix-based line endings and
		/// other lines use DOS-based line endings.
		/// </summary>
		Mixed,
	}
}

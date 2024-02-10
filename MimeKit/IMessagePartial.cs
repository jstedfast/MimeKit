//
// IMessagePartial.cs
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
	/// An interface for a MIME part containing a partial message as its content.
	/// </summary>
	/// <remarks>
	/// <para>The "message/partial" MIME-type is used to split large messages into
	/// multiple parts, typically to work around transport systems that have size
	/// limitations (for example, some SMTP servers limit have a maximum message
	/// size that they will accept).</para>
	/// </remarks>
	public interface IMessagePartial : IMimePart
	{
		/// <summary>
		/// Get the "id" parameter of the Content-Type header.
		/// </summary>
		/// <remarks>
		/// The "id" parameter is a unique identifier used to match the parts together.
		/// </remarks>
		/// <value>The identifier.</value>
		string Id {
			get;
		}

		/// <summary>
		/// Get the "number" parameter of the Content-Type header.
		/// </summary>
		/// <remarks>
		/// The "number" parameter is the sequential (1-based) index of the partial message fragment.
		/// </remarks>
		/// <value>The part number.</value>
		int? Number {
			get;
		}

		/// <summary>
		/// Get the "total" parameter of the Content-Type header.
		/// </summary>
		/// <remarks>
		/// The "total" parameter is the total number of pieces that make up the complete message.
		/// </remarks>
		/// <value>The total number of parts.</value>
		int? Total {
			get;
		}
	}
}

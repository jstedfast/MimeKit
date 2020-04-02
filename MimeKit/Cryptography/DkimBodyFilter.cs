//
// DkimBodyFilterBase.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 Xamarin Inc. (www.xamarin.com)
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

using MimeKit.IO.Filters;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A base implementation for DKIM body filters.
	/// </summary>
	/// <remarks>
	/// A base implementation for DKIM body filters.
	/// </remarks>
	abstract class DkimBodyFilter : MimeFilterBase
	{
		/// <summary>
		/// Get or set whether the last filtered character was a newline.
		/// </summary>
		/// <remarks>
		/// Gets or sets whether the last filtered character was a newline.
		/// </remarks>
		internal protected bool LastWasNewLine;

		/// <summary>
		/// Get or set whether the current line is empty.
		/// </summary>
		/// <remarks>
		/// Gets or sets whether the current line is empty.
		/// </remarks>
		protected bool IsEmptyLine;

		/// <summary>
		/// Get or set the number of consecutive empty lines encountered.
		/// </summary>
		/// <remarks>
		/// Gets or sets the number of consecutive empty lines encountered.
		/// </remarks>
		protected int EmptyLines;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.DkimBodyFilter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="DkimBodyFilter"/>.
		/// </remarks>
		protected DkimBodyFilter ()
		{
		}
	}
}

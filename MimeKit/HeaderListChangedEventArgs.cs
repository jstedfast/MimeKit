//
// HeaderChangedEventArgs.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc.
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

namespace MimeKit {
	/// <summary>
	/// Header list changed action.
	/// </summary>
    /// <remarks>
    /// Specifies the way that a <see cref="HeaderList"/> was changed.
    /// </remarks>
	public enum HeaderListChangedAction {
		/// <summary>
		/// A header was added.
		/// </summary>
		Added,

		/// <summary>
		/// A header was changed.
		/// </summary>
		Changed,

		/// <summary>
		/// A header was removed.
		/// </summary>
		Removed,

		/// <summary>
		/// The header list was cleared.
		/// </summary>
		Cleared
	}

	class HeaderListChangedEventArgs : EventArgs
	{
		internal HeaderListChangedEventArgs (Header header, HeaderListChangedAction action)
		{
			Header = header;
			Action = action;
		}

		public HeaderListChangedAction Action {
			get; private set;
		}

		public Header Header {
			get; private set;
		}
	}
}

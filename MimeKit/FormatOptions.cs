//
// FormatOptions.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Jeffrey Stedfast
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
	public sealed class FormatOptions
	{
		/// <summary>
		/// The default formatting options.
		/// </summary>
		public static readonly FormatOptions Default;

		/// <summary>
		/// Gets or sets the maximum line length used by the encoders. The encoders
		/// use this value to determine where to place line breaks.
		/// </summary>
		/// <value>The maximum line length.</value>
		public int MaxLineLength {
			get; private set;
		}

		internal int MaxPreEncodedLength {
			get { return MaxLineLength / 2; }
		}

		static FormatOptions ()
		{
			Default = new FormatOptions ();
			Default.MaxLineLength = 72;
		}

		/// <summary>
		/// Clones an instance of <see cref="MimeKit.FormatOptions"/>.
		/// </summary>
		public FormatOptions Clone ()
		{
			var options = new FormatOptions ();
			options.MaxLineLength = MaxLineLength;
			return options;
		}
	}
}

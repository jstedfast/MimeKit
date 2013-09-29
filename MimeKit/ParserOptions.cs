//
// ParserOptions.cs
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
using System.Text;

namespace MimeKit {
	/// <summary>
	/// Parser options as used by <see cref="MimeParser"/> as well as various Parse and TryParse methods in MimeKit.
	/// </summary>
	public sealed class ParserOptions
	{
		/// <summary>
		/// The default parser options.
		/// </summary>
		public static readonly ParserOptions Default;

		/// <summary>
		/// Gets or sets a value indicating whether rfc2047 workarounds should be used.
		/// </summary>
		/// <value><c>true</c> if rfc2047 workarounds are enabled; otherwise, <c>false</c>.</value>
		public bool EnableRfc2047Workarounds { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the Content-Length value should be
		/// respected when parsing mbox streams.
		/// </summary>
		/// <value><c>true</c> if the Content-Length value should be respected;
		/// otherwise, <c>false</c>.</value>
		/// <remarks>
		/// For more information about why this may be useful, you can find more information
		/// at http://www.jwz.org/doc/content-length.html
		/// </remarks>
		public bool RespectContentLength { get; set; }

		/// <summary>
		/// Gets or sets the charset encoding to use as a fallback for 8bit headers.
		/// </summary>
		/// <remarks>
		/// <see cref="MimeKit.Utils.Rfc2047.DecodeText(ParserOptions, byte[])"/> and
		/// <see cref="MimeKit.Utils.Rfc2047.DecodePhrase(ParserOptions, byte[])"/>
		/// use this charset encoding as a fallback when decoding 8bit text into unicode. The first
		/// charset encoding attempted is UTF-8, followed by this charset encoding, before finally
		/// falling back to iso-8859-1.
		/// </remarks>
		/// <value>The charset encoding.</value>
		public Encoding CharsetEncoding { get; set; }

		static ParserOptions ()
		{
			Default = new ParserOptions ();
			Default.EnableRfc2047Workarounds = true;
			Default.RespectContentLength = false;
			Default.CharsetEncoding = Encoding.Default;
		}

		/// <summary>
		/// Clones an instance of <see cref="MimeKit.ParserOptions"/>.
		/// </summary>
		public ParserOptions Clone ()
		{
			var options = new ParserOptions ();
			options.EnableRfc2047Workarounds = EnableRfc2047Workarounds;
			options.RespectContentLength = RespectContentLength;
			options.CharsetEncoding = CharsetEncoding;

			return options;
		}
	}
}

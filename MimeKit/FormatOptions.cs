//
// FormatOptions.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2014 Xamarin Inc. (www.xamarin.com)
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
using System.Collections.Generic;

using MimeKit.IO.Filters;

namespace MimeKit {
	/// <summary>
	/// A New-Line format.
	/// </summary>
	/// <remarks>
    /// There are two commonly used line-endings used by modern Operating Systems.
    /// Unix-based systems such as Linux and Mac OS use a single character (<c>'\n'</c> aka LF)
    /// to represent the end of line where-as Windows (or DOS) uses a sequence of two
    /// characters (<c>"\r\n"</c> aka CRLF). Most text-based network protocols such as SMTP,
    /// POP3, and IMAP use the CRLF sequence as well.
    /// </remarks>
	public enum NewLineFormat {
		/// <summary>
		/// The Unix New-Line format (<c>"\n"</c>).
		/// </summary>
		Unix,

		/// <summary>
		/// The DOS New-Line format (<c>"\r\n"</c>).
		/// </summary>
		Dos,
	}

	/// <summary>
	/// Format options for serializing various MimeKit objects.
	/// </summary>
	/// <remarks>
	/// Represents the available options for formatting MIME messages
	/// and entities when writing them to a stream.
	/// </remarks>
	public class FormatOptions
	{
		static readonly byte[][] NewLineFormats = new byte[][] {
			new byte[] { 0x0A }, new byte[] { 0x0D, 0x0A }
		};

		/// <summary>
		/// The default formatting options.
		/// </summary>
		/// <remarks>
		/// If a custom <see cref="FormatOptions"/> is not passed to methods such as
		/// <see cref="MimeMessage.WriteTo(FormatOptions,System.IO.Stream)"/>, the default options
		/// will be used.
		/// </remarks>
		public static readonly FormatOptions Default;

		/// <summary>
		/// Gets or sets the maximum line length used by the encoders. The encoders
		/// use this value to determine where to place line breaks.
		/// </summary>
		/// <remarks>
		/// Specifies the maximum line length to use when line-wrapping headers.
		/// </remarks>
		/// <value>The maximum line length.</value>
		public int MaxLineLength {
			get; private set;
		}

		/// <summary>
		/// Gets or sets the new-line format.
		/// </summary>
		/// <remarks>
		/// Specifies the new-line encoding to use when writing the message
		/// or entity to a stream.
		/// </remarks>
		/// <value>The new-line format.</value>
		public NewLineFormat NewLineFormat {
			get; set;
		}

		internal IMimeFilter CreateNewLineFilter ()
		{
			switch (NewLineFormat) {
			case NewLineFormat.Unix:
				return new Dos2UnixFilter ();
			default:
				return new Unix2DosFilter ();
			}
		}

		internal string NewLine {
			get {
				if (NewLineFormat == NewLineFormat.Unix)
					return "\n";

				return "\r\n";
			}
		}

		internal byte[] NewLineBytes {
			get { return NewLineFormats[(int) NewLineFormat]; }
		}

		internal bool WriteHeaders {
			get; set;
		}

		/// <summary>
		/// Gets the message headers that should be hidden.
		/// </summary>
		/// <remarks>
		/// <para>Specifies the set of headers that should be removed when
		/// writing a <see cref="MimeMessage"/> to a stream.</para>
		/// <para>This is primarily meant for the purposes of removing Bcc
		/// and Resent-Bcc headers when sending via a transport such as
		/// SMTP.</para>
		/// </remarks>
		/// <value>The message headers.</value>
		public HashSet<HeaderId> HiddenHeaders {
			get; private set;
		}

		static FormatOptions ()
		{
			Default = new FormatOptions ();
			Default.MaxLineLength = 72;
			Default.WriteHeaders = true;

			if (Environment.NewLine.Length == 1)
				Default.NewLineFormat = NewLineFormat.Unix;
			else
				Default.NewLineFormat = NewLineFormat.Dos;

			Default.HiddenHeaders = new HashSet<HeaderId> ();
		}

		/// <summary>
		/// Clones an instance of <see cref="MimeKit.FormatOptions"/>.
		/// </summary>
		/// <remarks>
		/// Clones the formatting options.
		/// </remarks>
		public FormatOptions Clone ()
		{
			var options = new FormatOptions ();
			options.MaxLineLength = MaxLineLength;
			options.NewLineFormat = NewLineFormat;
			options.HiddenHeaders = new HashSet<HeaderId> (HiddenHeaders);
			options.WriteHeaders = true;
			return options;
		}
	}
}

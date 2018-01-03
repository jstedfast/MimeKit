//
// FormatOptions.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2018 Xamarin Inc. (www.xamarin.com)
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
	public enum NewLineFormat : byte {
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
		static readonly byte[][] NewLineFormats = {
			new byte[] { (byte) '\n' }, new byte[] { (byte) '\r', (byte) '\n' }
		};

		internal const int MaximumLineLength = 998;
		internal const int MinimumLineLength = 60;

		const int DefaultMaxLineLength = 78;

		ParameterEncodingMethod parameterEncodingMethod;
		bool allowMixedHeaderCharsets;
		NewLineFormat newLineFormat;
		bool international;

		/// <summary>
		/// The default formatting options.
		/// </summary>
		/// <remarks>
		/// If a custom <see cref="FormatOptions"/> is not passed to methods such as
		/// <see cref="MimeMessage.WriteTo(FormatOptions,System.IO.Stream,System.Threading.CancellationToken)"/>,
		/// the default options will be used.
		/// </remarks>
		public static readonly FormatOptions Default;

		/// <summary>
		/// Gets the maximum line length used by the encoders. The encoders
		/// use this value to determine where to place line breaks.
		/// </summary>
		/// <remarks>
		/// Specifies the maximum line length to use when line-wrapping headers.
		/// </remarks>
		/// <value>The maximum line length.</value>
		public int MaxLineLength {
			get { return DefaultMaxLineLength; }
		}

		/// <summary>
		/// Gets or sets the new-line format.
		/// </summary>
		/// <remarks>
		/// Specifies the new-line encoding to use when writing the message
		/// or entity to a stream.
		/// </remarks>
		/// <value>The new-line format.</value>
		/// <exception cref="System.InvalidOperationException">
		/// <see cref="Default"/> cannot be changed.
		/// </exception>
		public NewLineFormat NewLineFormat {
			get { return newLineFormat; }
			set {
				if (this == Default)
					throw new InvalidOperationException ("The default formatting options cannot be changed.");

				newLineFormat = value;
			}
		}

		internal IMimeFilter CreateNewLineFilter (bool ensureNewLine = false)
		{
			switch (NewLineFormat) {
			case NewLineFormat.Unix:
				return new Dos2UnixFilter (ensureNewLine);
			default:
				return new Unix2DosFilter (ensureNewLine);
			}
		}

		internal string NewLine {
			get { return NewLineFormat == NewLineFormat.Unix ? "\n" : "\r\n"; }
		}

		internal byte[] NewLineBytes {
			get { return NewLineFormats[(int) NewLineFormat]; }
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

		/// <summary>
		/// Gets or sets whether the new "Internationalized Email" formatting standards should be used.
		/// </summary>
		/// <remarks>
		/// <para>The new "Internationalized Email" format is defined by
		/// <a href="https://tools.ietf.org/html/rfc6530">rfc6530</a> and
		/// <a href="https://tools.ietf.org/html/rfc6532">rfc6532</a>.</para>
		/// <para>This feature should only be used when formatting messages meant to be sent via
		/// SMTP using the SMTPUTF8 extension (<a href="https://tools.ietf.org/html/rfc6531">rfc6531</a>)
		/// or when appending messages to an IMAP folder via UTF8 APPEND
		/// (<a href="https://tools.ietf.org/html/rfc6855">rfc6855</a>).</para>
		/// </remarks>
		/// <value><c>true</c> if the new internationalized formatting should be used; otherwise, <c>false</c>.</value>
		/// <exception cref="System.InvalidOperationException">
		/// <see cref="Default"/> cannot be changed.
		/// </exception>
		public bool International {
			get { return international; }
			set {
				if (this == Default)
					throw new InvalidOperationException ("The default formatting options cannot be changed.");

				international = value;
			}
		}

		/// <summary>
		/// Gets or sets whether the formatter should allow mixed charsets in the headers.
		/// </summary>
		/// <remarks>
		/// <para>When this option is enabled, the MIME formatter will try to use US-ASCII and/or
		/// ISO-8859-1 to encode headers when appropriate rather than being forced to use the
		/// specified charset for all encoded-word tokens in order to maximize readability.</para>
		/// <para>Unfortunately, mail clients like Outlook and Thunderbird do not treat
		/// encoded-word tokens individually and assume that all tokens are encoded using the
		/// charset declared in the first encoded-word token despite the specification
		/// explicitly stating that each encoded-word token should be treated independently.</para>
		/// <para>The Thunderbird bug can be tracked at
		/// <a href="https://bugzilla.mozilla.org/show_bug.cgi?id=317263">
		/// https://bugzilla.mozilla.org/show_bug.cgi?id=317263</a>.</para>
		/// </remarks>
		/// <value><c>true</c> if the formatter should be allowed to use ISO-8859-1 when encoding headers; otherwise, <c>false</c>.</value>
		public bool AllowMixedHeaderCharsets {
			get { return allowMixedHeaderCharsets; }
			set {
				if (this == Default)
					throw new InvalidOperationException ("The default formatting options cannot be changed.");

				allowMixedHeaderCharsets = value;
			}
		}

		/// <summary>
		/// The method to use for encoding Content-Type and Content-Disposition parameter values.
		/// </summary>
		/// <remarks>
		/// <para>The method to use for encoding Content-Type and Content-Disposition parameter
		/// values when the <see cref="Parameter.EncodingMethod"/> is set to
		/// <see cref="MimeKit.ParameterEncodingMethod.Default"/>.</para>
		/// <para>The MIME specifications specify that the proper method for encoding Content-Type
		/// and Content-Disposition parameter values is the method described in
		/// <a href="https://tools.ietf.org/html/rfc2231">rfc2231</a>. However, it is common for
		/// some older email clients to improperly encode using the method described in
		/// <a href="https://tools.ietf.org/html/rfc2047">rfc2047</a> instead.</para>
		/// </remarks>
		/// <value>The parameter encoding method that will be used.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is not a valid value.
		/// </exception>
		public ParameterEncodingMethod ParameterEncodingMethod {
			get { return parameterEncodingMethod; }
			set {
				if (this == Default)
					throw new InvalidOperationException ("The default formatting options cannot be changed.");

				switch (value) {
				case ParameterEncodingMethod.Rfc2047:
				case ParameterEncodingMethod.Rfc2231:
					parameterEncodingMethod = value;
					break;
				default:
					throw new ArgumentOutOfRangeException (nameof (value));
				}
			}
		}

		static FormatOptions ()
		{
			Default = new FormatOptions ();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.FormatOptions"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new set of formatting options for use with methods such as
		/// <see cref="MimeMessage.WriteTo(System.IO.Stream,System.Threading.CancellationToken)"/>.
		/// </remarks>
		public FormatOptions ()
		{
			HiddenHeaders = new HashSet<HeaderId> ();
			parameterEncodingMethod = ParameterEncodingMethod.Rfc2231;
			//maxLineLength = DefaultMaxLineLength;
			allowMixedHeaderCharsets = false;
			international = false;

			if (Environment.NewLine.Length == 1)
				newLineFormat = NewLineFormat.Unix;
			else
				newLineFormat = NewLineFormat.Dos;
		}

		/// <summary>
		/// Clones an instance of <see cref="MimeKit.FormatOptions"/>.
		/// </summary>
		/// <remarks>
		/// Clones the formatting options.
		/// </remarks>
		/// <returns>An exact copy of the <see cref="FormatOptions"/>.</returns>
		public FormatOptions Clone ()
		{
			var options = new FormatOptions ();
			//options.maxLineLength = maxLineLength;
			options.newLineFormat = newLineFormat;
			options.HiddenHeaders = new HashSet<HeaderId> (HiddenHeaders);
			options.allowMixedHeaderCharsets = allowMixedHeaderCharsets;
			options.parameterEncodingMethod = parameterEncodingMethod;
			options.international = international;
			return options;
		}

		/// <summary>
		/// Get the default formatting options in a thread-safe way.
		/// </summary>
		/// <remarks>
		/// Gets the default formatting options in a thread-safe way.
		/// </remarks>
		/// <returns>The default formatting options.</returns>
		internal static FormatOptions CloneDefault ()
		{
			lock (Default) {
				return Default.Clone ();
			}
		}
	}
}

//
// FormatOptions.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2023 .NET Foundation and Contributors
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
using System.Runtime.CompilerServices;

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

		/// <summary>
		/// A mixed New-Line format where some lines use Unix-based line endings and
		/// other lines use DOS-based line endings.
		/// </summary>
		Mixed,
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

		const int VerifyingSignatureOffset = 23;
		const int VerifyingSignatureMask = 0x01 << VerifyingSignatureOffset;
		const int ReformatHeadersOffset = 22;
		const int ReformatHeadersMask = 0x01 << ReformatHeadersOffset;
		const int ReformatContentOffset = 21;
		const int ReformatContentMask = 0x01 << ReformatContentOffset;
		const int EnsureNewLineOffset = 20;
		const int EnsureNewLineMask = 0x01 << EnsureNewLineOffset;
		const int NewLineFormatOffset = 18;
		const int NewLineFormatMask = 0x03 << NewLineFormatOffset;

		// The following offsets/options are used for header formatting and so need to stay together.
		const int ParameterEncodingMethodOffset = 13;
		const int ParameterEncodingMethodMask = 0x03 << ParameterEncodingMethodOffset;
		const int AlwaysQuoteParameterValuesOffset = 12;
		const int AlwaysQuoteParameterValuesMask = 0x01 << AlwaysQuoteParameterValuesOffset;
		const int AllowMixedHeaderCharsetsOffset = 11;
		const int AllowMixedHeaderCharsetsMask = 0x01 << AllowMixedHeaderCharsetsOffset;
		const int InternationalOffset = 10;
		const int InternationalMask = 0x01 << InternationalOffset;
		const int MaxLineLengthOffset = 0;
		const int MaxLineLengthMask = 0x03FF;

		const int EncodedHeaderOptionsMask = 0x07FF;

		int encodedOptions;

		/// <summary>
		/// The default formatting options for verifying signatures.
		/// </summary>
		/// <remarks>
		/// If a custom <see cref="FormatOptions"/> is not passed to methods such as
		/// <see cref="MimeKit.Cryptography.DkimVerifier.Verify(FormatOptions,MimeMessage,Header,System.Threading.CancellationToken)"/>,
		/// the default options will be used.
		/// </remarks>
		internal static readonly FormatOptions VerifySignature;

		/// <summary>
		/// The default formatting options.
		/// </summary>
		/// <remarks>
		/// If a custom <see cref="FormatOptions"/> is not passed to methods such as
		/// <see cref="MimeMessage.WriteTo(FormatOptions,System.IO.Stream,System.Threading.CancellationToken)"/>,
		/// the default options will be used.
		/// </remarks>
		public static readonly FormatOptions Default;

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static bool GetValue (int encodedOptions, int mask)
		{
			return (encodedOptions & mask) != 0;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static void SetValue (ref int encodedOptions, int offset, bool value)
		{
			if (value)
				encodedOptions |= 1 << offset;
			else
				encodedOptions &= ~(1 << offset);
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static NewLineFormat GetNewLineFormat (int encodedOptions)
		{
			return (NewLineFormat) ((encodedOptions & NewLineFormatMask) >> NewLineFormatOffset);
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static void SetNewLineFormat (ref int encodedOptions, NewLineFormat value)
		{
			encodedOptions &= ~NewLineFormatMask;
			encodedOptions |= ((int) value) << NewLineFormatOffset;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static ParameterEncodingMethod GetParameterEncodingMethod (int encodedOptions)
		{
			return (ParameterEncodingMethod) ((encodedOptions & ParameterEncodingMethodMask) >> ParameterEncodingMethodOffset);
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static void SetParameterEncodingMethod (ref int encodedOptions, ParameterEncodingMethod value)
		{
			encodedOptions &= ~ParameterEncodingMethodMask;
			encodedOptions |= ((int) value) << ParameterEncodingMethodOffset;
		}

		/// <summary>
		/// Get an encoded representation of the formatting options relevant to header formatting.
		/// </summary>
		internal int EncodedHeaderOptions {
			get { return encodedOptions & EncodedHeaderOptionsMask; }
		}

		/// <summary>
		/// Get or set the maximum line length used by the encoders. The encoders
		/// use this value to determine where to place line breaks.
		/// </summary>
		/// <remarks>
		/// Specifies the maximum line length to use when line-wrapping headers.
		/// </remarks>
		/// <value>The maximum line length.</value>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="value"/> is out of range. It must be between 60 and 998.
		/// </exception>
		public int MaxLineLength {
			get { return encodedOptions & MaxLineLengthMask; }
			set {
				if (value < MinimumLineLength || value > MaximumLineLength)
					throw new ArgumentOutOfRangeException (nameof (value));

				encodedOptions &= ~MaxLineLengthMask;
				encodedOptions |= value & MaxLineLengthMask;
			}
		}

		/// <summary>
		/// Get or set the new-line format.
		/// </summary>
		/// <remarks>
		/// Specifies the new-line encoding to use when writing the message
		/// or entity to a stream.
		/// </remarks>
		/// <value>The new-line format.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"> is not a valid <see cref="NewLineFormat"/>.</paramref>
		/// </exception>
		public NewLineFormat NewLineFormat {
			get { return GetNewLineFormat (encodedOptions); }
			set {
				switch (value) {
				case NewLineFormat.Unix:
				case NewLineFormat.Dos:
					SetNewLineFormat (ref encodedOptions, value);
					break;
				default:
					throw new ArgumentOutOfRangeException (nameof (value));
				}
			}
		}

		/// <summary>
		/// Get or set whether the formatter should ensure that messages end with a new-line sequence.
		/// </summary>
		/// <remarks>
		/// <para>By default, when writing a <see cref="MimeMessage"/> to a stream, the serializer attempts to
		/// maintain byte-for-byte compatibility with the original stream that the message was parsed from.
		/// This means that if the ogirinal message stream did not end with a new-line sequence, then the
		/// output of writing the message back to a stream will also not end with a new-line sequence.</para>
		/// <para>To override this behavior, you can set this property to <c>true</c> in order to ensure
		/// that writing the message back to a stream will always end with a new-line sequence.</para>
		/// </remarks>
		/// <value><c>true</c> in order to ensure that the message will end with a new-line sequence; otherwise, <c>false</c>.</value>
		public bool EnsureNewLine {
			get { return GetValue (encodedOptions, EnsureNewLineMask); }
			set { SetValue (ref encodedOptions, EnsureNewLineOffset, value); }
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

		internal bool VerifyingSignature {
			get { return GetValue (encodedOptions, VerifyingSignatureMask); }
			set { SetValue (ref encodedOptions, VerifyingSignatureOffset, value); }
		}

		/// <summary>
		/// Get the message headers that should be hidden.
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
		/// Get or set whether the new "Internationalized Email" formatting standards should be used.
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
		public bool International {
			get { return GetValue (encodedOptions, InternationalMask); }
			set { SetValue (ref encodedOptions, InternationalOffset, value); }
		}

		/// <summary>
		/// Get or set whether the formatter should allow mixed charsets in the headers.
		/// </summary>
		/// <remarks>
		/// <para>When this option is enabled, the MIME formatter will try to use us-ascii and/or
		/// iso-8859-1 to encode headers when appropriate rather than being forced to use the
		/// specified charset for all encoded-word tokens in order to maximize readability.</para>
		/// <para>Unfortunately, mail clients like Outlook and Thunderbird do not treat
		/// encoded-word tokens individually and assume that all tokens are encoded using the
		/// charset declared in the first encoded-word token despite the specification
		/// explicitly stating that each encoded-word token should be treated independently.</para>
		/// <para>The Thunderbird bug can be tracked at
		/// <a href="https://bugzilla.mozilla.org/show_bug.cgi?id=317263">
		/// https://bugzilla.mozilla.org/show_bug.cgi?id=317263</a>.</para>
		/// </remarks>
		/// <value><c>true</c> if the formatter should be allowed to use us-ascii and/or iso-8859-1 when encoding headers; otherwise, <c>false</c>.</value>
		public bool AllowMixedHeaderCharsets {
			get { return GetValue (encodedOptions, AllowMixedHeaderCharsetsMask); }
			set { SetValue (ref encodedOptions, AllowMixedHeaderCharsetsOffset, value); }
		}

		/// <summary>
		/// Get or set the method to use for encoding Content-Type and Content-Disposition parameter values.
		/// </summary>
		/// <remarks>
		/// <para>The method to use for encoding Content-Type and Content-Disposition parameter
		/// values when the <see cref="Parameter.EncodingMethod"/> is set to
		/// <see cref="ParameterEncodingMethod.Default"/>.</para>
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
			get { return GetParameterEncodingMethod (encodedOptions); }
			set {
				switch (value) {
				case ParameterEncodingMethod.Rfc2047:
				case ParameterEncodingMethod.Rfc2231:
					SetParameterEncodingMethod (ref encodedOptions, value);
					break;
				default:
					throw new ArgumentOutOfRangeException (nameof (value));
				}
			}
		}

		/// <summary>
		/// Get or set whether Content-Type and Content-Disposition parameter values should always be quoted even when they don't need to be.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets whether Content-Type and Content-Disposition parameter values should always be quoted even when they don't need to be.</para>
		/// <para>Technically, Content-Type and Content-Disposition parameter values only require quoting when they contain characters
		/// that have special meaning to a MIME parser. However, for compatibility with email processing solutions that do not properly
		/// adhere to the MIME specifications, this property can be used to force MimeKit to quote parameter values that would normally
		/// not require quoting.</para>
		/// </remarks>
		/// <value><c>true</c> if Content-Type and Content-Disposition parameters should always be quoted; otherwise, <c>false</c>.</value>
		public bool AlwaysQuoteParameterValues {
			get { return GetValue (encodedOptions, AlwaysQuoteParameterValuesMask); }
			set { SetValue (ref encodedOptions, AlwaysQuoteParameterValuesOffset, value); }
		}

		/// <summary>
		/// Get or set whether headers should be reformatted when writing back out to a stream.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets whether headers should be reformatted when writing back out to a stream.</para>
		/// <para>If <see cref="ReformatHeaders"/> is set to <c>true</c>, then all headers (except those set using
		/// <see cref="Header.SetRawValue(byte[])"/>) will be reformatted.</para>
		/// <para>If <see cref="ReformatHeaders"/> is set to <c>false</c>, only headers that were not constructed by
		/// parser will be (re)formatted.</para>
		/// </remarks>
		/// <value><c>true</c> if headers should always be reformatted; otherwise, <c>false</c>.</value>
		public bool ReformatHeaders {
			get { return GetValue (encodedOptions, ReformatHeadersMask); }
			set { SetValue (ref encodedOptions, ReformatHeadersOffset, value); }
		}

		static FormatOptions ()
		{
			VerifySignature = new FormatOptions {
				NewLineFormat = NewLineFormat.Dos,
				VerifyingSignature = true
			};
			Default = new FormatOptions ();
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="FormatOptions"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new set of formatting options for use with methods such as
		/// <see cref="MimeMessage.WriteTo(System.IO.Stream,System.Threading.CancellationToken)"/>.
		/// </remarks>
		public FormatOptions ()
		{
			HiddenHeaders = new HashSet<HeaderId> ();
			ParameterEncodingMethod = ParameterEncodingMethod.Rfc2231;
			MaxLineLength = DefaultMaxLineLength;

			if (Environment.NewLine.Length == 1)
				NewLineFormat = NewLineFormat.Unix;
			else
				NewLineFormat = NewLineFormat.Dos;
		}

		/// <summary>
		/// Clone an instance of <see cref="FormatOptions"/>.
		/// </summary>
		/// <remarks>
		/// Clones the formatting options.
		/// </remarks>
		/// <returns>An exact copy of the <see cref="FormatOptions"/>.</returns>
		public FormatOptions Clone ()
		{
			return new FormatOptions {
				HiddenHeaders = new HashSet<HeaderId> (HiddenHeaders),
				encodedOptions = encodedOptions
			};
		}
	}
}

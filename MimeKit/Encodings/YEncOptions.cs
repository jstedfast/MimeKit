//
// YEncOptions.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
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

using MimeKit.Utils;

namespace MimeKit.Encodings {
	/// <summary>
	/// Options for reading and writing yEnc blocks.
	/// </summary>
	/// <remarks>
	/// Readers and writers take a snapshot of these options when constructed.
	/// </remarks>
	public sealed class YEncOptions
	{
		Encoding fileNameEncoding = CharsetUtils.Latin1;
		NewLineFormat newLineFormat = NewLineFormat.Dos;
		long maxDecodedSize = 1024L * 1024 * 1024;
		int maxLineLength = 16 * 1024;

		/// <summary>
		/// Initialize a new set of yEnc options.
		/// </summary>
		/// <remarks>
		/// Uses CRLF output, Latin-1 filenames, a 16 KB line limit and a 1 GB decoded block limit.
		/// </remarks>
		public YEncOptions ()
		{
		}

		/// <summary>
		/// Get or set the maximum encoded line length.
		/// </summary>
		/// <remarks>
		/// Applies to control lines, payload lines and scanned text, excluding the newline.
		/// The default is 16384.
		/// </remarks>
		/// <value>The maximum line length in bytes.</value>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The value is less than 1 or greater than <c>int.MaxValue - 2</c>.
		/// </exception>
		public int MaxLineLength {
			get { return maxLineLength; }
			set {
				if (value < 1 || value > int.MaxValue - 2)
					throw new ArgumentOutOfRangeException (nameof (value));

				maxLineLength = value;
			}
		}

		/// <summary>
		/// Get or set the maximum decoded size of an individual block.
		/// </summary>
		/// <remarks>
		/// For multipart files this limits each part, not the total file size.
		/// The default is 1 GB.
		/// </remarks>
		/// <value>The maximum decoded size in bytes.</value>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The value is less than zero.
		/// </exception>
		public long MaxDecodedSize {
			get { return maxDecodedSize; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException (nameof (value));

				maxDecodedSize = value;
			}
		}

		/// <summary>
		/// Get or set the filename encoding.
		/// </summary>
		/// <remarks>
		/// The format does not declare a charset. Latin-1 is the default, preserving every filename byte.
		/// Both reading and writing use exception fallbacks rather than silently replacing characters.
		/// </remarks>
		/// <value>The filename encoding.</value>
		/// <exception cref="ArgumentNullException">
		/// The value is <see langword="null"/>.
		/// </exception>
		public Encoding FileNameEncoding {
			get { return fileNameEncoding; }
			set {
				if (value is null)
					throw new ArgumentNullException (nameof (value));

				fileNameEncoding = value;
			}
		}

		/// <summary>
		/// Get or set the writer's newline format.
		/// </summary>
		/// <remarks>
		/// Defaults to CRLF. Readers accept both CRLF and LF independently of this setting.
		/// </remarks>
		/// <value>The newline format.</value>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The value is neither DOS nor Unix.
		/// </exception>
		public NewLineFormat NewLineFormat {
			get { return newLineFormat; }
			set {
				if (value != NewLineFormat.Dos && value != NewLineFormat.Unix)
					throw new ArgumentOutOfRangeException (nameof (value));

				newLineFormat = value;
			}
		}

		internal YEncOptions Snapshot ()
		{
			var encoding = (Encoding) FileNameEncoding.Clone ();
			encoding.EncoderFallback = EncoderFallback.ExceptionFallback;
			encoding.DecoderFallback = DecoderFallback.ExceptionFallback;

			return new YEncOptions {
				MaxLineLength = MaxLineLength,
				MaxDecodedSize = MaxDecodedSize,
				NewLineFormat = NewLineFormat,
				FileNameEncoding = encoding
			};
		}
	}
}

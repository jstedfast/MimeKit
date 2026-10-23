//
// YEncHeader.cs
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

namespace MimeKit.Encodings {
	/// <summary>
	/// The immutable metadata for a yEnc block.
	/// </summary>
	/// <remarks>
	/// Multipart byte ranges are one-based and inclusive.
	/// Metadata does not imply that content has been validated.
	/// </remarks>
	public sealed class YEncHeader
	{
		/// <summary>
		/// Initialize the metadata for a single-part or multipart yEnc block.
		/// </summary>
		/// <remarks>
		/// For a single-part file, leave all part and range arguments at zero.
		/// </remarks>
		/// <param name="fileName">The filename, treated as metadata, never as a filesystem path.</param>
		/// <param name="size">The entire file's decoded size.</param>
		/// <param name="lineLength">The nominal payload line length. Writers support 60 through 998.</param>
		/// <param name="partNumber">The part number, or zero for a single-part file.</param>
		/// <param name="totalParts">The total part count, or zero if unspecified.</param>
		/// <param name="begin">The one-based, inclusive start of a multipart range.</param>
		/// <param name="end">The one-based, inclusive end of a multipart range.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="fileName"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The filename is empty, has leading or trailing spaces, or contains NUL, CR or LF.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// A size, line length, part number or range is invalid or inconsistent.
		/// </exception>
		public YEncHeader (string fileName, long size, int lineLength = 128, int partNumber = 0, int totalParts = 0, long begin = 0, long end = 0)
		{
			if (fileName is null)
				throw new ArgumentNullException (nameof (fileName));

			if (fileName.Length == 0 || fileName[0] == ' ' || fileName[fileName.Length - 1] == ' ' || fileName.IndexOfAny (new[] { '\0', '\r', '\n' }) != -1)
				throw new ArgumentException ("The filename must be nonempty and must not contain NUL, CR, LF or surrounding spaces.", nameof (fileName));

			if (size < 0)
				throw new ArgumentOutOfRangeException (nameof (size));

			if (lineLength < 1 || lineLength > int.MaxValue - 2)
				throw new ArgumentOutOfRangeException (nameof (lineLength));

			if (partNumber < 0)
				throw new ArgumentOutOfRangeException (nameof (partNumber));

			if (totalParts < 0 || (partNumber == 0 && totalParts != 0) || (totalParts != 0 && partNumber > totalParts))
				throw new ArgumentOutOfRangeException (nameof (totalParts));

			if (partNumber == 0 ? begin != 0 : begin < 1 || begin > size)
				throw new ArgumentOutOfRangeException (nameof (begin));

			if (partNumber == 0 ? end != 0 : end < begin || end > size)
				throw new ArgumentOutOfRangeException (nameof (end));

			FileName = fileName;
			Size = size;
			LineLength = lineLength;
			PartNumber = partNumber;
			TotalParts = totalParts;
			Begin = begin;
			End = end;
		}

		/// <summary>
		/// Get the filename.
		/// </summary>
		/// <remarks>
		/// Untrusted metadata; callers must not use it directly as a filesystem path.
		/// </remarks>
		/// <value>The filename.</value>
		public string FileName { get; }

		/// <summary>
		/// Get the total file size.
		/// </summary>
		/// <remarks>
		/// For multipart files this is not the size of the current part.
		/// </remarks>
		/// <value>The size in bytes.</value>
		public long Size { get; }

		/// <summary>
		/// Get the nominal payload line length.
		/// </summary>
		/// <remarks>
		/// A final escape pair may extend a line by one byte.
		/// </remarks>
		/// <value>The line length, excluding the newline.</value>
		public int LineLength { get; }

		/// <summary>
		/// Get the part number.
		/// </summary>
		/// <remarks>
		/// Zero identifies a single-part file.
		/// </remarks>
		/// <value>The part number.</value>
		public int PartNumber { get; }

		/// <summary>
		/// Get the total part count.
		/// </summary>
		/// <remarks>
		/// Zero indicates that no total was specified.
		/// </remarks>
		/// <value>The total number of parts.</value>
		public int TotalParts { get; }

		/// <summary>
		/// Get the beginning of the part's byte range.
		/// </summary>
		/// <remarks>
		/// One-based and inclusive; zero for single-part files.
		/// </remarks>
		/// <value>The starting byte position.</value>
		public long Begin { get; }

		/// <summary>
		/// Get the end of the part's byte range.
		/// </summary>
		/// <remarks>
		/// One-based and inclusive; zero for single-part files.
		/// </remarks>
		/// <value>The ending byte position.</value>
		public long End { get; }

		/// <summary>
		/// Get the decoded size of this block.
		/// </summary>
		/// <remarks>
		/// Derived from the byte range for multipart files.
		/// </remarks>
		/// <value>The block size in bytes.</value>
		public long DecodedSize {
			get { return PartNumber == 0 ? Size : End - Begin + 1; }
		}
	}
}

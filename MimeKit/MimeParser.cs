//
// MimeParser.cs
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
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace MimeKit {
	enum BoundaryType
	{
		None,
		Eos,
		Boundary,
		EndBoundary
	}

	class Boundary
	{
		public static readonly byte[] MboxFrom = Encoding.ASCII.GetBytes ("From ");

		public byte[] Marker { get; private set; }
		public int FinalLength { get { return Marker.Length; } }
		public int Length { get; private set; }
		public int MaxLength { get; private set; }
		public long ContentEnd { get; set; }

		public Boundary (string boundary, int currentMaxLength)
		{
			Marker = Encoding.UTF8.GetBytes ("--" + boundary + "--");
			Length = Marker.Length - 2;

			MaxLength = Math.Max (currentMaxLength, Marker.Length);
			ContentEnd = -1;
		}

		Boundary ()
		{
		}

		public static Boundary CreateMboxBoundary ()
		{
			var boundary = new Boundary ();
			boundary.Marker = MboxFrom;
			boundary.ContentEnd = -1;
			boundary.MaxLength = 5;
			boundary.Length = 5;
			return boundary;
		}

		public override string ToString ()
		{
			return Encoding.UTF8.GetString (Marker);
		}
	}

	enum MimeParserState
	{
		Error = -1,
		Initialized,
		FromLine,
		MessageHeaders,
		Headers,
		Content,
		Complete,
		Eos
	}

	public class MimeParser
	{
		static readonly StringComparer icase = StringComparer.OrdinalIgnoreCase;
		const int ReadAheadSize = 128;
		const int BlockSize = 4096;

		// I/O buffering
		readonly byte[] input = new byte[ReadAheadSize + BlockSize + 1];
		readonly int inputStart = ReadAheadSize;
		int inputIndex = ReadAheadSize;
		int inputEnd = ReadAheadSize;

		// mbox From-line state
		byte[] mboxMarkerBuffer;
		long mboxMarkerOffset;
		int mboxMarkerLength;

		// header buffer
		byte[] headerBuffer = new byte[512];
		long headerOffset;
		int headerIndex;

		readonly List<Boundary> bounds;
		readonly List<Header> headers;

		MimeParserState state;
		ParserOptions options;
		MimeFormat format;
		Stream stream;
		long offset;

		// other state
		bool midline;

		public MimeParser (Stream stream, MimeFormat format) : this (ParserOptions.Default, stream, format)
		{
		}

		public MimeParser (Stream stream) : this (ParserOptions.Default, stream, MimeFormat.Default)
		{
		}

		public MimeParser (ParserOptions options, Stream stream) : this (options, stream, MimeFormat.Default)
		{
		}

		public MimeParser (ParserOptions options, Stream stream, MimeFormat format)
		{
			bounds = new List<Boundary> ();
			headers = new List<Header> ();

			SetStream (options, stream, format);
		}

		/// <summary>
		/// Gets a value indicating whether the parser has reached the end of the input stream.
		/// </summary>
		/// <value><c>true</c> if this parser has reached the end of the input stream;
		/// otherwise, <c>false</c>.</value>
		public bool IsEndOfStream {
			get { return state == MimeParserState.Eos; }
		}

		/// <summary>
		/// Gets the current stream offset that the parser is at.
		/// </summary>
		/// <value>The stream offset.</value>
		public long CurrentStreamOffset {
			get { return GetOffset (-1); }
		}

		public long MboxMarkerOffset {
			get { return mboxMarkerOffset; }
		}

		public void SetStream (ParserOptions options, Stream stream, MimeFormat format)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (stream == null)
				throw new ArgumentNullException ("stream");

			this.options = options.Clone ();
			this.format = format;
			this.stream = stream;

			inputIndex = inputStart;
			inputEnd = inputStart;

			mboxMarkerOffset = 0;
			mboxMarkerLength = 0;

			headers.Clear ();
			headerOffset = 0;
			headerIndex = 0;
			offset = 0;

			bounds.Clear ();
			if (format == MimeFormat.Mbox) {
				bounds.Add (Boundary.CreateMboxBoundary ());
				mboxMarkerBuffer = new byte[ReadAheadSize];
				state = MimeParserState.FromLine;
			} else {
				state = MimeParserState.Initialized;
			}
		}

		public void SetStream (ParserOptions options, Stream stream)
		{
			SetStream (options, stream, MimeFormat.Default);
		}

		public void SetStream (Stream stream, MimeFormat format)
		{
			SetStream (ParserOptions.Default, stream, format);
		}

		public void SetStream (Stream stream)
		{
			SetStream (ParserOptions.Default, stream, MimeFormat.Default);
		}

		static unsafe void MemMove (byte[] buffer, int sourceIndex, int destIndex, int length)
		{
			fixed (byte* buf = buffer) {
				if (sourceIndex + length > destIndex) {
					byte* src = buf + sourceIndex + length - 1;
					byte *dest = buf + destIndex + length - 1;
					byte *start = buf + sourceIndex;

					while (src >= start)
						*dest-- = *src--;
				} else {
					byte* src = buf + sourceIndex;
					byte* dest = buf + destIndex;
					byte* end = src + length;

					while (src < end)
						*dest++ = *src++;
				}
			}
		}

#if DEBUG
		static string ConvertToCString (byte[] buffer, int startIndex, int length)
		{
			var cstr = new StringBuilder ();
			cstr.AppendCString (buffer, startIndex, length);
			return cstr.ToString ();
		}
#endif

		static int NextAllocSize (int need)
		{
			return (need + 63) & ~63;
		}

		int ReadAhead (int atleast)
		{
			int left = inputEnd - inputIndex;

			if (left >= atleast)
				return left;

			int index = inputIndex;
			int start = inputStart;
			int end = inputEnd;

			// attempt to align the end of the remaining input with BackBufferSize
			if (index >= start) {
				start -= left < ReadAheadSize ? left : ReadAheadSize;
				MemMove (input, index, start, left);
				index = start;
				start += left;
			} else if (index > 0) {
				int shift = Math.Min (index, end - start);
				MemMove (input, index, index - shift, left);
				index -= shift;
				start = index + left;
			} else {
				// we can't shift...
				start = end;
			}

			inputIndex = index;
			inputEnd = start;

			end = input.Length;

			int nread = stream.Read (input, start, end - start);
			if (nread > 0) {
				inputEnd += nread;
				offset += nread;
			}

			return inputEnd - inputIndex;
		}

		long GetOffset (int index)
		{
			if (offset == -1)
				return -1;

			if (index == -1)
				index = inputIndex;

			return offset - (inputEnd - index);
		}

		static unsafe bool CStringsEqual (byte* str1, byte* str2, int length)
		{
			byte* se = str1 + length;
			byte* s1 = str1;
			byte* s2 = str2;

			while (s1 < se) {
				if (*s1 != *s2)
					return false;

				s1++;
				s2++;
			}

			return true;
		}

		static unsafe bool IsMboxMarker (byte* text)
		{
			fixed (byte* mbox = Boundary.MboxFrom) {
				return CStringsEqual (text, mbox, 5);
			}
		}

		unsafe int StepMboxMarker ()
		{
			bool complete = false;
			bool needInput;
			int left = 0;

			mboxMarkerLength = 0;

			do {
				if (ReadAhead (Math.Max (ReadAheadSize, left)) <= left) {
					// failed to find a From line; EOF reached
					state = MimeParserState.Error;
					inputIndex = inputEnd;
					return -1;
				}

				needInput = false;

				fixed (byte* inbuf = input) {
					byte* inptr = inbuf + inputIndex;
					byte* inend = inbuf + inputEnd;

					*inend = (byte) '\n';

					while (inptr < inend) {
						byte* start = inptr;

						// scan for the end of the line
						while (*inptr != (byte) '\n')
							inptr++;

						long length = inptr - start;

						// consume the '\n'
						inptr++;

						if (inptr >= inend) {
							// we don't have enough input data
							inputIndex = (int) (start - inbuf);
							left = (int) length;
							needInput = true;
							break;
						}

						if (length >= 5 && IsMboxMarker (start)) {
							long startIndex = start - inbuf;

							mboxMarkerOffset = GetOffset ((int) startIndex);
							mboxMarkerLength = (int) length;

							if (mboxMarkerBuffer.Length < mboxMarkerLength)
								Array.Resize (ref mboxMarkerBuffer, mboxMarkerLength);

							Array.Copy (input, startIndex, mboxMarkerBuffer, 0, length);
							complete = true;
							break;
						}
					}

					if (!needInput) {
						inputIndex = (int) (inptr - inbuf);
						left = 0;
					}
				}
			} while (!complete);

			state = MimeParserState.MessageHeaders;

			return 0;
		}

		void AppendRawHeaderData (int startIndex, int length)
		{
			int left = headerBuffer.Length - headerIndex;

			if (left < length)
				Array.Resize (ref headerBuffer, NextAllocSize (headerIndex + length));

			Array.Copy (input, startIndex, headerBuffer, headerIndex, length);
			headerIndex += length;
		}

		void ResetRawHeaderData ()
		{
			headerIndex = 0;
		}

		unsafe void ParseAndAppendHeader ()
		{
			if (headerIndex == 0)
				return;

			fixed (byte* buf = headerBuffer) {
				Header header;

				if (!Header.TryParse (options, buf, headerIndex, false, out header)) {
#if DEBUG
					Debug.WriteLine ("Invalid header at offset {0}: {1}", headerOffset, ConvertToCString (headerBuffer, 0, headerIndex));
#endif
					headerIndex = 0;
					return;
				}

				header.Offset = headerOffset;
				headers.Add (header);
				headerIndex = 0;
			}
		}

		static bool IsBlankOrControl (byte c)
		{
			return c.IsType (CharType.IsBlank | CharType.IsControl);
		}

		static unsafe bool IsEoln (byte *text)
		{
			if (*text == (byte) '\r')
				text++;

			return *text == (byte) '\n';
		}

		unsafe int StepHeaders ()
		{
			bool scanningFieldName = true;
			bool checkFolded = false;
			bool needInput = false;
			bool valid = true;
			int left = 0;
			long length;
			bool eoln;

			ResetRawHeaderData ();
			headers.Clear ();
			midline = false;

			do {
				if (ReadAhead (Math.Max (ReadAheadSize, left)) <= left) {
					// failed to find a From line; EOF reached
					state = MimeParserState.Error;
					inputIndex = inputEnd;
					return -1;
				}

				needInput = false;

				fixed (byte* inbuf = input) {
					byte* inptr = inbuf + inputIndex;
					byte* inend = inbuf + inputEnd;

					*inend = (byte) '\n';

					while (inptr < inend) {
						byte* start = inptr;

						// if we are scanning a new line, check for a folded header
						if (!midline && checkFolded && !(*inptr).IsBlank ()) {
							ParseAndAppendHeader ();

							headerOffset = GetOffset ((int) (inptr - inbuf));
							scanningFieldName = true;
							checkFolded = false;
							valid = true;
						}

						eoln = IsEoln (inptr);
						if (scanningFieldName && !eoln) {
							// scan and validate the field name
							if (*inptr != (byte) ':') {
								*inend = (byte) ':';

								while (*inptr != (byte) ':') {
									if (IsBlankOrControl (*inptr)) {
										valid = false;
										break;
									}

									inptr++;
								}

								if (inptr == inend) {
									// we don't have enough input data
									left = (int) (inend - start);
									inputIndex = (int) (start - inbuf);
									needInput = true;
									break;
								}

								*inend = (byte) '\n';
							} else {
								valid = false;
							}

							if (!valid) {
								length = inptr - start;

								if (format == MimeFormat.Mbox && length == 4 && IsMboxMarker (start)) {
									// we've found the start of the next message...
									inputIndex = (int) (start - inbuf);
									state = MimeParserState.Complete;
									headerIndex = 0;
									return 0;
								}

								if (state == MimeParserState.MessageHeaders && headers.Count == 0) {
									// ignore From-lines that might appear at the start of a message
									if (length != 4 || !IsMboxMarker (start)) {
										inputIndex = (int) (start - inbuf);
										state = MimeParserState.Error;
										headerIndex = 0;
										return -1;
									}
								}
							}
						}

						scanningFieldName = false;

						while (*inptr != (byte) '\n')
							inptr++;

						if (inptr == inend) {
							// we didn't manage to slurp up a full line, save what we have and refill our input buffer
							length = inptr - start;

							if (inptr > start) {
								// Note: if the last byte we got was a '\r', rewind a byte
								inptr--;
								if (*inptr == (byte) '\r')
									length--;
								else
									inptr++;
							}

							AppendRawHeaderData ((int) (start - inbuf), (int) length);
							inputIndex = (int) (inptr - inbuf);
							left = (int) (inend - inptr);
							needInput = true;
							midline = true;
							break;
						}

						// check to see if we've reached the end of the headers
						if (!midline && IsEoln (start)) {
							inputIndex = (int) (inptr - inbuf) + 1;
							state = MimeParserState.Content;
							ParseAndAppendHeader ();
							headerIndex = 0;
							return 0;
						}

						length = (inptr + 1) - start;

						AppendRawHeaderData ((int) (start - inbuf), (int) length);
						checkFolded = true;
						midline = false;
						inptr++;
					}

					if (!needInput) {
						inputIndex = (int) (inptr - inbuf);
						left = (int) (inend - inptr);
					}
				}
			} while (true);
		}

		unsafe bool SkipLine ()
		{
			do {
				fixed (byte* inbuf = input) {
					byte* inptr = inbuf + inputIndex;
					byte* inend = inbuf + inputEnd;

					*inend = (byte) '\n';

					while (*inptr != (byte) '\n')
						inptr++;

					if (inptr < inend) {
						inputIndex = (int) (inptr - inbuf) + 1;
						midline = false;
						return true;
					}
				}

				inputIndex = inputEnd;
				if (ReadAhead (ReadAheadSize) <= 0) {
					midline = true;
					return false;
				}
			} while (true);
		}

		MimeParserState Step ()
		{
			switch (state) {
			case MimeParserState.Error:
				break;
			case MimeParserState.Initialized:
				state = format == MimeFormat.Mbox ? MimeParserState.FromLine : MimeParserState.MessageHeaders;
				break;
			case MimeParserState.FromLine:
				StepMboxMarker ();
				break;
			case MimeParserState.MessageHeaders:
			case MimeParserState.Headers:
				StepHeaders ();
				break;
			case MimeParserState.Content:
				break;
			case MimeParserState.Complete:
				break;
			case MimeParserState.Eos:
				break;
			default:
				throw new ArgumentOutOfRangeException ();
			}

			return state;
		}

		ContentType GetContentType (ContentType parent)
		{
			ContentType type;

			foreach (var header in headers) {
				if (icase.Compare (header.Field, "Content-Type") != 0)
					continue;

				if (!ContentType.TryParse (options, header.RawValue, out type))
					return new ContentType ("application", "octet-stream");

				return type;
			}

			if (parent == null || !parent.Matches ("multipart", "digest"))
				return new ContentType ("text", "plain");

			return new ContentType ("message", "rfc822");
		}

		unsafe bool IsPossibleBoundary (byte* text, int length)
		{
			if (length < 2)
				return false;

			if (format == MimeFormat.Mbox && length >= 5 && IsMboxMarker (text))
				return true;

			if (*text == (byte) '-' && *(text + 1) == (byte) '-')
				return true;

			return false;
		}

		static unsafe bool IsBoundary (byte* text, int length, byte[] boundary, int boundaryLength)
		{
			if (boundaryLength > length)
				return false;

			fixed (byte* boundaryptr = boundary, from = Boundary.MboxFrom) {
				// make sure that the text matches the boundary
				if (!CStringsEqual (text, boundaryptr, boundaryLength))
					return false;

				// if this is an mbox marker, we're done
				if (CStringsEqual (text, from, 5))
					return true;

				// the boundary may optionally be followed by lwsp
				byte* inptr = text + boundaryLength;
				byte* inend = text + length;

				while (inptr < inend) {
					if (!(*inptr).IsWhitespace ())
						return false;

					inptr++;
				}
			}

			return true;
		}

		unsafe BoundaryType CheckBoundary (int startIndex, byte* start, int length)
		{
			if (!IsPossibleBoundary (start, length))
				return BoundaryType.None;

			long curOffset = GetOffset (startIndex);
			foreach (var boundary in bounds) {
				if (curOffset >= boundary.ContentEnd && IsBoundary (start, length, boundary.Marker, boundary.FinalLength))
					return BoundaryType.EndBoundary;

				if (IsBoundary (start, length, boundary.Marker, boundary.Length))
					return BoundaryType.Boundary;
			}

			return BoundaryType.None;
		}

		unsafe bool FoundImmediateBoundary (bool final)
		{
			int boundaryLength = final ? bounds[0].FinalLength : bounds[0].Length;

			fixed (byte* inbuf = input) {
				byte* start = inbuf + inputIndex;
				byte* inend = inbuf + inputEnd;
				byte *inptr = start;

				*inend = (byte) '\n';

				while (*inptr != (byte) '\n')
					inptr++;

				return IsBoundary (start, (int) (inptr - start), bounds[0].Marker, boundaryLength);
			}
		}

		int GetMaxBoundaryLength ()
		{
			return bounds.Count > 0 ? bounds[0].MaxLength + 2 : 0;
		}

		unsafe BoundaryType ScanContent (Stream content)
		{
			int atleast = Math.Min (ReadAheadSize, GetMaxBoundaryLength ());
			BoundaryType found = BoundaryType.None;
			int length, nleft;

			midline = false;

			do {
				nleft = inputEnd - inputIndex;
				if (ReadAhead (atleast) <= 0) {
					found = BoundaryType.Eos;
					break;
				}

				fixed (byte* inbuf = input) {
					byte* inptr = inbuf + inputIndex;
					byte* inend = inbuf + inputEnd;
					int startIndex = inputIndex;

					length = inputEnd - inputIndex;

					if (midline && length == nleft)
						found = BoundaryType.Eos;

					*inend = (byte) '\n';

					while (inptr < inend) {
						byte* start = inptr;

						while (*inptr != (byte) '\n')
							inptr++;

						length = (int) (inptr - start);

						if (inptr < inend) {
							found = CheckBoundary (startIndex, start, length);
							if (found != BoundaryType.None)
								break;

							length++;
							inptr++;
						} else {
							// didn't find the end of the line...
							midline = true;

							if (found == BoundaryType.None) {
								// not enough to tell if we found a boundary
								break;
							}

							found = CheckBoundary (startIndex, start, length);
							if (found != BoundaryType.None)
								break;
						}

						content.Write (input, startIndex, length);
						startIndex += length;
					}

					inputIndex = startIndex;
				}
			} while (found == BoundaryType.None);

			if (found != BoundaryType.Eos) {
				// the last \r\n belongs to the boundary
				if (content.Length > 0) {
					if (input[inputIndex - 2] == (byte) '\r')
						content.SetLength (content.Length - 2);
					else
						content.SetLength (content.Length - 1);
				}
			}

			return found;
		}

		BoundaryType ScanMimePartContent (MimePart part)
		{
			var memory = new MemoryStream ();
			var found = ScanContent (memory);

			memory.Seek (0, SeekOrigin.Begin);

			// FIXME: memory.ToArray() duplicates the buffer which hurts performance...
			// Maybe the ContentObject should take a stream instead or perhaps we should
			// use memory.GetBuffer() which returns the internal buffer, and then do
			// Array.Resize (ref data, stream.Length) to resize it to the correct size.
			// (The internal buffer is likely to be larger than what is actually needed.)
			part.ContentObject = new ContentObject (memory, part.ContentTransferEncoding);

			return found;
		}

		unsafe BoundaryType ScanMessagePart (MessagePart part)
		{
			BoundaryType found;

			if (bounds.Count > 0) {
				int atleast = Math.Min (ReadAheadSize, GetMaxBoundaryLength ());

				if (ReadAhead (atleast) <= 0)
					return BoundaryType.Eos;

				fixed (byte* inbuf = input) {
					byte* inptr = inbuf + inputIndex;
					byte* inend = inbuf + inputEnd;

					*inend = (byte) '\n';

					while (*inptr != (byte) '\n')
						inptr++;

					found = CheckBoundary (inputIndex, inbuf, (int) (inptr - inbuf));
					if (found == BoundaryType.Boundary)
						return BoundaryType.Boundary;

					if (found == BoundaryType.EndBoundary) {
						// ignore "From " boundaries, broken mailers tend to include these...
						if (!IsMboxMarker (inbuf))
							return BoundaryType.EndBoundary;
					}
				}
			}

			// parse the headers...
			state = MimeParserState.Headers;
			if (Step () == MimeParserState.Error) {
				// Note: currently this can't happen because StepHeaders() never returns error
				return BoundaryType.Eos;
			}

			var message = new MimeMessage (options, headers);
			var type = GetContentType (null);

			if (type.Matches ("multipart", "*")) {
				message.Body = ConstructMultipart (type, true, out found);
			} else {
				message.Body = ConstructEntity (type, true, out found);
			}

			part.Message = message;

			return found;
		}

		MimeEntity ConstructEntity (ContentType type, bool toplevel, out BoundaryType found)
		{
			var entity = MimeEntity.Create (options, type, headers, toplevel);

			if (entity is MessagePart) {
				found = ScanMessagePart ((MessagePart) entity);
			} else {
				found = ScanMimePartContent ((MimePart) entity);
			}

			return entity;
		}

		BoundaryType ScanMultipartPreambleOrEpilogue (Multipart multipart, bool preamble)
		{
			using (var memory = new MemoryStream ()) {
				var found = ScanContent (memory);

				if (preamble)
					multipart.RawPreamble = memory.ToArray ();
				else
					multipart.RawEpilogue = memory.ToArray ();

				return found;
			}
		}

		BoundaryType ScanMultipartSubparts (Multipart multipart)
		{
			BoundaryType found;
			MimeEntity entity;

			do {
				// skip over the boundary marker
				if (!SkipLine ())
					return BoundaryType.Eos;

				// parse the headers
				state = MimeParserState.Headers;
				if (Step () == MimeParserState.Error)
					return BoundaryType.Eos;

				//if (state == ParserState.Complete && headers.Count == 0)
				//	return BoundaryType.EndBoundary;

				var type = GetContentType (multipart.ContentType);

				if (type.Matches ("multipart", "*"))
					entity = ConstructMultipart (type, false, out found);
				else
					entity = ConstructEntity (type, false, out found);

				multipart.Add (entity);
			} while (found == BoundaryType.Boundary && FoundImmediateBoundary (false));

			return found;
		}

		void PushBoundary (string boundary)
		{
			if (bounds.Count > 0)
				bounds.Insert (0, new Boundary (boundary, bounds[0].MaxLength));
			else
				bounds.Add (new Boundary (boundary, 0));
		}

		void PopBoundary ()
		{
			bounds.RemoveAt (0);
		}

		Multipart ConstructMultipart (ContentType type, bool toplevel, out BoundaryType found)
		{
			var multipart = (Multipart) MimeEntity.Create (options, type, headers, toplevel);

			var boundary = type.Parameters["boundary"];
			if (boundary == null) {
				Debug.WriteLine ("Multipart without a boundary encountered!");

				// Note: this will scan all content into the preamble...
				found = ScanMultipartPreambleOrEpilogue (multipart, true);
				return multipart;
			}

			PushBoundary (boundary);

			found = ScanMultipartPreambleOrEpilogue (multipart, true);
			if (found == BoundaryType.Boundary)
				found = ScanMultipartSubparts (multipart);

			if (found == BoundaryType.EndBoundary && FoundImmediateBoundary (true)) {
				// consume the end boundary and read the epilogue (if there is one)
				SkipLine ();
				PopBoundary ();
				found = ScanMultipartPreambleOrEpilogue (multipart, false);
			} else {
				// We either found the end of the stream or we found a parent's boundary
				PopBoundary ();
			}

			return multipart;
		}

		public MimeEntity ParseEntity ()
		{
			state = MimeParserState.Headers;
			while (state < MimeParserState.Content) {
				if (Step () == MimeParserState.Error)
					throw new Exception ("Failed to parse entity headers.");
			}

			var type = GetContentType (null);
			BoundaryType found;
			MimeEntity entity;

			if (type.Matches ("multipart", "*"))
				entity = ConstructMultipart (type, true, out found);
			else
				entity = ConstructEntity (type, true, out found);

			if (found != BoundaryType.Eos)
				state = MimeParserState.Complete;
			else
				state = MimeParserState.Eos;

			return entity;
		}

		public MimeMessage ParseMessage ()
		{
			BoundaryType found;

			// scan the from-line if we are parsing an mbox
			while (state != MimeParserState.MessageHeaders) {
				switch (Step ()) {
				case MimeParserState.Error:
					throw new Exception ("Failed to find mbox From marker.");
				case MimeParserState.Eos:
					throw new Exception ("End of stream.");
				}
			}

			// parse the headers
			while (state < MimeParserState.Content) {
				if (Step () == MimeParserState.Error)
					throw new Exception ("Failed to parse message headers.");
			}

			var message = new MimeMessage (options, headers);

			if (format == MimeFormat.Mbox && options.RespectContentLength) {
				bounds[0].ContentEnd = -1;

				foreach (var header in headers) {
					if (icase.Compare (header.Field, "Content-Length") != 0)
						continue;

					var value = header.RawValue;
					int length, index = 0;

					if (!ParseUtils.TryParseInt32 (value, ref index, value.Length, out length))
						continue;

					long endOffset = mboxMarkerOffset + mboxMarkerLength + length;

					bounds[0].ContentEnd = endOffset;
					break;
				}
			}

			var type = GetContentType (null);

			if (type.Matches ("multipart", "*"))
				message.Body = ConstructMultipart (type, true, out found);
			else
				message.Body = ConstructEntity (type, true, out found);

			if (found != BoundaryType.Eos) {
				if (format == MimeFormat.Mbox)
					state = MimeParserState.FromLine;
				else
					state = MimeParserState.Complete;
			} else {
				state = MimeParserState.Eos;
			}

			return message;
		}
	}
}

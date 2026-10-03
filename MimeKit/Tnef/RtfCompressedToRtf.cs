//
// RtfCompressedToRtf.cs
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

using MimeKit.IO.Filters;
using MimeKit.Utils;

namespace MimeKit.Tnef {
	/// <summary>
	/// A filter to decompress a compressed RTF stream.
	/// </summary>
	/// <remarks>
	/// Used to decompress a compressed RTF stream.
	/// </remarks>
	public class RtfCompressedToRtf : MimeFilterBase
	{
		static ReadOnlySpan<byte> DictionaryInitializer => "{\\rtf1\\ansi\\mac\\deff0\\deftab720{\\fonttbl;}{\\f0\\fnil \\froman \\fswiss \\fmodern \\fscript \\fdecor MS Sans SerifSymbolArialTimes New RomanCourier{\\colortbl\\red0\\green0\\blue0\r\n\\par \\pard\\plain\\f0\\fs20\\b\\i\\u\\tab\\tx"u8;

		enum FilterState {
			CompressedSize,
			UncompressedSize,
			Magic,
			Crc32,
			BeginControlRun,
			ReadControlOffset,
			ProcessControl,
			ReadLiteral,
			Complete,
		}

		// The maximum number of bytes that we will pre-allocate for the output buffer based on the
		// (untrusted) RAWSIZE and COMPSIZE header fields.
		const int MaxEstimatedOutputSize = 1024 * 1024;

		readonly byte[] dict = new byte[4096];
		readonly Crc32 crc32 = new Crc32 ();
		FilterState state;
		int uncompressedSize;
		int compressedSize;
		short dictWriteOffset;
		short dictReadOffset;
		byte flagCount;
		byte flags;
		int checksum;
		int saved;
		int size;

		/// <summary>
		/// Initialize a new instance of the <see cref="RtfCompressedToRtf"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="RtfCompressedToRtf"/> converter filter.
		/// </remarks>
		public RtfCompressedToRtf ()
		{
			dictWriteOffset = (short) DictionaryInitializer.Length; // 207
			DictionaryInitializer.CopyTo (dict);
		}

		/// <summary>
		/// Get the compression mode.
		/// </summary>
		/// <remarks>
		/// <para>At least 12 bytes from the stream must be processed before this property value will
		/// be accurate. Until then, and after a call to <see cref="Reset"/>, the compression mode
		/// is <see cref="RtfCompressionMode.Unknown"/>.</para>
		/// <para>Once the header has been processed, this property holds the raw value of the COMPTYPE field.
		/// <a href="https://learn.microsoft.com/openspecs/exchange_server_protocols/ms-oxrtfcp/">[MS-OXRTFCP]</a>
		/// only defines <see cref="RtfCompressionMode.Compressed"/> and <see cref="RtfCompressionMode.Uncompressed"/>;
		/// any other value means that the stream is malformed, and the filter discards its content and produces no
		/// output.</para>
		/// </remarks>
		/// <value>The compression mode.</value>
		public RtfCompressionMode CompressionMode {
			get; private set;
		}

		/// <summary>
		/// Get a value indicating whether the crc32 is valid.
		/// </summary>
		/// <remarks>
		/// <para>Until all data has been processed, this property will always return <see langword="false" />.</para>
		/// <para>Note: <a href="https://learn.microsoft.com/openspecs/exchange_server_protocols/ms-oxrtfcp/">[MS-OXRTFCP]</a>
		/// only defines the CRC for <see cref="RtfCompressionMode.Compressed"/> streams; an
		/// <see cref="RtfCompressionMode.Uncompressed"/> stream must set the CRC field to <c>0</c>, so no
		/// checksum is computed over its content.</para>
		/// </remarks>
		/// <value><see langword="true" /> if the crc32 is valid; otherwise, <see langword="false" />.</value>
		public bool IsValidCrc32 {
			get { return crc32.Checksum == checksum; }
		}

		bool TryReadInt32 (byte[] buffer, ref int index, int endIndex, out int value)
		{
			if (index == endIndex) {
				value = saved;
				return false;
			}

			int nread = (saved >> 24) & 0xFF;

			saved &= 0x00FFFFFF;

			switch (nread) {
			case 0:
				saved = buffer[index++];
				nread++;

				if (index == endIndex)
					break;

				goto case 1;
			case 1:
				saved |= (buffer[index++] << 8);
				nread++;

				if (index == endIndex)
					break;

				goto case 2;
			case 2:
				saved |= (buffer[index++] << 16);
				nread++;

				if (index == endIndex)
					break;

				goto case 3;
			case 3:
				saved |= (buffer[index++] << 24);
				nread++;
				break;
			}

			value = saved;

			if (nread == 4) {
				saved = 0;
				return true;
			}

			saved |= nread << 24;

			return false;
		}

		/// <summary>
		/// Filter the specified input.
		/// </summary>
		/// <remarks>Filters the specified input buffer starting at the given index,
		/// spanning across the specified number of bytes.</remarks>
		/// <returns>The filtered output.</returns>
		/// <param name="input">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">Length.</param>
		/// <param name="outputIndex">Output index.</param>
		/// <param name="outputLength">Output length.</param>
		/// <param name="flush">If set to <see langword="true" /> flush.</param>
		protected override byte[] Filter (byte[] input, int startIndex, int length, out int outputIndex, out int outputLength, bool flush)
		{
			int endIndex = startIndex + length;
			int index = startIndex;

			// read the compressed size if we haven't already...
			if (state == FilterState.CompressedSize) {
				if (!TryReadInt32 (input, ref index, endIndex, out compressedSize)) {
					outputLength = 0;
					outputIndex = 0;
					return input;
				}

				state = FilterState.UncompressedSize;

				// Note: [MS-OXRTFCP] defines COMPSIZE as the length of the CONTENTS field plus 12 (the
				// number of header bytes that follow it), so anything smaller is nonsensical and is
				// treated as an empty CONTENTS field rather than allowed to go negative.
				compressedSize = compressedSize >= 12 ? compressedSize - 12 : 0;
			}

			// read the uncompressed size if we haven't already...
			if (state == FilterState.UncompressedSize) {
				if (!TryReadInt32 (input, ref index, endIndex, out uncompressedSize)) {
					outputLength = 0;
					outputIndex = 0;
					return input;
				}

				state = FilterState.Magic;
			}

			// read the compression mode magic if we haven't already...
			if (state == FilterState.Magic) {
				if (!TryReadInt32 (input, ref index, endIndex, out int magic)) {
					outputLength = 0;
					outputIndex = 0;
					return input;
				}

				CompressionMode = (RtfCompressionMode) magic;
				state = FilterState.Crc32;
			}

			// read the crc32 checksum if we haven't already...
			if (state == FilterState.Crc32) {
				if (!TryReadInt32 (input, ref index, endIndex, out checksum)) {
					outputLength = 0;
					outputIndex = 0;
					return input;
				}

				state = FilterState.BeginControlRun;
			}

			if (CompressionMode != RtfCompressionMode.Compressed && CompressionMode != RtfCompressionMode.Uncompressed) {
				// Note: [MS-OXRTFCP] 2.1.3.1.1 defines only the COMPRESSED and UNCOMPRESSED values of COMPTYPE, and
				// decompression is not defined for any other value. The contents cannot be interpreted, and passing
				// them through would label arbitrary bytes as RTF, so discard them.
				outputLength = 0;
				outputIndex = endIndex;

				return input;
			}

			if (CompressionMode == RtfCompressionMode.Uncompressed) {
				// Note: [MS-OXRTFCP] requires the CRC field of an UNCOMPRESSED stream to be 0 and only
				// defines a checksum over the content of a COMPRESSED stream, so do not accumulate one.
				outputLength = Math.Max (Math.Min (endIndex - index, compressedSize - size), 0);
				size += outputLength;
				outputIndex = index;

				return input;
			}

			// Note: Both compressedSize and uncompressedSize come from the (untrusted) header, so use 64-bit
			// arithmetic to avoid overflowing and clamp the estimate to a sane upper bound so that a bogus
			// size cannot force an enormous allocation. The output buffer will grow on demand if needed.
			long extra = Math.Abs ((long) uncompressedSize - compressedSize);
			long estimatedSize = Math.Min ((endIndex - index) + extra, MaxEstimatedOutputSize);

			EnsureOutputSize ((int) Math.Max (estimatedSize, 4096), false);
			outputLength = 0;
			outputIndex = 0;

			// Note: [MS-OXRTFCP] defines COMPSIZE as the length of the CONTENTS field plus 12, so stop once
			// that many bytes have been consumed. Without this, data that follows the stream would be
			// decompressed into the output and folded into the CRC.
			while (index < endIndex && size < compressedSize && state != FilterState.Complete) {
				byte value = input[index++];

				crc32.Update (value);
				size++;

				switch (state) {
				case FilterState.BeginControlRun:
					flags = value;
					flagCount = 1;

					if ((flags & 0x1) != 0)
						state = FilterState.ReadControlOffset;
					else
						state = FilterState.ReadLiteral;
					break;
				case FilterState.ReadLiteral:
					EnsureOutputSize (outputLength + 1, true);
					OutputBuffer[outputLength++] = value;
					dict[dictWriteOffset++] = value;

					dictWriteOffset = (short) (dictWriteOffset % 4096);

					if ((flagCount++ % 8) != 0) {
						flags = (byte) (flags >> 1);

						if ((flags & 0x1) != 0)
							state = FilterState.ReadControlOffset;
						else
							state = FilterState.ReadLiteral;
					} else {
						state = FilterState.BeginControlRun;
					}
					break;
				case FilterState.ReadControlOffset:
					state = FilterState.ProcessControl;
					dictReadOffset = value;
					break;
				case FilterState.ProcessControl:
					dictReadOffset = (short) ((dictReadOffset << 4) | (value >> 4));
					int controlLength = (value & 0x0F) + 2;

					if (dictReadOffset == dictWriteOffset) {
						state = FilterState.Complete;
						break;
					}

					EnsureOutputSize (outputLength + controlLength, true);

					int controlEnd = dictReadOffset + controlLength;

					while (dictReadOffset < controlEnd) {
						value = dict[dictReadOffset++ % 4096];
						OutputBuffer[outputLength++] = value;
						dict[dictWriteOffset++] = value;

						dictWriteOffset = (short) (dictWriteOffset % 4096);
					}

					if ((flagCount++ % 8) != 0) {
						flags = (byte) (flags >> 1);

						if ((flags & 0x1) != 0)
							state = FilterState.ReadControlOffset;
						else
							state = FilterState.ReadLiteral;
					} else {
						state = FilterState.BeginControlRun;
					}
					break;
				}
			}

			return OutputBuffer;
		}

		/// <summary>
		/// Reset the filter.
		/// </summary>
		/// <remarks>
		/// Resets the filter.
		/// </remarks>
		public override void Reset ()
		{
			dictWriteOffset = (short) DictionaryInitializer.Length; // 207
			DictionaryInitializer.CopyTo (dict);
			state = FilterState.CompressedSize;
			CompressionMode = RtfCompressionMode.Unknown;
			uncompressedSize = 0;
			dictReadOffset = 0;
			compressedSize = 0;
			crc32.Reset ();
			flagCount = 0;
			checksum = 0;
			flags = 0;
			saved = 0;
			size = 0;

			base.Reset ();
		}
	}
}

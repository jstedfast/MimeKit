//
// RtfCompressedToRtf.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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

#if PORTABLE
using Encoding = Portable.Text.Encoding;
#else
using Encoding = System.Text.Encoding;
#endif

namespace MimeKit.Tnef {
	/// <summary>
	/// A filter to decompress a compressed RTF stream.
	/// </summary>
	/// <remarks>
	/// Used to decompress a compressed RTF stream.
	/// </remarks>
	public class RtfCompressedToRtf : MimeFilterBase
	{
		const string DictionaryInitializerText = "{\\rtf1\\ansi\\mac\\deff0\\deftab720{\\fonttbl;}" +
			"{\\f0\\fnil \\froman \\fswiss \\fmodern \\fscript \\fdecor MS Sans SerifSymbolArialTimes New RomanCourier" +
			"{\\colortbl\\red0\\green0\\blue0\r\n\\par \\pard\\plain\\f0\\fs20\\b\\i\\u\\tab\\tx";
		static readonly byte[] DictionaryInitializer = Encoding.ASCII.GetBytes (DictionaryInitializerText);

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

		readonly byte[] dict = new byte[4096];
		readonly Crc32 crc32 = new Crc32 ();
		FilterState state;
		int uncompressedSize;
		int compressedSize;
		short dictWriteOffset;
		short dictReadOffset;
		short dictEndOffset;
		byte flagCount;
		byte flags;
		int checksum;
		int saved;
		int size;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Tnef.RtfCompressedToRtf"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimeKit.Tnef.RtfCompressedToRtf"/> converter filter.
		/// </remarks>
		public RtfCompressedToRtf ()
		{
			Buffer.BlockCopy (DictionaryInitializer, 0, dict, 0, DictionaryInitializer.Length);
			dictEndOffset = dictWriteOffset = (short) DictionaryInitializer.Length; // 207
		}

		/// <summary>
		/// Gets the compression mode.
		/// </summary>
		/// <remarks>
		/// At least 12 bytes from the stream must be processed before this property value will
		/// be accurate.
		/// </remarks>
		/// <value>The compression mode.</value>
		public RtfCompressionMode CompressionMode {
			get; private set;
		}

		/// <summary>
		/// Gets a value indicating whether the crc32 is valid.
		/// </summary>
		/// <remarks>
		/// Until all data has been processed, this property will always return <c>false</c>.
		/// </remarks>
		/// <value><c>true</c> if the crc32 is valid; otherwise, <c>false</c>.</value>
		public bool IsValidCrc32 {
			get { return crc32.Checksum == checksum; }
		}

		bool TryReadInt32 (byte[] buffer, ref int index, int endIndex, out int value)
		{
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
		/// <param name="flush">If set to <c>true</c> flush.</param>
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
				compressedSize -= 12;
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
				int magic;

				if (!TryReadInt32 (input, ref index, endIndex, out magic)) {
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

			if (CompressionMode != RtfCompressionMode.Compressed) {
				// the data is not compressed, just keep track of the CRC32 checksum
				crc32.Update (input, index, endIndex - index);

				outputLength = Math.Max (Math.Min (endIndex - index, compressedSize - size), 0);
				size += outputLength;
				outputIndex = index;

				return input;
			}

			int extra = Math.Abs (uncompressedSize - compressedSize);
			int estimatedSize = (endIndex - index) + extra;

			EnsureOutputSize (Math.Max (estimatedSize, 4096), false);
			outputLength = 0;
			outputIndex = 0;

			while (index < endIndex) {
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

					dictEndOffset = Math.Max (dictWriteOffset, dictEndOffset);
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

						dictEndOffset = Math.Max (dictWriteOffset, dictEndOffset);
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
				case FilterState.Complete:
					break;
				}
			}

			return OutputBuffer;
		}

		/// <summary>
		/// Resets the filter.
		/// </summary>
		/// <remarks>
		/// Resets the filter.
		/// </remarks>
		public override void Reset ()
		{
			Buffer.BlockCopy (DictionaryInitializer, 0, dict, 0, DictionaryInitializer.Length);
			dictEndOffset = dictWriteOffset = (short) DictionaryInitializer.Length; // 207
			state = FilterState.CompressedSize;
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

//
// RtfCompressedBuilder.cs
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

using System.Buffers.Binary;
using System.Collections.Generic;

using MimeKit.Tnef;
using MimeKit.Utils;

namespace UnitTests.Tnef {
	/// <summary>
	/// Builds compressed RTF streams as described by [MS-OXRTFCP].
	/// </summary>
	/// <remarks>
	/// This is a deliberately simple reference encoder written from the specification. It does not
	/// search for matches on its own; the caller chooses which tokens to emit, which is what makes it
	/// useful for synthesizing streams that exercise a specific part of the decompressor, including
	/// streams that are malformed on purpose.
	/// </remarks>
	class RtfCompressedBuilder
	{
		/// <summary>
		/// The length of the string that the dictionary is pre-initialized with.
		/// </summary>
		public const int DictionaryInitializerLength = 207;

		/// <summary>
		/// The size of the dictionary, in bytes.
		/// </summary>
		public const int DictionarySize = 4096;

		const string DictionaryInitializer = "{\\rtf1\\ansi\\mac\\deff0\\deftab720{\\fonttbl;}{\\f0\\fnil \\froman \\fswiss \\fmodern \\fscript \\fdecor MS Sans SerifSymbolArialTimes New RomanCourier{\\colortbl\\red0\\green0\\blue0\r\n\\par \\pard\\plain\\f0\\fs20\\b\\i\\u\\tab\\tx";

		struct Token
		{
			public bool IsLiteral;
			public byte Value;
			public int Offset;
			public int Length;
		}

		readonly List<Token> tokens = new List<Token> ();
		readonly List<byte> decompressed = new List<byte> ();
		readonly byte[] dict = new byte[DictionarySize];
		int writeOffset = DictionaryInitializerLength;

		public RtfCompressedBuilder ()
		{
			for (int i = 0; i < DictionaryInitializerLength; i++)
				dict[i] = (byte) DictionaryInitializer[i];
		}

		/// <summary>
		/// Get the current dictionary write offset.
		/// </summary>
		public int DictionaryWriteOffset {
			get { return writeOffset; }
		}

		/// <summary>
		/// Get the data that a conforming decompressor is expected to produce.
		/// </summary>
		public byte[] GetDecompressed ()
		{
			return decompressed.ToArray ();
		}

		void Append (byte value)
		{
			decompressed.Add (value);
			dict[writeOffset] = value;
			writeOffset = (writeOffset + 1) % DictionarySize;
		}

		/// <summary>
		/// Emit a literal byte.
		/// </summary>
		public RtfCompressedBuilder WriteLiteral (byte value)
		{
			tokens.Add (new Token { IsLiteral = true, Value = value });
			Append (value);

			return this;
		}

		/// <summary>
		/// Emit each byte of <paramref name="data"/> as a literal.
		/// </summary>
		public RtfCompressedBuilder WriteLiterals (byte[] data)
		{
			for (int i = 0; i < data.Length; i++)
				WriteLiteral (data[i]);

			return this;
		}

		/// <summary>
		/// Emit a dictionary reference.
		/// </summary>
		/// <remarks>
		/// The decompressor copies out of the dictionary one byte at a time and writes each byte back as
		/// it goes, so a reference is allowed to overlap the bytes it is producing.
		/// </remarks>
		public RtfCompressedBuilder WriteReference (int offset, int length)
		{
			tokens.Add (new Token { Offset = offset, Length = length });

			for (int i = 0; i < length; i++)
				Append (dict[(offset + i) % DictionarySize]);

			return this;
		}

		/// <summary>
		/// Emit the control token that marks the end of the stream.
		/// </summary>
		public RtfCompressedBuilder WriteEndOfStream ()
		{
			tokens.Add (new Token { Offset = writeOffset, Length = 2 });

			return this;
		}

		/// <summary>
		/// Get the CONTENTS field.
		/// </summary>
		public byte[] GetContents ()
		{
			var contents = new List<byte> ();

			for (int i = 0; i < tokens.Count; i += 8) {
				int n = Math.Min (8, tokens.Count - i);
				byte flags = 0;

				for (int j = 0; j < n; j++) {
					if (!tokens[i + j].IsLiteral)
						flags |= (byte) (1 << j);
				}

				contents.Add (flags);

				for (int j = 0; j < n; j++) {
					var token = tokens[i + j];

					if (token.IsLiteral) {
						contents.Add (token.Value);
					} else {
						contents.Add ((byte) (token.Offset >> 4));
						contents.Add ((byte) (((token.Offset & 0x0F) << 4) | (token.Length - 2)));
					}
				}
			}

			return contents.ToArray ();
		}

		/// <summary>
		/// Get the complete stream.
		/// </summary>
		/// <remarks>
		/// Each header field can be overridden so that a caller can produce a stream that lies about its
		/// own shape.
		/// </remarks>
		public byte[] ToArray (int? compressedSize = null, int? uncompressedSize = null, int? crc = null, int? compressionType = null)
		{
			var contents = GetContents ();
			var buffer = new byte[16 + contents.Length];

			if (!crc.HasValue) {
				var crc32 = new Crc32 ();

				crc32.Update (contents, 0, contents.Length);
				crc = crc32.Checksum;
			}

			BinaryPrimitives.WriteInt32LittleEndian (buffer.AsSpan (0, 4), compressedSize ?? (contents.Length + 12));
			BinaryPrimitives.WriteInt32LittleEndian (buffer.AsSpan (4, 4), uncompressedSize ?? decompressed.Count);
			BinaryPrimitives.WriteInt32LittleEndian (buffer.AsSpan (8, 4), compressionType ?? (int) RtfCompressionMode.Compressed);
			BinaryPrimitives.WriteInt32LittleEndian (buffer.AsSpan (12, 4), crc.Value);
			contents.CopyTo (buffer.AsSpan (16));

			return buffer;
		}
	}
}

//
// UnicodeNormalization.cs
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
using System.Buffers;
using System.Buffers.Binary;

namespace MimeKit.Utils {
	/// <summary>
	/// Unicode normalization checks (UAX #15).
	/// </summary>
	/// <remarks>
	/// The tables in UnicodeNormalization.g.cs are generated from the Unicode Character Database by
	/// tools/SmtpUtf8TableGenerator.
	/// </remarks>
	static partial class UnicodeNormalization
	{
		enum NormalizationQuickCheck : byte
		{
			Yes = 0,
			Maybe = 1,
			No = 2
		}

		// The Unicode Standard, Section 3.12: Conjoining Jamo Behavior.
		const int HangulSBase = 0xAC00;
		const int HangulLBase = 0x1100;
		const int HangulVBase = 0x1161;
		const int HangulTBase = 0x11A7;
		const int HangulLCount = 19;
		const int HangulVCount = 21;
		const int HangulTCount = 28;
		const int HangulNCount = HangulVCount * HangulTCount;
		const int HangulSCount = HangulLCount * HangulNCount;

		// Every code point below U+0300 has Canonical_Combining_Class=0 and NFC_Quick_Check=Yes (the generator verifies this).
		internal const int FirstNormalizationCodePoint = 0x0300;

		// The first primary composite is U+00C0 LATIN CAPITAL LETTER A WITH GRAVE.
		const int FirstCompositeCodePoint = 0x00C0;

		// While normalizing, each code point is packed together with its Canonical_Combining_Class.
		const int CombiningClassShift = 21;
		const int CodePointMask = (1 << CombiningClassShift) - 1;

		static int GetCombiningClass (int codePoint, out NormalizationQuickCheck quickCheck)
		{
			uint entry = LookupEntry (NormalizationData, NormalizationDataCount, 10, codePoint);

			quickCheck = (NormalizationQuickCheck) ((entry >> 8) & 0x3);

			return (int) (entry & 0xFF);
		}

		static int GetCombiningClass (int codePoint)
		{
			if (codePoint < FirstNormalizationCodePoint)
				return 0;

			return GetCombiningClass (codePoint, out _);
		}

		/// <summary>
		/// Check whether text is in Unicode Normalization Form C.
		/// </summary>
		/// <remarks>
		/// Uses the NFC_Quick_Check algorithm defined in UAX #15, Section 9 and, only if that returns <c>MAYBE</c>,
		/// compares the text with its NFC form.
		/// </remarks>
		/// <returns><see langword="true" /> if the text is in NFC; otherwise, <see langword="false" />.</returns>
		/// <param name="text">The text.</param>
		internal static bool IsNormalizationFormC (ReadOnlySpan<char> text)
		{
			int lastCombiningClass = 0;
			bool maybe = false;
			int index = 0;

			while (index < text.Length) {
				int codePoint = GetCodePoint (text, index, out int length);

				index += length;

				if (codePoint < FirstNormalizationCodePoint) {
					lastCombiningClass = 0;
					continue;
				}

				int combiningClass = GetCombiningClass (codePoint, out var quickCheck);

				if (combiningClass != 0 && lastCombiningClass > combiningClass)
					return false;

				if (quickCheck == NormalizationQuickCheck.No)
					return false;

				if (quickCheck == NormalizationQuickCheck.Maybe)
					maybe = true;

				lastCombiningClass = combiningClass;
			}

			return !maybe || IsNormalizationFormCSlow (text);
		}

		static bool IsNormalizationFormCSlow (ReadOnlySpan<char> text)
		{
			var buffer = ArrayPool<int>.Shared.Rent (Math.Max (text.Length, 16) + 16);
			int count = 0;

			try {
				int index = 0;

				// Get the canonical decomposition of the text. Note: Each code point is inserted in canonical order, which
				// costs linear time because the quick check has already verified that the text itself is in canonical order
				// and only a starter can expand into a starter followed by (at most a few) non-starters.
				while (index < text.Length) {
					int codePoint = GetCodePoint (text, index, out int length);

					Decompose (codePoint, ref buffer, ref count);
					index += length;
				}

				count = Compose (buffer, count);

				// Compare the result of normalization with the original text.
				index = 0;

				for (int i = 0; i < count; i++) {
					if (index == text.Length)
						return false;

					int codePoint = GetCodePoint (text, index, out int length);

					if (codePoint != (buffer[i] & CodePointMask))
						return false;

					index += length;
				}

				return index == text.Length;
			} finally {
				ArrayPool<int>.Shared.Return (buffer);
			}
		}

		static void Decompose (int codePoint, ref int[] buffer, ref int count)
		{
			int sIndex = codePoint - HangulSBase;

			if (sIndex >= 0 && sIndex < HangulSCount) {
				int tIndex = sIndex % HangulTCount;

				// Note: Conjoining jamo are all starters.
				Append (ref buffer, ref count, HangulLBase + sIndex / HangulNCount);
				Append (ref buffer, ref count, HangulVBase + (sIndex % HangulNCount) / HangulTCount);

				if (tIndex != 0)
					Append (ref buffer, ref count, HangulTBase + tIndex);

				return;
			}

			if (codePoint >= FirstCompositeCodePoint && TryGetDecomposition (codePoint, out int first, out int second)) {
				Decompose (first, ref buffer, ref count);
				Decompose (second, ref buffer, ref count);
				return;
			}

			int combiningClass = GetCombiningClass (codePoint);

			Append (ref buffer, ref count, (combiningClass << CombiningClassShift) | codePoint);

			if (combiningClass == 0)
				return;

			// Canonical Ordering Algorithm (The Unicode Standard, Section 3.11): move the non-starter in front of any
			// preceding non-starters that have a higher Canonical_Combining_Class.
			int index = count - 1;
			int value = buffer[index];

			while (index > 0 && (buffer[index - 1] >> CombiningClassShift) > combiningClass) {
				buffer[index] = buffer[index - 1];
				index--;
			}

			buffer[index] = value;
		}

		static void Append (ref int[] buffer, ref int count, int value)
		{
			if (count == buffer.Length) {
				var larger = ArrayPool<int>.Shared.Rent (buffer.Length * 2);

				buffer.AsSpan (0, count).CopyTo (larger);
				ArrayPool<int>.Shared.Return (buffer);
				buffer = larger;
			}

			buffer[count++] = value;
		}

		// The Unicode Standard, Section 3.11: Canonical Composition Algorithm.
		static int Compose (int[] buffer, int count)
		{
			int lastCombiningClass = 0;
			int starter = -1;
			int written = 0;

			for (int i = 0; i < count; i++) {
				int value = buffer[i];
				int codePoint = value & CodePointMask;
				int combiningClass = value >> CombiningClassShift;

				// Note: Since only non-starters can be written after the last starter, the code point is not blocked from
				// the last starter if it is adjacent to it or if the last code point written has a lower combining class.
				if (starter != -1 && (starter == written - 1 || lastCombiningClass < combiningClass) && TryCompose (buffer[starter], codePoint, out int composite)) {
					// Note: Every primary composite is a starter, so its combining class is 0.
					buffer[starter] = composite;
					continue;
				}

				if (combiningClass == 0)
					starter = written;

				lastCombiningClass = combiningClass;
				buffer[written++] = value;
			}

			return written;
		}

		static bool TryCompose (int first, int second, out int composite)
		{
			int lIndex = first - HangulLBase;

			if (lIndex >= 0 && lIndex < HangulLCount) {
				int vIndex = second - HangulVBase;

				if (vIndex >= 0 && vIndex < HangulVCount) {
					composite = HangulSBase + (lIndex * HangulVCount + vIndex) * HangulTCount;
					return true;
				}
			}

			int sIndex = first - HangulSBase;

			if (sIndex >= 0 && sIndex < HangulSCount && (sIndex % HangulTCount) == 0) {
				int tIndex = second - HangulTBase;

				if (tIndex > 0 && tIndex < HangulTCount) {
					composite = first + tIndex;
					return true;
				}
			}

			var pairs = CompositionPairs;
			var data = CompositionData;
			int low = 0, high = CompositionCount - 1;

			while (low <= high) {
				int mid = (low + high) >> 1;
				int offset = BinaryPrimitives.ReadUInt16LittleEndian (pairs.Slice (mid * 2, 2)) * 12;
				int midFirst = BinaryPrimitives.ReadInt32LittleEndian (data.Slice (offset + 4, 4));
				int midSecond = BinaryPrimitives.ReadInt32LittleEndian (data.Slice (offset + 8, 4));

				if (midFirst < first || (midFirst == first && midSecond < second)) {
					low = mid + 1;
				} else if (midFirst > first || midSecond > second) {
					high = mid - 1;
				} else {
					composite = BinaryPrimitives.ReadInt32LittleEndian (data.Slice (offset, 4));
					return true;
				}
			}

			composite = 0;

			return false;
		}

		static bool TryGetDecomposition (int composite, out int first, out int second)
		{
			var data = CompositionData;
			int low = 0, high = CompositionCount - 1;

			while (low <= high) {
				int mid = (low + high) >> 1;
				int offset = mid * 12;
				int midComposite = BinaryPrimitives.ReadInt32LittleEndian (data.Slice (offset, 4));

				if (midComposite < composite) {
					low = mid + 1;
				} else if (midComposite > composite) {
					high = mid - 1;
				} else {
					first = BinaryPrimitives.ReadInt32LittleEndian (data.Slice (offset + 4, 4));
					second = BinaryPrimitives.ReadInt32LittleEndian (data.Slice (offset + 8, 4));
					return true;
				}
			}

			first = second = 0;

			return false;
		}


		static int GetCodePoint (ReadOnlySpan<char> text, int index, out int length)
		{
			char c = text[index];

			if (char.IsHighSurrogate (c) && index + 1 < text.Length && char.IsLowSurrogate (text[index + 1])) {
				length = 2;
				return char.ConvertToUtf32 (c, text[index + 1]);
			}

			// Note: a lone surrogate is returned as-is; it has Canonical_Combining_Class=0 and NFC_Quick_Check=Yes.
			length = 1;

			return c;
		}

		static uint LookupEntry (ReadOnlySpan<byte> table, int count, int shift, int codePoint)
		{
			// Find the last entry whose first code point is <= codePoint. The first entry always starts at U+0000.
			int low = 0, high = count - 1;

			while (low < high) {
				int mid = (low + high + 1) >> 1;
				uint entry = BinaryPrimitives.ReadUInt32LittleEndian (table.Slice (mid * 4, 4));

				if ((int) (entry >> shift) <= codePoint)
					low = mid;
				else
					high = mid - 1;
			}

			return BinaryPrimitives.ReadUInt32LittleEndian (table.Slice (low * 4, 4));
		}
	}
}

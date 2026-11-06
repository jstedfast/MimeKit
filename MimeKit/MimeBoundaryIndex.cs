//
// MimeBoundaryIndex.cs
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
using System.Collections.Generic;

namespace MimeKit {
	sealed class MimeBoundaryIndex
	{
		sealed class MarkerComparer : IComparer<Entry>
		{
			public int Compare (Entry? x, Entry? y)
			{
				int order = x!.Marker.Length.CompareTo (y!.Marker.Length);

				return order != 0 ? order : x.Marker.Span.SequenceCompareTo (y.Marker.Span);
			}
		}

		sealed class Entry
		{
			public ReadOnlyMemory<byte> Marker;
			public readonly MimeBoundary? Boundary;
			public readonly Entry? Previous;
			public readonly int Depth;

			public Entry (MimeBoundary? boundary, Entry? previous, int depth)
			{
				if (boundary != null)
					Marker = new ReadOnlyMemory<byte> (boundary.Marker, 0, boundary.Length);
				Boundary = boundary;
				Previous = previous;
				Depth = depth;
			}
		}

		static readonly MarkerComparer Comparer = new MarkerComparer ();
		readonly SortedDictionary<Entry, Entry> markers = new SortedDictionary<Entry, Entry> (Comparer);
		// Reference keys avoid boxing in the dictionary's generic null checks
		// before tiered compilation optimizes them. Never insert the lookup key.
		readonly Entry lookup = new Entry (null, null, 0);
		int depth;

		public void Clear ()
		{
			markers.Clear ();
			depth = 0;
		}

		public void Add (MimeBoundary boundary)
		{
			lookup.Marker = new ReadOnlyMemory<byte> (boundary.Marker, 0, boundary.Length);

			markers.TryGetValue (lookup, out var previous);
			var entry = new Entry (boundary, previous, ++depth);
			markers[entry] = entry;
			lookup.Marker = default;
		}

		public void Remove (MimeBoundary boundary)
		{
			lookup.Marker = new ReadOnlyMemory<byte> (boundary.Marker, 0, boundary.Length);
			var entry = markers[lookup];

			if (entry.Previous != null)
				markers[entry] = entry.Previous;
			else
				markers.Remove (entry);

			lookup.Marker = default;
			depth--;
		}

		public bool TryGetBoundary (byte[] buffer, int startIndex, int length, out MimeBoundary? boundary, out bool final)
		{
			lookup.Marker = new ReadOnlyMemory<byte> (buffer, startIndex, length);

			markers.TryGetValue (lookup, out var match);
			final = false;

			// A closing marker may also be another boundary's opening marker. The
			// innermost active match wins, regardless of which form matched.
			if (length >= 2 && buffer[startIndex + length - 2] == (byte) '-' && buffer[startIndex + length - 1] == (byte) '-') {
				lookup.Marker = lookup.Marker.Slice (0, length - 2);
				if (markers.TryGetValue (lookup, out var closing) && (match == null || closing.Depth > match.Depth)) {
					match = closing;
					final = true;
				}
			}

			lookup.Marker = default;
			boundary = match?.Boundary;
			return match != null;
		}
	}
}

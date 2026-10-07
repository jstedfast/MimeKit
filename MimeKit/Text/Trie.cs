//
// Trie.cs
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

namespace MimeKit.Text {
	/// <summary>
	/// An Aho-Corasick Trie graph.
	/// </summary>
	/// <remarks>
	/// An Aho-Corasick Trie graph.
	/// </remarks>
	class Trie
	{
		class TrieState {
			public TrieState? Next;
			public TrieState? Fail;
			public TrieMatch? Match;

			// The pattern that ends at this state (if any).
			public string? Pattern;

			// The longest pattern that is a suffix of the string represented by this state (if any).
			public string? Output;

			// The length of the string represented by this state.
			public readonly int Depth;

			public TrieState (TrieState? fail, int depth)
			{
				Depth = depth;
				Fail = fail;
			}
		}

		class TrieMatch {
			public readonly TrieMatch? Next;
			public readonly TrieState State;
			public readonly char Value;

			public TrieMatch (char value, TrieMatch? next, TrieState state)
			{
				Value = value;
				Next = next;
				State = state;
			}
		}

		readonly List<TrieState?> failStates;
		readonly TrieState root;
		readonly bool icase;

		/// <summary>
		/// Initialize a new instance of the <see cref="Trie"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="Trie"/>.
		/// </remarks>
		/// <param name="ignoreCase"><see langword="true" /> if searching should ignore the case of ASCII letters; otherwise, <see langword="false" />.</param>
		public Trie (bool ignoreCase)
		{
			failStates = new List<TrieState?> ();
			root = new TrieState (null, 0);
			icase = ignoreCase;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="Trie"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="Trie"/>.
		/// </remarks>
		public Trie () : this (false)
		{
		}

		static void ValidateArguments (char[] text, int startIndex, int count)
		{
			if (text is null)
				throw new ArgumentNullException (nameof (text));

			if (startIndex < 0 || startIndex > text.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (count < 0 || count > (text.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (count));
		}

		static TrieMatch? FindMatch (TrieState state, char value)
		{
			var match = state.Match;

			while (match != null && match.Value != value)
				match = match.Next;

			return match;
		}

		// Note: Only ASCII letters are folded. Culture-sensitive case conversion would make the matching depend on the current
		// culture (e.g. 'I' does not lowercase to 'i' in Turkish) and could allow non-ASCII characters to match ASCII patterns.
		char Fold (char c)
		{
			return icase && c >= 'A' && c <= 'Z' ? (char) (c + 32) : c;
		}

		TrieState Insert (TrieState state, int depth, char value)
		{
			var inserted = new TrieState (root, depth + 1);
			var match = new TrieMatch (value, state.Match, inserted);
			state.Match = match;

			if (failStates.Count < depth + 1)
				failStates.Add (null);

			inserted.Next = failStates[depth];
			failStates[depth] = inserted;

			return inserted;
		}


		//
		// final = empty set
		// FOR p = 1 TO #pat
		//   q = root
		//   FOR j = 1 TO m[p]
		//     IF g(q, pat[p][j]) is null
		//       insert(q, pat[p][j])
		//     ENDIF
		//     q = g(q, pat[p][j])
		//   ENDFOR
		//   final = union(final, q)
		// ENDFOR
		//

		/// <summary>
		/// Add a search pattern.
		/// </summary>
		/// <remarks>
		/// Adds the specified search pattern.
		/// </remarks>
		/// <param name="pattern">The search pattern.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="pattern"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="pattern"/> cannot be an empty string.
		/// </exception>
		public void Add (string pattern)
		{
			TrieState? state = root;
			TrieMatch? match;
			int depth = 0;
			char c;

			if (pattern is null)
				throw new ArgumentNullException (nameof (pattern));

			if (pattern.Length == 0)
				throw new ArgumentException ("The pattern cannot be empty.", nameof (pattern));

			// Step 1: Add the pattern to the trie
			for (int i = 0; i < pattern.Length; i++) {
				c = Fold (pattern[i]);
				match = FindMatch (state, c);
				if (match is null)
					state = Insert (state, depth, c);
				else
					state = match.State;

				depth++;
			}

			state.Pattern = pattern;

			// Step 2: Compute the failure graph and the output of each state. The states are visited breadth-first so that
			// the failure state of each state (which is always shallower) has already been computed by the time it is needed.
			for (int i = 0; i < failStates.Count; i++) {
				for (state = failStates[i]; state != null; state = state.Next) {
					// Note: Fail is never null for non-root states.
					state.Output = state.Pattern ?? state.Fail!.Output;

					for (match = state.Match; match != null; match = match.Next) {
						TrieState? failState = state.Fail;
						TrieMatch? nextMatch = null;

						c = match.Value;

						while (failState != null && (nextMatch = FindMatch (failState, c)) is null)
							failState = failState.Fail;

						// Note: nextMatch is not null when the while loop exits with failState != null
						match.State.Fail = failState != null ? nextMatch!.State : root;
					}
				}
			}
		}

		//
		// Aho-Corasick (leftmost-longest)
		//
		// Each state represents a prefix of one or more patterns and its failure state represents the longest proper
		// suffix of that prefix that is also a prefix of a pattern. The output of a state is the longest pattern that
		// is a suffix of the state's prefix.
		//
		// Once a match has been found, the search continues only as long as a longer match starting at the same
		// offset (or a match starting at an earlier offset) is still possible.
		//

		/// <summary>
		/// Search the text for any of the patterns added to the trie.
		/// </summary>
		/// <remarks>
		/// Searches the text for the left-most occurrence of any of the patterns added to the trie. If more
		/// than one pattern matches at that offset, the longest pattern is returned.
		/// </remarks>
		/// <returns>The first index of a matched pattern if successful; otherwise, <c>-1</c>.</returns>
		/// <param name="text">The text to search.</param>
		/// <param name="startIndex">The starting index of the text.</param>
		/// <param name="count">The number of characters to search, starting at <paramref name="startIndex"/>.</param>
		/// <param name="pattern">The pattern that was matched.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="count"/> do not specify
		/// a valid range in the <paramref name="text"/> string.
		/// </exception>
		public int Search (char[] text, int startIndex, int count, out string? pattern)
		{
			ValidateArguments (text, startIndex, count);

			int endIndex = startIndex + count;
			TrieState state = root;
			int offset = -1;

			pattern = null;

			for (int i = startIndex; i < endIndex; i++) {
				char c = Fold (text[i]);
				TrieMatch? match;

				// Note: Fail is never null for non-root states.
				while ((match = FindMatch (state, c)) is null && state != root)
					state = state.Fail!;

				state = match?.State ?? root;

				// If the current state's prefix starts after the offset of the match that we've already found, then
				// no longer (or earlier) match is possible.
				if (pattern != null && i + 1 - state.Depth > offset)
					break;

				if (state.Output != null) {
					int index = i + 1 - state.Output.Length;

					if (pattern is null || index < offset || (index == offset && state.Output.Length > pattern.Length)) {
						pattern = state.Output;
						offset = index;
					}
				}
			}

			return offset;
		}

		/// <summary>
		/// Search the text for any of the patterns added to the trie.
		/// </summary>
		/// <remarks>
		/// Searches the text for any of the patterns added to the trie.
		/// </remarks>
		/// <returns>The first index of a matched pattern if successful; otherwise, <c>-1</c>.</returns>
		/// <param name="text">The text to search.</param>
		/// <param name="startIndex">The starting index of the text.</param>
		/// <param name="pattern">The pattern that was matched.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> is out of range.
		/// </exception>
		public int Search (char[] text, int startIndex, out string? pattern)
		{
			if (text is null)
				throw new ArgumentNullException (nameof (text));

			return Search (text, startIndex, text.Length - startIndex, out pattern);
		}

		/// <summary>
		/// Search the text for any of the patterns added to the trie.
		/// </summary>
		/// <remarks>
		/// Searches the text for any of the patterns added to the trie.
		/// </remarks>
		/// <returns>The first index of a matched pattern if successful; otherwise, <c>-1</c>.</returns>
		/// <param name="text">The text to search.</param>
		/// <param name="pattern">The pattern that was matched.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <see langword="null"/>.
		/// </exception>
		public int Search (char[] text, out string? pattern)
		{
			if (text is null)
				throw new ArgumentNullException (nameof (text));

			return Search (text, 0, text.Length, out pattern);
		}
	}
}

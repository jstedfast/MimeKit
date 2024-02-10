//
// Trie.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2024 .NET Foundation and Contributors
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
			public TrieState Next;
			public TrieState Fail;
			public TrieMatch Match;
			public string Pattern;
			public int Depth;

			public TrieState (TrieState fail)
			{
				Fail = fail;
			}
		}

		class TrieMatch {
			public TrieMatch Next;
			public TrieState State;

			public char Value { get; private set; }

			public TrieMatch (char value)
			{
				Value = value;
			}
		}

		readonly List<TrieState> failStates;
		readonly TrieState root;
		readonly bool icase;

		/// <summary>
		/// Initialize a new instance of the <see cref="Trie"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="Trie"/>.
		/// </remarks>
		/// <param name="ignoreCase"><c>true</c> if searching should ignore case; otherwise, <c>false</c>.</param>
		public Trie (bool ignoreCase)
		{
			failStates = new List<TrieState> ();
			root = new TrieState (null);
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

		static TrieMatch FindMatch (TrieState state, char value)
		{
			var match = state.Match;

			while (match != null && match.Value != value)
				match = match.Next;

			return match;
		}

		TrieState Insert (TrieState state, int depth, char value)
		{
			var inserted = new TrieState (root);
			var match = new TrieMatch (value) {
				Next = state.Match,
				State = inserted
			};

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
		/// <paramref name="pattern"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="pattern"/> cannot be an empty string.
		/// </exception>
		public void Add (string pattern)
		{
			TrieState state = root;
			TrieMatch match;
			int depth = 0;
			char c;

			if (pattern is null)
				throw new ArgumentNullException (nameof (pattern));

			if (pattern.Length == 0)
				throw new ArgumentException ("The pattern cannot be empty.", nameof (pattern));

			// Step 1: Add the pattern to the trie
			for (int i = 0; i < pattern.Length; i++) {
				c = icase ? char.ToLower (pattern[i]) : pattern[i];
				match = FindMatch (state, c);
				if (match is null)
					state = Insert (state, depth, c);
				else
					state = match.State;

				depth++;
			}

			state.Pattern = pattern;
			state.Depth = depth;

			// Step 2: Compute the failure graph
			for (int i = 0; i < failStates.Count; i++) {
				state = failStates[i];

				while (state != null) {
					match = state.Match;
					while (match != null) {
						TrieState matchedState = match.State;
						TrieState failState = state.Fail;
						TrieMatch nextMatch = null;

						c = match.Value;

						while (failState != null && (nextMatch = FindMatch (failState, c)) is null)
							failState = failState.Fail;

						if (failState != null) {
							matchedState.Fail = nextMatch.State;
							if (matchedState.Fail.Depth > matchedState.Depth)
								matchedState.Depth = matchedState.Fail.Depth;
						} else {
							if ((nextMatch = FindMatch (root, c)) != null)
								matchedState.Fail = nextMatch.State;
							else
								matchedState.Fail = root;
						}

						match = match.Next;
					}

					state = state.Next;
				}
			}
		}

		//
		// Aho-Corasick
		//
		// q = root
		// FOR i = 1 TO n
		//   WHILE q != fail AND g(q, text[i]) == fail
		//     q = h(q)
		//   ENDWHILE
		//   IF q == fail
		//     q = root
		//   ELSE
		//     q = g(q, text[i])
		//   ENDIF
		//   IF isElement(q, final)
		//     RETURN TRUE
		//   ENDIF
		// ENDFOR
		// RETURN FALSE
		//

		/// <summary>
		/// Search the text for any of the patterns added to the trie.
		/// </summary>
		/// <remarks>
		/// Searches the text for any of the patterns added to the trie.
		/// </remarks>
		/// <returns>The first index of a matched pattern if successful; otherwise, <c>-1</c>.</returns>
		/// <param name="text">The text to search.</param>
		/// <param name="startIndex">The starting index of the text.</param>
		/// <param name="count">The number of characters to search, starting at <paramref name="startIndex"/>.</param>
		/// <param name="pattern">The pattern that was matched.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="count"/> do not specify
		/// a valid range in the <paramref name="text"/> string.
		/// </exception>
		public int Search (char[] text, int startIndex, int count, out string pattern)
		{
			ValidateArguments (text, startIndex, count);

			int endIndex = Math.Min (text.Length, startIndex + count);
			TrieState state = root;
			TrieMatch match = null;
			int matched = 0;
			int offset = -1;
			char c;

			pattern = null;

			for (int i = startIndex; i < endIndex; i++) {
				c = icase ? char.ToLower (text[i]) : text[i];

				while (state != null && (match = FindMatch (state, c)) is null && matched == 0)
					state = state.Fail;

				if (state == root) {
					if (matched > 0)
						return offset;

					offset = i;
				}

				if (state is null) {
					if (matched > 0)
						return offset;

					state = root;
					offset = i;
				} else if (match != null) {
					state = match.State;

					if (state.Depth > matched) {
						pattern = state.Pattern;
						matched = state.Depth;
					}
				} else if (matched > 0) {
					return offset;
				}
			}

			return matched > 0 ? offset : -1;
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
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> is out of range.
		/// </exception>
		public int Search (char[] text, int startIndex, out string pattern)
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
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public int Search (char[] text, out string pattern)
		{
			if (text is null)
				throw new ArgumentNullException (nameof (text));

			return Search (text, 0, text.Length, out pattern);
		}
	}
}

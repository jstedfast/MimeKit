﻿//
// MimeIterator.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2025 .NET Foundation and Contributors
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
using System.Collections;
using System.Globalization;
using System.Collections.Generic;

using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// An iterator for a MIME tree structure.
	/// </summary>
	/// <remarks>
	/// Walks the MIME tree structure of a <see cref="MimeMessage"/> in depth-first order.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\MimeIterator.cs" />
	/// </example>
	public class MimeIterator : IEnumerator<MimeEntity>
	{
		class MimeNode
		{
			public readonly MimeEntity Entity;
			public readonly bool Indexed;

			public MimeNode (MimeEntity entity, bool indexed)
			{
				Entity = entity;
				Indexed = indexed;
			}
		}

		readonly Stack<MimeNode> stack = new Stack<MimeNode> ();
		readonly List<int> path = new List<int> ();
		bool moveFirst = true;
		MimeEntity? current;
		int index = -1;

		/// <summary>
		/// Initialize a new instance of the <see cref="MimeIterator"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimeIterator"/> for the specified message.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeIterator.cs" />
		/// </example>
		/// <param name="message">The message.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="message"/> is <see langword="null"/>.
		/// </exception>
		public MimeIterator (MimeMessage message)
		{
			if (message is null)
				throw new ArgumentNullException (nameof (message));

			Message = message;
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before
		/// the <see cref="MimeIterator"/> is reclaimed by garbage collection.
		/// </summary>
		/// <remarks>
		/// Releases unmanaged resources and performs other cleanup operations before
		/// the <see cref="MimeIterator"/> is reclaimed by garbage collection.
		/// </remarks>
		~MimeIterator ()
		{
			Dispose (false);
		}

		/// <summary>
		/// Get the top-level message.
		/// </summary>
		/// <remarks>
		/// Gets the top-level message.
		/// </remarks>
		/// <value>The message.</value>
		public MimeMessage Message {
			get; private set;
		}

		/// <summary>
		/// Get the parent of the current entity.
		/// </summary>
		/// <remarks>
		/// <para>After an iterator is created or after the <see cref="Reset()"/> method is called,
		/// the <see cref="MoveNext()"/> method must be called to advance the iterator to the
		/// first entity of the message before reading the value of the Parent property;
		/// otherwise, Parent throws a <see cref="System.InvalidOperationException"/>. Parent
		/// also throws a <see cref="System.InvalidOperationException"/> if the last call to
		/// <see cref="MoveNext()"/> returned false, which indicates the end of the message.</para>
		/// <para>If the current entity is the top-level entity of the message, then the parent
		/// will be <see langword="null"/>; otherwise the parent will be either be a
		/// <see cref="MessagePart"/> or a <see cref="Multipart"/>.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeIterator.cs" />
		/// </example>
		/// <value>The parent entity.</value>
		/// <exception cref="System.InvalidOperationException">
		/// Either <see cref="MoveNext()"/> has not been called or <see cref="MoveNext()"/>
		/// has moved beyond the end of the message.
		/// </exception>
		public MimeEntity? Parent {
			get {
				if (current is null)
					throw new InvalidOperationException ();

				return stack.Count > 0 ? stack.Peek ().Entity : null;
			}
		}

		/// <summary>
		/// Get the current entity.
		/// </summary>
		/// <remarks>
		/// After an iterator is created or after the <see cref="Reset()"/> method is called,
		/// the <see cref="MoveNext()"/> method must be called to advance the iterator to the
		/// first entity of the message before reading the value of the Current property;
		/// otherwise, Current throws a <see cref="System.InvalidOperationException"/>. Current
		/// also throws a <see cref="System.InvalidOperationException"/> if the last call to
		/// <see cref="MoveNext()"/> returned false, which indicates the end of the message.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeIterator.cs" />
		/// </example>
		/// <value>The current entity.</value>
		/// <exception cref="System.InvalidOperationException">
		/// Either <see cref="MoveNext()"/> has not been called or <see cref="MoveNext()"/>
		/// has moved beyond the end of the message.
		/// </exception>
		public MimeEntity Current {
			get {
				if (current is null)
					throw new InvalidOperationException ();

				return current;
			}
		}

		/// <summary>
		/// Get the current entity.
		/// </summary>
		/// <remarks>
		/// After an iterator is created or after the <see cref="Reset()"/> method is called,
		/// the <see cref="MoveNext()"/> method must be called to advance the iterator to the
		/// first entity of the message before reading the value of the Current property;
		/// otherwise, Current throws a <see cref="System.InvalidOperationException"/>. Current
		/// also throws a <see cref="System.InvalidOperationException"/> if the last call to
		/// <see cref="MoveNext()"/> returned false, which indicates the end of the message.
		/// </remarks>
		/// <value>The current entity.</value>
		/// <exception cref="System.InvalidOperationException">
		/// Either <see cref="MoveNext()"/> has not been called or <see cref="MoveNext()"/>
		/// has moved beyond the end of the message.
		/// </exception>
		object IEnumerator.Current {
			get { return Current; }
		}

		/// <summary>
		/// Get the path specifier for the current entity.
		/// </summary>
		/// <remarks>
		/// After an iterator is created or after the <see cref="Reset()"/> method is called,
		/// the <see cref="MoveNext()"/> method must be called to advance the iterator to the
		/// first entity of the message before reading the value of the PathSpecifier property;
		/// otherwise, PathSpecifier throws a <see cref="System.InvalidOperationException"/>.
		/// PathSpecifier also throws a <see cref="System.InvalidOperationException"/> if the
		/// last call to <see cref="MoveNext()"/> returned false, which indicates the end of
		/// the message.
		/// </remarks>
		/// <value>The path specifier.</value>
		/// <exception cref="System.InvalidOperationException">
		/// Either <see cref="MoveNext()"/> has not been called or <see cref="MoveNext()"/>
		/// has moved beyond the end of the message.
		/// </exception>
		public string PathSpecifier {
			get {
				if (current is null)
					throw new InvalidOperationException ();

				var specifier = new ValueStringBuilder(128);

				for (int i = 0; i < path.Count; i++) {
					specifier.AppendInvariant (path[i] + 1);
					specifier.Append ('.');
				}

				specifier.AppendInvariant (index + 1);

				return specifier.ToString ();
			}
		}

		/// <summary>
		/// Get the depth of the current entity.
		/// </summary>
		/// <remarks>
		/// After an iterator is created or after the <see cref="Reset()"/> method is called,
		/// the <see cref="MoveNext()"/> method must be called to advance the iterator to the
		/// first entity of the message before reading the value of the Depth property;
		/// otherwise, Depth throws a <see cref="System.InvalidOperationException"/>. Depth
		/// also throws a <see cref="System.InvalidOperationException"/> if the last call to
		/// <see cref="MoveNext()"/> returned false, which indicates the end of the message.
		/// </remarks>
		/// <value>The depth.</value>
		/// <exception cref="System.InvalidOperationException">
		/// Either <see cref="MoveNext()"/> has not been called or <see cref="MoveNext()"/>
		/// has moved beyond the end of the message.
		/// </exception>
		public int Depth {
			get {
				if (current is null)
					throw new InvalidOperationException ();

				return stack.Count;
			}
		}

		void Push (MimeEntity entity)
		{
			if (index != -1)
				path.Add (index);

			stack.Push (new MimeNode (entity, index != -1));
		}

		bool Pop ()
		{
			if (stack.Count == 0)
				return false;

			var node = stack.Pop ();

			if (node.Indexed) {
				index = path[path.Count - 1];
				path.RemoveAt (path.Count - 1);
			}

			current = node.Entity;

			return true;
		}

		/// <summary>
		/// Advance the iterator to the next depth-first entity of the tree structure.
		/// </summary>
		/// <remarks>
		/// After an iterator is created or after the <see cref="Reset()"/> method is called,
		/// an iterator is positioned before the first entity of the message, and the first
		/// call to the MoveNext method moves the iterator to the first entity of the message.
		/// If MoveNext advances beyond the last entity of the message, MoveNext returns false.
		/// When the iterator is at this position, subsequent calls to MoveNext also return
		/// false until <see cref="Reset()"/> is called.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeIterator.cs" />
		/// </example>
		/// <returns><see langword="true" /> if the iterator was successfully advanced to the next entity; otherwise, <see langword="false" />.</returns>
		public bool MoveNext ()
		{
			if (moveFirst) {
				current = Message.Body;
				moveFirst = false;

				return current != null;
			}

			var multipart = current as Multipart;

			if (current is MessagePart rfc822) {
				current = rfc822.Message?.Body;

				if (current != null) {
					Push (rfc822);
					index = current is Multipart ? -1 : 0;
					return true;
				}
			}

			if (multipart != null) {
				if (multipart.Count > 0) {
					Push (current);
					current = multipart[0];
					index = 0;
					return true;
				}
			}

			// find the next sibling
			while (stack.Count > 0) {
				multipart = stack.Peek ().Entity as Multipart;

				if (multipart != null) {
					// advance to the next part in the multipart...
					if (multipart.Count > ++index) {
						current = multipart[index];
						return true;
					}
				}

				if (!Pop ())
					break;
			}

			current = null;
			index = -1;

			return false;
		}

		static int[] Parse (string pathSpecifier)
		{
			var path = pathSpecifier.Split ('.');
			var indexes = new int[path.Length];

			for (int i = 0; i < path.Length; i++) {
				if (!int.TryParse (path[i], NumberStyles.None, CultureInfo.InvariantCulture, out int index) || index < 0)
					throw new FormatException ("Invalid path specifier format.");

				indexes[i] = index - 1;
			}

			return indexes;
		}

		/// <summary>
		/// Advance to the entity specified by the path specifier.
		/// </summary>
		/// <remarks>
		/// <para>Advances the iterator to the entity specified by the path specifier which
		/// must be in the same format as returned by <see cref="PathSpecifier"/>.</para>
		/// <para>If the iterator has already advanced beyond the entity at the specified
		/// path, the iterator will <see cref="Reset()"/> and advance as normal.</para>
		/// </remarks>
		/// <returns><see langword="true" /> if advancing to the specified entity was successful; otherwise, <see langword="false" />.</returns>
		/// <param name="pathSpecifier">The path specifier.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="pathSpecifier"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="pathSpecifier"/> is empty.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// <paramref name="pathSpecifier"/> is in an invalid format.
		/// </exception>
		public bool MoveTo (string pathSpecifier)
		{
			if (pathSpecifier is null)
				throw new ArgumentNullException (nameof (pathSpecifier));

			if (pathSpecifier.Length == 0)
				throw new ArgumentException ("The path specifier cannot be empty.", nameof (pathSpecifier));

			var indexes = Parse (pathSpecifier);
			int i;

			// OPTIMIZATION: only reset the iterator if we are jumping to a previous part
			for (i = 0; i < Math.Min (indexes.Length, path.Count); i++) {
				if (indexes[i] < path[i]) {
					Reset ();
					break;
				}
			}

			if (!moveFirst && indexes.Length < path.Count)
				Reset ();

			if (moveFirst && !MoveNext ())
				return false;

			do {
				if (path.Count + 1 == indexes.Length) {
					for (i = 0; i < path.Count; i++) {
						if (indexes[i] != path[i])
							break;
					}

					if (i == path.Count && indexes[i] == index)
						return true;
				}
			} while (MoveNext ());

			return false;
		}

		/// <summary>
		/// Reset the iterator to its initial state.
		/// </summary>
		/// <remarks>
		/// Resets the iterator to its initial state.
		/// </remarks>
		public void Reset ()
		{
			moveFirst = true;
			current = null;
			stack.Clear ();
			path.Clear ();
			index = -1;
		}

		/// <summary>
		/// Release the unmanaged resources used by the <see cref="MimeIterator"/> and
		/// optionally releases the managed resources.
		/// </summary>
		/// <remarks>
		/// Releases the unmanaged resources used by the <see cref="MimeIterator"/> and
		/// optionally releases the managed resources.
		/// </remarks>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources;
		/// <see langword="false" /> to release only the unmanaged resources.</param>
		protected virtual void Dispose (bool disposing)
		{
		}

		/// <summary>
		/// Release all resources used by the <see cref="MimeIterator"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose()"/> when you are finished using the <see cref="MimeIterator"/>. The
		/// <see cref="Dispose()"/> method leaves the <see cref="MimeIterator"/> in an unusable state. After
		/// calling <see cref="Dispose()"/>, you must release all references to the <see cref="MimeIterator"/> so
		/// the garbage collector can reclaim the memory that the <see cref="MimeIterator"/> was occupying.</remarks>
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}
	}
}

//
// MimeIterator.cs
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
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace MimeKit {
	/// <summary>
	/// An iterator for a MIME tree structure.
	/// </summary>
	/// <remarks>
	/// Walks the MIME tree structure of a <see cref="MimeMessage"/> in depth-first order.
	/// </remarks>
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
		MimeEntity current;
		int index = -1;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MimeIterator"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimeIterator"/> for the specified message.
		/// </remarks>
		/// <param name="message">The message.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="message"/> is <c>null</c>.
		/// </exception>
		public MimeIterator (MimeMessage message)
		{
			if (message == null)
				throw new ArgumentNullException ("message");

			Message = message;
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before
		/// the <see cref="MimeKit.MimeIterator"/> is reclaimed by garbage collection.
		/// </summary>
		/// <remarks>
		/// Releases unmanaged resources and performs other cleanup operations before
		/// the <see cref="MimeKit.MimeIterator"/> is reclaimed by garbage collection.
		/// </remarks>
		~MimeIterator ()
		{
			Dispose (false);
		}

		/// <summary>
		/// Gets the top-level message.
		/// </summary>
		/// <remarks>
		/// Gets the top-level message.
		/// </remarks>
		/// <value>The message.</value>
		public MimeMessage Message {
			get; private set;
		}

		/// <summary>
		/// Gets the parent of the current entity.
		/// </summary>
		/// <remarks>
		/// <para>After an iterator is created or after the <see cref="Reset()"/> method is called,
		/// the <see cref="MoveNext()"/> method must be called to advance the iterator to the
		/// first entity of the message before reading the value of the Parent property;
		/// otherwise, Parent throws a <see cref="System.InvalidOperationException"/>. Parent
		/// also throws a <see cref="System.InvalidOperationException"/> if the last call to
		/// <see cref="MoveNext()"/> returned false, which indicates the end of the message.</para>
		/// <para>If the current entity is the top-level entity of the message, then the parent
		/// will be <c>null</c>; otherwise the parent will be either be a
		/// <see cref="MessagePart"/> or a <see cref="Multipart"/>.</para>
		/// </remarks>
		/// <value>The parent entity.</value>
		/// <exception cref="System.InvalidOperationException">
		/// Either <see cref="MoveNext()"/> has not been called or <see cref="MoveNext()"/>
		/// has moved beyond the end of the message.
		/// </exception>
		public MimeEntity Parent {
			get {
				if (current == null)
					throw new InvalidOperationException ();

				return stack.Count > 0 ? stack.Peek ().Entity : null;
			}
		}

		/// <summary>
		/// Gets the current entity.
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
		public MimeEntity Current {
			get {
				if (current == null)
					throw new InvalidOperationException ();

				return current;
			}
		}

		/// <summary>
		/// Gets the current entity.
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
		/// Gets the path specifier for the current entity.
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
				if (current == null)
					throw new InvalidOperationException ();

				var specifier = new StringBuilder ();

				for (int i = 0; i < path.Count; i++)
					specifier.AppendFormat ("{0}.", path[i] + 1);

				specifier.AppendFormat ("{0}", index + 1);

				return specifier.ToString ();
			}
		}

		/// <summary>
		/// Gets the depth of the current entity.
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
				if (current == null)
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
		/// Advances the iterator to the next depth-first entity of the tree structure.
		/// </summary>
		/// <remarks>
		/// After an iterator is created or after the <see cref="Reset()"/> method is called,
		/// an iterator is positioned before the first entity of the message, and the first
		/// call to the MoveNext method moves the iterator to the first entity of the message.
		/// If MoveNext advances beyond the last entity of the message, MoveNext returns false.
		/// When the iterator is at this position, subsequent calls to MoveNext also return
		/// false until <see cref="Reset()"/> is called.
		/// </remarks>
		/// <returns><c>true</c> if the iterator was successfully advanced to the next entity; otherwise, <c>false</c>.</returns>
		public bool MoveNext ()
		{
			if (moveFirst) {
				current = Message.Body;
				moveFirst = false;

				return current != null;
			}

			var message_part = current as MessagePart;
			var multipart = current as Multipart;

			if (message_part != null) {
				current = message_part.Message != null ? message_part.Message.Body : null;

				if (current != null) {
					Push (message_part);
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
			int index;

			for (int i = 0; i < path.Length; i++) {
				if (!int.TryParse (path[i], out index) || index < 0)
					throw new FormatException ("Invalid path specifier format.");

				indexes[i] = index - 1;
			}

			return indexes;
		}

		/// <summary>
		/// Advances to the entity specified by the path specifier.
		/// </summary>
		/// <remarks>
		/// <para>Advances the iterator to the entity specified by the path specifier which
		/// must be in the same format as returned by <see cref="PathSpecifier"/>.</para>
		/// <para>If the iterator has already advanced beyond the entity at the specified
		/// path, the iterator will <see cref="Reset()"/> and advance as normal.</para>
		/// </remarks>
		/// <returns><c>true</c> if advancing to the specified entity was successful; otherwise, <c>false</c>.</returns>
		/// <param name="pathSpecifier">The path specifier.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="pathSpecifier"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="pathSpecifier"/> is empty.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// <paramref name="pathSpecifier"/> is in an invalid format.
		/// </exception>
		public bool MoveTo (string pathSpecifier)
		{
			if (pathSpecifier == null)
				throw new ArgumentNullException ("pathSpecifier");

			if (pathSpecifier.Length == 0)
				throw new ArgumentException ("The path specifier cannot be empty.", "pathSpecifier");

			var indexes = Parse (pathSpecifier);
			int i;

			// OPTIMIZATION: only reset the iterator if we are jumping to a previous part
			for (i = 0; i < Math.Min (indexes.Length, path.Count); i++) {
				if (indexes[i] < path[i]) {
					Reset ();
					break;
				}
			}

			if (i == path.Count && indexes[i] < index)
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
		/// Resets the iterator to its initial state.
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
		/// Releases the unmanaged resources used by the <see cref="MimeKit.MimeIterator"/> and
		/// optionally releases the managed resources.
		/// </summary>
		/// <remarks>
		/// Releases the unmanaged resources used by the <see cref="MimeKit.MimeIterator"/> and
		/// optionally releases the managed resources.
		/// </remarks>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
		/// <c>false</c> to release only the unmanaged resources.</param>
		protected virtual void Dispose (bool disposing)
		{
		}

		/// <summary>
		/// Releases all resources used by the <see cref="MimeKit.MimeIterator"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose()"/> when you are finished using the <see cref="MimeKit.MimeIterator"/>. The
		/// <see cref="Dispose()"/> method leaves the <see cref="MimeKit.MimeIterator"/> in an unusable state. After
		/// calling <see cref="Dispose()"/>, you must release all references to the <see cref="MimeKit.MimeIterator"/> so
		/// the garbage collector can reclaim the memory that the <see cref="MimeKit.MimeIterator"/> was occupying.</remarks>
		public void Dispose ()
		{
			Dispose (true);
		}
	}
}

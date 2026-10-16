//
// MimeIterator.cs
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
		// Each frame represents one level of the path from the message body to the current entity.
		// Index is the 0-based index of the entity within its parent Multipart. Message bodies
		// (the top-level body and the bodies of MessageParts) have an Index of 0 unless they are
		// multiparts, in which case their Index is -1 (they have no IMAP part number of their own).
		readonly struct Frame
		{
			public readonly MimeEntity Entity;
			public readonly int Index;

			public Frame (MimeEntity entity, int index)
			{
				Entity = entity;
				Index = index;
			}
		}

		readonly List<Frame> frames = new List<Frame> ();
		bool moveFirst = true;
		bool skipChildren;

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
				if (frames.Count == 0)
					throw new InvalidOperationException ();

				return frames.Count > 1 ? frames[frames.Count - 2].Entity : null;
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
				if (frames.Count == 0)
					throw new InvalidOperationException ();

				return frames[frames.Count - 1].Entity;
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
		/// <para>After an iterator is created or after the <see cref="Reset()"/> method is called,
		/// the <see cref="MoveNext()"/> method must be called to advance the iterator to the
		/// first entity of the message before reading the value of the PathSpecifier property;
		/// otherwise, PathSpecifier throws a <see cref="System.InvalidOperationException"/>.
		/// PathSpecifier also throws a <see cref="System.InvalidOperationException"/> if the
		/// last call to <see cref="MoveNext()"/> returned false, which indicates the end of
		/// the message.</para>
		/// <para>Path specifiers follow the IMAP part specifier conventions defined in
		/// <a href="https://tools.ietf.org/html/rfc3501#section-6.4.5">rfc3501</a>: the children
		/// of a multipart are numbered starting at 1 and a non-multipart message body (including
		/// the top-level body of the message) is part 1 of its message. Since IMAP does not
		/// assign a part number to a multipart that is the body of a message, MimeKit uses
		/// <c>0</c> for such multiparts (e.g. <c>"0"</c> for the top-level multipart and
		/// <c>"3.0"</c> for the multipart body of the message contained within part 3).</para>
		/// </remarks>
		/// <value>The path specifier.</value>
		/// <exception cref="System.InvalidOperationException">
		/// Either <see cref="MoveNext()"/> has not been called or <see cref="MoveNext()"/>
		/// has moved beyond the end of the message.
		/// </exception>
		public string PathSpecifier {
			get {
				if (frames.Count == 0)
					throw new InvalidOperationException ();

				using var specifier = new ValueStringBuilder (128);
				int last = frames.Count - 1;

				for (int i = 0; i < last; i++) {
					// Multiparts that are message bodies have no part number of their own.
					if (frames[i].Index == -1)
						continue;

					specifier.AppendInvariant (frames[i].Index + 1);
					specifier.Append ('.');
				}

				specifier.AppendInvariant (frames[last].Index + 1);

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
				if (frames.Count == 0)
					throw new InvalidOperationException ();

				return frames.Count - 1;
			}
		}

		/// <summary>
		/// Get the index of the current entity within its parent multipart.
		/// </summary>
		/// <remarks>
		/// <para>Gets the index of the current entity within its parent <see cref="Multipart"/>, which can be
		/// used to replace or remove the entity (e.g. <c>((Multipart) iter.Parent).RemoveAt (iter.Index)</c>).</para>
		/// <para>If the current entity is not the child of a <see cref="Multipart"/> (that is, if it is the body of
		/// the top-level message or the body of an attached message), then the index will be <c>-1</c>.</para>
		/// </remarks>
		/// <value>The index of the current entity within its parent multipart or <c>-1</c> if the parent is not a multipart.</value>
		/// <exception cref="System.InvalidOperationException">
		/// Either <see cref="MoveNext()"/> has not been called or <see cref="MoveNext()"/>
		/// has moved beyond the end of the message.
		/// </exception>
		public int Index {
			get {
				if (frames.Count == 0)
					throw new InvalidOperationException ();

				if (frames.Count > 1 && frames[frames.Count - 2].Entity is Multipart)
					return frames[frames.Count - 1].Index;

				return -1;
			}
		}

		/// <summary>
		/// Get the message that contains the current entity.
		/// </summary>
		/// <remarks>
		/// Gets the message that contains the current entity. This is either the top-level <see cref="Message"/>
		/// or, if the current entity is a descendant of a <see cref="MessagePart"/>, the
		/// <see cref="MessagePart.Message"/> of the nearest such ancestor.
		/// </remarks>
		/// <value>The message that contains the current entity.</value>
		/// <exception cref="System.InvalidOperationException">
		/// Either <see cref="MoveNext()"/> has not been called or <see cref="MoveNext()"/>
		/// has moved beyond the end of the message.
		/// </exception>
		public MimeMessage CurrentMessage {
			get {
				if (frames.Count == 0)
					throw new InvalidOperationException ();

				for (int i = frames.Count - 2; i >= 0; i--) {
					if (frames[i].Entity is MessagePart rfc822 && rfc822.Message != null)
						return rfc822.Message;
				}

				return Message;
			}
		}

		/// <summary>
		/// Get an ancestor of the current entity.
		/// </summary>
		/// <remarks>
		/// <para>Gets the ancestor of the current entity that is the specified number of generations above it:
		/// a <paramref name="generation"/> of <c>1</c> gets the <see cref="Parent"/>, <c>2</c> gets the parent
		/// of the parent, and <see cref="Depth"/> gets the body of the top-level message.</para>
		/// <para>This can be used, for example, to determine whether the current entity is contained within a
		/// <see cref="MultipartAlternative"/>.</para>
		/// </remarks>
		/// <returns>The ancestor.</returns>
		/// <param name="generation">The number of generations above the current entity, between <c>1</c> and <see cref="Depth"/>.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="generation"/> is less than <c>1</c> or greater than <see cref="Depth"/>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// Either <see cref="MoveNext()"/> has not been called or <see cref="MoveNext()"/>
		/// has moved beyond the end of the message.
		/// </exception>
		public MimeEntity GetAncestor (int generation)
		{
			if (frames.Count == 0)
				throw new InvalidOperationException ();

			if (generation < 1 || generation >= frames.Count)
				throw new ArgumentOutOfRangeException (nameof (generation));

			return frames[frames.Count - 1 - generation].Entity;
		}

		/// <summary>
		/// Get the enumerator.
		/// </summary>
		/// <remarks>
		/// <para>Returns this iterator so that it can be used in a <c>foreach</c> statement, which will
		/// enumerate the entities that follow the current position (or all of the entities in the
		/// message if <see cref="MoveNext()"/> has not yet been called).</para>
		/// <para>Since the iterator itself is used, members such as <see cref="SkipChildren()"/>,
		/// <see cref="PathSpecifier"/> and <see cref="Depth"/> can be used within the loop body.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#">
		/// var iter = new MimeIterator (message);
		///
		/// foreach (var entity in iter) {
		///     if (entity is MessagePart)
		///         iter.SkipChildren ();
		///
		///     Console.WriteLine ("{0}: {1}", iter.PathSpecifier, entity.ContentType.MimeType);
		/// }
		/// </code>
		/// </example>
		/// <returns>This iterator.</returns>
		public MimeIterator GetEnumerator ()
		{
			return this;
		}

		static Frame CreateMessageBodyFrame (MimeEntity body)
		{
			return new Frame (body, body is Multipart ? -1 : 0);
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
				moveFirst = false;

				if (Message.Body is null)
					return false;

				frames.Add (CreateMessageBodyFrame (Message.Body));

				return true;
			}

			if (frames.Count == 0)
				return false;

			var current = frames[frames.Count - 1].Entity;

			if (skipChildren) {
				skipChildren = false;
			} else if (current is MessagePart rfc822) {
				var body = rfc822.Message?.Body;

				if (body != null) {
					frames.Add (CreateMessageBodyFrame (body));
					return true;
				}
			} else if (current is Multipart multipart && multipart.Count > 0) {
				frames.Add (new Frame (multipart[0], 0));
				return true;
			}

			// find the next sibling of the current entity or of its nearest ancestor that has one
			while (frames.Count > 1) {
				var frame = frames[frames.Count - 1];

				frames.RemoveAt (frames.Count - 1);

				if (frames[frames.Count - 1].Entity is Multipart parent && frame.Index + 1 < parent.Count) {
					int index = frame.Index + 1;

					frames.Add (new Frame (parent[index], index));
					return true;
				}
			}

			frames.Clear ();

			return false;
		}

		/// <summary>
		/// Skip the descendants of the current entity.
		/// </summary>
		/// <remarks>
		/// <para>Causes the next call to <see cref="MoveNext()"/> to skip over the children of the
		/// current entity (if it is a <see cref="Multipart"/>) or the contained message (if it is a
		/// <see cref="MessagePart"/>) and advance to the next entity that is not a descendant of
		/// the current entity.</para>
		/// <para>This has no effect if the current entity has no descendants. Calling
		/// <see cref="MoveTo(string)"/> or <see cref="Reset()"/> cancels the request.</para>
		/// </remarks>
		/// <exception cref="System.InvalidOperationException">
		/// Either <see cref="MoveNext()"/> has not been called or <see cref="MoveNext()"/>
		/// has moved beyond the end of the message.
		/// </exception>
		public void SkipChildren ()
		{
			if (frames.Count == 0)
				throw new InvalidOperationException ();

			skipChildren = true;
		}

		static int[]? TryParse (string pathSpecifier)
		{
			var path = pathSpecifier.Split ('.');
			var indexes = new int[path.Length];

			for (int i = 0; i < path.Length; i++) {
				if (!int.TryParse (path[i], NumberStyles.None, CultureInfo.InvariantCulture, out int index))
					return null;

				indexes[i] = index - 1;
			}

			return indexes;
		}

		bool TryNavigate (int[] indexes)
		{
			var body = Message.Body;

			if (body is null)
				return false;

			frames.Add (CreateMessageBodyFrame (body));

			int i = 0;

			if (body is not Multipart) {
				// A non-multipart top-level body is part 1.
				if (indexes[0] != 0)
					return false;

				i++;
			}

			while (i < indexes.Length) {
				var frame = frames[frames.Count - 1];
				int index = indexes[i];

				if (frame.Entity is Multipart multipart) {
					if (index == -1) {
						// "0" refers to a multipart that is a message body and must be the last component.
						return frame.Index == -1 && i == indexes.Length - 1;
					}

					if (index >= multipart.Count)
						return false;

					frames.Add (new Frame (multipart[index], index));
					i++;
				} else if (frame.Entity is MessagePart rfc822) {
					body = rfc822.Message?.Body;

					if (body is null)
						return false;

					if (body is Multipart) {
						// A multipart message body has no part number of its own, so the next
						// component addresses either the multipart itself ("0") or one of its children.
						frames.Add (new Frame (body, -1));
					} else {
						if (index != 0)
							return false;

						frames.Add (new Frame (body, 0));
						i++;
					}
				} else {
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Advance to the entity specified by the path specifier.
		/// </summary>
		/// <remarks>
		/// <para>Moves the iterator directly to the entity specified by the path specifier which
		/// must be in the same format as returned by <see cref="PathSpecifier"/>.</para>
		/// <para>If no entity exists at the specified path, the iterator is positioned beyond
		/// the end of the message and <see langword="false" /> is returned.</para>
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

			var indexes = TryParse (pathSpecifier) ?? throw new FormatException ("Invalid path specifier format.");

			return MoveTo (indexes);
		}

		internal bool TryMoveTo (string pathSpecifier)
		{
			var indexes = pathSpecifier.Length > 0 ? TryParse (pathSpecifier) : null;

			if (indexes is null) {
				Reset ();
				moveFirst = false;
				return false;
			}

			return MoveTo (indexes);
		}

		bool MoveTo (int[] indexes)
		{
			frames.Clear ();
			skipChildren = false;
			moveFirst = false;

			if (TryNavigate (indexes))
				return true;

			frames.Clear ();

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
			skipChildren = false;
			frames.Clear ();
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

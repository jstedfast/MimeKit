//
// MessageIdList.cs
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
using System.Text;
using System.Collections;
using System.Collections.Generic;

using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// A list of Message-Ids.
	/// </summary>
	/// <remarks>
	/// Used by the <see cref="MimeMessage.References"/> property.
	/// </remarks>
	public class MessageIdList : IList<string>
	{
		readonly List<string> references;

		/// <summary>
		/// Initialize a new instance of the <see cref="MessageIdList"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new, empty, <see cref="MessageIdList"/>.
		/// </remarks>
		public MessageIdList ()
		{
			references = new List<string> ();
		}

		/// <summary>
		/// Clone the <see cref="MessageIdList"/>.
		/// </summary>
		/// <remarks>
		/// Creates an exact copy of the <see cref="MessageIdList"/>.
		/// </remarks>
		/// <returns>An exact copy of the <see cref="MessageIdList"/>.</returns>
		public MessageIdList Clone ()
		{
			var clone = new MessageIdList ();

			for (int i = 0; i < references.Count; i++)
				clone.references.Add (references[i]);

			return clone;
		}

		#region IList implementation

		/// <summary>
		/// Get the index of the requested Message-Id, if it exists.
		/// </summary>
		/// <remarks>
		/// Finds the index of the specified Message-Id, if it exists.
		/// </remarks>
		/// <returns>The index of the requested Message-Id; otherwise <value>-1</value>.</returns>
		/// <param name="messageId">The Message-Id.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="messageId"/> is <c>null</c>.
		/// </exception>
		public int IndexOf (string messageId)
		{
			if (messageId is null)
				throw new ArgumentNullException (nameof (messageId));

			return references.IndexOf (messageId);
		}

		static string ValidateMessageId (string messageId)
		{
			if (messageId.Length < 2 || messageId[0] != '<' || messageId[messageId.Length - 1] != '>')
				return messageId;

			return messageId.Substring (1, messageId.Length - 2);
		}

		/// <summary>
		/// Insert a Message-Id at the specified index.
		/// </summary>
		/// <remarks>
		/// Inserts the Message-Id at the specified index in the list.
		/// </remarks>
		/// <param name="index">The index to insert the Message-Id.</param>
		/// <param name="messageId">The Message-Id to insert.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="messageId"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public void Insert (int index, string messageId)
		{
			if (messageId is null)
				throw new ArgumentNullException (nameof (messageId));

			references.Insert (index, ValidateMessageId (messageId));
			OnChanged ();
		}

		/// <summary>
		/// Remove the Message-Id at the specified index.
		/// </summary>
		/// <remarks>
		/// Removes the Message-Id at the specified index.
		/// </remarks>
		/// <param name="index">The index.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public void RemoveAt (int index)
		{
			references.RemoveAt (index);
			OnChanged ();
		}

		/// <summary>
		/// Get or set the Message-Id at the specified index.
		/// </summary>
		/// <remarks>
		/// Gets or sets the Message-Id at the specified index.
		/// </remarks>
		/// <value>The Message-Id at the specified index.</value>
		/// <param name="index">The index.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public string this [int index] {
			get { return references[index]; }
			set {
				if (value is null)
					throw new ArgumentNullException (nameof (value));

				if (references[index] == value)
					return;

				references[index] = ValidateMessageId (value);
				OnChanged ();
			}
		}

		#endregion

		#region ICollection implementation

		/// <summary>
		/// Add a Message-Id.
		/// </summary>
		/// <remarks>
		/// Adds the specified Message-Id to the end of the list.
		/// </remarks>
		/// <param name="messageId">The Message-Id.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="messageId"/> is <c>null</c>.
		/// </exception>
		public void Add (string messageId)
		{
			if (messageId is null)
				throw new ArgumentNullException (nameof (messageId));

			references.Add (ValidateMessageId (messageId));
			OnChanged ();
		}

		/// <summary>
		/// Add a collection of Message-Ids.
		/// </summary>
		/// <remarks>
		/// Adds a collection of Message-Id items to append to the list.
		/// </remarks>
		/// <param name="items">The Message-Id items to add.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="items"/> is <c>null</c>.
		/// </exception>
		public void AddRange (IEnumerable<string> items)
		{
			if (items is null)
				throw new ArgumentNullException (nameof (items));

			foreach (var msgid in items)
				references.Add (ValidateMessageId (msgid));

			OnChanged ();
		}

		/// <summary>
		/// Clear the Message-Id list.
		/// </summary>
		/// <remarks>
		/// Removes all of the Message-Ids in the list.
		/// </remarks>
		public void Clear ()
		{
			references.Clear ();
			OnChanged ();
		}

		/// <summary>
		/// Check if the <see cref="MessageIdList"/> contains the specified Message-Id.
		/// </summary>
		/// <remarks>
		/// Determines whether or not the list contains the specified Message-Id.
		/// </remarks>
		/// <returns><value>true</value> if the specified Message-Id is contained;
		/// otherwise <value>false</value>.</returns>
		/// <param name="messageId">The Message-Id.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="messageId"/> is <c>null</c>.
		/// </exception>
		public bool Contains (string messageId)
		{
			if (messageId is null)
				throw new ArgumentNullException (nameof (messageId));

			return references.Contains (messageId);
		}

		/// <summary>
		/// Copy all of the Message-Ids in the <see cref="MessageIdList"/> to the specified array.
		/// </summary>
		/// <remarks>
		/// Copies all of the Message-Ids within the <see cref="MessageIdList"/> into the array,
		/// starting at the specified array index.
		/// </remarks>
		/// <param name="array">The array to copy the Message-Ids to.</param>
		/// <param name="arrayIndex">The index into the array.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="array"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex"/> is out of range.
		/// </exception>
		public void CopyTo (string[] array, int arrayIndex)
		{
			references.CopyTo (array, arrayIndex);
		}

		/// <summary>
		/// Remove a Message-Id from the <see cref="MessageIdList"/>.
		/// </summary>
		/// <remarks>
		/// Removes the first instance of the specified Message-Id from the list if it exists.
		/// </remarks>
		/// <returns><value>true</value> if the specified Message-Id was removed;
		/// otherwise <value>false</value>.</returns>
		/// <param name="messageId">The Message-Id.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="messageId"/> is <c>null</c>.
		/// </exception>
		public bool Remove (string messageId)
		{
			if (messageId is null)
				throw new ArgumentNullException (nameof (messageId));

			if (references.Remove (messageId)) {
				OnChanged ();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Get the number of Message-Ids in the <see cref="MessageIdList"/>.
		/// </summary>
		/// <remarks>
		/// Indicates the number of Message-Ids in the list.
		/// </remarks>
		/// <value>The number of Message-Ids.</value>
		public int Count {
			get { return references.Count; }
		}

		/// <summary>
		/// Get a value indicating whether the <see cref="MessageIdList"/> is read only.
		/// </summary>
		/// <remarks>
		/// A <see cref="MessageIdList"/> is never read-only.
		/// </remarks>
		/// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
		public bool IsReadOnly {
			get { return false; }
		}

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Get an enumerator for the list of Message-Ids.
		/// </summary>
		/// <remarks>
		/// Gets an enumerator for the list of Message-Ids.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		public IEnumerator<string> GetEnumerator ()
		{
			return references.GetEnumerator ();
		}

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Get an enumerator for the list of Message-Ids.
		/// </summary>
		/// <remarks>
		/// Gets an enumerator for the list of Message-Ids.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return references.GetEnumerator ();
		}

		#endregion

		/// <summary>
		/// Serialize a <see cref="MessageIdList"/> to a string.
		/// </summary>
		/// <remarks>
		/// <para>Each Message-Id will be surrounded by angle brackets.</para>
		/// <para>If there are multiple Message-Ids in the list, they will be separated by whitespace.</para>
		/// </remarks>
		/// <returns>A string representing the <see cref="MessageIdList"/>.</returns>
		public override string ToString ()
		{
			var builder = new ValueStringBuilder (128);

			for (int i = 0; i < references.Count; i++) {
				if (builder.Length > 0)
					builder.Append (' ');

				builder.Append ('<');
				builder.Append (references[i]);
				builder.Append ('>');
			}

			return builder.ToString ();
		}

		internal event EventHandler Changed;

		void OnChanged ()
		{
			Changed?.Invoke (this, EventArgs.Empty);
		}
	}
}

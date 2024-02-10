//
// HeaderListCollection.cs
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
using System.Collections;
using System.Collections.Generic;

namespace MimeKit {
	/// <summary>
	/// A collection of <see cref="HeaderList"/> groups.
	/// </summary>
	/// <remarks>
	/// A collection of <see cref="HeaderList"/> groups used with
	/// <see cref="MessageDeliveryStatus"/>.
	/// </remarks>
	/// <seealso cref="MessageDeliveryStatus"/>
	public class HeaderListCollection : ICollection<HeaderList>
	{
		readonly List<HeaderList> groups;

		/// <summary>
		/// Initialize a new instance of the <see cref="HeaderListCollection"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="HeaderListCollection"/>.
		/// </remarks>
		public HeaderListCollection ()
		{
			groups = new List<HeaderList> ();
		}

		/// <summary>
		/// Gets the number of groups in the collection.
		/// </summary>
		/// <remarks>
		/// Gets the number of groups in the collection.
		/// </remarks>
		/// <value>The number of groups.</value>
		public int Count {
			get { return groups.Count; }
		}

		/// <summary>
		/// Gets whether or not the header list collection is read only.
		/// </summary>
		/// <remarks>
		/// A <see cref="HeaderListCollection"/> is never read-only.
		/// </remarks>
		/// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
		public bool IsReadOnly {
			get { return false; }
		}

		/// <summary>
		/// Gets or sets the <see cref="HeaderList"/> at the specified index.
		/// </summary>
		/// <remarks>
		/// Gets or sets the <see cref="HeaderList"/> at the specified index.
		/// </remarks>
		/// <value>The group of headers at the specified index.</value>
		/// <param name="index">The index.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public HeaderList this [int index] {
			get {
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException (nameof (index));

				return groups[index];
			}
			set {
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException (nameof (index));

				if (value is null)
					throw new ArgumentNullException (nameof (value));

				if (groups[index] == value)
					return;

				groups[index].Changed -= OnGroupChanged;
				value.Changed += OnGroupChanged;
				groups[index] = value;
			}
		}

		/// <summary>
		/// Adds the group of headers to the collection.
		/// </summary>
		/// <remarks>
		/// Adds the group of headers to the collection.
		/// </remarks>
		/// <param name="group">The group of headers.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="group"/> is <c>null</c>.
		/// </exception>
		public void Add (HeaderList group)
		{
			if (group is null)
				throw new ArgumentNullException (nameof (@group));

			group.Changed += OnGroupChanged;
			groups.Add (group);
			OnChanged ();
		}

		/// <summary>
		/// Clears the header list collection.
		/// </summary>
		/// <remarks>
		/// Removes all of the groups from the collection.
		/// </remarks>
		public void Clear ()
		{
			for (int i = 0; i < groups.Count; i++)
				groups[i].Changed -= OnGroupChanged;

			groups.Clear ();
			OnChanged ();
		}

		/// <summary>
		/// Checks if the collection contains the specified group of headers.
		/// </summary>
		/// <remarks>
		/// Determines whether or not the collection contains the specified group of headers.
		/// </remarks>
		/// <returns><value>true</value> if the specified group of headers is contained;
		/// otherwise, <value>false</value>.</returns>
		/// <param name="group">The group of headers.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="group"/> is <c>null</c>.
		/// </exception>
		public bool Contains (HeaderList group)
		{
			if (group is null)
				throw new ArgumentNullException (nameof (@group));

			return groups.Contains (group);
		}

		/// <summary>
		/// Copies all of the header groups in the <see cref="HeaderListCollection"/> to the specified array.
		/// </summary>
		/// <remarks>
		/// Copies all of the header groups within the <see cref="HeaderListCollection"/> into the array,
		/// starting at the specified array index.
		/// </remarks>
		/// <param name="array">The array to copy the headers to.</param>
		/// <param name="arrayIndex">The index into the array.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="array"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex"/> is out of range.
		/// </exception>
		public void CopyTo (HeaderList[] array, int arrayIndex)
		{
			groups.CopyTo (array, arrayIndex);
		}

		/// <summary>
		/// Removes the specified header group.
		/// </summary>
		/// <remarks>
		/// Removes the specified header group from the collection, if it exists.
		/// </remarks>
		/// <returns><c>true</c> if the specified header group was removed;
		/// otherwise <c>false</c>.</returns>
		/// <param name="group">The group of headers.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="group"/> is <c>null</c>.
		/// </exception>
		public bool Remove (HeaderList group)
		{
			if (group is null)
				throw new ArgumentNullException (nameof (@group));

			if (!groups.Remove (group))
				return false;

			group.Changed -= OnGroupChanged;
			OnChanged ();

			return true;
		}

		/// <summary>
		/// Gets an enumerator for the groups of headers.
		/// </summary>
		/// <remarks>
		/// Gets an enumerator for the groups of headers.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		public IEnumerator<HeaderList> GetEnumerator ()
		{
			return groups.GetEnumerator ();
		}

		/// <summary>
		/// Gets an enumerator for the groups of headers.
		/// </summary>
		/// <remarks>
		/// Gets an enumerator for the groups of headers.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		internal event EventHandler Changed;

		void OnChanged ()
		{
			Changed?.Invoke (this, EventArgs.Empty);
		}

		void OnGroupChanged (object sender, HeaderListChangedEventArgs e)
		{
			OnChanged ();
		}
	}
}

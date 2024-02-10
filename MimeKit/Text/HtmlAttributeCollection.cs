//
// HtmlAttributeCollection.cs
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

namespace MimeKit.Text {
	/// <summary>
	/// A readonly collection of HTML attributes.
	/// </summary>
	/// <remarks>
	/// A readonly collection of HTML attributes.
	/// </remarks>
	public class HtmlAttributeCollection : IEnumerable<HtmlAttribute>
	{
		/// <summary>
		/// An empty attribute collection.
		/// </summary>
		/// <remarks>
		/// An empty attribute collection.
		/// </remarks>
		public static readonly HtmlAttributeCollection Empty = new HtmlAttributeCollection ();

		readonly List<HtmlAttribute> attributes = new List<HtmlAttribute> ();

		/// <summary>
		/// Initialize a new instance of the <see cref="HtmlAttributeCollection"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="HtmlAttributeCollection"/>.
		/// </remarks>
		/// <param name="collection">A collection of attributes.</param>
		public HtmlAttributeCollection (IEnumerable<HtmlAttribute> collection)
		{
			attributes = new List<HtmlAttribute> (collection);
		}

		internal HtmlAttributeCollection ()
		{
			attributes = new List<HtmlAttribute> ();
		}

		/// <summary>
		/// Get the number of attributes in the collection.
		/// </summary>
		/// <remarks>
		/// Gets the number of attributes in the collection.
		/// </remarks>
		/// <value>The number of attributes in the collection.</value>
		public int Count {
			get { return attributes.Count; }
		}

		internal void Add (HtmlAttribute attribute)
		{
			if (attribute is null)
				throw new ArgumentNullException (nameof (attribute));

			attributes.Add (attribute);
		}

		/// <summary>
		/// Check if an attribute exists.
		/// </summary>
		/// <remarks>
		/// Checks if an attribute exists.
		/// </remarks>
		/// <param name="id">The attribute.</param>
		/// <returns><c>true</c> if the attribute exists within the collection; otherwise, <c>false</c>.</returns>
		public bool Contains (HtmlAttributeId id)
		{
			return IndexOf (id) != -1;
		}

		/// <summary>
		/// Check if an attribute exists.
		/// </summary>
		/// <remarks>
		/// Checks if an attribute exists.
		/// </remarks>
		/// <param name="name">The name of the attribute.</param>
		/// <returns><c>true</c> if the attribute exists within the collection; otherwise, <c>false</c>.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="name"/> is <c>null</c>.
		/// </exception>
		public bool Contains (string name)
		{
			return IndexOf (name) != -1;
		}

		/// <summary>
		/// Get the index of a desired attribute.
		/// </summary>
		/// <remarks>
		/// Gets the index of a desired attribute.
		/// </remarks>
		/// <param name="id">The attribute.</param>
		/// <returns><c>true</c> if the attribute exists within the collection; otherwise, <c>false</c>.</returns>
		public int IndexOf (HtmlAttributeId id)
		{
			for (int i = 0; i < attributes.Count; i++) {
				if (attributes[i].Id == id)
					return i;
			}

			return -1;
		}

		/// <summary>
		/// Get the index of a desired attribute.
		/// </summary>
		/// <remarks>
		/// Gets the index of a desired attribute.
		/// </remarks>
		/// <param name="name">The name of the attribute.</param>
		/// <returns><c>true</c> if the attribute exists within the collection; otherwise, <c>false</c>.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="name"/> is <c>null</c>.
		/// </exception>
		public int IndexOf (string name)
		{
			if (name is null)
				throw new ArgumentNullException (nameof (name));

			for (int i = 0; i < attributes.Count; i++) {
				if (attributes[i].Name.Equals (name, StringComparison.OrdinalIgnoreCase))
					return i;
			}

			return -1;
		}

		/// <summary>
		/// Get the <see cref="HtmlAttribute"/> at the specified index.
		/// </summary>
		/// <remarks>
		/// Gets the <see cref="HtmlAttribute"/> at the specified index.
		/// </remarks>
		/// <value>The HTML attribute at the specified index.</value>
		/// <param name="index">The index.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public HtmlAttribute this[int index] {
			get { return attributes[index]; }
		}

		/// <summary>
		/// Get an attribute from the collection if it exists.
		/// </summary>
		/// <remarks>
		/// Gets an attribute from the collection if it exists.
		/// </remarks>
		/// <param name="id">The id of the attribute.</param>
		/// <param name="attribute">The attribute if found; otherwise, <c>null</c>.</param>
		/// <returns><c>true</c> if the desired attribute is found; otherwise, <c>false</c>.</returns>
		public bool TryGetValue (HtmlAttributeId id, out HtmlAttribute attribute)
		{
			int index;

			if ((index = IndexOf (id)) == -1) {
				attribute = null;
				return false;
			}

			attribute = attributes[index];

			return true;
		}

		/// <summary>
		/// Get an attribute from the collection if it exists.
		/// </summary>
		/// <remarks>
		/// Gets an attribute from the collection if it exists.
		/// </remarks>
		/// <param name="name">The name of the attribute.</param>
		/// <param name="attribute">The attribute if found; otherwise, <c>null</c>.</param>
		/// <returns><c>true</c> if the desired attribute is found; otherwise, <c>false</c>.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="name"/> is <c>null</c>.
		/// </exception>
		public bool TryGetValue (string name, out HtmlAttribute attribute)
		{
			int index;

			if ((index = IndexOf (name)) == -1) {
				attribute = null;
				return false;
			}

			attribute = attributes[index];

			return true;
		}

		/// <summary>
		/// Get an enumerator for the attribute collection.
		/// </summary>
		/// <remarks>
		/// Gets an enumerator for the attribute collection.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		public IEnumerator<HtmlAttribute> GetEnumerator ()
		{
			return attributes.GetEnumerator ();
		}

		/// <summary>
		/// Get an enumerator for the attribute collection.
		/// </summary>
		/// <remarks>
		/// Gets an enumerator for the attribute collection.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return attributes.GetEnumerator ();
		}
	}
}

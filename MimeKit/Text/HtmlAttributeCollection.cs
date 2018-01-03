//
// HtmlAttributeCollection.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2018 Xamarin Inc. (www.xamarin.com)
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
		/// Initializes a new instance of the <see cref="HtmlAttributeCollection"/> class.
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
			if (attribute == null)
				throw new ArgumentNullException (nameof (attribute));

			attributes.Add (attribute);
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
		/// Gets an enumerator for the attribute collection.
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
		/// Gets an enumerator for the attribute collection.
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

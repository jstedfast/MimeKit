﻿//
// HtmlAttribute.cs
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

namespace MimeKit.Text {
	/// <summary>
	/// An HTML attribute.
	/// </summary>
	/// <remarks>
	/// An HTML attribute.
	/// </remarks>
	public class HtmlAttribute
	{
		/// <summary>
		/// Get the HTML attribute identifier.
		/// </summary>
		/// <remarks>
		/// Gets the HTML attribute identifier.
		/// </remarks>
		/// <value>The attribute identifier.</value>
		public HtmlAttributeId Id {
			get; private set;
		}

		/// <summary>
		/// Get the name of the attribute.
		/// </summary>
		/// <remarks>
		/// Gets the name of the attribute.
		/// </remarks>
		/// <value>The name of the attribute.</value>
		public string Name {
			get; private set;
		}

		/// <summary>
		/// Get the value of the attribute.
		/// </summary>
		/// <remarks>
		/// Gets the value of the attribute.
		/// </remarks>
		/// <value>The value of the attribute.</value>
		public string Value {
			get; private set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Text.HtmlAttribute"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new HTML attribute with the given id and value.
		/// </remarks>
		/// <param name="id">The attribute identifier.</param>
		/// <param name="value">The attribute value.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="id"/> is not a valid value.
		/// </exception>
		public HtmlAttribute (HtmlAttributeId id, string value)
		{
			if (id == HtmlAttributeId.Unknown)
				throw new ArgumentOutOfRangeException ("id");

			Name = id.ToAttributeName ();
			Value = value;
			Id = id;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Text.HtmlAttribute"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new HTML attribute with the given name and value.
		/// </remarks>
		/// <param name="name">The attribute name.</param>
		/// <param name="value">The attribute value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="name"/> is <c>null</c>.
		/// </exception>
		public HtmlAttribute (string name, string value)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			if (name.Length == 0)
				throw new ArgumentException ("The attribute name cannot be empty.", "name");

			if (!HtmlUtils.IsValidAttributeName (name))
				throw new ArgumentException ("Invalid attribute name.", "name");

			Id = name.ToHtmlAttributeId ();
			Value = value;
			Name = name;
		}

		HtmlAttribute ()
		{
			Id = HtmlAttributeId.Unknown;
			Value = null;
			Name = null;
		}
	}
}

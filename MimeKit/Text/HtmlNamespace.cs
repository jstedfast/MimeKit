//
// HtmlNamespace.cs
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

namespace MimeKit.Text {
	/// <summary>
	/// An HTML namespace.
	/// </summary>
	/// <remarks>
	/// An HTML namespace.
	/// </remarks>
	public enum HtmlNamespace {
		/// <summary>
		/// The namespace is "http://www.w3.org/1999/xhtml".
		/// </summary>
		Html,

		/// <summary>
		/// The namespace is "http://www.w3.org/1998/Math/MathML".
		/// </summary>
		MathML,

		/// <summary>
		/// The namespace is "http://www.w3.org/2000/svg".
		/// </summary>
		Svg,

		/// <summary>
		/// The namespace is "http://www.w3.org/1999/xlink".
		/// </summary>
		XLink,

		/// <summary>
		/// The namespace is "http://www.w3.org/XML/1998/namespace".
		/// </summary>
		Xml,

		/// <summary>
		/// The namespace is "http://www.w3.org/2000/xmlns/".
		/// </summary>
		XmlNS
	}

	/// <summary>
	/// <see cref="HtmlNamespace"/> extension methods.
	/// </summary>
	/// <remarks>
	/// <see cref="HtmlNamespace"/> extension methods.
	/// </remarks>
	static class HtmlNamespaceExtensions
	{
		static readonly int NamespacePrefixLength = "http://www.w3.org/".Length;

		static readonly string[] NamespaceValues = {
			"http://www.w3.org/1999/xhtml",
			"http://www.w3.org/1998/Math/MathML",
			"http://www.w3.org/2000/svg",
			"http://www.w3.org/1999/xlink",
			"http://www.w3.org/XML/1998/namespace",
			"http://www.w3.org/2000/xmlns/"
		};

		/// <summary>
		/// Converts the enum value into the equivalent namespace url.
		/// </summary>
		/// <remarks>
		/// Converts the enum value into the equivalent namespace url.
		/// </remarks>
		/// <returns>The tag name.</returns>
		/// <param name="value">The enum value.</param>
		public static string ToNamespaceUrl (this HtmlNamespace value)
		{
			int index = (int) value;

			if (index < 0 || index >= NamespaceValues.Length)
				throw new ArgumentOutOfRangeException (nameof (value));

			return NamespaceValues[index];
		}

		/// <summary>
		/// Convert the tag name into the equivalent tag id.
		/// </summary>
		/// <remarks>
		/// Converts the tag name into the equivalent tag id.
		/// </remarks>
		/// <returns>The tag id.</returns>
		/// <param name="ns">The namespace.</param>
		public static HtmlNamespace ToHtmlNamespace (this string ns)
		{
			if (ns is null)
				throw new ArgumentNullException (nameof (ns));

			if (!ns.StartsWith ("http://www.w3.org/", StringComparison.OrdinalIgnoreCase))
				return HtmlNamespace.Html;

			for (int i = 0; i < NamespaceValues.Length; i++) {
				if (ns.Length != NamespaceValues[i].Length)
					continue;

				if (string.Compare (ns, NamespacePrefixLength, NamespaceValues[i], NamespacePrefixLength,
					ns.Length - NamespacePrefixLength, StringComparison.OrdinalIgnoreCase) == 0)
					return (HtmlNamespace) i;
			}

			return HtmlNamespace.Html;
		}
	}
}

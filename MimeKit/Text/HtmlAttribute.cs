// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace MimeKit.Text {
	/// <summary>
	/// An HTML attribute.
	/// </summary>
	/// <remarks>
	/// An HTML attribute.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
	/// </example>
	public class HtmlAttribute
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="HtmlAttribute"/> class.
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
				throw new ArgumentOutOfRangeException (nameof (id));

			Name = id.ToAttributeName ();
			Value = value;
			Id = id;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="HtmlAttribute"/> class.
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
			if (name is null)
				throw new ArgumentNullException (nameof (name));

			if (name.Length == 0)
				throw new ArgumentException ("The attribute name cannot be empty.", nameof (name));

			if (!HtmlUtils.IsValidTokenName (name))
				throw new ArgumentException ("Invalid attribute name.", nameof (name));

			Id = name.ToHtmlAttributeId ();
			Value = value;
			Name = name;
		}

		internal HtmlAttribute (string name)
		{
			if (name is null)
				throw new ArgumentNullException (nameof (name));

			if (name.Length == 0)
				throw new ArgumentException ("The attribute name cannot be empty.", nameof (name));

			Id = name.ToHtmlAttributeId ();
			Name = name;
		}

		/// <summary>
		/// Get the HTML attribute identifier.
		/// </summary>
		/// <remarks>
		/// Gets the HTML attribute identifier.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
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
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
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
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <value>The value of the attribute.</value>
		public string Value {
			get; internal set;
		}
	}
}

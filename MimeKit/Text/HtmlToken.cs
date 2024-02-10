//
// HtmlToken.cs
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
using System.IO;
using System.Collections.Generic;

namespace MimeKit.Text {
	/// <summary>
	/// An abstract HTML token class.
	/// </summary>
	/// <remarks>
	/// An abstract HTML token class.
	/// </remarks>
	public abstract class HtmlToken
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="HtmlToken"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="HtmlToken"/>.
		/// </remarks>
		/// <param name="kind">The kind of token.</param>
		protected HtmlToken (HtmlTokenKind kind)
		{
			Kind = kind;
		}

		/// <summary>
		/// Get the kind of HTML token that this object represents.
		/// </summary>
		/// <remarks>
		/// Gets the kind of HTML token that this object represents.
		/// </remarks>
		/// <value>The kind of token.</value>
		public HtmlTokenKind Kind {
			get; private set;
		}

		/// <summary>
		/// Write the HTML token to a <see cref="System.IO.TextWriter"/>.
		/// </summary>
		/// <remarks>
		/// Writes the HTML token to a <see cref="System.IO.TextWriter"/>.
		/// </remarks>
		/// <param name="output">The output.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="output"/> is <c>null</c>.
		/// </exception>
		public abstract void WriteTo (TextWriter output);

		/// <summary>
		/// Return a <see cref="System.String"/> that represents the current <see cref="HtmlToken"/>.
		/// </summary>
		/// <remarks>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="HtmlToken"/>.
		/// </remarks>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="HtmlToken"/>.</returns>
		public override string ToString ()
		{
			using (var output = new StringWriter ()) {
				WriteTo (output);

				return output.ToString ();
			}
		}
	}

	/// <summary>
	/// An HTML comment token.
	/// </summary>
	/// <remarks>
	/// An HTML comment token.
	/// </remarks>
	public class HtmlCommentToken : HtmlToken
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="HtmlCommentToken"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="HtmlCommentToken"/>.
		/// </remarks>
		/// <param name="comment">The comment text.</param>
		/// <param name="bogus"><c>true</c> if the comment is bogus; otherwise, <c>false</c>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="comment"/> is <c>null</c>.
		/// </exception>
		public HtmlCommentToken (string comment, bool bogus = false) : base (HtmlTokenKind.Comment)
		{
			if (comment is null)
				throw new ArgumentNullException (nameof (comment));

			IsBogusComment = bogus;
			Comment = comment;
		}

		/// <summary>
		/// Get the comment.
		/// </summary>
		/// <remarks>
		/// Gets the comment.
		/// </remarks>
		/// <value>The comment.</value>
		public string Comment {
			get; private set;
		}

		/// <summary>
		/// Get whether or not the comment is a bogus comment.
		/// </summary>
		/// <remarks>
		/// Gets whether or not the comment is a bogus comment.
		/// </remarks>
		/// <value><c>true</c> if the comment is bogus; otherwise, <c>false</c>.</value>
		public bool IsBogusComment {
			get; private set;
		}

		internal bool IsBangComment {
			get; set;
		}

		/// <summary>
		/// Write the HTML comment to a <see cref="System.IO.TextWriter"/>.
		/// </summary>
		/// <remarks>
		/// Writes the HTML comment to a <see cref="System.IO.TextWriter"/>.
		/// </remarks>
		/// <param name="output">The output.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="output"/> is <c>null</c>.
		/// </exception>
		public override void WriteTo (TextWriter output)
		{
			if (output is null)
				throw new ArgumentNullException (nameof (output));

			if (!IsBogusComment) {
				output.Write ("<!--");
				output.Write (Comment);
				output.Write ("-->");
			} else {
				output.Write ('<');
				if (IsBangComment)
					output.Write ('!');
				output.Write (Comment);
				output.Write ('>');
			}
		}
	}

	/// <summary>
	/// An HTML token constisting of character data.
	/// </summary>
	/// <remarks>
	/// An HTML token consisting of character data.
	/// </remarks>
	public class HtmlDataToken : HtmlToken
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="HtmlDataToken"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="HtmlDataToken"/>.
		/// </remarks>
		/// <param name="kind">The kind of character data.</param>
		/// <param name="data">The character data.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="kind"/> is not a valid <see cref="HtmlTokenKind"/>.
		/// </exception>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="data"/> is <c>null</c>.
		/// </exception>
		protected HtmlDataToken (HtmlTokenKind kind, string data) : base (kind)
		{
			switch (kind) {
			default: throw new ArgumentOutOfRangeException (nameof (kind));
			case HtmlTokenKind.ScriptData:
			case HtmlTokenKind.CData:
			case HtmlTokenKind.Data:
				break;
			}

			if (data is null)
				throw new ArgumentNullException (nameof (data));

			Data = data;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="HtmlDataToken"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="HtmlDataToken"/>.
		/// </remarks>
		/// <param name="data">The character data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="data"/> is <c>null</c>.
		/// </exception>
		public HtmlDataToken (string data) : base (HtmlTokenKind.Data)
		{
			if (data is null)
				throw new ArgumentNullException (nameof (data));

			Data = data;
		}

		internal bool EncodeEntities {
			get; set;
		}

		/// <summary>
		/// Get the character data.
		/// </summary>
		/// <remarks>
		/// Gets the character data.
		/// </remarks>
		/// <value>The character data.</value>
		public string Data {
			get; private set;
		}

		/// <summary>
		/// Write the HTML character data to a <see cref="System.IO.TextWriter"/>.
		/// </summary>
		/// <remarks>
		/// Writes the HTML character data to a <see cref="System.IO.TextWriter"/>,
		/// encoding it if it isn't already encoded.
		/// </remarks>
		/// <param name="output">The output.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="output"/> is <c>null</c>.
		/// </exception>
		public override void WriteTo (TextWriter output)
		{
			if (output is null)
				throw new ArgumentNullException (nameof (output));

			if (!EncodeEntities) {
				output.Write (Data);
				return;
			}

			HtmlUtils.HtmlEncode (output, Data);
		}
	}

	/// <summary>
	/// An HTML token constisting of <c>[CDATA[</c>.
	/// </summary>
	/// <remarks>
	/// An HTML token consisting of <c>[CDATA[</c>.
	/// </remarks>
	public class HtmlCDataToken : HtmlDataToken
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="HtmlCDataToken"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="HtmlCDataToken"/>.
		/// </remarks>
		/// <param name="data">The character data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="data"/> is <c>null</c>.
		/// </exception>
		public HtmlCDataToken (string data) : base (HtmlTokenKind.CData, data)
		{
		}

		/// <summary>
		/// Write the HTML character data to a <see cref="System.IO.TextWriter"/>.
		/// </summary>
		/// <remarks>
		/// Writes the HTML character data to a <see cref="System.IO.TextWriter"/>,
		/// encoding it if it isn't already encoded.
		/// </remarks>
		/// <param name="output">The output.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="output"/> is <c>null</c>.
		/// </exception>
		public override void WriteTo (TextWriter output)
		{
			if (output is null)
				throw new ArgumentNullException (nameof (output));

			output.Write ("<![CDATA[");
			output.Write (Data);
			output.Write ("]]>");
		}
	}

	/// <summary>
	/// An HTML token constisting of script data.
	/// </summary>
	/// <remarks>
	/// An HTML token consisting of script data.
	/// </remarks>
	public class HtmlScriptDataToken : HtmlDataToken
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="HtmlScriptDataToken"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="HtmlScriptDataToken"/>.
		/// </remarks>
		/// <param name="data">The script data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="data"/> is <c>null</c>.
		/// </exception>
		public HtmlScriptDataToken (string data) : base (HtmlTokenKind.ScriptData, data)
		{
		}

		/// <summary>
		/// Write the HTML script data to a <see cref="System.IO.TextWriter"/>.
		/// </summary>
		/// <remarks>
		/// Writes the HTML script data to a <see cref="System.IO.TextWriter"/>,
		/// encoding it if it isn't already encoded.
		/// </remarks>
		/// <param name="output">The output.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="output"/> is <c>null</c>.
		/// </exception>
		public override void WriteTo (TextWriter output)
		{
			if (output is null)
				throw new ArgumentNullException (nameof (output));

			output.Write (Data);
		}
	}

	/// <summary>
	/// An HTML tag token.
	/// </summary>
	/// <remarks>
	/// An HTML tag token.
	/// </remarks>
	public class HtmlTagToken : HtmlToken
	{
		HtmlTagId id = (HtmlTagId) (-1);

		/// <summary>
		/// Initialize a new instance of the <see cref="HtmlTagToken"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="HtmlTagToken"/>.
		/// </remarks>
		/// <param name="name">The name of the tag.</param>
		/// <param name="attributes">The attributes.</param>
		/// <param name="isEmptyElement"><c>true</c> if the tag is an empty element; otherwise, <c>false</c>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="name"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="attributes"/> is <c>null</c>.</para>
		/// </exception>
		public HtmlTagToken (string name, IEnumerable<HtmlAttribute> attributes, bool isEmptyElement) : base (HtmlTokenKind.Tag)
		{
			if (name is null)
				throw new ArgumentNullException (nameof (name));

			if (attributes is null)
				throw new ArgumentNullException (nameof (attributes));

			Attributes = new HtmlAttributeCollection (attributes);
			IsEmptyElement = isEmptyElement;
			Name = name;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="HtmlTagToken"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="HtmlTagToken"/>.
		/// </remarks>
		/// <param name="name">The name of the tag.</param>
		/// <param name="isEndTag"><c>true</c> if the tag is an end tag; otherwise, <c>false</c>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="name"/> is <c>null</c>.
		/// </exception>
		public HtmlTagToken (string name, bool isEndTag) : base (HtmlTokenKind.Tag)
		{
			if (name is null)
				throw new ArgumentNullException (nameof (name));

			Attributes = new HtmlAttributeCollection ();
			IsEndTag = isEndTag;
			Name = name;
		}

		/// <summary>
		/// Get the attributes.
		/// </summary>
		/// <remarks>
		/// Gets the attributes.
		/// </remarks>
		/// <value>The attributes.</value>
		public HtmlAttributeCollection Attributes {
			get; private set;
		}

		/// <summary>
		/// Get the HTML tag identifier.
		/// </summary>
		/// <remarks>
		/// Gets the HTML tag identifier.
		/// </remarks>
		/// <value>The HTML tag identifier.</value>
		public HtmlTagId Id {
			get {
				if (id == (HtmlTagId) (-1))
					id = Name.ToHtmlTagId ();

				return id;
			}
		}

		/// <summary>
		/// Get whether or not the tag is an empty element.
		/// </summary>
		/// <remarks>
		/// Gets whether or not the tag is an empty element.
		/// </remarks>
		/// <value><c>true</c> if the tag is an empty element; otherwise, <c>false</c>.</value>
		public bool IsEmptyElement {
			get; internal set;
		}

		/// <summary>
		/// Get whether or not the tag is an end tag.
		/// </summary>
		/// <remarks>
		/// Gets whether or not the tag is an end tag.
		/// </remarks>
		/// <value><c>true</c> if the tag is an end tag; otherwise, <c>false</c>.</value>
		public bool IsEndTag {
			get; private set;
		}

		/// <summary>
		/// Get the name of the tag.
		/// </summary>
		/// <remarks>
		/// Gets the name of the tag.
		/// </remarks>
		/// <value>The name.</value>
		public string Name {
			get; private set;
		}

		/// <summary>
		/// Write the HTML tag to a <see cref="System.IO.TextWriter"/>.
		/// </summary>
		/// <remarks>
		/// Writes the HTML tag to a <see cref="System.IO.TextWriter"/>.
		/// </remarks>
		/// <param name="output">The output.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="output"/> is <c>null</c>.
		/// </exception>
		public override void WriteTo (TextWriter output)
		{
			if (output is null)
				throw new ArgumentNullException (nameof (output));

			output.Write ('<');
			if (IsEndTag)
				output.Write ('/');
			output.Write (Name);
			for (int i = 0; i < Attributes.Count; i++) {
				output.Write (' ');
				output.Write (Attributes[i].Name);
				if (Attributes[i].Value != null) {
					output.Write ('=');
					HtmlUtils.HtmlAttributeEncode (output, Attributes[i].Value);
				}
			}
			if (IsEmptyElement)
				output.Write ('/');
			output.Write ('>');
		}
	}

	/// <summary>
	/// An HTML DOCTYPE token.
	/// </summary>
	/// <remarks>
	/// An HTML DOCTYPE token.
	/// </remarks>
	public class HtmlDocTypeToken : HtmlToken
	{
		string publicIdentifier;
		string systemIdentifier;

		/// <summary>
		/// Initialize a new instance of the <see cref="HtmlDocTypeToken"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="HtmlDocTypeToken"/>.
		/// </remarks>
		public HtmlDocTypeToken () : base (HtmlTokenKind.DocType)
		{
			RawTagName = "DOCTYPE";
		}

		internal string RawTagName {
			get; set;
		}

		/// <summary>
		/// Get whether or not quirks-mode should be forced.
		/// </summary>
		/// <remarks>
		/// Gets whether or not quirks-mode should be forced.
		/// </remarks>
		/// <value><c>true</c> if quirks-mode should be forced; otherwise, <c>false</c>.</value>
		public bool ForceQuirksMode {
			get; set;
		}

		/// <summary>
		/// Get or set the DOCTYPE name.
		/// </summary>
		/// <remarks>
		/// Gets or sets the DOCTYPE name.
		/// </remarks>
		/// <value>The name.</value>
		public string Name {
			get; set;
		}

		/// <summary>
		/// Get or set the public identifier.
		/// </summary>
		/// <remarks>
		/// Gets or sets the public identifier.
		/// </remarks>
		/// <value>The public identifier.</value>
		public string PublicIdentifier {
			get { return publicIdentifier; }
			set {
				publicIdentifier = value;
				if (value != null) {
					if (PublicKeyword is null)
						PublicKeyword = "PUBLIC";
				} else {
					if (systemIdentifier != null)
						SystemKeyword = "SYSTEM";
				}
			}
		}

		/// <summary>
		/// Get the public keyword that was used.
		/// </summary>
		/// <remarks>
		/// Gets the public keyword that was used.
		/// </remarks>
		/// <value>The public keyword or <c>null</c> if it wasn't used.</value>
		public string PublicKeyword {
			get; internal set;
		}

		/// <summary>
		/// Get or set the system identifier.
		/// </summary>
		/// <remarks>
		/// Gets or sets the system identifier.
		/// </remarks>
		/// <value>The system identifier.</value>
		public string SystemIdentifier {
			get { return systemIdentifier; }
			set {
				systemIdentifier = value;
				if (value != null) {
					if (publicIdentifier is null && SystemKeyword is null)
						SystemKeyword = "SYSTEM";
				} else {
					SystemKeyword = null;
				}
			}
		}

		/// <summary>
		/// Get the system keyword that was used.
		/// </summary>
		/// <remarks>
		/// Gets the system keyword that was used.
		/// </remarks>
		/// <value>The system keyword or <c>null</c> if it wasn't used.</value>
		public string SystemKeyword {
			get; internal set;
		}

		/// <summary>
		/// Write the DOCTYPE tag to a <see cref="System.IO.TextWriter"/>.
		/// </summary>
		/// <remarks>
		/// Writes the DOCTYPE tag to a <see cref="System.IO.TextWriter"/>.
		/// </remarks>
		/// <param name="output">The output.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="output"/> is <c>null</c>.
		/// </exception>
		public override void WriteTo (TextWriter output)
		{
			if (output is null)
				throw new ArgumentNullException (nameof (output));

			output.Write ("<!");
			output.Write (RawTagName);
			if (Name != null) {
				output.Write (' ');
				output.Write (Name);
			}
			if (PublicIdentifier != null) {
				output.Write (' ');
				output.Write (PublicKeyword);
				output.Write (" \"");
				output.Write (PublicIdentifier);
				output.Write ('"');
				if (SystemIdentifier != null) {
					output.Write (" \"");
					output.Write (SystemIdentifier);
					output.Write ('"');
				}
			} else if (SystemIdentifier != null) {
				output.Write (' ');
				output.Write (SystemKeyword);
				output.Write (" \"");
				output.Write (SystemIdentifier);
				output.Write ('"');
			}
			output.Write ('>');
		}
	}
}

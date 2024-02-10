//
// HtmlWriter.cs
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
using System.Text;

namespace MimeKit.Text {
	/// <summary>
	/// An HTML writer.
	/// </summary>
	/// <remarks>
	/// An HTML writer.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
	/// </example>
	public class HtmlWriter : IDisposable
	{
		TextWriter html;
		bool empty;

		/// <summary>
		/// Initialize a new instance of the <see cref="HtmlWriter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="HtmlWriter"/>.
		/// </remarks>
		/// <param name="stream">The output stream.</param>
		/// <param name="encoding">The encoding to use for the output.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="encoding"/> is <c>null</c>.</para>
		/// </exception>
		public HtmlWriter (Stream stream, Encoding encoding)
		{
			if (stream is null)
				throw new ArgumentNullException (nameof (stream));

			if (encoding is null)
				throw new ArgumentNullException (nameof (encoding));

			html = new StreamWriter (stream, encoding, 4096);
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="HtmlWriter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="HtmlWriter"/>.
		/// </remarks>
		/// <param name="output">The output text writer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="output"/> is <c>null</c>.
		/// </exception>
		public HtmlWriter (TextWriter output)
		{
			if (output is null)
				throw new ArgumentNullException (nameof (output));

			html = output;
		}

		/// <summary>
		/// Release unmanaged resources and perform other cleanup operations before the
		/// <see cref="HtmlWriter"/> is reclaimed by garbage collection.
		/// </summary>
		/// <remarks>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="HtmlWriter"/> is reclaimed by garbage collection.
		/// </remarks>
		~HtmlWriter ()
		{
			Dispose (false);
		}

		void CheckDisposed ()
		{
			if (html is null)
				throw new ObjectDisposedException ("HtmlWriter");
		}

		/// <summary>
		/// Get the current state of the writer.
		/// </summary>
		/// <remarks>
		/// Gets the current state of the writer.
		/// </remarks>
		/// <value>The state of the writer.</value>
		public HtmlWriterState WriterState {
			get; private set;
		}

		static void ValidateArguments (char[] buffer, int index, int count)
		{
			if (buffer is null)
				throw new ArgumentNullException (nameof (buffer));

			if (index < 0 || index > buffer.Length)
				throw new ArgumentOutOfRangeException (nameof (index));

			if (count < 0 || count > (buffer.Length - index))
				throw new ArgumentOutOfRangeException (nameof (count));
		}

		static void ValidateAttributeName (string name)
		{
			if (name is null)
				throw new ArgumentNullException (nameof (name));

			if (name.Length == 0)
				throw new ArgumentException ("The attribute name cannot be empty.", nameof (name));

			if (!HtmlUtils.IsValidTokenName (name))
				throw new ArgumentException ($"Invalid attribute name: {name}", nameof (name));
		}

		static void ValidateTagName (string name)
		{
			if (name is null)
				throw new ArgumentNullException (nameof (name));

			if (name.Length == 0)
				throw new ArgumentException ("The tag name cannot be empty.", nameof (name));

			if (!HtmlUtils.IsValidTokenName (name))
				throw new ArgumentException ($"Invalid tag name: {name}", nameof (name));
		}

		void EncodeAttributeName (string name)
		{
			if (WriterState == HtmlWriterState.Default)
				throw new InvalidOperationException ("Cannot write attributes in the Default state.");

			html.Write (' ');
			html.Write (name);
			WriterState = HtmlWriterState.Attribute;
		}

		void EncodeAttributeValue (char[] value, int startIndex, int count)
		{
			if (WriterState != HtmlWriterState.Attribute)
				throw new InvalidOperationException ("Attribute values can only be written in the Attribute state.");

			html.Write ('=');
			HtmlUtils.HtmlAttributeEncode (html, value, startIndex, count);
			WriterState = HtmlWriterState.Tag;
		}

		void EncodeAttributeValue (string value)
		{
			if (WriterState != HtmlWriterState.Attribute)
				throw new InvalidOperationException ("Attribute values can only be written in the Attribute state.");

			html.Write ('=');
			HtmlUtils.HtmlAttributeEncode (html, value);
			WriterState = HtmlWriterState.Tag;
		}

		void EncodeAttribute (string name, char[] value, int startIndex, int count)
		{
			EncodeAttributeName (name);
			EncodeAttributeValue (value, startIndex, count);
		}

		void EncodeAttribute (string name, string value)
		{
			EncodeAttributeName (name);
			EncodeAttributeValue (value);
		}

		/// <summary>
		/// Write the attribute to the output stream.
		/// </summary>
		/// <remarks>
		/// Writes the attribute to the output stream.
		/// </remarks>
		/// <param name="id">The attribute identifier.</param>
		/// <param name="buffer">A buffer containing the attribute value.</param>
		/// <param name="index">The starting index of the attribute value.</param>
		/// <param name="count">The number of characters in the attribute value.</param>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="id"/> is not a valid HTML attribute identifier.
		/// </exception>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero or greater than the length of
		/// <paramref name="buffer"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> and <paramref name="count"/> do not specify
		/// a valid range in the <paramref name="buffer"/>.</para>
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="HtmlWriter"/> is not in a state that allows writing attributes.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="HtmlWriter"/> has been disposed.
		/// </exception>
		public void WriteAttribute (HtmlAttributeId id, char[] buffer, int index, int count)
		{
			if (id == HtmlAttributeId.Unknown)
				throw new ArgumentException ("Invalid attribute.", nameof (id));

			ValidateArguments (buffer, index, count);

			CheckDisposed ();

			EncodeAttribute (id.ToAttributeName (), buffer, index, count);
		}

		/// <summary>
		/// Write the attribute to the output stream.
		/// </summary>
		/// <remarks>
		/// Writes the attribute to the output stream.
		/// </remarks>
		/// <param name="name">The attribute name.</param>
		/// <param name="buffer">A buffer containing the attribute value.</param>
		/// <param name="index">The starting index of the attribute value.</param>
		/// <param name="count">The number of characters in the attribute value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="name"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero or greater than the length of
		/// <paramref name="buffer"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> and <paramref name="count"/> do not specify
		/// a valid range in the <paramref name="buffer"/>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="name"/> is an invalid attribute name.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="HtmlWriter"/> is not in a state that allows writing attributes.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="HtmlWriter"/> has been disposed.
		/// </exception>
		public void WriteAttribute (string name, char[] buffer, int index, int count)
		{
			ValidateAttributeName (name);
			ValidateArguments (buffer, index, count);
			CheckDisposed ();

			EncodeAttribute (name, buffer, index, count);
		}

		/// <summary>
		/// Write the attribute to the output stream.
		/// </summary>
		/// <remarks>
		/// Writes the attribute to the output stream.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <param name="id">The attribute identifier.</param>
		/// <param name="value">The attribute value.</param>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="id"/> is not a valid HTML attribute identifier.
		/// </exception>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="HtmlWriter"/> is not in a state that allows writing attributes.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="HtmlWriter"/> has been disposed.
		/// </exception>
		public void WriteAttribute (HtmlAttributeId id, string value)
		{
			if (id == HtmlAttributeId.Unknown)
				throw new ArgumentException ("Invalid attribute.", nameof (id));

			if (value is null)
				throw new ArgumentNullException (nameof (value));

			CheckDisposed ();

			EncodeAttribute (id.ToAttributeName (), value);
		}

		/// <summary>
		/// Write the attribute to the output stream.
		/// </summary>
		/// <remarks>
		/// Writes the attribute to the output stream.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <param name="name">The attribute name.</param>
		/// <param name="value">The attribute value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="name"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="name"/> is an invalid attribute name.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="HtmlWriter"/> is not in a state that allows writing attributes.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="HtmlWriter"/> has been disposed.
		/// </exception>
		public void WriteAttribute (string name, string value)
		{
			ValidateAttributeName (name);

			if (value is null)
				throw new ArgumentNullException (nameof (value));

			CheckDisposed ();

			EncodeAttribute (name, value);
		}

		/// <summary>
		/// Write the attribute to the output stream.
		/// </summary>
		/// <remarks>
		/// Writes the attribute to the output stream.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <param name="attribute">The attribute.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="attribute"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="HtmlWriter"/> is not in a state that allows writing attributes.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="HtmlWriter"/> has been disposed.
		/// </exception>
		public void WriteAttribute (HtmlAttribute attribute)
		{
			if (attribute is null)
				throw new ArgumentNullException (nameof (attribute));

			EncodeAttributeName (attribute.Name);

			if (attribute.Value != null)
				EncodeAttributeValue (attribute.Value);
		}

		/// <summary>
		/// Write the attribute name to the output stream.
		/// </summary>
		/// <remarks>
		/// Writes the attribute name to the output stream.
		/// </remarks>
		/// <param name="id">The attribute identifier.</param>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="id"/> is not a valid HTML attribute identifier.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="HtmlWriter"/> is not in a state that allows writing attributes.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="HtmlWriter"/> has been disposed.
		/// </exception>
		public void WriteAttributeName (HtmlAttributeId id)
		{
			if (id == HtmlAttributeId.Unknown)
				throw new ArgumentException ("Invalid attribute.", nameof (id));

			if (WriterState == HtmlWriterState.Default)
				throw new InvalidOperationException ("Cannot write attributes in the Default state.");

			CheckDisposed ();

			EncodeAttributeName (id.ToAttributeName ());
		}

		/// <summary>
		/// Write the attribute name to the output stream.
		/// </summary>
		/// <remarks>
		/// Writes the attribute name to the output stream.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <param name="name">The attribute name.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="name"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="name"/> is an invalid attribute name.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="HtmlWriter"/> is not in a state that allows writing attributes.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="HtmlWriter"/> has been disposed.
		/// </exception>
		public void WriteAttributeName (string name)
		{
			ValidateAttributeName (name);

			if (WriterState == HtmlWriterState.Default)
				throw new InvalidOperationException ("Cannot write attributes in the Default state.");

			CheckDisposed ();

			EncodeAttributeName (name);
		}

		/// <summary>
		/// Write the attribute value to the output stream.
		/// </summary>
		/// <remarks>
		/// Writes the attribute value to the output stream.
		/// </remarks>
		/// <param name="buffer">A buffer containing the attribute value.</param>
		/// <param name="index">The starting index of the attribute value.</param>
		/// <param name="count">The number of characters in the attribute value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero or greater than the length of
		/// <paramref name="buffer"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> and <paramref name="count"/> do not specify
		/// a valid range in the <paramref name="buffer"/>.</para>
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="HtmlWriter"/> is not in a state that allows writing attribute values.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="HtmlWriter"/> has been disposed.
		/// </exception>
		public void WriteAttributeValue (char[] buffer, int index, int count)
		{
			ValidateArguments (buffer, index, count);
			CheckDisposed ();

			EncodeAttributeValue (buffer, index, count);
		}

		/// <summary>
		/// Write the attribute value to the output stream.
		/// </summary>
		/// <remarks>
		/// Writes the attribute value to the output stream.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <param name="value">The attribute value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="HtmlWriter"/> is not in a state that allows writing attribute values.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="HtmlWriter"/> has been disposed.
		/// </exception>
		public void WriteAttributeValue (string value)
		{
			if (value is null)
				throw new ArgumentNullException (nameof (value));

			CheckDisposed ();

			EncodeAttributeValue (value);
		}

		void FlushWriterState ()
		{
			if (WriterState != HtmlWriterState.Default) {
				WriterState = HtmlWriterState.Default;
				html.Write (empty ? "/>" : ">");
				empty = false;
			}
		}

		/// <summary>
		/// Write an empty element tag.
		/// </summary>
		/// <remarks>
		/// Writes an empty element tag.
		/// </remarks>
		/// <param name="id">The HTML tag identifier.</param>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="id"/> is not a valid HTML tag identifier.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="HtmlWriter"/> has been disposed.
		/// </exception>
		public void WriteEmptyElementTag (HtmlTagId id)
		{
			if (id == HtmlTagId.Unknown)
				throw new ArgumentException ("Invalid tag.", nameof (id));

			CheckDisposed ();

			FlushWriterState ();

			html.Write ($"<{id.ToHtmlTagName ()}");
			WriterState = HtmlWriterState.Tag;
			empty = true;
		}

		/// <summary>
		/// Write an empty element tag.
		/// </summary>
		/// <remarks>
		/// Writes an empty element tag.
		/// </remarks>
		/// <param name="name">The name of the HTML tag.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="name"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="name"/> is not a valid HTML tag.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="HtmlWriter"/> has been disposed.
		/// </exception>
		public void WriteEmptyElementTag (string name)
		{
			ValidateTagName (name);
			CheckDisposed ();

			FlushWriterState ();

			html.Write ($"<{name}");
			WriterState = HtmlWriterState.Tag;
			empty = true;
		}

		/// <summary>
		/// Write an end tag.
		/// </summary>
		/// <remarks>
		/// Writes an end tag.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <param name="id">The HTML tag identifier.</param>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="id"/> is not a valid HTML tag identifier.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="HtmlWriter"/> has been disposed.
		/// </exception>
		public void WriteEndTag (HtmlTagId id)
		{
			if (id == HtmlTagId.Unknown)
				throw new ArgumentException ("Invalid tag.", nameof (id));

			CheckDisposed ();

			FlushWriterState ();

			html.Write ($"</{id.ToHtmlTagName ()}>");
		}

		/// <summary>
		/// Write an end tag.
		/// </summary>
		/// <remarks>
		/// Writes an end tag.
		/// </remarks>
		/// <param name="name">The name of the HTML tag.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="name"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="name"/> is not a valid HTML tag.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="HtmlWriter"/> has been disposed.
		/// </exception>
		public void WriteEndTag (string name)
		{
			ValidateTagName (name);
			CheckDisposed ();

			FlushWriterState ();

			html.Write ($"</{name}>");
		}

		/// <summary>
		/// Write a buffer containing HTML markup directly to the output, without escaping special characters.
		/// </summary>
		/// <remarks>
		/// Writes a buffer containing HTML markup directly to the output, without escaping special characters.
		/// </remarks>
		/// <param name="buffer">The buffer containing HTML markup.</param>
		/// <param name="index">The index of the first character to write.</param>
		/// <param name="count">The number of characters to write.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero or greater than the length of
		/// <paramref name="buffer"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> and <paramref name="count"/> do not specify
		/// a valid range in the <paramref name="buffer"/>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="HtmlWriter"/> has been disposed.
		/// </exception>
		public void WriteMarkupText (char[] buffer, int index, int count)
		{
			ValidateArguments (buffer, index, count);
			CheckDisposed ();

			FlushWriterState ();

			html.Write (buffer, index, count);
		}

		/// <summary>
		/// Write a string containing HTML markup directly to the output, without escaping special characters.
		/// </summary>
		/// <remarks>
		/// Writes a string containing HTML markup directly to the output, without escaping special characters.
		/// </remarks>
		/// <param name="value">The string containing HTML markup.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="HtmlWriter"/> has been disposed.
		/// </exception>
		public void WriteMarkupText (string value)
		{
			if (value is null)
				throw new ArgumentNullException (nameof (value));

			CheckDisposed ();

			FlushWriterState ();

			html.Write (value);
		}

		/// <summary>
		/// Write a start tag.
		/// </summary>
		/// <remarks>
		/// Writes a start tag.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <param name="id">The HTML tag identifier.</param>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="id"/> is not a valid HTML tag identifier.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="HtmlWriter"/> has been disposed.
		/// </exception>
		public void WriteStartTag (HtmlTagId id)
		{
			if (id == HtmlTagId.Unknown)
				throw new ArgumentException ("Invalid tag.", nameof (id));

			CheckDisposed ();

			FlushWriterState ();

			html.Write ($"<{id.ToHtmlTagName ()}");
			WriterState = HtmlWriterState.Tag;
		}

		/// <summary>
		/// Write a start tag.
		/// </summary>
		/// <remarks>
		/// Writes a start tag.
		/// </remarks>
		/// <param name="name">The name of the HTML tag.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="name"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="name"/> is not a valid HTML tag.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="HtmlWriter"/> has been disposed.
		/// </exception>
		public void WriteStartTag (string name)
		{
			ValidateTagName (name);
			CheckDisposed ();

			FlushWriterState ();

			html.Write ($"<{name}");
			WriterState = HtmlWriterState.Tag;
		}

		/// <summary>
		/// Write text to the output stream, escaping special characters.
		/// </summary>
		/// <remarks>
		/// Writes text to the output stream, escaping special characters.
		/// </remarks>
		/// <param name="buffer">The text buffer.</param>
		/// <param name="index">The index of the first character to write.</param>
		/// <param name="count">The number of characters to write.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero or greater than the length of
		/// <paramref name="buffer"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> and <paramref name="count"/> do not specify
		/// a valid range in the <paramref name="buffer"/>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="HtmlWriter"/> has been disposed.
		/// </exception>
		public void WriteText (char[] buffer, int index, int count)
		{
			ValidateArguments (buffer, index, count);
			CheckDisposed ();

			FlushWriterState ();

			if (count > 0)
				HtmlUtils.HtmlEncode (html, buffer, index, count);
		}

		/// <summary>
		/// Write text to the output stream, escaping special characters.
		/// </summary>
		/// <remarks>
		/// Writes text to the output stream, escaping special characters.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <param name="value">The text.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="HtmlWriter"/> has been disposed.
		/// </exception>
		public void WriteText (string value)
		{
			if (value is null)
				throw new ArgumentNullException (nameof (value));

			CheckDisposed ();

			FlushWriterState ();

			if (value.Length > 0)
				HtmlUtils.HtmlEncode (html, value.ToCharArray (), 0, value.Length);
		}

		/// <summary>
		/// Write text to the output stream, escaping special characters.
		/// </summary>
		/// <remarks>
		/// Writes text to the output stream, escaping special characters.
		/// </remarks>
		/// <param name="format">A composit format string.</param>
		/// <param name="args">An object array that contains zero or more objects to format.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="format"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="args"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="HtmlWriter"/> has been disposed.
		/// </exception>
		public void WriteText (string format, params object[] args)
		{
			WriteText (string.Format (format, args));
		}

		/// <summary>
		/// Write a token to the output stream.
		/// </summary>
		/// <remarks>
		/// Writes a token that was emitted by the <see cref="HtmlTokenizer"/>
		/// to the output stream.
		/// </remarks>
		/// <param name="token">The HTML token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="token"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="HtmlWriter"/> has been disposed.
		/// </exception>
		public void WriteToken (HtmlToken token)
		{
			if (token is null)
				throw new ArgumentNullException (nameof (token));

			CheckDisposed ();

			FlushWriterState ();

			token.WriteTo (html);
		}

		/// <summary>
		/// Flush any remaining state to the output stream.
		/// </summary>
		/// <remarks>
		/// Flushes any remaining state to the output stream.
		/// </remarks>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="HtmlWriter"/> has been disposed.
		/// </exception>
		public void Flush ()
		{
			CheckDisposed ();

			FlushWriterState ();

			html.Flush ();
		}

		/// <summary>
		/// Release the unmanaged resources used by the <see cref="HtmlWriter"/> and
		/// optionally releases the managed resources.
		/// </summary>
		/// <remarks>
		/// Releases any unmanaged resources used by the <see cref="HtmlWriter"/> and
		/// optionally releases the managed resources.
		/// </remarks>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
		/// <c>false</c> to release only the unmanaged resources.</param>
		protected virtual void Dispose (bool disposing)
		{
			if (disposing)
				html = null;
		}

		/// <summary>
		/// Release all resource used by the <see cref="HtmlWriter"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose()"/> when you are finished using the <see cref="HtmlWriter"/>. The
		/// <see cref="Dispose()"/> method leaves the <see cref="HtmlWriter"/> in an unusable state. After calling
		/// <see cref="Dispose()"/>, you must release all references to the <see cref="HtmlWriter"/> so the garbage
		/// collector can reclaim the memory that the <see cref="HtmlWriter"/> was occupying.</remarks>
		public void Dispose ()
		{
			Dispose (true);

			GC.SuppressFinalize (this);
		}
	}
}

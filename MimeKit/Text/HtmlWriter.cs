//
// HtmlWriter.cs
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
using System.IO;
using System.Text;
using System.Globalization;

#if PORTABLE
using Encoding = Portable.Text.Encoding;
#else
using Encoding = System.Text.Encoding;
#endif

namespace MimeKit.Text {
	/// <summary>
	/// An HTML tag callback delegate.
	/// </summary>
	/// <remarks>
	/// The <see cref="HtmlTagCallback"/> delegate is called when a converter
	/// is ready to write a new HTML tag, allowing developers to customize
	/// whether the tag gets written at all, which attributes get written, etc.
	/// </remarks>
	public delegate void HtmlTagCallback (HtmlTagContext tagContext, HtmlWriter htmlWriter);

	/// <summary>
	/// An HTML writer.
	/// </summary>
	public class HtmlWriter : IDisposable
	{
		TextWriter writer;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Text.HtmlWriter"/> class.
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
			if (stream == null)
				throw new ArgumentNullException ("stream");

			if (encoding == null)
				throw new ArgumentNullException ("encoding");

			writer = new StreamWriter (stream, encoding, 4096);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Text.HtmlWriter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="HtmlWriter"/>.
		/// </remarks>
		/// <param name="writer">The text writer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="writer"/> is <c>null</c>.
		/// </exception>
		public HtmlWriter (TextWriter writer)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");

			this.writer = writer;
		}

		/// <summary>
		/// Releas unmanaged resources and perform other cleanup operations before the
		/// <see cref="MimeKit.Text.HtmlWriter"/> is reclaimed by garbage collection.
		/// </summary>
		/// <remarks>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="MimeKit.Text.HtmlWriter"/> is reclaimed by garbage collection.
		/// </remarks>
		~HtmlWriter ()
		{
			Dispose (false);
		}

		void CheckDisposed ()
		{
			if (writer == null)
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
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (index < 0 || index > buffer.Length)
				throw new ArgumentOutOfRangeException ("index");

			if (count < 0 || index + count > buffer.Length)
				throw new ArgumentOutOfRangeException ("count");
		}

		static void ValidateAttributeName (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			if (name.Length == 0)
				throw new ArgumentException ("The attribute name cannot be empty.", "name");

			if (!HtmlUtils.IsValidAttributeName (name))
				throw new ArgumentException ("Invalid attribute name.", "name");
		}

		static void ValidateTagName (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			if (name.Length == 0)
				throw new ArgumentException ("The tag name cannot be empty.", "name");

			if (!HtmlUtils.IsValidTagName (name))
				throw new ArgumentException ("Invalid tag name.", "name");
		}

		static string QuoteAttributeValue (char[] value, int startIndex, int count)
		{
			if (count == 0)
				return "\"\"";

			var quoted = new StringBuilder ();

			quoted.Append ("\"");
			using (var writer = new StringWriter (quoted))
				HtmlUtils.HtmlEncodeAttribute (writer, value, startIndex, count);
			quoted.Append ("\"");

			return quoted.ToString ();
		}

		void EncodeAttributeName (string name)
		{
			if (WriterState == HtmlWriterState.Default)
				throw new InvalidOperationException ("Cannot write attributes in the Default state.");

			writer.Write (' ');
			writer.Write (name);
			WriterState = HtmlWriterState.Attribute;
		}

		void EncodeAttributeValue (char[] value, int startIndex, int count)
		{
			if (WriterState != HtmlWriterState.Attribute)
				throw new InvalidOperationException ("Attribute values can only be written in the Attribute state.");

			writer.Write ('=');
			writer.Write (QuoteAttributeValue (value, startIndex, count));
			WriterState = HtmlWriterState.Tag;
		}

		void EncodeAttributeValue (string value)
		{
			var buffer = value.ToCharArray ();

			EncodeAttributeValue (buffer, 0, buffer.Length);
		}

		void EncodeAttribute (string name, char[] value, int startIndex, int count)
		{
			EncodeAttributeName (name);
			EncodeAttributeValue (value, startIndex, count);
		}

		void EncodeAttribute (string name, string value)
		{
			var buffer = value.ToCharArray ();

			EncodeAttribute (name, buffer, 0, buffer.Length);
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
		/// <paramref name="id"/> is invalid.
		/// </exception>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero or greater than the length of <paramref name="buffer"/>.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="buffer"/> is not large enough to contain <paramref name="count"/> bytes starting
		/// at the specified <paramref name="index"/>.</para>
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
				throw new ArgumentException ("Invalid attribute.", "id");

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
		/// <para><paramref name="index"/> is less than zero or greater than the length of <paramref name="buffer"/>.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="buffer"/> is not large enough to contain <paramref name="count"/> bytes starting
		/// at the specified <paramref name="index"/>.</para>
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

		//public void WriteAttribute (HtmlAttributeReader attributeReader)
		//{
		//}

		/// <summary>
		/// Write the attribute to the output stream.
		/// </summary>
		/// <remarks>
		/// Writes the attribute to the output stream.
		/// </remarks>
		/// <param name="id">The attribute identifier.</param>
		/// <param name="value">The attribute value.</param>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="id"/> is invalid.
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
				throw new ArgumentException ("Invalid attribute.", "id");

			if (value == null)
				throw new ArgumentNullException ("value");

			CheckDisposed ();

			EncodeAttribute (id.ToAttributeName (), value);
		}

		/// <summary>
		/// Write the attribute to the output stream.
		/// </summary>
		/// <remarks>
		/// Writes the attribute to the output stream.
		/// </remarks>
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

			if (value == null)
				throw new ArgumentNullException ("value");

			CheckDisposed ();

			EncodeAttribute (name, value);
		}

		//public void WriteAttributeName (HtmlAttributeReader attributeReader)
		//{
		//}

		/// <summary>
		/// Write the attribute name to the output stream.
		/// </summary>
		/// <remarks>
		/// Writes the attribute name to the output stream.
		/// </remarks>
		/// <param name="id">The attribute identifier.</param>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="id"/> is invalid.
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
				throw new ArgumentException ("Invalid attribute.", "id");

			if (WriterState == HtmlWriterState.Default)
				throw new InvalidOperationException ("Cannot write attributes in the Default state.");

			CheckDisposed ();

			EncodeAttributeName (id.ToString ());
		}

		/// <summary>
		/// Write the attribute name to the output stream.
		/// </summary>
		/// <remarks>
		/// Writes the attribute name to the output stream.
		/// </remarks>
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

		public void WriteAttributeValue (char[] buffer, int index, int count)
		{
			ValidateArguments (buffer, index, count);
			CheckDisposed ();

			EncodeAttributeValue (buffer, index, count);
		}

		public void WriteAttributeValue (HtmlAttributeReader attributeReader)
		{
		}

		public void WriteAttributeValue (string value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			CheckDisposed ();

			EncodeAttributeValue (value);
		}

		public void WriteEmptyElementTag (HtmlTagId id)
		{
			if (id == HtmlTagId.Unknown)
				throw new ArgumentException ("Invalid tag.", "id");

			CheckDisposed ();

			if (WriterState == HtmlWriterState.Attribute)
				EncodeAttributeValue (string.Empty);

			if (WriterState != HtmlWriterState.Default) {
				WriterState = HtmlWriterState.Default;
				writer.Write (">");
			}

			writer.Write (string.Format ("<{0}/>", id.ToHtmlTagName ()));
		}

		public void WriteEmptyElementTag (string name)
		{
			ValidateTagName (name);
			CheckDisposed ();

			if (WriterState != HtmlWriterState.Default) {
				WriterState = HtmlWriterState.Default;
				writer.Write (">");
			}

			writer.Write (string.Format ("<{0}/>", name));
		}

		public void WriteEndTag (HtmlTagId id)
		{
			if (id == HtmlTagId.Unknown)
				throw new ArgumentException ("Invalid tag.", "id");

			CheckDisposed ();

			if (WriterState != HtmlWriterState.Default) {
				WriterState = HtmlWriterState.Default;
				writer.Write (">");
			}

			writer.Write (string.Format ("</{0}>", id.ToHtmlTagName ()));
		}

		public void WriteEndTag (string name)
		{
			ValidateTagName (name);
			CheckDisposed ();

			if (WriterState != HtmlWriterState.Default) {
				WriterState = HtmlWriterState.Default;
				writer.Write (">");
			}

			writer.Write (string.Format ("</{0}>", name));
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
		public void WriteMarkupText (char[] buffer, int index, int count)
		{
			ValidateArguments (buffer, index, count);
			CheckDisposed ();

			if (WriterState != HtmlWriterState.Default) {
				WriterState = HtmlWriterState.Default;
				writer.Write (">");
			}

			writer.Write (buffer, index, count);
		}

		//public void WriteMarkupText (HtmlReader reader)
		//{
		//}

		/// <summary>
		/// Write a string containing HTML markup directly to the output, without escaping special characters.
		/// </summary>
		/// <remarks>
		/// Writes a string containing HTML markup directly to the output, without escaping special characters.
		/// </remarks>
		/// <param name="value">The string containing HTML markup.</param>
		public void WriteMarkupText (string value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			CheckDisposed ();

			if (WriterState != HtmlWriterState.Default) {
				WriterState = HtmlWriterState.Default;
				writer.Write (">");
			}

			writer.Write (value);
		}

		public void WriteStartTag (HtmlTagId id)
		{
			if (id == HtmlTagId.Unknown)
				throw new ArgumentException ("Invalid tag.", "id");

			CheckDisposed ();

			if (WriterState != HtmlWriterState.Default)
				writer.Write (">");

			writer.Write (string.Format ("<{0}", id.ToHtmlTagName ()));
			WriterState = HtmlWriterState.Tag;
		}

		public void WriteStartTag (string name)
		{
			ValidateTagName (name);
			CheckDisposed ();

			if (WriterState != HtmlWriterState.Default)
				writer.Write (">");

			writer.Write (string.Format ("<{0}", name));
			WriterState = HtmlWriterState.Tag;
		}

		//public void WriteTag (HtmlReader reader)
		//{
		//}

		public void WriteText (char[] buffer, int index, int count)
		{
			ValidateArguments (buffer, index, count);
			CheckDisposed ();

			if (WriterState != HtmlWriterState.Default) {
				WriterState = HtmlWriterState.Default;
				writer.Write (">");
			}

			if (count > 0)
				HtmlUtils.HtmlEncode (writer, buffer, index, count);
		}

		//public void WriteText (HtmlReader reader)
		//{
		//}

		public void WriteText (string value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			CheckDisposed ();

			if (WriterState != HtmlWriterState.Default) {
				WriterState = HtmlWriterState.Default;
				writer.Write (">");
			}

			if (value.Length > 0)
				HtmlUtils.HtmlEncode (writer, value.ToCharArray (), 0, value.Length);
		}

		public void Flush ()
		{
			CheckDisposed ();

			if (WriterState != HtmlWriterState.Default) {
				WriterState = HtmlWriterState.Default;
				writer.Write (">");
			}

			writer.Flush ();
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="HtmlWriter"/> and
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
				writer = null;
		}

		/// <summary>
		/// Releases all resource used by the <see cref="MimeKit.Text.HtmlWriter"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="MimeKit.Text.HtmlWriter"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="MimeKit.Text.HtmlWriter"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the <see cref="MimeKit.Text.HtmlWriter"/> so the garbage
		/// collector can reclaim the memory that the <see cref="MimeKit.Text.HtmlWriter"/> was occupying.</remarks>
		public void Dispose ()
		{
			Dispose (true);

			GC.SuppressFinalize (this);
		}
	}
}

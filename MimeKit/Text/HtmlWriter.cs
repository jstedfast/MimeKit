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

#if PORTABLE
using Encoding = Portable.Text.Encoding;
#else
using Encoding = System.Text.Encoding;
#endif

namespace MimeKit.Text {
	public delegate void HtmlTagCallback (HtmlTagContext tagContext, HtmlWriter htmlWriter);

	public class HtmlWriter : IDisposable
	{
		TextWriter writer;

		public HtmlWriter (Stream stream, Encoding encoding)
		{
			writer = new StreamWriter (stream, encoding, 4096);
		}

		public HtmlWriter (TextWriter output)
		{
			writer = output;
		}

		~HtmlWriter ()
		{
			Dispose (false);
		}

		void CheckDisposed ()
		{
			if (writer == null)
				throw new ObjectDisposedException ("HtmlWriter");
		}

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

			for (int i = 0; i < name.Length; i++) {
				bool isAlpha = (name[i] >= 'A' && name[i] <= 'Z') || (name[i] >= 'a' && name[i] <= 'z');
				bool isDigit = name[i] >= '0' && name[i] <= '9';

				if ((!isAlpha && !isDigit) || (isDigit && i == 0))
					throw new ArgumentException ("Invalid attribute name.", "name");
			}
		}

		static void ValidateTagName (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			if (name.Length == 0)
				throw new ArgumentException ("The tag name cannot be empty.", "name");

			for (int i = 0; i < name.Length; i++) {
				bool isAlpha = (name[i] >= 'A' && name[i] <= 'Z') || (name[i] >= 'a' && name[i] <= 'z');
				bool isDigit = name[i] >= '0' && name[i] <= '9';

				if ((!isAlpha && !isDigit) || (isDigit && i == 0))
					throw new ArgumentException ("Invalid tag name.", "name");
			}
		}

		static string QuoteAttributeValue (string value)
		{
			if (value.Length == 0)
				return "\"\"";

			var quoted = new StringBuilder ();

			quoted.Append ("\"");
			for (int i = 0; i < value.Length; i++) {
				char c = value[i];

				switch (c) {
				case '"': quoted.Append ("&quot;"); break;
				case '&': quoted.Append ("&amp;"); break;
				case '<': quoted.Append ("&lt;"); break;
				case '>': quoted.Append ("&gt;"); break;
				default:
					if (c < 32 || c >= 127)
						quoted.AppendFormat ("&#{0:D};", c);
					else
						quoted.Append (c);
					break;
				}
			}
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

		void EncodeAttributeValue (string value)
		{
			if (WriterState != HtmlWriterState.Attribute)
				throw new InvalidOperationException ("Attribute values can only be written in the Attribute state.");

			writer.Write ('=');
			writer.Write (QuoteAttributeValue (value));
			WriterState = HtmlWriterState.Tag;
		}

		void EncodeAttribute (string name, string value)
		{
			EncodeAttributeName (name);
			EncodeAttributeValue (value);
		}

		public void WriteAttribute (HtmlAttributeId id, char[] buffer, int index, int count)
		{
			if (id == HtmlAttributeId.Unknown)
				throw new ArgumentException ("Invalid attribute.", "id");

			ValidateArguments (buffer, index, count);

			EncodeAttribute (id.ToAttributeName (), new string (buffer, index, count));
		}

		public void WriteAttribute (string name, char[] buffer, int index, int count)
		{
			ValidateAttributeName (name);
			ValidateArguments (buffer, index, count);

			EncodeAttribute (name, new string (buffer, index, count));
		}

		public void WriteAttribute (HtmlAttributeReader attributeReader)
		{
		}

		public void WriteAttribute (HtmlAttributeId id, string value)
		{
			if (id == HtmlAttributeId.Unknown)
				throw new ArgumentException ("Invalid attribute.", "id");

			if (value == null)
				throw new ArgumentNullException ("value");

			EncodeAttribute (id.ToAttributeName (), value);
		}

		public void WriteAttribute (string name, string value)
		{
			ValidateAttributeName (name);

			if (value == null)
				throw new ArgumentNullException ("value");

			EncodeAttribute (name, value);
		}

		public void WriteAttributeName (HtmlAttributeReader attributeReader)
		{
		}

		public void WriteAttributeName (HtmlAttributeId id)
		{
			if (id == HtmlAttributeId.Unknown)
				throw new ArgumentException ("Invalid attribute.", "id");

			if (WriterState == HtmlWriterState.Default)
				throw new InvalidOperationException ("Cannot write attributes in the Default state.");

			EncodeAttributeName (id.ToString ());
		}

		public void WriteAttributeName (string name)
		{
			ValidateAttributeName (name);

			if (WriterState == HtmlWriterState.Default)
				throw new InvalidOperationException ("Cannot write attributes in the Default state.");

			EncodeAttributeName (name);
		}

		public void WriteAttributeValue (char[] buffer, int index, int count)
		{
			ValidateArguments (buffer, index, count);

			EncodeAttributeValue (new string (buffer, index, count));
		}

		public void WriteAttributeValue (HtmlAttributeReader attributeReader)
		{
		}

		public void WriteAttributeValue (string value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			EncodeAttributeValue (value);
		}

		public void WriteEmptyElementTag (HtmlTagId id)
		{
			if (id == HtmlTagId.Unknown)
				throw new ArgumentException ("Invalid tag.", "id");

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

			if (WriterState == HtmlWriterState.Attribute)
				EncodeAttributeValue (string.Empty);

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

			if (WriterState == HtmlWriterState.Attribute)
				EncodeAttributeValue (string.Empty);

			if (WriterState != HtmlWriterState.Default) {
				WriterState = HtmlWriterState.Default;
				writer.Write (">");
			}

			writer.Write (string.Format ("</{0}>", id.ToHtmlTagName ()));
		}

		public void WriteEndTag (string name)
		{
			ValidateTagName (name);

			if (WriterState == HtmlWriterState.Attribute)
				EncodeAttributeValue (string.Empty);

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

			if (WriterState == HtmlWriterState.Attribute)
				EncodeAttributeValue (string.Empty);

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

			if (WriterState == HtmlWriterState.Attribute)
				EncodeAttributeValue (string.Empty);

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

			if (WriterState == HtmlWriterState.Attribute)
				EncodeAttributeValue (string.Empty);

			if (WriterState != HtmlWriterState.Default)
				writer.Write (">");

			writer.Write (string.Format ("<{0}", id.ToHtmlTagName ()));
			WriterState = HtmlWriterState.Tag;
		}

		public void WriteStartTag (string name)
		{
			ValidateTagName (name);

			if (WriterState == HtmlWriterState.Attribute)
				EncodeAttributeValue (string.Empty);

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

			if (WriterState == HtmlWriterState.Attribute)
				EncodeAttributeValue (string.Empty);

			if (WriterState != HtmlWriterState.Default) {
				WriterState = HtmlWriterState.Default;
				writer.Write (">");
			}

			// TODO: escape the text
			writer.Write (buffer, index, count);
		}

		//public void WriteText (HtmlReader reader)
		//{
		//}

		public void WriteText (string value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			if (WriterState == HtmlWriterState.Attribute)
				EncodeAttributeValue (string.Empty);

			if (WriterState != HtmlWriterState.Default) {
				WriterState = HtmlWriterState.Default;
				writer.Write (">");
			}

			// TODO: escape the text
			writer.Write (value);
		}

		public void Flush ()
		{
			CheckDisposed ();

			writer.Flush ();
		}

		public void Close ()
		{
			CheckDisposed ();

			writer = null;
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
				Close ();
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

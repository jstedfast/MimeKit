//
// TextPart.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Jeffrey Stedfast
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
using System.Collections.Generic;

using MimeKit.IO;
using MimeKit.IO.Filters;
using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// A Textual MIME part.
	/// </summary>
	public class TextPart : MimePart
	{
		internal TextPart (ParserOptions options, ContentType type, IEnumerable<Header> headers, bool toplevel) : base (options, type, headers, toplevel)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.TextPart"/>
		/// class with the specified text subtype.
		/// </summary>
		/// <param name="subtype">The media subtype.</param>
		/// <param name="args">An array of initialization parameters: headers, message encoding and text.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="subtype"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="args"/> is <c>null</c>.</para>
		/// </exception>
		public TextPart (string subtype, params object[] args) : this (subtype)
		{
			if (args == null)
				throw new ArgumentNullException ("args");

			// Default to UTF8 if not given.
			Encoding encoding = null;

			// Used only in case some text is provided.
			StringBuilder sb = null;

			foreach (object obj in args) {
				if (obj == null || base.TryInit (obj))
					continue;

				Encoding e = obj as Encoding;
				if (e != null) {
					if (encoding != null)
						throw new ArgumentException ("Encoding should not be specified more than once.");
					encoding = e;
					continue;
				}

				string text = obj as string;
				if (text != null) {
					if (sb == null)
						sb = new StringBuilder ();
					sb.Append (text);
					continue;
				}

				throw new ArgumentException ("Unknown initialization parameter: " + obj.GetType ());
			}

			if (sb != null)
				SetText (encoding ?? Encoding.UTF8, sb.ToString ());
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.TextPart"/>
		/// class with the specified text subtype.
		/// </summary>
		/// <param name="subtype">The media subtype.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="subtype"/> is <c>null</c>.
		/// </exception>
		public TextPart (string subtype) : base ("text", subtype)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.TextPart"/>
		/// class with a Content-Type of text/plain.
		/// </summary>
		public TextPart () : base ("text", "plain")
		{
		}

		/// <summary>
		/// Gets the decoded text content.
		/// </summary>
		/// <value>The text.</value>
		public string Text {
			get {
				var charset = ContentType.Parameters["charset"];
				byte[] content;

				using (var memory = new MemoryStream ()) {
					ContentObject.DecodeTo (memory);
					content = memory.ToArray ();
				}

				Encoding encoding = null;

				if (charset != null) {
					try {
						encoding = CharsetUtils.GetEncoding (charset);
					} catch (NotSupportedException) {
					}
				}

				if (encoding == null)
					encoding = Encoding.GetEncoding (28591); // iso-8859-1

				return encoding.GetString (content);
			}
		}

		/// <summary>
		/// Gets the decoded text content using the provided charset to override
		/// the charset specified in the Content-Type parameters.
		/// </summary>
		/// <returns>The decoded text.</returns>
		/// <param name="charset">The charset encoding to use.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="charset"/> is <c>null</c>.
		/// </exception>
		public string GetText (Encoding charset)
		{
			if (charset == null)
				throw new ArgumentNullException ("charset");

			using (var memory = new MemoryStream ()) {
				using (var filtered = new FilteredStream (memory)) {
					filtered.Add (new CharsetFilter (charset, Encoding.UTF8));

					ContentObject.DecodeTo (filtered);
					filtered.Flush ();

					return Encoding.UTF8.GetString (memory.GetBuffer (), 0, (int) memory.Length);
				}
			}
		}

		/// <summary>
		/// Sets the text content and the charset parameter in the Content-Type header.
		/// </summary>
		/// <param name="charset">The charset encoding.</param>
		/// <param name="text">The text content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="charset"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="text"/> is <c>null</c>.</para>
		/// </exception>
		public void SetText (Encoding charset, string text)
		{
			if (charset == null)
				throw new ArgumentNullException ("charset");

			if (text == null)
				throw new ArgumentNullException ("text");

			var content = new MemoryStream (charset.GetBytes (text));
			ContentObject = new ContentObject (content, ContentEncoding.Default);
			ContentType.Parameters["charset"] = CharsetUtils.GetMimeCharset (charset);
		}
	}
}

//
// MultipartAlternative.cs
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

using MimeKit.Text;

namespace MimeKit {
	/// <summary>
	/// A multipart/alternative MIME entity.
	/// </summary>
	/// <remarks>
	/// A multipart/alternative MIME entity contains, as one might expect, is used to offer a list of
	/// alternative formats for the main body of the message (usually they will be "text/plain" and
	/// "text/html"). These alternatives are in order of increasing faithfulness to the original document
	/// (in other words, the last entity will be in a format that, when rendered, will most closely match
	/// what the sending client's WYSISYG editor produced).
	/// </remarks>
	public class MultipartAlternative : Multipart
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MultipartAlternative"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is used by <see cref="MimeKit.MimeParser"/>.
		/// </remarks>
		/// <param name="args">Information used by the constructor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		public MultipartAlternative (MimeEntityConstructorArgs args) : base (args)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MultipartAlternative"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MultipartAlternative"/> part.
		/// </remarks>
		/// <param name="args">An array of initialization parameters: headers and MIME entities.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="args"/> contains one or more arguments of an unknown type.
		/// </exception>
		public MultipartAlternative (params object[] args) : base ("alternative", args)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MultipartAlternative"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MultipartAlternative"/> part.
		/// </remarks>
		public MultipartAlternative () : base ("alternative")
		{
		}

		/// <summary>
		/// Get the text of the text/plain alternative.
		/// </summary>
		/// <remarks>
		/// Gets the text of the text/plain alternative, if it exists.
		/// </remarks>
		/// <value>The text if a text/plain alternative exists; otherwise, <c>null</c>.</value>
		public string TextBody {
			get { return GetTextBody (TextFormat.Plain); }
		}

		/// <summary>
		/// Get the HTML-formatted text of the text/html alternative.
		/// </summary>
		/// <remarks>
		/// Gets the HTML-formatted text of the text/html alternative, if it exists.
		/// </remarks>
		/// <value>The HTML if a text/html alternative exists; otherwise, <c>null</c>.</value>
		public string HtmlBody {
			get { return GetTextBody (TextFormat.Html); }
		}

		/// <summary>
		/// Dispatches to the specific visit method for this MIME entity.
		/// </summary>
		/// <remarks>
		/// This default implementation for <see cref="MimeKit.MultipartAlternative"/> nodes
		/// calls <see cref="MimeKit.MimeVisitor.VisitMultipartAlternative"/>. Override this
		/// method to call into a more specific method on a derived visitor class
		/// of the <see cref="MimeKit.MimeVisitor"/> class. However, it should still
		/// support unknown visitors by calling
		/// <see cref="MimeKit.MimeVisitor.VisitMultipartAlternative"/>.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <c>null</c>.
		/// </exception>
		public override void Accept (MimeVisitor visitor)
		{
			if (visitor == null)
				throw new ArgumentNullException (nameof (visitor));

			visitor.VisitMultipartAlternative (this);
		}

		internal static string GetText (TextPart text)
		{
			if (text.IsFlowed) {
				var converter = new FlowedToText ();
				string delsp;

				if (text.ContentType.Parameters.TryGetValue ("delsp", out delsp))
					converter.DeleteSpace = delsp.ToLowerInvariant () == "yes";

				return converter.Convert (text.Text);
			}

			return text.Text;
		}

		/// <summary>
		/// Get the text body in the specified format.
		/// </summary>
		/// <remarks>
		/// Gets the text body in the specified format, if it exists.
		/// </remarks>
		/// <returns>The text body in the desired format if it exists; otherwise, <c>null</c>.</returns>
		/// <param name="format">The desired text format.</param>
		public string GetTextBody (TextFormat format)
		{
			// walk the multipart/alternative children backwards from greatest level of faithfulness to the least faithful
			for (int i = Count - 1; i >= 0; i--) {
				var alternative = this[i] as MultipartAlternative;

				if (alternative != null) {
					// Note: nested multipart/alternative parts make no sense... yet here we are.
					return alternative.GetTextBody (format);
				}

				var related = this[i] as MultipartRelated;
				var text = this[i] as TextPart;

				if (related != null) {
					var root = related.Root;

					alternative = root as MultipartAlternative;
					if (alternative != null)
						return alternative.GetTextBody (format);

					text = root as TextPart;
				}

				if (text != null && text.IsFormat (format))
					return GetText (text);
			}

			return null;
		}
	}
}

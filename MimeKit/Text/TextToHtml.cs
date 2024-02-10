//
// TextToHtml.cs
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
	/// A text to HTML converter.
	/// </summary>
	/// <remarks>
	/// Used to convert plain text into HTML.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
	/// </example>
	public class TextToHtml : TextConverter
	{
		readonly UrlScanner scanner;

		/// <summary>
		/// Initialize a new instance of the <see cref="TextToHtml"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new text to HTML converter.
		/// </remarks>
		public TextToHtml ()
		{
			scanner = new UrlScanner ();

			for (int i = 0; i < UrlPatterns.Count; i++)
				scanner.Add (UrlPatterns[i]);
		}

		/// <summary>
		/// Get the input format.
		/// </summary>
		/// <remarks>
		/// Gets the input format.
		/// </remarks>
		/// <value>The input format.</value>
		public override TextFormat InputFormat {
			get { return TextFormat.Plain; }
		}

		/// <summary>
		/// Get the output format.
		/// </summary>
		/// <remarks>
		/// Gets the output format.
		/// </remarks>
		/// <value>The output format.</value>
		public override TextFormat OutputFormat {
			get { return TextFormat.Html; }
		}

		/// <summary>
		/// Get or set the footer format.
		/// </summary>
		/// <remarks>
		/// Gets or sets the footer format.
		/// </remarks>
		/// <value>The footer format.</value>
		public HeaderFooterFormat FooterFormat {
			get; set;
		}

		/// <summary>
		/// Get or set the header format.
		/// </summary>
		/// <remarks>
		/// Gets or sets the header format.
		/// </remarks>
		/// <value>The header format.</value>
		public HeaderFooterFormat HeaderFormat {
			get; set;
		}

		/// <summary>
		/// Get or set the <see cref="HtmlTagCallback"/> method to use for custom
		/// filtering of HTML tags and content.
		/// </summary>
		/// <remarks>
		/// Get or set the <see cref="HtmlTagCallback"/> method to use for custom
		/// filtering of HTML tags and content.
		/// </remarks>
		/// <value>The html tag callback.</value>
		public HtmlTagCallback HtmlTagCallback {
			get; set;
		}

		/// <summary>
		/// Get or set whether or not the converter should only output an HTML fragment.
		/// </summary>
		/// <remarks>
		/// Gets or sets whether or not the converter should only output an entire
		/// HTML document or just a fragment of the HTML body content.
		/// </remarks>
		/// <value><c>true</c> if the converter should only output an HTML fragment; otherwise, <c>false</c>.</value>
		public bool OutputHtmlFragment {
			get; set;
		}

		class TextToHtmlTagContext : HtmlTagContext
		{
			readonly HtmlAttributeCollection attributes;
			bool isEndTag;

			public TextToHtmlTagContext (HtmlTagId tag, HtmlAttribute attr) : base (tag)
			{
				attributes = new HtmlAttributeCollection (new [] { attr });
			}

			public TextToHtmlTagContext (HtmlTagId tag) : base (tag)
			{
				attributes = HtmlAttributeCollection.Empty;
			}

			public override string TagName {
				get { return TagId.ToHtmlTagName (); }
			}

			public override HtmlAttributeCollection Attributes {
				get { return attributes; }
			}

			public override bool IsEmptyElementTag {
				get { return TagId == HtmlTagId.Br; }
			}

			public override bool IsEndTag {
				get { return isEndTag; }
			}

			public void SetIsEndTag (bool value)
			{
				isEndTag = value;
			}
		}

		static void DefaultHtmlTagCallback (HtmlTagContext tagContext, HtmlWriter htmlWriter)
		{
			tagContext.WriteTag (htmlWriter, true);
		}

		static string Unquote (string line, out int quoteDepth)
		{
			int index = 0;

			quoteDepth = 0;

			if (line.Length == 0 || line[0] != '>')
				return line;

			do {
				quoteDepth++;
				index++;

				if (index < line.Length && line[index] == ' ')
					index++;
			} while (index < line.Length && line[index] == '>');

			return line.Substring (index);
		}

		static bool SuppressContent (IList<TextToHtmlTagContext> stack)
		{
			for (int i = stack.Count; i > 0; i--) {
				if (stack[i - 1].SuppressInnerContent)
					return true;
			}

			return false;
		}

		void WriteText (HtmlWriter htmlWriter, string text)
		{
			var callback = HtmlTagCallback ?? DefaultHtmlTagCallback;
			var content = text.ToCharArray ();
			int endIndex = content.Length;
			int startIndex = 0;
			int count;

			do {
				count = endIndex - startIndex;

				if (scanner.Scan (content, startIndex, count, out var match)) {
					count = match.EndIndex - match.StartIndex;

					if (match.StartIndex > startIndex) {
						// write everything up to the match
						htmlWriter.WriteText (content, startIndex, match.StartIndex - startIndex);
					}

					var href = match.Prefix + new string (content, match.StartIndex, count);
					var ctx = new TextToHtmlTagContext (HtmlTagId.A, new HtmlAttribute (HtmlAttributeId.Href, href));
					callback (ctx, htmlWriter);

					if (!ctx.SuppressInnerContent)
						htmlWriter.WriteText (content, match.StartIndex, count);

					if (!ctx.DeleteEndTag) {
						ctx.SetIsEndTag (true);

						if (ctx.InvokeCallbackForEndTag)
							callback (ctx, htmlWriter);
						else
							ctx.WriteTag (htmlWriter);
					}

					startIndex = match.EndIndex;
				} else {
					htmlWriter.WriteText (content, startIndex, count);
					break;
				}
			} while (startIndex < endIndex);
		}

		/// <summary>
		/// Convert the contents of <paramref name="reader"/> from the <see cref="InputFormat"/> to the
		/// <see cref="OutputFormat"/> and uses the <paramref name="writer"/> to write the resulting text.
		/// </summary>
		/// <remarks>
		/// Converts the contents of <paramref name="reader"/> from the <see cref="InputFormat"/> to the
		/// <see cref="OutputFormat"/> and uses the <paramref name="writer"/> to write the resulting text.
		/// </remarks>
		/// <param name="reader">The text reader.</param>
		/// <param name="writer">The text writer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="reader"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="writer"/> is <c>null</c>.</para>
		/// </exception>
		public override void Convert (TextReader reader, TextWriter writer)
		{
			if (reader is null)
				throw new ArgumentNullException (nameof (reader));

			if (writer is null)
				throw new ArgumentNullException (nameof (writer));

			if (!OutputHtmlFragment)
				writer.Write ("<html><body>");

			if (!string.IsNullOrEmpty (Header)) {
				if (HeaderFormat == HeaderFooterFormat.Text) {
					var converter = new TextToHtml { OutputHtmlFragment = true };

					using (var sr = new StringReader (Header))
						converter.Convert (sr, writer);
				} else {
					writer.Write (Header);
				}
			}

			using (var htmlWriter = new HtmlWriter (writer)) {
				var callback = HtmlTagCallback ?? DefaultHtmlTagCallback;
				var stack = new List<TextToHtmlTagContext> ();
				int currentQuoteDepth = 0;
				TextToHtmlTagContext ctx;
				string line;

				while ((line = reader.ReadLine ()) != null) {
					line = Unquote (line, out int quoteDepth);

					while (currentQuoteDepth < quoteDepth) {
						ctx = new TextToHtmlTagContext (HtmlTagId.BlockQuote);
						callback (ctx, htmlWriter);
						currentQuoteDepth++;
						stack.Add (ctx);
					}

					while (quoteDepth < currentQuoteDepth) {
						ctx = stack[stack.Count - 1];
						stack.RemoveAt (stack.Count - 1);

						if (!SuppressContent (stack) && !ctx.DeleteEndTag) {
							ctx.SetIsEndTag (true);

							if (ctx.InvokeCallbackForEndTag)
								callback (ctx, htmlWriter);
							else
								ctx.WriteTag (htmlWriter);
						}

						if (ctx.TagId == HtmlTagId.BlockQuote)
							currentQuoteDepth--;
					}

					if (!SuppressContent (stack)) {
						WriteText (htmlWriter, line);

						ctx = new TextToHtmlTagContext (HtmlTagId.Br);
						callback (ctx, htmlWriter);
					}
				}

				for (int i = stack.Count; i > 0; i--) {
					ctx = stack[i - 1];

					ctx.SetIsEndTag (true);

					if (ctx.InvokeCallbackForEndTag)
						callback (ctx, htmlWriter);
					else
						ctx.WriteTag (htmlWriter);
				}

				htmlWriter.Flush ();
			}

			if (!string.IsNullOrEmpty (Footer)) {
				if (FooterFormat == HeaderFooterFormat.Text) {
					var converter = new TextToHtml { OutputHtmlFragment = true };

					using (var sr = new StringReader (Footer))
						converter.Convert (sr, writer);
				} else {
					writer.Write (Footer);
				}
			}

			if (!OutputHtmlFragment)
				writer.Write ("</body></html>");
		}
	}
}

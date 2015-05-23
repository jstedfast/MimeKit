﻿//
// FlowedToHtml.cs
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
using System.Collections;
using System.Collections.Generic;

namespace MimeKit.Text {
	/// <summary>
	/// A flowed text to HTML converter.
	/// </summary>
	/// <remarks>
	/// Used to convert flowed text (as described in rfc3676) into HTML.
	/// </remarks>
	public class FlowedToHtml : TextConverter
	{
		readonly UrlScanner scanner;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Text.FlowedToHtml"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new flowed text to HTML converter.
		/// </remarks>
		public FlowedToHtml ()
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
			get { return TextFormat.Flowed; }
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
		/// Get or set whether the trailing space on a wrapped line should be deleted.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets whether the trailing space on a wrapped line should be deleted.</para>
		/// <para>The flowed text format defines a Content-Type parameter called "delsp" which can
		/// have a value of "yes" or "no". If the parameter exists and the value is "yes", then
		/// <see cref="DeleteSpace"/> should be set to <c>true</c>, otherwise <see cref="DeleteSpace"/>
		/// should be set to false.</para>
		/// </remarks>
		/// <value><c>true</c> if the trailing space on a wrapped line should be deleted; otherwise, <c>false</c>.</value>
		public bool DeleteSpace {
			get; set;
		}

		/// <summary>
		/// Get or set the text that will be appended to the end of the output.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the text that will be appended to the end of the output.</para>
		/// <para>The footer must be set before conversion begins.</para>
		/// </remarks>
		/// <value>The footer.</value>
		public string Footer {
			get; set;
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
		/// Get or set text that will be prepended to the beginning of the output.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the text that will be prepended to the beginning of the output.</para>
		/// <para>The header must be set before conversion begins.</para>
		/// </remarks>
		/// <value>The header.</value>
		public string Header {
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
		/// Gets or sets whether or not the converter should only output an HTML fragment.
		/// </remarks>
		/// <value><c>true</c> if the converter should only output an HTML fragment; otherwise, <c>false</c>.</value>
		public bool OutputHtmlFragment {
			get; set;
		}

		class FlowedToHtmlTagContext : HtmlTagContext
		{
			HtmlAttributeCollection attrs;

			public FlowedToHtmlTagContext (HtmlTagId tag, HtmlAttribute attr) : base (tag)
			{
				attrs = new HtmlAttributeCollection (new [] { attr });
			}

			public FlowedToHtmlTagContext (HtmlTagId tag) : base (tag)
			{
				attrs = HtmlAttributeCollection.Empty;
			}

			public override HtmlAttributeCollection Attributes {
				get { return attrs; }
			}

			public override bool IsEmptyElementTag {
				get { return TagId == HtmlTagId.Br; }
			}

			public void SetIsEndTag (bool value)
			{
				IsEndTag = value;
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

			if (line.Length == 0)
				return line;

			while (index < line.Length && line[index] == '>') {
				quoteDepth++;
				index++;
			}

			if (index > 0 && index < line.Length && line[index] == ' ')
				index++;

			return index > 0 ? line.Substring (index) : line;
		}

		static bool SuppressContent (IList<FlowedToHtmlTagContext> stack)
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
			UrlMatch match;
			int count;

			do {
				count = endIndex - startIndex;

				if (scanner.Scan (content, startIndex, count, out match)) {
					count = match.EndIndex - match.StartIndex;

					if (match.StartIndex > startIndex) {
						// write everything up to the match
						htmlWriter.WriteText (content, startIndex, match.StartIndex - startIndex);
					}

					var href = match.Prefix + new string (content, match.StartIndex, count);
					var ctx = new FlowedToHtmlTagContext (HtmlTagId.A, new HtmlAttribute (HtmlAttributeId.Href, href));
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

		void WriteParagraph (HtmlWriter htmlWriter, IList<FlowedToHtmlTagContext> stack, ref int currentQuoteDepth, StringBuilder para, int quoteDepth)
		{
			var callback = HtmlTagCallback ?? DefaultHtmlTagCallback;
			FlowedToHtmlTagContext ctx;

			while (currentQuoteDepth < quoteDepth) {
				ctx = new FlowedToHtmlTagContext (HtmlTagId.BlockQuote);
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

			if (SuppressContent (stack))
				return;

			ctx = new FlowedToHtmlTagContext (para.Length == 0 ? HtmlTagId.Br : HtmlTagId.P);
			callback (ctx, htmlWriter);

			if (para.Length > 0) {
				if (!ctx.SuppressInnerContent)
					WriteText (htmlWriter, para.ToString ());

				if (!ctx.DeleteEndTag) {
					ctx.SetIsEndTag (true);

					if (ctx.InvokeCallbackForEndTag)
						callback (ctx, htmlWriter);
					else
						ctx.WriteTag (htmlWriter);
				}
			}

			if (!ctx.DeleteTag)
				htmlWriter.WriteMarkupText (Environment.NewLine);
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
			if (reader == null)
				throw new ArgumentNullException ("reader");

			if (writer == null)
				throw new ArgumentNullException ("writer");

			if (!string.IsNullOrEmpty (Header)) {
				if (HeaderFormat == HeaderFooterFormat.Text) {
					var converter = new TextToHtml ();

					using (var sr = new StringReader (Header))
						converter.Convert (sr, writer);
				} else {
					writer.Write (Header);
				}
			}

			using (var htmlWriter = new HtmlWriter (writer)) {
				var callback = HtmlTagCallback ?? DefaultHtmlTagCallback;
				var stack = new List<FlowedToHtmlTagContext> ();
				var para = new StringBuilder ();
				int currentQuoteDepth = 0;
				int paraQuoteDepth = -1;
				int quoteDepth;
				string line;

				while ((line = reader.ReadLine ()) != null) {
					// unquote the line
					line = Unquote (line, out quoteDepth);

					// remove space-stuffing
					if (line.Length > 0 && line[0] == ' ')
						line = line.Substring (1);

					if (para.Length == 0) {
						paraQuoteDepth = quoteDepth;
					} else if (quoteDepth != paraQuoteDepth) {
						// Note: according to rfc3676, when a folded line has a different quote
						// depth than the previous line, then quote-depth rules win and we need
						// to treat this as a new paragraph.
						WriteParagraph (htmlWriter, stack, ref currentQuoteDepth, para, paraQuoteDepth);
						paraQuoteDepth = quoteDepth;
						para.Length = 0;
					}

					para.Append (line);

					if (line.Length == 0 || line[line.Length - 1] != ' ') {
						// line did not end with a space, so the next line will start a new paragraph
						WriteParagraph (htmlWriter, stack, ref currentQuoteDepth, para, paraQuoteDepth);
						paraQuoteDepth = 0;
						para.Length = 0;
					} else if (DeleteSpace) {
						// Note: lines ending with a space mean that the next line is a continuation
						para.Length--;
					}
				}

				for (int i = stack.Count; i > 0; i--) {
					var ctx = stack[i - 1];

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
					var converter = new TextToHtml ();

					using (var sr = new StringReader (Footer))
						converter.Convert (sr, writer);
				} else {
					writer.Write (Footer);
				}
			}
		}
	}
}

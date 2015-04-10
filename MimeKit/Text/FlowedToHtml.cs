//
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
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Text.FlowedToHtml"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new flowed text to HTML converter.
		/// </remarks>
		public FlowedToHtml ()
		{
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
			public FlowedToHtmlTagContext (HtmlTagId tag) : base (tag)
			{
			}

			public override HtmlAttributeCollection Attributes {
				get { return HtmlAttributeCollection.Empty; }
			}

			public override bool IsEmptyElementTag {
				get { return false; }
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

		static bool SuppressContent (IList<FlowedToHtmlTagContext> stack)
		{
			for (int i = stack.Count; i > 0; i--) {
				if (stack[i - 1].SuppressInnerContent)
					return true;
			}

			return false;
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
				FlowedToHtmlTagContext ctx;
				int currentQuoteDepth = 0;
				string line;

				while ((line = reader.ReadLine ()) != null) {
					var stuffed = line.Length > 0 && line[0] == ' ';

					if (stuffed)
						line = line.Substring (1);

					para.Append (line);

					if (line.Length == 0 || line[line.Length - 1] != ' ') {
						// line did not end with a space, so the next line will start a new paragraph
						int quoteDepth;

						line = Unquote (para.ToString (), out quoteDepth);

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

						if (!SuppressContent (stack)) {
							ctx = new FlowedToHtmlTagContext (HtmlTagId.P);
							callback (ctx, htmlWriter);

							if (!ctx.SuppressInnerContent)
								htmlWriter.WriteText (line);

							if (!ctx.DeleteEndTag) {
								ctx.SetIsEndTag (true);

								if (ctx.InvokeCallbackForEndTag)
									callback (ctx, htmlWriter);
								else
									ctx.WriteTag (htmlWriter);
							}
						}

						para.Clear ();
					} else if (DeleteSpace) {
						// Note: lines ending with a space mean that the next line is a continuation
						para.Length--;
					}
				}
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

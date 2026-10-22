//
// HtmlPreviewVisitor.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
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

using AngleSharp.Css.Dom;
using Ganss.Xss;

using MimeKit;
using MimeKit.Text;
using MimeKit.Tnef;

namespace MessageReader
{
	/// <summary>
	/// Visits a MimeMessage and generates HTML suitable to be rendered by a browser control.
	/// </summary>
	class HtmlPreviewVisitor : MimeVisitor
	{
		readonly List<MultipartRelated> stack = new List<MultipartRelated> ();
		readonly List<MimeEntity> attachments = new List<MimeEntity> ();
		readonly List<MimeEntity> calenderAttachments = new List<MimeEntity> ();
		readonly HtmlSanitizer sanitizer = new HtmlSanitizer ();

		string body;

		/// <summary>
		/// Creates a new HtmlPreviewVisitor.
		/// </summary>
		public HtmlPreviewVisitor ()
		{
			// This is a read-only email preview, not an interactive web page.
			sanitizer.AllowedTags.Clear ();
			sanitizer.AllowedTags.UnionWith (new[] {
				"html", "head", "body", "style", "a", "abbr", "b", "blockquote", "br",
				"caption", "center", "cite", "code", "col", "colgroup", "dd", "del",
				"div", "dl", "dt", "em", "font", "h1", "h2", "h3", "h4", "h5", "h6",
				"hr", "i", "img", "ins", "li", "ol", "p", "pre", "s", "small", "span",
				"strike", "strong", "sub", "sup", "table", "tbody", "td", "tfoot",
				"th", "thead", "tr", "u", "ul"
			});

			sanitizer.AllowedAttributes.Clear ();
			sanitizer.AllowedAttributes.UnionWith (new[] {
				"align", "alt", "bgcolor", "border", "cellpadding", "cellspacing",
				"class", "color", "colspan", "dir", "face", "height", "lang",
				"rowspan", "size", "span", "src", "start", "style", "title",
				"type", "valign", "width"
			});

			// Keep formatting, but not properties capable of loading images, fonts or other resources.
			sanitizer.AllowedCssProperties.Clear ();
			sanitizer.AllowedCssProperties.UnionWith (new[] {
				"background-color", "border", "border-bottom", "border-bottom-color",
				"border-bottom-style", "border-bottom-width", "border-collapse", "border-color",
				"border-left", "border-left-color", "border-left-style", "border-left-width",
				"border-right", "border-right-color", "border-right-style", "border-right-width",
				"border-spacing", "border-style", "border-top", "border-top-color",
				"border-top-style", "border-top-width", "border-width", "color", "direction",
				"display", "float", "font", "font-family", "font-size", "font-style",
				"font-variant", "font-weight", "height", "letter-spacing", "line-height",
				"list-style-position", "list-style-type", "margin", "margin-bottom",
				"margin-left", "margin-right", "margin-top", "max-height", "max-width",
				"min-height", "min-width", "padding", "padding-bottom", "padding-left",
				"padding-right", "padding-top", "table-layout", "text-align",
				"text-decoration", "text-indent", "text-transform", "vertical-align",
				"white-space", "width", "word-break", "word-spacing", "word-wrap"
			});
			sanitizer.AllowedAtRules.Clear ();
			sanitizer.AllowedAtRules.Add (CssRuleType.Style);
			sanitizer.AllowedAtRules.Add (CssRuleType.Media);
			sanitizer.AllowCssCustomProperties = false;
			sanitizer.AllowDataAttributes = false;
			sanitizer.KeepChildNodes = false;

			// FilterUrl also runs for rejected schemes. Only our MIME image resolver may supply a URL.
			sanitizer.AllowedSchemes.Clear ();
			sanitizer.FilterUrl += FilterUrl;
		}

		/// <summary>
		/// The list of attachments that were in the MimeMessage.
		/// </summary>
		public IList<MimeEntity> Attachments {
			get { return attachments; }
		}

		/// <summary>
		/// The list of text/calender entries that were in the MimeMessage.
		/// </summary>
		public IList<MimeEntity> CalenderAttachments {
			get { return calenderAttachments; }
		}

		/// <summary>
		/// The HTML string that can be set on the BrowserControl.
		/// </summary>
		public string HtmlBody {
			get { return body ?? string.Empty; }
		}

		protected override void VisitMultipartAlternative (MultipartAlternative alternative)
		{
			// walk the multipart/alternative children backwards from greatest level of faithfulness to the least faithful
			for (int i = alternative.Count - 1; i >= 0 && body == null; i--)
				alternative[i].Accept (this);
		}

		protected override void VisitMultipartRelated (MultipartRelated related)
		{
			var root = related.Root;

			// push this multipart/related onto our stack
			stack.Add (related);

			// visit the root document
			root.Accept (this);

			// pop this multipart/related off our stack
			stack.RemoveAt (stack.Count - 1);
		}

		// look up the image based on the img src url within our multipart/related stack
		bool TryGetImage (string url, out MimePart image)
		{
			image = null;

			if (!Uri.TryCreate (url, UriKind.RelativeOrAbsolute, out var uri))
				return false;

			for (int i = stack.Count - 1; i >= 0; i--) {
				int index = stack[i].IndexOf (uri);

				if (index == -1)
					continue;

				image = stack[i][index] as MimePart;
				if (image == null || image.Content == null)
					return false;

				// Do not embed SVG, HTML, or arbitrary MIME types as data: URLs.
				return image.ContentType.IsMimeType ("image", "png") ||
					image.ContentType.IsMimeType ("image", "jpeg") ||
					image.ContentType.IsMimeType ("image", "gif") ||
					image.ContentType.IsMimeType ("image", "bmp");
			}

			return false;
		}

		/// <summary>
		/// Get a data: URI for the image attachment.
		/// </summary>
		/// <remarks>
		/// Encodes the image attachment into a string suitable for setting as a src= attribute value in
		/// an img tag.
		/// </remarks>
		/// <returns>The data: URI.</returns>
		/// <param name="image">The image attachment.</param>
		string GetDataUri (MimePart image)
		{
			using (var memory = new MemoryStream ()) {
				image.Content.DecodeTo (memory);
				var buffer = memory.GetBuffer ();
				var length = (int) memory.Length;
				var base64 = Convert.ToBase64String (buffer, 0, length);

				return string.Format ("data:{0};base64,{1}", image.ContentType.MimeType, base64);
			}
		}

		void FilterUrl (object sender, FilterUrlEventArgs e)
		{
			e.SanitizedUrl = null;

			if (e.Tag.LocalName == "img" && TryGetImage (e.OriginalUrl, out var image))
				e.SanitizedUrl = GetDataUri (image);
		}

		protected override void VisitTextPart (TextPart entity)
		{
			string html;

			// treat text/calendar parts as attachments rather than message bodies
			if (entity.ContentType.IsMimeType ("text", "calendar")) {
				calenderAttachments.Add (entity);
				return;
			}

			if (body != null) {
				// since we've already found the body, treat this as an attachment
				attachments.Add (entity);
				return;
			}

			if (entity.IsHtml) {
				// Sanitize text/html content while multipart/related resources are still in scope.
				html = sanitizer.SanitizeDocument (entity.Text);
			} else if (entity.IsFlowed) {
				var flowed = new FlowedToHtml ();

				if (entity.ContentType.Parameters.TryGetValue ("delsp", out string delsp))
					flowed.DeleteSpace = delsp.Equals ("yes", StringComparison.OrdinalIgnoreCase);

				html = flowed.Convert (entity.Text);
			} else {
				html = new TextToHtml ().Convert (entity.Text);
			}

			body = html;
		}

		protected override void VisitTnefPart (TnefPart entity)
		{
			// extract any attachments in the MS-TNEF part
			using (var tnef = entity.LoadTnefMessage ()) {
				// the converted message (and its attachments) outlives the TnefMessage
				var result = tnef.ConvertToMime ();

				attachments.AddRange (result.Message.Attachments);
			}
		}

		protected override void VisitMessagePart (MessagePart entity)
		{
			// treat message/rfc822 parts as attachments
			attachments.Add (entity);
		}

		protected override void VisitMimePart (MimePart entity)
		{
			// realistically, if we've gotten this far, then we can treat this as an attachment
			// even if the IsAttachment property is false.
			attachments.Add (entity);
		}
	}
}

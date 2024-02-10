//
// HtmlPreviewVisitor.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2014-2024 Jeffrey Stedfast
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
using System.Collections.Generic;

using Foundation;
using UIKit;

using MimeKit;
using MimeKit.Text;
using MimeKit.Tnef;

namespace MessageReader.iOS {
	/// <summary>
	/// Visits a MimeMessage and generates HTML suitable to be rendered by a browser control.
	/// </summary>
	class HtmlPreviewVisitor : MimeVisitor
	{
		readonly List<MultipartRelated> stack = new List<MultipartRelated> ();
		readonly List<MimeEntity> attachments = new List<MimeEntity> ();
		readonly UIWebView webView;
		bool renderedBody;

		/// <summary>
		/// Creates a new HtmlPreviewVisitor.
		/// </summary>
		public HtmlPreviewVisitor (UIWebView webView)
		{
			this.webView = webView;
		}

		/// <summary>
		/// The list of attachments that were in the MimeMessage.
		/// </summary>
		public IList<MimeEntity> Attachments {
			get { return attachments; }
		}

		protected override void VisitMultipartAlternative (MultipartAlternative alternative)
		{
			// walk the multipart/alternative children backwards from greatest level of faithfulness to the least faithful
			for (int i = alternative.Count - 1; i >= 0 && !renderedBody; i--)
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

		// Modifies various HTML attributes for better suitability for rendering.
		void HtmlTagCallback (HtmlTagContext ctx, HtmlWriter htmlWriter)
		{
			if (ctx.TagId == HtmlTagId.Meta && !ctx.IsEndTag) {
				bool isContentType = false;

				ctx.WriteTag (htmlWriter, false);

				// replace charsets with "utf-8" since our output will be in utf-8 (and not whatever the original charset was)
				foreach (var attribute in ctx.Attributes) {
					if (attribute.Id == HtmlAttributeId.Charset) {
						htmlWriter.WriteAttributeName (attribute.Name);
						htmlWriter.WriteAttributeValue ("utf-8");
					} else if (isContentType && attribute.Id == HtmlAttributeId.Content) {
						htmlWriter.WriteAttributeName (attribute.Name);
						htmlWriter.WriteAttributeValue ("text/html; charset=utf-8");
					} else {
						if (attribute.Id == HtmlAttributeId.HttpEquiv && attribute.Value != null
							&& attribute.Value.Equals ("Content-Type", StringComparison.OrdinalIgnoreCase))
							isContentType = true;

						htmlWriter.WriteAttribute (attribute);
					}
				}
			} else if (ctx.TagId == HtmlTagId.Body && !ctx.IsEndTag) {
				ctx.WriteTag (htmlWriter, false);

				// add and/or replace oncontextmenu="return false;"
				foreach (var attribute in ctx.Attributes) {
					if (attribute.Name.Equals ("oncontextmenu", StringComparison.OrdinalIgnoreCase))
						continue;

					htmlWriter.WriteAttribute (attribute);
				}

				htmlWriter.WriteAttribute ("oncontextmenu", "return false;");
			} else {
				// pass the tag through to the output
				ctx.WriteTag (htmlWriter, true);
			}
		}

		protected override void VisitTextPart (TextPart entity)
		{
			TextConverter converter;

			if (renderedBody) {
				// since we've already found the body, treat this as an attachment
				attachments.Add (entity);
				return;
			}

			if (entity.IsHtml) {
				converter = new HtmlToHtml {
					HtmlTagCallback = HtmlTagCallback
				};
			} else if (entity.IsFlowed) {
				var flowed = new FlowedToHtml ();

				if (entity.ContentType.Parameters.TryGetValue ("delsp", out string delsp))
					flowed.DeleteSpace = delsp.Equals ("yes", StringComparison.OrdinalIgnoreCase);

				converter = flowed;
			} else {
				converter = new TextToHtml ();
			}

			var html = converter.Convert (entity.Text);

			NSUrlCache.SharedCache = new MultipartRelatedUrlCache (stack);
			webView.LoadHtmlString (html, new NSUrl ("index.html"));
			renderedBody = true;
		}

		protected override void VisitTnefPart (TnefPart entity)
		{
			// extract any attachments in the MS-TNEF part
			attachments.AddRange (entity.ExtractAttachments ());
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

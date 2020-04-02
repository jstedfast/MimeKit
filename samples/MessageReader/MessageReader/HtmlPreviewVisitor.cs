//
// HtmlPreviewVisitor.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2014-2019 Jeffrey Stedfast
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
		string body;

		/// <summary>
		/// Creates a new HtmlPreviewVisitor.
		/// </summary>
		public HtmlPreviewVisitor ()
		{
		}

		/// <summary>
		/// The list of attachments that were in the MimeMessage.
		/// </summary>
		public IList<MimeEntity> Attachments {
			get { return attachments; }
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
			UriKind kind;
			int index;
			Uri uri;

			if (Uri.IsWellFormedUriString (url, UriKind.Absolute))
				kind = UriKind.Absolute;
			else if (Uri.IsWellFormedUriString (url, UriKind.Relative))
				kind = UriKind.Relative;
			else
				kind = UriKind.RelativeOrAbsolute;

			try {
				uri = new Uri (url, kind);
			} catch {
				image = null;
				return false;
			}

			for (int i = stack.Count - 1; i >= 0; i--) {
				if ((index = stack[i].IndexOf (uri)) == -1)
					continue;

				image = stack[i][index] as MimePart;
				return image != null;
			}

			image = null;

			return false;
		}

		/// <summary>
		/// Gets the attachent content as a data URI.
		/// </summary>
		/// <returns>The data URI.</returns>
		/// <param name="attachment">The attachment.</param>
		string GetDataUri (MimePart attachment)
		{
			using (var memory = new MemoryStream ()) {
				attachment.Content.DecodeTo (memory);
				var buffer = memory.GetBuffer ();
				var length = (int) memory.Length;
				var base64 = Convert.ToBase64String (buffer, 0, length);

				return string.Format ("data:{0};base64,{1}", attachment.ContentType.MimeType, base64);
			}
		}

		// Replaces <img src=...> urls that refer to images embedded within the message with
		// "data:" urls that the browser control will actually be able to load.
		void HtmlTagCallback (HtmlTagContext ctx, HtmlWriter htmlWriter)
		{
			if (ctx.TagId == HtmlTagId.Image && !ctx.IsEndTag && stack.Count > 0) {
				ctx.WriteTag (htmlWriter, false);

				// replace the src attribute with a "data:" URL
				foreach (var attribute in ctx.Attributes) {
					if (attribute.Id == HtmlAttributeId.Src) {
						MimePart image;
						string url;

						if (!TryGetImage (attribute.Value, out image)) {
							htmlWriter.WriteAttribute (attribute);
							continue;
						}

						url = GetDataUri (image);

						htmlWriter.WriteAttributeName (attribute.Name);
						htmlWriter.WriteAttributeValue (url);
					} else {
						htmlWriter.WriteAttribute (attribute);
					}
				}
			} else if (ctx.TagId == HtmlTagId.Body && !ctx.IsEndTag) {
				ctx.WriteTag (htmlWriter, false);

				// add and/or replace oncontextmenu="return false;"
				foreach (var attribute in ctx.Attributes) {
					if (attribute.Name.ToLowerInvariant () == "oncontextmenu")
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

			if (body != null) {
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
				string delsp;

				if (entity.ContentType.Parameters.TryGetValue ("delsp", out delsp))
					flowed.DeleteSpace = delsp.ToLowerInvariant () == "yes";

				converter = flowed;
			} else {
				converter = new TextToHtml ();
			}

			body = converter.Convert (entity.Text);
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

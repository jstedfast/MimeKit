//
// BodyBuilder.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc.
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

using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// A message body builder.
	/// </summary>
	/// <remarks>
	/// <see cref="BodyBuilder"/> is a helper class for building common MIME body structures.
	/// </remarks>
	public class BodyBuilder
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.BodyBuilder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="BodyBuilder"/>.
		/// </remarks>
		public BodyBuilder ()
		{
			LinkedResources = new AttachmentCollection (true);
			Attachments = new AttachmentCollection ();
		}

		/// <summary>
		/// Gets the attachments.
		/// </summary>
		/// <remarks>
		/// Represents a collection of file attachments that will be included in the message.
		/// </remarks>
		/// <value>The attachments.</value>
		public AttachmentCollection Attachments {
			get; private set;
		}

		/// <summary>
		/// Gets the linked resources.
		/// </summary>
		/// <remarks>
		/// Linked resources are a special type of attachment which are linked to from the <see cref="HtmlBody"/>.
		/// </remarks>
		/// <value>The linked resources.</value>
		public AttachmentCollection LinkedResources {
			get; private set;
		}

		/// <summary>
		/// Gets or sets the text body.
		/// </summary>
		/// <remarks>
		/// Represents the plain-text formatted version of the message body.
		/// </remarks>
		/// <value>The text body.</value>
		public string TextBody {
			get; set;
		}

		/// <summary>
		/// Gets or sets the html body.
		/// </summary>
		/// <remarks>
		/// Represents the html formatted version of the message body and may link to any of the <see cref="LinkedResources"/>.
		/// </remarks>
		/// <value>The html body.</value>
		public string HtmlBody {
			get; set;
		}

		/// <summary>
		/// Constructs the message body based on the text-based bodies, the linked resources, and the attachments.
		/// </summary>
		/// <remarks>
		/// Combines the <see cref="Attachments"/>, <see cref="LinkedResources"/>, <see cref="TextBody"/>,
		/// and <see cref="HtmlBody"/> into the proper MIME structure suitable for display in many common
		/// mail clients.
		/// </remarks>
		/// <returns>The message body.</returns>
		public MimeEntity ToMessageBody ()
		{
			Multipart alternative = null;
			MimeEntity body = null;

			if (!string.IsNullOrEmpty (TextBody)) {
				var text = new TextPart ("plain");
				text.Text = TextBody;

				if (!string.IsNullOrEmpty (HtmlBody)) {
					alternative = new Multipart ("alternative");
					alternative.Add (text);
					body = alternative;
				} else {
					body = text;
				}
			}

			if (!string.IsNullOrEmpty (HtmlBody)) {
				var text = new TextPart ("html");
				MimeEntity html;

				text.ContentId = MimeUtils.GenerateMessageId ();
				text.Text = HtmlBody;

				if (LinkedResources.Count > 0) {
					var related = new MultipartRelated {
						Root = text
					};

					foreach (var resource in LinkedResources)
						related.Add (resource);

					html = related;
				} else {
					html = text;
				}

				if (alternative != null)
					alternative.Add (html);
				else
					body = html;
			}

			if (Attachments.Count > 0) {
				var mixed = new Multipart ("mixed");

				if (body != null)
					mixed.Add (body);

				foreach (var attachment in Attachments)
					mixed.Add (attachment);

				body = mixed;
			}

			return body ?? new TextPart ("plain") { Text = string.Empty };
		}
	}
}

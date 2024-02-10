//
// BodyBuilder.cs
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

using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// A message body builder.
	/// </summary>
	/// <remarks>
	/// <see cref="BodyBuilder"/> is a helper class for building common MIME body structures.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\BodyBuilder.cs" region="Complex" />
	/// </example>
	public class BodyBuilder
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="BodyBuilder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="BodyBuilder"/>.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\BodyBuilder.cs" region="Complex" />
		/// </example>
		public BodyBuilder ()
		{
			LinkedResources = new AttachmentCollection (true);
			Attachments = new AttachmentCollection ();
		}

		/// <summary>
		/// Get the attachments.
		/// </summary>
		/// <remarks>
		/// Represents a collection of file attachments that will be included in the message.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\BodyBuilder.cs" region="Complex" />
		/// </example>
		/// <value>The attachments.</value>
		public AttachmentCollection Attachments {
			get; private set;
		}

		/// <summary>
		/// Get the linked resources.
		/// </summary>
		/// <remarks>
		/// Linked resources are a special type of attachment which are linked to from the <see cref="HtmlBody"/>.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\BodyBuilder.cs" region="Complex" />
		/// </example>
		/// <value>The linked resources.</value>
		public AttachmentCollection LinkedResources {
			get; private set;
		}

		/// <summary>
		/// Get or set the text body.
		/// </summary>
		/// <remarks>
		/// Represents the plain-text formatted version of the message body.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\BodyBuilder.cs" region="Complex" />
		/// </example>
		/// <value>The text body.</value>
		public string TextBody {
			get; set;
		}

		/// <summary>
		/// Get or set the html body.
		/// </summary>
		/// <remarks>
		/// Represents the html formatted version of the message body and may link to any of the <see cref="LinkedResources"/>.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\BodyBuilder.cs" region="Complex" />
		/// </example>
		/// <value>The html body.</value>
		public string HtmlBody {
			get; set;
		}

		/// <summary>
		/// Construct the message body based on the text-based bodies, the linked resources, and the attachments.
		/// </summary>
		/// <remarks>
		/// Combines the <see cref="Attachments"/>, <see cref="LinkedResources"/>, <see cref="TextBody"/>,
		/// and <see cref="HtmlBody"/> into the proper MIME structure suitable for display in many common
		/// mail clients.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\BodyBuilder.cs" region="Complex" />
		/// </example>
		/// <returns>The message body.</returns>
		public MimeEntity ToMessageBody ()
		{
			MultipartAlternative alternative = null;
			MimeEntity body = null;

			if (TextBody != null) {
				var text = new TextPart ("plain") {
					Text = TextBody
				};

				if (HtmlBody != null) {
					alternative = new MultipartAlternative {
						text
					};
					body = alternative;
				} else {
					body = text;
				}
			}

			if (HtmlBody != null) {
				var text = new TextPart ("html") {
					Text = HtmlBody
				};
				MimeEntity html;

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
				if (body is null && Attachments.Count == 1)
					return Attachments[0];

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

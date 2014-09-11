//
// MessageReaderViewWindow.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (www.xamarin.com)
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
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

using HtmlAgilityPack;
using MimeKit;

namespace MessageReader
{
	public partial class MessageViewWindow : Form
	{
		MimeMessage message;

		public MessageViewWindow ()
		{
			InitializeComponent ();
		}

		protected override void OnShown (EventArgs e)
		{
			base.OnShown (e);

			Message = MimeMessage.Load (GetType ().Assembly.GetManifestResourceStream ("MessageReader.xamarin3.msg"));
		}

		public MimeMessage Message {
			get { return message; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				message = value;

				Render (message.Body);
			}
		}

		void Render (MultipartRelated related)
		{
			var root = related.Root;
			var multipart = root as Multipart;
			var text = root as TextPart;

			if (multipart != null) {
				// Note: the root document can sometimes be a multipart/alternative.
				// A multipart/alternative is just a collection of alternate views.
				// The last part is the format that most closely matches what the
				// user saw in his or her email client's WYSIWYG editor.
				for (int i = multipart.Count; i > 0; i--) {
					var body = multipart[i - 1] as TextPart;

					if (body == null)
						continue;

					// our preferred mime-type is text/html
					if (body.ContentType.Matches ("text", "html")) {
						text = body;
						break;
					}

					if (text == null)
						text = body;
				}
			}

			if (text != null && text.ContentType.Matches ("text", "html")) {
				var doc = new HtmlAgilityPack.HtmlDocument ();
				var saved = new Dictionary<MimePart, string> ();
				TextPart html;

				doc.LoadHtml (text.Text);

				// find references to related MIME parts and replace them with links to links to the saved attachments
				foreach (var img in doc.DocumentNode.SelectNodes ("//img[@src]")) {
					var src = img.Attributes["src"];
					int index;
					Uri uri;

					if (src == null || src.Value == null)
						continue;

					// parse the <img src=...> attribute value into a Uri
					if (Uri.IsWellFormedUriString (src.Value, UriKind.Absolute))
						uri = new Uri (src.Value, UriKind.Absolute);
					else
						uri = new Uri (src.Value, UriKind.Relative);

					// locate the index of the attachment within the multipart/related (if it exists)
					if ((index = related.IndexOf (uri)) != -1) {
						var attachment = related[index] as MimePart;

						// make sure the referenced part is a MimePart (as opposed to another Multipart or MessagePart)
						if (attachment != null) {
							string fileName;

							// save the attachment (if we haven't already saved it)
							if (!saved.TryGetValue (attachment, out fileName)) {
								fileName = attachment.FileName;

								if (string.IsNullOrEmpty (fileName))
									fileName = Guid.NewGuid ().ToString ();

								using (var stream = File.Create (fileName))
									attachment.ContentObject.DecodeTo (stream);

								saved.Add (attachment, fileName);
							}

							// replace the <img src=...> value with the local file name
							src.Value = "file://" + Path.GetFullPath (fileName);
						}
					}
				}

				if (saved.Count > 0) {
					// we had to make some modifications to the original html part, so create a new
					// (temporary) text/html part to render
					html = new TextPart ("html");
					using (var writer = new StringWriter ()) {
						doc.Save (writer);

						html.Text = writer.GetStringBuilder ().ToString ();
					}
				} else {
					html = text;
				}

				Render (html);
			} else {
				Render (related.Root);
			}
		}

		void Render (TextPart text)
		{
			string html;

			if (!text.ContentType.Matches ("text", "html")) {
				var builder = new StringBuilder ("<html><body><p>");
				var plain = text.Text;

				for (int i = 0; i < plain.Length; i++) {
					switch (plain[i]) {
					case ' ': builder.Append ("&nbsp;"); break;
					case '"': builder.Append ("&quot;"); break;
					case '&': builder.Append ("&amp;"); break;
					case '<': builder.Append ("&lt;"); break;
					case '>': builder.Append ("&gt;"); break;
					case '\r': break;
					case '\n': builder.Append ("<p>"); break;
					case '\t':
						for (int j = 0; j < 8; j++)
							builder.Append ("&nbsp;");
						break;
					default:
						if (char.IsControl (plain[i]) || plain[i] > 127) {
							int unichar;

							if (i + 1 < plain.Length && char.IsSurrogatePair (plain[i], plain[i + 1]))
								unichar = char.ConvertToUtf32 (plain[i], plain[i + 1]);
							else
								unichar = plain[i];

							builder.AppendFormat ("&#{0};", unichar);
						} else {
							builder.Append (plain[i]);
						}
						break;
					}
				}

				builder.Append ("</body></html>");
				html = builder.ToString ();
			} else {
				html = text.Text;
			}

			webBrowser.DocumentText = html;
		}

		void Render (MimeEntity entity)
		{
			var related = entity as MultipartRelated;
			if (related != null) {
				if (related.Root != null)
					Render (related);
				return;
			}

			var multipart = entity as Multipart;
			var text = entity as TextPart;

			if (multipart != null) {
				if (multipart.ContentType.Matches ("multipart", "alternative")) {
					// A multipart/alternative is just a collection of alternate views.
					// The last part is the format that most closely matches what the
					// user saw in his or her email client's WYSIWYG editor.
					for (int i = multipart.Count; i > 0; i--) {
						related = multipart[i - 1] as MultipartRelated;

						if (related != null) {
							var root = related.Root;

							if (root != null && root.ContentType.Matches ("text", "html")) {
								Render (related);
								return;
							}

							continue;
						}

						text = multipart[i - 1] as TextPart;

						if (text != null) {
							Render (text);
							return;
						}
					}
				} else if (multipart.Count > 0) {
					// The main message body is usually the first part of a multipart/mixed.
					Render (multipart[0]);
				}
			} else if (text != null) {
				Render (text);
			}
		}
	}
}

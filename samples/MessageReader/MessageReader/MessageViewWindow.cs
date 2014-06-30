using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

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
			// FIXME: need to figure out how to intercept the webBrowser control's
			// requests for fetching images so that we can look up the URI in the
			// multipart/related - if the image is included in the multipart/related,
			// then we'll want to feed the webBrowser control the decoded
			// content of the MimePart instead of allowing the webBrowser to try
			// fetch the image from the internet.
			Render (related.Root);
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

//
// MessageReaderViewWindow.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2014-2026 Jeffrey Stedfast
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
using System.Windows.Forms;

using MimeKit;

namespace MessageReader
{
	public partial class MessageViewWindow : Form
	{
		MimeMessage message;
		bool loadingPreview;

		public MessageViewWindow ()
		{
			InitializeComponent ();

			webBrowser.AllowWebBrowserDrop = false;
			webBrowser.IsWebBrowserContextMenuEnabled = false;
			webBrowser.WebBrowserShortcutsEnabled = false;
			webBrowser.Navigating += (sender, e) => {
				e.Cancel = !loadingPreview || e.Url.AbsoluteUri != "about:blank";
				if (!e.Cancel)
					loadingPreview = false;
			};
			webBrowser.NewWindow += (sender, e) => e.Cancel = true;
		}

		protected override void OnShown (EventArgs e)
		{
			base.OnShown (e);

			Message = MimeMessage.Load (GetType ().Assembly.GetManifestResourceStream ("MessageReader.xamarin3.eml"));
		}

		public MimeMessage Message {
			get { return message; }
			set {
				if (value == null)
					throw new ArgumentNullException (nameof (value));

				message = value;

				// render the message body
				Render ();
			}
		}

		void Render ()
		{
			var visitor = new HtmlPreviewVisitor ();
			message.Accept (visitor);

			// DocumentText navigates to about:blank before loading the new document.
			loadingPreview = true;
			try {
				webBrowser.DocumentText = visitor.HtmlBody;
			} catch {
				loadingPreview = false;
				throw;
			}
		}
	}
}

//
// MainActivity.cs
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
using System.Text;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Android.OS;

using MimeKit;

namespace MessageReader.Android {
	[Activity (Label = "MessageReader.Android", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		MimeMessage message;
		WebView webView;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our webView from the layout resource
			webView = FindViewById<WebView> (Resource.Id.webView);

			// Load the message
			Message = MimeMessage.Load (GetType ().Assembly.GetManifestResourceStream ("xamarin3.msg"));
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
			var client = new MultipartRelatedWebViewClient (related);

			webView.SetWebViewClient (client);

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

			webView.LoadData (html, "text/html", "utf-8");
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

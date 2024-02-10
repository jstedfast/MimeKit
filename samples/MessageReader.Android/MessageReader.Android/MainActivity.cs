//
// MainActivity.cs
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

using Android.App;
using Android.Webkit;
using Android.OS;

using MimeKit;
using MimeKit.Text;

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
			webView.SetInitialScale (100);

			// Load the message
			Message = MimeMessage.Load (GetType ().Assembly.GetManifestResourceStream ("xamarin3.msg"));
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
			var visitor = new HtmlPreviewVisitor (webView);

			message.Accept (visitor);
		}
	}
}

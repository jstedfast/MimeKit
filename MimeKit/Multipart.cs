//
// Multipart.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2012 Jeffrey Stedfast
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

namespace MimeKit {
	public class Multipart : MimeEntity
	{
		static readonly StringComparer icase = StringComparer.InvariantCultureIgnoreCase;

		string boundary;

		public Multipart (string subtype) : base ("multipart", subtype)
		{
			// FIXME: generate a random boundary
		}

		public string Boundary {
			get { return boundary; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (boundary == value)
					return;

				ContentType.Parameters["boundary"] = value;
			}
		}

		protected override void OnContentTypeChanged (object sender, EventArgs e)
		{
			boundary = ContentType.Parameters["boundary"];

			base.OnContentTypeChanged (sender, e);
		}

		public override void CopyTo (Stream stream)
		{
			throw new NotImplementedException ();
		}
	}
}

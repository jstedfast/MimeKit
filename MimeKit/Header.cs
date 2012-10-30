//
// Header.cs
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

namespace MimeKit {
	public class Header
	{
		string text;

		internal Header (string field, string value, long offset)
		{
			if (field == null)
				throw new ArgumentNullException ("field");

			if (value == null)
				throw new ArgumentNullException ("value");

			Offset = offset;
			Field = field;
			text = value;
		}

		public Header (string field, string value) : this (field, value, -1)
		{
		}

		public string Field {
			get; private set;
		}

		public string Value {
			get { return text; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (text == value)
					return;

				text = value;
				Offset = -1;

				if (Changed != null)
					Changed (this, EventArgs.Empty);
			}
		}

		public long Offset {
			get; private set;
		}

		public event EventHandler Changed;
	}
}

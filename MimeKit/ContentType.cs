//
// ContentType.cs
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
	public sealed class ContentType
	{
		string type, subtype;

		public ContentType (string type, string subtype)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			if (type == string.Empty)
				throw new ArgumentException ("The type is not allowed to be empty.");

			if (subtype == null)
				throw new ArgumentNullException ("subtype");

			if (subtype == string.Empty)
				throw new ArgumentException ("The subtype is not allowed to be empty.");

			Parameters = new ParameterList ();
			Parameters.Changed += OnParametersChanged;
			this.subtype = subtype;
			this.type = type;
		}

		public string Type {
			get { return type; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (value == string.Empty)
					throw new ArgumentException ("The type is not allowed to be empty.");

				if (type == value)
					return;

				type = value;

				OnChanged ();
			}
		}

		public string Subtype {
			get { return subtype; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (value == string.Empty)
					throw new ArgumentException ("The subtype is not allowed to be empty.");

				if (subtype == value)
					return;

				subtype = value;

				OnChanged ();
			}
		}

		public ParameterList Parameters {
			get; private set;
		}

		public event EventHandler Changed;

		void OnParametersChanged (object sender, EventArgs e)
		{
			OnChanged ();
		}

		void OnChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}
	}
}

//
// HtmlAttributeReader.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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

namespace MimeKit.Text {
	struct HtmlAttributeReader
	{
		readonly string rawValue;
		int index;

		public HtmlAttributeReader (string value)
		{
			index = value.IndexOf (' ');
			rawValue = value;
			Value = null;
			Name = null;
		}

		public bool HasValue {
			get { return Value != null; }
		}

		public string Name {
			get; private set;
		}

		public string Value {
			get; private set;
		}

		public bool ReadNext ()
		{
			if (index < 0)
				return false;

			while (char.IsWhiteSpace (rawValue[index]))
				index++;

			if (rawValue[index] == '/' || rawValue[index] == '>')
				return false;

			int startIndex = index;

			if (HtmlUtils.IsValidStartCharacter (rawValue[index]))
				index++;
			
			while (HtmlUtils.IsValidNameCharacter (rawValue[index]))
				index++;

			Name = rawValue.Substring (startIndex, index - startIndex);

			while (char.IsWhiteSpace (rawValue[index]))
				index++;

			if (rawValue[index] == '=') {
				index++;

				if (rawValue[index] == '\'' || rawValue[index] == '"') {
					char quote = rawValue[index++];

					startIndex = index;

					while (rawValue[index] != quote)
						index++;

					Value = rawValue.Substring (startIndex, index - startIndex);
					Value = HtmlUtils.HtmlDecode (Value, 0, Value.Length);

					index++;
				} else {
					startIndex = index;

					while (!char.IsWhiteSpace (rawValue[index]) && rawValue[index] != '>')
						index++;

					Value = rawValue.Substring (startIndex, index - startIndex);
					Value = HtmlUtils.HtmlDecode (Value, 0, Value.Length);
				}
			} else {
				Value = null;
			}

			return true;
		}
	}
}

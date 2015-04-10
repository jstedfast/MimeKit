//
// HtmlUtils.cs
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

namespace MimeKit.Text {
	static class HtmlUtils
	{
		static bool IsValidStartCharacter (char c)
		{
			return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_';
		}

		static bool IsValidNameCharacter (char c)
		{
			return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-' || c == '_';
		}

		public static bool IsValidAttributeName (string name)
		{
			if (string.IsNullOrEmpty (name))
				return false;

			if (!IsValidStartCharacter (name[0]))
				return false;

			for (int i = 0; i < name.Length; i++) {
				if (!IsValidNameCharacter (name[i]))
					return false;
			}

			return true;
		}

		public static bool IsValidTagName (string name)
		{
			if (string.IsNullOrEmpty (name))
				return false;

			if (!IsValidStartCharacter (name[0]))
				return false;

			for (int i = 0; i < name.Length; i++) {
				if (!IsValidNameCharacter (name[i]))
					return false;
			}

			return true;
		}
	}
}

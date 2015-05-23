//
// HtmlToken.cs
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

namespace MimeKit.Text {
	abstract class HtmlToken
	{
		public readonly HtmlTokenKind Kind;

		protected HtmlToken (HtmlTokenKind kind)
		{
			Kind = kind;
		}
	}

	sealed class HtmlTokenComment : HtmlToken
	{
		public readonly string Comment;

		public HtmlTokenComment (HtmlTokenKind kind, string comment) : base (kind)
		{
			Comment = comment;
		}
	}

	sealed class HtmlTokenDocType : HtmlToken
	{
		public readonly string DocType;

		public HtmlTokenDocType (HtmlTokenKind kind, string doctype) : base (kind)
		{
			DocType = doctype;
		}
	}

	sealed class HtmlTokenTag : HtmlToken
	{
		public HtmlTokenTag (HtmlTokenKind kind, string tag, string value) : base (kind)
		{
			AttributeReader = new HtmlAttributeReader (value);
			TagId = tag.ToHtmlTagId ();
			TagName = tag;
		}

		public HtmlAttributeReader AttributeReader {
			get; private set;
		}

		public HtmlTagId TagId {
			get; private set;
		}

		public string TagName {
			get; private set;
		}
	}

	sealed class HtmlTokenText : HtmlToken
	{
		readonly HtmlReader reader;

		public HtmlTokenText (HtmlTokenKind kind, HtmlReader htmlReader) : base (kind)
		{
			reader = htmlReader;
		}

		static void ValidateArguments (char[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (offset < 0 || offset > buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");

			if (count < 0 || offset + count > buffer.Length)
				throw new ArgumentOutOfRangeException ("count");
		}

		public int Read (char[] buffer, int offset, int count)
		{
			ValidateArguments (buffer, offset, count);

			return reader.ReadText (buffer, offset, count);
		}
	}
}

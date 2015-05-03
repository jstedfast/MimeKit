//
// HtmlReader.cs
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
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace MimeKit.Text {
	enum HtmlReaderState {
		Initial,
		Reading,
		Error,
		EndOfFile,
		Closed
	}

	class HtmlReader
	{
		static readonly HashSet<string> AutoCloseTags;
		const int ReadAheadSize = 128;
		const int BlockSize = 4096;
		const int PadSize = 1;

		enum HtmlParserState {
			Initial,
			Error,
			EndOfFile,
			Closed,

			ElementContent,
			TagName,
			BetweenAttributes,
			AttributeName,
			AttributeEquals,
			AttributeValue,
		}

		// I/O buffering
		readonly char[] input = new char[ReadAheadSize + BlockSize + PadSize];
		const int inputStart = ReadAheadSize;
		int inputIndex = ReadAheadSize;
		int inputEnd = ReadAheadSize;

		// attribute buffer
		char[] attributeBuffer = new char[512];
		char attributeValueQuote;
		long attributeOffset;
		int attributeIndex;

		readonly IList<HtmlAttribute> attributes = new List<HtmlAttribute> ();
		readonly IList<string> openTags = new List<string> ();

		HtmlParserState state;

		readonly TextReader reader;

		static HtmlReader ()
		{
			// Note: These are tags that auto-close when an identical tag is encountered and/or when a parent node is closed.
			AutoCloseTags = new HashSet<string> (StringComparer.OrdinalIgnoreCase);
			AutoCloseTags.Add ("li");
			AutoCloseTags.Add ("p");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Text.HtmlReader"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="HtmlReader"/>.
		/// </remarks>
		/// <param name="input">The input.</param>
		public HtmlReader (TextReader input)
		{
			reader = input;
		}

		/// <summary>
		/// Get the current state of the reader.
		/// </summary>
		/// <remarks>
		/// Gets the current state of the reader.
		/// </remarks>
		/// <value>The state of the reader.</value>
		public HtmlReaderState ReaderState {
			get {
				switch (state) {
				case HtmlParserState.Initial:
					return HtmlReaderState.Initial;
				case HtmlParserState.Error:
					return HtmlReaderState.Error;
				case HtmlParserState.EndOfFile:
					return HtmlReaderState.EndOfFile;
				case HtmlParserState.Closed:
					return HtmlReaderState.Closed;
				default:
					return HtmlReaderState.Reading;
				}
			}
		}

		/// <summary>
		/// Advance the reader to the next HTML element.
		/// </summary>
		/// <remarks>
		/// Advances the reader to the next HTML element.
		/// </remarks>
		/// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
		public bool Read ()
		{
			switch (state) {
			case HtmlParserState.EndOfFile:
			case HtmlParserState.Closed:
			case HtmlParserState.Error:
				return false;
			}

			return true;
		}


	}
}

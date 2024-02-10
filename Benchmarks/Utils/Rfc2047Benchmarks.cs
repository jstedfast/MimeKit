//
// Rfc2047Benchmarks.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2024 .NET Foundation and Contributors
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

using System.Text;

using BenchmarkDotNet.Attributes;

using MimeKit;
using MimeKit.Utils;

namespace Benchmarks.Utils {
	public class Rfc2047Benchmarks
	{
		static readonly byte[] ILoveMimeSoMuch = Encoding.ASCII.GetBytes ("I'm so happy! =?utf-8?b?8J+YgA==?= I love MIME so much =?utf-8?b?4p2k77iP4oCN8J+UpSE=?= Isn't it great?");

		[Benchmark]
		public void Rfc2047_DecodeText ()
		{
			Rfc2047.DecodeText (ParserOptions.Default, ILoveMimeSoMuch, 0, ILoveMimeSoMuch.Length);
		}

		[Benchmark]
		public void Rfc2047_DecodePhrase ()
		{
			Rfc2047.DecodePhrase (ParserOptions.Default, ILoveMimeSoMuch, 0, ILoveMimeSoMuch.Length);
		}
	}
}

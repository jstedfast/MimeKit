//
// TnefReader.cs
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
using System.IO;

namespace MimeKit.Tnef {
	public class TnefReader : IDisposable
	{
		readonly Stream stream;
		bool disposed;

		public short AttachmentKey {
			get; private set;
		}

		public int AttributeRawValueLength {
			get; private set;
		}

		public int AttributeRawValueStreamOffset {
			get; private set;
		}

		public TnefAttributeTag AttributeTag {
			get; private set;
		}

		public int MessageCodepage {
			get; private set;
		}

		public TnefPropertyReader TnefPropertyReader {
			get; private set;
		}

		public int StreamOffset {
			get; private set;
		}

		public int TnefVersion {
			get; private set;
		}

		public TnefReader (Stream inputStream)
		{
			stream = inputStream;
		}

		~TnefReader ()
		{
			Dispose (false);
		}

		public bool ReadNextAttribute ()
		{
			throw new NotImplementedException ();
		}

		public int ReadAttributeRawValue (byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException ();
		}



		public void Close ()
		{
			Dispose ();
		}

		#region IDisposable implementation

		protected virtual void Dispose (bool disposing)
		{
			stream.Dispose ();
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
			disposed = true;
		}

		#endregion
	}
}

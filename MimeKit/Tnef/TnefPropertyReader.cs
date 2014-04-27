//
// TnefPropertyReader.cs
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

using MimeKit.IO;

namespace MimeKit.Tnef {
	public struct TnefPropertyReader
	{
		readonly TnefReader reader;

		public bool IsComputedProperty {
			get { throw new NotImplementedException (); }
		}

		public bool IsEmbeddedMessage {
			get { throw new NotImplementedException (); }
		}

		public bool IsLargeValue {
			get { throw new NotImplementedException (); }
		}

		public bool IsMultiValuedProperty {
			get { throw new NotImplementedException (); }
		}

		public bool IsNamedProperty {
			get { throw new NotImplementedException (); }
		}

		public bool IsObjectProperty {
			get { throw new NotImplementedException (); }
		}

		public Guid ObjectIid {
			get { throw new NotImplementedException (); }
		}

		public int PropertyCount {
			get { throw new NotImplementedException (); }
		}

		public TnefNameId PropertyNameId {
			get { throw new NotImplementedException (); }
		}

		public TnefPropertyTag PropertyTag {
			get { throw new NotImplementedException (); }
		}

		public int RawValueLength {
			get { throw new NotImplementedException (); }
		}

		public int RawValueStreamOffset {
			get { throw new NotImplementedException (); }
		}

		public int RowCount {
			get { throw new NotImplementedException (); }
		}

		public int ValueCount {
			get { throw new NotImplementedException (); }
		}

		public Type ValueType {
			get { throw new NotImplementedException (); }
		}

		internal TnefPropertyReader (TnefReader tnef)
		{
			reader = tnef;
		}

		public TnefReader GetEmbeddedMessageReader ()
		{
			return new TnefReader (GetRawValueReadStream (), reader.MessageCodepage, reader.ComplianceMode);
		}

		public Stream GetRawValueReadStream ()
		{
			long start = reader.AttributeRawValueStreamOffset;
			long end = start + reader.AttributeRawValueLength;

			return new BoundStream (reader.InputStream, start, end, true);
		}

		public bool ReadNextProperty ()
		{
			throw new NotImplementedException ();
		}

		public bool ReadNextRow ()
		{
			throw new NotImplementedException ();
		}

		public bool ReadNextValue ()
		{
			throw new NotImplementedException ();
		}

		public int ReadRawValue (byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException ();
		}

		public int ReadTextValue (char[] buffer, int offset, int count)
		{
			throw new NotImplementedException ();
		}

		public object ReadValue ()
		{
			throw new NotImplementedException ();
		}

		public bool ReadValueAsBoolean ()
		{
			throw new NotImplementedException ();
		}

		public byte[] ReadValueAsBytes ()
		{
			throw new NotImplementedException ();
		}

		public DateTime ReadValueAsDateTime ()
		{
			throw new NotImplementedException ();
		}

		public double ReadValueAsDouble ()
		{
			throw new NotImplementedException ();
		}

		public float ReadValueAsFloat ()
		{
			throw new NotImplementedException ();
		}

		public Guid ReadValueAsGuid ()
		{
			throw new NotImplementedException ();
		}

		public short ReadValueAsInt16 ()
		{
			throw new NotImplementedException ();
		}

		public int ReadValueAsInt32 ()
		{
			throw new NotImplementedException ();
		}

		public long ReadValueAsInt64 ()
		{
			throw new NotImplementedException ();
		}

		public string ReadValueAsString ()
		{
			throw new NotImplementedException ();
		}

		public override int GetHashCode ()
		{
			return reader.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			if (!(obj is TnefPropertyReader))
				return false;

			var prop = (TnefPropertyReader) obj;

			return prop.reader == reader;
		}
	}
}

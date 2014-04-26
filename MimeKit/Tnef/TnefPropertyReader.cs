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

namespace MimeKit.Tnef {
	public struct TnefPropertyReader
	{
		public bool IsComputedProperty {
			get; private set;
		}

		public bool IsEmbeddedMessage {
			get; private set;
		}

		public bool IsLargeValue {
			get; private set;
		}

		public bool IsMultiValuedProperty {
			get; private set;
		}

		public bool IsNamedProperty {
			get; private set;
		}

		public bool IsObjectProperty {
			get; private set;
		}

		public Guid ObjectIid {
			get; private set;
		}

		public int PropertyCount {
			get; private set;
		}

		public TnefNameId PropertyNameId {
			get; private set;
		}

		public TnefPropertyTag PropertyTag {
			get; private set;
		}

		public int RawValueLength {
			get; private set;
		}

		public int RawValueStreamOffset {
			get; private set;
		}

		public int RowCount {
			get; private set;
		}

		public int ValueCount {
			get; private set;
		}

		public Type ValueType {
			get; private set;
		}

		public TnefReader GetEmbeddedMessageReader ()
		{
			throw new NotImplementedException ();
		}

		public Stream GetRawValueReadStream ()
		{
			throw new NotImplementedException ();
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
	}
}

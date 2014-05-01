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

#if PORTABLE
using Encoding = Portable.Text.Encoding;
#else
using Encoding = System.Text.Encoding;
#endif

using MimeKit.IO;

namespace MimeKit.Tnef {
	class TnefPropertyReader
	{
		static readonly Encoding DefaultEncoding = Encoding.GetEncoding (1252);
		TnefPropertyTag propertyTag;
		readonly TnefReader reader;
		TnefNameId propertyName;
		int rawValueOffset;
		int rawValueLength;
		int propertyIndex;
		int propertyCount;
		int valueIndex;
		int valueCount;
		int rowIndex;
		int rowCount;

		public bool IsComputedProperty {
			get { throw new NotImplementedException (); }
		}

		public bool IsEmbeddedMessage {
			get { return propertyTag.Id == TnefPropertyId.AttachData && propertyTag.ValueTnefType == TnefPropertyType.Object; }
		}

		public bool IsLargeValue {
			get { throw new NotImplementedException (); }
		}

		public bool IsMultiValuedProperty {
			get { return propertyTag.IsMultiValued; }
		}

		public bool IsNamedProperty {
			get { return propertyTag.IsNamed; }
		}

		public bool IsObjectProperty {
			get { return propertyTag.ValueTnefType == TnefPropertyType.Object; }
		}

		public Guid ObjectIid {
			get { throw new NotImplementedException (); }
		}

		public int PropertyCount {
			get { return propertyCount; }
		}

		public TnefNameId PropertyNameId {
			get { return propertyName; }
		}

		public TnefPropertyTag PropertyTag {
			get { return propertyTag; }
		}

		public int RawValueLength {
			get { return rawValueLength; }
		}

		public int RawValueStreamOffset {
			get { return rawValueOffset; }
		}

		public int RowCount {
			get { return rowCount; }
		}

		public int ValueCount {
			get { return valueCount; }
		}

		public Type ValueType {
			get {
				if (propertyCount > 0)
					return GetPropertyValueType ();

				return GetAttributeValueType ();
			}
		}

		internal TnefPropertyReader (TnefReader tnef)
		{
			propertyTag = TnefPropertyTag.Null;
			propertyName = new TnefNameId ();
			rawValueOffset = 0;
			rawValueLength = 0;
			propertyIndex = 0;
			propertyCount = 0;
			valueIndex = 0;
			valueCount = 0;
			rowIndex = 0;
			rowCount = 0;

			reader = tnef;
		}

		public TnefReader GetEmbeddedMessageReader ()
		{
			if (!IsEmbeddedMessage)
				throw new InvalidOperationException ();

			return new TnefReader (GetRawValueReadStream (), reader.MessageCodepage, reader.ComplianceMode);
		}

		public Stream GetRawValueReadStream ()
		{
			if (valueIndex >= valueCount)
				throw new InvalidOperationException ();

			int start = RawValueStreamOffset;
			int length = RawValueLength;

			if (propertyCount > 0) {
				switch (propertyTag.ValueTnefType) {
				case TnefPropertyType.Unicode:
				case TnefPropertyType.String8:
				case TnefPropertyType.Binary:
					start += 4;
					break;
				}
			}

			valueIndex++;

			return new BoundStream (reader.InputStream, start, start + length, true);
		}

		void CheckAvailable (long bytes)
		{
			long start = reader.AttributeRawValueStreamOffset;
			long end = start + reader.AttributeRawValueLength;

			if (reader.StreamOffset + bytes > end) {
				reader.ComplianceStatus |= TnefComplianceStatus.InvalidAttributeLength;
				if (reader.ComplianceMode == TnefComplianceMode.Strict)
					throw new TnefException ("Invalid attribute length.");

				//throw new IOException ();
			}
		}

		byte ReadByte ()
		{
			CheckAvailable (1);

			return reader.ReadByte ();
		}

		byte[] ReadBytes (int count)
		{
			CheckAvailable (count);

			var bytes = new byte[count];
			int offset = 0;
			int nread;

			while (offset < count && (nread = reader.ReadAttributeRawValue (bytes, offset, count - offset)) > 0)
				offset += nread;

			return bytes;
		}

		short ReadInt16 ()
		{
			CheckAvailable (2);

			return reader.ReadInt16 ();
		}

		int ReadInt32 ()
		{
			CheckAvailable (4);

			return reader.ReadInt32 ();
		}

		int PeekInt32 ()
		{
			CheckAvailable (4);

			return reader.PeekInt32 ();
		}

		long ReadInt64 ()
		{
			CheckAvailable (8);

			return reader.ReadInt64 ();
		}

		float ReadSingle ()
		{
			CheckAvailable (4);

			return reader.ReadSingle ();
		}

		double ReadDouble ()
		{
			CheckAvailable (8);

			return reader.ReadDouble ();
		}

		DateTime ReadDateTime ()
		{
			var date = new DateTime (1601, 1, 1);
			long fileTime = ReadInt64 ();

			date = date.AddMilliseconds (fileTime /= 10000);

			return date;
		}

		static int GetPaddedLength (int length)
		{
			return (length + 3) & ~3;
		}

		byte[] ReadByteArray ()
		{
			int length = ReadInt32 ();
			var bytes = ReadBytes (length);

			if ((length % 4) != 0) {
				// remaining bytes are padding
				int padding = 4 - (length % 4);

				reader.Seek (reader.StreamOffset + padding);
			}

			return bytes;
		}

		string ReadUnicodeString ()
		{
			var bytes = ReadByteArray ();
			int length = bytes.Length;

			// force length to a multiple of 2 bytes
			length &= ~1;

			while (length > 1 && bytes[length - 1] == 0 && bytes[length - 2] == 0)
				length -= 2;

			if (length < 2)
				return string.Empty;

			return Encoding.Unicode.GetString (bytes, 0, length);
		}

		string DecodeAnsiString (byte[] bytes)
		{
			int codepage = reader.MessageCodepage;
			int length = bytes.Length;

			while (length > 0 && bytes[length - 1] == 0)
				length--;

			if (length == 0)
				return string.Empty;

			if (codepage != 0 && codepage != 1252) {
				try {
					return Encoding.GetEncoding (codepage).GetString (bytes, 0, length);
				} catch {
					return DefaultEncoding.GetString (bytes, 0, length);
				}
			}

			return DefaultEncoding.GetString (bytes, 0, length);
		}

		string ReadString ()
		{
			var bytes = ReadByteArray ();

			return DecodeAnsiString (bytes);
		}

		byte[] ReadAttrBytes ()
		{
			return ReadBytes (RawValueLength);
		}

		string ReadAttrString ()
		{
			var bytes = ReadBytes (RawValueLength);

			// attribute strings are null-terminated
			return DecodeAnsiString (bytes);
		}

		DateTime ReadAttrDateTime ()
		{
			int year = ReadInt16 ();
			int month = ReadInt16 ();
			int day = ReadInt16 ();
			int hour = ReadInt16 ();
			int minute = ReadInt16 ();
			int second = ReadInt16 ();
			#pragma warning disable 219
			int dow = ReadInt16 ();
			#pragma warning restore 219

			var value = new DateTime (year, month, day, hour, minute, second);

			return value;
		}

		void LoadPropertyName ()
		{
			var guid = new Guid (ReadBytes (16));
			var kind = (TnefNameIdKind) ReadInt32 ();

			if (kind == TnefNameIdKind.Name) {
				var name = ReadUnicodeString ();

				propertyName = new TnefNameId (guid, name);
			} else if (kind == TnefNameIdKind.Id) {
				int id = ReadInt32 ();

				propertyName = new TnefNameId (guid, id);
			} else {
				reader.ComplianceStatus |= TnefComplianceStatus.InvalidAttributeValue;
				if (reader.ComplianceMode == TnefComplianceMode.Strict)
					throw new TnefException ("Invalid TnefNameIdKind.");

				propertyName = new TnefNameId (guid, 0);
			}
		}

		public bool ReadNextProperty ()
		{
			if (propertyIndex >= propertyCount)
				return false;

			while (ReadNextValue ()) {
				// skip over the value...
			}

			var type = (TnefPropertyType) ReadInt16 ();
			var id = (TnefPropertyId) ReadInt16 ();

			propertyTag = new TnefPropertyTag (id, type);

			if (propertyTag.IsNamed)
				LoadPropertyName ();

			LoadValueCount ();

			rawValueLength = GetPropertyValueLength ();
			rawValueOffset = reader.StreamOffset;

			propertyIndex++;

			return true;
		}

		public bool ReadNextRow ()
		{
			if (rowIndex >= rowCount)
				return false;

			while (ReadNextProperty ()) {
				// skip over the property...
			}

			LoadPropertyCount ();
			rowIndex++;

			return true;
		}

		public bool ReadNextValue ()
		{
			if (valueIndex >= valueCount || propertyCount == 0)
				return false;

			int offset = RawValueStreamOffset + RawValueLength;

			if (reader.StreamOffset < offset && !reader.Seek (offset))
				return false;

			rawValueLength = GetPropertyValueLength ();
			rawValueOffset = reader.StreamOffset;

			valueIndex++;

			return true;
		}

		public int ReadRawValue (byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");

			if (count < 0 || count > (buffer.Length - offset))
				throw new ArgumentOutOfRangeException ("count");

			throw new NotImplementedException ();
		}

		public int ReadTextValue (char[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");

			if (count < 0 || count > (buffer.Length - offset))
				throw new ArgumentOutOfRangeException ("count");

			throw new NotImplementedException ();
		}

		int GetPropertyValueLength ()
		{
			switch (propertyTag.ValueTnefType) {
			case TnefPropertyType.Unspecified:
			case TnefPropertyType.Null:
				return 0;
			case TnefPropertyType.Boolean:
			case TnefPropertyType.Error:
			case TnefPropertyType.Long:
			case TnefPropertyType.R4:
			case TnefPropertyType.I2:
				return 4;
			case TnefPropertyType.Currency:
			case TnefPropertyType.Double:
			case TnefPropertyType.I8:
				return 8;
			case TnefPropertyType.ClassId:
			case TnefPropertyType.Object:
				return 16;
			case TnefPropertyType.Unicode:
			case TnefPropertyType.String8:
			case TnefPropertyType.Binary:
				return 4 + GetPaddedLength (PeekInt32 ());
			case TnefPropertyType.AppTime:
			case TnefPropertyType.SysTime:
				return 8;
			default:
				reader.ComplianceStatus |= TnefComplianceStatus.UnsupportedPropertyType;
				if (reader.ComplianceMode == TnefComplianceMode.Strict)
					throw new TnefException ("Unsupported value type.");

				throw new TnefException (string.Format ("Unsupported value type: {0}", propertyTag.ValueTnefType));
			}
		}

		Type GetPropertyValueType ()
		{
			switch (propertyTag.ValueTnefType) {
			case TnefPropertyType.I2:       return typeof (short);
			case TnefPropertyType.Boolean:  return typeof (bool);
			case TnefPropertyType.Currency: return typeof (long);
			case TnefPropertyType.I8:       return typeof (long);
			case TnefPropertyType.Error:    return typeof (int);
			case TnefPropertyType.Long:     return typeof (int);
			case TnefPropertyType.Double:   return typeof (double);
			case TnefPropertyType.R4:       return typeof (float);
			case TnefPropertyType.AppTime:  return typeof (DateTime);
			case TnefPropertyType.SysTime:  return typeof (DateTime);
			case TnefPropertyType.Unicode:  return typeof (string);
			case TnefPropertyType.String8:  return typeof (string);
			case TnefPropertyType.Binary:   return typeof (byte[]);
			case TnefPropertyType.ClassId:  return typeof (byte[]);
			case TnefPropertyType.Object:   return typeof (Guid);
			default:                        return typeof (object);
			}
		}

		Type GetAttributeValueType ()
		{
			switch (reader.AttributeType) {
			case TnefAttributeType.Triples: return typeof (byte[]);
			case TnefAttributeType.String:  return typeof (string);
			case TnefAttributeType.Text:    return typeof (string);
			case TnefAttributeType.Date:    return typeof (DateTime);
			case TnefAttributeType.Short:   return typeof (short);
			case TnefAttributeType.Long:    return typeof (int);
			case TnefAttributeType.Byte:    return typeof (byte[]);
			case TnefAttributeType.Word:    return typeof (short);
			case TnefAttributeType.DWord:   return typeof (int);
			default:                        return typeof (object);
			}
		}

		object ReadPropertyValue ()
		{
			object value;

			switch (propertyTag.ValueTnefType) {
			case TnefPropertyType.Null:
				value = null;
				break;
			case TnefPropertyType.I2:
				// 2 bytes for the short followed by 2 bytes of padding
				value = (short) (ReadInt32 () & 0xFFFF);
				break;
			case TnefPropertyType.Boolean:
				value = (ReadInt32 () & 0xFF) != 0;
				break;
			case TnefPropertyType.Currency:
			case TnefPropertyType.I8:
				value = ReadInt64 ();
				break;
			case TnefPropertyType.Error:
			case TnefPropertyType.Long:
				value = ReadInt32 ();
				break;
			case TnefPropertyType.Double:
				value = ReadDouble ();
				break;
			case TnefPropertyType.R4:
				value = ReadSingle ();
				break;
			case TnefPropertyType.AppTime:
			case TnefPropertyType.SysTime:
				value = ReadDateTime ();
				break;
			case TnefPropertyType.Unicode:
				value = ReadUnicodeString ();
				break;
			case TnefPropertyType.String8:
				value = ReadString ();
				break;
			case TnefPropertyType.Binary:
				value = ReadByteArray ();
				break;
			case TnefPropertyType.ClassId:
				value = ReadBytes (16);
				break;
			case TnefPropertyType.Object:
				value = new Guid (ReadBytes (16));
				break;
			default:
				reader.ComplianceStatus |= TnefComplianceStatus.UnsupportedPropertyType;
				if (reader.ComplianceMode == TnefComplianceMode.Strict)
					throw new TnefException ("Unsupported property type.");

				value = null;
				break;
			}

			valueIndex++;

			return value;
		}

		public object ReadValue ()
		{
			if (valueIndex >= valueCount)
				throw new InvalidOperationException ();

			if (propertyCount > 0)
				return ReadPropertyValue ();

			object value;

			switch (reader.AttributeType) {
			case TnefAttributeType.Triples: value = ReadAttrBytes (); break;
			case TnefAttributeType.String: value = ReadAttrString (); break;
			case TnefAttributeType.Text:   value = ReadAttrString (); break;
			case TnefAttributeType.Date:   value = ReadAttrDateTime (); break;
			case TnefAttributeType.Short:  value = ReadInt16 (); break;
			case TnefAttributeType.Long:   value = ReadInt32 (); break;
			case TnefAttributeType.Byte:   value = ReadAttrBytes (); break;
			case TnefAttributeType.Word:   value = ReadInt16 (); break;
			case TnefAttributeType.DWord:  value = ReadInt32 (); break;
			default: throw new TnefException ("Unknown attribute type.");
			}

			valueIndex++;

			return value;
		}

		public bool ReadValueAsBoolean ()
		{
			if (valueIndex >= valueCount)
				throw new InvalidOperationException ();

			bool value;

			if (propertyCount > 0) {
				switch (propertyTag.ValueTnefType) {
				case TnefPropertyType.Boolean:
					value = (ReadInt32 () & 0xFF) != 0;
					break;
				case TnefPropertyType.I2:
					value = (ReadInt32 () & 0xFFFF) != 0;
					break;
				case TnefPropertyType.Error:
				case TnefPropertyType.Long:
					value = ReadInt32 () != 0;
					break;
				case TnefPropertyType.Currency:
				case TnefPropertyType.I8:
					value = ReadInt64 () != 0;
					break;
				default:
					throw new InvalidOperationException ();
				}
			} else {
				switch (reader.AttributeType) {
				case TnefAttributeType.Short:  value = ReadInt16 () != 0; break;
				case TnefAttributeType.Long:   value = ReadInt32 () != 0; break;
				case TnefAttributeType.Word:   value = ReadInt16 () != 0; break;
				case TnefAttributeType.DWord:  value = ReadInt32 () != 0; break;
				case TnefAttributeType.Byte:   value = ReadByte () != 0; break;
				default: throw new InvalidOperationException ();
				}
			}

			valueIndex++;

			return value;
		}

		public byte[] ReadValueAsBytes ()
		{
			if (valueIndex >= valueCount)
				throw new InvalidOperationException ();

			byte[] bytes;

			if (propertyCount > 0) {
				switch (propertyTag.ValueTnefType) {
				case TnefPropertyType.Unicode:
				case TnefPropertyType.String8:
				case TnefPropertyType.Binary:
					bytes = ReadByteArray ();
					break;
				case TnefPropertyType.ClassId:
				case TnefPropertyType.Object:
					bytes = ReadBytes (16);
					break;
				default:
					throw new InvalidOperationException ();
				}
			} else {
				switch (reader.AttributeType) {
				case TnefAttributeType.Triples:
				case TnefAttributeType.String:
				case TnefAttributeType.Text:
				case TnefAttributeType.Byte:
					bytes = ReadAttrBytes ();
					break;
				default:
					throw new ArgumentOutOfRangeException ();
				}
			}

			valueIndex++;

			return bytes;
		}

		public DateTime ReadValueAsDateTime ()
		{
			if (valueIndex >= valueCount)
				throw new InvalidOperationException ();

			DateTime value;

			if (propertyCount > 0) {
				if (propertyTag.ValueTnefType != TnefPropertyType.AppTime &&
					propertyTag.ValueTnefType != TnefPropertyType.SysTime)
					throw new InvalidOperationException ();

				value = ReadDateTime ();
			} else if (reader.AttributeType == TnefAttributeType.Date) {
				value = ReadAttrDateTime ();
			} else {
				throw new InvalidOperationException ();
			}

			valueIndex++;

			return value;
		}

		public double ReadValueAsDouble ()
		{
			if (valueIndex >= valueCount)
				throw new InvalidOperationException ();

			double value;

			if (propertyCount > 0) {
				switch (propertyTag.ValueTnefType) {
				case TnefPropertyType.Boolean:
					value = (ReadInt32 () & 0xFF);
					break;
				case TnefPropertyType.I2:
					value = (ReadInt32 () & 0xFFFF);
					break;
				case TnefPropertyType.Error:
				case TnefPropertyType.Long:
					value = ReadInt32 ();
					break;
				case TnefPropertyType.Currency:
				case TnefPropertyType.I8:
					value = ReadInt64 ();
					break;
				case TnefPropertyType.Double:
					value = ReadDouble ();
					break;
				case TnefPropertyType.R4:
					value = ReadSingle ();
					break;
				default:
					throw new InvalidOperationException ();
				}
			} else {
				switch (reader.AttributeType) {
				case TnefAttributeType.Short:  value = ReadInt16 (); break;
				case TnefAttributeType.Long:   value = ReadInt32 (); break;
				case TnefAttributeType.Word:   value = ReadInt16 (); break;
				case TnefAttributeType.DWord:  value = ReadInt32 (); break;
				case TnefAttributeType.Byte:   value = ReadDouble (); break;
				default: throw new InvalidOperationException ();
				}
			}

			valueIndex++;

			return value;
		}

		public float ReadValueAsFloat ()
		{
			if (valueIndex >= valueCount)
				throw new InvalidOperationException ();

			float value;

			if (propertyCount > 0) {
				switch (propertyTag.ValueTnefType) {
				case TnefPropertyType.Boolean:
					value = (ReadInt32 () & 0xFF);
					break;
				case TnefPropertyType.I2:
					value = (ReadInt32 () & 0xFFFF);
					break;
				case TnefPropertyType.Error:
				case TnefPropertyType.Long:
					value = ReadInt32 ();
					break;
				case TnefPropertyType.Currency:
				case TnefPropertyType.I8:
					value = ReadInt64 ();
					break;
				case TnefPropertyType.Double:
					value = (float) ReadDouble ();
					break;
				case TnefPropertyType.R4:
					value = ReadSingle ();
					break;
				default:
					throw new InvalidOperationException ();
				}
			} else {
				switch (reader.AttributeType) {
				case TnefAttributeType.Short:  value = ReadInt16 (); break;
				case TnefAttributeType.Long:   value = ReadInt32 (); break;
				case TnefAttributeType.Word:   value = ReadInt16 (); break;
				case TnefAttributeType.DWord:  value = ReadInt32 (); break;
				case TnefAttributeType.Byte:   value = ReadSingle (); break;
				default: throw new InvalidOperationException ();
				}
			}

			valueIndex++;

			return value;
		}

		public Guid ReadValueAsGuid ()
		{
			if (valueIndex >= valueCount)
				throw new InvalidOperationException ();

			Guid guid;

			if (propertyCount > 0) {
				switch (propertyTag.ValueTnefType) {
				case TnefPropertyType.ClassId:
				case TnefPropertyType.Object:
					guid = new Guid (ReadBytes (16));
					break;
				default:
					throw new InvalidOperationException ();
				}
			} else {
				throw new InvalidOperationException ();
			}

			valueIndex++;

			return guid;
		}

		public short ReadValueAsInt16 ()
		{
			if (valueIndex >= valueCount)
				throw new InvalidOperationException ();

			short value;

			if (propertyCount > 0) {
				switch (propertyTag.ValueTnefType) {
				case TnefPropertyType.Boolean:
					value = (short) (ReadInt32 () & 0xFF);
					break;
				case TnefPropertyType.I2:
					value = (short) (ReadInt32 () & 0xFFFF);
					break;
				case TnefPropertyType.Error:
				case TnefPropertyType.Long:
					value = (short) ReadInt32 ();
					break;
				case TnefPropertyType.Currency:
				case TnefPropertyType.I8:
					value = (short) ReadInt64 ();
					break;
				case TnefPropertyType.Double:
					value = (short) ReadDouble ();
					break;
				case TnefPropertyType.R4:
					value = (short) ReadSingle ();
					break;
				default:
					throw new InvalidOperationException ();
				}
			} else {
				switch (reader.AttributeType) {
				case TnefAttributeType.Short:  value = ReadInt16 (); break;
				case TnefAttributeType.Long:   value = (short) ReadInt32 (); break;
				case TnefAttributeType.Word:   value = ReadInt16 (); break;
				case TnefAttributeType.DWord:  value = (short) ReadInt32 (); break;
				case TnefAttributeType.Byte:   value = ReadInt16 (); break;
				default: throw new InvalidOperationException ();
				}
			}

			valueIndex++;

			return value;
		}

		public int ReadValueAsInt32 ()
		{
			if (valueIndex >= valueCount)
				throw new InvalidOperationException ();

			int value;

			if (propertyCount > 0) {
				switch (propertyTag.ValueTnefType) {
				case TnefPropertyType.Boolean:
					value = ReadInt32 () & 0xFF;
					break;
				case TnefPropertyType.I2:
					value = ReadInt32 () & 0xFFFF;
					break;
				case TnefPropertyType.Error:
				case TnefPropertyType.Long:
					value = ReadInt32 ();
					break;
				case TnefPropertyType.Currency:
				case TnefPropertyType.I8:
					value = (int) ReadInt64 ();
					break;
				case TnefPropertyType.Double:
					value = (int) ReadDouble ();
					break;
				case TnefPropertyType.R4:
					value = (int) ReadSingle ();
					break;
				default:
					throw new InvalidOperationException ();
				}
			} else {
				switch (reader.AttributeType) {
				case TnefAttributeType.Short:  value = ReadInt16 (); break;
				case TnefAttributeType.Long:   value = ReadInt32 (); break;
				case TnefAttributeType.Word:   value = ReadInt16 (); break;
				case TnefAttributeType.DWord:  value = ReadInt32 (); break;
				case TnefAttributeType.Byte:   value = ReadInt32 (); break;
				default: throw new InvalidOperationException ();
				}
			}

			valueIndex++;

			return value;
		}

		public long ReadValueAsInt64 ()
		{
			if (valueIndex >= valueCount)
				throw new InvalidOperationException ();

			long value;

			if (propertyCount > 0) {
				switch (propertyTag.ValueTnefType) {
				case TnefPropertyType.Boolean:
					value = ReadInt32 () & 0xFF;
					break;
				case TnefPropertyType.I2:
					value = ReadInt32 () & 0xFFFF;
					break;
				case TnefPropertyType.Error:
				case TnefPropertyType.Long:
					value = ReadInt32 ();
					break;
				case TnefPropertyType.Currency:
				case TnefPropertyType.I8:
					value = ReadInt64 ();
					break;
				case TnefPropertyType.Double:
					value = (long) ReadDouble ();
					break;
				case TnefPropertyType.R4:
					value = (long) ReadSingle ();
					break;
				default:
					throw new InvalidOperationException ();
				}
			} else {
				switch (reader.AttributeType) {
				case TnefAttributeType.Short:  value = ReadInt16 (); break;
				case TnefAttributeType.Long:   value = ReadInt32 (); break;
				case TnefAttributeType.Word:   value = ReadInt16 (); break;
				case TnefAttributeType.DWord:  value = ReadInt32 (); break;
				case TnefAttributeType.Byte:   value = ReadInt64 (); break;
				default: throw new InvalidOperationException ();
				}
			}

			valueIndex++;

			return value;
		}

		public string ReadValueAsString ()
		{
			if (valueIndex >= valueCount)
				throw new InvalidOperationException ();

			string value;

			if (propertyCount > 0) {
				switch (propertyTag.ValueTnefType) {
				case TnefPropertyType.Unicode: value = ReadUnicodeString (); break;
				case TnefPropertyType.String8: value = ReadString (); break;
				case TnefPropertyType.Binary:  value = ReadString (); break;
				default: throw new InvalidOperationException ();
				}
			} else {
				switch (reader.AttributeType) {
				case TnefAttributeType.String: value = ReadAttrString (); break;
				case TnefAttributeType.Text:   value = ReadAttrString (); break;
				case TnefAttributeType.Byte:   value = ReadAttrString (); break;
				default: throw new InvalidOperationException ();
				}
			}

			valueIndex++;

			return value;
		}

		public override int GetHashCode ()
		{
			return reader.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			var prop = obj as TnefPropertyReader;

			return prop != null && prop.reader == reader;
		}

		void LoadPropertyCount ()
		{
			if ((propertyCount = ReadInt32 ()) < 0) {
				reader.ComplianceStatus |= TnefComplianceStatus.InvalidAttributeValue;
				if (reader.ComplianceMode == TnefComplianceMode.Strict)
					throw new TnefException ("Invalid attribute value.");

				propertyCount = 0;
			}

			propertyIndex = 0;
			valueCount = 0;
			valueIndex = 0;
		}

		int ReadValueCount ()
		{
			int count;

			if ((count = ReadInt32 ()) < 0) {
				reader.ComplianceStatus |= TnefComplianceStatus.InvalidAttributeValue;
				if (reader.ComplianceMode == TnefComplianceMode.Strict)
					throw new TnefException ("Invalid attribute value.");

				return 0;
			}

			return count;
		}

		void LoadValueCount ()
		{
			if (propertyTag.IsMultiValued) {
				valueCount = ReadValueCount ();
			} else {
				switch (propertyTag.ValueTnefType) {
				case TnefPropertyType.Unicode:
				case TnefPropertyType.String8:
				case TnefPropertyType.Binary:
				case TnefPropertyType.Object:
					valueCount = ReadValueCount ();
					break;
				default:
					valueCount = 1;
					break;
				}
			}

			valueIndex = 0;
		}

		void LoadRowCount ()
		{
			if ((rowCount = ReadInt32 ()) < 0) {
				reader.ComplianceStatus |= TnefComplianceStatus.InvalidAttributeValue;
				if (reader.ComplianceMode == TnefComplianceMode.Strict)
					throw new TnefException ("Invalid attribute value.");

				rowCount = 0;
			}

			propertyCount = 0;
			propertyIndex = 0;
			valueCount = 0;
			valueIndex = 0;
			rowIndex = 0;
		}

		internal void Load ()
		{
			propertyTag = TnefPropertyTag.Null;
			rawValueOffset = 0;
			rawValueLength = 0;
			propertyCount = 0;
			propertyIndex = 0;
			valueCount = 0;
			valueIndex = 0;
			rowCount = 0;
			rowIndex = 0;

			switch (reader.AttributeTag) {
			case TnefAttributeTag.MapiProperties:
			case TnefAttributeTag.Attachment:
				LoadPropertyCount ();
				break;
			case TnefAttributeTag.RecipientTable:
				LoadRowCount ();
				break;
			default:
				rawValueLength = reader.AttributeRawValueLength;
				rawValueOffset = reader.StreamOffset;
				valueCount = 1;
				break;
			}
		}
	}
}

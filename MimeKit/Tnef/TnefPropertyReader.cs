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

		/// <summary>
		/// Gets a value indicating whether or not the current property has multiple values.
		/// </summary>
		/// <value><c>true</c> if the current property has multiple values; otherwise, <c>false</c>.</value>
		public bool IsMultiValuedProperty {
			get { return propertyTag.IsMultiValued; }
		}

		/// <summary>
		/// Gets a value indicating whether or not the current property is a named property.
		/// </summary>
		/// <value><c>true</c> if the current property is a named property; otherwise, <c>false</c>.</value>
		public bool IsNamedProperty {
			get { return propertyTag.IsNamed; }
		}

		/// <summary>
		/// Gets a value indicating whether the current property contains object values.
		/// </summary>
		/// <value><c>true</c> if the current property contains object values; otherwise, <c>false</c>.</value>
		public bool IsObjectProperty {
			get { return propertyTag.ValueTnefType == TnefPropertyType.Object; }
		}

		public Guid ObjectIid {
			get { throw new NotImplementedException (); }
		}

		/// <summary>
		/// Gets the number of properties available.
		/// </summary>
		/// <value>The property count.</value>
		public int PropertyCount {
			get { return propertyCount; }
		}

		/// <summary>
		/// Gets the property name identifier.
		/// </summary>
		/// <value>The property name identifier.</value>
		public TnefNameId PropertyNameId {
			get { return propertyName; }
		}

		/// <summary>
		/// Gets the property tag.
		/// </summary>
		/// <value>The property tag.</value>
		public TnefPropertyTag PropertyTag {
			get { return propertyTag; }
		}

		/// <summary>
		/// Gets the length of the raw value.
		/// </summary>
		/// <value>The length of the raw value.</value>
		public int RawValueLength {
			get { return rawValueLength; }
		}

		/// <summary>
		/// Gets the raw value stream offset.
		/// </summary>
		/// <value>The raw value stream offset.</value>
		public int RawValueStreamOffset {
			get { return rawValueOffset; }
		}

		/// <summary>
		/// Gets the number of table rows available.
		/// </summary>
		/// <value>The row count.</value>
		public int RowCount {
			get { return rowCount; }
		}

		/// <summary>
		/// Gets the number of values available.
		/// </summary>
		/// <value>The value count.</value>
		public int ValueCount {
			get { return valueCount; }
		}

		/// <summary>
		/// Gets the type of the value.
		/// </summary>
		/// <value>The type of the value.</value>
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

		/// <summary>
		/// Gets the embedded TNEF message reader.
		/// </summary>
		/// <returns>The embedded TNEF message reader.</returns>
		public TnefReader GetEmbeddedMessageReader ()
		{
			if (!IsEmbeddedMessage)
				throw new InvalidOperationException ();

			return new TnefReader (GetRawValueReadStream (), reader.MessageCodepage, reader.ComplianceMode);
		}

		/// <summary>
		/// Gets the raw value of the attribute or property as a stream.
		/// </summary>
		/// <returns>The raw value stream.</returns>
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

		bool CheckAvailable (long bytes)
		{
			long attrEndOffset = reader.AttributeRawValueStreamOffset + reader.AttributeRawValueLength;

			if (reader.StreamOffset + bytes > attrEndOffset) {
				reader.ComplianceStatus |= TnefComplianceStatus.InvalidAttributeLength;
				if (reader.ComplianceMode == TnefComplianceMode.Strict)
					throw new TnefException ("Invalid attribute length.");

				return false;
			}

			return true;
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

		/// <summary>
		/// Advances to the next MAPI property.
		/// </summary>
		/// <returns><c>true</c> if there is another property available to be read; otherwise <c>false</c>.</returns>
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
			propertyIndex++;

			if (!TryGetPropertyValueLength (out rawValueLength))
				return false;

			rawValueOffset = reader.StreamOffset;

			return true;
		}

		/// <summary>
		/// Advances to the next table row of properties.
		/// </summary>
		/// <returns><c>true</c> if there is another row available to be read; otherwise <c>false</c>.</returns>
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

		/// <summary>
		/// Advances to the next value in the TNEF stream.
		/// </summary>
		/// <returns><c>true</c> if there is another value available to be read; otherwise <c>false</c>.</returns>
		public bool ReadNextValue ()
		{
			if (valueIndex >= valueCount || propertyCount == 0)
				return false;

			int offset = RawValueStreamOffset + RawValueLength;

			if (reader.StreamOffset < offset && !reader.Seek (offset))
				return false;

			valueIndex++;

			if (!TryGetPropertyValueLength (out rawValueLength))
				return false;

			rawValueOffset = reader.StreamOffset;

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

		bool TryGetPropertyValueLength (out int length)
		{
			switch (propertyTag.ValueTnefType) {
			case TnefPropertyType.Unspecified:
			case TnefPropertyType.Null:
				length = 0;
				break;
			case TnefPropertyType.Boolean:
			case TnefPropertyType.Error:
			case TnefPropertyType.Long:
			case TnefPropertyType.R4:
			case TnefPropertyType.I2:
				length = 4;
				break;
			case TnefPropertyType.Currency:
			case TnefPropertyType.Double:
			case TnefPropertyType.I8:
				length = 8;
				break;
			case TnefPropertyType.ClassId:
			case TnefPropertyType.Object:
				length = 16;
				break;
			case TnefPropertyType.Unicode:
			case TnefPropertyType.String8:
			case TnefPropertyType.Binary:
				if (!CheckAvailable (4)) {
					length = 0;
					return false;
				}

				length = 4 + GetPaddedLength (PeekInt32 ());
				break;
			case TnefPropertyType.AppTime:
			case TnefPropertyType.SysTime:
				length = 8;
				break;
			default:
				reader.ComplianceStatus |= TnefComplianceStatus.UnsupportedPropertyType;
				if (reader.ComplianceMode == TnefComplianceMode.Strict)
					throw new TnefException ("Unsupported value type.");

				length = 0;

				return false;
			}

			return true;
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

		/// <summary>
		/// Reads the value.
		/// </summary>
		/// <returns>The value.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// There are no more values to read or the value could not be read.
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">
		/// The TNEF stream is truncated and the value could not be read.
		/// </exception>
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

		/// <summary>
		/// Reads the value as a boolean.
		/// </summary>
		/// <returns>The value as a boolean.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// There are no more values to read or the value could not be read as a boolean.
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">
		/// The TNEF stream is truncated and the value could not be read.
		/// </exception>
		public bool ReadValueAsBoolean ()
		{
			if (valueIndex >= valueCount)
				throw new InvalidOperationException ();

			if (!CheckAvailable (RawValueLength))
				return false;

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

		/// <summary>
		/// Reads the value as a byte array.
		/// </summary>
		/// <returns>The value as a byte array.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// There are no more values to read or the value could not be read as a byte array.
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">
		/// The TNEF stream is truncated and the value could not be read.
		/// </exception>
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

		/// <summary>
		/// Reads the value as a date and time.
		/// </summary>
		/// <returns>The value as a date and time.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// There are no more values to read or the value could not be read as a date and time.
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">
		/// The TNEF stream is truncated and the value could not be read.
		/// </exception>
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

		/// <summary>
		/// Reads the value as a double.
		/// </summary>
		/// <returns>The value as a double.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// There are no more values to read or the value could not be read as a double.
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">
		/// The TNEF stream is truncated and the value could not be read.
		/// </exception>
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

		/// <summary>
		/// Reads the value as a float.
		/// </summary>
		/// <returns>The value as a float.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// There are no more values to read or the value could not be read as a float.
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">
		/// The TNEF stream is truncated and the value could not be read.
		/// </exception>
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

		/// <summary>
		/// Reads the value as a GUID.
		/// </summary>
		/// <returns>The value as a GUID.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// There are no more values to read or the value could not be read as a GUID.
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">
		/// The TNEF stream is truncated and the value could not be read.
		/// </exception>
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

		/// <summary>
		/// Reads the value as a 16-bit integer.
		/// </summary>
		/// <returns>The value as a 16-bit integer.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// There are no more values to read or the value could not be read as a 16-bit integer.
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">
		/// The TNEF stream is truncated and the value could not be read.
		/// </exception>
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

		/// <summary>
		/// Reads the value as a 32-bit integer.
		/// </summary>
		/// <returns>The value as a 32-bit integer.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// There are no more values to read or the value could not be read as a 32-bit integer.
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">
		/// The TNEF stream is truncated and the value could not be read.
		/// </exception>
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

		/// <summary>
		/// Reads the value as a 64-bit integer.
		/// </summary>
		/// <returns>The value as a 64-bit integer.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// There are no more values to read or the value could not be read as a 64-bit integer.
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">
		/// The TNEF stream is truncated and the value could not be read.
		/// </exception>
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

		/// <summary>
		/// Reads the value as a string.
		/// </summary>
		/// <returns>The value as a string.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// There are no more values to read or the value could not be read as a string.
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">
		/// The TNEF stream is truncated and the value could not be read.
		/// </exception>
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

		/// <summary>
		/// Serves as a hash function for a <see cref="MimeKit.Tnef.TnefPropertyReader"/> object.
		/// </summary>
		/// <returns>A hash code for this instance that is suitable for use in hashing algorithms
		/// and data structures such as a hash table.</returns>
		public override int GetHashCode ()
		{
			return reader.GetHashCode ();
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="MimeKit.Tnef.TnefPropertyReader"/>.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="MimeKit.Tnef.TnefPropertyReader"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="MimeKit.Tnef.TnefPropertyReader"/>; otherwise, <c>false</c>.</returns>
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

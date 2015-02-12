//
// TnefPropertyReader.cs
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
using System.Text;

#if PORTABLE
using EncoderReplacementFallback = Portable.Text.EncoderReplacementFallback;
using DecoderReplacementFallback = Portable.Text.DecoderReplacementFallback;
using EncoderExceptionFallback = Portable.Text.EncoderExceptionFallback;
using DecoderExceptionFallback = Portable.Text.DecoderExceptionFallback;
using EncoderFallbackException = Portable.Text.EncoderFallbackException;
using DecoderFallbackException = Portable.Text.DecoderFallbackException;
using DecoderFallbackBuffer = Portable.Text.DecoderFallbackBuffer;
using DecoderFallback = Portable.Text.DecoderFallback;
using Encoding = Portable.Text.Encoding;
using Encoder = Portable.Text.Encoder;
using Decoder = Portable.Text.Decoder;
#endif

namespace MimeKit.Tnef {
	/// <summary>
	/// A TNEF property reader.
	/// </summary>
	/// <remarks>
	/// A TNEF property reader.
	/// </remarks>
	public class TnefPropertyReader
	{
		static readonly Encoding DefaultEncoding = Encoding.GetEncoding (1252);
		TnefPropertyTag propertyTag;
		readonly TnefReader reader;
		TnefNameId propertyName;
		int rawValueOffset;
		int rawValueLength;
		int propertyIndex;
		int propertyCount;
		Decoder decoder;
		int valueIndex;
		int valueCount;
		int rowIndex;
		int rowCount;

		internal TnefAttachMethod AttachMethod {
			get; set;
		}

		/// <summary>
		/// Gets a value indicating whether the current property is a computed property.
		/// </summary>
		/// <remarks>
		/// Gets a value indicating whether the current property is a computed property.
		/// </remarks>
		/// <value><c>true</c> if the current property is a computed property; otherwise, <c>false</c>.</value>
		public bool IsComputedProperty {
			get { throw new NotImplementedException (); }
		}

		/// <summary>
		/// Gets a value indicating whether the current property is an embedded TNEF message.
		/// </summary>
		/// <remarks>
		/// Gets a value indicating whether the current property is an embedded TNEF message.
		/// </remarks>
		/// <value><c>true</c> if the current property is an embedded TNEF message; otherwise, <c>false</c>.</value>
		public bool IsEmbeddedMessage {
			get { return propertyTag.Id == TnefPropertyId.AttachData && AttachMethod == TnefAttachMethod.EmbeddedMessage; }
		}

		/// <summary>
		/// Gets a value indicating whether the current property has a large value.
		/// </summary>
		/// <remarks>
		/// Gets a value indicating whether the current property has a large value.
		/// </remarks>
		/// <value><c>true</c> if the current property has a large value; otherwise, <c>false</c>.</value>
		public bool IsLargeValue {
			get { throw new NotImplementedException (); }
		}

		/// <summary>
		/// Gets a value indicating whether or not the current property has multiple values.
		/// </summary>
		/// <remarks>
		/// Gets a value indicating whether or not the current property has multiple values.
		/// </remarks>
		/// <value><c>true</c> if the current property has multiple values; otherwise, <c>false</c>.</value>
		public bool IsMultiValuedProperty {
			get { return propertyTag.IsMultiValued; }
		}

		/// <summary>
		/// Gets a value indicating whether or not the current property is a named property.
		/// </summary>
		/// <remarks>
		/// Gets a value indicating whether or not the current property is a named property.
		/// </remarks>
		/// <value><c>true</c> if the current property is a named property; otherwise, <c>false</c>.</value>
		public bool IsNamedProperty {
			get { return propertyTag.IsNamed; }
		}

		/// <summary>
		/// Gets a value indicating whether the current property contains object values.
		/// </summary>
		/// <remarks>
		/// Gets a value indicating whether the current property contains object values.
		/// </remarks>
		/// <value><c>true</c> if the current property contains object values; otherwise, <c>false</c>.</value>
		public bool IsObjectProperty {
			get { return propertyTag.ValueTnefType == TnefPropertyType.Object; }
		}

		/// <summary>
		/// Gets the object iid.
		/// </summary>
		/// <remarks>
		/// Gets the object iid.
		/// </remarks>
		/// <value>The object iid.</value>
		public Guid ObjectIid {
			get { throw new NotImplementedException (); }
		}

		/// <summary>
		/// Gets the number of properties available.
		/// </summary>
		/// <remarks>
		/// Gets the number of properties available.
		/// </remarks>
		/// <value>The property count.</value>
		public int PropertyCount {
			get { return propertyCount; }
		}

		/// <summary>
		/// Gets the property name identifier.
		/// </summary>
		/// <remarks>
		/// Gets the property name identifier.
		/// </remarks>
		/// <value>The property name identifier.</value>
		public TnefNameId PropertyNameId {
			get { return propertyName; }
		}

		/// <summary>
		/// Gets the property tag.
		/// </summary>
		/// <remarks>
		/// Gets the property tag.
		/// </remarks>
		/// <value>The property tag.</value>
		public TnefPropertyTag PropertyTag {
			get { return propertyTag; }
		}

		/// <summary>
		/// Gets the length of the raw value.
		/// </summary>
		/// <remarks>
		/// Gets the length of the raw value.
		/// </remarks>
		/// <value>The length of the raw value.</value>
		public int RawValueLength {
			get { return rawValueLength; }
		}

		/// <summary>
		/// Gets the raw value stream offset.
		/// </summary>
		/// <remarks>
		/// Gets the raw value stream offset.
		/// </remarks>
		/// <value>The raw value stream offset.</value>
		public int RawValueStreamOffset {
			get { return rawValueOffset; }
		}

		/// <summary>
		/// Gets the number of table rows available.
		/// </summary>
		/// <remarks>
		/// Gets the number of table rows available.
		/// </remarks>
		/// <value>The row count.</value>
		public int RowCount {
			get { return rowCount; }
		}

		/// <summary>
		/// Gets the number of values available.
		/// </summary>
		/// <remarks>
		/// Gets the number of values available.
		/// </remarks>
		/// <value>The value count.</value>
		public int ValueCount {
			get { return valueCount; }
		}

		/// <summary>
		/// Gets the type of the value.
		/// </summary>
		/// <remarks>
		/// Gets the type of the value.
		/// </remarks>
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
		/// <remarks>
		/// Gets the embedded TNEF message reader.
		/// </remarks>
		/// <returns>The embedded TNEF message reader.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The property does not contain any more values.</para>
		/// <para>-or-</para>
		/// <para>The property value is not an embedded message.</para>
		/// </exception>
		public TnefReader GetEmbeddedMessageReader ()
		{
			if (!IsEmbeddedMessage)
				throw new InvalidOperationException ();

			var stream = GetRawValueReadStream ();
			var guid = new byte[16];

			stream.Read (guid, 0, 16);

			return new TnefReader (stream, reader.MessageCodepage, reader.ComplianceMode);
		}

		/// <summary>
		/// Gets the raw value of the attribute or property as a stream.
		/// </summary>
		/// <remarks>
		/// Gets the raw value of the attribute or property as a stream.
		/// </remarks>
		/// <returns>The raw value stream.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The property does not contain any more values.
		/// </exception>
		public Stream GetRawValueReadStream ()
		{
			if (valueIndex >= valueCount)
				throw new InvalidOperationException ();

			int end = RawValueStreamOffset + RawValueLength;

			if (propertyCount > 0 && reader.StreamOffset == RawValueStreamOffset) {
				switch (propertyTag.ValueTnefType) {
				case TnefPropertyType.Unicode:
				case TnefPropertyType.String8:
				case TnefPropertyType.Binary:
				case TnefPropertyType.Object:
					ReadInt32 ();
					break;
				}
			}

			valueIndex++;

			return new TnefReaderStream (reader, end);
		}

		bool CheckRawValueLength ()
		{
			// Check that the property value does not go beyond the end of the end of the attribute
			int attrEndOffset = reader.AttributeRawValueStreamOffset + reader.AttributeRawValueLength;
			int valueEndOffset = RawValueStreamOffset + RawValueLength;

			if (valueEndOffset > attrEndOffset) {
				reader.SetComplianceError (TnefComplianceStatus.InvalidAttributeValue);
				return false;
			}

			return true;
		}

		byte ReadByte ()
		{
			return reader.ReadByte ();
		}

		byte[] ReadBytes (int count)
		{
			var bytes = new byte[count];
			int offset = 0;
			int nread;

			while (offset < count && (nread = reader.ReadAttributeRawValue (bytes, offset, count - offset)) > 0)
				offset += nread;

			return bytes;
		}

		short ReadInt16 ()
		{
			return reader.ReadInt16 ();
		}

		int ReadInt32 ()
		{
			return reader.ReadInt32 ();
		}

		int PeekInt32 ()
		{
			return reader.PeekInt32 ();
		}

		long ReadInt64 ()
		{
			return reader.ReadInt64 ();
		}

		float ReadSingle ()
		{
			return reader.ReadSingle ();
		}

		double ReadDouble ()
		{
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

		Encoding GetMessageEncoding ()
		{
			int codepage = reader.MessageCodepage;

			if (codepage != 0 && codepage != 1252) {
				try {
					return Encoding.GetEncoding (codepage, new EncoderExceptionFallback (), new DecoderExceptionFallback ());
				} catch {
					return DefaultEncoding;
				}
			}

			return DefaultEncoding;
		}

		string DecodeAnsiString (byte[] bytes)
		{
			int length = bytes.Length;

			while (length > 0 && bytes[length - 1] == 0)
				length--;

			if (length == 0)
				return string.Empty;

			try {
				return GetMessageEncoding ().GetString (bytes, 0, length);
			} catch {
				return DefaultEncoding.GetString (bytes, 0, length);
			}
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

			try {
				return new DateTime (year, month, day, hour, minute, second);
			} catch (ArgumentOutOfRangeException ex) {
				reader.SetComplianceError (TnefComplianceStatus.InvalidDate, ex);
				return default (DateTime);
			}
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
				reader.SetComplianceError (TnefComplianceStatus.InvalidAttributeValue);
				propertyName = new TnefNameId (guid, 0);
			}
		}

		/// <summary>
		/// Advances to the next MAPI property.
		/// </summary>
		/// <remarks>
		/// Advances to the next MAPI property.
		/// </remarks>
		/// <returns><c>true</c> if there is another property available to be read; otherwise <c>false</c>.</returns>
		/// <exception cref="TnefException">
		/// The TNEF data is corrupt or invalid.
		/// </exception>
		public bool ReadNextProperty ()
		{
			if (propertyIndex >= propertyCount)
				return false;

			while (ReadNextValue ()) {
				// skip over the value...
			}

			try {
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

				switch (id) {
				case TnefPropertyId.AttachMethod:
					AttachMethod = (TnefAttachMethod) PeekInt32 ();
					break;
				}
			} catch (EndOfStreamException) {
				return false;
			}

			return CheckRawValueLength ();
		}

		/// <summary>
		/// Advances to the next table row of properties.
		/// </summary>
		/// <remarks>
		/// Advances to the next table row of properties.
		/// </remarks>
		/// <returns><c>true</c> if there is another row available to be read; otherwise <c>false</c>.</returns>
		/// <exception cref="TnefException">
		/// The TNEF data is corrupt or invalid.
		/// </exception>
		public bool ReadNextRow ()
		{
			if (rowIndex >= rowCount)
				return false;

			while (ReadNextProperty ()) {
				// skip over the property...
			}

			try {
				LoadPropertyCount ();
				rowIndex++;
			} catch (EndOfStreamException) {
				reader.SetComplianceError (TnefComplianceStatus.StreamTruncated);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Advances to the next value in the TNEF stream.
		/// </summary>
		/// <remarks>
		/// Advances to the next value in the TNEF stream.
		/// </remarks>
		/// <returns><c>true</c> if there is another value available to be read; otherwise <c>false</c>.</returns>
		/// <exception cref="TnefException">
		/// The TNEF data is corrupt or invalid.
		/// </exception>
		public bool ReadNextValue ()
		{
			if (valueIndex >= valueCount || propertyCount == 0)
				return false;

			int offset = RawValueStreamOffset + RawValueLength;

			if (reader.StreamOffset < offset && !reader.Seek (offset))
				return false;

			try {
				if (!TryGetPropertyValueLength (out rawValueLength))
					return false;

				rawValueOffset = reader.StreamOffset;
				valueIndex++;
			} catch (EndOfStreamException) {
				return false;
			}

			return CheckRawValueLength ();
		}

		/// <summary>
		/// Reads the raw attribute or property value as a sequence of bytes.
		/// </summary>
		/// <remarks>
		/// Reads the raw attribute or property value as a sequence of bytes.
		/// </remarks>
		/// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many
		/// bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
		/// <param name="buffer">The buffer to read data into.</param>
		/// <param name="offset">The offset into the buffer to start reading data.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="offset"/> is less than zero or greater than the length of <paramref name="buffer"/>.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="buffer"/> is not large enough to contain <paramref name="count"/> bytes starting
		/// at the specified <paramref name="offset"/>.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public int ReadRawValue (byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");

			if (count < 0 || count > (buffer.Length - offset))
				throw new ArgumentOutOfRangeException ("count");

			if (propertyCount > 0 && reader.StreamOffset == RawValueStreamOffset) {
				switch (propertyTag.ValueTnefType) {
				case TnefPropertyType.Unicode:
				case TnefPropertyType.String8:
				case TnefPropertyType.Binary:
				case TnefPropertyType.Object:
					ReadInt32 ();
					break;
				}
			}

			int valueEndOffset = RawValueStreamOffset + RawValueLength;
			int valueLeft = valueEndOffset - reader.StreamOffset;
			int n = Math.Min (valueLeft, count);

			return n > 0 ? reader.ReadAttributeRawValue (buffer, offset, n) : 0;
		}

		/// <summary>
		/// Reads the raw attribute or property value as a sequence of unicode characters.
		/// </summary>
		/// <remarks>
		/// Reads the raw attribute or property value as a sequence of unicode characters.
		/// </remarks>
		/// <returns>The total number of characters read into the buffer. This can be less than the number of characters
		/// requested if that many bytes are not currently available, or zero (0) if the end of the stream has been
		/// reached.</returns>
		/// <param name="buffer">The buffer to read data into.</param>
		/// <param name="offset">The offset into the buffer to start reading data.</param>
		/// <param name="count">The number of characters to read.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="offset"/> is less than zero or greater than the length of <paramref name="buffer"/>.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="buffer"/> is not large enough to contain <paramref name="count"/> characters starting
		/// at the specified <paramref name="offset"/>.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public int ReadTextValue (char[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");

			if (count < 0 || count > (buffer.Length - offset))
				throw new ArgumentOutOfRangeException ("count");

			if (reader.StreamOffset == RawValueStreamOffset && decoder == null)
				throw new InvalidOperationException ();

			if (propertyCount > 0 && reader.StreamOffset == RawValueStreamOffset) {
				switch (propertyTag.ValueTnefType) {
				case TnefPropertyType.Unicode:
					ReadInt32 ();
					decoder = (Decoder) Encoding.Unicode.GetDecoder ();
					break;
				case TnefPropertyType.String8:
				case TnefPropertyType.Binary:
				case TnefPropertyType.Object:
					ReadInt32 ();
					decoder = (Decoder) GetMessageEncoding ().GetDecoder ();
					break;
				}
			}

			int valueEndOffset = RawValueStreamOffset + RawValueLength;
			int valueLeft = valueEndOffset - reader.StreamOffset;
			int n = Math.Min (valueLeft, count);

			if (n <= 0)
				return 0;

			var bytes = new byte[n];

			n = reader.ReadAttributeRawValue (bytes, 0, bytes.Length);

			var flush = reader.StreamOffset >= valueEndOffset;

			return decoder.GetChars (bytes, 0, n, buffer, offset, flush);
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
				length = 16;
				break;
			case TnefPropertyType.Unicode:
			case TnefPropertyType.String8:
			case TnefPropertyType.Binary:
			case TnefPropertyType.Object:
				length = 4 + GetPaddedLength (PeekInt32 ());
				break;
			case TnefPropertyType.AppTime:
			case TnefPropertyType.SysTime:
				length = 8;
				break;
			default:
				reader.SetComplianceError (TnefComplianceStatus.UnsupportedPropertyType);
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
			case TnefPropertyType.ClassId:  return typeof (Guid);
			case TnefPropertyType.Object:   return typeof (byte[]);
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
				value = new Guid (ReadBytes (16));
				break;
			case TnefPropertyType.Object:
				value = ReadByteArray ();
				break;
			default:
				reader.SetComplianceError (TnefComplianceStatus.UnsupportedPropertyType);
				value = null;
				break;
			}

			valueIndex++;

			return value;
		}

		/// <summary>
		/// Reads the value.
		/// </summary>
		/// <remarks>
		/// Reads an attribute or property value as its native type.
		/// </remarks>
		/// <returns>The value.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// There are no more values to read or the value could not be read.
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">
		/// The TNEF stream is truncated and the value could not be read.
		/// </exception>
		public object ReadValue ()
		{
			if (valueIndex >= valueCount || reader.StreamOffset > RawValueStreamOffset)
				throw new InvalidOperationException ();

			if (propertyCount > 0)
				return ReadPropertyValue ();

			object value = null;

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
			}

			valueIndex++;

			return value;
		}

		/// <summary>
		/// Reads the value as a boolean.
		/// </summary>
		/// <remarks>
		/// Reads any integer-based attribute or property value as a boolean.
		/// </remarks>
		/// <returns>The value as a boolean.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// There are no more values to read or the value could not be read as a boolean.
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">
		/// The TNEF stream is truncated and the value could not be read.
		/// </exception>
		public bool ReadValueAsBoolean ()
		{
			if (valueIndex >= valueCount || reader.StreamOffset > RawValueStreamOffset)
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

		/// <summary>
		/// Reads the value as a byte array.
		/// </summary>
		/// <remarks>
		/// Reads any string, binary blob, Class ID, or Object attribute or property value as a byte array.
		/// </remarks>
		/// <returns>The value as a byte array.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// There are no more values to read or the value could not be read as a byte array.
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">
		/// The TNEF stream is truncated and the value could not be read.
		/// </exception>
		public byte[] ReadValueAsBytes ()
		{
			if (valueIndex >= valueCount || reader.StreamOffset > RawValueStreamOffset)
				throw new InvalidOperationException ();

			byte[] bytes;

			if (propertyCount > 0) {
				switch (propertyTag.ValueTnefType) {
				case TnefPropertyType.Unicode:
				case TnefPropertyType.String8:
				case TnefPropertyType.Binary:
				case TnefPropertyType.Object:
					bytes = ReadByteArray ();
					break;
				case TnefPropertyType.ClassId:
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
		/// <remarks>
		/// Reads any date and time attribute or property value as a <see cref="DateTime"/>.
		/// </remarks>
		/// <returns>The value as a date and time.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// There are no more values to read or the value could not be read as a date and time.
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">
		/// The TNEF stream is truncated and the value could not be read.
		/// </exception>
		public DateTime ReadValueAsDateTime ()
		{
			if (valueIndex >= valueCount || reader.StreamOffset > RawValueStreamOffset)
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
		/// <remarks>
		/// Reads any numeric attribute or property value as a double.
		/// </remarks>
		/// <returns>The value as a double.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// There are no more values to read or the value could not be read as a double.
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">
		/// The TNEF stream is truncated and the value could not be read.
		/// </exception>
		public double ReadValueAsDouble ()
		{
			if (valueIndex >= valueCount || reader.StreamOffset > RawValueStreamOffset)
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
		/// <remarks>
		/// Reads any numeric attribute or property value as a float.
		/// </remarks>
		/// <returns>The value as a float.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// There are no more values to read or the value could not be read as a float.
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">
		/// The TNEF stream is truncated and the value could not be read.
		/// </exception>
		public float ReadValueAsFloat ()
		{
			if (valueIndex >= valueCount || reader.StreamOffset > RawValueStreamOffset)
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
		/// <remarks>
		/// Reads any Class ID value as a GUID.
		/// </remarks>
		/// <returns>The value as a GUID.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// There are no more values to read or the value could not be read as a GUID.
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">
		/// The TNEF stream is truncated and the value could not be read.
		/// </exception>
		public Guid ReadValueAsGuid ()
		{
			if (valueIndex >= valueCount || reader.StreamOffset > RawValueStreamOffset)
				throw new InvalidOperationException ();

			Guid guid;

			if (propertyCount > 0) {
				switch (propertyTag.ValueTnefType) {
				case TnefPropertyType.ClassId:
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
		/// <remarks>
		/// Reads any integer-based attribute or property value as a 16-bit integer.
		/// </remarks>
		/// <returns>The value as a 16-bit integer.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// There are no more values to read or the value could not be read as a 16-bit integer.
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">
		/// The TNEF stream is truncated and the value could not be read.
		/// </exception>
		public short ReadValueAsInt16 ()
		{
			if (valueIndex >= valueCount || reader.StreamOffset > RawValueStreamOffset)
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
		/// <remarks>
		/// Reads any integer-based attribute or property value as a 32-bit integer.
		/// </remarks>
		/// <returns>The value as a 32-bit integer.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// There are no more values to read or the value could not be read as a 32-bit integer.
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">
		/// The TNEF stream is truncated and the value could not be read.
		/// </exception>
		public int ReadValueAsInt32 ()
		{
			if (valueIndex >= valueCount || reader.StreamOffset > RawValueStreamOffset)
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
		/// <remarks>
		/// Reads any integer-based attribute or property value as a 64-bit integer.
		/// </remarks>
		/// <returns>The value as a 64-bit integer.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// There are no more values to read or the value could not be read as a 64-bit integer.
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">
		/// The TNEF stream is truncated and the value could not be read.
		/// </exception>
		public long ReadValueAsInt64 ()
		{
			if (valueIndex >= valueCount || reader.StreamOffset > RawValueStreamOffset)
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
		/// <remarks>
		/// Reads any string or binary blob values as a string.
		/// </remarks>
		/// <returns>The value as a string.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// There are no more values to read or the value could not be read as a string.
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">
		/// The TNEF stream is truncated and the value could not be read.
		/// </exception>
		public string ReadValueAsString ()
		{
			if (valueIndex >= valueCount || reader.StreamOffset > RawValueStreamOffset)
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
		/// <remarks>
		/// Serves as a hash function for a <see cref="MimeKit.Tnef.TnefPropertyReader"/> object.
		/// </remarks>
		/// <returns>A hash code for this instance that is suitable for use in hashing algorithms
		/// and data structures such as a hash table.</returns>
		public override int GetHashCode ()
		{
			return reader.GetHashCode ();
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="MimeKit.Tnef.TnefPropertyReader"/>.
		/// </summary>
		/// <remarks>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="MimeKit.Tnef.TnefPropertyReader"/>.
		/// </remarks>
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
				reader.SetComplianceError (TnefComplianceStatus.InvalidPropertyLength);
				propertyCount = 0;
			}

			propertyIndex = 0;
			valueCount = 0;
			valueIndex = 0;
			decoder = null;
		}

		int ReadValueCount ()
		{
			int count;

			if ((count = ReadInt32 ()) < 0) {
				reader.SetComplianceError (TnefComplianceStatus.InvalidAttributeValue);
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
			decoder = null;
		}

		void LoadRowCount ()
		{
			if ((rowCount = ReadInt32 ()) < 0) {
				reader.SetComplianceError (TnefComplianceStatus.InvalidRowCount);
				rowCount = 0;
			}

			propertyCount = 0;
			propertyIndex = 0;
			valueCount = 0;
			valueIndex = 0;
			decoder = null;
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
			decoder = null;
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

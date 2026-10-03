//
// TnefPropertyWriter.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
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
using System.Buffers.Binary;

using MimeKit.IO;
using MimeKit.Utils;

namespace MimeKit.Tnef {
	/// <summary>
	/// A forward-only writer for the MAPI properties of a TNEF attribute.
	/// </summary>
	/// <remarks>
	/// <para>A <see cref="TnefPropertyWriter"/> writes the MAPI properties of a <see cref="TnefAttributeTag.MapiProperties"/>,
	/// <see cref="TnefAttributeTag.Attachment"/> or <see cref="TnefAttributeTag.RecipientTable"/> attribute. It is
	/// obtained by calling <see cref="TnefWriter.OpenPropertyWriter(TnefAttributeTag)"/>.</para>
	/// <para>Each property is written by calling one of the <c>WritePropertyTag</c> methods followed by the property's
	/// values. A single-valued property must have exactly one value, except for a <see cref="TnefPropertyType.Null"/>
	/// property which has none. A multi-valued property may have any number of values. Alternatively, a property
	/// that was read using a <see cref="TnefPropertyReader"/> may be copied using <see cref="WriteProperty(TnefProperty)"/>.</para>
	/// <para>For a <see cref="TnefAttributeTag.RecipientTable"/> attribute, each row of properties must be started by
	/// calling <see cref="BeginRow"/>.</para>
	/// <para>The property and value counts, the value lengths and the padding are written automatically. The attribute
	/// is completed when the property writer is disposed. If the attribute is incomplete at that point (for example,
	/// because a single-valued property has no value), the attribute is discarded and the next call to the
	/// <see cref="TnefWriter"/> throws an <see cref="InvalidOperationException"/>.</para>
	/// </remarks>
	public sealed class TnefPropertyWriter : IDisposable
	{
		const int ClassIdLength = 16;

		static readonly byte[] Padding = new byte[4];

		readonly MemoryBlockStream buffer;
		readonly byte[] scratch = new byte[16];
		readonly TnefWriter writer;
		readonly bool isTable;

		// The offset of the current property count, or -1 if a row has not been started.
		long countOffset;
		int propertyCount, rowCount;

		// The current property.
		TnefPropertyTag tag;
		bool hasProperty;
		long valueCountOffset;
		int valueCount;

		// The value that is currently being written by a stream or embedded message writer.
		object? child;
		long valueLengthOffset;

		string? fault;
		bool disposed;

		internal TnefPropertyWriter (TnefWriter writer, MemoryBlockStream buffer, bool isTable)
		{
			this.isTable = isTable;
			this.writer = writer;
			this.buffer = buffer;

			// Note: Both a property list and a table begin with a 32-bit count (of properties or rows).
			WriteInt32 (0);
			countOffset = isTable ? -1 : 0;
		}

		/// <summary>
		/// Get the number of properties.
		/// </summary>
		/// <remarks>
		/// Gets the number of properties that have been written to the attribute or, for a
		/// <see cref="TnefAttributeTag.RecipientTable"/> attribute, to the current row.
		/// </remarks>
		/// <value>The number of properties.</value>
		public int PropertyCount {
			get { return propertyCount; }
		}

		/// <summary>
		/// Get the number of rows.
		/// </summary>
		/// <remarks>
		/// Gets the number of rows that have been started in a <see cref="TnefAttributeTag.RecipientTable"/> attribute.
		/// </remarks>
		/// <value>The number of rows.</value>
		public int RowCount {
			get { return rowCount; }
		}

		void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (nameof (TnefPropertyWriter));
		}

		void CheckCanWrite ()
		{
			CheckDisposed ();

			if (fault != null)
				throw new InvalidOperationException (fault);

			if (child != null)
				throw new InvalidOperationException ("The stream or writer for the previous value must be disposed first.");
		}

		void WriteInt32 (int value)
		{
			BinaryPrimitives.WriteInt32LittleEndian (scratch, value);
			buffer.Write (scratch, 0, 4);
		}

		// Note: [MS-OXTNEF] 2.1.3.5: A property tag is the 16-bit property type followed by the 16-bit property id.
		void WriteTag (TnefPropertyTag value)
		{
			BinaryPrimitives.WriteInt16LittleEndian (scratch, (short) value.TnefType);
			BinaryPrimitives.WriteInt16LittleEndian (scratch.AsSpan (2), (short) value.Id);
			buffer.Write (scratch, 0, 4);
		}

		void PatchInt32 (long offset, int value)
		{
			long end = buffer.Length;

			BinaryPrimitives.WriteInt32LittleEndian (scratch, value);
			buffer.Position = offset;
			buffer.Write (scratch, 0, 4);
			buffer.Position = end;
		}

		void WritePadding (long length)
		{
			int padding = (int) ((4 - (length & 3)) & 3);

			if (padding > 0)
				buffer.Write (Padding, 0, padding);
		}

		static void ValidatePropertyType (TnefPropertyType type, string paramName)
		{
			var valueType = (TnefPropertyType) ((short) type & ~(short) TnefPropertyType.MultiValued);
			bool multiValued = valueType != type;

			if (valueType == TnefPropertyType.Unspecified || TnefPropertyReader.GetFixedWidth (valueType) == -2)
				throw new ArgumentException (string.Format ("The {0} property type cannot be written.", valueType), paramName);

			if (multiValued) {
				switch (valueType) {
				case TnefPropertyType.Null:
				case TnefPropertyType.Error:
				case TnefPropertyType.Boolean:
				case TnefPropertyType.Object:
					throw new ArgumentException (string.Format ("The {0} property type cannot be multi-valued.", valueType), paramName);
				}
			}
		}

		// Completes the current property.
		void EndProperty ()
		{
			if (!hasProperty)
				return;

			if (tag.IsMultiValued) {
				PatchInt32 (valueCountOffset, valueCount);
			} else if (tag.ValueTnefType == TnefPropertyType.Null) {
				// Note: A Null property has no value.
			} else if (valueCount != 1) {
				throw new InvalidOperationException (string.Format ("The single-valued {0} property must have exactly one value.", tag));
			}

			hasProperty = false;
		}

		void EndRow ()
		{
			EndProperty ();

			if (countOffset >= 0)
				PatchInt32 (countOffset, propertyCount);
		}

		/// <summary>
		/// Begin a new row of properties.
		/// </summary>
		/// <remarks>
		/// Completes the current row (if any) and begins a new row of properties in a
		/// <see cref="TnefAttributeTag.RecipientTable"/> attribute.
		/// </remarks>
		/// <exception cref="System.ObjectDisposedException">
		/// The property writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The attribute is not a <see cref="TnefAttributeTag.RecipientTable"/> attribute.</para>
		/// <para>-or-</para>
		/// <para>The previous property does not have the required number of values.</para>
		/// <para>-or-</para>
		/// <para>The stream or writer for the previous value has not been disposed.</para>
		/// </exception>
		public void BeginRow ()
		{
			CheckCanWrite ();

			if (!isTable)
				throw new InvalidOperationException ("Only a RecipientTable attribute contains rows.");

			EndRow ();

			countOffset = buffer.Length;
			propertyCount = 0;
			WriteInt32 (0);
			rowCount++;
		}

		void BeginProperty (TnefPropertyTag value)
		{
			hasProperty = true;
			valueCount = 0;
			tag = value;
			propertyCount++;

			if (tag.IsMultiValued) {
				valueCountOffset = buffer.Length;
				WriteInt32 (0);
			} else if (TnefPropertyReader.GetFixedWidth (tag.ValueTnefType) == -1) {
				// Note: A single variable-length value is also preceded by a count.
				WriteInt32 (1);
			}
		}

		void CheckCanWriteProperty ()
		{
			CheckCanWrite ();

			if (countOffset < 0)
				throw new InvalidOperationException ("BeginRow must be called before writing the properties of a row.");
		}

		/// <summary>
		/// Begin a new property.
		/// </summary>
		/// <remarks>
		/// <para>Completes the current property (if any) and begins a new property with the specified tag. The
		/// property's values must be written next.</para>
		/// <para>To write a named property, use <see cref="WritePropertyTag(TnefNameId, TnefPropertyType)"/>.</para>
		/// </remarks>
		/// <param name="tag">The property tag.</param>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="tag"/> is a named property tag.</para>
		/// <para>-or-</para>
		/// <para>The type of <paramref name="tag"/> cannot be written.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The property writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The previous property does not have the required number of values.</para>
		/// <para>-or-</para>
		/// <para>A row has not been started in a <see cref="TnefAttributeTag.RecipientTable"/> attribute.</para>
		/// <para>-or-</para>
		/// <para>The stream or writer for the previous value has not been disposed.</para>
		/// </exception>
		public void WritePropertyTag (TnefPropertyTag tag)
		{
			if (tag.IsNamed)
				throw new ArgumentException ("Named properties must be written using a TnefNameId.", nameof (tag));

			ValidatePropertyType (tag.TnefType, nameof (tag));
			CheckCanWriteProperty ();
			EndProperty ();

			WriteTag (tag);
			BeginProperty (tag);
		}

		/// <summary>
		/// Begin a new named property.
		/// </summary>
		/// <remarks>
		/// <para>Completes the current property (if any) and begins a new named property. The property's values must
		/// be written next.</para>
		/// <para>The property id of the named property is assigned by the <see cref="TnefWriter"/>. The same name
		/// is always assigned the same id within a TNEF stream.</para>
		/// </remarks>
		/// <param name="name">The property name.</param>
		/// <param name="type">The property type.</param>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="type"/> cannot be written.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The property writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The previous property does not have the required number of values.</para>
		/// <para>-or-</para>
		/// <para>A row has not been started in a <see cref="TnefAttributeTag.RecipientTable"/> attribute.</para>
		/// <para>-or-</para>
		/// <para>The stream or writer for the previous value has not been disposed.</para>
		/// <para>-or-</para>
		/// <para>The TNEF stream contains too many named properties.</para>
		/// </exception>
		public void WritePropertyTag (TnefNameId name, TnefPropertyType type)
		{
			if (name.Kind != TnefNameIdKind.Id && name.Kind != TnefNameIdKind.Name)
				throw new ArgumentException ("The kind of property name is not supported.", nameof (name));

			ValidatePropertyType (type, nameof (type));
			CheckCanWriteProperty ();
			EndProperty ();

			int id = writer.GetNamedPropertyId (name);
			var value = new TnefPropertyTag ((TnefPropertyId) id, type);

			WriteTag (value);
			buffer.Write (name.PropertySetGuid.ToByteArray (), 0, 16);
			WriteInt32 ((int) name.Kind);

			if (name.Kind == TnefNameIdKind.Id) {
				WriteInt32 (name.Id);
			} else {
				// Note: [MS-OXTNEF] 2.1.3.5: The name is a nul-terminated UTF-16LE string preceded by its length in bytes
				// (including the terminator) and padded to a multiple of 4 bytes.
				var text = name.Name ?? string.Empty;
				int length = Encoding.Unicode.GetByteCount (text) + 2;
				var bytes = new byte[length];

				Encoding.Unicode.GetBytes (text, 0, text.Length, bytes, 0);
				WriteInt32 (length);
				buffer.Write (bytes, 0, length);
				WritePadding (length);
			}

			BeginProperty (value);
		}

		// Validates that a value of the specified type may be written next.
		void CheckValue (TnefPropertyType type1, TnefPropertyType type2 = TnefPropertyType.Unspecified)
		{
			CheckCanWrite ();

			if (!hasProperty)
				throw new InvalidOperationException ("WritePropertyTag must be called before writing a value.");

			var type = tag.ValueTnefType;

			if (type != type1 && type != type2)
				throw new InvalidOperationException (string.Format ("The value cannot be written to the {0} property.", tag));

			if (!tag.IsMultiValued && valueCount > 0)
				throw new InvalidOperationException (string.Format ("The single-valued {0} property already has a value.", tag));
		}

		void WriteFixedValue (int width)
		{
			buffer.Write (scratch, 0, width);
			valueCount++;
		}

		void WriteVariableValue (byte[] value, int startIndex, int length)
		{
			WriteInt32 (length);
			buffer.Write (value, startIndex, length);
			WritePadding (length);
			valueCount++;
		}

		/// <summary>
		/// Write a boolean value.
		/// </summary>
		/// <remarks>
		/// Writes a value of a <see cref="TnefPropertyType.Boolean"/> property.
		/// </remarks>
		/// <param name="value">The value.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The property writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The current property is not a <see cref="TnefPropertyType.Boolean"/> property.</para>
		/// <para>-or-</para>
		/// <para>The current property already has a value.</para>
		/// <para>-or-</para>
		/// <para>The stream or writer for the previous value has not been disposed.</para>
		/// </exception>
		public void WriteValue (bool value)
		{
			CheckValue (TnefPropertyType.Boolean);

			BinaryPrimitives.WriteInt32LittleEndian (scratch, value ? 1 : 0);
			WriteFixedValue (4);
		}

		/// <summary>
		/// Write a 16-bit integer value.
		/// </summary>
		/// <remarks>
		/// Writes a value of an <see cref="TnefPropertyType.I2"/> property.
		/// </remarks>
		/// <param name="value">The value.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The property writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The current property is not an <see cref="TnefPropertyType.I2"/> property.</para>
		/// <para>-or-</para>
		/// <para>The current property already has a value.</para>
		/// <para>-or-</para>
		/// <para>The stream or writer for the previous value has not been disposed.</para>
		/// </exception>
		public void WriteValue (short value)
		{
			CheckValue (TnefPropertyType.I2);

			// Note: [MS-OXTNEF] pads 16-bit values to 4 bytes.
			BinaryPrimitives.WriteInt16LittleEndian (scratch, value);
			scratch[2] = scratch[3] = 0;
			WriteFixedValue (4);
		}

		/// <summary>
		/// Write a 32-bit integer value.
		/// </summary>
		/// <remarks>
		/// Writes a value of a <see cref="TnefPropertyType.Long"/> or <see cref="TnefPropertyType.Error"/> property.
		/// </remarks>
		/// <param name="value">The value.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The property writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The current property is not a <see cref="TnefPropertyType.Long"/> or
		/// <see cref="TnefPropertyType.Error"/> property.</para>
		/// <para>-or-</para>
		/// <para>The current property already has a value.</para>
		/// <para>-or-</para>
		/// <para>The stream or writer for the previous value has not been disposed.</para>
		/// </exception>
		public void WriteValue (int value)
		{
			CheckValue (TnefPropertyType.Long, TnefPropertyType.Error);

			BinaryPrimitives.WriteInt32LittleEndian (scratch, value);
			WriteFixedValue (4);
		}

		/// <summary>
		/// Write a 64-bit integer value.
		/// </summary>
		/// <remarks>
		/// Writes a value of an <see cref="TnefPropertyType.I8"/> property.
		/// </remarks>
		/// <param name="value">The value.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The property writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The current property is not an <see cref="TnefPropertyType.I8"/> property.</para>
		/// <para>-or-</para>
		/// <para>The current property already has a value.</para>
		/// <para>-or-</para>
		/// <para>The stream or writer for the previous value has not been disposed.</para>
		/// </exception>
		public void WriteValue (long value)
		{
			CheckValue (TnefPropertyType.I8);

			BinaryPrimitives.WriteInt64LittleEndian (scratch, value);
			WriteFixedValue (8);
		}

		/// <summary>
		/// Write a single-precision floating-point value.
		/// </summary>
		/// <remarks>
		/// Writes a value of an <see cref="TnefPropertyType.R4"/> property.
		/// </remarks>
		/// <param name="value">The value.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The property writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The current property is not an <see cref="TnefPropertyType.R4"/> property.</para>
		/// <para>-or-</para>
		/// <para>The current property already has a value.</para>
		/// <para>-or-</para>
		/// <para>The stream or writer for the previous value has not been disposed.</para>
		/// </exception>
		public void WriteValue (float value)
		{
			CheckValue (TnefPropertyType.R4);

			BinaryPrimitives.WriteInt32LittleEndian (scratch, BitConverter.ToInt32 (BitConverter.GetBytes (value), 0));
			WriteFixedValue (4);
		}

		/// <summary>
		/// Write a double-precision floating-point value.
		/// </summary>
		/// <remarks>
		/// Writes a value of a <see cref="TnefPropertyType.Double"/> property.
		/// </remarks>
		/// <param name="value">The value.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The property writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The current property is not a <see cref="TnefPropertyType.Double"/> property.</para>
		/// <para>-or-</para>
		/// <para>The current property already has a value.</para>
		/// <para>-or-</para>
		/// <para>The stream or writer for the previous value has not been disposed.</para>
		/// </exception>
		public void WriteValue (double value)
		{
			CheckValue (TnefPropertyType.Double);

			BinaryPrimitives.WriteInt64LittleEndian (scratch, BitConverter.DoubleToInt64Bits (value));
			WriteFixedValue (8);
		}

		/// <summary>
		/// Write a currency value.
		/// </summary>
		/// <remarks>
		/// Writes a value of a <see cref="TnefPropertyType.Currency"/> property. The value is scaled by 10000 and
		/// rounded to the nearest integer (rounding to even), as required by the 64-bit fixed-point format defined
		/// by [MS-OXCDATA].
		/// </remarks>
		/// <param name="value">The value.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> cannot be represented as a currency value.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The property writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The current property is not a <see cref="TnefPropertyType.Currency"/> property.</para>
		/// <para>-or-</para>
		/// <para>The current property already has a value.</para>
		/// <para>-or-</para>
		/// <para>The stream or writer for the previous value has not been disposed.</para>
		/// </exception>
		public void WriteValue (decimal value)
		{
			CheckValue (TnefPropertyType.Currency);

			long scaled;

			try {
				scaled = decimal.ToInt64 (decimal.Round (value * 10000m, MidpointRounding.ToEven));
			} catch (OverflowException) {
				throw new ArgumentOutOfRangeException (nameof (value), "The value cannot be represented as a currency value.");
			}

			BinaryPrimitives.WriteInt64LittleEndian (scratch, scaled);
			WriteFixedValue (8);
		}

		/// <summary>
		/// Write a date and time value.
		/// </summary>
		/// <remarks>
		/// <para>Writes a value of a <see cref="TnefPropertyType.SysTime"/> or <see cref="TnefPropertyType.AppTime"/>
		/// property.</para>
		/// <para>A <see cref="TnefPropertyType.SysTime"/> value is written in UTC. If the <see cref="DateTime.Kind"/> of
		/// <paramref name="value"/> is <see cref="DateTimeKind.Local"/>, it is converted to UTC; otherwise, it is assumed
		/// to already be in UTC. A <see cref="TnefPropertyType.AppTime"/> value is written as-is.</para>
		/// </remarks>
		/// <param name="value">The value.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is earlier than the earliest time that can be represented by the property type.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The property writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The current property is not a <see cref="TnefPropertyType.SysTime"/> or
		/// <see cref="TnefPropertyType.AppTime"/> property.</para>
		/// <para>-or-</para>
		/// <para>The current property already has a value.</para>
		/// <para>-or-</para>
		/// <para>The stream or writer for the previous value has not been disposed.</para>
		/// </exception>
		public void WriteValue (DateTime value)
		{
			CheckValue (TnefPropertyType.SysTime, TnefPropertyType.AppTime);

			long bits;

			if (tag.ValueTnefType == TnefPropertyType.SysTime) {
				// Note: [MS-OXCDATA] defines PtypTime as a FILETIME, which is the number of 100-nanosecond intervals
				// since January 1, 1601 UTC.
				try {
					bits = value.ToFileTimeUtc ();
				} catch (ArgumentOutOfRangeException) {
					throw new ArgumentOutOfRangeException (nameof (value), "The value is earlier than January 1, 1601.");
				}
			} else {
				double oaDate;

				try {
					oaDate = value.ToOADate ();
				} catch (OverflowException) {
					throw new ArgumentOutOfRangeException (nameof (value), "The value cannot be represented as an OLE Automation date.");
				}

				bits = BitConverter.DoubleToInt64Bits (oaDate);
			}

			BinaryPrimitives.WriteInt64LittleEndian (scratch, bits);
			WriteFixedValue (8);
		}

		/// <summary>
		/// Write a GUID value.
		/// </summary>
		/// <remarks>
		/// Writes a value of a <see cref="TnefPropertyType.ClassId"/> property.
		/// </remarks>
		/// <param name="value">The value.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The property writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The current property is not a <see cref="TnefPropertyType.ClassId"/> property.</para>
		/// <para>-or-</para>
		/// <para>The current property already has a value.</para>
		/// <para>-or-</para>
		/// <para>The stream or writer for the previous value has not been disposed.</para>
		/// </exception>
		public void WriteValue (Guid value)
		{
			CheckValue (TnefPropertyType.ClassId);

			var bytes = value.ToByteArray ();

			Buffer.BlockCopy (bytes, 0, scratch, 0, ClassIdLength);
			WriteFixedValue (ClassIdLength);
		}

		/// <summary>
		/// Write a string value.
		/// </summary>
		/// <remarks>
		/// <para>Writes a value of a <see cref="TnefPropertyType.String8"/> or <see cref="TnefPropertyType.Unicode"/>
		/// property, terminated by a nul character.</para>
		/// <para>A <see cref="TnefPropertyType.String8"/> value is encoded using the <see cref="TnefWriter.Codepage"/>
		/// and a <see cref="TnefPropertyType.Unicode"/> value is encoded using UTF-16LE.</para>
		/// </remarks>
		/// <param name="value">The value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The property writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The current property is not a <see cref="TnefPropertyType.String8"/> or
		/// <see cref="TnefPropertyType.Unicode"/> property.</para>
		/// <para>-or-</para>
		/// <para>The current property already has a value.</para>
		/// <para>-or-</para>
		/// <para>The stream or writer for the previous value has not been disposed.</para>
		/// </exception>
		public void WriteValue (string value)
		{
			if (value is null)
				throw new ArgumentNullException (nameof (value));

			CheckValue (TnefPropertyType.String8, TnefPropertyType.Unicode);

			Encoding encoding;
			int terminator;

			if (tag.ValueTnefType == TnefPropertyType.Unicode) {
				encoding = Encoding.Unicode;
				terminator = 2;
			} else {
				encoding = writer.Encoding;
				terminator = 1;
			}

			int length = encoding.GetByteCount (value) + terminator;
			var bytes = new byte[length];

			encoding.GetBytes (value, 0, value.Length, bytes, 0);
			WriteVariableValue (bytes, 0, length);
		}

		/// <summary>
		/// Write a binary value.
		/// </summary>
		/// <remarks>
		/// <para>Writes a value of a <see cref="TnefPropertyType.Binary"/> or <see cref="TnefPropertyType.Object"/>
		/// property.</para>
		/// <para>An <see cref="TnefPropertyType.Object"/> value must begin with the 16-byte interface identifier of
		/// the object.</para>
		/// </remarks>
		/// <param name="buffer">The buffer containing the value.</param>
		/// <param name="startIndex">The index of the first byte of the value.</param>
		/// <param name="length">The length of the value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the <paramref name="buffer"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The value of an <see cref="TnefPropertyType.Object"/> property is shorter than 16 bytes.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The property writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The current property is not a <see cref="TnefPropertyType.Binary"/> or
		/// <see cref="TnefPropertyType.Object"/> property.</para>
		/// <para>-or-</para>
		/// <para>The current property already has a value.</para>
		/// <para>-or-</para>
		/// <para>The stream or writer for the previous value has not been disposed.</para>
		/// </exception>
		public void WriteValue (byte[] buffer, int startIndex, int length)
		{
			ArgumentValidator.Validate (buffer, startIndex, length);

			CheckValue (TnefPropertyType.Binary, TnefPropertyType.Object);

			if (tag.ValueTnefType == TnefPropertyType.Object && length < ClassIdLength)
				throw new ArgumentException ("The value of an Object property must begin with a 16-byte interface identifier.", nameof (length));

			WriteVariableValue (buffer, startIndex, length);
		}

		/// <summary>
		/// Write a binary value.
		/// </summary>
		/// <remarks>
		/// <para>Writes a value of a <see cref="TnefPropertyType.Binary"/> or <see cref="TnefPropertyType.Object"/>
		/// property.</para>
		/// <para>An <see cref="TnefPropertyType.Object"/> value must begin with the 16-byte interface identifier of
		/// the object.</para>
		/// </remarks>
		/// <param name="buffer">The value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The value of an <see cref="TnefPropertyType.Object"/> property is shorter than 16 bytes.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The property writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The current property is not a <see cref="TnefPropertyType.Binary"/> or
		/// <see cref="TnefPropertyType.Object"/> property.</para>
		/// <para>-or-</para>
		/// <para>The current property already has a value.</para>
		/// <para>-or-</para>
		/// <para>The stream or writer for the previous value has not been disposed.</para>
		/// </exception>
		public void WriteValue (byte[] buffer)
		{
			if (buffer is null)
				throw new ArgumentNullException (nameof (buffer));

			WriteValue (buffer, 0, buffer.Length);
		}

		// Completes a variable-length value that was written by a stream or embedded message writer.
		void OnValueClosed (object closedChild, string? error)
		{
			if (child != closedChild)
				return;

			child = null;

			if (error != null) {
				fault = error;
				return;
			}

			long length = buffer.Length - (valueLengthOffset + 4);

			if (length > int.MaxValue) {
				fault = string.Format ("The value of the {0} property is too large.", tag);
				return;
			}

			if (tag.ValueTnefType == TnefPropertyType.Object && length < ClassIdLength) {
				fault = "The value of an Object property must begin with a 16-byte interface identifier.";
				return;
			}

			PatchInt32 (valueLengthOffset, (int) length);
			WritePadding (length);
			valueCount++;
		}

		void BeginStreamValue ()
		{
			valueLengthOffset = buffer.Length;
			WriteInt32 (0);
		}

		/// <summary>
		/// Open a stream for writing a binary value.
		/// </summary>
		/// <remarks>
		/// <para>Opens a stream for writing a value of a <see cref="TnefPropertyType.Binary"/> or
		/// <see cref="TnefPropertyType.Object"/> property.</para>
		/// <para>An <see cref="TnefPropertyType.Object"/> value must begin with the 16-byte interface identifier of
		/// the object.</para>
		/// <para>The value is completed when the stream is disposed. No other value or property may be written until
		/// then.</para>
		/// </remarks>
		/// <returns>The value stream.</returns>
		/// <exception cref="System.ObjectDisposedException">
		/// The property writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The current property is not a <see cref="TnefPropertyType.Binary"/> or
		/// <see cref="TnefPropertyType.Object"/> property.</para>
		/// <para>-or-</para>
		/// <para>The current property already has a value.</para>
		/// <para>-or-</para>
		/// <para>The stream or writer for the previous value has not been disposed.</para>
		/// </exception>
		public Stream OpenValueStream ()
		{
			CheckValue (TnefPropertyType.Binary, TnefPropertyType.Object);
			BeginStreamValue ();

			var stream = new TnefValueWriteStream (buffer, s => OnValueClosed (s, null));

			child = stream;

			return stream;
		}

		/// <summary>
		/// Open a stream for writing a compressed RTF value.
		/// </summary>
		/// <remarks>
		/// <para>Opens a stream that encodes the RTF that is written to it as a compressed RTF value of a
		/// <see cref="TnefPropertyType.Binary"/> property, such as <see cref="TnefPropertyId.RtfCompressed"/>, as
		/// defined by [MS-OXRTFCP].</para>
		/// <para>If <paramref name="compress"/> is <see langword="true"/>, the RTF is compressed using
		/// <see cref="RtfCompressionMode.Compressed"/>; otherwise, it is stored using
		/// <see cref="RtfCompressionMode.Uncompressed"/>.</para>
		/// <para>The value is completed when the stream is disposed. No other value or property may be written until
		/// then.</para>
		/// </remarks>
		/// <returns>The compressed RTF stream.</returns>
		/// <param name="compress"><see langword="true"/> to compress the RTF; otherwise, <see langword="false"/>.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The property writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The current property is not a <see cref="TnefPropertyType.Binary"/> property.</para>
		/// <para>-or-</para>
		/// <para>The current property already has a value.</para>
		/// <para>-or-</para>
		/// <para>The stream or writer for the previous value has not been disposed.</para>
		/// </exception>
		public Stream OpenRtfCompressedStream (bool compress = true)
		{
			CheckValue (TnefPropertyType.Binary);
			BeginStreamValue ();

			var stream = new RtfCompressedWriteStream (buffer, compress, (s, error) => OnValueClosed (s, error));

			child = stream;

			return stream;
		}

		/// <summary>
		/// Open a writer for an embedded message.
		/// </summary>
		/// <remarks>
		/// <para>Opens a <see cref="TnefWriter"/> for writing an embedded message as the value of an
		/// <see cref="TnefPropertyType.Object"/> property, such as <see cref="TnefPropertyId.AttachData"/>. The value
		/// begins with the IID_IMessage interface identifier, which is written automatically.</para>
		/// <para>The embedded message uses the same codepage as the <see cref="TnefWriter"/> that this property
		/// writer belongs to.</para>
		/// <para>The value is completed when the embedded message writer is disposed. No other value or property may
		/// be written until then.</para>
		/// </remarks>
		/// <returns>The embedded message writer.</returns>
		/// <exception cref="System.ObjectDisposedException">
		/// The property writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The current property is not an <see cref="TnefPropertyType.Object"/> property.</para>
		/// <para>-or-</para>
		/// <para>The current property already has a value.</para>
		/// <para>-or-</para>
		/// <para>The stream or writer for the previous value has not been disposed.</para>
		/// </exception>
		public TnefWriter OpenEmbeddedMessage ()
		{
			return OpenEmbeddedMessage (writer.Codepage);
		}

		/// <summary>
		/// Open a writer for an embedded message.
		/// </summary>
		/// <remarks>
		/// <para>Opens a <see cref="TnefWriter"/> for writing an embedded message as the value of an
		/// <see cref="TnefPropertyType.Object"/> property, such as <see cref="TnefPropertyId.AttachData"/>. The value
		/// begins with the IID_IMessage interface identifier, which is written automatically.</para>
		/// <para>The value is completed when the embedded message writer is disposed. No other value or property may
		/// be written until then.</para>
		/// </remarks>
		/// <returns>The embedded message writer.</returns>
		/// <param name="codepage">The codepage that the embedded message writer uses to encode 8-bit strings.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="codepage"/> is not a supported codepage, or is a codepage that encodes characters
		/// using zero bytes.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The property writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The current property is not an <see cref="TnefPropertyType.Object"/> property.</para>
		/// <para>-or-</para>
		/// <para>The current property already has a value.</para>
		/// <para>-or-</para>
		/// <para>The stream or writer for the previous value has not been disposed.</para>
		/// </exception>
		public TnefWriter OpenEmbeddedMessage (int codepage)
		{
			CheckValue (TnefPropertyType.Object);

			// Note: The embedded message writer writes nothing until it is used, so create it first in order to
			// validate the codepage.
			var embedded = new TnefWriter (new TnefValueWriteStream (buffer, null), codepage, (w, error) => OnValueClosed (w, error));

			BeginStreamValue ();
			buffer.Write (TnefPropertyReader.IID_IMessage.ToByteArray (), 0, ClassIdLength);
			child = embedded;

			return embedded;
		}

		static Type? GetNativeType (TnefPropertyType type)
		{
			switch (type) {
			case TnefPropertyType.I2: return typeof (short);
			case TnefPropertyType.Long: return typeof (int);
			case TnefPropertyType.Error: return typeof (int);
			case TnefPropertyType.R4: return typeof (float);
			case TnefPropertyType.Double: return typeof (double);
			case TnefPropertyType.Currency: return typeof (decimal);
			case TnefPropertyType.AppTime: return typeof (DateTime);
			case TnefPropertyType.SysTime: return typeof (DateTime);
			case TnefPropertyType.Boolean: return typeof (bool);
			case TnefPropertyType.I8: return typeof (long);
			case TnefPropertyType.ClassId: return typeof (Guid);
			case TnefPropertyType.String8: return typeof (string);
			case TnefPropertyType.Unicode: return typeof (string);
			case TnefPropertyType.Binary: return typeof (byte[]);
			case TnefPropertyType.Object: return typeof (byte[]);
			default: return null;
			}
		}

		static bool IsValidValue (TnefPropertyType type, object? value)
		{
			var nativeType = GetNativeType (type);

			if (value is null || value.GetType () != nativeType)
				return false;

			return type != TnefPropertyType.Object || ((byte[]) value).Length >= ClassIdLength;
		}

		void WriteNativeValue (object value)
		{
			switch (value) {
			case short i2: WriteValue (i2); break;
			case int i4: WriteValue (i4); break;
			case float r4: WriteValue (r4); break;
			case double r8: WriteValue (r8); break;
			case decimal currency: WriteValue (currency); break;
			case DateTime date: WriteValue (date); break;
			case bool boolean: WriteValue (boolean); break;
			case long i8: WriteValue (i8); break;
			case Guid guid: WriteValue (guid); break;
			case string text: WriteValue (text); break;
			case byte[] bytes: WriteValue (bytes); break;
			}
		}

		/// <summary>
		/// Write a property.
		/// </summary>
		/// <remarks>
		/// <para>Writes a property and all of its values, such as a property that was read using
		/// <see cref="TnefPropertyReader.ReadProperty(System.Threading.CancellationToken)"/>.</para>
		/// <para>A named property is written using its <see cref="TnefProperty.Name"/>, so it is assigned a new
		/// property id. A <see cref="TnefPropertyType.String8"/> value is encoded using the
		/// <see cref="TnefWriter.Codepage"/>.</para>
		/// </remarks>
		/// <param name="property">The property.</param>
		/// <exception cref="System.ArgumentException">
		/// <para>The type of <paramref name="property"/> cannot be written.</para>
		/// <para>-or-</para>
		/// <para><paramref name="property"/> is a named property without a name.</para>
		/// <para>-or-</para>
		/// <para>The value of <paramref name="property"/> is not of the native type of the property.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// A value of <paramref name="property"/> cannot be represented by the property type.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The property writer has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The previous property does not have the required number of values.</para>
		/// <para>-or-</para>
		/// <para>A row has not been started in a <see cref="TnefAttributeTag.RecipientTable"/> attribute.</para>
		/// <para>-or-</para>
		/// <para>The stream or writer for the previous value has not been disposed.</para>
		/// </exception>
		public void WriteProperty (TnefProperty property)
		{
			var propertyTag = property.Tag;
			var type = propertyTag.ValueTnefType;

			ValidatePropertyType (propertyTag.TnefType, nameof (property));

			if (propertyTag.IsNamed && property.Name is null)
				throw new ArgumentException ("The named property does not have a name.", nameof (property));

			// Note: Validate the values before writing anything so that an invalid property is not partially written.
			if (type == TnefPropertyType.Null) {
				if (property.Value != null)
					throw new ArgumentException ("A Null property does not have a value.", nameof (property));
			} else if (propertyTag.IsMultiValued) {
				if (!(property.Value is Array values) || values.GetType ().GetElementType () != GetNativeType (type))
					throw new ArgumentException (string.Format ("The value of the {0} property is not an array of its native type.", propertyTag), nameof (property));

				foreach (var value in values) {
					if (!IsValidValue (type, value))
						throw new ArgumentException (string.Format ("The {0} property contains an invalid value.", propertyTag), nameof (property));
				}
			} else if (!IsValidValue (type, property.Value)) {
				throw new ArgumentException (string.Format ("The value of the {0} property is not of its native type.", propertyTag), nameof (property));
			}

			if (property.Name is TnefNameId name)
				WritePropertyTag (name, propertyTag.TnefType);
			else
				WritePropertyTag (propertyTag);

			if (type == TnefPropertyType.Null)
				return;

			if (propertyTag.IsMultiValued) {
				foreach (var value in (Array) property.Value!)
					WriteNativeValue (value!);
			} else {
				WriteNativeValue (property.Value!);
			}
		}

		/// <summary>
		/// Release all resources used by the <see cref="TnefPropertyWriter"/> object.
		/// </summary>
		/// <remarks>
		/// <para>Completes the attribute.</para>
		/// <para>If the attribute is incomplete (for example, because a single-valued property has no value or a value
		/// stream has not been disposed), the attribute is discarded and the next call to the <see cref="TnefWriter"/>
		/// throws an <see cref="InvalidOperationException"/>.</para>
		/// </remarks>
		public void Dispose ()
		{
			if (disposed)
				return;

			string? error = fault;

			disposed = true;

			if (error is null && child != null)
				error = "The stream or writer for a property value was not disposed.";

			if (error is null) {
				try {
					EndRow ();

					if (isTable)
						PatchInt32 (0, rowCount);
				} catch (InvalidOperationException ex) {
					error = ex.Message;
				}
			}

			child = null;
			writer.OnChildClosed (this, error);
		}
	}
}

//
// TnefPropertyReader.cs
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
using System.Threading;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace MimeKit.Tnef {
	/// <summary>
	/// A forward-only reader for the MAPI properties contained within a TNEF attribute.
	/// </summary>
	/// <remarks>
	/// <para>A <see cref="TnefPropertyReader"/> provides forward-only access to the MAPI properties contained within
	/// the <see cref="TnefAttributeTag.MapiProperties"/>, <see cref="TnefAttributeTag.Attachment"/> and
	/// <see cref="TnefAttributeTag.RecipientTable"/> attributes. It is obtained by calling
	/// <see cref="TnefReader.GetPropertyReader"/>.</para>
	/// <para>For a <see cref="TnefAttributeTag.RecipientTable"/> attribute, each row of properties is selected by
	/// calling <see cref="ReadNextRow(CancellationToken)"/> before reading the row's properties with
	/// <see cref="ReadNextProperty(CancellationToken)"/>.</para>
	/// <para>When positioned on a property, the reader is also positioned on the property's first value (if it has
	/// any). Additional values of a multi-valued property are selected using <see cref="ReadNextValue(CancellationToken)"/>.</para>
	/// <para>Fixed-width values (integers, floating point values, dates, booleans and GUIDs) are decoded when the
	/// reader is positioned on them and may be read any number of times. Variable-length values (strings, binary
	/// values and objects) may only be read once.</para>
	/// <para>The property reader is only valid until the <see cref="TnefReader"/> is advanced to the next attribute.
	/// After that, any use of the property reader throws <see cref="InvalidOperationException"/>.</para>
	/// <para>Like the <see cref="TnefReader"/>, the property reader never throws because of malformed data. Problems
	/// are reported to the reader's <see cref="TnefReader.ComplianceLogger"/>, and if the remainder of the attribute
	/// cannot be interpreted, the reader simply stops returning properties.</para>
	/// </remarks>
	public sealed partial class TnefPropertyReader
	{
		// {00020307-0000-0000-C000-000000000046}
		static readonly Guid IID_IMessage = new Guid (0x00020307, 0x0000, 0x0000, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		// Note: Fixed-width values are decoded from this buffer. The largest fixed-width value is a 16-byte GUID.
		readonly byte[] scratch = new byte[16];
		readonly TnefReader reader;
		readonly int generation;
		readonly bool isTable;

		// The state of the reader.
		int rowCount = -1, rowIndex;
		int propertyCount = -1, propertyIndex;
		bool inRow, stopped;

		// The state of the current property.
		TnefPropertyTag tag = TnefPropertyTag.Null;
		TnefNameId? name;
		bool hasProperty;
		int valueCount, valueIndex;

		// The state of the current value.
		bool hasValue, consumed;
		long dataStart, dataEnd, valueEnd;
		DateTime dateValue;
		Guid objectIid;

		internal TnefPropertyReader (TnefReader reader, bool isTable)
		{
			generation = reader.Generation;
			this.isTable = isTable;
			this.reader = reader;
		}

		/// <summary>
		/// Get the number of table rows.
		/// </summary>
		/// <remarks>
		/// <para>Gets the number of table rows in a <see cref="TnefAttributeTag.RecipientTable"/> attribute.</para>
		/// <para>The row count is read by the first call to <see cref="ReadNextRow(CancellationToken)"/>, so until then,
		/// and for attributes that do not contain a table, it is <c>0</c>.</para>
		/// </remarks>
		/// <value>The row count.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="TnefReader"/> has been advanced to another attribute.
		/// </exception>
		public int RowCount {
			get {
				CheckGeneration ();
				return Math.Max (rowCount, 0);
			}
		}

		/// <summary>
		/// Get the number of properties.
		/// </summary>
		/// <remarks>
		/// <para>Gets the number of properties in the attribute or, for a <see cref="TnefAttributeTag.RecipientTable"/>
		/// attribute, in the current row.</para>
		/// <para>The property count is read by the first call to <see cref="ReadNextProperty(CancellationToken)"/> (or, for
		/// a table, by <see cref="ReadNextRow(CancellationToken)"/>), so until then it is <c>0</c>.</para>
		/// </remarks>
		/// <value>The property count.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="TnefReader"/> has been advanced to another attribute.
		/// </exception>
		public int PropertyCount {
			get {
				CheckGeneration ();
				return Math.Max (propertyCount, 0);
			}
		}

		/// <summary>
		/// Get the tag of the current property.
		/// </summary>
		/// <remarks>
		/// Gets the tag of the current property.
		/// </remarks>
		/// <value>The property tag.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="TnefReader"/> has been advanced to another attribute.
		/// </exception>
		public TnefPropertyTag Tag {
			get {
				CheckGeneration ();
				return tag;
			}
		}

		/// <summary>
		/// Get the type of the current property's values.
		/// </summary>
		/// <remarks>
		/// Gets the type of the current property's values, without the <see cref="TnefPropertyType.MultiValued"/> flag.
		/// </remarks>
		/// <value>The property type.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="TnefReader"/> has been advanced to another attribute.
		/// </exception>
		public TnefPropertyType PropertyType {
			get {
				CheckGeneration ();
				return tag.ValueTnefType;
			}
		}

		/// <summary>
		/// Get whether the current property is multi-valued.
		/// </summary>
		/// <remarks>
		/// Gets whether the current property is multi-valued.
		/// </remarks>
		/// <value><see langword="true"/> if the current property is multi-valued; otherwise, <see langword="false"/>.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="TnefReader"/> has been advanced to another attribute.
		/// </exception>
		public bool IsMultiValued {
			get {
				CheckGeneration ();
				return tag.IsMultiValued;
			}
		}

		/// <summary>
		/// Get the name of the current property.
		/// </summary>
		/// <remarks>
		/// Gets the name of the current property if it is a named property; otherwise, <see langword="null"/>.
		/// </remarks>
		/// <value>The property name.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="TnefReader"/> has been advanced to another attribute.
		/// </exception>
		public TnefNameId? Name {
			get {
				CheckGeneration ();
				return name;
			}
		}

		/// <summary>
		/// Get the number of values of the current property.
		/// </summary>
		/// <remarks>
		/// <para>Gets the number of values of the current property.</para>
		/// <para>A malformed property may have no values at all, in which case there is no value to read.</para>
		/// </remarks>
		/// <value>The value count.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="TnefReader"/> has been advanced to another attribute.
		/// </exception>
		public int ValueCount {
			get {
				CheckGeneration ();
				return valueCount;
			}
		}

		/// <summary>
		/// Get whether the current variable-length value has already been read.
		/// </summary>
		/// <remarks>
		/// <para>Gets whether the current variable-length value has already been read.</para>
		/// <para>Variable-length values may only be read once. Fixed-width values may be read any number of times
		/// and are never considered to be consumed.</para>
		/// </remarks>
		/// <value><see langword="true"/> if the current value has been read; otherwise, <see langword="false"/>.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="TnefReader"/> has been advanced to another attribute.
		/// </exception>
		public bool IsValueConsumed {
			get {
				CheckGeneration ();
				return consumed;
			}
		}

		/// <summary>
		/// Get whether the current value is an embedded TNEF message.
		/// </summary>
		/// <remarks>
		/// <para>Gets whether the current value is an embedded TNEF message.</para>
		/// <para>An embedded message is an <see cref="TnefPropertyId.AttachData"/> value that is either a
		/// <see cref="TnefPropertyType.Object"/> value prefixed with the IID_IMessage interface identifier, or a value
		/// of an attachment whose <see cref="TnefPropertyId.AttachMethod"/> has already been read and is
		/// <see cref="TnefAttachMethod.EmbeddedMessage"/>. It can be read using <see cref="OpenEmbeddedMessage"/>.</para>
		/// </remarks>
		/// <value><see langword="true"/> if the current value is an embedded message; otherwise, <see langword="false"/>.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="TnefReader"/> has been advanced to another attribute.
		/// </exception>
		public bool IsEmbeddedMessage {
			get {
				CheckGeneration ();
				return IsEmbeddedMessageCore;
			}
		}

		// Whether the reader is positioned on a value.
		internal bool HasValue {
			get {
				CheckGeneration ();
				return hasValue;
			}
		}

		bool IsEmbeddedMessageCore {
			get {
				if (!hasValue || tag.Id != TnefPropertyId.AttachData)
					return false;

				// Note: [MS-OXTNEF] prefixes PtypObject values with the IID of the object's interface, which is
				// IID_IMessage for an embedded message. Since writers typically emit the attachment properties in
				// ascending order of property id, PidTagAttachMethod (0x3705) usually comes *after*
				// PidTagAttachDataObject (0x3701), so the IID is the only reliable indicator.
				if (tag.ValueTnefType == TnefPropertyType.Object && objectIid != Guid.Empty)
					return objectIid == IID_IMessage;

				if (reader.AttachMethod != TnefAttachMethod.EmbeddedMessage)
					return false;

				return tag.ValueTnefType == TnefPropertyType.Object || tag.ValueTnefType == TnefPropertyType.Binary;
			}
		}

		long Remaining {
			get { return Math.Max (reader.ValueEnd - reader.LocalOffset, 0); }
		}

		void CheckGeneration ()
		{
			reader.CheckGeneration (generation);
		}

		void Log (TnefComplianceViolation violation, long offset)
		{
			reader.Log (violation, offset, tag);
		}

		// Note: Once the structure of the attribute can no longer be trusted, there is no way to locate the
		// properties that follow, so the reader stops.
		void Stop ()
		{
			stopped = true;
			hasProperty = false;
			hasValue = false;
			inRow = false;
		}

		// Checks that the attribute has enough data left for the next structure.
		bool CheckAvailable (int count, TnefComplianceViolation violation)
		{
			if (Remaining >= count)
				return true;

			Log (violation, reader.LocalOffset);
			Stop ();

			return false;
		}

		// Checks that the stream did not end before the attribute did.
		bool CheckFilled (bool filled)
		{
			if (filled)
				return true;

			reader.SetTruncated (tag);
			Stop ();

			return false;
		}

		static int GetMaxCount (long remaining, int width)
		{
			return (int) Math.Min (remaining / width, int.MaxValue);
		}

		void LoadRowCount (int count, long offset)
		{
			// Note: Every row begins with a 32-bit property count.
			int max = GetMaxCount (Remaining, 4);

			if (count < 0 || count > max) {
				reader.Log (TnefComplianceViolation.InvalidRowCount, offset);
				count = count < 0 ? 0 : max;
			}

			rowCount = count;
			rowIndex = 0;
		}

		void LoadPropertyCount (int count, long offset)
		{
			// Note: Every property begins with a 32-bit property tag.
			int max = GetMaxCount (Remaining, 4);

			if (count < 0 || count > max) {
				reader.Log (TnefComplianceViolation.InvalidPropertyCount, offset);
				count = count < 0 ? 0 : max;
			}

			propertyCount = count;
			propertyIndex = 0;
		}

		// Gets the width of a fixed-width value, -1 for a variable-length value, or -2 for an unsupported type.
		static int GetFixedWidth (TnefPropertyType type)
		{
			switch (type) {
			case TnefPropertyType.Null:
				return 0;
			case TnefPropertyType.I2:
			case TnefPropertyType.Long:
			case TnefPropertyType.R4:
			case TnefPropertyType.Error:
			case TnefPropertyType.Boolean:
				// Note: [MS-OXTNEF] pads 16-bit values out to 4 bytes.
				return 4;
			case TnefPropertyType.Double:
			case TnefPropertyType.Currency:
			case TnefPropertyType.AppTime:
			case TnefPropertyType.SysTime:
			case TnefPropertyType.I8:
				return 8;
			case TnefPropertyType.ClassId:
				return 16;
			case TnefPropertyType.Unicode:
			case TnefPropertyType.String8:
			case TnefPropertyType.Binary:
			case TnefPropertyType.Object:
				return -1;
			default:
				return -2;
			}
		}

		// Starts a new property once its 32-bit property tag has been buffered.
		void LoadPropertyTag ()
		{
			var type = (TnefPropertyType) reader.TakeInt16 ();
			var id = (TnefPropertyId) reader.TakeInt16 ();

			tag = new TnefPropertyTag (id, type);
			hasProperty = true;
			valueCount = 0;
			valueIndex = -1;
			name = null;
		}

		// Reads the GUID and kind of a named property once they have been buffered.
		TnefNameIdKind LoadNameHeader (out Guid guid)
		{
			reader.TakeBytes (scratch, 0, 16);
			guid = new Guid (scratch);

			return (TnefNameIdKind) reader.TakeInt32 ();
		}

		bool CheckNameLength (int length, long offset)
		{
			if (length >= 0 && length <= Remaining)
				return true;

			Log (TnefComplianceViolation.InvalidPropertyLength, offset);
			Stop ();

			return false;
		}

		// Gets the number of padding bytes that follow a variable-length value of the specified length.
		int GetPadding (int length)
		{
			int padding = (4 - (length & 3)) & 3;

			if (padding > Remaining) {
				Log (TnefComplianceViolation.InvalidPropertyLength, reader.LocalOffset);
				padding = (int) Remaining;
			}

			return padding;
		}

		static string DecodeUnicode (byte[] bytes)
		{
			int length = bytes.Length & ~1;

			// Note: Unicode strings are usually nul-terminated.
			while (length > 1 && bytes[length - 1] == 0 && bytes[length - 2] == 0)
				length -= 2;

			if (length == 0)
				return string.Empty;

			return Encoding.Unicode.GetString (bytes, 0, length);
		}

		// Returns false if the property type is not one that can be parsed.
		bool CheckPropertyType (long offset)
		{
			if (GetFixedWidth (tag.ValueTnefType) != -2)
				return true;

			// Note: The length of a value of an unknown type is unknowable, so there is no way to locate the
			// properties that follow.
			Log (TnefComplianceViolation.UnsupportedPropertyType, offset);
			Stop ();

			return false;
		}

		bool HasValueCount {
			get { return tag.IsMultiValued || GetFixedWidth (tag.ValueTnefType) == -1; }
		}

		void LoadValueCount (int count, long offset)
		{
			int width = GetFixedWidth (tag.ValueTnefType);

			// Note: Variable-length values begin with a 32-bit length, and there is no way to bound a count
			// of zero-width values, so treat them as being 4 bytes wide.
			int max = GetMaxCount (Remaining, width > 0 ? width : 4);

			if (tag.IsMultiValued) {
				if (count < 0 || count > max) {
					Log (TnefComplianceViolation.InvalidValueCount, offset);
					count = count < 0 ? 0 : max;
				}
			} else {
				// Note: A single-valued property should have exactly one value, but if it claims to have some
				// other number of values, honor it so that the properties that follow can still be located.
				if (count != 1)
					Log (TnefComplianceViolation.InvalidValueCount, offset);

				if (count < 0)
					count = 0;
				else if (count > max)
					count = max;
			}

			valueCount = count;
		}

		// Begins positioning the reader on the next value of the current property.
		int BeginValue ()
		{
			reader.IncrementValueGeneration ();
			valueIndex++;
			consumed = false;
			hasValue = false;
			objectIid = Guid.Empty;

			return GetFixedWidth (tag.ValueTnefType);
		}

		bool HasObjectIid {
			get { return tag.ValueTnefType == TnefPropertyType.Object && dataEnd - dataStart >= 16; }
		}

		// Peeks at the interface identifier that prefixes an object value once it has been buffered.
		void LoadObjectIid ()
		{
			reader.PeekBytes (scratch, 0, 16);
			objectIid = new Guid (scratch);
		}

		// Loads a fixed-width value once it has been buffered.
		void LoadFixedValue (int width)
		{
			long offset = reader.LocalOffset;

			reader.TakeBytes (scratch, 0, width);
			dataStart = offset;
			dataEnd = valueEnd = reader.LocalOffset;
			hasValue = true;

			switch (tag.ValueTnefType) {
			case TnefPropertyType.AppTime:
				var appTime = BitConverter.Int64BitsToDouble (BinaryPrimitives.ReadInt64LittleEndian (scratch.AsSpan (0, 8)));

				try {
					dateValue = DateTime.FromOADate (appTime);
				} catch (ArgumentException) {
					Log (TnefComplianceViolation.InvalidDate, offset);
					dateValue = default;
				}
				break;
			case TnefPropertyType.SysTime:
				// Note: [MS-OXCDATA] defines PtypTime as a FILETIME, which is the number of 100-nanosecond
				// intervals since January 1, 1601 UTC.
				long fileTime = BinaryPrimitives.ReadInt64LittleEndian (scratch.AsSpan (0, 8));

				try {
					dateValue = DateTime.FromFileTimeUtc (fileTime);
				} catch (ArgumentOutOfRangeException) {
					Log (TnefComplianceViolation.InvalidDate, offset);
					dateValue = default;
				}
				break;
			case TnefPropertyType.Long:
				if (tag.Id == TnefPropertyId.AttachMethod)
					reader.AttachMethod = (TnefAttachMethod) BinaryPrimitives.ReadInt32LittleEndian (scratch.AsSpan (0, 4));
				break;
			}
		}

		// Loads a variable-length value once its 32-bit length has been buffered.
		bool LoadVariableValue ()
		{
			long offset = reader.LocalOffset;
			int length = reader.TakeInt32 ();

			if (length < 0 || length > Remaining) {
				Log (TnefComplianceViolation.InvalidPropertyLength, offset);
				Stop ();
				return false;
			}

			dataStart = reader.LocalOffset;
			dataEnd = dataStart + length;
			valueEnd = dataEnd + ((4 - (length & 3)) & 3);

			if (valueEnd > reader.ValueEnd) {
				Log (TnefComplianceViolation.InvalidPropertyLength, dataEnd);
				valueEnd = reader.ValueEnd;
			}

			hasValue = true;

			return true;
		}

		bool PositionNextValue (CancellationToken cancellationToken)
		{
			int width = BeginValue ();

			if (width >= 0) {
				if (!CheckAvailable (width, TnefComplianceViolation.InvalidPropertyLength) || !CheckFilled (reader.Fill (width, cancellationToken)))
					return false;

				LoadFixedValue (width);
				return true;
			}

			if (!CheckAvailable (4, TnefComplianceViolation.InvalidPropertyLength) || !CheckFilled (reader.Fill (4, cancellationToken)))
				return false;

			if (!LoadVariableValue ())
				return false;

			// Note: If the stream ends before the IID, the truncation is reported when the value is read or skipped.
			if (HasObjectIid && reader.Fill (16, cancellationToken))
				LoadObjectIid ();

			return true;
		}

		// Gets the number of bytes of the current value that remain to be skipped.
		long BeginSkipValue ()
		{
			if (!hasValue)
				return 0;

			hasValue = false;

			return valueEnd - reader.LocalOffset;
		}

		bool CanAdvanceValue {
			get { return hasProperty && !stopped; }
		}

		bool HasNextValue {
			get { return valueIndex + 1 < valueCount; }
		}

		// Skips whatever remains of the current value, then positions the reader on the next value.
		bool AdvanceValue (CancellationToken cancellationToken)
		{
			if (!CanAdvanceValue)
				return false;

			long skip = BeginSkipValue ();

			if (skip > 0 && !CheckFilled (reader.Skip (skip, cancellationToken)))
				return false;

			if (!HasNextValue)
				return false;

			return PositionNextValue (cancellationToken);
		}

		bool SkipProperty (CancellationToken cancellationToken)
		{
			while (AdvanceValue (cancellationToken)) {
				// skip over the remaining value(s) of the current property...
			}

			hasProperty = false;

			return !stopped;
		}

		// Returns true if the reader needs to read the property count before it can read the next property.
		bool NeedsPropertyCount {
			get { return !isTable && propertyCount < 0; }
		}

		// Returns true if there is another property to read (once the property count, if any, has been read).
		bool BeginProperty (out long offset)
		{
			offset = reader.LocalOffset;

			if ((isTable && !inRow) || propertyIndex >= propertyCount)
				return false;

			propertyIndex++;
			tag = TnefPropertyTag.Null;

			return true;
		}

		bool ReadNextPropertyCore (CancellationToken cancellationToken)
		{
			if (!SkipProperty (cancellationToken))
				return false;

			if (NeedsPropertyCount) {
				long countOffset = reader.LocalOffset;

				if (!CheckAvailable (4, TnefComplianceViolation.InvalidPropertyCount) || !CheckFilled (reader.Fill (4, cancellationToken)))
					return false;

				LoadPropertyCount (reader.TakeInt32 (), countOffset);
			}

			if (!BeginProperty (out long offset))
				return false;

			if (!CheckAvailable (4, TnefComplianceViolation.InvalidPropertyLength) || !CheckFilled (reader.Fill (4, cancellationToken)))
				return false;

			LoadPropertyTag ();

			if (tag.IsNamed) {
				if (!CheckAvailable (20, TnefComplianceViolation.InvalidPropertyLength) || !CheckFilled (reader.Fill (20, cancellationToken)))
					return false;

				var kind = LoadNameHeader (out var guid);

				if (kind == TnefNameIdKind.Id) {
					if (!CheckAvailable (4, TnefComplianceViolation.InvalidPropertyLength) || !CheckFilled (reader.Fill (4, cancellationToken)))
						return false;

					name = new TnefNameId (guid, reader.TakeInt32 ());
				} else if (kind == TnefNameIdKind.Name) {
					if (!CheckAvailable (4, TnefComplianceViolation.InvalidPropertyLength) || !CheckFilled (reader.Fill (4, cancellationToken)))
						return false;

					long lengthOffset = reader.LocalOffset;
					int length = reader.TakeInt32 ();

					if (!CheckNameLength (length, lengthOffset))
						return false;

					var bytes = reader.ReadValueBytes (length, tag, cancellationToken);

					if (bytes is null) {
						// Note: The name is too large to read into memory, so skip it.
						if (!CheckFilled (reader.Skip (length + GetPadding (length), cancellationToken)))
							return false;

						name = new TnefNameId (guid, string.Empty);
					} else {
						if (!CheckFilled (bytes.Length == length) || !CheckFilled (reader.Skip (GetPadding (length), cancellationToken)))
							return false;

						name = new TnefNameId (guid, DecodeUnicode (bytes));
					}
				} else {
					Log (TnefComplianceViolation.InvalidNamedPropertyKind, offset);
					name = new TnefNameId (guid, 0);
				}
			}

			if (!CheckPropertyType (offset))
				return false;

			if (HasValueCount) {
				long countOffset = reader.LocalOffset;

				if (!CheckAvailable (4, TnefComplianceViolation.InvalidPropertyLength) || !CheckFilled (reader.Fill (4, cancellationToken)))
					return false;

				LoadValueCount (reader.TakeInt32 (), countOffset);
			} else {
				valueCount = 1;
			}

			return valueCount == 0 || PositionNextValue (cancellationToken);
		}

		// Returns true if the reader needs to read the row count before it can read the next row.
		bool NeedsRowCount {
			get { return !inRow && rowCount < 0; }
		}

		// Returns true if there is another row to read (once the row count has been read).
		bool BeginRow (out long offset)
		{
			offset = reader.LocalOffset;
			inRow = false;

			if (rowIndex >= rowCount)
				return false;

			rowIndex++;
			propertyCount = 0;
			propertyIndex = 0;

			return true;
		}

		/// <summary>
		/// Advance to the next row of a table.
		/// </summary>
		/// <remarks>
		/// <para>Advances to the next row of a <see cref="TnefAttributeTag.RecipientTable"/> attribute. Any properties
		/// of the current row that have not been read are skipped.</para>
		/// <para>For attributes that do not contain a table, this method always returns <see langword="false"/>.</para>
		/// </remarks>
		/// <returns><see langword="true"/> if the reader was advanced to the next row; otherwise, <see langword="false"/>.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="TnefReader"/> has been advanced to another attribute.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public bool ReadNextRow (CancellationToken cancellationToken = default)
		{
			CheckGeneration ();

			if (!isTable || stopped)
				return false;

			if (inRow) {
				while (ReadNextPropertyCore (cancellationToken)) {
					// skip over the remaining properties of the current row...
				}

				if (stopped)
					return false;
			} else if (NeedsRowCount) {
				long countOffset = reader.LocalOffset;

				if (!CheckAvailable (4, TnefComplianceViolation.InvalidRowCount) || !CheckFilled (reader.Fill (4, cancellationToken)))
					return false;

				LoadRowCount (reader.TakeInt32 (), countOffset);
			}

			if (!BeginRow (out long offset))
				return false;

			if (!CheckAvailable (4, TnefComplianceViolation.InvalidPropertyCount) || !CheckFilled (reader.Fill (4, cancellationToken)))
				return false;

			LoadPropertyCount (reader.TakeInt32 (), offset);
			inRow = true;

			return true;
		}

		/// <summary>
		/// Advance to the next property.
		/// </summary>
		/// <remarks>
		/// <para>Advances to the next property. Any values of the current property that have not been read are
		/// skipped.</para>
		/// <para>When this method returns <see langword="true"/>, the reader is positioned on the first value of the
		/// property, unless the property has no values (see <see cref="ValueCount"/>).</para>
		/// <para>For a <see cref="TnefAttributeTag.RecipientTable"/> attribute, this method returns the properties of
		/// the current row and returns <see langword="false"/> until <see cref="ReadNextRow(CancellationToken)"/> has
		/// been called.</para>
		/// </remarks>
		/// <returns><see langword="true"/> if the reader was advanced to the next property; otherwise, <see langword="false"/>.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="TnefReader"/> has been advanced to another attribute.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public bool ReadNextProperty (CancellationToken cancellationToken = default)
		{
			CheckGeneration ();

			return ReadNextPropertyCore (cancellationToken);
		}

		/// <summary>
		/// Advance to the next value of the current property.
		/// </summary>
		/// <remarks>
		/// <para>Advances to the next value of the current (multi-valued) property. Any part of the current value
		/// that has not been read is skipped.</para>
		/// <para>Since the reader is already positioned on the first value of a property by
		/// <see cref="ReadNextProperty(CancellationToken)"/>, the values of a property are typically read using a
		/// <c>do { ... } while (reader.ReadNextValue ())</c> loop.</para>
		/// </remarks>
		/// <returns><see langword="true"/> if the reader was advanced to the next value; otherwise, <see langword="false"/>.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="TnefReader"/> has been advanced to another attribute.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public bool ReadNextValue (CancellationToken cancellationToken = default)
		{
			CheckGeneration ();

			return AdvanceValue (cancellationToken);
		}

		void CheckValue ()
		{
			CheckGeneration ();

			if (!hasValue)
				throw new InvalidOperationException ("The reader is not positioned on a value.");
		}

		static InvalidOperationException CannotReadAs (TnefPropertyType type, string what)
		{
			return new InvalidOperationException (string.Format ("A {0} value cannot be read as {1}.", type, what));
		}

		void ClaimVariableValue (string what)
		{
			CheckValue ();

			if (GetFixedWidth (tag.ValueTnefType) != -1)
				throw CannotReadAs (tag.ValueTnefType, what);

			if (consumed)
				throw new InvalidOperationException ("The value has already been read.");

			consumed = true;
		}

		short GetInt16 ()
		{
			return BinaryPrimitives.ReadInt16LittleEndian (scratch.AsSpan (0, 2));
		}

		int GetInt32 ()
		{
			return BinaryPrimitives.ReadInt32LittleEndian (scratch.AsSpan (0, 4));
		}

		long GetInt64 ()
		{
			return BinaryPrimitives.ReadInt64LittleEndian (scratch.AsSpan (0, 8));
		}

		float GetSingle ()
		{
			return BitConverter.ToSingle (scratch, 0);
		}

		double GetDouble ()
		{
			return BitConverter.Int64BitsToDouble (GetInt64 ());
		}

		/// <summary>
		/// Read the current value as a boolean.
		/// </summary>
		/// <remarks>
		/// Reads any integer-based value as a boolean.
		/// </remarks>
		/// <returns>The value as a boolean.</returns>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="TnefReader"/> has been advanced to another attribute.</para>
		/// <para>-or-</para>
		/// <para>The reader is not positioned on a value.</para>
		/// <para>-or-</para>
		/// <para>The value cannot be read as a boolean.</para>
		/// </exception>
		public bool ReadValueAsBoolean ()
		{
			CheckValue ();

			switch (tag.ValueTnefType) {
			case TnefPropertyType.Boolean: return (GetInt32 () & 0xFFFF) != 0;
			case TnefPropertyType.I2: return GetInt16 () != 0;
			case TnefPropertyType.Error:
			case TnefPropertyType.Long: return GetInt32 () != 0;
			case TnefPropertyType.Currency:
			case TnefPropertyType.I8: return GetInt64 () != 0;
			default: throw CannotReadAs (tag.ValueTnefType, "a boolean");
			}
		}

		/// <summary>
		/// Read the current value as a 16-bit integer.
		/// </summary>
		/// <remarks>
		/// Reads any numeric value as a 16-bit integer.
		/// </remarks>
		/// <returns>The value as a 16-bit integer.</returns>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="TnefReader"/> has been advanced to another attribute.</para>
		/// <para>-or-</para>
		/// <para>The reader is not positioned on a value.</para>
		/// <para>-or-</para>
		/// <para>The value cannot be read as a 16-bit integer.</para>
		/// </exception>
		public short ReadValueAsInt16 ()
		{
			CheckValue ();

			switch (tag.ValueTnefType) {
			case TnefPropertyType.Boolean: return (short) (GetInt32 () & 0xFFFF);
			case TnefPropertyType.I2: return GetInt16 ();
			case TnefPropertyType.Error:
			case TnefPropertyType.Long: return (short) GetInt32 ();
			case TnefPropertyType.Currency: return (short) (GetInt64 () / 10000);
			case TnefPropertyType.I8: return (short) GetInt64 ();
			case TnefPropertyType.Double: return (short) GetDouble ();
			case TnefPropertyType.R4: return (short) GetSingle ();
			default: throw CannotReadAs (tag.ValueTnefType, "a 16-bit integer");
			}
		}

		/// <summary>
		/// Read the current value as a 32-bit integer.
		/// </summary>
		/// <remarks>
		/// Reads any numeric value as a 32-bit integer.
		/// </remarks>
		/// <returns>The value as a 32-bit integer.</returns>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="TnefReader"/> has been advanced to another attribute.</para>
		/// <para>-or-</para>
		/// <para>The reader is not positioned on a value.</para>
		/// <para>-or-</para>
		/// <para>The value cannot be read as a 32-bit integer.</para>
		/// </exception>
		public int ReadValueAsInt32 ()
		{
			CheckValue ();

			switch (tag.ValueTnefType) {
			case TnefPropertyType.Boolean: return GetInt32 () & 0xFFFF;
			case TnefPropertyType.I2: return GetInt16 ();
			case TnefPropertyType.Error:
			case TnefPropertyType.Long: return GetInt32 ();
			case TnefPropertyType.Currency: return (int) (GetInt64 () / 10000);
			case TnefPropertyType.I8: return (int) GetInt64 ();
			case TnefPropertyType.Double: return (int) GetDouble ();
			case TnefPropertyType.R4: return (int) GetSingle ();
			default: throw CannotReadAs (tag.ValueTnefType, "a 32-bit integer");
			}
		}

		/// <summary>
		/// Read the current value as a 64-bit integer.
		/// </summary>
		/// <remarks>
		/// Reads any numeric value as a 64-bit integer.
		/// </remarks>
		/// <returns>The value as a 64-bit integer.</returns>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="TnefReader"/> has been advanced to another attribute.</para>
		/// <para>-or-</para>
		/// <para>The reader is not positioned on a value.</para>
		/// <para>-or-</para>
		/// <para>The value cannot be read as a 64-bit integer.</para>
		/// </exception>
		public long ReadValueAsInt64 ()
		{
			CheckValue ();

			switch (tag.ValueTnefType) {
			case TnefPropertyType.Boolean: return GetInt32 () & 0xFFFF;
			case TnefPropertyType.I2: return GetInt16 ();
			case TnefPropertyType.Error:
			case TnefPropertyType.Long: return GetInt32 ();
			case TnefPropertyType.Currency: return GetInt64 () / 10000;
			case TnefPropertyType.I8: return GetInt64 ();
			case TnefPropertyType.Double: return (long) GetDouble ();
			case TnefPropertyType.R4: return (long) GetSingle ();
			default: throw CannotReadAs (tag.ValueTnefType, "a 64-bit integer");
			}
		}

		/// <summary>
		/// Read the current value as a double.
		/// </summary>
		/// <remarks>
		/// Reads any numeric value as a double.
		/// </remarks>
		/// <returns>The value as a double.</returns>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="TnefReader"/> has been advanced to another attribute.</para>
		/// <para>-or-</para>
		/// <para>The reader is not positioned on a value.</para>
		/// <para>-or-</para>
		/// <para>The value cannot be read as a double.</para>
		/// </exception>
		public double ReadValueAsDouble ()
		{
			CheckValue ();

			switch (tag.ValueTnefType) {
			case TnefPropertyType.Boolean: return GetInt32 () & 0xFFFF;
			case TnefPropertyType.I2: return GetInt16 ();
			case TnefPropertyType.Error:
			case TnefPropertyType.Long: return GetInt32 ();
			case TnefPropertyType.Currency: return GetInt64 () / 10000.0;
			case TnefPropertyType.I8: return GetInt64 ();
			case TnefPropertyType.Double: return GetDouble ();
			case TnefPropertyType.R4: return GetSingle ();
			default: throw CannotReadAs (tag.ValueTnefType, "a double");
			}
		}

		/// <summary>
		/// Read the current value as a float.
		/// </summary>
		/// <remarks>
		/// Reads any numeric value as a float.
		/// </remarks>
		/// <returns>The value as a float.</returns>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="TnefReader"/> has been advanced to another attribute.</para>
		/// <para>-or-</para>
		/// <para>The reader is not positioned on a value.</para>
		/// <para>-or-</para>
		/// <para>The value cannot be read as a float.</para>
		/// </exception>
		public float ReadValueAsFloat ()
		{
			CheckValue ();

			switch (tag.ValueTnefType) {
			case TnefPropertyType.Boolean: return GetInt32 () & 0xFFFF;
			case TnefPropertyType.I2: return GetInt16 ();
			case TnefPropertyType.Error:
			case TnefPropertyType.Long: return GetInt32 ();
			case TnefPropertyType.Currency: return (float) (GetInt64 () / 10000.0);
			case TnefPropertyType.I8: return GetInt64 ();
			case TnefPropertyType.Double: return (float) GetDouble ();
			case TnefPropertyType.R4: return GetSingle ();
			default: throw CannotReadAs (tag.ValueTnefType, "a float");
			}
		}

		/// <summary>
		/// Read the current value as a date and time.
		/// </summary>
		/// <remarks>
		/// <para>Reads any <see cref="TnefPropertyType.AppTime"/> or <see cref="TnefPropertyType.SysTime"/> value as a
		/// <see cref="DateTime"/>.</para>
		/// <para>Values of type <see cref="TnefPropertyType.SysTime"/> are FILETIME values and are therefore returned with
		/// a <see cref="DateTime.Kind"/> of <see cref="DateTimeKind.Utc"/>.</para>
		/// <para>If the value is out of range, a <see cref="TnefComplianceViolation.InvalidDate"/> issue is reported
		/// and <c>default (DateTime)</c> is returned.</para>
		/// </remarks>
		/// <returns>The value as a date and time.</returns>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="TnefReader"/> has been advanced to another attribute.</para>
		/// <para>-or-</para>
		/// <para>The reader is not positioned on a value.</para>
		/// <para>-or-</para>
		/// <para>The value cannot be read as a date and time.</para>
		/// </exception>
		public DateTime ReadValueAsDateTime ()
		{
			CheckValue ();

			switch (tag.ValueTnefType) {
			case TnefPropertyType.AppTime:
			case TnefPropertyType.SysTime:
				return dateValue;
			default:
				throw CannotReadAs (tag.ValueTnefType, "a date and time");
			}
		}

		/// <summary>
		/// Read the current value as a GUID.
		/// </summary>
		/// <remarks>
		/// Reads a <see cref="TnefPropertyType.ClassId"/> value as a GUID.
		/// </remarks>
		/// <returns>The value as a GUID.</returns>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="TnefReader"/> has been advanced to another attribute.</para>
		/// <para>-or-</para>
		/// <para>The reader is not positioned on a value.</para>
		/// <para>-or-</para>
		/// <para>The value cannot be read as a GUID.</para>
		/// </exception>
		public Guid ReadValueAsGuid ()
		{
			CheckValue ();

			if (tag.ValueTnefType != TnefPropertyType.ClassId)
				throw CannotReadAs (tag.ValueTnefType, "a GUID");

			return new Guid (scratch);
		}

		bool TryGetFixedBytes (out byte[] bytes)
		{
			CheckValue ();

			if (tag.ValueTnefType == TnefPropertyType.ClassId) {
				bytes = new byte[16];
				Buffer.BlockCopy (scratch, 0, bytes, 0, 16);
				return true;
			}

			bytes = Array.Empty<byte> ();

			return false;
		}

		int VariableLength {
			get { return (int) (dataEnd - dataStart); }
		}

		/// <summary>
		/// Read the current value as a byte array.
		/// </summary>
		/// <remarks>
		/// <para>Reads any string, binary, object or <see cref="TnefPropertyType.ClassId"/> value as a byte array.</para>
		/// <para>Variable-length values may only be read once.</para>
		/// <para>If the value is larger than <see cref="TnefOptions.MaxPropertyValueLength"/> or the remaining
		/// <see cref="TnefOptions.MaxTotalDataBytes"/> allow, a <see cref="TnefComplianceViolation.DataSizeLimitExceeded"/> issue is
		/// reported and an empty value is returned.</para>
		/// </remarks>
		/// <returns>The value as a byte array.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="TnefReader"/> has been advanced to another attribute.</para>
		/// <para>-or-</para>
		/// <para>The reader is not positioned on a value.</para>
		/// <para>-or-</para>
		/// <para>The value cannot be read as a byte array.</para>
		/// <para>-or-</para>
		/// <para>The value has already been read.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public byte[] ReadValueAsBytes (CancellationToken cancellationToken = default)
		{
			if (TryGetFixedBytes (out var bytes))
				return bytes;

			ClaimVariableValue ("a byte array");

			return reader.ReadValueBytes (VariableLength, tag, cancellationToken) ?? Array.Empty<byte> ();
		}

		void ClaimStringValue ()
		{
			CheckValue ();

			switch (tag.ValueTnefType) {
			case TnefPropertyType.Unicode:
			case TnefPropertyType.String8:
			case TnefPropertyType.Binary:
				break;
			default:
				throw CannotReadAs (tag.ValueTnefType, "a string");
			}

			ClaimVariableValue ("a string");
		}

		string DecodeString (byte[]? bytes)
		{
			if (bytes is null)
				return string.Empty;

			if (tag.ValueTnefType == TnefPropertyType.Unicode)
				return DecodeUnicode (bytes);

			return TnefReader.DecodeString (reader.Encoding, bytes);
		}

		/// <summary>
		/// Read the current value as a string.
		/// </summary>
		/// <remarks>
		/// <para>Reads any string or binary value as a string.</para>
		/// <para><see cref="TnefPropertyType.String8"/> and <see cref="TnefPropertyType.Binary"/> values are decoded using
		/// the <see cref="TnefReader.Codepage"/>.</para>
		/// <para>The value may only be read once.</para>
		/// <para>If the value is larger than <see cref="TnefOptions.MaxPropertyValueLength"/> or the remaining
		/// <see cref="TnefOptions.MaxTotalDataBytes"/> allow, a <see cref="TnefComplianceViolation.DataSizeLimitExceeded"/> issue is
		/// reported and an empty value is returned.</para>
		/// </remarks>
		/// <returns>The value as a string.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="TnefReader"/> has been advanced to another attribute.</para>
		/// <para>-or-</para>
		/// <para>The reader is not positioned on a value.</para>
		/// <para>-or-</para>
		/// <para>The value cannot be read as a string.</para>
		/// <para>-or-</para>
		/// <para>The value has already been read.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public string ReadValueAsString (CancellationToken cancellationToken = default)
		{
			ClaimStringValue ();

			return DecodeString (reader.ReadValueBytes (VariableLength, tag, cancellationToken));
		}

		// Gets the value of a fixed-width property. Returns false for variable-length values.
		bool TryGetFixedValue (out object? value)
		{
			CheckValue ();

			switch (tag.ValueTnefType) {
			case TnefPropertyType.Null: value = null; break;
			case TnefPropertyType.I2: value = GetInt16 (); break;
			case TnefPropertyType.Boolean: value = (GetInt32 () & 0xFFFF) != 0; break;
			// Note: [MS-OXCDATA] defines PtypCurrency as a 64-bit signed integer scaled by 10000.
			case TnefPropertyType.Currency: value = decimal.FromOACurrency (GetInt64 ()); break;
			case TnefPropertyType.I8: value = GetInt64 (); break;
			case TnefPropertyType.Error:
			case TnefPropertyType.Long: value = GetInt32 (); break;
			case TnefPropertyType.Double: value = GetDouble (); break;
			case TnefPropertyType.R4: value = GetSingle (); break;
			case TnefPropertyType.AppTime:
			case TnefPropertyType.SysTime: value = dateValue; break;
			case TnefPropertyType.ClassId: value = new Guid (scratch); break;
			default: value = null; return false;
			}

			return true;
		}

		object DecodeVariableValue (byte[]? bytes)
		{
			switch (tag.ValueTnefType) {
			case TnefPropertyType.Unicode:
			case TnefPropertyType.String8:
				return DecodeString (bytes);
			default:
				return bytes ?? Array.Empty<byte> ();
			}
		}

		/// <summary>
		/// Read the current value.
		/// </summary>
		/// <remarks>
		/// <para>Reads the current value as its native type.</para>
		/// <para>A <see cref="TnefPropertyType.Currency"/> value is returned as a <see cref="decimal"/> that has already
		/// been scaled by 1/10000, since [MS-OXCDATA] defines PtypCurrency as a 64-bit signed integer with 4 digits to
		/// the right of the decimal point.</para>
		/// <para>Variable-length values may only be read once.</para>
		/// <para>If the value is larger than <see cref="TnefOptions.MaxPropertyValueLength"/> or the remaining
		/// <see cref="TnefOptions.MaxTotalDataBytes"/> allow, a <see cref="TnefComplianceViolation.DataSizeLimitExceeded"/> issue is
		/// reported and an empty value is returned.</para>
		/// </remarks>
		/// <returns>The value.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="TnefReader"/> has been advanced to another attribute.</para>
		/// <para>-or-</para>
		/// <para>The reader is not positioned on a value.</para>
		/// <para>-or-</para>
		/// <para>The value has already been read.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public object? ReadValue (CancellationToken cancellationToken = default)
		{
			if (TryGetFixedValue (out var value))
				return value;

			ClaimVariableValue ("a value");

			return DecodeVariableValue (reader.ReadValueBytes (VariableLength, tag, cancellationToken));
		}

		/// <summary>
		/// Open a stream for reading the current value.
		/// </summary>
		/// <remarks>
		/// <para>Opens a stream for reading the raw data of the current string, binary or object value.</para>
		/// <para>The stream is only valid until the reader is advanced to another value.</para>
		/// </remarks>
		/// <returns>The value stream.</returns>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="TnefReader"/> has been advanced to another attribute.</para>
		/// <para>-or-</para>
		/// <para>The reader is not positioned on a value.</para>
		/// <para>-or-</para>
		/// <para>The value is not a variable-length value.</para>
		/// <para>-or-</para>
		/// <para>The value has already been read.</para>
		/// </exception>
		public Stream OpenValueStream ()
		{
			ClaimVariableValue ("a stream");

			return new TnefReaderStream (reader, dataEnd, tag);
		}

		/// <summary>
		/// Open a reader for the embedded TNEF message contained within the current value.
		/// </summary>
		/// <remarks>
		/// <para>Opens a <see cref="TnefReader"/> for the embedded TNEF message contained within the current value
		/// (see <see cref="IsEmbeddedMessage"/>).</para>
		/// <para>The returned reader has a <see cref="TnefReader.Depth"/> one greater than the reader that created it,
		/// and uses the same options and compliance logger. Its <see cref="TnefReader.StreamOffset"/> values are
		/// relative to the start of the outermost TNEF stream.</para>
		/// <para>If the embedded message is nested more deeply than <see cref="TnefOptions.MaxNestingDepth"/> allows,
		/// a <see cref="TnefComplianceViolation.NestingTooDeep"/> issue is reported and the returned reader does not
		/// return any attributes.</para>
		/// <para>The embedded reader is only valid until this reader is advanced to another value. Disposing it does
		/// not affect this reader.</para>
		/// </remarks>
		/// <returns>The embedded message reader.</returns>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="TnefReader"/> has been advanced to another attribute.</para>
		/// <para>-or-</para>
		/// <para>The reader is not positioned on a value.</para>
		/// <para>-or-</para>
		/// <para>The value is not an embedded message.</para>
		/// <para>-or-</para>
		/// <para>The value has already been read.</para>
		/// </exception>
		public TnefReader OpenEmbeddedMessage ()
		{
			CheckValue ();

			if (!IsEmbeddedMessageCore)
				throw new InvalidOperationException ("The value is not an embedded message.");

			ClaimVariableValue ("an embedded message");

			var stream = new TnefReaderStream (reader, dataEnd, tag);

			return reader.CreateEmbeddedReader (stream, dataStart, tag);
		}

		// Checks that the reader is positioned on a property whose values have not been read.
		void CheckUnreadProperty ()
		{
			CheckGeneration ();

			if (!hasProperty)
				throw new InvalidOperationException ("The reader is not positioned on a property.");

			if (valueIndex > 0 || consumed)
				throw new InvalidOperationException ("The property's values have already been read.");
		}

		static Array CreateValueArray (TnefPropertyType type, int length)
		{
			switch (type) {
			case TnefPropertyType.I2: return new short[length];
			case TnefPropertyType.Error:
			case TnefPropertyType.Long: return new int[length];
			case TnefPropertyType.R4: return new float[length];
			case TnefPropertyType.Double: return new double[length];
			case TnefPropertyType.Currency: return new decimal[length];
			case TnefPropertyType.AppTime:
			case TnefPropertyType.SysTime: return new DateTime[length];
			case TnefPropertyType.Boolean: return new bool[length];
			case TnefPropertyType.I8: return new long[length];
			case TnefPropertyType.ClassId: return new Guid[length];
			case TnefPropertyType.String8:
			case TnefPropertyType.Unicode: return new string[length];
			case TnefPropertyType.Binary:
			case TnefPropertyType.Object: return new byte[length][];
			default: return new object?[length];
			}
		}

		// Shrinks the array of values when the property had fewer values than it claimed.
		Array TrimValueArray (Array values, int count)
		{
			if (count == values.Length)
				return values;

			var trimmed = CreateValueArray (tag.ValueTnefType, count);
			Array.Copy (values, trimmed, count);

			return trimmed;
		}

		TnefProperty CreateProperty (int count, object? value)
		{
			return new TnefProperty (tag, name, count, value, reader.Encoding);
		}

		/// <summary>
		/// Read the current property.
		/// </summary>
		/// <remarks>
		/// <para>Reads all of the values of the current property into a <see cref="TnefProperty"/> whose values can be
		/// read any number of times.</para>
		/// <para>This method must be called before any of the property's variable-length values have been read and
		/// before the reader has been advanced to any of its other values.</para>
		/// <para>Binary values are read into memory. Use <see cref="OpenValueStream"/> to read large values, such as
		/// attachment data, without buffering them.</para>
		/// <para>If any value is larger than <see cref="TnefOptions.MaxPropertyValueLength"/> or the remaining
		/// <see cref="TnefOptions.MaxTotalDataBytes"/> allow, a <see cref="TnefComplianceViolation.DataSizeLimitExceeded"/> issue is
		/// reported and the property is returned without any values (its <see cref="TnefProperty.Count"/> is <c>0</c>).</para>
		/// </remarks>
		/// <returns>The property.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <para>The <see cref="TnefReader"/> has been advanced to another attribute.</para>
		/// <para>-or-</para>
		/// <para>The reader is not positioned on a property.</para>
		/// <para>-or-</para>
		/// <para>Some of the property's values have already been read.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public TnefProperty ReadProperty (CancellationToken cancellationToken = default)
		{
			CheckUnreadProperty ();

			return ReadPropertyCore (cancellationToken);
		}

		// Note: Returned by ReadCurrentValue when the value was too large to read into memory.
		static readonly object SkippedValue = new object ();

		// Note: Unless the stream is known to contain enough data, the array of values for a multi-valued property starts
		// out at this size and grows as values are actually read, so that a bogus value count cannot force a huge allocation.
		const int MaxUnverifiedValueCount = 1024;

		object? ReadCurrentValue (CancellationToken cancellationToken)
		{
			if (TryGetFixedValue (out var value))
				return value;

			consumed = true;

			var bytes = reader.ReadValueBytes (VariableLength, tag, cancellationToken);

			return bytes is null ? SkippedValue : DecodeVariableValue (bytes);
		}

		Array CreateMultiValueArray ()
		{
			int width = GetFixedWidth (tag.ValueTnefType);
			int length = valueCount;

			if (length > MaxUnverifiedValueCount && !reader.CanAllocate ((long) length * (width > 0 ? width : 4)))
				length = MaxUnverifiedValueCount;

			return CreateValueArray (tag.ValueTnefType, length);
		}

		Array GrowValueArray (Array values)
		{
			int length = (int) Math.Min ((long) values.Length * 2, valueCount);
			var grown = CreateValueArray (tag.ValueTnefType, length);

			Array.Copy (values, grown, values.Length);

			return grown;
		}

		TnefProperty ReadPropertyCore (CancellationToken cancellationToken)
		{
			object? value;

			if (!tag.IsMultiValued) {
				if (!hasValue || (value = ReadCurrentValue (cancellationToken)) == SkippedValue)
					return CreateProperty (0, null);

				return CreateProperty (1, value);
			}

			var values = CreateMultiValueArray ();
			int count = 0;

			if (hasValue) {
				do {
					if ((value = ReadCurrentValue (cancellationToken)) == SkippedValue)
						return CreateProperty (0, null);

					if (count == values.Length)
						values = GrowValueArray (values);

					values.SetValue (value, count++);
				} while (count < valueCount && AdvanceValue (cancellationToken));
			}

			return CreateProperty (count, TrimValueArray (values, count));
		}

		/// <summary>
		/// Read the remaining properties.
		/// </summary>
		/// <remarks>
		/// <para>Reads all of the properties that follow the current property (or, if the reader has not yet been
		/// positioned on a property, all of the properties) into a <see cref="TnefPropertySet"/>.</para>
		/// <para>For a <see cref="TnefAttributeTag.RecipientTable"/> attribute, only the properties of the current row
		/// are read. Use <see cref="ReadRowsAsPropertySets(CancellationToken)"/> to read every row.</para>
		/// <para>Binary values are read into memory. Use <see cref="OpenValueStream"/> to read large values, such as
		/// attachment data, without buffering them.</para>
		/// <para>If any value is larger than <see cref="TnefOptions.MaxPropertyValueLength"/> or the remaining
		/// <see cref="TnefOptions.MaxTotalDataBytes"/> allow, a <see cref="TnefComplianceViolation.DataSizeLimitExceeded"/> issue is
		/// reported and the property is returned without any values (its <see cref="TnefProperty.Count"/> is <c>0</c>).</para>
		/// </remarks>
		/// <returns>The properties.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="TnefReader"/> has been advanced to another attribute.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public TnefPropertySet ReadPropertySet (CancellationToken cancellationToken = default)
		{
			CheckGeneration ();

			var properties = new TnefPropertySet ();

			while (ReadNextPropertyCore (cancellationToken))
				properties.Add (ReadPropertyCore (cancellationToken));

			return properties;
		}

		/// <summary>
		/// Read the remaining rows of a table.
		/// </summary>
		/// <remarks>
		/// <para>Reads the properties of each of the remaining rows of a <see cref="TnefAttributeTag.RecipientTable"/>
		/// attribute. Any unread properties of the current row are skipped.</para>
		/// <para>For attributes that do not contain a table, an empty list is returned.</para>
		/// </remarks>
		/// <returns>The properties of each row.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefReader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="TnefReader"/> has been advanced to another attribute.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public IReadOnlyList<TnefPropertySet> ReadRowsAsPropertySets (CancellationToken cancellationToken = default)
		{
			var rows = new List<TnefPropertySet> ();

			while (ReadNextRow (cancellationToken))
				rows.Add (ReadPropertySet (cancellationToken));

			return rows;
		}
	}
}

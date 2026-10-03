//
// TnefProperty.cs
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
using System.Text;

namespace MimeKit.Tnef {
	/// <summary>
	/// A MAPI property read from a TNEF stream.
	/// </summary>
	/// <remarks>
	/// <para>A <see cref="TnefProperty"/> is an immutable snapshot of a MAPI property and all of its values. Unlike
	/// the values exposed by a <see cref="TnefPropertyReader"/>, its values may be read any number of times and in
	/// any order, and remain valid after the <see cref="TnefReader"/> has been advanced or disposed.</para>
	/// <para>Properties are read using <see cref="TnefPropertyReader.ReadProperty(System.Threading.CancellationToken)"/>
	/// or, as a group, using <see cref="TnefPropertyReader.ReadPropertySet(System.Threading.CancellationToken)"/>.</para>
	/// <para>Binary values are held in memory as byte arrays. To read large values (such as attachment data) without
	/// buffering them, use <see cref="TnefPropertyReader.OpenValueStream"/> instead.</para>
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\TnefExamples.cs" region="ReadProperties"/>
	/// </example>
	public readonly struct TnefProperty
	{
		readonly Encoding? encoding;

		internal TnefProperty (TnefPropertyTag tag, TnefNameId? name, int count, object? value, Encoding encoding)
		{
			this.encoding = encoding;
			Value = value;
			Count = count;
			Name = name;
			Tag = tag;
		}

		/// <summary>
		/// Get the property tag.
		/// </summary>
		/// <remarks>
		/// <para>Gets the property tag.</para>
		/// <para>For a named property, the property id is only meaningful within the TNEF stream that it was read
		/// from. Use <see cref="Name"/> to identify named properties.</para>
		/// </remarks>
		/// <value>The property tag.</value>
		public TnefPropertyTag Tag { get; }

		/// <summary>
		/// Get the type of the property's values.
		/// </summary>
		/// <remarks>
		/// Gets the type of the property's values, without the <see cref="TnefPropertyType.MultiValued"/> flag.
		/// </remarks>
		/// <value>The property type.</value>
		public TnefPropertyType PropertyType {
			get { return Tag.ValueTnefType; }
		}

		/// <summary>
		/// Get whether the property is multi-valued.
		/// </summary>
		/// <remarks>
		/// Gets whether the property is multi-valued.
		/// </remarks>
		/// <value><see langword="true"/> if the property is multi-valued; otherwise, <see langword="false"/>.</value>
		public bool IsMultiValued {
			get { return Tag.IsMultiValued; }
		}

		/// <summary>
		/// Get the name of the property.
		/// </summary>
		/// <remarks>
		/// Gets the name of the property if it is a named property; otherwise, <see langword="null"/>.
		/// </remarks>
		/// <value>The property name.</value>
		public TnefNameId? Name { get; }

		/// <summary>
		/// Get the number of values.
		/// </summary>
		/// <remarks>
		/// <para>Gets the number of values that were read.</para>
		/// <para>A single-valued property normally has exactly one value, but a malformed or truncated property may
		/// have none, in which case <see cref="Value"/> is <see langword="null"/>.</para>
		/// </remarks>
		/// <value>The number of values.</value>
		public int Count { get; }

		/// <summary>
		/// Get the value of the property.
		/// </summary>
		/// <remarks>
		/// <para>Gets the value of the property as its native type.</para>
		/// <para>The native types are: <see cref="short"/> (<see cref="TnefPropertyType.I2"/>), <see cref="int"/>
		/// (<see cref="TnefPropertyType.Long"/> and <see cref="TnefPropertyType.Error"/>), <see cref="float"/>
		/// (<see cref="TnefPropertyType.R4"/>), <see cref="double"/> (<see cref="TnefPropertyType.Double"/>),
		/// <see cref="decimal"/> (<see cref="TnefPropertyType.Currency"/>, already scaled by 1/10000),
		/// <see cref="DateTime"/> (<see cref="TnefPropertyType.AppTime"/> and <see cref="TnefPropertyType.SysTime"/>),
		/// <see cref="bool"/> (<see cref="TnefPropertyType.Boolean"/>), <see cref="long"/>
		/// (<see cref="TnefPropertyType.I8"/>), <see cref="Guid"/> (<see cref="TnefPropertyType.ClassId"/>),
		/// <see cref="string"/> (<see cref="TnefPropertyType.String8"/> and <see cref="TnefPropertyType.Unicode"/>) and
		/// a <see cref="byte"/> array (<see cref="TnefPropertyType.Binary"/> and <see cref="TnefPropertyType.Object"/>).</para>
		/// <para>An <see cref="TnefPropertyType.Object"/> value includes the 16-byte interface identifier that
		/// prefixes it.</para>
		/// <para>The value of a multi-valued property is an array of the native type, such as a <see cref="string"/>
		/// array for a multi-valued <see cref="TnefPropertyType.Unicode"/> property.</para>
		/// </remarks>
		/// <value>The value.</value>
		public object? Value { get; }

		/// <summary>
		/// Get the values of a multi-valued property.
		/// </summary>
		/// <remarks>
		/// <para>Gets the values of the property as an array of the specified type.</para>
		/// <para>For a multi-valued property, <typeparamref name="T"/> must be the native type of the property's values
		/// (see <see cref="Value"/>). For a single-valued property, the value is returned as a one-element array if it
		/// is of type <typeparamref name="T"/>.</para>
		/// </remarks>
		/// <returns><see langword="true"/> if the values are of type <typeparamref name="T"/>; otherwise, <see langword="false"/>.</returns>
		/// <param name="values">The values.</param>
		/// <typeparam name="T">The type of the values.</typeparam>
		public bool TryGetValues<T> (out T[] values)
		{
			if (Value is T[] array && IsMultiValued) {
				values = array;
				return true;
			}

			if (!IsMultiValued && Value is T value) {
				values = new T[] { value };
				return true;
			}

			values = Array.Empty<T> ();

			return false;
		}

		/// <summary>
		/// Try to get the value as a boolean.
		/// </summary>
		/// <remarks>
		/// Gets the value of a single-valued boolean or integer property as a boolean.
		/// </remarks>
		/// <returns><see langword="true"/> if the value could be converted; otherwise, <see langword="false"/>.</returns>
		/// <param name="value">The value.</param>
		public bool TryGetBoolean (out bool value)
		{
			switch (IsMultiValued ? null : Value) {
			case bool b: value = b; return true;
			case short s: value = s != 0; return true;
			case int i: value = i != 0; return true;
			case long l: value = l != 0; return true;
			case decimal m: value = m != 0; return true;
			default: value = false; return false;
			}
		}

		bool TryGetInt64Core (out long value)
		{
			switch (IsMultiValued ? null : Value) {
			case bool b: value = b ? 1 : 0; return true;
			case short s: value = s; return true;
			case int i: value = i; return true;
			case long l: value = l; return true;
			// Note: A PtypCurrency value always fits in a long once it has been scaled.
			case decimal m: value = (long) m; return true;
			case double d: value = (long) d; return true;
			case float f: value = (long) f; return true;
			default: value = 0; return false;
			}
		}

		/// <summary>
		/// Try to get the value as a 16-bit integer.
		/// </summary>
		/// <remarks>
		/// <para>Gets the value of a single-valued numeric or boolean property as a 16-bit integer.</para>
		/// <para>Values that do not fit are truncated.</para>
		/// </remarks>
		/// <returns><see langword="true"/> if the value could be converted; otherwise, <see langword="false"/>.</returns>
		/// <param name="value">The value.</param>
		public bool TryGetInt16 (out short value)
		{
			bool result = TryGetInt64Core (out long l);

			value = unchecked ((short) l);

			return result;
		}

		/// <summary>
		/// Try to get the value as a 32-bit integer.
		/// </summary>
		/// <remarks>
		/// <para>Gets the value of a single-valued numeric or boolean property as a 32-bit integer.</para>
		/// <para>Values that do not fit are truncated.</para>
		/// </remarks>
		/// <returns><see langword="true"/> if the value could be converted; otherwise, <see langword="false"/>.</returns>
		/// <param name="value">The value.</param>
		public bool TryGetInt32 (out int value)
		{
			bool result = TryGetInt64Core (out long l);

			value = unchecked ((int) l);

			return result;
		}

		/// <summary>
		/// Try to get the value as a 64-bit integer.
		/// </summary>
		/// <remarks>
		/// Gets the value of a single-valued numeric or boolean property as a 64-bit integer.
		/// </remarks>
		/// <returns><see langword="true"/> if the value could be converted; otherwise, <see langword="false"/>.</returns>
		/// <param name="value">The value.</param>
		public bool TryGetInt64 (out long value)
		{
			return TryGetInt64Core (out value);
		}

		/// <summary>
		/// Try to get the value as a double.
		/// </summary>
		/// <remarks>
		/// Gets the value of a single-valued numeric or boolean property as a double.
		/// </remarks>
		/// <returns><see langword="true"/> if the value could be converted; otherwise, <see langword="false"/>.</returns>
		/// <param name="value">The value.</param>
		public bool TryGetDouble (out double value)
		{
			switch (IsMultiValued ? null : Value) {
			case double d: value = d; return true;
			case float f: value = f; return true;
			case decimal m: value = (double) m; return true;
			default:
				bool result = TryGetInt64Core (out long l);
				value = l;
				return result;
			}
		}

		/// <summary>
		/// Try to get the value as a float.
		/// </summary>
		/// <remarks>
		/// Gets the value of a single-valued numeric or boolean property as a float.
		/// </remarks>
		/// <returns><see langword="true"/> if the value could be converted; otherwise, <see langword="false"/>.</returns>
		/// <param name="value">The value.</param>
		public bool TryGetFloat (out float value)
		{
			bool result = TryGetDouble (out double d);

			value = (float) d;

			return result;
		}

		/// <summary>
		/// Try to get the value as a date and time.
		/// </summary>
		/// <remarks>
		/// <para>Gets the value of a single-valued <see cref="TnefPropertyType.AppTime"/> or
		/// <see cref="TnefPropertyType.SysTime"/> property.</para>
		/// <para>Values of type <see cref="TnefPropertyType.SysTime"/> have a <see cref="DateTime.Kind"/> of
		/// <see cref="DateTimeKind.Utc"/>.</para>
		/// </remarks>
		/// <returns><see langword="true"/> if the value is a date and time; otherwise, <see langword="false"/>.</returns>
		/// <param name="value">The value.</param>
		public bool TryGetDateTime (out DateTime value)
		{
			if (!IsMultiValued && Value is DateTime date) {
				value = date;
				return true;
			}

			value = default;

			return false;
		}

		/// <summary>
		/// Try to get the value as a GUID.
		/// </summary>
		/// <remarks>
		/// Gets the value of a single-valued <see cref="TnefPropertyType.ClassId"/> property.
		/// </remarks>
		/// <returns><see langword="true"/> if the value is a GUID; otherwise, <see langword="false"/>.</returns>
		/// <param name="value">The value.</param>
		public bool TryGetGuid (out Guid value)
		{
			if (!IsMultiValued && Value is Guid guid) {
				value = guid;
				return true;
			}

			value = Guid.Empty;

			return false;
		}

		/// <summary>
		/// Try to get the value as a string.
		/// </summary>
		/// <remarks>
		/// <para>Gets the value of a single-valued string or binary property as a string.</para>
		/// <para>Binary values are decoded using the codepage of the TNEF stream at the time that the property
		/// was read.</para>
		/// </remarks>
		/// <returns><see langword="true"/> if the value could be converted; otherwise, <see langword="false"/>.</returns>
		/// <param name="value">The value.</param>
		public bool TryGetString (out string value)
		{
			if (!IsMultiValued) {
				if (Value is string text) {
					value = text;
					return true;
				}

				if (PropertyType == TnefPropertyType.Binary && Value is byte[] bytes) {
					value = TnefReader.DecodeString (encoding ?? TnefReader.DefaultEncoding, bytes);
					return true;
				}
			}

			value = string.Empty;

			return false;
		}

		/// <summary>
		/// Try to get the value as a byte array.
		/// </summary>
		/// <remarks>
		/// <para>Gets the value of a single-valued <see cref="TnefPropertyType.Binary"/>,
		/// <see cref="TnefPropertyType.Object"/> or <see cref="TnefPropertyType.ClassId"/> property as a byte
		/// array.</para>
		/// <para>The returned array is the property's own value, not a copy.</para>
		/// </remarks>
		/// <returns><see langword="true"/> if the value could be converted; otherwise, <see langword="false"/>.</returns>
		/// <param name="value">The value.</param>
		public bool TryGetBytes (out byte[] value)
		{
			switch (IsMultiValued ? null : Value) {
			case byte[] bytes: value = bytes; return true;
			case Guid guid: value = guid.ToByteArray (); return true;
			default: value = Array.Empty<byte> (); return false;
			}
		}

		/// <summary>
		/// Return a string that represents the property.
		/// </summary>
		/// <remarks>
		/// Returns a string that represents the property, intended for debugging.
		/// </remarks>
		/// <returns>A string that represents the property.</returns>
		public override string ToString ()
		{
			if (Name.HasValue)
				return string.Format ("{0} ({1}) = {2}", Name.Value, Tag, Value);

			return string.Format ("{0} = {1}", Tag, Value);
		}
	}
}

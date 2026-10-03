//
// TnefPropertySet.cs
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
using System.Collections;
using System.Collections.Generic;

namespace MimeKit.Tnef {
	/// <summary>
	/// A set of MAPI properties read from a TNEF stream.
	/// </summary>
	/// <remarks>
	/// <para>A <see cref="TnefPropertySet"/> is an immutable snapshot of the MAPI properties contained within a
	/// <see cref="TnefAttributeTag.MapiProperties"/> or <see cref="TnefAttributeTag.Attachment"/> attribute, or within
	/// a single row of a <see cref="TnefAttributeTag.RecipientTable"/> attribute.</para>
	/// <para>Property sets are read using
	/// <see cref="TnefPropertyReader.ReadPropertySet(System.Threading.CancellationToken)"/> and
	/// <see cref="TnefPropertyReader.ReadRowsAsPropertySets(System.Threading.CancellationToken)"/>.</para>
	/// <para>The properties are kept in the order in which they appeared in the TNEF stream.</para>
	/// </remarks>
	public sealed class TnefPropertySet : IReadOnlyList<TnefProperty>
	{
		readonly List<TnefProperty> properties;

		internal TnefPropertySet ()
		{
			properties = new List<TnefProperty> ();
		}

		internal void Add (in TnefProperty property)
		{
			properties.Add (property);
		}

		/// <summary>
		/// Get the number of properties.
		/// </summary>
		/// <remarks>
		/// Gets the number of properties.
		/// </remarks>
		/// <value>The number of properties.</value>
		public int Count {
			get { return properties.Count; }
		}

		/// <summary>
		/// Get the property at the specified index.
		/// </summary>
		/// <remarks>
		/// Gets the property at the specified index.
		/// </remarks>
		/// <value>The property.</value>
		/// <param name="index">The index.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public TnefProperty this[int index] {
			get { return properties[index]; }
		}

		/// <summary>
		/// Try to get the property with the specified tag.
		/// </summary>
		/// <remarks>
		/// <para>Gets the first property with the same property id as <paramref name="tag"/>, regardless of its type.
		/// This allows, for example, <see cref="TnefPropertyTag.SubjectW"/> to match a subject that was written as a
		/// <see cref="TnefPropertyType.String8"/> value.</para>
		/// <para>The ids of named properties are only meaningful within the TNEF stream that they were read from. Use
		/// <see cref="TryGetValue(TnefNameId, out TnefProperty)"/> to look up named properties.</para>
		/// </remarks>
		/// <returns><see langword="true"/> if the property was found; otherwise, <see langword="false"/>.</returns>
		/// <param name="tag">The property tag.</param>
		/// <param name="property">The property, if found.</param>
		public bool TryGetValue (TnefPropertyTag tag, out TnefProperty property)
		{
			for (int i = 0; i < properties.Count; i++) {
				if (properties[i].Tag.Id == tag.Id) {
					property = properties[i];
					return true;
				}
			}

			property = default;

			return false;
		}

		/// <summary>
		/// Try to get the named property with the specified name.
		/// </summary>
		/// <remarks>
		/// Gets the first named property with the specified name.
		/// </remarks>
		/// <returns><see langword="true"/> if the property was found; otherwise, <see langword="false"/>.</returns>
		/// <param name="name">The property name.</param>
		/// <param name="property">The property, if found.</param>
		public bool TryGetValue (TnefNameId name, out TnefProperty property)
		{
			for (int i = 0; i < properties.Count; i++) {
				var id = properties[i].Name;

				if (id.HasValue && id.Value.Equals (name)) {
					property = properties[i];
					return true;
				}
			}

			property = default;

			return false;
		}

		/// <summary>
		/// Get the value of the specified property as a string.
		/// </summary>
		/// <remarks>
		/// Gets the value of the specified property as a string (see <see cref="TnefProperty.TryGetString"/>).
		/// </remarks>
		/// <returns>The value, or <see langword="null"/> if the property was not found or could not be converted.</returns>
		/// <param name="tag">The property tag.</param>
		public string? GetString (TnefPropertyTag tag)
		{
			return TryGetValue (tag, out var property) && property.TryGetString (out var value) ? value : null;
		}

		/// <summary>
		/// Get the value of the specified property as a boolean.
		/// </summary>
		/// <remarks>
		/// Gets the value of the specified property as a boolean (see <see cref="TnefProperty.TryGetBoolean"/>).
		/// </remarks>
		/// <returns>The value, or <see langword="null"/> if the property was not found or could not be converted.</returns>
		/// <param name="tag">The property tag.</param>
		public bool? GetBoolean (TnefPropertyTag tag)
		{
			return TryGetValue (tag, out var property) && property.TryGetBoolean (out var value) ? value : null;
		}

		/// <summary>
		/// Get the value of the specified property as a 32-bit integer.
		/// </summary>
		/// <remarks>
		/// Gets the value of the specified property as a 32-bit integer (see <see cref="TnefProperty.TryGetInt32"/>).
		/// </remarks>
		/// <returns>The value, or <see langword="null"/> if the property was not found or could not be converted.</returns>
		/// <param name="tag">The property tag.</param>
		public int? GetInt32 (TnefPropertyTag tag)
		{
			return TryGetValue (tag, out var property) && property.TryGetInt32 (out var value) ? value : null;
		}

		/// <summary>
		/// Get the value of the specified property as a 64-bit integer.
		/// </summary>
		/// <remarks>
		/// Gets the value of the specified property as a 64-bit integer (see <see cref="TnefProperty.TryGetInt64"/>).
		/// </remarks>
		/// <returns>The value, or <see langword="null"/> if the property was not found or could not be converted.</returns>
		/// <param name="tag">The property tag.</param>
		public long? GetInt64 (TnefPropertyTag tag)
		{
			return TryGetValue (tag, out var property) && property.TryGetInt64 (out var value) ? value : null;
		}

		/// <summary>
		/// Get the value of the specified property as a date and time.
		/// </summary>
		/// <remarks>
		/// Gets the value of the specified property as a date and time (see <see cref="TnefProperty.TryGetDateTime"/>).
		/// </remarks>
		/// <returns>The value, or <see langword="null"/> if the property was not found or could not be converted.</returns>
		/// <param name="tag">The property tag.</param>
		public DateTime? GetDateTime (TnefPropertyTag tag)
		{
			return TryGetValue (tag, out var property) && property.TryGetDateTime (out var value) ? value : null;
		}

		/// <summary>
		/// Get the value of the specified property as a GUID.
		/// </summary>
		/// <remarks>
		/// Gets the value of the specified property as a GUID (see <see cref="TnefProperty.TryGetGuid"/>).
		/// </remarks>
		/// <returns>The value, or <see langword="null"/> if the property was not found or could not be converted.</returns>
		/// <param name="tag">The property tag.</param>
		public Guid? GetGuid (TnefPropertyTag tag)
		{
			return TryGetValue (tag, out var property) && property.TryGetGuid (out var value) ? value : null;
		}

		/// <summary>
		/// Get the value of the specified property as a byte array.
		/// </summary>
		/// <remarks>
		/// Gets the value of the specified property as a byte array (see <see cref="TnefProperty.TryGetBytes"/>).
		/// </remarks>
		/// <returns>The value, or <see langword="null"/> if the property was not found or could not be converted.</returns>
		/// <param name="tag">The property tag.</param>
		public byte[]? GetBytes (TnefPropertyTag tag)
		{
			return TryGetValue (tag, out var property) && property.TryGetBytes (out var value) ? value : null;
		}

		/// <summary>
		/// Get an enumerator for the properties.
		/// </summary>
		/// <remarks>
		/// Gets an enumerator for the properties, in the order in which they appeared in the TNEF stream.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		public IEnumerator<TnefProperty> GetEnumerator ()
		{
			return properties.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
	}
}

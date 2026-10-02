//
// AsyncTnefPropertyReader.cs
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
using System.Threading;
using System.Threading.Tasks;

namespace MimeKit.Tnef {
	public sealed partial class TnefPropertyReader
	{
		async Task<bool> FillAsync (int count, TnefComplianceViolation violation, CancellationToken cancellationToken)
		{
			return CheckAvailable (count, violation) && CheckFilled (await reader.FillAsync (count, cancellationToken).ConfigureAwait (false));
		}

		async Task<bool> PositionNextValueAsync (CancellationToken cancellationToken)
		{
			int width = BeginValue ();

			if (width >= 0) {
				if (!await FillAsync (width, TnefComplianceViolation.InvalidPropertyLength, cancellationToken).ConfigureAwait (false))
					return false;

				LoadFixedValue (width);
				return true;
			}

			if (!await FillAsync (4, TnefComplianceViolation.InvalidPropertyLength, cancellationToken).ConfigureAwait (false))
				return false;

			if (!LoadVariableValue ())
				return false;

			// Note: If the stream ends before the IID, the truncation is reported when the value is read or skipped.
			if (HasObjectIid && await reader.FillAsync (16, cancellationToken).ConfigureAwait (false))
				LoadObjectIid ();

			return true;
		}

		async Task<bool> AdvanceValueAsync (CancellationToken cancellationToken)
		{
			if (!CanAdvanceValue)
				return false;

			long skip = BeginSkipValue ();

			if (skip > 0 && !CheckFilled (await reader.SkipAsync (skip, cancellationToken).ConfigureAwait (false)))
				return false;

			if (!HasNextValue)
				return false;

			return await PositionNextValueAsync (cancellationToken).ConfigureAwait (false);
		}

		async Task<bool> SkipPropertyAsync (CancellationToken cancellationToken)
		{
			while (await AdvanceValueAsync (cancellationToken).ConfigureAwait (false)) {
				// skip over the remaining value(s) of the current property...
			}

			hasProperty = false;

			return !stopped;
		}

		async Task<bool> ReadNextPropertyCoreAsync (CancellationToken cancellationToken)
		{
			if (!await SkipPropertyAsync (cancellationToken).ConfigureAwait (false))
				return false;

			if (NeedsPropertyCount) {
				long countOffset = reader.LocalOffset;

				if (!await FillAsync (4, TnefComplianceViolation.InvalidPropertyCount, cancellationToken).ConfigureAwait (false))
					return false;

				LoadPropertyCount (reader.TakeInt32 (), countOffset);
			}

			if (!BeginProperty (out long offset))
				return false;

			if (!await FillAsync (4, TnefComplianceViolation.InvalidPropertyLength, cancellationToken).ConfigureAwait (false))
				return false;

			LoadPropertyTag ();

			if (tag.IsNamed) {
				if (!await FillAsync (20, TnefComplianceViolation.InvalidPropertyLength, cancellationToken).ConfigureAwait (false))
					return false;

				var kind = LoadNameHeader (out var guid);

				if (kind == TnefNameIdKind.Id) {
					if (!await FillAsync (4, TnefComplianceViolation.InvalidPropertyLength, cancellationToken).ConfigureAwait (false))
						return false;

					name = new TnefNameId (guid, reader.TakeInt32 ());
				} else if (kind == TnefNameIdKind.Name) {
					if (!await FillAsync (4, TnefComplianceViolation.InvalidPropertyLength, cancellationToken).ConfigureAwait (false))
						return false;

					long lengthOffset = reader.LocalOffset;
					int length = reader.TakeInt32 ();

					if (!CheckNameLength (length, lengthOffset))
						return false;

					var bytes = await reader.ReadValueBytesAsync (length, tag, cancellationToken).ConfigureAwait (false);

					if (!CheckFilled (bytes.Length == length) || !CheckFilled (await reader.SkipAsync (GetPadding (length), cancellationToken).ConfigureAwait (false)))
						return false;

					name = new TnefNameId (guid, DecodeUnicode (bytes));
				} else {
					Log (TnefComplianceViolation.InvalidNamedPropertyKind, offset);
					name = new TnefNameId (guid, 0);
				}
			}

			if (!CheckPropertyType (offset))
				return false;

			if (HasValueCount) {
				long countOffset = reader.LocalOffset;

				if (!await FillAsync (4, TnefComplianceViolation.InvalidPropertyLength, cancellationToken).ConfigureAwait (false))
					return false;

				LoadValueCount (reader.TakeInt32 (), countOffset);
			} else {
				valueCount = 1;
			}

			return valueCount == 0 || await PositionNextValueAsync (cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Asynchronously advance to the next row of a table.
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
		public async Task<bool> ReadNextRowAsync (CancellationToken cancellationToken = default)
		{
			CheckGeneration ();

			if (!isTable || stopped)
				return false;

			if (inRow) {
				while (await ReadNextPropertyCoreAsync (cancellationToken).ConfigureAwait (false)) {
					// skip over the remaining properties of the current row...
				}

				if (stopped)
					return false;
			} else if (NeedsRowCount) {
				long countOffset = reader.LocalOffset;

				if (!await FillAsync (4, TnefComplianceViolation.InvalidRowCount, cancellationToken).ConfigureAwait (false))
					return false;

				LoadRowCount (reader.TakeInt32 (), countOffset);
			}

			if (!BeginRow (out long offset))
				return false;

			if (!await FillAsync (4, TnefComplianceViolation.InvalidPropertyCount, cancellationToken).ConfigureAwait (false))
				return false;

			LoadPropertyCount (reader.TakeInt32 (), offset);
			inRow = true;

			return true;
		}

		/// <summary>
		/// Asynchronously advance to the next property.
		/// </summary>
		/// <remarks>
		/// <para>Advances to the next property. Any values of the current property that have not been read are
		/// skipped.</para>
		/// <para>When this method returns <see langword="true"/>, the reader is positioned on the first value of the
		/// property, unless the property has no values (see <see cref="ValueCount"/>).</para>
		/// <para>For a <see cref="TnefAttributeTag.RecipientTable"/> attribute, this method returns the properties of
		/// the current row and returns <see langword="false"/> until <see cref="ReadNextRowAsync(CancellationToken)"/>
		/// has been called.</para>
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
		public Task<bool> ReadNextPropertyAsync (CancellationToken cancellationToken = default)
		{
			CheckGeneration ();

			return ReadNextPropertyCoreAsync (cancellationToken);
		}

		/// <summary>
		/// Asynchronously advance to the next value of the current property.
		/// </summary>
		/// <remarks>
		/// Advances to the next value of the current (multi-valued) property. Any part of the current value
		/// that has not been read is skipped.
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
		public Task<bool> ReadNextValueAsync (CancellationToken cancellationToken = default)
		{
			CheckGeneration ();

			return AdvanceValueAsync (cancellationToken);
		}

		/// <summary>
		/// Asynchronously read the current value as a byte array.
		/// </summary>
		/// <remarks>
		/// <para>Reads any string, binary, object or <see cref="TnefPropertyType.ClassId"/> value as a byte array.</para>
		/// <para>Variable-length values may only be read once.</para>
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
		public Task<byte[]> ReadValueAsBytesAsync (CancellationToken cancellationToken = default)
		{
			if (TryGetFixedBytes (out var bytes))
				return Task.FromResult (bytes);

			ClaimVariableValue ("a byte array");

			return reader.ReadValueBytesAsync (VariableLength, tag, cancellationToken);
		}

		/// <summary>
		/// Asynchronously read the current value as a string.
		/// </summary>
		/// <remarks>
		/// <para>Reads any string or binary value as a string.</para>
		/// <para><see cref="TnefPropertyType.String8"/> and <see cref="TnefPropertyType.Binary"/> values are decoded using
		/// the <see cref="TnefReader.Codepage"/>.</para>
		/// <para>The value may only be read once.</para>
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
		public async Task<string> ReadValueAsStringAsync (CancellationToken cancellationToken = default)
		{
			ClaimStringValue ();

			var bytes = await reader.ReadValueBytesAsync (VariableLength, tag, cancellationToken).ConfigureAwait (false);

			return DecodeString (bytes);
		}

		/// <summary>
		/// Asynchronously read the current value.
		/// </summary>
		/// <remarks>
		/// <para>Reads the current value as its native type.</para>
		/// <para>A <see cref="TnefPropertyType.Currency"/> value is returned as a <see cref="decimal"/> that has already
		/// been scaled by 1/10000.</para>
		/// <para>Variable-length values may only be read once.</para>
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
		public async Task<object?> ReadValueAsync (CancellationToken cancellationToken = default)
		{
			if (TryGetFixedValue (out var value))
				return value;

			ClaimVariableValue ("a value");

			var bytes = await reader.ReadValueBytesAsync (VariableLength, tag, cancellationToken).ConfigureAwait (false);

			return DecodeVariableValue (bytes);
		}
	}
}

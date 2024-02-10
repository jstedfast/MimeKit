//
// HeaderList.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2024 .NET Foundation and Contributors
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
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

using MimeKit.IO;
using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// A list of <see cref="Header"/>s.
	/// </summary>
	/// <remarks>
	/// Represents a list of headers as found in a <see cref="MimeMessage"/>
	/// or <see cref="MimeEntity"/>.
	/// </remarks>
	public sealed class HeaderList : IList<Header>
	{
		internal readonly ParserOptions Options;

		// this table references the first header of each field
		readonly Dictionary<string, Header> table;
		readonly List<Header> headers;

		internal HeaderList (ParserOptions options)
		{
			table = new Dictionary<string, Header> (MimeUtils.OrdinalIgnoreCase);
			headers = new List<Header> ();
			Options = options;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="HeaderList"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new empty header list.
		/// </remarks>
		public HeaderList () : this (ParserOptions.Default.Clone ())
		{
		}

		/// <summary>
		/// Add a header with the specified field and value.
		/// </summary>
		/// <remarks>
		/// Adds a new header for the specified field and value pair.
		/// </remarks>
		/// <param name="id">The header identifier.</param>
		/// <param name="value">The header value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="id"/> is not a valid <see cref="HeaderId"/>.
		/// </exception>
		public void Add (HeaderId id, string value)
		{
			Add (id, Encoding.UTF8, value);
		}

		/// <summary>
		/// Add a header with the specified field and value.
		/// </summary>
		/// <remarks>
		/// Adds a new header for the specified field and value pair.
		/// </remarks>
		/// <param name="field">The name of the header field.</param>
		/// <param name="value">The header value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="field"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The <paramref name="field"/> contains illegal characters.
		/// </exception>
		public void Add (string field, string value)
		{
			Add (field, Encoding.UTF8, value);
		}

		/// <summary>
		/// Add a header with the specified field and value.
		/// </summary>
		/// <remarks>
		/// Adds a new header for the specified field and value pair.
		/// </remarks>
		/// <param name="id">The header identifier.</param>
		/// <param name="encoding">The character encoding to use for the value.</param>
		/// <param name="value">The header value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="encoding"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="id"/> is not a valid <see cref="HeaderId"/>.
		/// </exception>
		public void Add (HeaderId id, Encoding encoding, string value)
		{
			Add (new Header (encoding, id, value));
		}

		/// <summary>
		/// Add a header with the specified field and value.
		/// </summary>
		/// <remarks>
		/// Adds a new header for the specified field and value pair.
		/// </remarks>
		/// <param name="field">The name of the header field.</param>
		/// <param name="encoding">The character encoding to use for the value.</param>
		/// <param name="value">The header value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="field"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="encoding"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The <paramref name="field"/> contains illegal characters.
		/// </exception>
		public void Add (string field, Encoding encoding, string value)
		{
			Add (new Header (encoding, field, value));
		}

		/// <summary>
		/// Check if the <see cref="HeaderList"/> contains a header with the specified field name.
		/// </summary>
		/// <remarks>
		/// Determines whether or not the header list contains the specified header.
		/// </remarks>
		/// <returns><value>true</value> if the requested header exists;
		/// otherwise <value>false</value>.</returns>
		/// <param name="id">The header identifier.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="id"/> is not a valid <see cref="HeaderId"/>.
		/// </exception>
		public bool Contains (HeaderId id)
		{
			if (id == HeaderId.Unknown)
				throw new ArgumentOutOfRangeException (nameof (id));

			return table.ContainsKey (id.ToHeaderName ());
		}

		/// <summary>
		/// Check if the <see cref="HeaderList"/> contains a header with the specified field name.
		/// </summary>
		/// <remarks>
		/// Determines whether or not the header list contains the specified header.
		/// </remarks>
		/// <returns><value>true</value> if the requested header exists;
		/// otherwise <value>false</value>.</returns>
		/// <param name="field">The name of the header field.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="field"/> is <c>null</c>.
		/// </exception>
		public bool Contains (string field)
		{
			if (field is null)
				throw new ArgumentNullException (nameof (field));

			return table.ContainsKey (field);
		}

		/// <summary>
		/// Get the index of the requested header, if it exists.
		/// </summary>
		/// <remarks>
		/// Finds the first index of the specified header, if it exists.
		/// </remarks>
		/// <returns>The index of the requested header; otherwise <value>-1</value>.</returns>
		/// <param name="id">The header id.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="id"/> is not a valid <see cref="HeaderId"/>.
		/// </exception>
		public int IndexOf (HeaderId id)
		{
			if (id == HeaderId.Unknown)
				throw new ArgumentOutOfRangeException (nameof (id));

			for (int i = 0; i < headers.Count; i++) {
				if (headers[i].Id == id)
					return i;
			}

			return -1;
		}

		/// <summary>
		/// Get the index of the requested header, if it exists.
		/// </summary>
		/// <remarks>
		/// Finds the first index of the specified header, if it exists.
		/// </remarks>
		/// <returns>The index of the requested header; otherwise <value>-1</value>.</returns>
		/// <param name="field">The name of the header field.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="field"/> is <c>null</c>.
		/// </exception>
		public int IndexOf (string field)
		{
			if (field is null)
				throw new ArgumentNullException (nameof (field));

			for (int i = 0; i < headers.Count; i++) {
				if (headers[i].Field.Equals (field, StringComparison.OrdinalIgnoreCase))
					return i;
			}

			return -1;
		}

		/// <summary>
		/// Insert a header with the specified field and value at the given index.
		/// </summary>
		/// <remarks>
		/// Inserts the header at the specified index in the list.
		/// </remarks>
		/// <param name="index">The index to insert the header.</param>
		/// <param name="id">The header identifier.</param>
		/// <param name="value">The header value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="id"/> is not a valid <see cref="HeaderId"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is out of range.</para>
		/// </exception>
		public void Insert (int index, HeaderId id, string value)
		{
			Insert (index, id, Encoding.UTF8, value);
		}

		/// <summary>
		/// Insert a header with the specified field and value at the given index.
		/// </summary>
		/// <remarks>
		/// Inserts the header at the specified index in the list.
		/// </remarks>
		/// <param name="index">The index to insert the header.</param>
		/// <param name="field">The name of the header field.</param>
		/// <param name="value">The header value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="field"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The <paramref name="field"/> contains illegal characters.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public void Insert (int index, string field, string value)
		{
			Insert (index, field, Encoding.UTF8, value);
		}

		/// <summary>
		/// Insert a header with the specified field and value at the given index.
		/// </summary>
		/// <remarks>
		/// Inserts the header at the specified index in the list.
		/// </remarks>
		/// <param name="index">The index to insert the header.</param>
		/// <param name="id">The header identifier.</param>
		/// <param name="encoding">The character encoding to use for the value.</param>
		/// <param name="value">The header value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="encoding"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="id"/> is not a valid <see cref="HeaderId"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is out of range.</para>
		/// </exception>
		public void Insert (int index, HeaderId id, Encoding encoding, string value)
		{
			Insert (index, new Header (encoding, id, value));
		}

		/// <summary>
		/// Insert a header with the specified field and value at the given index.
		/// </summary>
		/// <remarks>
		/// Inserts the header at the specified index in the list.
		/// </remarks>
		/// <param name="index">The index to insert the header.</param>
		/// <param name="field">The name of the header field.</param>
		/// <param name="encoding">The character encoding to use for the value.</param>
		/// <param name="value">The header value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="field"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="encoding"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The <paramref name="field"/> contains illegal characters.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public void Insert (int index, string field, Encoding encoding, string value)
		{
			Insert (index, new Header (encoding, field, value));
		}

		/// <summary>
		/// Get the last index of the requested header, if it exists.
		/// </summary>
		/// <remarks>
		/// Finds the last index of the specified header, if it exists.
		/// </remarks>
		/// <returns>The last index of the requested header; otherwise <value>-1</value>.</returns>
		/// <param name="id">The header id.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="id"/> is not a valid <see cref="HeaderId"/>.
		/// </exception>
		public int LastIndexOf (HeaderId id)
		{
			if (id == HeaderId.Unknown)
				throw new ArgumentOutOfRangeException (nameof (id));

			for (int i = headers.Count - 1; i >= 0; i--) {
				if (headers[i].Id == id)
					return i;
			}

			return -1;
		}

		/// <summary>
		/// Get the last index of the requested header, if it exists.
		/// </summary>
		/// <remarks>
		/// Finds the last index of the specified header, if it exists.
		/// </remarks>
		/// <returns>The last index of the requested header; otherwise <value>-1</value>.</returns>
		/// <param name="field">The name of the header field.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="field"/> is <c>null</c>.
		/// </exception>
		public int LastIndexOf (string field)
		{
			if (field is null)
				throw new ArgumentNullException (nameof (field));

			for (int i = headers.Count - 1; i >= 0; i--) {
				if (headers[i].Field.Equals (field, StringComparison.OrdinalIgnoreCase))
					return i;
			}

			return -1;
		}

		/// <summary>
		/// Remove the first occurrence of the specified header field.
		/// </summary>
		/// <remarks>
		/// Removes the first occurrence of the specified header field, if any exist.
		/// </remarks>
		/// <returns><value>true</value> if the first occurrence of the specified
		/// header was removed; otherwise <value>false</value>.</returns>
		/// <param name="id">The header identifier.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="id"/> is is not a valid <see cref="HeaderId"/>.
		/// </exception>
		public bool Remove (HeaderId id)
		{
			if (id == HeaderId.Unknown)
				throw new ArgumentOutOfRangeException (nameof (id));

			if (!table.TryGetValue (id.ToHeaderName (), out var header))
				return false;

			return Remove (header);
		}

		/// <summary>
		/// Remove the first occurrence of the specified header field.
		/// </summary>
		/// <remarks>
		/// Removes the first occurrence of the specified header field, if any exist.
		/// </remarks>
		/// <returns><value>true</value> if the first occurrence of the specified
		/// header was removed; otherwise <value>false</value>.</returns>
		/// <param name="field">The name of the header field.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="field"/> is <c>null</c>.
		/// </exception>
		public bool Remove (string field)
		{
			if (field is null)
				throw new ArgumentNullException (nameof (field));

			if (!table.TryGetValue (field, out var header))
				return false;

			return Remove (header);
		}

		/// <summary>
		/// Remove all of the headers matching the specified field name.
		/// </summary>
		/// <remarks>
		/// Removes all of the headers matching the specified field name.
		/// </remarks>
		/// <param name="id">The header identifier.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="id"/> is not a valid <see cref="HeaderId"/>.
		/// </exception>
		public void RemoveAll (HeaderId id)
		{
			if (id == HeaderId.Unknown)
				throw new ArgumentOutOfRangeException (nameof (id));

			if (!table.Remove (id.ToHeaderName ()))
				return;

			for (int i = headers.Count - 1; i >= 0; i--) {
				if (headers[i].Id != id)
					continue;

				var header = headers[i];
				headers.RemoveAt (i);

				OnChanged (header, HeaderListChangedAction.Removed);
			}
		}

		/// <summary>
		/// Remove all of the headers matching the specified field name.
		/// </summary>
		/// <remarks>
		/// Removes all of the headers matching the specified field name.
		/// </remarks>
		/// <param name="field">The name of the header field.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="field"/> is <c>null</c>.
		/// </exception>
		public void RemoveAll (string field)
		{
			if (field is null)
				throw new ArgumentNullException (nameof (field));

			if (!table.Remove (field))
				return;

			for (int i = headers.Count - 1; i >= 0; i--) {
				if (!headers[i].Field.Equals (field, StringComparison.OrdinalIgnoreCase))
					continue;

				var header = headers[i];
				headers.RemoveAt (i);

				OnChanged (header, HeaderListChangedAction.Removed);
			}
		}

		/// <summary>
		/// Replace all headers with identical field names with the single specified header.
		/// </summary>
		/// <remarks>
		/// <para>Replaces all headers with identical field names with the single specified header.</para>
		/// <para>If no headers with the specified field name exist, it is simply added.</para>
		/// </remarks>
		/// <param name="id">The header identifier.</param>
		/// <param name="encoding">The character encoding to use for the value.</param>
		/// <param name="value">The header value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="encoding"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="id"/> is not a valid <see cref="HeaderId"/>.
		/// </exception>
		public void Replace (HeaderId id, Encoding encoding, string value)
		{
			Replace (new Header (encoding, id, value));
		}

		/// <summary>
		/// Replace all headers with identical field names with the single specified header.
		/// </summary>
		/// <remarks>
		/// <para>Replaces all headers with identical field names with the single specified header.</para>
		/// <para>If no headers with the specified field name exist, it is simply added.</para>
		/// </remarks>
		/// <param name="id">The header identifier.</param>
		/// <param name="value">The header value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="id"/> is not a valid <see cref="HeaderId"/>.
		/// </exception>
		public void Replace (HeaderId id, string value)
		{
			Replace (new Header (id, value));
		}

		/// <summary>
		/// Replace all headers with identical field names with the single specified header.
		/// </summary>
		/// <remarks>
		/// <para>Replaces all headers with identical field names with the single specified header.</para>
		/// <para>If no headers with the specified field name exist, it is simply added.</para>
		/// </remarks>
		/// <param name="field">The name of the header field.</param>
		/// <param name="encoding">The character encoding to use for the value.</param>
		/// <param name="value">The header value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="field"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="encoding"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		public void Replace (string field, Encoding encoding, string value)
		{
			Replace (new Header (encoding, field, value));
		}

		/// <summary>
		/// Replace all headers with identical field names with the single specified header.
		/// </summary>
		/// <remarks>
		/// <para>Replaces all headers with identical field names with the single specified header.</para>
		/// <para>If no headers with the specified field name exist, it is simply added.</para>
		/// </remarks>
		/// <param name="field">The name of the header field.</param>
		/// <param name="value">The header value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="field"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The <paramref name="field"/> contains illegal characters.
		/// </exception>
		public void Replace (string field, string value)
		{
			Replace (new Header (field, value));
		}

		/// <summary>
		/// Get or set the value of the first occurrence of a header
		/// with the specified field name.
		/// </summary>
		/// <remarks>
		/// Gets or sets the value of the first occurrence of a header
		/// with the specified field name.
		/// </remarks>
		/// <value>The value of the first occurrence of the specified header if it exists; otherwise <c>null</c>.</value>
		/// <param name="id">The header identifier.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		public string this [HeaderId id] {
			get {
				if (id == HeaderId.Unknown)
					throw new ArgumentOutOfRangeException (nameof (id));

				if (table.TryGetValue (id.ToHeaderName (), out var header))
					return header.Value;

				return null;
			}
			set {
				if (id == HeaderId.Unknown)
					throw new ArgumentOutOfRangeException (nameof (id));

				if (value is null)
					throw new ArgumentNullException (nameof (value));

				if (table.TryGetValue (id.ToHeaderName (), out var header)) {
					header.Value = value;
				} else {
					Add (id, value);
				}
			}
		}

		/// <summary>
		/// Get or set the value of the first occurrence of a header
		/// with the specified field name.
		/// </summary>
		/// <remarks>
		/// Gets or sets the value of the first occurrence of a header
		/// with the specified field name.
		/// </remarks>
		/// <value>The value of the first occurrence of the specified header if it exists; otherwise <c>null</c>.</value>
		/// <param name="field">The name of the header field.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="field"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		public string this [string field] {
			get {
				if (field is null)
					throw new ArgumentNullException (nameof (field));

				if (table.TryGetValue (field, out var header))
					return header.Value;

				return null;
			}
			set {
				if (field is null)
					throw new ArgumentNullException (nameof (field));

				if (value is null)
					throw new ArgumentNullException (nameof (value));

				if (table.TryGetValue (field, out var header)) {
					header.Value = value;
				} else {
					Add (field, value);
				}
			}
		}

		/// <summary>
		/// Write the <see cref="HeaderList"/> to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Writes all of the headers to the output stream.
		/// </remarks>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The output stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public void WriteTo (FormatOptions options, Stream stream, CancellationToken cancellationToken = default)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (stream is null)
				throw new ArgumentNullException (nameof (stream));

			using (var filtered = new FilteredStream (stream)) {
				filtered.Add (options.CreateNewLineFilter ());

				foreach (var header in headers) {
					filtered.Write (header.RawField, 0, header.RawField.Length, cancellationToken);

					if (!header.IsInvalid) {
						var rawValue = header.GetRawValue (options);

						filtered.Write (Header.Colon, 0, Header.Colon.Length, cancellationToken);
						filtered.Write (rawValue, 0, rawValue.Length, cancellationToken);
					}
				}

				filtered.Flush (cancellationToken);
			}

			if (stream is ICancellableStream cancellable) {
				cancellable.Write (options.NewLineBytes, 0, options.NewLineBytes.Length, cancellationToken);
			} else {
				cancellationToken.ThrowIfCancellationRequested ();
				stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
			}
		}

		/// <summary>
		/// Asynchronously write the <see cref="HeaderList"/> to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Writes all of the headers to the output stream.
		/// </remarks>
		/// <returns>A task that represents the asynchronous write operation.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The output stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public async Task WriteToAsync (FormatOptions options, Stream stream, CancellationToken cancellationToken = default)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (stream is null)
				throw new ArgumentNullException (nameof (stream));

			using (var filtered = new FilteredStream (stream)) {
				filtered.Add (options.CreateNewLineFilter ());

				foreach (var header in headers) {
					await filtered.WriteAsync (header.RawField, 0, header.RawField.Length, cancellationToken).ConfigureAwait (false);

					if (!header.IsInvalid) {
						var rawValue = header.GetRawValue (options);

						await filtered.WriteAsync (Header.Colon, 0, Header.Colon.Length, cancellationToken).ConfigureAwait (false);
						await filtered.WriteAsync (rawValue, 0, rawValue.Length, cancellationToken).ConfigureAwait (false);
					}
				}

				await filtered.FlushAsync (cancellationToken).ConfigureAwait (false);
			}

			await stream.WriteAsync (options.NewLineBytes, 0, options.NewLineBytes.Length, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Write the <see cref="HeaderList"/> to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Writes all of the headers to the output stream.
		/// </remarks>
		/// <param name="stream">The output stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public void WriteTo (Stream stream, CancellationToken cancellationToken = default)
		{
			WriteTo (FormatOptions.Default, stream, cancellationToken);
		}

		/// <summary>
		/// Asynchronously write the <see cref="HeaderList"/> to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Writes all of the headers to the output stream.
		/// </remarks>
		/// <returns>A task that represents the asynchronous write operation.</returns>
		/// <param name="stream">The output stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public Task WriteToAsync (Stream stream, CancellationToken cancellationToken = default)
		{
			return WriteToAsync (FormatOptions.Default, stream, cancellationToken);
		}

		#region ICollection implementation

		/// <summary>
		/// Get the number of headers in the list.
		/// </summary>
		/// <remarks>
		/// Gets the number of headers in the list.
		/// </remarks>
		/// <value>The number of headers.</value>
		public int Count {
			get { return headers.Count; }
		}

		/// <summary>
		/// Get whether or not the header list is read only.
		/// </summary>
		/// <remarks>
		/// A <see cref="HeaderList"/> is never read-only.
		/// </remarks>
		/// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
		public bool IsReadOnly {
			get { return false; }
		}

		/// <summary>
		/// Add the specified header.
		/// </summary>
		/// <remarks>
		/// Adds the specified header to the end of the header list.
		/// </remarks>
		/// <param name="header">The header to add.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="header"/> is <c>null</c>.
		/// </exception>
		public void Add (Header header)
		{
			if (header is null)
				throw new ArgumentNullException (nameof (header));

			if (!table.ContainsKey (header.Field))
				table.Add (header.Field, header);

			header.Changed += HeaderChanged;
			headers.Add (header);

			OnChanged (header, HeaderListChangedAction.Added);
		}

		/// <summary>
		/// Clear the header list.
		/// </summary>
		/// <remarks>
		/// Removes all of the headers from the list.
		/// </remarks>
		public void Clear ()
		{
			foreach (var header in headers)
				header.Changed -= HeaderChanged;

			headers.Clear ();
			table.Clear ();

			OnChanged (null, HeaderListChangedAction.Cleared);
		}

		/// <summary>
		/// Check if the <see cref="HeaderList"/> contains the specified header.
		/// </summary>
		/// <remarks>
		/// Determines whether or not the header list contains the specified header.
		/// </remarks>
		/// <returns><value>true</value> if the specified header is contained;
		/// otherwise, <value>false</value>.</returns>
		/// <param name="header">The header.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="header"/> is <c>null</c>.
		/// </exception>
		public bool Contains (Header header)
		{
			if (header is null)
				throw new ArgumentNullException (nameof (header));

			return headers.Contains (header);
		}

		/// <summary>
		/// Copy all of the headers in the <see cref="HeaderList"/> to the specified array.
		/// </summary>
		/// <remarks>
		/// Copies all of the headers within the <see cref="HeaderList"/> into the array,
		/// starting at the specified array index.
		/// </remarks>
		/// <param name="array">The array to copy the headers to.</param>
		/// <param name="arrayIndex">The index into the array.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="array"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex"/> is out of range.
		/// </exception>
		public void CopyTo (Header[] array, int arrayIndex)
		{
			headers.CopyTo (array, arrayIndex);
		}

		/// <summary>
		/// Remove the specified header.
		/// </summary>
		/// <remarks>
		/// Removes the specified header from the list if it exists.
		/// </remarks>
		/// <returns><c>true</c> if the specified header was removed;
		/// otherwise <c>false</c>.</returns>
		/// <param name="header">The header.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="header"/> is <c>null</c>.
		/// </exception>
		public bool Remove (Header header)
		{
			if (header is null)
				throw new ArgumentNullException (nameof (header));

			int index = headers.IndexOf (header);

			if (index == -1)
				return false;

			header.Changed -= HeaderChanged;

			if (table[header.Field] == header) {
				table.Remove (header.Field);

				// find the next matching header and add it to the lookup table
				for (int i = index + 1; i < headers.Count; i++) {
					if (headers[i].Field.Equals (header.Field, StringComparison.OrdinalIgnoreCase)) {
						table.Add (headers[i].Field, headers[i]);
						break;
					}
				}
			}

			headers.RemoveAt (index);

			OnChanged (header, HeaderListChangedAction.Removed);

			return true;
		}

		/// <summary>
		/// Replace all headers with identical field names with the single specified header.
		/// </summary>
		/// <remarks>
		/// <para>Replaces all headers with identical field names with the single specified header.</para>
		/// <para>If no headers with the specified field name exist, it is simply added.</para>
		/// </remarks>
		/// <param name="header">The header.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="header"/> is <c>null</c>.
		/// </exception>
		public void Replace (Header header)
		{
			int i;

			if (header is null)
				throw new ArgumentNullException (nameof (header));

			if (!table.TryGetValue (header.Field, out var first)) {
				Add (header);
				return;
			}

			for (i = headers.Count - 1; i >= 0; i--) {
				if (headers[i] == first)
					break;

				if (!headers[i].Field.Equals (header.Field, StringComparison.OrdinalIgnoreCase))
					continue;

				headers[i].Changed -= HeaderChanged;
				headers.RemoveAt (i);
			}

			header.Changed += HeaderChanged;
			first.Changed -= HeaderChanged;

			table[header.Field] = header;
			headers[i] = header;

			OnChanged (first, HeaderListChangedAction.Removed);
			OnChanged (header, HeaderListChangedAction.Added);
		}

		#endregion

		#region IList implementation

		/// <summary>
		/// Get the index of the requested header, if it exists.
		/// </summary>
		/// <remarks>
		/// Finds the index of the specified header, if it exists.
		/// </remarks>
		/// <returns>The index of the requested header; otherwise <value>-1</value>.</returns>
		/// <param name="header">The header.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="header"/> is <c>null</c>.
		/// </exception>
		public int IndexOf (Header header)
		{
			if (header is null)
				throw new ArgumentNullException (nameof (header));

			return headers.IndexOf (header);
		}

		/// <summary>
		/// Insert the specified header at the given index.
		/// </summary>
		/// <remarks>
		/// Inserts the header at the specified index in the list.
		/// </remarks>
		/// <param name="index">The index to insert the header.</param>
		/// <param name="header">The header.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="header"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public void Insert (int index, Header header)
		{
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException (nameof (index));

			if (header is null)
				throw new ArgumentNullException (nameof (header));

			// update the lookup table
			if (table.TryGetValue (header.Field, out var hdr)) {
				int idx = headers.IndexOf (hdr);

				if (idx >= index)
					table[header.Field] = header;
			} else {
				table.Add (header.Field, header);
			}

			headers.Insert (index, header);
			header.Changed += HeaderChanged;

			OnChanged (header, HeaderListChangedAction.Added);
		}

		/// <summary>
		/// Remove the header at the specified index.
		/// </summary>
		/// <remarks>
		/// Removes the header at the specified index.
		/// </remarks>
		/// <param name="index">The index.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public void RemoveAt (int index)
		{
			if (index < 0 || index >= Count)
				throw new ArgumentOutOfRangeException (nameof (index));

			var header = headers[index];

			header.Changed -= HeaderChanged;

			if (table[header.Field] == header) {
				table.Remove (header.Field);

				// find the next matching header and add it to the lookup table
				for (int i = index + 1; i < headers.Count; i++) {
					if (headers[i].Field.Equals (header.Field, StringComparison.OrdinalIgnoreCase)) {
						table.Add (headers[i].Field, headers[i]);
						break;
					}
				}
			}

			headers.RemoveAt (index);

			OnChanged (header, HeaderListChangedAction.Removed);
		}

		/// <summary>
		/// Get or set the <see cref="Header"/> at the specified index.
		/// </summary>
		/// <remarks>
		/// Gets or sets the <see cref="Header"/> at the specified index.
		/// </remarks>
		/// <value>The header at the specified index.</value>
		/// <param name="index">The index.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public Header this [int index] {
			get {
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException (nameof (index));

				return headers[index];
			}
			set {
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException (nameof (index));

				if (value is null)
					throw new ArgumentNullException (nameof (value));

				var header = headers[index];

				if (header == value)
					return;

				header.Changed -= HeaderChanged;
				value.Changed += HeaderChanged;

				if (header.Field.Equals (value.Field, StringComparison.OrdinalIgnoreCase)) {
					// replace the old header with the new one
					if (table[header.Field] == header)
						table[header.Field] = value;
				} else {
					// update the table for the header field being replaced
					if (table[header.Field] == header) {
						table.Remove (header.Field);

						// find the next matching header and add it to the lookup table
						for (int i = index + 1; i < headers.Count; i++) {
							if (headers[i].Field.Equals (header.Field, StringComparison.OrdinalIgnoreCase)) {
								table.Add (headers[i].Field, headers[i]);
								break;
							}
						}
					}

					// update the table for the header being set
					if (table.TryGetValue (value.Field, out header)) {
						int idx = headers.IndexOf (header);

						if (idx > index)
							table[header.Field] = value;
					} else {
						table.Add (value.Field, value);
					}
				}

				headers[index] = value;

				if (header.Field.Equals (value.Field, StringComparison.OrdinalIgnoreCase)) {
					OnChanged (value, HeaderListChangedAction.Changed);
				} else {
					OnChanged (header, HeaderListChangedAction.Removed);
					OnChanged (value, HeaderListChangedAction.Added);
				}
			}
		}

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Get an enumerator for the list of headers.
		/// </summary>
		/// <remarks>
		/// Gets an enumerator for the list of headers.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		public IEnumerator<Header> GetEnumerator ()
		{
			return headers.GetEnumerator ();
		}

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Get an enumerator for the list of headers.
		/// </summary>
		/// <remarks>
		/// Gets an enumerator for the list of headers.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return headers.GetEnumerator ();
		}

		#endregion

		internal event EventHandler<HeaderListChangedEventArgs> Changed;

		void HeaderChanged (object sender, EventArgs args)
		{
			OnChanged ((Header) sender, HeaderListChangedAction.Changed);
		}

		void OnChanged (Header header, HeaderListChangedAction action)
		{
			Changed?.Invoke (this, new HeaderListChangedEventArgs (header, action));
		}

		internal bool TryGetHeader (HeaderId id, out Header header)
		{
			return table.TryGetValue (id.ToHeaderName (), out header);
		}

		internal bool TryGetHeader (string field, out Header header)
		{
			if (field is null)
				throw new ArgumentNullException (nameof (field));

			return table.TryGetValue (field, out header);
		}

		/// <summary>
		/// Load a <see cref="HeaderList"/> from the specified stream.
		/// </summary>
		/// <remarks>
		/// Loads a <see cref="HeaderList"/> from the given stream, using the
		/// specified <see cref="ParserOptions"/>.
		/// </remarks>
		/// <returns>The parsed list of headers.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="stream">The stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the headers.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static HeaderList Load (ParserOptions options, Stream stream, CancellationToken cancellationToken = default)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (stream is null)
				throw new ArgumentNullException (nameof (stream));

			var parser = new MimeParser (options, stream, MimeFormat.Entity);

			return parser.ParseHeaders (cancellationToken);
		}

		/// <summary>
		/// Asynchronously load a <see cref="HeaderList"/> from the specified stream.
		/// </summary>
		/// <remarks>
		/// Loads a <see cref="HeaderList"/> from the given stream, using the
		/// specified <see cref="ParserOptions"/>.
		/// </remarks>
		/// <returns>The parsed list of headers.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="stream">The stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the headers.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static Task<HeaderList> LoadAsync (ParserOptions options, Stream stream, CancellationToken cancellationToken = default)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (stream is null)
				throw new ArgumentNullException (nameof (stream));

			var parser = new MimeParser (options, stream, MimeFormat.Entity);

			return parser.ParseHeadersAsync (cancellationToken);
		}

		/// <summary>
		/// Load a <see cref="HeaderList"/> from the specified stream.
		/// </summary>
		/// <remarks>
		/// Loads a <see cref="HeaderList"/> from the given stream, using the
		/// default <see cref="ParserOptions"/>.
		/// </remarks>
		/// <returns>The parsed list of headers.</returns>
		/// <param name="stream">The stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the entity.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static HeaderList Load (Stream stream, CancellationToken cancellationToken = default)
		{
			return Load (ParserOptions.Default, stream, cancellationToken);
		}

		/// <summary>
		/// Asynchronously load a <see cref="HeaderList"/> from the specified stream.
		/// </summary>
		/// <remarks>
		/// Loads a <see cref="HeaderList"/> from the given stream, using the
		/// default <see cref="ParserOptions"/>.
		/// </remarks>
		/// <returns>The parsed list of headers.</returns>
		/// <param name="stream">The stream.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the entity.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static Task<HeaderList> LoadAsync (Stream stream, CancellationToken cancellationToken = default)
		{
			return LoadAsync (ParserOptions.Default, stream, cancellationToken);
		}

		/// <summary>
		/// Load a <see cref="HeaderList"/> from the specified file.
		/// </summary>
		/// <remarks>
		/// Loads a <see cref="HeaderList"/> from the file at the give file path,
		/// using the specified <see cref="ParserOptions"/>.
		/// </remarks>
		/// <returns>The parsed list of headers.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="fileName">The name of the file to load.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.
		/// </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// <paramref name="fileName"/> is an invalid file path.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file path could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the headers.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static HeaderList Load (ParserOptions options, string fileName, CancellationToken cancellationToken = default)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (fileName is null)
				throw new ArgumentNullException (nameof (fileName));

			using (var stream = File.OpenRead (fileName))
				return Load (options, stream, cancellationToken);
		}

		/// <summary>
		/// Asynchronously load a <see cref="HeaderList"/> from the specified file.
		/// </summary>
		/// <remarks>
		/// Loads a <see cref="HeaderList"/> from the file at the give file path,
		/// using the specified <see cref="ParserOptions"/>.
		/// </remarks>
		/// <returns>The parsed list of headers.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="fileName">The name of the file to load.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.
		/// </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// <paramref name="fileName"/> is an invalid file path.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file path could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the headers.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static async Task<HeaderList> LoadAsync (ParserOptions options, string fileName, CancellationToken cancellationToken = default)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (fileName is null)
				throw new ArgumentNullException (nameof (fileName));

			using (var stream = File.OpenRead (fileName))
				return await LoadAsync (options, stream, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Load a <see cref="HeaderList"/> from the specified file.
		/// </summary>
		/// <remarks>
		/// Loads a <see cref="HeaderList"/> from the file at the give file path,
		/// using the default <see cref="ParserOptions"/>.
		/// </remarks>
		/// <returns>The parsed list of headers.</returns>
		/// <param name="fileName">The name of the file to load.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.
		/// </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// <paramref name="fileName"/> is an invalid file path.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file path could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the headers.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static HeaderList Load (string fileName, CancellationToken cancellationToken = default)
		{
			return Load (ParserOptions.Default, fileName, cancellationToken);
		}

		/// <summary>
		/// Asynchronously load a <see cref="HeaderList"/> from the specified file.
		/// </summary>
		/// <remarks>
		/// Loads a <see cref="HeaderList"/> from the file at the give file path,
		/// using the default <see cref="ParserOptions"/>.
		/// </remarks>
		/// <returns>The parsed list of headers.</returns>
		/// <param name="fileName">The name of the file to load.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.
		/// </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// <paramref name="fileName"/> is an invalid file path.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file path could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the headers.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static Task<HeaderList> LoadAsync (string fileName, CancellationToken cancellationToken = default)
		{
			return LoadAsync (ParserOptions.Default, fileName, cancellationToken);
		}
	}
}

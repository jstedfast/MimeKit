//
// HeaderList.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc.
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
using System.Collections.Generic;

#if PORTABLE
using Encoding = Portable.Text.Encoding;
#endif

using MimeKit.IO;

namespace MimeKit {
	/// <summary>
	/// A list of <see cref="MimeKit.Header"/>s.
	/// </summary>
	/// <remarks>
	/// Represents a list of headers as found in a <see cref="MimeMessage"/>
	/// or <see cref="MimeEntity"/>.
	/// </remarks>
	public sealed class HeaderList : IList<Header>
	{
		static readonly StringComparer icase = StringComparer.OrdinalIgnoreCase;

		// this table references the first header of each field
		internal readonly ParserOptions Options;
		readonly Dictionary<string, Header> table;
		readonly List<Header> headers;

		internal HeaderList (ParserOptions options)
		{
			table = new Dictionary<string, Header> (icase);
			headers = new List<Header> ();
			Options = options;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.HeaderList"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new empty header list.
		/// </remarks>
		public HeaderList () : this (ParserOptions.Default.Clone ())
		{
		}

		/// <summary>
		/// Adds a header with the specified field and value.
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
			Add (new Header (id, value));
		}

		/// <summary>
		/// Adds a header with the specified field and value.
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
			Add (new Header (field, value));
		}

		/// <summary>
		/// Checks if the <see cref="MimeKit.HeaderList"/> contains a header with the specified field name.
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
				throw new ArgumentOutOfRangeException ("id");

			return table.ContainsKey (id.ToHeaderName ());
		}

		/// <summary>
		/// Checks if the <see cref="MimeKit.HeaderList"/> contains a header with the specified field name.
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
			if (field == null)
				throw new ArgumentNullException ("field");

			return table.ContainsKey (field);
		}

		/// <summary>
		/// Gets the index of the requested header, if it exists.
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
				throw new ArgumentOutOfRangeException ("id");

			for (int i = 0; i < headers.Count; i++) {
				if (headers[i].Id == id)
					return i;
			}

			return -1;
		}

		/// <summary>
		/// Gets the index of the requested header, if it exists.
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
			if (field == null)
				throw new ArgumentNullException ("field");

			for (int i = 0; i < headers.Count; i++) {
				if (headers[i].Field == field)
					return i;
			}

			return -1;
		}

		/// <summary>
		/// Inserts a header with the specified field and value at the given index.
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
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException ("index");

			Insert (index, new Header (id, value));
		}

		/// <summary>
		/// Inserts a header with the specified field and value at the given index.
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
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException ("index");

			Insert (index, new Header (field, value));
		}

		/// <summary>
		/// Removes the first occurance of the specified header field.
		/// </summary>
		/// <remarks>
		/// Removes the first occurance of the specified header field, if any exist.
		/// </remarks>
		/// <returns><value>true</value> if the first occurance of the specified
		/// header was removed; otherwise <value>false</value>.</returns>
		/// <param name="id">The header identifier.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="id"/> is is not a valid <see cref="HeaderId"/>.
		/// </exception>
		public bool Remove (HeaderId id)
		{
			if (id == HeaderId.Unknown)
				throw new ArgumentNullException ("id");

			Header header;
			if (!table.TryGetValue (id.ToHeaderName (), out header))
				return false;

			return Remove (header);
		}

		/// <summary>
		/// Removes the first occurance of the specified header field.
		/// </summary>
		/// <remarks>
		/// Removes the first occurance of the specified header field, if any exist.
		/// </remarks>
		/// <returns><value>true</value> if the first occurance of the specified
		/// header was removed; otherwise <value>false</value>.</returns>
		/// <param name="field">The name of the header field.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="field"/> is <c>null</c>.
		/// </exception>
		public bool Remove (string field)
		{
			if (field == null)
				throw new ArgumentNullException ("field");

			Header header;
			if (!table.TryGetValue (field, out header))
				return false;

			return Remove (header);
		}

		/// <summary>
		/// Removes all of the headers matching the specified field name.
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
				throw new ArgumentOutOfRangeException ("id");

			table.Remove (id.ToHeaderName ());

			for (int i = headers.Count - 1; i >= 0; i--) {
				if (headers[i].Id != id)
					continue;

				var header = headers[i];
				headers.RemoveAt (i);

				OnChanged (header, HeaderListChangedAction.Removed);
			}
		}

		/// <summary>
		/// Removes all of the headers matching the specified field name.
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
			if (field == null)
				throw new ArgumentNullException ("field");

			table.Remove (field);

			for (int i = headers.Count - 1; i >= 0; i--) {
				if (icase.Compare (headers[i].Field, field) != 0)
					continue;

				var header = headers[i];
				headers.RemoveAt (i);

				OnChanged (header, HeaderListChangedAction.Removed);
			}
		}

		/// <summary>
		/// Replaces all headers with identical field names with the single specified header.
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
			if (id == HeaderId.Unknown)
				throw new ArgumentOutOfRangeException ("id");

			if (encoding == null)
				throw new ArgumentNullException ("encoding");

			if (value == null)
				throw new ArgumentNullException ("value");

			Replace (new Header (encoding, id, value));
		}

		/// <summary>
		/// Replaces all headers with identical field names with the single specified header.
		/// </summary>
		/// <remarks>
		/// <para>Replaces all headers with identical field names with the single specified header.</para>
		/// <para>If no headers with the specified field name exist, it is simply added.</para>
		/// </remarks>
		/// <param name="id">The header identifier.</param>
		/// <param name="charset">The charset to use for the value.</param>
		/// <param name="value">The header value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="charset"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="id"/> is not a valid <see cref="HeaderId"/>.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <paramref name="charset"/> is not supported.
		/// </exception>
		public void Replace (HeaderId id, string charset, string value)
		{
			if (id == HeaderId.Unknown)
				throw new ArgumentOutOfRangeException ("id");

			if (charset == null)
				throw new ArgumentNullException ("charset");

			if (value == null)
				throw new ArgumentNullException ("value");

			Replace (new Header (charset, id, value));
		}

		/// <summary>
		/// Replaces all headers with identical field names with the single specified header.
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
			if (id == HeaderId.Unknown)
				throw new ArgumentOutOfRangeException ("id");

			if (value == null)
				throw new ArgumentNullException ("value");

			Replace (new Header (id, value));
		}

		/// <summary>
		/// Replaces all headers with identical field names with the single specified header.
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
			if (field == null)
				throw new ArgumentNullException ("field");

			if (encoding == null)
				throw new ArgumentNullException ("encoding");

			if (value == null)
				throw new ArgumentNullException ("value");

			Replace (new Header (encoding, field, value));
		}

		/// <summary>
		/// Replaces all headers with identical field names with the single specified header.
		/// </summary>
		/// <remarks>
		/// <para>Replaces all headers with identical field names with the single specified header.</para>
		/// <para>If no headers with the specified field name exist, it is simply added.</para>
		/// </remarks>
		/// <param name="field">The name of the header field.</param>
		/// <param name="charset">The charset to use for the value.</param>
		/// <param name="value">The header value.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="field"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="charset"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The <paramref name="field"/> contains illegal characters.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <paramref name="charset"/> is not supported.
		/// </exception>
		public void Replace (string field, string charset, string value)
		{
			if (field == null)
				throw new ArgumentNullException ("field");

			if (charset == null)
				throw new ArgumentNullException ("charset");

			if (value == null)
				throw new ArgumentNullException ("value");

			Replace (new Header (charset, field, value));
		}

		/// <summary>
		/// Replaces all headers with identical field names with the single specified header.
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
			if (field == null)
				throw new ArgumentNullException ("field");

			if (value == null)
				throw new ArgumentNullException ("value");

			Replace (new Header (field, value));
		}

		/// <summary>
		/// Gets or sets the value of the first occurance of a header
		/// with the specified field name.
		/// </summary>
		/// <remarks>
		/// Gets or sets the value of the first occurance of a header
		/// with the specified field name.
		/// </remarks>
		/// <value>The value of the first occurrance of the specified header if it exists; otherwise <c>null</c>.</value>
		/// <param name="id">The header identifier.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		public string this [HeaderId id] {
			get {
				if (id == HeaderId.Unknown)
					throw new ArgumentOutOfRangeException ("id");

				Header header;
				if (table.TryGetValue (id.ToHeaderName (), out header))
					return header.Value;

				return null;
			}
			set {
				if (id == HeaderId.Unknown)
					throw new ArgumentOutOfRangeException ("id");

				if (value == null)
					throw new ArgumentNullException ("value");

				Header header;
				if (table.TryGetValue (id.ToHeaderName (), out header)) {
					header.Value = value;
				} else {
					Add (id, value);
				}
			}
		}

		/// <summary>
		/// Gets or sets the value of the first occurance of a header
		/// with the specified field name.
		/// </summary>
		/// <remarks>
		/// Gets or sets the value of the first occurance of a header
		/// with the specified field name.
		/// </remarks>
		/// <value>The value of the first occurrance of the specified header if it exists; otherwise <c>null</c>.</value>
		/// <param name="field">The name of the header field.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="field"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		public string this [string field] {
			get {
				if (field == null)
					throw new ArgumentNullException ("field");

				Header header;
				if (table.TryGetValue (field, out header))
					return header.Value;

				return null;
			}
			set {
				if (field == null)
					throw new ArgumentNullException ("field");

				if (value == null)
					throw new ArgumentNullException ("value");

				Header header;
				if (table.TryGetValue (field, out header)) {
					header.Value = value;
				} else {
					Add (field, value);
				}
			}
		}

		/// <summary>
		/// Writes the <see cref="MimeKit.HeaderList"/> to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Writes all of the headers to the output stream.
		/// </remarks>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The output stream.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
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
		public void WriteTo (FormatOptions options, Stream stream, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (stream == null)
				throw new ArgumentNullException ("stream");

			using (var filtered = new FilteredStream (stream)) {
				filtered.Add (options.CreateNewLineFilter ());

				foreach (var header in headers) {
					byte[] rawValue;

					filtered.Write (header.RawField, 0, header.RawField.Length, cancellationToken);
					filtered.Write (new [] { (byte) ':' }, 0, 1, cancellationToken);

					if (options.International)
						rawValue = header.GetRawValue (options, Encoding.UTF8);
					else
						rawValue = header.RawValue;

					filtered.Write (rawValue, 0, rawValue.Length, cancellationToken);
				}

				filtered.Flush (cancellationToken);
			}
		}

		/// <summary>
		/// Writes the <see cref="MimeKit.HeaderList"/> to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Writes all of the headers to the output stream.
		/// </remarks>
		/// <param name="stream">The output stream.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public void WriteTo (Stream stream, CancellationToken cancellationToken = default (CancellationToken))
		{
			WriteTo (FormatOptions.GetDefault (), stream, cancellationToken);
		}

		#region ICollection implementation

		/// <summary>
		/// Gets the number of headers in the list.
		/// </summary>
		/// <remarks>
		/// Gets the number of headers in the list.
		/// </remarks>
		/// <value>The number of headers.</value>
		public int Count {
			get { return headers.Count; }
		}

		/// <summary>
		/// Gets whether or not the header list is read only.
		/// </summary>
		/// <remarks>
		/// A <see cref="HeaderList"/> is never read-only.
		/// </remarks>
		/// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
		public bool IsReadOnly {
			get { return false; }
		}

		/// <summary>
		/// Adds the specified header.
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
			if (header == null)
				throw new ArgumentNullException ("header");

			if (!table.ContainsKey (header.Field))
				table.Add (header.Field, header);

			header.Changed += HeaderChanged;
			headers.Add (header);

			OnChanged (header, HeaderListChangedAction.Added);
		}

		/// <summary>
		/// Clears the header list.
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
		/// Checks if the <see cref="HeaderList"/> contains the specified header.
		/// </summary>
		/// <remarks>
		/// Determines whether or not the header list contains the specified header.
		/// </remarks>
		/// <returns><value>true</value> if the specified header is contained;
		/// otherwise <value>false</value>.</returns>
		/// <param name="header">The header.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="header"/> is <c>null</c>.
		/// </exception>
		public bool Contains (Header header)
		{
			if (header == null)
				throw new ArgumentNullException ("header");

			return headers.Contains (header);
		}

		/// <summary>
		/// Copies all of the headers in the <see cref="MimeKit.HeaderList"/> to the specified array.
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
		/// Removes the specified header.
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
			if (header == null)
				throw new ArgumentNullException ("header");

			int index = headers.IndexOf (header);

			if (index == -1)
				return false;

			header.Changed -= HeaderChanged;

			if (table[header.Field] == header) {
				table.Remove (header.Field);

				// find the next matching header and add it to the lookup table
				for (int i = index + 1; i < headers.Count; i++) {
					if (icase.Compare (headers[i].Field, header.Field) == 0) {
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
		/// Replaces all headers with identical field names with the single specified header.
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

			if (header == null)
				throw new ArgumentNullException ("header");

			Header first;
			if (!table.TryGetValue (header.Field, out first)) {
				Add (header);
				return;
			}

			for (i = headers.Count - 1; i >= 0; i--) {
				if (headers[i] == first)
					break;

				if (icase.Compare (headers[i].Field, header.Field) != 0)
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
		/// Gets the index of the requested header, if it exists.
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
			if (header == null)
				throw new ArgumentNullException ("header");

			return headers.IndexOf (header);
		}

		/// <summary>
		/// Inserts the specified header at the given index.
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
				throw new ArgumentOutOfRangeException ("index");

			if (header == null)
				throw new ArgumentNullException ("header");

			// update the lookup table
			Header hdr;
			if (table.TryGetValue (header.Field, out hdr)) {
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
		/// Removes the header at the specified index.
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
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException ("index");

			var header = headers[index];

			header.Changed -= HeaderChanged;

			if (table[header.Field] == header) {
				table.Remove (header.Field);

				// find the next matching header and add it to the lookup table
				for (int i = index + 1; i < headers.Count; i++) {
					if (icase.Compare (headers[i].Field, header.Field) == 0) {
						table.Add (headers[i].Field, headers[i]);
						break;
					}
				}
			}

			headers.RemoveAt (index);

			OnChanged (header, HeaderListChangedAction.Removed);
		}

		/// <summary>
		/// Gets or sets the <see cref="MimeKit.Header"/> at the specified index.
		/// </summary>
		/// <remarks>
		/// Gets or sets the <see cref="MimeKit.Header"/> at the specified index.
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
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ("index");

				return headers[index];
			}
			set {
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ("index");

				if (value == null)
					throw new ArgumentNullException ("value");

				var header = headers[index];

				if (header == value)
					return;

				header.Changed -= HeaderChanged;
				value.Changed += HeaderChanged;

				if (icase.Compare (header.Field, value.Field) == 0) {
					// replace the old header with the new one
					if (table[header.Field] == header)
						table[header.Field] = value;
				} else {
					// update the table for the header field being replaced
					if (table[header.Field] == header) {
						table.Remove (header.Field);

						// find the next matching header and add it to the lookup table
						for (int i = index + 1; i < headers.Count; i++) {
							if (icase.Compare (headers[i].Field, header.Field) == 0) {
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

				OnChanged (header, HeaderListChangedAction.Removed);
				OnChanged (value, HeaderListChangedAction.Added);
			}
		}

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Gets an enumerator for the list of headers.
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
		/// Gets an enumerator for the list of headers.
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
			if (Changed != null)
				Changed (this, new HeaderListChangedEventArgs (header, action));
		}

		internal bool TryGetHeader (string field, out Header header)
		{
			if (field == null)
				throw new ArgumentNullException ("field");

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
		/// <param name="cancellationToken">A cancellation token.</param>
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
		public static HeaderList Load (ParserOptions options, Stream stream, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (stream == null)
				throw new ArgumentNullException ("stream");

			var parser = new MimeParser (options, stream, MimeFormat.Entity);

			return parser.ParseHeaders (cancellationToken);
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
		/// <param name="cancellationToken">A cancellation token.</param>
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
		public static HeaderList Load (Stream stream, CancellationToken cancellationToken = default (CancellationToken))
		{
			return Load (ParserOptions.Default, stream, cancellationToken);
		}

#if !PORTABLE
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
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters as defined by
		/// <see cref="System.IO.Path.InvalidPathChars"/>.
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
		public static HeaderList Load (ParserOptions options, string fileName, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			using (var stream = File.OpenRead (fileName)) {
				return Load (options, stream, cancellationToken);
			}
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
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters as defined by
		/// <see cref="System.IO.Path.InvalidPathChars"/>.
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
		public static HeaderList Load (string fileName, CancellationToken cancellationToken = default (CancellationToken))
		{
			return Load (ParserOptions.Default, fileName, cancellationToken);
		}
#endif // !PORTABLE
	}
}

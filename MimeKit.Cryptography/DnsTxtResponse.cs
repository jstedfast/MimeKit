//
// DnsTxtResponse.cs
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
using System.Collections.Generic;

namespace MimeKit.Cryptography {
	/// <summary>
	/// The response to a DNS TXT query.
	/// </summary>
	/// <remarks>
	/// The response to a DNS TXT query performed by an <see cref="IDnsResolver"/>.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
	/// </example>
	/// <seealso cref="IDnsResolver"/>
	public sealed class DnsTxtResponse
	{
		static readonly string[] NoRecords = Array.Empty<string> ();

		/// <summary>
		/// Initialize a new instance of the <see cref="DnsTxtResponse"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="DnsTxtResponse"/> that does not contain any records.</para>
		/// <para>This is typically used for <see cref="DnsQueryStatus.NonExistentDomain"/> and
		/// <see cref="DnsQueryStatus.TemporaryFailure"/> responses, but may also be used with
		/// <see cref="DnsQueryStatus.Success"/> to indicate that the domain name exists but has no
		/// TXT records.</para>
		/// </remarks>
		/// <param name="status">The status of the query.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="status"/> is not a valid <see cref="DnsQueryStatus"/>.
		/// </exception>
		public DnsTxtResponse (DnsQueryStatus status)
		{
			if (status < DnsQueryStatus.Success || status > DnsQueryStatus.TemporaryFailure)
				throw new ArgumentOutOfRangeException (nameof (status));

			Records = NoRecords;
			Status = status;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="DnsTxtResponse"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new successful <see cref="DnsTxtResponse"/> containing the specified TXT records.</para>
		/// <para>Each string MUST be the concatenation of all of the character-strings that make up a single
		/// TXT record.</para>
		/// </remarks>
		/// <param name="records">The TXT records.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="records"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// One or more of the <paramref name="records"/> is <see langword="null"/>.
		/// </exception>
		public DnsTxtResponse (IEnumerable<string> records)
		{
			if (records is null)
				throw new ArgumentNullException (nameof (records));

			var list = new List<string> (records);

			for (int i = 0; i < list.Count; i++) {
				if (list[i] is null)
					throw new ArgumentException ("One or more of the records is null.", nameof (records));
			}

			Status = DnsQueryStatus.Success;
			Records = list.AsReadOnly ();
		}

		/// <summary>
		/// Get the status of the query.
		/// </summary>
		/// <remarks>
		/// Gets the status of the query.
		/// </remarks>
		/// <value>The status of the query.</value>
		public DnsQueryStatus Status {
			get; private set;
		}

		/// <summary>
		/// Get the TXT records.
		/// </summary>
		/// <remarks>
		/// Gets the TXT records. Each string is the concatenation of all of the character-strings that make up a
		/// single TXT record.
		/// </remarks>
		/// <value>The TXT records.</value>
		public IReadOnlyList<string> Records {
			get; private set;
		}
	}
}

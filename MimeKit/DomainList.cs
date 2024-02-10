//
// DomainList.cs
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
using System.Text;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;

using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// A domain list.
	/// </summary>
	/// <remarks>
	/// Represents a list of domains, such as those that an email was routed through.
	/// </remarks>
	public class DomainList : IList<string>
	{
		readonly static byte[] DomainSentinels = new [] { (byte) ',', (byte) ':' };
		IList<string> domains;

		/// <summary>
		/// Initialize a new instance of the <see cref="DomainList"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="DomainList"/> based on the domains provided.
		/// </remarks>
		/// <param name="domains">A domain list.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="domains"/> is <c>null</c>.
		/// </exception>
		public DomainList (IEnumerable<string> domains)
		{
			if (domains is null)
				throw new ArgumentNullException (nameof (domains));

			this.domains = new List<string> (domains);
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="DomainList"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="DomainList"/>.
		/// </remarks>
		public DomainList ()
		{
			domains = Array.Empty<string> ();
		}

		void EnsureAllocated ()
		{
			if (domains == Array.Empty<string> ())
				domains = new List<string> ();
		}

		#region IList implementation

		/// <summary>
		/// Get the index of the requested domain, if it exists.
		/// </summary>
		/// <remarks>
		/// Finds the index of the specified domain, if it exists.
		/// </remarks>
		/// <returns>The index of the requested domain; otherwise <value>-1</value>.</returns>
		/// <param name="domain">The domain.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="domain"/> is <c>null</c>.
		/// </exception>
		public int IndexOf (string domain)
		{
			if (domain is null)
				throw new ArgumentNullException (nameof (domain));

			return domains.IndexOf (domain);
		}

		/// <summary>
		/// Insert a domain at the specified index.
		/// </summary>
		/// <remarks>
		/// Inserts the domain at the specified index in the list.
		/// </remarks>
		/// <param name="index">The index to insert the domain.</param>
		/// <param name="domain">The domain to insert.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="domain"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public void Insert (int index, string domain)
		{
			if (domain is null)
				throw new ArgumentNullException (nameof (domain));

			EnsureAllocated ();
			domains.Insert (index, domain);
			OnChanged ();
		}

		/// <summary>
		/// Remove the domain at the specified index.
		/// </summary>
		/// <remarks>
		/// Removes the domain at the specified index.
		/// </remarks>
		/// <param name="index">The index.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public void RemoveAt (int index)
		{
			domains.RemoveAt (index);
			OnChanged ();
		}

		/// <summary>
		/// Get or set the domain at the specified index.
		/// </summary>
		/// <remarks>
		/// Gets or sets the domain at the specified index.
		/// </remarks>
		/// <value>The domain at the specified index.</value>
		/// <param name="index">The index.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public string this [int index] {
			get { return domains[index]; }
			set {
				if (value is null)
					throw new ArgumentNullException (nameof (value));

				if (domains[index] == value)
					return;

				domains[index] = value;
				OnChanged ();
			}
		}

		#endregion

		#region ICollection implementation

		/// <summary>
		/// Add a domain.
		/// </summary>
		/// <remarks>
		/// Adds the specified domain to the end of the list.
		/// </remarks>
		/// <param name="domain">The domain.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="domain"/> is <c>null</c>.
		/// </exception>
		public void Add (string domain)
		{
			if (domain is null)
				throw new ArgumentNullException (nameof (domain));

			EnsureAllocated ();
			domains.Add (domain);
			OnChanged ();
		}

		/// <summary>
		/// Clear the domain list.
		/// </summary>
		/// <remarks>
		/// Removes all of the domains in the list.
		/// </remarks>
		public void Clear ()
		{
			domains.Clear ();
			OnChanged ();
		}

		/// <summary>
		/// Check if the <see cref="DomainList"/> contains the specified domain.
		/// </summary>
		/// <remarks>
		/// Determines whether or not the domain list contains the specified domain.
		/// </remarks>
		/// <returns><value>true</value> if the specified domain is contained;
		/// otherwise <value>false</value>.</returns>
		/// <param name="domain">The domain.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="domain"/> is <c>null</c>.
		/// </exception>
		public bool Contains (string domain)
		{
			if (domain is null)
				throw new ArgumentNullException (nameof (domain));

			return domains.Contains (domain);
		}

		/// <summary>
		/// Copy all of the domains in the <see cref="DomainList"/> to an array.
		/// </summary>
		/// <remarks>
		/// Copies all of the domains within the <see cref="DomainList"/> into the array,
		/// starting at the specified array index.
		/// </remarks>
		/// <param name="array">The array to copy the domains to.</param>
		/// <param name="arrayIndex">The index into the array.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="array"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex"/> is out of range.
		/// </exception>
		public void CopyTo (string[] array, int arrayIndex)
		{
			domains.CopyTo (array, arrayIndex);
		}

		/// <summary>
		/// Remove a domain.
		/// </summary>
		/// <remarks>
		/// Removes the first instance of the specified domain from the list if it exists.
		/// </remarks>
		/// <returns><value>true</value> if the domain was removed; otherwise <value>false</value>.</returns>
		/// <param name="domain">The domain.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="domain"/> is <c>null</c>.
		/// </exception>
		public bool Remove (string domain)
		{
			if (domain is null)
				throw new ArgumentNullException (nameof (domain));

			if (domains.Remove (domain)) {
				OnChanged ();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Get the number of domains in the <see cref="DomainList"/>.
		/// </summary>
		/// <remarks>
		/// Indicates the number of domains in the list.
		/// </remarks>
		/// <value>The number of domains.</value>
		public int Count {
			get { return domains.Count; }
		}

		/// <summary>
		/// Get a value indicating whether the <see cref="DomainList"/> is read only.
		/// </summary>
		/// <remarks>
		/// A <see cref="DomainList"/> is never read-only.
		/// </remarks>
		/// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
		public bool IsReadOnly {
			get { return false; }
		}

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Get an enumerator for the list of domains.
		/// </summary>
		/// <remarks>
		/// Gets an enumerator for the list of domains.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		public IEnumerator<string> GetEnumerator ()
		{
			return domains.GetEnumerator ();
		}

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Get an enumerator for the list of domains.
		/// </summary>
		/// <remarks>
		/// Gets an enumerator for the list of domains.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return domains.GetEnumerator ();
		}

		#endregion

		internal string Encode (FormatOptions options)
		{
			var builder = new ValueStringBuilder (256);

			for (int i = 0; i < domains.Count; i++) {
				if (string.IsNullOrWhiteSpace (domains[i]))
					continue;

				if (builder.Length > 0)
					builder.Append (',');

				builder.Append ('@');

				if (!options.International && ParseUtils.IsInternational (domains[i])) {
					var domain = MailboxAddress.IdnMapping.Encode (domains[i]);

					builder.Append (domain);
				} else {
					builder.Append (domains[i]);
				}
			}

			return builder.ToString ();
		}

		/// <summary>
		/// Return a string representation of the list of domains.
		/// </summary>
		/// <remarks>
		/// <para>Each non-empty domain string will be prepended by an '@'.</para>
		/// <para>If there are multiple domains in the list, they will be separated by a comma.</para>
		/// </remarks>
		/// <returns>A string representing the <see cref="DomainList"/>.</returns>
		public override string ToString ()
		{
			var builder = new ValueStringBuilder (128);

			for (int i = 0; i < domains.Count; i++) {
				if (string.IsNullOrWhiteSpace (domains[i]))
					continue;

				if (builder.Length > 0)
					builder.Append (',');

				builder.Append ('@');

				builder.Append (domains[i]);
			}

			return builder.ToString ();
		}

		internal event EventHandler Changed;

		void OnChanged ()
		{
			Changed?.Invoke (this, EventArgs.Empty);
		}

		/// <summary>
		/// Try to parse a list of domains.
		/// </summary>
		/// <remarks>
		/// Attempts to parse a <see cref="DomainList"/> from the text buffer starting at the
		/// specified index. The index will only be updated if a <see cref="DomainList"/> was
		/// successfully parsed.
		/// </remarks>
		/// <returns><c>true</c> if a <see cref="DomainList"/> was successfully parsed;
		/// <c>false</c> otherwise.</returns>
		/// <param name="buffer">The buffer to parse.</param>
		/// <param name="index">The index to start parsing.</param>
		/// <param name="endIndex">An index of the end of the input.</param>
		/// <param name="throwOnError">A flag indicating whether or not an
		/// exception should be thrown on error.</param>
		/// <param name="route">The parsed DomainList.</param>
		internal static bool TryParse (byte[] buffer, ref int index, int endIndex, bool throwOnError, out DomainList route)
		{
			var domains = new List<string> ();
			int startIndex = index;

			route = null;

			do {
				// skip over the '@'
				index++;

				if (index >= endIndex) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete domain-list at offset: {0}", startIndex), startIndex, index);

					return false;
				}

				if (!ParseUtils.TryParseDomain (buffer, ref index, endIndex, DomainSentinels, throwOnError, out var domain))
					return false;

				domains.Add (domain);

				// Note: obs-domain-list allows for null domains between commas
				do {
					if (!ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, throwOnError))
						return false;

					if (index >= endIndex || buffer[index] != (byte) ',')
						break;

					index++;
				} while (true);

				if (!ParseUtils.SkipCommentsAndWhiteSpace (buffer, ref index, endIndex, throwOnError))
					return false;
			} while (index < buffer.Length && buffer[index] == (byte) '@');

			route = new DomainList (domains);

			return true;
		}

		/// <summary>
		/// Try to parse a list of domains.
		/// </summary>
		/// <remarks>
		/// Attempts to parse a <see cref="DomainList"/> from the supplied text. The index
		/// will only be updated if a <see cref="DomainList"/> was successfully parsed.
		/// </remarks>
		/// <returns><c>true</c> if a <see cref="DomainList"/> was successfully parsed;
		/// <c>false</c> otherwise.</returns>
		/// <param name="text">The text to parse.</param>
		/// <param name="route">The parsed DomainList.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public static bool TryParse (string text, out DomainList route)
		{
			int index = 0;

			if (text is null)
				throw new ArgumentNullException (nameof (text));

			var buffer = Encoding.UTF8.GetBytes (text);

			return TryParse (buffer, ref index, buffer.Length, false, out route);
		}
	}
}

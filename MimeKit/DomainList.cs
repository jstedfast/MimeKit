//
// DomainList.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Jeffrey Stedfast
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
using System.Collections.Generic;

using MimeKit.Utils;

namespace MimeKit {
	public sealed class DomainList : IList<string>
	{
		readonly List<string> domains;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.DomainList"/> class.
		/// </summary>
		/// <param name="domains">A domain list.</param>
		public DomainList (IEnumerable<string> domains)
		{
			this.domains = new List<string> (domains);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.DomainList"/> class.
		/// </summary>
		public DomainList ()
		{
			domains = new List<string> ();
		}

		#region IList implementation

		/// <summary>
		/// Gets the index of the requested domain, if it exists.
		/// </summary>
		/// <returns>The index of the requested domain; otherwise <value>-1</value>.</returns>
		/// <param name="domain">The domain.</param>
		public int IndexOf (string domain)
		{
			return domains.IndexOf (domain);
		}

		/// <summary>
		/// Insert the domain at the specified index.
		/// </summary>
		/// <param name="index">The index to insert the domain.</param>
		/// <param name="domain">The domain to insert.</param>
		public void Insert (int index, string domain)
		{
			if (domain == null)
				throw new ArgumentNullException ("domain");

			domains.Insert (index, domain);
			OnChanged ();
		}

		/// <summary>
		/// Removes the domain at the specified index.
		/// </summary>
		/// <param name="index">The index.</param>
		public void RemoveAt (int index)
		{
			domains.RemoveAt (index);
			OnChanged ();
		}

		/// <summary>
		/// Gets or sets the <see cref="MimeKit.DomainList"/> at the specified index.
		/// </summary>
		/// <param name="index">The index.</param>
		public string this [int index] {
			get { return domains[index]; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (domains[index] == value)
					return;

				domains[index] = value;
				OnChanged ();
			}
		}

		#endregion

		#region ICollection implementation

		/// <summary>
		/// Add the specified domain.
		/// </summary>
		/// <param name="domain">The domain.</param>
		public void Add (string domain)
		{
			if (domain == null)
				throw new ArgumentNullException ("domain");

			domains.Add (domain);
			OnChanged ();
		}

		/// <summary>
		/// Clears the domain list.
		/// </summary>
		public void Clear ()
		{
			domains.Clear ();
			OnChanged ();
		}

		/// <summary>
		/// Checks if the <see cref="DomainList"/> contains the specified domain.
		/// </summary>
		/// <returns><value>true</value> if the specified domain is contained;
		/// otherwise <value>false</value>.</returns>
		/// <param name="domain">The domain.</param>
		public bool Contains (string domain)
		{
			return domains.Contains (domain);
		}

		/// <summary>
		/// Copies all of the domains in the <see cref="MimeKit.DomainList"/> to the specified array.
		/// </summary>
		/// <param name="array">The array to copy the domains to.</param>
		/// <param name="arrayIndex">The index into the array.</param>
		public void CopyTo (string[] array, int arrayIndex)
		{
			domains.CopyTo (array, arrayIndex);
		}

		/// <summary>
		/// Removes the specified domain.
		/// </summary>
		/// <returns><value>true</value> if the specified domain was removed;
		/// otherwise <value>false</value>.</returns>
		/// <param name="domain">The domain.</param>
		public bool Remove (string domain)
		{
			if (domains.Remove (domain)) {
				OnChanged ();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Gets the number of headers in the <see cref="MimeKit.DomainList"/>.
		/// </summary>
		/// <value>The number of headers.</value>
		public int Count {
			get { return domains.Count; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
		public bool IsReadOnly {
			get { return false; }
		}

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Gets an enumerator for the list of domains.
		/// </summary>
		/// <returns>The enumerator.</returns>
		public IEnumerator<string> GetEnumerator ()
		{
			return domains.GetEnumerator ();
		}

		#endregion

		#region IEnumerable implementation

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return domains.GetEnumerator ();
		}

		#endregion

		/// <summary>
		/// Serializes the <see cref="MimeKit.DomainList"/> to a string.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="MimeKit.DomainList"/>.</returns>
		public override string ToString ()
		{
			StringBuilder builder = new StringBuilder ();

			for (int i = 0; i < domains.Count; i++) {
				if (string.IsNullOrWhiteSpace (domains[i]) && builder.Length == 0)
					continue;

				if (builder.Length > 0)
					builder.Append (',');

				if (!string.IsNullOrWhiteSpace (domains[i]))
					builder.Append ('@');

				builder.Append (domains[i]);
			}

			return builder.ToString ();
		}

		public event EventHandler Changed;

		void OnChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}

		/// <summary>
		/// Attempts to parse a <see cref="DomainList"/> from the text buffer starting at the
		/// specified index. The index will only be updated if a <see cref="DomainList"/> was
		/// successfully parsed.
		/// </summary>
		/// <returns><c>true</c> if a <see cref="DomainList"/> was successfully parsed;
		/// <c>false</c> otherwise.</returns>
		/// <param name="text">The text buffer to parse.</param>
		/// <param name="index">The index to start parsing.</param>
		/// <param name="endIndex">An index of the end of the input.</param>
		/// <param name="throwOnError">A flag indicating whether or not an
		/// exception should be thrown on error.</param>
		/// <param name="route">The parsed DomainList.</param>
		internal static bool TryParse (byte[] text, ref int index, int endIndex, bool throwOnError, out DomainList route)
		{
			List<string> domains = new List<string> ();
			int startIndex = index;
			string domain;

			route = null;

			do {
				// skip over the '@'
				index++;

				if (index >= endIndex) {
					if (throwOnError)
						throw new ParseException (string.Format ("Incomplete domain-list at offset: {0}", startIndex), startIndex, index);

					return false;
				}

				if (!ParseUtils.TryParseDomain (text, ref index, endIndex, throwOnError, out domain))
					return false;

				domains.Add (domain);

				// Note: obs-domain-list allows for null domains between commas
				do {
					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;

					if (index >= endIndex || text[index] != (byte) ',')
						break;

					index++;
				} while (true);

				if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;
			} while (index < text.Length && text[index] == (byte) '@');

			route = new DomainList (domains);

			return true;
		}
	}
}

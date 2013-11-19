//
// CmsRecipientCollection.cs
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
using System.Collections;
using System.Collections.Generic;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A collection of <see cref="CmsRecipient"/>s.
	/// </summary>
	public sealed class CmsRecipientCollection : ICollection<CmsRecipient>
	{
		readonly IList<CmsRecipient> recipients;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.CmsRecipientCollection"/> class.
		/// </summary>
		public CmsRecipientCollection ()
		{
			recipients = new List<CmsRecipient> ();
		}

		#region ICollection implementation

		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
		public bool IsReadOnly {
			get; private set;
		}

		/// <summary>
		/// Gets the number of recipients in the collection.
		/// </summary>
		/// <value>The number of recipients in the collection.</value>
		public int Count {
			get { return recipients.Count; }
		}

		/// <summary>
		/// Adds the specified recipient.
		/// </summary>
		/// <param name="recipient">The recipient.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="recipient"/> is <c>null</c>.
		/// </exception>
		public void Add (CmsRecipient recipient)
		{
			if (recipient == null)
				throw new ArgumentNullException ("recipient");

			recipients.Add (recipient);
		}

		/// <summary>
		/// Clears the recipient collection.
		/// </summary>
		public void Clear ()
		{
			recipients.Clear ();
		}

		/// <summary>
		/// Check if the collection contains the specified recipient.
		/// </summary>
		/// <param name="recipient">The recipient.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="recipient"/> is <c>null</c>.
		/// </exception>
		public bool Contains (CmsRecipient recipient)
		{
			if (recipient == null)
				throw new ArgumentNullException ("recipient");

			return recipients.Contains (recipient);
		}

		/// <summary>
		/// Copies the recpients into the specified array starting at the specified index.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">The array index.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="array"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex"/> is out of range.
		/// </exception>
		public void CopyTo (CmsRecipient[] array, int arrayIndex)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			if (arrayIndex < 0 || arrayIndex + Count > array.Length)
				throw new ArgumentOutOfRangeException ("arrayIndex");

			recipients.CopyTo (array, arrayIndex);
		}

		/// <summary>
		/// Remove the specified recipient.
		/// </summary>
		/// <param name="recipient">The recipient.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="recipient"/> is <c>null</c>.
		/// </exception>
		public bool Remove (CmsRecipient recipient)
		{
			if (recipient == null)
				throw new ArgumentNullException ("recipient");

			return recipients.Remove (recipient);
		}

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Gets an enumerator for the collection of recipients.
		/// </summary>
		/// <returns>The enumerator.</returns>
		public IEnumerator<CmsRecipient> GetEnumerator ()
		{
			return recipients.GetEnumerator ();
		}

		#endregion

		#region IEnumerable implementation

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return recipients.GetEnumerator ();
		}

		#endregion
	}
}

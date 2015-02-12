//
// CmsRecipientCollection.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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
	/// A collection of <see cref="CmsRecipient"/> objects.
	/// </summary>
	/// <remarks>
	/// If the X.509 certificates are known for each of the recipients, you
	/// may wish to use a <see cref="CmsRecipientCollection"/> as opposed to
	/// using the methods that take a list of <see cref="MailboxAddress"/>
	/// objects.
	/// </remarks>
	public class CmsRecipientCollection : ICollection<CmsRecipient>
	{
		readonly IList<CmsRecipient> recipients;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.CmsRecipientCollection"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="CmsRecipientCollection"/>.
		/// </remarks>
		public CmsRecipientCollection ()
		{
			recipients = new List<CmsRecipient> ();
		}

		#region ICollection implementation

		/// <summary>
		/// Gets the number of recipients in the collection.
		/// </summary>
		/// <remarks>
		/// Indicates the number of recipients in the collection.
		/// </remarks>
		/// <value>The number of recipients in the collection.</value>
		public int Count {
			get { return recipients.Count; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <remarks>
		/// A <see cref="CmsRecipientCollection"/> is never read-only.
		/// </remarks>
		/// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
		public bool IsReadOnly {
			get; private set;
		}

		/// <summary>
		/// Adds the specified recipient.
		/// </summary>
		/// <remarks>
		/// Adds the specified recipient.
		/// </remarks>
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
		/// <remarks>
		/// Removes all of the recipients from the collection.
		/// </remarks>
		public void Clear ()
		{
			recipients.Clear ();
		}

		/// <summary>
		/// Checks if the collection contains the specified recipient.
		/// </summary>
		/// <remarks>
		/// Determines whether or not the collection contains the specified recipient.
		/// </remarks>
		/// <returns><value>true</value> if the specified recipient exists;
		/// otherwise <value>false</value>.</returns>
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
		/// Copies all of the recipients in the <see cref="CmsRecipientCollection"/> to the specified array.
		/// </summary>
		/// <remarks>
		/// Copies all of the recipients within the <see cref="CmsRecipientCollection"/> into the array,
		/// starting at the specified array index.
		/// </remarks>
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
		/// Removes the specified recipient.
		/// </summary>
		/// <remarks>
		/// Removes the specified recipient.
		/// </remarks>
		/// <returns><value>true</value> if the recipient was removed; otherwise <value>false</value>.</returns>
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
		/// <remarks>
		/// Gets an enumerator for the collection of recipients.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		public IEnumerator<CmsRecipient> GetEnumerator ()
		{
			return recipients.GetEnumerator ();
		}

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Gets an enumerator for the collection of recipients.
		/// </summary>
		/// <remarks>
		/// Gets an enumerator for the collection of recipients.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return recipients.GetEnumerator ();
		}

		#endregion
	}
}

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
	public class CmsRecipientCollection : ICollection<CmsRecipient>
	{
		readonly IList<CmsRecipient> recipients;

		public CmsRecipientCollection ()
		{
			recipients = new List<CmsRecipient> ();
		}

		#region ICollection implementation

		public bool IsReadOnly {
			get; private set;
		}

		public int Count {
			get { return recipients.Count; }
		}

		public void Add (CmsRecipient recipient)
		{
			if (recipient == null)
				throw new ArgumentNullException ("recipient");

			recipients.Add (recipient);
		}

		public void Clear ()
		{
			recipients.Clear ();
		}

		public bool Contains (CmsRecipient recipient)
		{
			if (recipient == null)
				throw new ArgumentNullException ("recipient");

			return recipients.Contains (recipient);
		}

		public void CopyTo (CmsRecipient[] array, int arrayIndex)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			if (arrayIndex < 0 || arrayIndex + Count > array.Length)
				throw new ArgumentOutOfRangeException ("arrayIndex");

			recipients.CopyTo (array, arrayIndex);
		}

		public bool Remove (CmsRecipient recipient)
		{
			if (recipient == null)
				throw new ArgumentNullException ("recipient");

			return recipients.Remove (recipient);
		}

		#endregion

		#region IEnumerable implementation

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

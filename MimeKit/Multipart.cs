//
// Multipart.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2012 Jeffrey Stedfast
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
using System.Collections;
using System.Collections.Generic;

namespace MimeKit {
	public class Multipart : MimeEntity, ICollection<MimeEntity>, IList<MimeEntity>
	{
		static readonly StringComparer icase = StringComparer.InvariantCultureIgnoreCase;

		string boundary, preamble, epilogue;
		byte[] rawPreamble, rawEpilogue;
		List<MimeEntity> children;

		public Multipart (string subtype) : base ("multipart", subtype)
		{
			children = new List<MimeEntity> ();
			Boundary = GenerateBoundary ();
		}

		public Multipart () : this ("mixed")
		{
		}

		static string GenerateBoundary ()
		{
			var base64 = new Base64Encoder ();
			var rand = new Random ();
			var digest = new byte[16];
			var b64buf = new byte[32];
			int length;

			rand.NextBytes (digest);
			length = base64.Flush (digest, 0, 16, b64buf);

			return "=-" + Encoding.ASCII.GetString (b64buf, 0, length);
		}

		public string Boundary {
			get { return boundary; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (boundary == value)
					return;

				ContentType.Parameters["boundary"] = value;
			}
		}

		public string Preamble {
			get {
				if (preamble == null && rawPreamble != null)
					preamble = CharsetUtils.ConvertToUnicode (rawPreamble, 0, rawPreamble.Length);

				return preamble;
			}
			set {
				if (preamble == value)
					return;

				if (value != null) {
					rawPreamble = Encoding.ASCII.GetBytes (value);
					preamble = value;
				} else {
					rawPreamble = null;
					preamble = null;
				}
			}
		}

		public string Epilogue {
			get {
				if (epilogue == null && rawEpilogue != null)
					epilogue = CharsetUtils.ConvertToUnicode (rawEpilogue, 0, rawEpilogue.Length);

				return epilogue;
			}
			set {
				if (epilogue == value)
					return;

				if (value != null) {
					rawEpilogue = Encoding.ASCII.GetBytes (value);
					epilogue = value;
				} else {
					rawEpilogue = null;
					epilogue = null;
				}
			}
		}

		protected override void OnContentTypeChanged (object sender, EventArgs e)
		{
			boundary = ContentType.Parameters["boundary"];

			base.OnContentTypeChanged (sender, e);
		}

		public override void WriteTo (Stream stream)
		{
			if (boundary == null)
				Boundary = GenerateBoundary ();

			base.WriteTo (stream);

			if (rawPreamble != null) {
				stream.Write (rawPreamble, 0, rawPreamble.Length);
				stream.WriteByte ((byte) '\n');
			}

			var bytes = Encoding.ASCII.GetBytes ("--" + boundary + "--\n");

			foreach (var part in children) {
				stream.Write (bytes, 0, bytes.Length - 3);
				stream.WriteByte ((byte) '\n');
				part.WriteTo (stream);
			}

			stream.Write (bytes, 0, bytes.Length);

			if (rawEpilogue != null) {
				stream.Write (rawEpilogue, 0, rawEpilogue.Length);
				stream.WriteByte ((byte) '\n');
			}
		}

		#region ICollection implementation

		public int Count {
			get { return children.Count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public void Add (MimeEntity part)
		{
			if (part == null)
				throw new ArgumentNullException ("part");

			children.Add (part);
			OnChanged ();
		}

		public void Clear ()
		{
			children.Clear ();
			OnChanged ();
		}

		public bool Contains (MimeEntity part)
		{
			if (part == null)
				throw new ArgumentNullException ("part");

			return children.Contains (part);
		}

		public void CopyTo (MimeEntity[] array, int arrayIndex)
		{
			children.CopyTo (array, arrayIndex);
		}

		public bool Remove (MimeEntity part)
		{
			if (part == null)
				throw new ArgumentNullException ("part");

			if (children.Remove (part)) {
				OnChanged ();
				return true;
			}

			return false;
		}

		#endregion

		#region IList implementation

		public int IndexOf (MimeEntity part)
		{
			if (part == null)
				throw new ArgumentNullException ("part");

			return children.IndexOf (part);
		}

		public void Insert (int index, MimeEntity part)
		{
			if (index < 0 || index > children.Count)
				throw new ArgumentOutOfRangeException ("index");

			if (part == null)
				throw new ArgumentNullException ("part");

			children.Insert (index, part);
			OnChanged ();
		}

		public void RemoveAt (int index)
		{
			children.RemoveAt (index);
			OnChanged ();
		}

		public MimeEntity this[int index] {
			get { return children[index]; }
			set { children[index] = value; }
		}

		#endregion

		#region IEnumerable implementation

		public IEnumerator<MimeEntity> GetEnumerator ()
		{
			return children.GetEnumerator ();
		}

		#endregion

		#region IEnumerable implementation

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return children.GetEnumerator ();
		}

		#endregion
	}
}

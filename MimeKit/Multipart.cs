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
		string preamble, epilogue;
		List<MimeEntity> children;

		public Multipart (ContentType type, IEnumerable<Header> headers, bool toplevel) : base (type, headers, toplevel)
		{
			children = new List<MimeEntity> ();
		}

		public Multipart (string subtype) : base ("multipart", subtype)
		{
			ContentType.Parameters["boundary"] = GenerateBoundary ();
			children = new List<MimeEntity> ();
		}

		public Multipart () : this ("mixed")
		{
		}

		static string GenerateBoundary ()
		{
			var base64 = new Base64Encoder (true);
			var rand = new Random ();
			var digest = new byte[16];
			var b64buf = new byte[24];
			int length;

			rand.NextBytes (digest);
			length = base64.Flush (digest, 0, 16, b64buf);

			return "=-" + Encoding.ASCII.GetString (b64buf, 0, length);
		}

		public string Boundary {
			get { return ContentType.Parameters["boundary"]; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (Boundary == value)
					return;

				ContentType.Parameters["boundary"] = value;
			}
		}

		internal byte[] RawPreamble {
			get; set;
		}

		public string Preamble {
			get {
				if (preamble == null && RawPreamble != null)
					preamble = CharsetUtils.ConvertToUnicode (RawPreamble, 0, RawPreamble.Length);

				return preamble;
			}
			set {
				if (preamble == value)
					return;

				if (value != null) {
					// FIXME: fold the preamble?
					RawPreamble = Encoding.ASCII.GetBytes (value);
					preamble = value;
				} else {
					RawPreamble = null;
					preamble = null;
				}
			}
		}

		internal byte[] RawEpilogue {
			get; set;
		}

		public string Epilogue {
			get {
				if (epilogue == null && RawEpilogue != null)
					epilogue = CharsetUtils.ConvertToUnicode (RawEpilogue, 0, RawEpilogue.Length);

				return epilogue;
			}
			set {
				if (epilogue == value)
					return;

				if (value != null) {
					// FIXME: fold the epilogue?
					RawEpilogue = Encoding.ASCII.GetBytes (value);
					epilogue = value;
				} else {
					RawEpilogue = null;
					epilogue = null;
				}
			}
		}

		public override void WriteTo (Stream stream)
		{
			if (Boundary == null)
				Boundary = GenerateBoundary ();

			base.WriteTo (stream);

			if (RawPreamble != null) {
				stream.Write (RawPreamble, 0, RawPreamble.Length);
				stream.WriteByte ((byte) '\n');
			}

			var boundary = Encoding.ASCII.GetBytes ("--" + Boundary + "--");

			foreach (var part in children) {
				stream.Write (boundary, 0, boundary.Length - 2);
				stream.WriteByte ((byte) '\n');
				part.WriteTo (stream);
			}

			stream.Write (boundary, 0, boundary.Length);
			stream.WriteByte ((byte) '\n');

			if (RawEpilogue != null) {
				stream.Write (RawEpilogue, 0, RawEpilogue.Length);
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

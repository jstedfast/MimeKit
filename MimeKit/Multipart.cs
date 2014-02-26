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
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using MimeKit.Encodings;
using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// A Multipart MIME part which may contain a collection of MIME parts.
	/// </summary>
	public class Multipart : MimeEntity, ICollection<MimeEntity>, IList<MimeEntity>
	{
		static int seed = (int) DateTime.Now.Ticks;
		readonly List<MimeEntity> children;
		string preamble, epilogue;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Multipart"/> class.
		/// </summary>
		/// <remarks>This constructor is used by <see cref="MimeKit.MimeParser"/>.</remarks>
		/// <param name="entity">Information used by the constructor.</param>
		public Multipart (MimeEntityConstructorInfo entity) : base (entity)
		{
			children = new List<MimeEntity> ();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Multipart"/> class.
		/// </summary>
		/// <param name="subtype">The multipart media sub-type.</param>
		/// <param name="args">An array of initialization parameters: headers and MIME entities.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="subtype"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="args"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="args"/> contains one or more arguments of an unknown type.
		/// </exception>
		public Multipart (string subtype, params object[] args) : this (subtype)
		{
			if (args == null)
				throw new ArgumentNullException ("args");

			foreach (object obj in args) {
				if (obj == null || TryInit (obj))
					continue;

				var entity = obj as MimeEntity;
				if (entity != null) {
					Add (entity);
					continue;
				}

				throw new ArgumentException ("Unknown initialization parameter: " + obj.GetType ());
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Multipart"/> class.
		/// </summary>
		/// <param name="subtype">The multipart media sub-type.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="subtype"/> is <c>null</c>.
		/// </exception>
		public Multipart (string subtype) : base ("multipart", subtype)
		{
			ContentType.Parameters["boundary"] = GenerateBoundary ();
			children = new List<MimeEntity> ();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Multipart"/> class
		/// with a default Content-Type of multipart/mixed.
		/// </summary>
		public Multipart () : this ("mixed")
		{
		}

		static string GenerateBoundary ()
		{
			var random = new Random (seed++);
			var digest = new byte[16];

			random.NextBytes (digest);

			return "=-" + Convert.ToBase64String (digest);
		}

		/// <summary>
		/// Gets or sets the boundary.
		/// </summary>
		/// <value>The boundary.</value>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		public string Boundary {
			get { return ContentType.Boundary; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (Boundary == value)
					return;

				ContentType.Boundary = value.Trim ();
			}
		}

		internal byte[] RawPreamble {
			get; set;
		}

		/// <summary>
		/// Gets or sets the preamble.
		/// </summary>
		/// <value>The preamble.</value>
		public string Preamble {
			get {
				if (preamble == null && RawPreamble != null)
					preamble = CharsetUtils.ConvertToUnicode (Headers.Options, RawPreamble, 0, RawPreamble.Length);

				return preamble;
			}
			set {
				if (preamble == value)
					return;

				if (value != null) {
					var folded = FoldPreambleOrEpilogue (FormatOptions.Default, value);
					RawPreamble = Encoding.ASCII.GetBytes (folded);
					preamble = folded;
				} else {
					RawPreamble = null;
					preamble = null;
				}
			}
		}

		internal byte[] RawEpilogue {
			get; set;
		}

		/// <summary>
		/// Gets or sets the epilogue.
		/// </summary>
		/// <value>The epilogue.</value>
		public string Epilogue {
			get {
				if (epilogue == null && RawEpilogue != null)
					epilogue = CharsetUtils.ConvertToUnicode (Headers.Options, RawEpilogue, 0, RawEpilogue.Length);

				return epilogue;
			}
			set {
				if (epilogue == value)
					return;

				if (value != null) {
					var folded = FoldPreambleOrEpilogue (FormatOptions.Default, value);
					RawEpilogue = Encoding.ASCII.GetBytes (folded);
					epilogue = folded;
				} else {
					RawEpilogue = null;
					epilogue = null;
				}
			}
		}

		static string FoldPreambleOrEpilogue (FormatOptions options, string text)
		{
			var builder = new StringBuilder ();
			int startIndex, wordIndex;
			int lineLength = 0;
			int index = 0;

			while (index < text.Length) {
				startIndex = index;

				while (index < text.Length) {
					if (!char.IsWhiteSpace (text[index]))
						break;

					if (text[index] == '\n') {
						builder.Append (options.NewLine);
						startIndex = index + 1;
						lineLength = 0;
					}

					index++;
				}

				wordIndex = index;

				while (index < text.Length && !char.IsWhiteSpace (text[index]))
					index++;

				int length = index - startIndex;

				if (lineLength > 0 && lineLength + length >= options.MaxLineLength) {
					builder.Append (options.NewLine);
					length = index - wordIndex;
					startIndex = wordIndex;
					lineLength = 0;
				}

				if (length > 0) {
					builder.Append (text, startIndex, length);
					lineLength += length;
				}
			}

			if (lineLength > 0)
				builder.Append (options.NewLine);

			return builder.ToString ();
		}

		static void WriteBytes (FormatOptions options, Stream stream, byte[] bytes)
		{
			var filter = options.CreateNewLineFilter ();
			int index, length;

			var output = filter.Flush (bytes, 0, bytes.Length, out index, out length);

			stream.Write (output, index, length);
		}

		/// <summary>
		/// Writes the <see cref="MimeKit.Multipart"/> to the specified output stream.
		/// </summary>
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
		public override void WriteTo (FormatOptions options, Stream stream, CancellationToken cancellationToken)
		{
			if (Boundary == null)
				Boundary = GenerateBoundary ();

			base.WriteTo (options, stream, cancellationToken);

			if (RawPreamble != null && RawPreamble.Length > 0) {
				cancellationToken.ThrowIfCancellationRequested ();
				WriteBytes (options, stream, RawPreamble);
			}

			var boundary = Encoding.ASCII.GetBytes ("--" + Boundary + "--");

			foreach (var part in children) {
				cancellationToken.ThrowIfCancellationRequested ();
				stream.Write (boundary, 0, boundary.Length - 2);
				stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
				part.WriteTo (options, stream, cancellationToken);
				stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
			}

			cancellationToken.ThrowIfCancellationRequested ();
			stream.Write (boundary, 0, boundary.Length);
			stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);

			if (RawEpilogue != null && RawEpilogue.Length > 0) {
				cancellationToken.ThrowIfCancellationRequested ();
				WriteBytes (options, stream, RawEpilogue);
			}
		}

		#region ICollection implementation

		/// <summary>
		/// Gets the number of child parts.
		/// </summary>
		/// <value>The number of child parts.</value>
		public int Count {
			get { return children.Count; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
		public bool IsReadOnly {
			get { return false; }
		}

		/// <summary>
		/// Adds the specified part.
		/// </summary>
		/// <param name="part">The part to add.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="part"/> is <c>null</c>.
		/// </exception>
		public void Add (MimeEntity part)
		{
			if (part == null)
				throw new ArgumentNullException ("part");

			children.Add (part);
			OnChanged ();
		}

		/// <summary>
		/// Removes all of the children.
		/// </summary>
		public void Clear ()
		{
			children.Clear ();
			OnChanged ();
		}

		/// <summary>
		/// Checks if the <see cref="MimeKit.Multipart"/> contains the specified part.
		/// </summary>
		/// <param name="part">The part to check for.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="part"/> is <c>null</c>.
		/// </exception>
		public bool Contains (MimeEntity part)
		{
			if (part == null)
				throw new ArgumentNullException ("part");

			return children.Contains (part);
		}

		/// <summary>
		/// Copies all of the entities in the <see cref="MimeKit.Multipart"/> to the specified array.
		/// </summary>
		/// <param name="array">The array to copy the headers to.</param>
		/// <param name="arrayIndex">The index into the array.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="array"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex"/> is out of range.
		/// </exception>
		public void CopyTo (MimeEntity[] array, int arrayIndex)
		{
			children.CopyTo (array, arrayIndex);
		}

		/// <summary>
		/// Remove the specified part.
		/// </summary>
		/// <returns><c>true</c> if the part was removed; otherwise <c>false</c>.</returns>
		/// <param name="part">The part to remove.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="part"/> is <c>null</c>.
		/// </exception>
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

		/// <summary>
		/// Gets the index of the specified part.
		/// </summary>
		/// <returns>The index of the specified part if found; otherwise <c>-1</c>.</returns>
		/// <param name="part">The part.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="part"/> is <c>null</c>.
		/// </exception>
		public int IndexOf (MimeEntity part)
		{
			if (part == null)
				throw new ArgumentNullException ("part");

			return children.IndexOf (part);
		}

		/// <summary>
		/// Insert the part atthe specified index.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="part">The part.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="part"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public void Insert (int index, MimeEntity part)
		{
			if (index < 0 || index > children.Count)
				throw new ArgumentOutOfRangeException ("index");

			if (part == null)
				throw new ArgumentNullException ("part");

			children.Insert (index, part);
			OnChanged ();
		}

		/// <summary>
		/// Removes the part at the specified index.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public void RemoveAt (int index)
		{
			children.RemoveAt (index);
			OnChanged ();
		}

		/// <summary>
		/// Gets or sets the <see cref="MimeKit.MimeEntity"/> at the specified index.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public MimeEntity this[int index] {
			get { return children[index]; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				children[index] = value;
			}
		}

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Gets the enumerator for the children of the <see cref="MimeKit.Multipart"/>.
		/// </summary>
		/// <returns>The enumerator.</returns>
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

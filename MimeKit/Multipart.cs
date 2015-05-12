//
// Multipart.cs
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

using MimeKit.Encodings;
using MimeKit.Utils;
using MimeKit.IO;

namespace MimeKit {
	/// <summary>
	/// A multipart MIME entity which may contain a collection of MIME entities.
	/// </summary>
	/// <remarks>
	/// <para>All multipart MIME entities will have a Content-Type with a media type of <c>"multipart"</c>.
	/// The most common multipart MIME entity used in email is the <c>"multipart/mixed"</c> entity.</para>
	/// <para>Four (4) initial subtypes were defined in the original MIME specifications: mixed, alternative,
	/// digest, and parallel.</para>
	/// <para>The "multipart/mixed" type is a sort of general-purpose container. When used in email, the
	/// first entity is typically the "body" of the message while additional entities are most often
	/// file attachments.</para>
	/// <para>Speaking of message "bodies", the "multipart/alternative" type is used to offer a list of
	/// alternative formats for the main body of the message (usually they will be "text/plain" and
	/// "text/html"). These alternatives are in order of increasing faithfulness to the original document
	/// (in other words, the last entity will be in a format that, when rendered, will most closely match
	/// what the sending client's WYSISYG editor produced).</para>
	/// <para>The "multipart/digest" type will typically contain a digest of MIME messages and is most
	/// commonly used by mailing-list software.</para>
	/// <para>The "multipart/parallel" type contains entities that are all meant to be shown (or heard)
	/// in parallel.</para>
	/// <para>Another commonly used type is the "multipart/related" type which contains, as one might expect,
	/// inter-related MIME parts which typically reference each other via URIs based on the Content-Id and/or
	/// Content-Location headers.</para>
	/// </remarks>
	public class Multipart : MimeEntity, ICollection<MimeEntity>, IList<MimeEntity>
	{
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
		/// <remarks>
		/// Creates a new <see cref="Multipart"/> with the specified subtype.
		/// </remarks>
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
		/// <remarks>
		/// Creates a new <see cref="Multipart"/> with the specified subtype.
		/// </remarks>
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
		/// Initializes a new instance of the <see cref="MimeKit.Multipart"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="Multipart"/> with a ContentType of multipart/mixed.
		/// </remarks>
		public Multipart () : this ("mixed")
		{
		}

		static string GenerateBoundary ()
		{
			var base64 = new Base64Encoder (true);
			var digest = new byte[16];
			var buf = new byte[24];
			int length;

			MimeUtils.GetRandomBytes (digest);

			length = base64.Flush (digest, 0, digest.Length, buf);

			return "=-" + Encoding.ASCII.GetString (buf, 0, length);
		}

		/// <summary>
		/// Gets or sets the boundary.
		/// </summary>
		/// <remarks>
		/// Gets or sets the boundary parameter on the Content-Type header.
		/// </remarks>
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
		/// <remarks>
		/// A multipart preamble appears before the first child entity of the
		/// multipart and is typically used only in the top-level multipart
		/// of the message to specify that the message is in MIME format and
		/// therefore requires a MIME compliant email application to render
		/// it correctly.
		/// </remarks>
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
					var options = FormatOptions.GetDefault ();
					var folded = FoldPreambleOrEpilogue (options, value);
					RawPreamble = Encoding.UTF8.GetBytes (folded);
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
		/// <remarks>
		/// A multipart epiloque is the text that ppears after the last
		/// child of the multipart and is rarely ever used.
		/// </remarks>
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
					var options = FormatOptions.GetDefault ();
					var folded = FoldPreambleOrEpilogue (options, value);
					RawEpilogue = Encoding.UTF8.GetBytes (folded);
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

		static void WriteBytes (FormatOptions options, Stream stream, byte[] bytes, CancellationToken cancellationToken)
		{
			var cancellable = stream as ICancellableStream;
			var filter = options.CreateNewLineFilter ();
			int index, length;

			var output = filter.Flush (bytes, 0, bytes.Length, out index, out length);

			if (cancellable != null) {
				cancellable.Write (output, index, length, cancellationToken);
			} else {
				cancellationToken.ThrowIfCancellationRequested ();
				stream.Write (output, index, length);
			}
		}

		/// <summary>
		/// Writes the <see cref="MimeKit.Multipart"/> to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Writes the multipart MIME entity and its subparts to the output stream.
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
		public override void WriteTo (FormatOptions options, Stream stream, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (Boundary == null)
				Boundary = GenerateBoundary ();

			base.WriteTo (options, stream, cancellationToken);

			if (ContentType.Matches ("multipart", "signed")) {
				// don't reformat the headers or content of any children of a multipart/signed
				if (options.International || options.HiddenHeaders.Count > 0) {
					options = options.Clone ();
					options.HiddenHeaders.Clear ();
					options.International = false;
				}
			}

			var cancellable = stream as ICancellableStream;

			if (RawPreamble != null && RawPreamble.Length > 0)
				WriteBytes (options, stream, RawPreamble, cancellationToken);

			var boundary = Encoding.ASCII.GetBytes ("--" + Boundary + "--");

			if (cancellable != null) {
				for (int i = 0; i < children.Count; i++) {
					cancellable.Write (boundary, 0, boundary.Length - 2, cancellationToken);
					cancellable.Write (options.NewLineBytes, 0, options.NewLineBytes.Length, cancellationToken);
					children[i].WriteTo (options, stream, cancellationToken);
					cancellable.Write (options.NewLineBytes, 0, options.NewLineBytes.Length, cancellationToken);
				}

				cancellable.Write (boundary, 0, boundary.Length, cancellationToken);
				cancellable.Write (options.NewLineBytes, 0, options.NewLineBytes.Length, cancellationToken);
			} else {
				for (int i = 0; i < children.Count; i++) {
					cancellationToken.ThrowIfCancellationRequested ();
					stream.Write (boundary, 0, boundary.Length - 2);
					stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
					children[i].WriteTo (options, stream, cancellationToken);
					stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
				}

				cancellationToken.ThrowIfCancellationRequested ();
				stream.Write (boundary, 0, boundary.Length);
				stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
			}

			if (RawEpilogue != null && RawEpilogue.Length > 0)
				WriteBytes (options, stream, RawEpilogue, cancellationToken);
		}

		#region ICollection implementation

		/// <summary>
		/// Gets the number of parts in the multipart.
		/// </summary>
		/// <remarks>
		/// Indicates the number of parts in the multipart.
		/// </remarks>
		/// <value>The number of parts in the multipart.</value>
		public int Count {
			get { return children.Count; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <remarks>
		/// A <see cref="Multipart"/> is never read-only.
		/// </remarks>
		/// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
		public bool IsReadOnly {
			get { return false; }
		}

		/// <summary>
		/// Adds the specified part.
		/// </summary>
		/// <remarks>
		/// Adds the specified part to the multipart.
		/// </remarks>
		/// <param name="part">The part to add.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="part"/> is <c>null</c>.
		/// </exception>
		public void Add (MimeEntity part)
		{
			if (part == null)
				throw new ArgumentNullException ("part");

			children.Add (part);
		}

		/// <summary>
		/// Clears the multipart.
		/// </summary>
		/// <remarks>
		/// Removes all of the parts within the multipart.
		/// </remarks>
		public void Clear ()
		{
			children.Clear ();
		}

		/// <summary>
		/// Checks if the <see cref="Multipart"/> contains the specified part.
		/// </summary>
		/// <remarks>
		/// Determines whether or not the multipart contains the specified part.
		/// </remarks>
		/// <returns><value>true</value> if the specified part exists;
		/// otherwise <value>false</value>.</returns>
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
		/// Copies all of the entities in the <see cref="Multipart"/> to the specified array.
		/// </summary>
		/// <remarks>
		/// Copies all of the entities within the <see cref="Multipart"/> into the array,
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
		public void CopyTo (MimeEntity[] array, int arrayIndex)
		{
			children.CopyTo (array, arrayIndex);
		}

		/// <summary>
		/// Removes the specified part.
		/// </summary>
		/// <remarks>
		/// Removes the specified part, if it exists within the multipart.
		/// </remarks>
		/// <returns><value>true</value> if the part was removed; otherwise <value>false</value>.</returns>
		/// <param name="part">The part to remove.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="part"/> is <c>null</c>.
		/// </exception>
		public bool Remove (MimeEntity part)
		{
			if (part == null)
				throw new ArgumentNullException ("part");

			return children.Remove (part);
		}

		#endregion

		#region IList implementation

		/// <summary>
		/// Gets the index of the specified part.
		/// </summary>
		/// <remarks>
		/// Finds the index of the specified part, if it exists.
		/// </remarks>
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
		/// Inserts the part at the specified index.
		/// </summary>
		/// <remarks>
		/// Inserts the part into the multipart at the specified index.
		/// </remarks>
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
		}

		/// <summary>
		/// Removes the part at the specified index.
		/// </summary>
		/// <remarks>
		/// Removes the part at the specified index.
		/// </remarks>
		/// <param name="index">The index.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public void RemoveAt (int index)
		{
			children.RemoveAt (index);
		}

		/// <summary>
		/// Gets or sets the <see cref="MimeEntity"/> at the specified index.
		/// </summary>
		/// <remarks>
		/// Gets or sets the <see cref="MimeEntity"/> at the specified index.
		/// </remarks>
		/// <value>The entity at the specified index.</value>
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
		/// Gets the enumerator for the children of the <see cref="Multipart"/>.
		/// </summary>
		/// <remarks>
		/// Gets the enumerator for the children of the <see cref="Multipart"/>.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		public IEnumerator<MimeEntity> GetEnumerator ()
		{
			return children.GetEnumerator ();
		}

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Gets the enumerator for the children of the <see cref="Multipart"/>.
		/// </summary>
		/// <remarks>
		/// Gets the enumerator for the children of the <see cref="Multipart"/>.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return children.GetEnumerator ();
		}

		#endregion
	}
}

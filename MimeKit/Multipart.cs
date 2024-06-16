//
// Multipart.cs
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
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;

using MimeKit.IO;
using MimeKit.Text;
using MimeKit.Utils;
using MimeKit.Encodings;

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
	public class Multipart : MimeEntity, IMultipart
	{
		readonly List<MimeEntity> children;
		string preamble, epilogue;

		/// <summary>
		/// Initialize a new instance of the <see cref="Multipart"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is used by <see cref="MimeParser"/>.
		/// </remarks>
		/// <param name="args">Information used by the constructor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		public Multipart (MimeEntityConstructorArgs args) : base (args)
		{
			children = new List<MimeEntity> ();
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="Multipart"/> class.
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
			if (args is null)
				throw new ArgumentNullException (nameof (args));

			foreach (object obj in args) {
				if (obj is null || TryInit (obj))
					continue;

				if (obj is MimeEntity entity) {
					Add (entity);
					continue;
				}

				throw new ArgumentException ("Unknown initialization parameter: " + obj.GetType ());
			}
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="Multipart"/> class.
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
			ContentType.Boundary = GenerateBoundary ();
			children = new List<MimeEntity> ();
			WriteEndBoundary = true;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="Multipart"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="Multipart"/> with a ContentType of multipart/mixed.
		/// </remarks>
		public Multipart () : this ("mixed")
		{
		}

		void CheckDisposed ()
		{
			CheckDisposed (nameof (Multipart));
		}

#if NET5_0_OR_GREATER
		[System.Runtime.CompilerServices.SkipLocalsInit]
#endif
		static string GenerateBoundary ()
		{
#if NET5_0_OR_GREATER
			Span<byte> buffer = stackalloc byte[24];
			Span<byte> digest = stackalloc byte[16];
			Span<char> ascii  = stackalloc char[26];

			RandomNumberGenerator.Fill (digest);

			System.Buffers.Text.Base64.EncodeToUtf8 (digest, buffer, out _, out int length);

			ascii[0] = '=';
			ascii[1] = '-';
			Encoding.ASCII.GetChars (buffer.Slice (0, length), ascii.Slice (2, length));

			return new string (ascii.Slice (0, length + 2));
#else
			var base64 = new Base64Encoder (true);
			var digest = new byte[16];
			var buf = new byte[24];
			int length;

			MimeUtils.GetRandomBytes (digest);

			length = base64.Flush (digest, 0, digest.Length, buf);

			return "=-" + Encoding.ASCII.GetString (buf, 0, length);
#endif
		}

		/// <summary>
		/// Get or set the boundary.
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
				if (value is null)
					throw new ArgumentNullException (nameof (value));

				if (Boundary == value)
					return;

				ContentType.Boundary = value.Trim ();
			}
		}

		internal byte[] RawPreamble {
			get; set;
		}

		/// <summary>
		/// Get or set the preamble.
		/// </summary>
		/// <remarks>
		/// A multipart preamble appears before the first child entity of the
		/// multipart and is typically used only in the top-level multipart
		/// of the message to specify that the message is in MIME format and
		/// therefore requires a MIME compliant email application to render
		/// it correctly.
		/// </remarks>
		/// <value>The preamble.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="Multipart"/> has been disposed.
		/// </exception>
		public string Preamble {
			get {
				CheckDisposed ();

				if (preamble is null && RawPreamble != null)
					preamble = CharsetUtils.ConvertToUnicode (Headers.Options, RawPreamble, 0, RawPreamble.Length);

				return preamble;
			}
			set {
				CheckDisposed ();

				if (Preamble == value)
					return;

				if (value != null) {
					var folded = FoldPreambleOrEpilogue (FormatOptions.Default, value, false);
					RawPreamble = Encoding.UTF8.GetBytes (folded);
					preamble = folded;
				} else {
					RawPreamble = null;
					preamble = null;
				}

				WriteEndBoundary = true;
			}
		}

		internal byte[] RawEpilogue {
			get; set;
		}

		/// <summary>
		/// Get or set the epilogue.
		/// </summary>
		/// <remarks>
		/// A multipart epiloque is the text that appears after the closing boundary
		/// of the multipart and is typically either empty or a single new line
		/// character sequence.
		/// </remarks>
		/// <value>The epilogue.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="Multipart"/> has been disposed.
		/// </exception>
		public string Epilogue {
			get {
				CheckDisposed ();

				if (epilogue is null && RawEpilogue != null) {
					int index = 0;

					// Note: In practice, the RawEpilogue contains the CRLF belonging to the end-boundary, but
					// for sanity, we pretend that it doesn't.
					if ((RawEpilogue.Length > 1 && RawEpilogue[0] == (byte) '\r' && RawEpilogue[1] == (byte) '\n'))
						index += 2;
					else if (RawEpilogue.Length > 1 && RawEpilogue[0] == (byte) '\n')
						index++;

					epilogue = CharsetUtils.ConvertToUnicode (Headers.Options, RawEpilogue, index, RawEpilogue.Length - index);
				}

				return epilogue;
			}
			set {
				CheckDisposed ();

				if (Epilogue == value)
					return;

				if (value != null) {
					var folded = FoldPreambleOrEpilogue (FormatOptions.Default, value, true);
					RawEpilogue = Encoding.UTF8.GetBytes (folded);
					epilogue = null;
				} else {
					RawEpilogue = null;
					epilogue = null;
				}

				WriteEndBoundary = true;
			}
		}

		/// <summary>
		/// Get or set whether the end boundary should be written.
		/// </summary>
		/// <remarks>
		/// Gets or sets whether the end boundary should be written.
		/// </remarks>
		/// <value><c>true</c> if the end boundary should be written; otherwise, <c>false</c>.</value>
		internal bool WriteEndBoundary {
			get; set;
		}

		/// <summary>
		/// Dispatches to the specific visit method for this MIME entity.
		/// </summary>
		/// <remarks>
		/// This default implementation for <see cref="Multipart"/> nodes
		/// calls <see cref="MimeVisitor.VisitMultipart"/>. Override this
		/// method to call into a more specific method on a derived visitor class
		/// of the <see cref="MimeVisitor"/> class. However, it should still
		/// support unknown visitors by calling
		/// <see cref="MimeVisitor.VisitMultipart"/>.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="Multipart"/> has been disposed.
		/// </exception>
		public override void Accept (MimeVisitor visitor)
		{
			if (visitor is null)
				throw new ArgumentNullException (nameof (visitor));

			CheckDisposed ();

			visitor.VisitMultipart (this);
		}

		/// <summary>
		/// Get the preferred message body if it exists.
		/// </summary>
		/// <remarks>
		/// Gets the preferred message body if it exists.
		/// </remarks>
		/// <param name="format">The preferred text format.</param>
		/// <param name="body">The MIME part containing the message body in the preferred text format.</param>
		/// <returns><c>true</c> if the body part is found; otherwise, <c>false</c>.</returns>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="Multipart"/> has been disposed.
		/// </exception>
		public virtual bool TryGetValue (TextFormat format, out TextPart body)
		{
			CheckDisposed ();

			for (int i = 0; i < Count; i++) {
				// Descend into nested multiparts if there are any...
				if (this[i] is Multipart multipart) {
					if (multipart.TryGetValue (format, out body))
						return true;

					// The text body should never come after a multipart.
					break;
				}

				// Look for the first non-attachment text part (realistically, the body text will
				// precede any attachments, but I'm not sure we can rely on that assumption).
				if (this[i] is TextPart text && !text.IsAttachment) {
					if (text.IsFormat (format)) {
						body = text;
						return true;
					}

					// Note: the first text/* part in a multipart/mixed is the text body.
					// If it's not in the format we're looking for, then it doesn't exist.
					break;
				}
			}

			body = null;

			return false;
		}

		internal static string FoldPreambleOrEpilogue (FormatOptions options, string text, bool isEpilogue)
		{
			var builder = new ValueStringBuilder (256);
			int startIndex, wordIndex;
			int lineLength = 0;
			int index = 0;

			if (isEpilogue)
				builder.Append (options.NewLine);

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
					builder.Append (text.AsSpan (startIndex, length));
					lineLength += length;
				}
			}

			if (lineLength > 0)
				builder.Append (options.NewLine);

			return builder.ToString ();
		}

		static void WriteBytes (FormatOptions options, Stream stream, byte[] bytes, bool ensureNewLine, CancellationToken cancellationToken)
		{
			var filter = options.CreateNewLineFilter (ensureNewLine);

			var output = filter.Flush (bytes, 0, bytes.Length, out int index, out int length);

			if (stream is ICancellableStream cancellable) {
				cancellable.Write (output, index, length, cancellationToken);
			} else {
				cancellationToken.ThrowIfCancellationRequested ();
				stream.Write (output, index, length);
			}
		}

		static Task WriteBytesAsync (FormatOptions options, Stream stream, byte[] bytes, bool ensureNewLine, CancellationToken cancellationToken)
		{
			var filter = options.CreateNewLineFilter (ensureNewLine);

			var output = filter.Flush (bytes, 0, bytes.Length, out int index, out int length);

			return stream.WriteAsync (output, index, length, cancellationToken);
		}

		/// <summary>
		/// Prepare the MIME entity for transport using the specified encoding constraints.
		/// </summary>
		/// <remarks>
		/// Prepares the MIME entity for transport using the specified encoding constraints.
		/// </remarks>
		/// <param name="constraint">The encoding constraint.</param>
		/// <param name="maxLineLength">The maximum number of octets allowed per line (not counting the CRLF). Must be between <c>60</c> and <c>998</c> (inclusive).</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="maxLineLength"/> is not between <c>60</c> and <c>998</c> (inclusive).</para>
		/// <para>-or-</para>
		/// <para><paramref name="constraint"/> is not a valid value.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="Multipart"/> has been disposed.
		/// </exception>
		public override void Prepare (EncodingConstraint constraint, int maxLineLength = 78)
		{
			if (maxLineLength < FormatOptions.MinimumLineLength || maxLineLength > FormatOptions.MaximumLineLength)
				throw new ArgumentOutOfRangeException (nameof (maxLineLength));

			CheckDisposed ();

			for (int i = 0; i < children.Count; i++)
				children[i].Prepare (constraint, maxLineLength);
		}

		static FormatOptions GetMultipartSignedFormatOptions (FormatOptions options)
		{
			// don't reformat the headers or content of any children of a multipart/signed
			if (options.International || options.HiddenHeaders.Count > 0) {
				options = options.Clone ();
				options.HiddenHeaders.Clear ();
				options.International = false;
			}

			return options;
		}

		/// <summary>
		/// Write the <see cref="Multipart"/> to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Writes the multipart MIME entity and its subparts to the output stream.
		/// </remarks>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The output stream.</param>
		/// <param name="contentOnly"><c>true</c> if only the content should be written; otherwise, <c>false</c>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="Multipart"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public override void WriteTo (FormatOptions options, Stream stream, bool contentOnly, CancellationToken cancellationToken = default)
		{
			base.WriteTo (options, stream, contentOnly, cancellationToken);

			if (ContentType.IsMimeType ("multipart", "signed"))
				options = GetMultipartSignedFormatOptions (options);

			if (RawPreamble != null && RawPreamble.Length > 0)
				WriteBytes (options, stream, RawPreamble, children.Count > 0 || EnsureNewLine, cancellationToken);

			var boundary = Encoding.ASCII.GetBytes ("--" + Boundary + "--");

			if (stream is ICancellableStream cancellable) {
				for (int i = 0; i < children.Count; i++) {
					var rfc822 = children[i] as MessagePart;
					var multi = children[i] as Multipart;
					var part = children[i] as MimePart;

					cancellable.Write (boundary, 0, boundary.Length - 2, cancellationToken);
					cancellable.Write (options.NewLineBytes, 0, options.NewLineBytes.Length, cancellationToken);
					children[i].WriteTo (options, stream, false, cancellationToken);

					if (rfc822 != null && rfc822.Message != null && rfc822.Message.Body != null) {
						multi = rfc822.Message.Body as Multipart;
						part = rfc822.Message.Body as MimePart;
					}

					if ((part != null && part.Content is null) ||
						(multi != null && !multi.WriteEndBoundary))
						continue;

					cancellable.Write (options.NewLineBytes, 0, options.NewLineBytes.Length, cancellationToken);
				}

				if (!WriteEndBoundary)
					return;

				cancellable.Write (boundary, 0, boundary.Length, cancellationToken);

				if (RawEpilogue is null)
					cancellable.Write (options.NewLineBytes, 0, options.NewLineBytes.Length, cancellationToken);
			} else {
				for (int i = 0; i < children.Count; i++) {
					var rfc822 = children[i] as MessagePart;
					var multi = children[i] as Multipart;
					var part = children[i] as MimePart;

					cancellationToken.ThrowIfCancellationRequested ();
					stream.Write (boundary, 0, boundary.Length - 2);
					stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
					children[i].WriteTo (options, stream, false, cancellationToken);

					if (rfc822 != null && rfc822.Message != null && rfc822.Message.Body != null) {
						multi = rfc822.Message.Body as Multipart;
						part = rfc822.Message.Body as MimePart;
					}

					if ((part != null && part.Content is null) ||
						(multi != null && !multi.WriteEndBoundary))
						continue;

					cancellationToken.ThrowIfCancellationRequested ();
					stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
				}

				if (!WriteEndBoundary)
					return;

				cancellationToken.ThrowIfCancellationRequested ();
				stream.Write (boundary, 0, boundary.Length);

				if (RawEpilogue is null) {
					cancellationToken.ThrowIfCancellationRequested ();
					stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
				}
			}

			if (RawEpilogue != null && RawEpilogue.Length > 0)
				WriteBytes (options, stream, RawEpilogue, EnsureNewLine, cancellationToken);
		}

		/// <summary>
		/// Asynchronously write the <see cref="Multipart"/> to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously writes the multipart MIME entity and its subparts to the output stream.
		/// </remarks>
		/// <returns>An awaitable task.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The output stream.</param>
		/// <param name="contentOnly"><c>true</c> if only the content should be written; otherwise, <c>false</c>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="Multipart"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public override async Task WriteToAsync (FormatOptions options, Stream stream, bool contentOnly, CancellationToken cancellationToken = default)
		{
			await base.WriteToAsync (options, stream, contentOnly, cancellationToken).ConfigureAwait (false);

			if (ContentType.IsMimeType ("multipart", "signed"))
				options = GetMultipartSignedFormatOptions (options);

			if (RawPreamble != null && RawPreamble.Length > 0)
				await WriteBytesAsync (options, stream, RawPreamble, children.Count > 0 || EnsureNewLine, cancellationToken).ConfigureAwait (false);

			var boundary = Encoding.ASCII.GetBytes ("--" + Boundary + "--");

			for (int i = 0; i < children.Count; i++) {
				var rfc822 = children[i] as MessagePart;
				var multi = children[i] as Multipart;
				var part = children[i] as MimePart;

				await stream.WriteAsync (boundary, 0, boundary.Length - 2, cancellationToken).ConfigureAwait (false);
				await stream.WriteAsync (options.NewLineBytes, 0, options.NewLineBytes.Length, cancellationToken).ConfigureAwait (false);
				await children[i].WriteToAsync (options, stream, false, cancellationToken).ConfigureAwait (false);

				if (rfc822 != null && rfc822.Message != null && rfc822.Message.Body != null) {
					multi = rfc822.Message.Body as Multipart;
					part = rfc822.Message.Body as MimePart;
				}

				if ((part != null && part.Content is null) ||
				    (multi != null && !multi.WriteEndBoundary))
					continue;

				await stream.WriteAsync (options.NewLineBytes, 0, options.NewLineBytes.Length, cancellationToken).ConfigureAwait (false);
			}

			if (!WriteEndBoundary)
				return;

			await stream.WriteAsync (boundary, 0, boundary.Length, cancellationToken).ConfigureAwait (false);

			if (RawEpilogue is null)
				await stream.WriteAsync (options.NewLineBytes, 0, options.NewLineBytes.Length, cancellationToken).ConfigureAwait (false);

			if (RawEpilogue != null && RawEpilogue.Length > 0)
				await WriteBytesAsync (options, stream, RawEpilogue, EnsureNewLine, cancellationToken).ConfigureAwait (false);
		}

		#region ICollection implementation

		/// <summary>
		/// Get the number of parts in the multipart.
		/// </summary>
		/// <remarks>
		/// Indicates the number of parts in the multipart.
		/// </remarks>
		/// <value>The number of parts in the multipart.</value>
		public int Count {
			get { return children.Count; }
		}

		/// <summary>
		/// Get a value indicating whether this instance is read only.
		/// </summary>
		/// <remarks>
		/// A <see cref="Multipart"/> is never read-only.
		/// </remarks>
		/// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
		public bool IsReadOnly {
			get { return false; }
		}

		/// <summary>
		/// Add an entity to the multipart.
		/// </summary>
		/// <remarks>
		/// Adds an entity to the multipart without changing the WriteEndBoundary state.
		/// </remarks>
		/// <param name="entity">The MIME entity to add.</param>
		internal void InternalAdd (MimeEntity entity)
		{
			children.Add (entity);
		}

		/// <summary>
		/// Add an entity to the multipart.
		/// </summary>
		/// <remarks>
		/// Adds the specified part to the multipart.
		/// </remarks>
		/// <param name="entity">The MIME entity to add.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="entity"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="Multipart"/> has been disposed.
		/// </exception>
		public void Add (MimeEntity entity)
		{
			if (entity is null)
				throw new ArgumentNullException (nameof (entity));

			CheckDisposed ();

			WriteEndBoundary = true;
			children.Add (entity);
		}

		/// <summary>
		/// Clear a multipart.
		/// </summary>
		/// <remarks>
		/// Removes all of the entities within the multipart.
		/// </remarks>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="Multipart"/> has been disposed.
		/// </exception>
		public void Clear ()
		{
			Clear (false);
		}

		/// <summary>
		/// Clear a multipart.
		/// </summary>
		/// <remarks>
		/// Removes all of the entities within the multipart, optionally disposing them in the process.
		/// </remarks>
		/// <param name="dispose"><c>true</c> if all of the child entities of the multipart should be disposed; otherwise, <c>false</c>.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="Multipart"/> has been disposed.
		/// </exception>
		public void Clear (bool dispose)
		{
			CheckDisposed ();

			if (dispose) {
				for (int i = 0; i < children.Count; i++)
					children[i].Dispose ();
			}

			WriteEndBoundary = true;
			children.Clear ();
		}

		/// <summary>
		/// Check if the <see cref="Multipart"/> contains the specified entity.
		/// </summary>
		/// <remarks>
		/// Determines whether or not the multipart contains the specified entity.
		/// </remarks>
		/// <returns><value>true</value> if the specified entity exists;
		/// otherwise <value>false</value>.</returns>
		/// <param name="entity">The entity to check for.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="entity"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="Multipart"/> has been disposed.
		/// </exception>
		public bool Contains (MimeEntity entity)
		{
			if (entity is null)
				throw new ArgumentNullException (nameof (entity));

			CheckDisposed ();

			return children.Contains (entity);
		}

		/// <summary>
		/// Copy all of the entities in the <see cref="Multipart"/> to the specified array.
		/// </summary>
		/// <remarks>
		/// Copies all of the entities within the <see cref="Multipart"/> into the array,
		/// starting at the specified array index.
		/// </remarks>
		/// <param name="array">The array to copy the child entities to.</param>
		/// <param name="arrayIndex">The index into the array.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="array"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex"/> is out of range.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="Multipart"/> has been disposed.
		/// </exception>
		public void CopyTo (MimeEntity[] array, int arrayIndex)
		{
			CheckDisposed ();
			children.CopyTo (array, arrayIndex);
		}

		/// <summary>
		/// Remove an entity from the multipart.
		/// </summary>
		/// <remarks>
		/// Removes the specified entity if it exists within the multipart.
		/// </remarks>
		/// <returns><value>true</value> if the part was removed; otherwise <value>false</value>.</returns>
		/// <param name="entity">The MIME entity to remove.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="entity"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="Multipart"/> has been disposed.
		/// </exception>
		public bool Remove (MimeEntity entity)
		{
			if (entity is null)
				throw new ArgumentNullException (nameof (entity));

			CheckDisposed ();

			if (!children.Remove (entity))
				return false;

			WriteEndBoundary = true;

			return true;
		}

		#endregion

		#region IList implementation

		/// <summary>
		/// Get the index of an entity.
		/// </summary>
		/// <remarks>
		/// Finds the index of the specified entity, if it exists.
		/// </remarks>
		/// <returns>The index of the specified entity if found; otherwise <c>-1</c>.</returns>
		/// <param name="entity">The MIME entity.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="entity"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="Multipart"/> has been disposed.
		/// </exception>
		public int IndexOf (MimeEntity entity)
		{
			if (entity is null)
				throw new ArgumentNullException (nameof (entity));

			CheckDisposed ();

			return children.IndexOf (entity);
		}

		/// <summary>
		/// Insert an entity into the <see cref="Multipart"/> at the specified index.
		/// </summary>
		/// <remarks>
		/// Inserts the part into the multipart at the specified index.
		/// </remarks>
		/// <param name="index">The index.</param>
		/// <param name="entity">The MIME entity.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="entity"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="Multipart"/> has been disposed.
		/// </exception>
		public void Insert (int index, MimeEntity entity)
		{
			if (index < 0 || index > children.Count)
				throw new ArgumentOutOfRangeException (nameof (index));

			if (entity is null)
				throw new ArgumentNullException (nameof (entity));

			CheckDisposed ();

			children.Insert (index, entity);
			WriteEndBoundary = true;
		}

		/// <summary>
		/// Remove an entity from the <see cref="Multipart"/> at the specified index.
		/// </summary>
		/// <remarks>
		/// <para>Removes the entity at the specified index.</para>
		/// <note type="note">It is the responsibility of the caller to dispose the entity at the specified <paramref name="index"/>.</note>
		/// </remarks>
		/// <param name="index">The index.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="Multipart"/> has been disposed.
		/// </exception>
		public void RemoveAt (int index)
		{
			CheckDisposed ();
			children.RemoveAt (index);
			WriteEndBoundary = true;
		}

		/// <summary>
		/// Get or set the <see cref="MimeEntity"/> at the specified index.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the <see cref="MimeEntity"/> at the specified index.</para>
		/// <note type="note">It is the responsibility of the caller to dispose the original entity at the specified <paramref name="index"/>.</note>
		/// </remarks>
		/// <value>The entity at the specified index.</value>
		/// <param name="index">The index.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="Multipart"/> has been disposed.
		/// </exception>
		public MimeEntity this[int index] {
			get {
				CheckDisposed ();

				return children[index];
			}
			set {
				if (value is null)
					throw new ArgumentNullException (nameof (value));

				CheckDisposed ();

				WriteEndBoundary = true;
				children[index] = value;
			}
		}

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Get the enumerator for the children of the <see cref="Multipart"/>.
		/// </summary>
		/// <remarks>
		/// Gets the enumerator for the children of the <see cref="Multipart"/>.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="Multipart"/> has been disposed.
		/// </exception>
		public IEnumerator<MimeEntity> GetEnumerator ()
		{
			CheckDisposed ();
			return children.GetEnumerator ();
		}

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Get the enumerator for the children of the <see cref="Multipart"/>.
		/// </summary>
		/// <remarks>
		/// Gets the enumerator for the children of the <see cref="Multipart"/>.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="Multipart"/> has been disposed.
		/// </exception>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			CheckDisposed ();
			return children.GetEnumerator ();
		}

		#endregion

		/// <summary>
		/// Release the unmanaged resources used by the <see cref="Multipart"/> and
		/// optionally releases the managed resources.
		/// </summary>
		/// <remarks>
		/// Releases the unmanaged resources used by the <see cref="Multipart"/> and
		/// optionally releases the managed resources.
		/// </remarks>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
		/// <c>false</c> to release only the unmanaged resources.</param>
		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				for (int i = 0; i < children.Count; i++)
					children[i].Dispose ();
			}

			base.Dispose (disposing);
		}
	}
}

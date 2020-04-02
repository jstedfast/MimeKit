﻿//
// MimePart.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 Xamarin Inc. (www.xamarin.com)
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
using System.Threading.Tasks;

using MD5 = System.Security.Cryptography.MD5;

using MimeKit.IO.Filters;
using MimeKit.Encodings;
using MimeKit.Utils;
using MimeKit.IO;

namespace MimeKit {
	/// <summary>
	/// A leaf-node MIME part that contains content such as the message body text or an attachment.
	/// </summary>
	/// <remarks>
	/// A leaf-node MIME part that contains content such as the message body text or an attachment.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\AttachmentExamples.cs" region="SaveAttachments" />
	/// </example>
	public class MimePart : MimeEntity
	{
		static readonly string[] ContentTransferEncodings = {
			null, "7bit", "8bit", "binary", "base64", "quoted-printable", "x-uuencode"
		};
		ContentEncoding encoding;
		string md5sum;
		int? duration;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MimePart"/> class
		/// based on the <see cref="MimeEntityConstructorArgs"/>.
		/// </summary>
		/// <remarks>
		/// This constructor is used by <see cref="MimeKit.MimeParser"/>.
		/// </remarks>
		/// <param name="args">Information used by the constructor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		public MimePart (MimeEntityConstructorArgs args) : base (args)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MimePart"/> class
		/// with the specified media type and subtype.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimePart"/> with the specified media type and subtype.
		/// </remarks>
		/// <param name="mediaType">The media type.</param>
		/// <param name="mediaSubtype">The media subtype.</param>
		/// <param name="args">An array of initialization parameters: headers and part content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="mediaType"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="mediaSubtype"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="args"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="args"/> contains more than one <see cref="MimeKit.IMimeContent"/> or
		/// <see cref="System.IO.Stream"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="args"/> contains one or more arguments of an unknown type.</para>
		/// </exception>
		public MimePart (string mediaType, string mediaSubtype, params object[] args) : this (mediaType, mediaSubtype)
		{
			if (args == null)
				throw new ArgumentNullException (nameof (args));

			IMimeContent content = null;

			foreach (object obj in args) {
				if (obj == null || TryInit (obj))
					continue;

				var co = obj as IMimeContent;
				if (co != null) {
					if (content != null)
						throw new ArgumentException ("IMimeContent should not be specified more than once.");

					content = co;
					continue;
				}

				var stream = obj as Stream;
				if (stream != null) {
					if (content != null)
						throw new ArgumentException ("Stream (used as content) should not be specified more than once.");

					// Use default as specified by ContentObject ctor when building a new MimePart.
					content = new MimeContent (stream);
					continue;
				}

				throw new ArgumentException ("Unknown initialization parameter: " + obj.GetType ());
			}

			if (content != null)
				Content = content;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MimePart"/> class
		/// with the specified media type and subtype.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimePart"/> with the specified media type and subtype.
		/// </remarks>
		/// <param name="mediaType">The media type.</param>
		/// <param name="mediaSubtype">The media subtype.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="mediaType"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="mediaSubtype"/> is <c>null</c>.</para>
		/// </exception>
		public MimePart (string mediaType, string mediaSubtype) : base (mediaType, mediaSubtype)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MimePart"/> class
		/// with the specified content type.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimePart"/> with the specified Content-Type value.
		/// </remarks>
		/// <param name="contentType">The content type.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="contentType"/> is <c>null</c>.
		/// </exception>
		public MimePart (ContentType contentType) : base (contentType)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MimePart"/> class
		/// with the specified content type.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimePart"/> with the specified Content-Type value.
		/// </remarks>
		/// <param name="contentType">The content type.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="contentType"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// <paramref name="contentType"/> could not be parsed.
		/// </exception>
		public MimePart (string contentType) : base (ContentType.Parse (contentType))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MimePart"/> class
		/// with the default Content-Type of application/octet-stream.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimePart"/> with a Content-Type of application/octet-stream.
		/// </remarks>
		public MimePart () : this ("application", "octet-stream")
		{
		}

		/// <summary>
		/// Gets or sets the duration of the content if available.
		/// </summary>
		/// <remarks>
		/// <para>The Content-Duration header specifies duration of timed media,
		/// such as audio or video, in seconds.</para>
		/// </remarks>
		/// <value>The duration of the content.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is negative.
		/// </exception>
		public int? ContentDuration {
			get { return duration; }
			set {
				if (duration == value)
					return;

				if (value.HasValue && value.Value < 0)
					throw new ArgumentOutOfRangeException (nameof (value));

				duration = value;

				if (value.HasValue)
					SetHeader ("Content-Duration", value.Value.ToString ());
				else
					RemoveHeader ("Content-Duration");
			}
		}

		/// <summary>
		/// Gets or sets the md5sum of the content.
		/// </summary>
		/// <remarks>
		/// <para>The Content-MD5 header specifies the base64-encoded MD5 checksum of the content
		/// in its canonical format.</para>
		/// <para>For more information, see <a href="https://tools.ietf.org/html/rfc1864">rfc1864</a>.</para>
		/// </remarks>
		/// <value>The md5sum of the content.</value>
		public string ContentMd5 {
			get { return md5sum; }
			set {
				if (md5sum == value)
					return;

				md5sum = value != null ? value.Trim () : null;

				if (value != null)
					SetHeader ("Content-Md5", md5sum);
				else
					RemoveHeader ("Content-Md5");
			}
		}

		/// <summary>
		/// Gets or sets the content transfer encoding.
		/// </summary>
		/// <remarks>
		/// The Content-Transfer-Encoding header specifies an auxiliary encoding
		/// that was applied to the content in order to allow it to pass through
		/// mail transport mechanisms (such as SMTP) which may have limitations
		/// in the byte ranges that it accepts. For example, many SMTP servers
		/// do not accept data outside of the 7-bit ASCII range and so sending
		/// binary attachments or even non-English text is not possible without
		/// applying an encoding such as base64 or quoted-printable.
		/// </remarks>
		/// <value>The content transfer encoding.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is not a valid content encoding.
		/// </exception>
		public ContentEncoding ContentTransferEncoding {
			get { return encoding; }
			set {
				if (encoding == value)
					return;

				int index = (int) value;

				if (index < 0 || index >= ContentTransferEncodings.Length)
					throw new ArgumentOutOfRangeException (nameof (value));

				var text = ContentTransferEncodings[index];

				encoding = value;

				if (text != null)
					SetHeader ("Content-Transfer-Encoding", text);
				else
					RemoveHeader ("Content-Transfer-Encoding");
			}
		}

		/// <summary>
		/// Gets or sets the name of the file.
		/// </summary>
		/// <remarks>
		/// <para>First checks for the "filename" parameter on the Content-Disposition header. If
		/// that does not exist, then the "name" parameter on the Content-Type header is used.</para>
		/// <para>When setting the filename, both the "filename" parameter on the Content-Disposition
		/// header and the "name" parameter on the Content-Type header are set.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\AttachmentExamples.cs" region="SaveAttachments" />
		/// </example>
		/// <value>The name of the file.</value>
		public string FileName {
			get {
				string filename = null;

				if (ContentDisposition != null)
					filename = ContentDisposition.FileName;

				if (filename == null)
					filename = ContentType.Name;

				return filename != null ? filename.Trim () : null;
			}
			set {
				if (value != null) {
					if (ContentDisposition == null)
						ContentDisposition = new ContentDisposition (ContentDisposition.Attachment);
					ContentDisposition.FileName = value;
				} else if (ContentDisposition != null) {
					ContentDisposition.FileName = value;
				}

				ContentType.Name = value;
			}
		}

		/// <summary>
		/// Gets or sets the MIME content.
		/// </summary>
		/// <remarks>
		/// Gets or sets the MIME content.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\AttachmentExamples.cs" region="SaveAttachments" />
		/// </example>
		/// <value>The MIME content.</value>
		public IMimeContent Content {
			get; set;
		}

		/// <summary>
		/// Gets or sets the MIME content.
		/// </summary>
		/// <remarks>
		/// Gets or sets the MIME content.
		/// </remarks>
		/// <value>The MIME content.</value>
		[Obsolete ("Use the Content property instead.")]
		public IMimeContent ContentObject {
			get { return Content; }
			set { Content = value; }
		}

		/// <summary>
		/// Dispatches to the specific visit method for this MIME entity.
		/// </summary>
		/// <remarks>
		/// This default implementation for <see cref="MimeKit.MimePart"/> nodes
		/// calls <see cref="MimeKit.MimeVisitor.VisitMimePart"/>. Override this
		/// method to call into a more specific method on a derived visitor class
		/// of the <see cref="MimeKit.MimeVisitor"/> class. However, it should still
		/// support unknown visitors by calling
		/// <see cref="MimeKit.MimeVisitor.VisitMimePart"/>.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <c>null</c>.
		/// </exception>
		public override void Accept (MimeVisitor visitor)
		{
			if (visitor == null)
				throw new ArgumentNullException (nameof (visitor));

			visitor.VisitMimePart (this);
		}

		/// <summary>
		/// Calculates the most efficient content encoding given the specified constraint.
		/// </summary>
		/// <remarks>
		/// If no <see cref="Content"/> is set, <see cref="ContentEncoding.SevenBit"/> will be returned.
		/// </remarks>
		/// <returns>The most efficient content encoding.</returns>
		/// <param name="constraint">The encoding constraint.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="constraint"/> is not a valid value.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public ContentEncoding GetBestEncoding (EncodingConstraint constraint, CancellationToken cancellationToken = default (CancellationToken))
		{
			return GetBestEncoding (constraint, 78, cancellationToken);
		}

		/// <summary>
		/// Calculates the most efficient content encoding given the specified constraint.
		/// </summary>
		/// <remarks>
		/// If no <see cref="Content"/> is set, <see cref="ContentEncoding.SevenBit"/> will be returned.
		/// </remarks>
		/// <returns>The most efficient content encoding.</returns>
		/// <param name="constraint">The encoding constraint.</param>
		/// <param name="maxLineLength">The maximum allowable length for a line (not counting the CRLF). Must be between <c>72</c> and <c>998</c> (inclusive).</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="maxLineLength"/> is not between <c>72</c> and <c>998</c> (inclusive).</para>
		/// <para>-or-</para>
		/// <para><paramref name="constraint"/> is not a valid value.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public ContentEncoding GetBestEncoding (EncodingConstraint constraint, int maxLineLength, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (Content == null)
				return ContentEncoding.SevenBit;

			using (var measure = new MeasuringStream ()) {
				using (var filtered = new FilteredStream (measure)) {
					var filter = new BestEncodingFilter ();

					filtered.Add (filter);
					Content.DecodeTo (filtered, cancellationToken);
					filtered.Flush ();

					return filter.GetBestEncoding (constraint, maxLineLength);
				}
			}
		}

		/// <summary>
		/// Computes the MD5 checksum of the content.
		/// </summary>
		/// <remarks>
		/// Computes the MD5 checksum of the MIME content in its canonical
		/// format and then base64-encodes the result.
		/// </remarks>
		/// <returns>The md5sum of the content.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="Content"/> is <c>null</c>.
		/// </exception>
		public string ComputeContentMd5 ()
		{
			if (Content == null)
				throw new InvalidOperationException ("Cannot compute Md5 checksum without a ContentObject.");

			using (var stream = Content.Open ()) {
				byte[] checksum;

				using (var filtered = new FilteredStream (stream)) {
					if (ContentType.IsMimeType ("text", "*"))
						filtered.Add (new Unix2DosFilter ());

					using (var md5 = MD5.Create ())
						checksum = md5.ComputeHash (filtered);
				}

				var base64 = new Base64Encoder (true);
				var digest = new byte[base64.EstimateOutputLength (checksum.Length)];
				int n = base64.Flush (checksum, 0, checksum.Length, digest);

				return Encoding.ASCII.GetString (digest, 0, n);
			}
		}

		static bool IsNullOrWhiteSpace (string value)
		{
			if (string.IsNullOrEmpty (value))
				return true;

			for (int i = 0; i < value.Length; i++) {
				if (!char.IsWhiteSpace (value[i]))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Verifies the Content-Md5 value against an independently computed md5sum.
		/// </summary>
		/// <remarks>
		/// Computes the MD5 checksum of the MIME content and compares it with the
		/// value in the Content-MD5 header, returning <c>true</c> if and only if
		/// the values match.
		/// </remarks>
		/// <returns><c>true</c>, if content MD5 checksum was verified, <c>false</c> otherwise.</returns>
		public bool VerifyContentMd5 ()
		{
			if (IsNullOrWhiteSpace (md5sum) || Content == null)
				return false;

			return md5sum == ComputeContentMd5 ();
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
		public override void Prepare (EncodingConstraint constraint, int maxLineLength = 78)
		{
			if (maxLineLength < FormatOptions.MinimumLineLength || maxLineLength > FormatOptions.MaximumLineLength)
				throw new ArgumentOutOfRangeException (nameof (maxLineLength));

			switch (ContentTransferEncoding) {
			case ContentEncoding.QuotedPrintable:
			case ContentEncoding.UUEncode:
			case ContentEncoding.Base64:
				// these are all safe no matter what the constraints are
				return;
			case ContentEncoding.Binary:
				if (constraint == EncodingConstraint.None) {
					// no need to re-encode anything
					return;
				}
				break;
			}

			var best = GetBestEncoding (constraint, maxLineLength);

			if (ContentTransferEncoding == ContentEncoding.Default && best == ContentEncoding.SevenBit)
				return;

			ContentTransferEncoding = best;
		}

		/// <summary>
		/// Writes the <see cref="MimeKit.MimePart"/> to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Writes the MIME part to the output stream.
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
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public override void WriteTo (FormatOptions options, Stream stream, bool contentOnly, CancellationToken cancellationToken = default (CancellationToken))
		{
			base.WriteTo (options, stream, contentOnly, cancellationToken);

			if (Content == null)
				return;

			var cancellable = stream as ICancellableStream;

			if (Content.Encoding != encoding) {
				if (encoding == ContentEncoding.UUEncode) {
					var begin = string.Format ("begin 0644 {0}", FileName ?? "unknown");
					var buffer = Encoding.UTF8.GetBytes (begin);

					if (cancellable != null) {
						cancellable.Write (buffer, 0, buffer.Length, cancellationToken);
						cancellable.Write (options.NewLineBytes, 0, options.NewLineBytes.Length, cancellationToken);
					} else {
						cancellationToken.ThrowIfCancellationRequested ();
						stream.Write (buffer, 0, buffer.Length);
						stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
					}
				}

				// transcode the content into the desired Content-Transfer-Encoding
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (EncoderFilter.Create (encoding));

					if (encoding != ContentEncoding.Binary)
						filtered.Add (options.CreateNewLineFilter (EnsureNewLine));

					Content.DecodeTo (filtered, cancellationToken);
					filtered.Flush (cancellationToken);
				}

				if (encoding == ContentEncoding.UUEncode) {
					var buffer = Encoding.ASCII.GetBytes ("end");

					if (cancellable != null) {
						cancellable.Write (buffer, 0, buffer.Length, cancellationToken);
						cancellable.Write (options.NewLineBytes, 0, options.NewLineBytes.Length, cancellationToken);
					} else {
						cancellationToken.ThrowIfCancellationRequested ();
						stream.Write (buffer, 0, buffer.Length);
						stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
					}
				}
			} else if (encoding != ContentEncoding.Binary) {
				using (var filtered = new FilteredStream (stream)) {
					// Note: if we are writing the top-level MimePart, make sure it ends with a new-line so that
					// MimeMessage.WriteTo() *always* ends with a new-line.
					filtered.Add (options.CreateNewLineFilter (EnsureNewLine));
					Content.WriteTo (filtered, cancellationToken);
					filtered.Flush (cancellationToken);
				}
			} else {
				Content.WriteTo (stream, cancellationToken);
			}
		}

		/// <summary>
		/// Asynchronously writes the <see cref="MimeKit.MimePart"/> to the specified output stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously writes the MIME part to the output stream.
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
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public override async Task WriteToAsync (FormatOptions options, Stream stream, bool contentOnly, CancellationToken cancellationToken = default (CancellationToken))
		{
			await base.WriteToAsync (options, stream, contentOnly, cancellationToken).ConfigureAwait (false);

			if (Content == null)
				return;

			if (Content.Encoding != encoding) {
				if (encoding == ContentEncoding.UUEncode) {
					var begin = string.Format ("begin 0644 {0}", FileName ?? "unknown");
					var buffer = Encoding.UTF8.GetBytes (begin);

					await stream.WriteAsync (buffer, 0, buffer.Length, cancellationToken).ConfigureAwait (false);
					await stream.WriteAsync (options.NewLineBytes, 0, options.NewLineBytes.Length, cancellationToken).ConfigureAwait (false);
				}

				// transcode the content into the desired Content-Transfer-Encoding
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (EncoderFilter.Create (encoding));

					if (encoding != ContentEncoding.Binary)
						filtered.Add (options.CreateNewLineFilter (EnsureNewLine));

					await Content.DecodeToAsync (filtered, cancellationToken).ConfigureAwait (false);
					await filtered.FlushAsync (cancellationToken).ConfigureAwait (false);
				}

				if (encoding == ContentEncoding.UUEncode) {
					var buffer = Encoding.ASCII.GetBytes ("end");

					await stream.WriteAsync (buffer, 0, buffer.Length, cancellationToken).ConfigureAwait (false);
					await stream.WriteAsync (options.NewLineBytes, 0, options.NewLineBytes.Length, cancellationToken).ConfigureAwait (false);
				}
			} else if (encoding != ContentEncoding.Binary) {
				using (var filtered = new FilteredStream (stream)) {
					// Note: if we are writing the top-level MimePart, make sure it ends with a new-line so that
					// MimeMessage.WriteTo() *always* ends with a new-line.
					filtered.Add (options.CreateNewLineFilter (EnsureNewLine));
					await Content.WriteToAsync (filtered, cancellationToken).ConfigureAwait (false);
					await filtered.FlushAsync (cancellationToken).ConfigureAwait (false);
				}
			} else {
				await Content.WriteToAsync (stream, cancellationToken).ConfigureAwait (false);
			}
		}

		/// <summary>
		/// Called when the headers change in some way.
		/// </summary>
		/// <remarks>
		/// Updates the <see cref="ContentTransferEncoding"/>, <see cref="ContentDuration"/>,
		/// and <see cref="ContentMd5"/> properties if the corresponding headers have changed.
		/// </remarks>
		/// <param name="action">The type of change.</param>
		/// <param name="header">The header being added, changed or removed.</param>
		protected override void OnHeadersChanged (HeaderListChangedAction action, Header header)
		{
			int value;

			base.OnHeadersChanged (action, header);

			switch (action) {
			case HeaderListChangedAction.Added:
			case HeaderListChangedAction.Changed:
				switch (header.Id) {
				case HeaderId.ContentTransferEncoding:
					MimeUtils.TryParse (header.Value, out encoding);
					break;
				case HeaderId.ContentDuration:
					if (int.TryParse (header.Value, out value))
						duration = value;
					else
						duration = null;
					break;
				case HeaderId.ContentMd5:
					md5sum = header.Value.Trim ();
					break;
				}
				break;
			case HeaderListChangedAction.Removed:
				switch (header.Id) {
				case HeaderId.ContentTransferEncoding:
					encoding = ContentEncoding.Default;
					break;
				case HeaderId.ContentDuration:
					duration = null;
					break;
				case HeaderId.ContentMd5:
					md5sum = null;
					break;
				}
				break;
			case HeaderListChangedAction.Cleared:
				encoding = ContentEncoding.Default;
				duration = null;
				md5sum = null;
				break;
			default:
				throw new ArgumentOutOfRangeException (nameof (action));
			}
		}
	}
}

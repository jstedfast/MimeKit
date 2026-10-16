//
// MimeWriter.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
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

using MimeKit.IO;

namespace MimeKit {
	/// <summary>
	/// Writes MIME messages and entities to a stream.
	/// </summary>
	/// <remarks>
	/// <para>Nested multiparts and message parts are written using an explicit stack rather than by recursion
	/// so that the nesting depth of a message cannot exhaust the call stack.</para>
	/// <para>Only multiparts need a frame on the stack because they have more to write (the remaining children,
	/// the end boundary and the epilogue) once a child has been written. The embedded message is the last thing
	/// that a message part writes and the body is the last thing that a message writes, so the writer simply
	/// continues with them instead.</para>
	/// </remarks>
	sealed class MimeWriter
	{
		struct MultipartFrame
		{
			public Multipart Multipart;
			public FormatOptions Options;
			public byte[] DefaultBoundary;
			public bool EnsureNewLine;
			public int Index;
		}

		readonly CancellationToken cancellationToken;
		readonly Stream stream;
		MultipartFrame[]? frames;
		int depth;

		MimeWriter (Stream stream, CancellationToken cancellationToken)
		{
			this.cancellationToken = cancellationToken;
			this.stream = stream;
		}

		/// <summary>
		/// Write a message to the specified stream.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The output stream.</param>
		/// <param name="headersOnly"><see langword="true" /> if only the headers should be written; otherwise, <see langword="false" />.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		public static void Write (MimeMessage message, FormatOptions options, Stream stream, bool headersOnly, CancellationToken cancellationToken)
		{
			var writer = new MimeWriter (stream, cancellationToken);

			message.WriteHeaders (options, stream, cancellationToken);

			var body = GetMessageBody (message, options, headersOnly, out bool ensureNewLine);

			if (body != null)
				writer.Write (body, options, true, ensureNewLine);
		}

		/// <summary>
		/// Asynchronously write a message to the specified stream.
		/// </summary>
		/// <returns>An awaitable task.</returns>
		/// <param name="message">The message.</param>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The output stream.</param>
		/// <param name="headersOnly"><see langword="true" /> if only the headers should be written; otherwise, <see langword="false" />.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		public static async Task WriteAsync (MimeMessage message, FormatOptions options, Stream stream, bool headersOnly, CancellationToken cancellationToken)
		{
			var writer = new MimeWriter (stream, cancellationToken);

			await message.WriteHeadersAsync (options, stream, cancellationToken).ConfigureAwait (false);

			var body = GetMessageBody (message, options, headersOnly, out bool ensureNewLine);

			if (body != null)
				await writer.WriteAsync (body, options, true, ensureNewLine).ConfigureAwait (false);
		}

		/// <summary>
		/// Write an entity to the specified stream.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The output stream.</param>
		/// <param name="contentOnly"><see langword="true" /> if only the content should be written; otherwise, <see langword="false" />.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		public static void Write (MimeEntity entity, FormatOptions options, Stream stream, bool contentOnly, CancellationToken cancellationToken)
		{
			var writer = new MimeWriter (stream, cancellationToken);

			writer.Write (entity, options, contentOnly, entity.EnsureNewLine);
		}

		/// <summary>
		/// Asynchronously write an entity to the specified stream.
		/// </summary>
		/// <returns>An awaitable task.</returns>
		/// <param name="entity">The entity.</param>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The output stream.</param>
		/// <param name="contentOnly"><see langword="true" /> if only the content should be written; otherwise, <see langword="false" />.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		public static Task WriteAsync (MimeEntity entity, FormatOptions options, Stream stream, bool contentOnly, CancellationToken cancellationToken)
		{
			var writer = new MimeWriter (stream, cancellationToken);

			return writer.WriteAsync (entity, options, contentOnly, entity.EnsureNewLine);
		}

		// Gets the body of the message that should be written after the message headers (if any) and whether it must end with a new-line.
		static MimeEntity? GetMessageBody (MimeMessage message, FormatOptions options, bool headersOnly, out bool ensureNewLine)
		{
			if (headersOnly || message.Body is null) {
				ensureNewLine = false;
				return null;
			}

			ensureNewLine = message.compliance == RfcComplianceMode.Strict || options.EnsureNewLine;

			return message.Body;
		}

		// Note: Only ensure that the embedded message ends with a new-line if the message part is supposed to end with one.
		static FormatOptions GetEmbeddedMessageFormatOptions (FormatOptions options, bool ensureNewLine)
		{
			if (options.EnsureNewLine != ensureNewLine) {
				options = options.Clone ();
				options.EnsureNewLine = ensureNewLine;
			}

			return options;
		}

		// Get the formatting options to use for the content of the multipart (its preamble, children and epilogue).
		static FormatOptions GetMultipartContentFormatOptions (Multipart multipart, FormatOptions options)
		{
			// don't reformat the headers or content of any children of a multipart/signed
			if (multipart.ContentType.IsMimeType ("multipart", "signed") && (options.International || options.HiddenHeaders.Count > 0)) {
				options = options.Clone ();
				options.HiddenHeaders.Clear ();
				options.International = false;
			}

			return options;
		}

		static byte[] GetDefaultBoundary (Multipart multipart, FormatOptions options)
		{
			return Encoding.ASCII.GetBytes ("--" + multipart.Boundary + options.NewLine);
		}

		static byte[] GetDefaultEndBoundary (Multipart multipart, FormatOptions options)
		{
			// Note: The generated end boundary marker only includes a new-line sequence if there is no epilogue.
			return Encoding.ASCII.GetBytes (string.Concat ("--", multipart.Boundary, "--", multipart.RawEpilogue is null ? options.NewLine : string.Empty));
		}

		// Check whether the new-line sequence that precedes a boundary marker needs to be written after the specified child.
		//
		// Per rfc2046, the new-line sequence that precedes a boundary marker belongs to the boundary marker and not to the
		// content of the preceding child, so it must be written before the boundary marker unless the child's serialized
		// form already ends with it (see MimeEntity.EndsWithDelimiterNewLine).
		static bool NeedsNewLineAfter (Multipart multipart, int index)
		{
			return index >= 0 && !multipart[index].EndsWithDelimiterNewLine;
		}

		static bool IsMultipartComplete (Multipart multipart, int index)
		{
			return index >= multipart.Count;
		}

		void PushMultipart (Multipart multipart, FormatOptions options, bool ensureNewLine)
		{
			if (frames is null)
				frames = new MultipartFrame[8];
			else if (depth == frames.Length)
				Array.Resize (ref frames, frames.Length * 2);

			ref var frame = ref frames[depth++];
			frame.Multipart = multipart;
			frame.Options = options;
			frame.DefaultBoundary = GetDefaultBoundary (multipart, options);
			frame.EnsureNewLine = ensureNewLine;
			frame.Index = 0;
		}

		void PopMultipart ()
		{
			frames![--depth] = default;
		}

		void Write (byte[] buffer, int offset, int count)
		{
			if (stream is ICancellableStream cancellable) {
				cancellable.Write (buffer, offset, count, cancellationToken);
			} else {
				cancellationToken.ThrowIfCancellationRequested ();
				stream.Write (buffer, offset, count);
			}
		}

		void Write (byte[] buffer)
		{
			Write (buffer, 0, buffer.Length);
		}

		// Write the bytes, converting new-lines to the requested format.
		void WriteBytes (FormatOptions options, byte[] bytes, bool ensureNewLine)
		{
			var filter = options.CreateNewLineFilter (ensureNewLine);
			var output = filter.Flush (bytes, 0, bytes.Length, out int index, out int length);

			Write (output, index, length);
		}

		// Writes a raw boundary marker (which may consist of multiple lines in the case of a "double boundary").
		//
		// Normally, the raw boundary is written out byte-for-byte exactly as it was parsed. However, when verifying
		// a signature, the content must be canonicalized to CRLF (options.NewLineFormat is always Dos in that case),
		// so any bare LF gets converted to CRLF. Runs of bytes that are already CRLF-terminated are written as-is.
		void WriteRawBoundary (FormatOptions options, byte[] rawBoundary)
		{
			int startIndex = 0;

			if (options.VerifyingSignature) {
				int index = 0, n;

				while ((n = rawBoundary.AsSpan (index).IndexOf ((byte) '\n')) != -1) {
					index += n;

					if (index == 0 || rawBoundary[index - 1] != (byte) '\r') {
						Write (rawBoundary, startIndex, index - startIndex);
						Write (options.NewLineBytes);
						startIndex = index + 1;
					}

					index++;
				}
			}

			if (startIndex < rawBoundary.Length)
				Write (rawBoundary, startIndex, rawBoundary.Length - startIndex);
		}

		void WritePreamble (Multipart multipart, FormatOptions options, bool ensureNewLine)
		{
			if (multipart.RawPreamble != null && multipart.RawPreamble.Length > 0)
				WriteBytes (options, multipart.RawPreamble, multipart.Count > 0 || ensureNewLine);
		}

		// Write the boundary marker (and the new-line sequence that precedes it, if needed) for the specified child.
		void WriteBoundary (in MultipartFrame frame, int index)
		{
			if (NeedsNewLineAfter (frame.Multipart, index - 1))
				Write (frame.Options.NewLineBytes);

			if (frame.Multipart.rawBoundaries?[index] is byte[] rawBoundary)
				WriteRawBoundary (frame.Options, rawBoundary);
			else
				Write (frame.DefaultBoundary);
		}

		// Write everything that follows the last child: the end boundary and the epilogue.
		void WriteEnd (in MultipartFrame frame)
		{
			var multipart = frame.Multipart;
			var options = frame.Options;

			if (NeedsNewLineAfter (multipart, multipart.Count - 1) && (!multipart.IsTruncated || multipart.RawTrailingBoundary != null))
				Write (options.NewLineBytes);

			if (multipart.IsTruncated) {
				// The multipart was truncated, so there is no end boundary (or epilogue) to write. There may, however,
				// have been a boundary marker at the end of the stream that was not followed by a MIME part.
				if (multipart.RawTrailingBoundary != null) {
					WriteRawBoundary (options, multipart.RawTrailingBoundary);

					if (frame.EnsureNewLine && !Multipart.EndsWithNewLine (multipart.RawTrailingBoundary))
						Write (options.NewLineBytes);
				}

				return;
			}

			if (multipart.RawEndBoundary != null)
				WriteRawBoundary (options, multipart.RawEndBoundary);
			else
				Write (GetDefaultEndBoundary (multipart, options));

			if (multipart.RawEpilogue != null && multipart.RawEpilogue.Length > 0) {
				WriteBytes (options, multipart.RawEpilogue, frame.EnsureNewLine);
			} else if (frame.EnsureNewLine && (multipart.RawEndBoundary != null || multipart.RawEpilogue != null)) {
				// Note: The end boundary marker only includes a new-line sequence if it was generated and there is no epilogue.
				Write (options.NewLineBytes);
			}
		}

		// Write the entity (and all of its descendants).
		void Write (MimeEntity? entity, FormatOptions options, bool contentOnly, bool ensureNewLine)
		{
			while (true) {
				if (entity != null) {
					entity.CheckDisposed (nameof (MimeEntity));

					if (!contentOnly)
						entity.Headers.WriteTo (options, stream, cancellationToken);

					if (entity is Multipart multipart) {
						var multipartOptions = GetMultipartContentFormatOptions (multipart, options);

						WritePreamble (multipart, multipartOptions, ensureNewLine);
						PushMultipart (multipart, multipartOptions, ensureNewLine);
						entity = null;
					} else if (entity is MessagePart messagePart) {
						var message = messagePart.Message;

						if (message != null) {
							if (message.MboxMarker != null && message.MboxMarker.Length != 0)
								Write (message.MboxMarker);

							options = GetEmbeddedMessageFormatOptions (options, ensureNewLine);
							message.WriteHeaders (options, stream, cancellationToken);
							entity = GetMessageBody (message, options, false, out ensureNewLine);
							contentOnly = true;
						} else {
							entity = null;
						}
					} else {
						if (entity is MimePart part)
							part.WriteContent (options, stream, ensureNewLine, cancellationToken);

						entity = null;
					}

					continue;
				}

				if (depth == 0)
					return;

				ref var frame = ref frames![depth - 1];

				if (IsMultipartComplete (frame.Multipart, frame.Index)) {
					WriteEnd (in frame);
					PopMultipart ();
					continue;
				}

				int index = frame.Index++;

				WriteBoundary (in frame, index);

				entity = frame.Multipart[index];
				ensureNewLine = entity.EnsureNewLine || (frame.EnsureNewLine && frame.Multipart.IsLastChildOfTruncatedMultipart (index));
				options = frame.Options;
				contentOnly = false;
			}
		}

		Task WriteAsync (byte[] buffer, int offset, int count)
		{
			return stream.WriteAsync (buffer, offset, count, cancellationToken);
		}

		Task WriteAsync (byte[] buffer)
		{
			return stream.WriteAsync (buffer, 0, buffer.Length, cancellationToken);
		}

		// Asynchronously write the bytes, converting new-lines to the requested format.
		Task WriteBytesAsync (FormatOptions options, byte[] bytes, bool ensureNewLine)
		{
			var filter = options.CreateNewLineFilter (ensureNewLine);
			var output = filter.Flush (bytes, 0, bytes.Length, out int index, out int length);

			return WriteAsync (output, index, length);
		}

		// Asynchronously writes a raw boundary marker (which may consist of multiple lines in the case of a "double boundary").
		//
		// See WriteRawBoundary() for details.
		async Task WriteRawBoundaryAsync (FormatOptions options, byte[] rawBoundary)
		{
			int startIndex = 0;

			if (options.VerifyingSignature) {
				int index = 0, n;

				while ((n = rawBoundary.AsSpan (index).IndexOf ((byte) '\n')) != -1) {
					index += n;

					if (index == 0 || rawBoundary[index - 1] != (byte) '\r') {
						await WriteAsync (rawBoundary, startIndex, index - startIndex).ConfigureAwait (false);
						await WriteAsync (options.NewLineBytes).ConfigureAwait (false);
						startIndex = index + 1;
					}

					index++;
				}
			}

			if (startIndex < rawBoundary.Length)
				await WriteAsync (rawBoundary, startIndex, rawBoundary.Length - startIndex).ConfigureAwait (false);
		}

		Task WritePreambleAsync (Multipart multipart, FormatOptions options, bool ensureNewLine)
		{
			if (multipart.RawPreamble != null && multipart.RawPreamble.Length > 0)
				return WriteBytesAsync (options, multipart.RawPreamble, multipart.Count > 0 || ensureNewLine);

			return Task.CompletedTask;
		}

		// Asynchronously write the boundary marker (and the new-line sequence that precedes it, if needed) for the specified child.
		async Task WriteBoundaryAsync (MultipartFrame frame, int index)
		{
			if (NeedsNewLineAfter (frame.Multipart, index - 1))
				await WriteAsync (frame.Options.NewLineBytes).ConfigureAwait (false);

			if (frame.Multipart.rawBoundaries?[index] is byte[] rawBoundary)
				await WriteRawBoundaryAsync (frame.Options, rawBoundary).ConfigureAwait (false);
			else
				await WriteAsync (frame.DefaultBoundary).ConfigureAwait (false);
		}

		// Asynchronously write everything that follows the last child: the end boundary and the epilogue.
		async Task WriteEndAsync (MultipartFrame frame)
		{
			var multipart = frame.Multipart;
			var options = frame.Options;

			if (NeedsNewLineAfter (multipart, multipart.Count - 1) && (!multipart.IsTruncated || multipart.RawTrailingBoundary != null))
				await WriteAsync (options.NewLineBytes).ConfigureAwait (false);

			if (multipart.IsTruncated) {
				// The multipart was truncated, so there is no end boundary (or epilogue) to write. There may, however,
				// have been a boundary marker at the end of the stream that was not followed by a MIME part.
				if (multipart.RawTrailingBoundary != null) {
					await WriteRawBoundaryAsync (options, multipart.RawTrailingBoundary).ConfigureAwait (false);

					if (frame.EnsureNewLine && !Multipart.EndsWithNewLine (multipart.RawTrailingBoundary))
						await WriteAsync (options.NewLineBytes).ConfigureAwait (false);
				}

				return;
			}

			if (multipart.RawEndBoundary != null)
				await WriteRawBoundaryAsync (options, multipart.RawEndBoundary).ConfigureAwait (false);
			else
				await WriteAsync (GetDefaultEndBoundary (multipart, options)).ConfigureAwait (false);

			if (multipart.RawEpilogue != null && multipart.RawEpilogue.Length > 0) {
				await WriteBytesAsync (options, multipart.RawEpilogue, frame.EnsureNewLine).ConfigureAwait (false);
			} else if (frame.EnsureNewLine && (multipart.RawEndBoundary != null || multipart.RawEpilogue != null)) {
				// Note: The end boundary marker only includes a new-line sequence if it was generated and there is no epilogue.
				await WriteAsync (options.NewLineBytes).ConfigureAwait (false);
			}
		}

		// Asynchronously write the entity (and all of its descendants).
		async Task WriteAsync (MimeEntity? entity, FormatOptions options, bool contentOnly, bool ensureNewLine)
		{
			while (true) {
				if (entity != null) {
					entity.CheckDisposed (nameof (MimeEntity));

					if (!contentOnly)
						await entity.Headers.WriteToAsync (options, stream, cancellationToken).ConfigureAwait (false);

					if (entity is Multipart multipart) {
						var multipartOptions = GetMultipartContentFormatOptions (multipart, options);

						await WritePreambleAsync (multipart, multipartOptions, ensureNewLine).ConfigureAwait (false);
						PushMultipart (multipart, multipartOptions, ensureNewLine);
						entity = null;
					} else if (entity is MessagePart messagePart) {
						var message = messagePart.Message;

						if (message != null) {
							if (message.MboxMarker != null && message.MboxMarker.Length != 0)
								await WriteAsync (message.MboxMarker).ConfigureAwait (false);

							options = GetEmbeddedMessageFormatOptions (options, ensureNewLine);
							await message.WriteHeadersAsync (options, stream, cancellationToken).ConfigureAwait (false);
							entity = GetMessageBody (message, options, false, out ensureNewLine);
							contentOnly = true;
						} else {
							entity = null;
						}
					} else {
						if (entity is MimePart part)
							await part.WriteContentAsync (options, stream, ensureNewLine, cancellationToken).ConfigureAwait (false);

						entity = null;
					}

					continue;
				}

				if (depth == 0)
					return;

				// Note: A ref local cannot be used in an async method, so the frame is copied instead. This is fine because
				// the only field that changes (Index) is updated in the array before the copy is made.
				int top = depth - 1;

				if (IsMultipartComplete (frames![top].Multipart, frames[top].Index)) {
					await WriteEndAsync (frames[top]).ConfigureAwait (false);
					PopMultipart ();
					continue;
				}

				int index = frames[top].Index++;
				var frame = frames[top];

				await WriteBoundaryAsync (frame, index).ConfigureAwait (false);

				entity = frame.Multipart[index];
				ensureNewLine = entity.EnsureNewLine || (frame.EnsureNewLine && frame.Multipart.IsLastChildOfTruncatedMultipart (index));
				options = frame.Options;
				contentOnly = false;
			}
		}
	}
}
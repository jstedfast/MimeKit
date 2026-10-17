//
// MimeAnonymizer.cs
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
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using MimeKit.IO;
using MimeKit.IO.Filters;
using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// A tool for anonymizing a message or MIME entity.
	/// </summary>
	/// <remarks>
	/// Allows you to anonymize a <see cref="MimeMessage"/> or <see cref="MimeEntity"/>.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\MimeAnonymizerExamples.cs" region="AnonymizeMessage"/>
	/// </example>
	public class MimeAnonymizer
	{
		static ReadOnlySpan<byte> AddressSpecials => " \t\r\n()<>[]:;@,."u8;
		static ReadOnlySpan<byte> ReceivedSpecials => "<>[]:@,."u8;
		static ReadOnlySpan<byte> ReceivedFrom => "from"u8;
		static ReadOnlySpan<byte> ReceivedBy => "by"u8;
		static ReadOnlySpan<byte> ReceivedVia => "via"u8;
		static ReadOnlySpan<byte> ReceivedWith => "with"u8;
		static ReadOnlySpan<byte> ReceivedId => "id"u8;
		static ReadOnlySpan<byte> ReceivedFor => "for"u8;
		static ReadOnlySpan<byte> BoundaryParameter => "boundary"u8;
		static ReadOnlySpan<byte> CharsetParameter => "charset"u8;
		static ReadOnlySpan<byte> DelspParameter => "delsp"u8;
		static ReadOnlySpan<byte> FormatParameter => "format"u8;
		static ReadOnlySpan<byte> Whitespace => " \t\r\n"u8;

		readonly HashSet<string> preserveHeaders;

		/// <summary>
		/// Initialize a new instance of the <see cref="MimeAnonymizer"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimeAnonymizer"/>.
		/// </remarks>
		public MimeAnonymizer ()
		{
			preserveHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Get the set of headers that this anonymizer is configured to preserve.
		/// </summary>
		/// <remarks>
		/// <para>Gets the set of headers that this anonymizer is configured to preserve.</para>
		/// <para>Headers can be added or removed from this set in order to influence the output of the anonymizer.</para>
		/// <note type="note">This set of headers to preserve also applies to the status headers in the content of
		/// message/delivery-status parts as well.</note>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeAnonymizerExamples.cs" region="PreserveHeaders"/>
		/// </example>
		/// <value>The set of headers that the anonymizer is configured to preserve.</value>
		public HashSet<string> PreserveHeaders {
			get { return preserveHeaders; }
		}

		/// <summary>
		/// Anonymize a <see cref="MimeMessage"/>.
		/// </summary>
		/// <remarks>
		/// Writes an anonymized version of the message to a stream.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeAnonymizerExamples.cs" region="AnonymizeMessage"/>
		/// </example>
		/// <param name="message">The message to anonymize.</param>
		/// <param name="stream">The stream to write the anonymized message to.</param>
		/// <exception cref="ArgumentNullException">
		/// <para><paramref name="message"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <see langword="null"/>.</para>
		/// </exception>
		public void Anonymize (MimeMessage message, Stream stream)
		{
			Anonymize (FormatOptions.Default, message, stream);
		}

		/// <summary>
		/// Anonymize a <see cref="MimeMessage"/>.
		/// </summary>
		/// <remarks>
		/// Writes an anonymized version of the message to a stream.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeAnonymizerExamples.cs" region="PreserveHeaders"/>
		/// </example>
		/// <param name="options">The formatting options.</param>
		/// <param name="message">The message to anonymize.</param>
		/// <param name="stream">The stream to write the anonymized message to.</param>
		/// <exception cref="ArgumentNullException">
		/// <para><paramref name="options"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="message"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <see langword="null"/>.</para>
		/// </exception>
		public void Anonymize (FormatOptions options, MimeMessage message, Stream stream)
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			if (message == null)
				throw new ArgumentNullException (nameof (message));

			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			AnonymizeMessage (options, message, stream);
		}

		/// <summary>
		/// Anonymize a <see cref="MimeEntity"/>.
		/// </summary>
		/// <remarks>
		/// Writes an anonymized version of the entity to a stream.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeAnonymizerExamples.cs" region="AnonymizeEntity"/>
		/// </example>
		/// <param name="entity">The MIME entity to anonymize.</param>
		/// <param name="stream">The stream to write the anonymized entity to.</param>
		/// <exception cref="ArgumentNullException">
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <see langword="null"/>.</para>
		/// </exception>
		public void Anonymize (MimeEntity entity, Stream stream)
		{
			Anonymize (FormatOptions.Default, entity, stream);
		}

		/// <summary>
		/// Anonymize a <see cref="MimeEntity"/>.
		/// </summary>
		/// <remarks>
		/// Writes an anonymized version of the entity to a stream.
		/// </remarks>
		/// <param name="options">The formatting options.</param>
		/// <param name="entity">The MIME entity to anonymize.</param>
		/// <param name="stream">The stream to write the anonymized entity to.</param>
		/// <exception cref="ArgumentNullException">
		/// <para><paramref name="options"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <see langword="null"/>.</para>
		/// </exception>
		public void Anonymize (FormatOptions options, MimeEntity entity, Stream stream)
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			AnonymizeEntity (options, entity, stream, false, entity.EnsureNewLine);
		}

		static bool IsReceivedKeyword (byte[] rawValue, int index, out int length)
		{
			// look for: from, by, via, with, id, for
			byte[] buffer = ArrayPool<byte>.Shared.Rent (4);
			length = 0;

			while (index < rawValue.Length && length < 4 && !rawValue[index].IsWhitespace ()) {
				if (rawValue[index] >= (byte) 'A' && rawValue[index] <= 'Z') {
					buffer[length++] = (byte) (rawValue[index] + 0x20);
				} else if (rawValue[index] >= 'a' && rawValue[index] <= 'z') {
					buffer[length++] = rawValue[index];
				} else {
					ArrayPool<byte>.Shared.Return (buffer);
					return false;
				}

				index++;
			}

			if (index >= rawValue.Length || !rawValue[index].IsWhitespace ()) {
				ArrayPool<byte>.Shared.Return (buffer);
				return false;
			}

			var word = new ReadOnlySpan<byte> (buffer, 0, length);
			var keyword = word.SequenceEqual (ReceivedFrom) ||
				word.SequenceEqual (ReceivedBy) ||
				word.SequenceEqual (ReceivedVia) ||
				word.SequenceEqual (ReceivedWith) ||
				word.SequenceEqual (ReceivedId) ||
				word.SequenceEqual (ReceivedFor);

			ArrayPool<byte>.Shared.Return (buffer);

			return keyword;
		}

		internal static byte[] AnonymizeReceivedHeaderValue (byte[] rawValue)
		{
			var anonymized = new byte[rawValue.Length];
			int index = 0;

			while (index < rawValue.Length) {
				while (index < rawValue.Length && rawValue[index].IsWhitespace ()) {
					anonymized[index] = rawValue[index];
					index++;
				}

				if (index >= rawValue.Length)
					break;

				if (rawValue[index] == (byte) '(') {
					// comment
					int commentDepth = 1;
					bool escaped = false;

					// consume the '('
					anonymized[index] = rawValue[index];
					index++;

					while (index < rawValue.Length && commentDepth > 0) {
						if (rawValue[index] == (byte) '\\') {
							anonymized[index] = rawValue[index];
							escaped = !escaped;
						} else if (!escaped) {
							if (rawValue[index] == (byte) '(') {
								anonymized[index] = rawValue[index];
								commentDepth++;
							} else if (rawValue[index] == (byte) ')') {
								anonymized[index] = rawValue[index];
								commentDepth--;
							} else if (rawValue[index].IsWhitespace ()) {
								anonymized[index] = rawValue[index];
							} else if (ReceivedSpecials.IndexOf (rawValue[index]) != -1) {
								anonymized[index] = rawValue[index];
							} else {
								// anonymize everything else in the comment
								anonymized[index] = (byte) 'x';
							}
						} else {
							// escaped
							anonymized[index] = (byte) 'x';
							escaped = false;
						}

						index++;
					}
				} else if (rawValue[index] == (byte) ';') {
					anonymized[index] = rawValue[index];
					index++;

					while (index < rawValue.Length && rawValue[index].IsWhitespace ()) {
						anonymized[index] = rawValue[index];
						index++;
					}

					// This might be a date...
					if (DateUtils.TryParse (rawValue, index, out var date)) {
						// don't anonymize the date
						Buffer.BlockCopy (rawValue, index, anonymized, index, rawValue.Length - index);
						break;
					}
				} else if (IsReceivedKeyword (rawValue, index, out int length)) {
					Buffer.BlockCopy (rawValue, index, anonymized, index, length);
					index += length;
				} else {
					while (index < rawValue.Length) {
						if (rawValue[index].IsWhitespace () || rawValue[index] == (byte) ';' || rawValue[index] == (byte) '(')
							break;

						if (ReceivedSpecials.IndexOf (rawValue[index]) != -1) {
							anonymized[index] = rawValue[index];
						} else {
							anonymized[index] = (byte) 'x';
						}

						index++;
					}
				}
			}

			return anonymized;
		}

		enum Rfc2047EncodedWordState
		{
			None,
			Equals,
			EqualsQuestion,
			Charset,
			CharsetQuestion,
			Encoding,
			EncodingQuestion,
			Payload,
			PayloadQuestion,
		}

		static void PushPotentialRfc2047EncodedWordByte (ref Rfc2047EncodedWordState rfc2047, byte c, ref int index, byte[] anonymized, ReadOnlySpan<byte> specials)
		{
			if (c == (byte) '=') {
				switch (rfc2047) {
				case Rfc2047EncodedWordState.None:
					rfc2047 = Rfc2047EncodedWordState.Equals;
					anonymized[index] = c;
					break;
				case Rfc2047EncodedWordState.PayloadQuestion:
					rfc2047 = Rfc2047EncodedWordState.None;
					anonymized[index] = c;
					break;
				case Rfc2047EncodedWordState.Payload:
					// anonymize '=' in the payload
					anonymized[index] = (byte) 'x';
					break;
				default:
					// break out of rfc2047 encoded-word mode
					rfc2047 = Rfc2047EncodedWordState.None;
					anonymized[index] = c;
					break;
				}
			} else if (c == (byte) '?') {
				anonymized[index] = c;

				switch (rfc2047) {
				case Rfc2047EncodedWordState.Equals:
					rfc2047 = Rfc2047EncodedWordState.EqualsQuestion;
					break;
				case Rfc2047EncodedWordState.Charset:
					rfc2047 = Rfc2047EncodedWordState.CharsetQuestion;
					break;
				case Rfc2047EncodedWordState.Encoding:
					rfc2047 = Rfc2047EncodedWordState.EncodingQuestion;
					break;
				case Rfc2047EncodedWordState.Payload:
					rfc2047 = Rfc2047EncodedWordState.PayloadQuestion;
					break;
				case Rfc2047EncodedWordState.None:
					// just a normnal question mark
					break;
				default:
					// break out of rfc2047 encoded-word mode
					rfc2047 = Rfc2047EncodedWordState.None;
					break;
				}
			} else {
				switch (rfc2047) {
				case Rfc2047EncodedWordState.EqualsQuestion:
					rfc2047 = Rfc2047EncodedWordState.Charset;
					goto case Rfc2047EncodedWordState.Charset;
				case Rfc2047EncodedWordState.Charset:
					// allow charset name to pass through...
					anonymized[index] = c;
					break;
				case Rfc2047EncodedWordState.CharsetQuestion:
					rfc2047 = Rfc2047EncodedWordState.Encoding;
					goto case Rfc2047EncodedWordState.Encoding;
				case Rfc2047EncodedWordState.Encoding:
					// allow encoding name to pass through...
					anonymized[index] = c;
					break;
				case Rfc2047EncodedWordState.EncodingQuestion:
					rfc2047 = Rfc2047EncodedWordState.Payload;
					goto case Rfc2047EncodedWordState.Payload;
				case Rfc2047EncodedWordState.Payload:
					// anonymize everything but whitespace in the rfc2047 encoded-word payload
					// mostly in case of folding whitespace but also because it can screw up
					// tokenization in MIME parsers and we want to be able to see that.
					if (c.IsWhitespace ())
						anonymized[index] = c;
					else
						anonymized[index] = (byte) 'x';
					break;
				case Rfc2047EncodedWordState.None:
					if (specials.IndexOf (c) != -1) {
						anonymized[index] = c;
					} else {
						anonymized[index] = (byte) 'x';
					}
					break;
				default:
					// break out of rfc2047 encoded-word mode
					rfc2047 = Rfc2047EncodedWordState.None;
					// rewind 1 character
					index--;
					break;
				}
			}
		}

		internal static byte[] AnonymizeAddressHeaderValue (byte[] rawValue)
		{
			var anonymized = new byte[rawValue.Length];
			var rfc2047 = Rfc2047EncodedWordState.None;
			var escaped = false;
			var quoted = false;

			for (int i = 0; i < rawValue.Length; i++) {
				byte c = rawValue[i];

				if (c == (byte) '\\') {
					anonymized[i] = c;
					escaped = !escaped;
				} else if (c == (byte) '"') {
					anonymized[i] = c;
					if (escaped)
						escaped = false;
					else
						quoted = !quoted;
				} else if (escaped) {
					anonymized[i] = c;
					escaped = false;
				} else if (quoted) {
					// anonymize everything but folding whitespace within a quoted string
					if (c == (byte) '\r' || c == (byte) '\n')
						anonymized[i] = c;
					else
						anonymized[i] = (byte) 'x';
				} else {
					PushPotentialRfc2047EncodedWordByte (ref rfc2047, c, ref i, anonymized, AddressSpecials);
				}
			}

			return anonymized;
		}

		internal static byte[] AnonymizeUnstructuredHeaderValue (byte[] rawValue)
		{
			var anonymized = new byte[rawValue.Length];
			var rfc2047 = Rfc2047EncodedWordState.None;

			for (int i = 0; i < rawValue.Length; i++) {
				byte c = rawValue[i];

				PushPotentialRfc2047EncodedWordByte (ref rfc2047, c, ref i, anonymized, Whitespace);
			}

			return anonymized;
		}

		enum ParameterState
		{
			Semicolon,
			Name,
			NameStar,
			Value,
		}

		static bool IsSafeParameterName (ByteArrayBuilder name)
		{
			return name.Equals (BoundaryParameter, StringComparison.OrdinalIgnoreCase) ||
				name.Equals (CharsetParameter, StringComparison.OrdinalIgnoreCase) ||
				name.Equals (DelspParameter, StringComparison.OrdinalIgnoreCase) ||
				name.Equals (FormatParameter, StringComparison.OrdinalIgnoreCase);
		}

		static void AnonymizeParameterList (byte[] rawValue, byte[] anonymized, int startIndex)
		{
			using var name = new ByteArrayBuilder (16);
			var state = ParameterState.Semicolon;
			int index = startIndex;
			var escaped = false;
			var quoted = false;
			var safe = false;

			if (index < rawValue.Length) {
				anonymized[index] = rawValue[index];
				index++;
			}

			while (index < rawValue.Length) {
				switch (state) {
				case ParameterState.Semicolon:
					if (rawValue[index] == (byte) ';') {
						// multiple semicolons in a row
						anonymized[index] = rawValue[index];
					} else if (rawValue[index].IsWhitespace ()) {
						// whitespace character
						anonymized[index] = rawValue[index];
					} else {
						state = ParameterState.Name;
						goto case ParameterState.Name;
					}
					break;
				case ParameterState.Name:
					if (rawValue[index] == (byte) '=') {
						anonymized[index] = rawValue[index];
						safe = IsSafeParameterName (name);
						state = ParameterState.Value;
					} else if (rawValue[index] == (byte) ';') {
						// shouldn't happen...
						anonymized[index] = rawValue[index];
						state = ParameterState.Semicolon;
						name.Clear ();
					} else if (rawValue[index] == (byte) '*') {
						state = ParameterState.NameStar;
						anonymized[index] = rawValue[index];
					} else {
						name.Append (rawValue[index]);
						anonymized[index] = rawValue[index];
					}
					break;
				case ParameterState.NameStar:
					if (rawValue[index] == (byte) '=') {
						anonymized[index] = rawValue[index];
						safe = IsSafeParameterName (name);
						state = ParameterState.Value;
					} else if (rawValue[index] == (byte) ';') {
						// parameter seems to be incomplete?
						state = ParameterState.Semicolon;
						anonymized[index] = rawValue[index];
					} else {
						anonymized[index] = rawValue[index];
					}
					break;
				case ParameterState.Value:
					if (rawValue[index] == (byte) '"') {
						anonymized[index] = rawValue[index];
						if (escaped)
							escaped = false;
						else
							quoted = !quoted;
					} else if (quoted) {
						if (rawValue[index] == (byte) '\\') {
							anonymized[index] = rawValue[index];
							escaped = !escaped;
						} else if (rawValue[index] == (byte) '\r' || rawValue[index] == (byte) '\n') {
							// don't anonymize folding whitespace within a quoted value
							anonymized[index] = rawValue[index];
							escaped = false;
						} else {
							if (safe)
								anonymized[index] = rawValue[index];
							else
								anonymized[index] = (byte) 'x';
							escaped = false;
						}
					} else if (rawValue[index] == (byte) ';') {
						anonymized[index] = rawValue[index];
						state = ParameterState.Semicolon;
						name.Clear ();
					} else if (rawValue[index].IsWhitespace ()) {
						anonymized[index] = rawValue[index];
					} else if (safe) {
						anonymized[index] = rawValue[index];
					} else {
						anonymized[index] = (byte) 'x';
					}
					break;
				}

				index++;
			}
		}

		internal static byte[] AnonymizeContentDispositionValue (byte[] rawValue)
		{
			var anonymized = new byte[rawValue.Length];
			int index = 0;

			// don't anonymize the "attachment" or "inline" part
			while (index < rawValue.Length && rawValue[index] != (byte) ';') {
				anonymized[index] = rawValue[index];
				index++;
			}

			AnonymizeParameterList (rawValue, anonymized, index);

			return anonymized;
		}

		internal static byte[] AnonymizeContentTypeValue (byte[] rawValue)
		{
			var anonymized = new byte[rawValue.Length];
			int index = 0;

			// don't anonymize the mime-type
			while (index < rawValue.Length && rawValue[index] != (byte) ';') {
				anonymized[index] = rawValue[index];
				index++;
			}

			AnonymizeParameterList (rawValue, anonymized, index);

			return anonymized;
		}

		byte[] AnonymizeHeader (FormatOptions options, Header header)
		{
			var rawValue = header.GetRawValue (options);

			if (preserveHeaders.Contains (header.Field)) {
				// don't anonymize this header
				return rawValue;
			}

			switch (header.Id) {
			case HeaderId.DispositionNotificationTo:
			case HeaderId.ResentReplyTo:
			case HeaderId.ResentSender:
			case HeaderId.ResentFrom:
			case HeaderId.ResentBcc:
			case HeaderId.ResentCc:
			case HeaderId.ResentTo:
			case HeaderId.ReplyTo:
			case HeaderId.Sender:
			case HeaderId.From:
			case HeaderId.Bcc:
			case HeaderId.Cc:
			case HeaderId.To:
				return AnonymizeAddressHeaderValue (rawValue);
			case HeaderId.Received:
				return AnonymizeReceivedHeaderValue (rawValue);
			case HeaderId.OriginalMessageId:
			case HeaderId.ResentMessageId:
			case HeaderId.References:
			case HeaderId.InReplyTo:
			case HeaderId.MessageId:
			case HeaderId.ContentId:
				// We'll treat these like address headers (for now)...
				return AnonymizeAddressHeaderValue (rawValue);
			case HeaderId.ContentDisposition:
				return AnonymizeContentDispositionValue (rawValue);
			case HeaderId.ContentType:
				return AnonymizeContentTypeValue (rawValue);
			case HeaderId.ArcAuthenticationResults:
			case HeaderId.AuthenticationResults:
			case HeaderId.ArcMessageSignature:
			case HeaderId.ArcSeal:
			case HeaderId.DkimSignature:
				// TODO: should we have custom logic for anonymizing these?
				return AnonymizeUnstructuredHeaderValue (rawValue);
			case HeaderId.ContentTransferEncoding:
			case HeaderId.MimeVersion:
			case HeaderId.Date:
				// don't anonymize these
				return rawValue;
			default:
				return AnonymizeUnstructuredHeaderValue (rawValue);
			}
		}

		void AnonymizeHeaders (FormatOptions options, IEnumerable<Header> headers, Stream stream)
		{
			using (var filtered = new FilteredStream (stream)) {
				filtered.Add (options.CreateNewLineFilter ());

				foreach (var header in headers) {
					if (header.IsInvalid) {
						AnonymizeBytes (options, stream, header.RawField, false);
					} else {
						var rawValue = AnonymizeHeader (options, header);

						filtered.Write (header.RawField, 0, header.RawField.Length);
						filtered.Write (Header.Colon, 0, Header.Colon.Length);
						filtered.Write (rawValue, 0, rawValue.Length);
					}
				}

				filtered.Flush ();
			}
		}

		// Anonymize the message headers (merged with the headers of the body, if any) and return the body (if any) that should be
		// anonymized next as well as whether it must end with a new-line.
		MimeEntity? AnonymizeMessageHeaders (FormatOptions options, MimeMessage message, Stream stream, out bool ensureNewLine)
		{
			if (message.compliance == RfcComplianceMode.Strict && message.Body != null && message.Body.Headers.Count > 0 && !message.Headers.Contains (HeaderId.MimeVersion))
				message.MimeVersion = new Version (1, 0);

			if (message.Body != null) {
				AnonymizeHeaders (options, MimeMessage.MergeHeaders (message.Headers, message.Body), stream);

				if (message.compliance == RfcComplianceMode.Strict || message.Body.Headers.HasBodySeparator)
					stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);

				ensureNewLine = message.compliance == RfcComplianceMode.Strict || options.EnsureNewLine;

				return message.Body;
			}

			AnonymizeHeaders (options, message.Headers, stream);
			stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
			ensureNewLine = false;

			return null;
		}

		void AnonymizeMessage (FormatOptions options, MimeMessage message, Stream stream)
		{
			var body = AnonymizeMessageHeaders (options, message, stream, out bool ensureNewLine);

			if (body != null)
				AnonymizeEntity (options, body, stream, true, ensureNewLine);
		}
		static byte[] GenerateBoundaryMarker (string boundary, byte[] newLine)
		{
			var marker = new byte[2 + boundary.Length + newLine.Length];
			int index = 0;

			marker[index++] = (byte) '-';
			marker[index++] = (byte) '-';

			for (int i = 0; i < boundary.Length; i++)
				marker[index++] = (byte) boundary[i];

			for (int i = 0; i < newLine.Length; i++)
				marker[index++] = newLine[i];

			return marker;
		}

		static byte[] GenerateEndBoundaryMarker (string boundary, byte[] newLine)
		{
			var marker = new byte[4 + boundary.Length + newLine.Length];
			int index = 0;

			marker[index++] = (byte) '-';
			marker[index++] = (byte) '-';

			for (int i = 0; i < boundary.Length; i++)
				marker[index++] = (byte) boundary[i];

			marker[index++] = (byte) '-';
			marker[index++] = (byte) '-';

			for (int i = 0; i < newLine.Length; i++)
				marker[index++] = newLine[i];

			return marker;
		}

		static void AnonymizeBytes (FormatOptions options, Stream stream, byte[]? rawValue, bool ensureNewLine)
		{
			if (rawValue == null || rawValue.Length == 0)
				return;

			using (var filtered = new FilteredStream (stream)) {
				filtered.Add (new AnonymizeFilter ());
				filtered.Add (options.CreateNewLineFilter (ensureNewLine));

				filtered.Write (rawValue, 0, rawValue.Length);
				filtered.Flush ();
			}
		}

		static bool TryGetStatusGroups (MessageDeliveryStatus mds, [NotNullWhen (true)] out HeaderListCollection? statusGroups)
		{
			try {
				statusGroups = mds.StatusGroups;
				return true;
			} catch (Exception ex) when (ex is FormatException || ex is ObjectDisposedException) {
				statusGroups = null;
				return false;
			}
		}

		static bool TryGetNotificationFields (MessageDispositionNotification mdn, [NotNullWhen (true)] out HeaderList? fields)
		{
			try {
				fields = mdn.Fields;
				return true;
			} catch (Exception ex) when (ex is FormatException || ex is ObjectDisposedException) {
				fields = null;
				return false;
			}
		}

		struct MultipartFrame
		{
			public Multipart Multipart;
			public FormatOptions Options;
			public byte[] DefaultBoundary;
			public bool EnsureNewLine;
			public int Index;
		}

		static void WriteMultipartEnd (in MultipartFrame frame, Stream stream)
		{
			var multipart = frame.Multipart;
			var options = frame.Options;

			if (multipart.IsTruncated) {
				// The multipart was truncated, so there is no end boundary (or epilogue) to write.
				if (multipart.RawTrailingBoundary != null) {
					if (multipart.Count > 0 && !multipart[multipart.Count - 1].EndsWithDelimiterNewLine)
						stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);

					stream.Write (multipart.RawTrailingBoundary, 0, multipart.RawTrailingBoundary.Length);

					if (frame.EnsureNewLine && !Multipart.EndsWithNewLine (multipart.RawTrailingBoundary))
						stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
				}
				return;
			}

			if (multipart.Count > 0 && !multipart[multipart.Count - 1].EndsWithDelimiterNewLine)
				stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);

			if (multipart.RawEndBoundary != null) {
				stream.Write (multipart.RawEndBoundary, 0, multipart.RawEndBoundary.Length);
			} else {
				var boundary = GenerateEndBoundaryMarker (multipart.Boundary ?? string.Empty, multipart.RawEpilogue is null ? options.NewLineBytes : Array.Empty<byte> ());

				stream.Write (boundary, 0, boundary.Length);
			}

			if (multipart.RawEpilogue != null && multipart.RawEpilogue.Length > 0)
				AnonymizeBytes (options, stream, multipart.RawEpilogue, frame.EnsureNewLine);
			else if (frame.EnsureNewLine && (multipart.RawEndBoundary != null || multipart.RawEpilogue != null))
				stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
		}

		// Anonymize the entity (and all of its descendants).
		//
		// Note: Nested multiparts and message parts are handled using an explicit stack rather than by recursion so that
		// the nesting depth of the entity cannot exhaust the call stack. Only multiparts need a frame on the stack because
		// they have more to write once a child has been written. The embedded message is the last thing that a message part
		// writes and the body is the last thing that a message writes, so the anonymizer simply continues with them instead.
		void AnonymizeEntity (FormatOptions options, MimeEntity? entity, Stream stream, bool contentOnly, bool ensureNewLine)
		{
			MultipartFrame[]? frames = null;
			int depth = 0;

			while (true) {
				if (entity != null) {
					if (!contentOnly) {
						AnonymizeHeaders (options, entity.Headers, stream);

						if (entity.Headers.HasBodySeparator)
							stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
					}

					if (entity is MessagePart messagePart) {
						if (messagePart.Message != null) {
							// Note: Only ensure that the embedded message ends with a new-line if this part is supposed to end with one.
							if (options.EnsureNewLine != ensureNewLine) {
								options = options.Clone ();
								options.EnsureNewLine = ensureNewLine;
							}

							entity = AnonymizeMessageHeaders (options, messagePart.Message, stream, out ensureNewLine);
							contentOnly = true;
							continue;
						}
					} else if (entity is Multipart multipart) {
						AnonymizeBytes (options, stream, multipart.RawPreamble, multipart.Count > 0 || ensureNewLine);

						if (frames is null)
							frames = new MultipartFrame[8];
						else if (depth == frames.Length)
							Array.Resize (ref frames, frames.Length * 2);

						ref var pushed = ref frames[depth++];
						pushed.Multipart = multipart;
						pushed.Options = options;
						pushed.DefaultBoundary = GenerateBoundaryMarker (multipart.Boundary ?? string.Empty, options.NewLineBytes);
						pushed.EnsureNewLine = ensureNewLine;
						pushed.Index = 0;
					} else if (entity is MessageDeliveryStatus mds && TryGetStatusGroups (mds, out var statusGroups)) {
						for (int i = 0; i < statusGroups.Count; i++) {
							var statusGroup = statusGroups[i];

							AnonymizeHeaders (options, statusGroup, stream);

							if (i + 1 < statusGroups.Count)
								stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
						}
					} else if (entity is MessageDispositionNotification mdn && TryGetNotificationFields (mdn, out var fields)) {
						AnonymizeHeaders (options, fields, stream);
					} else {
						entity.CheckDisposed (nameof (MimeEntity));

						if (entity is MimePart part) {
							using (var filtered = new FilteredStream (stream)) {
								filtered.Add (new AnonymizeFilter ());

								part.WriteContent (options, filtered, ensureNewLine, default);
							}
						}
					}

					entity = null;
					continue;
				}

				if (depth == 0)
					return;

				ref var frame = ref frames![depth - 1];

				if (frame.Index < frame.Multipart.Count) {
					int index = frame.Index++;
					var boundary = frame.Multipart.rawBoundaries?[index] ?? frame.DefaultBoundary;

					if (index > 0 && !frame.Multipart[index - 1].EndsWithDelimiterNewLine)
						stream.Write (frame.Options.NewLineBytes, 0, frame.Options.NewLineBytes.Length);

					stream.Write (boundary, 0, boundary.Length);

					entity = frame.Multipart[index];
					ensureNewLine = entity.EnsureNewLine || (frame.EnsureNewLine && frame.Multipart.IsLastChildOfTruncatedMultipart (index));
					options = frame.Options;
					contentOnly = false;
				} else {
					WriteMultipartEnd (in frame, stream);
					frames[--depth] = default;
				}
			}
		}
	}
}
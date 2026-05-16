//
// Received.cs
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
using System.Net;
using System.Linq;
using System.Text;
using System.Buffers;
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// An enumeration of common Received header clauses.
	/// </summary>
	/// <remarks>
	/// <para>A typical <c>Received</c> header contains several clauses that provide information about the
	/// message transfer, including:</para>
	/// <list type="bullet">
	/// <item><description><c>from</c> - the name of the sending host</description></item>
	/// <item><description><c>by</c> - the name of the receiving host</description></item>
	/// <item><description><c>via</c> - the physical link or protocol used</description></item>
	/// <item><description><c>with</c> - the mail protocol used (e.g., SMTP, ESMTP, etc.)</description></item>
	/// <item><description><c>id</c> - a unique identifier for the message</description></item>
	/// <item><description><c>for</c> - the recipient address</description></item>
	/// </list>
	/// <para>For more information, see
	/// <a href="https://tools.ietf.org/html/rfc5321#section-4.4">section 4.4 of RFC 5321</a>.</para>
	/// </remarks>
	public enum ReceivedClauseId
	{
		/// <summary>
		/// Represents any comments that appear before the first keyword in the Received header value.
		/// </summary>
		/// <remarks>
		/// This clause is what preceedes the first keyword in the Received header value.
		/// It is used to capture any comments that appear before the first keyword.
		/// </remarks>
		None,

		/// <summary>
		/// Represents the "from" clause which identifies the name of the sending host.
		/// </summary>
		/// <remarks>
		/// This clause is used to capture the "from" clause which identifies the name of the sending host.
		/// </remarks>
		From,

		/// <summary>
		/// Represents the "by" clause which identifies the name of the receiving host.
		/// </summary>
		/// <remarks>
		/// This clause is used to capture the "by" clause which identifies the name of the receiving host.
		/// </remarks>
		By,

		/// <summary>
		/// Represents the "via" clause which identifies the physical link or protocol used.
		/// </summary>
		/// <remarks>
		/// This clause is used to capture the "via" clause which identifies the physical link or protocol used.
		/// </remarks>
		Via,

		/// <summary>
		/// Represents the "with" clause which identifies the mail protocol used (e.g., SMTP, ESMTP, etc.).
		/// </summary>
		/// <remarks>
		/// This clause is used to capture the "with" clause which identifies the mail protocol used (e.g., SMTP, ESMTP, etc.).
		/// </remarks>
		With,

		/// <summary>
		/// Represents the "id" clause which identifies a unique identifier for the message.
		/// </summary>
		/// <remarks>
		/// This clause is used to capture the "id" clause which identifies a unique identifier for the message.
		/// </remarks>
		Id,

		/// <summary>
		/// Represents the "for" clause which identifies the recipient address.
		/// </summary>
		/// <remarks>
		/// This clause is used to capture the "for" clause which identifies the recipient address.
		/// </remarks>
		For,

		/// <summary>
		/// Represents an unknown clause which could not be identified.
		/// </summary>
		/// <remarks>
		/// This clause is used to capture any clause that could not be identified.
		/// </remarks>
		Unknown
	}

	/// <summary>
	/// A parsed representation of the <c>Received</c> header.
	/// </summary>
	/// <remarks>
	/// <para>The <c>Received</c> header is used for tracing the path a message has taken through various SMTP servers
	/// as it is transmitted from sender to recipient. Each SMTP server that handles the message adds a new
	/// <c>Received</c> header to the top of the message, creating a chronological trace of the message's journey
	/// through the email infrastructure.</para>
	/// <para>A typical <c>Received</c> header contains several clauses that provide information about the
	/// message transfer, including:</para>
	/// <list type="bullet">
	/// <item><description><c>from</c> - the name of the sending host</description></item>
	/// <item><description><c>by</c> - the name of the receiving host</description></item>
	/// <item><description><c>via</c> - the physical link or protocol used</description></item>
	/// <item><description><c>with</c> - the mail protocol used (e.g., SMTP, ESMTP, etc.)</description></item>
	/// <item><description><c>id</c> - a unique identifier for the message</description></item>
	/// <item><description><c>for</c> - the recipient address</description></item>
	/// </list>
	/// <para>For more information, see
	/// <a href="https://tools.ietf.org/html/rfc5321#section-4.4">section 4.4 of RFC 5321</a>.</para>
	/// </remarks>
	public class Received
	{
#if NET8_0_OR_GREATER
		static readonly SearchValues<byte> EndOfTokenSentinels = SearchValues.Create ("\t\r\n (;"u8);
#else
		static ReadOnlySpan<byte> EndOfTokenSentinels => "\t\r\n (;"u8;
#endif
		static readonly char[] NonSpaceWhitespace = new char[] { '\t', '\r', '\n' };
		static readonly char[] CommentSpecials = new char[] { '\\' };
		internal static readonly string[] Keywords = new string[] {
			"from", "by", "via", "with", "id", "for"
		};

		readonly List<ReceivedClause> clauses;
		DateTimeOffset? dateTime;
		string? dateTimeStr;

		private Received (List<ReceivedClause> clauses, DateTimeOffset? dateTime, string? dateTimeStr)
		{
			this.dateTimeStr = dateTimeStr;
			this.dateTime = dateTime;
			this.clauses = clauses;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Received"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="Received"/> instance with all properties set to <see langword="null"/>.
		/// Properties can be set individually after construction to build up the Received header information.
		/// </remarks>
		public Received ()
		{
			clauses = new List<ReceivedClause> ();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Received"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="Received"/> instance with the specified source and receiving host information,
		/// along with the date and time the message was received.
		/// </remarks>
		/// <param name="from">The name of the source host.</param>
		/// <param name="fromTcpInfo">The IP address of the source host.</param>
		/// <param name="by">The name of the receiving host.</param>
		/// <param name="byTcpInfo">The IP address of the receiving host.</param>
		/// <param name="dateTime">The date and time the message was received.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="from"/> is <see langword="null"/>.
		/// <para>-or-</para>
		/// <paramref name="fromTcpInfo"/> is <see langword="null"/>.
		/// <para>-or-</para>
		/// <paramref name="by"/> is <see langword="null"/>.
		/// <para>-or-</para>
		/// <paramref name="byTcpInfo"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="from"/> is not a valid domain name or address literal.
		/// <para>-or-</para>
		/// <paramref name="by"/> is not a valid domain name or address literal.
		/// </exception>
		public Received (string from, IPAddress fromTcpInfo, string by, IPAddress byTcpInfo, DateTimeOffset dateTime)
		{
			if (from == null)
				throw new ArgumentNullException (nameof (from));

			if (fromTcpInfo == null)
				throw new ArgumentNullException (nameof (fromTcpInfo));

			if (by == null)
				throw new ArgumentNullException (nameof (by));

			if (byTcpInfo == null)
				throw new ArgumentNullException (nameof (byTcpInfo));

			clauses = new List<ReceivedClause> {
				new ReceivedClause (ReceivedClauseId.From, "from", from, $"[{fromTcpInfo}]"),
				new ReceivedClause (ReceivedClauseId.By, "by", by, $"[{byTcpInfo}]")
			};
			DateTime = dateTime;
		}

		/// <summary>
		/// Get the collection of clauses that make up the <c>Received</c> header.
		/// </summary>
		/// <remarks>
		/// Gets the collection of clauses that make up the <c>Received</c> header.
		/// </remarks>
		/// <value>The collection of clauses.</value>
		public IList<ReceivedClause> Clauses {
			get { return clauses; }
		}

		/// <summary>
		/// Get or set the date and time when the message was received.
		/// </summary>
		/// <remarks>
		/// Gets or sets the date and time that appears after the semicolon in the Received header,
		/// indicating when the receiving SMTP server accepted the message for delivery.
		/// </remarks>
		/// <value>The date and time the message was received if available; otherwise, <see langword="null"/>.</value>
		public DateTimeOffset? DateTime {
			get { return dateTime; }
			set {
				dateTime = value;
				dateTimeStr = null;
			}
		}

		#region Convenience Properties

		static string Unfold (string text)
		{
			int index = text.IndexOfAny (NonSpaceWhitespace);

			if (index == -1)
				return text;

			var lwsp = NonSpaceWhitespace.AsSpan ();
			var buffer = new char[text.Length];
			int length;

			text.AsSpan (0, index).CopyTo (buffer);
			length = index;

			for (int i = index; i < text.Length; i++) {
				char c = text[i];

				if (lwsp.IndexOf (c) == -1)
					buffer[length++] = c;
				else if (c != '\r' && c != '\n')
					buffer[length++] = ' ';
			}

			return new string (buffer, 0, length);
		}

		class ReceivedClauseComparer : IComparer<ReceivedClause>
		{
			public static readonly ReceivedClauseComparer Instance = new ReceivedClauseComparer ();

			/// <summary>
			/// Compare two <see cref="ReceivedClause"/> objects.
			/// </summary>
			/// <remarks>
			/// Compares two <see cref="ReceivedClause"/> objects and returns an integer that indicates
			/// whether the first instance precedes, follows, or occurs in the same position in the sort
			/// order as the second object.
			/// </remarks>
			/// <param name="x">The first <see cref="ReceivedClause"/> instance.</param>
			/// <param name="y">The second <see cref="ReceivedClause"/> instance.</param>
			/// <returns>A value less than zero if the first instance precedes the second in the sort order;
			/// zero if the first instance occurs in the same position as the second; a value greater than
			/// zero if the first instance follows the second in the sort order.</returns>
			public int Compare (ReceivedClause? x, ReceivedClause? y)
			{
				if (x == null)
					return -1;

				if (y == null)
					return 1;

				return ((int) x.Id) - ((int) y.Id);
			}
		}

		void AddOrUpdateComment (ReceivedClauseId id, string? comment)
		{
			var clause = clauses.FirstOrDefault (clause => clause.Id == id);

			if (comment != null)
				comment = Unfold (comment);

			if (clause != null) {
				clause.Comments.Clear ();

				if (comment != null)
					clause.Comments.Add (comment);
			} else if (comment != null) {
				// Note: use the same .ctor as the parser so that the value is not validated.
				clause = new ReceivedClause (id, Keywords[(int) id - 1], string.Empty, new List<string> () { comment });
				clauses.Add (clause);
				clauses.Sort (ReceivedClauseComparer.Instance);
			}
		}

		void AddOrUpdateValue (ReceivedClauseId id, string value)
		{
			var clause = clauses.FirstOrDefault (clause => clause.Id == id);

			if (clause != null) {
				clause.Value = value;
			} else {
				clause = new ReceivedClause (id, Keywords[(int) id - 1], value);
				clauses.Add (clause);
				clauses.Sort (ReceivedClauseComparer.Instance);
			}
		}

		void Remove (ReceivedClauseId id)
		{
			for (int index = 0; index < clauses.Count; index++) {
				if (clauses[index].Id == id) {
					clauses.RemoveAt (index);
					break;
				}
			}
		}

		/// <summary>
		/// Get or set the name of the source host.
		/// </summary>
		/// <remarks>
		/// Gets or sets the "from" clause which identifies the name of the sending host as presented
		/// in the <c>HELO</c> or <c>EHLO</c> SMTP command. This may be a domain name or an address literal
		/// (e.g., <c>[192.168.1.1]</c>).
		/// </remarks>
		/// <value>The name of the source host if available; otherwise, <see langword="null"/>.</value>
		/// <exception cref="ArgumentException">
		/// The value is not a valid domain name or address literal.
		/// </exception>
		public string? From {
			get { return clauses.FirstOrDefault (clause => clause.Id == ReceivedClauseId.From)?.Value; }
			set {
				if (value == null) {
					Remove (ReceivedClauseId.From);
				} else {
					AddOrUpdateValue (ReceivedClauseId.From, value);
				}
			}
		}

		/// <summary>
		/// Get or set the TCP connection information for the source host.
		/// </summary>
		/// <remarks>
		/// Gets or sets the TCP connection information, typically provided as a comment following the "from" clause.
		/// This often contains details such as the IP address and connection details of the sending host.
		/// </remarks>
		/// <value>The TCP connection information for the source host if available; otherwise, <see langword="null"/>.</value>
		public string? FromTcpInfo {
			get {
				var clause = clauses.FirstOrDefault (clause => clause.Id == ReceivedClauseId.From);

				if (clause != null && clause.Comments.Count > 0)
					return clause.Comments[0];

				return null;
			}
			set {
				AddOrUpdateComment (ReceivedClauseId.From, value);
			}
		}

		/// <summary>
		/// Get or set the name of the receiving host.
		/// </summary>
		/// <remarks>
		/// Gets or sets the "by" clause which identifies the name of the receiving SMTP server that
		/// accepted the message. This may be a domain name or an address literal (e.g., <c>[192.168.1.1]</c>).
		/// </remarks>
		/// <value>The name of the receiving host if available; otherwise, <see langword="null"/>.</value>
		/// <exception cref="ArgumentException">
		/// The value is not a valid domain name or address literal.
		/// </exception>
		public string? By {
			get { return clauses.FirstOrDefault (clause => clause.Id == ReceivedClauseId.By)?.Value; }
			set {
				if (value == null) {
					Remove (ReceivedClauseId.By);
				} else {
					AddOrUpdateValue (ReceivedClauseId.By, value);
				}
			}
		}

		/// <summary>
		/// Get or set the TCP connection information for the receiving host.
		/// </summary>
		/// <remarks>
		/// Gets or sets the TCP connection information, typically provided as a comment following the "by" clause.
		/// This often contains details such as the IP address and connection details of the receiving host.
		/// </remarks>
		/// <value>The TCP connection information for the receiving host if available; otherwise, <see langword="null"/>.</value>
		public string? ByTcpInfo {
			get {
				var clause = clauses.FirstOrDefault (clause => clause.Id == ReceivedClauseId.By);

				if (clause != null && clause.Comments.Count > 0)
					return clause.Comments[0];

				return null;
			}
			set {
				AddOrUpdateComment (ReceivedClauseId.By, value);
			}
		}

		/// <summary>
		/// Get or set the physical link or protocol used to receive the message.
		/// </summary>
		/// <remarks>
		/// Gets or sets the "via" clause which indicates the physical path or link (e.g., "TCP", "UUCP")
		/// through which the message was received.
		/// </remarks>
		/// <value>The physical link identifier if available; otherwise, <see langword="null"/>.</value>
		/// <exception cref="ArgumentException">
		/// The value is not a valid atom token.
		/// </exception>
		public string? Via {
			get { return clauses.FirstOrDefault (clause => clause.Id == ReceivedClauseId.Via)?.Value; }
			set {
				if (value == null) {
					Remove (ReceivedClauseId.Via);
				} else {
					AddOrUpdateValue (ReceivedClauseId.Via, value);
				}
			}
		}

		/// <summary>
		/// Get or set the protocol or mechanism used to receive the message.
		/// </summary>
		/// <remarks>
		/// Gets or sets the "with" clause which indicates the mail protocol (e.g., "SMTP", "ESMTP", etc)
		/// used during the message transmission.
		/// </remarks>
		/// <value>The protocol identifier if available; otherwise, <see langword="null"/>.</value>
		/// <exception cref="ArgumentException">
		/// The value is not a valid atom token.
		/// </exception>
		public string? With {
			get { return clauses.FirstOrDefault (clause => clause.Id == ReceivedClauseId.With)?.Value; }
			set {
				if (value == null) {
					Remove (ReceivedClauseId.With);
				} else {
					AddOrUpdateValue (ReceivedClauseId.With, value);
				}
			}
		}

		/// <summary>
		/// Get or set the message identifier.
		/// </summary>
		/// <remarks>
		/// Gets or sets the "id" clause which contains a unique identifier for the message. This value is
		/// typically the value of the Message-ID header.
		/// </remarks>
		/// <value>The message identifier if available; otherwise, <see langword="null"/>.</value>
		/// <exception cref="ArgumentException">
		/// The value is not a valid message identifier.
		/// </exception>
		public string? Id {
			get { return clauses.FirstOrDefault (clause => clause.Id == ReceivedClauseId.Id)?.Value; }
			set {
				if (value == null) {
					Remove (ReceivedClauseId.Id);
				} else {
					AddOrUpdateValue (ReceivedClauseId.Id, value);
				}
			}
		}

		/// <summary>
		/// Get or set the recipient address for which the message was received.
		/// </summary>
		/// <remarks>
		/// Gets or sets the "for" clause which specifies the recipient mailbox address for which the message
		/// was accepted. This is particularly useful when a single message has multiple recipients.
		/// </remarks>
		/// <value>The recipient address if available; otherwise, <see langword="null"/>.</value>
		/// <exception cref="ArgumentException">
		/// The value is not a valid mailbox address.
		/// </exception>
		public string? For {
			get { return clauses.FirstOrDefault (clause => clause.Id == ReceivedClauseId.For)?.Value; }
			set {
				if (value == null) {
					Remove (ReceivedClauseId.For);
				} else {
					AddOrUpdateValue (ReceivedClauseId.For, value);
				}
			}
		}

		#endregion Convenience Properties

		static int GetCommentLength (string comment)
		{
			int index = comment.IndexOfAny (CommentSpecials);
			int specials = 0;

			if (index != -1) {
				for (int i = index; i < comment.Length; i++) {
					if (CommentSpecials.Contains (comment[i]))
						specials++;
				}
			}

			return comment.Length + specials + 2;
		}

		static string FormatComment (string comment)
		{
			int index = comment.IndexOfAny (CommentSpecials);

			if (index == -1)
				return $"({comment})";

			using var builder = new ValueStringBuilder (GetCommentLength (comment));
			int startIndex = 0;

			try {
				builder.Append ('(');

				do {
					builder.Append (comment.AsSpan (startIndex, index - startIndex));
					builder.Append ('\\');
					builder.Append (comment[index++]);

					startIndex = index;
					index = comment.IndexOfAny (CommentSpecials, startIndex);
				} while (index != -1);

				builder.Append (comment.AsSpan (startIndex, comment.Length - startIndex));
				builder.Append (')');

				return builder.ToString ();
			} finally {
				builder.Dispose ();
			}
		}

		static void AppendClause (FormatOptions options, StringBuilder builder, ref int lineLength, ReceivedClause clause)
		{
			if (clause.Id != ReceivedClauseId.None) {
				int commentLength = clause.Comments.Count > 0 ? GetCommentLength (clause.Comments[0]) : 0;
				var keyword = clause.Keyword;
				var value = clause.Value;

				if (commentLength > 0 && keyword.Length + 1 + value.Length + 1 + commentLength < options.MaxLineLength) {
					// keyword, value and (the first) comment can all fit on a single line
					if (lineLength + 1 + keyword.Length + 1 + value.Length + 1 + commentLength > options.MaxLineLength) {
						// fold the header value here so that we can put as many tokens on the same line as we can
						builder.Append (options.NewLine);
						builder.Append ('\t');
						lineLength = 1;
					} else {
						// they can all fit on the current line
						builder.Append (' ');
						lineLength++;
					}
				} else if (keyword.Length + 1 + value.Length < options.MaxLineLength) {
					// keyword and value can both fit on a single line
					if (lineLength + 1 + keyword.Length + 1 + value.Length > options.MaxLineLength) {
						// they are too long to fit on the current line, so wrap and put them on the next line
						builder.Append (options.NewLine);
						builder.Append ('\t');
						lineLength = 1;
					} else {
						// they can both fit on the current line
						builder.Append (' ');
						lineLength++;
					}
				} else {
					// we'll need to separate the keyword and value
					if (lineLength + 1 + keyword.Length > options.MaxLineLength) {
						// keyword is too long to fit on the current line
						builder.Append (options.NewLine);
						builder.Append ('\t');
						lineLength = 1;
					} else {
						// keyword fit on the current line
						builder.Append (' ');
						lineLength++;
					}
				}

				// append the keyword
				builder.Append (keyword);
				lineLength += keyword.Length;

				if (lineLength + 1 + value.Length > options.MaxLineLength) {
					builder.Append (options.NewLine);
					builder.Append ('\t');
					lineLength = 1;
				} else {
					builder.Append (' ');
					lineLength++;
				}

				// append the value
				builder.Append (value);
				lineLength += value.Length;
			}

			for (int i = 0; i < clause.Comments.Count; i++) {
				var comment = FormatComment (clause.Comments[i]);

				if (lineLength + 1 + comment.Length > options.MaxLineLength) {
					builder.Append (options.NewLine);
					builder.Append ('\t');
					lineLength = 1;
				} else {
					builder.Append (' ');
					lineLength++;
				}

				builder.Append (comment);
				lineLength += comment.Length;
			}
		}

		/// <summary>
		/// Serializes the <see cref="Received"/> value to a string.
		/// </summary>
		/// <remarks>
		/// Creates a string-representation of the <see cref="Received"/> value using the provided formatting options.
		/// </remarks>
		/// <returns>The serialized string.</returns>
		/// <param name="options">The formatting options.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="options"/> is <see langword="null"/>.
		/// </exception>
		public string ToString (FormatOptions options)
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			StringBuilder builder = new StringBuilder ();
			int lineLength = "Received:".Length;

			foreach (var clause in clauses) {
				if (clause == null)
					continue;

				AppendClause (options, builder, ref lineLength, clause);
			}

			if (dateTime != null) {
				if (lineLength + 1 > options.MaxLineLength) {
					builder.Append (options.NewLine);
					builder.Append ('\t');
					lineLength = 1;
				}

				builder.Append (';');
				lineLength++;

				// use the cached value if available (so when reformatting a parsed header, we keep it in tact)
				var stamp = dateTimeStr ?? DateUtils.FormatDate (dateTime.Value);

				if (lineLength + 1 + stamp.Length > options.MaxLineLength) {
					builder.Append (options.NewLine);
					builder.Append ('\t');
					lineLength = 1;
				} else {
					builder.Append (' ');
					lineLength++;
				}

				builder.Append (stamp);
			}

			builder.Append (options.NewLine);

			return builder.ToString ();
		}

		/// <summary>
		/// Serializes the <see cref="Received"/> value to a string.
		/// </summary>
		/// <remarks>
		/// Creates a string-representation of the <see cref="Received"/> value.
		/// </remarks>
		/// <returns>The serialized string.</returns>
		public override string ToString ()
		{
			return ToString (FormatOptions.Default);
		}

		static string ParseKeyword (ParserOptions options, byte[] rawValue, ref int index, int endIndex)
		{
			var span = rawValue.AsSpan (index, endIndex - index);
			int endOfValue = span.IndexOfAny (EndOfTokenSentinels);
			int length = endOfValue == -1 ? span.Length : endOfValue;

			var keyword = CharsetUtils.ConvertToUnicode (options, rawValue, index, length);
			index += length;

			return keyword;
		}

		static bool TryParseValue (ParserOptions options, byte[] rawValue, ref int index, int endIndex, [NotNullWhen (true)] out string? value)
		{
			var span = rawValue.AsSpan (index, endIndex - index);
			int endOfValue = span.IndexOfAny (EndOfTokenSentinels);
			int length = endOfValue == -1 ? span.Length : endOfValue;
			var token = new ReadOnlySpan<byte> (rawValue, index, length);

			if (token.Equals ("from"u8, StringComparison.OrdinalIgnoreCase) ||
				token.Equals ("by"u8, StringComparison.OrdinalIgnoreCase) ||
				token.Equals ("via"u8, StringComparison.OrdinalIgnoreCase) ||
				token.Equals ("with"u8, StringComparison.OrdinalIgnoreCase) ||
				token.Equals ("id"u8, StringComparison.OrdinalIgnoreCase) ||
				token.Equals ("for"u8, StringComparison.OrdinalIgnoreCase)) {
				value = null;
				return false;
			}

			value = CharsetUtils.ConvertToUnicode (options, rawValue, index, length);
			index += length;

			return true;
		}

		static bool TryParseComment (ParserOptions options, byte[] rawValue, ref int index, int endIndex, bool throwOnError, [NotNullWhen (true)] out string? comment)
		{
			int startIndex = index;

			// skip over the '('
			index++;

			using (var builder = new ByteArrayBuilder (64)) {
				bool escaped = false;
				int depth = 1;

				while (index < endIndex) {
					byte c = rawValue[index];

					if (escaped) {
						if (!c.IsWhitespace ())
							builder.Append (c);
						else if (c != (byte) '\r' && c != (byte) '\n')
							builder.Append ((byte) ' ');

						escaped = false;
					} else if (c == (byte) '\\') {
						// escape sequence
						escaped = true;
					} else if (c == (byte) '(') {
						builder.Append (c);
						depth++;
					} else if (c == (byte) ')') {
						if (--depth == 0)
							break;

						builder.Append ((byte) ')');
					} else {
						if (!c.IsWhitespace ())
							builder.Append (c);
						else if (c != (byte) '\r' && c != (byte) '\n')
							builder.Append ((byte) ' ');
					}

					index++;
				}

				if (index >= endIndex) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete comment token at offset {0}", startIndex), startIndex, index);

					comment = null;
					return false;
				}

				comment = builder.ToString (options);

				// skip over ')'
				index++;

				return true;
			}
		}

		static bool UpdateState (ref int mask, string keyword, int keywordIndex, int index, bool throwOnError, out ReceivedClauseId id)
		{
			for (int i = 0; i < Keywords.Length; i++) {
				if (keyword.Equals (Keywords[i], StringComparison.OrdinalIgnoreCase)) {
					id = (ReceivedClauseId) i + 1;

					int bit = 1 << (int) id;

					if ((mask & bit) != 0) {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Duplicate '{0}' clause at offset {1}", keyword.ToLowerInvariant (), keywordIndex), keywordIndex, index);

						return false;
					}

					mask |= bit;

					return true;
				}
			}

			id = ReceivedClauseId.Unknown;

			return true;
		}

		static bool TryParse (ParserOptions options, byte[] rawValue, int startIndex, int length, bool throwOnError, out Received? received)
		{
			var clauses = new List<ReceivedClause> ();
			ReceivedClauseId id = ReceivedClauseId.None;
			ReceivedClause? clause = null;
			int endIndex = startIndex + length;
			int index = startIndex;
			int clauseMask = 0;

			received = null;

			// skip over any leading whitespace, collecting any comments along the way
			while (index < endIndex) {
				ParseUtils.SkipWhiteSpace (rawValue, ref index, endIndex);

				if (index >= endIndex || rawValue[index] != (byte) '(')
					break;

				if (!TryParseComment (options, rawValue, ref index, endIndex, throwOnError, out string? comment))
					return false;

				if (clause == null) {
					clause = new ReceivedClause (ReceivedClauseId.None, string.Empty, string.Empty, new List<string> ());
					clauses.Add (clause);
				}

				clause.Comments.Add (comment);
			}

			// continue parsing clauses until we reach a ';' or the end of the header value
			while (index < endIndex && rawValue[index] != (byte) ';') {
				int keywordIndex = index;

				// parse the keyword token
				var keyword = ParseKeyword (options, rawValue, ref index, endIndex);

				// update our parser state and check for duplicate clauses
				if (!UpdateState (ref clauseMask, keyword, keywordIndex, index, throwOnError, out id))
					return false;

				var comments = new List<string> ();

				// skip over any whitespace, collecting any comments along the way...
				while (index < endIndex) {
					ParseUtils.SkipWhiteSpace (rawValue, ref index, endIndex);

					if (index >= endIndex || rawValue[index] != (byte) '(')
						break;

					if (!TryParseComment (options, rawValue, ref index, endIndex, throwOnError, out string? comment))
						return false;

					comments.Add (comment);
				}

				// check if we've reached the end of the header -or- we've reached a ';'
				if (index >= endIndex || rawValue[index] == (byte) ';') {
					clause = new ReceivedClause (id, keyword, string.Empty, comments);
					clauses.Add (clause);
					break;
				}

				// parse the value token
				if (!TryParseValue (options, rawValue, ref index, endIndex, out var value)) {
					// Note: If TryParseValue returns false, then it means we've encountered what looks like the next keyword...
					clause = new ReceivedClause (id, keyword, string.Empty, comments);
					clauses.Add (clause);
					continue;
				}

				// Note: some servers like to use multi-string values (e.g. "with Microsoft SMTP Server" or "via Frontend Transport")
				StringBuilder? builder = null;

				do {
					// skip over any whitespace after the value token
					ParseUtils.SkipWhiteSpace (rawValue, ref index, endIndex);

					// break out of the loop if we've reached the end of the header or we've reached ';' or the start of a comment
					if (index >= endIndex || rawValue[index] == (byte) ';' || rawValue[index] == (byte) '(')
						break;

					// check to make sure this next token isn't a keyword
					if (!TryParseValue (options, rawValue, ref index, endIndex, out var token))
						break;

					// append the token to our value
					builder ??= new StringBuilder (value);
					builder.Append (' ');
					builder.Append (token);
				} while (true);

				if (builder != null)
					value = builder.ToString ();

				// parse any comments that exist after the value...
				while (index < endIndex) {
					ParseUtils.SkipWhiteSpace (rawValue, ref index, endIndex);

					if (index >= endIndex || rawValue[index] != (byte) '(')
						break;

					if (!TryParseComment (options, rawValue, ref index, endIndex, throwOnError, out string? comment))
						return false;

					comments.Add (comment);
				}

				clause = new ReceivedClause (id, keyword, value, comments);
				clauses.Add (clause);
			}

			if (index < endIndex && rawValue[index] == (byte) ';') {
				index++;

				ParseUtils.SkipWhiteSpace (rawValue, ref index, endIndex);

				int dateEnd = endIndex;

				// trim trailing whitespace
				while (dateEnd > index && rawValue[dateEnd - 1].IsWhitespace ())
					dateEnd--;

				DateTimeOffset? dateTime = null;
				string? dateTimeStr = null;

				if (index < dateEnd) {
					if (!DateUtils.TryParse (rawValue, index, dateEnd - index, out var dto)) {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid date-time format at offset {0}", index), index, dateEnd);

						return false;
					}

					// cache the formatting of the date
					dateTimeStr = CharsetUtils.ConvertToUnicode (options, rawValue, index, dateEnd - index);
					dateTimeStr = Unfold (dateTimeStr);
					dateTime = dto;
				}

				received = new Received (clauses, dateTime, dateTimeStr);
			} else {
				received = new Received (clauses, null, null);
			}

			return true;
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="Received"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Received header value from the supplied buffer starting at the given index
		/// and spanning across the specified number of bytes.
		/// </remarks>
		/// <returns><see langword="true" /> if the Received header value was successfully parsed; otherwise, <see langword="false" />.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="received">The parsed Received header value.</param>
		public static bool TryParse (ParserOptions? options, byte[]? buffer, int startIndex, int length, [NotNullWhen (true)] out Received? received)
		{
			if (!ArgumentValidator.TryValidate (options, buffer, startIndex, length)) {
				received = null;
				return false;
			}

			return TryParse (options, buffer, startIndex, length, false, out received);
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="Received"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Received header value from the supplied buffer starting at the given index
		/// and spanning across the specified number of bytes.
		/// </remarks>
		/// <returns><see langword="true" /> if the Received header value was successfully parsed; otherwise, <see langword="false" />.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="received">The parsed Received header value.</param>
		public static bool TryParse (byte[]? buffer, int startIndex, int length, [NotNullWhen (true)] out Received? received)
		{
			return TryParse (ParserOptions.Default, buffer, startIndex, length, out received);
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="Received"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Received header value from the supplied buffer.
		/// </remarks>
		/// <returns><see langword="true" /> if the Received header value was successfully parsed; otherwise, <see langword="false" />.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="received">The parsed Received header value.</param>
		public static bool TryParse (ParserOptions? options, byte[]? buffer, [NotNullWhen (true)] out Received? received)
		{
			if (!ArgumentValidator.TryValidate (options, buffer)) {
				received = null;
				return false;
			}

			return TryParse (options, buffer, 0, buffer.Length, false, out received);
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="Received"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Received header value from the supplied buffer.
		/// </remarks>
		/// <returns><see langword="true" /> if the Received header value was successfully parsed; otherwise, <see langword="false" />.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="received">The parsed Received header value.</param>
		public static bool TryParse (byte[]? buffer, [NotNullWhen (true)] out Received? received)
		{
			return TryParse (ParserOptions.Default, buffer, out received);
		}

		/// <summary>
		/// Parse the specified input buffer into a new <see cref="Received"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Received header value from the supplied buffer starting at the given index
		/// and spanning across the specified number of bytes.
		/// </remarks>
		/// <returns>The parsed <see cref="Received"/> instance.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <exception cref="ArgumentNullException">
		/// <para><paramref name="options"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		/// <exception cref="ParseException">
		/// The <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static Received Parse (ParserOptions options, byte[] buffer, int startIndex, int length)
		{
			ArgumentValidator.Validate (options, buffer, startIndex, length);

			TryParse (options, buffer, startIndex, length, true, out var received);

			return received!;
		}

		/// <summary>
		/// Parse the specified input buffer into a new <see cref="Received"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Received header value from the supplied buffer starting at the given index
		/// and spanning across the specified number of bytes.
		/// </remarks>
		/// <returns>The parsed <see cref="Received"/> instance.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="buffer"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		/// <exception cref="ParseException">
		/// The <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static Received Parse (byte[] buffer, int startIndex, int length)
		{
			return Parse (ParserOptions.Default, buffer, startIndex, length);
		}

		/// <summary>
		/// Parse the specified input buffer into a new <see cref="Received"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Received header value from the supplied buffer.
		/// </remarks>
		/// <returns>The parsed <see cref="Received"/> instance.</returns>
		/// <param name="options">The parser options.</param>
		/// <param name="buffer">The input buffer.</param>
		/// <exception cref="ArgumentNullException">
		/// <para><paramref name="options"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="buffer"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="ParseException">
		/// The <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static Received Parse (ParserOptions options, byte[] buffer)
		{
			ArgumentValidator.Validate (options, buffer);

			TryParse (options, buffer, 0, buffer.Length, true, out var received);

			return received!;
		}

		/// <summary>
		/// Parse the specified input buffer into a new <see cref="Received"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a Received header value from the supplied buffer.
		/// </remarks>
		/// <returns>The parsed <see cref="Received"/> instance.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="buffer"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ParseException">
		/// The <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static Received Parse (byte[] buffer)
		{
			return Parse (ParserOptions.Default, buffer);
		}
	}

	/// <summary>
	/// A parsed representation of a clause in a <c>Received</c> header.
	/// </summary>
	/// <remarks>
	/// Each clause in a <c>Received</c> header is made up of a keyword (such as "from", "by",
	/// "via", "with", "id", or "for") and a corresponding value. A clause may also include a
	/// comment (or, as with some non-compliant mail software, a series of comments).
	/// </remarks>
	public class ReceivedClause
	{
		static ReadOnlySpan<char> Specials => new char[] { '(', ')', ';' };

		string value;

		// Note: This .ctor is used by Received.AddOrUpdateValue() and therefore needs to validate the value.
		internal ReceivedClause (ReceivedClauseId id, string keyword, string value)
		{
			Id = id;
			Comments = new List<string> ();
			Keyword = keyword;
			Value = value;
		}

		// Note: This .ctor is used by the Received .ctor that takes from/by and tcpInfos.
		internal ReceivedClause (ReceivedClauseId id, string keyword, string value, string comment)
		{
			Id = id;
			Comments = new List<string> () { comment };
			Keyword = keyword;
			Value = value;
		}

		// Note: This is the only .ctor used by the parser and therefore should not validate the value.
		internal ReceivedClause (ReceivedClauseId id, string keyword, string value, List<string> comments)
		{
			Id = id;
			Comments = comments;
			Keyword = keyword;
			this.value = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ReceivedClause"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="ReceivedClause"/> instance  with the specified keyword and value.
		/// </remarks>
		/// <param name="keyword">The keyword that identifies the clause.</param>
		/// <param name="value">The value associated with the keyword.</param>
		/// <exception cref="ArgumentNullException">
		/// <para><paramref name="keyword"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <para><paramref name="keyword"/> is invalid.</para>
		/// </exception>
		public ReceivedClause (string keyword, string value)
		{
			Id = ValidateKeyword (keyword);
			Keyword = keyword;
			Value = value;

			Comments = new List<string> ();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ReceivedClause"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="ReceivedClause"/> instance  with the specified keyword, value and comment.
		/// </remarks>
		/// <param name="keyword">The keyword that identifies the clause.</param>
		/// <param name="value">The value associated with the keyword.</param>
		/// <param name="comment">The comment associated with the value.</param>
		/// <exception cref="ArgumentNullException">
		/// <para><paramref name="keyword"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="comment"/> is <see langword="null"/>.</para>
		/// </exception>
		public ReceivedClause (string keyword, string value, string comment) : this (keyword, value)
		{
			if (comment == null)
				throw new ArgumentNullException (nameof (comment));

			Comments.Add (comment);
		}

		/// <summary>
		/// Get the identifier for the clause.
		/// </summary>
		/// <remarks>
		/// <para>Gets the identifier for the clause.</para>
		/// <para>This property is mainly used for switch-statements for performance reasons.</para>
		/// </remarks>
		/// <value>The clause identifier.</value>
		public ReceivedClauseId Id { get; private set; }

		/// <summary>
		/// Get the keyword used to identify the clause.
		/// </summary>
		/// <remarks>
		/// Gets the keyword used to identify the clause.
		/// </remarks>
		/// <value>The keyword used to identify the clause.</value>
		public string Keyword { get; private set; }

		/// <summary>
		/// Get or set the value for the clause.
		/// </summary>
		/// <remarks>
		/// Gets or sets the value for the clause.
		/// </remarks>
		/// <value>The value for the clause.</value>
		/// <exception cref="ArgumentNullException">
		/// The value is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The value is invalid.
		/// </exception>
		public string Value {
			get { return value; }

			[MemberNotNull (nameof (value))]
			set {
				ValidateValue (Id, value);
				this.value = value;
			}
		}

		/// <summary>
		/// Get the collection of comments associated with the clause.
		/// </summary>
		/// <remarks>
		/// Gets the collection of comments associated with the clause.
		/// </remarks>
		/// <value>The collection of comments.</value>
		public IList<string> Comments { get; private set; }

		static ReceivedClauseId ValidateKeyword (string keyword)
		{
			if (keyword == null)
				throw new ArgumentNullException (nameof (keyword));

			if (keyword.Length == 0)
				throw new ArgumentException ("Keyword cannot be an empty string.", nameof (keyword));

			if (keyword[0] == '-')
				throw new ArgumentException ("Invalid keyword.", nameof (keyword));

			for (int i = 0; i < keyword.Length; i++) {
				if ((keyword[i] >= 'A' && keyword[i] <= 'Z') ||
					(keyword[i] >= 'a' && keyword[i] <= 'z') ||
					keyword[i] == '-')
					continue;

				throw new ArgumentException ("Invalid keyword.", nameof (keyword));
			}

			for (int i = 0; i < Received.Keywords.Length; i++) {
				if (keyword.Equals (Received.Keywords[i], StringComparison.OrdinalIgnoreCase))
					return (ReceivedClauseId) (i + 1);
			}

			return ReceivedClauseId.Unknown;
		}

		static void ValidateValue (ReceivedClauseId id, string value)
		{
			if (value == null)
				throw new ArgumentNullException (nameof (value));

			if (value.Length == 0)
				throw new ArgumentException ($"The value cannot be an empty string.", nameof (value));

			switch (id) {
			case ReceivedClauseId.From:
			case ReceivedClauseId.By:
				ValidateDomain (value);
				break;
			case ReceivedClauseId.Id:
				var buffer = Encoding.UTF8.GetBytes (value);
				int index = 0;

				try {
					ParseUtils.TryParseMsgId (buffer, ref index, buffer.Length, false, true, out _);
				} catch (ParseException ex) {
					throw new ArgumentException ("Invalid message identifier.", nameof (value), ex);
				}
				break;
			case ReceivedClauseId.For:
				try {
					// Note: validate the value by parsing as a mailbox address
					MailboxAddress.Parse (value);
				} catch (ParseException ex) {
					throw new ArgumentException ("Invalid mailbox address.", nameof (value), ex);
				}
				break;
			default:
				ValidateValue (value);
				break;
			}
		}

		static void ValidateValue (string value)
		{
			for (int i = 0; i < value.Length; i++) {
				if (value[i] < 32 || value[i] == 127 || Specials.IndexOf (value[i]) != -1)
					throw new ArgumentException ($"The value contains illegal characters.", nameof (value));
			}
		}

		static void ValidateDomain (string value)
		{
			if (value[0] == '[') {
				if (value.Length < 2 || value[value.Length - 1] != ']')
					throw new ArgumentException ("Invalid domain literal.", nameof (value));

				var ipAddress = value.Substring (1, value.Length - 2);
				if (!IPAddress.TryParse (ipAddress, out _))
					throw new ArgumentException ("Invalid domain literal.", nameof (value));
			} else {
				bool dot = true;

				for (int i = 0; i < value.Length; i++) {
					if (value[i] <= 32 || value[i] == 127 || (value[i] > 90 && value[i] < 94))
						throw new ArgumentException ("Invalid domain.", nameof (value));

					if (dot) {
						if (value[i] == '.' || value[i] == '-')
							throw new ArgumentException ("Invalid domain.", nameof (value));

						dot = false;
					} else if (value[i] == '.') {
						dot = true;
					}
				}
			}
		}
	}
}

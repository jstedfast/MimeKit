//
// AuthenticationResults.cs
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
using System.Text;
using System.Globalization;
using System.Collections.Generic;

using MimeKit.Utils;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A parsed representation of the Authentication-Results header.
	/// </summary>
	/// <remarks>
	/// The Authentication-Results header is used with electronic mail messages to
	/// indicate the results of message authentication efforts. Any receiver-side
	/// software, such as mail filters or Mail User Agents (MUAs), can use this header
	/// field to relay that information in a convenient and meaningful way to users or
	/// to make sorting and filtering decisions.
	/// </remarks>
	public class AuthenticationResults
	{
		AuthenticationResults ()
		{
			Results = new List<AuthenticationMethodResult> ();
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="AuthenticationResults"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="AuthenticationResults"/>.
		/// </remarks>
		/// <param name="authservid">The authentication service identifier.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="authservid"/> is <c>null</c>.
		/// </exception>
		public AuthenticationResults (string authservid) : this ()
		{
			if (authservid is null)
				throw new ArgumentNullException (nameof (authservid));

			AuthenticationServiceIdentifier = authservid;
		}

		/// <summary>
		/// Get the authentication service identifier.
		/// </summary>
		/// <remarks>
		/// <para>Gets the authentication service identifier.</para>
		/// <para>The authentication service identifier is the <c>authserv-id</c> token
		/// as defined in <a href="https://tools.ietf.org/html/rfc7601">rfc7601</a>.</para>
		/// </remarks>
		/// <value>The authserv-id token.</value>
		public string AuthenticationServiceIdentifier {
			get; private set;
		}

		/// <summary>
		/// Get or set the instance value.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the instance value.</para>
		/// <para>This value will only be set if the <see cref="AuthenticationResults"/>
		/// represents an ARC-Authentication-Results header value.</para>
		/// </remarks>
		/// <value>The instance.</value>
		public int? Instance {
			get; set;
		}

		/// <summary>
		/// Get or set the Authentication-Results version.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the Authentication-Results version.</para>
		/// <para>The version value is the <c>authres-version</c> token as defined in
		/// <a href="https://tools.ietf.org/html/rfc7601">rfc7601</a>.</para>
		/// </remarks>
		/// <value>The authres-version token.</value>
		public int? Version {
			get; set;
		}

		/// <summary>
		/// Get the list of authentication results.
		/// </summary>
		/// <remarks>
		/// Gets the list of authentication results.
		/// </remarks>
		/// <value>The list of authentication results.</value>
		public List<AuthenticationMethodResult> Results {
			get; private set;
		}

		internal void Encode (FormatOptions options, StringBuilder builder, int lineLength)
		{
			int space = 1;

			if (Instance.HasValue) {
				var i = Instance.Value.ToString (CultureInfo.InvariantCulture);

				builder.AppendFormat (" i={0};", i);
				lineLength += 4 + i.Length;
			}

			if (AuthenticationServiceIdentifier != null) {
				if (lineLength + space + AuthenticationServiceIdentifier.Length > options.MaxLineLength) {
					builder.Append (options.NewLine);
					builder.Append ('\t');
					lineLength = 1;
					space = 0;
				}

				if (space > 0) {
					builder.Append (' ');
					lineLength++;
				}

				builder.Append (AuthenticationServiceIdentifier);
				lineLength += AuthenticationServiceIdentifier.Length;

				if (Version.HasValue) {
					var version = Version.Value.ToString (CultureInfo.InvariantCulture);

					if (lineLength + 1 + version.Length > options.MaxLineLength) {
						builder.Append (options.NewLine);
						builder.Append ('\t');
						lineLength = 1;
					} else {
						builder.Append (' ');
						lineLength++;
					}

					lineLength += version.Length;
					builder.Append (version);
				}

				builder.Append (';');
				lineLength++;
			}

			if (Results.Count > 0) {
				for (int i = 0; i < Results.Count; i++) {
					if (i > 0) {
						builder.Append (';');
						lineLength++;
					}

					Results[i].Encode (options, builder, ref lineLength);
				}
			} else {
				builder.Append (" none");
			}

			builder.Append (options.NewLine);
		}

		/// <summary>
		/// Serializes the <see cref="AuthenticationMethodResult"/> to a string.
		/// </summary>
		/// <remarks>
		/// Creates a string-representation of the <see cref="AuthenticationMethodResult"/>.
		/// </remarks>
		/// <returns>The serialized string.</returns>
		public override string ToString ()
		{
			var builder = new ValueStringBuilder (256);

			WriteTo (ref builder);

			return builder.ToString ();
		}

		internal void WriteTo (ref ValueStringBuilder builder)
		{
			if (Instance.HasValue) {
				builder.Append ("i=");
				builder.AppendInvariant (Instance.Value);
				builder.Append ("; ");
			}

			if (AuthenticationServiceIdentifier != null) {
				builder.Append (AuthenticationServiceIdentifier);

				if (Version.HasValue) {
					builder.Append (' ');
					builder.AppendInvariant (Version.Value);
				}

				builder.Append ("; ");
			}

			if (Results.Count > 0) {
				for (int i = 0; i < Results.Count; i++) {
					if (i > 0)
						builder.Append ("; ");

					Results[i].WriteTo (ref builder);
				}
			} else {
				builder.Append ("none");
			}
		}

		static bool IsKeyword (byte c)
		{
			return (c >= (byte) 'A' && c <= (byte) 'Z') ||
				(c >= (byte) 'a' && c <= (byte) 'z') ||
				(c >= (byte) '0' && c <= (byte) '9') ||
				c == (byte) '-' || c == (byte) '_';
		}

		static bool SkipKeyword (byte[] text, ref int index, int endIndex)
		{
			int startIndex = index;

			while (index < endIndex && IsKeyword (text[index]))
				index++;

			return index > startIndex;
		}

		static bool SkipValue (byte[] text, ref int index, int endIndex, out bool quoted)
		{
			if (text[index] == (byte) '"') {
				quoted = true;

				if (!ParseUtils.SkipQuoted (text, ref index, endIndex, false))
					return false;
			} else {
				quoted = false;

				if (!ParseUtils.SkipToken (text, ref index, endIndex))
					return false;
			}

			return true;
		}

		static bool SkipDomain (byte[] text, ref int index, int endIndex)
		{
			int startIndex = index;

			while (ParseUtils.SkipAtom (text, ref index, endIndex) && index < endIndex && text[index] == (byte) '.')
				index++;

			if (index > startIndex && text[index - 1] != (byte) '.')
				return true;

			return false;
		}

		static bool SkipPropertyValue (byte[] text, ref int index, int endIndex, out bool quoted)
		{
			// pvalue := [CFWS] ( value / [ [ local-part ] "@" ] domain-name ) [CFWS]
			// value  := token / quoted-string
			// token  := 1*<any (US-ASCII) CHAR except SPACE, CTLs, or tspecials>
			// tspecials :=  "(" / ")" / "<" / ">" / "@" / "," / ";" / ":" / "\" / <"> / "/" / "[" / "]" / "?" / "="
			if (text[index] == (byte) '"') {
				// quoted-string
				quoted = true;

				if (!ParseUtils.SkipQuoted (text, ref index, endIndex, false))
					return false;

				return true;
			}

			quoted = false;

			// Note: we're forced to accept even tspecials in the property value because they are used in the real-world.
			// See https://github.com/jstedfast/MimeKit/issues/518 ('/') and https://github.com/jstedfast/MimeKit/issues/590 ('=')
			// for details.
			while (index < endIndex && !text[index].IsWhitespace () && text[index] != (byte) ';' && text[index] != (byte) '(')
				index++;

			return true;
		}

		static bool TryParseMethods (byte[] text, ref int index, int endIndex, bool throwOnError, AuthenticationResults authres)
		{
			string value;
			bool quoted;

			while (index < endIndex) {
				string srvid = null;

			method_token:
				if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;

				if (index >= endIndex)
					break;

				int methodIndex = index;

				// skip the method name
				if (!SkipKeyword (text, ref index, endIndex)) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid method token at offset {0}", methodIndex), methodIndex, index);

					return false;
				}

				// Note: Office365 seems to (sometimes) place a method-specific authserv-id token before each
				// method. This block of code is here to handle that case.
				//
				// See https://github.com/jstedfast/MimeKit/issues/527 for details.
				if (srvid is null && index < endIndex && text[index] == '.') {
					index = methodIndex;

					if (!SkipDomain (text, ref index, endIndex)) {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid Office365 authserv-id token at offset {0}", methodIndex), methodIndex, index);

						return false;
					}

					srvid = Encoding.UTF8.GetString (text, methodIndex, index - methodIndex);

					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;

					if (index >= endIndex) {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Missing semi-colon after Office365 authserv-id token at offset {0}", methodIndex), methodIndex, index);

						return false;
					}

					if (text[index] != (byte) ';') {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Unexpected token after Office365 authserv-id token at offset {0}", index), index, index);

						return false;
					}

					// skip over ';'
					index++;

					goto method_token;
				}

				var method = Encoding.ASCII.GetString (text, methodIndex, index - methodIndex);

				if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;

				if (index >= endIndex) {
					if (method != "none") {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete methodspec token at offset {0}", methodIndex), methodIndex, index);

						return false;
					}

					if (authres.Results.Count > 0) {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid no-result token at offset {0}", methodIndex), methodIndex, index);

						return false;
					}

					break;
				}

				var resinfo = new AuthenticationMethodResult (method) {
					Office365AuthenticationServiceIdentifier = srvid
				};
				authres.Results.Add (resinfo);

				int tokenIndex;

				if (text[index] == (byte) '/') {
					// optional method-version token
					index++;

					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;

					tokenIndex = index;

					if (!ParseUtils.TryParseInt32 (text, ref index, endIndex, out int version)) {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid method-version token at offset {0}", tokenIndex), tokenIndex, index);

						return false;
					}

					resinfo.Version = version;

					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;

					if (index >= endIndex) {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete methodspec token at offset {0}", methodIndex), methodIndex, index);

						return false;
					}
				}

				if (text[index] != (byte) '=') {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid methodspec token at offset {0}", methodIndex), methodIndex, index);

					return false;
				}

				// skip over '='
				index++;

				if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;

				if (index >= endIndex) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete methodspec token at offset {0}", methodIndex), methodIndex, index);

					return false;
				}

				tokenIndex = index;

				if (!SkipKeyword (text, ref index, endIndex)) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid result token at offset {0}", tokenIndex), tokenIndex, index);

					return false;
				}

				resinfo.Result = Encoding.ASCII.GetString (text, tokenIndex, index - tokenIndex);

				ParseUtils.SkipWhiteSpace (text, ref index, endIndex);

				if (index < endIndex && text[index] == (byte) '(') {
					int commentIndex = index;

					if (!ParseUtils.SkipComment (text, ref index, endIndex)) {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete comment token at offset {0}", commentIndex), commentIndex, index);

						return false;
					}

					commentIndex++;

					resinfo.ResultComment = Header.Unfold (Encoding.UTF8.GetString (text, commentIndex, (index - 1) - commentIndex));

					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;
				}

				if (index >= endIndex)
					break;

				if (text[index] == (byte) ';') {
					index++;
					continue;
				}

				// optional reasonspec or propspec
				tokenIndex = index;

				if (!SkipKeyword (text, ref index, endIndex)) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid reasonspec or propspec token at offset {0}", tokenIndex), tokenIndex, index);

					return false;
				}

				value = Encoding.ASCII.GetString (text, tokenIndex, index - tokenIndex);

				if (value == "reason" || value == "action") {
					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;

					if (index >= endIndex) {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete {0}spec token at offset {1}", value, tokenIndex), tokenIndex, index);

						return false;
					}

					if (text[index] != (byte) '=') {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid {0}spec token at offset {1}", value, tokenIndex), tokenIndex, index);

						return false;
					}

					index++;

					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;

					int reasonIndex = index;

					if (index >= endIndex || !SkipValue (text, ref index, endIndex, out quoted)) {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid {0}spec value token at offset {1}", value, reasonIndex), reasonIndex, index);

						return false;
					}

					var reason = Encoding.UTF8.GetString (text, reasonIndex, index - reasonIndex);

					if (quoted)
						reason = MimeUtils.Unquote (reason, true);

					if (value == "action")
						resinfo.Action = reason;
					else
						resinfo.Reason = reason;

					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;

					if (index >= endIndex)
						break;

					if (text[index] == (byte) ';') {
						index++;
						continue;
					}

					// optional propspec
					tokenIndex = index;

					if (!SkipKeyword (text, ref index, endIndex)) {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid propspec token at offset {0}", tokenIndex), tokenIndex, index);

						return false;
					}

					value = Encoding.ASCII.GetString (text, tokenIndex, index - tokenIndex);
				}

				do {
					// value is a propspec ptype token
					var ptype = value;

					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;

					if (index >= endIndex) {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete propspec token at offset {0}", tokenIndex), tokenIndex, index);

						return false;
					}

					if (text[index] != (byte) '.') {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid propspec token at offset {0}", tokenIndex), tokenIndex, index);

						return false;
					}

					index++;

					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;

					if (index >= endIndex) {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete propspec token at offset {0}", tokenIndex), tokenIndex, index);

						return false;
					}

					int propertyIndex = index;

					if (!SkipKeyword (text, ref index, endIndex)) {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid property token at offset {0}", propertyIndex), propertyIndex, index);

						return false;
					}

					var property = Encoding.ASCII.GetString (text, propertyIndex, index - propertyIndex);

					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;

					if (index >= endIndex) {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete propspec token at offset {0}", tokenIndex), tokenIndex, index);

						return false;
					}

					if (text[index] != (byte) '=') {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid propspec token at offset {0}", tokenIndex), tokenIndex, index);

						return false;
					}

					index++;

					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;

					int valueIndex = index;

					if (index >= text.Length || !SkipPropertyValue (text, ref index, endIndex, out quoted)) {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete propspec token at offset {0}", tokenIndex), tokenIndex, index);

						return false;
					}

					value = Encoding.UTF8.GetString (text, valueIndex, index - valueIndex);

					if (quoted)
						value = MimeUtils.Unquote (value, true);

					var propspec = new AuthenticationMethodProperty (ptype, property, value, quoted);
					resinfo.Properties.Add (propspec);

					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;

					if (index >= endIndex || text[index] == (byte) ';')
						break;

					tokenIndex = index;

					if (!SkipKeyword (text, ref index, endIndex)) {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid propspec token at offset {0}", tokenIndex), tokenIndex, index);

						return false;
					}

					value = Encoding.ASCII.GetString (text, tokenIndex, index - tokenIndex);
				} while (true);

				// skip over ';'
				index++;
			}

			return true;
		}

		static bool TryParse (byte[] text, ref int index, int endIndex, bool throwOnError, out AuthenticationResults authres)
		{
			int? instance = null;
			string srvid = null;
			string value;

			authres = null;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			do {
				int start = index;

				if (index >= endIndex || !SkipValue (text, ref index, endIndex, out bool quoted)) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete authserv-id token at offset {0}", start), start, index);

					return false;
				}

				value = Encoding.UTF8.GetString (text, start, index - start);

				if (quoted) {
					// this can only be the authserv-id token
					srvid = MimeUtils.Unquote (value, true);
				} else {
					// this could either be the authserv-id or it could be "i=#" (ARC instance)
					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;

					if (index < endIndex && text[index] == (byte) '=') {
						// probably i=#
						if (instance.HasValue) {
							if (throwOnError)
								throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid token at offset {0}", start), start, index);

							return false;
						}

						if (value != "i") {
							// Office 365 Authentication-Results do not include an authserv-id token, so this is probably a method.
							// Rewind the parser and start over again with the assumption that the Authentication-Results only
							// contains methods.
							//
							// See https://github.com/jstedfast/MimeKit/issues/490 for details.

							authres = new AuthenticationResults ();
							index = 0;

							return TryParseMethods (text, ref index, endIndex, throwOnError, authres);
						}

						// skip over '='
						index++;

						if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
							return false;

						start = index;

						if (!ParseUtils.TryParseInt32 (text, ref index, endIndex, out int i)) {
							if (throwOnError)
								throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid instance value at offset {0}", start), start, index);

							return false;
						}

						instance = i;

						if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
							return false;

						if (index >= endIndex) {
							if (throwOnError)
								throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Missing semi-colon after instance value at offset {0}", start), start, index);

							return false;
						}

						if (text[index] != (byte) ';') {
							if (throwOnError)
								throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Unexpected token after instance value at offset {0}", index), index, index);

							return false;
						}

						// skip over ';'
						index++;

						if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
							return false;
					} else {
						srvid = value;
					}
				}
			} while (srvid is null);

			authres = new AuthenticationResults (srvid) { Instance = instance };

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			if (index >= endIndex)
				return true;

			if (text[index] != (byte) ';') {
				// might be the authres-version token
				int start = index;

				if (!ParseUtils.TryParseInt32 (text, ref index, endIndex, out int version)) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid authres-version at offset {0}", start), start, index);

					return false;
				}

				authres.Version = version;

				if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;

				if (index >= endIndex)
					return true;

				if (text[index] != (byte) ';') {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Unknown token at offset {0}", index), index, index);

					return false;
				}
			}

			// skip the ';'
			index++;

			return TryParseMethods (text, ref index, endIndex, throwOnError, authres);
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="AuthenticationResults"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses an Authentication-Results header value from the supplied buffer starting at the given index
		/// and spanning across the specified number of bytes.
		/// </remarks>
		/// <returns><c>true</c> if the authentication results were successfully parsed; otherwise, <c>false</c>.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The number of bytes in the input buffer to parse.</param>
		/// <param name="authres">The parsed authentication results.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static bool TryParse (byte[] buffer, int startIndex, int length, out AuthenticationResults authres)
		{
			ParseUtils.ValidateArguments (buffer, startIndex, length);

			int index = startIndex;

			return TryParse (buffer, ref index, startIndex + length, false, out authres);
		}

		/// <summary>
		/// Try to parse the given input buffer into a new <see cref="AuthenticationResults"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses an Authentication-Results header value from the supplied buffer.
		/// </remarks>
		/// <returns><c>true</c> if the authentication results were successfully parsed; otherwise, <c>false</c>.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="authres">The parsed authentication results.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		public static bool TryParse (byte[] buffer, out AuthenticationResults authres)
		{
			if (buffer is null)
				throw new ArgumentNullException (nameof (buffer));

			int index = 0;

			return TryParse (buffer, ref index, buffer.Length, false, out authres);
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="AuthenticationResults"/> class.
		/// </summary>
		/// <remarks>
		/// Parses an Authentication-Results header value from the supplied buffer starting at the given index
		/// and spanning across the specified number of bytes.
		/// </remarks>
		/// <returns>The parsed <see cref="AuthenticationResults"/>.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <param name="startIndex">The start index of the buffer.</param>
		/// <param name="length">The length of the buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// The <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static AuthenticationResults Parse (byte[] buffer, int startIndex, int length)
		{
			ParseUtils.ValidateArguments (buffer, startIndex, length);

			int index = startIndex;

			TryParse (buffer, ref index, startIndex + length, true, out AuthenticationResults authres);

			return authres;
		}

		/// <summary>
		/// Parse the specified input buffer into a new instance of the <see cref="AuthenticationResults"/> class.
		/// </summary>
		/// <remarks>
		/// Parses an Authentication-Results header value from the supplied buffer.
		/// </remarks>
		/// <returns>The parsed <see cref="AuthenticationResults"/>.</returns>
		/// <param name="buffer">The input buffer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="buffer"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="MimeKit.ParseException">
		/// The <paramref name="buffer"/> could not be parsed.
		/// </exception>
		public static AuthenticationResults Parse (byte[] buffer)
		{
			if (buffer is null)
				throw new ArgumentNullException (nameof (buffer));

			int index = 0;

			TryParse (buffer, ref index, buffer.Length, true, out AuthenticationResults authres);

			return authres;
		}
	}

	/// <summary>
	/// An authentication method results.
	/// </summary>
	/// <remarks>
	/// An authentication method results.
	/// </remarks>
	public class AuthenticationMethodResult
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="AuthenticationMethodResult"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="AuthenticationMethodResult"/>.
		/// </remarks>
		/// <param name="method">The method used for authentication.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="method"/> is <c>null</c>.
		/// </exception>
		internal AuthenticationMethodResult (string method)
		{
			if (method is null)
				throw new ArgumentNullException (nameof (method));

			Properties = new List<AuthenticationMethodProperty> ();
			Method = method;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="AuthenticationMethodResult"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="AuthenticationMethodResult"/>.
		/// </remarks>
		/// <param name="method">The method used for authentication.</param>
		/// <param name="result">The result of the authentication method.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="method"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="result"/> is <c>null</c>.</para>
		/// </exception>
		public AuthenticationMethodResult (string method, string result) : this (method)
		{
			if (result is null)
				throw new ArgumentNullException (nameof (result));

			Result = result;
		}

		/// <summary>
		/// Get the Office365 method-specific authserv-id.
		/// </summary>
		/// <remarks>
		/// <para>Gets the Office365 method-specific authserv-id.</para>
		/// <para>An authentication service identifier is the <c>authserv-id</c> token
		/// as defined in <a href="https://tools.ietf.org/html/rfc7601">rfc7601</a>.</para>
		/// <para>Instead of specifying a single authentication service identifier at the
		/// beginning of the header value, Office365 seems to provide a different
		/// authentication service identifier for each method.</para>
		/// </remarks>
		/// <value>The authserv-id token.</value>
		public string Office365AuthenticationServiceIdentifier {
			get; internal set;
		}

		/// <summary>
		/// Get the authentication method.
		/// </summary>
		/// <remarks>
		/// Gets the authentication method.
		/// </remarks>
		/// <value>The authentication method.</value>
		public string Method {
			get; private set;
		}

		/// <summary>
		/// Get the authentication method version.
		/// </summary>
		/// <remarks>
		/// Gets the authentication method version.
		/// </remarks>
		/// <value>The authentication method version.</value>
		public int? Version {
			get; set;
		}

		/// <summary>
		/// Get the authentication method results.
		/// </summary>
		/// <remarks>
		/// Gets the authentication method results.
		/// </remarks>
		/// <value>The authentication method results.</value>
		public string Result {
			get; internal set;
		}

		/// <summary>
		/// Get the comment regarding the authentication method result.
		/// </summary>
		/// <remarks>
		/// Gets the comment regarding the authentication method result.
		/// </remarks>
		/// <value>The comment regarding the authentication method result.</value>
		public string ResultComment {
			get; set;
		}

		/// <summary>
		/// Get the action taken for the authentication method result.
		/// </summary>
		/// <remarks>
		/// Gets the action taken for the authentication method result.
		/// </remarks>
		/// <value>The action taken for the authentication method result.</value>
		public string Action {
			get; internal set;
		}

		/// <summary>
		/// Get the reason for the authentication method result.
		/// </summary>
		/// <remarks>
		/// Gets the reason for the authentication method result.
		/// </remarks>
		/// <value>The reason for the authentication method result.</value>
		public string Reason {
			get; set;
		}

		/// <summary>
		/// Get the properties used by the authentication method.
		/// </summary>
		/// <remarks>
		/// Gets the properties used by the authentication method.
		/// </remarks>
		/// <value>The properties used by the authentication method.</value>
		public List<AuthenticationMethodProperty> Properties {
			get; private set;
		}

		internal void Encode (FormatOptions options, StringBuilder builder, ref int lineLength)
		{
			// try to put the entire result on 1 line
			var complete = ToString ();

			if (complete.Length + 1 < options.MaxLineLength) {
				// if it fits, it sits...
				if (lineLength + complete.Length + 1 > options.MaxLineLength) {
					builder.Append (options.NewLine);
					builder.Append ('\t');
					lineLength = 1;
				} else {
					builder.Append (' ');
					lineLength++;
				}

				lineLength += complete.Length;
				builder.Append (complete);
				return;
			}

			// Note: if we've made it this far, then we can't put everything on one line...

			var tokens = new List<string> {
				" "
			};

			if (Office365AuthenticationServiceIdentifier != null) {
				tokens.Add (Office365AuthenticationServiceIdentifier);
				tokens.Add (";");
				tokens.Add (" ");
			}

			if (Version.HasValue) {
				var version = Version.Value.ToString (CultureInfo.InvariantCulture);

				if (Method.Length + 1 + version.Length + 1 + Result.Length < options.MaxLineLength) {
					tokens.Add ($"{Method}/{version}={Result}");
				} else if (Method.Length + 1 + version.Length < options.MaxLineLength) {
					tokens.Add ($"{Method}/{version}");
					tokens.Add ("=");
					tokens.Add (Result);
				} else {
					tokens.Add (Method);
					tokens.Add ("/");
					tokens.Add (version);
					tokens.Add ("=");
					tokens.Add (Result);
				}
			} else {
				if (Method.Length + 1 + Result.Length < options.MaxLineLength) {
					tokens.Add ($"{Method}={Result}");
				} else {
					// we will have to break this up into individual tokens
					tokens.Add (Method);
					tokens.Add ("=");
					tokens.Add (Result);
				}
			}

			if (!string.IsNullOrEmpty (ResultComment)) {
				tokens.Add (" ");
				tokens.Add ($"({ResultComment})");
			}

			if (!string.IsNullOrEmpty (Reason)) {
				var reason = MimeUtils.Quote (Reason);

				tokens.Add (" ");

				if ("reason=".Length + reason.Length < options.MaxLineLength) {
					tokens.Add ($"reason={reason}");
				} else {
					tokens.Add ("reason=");
					tokens.Add (reason);
				}
			} else if (!string.IsNullOrEmpty (Action)) {
				var action = MimeUtils.Quote (Action);

				tokens.Add (" ");

				if ("action=".Length + action.Length < options.MaxLineLength) {
					tokens.Add ($"action={action}");
				} else {
					tokens.Add ("action=");
					tokens.Add (action);
				}
			}

			for (int i = 0; i < Properties.Count; i++)
				Properties[i].AppendTokens (options, tokens);

			builder.AppendTokens (options, ref lineLength, tokens);
		}

		/// <summary>
		/// Serializes the <see cref="AuthenticationMethodResult"/> to a string.
		/// </summary>
		/// <remarks>
		/// Creates a string-representation of the <see cref="AuthenticationMethodResult"/>.
		/// </remarks>
		/// <returns>The serialized string.</returns>
		public override string ToString ()
		{
			var builder = new ValueStringBuilder (128);

			WriteTo (ref builder);

			return builder.ToString ();
		}

		internal void WriteTo (ref ValueStringBuilder builder)
		{
			if (Office365AuthenticationServiceIdentifier != null) {
				builder.Append (Office365AuthenticationServiceIdentifier);
				builder.Append ("; ");
			}

			builder.Append (Method);

			if (Version.HasValue) {
				builder.Append ('/');
				builder.AppendInvariant (Version.Value);
			}

			builder.Append ('=');
			builder.Append (Result);

			if (!string.IsNullOrEmpty (ResultComment)) {
				builder.Append (" (");
				builder.Append (ResultComment);
				builder.Append (')');
			}

			if (!string.IsNullOrEmpty (Reason)) {
				builder.Append (" reason=");
				builder.AppendQuoted (Reason);
			} else if (!string.IsNullOrEmpty (Action)) {
				builder.Append (" action=");
				builder.AppendQuoted (Action);
			}

			for (int i = 0; i < Properties.Count; i++) {
				builder.Append (' ');
				Properties[i].WriteTo (ref builder);
			}
		}
	}

	/// <summary>
	/// An authentication method property.
	/// </summary>
	/// <remarks>
	/// An authentication method property.
	/// </remarks>
	public class AuthenticationMethodProperty
	{
		static readonly char[] TokenSpecials = ByteExtensions.TokenSpecials.ToCharArray ();
		readonly bool? quoted;

		/// <summary>
		/// Initialize a new instance of the <see cref="AuthenticationMethodProperty"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="AuthenticationMethodProperty"/>.
		/// </remarks>
		/// <param name="ptype">The property type.</param>
		/// <param name="property">The name of the property.</param>
		/// <param name="value">The value of the property.</param>
		/// <param name="quoted"><c>true</c> if the property value was originally quoted; otherwise, <c>false</c>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ptype"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="property"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		internal AuthenticationMethodProperty (string ptype, string property, string value, bool? quoted)
		{
			if (ptype is null)
				throw new ArgumentNullException (nameof (ptype));

			if (property is null)
				throw new ArgumentNullException (nameof (property));

			if (value is null)
				throw new ArgumentNullException (nameof (value));

			this.quoted = quoted;
			PropertyType = ptype;
			Property = property;
			Value = value;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="AuthenticationMethodProperty"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="AuthenticationMethodProperty"/>.
		/// </remarks>
		/// <param name="ptype">The property type.</param>
		/// <param name="property">The name of the property.</param>
		/// <param name="value">The value of the property.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ptype"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="property"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		public AuthenticationMethodProperty (string ptype, string property, string value) : this (ptype, property, value, null)
		{
		}

		/// <summary>
		/// Get the type of the property.
		/// </summary>
		/// <remarks>
		/// Gets the type of the property.
		/// </remarks>
		/// <value>The type of the property.</value>
		public string PropertyType {
			get; private set;
		}

		/// <summary>
		/// Get the property name.
		/// </summary>
		/// <remarks>
		/// Gets the property name.
		/// </remarks>
		/// <value>The name of the property.</value>
		public string Property {
			get; private set;
		}

		/// <summary>
		/// Get the property value.
		/// </summary>
		/// <remarks>
		/// Gets the property value.
		/// </remarks>
		/// <value>The value of the property.</value>
		public string Value {
			get; private set;
		}

		internal void AppendTokens (FormatOptions options, List<string> tokens)
		{
			var quote = quoted ?? Value.IndexOfAny (TokenSpecials) != -1;
			var value = quote ? MimeUtils.Quote (Value) : Value;

			tokens.Add (" ");

			if (PropertyType.Length + 1 + Property.Length + 1 + value.Length < options.MaxLineLength) {
				tokens.Add ($"{PropertyType}.{Property}={value}");
			} else if (PropertyType.Length + 1 + Property.Length + 1 < options.MaxLineLength) {
				tokens.Add ($"{PropertyType}.{Property}=");
				tokens.Add (value);
			} else {
				tokens.Add (PropertyType);
				tokens.Add (".");
				tokens.Add (Property);
				tokens.Add ("=");
				tokens.Add (value);
			}
		}

		/// <summary>
		/// Serializes the <see cref="AuthenticationMethodProperty"/> to a string.
		/// </summary>
		/// <remarks>
		/// Creates a string-representation of the <see cref="AuthenticationMethodProperty"/>.
		/// </remarks>
		/// <returns>The serialized string.</returns>
		public override string ToString ()
		{
			var builder = new ValueStringBuilder (128);

			WriteTo (ref builder);

			return builder.ToString ();
		}

		internal void WriteTo (ref ValueStringBuilder builder)
		{
			bool quote = quoted ?? Value.IndexOfAny (TokenSpecials) != -1;

			builder.Append (PropertyType);
			builder.Append ('.');
			builder.Append (Property);
			builder.Append ('=');

			if (quote) {
				builder.AppendQuoted(Value);
			} else {
				builder.Append (Value);
			}
		}
	}
}

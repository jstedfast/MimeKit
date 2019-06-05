//
// AuthenticationResults.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2019 Xamarin Inc. (www.xamarin.com)
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

#if PORTABLE
using Encoding = Portable.Text.Encoding;
#endif

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
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.AuthenticationResults"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="AuthenticationResults"/>.
		/// </remarks>
		/// <param name="authservid">The authentication service identifier.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="authservid"/> is <c>null</c>.
		/// </exception>
		public AuthenticationResults (string authservid)
		{
			if (authservid == null)
				throw new ArgumentNullException (nameof (authservid));

			Results = new List<AuthenticationMethodResult> ();
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

			if (Results.Count > 0) {
				for (int i = 0; i < Results.Count; i++)
					Results[i].Encode (options, builder, ref lineLength);
			} else {
				builder.Append ("; none");
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
			var builder = new StringBuilder ();

			if (Instance.HasValue)
				builder.AppendFormat ("i={0}; ", Instance.Value.ToString (CultureInfo.InvariantCulture));

			builder.Append (AuthenticationServiceIdentifier);

			if (Version.HasValue) {
				builder.Append (' ');
				builder.Append (Version.Value.ToString (CultureInfo.InvariantCulture));
			}

			if (Results.Count > 0) {
				for (int i = 0; i < Results.Count; i++) {
					builder.Append ("; ");
					builder.Append (Results[i]);
				}
			} else {
				builder.Append ("; none");
			}

			return builder.ToString ();
		}

		static bool IsKeyword (byte c)
		{
			return (c >= (byte) 'A' && c <= (byte) 'Z') ||
				(c >= (byte) 'a' && c <= (byte) 'z') ||
				(c >= (byte) '0' && c <= (byte) '9') ||
				c == (byte) '-';
		}

		static bool SkipKeyword (byte[] text, ref int index, int endIndex)
		{
			int startIndex = index;

			while (index < endIndex && IsKeyword (text[index]))
				index++;

			return index > startIndex;
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
				// we're either at the start of the authserv-id token or "i=#;" (if we're parsing an ARC-Authentication-Results header)
				if (index < endIndex && text[index] == (byte) '"') {
					int start = index;

					if (!ParseUtils.SkipQuoted (text, ref index, endIndex, throwOnError))
						return false;

					srvid = MimeUtils.Unquote (Encoding.UTF8.GetString (text, start, index - start));
				} else {
					int start = index;

					if (!ParseUtils.SkipToken (text, ref index, endIndex)) {
						if (throwOnError)
							throw new ParseException (string.Format ("Incomplete authserv-id token at offset {0}", start), start, index);

						return false;
					}

					value = Encoding.UTF8.GetString (text, start, index - start);

					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;

					if (index < endIndex && text[index] == (byte) '=') {
						// probably i=#
						if (instance.HasValue) {
							if (throwOnError)
								throw new ParseException (string.Format ("Invalid token at offset {0}", start), start, index);

							return false;
						}

						if (value != "i") {
							if (throwOnError)
								throw new ParseException (string.Format ("Invalid instance token at offset {0}", start), start, index);

							return false;
						}

						// skip over '='
						index++;

						if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
							return false;

						start = index;

						if (!ParseUtils.TryParseInt32 (text, ref index, endIndex, out int i)) {
							if (throwOnError)
								throw new ParseException (string.Format ("Invalid instance value at offset {0}", start), start, index);

							return false;
						}

						instance = i;

						if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
							return false;

						if (index >= endIndex) {
							if (throwOnError)
								throw new ParseException (string.Format ("Missing semi-colon after instance value at offset {0}", start), start, index);

							return false;
						}

						if (text[index] != ';') {
							if (throwOnError)
								throw new ParseException (string.Format ("Unexpected token after instance value at offset {0}", index), index, index);

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
			} while (srvid == null);

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
						throw new ParseException (string.Format ("Invalid authres-version at offset {0}", start), start, index);

					return false;
				}

				authres.Version = version;

				if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;

				if (index >= endIndex)
					return true;

				if (text[index] != (byte) ';') {
					if (throwOnError)
						throw new ParseException (string.Format ("Unknown token at offset {0}", index), index, index);

					return false;
				}
			}

			// skip the ';'
			index++;

			while (index < endIndex) {
				if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;

				if (index >= endIndex)
					break;

				int methodIndex = index;

				// skip the method name
				if (!SkipKeyword (text, ref index, endIndex)) {
					if (throwOnError)
						throw new ParseException (string.Format ("Invalid method token at offset {0}", methodIndex), methodIndex, index);

					return false;
				}

				var method = Encoding.ASCII.GetString (text, methodIndex, index - methodIndex);

				if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;

				if (index >= endIndex) {
					if (method != "none") {
						if (throwOnError)
							throw new ParseException (string.Format ("Incomplete methodspec token at offset {0}", methodIndex), methodIndex, index);

						return false;
					}

					if (authres.Results.Count > 0) {
						if (throwOnError)
							throw new ParseException (string.Format ("Invalid no-result token at offset {0}", methodIndex), methodIndex, index);

						return false;
					}

					break;
				}

				var resinfo = new AuthenticationMethodResult (method);
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
							throw new ParseException (string.Format ("Invalid method-version token at offset {0}", tokenIndex), tokenIndex, index);

						return false;
					}

					resinfo.Version = version;

					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;

					if (index >= endIndex) {
						if (throwOnError)
							throw new ParseException (string.Format ("Incomplete methodspec token at offset {0}", methodIndex), methodIndex, index);

						return false;
					}
				}

				if (text[index] != (byte) '=') {
					if (throwOnError)
						throw new ParseException (string.Format ("Invalid methodspec token at offset {0}", methodIndex), methodIndex, index);

					return false;
				}

				// skip over '='
				index++;

				if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;

				if (index >= endIndex) {
					if (throwOnError)
						throw new ParseException (string.Format ("Incomplete methodspec token at offset {0}", methodIndex), methodIndex, index);

					return false;
				}

				tokenIndex = index;

				if (!SkipKeyword (text, ref index, endIndex)) {
					if (throwOnError)
						throw new ParseException (string.Format ("Invalid result token at offset {0}", tokenIndex), tokenIndex, index);

					return false;
				}

				resinfo.Result = Encoding.ASCII.GetString (text, tokenIndex, index - tokenIndex);

				ParseUtils.SkipWhiteSpace (text, ref index, endIndex);

				if (index < endIndex && text[index] == '(') {
					int commentIndex = index;

					if (!ParseUtils.SkipComment (text, ref index, endIndex)) {
						if (throwOnError)
							throw new ParseException (string.Format ("Incomplete comment token at offset {0}", commentIndex), commentIndex, index);

						return false;
					}

					commentIndex++;

					resinfo.ResultComment = Encoding.UTF8.GetString (text, commentIndex, (index - 1) - commentIndex);

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
						throw new ParseException (string.Format ("Invalid reasonspec or propspec token at offset {0}", tokenIndex), tokenIndex, index);

					return false;
				}

				value = Encoding.ASCII.GetString (text, tokenIndex, index - tokenIndex);

				if (value == "reason") {
					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;

					if (index >= endIndex) {
						if (throwOnError)
							throw new ParseException (string.Format ("Incomplete reasonspec token at offset {0}", tokenIndex), tokenIndex, index);

						return false;
					}

					if (text[index] != (byte) '=') {
						if (throwOnError)
							throw new ParseException (string.Format ("Invalid reasonspec token at offset {0}", tokenIndex), tokenIndex, index);

						return false;
					}

					index++;

					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;

					if (index < endIndex && text[index] == (byte) '"') {
						int start = index;

						if (!ParseUtils.SkipQuoted (text, ref index, endIndex, throwOnError))
							return false;

						resinfo.Reason = MimeUtils.Unquote (Encoding.UTF8.GetString (text, start, index - start));
					} else {
						int start = index;

						if (!ParseUtils.SkipToken (text, ref index, endIndex)) {
							if (throwOnError)
								throw new ParseException (string.Format ("Invalid reasonspec value token at offset {0}", start), start, index);

							return false;
						}

						resinfo.Reason = Encoding.UTF8.GetString (text, start, index - start);
					}

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
							throw new ParseException (string.Format ("Invalid propspec token at offset {0}", tokenIndex), tokenIndex, index);

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
							throw new ParseException (string.Format ("Incomplete propspec token at offset {0}", tokenIndex), tokenIndex, index);

						return false;
					}

					if (text[index] != (byte) '.') {
						if (throwOnError)
							throw new ParseException (string.Format ("Invalid propspec token at offset {0}", tokenIndex), tokenIndex, index);

						return false;
					}

					index++;

					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;

					if (index >= endIndex) {
						if (throwOnError)
							throw new ParseException (string.Format ("Incomplete propspec token at offset {0}", tokenIndex), tokenIndex, index);

						return false;
					}

					int propertyIndex = index;

					if (!SkipKeyword (text, ref index, endIndex)) {
						if (throwOnError)
							throw new ParseException (string.Format ("Invalid property token at offset {0}", propertyIndex), propertyIndex, index);

						return false;
					}

					var property = Encoding.ASCII.GetString (text, propertyIndex, index - propertyIndex);

					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;

					if (index >= endIndex) {
						if (throwOnError)
							throw new ParseException (string.Format ("Incomplete propspec token at offset {0}", tokenIndex), tokenIndex, index);

						return false;
					}

					if (text[index] != (byte) '=') {
						if (throwOnError)
							throw new ParseException (string.Format ("Invalid propspec token at offset {0}", tokenIndex), tokenIndex, index);

						return false;
					}

					index++;

					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;

					if (index >= endIndex) {
						if (throwOnError)
							throw new ParseException (string.Format ("Incomplete propspec token at offset {0}", tokenIndex), tokenIndex, index);

						return false;
					}

					int valueIndex = index;

					while (index < endIndex && text[index] != ';' && !text[index].IsWhitespace ())
						index++;

					if (index == valueIndex) {
						if (throwOnError)
							throw new ParseException (string.Format ("Incomplete propspec token at offset {0}", tokenIndex), tokenIndex, index);

						return false;
					}

					value = Encoding.UTF8.GetString (text, valueIndex, index - valueIndex);

					var propspec = new AuthenticationMethodProperty (ptype, property, value);
					resinfo.Properties.Add (propspec);

					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;

					if (index >= endIndex || text[index] == (byte) ';')
						break;

					tokenIndex = index;

					if (!SkipKeyword (text, ref index, endIndex)) {
						if (throwOnError)
							throw new ParseException (string.Format ("Invalid propspec token at offset {0}", tokenIndex), tokenIndex, index);

						return false;
					}

					value = Encoding.ASCII.GetString (text, tokenIndex, index - tokenIndex);
				} while (true);

				// skip over ';'
				index++;
			}

			return true;
		}

		/// <summary>
		/// Tries to parse the given input buffer into a new <see cref="AuthenticationResults"/> instance.
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
		/// Tries to parse the given input buffer into a new <see cref="AuthenticationResults"/> instance.
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
			if (buffer == null)
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

			AuthenticationResults authres;
			int index = startIndex;

			TryParse (buffer, ref index, startIndex + length, true, out authres);

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
			if (buffer == null)
				throw new ArgumentNullException (nameof (buffer));

			AuthenticationResults authres;
			int index = 0;

			TryParse (buffer, ref index, buffer.Length, true, out authres);

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
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.AuthenticationMethodResult"/> class.
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
			if (method == null)
				throw new ArgumentNullException (nameof (method));

			Properties = new List<AuthenticationMethodProperty> ();
			Method = method;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.AuthenticationMethodResult"/> class.
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
			if (result == null)
				throw new ArgumentNullException (nameof (result));

			Result = result;
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
		/// <value>THe comment regarding the authentication method result.</value>
		public string ResultComment {
			get; set;
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
			builder.Append (';');
			lineLength++;

			// try to put the entire result on 1 line
			var complete = ToString ();

			if (complete.Length < options.MaxLineLength) {
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

			var tokens = new List<string> ();

			if (Version.HasValue) {
				var version = Version.Value.ToString (CultureInfo.InvariantCulture);
				var combinedLength = Method.Length + 1 + version.Length + 1 + Result.Length;

				if (combinedLength > options.MaxLineLength) {
					// we will have to break this up into individual tokens
					tokens.Add (Method);
					tokens.Add ("/");
					tokens.Add (version);
					tokens.Add ("=");
					tokens.Add (Result);
				} else {
					tokens.Add (string.Format ("{0}/{1}={2}", Method, version, Result));
				}
			} else {
				var combinedLength = Method.Length + 1 + Result.Length;

				if (combinedLength > options.MaxLineLength) {
					// we will have to break this up into individual tokens
					tokens.Add (Method);
					tokens.Add ("=");
					tokens.Add (Result);
				} else {
					tokens.Add (string.Format ("{0}={1}", Method, Result));
				}
			}

			builder.AppendTokens (options, ref lineLength, tokens, true);
			tokens.Clear ();

			if (!string.IsNullOrEmpty (ResultComment)) {
				if (lineLength + ResultComment.Length + 3 > options.MaxLineLength) {
					builder.Append (options.NewLine);
					builder.Append ('\t');
					lineLength = 1;
				} else {
					builder.Append (' ');
					lineLength++;
				}

				lineLength += ResultComment.Length + 2;
				builder.Append ('(');
				builder.Append (ResultComment);
				builder.Append (')');
			}

			if (!string.IsNullOrEmpty (Reason)) {
				var reason = MimeUtils.Quote (Reason);
				var combinedLength = "reason=".Length + reason.Length;

				tokens.Clear ();
				if (combinedLength > options.MaxLineLength) {
					// we will have to break this up into individual tokens
					tokens.Add ("reason=");
					tokens.Add (reason);
				} else {
					tokens.Add ("reason=" + reason);
				}

				builder.AppendTokens (options, ref lineLength, tokens);
				tokens.Clear ();
			}

			for (int i = 0; i < Properties.Count; i++)
				Properties[i].Encode (options, builder, ref lineLength);
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
			var builder = new StringBuilder (Method);

			if (Version.HasValue) {
				builder.Append ('/');
				builder.Append (Version.Value.ToString (CultureInfo.InvariantCulture));
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
				builder.Append (MimeUtils.Quote (Reason));
			}

			for (int i = 0; i < Properties.Count; i++) {
				builder.Append (' ');
				builder.Append (Properties[i]);
			}

			return builder.ToString ();
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
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.AuthenticationMethodProperty"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="AuthenticationMethodProperty"/>.
		/// </remarks>
		/// <param name="ptype">The property type.</param>
		/// <param name="property">The name of the property.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ptype"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="property"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is <c>null</c>.</para>
		/// </exception>
		public AuthenticationMethodProperty (string ptype, string property, string value)
		{
			if (ptype == null)
				throw new ArgumentNullException (nameof (ptype));

			if (property == null)
				throw new ArgumentNullException (nameof (property));

			if (value == null)
				throw new ArgumentNullException (nameof (value));

			PropertyType = ptype;
			Property = property;
			Value = value;
		}

		/// <summary>
		/// Get the type of the property.
		/// </summary>
		/// <remarks>
		/// Gets the type of the property.
		/// </remarks>
		/// <remarks>The type of the property.</remarks>
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

		internal void Encode (FormatOptions options, StringBuilder builder, ref int lineLength)
		{
			var combinedLength = PropertyType.Length + 1 + Property.Length + 1 + Value.Length;
			var tokens = new List<string> ();

			if (combinedLength > options.MaxLineLength) {
				// we will have to break this up into individual tokens
				combinedLength = PropertyType.Length + 1 + Property.Length + 1;

				if (combinedLength > options.MaxLineLength) {
					tokens.Add (PropertyType);
					tokens.Add (".");
					tokens.Add (Property);
					tokens.Add ("=");
					tokens.Add (Value);
				} else {
					tokens.Add (PropertyType + "." + Property + "=");
					tokens.Add (Value);
				}
			} else {
				tokens.Add (ToString ());
			}

			builder.AppendTokens (options, ref lineLength, tokens);
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
			return $"{PropertyType}.{Property}={Value}";
		}
	}
}

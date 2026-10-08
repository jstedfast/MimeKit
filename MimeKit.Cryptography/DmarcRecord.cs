//
// DmarcRecord.cs
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A DMARC Policy Record.
	/// </summary>
	/// <remarks>
	/// <para>A DMARC Policy Record is published by a Domain Owner as a DNS TXT record at <c>_dmarc.&lt;domain&gt;</c>
	/// and is defined by <a href="https://www.rfc-editor.org/rfc/rfc9989#section-4.7">RFC 9989, Section 4.7</a>.</para>
	/// <para>The record must begin with a <c>v=DMARC1</c> tag; any text that does not is not a DMARC Policy Record.
	/// Beyond that, the parser is lenient, as required by
	/// <a href="https://www.rfc-editor.org/rfc/rfc9989#section-4.8">RFC 9989, Section 4.8</a>: unknown tags
	/// (including the historic <c>pct</c>, <c>rf</c> and <c>ri</c> tags) are ignored, and a tag with an invalid value is
	/// discarded in favor of the default value. Discarded errors are reported by the <see cref="Errors"/>
	/// property.</para>
	/// </remarks>
	public sealed class DmarcRecord
	{
		[Flags]
		enum Tags
		{
			None = 0,
			Version = 1 << 0,
			Policy = 1 << 1,
			SubdomainPolicy = 1 << 2,
			NonExistentSubdomainPolicy = 1 << 3,
			PublicSuffixDomain = 1 << 4,
			Testing = 1 << 5,
			DkimAlignment = 1 << 6,
			SpfAlignment = 1 << 7,
			AggregateReportUris = 1 << 8,
			FailureReportUris = 1 << 9,
			FailureReportingOptions = 1 << 10
		}

		const string Version = "DMARC1";

		DmarcRecord ()
		{
			FailureReportingOptions = DmarcFailureReportingOptions.AllFail;
			AggregateReportUris = Array.Empty<string> ();
			FailureReportUris = Array.Empty<string> ();
		}

		/// <summary>
		/// Get the Domain Owner Assessment Policy.
		/// </summary>
		/// <remarks>
		/// <para>Gets the policy specified by the <c>p</c> tag.</para>
		/// <para>The value is <see langword="null"/> if the <c>p</c> tag was absent or invalid. To distinguish the two,
		/// check <see cref="Errors"/> for <see cref="DmarcRecordErrors.InvalidPolicy"/>. According to
		/// <a href="https://www.rfc-editor.org/rfc/rfc9989#section-4.10.1">RFC 9989, Section 4.10.1</a>, a record
		/// without a <c>p</c> tag is treated as if it specified <c>p=none</c>, while a record with an invalid
		/// <c>p</c> tag is treated as if it specified <c>p=none</c> only if it has at least one valid
		/// <see cref="AggregateReportUris">aggregate report URI</see>.</para>
		/// </remarks>
		/// <value>The policy, or <see langword="null"/> if it was absent or invalid.</value>
		public DmarcPolicy? Policy {
			get; private set;
		}

		/// <summary>
		/// Get the Domain Owner Assessment Policy for existing subdomains.
		/// </summary>
		/// <remarks>
		/// <para>Gets the policy specified by the <c>sp</c> tag. If it is absent, the <see cref="Policy"/> applies
		/// to subdomains.</para>
		/// </remarks>
		/// <value>The subdomain policy, or <see langword="null"/> if it was absent or invalid.</value>
		public DmarcPolicy? SubdomainPolicy {
			get; private set;
		}

		/// <summary>
		/// Get the Domain Owner Assessment Policy for non-existent subdomains.
		/// </summary>
		/// <remarks>
		/// <para>Gets the policy specified by the <c>np</c> tag. If it is absent, the <see cref="SubdomainPolicy"/>
		/// applies to non-existent subdomains, or the <see cref="Policy"/> if that is also absent.</para>
		/// </remarks>
		/// <value>The non-existent subdomain policy, or <see langword="null"/> if it was absent or invalid.</value>
		public DmarcPolicy? NonExistentSubdomainPolicy {
			get; private set;
		}

		/// <summary>
		/// Get whether the domain is a Public Suffix Domain.
		/// </summary>
		/// <remarks>
		/// Gets the value of the <c>psd</c> tag. The default is <see cref="DmarcPublicSuffixDomain.Unspecified"/>.
		/// </remarks>
		/// <value>The value of the <c>psd</c> tag.</value>
		public DmarcPublicSuffixDomain PublicSuffixDomain {
			get; private set;
		}

		/// <summary>
		/// Get whether the Domain Owner is testing its policy.
		/// </summary>
		/// <remarks>
		/// <para>Gets whether the record specified <c>t=y</c>, which requests that the policy be applied one level
		/// below the one specified (<c>reject</c> becomes <c>quarantine</c>, and <c>quarantine</c> becomes
		/// <c>none</c>). The default is <see langword="false"/>.</para>
		/// </remarks>
		/// <value><see langword="true" /> if the Domain Owner is testing its policy; otherwise, <see langword="false" />.</value>
		public bool Testing {
			get; private set;
		}

		/// <summary>
		/// Get the DKIM identifier alignment mode.
		/// </summary>
		/// <remarks>
		/// Gets the value of the <c>adkim</c> tag. The default is <see cref="DmarcAlignmentMode.Relaxed"/>.
		/// </remarks>
		/// <value>The DKIM alignment mode.</value>
		public DmarcAlignmentMode DkimAlignment {
			get; private set;
		}

		/// <summary>
		/// Get the SPF identifier alignment mode.
		/// </summary>
		/// <remarks>
		/// Gets the value of the <c>aspf</c> tag. The default is <see cref="DmarcAlignmentMode.Relaxed"/>.
		/// </remarks>
		/// <value>The SPF alignment mode.</value>
		public DmarcAlignmentMode SpfAlignment {
			get; private set;
		}

		/// <summary>
		/// Get the URIs to which aggregate feedback reports should be sent.
		/// </summary>
		/// <remarks>
		/// <para>Gets the valid URIs listed in the <c>rua</c> tag, in the order in which they appeared. The
		/// obsolete <c>!size</c> suffix is removed. Invalid URIs are omitted and reported by
		/// <see cref="DmarcRecordErrors.InvalidAggregateReportUri"/>.</para>
		/// </remarks>
		/// <value>The aggregate report URIs.</value>
		public IReadOnlyList<string> AggregateReportUris {
			get; private set;
		}

		/// <summary>
		/// Get the URIs to which failure reports should be sent.
		/// </summary>
		/// <remarks>
		/// <para>Gets the valid URIs listed in the <c>ruf</c> tag, in the order in which they appeared. The
		/// obsolete <c>!size</c> suffix is removed. Invalid URIs are omitted and reported by
		/// <see cref="DmarcRecordErrors.InvalidFailureReportUri"/>.</para>
		/// </remarks>
		/// <value>The failure report URIs.</value>
		public IReadOnlyList<string> FailureReportUris {
			get; private set;
		}

		/// <summary>
		/// Get the failure reporting options.
		/// </summary>
		/// <remarks>
		/// <para>Gets the value of the <c>fo</c> tag. The default is <see cref="DmarcFailureReportingOptions.AllFail"/>.</para>
		/// <para>As required by <a href="https://www.rfc-editor.org/rfc/rfc9989#section-4.7">RFC 9989, Section 4.7</a>,
		/// the <c>fo</c> tag is ignored if the record does not also have a <c>ruf</c> tag.</para>
		/// </remarks>
		/// <value>The failure reporting options.</value>
		public DmarcFailureReportingOptions FailureReportingOptions {
			get; private set;
		}

		/// <summary>
		/// Get the syntax errors that were discarded while parsing the record.
		/// </summary>
		/// <remarks>
		/// Gets the syntax errors that were discarded in favor of default values while parsing the record.
		/// </remarks>
		/// <value>The errors.</value>
		public DmarcRecordErrors Errors {
			get; private set;
		}

		static bool IsWhiteSpace (char c)
		{
			return c == ' ' || c == '\t' || c == '\r' || c == '\n';
		}

		static bool IsAlpha (char c)
		{
			return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
		}

		static bool IsDigit (char c)
		{
			return c >= '0' && c <= '9';
		}

		static void SkipWhiteSpace (string text, ref int index, int endIndex)
		{
			while (index < endIndex && IsWhiteSpace (text[index]))
				index++;
		}

		static bool IsMatch (string text, int startIndex, int length, string value)
		{
			return length == value.Length && string.Compare (text, startIndex, value, 0, length, StringComparison.OrdinalIgnoreCase) == 0;
		}

		static bool TryParsePolicy (string text, int startIndex, int length, out DmarcPolicy policy)
		{
			if (IsMatch (text, startIndex, length, "none")) {
				policy = DmarcPolicy.None;
				return true;
			}

			if (IsMatch (text, startIndex, length, "quarantine")) {
				policy = DmarcPolicy.Quarantine;
				return true;
			}

			if (IsMatch (text, startIndex, length, "reject")) {
				policy = DmarcPolicy.Reject;
				return true;
			}

			policy = DmarcPolicy.None;

			return false;
		}

		static bool TryParseAlignmentMode (string text, int startIndex, int length, out DmarcAlignmentMode mode)
		{
			if (IsMatch (text, startIndex, length, "r")) {
				mode = DmarcAlignmentMode.Relaxed;
				return true;
			}

			if (IsMatch (text, startIndex, length, "s")) {
				mode = DmarcAlignmentMode.Strict;
				return true;
			}

			mode = DmarcAlignmentMode.Relaxed;

			return false;
		}

		static bool TryParsePublicSuffixDomain (string text, int startIndex, int length, out DmarcPublicSuffixDomain psd)
		{
			if (IsMatch (text, startIndex, length, "y")) {
				psd = DmarcPublicSuffixDomain.Yes;
				return true;
			}

			if (IsMatch (text, startIndex, length, "n")) {
				psd = DmarcPublicSuffixDomain.No;
				return true;
			}

			if (IsMatch (text, startIndex, length, "u")) {
				psd = DmarcPublicSuffixDomain.Unspecified;
				return true;
			}

			psd = DmarcPublicSuffixDomain.Unspecified;

			return false;
		}

		static bool TryParseYesOrNo (string text, int startIndex, int length, out bool value)
		{
			if (IsMatch (text, startIndex, length, "y")) {
				value = true;
				return true;
			}

			if (IsMatch (text, startIndex, length, "n")) {
				value = false;
				return true;
			}

			value = false;

			return false;
		}

		static bool TryParseFailureReportingOptions (string text, int startIndex, int endIndex, out DmarcFailureReportingOptions options)
		{
			int index = startIndex;

			options = DmarcFailureReportingOptions.None;

			while (index < endIndex) {
				DmarcFailureReportingOptions option;

				SkipWhiteSpace (text, ref index, endIndex);

				if (index == endIndex)
					return false;

				switch (text[index]) {
				case '0': option = DmarcFailureReportingOptions.AllFail; break;
				case '1': option = DmarcFailureReportingOptions.AnyFail; break;
				case 'd': case 'D': option = DmarcFailureReportingOptions.Dkim; break;
				case 's': case 'S': option = DmarcFailureReportingOptions.Spf; break;
				default: return false;
				}

				// each value may appear at most once
				if ((options & option) != 0)
					return false;

				options |= option;
				index++;

				SkipWhiteSpace (text, ref index, endIndex);

				if (index < endIndex) {
					if (text[index] != ':')
						return false;

					index++;

					// a trailing ':' is not allowed
					if (index == endIndex)
						return false;
				}
			}

			// "0" and "1" are mutually exclusive
			const DmarcFailureReportingOptions exclusive = DmarcFailureReportingOptions.AllFail | DmarcFailureReportingOptions.AnyFail;

			return options != DmarcFailureReportingOptions.None && (options & exclusive) != exclusive;
		}

		static bool IsValidUri (string text, int startIndex, int endIndex)
		{
			int index = startIndex;

			// Note: Uri.TryCreate() treats an absolute file system path (e.g. "/path" on Unix) as an absolute URI,
			// so make sure that the URI begins with a scheme as defined by rfc3986.
			if (index == endIndex || !IsAlpha (text[index]))
				return false;

			index++;

			while (index < endIndex && (IsAlpha (text[index]) || IsDigit (text[index]) || text[index] == '+' || text[index] == '-' || text[index] == '.'))
				index++;

			if (index == endIndex || text[index] != ':')
				return false;

			// a dmarc-uri may only contain printable ASCII characters, and commas must be percent-encoded
			for (int i = index + 1; i < endIndex; i++) {
				if (text[i] <= 0x20 || text[i] >= 0x7F || text[i] == ',')
					return false;
			}

			return Uri.TryCreate (text.Substring (startIndex, endIndex - startIndex), UriKind.Absolute, out _);
		}

		static IReadOnlyList<string> ParseUriList (string text, int startIndex, int endIndex, out bool valid)
		{
			var uris = new List<string> ();
			int index = startIndex;

			valid = true;

			while (index <= endIndex) {
				SkipWhiteSpace (text, ref index, endIndex);

				int uriIndex = index;

				while (index < endIndex && text[index] != ',')
					index++;

				int uriEnd = index;

				while (uriEnd > uriIndex && IsWhiteSpace (text[uriEnd - 1]))
					uriEnd--;

				// Note: exclamation points must be percent-encoded in a dmarc-uri, so a '!' can only introduce
				// the obsolete report size limit, which must be ignored.
				int bang = text.IndexOf ('!', uriIndex, uriEnd - uriIndex);
				if (bang != -1)
					uriEnd = bang;

				if (IsValidUri (text, uriIndex, uriEnd))
					uris.Add (text.Substring (uriIndex, uriEnd - uriIndex));
				else
					valid = false;

				// skip over the ','
				index++;
			}

			return uris.Count > 0 ? uris.ToArray () : Array.Empty<string> ();
		}

		void ParseTag (string text, int startIndex, int endIndex, ref Tags seen, ref int foIndex, ref int foEnd)
		{
			int index = startIndex;
			Tags tag;

			SkipWhiteSpace (text, ref index, endIndex);

			while (endIndex > index && IsWhiteSpace (text[endIndex - 1]))
				endIndex--;

			// tolerate empty tags (e.g. ";;")
			if (index == endIndex)
				return;

			int nameIndex = index;

			while (index < endIndex && IsAlpha (text[index]))
				index++;

			int nameLength = index - nameIndex;

			SkipWhiteSpace (text, ref index, endIndex);

			if (nameLength == 0 || index == endIndex || text[index] != '=') {
				Errors |= DmarcRecordErrors.MalformedTag;
				return;
			}

			// skip over the '='
			index++;

			SkipWhiteSpace (text, ref index, endIndex);

			int valueLength = endIndex - index;

			if (IsMatch (text, nameIndex, nameLength, "v"))
				tag = Tags.Version;
			else if (IsMatch (text, nameIndex, nameLength, "p"))
				tag = Tags.Policy;
			else if (IsMatch (text, nameIndex, nameLength, "sp"))
				tag = Tags.SubdomainPolicy;
			else if (IsMatch (text, nameIndex, nameLength, "np"))
				tag = Tags.NonExistentSubdomainPolicy;
			else if (IsMatch (text, nameIndex, nameLength, "psd"))
				tag = Tags.PublicSuffixDomain;
			else if (IsMatch (text, nameIndex, nameLength, "t"))
				tag = Tags.Testing;
			else if (IsMatch (text, nameIndex, nameLength, "adkim"))
				tag = Tags.DkimAlignment;
			else if (IsMatch (text, nameIndex, nameLength, "aspf"))
				tag = Tags.SpfAlignment;
			else if (IsMatch (text, nameIndex, nameLength, "rua"))
				tag = Tags.AggregateReportUris;
			else if (IsMatch (text, nameIndex, nameLength, "ruf"))
				tag = Tags.FailureReportUris;
			else if (IsMatch (text, nameIndex, nameLength, "fo"))
				tag = Tags.FailureReportingOptions;
			else
				return; // unknown tags MUST be ignored

			if ((seen & tag) != 0) {
				Errors |= DmarcRecordErrors.DuplicateTag;
				return;
			}

			seen |= tag;

			switch (tag) {
			case Tags.Policy:
				if (TryParsePolicy (text, index, valueLength, out var policy))
					Policy = policy;
				else
					Errors |= DmarcRecordErrors.InvalidPolicy;
				break;
			case Tags.SubdomainPolicy:
				if (TryParsePolicy (text, index, valueLength, out policy))
					SubdomainPolicy = policy;
				else
					Errors |= DmarcRecordErrors.InvalidSubdomainPolicy;
				break;
			case Tags.NonExistentSubdomainPolicy:
				if (TryParsePolicy (text, index, valueLength, out policy))
					NonExistentSubdomainPolicy = policy;
				else
					Errors |= DmarcRecordErrors.InvalidNonExistentSubdomainPolicy;
				break;
			case Tags.PublicSuffixDomain:
				if (TryParsePublicSuffixDomain (text, index, valueLength, out var psd))
					PublicSuffixDomain = psd;
				else
					Errors |= DmarcRecordErrors.InvalidPublicSuffixDomain;
				break;
			case Tags.Testing:
				if (TryParseYesOrNo (text, index, valueLength, out var testing))
					Testing = testing;
				else
					Errors |= DmarcRecordErrors.InvalidTesting;
				break;
			case Tags.DkimAlignment:
				if (TryParseAlignmentMode (text, index, valueLength, out var mode))
					DkimAlignment = mode;
				else
					Errors |= DmarcRecordErrors.InvalidDkimAlignment;
				break;
			case Tags.SpfAlignment:
				if (TryParseAlignmentMode (text, index, valueLength, out mode))
					SpfAlignment = mode;
				else
					Errors |= DmarcRecordErrors.InvalidSpfAlignment;
				break;
			case Tags.AggregateReportUris:
				AggregateReportUris = ParseUriList (text, index, endIndex, out var valid);
				if (!valid)
					Errors |= DmarcRecordErrors.InvalidAggregateReportUri;
				break;
			case Tags.FailureReportUris:
				FailureReportUris = ParseUriList (text, index, endIndex, out valid);
				if (!valid)
					Errors |= DmarcRecordErrors.InvalidFailureReportUri;
				break;
			case Tags.FailureReportingOptions:
				// the fo tag is parsed once we know whether there is also a ruf tag
				foIndex = index;
				foEnd = endIndex;
				break;
			}
		}

		static bool TryParse (string text, [NotNullWhen (true)] out DmarcRecord? record, out int errorIndex)
		{
			int index = 0;

			record = null;

			// Note: the record MUST begin with "v=DMARC1" (the "v" is case-insensitive, but "DMARC1" is not).
			// Be lenient about leading whitespace.
			SkipWhiteSpace (text, ref index, text.Length);

			if (index == text.Length || (text[index] != 'v' && text[index] != 'V')) {
				errorIndex = index;
				return false;
			}

			index++;

			SkipWhiteSpace (text, ref index, text.Length);

			if (index == text.Length || text[index] != '=') {
				errorIndex = index;
				return false;
			}

			index++;

			SkipWhiteSpace (text, ref index, text.Length);

			if (string.CompareOrdinal (text, index, Version, 0, Version.Length) != 0) {
				errorIndex = index;
				return false;
			}

			index += Version.Length;

			SkipWhiteSpace (text, ref index, text.Length);

			if (index < text.Length && text[index] != ';') {
				errorIndex = index;
				return false;
			}

			var result = new DmarcRecord ();
			var seen = Tags.Version;
			int foIndex = -1;
			int foEnd = -1;

			while (index < text.Length) {
				// skip over the ';'
				index++;

				int tagIndex = index;

				while (index < text.Length && text[index] != ';')
					index++;

				result.ParseTag (text, tagIndex, index, ref seen, ref foIndex, ref foEnd);
			}

			// Note: the fo tag MUST be ignored if there is no ruf tag.
			if (foIndex != -1 && (seen & Tags.FailureReportUris) != 0) {
				if (TryParseFailureReportingOptions (text, foIndex, foEnd, out var options))
					result.FailureReportingOptions = options;
				else
					result.Errors |= DmarcRecordErrors.InvalidFailureReportingOptions;
			}

			errorIndex = -1;
			record = result;

			return true;
		}

		/// <summary>
		/// Try to parse the given text into a new <see cref="DmarcRecord"/> instance.
		/// </summary>
		/// <remarks>
		/// <para>Parses a DMARC Policy Record from the text of a DNS TXT record. If the TXT record consists of multiple
		/// character-strings, they must be concatenated without any separator before being parsed.</para>
		/// <para>Returns <see langword="false"/> only if the text is not a DMARC Policy Record (i.e. it does not begin
		/// with <c>v=DMARC1</c>). Syntax errors in the remainder of the record are discarded in favor of default values
		/// and reported by the <see cref="Errors"/> property.</para>
		/// </remarks>
		/// <returns><see langword="true" /> if the text is a DMARC Policy Record; otherwise, <see langword="false" />.</returns>
		/// <param name="text">The text to parse.</param>
		/// <param name="record">The parsed DMARC Policy Record.</param>
		public static bool TryParse (string? text, [NotNullWhen (true)] out DmarcRecord? record)
		{
			if (text is null) {
				record = null;
				return false;
			}

			return TryParse (text, out record, out _);
		}

		/// <summary>
		/// Parse the given text into a new <see cref="DmarcRecord"/> instance.
		/// </summary>
		/// <remarks>
		/// <para>Parses a DMARC Policy Record from the text of a DNS TXT record. If the TXT record consists of multiple
		/// character-strings, they must be concatenated without any separator before being parsed.</para>
		/// <para>Throws a <see cref="ParseException"/> only if the text is not a DMARC Policy Record (i.e. it does not
		/// begin with <c>v=DMARC1</c>). Syntax errors in the remainder of the record are discarded in favor of default
		/// values and reported by the <see cref="Errors"/> property.</para>
		/// </remarks>
		/// <returns>The parsed DMARC Policy Record.</returns>
		/// <param name="text">The text to parse.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ParseException">
		/// <paramref name="text"/> is not a DMARC Policy Record.
		/// </exception>
		public static DmarcRecord Parse (string text)
		{
			if (text is null)
				throw new ArgumentNullException (nameof (text));

			if (!TryParse (text, out var record, out int errorIndex))
				throw new ParseException ("The text is not a DMARC Policy Record: it does not begin with v=DMARC1.", 0, errorIndex);

			return record;
		}
	}
}

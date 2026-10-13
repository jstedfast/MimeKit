//
// DnsRecordsTarget.cs
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

using System.Text;

using Org.BouncyCastle.Crypto.Parameters;

using MimeKit.Cryptography;
using MimeKit.Fuzzing.Helpers;

namespace MimeKit.Fuzzing.Targets {
	// Feeds the input (as UTF-8) to the parsers for everything the DKIM, ARC and DMARC verifiers get from DNS or
	// from signature headers: DKIM public key records, DMARC policy records, tag=value lists, DKIM signature tags
	// and domain names. These parsers process data published by whoever controls the sending domain, so they
	// must never throw, and the values they produce must be self-consistent.
	sealed class DnsRecordsTarget : IFuzzTarget
	{
		const int MaxLines = 16;

		static readonly DkimSignatureAlgorithm[] Algorithms = (DkimSignatureAlgorithm[]) Enum.GetValues (typeof (DkimSignatureAlgorithm));

		public string Name => "dns-records";

		public int MaxLength => 16 * 1024;

		public void Run (ReadOnlySpan<byte> data)
		{
			var text = Fuzz.Utf8.GetString (data);

			CheckDkimPublicKeyRecord (text);
			CheckDmarcRecord (text);
			CheckTagValueList (text);
			CheckParameterTags (text);

			// Domain names are a single line, so also try each of the first few lines on its own.
			CheckDomainName (text);

			var lines = text.Split ('\n', MaxLines + 1);

			for (int i = 0; i < Math.Min (lines.Length, MaxLines); i++)
				CheckDomainName (lines[i].TrimEnd ('\r'));
		}

		static void CheckDkimPublicKeyRecord (string text)
		{
			if (!DkimPublicKeyRecord.TryParse (text, out var record))
				return;

			switch (record.KeyType) {
			case DkimKeyType.Rsa:
				Fuzz.Check (record.PublicKey is null or RsaKeyParameters { IsPrivate: false }, $"DkimPublicKeyRecord k=rsa has a {record.PublicKey?.GetType ().Name} key");
				break;
			case DkimKeyType.Ed25519:
				Fuzz.Check (record.PublicKey is null or Ed25519PublicKeyParameters, $"DkimPublicKeyRecord k=ed25519 has a {record.PublicKey?.GetType ().Name} key");
				break;
			default:
				Fuzz.Check (false, $"DkimPublicKeyRecord has an unknown KeyType: {record.KeyType}");
				break;
			}

			Fuzz.Check (record.IsRevoked == (record.PublicKey is null), "DkimPublicKeyRecord.IsRevoked disagrees with PublicKey");

			if (record.HashAlgorithms != null) {
				foreach (var hash in record.HashAlgorithms)
					Fuzz.Check (hash.Length > 0 && hash.IndexOf (':') == -1 && !IsWhiteSpace (hash[0]) && !IsWhiteSpace (hash[hash.Length - 1]), $"DkimPublicKeyRecord has a malformed hash algorithm: \"{Fuzz.Escape (hash)}\"");
			}

			int compatible = 0;

			foreach (var algorithm in Algorithms) {
				if (record.IsCompatible (algorithm))
					compatible++;
			}

			// An Ed25519 key can only ever be used with ed25519-sha256.
			if (record.KeyType == DkimKeyType.Ed25519)
				Fuzz.Check (compatible <= 1, $"DkimPublicKeyRecord k=ed25519 is compatible with {compatible} algorithms");

			Fuzz.Check (DkimPublicKeyRecord.TryParse (text, out var again), "DkimPublicKeyRecord.TryParse is not deterministic");
			Fuzz.Equal (Dump (record), Dump (again!), "DkimPublicKeyRecord re-parse");
		}

		static string Dump (DkimPublicKeyRecord record)
		{
			var builder = new StringBuilder ();

			builder.Append ("k=").Append (record.KeyType);
			builder.Append ("; revoked=").Append (record.IsRevoked);
			builder.Append ("; testing=").Append (record.IsTesting);
			builder.Append ("; strict=").Append (record.IsStrict);

			if (record.HashAlgorithms != null)
				builder.Append ("; h=").Append (string.Join (":", record.HashAlgorithms));

			switch (record.PublicKey) {
			case RsaKeyParameters rsa:
				builder.Append ("; n=").Append (rsa.Modulus).Append ("; e=").Append (rsa.Exponent);
				break;
			case Ed25519PublicKeyParameters ed25519:
				builder.Append ("; p=").Append (Convert.ToBase64String (ed25519.GetEncoded ()));
				break;
			}

			return builder.ToString ();
		}

		static void CheckDmarcRecord (string text)
		{
			bool parsed = DmarcRecord.TryParse (text, out var record);
			DmarcRecord? result = null;

			try {
				result = DmarcRecord.Parse (text);
			} catch (ParseException) {
				Fuzz.Check (!parsed, "DmarcRecord.Parse threw a ParseException but TryParse succeeded");
				return;
			}

			Fuzz.Check (parsed, "DmarcRecord.Parse succeeded but TryParse failed");
			Fuzz.Equal (Dump (record!), Dump (result), "DmarcRecord Parse vs TryParse");

			Fuzz.Check (Enum.IsDefined (result.DkimAlignment) && Enum.IsDefined (result.SpfAlignment), "DmarcRecord has an undefined alignment mode");
			Fuzz.Check (Enum.IsDefined (result.PublicSuffixDomain), "DmarcRecord has an undefined PublicSuffixDomain");
			Fuzz.Check (result.Policy is null || Enum.IsDefined (result.Policy.Value), "DmarcRecord has an undefined Policy");
			Fuzz.Check (result.SubdomainPolicy is null || Enum.IsDefined (result.SubdomainPolicy.Value), "DmarcRecord has an undefined SubdomainPolicy");
			Fuzz.Check (result.NonExistentSubdomainPolicy is null || Enum.IsDefined (result.NonExistentSubdomainPolicy.Value), "DmarcRecord has an undefined NonExistentSubdomainPolicy");

			foreach (var uri in result.AggregateReportUris.Concat (result.FailureReportUris))
				Fuzz.Check (uri != null && uri.Length > 0, "DmarcRecord has a null or empty report URI");
		}

		static string Dump (DmarcRecord record)
		{
			var builder = new StringBuilder ();

			builder.Append ("p=").Append (record.Policy?.ToString () ?? "(null)");
			builder.Append ("; sp=").Append (record.SubdomainPolicy?.ToString () ?? "(null)");
			builder.Append ("; np=").Append (record.NonExistentSubdomainPolicy?.ToString () ?? "(null)");
			builder.Append ("; psd=").Append (record.PublicSuffixDomain);
			builder.Append ("; t=").Append (record.Testing);
			builder.Append ("; adkim=").Append (record.DkimAlignment);
			builder.Append ("; aspf=").Append (record.SpfAlignment);
			builder.Append ("; rua=").Append (string.Join (",", record.AggregateReportUris));
			builder.Append ("; ruf=").Append (string.Join (",", record.FailureReportUris));
			builder.Append ("; fo=").Append (record.FailureReportingOptions);
			builder.Append ("; errors=").Append (record.Errors);

			return builder.ToString ();
		}

		static bool IsWhiteSpace (char c)
		{
			return c == ' ' || c == '\t' || c == '\r' || c == '\n';
		}

		static void CheckTagValueList (string text)
		{
			if (!TagValueList.TryParse (text, out var tags))
				return;

			Fuzz.Check (tags.Count <= TagValueList.MaxTags, $"TagValueList.TryParse returned {tags.Count} tags");

			foreach (var tag in tags) {
				Fuzz.Check (tag.Key.Length > 0 && char.IsAsciiLetter (tag.Key[0]), $"TagValueList has a malformed tag name: \"{Fuzz.Escape (tag.Key)}\"");
				Fuzz.Check (tag.Value.IndexOf (';') == -1, $"TagValueList has a value containing ';': \"{Fuzz.Escape (tag.Value)}\"");
				Fuzz.Check (tag.Value.Length == 0 || (!IsWhiteSpace (tag.Value[0]) && !IsWhiteSpace (tag.Value[tag.Value.Length - 1])), $"TagValueList did not trim a value: \"{Fuzz.Escape (tag.Value)}\"");

				var compact = TagValueList.RemoveWhiteSpace (tag.Value);

				Fuzz.Check (compact.IndexOfAny (new[] { ' ', '\t', '\r', '\n' }) == -1, $"TagValueList.RemoveWhiteSpace left whitespace: \"{Fuzz.Escape (compact)}\"");

				foreach (var item in TagValueList.SplitColonList (tag.Value)) {
					Fuzz.Check (item.Length > 0 && item.IndexOf (':') == -1, $"TagValueList.SplitColonList returned a malformed item: \"{Fuzz.Escape (item)}\"");
					Fuzz.Check (!IsWhiteSpace (item[0]) && !IsWhiteSpace (item[item.Length - 1]), $"TagValueList.SplitColonList did not trim an item: \"{Fuzz.Escape (item)}\"");
				}
			}
		}

		// The DKIM-Signature and ARC-* header values are parsed with ParseParameterTags, which may only fail with a
		// FormatException (which the verifiers turn into a PermError).
		static void CheckParameterTags (string text)
		{
			Dictionary<string, string> parameters;

			try {
				parameters = DkimVerifierBase.ParseParameterTags (HeaderId.DkimSignature, text);
			} catch (FormatException) {
				return;
			}

			foreach (var parameter in parameters)
				Fuzz.Check (parameter.Value.IndexOfAny (new[] { ' ', '\t', ';' }) == -1, $"ParseParameterTags returned an unfolded value: \"{Fuzz.Escape (parameter.Value)}\"");

			try {
				DkimVerifierBase.ValidateCommonParameters ("DKIM-Signature", parameters, out _, out _, out _, out _, out _);
			} catch (FormatException) {
			}
		}

		static void CheckDomainName (string text)
		{
			if (!DnsDomainName.TryNormalize (text, out var normalized))
				return;

			Fuzz.Check (normalized.Length > 0 && normalized.Length <= DnsDomainName.MaxLength, $"DnsDomainName.TryNormalize returned a name with length {normalized.Length}");

			int labelLength = 0;

			foreach (var c in normalized) {
				if (c == '.') {
					Fuzz.Check (labelLength > 0, $"DnsDomainName.TryNormalize returned an empty label: \"{Fuzz.Escape (normalized)}\"");
					labelLength = 0;
				} else {
					Fuzz.Check (char.IsAsciiLetterLower (c) || char.IsAsciiDigit (c) || c == '-' || c == '_', $"DnsDomainName.TryNormalize returned an invalid character: \"{Fuzz.Escape (normalized)}\"");
					labelLength++;
					Fuzz.Check (labelLength <= DnsDomainName.MaxLabelLength, $"DnsDomainName.TryNormalize returned a label that is too long: \"{Fuzz.Escape (normalized)}\"");
				}
			}

			Fuzz.Check (labelLength > 0, $"DnsDomainName.TryNormalize returned an empty label: \"{Fuzz.Escape (normalized)}\"");

			Fuzz.Check (DnsDomainName.TryNormalize (normalized, out var again), $"DnsDomainName.TryNormalize rejected its own output: \"{Fuzz.Escape (normalized)}\"");
			Fuzz.Equal (normalized, again!, "DnsDomainName.TryNormalize idempotence");

			if (DnsDomainName.TryCombine ("_dmarc", normalized, out var combined)) {
				Fuzz.Check (combined.Length <= DnsDomainName.MaxLength, $"DnsDomainName.TryCombine returned a name with length {combined.Length}");
				Fuzz.Check (DnsDomainName.TryNormalize (combined, out var check) && check == combined, $"DnsDomainName.TryCombine returned an invalid name: \"{Fuzz.Escape (combined)}\"");
			} else {
				Fuzz.Check (normalized.Length + "._dmarc".Length > DnsDomainName.MaxLength, $"DnsDomainName.TryCombine rejected a short name: \"{Fuzz.Escape (normalized)}\"");
			}
		}
	}
}

//
// VerifierChecks.cs
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
using System.Diagnostics;

using MimeKit.Cryptography;

namespace MimeKit.Fuzzing.Helpers {
	// Shared checks and result dumps for the DKIM, ARC and DMARC verifier targets.
	static class VerifierChecks
	{
		public static MimeMessage? ParseMessage (ReadOnlySpan<byte> data)
		{
			try {
				return MimeRoundTrip.Parse (data.ToArray (), false);
			} catch (FormatException) {
				return null;
			}
		}

		public static FormatOptions CreateFormatOptions (NewLineFormat format)
		{
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = format;
			return options;
		}

		// The verifiers catch every exception thrown while verifying a signature and report it in the result,
		// which would hide MimeKit bugs. The only exceptions that are expected are FormatExceptions for malformed
		// input, exceptions thrown by BouncyCastle for invalid keys or signatures, and the simulated DNS failures.
		public static void CheckException (Exception? ex, string what)
		{
			if (ex is null || ex is FormatException || ex is FuzzDnsException)
				return;

			foreach (var frame in new StackTrace (ex, false).GetFrames ()) {
				var ns = frame.GetMethod ()?.DeclaringType?.Namespace;

				if (ns is null || ns == "System" || ns.StartsWith ("System.", StringComparison.Ordinal))
					continue;

				if (ns.StartsWith ("Org.BouncyCastle", StringComparison.Ordinal))
					return;

				break;
			}

			throw new FuzzInvariantException ($"{what} reported an unexpected exception: {ex}");
		}

		static void AppendException (StringBuilder builder, Exception? ex)
		{
			if (ex != null)
				builder.Append (" exception=").Append (ex.GetType ().FullName).Append (": ").Append (ex.Message);
		}

		public static void Dump (StringBuilder builder, DkimSignatureValidationResult result)
		{
			builder.Append ("status=").Append (result.Status);
			builder.Append (" d=").Append (result.Domain ?? "(null)");
			builder.Append (" s=").Append (result.Selector ?? "(null)");
			builder.Append (" i=").Append (result.AgentOrUserIdentifier ?? "(null)");
			builder.Append (" a=").Append (result.SignatureAlgorithm?.ToString () ?? "(null)");
			builder.Append (" reason=").Append (result.Reason ?? "(null)");
			AppendException (builder, result.Exception);
			builder.Append (" header=").Append (result.Header?.Field ?? "(null)");
			builder.Append ('\n');
		}

		public static string Dump (DkimSignatureValidationResult[] results)
		{
			var builder = new StringBuilder ();

			foreach (var result in results)
				Dump (builder, result);

			return builder.ToString ();
		}

		static void Dump (StringBuilder builder, string what, ArcHeaderValidationResult? result)
		{
			builder.Append (what);

			if (result is null) {
				builder.Append (" (null)\n");
				return;
			}

			builder.Append (" signature=").Append (result.Signature);
			builder.Append (" reason=").Append (result.Reason ?? "(null)");
			AppendException (builder, result.Exception);
			builder.Append ('\n');
		}

		public static string Dump (ArcValidationResult result)
		{
			var builder = new StringBuilder ();

			builder.Append ("chain=").Append (result.Chain).Append (" errors=").Append (result.ChainErrors).Append ('\n');
			Dump (builder, "ams", result.MessageSignature);

			if (result.Seals != null) {
				for (int i = 0; i < result.Seals.Length; i++)
					Dump (builder, "as[" + i + "]", result.Seals[i]);
			}

			return builder.ToString ();
		}

		public static void CheckResult (DkimSignatureValidationResult result)
		{
			CheckException (result.Exception, "DkimVerifier");

			Fuzz.Check (Enum.IsDefined (result.Status), $"DkimSignatureValidationResult has an undefined Status: {result.Status}");
			Fuzz.Check (result.Header != null && result.Header.Id == HeaderId.DkimSignature, "DkimSignatureValidationResult.Header is not a DKIM-Signature header");

			if (result.Status == DkimSignatureStatus.Pass) {
				Fuzz.Check (result.Exception is null && result.Reason is null, $"DkimSignatureValidationResult passed with a reason: {result.Reason}");
				Fuzz.Check (!string.IsNullOrEmpty (result.Domain) && !string.IsNullOrEmpty (result.Selector) && result.SignatureAlgorithm.HasValue, "DkimSignatureValidationResult passed without a domain, selector and algorithm");
			} else {
				Fuzz.Check (result.Reason != null, $"DkimSignatureValidationResult has status {result.Status} without a reason");
			}
		}

		public static void CheckResult (ArcValidationResult result)
		{
			Fuzz.Check (Enum.IsDefined (result.Chain), $"ArcValidationResult has an undefined Chain: {result.Chain}");

			if (result.Chain == ArcSignatureValidationResult.Pass)
				Fuzz.Check (result.ChainErrors == ArcValidationErrors.None, $"ArcValidationResult passed with errors: {result.ChainErrors}");
			else if (result.Chain == ArcSignatureValidationResult.Fail)
				Fuzz.Check (result.ChainErrors != ArcValidationErrors.None, "ArcValidationResult failed without any errors");

			if (result.MessageSignature != null) {
				CheckException (result.MessageSignature.Exception, "ArcVerifier (ARC-Message-Signature)");
				Fuzz.Check (result.MessageSignature.Header.Id == HeaderId.ArcMessageSignature, "ArcValidationResult.MessageSignature.Header is not an ARC-Message-Signature header");
			}

			if (result.Seals != null) {
				foreach (var seal in result.Seals) {
					// Every seal is verified, even after an earlier one has failed.
					Fuzz.Check (seal != null, "ArcValidationResult.Seals contains a null result");
					CheckException (seal!.Exception, "ArcVerifier (ARC-Seal)");
					Fuzz.Check (seal.Header.Id == HeaderId.ArcSeal, "ArcValidationResult.Seals contains a result for a header that is not an ARC-Seal");
				}
			}
		}

		// The verifier results are meant to be added to an Authentication-Results header, and parsing that header
		// back must give exactly the same results.
		public static void CheckAuthenticationResults (IEnumerable<AuthenticationMethodResult> results, string what)
		{
			var authres = new AuthenticationResults ("fuzz.example");

			authres.Results.AddRange (results);

			var text = authres.ToString ();

			Fuzz.Check (AuthenticationResults.TryParse (Fuzz.Utf8.GetBytes (text), out var reparsed), $"{what}: AuthenticationResults failed to parse the result: \"{Fuzz.Escape (text)}\"");
			Fuzz.Equal (text, reparsed!.ToString (), $"{what}: AuthenticationResults round-trip");
		}
	}
}

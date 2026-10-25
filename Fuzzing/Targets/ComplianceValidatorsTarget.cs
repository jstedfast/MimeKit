//
// ComplianceValidatorsTarget.cs
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

using MimeKit.Fuzzing.Helpers;

namespace MimeKit.Fuzzing.Targets {
	// Exercises the compliance validators (base64, quoted-printable, uuencode and address) and checks that
	// enabling them never changes what the reader reports, that every issue is well-formed and that the
	// per-violation cap is honored.
	sealed class ComplianceValidatorsTarget : IFuzzTarget
	{
		static readonly MimeComplianceValidators[] Validators = {
			MimeComplianceValidators.All,
			MimeComplianceValidators.Base64,
			MimeComplianceValidators.QuotedPrintable,
			MimeComplianceValidators.UUEncode,
			MimeComplianceValidators.Address,
		};

		public string Name => "compliance-validators";

		public int MaxLength => 256 * 1024;

		public void Run (ReadOnlySpan<byte> data)
		{
			var input = data.ToArray ();
			uint selector = Fuzz.Selector (data);
			int cap = (selector % 3) switch {
				0 => 1,
				1 => int.MaxValue,
				_ => MimeComplianceOptions.Default.MaxIssuesPerViolation,
			};
			var context = ((selector >> 2) & 1) != 0 ? MimeComplianceContext.Storage : MimeComplianceContext.Transport;
			var validators = Validators[(selector >> 3) % Validators.Length];

			var baseline = Read (input, null, null);
			var checker = new MimeComplianceChecker (context, cap, input.Length);
			var options = new MimeComplianceOptions {
				EnabledValidators = validators,
				MaxIssuesPerViolation = cap,
				Context = context,
			};
			var logged = Read (input, options, checker);

			Fuzz.Equal (baseline, logged, $"MimeReader callbacks changed when the {validators} validators were enabled");

			var parsed = Parse (input, null, null);
			var checker2 = new MimeComplianceChecker (context, cap, input.Length);
			var parsedLogged = Parse (input, options, checker2);

			Fuzz.Equal (parsed, parsedLogged, $"MimeParser output changed when the {validators} validators were enabled");
			Fuzz.Equal (checker.Trace, checker2.Trace, "MimeReader vs MimeParser compliance issues");
		}

		static string Read (byte[] input, MimeComplianceOptions? options, MimeComplianceChecker? checker)
		{
			var reader = new RecordingMimeReader (MimeRoundTrip.CreateParserOptions (), new MemoryStream (input, false), MimeFormat.Entity);

			if (checker != null) {
				reader.ComplianceOptions = options!;
				reader.ComplianceLogger = checker;
				checker.BeginOperation ();
			}

			try {
				reader.ReadMessage ();
			} catch (FormatException ex) {
				reader.Append ($"FormatException: {ex.Message}");
			}

			return reader.Trace;
		}

		static string Parse (byte[] input, MimeComplianceOptions? options, MimeComplianceChecker? checker)
		{
			var parser = new MimeParser (MimeRoundTrip.CreateParserOptions (), new MemoryStream (input, false), MimeFormat.Entity, MimeContentStorage.Memory);

			if (checker != null) {
				parser.ComplianceOptions = options!;
				parser.ComplianceLogger = checker;
				checker.BeginOperation ();
			}

			try {
				var message = parser.ParseMessage ();

				return Fuzz.Latin1.GetString (MimeRoundTrip.Serialize (message));
			} catch (FormatException ex) {
				return $"{ex.GetType ().Name}: {ex.Message}";
			}
		}
	}
}

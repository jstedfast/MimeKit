//
// SmtpUtf8ValidatorTarget.cs
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

using MimeKit.Fuzzing.Helpers;
using MimeKit.Utils;

namespace MimeKit.Fuzzing.Targets {
	// Validates the input as an SMTPUTF8 address with every combination of options and checks that the results
	// are consistent with each other and with the documented meaning of each option.
	sealed class SmtpUtf8ValidatorTarget : IFuzzTarget
	{
		const SmtpUtf8AddressValidationOptions DecodeIdn = SmtpUtf8AddressValidationOptions.DecodeIdn;
		const SmtpUtf8AddressValidationOptions AsciiIsLatin = SmtpUtf8AddressValidationOptions.AsciiIsLatin;
		const SmtpUtf8AddressValidationOptions EnforceLengthLimits = SmtpUtf8AddressValidationOptions.EnforceLengthLimits;
		const int OptionCombinations = 8;

		static readonly Encoding Utf16 = new UnicodeEncoding (false, false, false);

		public string Name => "smtputf8-validator";

		public int MaxLength => 4096;

		public void Run (ReadOnlySpan<byte> data)
		{
			// Decoding the input as UTF-16 lets the fuzzer produce lone surrogates, which UTF-8 decoding never does.
			var address = (Fuzz.Selector (data) & 1) != 0 ? Utf16.GetString (data) : Fuzz.Utf8.GetString (data);
			var errors = new SmtpUtf8AddressError[OptionCombinations];

			for (int i = 0; i < OptionCombinations; i++)
				errors[i] = Validate (address, (SmtpUtf8AddressValidationOptions) i);

			for (int i = 0; i < OptionCombinations; i++) {
				var options = (SmtpUtf8AddressValidationOptions) i;

				if ((options & EnforceLengthLimits) == 0)
					CheckEnforceLengthLimits (address, options, errors[i], errors[i | (int) EnforceLengthLimits]);

				if ((options & AsciiIsLatin) == 0)
					CheckAsciiIsLatin (address, options, errors[i], errors[i | (int) AsciiIsLatin]);

				if ((options & DecodeIdn) == 0)
					CheckDecodeIdn (address, options, errors[i], errors[i | (int) DecodeIdn]);
			}

			if (errors[0] == SmtpUtf8AddressError.None)
				CheckMailbox (address);
		}

		static SmtpUtf8AddressError Validate (string address, SmtpUtf8AddressValidationOptions options)
		{
			bool valid = SmtpUtf8AddressValidator.TryValidate (address, options, out var error);

			Fuzz.Check (valid == (error == SmtpUtf8AddressError.None), $"TryValidate returned {valid} but reported {error} for \"{Fuzz.Escape (address)}\" ({options})");
			Fuzz.Check (SmtpUtf8AddressValidator.IsValid (address, options) == valid, $"IsValid and TryValidate disagree for \"{Fuzz.Escape (address)}\" ({options})");

			SmtpUtf8AddressValidator.TryValidate (address, options, out var again);
			Fuzz.Check (again == error, $"TryValidate is not deterministic for \"{Fuzz.Escape (address)}\" ({options}): {error} vs {again}");

			if (options == SmtpUtf8AddressValidationOptions.None) {
				SmtpUtf8AddressValidator.TryValidate (address, out var defaultError);
				Fuzz.Check (defaultError == error, $"TryValidate without options disagrees with None for \"{Fuzz.Escape (address)}\": {defaultError} vs {error}");
			}

			return error;
		}

		static bool IsLengthError (SmtpUtf8AddressError error)
		{
			switch (error) {
			case SmtpUtf8AddressError.LocalPartTooLong:
			case SmtpUtf8AddressError.LabelTooLong:
			case SmtpUtf8AddressError.DomainTooLong:
			case SmtpUtf8AddressError.AddressTooLong:
				return true;
			default:
				return false;
			}
		}

		// Enforcing the length limits may only add a length error; it never changes any other result.
		static void CheckEnforceLengthLimits (string address, SmtpUtf8AddressValidationOptions options, SmtpUtf8AddressError without, SmtpUtf8AddressError with)
		{
			Fuzz.Check (!IsLengthError (without), $"A length error was reported without EnforceLengthLimits for \"{Fuzz.Escape (address)}\" ({options}): {without}");

			if (without == SmtpUtf8AddressError.Syntax)
				Fuzz.Check (with == SmtpUtf8AddressError.Syntax, $"EnforceLengthLimits changed a syntax error into {with} for \"{Fuzz.Escape (address)}\" ({options})");
			else
				Fuzz.Check (with == without || IsLengthError (with), $"EnforceLengthLimits changed {without} into {with} for \"{Fuzz.Escape (address)}\" ({options})");
		}

		// Treating ASCII as Latin is strictly stricter: it may only turn a valid address into a MixedScripts error.
		static void CheckAsciiIsLatin (string address, SmtpUtf8AddressValidationOptions options, SmtpUtf8AddressError without, SmtpUtf8AddressError with)
		{
			if (without == SmtpUtf8AddressError.None)
				Fuzz.Check (with == SmtpUtf8AddressError.None || with == SmtpUtf8AddressError.MixedScripts, $"AsciiIsLatin changed a valid address into {with} for \"{Fuzz.Escape (address)}\" ({options})");
			else
				Fuzz.Check (with == without, $"AsciiIsLatin changed {without} into {with} for \"{Fuzz.Escape (address)}\" ({options})");
		}

		// Decoding IDN only affects addresses that would otherwise be rejected because of an A-label.
		static void CheckDecodeIdn (string address, SmtpUtf8AddressValidationOptions options, SmtpUtf8AddressError without, SmtpUtf8AddressError with)
		{
			if (without != SmtpUtf8AddressError.ALabel)
				Fuzz.Check (with == without, $"DecodeIdn changed {without} into {with} for \"{Fuzz.Escape (address)}\" ({options})");
			else
				Fuzz.Check (!IsLengthError (with) && with != SmtpUtf8AddressError.Syntax, $"DecodeIdn changed an A-label error into {with} for \"{Fuzz.Escape (address)}\" ({options})");
		}

		// A valid SMTPUTF8 address must also be accepted by MimeKit's address parser and must still be valid
		// once it has been parsed.
		static void CheckMailbox (string address)
		{
			var options = new ParserOptions { AddressParserComplianceMode = RfcComplianceMode.Strict };

			Fuzz.Check (MailboxAddress.TryParse (options, address, out var mailbox), $"MailboxAddress failed to parse a valid SMTPUTF8 address: \"{Fuzz.Escape (address)}\"");

			bool valid = SmtpUtf8AddressValidator.TryValidate (mailbox!, out var error);

			Fuzz.Check (valid, $"The parsed mailbox \"{Fuzz.Escape (mailbox!.Address)}\" is not a valid SMTPUTF8 address ({error}) even though \"{Fuzz.Escape (address)}\" is");
			Fuzz.Check (SmtpUtf8AddressValidator.IsValid (mailbox!), $"IsValid and TryValidate disagree for the parsed mailbox \"{Fuzz.Escape (mailbox!.Address)}\"");
		}
	}
}

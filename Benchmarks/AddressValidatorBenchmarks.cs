//
// AddressValidatorBenchmarks.cs
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
using System.Linq;
using System.Text;

using MimeKit;

using BenchmarkDotNet.Attributes;

namespace Benchmarks {
	/// <summary>
	/// Benchmarks for the address header compliance validator.
	/// </summary>
	/// <remarks>
	/// <para>The validator runs over the value of every address header of every message that is
	/// parsed with compliance reporting turned on, so its cost is paid on the hot path of a mail
	/// server rather than on demand.</para>
	/// <para>The cases below fall into two groups. The conformant ones describe what the cost
	/// actually is in production, where nearly all mail is well formed. The malformed ones exist
	/// because an address header is attacker-controlled: a shape that is merely unusual must not be
	/// dramatically more expensive to describe than the equivalent well-formed one, or reporting on
	/// it becomes a denial of service. Each of those is annotated with the shape it guards.</para>
	/// </remarks>
	[MemoryDiagnoser]
	public class AddressValidatorBenchmarks
	{
		// Note: A logger that does nothing keeps the benchmarks focused on the cost of scanning the
		// value rather than on the cost of recording any issues that are found within it.
		class NullMimeComplianceLogger : IMimeComplianceLogger
		{
			public void Log (in MimeComplianceIssue issue)
			{
			}
		}

		readonly NullMimeComplianceLogger logger = new NullMimeComplianceLogger ();

		readonly byte[] SingleAddress;
		readonly byte[] TenRecipients;
		readonly byte[] HundredRecipients;
		readonly byte[] ThousandRecipients;
		readonly byte[] InternationalDisplayNames;
		readonly byte[] QuotedDisplayNames;
		readonly byte[] Groups;
		readonly byte[] LongAtoms;
		readonly byte[] BareAddrspecs;
		readonly byte[] MissingSeparators;
		readonly byte[] ExtraneousCommas;
		readonly byte[] UnbalancedAngleBrackets;
		readonly byte[] ControlCharacters;
		readonly byte[] NullBytes;
		readonly byte[] Iso2022Sequences;

		public AddressValidatorBenchmarks ()
		{
			SingleAddress = Encode ("Joe Sixpack <joe@example.com>");
			TenRecipients = Encode (Join (", ", 10, i => $"User {i} <u{i}@example.com>"));
			HundredRecipients = Encode (Join (", ", 100, i => $"User {i} <u{i}@example.com>"));
			ThousandRecipients = Encode (Join (", ", 1000, i => $"User {i} <u{i}@example.com>"));

			// Note: rfc6532 makes the 8-bit range ordinary address text, so this is conformant and is
			// here to keep any byte classification from quietly treating non-ASCII as interesting.
			InternationalDisplayNames = Encode (Join (", ", 100, i => $"Ren\u00e9e M\u00fcller {i} <u{i}@example.com>"));

			QuotedDisplayNames = Encode (Join (", ", 100, i => $"\"Doe, John {i}\" <u{i}@example.com>"));
			Groups = Encode (Join ("; ", 20, g => $"Group {g}: " + Join (", ", 5, i => $"u{g}_{i}@example.com")) + ";");

			// Note: Atoms far longer than the handful of characters a real local-part or domain label
			// runs to, which is the shape that decides whether scanning one is worth vectorizing.
			LongAtoms = Encode (Join (", ", 100, i => $"{new string ('a', 48)}{i}@{new string ('b', 48)}.example.com"));

			BareAddrspecs = Encode (Join (", ", 500, i => $"u{i}@example.com"));

			// Note: An address list written without any separators. A phrase scan runs forward until
			// it finds something that could end a display-name, so this used to re-scan the whole
			// remainder of the value once per address, at O(length squared).
			MissingSeparators = Encode (Join (" ", 500, i => $"u{i}@example.com"));

			// Note: One violation per byte. Working out the line and column of a violation used to
			// start counting from the beginning of the value every time, at O(length x violations).
			ExtraneousCommas = Encode (new string (',', 2000));

			UnbalancedAngleBrackets = Encode (Join (", ", 500, i => $"<<u{i}@example.com>"));

			// Note: Control characters, null bytes and ISO-2022 sequences are recorded during an
			// up-front scan and attributed to a token later, so these exercise the paths that allocate.
			ControlCharacters = Encode (Join (", ", 100, i => $"a\u0001b {i} <u{i}@example.com>"));
			NullBytes = Encode (Join (", ", 500, i => $"a\0b {i} <u{i}@example.com>"));
			Iso2022Sequences = Encode (Join (", ", 500, i => $"\u001b$B{i}\u001b(B@example.com"));
		}

		static string Join (string separator, int count, Func<int, string> selector)
		{
			return string.Join (separator, Enumerable.Range (0, count).Select (selector));
		}

		// Note: Latin1 round-trips every byte value, which lets a case embed raw 8-bit bytes such as
		// the escapes of an ISO-2022 sequence without them being re-encoded.
		static byte[] Encode (string value)
		{
			return Encoding.Latin1.GetBytes (value);
		}

		// Note: MimeReader constructs a validator for each address header it encounters, so the
		// allocation is part of what each of these costs in practice.
		void Validate (byte[] value)
		{
			// Note: Column 5 is where the value of a "To:" header begins.
			var validator = new AddressValidator (logger, MimeComplianceContext.Transport, 0, 1, 5);

			validator.Validate (value, 0, value.Length);
		}

		[Benchmark]
		public void SingleAddressValidate ()
		{
			Validate (SingleAddress);
		}

		[Benchmark]
		public void TenRecipientsValidate ()
		{
			Validate (TenRecipients);
		}

		[Benchmark]
		public void HundredRecipientsValidate ()
		{
			Validate (HundredRecipients);
		}

		[Benchmark]
		public void ThousandRecipientsValidate ()
		{
			Validate (ThousandRecipients);
		}

		[Benchmark]
		public void InternationalDisplayNamesValidate ()
		{
			Validate (InternationalDisplayNames);
		}

		[Benchmark]
		public void QuotedDisplayNamesValidate ()
		{
			Validate (QuotedDisplayNames);
		}

		[Benchmark]
		public void GroupsValidate ()
		{
			Validate (Groups);
		}

		[Benchmark]
		public void LongAtomsValidate ()
		{
			Validate (LongAtoms);
		}

		[Benchmark]
		public void BareAddrspecsValidate ()
		{
			Validate (BareAddrspecs);
		}

		[Benchmark]
		public void MissingSeparatorsValidate ()
		{
			Validate (MissingSeparators);
		}

		[Benchmark]
		public void ExtraneousCommasValidate ()
		{
			Validate (ExtraneousCommas);
		}

		[Benchmark]
		public void UnbalancedAngleBracketsValidate ()
		{
			Validate (UnbalancedAngleBrackets);
		}

		[Benchmark]
		public void ControlCharactersValidate ()
		{
			Validate (ControlCharacters);
		}

		[Benchmark]
		public void NullBytesValidate ()
		{
			Validate (NullBytes);
		}

		[Benchmark]
		public void Iso2022SequencesValidate ()
		{
			Validate (Iso2022Sequences);
		}
	}
}

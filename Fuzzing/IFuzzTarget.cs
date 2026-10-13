//
// IFuzzTarget.cs
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

using System.Diagnostics.CodeAnalysis;

using MimeKit.Fuzzing.Targets;

namespace MimeKit.Fuzzing {
	// A single fuzz target. Run must only let an exception escape when it has found a bug: either an
	// undocumented exception thrown by MimeKit or a FuzzInvariantException thrown by the harness itself.
	interface IFuzzTarget
	{
		string Name { get; }

		// The maximum input length passed to libFuzzer via -max_len.
		int MaxLength { get; }

		void Run (ReadOnlySpan<byte> data);
	}

	static class FuzzTargets
	{
		public static readonly IReadOnlyList<IFuzzTarget> All = new IFuzzTarget[] {
			new TnefReaderTarget (),
			new TnefMessageTarget (),
			new RtfCompressedTarget (),
			new MimeReaderTarget (),
			new MimeParserTarget (),
			new AddressParserTarget (),
			new ComplianceValidatorsTarget (),
			new Rfc2047Target (),
			new HeaderValuesTarget (),
			new DecodersTarget (),
			new EncodersTarget (),
			new RtfConvertersTarget (),
			new RtfTokenizerTarget (),
			new SmtpUtf8ValidatorTarget (),
			new AuthenticationResultsTarget (),
			new DnsRecordsTarget (),
			new DkimArcVerifierTarget (),
			new DmarcVerifierTarget (),
			new HtmlTokenizerTarget (),
			new TextConvertersTarget (),
		};

		public static bool TryGet (string name, [NotNullWhen (true)] out IFuzzTarget? target)
		{
			target = All.FirstOrDefault (t => t.Name.Equals (name, StringComparison.OrdinalIgnoreCase));

			return target != null;
		}
	}
}

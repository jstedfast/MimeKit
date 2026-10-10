//
// AddressParserTarget.cs
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
	// Parses the input as an address list, a mailbox and a group, then checks that formatting the result
	// and parsing it again is a fixed point.
	sealed class AddressParserTarget : IFuzzTarget
	{
		public string Name => "address-parser";

		const int MaxConvergenceRounds = 8;

		public int MaxLength => 64 * 1024;

		public void Run (ReadOnlySpan<byte> data)
		{
			var input = data.ToArray ();
			uint selector = Fuzz.Selector (data);
			var options = new ParserOptions {
				AddressParserComplianceMode = (selector % 3) switch {
					0 => RfcComplianceMode.Strict,
					1 => RfcComplianceMode.Loose,
					_ => RfcComplianceMode.Looser,
				},
				AllowAddressesWithoutDomain = (selector & 4) != 0,
				AllowUnquotedCommasInAddresses = (selector & 8) != 0,
				MaxAddressGroupDepth = 4,
			};
			var format = MimeRoundTrip.CreateFormatOptions ();

			if (InternetAddressList.TryParse (options, input, out var list))
				CheckList (options, format, list);

			if (MailboxAddress.TryParse (options, input, out var mailbox))
				CheckAddress (options, format, mailbox, "MailboxAddress");

			if (GroupAddress.TryParse (options, input, out var group))
				CheckAddress (options, format, group, "GroupAddress");

			if (InternetAddress.TryParse (options, input, out var address))
				CheckAddress (options, format, address, "InternetAddress");
		}

		static void CheckList (ParserOptions options, FormatOptions format, InternetAddressList list)
		{
			var first = list.ToString (format, true);
			GC.KeepAlive (list.ToString (format, false));
			GC.KeepAlive (list.ToString ());

			Fuzz.Check (InternetAddressList.TryParse (options, Fuzz.Utf8.GetBytes (first), out var reparsed), $"InternetAddressList failed to parse its own output: \"{Fuzz.Escape (first)}\"");

			var second = reparsed!.ToString (format, true);

			CheckConverges (options, format, list, reparsed, first, second, "InternetAddressList");
		}

		static void CheckAddress (ParserOptions options, FormatOptions format, InternetAddress address, string what)
		{
			var first = address.ToString (format, true);
			GC.KeepAlive (address.ToString (format, false));
			GC.KeepAlive (address.ToString ());

			Fuzz.Check (InternetAddressList.TryParse (options, Fuzz.Utf8.GetBytes (first), out var reparsed), $"{what} output failed to parse: \"{Fuzz.Escape (first)}\"");

			var second = reparsed!.ToString (format, true);

			CheckConverges (options, format, address, reparsed, first, second, what);
		}

		static void CheckConverges (ParserOptions options, FormatOptions format, object original, InternetAddressList reparsed, string first, string second, string what)
		{
			if (first == second)
				return;

			// Note: Folding (StringBuilderExtensions.AppendFolded) intentionally collapses runs of whitespace between words
			// into a single space (or a fold), which is permitted by RFC 5322, and leading/trailing whitespace in a display
			// name can be lost when it is adjacent to an rfc2047 encoded-word. This means that a display name's whitespace
			// can change on each round trip (whitespace that ends up between two encoded-words is dropped when decoding).
			// Allow this as long as the only difference is whitespace and the output reaches a fixed point within a few
			// more round trips.
			var originalText = original is InternetAddress address ? Describe (address) + ", " : Describe ((InternetAddressList) original);

			Fuzz.Equal (originalText, Describe (reparsed), $"{what} parse(format(x)) (ignoring display-name whitespace)");

			for (int round = 0; round < MaxConvergenceRounds; round++) {
				Fuzz.Check (InternetAddressList.TryParse (options, Fuzz.Utf8.GetBytes (second), out var again), $"{what} output failed to parse: \"{Fuzz.Escape (second)}\"");
				Fuzz.Equal (originalText, Describe (again!), $"{what} parse(format(...)) (ignoring display-name whitespace)");

				var next = again!.ToString (format, true);

				if (next == second)
					return;

				second = next;
			}

			Fuzz.Check (false, $"{what} output did not converge after {MaxConvergenceRounds} round trips: \"{Fuzz.Escape (second)}\"");
		}

		static string Describe (InternetAddressList list)
		{
			var builder = new System.Text.StringBuilder ();

			foreach (var address in list)
				builder.Append (Describe (address)).Append (", ");

			return builder.ToString ();
		}

		static string Describe (InternetAddress address)
		{
			var name = CollapseWhitespace (address.Name ?? string.Empty);

			if (address is GroupAddress group)
				return name + ": " + Describe (group.Members) + ";";

			var mailbox = (MailboxAddress) address;

			// Note: IDNA mapping can legitimately change the Unicode form of a domain (e.g. U+200B ZERO WIDTH SPACE is
			// ignored), so compare the IDN-encoded forms (of the route as well). This can also leave a trailing '.'
			// (e.g. "example.com.\u200B") which the parser then drops.
			var route = string.Join (",", mailbox.Route.Select (domain => domain.Any (c => c > 127) ? MailboxAddress.IdnMapping.Encode (domain).TrimEnd ('.') : domain));

			return name + " <" + route + ":" + mailbox.GetAddress (true).TrimEnd ('.') + ">";
		}

		static string CollapseWhitespace (string text)
		{
			var builder = new System.Text.StringBuilder (text.Length);
			bool lwsp = false;

			foreach (var c in text) {
				if (char.IsWhiteSpace (c)) {
					lwsp = true;
					continue;
				}

				if (lwsp && builder.Length > 0)
					builder.Append (' ');

				builder.Append (c);
				lwsp = false;
			}

			return builder.ToString ();
		}
	}
}

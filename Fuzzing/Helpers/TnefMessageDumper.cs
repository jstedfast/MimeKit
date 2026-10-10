//
// TnefMessageDumper.cs
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

using MimeKit.Tnef;

namespace MimeKit.Fuzzing.Helpers {
	// Produces a canonical textual dump of a TnefMessage (including embedded messages) so that messages
	// loaded through different code paths can be compared.
	static class TnefMessageDumper
	{
		public static string Dump (TnefMessage message)
		{
			var builder = new StringBuilder ();

			Fuzz.Wait (DumpAsync (builder, message, 0, false));

			return builder.ToString ();
		}

		public static async Task<string> DumpAsync (TnefMessage message)
		{
			var builder = new StringBuilder ();

			await DumpAsync (builder, message, 0, true).ConfigureAwait (false);

			return builder.ToString ();
		}

		static void DumpProperties (StringBuilder builder, string indent, TnefPropertySet properties)
		{
			builder.Append (indent).Append ("properties ").Append (properties.Count).AppendLine ();

			for (int i = 0; i < properties.Count; i++) {
				var property = properties[i];

				builder.Append (indent).Append ("  ").Append (property.Tag).Append (' ').Append (property.PropertyType).Append (" n=").Append (property.Count)
					.Append (" name=").Append (property.Name?.ToString () ?? "-").Append (' ').Append (TnefWalker.FormatValue (property.Value)).AppendLine ();
			}
		}

		static void DumpBody (StringBuilder builder, string indent, string name, TnefMessageBody? body)
		{
			if (body == null)
				return;

			builder.Append (indent).Append (name).Append (' ').Append (body.Format).Append (' ').Append (body.Tag).Append (' ').Append (body.Length)
				.Append (' ').Append (body.Encoding?.WebName ?? "-");

			using (var stream = body.OpenRead ()) {
				var hash = Fuzz.Hash (stream, out var length);

				Fuzz.Check (length == body.Length, $"{name}: OpenRead returned {length} bytes but Length is {body.Length}");
				builder.Append (' ').Append (hash.ToString ("x16"));
			}

			using (var stream = body.OpenDecodedRead ())
				builder.Append (' ').Append (Fuzz.Hash (stream, out _).ToString ("x16"));

			builder.Append (' ').Append (Fuzz.HashString (body.GetText ())).AppendLine ();
		}

		static async Task DumpAsync (StringBuilder builder, TnefMessage message, int depth, bool useAsync)
		{
			var indent = new string (' ', depth * 2);

			builder.Append (indent).Append ("message cp=").Append (message.Codepage).Append (" key=").Append (message.LegacyKey)
				.Append (" class=").Append (Fuzz.HashString (message.MessageClass)).Append (" subject=").Append (Fuzz.HashString (message.Subject))
				.Append (" id=").Append (Fuzz.HashString (message.InternetMessageId)).AppendLine ();

			DumpProperties (builder, indent, message.Properties);
			DumpBody (builder, indent, "text", message.TextBody);
			DumpBody (builder, indent, "html", message.HtmlBody);
			DumpBody (builder, indent, "rtf", message.RtfBody);

			foreach (var recipient in message.Recipients) {
				builder.Append (indent).Append ("recipient ").Append (recipient.RecipientType).Append (' ').Append (Fuzz.HashString (recipient.DisplayName))
					.Append (' ').Append (Fuzz.HashString (recipient.EmailAddress)).Append (' ').Append (Fuzz.HashString (recipient.AddressType)).AppendLine ();
				DumpProperties (builder, indent + "  ", recipient.Properties);
			}

			foreach (var attachment in message.Attachments) {
				builder.Append (indent).Append ("attachment ").Append (attachment.Method).Append (' ').Append (attachment.Flags).Append (' ').Append (Fuzz.HashString (attachment.FileName))
					.Append (' ').Append (Fuzz.HashString (attachment.MimeType)).Append (' ').Append (Fuzz.HashString (attachment.ContentId))
					.Append (" content=").Append (attachment.HasContent).Append (' ').Append (attachment.Length).Append (" embedded=").Append (attachment.IsEmbeddedMessage).AppendLine ();
				DumpProperties (builder, indent + "  ", attachment.Properties);

				if (attachment.HasContent) {
					using (var stream = attachment.OpenRead ()) {
						var hash = Fuzz.Hash (stream, out var length);

						Fuzz.Check (length == attachment.Length, $"Attachment OpenRead returned {length} bytes but Length is {attachment.Length}");
						builder.Append (indent).Append ("  content ").Append (hash.ToString ("x16")).AppendLine ();
					}
				}

				if (attachment.IsEmbeddedMessage) {
					TnefMessage embedded;

					try {
						embedded = useAsync ? await attachment.LoadEmbeddedMessageAsync ().ConfigureAwait (false) : attachment.LoadEmbeddedMessage ();
					} catch (TnefException ex) when (ex.Violation == TnefComplianceViolation.InvalidSignature) {
						// Documented: the embedded message does not begin with the TNEF signature.
						builder.Append (indent).AppendLine ("  embedded: invalid signature");
						continue;
					}

					using (embedded)
						await DumpAsync (builder, embedded, depth + 1, useAsync).ConfigureAwait (false);
				}
			}
		}
	}
}

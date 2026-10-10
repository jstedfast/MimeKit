//
// TnefWalker.cs
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

using System.Collections;
using System.Globalization;
using System.Text;

using MimeKit.Tnef;

namespace MimeKit.Fuzzing.Helpers {
	// Walks a TnefReader (and every embedded reader) recording what it reads into a trace. The sync and async
	// walkers make identical choices so that their traces can be compared.
	//
	// Every value is consumed through one of the public value APIs. Variable-length values may only be
	// consumed once, so a counter rotates through the APIs; fixed-width values may be read any number of
	// times, so they are additionally read through every typed coercion.
	sealed class TnefWalker
	{
		readonly StringBuilder trace = new StringBuilder ();
		int counter;

		public string Trace => trace.ToString ();

		public static bool ContainsProperties (TnefAttributeTag tag)
		{
			switch (tag) {
			case TnefAttributeTag.MapiProperties:
			case TnefAttributeTag.Attachment:
			case TnefAttributeTag.RecipientTable:
				return true;
			default:
				return false;
			}
		}

		public static bool IsVariableLength (TnefPropertyType type)
		{
			switch (type) {
			case TnefPropertyType.Unicode:
			case TnefPropertyType.String8:
			case TnefPropertyType.Binary:
			case TnefPropertyType.Object:
				return true;
			default:
				return false;
			}
		}

		static bool IsScalar (TnefAttributeType type)
		{
			switch (type) {
			case TnefAttributeType.Short:
			case TnefAttributeType.Word:
			case TnefAttributeType.Long:
			case TnefAttributeType.DWord:
				return true;
			default:
				return false;
			}
		}

		public static string FormatValue (object? value)
		{
			switch (value) {
			case null:
				return "null";
			case string text:
				return "s" + Fuzz.HashString (text);
			case byte[] bytes:
				return $"b{bytes.Length}:{Fuzz.Hash (bytes):x16}";
			case DateTime dateTime:
				return dateTime.Ticks.ToString (CultureInfo.InvariantCulture) + dateTime.Kind;
			case double d:
				return BitConverter.DoubleToInt64Bits (d).ToString ("x16");
			case float f:
				return BitConverter.SingleToInt32Bits (f).ToString ("x8");
			case IFormattable formattable:
				return value.GetType ().Name + ":" + formattable.ToString (null, CultureInfo.InvariantCulture);
			case IEnumerable items:
				var builder = new StringBuilder ("[");

				foreach (var item in items)
					builder.Append (FormatValue (item)).Append (',');

				return builder.Append (']').ToString ();
			default:
				return value.GetType ().Name + ":" + value;
			}
		}

		void AppendAttribute (TnefReader reader, int depth)
		{
			trace.Append (depth).Append (" attr ").Append (reader.Level).Append (' ').Append (reader.Tag).Append (' ').Append (reader.AttributeType)
				.Append (' ').Append (reader.Length).Append (' ').Append (reader.StreamOffset).Append (" cp=").Append (reader.Codepage).Append (" key=").Append (reader.LegacyKey).AppendLine ();
		}

		void AppendProperty (TnefPropertyReader properties)
		{
			trace.Append ("  prop ").Append (properties.Tag).Append (' ').Append (properties.PropertyType).Append (" mv=").Append (properties.IsMultiValued)
				.Append (" n=").Append (properties.ValueCount).Append (" name=").Append (properties.Name?.ToString () ?? "-").AppendLine ();
		}

		// Fixed-width property values: read through every coercion, catching only the documented
		// InvalidOperationException for coercions that are not possible.
		void AppendFixedValue (TnefPropertyReader properties)
		{
			var first = FormatValue (properties.ReadValue ());

			trace.Append ("    value ").Append (first);
			Coerce (() => properties.ReadValueAsBoolean ());
			Coerce (() => properties.ReadValueAsInt16 ());
			Coerce (() => properties.ReadValueAsInt32 ());
			Coerce (() => properties.ReadValueAsInt64 ());
			Coerce (() => properties.ReadValueAsFloat ());
			Coerce (() => properties.ReadValueAsDouble ());
			Coerce (() => properties.ReadValueAsDateTime ());
			Coerce (() => properties.ReadValueAsGuid ());
			trace.AppendLine ();

			var second = FormatValue (properties.ReadValue ());

			Fuzz.Equal (first, second, $"Re-reading fixed-width property {properties.Tag}");
		}

		void Coerce (Func<object> read)
		{
			try {
				trace.Append (' ').Append (FormatValue (read ()));
			} catch (InvalidOperationException) {
				// Documented: the value cannot be coerced to the requested type.
				trace.Append (" !");
			}
		}

		public void Walk (TnefReader reader, int depth = 0)
		{
			while (reader.Read ()) {
				AppendAttribute (reader, depth);

				if (ContainsProperties (reader.Tag)) {
					WalkProperties (reader.GetPropertyReader (), reader.Tag == TnefAttributeTag.RecipientTable, depth);
				} else if (IsScalar (reader.AttributeType)) {
					int value = reader.ReadValueAsInt32 ();
					short value16 = reader.ReadValueAsInt16 ();

					Fuzz.Check (reader.ReadValueAsInt32 () == value, $"Re-reading scalar attribute {reader.Tag} returned a different value");
					trace.Append ("  scalar ").Append (value).Append (' ').Append (value16).AppendLine ();
				} else if (reader.AttributeType == TnefAttributeType.Date) {
					var value = reader.ReadValueAsDateTime ();

					Fuzz.Check (reader.ReadValueAsDateTime () == value, $"Re-reading date attribute {reader.Tag} returned a different value");
					trace.Append ("  date ").Append (FormatValue (value)).AppendLine ();
				} else {
					switch (counter++ % 3) {
					case 0:
						using (var stream = reader.OpenValueStream ()) {
							var hash = Fuzz.Hash (stream, out var length);
							trace.Append ("  raw ").Append (length).Append (' ').Append (hash.ToString ("x16")).AppendLine ();
						}
						break;
					case 1:
						trace.Append ("  raw ").Append (FormatValue (reader.ReadValueAsBytes ())).AppendLine ();
						break;
					default:
						string text;

						try {
							text = reader.ReadValueAsString ();
						} catch (InvalidOperationException) {
							// Documented: the attribute's value cannot be read as a string. The value has not been
							// claimed, so it can still be read as bytes.
							trace.Append ("  !string ").Append (FormatValue (reader.ReadValueAsBytes ())).AppendLine ();
							break;
						}

						trace.Append ("  string ").Append (FormatValue (text)).AppendLine ();
						break;
					}
				}
			}
		}

		void WalkProperties (TnefPropertyReader properties, bool table, int depth)
		{
			if (table) {
				while (properties.ReadNextRow ()) {
					trace.Append ("  row ").Append (properties.RowCount).Append (' ').Append (properties.PropertyCount).AppendLine ();

					while (properties.ReadNextProperty ())
						WalkProperty (properties, depth);
				}
			} else {
				trace.Append ("  props ").Append (properties.PropertyCount).AppendLine ();

				while (properties.ReadNextProperty ())
					WalkProperty (properties, depth);
			}
		}

		void WalkProperty (TnefPropertyReader properties, int depth)
		{
			AppendProperty (properties);

			if (properties.ValueCount == 0)
				return;

			do {
				if (properties.IsEmbeddedMessage) {
					trace.AppendLine ("    embedded");

					using (var embedded = properties.OpenEmbeddedMessage ())
						Walk (embedded, depth + 1);
				} else if (IsVariableLength (properties.PropertyType)) {
					switch (counter++ % 4) {
					case 0:
						using (var stream = properties.OpenValueStream ()) {
							var hash = Fuzz.Hash (stream, out var length);
							trace.Append ("    raw ").Append (length).Append (' ').Append (hash.ToString ("x16")).AppendLine ();
						}
						break;
					case 1:
						trace.Append ("    bytes ").Append (FormatValue (properties.ReadValueAsBytes ())).AppendLine ();
						break;
					case 2:
						if (properties.PropertyType == TnefPropertyType.Object)
							trace.Append ("    bytes ").Append (FormatValue (properties.ReadValueAsBytes ())).AppendLine ();
						else
							trace.Append ("    string ").Append (FormatValue (properties.ReadValueAsString ())).AppendLine ();
						break;
					default:
						trace.Append ("    value ").Append (FormatValue (properties.ReadValue ())).AppendLine ();
						break;
					}
				} else {
					AppendFixedValue (properties);
				}
			} while (properties.ReadNextValue ());
		}

		public async Task WalkAsync (TnefReader reader, int depth = 0)
		{
			while (await reader.ReadAsync ().ConfigureAwait (false)) {
				AppendAttribute (reader, depth);

				if (ContainsProperties (reader.Tag)) {
					await WalkPropertiesAsync (reader.GetPropertyReader (), reader.Tag == TnefAttributeTag.RecipientTable, depth).ConfigureAwait (false);
				} else if (IsScalar (reader.AttributeType)) {
					int value = await reader.ReadValueAsInt32Async ().ConfigureAwait (false);
					short value16 = await reader.ReadValueAsInt16Async ().ConfigureAwait (false);

					Fuzz.Check (await reader.ReadValueAsInt32Async ().ConfigureAwait (false) == value, $"Re-reading scalar attribute {reader.Tag} returned a different value");
					trace.Append ("  scalar ").Append (value).Append (' ').Append (value16).AppendLine ();
				} else if (reader.AttributeType == TnefAttributeType.Date) {
					var value = await reader.ReadValueAsDateTimeAsync ().ConfigureAwait (false);

					Fuzz.Check (await reader.ReadValueAsDateTimeAsync ().ConfigureAwait (false) == value, $"Re-reading date attribute {reader.Tag} returned a different value");
					trace.Append ("  date ").Append (FormatValue (value)).AppendLine ();
				} else {
					switch (counter++ % 3) {
					case 0:
						using (var stream = reader.OpenValueStream ()) {
							var (hash, length) = await Fuzz.HashAsync (stream).ConfigureAwait (false);
							trace.Append ("  raw ").Append (length).Append (' ').Append (hash.ToString ("x16")).AppendLine ();
						}
						break;
					case 1:
						trace.Append ("  raw ").Append (FormatValue (await reader.ReadValueAsBytesAsync ().ConfigureAwait (false))).AppendLine ();
						break;
					default:
						string text;

						try {
							text = await reader.ReadValueAsStringAsync ().ConfigureAwait (false);
						} catch (InvalidOperationException) {
							trace.Append ("  !string ").Append (FormatValue (await reader.ReadValueAsBytesAsync ().ConfigureAwait (false))).AppendLine ();
							break;
						}

						trace.Append ("  string ").Append (FormatValue (text)).AppendLine ();
						break;
					}
				}
			}
		}

		async Task WalkPropertiesAsync (TnefPropertyReader properties, bool table, int depth)
		{
			if (table) {
				while (await properties.ReadNextRowAsync ().ConfigureAwait (false)) {
					trace.Append ("  row ").Append (properties.RowCount).Append (' ').Append (properties.PropertyCount).AppendLine ();

					while (await properties.ReadNextPropertyAsync ().ConfigureAwait (false))
						await WalkPropertyAsync (properties, depth).ConfigureAwait (false);
				}
			} else {
				trace.Append ("  props ").Append (properties.PropertyCount).AppendLine ();

				while (await properties.ReadNextPropertyAsync ().ConfigureAwait (false))
					await WalkPropertyAsync (properties, depth).ConfigureAwait (false);
			}
		}

		async Task WalkPropertyAsync (TnefPropertyReader properties, int depth)
		{
			AppendProperty (properties);

			if (properties.ValueCount == 0)
				return;

			do {
				if (properties.IsEmbeddedMessage) {
					trace.AppendLine ("    embedded");

					using (var embedded = properties.OpenEmbeddedMessage ())
						await WalkAsync (embedded, depth + 1).ConfigureAwait (false);
				} else if (IsVariableLength (properties.PropertyType)) {
					switch (counter++ % 4) {
					case 0:
						using (var stream = properties.OpenValueStream ()) {
							var (hash, length) = await Fuzz.HashAsync (stream).ConfigureAwait (false);
							trace.Append ("    raw ").Append (length).Append (' ').Append (hash.ToString ("x16")).AppendLine ();
						}
						break;
					case 1:
						trace.Append ("    bytes ").Append (FormatValue (await properties.ReadValueAsBytesAsync ().ConfigureAwait (false))).AppendLine ();
						break;
					case 2:
						if (properties.PropertyType == TnefPropertyType.Object)
							trace.Append ("    bytes ").Append (FormatValue (await properties.ReadValueAsBytesAsync ().ConfigureAwait (false))).AppendLine ();
						else
							trace.Append ("    string ").Append (FormatValue (await properties.ReadValueAsStringAsync ().ConfigureAwait (false))).AppendLine ();
						break;
					default:
						trace.Append ("    value ").Append (FormatValue (await properties.ReadValueAsync ().ConfigureAwait (false))).AppendLine ();
						break;
					}
				} else {
					// Note: The typed coercions of fixed-width values are synchronous by design (the value has
					// already been buffered), so the async walker uses ReadValueAsync and then the same coercions.
					var first = FormatValue (await properties.ReadValueAsync ().ConfigureAwait (false));

					trace.Append ("    value ").Append (first);
					Coerce (() => properties.ReadValueAsBoolean ());
					Coerce (() => properties.ReadValueAsInt16 ());
					Coerce (() => properties.ReadValueAsInt32 ());
					Coerce (() => properties.ReadValueAsInt64 ());
					Coerce (() => properties.ReadValueAsFloat ());
					Coerce (() => properties.ReadValueAsDouble ());
					Coerce (() => properties.ReadValueAsDateTime ());
					Coerce (() => properties.ReadValueAsGuid ());
					trace.AppendLine ();

					var second = FormatValue (properties.ReadValue ());

					Fuzz.Equal (first, second, $"Re-reading fixed-width property {properties.Tag}");
				}
			} while (await properties.ReadNextValueAsync ().ConfigureAwait (false));
		}
	}
}

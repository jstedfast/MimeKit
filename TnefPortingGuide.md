# Porting TNEF code from MimeKit 4.x to MimeKit 5.0

MimeKit 5.0 replaces the `MimeKit.Tnef` API. The reader was rewritten against the published
[MS-OXTNEF], [MS-OXCDATA] and [MS-OXCMAIL] specifications, a `TnefMessage` object model was added, and
`TnefPart.ConvertToMessage ()` was replaced by a spec-driven MIME converter. This guide explains how to
port code written against MimeKit 4.x (4.18.1 and earlier).

The guide is written for people and for automated (AI) refactoring tools. Each section starts with a
mapping table that can be applied mechanically, followed by the semantic changes that need a person
(or an agent) to review them. The [checklist](#porting-checklist) at the end summarizes everything.

## Contents

1. [Choosing an API](#choosing-an-api)
2. [Removed and renamed types](#removed-and-renamed-types)
3. [TnefPart](#tnefpart)
4. [TnefReader](#tnefreader)
5. [TnefPropertyReader](#tnefpropertyreader)
6. [Compliance: TnefComplianceStatus and TnefComplianceMode](#compliance-tnefcompliancestatus-and-tnefcompliancemode)
7. [TnefException](#tnefexception)
8. [Other public types](#other-public-types)
9. [Recipes](#recipes)
10. [Porting checklist](#porting-checklist)

## Choosing an API

MimeKit 5.0 offers three levels of TNEF API. Most 4.x code reached for `TnefReader` only because there
was nothing between it and `ConvertToMessage ()`, and can now use a higher level.

| If your 4.x code... | Use in 5.0 |
|---|---|
| called `TnefPart.ConvertToMessage ()` or `TnefPart.ExtractAttachments ()` | `TnefPart.LoadTnefMessage ()` + `TnefMessage.ConvertToMime ()` |
| walked attributes to collect the subject, recipients, bodies or attachments | `TnefMessage` (`Properties`, `Recipients`, `TextBody`/`HtmlBody`/`RtfBody`, `Attachments`) |
| needed attribute-level detail (offsets, checksums, unknown attributes, streaming) | `TnefReader` + `TnefPropertyReader` |
| needed to produce TNEF (4.x could not) | `TnefWriter` + `TnefPropertyWriter` (see [Write a TNEF stream](#write-a-tnef-stream)) |

`TnefMessage` buffers bodies and attachment content in memory, subject to the limits in `TnefOptions`.
`TnefReader` streams and does not buffer.

## Removed and renamed types

| 4.x | 5.0 |
|---|---|
| `TnefComplianceMode` (`Loose`, `Strict`) | Removed. The reader is always "loose"; see [Compliance](#compliance-tnefcompliancestatus-and-tnefcompliancemode). |
| `TnefComplianceStatus` (flags) | Removed. Use `ITnefComplianceLogger`, `TnefComplianceIssue` and `TnefComplianceViolation`. |
| `TnefPropertyId.InternetCPID` | `TnefPropertyId.InternetCodepage` |
| `TnefPropertyTag.InternetCPID` | `TnefPropertyTag.InternetCodepage` |
| `TnefPropertyTag.Puid` | Removed (obsolete); use `TnefPropertyTag.PuidA` or `TnefPropertyTag.PuidW`. |

New types: `TnefOptions`, `ITnefComplianceLogger`, `TnefComplianceIssue`, `TnefComplianceViolation`,
`TnefMessage`, `TnefAttachment`, `TnefRecipient`, `TnefRecipientType`,
`TnefMessageBody`, `TnefMessageBodyFormat`, `TnefPropertySet`, `TnefProperty`, `TnefPropertySetGuid`,
`TnefConversionOptions`, `TnefConversionResult`, `TnefConversionLoss` and `TnefConversionLossKind`.
`TnefAttributeType` (internal in 4.x) is now public.
`TnefNameId` gained static fields for well-known named properties (for example `TnefNameId.Location`).

## TnefPart

| 4.x | 5.0 |
|---|---|
| `MimeMessage ConvertToMessage ()` | `using var tnef = part.LoadTnefMessage ();` then `using var result = tnef.ConvertToMime ();` and use `result.Message` |
| `IEnumerable<MimeEntity> ExtractAttachments ()` | `result.Message.BodyParts` or `result.Message.Attachments` (see [below](#replacing-extractattachments)), or `TnefMessage.Attachments` |
| — | `Task<TnefMessage> LoadTnefMessageAsync (TnefOptions?, CancellationToken)` |

The same members were removed from and added to `ITnefPart`.

### Ownership and disposal

- `LoadTnefMessage ()` returns a `TnefMessage` that is independent of the `TnefPart` and must be disposed.
- `ConvertToMime ()` returns a `TnefConversionResult`. Disposing it disposes `result.Message`. To keep
  the message, take a reference to `result.Message` and do not dispose the result.
- The converted `MimeMessage` does not depend on the `TnefMessage`, so the `TnefMessage` may be disposed
  as soon as `ConvertToMime ()` returns.

### Conversion behavior changes

`ConvertToMime ()` follows [MS-OXCMAIL]. Differences from 4.x `ConvertToMessage ()` that may affect
callers:

- **PidTagMimeSkeleton.** If the message has a MIME skeleton (common for mail that passed through
  Exchange), the skeleton's MIME structure and headers are used and filled in with the TNEF bodies and
  attachments. 4.x ignored it.
- **Headers.** The Received headers from `PidTagTransportMessageHeaders` come first, then the headers
  derived from MAPI properties, then the headers stored in `PS_INTERNET_HEADERS` named properties.
  No header is fabricated: there is no generated Message-Id and no empty From or Subject header.
- **Dates.** `PT_SYSTIME` values are UTC (see [TnefPropertyReader](#tnefpropertyreader)), so the Date
  header no longer depends on the time zone of the machine that ran the conversion.
- **Structure.** When there is more than one body, the bodies become a `multipart/alternative`
  (`text/plain`, then `text/html`). Which bodies are used is decided by the [MS-OXBBODY] best body
  algorithm (`PidTagNativeBody`, then `PidTagRtfInSync`). When the best body is RTF, both alternatives
  are generated from the compressed RTF with `RtfToText` and `RtfToHtml` ([MS-OXCMAIL] 2.1.3.3.5);
  the `PidTagBody` and `PidTagHtml` values are not used. Otherwise the plain text and HTML bodies are
  used as they are, and the out-of-date RTF body is dropped. A `text/rtf` part is never produced.
  In 4.x, `ConvertToMessage ()` added every body property it found (`text/rtf`, `text/html` and
  `text/plain`) to the `multipart/alternative`, in the order the properties appeared in the TNEF
  stream, without considering which body was authoritative. The inline attachments that the HTML
  body references are grouped with the body in a `multipart/related` ([MS-OXCMAIL] 2.1.3.3.6). The
  other attachments follow in a `multipart/mixed`. A message with no body and one attachment is still
  wrapped in a `multipart/mixed`.
- **Inline attachments.** Inline status follows the [MS-OXCMAIL] 2.1.3.4.1 "best body" rules. When there
  is an HTML body, attachments flagged `afRenderedInBody` that the HTML references by `cid:` or
  `Content-Location` are inline. When the best body is RTF, the attachments are matched, in
  `PidTagRenderingPosition` order, with the RTF's `\objattph` placeholders ([MS-OXRTFEX] 2.2.3.4,
  [MS-OXCMAIL] 2.1.3.4.1.1). Hidden attachments and those with a rendering position of -1 are skipped.
  Attachments whose content is an image a browser can display (PNG, JPEG, GIF, BMP or WebP) are given a
  `Content-Id`, are referenced by an `<img src="cid:...">` in the generated HTML, and are inline. If the
  numbers of placeholders and attachments differ, the images are appended to the end of the HTML body
  instead. Other attachments, including OLE objects, are not inline. In 4.x, OLE attachments were always
  inline. Inline attachments get `Content-Disposition: inline`, so `MimeMessage.Attachments` does not
  list them.
- **RTF attachment placeholders.** By default, the generated plain-text body has nothing at an attachment
  placeholder, and the HTML body has an image or nothing. Set
  `TnefConversionOptions.AttachmentPlaceholderCallback` to return text (for example, the attachment's
  file name) to insert at the placeholder. The text is HTML-encoded in the HTML body, where it is used
  only for attachments that are not displayed as images.
- **OLE objects.** OLE attachments are kept as-is unless `TnefConversionOptions.OleObjectConverter` is
  set. A `TnefOleObjectConverter` can render an OLE object as an image. The image replaces the OLE
  attachment and is named after its display name ([MS-OXCMAIL] 2.1.3.4.4). If the converter returns
  `null` or content that is not an image, the OLE attachment is kept.
- **Embedded messages.** As in 4.x, embedded messages are added as `application/ms-tnef` `TnefPart`s by
  default. Set `TnefConversionOptions.ConvertEmbeddedMessages = true` to get `message/rfc822`
  `MessagePart`s instead.
- **Losses.** Anything that could not be represented in MIME is listed in `result.Losses`
  (`TnefConversionLoss.Kind` and `Description`) instead of being silently dropped.
- **Unsafe content types.** An attachment that claims to be `multipart/*`, `message/*`,
  `application/applefile` or `application/mac-binhex40`, or that has an invalid MIME type, is emitted as
  `application/octet-stream`.

### Replacing ExtractAttachments

4.x `ExtractAttachments ()` returned the body alternatives *and* the attachments as a flat list. To get
the same set of entities:

```csharp
using var tnef = part.LoadTnefMessage ();
using var result = tnef.ConvertToMime ();

// Everything (bodies + attachments), like 4.x ExtractAttachments ():
foreach (var entity in result.Message.BodyParts) {
	// ...
}

// Only the attachments (Content-Disposition: attachment):
foreach (var attachment in result.Message.Attachments) {
	// ...
}
```

The entities belong to `result.Message`. Either finish with them before `result` is disposed, or do not
dispose `result` until you are done with them.

If you only need the raw attachment bytes, skip MIME conversion entirely and use
`TnefMessage.Attachments` (see [Extract attachments](#extract-attachments)).

## TnefReader

`TnefReader` is now `sealed`.

| 4.x | 5.0 | Notes |
|---|---|---|
| `TnefReader (Stream)` | `TnefReader (Stream stream, TnefOptions? options = null, bool leaveOpen = false)` | |
| `TnefReader (Stream, int defaultMessageCodepage, TnefComplianceMode)` | `new TnefReader (stream, new TnefOptions { DefaultCodepage = codepage })` | Compliance mode has no replacement; see [Compliance](#compliance-tnefcompliancestatus-and-tnefcompliancemode). |
| `MaxNestingDepth` | `TnefOptions.MaxNestingDepth` | |
| `bool ReadNextAttribute ()` | `bool Read (CancellationToken = default)` / `ReadAsync` | |
| `AttributeLevel` | `Level` | |
| `AttributeTag` | `Tag` | |
| — | `AttributeType` | Type encoded in the attribute id. |
| `AttributeRawValueLength` | `Length` | |
| `AttributeRawValueStreamOffset` | `StreamOffset + 9` | `StreamOffset` is now the offset of the attribute *header* (1-byte level, 4-byte id, 4-byte length). |
| `StreamOffset` (`int`, current read position) | `StreamOffset` (`long`, start of current attribute) | Meaning changed. Offsets of embedded readers are relative to the outermost stream. |
| `int ReadAttributeRawValue (byte[], int, int)` | `Stream OpenValueStream ()` then `Read` | |
| `TnefPropertyReader` property | `GetPropertyReader ()` | Only valid for `MapiProperties`, `Attachment` and `RecipientTable` attributes; throws `InvalidOperationException` otherwise. |
| `AttachmentKey` (`short`) | `LegacyKey` (`ushort`) | |
| `MessageCodepage` | `Codepage` | An `attOemCodepage` of `0` is now ignored, and an unsupported one keeps `TnefOptions.DefaultCodepage` rather than switching to windows-1252. If the stream has no `attOemCodepage`, the message's `PidTagInternetCodepage` is used from the point where it is read ([MS-OXTNEF] 2.3.3.2). |
| `TnefVersion` | Read the `TnefAttributeTag.TnefVersion` attribute with `ReadValueAsInt32 ()`. | A non-`0x00010000` version is reported as `UnsupportedVersion`. |
| `ComplianceMode` | — | Removed. |
| `ComplianceStatus` | `ComplianceLogger` | See [Compliance](#compliance-tnefcompliancestatus-and-tnefcompliancemode). |
| `ResetComplianceStatus ()` | — | Removed; use a new logger. |
| — | `MaxComplianceIssuesPerViolation` | Caps repeated reports. Set this when reading untrusted input. |
| — | `Depth` | Embedded-message nesting depth (0 for the outermost stream). |
| — | `Options` | The `TnefOptions` the reader was created with. |
| `Close ()` | `Dispose ()` | |
| `Dispose ()` | `Dispose ()` | Disposes the stream unless `leaveOpen` is `true`. |

### Reading non-MAPI attribute values

In 4.x, the values of legacy attributes such as `attSubject` or `attDateSent` were read through
`reader.TnefPropertyReader.ReadValueAs* ()`. In 5.0 they are read directly from the reader:

| 4.x | 5.0 |
|---|---|
| `reader.TnefPropertyReader.ReadValueAsString ()` | `reader.ReadValueAsString ()` |
| `reader.TnefPropertyReader.ReadValueAsInt16 ()` | `reader.ReadValueAsInt16 ()` |
| `reader.TnefPropertyReader.ReadValueAsInt32 ()` | `reader.ReadValueAsInt32 ()` |
| `reader.TnefPropertyReader.ReadValueAsDateTime ()` | `reader.ReadValueAsDateTime ()` |
| `reader.TnefPropertyReader.ReadValueAsBytes ()` | `reader.ReadValueAsBytes ()` |
| `reader.TnefPropertyReader.ReadValue ()` | Choose the typed method from `reader.AttributeType`. |

Each has an `*Async` twin. Strings and byte arrays may only be read once.

### Semantic changes

- **Nothing in the stream causes an exception.** 4.x threw `TnefException` in strict mode and
  `EndOfStreamException`/`TnefException` from several paths in loose mode. 5.0 reports a
  `TnefComplianceIssue` and recovers. A truncated stream simply stops returning attributes.
- **A wrong signature does not throw from `Read ()`.** It reports `InvalidSignature` and returns `false`.
  (`TnefMessage.Load ()` does throw `TnefException` in this case.)
- **Late message attributes.** A message-level attribute after the first attachment-level attribute is
  reported as `MessageAttributeAfterAttachment`.
- **The reader is only valid on its own attribute.** `TnefPropertyReader`s and value streams
  obtained for one attribute throw `InvalidOperationException` once `Read ()` has advanced past it.

## TnefPropertyReader

`TnefPropertyReader` is now `sealed`, and is obtained by calling `TnefReader.GetPropertyReader ()`.

| 4.x | 5.0 | Notes |
|---|---|---|
| `PropertyTag` | `Tag` | |
| — | `PropertyType` | The value type, `Tag.ValueTnefType`. |
| `IsMultiValuedProperty` | `IsMultiValued` | |
| `IsNamedProperty` | `Name.HasValue` (or `Tag.IsNamed`) | |
| `PropertyNameId` (`TnefNameId`) | `Name` (`TnefNameId?`) | `null` for properties that are not named. |
| `IsObjectProperty` | `PropertyType == TnefPropertyType.Object` | |
| `IsEmbeddedMessage` | `IsEmbeddedMessage` | Now detects the IID_IMessage prefix, so it no longer depends on `AttachMethod` having been read first. |
| `ValueCount`, `PropertyCount`, `RowCount` | unchanged | |
| `RawValueLength`, `RawValueStreamOffset` | — | Removed. Use `OpenValueStream ().Length` for variable-length values. |
| `ValueType` | — | Removed. Use `PropertyType`, or `ReadValue ()?.GetType ()`. |
| — | `IsValueConsumed` | Whether a variable-length value has already been read. |
| `bool ReadNextRow ()` | `ReadNextRow (CancellationToken = default)` / `ReadNextRowAsync` | |
| `bool ReadNextProperty ()` | `ReadNextProperty (CancellationToken = default)` / `ReadNextPropertyAsync` | Now positions on the first value; see below. |
| `bool ReadNextValue ()` | `ReadNextValue (CancellationToken = default)` / `ReadNextValueAsync` | Advances to the *second and later* values. |
| `Stream GetRawValueReadStream ()` | `Stream OpenValueStream ()` | |
| `int ReadRawValue (byte[], int, int)` | `OpenValueStream ().Read (...)` | |
| `int ReadTextValue (char[], int, int)` | `ReadValueAsString ()` | Or wrap `OpenValueStream ()` in a `StreamReader`. |
| `TnefReader GetEmbeddedMessageReader ()` | `TnefReader OpenEmbeddedMessage ()` | Shares the parent's options and logger; dispose it when done. |
| `ReadValue ()` | `ReadValue (CancellationToken = default)` / `ReadValueAsync` | `Currency` now returns `decimal`; see below. |
| `ReadValueAsString ()`, `ReadValueAsBytes ()` | unchanged, plus a `CancellationToken` and `*Async` twins | May only be called once per value. |
| `ReadValueAsBoolean/Int16/Int32/Int64/Float/Double/DateTime/Guid ()` | unchanged | May be called any number of times. |
| — | `ReadProperty ()`, `ReadPropertySet ()`, `ReadRowsAsPropertySets ()` (+ `*Async`) | Materialize the current property, the remaining properties, or every row. |

### Semantic changes

- **Cursor position.** After `ReadNextProperty ()` returns `true`, the reader is already positioned on
  the property's first value (if `ValueCount > 0`). Read all values with:

  ```csharp
  do {
  	var value = prop.ReadValue ();
  } while (prop.ReadNextValue ());
  ```

  Code that called `ReadNextValue ()` *before* the first read (`while (prop.ReadNextValue ()) { ... }`)
  skips the first value in 5.0 and must be rewritten as a `do`/`while` loop.
- **Single-use variable-length values.** Strings, binary values and objects can be read once (via
  `ReadValue`, `ReadValueAsString`, `ReadValueAsBytes`, `OpenValueStream` or `OpenEmbeddedMessage`).
  A second read throws `InvalidOperationException`. Fixed-width values can be read repeatedly.
- **Stale readers throw.** Any use after the owning `TnefReader` advances to another attribute throws
  `InvalidOperationException`.
- **`PT_SYSTIME` is UTC.** 4.x used `DateTime.FromFileTime` (local time). 5.0 returns
  `DateTimeKind.Utc` values. Remove any `.ToUniversalTime ()` calls added to compensate.
- **`PT_CURRENCY` is scaled.** [MS-OXCDATA] defines currency as a 64-bit integer scaled by 10,000.
  `ReadValue ()` returns a `decimal` (4.x returned the raw `long`), and the numeric `ReadValueAs* ()`
  methods return the scaled value (4.x `ReadValueAsInt64 ()` returned the raw value).
- **Size limits.** Values larger than `TnefOptions.MaxPropertyValueLength`, or past
  `TnefOptions.MaxTotalDataBytes`, are reported as `DataSizeLimitExceeded` and read as empty.
- **No exceptions for malformed data.** Problems are reported to the reader's `ComplianceLogger` and the
  property reader stops returning properties when the rest of the attribute cannot be interpreted.

## Compliance: TnefComplianceStatus and TnefComplianceMode

4.x accumulated a `TnefComplianceStatus` bit field in loose mode and threw `TnefException` in strict mode.
5.0 reports each problem as a `TnefComplianceIssue` to an `ITnefComplianceLogger`. Each issue carries the
`Violation`, the `StreamOffset`, the embedded-message `Depth`, the `AttributeTag` and `PropertyTag` it was
found in, a `Severity`, a `Description` and `Remarks`.

| 4.x `TnefComplianceStatus` | 5.0 `TnefComplianceViolation` |
|---|---|
| `Compliant` | No issues logged. |
| `AttributeOverflow` | `InvalidAttributeLength` or `TruncatedStream` |
| `InvalidAttribute` | `UnknownAttribute`, `AttributeLevelMismatch` |
| `InvalidAttributeChecksum` | `AttributeChecksumMismatch` |
| `InvalidAttributeLength` | `InvalidAttributeLength` |
| `InvalidAttributeLevel` | `InvalidAttributeLevel`, `MessageAttributeAfterAttachment` |
| `InvalidAttributeValue` | `InvalidAttributeValue`, `InvalidValueCount`, `InvalidNamedPropertyKind` |
| `InvalidDate` | `InvalidDate` |
| `InvalidMessageClass` | `InvalidMessageClass` |
| `InvalidMessageCodepage` | `InvalidMessageCodepage` |
| `InvalidPropertyLength` | `InvalidPropertyLength`, `InvalidPropertyCount` |
| `InvalidRowCount` | `InvalidRowCount` |
| `InvalidTnefSignature` | `InvalidSignature` |
| `InvalidTnefVersion` | `UnsupportedVersion` |
| `NestingTooDeep` | `NestingTooDeep` |
| `StreamTruncated` | `TruncatedStream` |
| `UnsupportedPropertyType` | `UnsupportedPropertyType` |
| — | `TooManyAttachments`, `DataSizeLimitExceeded` (resource limits from `TnefOptions`) |
| — | `TooManyComplianceIssues` (the reader's per-violation cap was reached) |

| 4.x pattern | 5.0 replacement |
|---|---|
| `TnefComplianceMode.Loose` + check `reader.ComplianceStatus` afterwards | Set `reader.ComplianceLogger` to a logger that collects issues, and inspect it afterwards. |
| `TnefComplianceMode.Strict` (throw on first problem) | Set a logger whose `Log` throws, e.g. `throw new TnefException (issue.Violation, issue.Description)`. |
| `reader.ResetComplianceStatus ()` | Clear your logger's state. |
| `status.HasFlag (TnefComplianceStatus.X)` | `issues.Any (i => i.Violation == TnefComplianceViolation.X)` |

When reading untrusted streams, bound the number of issues with `reader.MaxComplianceIssuesPerViolation`
or have your logger stop recording after a limit of its own. To get a logger into `TnefMessage`, create the
reader yourself and call `TnefMessage.Load (TnefReader)` (see [Collect compliance issues](#collect-compliance-issues)).

## TnefException

| 4.x | 5.0 |
|---|---|
| `TnefException (TnefComplianceStatus, string?)` | `TnefException (TnefComplianceViolation, string?)` |
| `TnefException (TnefComplianceStatus, string?, Exception?)` | `TnefException (TnefComplianceViolation, string?, Exception?)` |
| `Error` (`TnefComplianceStatus`) | `Violation` (`TnefComplianceViolation`) |

MimeKit itself only throws `TnefException` from `TnefMessage.Load[Async] ()` and
`TnefPart.LoadTnefMessage[Async] ()`, when the stream does not begin with the TNEF signature.

## Other public types

The following types are unchanged apart from additions: `TnefAttachFlags`, `TnefAttachMethod`,
`TnefAttributeLevel`, `TnefAttributeTag`, `TnefNameId`, `TnefNameIdKind`, `TnefPropertyId` (except
`InternetCPID`), `TnefPropertyTag` (except `InternetCPID` and `Puid`), `TnefPropertyType`,
`RtfCompressedToRtf` and `RtfCompressionMode`.

One behavioural change applies to `RtfCompressedToRtf`. In 4.x, a stream whose compression type was
neither `LZFu` nor `MELA` was passed through as if it were uncompressed. In 5.0, such a stream is
malformed ([MS-OXRTFCP] 2.1.3.1.1), so the filter produces no output. Check `CompressionMode`
to detect it. `ConvertToMime` drops an RTF body like this and reports a
`TnefConversionLossKind.InvalidRtfBody` loss. When a compressed RTF body has a CRC mismatch, it is
still converted, and an `RtfChecksumMismatch` loss is reported.

## Recipes

All recipes assume `using MimeKit; using MimeKit.Tnef;`.

### Convert a TNEF part to a MimeMessage

Before (4.x):

```csharp
var message = tnefPart.ConvertToMessage ();
```

After (5.0):

```csharp
MimeMessage message;

using (var tnef = tnefPart.LoadTnefMessage ()) {
	var result = tnef.ConvertToMime ();

	foreach (var loss in result.Losses)
		Console.WriteLine ("TNEF conversion loss: {0}", loss);

	// Keep the message; do not dispose the result.
	message = result.Message;
}
```

Async:

```csharp
using (var tnef = await tnefPart.LoadTnefMessageAsync (null, cancellationToken)) {
	var result = tnef.ConvertToMime (null, cancellationToken);
	// ...
}
```

`ConvertToMime ()` has no async twin because all of the content is already in memory.

### Extract attachments

Before (4.x):

```csharp
foreach (var attachment in tnefPart.ExtractAttachments ()) {
	if (attachment is MimePart part) {
		using (var output = File.Create (part.FileName))
			part.Content.DecodeTo (output);
	}
}
```

After (5.0), without MIME conversion:

```csharp
using (var tnef = tnefPart.LoadTnefMessage ()) {
	foreach (var attachment in tnef.Attachments) {
		if (!attachment.HasContent || attachment.IsEmbeddedMessage)
			continue;

		// FileName comes from an untrusted source; sanitize it before using it as a path.
		var fileName = Path.GetFileName (attachment.FileName ?? "attachment.dat");

		using (var input = attachment.OpenRead ())
		using (var output = File.Create (fileName))
			input.CopyTo (output);
	}
}
```

Embedded messages are read with `attachment.LoadEmbeddedMessage ()`, which returns another `TnefMessage`.

### Read message properties

Before (4.x), walking attributes:

```csharp
using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
	while (reader.ReadNextAttribute ()) {
		if (reader.AttributeLevel != TnefAttributeLevel.Message || reader.AttributeTag != TnefAttributeTag.MapiProperties)
			continue;

		var prop = reader.TnefPropertyReader;

		while (prop.ReadNextProperty ()) {
			if (prop.PropertyTag.Id == TnefPropertyId.Subject)
				subject = prop.ReadValueAsString ();
		}
	}
}
```

After (5.0), with the object model:

```csharp
using (var tnef = TnefMessage.Load (stream)) {
	var subject = tnef.Subject;
	var sentRepresenting = tnef.Properties.GetString (TnefPropertyTag.SentRepresentingEmailAddressW);
	var submitted = tnef.Properties.GetDateTime (TnefPropertyTag.ClientSubmitTime); // UTC

	if (tnef.Properties.TryGetValue (TnefNameId.Location, out var location))
		Console.WriteLine ("Location: {0}", location.Value);
}
```

`TnefPropertySet` lookups by `TnefPropertyTag` match the property id and accept either string type
(`String8`/`Unicode`).

After (5.0), with the reader:

```csharp
using (var reader = new TnefReader (stream)) {
	while (reader.Read ()) {
		if (reader.Level != TnefAttributeLevel.Message || reader.Tag != TnefAttributeTag.MapiProperties)
			continue;

		var prop = reader.GetPropertyReader ();

		while (prop.ReadNextProperty ()) {
			if (prop.Tag.Id == TnefPropertyId.Subject && prop.ValueCount > 0)
				subject = prop.ReadValueAsString ();
		}
	}
}
```

### Read multi-valued properties

Before (4.x):

```csharp
while (prop.ReadNextProperty ()) {
	if (prop.IsMultiValuedProperty) {
		while (prop.ReadNextValue ())
			values.Add (prop.ReadValue ());
	}
}
```

After (5.0):

```csharp
while (prop.ReadNextProperty ()) {
	if (prop.IsMultiValued && prop.ValueCount > 0) {
		do {
			values.Add (prop.ReadValue ());
		} while (prop.ReadNextValue ());
	}
}
```

Or materialize it: `var property = prop.ReadProperty ();` and then `property.TryGetValues<string> (out var strings)`.

### Read the recipient table

Before (4.x):

```csharp
var prop = reader.TnefPropertyReader;

while (prop.ReadNextRow ()) {
	while (prop.ReadNextProperty ()) {
		switch (prop.PropertyTag.Id) {
		case TnefPropertyId.DisplayName: name = prop.ReadValueAsString (); break;
		case TnefPropertyId.EmailAddress: addr = prop.ReadValueAsString (); break;
		}
	}
}
```

After (5.0), with the object model:

```csharp
foreach (var recipient in tnef.Recipients)
	Console.WriteLine ("{0}: {1} <{2}> ({3})", recipient.RecipientType, recipient.DisplayName, recipient.EmailAddress, recipient.AddressType);
```

After (5.0), with the reader (the loop shape is unchanged; only the member names differ):

```csharp
var prop = reader.GetPropertyReader ();

while (prop.ReadNextRow ()) {
	while (prop.ReadNextProperty ()) {
		switch (prop.Tag.Id) {
		case TnefPropertyId.DisplayName: name = prop.ReadValueAsString (); break;
		case TnefPropertyId.EmailAddress: addr = prop.ReadValueAsString (); break;
		}
	}
}
```

Or `IReadOnlyList<TnefPropertySet> rows = prop.ReadRowsAsPropertySets ();`.

### Read the bodies

Before (4.x), the bodies were found by walking `MapiProperties` and decompressing `RtfCompressed` with
`RtfCompressedToRtf`. After (5.0):

```csharp
if (tnef.HtmlBody != null) {
	var html = tnef.HtmlBody.GetText ();
} else if (tnef.RtfBody != null) {
	using (var rtf = tnef.RtfBody.OpenDecodedRead ()) {
		// decompressed RTF bytes
	}
} else if (tnef.TextBody != null) {
	var text = tnef.TextBody.GetText ();
}
```

### Read embedded messages

Before (4.x):

```csharp
if (prop.IsEmbeddedMessage) {
	using (var embedded = prop.GetEmbeddedMessageReader ()) {
		// ...
	}
}
```

After (5.0), with the reader (the depth limit is `TnefOptions.MaxNestingDepth`):

```csharp
if (prop.IsEmbeddedMessage) {
	using (var embedded = prop.OpenEmbeddedMessage ()) {
		while (embedded.Read ()) {
			// ...
		}
	}
}
```

After (5.0), with the object model:

```csharp
foreach (var attachment in tnef.Attachments) {
	if (attachment.IsEmbeddedMessage) {
		using (var embedded = attachment.LoadEmbeddedMessage ())
			Console.WriteLine ("Embedded: {0}", embedded.Subject);
	}
}
```

### Collect compliance issues

Before (4.x):

```csharp
using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
	while (reader.ReadNextAttribute ()) {
		// ...
	}

	if (reader.ComplianceStatus != TnefComplianceStatus.Compliant)
		Console.WriteLine ("TNEF problems: {0}", reader.ComplianceStatus);
}
```

After (5.0):

```csharp
class TnefIssueCollector : ITnefComplianceLogger
{
	public List<TnefComplianceIssue> Issues { get; } = new List<TnefComplianceIssue> ();

	public void Log (in TnefComplianceIssue issue)
	{
		Issues.Add (issue);
	}
}

var collector = new TnefIssueCollector ();

using (var reader = new TnefReader (stream, leaveOpen: true) { ComplianceLogger = collector, MaxComplianceIssuesPerViolation = 16 }) {
	using (var tnef = TnefMessage.Load (reader)) {
		// ...
	}
}

foreach (var issue in collector.Issues)
	Console.WriteLine (issue); // violation, offset, attribute/property and description
```

### Strict mode

Before (4.x):

```csharp
var reader = new TnefReader (stream, 0, TnefComplianceMode.Strict);
```

After (5.0):

```csharp
class StrictTnefLogger : ITnefComplianceLogger
{
	public void Log (in TnefComplianceIssue issue)
	{
		throw new TnefException (issue.Violation, issue.Description);
	}
}

var reader = new TnefReader (stream) { ComplianceLogger = new StrictTnefLogger () };
```

You can also throw only for some violations, for example only when
`issue.Severity >= MimeComplianceSeverity.Major`.

### Limits for untrusted input

4.x had only `MaxNestingDepth`. 5.0 adds:

```csharp
var options = new TnefOptions {
	DefaultCodepage = 1252,
	MaxNestingDepth = 8,                         // default 32
	MaxPropertyValueLength = 16 * 1024 * 1024,   // default 32 MB
	MaxTotalDataBytes = 64L * 1024 * 1024,       // default 64 MB
	MaxAttachments = 256                         // default 1024
};

using (var tnef = tnefPart.LoadTnefMessage (options)) {
	// ...
}
```

### Write a TNEF stream

4.x could only read TNEF. 5.0 adds `TnefWriter` (attributes) and `TnefPropertyWriter` (MAPI properties).
The writer emits the signature, `attTnefVersion` and `attOemCodepage` itself, computes every length,
count, checksum and padding, and throws `InvalidOperationException` when attributes are written out of
the order required by [MS-OXTNEF] (message attributes, then `attMsgProps`, then for each attachment
`attAttachRenderData` ... `attAttachment`).

```csharp
using (var output = File.Create ("winmail.dat"))
using (var writer = new TnefWriter (output, 1252)) {
	writer.WriteAttribute (TnefAttributeTag.MessageClass, "IPM.Note");

	using (var properties = writer.OpenPropertyWriter (TnefAttributeTag.MapiProperties)) {
		properties.WritePropertyTag (TnefPropertyTag.SubjectW);
		properties.WriteValue ("Quarterly report");

		properties.WritePropertyTag (TnefPropertyTag.RtfCompressed);
		using (var rtf = properties.OpenRtfCompressedStream ())
			rtf.Write (rtfBytes, 0, rtfBytes.Length);

		// Named properties are assigned ids (0x8000 and up) by the writer.
		properties.WritePropertyTag (new TnefNameId (TnefPropertySetGuid.PublicStrings, "Keywords"), TnefPropertyType.Unicode | TnefPropertyType.MultiValued);
		properties.WriteValue ("finance");
		properties.WriteValue ("q3");
	}

	writer.WriteAttribute (TnefAttributeTag.AttachRenderData, renderData);

	using (var properties = writer.OpenPropertyWriter (TnefAttributeTag.Attachment)) {
		properties.WritePropertyTag (TnefPropertyTag.AttachMethod);
		properties.WriteValue ((int) TnefAttachMethod.ByValue);
		properties.WritePropertyTag (TnefPropertyTag.AttachLongFilenameW);
		properties.WriteValue ("report.pdf");
		properties.WritePropertyTag (TnefPropertyTag.AttachDataBin);

		using (var content = properties.OpenValueStream ())
			pdfStream.CopyTo (content);
	}
}
```

Use `TnefPropertyWriter.OpenEmbeddedMessage ()` on an `Object` property (`PidTagAttachDataObject`) to write an
embedded message, and `TnefPropertyWriter.WriteProperty (TnefProperty)` to copy a property from a
`TnefMessage` or `TnefPropertySet` unchanged. `TnefWriter` has `WriteAttributeAsync` and `FlushAsync`;
property values are buffered in memory, so `TnefPropertyWriter` is synchronous.

## Porting checklist

1. Replace `TnefPart.ConvertToMessage ()` with `LoadTnefMessage ()` + `ConvertToMime ()`; dispose the
   `TnefMessage`; decide who owns `result.Message`; consider logging `result.Losses`.
2. Replace `TnefPart.ExtractAttachments ()` with `result.Message.BodyParts`/`Attachments`, or with
   `TnefMessage.Attachments` when MIME conversion is not needed.
3. Replace `new TnefReader (stream, codepage, mode)` with `new TnefReader (stream, new TnefOptions { DefaultCodepage = codepage })`.
4. Rename: `ReadNextAttribute` → `Read`, `AttributeLevel` → `Level`, `AttributeTag` → `Tag`,
   `AttributeRawValueLength` → `Length`, `MessageCodepage` → `Codepage`, `AttachmentKey` → `LegacyKey`,
   `TnefPropertyReader` (property) → `GetPropertyReader ()`, `Close ()` → `Dispose ()`.
5. Replace `ReadAttributeRawValue`/`GetRawValueReadStream`/`ReadRawValue`/`ReadTextValue` with
   `OpenValueStream ()` or the `ReadValueAs* ()` methods.
6. Move legacy attribute reads from `reader.TnefPropertyReader.ReadValueAs* ()` to `reader.ReadValueAs* ()`.
7. Rename: `PropertyTag` → `Tag`, `PropertyNameId` → `Name` (now nullable), `IsNamedProperty` →
   `Name.HasValue`, `IsMultiValuedProperty` → `IsMultiValued`, `GetEmbeddedMessageReader` →
   `OpenEmbeddedMessage`.
8. Rewrite every `while (prop.ReadNextValue ())` value loop as `do { ... } while (prop.ReadNextValue ())`.
9. Make sure each string, binary or object value is read only once.
10. Do not use a `TnefPropertyReader` or value stream after calling `Read ()` again.
11. Remove compensation for local-time `PT_SYSTIME` values; they are now UTC.
12. Review `PT_CURRENCY` handling: values are now scaled by 1/10,000 (`decimal` from `ReadValue ()`).
13. Replace `ComplianceMode`/`ComplianceStatus`/`ResetComplianceStatus` with an `ITnefComplianceLogger`
    and map flags using the [violation table](#compliance-tnefcompliancestatus-and-tnefcompliancemode).
14. Replace `TnefException.Error` with `TnefException.Violation`.
15. Remove `try`/`catch` blocks that existed only to survive malformed TNEF (`EndOfStreamException`,
    `TnefException` from reading); keep the one around `TnefMessage.Load ()` for non-TNEF input.
16. Rename `InternetCPID` → `InternetCodepage`; replace `TnefPropertyTag.Puid` with `PuidA`/`PuidW`.
17. For untrusted input, set `TnefOptions` limits and `MaxComplianceIssuesPerViolation`.
18. Re-run any tests that compared converted MIME output: header order, dates, inline dispositions
    and the absence of generated headers have changed as described in [TnefPart](#tnefpart).

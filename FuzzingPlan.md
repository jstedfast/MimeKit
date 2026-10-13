# Coverage-Guided Fuzzing Plan for MimeKit

This document is a plan for adding coverage-guided fuzzing to MimeKit with
[SharpFuzz](https://github.com/Metalnem/sharpfuzz) and
[libFuzzer](https://llvm.org/docs/LibFuzzer.html). Every fuzz target is meant to be added in a single
pass and share one harness project, one corpus layout and one CI workflow.

The intended reader is a contributor, human or AI agent, who will implement the plan later. It
describes the tools, how the work is laid out, the targets, the invariant each target must check,
and how to turn a crash into a regression test.

## Contents

- [Why coverage-guided fuzzing](#why-coverage-guided-fuzzing)
- [Tooling](#tooling)
- [Repository layout](#repository-layout)
- [Harness design](#harness-design)
- [Targets](#targets)
- [Seed corpora](#seed-corpora)
- [Dictionaries](#dictionaries)
- [Running locally](#running-locally)
- [Continuous integration](#continuous-integration)
- [Triage and regression tests](#triage-and-regression-tests)
- [Implementation checklist](#implementation-checklist)

## Why coverage-guided fuzzing

The unit tests already contain deterministic mutation fuzzers:

- `UnitTests/Tnef/TnefFuzzTests.cs`
- `UnitTests/Tnef/TnefCorpusFuzzTests.cs`
- `UnitTests/Tnef/RtfCompressedToRtfFuzzTests.cs`

These apply fixed mutators (bit flips, truncation, run insertion and so on) with fixed seeds. They
are fast, reproducible, and catch regressions, so **they stay**. However, they cannot find inputs
that need several coordinated changes to reach a deep branch, such as a valid TNEF attribute
checksum around a corrupt MAPI property, or a multipart boundary that only matches after a
particular header fold.

Coverage-guided fuzzing does find such inputs. libFuzzer keeps any mutated input that reaches new
code and builds on it. SharpFuzz instruments the .NET IL so that libFuzzer receives that coverage
feedback.

The main goal is to make the parsers safe for untrusted input in high-volume server use:

- no unexpected exception types
- no hangs
- no unbounded memory use
- no divergence between the sync and async code paths

## Tooling

| Component | Purpose | License |
|---|---|---|
| [`SharpFuzz`](https://www.nuget.org/packages/SharpFuzz) (NuGet, ≥ 2.0) | `Fuzzer.LibFuzzer.Run` entry point used by the harness | MIT |
| [`SharpFuzz.CommandLine`](https://www.nuget.org/packages/SharpFuzz.CommandLine) (dotnet tool) | `sharpfuzz <assembly>.dll` rewrites an assembly in place to add coverage instrumentation | MIT |
| [`libfuzzer-dotnet`](https://github.com/Metalnem/libfuzzer-dotnet) | libFuzzer driver that runs the harness process and exchanges inputs/coverage with it | MIT |
| clang ≥ 14 | Only if building `libfuzzer-dotnet` from source instead of using a release binary | Apache-2.0 WITH LLVM-exception |

All of these are build- and CI-time tools only. Nothing is added to the shipped `MimeKit.Core`,
`MimeKit.Cryptography` or `MimeKit` packages.

Notes:

- **Linux is the primary platform.** Use `libfuzzer-dotnet` (Linux build) on `ubuntu-latest`. A
  Windows build (`libfuzzer-dotnet-windows.exe`) exists and is convenient for reproducing crashes,
  but CI should fuzz on Linux, which is also the deployment platform of the TNEF pipeline.
- **Instrument only MimeKit's assemblies.** Run `sharpfuzz` on `MimeKit.Core.dll` (and
  `MimeKit.Cryptography.dll` for crypto targets). Do not instrument BouncyCastle, the BCL or the
  harness itself. Instrumenting them dilutes the coverage signal and slows execution.
- **Instrumentation invalidates the strong-name signature.** .NET (Core) does not verify strong
  names, so the instrumented assembly still loads. Never ship or test-publish an instrumented
  assembly.
- **Never mix instrumenters.** AltCover (used by `scripts/test.ps1` for code coverage) also
  rewrites assemblies in place. Always instrument a fresh `dotnet publish` output.

## Repository layout

```
Fuzzing/
  MimeKit.Fuzzing.csproj        console app, net10.0, references MimeKit + MimeKit.Cryptography + SharpFuzz
  Program.cs                    selects a target from MIMEKIT_FUZZ_TARGET and calls Fuzzer.LibFuzzer.Run
  Targets/
    IFuzzTarget.cs
    TnefReaderTarget.cs
    TnefMessageTarget.cs
    ...                         one file per target (see "Targets")
  Corpora/<target>/             small, sanitized, committed seed inputs (see "Seed corpora")
  Dictionaries/<target>.dict    libFuzzer dictionaries
scripts/
  fuzz.ps1                      publish → instrument → run one target (local + CI)
  fuzz-seed-corpora.ps1         copy seeds from UnitTests/TestData into a working corpus
.github/workflows/fuzz.yml      scheduled fuzzing job
```

Conventions:

- `Fuzzing/` is **not** added to `MimeKit.sln`. This keeps the `dotnet msbuild MimeKit.sln`, test and
  AOT steps unchanged. Build the harness project directly.
- Use the repository's usual style (tabs, CRLF, Mono-style spacing, MIT license header with the
  `.NET Foundation and Contributors` line) and `LangVersion` 12.
- Give the harness `InternalsVisibleTo` access, as is already done for `UnitTests` and `Benchmarks` in
  `MimeKit/Properties/AssemblyInfo.cs`, signed with the same key, so that internal components such
  as `AddressValidator` can be called directly when that reaches more code than the public API.
  Prefer the public API whenever it reaches the same code.

## Harness design

One process fuzzes one target. libFuzzer drives a single entry point per process, and
`libfuzzer-dotnet` does not forward custom arguments reliably, so the target is chosen with an
environment variable:

```csharp
// Program.cs (sketch)
static void Main ()
{
	var name = Environment.GetEnvironmentVariable ("MIMEKIT_FUZZ_TARGET")
		?? throw new InvalidOperationException ("MIMEKIT_FUZZ_TARGET is not set.");
	var target = FuzzTargets.Get (name);

	Fuzzer.LibFuzzer.Run (span => target.Run (span));
}

interface IFuzzTarget
{
	string Name { get; }

	// Must throw only for genuine bugs; see "Exception policy".
	void Run (ReadOnlySpan<byte> data);
}
```

### Exception policy

A crash is any exception that escapes `Run`, so every target must **catch exactly the exceptions
the API documents for malformed input** and let everything else escape:

| Expected (catch and return) | Always a bug (let escape) |
|---|---|
| `MimeKit.ParseException`, `System.FormatException` from documented `Parse` methods | `NullReferenceException`, `IndexOutOfRangeException`, `InvalidCastException` |
| `MimeKit.Tnef.TnefException` (invalid signature, limits) | `ArgumentException` / `ArgumentOutOfRangeException` thrown from *inside* MimeKit (the harness always passes valid arguments) |
| `System.IO.EndOfStreamException` only where documented | `OverflowException`, `InvalidOperationException` not documented for the call, `KeyNotFoundException` |
| `NotSupportedException` for documented unsupported charsets/encodings | `OutOfMemoryException`, `StackOverflowException` (process dies, so libFuzzer records it) |

Each `catch` must be narrow and commented with the `<exception>` tag that justifies it. If fuzzing
finds a legitimate new failure mode, document it on the public API **before** the harness catches
it.

### Resource limits

- Run with `-timeout=10` (seconds per input) and `-rss_limit_mb=2048`. Set `-max_len` per target (see
  the table below) to keep the search space reasonable.
- Each target sets the library's own limits explicitly, so that libFuzzer's limits detect real
  leaks instead of permitted allocations:
  - `ParserOptions.MaxMimeDepth` and `MaxAddressGroupDepth`
  - `TnefOptions.MaxNestingDepth`, `MaxPropertyValueLength`, `MaxTotalDataBytes`, `MaxAttachments`
  - `MimeReader.MaxComplianceIssuesPerViolation` and `TnefReader.MaxComplianceIssuesPerViolation`
- A slow-unit (`-report_slow_units`) or timeout finding is a bug: an input that is linear in size
  must parse in linear time.

### Invariants: beyond "does not crash"

Every target should check one or more of the following and throw a distinct
`FuzzInvariantException` when one fails. That exception counts as a crash.

1. **Sync/async equivalence.** `MimeReader` / `AsyncMimeReader`, `TnefReader` / its async half, and
   `Load` / `LoadAsync` are hand-maintained mirrors. Run both on the same input, record a canonical
   trace (callbacks, offsets and values, or the serialized result) and require the two traces to be
   equal. This is the most valuable invariant in the plan, because it catches a fix applied to only
   one path.
2. **Logger transparency.** Parsing with an `IMimeComplianceLogger` / `ITnefComplianceLogger`
   attached must produce exactly the same object model and serialization as parsing without one.
3. **Compliance issue sanity.** Every reported issue must have:
   - a defined `MimeComplianceViolation` / `TnefComplianceViolation` value
   - a stream offset in `[0, input.Length]`
   - a line number ≥ 1 and a column ≥ 1, where they apply
   - no more issues per violation than the configured cap
4. **Round-trip stability.** For any input that parses, `write(parse(write(parse(x))))` must equal
   `write(parse(x))` byte for byte (with `FormatOptions.Default` and a fixed `NewLineFormat`). The
   first write may normalize the input, but a second pass must not change it again. For TNEF, re-emitting
   a loaded stream through `TnefWriter`/`TnefPropertyWriter` must load to an equivalent `TnefMessage`,
   and re-emitting that output must reproduce it byte for byte.
5. **Chunking independence.** Filters, encoders and decoders must give the same output whether the
   input arrives in one call or is split at an arbitrary point. Derive the split point from the first
   input byte so that it stays reproducible.
6. **Encode/decode identity.** `Decode (Encode (x)) == x` for the content encoders, and
   `Rfc2047.DecodeText (Rfc2047.EncodeText (x)) == x` for valid Unicode `x`.
7. **Output bounds.** A decoder's output must not exceed its `EstimateOutputLength`. The output of
   `RtfCompressedToRtf` must not exceed the declared raw size plus the documented slack. The output of
   `RtfToText` and of rendering `RtfToHtml` must stay within a constant factor of the input length.
   Measure the constant over the seed corpus, then fix it in the harness with some headroom.
8. **Generated HTML safety.** When `RtfToHtml` *renders* RTF, rather than extracting encapsulated
   HTML, the attacker controls only the text and formatting, never the markup. Tokenize the output with
   `HtmlTokenizer` and require all of the following:
   - Every element is one that `RtfToHtml` emits: `html`, `head`, `body`, `div`, `p`, `span`, `a`,
     `br`, `table`, `tr`, `td`, plus `img` when an `AttachmentPlaceholderCallback` writes one. Derive the
     final list from `RtfToHtml.cs`.
   - The only attributes are `style`, `href` and the callback's `img` attributes.
   - Every `href` uses an allowed scheme: `http`, `https`, `mailto`, `ftp` or `tel` (see
     `RtfInterpreter.IsAllowedScheme`).
   - No `style` value contains `url(`, `expression(`, `@import`, `<` or `"`.
   - Start and end tags are balanced.

   In extract mode, the HTML is the document's own and only invariants 1, 5 and 7 apply.

## Targets

Priority 1 targets are the untrusted-input entry points of a mail transport, and are where the
pipeline's risk lies. Implement all of them in the first pass. Priority 2 can follow in the same PR
if time allows.

| # | Target name | Entry point | Invariants | `-max_len` | Priority |
|---|---|---|---|---|---|
| 1 | `tnef-reader` | `new TnefReader (stream, options)` and walking every attribute, property, row and value with the `ReadValueAs*` coercions; for inputs that load, re-emit through `TnefWriter` | 1, 3, 4 | 1 MiB | 1 |
| 2 | `tnef-message` | `TnefMessage.Load` / `LoadAsync`, including embedded messages, then `ConvertToMime` with `ConvertEmbeddedMessages` on and off, then `MimeMessage.WriteTo` (see the TNEF notes for the RTF body options) | 1, 2, 3, 4 (on the MIME output) | 4 MiB | 1 |
| 3 | `rtf-compressed` | `RtfCompressedToRtf` as an `IMimeFilter` through `FilteredStream`; also compress arbitrary input with `TnefPropertyWriter.OpenRtfCompressedStream` and decompress it again | 5, 6, 7 | 1 MiB | 1 |
| 4 | `mime-reader` | A `MimeReader` subclass that records every `On*` callback, in both `MimeFormat.Entity` and `MimeFormat.Mbox` | 1, 3 | 1 MiB | 1 |
| 5 | `mime-parser` | `MimeParser.ParseMessage` / `ParseMessageAsync`, then `WriteTo` | 1, 2, 4 | 1 MiB | 1 |
| 6 | `address-parser` | `InternetAddressList.TryParse (options, byte[])`, `MailboxAddress.TryParse` and `GroupAddress.TryParse`, under each `RfcComplianceMode` | 4 (via `ToString` → reparse yields the same count and addresses) | 64 KiB | 1 |
| 7 | `compliance-validators` | `MimeReader` with an `IMimeComplianceLogger` attached, over header-heavy inputs. This exercises `AddressValidator` and the header, content-type and transfer-encoding checks, and reaches `Base64Validator`, `QuotedPrintableValidator` and `UUValidator` through part content. It may also call `AddressValidator` directly via `InternalsVisibleTo`. | 2, 3 | 256 KiB | 1 |
| 8 | `rfc2047` | `Rfc2047.DecodeText` / `DecodePhrase` on bytes; `EncodeText` / `EncodePhrase` round-trip on valid UTF-8 | 6 | 64 KiB | 1 |
| 9 | `header-values` | `ContentType.TryParse`, `ContentDisposition.TryParse`, `ParameterList` (RFC 2231 continuations and charsets), `DateUtils.TryParse`, `MimeUtils.ParseMessageId` / `EnumerateReferences`, `Header.Unfold` | 4 (format → reparse) | 64 KiB | 1 |
| 10 | `decoders` | `Base64Decoder`, `QuotedPrintableDecoder`, `UUDecoder`, `YDecoder`, `HexDecoder`; the first byte selects the decoder and the split point | 5, 7 | 1 MiB | 1 |
| 17 | `rtf-converters` | `RtfToText` and `RtfToHtml` (`ExtractEncapsulatedHtml` on and off), through both `Convert (TextReader, TextWriter)` and `Convert (Stream, Stream)` | 5 (whole reader vs. a 1-char-per-`Read` reader), 7, 8 | 1 MiB | 1 |
| 18 | `rtf-tokenizer` | The internal `RtfTokenizer` (`ReadNextToken` vs. `ReadNextTokenAsync`) and `RtfInterpreter` (sync vs. async), via `InternalsVisibleTo` | 1 (token / content-handler trace) | 1 MiB | 1 |
| 11 | `encoders` | `Base64Encoder` (every SIMD code path), `QuotedPrintableEncoder`, `UUEncoder` (every SIMD code path), `YEncoder`, `PassThroughEncoder`, the internal RFC 2047 B / Q encoders and `HexEncoder`; the first byte selects the encoder, the second the line length, and both seed the chunking. The streaming encoders also run through `EncoderFilter` / `DecoderFilter` | 5 (whole vs. random chunks vs. a mid-stream `Clone`), 6, 7 (`EstimateOutputLength` is never exceeded); output is well-formed for its encoding (line lengths, alphabet, padding, escapes); base64 matches `Convert.ToBase64String` and the yEnc checksum matches a reference CRC-32 | 256 KiB | 2 |
| 12 | `html-tokenizer` | `HtmlTokenizer` until EOF; `HtmlToHtml` (with and without a filtering `HtmlTagCallback`) and `HtmlTextPreviewer` | 5; token count ≤ input length + 1; re-tokenizing the `HtmlToHtml` output yields exactly the tags the callback wrote; `HtmlToHtml` is idempotent; differential against AngleSharp (every element a browser creates from the input or the `HtmlToHtml` output must be a start tag that `HtmlTokenizer` reported at the same offset) | 256 KiB | 2 |
| 13 | `text-converters` | `TextToText`, `TextToFlowed`, `FlowedToText`, `FlowedToHtml`, `TextToHtml` (the last one covers `UrlScanner`) | 5, 7; the HTML output uses only generated elements with balanced end tags and honours the callback; links only use known schemes; `FlowedToText` of the `TextToFlowed` output reproduces the input | 256 KiB | 2 |
| 14 | `charset-filter` | `CharsetFilter` across a fixed set of source charsets selected by the first byte | 5 | 256 KiB | 2 |
| 15 | `dkim-arc-headers` | Superseded by targets 19–23 | — | — | — |
| 16 | `legacy-mime-parser` | `LegacyMimeParser` vs `MimeParser` differential | Informational only; divergence is logged, not failed | 1 MiB | 2 |
| 19 | `smtputf8-validator` | `SmtpUtf8AddressValidator.TryValidate` with every combination of options, on the input decoded as UTF-8 and as UTF-16 (to reach lone surrogates) | Options only ever add errors (length limits add only length errors; `AsciiIsLatin` adds only `MixedScripts`) | 4 KiB | 1 |
| 20 | `authentication-results` | `AuthenticationResults.TryParse` / `Parse` | 4 (`ToString` → reparse → `ToString`) | 16 KiB | 1 |
| 21 | `dns-records` | `DkimPublicKeyRecord`, `DmarcRecord.Parse` / `TryParse`, `TagValueList`, DKIM signature tag parsing and `DnsDomainName.TryNormalize` / `TryCombine`, on the input decoded as UTF-8 | Parsing is deterministic; `Parse` and `TryParse` agree; parsed values are well-formed (trimmed, no separators, defined enum values, valid label and name lengths); normalizing is idempotent | 16 KiB | 1 |
| 22 | `dkim-arc-verifier` | `DkimVerifier.Verify` (all signatures) and `ArcVerifier.Verify`, against a fake DNS zone taken from the input | 1 (sync vs. async, including the DNS query trace), Dos vs. Unix `FormatOptions` give the same results, results are self-consistent, `ToAuthenticationMethodResult` round-trips | 64 KiB | 1 |
| 23 | `dmarc-verifier` | `DmarcVerifier.Verify`, against a fake DNS zone and SPF result taken from the input | 1 (sync vs. async, including the DNS query trace), the precomputed-DKIM overload makes the same decision, results are self-consistent, `ToAuthenticationMethodResult` round-trips | 64 KiB | 1 |

Notes per area:

- **TNEF.** Set every `TnefOptions` limit to a small value (for example `MaxNestingDepth = 4` and
  `MaxTotalDataBytes = 16 MiB`), because otherwise libFuzzer can trivially reach the defaults. Use a
  capped compliance logger. For target 1, call both `ReadValue ()` and each typed `ReadValueAs*`
  accessor on fixed-width values. A coercion may throw only the documented `InvalidOperationException`
  for unsupported conversions.
- **TNEF RTF bodies.** Target 2 is the only one that reaches the RTF best-body path of
  `ConvertToMime`. That path covers the `\objattph` placeholder matching, the scan of encapsulated
  `\fromhtml` HTML for `cid:` references, `multipart/related` grouping and the OLE handling. Use
  input bytes to choose the options:
  - Set `TnefConversionOptions.AttachmentPlaceholderCallback` to null, or to a callback that returns
    fixed text.
  - Set `OleObjectConverter` to null, or to a converter that returns a fixed PNG, `null` or non-image
    bytes.

  Add `PidTagRtfCompressed` seeds with `\fromhtml1` and `\objattph`, built with
  `RtfCompressedBuilder` and `TnefBuilder` from `UnitTests`.
- **RTF converters.** Use the first input byte to pick one of two configurations:
  - small limits, so that libFuzzer reaches the folding and limit code quickly (for example
    `MaxGroupDepth = 16`, `MaxFontTableEntries = 8`, `MaxColorTableEntries = 8`)
  - the defaults (4096 each)

  Set `AttachmentPlaceholderCallback` to a callback that writes a fixed `<img>` (HTML) or fixed text.
  For the `Stream` overloads, also exercise `InputEncoding` = Latin-1, and pick some `\ansicpg` /
  `\fcharset` code pages that `CharsetUtils` supports. A slow unit on deeply nested groups, or on a
  large font or color table, is a bug: they must stay linear.
- **MimeReader.** The recording subclass should log `(callback, offset, lineNumber, length)` tuples.
  It must not buffer content without bound; hash content chunks instead.
- **Address parser.** Exercise every `RfcComplianceMode`. `AllowAddressesWithoutDomain`,
  `MaxAddressGroupDepth` and the IDN (`Punycode`) paths are high value.
- **Compliance validators.** These run only when a logger is attached, which is why target 7 exists
  separately from targets 4 and 5. Fuzz with the logger cap set to `1`, to `int.MaxValue` and to the
  default.
- **Cryptography.** S/MIME and OpenPGP parsing is mostly BouncyCastle's code and is out of scope.
  DKIM, ARC and DMARC are MimeKit's own code and are in scope (targets 21–23), so `fuzz.ps1`
  instruments `MimeKit.Cryptography.dll` as well as `MimeKit.Core.dll`. An exception whose first
  non-`System` stack frame is in `Org.BouncyCastle` is tolerated.
- **DKIM / ARC / DMARC input format.** Targets 22 and 23 split the input at the first `%%` line.
  Everything before it is a fake DNS zone (`Fuzzing/Helpers/FuzzDnsZone.cs`), and everything after
  it is the message. Without a `%%` line, the whole input is the message and every lookup is
  NXDOMAIN. Each zone line is `<name> <txt>`, where `<txt>` may instead be `!nxdomain`, `!tempfail`,
  `!empty`, `!throw` (the resolver throws) or `!null` (the resolver returns null). `@spf <status>
  <domain>` sets the SPF result that is passed to `DmarcVerifier`. Lines starting with `#` are
  ignored.

## Seed corpora

libFuzzer works much better from good seeds. Do **not** duplicate the existing test data. Instead,
`scripts/fuzz-seed-corpora.ps1` builds a working corpus per target, outside the repository (for
example under `artifacts/fuzz/<target>/corpus`), from:

| Target(s) | Seeds |
|---|---|
| TNEF, RTF | `UnitTests/TestData/tnef/*.tnef`. For `rtf-compressed`, the `PidTagRtfCompressed` values extracted from those files. |
| MIME reader / parser / validators | `UnitTests/TestData/messages/*`, `UnitTests/TestData/mbox/*` (split large mbox files), `UnitTests/TestData/compliance/*`, `UnitTests/TestData/partial/*` |
| Decoders / encoders | `UnitTests/TestData/encoders/*`, `UnitTests/TestData/yenc/*` |
| HTML / text converters | `UnitTests/TestData/html/*`, `UnitTests/TestData/text/*` |
| RTF converters / tokenizer | `UnitTests/TestData/rtf/**/*.rtf` (third-party, permissively licensed; see its `README.md`), plus the RTF decompressed from the `PidTagRtfCompressed` values in `UnitTests/TestData/tnef/*.tnef` |
| DNS records | The zone files and keys in `UnitTests/TestData/dkim/`, the string literals of the DKIM, ARC and DMARC unit tests, and the `DKIM-Signature` / `ARC-*` header values and `From` domains of the message corpus |
| DKIM / ARC / DMARC verifiers | The messages in `UnitTests/TestData/messages/` and `UnitTests/TestData/dkim/`, signed by `MimeKit.Fuzzing --generate-dkim-seeds` with RSA DKIM, Ed25519 DKIM and RSA DKIM + ARC, each prefixed with a zone that publishes the keys, a DMARC record and an SPF pass, so that the seeds verify; plus the raw `UnitTests/TestData/dkim/*.msg` files |
| SMTPUTF8 validator | The string literals of `SmtpUtf8AddressValidatorTests.cs` that contain `@`, plus the addresses in the message corpus |
| Authentication-Results | The string literals of `AuthenticationResultsTests.cs`, plus the `Authentication-Results` / `ARC-Authentication-Results` values in the message corpus |
| Address / header values / RFC 2047 | Header values from the message corpus, plus the small hand-written files in `Fuzzing/Corpora/<target>/` |

Rules:

- **Exclude very large files** (for example `UnitTests/TestData/messages/80k-participants.eml`) and
  anything above the target's `-max_len`.
- **Hand-written seeds** in `Fuzzing/Corpora/` must be **synthetic**. Never commit real mail, personal
  data, credentials or internal hostnames. Use `example.com` / `example.org` domains.
- **Minimize before committing.** Run `-merge=1` to minimize a grown corpus before committing anything
  back. Commit only small seeds that add coverage. The full grown corpus belongs in CI cache or
  artifacts, not in git.

## Dictionaries

Dictionaries (`Fuzzing/Dictionaries/<target>.dict`, libFuzzer format `"token"`) help the fuzzer
build structured input:

- **tnef:**
  - the signature bytes `"\x78\x9F\x3E\x22"`
  - the level bytes `"\x01"` / `"\x02"`
  - common attribute ids (`attMAPIProps`, `attAttachment`, `attAttachRenderData`, `attAttachData`,
    `attTnefVersion`, `attOemCodepage`), little-endian
  - property type words (`PT_UNICODE`, `PT_BINARY`, `PT_OBJECT`, `PT_MV_*`)
  - the IID_IMessage GUID bytes
  - the PS_MAPI / PS_PUBLIC_STRINGS / PS_INTERNET_HEADERS GUID bytes
- **rtf-compressed:**
  - the `LZFu` / `MELA` magic numbers
  - the MS-OXRTFCP initial dictionary prefix (`{\rtf1\ansi\mac\deff0\deftab720`)
- **rtf:**
  - `"{\\rtf1"`, `"\\ansi"`, `"\\ansicpg"`, `"\\deff0"`, `"\\uc1"`, `"\\u"`, `"\\'"`, `"\\bin"`, `"{\\*\\"`
  - `"{\\fonttbl"`, `"\\fcharset"`, `"{\\colortbl;"`, `"\\red255\\green0\\blue0;"`, `"\\cf1"`, `"\\highlight"`
  - `"\\fromhtml1"`, `"{\\*\\htmltag"`, `"\\htmlrtf"`, `"\\htmlrtf0"`, `"\\fromtext"`
  - `"\\par"`, `"\\pard"`, `"\\plain"`, `"\\b"`, `"\\i"`, `"\\ul"`, `"\\qc"`, `"\\trowd"`, `"\\cell"`, `"\\row"`
  - `"{\\field{\\*\\fldinst HYPERLINK \""`, `"}{\\fldrslt"`, `"javascript:"`
  - `"\\objattph"`, `"{\\object"`, `"{\\pict"`, `"{\\*\\shppict"`, `"\\upr"`, `"{\\*\\ud"`
- **mime:**
  - `"\r\n\r\n"`, `"--"`
  - `"Content-Type: multipart/mixed; boundary="`, `"Content-Transfer-Encoding: "`, plus each
    encoding name
  - `"From "` (mbox), `"charset="`, `"=?utf-8?b?"`, `"=?iso-8859-1?q?"`, `"?="`, `"*0*="`,
    `"''"` (RFC 2231)
  - `"message/rfc822"`, `"message/partial"`, `"application/ms-tnef"`
- **address:** `"<"`, `">"`, `":"`, `";"`, `","`, `"@"`, `"\""`, `"("`, `")"`, `"[IPv6:"`, `"xn--"`, `"=?"`
  (also used by `smtputf8-validator`)
- **authres:** the method, result and property names (`dkim`, `spf`, `dmarc`, `arc`, `pass`,
  `temperror`, `smtp.mailfrom=`, `header.d=`, `reason=`, ...)
- **dkim** (`dns-records`, `dkim-arc-verifier`, `dmarc-verifier`): the fake zone syntax (`%%`,
  `!nxdomain`, `@spf pass `, ...), `_domainkey.` / `_dmarc.`, DKIM key record, DMARC record and
  signature tags (`v=DKIM1`, `k=ed25519`, `p=reject`, `adkim=s`, `c=relaxed/relaxed`, `cv=pass`, ...)
  and common base64 public key prefixes
- **html:** `"<!--"`, `"-->"`, `"<![CDATA["`, `"<script"`, `"&#x"`, `"&amp;"`, `"<meta charset="`,
  plus the doctype, raw-text (`<script>`, `<textarea>`, `<plaintext>`, ...), foreign-content (`<svg>`,
  `<math>`) and frame/template tags that change the tokenizer state, and event-handler attributes
- **text** (`text-converters`): `format=flowed` quote prefixes (`"> "`, `">>"`), soft line breaks
  (`" \n"`), the signature separator (`"-- "`), `From ` stuffing and the URL schemes and prefixes that
  `UrlScanner` detects

## Running locally

`scripts/fuzz.ps1 -Target <name> [-Duration <seconds>] [-Jobs <n>]` should:

1. Run `dotnet publish Fuzzing/MimeKit.Fuzzing.csproj -c Release -r linux-x64 --self-contained -o artifacts/fuzz/bin`.
   On Windows use `-r win-x64`.
2. Run `sharpfuzz artifacts/fuzz/bin/MimeKit.Core.dll`, and the same for `MimeKit.Cryptography.dll`
   when the target needs it.
3. Run `fuzz-seed-corpora.ps1 -Target <name>` if the working corpus does not exist yet.
4. Set `MIMEKIT_FUZZ_TARGET=<name>` and run:
   ```
   libfuzzer-dotnet --target_path=artifacts/fuzz/bin/MimeKit.Fuzzing \
     -dict=Fuzzing/Dictionaries/<name>.dict -max_len=<n> -timeout=10 -rss_limit_mb=2048 \
     -max_total_time=<Duration> -artifact_prefix=artifacts/fuzz/<name>/crashes/ \
     artifacts/fuzz/<name>/corpus
   ```

To reproduce a crash, run the same command with the crash file as the only positional argument.

### Fuzzing everything on a dedicated Linux VM

A long campaign should cover every target in one session, not one target at a time.
`scripts/fuzz.ps1 -Target all -Duration <seconds> [-Parallel <n>]` should:

1. Publish and instrument once, as in steps 1 and 2 above. Instrument both `MimeKit.Core.dll` and
   `MimeKit.Cryptography.dll`, because some targets need the latter.
2. Seed every target's corpus that does not exist yet.
3. Start one `libfuzzer-dotnet` process per target. Run at most `-Parallel` at once (default: the
   number of cores) and queue the rest. Each process gets its own `MIMEKIT_FUZZ_TARGET`, corpus,
   dictionary, `-max_len` and `artifacts/fuzz/<target>/crashes/` directory. Use
   `-max_total_time=<Duration>` for each.
4. Write each target's log to `artifacts/fuzz/<target>/fuzz.log`. At the end, print a summary
   table: target, executions, coverage (`cov:`), corpus size, and crash / timeout / OOM counts. Exit
   non-zero if any target found anything.

Notes:

- **Sizing.** Each process may use up to `-rss_limit_mb` (2 GiB), so size the VM's memory as
  `-Parallel` × 2 GiB plus headroom, or lower `-Parallel`.
- **Long sessions.** Run under `tmux` or `nohup` so that the session survives a disconnect.
- **Dependencies.** Install the .NET 8 SDK, the `SharpFuzz.CommandLine` tool and the Linux
  `libfuzzer-dotnet` binary.
- **Repeated runs.** Keep `artifacts/fuzz/` between runs: the corpora keep growing, and later runs
  start from them. When the campaign ends, run `-merge=1` on each corpus before committing new seeds.

SharpFuzz also ships a `fuzz-libfuzzer.ps1` script (see its
[libFuzzer guide](https://github.com/Metalnem/sharpfuzz/blob/master/docs/libFuzzer.md)). Use it as the
reference for the publish and instrument steps, but MimeKit needs its own script for the per-target
environment variable and options.

## Continuous integration

Add `.github/workflows/fuzz.yml`:

- **Triggers.** A nightly `schedule` and `workflow_dispatch`. Do **not** run on every PR: fuzzing is
  not deterministic, and a PR that fails because of an unrelated pre-existing bug is noise.
- **Runner.** `ubuntu-latest`. Use a matrix over the Priority 1 targets, with
  `-max_total_time=600` (10 minutes) per target. Increase the time once the corpora are stable.
- **Caching.** Cache `artifacts/fuzz/<target>/corpus` with `actions/cache`, keyed by target, so that
  corpora grow from run to run.
- **Crashes.** On a crash or timeout, upload `artifacts/fuzz/<target>/crashes/` as a workflow
  artifact and fail the job. Do not open issues automatically; the maintainer triages them.
- **Separation.** The fuzz job must not reuse the build output of `main.yml`, because instrumentation
  modifies binaries in place. It publishes separately.
- **Possible follow-up.** [ClusterFuzzLite](https://google.github.io/clusterfuzzlite/) or OSS-Fuzz can
  provide longer-running fuzzing later. Check their current .NET support at that time; this plan does
  not depend on them.

## Triage and regression tests

Every confirmed crash, hang or invariant failure becomes a deterministic unit test **in the existing
test suite** before or with the fix. Do not keep it only in the fuzz corpus.

1. Minimize the input with `-minimize_crash=1 -runs=100000`.
2. **Sanitize it.** Crash inputs come from mutating seeds, but you must still confirm that they
   contain no personal data from any non-synthetic seed.
3. Add it as a regression test:
   - **TNEF:** add it as a `MalformedCases` entry in `UnitTests/Tnef/TnefFuzzTests.cs`, built with
     `TnefBuilder` when possible so that the input is readable, or as raw bytes otherwise.
   - **MIME, address, header and encoding inputs:** add a `TestXxx` / `TestXxxAsync` pair to the
     matching fixture (`MimeParserTests`, `MimeReaderTests`, `InternetAddressListTests`,
     `Rfc2047Tests`, `ParameterListTests`, the `UnitTests/Encodings/` fixtures, and so on). Store
     binary inputs under the matching `UnitTests/TestData/<area>/` directory.
   - **RTF inputs:** add a test to `RtfToTextTests`, `RtfToHtmlTests`, `RtfTokenizerTests` or
     `RtfInterpreterTests` in `UnitTests/Text/`. Prefer a short inline RTF string when the crash can be
     reduced to one; otherwise, add the file under `UnitTests/TestData/rtf/mimekit/` and list it in that
     directory's `README.md`.
4. Fix both the sync and async paths. A sync-only fix is a bug, and invariant 1 will find it again.
5. Add a `ReleaseNotes.md` entry for the fix.

## Implementation checklist

- [ ] Create `Fuzzing/MimeKit.Fuzzing.csproj` (net8.0, `SharpFuzz` package, project references to
      `MimeKit` and `MimeKit.Cryptography`). Keep it outside `MimeKit.sln`.
- [ ] Add `InternalsVisibleTo ("MimeKit.Fuzzing, PublicKey=...")` to both libraries'
      `AssemblyInfo.cs`, and sign the harness with `mimekit.snk`.
- [ ] Write `Program.cs`, `IFuzzTarget`, `FuzzInvariantException` and the target registry.
- [ ] Implement Priority 1 targets 1–10, 17 and 18, each with its documented exception allow-list and
      its invariants.
- [ ] Write the shared helpers: a recording `MimeReader`, a canonical TNEF trace writer, a chunked
      filter runner, a counting compliance logger that checks invariant 3, a 1-char-per-`Read`
      `TextReader`, and the RTF HTML-output checker for invariant 8.
- [ ] Write `scripts/fuzz.ps1` (including `-Target all` and `-Parallel`) and `scripts/fuzz-seed-corpora.ps1`.
- [ ] Add the dictionaries and a few synthetic hand-written seeds per target.
- [ ] Smoke-test every target locally: 60 seconds each with no findings, or file the findings.
- [ ] Add `.github/workflows/fuzz.yml` (nightly, matrix, corpus cache, crash artifacts).
- [ ] Link this document from `README.md` (or a future `CONTRIBUTING.md`).
- [ ] Optionally implement Priority 2 targets 11–16.

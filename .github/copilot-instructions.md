# MimeKit — Copilot Instructions

MimeKit is a high-performance C# library for creating and parsing MIME, S/MIME, PGP/MIME, DKIM
and ARC messages. It is a .NET Foundation project. Correctness against the
[relevant IETF RFCs](../RFCs.md) and parser throughput are the two dominant design constraints —
prefer a change that is both spec-correct and allocation-free over one that is merely simpler.

## Build, Test, and Coverage

```powershell
dotnet restore MimeKit.sln
dotnet tool restore                      # installs altcover, coveralls.net, nunit console runner
dotnet msbuild MimeKit.sln -property:Platform="Any CPU" -property:Configuration=Debug
```

On non-Windows also pass `-property:MonoRuntime=true` (this defines `MONO` in `UnitTests` and
selects the `linux-x64`/`osx-x64` SQLite interop native binaries).

Run the full test suite the way CI does:

```powershell
./scripts/test.ps1 -Configuration Debug -GenerateCodeCoverage false
```

`scripts/test.ps1` reads `UnitTests/UnitTests.csproj` to locate the build output, optionally
instruments it with AltCover in-place, then runs `dotnet nunit UnitTests/bin/<Config>/net8.0/UnitTests.dll`.
Because AltCover instruments *in place*, always rebuild before a non-coverage run if you previously
ran with `-GenerateCodeCoverage true`.

Run a single test or fixture (fastest inner loop; builds as a side effect):

```powershell
dotnet test UnitTests\UnitTests.csproj --filter "FullyQualifiedName~MimeParserTests.TestHeaderParser"
dotnet test UnitTests\UnitTests.csproj --filter "FullyQualifiedName~UnitTests.MimeParserTests"

# or, against an already-built test assembly (no rebuild):
dotnet nunit UnitTests\bin\Debug\net8.0\UnitTests.dll --where "class =~ /MimeParserTests/" --noresult
```

The full suite takes several minutes. A couple of hundred skipped tests is normal; they depend
on platform features or external resources.

Before finishing a library change, also build every target framework in Release (with
`-property:Configuration=Release`) to catch API-availability problems on `net4x`/`netstandard2.0`:

```powershell
dotnet msbuild MimeKit\MimeKit.csproj -restore -property:Configuration=Release -v:m
```

AOT compatibility is a hard requirement and is enforced in CI:

```powershell
./scripts/test-aot-compatibility.ps1     # fails if `dotnet publish` emits any `warning IL*`
```

On Windows the native link step needs `vswhere.exe` on `PATH`. If publish fails with
`'vswhere.exe' is not recognized`, prepend `${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer`
to `PATH` — this is an environment problem, not an AOT warning.

There is no separate lint step. Style is enforced by `.editorconfig` and the analyzer sets; the
`NoWarn` lists in the `.csproj` files are deliberate — do not "fix" a suppressed diagnostic by
editing `NoWarn`.

## Project Layout and Assembly Split

| Path | Assembly / PackageId | Notes |
|---|---|---|
| `MimeKit/` | `MimeKit.Core` | The whole MIME object model and parser. No crypto dependencies beyond BouncyCastle. |
| `MimeKit.Cryptography/` | `MimeKit.Cryptography` | S/MIME, PGP/MIME, DKIM, ARC, X.509 certificate databases. References `MimeKit.Core`. |
| `nuget/MimeKit.nuspec` | `MimeKit` | Meta-package pulling in both of the above. |
| `UnitTests/` | — | NUnit 4, `net8.0` only, references both libraries. |
| `AotCompatibility/`, `Benchmarks/` | — | Native-AOT publish probe and BenchmarkDotNet suite. |
| `Fuzzing/` | — | SharpFuzz/libFuzzer harness (`IFuzzTarget` implementations in `Fuzzing/Targets/`), run nightly by `.github/workflows/fuzz.yml`. See `FuzzingPlan.md`. The HTML targets cross-check `HtmlTokenizer` against AngleSharp and confirm disagreements with Chromium (via Playwright). |
| `samples/` | — | Standalone sample apps, not part of `MimeKit.sln`'s test flow. |

Note the asymmetry: the project directory `MimeKit/` produces an assembly named `MimeKit.Core`.
There is no assembly literally named `MimeKit`.

A handful of files are conditionally compiled or excluded via `<Compile Remove/Include>` in the
csproj (e.g. `Utils/EventHandler.cs`, `Utils/OptimizedOrdinalComparer.cs`,
`NpgsqlCertificateDatabase.cs`, `LdapUri.cs`). Adding a file to one of those directories does not
automatically mean it ships on every target framework — check the csproj conditions.

## Target Frameworks and Conditional Compilation

Both libraries multi-target `net462;net47;net48;netstandard2.0;netstandard2.1;net8.0;net10.0`
(`net48` only when building on Windows). Consequences for every change:

- `LangVersion` is pinned to **12**. Do not use newer language features.
- `Nullable` is enabled, but `CS8601;CS8602;CS8604` are suppressed on `net4x`/`netstandard2.0`.
- `ENABLE_SNM` is defined everywhere.
- Modern BCL APIs (spans, `ReadOnlySpan<char>` overloads, `string.Contains(char)`, etc.) are not
  available on all targets. Guard with `#if NET*` or use the shims in `MimeKit/Utils/`.
- `IsAotCompatible` is set for `net7.0`-compatible targets: no reflection-based dynamic code,
  no `BinaryFormatter`, no unbounded generic virtual instantiation in new public APIs.

## Architecture

### The parser is a two-layer design

`MimeReader` (`MimeKit/MimeReader.cs` + `AsyncMimeReader.cs`) is the low-level, allocation-conscious,
push-style scanner. Its public `ReadHeaders`/`ReadEntity`/`ReadMessage` (and `*Async`) methods walk
the byte stream and report structure through `protected virtual On*` callbacks: `OnHeadersBegin`,
`OnHeaderRead`, `OnMimePartContentRead`, `OnMultipartBoundaryRead`, `OnMimeMessageEnd`, and so on.
It never builds an object tree. Every sync callback `OnX` has an `OnXAsync` twin whose default
implementation calls `OnX` and returns `Task.CompletedTask`. The async scanner must always invoke
the `*Async` variant, so that a subclass overriding only the sync callback still sees every event.

`MimeParser` (`MimeKit/MimeParser.cs`) **does not derive from `MimeReader`** — it *composes* one.
A private nested `sealed class Reader : MimeReader` (in `MimeKit/MimeParserReader.cs`, a `partial`
of `MimeParser`) overrides every callback and forwards it to a `protected virtual` method of the
same name and signature on `MimeParser`. `MimeParser` builds the `MimeMessage`/`MimeEntity` graph in
those methods. Subclasses may override them (e.g. to record offsets) but must call `base`. The design
keeps `ReadMessage`/`ReadEntity`/`ReadHeaders` off the parser's public surface: calling them
directly would skip `Initialize`/`Reset` and corrupt the parser's stack. `MimeParser` re-exposes only
`Options`, `IsEndOfStream`, `Position`, `ComplianceLogger` and `ComplianceOptions` as pass-through
properties. Its `Parse*` methods follow the pattern `Initialize` → `reader.ReadXxx` → `Reset ()` on
exception → pop the result off the stack.

When you add a callback to `MimeReader`, you must:

- add both the sync and the async version;
- invoke the async version from `AsyncMimeReader.cs`;
- mirror both as `protected virtual` on `MimeParser`;
- forward both in `MimeParserReader.cs`;
- add the sync version to the `RecordingMimeReader`/`RecordingMimeParser` classes in
  `UnitTests/MimeParserTests.cs`.

`TestMimeParserForwardsAllCallbacks[Async]` asserts that both produce identical event sequences.

`LegacyMimeParser` (`LegacyMimeParser.cs` + `AsyncLegacyMimeParser.cs`) is the public, standalone
pre-5.0 parser that is retained for compatibility. It shares no code with `MimeReader`, so parser
fixes may need to be applied there as well.

When you change parsing behaviour, the change almost always belongs in `MimeReader`, and it must be
made in **both** the sync and async code paths.

### Every async method is a hand-maintained mirror of its sync twin

`MimeReader.cs` / `AsyncMimeReader.cs` and `LegacyMimeParser.cs` / `AsyncLegacyMimeParser.cs` are
`partial class` pairs. The async file duplicates the sync logic line-for-line with `await ... .ConfigureAwait(false)`
substituted in. There is no code generator. **A fix applied only to the sync file is a bug.**
(`MimeParser`'s own `Parse*Async` methods live inline in `MimeParser.cs`.)
The same applies to `OnFooAsync` vs `OnFoo` callback overrides and to the public `WriteTo`/`WriteToAsync`,
`Load`/`LoadAsync`, `ParseMessage`/`ParseMessageAsync` API pairs across the object model. Watch for
subtle drift between the twins (e.g. one passing a different local variable as an argument).

### Object model

`MimeEntity` is the abstract base; `MimePart`, `Multipart` (and `MultipartAlternative`,
`MultipartRelated`, `MultipartReport`, `MultipartSigned`, `MultipartEncrypted`), `MessagePart`,
`TextPart`, `TnefPart` derive from it. `MimeMessage` owns a `HeaderList` plus a root `Body` entity.

Traversal is offered three ways and new visitors should follow the existing pattern:
`MimeVisitor` (double-dispatch via `Accept`), `MimeIterator` (cursor with MIME part-specifier paths),
and plain recursion over `Multipart`.

Almost every concrete public type has a matching `I*` interface (`IMimePart`, `IMultipart`,
`IMimeMessage`, `ITextPart`, ...). When adding a public member to a type that has an interface, add
it to the interface too.

### Options objects thread through everything

`ParserOptions` (input-side: the several `RfcComplianceMode` knobs, `MaxMimeDepth`,
`MaxAddressGroupDepth`, `MaxHeaderLength`, `RespectContentLength`, `CharsetEncoding`) and `FormatOptions`
(output-side: `NewLineFormat`, `HeaderEncoding`,
`ParameterEncodingMethod`, max line length) are passed explicitly into parse/write APIs, with
`ParserOptions.Default` / `FormatOptions.Default` as the implicit fallback. New parsing or
serialization knobs belong on one of these two classes, not as new method parameters.

### Hostile input

Assume all input is attacker-controlled. Anything that can grow with the input (nesting depth,
header length, table sizes, address-group depth) needs a configurable limit with a sane default,
and must fail fast with a `FormatException` instead of after consuming the whole stream. Avoid
recursion that can be driven by input depth and algorithms that are quadratic in input length.

### MIME compliance reporting

`MimeReader.ComplianceLogger` (an `IMimeComplianceLogger`) receives `MimeComplianceIssue`s while
the reader scans. When the logger is `null`, no checks run at all, so keep compliance checks behind
that null check on hot paths. `MimeComplianceOptions` (`MimeReader.ComplianceOptions`) selects
which validators run. Content-transfer-encoding validators (`Base64Validator`,
`QuotedPrintableValidator`, `UUValidator`, implementing `IEncodingValidator`) live in
`MimeKit/Encodings/`. TNEF has its own `ITnefComplianceLogger` family in `MimeKit/Tnef/`.

When adding a `MimeComplianceViolation` value:

- give it a severity and categories in `MimeComplianceIssue.cs`, plus a description/remarks;
- update the value table in `MimeComplianceViolationTests` and the categories table in
  `MimeComplianceIssueTests`;
- **never renumber existing values once MimeKit 5.0 has shipped** — the enum values are public API
  and may be persisted by consumers, so new values must be appended with new numbers. (Before the
  5.0 release, values are grouped by region and inserting one renumbers everything after it.)

### Text converters

`MimeKit/Text/` contains `TextConverter` subclasses (`TextToText`, `TextToHtml`, `TextToFlowed`,
`FlowedToText`, `FlowedToHtml`, `HtmlToHtml`, `RtfToText`, `RtfToHtml`) with their tokenizers
(`HtmlTokenizer`, `RtfTokenizer`). `RtfTokenizer` exposes both sync and async APIs so that an async
`TextConverter.ConvertAsync` can be added later. The RTF converters are designed for untrusted
input:

- `MaxFontTableEntries`, `MaxColorTableEntries` and `MaxGroupDepth` default to 4096;
- nested groups that don't change formatting are folded rather than pushed on a stack.

Keep new converters to the same standard.

### Streams and filters

`MimeKit/IO/` provides the stream primitives the parser and writer are built on: `BoundStream`,
`ChainedStream`, `MeasuringStream`, `MemoryBlockStream`, and `FilteredStream`. Content transforms are
`IMimeFilter` implementations in `MimeKit/IO/Filters/` (charset conversion, `EncoderFilter`/`DecoderFilter`,
CRLF conversion, best-encoding detection). New encoders/decoders go in `MimeKit/Encodings/` as
`IMimeEncoder`/`IMimeDecoder` and are then exposed through `EncoderFilter`/`DecoderFilter`.

`ICancellableStream` exists so that a stream can receive the caller's `CancellationToken` on
otherwise-uncancellable synchronous reads — honour it when writing new stream code.

### Cryptography

`CryptographyContext` is the abstract registry; concrete contexts (`SecureMimeContext` and its
`BouncyCastleSecureMimeContext`, `DefaultSecureMimeContext`, `TemporarySecureMimeContext`,
`WindowsSecureMimeContext`, plus `OpenPgpContext`/`GnuPGContext`) are registered and resolved by
protocol string. `MultipartSigned` / `MultipartEncrypted` call into a context rather than into a
crypto library directly. `DefaultSecureMimeContext` persists X.509 certificates through the
`IX509CertificateDatabase` abstraction (`SqliteCertificateDatabase`, `SQLServerCertificateDatabase`,
`NpgsqlCertificateDatabase`), which is why `UnitTests` depends on SQLite and copies
`SQLite.Interop.dll` in a post-build target.

DKIM and ARC signing/verification live here too and share the `DkimSignerBase`/`DkimVerifierBase`
and body-canonicalization filters (`DkimRelaxedBodyFilter`, `DkimSimpleBodyFilter`). DMARC
evaluation (`DmarcVerifier`, `DmarcRecord`, ...) builds on those results.

`MimeKit.Core` must not reference anything cryptographic, so the split relies on these hooks:

- `CryptographyModule.Initialize ()` registers the factory that makes the parser create
  `MultipartSigned`/`MultipartEncrypted`/`ApplicationPkcs7Mime`/etc. instead of plain
  `Multipart`/`MimePart`. Using `CryptographyContext` triggers it automatically.
- `MimeMessage.Sign`/`Encrypt`/`SignAndEncrypt` are extension methods in `MimeKit.Cryptography`.
- Visitors for cryptographic entities derive from `CryptographicMimeVisitor`, not `MimeVisitor`.
- For AOT compatibility, no S/MIME context is registered by default.

Don't reintroduce reflection-based discovery.

## Conventions

- **Formatting:** tabs for indentation, CRLF line endings, a space between a method name and its
  argument list (`ParseMessage (stream, cancellationToken)`) — this is Mono/GNOME style and applies
  to calls as well as declarations. `.editorconfig` is authoritative.
- **Namespaces** use the brace-on-same-line form: `namespace MimeKit {`.
- **License header:** every source file starts with the standard MIT block including
  `Author: Jeffrey Stedfast <jestedfa@microsoft.com>` and the `.NET Foundation and Contributors`
  copyright line. Copy it verbatim into new files.
- **Argument validation:** use the `MimeKit/Utils/ArgumentValidator.cs` helpers —
  `ArgumentValidator.Validate (options, buffer, startIndex, length)` to throw, or
  `ArgumentValidator.TryValidate (...)` in `TryParse` methods — rather than hand-rolled
  `throw new ArgumentNullException`. Public APIs are expected to validate, and
  `UnitTests/ArgumentExceptionTests.cs` asserts that they do.
- **Performance:** the hot parsing paths deliberately use `unsafe` pointers, `stackalloc`,
  `ValueStringBuilder`, `ByteArrayBuilder`, `PackedByteArray` and `BufferPool` instead of
  `string`/`List<T>`/LINQ. Do not introduce LINQ or intermediate allocations into `MimeReader`,
  `Rfc2047`, `ParseUtils`, the encoders, or the filters. Use `Benchmarks/` to validate, and decide
  optimizations from measurements on representative inputs (e.g. English, non-Latin and binary text),
  not intuition. Example: `Span.IndexOf`/`IndexOfAny` is a big win on long runs but measurably slower
  on short ones. The quoted-printable decoder and validator therefore use a hybrid: a short scalar
  scan first, then switch to `IndexOf` only when the run turns out to be long.
- **XML documentation** is required on public members — `GenerateDocumentationFile` is on and the
  docs are shipped in the NuGet package.

## Testing Conventions

- NUnit 4 with `ImplicitUsings` and a global `using NUnit.Framework;`; fixtures are named
  `<Type>Tests` and live in the directory mirroring the source layout (`UnitTests/Cryptography/`,
  `UnitTests/Encodings/`, `UnitTests/IO/`, `UnitTests/Text/`, `UnitTests/Tnef/`, `UnitTests/Utils/`).
- Test methods are `TestXxx`, and the async mirror is `TestXxxAsync` — when you add a test for a
  sync API that has an async counterpart, add both.
- `Nullable` is **not** enabled in `UnitTests`; don't use `?`/`!` annotations there (CS8632).
- Parser tests commonly take a `[Values (false, true)] bool chunked` parameter and wrap the input
  with `CreateStream (data, chunked)`, so buffer-boundary handling is exercised too.
- Corpus-driven tests read from `UnitTests/TestData/<area>/` (`messages`, `mbox`, `smime`, `dkim`,
  `openpgp`, `tnef`, `html`, `rtf`, `text`, `images`, `encoders`, `yenc`, `partial`, `compliance`). Parser tests typically
  compare against a checked-in `.txt`/`.json` expectation file; when behaviour legitimately changes,
  the expectation files must be regenerated, not the assertion loosened.
- `UnitTests/TestHelper.cs` holds the shared assertion helpers; prefer extending it over duplicating
  comparison logic.
- Every new public API needs an `ArgumentExceptionTests.cs` entry covering its null/range checks.
- When a test is meant to pin a behaviour, sanity-check it by temporarily breaking the code under
  test and confirming the test fails.

## Releasing

Version numbers live in `VersionPrefix` in both `.csproj` files and in the `nuget/*.nuspec` files
(both `<version>` and the inter-package `MimeKit*` dependency versions). `scripts/update-version.ps1`
updates all of them, along with the `.csproj` files, the attributes in both
`Properties/AssemblyInfo.cs` files and the sample projects.

`.github/workflows/main.yml` (CI) builds pre-release packages versioned as `VersionPrefix` plus the
GitHub run number. Official releases are made by manually running `.github/workflows/release.yml` with
the full 40-character commit SHA. It verifies that all of the version numbers agree, builds and packs,
optionally code-signs (the `sign` input), generates build provenance attestations for the packages and
the assemblies within them, pushes MimeKit.Core and MimeKit.Cryptography before the MimeKit
meta-package to NuGet.org, and finally tags the commit. Any step that modifies the packages must run
before the attestation step.

Update `ReleaseNotes.md` for user-visible changes.

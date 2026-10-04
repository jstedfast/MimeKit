# MimeAnalyzer

A console sample that showcases MimeKit's MIME compliance validation API by reporting
violations as compiler-style diagnostics: a quoted snapshot of the offending line with a
caret pointing at the exact column, much like the warnings and errors produced by clang or gcc.

## Building

```
cd samples/MimeAnalyzer
dotnet build
```

The project currently uses a `ProjectReference` to the in-tree `MimeKit` project because the
compliance API has not shipped yet. Once MimeKit 5.0 is released, this can be swapped for a
regular `PackageReference`.

## Usage

```
MimeAnalyzer [options] <file.eml> [<file.eml> ...]
```

Each argument is an individual message file, parsed with `MimeFormat.Default`.

| Option | Description |
| ------ | ----------- |
| `-c`, `--context <transport\|storage>` | How the message is being used, which affects how severely a few violations are rated. Default: `transport`. |
| `-s`, `--severity <minor\|major\|critical>` | Only report issues at or above this severity. Default: `minor`. |
| `--category <list>` | Only report issues in these comma-separated categories: `interoperability`, `dataloss`, `security`. |
| `-r`, `--remarks` | Print the detailed explanation of each issue. |
| `-W`, `--werror` | Report every issue as an error. |
| `--max-issues <n>` | Stop collecting after `n` issues. Default: 1000. |
| `--max-per-violation <n>` | Ask the parser to report each violation at most `n` times. Default: 0, meaning no limit. |
| `--no-caret` | Do not quote the offending source line. |
| `--no-color` | Disable colored output. |
| `-q`, `--quiet` | Only print the per-file summary. |
| `-h`, `--help` | Print the help text. |

Color is also disabled automatically when the `NO_COLOR` environment variable is set or when
output is redirected.

### Exit codes

| Code | Meaning |
| ---- | ------- |
| 0 | No issue was reported as an error. |
| 1 | An issue was reported as an error, or a file could not be read. |
| 2 | The command line could not be parsed. |

## Example output

```
$ MimeAnalyzer addr.eml
addr.eml:2:9: warning: An address had an opening angle bracket without a closing one, or a closing bracket without an opening one. [UnbalancedAngleBracketsInAddress]
2 | To: Bad <first@example.com, second@example.com>
  |         ^
addr.eml:2:47: warning: An address had an opening angle bracket without a closing one, or a closing bracket without an opening one. [UnbalancedAngleBracketsInAddress]
2 | To: Bad <first@example.com, second@example.com>
  |                                               ^

addr.eml: 2 warnings
```

Adding `-r` appends the rationale behind each rule, and the categories it falls under:

```
$ MimeAnalyzer -r raw-koi8r-header.eml
raw-koi8r-header.eml:3:10: note: A MIME part or message header contained 8-bit bytes where only 7-bit bytes were expected. [Unexpected8BitBytesInHeader]
3 | Subject: \xEF\xD4\xCB\xD5\xC4\xC1 \xCF\xCE \xD0\xCF\xD1\xD7\xC9\xCC\xD3\xD1?
  |          ^
  | = categories: Interoperability, DataLoss
  | = Older Internet Message Format specifications require that headers are strictly US-ASCII
  | = while the newer Internationalized Email Headers specification allows for UTF-8. Header
  | = values that are not US-ASCII should be encoded using the encoding mechanism described in
  | = the MIME specification and/or should be valid UTF-8 as allowed in the Internationalized
  | = Email Headers specification.

raw-koi8r-header.eml: 1 note
```

The repository's `UnitTests/TestData/compliance/` directory contains a corpus of deliberately
non-compliant messages that make a good demo. Any number of files may be passed at once:

```
MimeAnalyzer ../../UnitTests/TestData/compliance/incomplete-header.eml \
             ../../UnitTests/TestData/compliance/raw-koi8r-header.eml
```

When more than one file is given, each is analyzed and summarized independently.

## The bundled sample message

Each file in that corpus isolates a single violation. `noncompliant.eml`, next to this README,
is the opposite: one message that packs in as many different violations as a single message can
plausibly hold — roughly sixty issues spanning every category and all three severities.

```
MimeAnalyzer noncompliant.eml
```

It is a `multipart/mixed` whose headers contain a dozen malformed addresses, repeated `Date` and
`Subject` fields, a header field name containing a space, raw koi8-r bytes, a bare linefeed, an
oversized line, and a preamble that is not 7-bit clean. Its child parts then cover the body-side
rules: repeated `Content-Type`/`Content-Transfer-Encoding` headers, 8-bit and null bytes in the
body, broken base64, quoted-printable and uuencoded content, an illegal transfer encoding on a
`message/rfc822` part and on a nested multipart, an unparsable `Content-Type`, a missing and an
unusable `boundary` parameter, a header that is never terminated, and a multipart that is never
closed.

Because it deliberately contains NUL bytes, 8-bit bytes in several charsets, a bare linefeed and
a line over 1000 characters long, it is not a file that survives hand-editing. It is generated
instead by `gen-sample.ps1`:

```
pwsh ./gen-sample.ps1
```

Edit that script rather than the `.eml` when you want to add or change a case.

## How it works

Compliance validation is exposed on `MimeReader`, the low-level scanner that `MimeParser` is
built on top of. Because `MimeReader` only scans the message rather than constructing a
`MimeMessage` object graph, it is a cheap way to lint a message:

```csharp
var collector = new ComplianceCollector (maxIssues);

using (var stream = File.OpenRead (fileName)) {
    var reader = new MimeReader (stream) {
        ComplianceLogger = collector,
        ComplianceOptions = new MimeComplianceOptions {
            Context = context,
            MaxIssuesPerViolation = maxPerViolation
        }
    };

    reader.ReadMessage ();
}
```

`ComplianceCollector` implements `IMimeComplianceLogger`, whose single `Log (in MimeComplianceIssue)`
method is called for each violation found. By default MimeKit places no limit on how many issues it
will report, and the number is bounded only by the size of the message — a message built for the
purpose can produce one issue every few bytes. There are two defenses, and the sample uses both:

* `MimeComplianceOptions.MaxIssuesPerViolation` (`--max-per-violation`) bounds the report at the
  source. The budget is per violation rather than a single total, so that a flood of one cheap
  violation cannot push a more interesting one out of the report. When a budget runs out, a single
  `TooManyComplianceIssues` issue is reported so the report is never silently incomplete.
* The collector's own cap (`--max-issues`), because a logger cannot assume that every caller
  configures the first one.

`ComplianceCollector` does not print `TooManyComplianceIssues` as a diagnostic: it has no source
construct to point at, and counting it as a warning would inflate the summary — and, under
`--werror`, the exit code — for a message that may be fine apart from being verbosely wrong in one
place. It is reported as the `issue limit reached` note instead.

Each `MimeComplianceIssue` carries a `Severity`, a set of `Categories`, a one-based `LineNumber`,
and a one-based `ColumnNumber` (or `0` when the column is unknown, in which case the sample
suppresses the caret rather than misleadingly pointing at column 1).

Severity is mapped onto the familiar diagnostic levels:

| `MimeComplianceSeverity` | Diagnostic | Color |
| ------------------------ | ---------- | ----- |
| `Critical` | `error` | red |
| `Major` | `warning` | yellow |
| `Minor` | `note` | cyan |

### Narrowing an approximate position

Not every violation can be pinned to a single byte while parsing. Some describe an element as a
whole — an unparsable `Content-Type`, a repeated header — and others could only be narrowed
further by re-scanning input the parser has already moved past. Making every parse pay for that
would be a poor trade when most callers never look at the positions.

So each issue also carries a `PositionKind` saying what its position actually refers to:

| `MimeCompliancePositionKind` | Meaning |
| ---------------------------- | ------- |
| `Exact` | the position *is* the offending byte |
| `LineStart` | the violation is somewhere on this line |
| `ElementStart` | the violation is somewhere in this header or body part |

This lets the cost be shifted to the tool, which pays it only for the issues it actually prints.
`SourceText.TryLocateOffendingByte` does exactly that: for the 8-bit and null-byte violations it
scans forward from the reported position for the first byte to blame, then maps that back onto a
line and column.

The difference is easiest to see on a *folded* header, where the parser reports the start of the
header but the bad bytes are on a continuation line:

```
8bit-folded-header.eml:4:2: note: A MIME part or message header contained 8-bit bytes where only 7-bit bytes were expected. [Unexpected8BitBytesInHeader]
4 |         \xEF\xD4\xCB\xD5\xC4\xC1 folded
  |         ^
```

Without narrowing, that diagnostic would point at column 1 of line 3 — at the `Subject:` field
name, a line above the actual problem.

Note that for the 8-bit violations the sample looks for the first **non-ASCII** byte rather than
the byte where UTF-8 validation fails. The two are not the same: in koi8-r text such as
`EF D4 CB`, `0xEF` opens what *looks* like a three-byte UTF-8 sequence, so where validation gives
up depends on how the following bytes happen to combine and the caret can land mid-run. The first
non-ASCII byte is where the charset mistake actually begins.

### Rendering the source line

The message is held in memory as **raw bytes and is never decoded into a string**. This matters
for two reasons: non-compliant messages routinely contain byte sequences that are not valid in
any charset, so decoding would destroy the very evidence being reported; and decoding would
shift the column numbers away from the byte offsets the parser reported.

Instead, the offending line is rendered byte by byte: tabs expand to the next 8-column tab stop,
printable ASCII passes through unchanged, and every other byte is escaped as `\xNN`. Since an
escape occupies four display columns for a single byte, the caret's position has to be computed
*during* rendering rather than taken directly from the reported column.

Very long lines — the compliance corpus includes one over 1000 characters — are elided to a
window centered on the caret, sized from the terminal width.

## License

Released under the MIT license, the same as MimeKit itself.

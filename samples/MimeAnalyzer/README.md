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
raw-koi8r-header.eml:3:1: note: A MIME part or message header contained 8-bit bytes where only 7-bit bytes were expected. [Unexpected8BitBytesInHeader]
3 | Subject: \xEF\xD4\xCB\xD5\xC4\xC1 \xCF\xCE \xD0\xCF\xD1\xD7\xC9\xCC\xD3\xD1?
  | ^
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

## How it works

Compliance validation is exposed on `MimeReader`, the low-level scanner that `MimeParser` is
built on top of. Because `MimeReader` only scans the message rather than constructing a
`MimeMessage` object graph, it is a cheap way to lint a message:

```csharp
var collector = new ComplianceCollector (maxIssues);

using (var stream = File.OpenRead (fileName)) {
    var reader = new MimeReader (stream) {
        ComplianceContext = context,
        ComplianceLogger = collector
    };

    reader.ReadMessage ();
}
```

`ComplianceCollector` implements `IMimeComplianceLogger`, whose single `Log (in MimeComplianceIssue)`
method is called for each violation found. MimeKit places no limit on how many issues it will
report, so a logger that retains them has to impose its own cap — hence the `--max-issues` option.

Each `MimeComplianceIssue` carries a `Severity`, a set of `Categories`, a one-based `LineNumber`,
and a one-based `ColumnNumber` (or `0` when the column is unknown, in which case the sample
suppresses the caret rather than misleadingly pointing at column 1).

Severity is mapped onto the familiar diagnostic levels:

| `MimeComplianceSeverity` | Diagnostic | Color |
| ------------------------ | ---------- | ----- |
| `Critical` | `error` | red |
| `Major` | `warning` | yellow |
| `Minor` | `note` | cyan |

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

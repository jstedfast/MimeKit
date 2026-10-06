# RTF Parser Kit test documents

- **Upstream:** https://github.com/joniles/rtfparserkit
- **Commit:** [`26c80c8700eef4c127a7a12ec4af35f190993b21`](https://github.com/joniles/rtfparserkit/tree/26c80c8700eef4c127a7a12ec4af35f190993b21)
- **Original directory:** `src/test/resources/com/rtfparserkit/parser/standard/data/`, except `testSpecialChars.rtf`, which comes from `src/test/resources/com/rtfparserkit/parser/raw/data/`
- **License:** Apache License 2.0. See [`LICENSE.txt`](LICENSE.txt), copied unmodified from the upstream `licence.txt`. The upstream project has no `NOTICE` file.

The `.rtf` files are unmodified copies. The `.txt` and `.html` files are expected output generated
by MimeKit.

| File | Exercises |
|---|---|
| `testGitHubIssue6.rtf` | Color table, `\cf`/`\chcbpat`, `HYPERLINK` fields, `\info`, `\header`, style sheet and list tables, `\ql`/`\qr`/`\qj`, `\line`, `\tab`, `\uN` |
| `testSpecialChars.rtf` | Typographic symbol control words (`\bullet`, `\emdash`, `\endash`, `\emspace`, `\enspace`, `\qmspace`, quotes), `\uc1`, raw newlines |
| `test874Encoding.rtf` | `\ansicpg874` (Thai), and many `\fcharset` values (161, 162, 163, 177, 178, 186, 222, 238) |
| `test10001Encoding.rtf` | `\mac` and `\ansicpg10001` (Mac Japanese), `\fcharset77`/`\fcharset78` |
| `testKoreanEncoding.rtf` | `\ansicpg949`, `\fcharset129`, `\mmathPr` |
| `test437Encoding.rtf` | `\ansicpg437`, `\footer` |
| `testJapaneseUtf8Encoding.rtf` | `\cpg65001` |
| `testTurkishEncoding.rtf` | `\ansicpg1254` |
| `testMultiByteHex.rtf` | `\ansicpg932` with double-byte characters split across `\'hh` escapes |
| `test950Encoding.rtf` | `\ansicpg950` (Big5), including a lone lead byte |
| `test10007Encoding.rtf` | `\ansicpg10007` (Mac Cyrillic) |
| `testNegativeUnicode.rtf` | Negative `\uN` values (16-bit two's complement) |

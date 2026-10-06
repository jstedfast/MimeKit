# Apache Tika test documents

- **Upstream:** https://github.com/apache/tika
- **Commit:** [`5486c9a2b75bb7354cfa2b66dcfd2a90fc6bb5ff`](https://github.com/apache/tika/tree/5486c9a2b75bb7354cfa2b66dcfd2a90fc6bb5ff)
- **Original directory:** `tika-parsers/tika-parsers-standard/tika-parsers-standard-modules/tika-parser-microsoft-module/src/test/resources/test-documents/`
- **License:** Apache License 2.0. See [`LICENSE.txt`](LICENSE.txt) and [`NOTICE.txt`](NOTICE.txt), both copied unmodified from the upstream repository.

The `.rtf` files are unmodified copies. The `.txt` and `.html` files are expected output generated
by MimeKit.

| File | Exercises |
|---|---|
| `testRTFTIKA_1713.rtf` | `\fromhtml1` encapsulated HTML (`\htmlrtf`, `\*\htmltag`), `\ansicpg65001`, escaped `\{` `\}`, and an extra `}` that closes the root group early, followed by more content |
| `testRTFUnicodeUCNControlWordCharacterDoubling.rtf` | `\uc2` with `\'hh` fallback bytes, Shift-JIS `\fcharset128` |
| `testRTFHyperlinkAndStyles.rtf` | `HYPERLINK` fields, `\fcharset1`, escaped `\\` |
| `testRTFInvalidUnicode.rtf` | Surrogate pairs given as `\uN`, and unpaired surrogates |
| `testRTFWindowsCodepage1250.rtf` | `\ansicpg1250` (Central European) |

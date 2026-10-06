# RtfPipe test documents

- **Upstream:** https://github.com/erdomke/RtfPipe
- **Commit:** [`9851ab97d693037073d567d74346184ae2f3d4b3`](https://github.com/erdomke/RtfPipe/tree/9851ab97d693037073d567d74346184ae2f3d4b3)
- **Original directory:** `RtfPipe.Tests/Files/`
- **License:** MIT, Copyright (c) 2018 Eric Domke. See [`LICENSE.txt`](LICENSE.txt), copied unmodified from the upstream repository.

The `.rtf` files are unmodified copies. The `.txt` and `.html` files are expected output generated
by MimeKit.

| File | Exercises |
|---|---|
| `Test01.rtf` | `\strike`, `\ul`, `\cb`, `\fcharset2` (symbol), `\uc0`, the `\upr`/`\ud` Unicode destination pair |
| `Issue35.rtf` | Bookmarks (`\bkmkstart`/`\bkmkend`), `\revtbl`, hidden text (`\v`), `\striked`, `\uld`, `\uldb` |
| `tabs.rtf` | `\ansicpg10000` (Mac Roman), an out-of-range `\fcharset256` |
| `RtfParserTest_fail_4.rtf` | An empty document |
| `RtfParserTest_fail_3.rtf` | A document with unclosed groups |

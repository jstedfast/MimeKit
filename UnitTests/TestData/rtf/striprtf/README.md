# striprtf test documents

- **Upstream:** https://github.com/joshy/striprtf
- **Commit:** [`4f856ae78d1911fd395601891a729da0a6996ba1`](https://github.com/joshy/striprtf/tree/4f856ae78d1911fd395601891a729da0a6996ba1)
- **Original directory:** `tests/rtf/`
- **License:** BSD 3-Clause, Copyright (c) 2018 Joshy Cyriac. See [`LICENSE.txt`](LICENSE.txt), copied unmodified from the upstream repository.

The `.rtf` files are unmodified copies. The `.txt` and `.html` files are expected output generated
by MimeKit.

| File | Exercises |
|---|---|
| `TX_RTF32_18.0.541.501.rtf` | `\background` and `\shpinst` shape destinations, control space (`\ `) |
| `specialchars.rtf` | Optional hyphen `\-` and non-breaking hyphen `\_` |
| `issue_28.rtf` | `\ansicpg936` (Simplified Chinese), `\fcharset134` |
| `unicode.rtf` | `\uN` values above 32767, `\pgdsctbl`, `\userprops` |
| `issue_20.rtf` | `\ansicpg0` (an invalid code page) |

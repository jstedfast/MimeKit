# Benchmarks

## Latest Results

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.2894)
Intel Core i7-9700 CPU 3.00GHz, 1 CPU, 8 logical and 8 physical cores
  [Host]     : .NET Framework 4.8.1 (4.8.9290.0), X64 RyuJIT VectorSize=256
  DefaultJob : .NET Framework 4.8.1 (4.8.9290.0), X64 RyuJIT VectorSize=256

### MimeMessage

| Method                                          | Mean     | Error     | StdDev    |
|------------------------------------------------ |---------:|----------:|----------:|
| MimeMessage_Prepare_EncodingConstraint_None     | 7.727 us | 0.0484 us | 0.0453 us |
| MimeMessage_Prepare_EncodingConstraint_SevenBit | 6.411 us | 0.0380 us | 0.0317 us |
| MimeMessage_Prepare_EncodingConstraint_EightBit | 6.415 us | 0.0200 us | 0.0177 us |

### MimeParser

| Method                                             | Mean        | Error      | StdDev     |
|--------------------------------------------------- |------------:|-----------:|-----------:|
| MimeParser_StarTrekMessage                         |   123.34 us |   2.215 us |   2.071 us |
| MimeParser_StarTrekMessagePersistent               |   105.51 us |   0.962 us |   0.853 us |
| MimeParser_ContentLengthMbox                       | 1,093.34 us |  15.356 us |  14.364 us |
| MimeParser_ContentLengthMboxPersistent             | 1,001.96 us |   7.074 us |   5.907 us |
| MimeParser_JwzMbox                                 | 8,900.18 us |  74.639 us |  69.817 us |
| MimeParser_JwzMboxPersistent                       | 7,991.92 us | 107.522 us |  95.315 us |
| MimeParser_HeaderStressTest                        |    31.96 us |   0.312 us |   0.277 us |
| ExperimentalMimeParser_StarTrekMessage             |   109.28 us |   0.570 us |   0.533 us |
| ExperimentalMimeParser_StarTrekMessagePersistent   |   107.95 us |   1.304 us |   1.156 us |
| ExperimentalMimeParser_ContentLengthMbox           | 1,019.40 us |   4.562 us |   3.809 us |
| ExperimentalMimeParser_ContentLengthMboxPersistent |   988.76 us |   8.919 us |   7.906 us |
| ExperimentalMimeParser_JwzMbox                     | 8,323.95 us |  59.375 us |  52.634 us |
| ExperimentalMimeParser_JwzMboxPersistent           | 7,830.84 us |  69.701 us |  58.203 us |
| ExperimentalMimeParser_HeaderStressTest            |    25.08 us |   0.235 us |   0.208 us |
| MimeReader_StarTrekMessage                         |    81.76 us |   0.706 us |   0.626 us |
| MimeReader_ContentLengthMbox                       |   707.73 us |   4.011 us |   3.349 us |
| MimeReader_JwzMbox                                 | 6,340.44 us | 121.161 us | 118.996 us |
| MimeReader_HeaderStressTest                        |    15.78 us |   0.062 us |   0.055 us |

### BestEncodingFilter

| Method                  | Mean        | Error     | StdDev    |
|------------------------ |------------:|----------:|----------:|
| BestEncoding_LoremIpsum |    431.8 us |   7.50 us |   6.65 us |
| BestEncoding_GirlJpeg   | 50,578.7 us | 972.22 us | 811.85 us |

### MIME Decoders

| Method                 | Mean       | Error     | StdDev    |
|----------------------- |-----------:|----------:|----------:|
| Base64Decoder          | 423.645 us | 8.2470 us | 8.0997 us |
| QuotedPrintableDecoder |   5.572 us | 0.1097 us | 0.1387 us |
| UUDecoder              | 549.760 us | 3.5494 us | 3.1464 us |

### MIME Encoders

| Method                 | Mean       | Error      | StdDev     |
|----------------------- |-----------:|-----------:|-----------:|
| Base64Encoder          | 211.729 us |  2.4491 us |  2.1711 us |
| HexEncoder             | 693.612 us | 12.6747 us | 14.0879 us |
| QEncoder               |   5.814 us |  0.1051 us |  0.1668 us |
| QuotedPrintableEncoder |   6.530 us |  0.1251 us |  0.2318 us |
| UUEncoder              | 231.718 us |  1.9160 us |  1.6985 us |

### TrailingWhitespaceFilter

| Method                        | Mean     | Error    | StdDev   |
|------------------------------ |---------:|---------:|---------:|
| TrailingWhitespace_LoremIpsum | 18.03 us | 0.126 us | 0.118 us |

### Dos2UnixFilter and Unix2DosFilter

| Method                  | Mean      | Error     | StdDev    |
|------------------------ |----------:|----------:|----------:|
| Dos2Unix_LoremIpsumDos  |  9.612 us | 0.1028 us | 0.0911 us |
| Dos2Unix_LoremIpsumUnix |  9.834 us | 0.1893 us | 0.2775 us |
| Unix2Dos_LoremIpsumDos  | 12.052 us | 0.1954 us | 0.1732 us |
| Unix2Dos_LoremIpsumUnix | 12.036 us | 0.1518 us | 0.1420 us |

### Rfc2047

| Method               | Mean     | Error     | StdDev    |
|--------------------- |---------:|----------:|----------:|
| Rfc2047_DecodeText   | 1.775 us | 0.0347 us | 0.0341 us |
| Rfc2047_DecodePhrase | 1.740 us | 0.0060 us | 0.0053 us |

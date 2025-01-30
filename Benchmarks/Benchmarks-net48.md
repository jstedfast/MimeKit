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

| Method                                             | Mean         | Error      | StdDev       | Median       |
|--------------------------------------------------- |-------------:|-----------:|-------------:|-------------:|
| MimeParser_StarTrekMessage                         |    434.95 us |   5.977 us |     4.991 us |    435.15 us |
| MimeParser_StarTrekMessagePersistent               |    331.47 us |   3.476 us |     3.251 us |    332.54 us |
| MimeParser_ContentLengthMbox                       |  3,084.92 us |  33.199 us |    29.430 us |  3,089.20 us |
| MimeParser_ContentLengthMboxPersistent             |  2,576.39 us |  17.208 us |    16.096 us |  2,577.29 us |
| MimeParser_JwzMbox                                 | 25,505.79 us | 206.305 us |   192.977 us | 25,490.72 us |
| MimeParser_JwzMboxPersistent                       | 20,065.87 us | 160.457 us |   133.989 us | 20,073.63 us |
| MimeParser_HeaderStressTest                        |     66.71 us |   1.302 us |     1.154 us |     66.40 us |
| ExperimentalMimeParser_StarTrekMessage             |    445.25 us |   7.075 us |     6.618 us |    446.03 us |
| ExperimentalMimeParser_StarTrekMessagePersistent   |    336.57 us |   3.028 us |     2.833 us |    335.68 us |
| ExperimentalMimeParser_ContentLengthMbox           |  3,119.02 us |  60.423 us |    56.519 us |  3,098.73 us |
| ExperimentalMimeParser_ContentLengthMboxPersistent |  2,669.13 us |  52.815 us |    92.502 us |  2,628.85 us |
| ExperimentalMimeParser_JwzMbox                     | 28,497.84 us | 635.932 us | 1,875.061 us | 28,324.19 us |
| ExperimentalMimeParser_JwzMboxPersistent           | 20,953.24 us | 414.900 us | 1,025.531 us | 20,628.97 us |
| ExperimentalMimeParser_HeaderStressTest            |     57.82 us |   0.444 us |     0.371 us |     57.82 us |
| MimeReader_StarTrekMessage                         |    271.03 us |   3.367 us |     2.629 us |    271.94 us |
| MimeReader_ContentLengthMbox                       |  1,332.58 us |  15.837 us |    14.814 us |  1,330.20 us |
| MimeReader_JwzMbox                                 | 13,394.55 us | 243.807 us |   250.372 us | 13,310.63 us |
| MimeReader_HeaderStressTest                        |     18.18 us |   0.198 us |     0.176 us |     18.17 us |

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

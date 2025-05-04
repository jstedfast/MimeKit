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

| Method                                             | Mean        | Error     | StdDev    |
|--------------------------------------------------- |------------:|----------:|----------:|
| MimeParser_StarTrekMessage                         |   129.88 us |  2.487 us |  2.765 us |
| MimeParser_StarTrekMessagePersistent               |   114.42 us |  2.191 us |  2.050 us |
| MimeParser_ContentLengthMbox                       | 1,094.80 us |  6.510 us |  5.083 us |
| MimeParser_ContentLengthMboxPersistent             | 1,016.36 us | 15.169 us | 14.189 us |
| MimeParser_JwzMbox                                 | 8,887.55 us | 52.108 us | 43.513 us |
| MimeParser_JwzMboxPersistent                       | 7,958.80 us | 81.472 us | 76.209 us |
| MimeParser_HeaderStressTest                        |    31.98 us |  0.233 us |  0.207 us |
| ExperimentalMimeParser_StarTrekMessage             |   120.56 us |  1.018 us |  0.903 us |
| ExperimentalMimeParser_StarTrekMessagePersistent   |   106.50 us |  0.631 us |  0.527 us |
| ExperimentalMimeParser_ContentLengthMbox           | 1,054.48 us |  5.349 us |  4.466 us |
| ExperimentalMimeParser_ContentLengthMboxPersistent |   957.46 us |  5.222 us |  4.630 us |
| ExperimentalMimeParser_JwzMbox                     | 8,555.51 us | 80.363 us | 71.239 us |
| ExperimentalMimeParser_JwzMboxPersistent           | 7,504.97 us | 60.622 us | 53.740 us |
| ExperimentalMimeParser_HeaderStressTest            |    24.93 us |  0.173 us |  0.144 us |
| MimeReader_StarTrekMessage                         |    86.82 us |  1.015 us |  0.900 us |
| MimeReader_ContentLengthMbox                       |   691.30 us |  6.961 us |  6.171 us |
| MimeReader_JwzMbox                                 | 5,918.21 us | 61.377 us | 51.252 us |
| MimeReader_HeaderStressTest                        |    15.62 us |  0.081 us |  0.063 us |

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

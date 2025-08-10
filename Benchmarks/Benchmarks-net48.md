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
| MimeParser_StarTrekMessage                         |   135.64 us |   1.390 us |   1.161 us |
| MimeParser_StarTrekMessagePersistent               |   123.61 us |   1.596 us |   1.492 us |
| MimeParser_ContentLengthMbox                       | 1,130.15 us |  18.279 us |  35.651 us |
| MimeParser_ContentLengthMboxPersistent             | 1,018.27 us |  12.764 us |  10.658 us |
| MimeParser_JwzMbox                                 | 8,771.53 us |  44.376 us |  39.338 us |
| MimeParser_JwzMboxPersistent                       | 7,974.29 us |  62.088 us |  55.040 us |
| MimeParser_HeaderStressTest                        |    30.62 us |   0.155 us |   0.145 us |
| ExperimentalMimeParser_StarTrekMessage             |   120.82 us |   2.290 us |   2.249 us |
| ExperimentalMimeParser_StarTrekMessagePersistent   |   104.48 us |   0.664 us |   0.622 us |
| ExperimentalMimeParser_ContentLengthMbox           | 1,036.78 us |  14.060 us |  12.464 us |
| ExperimentalMimeParser_ContentLengthMboxPersistent |   979.43 us |   5.135 us |   4.552 us |
| ExperimentalMimeParser_JwzMbox                     | 8,508.28 us |  58.749 us |  49.058 us |
| ExperimentalMimeParser_JwzMboxPersistent           | 7,491.65 us | 130.215 us | 121.803 us |
| ExperimentalMimeParser_HeaderStressTest            |    24.53 us |   0.200 us |   0.167 us |
| MimeReader_StarTrekMessage                         |    85.77 us |   1.237 us |   1.033 us |
| MimeReader_ContentLengthMbox                       |   679.32 us |   4.484 us |   3.975 us |
| MimeReader_JwzMbox                                 | 5,882.19 us |  89.402 us |  83.627 us |
| MimeReader_HeaderStressTest                        |    15.70 us |   0.293 us |   0.314 us |

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

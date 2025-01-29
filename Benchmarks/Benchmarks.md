# Benchmarks

## Latest Results

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.2894)
Intel Core i7-9700 CPU 3.00GHz, 1 CPU, 8 logical and 8 physical cores
.NET SDK 9.0.200-preview.0.25057.12
  [Host]     : .NET 8.0.12 (8.0.1224.60305), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.12 (8.0.1224.60305), X64 RyuJIT AVX2

### MimeMessage

| Method                                          | Mean     | Error     | StdDev    |
|------------------------------------------------ |---------:|----------:|----------:|
| MimeMessage_Prepare_EncodingConstraint_None     | 4.572 us | 0.0894 us | 0.1029 us |
| MimeMessage_Prepare_EncodingConstraint_SevenBit | 4.628 us | 0.0910 us | 0.1617 us |
| MimeMessage_Prepare_EncodingConstraint_EightBit | 5.052 us | 0.0850 us | 0.0754 us |

### MimeParser

| Method                                             | Mean          | Error       | StdDev      |
|--------------------------------------------------- |--------------:|------------:|------------:|
| MimeParser_StarTrekMessage                         |    400.462 us |   5.7279 us |   7.4479 us |
| MimeParser_StarTrekMessagePersistent               |    284.192 us |   2.8357 us |   2.3679 us |
| MimeParser_ContentLengthMbox                       |  2,702.926 us |  41.4907 us |  38.8104 us |
| MimeParser_ContentLengthMboxPersistent             |  2,132.559 us |  24.8048 us |  21.9888 us |
| MimeParser_JwzMbox                                 | 22,628.980 us | 252.5825 us | 223.9078 us |
| MimeParser_JwzMboxPersistent                       | 17,000.140 us | 141.0316 us | 131.9211 us |
| MimeParser_HeaderStressTest                        |     51.124 us |   0.7945 us |   0.7043 us |
| ExperimentalMimeParser_StarTrekMessage             |    383.670 us |   7.0703 us |   6.9440 us |
| ExperimentalMimeParser_StarTrekMessagePersistent   |    281.025 us |   4.0180 us |   3.5618 us |
| ExperimentalMimeParser_ContentLengthMbox           |  2,595.748 us |  27.7057 us |  24.5604 us |
| ExperimentalMimeParser_ContentLengthMboxPersistent |  2,098.446 us |  30.5942 us |  28.6179 us |
| ExperimentalMimeParser_JwzMbox                     | 22,666.019 us | 434.5746 us | 533.6967 us |
| ExperimentalMimeParser_JwzMboxPersistent           | 16,821.016 us | 327.6765 us | 490.4508 us |
| ExperimentalMimeParser_HeaderStressTest            |     41.746 us |   0.5287 us |   0.4415 us |
| MimeReader_StarTrekMessage                         |    221.958 us |   4.1713 us |   4.6364 us |
| MimeReader_ContentLengthMbox                       |    992.352 us |  17.0429 us |  15.9419 us |
| MimeReader_JwzMbox                                 | 10,846.372 us | 214.6453 us | 387.0498 us |
| MimeReader_HeaderStressTest                        |      9.911 us |   0.0660 us |   0.0585 us |

### BestEncodingFilter

| Method                  | Mean        | Error     | StdDev    |
|------------------------ |------------:|----------:|----------:|
| BestEncoding_LoremIpsum |    382.0 us |   1.71 us |   1.60 us |
| BestEncoding_GirlJpeg   | 50,750.1 us | 667.52 us | 591.74 us |

### MIME Decoders

| Method                 | Mean         | Error       | StdDev      |
|----------------------- |-------------:|------------:|------------:|
| Base64Decoder          | 362,045.2 ns | 5,364.75 ns | 5,018.19 ns |
| QuotedPrintableDecoder |     665.2 ns |    13.12 ns |    19.64 ns |
| UUDecoder              | 478,663.6 ns | 7,484.50 ns | 6,249.90 ns |

### MIME Encoders

| Method                 | Mean         | Error       | StdDev      |
|----------------------- |-------------:|------------:|------------:|
| Base64Encoder          | 189,400.6 ns | 3,760.16 ns | 6,486.09 ns |
| HexEncoder             | 585,685.4 ns | 8,224.89 ns | 7,291.15 ns |
| QEncoder               |     700.2 ns |    13.59 ns |    13.35 ns |
| QuotedPrintableEncoder |     946.1 ns |    13.60 ns |    11.36 ns |
| UUEncoder              | 239,259.4 ns | 4,695.51 ns | 6,882.61 ns |

### TrailingWhitespaceFilter

| Method                        | Mean     | Error     | StdDev    |
|------------------------------ |---------:|----------:|----------:|
| TrailingWhitespace_LoremIpsum | 5.200 us | 0.0809 us | 0.0757 us |

### Dos2UnixFilter and Unix2DosFilter

| Method                  | Mean     | Error     | StdDev    |
|------------------------ |---------:|----------:|----------:|
| Dos2Unix_LoremIpsumDos  | 3.680 us | 0.0548 us | 0.0486 us |
| Dos2Unix_LoremIpsumUnix | 3.560 us | 0.0330 us | 0.0308 us |
| Unix2Dos_LoremIpsumDos  | 4.668 us | 0.0765 us | 0.0678 us |
| Unix2Dos_LoremIpsumUnix | 4.872 us | 0.0892 us | 0.1539 us |

### Rfc2047

| Method               | Mean     | Error    | StdDev   |
|--------------------- |---------:|---------:|---------:|
| Rfc2047_DecodeText   | 914.6 ns | 18.19 ns | 30.40 ns |
| Rfc2047_DecodePhrase | 831.7 ns |  5.32 ns |  4.15 ns |

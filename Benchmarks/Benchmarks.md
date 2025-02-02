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
| MimeParser_StarTrekMessage                         |    269.321 us |   5.3183 us |   8.4355 us |
| MimeParser_StarTrekMessagePersistent               |    246.636 us |   4.7926 us |   5.5192 us |
| MimeParser_ContentLengthMbox                       |  1,377.439 us |  17.4600 us |  15.4778 us |
| MimeParser_ContentLengthMboxPersistent             |  1,249.829 us |  12.7459 us |  11.9225 us |
| MimeParser_JwzMbox                                 | 14,267.191 us | 274.8707 us | 269.9597 us |
| MimeParser_JwzMboxPersistent                       | 12,674.922 us | 113.3533 us |  94.6551 us |
| MimeParser_HeaderStressTest                        |     21.527 us |   0.1436 us |   0.1121 us |
| ExperimentalMimeParser_StarTrekMessage             |    253.124 us |   4.2137 us |   3.9415 us |
| ExperimentalMimeParser_StarTrekMessagePersistent   |    239.576 us |   3.6207 us |   3.3868 us |
| ExperimentalMimeParser_ContentLengthMbox           |  1,325.283 us |   9.0779 us |   8.4914 us |
| ExperimentalMimeParser_ContentLengthMboxPersistent |  1,176.737 us |  22.4083 us |  29.1371 us |
| ExperimentalMimeParser_JwzMbox                     | 13,602.926 us | 235.3053 us | 220.1048 us |
| ExperimentalMimeParser_JwzMboxPersistent           | 12,184.423 us |  96.3543 us |  90.1299 us |
| ExperimentalMimeParser_HeaderStressTest            |     13.083 us |   0.0830 us |   0.0735 us |
| MimeReader_StarTrekMessage                         |    223.907 us |   3.1057 us |   2.9051 us |
| MimeReader_ContentLengthMbox                       |    980.042 us |  12.7192 us |  10.6211 us |
| MimeReader_JwzMbox                                 | 11,136.306 us | 204.2014 us | 170.5174 us |
| MimeReader_HeaderStressTest                        |      8.395 us |   0.0931 us |   0.0871 us |

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

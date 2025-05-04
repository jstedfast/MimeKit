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

| Method                                             | Mean         | Error       | StdDev      |
|--------------------------------------------------- |-------------:|------------:|------------:|
| MimeParser_StarTrekMessage                         |   107.756 us |   2.1442 us |   5.9058 us |
| MimeParser_StarTrekMessagePersistent               |    83.645 us |   0.7263 us |   0.6439 us |
| MimeParser_ContentLengthMbox                       |   797.602 us |   7.2364 us |   6.4148 us |
| MimeParser_ContentLengthMboxPersistent             |   715.624 us |   5.7660 us |   5.1115 us |
| MimeParser_JwzMbox                                 | 6,917.998 us |  70.9975 us |  66.4111 us |
| MimeParser_JwzMboxPersistent                       | 6,196.110 us | 123.8234 us | 161.0054 us |
| MimeParser_HeaderStressTest                        |    20.632 us |   0.2864 us |   0.2539 us |
| ExperimentalMimeParser_StarTrekMessage             |    99.692 us |   0.5002 us |   0.4177 us |
| ExperimentalMimeParser_StarTrekMessagePersistent   |    90.032 us |   1.4249 us |   1.3329 us |
| ExperimentalMimeParser_ContentLengthMbox           |   787.329 us |   8.0334 us |   7.1214 us |
| ExperimentalMimeParser_ContentLengthMboxPersistent |   688.084 us |   5.1357 us |   4.5527 us |
| ExperimentalMimeParser_JwzMbox                     | 6,907.650 us |  69.6696 us |  65.1690 us |
| ExperimentalMimeParser_JwzMboxPersistent           | 6,434.393 us | 126.1687 us | 192.6729 us |
| ExperimentalMimeParser_HeaderStressTest            |    13.071 us |   0.2190 us |   0.1941 us |
| MimeReader_StarTrekMessage                         |    79.039 us |   0.6652 us |   0.5193 us |
| MimeReader_ContentLengthMbox                       |   506.600 us |   2.4274 us |   2.0270 us |
| MimeReader_JwzMbox                                 | 4,986.448 us |  30.5282 us |  27.0625 us |
| MimeReader_HeaderStressTest                        |     8.118 us |   0.0573 us |   0.0478 us |

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

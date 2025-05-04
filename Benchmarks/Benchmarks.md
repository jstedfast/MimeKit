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

| Method                                             | Mean         | Error      | StdDev     |
|--------------------------------------------------- |-------------:|-----------:|-----------:|
| MimeParser_StarTrekMessage                         |    90.435 us |  0.7455 us |  0.6973 us |
| MimeParser_StarTrekMessagePersistent               |    81.716 us |  0.5504 us |  0.4596 us |
| MimeParser_ContentLengthMbox                       |   773.167 us |  9.6001 us |  8.0165 us |
| MimeParser_ContentLengthMboxPersistent             |   712.220 us |  5.6727 us |  4.7370 us |
| MimeParser_JwzMbox                                 | 6,837.832 us | 67.6953 us | 60.0101 us |
| MimeParser_JwzMboxPersistent                       | 6,115.685 us | 31.6801 us | 24.7338 us |
| MimeParser_HeaderStressTest                        |    19.415 us |  0.2653 us |  0.2352 us |
| ExperimentalMimeParser_StarTrekMessage             |    69.727 us |  0.5362 us |  0.4753 us |
| ExperimentalMimeParser_StarTrekMessagePersistent   |    58.905 us |  0.6591 us |  0.5503 us |
| ExperimentalMimeParser_ContentLengthMbox           |   608.324 us |  8.1063 us |  7.5826 us |
| ExperimentalMimeParser_ContentLengthMboxPersistent |   524.317 us |  7.7340 us |  6.4582 us |
| ExperimentalMimeParser_JwzMbox                     | 5,265.384 us | 56.6687 us | 50.2353 us |
| ExperimentalMimeParser_JwzMboxPersistent           | 4,474.884 us | 89.2177 us | 87.6238 us |
| ExperimentalMimeParser_HeaderStressTest            |    11.653 us |  0.1319 us |  0.1169 us |
| MimeReader_StarTrekMessage                         |    48.659 us |  0.2957 us |  0.2469 us |
| MimeReader_ContentLengthMbox                       |   353.706 us |  6.8101 us |  6.3702 us |
| MimeReader_JwzMbox                                 | 3,398.939 us | 32.1085 us | 28.4634 us |
| MimeReader_HeaderStressTest                        |     6.625 us |  0.0616 us |  0.0481 us |

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

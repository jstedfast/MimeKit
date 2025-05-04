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

| Method                                             | Mean         | Error      | StdDev      |
|--------------------------------------------------- |-------------:|-----------:|------------:|
| MimeParser_StarTrekMessage                         |    71.731 us |  0.3290 us |   0.2747 us |
| MimeParser_StarTrekMessagePersistent               |    60.809 us |  0.8136 us |   0.7213 us |
| MimeParser_ContentLengthMbox                       |   621.492 us |  3.7083 us |   2.8952 us |
| MimeParser_ContentLengthMboxPersistent             |   551.683 us | 10.7364 us |  10.5446 us |
| MimeParser_JwzMbox                                 | 5,238.200 us | 67.6061 us |  63.2388 us |
| MimeParser_JwzMboxPersistent                       | 4,612.795 us | 91.5963 us | 119.1010 us |
| MimeParser_HeaderStressTest                        |    15.356 us |  0.0761 us |   0.0635 us |

| ExperimentalMimeParser_StarTrekMessage             |    70.105 us |  1.1094 us |   0.9834 us |
| ExperimentalMimeParser_StarTrekMessagePersistent   |    57.515 us |  0.6193 us |   0.5490 us |
| ExperimentalMimeParser_ContentLengthMbox           |   622.374 us | 11.9024 us |  14.6173 us |
| ExperimentalMimeParser_ContentLengthMboxPersistent |   534.965 us |  4.6371 us |   4.1107 us |
| ExperimentalMimeParser_JwzMbox                     | 5,030.161 us | 63.8917 us |  56.6384 us |
| ExperimentalMimeParser_JwzMboxPersistent           | 4,557.651 us | 40.6831 us |  33.9722 us |
| ExperimentalMimeParser_HeaderStressTest            |    12.365 us |  0.2359 us |   0.2091 us |
| MimeReader_StarTrekMessage                         |    47.750 us |  0.2926 us |   0.2444 us |
| MimeReader_ContentLengthMbox                       |   353.960 us |  2.9889 us |   2.7958 us |
| MimeReader_JwzMbox                                 | 3,209.473 us | 26.7118 us |  22.3056 us |
| MimeReader_HeaderStressTest                        |     6.901 us |  0.1346 us |   0.1259 us |

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

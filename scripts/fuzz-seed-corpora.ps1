#
# fuzz-seed-corpora.ps1
#
# Builds the initial (seed) corpus of one or all of the MimeKit fuzz targets from UnitTests/TestData
# plus the hand-written synthetic seeds in Fuzzing/Corpora/<target>/. See FuzzingPlan.md.
#
# Usage:
#   ./scripts/fuzz-seed-corpora.ps1 [-Target <name>|all] [-Harness <path>] [-OutputDirectory <dir>] [-Force]
#
# -Harness is the path to an *uninstrumented* MimeKit.Fuzzing executable. If omitted, the harness is built
# (Debug) first. The corpus of a target is only (re)seeded if it does not exist yet or if -Force is given.
#
[CmdletBinding()]
param (
    [string]
    $Target = "all",
    [string]
    $Harness = "",
    [string]
    $OutputDirectory = "",
    [switch]
    $Force
)

$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $PSScriptRoot
$TestData = Join-Path $RepoRoot "UnitTests" "TestData"
$HandWritten = Join-Path $RepoRoot "Fuzzing" "Corpora"
$Latin1 = [System.Text.Encoding]::Latin1
$Utf8 = [System.Text.UTF8Encoding]::new($false)

if ($OutputDirectory -eq "") {
    $OutputDirectory = Join-Path $RepoRoot "artifacts" "fuzz"
}

# Do not seed more than this many messages from a single mbox file.
$MaxMessagesPerMbox = 200

if ($Harness -eq "") {
    $project = Join-Path $RepoRoot "Fuzzing" "MimeKit.Fuzzing.csproj"

    Write-Output "Building the fuzzing harness..."
    & dotnet build $project -c Debug -v q -nologo | Out-Null

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to build $project"
    }

    $exe = if ($IsWindows) { "MimeKit.Fuzzing.exe" } else { "MimeKit.Fuzzing" }
    $Harness = Join-Path $RepoRoot "Fuzzing" "bin" "Debug" "net10.0" $exe
}

# Get the name and max_len of every target from the harness.
$MaxLengths = [ordered]@{}

foreach ($line in (& $Harness --list)) {
    $fields = $line.Split("`t")
    $MaxLengths[$fields[0]] = [int]$fields[1]
}

if ($Target -eq "all") {
    $Targets = @($MaxLengths.Keys)
} elseif ($MaxLengths.Contains($Target)) {
    $Targets = @($Target)
} else {
    throw "Unknown fuzz target: $Target (expected one of: all, $($MaxLengths.Keys -join ', '))"
}

function Get-Files ([string] $Directory, [string[]] $Include = @("*")) {
    if (-not (Test-Path $Directory)) {
        return @()
    }

    return Get-ChildItem -Path $Directory -Recurse -File -Include $Include | Sort-Object FullName
}

# Split an mbox file into individual messages (each including its "From " marker line).
function Split-Mbox ([string] $Path) {
    $text = $Latin1.GetString([System.IO.File]::ReadAllBytes($Path))
    $messages = [regex]::Split($text, '(?<=\n)(?=From )')

    return $messages | Where-Object { $_.StartsWith("From ") } | Select-Object -First $MaxMessagesPerMbox
}

# Get the raw (still folded) header fields of a message as @{ Name; Value; Raw } objects.
function Get-HeaderFields ([string] $Message) {
    $match = [regex]::Match($Message, '\r?\n\r?\n')
    $headers = if ($match.Success) { $Message.Substring(0, $match.Index + 1) } else { $Message }

    foreach ($field in [regex]::Matches($headers, '(?m)^(?<name>[!-9;-~]+)[ \t]*:[ \t]*(?<value>[^\r\n]*(?:\r?\n[ \t][^\r\n]*)*)')) {
        [pscustomobject]@{
            Name = $field.Groups["name"].Value.ToLowerInvariant()
            Value = $field.Groups["value"].Value
            Raw = $field.Value
        }
    }
}

$MessageCache = $null

# All of the messages in the MIME corpora (split mbox files included) as Latin1 strings.
function Get-Messages {
    if ($null -eq $script:MessageCache) {
        $list = [System.Collections.Generic.List[string]]::new()

        foreach ($dir in "messages", "compliance", "partial") {
            foreach ($file in (Get-Files (Join-Path $TestData $dir) @("*.eml", "*.txt", "*.msg"))) {
                if ($file.Name -notlike "*summary*" -and $file.Length -le 1MB) {
                    $list.Add($Latin1.GetString([System.IO.File]::ReadAllBytes($file.FullName)))
                }
            }
        }

        foreach ($file in (Get-Files (Join-Path $TestData "dkim") @("*.msg"))) {
            $list.Add($Latin1.GetString([System.IO.File]::ReadAllBytes($file.FullName)))
        }

        foreach ($file in (Get-Files (Join-Path $TestData "mbox") @("*.mbox.txt"))) {
            # Small mbox files are also seeded whole so that the mime-parser target exercises MimeFormat.Mbox.
            if ($file.Length -le 1MB) {
                $list.Add($Latin1.GetString([System.IO.File]::ReadAllBytes($file.FullName)))
            }

            foreach ($message in (Split-Mbox $file.FullName)) {
                $list.Add($message)
            }
        }

        $script:MessageCache = $list
    }

    return $script:MessageCache
}

$AddressHeaders = @("from", "to", "cc", "bcc", "reply-to", "sender", "resent-from", "resent-to", "resent-cc", "resent-bcc", "resent-sender", "return-path", "disposition-notification-to")
$StructuredHeaders = @("content-type", "content-disposition", "date", "resent-date", "message-id", "resent-message-id", "content-id", "references", "in-reply-to", "received")
$AuthenticationResultsHeaders = @("authentication-results", "arc-authentication-results")

# Get the (non-verbatim) string literals in a C# source file, with their escape sequences decoded.
function Get-CSharpStrings ([string] $Path) {
    $text = [System.IO.File]::ReadAllText($Path)

    foreach ($match in [regex]::Matches($text, '(?<![@$\\])"((?:[^"\\\r\n]|\\.)*)"')) {
        # Regex.Unescape does not understand C#'s 8-digit \UXXXXXXXX escapes, so expand those first.
        $value = [regex]::Replace($match.Groups[1].Value, '(?<!\\)((?:\\\\)*)\\U([0-9A-Fa-f]{8})', {
            param ($m)
            $m.Groups[1].Value + [char]::ConvertFromUtf32([Convert]::ToInt32($m.Groups[2].Value, 16))
        })

        [regex]::Unescape($value)
    }
}

$RtfExtractCache = $null

# Extract the PidTagRtfCompressed values (and the decompressed RTF) from the TNEF corpus.
function Get-RtfExtract {
    if ($null -eq $script:RtfExtractCache) {
        $dir = Join-Path ([System.IO.Path]::GetTempPath()) ("mimekit-fuzz-rtf-" + [guid]::NewGuid().ToString("N"))

        & $Harness --extract-rtf (Join-Path $dir "compressed") (Join-Path $dir "rtf") (Join-Path $TestData "tnef") | Out-Null

        if ($LASTEXITCODE -ne 0) {
            throw "Failed to extract the PidTagRtfCompressed values from the TNEF corpus"
        }

        $script:RtfExtractCache = $dir
    }

    return $script:RtfExtractCache
}

$DkimHeaders = @("dkim-signature", "arc-seal", "arc-message-signature")
$DkimSeedCache = $null

# Sign the message corpora with DKIM/ARC and prepend a FuzzDnsZone publishing the keys (see DkimSeedGenerator.cs).
function Get-DkimSeeds {
    if ($null -eq $script:DkimSeedCache) {
        $dir = Join-Path ([System.IO.Path]::GetTempPath()) ("mimekit-fuzz-dkim-" + [guid]::NewGuid().ToString("N"))

        & $Harness --generate-dkim-seeds $dir (Join-Path $TestData "dkim" "example.pem") (Join-Path $TestData "messages") (Join-Path $TestData "dkim") 2>$null | Out-Null

        if ($LASTEXITCODE -ne 0) {
            throw "Failed to generate the DKIM/ARC seeds"
        }

        $script:DkimSeedCache = $dir
    }

    return $script:DkimSeedCache
}

# Write the seeds of a single target. Each seed is a byte[]; seeds are named by their SHA-1 like libFuzzer does.
function Write-Seeds ([string] $Name, [System.Collections.Generic.List[byte[]]] $Seeds) {
    $corpus = Join-Path $OutputDirectory $Name "corpus"
    $maxLength = $MaxLengths[$Name]
    $count = 0

    New-Item -ItemType Directory -Force -Path $corpus | Out-Null

    foreach ($seed in $Seeds) {
        if ($seed.Length -eq 0 -or $seed.Length -gt $maxLength) {
            continue
        }

        $hash = [System.Convert]::ToHexString([System.Security.Cryptography.SHA1]::HashData($seed)).ToLowerInvariant()
        $path = Join-Path $corpus $hash

        if (-not (Test-Path $path)) {
            [System.IO.File]::WriteAllBytes($path, $seed)
            $count++
        }
    }

    Write-Output ("{0,-24} {1,5} seed(s) -> {2}" -f $Name, $count, $corpus)
}

foreach ($name in $Targets) {
    $corpus = Join-Path $OutputDirectory $name "corpus"

    if (-not $Force -and (Test-Path $corpus) -and (Get-ChildItem $corpus -File | Select-Object -First 1)) {
        Write-Output ("{0,-24} corpus already exists, skipping (use -Force to reseed)" -f $name)
        continue
    }

    $seeds = [System.Collections.Generic.List[byte[]]]::new()

    foreach ($file in (Get-Files (Join-Path $HandWritten $name))) {
        $seeds.Add([System.IO.File]::ReadAllBytes($file.FullName))
    }

    switch ($name) {
        { $_ -in "tnef-reader", "tnef-message" } {
            foreach ($file in (Get-Files (Join-Path $TestData "tnef") @("*.tnef"))) {
                $seeds.Add([System.IO.File]::ReadAllBytes($file.FullName))
            }
        }
        "html-tokenizer" {
            foreach ($file in (Get-Files (Join-Path $TestData "html") @("*.html", "*.xhtml"))) {
                $seeds.Add([System.IO.File]::ReadAllBytes($file.FullName))
            }

            foreach ($file in (Get-Files (Join-Path $TestData "text") @("*.html"))) {
                $seeds.Add([System.IO.File]::ReadAllBytes($file.FullName))
            }

            foreach ($test in "HtmlTokenizerTests.cs", "HtmlToHtmlTests.cs", "HtmlTextPreviewerTests.cs") {
                $path = Join-Path $RepoRoot "UnitTests" "Text" $test

                if (Test-Path $path) {
                    foreach ($value in (Get-CSharpStrings $path)) {
                        if ($value.Contains("<")) {
                            $seeds.Add($Utf8.GetBytes($value))
                        }
                    }
                }
            }
        }
        "text-converters" {
            foreach ($file in (Get-Files (Join-Path $TestData "text") @("*.txt"))) {
                $seeds.Add([System.IO.File]::ReadAllBytes($file.FullName))
            }

            foreach ($test in "TextToHtmlTests.cs", "TextToFlowedTests.cs", "FlowedToTextTests.cs", "FlowedToHtmlTests.cs", "TextToTextTests.cs", "UrlScannerTests.cs") {
                $path = Join-Path $RepoRoot "UnitTests" "Text" $test

                if (Test-Path $path) {
                    foreach ($value in (Get-CSharpStrings $path)) {
                        $seeds.Add($Utf8.GetBytes($value))
                    }
                }
            }

            foreach ($message in (Get-Messages)) {
                $match = [regex]::Match($message, "\r?\n\r?\n")

                if ($match.Success -and $message.Substring(0, $match.Index) -match '(?im)^content-type:\s*text/plain') {
                    $seeds.Add($Latin1.GetBytes($message.Substring($match.Index + $match.Length)))
                }
            }
        }
        "rtf-compressed" {
            foreach ($file in (Get-Files (Join-Path (Get-RtfExtract) "compressed"))) {
                $seeds.Add([System.IO.File]::ReadAllBytes($file.FullName))
            }
        }
        { $_ -in "rtf-tokenizer", "rtf-converters" } {
            foreach ($file in (Get-Files (Join-Path $TestData "rtf") @("*.rtf"))) {
                $seeds.Add([System.IO.File]::ReadAllBytes($file.FullName))
            }

            foreach ($file in (Get-Files (Join-Path (Get-RtfExtract) "rtf"))) {
                $seeds.Add([System.IO.File]::ReadAllBytes($file.FullName))
            }
        }
        { $_ -in "mime-reader", "mime-parser", "compliance-validators" } {
            foreach ($message in (Get-Messages)) {
                $seeds.Add($Latin1.GetBytes($message))
            }
        }
        "decoders" {
            # The first byte of the input selects the decoder (low 3 bits) and where the input is split (high 5 bits).
            $kinds = @{ ".b64" = @(0); ".qp" = @(1, 2); ".uu" = @(3, 4); ".uu-states" = @(3, 4); ".ntx" = @(5, 6); ".msg" = @(5, 6) }

            foreach ($file in (Get-Files (Join-Path $TestData "encoders")) + (Get-Files (Join-Path $TestData "yenc"))) {
                $bytes = [System.IO.File]::ReadAllBytes($file.FullName)
                $selected = if ($kinds.ContainsKey($file.Extension)) { $kinds[$file.Extension] } else { 0..7 }

                foreach ($kind in $selected) {
                    $seeds.Add([byte[]](@([byte]($kind -bor (16 -shl 3))) + $bytes))
                }
            }
        }
        "encoders" {
            # The first byte of the input selects the encoder (modulo 9) and the second byte selects the line length
            # (and, together with the first byte, seeds the chunking pattern).
            foreach ($file in (Get-Files (Join-Path $TestData "encoders"))) {
                if ($file.Extension -notin ".jpg", ".txt") {
                    continue
                }

                $bytes = [System.IO.File]::ReadAllBytes($file.FullName)

                foreach ($kind in 0..8) {
                    $seeds.Add([byte[]](@([byte]$kind, [byte]16) + $bytes))
                }
            }

            # Line-ending and whitespace edge cases for the quoted-printable encoder.
            $text = "line with trailing space `r`nline with trailing tab`t`n= sign`r`r`n" + ("x" * 74) + "`t`r`n" + ("y" * 75) + "=`n."
            foreach ($kind in 0..8) {
                $seeds.Add([byte[]](@([byte]$kind, [byte]4) + $Latin1.GetBytes($text)))
            }
        }
        "filters" {
            # The first byte of the input selects the filter (modulo 13) and the second byte selects its options
            # (and, together with the first byte, seeds the chunking pattern). See FiltersTarget.cs.
            $index = 0
            foreach ($message in (Get-Messages)) {
                $seeds.Add([byte[]](@([byte]($index % 13), [byte](($index / 13) % 256)) + $Latin1.GetBytes($message)))
                $index++
            }

            foreach ($file in (Get-Files (Join-Path $TestData "openpgp") @("*.asc", "*.txt"))) {
                $bytes = [System.IO.File]::ReadAllBytes($file.FullName)

                foreach ($kind in 10..12) {
                    foreach ($option in 0..3) {
                        $seeds.Add([byte[]](@([byte]$kind, [byte]$option) + $bytes))
                    }
                }
            }

            # Line-ending, "From " and trailing-whitespace edge cases plus every OpenPGP armor marker.
            $texts = @(
                "a`r`r`nb`rc`n`r`nFrom me`n>From you`r`nFro",
                "line  `t `r`nnext `t`n  `n`n`nlast `t",
                ("x" * 78) + "`r`n" + ("y" * 79) + "`nFrom z`n" + [char]0 + [char]0xE9 + "`r",
                "pre`r`n-----BEGIN PGP MESSAGE-----`r`n`r`nabc`r`n-----END PGP MESSAGE-----`r`npost",
                "pre`n-----BEGIN PGP SIGNED MESSAGE-----`nHash: SHA256`n`nhi`n-----BEGIN PGP SIGNATURE-----`n`nsig`n-----END PGP SIGNATURE-----`npost`n",
                "-----BEGIN PGP PUBLIC KEY BLOCK-----`nk`n-----END PGP PUBLIC KEY BLOCK-----`n-----BEGIN PGP PRIVATE KEY BLOCK-----`nk`n-----END PGP PRIVATE KEY BLOCK-----"
            )
            foreach ($text in $texts) {
                foreach ($kind in 0..12) {
                    $seeds.Add([byte[]](@([byte]$kind, [byte]5) + $Latin1.GetBytes($text)))
                }
            }
        }
        "address-parser" {
            foreach ($message in (Get-Messages)) {
                foreach ($field in (Get-HeaderFields $message)) {
                    if ($field.Name -in $AddressHeaders) {
                        $seeds.Add($Latin1.GetBytes($field.Value))
                    }
                }
            }
        }
        "header-values" {
            foreach ($message in (Get-Messages)) {
                foreach ($field in (Get-HeaderFields $message)) {
                    if ($field.Name -in $StructuredHeaders) {
                        $seeds.Add($Latin1.GetBytes($field.Value))
                        $seeds.Add($Latin1.GetBytes($field.Raw))
                    }
                }
            }
        }
        "rfc2047" {
            foreach ($message in (Get-Messages)) {
                foreach ($field in (Get-HeaderFields $message)) {
                    if ($field.Name -eq "subject" -or $field.Value.Contains("=?")) {
                        $seeds.Add($Latin1.GetBytes($field.Value))
                    }
                }
            }
        }
        "smtputf8-validator" {
            foreach ($value in (Get-CSharpStrings (Join-Path $RepoRoot "UnitTests" "Utils" "SmtpUtf8AddressValidatorTests.cs"))) {
                if ($value.Contains("@")) {
                    $seeds.Add($Utf8.GetBytes($value))
                }
            }

            foreach ($message in (Get-Messages)) {
                foreach ($field in (Get-HeaderFields $message)) {
                    if ($field.Name -in $AddressHeaders) {
                        foreach ($address in [regex]::Matches($field.Value, '[^\s<>(),:;]+@[^\s<>(),:;]+')) {
                            $seeds.Add($Latin1.GetBytes($address.Value))
                        }
                    }
                }
            }
        }
        "authentication-results" {
            foreach ($value in (Get-CSharpStrings (Join-Path $RepoRoot "UnitTests" "Cryptography" "AuthenticationResultsTests.cs"))) {
                $seeds.Add($Utf8.GetBytes($value))
            }

            foreach ($message in (Get-Messages)) {
                foreach ($field in (Get-HeaderFields $message)) {
                    if ($field.Name -in $AuthenticationResultsHeaders) {
                        $seeds.Add($Latin1.GetBytes($field.Value))
                    }
                }
            }
        }
        "dns-records" {
            # Zone files (bind9/tinydns) and the TXT strings embedded in them.
            foreach ($file in (Get-Files (Join-Path $TestData "dkim") @("*.bind9", "*.tinydns", "*.pub"))) {
                $text = $Utf8.GetString([System.IO.File]::ReadAllBytes($file.FullName))
                $seeds.Add($Utf8.GetBytes($text))

                $quoted = [regex]::Matches($text, '"([^"]*)"') | ForEach-Object { $_.Groups[1].Value }
                if ($quoted) {
                    $seeds.Add($Utf8.GetBytes(($quoted -join "")))
                }
            }

            foreach ($test in "DkimPublicKeyRecordTests.cs", "DmarcRecordTests.cs", "DmarcVerifierTests.cs", "DkimTests.cs", "ArcVerifierTests.cs") {
                foreach ($value in (Get-CSharpStrings (Join-Path $RepoRoot "UnitTests" "Cryptography" $test))) {
                    if ($value.Contains("=") -or $value.Contains(".")) {
                        $seeds.Add($Utf8.GetBytes($value))
                    }
                }
            }

            foreach ($message in (Get-Messages)) {
                foreach ($field in (Get-HeaderFields $message)) {
                    if ($field.Name -in $DkimHeaders) {
                        $seeds.Add($Latin1.GetBytes($field.Value))
                    } elseif ($field.Name -eq "from") {
                        foreach ($address in [regex]::Matches($field.Value, '@([^\s<>(),:;]+)')) {
                            $seeds.Add($Latin1.GetBytes($address.Groups[1].Value))
                        }
                    }
                }
            }
        }
        { $_ -in "dkim-arc-verifier", "dmarc-verifier" } {
            foreach ($file in (Get-Files (Get-DkimSeeds))) {
                $seeds.Add([System.IO.File]::ReadAllBytes($file.FullName))
            }

            # The unit test messages as-is: no zone, so every key lookup is NXDOMAIN.
            foreach ($file in (Get-Files (Join-Path $TestData "dkim") @("*.msg"))) {
                $seeds.Add([System.IO.File]::ReadAllBytes($file.FullName))
            }
        }
        default {
            Write-Warning "No seed sources are defined for $name; only the hand-written seeds were used."
        }
    }

    Write-Seeds $name $seeds
}

if ($null -ne $RtfExtractCache) {
    Remove-Item -Recurse -Force $RtfExtractCache
}

if ($null -ne $DkimSeedCache) {
    Remove-Item -Recurse -Force $DkimSeedCache
}

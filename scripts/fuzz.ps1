#
# fuzz.ps1
#
# Runs coverage-guided fuzzing (SharpFuzz + libFuzzer) against one or all of the MimeKit fuzz targets.
# See FuzzingPlan.md for the targets, their invariants and how to triage findings.
#
# Usage:
#   ./scripts/fuzz.ps1 -Target <name>|all [-Duration <seconds>] [-Parallel <n>] [-Jobs <n>]
#   ./scripts/fuzz.ps1 -Target <name> -Reproduce <crash-file>
#
# Everything is written under artifacts/fuzz/:
#   bin/                    The published, sharpfuzz-instrumented harness (recreated on every run).
#   tools/                  The downloaded libfuzzer-dotnet driver.
#   <target>/corpus/        The working corpus. It is seeded once and keeps growing from run to run.
#   <target>/crashes/       Crashes, timeouts and out-of-memory inputs found by libFuzzer.
#   <target>/fuzz.log       The libFuzzer output of the last run.
#
# The script exits non-zero if any target found a crash, timeout or out-of-memory input.
#
[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [string]
    $Target,
    [int]
    $Duration = 600,
    [int]
    $Parallel = [Environment]::ProcessorCount,
    [int]
    $Jobs = 1,
    [string]
    $Reproduce = "",
    [int]
    $Timeout = 10,
    [int]
    $RssLimitMB = 2048,
    [string]
    $LibFuzzer = "",
    [switch]
    $SkipBuild
)

$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $PSScriptRoot
$Artifacts = Join-Path $RepoRoot "artifacts" "fuzz"
$BinDirectory = Join-Path $Artifacts "bin"
$ToolsDirectory = Join-Path $Artifacts "tools"
$Dictionaries = Join-Path $RepoRoot "Fuzzing" "Dictionaries"
$Project = Join-Path $RepoRoot "Fuzzing" "MimeKit.Fuzzing.csproj"

# The libfuzzer-dotnet release to download (https://github.com/Metalnem/libfuzzer-dotnet).
$LibFuzzerVersion = "v2025.05.02.0904"

# The dictionary used by each target (targets that are not listed run without a dictionary).
$TargetDictionaries = @{
    "tnef-reader" = "tnef.dict"
    "tnef-message" = "tnef.dict"
    "rtf-compressed" = "rtf-compressed.dict"
    "rtf-tokenizer" = "rtf.dict"
    "rtf-converters" = "rtf.dict"
    "mime-reader" = "mime.dict"
    "mime-parser" = "mime.dict"
    "compliance-validators" = "mime.dict"
    "header-values" = "mime.dict"
    "rfc2047" = "mime.dict"
    "address-parser" = "address.dict"
    "decoders" = "decoders.dict"
}

if ($IsWindows) {
    $RuntimeIdentifier = "win-x64"
    $Harness = Join-Path $BinDirectory "MimeKit.Fuzzing.exe"
    $LibFuzzerAsset = "libfuzzer-dotnet-windows.exe"
} elseif ($IsLinux) {
    $RuntimeIdentifier = "linux-x64"
    $Harness = Join-Path $BinDirectory "MimeKit.Fuzzing"
    $LibFuzzerAsset = "libfuzzer-dotnet-ubuntu"
} else {
    throw "Fuzzing is only supported on Linux and Windows (libfuzzer-dotnet is not available for this platform)."
}

function Get-LibFuzzer {
    if ($LibFuzzer -ne "") {
        return (Resolve-Path $LibFuzzer).Path
    }

    $path = Join-Path $ToolsDirectory $LibFuzzerAsset

    if (-not (Test-Path $path)) {
        $url = "https://github.com/Metalnem/libfuzzer-dotnet/releases/download/$LibFuzzerVersion/$LibFuzzerAsset"

        Write-Host "Downloading $url..."
        New-Item -ItemType Directory -Force -Path $ToolsDirectory | Out-Null
        Invoke-WebRequest -Uri $url -OutFile $path

        if (-not $IsWindows) {
            & chmod +x $path | Out-Null
        }
    }

    return $path
}

function Invoke-Native ([string] $Description, [scriptblock] $Command) {
    & $Command

    if ($LASTEXITCODE -ne 0) {
        throw "$Description failed (exit code $LASTEXITCODE)."
    }
}

# 1. Publish the harness. The instrumented binaries are always recreated from scratch because sharpfuzz
#    modifies MimeKit.Core.dll in place (and refuses to instrument an assembly twice).
if (-not $SkipBuild) {
    if (Test-Path $BinDirectory) {
        Remove-Item -Recurse -Force $BinDirectory
    }

    Write-Output "Publishing the fuzzing harness ($RuntimeIdentifier)..."
    Invoke-Native "dotnet publish" { dotnet publish $Project -c Release -r $RuntimeIdentifier --self-contained -o $BinDirectory -v q -nologo }
} elseif (-not (Test-Path $Harness)) {
    throw "-SkipBuild was specified but $Harness does not exist."
}

# Get the name and max_len of every target from the (not yet instrumented) harness.
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

if ($Reproduce -ne "" -and $Targets.Count -ne 1) {
    throw "-Reproduce requires a single -Target."
}

# 2. Seed the corpora that do not exist yet. This must happen before instrumentation because the seed
#    script runs the harness to extract the PidTagRtfCompressed values from the TNEF corpus.
if (-not $SkipBuild -and $Reproduce -eq "") {
    foreach ($name in $Targets) {
        & (Join-Path $PSScriptRoot "fuzz-seed-corpora.ps1") -Target $name -Harness $Harness -OutputDirectory $Artifacts
    }
}

# 3. Instrument MimeKit.Core.dll.
if (-not $SkipBuild) {
    Write-Output "Instrumenting MimeKit.Core.dll..."
    Push-Location $RepoRoot
    try {
        Invoke-Native "dotnet tool restore" { dotnet tool restore | Out-Null }
        Invoke-Native "sharpfuzz" { dotnet sharpfuzz (Join-Path $BinDirectory "MimeKit.Core.dll") }
    } finally {
        Pop-Location
    }
}

$LibFuzzerPath = Get-LibFuzzer

function Get-LibFuzzerArguments ([string] $Name) {
    $directory = Join-Path $Artifacts $Name
    $crashes = Join-Path $directory "crashes"

    New-Item -ItemType Directory -Force -Path $crashes | Out-Null
    New-Item -ItemType Directory -Force -Path (Join-Path $directory "corpus") | Out-Null

    $arguments = @(
        "--target_path=$Harness",
        "-max_len=$($MaxLengths[$Name])",
        "-timeout=$Timeout",
        "-rss_limit_mb=$RssLimitMB",
        # A trailing "/" works on Windows too, and unlike "\" it does not escape the closing quote that Start-Process adds.
        "-artifact_prefix=$crashes/",
        "-print_final_stats=1"
    )

    if ($TargetDictionaries.ContainsKey($Name)) {
        $arguments += "-dict=$(Join-Path $Dictionaries $TargetDictionaries[$Name])"
    }

    return $arguments
}

# Reproduce a single crash file.
if ($Reproduce -ne "") {
    $env:MIMEKIT_FUZZ_TARGET = $Targets[0]
    $arguments = (Get-LibFuzzerArguments $Targets[0]) + (Resolve-Path $Reproduce).Path

    & $LibFuzzerPath @arguments
    exit $LASTEXITCODE
}

# 4. Fuzz. A single target runs in the foreground; with -Target all, one libFuzzer process is started per
#    target with at most -Parallel of them running at once.
function Start-Fuzzer ([string] $Name) {
    $directory = Join-Path $Artifacts $Name
    $log = Join-Path $directory "fuzz.log"
    $arguments = (Get-LibFuzzerArguments $Name) + "-max_total_time=$Duration"

    if ($Jobs -gt 1) {
        $arguments += "-jobs=$Jobs", "-workers=$Jobs"
    }

    $arguments += Join-Path $directory "corpus"

    # The child process inherits a snapshot of the environment when it starts.
    $env:MIMEKIT_FUZZ_TARGET = $Name

    $process = Start-Process -FilePath $LibFuzzerPath -ArgumentList ($arguments | ForEach-Object { "`"$_`"" }) `
        -WorkingDirectory $directory -RedirectStandardError $log -RedirectStandardOutput "$log.stdout" `
        -NoNewWindow -PassThru

    # Cache the process handle; otherwise ExitCode may be unavailable once the process has exited.
    $null = $process.Handle

    return [pscustomobject]@{ Name = $Name; Process = $process }
}

function Get-Summary ([string] $Name) {
    $directory = Join-Path $Artifacts $Name
    $log = Join-Path $directory "fuzz.log"
    $crashes = Get-ChildItem -Path (Join-Path $directory "crashes") -File -ErrorAction SilentlyContinue
    $lines = if (Test-Path $log) { Get-Content $log } else { @() }
    $executions = ""
    $features = ""
    $corpus = ""

    $status = $lines | Where-Object { $_ -match '^#\d+\s' } | Select-Object -Last 1

    if ($status -match '^#(\d+)') { $executions = $Matches[1] }
    # libfuzzer-dotnet reports the managed coverage as features (ft:); cov: only counts the native driver.
    if ($status -match '\bft: (\d+)') { $features = $Matches[1] }
    if ($status -match '\bcorp: (\d+)') { $corpus = $Matches[1] }

    $stat = $lines | Where-Object { $_ -match '^stat::number_of_executed_units: (\d+)' } | Select-Object -Last 1
    if ($stat -match '(\d+)$') { $executions = $Matches[1] }

    [pscustomobject]@{
        Target = $Name
        Executions = $executions
        Features = $features
        Corpus = $corpus
        Crashes = @($crashes | Where-Object { $_.Name -like "crash-*" -or $_.Name -like "deadly-signal*" }).Count
        Timeouts = @($crashes | Where-Object { $_.Name -like "timeout-*" }).Count
        OOMs = @($crashes | Where-Object { $_.Name -like "oom-*" }).Count
    }
}

$StartTime = Get-Date

if ($Targets.Count -eq 1) {
    $name = $Targets[0]
    $directory = Join-Path $Artifacts $name
    $arguments = (Get-LibFuzzerArguments $name) + "-max_total_time=$Duration"

    if ($Jobs -gt 1) {
        $arguments += "-jobs=$Jobs", "-workers=$Jobs"
    }

    $arguments += Join-Path $directory "corpus"
    $env:MIMEKIT_FUZZ_TARGET = $name

    Write-Output "Fuzzing $name for $Duration second(s)..."
    Push-Location $directory
    try {
        & $LibFuzzerPath @arguments 2>&1 | Tee-Object -FilePath (Join-Path $directory "fuzz.log")
        $exitCode = $LASTEXITCODE
    } finally {
        Pop-Location
    }
} else {
    $pending = [System.Collections.Generic.Queue[string]]::new([string[]]$Targets)
    $running = [System.Collections.Generic.List[object]]::new()

    $exitCode = 0

    Write-Output "Fuzzing $($Targets.Count) targets for $Duration second(s) each, $Parallel at a time..."

    while ($pending.Count -gt 0 -or $running.Count -gt 0) {
        while ($pending.Count -gt 0 -and $running.Count -lt $Parallel) {
            $fuzzer = Start-Fuzzer $pending.Dequeue()
            $running.Add($fuzzer)
            Write-Output "  started $($fuzzer.Name) (pid $($fuzzer.Process.Id))"
        }

        Start-Sleep -Seconds 5

        foreach ($fuzzer in @($running)) {
            if ($fuzzer.Process.HasExited) {
                $running.Remove($fuzzer) | Out-Null
                Write-Output "  finished $($fuzzer.Name) (exit code $($fuzzer.Process.ExitCode))"

                if ($fuzzer.Process.ExitCode -ne 0) {
                    $exitCode = $fuzzer.Process.ExitCode
                }
            }
        }
    }
}

$summary = $Targets | ForEach-Object { Get-Summary $_ }

Write-Output ""
Write-Output "Fuzzing finished in $([int]((Get-Date) - $StartTime).TotalSeconds) second(s)."
$summary | Format-Table -AutoSize | Out-String -Width 200 | Write-Output

$findings = ($summary | Measure-Object -Property Crashes, Timeouts, OOMs -Sum | Measure-Object -Property Sum -Sum).Sum

if ($findings -gt 0) {
    Write-Output "Found $findings crashing input(s); see artifacts/fuzz/<target>/crashes/ and FuzzingPlan.md (Triage)."
    exit 1
}

if ($exitCode -ne 0) {
    Write-Output "libFuzzer exited with code $exitCode."
    exit $exitCode
}

exit 0

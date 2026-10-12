$rootDirectory = Split-Path $PSScriptRoot -Parent

# The Native AOT toolchain uses vswhere.exe to locate the MSVC linker, but the Visual Studio Installer
# directory is not always on the PATH (e.g. in non-Developer shells).
if ($IsWindows -or $env:OS -eq "Windows_NT")
{
    if (-not (Get-Command vswhere.exe -ErrorAction SilentlyContinue))
    {
        $vsInstallerDirectory = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer"

        if (Test-Path (Join-Path $vsInstallerDirectory "vswhere.exe"))
        {
            $env:PATH = "$env:PATH;$vsInstallerDirectory"
        }
    }
}
$publishOutput = dotnet publish $rootDirectory/AotCompatibility/AotCompatibility.csproj -nodeReuse:false /p:UseSharedCompilation=false /p:ExposeExperimentalFeatures=true
$publishExitCode = $LastExitCode

if ($publishExitCode -ne 0)
{
    Write-Host $publishOutput
    Write-Host "Failed to publish the AotCompatibility test app. Exit code is:", $publishExitCode
    Exit 1
}

$actualWarningCount = 0

foreach ($line in $($publishOutput -split "`r`n"))
{
    if ($line -like "*warning IL*")
    {
        Write-Host $line

        $actualWarningCount += 1
    }
}

Push-Location $rootDirectory/AotCompatibility/bin/Release/net10.0/win-x64/publish

Write-Host "Executing test App..."
./AotCompatibility.exe
$appExitCode = $LastExitCode
Write-Host "Finished executing test App"

Pop-Location

$testPassed = 0

if ($appExitCode -ne 0)
{
    $testPassed = 1
    Write-Host "There was an error while executing AotCompatibility Test App. LastExitCode is:", $appExitCode
}

Write-Host "Actual warning count is:", $actualWarningCount
$expectedWarningCount = 0

if ($actualWarningCount -ne $expectedWarningCount)
{
    $testPassed = 1
    Write-Host "Actual warning count:", $actualWarningCount, "is not as expected. Expected warning count is:", $expectedWarningCount
}

Exit $testPassed

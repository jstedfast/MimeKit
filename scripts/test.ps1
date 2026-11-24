[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $Configuration = "Debug",
    [string]
    $GenerateCodeCoverage = "false"
)

Write-Output "Configuration:        $Configuration"
Write-Output "GenerateCodeCoverage: $GenerateCodeCoverage"
Write-Output ""

[xml]$project = Get-Content UnitTests\UnitTests.csproj

# Get the OutputPath
$targetFramework = $project.SelectSingleNode("/Project/PropertyGroup/TargetFramework")
$OutputDir = Join-Path "UnitTests" "bin" $Configuration $targetFramework.InnerText

if ($GenerateCodeCoverage -eq 'true') {
    Write-Output "Instrumenting code..."

    & dotnet AltCover -i="$OutputDir" --inplace -s="System.*" -s="Microsoft.*" -s="BouncyCastle.*" -s="NUnit*" -s="Moq*" -s="AltCover.*" -s="testhost" -s="UnitTests"
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

Write-Output "Running the UnitTests"

$resultDir = (Resolve-Path $OutputDir).Path
dotnet test UnitTests\UnitTests.csproj --configuration $Configuration --no-build -- "NUnit.TestOutputXml=$resultDir"
exit $LASTEXITCODE

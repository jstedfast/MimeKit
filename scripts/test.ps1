[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $Configuration = "Debug",
    [string]
    $GenerateCodeCoverage = "no"
)

Write-Output "Configuration:        $Configuration"
Write-Output "GenerateCodeCoverage: $GenerateCodeCoverage"
Write-Output ""

if ($GenerateCodeCoverage -eq 'yes') {
    Write-Output "Running the UnitTests (code coverage enabled)"

    dotnet test -v 5 /p:Configuration=$Configuration /p:AltCover=true /p:AltCoverAssemblyExcludeFilter="Microsoft*" /p:AltCoverAssemblyExcludeFilter="NUnit*" /p:AltCoverAssemblyExcludeFilter="UnitTests*" UnitTests\UnitTests.csproj
} else {
    Write-Output "Running the UnitTests"

    dotnet test -v 5 /p:Configuration=$Configuration UnitTests\UnitTests.csproj
}
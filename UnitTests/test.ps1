$testDir = ".\bin\Debug\"
$tests = @("$testdir\UnitTests.dll")

# nuget install OpenCover
# nuget install NUnit.Runners

foreach ($elem in $tests) {
	.\OpenCover.4.5.3723\OpenCover.Console.exe `
	-register:user `
	-target:.\NUnit.Runners.2.6.4\tools\nunit-console.exe `
	"-targetargs: ""$elem"" /framework:net-4.5 /noshadow" `
	"-filter:+[MimeKit]* -[UnitTests]*" `
	-output:opencover.xml `
	-mergeoutput
}

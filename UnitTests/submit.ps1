$version = git rev-parse HEAD
$branch = git rev-parse --abbrev-ref HEAD

$commitAuthor = git show --quiet --format="%aN" $version
$commitEmail = git show --quiet --format="%aE" $version
$commitMessage = git show --quiet --format="%s" $version

#nuget install coveralls.net

Set-PSDebug -Trace 2
.\coveralls.net.0.5.0\csmacnz.Coveralls.exe `
--opencover -i opencover.xml `
--repoToken `
--commitId $version `
--commitBranch $branch `
--commitAuthor $commitAuthor `
--commitEmail $commitEmail `
--commitMessage $commitMessage `
--useRelativePaths `
--basePath .\UnitTests\bin\Debug


SET NUNITRUNNER=%USERPROFILE%\.nuget\packages\nunit.consolerunner\3.11.1\tools\nunit3-console.exe
SET COVERALLS=%USERPROFILE%\.nuget\packages\coveralls.net\0.7.0\tools
SET OPENCOVER=%USERPROFILE%\.nuget\packages\opencover\4.7.922\tools
SET OUTPUTDIR=UnitTests\bin\Debug\net48

%OPENCOVER%\OpenCover.Console.exe -filter:"+[MimeKit]* -[UnitTests]* -[submodules]* -[Mono.Data.Sqlite]*" -target:"%NUNITRUNNER%" -targetargs:"/domain:single %OUTPUTDIR%\UnitTests.dll" -output:opencover.xml

%COVERALLS%\csmacnz.Coveralls.exe --opencover -i opencover.xml --repoToken %COVERALLS_REPO_TOKEN% --useRelativePaths --basePath %OUTPUTDIR% --commitId %APPVEYOR_REPO_COMMIT% --commitBranch %APPVEYOR_REPO_BRANCH% --commitAuthor %APPVEYOR_REPO_COMMIT_AUTHOR% --commitEmail %APPVEYOR_REPO_COMMIT_AUTHOR_EMAIL% --commitMessage %APPVEYOR_REPO_COMMIT_MESSAGE% --jobId %APPVEYOR_BUILD_NUMBER% --serviceName appveyor

rmdir /q /s cov-int
del cov-int.zip
..\cov-analysis\bin\cov-build.exe --dir cov-int "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe" /t:Rebuild MimeKit.Coverity.sln

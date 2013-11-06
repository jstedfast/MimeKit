all:
	xbuild /target:Build /p:Configuration=Release MimeKitDesktopOnly.sln

debug:
	xbuild /target:Build /p:Configuration=Debug MimeKitDesktopOnly.sln

clean:
	xbuild /target:Clean /p:Configuration=Debug MimeKitDesktopOnly.sln
	xbuild /target:Clean /p:Configuration=Release MimeKitDesktopOnly.sln

update-docs: MimeKit/bin/Release/lib/net40/MimeKit.dll
	mdoc update --delete -o docs/en MimeKit/bin/Release/lib/net40/MimeKit.dll

merge-docs: MimeKit/bin/Release/lib/net40/MimeKit.dll MimeKit/bin/Release/lib/net40/MimeKit.xml
	mdoc update -i MimeKit/bin/Release/lib/net40/MimeKit.xml -o docs/en MimeKit/bin/Release/lib/net40/MimeKit.dll

html-docs:
	mdoc export-html -o htmldocs docs/en

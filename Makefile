all:

update-docs: MimeKit/bin/Debug/lib/net40/MimeKit.dll
	mdoc update --delete -o docs/en MimeKit/bin/Debug/lib/net40/MimeKit.dll

merge-docs:
	mdoc update -i MimeKit/bin/Release/lib/net40/MimeKit.xml -o docs/en MimeKit/bin/Release/lib/net40/MimeKit.dll


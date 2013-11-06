all:

integrate-csharp:
	mdoc update -i MimeKit/bin/Debug/lib/net40/MimeKit.xml -o docs/en MimeKit/bin/Debug/lib/net40/MimeKit.dll

make-html:
	mdoc export-html -o htmldocs docs/en

update-docs: MimeKit/bin/Debug/lib/net40/MimeKit.dll
	(cd MimeKit/bin/Debug/lib/net40; mdoc update --delete -o ../../../../../docs/en MimeKit.dll)

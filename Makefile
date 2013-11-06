all:

update-docs: MimeKit/bin/Debug/lib/mac/MimeKit.Mac.dll
	(cd MimeKit/bin/Debug/lib/mac; mdoc update --delete -o ../../../../../docs/en MimeKit.Mac.dll)

integrate-csharp:
	mdoc update -i MimeKit/bin/Debug/lib/mac/MimeKit.Mac.xml -o docs/en MimeKit/bin/Debug/lib/mac/MimeKit.Mac.dll

make-html:
	mdoc export-html -o htmldocs docs/en


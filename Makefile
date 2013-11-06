all:

update-docs: MimeKit/bin/Debug/lib/mac/MimeKit.Mac.dll
	(cd MimeKit/bin/Debug/lib/mac; mdoc update --delete -o ../../../../docs/en MimeKit.Mac.dll)


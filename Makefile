all:

update-docs: MimeKit/bin/Debug/lib/net40/MimeKit.dll
	(cd MimeKit/bin/Debug/lib/net40; mdoc update --delete -o ../../../../../docs/en MimeKit.dll)

OUTDIR=MimeKit/bin/Release/lib/net40
ASSEMBLY=$(OUTDIR)/MimeKit.dll
XMLDOCS=$(OUTDIR)/MimeKit.xml

all:
	xbuild /target:Build /p:Configuration=Release MimeKitDesktopOnly.sln

debug:
	xbuild /target:Build /p:Configuration=Debug MimeKitDesktopOnly.sln

clean:
	xbuild /target:Clean /p:Configuration=Debug MimeKitDesktopOnly.sln
	xbuild /target:Clean /p:Configuration=Release MimeKitDesktopOnly.sln

update-docs: $(ASSEMBLY)
	mdoc update --delete -o docs/en $(ASSEMBLY)

merge-docs: $(ASSEMBLY) $(XMLDOCS)
	mdoc update -i $(XMLDOCS) -o docs/en $(ASSEMBLY)

html-docs:
	mdoc export-html -o htmldocs docs/en

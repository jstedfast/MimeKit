OUTDIR=MimeKit/bin/Release/lib/net40
ASSEMBLY=$(OUTDIR)/MimeKit.dll
XMLDOCS=$(OUTDIR)/MimeKit.xml

all:
	xbuild /target:Build /p:Configuration=Release MimeKit.Net40.sln

debug:
	xbuild /target:Build /p:Configuration=Debug MimeKit.Net40.sln

clean:
	xbuild /target:Clean /p:Configuration=Debug MimeKit.Net40.sln
	xbuild /target:Clean /p:Configuration=Release MimeKit.Net40.sln

check-docs:
	@find docs/en -name "*.xml" -exec grep -l "To be added." {} \;

update-docs: $(ASSEMBLY)
	mdoc update --delete -o docs/en $(ASSEMBLY)

merge-docs: $(ASSEMBLY) $(XMLDOCS)
	mdoc update -i $(XMLDOCS) -o docs/en $(ASSEMBLY)

html-docs:
	mdoc export-html --force-update --template=docs/github-pages.xslt -o ../MimeKit-docs/docs docs/en

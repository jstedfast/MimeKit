OUTDIR=MimeKit/bin/Release/lib/net45
ASSEMBLY=$(OUTDIR)/MimeKit.dll
XMLDOCS=$(OUTDIR)/MimeKit.xml
SOLUTION=MimeKit.Net45.sln

all:
	msbuild /target:Build /p:Configuration=Release $(SOLUTION)

debug:
	msbuild /target:Build /p:Configuration=Debug $(SOLUTION)

clean:
	msbuild /target:Clean /p:Configuration=Debug $(SOLUTION)
	msbuild /target:Clean /p:Configuration=Release $(SOLUTION)

check-docs:
	@find docs/en -name "*.xml" -exec grep -l "To be added." {} \;

update-docs: $(ASSEMBLY)
	mdoc update --delete -o docs/en $(ASSEMBLY)

merge-docs: $(ASSEMBLY) $(XMLDOCS)
	mdoc update -i $(XMLDOCS) -o docs/en $(ASSEMBLY)

html-docs:
	mdoc export-html --force-update --template=docs/github-pages.xslt -o ../MimeKit-docs/docs docs/en

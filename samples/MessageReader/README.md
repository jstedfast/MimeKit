# MessageReader

This Windows Forms sample displays an email using MimeKit and
[Ganss.Xss.HtmlSanitizer](https://github.com/mganss/HtmlSanitizer). The sanitizer is a
dependency of this sample only; it is not a dependency of MimeKit.

## Sanitizing email

`HtmlPreviewVisitor` selects the message body and sanitizes `text/html` parts
using `HtmlSanitizer.SanitizeDocument`, rather than a hand-written tag filter.
`SanitizeDocument` processes the whole document, including styles in the head,
while related MIME resources are still in scope.

Plain and flowed text are converted with `TextToHtml` and `FlowedToHtml`,
respectively. Their generated HTML is used directly, without an additional
sanitization pass. The resulting body, whether sanitized HTML or converted text,
is passed to `WebBrowser.DocumentText`.

The constructor defines a deliberately limited, read-only policy for HTML message
bodies:

* Keep common formatting elements, tables, inline styles, CSS classes, and
  sanitized stylesheet rules, including `@media`.
* Keep a limited set of CSS properties for colors, fonts, spacing, and layout.
  Drop resource-loading properties, including background images, cursors,
  filters, and generated content. Drop CSS custom properties, `@import`, and
  `@font-face`. Some messages will consequently look different.
* Remove scripts, event handlers, forms, embedded documents, SVG/MathML, metadata,
  and external stylesheet links.
* Remove link destinations and other navigation attributes. Links remain as text;
  they are not opened in the preview or in an external browser.
* Block remote, relative, local-file, and sender-supplied `data:` image URLs unless
  the reference resolves to a permitted image MIME part within the message.
  An absolute `Content-Location` can resolve to a MIME part; it is never fetched.

`FilterUrl` rejects URLs by default. For an image element, it may replace the URL
with a newly generated `data:` URI for a PNG, JPEG, GIF, or BMP part found in the
current `multipart/related` scope. It uses `cid:` or `Content-Location` resolution
without downloading anything. Missing images and other MIME types lose their
source attribute, while alternative text and dimensions are retained.

The event runs even when the original URI scheme was rejected. The sample uses
this to resolve `cid:` references **without globally allowing `data:` or `cid:`**.
It never cancels a sanitizer removal event to restore arbitrary input. MIME
resolution happens during sanitization, before the visitor leaves the related
part; there is no subsequent rewriting of the sanitized HTML.

## Rendering boundary

`MessageViewWindow` disables navigation, new windows, browser shortcuts, the
browser context menu, and drag-and-drop navigation for all message bodies,
including converted text. Sanitizer failures propagate; the sample does not fall
back to displaying the original HTML.

The existing WinForms `WebBrowser` control uses the legacy Internet Explorer
engine. This sample demonstrates the integration, **not a general-purpose browser
sandbox or a guarantee of pixel-perfect rendering**. Data-URI image support and
CSS rendering depend on the control's document mode. For a production viewer, use
a maintained browser engine, configure its isolation, script/native-bridge,
navigation, and resource-loading controls, and impose application-appropriate
message/image size limits. Keep HtmlSanitizer and its dependencies updated.

This policy renders a separate document. Do not assume that its styles are
suitable for inserting directly into the DOM of a webmail application's own UI;
use an appropriately isolated rendering context there.

## Build and run

Use a Visual Studio Developer PowerShell with the .NET desktop build tools and
the .NET Framework 4.8 targeting pack:

```powershell
msbuild .\samples\MessageReader\MessageReader.sln -restore -p:Configuration=Debug
.\samples\MessageReader\MessageReader\bin\Debug\MessageReader.exe
```

The sample references the MimeKit project in this checkout. On startup it displays
the bundled `xamarin3.eml` message.

The sample's NUnit project checks the email policy, inline-resource resolution,
text/HTML body selection, and loading message previews into the Windows browser
control. It does not add sanitizer dependencies to MimeKit's core test suite:

```powershell
msbuild .\samples\MessageReader\MessageReader.Tests\MessageReader.Tests.csproj -restore -p:Configuration=Debug
dotnet test .\samples\MessageReader\MessageReader.Tests\MessageReader.Tests.csproj --no-build --no-restore
```

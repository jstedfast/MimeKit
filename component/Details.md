MimeKit is an Open Source library for creating and parsing MIME, S/MIME and PGP 
messages on desktop platforms (e.g. Windows, Mac, and Linux) as well as mobile
platforms (e.g. iOS and Android). It also supports parsing of Unix mbox files.

Unlike any other .NET MIME parser, MimeKit's parser does not need to parse
string input nor does it use a TextReader. Instead, it parses raw byte streams,
thus allowing it to better support undeclared 8bit text in headers as well as
message bodies. It also means that MimeKit's parser is significantly faster
than other .NET MIME parsers.

MimeKit's parser also uses a real tokenizer when parsing the headers rather
than regex or string.Split() like most other .NET MIME parsers. This means that
MimeKit is much more RFC-compliant than any other .NET MIME parser out there,
including the commercial implementations.

In addition to having a far superior parser implementation, MimeKit's object
tree is not a derivative of System.Net.Mail objects and thus does not suffer
from System.Net.Mail's massive limitations or bugs.

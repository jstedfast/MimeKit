# MimeKit

## What is MimeKit?

MimeKit is a C# library which may be used for the creation and parsing of messages using the Multipurpose Internet Mail Extension (MIME), as defined by the following RFCs:

* [0822](http://www.ietf.org/rfc/rfc0822.txt): Standard for the Format of Arpa Internet Text Messages
* [1341](http://www.ietf.org/rfc/rfc1341.txt): MIME (Multipurpose Internet Mail Extensions): Mechanisms for Specifying and Describing the Format of Internet Message Bodies
* [1342](http://www.ietf.org/rfc/rfc1342.txt): Representation of Non-ASCII Text in Internet Message Headers
* [1521](http://www.ietf.org/rfc/rfc1521.txt): MIME (Multipurpose Internet Mail Extensions) Part One: Mechanisms for Specifying and Describing the Format of Internet Message Bodies (Obsoletes rfc1341)
* [1522](http://www.ietf.org/rfc/rfc1522.txt): MIME (Multipurpose Internet Mail Extensions) Part Two: Message Header Extensions for Non-ASCII Text (Obsoletes rfc1342)
* [1544](http://www.ietf.org/rfc/rfc1544.txt): The Content-MD5 Header Field
* [1847](http://www.ietf.org/rfc/rfc1847.txt): Security Multiparts for MIME: Multipart/Signed and Multipart/Encrypted
* [1864](http://www.ietf.org/rfc/rfc1864.txt): The Content-MD5 Header Field (Obsoletes rfc1544)
* [2015](http://www.ietf.org/rfc/rfc2015.txt): MIME Security with Pretty Good Privacy (PGP)
* [2045](http://www.ietf.org/rfc/rfc2045.txt): Multipurpose Internet Mail Extensions (MIME) Part One: Format of Internet Message Bodies
* [2046](http://www.ietf.org/rfc/rfc2046.txt): Multipurpose Internet Mail Extensions (MIME) Part Two: Media Types
* [2047](http://www.ietf.org/rfc/rfc2047.txt): Multipurpose Internet Mail Extensions (MIME) Part Three: Message Header Extensions for Non-ASCII Text
* [2048](http://www.ietf.org/rfc/rfc2048.txt): Multipurpose Internet Mail Extensions (MIME) Part Four: Registration Procedures
* [2049](http://www.ietf.org/rfc/rfc2049.txt): Multipurpose Internet Mail Extensions (MIME) Part Five: Conformance Criteria and Examples
* [2183](http://www.ietf.org/rfc/rfc2183.txt): Communicating Presentation Information in Internet Messages: The Content-Disposition Header Field
* [2184](http://www.ietf.org/rfc/rfc2184.txt): MIME Parameter Value and Encoded Word Extensions: Character Sets, Languages, and Continuations
* [2231](http://www.ietf.org/rfc/rfc2231.txt): MIME Parameter Value and Encoded Word Extensions: Character Sets, Languages, and Continuations (Obsoletes rfc2184)
* [2311](http://www.ietf.org/rfc/rfc2311.txt): S/MIME Version 2 Message Specification
* [2312](http://www.ietf.org/rfc/rfc2312.txt): S/MIME Version 2 Certificate Handling
* [2424](http://www.ietf.org/rfc/rfc2424.txt): Content Duration MIME Header Definition
* [2630](http://www.ietf.org/rfc/rfc2630.txt): Cryptographic Message Syntax
* [2632](http://www.ietf.org/rfc/rfc2632.txt): S/MIME Version 3 Certificate Handling
* [2633](http://www.ietf.org/rfc/rfc2633.txt): S/MIME Version 3 Message Specification
* [2634](http://www.ietf.org/rfc/rfc2634.txt): Enhanced Security Services for S/MIME
* [2822](http://www.ietf.org/rfc/rfc2822.txt): Internet Message Format (Obsoletes rfc0822)
* [3156](http://www.ietf.org/rfc/rfc3156.txt): MIME Security with OpenPGP (Updates rfc2015)
* [3850](http://www.ietf.org/rfc/rfc3850.txt): S/MIME Version 3.1 Certificate Handling (Obsoletes rfc2632)
* [3851](http://www.ietf.org/rfc/rfc3851.txt): S/MIME Version 3.1 Message Specification (Obsoletes rfc2633)
* [5322](http://www.ietf.org/rfc/rfc5322.txt): Internet Message Format (Obsoletes rfc2822) 

#### Other RFCs of interest:

* [1523](http://www.ietf.org/rfc/rfc1523.txt): The text/enriched MIME Content-type
* [1872](http://www.ietf.org/rfc/rfc1872.txt): The MIME Multipart/Related Content-type
* [1927](http://www.ietf.org/rfc/rfc1927.txt): Suggested Additional MIME Types for Associating Documents
* [2110](http://www.ietf.org/rfc/rfc2110.txt): MIME E-mail Encapsulation of Aggregate Documents, such as HTML (MHTML)
* [2111](http://www.ietf.org/rfc/rfc2111.txt): Content-ID and Message-ID Uniform Resource Locators
* [2112](http://www.ietf.org/rfc/rfc2112.txt): The MIME Multipart/Related Content-type (Obsoletes rfc1872)
* [2387](http://www.ietf.org/rfc/rfc2387.txt): The MIME Multipart/Related Content-type (Obsoletes rfc2112)

#### Cryptography related RFCs (needed by S/MIME)

* [2268](http://www.ietf.org/rfc/rfc2268.txt): A Description of the RC2(r) Encryption Algorithm
* [2313](http://www.ietf.org/rfc/rfc2313.txt): PKCS #1: RSA Encryption
* [2314](http://www.ietf.org/rfc/rfc2314.txt): PKCS #10: Certification Request Syntax
* [2315](http://www.ietf.org/rfc/rfc2315.txt): PKCS #7: Cryptographic Message Syntax
* [2631](http://www.ietf.org/rfc/rfc2631.txt): Diffie-Hellman Key Agreement Method 


## License Information

MimeKit is Copyright (C) 2012, 2013 Jeffrey Stedfast and is licensed under the MIT/X11 license:

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.

## History

As a developer and user of Electronic Mail clients, I had come to realize that the vast majority of E-Mail client
(and server) software had less-than-satisfactory MIME implementations. More often than not these E-Mail clients
created broken MIME messages and/or would incorrectly try to parse a MIME message thus subtracting from the full
benefits that MIME was meant to provide. MimeKit is meant to address this issue by following the MIME specification
as closely as possible while also providing programmers with an extremely easy to use high-level API.

This led me, at first, to implement another MIME parser library called [GMime](http://spruce.sourceforge.net/gmime)
which is implemented in C and later added a C# binding called GMime-Sharp.

Now that I typically find myself working in C# rather than lower level languages like C, I decided to
begin writing a new parser in C# which would not depend on GMime. This would also allow me to have more
flexibility in that I'd be able use Generics and create a more .NET-compliant API.

## Building

First, you'll need to clone MimeKit and Bouncy Castle from my GitHub repository:

    git clone git://github.com/jstedfast/MimeKit.git
    git clone git://github.com/jstedfast/bc-csharp.git

Currently, MimeKit depends on the visual-studio-2010 branch of bc-csharp for the Visual Studio 2010 project
files that I've added (to replace the Visual Studio 2003 project files). To switch to that branch,

    cd bc-csharp
    git checkout -b visual-studio-2010 origin/visual-studio-2010

In the top-level MimeKit source directory, there are two solution files: MimeKitDesktopOnly.sln and MimeKit.sln.

MimeKitDesktopOnly.sln just includes the .NET Framework 4.0 C# project (MimeKit/MimeKit.csproj) and the UnitTests
project (UnitTests/UnitTests.csproj).

MimeKit.sln includes everything that is in the MimeKitDesktopOnly solution as well as the projects for Xamarin.Android,
Xamarin.iOS, and Xamarin.Mac.

If you don't have the Xamarin products, you'll probably want to open the MimeKitDesktopOnly.sln instead of MimeKit.sln.

Once you've opened the appropriate MimeKit solution file in either Xamarin Studio or Visual Studio 2010+ (either will work),
you can simply choose Debug or Release and build.

Note: The Release build will generate the xml API documentation, but the Debug build will not.

## Contributing

The first thing you'll need to do is fork MimeKit to your own GitHub repository. Once you do that,

    git clone git://github.com/<your-account>/MimeKit.git

Once you've got some changes that you'd like to submit upstream to the official MimeKit repository,
simply send me a Pull Request and I will try to review your changes in a timely manner.

## Reporting Bugs

Have a bug or a feature request? [Please open a new issue](https://github.com/jstedfast/MimeKit/issues).

Before opening a new issue, please search for existing issues to avoid submitting duplicates.

## Documentation

You're looking at it.

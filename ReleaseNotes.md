# Release Notes

## MimeKit 4.6.0 (2024-05-17)

* Fixed hex format specifier for PGP keyserver lookup. (issue [#1028](https://github.com/jstedfast/MimeKit/issues/1028))
* Bumped the BouncyCastle.Cryptography dependency to v2.3.1 to fix some security issues.
* Fixed a bug in conversion logic between BouncyCastle DSA key parameters and System.Security.Cryptography's DSA implementation.

## MimeKit 4.5.0 (2024-04-13)

* Fixed MailboxAddress to not use punycode to encode or decode the local-part of an addr-spec.
  (issue [#1012](https://github.com/jstedfast/MimeKit/issues/1012))
* Removed explicit refs to CompilerServices.Unsafe and Encoding.CodePages from net6.0/8.0.
  (issue [#1013](https://github.com/jstedfast/MimeKit/issues/1013))

## MimeKit 4.4.0 (2024-03-02)

* Added net8.0 target.
* Improved folding logic for Disposition-Notification-Options header values.
  (issue [#979](https://github.com/jstedfast/MimeKit/issues/979))
* Added interfaces for MimeMessage, MimeEntity, MimePart, Multipart, etc.
  (issue [#980](https://github.com/jstedfast/MimeKit/issues/980))
* Fixed the FormatOptions.NewLineFormat setter logic.
* Modified AttachmentCollection.Add() for message/rfc822 attachments to better
  handle MimeParser exceptions.
  (issue [#1001](https://github.com/jstedfast/MimeKit/issues/1001))
* Bump BouncyCastle dependency to v2.3.0.
* Added support for ECC S/MIME certificates.
  (issue [#998](https://github.com/jstedfast/MimeKit/issues/998))
* Improved Unix2Dos and Dos2Unix filters by fixing some corner cases exposed by new unit tests.
* Fixed MaxMimeDepth logic to still use MimePart subclasses when reached.
  (issue [#1006](https://github.com/jstedfast/MimeKit/issues/1006))

## MimeKit 4.3.0 (2023-11-11)

* Added work-around for broken Message-ID header values of the form &lt;id@@domain&gt;.
  (issue [#962](https://github.com/jstedfast/MimeKit/issues/962))
* Added virtual Multipart.TryGetValue(TextFormat, out TextPart) method that recursively
  iterates over child parts to find the TextPart with the desired format.
* Fixed MimeMessage.TextBody/HtmlBody to locate the text body in a multipart/mixed that is
  inside of a multipart/alternative. This resolves an issue locating the text body within
  some broken iOS Apple Mail messages. (issue [#963](https://github.com/jstedfast/MimeKit/issues/963))

## MimeKit 4.2.0 (2023-09-02)

* Follow the spec more closely for allowable header field characters.
  (issue [#936](https://github.com/jstedfast/MimeKit/issues/936))
* Avoid throwing NRE when an RC2 algorithm was used for S/MIME w/o parameters.
  (issue [#941](https://github.com/jstedfast/MimeKit/issues/941))
* Added a few more (undocumented) TnefPropertyIds.
* Optimized AttachmentCollection.Add(byte[], ...) by not copying the data to a new stream.
* Improved performance of HtmlTokenizer.
* Added new HtmlTokenizer constructors that take a Stream instead of a TextReader. This
  allows for a slight performance improvement over using a TextReader as well.
* Lazy-allocate Base64/QuotedPrintable decoders when decoding rfc2047-encoded headers.
  This is a very small reduction in GC pressure.
* Reduced memory allocations in Rfc2047.DecodePhrase() and DecodeText().
* Avoid allocating empty `List<string>`s in DomainList.ctor(), lazily allocate the
  list only when a domain is added. Another minor reduction in GC pressure.
* Updated the Date parser to allocate an internal list of tokens with a optimal
  initial capacity to avoid the need for reallocating.

## MimeKit 4.1.0 (2023-06-17)

* Readded the System.Net.Mail-to-MimeKit conversion APIs for the netstandard2.x frameworks.
  (issue [#913](https://github.com/jstedfast/MimeKit/issues/913))
* Fixed the MimeEntity.LoadAsync() overloads that take a ContentType parameter to to properly wait
  to dispose of the stream until after the entity has been parsed.
  (issue [#916](https://github.com/jstedfast/MimeKit/issues/916))
* Optimized conversion between HeaderId and string.
* Added a new IMimeParser interface that both MimeParser and ExperimentalMimePraser implement.
* Added "unicode" to the list of charset aliases for UTF-8.
  (issue [#923](https://github.com/jstedfast/MimeKit/issues/923))
* Added support for the Edwards Curve DSA PGP public key algorithm.
  (issue [#932](https://github.com/jstedfast/MimeKit/issues/932))
* Bumped System.Security.Cryptography.Pkcs dependency to 7.0.2.
* Bumped BouncyCastle dependency to 2.2.1.

## MimeKit 4.0.0 (2023-04-15)

* Ported to BouncyCastle v2.1.1. (issue [#865](https://github.com/jstedfast/MimeKit/issues/865))
* Fixed System.Net.Mail.MailMessage -> MimeMessage converter to reset MailMessage attachment/alternateview streams back
  to 0 after copying them. (issue [#907](https://github.com/jstedfast/MimeKit/issues/907))
* Added support for the signature expiration field in DKIM signatures which can be specified
  using the new DkimSigner.SignatureExpiresAfter property.
* Added equality operators for TnefNameId and TnefPropertyTag.
* Fixed MimeMessage's MessageId, ResentMessageId and InReplyTo property setters to be more lax.
  (issue [#912](https://github.com/jstedfast/MimeKit/issues/912))
* MimeKit and MimeKitLite nuget packages now include MimeKit.dll.config that contain assembly redirects
  which *may* resolve some issues some developers were having with loading assemblies such as
  System.Runtime.CompilerServices.Unsafe.dll.

## MimeKit 3.6.1 (2023-03-19)

* Improved the UrlScanner to allow numeric key/value pairs (seems to match behavior in other url detection algorithms).
* Fixed a mis-use of ArrayPool in the OpenPgpContext.Encrypt/EncryptAsync that could cause memory corruption.
* Use BitConverter to decode floats/doubles from the input buffer instead of custom code.
* Bumped the System.Security.Cryptography.Pkcs dependency to v6.0.2.

## MimeKit 3.6.0 (2023-03-04)

* Added the .msg &lt;-&gt; application/vnd.ms-outlook mime-type mapping.
  (issue [#880](https://github.com/jstedfast/MimeKit/issues/880))
* Improved encoding/formatting of List-Archive, List-Help, List-Post, List-Subscribe and List-Unsubscribe headers.
  (issue [#885](https://github.com/jstedfast/MimeKit/issues/885))
* Reduced memory allocations when encoding the mailbox/group names.
* Added more Rfc2047.EncodePhrase()/EncodeText() overloads that take `startIndex` and `count` arguments.
* Fixed parsing of Message-Id's containing a quoted string dot-atom (among regular dot-atoms).
  (issue [#889](https://github.com/jstedfast/MimeKit/issues/889))
* Fixed a bug in the parsing of HTML &lt;script&gt; content that sometimes caused a character to be duplicated.
* Use MailMessage.HeadersEncoding when coverting a MailMessage to a MimeMessage.
* Improved the UrlScanner to accept urls like `https://example.com?query`
  (issue [#897](https://github.com/jstedfast/MimeKit/issues/897))

## MimeKit 3.5.0 (2023-01-28)

* Fixed potential NRE's in the GnuPG config parser
* Modified AsBouncyCastleCertificate() extension method to throw on fail
* Added Clone() methods to ContentType, ContentDisposition and Parameter.

## MimeKit 3.4.3 (2022-11-25)

* Fixed a variety of memory leaks revealed by (issue [#852](https://github.com/jstedfast/MimeKit/issues/852))
* Fixed the message/delivery-status parser to handle extra blank lines between status groups.
  (issue [#855](https://github.com/jstedfast/MimeKit/issues/855))
* Updated packages to explicitly depend on System.Runtime.CompilerServices.Unsafe v6.0.0.
* Updated net6.0 dependencies to explicitly include System.Text.Encoding.CodePages v6.0.0.

## MimeKit 3.4.2 (2022-10-24)

* Fixed MessageDeliveryStatus.ParseStatusGroups() to catch FormatException instead of ParseException.
  (issue [#837](https://github.com/jstedfast/MimeKit/issues/837))
* Fixed DefaultSecureMimeContext to use the correct AppData path on Windows.
* Fixed MimeMessage .ctor(params object[] args) to respect a Date header argument.
  (issue [#840](https://github.com/jstedfast/MimeKit/issues/840))
* Updated MIME-Type to file extension mappings to add a few missing types/extensions.
  (issue [#844](https://github.com/jstedfast/MimeKit/issues/844))
* Fixed address parser to no longer throw ArgumentOutOfRangeException when parsing some mailbox names.
  (issue [#846](https://github.com/jstedfast/MimeKit/issues/846) and [#847](https://github.com/jstedfast/MimeKit/issues/847))
* Map codepage 932 to shift_jis instead of iso-2022-jp.
  (issue [#848](https://github.com/jstedfast/MimeKit/issues/848))

## MimeKit 3.4.1 (2022-09-12)

* Improved logic for reformatting headers when MimeMessage.WriteTo() is called with FormatOptions.International
  set to true.
* Fixed logic for quoting and/or encoding the MailboxAddress.Name in cases where the Name string contains
  quotes or parenthesis (especially when unicode characters are within the quotes or parenthesis).
* Improved logic in the address parser when it comes to unquoting mailbox names (i.o.w. unquote *before* decoding
  rather than after).
* Modified the Message-ID/Content-ID parser to be more lenient.
  (issue [#835](https://github.com/jstedfast/MimeKit/issues/835))

## MimeKit 3.4.0 (2022-08-17)

* Introduced a new IPunycode interface and Punycode class allowing developers to override the default
  implementation that uses .NET's IdnMapping class.
  (issue [#801](https://github.com/jstedfast/MimeKit/issues/801))
* Added HtmlAttributeCollection Contains, IndexOf and TryGetValue methods.
* Dropped .NET5.0 support.
* Changed HtmlToHtml converter to avoid decoding character references.
  (issue [#808](https://github.com/jstedfast/MimeKit/issues/808))
* Fixed BoundStream to only seek in the base stream if seeking is supported.
* Added a new TextPart.TryDetectEncoding() API.
  (issue [#804](https://github.com/jstedfast/MimeKit/issues/804))
* Added support for message/feedback-report via a new MessageFeedbackReport class.
* Fixed a potential memory leak.
* When unquoting parameter values, don't convert tabs to spaces.
  (issue [#809](https://github.com/jstedfast/MimeKit/issues/809))
* Don't call Encoding.RegisterProvider() anymore. Rely on developers doing this themselves in their application
  startup logic.
* Remove System.Text.Encoding.CodePages dependency for net4x.
* Expose the LineNumber property on MimeMessageBeginEventArgs and MimeEntityBeginEventArgs.
  (issue [#819](https://github.com/jstedfast/MimeKit/issues/819))

## MimeKit 3.3.0 (2022-06-11)

* Added Import() methods for X509Certificate2 for all S/MIME contexts.
  (issue [#784](https://github.com/jstedfast/MimeKit/issues/784))
* Handle S/MIME sha# as well as sha-# micalg names for improved interop.
  (issue [#790](https://github.com/jstedfast/MimeKit/issues/790))
* Fixed the MemoryBlockStream.Read() method to handle cases where the length of the stream is longer than
  int.MaxValue.
* Fixed TnefPart.ConvertToMessage() to promote lone multipart/mixed subparts to become the message body
  much like it used to work pre-v3.2.0. (issue [#789](https://github.com/jstedfast/MimeKit/issues/789))
* Reduced memory usage when using SecureMimeContext.Compress() and CompressAsync().
* Dropped support for net452 and net461 now that their life cycles have ended and are no longer supported
  by Microsoft. (issue [#768](https://github.com/jstedfast/MimeKit/issues/768))
* Added support for net462.

Special thanks to Fedir Klymenko for his improvements to MemoryBlockStream and SecureMimeContext.Compress!

## MimeKit 3.2.0 (2022-03-26)

* Rewrote QuotedPrintableEncoder to more strictly fold at the specified line length.
  (issue [#781](https://github.com/jstedfast/MimeKit/issues/781))
* Change the default maxLineLength for quoted-printable/base64 encoders to 76 to match the recommendation
  in the specification (was previously 72).
* Use cached Task instances (e.g. Task.CompletedTask) when possible to improve performance.
* Make use of ReadOnlySpan&lt;T&gt; instead of String.Substring() wherever possible to improve performance.
* Reduced string allocations in other ways.
* Provide MailboxAddress accessors for LocalPart and Domain.
  (issue [#766](https://github.com/jstedfast/MimeKit/issues/766))
* Replaced support for .NET Framework v4.6 with 4.6.1 and added a System.Text.Encoding.CodePages dependency
  to solve various cases where MimeKit would fail to initialize properly on ASP.NET systems using net461
  when system character encodings were not available.
* Fixed MessagePartial to use invariant culture when setting number/total param values.
* Make sure all int.TryParse() calls use the correct NumberStyles.
* Make use of a ValueStringBuilder to construct strings without needing to allocate a StringBuilder.
* Fixed InternetAddressList.TryParse() to fail on invalid input.
  (issue [#762](https://github.com/jstedfast/MimeKit/issues/762))
* Added dispose handling to MimeMessage.CreateFromMailMessage().
* Improved MIME structure returned by TnefPart.ConvertToMessage().
* Rewrote header folding logic to avoid string allocations.
* Implemented IEquatable&lt;T&gt; on TnefNameId.
* If iso-8859-1 isn't available, fall back to ASCII instead of Windows-1252.
  (issue [#751](https://github.com/jstedfast/MimeKit/issues/751))

Special Thanks to Jason Nelson for taking the lead on many of the listed (and unlisted) performance
improvements and helping me make MimeKit even more awesome!

## MimeKit 3.1.1 (2022-01-30)

* When initializing character encodings for netstandard and net50/net60, wrap the Reflection logic
  to invoke System.Text.Encoding.RegisterProvider() in a try/catch to prevents exceptions when
  using the netstandard version of MimeKit in a .NET Framework app.
  (issue [#751](https://github.com/jstedfast/MimeKit/issues/751))
* Added a work-around for Office365 `message/delivery-status` parts where all status groups after
  the first are base64 encoded. This seems to be a bug in Office365 where it treats the first
  status group as MIME entity and the following status groups as the content.
  (issue [#250](https://github.com/jstedfast/MimeKit/issues/250))
* Fixed the MimeMessage .ctor that takes object parameters to first check that a Message-Id
  header was not supplied before generating one for the message.
  (issue [#747](https://github.com/jstedfast/MimeKit/issues/747))
* Fixed the BestEncodingFilter logic such that if any line in binary content is > 998 and it contains
  nul bytes, it should recommend base64 (not quoted-printable).

## MimeKit 3.1.0 (2022-01-14)

* Always use a lowercase domain name in the Message-Id to work around bugs in eM Client.
  (issue [#734](https://github.com/jstedfast/MimeKit/issues/734))
* Improved handling of parsing Content-Types like "multipart/multipart/mixed; boundary=...".
  (issue [#737](https://github.com/jstedfast/MimeKit/issues/737))
* Added a maxLineLength argument to the QuotedPrintableEncoder .ctor.
* Added maxLineLength arguments to EncoderFilter.Create() methods.
* Fixed MimePart.Prepare() to remember the maxLineLength argument value for later use in the
  WriteTo() implementation. This maxLineLength value can then be passed to the Base64 or
  QuotedPrintable encoder so that it can properly limit lines to that length (up to a max of
  76 characters as per the specs). (issue [#743](https://github.com/jstedfast/MimeKit/issues/743))
* Added net6.0 to the list of TargetFrameworks.

## MimeKit 3.0.0 (2021-12-11)

* Removed APIs marked as \[Obsolete\] in 2.x.
* Refactored X509CertificateDatabase protected methods to include a DbConnection parameter.
* Removed OpenPgpContextBase by folding the logic into OpenPgpContext.
* Added Async APIs for OpenPGP and S/MIME.
* Lazy-load headers on MimeMessage and MimeEntity (and subclasses) to improve performance.
* Added a new MimeReader class that acts as a lower-level MimeParser alternative, allowing developers
  to parse MIME content without having to instantiate a MIME tree of objects or wait until the parser
  has completed (and returned a MimeMessage or MimeEntity object) before processing MIME data. This is
  conceptually similar to a SAX XML parser approach.
* Added a new ExperimentalMimeParser that duplicates MimeParser functionality, but is built on top of
  MimeReader. This implementation will eventually replace MimeParser once I get some feedback on it.
  Should be ~5% faster than MimeParser.
* Improved MimeParser performance slightly based on some of the experimentation done to make the
  ExperimentalMimeParser fast.
* Added CancellationToken arguments for some AttachmentCollection.Add() overloads.
* Use 'net5.0' as the .NET 5.0 target framework moniker instead of 'net50'.
  (issue [#720](https://github.com/jstedfast/MimeKit/issues/720))
* Drop support for .NET 4.5 and replace it with .NET 4.5.2
* Bumped Portable.BouncyCastle to 1.9.0
* Added new MimeMessage.GetRecipients() method.
* Make it possible to bypass MimeEntity preparation for signing by adding a PrepareBeforeSigning property to
  CryptographyContext. (issue [#721](https://github.com/jstedfast/MimeKit/issues/721))
* MimeMessage and MimeEntity now implement IDisposable.
  (issue [#732](https://github.com/jstedfast/MimeKit/issues/732))

## MimeKit 2.15.1 (2021-09-13)

* Improved MimeParser to be a little more efficient based on work being done for the upcoming v3.0 release.
* Fixed a bug in the MimeParser exposed by added unit tests regarding Content-Length handling.
* Improved address parser error messages.
* Fixed MailboxAddress.Address to be forgiving if there is trailing whitespace after the addr-spec token when
  setting MailboxAddress.Address. (issue [#705](https://github.com/jstedfast/MimeKit/issues/705))
* Fixed MimeMessage and MimeEntity.ToString() to not write a newline before the message/entity
  (regression introduced in 2.14.0).

## MimeKit 2.15.0 (2021-08-18)

* Use DebugType=full for .NET Framework v4.x. (MailKit issue [#1239](https://github.com/jstedfast/MailKit/issues/1239))
* Fixed bug in MultipartSigned.VerifyAsync() that would dispose of the crypto context before the async task was
  complete, resulting in an OperationCanceledException.
* Default to using the Environment.SpecialFolder.UserProfile directory instead of Personal when GNUPGHOME isn't defined
  in the environment. The Personal directory maps to the MyDocuments directory, so this wasn't correct. The .gnupg
  directory should be in the user's HOME directory.
* Added ContentType.ToString(bool encode) and ContentDisposition.ToString(bool encode) convenience methods.
* Changed the public Header.Parse/TryParse APIs to canonicalize header values to end with a newline even if the input
  string does not. (issue [#695](https://github.com/jstedfast/MimeKit/issues/695))

## MimeKit 2.14.0 (2021-07-28)

* Allow ..'s and trailing .'s in the local-part of an addr-spec by introducing a new RfcComplianceMode.Looser
  enum value that can be set on the ParserOptions.AddressParserComplianceMode property.
  (issue [#682](https://github.com/jstedfast/MimeKit/issues/682))
* Use Reflection to call Encoding.RegisterProvider() so that referencing the netstandard MimeKit assemblies
  from .NET 4.8 won't crash. (issue [#683](https://github.com/jstedfast/MimeKit/issues/683))
* Don't write the X-MimeKit warning header in ToString() anymore. This is a lost cause.
* Updated the OpenPgpContext to default to keys.openpgp.org since keys.gnupg.net does not resolve via DNS anymore.

## MimeKit 2.13.0 (2021-06-11)

* Added a way to force MimeKit to always quote parameter values.
  (issue [#674](https://github.com/jstedfast/MimeKit/issues/674))
* Fixed PGP/MIME signatures to use the proper BEGIN/END PGP SIGNATURE markers instead of
  BEGIN/END PGP MESSAGE markers by avoiding compressing the PGP signature packet.
  (issue [#681](https://github.com/jstedfast/MimeKit/issues/681))

## MimeKit 2.12.0 (2021-05-12)

* Fixed S/MIME support using WindowsSecureMimeContext with MimeKit's CmsSigner classes which was
  causing a PlatformNotSupportedException.
  (issue [#664](https://github.com/jstedfast/MimeKit/issues/664))
* When extracting HTML from TNEF, try to use the charset delcared in the HTML's meta tags.
  (issue [#667](https://github.com/jstedfast/MimeKit/issues/667))
* Added AttachmentCollection.AddAsync() methods.
  (issue [#670](https://github.com/jstedfast/MimeKit/issues/670))
* Enable SqliteCertificateDatabase initialization logic for .NET v5.0.
  (issue [#673](https://github.com/jstedfast/MimeKit/issues/673))

## MimeKit 2.11.0 (2021-03-12)

* Fixed DSA key conversion logic to work more reliably.
* Catch exceptions from IPGlobalProperties.GetIPGlobalProperties() and fall back to localhost.
  (issue [#630](https://github.com/jstedfast/MimeKit/issues/630))
* Fixed base64 encoder to only flush with a newline if it is midline.
  (issue [#646](https://github.com/jstedfast/MimeKit/issues/646))
* Added .NET 5 build targets and include in the nuget packages.
* Added a new GnuPGContext .ctor that allows specifying a custom path.
* Bumped Portable.BouncyCastle dependency to 1.8.10.
* Improved HtmlWriter ArgumentException messages.

## MimeKit 2.10.1 (2020-12-05)

* Treat message/disposition-notification and message/delivery-status the same as text/*
  when preparing for signing. (issue [#626](https://github.com/jstedfast/MimeKit/issues/626))
* Always set Content-Disposition: inline for BodyBuilder.LinkedResources. This fixes a
  regression introduced in 2.10.0.
  (issue [#627](https://github.com/jstedfast/MimeKit/issues/627))
* Fixed NuGet package references to System.Data.DataSetExtensions for netstandard2.1 and
  net4x.

## MimeKit 2.10.0 (2020-11-20)

* Added SQL Server support. (issue [#619](https://github.com/jstedfast/MimeKit/issues/619))
* Fixed a leak in SqlCertificateDatabase when creating the certificates database.
* Bumped BouncyCastle dependency to v1.8.8. (issue [#610](https://github.com/jstedfast/MimeKit/issues/610))
* Exposed some ArcVerifier and DkimVerifier internal methods.
  (issue [#601](https://github.com/jstedfast/MimeKit/issues/601))
* Improved MimeParser performance.
* Fixed potential leaks in MimeParser when loading MimePart content in exception cases.
* Made use of ArrayPools for various buffers which may help performance.
  (issue [#616](https://github.com/jstedfast/MimeKit/issues/616))
* Fixed MimeUtils.GenerateMessageId() to encode international domain names.
* Fixed MimeUtils.GenerateMessageId() to cache the local hostname.
  (issue [#612](https://github.com/jstedfast/MimeKit/issues/612))
* Modified AttachmentCollection to use a custom implementation of Path.GetFileName()
  that allows illegal path characters.
* Only generate a ContentId for the MultipartRelated Root if it is not the first part.

## MimeKit 2.9.2 (2020-09-12)

* Include WindowsSecureMimeContext in the .NET Standard 2.x build.
  (issue [#600](https://github.com/jstedfast/MimeKit/issues/600))
* Fixed message.Prepare() to never choose the quoted-printable encoding
  for non-text based MimeParts.
  (issue [#598](https://github.com/jstedfast/MimeKit/issues/598))
* Added work-around for mailers that don't use a ';' between Content-Type
  and Content-Disposition parameters.
  (issue [#595](https://github.com/jstedfast/MimeKit/issues/595))
* Added improved error reporting for ArcVerifier.
  (issue [#591](https://github.com/jstedfast/MimeKit/issues/591))
* Added another work-around for parsing Authentication-Results headers.
  (issue [#590](https://github.com/jstedfast/MimeKit/issues/590))
* MimeMessage.ToString() now adds an X-MimeKit-Warning header to the
  beginning of the output string to make it clear to developers doing this
  that they are Doing it Wrong(tm).
* Added a TLS-Required HeaderId enum value.

## MimeKit 2.9.1 (2020-07-11)

* Refactored OpenPgpContext to separate out key storage implementation.
  (issue [#576](https://github.com/jstedfast/MimeKit/issues/576))
* Fixed the TextToFlowed converter.
  (issue [#580](https://github.com/jstedfast/MimeKit/issues/580))
* Protect against ABRs in AuthenticationResults.TryParse().
  (issue [#581](https://github.com/jstedfast/MimeKit/issues/581))
* The net45 version of MimeKit now depends on Portable.BouncyCastle instead of official
  BouncyCastle.
* Added MimeParser events to report stream offsets for MimeMessages and MimeEntities.
  (issue [#582](https://github.com/jstedfast/MimeKit/issues/582))
* Fixed DkimPublicKeyLocatorBase to treat unspecified 'k' values in DKIM DNS records as
  "k=rsa".
  (issue [#583](https://github.com/jstedfast/MimeKit/issues/583))
* Fixed date format serializer to use CultureInfo.InvariantCulture.
* Fixed AuthenticationResults parser to allow '_' characters in method results.
  (issue [#584](https://github.com/jstedfast/MimeKit/issues/584))
* Improved RSACng and DSACng support.

## MimeKit 2.8.0 (2020-05-30)

* Improved logic for verifying signatures for MimeParts containing mixed line endings.
  (issue [#569](https://github.com/jstedfast/MimeKit/issues/569))
* Fixed MailboxAddress parser to decode IDN-encoded local-parts of email addresses.
  (MailKit issue [#1026](https://github.com/jstedfast/MailKit/issues/1026))
* Added new MailboxAddress.GetAddress(bool idnEncode) method.
* Improved subclassability of OpenPgpContext by making a number of methods virtual.
  (issue [#571](https://github.com/jstedfast/MimeKit/issues/571))
* Added support for RSACng and DSACng.
  (issue [#567](https://github.com/jstedfast/MimeKit/issues/567))
* Dropped Xamarin platforms since they are compatible with netstandard2.0.

## MimeKit 2.7.0 (2020-05-19)

* Fixed InternetAddressList.Insert() to allow inserting at the end of the list.
  (issue [#559](https://github.com/jstedfast/MimeKit/issues/559))
* Added ParserOptions.MaxMimeDepth to allow developers to set the max nesting depth
  allowed by the parser.
* Added logic to handle multipart children without any headers or content.
* Added a new Verify(bool verifySignatureOnly) method to IDigitalSignature for
  developers who just want to be able to verify the signature without worrying
  about the certificate chain.
* Fixed MimePart.WriteTo() to avoid canonicalizing line endings for MimeParts that
  do not define a Content-Transfer-Encoding.
  (issue [#569](https://github.com/jstedfast/MimeKit/issues/569))
* NuGet packages now include the portable pdb's.

## MimeKit 2.6.0 (2020-04-03)

* Fixed the MimeEntity.ContentId setter to use ParseUtils.TryParseMsgId() instead of
  MailboxAddress.TryParse() so that it is more lenient in what it accepts.
  (issue [#542](https://github.com/jstedfast/MimeKit/issues/542))
* Added an HtmlTokenizer.IgnoreTruncatedTags property which is useful when working with
  truncated HTML.
* Optimized the heck out of HtmlEntityDecoder.
* Added a TextPart.Format property for a quick way to determine the type of text it
  contains.
* Added text/plain and text/html preview/snippet generators (PlainTextPreviewer and
  HtmlTextPreviewer, respectively). This is part of a larger improvement to MailKit's
  text preview feature for IMAP.
  (MailKit issue [#1001](https://github.com/jstedfast/MailKit/issues/1001))
* Fixed SqlCertificateDatabase to accept null SubjectKeyIdentifiers.
* Changed Header.FormatRawValue() to be protected virtual and added Header.SetRawValue()
  to allow developers to override the default formatting behavior by either subclassing
  Header or by calling header.SetRawValue().
  (issue [#546](https://github.com/jstedfast/MimeKit/issues/546))
* Switched MimeKit for Android and iOS over to using Portable.BouncyCastle.
* Added MimeTypes.Register() to allow developers to register their own mime-type mappings
  to file extensions.

## MimeKit 2.5.2 (2020-03-14)

* Updated net46, net47, and net48 builds to reference Portable.BouncyCastle instead of
  the standard BouncyCastle package, just like the netstandard builds.
  (issue [#540](https://github.com/jstedfast/MimeKit/issues/540))
* Fixed extraction of TNEF EmbeddedMessage attachment data to skip the leading GUID.
  (issue [#538](https://github.com/jstedfast/MimeKit/issues/538))
* Added a few more TNEF property tags.
* Fixed the HtmlEntityDecoder to require some named attributes to end with a `;`.

## MimeKit 2.5.1 (2020-02-15)

* Fixed parsing of email addresses containing unicode or other types of 8-bit text.
  (issue [#536](https://github.com/jstedfast/MimeKit/issues/536))
* Added a MimeTypes.TryGetExtension() method to try and get a file name extension
  based on a mime-type.
  (issue [#534](https://github.com/jstedfast/MimeKit/issues/534))
* Updated mime-type mappings.

## MimeKit 2.5.0 (2020-01-18)

* Fixed message reserialization after prepending headers.
  (issue [#524](https://github.com/jstedfast/MimeKit/issues/524))
* Added a ContentType.CharsetEncoding property.
  (issue [#526](https://github.com/jstedfast/MimeKit/issues/526))
* Allow empty prop-spec token values in Authentication-Results headers.
  (issue [#527](https://github.com/jstedfast/MimeKit/issues/527))
* Added logic to quote Authentication-Results pvalue tokens if needed.
* Added support for converting RSACng keys into BouncyCastle keys for
  net4x versions that support it.
* Added support for RSAES-OAEP for the BouncyCastle backend.
  (issue [#528](https://github.com/jstedfast/MimeKit/issues/528))
* Updated and changed the API for RSASSA-PSS. CmsSigner now has a
  RsaSignaturePadding property which obsoletes the previous
  RsaSignaturePaddingScheme property.
* Added more columns to the default SQLite database CERTIFICATES table
  that allow more optimal SQL searches for certificates given various
  matching criteria.
* Fixed WindowsSecureMimeContext.Decrypt() to make sure it doesn't stop
  at the first failed recipient.
  (issue [#530](https://github.com/jstedfast/MimeKit/issues/530))
* Fixed splitting and reassembly of message/partial messages.
* Improved handling of Office365 Authentication-Results headers by adding
  a Office365AuthenticationServiceIdentifier property to the
  AuthenticationMethodResult class.
* Fixed mailbox address parser to be more lenient about `"["` and `"]"`
  characters in the display-name.
  (issue [#532](https://github.com/jstedfast/MimeKit/issues/532))

## MimeKit 2.4.1 (2019-11-10)

* Don't use PublicSign on non-Windows NT machines when building.
  (issue [#516](https://github.com/jstedfast/MimeKit/issues/516))
* Improved BouncyCastleSecureMimeContext logic for building certificate chains so that
  certificate chains are included in the S/MIME signature.
  (issue [#515](https://github.com/jstedfast/MimeKit/issues/515))
* Improved SqlCertificateDatabase.Find() by using more IX509Selector properties.
* Relaxed the Authentication-Results header parser a bit to allow '/' in pvalue tokens.
  (issue [#518](https://github.com/jstedfast/MimeKit/issues/518))

## MimeKit 2.4.0 (2019-11-02)

* Added the `text/csv` mime-type to the `MimeTypes` mapping table for files with a .csv extension.
* Expanded the .NETStandard API to match the .NET 4.5 API, so .NETStandard is now complete.
* Dropped support for .NETPortable and WindowsPhone/Universal v8.1.
* Added a net48 assembly to the NuGet package.
* Improved HTML tokenizer performance.
* Fixed X509Crl.IsDelta for CRLs without extensions.
  (issue [#513](https://github.com/jstedfast/MimeKit/issues/513))
* Added support for `message/global-delivery-status`, `message/global-disposition-notification`,
  and `message/global-headers` to `MimeParser`.
  (issue [#514](https://github.com/jstedfast/MimeKit/issues/514))
* Fixed S/MIME signatures generated by a TemporarySecureMimeContext to include the certificate chain.
  (issue [#515](https://github.com/jstedfast/MimeKit/issues/515))

## MimeKit 2.3.2 (2019-10-12)

* Fixed reserialization of message/rfc822 parts to not add an extra new-line sequence
  to the end of the message. (issue [#510](https://github.com/jstedfast/MimeKit/issues/510))
* Fixed DefaultSecureMimeContext to build the cert chain outside of the private key query.
  (issue [#508](https://github.com/jstedfast/MimeKit/issues/508))
* Modified the Message-Id parser to gobble ctrl chars in the local-part.
* Fixed some buglets in the TextToFlowed converter involving space-stuffing lines.
* Fixed BodyBuilder logic for constructing a body with an HtmlBody set to string.Empty.
  (issue [#506](https://github.com/jstedfast/MimeKit/issues/506))
* Fixed potential memory leaks in WindowsSecureMimeContext and BouncyCastleSecureMimeContext
  in the Export() methods in cases where an exception is throw while adding certificates.
* Removed MimeKit.Cryptography.NpgsqlCertificateDatabase. It is unlikely anyone actually
  uses this.

## MimeKit 2.3.1 (2019-09-08)

* Updated CmsSigner's default DigestAlgorithm to Sha256 instead of Sha1 to match
  System.Security.Cryptography.Pkcs.CmsSigner's default.
* Updated WindowsSecureMimeContext to default to IssuerAndSerialNumber for
  System.Security.Cryptography.Pkcs.CmsSigner.
* Added support for the RSASSA-PSS signature padding algorithm when using the
  BouncyCastle backend.
* Improved robustness of TNEF processing of email address fields.
* Modified FilteredStream.Flush*() to not flush the source stream.
  (MailKit issue [#904](https://github.com/jstedfast/MailKit/issues/904))
* Added net46 and net47 assemblies to the NuGet package.

## MimeKit 2.3.0 (2019-08-24)

* Fixed MultipartRelated to fall back to the multipart/related type parameter when
  locating the Root. (issue [#489](https://github.com/jstedfast/MimeKit/issues/489))
* Improved Authentication-Results parser to handle non-standard syntax.
  (issue [#490](https://github.com/jstedfast/MimeKit/issues/490))
* When FormatOptions.AllowMixedHeaderCharsets is disabled, always use the user-specified
  charset. Previously this could/would still use us-ascii and/or iso-8859-1 if the entire
  header could fit within one of those charsets.
  (issue [#493](https://github.com/jstedfast/MimeKit/issues/493))
* Fixed the line length calculations in the BestEncodingFilter.
  (issue [#497](https://github.com/jstedfast/MimeKit/issues/497))
* Fixed Multipart to properly ensure the epilogue ends w/ a new-line when
  FormatOptions.EnsureNewLine is true.
  (issue [#499](https://github.com/jstedfast/MimeKit/issues/499))
* Modified Multipart.WriteTo[Async] to not ensure that a Content-Type boundary parameter
  has been set. This code-path was only hit if the multipart was parsed by the parser and
  did not have a boundary parameter in the first place. In the interest of preserving
  byte-for-byte compatibility with the original input, this sanity check has been removed.
  (issue [#499](https://github.com/jstedfast/MimeKit/issues/499))

## MimeKit 2.2.0 (2019-06-11)

* Added support for [ARC](https://arc-spec.org).
* Added AuthenticationResults class for parsing and constructing Authentication-Results and
  ARC-Authentication-Results headers.
* Added support for the Ed25519-SHA256 DKIM signature algorithm.
* Obsoleted MimeMessage DKIM API's in favor of the newer DKIM API's:
  * MimeMessage.Sign (DkimSigner, ...) has been replaced by DkimSigner.Sign (MimeMessage, ...).
  * MimeMessage.Verify (Header, ...) has been replaced by DkimVerifier.Verify (MimeMessage, Header, ...).
* Added DkimPublicKeyLocatorBase to help simplify implementing IDkimPublicKeyLocator.

## MimeKit 2.1.5 (2019-05-13)

* Updated the BouncyCastle assemblies to version 1.8.5 for iOS and Android.
* Fixed a possible NullReferenceException when decoding S/MIME digital signatures.
* Fixed the netstandard2.0 dependencies to no longer explicitly include System.Net.Http.
  (issue [#482](https://github.com/jstedfast/MimeKit/issues/482))
* Override Equals(object) and GetHashCode() for InternetAddress and InternetAddressList.
  (issue [#481](https://github.com/jstedfast/MimeKit/issues/481))
* Fixed TnefReader.Dispose() to avoid a potential NullReferenceException if double disposed.
* Fixed the Message-Id, Content-Id, References and In-Reply-To parsers to be more liberal
  in what they accept in terms of the `msg-id` token.
* Changed the Header encoding logic for the In-Reply-To header to not rfc2047 encode the value
  even if it is longer than the suggested line-length.
  (issue [#479](https://github.com/jstedfast/MimeKit/issues/479))
* Reduced netstandard dependencies. (issue [#475](https://github.com/jstedfast/MimeKit/issues/475))

## MimeKit 2.1.4 (2019-04-13)

* Added a setter for FormatOptions.MaxLineLength, allowing developers to override this value.
* Improved TNEF handling of Content-Disposition and Content-Id properties.
  (issue [#470](https://github.com/jstedfast/MimeKit/pull/470) and
  issue [#471](https://github.com/jstedfast/MimeKit/pull/471))
* Improved Content-Id parser to be more forgiving with improperly formatted IDs.
  (issue [#472](https://github.com/jstedfast/MimeKit/issue/472))
* Added support for the text/rfc822-headers MIME-type via the new TextRfc822Headers class.
  (issue [#474](https://github.com/jstedfast/MimeKit/issue/474))
* Added fallback logic for international email addresses that are not properly encoded in UTF-8.
  (issue [#477](https://github.com/jstedfast/MimeKit/issue/477))

## MimeKit 2.1.3 (2019-02-24)

* Fixed an NRE in X509CertificateDatabase.Dispose().
* Fixed TextPart.Text and GetText() to properly canonicalize EOLN for multi-byte charsets
  such as UTF-16. (issue [#442](https://github.com/jstedfast/MimeKit/issues/442))
* Fixed System.Net.Mail.MailMessage cast to MimeMessage when the ContentStream of
  the attachments has not been rewound to the beginning of the stream.
  (issue [#467](https://github.com/jstedfast/MimeKit/issues/467))
* Changed ParserOptions.AllowAddressesWithoutDomain to work as users expected and
  moved the old logic into ParserOptions.AllowUnquotedCommasInAddresses.
  (issue [#465](https://github.com/jstedfast/MimeKit/issues/465))

## MimeKit 2.1.2 (2018-12-30)

* Fixed WindowsSecureMimeDigitalCertificate logic for ECDsa.
* Added X509Certificate.GetPublicKeyAlgorithm() extension method.
* Modified ApplicationPkcs7Mime to be less strict about the smime-type.

## MimeKit 2.1.1 (2018-12-16)

* Mapped the TNEF Sensitivity property to the Sensitivity message header when calling
  TnefPart.ConvertToMessage().
* Fixed the TNEF Importance and Priority mappings when calling TnefPart.ConvertToMessage().
* Added more TnefPropertyId's that have been identified.
* Map PidTagTnefCorrelationKey to the Message-Id message header.
* When the TNEF data does not have a SentDate property, set the MimeMessage.Date property
  to DateTimeOffset.MinValue instead of DateTimeOffset.Now.
* Fixed TnefPart.ConvertToMessage() to check the TNEF SubjectPrefix and NormalizedSubject
  properties and use them if a TNEF Subject property is not available.
* Fixed TNEF logic for extracting attachment content to not truncate some bytes from the beginning
  of the content.
* Added more fallbacks for attempting to extract the sender information out of the TNEF data.
* Bumped Android and iOS versions of BouncyCastle to v1.8.4.

## MimeKit 2.1.0 (2018-12-01)

* Optimized SecureMimeCryptographyContext.Supports() and OpenPgpCryptographyContext.Supports()
  implementations.
* Optimized the OptimizedOrdinalIgnoreCaseComparer even more.
* Fixed OpenPgpDigitalCertificate.ExpirationDate for PGP keys that never expire.
* Reduced string allocations in MultipartSigned.Verify() and MultipartEncrypted.Decrypt().
* Fixed OpenPgpContext.Decrypt() to make sure to always clean up MemoryBlockStreams.
* Added a bunch more HeaderId enum values.
* Improved header folding logic for headers with long words.
  (issue [#451](https://github.com/jstedfast/MimeKit/issues/451))

## MimeKit 2.0.7 (2018-10-28)

* Fixed a bug in the UUEncoder.
* Fixed a bug in MimeIterator.MoveTo().
* Modified BodyBuilder.ToMessageBody() to avoid returning a multipart/mixed with only a single
  child. (issue [#441](https://github.com/jstedfast/MimeKit/issues/441))
* Modified TnefPart to no longer set the name parameter on the Content-Type header of
  extracted message bodies. (issue [#435](https://github.com/jstedfast/MimeKit/issues/435))
* Fixed various locations that loaded content from files to use FileShare.Read so as to avoid file
  sharing violations if the application already has that file opened elsewhere. (issue [#426](https://github.com/jstedfast/MimeKit/issues/426))
* Improved address parser to handle "local-part (User Name)" style addresses.
* Updated the iOS and Android BouncyCastle dependency to 1.8.3.
* Modified TextPart.Text and GetText() to canonicalize the newlines. (issue [#442](https://github.com/jstedfast/MimeKit/issues/442))
* Fixed WindowsSecureMimeContext.EncapsulatedSign (CmsSigner, ...) and Sign (CmsSigner, ...).
* Added SecureMimeContext.Import(string, string) to import passworded pk12 files.
* Improved MimeParser's support of Content-Length.
* Fixed MimeParser.ParseEntity() and MimeEntity.Load() to throw a FormatException if the
  stream does not have properly formatted headers. (issue [#443](https://github.com/jstedfast/MimeKit/issues/443))
* Added support for message/global.

## MimeKit 2.0.6 (2018-08-04)

* Added more bounds checking for parsing mailbox addresses to fix IndexOutOfRangeExceptions
  given an incomplete address like "Name <". (issue [#421](https://github.com/jstedfast/MimeKit/issues/421))
* Fixed support for parsing mbox files using Content-Length.
* Modified the TextPart.Text getter property to check for a UTF-16 BOM and use an appropriate
  UTF-16 System.Text.Encoding if found instead of simply assuming UTF-8 and falling back to
  iso-8859-1. (issue [#417](https://github.com/jstedfast/MimeKit/issues/417))
* Minor optimizations.

## MimeKit 2.0.5 (2018-07-07)

* Make sure messages created from System.Net.Mail.MailMessages have a Date header. (MailKit issue [#710](https://github.com/jstedfast/MailKit/issues/710))
* Allow developers to pass in their own SecureRandom when generating PGP key pairs. (issue [#404](https://github.com/jstedfast/MimeKit/issues/404))
* Modified MemoryBlockStream to use a shared buffer pool to relieve pressure on the GC. (MailKit issue [#725](https://github.com/jstedfast/MailKit/issues/725))

## MimeKit 2.0.4 (2018-05-21)

* The default value of the `CheckCertificateRevocation` property located on the `BouncyCastleSecureMimeContext` has been changed
  to `false` due to privacy concerns noted in the [Efail](https://efail.de) document published in May of 2018. Clients that wish
  to continue automatic downloads of S/MIME CRLs can manually set the property to `true`.
* Properly wrap long mailbox names with quoted phrases.
* Fixed parsing of header blocks that span across read boundaries. (issue [#395](https://github.com/jstedfast/MimeKit/issues/395))
* Added FormatOptions.EnsureNewLine property (MailKit issue [#251](https://github.com/jstedfast/MailKit/issues/251))
* Enable System.Net.Mail support for .NET Core 2.0. (issue [#393](https://github.com/jstedfast/MimeKit/issues/393))

## MimeKit 2.0.3 (2018-04-15)

* Allow empty TextBody and HtmlBody properties for BodyBuilder. (issue [#391](https://github.com/jstedfast/MimeKit/issues/391))
* Fixed BodyBuilder.Attachments.Add() to properly handle message/rfc822 attachments.
* Fixed HTML entity encoder logic when a surrogate pair is at the end of the input. (issue [#385](https://github.com/jstedfast/MimeKit/issues/385))

## MimeKit 2.0.2 (2018-03-18)

* IDN encode/decode the local part of mailbox addresses as well. (MailKit issue [#649](https://github.com/jstedfast/MailKit/issues/649))
* Added a record for .epub to the MimeTypes database. (issue [#376](https://github.com/jstedfast/MimeKit/issues/376))
* Explicitly pass 'false' as the silent argument to SignedCms.ComputeSignature(). (issue [#374](https://github.com/jstedfast/MimeKit/issues/374))
* Make sure the MimeParser does not hang if the last header line is truncated before CRLF.
* Don't use Encoder/DecoderExceptionFallbacks in the TNEF reader. (issue [#370](https://github.com/jstedfast/MimeKit/issues/370))
* Provide a better error message when the cert within a pkcs12 cannot digital sign. (issue [#367](https://github.com/jstedfast/MimeKit/issues/367))
* Fixed TemporarySecureMimeContext to key off the certificate's fingerprint.

## MimeKit 2.0.1 (2018-01-06)

* Improved the HTML parser logic to better handle a number of edge cases.
* MimeKit will now automatically download CRLs based on the CRL Distribution Point
  certificate extension if any HTTP URLs are defined (LDAP and FTP are not yet supported)
  when verifying S/MIME digital signatures using a derivative of the
  BouncyCastleSecureMimeContext backend (the WindowsSecureMimeContext gets this for free
  from System.Security's CMS implementation).
* Fixed OpenPgpContext.RetrievePublicKeyRingAsync() to use the filtered stream.
* Added support for using the Blowfish encryption algorithm with S/MIME (only supported
  in the BouncyCastle backends).
* Added support for using the SEED encryption algorithm with S/MIME (also only supported
  in the BouncyCastle backends).
* Added an optional 'algorithm' argument to OpenPgpContext.GenerateKeyPair() to allow
  specifying the symmetric key algorithm to use in generating the key pair. This defaults
  to AES-256, which is the same value used in older versions of MimeKit.

## MimeKit 2.0.0 (2017-12-22)

* Added IDkimPublicKeyLocator.LookupPublicKeyAsync() and MimeMessage.VerifyAsync() to support
  asynchronous DNS lookups of DKIM public keys.
* Fixed tokenization of unquoted HTML attributes containing entities.
* Vastly improved the WindowsSecureMimeContext to do everything using System.Security
  instead of a mix of System.Security and Bouncy Castle.
* Refactored SecureMimeContext into a base SecureMimeContext and a
  BouncyCastleSecureMimeContext that contained all of the Bouncy Castle-specific logic.
* Added useful extension methods to facilitate conversion between System.Security and
  Bouncy Castle crypto types (such as X509Certificates and AsymmetricAlgorithms).
* Renamed the IContentObject interface to IMimeContent.
* Renamed the ContentObject class to MimeContent.
* Renamed the MimePart.ContentObject property to MimePart.Content.
* Dropped support for .NET 3.5 and .NET 4.0.

## MimeKit 1.22.0 (2017-11-24)

* Fixed a buffering bug in MimeParser's header parser. (issue [#358](https://github.com/jstedfast/MimeKit/issues/358))
* Set the TnefReader charset on extracted text/plain and text/html bodies. (issue [#357](https://github.com/jstedfast/MimeKit/issues/357))
* Added safeguard to protect against malformed nested group addresses which could cause
  a stack overflow in the parser. ParserOptions now has a way of limiting the recursive
  depth of rfc822 group addresses using the MaxAddressGroupDepth property. (issue [#355](https://github.com/jstedfast/MimeKit/issues/355))
* Fixed the S/MIME certificate database for .NETStandard by using GetFieldValue() instead
  of GetBytes() which is not supported on .NETStandard. (issue [#351](https://github.com/jstedfast/MimeKit/issues/351))

## MimeKit 1.20.0 (2017-10-28)

* Added async support for writing MimeMessage, MimeEntity, HeaderList and ContentObject.
* Added async support for parsing MimeMessage, MimeEntity, and HeaderList.
* Added async support to MimeKit.IO streams.
* Removed methods marked [Obsolete] (which have been marked obsolete for several years now).
* Improved performance of writing messages by a small amount.
* Fixed SecureMimeDigitalSignature to capture the signature digest algorithm used by the sending
  client. (issue [#341](https://github.com/jstedfast/MimeKit/issues/341))
* Fixed the S/MIME decoder to correctly determine the RC2 algorithm used by the sending client.
  (issue [#337](https://github.com/jstedfast/MimeKit/issues/337))
* Fixed a bug in BoundStream.Seek().

## MimeKit 1.18.1 (2017-09-03)

* Added CanSign() and CanEncrypt() methods to CryptographyContext for checking
  whether or not a mailbox can be used for signing or be encrypted to. (issue [#325](https://github.com/jstedfast/MimeKit/issues/325))
* Automatically register the CodePagesEncodingProvider when running on .NETStandard. (issue [#330](https://github.com/jstedfast/MimeKit/issues/330))
* Fixed MimeMessage.TextBody to return null when the top-level MIME part is a TextPart
  marked as an attachment.
* Fixed the HtmlToHtml converter to suppress comments if the HtmlTagContext's SuppressInnerContent
  property is active (even if FilterComments is false).
* Documented OpenPgpContext.GenerateKeyPair() which was added in 1.18.0.
* Added OpenPgpContext.Delete() methods to delete public and secret keyrings.
* Added OpenPgpContext.SignKey().
* Remove "Version:" header from armored OpenPGP output. (issue [#319](https://github.com/jstedfast/MimeKit/issues/319))

## MimeKit 1.18.0 (2017-08-07)

* Allow importing of known PGP keys (needed when re-importing keys after signing them). (issue [#315](https://github.com/jstedfast/MimeKit/issues/315))
* Added APIs to enumerate public and secret PGP keys.
* Added an OpenPgpDetectionFilter to detect OpenPGP blocks and their stream offsets.
* Added a MimeMessage.WriteTo() overload that takes a bool headersOnly argument.
* Pushed SecureMimeContext's EncryptionAlgorithm preferences down into CryptographyContext.
* Updated GnuPGContext to load algorithm preferences from gpg.conf.
* Fixed TemporarySecureMimeContext to use the fingerprint in the certificate lookup methods
  when the MailboxAddress argument is a SecureMailboxAddress. (issue [#322](https://github.com/jstedfast/MimeKit/issues/322))
* Fall back to using the Subject Alternative Rfc822 Name if the SubjectEmailAddress fails. (issue [#323](https://github.com/jstedfast/MimeKit/issues/323))

## MimeKit 1.16.2 (2017-07-01)

* Fixed a bug in the MailMessage to MimeMessage conversion which corrupted the Subject string. (issue [#306](https://github.com/jstedfast/MimeKit/issues/306))
* If no KeyUsage extension exists for an X509 certificate, assume no restrictions on key usage.
* Throw an exception if there is a problem building an X509 certificate chain when verifying
  S/MIME signatures.

## MimeKit 1.16.1 (2017-05-05)

* Fixed TextToHtml and FlowedToHtml's OutputHtmlFragment property to work.
* Fixed EncodeAddrspec and DecodeAddrspec to handle string.Empty. (issue [#302](https://github.com/jstedfast/MimeKit/issues/302))
* Allow string.Empty as a valid addrspec for MailboxAddress. (issue [#302](https://github.com/jstedfast/MimeKit/issues/302))
* Catch exceptions trying to import CRLs and Certs when verifying S/MIME signatures. (issue [#304](https://github.com/jstedfast/MimeKit/issues/304))

## MimeKit 1.16.0 (2017-04-21)

* Added new ParserOptions option to allow local-only mailbox addresses (e.g. no @domain).
* Improved address parser to interpret unquoted names containing commas in email addresses
  as all part of the same name/email address instead of as a separate email address.
* Greatly improved the WindowsSecureMimeContext backend.
* A number of fixes to bugs exposed by an ever-increasing set of unit tests (up to 87% coverage).

## MimeKit 1.14.0 (2017-04-09)

* Added International Domain Name support for email addresses.
* Added a work-around for mailers that didn't provide a disposition value in a
  Content-Disposition header.
* Added a work-around for mailers that quote the disposition value in a Content-Disposition
  header.
* Added automatic key retrieval functionality for the GnuPG crypto context.
* Added a virtual DigestSigner property to DkimSigner so that consumers can hook into services
  such as Azure. (issue [#296](https://github.com/jstedfast/MimeKit/issues/296))
* Fixed a bug in the MimeFilterBase.SaveRemainingInput() logic.
* Preserve munged From-lines at the start of message/rfc822 parts.
* Map code page 50220 to iso-2022-jp.
* Format Reply-To and Sender headers as address headers when using Header.SetValue().
* Fixed MimeMessage.CreateFromMailMessage() to set MimeVersion. (issue [#290](https://github.com/jstedfast/MimeKit/issues/290))

## MimeKit 1.12.0 (2017-03-12)

* Added new DKIM MimeMessage.Sign() methods that take an IList&lt;string&gt; of header field names
  to sign.
* Improved the address parser to allow the lack of a terminating ';' character at the end of
  group addresses.
* Improved the address parser to unquoted ',' and '.' characters in the name component of
  mailbox and group addresses.
* Added support for CryptographyContext factories by adding new Register() methods that
  take function callbacks that return a SecureMimeContext or OpenPgpContext. Thanks to
  Christoph Enzmann for this feature. (issue [#283](https://github.com/jstedfast/MimeKit/issues/283))
* Fixed DefaultSecureMimeContext..cctor() to not call Directory.CreateDirectory() on
  the default database directory. Instead, let the .ctor() create it instead if and when
  an instance of the DefaultSecureMimeContext is created. (issue [#285](https://github.com/jstedfast/MimeKit/issues/285))
* Store DBNull in S/MIME SQL backends for null values (SQLite handles `null` but
  databases such as Postgres do not). (issue [#286](https://github.com/jstedfast/MimeKit/issues/286))

## MimeKit 1.10.1 (2017-01-28)

* Fixed the Content-Type and Content-Disposition parameter parser to remove trailing lwsp from
  unquoted parameter values. (issue [#278](https://github.com/jstedfast/MimeKit/issues/278))
* Fixed MimePart.WriteTo() to not necessarily force the content to end with a new-line.

## MimeKit 1.10.0 (2016-10-31)

* Fixed OpenPgpContext.Verify() to throw FormatException if no data packets found.
* Added new MailboxAddress constructors that do not take a 'name' argument. (issue [#267](https://github.com/jstedfast/MimeKit/issues/267))
* Added an HtmlToHtml.FilterComments property to remove comments. (issue [#271](https://github.com/jstedfast/MimeKit/issues/271))
* Modified address parser to handle invalid addresses like "user@example.com <user@example.com>".

## MimeKit 1.8.0 (2016-09-25)

* Improved parsing of malformed mailbox addresses.
* Added DecompressTo() and DecryptTo() methods to SecureMimeContext.
* Fixed MessagePartial.Split().

## MimeKit 1.6.0 (2016-09-11)

* Use RandomNumberGenerator.Create() for .NET Core instead of System.Random when generating
  multipart boundaries.

## MimeKit 1.4.2 (2016-08-14)

* Strong-name the .NET Core assemblies.
* Fixed logic for selecting certificates from the Windows X.509 Store. (issue [#262](https://github.com/jstedfast/MimeKit/issues/262))

## MimeKit 1.4.1 (2016-07-17)

* Fixed QuotedPrintableDecoder to handle soft breaks that fall on a buffer boundary.
* Fixed MimeMessage.WriteTo() to properly respect the FormatOptions when writing the
  message headers.
* Updated TextFormat to contain a Plain value (Text is now an alias) to hopefully make
  its mapping to text/plain more obvious.
* Added new TextPart .ctor that takes a TextFormat argument so that developers that
  don't understand mime-types can more easily intuit what that argument should be.

## MimeKit 1.4.0 (2016-07-01)

* Added support for .NET Core 1.0
* Changed the default value of FormatOptions.AllowMixedHeaderCharsets to false.
* Added a new DkimSigner .ctor that takes a stream of key data. (issue [#255](https://github.com/jstedfast/MimeKit/issues/255))

## MimeKit 1.2.25 (2016-06-16)

* Fixed parsing bugs in MessageDeliveryStatus.StatusGroups. (issue [#253](https://github.com/jstedfast/MimeKit/issues/253))
* Fixed MimeParser.ParseHeaders() to handle header blocks that do not end with a blank line. (issue [#250](https://github.com/jstedfast/MimeKit/issues/250))
* Fixed the MailboxAddress parser to handle whitespace between '<' and the addr-spec.
* Fixed TemporarySecureMimeContext to handle certificates with null email addresses. (issue [#252](https://github.com/jstedfast/MimeKit/issues/252))

## MimeKit 1.2.24 (2016-05-22)

* Modified MimeMessage .ctor to not add an empty To: header by default. (issue [#241](https://github.com/jstedfast/MimeKit/issues/241))
* Modified MimeMessage to remove address headers when all addresses in that field are removed.
* Properly apply SecurityCriticalAttribute to GetObjectData() on custom Exceptions.
* Fixed TnefPropertyReader to convert APPTIME values into DateTimes from the OLE Automation
  Date format. (issue [#245](https://github.com/jstedfast/MimeKit/issues/245))

## MimeKit 1.2.23 (2016-05-07)

* Modified ParamaterList.TryParse() to handle quoted rfc2231-encoded param values. (issue [#239](https://github.com/jstedfast/MimeKit/issues/239))
* Updated to reference BouncyCastle via NuGet packages rather than bundling the assemblies.
* Fixed MimeParser to set a multipart's raw epilogue to null instead of an empty byte array.
  Fixes some issues with digital signature verification (as well as DKIM verification).
* Added an HtmlWriter.WriteText() override with Console.WriteLine() style params.
* Added convenience MimeMessage property for the X-Priority header.
* Fixed MimeMessage.ConvertFromMailMessage() to use appropriate MimeEntity subclasses. (issue [#232](https://github.com/jstedfast/MimeKit/issues/232))

## MimeKit 1.2.22 (2016-02-28)

* Added a new SecureMimeContext.Verify() overload that returns the extracted content stream.
* Exposed the SecureMimeContext.GetDigitalSignatures() method as protected, allowing custom
  subclasses to implement their own Verify() methods.
* Fixed X509CertificateDatabase to store the X509Certificate NotBefore and NotAfter DateTimes
  in UTC rather than LocalTime.
* Added a work-around for GoDaddy's ASP.NET web host which does not support the iso-8859-1
  System.Text.Encoding (used as a fallback encoding within MimeKit) by falling back to
  Windows-1252 instead.
* Added new convenience .ctors for CmsSigner and CmsRecipient for loading certificates from a
  file or stream.
* Fixed UrlScanner to properly deal with IPv6 literals in email addresses.

## MimeKit 1.2.21 (2016-02-13)

* Added a MultipartReport class for multipart/report.
* Fixed serialization for embedded message/* parts. (issue [#228](https://github.com/jstedfast/MimeKit/issues/228))
* Fixed MimeMessage.WriteTo() to only make sure that the stream ends with a newline if it
  wasn't parsed. (issue [#227](https://github.com/jstedfast/MimeKit/issues/227))
* Fixed MimeMessage to only set a MIME-Version if the message was not produced by the parser.
* Ignore timezones outside the range of -1200 to +1400.
* Added InternetAddress.Clone() to allow addresses to be cloned.
* Properly serialize message/rfc822 parts that contain an mbox marker.
* Fixed MimeMessage.DkimSign() to not enforce 7bit encoding of the body. (issue [#224](https://github.com/jstedfast/MimeKit/issues/224))
* Fixed ParameterList.IndexOf(string) to be case insensitive.

## MimeKit 1.2.20 (2016-01-24)

* Fixed serialization of mime parts with empty content. (issue [#221](https://github.com/jstedfast/MimeKit/issues/221))
* Fixed a bug in the TnefPropertyReader that would break when not all properties were read
  by the consumer of the API.
* Fixed the InternetAddress parser to throw a more informative error when parsing broken
  routes in mailboxes.
* Added HeaderList.Add(*, Encoding, string) and .Insert(*, Encoding, string) methods.
* Added more OpenPgpContext.Encrypt() overloads (and equivalent MultipartEncrypted overloads).
* Added OpenPgpContext.Import(PgpSecretKeyRing) and OpenPgpContext.Import(PgpSecretKeyRingBundle).
* Fixed HtmlUtils.HtmlAttributeEncode() to properly encode non-ascii characters as entities.
* Fixed HtmlUtils.HtmlEncode() to properly encode non-ascii characters as entities.
* Fixed MimeParser to track whether or not each multipart had an end boundary so that
  when they get reserialized, they match the original. (issue [#218](https://github.com/jstedfast/MimeKit/issues/218))
* Implemented an optimized OrdinalIgnoreCase string comparer which improves the performance
  of the MimeParser slightly.
* Fixed QuotedPrintableDecoder to properly handle "==" sequences.
* Added a ContentDisposition.TryParse(ParserOptions,string) method.
* Added a ContentType.TryParse(ParserOptions,string) method.
* Fixed MimeParser to trim the CR from the mbox From marker.
* Fixed SqlCertificateDatabase to properly chain Dispose.

## MimeKit 1.2.19 (2016-01-01)

* Handle illegal Content-Id headers that do not enclose their values in <>'s. (issue [#215](https://github.com/jstedfast/MimeKit/issues/215))
* Fixed reserialization of MimeParts with empty content. (issue [#213](https://github.com/jstedfast/MimeKit/issues/213))
* Improved parsing logic for malformed Content-Type headers.
* Fixed HtmlTokenizer to work properly when some closing tags were not lowercase.
* Bumped Bouncy Castle to v1.8.1.

## MimeKit 1.2.18 (2015-12-16)

* Removed unimplemented TNEF APIs.
* Use DateTime.UtcNow for S/MIME certificate validity checks.
* Added ToString() methods on ContentType/Disposition that take FormatOptions.
* Added a new ToString() method to InternetAddress that takes a FormatOptions. (issue [#208](https://github.com/jstedfast/MimeKit/issues/208))
* Added a MimeEntity.WriteTo() method that takes a bool contentOnly parameter. (issue [#207](https://github.com/jstedfast/MimeKit/issues/207))
* Added support for encoding parameter values using rfc2047 encoded-words instead of
  the standard rfc2231 encoding.
* Fixed SecureMailboxAddress's Fingerprint property to work with both the PGP key ID
  *and* the fingerprint. Previously only worked with the PGP key id. (issue [#203](https://github.com/jstedfast/MimeKit/issues/203))
* Added GroupAddress.Parse() and MailboxAddress.Parse() methods. (issue [#197](https://github.com/jstedfast/MimeKit/issues/197))
* Set a default filename when generating application/pgp-signature parts. (issue [#195](https://github.com/jstedfast/MimeKit/issues/195))

## MimeKit 1.2.17 (2015-12-05)

* Fixed DkimRelaxedBodyFilter to properly handle CRLF split across buffers.
* Added ContentType.IsMimeType method to replace CongtentType.Matches.
* Added S/MIME, PGP and DKIM support to the PCL and WindowsUniversal versions of MimeKit.
* Fixed PGP key expiration calculation when encrypting. (issue [#194](https://github.com/jstedfast/MimeKit/issues/194))

## MimeKit 1.2.16 (2015-11-29)

* Fixed relaxed body canonicalization logic for DKIM signatures. (issue [#190](https://github.com/jstedfast/MimeKit/issues/190))

## MimeKit 1.2.15 (2015-11-22)

* Fixed the Date parser to catch exceptions thrown by the DateTimeOffset .ctor if any of the
  fields are out of range.
* Fixed logic for trimming trailing blank lines for the DKIM relaxed body algorithm. (issue [#187](https://github.com/jstedfast/MimeKit/issues/187))
* Fixed DKIM body filters to reserve extra space in the output buffer. (issue [#188](https://github.com/jstedfast/MimeKit/issues/188))
* Allow specifying a charset encoding for each Content-Type/Disposition parameter.

## MimeKit 1.2.14 (2015-10-18)

* Fixed DKIM-Signature signing logic to use a UTC-based timestamp value rather than a
  timestamp based on the local-time. (issue [#180](https://github.com/jstedfast/MimeKit/issues/180))
* Fixed Multipart epilogue parsing and serialization logic to make sure that serializing
  a multipart is properly byte-for-byte identical to the original text. This fixes a
  corner-case that affected all types of digital signatures (DKIM, PGP, and S/MIME)
  spanning across nested multiparts. (issue [#181](https://github.com/jstedfast/MimeKit/issues/181))
* Fixed MimeMessage.WriteTo() to ensure that the output stream always ends with a new-line.

## MimeKit 1.2.13 (2015-10-11)

* Modified Base64Encoder's .ctor to allow specifying a maxLineLength.
* Fixed DKIM signing logic for multipart/alternative messages. (issue [#178](https://github.com/jstedfast/MimeKit/issues/178))

## MimeKit 1.2.12 (2015-09-20)

* Prevent infinite loop when flushing CharsetFilter when there is no input data left.

## MimeKit 1.2.11 (2015-09-06)

* Fixed an IndexOutOfRangeException bug in the TextToHTML converter logic. (issue [#165](https://github.com/jstedfast/MimeKit/issues/165))
* Fixed the DKIM-Signature verification logic to be more lenient in parsing DKIM-Signature
  headers. (issue [#166](https://github.com/jstedfast/MimeKit/issues/166))
* Fixed the DKIM-Signature verification logic to error-out if the h= parameter does not
  include the From header. (issue [#167](https://github.com/jstedfast/MimeKit/issues/167))
* Fixed the DKIM-Signature verification logic to make sure that the domain-name in the i=
  param matches (or is a subdomain of) the d= value. (issue [#169](https://github.com/jstedfast/MimeKit/issues/169))
* Fixed the CharsetFilter to avoid calling Convert() on empty input.
* Fixed logic for canonicalizing header values using the relaxed DKIM algorithm.
  (issue [#171](https://github.com/jstedfast/MimeKit/issues/171))
* Fixed AttachmentCollection to mark embedded parts as inline instead of attachment.
* Fixed the DKIM-Signature logic (both signing and verifying) to properly canonicalize the
  body content. (issue [#172](https://github.com/jstedfast/MimeKit/issues/172))

## MimeKit 1.2.10 (2015-08-16)

* Added public Stream property to IContentObject.
* Implemented a better fix for illegal unquoted multi-line Content-Type and
  Content-Disposition parameter values. (issue [#159](https://github.com/jstedfast/MimeKit/issues/159))
* Fixed the UrlScanner to properly handle "ftp." at the very end of the message text.
  (issue [#161](https://github.com/jstedfast/MimeKit/issues/161))
* Fixed charset handling logic to not override charset aliases already in the cache.

## MimeKit 1.2.9 (2015-08-08)

* Fixed WriteTo(string fileName) methods to overwrite the existing file. (issue [#154](https://github.com/jstedfast/MimeKit/issues/154))
* Updated InternetAddressList to implement IComparable.
* Fixed DKIM-Signature generation and verification.
* Added support for Message-Id headers that do not properly use encapsulate the value
  with angle brackets.

## MimeKit 1.2.8 (2015-07-19)

* Added a new MessageDeliveryStatus MimePart subclass to make message/delivery-status
  MIME parts easier to deal with.
* Improved HtmlTokenizer's support for the script tag - it is should now be completely
  bug free.
* Fixed to filter out duplicate recipients when encrypting for S/MIME or PGP.
* Fixed MimeParser to handle a message stream of just "\r\n".
* Add a leading space in the Sender and Resent-Sender header values.

## MimeKit 1.2.7 (2015-07-05)

* Fixed encoding GroupAddress with multiple mailbox addresses.
* Fixed MessageIdList to be less strict in what it will accept.
* Fixed logic for DKIM-Signature header folding.

## MimeKit 1.2.6 (2015-06-25)

* Fixed a bug in the HTML tokenizer to handle some weird HTML created by Outlook 15.0.
* Added CmsRecipient .ctor overloads that accept X509Certificate2. (issue [#149](https://github.com/jstedfast/MimeKit/issues/149))

## MimeKit 1.2.5 (2015-06-22)

* Changed BodyParts and Attachments to be IEnumerable&lt;MimeEntity&gt; -
  WARNING! This is an API change! (issue [#148](https://github.com/jstedfast/MimeKit/issues/148))
* Moved the IsAttachment property from MimePart down into MimeEntity.
* Added MimeMessage.Importance and MimeMessage.Priority properties.
* Vastly improved the HtmlToHtml text converter with a w3 compliant
  HTML tokenizer.

## MimeKit 1.2.4 (2015-06-14)

* Added support for generating and verifying DKIM-Signature headers.
* Improved error handling for Encoding.GetEncoding() in CharsetFilter constructors.
* Fixed buffering in the HTML parser.
* Fixed Windows and Temporary S/MIME contexts to use case-insensitive address
  comparisons like the other backends do. (issue [#146](https://github.com/jstedfast/MimeKit/issues/146)).
* Added HeaderList.LastIndexOf() convenience methods.
* Added a new Prepare() method to prepare a message or entity for transport
  and/or signing (used by MultipartSigned and MailKit.SmtpClient) to reduce
  duplicated code.
* Fixed FilteredStream.Flush() to flush filters even when no data has been
  written.
* Fixed the ChainedStream.Read() logic. (issue [#143](https://github.com/jstedfast/MimeKit/issues/143))
* Added EncoderFilter and DecoderFilter.Create() overloads that take an encoding
  name (string).
* HeaderList.WriteTo() now adds a blank line to the end of the output instead
  of leaving this up to the MimeEntity.WriteTo() method. This was needed for
  the DKIM-Signatures feature.

## MimeKit 1.2.3 (2015-06-01)

* Fixed TextToFlowed logic that stripped trailing spaces.
* Switched to PCL Profile78 to support Xamarin.Forms.

## MimeKit 1.2.2 (2015-05-31)

* Added a MultipartAlternative class which adds some useful convenience methods
  and properties for use with the multipart/alternative mime-type.
* Fixed MimeKitLite's MimeParser to use TnefPart for the ms-tnef mime-types.
* Fixed MimeMessage.TextBody to convert format=flowed into plain text.
* Made BoundStream.LeaveOpen protected instead of private.
* Fixed ChainedStream to dispose child streams when it is disposed.
* Obsoleted MultipartEncrypted.Create() methods in favor of equivalent
  Encrypt() and SignAndEncrypt() methods to make them a bit more intuitive.
* Added a MimeVisitor class that implements the visitor pattern for visiting
  MIME nodes.

## MimeKit 1.2.1 (2015-05-25)

* Added a Format property to ContentType.
* Added a TryGetValue() method to ParameterList.
* Added IsFlowed and IsRichText convenience properties to TextPart.
* Fixed the HtmlToHtml converter to properly handle HTML text that begins
  with leading text data.
* Fixed MimeParser.ParseHeaders() to handle input that does not end with a
  blank line. (issue [#142](https://github.com/jstedfast/MimeKit/issues/142))
* Renamed MimeEntityConstructorInfo to MimeEntityConstructorArgs.
* Modified the MimeParser to use TextPart to represent application/rtf.

## MimeKit 1.2.0 (2015-05-24)

* Force the use of the rfc2047 "B" encoding for ISO-2022-JP. (issue [#139](https://github.com/jstedfast/MimeKit/issues/139))
* Added some text converters to convert between various text formats
  including format=flowed and HTML.

## MimeKit 1.0.15 (2015-05-12)

* Fixed MimeMessage.WriteTo() to be thread-safe. (issue [#138](https://github.com/jstedfast/MimeKit/issues/138))

## MimeKit 1.0.14 (2015-05-09)

* Added support for .NET 3.5.
* Added a convenience CmsSigner .ctor that takes an X509Certificate2 argument.
* Fixed BodyBuilder to never return a TextPart w/ a null ContentObject.
* Fixed TextPart.GetText() to protect against NullReferenceExceptions if the
  ContentObject is null.
* Fixed MimeFilterBase.EnsureOutputSize() to initialize OutputBuffer if it is
  null. Prevents NullReferenceExceptions in obscure corner cases. (issue [#135](https://github.com/jstedfast/MimeKit/issues/135))
* Added a TnefAttachFlags enum which is used to determine if image attachments
  in MS-TNEF data are meant to have a Content-Disposition of "inline" when
  extracted as MIME attachments. (issue [#129](https://github.com/jstedfast/MimeKit/issues/129))
* Fixed TnefPart.ConvertToMessage() and ExtractAttachments() to use the
  PR_ATTACH_MIME_TAG property to determine the intended mime-type for extracted
  attachments.
* Catch DecoderFallbackExceptions in MimeMessage.ToString() and fall back to
  Latin1. (issue [#137](https://github.com/jstedfast/MimeKit/issues/137))

## MimeKit 1.0.13 (2015-04-11)

* Added a work-around for a bug in Thunderbird's multipart/related implementation.
  (issue [#124](https://github.com/jstedfast/MimeKit/issues/124))
* Improved MimeMessage.CreateFromMailMessage() a bit more to avoid creating empty
  From, Reply-To, To, Cc and/or Bcc headers.
* Modified the HeaderIdExtensions to only be available for the HeaderId enum values.

## MimeKit 1.0.12 (2015-03-29)

* Modified InternetAddressList.Equals() to return true if the lists contain the same
  addresses even if they are in different orders. (issue [#118](https://github.com/jstedfast/MimeKit/issues/118))
* Allow S/MIME certificates with the NonRepudiation key usage to be used for signing.
  (issue [#119](https://github.com/jstedfast/MimeKit/issues/119))
* Don't change the Content-Transfer-Encoding of MIME parts being encrypted as part of
  a multipart/encrypted. (issue [#122](https://github.com/jstedfast/MimeKit/issues/122))
* Fixed logic to decide if a PGP secret key is expired. (issue [#120](https://github.com/jstedfast/MimeKit/issues/120))
* Added support for SecureMailboxAddresses to OpenPgpContext to allow key lookups by
  fingerprints instead of email addresses.

## MimeKit 1.0.11 (2015-03-21)

* Added the ContentDisposition.FormData string constant.
* Allow the ContentDisposition.Disposition property to be set to values other than
  "attachment" and "inline". (issue [#112](https://github.com/jstedfast/MimeKit/issues/112))
* Shortened the length of the local-part of auto-generated Message-Ids.
* Fixed MimeMessage.CreateFromMailMessage() to not duplicate From/To/Cc/etc addresses
  if the System.Net.Mail.MailMessage has been sent via System.Net.Mail.SmtpClient
  prior to calling MimeMessage.CreateFromMailMessage(). (issue [#115](https://github.com/jstedfast/MimeKit/issues/115))
* When parsing S/MIME digital signatures, don't import the full certificate chain.
  (issue [#110](https://github.com/jstedfast/MimeKit/issues/110))
* Added immutability-friendly .ctor to MimeMessage for use with languages such as F#.
  (issue [#116](https://github.com/jstedfast/MimeKit/issues/116))

## MimeKit 1.0.10 (2015-03-14)

* Ignore semi-colons in Content-Transfer-Encoding headers to work around broken mailers.
* Added ParserOptions.ParameterComplianceMode (defaults to RfcComoplianceMode.Loose)
  which works around unquoted parameter values in Content-Type and Content-Disposition
  headers. (issue [#106](https://github.com/jstedfast/MimeKit/issues/106))
* Modified the MimeParser to handle whitespace between header field names and the ':'.
* Probe to make sure that various System.Text.Encodings are available before adding
  aliases for them (some may not be available depending on the platform).
* Added a MimePart.GetBestEncoding() overload that takes a maxLineLength argument.
* Modified MultipartSigned to use 78 characters as the max line length rather than 998
  characters. (issue [#107](https://github.com/jstedfast/MimeKit/issues/107))

## MimeKit 1.0.9 (2015-03-08)

* Added a new MessageDispositionNotification MimePart subclass to represent
  message/disposition-notification parts.
* Fixed the TNEF parser to gracefully deal with duplicate attachment properties.

## MimeKit 1.0.8 (2015-03-02)

* Modified the parser to accept Message-Id values without a domain (i.e. "<local-part@>").
* Fixed a NullReferenceException in MimeMessage.BodyParts in cases where a MessagePart
  has a null Message.
* Renamed DateUtils.TryParseDateTime() to DateUtils.TryParse() (the old API still exists
  but has been marked [Obsolete]).
* Renamed MimeUtils.TryParseVersion() to MimeUtils.TryParse() (the old API still exists
  but has been marked [Obsolete]).
* Fixed S/MIME support to gracefully deal with badly formatted signature timestamps
  which incrorectly use leap seconds. (issue [#103](https://github.com/jstedfast/MimeKit/issues/103))

## MimeKit 1.0.7 (2015-02-17)

* Fixed TnefPropertyReader.GetEmbeddedMessageReader() to skip the Guid.
* When decrypting PGP data, iterate over all encrypted packets to find one that
  can be decrypted (i.e. the private key exists in the user's keychain).
* Updated WindowsSecureMimeContext to respect SecureMailboxAddresses like the
  other backends. (issue [#100](https://github.com/jstedfast/MimeKit/issues/100))
* Added a Pkcs9SigningTime attribute to the CmsSigner for WindowsSecureMimeContext.
  (issue [#101](https://github.com/jstedfast/MimeKit/issues/101))

## MimeKit 1.0.6 (2015-01-18)

* Vastly improved MS-TNEF support. In addition to being fixed to properly extract
  the AttachData property of an Attachment attribute, more metadata is captured
  and translated to the MIME equivalents (such as attachment creation and
  modification times, the size of the attachment, and the display name).
* Migrated the iOS assemblies to Xamarin.iOS Unified API for 64-bit support.

Note: If you are not yet ready to port your iOS application to the Unified API,
      you will need to stick with the 1.0.5 release. The Classic MonoTouch API
      is no longer supported.

## MimeKit 1.0.5 (2015-01-10)

* Fixed out-of-memory error when encoding some long non-ASCII parameter values in
  Content-Type and Content-Disposition headers.

## MimeKit 1.0.4 (2015-01-08)

* Added workaround for msg-id tokens with multiple domains
  (e.g. id@domain1@domain2).
* Added convenience methods to Header to allow the use of charset strings.
* Added more HeaderList.Replace() method overloads for convenience.
* Added a FormatOptions property to disallow the use of mixed charsets when
  encoding headers (issue [#139](https://github.com/jstedfast/MimeKit/issues/139)).

## MimeKit 1.0.3 (2014-12-13)

* Improved MimeMessage.TextBody and MimeMessage.HtmlBody logic. (issue [#87](https://github.com/jstedfast/MimeKit/issues/87))
* Added new overrides of TextPart.GetText() and SetText() methods that take a
  charset string argument instead of a System.Text.Encoding.
* Fixed charset fallback logic to work properly (it incorrectly assumed that
  by default, Encoding.UTF8.GetString() would throw an exception when it
  encountered illegal byte sequences). (issue [#88](https://github.com/jstedfast/MimeKit/issues/88))
* Fixed S/MIME logic for finding X.509 certificates to use for encipherment.
  (issue [#89](https://github.com/jstedfast/MimeKit/issues/89))

## MimeKit 1.0.2 (2014-12-05)

* Fixed MimeMessage.HtmlBody and MimeMessage.TextBody to properly
  handle nested multipart/alternatives (only generated by automated
  mailers).

## MimeKit 1.0.1 (2014-11-23)

* Added MimeMessage.HtmlBody and MimeMessage.TextBody convenience properties.
* Added TextPart.IsPlain and TextPart.IsHtml convenience properties.

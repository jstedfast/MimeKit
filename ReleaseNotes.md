# Release Notes

## MimeKit 5.0.0 (unreleased)

MimeKit 5.0 is a major release containing a number of breaking changes. The headline changes are
the split of the cryptography support into its own assembly, the promotion of the new MIME parser,
a redesigned TNEF implementation, DMARC validation, and a new MIME compliance violation reporting API.
These notes describe changes since MimeKit 4.18.1.
See the [Porting Guide](PortingGuide.md) for help migrating from MimeKit 4.x.

### Breaking Changes

* Replaced the `persistent` boolean in `MimeParser`, `IMimeParser.SetStream` and
  `MimeMessage`/`MimeEntity` stream `Load` and `LoadAsync` APIs with `MimeContentStorage`.
  `Memory` (0) is the default and copies part content; `Source` (1) references the caller-owned
  input stream, which must remain open and unchanged while the content is used. `Source` rejects
  nonseekable streams with `ArgumentException` instead of silently copying.
  `LegacyMimeParser` retains its public boolean APIs.
* MIME header and address normalization now removes only ASCII spaces, tabs and line breaks,
  preserving non-ASCII whitespace in decoded text and message identifiers. Internationalized
  display-name folding no longer replaces Unicode whitespace with spaces. MIME token and numeric
  parsing no longer treats non-linear whitespace as padding.
  `MimePart.FileName` and `Multipart.Boundary` preserve their parameter values without trimming.
  `InternetAddress.Name` trims leading and trailing ASCII whitespace when set, including through
  constructors and parsing; a name such as `" John Smith "` becomes `"John Smith"`.
* Encoding resolution and explicit-charset decoding now propagate unexpected provider or decoder
  exceptions rather than silently falling back. Unsupported encodings and decoding errors retain
  their existing fallbacks.
* Renamed `ContentEncoding.Default` to `ContentEncoding.Unspecified`. Its numeric value remains
  zero and its behavior is unchanged: no explicit transfer encoding is selected.
* `MimeTypes.GetMimeType` now returns `application/gzip` for `.gz`/`.gzip`, `audio/flac` for
  `.flac`, `video/matroska` for `.mkv`, and `model/stl` for `.stl`. The previous MIME names
  remain supported by `TryGetExtension`.
* `MimeTypes` now uses registered names for PKCS#12, Debian packages, Dart, DWG/DXF, EOT,
  BinHex, FrameMaker MIF, PCL, SMIL, STEP, Visio, WordPerfect, legacy Excel and troff files.
  Ogg Opus files now use `audio/ogg`. AIFF, AVI, AU, ASF playlists, Java class files, M3U,
  portable anymaps, XPM, VRML and scriptlets also use corrected mappings. Previous MIME names
  remain accepted by `TryGetExtension`; its preferred extensions for `image/x-xpixmap` and
  `model/vrml` are now `.xpm` and `.wrl`, respectively.
* Mailbox sorting now compares IDN-encoded domains, so Unicode domains and their equivalent
  punycode forms sort together. Display names still take precedence, followed by domains and
  then local-parts, using case-insensitive comparisons.
* Renamed `MimeKit.IO.BoundStream` to `MimeKit.IO.BoundedStream`. Constructor parameters and
  stream behavior are unchanged; the old type name is no longer available.
* Renamed `MimeKit.Encodings.YEncoder` and `YDecoder` to `YEncEncoder` and `YEncDecoder`.
  Update references to the new names; the low-level checksum representation is unchanged.
* `Base64Encoder`, `QuotedPrintableEncoder`, `UUEncoder`, `YEncEncoder` and `EncoderFilter.Create`
  now default to CRLF line endings. New overloads accept `NewLineFormat.Dos` or
  `NewLineFormat.Unix`; `Mixed` is rejected. Specify `Unix` to retain the previous LF output.
  MIME serialization still follows `FormatOptions.NewLineFormat`, without a separate newline
  conversion filter when freshly encoding base64, quoted-printable or uuencoded content.
* OpenPGP decryption now verifies encrypted-data integrity and rejects legacy packets without
  integrity protection by default. Set `OpenPgpContext.AllowUnauthenticatedDecryption` to `true`
  only when legacy compatibility is required. Stream output must not be used until decryption
  completes successfully.
* `TemporarySecureMimeContext` no longer trusts self-signed certificates merely because they were
  imported, including certificates supplied by signed messages or PKCS#12 files. Configure trust
  explicitly with `Import (certificate, trusted: true)` or its async counterpart.
* CMS decompression now has a 64 MiB expanded-content limit, including MIME headers. Configure
  `SecureMimeContext.MaxDecompressedSize` (also on `ISecureMimeContext`) to change the limit;
  oversized content throws `FormatException`.
* Split the cryptography support out of MimeKit into a separate `MimeKit.Cryptography` assembly and
  NuGet package. (issue [#820](https://github.com/jstedfast/MimeKit/issues/820))
  * The `MimeKit` package is now a meta-package that references both `MimeKit.Core` and
    `MimeKit.Cryptography`, so projects referencing `MimeKit` continue to get everything. Projects
    that do not need S/MIME, PGP/MIME, DKIM or ARC can reference `MimeKit.Core` on its own.
  * The assembly produced by the `MimeKit` project is now named `MimeKit.Core`. The `MimeKit`
    namespace is unchanged.
  * `MimeKitLite` has been retired. `MimeKit.Core` replaces it.
  * Applications using `MimeKit.Cryptography` should call `CryptographyModule.Initialize ()` during
    startup to register the cryptographic entity factory. Without it, `multipart/signed` and
    `multipart/encrypted` content will be parsed as a plain `Multipart`. This happens automatically
    the first time `CryptographyContext` is used (for example, by `CryptographyContext.Register ()`),
    but messages parsed before then will not contain any cryptographic MIME entities.
  * The `MimeMessage.Sign ()`, `Encrypt ()` and `SignAndEncrypt ()` methods (and their async
    counterparts) are now extension methods in `MimeKit.Cryptography`. Add a
    `using MimeKit.Cryptography;` directive to continue using them.
  * The `VisitApplicationPgpEncrypted ()`, `VisitApplicationPgpSignature ()`,
    `VisitApplicationPkcs7Mime ()`, `VisitApplicationPkcs7Signature ()`, `VisitMultipartEncrypted ()`
    and `VisitMultipartSigned ()` methods have moved from `MimeVisitor` to the new
    `CryptographicMimeVisitor` class in `MimeKit.Cryptography`. Visitors that override any of these
    methods must now derive from `CryptographicMimeVisitor`. A visitor that derives from `MimeVisitor`
    will visit these entities via `VisitMultipart ()` or `VisitMimePart ()` instead.
* Promoted the new MIME parser, replacing the implementation used by default in 4.18.1:
  * `ExperimentalMimeParser` has been renamed to `MimeParser`.
  * The previous `MimeParser` implementation has been renamed to `LegacyMimeParser` and is retained
    for compatibility.
  * `MessageDeliveryStatus` now uses the new parser and is no longer tied to `LegacyMimeParser`.
  * Unlike `ExperimentalMimeParser`, the new `MimeParser` uses a `MimeReader` internally instead of
    inheriting from it. Use its `Parse*` methods, not `MimeReader.ReadHeaders ()`, `ReadEntity ()` or
    `ReadMessage ()`. It exposes `Options`, `IsEndOfStream`, `Position`, `ComplianceLogger` and
    `ComplianceOptions`, and mirrors the reader's `On*` callbacks as `protected virtual` methods.
    Subclasses overriding these callbacks must call the base implementation.
* `CryptographyContext` no longer registers a default S/MIME implementation. Applications must now
  explicitly register the context they want (for example via `CryptographyContext.Register ()`).
  This change is what makes MimeKit AOT-compatible; the `DefaultSecureMimeContext` constructors that
  rely on reflection to load a platform-appropriate SQLite library remain available for applications
  that do not need AOT compatibility.
* Removed the `DbConnection` parameter from protected `X509CertificateDatabase` and
  `SqlCertificateDatabase` APIs
  (`GetSelectCommand`, `GetSelectAllCrlsCommand`, `GetInsertCommand`, `GetUpdateCommand`,
  `GetDeleteCommand`, `GetTableColumns`, `CreateTable`, `AddTableColumn`, `CreateIndex` and
  `RemoveIndex`). Subclasses must update their overrides. The constructors still take a `DbConnection`.
* Removed the `beginOffset` and `lineNumber` arguments from `MimeReader.OnMboxMarkerRead ()` and
  `OnMboxMarkerReadAsync ()`. Both values are already supplied by the corresponding
  `OnMboxMarkerBegin ()` call, which makes the signature consistent with `OnMimePartContentRead ()`.
* Removed previously obsoleted APIs:
  * The `QEncoder` class and the public `HexEncoder` and `HexDecoder` implementations.
  * The `HtmlToHtml.FilterHtml` property. Use a dedicated library such as HtmlSanitizer instead.
  * The obsolete `IX509CertificateDatabase` APIs.
  * The obsolete `DkimSigner` and `ArcSigner` APIs, including the protected `DkimSignerBase.PrivateKey`
    setter. Subclasses must now pass the private key to the base class constructor instead.
  * The obsolete `MimeReader` APIs.
  * `TnefPropertyTag.Puid`. Use `TnefPropertyTag.PuidA` or `TnefPropertyTag.PuidW`.
* Changed the `partials` parameter of `MessagePartial.Join ()` from `IEnumerable<MessagePartial>` to
  `IReadOnlyList<MessagePartial>`. Arrays and `List<MessagePartial>` continue to work unchanged.
  `Join ()` now also validates the `id` and `number` parameters of every partial up front, so a
  mismatched `id` or missing `number` throws `InvalidOperationException` even when only a single partial
  is provided.
* Removed the `AsymmetricAlgorithmExtensions.AsAsymmetricAlgorithm ()` extension methods that converted
  BouncyCastle `AsymmetricKeyParameter` and `AsymmetricCipherKeyPair` keys into
  `System.Security.Cryptography` `RSA`, `DSA` and `ECDsa` instances. MimeKit no longer uses them.
  The `AsAsymmetricKeyParameter ()` and `AsAsymmetricCipherKeyPair ()` conversions in the other
  direction are unchanged.
* Removed the legacy binary serialization support (`[Serializable]`, the protected
  `(SerializationInfo, StreamingContext)` constructors and the `GetObjectData ()` overrides) from
  `ParseException`, `TnefException`, `CertificateNotFoundException`, `DigitalSignatureVerifyException`,
  `PrivateKeyNotFoundException` and `PublicKeyNotFoundException`. It was only available on .NET
  Framework, and the APIs it is built on have been
  [obsolete since .NET 8](https://learn.microsoft.com/en-us/dotnet/fundamentals/syslib-diagnostics/syslib0051).
  Microsoft considers `BinaryFormatter` deserialization a security vulnerability that
  [cannot be made secure](https://learn.microsoft.com/en-us/dotnet/standard/serialization/binaryformatter-security-guide),
  even for data believed to be trustworthy, and it always throws starting with .NET 9.
* The `MimeMessage.Sign ()`, `SignAsync ()`, `SignAndEncrypt ()` and `SignAndEncryptAsync ()`
  extension method overloads that do not take a `DigestAlgorithm` now sign using SHA-256 instead of
  SHA-1. SHA-1 is no longer considered secure for digital signatures, and RFC 8551 requires S/MIME
  agents to support SHA-256. Pass `DigestAlgorithm.Sha1` explicitly to restore the previous behavior.
* Moved the digest algorithm preference APIs (`DigestAlgorithmRank`, `EnabledDigestAlgorithms`, and the
  `Enable (DigestAlgorithm)`, `Disable (DigestAlgorithm)` and `IsEnabled (DigestAlgorithm)` methods) from
  `CryptographyContext` and `ICryptographyContext` to `OpenPgpContext`. They were never used by S/MIME,
  where the caller always chooses the signing digest algorithm, so on a `SecureMimeContext` they reported
  a misleading SHA-1-only list. In OpenPGP they only determine the preferred hash algorithms advertised by
  keys created with `GnuPGContext.GenerateKeyPair ()`. The default OpenPGP ranking now matches GnuPG's
  defaults for new keys: SHA-512, SHA-384, SHA-256, SHA-224 and SHA-1. Previously SHA-1 was ranked first.
  RIPEMD-160 is no longer included by default.
* `MimeReader` no longer includes the newline sequence that precedes a multipart boundary marker in the
  content passed to `OnMimePartContentRead[Async] ()` and `OnMultipartEpilogueRead[Async] ()`. Per
  RFC 2046, that newline belongs to the boundary marker. Previously it was passed to the callback and then
  excluded only from the `endOffset` reported to the corresponding `End` callback, so subclasses that
  buffered the content had to trim it themselves. The bytes passed to the `Read` callbacks now match the
  range between the `Begin` and `End` offsets exactly. Preamble content is unchanged.
* Replaced `IDkimPublicKeyLocator` and `DkimPublicKeyLocatorBase` with a new, more general
  `IDnsResolver` interface. `DkimVerifier` and `ArcVerifier` now take an `IDnsResolver`, and the
  protected `DkimVerifierBase.PublicKeyLocator` property has been replaced by `DnsResolver`.
  * Implementations only need to perform DNS TXT queries and return the raw record strings in a
    `DnsTxtResponse`, along with a `DnsQueryStatus` that distinguishes a successful query, a
    non-existent domain (NXDOMAIN) and a temporary failure. MimeKit now builds the
    `<selector>._domainkey.<domain>` query name and parses the DKIM key records itself.
  * DKIM key records are parsed more strictly per RFC 6376: records with duplicate tags, a `v=` tag
    that is not first or not `DKIM1`, an unrecognized `k=` key type, or an `s=` tag that excludes
    `email` are ignored. Keys whose `h=` tag does not permit the signature's hash algorithm or whose
    key type does not match the signature algorithm are rejected, revoked keys (an empty `p=`) are
    recognized, and the `t=s` flag is enforced for DKIM signatures.
  * A failed key lookup or an unparsable key record now causes verification to fail rather than
    throwing an exception.
* `DkimVerifier.Verify[Async] ()` now returns a `DkimSignatureValidationResult` instead of a `bool`.
  * The result's `Status` is one of `Pass`, `Fail`, `Policy`, `TempError` or `PermError` (matching the
    RFC 8601 `dkim` method results), and the `Reason` property describes why a signature did not pass.
    The result also exposes the signature's `Domain`, `Selector`, `AgentOrUserIdentifier` and
    `SignatureAlgorithm`, and `ToAuthenticationMethodResult ()` can be used to build an
    Authentication-Results entry.
  * Malformed DKIM-Signature headers no longer throw a `FormatException`; they are reported as
    `PermError` and the `FormatException` is available via the result's `Exception` property. DNS
    temporary failures (including exceptions thrown by the `IDnsResolver`) are reported as `TempError`.
    Only argument errors and cancellation throw.
  * Signatures whose `x=` expiration time has passed are now reported as `PermError`, and `t=`
    timestamps beyond the year 2038 are now accepted.
  * Added `DkimVerifier.Verify[Async] (MimeMessage)` overloads that verify every DKIM-Signature header
    in a message, up to the new `DkimVerifier.MaxSignatures` limit (default: 10).
* `DkimSigner` now always writes the `x=` tag as an integer, even when `SignaturesExpireAfter` has a
  fractional number of seconds.
* `ArcVerifier.Verify[Async] ()` now provides diagnostics for ARC chain validation failures. As required
  by RFC 8617, any error (including a DNS failure) still results in a `Chain` result of `Fail`, but:
  * The new `ArcValidationErrors.DnsTemporaryFailure` flag indicates that a public key could not be
    retrieved due to a temporary DNS failure (or an exception thrown by the `IDnsResolver`), so that
    receivers can choose to defer the message and try again later rather than seal it with `cv=fail`.
  * `ArcHeaderValidationResult` has new `Reason` and `Exception` properties that describe why an
    ARC-Message-Signature or ARC-Seal did not pass.
* Replaced `HtmlTokenizerState.AttributeValueQuoted` with separate `AttributeValueDoubleQuoted` and
  `AttributeValueSingleQuoted` states, matching the HTML specification. This also changes the numeric
  values of the `HtmlTokenizerState` members that follow them.
* The address parser and the `MailboxAddress.Address` setter no longer decode IDN-encoded (punycode) domains.
  `MailboxAddress.Address` and `Domain` now return the domain exactly as it was parsed or set, so
  `user@xn--bcher-kva.example` is no longer turned into `user@bücher.example`. Decoding was lossy (re-encoding
  does not always reproduce the original A-label), platform-dependent (`IdnMapping` uses ICU or the Windows NLS
  APIs), hid the A-label of a possible homograph domain, and caused `FormatOptions.International` to write an
  ASCII domain as Unicode. Related changes:
  * `MailboxAddress.GetAddress (false)` now decodes the domain into its Unicode form instead of returning the
    address as-is. Use `Address` to get the address as it was parsed or set.
  * Added `MailboxAddress.GetDomain (bool idnEncode)` to get the domain in its IDN-encoded or Unicode form.
  * `MailboxAddress.IsInternational` is now `false` for an IDN-encoded domain, and an address is serialized with
    its domain as-is when `FormatOptions.International` is `true`.
  * `MailboxAddress.Equals ()` compares the addresses as-is, so an address with an IDN-encoded domain is no longer
    equal to the same address with a Unicode domain. Compare `GetAddress (true)` to compare them regardless of form.
* `MimeIterator.PathSpecifier` now returns `"1"` (instead of `"0"`) for a non-multipart top-level message body,
  consistent with IMAP part specifiers and with the `"N.1"` already used for the non-multipart body of a
  `message/rfc822` part. `MimeIterator.MoveTo ()` now expects `"1"` for such a body. A multipart that is the body
  of a message (which has no IMAP part number) is still `"0"` (or `"N.0"`).
* The `MimeMessage.MessageId`, `ResentMessageId` and `InReplyTo` setters, the `MimeEntity.ContentId`
  setter and the `MessageIdList` (`MimeMessage.References`) `Add`, `AddRange`, `Insert` and indexer
  now share the same msg-id validation. The value may be specified with or without angle brackets, but an
  `ArgumentException` is now thrown for values that previously would have been silently truncated or
  altered: empty msg-ids (`<>`), anything other than whitespace or comments after the msg-id (such as a
  second msg-id), unquoted whitespace in a msg-id without angle brackets, and line breaks (other than
  folding whitespace) or NUL characters. Conversely, unusual msg-ids returned by the getters of a parsed message,
  such as `(paren@example.com`, can now be assigned back to these setters.
  `MessageIdList.AddRange ()` now validates every item before adding any of them.
* An empty msg-id (`<>`) is now ignored when parsing: the msg-id getters (`MessageId`, `ResentMessageId`,
  `InReplyTo` and `ContentId`) and `MimeUtils.ParseMessageId ()` return `null` instead of an empty string,
  and `MimeMessage.References` and `MimeUtils.EnumerateReferences ()` skip it. Folding whitespace within a
  quoted local-part is now unfolded.
* The `HtmlDocTypeToken.Name`, `PublicIdentifier` and `SystemIdentifier` setters now throw an
  `ArgumentException` for values that cannot be written as a well-formed DOCTYPE: an empty name, or a
  name containing whitespace, `>` or NUL; or an identifier containing `>` or NUL, or containing both
  `'` and `"`. `HtmlDocTypeToken.WriteTo ()` no longer writes the identifiers when `Name` is `null`.
* `MimeEntity.WriteTo (FormatOptions, Stream, bool, CancellationToken)` and `WriteToAsync ()` are no longer
  `virtual`, and `MimePart`, `Multipart` and `MessagePart` no longer override them. The entity (and all of its
  descendants) is now written by an internal writer that walks the tree without recursion. Subclasses that
  overrode these methods to customize the output must instead customize the entity's headers or content.

### DMARC

* Added `DmarcVerifier`, which validates a message's author domain against its DMARC policy as defined
  by RFC 9989 (DMARCbis). (issue [#1180](https://github.com/jstedfast/MimeKit/issues/1180))
  * Policy discovery and Organizational Domain determination use the RFC 9989 DNS tree walk via the
    `IDnsResolver` interface, so no Public Suffix List is required. The walk is bounded to at most 8
    queries per domain, and results are cached for the duration of each verification.
  * DKIM and SPF identifiers are checked for relaxed or strict alignment as requested by the policy's
    `adkim` and `aspf` tags. Unrelated identifier domains never trigger DNS queries.
  * DKIM signatures are verified automatically (using the `DkimVerifier` property) unless the caller
    supplies existing `DkimSignatureValidationResult`s, and only once a DMARC policy has been found.
    `DkimSignatureValidationResult` has a public constructor for supplying existing results.
  * MimeKit does not implement SPF. Callers supply the result of their own SPF check as an
    `SpfCheckResult`, using the new `SpfStatus` enum.
  * The `DmarcValidationResult` exposes the `DmarcStatus` (`None`, `Pass`, `Fail`, `TempError` or
    `PermError`), the policy to apply (after the `sp=`, `np=` and `t=y` rules), the requested policy,
    the policy and Organizational Domains, the aligned DKIM and SPF results, and `DmarcErrors`
    diagnostics. `ToAuthenticationMethodResult ()` can be used to build an Authentication-Results entry.
  * Messages with multiple From headers, or with no usable author domain, are reported as `PermError`.
    Messages with multiple author domains are rejected by default; raising `MaxAuthorDomains` evaluates
    each one and reports the most severe result.
* Added `DmarcRecord` for parsing DMARC policy records (`DmarcRecord.Parse ()` and `TryParse ()`),
  including the new RFC 9989 `np=`, `psd=` and `t=` tags. As required by RFC 9989, the parser is lenient:
  invalid tag values are discarded in favor of their defaults and reported via `DmarcRecordErrors` rather
  than causing the whole record to be rejected.

### MIME Compliance Violation Reporting

* Added optional MIME compliance reporting to `MimeReader` and `MimeParser`. Set `ComplianceLogger`
  to an `IMimeComplianceLogger` implementation to receive `MimeComplianceIssue` diagnostics with
  the violation, stream offset, line and column numbers, and `MimeCompliancePositionKind`.
  Reporting does not change tolerant parsing or decoding.
* Violations are classified along two axes: a `MimeComplianceSeverity` and a set of
  `MimeComplianceCategories` (`Interoperability`, `DataLoss` and `Security`). Severity is further
  qualified by the `MimeComplianceContext` (`Transport` or `Storage`), since defects such as a bare
  linefeed are far more serious in flight than at rest.
* Structural checks cover malformed, missing and repeated headers, ambiguous line endings and
  whitespace-only continuation lines, malformed MIME structure, and exceeded MIME nesting limits.
  Repeated `Content-Id` fields are checked per MIME part, not by comparing identifiers across parts.
* Address checks cover malformed mailbox and group syntax, nested groups, unterminated strings and
  comments, invalid characters, and potentially misleading display names.
* Parameter checks cover malformed and repeated declarations, invalid RFC 2231 encoding, missing
  continuation segments, and RFC 2047 encoded words used in parameter values. Legitimate continuation
  segments and an ordinary filename fallback alongside an extended filename are accepted.
* Encoded-word checks cover RFC 2047 B/Q payload syntax, padding, length, separation and placement in
  headers. Base64, quoted-printable and uuencode content validators check transfer-encoding syntax,
  including nonzero Base64 pad bits. These checks do not perform charset-level text validation.
* `MimeComplianceOptions` controls the reporting context, enables or disables individual address,
  parameter, encoded-word and transfer-encoding validators, and caps reports with
  `MaxIssuesPerViolation`. All validators are enabled by default; the default report cap is `0`
  (unlimited). A `TooManyComplianceIssues` diagnostic indicates when a cap has been reached.
* Added a `MimeAnalyzer` sample that reports violations in a compiler-style
  `file:line:column: severity: message` format.

### TNEF

* Redesigned the TNEF API (`MimeKit.Tnef`). This is a breaking change:
  * `TnefReader` and `TnefPropertyReader` were rewritten against [MS-OXTNEF] and [MS-OXCDATA] with
    bounded allocations, async equivalents and resilient recovery from corrupt or truncated streams.
  * `TnefComplianceMode` and `TnefComplianceStatus` were replaced by `TnefOptions`, `ITnefComplianceLogger`
    and `TnefComplianceViolation`. There is no longer a strict mode that throws on the first violation.
    `TnefReader.ComplianceOptions` (a `TnefComplianceOptions`) controls how violations are reported;
    `MaxIssuesPerViolation` caps how many times each violation is reported per stream.
  * Added a `TnefMessage` object model (`TnefAttachment`, `TnefRecipient`, `TnefMessageBody`,
    `TnefPropertySet`) loaded via `TnefMessage.Load[Async] ()` or `TnefPart.LoadTnefMessage[Async] ()`.
  * Replaced `TnefPart.ConvertToMessage ()` and `TnefPart.ExtractAttachments ()` with
    `TnefMessage.ConvertToMime ()`, which follows [MS-OXCMAIL] (including `PidTagMimeSkeleton`,
    `PS_INTERNET_HEADERS` and transport `Received` headers) and reports anything it could not
    represent via `TnefConversionResult.Losses`.
  * MIME conversion generates a `text/calendar` part for appointments, meeting
    requests, responses, counter-proposals and cancellations, following [MS-OXCICAL]. It covers time
    zones, recurrence rules, deleted and modified occurrences, attendees and reminders. Per
    [MS-OXCMAIL] 2.1.3.3.8 it is added as the last alternative of the body. Exception attachments it
    consumes are not repeated as MIME attachments. You can turn it off with
    `TnefConversionOptions.GenerateCalendar`. Data it cannot represent is reported as
    `TnefConversionLossKind.InvalidCalendarData` or `UnsupportedCalendarData`.
  * Added `TnefConversionOptions.MaxCalendarExceptions` (default 1024) and `TnefConversionOptions.MaxCalendarExceptionsSize`
    (default 16 MiB) to bound the size of the generated `text/calendar` part for recurring appointments with many
    exceptions. Exceptions beyond either limit are omitted and reported as
    `TnefConversionLossKind.CalendarExceptionLimitExceeded`.
  * `TnefMessage.ConvertToMime ()` chooses the message body using the [MS-OXBBODY] best body algorithm
    and never adds a `text/rtf` part. 4.x's `TnefPart.ConvertToMessage ()` added every body (RTF, HTML
    and plain text) as an alternative. When the best body is the compressed RTF body, the `text/plain`
    and `text/html` alternatives are generated from it using `RtfToText` and `RtfToHtml`, as recommended
    by [MS-OXCMAIL] 2.1.3.3.5. Otherwise an out-of-date RTF body is dropped.
  * When the best body is RTF, `TnefMessage.ConvertToMime ()` matches the RTF `\objattph` attachment
    placeholders with the attachments in `PidTagRenderingPosition` order ([MS-OXRTFEX] 2.2.3.4,
    [MS-OXCMAIL] 2.1.3.4.1.1). Attachments that are browser-displayable images become inline,
    `Content-Id`-referenced `<img>` elements in the generated HTML, grouped with the body in a
    `multipart/related`. OLE attachments require explicit conversion to render inline. When the RTF encapsulates HTML,
    the attachments that the extracted HTML references are inline and in the `multipart/related`,
    following the HTML best body rules ([MS-OXCMAIL] 2.1.3.4.1.2).
  * Added `TnefConversionOptions.AttachmentPlaceholderCallback` (`TnefAttachmentPlaceholderCallback`),
    which can supply text, such as the attachment's file name, to insert at each attachment placeholder.
  * Added `TnefConversionOptions.OleObjectConverter` and the abstract `TnefOleObjectConverter` class
    (with `Convert` and `ConvertAsync`), which can render OLE object attachments as images
    ([MS-OXCMAIL] 2.1.3.4.4). OLE attachments that are not rendered are reported as a
    `TnefConversionLossKind.OleObjectNotRendered` loss.
  * See the [TNEF section of the Porting Guide](PortingGuide.md#tnef) for help migrating from MimeKit 4.x.
* Added `TnefWriter` and `TnefPropertyWriter` for producing [MS-OXTNEF] streams, including named
  properties, multi-valued properties, recipient tables, embedded messages and [MS-OXRTFCP]
  compressed RTF. `TnefWriter.TemporaryStreamFactory` allows attribute and property buffers to use
  application-selected temporary streams instead of memory.
* Hardened the TNEF reader against malformed and malicious input: the TNEF signature, attribute
  levels, attribute containment, `attMessageClass` and checksums are now validated; truncation is
  reported as `StreamTruncated`; property, value and row counts are clamped to the attribute size;
  embedded-message nesting is limited; and date, GUID and end-of-stream decoding errors can no longer
  escape.
* Fixed a number of TNEF value decoding bugs: `PT_R4` and `PT_DOUBLE` are now decoded
  little-endian, `PT_SYSTIME` as UTC and `PT_CURRENCY` scaled by 1/10000; the `PT_I2` and
  `PT_BOOLEAN` coercions and `ReadTextValue` were corrected; stream offsets were widened to 64 bits;
  and the codepage is now selected per [MS-OXTNEF] 2.3.3.2, degrading gracefully when it is not
  available.
* Fixed the compressed RTF decoder to not trust `COMPSIZE`/`RAWSIZE`, to stop at the end of
  `CONTENTS`, to skip the CRC for uncompressed streams, and to discard unknown `COMPTYPE`s.

### Other Enhancements

* Added standalone `YEncReader` and `YEncWriter` APIs for framed yEnc content, including individual
  multipart ranges, streaming sync/async operations, metadata, configurable limits, and size/checksum
  validation. They do not change MIME parsing or assemble multipart downloads.
* Added `MimeKit.Text.HtmlToText` for converting HTML documents and fragments to plain text, with
  support for paragraphs, headings, lists, blockquotes and preformatted whitespace. Link destinations
  and image alt text are optional, and input, output and nesting limits are configurable.
* Added `MimeKit.Text.EnrichedToHtml` for RFC 1896 `text/enriched`, supporting formatting, colors,
  fonts, alignment, excerpts, language tags, document or fragment output, and resource limits.
  The older `text/richtext` format is not supported.
* Expanded `MimeTypes` mappings for modern document, image, audio, video, archive and data formats,
  including WebAssembly, YAML, TOML, Zstandard, CBOR, GeoJSON, Parquet, Apache Arrow, SQLite,
  HEIC/HEIF, JPEG XL, APNG, WebVTT and glTF. Corrected legacy names and reverse lookups; changed
  preferred MIME types are listed under Breaking Changes.
* Setting Subject, Date, Message-Id, In-Reply-To, MIME-Version, Importance, Priority or X-Priority
  through message properties removes duplicate occurrences, even when assigning a cached value.
  Updating Resent-Date or Resent-Message-Id continues to modify only the first occurrence,
  preserving older resent blocks.

* `MimeReader` (and therefore `MimeParser`) no longer uses recursion to parse nested multipart and
  message/rfc822 parts. Instead, it keeps an explicit stack of the entities being parsed, so a maliciously
  deep nesting of MIME parts can no longer overflow the call stack (which terminates the process) when
  `ParserOptions.MaxMimeDepth` is set to a large value. The sequence of callbacks is unchanged.

* `MimeMessage.WriteTo ()`, `MimeEntity.WriteTo ()` (and their async counterparts) and `MimeAnonymizer.Anonymize ()`
  no longer use recursion to write nested multipart and message/rfc822 parts, so writing a deeply nested message can
  no longer overflow the call stack. The output is unchanged.

* `MimeMessage.Dispose ()` and `MimeEntity.Dispose ()` no longer dispose nested entities recursively, so disposing a
  deeply nested message can no longer overflow the call stack. An iterative, child-before-parent traversal uses
  storage proportional to nesting depth. `Dispose (bool)` overrides now release only resources owned directly by
  that object; MimeKit disposes structural children before invoking the override. All cleanup hooks are attempted
  even if one fails; a single exception is rethrown, or multiple exceptions are reported in an `AggregateException`.
  Retrying disposal of the root retries incomplete cleanup without repeating successful hooks. See the
  [porting guide](PortingGuide.md#disposing-messages-and-entities) for the subclassing contract.

* Added `MimeReader.OnMboxMarkerBeginAsync ()` and `OnMboxMarkerEndAsync ()` so that every `MimeReader`
  callback now has an asynchronous counterpart. The default implementations call the synchronous methods.

* Added `SmtpUtf8AddressValidator`, which checks whether an address is a well-formed SMTPUTF8 mailbox
  (RFC 5321 as extended by RFC 6531) that also follows the additional rules in
  [draft-ietf-mailmaint-smtputf8-syntax](https://datatracker.ietf.org/doc/draft-ietf-mailmaint-smtputf8-syntax/):
  no A-labels, only PRECIS IdentifierClass code points (with the RFC 5892 contextual rules), Unicode
  Normalization Form C, and at most one script per address (Han may be combined with Hiragana and Katakana,
  with Hangul, or with Bopomofo). `TryValidate ()` reports the reason through `SmtpUtf8AddressError`. The
  validation tables use Unicode 18.0.0.
  `SmtpUtf8AddressValidationOptions.DecodeIdn` decodes IDN-encoded (`xn--`) domain labels
  and then validates the Unicode form. `AsciiIsLatin` counts ASCII letters as Latin, checking the
  local-part and each domain label separately, so that homographs such as `paypal@рaypal.com` (with a
  Cyrillic `р`) are detected. `EnforceLengthLimits` enforces the RFC 5321 length limits (64-octet local-part,
  63-octet labels and 253-octet domain in A-label form, 254-octet address), reporting each through its own error.

* Added `SmtpAddressValidator`, which checks whether an address is a well-formed RFC 5321 mailbox that can be
  used in the `MAIL FROM` and `RCPT TO` commands without the `SMTPUTF8` extension. `TryValidate ()` reports
  the reason through `SmtpAddressError`; addresses that are only invalid because they contain non-ASCII
  characters are reported as `NonAscii`. IDN-encoded domains are accepted.
  `SmtpAddressValidationOptions.AllowIso2022JpLocalPart` permits
  ISO-2022-JP escape sequences within a quoted-string local-part (as used by some Japanese mail systems), and
  `EnforceLengthLimits` enforces the RFC 5321 length limits.

* Added the `RtfToText` and `RtfToHtml` text converters for `text/rtf` content (such as the
  `PR_RTF_COMPRESSED` body of TNEF attachments). `RtfToHtml` extracts the original HTML from RTF that
  encapsulates HTML ([MS-OXRTFEX]) unless `ExtractEncapsulatedHtml` is `false`, and otherwise renders the
  RTF as HTML. Both converters are designed for untrusted input: their `MaxFontTableEntries`,
  `MaxColorTableEntries` and `MaxGroupDepth` properties (4096 by default) bound the resources used by
  hostile documents, nested groups that do not change any formatting are folded so that arbitrarily deep
  nesting uses constant memory, `\binN` data is skipped without buffering, and `RtfToHtml` only renders
  `http`, `https`, `mailto`, `ftp` and `tel` hyperlinks. The extracted HTML is filtered by the same logic as
  `HtmlToHtml`: `RtfToHtml.HtmlTagCallback` is applied to it, `RtfToHtml.MaxElementDepth` limits its element
  nesting (see `HtmlToHtml.MaxElementDepth` below), and `RtfToHtml.NoScriptHandling` controls how `<noscript>`
  elements are handled (see `HtmlToHtml.NoScriptHandling` below). `RtfToHtml.AttachmentPlaceholderCallback` and
  `RtfToText.AttachmentPlaceholderCallback` let the caller write content at each `\objattph` attachment
  placeholder.
* Added optional RTF compliance reporting through `IRtfComplianceLogger`, configured on `RtfToText`
  and `RtfToHtml`. Diagnostics cover malformed syntax, unbalanced groups, undefined fonts and resource
  limits, with byte offsets, line and column numbers, and group depth. `RtfComplianceOptions` supports
  per-violation report caps.
* Added optional HTML compliance reporting through `IHtmlComplianceLogger`, configured on
  `HtmlTokenizer` and `HtmlToHtml`. Diagnostics cover tokenizer errors, ignored tags, foreign-content
  transitions and depth limits, with character offsets and line and column numbers.
  `HtmlComplianceOptions` supports per-violation report caps. Reporting does not change the output.
* Added `HtmlTokenizer.ScriptingEnabled`, which corresponds to the HTML5 scripting flag and controls whether
  `<noscript>` content is tokenized as raw text (`true`, the default and previous behavior) or as normal
  markup (`false`).
* Added `HtmlToHtml.NoScriptHandling`. Previously, `<noscript>` content was always treated as raw text and was
  never passed to the `HtmlToHtml.HtmlTagCallback`, so markup such as tracking images inside `<noscript>`
  elements bypassed any filtering when the output was rendered with scripting disabled (as is typical for email).
  The new default, `HtmlNoScriptHandling.Unwrap`, tokenizes `<noscript>` content as normal markup (so that it is
  passed to the callback) and removes the `<noscript>` tags themselves, producing output that is interpreted the
  same way regardless of whether the renderer has scripting enabled. See the `HtmlToHtml.NoScriptHandling`
  documentation for the security implications of each setting.
* Added `ParserOptions.MaxHeaderLength` to limit how much memory the parser will use to buffer any
  single header. A header whose raw length exceeds the limit causes the parser to throw a `FormatException`.
  The default limit is 16 KB.
* Added `HtmlTokenizer.MaxElementDepth` and `HtmlToHtml.MaxElementDepth` to bound element tracking
  (default: 4096). Consecutive identical elements count once. When the limit is exceeded, the remaining
  input is returned as literal character data, which `HtmlToHtml` escapes rather than emitting as markup.
* Added `HtmlToHtml.OutputHtmlFragment`, which removes the `<!DOCTYPE>`, `<html>`, `<head>` (including its
  content) and `<body>` tags so that the output can be embedded within another HTML document. Like browsers,
  it treats the `<head>` element as implicitly closed at the first start tag or non-whitespace text that does
  not belong in the `<head>`, so content that browsers would render is not removed.
* `SqliteCertificateDatabase` (and therefore `DefaultSecureMimeContext`) now falls back to
  `Microsoft.Data.Sqlite` on .NET 8 and later if neither `System.Data.SQLite` nor `Mono.Data.Sqlite`
  is usable. Previously, they were unavailable on platforms for which `System.Data.SQLite` does not ship
  a native library, such as macOS on Apple Silicon (arm64). Connection pooling is disabled for
  `Microsoft.Data.Sqlite` connections, as with the other SQLite bindings, so that the database file is not
  left open (and locked on Windows) after the database is disposed.
* Added `MimeIterator.SkipChildren ()`, which makes the next `MoveNext ()` skip the descendants of the current
  entity (for example, the content of an attached message).
* Added `MimeIterator.Index` (the index of the current entity within its parent multipart),
  `MimeIterator.CurrentMessage` (the top-level or attached message that contains the current entity) and
  `MimeIterator.GetAncestor (int generation)`.
* `MimeIterator` can now be used in a `foreach` statement. Since the iterator itself is enumerated, its
  members (such as `SkipChildren ()` and `PathSpecifier`) can be used within the loop.
* Added `MimeMessage.TryGetEntity (string pathSpecifier, out MimeEntity entity)` (and to `IMimeMessage`), which
  gets the entity at the specified `MimeIterator` path specifier (e.g. `"3.2"`).

### Performance

* Reduced address parsing and RFC 2047 decoding allocations, including direct parsing of string
  input, fewer intermediate buffers for display names and ASCII addresses, and reuse of charset
  lookups within a parse. On .NET 8 and later, valid UTF-8 is decoded directly into the output buffer.
* Improved Base64 encoding and decoding throughput, including MIME line wrapping. `Base64Decoder`
  uses MimeKit's own SSSE3, AVX2 and Arm64 AdvSimd kernels while preserving permissive decoding.
  `EnableHardwareAcceleration` is enabled by default on .NET 8 and later; it was disabled in 4.18.1
  because of bugs in the runtime's Base64 decoder.
* Added SSSE3, AVX2 and Arm64 AdvSimd acceleration to `UUEncoder` and `UUDecoder`, with scalar fallback
  for malformed input. Both expose an `EnableHardwareAcceleration` property.
* Improved quoted-printable decoding throughput by scanning and copying literal runs in bulk,
  particularly for mostly-ASCII content.
* DKIM and ARC signed-header selection now indexes headers once and consumes repeated fields in
  linear time, avoiding quadratic work on large signed-header lists.
* `MimeIterator.MoveTo ()` now navigates directly to the specified entity in time proportional to its depth,
  rather than walking the tree from the beginning (or from the current position). `MimeIterator` also no longer
  allocates for each level it descends into.
* `MimeMessage.BodyParts` and `Attachments` now walk the MIME tree with `MimeIterator` instead of nested
  iterators, so each step no longer costs time and stack space proportional to the nesting depth.
* Reduced allocations for message and header bookkeeping, and removed no-op finalizers that
  needlessly placed managed objects on the GC finalization queue.
* Fixed quadratic buffer growth when parsing long or heavily folded headers in both MIME parsers.
* Accelerated `MimeReader` content scanning with SIMD on .NET 8 and later when compliance reporting
  is disabled.
* Fixed quadratic processing of unclosed elements in `HtmlToHtml` and `HtmlTextPreviewer`.
  Bulk scanning and allocation reductions also improve ordinary HTML processing: compared with
  4.18.1, the benchmarked documents tokenize about 15-25% faster with 34-39% fewer allocations;
  `HtmlToHtml` uses 25-52% fewer allocations and is about 7-20% faster with `TextWriter` output.

### Bug Fixes

* Fixed MIME parsing with short stream reads, including mbox markers and empty `message/rfc822`
  parts followed by multipart or mbox delimiters. Empty entities now have consistent offsets and
  line counts, and CRLF sequences split across reads are recognized correctly.
* Fixed hangs on truncated mbox markers ending in a carriage return, and handling of malformed
  headers at end of input. An mbox marker without a valid message no longer silently consumes
  following content or produces an empty message.
* Fixed MIME depth-limit handling that could cause an `InvalidCastException` in the reader-based
  parser (previously `ExperimentalMimeParser`).
* Improved preservation of truncated multipart messages when reserializing with `MimeParser`,
  including trailing boundary markers and missing final newlines. Use `FormatOptions.EnsureNewLine`
  when the output must end with a newline.
* Fixed signature verification of multipart messages stored with LF line endings: preserved boundary
  markers are converted to CRLF when `FormatOptions.VerifyingSignature` is set.
* Preserved malformed carriage-return-only header lines during newline conversion, preventing
  repeated serialization from turning them into the blank line that ends the header block.
* Fixed lossy folding of internationalized headers and long RFC 2047 words, preserving Unicode
  characters and whitespace. Header and parameter encoding falls back to UTF-8 when the selected
  charset cannot represent the value, rather than losing characters.
* Fixed RFC 2047 handling of literal encoded-word-like text, backslashes in Q-encoded values,
  malformed words preceding valid words, and quotes and backslashes in encoded group names.
* Fixed RFC 2231 decoding with empty charset fields and encoding of supplementary Unicode
  characters across continuation segments.
* Fixed parameter round-tripping for empty values, literal `=?` text, and RFC 2047-encoded UTF-8.
  Loose parsing of unquoted values now preserves the last character, trims trailing whitespace
  correctly, and ignores following comments. Literal parentheses must be quoted or encoded.
* Fixed serialization of domainless mailboxes and empty group names, and normalization of tabs in
  comment-derived display names. IDNA encoding no longer emits domains containing ASCII whitespace
  or address syntax characters.
* Fixed `Received.Id` validation and unfolding of `ReceivedClause.Comments`, and recovery of invalid
  quoted `Content-Disposition` values.
* Fixed negative fractional-hour UTC offsets in date formatting and rejection of numeric timezone
  offsets with invalid minutes.
* Fixed `AuthenticationResults` parsing with buffer offsets, empty values, dotted method names,
  Office 365-style headers and escaped comments. Serialization now quotes authserv-ids and property
  values as needed and preserves whitespace-containing values when folding.
* Corrected encoder output-size estimates for pending input and line breaks, preventing buffer
  overruns. Encoder and decoder estimates throw `ArgumentOutOfRangeException` rather than overflow
  when the result cannot be represented as an `int`.
* Fixed hardware-accelerated Base64 encoding of empty input, uuencode byte ordering across calls,
  and chunk-dependent uudecoding of malformed lines.
* Fixed quoted-printable line wrapping that could exceed the configured limit, yEnc wrapping at
  long line lengths, and loss of the initial yEnc escape character in single-part framed content.
* Fixed `FilteredStream` reads to flush filters at end of input and avoid reporting end-of-stream
  prematurely when a filter buffers an entire chunk.
* Fixed `CharsetFilter` to flush buffered decoder state even when the final input buffer is empty,
  preserving replacement characters and reporting strict decoding errors for truncated input.
* Fixed trailing carriage-return handling in `Dos2UnixFilter`, split-CRLF line lengths and
  unterminated `From ` line detection in `BestEncodingFilter`, and chunk-dependent offsets and
  exceptions in `OpenPgpDetectionFilter`.
* Fixed `RtfCompressedToRtf.IsValidCrc32` incorrectly accepting truncated headers and
  `RtfCompressedToRtf.Reset()` retaining dictionary bytes from the previous stream.
* Improved HTML tokenization and serialization of malformed tags, comments, DOCTYPEs, character
  references and truncated input. Corrected cases where markup could be hidden from
  `HtmlToHtml.HtmlTagCallback` or created accidentally when filtered output was parsed again.
* HTML tokenization now accounts for SVG/MathML foreign content, CDATA, self-closing raw-text
  elements, forms, framesets and misnested formatting. `HtmlToHtml` and `HtmlTextPreviewer` handle
  the corresponding content suppression and element closures consistently.
* Fixed HTML attribute and DOCTYPE identifier quoting, and escaping of literal `<` characters
  adjacent to removed markup.
* Improved URL detection in `TextToHtml` and `FlowedToHtml`, including culture-independent matching,
  balanced delimiters, IPv6 hosts, address literals, IDNA domains, query strings and fragments.
  Whitespace, control characters and invisible formatting characters no longer disguise link
  targets; bounded scanning also avoids excessive work on malformed addresses.
* Fixed `TextToFlowed` space-stuffing and long-word wrapping, and excessive output growth at deep
  quote depths. `TextToHtml` and `FlowedToHtml` now honor callback suppression of blockquote end tags
  and nested content.
* Fixed DKIM/ARC tag-name parsing and DKIM body canonicalization of bare carriage returns and long
  runs of empty LF-terminated lines. Body hashes for messages containing bare CRs may change.
* Restricted opt-in CRL retrieval to certificates on paths validated against configured trust
  anchors. Unrelated certificates no longer trigger requests.
* Fixed cancellation propagation during CRL downloads, ARC verification and synchronous CMS
  decompression, and preservation of original failures during S/MIME verification.
* `OpenPgpContext.Verify[Async] ()` now throws `FormatException`, rather than `InvalidCastException`,
  for non-signature packets in signature data.
* Fixed `WindowsSecureMimeContext` to import signer certificates regardless of whether S/MIME
  capabilities are present, and to retain advertised capabilities for selecting reply encryption.
* `SqliteCertificateDatabase` validates constructor arguments before creating the database file.

## MimeKit 4.18.1 (2026-09-19)

* Prevent integer overflows in TnefPropertyReader due to corrupt content.

## MimeKit 4.18.0 (2026-09-13)

* Removed base, embed, input, link and source tags as "unsafe" tags. These are all void tags in HTML,
  meaning they do not have inner content. (issue [#1247](https://github.com/jstedfast/MimeKit/issues/1247))
* Fixed InternetAddressListConverter to allow conversion of empty strings into an empty list.
  (issue [#1249](https://github.com/jstedfast/MimeKit/issues/1249))
* Fixed NullReferenceException in BouncyCastleSecureMimeContext when SubjectKeyIdentifier is requested
  but not available. (issue [#1250](https://github.com/jstedfast/MimeKit/issues/1250))
* Fixed MimeReader to support MimePart content larger than 2GB.
  (issue [#1252](https://github.com/jstedfast/MimeKit/issues/1252))
* Fixed Message-Id parser to not decode international domains.
  (issue [#1254](https://github.com/jstedfast/MimeKit/issues/1254))
* Bumped System.Security.Cryptography.Pkcs dependency to 10.0.0.
* Bumped System.Text.Encoding.CodePages dependency to 10.0.0.
* Bumped BouncyCastle.Cryptography dependency to 2.7.0.

Note: As of BouncyCastle 2.7.0, decrypting S/MIME content encrypted with Blowfish, CAST5, RC2/40, RC2/64,
or RC2/128 will fail with a `CmsException: no fixed size for content-encryption key; constant-time RSA unwrap unavailable.`

Use the following code snippet to restore pre-2.7.0 behavior:

```csharp
Org.BouncyCastle.Utilities.Properties.SetThreadBoolean (Org.BouncyCastle.Utilities.Properties.CmsAllowLenientRsaPkcs1, true);
```

## MimeKit 4.17.0 (2026-05-26)

* Added a Received header parser.
* Marked HtmlToHtml.FilterHtml as obsolete to warn developers that this feature is not enough to protect
  against Cross-Site Scripting (XSS) vulnerabilities.
* Fixed logic for calculating maxOffset for Base64Encoder's hwaccel routines.
  (issue [#1239](https://github.com/jstedfast/MimeKit/issues/1239))
* Capped the maxLineLength at 76 for Base64Encoder (which is what it used to be capped at until v4.14).
* Changed the default TnefPart mime-type to application/ms-tnef to match O365 Exchange behavior.
* Code quality improvements.

## MimeKit 4.16.0 (2026-04-15)

* Simplified logic for illegal ctrl characters within quoted-string local-part tokens in addr-specs.
* Fixed IndexOutOfRangeException in ParseUtils.TryParseMsgId().
  (issue [#1227](https://github.com/jstedfast/MimeKit/issues/1227))
* Improved ParseException message for invalid ctrl chars in quoted-strings.
* Improved recovery logic for InternetAddressList.TryParse().
* Fixed leak in X509Certificate2Extensions.GetPrivateKeyAsAsymmetricKeyParameter().
  (issue [#1230](https://github.com/jstedfast/MimeKit/issues/1230))
* Fixed potential integer overflow in TnefPropertyReader.
  (issue [#1231](https://github.com/jstedfast/MimeKit/issues/1231))
* Added support for converting ECDsa PrivateKeys between Windows and BouncyCastle.
* Partial Encryption & Certificate Validation Failures for S/MIME.
  (issue [#1224](https://github.com/jstedfast/MimeKit/issues/1224))
* Fixed MimeParser/MimeReader.ScanContent to not init local until needed.
  (issue [#1234](https://github.com/jstedfast/MimeKit/issues/1234))
* Improved Received header folding logic.

## MimeKit 4.15.1 (2026-03-04)

* SECURITY: Use stricter parsing logic for quoted-strings in addr-specs to prevent
  SMTP command injection attacks when those quoted-strings include CRLF sequences.

## MimeKit 4.15.0 (2026-02-14)

* Bumped System.Buffers to 4.6.1 and System.Memory to 4.6.3
* Fixed inexact read in TnefPropertyReader.GetEmbeddedMessageReader()
* Fixed IndexOutOfRangeException in DateTImeUtils.TryParse() when missing timezone after AM/PM.
  (issue [#1204](https://github.com/jstedfast/MimeKit/issues/1204))
* Added nullable attributes to the API.
  (issue [#1067](https://github.com/jstedfast/MimeKit/issues/1067))
* AuthenticationResults parsing now supports Gmail style headers.
  (issue [#1208](https://github.com/jstedfast/MimeKit/issues/1208))
* Changed the Base64Decoder to no longer default to enabling hardware acceleration due to
  bugs in System.Buffers.Text.Base64.DecodeFromUtf8 (will likely be fixed in .NET 11).
  (issue [#1215](https://github.com/jstedfast/MimeKit/issues/1215))
* Improved SqlCertificateDatabase abstraction to make it easier to subclass when the
  SQL syntax does not match SQLite.
  (issue [#1218](https://github.com/jstedfast/MimeKit/issues/1218))
* Bumped BouncyCastle.Cryptography to 2.6.2.
* Added support for .NET 10.

## MimeKit 4.14.0 (2025-09-28)

* Updated BouncyCastle.Cryptography dependency to v2.6.1.
* Added BodyBuilder.BodyEncoding property to allow setting body text encoding.
* Fixed a number of TnefPropertyTags that didn't have proper Type info.
* Added TnefPropertyTag.PuidA and TnefPropertyTag.PuidW and Obsoleted
  TnefPropertyTag.Puid.
* Fixed TnefPropertyTag.ToUnicode() to only affect String8 tags.
* Optimized the Base64Encoder to use hardware acceleration when available.
* Optimized the Base64Decoder to use hardware acceleration when available.
* Fixed parsing of short StatusGroups in MessageDeliveryStatus
  (issue [#1181](https://github.com/jstedfast/MimeKit/issues/1181))
* Fixed MimeParser and ExperimentalMimeParser to consistently adhere to
  ParserOptions.MaxMimeDepth.
* Fixed MimeParser and ExperimentalMimeParser to properly (re)initialize their
  MboxMarker and MboxMarkerOffset properties before parsing each message in an
  mbox stream. When parsing fails, these properties will now have values of
  null and -1, respectively.
* Fixed MultipartSigned.Create() to avoid corrupting binary content of the
  entity (or child entities) being signed (important for AS2 messages).
  (issue [#1184](https://github.com/jstedfast/MimeKit/issues/1184))
    * Note: It is important for developers to properly set the
      ContentTransferEncoding of these parts to ContentEncoding.Binary
      to avoid corruption.
* Fixed MimeParser, ExperimentalMimeParser and MimeReader to handle header
  lines that exceed the internal input buffer length.
  (issue [#1189](https://github.com/jstedfast/MimeKit/issues/1189))
* Obsoleted DkimSignerBase, DkimSigner, and ArcSigner .ctors that did not take a
  parameter that could be used to initialize the PrivateKey property.
* Obsoleted DkimSignerBase.PrivateKey property setter.

## MimeKit 4.13.0 (2025-06-25)

* Fixed a memory leak in MimeAnonymizer and MimeUtils.Unquote() which gets used by the
  address parser. (issue [#1161](https://github.com/jstedfast/MimeKit/issues/1161))
* Optimized MimeReader and MimeParser to use Span&lt;T&gt;.IndexOf() on .NET Core which
  can improve performance by 20-30% when parsing MemoryStreams or 5-10% when parsing
  FileStreams.
* Optimized the MboxFromFilter and ArmoredFromFilter by using Span&lt;T&gt;.IndexOf().
* Fixed S/MIME logic to allow certificates without the KeyEncipherment key usage for
  encryption. (issue [#1165](https://github.com/jstedfast/MimeKit/issues/1165))
* Added MimeAnonymizer.PreserveHeaders as a way of preventing anonymization for
  specific headers.
* Added message/deliver-status and message/disposition-notification support to
  MimeAnonymizer.
* Slightly optimized the Unix2DosFilter.

## MimeKit 4.12.0 (2025-04-28)

* Removed CTRL characters from the list of allowed domain characters.
* Fixed a bug in the address list parser in Strict mode that allowed 2 addresses to be
  separated by whitespace instead of requiring a comma.
* Added a new MimeAnonymizer class that can be used to anonymize MimeMessages by
  x-ing out non-syntactically relevant information.
* Fixed TryParse methods to not throw ArgumentExceptions.
  (issue [#1158](https://github.com/jstedfast/MimeKit/issues/1158))
* Fixed a bug in MimeReader that used the buffer offset instead of the stream offset
  to check if the stream position was beyond the byte offset specified by a
  Content-Length header when parsing UNIX mbox files.
* Fixed a long-standing bug in MimeParser (which also existed in MimeReader) where
  it would encounter a buffering bug when parsing some Mbox files (typically
  multi-gigabyte). (issue [#991](https://github.com/jstedfast/MimeKit/issues/991))
* Fixed AttachmentCollection.Add/AddAsync methods to dispose the MimePart it is creating
  if loading content fails from the stream fails.
* Added leaveOpen parameters for HtmlWriter .ctors.
  (issue [#1159](https://github.com/jstedfast/MimeKit/issues/1159))
* Fixed parsing when ParserOptions.AllowUnquotedCommasInAddresses = false. This used to
  incorrectly allow unquoted commas in address Display Names if the comma followed at least
  2 words.

## MimeKit 4.11.0 (2025-03-08)

* Fixed logic for validating HTML Tag and Attribute names.
  (issue [#1136](https://github.com/jstedfast/MimeKit/discussions/1136))
* Minor performance improvements to MimeParser, ExperimentalMimeParser, and MimeReader.
* Obsoleted MimeReader.OnMultipartBoundary() and OnMultipartEndBoundary(), replacing them
  with OnMultipartBoundaryBegin/Read/End() and OnMultipartEndBoundaryBegin/Read/End().
  This now allows ExperimentalMimeParser to track trailing whitespace on boundary lines,
  resulting in improved re-serialization of parsed messages.
* Fixed serialization of message/rfc822 parts without a message.
* Fixed serialization of messages that did not have a blank line between the headers and body
  (only when parsed using the ExperimentalMimeParser).
* When using DbTransactions, set the transaction on the DbCommands.
  (issue [#1142](https://github.com/jstedfast/MimeKit/issues/1142))
* Improved Date header parser for JST and KST timezones.
* Improved Date header parser to handle AM/PM and leap seconds.
* Improved address parser to handle unbalanced ')' (rfc7103)

## MimeKit 4.10.0 (2025-01-26)

* Fixed logic for converting BouncyCastle DSA keys to System.Security equivalents.
* Fixed BouncyCastleSecureMimeContext to respect the CheckCertificateRevocation property when
  encrypting to recipeints and when verifying signatures.
* Marked IX509CertificateDatabase.Update(X509CrlRecord) as obsolete.
* Fixed TemporarySecureMimeContext.Import(X509Crl) to not import duplicates.
* Added new MimeMessage .ctor that takes IEnumerable&lt;Header&gt;.
* Fixed MimeReader to better handle garbage at the start of an mbox.
* Fixed MimeReader/ExperimentalMimeParser to handle really long mbox markers by introducing
  2 new methods: OnMboxMarkerBegin() and OnMboxMarkerEnd().
* Improved MimeReader header parsing (mostly just state tracking improvements) which
  allows it to throw the appropriate exception if EOS is reached before parsing any
  headers.
* Make sure to flush any remaining text in the FlowedToText and FlowedToHtml converters
  (issue [#1130](https://github.com/jstedfast/MimeKit/issues/1130))
* Fixed Header folding/encoding logic for Original-Message-ID by making it follow the same
  rules as Message-ID/Content-ID/etc.
  (issue [#1133](https://github.com/jstedfast/MimeKit/issues/1133))

## MimeKit 4.9.0 (2024-12-09)

* Started adding some DynamicallyAccessedMembers attributes for AOT compatibility.
* Refactored some code for AOT Compatibility (MimeKitLite is now 100% AOT Compatible but MimeKit still has
  issues related to SQLite database loading for the S/MIME certificate database).
  (MailKit issue [#10844](https://github.com/jstedfast/MailKit/issues/1844))
* Fixed TextPreviewer to use an encoding with an empty string fallback to prevent '?' characters from
  being appended to the generated preview string if the byte sequence was truncated.
* Improved performance of InternetAddressList.Parse()/TryParse().
* Improved InternetAddressList parser performance for malformed addresses that only contain
  display-name strings separated by commas.
  (issue [#1106](https://github.com/jstedfast/MimeKit/issues/1106))
* Exposed BouncyCastleCertificateExtensions.IsSelfSigned(), GetKeyUsageFlags() and IsDelta() as new public APIs.
* Exposed X509KeyUsageBits enum as public.
* Added support for domain-bound S/MIME certificates. (issue [#1113](https://github.com/jstedfast/MimeKit/issues/1113))
* Dropped support for net6.0 in the nuget packages (Microsoft support ended Nov 12, 2024).
* Removed explicit dependency on System.Runtime.CompilerServices.Unsafe.
* Bumped System.Security.Cryptography.Pkcs dependency to v8.0.1.
* Bumped BouncyCastle.Cryptography dependency to v2.5.0.
* Bumped System.Buffers dependency to v4.6.0.
* Bumped System.Memory dependency to v4.6.0.

## MimeKit 4.8.0 (2024-09-29)

* Added TypeConverters for InternetAddress and InternetAddressList.

## MimeKit 4.7.1 (2024-07-11)

* Bumped System.Formats.Asn1 to v8.0.1 to fix a denial of service security issue.

## MimeKit 4.7.0 (2024-06-29)

* Bumped BouncyCastle.Cryptography from 2.3.1 to 2.4.0
* Don't override the parameter encoding with UTF-8 if format.International is true.
  (issue [#1041](https://github.com/jstedfast/MimeKit/issues/1041))
* Fixed BouncyCastle S/MIME backend to properly encrypt/decrypt for SubjectKeyIdentifier.
  (BouncyCastle issue [#532](https://github.com/bcgit/bc-csharp/issues/532))
* Added support for addresses like "webmaster\@custom-host.com@mail-host.com".
  (issue [#1043](https://github.com/jstedfast/MimeKit/issues/1043))
* Improved Content-Type and Content-Disposition parameter serialization in a few cases.
* Added tests for HeaderList.set_Item[int index] and fixed a bug that was exposed.
* Fixed ContentDisposition.Size parsing to use InvariantCulture.
* Fixed parser logic for determining if a message/rfc822 part is encoded.
  (issue [#1049](https://github.com/jstedfast/MimeKit/issues/1049))

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

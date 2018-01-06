# Release Notes

### MimeKit 2.0.1

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

### MimeKit 2.0.0

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

### MimeKit 1.22.0

* Fixed a buffering bug in MimeParser's header parser. (issue #358)
* Set the TnefReader charset on extracted text/plain and text/html bodies. (issue #357)
* Added safeguard to protect against malformed nested group addresses which could cause
  a stack overflow in the parser. ParserOptions now has a way of limiting the recursive
  depth of rfc822 group addresses using the MaxAddressGroupDepth property. (issue #355)
* Fixed the S/MIME certificate database for .NETStandard by using GetFieldValue() instead
  of GetBytes() which is not supported on .NETStandard. (issue #351)

### MimeKit 1.20.0

* Added async support for writing MimeMessage, MimeEntity, HeaderList and ContentObject.
* Added async support for parsing MimeMessage, MimeEntity, and HeaderList.
* Added async support to MimeKit.IO streams.
* Removed methods marked [Obsolete] (which have been marked obsolete for several years now).
* Improved performance of writing messages by a small amount.
* Fixed SecureMimeDigitalSignature to capture the signature digest algorithm used by the sending
  client. (issue #341)
* Fixed the S/MIME decoder to correctly determine the RC2 algorithm used by the sending client.
  (issue #337)
* Fixed a bug in BoundStream.Seek().

### MimeKit 1.18.1

* Added CanSign() and CanEncrypt() methods to CryptographyContext for checking
  whether or not a mailbox can be used for signing or be encrypted to. (issue #325)
* Automatically register the CodePagesEncodingProvider when running on .NETStandard. (issue #330)
* Fixed MimeMessage.TextBody to return null when the top-level MIME part is a TextPart
  marked as an attachment.
* Fixed the HtmlToHtml converter to suppress comments if the HtmlTagContext's SuppressInnerContent
  property is active (even if FilterComments is false).
* Documented OpenPgpContext.GenerateKeyPair() which was added in 1.18.0.
* Added OpenPgpContext.Delete() methods to delete public and secret keyrings.
* Added OpenPgpContext.SignKey().
* Remove "Version:" header from armored OpenPGP output. (issue #319)

### MimeKit 1.18.0

* Allow importing of known PGP keys (needed when re-importing keys after signing them). (issue #315)
* Added APIs to enumerate public and secret PGP keys.
* Added an OpenPgpDetectionFilter to detect OpenPGP blocks and their stream offsets.
* Added a MimeMessage.WriteTo() overload that takes a bool headersOnly argument.
* Pushed SecureMimeContext's EncryptionAlgorithm preferences down into CryptographyContext.
* Updated GnuPGContext to load algorithm preferences from gpg.conf.
* Fixed TemporarySecureMimeContext to use the fingerprint in the certificate lookup methods
  when the MailboxAddress argument is a SecureMailboxAddress. (issue #322)
* Fall back to using the Subject Alternative Rfc822 Name if the SubjectEmailAddress fails. (issue #323)

### MimeKit 1.16.2

* Fixed a bug in the MailMessage to MimeMessage conversion which corrupted the Subject string. (issue #306)
* If no KeyUsage extension exists for an X509 certificate, assume no restrictions on key usage.
* Throw an exception if there is a problem building an X509 certificate chain when verifying
  S/MIME signatures.

### MimeKit 1.16.1

* Fixed TextToHtml and FlowedToHtml's OutputHtmlFragment property to work.
* Fixed EncodeAddrspec and DecodeAddrspec to handle string.Empty. (issue #302)
* Allow string.Empty as a valid addrspec for MailboxAddress. (issue #302)
* Catch exceptions trying to import CRLs and Certs when verifying S/MIME signatures. (issue #304)

### MimeKit 1.16.0

* Added new ParserOptions option to allow local-only mailbox addresses (e.g. no @domain).
* Improved address parser to interpret unquoted names containing commas in email addresses
  as all part of the same name/email address instead of as a separate email address.
* Greatly improved the WindowsSecureMimeContext backend.
* A number of fixes to bugs exposed by an ever-increasing set of unit tests (up to 87% coverage).

### MimeKit 1.14.0

* Added International Domain Name support for email addresses.
* Added a work-around for mailers that didn't provide a disposition value in a
  Content-Disposition header.
* Added a work-around for mailers that quote the disposition value in a Content-Disposition
  header.
* Added automatic key retrieval functionality for the GnuPG crypto context.
* Added a virtual DigestSigner property to DkimSigner so that consumers can hook into services
  such as Azure. (issue #296)
* Fixed a bug in the MimeFilterBase.SaveRemainingInput() logic.
* Preserve munged From-lines at the start of message/rfc822 parts.
* Map code page 50220 to iso-2022-jp.
* Format Reply-To and Sender headers as address headers when using Header.SetValue().
* Fixed MimeMessage.CreateFromMailMessage() to set MimeVersion. (issue #290)

### MimeKit 1.12.0

* Added new DKIM MimeMessage.Sign() methods that take an IList<string> of header field names
  to sign.
* Improved the address parser to allow the lack of a terminating ';' character at the end of
  group addresses.
* Improved the address parser to unquoted ',' and '.' characters in the name component of
  mailbox and group addresses.
* Added support for CryptographyContext factories by adding new Register() methods that
  take function callbacks that return a SecureMimeContext or OpenPgpContext. Thanks to
  Christoph Enzmann for this feature. (issue #283)
* Fixed DefaultSecureMimeContext..cctor() to not call Directory.CreateDirectory() on
  the default database directory. Instead, let the .ctor() create it instead if and when
  an instance of the DefaultSecureMimeContext is created. (issue #285)
* Store DBNull in S/MIME SQL backends for null values (SQLite handles `null` but
  databases such as Postgres do not). (issue #286)

### MimeKit 1.10.1

* Fixed the Content-Type and Content-Disposition parameter parser to remove trailing lwsp from
  unquoted parameter values. (issue #278)
* Fixed MimePart.WriteTo() to not necessarily force the content to end with a new-line.

### MimeKit 1.10.0

* Fixed OpenPgpContext.Verify() to throw FormatException if no data packets found.
* Added new MailboxAddress constructors that do not take a 'name' argument. (issue #267)
* Added an HtmlToHtml.FilterComments property to remove comments. (issue #271)
* Modified address parser to handle invalid addresses like "user@example.com <user@example.com>".

### MimeKit 1.8.0

* Improved parsing of malformed mailbox addresses.
* Added DecompressTo() and DecryptTo() methods to SecureMimeContext.
* Fixed MessagePartial.Split().

### MimeKit 1.6.0

* Use RandomNumberGenerator.Create() for .NET Core instead of System.Random when generating
  multipart boundaries.

### MimeKit 1.4.2

* Strong-name the .NET Core assemblies.
* Fixed logic for selecting certificates from the Windows X.509 Store. (issue #262)

### MimeKit 1.4.1

* Fixed QuotedPrintableDecoder to handle soft breaks that fall on a buffer boundary.
* Fixed MimeMessage.WriteTo() to properly respect the FormatOptions when writing the
  message headers.
* Updated TextFormat to contain a Plain value (Text is now an alias) to hopefully make
  its mapping to text/plain more obvious.
* Added new TextPart .ctor that takes a TextFormat argument so that developers that
  don't understand mime-types can more easily intuit what that argument should be.

### MimeKit 1.4.0

* Added support for .NET Core 1.0
* Changed the default value of FormatOptions.AllowMixedHeaderCharsets to false.
* Added a new DkimSigner .ctor that takes a stream of key data. (issue #255)

### MimeKit 1.2.25

* Fixed parsing bugs in MessageDeliveryStatus.StatusGroups. (issue #253)
* Fixed MimeParser.ParseHeaders() to handle header blocks that do not end with a blank line. (issue #250)
* Fixed the MailboxAddress parser to handle whitespace between '<' and the addr-spec.
* Fixed TemporarySecureMimeContext to handle certificates with null email addresses. (issue #252)

### MimeKit 1.2.24

* Modified MimeMessage .ctor to not add an empty To: header by default. (issue #241)
* Modified MimeMessage to remove address headers when all addresses in that field are removed.
* Properly apply SecurityCriticalAttribute to GetObjectData() on custom Exceptions.
* Fixed TnefPropertyReader to convert APPTIME values into DateTimes from the OLE Automation
  Date format. (issue #245)

### MimeKit 1.2.23

* Modified ParamaterList.TryParse() to handle quoted rfc2231-encoded param values. (issue #239)
* Updated to reference BouncyCastle via NuGet packages rather than bundling the assemblies.
* Fixed MimeParser to set a multipart's raw epilogue to null instead of an empty byte array.
  Fixes some issues with digital signature verification (as well as DKIM verification).
* Added an HtmlWriter.WriteText() override with Console.WriteLine() style params.
* Added convenience MimeMessage property for the X-Priority header.
* Fixed MimeMessage.ConvertFromMailMessage() to use appropriate MimeEntity subclasses. (issue #232)

### MimeKit 1.2.22

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

### MimeKit 1.2.21

* Added a MultipartReport class for multipart/report.
* Fixed serialization for embedded message/* parts. (issue #228)
* Fixed MimeMessage.WriteTo() to only make sure that the stream ends with a newline if it
  wasn't parsed. (issue #227)
* Fixed MimeMessage to only set a MIME-Version if the message was not produced by the parser.
* Ignore timezones outside the range of -1200 to +1400.
* Added InternetAddress.Clone() to allow addresses to be cloned.
* Properly serialize message/rfc822 parts that contain an mbox marker.
* Fixed MimeMessage.DkimSign() to not enforce 7bit encoding of the body. (issue #224)
* Fixed ParameterList.IndexOf(string) to be case insensitive.

### MimeKit 1.2.20

* Fixed serialization of mime parts with empty content. (issue #221)
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
  when they get reserialized, they match the original. (issue #218)
* Implemented an optimized OrdinalIgnoreCase string comparer which improves the performance
  of the MimeParser slightly.
* Fixed QuotedPrintableDecoder to properly handle "==" sequences.
* Added a ContentDisposition.TryParse(ParserOptions,string) method.
* Added a ContentType.TryParse(ParserOptions,string) method.
* Fixed MimeParser to trim the CR from the mbox From marker.
* Fixed SqlCertificateDatabase to properly chain Dispose.

### MimeKit 1.2.19

* Handle illegal Content-Id headers that do not enclose their values in <>'s. (issue #215)
* Fixed reserialization of MimeParts with empty content. (issue #213)
* Improved parsing logic for malformed Content-Type headers.
* Fixed HtmlTokenizer to work properly when some closing tags were not lowercase.
* Bumped Bouncy Castle to v1.8.1.

### MimeKit 1.2.18

* Removed unimplemented TNEF APIs.
* Use DateTime.UtcNow for S/MIME certificate validity checks.
* Added ToString() methods on ContentType/Disposition that take FormatOptions.
* Added a new ToString() method to InternetAddress that takes a FormatOptions. (issue #208)
* Added a MimeEntity.WriteTo() method that takes a bool contentOnly parameter. (issue #207)
* Added support for encoding parameter values using rfc2047 encoded-words instead of
  the standard rfc2231 encoding.
* Fixed SecureMailboxAddress's Fingerprint property to work with both the PGP key ID
  *and* the fingerprint. Previously only worked with the PGP key id. (issue #203)
* Added GroupAddress.Parse() and MailboxAddress.Parse() methods. (issue #197)
* Set a default filename when generating application/pgp-signature parts. (issue #195)

### MimeKit 1.2.17

* Fixed DkimRelaxedBodyFilter to properly handle CRLF split across buffers.
* Added ContentType.IsMimeType method to replace CongtentType.Matches.
* Added S/MIME, PGP and DKIM support to the PCL and WindowsUniversal versions of MimeKit.
* Fixed PGP key expiration calculation when encrypting. (issue #194)

### MimeKit 1.2.16

* Fixed relaxed body canonicalization logic for DKIM signatures. (issue #190)

### MimeKit 1.2.15

* Fixed the Date parser to catch exceptions thrown by the DateTimeOffset .ctor if any of the
  fields are out of range.
* Fixed logic for trimming trailing blank lines for the DKIM relaxed body algorithm. (issue #187)
* Fixed DKIM body filters to reserve extra space in the output buffer. (issue #188)
* Allow specifying a charset encoding for each Content-Type/Disposition parameter.

### MimeKit 1.2.14

* Fixed DKIM-Signature signing logic to use a UTC-based timestamp value rather than a
  timestamp based on the local-time. (issue #180)
* Fixed Multipart epilogue parsing and serialization logic to make sure that serializing
  a multipart is properly byte-for-byte identical to the original text. This fixes a
  corner-case that affected all types of digital signatures (DKIM, PGP, and S/MIME)
  spanning across nested multiparts. (issue #181)
* Fixed MimeMessage.WriteTo() to ensure that the output stream always ends with a new-line.

### MimeKit 1.2.13

* Modified Base64Encoder's .ctor to allow specifying a maxLineLength.
* Fixed DKIM signing logic for multipart/alternative messages. (issue #178)

### MimeKit 1.2.12

* Prevent infinite loop when flushing CharsetFilter when there is no input data left.

### MimeKit 1.2.11

* Fixed an IndexOutOfRangeException bug in the TextToHTML converter logic. (issue #165)
* Fixed the DKIM-Signature verification logic to be more lenient in parsing DKIM-Signature
  headers. (issue #166)
* Fixed the DKIM-Signature verification logic to error-out if the h= parameter does not
  include the From header. (issue #167)
* Fixed the DKIM-Signature verification logic to make sure that the domain-name in the i=
  param matches (or is a subdomain of) the d= value. (issue #169)
* Fixed the CharsetFilter to avoid calling Convert() on empty input.
* Fixed logic for canonicalizing header values using the relaxed DKIM algorithm.
  (issue #171)
* Fixed AttachmentCollection to mark embedded parts as inline instead of attachment.
* Fixed the DKIM-Signature logic (both signing and verifying) to properly canonicalize the
  body content. (issue #172)

### MimeKit 1.2.10

* Added public Stream property to IContentObject.
* Implemented a better fix for illegal unquoted multi-line Content-Type and
  Content-Disposition parameter values. (issue #159)
* Fixed the UrlScanner to properly handle "ftp." at the very end of the message text.
  (issue #161)
* Fixed charset handling logic to not override charset aliases already in the cache.

### MimeKit 1.2.9

* Fixed WriteTo(string fileName) methods to overwrite the existing file. (issue #154)
* Updated InternetAddressList to implement IComparable.
* Fixed DKIM-Signature generation and verification.
* Added support for Message-Id headers that do not properly use encapsulate the value
  with angle brackets.

### MimeKit 1.2.8

* Added a new MessageDeliveryStatus MimePart subclass to make message/delivery-status
  MIME parts easier to deal with.
* Improved HtmlTokenizer's support for the script tag - it is should now be completely
  bug free.
* Fixed to filter out duplicate recipients when encrypting for S/MIME or PGP.
* Fixed MimeParser to handle a message stream of just "\r\n".
* Add a leading space in the Sender and Resent-Sender header values.

### MimeKit 1.2.7

* Fixed encoding GroupAddress with multiple mailbox addresses.
* Fixed MessageIdList to be less strict in what it will accept.
* Fixed logic for DKIM-Signature header folding.

### MimeKit 1.2.6

* Fixed a bug in the HTML tokenizer to handle some weird HTML created by Outlook 15.0.
* Added CmsRecipient .ctor overloads that accept X509Certificate2. (issue #149)

### MimeKit 1.2.5

* Changed BodyParts and Attachments to be IEnumerable<MimeEntity> -
  WARNING! This is an API change! (issue #148)
* Moved the IsAttachment property from MimePart down into MimeEntity.
* Added MimeMessage.Importance and MimeMessage.Priority properties.
* Vastly improved the HtmlToHtml text converter with a w3 compliant
  HTML tokenizer.

### MimeKit 1.2.4

* Added support for generating and verifying DKIM-Signature headers.
* Improved error handling for Encoding.GetEncoding() in CharsetFilter constructors.
* Fixed buffering in the HTML parser.
* Fixed Windows and Temporary S/MIME contexts to use case-insensitive address
  comparisons like the other backends do. (issue #146).
* Added HeaderList.LastIndexOf() convenience methods.
* Added a new Prepare() method to prepare a message or entity for transport
  and/or signing (used by MultipartSigned and MailKit.SmtpClient) to reduce
  duplicated code.
* Fixed FilteredStream.Flush() to flush filters even when no data has been
  written.
* Fixed the ChainedStream.Read() logic. (issue #143)
* Added EncoderFilter and DecoderFilter.Create() overloads that take an encoding
  name (string).
* HeaderList.WriteTo() now adds a blank line to the end of the output instead
  of leaving this up to the MimeEntity.WriteTo() method. This was needed for
  the DKIM-Signatures feature.

### MimeKit 1.2.3

* Fixed TextToFlowed logic that stripped trailing spaces.
* Switched to PCL Profile78 to support Xamarin.Forms.

### MimeKit 1.2.2

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

### MimeKit 1.2.1

* Added a Format property to ContentType.
* Added a TryGetValue() method to ParameterList.
* Added IsFlowed and IsRichText convenience properties to TextPart.
* Fixed the HtmlToHtml converter to properly handle HTML text that begins
  with leading text data.
* Fixed MimeParser.ParseHeaders() to handle input that does not end with a
  blank line. (issue #142)
* Renamed MimeEntityConstructorInfo to MimeEntityConstructorArgs.
* Modified the MimeParser to use TextPart to represent application/rtf.

### MimeKit 1.2.0

* Force the use of the rfc2047 "B" encoding for ISO-2022-JP. (issue #139)
* Added some text converters to convert between various text formats
  including format=flowed and HTML.

### MimeKit 1.0.15

* Fixed MimeMessage.WriteTo() to be thread-safe. (issue #138)

### MimeKit 1.0.14

* Added support for .NET 3.5.
* Added a convenience CmsSigner .ctor that takes an X509Certificate2 argument.
* Fixed BodyBuilder to never return a TextPart w/ a null ContentObject.
* Fixed TextPart.GetText() to protect against NullReferenceExceptions if the
  ContentObject is null.
* Fixed MimeFilterBase.EnsureOutputSize() to initialize OutputBuffer if it is
  null. Prevents NullReferenceExceptions in obscure corner cases. (issue #135)
* Added a TnefAttachFlags enum which is used to determine if image attachments
  in MS-TNEF data are meant to have a Content-Disposition of "inline" when
  extracted as MIME attachments. (issue #129)
* Fixed TnefPart.ConvertToMessage() and ExtractAttachments() to use the
  PR_ATTACH_MIME_TAG property to determine the intended mime-type for extracted
  attachments.
* Catch DecoderFallbackExceptions in MimeMessage.ToString() and fall back to
  Latin1. (issue #137)

### MimeKit 1.0.13

* Added a work-around for a bug in Thunderbird's multipart/related implementation.
  (issue #124)
* Improved MimeMessage.CreateFromMailMessage() a bit more to avoid creating empty
  From, Reply-To, To, Cc and/or Bcc headers.
* Modified the HeaderIdExtensions to only be available for the HeaderId enum values.

### MimeKit 1.0.12

* Modified InternetAddressList.Equals() to return true if the lists contain the same
  addresses even if they are in different orders. (issue #118)
* Allow S/MIME certificates with the NonRepudiation key usage to be used for signing.
  (issue #119)
* Don't change the Content-Transfer-Encoding of MIME parts being encrypted as part of
  a multipart/encrypted. (issue #122)
* Fixed logic to decide if a PGP secret key is expired. (issue #120)
* Added support for SecureMailboxAddresses to OpenPgpContext to allow key lookups by
  fingerprints instead of email addresses.

### MimeKit 1.0.11

* Added the ContentDisposition.FormData string constant.
* Allow the ContentDisposition.Disposition property to be set to values other than
  "attachment" and "inline". (issue #112)
* Shortened the length of the local-part of auto-generated Message-Ids.
* Fixed MimeMessage.CreateFromMailMessage() to not duplicate From/To/Cc/etc addresses
  if the System.Net.Mail.MailMessage has been sent via System.Net.Mail.SmtpClient
  prior to calling MimeMessage.CreateFromMailMessage(). (issue #115)
* When parsing S/MIME digital signatures, don't import the full certificate chain.
  (issue #110)
* Added immutability-friendly .ctor to MimeMessage for use with languages such as F#.
  (issue #116)

### MimeKit 1.0.10

* Ignore semi-colons in Content-Transfer-Encoding headers to work around broken mailers.
* Added ParserOptions.ParameterComplianceMode (defaults to RfcComoplianceMode.Loose)
  which works around unquoted parameter values in Content-Type and Content-Disposition
  headers. (issue #106)
* Modified the MimeParser to handle whitespace between header field names and the ':'.
* Probe to make sure that various System.Text.Encodings are available before adding
  aliases for them (some may not be available depending on the platform).
* Added a MimePart.GetBestEncoding() overload that takes a maxLineLength argument.
* Modified MultipartSigned to use 78 characters as the max line length rather than 998
  characters. (issue #107)

### MimeKit 1.0.9

* Added a new MessageDispositionNotification MimePart subclass to represent
  message/disposition-notification parts.
* Fixed the TNEF parser to gracefully deal with duplicate attachment properties.

### MimeKit 1.0.8

* Modified the parser to accept Message-Id values without a domain (i.e. "<local-part@>").
* Fixed a NullReferenceException in MimeMessage.BodyParts in cases where a MessagePart
  has a null Message.
* Renamed DateUtils.TryParseDateTime() to DateUtils.TryParse() (the old API still exists
  but has been marked [Obsolete]).
* Renamed MimeUtils.TryParseVersion() to MimeUtils.TryParse() (the old API still exists
  but has been marked [Obsolete]).
* Fixed S/MIME support to gracefully deal with badly formatted signature timestamps
  which incrorectly use leap seconds. (issue #103)

### MimeKit 1.0.7

* Fixed TnefPropertyReader.GetEmbeddedMessageReader() to skip the Guid.
* When decrypting PGP data, iterate over all encrypted packets to find one that
  can be decrypted (i.e. the private key exists in the user's keychain).
* Updated WindowsSecureMimeContext to respect SecureMailboxAddresses like the
  other backends. (issue #100)
* Added a Pkcs9SigningTime attribute to the CmsSigner for WindowsSecureMimeContext.
  (issue #101)

### MimeKit 1.0.6

* Vastly improved MS-TNEF support. In addition to being fixed to properly extract
  the AttachData property of an Attachment attribute, more metadata is captured
  and translated to the MIME equivalents (such as attachment creation and
  modification times, the size of the attachment, and the display name).
* Migrated the iOS assemblies to Xamarin.iOS Unified API for 64-bit support.

Note: If you are not yet ready to port your iOS application to the Unified API,
      you will need to stick with the 1.0.5 release. The Classic MonoTouch API
      is no longer supported.

### MimeKit 1.0.5

* Fixed out-of-memory error when encoding some long non-ASCII parameter values in
  Content-Type and Content-Disposition headers.

### MimeKit 1.0.4

* Added workaround for msg-id tokens with multiple domains
  (e.g. id@domain1@domain2).
* Added convenience methods to Header to allow the use of charset strings.
* Added more HeaderList.Replace() method overloads for convenience.
* Added a FormatOptions property to disallow the use of mixed charsets when
  encoding headers (issue #139).

### MimeKit 1.0.3

* Improved MimeMessage.TextBody and MimeMessage.HtmlBody logic. (issue #87)
* Added new overrides of TextPart.GetText() and SetText() methods that take a
  charset string argument instead of a System.Text.Encoding.
* Fixed charset fallback logic to work properly (it incorrectly assumed that
  by default, Encoding.UTF8.GetString() would throw an exception when it
  encountered illegal byte sequences). (issue #88)
* Fixed S/MIME logic for finding X.509 certificates to use for encipherment.
  (issue #89)

### MimeKit 1.0.2

* Fixed MimeMessage.HtmlBody and MimeMessage.TextBody to properly
  handle nested multipart/alternatives (only generated by automated
  mailers).

### MimeKit 1.0.1

* Added MimeMessage.HtmlBody and MimeMessage.TextBody convenience properties.
* Added TextPart.IsPlain and TextPart.IsHtml convenience properties.

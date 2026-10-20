# Porting from MimeKit 4.x to MimeKit 5.0

MimeKit 5.0 is a major release with a number of breaking changes. This guide explains how to port code
written against MimeKit 4.x (4.18.1 and earlier). The [release notes](ReleaseNotes.md) list every change;
this guide covers only the ones that require changes to your code or that change behavior you may rely on.

The guide is written for people and for automated (AI) refactoring tools. Most sections start with a
mapping table that can be applied mechanically, followed by the semantic changes that need a person
(or an agent) to review them. The [checklist](#porting-checklist) at the end summarizes everything.

## Contents

1. [Packages and assemblies](#packages-and-assemblies)
2. [S/MIME and OpenPGP](#smime-and-openpgp)
3. [DKIM and ARC](#dkim-and-arc)
4. [MimeParser and MimeReader](#mimeparser-and-mimereader)
5. [Message-ids, addresses and other MIME APIs](#message-ids-addresses-and-other-mime-apis)
6. [HTML](#html)
7. [Removed APIs](#removed-apis)
8. [TNEF](#tnef)
9. [Porting checklist](#porting-checklist)

## Packages and assemblies

The cryptography support (S/MIME, PGP/MIME, DKIM, ARC and the new DMARC support) has been split out of the
core library into a separate assembly.

| 4.x | 5.0 |
|---|---|
| `MimeKit` package (assembly `MimeKit.dll`) | `MimeKit` meta-package, which references `MimeKit.Core` (assembly `MimeKit.Core.dll`) and `MimeKit.Cryptography` |
| `MimeKitLite` package | `MimeKit.Core` package |

- Projects that reference the `MimeKit` package continue to get everything and usually need no changes.
- Projects that don't need cryptography can reference `MimeKit.Core` on its own.
- Namespaces are unchanged: the core types are still in `MimeKit`, `MimeKit.IO`, `MimeKit.Text` and so
  on, and the cryptography types are still in `MimeKit.Cryptography`.
- Anything that refers to the assembly by name must be updated, because the core types now live in
  `MimeKit.Core`. This includes assembly-qualified type names in configuration files, `[InternalsVisibleTo]`
  workarounds, linker/trimmer descriptors and binding redirects.

## S/MIME and OpenPGP

| 4.x | 5.0 |
|---|---|
| `message.Sign (...)`, `Encrypt (...)`, `SignAndEncrypt (...)` (instance methods) | The same calls, now extension methods. Add `using MimeKit.Cryptography;`. |
| `MimeVisitor.VisitMultipartSigned ()`, `VisitMultipartEncrypted ()`, `VisitApplicationPkcs7Mime ()`, `VisitApplicationPkcs7Signature ()`, `VisitApplicationPgpEncrypted ()`, `VisitApplicationPgpSignature ()` | Derive from `MimeKit.Cryptography.CryptographicMimeVisitor` and override the same methods there |
| `CryptographyContext.DigestAlgorithmRank`, `EnabledDigestAlgorithms`, `Enable (DigestAlgorithm)`, `Disable (DigestAlgorithm)`, `IsEnabled (DigestAlgorithm)` (also on `ICryptographyContext`) | The same members on `OpenPgpContext` |
| `X509CertificateDatabase.GetSelectCommand (DbConnection connection, ...)` and the other protected `Get*Command`, `GetTableColumns`, `CreateTable`, `AddTableColumn`, `CreateIndex` and `RemoveIndex` methods | The same methods without the `DbConnection` parameter. Use the protected `CreateCommand ()` method to create commands. |
| `IX509CertificateDatabase.Update (X509CrlRecord)`, `X509CertificateDatabase.GetUpdateCommand (DbConnection, X509CrlRecord)` | Removed (obsolete and unused) |
| `AsymmetricAlgorithmExtensions.AsAsymmetricAlgorithm ()` | Removed. `AsAsymmetricKeyParameter ()` and `AsAsymmetricCipherKeyPair ()` are unchanged. |

### Register an S/MIME context

MimeKit 4.x automatically used `DefaultSecureMimeContext` for S/MIME when a SQLite library was available.
To make MimeKit AOT-compatible, MimeKit 5.0 no longer registers any S/MIME context. If you use S/MIME
without registering a context, `CryptographyContext.Create ()` (and therefore the `MultipartSigned`,
`MultipartEncrypted` and `ApplicationPkcs7Mime` methods that don't take a context) throws a
`NotSupportedException`.

Register the context you want during startup:

```csharp
CryptographyContext.Register (typeof (MySecureMimeContext));

// or, to keep the 4.x behavior:
CryptographyContext.Register (() => new DefaultSecureMimeContext ());
```

Alternatively, pass a context explicitly to the methods that accept one.

### Initialize the cryptography module before parsing

The parser in `MimeKit.Core` knows nothing about cryptography. It only creates `MultipartSigned`,
`MultipartEncrypted`, `ApplicationPkcs7Mime`, `ApplicationPkcs7Signature`, `ApplicationPgpEncrypted` and
`ApplicationPgpSignature` entities once `MimeKit.Cryptography` has registered its entity factory. This
happens automatically the first time `CryptographyContext` is used (for example, by
`CryptographyContext.Register ()`). Messages that are parsed before then contain a plain `Multipart` or
`MimePart` instead, so `body is MultipartSigned` checks silently fail.

If your application might parse messages before it touches `CryptographyContext`, call this during
startup:

```csharp
MimeKit.Cryptography.CryptographyModule.Initialize ();
```

### Semantic changes

- **OpenPGP integrity is enforced.** Decryption rejects modified encrypted data and, by default,
  legacy packets without integrity protection. Set `OpenPgpContext.AllowUnauthenticatedDecryption`
  to `true` only if legacy compatibility is necessary; this does not bypass integrity checks on
  protected messages. `DecryptTo ()` and `DecryptToAsync ()` may write unauthenticated output before
  throwing, so do not consume their output until they complete successfully.
- **Temporary S/MIME trust is explicit.** Ordinary imports, including PKCS#12 and message-supplied
  certificates, no longer create trust anchors. Use `temporaryContext.Import (rootCertificate, true)`
  or `ImportAsync (rootCertificate, true)` with a Bouncy Castle `X509Certificate` obtained from a
  trusted source. Passing `false` to these overloads removes trust; ordinary imports preserve any
  trust already configured. Do not mark certificates trusted just because they are self-signed.
- **CMS expansion is bounded.** `SecureMimeContext.MaxDecompressedSize` defaults to 64 MiB and
  includes MIME headers. Set a larger positive byte limit if required. All four decompression APIs
  throw `FormatException` when the limit is exceeded; stream outputs may contain partial data.
  Implementers of `ISecureMimeContext` must also provide this property.
- **CRL retrieval requires a trusted path.** With `CheckCertificateRevocation` enabled, the Bouncy
  Castle contexts first validate a certificate path without revocation checking, fetch CRLs only
  for that path, and then validate with revocation checking. An untrusted chain now fails without
  fetching its distribution points. Configure roots and intermediate certificates locally as needed.
  This is not a network sandbox: use outbound network restrictions appropriate to your deployment.
- **SHA-256 is the default signing digest.** The `Sign ()`, `SignAsync ()`, `SignAndEncrypt ()` and
  `SignAndEncryptAsync ()` overloads that don't take a `DigestAlgorithm` now use SHA-256 instead of SHA-1.
  Pass `DigestAlgorithm.Sha1` explicitly if a recipient really can't handle SHA-256.
- **OpenPGP digest preferences.** The default `OpenPgpContext.DigestAlgorithmRank` now matches GnuPG:
  SHA-512, SHA-384, SHA-256, SHA-224 and SHA-1 (SHA-1 used to be first, and RIPEMD-160 is no longer
  included). This only affects the preferences advertised by keys created with
  `GnuPGContext.GenerateKeyPair ()`.
- **No more binary serialization.** `ParseException`, `TnefException`, `CertificateNotFoundException`,
  `DigitalSignatureVerifyException`, `PrivateKeyNotFoundException` and `PublicKeyNotFoundException` are no
  longer `[Serializable]` and no longer have `(SerializationInfo, StreamingContext)` constructors or
  `GetObjectData ()` overrides. Remove any subclass constructors or overrides that chain to them.

## DKIM and ARC

| 4.x | 5.0 |
|---|---|
| `IDkimPublicKeyLocator`, `DkimPublicKeyLocatorBase` | `IDnsResolver` (see [below](#replace-your-public-key-locator)) |
| `new DkimVerifier (IDkimPublicKeyLocator)`, `new ArcVerifier (IDkimPublicKeyLocator)` | `new DkimVerifier (IDnsResolver)`, `new ArcVerifier (IDnsResolver)` |
| `DkimVerifierBase.PublicKeyLocator` (protected) | `DkimVerifierBase.DnsResolver` |
| `bool DkimVerifier.Verify (...)`, `Task<bool> VerifyAsync (...)` | `DkimSignatureValidationResult Verify (...)`, `Task<DkimSignatureValidationResult> VerifyAsync (...)`. Test `result.Status == DkimSignatureStatus.Pass`. |
| — | `DkimSignatureValidationResult[] Verify (MimeMessage)` and `VerifyAsync`: verify every `DKIM-Signature` header, up to `DkimVerifier.MaxSignatures` (default 10) |
| `new DkimSigner (string domain, string selector, ...)`, `ArcSigner (string domain, string selector, ...)` and `DkimSignerBase (string domain, string selector, ...)` (obsolete) | Removed. Use the constructors that take the private key. |
| `DkimSignerBase.PrivateKey` setter (protected, obsolete) | Removed. Pass the private key to the base class constructor. |

### Replace your public key locator

In 4.x, you implemented `LocatePublicKey (methods, domain, selector)`: you built the
`<selector>._domainkey.<domain>` query name yourself, fetched the TXT record and (usually via
`DkimPublicKeyLocatorBase.GetPublicKey ()`) parsed the key. In 5.0, you only perform the DNS TXT query.
MimeKit builds the query name and parses (and validates) the key record itself.

```csharp
class DnsResolver : IDnsResolver
{
	public DnsTxtResponse QueryTxt (string domain, CancellationToken cancellationToken = default)
	{
		// Perform a TXT query for `domain` (it is already the full query name).
		// Concatenate the character-strings within each TXT record, but return
		// each record as a separate string.
		if (nxdomain)
			return new DnsTxtResponse (DnsQueryStatus.NonExistentDomain);

		if (serverFailureOrTimeout)
			return new DnsTxtResponse (DnsQueryStatus.TemporaryFailure);

		return new DnsTxtResponse (records);
	}

	public Task<DnsTxtResponse> QueryTxtAsync (string domain, CancellationToken cancellationToken = default)
	{
		// ...
	}
}
```

Distinguishing `NonExistentDomain` from `TemporaryFailure` matters: a temporary failure is reported as
`TempError` (DKIM) or `ArcValidationErrors.DnsTemporaryFailure` (ARC) so that receivers can defer the
message rather than fail it. The same `IDnsResolver` is used by the new `DmarcVerifier`. See the
[DkimVerifier sample](samples/DkimVerifier/DkimVerifier/Program.cs) for a complete implementation using
DnsClient.NET.

### Semantic changes

- **Verification no longer throws for bad input.** A malformed `DKIM-Signature` header, a failed key
  lookup, an unparsable key record or an exception thrown by your `IDnsResolver` is reported through the
  result (`PermError` or `TempError`, with `Reason` and `Exception`), not as an exception. Only argument
  errors and cancellation throw. Remove `try`/`catch` blocks that existed to treat `FormatException` or
  `ParseException` as a failed signature.
- **A `bool` is not enough anymore.** `Pass`, `Fail`, `Policy`, `TempError` and `PermError` match the
  RFC 8601 `dkim` results. Use `result.ToAuthenticationMethodResult ()` to build an Authentication-Results
  entry.
- **Stricter key records and signatures.** Key records are parsed according to RFC 6376: records with
  duplicate tags, a misplaced or wrong `v=`, an unknown `k=`, or an `s=` that excludes `email` are ignored;
  `h=` and key-type mismatches are rejected; an empty `p=` is treated as revoked; and `t=s` is enforced.
  Signatures whose `x=` expiration time has passed are now `PermError`. Messages that "verified" with 4.x
  may not verify with 5.0.
- **ARC.** As required by RFC 8617, any error still makes the chain `Fail`, but
  `ArcHeaderValidationResult.Reason`/`Exception` and the new `ArcValidationErrors.DnsTemporaryFailure`
  flag now tell you why.

## MimeParser and MimeReader

| 4.x | 5.0 |
|---|---|
| `ExperimentalMimeParser` | `MimeParser` |
| `MimeParser` | `LegacyMimeParser` (retained for compatibility; only use it if you rely on its exact behavior) |
| `mimeParser.ReadHeaders ()`, `ReadEntity ()`, `ReadMessage ()` (and `Async` variants), inherited from `MimeReader` | Removed. `MimeParser` no longer derives from `MimeReader`. Use `ParseHeaders ()`, `ParseEntity ()` or `ParseMessage ()`, or subclass `MimeReader` directly. |
| `OnMboxMarkerRead (byte[] buffer, int startIndex, int count, long beginOffset, int lineNumber, CancellationToken)` (and `Async`) | `OnMboxMarkerRead (byte[] buffer, int startIndex, int count, CancellationToken)`. The offset and line number are passed to `OnMboxMarkerBegin ()`. |
| `OnMultipartBoundary ()`, `OnMultipartEndBoundary ()` (and `Async`; obsolete) | `OnMultipartBoundaryEnd ()`, `OnMultipartEndBoundaryEnd ()` |

For most code, the parser change is a source-compatible rename: `new MimeParser (stream)` and
`MimeMessage.Load ()` keep working, but are now backed by the new parser.

### Semantic changes

- **`MimeParser` subclasses.** `MimeParser` mirrors all of `MimeReader`'s `On*` callbacks as
  `protected virtual` methods. If you override any of them, you must call the base implementation, because
  that is where `MimeParser` builds the message. `MimeParser` still exposes `Options`, `IsEndOfStream`,
  `Position`, `ComplianceLogger` and `ComplianceOptions`.
- **`MimeReader` content callbacks.** The newline that precedes a multipart boundary marker belongs to the
  boundary (RFC 2046) and is no longer passed to `OnMimePartContentRead[Async] ()` or
  `OnMultipartEpilogueRead[Async] ()`. The bytes passed to these callbacks now exactly match the range
  between the `Begin` and `End` offsets. Remove any code that trimmed the trailing newline itself.
- **Header length limit.** The new `ParserOptions.MaxHeaderLength` (16 MB by default) bounds how much a
  single header can buffer. Longer headers make the parser throw a `FormatException`.
- **Byte-for-byte round trips.** `MimeParser` now re-serializes messages exactly as they were parsed, even
  when they're truncated or don't end with a newline. If you append messages to an mbox file, set
  `FormatOptions.EnsureNewLine` to `true`.
- **Mbox edge cases.** `ParseMessage ()` now throws a `FormatException` for a stream that contains nothing
  but mbox `From ` lines. A `From ` line that is followed by lines that aren't headers is now a parse error,
  where previously that content was swallowed by the mbox marker.

## Message-ids, addresses and other MIME APIs

| 4.x | 5.0 |
|---|---|
| `MessagePartial.Join (ParserOptions, MimeMessage, IEnumerable<MessagePartial>)` (and overloads) | Takes an `IReadOnlyList<MessagePartial>`. Arrays and `List<MessagePartial>` work unchanged; call `.ToList ()` on other sequences. |

### Semantic changes

- **Message-id validation.** The `MimeMessage.MessageId`, `ResentMessageId` and `InReplyTo` setters, the
  `MimeEntity.ContentId` setter and `MimeMessage.References` (`Add`, `AddRange`, `Insert` and the indexer)
  share the same validation. Values that 4.x silently truncated or altered now throw an
  `ArgumentException`: `<>`, a second msg-id or other trailing garbage, unquoted whitespace in a msg-id
  without angle brackets, and line breaks or NUL characters. Angle brackets remain optional.
- **Empty msg-ids.** When parsing, an empty msg-id (`<>`) is ignored: `MessageId`, `ResentMessageId`,
  `InReplyTo`, `ContentId` and `MimeUtils.ParseMessageId ()` return `null` instead of `""`, and
  `References`/`MimeUtils.EnumerateReferences ()` skip it. Check for `null` rather than an empty string.
- **Trimmed display names.** `InternetAddress.Name` (and so `MailboxAddress.Name` and `GroupAddress.Name`) is
  trimmed when set, including by constructors and the parser. A whitespace-only name becomes `""`.
- **IDN-encoded domains are no longer decoded.** The address parser and the `MailboxAddress.Address` setter
  preserve the domain as-is, so `MailboxAddress.Address` and `Domain` may now contain A-labels (e.g.
  `user@xn--bcher-kva.example`). Use `GetAddress (false)` or the new `GetDomain (false)` to get the Unicode form
  for display, and `GetAddress (true)`/`GetDomain (true)` for the ASCII form. `GetAddress (false)` now decodes
  rather than returning the address as-is. `IsInternational` is `false` for an IDN-encoded domain, `Equals ()`
  no longer treats the A-label and Unicode forms as equal, and `FormatOptions.International` no longer causes an
  IDN-encoded domain to be written as Unicode. To validate the Unicode form of a parsed address with
  `SmtpUtf8AddressValidator`, pass `SmtpUtf8AddressValidationOptions.DecodeIdn`.
- **`MessagePartial.Join ()`** validates the `id` and `number` of every partial up front, so a mismatched
  `id` or a missing `number` throws `InvalidOperationException` even for a single partial.
- **`MimeEntity.WriteTo ()` is no longer virtual.** `WriteTo (FormatOptions, Stream, bool, CancellationToken)`
  and `WriteToAsync ()` can no longer be overridden; a non-recursive writer now writes the entity and all of its
  descendants. Customize a subclass's output through its headers or content instead.

### Disposing messages and entities

`MimeMessage` and `MimeEntity` still implement `IDisposable`; existing `using` statements and calls to
`Dispose ()` remain valid. MimeKit now walks structural ownership iteratively, with storage proportional
to nesting depth: a message owns its `Body`, a multipart owns its children in list order, and a
`MessagePart` owns its `Message`. Each object's `Dispose (bool)` override runs **after** cleanup has been
attempted for its structural descendants.

Update subclasses so that `Dispose (bool)` releases only resources owned directly by that object and
calls `base.Dispose (disposing)`. Do not dispose `Body`, multipart children or `MessagePart.Message`
there; traversal no longer happens inside `base.Dispose (disposing)`. In particular, code before the
base call can no longer assume that children are still usable. Custom-owned resources are still the
override's responsibility; stack safety does not extend to arbitrary recursive application code.

If cleanup throws, MimeKit still attempts the remaining siblings and parent cleanup hooks. A single
exception is rethrown; multiple exceptions are reported in an `AggregateException` in traversal order.
Calling `Dispose ()` again on the root retries incomplete branches and failed hooks without repeating
successful hooks. A failed hook may have partially released its resources, so it must tolerate retry.
Successful local cleanup suppresses finalization even if a descendant failed; failed local cleanup
does not suppress finalization.

Treat the tree as unusable once disposal has begun, even if disposal throws. Retain ownership links for
retry: do not clear, replace or otherwise mutate them during or after disposal, and do not dispose the
same tree concurrently. Reentrant disposal of an object already being disposed is ignored. An ownership
cycle is reported as an `InvalidOperationException`, while other cleanup is still attempted.

## HTML

| 4.x | 5.0 |
|---|---|
| `HtmlTokenizerState.AttributeValueQuoted` | `AttributeValueDoubleQuoted` or `AttributeValueSingleQuoted`. The numeric values of the members that follow have changed, so don't persist them. |
| `HtmlToHtml.FilterHtml` (obsolete) | Removed. Use `HtmlTagCallback` with an allow-list, or a dedicated sanitizer such as HtmlSanitizer. |
| — | `HtmlToHtml.NoScriptHandling`, `HtmlTokenizer.ScriptingEnabled` |
| — | `HtmlTokenizer.MaxElementDepth`, `HtmlToHtml.MaxElementDepth` (4096 by default) |

### Semantic changes

- **`<noscript>` is now filtered.** By default (`HtmlNoScriptHandling.Unwrap`), `HtmlToHtml` tokenizes the
  content of `<noscript>` elements as markup, passes it to your `HtmlTagCallback` and removes the
  `<noscript>` tags themselves. In 4.x the content was copied verbatim without being passed to the callback,
  which let tags inside `<noscript>` bypass filtering when the output was rendered with scripting disabled
  (as is typical for email). `HtmlNoScriptHandling.ScriptingEnabled` restores the 4.x behavior; only use
  it if the output is always rendered with scripting enabled.
- **Tokenization matches browsers.** `HtmlTokenizer` now conforms to the html5lib tokenizer tests. Your
  `HtmlTagCallback` may now see tags that 4.x treated as text (and vice versa), CDATA sections are only
  recognized inside SVG and MathML, and `<img/onerror=...>` has an `onerror` attribute. If you filter HTML
  with `HtmlToHtml`, this makes the filtering more reliable; review any code that matched the old quirks.
- **Character references.** Numeric character references follow the HTML specification (any number of
  digits, an optional `;`, out-of-range values become U+FFFD), and legacy named references without a `;` in
  attribute values are left alone when followed by `=` or an alphanumeric (as in `href="?a=1&not=2"`). This
  also affects `HtmlEntityDecoder` and `HtmlUtils.HtmlDecode ()`.
- **Element depth limit.** Once more than `MaxElementDepth` distinct elements are open, the rest of the
  input is returned as character data, which `HtmlToHtml` writes as encoded text.
- **`HtmlDocTypeToken` setters** now throw an `ArgumentException` for values that can't be written as a
  well-formed DOCTYPE (for example, a name that contains whitespace or `>`).

## Removed APIs

These APIs were removed without a direct replacement (most were already obsolete in 4.x):

| 4.x | 5.0 |
|---|---|
| `QEncoder` | Removed. Use `Rfc2047.EncodePhrase ()` or `Rfc2047.EncodeText ()` to encode header text. |
| `HexEncoder`, `HexDecoder` | Removed (`HexDecoder` is now internal) |
| `TnefPropertyTag.Puid` | `TnefPropertyTag.PuidA` or `TnefPropertyTag.PuidW` |

The obsolete members of `MimeReader`, `DkimSigner`, `ArcSigner`, `DkimSignerBase`, `HtmlToHtml` and
`IX509CertificateDatabase` are covered in the sections above.

## TNEF

MimeKit 5.0 replaces the `MimeKit.Tnef` API. The reader was rewritten against the published
[MS-OXTNEF], [MS-OXCDATA] and [MS-OXCMAIL] specifications, a `TnefMessage` object model was added, and
`TnefPart.ConvertToMessage ()` was replaced by a spec-driven MIME converter. The
[TNEF porting checklist](#tnef-porting-checklist) summarizes this section.

### Choosing a TNEF API

MimeKit 5.0 offers three levels of TNEF API. Most 4.x code reached for `TnefReader` only because there
was nothing between it and `ConvertToMessage ()`, and can now use a higher level.

| If your 4.x code... | Use in 5.0 |
|---|---|
| called `TnefPart.ConvertToMessage ()` or `TnefPart.ExtractAttachments ()` | `TnefPart.LoadTnefMessage ()` + `TnefMessage.ConvertToMime ()` |
| walked attributes to collect the subject, recipients, bodies or attachments | `TnefMessage` (`Properties`, `Recipients`, `TextBody`/`HtmlBody`/`RtfBody`, `Attachments`) |
| needed attribute-level detail (offsets, checksums, unknown attributes, streaming) | `TnefReader` + `TnefPropertyReader` |
| needed to produce TNEF (4.x could not) | `TnefWriter` + `TnefPropertyWriter` (see [Write a TNEF stream](#write-a-tnef-stream)) |

`TnefMessage` buffers bodies and attachment content in memory, subject to the limits in `TnefOptions`.
`TnefReader` streams and does not buffer.

### Removed and renamed TNEF types

| 4.x | 5.0 |
|---|---|
| `TnefComplianceMode` (`Loose`, `Strict`) | Removed. The reader is always "loose"; see [Compliance](#compliance-tnefcompliancestatus-and-tnefcompliancemode). |
| `TnefComplianceStatus` (flags) | Removed. Use `ITnefComplianceLogger`, `TnefComplianceIssue` and `TnefComplianceViolation`. |
| `TnefPropertyId.InternetCPID` | `TnefPropertyId.InternetCodepage` |
| `TnefPropertyTag.InternetCPID` | `TnefPropertyTag.InternetCodepage` |
| `TnefPropertyTag.Puid` | Removed (obsolete); use `TnefPropertyTag.PuidA` or `TnefPropertyTag.PuidW`. |

New types: `TnefOptions`, `ITnefComplianceLogger`, `TnefComplianceIssue`, `TnefComplianceViolation`,
`TnefMessage`, `TnefAttachment`, `TnefRecipient`, `TnefRecipientType`,
`TnefMessageBody`, `TnefMessageBodyFormat`, `TnefPropertySet`, `TnefProperty`, `TnefPropertySetGuid`,
`TnefConversionOptions`, `TnefConversionResult`, `TnefConversionLoss` and `TnefConversionLossKind`.
`TnefAttributeType` (internal in 4.x) is now public.
`TnefNameId` gained static fields for well-known named properties (for example `TnefNameId.Location`).

### TnefPart

| 4.x | 5.0 |
|---|---|
| `MimeMessage ConvertToMessage ()` | `using var tnef = part.LoadTnefMessage ();` then `using var result = tnef.ConvertToMime ();` and use `result.Message` |
| `IEnumerable<MimeEntity> ExtractAttachments ()` | `result.Message.BodyParts` or `result.Message.Attachments` (see [below](#replacing-extractattachments)), or `TnefMessage.Attachments` |
| — | `Task<TnefMessage> LoadTnefMessageAsync (TnefOptions?, CancellationToken)` |

The same members were removed from and added to `ITnefPart`.

#### Ownership and disposal

- `LoadTnefMessage ()` returns a `TnefMessage` that is independent of the `TnefPart` and must be disposed.
- `ConvertToMime ()` returns a `TnefConversionResult`. Disposing it disposes `result.Message`. To keep
  the message, take a reference to `result.Message` and do not dispose the result.
- The converted `MimeMessage` does not depend on the `TnefMessage`, so the `TnefMessage` may be disposed
  as soon as `ConvertToMime ()` returns.

#### Conversion behavior changes

`ConvertToMime ()` follows [MS-OXCMAIL]. Differences from 4.x `ConvertToMessage ()` that may affect
callers:

- **PidTagMimeSkeleton.** If the message has a MIME skeleton (common for mail that passed through
  Exchange), the skeleton's MIME structure and headers are used and filled in with the TNEF bodies and
  attachments. 4.x ignored it.
- **Headers.** The Received headers from `PidTagTransportMessageHeaders` come first, then the headers
  derived from MAPI properties, then the headers stored in `PS_INTERNET_HEADERS` named properties.
  No header is fabricated: there is no generated Message-Id and no empty From or Subject header.
- **Dates.** `PT_SYSTIME` values are UTC (see [TnefPropertyReader](#tnefpropertyreader)), so the Date
  header no longer depends on the time zone of the machine that ran the conversion.
- **Structure.** When there is more than one body, the bodies become a `multipart/alternative`
  (`text/plain`, then `text/html`). Which bodies are used is decided by the [MS-OXBBODY] best body
  algorithm (`PidTagNativeBody`, then `PidTagRtfInSync`). When the best body is RTF, both alternatives
  are generated from the compressed RTF with `RtfToText` and `RtfToHtml` ([MS-OXCMAIL] 2.1.3.3.5);
  the `PidTagBody` and `PidTagHtml` values are not used. Otherwise the plain text and HTML bodies are
  used as they are, and the out-of-date RTF body is dropped. A `text/rtf` part is never produced.
  In 4.x, `ConvertToMessage ()` added every body property it found (`text/rtf`, `text/html` and
  `text/plain`) to the `multipart/alternative`, in the order the properties appeared in the TNEF
  stream, without considering which body was authoritative. The inline attachments that the HTML
  body references are grouped with the body in a `multipart/related` ([MS-OXCMAIL] 2.1.3.3.6). The
  other attachments follow in a `multipart/mixed`. A message with no body and one attachment is still
  wrapped in a `multipart/mixed`.
- **Inline attachments.** Inline status follows the [MS-OXCMAIL] 2.1.3.4.1 "best body" rules. When there
  is an HTML body, attachments flagged `afRenderedInBody` that the HTML references by `cid:` or
  `Content-Location` are inline. When the best body is RTF, the attachments are matched, in
  `PidTagRenderingPosition` order, with the RTF's `\objattph` placeholders ([MS-OXRTFEX] 2.2.3.4,
  [MS-OXCMAIL] 2.1.3.4.1.1). Hidden attachments and those with a rendering position of -1 are skipped.
  Attachments whose content is an image a browser can display (PNG, JPEG, GIF, BMP or WebP) are given a
  `Content-Id`, are referenced by an `<img src="cid:...">` in the generated HTML, and are inline. If the
  numbers of placeholders and attachments differ, the images are appended to the end of the HTML body
  instead. Other attachments, including OLE objects, are not inline. In 4.x, OLE attachments were always
  inline. If the RTF encapsulates HTML ([MS-OXRTFEX]), the extracted HTML is the HTML body, and the HTML
  rules apply to it instead of the placeholders. Inline attachments get `Content-Disposition: inline`, so
  `MimeMessage.Attachments` does not list them.
- **RTF attachment placeholders.** By default, the generated plain-text body has nothing at an attachment
  placeholder, and the HTML body has an image or nothing. Set
  `TnefConversionOptions.AttachmentPlaceholderCallback` to return text (for example, the attachment's
  file name) to insert at the placeholder. The text is HTML-encoded in the HTML body, where it is used
  only for attachments that are not displayed as images.
- **OLE objects.** OLE attachments are kept as-is unless `TnefConversionOptions.OleObjectConverter` is
  set. A `TnefOleObjectConverter` can render an OLE object as an image. The image replaces the OLE
  attachment and is named after its display name ([MS-OXCMAIL] 2.1.3.4.4). If the converter returns
  `null` or content that is not an image, the OLE attachment is kept. Any OLE attachment that is kept
  is reported as a `TnefConversionLossKind.OleObjectNotRendered` loss.
- **Embedded messages.** As in 4.x, embedded messages are added as `application/ms-tnef` `TnefPart`s by
  default. Set `TnefConversionOptions.ConvertEmbeddedMessages = true` to get `message/rfc822`
  `MessagePart`s instead.
- **Losses.** Anything that could not be represented in MIME is listed in `result.Losses`
  (`TnefConversionLoss.Kind` and `Description`) instead of being silently dropped.
- **Unsafe content types.** An attachment that claims to be `multipart/*`, `message/*`,
  `application/applefile` or `application/mac-binhex40`, or that has an invalid MIME type, is emitted as
  `application/octet-stream`.

#### Replacing ExtractAttachments

4.x `ExtractAttachments ()` returned the body alternatives *and* the attachments as a flat list. To get
the same set of entities:

```csharp
using var tnef = part.LoadTnefMessage ();
using var result = tnef.ConvertToMime ();

// Everything (bodies + attachments), like 4.x ExtractAttachments ():
foreach (var entity in result.Message.BodyParts) {
	// ...
}

// Only the attachments (Content-Disposition: attachment):
foreach (var attachment in result.Message.Attachments) {
	// ...
}
```

The entities belong to `result.Message`. Either finish with them before `result` is disposed, or do not
dispose `result` until you are done with them.

If you only need the raw attachment bytes, skip MIME conversion entirely and use
`TnefMessage.Attachments` (see [Extract attachments](#extract-attachments)).

### TnefReader

`TnefReader` is now `sealed`.

| 4.x | 5.0 | Notes |
|---|---|---|
| `TnefReader (Stream)` | `TnefReader (Stream stream, TnefOptions? options = null, bool leaveOpen = false)` | |
| `TnefReader (Stream, int defaultMessageCodepage, TnefComplianceMode)` | `new TnefReader (stream, new TnefOptions { DefaultCodepage = codepage })` | Compliance mode has no replacement; see [Compliance](#compliance-tnefcompliancestatus-and-tnefcompliancemode). |
| `MaxNestingDepth` | `TnefOptions.MaxNestingDepth` | |
| `bool ReadNextAttribute ()` | `bool Read (CancellationToken = default)` / `ReadAsync` | |
| `AttributeLevel` | `Level` | |
| `AttributeTag` | `Tag` | |
| — | `AttributeType` | Type encoded in the attribute id. |
| `AttributeRawValueLength` | `Length` | |
| `AttributeRawValueStreamOffset` | `StreamOffset + 9` | `StreamOffset` is now the offset of the attribute *header* (1-byte level, 4-byte id, 4-byte length). |
| `StreamOffset` (`int`, current read position) | `StreamOffset` (`long`, start of current attribute) | Meaning changed. Offsets of embedded readers are relative to the outermost stream. |
| `int ReadAttributeRawValue (byte[], int, int)` | `Stream OpenValueStream ()` then `Read` | |
| `TnefPropertyReader` property | `GetPropertyReader ()` | Only valid for `MapiProperties`, `Attachment` and `RecipientTable` attributes; throws `InvalidOperationException` otherwise. |
| `AttachmentKey` (`short`) | `LegacyKey` (`ushort`) | |
| `MessageCodepage` | `Codepage` | An `attOemCodepage` of `0` is now ignored, and an unsupported one keeps `TnefOptions.DefaultCodepage` rather than switching to windows-1252. If the stream has no `attOemCodepage`, the message's `PidTagInternetCodepage` is used from the point where it is read ([MS-OXTNEF] 2.3.3.2). |
| `TnefVersion` | Read the `TnefAttributeTag.TnefVersion` attribute with `ReadValueAsInt32 ()`. | A non-`0x00010000` version is reported as `UnsupportedVersion`. |
| `ComplianceMode` | — | Removed. |
| `ComplianceStatus` | `ComplianceLogger` | See [Compliance](#compliance-tnefcompliancestatus-and-tnefcompliancemode). |
| `ResetComplianceStatus ()` | — | Removed; use a new logger. |
| — | `ComplianceOptions` | A `TnefComplianceOptions`; its `MaxIssuesPerViolation` caps repeated reports. Set this when reading untrusted input. |
| — | `Depth` | Embedded-message nesting depth (0 for the outermost stream). |
| — | `Options` | The `TnefOptions` the reader was created with. |
| `Close ()` | `Dispose ()` | |
| `Dispose ()` | `Dispose ()` | Disposes the stream unless `leaveOpen` is `true`. |

#### Reading non-MAPI attribute values

In 4.x, the values of legacy attributes such as `attSubject` or `attDateSent` were read through
`reader.TnefPropertyReader.ReadValueAs* ()`. In 5.0 they are read directly from the reader:

| 4.x | 5.0 |
|---|---|
| `reader.TnefPropertyReader.ReadValueAsString ()` | `reader.ReadValueAsString ()` |
| `reader.TnefPropertyReader.ReadValueAsInt16 ()` | `reader.ReadValueAsInt16 ()` |
| `reader.TnefPropertyReader.ReadValueAsInt32 ()` | `reader.ReadValueAsInt32 ()` |
| `reader.TnefPropertyReader.ReadValueAsDateTime ()` | `reader.ReadValueAsDateTime ()` |
| `reader.TnefPropertyReader.ReadValueAsBytes ()` | `reader.ReadValueAsBytes ()` |
| `reader.TnefPropertyReader.ReadValue ()` | Choose the typed method from `reader.AttributeType`. |

Each has an `*Async` twin. Strings and byte arrays may only be read once.

#### Semantic changes

- **Nothing in the stream causes an exception.** 4.x threw `TnefException` in strict mode and
  `EndOfStreamException`/`TnefException` from several paths in loose mode. 5.0 reports a
  `TnefComplianceIssue` and recovers. A truncated stream simply stops returning attributes.
- **A wrong signature does not throw from `Read ()`.** It reports `InvalidSignature` and returns `false`.
  (`TnefMessage.Load ()` does throw `TnefException` in this case.)
- **Late message attributes.** A message-level attribute after the first attachment-level attribute is
  reported as `MessageAttributeAfterAttachment`.
- **The reader is only valid on its own attribute.** `TnefPropertyReader`s and value streams
  obtained for one attribute throw `InvalidOperationException` once `Read ()` has advanced past it.

### TnefPropertyReader

`TnefPropertyReader` is now `sealed`, and is obtained by calling `TnefReader.GetPropertyReader ()`.

| 4.x | 5.0 | Notes |
|---|---|---|
| `PropertyTag` | `Tag` | |
| — | `PropertyType` | The value type, `Tag.ValueTnefType`. |
| `IsMultiValuedProperty` | `IsMultiValued` | |
| `IsNamedProperty` | `Name.HasValue` (or `Tag.IsNamed`) | |
| `PropertyNameId` (`TnefNameId`) | `Name` (`TnefNameId?`) | `null` for properties that are not named. |
| `IsObjectProperty` | `PropertyType == TnefPropertyType.Object` | |
| `IsEmbeddedMessage` | `IsEmbeddedMessage` | Now detects the IID_IMessage prefix, so it no longer depends on `AttachMethod` having been read first. |
| `ValueCount`, `PropertyCount`, `RowCount` | unchanged | |
| `RawValueLength`, `RawValueStreamOffset` | — | Removed. Use `OpenValueStream ().Length` for variable-length values. |
| `ValueType` | — | Removed. Use `PropertyType`, or `ReadValue ()?.GetType ()`. |
| — | `IsValueConsumed` | Whether a variable-length value has already been read. |
| `bool ReadNextRow ()` | `ReadNextRow (CancellationToken = default)` / `ReadNextRowAsync` | |
| `bool ReadNextProperty ()` | `ReadNextProperty (CancellationToken = default)` / `ReadNextPropertyAsync` | Now positions on the first value; see below. |
| `bool ReadNextValue ()` | `ReadNextValue (CancellationToken = default)` / `ReadNextValueAsync` | Advances to the *second and later* values. |
| `Stream GetRawValueReadStream ()` | `Stream OpenValueStream ()` | |
| `int ReadRawValue (byte[], int, int)` | `OpenValueStream ().Read (...)` | |
| `int ReadTextValue (char[], int, int)` | `ReadValueAsString ()` | Or wrap `OpenValueStream ()` in a `StreamReader`. |
| `TnefReader GetEmbeddedMessageReader ()` | `TnefReader OpenEmbeddedMessage ()` | Shares the parent's options and logger; dispose it when done. |
| `ReadValue ()` | `ReadValue (CancellationToken = default)` / `ReadValueAsync` | `Currency` now returns `decimal`; see below. |
| `ReadValueAsString ()`, `ReadValueAsBytes ()` | unchanged, plus a `CancellationToken` and `*Async` twins | May only be called once per value. |
| `ReadValueAsBoolean/Int16/Int32/Int64/Float/Double/DateTime/Guid ()` | unchanged | May be called any number of times. |
| — | `ReadProperty ()`, `ReadPropertySet ()`, `ReadRowsAsPropertySets ()` (+ `*Async`) | Materialize the current property, the remaining properties, or every row. |

#### Semantic changes

- **Cursor position.** After `ReadNextProperty ()` returns `true`, the reader is already positioned on
  the property's first value (if `ValueCount > 0`). Read all values with:

  ```csharp
  do {
  	var value = prop.ReadValue ();
  } while (prop.ReadNextValue ());
  ```

  Code that called `ReadNextValue ()` *before* the first read (`while (prop.ReadNextValue ()) { ... }`)
  skips the first value in 5.0 and must be rewritten as a `do`/`while` loop.
- **Single-use variable-length values.** Strings, binary values and objects can be read once (via
  `ReadValue`, `ReadValueAsString`, `ReadValueAsBytes`, `OpenValueStream` or `OpenEmbeddedMessage`).
  A second read throws `InvalidOperationException`. Fixed-width values can be read repeatedly.
- **Stale readers throw.** Any use after the owning `TnefReader` advances to another attribute throws
  `InvalidOperationException`.
- **`PT_SYSTIME` is UTC.** 4.x used `DateTime.FromFileTime` (local time). 5.0 returns
  `DateTimeKind.Utc` values. Remove any `.ToUniversalTime ()` calls added to compensate.
- **`PT_CURRENCY` is scaled.** [MS-OXCDATA] defines currency as a 64-bit integer scaled by 10,000.
  `ReadValue ()` returns a `decimal` (4.x returned the raw `long`), and the numeric `ReadValueAs* ()`
  methods return the scaled value (4.x `ReadValueAsInt64 ()` returned the raw value).
- **Size limits.** Values larger than `TnefOptions.MaxPropertyValueLength`, or past
  `TnefOptions.MaxTotalDataBytes`, are reported as `DataSizeLimitExceeded` and read as empty.
- **No exceptions for malformed data.** Problems are reported to the reader's `ComplianceLogger` and the
  property reader stops returning properties when the rest of the attribute cannot be interpreted.

### Compliance: TnefComplianceStatus and TnefComplianceMode

4.x accumulated a `TnefComplianceStatus` bit field in loose mode and threw `TnefException` in strict mode.
5.0 reports each problem as a `TnefComplianceIssue` to an `ITnefComplianceLogger`. Each issue carries the
`Violation`, the `StreamOffset`, the embedded-message `Depth`, the `AttributeTag` and `PropertyTag` it was
found in, a `Severity`, a `Description` and `Remarks`.

| 4.x `TnefComplianceStatus` | 5.0 `TnefComplianceViolation` |
|---|---|
| `Compliant` | No issues logged. |
| `AttributeOverflow` | `InvalidAttributeLength` or `TruncatedStream` |
| `InvalidAttribute` | `UnknownAttribute`, `AttributeLevelMismatch` |
| `InvalidAttributeChecksum` | `AttributeChecksumMismatch` |
| `InvalidAttributeLength` | `InvalidAttributeLength` |
| `InvalidAttributeLevel` | `InvalidAttributeLevel`, `MessageAttributeAfterAttachment` |
| `InvalidAttributeValue` | `InvalidAttributeValue`, `InvalidValueCount`, `InvalidNamedPropertyKind` |
| `InvalidDate` | `InvalidDate` |
| `InvalidMessageClass` | `InvalidMessageClass` |
| `InvalidMessageCodepage` | `InvalidMessageCodepage` |
| `InvalidPropertyLength` | `InvalidPropertyLength`, `InvalidPropertyCount` |
| `InvalidRowCount` | `InvalidRowCount` |
| `InvalidTnefSignature` | `InvalidSignature` |
| `InvalidTnefVersion` | `UnsupportedVersion` |
| `NestingTooDeep` | `NestingTooDeep` |
| `StreamTruncated` | `TruncatedStream` |
| `UnsupportedPropertyType` | `UnsupportedPropertyType` |
| — | `TooManyAttachments`, `DataSizeLimitExceeded` (resource limits from `TnefOptions`) |
| — | `TooManyComplianceIssues` (the reader's per-violation cap was reached) |

| 4.x pattern | 5.0 replacement |
|---|---|
| `TnefComplianceMode.Loose` + check `reader.ComplianceStatus` afterwards | Set `reader.ComplianceLogger` to a logger that collects issues, and inspect it afterwards. |
| `TnefComplianceMode.Strict` (throw on first problem) | Set a logger whose `Log` throws, e.g. `throw new TnefException (issue.Violation, issue.Description)`. |
| `reader.ResetComplianceStatus ()` | Clear your logger's state. |
| `status.HasFlag (TnefComplianceStatus.X)` | `issues.Any (i => i.Violation == TnefComplianceViolation.X)` |

When reading untrusted streams, bound the number of issues with `reader.ComplianceOptions.MaxIssuesPerViolation`
or have your logger stop recording after a limit of its own. To get a logger into `TnefMessage`, create the
reader yourself and call `TnefMessage.Load (TnefReader)` (see [Collect compliance issues](#collect-compliance-issues)).

### TnefException

| 4.x | 5.0 |
|---|---|
| `TnefException (TnefComplianceStatus, string?)` | `TnefException (TnefComplianceViolation, string?)` |
| `TnefException (TnefComplianceStatus, string?, Exception?)` | `TnefException (TnefComplianceViolation, string?, Exception?)` |
| `Error` (`TnefComplianceStatus`) | `Violation` (`TnefComplianceViolation`) |

MimeKit itself only throws `TnefException` from `TnefMessage.Load[Async] ()` and
`TnefPart.LoadTnefMessage[Async] ()`, when the stream does not begin with the TNEF signature.

### Other public TNEF types

The following types are unchanged apart from additions: `TnefAttachFlags`, `TnefAttachMethod`,
`TnefAttributeLevel`, `TnefAttributeTag`, `TnefNameId`, `TnefNameIdKind`, `TnefPropertyId` (except
`InternetCPID`), `TnefPropertyTag` (except `InternetCPID` and `Puid`), `TnefPropertyType`,
`RtfCompressedToRtf` and `RtfCompressionMode`.

One behavioural change applies to `RtfCompressedToRtf`. In 4.x, a stream whose compression type was
neither `LZFu` nor `MELA` was passed through as if it were uncompressed. In 5.0, such a stream is
malformed ([MS-OXRTFCP] 2.1.3.1.1), so the filter produces no output. Check `CompressionMode`
to detect it. `ConvertToMime` drops an RTF body like this and reports a
`TnefConversionLossKind.InvalidRtfBody` loss. When a compressed RTF body has a CRC mismatch, it is
still converted, and an `RtfChecksumMismatch` loss is reported.

### TNEF recipes

All recipes assume `using MimeKit; using MimeKit.Tnef;`.

#### Convert a TNEF part to a MimeMessage

Before (4.x):

```csharp
var message = tnefPart.ConvertToMessage ();
```

After (5.0):

```csharp
MimeMessage message;

using (var tnef = tnefPart.LoadTnefMessage ()) {
	var result = tnef.ConvertToMime ();

	foreach (var loss in result.Losses)
		Console.WriteLine ("TNEF conversion loss: {0}", loss);

	// Keep the message; do not dispose the result.
	message = result.Message;
}
```

Async:

```csharp
using (var tnef = await tnefPart.LoadTnefMessageAsync (null, cancellationToken)) {
	var result = tnef.ConvertToMime (null, cancellationToken);
	// ...
}
```

`ConvertToMime ()` has no async twin because all of the content is already in memory.

#### Extract attachments

Before (4.x):

```csharp
foreach (var attachment in tnefPart.ExtractAttachments ()) {
	if (attachment is MimePart part) {
		using (var output = File.Create (part.FileName))
			part.Content.DecodeTo (output);
	}
}
```

After (5.0), without MIME conversion:

```csharp
using (var tnef = tnefPart.LoadTnefMessage ()) {
	foreach (var attachment in tnef.Attachments) {
		if (!attachment.HasContent || attachment.IsEmbeddedMessage)
			continue;

		// FileName comes from an untrusted source; sanitize it before using it as a path.
		var fileName = Path.GetFileName (attachment.FileName ?? "attachment.dat");

		using (var input = attachment.OpenRead ())
		using (var output = File.Create (fileName))
			input.CopyTo (output);
	}
}
```

Embedded messages are read with `attachment.LoadEmbeddedMessage ()`, which returns another `TnefMessage`.

#### Read message properties

Before (4.x), walking attributes:

```csharp
using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
	while (reader.ReadNextAttribute ()) {
		if (reader.AttributeLevel != TnefAttributeLevel.Message || reader.AttributeTag != TnefAttributeTag.MapiProperties)
			continue;

		var prop = reader.TnefPropertyReader;

		while (prop.ReadNextProperty ()) {
			if (prop.PropertyTag.Id == TnefPropertyId.Subject)
				subject = prop.ReadValueAsString ();
		}
	}
}
```

After (5.0), with the object model:

```csharp
using (var tnef = TnefMessage.Load (stream)) {
	var subject = tnef.Subject;
	var sentRepresenting = tnef.Properties.GetString (TnefPropertyTag.SentRepresentingEmailAddressW);
	var submitted = tnef.Properties.GetDateTime (TnefPropertyTag.ClientSubmitTime); // UTC

	if (tnef.Properties.TryGetValue (TnefNameId.Location, out var location))
		Console.WriteLine ("Location: {0}", location.Value);
}
```

`TnefPropertySet` lookups by `TnefPropertyTag` match the property id and accept either string type
(`String8`/`Unicode`).

After (5.0), with the reader:

```csharp
using (var reader = new TnefReader (stream)) {
	while (reader.Read ()) {
		if (reader.Level != TnefAttributeLevel.Message || reader.Tag != TnefAttributeTag.MapiProperties)
			continue;

		var prop = reader.GetPropertyReader ();

		while (prop.ReadNextProperty ()) {
			if (prop.Tag.Id == TnefPropertyId.Subject && prop.ValueCount > 0)
				subject = prop.ReadValueAsString ();
		}
	}
}
```

#### Read multi-valued properties

Before (4.x):

```csharp
while (prop.ReadNextProperty ()) {
	if (prop.IsMultiValuedProperty) {
		while (prop.ReadNextValue ())
			values.Add (prop.ReadValue ());
	}
}
```

After (5.0):

```csharp
while (prop.ReadNextProperty ()) {
	if (prop.IsMultiValued && prop.ValueCount > 0) {
		do {
			values.Add (prop.ReadValue ());
		} while (prop.ReadNextValue ());
	}
}
```

Or materialize it: `var property = prop.ReadProperty ();` and then `property.TryGetValues<string> (out var strings)`.

#### Read the recipient table

Before (4.x):

```csharp
var prop = reader.TnefPropertyReader;

while (prop.ReadNextRow ()) {
	while (prop.ReadNextProperty ()) {
		switch (prop.PropertyTag.Id) {
		case TnefPropertyId.DisplayName: name = prop.ReadValueAsString (); break;
		case TnefPropertyId.EmailAddress: addr = prop.ReadValueAsString (); break;
		}
	}
}
```

After (5.0), with the object model:

```csharp
foreach (var recipient in tnef.Recipients)
	Console.WriteLine ("{0}: {1} <{2}> ({3})", recipient.RecipientType, recipient.DisplayName, recipient.EmailAddress, recipient.AddressType);
```

After (5.0), with the reader (the loop shape is unchanged; only the member names differ):

```csharp
var prop = reader.GetPropertyReader ();

while (prop.ReadNextRow ()) {
	while (prop.ReadNextProperty ()) {
		switch (prop.Tag.Id) {
		case TnefPropertyId.DisplayName: name = prop.ReadValueAsString (); break;
		case TnefPropertyId.EmailAddress: addr = prop.ReadValueAsString (); break;
		}
	}
}
```

Or `IReadOnlyList<TnefPropertySet> rows = prop.ReadRowsAsPropertySets ();`.

#### Read the bodies

Before (4.x), the bodies were found by walking `MapiProperties` and decompressing `RtfCompressed` with
`RtfCompressedToRtf`. After (5.0):

```csharp
if (tnef.HtmlBody != null) {
	var html = tnef.HtmlBody.GetText ();
} else if (tnef.RtfBody != null) {
	using (var rtf = tnef.RtfBody.OpenDecodedRead ()) {
		// decompressed RTF bytes
	}
} else if (tnef.TextBody != null) {
	var text = tnef.TextBody.GetText ();
}
```

#### Read embedded messages

Before (4.x):

```csharp
if (prop.IsEmbeddedMessage) {
	using (var embedded = prop.GetEmbeddedMessageReader ()) {
		// ...
	}
}
```

After (5.0), with the reader (the depth limit is `TnefOptions.MaxNestingDepth`):

```csharp
if (prop.IsEmbeddedMessage) {
	using (var embedded = prop.OpenEmbeddedMessage ()) {
		while (embedded.Read ()) {
			// ...
		}
	}
}
```

After (5.0), with the object model:

```csharp
foreach (var attachment in tnef.Attachments) {
	if (attachment.IsEmbeddedMessage) {
		using (var embedded = attachment.LoadEmbeddedMessage ())
			Console.WriteLine ("Embedded: {0}", embedded.Subject);
	}
}
```

#### Collect compliance issues

Before (4.x):

```csharp
using (var reader = new TnefReader (stream, 0, TnefComplianceMode.Loose)) {
	while (reader.ReadNextAttribute ()) {
		// ...
	}

	if (reader.ComplianceStatus != TnefComplianceStatus.Compliant)
		Console.WriteLine ("TNEF problems: {0}", reader.ComplianceStatus);
}
```

After (5.0):

```csharp
class TnefIssueCollector : ITnefComplianceLogger
{
	public List<TnefComplianceIssue> Issues { get; } = new List<TnefComplianceIssue> ();

	public void Log (in TnefComplianceIssue issue)
	{
		Issues.Add (issue);
	}
}

var collector = new TnefIssueCollector ();

using (var reader = new TnefReader (stream, leaveOpen: true) { ComplianceLogger = collector }) {
	reader.ComplianceOptions.MaxIssuesPerViolation = 16;

	using (var tnef = TnefMessage.Load (reader)) {
		// ...
	}
}

foreach (var issue in collector.Issues)
	Console.WriteLine (issue); // violation, offset, attribute/property and description
```

#### Strict mode

Before (4.x):

```csharp
var reader = new TnefReader (stream, 0, TnefComplianceMode.Strict);
```

After (5.0):

```csharp
class StrictTnefLogger : ITnefComplianceLogger
{
	public void Log (in TnefComplianceIssue issue)
	{
		throw new TnefException (issue.Violation, issue.Description);
	}
}

var reader = new TnefReader (stream) { ComplianceLogger = new StrictTnefLogger () };
```

You can also throw only for some violations, for example only when
`issue.Severity >= MimeComplianceSeverity.Major`.

#### Limits for untrusted input

4.x had only `MaxNestingDepth`. 5.0 adds:

```csharp
var options = new TnefOptions {
	DefaultCodepage = 1252,
	MaxNestingDepth = 8,                         // default 32
	MaxPropertyValueLength = 16 * 1024 * 1024,   // default 32 MB
	MaxTotalDataBytes = 64L * 1024 * 1024,       // default 64 MB
	MaxAttachments = 256                         // default 1024
};

using (var tnef = tnefPart.LoadTnefMessage (options)) {
	// ...
}
```

#### Write a TNEF stream

4.x could only read TNEF. 5.0 adds `TnefWriter` (attributes) and `TnefPropertyWriter` (MAPI properties).
The writer emits the signature, `attTnefVersion` and `attOemCodepage` itself, computes every length,
count, checksum and padding, and throws `InvalidOperationException` when attributes are written out of
the order required by [MS-OXTNEF] (message attributes, then `attMsgProps`, then for each attachment
`attAttachRenderData` ... `attAttachment`).

```csharp
using (var output = File.Create ("winmail.dat"))
using (var writer = new TnefWriter (output, 1252)) {
	writer.WriteAttribute (TnefAttributeTag.MessageClass, "IPM.Note");

	using (var properties = writer.OpenPropertyWriter (TnefAttributeTag.MapiProperties)) {
		properties.WritePropertyTag (TnefPropertyTag.SubjectW);
		properties.WriteValue ("Quarterly report");

		properties.WritePropertyTag (TnefPropertyTag.RtfCompressed);
		using (var rtf = properties.OpenRtfCompressedStream ())
			rtf.Write (rtfBytes, 0, rtfBytes.Length);

		// Named properties are assigned ids (0x8000 and up) by the writer.
		properties.WritePropertyTag (new TnefNameId (TnefPropertySetGuid.PublicStrings, "Keywords"), TnefPropertyType.Unicode | TnefPropertyType.MultiValued);
		properties.WriteValue ("finance");
		properties.WriteValue ("q3");
	}

	writer.WriteAttribute (TnefAttributeTag.AttachRenderData, renderData);

	using (var properties = writer.OpenPropertyWriter (TnefAttributeTag.Attachment)) {
		properties.WritePropertyTag (TnefPropertyTag.AttachMethod);
		properties.WriteValue ((int) TnefAttachMethod.ByValue);
		properties.WritePropertyTag (TnefPropertyTag.AttachLongFilenameW);
		properties.WriteValue ("report.pdf");
		properties.WritePropertyTag (TnefPropertyTag.AttachDataBin);

		using (var content = properties.OpenValueStream ())
			pdfStream.CopyTo (content);
	}
}
```

Use `TnefPropertyWriter.OpenEmbeddedMessage ()` on an `Object` property (`PidTagAttachDataObject`) to write an
embedded message, and `TnefPropertyWriter.WriteProperty (TnefProperty)` to copy a property from a
`TnefMessage` or `TnefPropertySet` unchanged. `TnefWriter` has `WriteAttributeAsync` and `FlushAsync`;
property values are buffered in memory, so `TnefPropertyWriter` is synchronous.

### TNEF porting checklist

1. Replace `TnefPart.ConvertToMessage ()` with `LoadTnefMessage ()` + `ConvertToMime ()`; dispose the
   `TnefMessage`; decide who owns `result.Message`; consider logging `result.Losses`.
2. Replace `TnefPart.ExtractAttachments ()` with `result.Message.BodyParts`/`Attachments`, or with
   `TnefMessage.Attachments` when MIME conversion is not needed.
3. Replace `new TnefReader (stream, codepage, mode)` with `new TnefReader (stream, new TnefOptions { DefaultCodepage = codepage })`.
4. Rename: `ReadNextAttribute` → `Read`, `AttributeLevel` → `Level`, `AttributeTag` → `Tag`,
   `AttributeRawValueLength` → `Length`, `MessageCodepage` → `Codepage`, `AttachmentKey` → `LegacyKey`,
   `TnefPropertyReader` (property) → `GetPropertyReader ()`, `Close ()` → `Dispose ()`.
5. Replace `ReadAttributeRawValue`/`GetRawValueReadStream`/`ReadRawValue`/`ReadTextValue` with
   `OpenValueStream ()` or the `ReadValueAs* ()` methods.
6. Move legacy attribute reads from `reader.TnefPropertyReader.ReadValueAs* ()` to `reader.ReadValueAs* ()`.
7. Rename: `PropertyTag` → `Tag`, `PropertyNameId` → `Name` (now nullable), `IsNamedProperty` →
   `Name.HasValue`, `IsMultiValuedProperty` → `IsMultiValued`, `GetEmbeddedMessageReader` →
   `OpenEmbeddedMessage`.
8. Rewrite every `while (prop.ReadNextValue ())` value loop as `do { ... } while (prop.ReadNextValue ())`.
9. Make sure each string, binary or object value is read only once.
10. Do not use a `TnefPropertyReader` or value stream after calling `Read ()` again.
11. Remove compensation for local-time `PT_SYSTIME` values; they are now UTC.
12. Review `PT_CURRENCY` handling: values are now scaled by 1/10,000 (`decimal` from `ReadValue ()`).
13. Replace `ComplianceMode`/`ComplianceStatus`/`ResetComplianceStatus` with an `ITnefComplianceLogger`
    and map flags using the [violation table](#compliance-tnefcompliancestatus-and-tnefcompliancemode).
14. Replace `TnefException.Error` with `TnefException.Violation`.
15. Remove `try`/`catch` blocks that existed only to survive malformed TNEF (`EndOfStreamException`,
    `TnefException` from reading); keep the one around `TnefMessage.Load ()` for non-TNEF input.
16. Rename `InternetCPID` → `InternetCodepage`; replace `TnefPropertyTag.Puid` with `PuidA`/`PuidW`.
17. For untrusted input, set `TnefOptions` limits and `TnefComplianceOptions.MaxIssuesPerViolation`.
18. Re-run any tests that compared converted MIME output: header order, dates, inline dispositions
    and the absence of generated headers have changed as described in [TnefPart](#tnefpart).

## Porting checklist

1. Update references to the `MimeKitLite` package to `MimeKit.Core`, and any assembly-qualified type names,
   binding redirects or trimmer descriptors from `MimeKit` to `MimeKit.Core`.
2. Add `using MimeKit.Cryptography;` wherever `MimeMessage.Sign ()`, `Encrypt ()` or `SignAndEncrypt ()` is
   called.
3. Register an S/MIME context with `CryptographyContext.Register ()` during startup, and call
   `CryptographyModule.Initialize ()` if messages might be parsed before `CryptographyContext` is used.
4. Change visitors that override the `Visit*` methods for signed or encrypted entities to derive from
   `CryptographicMimeVisitor`.
5. Pass `DigestAlgorithm.Sha1` explicitly if you depend on SHA-1 signatures; otherwise expect SHA-256.
6. Move digest-preference code from `CryptographyContext`/`ICryptographyContext` to `OpenPgpContext`.
7. Remove the `DbConnection` parameter from `X509CertificateDatabase` overrides and use `CreateCommand ()`.
8. Remove serialization constructors and `GetObjectData ()` overrides from subclasses of MimeKit exceptions.
9. Replace your `IDkimPublicKeyLocator` with an `IDnsResolver` that returns `DnsTxtResponse` results,
   distinguishing NXDOMAIN from temporary failures.
10. Change `DkimVerifier.Verify ()` callers to check `result.Status`, and remove `try`/`catch` blocks that
    treated exceptions as failed signatures.
11. Pass the private key to the `DkimSigner`, `ArcSigner` and `DkimSignerBase` constructors.
12. Rename `ExperimentalMimeParser` to `MimeParser`; make sure `MimeParser` subclasses call the base
    implementation of any `On*` method they override and don't call `ReadHeaders ()`, `ReadEntity ()` or
    `ReadMessage ()`.
13. Update `MimeReader` subclasses: the new `OnMboxMarkerRead ()` signature, `OnMultipartBoundaryEnd ()` and
    `OnMultipartEndBoundaryEnd ()`, and no more trimming of the newline before a boundary.
14. Set `FormatOptions.EnsureNewLine` when appending messages to an mbox file.
15. Handle `ArgumentException` from the msg-id setters and `null` instead of `""` for empty msg-ids.
16. Don't rely on leading or trailing whitespace in address names, and remove `MimeEntity.WriteTo ()`/`WriteToAsync ()`
    overrides.
17. Use `MailboxAddress.GetAddress (false)`/`GetDomain (false)` where you relied on parsed IDN-encoded domains
    being decoded, and compare `GetAddress (true)` where the A-label and Unicode forms should be equal.
18. Replace `HtmlTokenizerState.AttributeValueQuoted`, and stop persisting `HtmlTokenizerState` values.
19. Review `HtmlTagCallback` implementations for `<noscript>` content, which is now passed to the callback,
    and for the tokenizer changes.
20. Replace `HtmlToHtml.FilterHtml`, `QEncoder`, `HexEncoder` and `TnefPropertyTag.Puid`.
21. Port TNEF code using the [TNEF porting checklist](#tnef-porting-checklist).
22. Update message/entity `Dispose (bool)` overrides to perform only local cleanup, tolerate retry after
    failure, and no longer assume children are usable before the base call. See
    [disposing messages and entities](#disposing-messages-and-entities).

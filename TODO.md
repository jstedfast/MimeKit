## TODO

* DKIM
  * It would be nice to make DKIM support not depend on BouncyCastle at all so
    that DKIM support could be added to MimeKitLite.
    See: https://github.com/jstedfast/MimeKit/pull/296#issuecomment-355656935
  * Replace the DkimSigner.DigestSigner property with a method called
    CreateSigningContext() or some such that returns an IDisposable replacement
    for ISigner.
      * The returned context should be IDisposable because System.Security-based
      	implementations need to be able to dispose the RSACryptoServiceProvider.
      * This would also help facilitate dropping the dependency on BouncyCastle
      	since we could create a new IDkimSigningContext interface to replace the
	use of ISigner.
      * Or maybe DkimSigner could *be* the signing context instead of having a
      	CreateSigningContext() method?
  * If we had a nice DNS library that supported async/await, we could drop the
    IDkimPublicKeyLocator interface or at least provide a default implementation.
    Is it time for me to write DnsKit??
* S/MIME
  * BouncyCastleSecureMimeContext
    * Add support for downloading CRLs via FTP? Is this needed or at all common?

### Pie-in-the-sky Ideas

* vCard support
* iCal support


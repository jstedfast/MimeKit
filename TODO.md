# TODO

* DKIM
  * It would be nice to make DKIM support not depend on BouncyCastle at all so
    that DKIM support could be added to MimeKitLite.
    See: https://github.com/jstedfast/MimeKit/pull/296#issuecomment-355656935
  * If we had a nice DNS library that supported async/await, we could drop the
    IDkimPublicKeyLocator interface or at least provide a default implementation.
    Is it time for me to write DnsKit??
* S/MIME
  * BouncyCastleSecureMimeContext
    * Add support for downloading CRLs via FTP? Is this needed or at all common?
  * Add a RECORDVERSION table column to the SQL database so we can add columns
    in the future and work around gaps in the row data?

## Pie-in-the-sky Ideas

* vCard support
* iCal support

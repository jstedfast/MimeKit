## Getting Started

### Parsing Messages

One of the more common operations that MimeKit is meant for is parsing email messages from arbitrary streams.
There are two ways of accomplishing this task.

The first way is to use one of the Load() methods on MimeKit.MimeMessage:

```csharp
// Load a MimeMessage from a stream
var message = MimeMessage.Load (stream);
```

The second way is to use the MimeParser class. For the most part, using the MimeParser directly is not necessary
unless you wish to parse a Unix mbox file stream. However, this is how you would do it:

```csharp
// Load a MimeMessage from a stream
var parser = new MimeParser (stream, MimeFormat.Entity);
var message = parser.ParseMessage ();
```

For Unix mbox file streams, you would use the parser like this:

```csharp
// Load every message from a Unix mbox
var parser = new MimeParser (stream, MimeFormat.Mbox);
while (!parser.IsEndOfStream) {
    var message = parser.ParseMessage ();

    // do something with the message
}
```

### Traversing a MimeMessage

Once you have parsed a MimeMessage, you'll most likely want to traverse the tree of MIME entities.

The MimeMessage.Body is the top-level MIME entity of the message. Generally, it will either be a
TextPart or a Multipart.

As an example, if you wanted to render the MimeMessage to some sort of UI control, you might
use code similar to this:

```csharp
void RenderMessage (MimeMessage message)
{
    RenderMimeEntity (message.Body);
}

void RenderMimeEntity (MimeEntity entity)
{
    if (entity is MessagePart) {
        // This entity is an attached message/rfc822 mime part.
        var messagePart = (MessagePart) entity;

        // If you'd like to render this inline instead of treating
        // it as an attachment, you would just continue to recurse:
        RenderMessage (messagePart.Message);
    } else if (entity is Multipart) {
        // This entity is a multipart container.
        var multipart = (Multipart) entity;

        foreach (var subpart in multipart)
            RenderMimeEntity (subpart);
    } else {
        // Everything that isn't either a MessagePart or a Multipart is a MimePart
        var part = (MimePart) entity;

        // Don't render anything that is explicitly marked as an attachment.
        if (part.IsAttachment)
            return;

        if (part is TextPart) {
            // This is a mime part with textual content.
            var text = (TextPart) part;
    
            if (text.ContentType.Matches ("text", "html"))
                RenderHtml (text.Text);
            else
                RenderText (text.Text);
        } else if (entity.ContentType.Matches ("image", "*")) {
            using (var content = new MemoryStream ()) {
                // If the content is base64 encoded (which it probably is), decode it.
                part.ContentObject.DecodeTo (memory);

                RenderImage (memory);
            }
        }
    }
}
```

### Getting the Decoded Content of a MIME Part

At some point, you're going to want to extract the decoded content of a MimePart (such as an image) and
save it to disk or feed it to a UI control to display it.

Once you've found the MimePart object that you'd like to extract the content of, here's how you can
save the decoded content to a file:

```csharp
// This will get the name of the file as specified by the sending mail client.
// Note: this value *may* be null, so you'll want to handle that case in your code.
var fileName = part.FileName;

using (var stream = File.Create (fileName)) {
    part.ContentObject.DecodeTo (stream);
}
```

You can also get access to the original encoded content and its encoding by poking at the Stream and
Encoding properties of the ContentObject. This might be useful if you want to pass the content off
to a UI control that can do its own loading from a stream.

```csharp
var filtered = new FilteredStream (part.ContentObject.Stream);

// Note: if the MimePart was parsed by a MimeParser (or loaded using MimeMessage.Load
// or MimeEntity.Load), the ContentObject.Encoding will match the part.Encoding.

// Create an IMimeFilter that can decode the ContentEncoding.
var decoder = DecoderFilter.Create (part.ContentObject.Encoding);

// Add the filter to our filtered stream.
filtered.Add (decoder);

// At this point, you can now read from the 'filtered' stream as if it were the
// original, raw content. Assuming you have an image UI control that could load
// from a stream, you could do something like this:
imageControl.Load (filtered);
```

There are a number of useful filters that can be applied to a FilteredStream, so if you find this type of
interface appealing, I suggest taking a look at the available filters in the MimeKit.IO.Filters namespace
or even write your own! The possibilities are limited only by your imagination.

### Creating a Simple Message

Creating MIME messages using MimeKit is really trivial.

```csharp
var message = new MimeMessage ();
message.From.Add (new MailboxAddress ("Joey", "joey@friends.com"));
message.To.Add (new MailboxAddress ("Alice", "alice@wonderland.com"));
message.Subject = "How you doin?";

message.Body = new TextPart ("plain") {
    Text = @"Hey Alice,

What are you up to this weekend? Monica is throwing one of her parties on
Saturday and I was hoping you could make it.

Will you be my +1?

-- Joey
"
};
```

A TextPart is a leaf-node MIME part with a text media-type. The first argument to the TextPart constructor
specifies the media-subtype, in this case, "plain". Another media subtype you are probably familiar with
is the "html" subtype. Some other examples include "enriched" and "csv".

The Text property is the easiest way to both get and set the string content of the MIME part.

### Creating a Message with Attachments

Attachments are just like any other MimePart, the only difference is that they typically have
a Content-Disposition header with a value of "attachment" instead of "inline" or no
Content-Disposition header at all.

Typically, when a mail client adds attachments to a message, it will create a multipart/mixed
part and add the text body part and all of the file attachments to the multipart/mixed.

Here's how you can do that with MimeKit:

```csharp
var message = new MimeMessage ();
message.From.Add (new MailboxAddress ("Joey", "joey@friends.com"));
message.To.Add (new MailboxAddress ("Alice", "alice@wonderland.com"));
message.Subject = "How you doin?";

// create our message text, just like before (except don't set it as the message.Body)
var body = new TextPart ("plain") {
    Text = @"Hey Alice,

What are you up to this weekend? Monica is throwing one of her parties on
Saturday and I was hoping you could make it.

Will you be my +1?

-- Joey
"
};

// create an image attachment for the file located at path
var attachment = new MimePart ("image", "gif") {
    ContentObject = new ContentObject (File.OpenRead (path), ContentEncoding.Default),
    ContentDisposition = new ContentDisposition (ContentDisposition.Attachment),
    FileName = Path.GetFileName (path)
};

// now create the multipart/mixed container to hold the message text and the
// image attachment
var multipart = new Multipart ("mixed");
multipart.Add (body);
multipart.Add (attachment);

// now set the multipart/mixed as the message body
message.Body = multipart;
```

Of course, that is just a simple example. A lot of modern mail clients such as Outlook or Thunderbird will 
send out both a text/html and a text/plain version of the message text. To do this, you'd create a TextPart
for the text/plain part and a TextPart for the text/html part and then add them to a multipart/alternative
like so:

```csharp
var attachment = CreateAttachment ();
var plain = CreateTextPlainPart ();
var html = CreateTextHtmlPart ();

// Note: it is important that the text/html part is added second, because it is the
// most expressive version and (probably) the most faithful to the sender's WYSIWYG 
// editor.
var alternative = new Multipart ("alternative");
alternative.Add (plain);
alternative.Add (html);

// now create the multipart/mixed container to hold the multipart/alternative
// and the image attachment
var multipart = new Multipart ("mixed");
multipart.Add (alternative);
multipart.Add (attachment);

// now set the multipart/mixed as the message body
message.Body = multipart;
```

### Digitally Signing Messages with S/MIME or PGP/MIME

Both S/MIME and PGP/MIME use a multipart/signed to contain the signed content and the detached signature data.

To create a multipart/signed MIME part, let's pretend we're going to digitally sign our multipart/mixed example
from the previous sample using S/MIME.

```csharp
var joey = new MailboxAddress ("Joey", "joey@friends.com");
var alice = new MailboxAddress ("Alice", "alice@wonderland.com");

var message = new MimeMessage ();
message.From.Add (joey);
message.To.Add (alice);
message.Subject = "How you doin?";

// create our message body (perhaps a multipart/mixed with the message text and some
// image attachments, for example)
var body = CreateMessageBody ();

// now to digitally sign our message body using the default S/MIME cryptography context
using (var ctx = new DefaultSecureMimeContext ()) {
    // Note: this assumes that "Joey" has an S/MIME signing certificate and private key
    // with an X.509 Subject Email identifier that matches Joey's email address.
    message.Body = MultipartSigned.Create (ctx, joey, DigestAlgorithm.Sha1, body);
}
```

For S/MIME, if you have a way for the user to configure which S/MIME certificate to use
as their signing certificate, you could also do something more like this:

```csharp
// now to digitally sign our message body using the default S/MIME cryptography context
using (var ctx = new DefaultSecureMimeContext ()) {
    var certificate = GetJoeysX509Certificate ();
    var signer = new CmsSigner (certificate);
    signer.DigestAlgorithm = DigestAlgorithm.Sha1;

    message.Body = MultipartSigned.Create (ctx, signer, body);
}
```

If you'd prefer to use PGP instead of S/MIME, things work almost exactly the same except that you
would use an OpenPGP cryptography context. For example, you might use a subclass of the
GnuPGContext that comes with MimeKit if you want to re-use the user's GnuPG keyrings (you can't
use GnuPGContext directly because it has no way of prompting the user for their passphrase).

For the sake of this example, let's pretend that you've written a minimal subclass of
MimeKit.Cryptography.GnuPGContext that simply overrides the GetPassword() method and
that this subclass is called MyGnuPGContext.

```csharp
// now to digitally sign our message body using our custom OpenPGP cryptography context
using (var ctx = new MyGnuPGContext ()) {
    // Note: this assumes that "Joey" has a PGP key that matches his email address.
    message.Body = MultipartSigned.Create (ctx, joey, DigestAlgorithm.Sha1, body);
}
```

Just like S/MIME, however, you can also do your own PGP key lookups instead of
relying on email addresses to match up with the user's private key.

```csharp
// now to digitally sign our message body using our custom OpenPGP cryptography context
using (var ctx = new MyGnuPGContext ()) {
    var key = GetJoeysPrivatePgpKey ();
    message.Body = MultipartSigned.Create (ctx, key, DigestAlgorithm.Sha1, body);
}
```

### Verifying S/MIME and PGP/MIME Digital Signatures

As mentioned earlier, both S/MIME and PGP/MIME typically use a multipart/signed part to contain the
signed content and the detached signature data.

A multipart/signed contains exactly 2 parts: the first MimeEntity is the signed content while the second
MimeEntity is the detached signature and, by default, will either be an ApplicationPgpSignature part or
an ApplicationPkcs7Signature part (depending on whether the sending client signed using OpenPGP or S/MIME).

Because the multipart/signed part may have been signed by multiple signers, it is important to
verify each of the digital signatures (one for each signer) that are returned by the
MultipartSigned.Verify() method:

```csharp
if (entity is MultipartSigned) {
    var signed = (MultipartSigned) entity;

    foreach (var signature in signed.Verify ()) {
        try {
            bool valid = signature.Verify ();

            // If valid is true, then it signifies that the signed content has not been
            // modified since this particular signer signed the content.
            //
            // However, if it is false, then it indicates that the signed content has
            // been modified.
        } catch (DigitalSignatureVerifyException) {
            // There was an error verifying the signature.
        }
    }
}
```

It should be noted, however, that while most S/MIME clients will use the preferred multipart/signed
approach, it is possible that you may encounter an "application/pkcs7-mime" part with an
"smime-type" parameter set to "signed-data". Luckily, MimeKit can handle this format as well:

```csharp
if (entity is ApplicationPkcs7Mime) {
    var pkcs7 = (ApplicationPkcs7Mime) entity;

    if (pkcs7.SecureMimeType == SecureMimeType.SignedData) {
        // extract the original content and get a list of signatures
        IList<IDigitalSignature> signatures;

        // Note: if you are rendering the message, you'll want to render
        // the extracted mime part rather than the application/pkcs7-mime part.
        MimeEntity extracted = pkcs7.Verify (out signatures);

        foreach (var signature in signatures) {
            try {
                bool valid = signature.Verify ();

                // If valid is true, then it signifies that the signed content has not
                // been modified since this particular signer signed the content.
                //
                // However, if it is false, then it indicates that the signed content
                // has been modified.
            } catch (DigitalSignatureVerifyException) {
                // There was an error verifying the signature.
            }
        }
    }
}
```

## API Documentation

For the complete API documentation, you can find up-todate documentation on the web
at [http://jstedfast.github.io/MimeKit/docs](http://jstedfast.github.io/MimeKit/docs).

A copy of the xml formatted API documentation is also included in the NuGet and/or
Xamarin Component package.

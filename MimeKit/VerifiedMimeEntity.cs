using MimeKit.Cryptography;

namespace MimeKit
{
    public class VerifiedMimeEntity
    {
        public DigitalSignatureCollection DigitalSignatureCollection { get; set; }
        public MimeEntity MimeEntity { get; set; }

        public VerifiedMimeEntity(DigitalSignatureCollection digitalSignatureCollection, MimeEntity mimeEntity)
        {
            DigitalSignatureCollection = digitalSignatureCollection;
            MimeEntity = mimeEntity;
        }
    }
}